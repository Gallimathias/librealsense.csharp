using Intel.RealSense.Frames;
using Intel.RealSense.Types;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Intel.RealSense.Pooling
{
    public sealed class FramePool : Pool<Frame>
    {
        public FramePool(Context context) : base(context)
        {
        }

        internal async Task<Frame> CreateFrame(IntPtr ptr, CancellationToken token)
        {
            if (NativeMethods.rs2_is_frame_extendable_to(ptr, Extension.Points, out var error) > 0) ;
            else if (NativeMethods.rs2_is_frame_extendable_to(ptr, Extension.DepthFrame, out error) > 0) ;
            else if (NativeMethods.rs2_is_frame_extendable_to(ptr, Extension.VideoFrame, out error) > 0) ;

            var frame = await Next(token);

            if (frame == null)
            {
                frame = new Frame(context, ptr);
            }
            frame.Initialize(ptr);
            return frame;
        }
    }
}
