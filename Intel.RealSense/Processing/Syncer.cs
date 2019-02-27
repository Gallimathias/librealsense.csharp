using Intel.RealSense.Frames;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Intel.RealSense.Processing
{
    public class Syncer : ProcessingBlock
    {
        public Syncer(Context context) : base(context)
        {
            Instance = new HandleRef(this, NativeMethods.rs2_create_sync_processing_block(out var error));

            NativeMethods.rs2_start_processing_queue(Instance.Handle, queue.Instance.Handle, out error);
        }

        public void SubmitFrame(Frame frame)
        {
            NativeMethods.rs2_frame_add_ref(frame.Instance.Handle, out var error);
            NativeMethods.rs2_process_frame(Instance.Handle, frame.Instance.Handle, out error);
        }

        public Task<FrameSet> WaitForFrames(CancellationToken token,uint timeoutMs = 5000) 
            => context.FrameSetPool.Next(NativeMethods.rs2_wait_for_frame(queue.Instance.Handle, timeoutMs, out var error), token);

        public bool PollForFrames(out FrameSet result)
        {
            result = null;

            if (NativeMethods.rs2_poll_for_frame(queue.Instance.Handle, out IntPtr ptr, out var error) > 0)
            {
                var task = context.FrameSetPool.Next(ptr, CancellationToken.None);
                task.Wait();
                result = task.Result;
                return true;
            }

            return false;
        }
    }
}