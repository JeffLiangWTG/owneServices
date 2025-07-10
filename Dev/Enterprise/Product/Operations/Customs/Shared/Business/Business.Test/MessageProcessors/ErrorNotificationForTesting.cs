using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.Business.MessageProcessors.ErrorReporting.Testing
{
	sealed class ErrorNotificationForTesting : IErrorNotification
	{
		#region Implementation of IErrorNotification

		public void SendError(EmailDef email)
		{
			SentEmail = email;
			SendErrorRun = true;
		}

		public void SendErrorToPostMaster(EmailDef email)
		{
			SentEmail = email;
			SendErrorToPostMasterRun = true;
		}

		public void LogError(string errorMessage)
		{
			LastLog = errorMessage;
		}

		public string MessageProcessorName
		{
			get { return "Test"; }
		}

		#endregion

		public string LastLog { get; set; }
		public EmailDef SentEmail { get; set; }
		public bool SendErrorRun { get; set; }
		public bool SendErrorToPostMasterRun { get; set; }
	}
}
