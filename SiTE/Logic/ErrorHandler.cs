using System.Windows;

namespace SiTE.Logic
{
    public class ErrorHandler
    {
        public ErrorHandler(string errorCode) => new ErrorHandler(errorCode, string.Empty);

        public ErrorHandler(string errorCode, string exceptionMessage)
        {
            string message = (exceptionMessage != string.Empty) ? string.Format("\n\nException message: {0}", exceptionMessage) : exceptionMessage;

            try
            { message = (string)Application.Current.FindResource(errorCode) + message; }
            catch
            { message = (string)Application.Current.FindResource("ErrorDefault") + message; }

            Views.ErrorMessage messageWindow = new Views.ErrorMessage(message);
            messageWindow.ShowDialog();
        }
    }
}
