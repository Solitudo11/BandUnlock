using BandUnlock.Models;

namespace BandUnlock.Services;

/// <summary>
/// BLE 距离监控服务：定时定向连接 → 读 RSSI → 判断远近
/// </summary>
public class BleProximityMonitor
{
    private readonly BleDirectConnector _connector;
    private readonly ulong _address;
    private readonly short _rssiThreshold;
    private System.Windows.Threading.DispatcherTimer? _timer;

    private readonly Queue<short> _rssiHistory = new();
    private const int RssiWindowSize = 5;

    private bool _isNearby;

    /// <summary>
    /// 手环进入近距离时触发
    /// </summary>
    public event EventHandler? BandNearby;

    /// <summary>
    /// 手环离开远距离时触发
    /// </summary>
    public event EventHandler? BandFar;

    /// <summary>
    /// 状态变化时触发（供 UI 更新）
    /// </summary>
    public event EventHandler<string>? StatusChanged;

    /// <summary>
    /// 当前是否在附近
    /// </summary>
    public bool IsNearby => _isNearby;

    /// <summary>
    /// 最新 RSSI 值
    /// </summary>
    public short? LatestRssi { get; private set; }


    public BleProximityMonitor(BandBindingInfo binding)
    {
        _connector = new BleDirectConnector();
        _address = binding.Address;
        _rssiThreshold = binding.RssiThreshold;
    }


    /// <summary>
    /// 启动定时监控
    /// </summary>
    /// <param name="intervalSeconds">检测间隔（秒）</param>
    public void Start(int intervalSeconds = 5)
    {
        Console.WriteLine(
            $"ProximityMonitor: 启动监控 " +
            $"间隔={intervalSeconds}s " +
            $"阈值={_rssiThreshold}dBm");

        _timer = new System.Windows.Threading.DispatcherTimer();
        _timer.Interval =
            TimeSpan.FromSeconds(intervalSeconds);
        _timer.Tick += async (s, e) =>
            await CheckProximity();
        _timer.Start();

        UpdateStatus("监控中...");
    }

    /// <summary>
    /// 停止监控
    /// </summary>
    public void Stop()
    {
        _timer?.Stop();
        _timer = null;
        _connector.Disconnect();

        Console.WriteLine("ProximityMonitor: 已停止");
        UpdateStatus("已停止");
    }


    private async Task CheckProximity()
    {
        try
        {
            // 每次检测前先断开上一次的连接
            _connector.Disconnect();

            // 从已配对设备中查找手环并连接
            bool connected =
                await _connector.ConnectToPairedBandAsync();

            if (!connected)
            {
                Console.WriteLine(
                    "ProximityMonitor: 连接失败");

                // 连接失败 → 视为离开
                if (_isNearby)
                {
                    _isNearby = false;
                    LatestRssi = null;
                    BandFar?.Invoke(this, EventArgs.Empty);
                    UpdateStatus("手环已离开（连接失败）");
                }
                else
                {
                    UpdateStatus("手环未连接");
                }

                return;
            }

            // 连接成功 = 设备在附近（BLE 连接有距离限制）
            short? rssi = _connector.ReadRssi();
            LatestRssi = rssi;

            // 连接成功本身就意味着距离够近
            // BLE 连接超过 ~10m 基本连不上
            bool nowNearby = true;

            // 如果能读到 RSSI，用阈值进一步判断
            if (rssi.HasValue)
            {
                _rssiHistory.Enqueue(rssi.Value);

                while (_rssiHistory.Count > RssiWindowSize)
                {
                    _rssiHistory.Dequeue();
                }

                short avgRssi = (short)(
                    _rssiHistory.Average(x => x));

                nowNearby =
                    avgRssi >= _rssiThreshold;

                Console.WriteLine(
                    $"ProximityMonitor: " +
                    $"RSSI={rssi.Value} " +
                    $"Avg={avgRssi} " +
                    $"Nearby={nowNearby}");
            }
            else
            {
                // 无法读 RSSI 但连接成功，
                // 视为在附近
                Console.WriteLine(
                    "ProximityMonitor: " +
                    "连接成功（RSSI 未读取）");
            }

            // 状态变化检测
            if (nowNearby && !_isNearby)
            {
                _isNearby = true;
                BandNearby?.Invoke(this, EventArgs.Empty);
                UpdateStatus("手环在附近 ✓");
            }
            else if (!nowNearby && _isNearby)
            {
                _isNearby = false;
                BandFar?.Invoke(this, EventArgs.Empty);
                UpdateStatus("手环已离开");
            }
            else
            {
                UpdateStatus(nowNearby
                    ? "手环在附近 ✓"
                    : "手环已离开");
            }

            // 读取完毕，断开连接释放资源
            _connector.Disconnect();
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"ProximityMonitor: 检测异常: {ex.Message}");

            UpdateStatus($"检测异常: {ex.Message}");
        }
    }

    private void UpdateStatus(string status)
    {
        StatusChanged?.Invoke(this, status);
    }
}
