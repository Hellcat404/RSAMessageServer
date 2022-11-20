namespace MessageServer {
    internal static class StreamUtils {
        public static byte[] AddTerminator(byte[] data) {
            //Terminator is 4 empty bytes (may change later)
            //Create a new byte array with 4 extra bytes, add all data and return the array. extra bytes start at 0, no need to touch them.
            byte[] output = new byte[data.Length + 4];
            for (int i = 0; i < data.Length; i++) {
                output[i] = data[i];
            }
            return output;
        }

        public static byte[] RemoveTerminator(byte[] data, int length) {
            //Add terminator in reverse, 4 end bytes are empty, strip them off.
            byte[] output = new byte[length - 4];
            for (int i = 0; i < output.Length; i++) {
                output[i] = data[i];
            }
            return output;
        }

        //Opcodes,
        //00 - Message
        //10 - RSA Request
        //100 - Client Disconnect (code will change - testing only)

        public static byte[] AddOpcode(byte[] data, int opcode) {
            //Opcode is added before data to allow the server/client to understand what the data will be (Message, RSA request, etc)
            byte[] output = new byte[data.Length + 1];
            output[0] = (byte)opcode;
            for (int i = 1; i < output.Length; i++) {
                output[i] = data[i - 1];
            }
            return output;
        }
    }
}
