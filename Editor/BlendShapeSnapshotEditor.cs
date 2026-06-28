using System;
using UnityEditor;
using UnityEngine;

namespace Hrpnx.BlendShapeSnapshot
{
    [CustomEditor(typeof(BlendShapeSnapshot))]
    public class BlendShapeSnapshotEditor : Editor
    {
        private const float ButtonWidth = 24f;
        private const float SaveButtonWidth = 60f;

        private string _inputName = string.Empty;

        public override void OnInspectorGUI()
        {
            var component = (BlendShapeSnapshot)target;
            var smr = component.GetComponent<SkinnedMeshRenderer>();
            var mesh = smr != null ? smr.sharedMesh : null;

            DrawSaveRow(component, smr, mesh);

            if (mesh == null)
            {
                EditorGUILayout.HelpBox(
                    "SkinnedMeshRenderer に Mesh が設定されていません。",
                    MessageType.Warning
                );
            }

            EditorGUILayout.Space();
            DrawHistory(component, smr, mesh);
        }

        private void DrawSaveRow(BlendShapeSnapshot component, SkinnedMeshRenderer smr, Mesh mesh)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                _inputName = EditorGUILayout.TextField(_inputName);
                using (new EditorGUI.DisabledScope(mesh == null))
                {
                    if (GUILayout.Button("保存", GUILayout.Width(SaveButtonWidth)))
                    {
                        Save(component, smr);
                    }
                }
            }
        }

        private void DrawHistory(BlendShapeSnapshot component, SkinnedMeshRenderer smr, Mesh mesh)
        {
            var snapshots = component.Snapshots;
            int deleteIndex = -1;

            for (int i = 0; i < snapshots.Count; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(mesh == null))
                    {
                        if (GUILayout.Button("↑", GUILayout.Width(ButtonWidth)))
                        {
                            Restore(smr, snapshots[i]);
                        }
                    }

                    if (GUILayout.Button("✕", GUILayout.Width(ButtonWidth)))
                    {
                        deleteIndex = i;
                    }

                    EditorGUILayout.LabelField(snapshots[i].Name);
                }
            }

            if (deleteIndex >= 0)
            {
                Delete(component, deleteIndex);
            }
        }

        private void Save(BlendShapeSnapshot component, SkinnedMeshRenderer smr)
        {
            string name = SnapshotName.Resolve(_inputName, DateTime.Now);
            var snapshot = BlendShapeSnapshotIO.Capture(smr, name);

            Undo.RecordObject(component, "Save BlendShape Snapshot");
            component.Snapshots.Insert(0, snapshot);
            EditorUtility.SetDirty(component);

            _inputName = string.Empty;
            GUI.FocusControl(null);
        }

        private static void Restore(SkinnedMeshRenderer smr, BlendShapeSnapshot.Snapshot snapshot)
        {
            Undo.RecordObject(smr, "Restore BlendShape Snapshot");
            int applied = BlendShapeSnapshotIO.Apply(smr, snapshot);
            EditorUtility.SetDirty(smr);

            int missing = snapshot.Values.Count - applied;
            if (missing > 0)
            {
                Debug.LogWarning(
                    $"[BlendShapeSnapshot] {missing} 件の BlendShape が現在のメッシュに存在せずスキップしました。"
                );
            }
        }

        private static void Delete(BlendShapeSnapshot component, int index)
        {
            Undo.RecordObject(component, "Delete BlendShape Snapshot");
            component.Snapshots.RemoveAt(index);
            EditorUtility.SetDirty(component);
        }
    }
}
