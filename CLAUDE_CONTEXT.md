你现在接管一个正在开发中的软件项目，请不要从零开始设计，而是基于已有进度继续开发。

项目名称：
BandUnlock

项目目标：
开发一个 Windows 自动锁定/解锁软件。
核心目标是利用小米手环（当前目标设备：Xiaomi Mi Band 8 Pro）的 BLE 信号，实现用户靠近时自动解锁 Windows，离开时自动锁定。
未来计划支持更多 BLE 设备。

====================
当前开发环境
====================

操作系统：
Windows 11 25H2
Build:
26200.8875

开发环境：
Visual Studio 2026 Preview

项目类型：
WPF Desktop Application

框架：
.NET 8

目标框架：
net8.0-windows10.0.26100.1

语言：
C#

主要 NuGet：
InTheHand.BluetoothLE 4.0.44

====================
当前项目结构
====================

BandUnlock
├── Models
│   ├── BluetoothDeviceInfo.cs    # BLE 设备数据模型
│   └── BandBindingInfo.cs        # 手环绑定信息模型（MAC、名称、RSSI 阈值）
├── Services
│   ├── BleAdvertisementScanner.cs  # BLE 广播扫描器（Windows SDK）
│   ├── BleDeviceCache.cs           # 设备缓存（Dictionary<ulong, BluetoothDeviceInfo>）
│   ├── BleDeviceFoundEventArgs.cs  # 扫描事件参数（7个属性）
│   ├── BandBinder.cs               # 绑定服务（扫描→筛选→保存到 JSON）
│   ├── BleDirectConnector.cs       # 定向连接服务（FromIdAsync/FromBluetoothAddressAsync）
│   └── BleProximityMonitor.cs      # 距离监控服务（定时连接→RSSI→BandNearby/BandFar 事件）
├── ViewModels
├── MainWindow.xaml / .xaml.cs      # 主窗口（绑定+监控 UI）
├── App.xaml / App.xaml.cs
└── BandUnlock.csproj


====================
已经完成的工作
====================

1-5. BLE 广播扫描、设备缓存、RSSI 统计（详见 git 历史）

6. BLE 数据分析实验

通过多次 A/B 实验（近距离/远距离/配对模式）确认：
- 小米手环 8 Pro 在正常连接手机后**完全停止 BLE 广播**，扫描不到
- 手环在配对模式下广播名称 "Xiaomi Smart Band 8 Pro 0DE0"
- 手环使用 BLE 随机地址（RPA），MAC 定期变化
- Name、ManufacturerData、Services 在正常模式下全为空

7. 绑定 + 定向连接架构（当前阶段）

实现了"绑定一次 + 日常定向连接"方案：
- BandBindingInfo：绑定信息模型（保存到 %AppData%/BandUnlock/binding.json）
- BandBinder：绑定扫描服务（筛选 Name 含 "Xiaomi Smart Band" 或 Service UUID 含 0000fe95/0000fee0）
- BleDirectConnector：定向连接服务（用 FromBluetoothAddressAsync 或 FromIdAsync）
- BleProximityMonitor：距离监控（每 5 秒定时检测，触发 BandNearby/BandFar 事件）
- MainWindow UI：绑定按钮、解绑按钮、状态显示

8. 测试状态

- ✅ 绑定流程：扫描→发现手环→确认→保存 MAC — 已测试通过
- ✅ Windows 蓝牙配对：手环已在 Windows 蓝牙设置中配对
- ⚠️ 定向连接：FromBluetoothAddressAsync 返回 null（MAC 不匹配）
- 🔄 改用 FromIdAsync（从已配对设备列表查找）— 等待测试结果


====================
已知问题和关键发现
====================

关键发现（来自用户提供的技术分析）：

手环连手机后只发 ADV_DIRECT_IND（定向广播），只响应手机，Windows 看不到。
官方小米感应钥匙的做法：
1. 绑定时临时断手机，手环进入配对模式，Windows 扫描+配对+交换密钥
2. 日常用保存的 MAC 做定向连接（不扫描），读 RSSI 做距离判断
关键要求：手环必须在 Windows 蓝牙系统中完成配对（不只是我们的程序扫描到）

当前阻塞问题：
FromBluetoothAddressAsync 返回 null — 即使 Windows 已配对手环，用程序化 MAC 访问仍可能失败。
下一步需要测试 FromIdAsync 方式（通过 DeviceInformation.Id 访问已配对设备）。

手环连接限制：同时最多 2 个 BLE 加密连接（手机 + 电脑）


====================
下一阶段目标
====================

1. 测试 FromIdAsync 能否连接已配对的手环
2. 如果能连接，实现 RSSI 读取（WinRT 没有直接的 BluetoothLEDevice.Rssi 属性，需要通过 advertisement watcher 或 GATT 获取）
3. 实现 Windows 锁定/解锁（LockWorkStation API）
4. 研究 Windows Hello / Credential Provider 方案实现安全解锁


====================
开发要求
====================

请遵守：

1. 先读取当前项目全部文件。

2. 不要主动重新规划架构，不要偏离已有路线。
   每次修改前说明修改哪些文件、为什么修改。

3. 所有代码必须兼容 .NET 8、WPF、Windows 11 25H2

4. 使用 InTheHand.BluetoothLE 4.0.44（不要换版本）
   定向连接使用 Windows.Devices.Bluetooth（WinRT，已有 SDK 引用）

5. 不要删除已有代码。

6. 保持代码可维护性。这是一个长期项目，不是一次性 Demo。

