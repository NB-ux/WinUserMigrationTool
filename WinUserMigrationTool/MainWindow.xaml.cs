using System.Collections.Generic;
using System.IO;
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

        private async void GetAllNotHiddenUsers()
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
        }



        private void CopyTestButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
