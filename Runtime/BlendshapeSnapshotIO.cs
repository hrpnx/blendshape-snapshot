using UnityEngine;

namespace Hrpnx.BlendshapeSnapshot
{
    /// <summary>
    /// SkinnedMeshRenderer と Snapshot の間で BlendShape ウェイトを採取・適用する純粋ロジック。
    /// Undo やダーティ化は呼び出し側 (Editor) の責務とし、ここでは値の読み書きのみ行う。
    /// </summary>
    public static class BlendshapeSnapshotIO
    {
        /// <summary>SMR の全 BlendShape を名前→ウェイトで採取する。</summary>
        public static BlendshapeSnapshot.Snapshot Capture(SkinnedMeshRenderer smr, string name)
        {
            var snapshot = new BlendshapeSnapshot.Snapshot { Name = name };
            var mesh = smr != null ? smr.sharedMesh : null;
            if (mesh == null)
            {
                return snapshot;
            }

            int count = mesh.blendShapeCount;
            for (int i = 0; i < count; i++)
            {
                snapshot.Values.Add(
                    new BlendshapeSnapshot.ShapeValue
                    {
                        Name = mesh.GetBlendShapeName(i),
                        Weight = smr.GetBlendShapeWeight(i),
                    }
                );
            }

            return snapshot;
        }

        /// <summary>
        /// スナップショットの値を SMR に書き戻す。現在のメッシュに存在しない名前はスキップする。
        /// </summary>
        /// <returns>実際に適用できた BlendShape の件数。</returns>
        public static int Apply(SkinnedMeshRenderer smr, BlendshapeSnapshot.Snapshot snapshot)
        {
            var mesh = smr != null ? smr.sharedMesh : null;
            if (mesh == null || snapshot == null)
            {
                return 0;
            }

            int applied = 0;
            foreach (var value in snapshot.Values)
            {
                int index = mesh.GetBlendShapeIndex(value.Name);
                if (index < 0)
                {
                    continue;
                }

                smr.SetBlendShapeWeight(index, value.Weight);
                applied++;
            }

            return applied;
        }
    }
}
