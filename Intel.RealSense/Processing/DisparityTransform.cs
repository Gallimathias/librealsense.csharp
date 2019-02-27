using Intel.RealSense.Frames;
using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Intel.RealSense.Processing
{
    public class DisparityTransform : ProcessingBlock
    {

        public DisparityTransform(Context context, bool transformToDisparity = true) : base(context)
        {
            var transformDirection = transformToDisparity ? (byte)1 : (byte)0;
            Instance = new HandleRef(this, NativeMethods.rs2_create_disparity_transform_block(transformDirection, out var error));
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