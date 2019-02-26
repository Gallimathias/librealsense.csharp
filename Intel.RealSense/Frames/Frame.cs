using Intel.RealSense.Pooling;
using Intel.RealSense.Profiles;
using Intel.RealSense.Types;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Intel.RealSense.Frames
{
    public class Frame : IDisposable, IAsyncPoolElement
    {
        public bool Initialized { get; private set; }

        public bool IsComposite => NativeMethods.rs2_is_frame_extendable_to(Instance.Handle, Extension.CompositeFrame, out var error) > 0;
        public IntPtr Data => NativeMethods.rs2_get_frame_data(Instance.Handle, out var error);
        public StreamProfile Profile => StreamProfile.Pool.Get(NativeMethods.rs2_get_frame_stream_profile(Instance.Handle, out var error));
        public ulong Number
        {
            get
            {
                var frameNumber = NativeMethods.rs2_get_frame_number(Instance.Handle, out var error);
                return frameNumber;
            }
        }

        internal static object CreateFrame(IntPtr ptr, object cancelationTokenSource) => throw new NotImplementedException();

        public double Timestamp => NativeMethods.rs2_get_frame_timestamp(Instance.Handle, out var error);
        public TimestampDomain TimestampDomain => NativeMethods.rs2_get_frame_timestamp_domain(Instance.Handle, out var error);

        public IntPtr NativePtr => Instance.Handle; //TODO: Native pointers should not be visible

        internal HandleRef Instance;
        protected Pool<Frame> pool;
        protected Context context;

        public Frame(Context context, IntPtr ptr)
        {
            Initialized = false;
            this.context = context;
            Initialize(ptr);
        }

        public Task<Frame> Clone(CancellationToken token)
        {
            NativeMethods.rs2_frame_add_ref(Instance.Handle, out var error);
            return context.FramePool.CreateFrame(Instance.Handle, token);
        }

        public virtual void Initialize(IntPtr ptr)
        {
            Instance = new HandleRef(this, ptr);
            Initialized = true;
        }
        

        #region IDisposable Support
        private bool disposedValue = false;

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
                    pool.OnFinalize(this, CancellationToken.None);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                disposedValue = true;
            }
        }

        public Task Pool(IAsyncPool pool, CancellationToken cancellationToken)
            => Task.Run(() => this.pool = pool as Pool<Frame>, cancellationToken);

        public Task Release(CancellationToken cancellationToken)
        {
            if (Instance.Handle != IntPtr.Zero)
                NativeMethods.rs2_release_frame(Instance.Handle);

            Instance = new HandleRef(this, IntPtr.Zero);
            Initialized = false;

            return pool.OnRelease(this, cancellationToken);
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~Frame()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }
        #endregion
    }
}
