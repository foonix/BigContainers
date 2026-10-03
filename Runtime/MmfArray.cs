using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace BigContainers.Runtime
{
    /// <summary>
    /// Wrapper for MemoryMappedFile that provides a NativeArray interface to the file.
    /// 
    /// </summary>
    /// <typeparam name="T">Type of record to store in the file</typeparam>
    public class MmfArray<T> : IDisposable
        where T : unmanaged
    {
        readonly MemoryMappedFile mmf;
        readonly MemoryMappedViewAccessor viewAccessor;
        readonly IntPtr startPtr;
        readonly long length;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        private AtomicSafetyHandle atomicSafetyHandle;
#endif

        /// <summary>
        /// Open an existing FileStream as a memory mapped file.
        /// </summary>
        public MmfArray(FileStream fileStream, long startOffset = 0, MemoryMappedFileAccess access = MemoryMappedFileAccess.ReadWrite, string mapName = null)
        {
            mmf = MemoryMappedFile.CreateFromFile(fileStream, mapName, 0, access, HandleInheritability.None, true);
            viewAccessor = mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
            unsafe
            {
                byte* ptr = null;
                viewAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
                startPtr = (IntPtr)(ptr + startOffset);
            }
            length = (viewAccessor.Capacity - startOffset) / Marshal.SizeOf<T>();
        }

        public NativeArray<T> AsArray()
        {
            unsafe
            {
                NativeArray<T> array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(startPtr.ToPointer(), (int)length, Allocator.None);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, atomicSafetyHandle);
#endif

                return array;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                viewAccessor.SafeMemoryMappedViewHandle.ReleasePointer();
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckDeallocateAndThrow(atomicSafetyHandle);
                AtomicSafetyHandle.Release(atomicSafetyHandle);
#endif
            }

            viewAccessor.Dispose();
            mmf.Dispose();
        }

        ~MmfArray()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}