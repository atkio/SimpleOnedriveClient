using SimpleOneDrive;
using System;
using System.Windows.Forms;

namespace SimpleOnedriveClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            if(AuthStore.Instance.TryRefreshToken())
            {
                RefreshListAsync();
            }
            else
            {
                new Signin().Show();
            }
        }

        private async System.Threading.Tasks.Task RefreshListAsync()
        {
            await SimpleClient.Instance.CreateFolder("cosplay2", "testfolder2", "testfolder3");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new Signin().Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }
    }
}
