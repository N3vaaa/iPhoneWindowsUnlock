using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;

namespace iPhoneWindowsUnlock;

internal class Program
{
    private const string IapUuid =
        "00000000-DECA-FADE-DECA-DEAFDECACAFE";

    static async Task Main()
    {
        Console.Title = "iPhoneWindowsUnlock - iAP Diagnostic";

        Console.WriteLine("======================================");
        Console.WriteLine("     iPhoneWindowsUnlock - iAP");
        Console.WriteLine("======================================");
        Console.WriteLine();

        try
        {
            // 1. Recherche de l'iPhone
            Console.WriteLine("Recherche de l'iPhone...");

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
                Console.WriteLine("🔴 iPhone introuvable.");
                Pause();
                return;
            }

            Console.WriteLine($"🟢 iPhone : {iphone.Name}");
            Console.WriteLine();

            // 2. Ouverture Bluetooth
            using BluetoothDevice? bluetooth =
                await BluetoothDevice.FromIdAsync(
                    iphone.Id);

            if (bluetooth == null)
            {
                Console.WriteLine(
                    "🔴 Impossible d'ouvrir Bluetooth.");
                Pause();
                return;
            }

            Console.WriteLine(
                "🟢 Périphérique Bluetooth ouvert.");
            Console.WriteLine();

            // 3. Recherche du service iAP
            Console.WriteLine(
                "Recherche du service Wireless iAP...");

            var services =
                await bluetooth.GetRfcommServicesAsync(
                    BluetoothCacheMode.Uncached);

            RfcommDeviceService? iap = null;

            foreach (var service in services.Services)
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
                    "🔴 Service Wireless iAP introuvable.");
                Pause();
                return;
            }

            Console.WriteLine(
                "🟢 Service Wireless iAP trouvé.");
            Console.WriteLine();

            Console.WriteLine(
                $"UUID : {iap.ServiceId.AsString()}");

            Console.WriteLine(
                $"Hôte : {iap.ConnectionHostName}");

            Console.WriteLine(
                $"Protection : {iap.ProtectionLevel}");

            Console.WriteLine();

            // 4. Connexion
            Console.WriteLine(
                "Connexion au canal RFCOMM...");

            using StreamSocket socket =
                new StreamSocket();

            await socket.ConnectAsync(
                iap.ConnectionHostName,
                iap.ConnectionServiceName,
                SocketProtectionLevel
                    .BluetoothEncryptionWithAuthentication);

            Console.WriteLine();
            Console.WriteLine(
                "🟢 CONNEXION iAP ÉTABLIE !");
            Console.WriteLine();

            // 5. Diagnostic du flux
            using Stream input =
                socket.InputStream.AsStreamForRead();

            Console.WriteLine(
                "Écoute du flux iAP...");
            Console.WriteLine(
                "Aucune donnée n'est envoyée.");
            Console.WriteLine();

            byte[] buffer = new byte[4096];

            using CancellationTokenSource timeout =
                new CancellationTokenSource(
                    TimeSpan.FromSeconds(20));

            int totalReceived = 0;

            try
            {
                while (!timeout.IsCancellationRequested)
                {
                    int count =
                        await input.ReadAsync(
                            buffer,
                            0,
                            buffer.Length,
                            timeout.Token);

                    if (count == 0)
                    {
                        Console.WriteLine();
                        Console.WriteLine(
                            "⚠️ Le périphérique a fermé le flux.");
                        break;
                    }

                    totalReceived += count;

                    Console.WriteLine(
                        $"📥 {count} octet(s) reçu(s)");

                    PrintHex(buffer, count);

                    Console.WriteLine();
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine();
                Console.WriteLine(
                    "ℹ️ Fin du délai d'écoute.");
            }

            Console.WriteLine();
            Console.WriteLine(
                "======================================");

            Console.WriteLine(
                $"Total reçu : {totalReceived} octet(s)");

            Console.WriteLine(
                "======================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine(
                "🔴 ERREUR");

            Console.WriteLine(
                $"Type    : {ex.GetType().Name}");

            Console.WriteLine(
                $"Message : {ex.Message}");

            Console.WriteLine(
                $"HRESULT : 0x{ex.HResult:X8}");
        }

        Pause();
    }

    private static void PrintHex(
        byte[] buffer,
        int count)
    {
        for (int i = 0; i < count; i++)
        {
            Console.Write(
                $"{buffer[i]:X2} ");

            if ((i + 1) % 16 == 0)
                Console.WriteLine();
        }

        Console.WriteLine();
    }

    private static void Pause()
    {
        Console.WriteLine();
        Console.WriteLine(
            "Appuyez sur une touche pour quitter...");

        Console.ReadKey();
    }
}
