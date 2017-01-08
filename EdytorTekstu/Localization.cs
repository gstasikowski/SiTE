using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdytorTekstu
{
    class Localization
    {
        public int currLang = 0;

        public string appName = "SiTE";
        public string[] languages = { "English", "Polski" };

        string[][] textLines = { new string[]{"formats", "RTF document (RTF) (*.rtf)|*.rtf|Plain text (*.txt)|*.txt|HTML (*.html)|*.html|PHP (*.php)|*.php|XML (*.xml) |*.xml|All files (*.*)|*.*", 
                                                         "Tekst sformatowany (RTF) (*.rtf)|*.rtf|Dokument tekstowy (*.txt)|*.txt|Plik HTML (*.html)|*.html|Plik PHP (*.php)|*.php|Plik XML (*.xml) |*.xml|Wszystkie pliki (*.*)|*.*"},
                                 new string[]{"tm_file", "File", "Plik"}, 
                                 new string[]{"tm_edit", "Edit", "Edycja"},
                                 new string[]{"tm_format", "Format", "Format"},
                                 new string[]{"tm_prefs", "Prefernces", "Preferencje"},
                                 new string[]{"tm_help", "Help", "Pomoc"},
                                 new string[]{"f_new", "New", "Nowy"},
                                 new string[]{"f_open", "Open", "Otwórz"},
                                 new string[]{"f_save", "Save", "Zapisz"},
                                 new string[]{"f_saveAs", "Save as...", "Zapisz jako..."},
                                 new string[]{"f_page", "Page settings", "Ustawienia strony"},
                                 new string[]{"f_quit", "Exit", "Zakończ"},
                                 new string[]{"e_undo", "Undo", "Cofnij"},
                                 new string[]{"e_redo", "Redo", "Powtórz"},
                                 new string[]{"e_cut", "Cut", "Wytnij"},
                                 new string[]{"e_copy", "Copy", "Kopiuj"},
                                 new string[]{"e_paste", "Paste", "Wklej"},
                                 new string[]{"e_delete", "Delete", "Usuń"},
                                 new string[]{"e_selectAll", "Select all", "Zaznacz wszystko"},
                                 new string[]{"fr_font", "Font", "Czcionka"},
                                 new string[]{"fr_fColor", "Font color", "Kolor czcionki"},
                                 new string[]{"fr_hColor", "Highlight color", "Kolor podkreślenia"},
                                 new string[]{"fr_wordWrap", "Word wrap", "Zawijanie wierszy"},
                                 new string[]{"pr_lang", "Language", "Język"},
                                 new string[]{"h_about", "About", "O programie"},
                                 new string[]{"dlg_save", "Save current changes?", "Czy chcesz zapisać zmiany w pliku?"},
                                 new string[]{"dlg_cantSave", "Can't save the file - access denied.", "Nie można zapisać pliku - zakaz dostępu."},
                                 new string[]{"dlg_about", "\n\nAuthor: Gerard Stasikowski", "\n\nAutor: Gerard Stasikowski"},
                                 new string[]{"i_lines", "Lines", "Linii"},
                                 new string[]{"tt_new", "New (Ctrl+N)\nStart new document.", "Nowy (Ctrl+N)\nUtwórz nowy dokument."},
                                 new string[]{"tt_open", "Open (Ctrl+O)\nOpen existing file.", "Otwórz (Ctrl+O)\nOtwórz istniejący plik."},
                                 new string[]{"tt_save", "Save (Ctrl+S)\nSave current document.", "Zapisz (Ctrl+S)\nZapisz plik tekstowy."},
                                 new string[]{"tt_cut", "Cut (Ctrl+X)\nCut current selection.", "Wytnij (Ctrl+X)\nWytnij zaznaczony fragment tekstu."},
                                 new string[]{"tt_copy", "Copy (Ctrl+C)\nCopy current selection.", "Kopiuj (Ctrl+C)\nSkopiuj zaznaczony fragment tekstu."},
                                 new string[]{"tt_paste", "Paste (Ctrl+V)\nPaste text from clipboard.", "Wklej (Ctrl+V)\nWklej zawartość schowka."},
                                 new string[]{"tt_undo", "Undo (Ctrl+Z)\nUndo last change.", "Cofnij (Ctrl+Z)\nCofnij ostatnią akcję."},
                                 new string[]{"tt_redo", "Redo (Ctr+Y)\nRedo reverted change.", "Ponów (Ctrl+Y)\nPowtórz ostatnią akcję."},
                                 new string[]{"tt_bold", "Bold (Ctrl+B)", "Pogrubienie (Ctrl+B)"},
                                 new string[]{"tt_italics", "Italics (Ctrl+J)", "Kursywa (Ctrl+J)"},
                                 new string[]{"tt_underline", "Underline (Ctrl+U)", "Podkreślenie (Ctrl+U)"},
                                 new string[]{"tt_strikeout", "Strikeout (Ctrl+T)", "Przekreślenie (Ctrl+T)"},
                                 new string[]{"tt_fColor", "Font color", "Kolor czcionki"},
                                 new string[]{"tt_hColor", "Highlight color", "Kolor wyróżnienia tekstu"},
                                 new string[]{"tt_algLeft", "Align text to the left", "Wyrównaj tekst do lewej"},
                                 new string[]{"tt_algCenter", "Align text to center", "Wyrównaj tekst to środka"},
                                 new string[]{"tt_algRight", "Align text to the right", "Wyrównaj tekst do prawej"},
                                 new string[]{"tt_wordWrap", "Toggle word wrap", "Zawijanie wierszy"},
                                 //new string[]{"", "", ""},
                               };

        public string GetLine(string term)
        {
            string nextLine = "ERROR: No translation for '" + term + "'!" ;

            for (int i = 0; i < textLines.Length; i++)
                if (textLines[i][0] == term)
                {
                    nextLine = textLines[i][currLang+1];
                    break;
                }

            return nextLine;
        }

        public void ChangeLanguage(int newLang)
        {
            if (newLang > languages.Length)
                return;

            currLang = newLang;
        }
    }
}
