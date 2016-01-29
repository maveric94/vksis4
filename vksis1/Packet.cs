using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vksis1
{
    public static class Packet
    {
        private enum Operation
        {
            Extract,
            Add,
        }
        // packet structure
        // eop - source address - destination address - data - crc8 checksumm - eop
        public static byte endOfPacketByte = 0x7E;
        public static int bytesPerPacket = 10;
        private static List<bool> forbiddenSequence = new List<bool> { false, true, true, true, true, true, true };
        private static List<bool> stuffedSequence = new List<bool> { false, true, true, true, true, true, true, true };
        private static List<bool> packets = new List<bool>();
        public static byte[] makePacket(byte[] rawData, byte source, byte destination, bool addError = false)
        {
            int packetsAmount = rawData.Length / bytesPerPacket;

            if ((rawData.Length % bytesPerPacket) > 0)
                packetsAmount++;

            packets.Clear();
            for (int packetNumber = 0; packetNumber < packetsAmount; packetNumber++)
            {
                CRC8.clearValue();
                insertEndOfPacket();

                insertByte(source);
                CRC8.updateChecksumm(source);
                insertByte(destination);
                CRC8.updateChecksumm(destination);

                

                int currentPosition = packetNumber * bytesPerPacket;
               
                for (int byteNumber = 0; byteNumber < bytesPerPacket; byteNumber++)
                {
                    byte currentByte;

                    if (currentPosition + byteNumber >= rawData.Length)
                        break;
                    else
                        currentByte = rawData[currentPosition + byteNumber];

                    insertByte(currentByte);
                    CRC8.updateChecksumm(currentByte);
                }
                if (addError)
                {
                    if (packets[packets.Count - 1])
                        packets[packets.Count - 1] = false;
                    else
                        packets[packets.Count - 1] = true;
                }
                insertByte(CRC8.Value);
                insertEndOfPacket();
            }
            return bitListToByteArray();
        }
        public static byte[] extractFromPacket(byte[] packet)
        {
            int bitsDeleted = 0;
            bool bitDeleted = false;
            packets.Clear();

            for (int byteNumber = 1; byteNumber < packet.Length - 1; byteNumber++)
            {
                for (int bitNumber = 7; bitNumber >= 0; bitNumber--)
                {
                    packets.Add(((packet[byteNumber] >> bitNumber) & 1) == 1);

                    if (!bitDeleted && checkForSequence(Operation.Extract))
                    {
                        bitDeleted = true;
                        bitsDeleted++;
                        continue;
                    }
                    bitDeleted = false;
                }
            }
            int bitsToDelete = 8 - (bitsDeleted % 8);

            if (bitsToDelete != 8)
            {
                for (int i = 0; i < bitsToDelete; i++)
                    packets.RemoveAt(packets.Count - 1); 
            }

            byte expectedChecksumm = extractByte((packets.Count / 8) - 1);
            for (int i = 0; i < 8; i++)
                packets.RemoveAt(packets.Count - 1);

            byte[] extractedMessage = bitListToByteArray();
            CRC8.clearValue();
            CRC8.updateChecksumm(extractedMessage);
            byte checksumm = CRC8.Value;

            if (checksumm != expectedChecksumm)
                return null;

            return extractedMessage;
        }
        private static byte[] bitListToByteArray()
        {
            int packetsSize = packets.Count;
            int bytesNumber = packetsSize / 8;
            byte[] processedData = new byte[bytesNumber];

            for (int i = 0; i < bytesNumber; i++)
                processedData[i] = extractByte(i);

            return processedData;
        }
        private static byte extractByte(int number)
        {
            byte b = 0;
            for (int i = 0; i < 8; i++)
                b |= (byte)(((packets[number * 8 + i]) ? 1 : 0) << 7 - i);

            return b;
        }
        private static bool checkForSequence(Operation op)
        {
            List<bool> sequence;
            if (op == Operation.Add)
                sequence = forbiddenSequence;
            else
                sequence = stuffedSequence;
            int sequenceSize = sequence.Count;
            int packetSize = packets.Count;
            if (packetSize < sequenceSize)
                return false;

            for (int i = 1; i <= sequenceSize; i++)
            {
                if (packets[packetSize - i] != sequence[sequenceSize - i])
                    return false;

            }
            if (op == Operation.Add)
                packets.Add(true);
            else
                packets.RemoveAt(packetSize - 1);

            return true;
        }
        private static void insertEndOfPacket()
        {
            int packetsSize = packets.Count;
            int lastBits = packetsSize % 8;
            //filling the last byte cause we need endofpacket symbol to be aligned to byte border
            if (lastBits > 0)
            {
                for (int i = 0; i < 8 - lastBits; i++)
                    packets.Add(false);

            }
            insertByte(endOfPacketByte, false);
        }
        private static void insertCheckSum(byte[] data, int currentPos)
        {
            CRC8.clearValue();
            for (int i = 0; i < bytesPerPacket; i++)
            {
                if (currentPos + i >= data.Length)
                    break;

                CRC8.updateChecksumm(data[currentPos + i]);
            }
            insertByte(CRC8.Value);
        }
        private static void insertByte(byte b, bool bitStuffing = true)
        {
            for (int bitNumber = 7; bitNumber >= 0; bitNumber--)
            {
                packets.Add(((b >> bitNumber) & 1) == 1);
                if (bitStuffing)
                    checkForSequence(Operation.Add);
            }
        }
    }
}
