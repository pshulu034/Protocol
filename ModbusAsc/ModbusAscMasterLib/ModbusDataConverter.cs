using System;
using System.Text;

namespace ModbusRtuMasterLib
{
    public class ModbusDataConverter
    {
        public bool IsHighWordFirst { get; set; } = false;

        public short ToInt16(ushort word)
        {
            return unchecked((short)word);
        }

        public short[] ToInt16Array(ushort[] words)
        {
            var result = new short[words.Length];
            for (int i = 0; i < words.Length; i++) result[i] = unchecked((short)words[i]);
            return result;
        }

        public int ToInt32(ushort w0, ushort w1)
        {
            uint v = IsHighWordFirst ? ((uint)w0 << 16) | w1 : ((uint)w1 << 16) | w0;
            return unchecked((int)v);
        }

        public int ToInt32(ushort[] words)
        {
            if (words.Length < 2) throw new ArgumentException("Requires at least 2 words");
            return ToInt32(words[0], words[1]);
        }

        public int[] ToInt32Array(ushort[] words)
        {
            if (words.Length % 2 != 0) throw new ArgumentException("Words length must be even");
            int count = words.Length / 2;
            var result = new int[count];
            for (int i = 0; i < count; i++)
            {
                int idx = i * 2;
                result[i] = ToInt32(words[idx], words[idx + 1]);
            }
            return result;
        }

        public uint ToUInt32(ushort w0, ushort w1)
        {
            return IsHighWordFirst ? ((uint)w0 << 16) | w1 : ((uint)w1 << 16) | w0;
        }

        public uint ToUInt32(ushort[] words)
        {
            if (words.Length < 2) throw new ArgumentException("Requires at least 2 words");
            return ToUInt32(words[0], words[1]);
        }

        public uint[] ToUInt32Array(ushort[] words)
        {
            if (words.Length % 2 != 0) throw new ArgumentException("Words length must be even");
            int count = words.Length / 2;
            var result = new uint[count];
            for (int i = 0; i < count; i++)
            {
                int idx = i * 2;
                result[i] = ToUInt32(words[idx], words[idx + 1]);
            }
            return result;
        }

        public float ToFloat(ushort w0, ushort w1)
        {
            uint u = ToUInt32(w0, w1);
            int bits = unchecked((int)u);
            return BitConverter.Int32BitsToSingle(bits);
        }

        public float ToFloat(ushort[] words)
        {
            if (words.Length < 2) throw new ArgumentException("Requires at least 2 words");
            return ToFloat(words[0], words[1]);
        }

        public float[] ToFloatArray(ushort[] words)
        {
            if (words.Length % 2 != 0) throw new ArgumentException("Words length must be even");
            int count = words.Length / 2;
            var result = new float[count];
            for (int i = 0; i < count; i++)
            {
                int idx = i * 2;
                result[i] = ToFloat(words[idx], words[idx + 1]);
            }
            return result;
        }

        public string ToString(ushort[] words, Encoding? encoding = null)
        {
            var enc = encoding ?? Encoding.ASCII;
            var bytes = new byte[words.Length * 2];
            for (int i = 0; i < words.Length; i++)
            {
                bytes[2 * i] = (byte)(words[i] >> 8);
                bytes[2 * i + 1] = (byte)(words[i] & 0xFF);
            }
            var s = enc.GetString(bytes);
            int idxNull = s.IndexOf('\0');
            if (idxNull >= 0) s = s[..idxNull];
            return s;
        }

        public ushort[] FromInt16(short value)
        {
            return new ushort[] { unchecked((ushort)value) };
        }

        public ushort[] FromInt16Array(short[] values)
        {
            var result = new ushort[values.Length];
            for (int i = 0; i < values.Length; i++) result[i] = unchecked((ushort)values[i]);
            return result;
        }

        public ushort[] FromInt32(int value)
        {
            uint u = unchecked((uint)value);
            return FromUInt32(u);
        }

        public ushort[] FromInt32Array(int[] values)
        {
            var result = new ushort[values.Length * 2];
            for (int i = 0; i < values.Length; i++)
            {
                var pair = FromInt32(values[i]);
                result[i * 2] = pair[0];
                result[i * 2 + 1] = pair[1];
            }
            return result;
        }

        public ushort[] FromUInt32(uint value)
        {
            ushort wHigh = (ushort)(value >> 16);
            ushort wLow = (ushort)(value & 0xFFFF);
            return IsHighWordFirst ? new ushort[] { wHigh, wLow } : new ushort[] { wLow, wHigh };
        }

        public ushort[] FromUInt32Array(uint[] values)
        {
            var result = new ushort[values.Length * 2];
            for (int i = 0; i < values.Length; i++)
            {
                var pair = FromUInt32(values[i]);
                result[i * 2] = pair[0];
                result[i * 2 + 1] = pair[1];
            }
            return result;
        }

        public ushort[] FromFloat(float value)
        {
            int bits = BitConverter.SingleToInt32Bits(value);
            uint u = unchecked((uint)bits);
            return FromUInt32(u);
        }

        public ushort[] FromFloatArray(float[] values)
        {
            var result = new ushort[values.Length * 2];
            for (int i = 0; i < values.Length; i++)
            {
                var pair = FromFloat(values[i]);
                result[i * 2] = pair[0];
                result[i * 2 + 1] = pair[1];
            }
            return result;
        }

        public ushort[] FromString(string value, Encoding? encoding = null)
        {
            var enc = encoding ?? Encoding.ASCII;
            var bytes = enc.GetBytes(value);
            int regCount = (bytes.Length + 1) / 2;
            var data = new ushort[regCount];
            for (int i = 0; i < regCount; i++)
            {
                int bi = i * 2;
                byte hi = bytes[bi];
                byte lo = bi + 1 < bytes.Length ? bytes[bi + 1] : (byte)0;
                data[i] = (ushort)((hi << 8) | lo);
            }
            return data;
        }

        public ushort[] FromString(string value, ushort registerLength, Encoding? encoding = null)
        {
            var enc = encoding ?? Encoding.ASCII;
            var bytes = enc.GetBytes(value);
            var data = new ushort[registerLength];
            for (int i = 0; i < registerLength; i++)
            {
                int bi = i * 2;
                byte hi = bi < bytes.Length ? bytes[bi] : (byte)0;
                byte lo = bi + 1 < bytes.Length ? bytes[bi + 1] : (byte)0;
                data[i] = (ushort)((hi << 8) | lo);
            }
            return data;
        }
    }
}

