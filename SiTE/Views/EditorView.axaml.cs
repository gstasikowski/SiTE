// using Markdig;
// using Neo.Markdig.Xaml;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;

namespace SiTE.Views
{
    enum EditorMode { editor, mixed, render };

    public partial class EditorView : UserControl
    {
        private Task autosaveTask;
        private CancellationTokenSource cancellationToken;

        private EditorMode editorMode = EditorMode.editor;
        private bool isNoteModified;
        private System.Guid selectedNote;

        public EditorView()
        {
            InitializeComponent();
            Logic.FileOperations.InitialSetup();

            WindowSetup();
            LoadNoteList();
            NewNote();
        }

        #region Methods (window)
        private void WindowSetup()
        {
            // TODO remove hard references to MainWindow
            ((IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime).MainWindow.Title = "SiTE";//App.ResourceAssembly.GetName().Name;
            lvwNoteList.ItemsSource = Logic.Refs.dataBank.NoteList;

            if (isNoteModified)
            {
                ((IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime).MainWindow.Title += '*';
            }

            int tempMode;
            int.TryParse(Logic.Refs.dataBank.GetSetting("editorMode"), out tempMode);
            editorMode = (EditorMode)tempMode;

            UpdateEditorView();
        }

        private void OpenAppInfo()
        {
            string message = string.Format("{0}\n\nVersion: {1}", "SiTE"/*App.ResourceAssembly.GetName().Name*/, "2.0"/*Avalonia.Application.ResourceAssembly.GetName().Version*/);

            ((IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime).MainWindow.IsEnabled = false;
            ErrorMessageView messageWindow = new ErrorMessageView(message, string.Empty);
            messageWindow.Title = (string)Logic.Localizer.Instance["MenuAbout"];
            messageWindow.Show();
        }

        private void OpenSettings()
        {
            // Logic.Refs.viewControl.CurrentPageViewModel = Logic.Refs.viewControl.PageViewModels[1]; // TODO switch to binding
        }

        private void ExitApp()
        {
            // Application.Current.MainWindow.Close();
        }
        #endregion Methods (window)

        #region Methods (view)
        private void UpdateNoteListSelection()
        {
            lvwNoteList.SelectedIndex = Logic.Refs.dataBank.GetNoteIndexFromTitle(txtTitle.Text);
            selectedNote = ((Models.NoteModel)lvwNoteList.SelectedItem).ID;
        }

        private void UpdateMarkdownView()
        {
            // var doc = MarkdownXaml.ToFlowDocument(txtNoteContent.Text,
            //     new MarkdownPipelineBuilder()
            //     .UseXamlSupportedExtensions()
            //     .Build()
            // );

            var doc = txtNoteContent.Text; // temp placeholder until markdown renderer replacement is implemented
            markdownContent.Text = doc;
        }

        private void ToggleNoteList(bool expand)
        {
            // if (!gclNoteList.IsLoaded || !gclSplitter.IsLoaded)
            // {
            //     return;
            // }

            // gclSplitter.IsEnabled = expand;
            // gclNoteList.MinWidth = expand ? 130 : 30;
            // gclNoteList.Width = GridLength.Auto;
            lvwNoteList.IsVisible = expand;
        }

        private void ToggleEditorMode()
        {
            editorMode++;

            if (editorMode > EditorMode.render)
            {
                editorMode = 0;
            }

            UpdateEditorView();

            Logic.Refs.dataBank.SetSetting("editorMode", ((int)editorMode).ToString());
            Logic.FileOperations.SaveSettings();
        }

        private void UpdateEditorView()
        {
            grdEditorMode.ColumnDefinitions[0].Width = grdEditorMode.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);

            switch (editorMode)
            {
                case EditorMode.editor:
                    txtNoteContent.IsVisible = true;
                    markdownContent.IsVisible = false;
                    grdEditorMode.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                    grdEditorMode.ColumnDefinitions[1].Width = new GridLength(0, GridUnitType.Auto);
                    break;

                case EditorMode.render:
                    txtNoteContent.IsVisible = false;
                    markdownContent.IsVisible = true;
                    grdEditorMode.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Auto);
                    grdEditorMode.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                    break;

                default:
                    txtNoteContent.IsVisible = true;
                    markdownContent.IsVisible = true;
                    grdEditorMode.ColumnDefinitions[0].Width = grdEditorMode.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                    break;
            }

        }
        #endregion Methods (view)

        #region Methods (note operations)
        private void LoadNoteList()
        {
            Logic.DatabaseOperations.GetNoteList();
        }

        private void NewNote()
        {
            lvwNoteList.SelectedIndex = -1;
            selectedNote = System.Guid.Empty;
            txtTitle.Text = "";
            txtNoteContent.Text = string.Empty;
            btnDeleteNote.IsEnabled = btnCreateLink.IsEnabled = false;
            SetModifiedState(false, string.Empty);
        }

        private void SelectNote()
        {
            if (lvwNoteList.SelectedIndex < 0)
            {
                return;
            }

            var tempItem = (Models.NoteModel)lvwNoteList.SelectedItem;
            OpenNote(tempItem.ID);
        }

        private void OpenNote(System.Guid noteID)
        {
            if (selectedNote == noteID)
            {
                return;
            }

            if (!AreUnsavedChangesHandled())
            {
                lvwNoteList.SelectedIndex = Logic.Refs.dataBank.GetNoteIndex(selectedNote);
                return;
            }

            txtNoteContent.IsUndoEnabled = false; // TODO figure out a better way to do this

            var tempNote = Logic.DatabaseOperations.LoadNote(noteID);
            
            txtTitle.Text = tempNote.Title;
            txtNoteContent.Text = tempNote.Content;

            SetModifiedState(false, tempNote.Modified.ToString());
            txtNoteContent.IsUndoEnabled = true; // TODO figure out a better way to do this
            btnDeleteNote.IsEnabled = btnCreateLink.IsEnabled = true;

            ResetAutosave();
            UpdateNoteListSelection();
        }

        private void SaveNote()
        {
            Logic.DatabaseOperations.SaveNote(selectedNote, txtTitle.Text, txtNoteContent.Text);

            LoadNoteList();
            SetModifiedState(false, System.DateTime.Now.ToString());

            if (lvwNoteList.SelectedIndex < 0)
            {
                UpdateNoteListSelection();
            }

            ResetAutosave();
        }

        private void DeleteNote()
        {
            if (lvwNoteList.SelectedIndex < 0)
            {
                return;
            }

            var tempItem = (Models.NoteModel)lvwNoteList.SelectedItem;
            Logic.DatabaseOperations.DeleteNote(tempItem.ID);
            NewNote();
            LoadNoteList();
        }

        private void OpenLink(string url)
        {
            if (url.StartsWith("nid:"))
            {
                url = url.Replace("nid:", string.Empty);
                System.Guid tempID = System.Guid.Parse(url);
                OpenNote(tempID);
            }
            else
            {
                url = url.Replace("&", "^&");
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
            }
        }
        #endregion (note operations)

        #region Methods (text operations)
        private void CutText()
        {
            txtNoteContent.Cut();
        }

        private void CopyText()
        {
            txtNoteContent.Copy();
        }

        private void PasteText()
        {
            txtNoteContent.Paste();
        }

        private void UndoChanges()
        {
            txtNoteContent.Undo();
        }

        private void RedoChanges()
        {
            txtNoteContent.Redo();
        }

        private void ToggleTextBold()
        {
            if (!IsNoteContentActive())
            {
                return;
            }

            if (txtNoteContent.SelectedText.StartsWith("**") && txtNoteContent.SelectedText.EndsWith("**"))
            {
                txtNoteContent.SelectedText = txtNoteContent.SelectedText.Substring(2, txtNoteContent.SelectedText.Length - 4);
            }
            else
            {
                txtNoteContent.SelectedText = string.Format("**{0}**", txtNoteContent.SelectedText);
            }
        }

        private void ToggleTextItalic()
        {
            if (!IsNoteContentActive())
            {
                return;
            }

            if (!CheckIfAddStyle(txtNoteContent.SelectedText, "*"))
            {
                txtNoteContent.SelectedText = txtNoteContent.SelectedText.Substring(1, txtNoteContent.SelectedText.Length - 2);
            }
            else
            {
                txtNoteContent.SelectedText = string.Format("*{0}*", txtNoteContent.SelectedText);
            }
        }

        private void ToggleTextHighlight()
        {
            if (!IsNoteContentActive())
            {
                return;
            }

            if (txtNoteContent.SelectedText.StartsWith("==") && txtNoteContent.SelectedText.EndsWith("=="))
            {
                txtNoteContent.SelectedText = txtNoteContent.SelectedText.Substring(2, txtNoteContent.SelectedText.Length - 4);
            }
            else
            {
                txtNoteContent.SelectedText = string.Format("=={0}==", txtNoteContent.SelectedText);
            }
        }

        private void ToggleTextStrikethrough()
        {
            if (!IsNoteContentActive())
            {
                return;
            }

            if (txtNoteContent.SelectedText.StartsWith("~~") && txtNoteContent.SelectedText.EndsWith("~~"))
            {
                txtNoteContent.SelectedText = txtNoteContent.SelectedText.Substring(2, txtNoteContent.SelectedText.Length - 4);
            }
            else
            {
                txtNoteContent.SelectedText = string.Format("~~{0}~~", txtNoteContent.SelectedText);
            }
        }

        private void ToggleTextSubscript()
        {
            if (!IsNoteContentActive())
            {
                return;
            }

            if (!CheckIfAddStyle(txtNoteContent.SelectedText, "~"))
            {
                txtNoteContent.SelectedText = txtNoteContent.SelectedText.Substring(1, txtNoteContent.SelectedText.Length - 2);
            }
            else
            {
                txtNoteContent.SelectedText = string.Format("~{0}~", txtNoteContent.SelectedText);
            }
        }

        private void ToggleTextSuperscript()
        {
            if (!IsNoteContentActive())
            {
                return;
            }

            if (txtNoteContent.SelectedText.StartsWith("^") && txtNoteContent.SelectedText.EndsWith("^"))
            {
                txtNoteContent.SelectedText = txtNoteContent.SelectedText.Substring(1, txtNoteContent.SelectedText.Length - 2);
            }
            else
            {
                txtNoteContent.SelectedText = string.Format("^{0}^", txtNoteContent.SelectedText);
            }
        }
        #endregion Methods (text operations)

        #region Methods (helpers)
        private bool IsNoteContentActive()
        {
            return txtNoteContent.IsFocused;
        }

        private bool CheckIfAddStyle(string textToCheck, string symbol)
        {
            bool addStyle = true;

            if (textToCheck.StartsWith(symbol) && textToCheck.EndsWith(symbol))
            {
                addStyle = false;
            }

            if (textToCheck.StartsWith(symbol + symbol))
            {
                addStyle = true;
            }

            if (textToCheck.StartsWith(symbol + symbol + symbol) && textToCheck.EndsWith(symbol + symbol + symbol))
            {
                addStyle = false;
            }

            return addStyle;
        }

        private void CreateNoteLink()
        {
            if (lvwNoteList.SelectedIndex < 0)
                return;

            var tempNote = (Models.NoteModel)lvwNoteList.SelectedItem;
            string noteLink = string.Format("[{0}](nID:{1})", tempNote.Title, tempNote.ID);
            
            // Avalonia.Controls.TopLevel window
            TopLevel.GetTopLevel(this).Clipboard.SetTextAsync(noteLink);
        }

        private void SetModifiedState(bool isModified, string modifiedDate)
        {
            isNoteModified = isModified;

            btnSaveNote.IsEnabled = isModified;
            btnUndoMenu.IsEnabled = btnUndoToolbar.IsEnabled = txtNoteContent.CanUndo;
            btnRedoMenu.IsEnabled = btnRedoToolbar.IsEnabled = txtNoteContent.CanRedo;

            WindowSetup();

            if (modifiedDate != null)
            {
                lblLastSaveTime.Content = modifiedDate;
            }
        }
        #endregion Methods (helpers)

        #region Methods (saving)
        public bool AreUnsavedChangesHandled()
        {
            if (isNoteModified)
            {
                var saveWindow = new SaveReminderView();
                saveWindow.ShowDialog(((IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime).MainWindow);

                switch (saveWindow.dialogChoice)
                {
                    case 0:
                        SaveNote();
                        return true;

                    case 1:
                        return true;

                    default:
                        return false;
                }
            }

            return true;
        }

        private void ResetAutosave()
        {
            if (autosaveTask != null && !autosaveTask.IsCompleted)
            { 
                cancellationToken.Cancel();

                if (autosaveTask.IsCanceled)
                {
                    autosaveTask.Dispose();
                }
            }

            cancellationToken = new CancellationTokenSource();
            autosaveTask = AutoSaveTask();
        }

        private async Task AutoSaveTask()
        {
            if (System.Convert.ToBoolean(Logic.Refs.dataBank.GetSetting("autoSave")) && lvwNoteList.SelectedIndex >= 0)
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
        #endregion Methods (autosaving)
        
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

        private void MIGit_Click(object sender, RoutedEventArgs e)
        {
            OpenLink(Logic.Refs.dataBank.projectUrl);
        }

        private void MIAbout_Click(object sender, RoutedEventArgs e)
        {
            OpenAppInfo();
        }

        private void LVNoteList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectNote();
        }

        private void EXNoteList_Collapsed(object sender, RoutedEventArgs e)
        {
            ToggleNoteList(false);
        }

        private void EXNoteList_Expanded(object sender, RoutedEventArgs e)
        {
            ToggleNoteList(true);
        }

        private void BtnCreateLink_Click(object sender, RoutedEventArgs e)
        {
            CreateNoteLink();
        }

        // private void CommandBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        // {
        //     OpenLink(e.Parameter.ToString());
        // }
        #endregion UI Events
    }
}