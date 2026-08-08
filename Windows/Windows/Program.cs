using System;
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

        Console.WriteLine("Recherche de votre iPhone...");
        Console.WriteLine();

        try
        {
            string selector = BluetoothDevice.GetDeviceSelector();

            DeviceInformationCollection devices =
                await DeviceInformation.FindAllAsync(selector);

            DeviceInformation? iPhone = null;

            foreach (DeviceInformation device in devices)
            {
                if (!string.IsNullOrWhiteSpace(device.Name) &&
                    device.Name.Contains("iPhone", StringComparison.OrdinalIgnoreCase))
                {
                    iPhone = device;
                    break;
                }
            }

            if (iPhone == null)
            {
                Console.WriteLine("❌ Aucun iPhone détecté.");
                Console.WriteLine();
                Console.WriteLine("Vérifiez que le Bluetooth est activé");
                Console.WriteLine("sur l'iPhone et sur le PC.");
            }
            else
            {
                Console.WriteLine("✅ iPhone détecté !");
                Console.WriteLine();
                Console.WriteLine($"Nom : {iPhone.Name}");
                Console.WriteLine($"ID  : {iPhone.Id}");
                Console.WriteLine();
                Console.WriteLine("📱 L'iPhone est visible par Windows.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Erreur pendant la recherche.");
            Console.WriteLine();
            Console.WriteLine(ex.Message);
        }

        Console.WriteLine();
        Console.WriteLine("Appuyez sur une touche pour quitter...");
        Console.ReadKey();
    }
}
