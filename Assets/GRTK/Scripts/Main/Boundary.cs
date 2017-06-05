using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

// This class represents a boundary object inside a level
namespace GRTK
{
    public class Boundary : MonoBehaviour
    {

        // Return a list of points representing the 2D bounds from a top down view
        public List<Vector2> Get2DBounds()
        {
            // Get data about the boundary
            List<Vector2> toRet = new List<Vector2>();
            Vector3 center = GetComponent<Renderer>().bounds.center;
            Vector3 extents = GetComponent<Renderer>().bounds.extents;

            //Find the 4 corners
            toRet.Add(new Vector2(center.x + extents.x, center.y + extents.y));
            toRet.Add(new Vector2(center.x + extents.x, center.y - extents.y));
            toRet.Add(new Vector2(center.x - extents.x, center.y - extents.y));
            toRet.Add(new Vector2(center.x - extents.x, center.y + extents.y));

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
                BoundaryGraphNode node = new BoundaryGraphNode(corners[i], false);
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
            // Do this by taking the scaler of vectors of vectors
            // If rect is defined by points A,B,C,D and we test point M
            // (0 < AM dot AB < AB dot AB) and (0 < AM dot AD < AD dot AD)
            List<Vector2> corners = Get2DBounds();
            Vector2 cornerA = corners[0];
            Vector2 cornerB = corners[1];
            Vector2 cornerD = corners[3];
            Vector2 AM = point - cornerA;
            Vector2 AB = cornerB - cornerA;
            Vector2 AD = cornerD - cornerA;
            double AMdotAB = Vector2.Dot(AM, AB);
            double ABdotAB = Vector2.Dot(AB, AB);
            double AMdotAD = Vector2.Dot(AM, AD);
            double ADdotAD = Vector2.Dot(AD, AD);

            bool pass = true;
            // Now we run the pass bool through a series of tests. Failing one will trip a false.
            if (AMdotAB <= 0)
                pass = false;
            if (AMdotAB >= ABdotAB)
                pass = false;
            if (AMdotAD <= 0)
                pass = false;
            if (AMdotAB >= ADdotAD)
                pass = false;

            return pass;
        }

        #region Editor
        [MenuItem("GameObject/GRTK/Boundary", false, 12)]
        public static void SpawnBoundaryObject(MenuCommand menuCommand)
        {
            // Create a new GameObject with a Level attached
            GameObject newBoundary = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newBoundary.name = "Boundary";
            newBoundary.AddComponent<Boundary>();

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(newBoundary, (GameObject)menuCommand.context);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(newBoundary, "Create " + newBoundary.name);

            // Set the selected object to be the level you just created
            Selection.activeObject = newBoundary;
        }
        #endregion
    }
}
