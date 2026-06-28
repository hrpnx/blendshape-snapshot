using System;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace Hrpnx.BlendshapeSnapshot
{
    /// <summary>
    /// 同一 GameObject の SkinnedMeshRenderer の BlendShape ウェイトを
    /// 名前付きスナップショットとして保存・復元するエディタ専用コンポーネント。
    /// </summary>
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public class BlendshapeSnapshot : MonoBehaviour, IEditorOnly
    {
        [SerializeField]
        private List<Snapshot> _snapshots = new();

        /// <summary>保存済みスナップショット。先頭 (index 0) が最新。</summary>
        public List<Snapshot> Snapshots => _snapshots;

        /// <summary>1 回分のスナップショット。全 BlendShape を名前→ウェイトで保持する。</summary>
        [Serializable]
        public class Snapshot
        {
            public string Name;
            public List<ShapeValue> Values = new();
        }

        /// <summary>BlendShape 1 個分の記録。インデックスではなく名前で持つことでメッシュ変更に強くする。</summary>
        [Serializable]
        public class ShapeValue
        {
            public string Name;
            public float Weight;
        }
    }
}
