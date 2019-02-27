using System;
using System.Threading;
using System.Threading.Tasks;

namespace Intel.RealSense.Pooling
{
    public interface IAsyncPool
    {
        Task OnError(Exception exception, CancellationToken cancellationToken);
        Task OnRelease(IAsyncPoolElement poolElement, CancellationToken cancellationToken);
        Task OnFinalize(IAsyncPoolElement poolElement, CancellationToken cancellationToken);
    }
}
