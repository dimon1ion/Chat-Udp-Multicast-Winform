using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Chat_Udp_Multicast_Winform.Interface
{
    public interface IMessenger
    {
        public string UserName { get; set; }

        void SendMessage(string message, IPAddress address);
        void SendCommandInMessage(string command, IPAddress address);
        void ReceiveMessage();
        void ReceiveCommand();
    }
}
