using System.Threading;
using System.Threading.Tasks;

namespace Intel.RealSense.Pooling
{
    public interface IAsyncPoolElement
    {
        Task Pool(IAsyncPool pool, CancellationToken cancellationToken);
        Task Release(CancellationToken cancellationToken);
    }
}
