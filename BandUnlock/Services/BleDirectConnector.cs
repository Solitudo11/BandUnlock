using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace BandUnlock.Services;

/// <summary>
/// BLE 定向连接服务：用已知 MAC 直连手环，或从已配对设备中查找手环
/// </summary>
public class BleDirectConnector
{
    private BluetoothLEDevice? _device;

    /// <summary>
    /// 是否已连接
    /// </summary>
    public bool IsConnected =>
        _device?.ConnectionStatus ==
        BluetoothConnectionStatus.Connected;

    /// <summary>
    /// 当前连接的设备名称
    /// </summary>
    public string? DeviceName => _device?.Name;


    /// <summary>
    /// 从 Windows 已配对设备中查找小米手环并连接
    /// </summary>
    /// <returns>是否连接成功</returns>
    public async Task<bool> ConnectToPairedBandAsync()
    {
        try
        {
            Console.WriteLine(
                "BleDirect: 正在查找已配对的 BLE 设备...");

            // 枚举所有已配对的 BLE 设备
            string selector =
                BluetoothLEDevice.GetDeviceSelector();

            var devices = await DeviceInformation
                .FindAllAsync(selector);

            Console.WriteLine(
                $"BleDirect: 找到 {devices.Count} 个已配对 BLE 设备");

            foreach (var di in devices)
            {
                Console.WriteLine(
                    $"BleDirect: 设备 Name={di.Name} " +
                    $"Id={di.Id}");
            }

            // 查找小米手环
            DeviceInformation? bandDevice = devices
                .FirstOrDefault(d =>
                    d.Name != null &&
                    d.Name.Contains("Xiaomi Smart Band",
                        StringComparison.OrdinalIgnoreCase));

            if (bandDevice == null)
            {
                Console.WriteLine(
                    "BleDirect: 未找到已配对的小米手环");

                // 输出所有设备名供调试
                foreach (var d in devices)
                {
                    Console.WriteLine(
                        $"  - {d.Name}");
                }

                return false;
            }

            Console.WriteLine(
                $"BleDirect: 找到手环: {bandDevice.Name}");

            return await ConnectToDeviceAsync(bandDevice);
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"BleDirect: 查找已配对设备异常: " +
                $"{ex.Message}");
            return false;
        }
    }


    /// <summary>
    /// 定向连接到指定 MAC 地址的 BLE 设备
    /// </summary>
    /// <param name="address">蓝牙地址</param>
    /// <param name="retryCount">重试次数</param>
    /// <returns>是否连接成功</returns>
    public async Task<bool> ConnectAsync(
        ulong address, int retryCount = 3)
    {
        for (int i = 0; i < retryCount; i++)
        {
            try
            {
                Console.WriteLine(
                    $"BleDirect: 尝试连接 " +
                    $"0x{address:X12} (第 {i + 1} 次)");

                _device = await BluetoothLEDevice
                    .FromBluetoothAddressAsync(address);

                if (_device != null &&
                    _device.ConnectionStatus ==
                        BluetoothConnectionStatus.Connected)
                {
                    Console.WriteLine(
                        $"BleDirect: 连接成功 " +
                        $"Name={_device.Name}");
                    return true;
                }

                if (_device != null)
                {
                    Console.WriteLine(
                        $"BleDirect: 设备找到，状态=" +
                        $"{_device.ConnectionStatus}");

                    await Task.Delay(1000);

                    if (_device.ConnectionStatus ==
                        BluetoothConnectionStatus.Connected)
                    {
                        Console.WriteLine(
                            "BleDirect: 延迟后连接成功");
                        return true;
                    }
                }
                else
                {
                    Console.WriteLine(
                        "BleDirect: FromBluetoothAddress " +
                        "返回 null");
                }

                DisposeDevice();

                if (i < retryCount - 1)
                {
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"BleDirect: 连接异常: {ex.Message}");
                DisposeDevice();

                if (i < retryCount - 1)
                {
                    await Task.Delay(1000);
                }
            }
        }

        Console.WriteLine(
            "BleDirect: 连接失败，已用尽重试次数");
        return false;
    }


    /// <summary>
    /// 连接到指定 DeviceInfo 设备
    /// </summary>
    private async Task<bool> ConnectToDeviceAsync(
        DeviceInformation deviceInfo)
    {
        try
        {
            DisposeDevice();

            _device = await BluetoothLEDevice
                .FromIdAsync(deviceInfo.Id);

            if (_device == null)
            {
                Console.WriteLine(
                    "BleDirect: FromIdAsync 返回 null");
                return false;
            }

            Console.WriteLine(
                $"BleDirect: 连接结果 " +
                $"Name={_device.Name} " +
                $"Status={_device.ConnectionStatus}");

            return _device.ConnectionStatus ==
                BluetoothConnectionStatus.Connected;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"BleDirect: 连接设备异常: {ex.Message}");
            DisposeDevice();
            return false;
        }
    }


    /// <summary>
    /// 读取当前 RSSI（需要已连接状态）
    /// </summary>
    public short? ReadRssi()
    {
        if (_device == null ||
            _device.ConnectionStatus !=
                BluetoothConnectionStatus.Connected)
        {
            Console.WriteLine(
                "BleDirect: 设备未连接，无法读取 RSSI");
            return null;
        }

        try
        {
            Console.WriteLine(
                "BleDirect: 设备已连接，RSSI 读取待实现");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"BleDirect: 读取 RSSI 异常: {ex.Message}");
            return null;
        }
    }


    /// <summary>
    /// 断开连接并释放资源
    /// </summary>
    public void Disconnect()
    {
        DisposeDevice();
        Console.WriteLine("BleDirect: 已断开连接");
    }


    private void DisposeDevice()
    {
        if (_device != null)
        {
            _device.Dispose();
            _device = null;
        }
    }
}
