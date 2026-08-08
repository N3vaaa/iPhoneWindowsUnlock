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
                    device.Name.Contains(
                        "iPhone",
                        StringComparison.OrdinalIgnoreCase))
                {
                    iPhone = device;
                    break;
                }
            }

            if (iPhone == null)
            {
                Console.WriteLine("❌ Aucun iPhone détecté.");
            }
            else
            {
                Console.WriteLine("✅ iPhone détecté !");
                Console.WriteLine();
                Console.WriteLine($"Nom : {iPhone.Name}");
                Console.WriteLine($"ID  : {iPhone.Id}");
                Console.WriteLine();

                Console.WriteLine(
                    "Ouverture du périphérique Bluetooth..."
                );

                using BluetoothDevice? bluetoothDevice =
                    await BluetoothDevice.FromIdAsync(iPhone.Id);

                if (bluetoothDevice == null)
                {
                    Console.WriteLine();
                    Console.WriteLine(
                        "❌ Impossible d'ouvrir le périphérique."
                    );
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine(
                        "✅ Périphérique Bluetooth ouvert !"
                    );

                    Console.WriteLine();
                    Console.WriteLine(
                        "Recherche des services Bluetooth..."
                    );

                    try
                    {
                        var services =
                            await bluetoothDevice
                                .GetRfcommServicesAsync();

                        Console.WriteLine();
                        Console.WriteLine(
                            $"Services RFCOMM trouvés : {services.Services.Count}"
                        );

                        if (services.Services.Count == 0)
                        {
                            Console.WriteLine(
                                "ℹ️ Aucun service RFCOMM exposé."
                            );
                        }
                        else
                        {
                            foreach (var service in services.Services)
                            {
                                Console.WriteLine();
                                Console.WriteLine(
                                    $"Service : {service.ServiceId}"
                                );
                            }
                        }
                    }
                    catch (Exception serviceError)
                    {
                        Console.WriteLine();
                        Console.WriteLine(
                            "⚠️ Les services ne peuvent pas être énumérés."
                        );
                        Console.WriteLine(
                            $"Message : {serviceError.Message}"
                        );
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("❌ Erreur Bluetooth.");
            Console.WriteLine($"Type : {ex.GetType().Name}");
            Console.WriteLine($"Message : {ex.Message}");
        }

        Console.WriteLine();
        Console.WriteLine("Appuyez sur une touche pour quitter...");
        Console.ReadKey();
    }
}
