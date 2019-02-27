﻿using System;
using System.Runtime.InteropServices;

namespace Intel.RealSense.Frames
{
    public class Points : Frame
    {
        public int Count => NativeMethods.rs2_get_frame_points_count(Instance.Handle, out var error);

        protected IntPtr VertexData => NativeMethods.rs2_get_frame_vertices(Instance.Handle, out var error);
        protected IntPtr TextureData => NativeMethods.rs2_get_frame_texture_coordinates(Instance.Handle, out var error);

        public Points(Context context, IntPtr ptr) : base(context, ptr)
        {
        }

        /// <summary>
        /// Copy frame data to Vertex array
        /// </summary>
        /// <param name="array"></param>
        public void CopyTo(Vertex[] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            var handle = GCHandle.Alloc(array, GCHandleType.Pinned);

            try
            {
                NativeMethods.memcpy(handle.AddrOfPinnedObject(), VertexData, Count * Marshal.SizeOf(typeof(Vertex)));
            }
            finally
            {
                handle.Free();
            }
        }

        
        /// <summary>
        /// Copy frame data to TextureCoordinate array
        /// </summary>
        /// <param name="textureArray"></param>
        public void CopyTo(TextureCoordinate[] textureArray)
        {
            if (textureArray == null)
                throw new ArgumentNullException(nameof(textureArray));

            var handle = GCHandle.Alloc(textureArray, GCHandleType.Pinned);
            try
            {
                var size = Count * Marshal.SizeOf(typeof(TextureCoordinate));
                NativeMethods.memcpy(handle.AddrOfPinnedObject(), TextureData, size);
            }
            finally
            {
                handle.Free();
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex
        {
            public float X;
            public float Y;
            public float Z;
        }

        public struct TextureCoordinate
        {
            public float U;
            public float V;
        }
    }
}
