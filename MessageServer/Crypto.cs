using System.Runtime.Versioning;
using System.Text;
using System.Security.Cryptography;

namespace MessageServer {
    [SupportedOSPlatform("windows")]
    public class Crypto {
        RSA? rsa = null;
        float ver = 0;

        public Crypto(float rsaKeyVersion = 0) {
            //TODO: Work out a way to check if keys need to be updated - static class & set version at start?
            if(rsaKeyVersion != 0)
                ver = rsaKeyVersion;

            //MSKeys - Message Server keys, this will contain the server's config for RSACng (This includes the private key!)
            string keyFile = "MSKeys_" + ver;
            rsa = InitRSA(keyFile);
        }

        private RSA InitRSA(string keyFile) {
            //Instantiate RSACng with key size of 2048 bits (256 bytes)
            rsa = RSACng.Create(2048);

            //If we already have a keyfile, read the keyfile into the rsa, this overwrites our instantiated RSA
            //If we don't - Write the new instantiated RSA config to a new keyfile
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

        //Padding type for encryption and decryption will assume Pkcs1

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
            return Encoding.UTF8.GetString(DecryptBytes(data));
        }
    }
}
