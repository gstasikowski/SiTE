using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;

namespace SiTE.Views
{
	enum EditorMode { editor, mixed, render };

	public partial class EditorView : UserControl
	{
		private Task _autosaveTask;
		private CancellationTokenSource _cancellationTokenSource;

		private EditorMode _editorMode = EditorMode.editor;
		private bool _continueNoteSwitch = true;

		private int selectionStartPosition = -1;
		private int selectionEndPosition = -1;

		public EditorView()
		{
			InitializeComponent();
			Core.Instance.fileOperations.InitialSetup();

			WindowSetup();
			LoadNoteList();
			ResetAutosave();

			Core.Instance.dataBank.NoteSwitched += UpdateNoteListSelection;
			Core.Instance.dataBank.NoteSwitched += ResetEditorStatus;
		}

		#region Methods (window)
		private void WindowSetup()
		{
			// TODO remove hard references to MainWindow
			// ((IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime).MainWindow.Title = "SiTE";//App.ResourceAssembly.GetName().Name;

			// if (((ViewModels.EditorViewModel)this.DataContext).IsNoteModified)
			// {
			// 	((IClassicDesktopStyleApplicationLifetime)App.Current.ApplicationLifetime).MainWindow.Title += '*';
			// }

			int tempMode;
			int.TryParse(Core.Instance.dataBank.GetSetting("EditorMode"), out tempMode);
			_editorMode = (EditorMode)tempMode;

			UpdateEditorView();
		}

		private void ExitApp()
		{
			// Application.Current.MainWindow.Close();
		}
		#endregion Methods (window)

		#region Methods (view)
		private void UpdateNoteListSelection()
		{
			NoteList.SelectedIndex = Core.Instance.dataBank.GetNoteIndex(((ViewModels.EditorViewModel)this.DataContext).ActiveNote.ID);
		}

		private void ToggleNoteList(bool expand)
		{
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

			Core.Instance.dataBank.SetSetting("editorMode", ((int)_editorMode).ToString());
			Core.Instance.fileOperations.SaveSettings();
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
			Core.Instance.databaseOperations.GetNoteList();
		}

		private void NewNote()
		{
			NoteList.SelectedIndex = -1;
			((ViewModels.EditorViewModel)this.DataContext).NewNote();
			ResetEditorStatus();
		}

		private void SelectNoteOnList()
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
			if (((ViewModels.EditorViewModel)this.DataContext).ActiveNote.ID == noteID)
			{
				return;
			}

			await DisplaySaveReminder();

			if (!_continueNoteSwitch)
			{
				UpdateNoteListSelection();
				return;
			}

			((ViewModels.EditorViewModel)this.DataContext).OpenNote(noteID);
		}

