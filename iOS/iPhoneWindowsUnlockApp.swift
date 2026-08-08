import SwiftUI
import CoreBluetooth

let unlockServiceUUID = CBUUID(string: "7A1E0001-5B7A-4E91-9D21-123456789001")
let unlockCharacteristicUUID = CBUUID(string: "7A1E0002-5B7A-4E91-9D21-123456789001")

@main
struct iPhoneWindowsUnlockApp: App {
    var body: some Scene {
        WindowGroup {
            ContentView()
        }
    }
}

struct ContentView: View {
    @StateObject private var bluetooth = BluetoothManager()

    var body: some View {
        VStack(spacing: 20) {
            Text("iPhone Windows Unlock")
                .font(.title)
                .bold()

            Text(bluetooth.status)
                .font(.headline)

            Circle()
                .fill(bluetooth.isReady ? .green : .orange)
                .frame(width: 30, height: 30)

            Text(bluetooth.isReady
                 ? "Bluetooth LE actif"
                 : "Initialisation du Bluetooth…")

            Text("En attente du PC Windows…")
                .foregroundStyle(.secondary)
        }
        .padding()
    }
}

final class BluetoothManager: NSObject, ObservableObject {
    @Published var status = "Initialisation…"
    @Published var isReady = false

    private var peripheralManager: CBPeripheralManager!

    private var responseCharacteristic: CBMutableCharacteristic!

    override init() {
        super.init()

        peripheralManager = CBPeripheralManager(
            delegate: self,
            queue: nil
        )
    }

    private func startBluetoothService() {
        let properties: CBCharacteristicProperties = [
            .write,
            .writeWithoutResponse,
            .notify
        ]

        let permissions: CBAttributePermissions = [
            .readable,
            .writeable
        ]

        responseCharacteristic = CBMutableCharacteristic(
            type: unlockCharacteristicUUID,
            properties: properties,
            value: nil,
            permissions: permissions
        )

        let service = CBMutableService(
            type: unlockServiceUUID,
            primary: true
        )

        service.characteristics = [responseCharacteristic]

        peripheralManager.removeAllServices()
        peripheralManager.add(service)

        peripheralManager.startAdvertising([
            CBAdvertisementDataLocalNameKey: "iPhone Windows Unlock",
            CBAdvertisementDataServiceUUIDsKey: [unlockServiceUUID]
        ])

        DispatchQueue.main.async {
            self.status = "Service BLE actif"
            self.isReady = true
        }
    }
}

extension BluetoothManager: CBPeripheralManagerDelegate {

    func peripheralManagerDidUpdateState(
        _ peripheral: CBPeripheralManager
    ) {
        DispatchQueue.main.async {
            switch peripheral.state {

            case .poweredOn:
                self.startBluetoothService()

            case .poweredOff:
                self.status = "Bluetooth désactivé"
                self.isReady = false

            case .unauthorized:
                self.status = "Autorisation Bluetooth refusée"
                self.isReady = false

            case .unsupported:
                self.status = "Bluetooth LE non supporté"
                self.isReady = false

            case .resetting:
                self.status = "Bluetooth en réinitialisation…"
                self.isReady = false

            case .unknown:
                self.status = "État Bluetooth inconnu"
                self.isReady = false

            @unknown default:
                self.status = "État Bluetooth inconnu"
                self.isReady = false
            }
        }
    }

    func peripheralManager(
        _ peripheral: CBPeripheralManager,
        didReceiveWrite requests: [CBATTRequest]
    ) {
        for request in requests {

            guard request.characteristic.uuid == unlockCharacteristicUUID else {
                peripheral.respond(
                    to: request,
                    withResult: .requestNotSupported
                )
                continue
            }

            if let data = request.value,
               let message = String(data: data, encoding: .utf8) {

                print("Message reçu : \(message)")

                if message == "PING" {
                    let response = Data("PONG".utf8)

                    peripheral.updateValue(
                        response,
                        for: responseCharacteristic,
                        onSubscribedCentrals: nil
                    )
                }
            }

            peripheral.respond(
                to: request,
                withResult: .success
            )
        }
    }
}
