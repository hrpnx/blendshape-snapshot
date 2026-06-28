using System;
using NUnit.Framework;

namespace Hrpnx.BlendShapeSnapshot.Tests
{
    public class SnapshotNameTests
    {
        [Test]
        public void Resolve_EmptyInput_ReturnsTimestamp()
        {
            var now = new DateTime(2026, 6, 28, 15, 4, 5);
            Assert.AreEqual("2026-06-28 15:04:05", SnapshotName.Resolve("", now));
        }

        [Test]
        public void Resolve_Whitespace_ReturnsTimestamp()
        {
            var now = new DateTime(2026, 1, 2, 3, 4, 5);
            Assert.AreEqual("2026-01-02 03:04:05", SnapshotName.Resolve("   ", now));
        }

        [Test]
        public void Resolve_AfternoonUses24Hour()
        {
            var now = new DateTime(2026, 12, 31, 23, 59, 59);
            Assert.AreEqual("2026-12-31 23:59:59", SnapshotName.Resolve(null, now));
        }

        [Test]
        public void Resolve_NonEmpty_ReturnsTrimmedInput()
        {
            var now = new DateTime(2026, 6, 28, 15, 4, 5);
            Assert.AreEqual("smile", SnapshotName.Resolve("  smile  ", now));
        }
    }
}
