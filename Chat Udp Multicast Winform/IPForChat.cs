using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Chat_Udp_Multicast_Winform
{
    public enum IP
    {
        Message,
        Command
    }
    public class IPForChat
    {
        static IPAddress GeneralForMessageMulticastIp { get; set; }
        static IPAddress GeneralForCommandMulticastIp { get; set; }

        static IPForChat()
        {
            GeneralForMessageMulticastIp = IPAddress.Parse("224.0.0.0");
            GeneralForCommandMulticastIp = IPAddress.Parse("224.5.5.5");
        }

        public static IPAddress GetIp(IP ip)
        {
            switch (ip)
            {
                case IP.Message:
                    return GeneralForMessageMulticastIp;
                case IP.Command:
                    return GeneralForCommandMulticastIp;
                default:
                    return null;
            }
        }
    }
}
