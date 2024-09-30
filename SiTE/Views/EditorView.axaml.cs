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
        private Task _autosaveTask;
        private CancellationTokenSource _cancellationToken;

        private EditorMode _editorMode = EditorMode.editor;
        private bool _isNoteModified;
        private bool _continueNoteSwitch = true;
        private System.Guid _selectedNote;

        public EditorView()
        {
            InitializeComponent();
            Logic.FileOperations.InitialSetup();

            WindowSetup();
            LoadNoteList();
        }

        #region Methods (window)
        private void WindowSetup()
        {
            // TODO remove hard references to MainWindow
            ((IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime).MainWindow.Title = "SiTE";//App.ResourceAssembly.GetName().Name;

            if (_isNoteModified)
            {
                ((IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime).MainWindow.Title += '*';
            }

            int tempMode;
            int.TryParse(Models.DataBank.Instance.GetSetting("editorMode"), out tempMode);
            _editorMode = (EditorMode)tempMode;

            UpdateEditorView();
        }

        private void OpenAppInfo()
        {
            string message = string.Format("{0}\n\nVersion: {1}", "SiTE"/*App.ResourceAssembly.GetName().Name*/, "2.0"/*Avalonia.Application.ResourceAssembly.GetName().Version*/);

            ((IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime).MainWindow.IsEnabled = false;
            ErrorMessageView messageWindow = new ErrorMessageView(message, string.Empty);
            messageWindow.Title = (string)Logic.Localizer.Instance["MenuAbout"];
            messageWindow.ShowDialog(((IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime).MainWindow);
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
            NoteList.SelectedIndex = Models.DataBank.Instance.GetNoteIndex(((ViewModels.EditorViewModel)this.DataContext).ActiveNote.ID);
            _selectedNote = (NoteList.SelectedIndex > -1) ? ((ViewModels.EditorViewModel)this.DataContext).ActiveNote.ID : System.Guid.Empty;
        }

        private void UpdateMarkdownView()
        {
            // var doc = MarkdownXaml.ToFlowDocument(txtNoteContent.Text,
            //     new MarkdownPipelineBuilder()
            //     .UseXamlSupportedExtensions()
            //     .Build()
            // );

            var doc = NoteContent.Text; // temp placeholder until markdown renderer replacement is implemented
            NoteMarkdownDisplay.Text = doc;
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
            NoteList.IsVisible = expand;
        }

        private void ToggleEditorMode()
        {
            _editorMode++;

            if (_editorMode > EditorMode.render)
            {
                _editorMode = 0;
            }

            UpdateEditorView();

            Models.DataBank.Instance.SetSetting("_editorMode", ((int)_editorMode).ToString());
            Logic.FileOperations.SaveSettings();
        }

        private void UpdateEditorView()
        {
            EditorGrid.ColumnDefinitions[0].Width = EditorGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);

            switch (_editorMode)
            {
                case EditorMode.editor:
                    NoteContent.IsVisible = true;
                    NoteMarkdownDisplay.IsVisible = false;
                    EditorGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                    EditorGrid.ColumnDefinitions[1].Width = new GridLength(0, GridUnitType.Auto);
                    break;

                case EditorMode.render:
                    NoteContent.IsVisible = false;
                    NoteMarkdownDisplay.IsVisible = true;
                    EditorGrid.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Auto);
                    EditorGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                    break;

                default:
                    NoteContent.IsVisible = true;
                    NoteMarkdownDisplay.IsVisible = true;
                    EditorGrid.ColumnDefinitions[0].Width = EditorGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                    break;
            }
        }

        private void ToggleNoteModifiedDate()
        {
            LastSaveTime.IsVisible = (NoteList.SelectedIndex >= 0);
        }
        #endregion Methods (view)

        #region Methods (note operations)
        private void LoadNoteList()
        {
            Logic.DatabaseOperations.GetNoteList();
        }

        private void NewNote()
        {
            NoteList.SelectedIndex = -1;
            ((ViewModels.EditorViewModel)this.DataContext).NewNote();

            btnDeleteNote.IsEnabled = btnCreateLink.IsEnabled = false;
            SetModifiedState(false);
            ToggleNoteModifiedDate();
        }

        private void SelectNote()
        {
            if (NoteList.SelectedIndex < 0)
            {
                return;
            }

            var tempItem = (Models.NoteModel)NoteList.SelectedItem;
            OpenNote(tempItem.ID);
        }

        private async Task OpenNote(System.Guid noteID)
        {
            if (_selectedNote == noteID)
            {
                return;
            }

            await DisplaySaveReminder();

            if (!_continueNoteSwitch)
            {
                NoteList.SelectedIndex = Models.DataBank.Instance.GetNoteIndex(_selectedNote);
                return;
            }

            NoteContent.IsUndoEnabled = false; // TODO figure out a better way to do this

            ((ViewModels.EditorViewModel)this.DataContext).OpenNote(noteID);

            NoteContent.IsUndoEnabled = true; // TODO figure out a better way to do this
            btnDeleteNote.IsEnabled = btnCreateLink.IsEnabled = true;

            ToggleNoteModifiedDate();
            SetModifiedState(false);
            ResetAutosave();
        }

        private void SaveNote()
        {
            ((ViewModels.EditorViewModel)this.DataContext).SaveNote();

            LoadNoteList();
            SetModifiedState(false);

            if (NoteList.SelectedIndex < 0)
            {
                UpdateNoteListSelection();
            }

            ToggleNoteModifiedDate();
            ResetAutosave();
        }

        private void DeleteNote()
        {
            if (NoteList.SelectedIndex < 0)
            {
                return;
            }
            
            ((ViewModels.EditorViewModel)this.DataContext).DeleteNote();
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
            NoteContent.Cut();
        }

        private void CopyText()
        {
            NoteContent.Copy();
        }

        private void PasteText()
        {
            NoteContent.Paste();
        }

        private void UndoChanges()
        {
            NoteContent.Undo();
        }

        private void RedoChanges()
        {
            NoteContent.Redo();
        }

        private void ToggleTextBold()
        {
            if (!IsNoteContentActive())
            {
                return;
            }

            if (NoteContent.SelectedText.StartsWith("**") && NoteContent.SelectedText.EndsWith("**"))
            {
                NoteContent.SelectedText = NoteContent.SelectedText.Substring(2, NoteContent.SelectedText.Length - 4);
            }
            else
            {
                NoteContent.SelectedText = string.Format("**{0}**", NoteContent.SelectedText);
            }
        }

        private void ToggleTextItalic()
        {
            if (!IsNoteContentActive())
            {
                return;
            }

            if (!CheckIfAddStyle(NoteContent.SelectedText, "*"))
            {
                NoteContent.SelectedText = NoteContent.SelectedText.Substring(1, NoteContent.SelectedText.Length - 2);
            }
            else
            {
                NoteContent.SelectedText = string.Format("*{0}*", NoteContent.SelectedText);
            }
        }

        private void ToggleTextHighlight()
        {
            if (!IsNoteContentActive())
            {
                return;
            }

            if (NoteContent.SelectedText.StartsWith("==") && NoteContent.SelectedText.EndsWith("=="))
            {
                NoteContent.SelectedText = NoteContent.SelectedText.Substring(2, NoteContent.SelectedText.Length - 4);
            }
            else
            {
                NoteContent.SelectedText = string.Format("=={0}==", NoteContent.SelectedText);
            }
        }

        private void ToggleTextStrikethrough()
        {
            if (!IsNoteContentActive())
            {
                return;
            }

            if (NoteContent.SelectedText.StartsWith("~~") && NoteContent.SelectedText.EndsWith("~~"))
            {
                NoteContent.SelectedText = NoteContent.SelectedText.Substring(2, NoteContent.SelectedText.Length - 4);
            }
            else
            {
                NoteContent.SelectedText = string.Format("~~{0}~~", NoteContent.SelectedText);
            }
        }

        private void ToggleTextSubscript()
        {
            if (!IsNoteContentActive())
            {
                return;
            }

            if (!CheckIfAddStyle(NoteContent.SelectedText, "~"))
            {
                NoteContent.SelectedText = NoteContent.SelectedText.Substring(1, NoteContent.SelectedText.Length - 2);
            }
            else
            {
                NoteContent.SelectedText = string.Format("~{0}~", NoteContent.SelectedText);
            }
        }

        private void ToggleTextSuperscript()
        {
            if (!IsNoteContentActive())
            {
                return;
            }

            if (NoteContent.SelectedText.StartsWith("^") && NoteContent.SelectedText.EndsWith("^"))
            {
                NoteContent.SelectedText = NoteContent.SelectedText.Substring(1, NoteContent.SelectedText.Length - 2);
            }
            else
            {
                NoteContent.SelectedText = string.Format("^{0}^", NoteContent.SelectedText);
            }
        }
        #endregion Methods (text operations)

        #region Methods (helpers)
        private bool IsNoteContentActive()
        {
            return NoteContent.IsFocused;
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
            if (NoteList.SelectedIndex < 0)
                return;

            var tempNote = (Models.NoteModel)NoteList.SelectedItem;
            string noteLink = string.Format("[{0}](nID:{1})", tempNote.Title, tempNote.ID);
            
            // Avalonia.Controls.TopLevel window
            TopLevel.GetTopLevel(this).Clipboard.SetTextAsync(noteLink);
        }

        private void SetModifiedState(bool isModified)
        {
            _isNoteModified = isModified;

            btnSaveNote.IsEnabled = isModified;
            btnUndoMenu.IsEnabled = btnUndoToolbar.IsEnabled = NoteContent.CanUndo;
            btnRedoMenu.IsEnabled = btnRedoToolbar.IsEnabled = NoteContent.CanRedo;

            WindowSetup();
        }
        #endregion Methods (helpers)

        #region Methods (saving)
        public async Task DisplaySaveReminder()
        {
            if (_isNoteModified)
            {
                var saveWindow = new SaveReminderView();
                int saveChoice = await saveWindow.ShowDialog<int>(((IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime).MainWindow);

                switch (saveChoice)
                {
                    case 0:
                        SaveNote();
                        break;

                    case 1:
                        break;

                    default:
                        _continueNoteSwitch = false;
                        return;
                }
            }

            _continueNoteSwitch = true;
        }

        private void ResetAutosave()
        {
            if (_autosaveTask != null && !_autosaveTask.IsCompleted)
            { 
                _cancellationToken.Cancel();

                if (_autosaveTask.IsCanceled)
                {
                    _autosaveTask.Dispose();
                }
            }

            _cancellationToken = new CancellationTokenSource();
            _autosaveTask = AutosaveTask();
        }

        private async Task AutosaveTask()
        {
            if (System.Convert.ToBoolean(Models.DataBank.Instance.GetSetting("autoSave")) && NoteList.SelectedIndex >= 0)
            {
                using (_cancellationToken)
                {
                    int autoSaveDelay = 5;
                    int.TryParse(Models.DataBank.Instance.GetSetting("autoSaveDelay"), out autoSaveDelay);
                    await Task.Delay(autoSaveDelay * 30000);
                        
                    SaveNote();
                }
            }
        }
        #endregion Methods (autosaving)
        
        #region UI Events
        private void TANoteContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).IsFocused)
            {
                SetModifiedState(true);
            }
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
            OpenLink(Models.DataBank.Instance.projectUrl);
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