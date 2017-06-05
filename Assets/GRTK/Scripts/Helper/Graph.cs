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
            nodes.Remove(node);
            foreach (BoundaryGraphNode neighbor in node.GetNeighbors())
            {
                neighbor.RemoveNeighbor(node);
            }
        }

        // Checks existing graph for any intersections, then splits the edges that intersect
        // By a node with a parent to each boundary that caused the intersection
        public void Unify()
        {
            // Code goes here
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
        Vector2 position;
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
    }

}