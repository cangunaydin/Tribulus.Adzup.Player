
#ifndef video_reader_h
#define video_reader_h

extern "C" {
#include <libavcodec/avcodec.h>
#include <libavformat/avformat.h>
#include <libavutil/avutil.h>
#include <libavutil/bprint.h>
#include <libavutil/imgutils.h>
#include <libswscale/swscale.h>
}

typedef void (*ReaderCallBack)(uint8_t* frameBytes, int64_t* pts, double ptInSeconds,int frameWidth,int frameHeight);

extern "C"
{
	typedef struct FrameDetails {
		// Public things for other parts of the program to read from
		int width;
		int height;
		int timebase_num; ///< Time Base Numerator
		int timebase_den; ///< Time Base Denominator
	} FrameDetails;
}



class IVideoReader {
public:
	int width, height;
	AVRational time_base;

	virtual void setCallback(ReaderCallBack callback) = 0;
	virtual void saveFile(uint8_t* fileBytes, int length, char* filename) = 0;
	virtual void getFrameDetails(FrameDetails* frameDetails) = 0;
	virtual int open(const char* filename) = 0;
	virtual int readFrame(uint8_t* frame_buffer, int64_t* pts) = 0;
	virtual void close() = 0;
	virtual ~IVideoReader() {}


private:
	ReaderCallBack readerCallback = NULL;



	// Private internal state
	AVFormatContext* av_format_ctx;
	AVCodecContext* av_codec_ctx;
	int video_stream_index;
	AVFrame* av_frame;
	AVPacket* av_packet;
	SwsContext* sws_scaler_ctx;
};

#endif


#include <stdlib.h>
#include <string>
#include <iostream>
#include <fstream>
#include <vector>


#ifdef _MSC_VER
#define _CRT_SECURE_NO_WARNINGS
#endif



#ifdef _WIN32
static int check_align(size_t align)
{
	for (size_t i = sizeof(void*); i != 0; i *= 2)
		if (align == i)
			return 0;
	return EINVAL;
}

int posix_memalign(void** ptr, size_t align, size_t size)
{
	if (check_align(align))
		return EINVAL;

	int saved_errno = errno;
	void* p = _aligned_malloc(size, align);
	if (p == NULL)
	{
		errno = saved_errno;
		return ENOMEM;
	}

	*ptr = p;
	return 0;
}
#endif

// av_err2str returns a temporary array. This doesn't work in gcc.
// This function can be used as a replacement for av_err2str.
static const char* av_make_error(int errnum) {
	static char str[AV_ERROR_MAX_STRING_SIZE];
	memset(str, 0, sizeof(str));
	return av_make_error_string(str, AV_ERROR_MAX_STRING_SIZE, errnum);
}

static AVPixelFormat correct_for_deprecated_pixel_format(AVPixelFormat pix_fmt) {
	// Fix swscaler deprecated pixel format warning
	// (YUVJ has been deprecated, change pixel format to regular YUV)
	switch (pix_fmt) {
	case AV_PIX_FMT_YUVJ420P: return AV_PIX_FMT_YUV420P;
	case AV_PIX_FMT_YUVJ422P: return AV_PIX_FMT_YUV422P;
	case AV_PIX_FMT_YUVJ444P: return AV_PIX_FMT_YUV444P;
	case AV_PIX_FMT_YUVJ440P: return AV_PIX_FMT_YUV440P;
	default:                  return pix_fmt;
	}
}


class VideoReader : public IVideoReader {
public:
	// Public things for other parts of the program to read from
	int width, height;
	AVRational time_base;

