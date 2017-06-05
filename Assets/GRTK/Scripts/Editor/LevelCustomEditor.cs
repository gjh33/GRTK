using UnityEngine;
using UnityEditor;
using System.Collections;

namespace GRTK
{
    [CustomEditor(typeof(Level))]
    public class LevelCustomEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // INITIALIZATION
            Level level = target as Level;

            // DRAW THE UI
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bool addButtonResult = GUILayout.Button("Add Boundary", GUILayout.Width(300), GUILayout.Height(20));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            bool buildButtonResult = GUILayout.Button("Build", GUILayout.Width(300), GUILayout.Height(30));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // HANDLE THE INPUT
            if (buildButtonResult)
            {
                level.CompileLevel();
            }
            else if (addButtonResult)
            {
                // Create a new GameObject with a Level attached
                GameObject newBoundary = GameObject.CreatePrimitive(PrimitiveType.Cube);
                newBoundary.name = "Boundary";
                newBoundary.AddComponent<Boundary>();

                // Ensure it gets reparented if this was a context click (otherwise does nothing)
                GameObjectUtility.SetParentAndAlign(newBoundary, ((Level)target).gameObject);

                // Register the creation in the undo system
                Undo.RegisterCreatedObjectUndo(newBoundary, "Create " + newBoundary.name);
            }
        }
    }
}
