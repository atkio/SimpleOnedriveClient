using SimpleOneDrive;
using System;
using System.Diagnostics;
using System.Threading;
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

        private async void RefreshListAsync()
        {
            Thread.Sleep(300);
            var root = await SimpleClient.Instance.getItem();
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();
            treeView1.Nodes.Add(root.id, "Root");
            foreach (var f in root.children)
            {

                treeView1.Nodes[0].Nodes.Add(f.id, f.name);
            }
            treeView1.EndUpdate();
            treeView1.ExpandAll();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            new Signin().Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            RefreshListAsync();
        }

        private void treeView1_DoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {

           

        }

        private void treeView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private async void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                var root = await SimpleClient.Instance.getItem(e.Node.Name);
                if(root.folder ==null)
                {
                    MessageBox.Show(root.webUrl);
                }

                treeView1.BeginUpdate();
                e.Node.Nodes.Clear();
                foreach (var f in root.children)
                {
                    e.Node.Nodes.Add(f.id, f.name);
                }
                treeView1.EndUpdate();
                treeView1.ExpandAll();
            }
            // If the file is not found, handle the exception and inform the user.
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
           
            button4.Enabled = true;
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(textBox1.Text))
                return;
            if (treeView1.SelectedNode == null)
            {
                await SimpleClient.Instance.CreateFolder(textBox1.Text);
                return;
            }
                

          

            var pid = treeView1.SelectedNode.Name;

            await SimpleClient.Instance.CreateFolder(textBox1.Text,pid);

            RefreshListAsync();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
            
        }

        private async void saveFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (treeView1.SelectedNode == null)
               return;
            
            await SimpleClient.Instance.downloadFile(treeView1.SelectedNode.Name,saveFileDialog1.FileName);
        }

        private async void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string pid = "";
            if (treeView1.SelectedNode == null)
                pid = null;
            else
                pid = treeView1.SelectedNode.Name;

            string[] files = openFileDialog1.FileNames;
      
            foreach (string file in files)
            {
                await SimpleClient.Instance.uploadFile(file, pid);
            }

            RefreshListAsync();
        }

        private async void button5_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == null)
                return;
            if (textBox3.Text == null)
                return;
            
            string pid = "";
            if (treeView1.SelectedNode == null)
                pid = null;
            else
                pid = treeView1.SelectedNode.Name;

            await SimpleClient.Instance.uploadFileFromUrl(textBox2.Text, textBox3.Text, pid);

            RefreshListAsync();
        }

        private async void button7_Click(object sender, EventArgs e)
        {

            if (treeView1.SelectedNode == null)
                return;

            var pid = treeView1.SelectedNode.Name;
            await SimpleClient.Instance.deleteItem(pid);
            RefreshListAsync();

        }
    }
}
