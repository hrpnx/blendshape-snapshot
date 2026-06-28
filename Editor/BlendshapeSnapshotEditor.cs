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
        private const string RenameControlName = "bss_rename_field";

        private string _inputName = string.Empty;

        // インラインリネームの状態。_editingIndex < 0 なら非編集。
        private int _editingIndex = -1;
        private string _editingName = string.Empty;
        private bool _focusPending;
        private bool _focusEstablished;

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
                    if (GUILayout.Button("Save", GUILayout.Width(SaveButtonWidth)))
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
                    if (i == _editingIndex)
                    {
                        DrawEditRow(component, i);
                    }
                    else
                    {
                        DrawNormalRow(component, smr, mesh, i);
                    }
                }
            }
        }

        private void DrawNormalRow(BlendshapeSnapshot component, SkinnedMeshRenderer smr, Mesh mesh, int index)
        {
            // 名前を左に置いて伸縮させ、操作ボタンを右寄せにする。
            EditorGUILayout.LabelField(component.Snapshots[index].Name);

            using (new EditorGUI.DisabledScope(mesh == null))
            {
                if (GUILayout.Button("Apply", GUILayout.Width(ApplyButtonWidth)))
                {
                    Restore(smr, component.Snapshots[index]);
                }
            }

            // クリック(used)時に GetLastRect は確定していないため、矩形を明示確保してから描く。
            var menuContent = new GUIContent("︙");
            var menuRect = GUILayoutUtility.GetRect(
                menuContent,
                GUI.skin.button,
                GUILayout.Width(MenuButtonWidth)
            );
            if (GUI.Button(menuRect, menuContent))
            {
                ShowRowMenu(component, index, menuRect);
            }
        }

        private void DrawEditRow(BlendshapeSnapshot component, int index)
        {
            GUI.SetNextControlName(RenameControlName);
            _editingName = EditorGUILayout.TextField(_editingName);

            // 編集開始フレーム: フォーカスを当て、全選択する。フォーカスが効いた次フレームで確定。
            if (_focusPending)
            {
                EditorGUI.FocusTextInControl(RenameControlName);
                if (GUI.GetNameOfFocusedControl() == RenameControlName
                    && Event.current.type == EventType.Repaint)
                {
                    var editor = (TextEditor)GUIUtility.GetStateObject(
                        typeof(TextEditor),
                        GUIUtility.keyboardControl
                    );
                    editor?.SelectAll();
                    _focusPending = false;
                    _focusEstablished = true;
                }
            }

            bool commit = false;
            bool cancel = false;

            // Enter=確定 / Esc=取消
            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.Return
                    || Event.current.keyCode == KeyCode.KeypadEnter)
                {
                    commit = true;
                    Event.current.Use();
                }
                else if (Event.current.keyCode == KeyCode.Escape)
                {
                    cancel = true;
                    Event.current.Use();
                }
            }

            if (GUILayout.Button("Save", GUILayout.Width(SaveButtonWidth)))
            {
                commit = true;
            }

            // フォーカスが確立した後にフィールドから外れたら確定する。
            if (!commit
                && !cancel
                && _focusEstablished
                && Event.current.type == EventType.Layout
                && GUI.GetNameOfFocusedControl() != RenameControlName)
            {
                commit = true;
            }

            if (commit)
            {
                CommitEdit(component);
            }
            else if (cancel)
            {
                CancelEdit();
            }
        }

        private void ShowRowMenu(BlendshapeSnapshot component, int index, Rect anchor)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Rename"), false, () => BeginRename(component, index));
            menu.AddItem(new GUIContent("Delete"), false, () => ConfirmDelete(component, index));
            menu.DropDown(anchor);
        }

        private void BeginRename(BlendshapeSnapshot component, int index)
        {
            if (index < 0 || index >= component.Snapshots.Count)
            {
                return;
            }

            // 別の行を編集中なら、フォーカス喪失と同じく現編集を確定してから移る。
            if (_editingIndex >= 0 && _editingIndex != index)
            {
                CommitEdit(component);
            }

            _editingIndex = index;
            _editingName = component.Snapshots[index].Name;
            _focusPending = true;
            _focusEstablished = false;
        }

        private void CommitEdit(BlendshapeSnapshot component)
        {
            int index = _editingIndex;
            string newName = (_editingName ?? string.Empty).Trim();
            EndEdit();

            if (index < 0 || index >= component.Snapshots.Count)
            {
                return;
            }

            // 空はキャンセル扱い（元名維持）、変化なしも何もしない。
            if (string.IsNullOrEmpty(newName) || component.Snapshots[index].Name == newName)
            {
                return;
            }

            Undo.RecordObject(component, "Rename Blendshape Snapshot");
            component.Snapshots[index].Name = newName;
            EditorUtility.SetDirty(component);
        }

        private void CancelEdit()
        {
            EndEdit();
        }

        private void EndEdit()
        {
            _editingIndex = -1;
            _editingName = string.Empty;
            _focusPending = false;
            _focusEstablished = false;
            GUI.FocusControl(null);
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
