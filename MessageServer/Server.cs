using System.Net;
using System.Net.Sockets;

namespace MessageServer {
    public class Server {
        bool _disposing = false;

        private List<Connection> connections = new List<Connection>();

        public Crypto _crypto = new Crypto(0.1f);
        TcpListener _server = new TcpListener(Dns.GetHostEntry("localhost").AddressList[0], 4434);

        public void Start() {
            Task.Run(Listen);
        }

        private void Listen() {
            _server.Start();
            Console.WriteLine("Server started.");
            while (!_disposing) {
                if (_server.Pending()) {
                    Connection c = new Connection(_server.AcceptTcpClient(), this);
                    connections.Add(c);
                    Task.Run(c.Handshake);
                }
            }
            DisposeConnections();
            _server.Stop();
            Console.WriteLine("Server stopped.");
        }

        public void SendAll(byte[] data) {
            foreach(var connection in connections) {
                connection.SendData(data);
            }
        }

        private void DisposeConnections() {
            Console.WriteLine("Disposing connections...");
            foreach (var connection in connections) {
                connection.Dispose();
            }
            connections.Clear();
        }

        public void Dispose() {
            Console.WriteLine("Disposing server...");
            this._disposing = true;
        }

    }
}