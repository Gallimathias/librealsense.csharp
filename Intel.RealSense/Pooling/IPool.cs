using System;
using System.Collections.Generic;
using System.Text;

namespace Intel.RealSense.Pooling
{
    public interface IPool
    {
        void OnError(Exception exception);
        void OnRelease(IPoolElement poolElement);
        void OnFinalize(IPoolElement poolElement);
    }
}
