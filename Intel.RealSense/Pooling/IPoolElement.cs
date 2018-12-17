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
    public interface IPoolElement<T>
    {
        void Pool(IPool<T> pool);
        void Release();
    }
    public interface IPoolElementAsync
    {
        Task Pool(IPoolAsync pool, CancellationToken cancellationToken);
        Task Release(CancellationToken cancellationToken);
    }
    public interface IPoolElementAsync<T>
    {
        Task Pool(IPoolAsync<T> pool, CancellationToken cancellationToken);
        Task Release(CancellationToken cancellationToken);
    }
}
