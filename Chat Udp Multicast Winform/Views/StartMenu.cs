using Chat_Udp_Multicast_Winform.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Chat_Udp_Multicast_Winform
{
    public partial class StartMenu : Form
    {
        public StartMenu()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != String.Empty)
            {
                this.Visible = false;
                if (textBox1.Text != "admin")
                {
                    UserForm form = new UserForm(textBox1.Text);
                    form.ShowDialog();
                }
                else
                {
                    AdminForm form = new AdminForm();
                    form.ShowDialog();
                }
                this.Close();
            }
        }
    }
}
