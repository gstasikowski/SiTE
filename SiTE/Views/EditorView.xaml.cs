using SiTE.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SiTE.Views
{
    /// <summary>
    /// Interaction logic for EditorView.xaml
    /// </summary>
    public partial class EditorView : IPageViewModel
    {
        private Task autosaveTask;
        private CancellationTokenSource cancellationToken;

        private bool noteModified = false;

        public EditorView()
        {
            InitializeComponent();
            Logic.FileOperations.InitialSetup();

            WindowSetup();
            LoadNoteList();
            NewNote(); // TODO see if undo/redo buttons can start as disabled without calling this method
        }

        #region Methods
        private void WindowSetup()
        {
            string appTitle = Application.ResourceAssembly.GetName().ToString();
            Application.Current.MainWindow.Title = appTitle.Substring(0, appTitle.IndexOf(','));
            lv_NoteList.ItemsSource = Logic.Refs.dataBank.NoteList;

            if (noteModified)
                Application.Current.MainWindow.Title += '*';
        }

        private void LoadNoteList()
        {
            Logic.FileOperations.GetNoteList();
        }

        private void NewNote()
        {
            lv_NoteList.SelectedIndex = -1;
            tb_Title.Text = "";
            ta_Note.Document = new FlowDocument();
            Logic.Refs.dataBank.NoteCurrentOpen = Logic.Refs.dataBank.NoteLastSaveTime = string.Empty;
            SetFontFamilySelection(ta_Note.FontFamily);
            SetFontSizeSelection(ta_Note.FontSize.ToString());
            SetModifiedState(false);
        }

        private void OpenNote()
        {
            if (lv_NoteList.SelectedIndex < 0 || !CheckSaveReminder())
                return;

            ta_Note.IsUndoEnabled = false; // TODO figure out a better way to do this
            
            var tempItem = (Models.NoteModel)lv_NoteList.SelectedItem;
            var tempNote = Logic.FileOperations.LoadNote(tempItem.ID);

            tb_Title.Text = tempNote.Title;
            var tempTextRange = new TextRange(ta_Note.Document.ContentStart, ta_Note.Document.ContentEnd);
            tempTextRange.Text = tempNote.Content;

            Logic.Refs.dataBank.NoteCurrentOpen = tempNote.ID.ToString();
            Logic.Refs.dataBank.NoteLastSaveTime = tempNote.Modified.ToString();

            SetModifiedState(false);
            ta_Note.IsUndoEnabled = true; // TODO figure out a better way to do this

            //autosaveTask = SetAutoSaveTask();
            ResetAutosave();
        }

        private void SaveNote()
        {
            string noteID = null;

            if (lv_NoteList.SelectedIndex >= 0)
            {
                var tempItem = (Models.NoteModel)lv_NoteList.SelectedItem;
                noteID = tempItem.ID.ToString();
            }

            Logic.FileOperations.SaveNote(noteID, tb_Title.Text, ta_Note.Document.ContentStart, ta_Note.Document.ContentEnd);

            LoadNoteList();

            if (lv_NoteList.SelectedIndex < 0)
            { lv_NoteList.SelectedIndex = lv_NoteList.Items.IndexOf(tb_Title.Text); }

            SetModifiedState(false);
            //autosaveTask = SetAutoSaveTask();
            ResetAutosave();
        }

        private void DeleteNote()
        {
            if (lv_NoteList.SelectedIndex < 0)
                return;

            var tempItem = (Models.NoteModel)lv_NoteList.SelectedItem;
            Logic.FileOperations.DeleteNote(tempItem.ID);
            NewNote();
            LoadNoteList();
        }

        // TODO add note modification time
        private void CheckModified()
        {
            btn_UndoMenu.IsEnabled = btn_UndoToolbar.IsEnabled = ta_Note.CanUndo;
            btn_RedoMenu.IsEnabled = btn_RedoToolbar.IsEnabled = ta_Note.CanRedo;

            WindowSetup();
            lbl_LastSaveTime.Content = (Logic.Refs.dataBank.NoteLastSaveTime != string.Empty) ? (string)FindResource("NoteLastSaveTime") + ": " + Logic.Refs.dataBank.NoteLastSaveTime : string.Empty;
        }

        private bool CheckSaveReminder()
        {
            if (ta_Note.CanUndo)
            {
                // TODO show save prompt (yes/no/cancel)
                // return false when canceling operation
            }

            return true;
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
                cbValue = cb_FontSize.Text; // TODO attempt to add custom font sizes, doesn't work
            }
            else
            {
                cbValue = cb_FontSize.SelectedValue.ToString();
                cbValue = cbValue.Substring(cbValue.LastIndexOf(": ") + 2);
            }

            double fontSize;
            double.TryParse(cbValue, out fontSize);
            
            if (fontSize > 0)
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

        private void FontUnderline() // TODO change to allow for underline + strikethrough
        {
            if (ta_Note.Selection.GetPropertyValue(Inline.TextDecorationsProperty) != TextDecorations.Underline)
                ta_Note.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
            else
                ta_Note.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Baseline);
        }

        private void FontStrike() // TODO change to allow for underline + strikethrough
        {
            if (ta_Note.Selection.GetPropertyValue(Inline.TextDecorationsProperty) != TextDecorations.Strikethrough)
                ta_Note.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Strikethrough);
            else
                ta_Note.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Baseline);
        }

        private void FontColor()
        {
            // TODO Implement
        }

        private void FontHighlight()
        {
            // TODO Implement
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
            Logic.Refs.viewControl.CurrentPageViewModel = Logic.Refs.viewControl.PageViewModels[1]; // TODO switch to binding
        }

        private void ExitApp()
        {
            // TODO throw save reminder in note was modified
            Application.Current.Shutdown();
        }

        private void UpdateTextStyle()
        {
            if (ta_Note.Selection.IsEmpty)
                return;
            
            SetFontFamilySelection(ta_Note.Selection.GetPropertyValue(TextElement.FontFamilyProperty));
            SetFontSizeSelection(ta_Note.Selection.GetPropertyValue(TextElement.FontSizeProperty).ToString());
        }

        private void SetFontFamilySelection(object fontFamily)
        {
            if (!fontFamily.ToString().Contains("UnsetValue"))
            { cb_Font.SelectedItem = fontFamily; }
        }

        private void SetFontSizeSelection(string fontSize)
        {
            if (!fontSize.Contains("UnsetValue"))
            { cb_FontSize.Text = fontSize; }
        }

        private void ResetAutosave()
        {
            return; // temp solution until fixed

            if (autosaveTask != null && !autosaveTask.IsCompleted)
            { 
                cancellationToken.Cancel();

                if (autosaveTask.IsCanceled)
                { autosaveTask.Dispose(); }
            }

            cancellationToken = new CancellationTokenSource();

            autosaveTask = AutoSaveTask();
        }

        private async Task AutoSaveTask() // TODO reset timer on saving/opening note (cancel and recreate task?)
        {
            if (System.Convert.ToBoolean(Logic.Refs.dataBank.GetSetting("autoSave")) && Logic.Refs.dataBank.NoteCurrentOpen != string.Empty)
            {
                using (cancellationToken)
                {
                    await Task.Run(async delegate
                    {
                        int autoSaveDelay = 5;
                        int.TryParse(Logic.Refs.dataBank.GetSetting("autoSaveDelay"), out autoSaveDelay);
                        await Task.Delay(autoSaveDelay * 60000);
                    });

                    SaveNote();
                }
            }
        }

        private void SetModifiedState(bool isModified)
        {
            noteModified = isModified;
            CheckModified();
        }
        #endregion Methods

        #region UI Events
        private void TANoteContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetModifiedState(true);
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

        private void TANote_SelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdateTextStyle();
        }
        #endregion UI Events
    }
}
