using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using PCL.Core.App.IoC;
using PCL.Core.IO;
using PCL.Core.Utils;

namespace PCL.Core.App.Essentials;

[LifecycleService(LifecycleState.BeforeLoading, Priority = -2134567890)]
[LifecycleScope("single-instance", "单例", false)]
public sealed partial class SingleInstanceService
{
    private static FileStream? _lockStream;
    private static readonly string _LockFilePath = Path.Combine(Paths.SharedLocalData, "instance.lock");

    private static void _TryRpc(string processId, string content)
    {
        var pipeName = $"{RpcService.PipePrefix}@{processId}";
        using var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
        pipe.Connect(1000);
        using var sw = new StreamWriter(pipe, PipeComm.PipeEncoding);
        sw.WriteLine(content);
        sw.Write(PipeComm.PipeEndingChar);
        sw.Flush();
    }

    [LifecycleStart]
    private static void _Start()
    {
        // PCLPLUSS：先确保数据目录存在，并记录创建锁失败的具体原因（原实现会静默吞掉）
        try
        {
            Directory.CreateDirectory(Paths.SharedLocalData);
            var stream = File.Open(_LockFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            Context.Debug("未发现重复实例，正在向单例锁写入信息");
            using var sw = new StreamWriter(stream, Encoding.ASCII, 8, true);
            sw.Write(Basics.CurrentProcessId);
            sw.Flush();
            _lockStream = stream;
            return;
        }
        catch (Exception createEx)
        {
            Context.Warn("创建单例锁失败，尝试按重复实例处理", createEx);
        }

        string? pid = null;
        try
        {
            using var stream = File.Open(_LockFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);
            pid = reader.ReadToEnd();
        }
        catch (Exception readEx)
        {
            // PCLPLUSS：既拿不到锁也读不到已有实例时，降级为允许多开继续启动，
            // 而不是直接退出导致「双击完全没反应」。
            Context.Error("读取单例锁出错，已跳过单实例检查并继续启动", readEx);
            return;
        }

        Context.Info($"发现重复实例 {pid}，尝试传递参数并拉起主窗口");
        try
        {
            _TryRpc(pid, "REQ cli\n" + JsonSerializer.Serialize(StartupService.UnhandledCommands, JsonCompat.SerializerOptions));
            _TryRpc(pid, "REQ activate");
        }
        catch (Exception ex)
        {
            Context.Warn("RPC 通信失败", ex);
        }

        Context.RequestExit(1);
    }

    [LifecycleStop]
    private static void _Stop()
    {
        if (_lockStream is null) return;
        Context.Debug("正在删除单例锁");
        _lockStream.Dispose();
        try
        {
            File.Delete(_LockFilePath);
        }
        catch (Exception ex)
        {
            Context.Warn("删除单例锁失败", ex);
        }
    }
}
