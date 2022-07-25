mkdir -p dist
docker build -t video-reader .
docker create -ti --name video-reader-container video-reader
docker cp video-reader-container:/build/dist/ www
docker rm -fv video-reader-container