using Chat_Udp_Multicast_Winform.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Chat_Udp_Multicast_Winform.Views
{
    public partial class AdminForm : Form, IMessenger
    {
        delegate void AppendText(string text);
        event AppendText appendText;

        IPEndPoint thisEndPoint;

        List<Command> commands;

        List<(string, int)> banList;

        public string UserName { get; set; }
        private int port;

        private bool receiveMesStarted;
        private bool receiveComStarted;

        public int Port
        {
            get { return port; }
            set
            {
                if (!port.Equals(value))
                {
                    receiveMesStarted = false;
                    receiveComStarted = false;
                    port = value;
                    thisEndPoint = new IPEndPoint(IPAddress.Any, Port);
                    Thread thread = new Thread(new ThreadStart(ReceiveMessage));
                    Thread thread2 = new Thread(new ThreadStart(ReceiveCommand));
                    thread.IsBackground = true;
                    thread2.IsBackground = true;
                    thread.Start();
                    thread2.Start();
                    while (!receiveMesStarted && !receiveComStarted) { }
                    SendMessage($"Admin has joined the chat", IPForChat.GetIp(IP.Message));
                }
            }
        }

        private int lobby;

        public int Lobby
        {
            get { return lobby; }
            set
            {
                if (!lobby.Equals(value))
                {
                    Port = (Port - lobby) + value;
                    lobby = value;
                }
            }
        }


        public string TextSend
        {
            get
            {
                return textBox2.Text;
            }
            set
            {
                textBox2.Text = value;
            }
        }

        public AdminForm()
        {
            InitializeComponent();

            UserName = "admin";

            banList = new List<(string, int)>();

            #region Commands initialize

            commands = new List<Command>()
            {
                new Command("/private", state =>
                {
                    string command = state.Replace(" /private ", null);

                    int startindex = command.IndexOf(':');
                    int indexEndName = command.IndexOf(' ');
                    if (indexEndName == -1)
                    {
                        indexEndName = command.Length - 1;
                    }
                    string recipient = command.Substring(startindex + 1, (indexEndName - 1) - startindex);
                    string sender = command.Substring(0, startindex).ToLower();

                    if (recipient == UserName)
                    {
                        this.Invoke(appendText, $"[{sender} -> you]: {command.Substring(indexEndName + 1)}");
                    }
                    else if (sender == UserName)
                    {
                        this.Invoke(appendText, $"[you -> {recipient}]: {command.Substring(indexEndName + 1)}");
                    }
                }, state =>
                {
                    state = state.Trim();
                    string[] check = state.Split(' ');
                    if (check[0] == "/private")
                    {
                        return true;
                    }
                    return false;
                }),

                new Command("/lobby", state =>
                {
                    string command = state.Replace(" /lobby ", null);

                    int startindex = command.IndexOf(':');

                    string[] names = command.Substring(startindex + 1).Trim().Split(',');

                    int indexLastName = names[names.Length - 1].LastIndexOf(' ');

                    if (indexLastName == -1) { indexLastName = 0; }

                    int newLobby;
                    if (!Int32.TryParse(names[names.Length - 1].Substring(indexLastName), out newLobby))
                    {
                        return;
                    }

                    names[names.Length - 1] = names[names.Length - 1].Substring(0, indexLastName);


                    if (UserName == command.Substring(0, startindex).ToLower())
                    {
                        Lobby = newLobby;
                        this.Invoke(appendText, $"You joined to lobby number:{newLobby}!\r\n");
                    }
                    else{
                        foreach(string name in names)
                        {
                            if (name == UserName)
                            {
                                Lobby = newLobby;
                                this.Invoke(appendText, $"You joined to lobby number:{newLobby}!\r\n");
                                break;
                            }
                        }
                    }

                }, state =>
                {
                    state = state.Trim();
                    string[] check = state.Split(' ');
                    int resultLobby;
                    if (check[0] == "/lobby" && Int32.TryParse(check[check.Length - 1], out resultLobby) && check.Length > 2)
                    {
                        if (resultLobby < 1000)
                        {
                            return true;
                        }
                    }
                    return false;
                }),
                new Command("/ban", fullCommand =>
                {
                    string command = fullCommand.Replace(" /ban ", null);

                    int startindex = command.IndexOf(':');

                    string[] result = command.Substring(startindex + 1).Trim().Split(',');

                    string banName = result[0];

                    int minutes = -1;
                    bool banTime = false;
                    if (result.Length > 1)
                    {
                        banTime = true;
                        if (!Int32.TryParse(result[result.Length - 1], out minutes))
                        {
                            return;
                        }
                    }

                    if (banList.Contains((banName, minutes))) { return; }

                    banList.Add((banName, minutes));
                    if (banTime)
                    {
                        System.Threading.Timer timer = new System.Threading.Timer(Tick, (banName, minutes).ToTuple(), Convert.ToInt64(TimeSpan.FromMinutes(minutes).TotalMilliseconds), 0);
                    }
                    this.Invoke(appendText, $"User {banName} has banned from this chat {(banTime ? $"for {minutes} minutes" : "")}!\r\n");

                }, fullCommand =>
                {
                    fullCommand = fullCommand.Trim();
                    string[] check = fullCommand.Split(' ');
                    if (check[0] == "/ban")
                    {
                        int minutes;
                        string[] text = fullCommand.Split(',');
                        if (text.Length > 1)
                        {
                            if (!Int32.TryParse(text[text.Length - 1], out minutes))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                    return false;
                }),
            };

            #endregion

            appendText += AppendToTextBox;

            Port = 51234;
            Lobby = 0;
        }

        private void Tick(object state)
        {
            Tuple<string, int> tup = state as Tuple<string, int>;
            banList.Remove((tup.Item1, tup.Item2));
            this.Invoke(appendText, $"User {tup.Item1} has unbanned!");
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void AppendToTextBox(string text)
        {
            textBox1.Text += text;
        }

        public void ReceiveMessage()
        {
            int thisPort = Port;
            using (UdpClient udpClient = new UdpClient(AddressFamily.InterNetwork))
            {
                IPAddress address = IPForChat.GetIp(IP.Message);
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpClient.Client.Bind(thisEndPoint);
                udpClient.JoinMulticastGroup(address);
                IPEndPoint end;
                while (true)
                {

                    end = new IPEndPoint(IPAddress.Any, 0);
                    receiveMesStarted = true;
                    byte[] buffer = udpClient.Receive(ref end);
                    if (thisPort != Port)
                    {
                        return;
                    }
                    string message = Encoding.UTF8.GetString(buffer);
                    bool profanity;
                    message = message.IsProfanity(out profanity);
                    if (profanity)
                    {
                        message += "!!!(Profanity)!!!\r\n";
                    }
                    if (!message.Contains("left"))
                    {
                        string sender = message.Substring(0, message.IndexOf(':'));
                        //for (int i = 0; i < banList.Count; i++)
                        //{
                        //    if (sender == banList[i].Item1)
                        //    {
                        //        SendCommandInMessage($"/ban {sender}{(banList[i].Item2 == -1 ? "" : $", {banList[i].Item2}")}", IPForChat.GetIp(IP.Command));
                        //    }
                        //}
                        foreach ((string, int) user in banList)
                        {
                            if (sender == user.Item1)
                            {
                                SendCommandInMessage($"/ban {sender}{(user.Item2 == -1 ? "" : $", {user.Item2}")}", IPForChat.GetIp(IP.Command));
                            }
                        }
                    }

                    while (true)
                    {
                        if (this.Created)
                        {
                            this.Invoke(appendText, message);
                            break;
                        }
                    }
                }

            }
        }

        public void ReceiveCommand()
        {
            int thisPort = Port;
            using (UdpClient udpClient = new UdpClient(AddressFamily.InterNetwork))
            {
                IPAddress address = IPForChat.GetIp(IP.Command);
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpClient.Client.Bind(thisEndPoint);
                udpClient.JoinMulticastGroup(address);
                IPEndPoint end;
                while (true)
                {

                    end = new IPEndPoint(IPAddress.Any, 0);
                    receiveComStarted = true;
                    byte[] buffer = udpClient.Receive(ref end);
                    if (thisPort != Port)
                    {
                        return;
                    }

                    string command = Encoding.UTF8.GetString(buffer);

                    foreach (Command item in commands)
                    {
                        if (command.Contains(item.Name))
                        {
                            item.CommandIvent(command);
                            break;
                        }
                    }
                }

            }
        }

        private void button1_Send_Click(object sender, EventArgs e)
        {
            if (Command.IsCommand(TextSend))
            {
                SendCommandInMessage(TextSend, IPForChat.GetIp(IP.Command));
            }
            else { SendMessage(TextSend, IPForChat.GetIp(IP.Message)); }
        }

        public void SendMessage(string message, IPAddress address)
        {
            if (message.Trim() != String.Empty)
            {
                using (UdpClient udpClient = new UdpClient(AddressFamily.InterNetwork))
                {
                    IPEndPoint end = new IPEndPoint(address, Port);

                    message = $"{UserName}: {message}\r\n";

                    byte[] buffer = Encoding.UTF8.GetBytes(message);
                    udpClient.Send(buffer, buffer.Length, end);
                }
            }
        }

        public void SendCommandInMessage(string command, IPAddress address)
        {
            foreach (Command item in commands)
            {
                if (item.IsCommandIvent(command))
                {
                    SendMessage(command, address);
                    return;
                }
            }
            appendText.Invoke($"Command: \"{command}\" not found!\r\n");
        }

        private void button2_Click_PrivateExampleCommand(object sender, EventArgs e)
        {
            TextSend = ("/private name message");
        }

        private void button3_Click_ChangeLobby(object sender, EventArgs e)
        {
            TextSend = ("/lobby name1,name2,name3 1-999");
        }

        private void button4_Click_BanForAWhile(object sender, EventArgs e)
        {
            TextSend = ("/ban name, 5");
        }

        private void button5_Click_Permanently(object sender, EventArgs e)
        {
            TextSend = ("/ban name");
        }
    }
}
