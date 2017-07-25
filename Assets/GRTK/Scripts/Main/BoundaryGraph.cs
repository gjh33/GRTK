using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace GRTK
{
    /// <summary>
    /// A graph representation of a shape to track the edges and connectivity for the recovery algorithm
    /// </summary>
    public class BoundaryGraph : IEnumerable<BoundaryGraphNode>
    {
        private HashSet<BoundaryGraphNode> nodes = new HashSet<BoundaryGraphNode>();

        // Add a node to the graph
        public void AddNode(BoundaryGraphNode node)
        {
            nodes.Add(node);
        }

        // Removes a node and removes it as a neighbor from any of it's neighbors
        public void DeleteNode(BoundaryGraphNode node)
        {
            List<BoundaryGraphNode> removalList = new List<BoundaryGraphNode>();
            foreach (BoundaryGraphNode neighbor in node.GetNeighbors())
            {
                // Have to use removal list because remove from the list of neighbors while itterating
                removalList.Add(neighbor);
            }

            foreach (BoundaryGraphNode neighbor in removalList)
            {
                node.RemoveNeighbor(neighbor);
            }
            // Remove the node from the graph
            nodes.Remove(node);
        }

        // Returns true if the graph has no nodes
        public bool IsEmpty()
        {
            return nodes.Count == 0;
        }

        // Checks existing graph for any intersections, then splits the edges that intersect
        // By a node with a parent to each boundary that caused the intersection
        public void Unify()
        {
            List<UnorderedPair<BoundaryGraphNode, BoundaryGraphNode>> edges = getEdgesAsUniquePairs();
            // Intersect each edge using Line2D and split the edges
            int i, j;
            for (i = 0; i < edges.Count; i++)
            {
                UnorderedPair<BoundaryGraphNode, BoundaryGraphNode> edge1 = edges[i];

                // Build the first line
                Line2D line1 = new Line2D(edge1.item1.position, edge1.item2.position);

                for (j = i; j < edges.Count; j++)
                {
                    UnorderedPair<BoundaryGraphNode, BoundaryGraphNode> edge2 = edges[j];

                    // Skip if they are the same edge
                    if (i == j) continue;

                    // Skip if these edges are already neighbors (share a common vertex)
                    // because they already intersect at that common vertex
                    if (edge1.item1 == edge2.item1 || edge1.item1 == edge2.item2 || edge1.item2 == edge2.item1 || edge1.item2 == edge2.item2)
                        continue;

                    // NOTE: no need to equals test since this is a set of unique unordered pairs
                    Line2D line2 = new Line2D(edge2.item1.position, edge2.item2.position);

                    // test for intersection
                    Vector2 intersection;
                    if (line1.Intersect(line2, out intersection))
                    {
                        // Create intersection node and split edges
                        BoundaryGraphNode intersectionNode = new BoundaryGraphNode(intersection);
                        intersectionNode.AddParentBoundary(edge1.item1.GetParentBoundaries()[0]);
                        intersectionNode.AddParentBoundary(edge2.item1.GetParentBoundaries()[0]);
                        AddNode(intersectionNode);
                        // Remove neighbors
                        edge1.item1.RemoveNeighbor(edge1.item2); // Method auto removes the reciprical relation too
                        edge2.item1.RemoveNeighbor(edge2.item2);
                        // Rejoin the verticies to intersection
                        intersectionNode.AddNeighbor(edge1.item1);
                        intersectionNode.AddNeighbor(edge1.item2);
                        intersectionNode.AddNeighbor(edge2.item1);
                        intersectionNode.AddNeighbor(edge2.item2);
                        // Now we need to update our edges list so future runs of the loop interact with the new edges not the old ones
                        edges.Remove(edge1);
                        edges.Remove(edge2);
                        // Decrement outter loop because we are removing the current index
                        i--;
                        // Add in the new edges we just formed.
                        edges.Add(new UnorderedPair<BoundaryGraphNode, BoundaryGraphNode>(edge1.item1, intersectionNode));
                        edges.Add(new UnorderedPair<BoundaryGraphNode, BoundaryGraphNode>(edge1.item2, intersectionNode));
                        edges.Add(new UnorderedPair<BoundaryGraphNode, BoundaryGraphNode>(edge2.item1, intersectionNode));
                        edges.Add(new UnorderedPair<BoundaryGraphNode, BoundaryGraphNode>(edge2.item2, intersectionNode));

                        /* Deeper explanation:
                         * We are removing the edge at index i, meaning that i+1 will become i. In order to not skip over i+1,
                         * we decrement i by 1 so when the loop restarts, new i = i + 1 = old i
                         * then we must immediately break the current inner loop since we are no longer matching intersections
                         * against edge i, since we just found one and removed that edge from the list
                         */
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets a list of node pairs that represent unique edges in the graph
        /// </summary>
        /// <returns>List of unordered pairs containing boundary nodes</returns>
        public List<UnorderedPair<BoundaryGraphNode, BoundaryGraphNode>> getEdgesAsUniquePairs()
        {
            // Find each unique edge as a pair of nodes using hashset
            HashSet<UnorderedPair<BoundaryGraphNode, BoundaryGraphNode>> edges = new HashSet<UnorderedPair<BoundaryGraphNode, BoundaryGraphNode>>();
            foreach (BoundaryGraphNode node1 in this)
            {
                foreach (BoundaryGraphNode node2 in this)
                {
                    // If they are equal skip
                    if (node1 == node2)
                        continue;
                    // If they are not neighbors, skip
                    if (!node1.HasNeighbor(node2))
                        continue;

                    UnorderedPair<BoundaryGraphNode, BoundaryGraphNode> temp = new UnorderedPair<BoundaryGraphNode, BoundaryGraphNode>(node1, node2);
                    edges.Add(temp);
                }
            }

            // Convert to list and return
            return new List<UnorderedPair<BoundaryGraphNode, BoundaryGraphNode>>(edges);
        }

        // Returns an extrema node (min x and y)
        public BoundaryGraphNode GetExtrema()
        {
            BoundaryGraphNode curMin = null;
            foreach (BoundaryGraphNode node in nodes)
            {
                if (curMin == null)
                    curMin = node;
                else if (node.position.x < curMin.position.x)
                    curMin = node;
                else if (node.position.x == curMin.position.x && node.position.y < curMin.position.y)
                    curMin = node;
            }

            return curMin;
        }

        // Given a root node, remove all nodes connected to it. Returns a list of removed nodes.
        // If the component is a polygon, it will return the nodes in the correct order in
        // relation to the neighbors.
        // Searches connectivity via DFS
        public List<BoundaryGraphNode> RemoveConnectedComponent(BoundaryGraphNode root)
        {
            // Initialize the dfs
            List<BoundaryGraphNode> visited = new List<BoundaryGraphNode>();
            Stack<BoundaryGraphNode> dfs = new Stack<BoundaryGraphNode>();
            dfs.Push(root);
            while (dfs.Count > 0)
            {
                // Pop the current node and mark as visited
                BoundaryGraphNode current = dfs.Pop();
                visited.Add(current);

                // For each neighbor if it hasn't been visited, add it to the stack
                foreach (BoundaryGraphNode neighbor in current.GetNeighbors())
                {
                    if (!visited.Contains(neighbor))
                    {
                        dfs.Push(neighbor);
                        // NOTE: If you need to add anything to the dfs when visiting a node
                        // do it here
                    }
                }
            }

            // Remove them from the graph
            foreach (BoundaryGraphNode node in visited)
            {
                DeleteNode(node);
            }

            return visited;
        }

        public IEnumerator<BoundaryGraphNode> GetEnumerator()
        {
            return nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return nodes.GetEnumerator();
        }

        //DEBUG CODE
        #region Debug
        // Dumps a series of spheres and cylinders to visualize the graph structure
        // Note you will have to manually clean these out when you're done
        public void _Visualize()
        {
            //NOTE: There is a debug material you can use to help visualize in assets/materials
            foreach (var node in this)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.transform.position = node.position;
                go.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                foreach (var neigh in node.GetNeighbors())
                {
                    var go2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    go2.transform.localScale = new Vector3(0.2f, (neigh.position - node.position).magnitude / 2, 0.2f);
                    go2.transform.position = neigh.position + ((node.position - neigh.position) / 2);
                    go2.transform.LookAt(new Vector3(neigh.position.x, neigh.position.y, 0));
                    go2.transform.Rotate(new Vector3(-90f, 0, 0));
                }
            }
        }
        #endregion
    }

    public class BoundaryGraphNode
    {
        public Vector2 position { get; private set; }

        List<Boundary> parentBoundaries = new List<Boundary>();


        private HashSet<BoundaryGraphNode> neighbors = new HashSet<BoundaryGraphNode>();

        public BoundaryGraphNode(Vector2 position)
        {
            this.position = position;
        }

        // Add a neighbor to the node. Also adds the reciprical relation
        public void AddNeighbor(BoundaryGraphNode node)
        {
            neighbors.Add(node);
            node.neighbors.Add(this);
        }

        // Remove a neighbor from the node. Also removes the reciprical relation
        public void RemoveNeighbor(BoundaryGraphNode node)
        {
            neighbors.Remove(node);
            node.neighbors.Remove(this);
        }

        // See a set of neighbors
        public HashSet<BoundaryGraphNode> GetNeighbors()
        {
            return neighbors;
        }

        // Add a parent boundary to this node
        public void AddParentBoundary(Boundary parent)
        {
            parentBoundaries.Add(parent);
        }

        // Get a list of parent boundaries
        public List<Boundary> GetParentBoundaries()
        {
            return parentBoundaries;
        }

        // Checks if a given boundary is a parent of this node
        public bool IsParent(Boundary parent)
        {
            return parentBoundaries.Contains(parent);
        }

        // Checks if this node has other node as a neighbor
        public bool HasNeighbor(BoundaryGraphNode other)
        {
            return neighbors.Contains(other);
        }

        //DEBUG CODE
        #region Debug
        public void _VisualizeNeighbors()
        {
            //NOTE: There is a debug material you can use to help visualize in assets/materials
            foreach (var node in neighbors)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.transform.position = node.position;
                go.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            }
        }

        public void _Visualize()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.position = position;
            go.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        }
        #endregion
    }
}