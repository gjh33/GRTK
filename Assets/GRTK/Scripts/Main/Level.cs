using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

// Class represents a level. From this class you can compile the
// geometry
namespace GRTK
{
    public class Level : MonoBehaviour
    {
        // Compiles the level recovering interior geometry
        // Output is saved as a scriptable object instance in assets
        public void CompileLevel()
        {
            // Build a list of boundaries and a shape graph
            List<Boundary> boundaries = new List<Boundary>();
            BoundaryGraph graph = new BoundaryGraph();
            foreach(Transform trans in transform)
            {
                Boundary temp = trans.gameObject.GetComponent<Boundary>();
                if (temp != null)
                {
                    boundaries.Add(temp);
                    temp.AddToGraph(graph);
                }
            }

            // Unify the graph and make sure any intersections have a vertex
            graph.Unify();

            // Remove any verticies interior to another boundary
            // This does not account for exterior - exterior edges that cross the shape
            List<BoundaryGraphNode> deleteList = new List<BoundaryGraphNode>();
            foreach (BoundaryGraphNode node in graph)
            {
                foreach (Boundary bound in boundaries)
                {
                    // Ignore its parent boundaries
                    if (node.IsParent(bound))
                        continue;
                    // If position is interior, remove this node
                    if (bound.Interior(node.position))
                    {
                        deleteList.Add(node);
                        break;
                    }
                }
            }

            // Cleanup deleted nodes
            foreach (BoundaryGraphNode node in deleteList)
                graph.DeleteNode(node);

            graph._Visualize();

            // Now use our graph to build a series of related polygons
            // The most exterior polygon (outter loop of verticies)
            Polygon outtermost = null;
            Polygon last = null;

            // NOTE: work still needed. Needs to handle multiple islands and error cases.
            // right now it only works for the trivial case

            // While the graph has nodes remaining
            while (!graph.IsEmpty())
            {
                // Pick an extrema node. This will belong to the exterior most cycle in the graph
                BoundaryGraphNode extrema = graph.GetExtrema();

                // Clean up any exterior - exterior edges that could connect two seperate layers
                // These are left over after deleting the interior nodes
                // To do this, we track whether we are currently in an exterior

                // Remove that node and all nodes connected. This will give you the loop.
                List<BoundaryGraphNode> loop = graph.RemoveConnectedComponent(extrema);

                // Convert loop into a list of vector2
                List<Vector2> vertexList = new List<Vector2>();
                foreach (BoundaryGraphNode node in loop)
                {
                    vertexList.Add(node.position);
                }

                // Make a polygon from the loop
                Polygon poly = new Polygon(vertexList);

                // if this is the first run, set it as outtermost and dont add a parent
                // otherwise add the previous as its parent
                if (last == null)
                {
                    last = poly;
                    outtermost = poly;
                }
                else
                {
                    poly.SetParent(last);
                    last = poly;
                }
            }

            // Use our data to create a geometry class and store our results
            LevelGeometry lg = gameObject.GetComponent<LevelGeometry>();
            if (lg == null)
                lg = Undo.AddComponent<LevelGeometry>(gameObject);
            lg.Exterior = outtermost;
            return;
        }

        #region Editor
        // Method for the menu item to create a level
        [MenuItem("GameObject/GRTK/Level", false, 12)]
        static void SpawnLevelObject(MenuCommand menuCommand)
        {
            // Create a new GameObject with a Level attached
            GameObject newLevel = new GameObject("Level");
            newLevel.AddComponent<Level>();

            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(newLevel, menuCommand.context as GameObject);

            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(newLevel, "Create " + newLevel.name);

            // Set the selected object to be the level you just created
            Selection.activeObject = newLevel;
        }
        #endregion
    }
}
