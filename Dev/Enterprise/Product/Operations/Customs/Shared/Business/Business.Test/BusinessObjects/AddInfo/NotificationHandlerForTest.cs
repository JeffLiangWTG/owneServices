using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.Testing
{
	public class NotificationHandlerForTest : INotificationHandler
	{
		public string ReportInformationMessage;
		public string ReportInformationCaption;
		public string ReportErrorMessage;
		public string ReportErrorCaption;

		#region INotificationHandler Members

		public void ReportInformation(string message, string caption)
		{
			ReportInformationMessage = message;
			ReportInformationCaption = caption;
		}

		public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
		{
			ReportErrorMessage = message;
			ReportErrorCaption = caption;
		}

		#endregion
	}
}
