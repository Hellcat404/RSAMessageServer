using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MessageServer {
    internal class Connection {
        bool _disposing = false;

        Server _server;

        TcpClient client;
        NetworkStream stream;

        public Connection(TcpClient client, Server server) {
            this._server = server;
            this.client = client;
            stream = this.client.GetStream();
        }

        public void Handshake() {
            int opcode = stream.ReadByte();
            Console.WriteLine("op: " + opcode);
            if (opcode == 10)
                SendData(_server._crypto.getPubKey());

            Task.Run(Listen);
        }

        public void Listen() {
            while (!_disposing) {
                int opcode = stream.ReadByte();
                Console.WriteLine("op: " + opcode);
                if (opcode == 0) {
                    byte[] buffer = new byte[1024];
                    int termCount = 0;
                    int writtenBytes = 0;
                    do {
                        buffer[writtenBytes] = (byte)stream.ReadByte();
                        if (buffer[writtenBytes] == 0)
                            termCount++;
                        else
                            termCount = 0;
                        writtenBytes++;
                    } while (termCount < 4 && writtenBytes < 1024);
                    byte[] output = new byte[writtenBytes - 4];
                    for (int i = 0; i < output.Length; i++) {
                        output[i] = buffer[i];
                    }
                    byte[] decryptedBytes = _server._crypto.DecryptBytes(output);
                    _server.SendAll(decryptedBytes);
                } else if (opcode == 100)
                    Dispose();
            }
            if (client.Connected)
                client.Close();
        }

        public void SendDataEnc(byte[] data) {
            //TODO: Store individual connection's public key (sent by client on handshake)
        }

        public void SendData(byte[] data) {
            if (client.Connected) {
                byte[] send = new byte[data.Length + 4];
                for (int i = 0; i < data.Length; i++) {
                    send[i] = data[i];
                }
                stream.Write(send);
            }
        }

        public void Dispose() { 
            _disposing = true;
        }
    }
}
