using Windows.Devices.Bluetooth.Advertisement;
using InTheHand.Bluetooth;

namespace BandUnlock.Services;

public class BluetoothScanner
{
    public async Task ScanAsync()
    {
        var devices = await Bluetooth.ScanForDevicesAsync();

        foreach (var device in devices)
        {
            Console.WriteLine(device.Name);
        }
        {
            
        }
    }
}