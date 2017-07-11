using UnityEngine;
using UnityEditor;
using System.Collections;

namespace GRTK
{
    [CustomEditor(typeof(LevelGeometry))]
    public class LevelGeometryEditor : Editor
    {
        private void OnSceneGUI()
        {
            LevelGeometry lg = target as LevelGeometry;
            //Handles.DrawAAPolyLine(3.0f, lg.Exterior.GetRaw3());
        }
    }
}

