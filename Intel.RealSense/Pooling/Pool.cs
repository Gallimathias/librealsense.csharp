using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Intel.RealSense.Pooling
{
    public abstract class Pool<T> : IAsyncPool where T : IAsyncPoolElement
    {
        protected readonly ConcurrentStack<IAsyncPoolElement> stack;

        public Pool()
            => stack = new ConcurrentStack<IAsyncPoolElement>();

        public virtual Task<T> Next(CancellationToken cancellationToken) => Task.Run(() =>
        {
            if (stack.TryPop(out IAsyncPoolElement poolElement))
            {
                return (T)poolElement;
            }
            else
            {
                return default(T);
            }
        }, cancellationToken);

        public virtual Task OnError(Exception exception, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public virtual Task OnFinalize(IAsyncPoolElement poolElement, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public virtual Task OnRelease(IAsyncPoolElement poolElement, CancellationToken cancellationToken)
            => Task.Run(() => stack.Push(poolElement), cancellationToken);
    }
}
