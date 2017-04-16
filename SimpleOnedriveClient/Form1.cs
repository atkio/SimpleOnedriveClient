using SimpleOneDrive;
using System;
using System.Diagnostics;
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
            var files=await SimpleClient.Instance.GetSubFiles("cosplay");
            foreach(var f in files.value)
            Debug.WriteLine(f.id+":"+f.webUrl);
           // await SimpleClient.Instance.uploadFileFromUrl("http://baby.japaninfoz.com/wp-content/uploads/2016/10/IMGP4251-300x199.jpg","baby.jpg", "cosplay", "testfolder1");
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
