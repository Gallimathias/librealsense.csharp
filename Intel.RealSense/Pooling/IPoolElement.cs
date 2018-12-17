using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Intel.RealSense.Pooling
{
    public interface IPoolElement
    {
        void Pool(IPool pool);
        void Release();
    }
    public interface IAsyncPoolElement
    {
        Task Pool(IAsyncPool pool, CancellationToken cancellationToken);
        Task Release(CancellationToken cancellationToken);
    }
}
