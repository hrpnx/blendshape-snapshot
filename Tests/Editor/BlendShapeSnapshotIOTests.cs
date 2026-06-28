using NUnit.Framework;
using UnityEngine;

namespace Hrpnx.BlendShapeSnapshot.Tests
{
    public class BlendShapeSnapshotIOTests
    {
        private static Mesh CreateMesh(params string[] shapeNames)
        {
            var mesh = new Mesh
            {
                vertices = new[] { Vector3.zero, Vector3.up, Vector3.right },
            };
            var delta = new Vector3[3];
            foreach (var shapeName in shapeNames)
            {
                mesh.AddBlendShapeFrame(shapeName, 100f, delta, null, null);
            }

            return mesh;
        }

        private static SkinnedMeshRenderer CreateRenderer(Mesh mesh, out GameObject go)
        {
            go = new GameObject("smr-test");
            var smr = go.AddComponent<SkinnedMeshRenderer>();
            smr.sharedMesh = mesh;
            return smr;
        }

        [Test]
        public void Capture_RecordsAllBlendShapes()
        {
            var mesh = CreateMesh("a", "b", "c");
            var smr = CreateRenderer(mesh, out var go);
            try
            {
                smr.SetBlendShapeWeight(0, 10f);
                smr.SetBlendShapeWeight(1, 20f);
                smr.SetBlendShapeWeight(2, 30f);

                var snapshot = BlendShapeSnapshotIO.Capture(smr, "test");

                Assert.AreEqual("test", snapshot.Name);
                Assert.AreEqual(3, snapshot.Values.Count);
                Assert.AreEqual("b", snapshot.Values[1].Name);
                Assert.AreEqual(20f, snapshot.Values[1].Weight);
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void Apply_RestoresWeightsByName()
        {
            var mesh = CreateMesh("a", "b", "c");
            var smr = CreateRenderer(mesh, out var go);
            try
            {
                smr.SetBlendShapeWeight(0, 10f);
                smr.SetBlendShapeWeight(1, 20f);
                smr.SetBlendShapeWeight(2, 30f);
                var snapshot = BlendShapeSnapshotIO.Capture(smr, "test");

                smr.SetBlendShapeWeight(0, 0f);
                smr.SetBlendShapeWeight(1, 0f);
                smr.SetBlendShapeWeight(2, 0f);

                int applied = BlendShapeSnapshotIO.Apply(smr, snapshot);

                Assert.AreEqual(3, applied);
                Assert.AreEqual(10f, smr.GetBlendShapeWeight(0));
                Assert.AreEqual(20f, smr.GetBlendShapeWeight(1));
                Assert.AreEqual(30f, smr.GetBlendShapeWeight(2));
            }
            finally
            {
                Object.DestroyImmediate(go);
                Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void Apply_SkipsMissingShapeNames()
        {
            var sourceMesh = CreateMesh("a", "b", "c");
            var sourceSmr = CreateRenderer(sourceMesh, out var sourceGo);
            BlendShapeSnapshot.Snapshot snapshot;
            try
            {
                sourceSmr.SetBlendShapeWeight(0, 10f);
                sourceSmr.SetBlendShapeWeight(1, 20f);
                sourceSmr.SetBlendShapeWeight(2, 30f);
                snapshot = BlendShapeSnapshotIO.Capture(sourceSmr, "test");
            }
            finally
            {
                Object.DestroyImmediate(sourceGo);
                Object.DestroyImmediate(sourceMesh);
            }

            // "b" を欠いたメッシュへ適用する
            var targetMesh = CreateMesh("a", "c");
            var targetSmr = CreateRenderer(targetMesh, out var targetGo);
            try
            {
                int applied = BlendShapeSnapshotIO.Apply(targetSmr, snapshot);

                Assert.AreEqual(2, applied);
                Assert.AreEqual(
                    10f,
                    targetSmr.GetBlendShapeWeight(targetMesh.GetBlendShapeIndex("a"))
                );
                Assert.AreEqual(
                    30f,
                    targetSmr.GetBlendShapeWeight(targetMesh.GetBlendShapeIndex("c"))
                );
            }
            finally
            {
                Object.DestroyImmediate(targetGo);
                Object.DestroyImmediate(targetMesh);
            }
        }
    }
}
