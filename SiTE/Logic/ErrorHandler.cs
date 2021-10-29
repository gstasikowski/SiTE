using System.Windows;

namespace SiTE.Logic
{
    public class ErrorHandler
    {
        public ErrorHandler(Application app, string errorCode)
        {
            string message;

            try
            { message = (string)app.FindResource(errorCode); }
            catch
            { message = (string)app.FindResource("ErrorDefault"); }
            View.ErrorMessage messageWindows = new View.ErrorMessage(message);

            messageWindows.Show();
        }
    }
}
