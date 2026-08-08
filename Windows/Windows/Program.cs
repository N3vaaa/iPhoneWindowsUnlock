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

            string selector =
                BluetoothDevice.GetDeviceSelector();

            DeviceInformationCollection devices =
                await DeviceInformation.FindAllAsync(selector);

            DeviceInformation? iPhone = null;

            foreach (var device in devices)
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
                Console.WriteLine("❌ iPhone introuvable");
                Exit();
                return;
            }

            Console.WriteLine();
            Console.WriteLine($"✅ {iPhone.Name} trouvé");

            using BluetoothDevice? bluetooth =
                await BluetoothDevice.FromIdAsync(iPhone.Id);

            if (bluetooth == null)
            {
                Console.WriteLine("❌ Bluetooth impossible");
                Exit();
                return;
            }

            var services =
                await bluetooth.GetRfcommServicesAsync(
                    BluetoothCacheMode.Uncached);

            RfcommDeviceService? iap = null;

            foreach (var service in services.Services)
            {
                string uuid =
                    service.ServiceId.AsString()
                    .Trim('{', '}');

                if (uuid.Equals(
                    IapServiceUuid,
                    StringComparison.OrdinalIgnoreCase))
                {
                    iap = service;
                }
                else
                {
                    service.Dispose();
                }
            }

            if (iap == null)
            {
                Console.WriteLine(
                    "❌ Service iAP absent");
                Exit();
                return;
            }

            Console.WriteLine();
            Console.WriteLine(
                "Connexion Wireless iAP..."
            );

            using StreamSocket socket =
                new StreamSocket();

            await socket.ConnectAsync(
                iap.ConnectionHostName,
                iap.ConnectionServiceName,
                SocketProtectionLevel
                    .BluetoothEncryptionWithAuthentication);

            Console.WriteLine(
                "🟢 Connexion établie"
            );

            Console.WriteLine();

            Stream output =
                socket.OutputStream.AsStreamForWrite();

            Stream input =
                socket.InputStream.AsStreamForRead();

            /*
             * Test neutre :
             * aucun ordre iPhone,
             * juste une vérification du canal.
             */
            byte[] test =
            {
                0x00
            };

            Console.WriteLine(
                "Envoi test canal..."
            );

            await output.WriteAsync(
                test,
                0,
                test.Length);

            await output.FlushAsync();

            Console.WriteLine(
                "Test envoyé."
            );

            Console.WriteLine(
                "Attente réponse (5 secondes)..."
            );

            byte[] buffer = new byte[256];

            using CancellationTokenSource cts =
                new CancellationTokenSource(
                    TimeSpan.FromSeconds(5));

            try
            {
                int count =
                    await input.ReadAsync(
                        buffer,
                        0,
                        buffer.Length,
                        cts.Token);

                Console.WriteLine();

                if (count > 0)
                {
                    Console.WriteLine(
                        $"🟢 Réponse reçue : {count} octets"
                    );

                    for (int i = 0; i < count; i++)
                    {
                        Console.Write(
                            $"{buffer[i]:X2} "
                        );
                    }

                    Console.WriteLine();
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine(
                    "ℹ️ Aucune réponse reçue."
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("🔴 ERREUR");
            Console.WriteLine(ex.Message);
        }

        Exit();
    }

    static void Exit()
    {
        Console.WriteLine();
        Console.WriteLine(
            "Appuyez sur une touche pour quitter..."
        );

        Console.ReadKey();
    }
}
