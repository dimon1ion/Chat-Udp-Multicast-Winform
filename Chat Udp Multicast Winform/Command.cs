using System;
using System.Collections.Generic;
using System.Text;

namespace Chat_Udp_Multicast_Winform
{
    public class Command
    {
        public string Name { get; set; }

        private Action<string> actionCommand;
        private Func<string, bool> isTrueCommand;

        public Command(string name, Action<string> command, Func<string, bool> _isTrueCommand)
        {
            Name = name;
            actionCommand = command;
            this.isTrueCommand = _isTrueCommand;
        }
        public void CommandIvent(string state)
        {
            this.actionCommand?.Invoke(state);
        }

        public bool IsCommandIvent(string state)
        {
            return this.isTrueCommand.Invoke(state);
        }

        public static bool IsCommand(string text)
        {
            if (text.StartsWith('/'))
            {
                return true;
            }
            return false;
        }
    }
}
