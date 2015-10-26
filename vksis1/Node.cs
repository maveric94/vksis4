using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vksis1
{
    class Node
    {
        public bool isEnabled;

        private ComPort _right, _left;
        private byte _address;

        public static event DataReceivedEventHandler DataReceived;
       
        public Node(String rightPort, String leftPort, byte address, bool enabled)
        {
            _right = new ComPort(rightPort);
            _left = new ComPort(leftPort);

            _address = address;

            isEnabled = enabled;
            
            ComPort.DataReceived += ComPort_DataReceived;
        }

        public void Send(byte[] data, byte address)
        {
            _right.SendData(Packet.makePacket(data, _address, address));
        }

        private void ComPort_DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!isEnabled)
            {
                _right.SendData(e._data);
                return;
            }

            byte[] msg = Packet.extractFromPacket(e._data);

            if(msg == null)
            {
                DataReceived(this, new DataReceivedEventArgs(null, DataReceivedEventArgs.Error.CRCError));
                return;
            }

            byte src = msg[0];
            byte dest = msg[1];

            if (src == _address)
            {
                DataReceived(this, new DataReceivedEventArgs(null, DataReceivedEventArgs.Error.DestNotFound));
                return;
            }

            if (dest == _address)
            {
                byte[] m = new byte[msg.Length - 2];
                Array.Copy(msg, 2, m, 0, msg.Length - 2);
                DataReceived(this, new DataReceivedEventArgs(m));
                return;
            }

            _right.SendData(e._data);
        }
        public void Close()
        {
            _right.ClosePort();
            _left.ClosePort();
        }
    }
}
