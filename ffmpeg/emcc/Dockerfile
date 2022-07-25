FROM emscripten/emsdk:2.0.23 as build

ARG FFMPEG_VERSION=4.4.1

ARG PREFIX=/opt/ffmpeg
ARG MAKEFLAGS="-j4"

RUN apt-get update && apt-get install -y autoconf libtool build-essential

RUN git config --global http.sslverify false

# libx264
RUN cd /tmp && \
  git clone --depth 1 https://code.videolan.org/videolan/x264.git 

RUN cd /tmp/x264 && \
  emconfigure ./configure \
  --prefix=${PREFIX} \
  --host=i686-gnu \
  --enable-static \
  --disable-cli \
  --disable-asm \
  --extra-cflags="-s USE_PTHREADS=1"

RUN cd /tmp/x264 && \
  emmake make && emmake make install-lib-static -j 

# Get ffmpeg source.
RUN cd /tmp/ && \
  wget http://ffmpeg.org/releases/ffmpeg-${FFMPEG_VERSION}.tar.gz && \
  tar zxf ffmpeg-${FFMPEG_VERSION}.tar.gz && rm ffmpeg-${FFMPEG_VERSION}.tar.gz

ARG CFLAGS="-s USE_PTHREADS=1 -O3 -I${PREFIX}/include"
ARG LDFLAGS="$CFLAGS -L${PREFIX}/lib -s INITIAL_MEMORY=33554432"

# Compile ffmpeg.
RUN cd /tmp/ffmpeg-${FFMPEG_VERSION} && \
  emconfigure ./configure \
  --prefix=${PREFIX} \
  --target-os=none \
  --arch=x86_32 \
  --enable-cross-compile \
  --disable-debug \
  --disable-x86asm \
  --disable-inline-asm \
  --disable-stripping \
  --disable-programs \
  --disable-doc \
  --disable-runtime-cpudetect \ 
  --disable-autodetect \    
  --enable-gpl \
  --enable-libx264 \
  --extra-cflags="$CFLAGS" \
  --extra-cxxflags="$CFLAGS" \
  --extra-ldflags="$LDFLAGS" \
  --pkg-config-flags="--static" \
  --nm="llvm-nm" \
  --ar=emar \
  --as=llvm-as \
  --ranlib=emranlib \
  --cc=emcc \
  --cxx=em++ \
  --objcc=emcc \
  --dep-cc=emcc

RUN cd /tmp/ffmpeg-${FFMPEG_VERSION} && \
  emmake make -j4 && \
  emmake make install


COPY ./src/VideoReader.cpp /build/src/VideoReader.cpp
COPY ./Makefile /build/Makefile

WORKDIR /build

RUN make