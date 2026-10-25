# PCLPLUSS 修改说明

本项目基于 **PCL CE 2.15.1**（提交 `622e0f9`）二次开发。以下为相对上游的全部改动。

## 1. 品牌化

| 文件 | 改动 |
|---|---|
| `Plain Craft Launcher 2/metadata.json` | 名称 `PCLPLUSS`、版本 `3.0.0`、内部代号 `1000` |
| `Plain Craft Launcher 2/Plain Craft Launcher 2.csproj` | AssemblyTitle / Product / Description / Copyright / 版本号 |
| `Plain Craft Launcher 2/FormMain.xaml` | 窗口标题 `PCLPLUSS` |
| `Plain Craft Launcher 2/Modules/Base/ModSetup.cs` | 顶栏默认显示 `PCLPLUSS` 文本 Logo，隐藏 PCL 矢量 Logo 与 CE 角标 |
| `PCL.Core/App/Localization/Languages/*.xaml` | 界面文案 `PCL CE` → `PCLPLUSS`（保留关于页原作者署名） |
| `Plain Craft Launcher 2/Images/icon.ico`、`Images/Heads/Logo-CE.png` | 重新绘制程序图标与启动 Logo |
| `Plain Craft Launcher 2/Modules/Updates/UpdateManager.cs` | 停用 PCL CE 专属「版本说明」弹窗 |
| `Plain Craft Launcher 2/Pages/PageLaunch/PageLaunchRight.xaml.cs` | 移除「PCL CE 社区版提示」面板 |

## 2. 功能限制移除

| 文件 | 改动 |
|---|---|
| `Plain Craft Launcher 2/Modules/Minecraft/RestrictedFeaturePolicy.cs` | 区域功能限制改为恒放行，恢复下载游戏本体 / 安装加载器 / 整合包 / 修复 / 重置 |
| `Plain Craft Launcher 2/Modules/Minecraft/ProfileUi.cs` | 允许直接创建离线 / 第三方档案；未配置微软 Client ID 时正版档案自动降级为离线验证；新建档案不再提供「正版验证」选项 |
| `Plain Craft Launcher 2/Pages/PageLaunch/PageLoginProfile.xaml.cs` | 隐藏「需要先进行一次正版验证」提示 |
| `Plain Craft Launcher 2/Modules/Minecraft/ModLaunch.cs` | 取消启动前的正版购买 / 演示模式弹窗 |
| `PCL.Core/App/Tools/DependencyCheckService.cs` | 跳过 WebP 组件包缺失检查，避免弹窗打断启动 |

## 3. 稳定性 / 便携性

| 文件 | 改动 |
|---|---|
| `PCL.Core/App/Essentials/SingleInstanceService.cs` | 单例锁创建失败时记录真实异常并降级为允许多开继续启动（修复「双击完全没反应」） |
| `PCL.Core/App/Paths.cs` | 用户目录 / 临时目录不可写时自动回退到程序目录（便携模式） |
| `Plain Craft Launcher 2/Modules/Minecraft/ModFolder.cs` | 无效文件夹不再弹窗打断启动；无可用文件夹时自动在程序目录创建 `.minecraft` |
| `PCL.Core/App/Secrets.cs` | 微软 Client ID 支持运行时覆盖：构建注入 > 环境变量 `PCLPLUSS_MS_CLIENT_ID` > `PCL\ms_client_id.txt` |
| `PCL.Core/Minecraft/Profile/Authentication/Utils/MojangUtils.cs` | 登录接口失败时输出完整响应体，便于定位 403 等原因 |

## 4. 整合包安装加速

| 文件 | 改动 |
|---|---|
| `Plain Craft Launcher 2/Modules/Base/ModBase.cs` | ① `CopyDirectory` 并行复制；② 新增 `CopyFileOrHardlink`（`CreateHardLinkW` 优先，失败回落复制）；③ `ExtractFile` 多线程并行解压（每线程独立 ZipArchive 分片，重复条目冲突重试） |
| `Plain Craft Launcher 2/Modules/Minecraft/ModModpack.cs` | 整合包覆写目录改为硬链接复用（同卷零拷贝，跨卷自动回落复制） |

## 5. 未改动

- 上游全部第三方库、资源文件、CI 流程保持原样。
- 关于页 / NOTICE 中保留 PCL Community 与 龙腾猫跃 的版权信息。

## v3.1.0（基岩版支持）

新增文件：

| 文件 | 说明 |
|---|---|
| `Plain Craft Launcher 2/Modules/Minecraft/ModBedrock.cs` | 基岩版检测与启动：通过 `Get-AppxPackage` 读取 AppX 包信息（名称 / 版本 / 架构 / 安装路径 / PackageFamilyName），失败时回退扫描 `%LOCALAPPDATA%\Packages`；通过 `shell:appsFolder\<AUMID>` 启动，失败回退 `minecraft://` 协议 |
| `Plain Craft Launcher 2/Pages/PageTools/PageToolsBedrock.xaml(.cs)` | 新页面「工具 → 基岩版」：版本列表（UWP/GDK 区分）、启动、打开安装目录、重新扫描、打开 Microsoft Store |

修改文件：

| 文件 | 改动 |
|---|---|
| `FormMain.xaml.cs` | 新增子页面枚举 `ToolsBedrock = 5` |
| `Modules/ModMain.cs` | 新增 `frmToolsBedrock` 页面引用 |
| `Pages/PageTools/PageToolsLeft.xaml(.cs)` | 工具页侧边栏新增「基岩版」入口与页面路由 |
| `metadata.json` / `csproj` | 版本号提升到 3.1.0（内部代号 1010） |

修复：

- `解除锁定.bat` 之前使用 LF 换行导致 cmd 解析失败，已改为 CRLF + GBK 编码，并加 `chcp 936`。

说明：

- v3.1 只做「启动已安装的基岩版」，不涉及 Xbox 登录与安装包导入（后续 v3.2 / v3.3）。
