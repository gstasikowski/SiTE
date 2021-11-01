using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SiTE.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Logic.Refs.fileOperations.LoadTranslations();
            Logic.Refs.fileOperations.LoadAppSettings();

            WindowSetup();
            LoadNoteList();
            NewNote(); // TODO see if undo/redo buttons can start as disabled without calling this method
        }

        private void WindowSetup()
        {
            string appTitle = Application.ResourceAssembly.GetName().ToString();
            Application.Current.MainWindow.Title = appTitle.Substring(0, appTitle.IndexOf(','));

            if (ta_Note.CanUndo)
                Application.Current.MainWindow.Title += '*';
        }

        private void LoadNoteList()
        {
            lv_NoteList.Items.Clear();
            
            foreach (string note in Logic.Refs.fileOperations.GetNoteList())
            {
                lv_NoteList.Items.Add(note.Replace(Logic.Refs.dataBank.DefaultNotePath, "").Replace(".aes", "").Replace(".site", ""));
            }
        }

        private void NewNote()
        {
            tb_Title.Text = "";
            ta_Note.Document = new FlowDocument();
            Logic.Refs.dataBank.CurrentOpenNote = Logic.Refs.dataBank.LastSaveNote = string.Empty;
            CheckModified();
        }

        private void OpenNote()
        {
            if (lv_NoteList.SelectedIndex < 0)
                return;

            if (Logic.Refs.fileOperations.LoadNote(lv_NoteList.SelectedItem.ToString(), ta_Note.Document.ContentStart, ta_Note.Document.ContentEnd))
            {
                ta_Note.IsUndoEnabled = false; // TODO figure out a better way to do this
                tb_Title.Text = lv_NoteList.SelectedItem.ToString();
                CheckModified();
                ta_Note.IsUndoEnabled = true; // TODO figure out a better way to do this
            }
        }

        private void SaveNote()
        {
            Logic.Refs.fileOperations.SaveNote(tb_Title.Text, ta_Note.Document.ContentStart, ta_Note.Document.ContentEnd);
            LoadNoteList();
        }

        private void DeleteNote()
        {
            Logic.Refs.fileOperations.DeleteNote(lv_NoteList.SelectedItem.ToString() + ".aes");
            NewNote();
            LoadNoteList();
        }

        private void CheckModified()
        {
            btn_UndoMenu.IsEnabled = btn_UndoToolbar.IsEnabled = ta_Note.CanUndo;
            btn_RedoMenu.IsEnabled = btn_RedoToolbar.IsEnabled = ta_Note.CanRedo;

            WindowSetup();
            lbl_LastSaveTime.Content = (Logic.Refs.dataBank.LastSaveNote != string.Empty) ? (string)FindResource("NoteLastSaveTime") + ": " + Logic.Refs.dataBank.LastSaveNote : string.Empty;
        }

        private void CutText()
        {
            ta_Note.Cut();
        }

        private void CopyText()
        {
            ta_Note.Copy();
        }

        private void PasteText()
        {
            ta_Note.Paste();
        }

        private void UndoChanges()
        {
            ta_Note.Undo();
        }

        private void RedoChanges()
        {
            ta_Note.Redo();
        }

        private void FontSetFamily()
        {
            if (ta_Note == null)
                return;

            if (ta_Note.Selection != null)
            {
                ta_Note.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, cb_Font.SelectedItem);
                FontSetSize();
            }
        }

        private void FontSetSize()
        {
            if (ta_Note == null)
                return;

            string cbValue;

            if (cb_FontSize.SelectedValue == null)
            {
                cbValue = cb_FontSize.Text; // attempt to add custom font sizes, doesn't work
            }
            else
            {
                cbValue = cb_FontSize.SelectedValue.ToString();
                cbValue = cbValue.Substring(cbValue.LastIndexOf(": ") + 2);
            }

            double fontSize;
            double.TryParse(cbValue, out fontSize);
            
            ta_Note.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize);
            ta_Note.Focus();
        }

        private void FontBold()
        {
            if (ta_Note.Selection.GetPropertyValue(TextElement.FontWeightProperty).ToString() != "Bold")
                ta_Note.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, "Bold");
            else
                ta_Note.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, "Normal");
        }

        private void FontItalic()
        {
            if (ta_Note.Selection.GetPropertyValue(TextElement.FontStyleProperty).ToString() != "Italic")
                ta_Note.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, "Italic");
            else
                ta_Note.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, "Normal");
        }

        private void FontUnderline() // change to allow for underline + strikethrough
        {
            if (ta_Note.Selection.GetPropertyValue(Inline.TextDecorationsProperty) != TextDecorations.Underline)
                ta_Note.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
            else
                ta_Note.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Baseline);
        }

        private void FontStrike() // change to allow for underline + strikethrough
        {
            if (ta_Note.Selection.GetPropertyValue(Inline.TextDecorationsProperty) != TextDecorations.Strikethrough)
                ta_Note.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Strikethrough);
            else
                ta_Note.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Baseline);
        }

        private void FontColor()
        {

        }

        private void FontHighlight()
        {

        }

        private void TextPosition(object sender)
        {
            switch (((Button)sender).Name)
            {
                case "textCenter":
                    ta_Note.Document.TextAlignment = TextAlignment.Center;
                    break;

                case "textRight":
                    ta_Note.Document.TextAlignment = TextAlignment.Right;
                    break;

                case "textJust":
                    ta_Note.Document.TextAlignment = TextAlignment.Justify;
                    break;

                default:
                    ta_Note.Document.TextAlignment = TextAlignment.Left;
                    break;
            }
        }

        private void SpellCheckToggle() // currently non functioning, requires (?) Telerik.Windows.Documents.Proofing.Dictionaries.En-US.dll for EN spellcheck
        {
            ta_Note.SpellCheck.IsEnabled = !ta_Note.SpellCheck.IsEnabled;
        }

        private void ToggleNoteList(bool expand)
        {
            if (!gcl_NoteList.IsLoaded || !gcl_splitter.IsLoaded)
                return;

            gcl_splitter.IsEnabled = expand;
            gcl_NoteList.MinWidth = expand ? 130 : 30;
            gcl_NoteList.Width = GridLength.Auto;
        }

        private void OpenSettings()
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            this.IsEnabled = false;
            settingsWindow.Show();
        }

        private void ExitApp()
        {
            // throw save reminder in note was modified
            Application.Current.Shutdown();
        }

        //* UI Events *//
        private void TANoteContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckModified();
        }

        private void BtnNewNote_Click(object sender, RoutedEventArgs e)
        {
            NewNote();
        }

        private void BtnSaveNote_Click(object sender, RoutedEventArgs e)
        {
            SaveNote();
        }

        private void BtnDeleteNote_Click(object sender, RoutedEventArgs e)
        {
            DeleteNote();
        }

        private void BtnCut_Click(object sender, RoutedEventArgs e)
        {
            CutText();
        }

        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            CopyText();
        }

        private void BtnPaste_Click(object sender, RoutedEventArgs e)
        {
            PasteText();
        }

        private void BtnUndo_Click(object sender, RoutedEventArgs e)
        {
            UndoChanges();
        }

        private void BtnTRedo_Click(object sender, RoutedEventArgs e)
        {
            RedoChanges();
        }

        private void CBFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FontSetFamily();
        }

        private void CBFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FontSetSize();
        }

        private void BtnTBold_Click(object sender, RoutedEventArgs e)
        {
            FontBold();
        }

        private void BtnItalics_Click(object sender, RoutedEventArgs e)
        {
            FontItalic();
        }

        private void BtnUnderline_Click(object sender, RoutedEventArgs e)
        {
            FontUnderline();
        }

        private void BtnStrike_Click(object sender, RoutedEventArgs e)
        {
            FontStrike();
        }

        private void BtnFontColor_Click(object sender, RoutedEventArgs e)
        {
            FontColor();
        }

        private void BtnFontHighlight_Click(object sender, RoutedEventArgs e)
        {
            FontHighlight();
        }

        private void BtnTextPosition_Click(object sender, RoutedEventArgs e)
        {
            TextPosition(sender);
        }

        private void BtnSpellCheck_Click(object sender, RoutedEventArgs e)
        {
            SpellCheckToggle();
        }

        private void MIExit_Click(object sender, RoutedEventArgs e)
        {
            ExitApp();
        }

        private void MISettings_Click(object sender, RoutedEventArgs e)
        {
            OpenSettings();
        }

        private void LVNoteList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OpenNote();
        }

        private void EXNoteList_Collapsed(object sender, RoutedEventArgs e)
        {
            ToggleNoteList(false);
        }

        private void EXNoteList_Expanded(object sender, RoutedEventArgs e)
        {
            ToggleNoteList(true);
        }
    }
}
