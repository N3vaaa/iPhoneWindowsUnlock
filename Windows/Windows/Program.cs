using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;

namespace iPhoneWindowsUnlock;

internal class Program
{
    private const string IapServiceUuid =
        "00000000-DECA-FADE-DECA-DEAFDECACAFE";

    static async Task Main()
    {
        Console.Title = "iPhoneWindowsUnlock";

        Console.WriteLine("=================================");
        Console.WriteLine("       iPhoneWindowsUnlock");
        Console.WriteLine("=================================");
        Console.WriteLine();

        try
        {
            Console.WriteLine("Recherche de votre iPhone...");
            Console.WriteLine();

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

            Console.WriteLine("Ouverture du périphérique Bluetooth...");

            using BluetoothDevice? bluetoothDevice =
                await BluetoothDevice.FromIdAsync(iPhone.Id);

            if (bluetoothDevice == null)
            {
                Console.WriteLine("❌ Impossible d'ouvrir l'iPhone.");
                WaitAndExit();
                return;
            }

            Console.WriteLine("✅ Périphérique Bluetooth ouvert !");
            Console.WriteLine();

            Console.WriteLine("Recherche des services RFCOMM...");

            var result =
                await bluetoothDevice.GetRfcommServicesAsync(
                    BluetoothCacheMode.Uncached);

            Console.WriteLine();
            Console.WriteLine(
                $"✅ {result.Services.Count} services RFCOMM trouvés.");

            RfcommDeviceService? iapService = null;

            foreach (RfcommDeviceService service in result.Services)
            {
                string uuid = service.ServiceId
                    .AsString()
                    .Trim('{', '}');

                Console.WriteLine();
                Console.WriteLine($"Service : {uuid}");

                if (uuid.Equals(
                    IapServiceUuid,
                    StringComparison.OrdinalIgnoreCase))
                {
                    iapService = service;
                    break;
                }

                service.Dispose();
            }

            if (iapService == null)
            {
                Console.WriteLine();
                Console.WriteLine(
                    "❌ Service Wireless iAP introuvable.");
                WaitAndExit();
                return;
            }

            Console.WriteLine();
            Console.WriteLine("=================================");
            Console.WriteLine("       SERVICE WIRELESS iAP");
            Console.WriteLine("=================================");
            Console.WriteLine();

            Console.WriteLine(
                $"UUID : {iapService.ServiceId.AsString()}");

            Console.WriteLine(
                $"Hôte : {iapService.ConnectionHostName}");

            Console.WriteLine(
                $"Service : {iapService.ConnectionServiceName}");

            Console.WriteLine(
                $"Protection : {iapService.ProtectionLevel}");

            Console.WriteLine();
            Console.WriteLine(
                "Tentative de connexion RFCOMM...");
            Console.WriteLine();

            using StreamSocket socket = new StreamSocket();

            await socket.ConnectAsync(
                iapService.ConnectionHostName,
                iapService.ConnectionServiceName,
                SocketProtectionLevel
                    .BluetoothEncryptionWithAuthentication);

            Console.WriteLine("🟢 CONNEXION RFCOMM RÉUSSIE !");
            Console.WriteLine();

            Console.WriteLine(
                "Le service Wireless iAP accepte la connexion.");

            Console.WriteLine();
            Console.WriteLine(
                "Aucune donnée n'a été envoyée à l'iPhone.");

            iapService.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("🔴 ÉCHEC DE LA CONNEXION");
            Console.WriteLine();
            Console.WriteLine($"Type    : {ex.GetType().Name}");
            Console.WriteLine($"Message : {ex.Message}");
            Console.WriteLine($"Code    : 0x{ex.HResult:X8}");
        }

        WaitAndExit();
    }

    private static void WaitAndExit()
    {
        Console.WriteLine();
        Console.WriteLine("Appuyez sur une touche pour quitter...");
        Console.ReadKey();
    }
}
