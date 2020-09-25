using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using fbchat_sharp.API;

namespace BroadcastMessage
{
    public partial class Simple_Login : Form
    {
        public Simple_Login()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await doWork();
        }
        private async Task doWork()
        {
            progressBar1.Style = ProgressBarStyle.Marquee;
            Custom_Client client = Custom_Client.getInstance();
            await client.DoLogin(textBox1.Text, textBox2.Text);
            progressBar1.Style = ProgressBarStyle.Blocks;
            progressBar1.Value = progressBar1.Maximum;
            if (await client.isLoggedIn())
            {
                Setting.Default["username"] = textBox1.Text;
                Setting.Default["password"] = textBox2.Text;
                Setting.Default.Save();
                client.useCookies = false;
                MessageBox.Show("login success");
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                this.DialogResult = DialogResult.Abort;
            }
            this.Close();
        }
    }
}
