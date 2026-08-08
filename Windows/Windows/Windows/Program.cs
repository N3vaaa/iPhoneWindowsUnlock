using System;

namespace iPhoneWindowsUnlock;

internal class Program
{
    static void Main()
    {
        Console.Title = "iPhoneWindowsUnlock";

        Console.WriteLine("=================================");
        Console.WriteLine("       iPhoneWindowsUnlock");
        Console.WriteLine("=================================");
        Console.WriteLine();

        Console.WriteLine("État de l'iPhone :");
        Console.WriteLine("❌ Non détecté");
        Console.WriteLine();

        Console.WriteLine("Appuyez sur une touche pour quitter...");
        Console.ReadKey();
    }
}
