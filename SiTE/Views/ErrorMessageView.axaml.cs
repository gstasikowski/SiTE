using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace SiTE.Views
{
	public partial class ErrorMessageView : Window
	{
		public ErrorMessageView(string message, string errorDetails)
		{
			InitializeComponent();
			lblErrorContent.Text = message;
			lblErrorDetails.Content = errorDetails;

			expErrorDetails.IsVisible = (errorDetails == string.Empty) ? false : true;
		}
	}
}