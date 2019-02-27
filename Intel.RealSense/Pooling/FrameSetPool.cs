using Intel.RealSense.Pooling;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Intel.RealSense.Frames;

namespace Intel.RealSense
{
    public sealed class FrameSetPool : Pool<FrameSet>
    {
        public FrameSetPool(Context context) : base(context)
        {
        }

        public async Task<FrameSet> Next(IntPtr ptr, CancellationToken cancellationToken)
        {
            var result = await Next(cancellationToken);

            if (result == null)
            {
                result = new FrameSet(context, ptr);
                await result.Pool(this, cancellationToken);
            }
            else
            {
                result.Initialize(ptr);
            }

            return result;
        }
    }
}
