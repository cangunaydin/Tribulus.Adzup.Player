using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Tribulus.Adzup.Player.FFmpeg
{
    public static class VideoReaderApi
    {

        #region <DllImports (Native Calls)>
        [DllImport("FFmpegVideoReader")]
        public extern static nint video_reader_new();

        [DllImport("FFmpegVideoReader")]
        public extern static void video_reader_delete(nint obj);


        [DllImport("FFmpegVideoReader")]
        public extern static void video_reader_savefile(nint obj, IntPtr file_bytes, int file_length, string filename);

        [DllImport("FFmpegVideoReader")]
        public extern static int video_reader_deletefile(nint obj, string filename);

        [DllImport("FFmpegVideoReader")]
        public extern static int video_reader_open(nint obj, string filename);

        [DllImport("FFmpegVideoReader")]
        public extern static int video_reader_readframe(nint obj, ref Frame frame);

        [DllImport("FFmpegVideoReader")]
        public extern static void video_reader_getframedetails(nint obj, ref FrameDetails frameDetails);

        [DllImport("FFmpegVideoReader")]
        public extern static void video_reader_close(nint obj);

        #endregion
    }
}
