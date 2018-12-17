using Intel.RealSense.Pooling;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Intel.RealSense
{
    public class FrameSetPool : Pool<FrameSet>
    {
        public async Task<FrameSet> Next(IntPtr ptr, CancellationToken cancellationToken)
        {
            var result = await Next(cancellationToken);

            if (result == null)
                result = new FrameSet(ptr);

            return result;
        }
    }
}
