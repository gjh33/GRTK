using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

// This class represents a boundary object inside a level
namespace GRTK
{
    public class Boundary : MonoBehaviour
    {

        // Return a list of points representing the 2D bounds from a top down view in clockwise order
        public List<Vector2> Get2DBounds()
        {
            // Get data about the boundary
            List<Vector2> toRet = new List<Vector2>();

            // Negative scales can mess up the order of the function below
            if (transform.localScale.x < 0 || transform.localScale.y < 0)
            {
                Debug.LogWarning(gameObject.name + ": Boundaries with negative scales can fail interior tests. See source for more detail!");
            }

            // Use the local coordinates of a cube corner and transform into world
            Vector3 tr = transform.TransformPoint(0.5f, 0.5f, 0);
            Vector3 br = transform.TransformPoint(0.5f, -0.5f, 0);
            Vector3 bl = transform.TransformPoint(-0.5f, -0.5f, 0);
            Vector3 tl = transform.TransformPoint(-0.5f, 0.5f, 0);

            // Add them as Vector2D
            toRet.Add(new Vector2(tr.x, tr.y));
            toRet.Add(new Vector2(br.x, br.y));
            toRet.Add(new Vector2(bl.x, bl.y));
            toRet.Add(new Vector2(tl.x, tl.y));

            return toRet;
        }

        // Adds this boundary to a graph as a series of nodes and edges
        public void AddToGraph(BoundaryGraph graph)
        {
            List<Vector2> corners = Get2DBounds();
            List<BoundaryGraphNode> nodes = new List<BoundaryGraphNode>();
            int i = 0;
            // Convert each point into a node
            for (i = 0; i < corners.Count; i++)
            {
                BoundaryGraphNode node = new BoundaryGraphNode(corners[i]);
                nodes.Add(node);
            }
            // Link each node to the next one (note linking is reciprical so I don't have to link the reverse)
            for (i = 0; i < nodes.Count; i++)
            {
                int nextInd = (i + 1) % nodes.Count;
                nodes[i].AddNeighbor(nodes[nextInd]);
            }
            // Add the nodes to the graph
            foreach (BoundaryGraphNode node in nodes)
            {
                node.AddParentBoundary(this);
                graph.AddNode(node);
            }
        }

        // Checks if a point is interior to this boundary
        public bool Interior(Vector2 point)
        {
            List<Vector2> bounds = Get2DBounds();
            // Check if point is interior to each of the 4 edges
            Line2D l1 = new Line2D(bounds[1], bounds[0]);
            Line2D l2 = new Line2D(bounds[2], bounds[1]);
            Line2D l3 = new Line2D(bounds[3], bounds[2]);
            Line2D l4 = new Line2D(bounds[0], bounds[3]);

            // All lines are build counter clockwise so we check if point is left of all of them
            return l1.Left(point) && l2.Left(point) && l3.Left(point) && l4.Left(point);
        }

        #region Editor
        [MenuItem("GameObject/GRTK/Boundary", false, 12)]
        public static void SpawnBoundaryObject(MenuCommand menuCommand)
        {
            // Create a new GameObject with a Boundary attached
            GameObject newBoundary = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newBoundary.name = "Boundary";
            newBoundary.AddComponent<Boundary>();

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(newBoundary, (GameObject)menuCommand.context);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(newBoundary, "Create " + newBoundary.name);

            // Set the selected object to be the boundary you just created
            Selection.activeObject = newBoundary;
        }
        #endregion
    }
}
