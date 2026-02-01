namespace SiTE.Logic
{
	public class ErrorHandler
	{
		public ErrorHandler(string errorCode) => new ErrorHandler(errorCode, string.Empty);

		public ErrorHandler(string errorCode, string exceptionMessage)
		{
			string message;
			string details = (exceptionMessage != string.Empty) ? string.Format("{0}: {1}", Localizer.Instance["MessageErrorDetails"], exceptionMessage) : string.Empty;

			try
			{
				message = Localizer.Instance[errorCode];
			}
			catch
			{
				message = Localizer.Instance["ErrorDefault"];
			}

			Views.ErrorMessageView messageWindow = new Views.ErrorMessageView(message, details);
		}
	}
}
