using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Intel.RealSense
{
    class FrameSetMarshaler : ICustomMarshaler
    {
        private static FrameSetMarshaler instance;
        private readonly Context context;

        private FrameSetMarshaler(Context context)
        {
            this.context = context;
        }

        public static ICustomMarshaler GetInstance(Context context)
        {
            if (instance == null)
            {
                instance = new FrameSetMarshaler(context);
            }
            return instance;
        }

        public void CleanUpManagedData(object ManagedObj)
        {
        }

        public void CleanUpNativeData(IntPtr pNativeData)
        {
        }

        public int GetNativeDataSize()
        {
            return -1;
        }

        public IntPtr MarshalManagedToNative(object ManagedObj)
        {
            throw new NotImplementedException();
        }

        public object MarshalNativeToManaged(IntPtr pNativeData)
        {
            var task = context.FrameSetPool.Next(pNativeData, CancellationToken.None);
            task.Wait();
            return task.Result;
        }
    }
}
