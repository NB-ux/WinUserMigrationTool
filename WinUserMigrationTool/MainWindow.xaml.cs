using System.Collections.Generic;
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
        }

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
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
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

        private async void CopyTestButton_Click(object sender, RoutedEventArgs e)
        {
            //CopyPasteUser(@"C:\temp\test1", @"C:\temp\test2");

        }

        private void PopulateButton_Click(object sender, RoutedEventArgs e)
        {
            PopulateUserFolderListbox();
        }
    }
}
