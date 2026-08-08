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
    @State private var status = "Vérification..."
    @State private var keyStatus = ""

    private let keyTag = "com.n3vaaa.iPhoneWindowsUnlock.key"

    var body: some View {
        VStack(spacing: 25) {
            Text("iPhone Windows Unlock")
                .font(.title)
                .bold()

            Text(status)
                .font(.headline)
                .multilineTextAlignment(.center)

            Text(keyStatus)
                .foregroundColor(.secondary)
                .multilineTextAlignment(.center)

            Button {
                authenticate()
            } label: {
                Text("🔐 Enregistrer cet iPhone")
                    .padding()
            }
            .buttonStyle(.borderedProminent)
        }
        .padding()
        .onAppear {
            checkKey()
        }
    }

    // Vérifie si une identité cryptographique existe déjà
    private func checkKey() {
        guard let tagData = keyTag.data(using: .utf8) else {
            status = "❌ Erreur interne"
            return
        }

        let query: [String: Any] = [
            kSecClass as String: kSecClassKey,
            kSecAttrApplicationTag as String: tagData,
            kSecReturnRef as String: true
        ]

        var result: AnyObject?

        let resultCode = SecItemCopyMatching(
            query as CFDictionary,
            &result
        )

        DispatchQueue.main.async {
            if resultCode == errSecSuccess {
                status = "✅ iPhone déjà enregistré"
                keyStatus = "Clé sécurisée disponible"
            } else {
                status = "Aucune identité trouvée"
                keyStatus = ""
            }
        }
    }

    // Demande l'authentification Face ID
    private func authenticate() {
        let context = LAContext()
        var authError: NSError?

        guard context.canEvaluatePolicy(
            .deviceOwnerAuthenticationWithBiometrics,
            error: &authError
        ) else {
            DispatchQueue.main.async {
                status = "❌ Face ID indisponible"
                keyStatus = authError?.localizedDescription ?? "Authentification biométrique indisponible"
            }
            return
        }

        context.evaluatePolicy(
            .deviceOwnerAuthenticationWithBiometrics,
            localizedReason: "Autoriser l'enregistrement de cet iPhone"
        ) { success, error in

            DispatchQueue.main.async {
                if success {
                    createKey()
                } else {
                    status = "❌ Face ID refusé"
                    keyStatus = error?.localizedDescription ?? ""
                }
            }
        }
    }

    // Création de la clé cryptographique
    private func createKey() {
        guard let tagData = keyTag.data(using: .utf8) else {
            status = "❌ Erreur interne"
            return
        }

        // Vérifie d'abord si la clé existe déjà.
        let existingQuery: [String: Any] = [
            kSecClass as String: kSecClassKey,
            kSecAttrApplicationTag as String: tagData,
            kSecReturnRef as String: true
        ]

        var existingKey: AnyObject?

        let existingStatus = SecItemCopyMatching(
            existingQuery as CFDictionary,
            &existingKey
        )

        if existingStatus == errSecSuccess {
            status = "✅ iPhone déjà enregistré"
            keyStatus = "Identité cryptographique disponible"
            return
        }

        let attributes: [String: Any] = [
            kSecAttrKeyType as String: kSecAttrKeyTypeECSECPrimeRandom,
            kSecAttrKeySizeInBits as String: 256,
            kSecPrivateKeyAttrs as String: [
                kSecAttrIsPermanent as String: true,
                kSecAttrApplicationTag as String: tagData
            ]
        ]

        var keyError: Unmanaged<CFError>?

        let privateKey = SecKeyCreateRandomKey(
            attributes as CFDictionary,
            &keyError
        )

        if privateKey != nil {
            status = "✅ iPhone enregistré"
            keyStatus = "Identité cryptographique créée"
        } else {
            status = "❌ Erreur création clé"

            if let keyError {
                keyStatus = (keyError.takeRetainedValue() as Error).localizedDescription
            }
        }
    }
}
