using System;
using System.Threading.Tasks;
using iPhoneWindowsUnlock.Bluetooth;

namespace iPhoneWindowsUnlock;

internal class Program
{
    static async Task Main()
    {
        Console.Title = "iPhoneWindowsUnlock";

        Console.WriteLine("======================================");
        Console.WriteLine("        iPhoneWindowsUnlock");
        Console.WriteLine("======================================");
        Console.WriteLine();

        var connection = new IPhoneConnection();

        Console.WriteLine("🔎 Recherche de l'iPhone...");
        Console.WriteLine();

        try
        {
            await connection.DetectAsync();

            if (!connection.IsDetected)
            {
                Console.WriteLine(
                    "🔴 Aucun iPhone détecté.");
            }
            else
            {
                Console.WriteLine(
                    $"🟢 iPhone : {connection.IPhoneName}");

                Console.WriteLine(
                    connection.IsBluetoothAvailable
                        ? "🟢 Bluetooth : disponible"
                        : "🔴 Bluetooth : indisponible");

                Console.WriteLine(
                    connection.IsIapAvailable
                        ? "🟢 Wireless iAP : disponible"
                        : "🟠 Wireless iAP : indisponible");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("🔴 Erreur");
            Console.WriteLine(ex.Message);
        }

        Console.WriteLine();
        Console.WriteLine("--------------------------------------");
        Console.WriteLine("Appuyez sur une touche pour quitter...");
        Console.ReadKey();
    }
}
