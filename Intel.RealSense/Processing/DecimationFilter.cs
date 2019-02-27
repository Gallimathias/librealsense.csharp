using Intel.RealSense.Frames;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Intel.RealSense.Processing
{
    public class DecimationFilter : ProcessingBlock
    {

        public DecimationFilter(Context context) : base(context)
        {
            Instance = new HandleRef(this, NativeMethods.rs2_create_decimation_filter_block(out var error));
            NativeMethods.rs2_start_processing_queue(Instance.Handle, queue.Instance.Handle, out error);
        }

        [Obsolete("This method is obsolete. Use Process method instead")]
        public VideoFrame ApplyFilter(VideoFrame original, FramesReleaser releaser = null)
        {
            var task = Process(original, CancellationToken.None);
            task.Wait();
            return task.Result.DisposeWith(releaser) as VideoFrame;
        }

    }
}