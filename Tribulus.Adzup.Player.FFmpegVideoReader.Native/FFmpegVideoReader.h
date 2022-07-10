#pragma once

#ifdef FFMPEGVIDEOREADER_EXPORTS
#define VIDEOREADERLIBRARY_API __declspec(dllexport)
#else
#define VIDEOREADERLIBRARY_API __declspec(dllimport)
#endif


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


extern "C"
{
	VIDEOREADERLIBRARY_API typedef struct FrameDetails {
		int width;
		int height;
		int timebase_num; ///< Time Base Numerator
		int timebase_den; ///< Time Base Denominator
	} FrameDetails;
	VIDEOREADERLIBRARY_API typedef struct Frame {
		double pts_seconds;
		int64_t pts;
		int size;
		int linesize;
		uint8_t* frame_buffer;
	} Frame;
};



class IVideoReader {
public:
	int width, height;
	AVRational time_base;

	virtual void saveFile(uint8_t* fileBytes, int length, char* filename) = 0;
	virtual void getFrameDetails(FrameDetails* frameDetails) = 0;
	virtual int open(const char* filename) = 0;
	virtual int readFrame(Frame* frame) = 0;
	virtual void close() = 0;
	virtual ~IVideoReader() {}


private:

	// Private internal state
	AVFormatContext* av_format_ctx;
	AVCodecContext* av_codec_ctx;
	int video_stream_index;
	AVFrame* av_frame;
	AVFrame* av_frame_rgb;
	AVPacket* av_packet;
	SwsContext* sws_scaler_ctx;
};

extern "C" {
	VIDEOREADERLIBRARY_API void* video_reader_new();
	VIDEOREADERLIBRARY_API void video_reader_delete(void* obj);
	VIDEOREADERLIBRARY_API void video_reader_savefile(void* obj, uint8_t* file_bytes, int file_length, char* filename);
	VIDEOREADERLIBRARY_API int video_reader_open(void* obj, char* filename);
	VIDEOREADERLIBRARY_API void video_reader_getframedetails(void* obj, FrameDetails* frameDetails);
	VIDEOREADERLIBRARY_API int video_reader_readframe(void* obj, Frame* frame);
	VIDEOREADERLIBRARY_API void video_reader_close(void* obj);
}

#endif



