using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class DummyNotificationHandler : INotificationHandler
	{
		public string LastError { get; set; }
		public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
		{
			LastError = caption + message;
		}

		public void ReportInformation(string message, string caption)
		{
			LastError = caption + message;
		}
	}
}
