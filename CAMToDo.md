If you're interested in CAM algorithms and would like to help with this project, here is a list of algorithms and ideas that need implementing!

# 1D algorithms #
These are basically drilling tool paths.
  * Travelling salesman problem (TSP) solver

  * Genetic algorithm based?
  * simulated annealing based?
  * modified TSP: asymmetric graph. This could be used as a general algorithm for optimizing 2D paths. You come into a 'city' at the beginning of a cut and exit the 'city' after the cut (at another point in space). Or maybe you could implement this cutting idea with two cities per cut. As you enter the first city there is a zero-cost edge in the graph which you must travel along.

# 2D algorithms #
  * Line segment offset
  * Line/Arc segment offset
  * Line-filter (aka Douglass-Peuckert)
  * Arc-filter (find arcs with given tolerance from short line segments)

## 2D clearing algorithms ##
once we have a pocket or contour (from the offset algorithm) we need to clear(rough) it.
  * zigzag
  * spiral
  * successive offset
  * adaptive (lot's of ideas to be added later on a separate page AdaptiveRoughing)

# 3D algorithms #
  * Drop-Cutter (work in progress)
    * sample rate calculation needed
  * kd-tree search for triangles under the cutter
  * Push-cutter
  * STL surface z-slice
  * STL surface outline ('shadow' of STL surface on XY-plane).
    * possibly a combination of two algorithms: split the triangulation into convex parts. calculate the convex hull of the parts. (?) but how do we find the overall outline (?)
  * Waterline finishing (a combination of z-slice and push-cutter)

# Multi-threading + Benchmarking #
To take advantage of multi-core processors as much as possible of the algorithms should be multi-threaded.

A set of standard benchmarking tests + timing mechanisms will be useful for measuring improvements in algorithm speed.