using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.DirectoryServices;

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
        private async Task<List<string>> GetAllNotHiddenUsers(string dirToCopyFrom)
        {
            var copydirs = new List<string>();
            //string[] dirs = Directory.GetDirectories("C:\\Users");
            string[] dirs = Directory.GetDirectories(dirToCopyFrom);
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

        public string[] FilterDirs(string[] input)
        {
            List<string> filteredDirs = new List<string>();
            string[] filter = 
            {
                "Kuvat", "Pictures", 
                "Ladatut tiedostot", "Downloads", 
                "Musiikki", "Music", 
                "Suosikit", "Favorites",
                "Tiedostot", "Documents",
                "Työpöytä", "Desktop",
                "Videot", "Videos",
                //"AppData\\Local\\Google\\Chrome\\User Data\\Default"
                "AppData"
            };
            foreach(string d in input)
            {
                foreach (string f in filter)
                {
                    if (d.EndsWith(f))
                    {
                        filteredDirs.Add(d);
                    }
                }
            }

            int modifyIndex = filteredDirs.FindIndex(s => s.EndsWith("AppData"));
            filteredDirs[modifyIndex] += "\\Local\\Google\\Chrome\\User Data\\Default";

            string[] output = filteredDirs.ToArray();
            return output;
        }
        private async void CopyPasteUser(string sourcePath, string targetPath)
        {
            try
            {
                string[] dirs = Directory.GetDirectories(sourcePath, "*");
                string test = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string[] filteredDirs = FilterDirs(dirs);

                //Now Create all of the directories
                //Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories)
                foreach (string dirPath in dirs)
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(dirPath);
                    /*if (!directoryInfo.Attributes.HasFlag(FileAttributes.Hidden))
                    {
                        Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
                    }*/
                    Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
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

        private async void PopulateUserFolderListbox(ListBox incomingListBox)
        {
            // Populate local users list
            if(incomingListBox.Name == "UserListBox")
            {
                var task = GetAllNotHiddenUsers("C:\\Users");
                var userList = await task;

                foreach (var user in userList)
                {
                    incomingListBox.Items.Add(user);
                }
            }

            // Populate already copied users list
            if(incomingListBox.Name == "UserRestoreListbox")
            {
                string restorableUsersFolders= AppDomain.CurrentDomain.BaseDirectory + "CopiedUsers\\";
                var task = GetAllNotHiddenUsers(restorableUsersFolders);
                var restoreUserList = await task;

                foreach (var user in restoreUserList)
                {
                    incomingListBox.Items.Add(user);
                }
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
            try
            {
                foreach (string drive in drives)
                {
                    config.AppSettings.Settings.Add("paths", drive);
                }
                config.Save(ConfigurationSaveMode.Minimal);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private async void CopyTestButton_Click(object sender, RoutedEventArgs e)
        {
            //CopyPasteUser(@"C:\temp\testuser", @"C:\temp\testdestination\testuser");

            var selectedItems = UserListBox.SelectedItems;
            string savedUsersdir = AppDomain.CurrentDomain.BaseDirectory + "CopiedUsers\\";
            if (!Directory.Exists(savedUsersdir))
            {
                Directory.CreateDirectory(savedUsersdir);
            }
            foreach (string item in selectedItems)
            {
                string topfolder = new DirectoryInfo(item).Name;
                CopyPasteUser(item, savedUsersdir + topfolder);
            }
            //uncpaths.Add("Z:\\");
            //MapNetworkDrives(uncpaths);

        }

        private void PopulateButton_Click(object sender, RoutedEventArgs e)
        {
            PopulateUserFolderListbox(UserListBox);
        }

        private async void SaveNdrivesButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> uncpaths = await GetNetworkDrives();
            SaveUncsToConfig(uncpaths);
            MessageBox.Show("Network drive paths saved to config successfully!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PopulateRestoreViewButton_Click(object sender, RoutedEventArgs e)
        {
            PopulateUserFolderListbox(UserRestoreListbox);
        }

        private void RestoreUserButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = UserRestoreListbox.Items;

            string RestoreTo = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList", "ProfilesDirectory", "").ToString();

            foreach (string item in selectedItems)
            {
                string topfolder = new DirectoryInfo(item).Name;
                string localFolder = RestoreTo + topfolder;
                if (!Directory.Exists(localFolder))
                {
                    CopyPasteUser(item, RestoreTo);
                }   
            }
        }
    }
}
