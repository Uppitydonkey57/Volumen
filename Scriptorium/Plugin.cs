using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Scriptorium
{
    abstract public class Plugin
    {
        public string fileName;

        public string path;

        public string menuPath;

        private string logPath = "Plugins/Log.txt";

        public bool shouldRun = true;

        public MainWindow window;

        public abstract void OnRun();

        public void Log(string message)
        {
            File.AppendAllText(logPath, $"{fileName}: {message} \n");
        }

        public void LogPlain(string message)
        {
            File.AppendAllText(logPath + "\n", message);
        }

        public void SaveFile(string fileType, string contents)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            //saveFileDialog.Filter = "JSON file (*.json)|*.json";

            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName + fileType, contents);
        }

        public void ClearLog()
        {
            File.WriteAllText(logPath, string.Empty);
        }

        public MainWindow.LatinText GetData()
        {
            return window.GetCurrentVersion();
        }

        public void CreateMenuItem()
        {
            string[] pathList = menuPath.Split('/');

            ItemCollection currentCheckItem = window.menu.Items;

            for (int i = 0; i < pathList.Length - 1; i++)
            {
                foreach (MenuItem item in currentCheckItem)
                {
                    if (item.Name == pathList[i])
                    {
                        currentCheckItem = item.Items;
                    } 
                }
            }
        }
    }
}
