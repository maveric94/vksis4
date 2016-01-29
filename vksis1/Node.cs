using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace vksis1
{
    class Node
    {
        public const int ReceiverPort = 27020;
        public const int TransmitterPort = 27021;

        public bool isEnabled;
        

        private UdpClient _receiver;
        private UdpClient _transmitter;
        private byte _address;
        private String _ipAddress;
        private bool _isClosed;

        public static event DataReceivedEventHandler DataReceived;
       
        public Node(byte address, bool enabled, String ipAddress)
        {
            _address = address;
            _ipAddress = ipAddress;
            isEnabled = enabled;
            _isClosed = false;
            
            _receiver = new UdpClient(ReceiverPort, AddressFamily.InterNetwork);
            _transmitter = new UdpClient(TransmitterPort, AddressFamily.InterNetwork);


            _receiver.BeginReceive(OnDataReceived, null);
        }

        public void Send(byte[] data, byte address)
        {
            byte[] packet = Packet.makePacket(data, _address, address);
            _transmitter.Send(packet, packet.Length, _ipAddress, ReceiverPort);
        }

        private void OnDataReceived(IAsyncResult result)
        {
            if (_isClosed)
                return;

            IPEndPoint senderAddress = new IPEndPoint(IPAddress.Any, TransmitterPort);
            byte[] data = _receiver.EndReceive(result, ref senderAddress);

            if (!isEnabled)
            {
                _transmitter.Send(data, data.Length, _ipAddress, ReceiverPort);
                return;
            }

            List<byte> packet = new List<byte>();
            int i = 0;
            while(true)
            {
                while (data[i++] != Packet.endOfPacketByte);

                packet.Add(Packet.endOfPacketByte);

                while (data[i] != Packet.endOfPacketByte)
                {
                    packet.Add(data[i]);
                    i++;
                }
              
                packet.Add(Packet.endOfPacketByte);
                handlePacket(packet.ToArray());
                packet.Clear();
                if (data.Length == i + 1)
                {
                    break;
                }
                else
                {
                    i++;
                }
            }
        }
        private void handlePacket(byte[] data)
        {
            byte[] msg = Packet.extractFromPacket(data);
            byte src = msg[0];
            byte dest = msg[1];

            if (msg == null)
            {
                DataReceived(this, new DataReceivedEventArgs(null, DataReceivedEventArgs.Error.CRCError));
            }
            else if (dest == _address)
            {
                byte[] m = new byte[msg.Length - 2];
                Array.Copy(msg, 2, m, 0, msg.Length - 2);
                DataReceived(this, new DataReceivedEventArgs(m));
            }
            else if (src == _address)
            {
                DataReceived(this, new DataReceivedEventArgs(null, DataReceivedEventArgs.Error.DestNotFound));
            }
            else
            {
                _transmitter.Send(data, data.Length, _ipAddress, ReceiverPort);
            }

            _receiver.BeginReceive(OnDataReceived, null);
        }
        public void Close()
        {
            _isClosed = true;
            _receiver.Close();
            _transmitter.Close();
        }
    }
}
