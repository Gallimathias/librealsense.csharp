using Intel.RealSense;
using Intel.RealSense.Pipelines;
using Intel.RealSense.Processing;
using Intel.RealSense.Types;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using Stream = Intel.RealSense.Types.Stream;

namespace DemoProject
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var context = new Context())
            {
                if (context.Devices.Count < 1)
                    return;

                var device = context.Devices[0];
                var pipeline = new Pipeline(context);

                var configuration = new Config(context);

                configuration.DisableAllStreams();
                configuration.EnableStream(Stream.Color, 1280, 720, Format.Bgr8);
                configuration.EnableStream(Stream.Depth, 1280, 720, Format.Z16);

                var infaredSensor = device.Sensors.First(s => s.StreamProfiles.Any(p => p.Stream == Stream.Infrared || p.Stream == Stream.Depth));
                infaredSensor.Options[Option.EnableAutoExposure].Value = 1;
                infaredSensor.Options[Option.LaserPower].Value = 150;
                infaredSensor.Options[Option.DepthUnits].Value = 0.001f;

                var profile = pipeline.Start(configuration);

                if (pipeline.PollForFrames(out var set))
                {
                    var align = new Align(context, Stream.Color);
                    var coloredFrame = set.ColorFrame;
                    var task = align.Process(set, CancellationToken.None);
                    task.Wait();
                    var depthFrame = task.Result.DepthFrame;

                    var data = new ushort[depthFrame.Stride * depthFrame.Height];
                    depthFrame.CopyTo(data);

                    var colordata = new byte[coloredFrame.Stride * coloredFrame.Height];
                    coloredFrame.CopyTo(colordata);

                    unsafe
                    {
                        fixed (byte* ptr = colordata)
                        {
                            var internalBitmap = new Bitmap(
                                 coloredFrame.Width,
                                 coloredFrame.Height,
                                 coloredFrame.Stride,
                                 PixelFormat.Format24bppRgb,
                                 new IntPtr(ptr));

                            internalBitmap.Save(Path.Combine(".", "test.bmp"));
                        }
                    }
                }
            }


        }
    }
}
