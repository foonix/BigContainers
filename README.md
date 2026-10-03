# BigContainers

A collection of algorithms and utilities for dealing with large amounts of data when using Unity's Burst compiler.

## For dealing with large amounts of data

### MmfArray

`MmfArray<T>` is a wrapper class for `MemoryMappedFile` that allows access from Burst code.

`MmfArray` its self is a managed object, and can't be directly used in Burst.
Use the `AsArray()` method to get a `NativeArray<T>` pointing to the file's view.

`MmfArray` must be kept alive as long as any of the returned `NativeArray`s are in use.

This container has several advantages:
- Arrays exceeding main memory capacity can be partially stored on disk without swapping.
- This indirectly allows file IO from the Unity Job system. Changes to the array will (eventually) be written to disk.
- The array's content can be persisted on disk between sessions.  This is particularly useful for expensive-to-generate indices on static data.
- Various other applications of MMF are possible, such as data sharing memory programs.

Note: Portability of an array between platforms is not guaranteed, especially between big-endian and little-endian systems.  
For maximum performance, array members are interpreted directly as `<T>` without marshaling.

## Implicit data structures

These are structs that help organize the content of an array into complex data structures.  They are not containers in and of themselves, 
and must be provided the array that actually stores the content.

"implicit" data structures and "in-place" algorithms are used wherever possible to reduce allocations.

The main purpose is to:
- avoid using additional memory for things like left/right indices on (already large) data sets
- avoid allocating large amounts of temporary memory for common operations.
- try to improve spatial locality for common operations. 

### KdTree

Sorts an array into a k-dimensional search tree, and provides tree traversal algorithm.

Tree construction uses and offline algorithm, and nodes can not be moved, added, or removed without re-sorting the array. 

The resulting tree is a left-balanced, complete tree in Eytzinger layout.

Tree traversal algorithm uses the stack-free traversal by Ingo Wald. 
See: [https://ingowald.blog/2022/10/25/stack-free-k-d-tree-traversal/] and [ingowald/cudaKDTree].


## Helpers

The `Helpers` namespace contains several helper structs for dealing with common math involved with implicit trees/heaps,
many borrowed from [ingowald/cudaKDTree].

    int numSettled = new FullBinaryTreeOf(step).NumNodes();

Burst/LLVM can delete the struct and inline the method, and can vectorize the instructions.

## A note about Burst compatibility

Burst supports c# generic types, interfaces, and interfaces with generic type parameters.
What it does not support is reference types.  As this library tries to generalize various algorithms, this has two major implications throughout:

- All generic type parameters are required to meet `unamanaged` type criteria;  The implementing type must thus be a `struct` (or primitive) value type,
  and can not contain any references.
- Interfaces can only be used as generic type constraints. It does not work to pass an interface object/struct (in the normal way) as a parameter.
  However, it's possible to accept a parameter with generic type `<T>`, and constrain the type to the interface.
  This will work as long as the concrete type is `unmanaged` and supports the required interfaces.

On the upside, the compiler will be able to optimize against the actual concrete types at compile time.
This generates very fast code.  It will often optimize the struct and/or methods down to the bare minimum assembly required to produce the intended results.

Passing value types by generic interface in this way will not cause boxing, even in managed code.  It's a neat trick for writing faster code, use it.

## Credits

Huge thanks to Ingo Wald et all for various papers on KD tree construction, with excellent examples.  See [ingowald/cudaKDTree]



