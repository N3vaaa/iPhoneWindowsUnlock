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
        Console.Title = "iPhoneWindowsUnlock";

        Console.WriteLine("==============================");
        Console.WriteLine("     iPhoneWindowsUnlock");
        Console.WriteLine("==============================");
        Console.WriteLine();

        try
        {
            Console.WriteLine("Recherche iPhone...");

            var devices =
                await DeviceInformation.FindAllAsync(
                    BluetoothDevice.GetDeviceSelector());

            DeviceInformation? iphone = null;

            foreach (var d in devices)
            {
                if (d.Name.Contains(
                    "iPhone",
                    StringComparison.OrdinalIgnoreCase))
                {
                    iphone = d;
                    break;
                }
            }

            if (iphone == null)
            {
                Console.WriteLine(
                    "❌ iPhone introuvable");
                Pause();
                return;
            }

            Console.WriteLine(
                $"✅ Trouvé : {iphone.Name}");

            using BluetoothDevice? bt =
                await BluetoothDevice.FromIdAsync(
                    iphone.Id);

            if (bt == null)
            {
                Console.WriteLine(
                    "❌ Bluetooth indisponible");
                Pause();
                return;
            }

            var result =
                await bt.GetRfcommServicesAsync(
                    BluetoothCacheMode.Uncached);

            RfcommDeviceService? iap = null;

            foreach (var service in result.Services)
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
                    "❌ Wireless iAP absent");
                Pause();
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
                "🟢 Canal ouvert");

            Console.WriteLine();
            Console.WriteLine(
                "Surveillance du canal...");
            Console.WriteLine(
                "Durée : 60 secondes");
            Console.WriteLine();

            using Stream input =
                socket.InputStream.AsStreamForRead();

            byte[] buffer = new byte[512];

            using CancellationTokenSource cancel =
                new CancellationTokenSource(
                    TimeSpan.FromSeconds(60));

            try
            {
                while (!cancel.IsCancellationRequested)
                {
                    if (socket.Information == null)
                    {
                        Console.WriteLine(
                            "⚠️ Socket fermé");
                        break;
                    }

                    Console.Write(".");
                    
                    await Task.Delay(1000);

                    if (input.CanRead &&
                        input.Length > 0)
                    {
                        int size =
                            await input.ReadAsync(
                                buffer,
                                0,
                                buffer.Length);

                        Console.WriteLine();

                        Console.WriteLine(
                            $"📥 {size} octet(s) reçu(s)");

                        for (int i = 0; i < size; i++)
                        {
                            Console.Write(
                                $"{buffer[i]:X2} ");
                        }

                        Console.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine(
                    "⚠️ Lecture interrompue");
                Console.WriteLine(
                    ex.Message);
            }

            Console.WriteLine();
            Console.WriteLine(
                "Fin de surveillance.");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine(
                "🔴 ERREUR");
            Console.WriteLine(
                $"Type : {ex.GetType().Name}");
            Console.WriteLine(
                ex.Message);
        }

        Pause();
    }

    static void Pause()
    {
        Console.WriteLine();
        Console.WriteLine(
            "Appuyez sur une touche pour quitter...");
        Console.ReadKey();
    }
}
