using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chat_Udp_Multicast_Winform
{
    public partial class Form1 : Form
    {
        IPEndPoint thisEndPoint;
        delegate void AppendText(string text);
        event AppendText appendText;

        public string Name { get; set; }

        public Form1(string name)
        {
            Name = name;

            InitializeComponent();

            appendText += AppendToTextBox;

            thisEndPoint = new IPEndPoint(IPAddress.Any, 51234);

            Thread thread = new Thread(new ThreadStart(ReceiveMessage));
            thread.IsBackground = true;
            thread.Start();
        }

        private void AppendToTextBox(string text)
        {
            textBox1.Text += text;
        }

        private void ReceiveMessage()
        {
            while (true)
            {
                using (UdpClient udpClient = new UdpClient(AddressFamily.InterNetwork))
                {
                    IPAddress address = IPAddress.Parse("224.5.5.5");

                    udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    udpClient.Client.Bind(thisEndPoint);
                    udpClient.JoinMulticastGroup(address);
                    IPEndPoint end = new IPEndPoint(IPAddress.Any, 0);
                    byte[] buffer = udpClient.Receive(ref end);

                    this.Invoke(appendText, Encoding.UTF8.GetString(buffer));
                }

            }
        }

        private void button1_Send_Click(object sender, EventArgs e)
        {
            if (textBox2.Text.Trim() != String.Empty)
            {
                using (UdpClient udpClient = new UdpClient(AddressFamily.InterNetwork))
                {
                    IPAddress address = IPAddress.Parse("224.5.5.5");
                    IPEndPoint end = new IPEndPoint(address, 51234);
                    udpClient.JoinMulticastGroup(address);

                    string message = $"{Name}: {textBox2.Text}";

                    byte[] buffer = Encoding.UTF8.GetBytes(textBox2.Text);
                    udpClient.Send(buffer, buffer.Length, end);
                }
            }
        }
    }
}
