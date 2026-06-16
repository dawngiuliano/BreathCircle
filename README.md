# BreathCircle · 呼吸圆圈

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/UI-WPF-blue.svg)](https://github.com/dotnet/wpf)

一款极简的 **Windows 桌面呼吸训练工具**。屏幕显示一个玻璃质感彩色圆形，通过缩放动画引导你完成各种呼吸练习。无需安装，双击即用。

<p align="center">
  <img src="BreathCircle/Assets/BreathCircle.ico" width="64" height="64">
</p>

## 快速开始

```bash
# 克隆仓库
git clone https://github.com/你的用户名/BreathCircle.git
cd BreathCircle

# 直接运行（需要 .NET 8 SDK）
dotnet run

# 发布为独立 .exe（无需 .NET 运行时）
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:SelfContained=true
```

发布后把 `publish` 文件夹打包 zip 发给别人，解压双击 `BreathCircle.exe` 即可。

## 操作方式

| 操作 | 功能 |
|------|------|
| 🖱️ **单击圆形** | 开始 → 暂停 → 继续 |
| 🖱️ **拖拽圆形** | 移动窗口位置 |
| 🖱️ **右键圆形** | 打开功能菜单 |
| ⌨️ `Ctrl+Alt+R` | 开始 / 暂停 |
| ⌨️ `Ctrl+Alt+N` | 切换下一个模式 |
| ⌨️ `Ctrl+Alt+W` | Wim Hof 模式 |
| ⌨️ `Ctrl+Alt+H` | 显示 / 隐藏窗口 |
| ⌨️ `Ctrl+Alt+Q` | 退出 |

## 呼吸模式

| 模式 | 节奏 | 说明 |
|------|------|------|
| 🟦 **盒式呼吸** | 吸4s → 屏4s → 呼4s → 屏4s | 经典四方呼吸 |
| 🟪 **4-7-8 呼吸** | 吸4s → 屏7s → 呼8s | 助眠放松 |
| 🟩 **共鸣呼吸** | 吸5s → 呼5s | 心率平衡 |
| 🟧 **生理性叹息** | 吸2s → 屏2s → 呼8s | 快速减压 |
| 🟥 **Wim Hof** | 30次快呼吸 → 屏息 → 恢复吐气 | 进阶练习 |

每种模式都有独立配色，可在颜色设置中自定义。

## 功能亮点

- **玻璃拟态 UI** — 径向渐变圆形 + 圆环描边 + 发光辉光
- **自定义配色** — RGB 滑块为每种模式独立调色
- **Wim Hof 可调参数** — 轮次、呼吸频率、屏息时长、递增步长
- **精确计时** — 基于 Stopwatch 真实时间，不受帧率影响
- **暂停 / 继续** — 不丢失当前进度
- **系统托盘** — 最小化到托盘，自定义图标
- **全局热键** — 不打断工作流
- **点击穿透** — 圆形可设为鼠标穿透
- **显示选项** — 阶段文字与倒计时独立开关
- **安全提示** — Wim Hof 首次使用显示警告

## 项目架构

```
BreathCircle/
├── Models/          # 数据模型（呼吸模式、阶段、设置）
├── StateMachine/    # 状态模式（吸气/呼气/屏息/空闲…）
├── Services/        # 核心服务（呼吸引擎、动画、热键、托盘）
├── ViewModels/      # MVVM ViewModel
├── Views/Dialogs/   # 设置对话框（颜色、Wim Hof 参数、安全提示）
├── Helpers/         # 工具类（缓动函数、颜色主题、Win32 API）
└── Assets/          # 应用图标
```

- **框架**: WPF + .NET 8 + CommunityToolkit.Mvvm
- **动画**: CompositionTarget.Rendering (60FPS) + EaseInOutSine 缓动
- **计时**: Stopwatch 唯一时间源（无帧率漂移）
- **状态**: State Pattern 管理呼吸阶段

## 系统要求

- Windows 10 / 11 x64
- .NET 8.0（开发时）
- 单文件发布后无需任何依赖

## 开源协议

[MIT](LICENSE)
