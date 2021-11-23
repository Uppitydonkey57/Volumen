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
using Newtonsoft.Json;
using System.IO;
using Microsoft.Win32;

namespace Scriptorium
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
        
        public void NewFile(object sender, RoutedEventArgs e)
        {
            Title.Text = "";
            Text.Text = "";
        }

        public void OpenFile(object sender, RoutedEventArgs e)
        {
            LatinText openText = JsonToLatin();
            Text.Text = openText.text;
            Title.Text = openText.name;
        }

        public void SaveFile(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON file (*.json)|*.json";

            LatinText latinText = new LatinText();
            latinText.name = Title.Text;
            latinText.text = Text.Text;

            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, JsonConvert.SerializeObject(latinText));
        }

        public void SaveFileAs(object sender, RoutedEventArgs e)
        {

        }

        public LatinText JsonToLatin()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON Files (*.json)|*.json|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                string fileName = openFileDialog.FileName;

                using (StreamReader sr = new StreamReader(fileName))
                {
                    LatinText latinText = JsonConvert.DeserializeObject<LatinText>(sr.ReadToEnd());
                    return latinText;
                }
            }

            return null;
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
