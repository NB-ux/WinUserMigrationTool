using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.Printing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
            CopiedUsersSTR = AppDomain.CurrentDomain.BaseDirectory + "CopiedUsers\\";
            if (!Directory.Exists(CopiedUsersSTR))
            {
                Directory.CreateDirectory(CopiedUsersSTR);
            }
        }

        private string CopiedUsersSTR;
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

        public List<string> NonUserFolderFilter(List<string> input)
        {
            List<string> filtered = new List<string>();
            filtered.AddRange(input);
            string[] filter =
            {
                "3D Objects",
                "ansel",
                "Contacts",
                "Desktop",
                "Documents",
                "Downloads",
                "Favorites",
                "Links",
                "Music",
                "OneDrive",
                "Pictures",
                "Public",
                "Saved Games",
                "Searches",
                "Videos"
            };

            foreach (string d in input)
            {
                foreach (string f in filter)
                {
                    if (d.EndsWith(f))
                    {
                        filtered.Remove(d);
                    }
                }
            }

            return filtered;

        }

        //Folders that are included in Copy process.
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
            foreach (string d in input)
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
                foreach (string dirPath in filteredDirs)
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
            if (incomingListBox.Name == "UserListBox" && incomingListBox.Items.IsEmpty)
            {
                var task = GetAllNotHiddenUsers("C:\\Users");
                var userList = await task;
                var newList = NonUserFolderFilter(userList);

                foreach (var user in newList)
                {
                    incomingListBox.Items.Add(user);
                }
            }

            // Populate already copied users list
            if (incomingListBox.Name == "UserRestoreListbox" && incomingListBox.Items.IsEmpty)
            {
                var task = GetAllNotHiddenUsers(CopiedUsersSTR);
                var restoreUserList = await task;

                foreach (var user in restoreUserList)
                {
                    string topfolder = new DirectoryInfo(user).Name;
                    incomingListBox.Items.Add(topfolder);
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

        private async Task SaveUncsToConfig(List<string> drives)
        {
            try
            {

                foreach (string drive in drives)
                {
                    config.AppSettings.Settings.Add("paths", drive);
                }

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private async void InstallLocalPrinters(List<string> printernames)
        {
            LocalPrintServer printServer = new LocalPrintServer();

            foreach (string printerName in printernames)
            {
                PrintQueue newPrinter = new PrintQueue(printServer, printerName);

                newPrinter.Commit();
            }
        }

        private async Task<List<string>> GetPrinters(string option)
        {

            PrinterSettings printerSettings = new PrinterSettings();
            List<string> localPrinters = new List<string>();
            List<string> networkPrinters = new List<string>();

            switch (option)
            {
                case "local":

                    foreach (string lprinter in PrinterSettings.InstalledPrinters)
                    {
                        localPrinters.Add(lprinter);
                    }
                    break;

                case "network":


                    break;
            }
            if (localPrinters.Count > 0)
            {
                return localPrinters;
            }
            else if (networkPrinters.Count > 0)
            {
                return networkPrinters;
            }
            else
            {
                return null;
            }
        }

        private async void CopyTestButton_Click(object sender, RoutedEventArgs e)
        {
            //CopyPasteUser(@"C:\temp\testuser", @"C:\temp\testdestination\testuser");

            var selectedItems = UserListBox.SelectedItems;
            if (!Directory.Exists(CopiedUsersSTR))
            {
                Directory.CreateDirectory(CopiedUsersSTR);
            }
            foreach (string item in selectedItems)
            {
                string topfolder = new DirectoryInfo(item).Name;
                CopyPasteUser(item, CopiedUsersSTR + topfolder);
                CopyUserOutlookSignatures(topfolder);
            }
            //uncpaths.Add("Z:\\");
            //MapNetworkDrives(uncpaths);

        }

        private async void CopyUserOutlookSignatures(string UserName)
        {
            try
            {
                string CopiedOutlookSignatures = CopiedUsersSTR + UserName + "\\" + "CopiedOutlookSignatures";
                if (!Directory.Exists(CopiedOutlookSignatures))
                {
                    Directory.CreateDirectory(CopiedOutlookSignatures);
                }
                string userappdataFolder = string.Format(@"C:\Users\{0}\AppData\Roaming\Microsoft", UserName);

                string[] subfolders = Directory.GetDirectories(userappdataFolder);

                foreach (var subfolder in subfolders)
                {
                    var topdir = new DirectoryInfo(subfolder).Name;
                    if (topdir == "Signatures" || topdir == "Allekirjoitukset")
                    {
                        //CopyPasteUser(userappdataFolder + "\\" + topdir, CopiedOutlookSignatures);
                        string[] signs = Directory.GetFiles(userappdataFolder + "\\" + topdir);

                        foreach (string sign in signs)
                        {
                            var topfile = new FileInfo(sign).Name;
                            File.Copy(sign, CopiedOutlookSignatures + "\\" + topfile);
                        }
                        break;
                    }
                }


                //CopyPasteUser(userappdataFolder, CopiedOutlookSignatures);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void PopulateButton_Click(object sender, RoutedEventArgs e)
        {
            PopulateUserFolderListbox(UserListBox);
        }

        private async void SaveNdrivesButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> uncpaths = await GetNetworkDrives();
            await SaveUncsToConfig(uncpaths);
            MessageBox.Show("Network drive paths saved to config successfully!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void PopulateRestoreViewButton_Click(object sender, RoutedEventArgs e)
        {
            PopulateUserFolderListbox(UserRestoreListbox);
        }

        // searches location by topmost and returns topmost folders path
        //topmost = topmost folders name. location = Function searches for path inside this folder. 
        private string GetPathByName(string topmost, string location)
        {
            string[] allFolders = Directory.GetDirectories(location);
            string pathToReturn = "";

            foreach (string folder in allFolders)
            {
                string topfolder = new DirectoryInfo(folder).Name;
                if (topfolder == topmost)
                {
                    pathToReturn = folder;
                    break;
                }
            }
            return pathToReturn;
        }

        private void RestoreUserButton_Click(object sender, RoutedEventArgs e)
        {
            //GetPathByName("test", @"C:\\Users\\Niko\\source\\repos\\WinUserMigrationTool\\WinUserMigrationTool\\bin\\Debug\\net6.0-windows\\CopiedUsers");
            var selectedItems = UserRestoreListbox.Items;

            string RestoreTo = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList", "ProfilesDirectory", "").ToString();

            foreach (string item in selectedItems)
            {
                string itempath = GetPathByName(item, CopiedUsersSTR);
                string topfolder = new DirectoryInfo(item).Name;
                string localFolder = RestoreTo + "\\" + topfolder;
                if (!Directory.Exists(localFolder))
                {
                    CopyPasteUser(itempath, RestoreTo);
                }
            }
        }

        private async void SaveAllPrinters_Click(object sender, RoutedEventArgs e)
        {
            var task = GetPrinters("local");
            List<string> printers = await task;
            InstallLocalPrinters(printers);
        }
    }
}
