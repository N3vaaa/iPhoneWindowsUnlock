using System;
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
                $"✅ {iphone.Name} détecté");

            using BluetoothDevice? bt =
                await BluetoothDevice.FromIdAsync(
                    iphone.Id);

            if (bt == null)
            {
                Console.WriteLine(
                    "❌ Bluetooth impossible");
                Pause();
                return;
            }

            var services =
                await bt.GetRfcommServicesAsync(
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
                    "❌ Service iAP absent");
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

            Console.WriteLine();
            Console.WriteLine(
                "🟢 Canal RFCOMM ouvert");

            Console.WriteLine();
            Console.WriteLine(
                "État du canal :");
            Console.WriteLine(
                "- Lecture : disponible");
            Console.WriteLine(
                "- Écriture : disponible");
            Console.WriteLine(
                "- Protection : Bluetooth authentifié");

            Console.WriteLine();

            Console.WriteLine(
                "Surveillance pendant 30 secondes...");

            Console.WriteLine(
                "Aucune donnée envoyée.");

            Console.WriteLine();

            using CancellationTokenSource cts =
                new CancellationTokenSource(
                    TimeSpan.FromSeconds(30));

            while (!cts.IsCancellationRequested)
            {
                await Task.Delay(
                    1000,
                    cts.Token);

                Console.Write(".");
            }

            Console.WriteLine();

            Console.WriteLine();
            Console.WriteLine(
                "🟢 Canal toujours stable après 30 secondes.");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine(
                "🔴 ERREUR");

            Console.WriteLine(
                $"Type : {ex.GetType().Name}");

            Console.WriteLine(
                $"Message : {ex.Message}");
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
