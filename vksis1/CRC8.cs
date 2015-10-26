using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vksis1
{
    public static class CRC8
    {

        public const byte polynomial = 0x07; //x^8 + x^2 + x + 1
        public const short initValue = 0;

        private static short[] CRCTable = new short[256];
        private static short value = initValue;
        private static bool isTableCounted = false;

        private static void countTable()
        {
            for (int dividend = 0; dividend < 256; dividend++)
            {
                int remainder = dividend;
                for (int bit = 0; bit < 8; ++bit)
                {
                    if ((remainder & 0x01) != 0)
                        remainder = (remainder >> 1) ^ polynomial;
                    else
                        remainder = remainder >> 1;
                }

                CRCTable[dividend] = (short)remainder;
            }
        }

        public static void updateChecksumm(byte b)
        {
            if (!isTableCounted)
            {
                countTable();
                isTableCounted = true;
            }
            int data = (b ^ value) & 0xff;
            value = (short)(CRCTable[data] ^ (value << 8));
        }

        public static void updateChecksumm(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                updateChecksumm(data[i]);
            }
        }

        public static byte Value
        {
            get
            {
                byte b = unchecked((byte)(0xff & value));
                return b;
            }
            //private set;
        }

        public static void clearValue()
        {
            value = initValue;
        }
    }

}
