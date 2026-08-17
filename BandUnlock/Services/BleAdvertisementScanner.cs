using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth.Advertisement;

namespace BandUnlock.Services;

public class BleAdvertisementScanner
{
    private readonly BluetoothLEAdvertisementWatcher _watcher;

    public event EventHandler<BleDeviceFoundEventArgs>? DeviceFound;

    public BleAdvertisementScanner()
    {
        _watcher = new BluetoothLEAdvertisementWatcher();

        _watcher.Received += Watcher_Received;
    }


    public void Start()
    {
        try
        {
            Console.WriteLine(
                $"BLE Status Before Start: {_watcher.Status}");

            _watcher.Start();

            Console.WriteLine(
                $"BLE Status After Start: {_watcher.Status}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("BLE Start Exception:");
            Console.WriteLine(ex.ToString());
        }
    }


    public void Stop()
    {
        if (_watcher.Status ==
            BluetoothLEAdvertisementWatcherStatus.Started)
        {
            _watcher.Stop();
        }
    }


    private void Watcher_Received(
        BluetoothLEAdvertisementWatcher sender,
        BluetoothLEAdvertisementReceivedEventArgs args)
    {
        var deviceName = args.Advertisement.LocalName;

        var address = args.BluetoothAddress;

        var rssi = args.RawSignalStrengthInDBm;


        var manufacturer = "";

        foreach (var data in args.Advertisement.ManufacturerData)
        {
            manufacturer += BitConverter.ToString(data.Data.ToArray());
        }


        var services = "";

        foreach (var uuid in args.Advertisement.ServiceUuids)
        {
            services += uuid.ToString() + " ";
        }


        var advertisementType = args.AdvertisementType.ToString();

        var timestamp = DateTime.Now;


        DeviceFound?.Invoke(
            this,
            new BleDeviceFoundEventArgs(
                deviceName,
                address,
                rssi,
                manufacturer,
                services,
                advertisementType,
                timestamp));
    }
}