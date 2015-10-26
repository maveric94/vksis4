using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Collections;
using System.Threading;

namespace vksis1
{
    public delegate void DataReceivedEventHandler(object sender, DataReceivedEventArgs e);

    public class DataReceivedEventArgs
    {
        public enum Error
        {
            None,
            CRCError,
            DestNotFound
        }
        public byte[] _data;
        public Error _error;
        public DataReceivedEventArgs(byte[] data, Error e = Error.None)
        {
            _error = e;
            _data = data;
        }
    }

    public class ComPort
    {
        private SerialPort _serialPort;
        public static event DataReceivedEventHandler DataReceived;

        public ComPort(String portName, int baudRate = 9600)
        {
            _serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
            _serialPort.ErrorReceived += new SerialErrorReceivedEventHandler(SerialPort_ErrorReceived);
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(SerialPort_DataReceived);

            _serialPort.Open();
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            do
            {
                List<byte> receivedMessage = new List<byte>();
                byte[] b = new byte[1];

                do
                {
                    _serialPort.Read(b, 0, 1);
                }
                while (b[0] != Packet.endOfPacketByte);

                receivedMessage.Add(b[0]);

                do
                {
                    _serialPort.Read(b, 0, 1);
                    receivedMessage.Add(b[0]);
                }
                while (b[0] != Packet.endOfPacketByte);

                DataReceived(this, new DataReceivedEventArgs(receivedMessage.ToArray()));
            }
            while (_serialPort.BytesToRead != 0);
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void SendData(byte[] data)
        {
            _serialPort.RtsEnable = true;
            _serialPort.Write(data, 0, data.Length);
            Thread.Sleep(100); 
            _serialPort.RtsEnable = false;
            
        }
        public void ChangeBaudRate(int newRate)
        {
            _serialPort.BaudRate = newRate;
        }
        public void ClosePort()
        {
            _serialPort.Close();
        }
    }
}
