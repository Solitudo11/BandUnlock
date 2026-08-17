using BandUnlock.Models;

namespace BandUnlock.Services;

public class BleDeviceCache
{
    private readonly Dictionary<ulong, BluetoothDeviceInfo> _devices = new();

    public void AddOrUpdate(
        ulong address,
        string name,
        short rssi,
        string manufacturer,
        string services,
        string advertisementType,
        DateTime timestamp)
    {
        if (_devices.TryGetValue(address, out var existing))
        {
            existing.Name = name;
            existing.Rssi = rssi;
            existing.LastSeen = timestamp;
            existing.AdvertisementCount++;
            existing.Manufacturer = manufacturer;
            existing.Services = services;
            existing.AdvertisementType = advertisementType;

            var totalRssi = (int)existing.AverageRssi * (existing.AdvertisementCount - 1) + rssi;
            existing.AverageRssi = (short)(totalRssi / existing.AdvertisementCount);
        }
        else
        {
            _devices[address] = new BluetoothDeviceInfo
            {
                Name = name,
                Address = address,
                Rssi = rssi,
                AverageRssi = rssi,
                AdvertisementCount = 1,
                Manufacturer = manufacturer,
                Services = services,
                AdvertisementType = advertisementType,
                FirstSeen = timestamp,
                LastSeen = timestamp
            };
        }
    }

    public List<BluetoothDeviceInfo> GetAll()
    {
        return _devices.Values.ToList();
    }

    public BluetoothDeviceInfo? GetByAddress(ulong address)
    {
        _devices.TryGetValue(address, out var device);
        return device;
    }

    public void Clear()
    {
        _devices.Clear();
    }

    public int Count => _devices.Count;
}
