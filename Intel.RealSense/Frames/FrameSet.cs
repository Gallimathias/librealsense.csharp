using Intel.RealSense.Frames;
using Intel.RealSense.Pooling;
using Intel.RealSense.Processing;
using Intel.RealSense.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Intel.RealSense.Frames
{
    public class FrameSet : ICompositeDisposable, IEnumerable<Frame>, IAsyncPoolElement
    {
        public DepthFrame DepthFrame => FirstOrDefault<DepthFrame>(Stream.Depth, Format.Z16);
        public VideoFrame InfraredFrame => FirstOrDefault<VideoFrame>(Stream.Infrared);
        public VideoFrame ColorFrame => FirstOrDefault<VideoFrame>(Stream.Color);
        
        internal HandleRef Instance;
        internal readonly FrameEnumerator enumerator;
        internal readonly List<IDisposable> disposables;

        private Pool<FrameSet> pool;
        private bool initialized;
        private readonly Context context;
        
        public Task<Frame> AsFrame(CancellationToken token)
        {
            NativeMethods.rs2_frame_add_ref(Instance.Handle, out var error);
            return context.FramePool.CreateFrame(Instance.Handle, token);
        }

        public T FirstOrDefault<T>(Stream stream, Format format = Format.Any) where T : Frame
        {
            for (int i = 0; i < Count; i++)
            {
                var frame = this[i];

                using (var fp = frame.Profile)
                    if (fp.Stream == stream && (format == Format.Any || fp.Format == format))
                        return frame as T;

                frame.Dispose();
            }

            return null;
        }
        public T FirstOrDefault<T>(Predicate<Frame> predicate) where T : Frame
        {
            for (int i = 0; i < Count; i++)
            {
                var frame = this[i];

                if (predicate(frame))
                    return frame as T;

                frame.Dispose();
            }
            return null;
        }

        public IEnumerator<Frame> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                var ptr = NativeMethods.rs2_extract_frame(Instance.Handle, i, out var error);
                var task = context.FramePool.CreateFrame(ptr, CancellationToken.None);
                task.Wait();
                yield return task.Result;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public int Count { get; internal set; }

        public Frame this[int index]
        {
            get
            {
                var ptr = NativeMethods.rs2_extract_frame(Instance.Handle, index, out var error);
                var task = context.FramePool.CreateFrame(ptr, CancellationToken.None);
                task.Wait();
                return task.Result;
            }
        }
        public Frame this[Stream stream, int index = 0] => FirstOrDefault<Frame>(f =>
        {
            using (var p = f.Profile)
                return p.Stream == stream && p.Index == index;
        });
        public Frame this[Stream stream, Format format, int index = 0] => FirstOrDefault<Frame>(f =>
        {
            using (var p = f.Profile)
                return p.Stream == stream && p.Format == format && p.Index == index;
        });

        internal FrameSet(Context context)
        {
            enumerator = new FrameEnumerator(context, this);
            disposables = new List<IDisposable>();
            initialized = false;
            this.context = context;
        }
        internal FrameSet(Context context, IntPtr ptr) : this(context)
        {            
            Initialize(ptr);
        }

        public Task<FrameSet> ApplyFilter(IProcessingBlock block, CancellationToken token)
           => block.Process(this, token);

        internal void Initialize(IntPtr ptr)
        {
            if (initialized)
                return;

            Instance = new HandleRef(this, ptr);
            Count = NativeMethods.rs2_embedded_frames_count(Instance.Handle, out var error);
            enumerator.Reset();
            disposables.Clear();
            initialized = true;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue)
                throw new ObjectDisposedException(nameof(FrameSet));

            if (disposing)
            {
                pool.OnFinalize(this, CancellationToken.None);
                // TODO: dispose managed state (managed objects).
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            disposables.ForEach(d => d?.Dispose());
            disposedValue = true;
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~FrameSet()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion

        public void ForEach(Action<Frame> action)
        {
            for (int i = 0; i < Count; i++)
            {
                using (var frame = this[i])
                    action(frame);
            }
        }

        public void AddDisposable(IDisposable disposable)
        {
            disposables.Add(disposable);
        }

        public static FrameSet FromFrame(Frame composite, Pool<FrameSet> pool)
        {
            if (!composite.IsComposite)
                throw new ArgumentException("The frame is a not composite frame", nameof(composite));

            NativeMethods.rs2_frame_add_ref(composite.Instance.Handle, out object error);
            var task = pool.Next(CancellationToken.None);
            task.Wait();
            var frameSet = task.Result;
            frameSet.Initialize(composite.Instance.Handle);
            return frameSet;
        }
        [Obsolete("This method is obsolete. Use DisposeWith method instead")]
        public static FrameSet FromFrame(Frame composite, FramesReleaser releaser)
            => FromFrame(composite, null as Pool<FrameSet>).DisposeWith(releaser);


        public Task Pool(IAsyncPool pool, CancellationToken cancellationToken) 
            => Task.Run(() => this.pool = pool as Pool<FrameSet>, cancellationToken);

        public Task Release(CancellationToken cancellationToken)
        {
            if (Instance.Handle != IntPtr.Zero)
                NativeMethods.rs2_release_frame(Instance.Handle);

            Instance = new HandleRef(this, IntPtr.Zero);
            initialized = false;

            return pool.OnRelease(this, cancellationToken);
        }

        public static explicit operator IntPtr(FrameSet frameSet)
            => frameSet.Instance.Handle;
    }
}
