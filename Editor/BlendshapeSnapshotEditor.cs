using System;
using UnityEditor;
using UnityEngine;

namespace Hrpnx.BlendshapeSnapshot
{
    [CustomEditor(typeof(BlendshapeSnapshot))]
    public class BlendshapeSnapshotEditor : Editor
    {
        private const float ApplyButtonWidth = 60f;
        private const float MenuButtonWidth = 24f;
        private const float SaveButtonWidth = 60f;

        private string _inputName = string.Empty;

        public override void OnInspectorGUI()
        {
            var component = (BlendshapeSnapshot)target;
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

        private void DrawSaveRow(BlendshapeSnapshot component, SkinnedMeshRenderer smr, Mesh mesh)
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

        private void DrawHistory(BlendshapeSnapshot component, SkinnedMeshRenderer smr, Mesh mesh)
        {
            var snapshots = component.Snapshots;

            for (int i = 0; i < snapshots.Count; i++)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    // 名前を左に置いて伸縮させ、操作ボタンを右寄せにする。
                    EditorGUILayout.LabelField(snapshots[i].Name);

                    using (new EditorGUI.DisabledScope(mesh == null))
                    {
                        if (GUILayout.Button("Apply", GUILayout.Width(ApplyButtonWidth)))
                        {
                            Restore(smr, snapshots[i]);
                        }
                    }

                    if (GUILayout.Button("︙", GUILayout.Width(MenuButtonWidth)))
                    {
                        ShowRowMenu(component, i, GUILayoutUtility.GetLastRect());
                    }
                }
            }
        }

        private void ShowRowMenu(BlendshapeSnapshot component, int index, Rect anchor)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Delete"), false, () => ConfirmDelete(component, index));
            menu.DropDown(anchor);
        }

        private static void ConfirmDelete(BlendshapeSnapshot component, int index)
        {
            if (index < 0 || index >= component.Snapshots.Count)
            {
                return;
            }

            bool ok = EditorUtility.DisplayDialog(
                "スナップショットの削除",
                $"「{component.Snapshots[index].Name}」を削除しますか？",
                "削除",
                "キャンセル"
            );
            if (ok)
            {
                Delete(component, index);
            }
        }

        private void Save(BlendshapeSnapshot component, SkinnedMeshRenderer smr)
        {
            string name = SnapshotName.Resolve(_inputName, DateTime.Now);
            var snapshot = BlendshapeSnapshotIO.Capture(smr, name);

            Undo.RecordObject(component, "Save Blendshape Snapshot");
            component.Snapshots.Insert(0, snapshot);
            EditorUtility.SetDirty(component);

            _inputName = string.Empty;
            GUI.FocusControl(null);
        }

        private static void Restore(SkinnedMeshRenderer smr, BlendshapeSnapshot.Snapshot snapshot)
        {
            Undo.RecordObject(smr, "Restore Blendshape Snapshot");
            int applied = BlendshapeSnapshotIO.Apply(smr, snapshot);
            EditorUtility.SetDirty(smr);

            int missing = snapshot.Values.Count - applied;
            if (missing > 0)
            {
                Debug.LogWarning(
                    $"[BlendshapeSnapshot] {missing} 件の BlendShape が現在のメッシュに存在せずスキップしました。"
                );
            }
        }

        private static void Delete(BlendshapeSnapshot component, int index)
        {
            Undo.RecordObject(component, "Delete Blendshape Snapshot");
            component.Snapshots.RemoveAt(index);
            EditorUtility.SetDirty(component);
        }
    }
}
