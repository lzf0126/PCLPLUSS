using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;

namespace PCL;

/// <summary>
///     Minecraft 基岩版（Bedrock Edition）检测与启动。
///     PCLPLUSS v3.1：读取系统已安装的基岩版（UWP 旧版 / GDK 新版），一键启动、打开安装目录。
/// </summary>
public static class ModBedrock
{
    /// <summary>
    ///     一个已安装的基岩版条目。
    /// </summary>
    public class BedrockInstall
    {
        /// <summary>包名，例如 Microsoft.MinecraftUWP</summary>
        public string Name = "";

        /// <summary>PackageFullName</summary>
        public string FullName = "";

        /// <summary>PackageFamilyName，用于构造 AUMID</summary>
        public string FamilyName = "";

        /// <summary>版本号</summary>
        public string Version = "";

        /// <summary>架构</summary>
        public string Architecture = "";

        /// <summary>安装目录</summary>
        public string Location = "";

        /// <summary>是否为 2025 年后的 GDK 统一版本</summary>
        public bool IsGdk;

        /// <summary>应用用户模型 ID（AUMID）</summary>
        public string Aumid => string.IsNullOrWhiteSpace(FamilyName) ? "" : FamilyName + "!App";

        public string TypeText => IsGdk ? "GDK 新版" : "UWP 旧版";

        public string DisplayName => IsGdk ? "Minecraft 基岩版（GDK）" : "Minecraft 基岩版（UWP）";
    }

    private static readonly string[] BedrockPackageNames =
    [
        "Microsoft.MinecraftUWP",
        "Microsoft.MinecraftWindowsBeta",
        "Microsoft.Minecraft"
    ];

    /// <summary>
    ///     扫描本机已安装的基岩版。
    /// </summary>
    public static List<BedrockInstall> Scan()
    {
        var result = ScanByAppx();
        if (result.Count == 0)
            result = ScanByPackageFolder();
        return result
            .OrderBy(i => i.IsGdk)
            .ThenBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool IsBedrockPackage(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        return BedrockPackageNames.Any(n =>
            name.Equals(n, StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith(n + ".", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    ///     通过 Get-AppxPackage 查询（能拿到版本、架构、安装位置）。
    /// </summary>
    private static List<BedrockInstall> ScanByAppx()
    {
        var list = new List<BedrockInstall>();
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments =
                    "-NoProfile -ExecutionPolicy Bypass -Command \"Get-AppxPackage -Name '*Minecraft*' | " +
                    "Select-Object Name,Version,Architecture,PackageFullName,PackageFamilyName,InstallLocation | " +
                    "ConvertTo-Json -Compress\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };
            using var ps = Process.Start(psi);
            if (ps is null) return list;
            var json = ps.StandardOutput.ReadToEnd();
            ps.WaitForExit(20000);
            if (string.IsNullOrWhiteSpace(json)) return list;
            var node = JsonNode.Parse(json);
            IEnumerable<JsonNode?> items = node is JsonArray array ? array : new JsonArray(node);
            foreach (var item in items)
            {
                if (item is not JsonObject obj) continue;
                var name = obj["Name"]?.ToString() ?? "";
                if (!IsBedrockPackage(name)) continue;
                list.Add(new BedrockInstall
                {
                    Name = name,
                    FullName = obj["PackageFullName"]?.ToString() ?? "",
                    FamilyName = obj["PackageFamilyName"]?.ToString() ?? "",
                    Version = obj["Version"]?.ToString() ?? "",
                    Architecture = obj["Architecture"]?.ToString() ?? "",
                    Location = obj["InstallLocation"]?.ToString() ?? "",
                    IsGdk = name.Equals("Microsoft.Minecraft", StringComparison.OrdinalIgnoreCase)
                });
            }
        }
        catch (Exception ex)
        {
            ModBase.Log(ex, "[Bedrock] 通过 AppX 查询基岩版失败，将回退到目录扫描");
        }

        return list;
    }

    /// <summary>
    ///     回退方案：扫描 %LOCALAPPDATA%\Packages 目录（拿不到版本，但能识别出安装过）。
    /// </summary>
    private static List<BedrockInstall> ScanByPackageFolder()
    {
        var list = new List<BedrockInstall>();
        try
        {
            var packages = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages");
            if (!Directory.Exists(packages)) return list;
            foreach (var dir in Directory.GetDirectories(packages))
            {
                var folder = Path.GetFileName(dir);
                var index = folder.IndexOf('_');
                if (index <= 0) continue;
                var name = folder[..index];
                if (!IsBedrockPackage(name)) continue;
                list.Add(new BedrockInstall
                {
                    Name = name,
                    FamilyName = folder,
                    Version = "未知",
                    Architecture = "未知",
                    Location = dir,
                    IsGdk = name.Equals("Microsoft.Minecraft", StringComparison.OrdinalIgnoreCase)
                });
            }
        }
        catch (Exception ex)
        {
            ModBase.Log(ex, "[Bedrock] 扫描 Packages 目录失败");
        }

        return list;
    }

    /// <summary>
    ///     启动基岩版。
    /// </summary>
    public static bool Launch(BedrockInstall install)
    {
        if (install is null || string.IsNullOrWhiteSpace(install.Aumid))
            return false;
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = "shell:appsFolder\\" + install.Aumid,
                UseShellExecute = true
            });
            ModBase.Log("[Bedrock] 已请求启动：" + install.Aumid);
            return true;
        }
        catch (Exception ex)
        {
            ModBase.Log(ex, "[Bedrock] 通过 AUMID 启动失败，尝试 minecraft:// 协议");
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "minecraft://",
                    UseShellExecute = true
                });
                return true;
            }
            catch (Exception ex2)
            {
                ModBase.Log(ex2, "[Bedrock] 通过协议启动也失败");
                return false;
            }
        }
    }

    /// <summary>
    ///     打开安装目录。
    /// </summary>
    public static bool OpenFolder(BedrockInstall install)
    {
        if (install is null || string.IsNullOrWhiteSpace(install.Location))
            return false;
        try
        {
            var path = install.Location;
            // WindowsApps 目录受系统保护，打不开时尝试退到 Packages 目录
            if (!Directory.Exists(path))
                return false;
            ModBase.Log("[Bedrock] 打开安装目录：" + path);
            PCL.Core.App.Basics.OpenPath(path);
            return true;
        }
        catch (Exception ex)
        {
            ModBase.Log(ex, "[Bedrock] 打开安装目录失败");
            return false;
        }
    }

    /// <summary>
    ///     打开 Microsoft Store 的 Minecraft 页面。
    /// </summary>
    public static void OpenStore()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ms-windows-store://pdp/?productid=9NXP44L49SHJ",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            ModBase.Log(ex, "[Bedrock] 打开 Microsoft Store 失败");
            HintService.Hint("无法打开 Microsoft Store，请手动搜索 Minecraft。", HintType.Error);
        }
    }
}
