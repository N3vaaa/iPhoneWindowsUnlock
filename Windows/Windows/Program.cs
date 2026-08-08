using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Storage.Streams;

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
                WaitAndExit();
                return;
            }

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
                    "❌ Impossible d'ouvrir l'iPhone."
                );

                WaitAndExit();
                return;
            }

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
            Console.WriteLine(
                $"✅ {result.Services.Count} services RFCOMM trouvés."
            );

            Console.WriteLine();

            int number = 1;

            foreach (RfcommDeviceService service in result.Services)
            {
                string uuid = service.ServiceId
                    .AsString()
                    .Trim('{', '}');

                Console.WriteLine(
                    $"Service détecté {number} : {uuid}"
                );

                /*
                 * 0x112F = Phonebook Access
                 * 0x111F = Hands-Free
                 *
                 * On ne les inspecte pas.
                 *
                 * Tous les autres services sont inspectés.
                 */
                bool isStandardService =
                    uuid.Equals(
                        "0000112F-0000-1000-8000-00805F9B34FB",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    uuid.Equals(
                        "0000111F-0000-1000-8000-00805F9B34FB",
                        StringComparison.OrdinalIgnoreCase);

                if (!isStandardService)
                {
                    await InspectServiceAsync(
                        service,
                        $"SERVICE {number}"
                    );
                }
                else
                {
                    Console.WriteLine(
                        "   → Service Bluetooth standard ignoré."
                    );

                    service.Dispose();
                }

                Console.WriteLine();

                number++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("❌ ERREUR");
            Console.WriteLine();
            Console.WriteLine(
                $"Type    : {ex.GetType().Name}"
            );
            Console.WriteLine(
                $"Message : {ex.Message}"
            );
        }

        WaitAndExit();
    }

    private static async Task InspectServiceAsync(
        RfcommDeviceService service,
        string serviceName)
    {
        try
        {
            Console.WriteLine();
            Console.WriteLine(
                "================================="
            );
            Console.WriteLine(
                $"        {serviceName}"
            );
            Console.WriteLine(
                "================================="
            );

            Console.WriteLine();

            Console.WriteLine(
                $"UUID : {service.ServiceId.AsString()}"
            );

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
            Console.WriteLine(
                "Lecture des attributs SDP..."
            );

            Console.WriteLine();

            var attributes =
                await service.GetSdpRawAttributesAsync(
                    BluetoothCacheMode.Uncached
                );

            if (attributes == null ||
                attributes.Count == 0)
            {
                Console.WriteLine(
                    "⚠️ Aucun attribut SDP disponible."
                );
            }
            else
            {
                Console.WriteLine(
                    $"✅ {attributes.Count} attribut(s) SDP trouvé(s)."
                );

                Console.WriteLine();

                foreach (var attribute in attributes)
                {
                    Console.WriteLine(
                        $"--- Attribut 0x{attribute.Key:X4} ---"
                    );

                    byte[] data =
                        BufferToBytes(attribute.Value);

                    Console.WriteLine(
                        $"Taille : {data.Length} octet(s)"
                    );

                    Console.WriteLine(
                        $"Données : {ToHex(data)}"
                    );

                    Console.WriteLine();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"❌ Impossible d'inspecter {serviceName}"
            );

            Console.WriteLine(
                $"Type : {ex.GetType().Name}"
            );

            Console.WriteLine(
                $"Message : {ex.Message}"
            );

            Console.WriteLine();
        }
        finally
        {
            service.Dispose();
        }
    }

    private static byte[] BufferToBytes(IBuffer buffer)
    {
        if (buffer == null || buffer.Length == 0)
            return Array.Empty<byte>();

        byte[] data = new byte[buffer.Length];

        using DataReader reader =
            DataReader.FromBuffer(buffer);

        reader.ReadBytes(data);

        return data;
    }

    private static string ToHex(byte[] data)
    {
        if (data.Length == 0)
            return "(vide)";

        return BitConverter
            .ToString(data)
            .Replace("-", " ");
    }

    private static void WaitAndExit()
    {
        Console.WriteLine();
        Console.WriteLine(
            "Appuyez sur une touche pour quitter..."
        );

        Console.ReadKey();
    }
}
