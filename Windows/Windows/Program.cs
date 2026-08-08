using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace iPhoneWindowsUnlock;

internal class Program
{
    static async Task Main()
    {
        Console.Title = "iPhoneWindowsUnlock";

        Console.WriteLine("=================================");
        Console.WriteLine("       iPhoneWindowsUnlock");
        Console.WriteLine("=================================");
        Console.WriteLine();
        Console.WriteLine("Recherche des appareils Bluetooth...");
        Console.WriteLine();

        try
        {
            string selector = BluetoothDevice.GetDeviceSelector();

            DeviceInformationCollection devices =
                await DeviceInformation.FindAllAsync(selector);

            if (devices.Count == 0)
            {
                Console.WriteLine("❌ Aucun appareil Bluetooth détecté.");
            }
            else
            {
                Console.WriteLine($"✅ {devices.Count} appareil(s) Bluetooth trouvé(s) :");
                Console.WriteLine();

                foreach (DeviceInformation device in devices)
                {
                    Console.WriteLine($"• {device.Name}");

                    if (!string.IsNullOrWhiteSpace(device.Id))
                    {
                        Console.WriteLine($"  ID : {device.Id}");
                    }

                    Console.WriteLine();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Erreur pendant la recherche Bluetooth.");
            Console.WriteLine();
            Console.WriteLine(ex.Message);
        }

        Console.WriteLine();
        Console.WriteLine("Appuyez sur une touche pour quitter...");
        Console.ReadKey();
    }
}
