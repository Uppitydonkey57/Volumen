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
            Text.Text = "";
            GeneralNotesText.Text = "";
            VocabText.Text = "";
            LatinNotesText.Text = "";
            TraslationText.Text = "";

            Text2.Text = "";
            GeneralNotesText2.Text = "";
            VocabText2.Text = "";
            LatinNotesText2.Text = "";
            TraslationText2.Text = "";

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

                ParseText(latinText, Text);
                ParseText(latinText, Text2);

                if (latinText.translationVisible)
                {
                    TraslationText2.Text = latinText.translation;
                    TraslationText.Text = latinText.translation;
                }
                else
                {
                    TraslationText2.Text = "There is no translation available.";
                    TraslationText.Text = "There is no translation available.";
                }

                if (latinText.latinNotes != null)
                {
                    foreach (LatinNote latinNote in latinText.latinNotes)
                    {
                        LatinNotesText.Text += latinNote.id +           "\n";
                        LatinNotesText.Text += latinNote.title +        "\n";
                        LatinNotesText.Text += latinNote.description +  "\n";
                        LatinNotesText.Text +=                          "\n";

                        LatinNotesText2.Text += latinNote.id + "\n";
                        LatinNotesText2.Text += latinNote.title + "\n";
                        LatinNotesText2.Text += latinNote.description + "\n";
                        LatinNotesText2.Text += "\n";
                    }
                }

                if (latinText.generalNotes != null)
                {
                    foreach (GeneralNote latinNote in latinText.generalNotes)
                    {
                        GeneralNotesText.Text += latinNote.id +          "\n";
                        GeneralNotesText.Text += latinNote.title +       "\n";
                        GeneralNotesText.Text += latinNote.description + "\n";
                        GeneralNotesText.Text +=                         "\n";

                        GeneralNotesText2.Text += latinNote.id + "\n";
                        GeneralNotesText2.Text += latinNote.title + "\n";
                        GeneralNotesText2.Text += latinNote.description + "\n";
                        GeneralNotesText2.Text += "\n";
                    }
                }

                if (latinText.vocabNotes != null)
                {
                    foreach (VocabNote latinNote in latinText.vocabNotes)
                    {
                        VocabText.Text += latinNote.id +          "\n";
                        VocabText.Text += latinNote.word +        "\n";
                        VocabText.Text += latinNote.description + "\n";
                        VocabText.Text +=                         "\n";

                        VocabText2.Text += latinNote.id + "\n";
                        VocabText2.Text += latinNote.word + "\n";
                        VocabText2.Text += latinNote.description + "\n";
                        VocabText2.Text += "\n";

                        Console.WriteLine(latinNote.id + " " + latinNote.word + " " + latinNote.description + " ");
                    }
                }
            }
        }

        public class LatinText
        {
            public string name;
            public string text;

            public string translation;
            public bool translationVisible;

            public LatinNote[] latinNotes;
            public GeneralNote[] generalNotes;
            public VocabNote[] vocabNotes;
        }

        public class LatinNote
        {
            public string id;
            public string title;
            public string description;
        }

        public class GeneralNote
        {
            public string id;
            public string title;
            public string description;
        }

        public class VocabNote
        {
            public string id;
            public string word;
            public string description;
        }

        public void ParseText(LatinText latinText, TextBlock textObject)
        {
            string[] splitText = latinText.text.Split('[', ']');

            bool isInBrackets = false;

            bool isBold = false;
            bool isItalic = false;

            string effectedSegment = "";

            foreach (string text in splitText)
            {
                Console.WriteLine(text + " line!");
                if (!isInBrackets)
                {
                    Console.WriteLine(text + "  out of bracket!");

                    if (isBold || isItalic)
                    {
                        effectedSegment += text;
                    }
                    else
                    {
                        textObject.Inlines.Add(new Run(text));
                    }
                }
                else
                {
                    Console.WriteLine(text + " in bracket!");

                    switch (text)
                    {
                        case "sub":
                            isBold = true;
                            break;

                        case "do":
                            isItalic = true;
                            break;

                        case "/sub":
                            isBold = false;
                            textObject.Inlines.Add(new Bold(new Run(effectedSegment)));
                            Console.WriteLine(effectedSegment + " in bold");
                            effectedSegment = "";
                            break;

                        case "/do":
                            isItalic = false;
                            textObject.Inlines.Add(new Italic(new Run(effectedSegment)));
                            Console.WriteLine(effectedSegment + " in italic");
                            effectedSegment = "";
                            break;

                        default:
                            //Text.Text += "[" + text + "]";
                            break;
                    }
                }

                isInBrackets = !isInBrackets;
            }
        }
    }
}
