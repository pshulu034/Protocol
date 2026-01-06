using System;
using System.Buffers.Binary;
using System.Text.RegularExpressions;

namespace Fins.Core
{
    public static class FinsFrameBuilder
    {
        private const byte ICF = 0x80;
        private const byte RSV = 0x00;
        private const byte GCT = 0x02;

        public static byte[] BuildMemoryAreaRead(
            byte destNode, byte srcNode, byte serviceId,
            string address, ushort count,
            byte destUnit = 0x00, byte srcUnit = 0x00,
            byte destNetwork = 0x00, byte srcNetwork = 0x00)
        {
            ParseAddress(address, out byte areaCode, out ushort wordAddress, out byte bitOffset);

            var buffer = new byte[10 + 2 + 1 + 3 + 2];
            buffer[0] = ICF;
            buffer[1] = RSV;
            buffer[2] = GCT;
            buffer[3] = destNetwork;
            buffer[4] = destNode;
            buffer[5] = destUnit;
            buffer[6] = srcNetwork;
            buffer[7] = srcNode;
            buffer[8] = srcUnit;
            buffer[9] = serviceId;
            buffer[10] = 0x01; // command code high (0101)
            buffer[11] = 0x01; // command code low
            buffer[12] = areaCode;
            buffer[13] = (byte)(wordAddress >> 8);
            buffer[14] = (byte)(wordAddress & 0xFF);
            buffer[15] = bitOffset;
            buffer[16] = (byte)(count >> 8);
            buffer[17] = (byte)(count & 0xFF);
            return buffer;
        }

        public static byte[] BuildMemoryAreaWrite(
            byte destNode, byte srcNode, byte serviceId,
            string address, ReadOnlySpan<byte> payload,
            byte destUnit = 0x00, byte srcUnit = 0x00,
            byte destNetwork = 0x00, byte srcNetwork = 0x00)
        {
            ParseAddress(address, out byte areaCode, out ushort wordAddress, out byte bitOffset);
            ushort count = (ushort)(payload.Length / 2);

            var buffer = new byte[10 + 2 + 1 + 3 + 2 + payload.Length];
            buffer[0] = ICF;
            buffer[1] = RSV;
            buffer[2] = GCT;
            buffer[3] = destNetwork;
            buffer[4] = destNode;
            buffer[5] = destUnit;
            buffer[6] = srcNetwork;
            buffer[7] = srcNode;
            buffer[8] = srcUnit;
            buffer[9] = serviceId;
            buffer[10] = 0x01; // command code high (0102)
            buffer[11] = 0x02; // command code low
            buffer[12] = areaCode;
            buffer[13] = (byte)(wordAddress >> 8);
            buffer[14] = (byte)(wordAddress & 0xFF);
            buffer[15] = bitOffset;
            buffer[16] = (byte)(count >> 8);
            buffer[17] = (byte)(count & 0xFF);
            payload.CopyTo(buffer.AsSpan(18));
            return buffer;
        }

        public static byte[] BuildResponse(byte[] request, ushort endCode, ReadOnlySpan<byte> data)
        {
            var response = new byte[request.Length + 2 + data.Length];
            Buffer.BlockCopy(request, 0, response, 0, 10); // header
            response[0] = (byte)(request[0] | 0x01); // response bit
            // swap DA and SA
            response[4] = request[7];
            response[5] = request[8];
            response[7] = request[4];
            response[8] = request[5];
            // SID keep same
            response[10] = request[10];
            response[11] = request[11];
            // end code + data
            response[12] = (byte)(endCode >> 8);
            response[13] = (byte)(endCode & 0xFF);
            data.CopyTo(response.AsSpan(14));
            return response;
        }

        private static void ParseAddress(string address, out byte areaCode, out ushort wordAddress, out byte bitOffset)
        {
            address = address.Trim();
            var m = Regex.Match(address, @"^(?<area>[DCWAH])(?<addr>\d+)(?:\.(?<bit>\d+))?$", RegexOptions.IgnoreCase);
            if (!m.Success) throw new ArgumentException($"地址格式不支持: {address}");
            var area = m.Groups["area"].Value.ToUpperInvariant();
            bitOffset = m.Groups["bit"].Success ? byte.Parse(m.Groups["bit"].Value) : (byte)0;
            wordAddress = ushort.Parse(m.Groups["addr"].Value);
            areaCode = area switch
            {
                "D" => (byte)0x82, // DM Area
                "C" => (byte)0x30, // CIO
                "W" => (byte)0x31, // Work
                "H" => (byte)0xB1, // Holding
                "A" => (byte)0xB0, // Auxiliary
                _ => throw new ArgumentException($"未知的区域: {area}")
            };
        }
    }
}

