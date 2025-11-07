using Unity.Mathematics;
using UnityEngine;

namespace BigContainers.Runtime.Helpers
{
    /// <summary>
    /// General helpers for common math involving zero-indexed binary trees,
    /// particularly in Eytzinger memory layout.
    /// </summary>
    public static class BinaryTree
    {
        /// <summary>
        /// Tree depth of node at i in Eytzinger layout
        /// </summary>
        public static int LevelOf(int i) => 31 - math.lzcnt(i + 1);

        /// <summary>
        /// Number of levels in a balanced full tree of the given capacity
        /// </summary>
        public static int NumLevelsFor(int capacity) => LevelOf(capacity - 1) + 1;

        /// <summary>
        /// Left child of node i in Eytzinger layout
        /// </summary>
        public static int LeftChild(int i) => 2 * i + 1;

        /// <summary>
        /// Right child of node i in Eytzinger layout
        /// </summary>
        public static int RightChild(int i) => 2 * i + 2;

        /// <summary>
        /// Parent of node i in Eytzinger layout
        /// </summary>
        public static int ParentOf(int i) => (i - 1) / 2;
    }
}