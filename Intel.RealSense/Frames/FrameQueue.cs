﻿using Intel.RealSense.Frames;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Intel.RealSense.Frames
{
    public class FrameQueue : IDisposable, IEnumerable<Frame>
    {
        internal HandleRef Instance;
        private readonly Context context;

        public FrameQueue(Context context, int capacity = 1)
        {
            Instance = new HandleRef(this, NativeMethods.rs2_create_frame_queue(capacity, out var error));
            this.context = context;
        }

        public bool PollForFrame(out Frame frame)
        {
            frame = null;

            if (NativeMethods.rs2_poll_for_frame(Instance.Handle, out IntPtr ptr, out var error) > 0)
            {
                var task = context.FramePool.CreateFrame(ptr, CancellationToken.None);
                task.Wait();
                frame = task.Result;
                return true;
            }
            
            return false;
        }

        public Task<Frame> WaitForFrame(CancellationToken token, uint timeoutMs = 5000)
        {
            var ptr = NativeMethods.rs2_wait_for_frame(Instance.Handle, timeoutMs, out var error);
            return context.FramePool.CreateFrame(ptr, token);
        }

        public async Task<FrameSet> WaitForFrames(CancellationToken token, uint timeoutMs = 5000)
        {
            var ptr = NativeMethods.rs2_wait_for_frame(Instance.Handle, timeoutMs, out var error);
            var frameSet = await context.FrameSetPool.Next(token);
            frameSet.Initialize(ptr);
            return frameSet;
        }

        public void Enqueue(Frame f)
        {
            NativeMethods.rs2_frame_add_ref(f.Instance.Handle, out var error);
            NativeMethods.rs2_enqueue_frame(f.Instance.Handle, Instance.Handle);
        }

        public IEnumerator<Frame> GetEnumerator()
        {
            while (PollForFrame(out Frame frame))
            {
                yield return frame;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() 
            => GetEnumerator();

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

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
                NativeMethods.rs2_delete_frame_queue(Instance.Handle);

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~FrameQueue()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        
        

      
        #endregion
    }
}
