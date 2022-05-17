using SiTE.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Markdig;
using Neo.Markdig.Xaml;

namespace SiTE.Views
{
    enum EditorMode { editor, mixed, render };

    /// <summary>
    /// Interaction logic for EditorView.xaml
    /// </summary>
    public partial class EditorView : IPageViewModel
    {
        private Task autosaveTask;
        private CancellationTokenSource cancellationToken;

        private EditorMode mode = EditorMode.editor;
        private bool noteModified = false;

        public EditorView()
        {
            InitializeComponent();
            Logic.FileOperations.InitialSetup();

            WindowSetup();
            LoadNoteList();
            NewNote();
        }

        #region Methods
        private void WindowSetup()
        {
            string appTitle = Application.ResourceAssembly.GetName().ToString();
            Application.Current.MainWindow.Title = appTitle.Substring(0, appTitle.IndexOf(','));
            lv_NoteList.ItemsSource = Logic.Refs.dataBank.NoteList;

            if (noteModified)
                Application.Current.MainWindow.Title += '*';
            
            UpdateEditorView();
        }

        private void LoadNoteList()
        {
            Logic.FileOperations.GetNoteList();
        }

        private void NewNote()
        {
            lv_NoteList.SelectedIndex = -1;
            tb_Title.Text = "";
            ta_Note.Text = string.Empty;
            SetModifiedState(false, string.Empty);
        }

        private void OpenNote()
        {
            if (lv_NoteList.SelectedIndex < 0 || !CheckSaveReminder())
                return;

            ta_Note.IsUndoEnabled = false; // TODO figure out a better way to do this
            
            var tempItem = (Models.NoteModel)lv_NoteList.SelectedItem;
            var tempNote = Logic.FileOperations.LoadNote(tempItem.ID);
            
            tb_Title.Text = tempNote.Title;
            ta_Note.Text = tempNote.Content;

            SetModifiedState(false, tempNote.Modified.ToString());
            ta_Note.IsUndoEnabled = true; // TODO figure out a better way to do this

            ResetAutosave();
        }

        private void UpdateMarkdownView()
        {
            var doc = MarkdownXaml.ToFlowDocument(ta_Note.Text,
                new MarkdownPipelineBuilder()
                .UseXamlSupportedExtensions()
                .Build()
            );

            flowDocumentViewer.Document = doc;
        }

