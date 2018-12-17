using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Intel.RealSense.Pooling
{
    public interface IPool
    {
        void OnError(Exception exception);
        void OnRelease(IPoolElement poolElement);
        void OnFinalize(IPoolElement poolElement);
    }
    public interface IPool<T>
    {
        void OnError(Exception exception);
        void OnRelease(IPoolElement<T> poolElement);
        void OnFinalize(IPoolElement<T> poolElement);
    }
    public interface IPoolAsync
    {
        Task OnError(Exception exception, CancellationToken cancellationToken);
        Task OnRelease(IPoolElement poolElement, CancellationToken cancellationToken);
        Task OnFinalize(IPoolElement poolElement, CancellationToken cancellationToken);
    }
    public interface IPoolAsync<T>
    {
        Task OnError(Exception exception, CancellationToken cancellationToken);
        Task OnRelease(IPoolElement<T> poolElement, CancellationToken cancellationToken);
        Task OnFinalize(IPoolElement<T> poolElement, CancellationToken cancellationToken);
    }
}
