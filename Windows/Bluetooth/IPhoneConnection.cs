using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.Rfcomm;

namespace iPhoneWindowsUnlock.Bluetooth;

public sealed class IPhoneConnection
{
    private const string IapUuid =
        "00000000-DECA-FADE-DECA-DEAFDECACAFE";

    public string? IPhoneName { get; private set; }

    public bool IsDetected { get; private set; }

    public bool IsBluetoothAvailable { get; private set; }

    public bool IsIapAvailable { get; private set; }

    public async Task<bool> DetectAsync()
    {
        IsDetected = false;
        IsBluetoothAvailable = false;
        IsIapAvailable = false;
        IPhoneName = null;

        var devices =
            await DeviceInformation.FindAllAsync(
                BluetoothDevice.GetDeviceSelector());

        DeviceInformation? iphone = null;

        foreach (var device in devices)
        {
            if (!string.IsNullOrWhiteSpace(device.Name) &&
                device.Name.Contains(
                    "iPhone",
                    StringComparison.OrdinalIgnoreCase))
            {
                iphone = device;
                break;
            }
        }

        if (iphone == null)
            return false;

        IPhoneName = iphone.Name;
        IsDetected = true;

        using BluetoothDevice? bluetooth =
            await BluetoothDevice.FromIdAsync(iphone.Id);

        if (bluetooth == null)
            return true;

        IsBluetoothAvailable = true;

        var services =
            await bluetooth.GetRfcommServicesAsync(
                BluetoothCacheMode.Uncached);

        foreach (RfcommDeviceService service in services.Services)
        {
            string uuid =
                service.ServiceId
                    .AsString()
                    .Trim('{', '}');

            if (uuid.Equals(
                IapUuid,
                StringComparison.OrdinalIgnoreCase))
            {
                IsIapAvailable = true;
                service.Dispose();
                break;
            }

            service.Dispose();
        }

        return true;
    }
}