        private void SaveNote()
        {
            string noteID = null;

            if (lv_NoteList.SelectedIndex >= 0)
            {
                var tempItem = (Models.NoteModel)lv_NoteList.SelectedItem;
                noteID = tempItem.ID.ToString();
            }

            Logic.FileOperations.SaveNote(noteID, tb_Title.Text, ta_Note.Text);

            LoadNoteList();

            if (lv_NoteList.SelectedIndex < 0)
            { lv_NoteList.SelectedIndex = Logic.Refs.dataBank.GetNoteIndex(tb_Title.Text); }

            SetModifiedState(false, System.DateTime.Now.ToString());
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

        private void ToggleTextBold()
        {
            if (!IsNoteContentActive())
            { return; }

            if (ta_Note.SelectedText.StartsWith("**") && ta_Note.SelectedText.EndsWith("**"))
            { ta_Note.SelectedText = ta_Note.SelectedText.Substring(2, ta_Note.SelectedText.Length - 4); }
            else
            { ta_Note.SelectedText = string.Format("**{0}**", ta_Note.SelectedText); }
        }

        private void ToggleTextItalic()
        {
            if (!IsNoteContentActive())
            { return; }

            if (!CheckIfAddStyle(ta_Note.SelectedText, "*"))
            { ta_Note.SelectedText = ta_Note.SelectedText.Substring(1, ta_Note.SelectedText.Length - 2); }
            else
            { ta_Note.SelectedText = string.Format("*{0}*", ta_Note.SelectedText); }
        }

        private void ToggleTextHighlight()
        {
            if (!IsNoteContentActive())
            { return; }

            if (ta_Note.SelectedText.StartsWith("==") && ta_Note.SelectedText.EndsWith("=="))
            { ta_Note.SelectedText = ta_Note.SelectedText.Substring(2, ta_Note.SelectedText.Length - 4); }
            else
            { ta_Note.SelectedText = string.Format("=={0}==", ta_Note.SelectedText); }
        }

        private void ToggleTextStrikethrough()
        {
            if (!IsNoteContentActive())
            { return; }

            if (ta_Note.SelectedText.StartsWith("~~") && ta_Note.SelectedText.EndsWith("~~"))
            { ta_Note.SelectedText = ta_Note.SelectedText.Substring(2, ta_Note.SelectedText.Length - 4); }
            else
            { ta_Note.SelectedText = string.Format("~~{0}~~", ta_Note.SelectedText); }
        }

        private void ToggleTextSubscript()
        {
            if (!IsNoteContentActive())
            { return; }

            if (!CheckIfAddStyle(ta_Note.SelectedText, "~"))
            { ta_Note.SelectedText = ta_Note.SelectedText.Substring(1, ta_Note.SelectedText.Length - 2); }
            else
            { ta_Note.SelectedText = string.Format("~{0}~", ta_Note.SelectedText); }
        }

        private void ToggleTextSuperscript()
        {
            if (!IsNoteContentActive())
            { return; }

            if (ta_Note.SelectedText.StartsWith("^") && ta_Note.SelectedText.EndsWith("^"))
            { ta_Note.SelectedText = ta_Note.SelectedText.Substring(1, ta_Note.SelectedText.Length - 2); }
            else
            { ta_Note.SelectedText = string.Format("^{0}^", ta_Note.SelectedText); }
        }

        private bool IsNoteContentActive()
        {
            return ta_Note.IsFocused;
        }

        private bool CheckIfAddStyle(string textToCheck, string symbol)
        {
            bool addStyle = true;

            if (textToCheck.StartsWith(symbol) && textToCheck.EndsWith(symbol))
            { addStyle = false; }

            if (textToCheck.StartsWith(symbol + symbol))
            { addStyle = true; }

            if (textToCheck.StartsWith(symbol + symbol + symbol) && textToCheck.EndsWith(symbol + symbol + symbol))
            { addStyle = false; }

            return addStyle;
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

        private void ResetAutosave()
        {
            if (autosaveTask != null && !autosaveTask.IsCompleted)
            { 
                cancellationToken.Cancel();

                if (autosaveTask.IsCanceled)
                { autosaveTask.Dispose(); }
            }

            cancellationToken = new CancellationTokenSource();
            autosaveTask = AutoSaveTask();
        }

        private async Task AutoSaveTask()
        {
            if (System.Convert.ToBoolean(Logic.Refs.dataBank.GetSetting("autoSave")) && lv_NoteList.SelectedIndex >= 0)
            {
                using (cancellationToken)
                {
                    int autoSaveDelay = 5;
                    int.TryParse(Logic.Refs.dataBank.GetSetting("autoSaveDelay"), out autoSaveDelay);
                    await Task.Delay(autoSaveDelay * 30000);
                        
                    SaveNote();
                }
            }
        }

        private void SetModifiedState(bool isModified, string modifiedDate)
        {
            noteModified = isModified;


            btn_UndoMenu.IsEnabled = btn_UndoToolbar.IsEnabled = ta_Note.CanUndo;
            btn_RedoMenu.IsEnabled = btn_RedoToolbar.IsEnabled = ta_Note.CanRedo;

            WindowSetup();

            if (modifiedDate != null)
            { lbl_LastSaveTime.Content = modifiedDate; }
        }

        private void ToggleEditorMode()
        {
            mode++;

            if (mode > EditorMode.render)
            { mode = 0; }

            UpdateEditorView();
        }

        private void UpdateEditorView()
        { 
            grid_editorMode.ColumnDefinitions[0].Width = grid_editorMode.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
            
            switch (mode)
            {
                case EditorMode.editor:
                    ta_Note.Visibility = Visibility.Visible;
                    flowDocumentViewer.Visibility = Visibility.Collapsed;
                    grid_editorMode.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                    grid_editorMode.ColumnDefinitions[1].Width = new GridLength(0, GridUnitType.Auto);
                    break;

                case EditorMode.render:
                    ta_Note.Visibility = Visibility.Collapsed;
                    flowDocumentViewer.Visibility = Visibility.Visible;
                    grid_editorMode.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Auto);
                    grid_editorMode.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                    break;

                default:
                    ta_Note.Visibility = Visibility.Visible;
                    flowDocumentViewer.Visibility = Visibility.Visible;
                    grid_editorMode.ColumnDefinitions[0].Width = grid_editorMode.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                    break;
            }

        }
        #endregion Methods

        #region UI Events
        private void TANoteContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            SetModifiedState(true, null);
            UpdateMarkdownView();
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

        private void BtnTBold_Click(object sender, RoutedEventArgs e)
        {
            ToggleTextBold();
        }

        private void BtnItalics_Click(object sender, RoutedEventArgs e)
        {
            ToggleTextItalic();
        }

        private void BtnHighlight_Click(object sender, RoutedEventArgs e)
        {
            ToggleTextHighlight();
        }

        private void BtnStrike_Click(object sender, RoutedEventArgs e)
        {
            ToggleTextStrikethrough();
        }

        private void BtnSubscript_Click(object sender, RoutedEventArgs e)
        {
            ToggleTextSubscript();
        }

        private void BtnSuperscript_Click(object sender, RoutedEventArgs e)
        {
            ToggleTextSuperscript();
        }

        private void BtnViewMode_Click(object sender, RoutedEventArgs e)
        {
            ToggleEditorMode();
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

        // TODO add opening web links using browser
        // TODO add switching notes when using note links
        private void CommandBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            MessageBox.Show(System.String.Format("Link clicked: {0}", e.Parameter));
        }
        #endregion UI Events
    }
}
