# Moving Maze Algorithm "[Origin Shift](https://www.youtube.com/watch?v=zbXKcDVV4G0&t=151s)" Explained:

### First, a short explanation about non-moving maze generation:

The algorithm used to generate a basic maze is [Wilson's Algorithm](https://en.wikipedia.org/wiki/Maze_generation_algorithm). This algorithm starts with a grid of nodes/tiles. Initially, all tiles are not a part of the maze.

Wilson's algorithm follows these steps:

1. A random tile "O" is chosen to be part of the maze.

2. Another random tile "T" is chosen to be the start of a "walk".

3. From tile "T", a path is randomly generated. If this path loops in on itself, all tiles in the loop are removed from the path.

4. The "walk" ends when a tile that is already in the maze is encountered during the walk.

5. Steps 2-4 are repeated until all tiles are a part of the maze.

Wilson's algorithm always generates a perfect maze, or a maze with no loops and unreachable areas.

### Visualizing mazes as trees:

Mazes can be visualized as trees, or acyclic graphs.

**Figure 1:**

![image](Visuals/MazeToTreeBig.png)

As shown above in figure 1:

1. You see a simple maze consisting of 9 nodes in a 3x3 pattern. The nodes are colored in with green or red. If there is a path between nodes, there is no maze wall. 

2. The red node is the "root" of the tree, and this was decided arbitrarily. As you can see from the image above, if you replace open paths between nodes in a maze with edges, you'll end up with a tree.

3. The tree representation of the maze is unfolded, and here you can see a more typical drawing of a tree.

The current tree visualization is undirected, meaning that the edges of the tree only go one way. We can also visualize the trees derived from these mazes as directed, as shown below:

**Figure 2:**

![image](Visuals/DirectedTree.png)

