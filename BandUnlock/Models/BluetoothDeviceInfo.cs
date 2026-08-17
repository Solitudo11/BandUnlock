using InTheHand.Bluetooth;

namespace BandUnlock.Models;

public class BluetoothDeviceInfo
{
    /// <summary>
    /// BLE设备对象
    /// </summary>
    public BluetoothDevice Device { get; set; } = null!;

    /// <summary>
    /// 设备名称
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 蓝牙地址（唯一）
    /// </summary>
    public ulong Address { get; set; }

    /// <summary>
    /// RSSI 信号强度（当前值）
    /// </summary>
    public short Rssi { get; set; }

    /// <summary>
    /// 平均 RSSI
    /// </summary>
    public short AverageRssi { get; set; }

    /// <summary>
    /// 广播次数
    /// </summary>
    public int AdvertisementCount { get; set; }

    /// <summary>
    /// Manufacturer Data（十六进制字符串）
    /// </summary>
    public string Manufacturer { get; set; } = "";

    /// <summary>
    /// Service UUID 列表
    /// </summary>
    public string Services { get; set; } = "";

    /// <summary>
    /// 广播类型
    /// </summary>
    public string AdvertisementType { get; set; } = "";

    /// <summary>
    /// 第一次发现时间
    /// </summary>
    public DateTime FirstSeen { get; set; }

    /// <summary>
    /// 最后发现时间
    /// </summary>
    public DateTime LastSeen { get; set; }
}