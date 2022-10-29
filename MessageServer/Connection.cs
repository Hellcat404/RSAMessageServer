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
            if (opcode == 10)
                SendData(_server._crypto.getPubKey());

            Task.Run(Listen);
        }

        public void Listen() {
            while (!_disposing) {
                int opcode = stream.ReadByte();
                Console.WriteLine("op: " + opcode);
                if(opcode == 0) {
                    byte[] buffer = new byte[255];
                    int termCount = 0;
                    int writtenBytes = 0;
                    do {
                        buffer[writtenBytes] = (byte)stream.ReadByte();
                        if (buffer[writtenBytes] == 0)
                            termCount++;
                        else
                            termCount = 0;
                        writtenBytes++;
                    } while (termCount < 4 && writtenBytes < 255);

                    byte[] decryptedBytes = _server._crypto.DecryptBytes(buffer);
                    _server.SendAll(decryptedBytes);
                }
            }
            if (client.Connected)
                client.Close();
        }

        public void SendDataEnc(byte[] data) {
            //TODO: Store individual connection's public key (sent by client on handshake)
        }

        public void SendData(byte[] data) {
            if(client.Connected)
                stream.Write(data);
        }

        public void Dispose() { 
            _disposing = true;
        }
    }
}
