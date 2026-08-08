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
                Console.WriteLine("Vérifiez que l'iPhone est allumé");
                Console.WriteLine("et que le Bluetooth est activé.");
            }
            else
            {
                Console.WriteLine("✅ iPhone détecté !");
                Console.WriteLine();
                Console.WriteLine($"Nom : {iPhone.Name}");
                Console.WriteLine($"ID  : {iPhone.Id}");
                Console.WriteLine();

                Console.WriteLine("Tentative d'ouverture du périphérique Bluetooth...");

                BluetoothDevice? bluetoothDevice =
                    await BluetoothDevice.FromIdAsync(iPhone.Id);

                if (bluetoothDevice == null)
                {
                    Console.WriteLine();
                    Console.WriteLine("⚠️ Windows a trouvé l'iPhone,");
                    Console.WriteLine("mais ne peut pas ouvrir son périphérique Bluetooth.");
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("✅ Périphérique Bluetooth ouvert !");
                    Console.WriteLine();
                    Console.WriteLine($"Nom : {bluetoothDevice.Name}");
                    Console.WriteLine(
                        $"Adresse Bluetooth : 0x{bluetoothDevice.BluetoothAddress:X}"
                    );
                    Console.WriteLine();
                    Console.WriteLine("📱 Windows peut maintenant accéder");
                    Console.WriteLine("au périphérique Bluetooth de l'iPhone.");

                    bluetoothDevice.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("❌ Erreur pendant la communication Bluetooth.");
            Console.WriteLine();
            Console.WriteLine($"Type : {ex.GetType().Name}");
            Console.WriteLine($"Message : {ex.Message}");
        }

        Console.WriteLine();
        Console.WriteLine("Appuyez sur une touche pour quitter...");
        Console.ReadKey();
    }
}
