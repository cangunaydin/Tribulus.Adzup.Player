dist/VideoReader.o:
	mkdir -p dist && \
	emcc --bind \
	-O3 \
	-L/opt/ffmpeg/lib \
	-I/opt/ffmpeg/include/ \
	-s INITIAL_MEMORY=2146435072 \
	-lavcodec -lavformat -lavfilter -lavdevice -lswresample -lswscale -lavutil -lm -lx264 \
	-pthread \
	-shared \
	-o dist/VideoReader.o \
	src/VideoReader.cpp