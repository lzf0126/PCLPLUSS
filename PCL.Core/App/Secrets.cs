using System;
using System.IO;
using PCL.Core.Utils.Exts;
using PCL.Core.Utils.OS;

namespace PCL.Core.App;

// ReSharper disable InconsistentNaming
public static class Secrets
{
    /// <summary>
    ///     微软 OAuth 的 Client ID。<br />
    ///     PCLPLUSS：除构建时注入外，还支持运行时覆盖，方便自行提供 Client ID 而无需重新编译。
    ///     优先级：构建注入 &gt; 环境变量 PCLPLUSS_MS_CLIENT_ID &gt; 文件 PCL\ms_client_id.txt
    /// </summary>
    public static string MSOAuthClientId { get; } = ResolveMsOAuthClientId();

    private static string ResolveMsOAuthClientId()
    {
        // 1. 构建时注入（PCL_WRITE_SECRET + PCL_MS_CLIENT_ID）
        var embedded = EnvironmentInterop.GetSecret("MS_CLIENT_ID", readEnvDebugOnly: true).ReplaceNullOrEmpty();
        if (!string.IsNullOrWhiteSpace(embedded)) return embedded.Trim();
        // 2. 运行时环境变量
        try
        {
            var fromEnv = Environment.GetEnvironmentVariable("PCLPLUSS_MS_CLIENT_ID");
            if (!string.IsNullOrWhiteSpace(fromEnv)) return fromEnv.Trim();
        }
        catch
        {
            // 忽略
        }

        // 3. 启动器目录下的配置文件 PCL\ms_client_id.txt
        try
        {
            var path = Path.Combine(Paths.Data, "ms_client_id.txt");
            if (File.Exists(path))
            {
                var text = File.ReadAllText(path).Trim();
                if (!string.IsNullOrWhiteSpace(text)) return text;
            }
        }
        catch
        {
            // 忽略
        }

        return string.Empty;
    }

    /// <summary>
    ///     CurseForge API 的 Client ID
    /// </summary>
    public static string CurseForgeAPIKey { get; } = EnvironmentInterop.GetSecret("CURSEFORGE_API_KEY", readEnvDebugOnly: true).ReplaceNullOrEmpty();

    /// <summary>
    ///     遥测密钥
    /// </summary>
    public static string TelemetryKey { get; } = EnvironmentInterop.GetSecret("TELEMETRY_KEY", readEnvDebugOnly: true).ReplaceNullOrEmpty();

    /// <summary>
    ///     Natayark ID OAuth 的 Client ID
    /// </summary>
    public static string NatayarkClientId { get; } = EnvironmentInterop.GetSecret("NAID_CLIENT_ID", readEnvDebugOnly: true).ReplaceNullOrEmpty();

    /// <summary>
    ///     Natayark ID OAuth 的 Client Secret
    /// </summary>
    public static string NatayarkClientSecret { get; } = EnvironmentInterop.GetSecret("NAID_CLIENT_SECRET", readEnvDebugOnly: true).ReplaceNullOrEmpty();

    /// <summary>
    ///     联机根服务器
    /// </summary>
    public static string[] LinkServers { get; } = EnvironmentInterop.GetSecret("LINK_SERVER_ROOT", readEnvDebugOnly: true).ReplaceNullOrEmpty().Split("|");

    /// <summary>
    ///     当前版本的 Git 提交 SHA
    /// </summary>
    public static string CommitHash { get; } = EnvironmentInterop.GetSecret("GITHUB_SHA", readEnvDebugOnly: true).ReplaceNullOrEmpty();
}
