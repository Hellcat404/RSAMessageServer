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
            _server = server;
            this.client = client;
            stream = this.client.GetStream();
        }

        public void Handshake() {
            int opcode = stream.ReadByte();
            if (opcode == 10)
                SendKey(_server._crypto.getPubKey());

            Task.Run(Listen);
        }

        public void Listen() {
            while (!_disposing) {
                int opcode = stream.ReadByte();
                if (opcode == 0) {
                    //1024 byte buffer - Arbitrary number, allows for short-mid sized messages to be read
                    byte[] buffer = new byte[1024];
                    //termCount - Terminator count, empty bytes read - 4 in a row to end a message to the server
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
                    //stores the message with terminators (trailing empty bytes) stripped
                    byte[] output = StreamUtils.RemoveTerminator(buffer, writtenBytes);

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

        public void SendData(byte[] data, int opcode) {
            if (client.Connected) {
                byte[] buffer = StreamUtils.AddOpcode(data, opcode);
                byte[] send = StreamUtils.AddTerminator(buffer);
                stream.Write(send);
            }
        }

        public void SendKey(byte[] data) {
            if (client.Connected) {
                byte[] send = StreamUtils.AddTerminator(data);
                stream.Write(send);
            }
        }

        public void Dispose() { 
            _disposing = true;
        }
    }
}
