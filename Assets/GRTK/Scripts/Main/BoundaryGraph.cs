﻿using UnityEngine;
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
                        BoundaryGraphNode intersectionNode = new BoundaryGraphNode(intersection, true);
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

        public IEnumerator<BoundaryGraphNode> GetEnumerator()
        {
            return nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return nodes.GetEnumerator();
        }
    }

    public class BoundaryGraphNode
    {
        public Vector2 position { get; private set; }
        bool intersectionNode;

        List<Boundary> parentBoundaries = new List<Boundary>();
    

        private HashSet<BoundaryGraphNode> neighbors = new HashSet<BoundaryGraphNode>();

        public BoundaryGraphNode(Vector2 position, bool intersectionNode)
        {
            this.position = position;
            this.intersectionNode = intersectionNode;
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
    }

}