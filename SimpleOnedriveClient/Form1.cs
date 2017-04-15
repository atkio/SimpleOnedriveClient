using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleOnedriveClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            if(AuthHelper.TryRefreshToken())
            {
                graphClient = AuthHelper.GetClient();
                LoadFolderFromPath();
            }
            else
            {
                MessageBox.Show("need signin!");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new Signin().Show();
        }

        private const int UploadChunkSize = 10 * 1024 * 1024;       // 10 MB
        //private IOneDriveClient oneDriveClient { get; set; }
        private GraphServiceClient graphClient { get; set; }
      
        private DriveItem CurrentFolder { get; set; }
        private DriveItem SelectedItem { get; set; }

        private OneDriveTile _selectedTile;

        private async Task LoadFolderFromPath()
        {
            if (null == this.graphClient) return;

            LoadChildren(new DriveItem[0]);

            try
            {
                DriveItem folder;

                var expandValue = "thumbnails,children($expand=thumbnails)";
                 
              
                folder = await this.graphClient.Drive.Root.Request().Expand(expandValue).GetAsync();
               

                ProcessFolder(folder);

                await new SimpleClient(AuthStore.Instance.access_token).CreateFolder("cosplay", "1");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

        
        }

        private async Task LoadFolderFromId(string id)
        {
            if (null == this.graphClient) return;

            // Update the UI for loading something new
          
            LoadChildren(new DriveItem[0]);

            try
            {
                var expandValue = "thumbnails,children($expand=thumbnails)";



                var folder =
                    await this.graphClient.Drive.Items[id].Request().Expand(expandValue).GetAsync();

             

                ProcessFolder(folder);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

          
        }

        private void ProcessFolder(DriveItem folder)
        {
            if (folder != null)
            {
                this.CurrentFolder = folder;

                LoadProperties(folder);

                if (folder.Folder != null && folder.Children != null && folder.Children.CurrentPage != null)
                {
                    LoadChildren(folder.Children.CurrentPage);
                }
            }
        }

        private void LoadProperties(DriveItem item)
        {
            this.SelectedItem = item;
          
        }

        private void LoadChildren(IList<DriveItem> items)
        {
            flowLayoutContents.SuspendLayout();
            flowLayoutContents.Controls.Clear();

            // Load the children
            foreach (var obj in items)
            {
                AddItemToFolderContents(obj);
            }

            flowLayoutContents.ResumeLayout();
        }

        private void AddItemToFolderContents(DriveItem obj)
        {
            flowLayoutContents.Controls.Add(CreateControlForChildObject(obj));
        }

        private void RemoveItemFromFolderContents(DriveItem itemToDelete)
        {
            flowLayoutContents.Controls.RemoveByKey(itemToDelete.Id);
        }

        private Control CreateControlForChildObject(DriveItem item)
        {
            OneDriveTile tile = new OneDriveTile(this.graphClient);
            tile.SourceItem = item;
            tile.Click += ChildObject_Click;
            tile.DoubleClick += ChildObject_DoubleClick;
            tile.Name = item.Id;
            return tile;
        }

        void ChildObject_DoubleClick(object sender, EventArgs e)
        {
            var item = ((OneDriveTile)sender).SourceItem;
         

            // Look up the object by ID
            LoadFolderFromId(item.Id);

           
        }
        void ChildObject_Click(object sender, EventArgs e)
        {
            if (null != _selectedTile)
            {
                _selectedTile.Selected = false;
            }

            var item = ((OneDriveTile)sender).SourceItem;
            LoadProperties(item);
            _selectedTile = (OneDriveTile)sender;
            _selectedTile.Selected = true;
        }

        private void FormBrowser_Load(object sender, EventArgs e)
        {

        }

        //private void NavigateToFolder(DriveItem folder)
        //{
        //    Task t = LoadFolderFromId(folder.Id);

        //    // Fix up the breadcrumbs
        //    var breadcrumbs = flowLayoutPanelBreadcrumb.Controls;
        //    bool existingCrumb = false;
        //    foreach (LinkLabel crumb in breadcrumbs)
        //    {
        //        if (crumb.Tag == folder)
        //        {
        //            RemoveDeeperBreadcrumbs(crumb);
        //            existingCrumb = true;
        //            break;
        //        }
        //    }

        //    if (!existingCrumb)
        //    {
        //        LinkLabel label = new LinkLabel();
        //        label.Text = "> " + folder.Name;
        //        label.LinkArea = new LinkArea(2, folder.Name.Length);
        //        label.LinkClicked += linkLabelBreadcrumb_LinkClicked;
        //        label.AutoSize = true;
        //        label.Tag = folder;
        //        flowLayoutPanelBreadcrumb.Controls.Add(label);
        //    }
        //}

        private void linkLabelBreadcrumb_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel link = (LinkLabel)sender;

       

            DriveItem item = link.Tag as DriveItem;
            if (null == item)
            {

                Task t = LoadFolderFromPath();
            }
            else
            {
                Task t = LoadFolderFromId(item.Id);
            }
        }

     
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

   

        private System.IO.Stream GetFileStreamForUpload(string targetFolderName, out string originalFilename)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Upload to " + targetFolderName;
            dialog.Filter = "All Files (*.*)|*.*";
            dialog.CheckFileExists = true;
            var response = dialog.ShowDialog();
            if (response != DialogResult.OK)
            {
                originalFilename = null;
                return null;
            }

            try
            {
                originalFilename = System.IO.Path.GetFileName(dialog.FileName);
                return new System.IO.FileStream(dialog.FileName, System.IO.FileMode.Open);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error uploading file: " + ex.Message);
                originalFilename = null;
                return null;
            }
        }

        private async void button2_ClickAsync(object sender, EventArgs e)
        {
            var targetFolder = this.CurrentFolder;

            string filename;
            using (var stream = GetFileStreamForUpload(targetFolder.Name, out filename))
            {
                if (stream != null)
                {
                    try
                    {
                        var uploadedItem =
                            await
                                this.graphClient.Drive.Items[targetFolder.Id].ItemWithPath(filename).Content.Request()
                                    .PutAsync<DriveItem>(stream);

                        AddItemToFolderContents(uploadedItem);

                        MessageBox.Show("Uploaded with ID: " + uploadedItem.Id);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message);
                    }
                }
            }
        }
    }
}
