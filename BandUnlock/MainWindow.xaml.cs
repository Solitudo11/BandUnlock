using BandUnlock.Models;
using BandUnlock.Services;
using System.Windows;

namespace BandUnlock
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BandBinder? _binder;
        private BleProximityMonitor? _monitor;
        private BandBindingInfo? _bindingInfo;

        public MainWindow()
        {
            Console.WriteLine("MainWindow Constructor Start");

            InitializeComponent();

            Console.WriteLine("InitializeComponent Finished");

            // 检查是否有已保存的绑定
            _bindingInfo = BandBinder.LoadBinding();

            if (_bindingInfo != null)
            {
                ShowBoundState(_bindingInfo);
            }
            else
            {
                ShowUnboundState();
            }
        }


        /// <summary>
        /// "绑定手环"按钮点击
        /// </summary>
        private void BindButton_Click(
            object sender, RoutedEventArgs e)
        {
            Console.WriteLine("开始绑定流程");

            // 提示用户
            var result = MessageBox.Show(
                "请先在手机上断开手环蓝牙连接，" +
                "让手环进入配对模式（显示'连接新手机'），" +
                "然后点击确定开始扫描。\n\n" +
                "扫描到手环后会自动提示确认绑定。",
                "绑定手环",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Information);

            if (result != MessageBoxResult.OK)
            {
                return;
            }

            // 切换到绑定扫描 UI
            BindButton.Visibility = Visibility.Collapsed;
            UnbindButton.Visibility = Visibility.Collapsed;
            StopBindButton.Visibility = Visibility.Visible;
            StatusText.Text = "正在扫描手环...";

            // 创建 Binder 并开始扫描
            _binder = new BandBinder();

            _binder.BandFound += (s, args) =>
            {
                // 在 UI 线程处理
                Dispatcher.Invoke(() =>
                {
                    OnBandFound(args);
                });
            };

            _binder.StartScan();
        }


        /// <summary>
        /// 发现手环候选设备
        /// </summary>
        private void OnBandFound(BleDeviceFoundEventArgs e)
        {
            Console.WriteLine(
                $"发现手环候选: " +
                $"Name={e.Name} " +
                $"Address=0x{e.Address:X12} " +
                $"RSSI={e.Rssi}");

            // 停止扫描
            _binder?.StopScan();

            // 确认绑定
            var confirm = MessageBox.Show(
                $"发现手环:\n\n" +
                $"名称: {e.Name}\n" +
                $"地址: 0x{e.Address:X12}\n" +
                $"RSSI: {e.Rssi} dBm\n\n" +
                $"是否绑定此设备？",
                "确认绑定",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm == MessageBoxResult.Yes)
            {
                bool success =
                    _binder!.ConfirmBind(e.Address);

                if (success)
                {
                    _bindingInfo = BandBinder.LoadBinding();

                    MessageBox.Show(
                        "绑定成功！\n\n" +
                        "现在可以重新连接手机和手环。" +
                        "程序会自动监控手环距离。",
                        "绑定成功",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    ShowBoundState(_bindingInfo!);
                }
                else
                {
                    MessageBox.Show(
                        "绑定失败，请重试。",
                        "错误",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    ShowUnboundState();
                }
            }
            else
            {
                ShowUnboundState();
            }
        }


        /// <summary>
        /// "停止扫描"按钮点击
        /// </summary>
        private void StopBindButton_Click(
            object sender, RoutedEventArgs e)
        {
            _binder?.StopScan();

            if (_bindingInfo != null)
            {
                ShowBoundState(_bindingInfo);
            }
            else
            {
                ShowUnboundState();
            }
        }


        /// <summary>
        /// "解除绑定"按钮点击
        /// </summary>
        private void UnbindButton_Click(
            object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "确定要解除绑定吗？\n" +
                "解除后需要重新绑定才能使用。",
                "解除绑定",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            // 停止监控
            _monitor?.Stop();
            _monitor = null;

            // 删除绑定信息
            BandBinder.ClearBinding();
            _bindingInfo = null;

            ShowUnboundState();
        }


        /// <summary>
        /// 显示"已绑定"状态：启动监控
        /// </summary>
        private void ShowBoundState(BandBindingInfo binding)
        {
            StatusText.Text =
                $"已绑定: {binding.Name} " +
                $"(0x{binding.Address:X12})";

            BindButton.Visibility = Visibility.Collapsed;
            UnbindButton.Visibility = Visibility.Visible;
            StopBindButton.Visibility = Visibility.Collapsed;

            // 启动距离监控
            _monitor = new BleProximityMonitor(binding);

            _monitor.StatusChanged += (s, status) =>
            {
                Dispatcher.Invoke(() =>
                {
                    StatusText.Text =
                        $"已绑定: {binding.Name} | {status}";
                });
            };

            _monitor.BandNearby += (s, e) =>
            {
                Console.WriteLine("事件: 手环进入近距离");
                // 后续: 解锁 Windows
            };

            _monitor.BandFar += (s, e) =>
            {
                Console.WriteLine("事件: 手环离开远距离");
                // 后续: 锁定 Windows
            };

            _monitor.Start(intervalSeconds: 5);
        }


        /// <summary>
        /// 显示"未绑定"状态
        /// </summary>
        private void ShowUnboundState()
        {
            StatusText.Text = "未绑定手环，请先绑定";

            BindButton.Visibility = Visibility.Visible;
            UnbindButton.Visibility = Visibility.Collapsed;
            StopBindButton.Visibility = Visibility.Collapsed;
        }
    }
}
