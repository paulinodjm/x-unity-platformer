using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CameraController))]
public class CameraControllerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var cameraCtrl = (CameraController)target;

        EditorGUILayout.Space();
        if (GUILayout.Button("Center Camera"))
        {
            cameraCtrl.Start();
            cameraCtrl.LateUpdate();
        }
    }
}
