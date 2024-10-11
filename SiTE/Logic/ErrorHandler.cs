using System.Windows; // TODO replace

namespace SiTE.Logic
{
	public class ErrorHandler
	{
		public ErrorHandler(string errorCode) => new ErrorHandler(errorCode, string.Empty);

		public ErrorHandler(string errorCode, string exceptionMessage)
		{
			string message;
			string details = (exceptionMessage != string.Empty) ? string.Format("{0}: {1}", (string)Logic.Localizer.Instance["MessageErrorDetails"], exceptionMessage) : string.Empty;

			try
			{
				message = (string)Logic.Localizer.Instance[errorCode];
			}
			catch
			{
				message = (string)Logic.Localizer.Instance["ErrorDefault"];
			}

			Views.ErrorMessageView messageWindow = new Views.ErrorMessageView(message, details);
		}
	}
}
