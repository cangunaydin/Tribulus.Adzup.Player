
using SkiaSharp;
using Tribulus.Adzup.Player.FFmpeg;
using Tribulus.Adzup.Player.Maui.Model;

namespace Tribulus.Adzup.Player.Maui.Controls;

public partial class PlayerCanvas : ContentView
{

    private byte[] _frameBuffer;
    private int _frameWidth;
    private int _frameHeight;
    private PlaylistFile playlistFilePlaying;

    public static readonly BindableProperty PlaylistFilesProperty = BindableProperty.Create(nameof(PlaylistFiles),
       typeof(List<PlaylistFile>),
       typeof(PlayerCanvas),
       null);
    public List<PlaylistFile> PlaylistFiles
    {
        get => (List<PlaylistFile>)GetValue(PlaylistFilesProperty);
        set => SetValue(PlaylistFilesProperty, value);
    }

    public static BindableProperty PlayProperty = BindableProperty.Create(nameof(Play), typeof(bool), typeof(PlayerCanvas), false, propertyChanged: OnPlayChanged);

    private static async void OnPlayChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (!(bindable is PlayerCanvas item))
        {
            return;
        }


        if (newValue is bool value)
        {
            if (value)
            {
                await item.StartPlaying();
            }
        }
    }

    public bool Play
    {
        get => (bool)GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }
    public PlayerCanvas()
    {
        InitializeComponent();
    }

    private void skiaView_PaintSurface(object sender, SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e)
    {
        if (_frameBuffer == null)
            return;
        // the the canvas and properties
        var canvas = e.Surface.Canvas;
        // make sure the canvas is blank
        canvas.Clear(SKColors.White);
        var canvasWidth = (int)skiaView.CanvasSize.Width;
        var canvasHeight = (int)skiaView.CanvasSize.Height;
        if (playlistFilePlaying.Type == PlaylistFileType.Video)
        {
            using (var data = SKData.CreateCopy(_frameBuffer))
            using (var original = new SKBitmap())
            {
                // create a new bitmap using the memory
                original.InstallPixels(new SKImageInfo(_frameWidth, _frameHeight, SKColorType.Rgba8888), data.Data);
                using (var resized = original.Resize(new SKImageInfo(canvasWidth, canvasHeight), SKFilterQuality.Medium))
                {
                    canvas.DrawBitmap(resized, 0, 0);
                }
            }
        }
        else if (playlistFilePlaying.Type == PlaylistFileType.Image)
        {
            using (var image = SKBitmap.Decode(_frameBuffer))
            {
                canvas.DrawBitmap(image, 0, 0);
            }

        }
        using (var paint = new SKPaint())
        {
            paint.TextSize = 32.0f;
            paint.IsAntialias = true;
            paint.Color = new SKColor(124, 252, 0);
            paint.IsStroke = false;

            canvas.DrawText(playlistFilePlaying.Name, 0, canvasHeight - 64.0f, paint);
        }

    }
    private async Task StartPlaying()
    {
        while (true)
        {
            foreach (var playlistFile in PlaylistFiles)
            {
                playlistFilePlaying = playlistFile;
                if (playlistFile.Type == PlaylistFileType.Image)
                    await PlayImageFile(playlistFile);
                else if (playlistFile.Type == PlaylistFileType.Video)
                    await PlayVideoFile(playlistFile);

            }
        }
    }
    private async Task PlayVideoFile(PlaylistFile playlistFile)
    {
        long startTime = 0;
        var videoReader = new VideoReader();
        videoReader.SetFilepath(playlistFile.Path);
        var success = videoReader.Open();
        videoReader.GetFrameDetails();
        videoReader.AllocateFrameMemory();

        _frameWidth = videoReader.FrameDetails.width;
        _frameHeight = videoReader.FrameDetails.height;


        await Task.Run(async () =>
        {
            long nextTime = 0;
            long previousTime = 0;
            while (true)
            {
                videoReader.ReadFrame();
                _frameBuffer = videoReader._frameBuffer;

                if (videoReader.Frame.pts == 0)
                    startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                var ptsInMs = (long)(videoReader.Frame.pts_seconds * 1000);
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                nextTime = startTime + ptsInMs;
                if (previousTime == nextTime || !Play)
                {
                    break;
                }

                previousTime = nextTime;


                skiaView.InvalidateSurface();
                if (nextTime > now)
                    await Task.Delay((int)(nextTime - now));


            }

        });

        videoReader.Close();
        videoReader.Dispose();
    }

    private async Task PlayImageFile(PlaylistFile playlistFile)
    {
        //long startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _frameBuffer = await File.ReadAllBytesAsync(playlistFile.Path);
        skiaView.InvalidateSurface();
        await Task.Delay(playlistFile.Duration);


    }


}