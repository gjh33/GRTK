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

        public void Start()
        {
            //Used for testing
        }

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
                        Debug.Log("Point " + node.position + " interior to " + bound.transform.position);                       
                    }
                }
            }

            Debug.Log(boundaries[1].Interior(new Vector2(-0.5f, 0.5f)));

            // Cleanup deleted nodes
            foreach (BoundaryGraphNode node in deleteList)
                graph.DeleteNode(node);

            //TEST VISUALIZATION
            foreach (BoundaryGraphNode node in graph)
            {
                GameObject rep = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rep.transform.position = node.position;
                rep.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                //Debug.Log("Position: " + node.position + " | neighbors: " + node.GetNeighbors().Count + " | parents: " + node.GetParentBoundaries()[0].GetInstanceID());
            }
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
