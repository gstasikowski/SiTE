using System.Windows;

namespace SiTE.Logic
{
    public class ErrorHandler
    {
        public ErrorHandler(Application app, string errorCode) => new ErrorHandler(app, errorCode, string.Empty);

        public ErrorHandler(Application app, string errorCode, string exceptionMessage)
        {
            string message = (exceptionMessage != string.Empty) ? string.Format("\n\nException message: {0}", exceptionMessage) : exceptionMessage;

            try
            { message = (string)app.FindResource(errorCode) + message; }
            catch
            { message = (string)app.FindResource("ErrorDefault") + message; }

            Application.Current.MainWindow.IsEnabled = false;
            Views.ErrorMessage messageWindows = new Views.ErrorMessage(message);
            messageWindows.Show();
        }
    }
}
