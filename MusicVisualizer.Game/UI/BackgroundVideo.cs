using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MusicVisualizer.Game.IO;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Video;
using osu.Framework.Logging;
using osuTK;
using osuTK.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.ColorSpaces.Conversion;
using SixLabors.ImageSharp.PixelFormats;
using osuTK.Graphics.ES30;
using System.Runtime.InteropServices;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Graphics.OpenGL.Buffers;
using osu.Framework.Platform;
using SixLabors.ImageSharp.Processing;
using System.IO;
using FFMpegCore.Pipes;
using FFMpegCore;
using FFMpegCore.Arguments;
using osu.Framework.Timing;

namespace MusicVisualizer.Game.UI
{
    public class BackgroundVideo : BufferedContainer
    {
        [Resolved]
        private VisualizerContainer vis { get; set; }

        public Bindable<Track> Track = new Bindable<Track>();

        public Bindable<SongConfig> Config = new Bindable<SongConfig>();

        public Bindable<ColorInfo> Colors = new Bindable<ColorInfo>();

        private Video video;

        private FileStore store;

        private string videoFile;

        private GameHost host;

        private StopwatchClock clock;

        public BackgroundVideo()
        {
            RelativeSizeAxes = Axes.Both;
            BlurSigma = new Vector2(2);
        }

        private IEnumerable<Hsv> getSimilarColors(Hsv color, IEnumerable<Hsv> colors)
        {
            var result = new List<Hsv>();

            foreach (var compare in colors)
            {
                if (compare == color) continue;

                if (Math.Abs(color.H - compare.H) < 12 && Math.Abs(color.S - compare.S) < 0.1 && Math.Abs(color.V - compare.V) < 0.1)
                {
                    result.Add(compare);
                }
            }

            return result;
        }

        private Hsv avgHsv(IEnumerable<Hsv> colors)
        {
            var h = colors.Select(c => c.H).Sum() / colors.Count();
            var s = colors.Select(c => c.S).Sum() / colors.Count();
            var v = colors.Select(c => c.V).Sum() / colors.Count();

            return new Hsv(h, s, v);
        }

        private void getImageGL(Action<Image<Rgba32>> callback)
        {
            host.DrawThread.Scheduler.Add(() =>
            {
                var image = new Image<Rgba32>((int)DrawWidth, (int)DrawHeight);

                var sharedData = (BufferedDrawNodeSharedData)typeof(BufferedContainer<Drawable>).GetField("sharedData", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);

                var fbo = new FrameBuffer();
                fbo.Bind();
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget2d.Texture2D, sharedData.MainBuffer.Texture.TextureId, 0);
                GL.ReadPixels(0, 0, (int)DrawWidth, (int)DrawHeight, PixelFormat.Rgba, PixelType.UnsignedByte, ref MemoryMarshal.GetReference(image.GetPixelSpan()));
                fbo.Dispose();
                GLWrapper.BindFrameBuffer(GLWrapper.DefaultFrameBuffer);

                callback(image);
            });
        }

        public void GetColors(Action<Color4[]> callback)
        {
            var sent = Clock.CurrentTime;
            getImageGL(image =>
            {
                Logger.Log($"OpenGL frame query took {Clock.CurrentTime - sent}ms", LoggingTarget.Performance);
                Task.Run(() =>
                {
                    image.Mutate(c => c.Resize(new ResizeOptions
                    {
                        Size = new SixLabors.Primitives.Size(64)
                    }));

                    var distinct = image.GetPixelSpan().ToArray().Distinct();
                    Span<Hsv> hsv = new Span<Hsv>(new Hsv[distinct.Count()]);
                    var colors = new ReadOnlySpan<Rgb>(distinct.Select(c => new Rgb(c.R / 255f, c.G / 255f, c.B / 255f)).ToArray());
                    new ColorSpaceConverter().Convert(colors, hsv);

                    var searched = new List<Hsv>();
                    var result = new List<Hsv>();

                    foreach (var color in hsv)
                    {
                        if (searched.Contains(color)) continue;

                        var sim = getSimilarColors(color, hsv.ToArray());

                        if (sim.Count() > 0)
                            result.Add(avgHsv(sim));
                        else
                            result.Add(color);

                        searched.AddRange(sim);
                    }

                    callback(result.Select(c => Color4.FromHsv(new Vector4(c.H / 360, c.S, c.V, 1))).ToArray());
                });
            });
        }

