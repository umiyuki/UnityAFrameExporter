using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(AFrameExporter))]
public class AFrameExporterEditor : Editor {

    public override void OnInspectorGUI()
    {

        AFrameExporter myExporter = (AFrameExporter)target;
        if (GUILayout.Button("Export"))
        {
            myExporter.Export();
        }

        if (GUILayout.Button("Run"))
        {
            myExporter.RunAFrame();
        }

        DrawDefaultInspector();

        if (GUILayout.Button("Clear Exported Files"))
        {
            myExporter.CrearExport();
        }
    }
}
