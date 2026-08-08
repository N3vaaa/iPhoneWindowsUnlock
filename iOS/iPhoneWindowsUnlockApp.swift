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



    // Vérifie si l'iPhone possède déjà une identité

    func checkKey() {

        let tag =
        "com.n3vaaa.iPhoneWindowsUnlock.key"


        let query: [String: Any] = [

            kSecClass as String:
                kSecClassKey,

            kSecAttrApplicationTag as String:
                tag.data(using: .utf8)!,

            kSecReturnRef as String:
                true
        ]


        var result: AnyObject?


        let status =
        SecItemCopyMatching(
            query as CFDictionary,
            &result
        )


        if status == errSecSuccess {

            self.status =
            "✅ iPhone déjà enregistré"

            self.keyStatus =
            "Clé sécurisée disponible"

        } else {

            self.status =
            "Aucune identité trouvée"

        }
    }



    // Validation Face ID

    func authenticate() {


        let context = LAContext()


        context.evaluatePolicy(

            .deviceOwnerAuthenticationWithBiometrics,

            localizedReason:
                "Autoriser l'enregistrement de cet iPhone"

        ) { success, error in


            DispatchQueue.main.async {


                if success {


                    createKey()


                } else {


                    status =
                    "❌ Face ID refusé"

                }

            }
        }
    }




    // Création de l'identité

    func createKey() {


        let tag =
        "com.n3vaaa.iPhoneWindowsUnlock.key"


        let attributes: [String: Any] = [


            kSecAttrKeyType as String:
                kSecAttrKeyTypeECSECPrimeRandom,


            kSecAttrKeySizeInBits as String:
                256,


            kSecPrivateKeyAttrs as String: [


                kSecAttrIsPermanent as String:
                    true,


                kSecAttrApplicationTag as String:
                    tag.data(using: .utf8)!

            ]
        ]



        var error:
        Unmanaged<CFError>?



        if SecKeyCreateRandomKey(

            attributes as CFDictionary,

            &error

        ) != nil {



            status =
            "✅ iPhone enregistré"


            keyStatus =
            "Identité cryptographique créée"



        } else {


            status =
            "❌ Erreur création clé"

        }
    }
}
