using System;
using System.IO;
using BigContainers.Runtime;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;

namespace BigContainers.Editor.Tests
{
    public class MmfArrayTests
    {
        private struct ReadJob : IJob
        {
            [ReadOnly] public NativeArray<byte> input;
            public NativeArray<byte> output;
            public void Execute() => output[0] = input[0];
        }

        [Test]
        public void MapsOnlyFileBytesAfterOffsetAndOutlivesSourceStream()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".bin");
            File.WriteAllBytes(path, new byte[] { 99, 10, 20, 30 });
            try
            {
                MmfArray<byte> mapping;
                using (var stream = File.OpenRead(path))
                    mapping = new MmfArray<byte>(stream, 1);
                using (mapping)
                {
                    var bytes = mapping.AsArray();
                    Assert.AreEqual(3, bytes.Length);
                    Assert.AreEqual(10, bytes[0]);
                    Assert.AreEqual(30, bytes[2]);
                    mapping.Dispose();
                    Assert.DoesNotThrow(() => mapping.Dispose());
                    Assert.Throws<ObjectDisposedException>(() => mapping.AsArray());
                }
                using var exclusive = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            finally { File.Delete(path); }
        }

        [Test]
        public void WritableMappingChangesTheUnderlyingFile()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".bin");
            File.WriteAllBytes(path, new byte[] { 1, 2, 3 });
            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
                using (var mapping = new MmfArray<byte>(stream, access: System.IO.MemoryMappedFiles.MemoryMappedFileAccess.ReadWrite))
                {
                    var bytes = mapping.AsArray();
                    bytes[1] = 42;
                }
                CollectionAssert.AreEqual(new byte[] { 1, 42, 3 }, File.ReadAllBytes(path));
            }
            finally { File.Delete(path); }
        }

        [Test]
        public void DisposalRejectsOutstandingJobsWithoutReleasingTheMapping()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".bin");
            File.WriteAllBytes(path, new byte[] { 42 });
            try
            {
                using var stream = File.OpenRead(path);
                using var mapping = new MmfArray<byte>(stream);
                using var output = new NativeArray<byte>(1, Allocator.TempJob);
                var handle = new ReadJob { input = mapping.AsArray(), output = output }.Schedule();
                try
                {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    Assert.Throws<InvalidOperationException>(() => mapping.Dispose());
#endif
                }
                finally { handle.Complete(); }
                Assert.AreEqual(42, output[0]);
                Assert.AreEqual(42, mapping.AsArray()[0]);
            }
            finally { File.Delete(path); }
        }
    }
}
