namespace BandUnlock.Models;

/// <summary>
/// 手环绑定信息，持久化保存到本地
/// </summary>
public class BandBindingInfo
{
    /// <summary>
    /// 手环蓝牙地址（MAC）
    /// </summary>
    public ulong Address { get; set; }

    /// <summary>
    /// 手环设备名称（绑定时广播的名称）
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// 绑定时间
    /// </summary>
    public DateTime BoundAt { get; set; }

    /// <summary>
    /// RSSI 阈值（dBm），高于此值视为"在附近"
    /// </summary>
    public short RssiThreshold { get; set; } = -70;
}
