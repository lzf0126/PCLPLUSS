using PCL.Core.App.Region;
using PCL.Core.App.Localization;
using PCL.Core.UI;

namespace PCL;

/// <summary>
///     功能可用性策略。
///     PCLPLUSS 分支：移除原社区版针对特定区域的功能限制，
///     恢复与原版 PCL2 一致的下载、安装与整合包功能。
/// </summary>
internal static class RestrictedFeaturePolicy
{
    /// <summary>
    ///     当前分支始终允许全部功能，保留该方法仅为兼容原有调用点。
    /// </summary>
    internal static RegionAssessment Assess() => RegionPolicy.Evaluate(RegionPolicy.ReadWindowsSignals());

    /// <summary>
    ///     是否限制功能。PCLPLUSS 恒为 <c>false</c>。
    /// </summary>
    internal static bool IsRestricted => false;

    /// <summary>
    ///     检查功能是否可用。PCLPLUSS 恒返回 <c>true</c>。
    /// </summary>
    internal static bool Check(string feature, bool showHint = true) => true;

    /// <summary>
    ///     若功能受限则抛出异常。PCLPLUSS 不做任何处理。
    /// </summary>
    internal static void ThrowIfRestricted(string feature)
    {
    }
}
