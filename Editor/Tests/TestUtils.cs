using NUnit.Framework;
using System;
using Unity.Collections;

namespace BigContainers.Editor.Tests
{
    public static class TestUtils
    {
        public static NativeArray<float> CreateSortedFloatArray(int length)
        {
            NativeArray<float> array = new(length, Allocator.Temp);

            for (int i = 0; i < length; i++)
            {
                array[i] = i;
            }

            return array;
        }

        public static NativeArray<float> CreateReversedSortedFloatArray(int length)
        {
            NativeArray<float> array = new(length, Allocator.Temp);

            for (int i = 0; i < length; i++)
            {
                array[i] = length - i - 1;
            }

            return array;
        }

        public static NativeArray<float> CreateBitonicSortedFloatArray(int length)
        {
            NativeArray<float> array = new(length, Allocator.Temp);

            for (int j = 0; j < length; j++)
            {
                if (j < length / 2)
                {
                    array[j] = j;
                }
                else
                {
                    array[j] = length - j;
                }
            }

            return array;
        }

        public static void VerifyKthSmallestProperty(NativeArray<float> array, int k)
        {
            for (int i = 0; i < k; i++)
            {
                Assert.LessOrEqual(array[i], array[k]);
            }
            for (int i = k + 1; i < array.Length; i++)
            {
                Assert.Greater(array[i], array[k]);
            }
        }
    }
}