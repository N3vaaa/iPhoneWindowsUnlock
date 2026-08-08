namespace iPhoneWindowsUnlock.UI;

public sealed class AppStatus
{
    public string IPhoneName { get; set; } = "Aucun iPhone";

    public bool IPhoneDetected { get; set; }

    public bool BluetoothAvailable { get; set; }

    public bool IapAvailable { get; set; }

    public string ConnectionStatus
    {
        get
        {
            if (!IPhoneDetected)
                return "iPhone non détecté";

            if (!BluetoothAvailable)
                return "Bluetooth indisponible";

            if (!IapAvailable)
                return "Wireless iAP indisponible";

            return "iPhone prêt";
        }
    }
}
