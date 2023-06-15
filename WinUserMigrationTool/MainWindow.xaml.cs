using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace WinUserMigrationTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            config = ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        private Configuration config;
        private async Task<List<string>> GetAllNotHiddenUsers()
        {
            var copydirs = new List<string>();
            string[] dirs = Directory.GetDirectories("C:\\Users");
            DirectoryInfo dirin = new DirectoryInfo(dirs[0]);
            foreach (string dir in dirs)
            {
                DirectoryInfo dirinfo = new DirectoryInfo(dir);
                if (!dirinfo.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    copydirs.Add(dirinfo.FullName);
                }
            }
            return copydirs;
        }

        private async void CopyPasteUser(string sourcePath, string targetPath)
        {
            try
            {
                string[] dirs = Directory.GetDirectories(sourcePath, "*");
                //Now Create all of the directories
                //Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories)
                foreach (string dirPath in dirs)
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(dirPath);
                    if (!directoryInfo.Attributes.HasFlag(FileAttributes.Hidden))
                    {
                        Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
                    }
                }

                //Copy all the files & Replaces any files with the same name
                foreach (string newPath in Directory.GetFiles(sourcePath, "*"/*, SearchOption.AllDirectories*/))
                {
                    File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
                }
                MessageBox.Show(sourcePath + " was copied to " + targetPath + " successfully!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void PopulateUserFolderListbox()
        {
            var task = GetAllNotHiddenUsers();
            var userList = await task;

            foreach (var user in userList)
            {
                UserListBox.Items.Add(user);
            }
        }

        private async Task<List<string>> GetNetworkDrives()
        {
            DriveInfo[] driveInfos = DriveInfo.GetDrives();
            List<string> networkDrives = new List<string>();
            foreach (var driveInfo in driveInfos)
            {
                if (driveInfo.DriveType == DriveType.Network)
                {
                    networkDrives.Add(driveInfo.ToString());
                }
            }
            return networkDrives;
        }

        private async void MapNetworkDrives(List<string> drives)
        {
            foreach (string drive in drives)
            {
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = "net";
                p.StartInfo.Arguments = $" use R: {drive}";
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.WaitForExit();
                p.Dispose();
            }
        }

        private async void SaveUncsToConfig(List<string> drives)
        {
            foreach (string drive in drives)
            {
                config.AppSettings.Settings.Add("paths", drive);
            }
            config.Save(ConfigurationSaveMode.Minimal);
        }

        private async void CopyTestButton_Click(object sender, RoutedEventArgs e)
        {
            //CopyPasteUser(@"C:\temp\testuser", @"C:\temp\testdestination\testuser");

            var selectedItems = UserListBox.SelectedItems;
            foreach (string item in selectedItems)
            {
                string topfolder = new DirectoryInfo(item).Name;
                CopyPasteUser(item, AppDomain.CurrentDomain.BaseDirectory + topfolder);
            }
            List<string> uncpaths = await GetNetworkDrives();
            SaveUncsToConfig(uncpaths);
            //uncpaths.Add("Z:\\");
            //MapNetworkDrives(uncpaths);

        }

        private void PopulateButton_Click(object sender, RoutedEventArgs e)
        {
            PopulateUserFolderListbox();
        }
    }
}
