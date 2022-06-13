using System.Windows;

namespace SiTE.Logic
{
    public class ErrorHandler
    {
        public ErrorHandler(string errorCode) => new ErrorHandler(errorCode, string.Empty);

        public ErrorHandler(string errorCode, string exceptionMessage)
        {
            string message;
            string details = (exceptionMessage != string.Empty) ? string.Format("{0}: {1}", (string)Application.Current.FindResource("MessageErrorDetails"), exceptionMessage) : string.Empty;

            try
            { message = (string)Application.Current.FindResource(errorCode); }
            catch
            { message = (string)Application.Current.FindResource("ErrorDefault"); }

            Views.ErrorMessage messageWindow = new Views.ErrorMessage(message, details);
            messageWindow.ShowDialog();
        }
    }
}
