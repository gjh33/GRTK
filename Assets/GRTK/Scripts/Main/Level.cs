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
