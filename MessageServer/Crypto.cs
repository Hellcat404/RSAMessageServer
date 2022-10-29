using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace MessageServer {
    [SupportedOSPlatform("windows")]
    public class Crypto {
        RSA? rsa = null;
        float ver = 0;

        public Crypto(float rsaKeyVersion = 0) {
            if(rsaKeyVersion != 0)
                ver = rsaKeyVersion;

            string keyFile = "MSKeys_" + ver;
            rsa = InitRSA(keyFile);
        }

        private RSA InitRSA(string keyFile) {
            rsa = RSACng.Create();

            if (File.Exists(keyFile))
                rsa.FromXmlString(File.ReadAllText(keyFile));
            else
                File.WriteAllText(keyFile, rsa.ToXmlString(true));

            return rsa;
        }

        public byte[] getPubKey() {
            if (rsa == null) throw new Exception("RSA Does not exist!");
            return rsa.ExportRSAPublicKey();
        }

        public byte[] EncryptString(string data) {
            if (rsa == null) throw new Exception("RSA Does not exist!");
            return rsa.Encrypt(Encoding.UTF8.GetBytes(data), RSAEncryptionPadding.Pkcs1);
        }

        public byte[] DecryptBytes(byte[] data) {
            if (rsa == null) throw new Exception("RSA Does not exist!");
            return rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1);
        }

        public string DecryptString(byte[] data) {
            if (rsa == null) throw new Exception("RSA Does not exist!");
            return Encoding.UTF8.GetString(rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1));
        }
    }
}
