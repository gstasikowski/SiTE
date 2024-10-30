using System;  
using System.Windows.Input;

namespace SiTE.Logic
{
    public class NoteLinkCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            string url = (string)parameter;

            if (url.StartsWith("nID:"))
            {
                url = url.Replace("nID:", string.Empty);
                Guid noteID = Guid.Parse(url);
                Core.Instance.dataBank.OpenNote(noteID);
            }
            else
            {
                if (!url.StartsWith("http"))
                {
                    url = url.Insert(0, "https://");
                }

                url = url.Replace("&", "^&");
                Markdown.Avalonia.Utils.DefaultHyperlinkCommand.GoTo(url);
            }
        }
    }
}