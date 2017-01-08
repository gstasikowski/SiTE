using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EdytorTekstu
{
    public partial class EdytorTekstu : Form
    {
        private string filePath = "Dokument";

        private FontFamily[] fonts;
        private System.Drawing.FontStyle newFontStyle;

        private Localization lang = new Localization();

        public EdytorTekstu()
        {
            InitializeComponent();
            fontSizeList.SelectedIndex = 4;
            AddAvailableFonts();
            ChangeLang(System.Globalization.CultureInfo.CurrentCulture.DisplayName);
        }

        private void CheckKeyShortcuts(object sender, KeyEventArgs e)
        {
            if (Form.ModifierKeys.Equals(Keys.Control))
            {
                
                switch (e.KeyCode)
                {
                    case Keys.B:
                        ToggleBold();
                        break;

                    case Keys.J: //TO FIX: Ctrl+I acts as TAB
                        ToggleItalic();
                        break;

                    case Keys.U:
                        ToggleUnderline();
                        break;

                    case Keys.T:
                        ToggleStrikeout();
                        break;

                    default:
                        break;
                }
            }
        }

        //*********************************************//
        private void NewFile()
        {
            filePath = "Dokument";
            textArea.Text = "";
        }

        private void OpenFile()
        {
            Stream fileContent = null;
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = lang.GetLine("formats");
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = false;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if ((fileContent = openFileDialog.OpenFile()) != null)
                {
                    NewFile();
                    filePath = openFileDialog.FileName;

                    using (StreamReader content = new StreamReader(fileContent))
                    {
                        if (filePath.EndsWith(".rtf"))
                            textArea.LoadFile(filePath, RichTextBoxStreamType.RichText);
                        else
                            textArea.LoadFile(filePath, RichTextBoxStreamType.PlainText);
                    }
               }
            }
        }

        private bool SaveReminder()
        {
            if (Form.ActiveForm.Text.Contains("*"))
            {
                DialogResult result = MessageBox.Show(lang.GetLine("dlg_save"), lang.appName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                switch (result)
                {
                    case DialogResult.Yes:
                        SaveFile();
                        return true;

                    case DialogResult.No:
                        return true;

                    case DialogResult.Cancel:
                        return false;

                    default:
                        return true;
                }
            }
            else
                return true;
        }

        private void SaveFile()
        {
            if (filePath != "Dokument")
            {
                try
                {
                    using (StreamWriter content = new StreamWriter(filePath))
                        content.WriteLine(textArea.Rtf);
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show(lang.GetLine("dlg_cantSave"), lang.appName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                Form.ActiveForm.Text = filePath + " - " + lang.appName;
            }
            else
                SaveFileAs();
        }

        private void SaveFileAs()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = lang.GetLine("formats");
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = false;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (textArea.Text != null)
                {
                    using (StreamWriter content = new StreamWriter(saveFileDialog.FileName))
                        content.WriteLine(textArea.Rtf);
                }

                filePath = saveFileDialog.FileName;
                Form.ActiveForm.Text = filePath + " - " + lang.appName;
            }
        }

        private void CheckModification()
        {
            undoBtn.Enabled = undoTBtn.Enabled = undoTSBtn.Enabled = textArea.CanUndo;
            redoBtn.Enabled = redoTBtn.Enabled = redoTSBtn.Enabled = textArea.CanRedo;

            if (textArea.CanUndo)
                Form.ActiveForm.Text = "*" + filePath + " - " + lang.appName;
            else
                Form.ActiveForm.Text = filePath + " - " + lang.appName;

            lineLabel.Text = lang.GetLine("i_lines") + ": " + textArea.Lines.Count();
        }

        private void AddAvailableFonts()
        {
            InstalledFontCollection installedFonts = new InstalledFontCollection();
            fonts = installedFonts.Families;
            
            for (int i = 0; i < fonts.Length; i++)
                fontList.Items.Add(fonts[i].Name);

            fontList.SelectedIndex = fontList.FindString("Arial");

            if (fontList.SelectedIndex < 0)
                fontList.SelectedIndex = 0;
        }

        private void SetFont()
        {
            if (textArea.SelectionFont != null)
                textArea.SelectionFont = new Font(fonts[fontList.SelectedIndex], textArea.SelectionFont.Size, textArea.SelectionFont.Style);
        }

        private void SetFontSize()
        {
            if (textArea.SelectionFont != null)
            {
                int newSize = (int)textArea.SelectionFont.Size;
                
                try
	            {	        
		            newSize = Convert.ToInt32(fontSizeList.Text);
	            }
	            catch (Exception)
	            {
		            //throw;
	            }

                textArea.SelectionFont = new Font(textArea.SelectionFont.FontFamily, newSize, textArea.SelectionFont.Style);
            }
        }

        private void ToggleBold()
        {
            SetFontStyle(0);
        }

        private void ToggleItalic()
        {
            SetFontStyle(1);
        }

        private void ToggleUnderline()
        {
            SetFontStyle(2);
        }

        private void ToggleStrikeout()
        {
            SetFontStyle(3);
        }
        
        private void SetFontStyle(int style)
        {
            if (textArea.SelectionFont != null)
            {
                newFontStyle = FontStyle.Regular;
                bool fontBold = textArea.SelectionFont.Bold;
                bool fontItalic = textArea.SelectionFont.Italic;
                bool fontUnderline = textArea.SelectionFont.Underline;
                bool fontStrikeout = textArea.SelectionFont.Strikeout;

                switch (style)
                {
                    case 0:
                        fontBold = !fontBold;
                        break;

                    case 1:
                        fontItalic = !fontItalic;
                        break;

                    case 2:
                        fontUnderline = !fontUnderline;
                        break;

                    case 3:
                        fontStrikeout = !fontStrikeout;
                        break;
                }

                if (fontBold)
                    newFontStyle = newFontStyle | FontStyle.Bold;

                if (fontItalic)
                    newFontStyle = newFontStyle | FontStyle.Italic;

                if (fontUnderline)
                    newFontStyle = newFontStyle | FontStyle.Underline;

                if (fontStrikeout)
                    newFontStyle = newFontStyle | FontStyle.Strikeout;

                textArea.SelectionFont = new Font(textArea.SelectionFont.FontFamily, textArea.SelectionFont.Size, newFontStyle);
            }
        }

        private void TextPosition(string position)
        {
            switch (position)
            {
                case "left":
                    textArea.SelectionAlignment = HorizontalAlignment.Left;
                    break;

                case "center":
                    textArea.SelectionAlignment = HorizontalAlignment.Center;
                    break;

                case "right":
                    textArea.SelectionAlignment = HorizontalAlignment.Right;
                    break;
            }
        }

        private void UndoChanges()
        {
            textArea.Undo();
        }

        private void RedoChanges()
        {
            textArea.Redo();
        }

        private void CutText()
        {
            if (textArea.SelectedText != "")
                textArea.Cut();
        }

        private void CopyText()
        {
            if (textArea.SelectedText != "")
                textArea.Copy();
        }

        private void PasteText()
        {
            textArea.Paste();
        }

        private void DeleteText()
        {
            textArea.SelectedText = "";
        }

        private void SetFontFromDialog()
        {
            FontDialog fontDialog = new FontDialog();

            if (fontDialog.ShowDialog() == DialogResult.OK)
                textArea.SelectionFont = fontDialog.Font;
        }

        private void SetColor()
        {
            ColorDialog colorDialog = new ColorDialog();

            if (colorDialog.ShowDialog() == DialogResult.OK)
                textArea.SelectionColor = colorDialog.Color;
        }

        private void SetBackColor()
        {
            ColorDialog colorDialog = new ColorDialog();

            if (colorDialog.ShowDialog() == DialogResult.OK)
                textArea.SelectionBackColor = colorDialog.Color;
        }

        private void ChangeLang(string newLang)
        {
            for (int i = 0; i < lang.languages.Length; i++)
            {
                if (newLang.Contains(lang.languages[i]))
                {
                    lang.ChangeLanguage(i);
                    ApplyLocalization();
                    break;
                }
            }
        }

        private void ApplyLocalization()
        {
            lineLabel.Text = lang.GetLine("i_lines") + ": " + textArea.Lines.Count();

            //Categories
            tmFile.Text = lang.GetLine("tm_file");
            tmEdit.Text = lang.GetLine("tm_edit");
            tmFormat.Text = lang.GetLine("tm_format");
            tmPrefs.Text = lang.GetLine("tm_prefs");
            tmHelp.Text = lang.GetLine("tm_help");
            //File menu
            newFileBtn.Text = lang.GetLine("f_new");
            openFileBtn.Text = lang.GetLine("f_open");
            saveFileBtn.Text = lang.GetLine("f_save");
            saveAsBtn.Text = lang.GetLine("f_saveAs");
            pageSetupBtn.Text = lang.GetLine("f_page");
            quitBtn.Text = lang.GetLine("f_quit");
            //Edit menu
            undoBtn.Text = lang.GetLine("e_undo");
            redoBtn.Text = lang.GetLine("e_redo");
            cutBtn.Text = lang.GetLine("e_cut");
            copyBtn.Text = lang.GetLine("e_copy");
            pasteBtn.Text = lang.GetLine("e_paste");
            deleteBtn.Text = lang.GetLine("e_delete");
            selectAllBtn.Text = lang.GetLine("e_selectAll");
            //Format menu
            fontBtn.Text = lang.GetLine("fr_font");
            fontColorBtn.Text = lang.GetLine("fr_fColor");
            bgColorBtn.Text = lang.GetLine("fr_hColor");
            wordWrapBtn.Text = lang.GetLine("fr_wordWrap");
            //Preferences
            lngBtn.Text = lang.GetLine("pr_lang");
            //Languages
            lngENBtn.Text = lang.languages[0];
            lngPLBtn.Text = lang.languages[1];
            //Help
            helpBtn.Text = lang.GetLine("h_about");
            //Toolbox
            newFileTBtn.ToolTipText = lang.GetLine("tt_new");
            openFileTBtn.ToolTipText = lang.GetLine("tt_open");
            saveFileTBtn.ToolTipText = lang.GetLine("tt_save");
            cutTBtn.ToolTipText = lang.GetLine("tt_cut");
            copyTBtn.ToolTipText = lang.GetLine("tt_copy");
            pasteTBtn.ToolTipText = lang.GetLine("tt_paste");
            undoTBtn.ToolTipText = lang.GetLine("tt_undo");
            redoTBtn.ToolTipText = lang.GetLine("tt_redo");
            boldTBtn.ToolTipText = lang.GetLine("tt_bold");
            italicTBtn.ToolTipText = lang.GetLine("tt_italics");
            underlineTBtn.ToolTipText = lang.GetLine("tt_underline");
            strikeTBtn.ToolTipText = lang.GetLine("tt_strikeout");
            fontColorTBtn.ToolTipText = lang.GetLine("tt_fColor");
            bgColorTBtn.ToolTipText = lang.GetLine("tt_hColor");
            textLeftTBtn.ToolTipText = lang.GetLine("tt_algLeft");
            textCenterTBtn.ToolTipText = lang.GetLine("tt_algCenter");
            textRightTBtn.ToolTipText = lang.GetLine("tt_algRight");
            wordWrapTBtn.ToolTipText = lang.GetLine("tt_wordWrap");
            //.Text = lang.GetLine("");
        }

        //*********************************************//
        private void NewFile_Click(object sender, EventArgs e)
        {
            if (SaveReminder())
            {
                NewFile();
                CheckModification();
            }
        }

        private void OpenFile_Click(object sender, EventArgs e)
        {
            if (SaveReminder())
            {
                OpenFile();
                CheckModification();
            }
        }

        private void SaveFile_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void SaveFileAs_Click(object sender, EventArgs e)
        {
            SaveFileAs();
        }

        private void Quit_Click(object sender, EventArgs e)
        {
            if (SaveReminder())
                Application.Exit();
        }

        private void Close_Click(object sender, FormClosingEventArgs e)
        {
            if (!SaveReminder())
                e.Cancel = true;
        }

        private void ToggleControls(object sender, EventArgs e)
        {
            cutBtn.Enabled = copyBtn.Enabled = deleteBtn.Enabled = (textArea.SelectedText != "");
            pasteBtn.Enabled = Clipboard.ContainsText();
        }

        private void Undo_Click(object sender, EventArgs e)
        {
            UndoChanges();
        }

        private void Redo_Click(object sender, EventArgs e)
        {
            RedoChanges();
        }

        private void Cut_Click(object sender, EventArgs e)
        {
            CutText();
        }

        private void Copy_Click(object sender, EventArgs e)
        {
            CopyText();
        }

        private void Paste_Click(object sender, EventArgs e)
        {
            PasteText();
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            DeleteText();
        }

        private void SelectAll_Click(object sender, EventArgs e)
        {
            textArea.SelectAll();
        }

        private void Font_Click(object sender, EventArgs e)
        {
            SetFontFromDialog();
        }

        private void Color_Click(object sender, EventArgs e)
        {
            SetColor();
        }

        private void BackColor_Click(object sender, EventArgs e)
        {
            SetBackColor();
        }

        private void WordWrap_Click(object sender, EventArgs e)
        {
            textArea.WordWrap = !textArea.WordWrap;
            wordWrapBtn.Checked = wordWrapTBtn.Checked = textArea.WordWrap;
        }

        private void Help_Click(object sender, EventArgs e)
        {
            MessageBox.Show(lang.appName + " v." + Application.ProductVersion + lang.GetLine("dlg_about"));
        }

        private void Lang_Click(object sender, EventArgs e)
        {
            ChangeLang(sender.ToString());
        }

        //*********************************************//
        private void ContentModified(object sender, EventArgs e)
        {
            CheckModification();
        }

        private void FontList(object sender, EventArgs e)
        {
            SetFont();
        }

        private void FontSizeList(object sender, EventArgs e)
        {
            SetFontSize();
        }

        private void FontSizeList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                SetFontSize();
        }

        private void Bold_Click(object sender, EventArgs e)
        {
            ToggleBold();
        }

        private void Italic_Click(object sender, EventArgs e)
        {
            ToggleItalic();
        }

        private void Underline_Click(object sender, EventArgs e)
        {
            ToggleUnderline();
        }

        private void Strikeout_Click(object sender, EventArgs e)
        {
            ToggleStrikeout();
        }

        private void TextLeft_Click(object sender, EventArgs e)
        {
            TextPosition("left");
        }

        private void TextCenter_Click(object sender, EventArgs e)
        {
            TextPosition("center");
        }

        private void TextRight_Click(object sender, EventArgs e)
        {
            TextPosition("right");
        }
    }
}
