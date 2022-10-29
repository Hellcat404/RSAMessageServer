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

        public void Listen() {
            while (!_disposing) {
                int opcode = stream.ReadByte();
                Console.WriteLine("op: " + opcode);
            }
            if (client.Connected)
                client.Close();
        }

        public void SendDataEnc(byte[] data) {
            //TODO: Store individual connection's public key (sent by client on handshake)
        }

        public void SendData(byte[] data) {
            stream.Write(data);
        }

        public void Dispose() { 
            _disposing = true;
        }
    }
}
