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
    @State private var keyStatus = "Aucune clé créée"

    var body: some View {

        VStack(spacing: 25) {

            Text("iPhone Windows Unlock")
                .font(.title)
                .bold()


            Text(status)
                .font(.headline)


            Text(keyStatus)
                .foregroundColor(.secondary)


            Button {
                authenticate()
            } label: {
                Text("🔐 Authentifier avec Face ID")
                    .padding()
            }
            .buttonStyle(.borderedProminent)

        }
        .padding()
    }


    func authenticate() {

        let context = LAContext()

        var error: NSError?

        guard context.canEvaluatePolicy(
            .deviceOwnerAuthenticationWithBiometrics,
            error: &error
        ) else {

            status = "Face ID indisponible"
            return
        }


        context.evaluatePolicy(
            .deviceOwnerAuthenticationWithBiometrics,
            localizedReason: "Autoriser le déverrouillage Windows"
        ) { success, error in


            DispatchQueue.main.async {

                if success {

                    status = "✅ Face ID validé"

                    createKey()

                } else {

                    status = "❌ Authentification refusée"
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
                    tagData,

                kSecAccessControl as String:
                    SecAccessControlCreateWithFlags(
                        nil,
                        kSecAttrAccessibleWhenUnlockedThisDeviceOnly,
                        .biometryCurrentSet,
                        nil
                    )!
            ]
        ]


        var error: Unmanaged<CFError>?


        if let privateKey =
            SecKeyCreateRandomKey(
                attributes as CFDictionary,
                &error
            ) {

            _ = privateKey

            keyStatus =
            "🔑 Clé sécurisée créée"

        } else {

            keyStatus =
            "Erreur création clé"
        }
    }
}
