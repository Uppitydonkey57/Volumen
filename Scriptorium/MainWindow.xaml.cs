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
        List<VocabNote> vocabNotes = new List<VocabNote>();

        public MainWindow()
        {
            InitializeComponent();
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
        }

        public void OpenFile(object sender, RoutedEventArgs e)
        {
            currentWord = -1;

            NewFile(null, null);
            LatinText openText = JsonToLatin();

            if (openText != null)
            {
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

            if (saveFileDialog.ShowDialog() == true)
                File.WriteAllText(saveFileDialog.FileName, JsonConvert.SerializeObject(latinText));
        }

        public void SaveFileAs(object sender, RoutedEventArgs e)
        {

        }

        #endregion

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

        #region File Saving & Formats

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