	void setCallback(ReaderCallBack callback)
	{
		this->readerCallback = callback;
	}
	void saveFile(uint8_t* fileBytes, int length, char* filename)
	{
		std::ofstream fp;
		std::string fileNameStr(filename);
		fp.open(fileNameStr, std::ios::out | std::ios::binary);
		fp.write((char*)fileBytes, length);
	}
	void getFrameDetails(FrameDetails* frameDetails) {
		frameDetails->width = width;
		frameDetails->height = height;
		frameDetails->timebase_num = time_base.num;
		frameDetails->timebase_den = time_base.den;
	}
	int open(const char* filename)
	{
		// Open the file using libavformat
		av_format_ctx = avformat_alloc_context();
		if (!av_format_ctx) {
			printf("Couldn't created AVFormatContext\n");
			return -1;
		}

		if (avformat_open_input(&av_format_ctx, filename, NULL, NULL) != 0) {
			printf("Couldn't open video file\n");
			return -1;
		}

		// Find the first valid video stream inside the file
		video_stream_index = -1;
		AVCodecParameters* av_codec_params = nullptr;
		AVCodec* av_codec = nullptr;
		for (int i = 0; i < av_format_ctx->nb_streams; ++i) {
			av_codec_params = av_format_ctx->streams[i]->codecpar;
			av_codec = avcodec_find_decoder(av_codec_params->codec_id);
			if (!av_codec) {
				continue;
			}
			if (av_codec_params->codec_type == AVMEDIA_TYPE_VIDEO) {
				video_stream_index = i;
				width = av_codec_params->width;
				height = av_codec_params->height;
				time_base = av_format_ctx->streams[i]->time_base;
				break;
			}
		}
		if (video_stream_index == -1) {
			printf("Couldn't find valid video stream inside file\n");
			return -1;
		}
		// Set up a codec context for the decoder
		av_codec_ctx = avcodec_alloc_context3(av_codec);
		if (!av_codec_ctx) {
			printf("Couldn't create AVCodecContext\n");
			return -1;
		}
		if (avcodec_parameters_to_context(av_codec_ctx, av_codec_params) < 0) {
			printf("Couldn't initialize AVCodecContext\n");
			return -1;
		}
		if (avcodec_open2(av_codec_ctx, av_codec, NULL) < 0) {
			printf("Couldn't open codec\n");
			return -1;
		}

		av_frame = av_frame_alloc();
		if (!av_frame) {
			printf("Couldn't allocate AVFrame\n");
			return -1;
		}
		av_packet = av_packet_alloc();
		if (!av_packet) {
			printf("Couldn't allocate AVPacket\n");
			return -1;
		}

		return 1;
	}
	int readFrame(uint8_t* frame_buffer, int64_t* pts)
	{
		// Decode one frame
		int response;
		while (av_read_frame(av_format_ctx, av_packet) >= 0) {
			if (av_packet->stream_index != video_stream_index) {
				av_packet_unref(av_packet);
				continue;
			}

			response = avcodec_send_packet(av_codec_ctx, av_packet);
			if (response < 0) {
				printf("Failed to decode packet: %s\n", av_make_error(response));
				return -1;
			}

			response = avcodec_receive_frame(av_codec_ctx, av_frame);
			if (response == AVERROR(EAGAIN) || response == AVERROR_EOF) {
				av_packet_unref(av_packet);
				continue;
			}
			else if (response < 0) {
				printf("Failed to decode packet: %s\n", av_make_error(response));
				return -1;
			}

			av_packet_unref(av_packet);
			break;
		}

		*pts = av_frame->pts;

		// Set up sws scaler
		if (!sws_scaler_ctx) {
			auto source_pix_fmt = correct_for_deprecated_pixel_format(av_codec_ctx->pix_fmt);
			sws_scaler_ctx = sws_getContext(width, height, source_pix_fmt,
				width, height, AV_PIX_FMT_RGB0,
				SWS_BILINEAR, NULL, NULL, NULL);
		}
		if (!sws_scaler_ctx) {
			printf("Couldn't initialize sw scaler\n");
			return -1;
		}

		uint8_t* dest[4] = { frame_buffer, NULL, NULL, NULL };
		int dest_linesize[4] = { width * 4, 0, 0, 0 };
		sws_scale(sws_scaler_ctx, av_frame->data, av_frame->linesize, 0, av_frame->height, dest, dest_linesize);
		double pt_in_seconds = *pts * (double)time_base.num / time_base.den;
		if (readerCallback != NULL)
		{
			readerCallback(frame_buffer, pts, pt_in_seconds,width,height);
		}
		return 1;
	}
	void close() {
		sws_freeContext(sws_scaler_ctx);
		avformat_close_input(&av_format_ctx);
		avformat_free_context(av_format_ctx);
		av_frame_free(&av_frame);
		av_packet_free(&av_packet);
		avcodec_free_context(&av_codec_ctx);
	}

private:

	ReaderCallBack readerCallback = NULL;



	// Private internal state
	AVFormatContext* av_format_ctx;
	AVCodecContext* av_codec_ctx;
	int video_stream_index;
	AVFrame* av_frame;
	AVPacket* av_packet;
	SwsContext* sws_scaler_ctx;

};

extern "C" {
	void* video_reader_new() { return new VideoReader(); }
	void video_reader_delete(void* obj) { delete (VideoReader*)obj; }
	void video_reader_setcallback(
		void* obj, ReaderCallBack callback)
	{
		((VideoReader*)obj)->setCallback(callback);
	}
	void video_reader_savefile(void* obj, uint8_t* file_bytes, int file_length, char* filename)
	{
		((VideoReader*)obj)->saveFile(file_bytes, file_length, filename);
	}
	int video_reader_open(void* obj, char* filename)
	{
		return ((VideoReader*)obj)->open(filename);
	}
	void video_reader_getframedetails(void* obj, FrameDetails* frameDetails) {
		((VideoReader*)obj)->getFrameDetails(frameDetails);
	}
	int video_reader_readframe(void* obj, uint8_t* frame_buffer, int64_t* pts)
	{
		return ((VideoReader*)obj)->readFrame(frame_buffer, pts);
	}
	void video_reader_close(void* obj) {
		((VideoReader*)obj)->close();
	}
}

//int main() {
//	// Allocate frame buffer
//	VideoReader reader;
//	reader.open("small_bunny_1080p_60fps.mp4");
//	FrameDetails details;
//	reader.getFrameDetails(&details);
//
//	std::cout<<"width - height"<< details.width << " - " << details.height<<", timebase_num:" <<details.timebase_num <<" - timebase_den: "<< details.timebase_den << "\n";
//
//	constexpr int ALIGNMENT = 128;
//	const int frame_width = reader.width;
//	const int frame_height = reader.height;
//	uint8_t* frame_data;
//	//frame_data = (uint8_t*)_aligned_malloc(frame_width * frame_height * 4, ALIGNMENT);
//	//frame_data = (uint8_t*)allocated_frame;
//	if (posix_memalign((void**)&frame_data, ALIGNMENT, frame_width * frame_height * 4) != 0) {
//		printf("Couldn't allocate frame buffer\n");
//		return 0;
//	}
//	int exitKey;
//	int64_t pts;
//	while (true)
//	{
//		std::cout << "Enter 0 to break:";
//		std::string s;
//		std::getline(std::cin, s);
//		if (s == "0")
//			break;
//		if (!reader.readFrame(frame_data, &pts)) {
//			printf("Couldn't load video frame\n");
//			return 0;
//		}
//		static bool first_frame = true;
//		if (first_frame) {
//			first_frame = false;
//		}
//		double pt_in_seconds = pts * (double)reader.time_base.num / reader.time_base.den;
//		printf("pt_in_seconds: %f", pt_in_seconds);
//	}
//	reader.close();
//}