using System;
using System.Collections;
using System.Collections.Generic;

namespace BigContainers.Runtime
{
    public interface IKdNode
    {
        /// <summary>
        /// Get a float representation of a given dimension's coordinate.
        /// This is used for tree searching. It may be rounded to the nearest float,
        /// but larger approximation errors can cause traversal to miss nodes.
        /// </summary>
        /// <param name="dimension"></param>
        /// <returns></returns>
        float GetCoordinate(int dimension);
    }
}
