using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

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

            string selector =
                BluetoothDevice.GetDeviceSelector();

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
                Console.WriteLine(
                    "❌ Impossible d'ouvrir l'iPhone."
                );

                WaitAndExit();
                return;
            }

            Console.WriteLine(
                "✅ Périphérique Bluetooth ouvert !"
            );

            Console.WriteLine();
            Console.WriteLine(
                "Recherche du service Wireless iAP..."
            );

            var result =
                await bluetoothDevice.GetRfcommServicesAsync(
                    BluetoothCacheMode.Uncached);

            RfcommDeviceService? iapService = null;

            foreach (RfcommDeviceService service in result.Services)
            {
                string uuid = service.ServiceId
                    .AsString()
                    .Trim('{', '}');

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
                Console.WriteLine(
                    "❌ Service Wireless iAP introuvable."
                );

                WaitAndExit();
                return;
            }

            Console.WriteLine();
            Console.WriteLine(
                "✅ Service Wireless iAP trouvé."
            );

            Console.WriteLine();
            Console.WriteLine(
                "Ouverture de la connexion RFCOMM..."
            );

            using StreamSocket socket =
                new StreamSocket();

            await socket.ConnectAsync(
                iapService.ConnectionHostName,
                iapService.ConnectionServiceName,
                SocketProtectionLevel
                    .BluetoothEncryptionWithAuthentication);

            Console.WriteLine();
            Console.WriteLine(
                "🟢 CONNEXION RFCOMM RÉUSSIE !"
            );

            Console.WriteLine();
            Console.WriteLine(
                "Écoute de l'iPhone pendant 5 secondes..."
            );

            Console.WriteLine(
                "(Aucune donnée ne sera envoyée.)"
            );

            Console.WriteLine();

            using Stream input =
                socket.InputStream.AsStreamForRead();

            byte[] buffer = new byte[4096];

            using CancellationTokenSource timeout =
                new CancellationTokenSource(
                    TimeSpan.FromSeconds(5));

            try
            {
                int bytesRead =
                    await input.ReadAsync(
                        buffer,
                        0,
                        buffer.Length,
                        timeout.Token);

                if (bytesRead == 0)
                {
                    Console.WriteLine(
                        "ℹ️ L'iPhone n'a envoyé aucune donnée."
                    );
                }
                else
                {
                    Console.WriteLine(
                        $"🟢 {bytesRead} octet(s) reçus !"
                    );

                    Console.WriteLine();
                    Console.WriteLine(
                        "Données reçues (HEX) :"
                    );

                    Console.WriteLine();

                    for (int i = 0; i < bytesRead; i++)
                    {
                        Console.Write(
                            $"{buffer[i]:X2} "
                        );

                        if ((i + 1) % 16 == 0)
                            Console.WriteLine();
                    }

                    Console.WriteLine();
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine(
                    "ℹ️ Aucun paquet reçu pendant les 5 secondes."
                );
            }

            iapService.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine(
                "🔴 ERREUR"
            );

            Console.WriteLine();
            Console.WriteLine(
                $"Type    : {ex.GetType().Name}"
            );

            Console.WriteLine(
                $"Message : {ex.Message}"
            );

            Console.WriteLine(
                $"Code    : 0x{ex.HResult:X8}"
            );
        }

        WaitAndExit();
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
