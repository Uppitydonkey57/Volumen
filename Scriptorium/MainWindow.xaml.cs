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
using PdfSharp;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using Scriptorium.Plugins;

namespace Scriptorium
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<VocabNote> vocabNotes = new List<VocabNote>();
        List<GeneralNote> generalNotes = new List<GeneralNote>();
        List<LatinNote> latinNotes = new List<LatinNote>();

        List<Plugin> plugins = new List<Plugin>();

        public Menu menu { get { return Menu; } set { Menu = value; } }

        public MainWindow()
        {
            InitializeComponent();

            string[] files = Directory.GetFiles(@"Plugins/","*.lua");

            foreach (string file in files)
            {
                LuaPlugin plugin = new LuaPlugin(file, false);
                plugins.Add(plugin);
            }

            foreach (Plugin plugin in plugins)
            {
                plugin.window = this;

                if (plugin.shouldRun)
                {
                    plugin.OnRun();
                }
            }
        }

        #region File Loading

        public void NewFile(object sender, RoutedEventArgs e)
        {
            Title.Text = "";
            Text.Text = "";
            TranslationBox.Text = "";
            TranslationVisibleBox.IsChecked = true;
            vocabNotes = new List<VocabNote>();
            VocabTitle.Text = "";
            VocabDefinition.Text = "";
            VocabList.Items.Clear();

            generalNotes = new List<GeneralNote>();
            GeneralNoteTitle.Text = "";
            GeneralNoteText.Text = "";
            GeneralNoteList.Items.Clear();

            latinNotes = new List<LatinNote>();
            LatinNoteTitle.Text = "";
            LatinNoteText.Text = "";
            LatinNoteList.Items.Clear();

        }

        public void OpenFile(object sender, RoutedEventArgs e)
        {
            currentWord = -1;
            currentGeneralNote = -1;
            currentLatinNote = -1;

            NewFile(null, null);
            LatinText openText = JsonToLatin();

            if (openText != null)
            {
                Text.Text = openText.text;
                Title.Text = openText.name;
                TranslationBox.Text = openText.translation;
                TranslationVisibleBox.IsChecked = openText.translationVisible;

                Text.Text = openText.text;
                Title.Text = openText.name;
                TranslationBox.Text = openText.translation;
                TranslationVisibleBox.IsChecked = openText.translationVisible;

                if (openText.vocabNotes != null)
                {
                    vocabNotes = openText.vocabNotes.ToList();
                    VocabTitle.Text = vocabNotes[0].word;
                    VocabDefinition.Text = vocabNotes[0].description;
                    foreach (VocabNote note in vocabNotes)
                    {
                        VocabList.Items.Add(new ListBoxItem() { Content = note.word });
                    }
                }
                VocabList.SelectedItem = 0;
                currentWord = 0;

                if (openText.generalNotes != null)
                {
                    generalNotes = openText.generalNotes.ToList();
                    GeneralNoteTitle.Text = generalNotes[0].title;
                    GeneralNoteText.Text = generalNotes[0].description;
                    foreach (GeneralNote note in generalNotes)
                    {
                        GeneralNoteList.Items.Add(new ListBoxItem() { Content = note.title });
                    }
                }
                GeneralNoteList.SelectedItem = 0;
                currentGeneralNote = 0;

                if (openText.latinNotes != null)
                {
                    latinNotes = openText.latinNotes.ToList();
                    LatinNoteTitle.Text = latinNotes[0].title;
                    LatinNoteText.Text = latinNotes[0].description;
                    foreach (LatinNote note in latinNotes)
                    {
                        LatinNoteList.Items.Add(new ListBoxItem() { Content = note.title });
                    }
                }
                LatinNoteList.SelectedItem = 0;
                currentLatinNote = 0;
            }
        }

        public void SaveFile(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON file (*.json)|*.json";

            LatinText latinText = new LatinText();
            latinText.name = Title.Text;
            latinText.text = Text.Text;
            latinText.translationVisible = (bool)TranslationVisibleBox.IsChecked;
            latinText.translation = TranslationBox.Text;
            latinText.vocabNotes = vocabNotes.ToArray();
            latinText.generalNotes = generalNotes.ToArray();
            latinText.latinNotes = latinNotes.ToArray();

            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, JsonConvert.SerializeObject(latinText));
        }

        public void SaveFileAs(object sender, RoutedEventArgs e)
        {

        }

        public void ExportPDF(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF file (*.pdf)|*.pdf";

            PdfDocument pdf = new PdfDocument();
            pdf.Info.Title = Title.Text;
            PdfPage titlePage = pdf.AddPage();
            XGraphics titleGraphics = XGraphics.FromPdfPage(titlePage);
            //XFont font = new XFont("", 20);

            if (saveFileDialog.ShowDialog() == true)
                pdf.Save(saveFileDialog.FileName);
        }

        #endregion
        
        //I know just reusing the same code 3 times probably isn't very efficient I just don't know a better way to do it.
        #region Vocab
        public void AddToVocab(object sender, RoutedEventArgs e)
        {
            ListBoxItem item = new ListBoxItem();
            item.Content = VocabTitle.Text;

            VocabNote note = new VocabNote();
            note.id = (VocabList.Items.Count - 1).ToString();
            note.word = VocabTitle.Text;
            note.description = VocabDefinition.Text;

            if ((VocabList.Items.Count == 0 || (currentWord > 0 && vocabNotes[currentWord].word == "")) && string.IsNullOrEmpty(VocabTitle.Text))
            {
                note.word = "New Word";
                VocabTitle.Text = "New Word";
                item.Content = "New Word";
            }

            VocabList.Items.Add(item);
            vocabNotes.Add(note);

            currentWord = VocabList.Items.Count - 1;
        }

        public void TitleChanged(object sender, TextChangedEventArgs e)
        {
            if (currentWord >= 0 && VocabList.Items.Count > 0)
            {
                //change vocablist when reusing function
                vocabNotes[currentWord].word = VocabTitle.Text;
                ListBoxItem vocabItem = new ListBoxItem() { Content = vocabNotes[currentWord].word };
                VocabList.Items[currentWord] = vocabItem;
            }
        }

        public void DescriptionChanged(object sender, TextChangedEventArgs e)
        {
            if (currentWord >= 0 && VocabList.Items.Count > 0)
            {
                vocabNotes[currentWord].description = VocabDefinition.Text;
            }
        }

        public void DeleteVocabItem(object sender, RoutedEventArgs e)
        {
            if (VocabList.SelectedIndex >= 0)
            {
                vocabNotes.RemoveAt(VocabList.SelectedIndex);
                VocabList.Items.Remove(VocabList.SelectedItem);
            }

        }

        int currentWord = 0;

        private void WordChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VocabList.SelectedIndex >= 0 && VocabList.Items.Count > 0)
            {
                currentWord = VocabList.SelectedIndex;
                Console.WriteLine(currentWord + " " + VocabList.SelectedIndex + " " + VocabList.Items.Count);

                VocabNote note = vocabNotes[currentWord];

                VocabDefinition.Text = note.description;
                VocabTitle.Text = note.word;
            }
        }

        #endregion

        #region General Notes
        public void AddToGeneralNote(object sender, RoutedEventArgs e)
        {
            ListBoxItem item = new ListBoxItem();
            item.Content = GeneralNoteTitle.Text;

            GeneralNote note = new GeneralNote();
            note.id = (GeneralNoteList.Items.Count - 1).ToString();
            note.title = GeneralNoteTitle.Text;
            note.description = GeneralNoteText.Text;

            if ((GeneralNoteList.Items.Count == 0 || (currentGeneralNote > 0 && vocabNotes[currentGeneralNote].word == "")) && string.IsNullOrEmpty(GeneralNoteTitle.Text))
            {
                note.title = "New Note";
                GeneralNoteTitle.Text = "New Note";
                item.Content = "New Note";
            }

            GeneralNoteList.Items.Add(item);
            generalNotes.Add(note);

            currentGeneralNote = GeneralNoteList.Items.Count - 1;
        }

        public void TitleChangedGeneral(object sender, TextChangedEventArgs e)
        {
            if (currentGeneralNote >= 0 && GeneralNoteList.Items.Count > 0)
            {
                //change vocablist when reusing function
                generalNotes[currentGeneralNote].title = GeneralNoteTitle.Text;
                ListBoxItem vocabItem = new ListBoxItem() { Content = generalNotes[currentGeneralNote].title };
                GeneralNoteList.Items[currentGeneralNote] = vocabItem;
            }
        }

        public void DescriptionChangedGeneral(object sender, TextChangedEventArgs e)
        {
            if (currentGeneralNote >= 0 && GeneralNoteList.Items.Count > 0)
            {
                generalNotes[currentGeneralNote].description = GeneralNoteText.Text;
            }
        }

        public void DeleteGeneralNote(object sender, RoutedEventArgs e)
        {
            if (GeneralNoteList.SelectedIndex >= 0)
            {
                generalNotes.RemoveAt(GeneralNoteList.SelectedIndex);
                GeneralNoteList.Items.Remove(GeneralNoteList.SelectedItem);
            }

        }

        int currentGeneralNote = 0;

        private void GeneralNoteChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GeneralNoteList.SelectedIndex >= 0 && GeneralNoteList.Items.Count > 0)
            {
                currentGeneralNote = GeneralNoteList.SelectedIndex;
                Console.WriteLine(currentGeneralNote + " " + VocabList.SelectedIndex + " " + VocabList.Items.Count);

                GeneralNote note = generalNotes[currentGeneralNote];

                GeneralNoteText.Text = note.description;
                GeneralNoteTitle.Text = note.title;
            }
        }

        #endregion

        #region Latin Notes
        public void AddToLatinNotes(object sender, RoutedEventArgs e)
        {
            ListBoxItem item = new ListBoxItem();
            item.Content = LatinNoteTitle.Text;

            LatinNote note = new LatinNote();
            note.id = (LatinNoteList.Items.Count - 1).ToString();
            note.title = LatinNoteTitle.Text;
            note.description = LatinNoteText.Text;

            if ((LatinNoteList.Items.Count == 0 || (currentLatinNote > 0 && vocabNotes[currentLatinNote].word == "")) && string.IsNullOrEmpty(LatinNoteTitle.Text))
            {
                note.title = "New Note";
                LatinNoteTitle.Text = "New Note";
                item.Content = "New Note";
            }

            LatinNoteList.Items.Add(item);
            latinNotes.Add(note);

            currentLatinNote = LatinNoteList.Items.Count - 1;
        }

        public void TitleChangedLatin(object sender, TextChangedEventArgs e)
        {
            if (currentLatinNote >= 0 && LatinNoteList.Items.Count > 0)
            {
                //change vocablist when reusing function
                latinNotes[currentLatinNote].title = LatinNoteTitle.Text;
                ListBoxItem vocabItem = new ListBoxItem() { Content = latinNotes[currentLatinNote].title };
                LatinNoteList.Items[currentLatinNote] = vocabItem;
            }
        }

        public void DescriptionChangedLatin(object sender, TextChangedEventArgs e)
        {
            if (currentLatinNote >= 0 && LatinNoteList.Items.Count > 0)
            {
                latinNotes[currentLatinNote].description = LatinNoteText.Text;
            }
        }

        public void DeleteLatinNote(object sender, RoutedEventArgs e)
        {
            if (LatinNoteList.SelectedIndex >= 0)
            {
                latinNotes.RemoveAt(LatinNoteList.SelectedIndex);
                LatinNoteList.Items.Remove(LatinNoteList.SelectedItem);
            }

        }

        int currentLatinNote = 0;

        private void LatinNoteChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LatinNoteList.SelectedIndex >= 0 && LatinNoteList.Items.Count > 0)
            {
                currentLatinNote = LatinNoteList.SelectedIndex;
                Console.WriteLine(currentLatinNote + " " + VocabList.SelectedIndex + " " + VocabList.Items.Count);

                LatinNote note = latinNotes[currentLatinNote];

                LatinNoteText.Text = note.description;
                LatinNoteTitle.Text = note.title;
            }
        }

        #endregion

        #region Json & Object Formats

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

        public LatinText GetCurrentVersion()
        {
            LatinText latinText = new LatinText();
            latinText.name = Title.Text;
            latinText.text = Text.Text;
            latinText.translationVisible = (bool)TranslationVisibleBox.IsChecked;
            latinText.translation = TranslationBox.Text;
            latinText.vocabNotes = vocabNotes.ToArray();
            latinText.generalNotes = generalNotes.ToArray();
            latinText.latinNotes = latinNotes.ToArray();
            return latinText;
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
        #endregion
    }
}
