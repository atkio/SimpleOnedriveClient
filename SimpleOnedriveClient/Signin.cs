using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimpleOneDrive;

namespace SimpleOnedriveClient
{
    public partial class Signin : Form
    {
        public Signin()
        {
            InitializeComponent();

            this.TopMost = true;

            client_id.Text = AuthStore.Instance.client_id;
            client_secret.Text = AuthStore.Instance.client_secret;
            redirect_uri.Text = AuthStore.Instance.redirect_uri;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.webBrowser1.Url = new Uri("https://dev.onedrive.com/auth/graph_oauth.htm");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var uri = new Uri("https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id=" + client_id.Text +
                             "&scope=files.readwrite+offline_access&response_type=code&redirect_uri=" + redirect_uri.Text);


            webBrowser1.Navigated += new WebBrowserNavigatedEventHandler((obj, ergs) =>
            {

                System.Diagnostics.Debug.WriteLine(webBrowser1.Url.ToString());

                if (webBrowser1.Url.ToString().StartsWith(redirect_uri.Text))
                {

                    if (AuthStore.TryAuthentication(
                        client_id.Text,
                        client_secret.Text,
                        redirect_uri.Text,
                        webBrowser1.Url.ToString().Split('=')[1]))
                        this.Close();
                }
            });

            webBrowser1.Url = uri;
        }

        
    }
}
