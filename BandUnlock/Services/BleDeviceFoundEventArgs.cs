namespace BandUnlock.Services;

public class BleDeviceFoundEventArgs : EventArgs
{
    public string Name { get; }

    public ulong Address { get; }

    public short Rssi { get; }

    public string Manufacturer { get; }

    public string Services { get; }

    public string AdvertisementType { get; }

    public DateTime Timestamp { get; }


    public BleDeviceFoundEventArgs(
        string name,
        ulong address,
        short rssi,
        string manufacturer,
        string services,
        string advertisementType,
        DateTime timestamp)
    {
        Name = name;
        Address = address;
        Rssi = rssi;
        Manufacturer = manufacturer;
        Services = services;
        AdvertisementType = advertisementType;
        Timestamp = timestamp;
    }
}