using System.Net;
using System.Net.Sockets;
using System.Text;

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

            Console.WriteLine($"[{client.Client.RemoteEndPoint?.ToString()}] Connection established!");
            Task.Run(Listen);
        }

        public void Listen() {
            while (!_disposing) {
                int opcode = stream.ReadByte();
                if (opcode == 0) {

                    List<byte[]> data = ReadData();

                    byte[] buffer = data[0];

                    byte[] decryptedBytes = _server._crypto.DecryptBytes(buffer);
                    Console.WriteLine($"From client: {Encoding.UTF8.GetString(decryptedBytes)}");
                    _server.SendAll(decryptedBytes);
                } else if (opcode == 100)
                    Dispose();
            }
            if (client.Connected)
                client.Close();
        }

        //Read data up to 4 terminator bytes, split data read into 256 byte arrays to decrypt in chunks
        //This will allow for more data to be sent at one time and allow us to throw away the termination bytes easily (final array will be 4 bytes long)
        private List<byte[]> ReadData() {
            List<byte[]> data = new List<byte[]>();

            //termCount - Terminator count, empty bytes read - 4 in a row to end a message
            int termCount = 0;
            do {
                //256 byte buffer - Reads one "chunk" of decryptable data
                byte[] buffer = new byte[256];

                //Attempt to read one chunk of data from the stream, if 4 termination bytes are read, break (We're done reading data)
                for (int i = 0; i < 256; i++) {
                    buffer[i] = (byte)stream.ReadByte();

                    if (buffer[i] != 0) {
                        termCount = 0;
                    } else {
                        termCount++;
                        if (termCount == 4) {
                            break;
                        }
                    }
                }

                //Only add data to the list if it is NOT the terminator
                if(termCount < 4)
                    data.Add(buffer);
            } while (termCount < 4);

            return data;
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
