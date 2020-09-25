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
using fbchat_sharp;
using System.IO;

namespace BroadcastMessage
{
    public partial class Form1 : Form
    {
        private Custom_Client client;
        private List<FB_User> allActiveUsers;
        public Form1()
        {
            InitializeComponent();
            setState(false);
        }
        private void setState(bool state)
        {
            button1.Enabled = state;
            button2.Enabled = state;
            button3.Enabled = state;
            richTextBox1.Enabled = state;
            dataGridView1.Enabled = state;
            if (state)
            {
                progressBar1.Style = ProgressBarStyle.Blocks;
                progressBar1.Value = 0;
            }
            else
            {
                progressBar1.Style = ProgressBarStyle.Marquee;
            }
        }
        private async Task<bool> checkUserInfo()
        {
            DialogResult res = DialogResult.Abort;
            bool userpass = (!string.IsNullOrEmpty(Setting.Default["username"].ToString())) && (!string.IsNullOrEmpty(Setting.Default["password"].ToString()));
            bool cookies = !string.IsNullOrEmpty(Setting.Default["CookiePath"].ToString());
            while (cookies==false)
            {
                FolderBrowserDialog fdialog = new FolderBrowserDialog();
                res = fdialog.ShowDialog();
                if (res == DialogResult.OK)
                {
                    Setting.Default["CookiePath"] = fdialog.SelectedPath + "\\";
                    Setting.Default.Save();
                }
                cookies = !string.IsNullOrEmpty(Setting.Default["CookiePath"].ToString());
            }
            client = Custom_Client.getInstance();
            if (client.useCookies)
            {
                try
                {
                    await client.fromSavedSession();
                }
                catch
                {
                    try
                    {
                        File.Delete(Setting.Default["CookiePath"].ToString() + Setting.Default["Filename"].ToString());
                        await client.DoLogin(Setting.Default["username"].ToString(), Setting.Default["password"].ToString());
                    }
                    catch(Exception e)
                    {
                        throw e;
                    }
                }
            }
            else
            {
                if (userpass == false)
                {
                    res = new Simple_Login().ShowDialog();
                    if (res!= DialogResult.OK)
                    {
                        Application.Exit();
                    }
                }
                else
                {
                    try
                    {
                        await client.DoLogin(Setting.Default["username"].ToString(), Setting.Default["password"].ToString());
                    }
                    catch(Exception ex)
                    {
                        throw ex;
                    }
                }
            }
            label1.Text = (await client.fetchProfile()).name;
            return true;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            bool status = await checkUserInfo();
            setState(status);
            loadUsers();
        }
        private async void loadUsers()
        {
            List<string> uids = await client.fetchActiveUsers();
            List < FB_User > d  = await client.fetchAllUsers();
            allActiveUsers = d.Where(x => uids.Contains(x.uid)).ToList();
            dataGridView1.DataSource = allActiveUsers.Select(x => new { x.uid, x.name }).ToList() ;
        }
        private async void button2_Click(object sender, EventArgs e)
        {
            DialogResult r = MessageBox.Show("Delete Cookies?", Application.ProductName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r == DialogResult.Yes)
            {
                await client.DoLogout();
                Setting.Default["username"] = null;
                Setting.Default["password"] = null;
                Setting.Default["CookiePath"] = null;
                Setting.Default.Save();
            }
            else
            {
                await client.logout();
            }
            this.Close();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            progressBar1.Maximum = allActiveUsers.Count;
            progressBar1.Value = 0;
            for(int i=0;i<allActiveUsers.Count;i++)
            {
                string msg = richTextBox1.Text.Replace("{Name}", allActiveUsers[i].name);
                await client.send(new FB_Message(msg),allActiveUsers[i].uid, allActiveUsers[i].type);
                progressBar1.Value = i;
            }
            setState(true);
            MessageBox.Show("Clear");
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            setState(false);
            loadUsers();
            setState(true);
        }
    }
}
