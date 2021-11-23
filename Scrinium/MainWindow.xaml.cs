using System;
using System.Collections.Generic;
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
using System.IO;
using System.Web;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace Scrinium
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string currentFileName;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void SelectFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON Files (*.json)|*.json|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                currentFileName = openFileDialog.FileName;
                Console.WriteLine(currentFileName);
                ProcessFile(currentFileName);
            }
        }

        public void ProcessFile(string fileName)
        {
            using (StreamReader sr = new StreamReader(fileName))
            {
                LatinText latinText = JsonConvert.DeserializeObject<LatinText>(sr.ReadToEnd());
                Text.Text = latinText.text;

                if (latinText.footnotes != null)
                {
                    foreach (FootNote footnote in latinText.footnotes)
                    {
                        Footnotes.Text += footnote.id + "\n";
                        Footnotes.Text += footnote.word + "\n";
                        Footnotes.Text += footnote.description + "\n";
                        Footnotes.Text += "\n";
                    }
                }
            }
        }

        public class LatinText
        {
            public string name;
            public string text;

            public FootNote[] footnotes;
        }

        public class FootNote
        {
            public string id;
            public string word;
            public string description;
        }
    }
}
