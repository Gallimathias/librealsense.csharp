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
    public interface IAsyncPool
    {
        Task OnError(Exception exception, CancellationToken cancellationToken);
        Task OnRelease(IAsyncPoolElement poolElement, CancellationToken cancellationToken);
        Task OnFinalize(IAsyncPoolElement poolElement, CancellationToken cancellationToken);
    }
}
