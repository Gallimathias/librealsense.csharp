using Intel.RealSense.Frames;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Intel.RealSense.Frames
{
    public class FrameEnumerator : IEnumerator<Frame>
    {
        public Frame Current { get; private set; }

        private readonly Context context;

        object IEnumerator.Current
        {
            get
            {
                if (index == 0 || index == frameSet.Count + 1)
                    throw new InvalidOperationException();

                return Current;
            }
        }        

        private readonly FrameSet frameSet;
        private int index;

        public FrameEnumerator(Context context, FrameSet frameSet)
        {
            this.frameSet = frameSet;
            index = 0;
            Current = default(Frame);
            this.context = context;
        }

        public void Dispose()
        {
            // Method intentionally left empty.
        }

        public bool MoveNext()
        {
            if ((uint)index < (uint)frameSet.Count)
            {
                var ptr = NativeMethods.rs2_extract_frame(frameSet.Instance.Handle, index, out var error);
                var task = context.FramePool.Next(ptr, CancellationToken.None);
                task.Wait();
                Current = task.Result;
                index++;
                return true;
            }

            index = frameSet.Count + 1;
            Current = null;
            return false;
        }

        public void Reset()
        {
            index = 0;
            Current = null;
        }
    }
}
