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

        internal async Task<Frame> Next(IntPtr ptr, CancellationToken token)
        {
            var frame = await Next(token);

            if (frame == null)
            {
                if (NativeMethods.rs2_is_frame_extendable_to(ptr, Extension.Points, out var error) > 0)
                    frame = new Points(context, ptr);
                else if (NativeMethods.rs2_is_frame_extendable_to(ptr, Extension.DepthFrame, out error) > 0)
                    frame = new DepthFrame(context, ptr);
                else if (NativeMethods.rs2_is_frame_extendable_to(ptr, Extension.VideoFrame, out error) > 0)
                    frame = new VideoFrame(context, ptr);
                else
                    frame = new Frame(context, ptr);

                await frame.Pool(this, token);
            }
            else
            {
                frame.Initialize(ptr);
            }

            return frame;
        }
    }
}
