using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace BigContainers.Runtime
{
    /// <summary>
    /// Owns a file mapping and exposes its unmanaged records without copying.
    /// Arrays borrow the mapping: complete all jobs before disposing this owner.
    /// The caller retains ownership of the source FileStream.
    /// </summary>
    public class MmfArray<T> : IDisposable where T : unmanaged
    {
        MemoryMappedFile mmf;
        MemoryMappedViewAccessor viewAccessor;
        IntPtr startPtr;
        readonly int length;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        AtomicSafetyHandle atomicSafetyHandle;
#endif

        public MmfArray(FileStream fileStream, long startOffset = 0,
            MemoryMappedFileAccess access = MemoryMappedFileAccess.Read, string mapName = null)
        {
            long fileLength = fileStream.Length;
            int stride = UnsafeUtility.SizeOf<T>();
            // Validate the record extent once, before exposing potentially unsafe array access.
            if (startOffset < 0 || startOffset > fileLength)
                throw new ArgumentOutOfRangeException(nameof(startOffset));
            long byteLength = fileLength - startOffset;
            if (byteLength % stride != 0)
                throw new InvalidDataException("The mapped extent must contain whole records.");
            length = checked((int)(byteLength / stride));
            try
            {
                mmf = MemoryMappedFile.CreateFromFile(fileStream, mapName, 0, access, HandleInheritability.None, true);
                // Explicit size avoids page-rounded capacities reported by some runtimes.
                viewAccessor = mmf.CreateViewAccessor(0, fileLength, access);
                unsafe
                {
                    byte* ptr = null;
                    viewAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
                    startPtr = (IntPtr)(ptr + viewAccessor.PointerOffset + startOffset);
                }
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                atomicSafetyHandle = AtomicSafetyHandle.Create();
#endif
            }
            catch
            {
                Dispose(true);
                throw;
            }
        }

        public NativeArray<T> AsArray()
        {
            // Conversion of a released pointer cannot safely detect disposal downstream.
            if (viewAccessor == null) throw new ObjectDisposedException(nameof(MmfArray<T>));
            unsafe
            {
                var array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(startPtr.ToPointer(), length, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array, atomicSafetyHandle);
#endif
                return array;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (!AtomicSafetyHandle.IsDefaultValue(in atomicSafetyHandle))
            {
                // A rejected disposal must leave both the handle and mapping intact.
                if (disposing) AtomicSafetyHandle.CheckDeallocateAndThrow(atomicSafetyHandle);
                AtomicSafetyHandle.Release(atomicSafetyHandle);
                atomicSafetyHandle = default;
            }
#endif
            if (startPtr != IntPtr.Zero)
            {
                viewAccessor.SafeMemoryMappedViewHandle.ReleasePointer();
                startPtr = IntPtr.Zero;
            }
            viewAccessor?.Dispose();
            viewAccessor = null;
            mmf?.Dispose();
            mmf = null;
        }

        ~MmfArray() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
