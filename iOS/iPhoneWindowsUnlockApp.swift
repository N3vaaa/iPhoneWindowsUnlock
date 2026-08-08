import SwiftUI
import LocalAuthentication
import Security

@main
struct iPhoneWindowsUnlockApp: App {

    var body: some Scene {
        WindowGroup {
            ContentView()
        }
    }
}


struct ContentView: View {

    @State private var status = "En attente"
    @State private var keyStatus = "Aucune clé"

    var body: some View {

        VStack(spacing: 25) {

            Text("iPhone Windows Unlock")
                .font(.title)
                .bold()

            Text(status)

            Text(keyStatus)
                .foregroundColor(.gray)

            Button {
                authenticate()
            } label: {
                Text("🔐 Tester Face ID")
                    .padding()
            }
            .buttonStyle(.borderedProminent)
        }
        .padding()
    }


    func authenticate() {

        let context = LAContext()

        context.evaluatePolicy(
            .deviceOwnerAuthenticationWithBiometrics,
            localizedReason: "Autoriser le déverrouillage Windows"
        ) { success, error in

            DispatchQueue.main.async {

                if success {

                    status = "✅ Face ID validé"
                    createKey()

                } else {

                    status = "❌ Face ID refusé"
                }
            }
        }
    }


    func createKey() {

        let tag = "com.n3vaaa.iPhoneWindowsUnlock.key"

        let tagData = tag.data(using: .utf8)!

        let attributes: [String: Any] = [

            kSecAttrKeyType as String:
                kSecAttrKeyTypeECSECPrimeRandom,

            kSecAttrKeySizeInBits as String:
                256,

            kSecPrivateKeyAttrs as String: [

                kSecAttrIsPermanent as String:
                    true,

                kSecAttrApplicationTag as String:
                    tagData
            ]
        ]


        var error: Unmanaged<CFError>?

        if SecKeyCreateRandomKey(
            attributes as CFDictionary,
            &error
        ) != nil {

            keyStatus = "🔑 Clé créée"

        } else {

            keyStatus = "❌ Erreur création clé"
        }
    }
}
