using Intel.RealSense.Frames;
using Intel.RealSense.Sensors;
using Intel.RealSense.Types;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Intel.RealSense.Processing
{
    public abstract class ProcessingBlock : IProcessingBlock, IDisposable
    {
        public IOptionsContainer Options => options = options ?? new Sensor.SensorOptions(Instance.Handle);
        
        internal HandleRef Instance;

        protected readonly FrameQueue queue;
        protected readonly Context context;
        private Sensor.SensorOptions options;

        public ProcessingBlock(Context context)
        {
            queue = new FrameQueue(context, 1);
            this.context = context;
        }

        /// <summary>
        /// Process frame and return the result
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public Task<Frame> Process(Frame original, CancellationToken token) => Task.Run(() =>
        {
            NativeMethods.rs2_frame_add_ref(original.Instance.Handle, out var error);
            NativeMethods.rs2_process_frame(Instance.Handle, original.Instance.Handle, out error);

            if (queue.PollForFrame(out Frame f))
                return f;

            return original;
        }, token);
        public async Task<FrameSet> Process(FrameSet original, CancellationToken token)
        {
            FrameSet rv;

            using (var singleOriginal = await original.AsFrame(token))
            using (var processed = await Process(singleOriginal, token))
                rv = FrameSet.FromFrame(context, processed);

            return rv;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                Release();
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~ProcessingBlock()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }
        #endregion

        public void Release()
        {
            if (Instance.Handle != IntPtr.Zero)
                NativeMethods.rs2_delete_processing_block(Instance.Handle);

            Instance = new HandleRef(this, IntPtr.Zero);
        }
    }
}