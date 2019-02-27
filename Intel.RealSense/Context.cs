using Intel.RealSense.Devices;
using Intel.RealSense.Frames;
using Intel.RealSense.Pooling;
using Intel.RealSense.Types;
using System;
using System.Runtime.InteropServices;

namespace Intel.RealSense
{
    public class Context : IDisposable
    {
        /// <summary>
        /// create a static snapshot of all connected devices at the time of the call
        /// </summary>
        public DeviceList Devices => QueryDevices();
        public string Version
        {
            get
            {
                if (apiVersion / 10000 == 0)
                    return apiVersion.ToString();

                return (apiVersion / 10000) + "." + (apiVersion % 10000) / 100 + "." + (apiVersion % 100);
            }
        }

        public delegate void OnDevicesChangedDelegate(DeviceList removed, DeviceList added);
        public event OnDevicesChangedDelegate OnDevicesChanged;

        internal FramePool FramePool { get; private set; }
        internal FrameSetPool FrameSetPool { get; private set; }

        internal IntPtr Instance => instance.Handle;

        private readonly HandleRef instance;
        public readonly int apiVersion;
       

        // Keeps the delegate alive, if we were to assign onDevicesChanged directly, there'll be 
        // no managed reference it, it will be collected and cause a native exception.
        private readonly rs2_devices_changed_callback onDevicesChangedCallback;

        /// <summary>
        /// default librealsense context class
        /// </summary>
        public Context()
        {
            apiVersion = NativeMethods.rs2_get_api_version(out var error);
            instance = new HandleRef(this, NativeMethods.rs2_create_context(apiVersion, out error));

            onDevicesChangedCallback = new rs2_devices_changed_callback(InvokeDevicesChanged);
            NativeMethods.rs2_set_devices_changed_callback(instance.Handle, onDevicesChangedCallback, IntPtr.Zero, out error);

            FramePool = new FramePool(this);
            FrameSetPool = new FrameSetPool(this);
        }

        /// <summary>
        /// create a static snapshot of all connected devices at the time of the call
        /// </summary>
        /// <returns></returns>
        public DeviceList QueryDevices(bool include_platform_camera = false)
        {
            var ptr = NativeMethods.rs2_query_devices_ex(instance.Handle,
                include_platform_camera ? 0xff : 0xfe, out var error);

            return new DeviceList(this, ptr);
        }

        private void InvokeDevicesChanged(IntPtr removedList, IntPtr addedList, IntPtr userData)
        {
            using (var removed = new DeviceList(this, removedList))
            using (var added = new DeviceList(this, addedList))
                OnDevicesChanged?.Invoke(removed, added);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    OnDevicesChanged = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                if (instance.Handle != IntPtr.Zero)
                {
                    NativeMethods.rs2_delete_context(instance.Handle);
                    //instance = new HandleRef(this, IntPtr.Zero);
                }

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~Context()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }


}
