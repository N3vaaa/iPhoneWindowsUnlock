using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.Rfcomm;

namespace iPhoneWindowsUnlock;

internal class Program
{
    private const string IapUuid =
        "00000000-DECA-FADE-DECA-DEAFDECACAFE";

    static async Task Main()
    {
        Console.Title = "iPhoneWindowsUnlock";

        Console.WriteLine("======================================");
        Console.WriteLine("        iPhoneWindowsUnlock");
        Console.WriteLine("======================================");
        Console.WriteLine();

        await DetectIPhoneAsync();

        Console.WriteLine();
        Console.WriteLine("--------------------------------------");
        Console.WriteLine("Diagnostic terminé.");
        Console.WriteLine("--------------------------------------");
        Console.WriteLine();
        Console.WriteLine("Appuyez sur une touche pour quitter...");

        Console.ReadKey();
    }

    private static async Task DetectIPhoneAsync()
    {
        try
        {
            Console.WriteLine("🔎 Recherche de l'iPhone...");

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
            {
                Console.WriteLine("🔴 Aucun iPhone détecté.");
                return;
            }

            Console.WriteLine($"🟢 iPhone détecté : {iphone.Name}");
            Console.WriteLine();

            using BluetoothDevice? bluetooth =
                await BluetoothDevice.FromIdAsync(iphone.Id);

            if (bluetooth == null)
            {
                Console.WriteLine(
                    "🔴 Impossible d'ouvrir le périphérique Bluetooth.");
                return;
            }

            Console.WriteLine("🟢 Bluetooth accessible.");

            var result =
                await bluetooth.GetRfcommServicesAsync(
                    BluetoothCacheMode.Uncached);

            RfcommDeviceService? iap = null;

            foreach (var service in result.Services)
            {
                string uuid =
                    service.ServiceId
                        .AsString()
                        .Trim('{', '}');

                if (uuid.Equals(
                    IapUuid,
                    StringComparison.OrdinalIgnoreCase))
                {
                    iap = service;
                    break;
                }

                service.Dispose();
            }

            if (iap == null)
            {
                Console.WriteLine(
                    "🟠 Wireless iAP non disponible.");
                return;
            }

            Console.WriteLine(
                "🟢 Wireless iAP détecté.");

            Console.WriteLine();
            Console.WriteLine("État :");
            Console.WriteLine("  • iPhone       : DÉTECTÉ");
            Console.WriteLine("  • Bluetooth    : DISPONIBLE");
            Console.WriteLine("  • Wireless iAP : DISPONIBLE");

            iap.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("🔴 Erreur :");
            Console.WriteLine(ex.Message);
        }
    }
}
