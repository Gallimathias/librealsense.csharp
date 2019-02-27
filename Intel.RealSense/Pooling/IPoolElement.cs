using System;
using System.Collections.Generic;
using System.Text;

namespace Intel.RealSense.Pooling
{
    public interface IPoolElement
    {
        void Pool(IPool pool);
        void Release();
    }
}