		private void SaveNote()
		{
			if (AreChangesSaveWorthy())
			{
				((ViewModels.EditorViewModel)this.DataContext).SaveNote();

				LoadNoteList();
				SetModifiedState(false);

				if (NoteList.SelectedIndex < 0)
				{
					UpdateNoteListSelection();
				}
			}
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

		private void ToggleTextHorizontalLine()
		{
			NoteContent.Focus();
			NoteContent.Text = NoteContent.Text.Insert(selectionStartPosition, "***");
		}

		private void ToggleTextDecoration(string decorationMarker)
		{
			int markerLength = decorationMarker.Length;
			NoteContent.Focus();
			RestoreTextSelection();

			if (NoteContent.SelectedText.StartsWith(decorationMarker) && NoteContent.SelectedText.EndsWith(decorationMarker))
			{
				NoteContent.SelectedText = NoteContent.SelectedText[markerLength..^markerLength];
				UpdateTextSelectionPosition(-markerLength * 2);
			}
			else
			{
				NoteContent.SelectedText = string.Format("{0}{1}{0}", decorationMarker, NoteContent.SelectedText);
				UpdateTextSelectionPosition(markerLength * 2);
			}
		}
		#endregion Methods (text operations)

		#region Methods (helpers)
		private void CreateNoteLink()
		{
			if (NoteList.SelectedIndex < 0)
				return;

			var tempNote = (Models.NoteModel)NoteList.SelectedItem;
			string noteLink = string.Format("[{0}](nID:{1})", tempNote.Title, tempNote.ID);
			TopLevel.GetTopLevel(this).Clipboard.SetTextAsync(noteLink);
		}

		private void SetModifiedState(bool isModified)
		{
			((ViewModels.EditorViewModel)this.DataContext).IsNoteModified = isModified;

			btnSaveNote.IsEnabled = isModified;
			btnUndoMenu.IsEnabled = btnUndoToolbar.IsEnabled = NoteContent.CanUndo;
			btnRedoMenu.IsEnabled = btnRedoToolbar.IsEnabled = NoteContent.CanRedo;

			ToggleNoteModifiedDate();
			WindowSetup();
		}

		private void CacheTextSelectionPosition()
		{
			selectionStartPosition = NoteContent.SelectionStart;
			selectionEndPosition = NoteContent.SelectionEnd;
		}

		private void RestoreTextSelection()
		{
			NoteContent.SelectionStart = selectionStartPosition;
			NoteContent.SelectionEnd = selectionEndPosition;
		}

		private void UpdateTextSelectionPosition(int change)
		{
			selectionEndPosition += change;
		}

		private void ResetEditorStatus()
		{
			NoteContent.IsUndoEnabled = false;
			btnDeleteNote.IsEnabled = btnCreateLink.IsEnabled = (NoteList.SelectedIndex > -1);

			SetModifiedState(false);
			ResetAutosave();
		}
		#endregion Methods (helpers)

		#region Methods (saving)
		public async Task DisplaySaveReminder()
		{
			if (((ViewModels.EditorViewModel)this.DataContext).IsNoteModified)
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
			_cancellationTokenSource?.Cancel();
			_cancellationTokenSource?.Dispose();
			_cancellationTokenSource = new CancellationTokenSource();

			if (_autosaveTask != null && !_autosaveTask.IsCompleted)
			{
				if (_autosaveTask.IsCanceled)
				{
					_autosaveTask.Dispose();
				}
			}

			_autosaveTask = AutosaveTask();
		}

		private async Task AutosaveTask()
		{
			if (System.Convert.ToBoolean(SiTE.Core.Instance.dataBank.GetSetting("AutoSave")))
			{
				using (_cancellationTokenSource)
				{

					CancellationToken cancellationToken = _cancellationTokenSource.Token;

					int autoSaveDelay = 5;
					int.TryParse(SiTE.Core.Instance.dataBank.GetSetting("AutoSaveDelay"), out autoSaveDelay);
					await Task.Delay(System.TimeSpan.FromMinutes(autoSaveDelay));

					SaveNote();
				}
			}
		}

		private bool AreChangesSaveWorthy()
		{
			if (NoteList.SelectedIndex >= 0)
			{
				if (((ViewModels.EditorViewModel)this.DataContext).IsNoteModified)
				{
					return true;
				}
			}
			else
			{
				if (((ViewModels.EditorViewModel)this.DataContext).ActiveNote.Title != string.Empty
					|| ((ViewModels.EditorViewModel)this.DataContext).ActiveNote.Content != string.Empty)
				{
					return true;
				}
			}

			return false;
		}
		#endregion Methods (autosaving)

		#region UI Events
		private void TANoteContent_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (((TextBox)sender).IsFocused)
			{
				SetModifiedState(true);
			}
		}

		private void TextSelectionChanged(object sender, Avalonia.Input.PointerEventArgs e)
		{
			CacheTextSelectionPosition();
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
			ToggleTextDecoration("**");
		}

		private void BtnItalics_Click(object sender, RoutedEventArgs e)
		{
			ToggleTextDecoration("*");
		}

		private void BtnUnderline_Click(object sender, RoutedEventArgs e)
		{
			ToggleTextDecoration("__");
		}

		private void BtnStrike_Click(object sender, RoutedEventArgs e)
		{
			ToggleTextDecoration("~~");
		}

		private void BtnCode_Click(object sender, RoutedEventArgs e)
		{
			ToggleTextDecoration("```");
		}

		private void BtnHorizontalLine_Click(object sender, RoutedEventArgs e)
		{
			ToggleTextHorizontalLine();
		}

		private void BtnViewMode_Click(object sender, RoutedEventArgs e)
		{
			ToggleEditorMode();
		}

		private void MIExit_Click(object sender, RoutedEventArgs e)
		{
			ExitApp();
		}

		private void LVNoteList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SelectNoteOnList();
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
		#endregion UI Events
	}
}