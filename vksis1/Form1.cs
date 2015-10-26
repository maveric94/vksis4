using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;

namespace vksis1
{
    public partial class Form1 : Form
    {
        private Node _node;

        public Form1()
        {
            InitializeComponent();

            portsListBox.Items.AddRange(SerialPort.GetPortNames());
            listBox1.Items.AddRange(SerialPort.GetPortNames());

            FormClosing += ComChatForm_FormClosing;
            outputTextBox.TextChanged += outputTextBox_TextChanged;
            inputTextBox.KeyDown += inputTextBox_KeyDown;
            Node.DataReceived += OnDataReceived;

        }

        void inputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                sendButton.PerformClick();
        }

        void outputTextBox_TextChanged(object sender, EventArgs e)
        {
            outputTextBox.SelectionStart = outputTextBox.Text.Length;
            outputTextBox.ScrollToCaret(); 
        }

        void ComChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_node != null)
                _node.Close();
        }

        private void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            /*System.Text.StringBuilder str = new StringBuilder();
            for (int i = 0; i < e._data.Length; i += 2)
                str.Append((char)e._data[i]);*/
            String str = null;

            if (e._error == DataReceivedEventArgs.Error.None)
                str = System.Text.Encoding.ASCII.GetString(e._data) + "\n";
            else if (e._error == DataReceivedEventArgs.Error.CRCError)
                str = "CRC error.\n";
            else if (e._error == DataReceivedEventArgs.Error.DestNotFound)
                str = "Destination device not found.\n";
            else
                str = "Unknown error.\n";

            this.Invoke((MethodInvoker)delegate { outputTextBox.AppendText(str); });
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            outputTextBox.AppendText(inputTextBox.Text + "\n");

            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(inputTextBox.Text);
            byte adr;

            byte.TryParse(textBox2.Text, out adr);
            _node.Send(bytes, adr);

            inputTextBox.Clear();
        }

        private void InitButton_Click(object sender, EventArgs e)
        {
            byte adr = 0;
            byte.TryParse(textBox1.Text, out adr);
            _node = new Node(portsListBox.SelectedItem.ToString(), listBox1.SelectedItem.ToString(), adr, !checkBox2.Checked);

            sendButton.Enabled = true;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            _node.isEnabled = !checkBox2.Checked;
        }

    }
}
