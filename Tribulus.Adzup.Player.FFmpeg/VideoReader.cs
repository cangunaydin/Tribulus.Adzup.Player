using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Tribulus.Adzup.Player.FFmpeg
{
    
    [StructLayout(LayoutKind.Sequential)]
    public struct FrameDetails
    {
        public int width;
        public int height;
        public int timebase_num;
        public int timebase_den;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct Frame
    {
        public double pts_seconds;
        public long pts;
        public int size;
        public int linesize;
        public IntPtr frame_buffer;
    };
    public class VideoReader : IDisposable
    {
       

        #region <private properties>
        private nint _handle;

        private IntPtr _fileBufferPtr = IntPtr.Zero;

        private FrameDetails _frameDetails;

        private Frame _frame;

        private string _filename;

        private string _filepath;

        public byte[] _frameBuffer;

        private bool _readyToReadFrame;
        #endregion

        #region <public properties>
        public string Filename
        {
            get => _filename;
            private set => _filename = value;
        }
        public string Filepath
        {
            get => _filepath;
            private set => _filepath = value;
        }

        //public byte[] FrameBuffer
        //{
        //    get => _frameBuffer;
        //    private set => _frameBuffer = value;
        //}
        public FrameDetails FrameDetails
        {
            get => _frameDetails;
            private set => _frameDetails = value;
        }

        public Frame Frame
        {
            get => _frame;
            private set => _frame = value;
        }
        public bool ReadyToReadFrame
        {
            get => _readyToReadFrame;
            private set => _readyToReadFrame = value;
        }

        #endregion

       


        
        public VideoReader()
        {
            
            InitializeValues();
        }
        public VideoReader(byte[] bytes, string filename)
        {
            InitializeValues();
            SaveFile(bytes, filename);
            
        }

       
        private void InitializeValues()
        {
            _handle = VideoReaderApi.video_reader_new();
            _frameDetails = new FrameDetails();
            _frame = new Frame() { frame_buffer=IntPtr.Zero};
            ReadyToReadFrame = false;
        }

        public void SaveFile(byte[] bytes, string filename)
        {
            _fileBufferPtr = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, _fileBufferPtr, bytes.Length);
            VideoReaderApi.video_reader_savefile(_handle, _fileBufferPtr, bytes.Length, filename);
            SetFilepath(filename);

        }
        public void SetFilepath(string filepath)
        {
            if (string.IsNullOrEmpty(filepath))
            {
                throw new Exception("filepath shouldn't be empty");
            }
            SetFilename(Path.GetFileName(filepath));
            Filepath = filepath;
        }
        private void SetFilename(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                throw new Exception("filename shouldn't be empty");
            }
            Filename = filename;
        }
        public int Open()
        {
            //if (_fileBufferPtr == IntPtr.Zero)
            //    throw new Exception("First you need to save video file, call SaveFile() first");
            if (string.IsNullOrEmpty(Filename) && _fileBufferPtr == IntPtr.Zero)
                throw new ArgumentNullException("filename or fileBuffer is null or empty, save the video first.");
            if (string.IsNullOrEmpty(Filename) && string.IsNullOrEmpty(Filepath))
                throw new ArgumentNullException("filename or filePath is null or empty, save the video first.");


            return VideoReaderApi.video_reader_open(_handle, Filepath);
        }
        public int DeleteFile(string filename)
        {
            return VideoReaderApi.video_reader_deletefile(_handle, filename);
        }
        
        public void GetFrameDetails()
        {
            VideoReaderApi.video_reader_getframedetails(_handle, ref _frameDetails);
        }
        public void AllocateFrameMemory()
        {
            if (FrameDetails.width == 0 || FrameDetails.height == 0)
                throw new Exception("frame details can not be null");

            nuint block = GetSizeOfOutputFrame();
            unsafe
            {
                _frame.frame_buffer = (IntPtr)NativeMemory.AlignedAlloc(block, 128);
            }

            //_frame.frame_buffer = Marshal.AllocHGlobal(sizeof(UInt64) * FrameDetails.width * FrameDetails.height * 3);
            _frame.linesize = FrameDetails.width * 4;
          
            ReadyToReadFrame = true;
        }

      
        public nuint GetSizeOfOutputFrame()
        {
            if (FrameDetails.width == 0 || FrameDetails.height == 0)
                throw new Exception("frame details can not be null");

            return (nuint)(FrameDetails.width * FrameDetails.height * 4);
        }

        public void ReadFrame()
        {
            //if (_frameBufferPtr == IntPtr.Zero)
            //    throw new Exception("buffer pointer can not be null AllocateFrameMemory() first.");
            //if (_ptsPtr == IntPtr.Zero)
            //    throw new Exception("pts pointer can not be null AllocateFrameMemory() first");

            var result = VideoReaderApi.video_reader_readframe(_handle,ref _frame);
            _frameBuffer = new byte[_frame.size];
            Marshal.Copy(_frame.frame_buffer,_frameBuffer,0, Frame.size);

            ////now ptr has changed, we need to convert the pointers to c# values
            ////Convert Pts value
            //Pts = Marshal.ReadInt64(_ptsPtr);

            ////Convert frame buffer ptr to byte array and copy it
            //var length = FrameDetails.width * FrameDetails.height * 4;
            //FrameBuffer = new byte[length];
            //Marshal.Copy(_frameBufferPtr, FrameBuffer, 0, length);

            ////Calculate Pts in seconds
            //PtsInSeconds = Pts * (double)FrameDetails.timebase_num / FrameDetails.timebase_den;


        }
        public void Close()
        {
            DisposeFrameBuffer();
            VideoReaderApi.video_reader_close(_handle);
            ReadyToReadFrame = false;
        }

        //[UnmanagedCallersOnly]
        //private static void ReadOutputBytes(IntPtr frameBytes, IntPtr pts, double ptInSeconds, int frameWidth, int frameHeight)
        //{
        //    long ptsValue = Marshal.ReadInt64(pts);
        //    var length = frameWidth * frameHeight * 4;
        //    byte[] outputArray = new byte[length];
        //    Marshal.Copy(frameBytes, outputArray, 0, length);
        //    NewFrameRendered?.Invoke(outputArray, ptInSeconds);

        //}
       
        public void DisposeFile()
        {
            if (_fileBufferPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_fileBufferPtr);
                _fileBufferPtr = IntPtr.Zero;
            }
            
        }
        public void DisposeFrameBuffer()
        {
            if (_frame.frame_buffer != IntPtr.Zero)
            {
                unsafe
                {
                    NativeMemory.AlignedFree(this._frame.frame_buffer.ToPointer());
                }
                _frame.frame_buffer = IntPtr.Zero;
            }
           
        }
        public void Dispose()
        {
            DisposeFile();
            DisposeFrameBuffer();
            VideoReaderApi.video_reader_delete(this._handle);
        }
    }
}
