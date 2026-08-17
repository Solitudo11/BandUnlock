using System.IO;
using System.Text.Json;
using BandUnlock.Models;

namespace BandUnlock.Services;

/// <summary>
/// 手环绑定服务：扫描 → 筛选小米手环 → 保存绑定信息
/// </summary>
public class BandBinder
{
    private readonly BleAdvertisementScanner _scanner;
    private readonly BleDeviceCache _cache;

    private static readonly string ConfigDir =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData),
            "BandUnlock");

    private static readonly string BindingFile =
        Path.Combine(ConfigDir, "binding.json");

    /// <summary>
    /// 扫描过程中发现手环时触发（提供设备信息供 UI 展示）
    /// </summary>
    public event EventHandler<BleDeviceFoundEventArgs>? BandFound;


    public BandBinder()
    {
        _scanner = new BleAdvertisementScanner();
        _cache = new BleDeviceCache();

        _scanner.DeviceFound += OnDeviceFound;
    }


    /// <summary>
    /// 启动绑定扫描
    /// </summary>
    public void StartScan()
    {
        Console.WriteLine("BandBinder: 开始绑定扫描");
        _cache.Clear();
        _scanner.Start();
    }

    /// <summary>
    /// 停止绑定扫描
    /// </summary>
    public void StopScan()
    {
        Console.WriteLine("BandBinder: 停止绑定扫描");
        _scanner.Stop();
    }

    /// <summary>
    /// 确认绑定：从缓存中取出手环信息并保存
    /// </summary>
    /// <param name="address">要绑定的手环地址</param>
    /// <returns>绑定是否成功</returns>
    public bool ConfirmBind(ulong address)
    {
        var device = _cache.GetByAddress(address);

        if (device == null)
        {
            Console.WriteLine(
                $"BandBinder: 地址 {address} 不在缓存中");
            return false;
        }

        var info = new BandBindingInfo
        {
            Address = device.Address,
            Name = device.Name,
            BoundAt = DateTime.Now,
            RssiThreshold = -70
        };

        SaveBinding(info);

        Console.WriteLine(
            $"BandBinder: 绑定成功 " +
            $"Name={info.Name} " +
            $"Address={info.Address}");

        return true;
    }

    /// <summary>
    /// 获取所有扫描到的设备
    /// </summary>
    public List<BluetoothDeviceInfo> GetScannedDevices()
    {
        return _cache.GetAll();
    }

    /// <summary>
    /// 获取已筛选为手环候选的设备
    /// </summary>
    public List<BluetoothDeviceInfo> GetBandCandidates()
    {
        return _cache.GetAll()
            .Where(IsBandCandidate)
            .ToList();
    }


    /// <summary>
    /// 加载已保存的绑定信息
    /// </summary>
    public static BandBindingInfo? LoadBinding()
    {
        if (!File.Exists(BindingFile))
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(BindingFile);
            return JsonSerializer.Deserialize<BandBindingInfo>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"BandBinder: 加载绑定信息失败: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 删除绑定信息
    /// </summary>
    public static void ClearBinding()
    {
        if (File.Exists(BindingFile))
        {
            File.Delete(BindingFile);
            Console.WriteLine("BandBinder: 绑定信息已清除");
        }
    }


    private void OnDeviceFound(
        object? sender, BleDeviceFoundEventArgs e)
    {
        _cache.AddOrUpdate(
            e.Address,
            e.Name,
            e.Rssi,
            e.Manufacturer,
            e.Services,
            e.AdvertisementType,
            e.Timestamp);

        // 如果是手环候选，触发 BandFound 事件
        bool isCandidate = IsBandCandidate(e.Name, e.Services);

        if (!string.IsNullOrEmpty(e.Name))
        {
            Console.WriteLine(
                $"BandBinder: 设备 Name={e.Name} " +
                $"Services={e.Services} " +
                $"IsCandidate={isCandidate}");
        }

        if (isCandidate)
        {
            Console.WriteLine(
                $"BandBinder: ★ 发现手环候选! " +
                $"Name={e.Name} Address=0x{e.Address:X12}");
            BandFound?.Invoke(this, e);
        }
    }

    /// <summary>
    /// 判断设备是否为小米手环候选
    /// </summary>
    private static bool IsBandCandidate(BluetoothDeviceInfo d)
    {
        return IsBandCandidate(d.Name, d.Services);
    }

    /// <summary>
    /// 根据名称或 Service UUID 筛选小米手环
    /// </summary>
    private static bool IsBandCandidate(
        string name, string services)
    {
        // 名称包含 "Xiaomi Smart Band"
        if (!string.IsNullOrEmpty(name) &&
            name.Contains("Xiaomi Smart Band",
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Service UUID 包含 MiBeacon (0000fe95) 或
        // Mi Band 主服务 (0000fee0)
        if (!string.IsNullOrEmpty(services))
        {
            if (services.Contains("0000fe95",
                    StringComparison.OrdinalIgnoreCase) ||
                services.Contains("0000fee0",
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static void SaveBinding(BandBindingInfo info)
    {
        if (!Directory.Exists(ConfigDir))
        {
            Directory.CreateDirectory(ConfigDir);
        }

        var json = JsonSerializer.Serialize(
            info,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(BindingFile, json);

        Console.WriteLine(
            $"BandBinder: 绑定信息已保存到 {BindingFile}");
    }
}
