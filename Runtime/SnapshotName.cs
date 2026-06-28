using System;

namespace Hrpnx.BlendShapeSnapshot
{
    /// <summary>スナップショット名の決定ロジック。未入力なら現在時刻を 24 時間表記で用いる。</summary>
    public static class SnapshotName
    {
        public const string TimestampFormat = "yyyy-MM-dd HH:mm:ss";

        public static string Resolve(string input, DateTime now) =>
            string.IsNullOrWhiteSpace(input) ? now.ToString(TimestampFormat) : input.Trim();
    }
}
