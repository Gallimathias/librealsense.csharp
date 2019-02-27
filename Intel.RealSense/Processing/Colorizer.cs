using Intel.RealSense.Frames;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Intel.RealSense.Processing
{
    public class Colorizer : ProcessingBlock
    {
        public Colorizer(Context context) : base(context)
        {
            Instance = new HandleRef(this, NativeMethods.rs2_create_colorizer(out var error));
            NativeMethods.rs2_start_processing_queue(Instance.Handle, queue.Instance.Handle, out error);
        }

        [Obsolete("This method is obsolete. Use Process method instead")]
        public VideoFrame Colorize(VideoFrame original, FramesReleaser releaser = null)
        {
            var task = Process(original, CancellationToken.None);
            task.Wait();
            return task.Result.DisposeWith(releaser) as VideoFrame;
        }
    }
}