        // Old solution using FFmpeg
        public void GetColors(Action<Color4[]> callback, int time)
        {
            Task.Run(() =>
            {
                if (videoFile == null) return;

                Stream stream;

                var sink = new StreamPipeSink(stream = new MemoryStream());

                var path = store.Storage.GetFullPath(videoFile);

                FFMpegArguments
                    .FromSeekedFiles((path, new TimeSpan(0, 0, 0, 0, time)))
                    .WithFrameOutputCount(1)
                    .WithArgument(new CustomArgument("-s 16x16"))
                    .ForceFormat("image2")
                    .OutputToPipe(sink)
                    .ProcessSynchronously();

                stream.Seek(0, SeekOrigin.Begin);

                using (Image<Rgb24> image = SixLabors.ImageSharp.Image.Load<Rgb24>(stream))
                {
                    var distinct = image.GetPixelSpan().ToArray().Distinct();
                    Span<Hsv> hsv = new Span<Hsv>(new Hsv[distinct.Count()]);
                    var colors = new ReadOnlySpan<Rgb>(distinct.Select(c => new Rgb(c.R / 255f, c.G / 255f, c.B / 255f)).ToArray());
                    new ColorSpaceConverter().Convert(colors, hsv);

                    var searched = new List<Hsv>();
                    var result = new List<Hsv>();

                    foreach (var color in hsv)
                    {
                        if (searched.Contains(color)) continue;

                        var sim = getSimilarColors(color, hsv.ToArray());

                        if (sim.Count() > 0)
                            result.Add(avgHsv(sim));
                        else
                            result.Add(color);

                        searched.AddRange(sim);
                    }

                    callback(result.Select(c => Color4.FromHsv(new Vector4(c.H / 360, c.S, c.V, 1))).ToArray());
                }
            });
        }

        private double lastColorChange = 0;

        public void UpdateColors(bool ffMpeg = false)
        {
            var sent = Clock.CurrentTime;
            if (ffMpeg)
            {
                GetColors(colors =>
                {
                    Logger.Log($"Video color query took {Clock.CurrentTime - sent}ms", LoggingTarget.Performance);
                    Colors.Value = new ColorInfo
                    {
                        Colors = colors,
                        Initial = true
                    };
                }, (int)video.PlaybackPosition);
            }
            else
            {
                GetColors(colors =>
                {
                    Logger.Log($"Video color query took {Clock.CurrentTime - sent}ms", LoggingTarget.Performance);
                    Colors.Value = new ColorInfo
                    {
                        Colors = colors
                    };
                });
            }
        }

        protected override void Update()
        {
            base.Update();

            var track = Track.Value;

            if (track == null) return;

            if (video != null && Math.Abs(video.Time.Current - Track.Value.CurrentTime) > 500)
            {
                video.Seek(Track.Value.CurrentTime);
            }

            if (vis.ActivityDelta > 0.5 && Clock.CurrentTime - lastColorChange >= 1000)
            {
                UpdateColors();

                lastColorChange = Clock.CurrentTime;
            }

            if (track.IsRunning && !clock.IsRunning)
            {
                clock.Start();
            }
            else if (!track.IsRunning && clock.IsRunning)
            {
                clock.Stop();
            }
        }

        [BackgroundDependencyLoader]
        private void load(FileStore store, GameHost host)
        {
            this.host = host;
            this.store = store;

            Config.ValueChanged += ev =>
            {
                videoFile = ev.NewValue.Video;

                if (videoFile != null)
                {
                    var stream = store.Store.GetStream(videoFile);
                    InternalChild = video = new Video(stream)
                    {
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fill,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Clock = new FramedClock(clock = new StopwatchClock())
                    };

                    UpdateColors(true);
                }
                else
                {
                    InternalChild = new Box
                    {
                        Colour = Color4.Black,
                        RelativeSizeAxes = Axes.Both
                    };
                }
            };
        }

        public class ColorInfo
        {
            public Color4[] Colors { get; set; }

            public bool Initial { get; set; } = false;
        }
    }
}
