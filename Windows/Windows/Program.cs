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
            Console.WriteLine("Recherche iPhone...");

            var devices =
                await DeviceInformation.FindAllAsync(
                    BluetoothDevice.GetDeviceSelector());

            DeviceInformation? iphone = null;

            foreach (var device in devices)
            {
                if (device.Name.Contains(
                    "iPhone",
                    StringComparison.OrdinalIgnoreCase))
                {
                    iphone = device;
                    break;
                }
            }

            if (iphone == null)
            {
                Console.WriteLine(
                    "❌ iPhone non trouvé");
                Exit();
                return;
            }

            Console.WriteLine(
                $"✅ Trouvé : {iphone.Name}");

            using BluetoothDevice? bluetooth =
                await BluetoothDevice.FromIdAsync(
                    iphone.Id);

            if (bluetooth == null)
            {
                Console.WriteLine(
                    "❌ Bluetooth impossible");
                Exit();
                return;
            }

            var services =
                await bluetooth.GetRfcommServicesAsync(
                    BluetoothCacheMode.Uncached);

            RfcommDeviceService? iap = null;

            foreach (var service in services.Services)
            {
                string id =
                    service.ServiceId
                    .AsString()
                    .Trim('{', '}');

                if (id.Equals(
                    IapServiceUuid,
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
                    "❌ iAP introuvable");
                Exit();
                return;
            }

            Console.WriteLine();
            Console.WriteLine(
                "Connexion Wireless iAP...");

            using StreamSocket socket =
                new StreamSocket();

            await socket.ConnectAsync(
                iap.ConnectionHostName,
                iap.ConnectionServiceName,
                SocketProtectionLevel
                    .BluetoothEncryptionWithAuthentication);

            Console.WriteLine(
                "🟢 Connexion établie");

            Console.WriteLine();
            Console.WriteLine(
                "Écoute du canal pendant 15 secondes...");
            Console.WriteLine(
                "Aucune donnée envoyée.");

            Console.WriteLine();

            using Stream input =
                socket.InputStream.AsStreamForRead();

            byte[] buffer = new byte[1024];

            using CancellationTokenSource timeout =
                new CancellationTokenSource(
                    TimeSpan.FromSeconds(15));

            try
            {
                int total = 0;

                while (!timeout.IsCancellationRequested)
                {
                    int read =
                        await input.ReadAsync(
                            buffer,
                            0,
                            buffer.Length,
                            timeout.Token);

                    if (read <= 0)
                        break;

                    total += read;

                    Console.WriteLine(
                        $"Données reçues : {read} octets");

                    for (int i = 0; i < read; i++)
                    {
                        Console.Write(
                            $"{buffer[i]:X2} ");
                    }

                    Console.WriteLine();
                }

                if (total == 0)
                {
                    Console.WriteLine(
                        "ℹ️ Aucun paquet reçu.");
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine(
                    "ℹ️ Fin du délai d'écoute.");
            }

            iap.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine(
                "🔴 Erreur");
            Console.WriteLine(
                ex.Message);
        }

        Exit();
    }

    static void Exit()
    {
        Console.WriteLine();
        Console.WriteLine(
            "Appuyez sur une touche pour quitter...");
        Console.ReadKey();
    }
}
