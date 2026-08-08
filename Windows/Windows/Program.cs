using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.Rfcomm;

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
                        "Recherche des services RFCOMM..."
                    );

                    var result =
                        await bluetoothDevice.GetRfcommServicesAsync();

                    Console.WriteLine();

                    if (result.Services.Count == 0)
                    {
                        Console.WriteLine(
                            "❌ Aucun service RFCOMM trouvé."
                        );
                    }
                    else
                    {
                        Console.WriteLine(
                            $"✅ {result.Services.Count} services RFCOMM trouvés."
                        );

                        Console.WriteLine();

                        int number = 1;

                        foreach (RfcommDeviceService service
                                 in result.Services)
                        {
                            Console.WriteLine(
                                $"========== SERVICE {number} =========="
                            );

                            Console.WriteLine(
                                $"UUID : {service.ServiceId.AsString()}"
                            );

                            try
                            {
                                Console.WriteLine(
                                    $"Short ID : 0x{service.ServiceId.AsShortId():X4}"
                                );
                            }
                            catch
                            {
                                Console.WriteLine(
                                    "Short ID : non disponible"
                                );
                            }

                            Console.WriteLine(
                                $"Nom de connexion : {service.ConnectionServiceName}"
                            );

                            Console.WriteLine(
                                $"Hôte : {service.ConnectionHostName}"
                            );

                            Console.WriteLine(
                                $"Protection max : {service.MaxProtectionLevel}"
                            );

                            Console.WriteLine(
                                $"Protection actuelle : {service.ProtectionLevel}"
                            );

                            Console.WriteLine();

                            service.Dispose();

                            number++;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("❌ Erreur Bluetooth.");
            Console.WriteLine();
            Console.WriteLine($"Type : {ex.GetType().Name}");
            Console.WriteLine($"Message : {ex.Message}");
        }

        Console.WriteLine();
        Console.WriteLine("Appuyez sur une touche pour quitter...");
        Console.ReadKey();
    }
}
