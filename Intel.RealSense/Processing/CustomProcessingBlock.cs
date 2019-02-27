using Intel.RealSense.Frames;
using Intel.RealSense.Types;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Intel.RealSense.Processing
{
    public class CustomProcessingBlock : ProcessingBlock
    {
        public delegate void FrameCallback(Frame frame);
        public delegate void FrameProcessorCallback(Frame frame, FrameSource source);

        private GCHandle frameCallbackHandle;
        private readonly GCHandle frameProcessorCallbackHandle;
        private readonly FrameCallbackHandler frameCallback;
        private readonly FrameProcessorCallbackHandler frameProcessorCallback;

        public CustomProcessingBlock(Context context, FrameProcessorCallback cb) : base(context)
        {
            frameCallback = new FrameCallbackHandler(ProcessingBlockFrameCallback);
            frameProcessorCallback = new FrameProcessorCallbackHandler(ProcessingBlockCallback);
            frameProcessorCallbackHandle = GCHandle.Alloc(cb, GCHandleType.Normal);
            var cbPtr = GCHandle.ToIntPtr(frameProcessorCallbackHandle);
            var pb = NativeMethods.rs2_create_processing_block_fptr(frameProcessorCallback, cbPtr, out var error);
            Instance = new HandleRef(this, pb);
        }

        public Task ProcessFrame(Frame f) => Task.Run(() =>
        {
            NativeMethods.rs2_frame_add_ref(f.Instance.Handle, out var error);
            NativeMethods.rs2_process_frame(Instance.Handle, f.Instance.Handle, out error);
        });

        public async Task ProcessFrames(FrameSet fs, CancellationToken token)
        {
            using (var f = await fs.AsFrame(token))
               await ProcessFrame(f);
        }

        /// <summary>
        /// Start the processing block, delivering frames to external queue
        /// </summary>
        /// <param name="queue"></param>
        public void Start(FrameQueue queue)
            => NativeMethods.rs2_start_processing_queue(Instance.Handle, queue.Instance.Handle, out var error);
        /// <summary>
        /// Start the processing block
        /// </summary>
        public void Start()
            => NativeMethods.rs2_start_processing_queue(Instance.Handle, queue.Instance.Handle, out var error);
        /// <summary>
        /// Start the processing block, delivering frames to a callback
        /// </summary>
        /// <param name="cb"></param>
        public void Start(FrameCallback cb)
        {
            frameCallbackHandle = GCHandle.Alloc(cb, GCHandleType.Normal);
            var cbPtr = GCHandle.ToIntPtr(frameCallbackHandle);
            NativeMethods.rs2_start_processing_fptr(Instance.Handle, frameCallback, cbPtr, out var error);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (frameCallbackHandle.IsAllocated)
                    frameCallbackHandle.Free();
                if (frameProcessorCallbackHandle.IsAllocated)
                    frameProcessorCallbackHandle.Free();
            }

            base.Dispose(disposing);
        }

        private void ProcessingBlockCallback(IntPtr f, IntPtr src, IntPtr u)
        {
            var callback = GCHandle.FromIntPtr(u).Target as FrameProcessorCallback;
            var task = context.FramePool.CreateFrame(f, CancellationToken.None);
            task.Wait();
            using (var frame = task.Result)
                callback(frame, new FrameSource(new HandleRef(frame, src), context));
        }

        private void ProcessingBlockFrameCallback(IntPtr f, IntPtr u)
        {
            var callback = GCHandle.FromIntPtr(u).Target as FrameCallback;
            var task = context.FramePool.CreateFrame(f, CancellationToken.None);
            task.Wait();
            using (var frame = task.Result)
                callback(frame);
        }
    }
}