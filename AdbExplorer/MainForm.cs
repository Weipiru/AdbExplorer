using AdbExplorerService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdbExplorer
{
    public partial class MainForm : Form
    {
        [DllImport("uxtheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern void SetWindowTheme(IntPtr hWnd, string appId, string classId);

        private ExplorerService service = null;
        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitUIStyle();
            InitData();
            InitUI();
        }

        private void InitUIStyle()
        {
            SetWindowTheme(this.lvExplorer.Handle, "Explorer", null);
            SetWindowTheme(this.treeDevices.Handle, "Explorer", null);
            //SetWindowTheme(this.textBox1.Handle, "Explorer", null);
        }

        private void InitData()
        {
            service = new ExplorerService();
            service.Init();
        }


        private void InitUI()
        {
            cbDevices.SelectedIndexChanged += CbDevices_SelectedIndexChanged;

            RefreshDeviceList();

            cbDevices.SelectedIndex = 0;
            
            lvExplorer.MouseDoubleClick += (s, e) =>
            {
               var clickItem= lvExplorer.HitTest(e.X, e.Y);
                if (clickItem != null)
                {
                    var data = clickItem.Item as ListViewItem;
                    if (data.Tag is AndroidFile)
                    {
                        var androidFile= (AndroidFile)data.Tag;
                        if (androidFile.Type == AndroidFile.FileType.Directory || androidFile.Type == AndroidFile.FileType.Link)
                        {
                            if (!androidFile.Name.Equals(".."))
                            {
                                service.Go(androidFile.Name);
                                RefreshExplorer();
                            }
                            else
                            {
                                service.Back();
                                RefreshExplorer();
                            }
                        }
                    }
                   
                }
            };
        }

        private void CbDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            service.SetUseDevice(cbDevices.SelectedIndex);
            RefreshExplorer();
        }

        private void RefreshDeviceList()
        {
            var devices = service.GetDeviceDatas(true);
            cbDevices.Items.Clear();
            treeDevices.Nodes[0].Nodes.Clear();
            foreach (var device in devices)
            {
                var deviceName = String.Format("{0}({1}):{2}", device.Name, device.Model, device.State);
                cbDevices.Items.Add(deviceName);
                treeDevices.Nodes[0].Nodes.Add(deviceName);
            }
            treeDevices.Nodes[0].Expand();
        }

        private void RefreshExplorer(bool focus = false)
        {
            var curFiles = service.RefreshCurrentDir(focus);
            lvExplorer.Items.Clear();
            lvExplorer.BeginUpdate();
            lvExplorer.SmallImageList = imageResourcesSmall;
            lvExplorer.LargeImageList = imageResourceMax;
            var bk = AndroidFile.BackAndroidFile();
            lvExplorer.Items.Add(CreateListViewItem(bk));

            foreach (var file in curFiles)
            {
                ListViewItem lvItem = CreateListViewItem(file);
                lvExplorer.Items.Add(lvItem);
            }
            lvExplorer.Show();
            lvExplorer.EndUpdate();
            statusText.Text = String.Format("本页共{0}项",curFiles.Count);
            tbPath.Text=service.GetCurrentPath();
        }

        private static ListViewItem CreateListViewItem(AndroidFile file)
        {
            var lvItem = new ListViewItem();
            lvItem.Text = file.Name;
            lvItem.Tag = file;
            switch (file.Type)
            {
                case AndroidFile.FileType.Directory:
                    lvItem.ImageIndex = 1;
                    break;
                case AndroidFile.FileType.File:
                    lvItem.ImageIndex = 0;
                    break;
                case AndroidFile.FileType.Link:
                    lvItem.ImageIndex = 1;
                    break;
                default:
                    break;
            }
            lvItem.SubItems.Add(file.Permission);
            lvItem.SubItems.Add(file.CreateDataTime);
            return lvItem;
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
              
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshExplorer(true);
        }
    }
}
