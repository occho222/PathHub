using System.Reflection;

namespace PathHub.Utils
{
    public static class VersionHelper
    {
        /// <summary>
        /// アプリケーションのバージョンを取得します
        /// </summary>
        public static string GetVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "Unknown";
        }

        /// <summary>
        /// 表示用のバージョン文字列を取得します（"v" プレフィックス付き）
        /// </summary>
        public static string GetDisplayVersion()
        {
            return $"v{GetVersion()}";
        }
    }
}