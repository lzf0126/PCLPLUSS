# PCLPLUSS

PCLPLUSS 是 [PCL CE](https://github.com/PCL-Community/PCL-CE)（[PCL2](https://github.com/Meloong-Git/PCL) 的社区分支）的二次开发版本 —— 一个面向 Minecraft: Java 版的 Windows 启动器。

> 本项目遵循 **Apache License 2.0**，保留上游全部版权声明。详细改动见 [CHANGES.md](CHANGES.md)。

## 特性

- **完整下载能力**：游戏本体、Forge / NeoForge / Fabric / Quilt / OptiFine、MOD、整合包，官方源与 BMCLAPI 镜像可在设置中切换。
- **离线优先**：无需先完成一次正版登录即可创建离线 / 第三方档案。
- **整合包极速安装**：并行解压（每个工作线程独立 ZipArchive 分片）+ 覆写目录硬链接复用，同卷零拷贝。
- **便携模式**：当用户目录 / 临时目录不可写时（常见于安全软件拦截），自动把配置与数据回退到程序目录；没有可用游戏文件夹时自动创建 `.minecraft`。
- **启动容错**：单例锁无法获取时降级为允许多开继续启动，而不是静默退出。
- **可配置微软登录**：支持在运行时提供自己的 Azure 应用 Client ID，无需重新编译。
- **🆕 基岩版支持（v3.1）**：「工具 → 基岩版」自动检测本机已安装的 Minecraft 基岩版（UWP 旧版 / GDK 新版），显示版本、架构、安装路径，一键启动或打开安装目录，无需先登录微软账号。

## 运行

发布包为**自包含版本**，无需安装 .NET 运行时：解压后双击 `PCLPLUSS.exe` 即可。

### 首次运行：Windows SmartScreen 提示

从网络下载的压缩包会带有「Mark-of-the-Web」标记，解压后运行未签名的 exe 时，Windows 可能提示：

> Windows 已保护你的电脑
> Microsoft Defender SmartScreen 阻止了无法识别的应用启动
> 发布者：发布者未知

这是**未做代码签名的正常现象**，不是病毒报告（本项目开源，可自行审计全部源码）。任选一种方式处理：

1. 点弹窗里的 **「更多信息」→「仍要运行」**；
2. **推荐**：右键下载的 zip 文件 → **属性** → 勾选 **「解除锁定」** → 确定，然后再解压，解压出的 exe 便不会触发该提示；
3. 或在解压后的文件夹里双击 **`解除锁定.bat`**，一键清除整个文件夹的锁定标记。

手动处理（PowerShell，在程序目录执行）：

```powershell
Get-ChildItem -Recurse | Unblock-File
```

## 从源码构建

环境要求：

- Windows 10 / 11 x64
- **.NET SDK 10.0.402 或更高**（低版本 SDK 的 Roslyn 5.0 无法加载本仓库的源生成器，会报大量 CS9248）
- 可选：Visual Studio 2022 17.14+ / JetBrains Rider

```powershell
# 便携（自包含，推荐）
dotnet publish "Plain Craft Launcher 2/Plain Craft Launcher 2.csproj" `
  -p:Configuration=Release -p:Platform=x64 -p:DeleteExistingFiles=true `
  -p:PublishSingleFile=false --self-contained true `
  -o ./artifact

# 单文件（需要可写的 %TEMP% 用于解压内嵌运行时）
dotnet publish "Plain Craft Launcher 2/Plain Craft Launcher 2.csproj" `
  -p:Configuration=Release -p:Platform=x64 `
  -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true `
  --self-contained true -o ./artifact
```

构建产物在 `artifact/`，主程序为 `Plain Craft Launcher 2.exe`。

### 注入构建期密钥（可选）

若需把微软 OAuth Client ID、CurseForge API Key 等编进程序，构建前设置环境变量：

```powershell
$env:PCL_WRITE_SECRET = "1"
$env:PCL_MS_CLIENT_ID = "<你的 Azure 应用 Client ID>"
$env:PCL_CURSEFORGE_API_KEY = "<CurseForge Key>"
```

## 微软正版登录说明

本项目**不内置**微软 OAuth Client ID。要使用正版登录，需要：

1. 在 [Azure Portal](https://portal.azure.com) 注册应用：
   - 受支持的账户类型：**个人 Microsoft 账户**（或「任何组织目录中的账户和个人 Microsoft 账户」）
   - 身份验证 → **允许公共客户端流 = 是**
   - 复制「应用程序(客户端) ID」
2. 把 Client ID 写入 `<程序目录>\PCL\ms_client_id.txt`（或设置环境变量 `PCLPLUSS_MS_CLIENT_ID`，或在构建时注入）。
3. ⚠️ **注意**：Minecraft 服务要求第三方 Client ID 先提交 [Mojang 官方审批](https://help.minecraft.net/hc/en-us/articles/16254801392141) 加入白名单，否则 `login_with_xbox` 会返回 `403 Invalid app registration`。这是服务端限制，与本项目代码无关。

## 目录结构

```
Plain Craft Launcher 2/    启动器主程序（WPF, net10.0-windows）
PCL.Core/                  核心库（网络、下载、Minecraft、身份认证等）
PCL.Core.SourceGenerators/ 配置 / 生命周期 / 环境变量的源生成器
PCL.Core.Test/             单元测试
tools/                     辅助脚本
```

## 许可与致谢

- 本项目基于 **PCL CE**（Apache-2.0），版权归 PCL Community 所有。
- PCL CE 基于 **Plain Craft Launcher 2**，原作者 **龙腾猫跃**。
- 上游及第三方组件的完整版权与许可信息见 [NOTICE](NOTICE) 与程序内「设置 → 关于」页面。
- 使用、修改、分发请遵守 Apache License 2.0，并保留上述版权声明。

## 免责声明

- 本项目**不附带、不分发任何 Minecraft 游戏文件**，所有下载均由使用者自行发起。
- Minecraft 是 Mojang Studios / Microsoft 的商标，本项目与其无任何隶属或官方关联。
- 使用本项目产生的任何风险与合规责任由使用者自行承担。
