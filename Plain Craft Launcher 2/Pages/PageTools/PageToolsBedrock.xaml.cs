using System;
using System.Windows;
using System.Windows.Input;
using PCL.Core.App.Localization;
using PCL.Core.UI;

namespace PCL;

public partial class PageToolsBedrock
{
    private bool _loaded;

    public PageToolsBedrock()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (_loaded) return;
            _loaded = true;
            RefreshList();
        };
    }

    private void RefreshList()
    {
        PanList.Children.Clear();
        var list = ModBedrock.Scan();
        LabEmpty.Visibility = list.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        foreach (var install in list)
        {
            var item = new MyListItem
            {
                Title = install.DisplayName,
                Info = $"{install.Version} · {install.Architecture} · {install.TypeText}",
                Type = MyListItem.CheckType.Clickable,
                SvgIcon = "lucide/boxes",
                Tag = install
            };
            var btnLaunch = new MyIconButton
            {
                SvgIcon = "lucide/play",
                ToolTip = "启动",
                Tag = install,
                LogoScale = 0.9
            };
            btnLaunch.Click += (_, _) => Launch(install);
            var btnFolder = new MyIconButton
            {
                SvgIcon = "lucide/folder-open",
                ToolTip = "打开安装目录",
                Tag = install,
                LogoScale = 0.9
            };
            btnFolder.Click += (_, _) =>
            {
                if (!ModBedrock.OpenFolder(install))
                    HintService.Hint("无法打开安装目录（WindowsApps 目录受系统保护）。", HintType.Error);
            };
            item.Buttons = [btnLaunch, btnFolder];
            PanList.Children.Add(item);
        }
    }

    private static void Launch(ModBedrock.BedrockInstall install)
    {
        if (ModBedrock.Launch(install))
            HintService.Hint($"正在启动 {install.DisplayName}……", HintType.Success);
        else
            HintService.Hint("启动失败，请确认基岩版仍处于已安装状态。", HintType.Error);
    }

    private void BtnRefresh_Click(object sender, MouseButtonEventArgs e) => RefreshList();

    private void BtnStore_Click(object sender, MouseButtonEventArgs e) => ModBedrock.OpenStore();
}
