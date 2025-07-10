using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class EmailSenderConfigurationForTest : EmailSenderConfiguration
	{
		public EmailSenderConfigurationForTest(ISendEmailSource contextItemSource)
			: base(contextItemSource)
		{
		}

		public bool IsCheckedToSendEmail;

		public override bool CheckIsReadyToSendEmail()
		{
			IsCheckedToSendEmail = true;
			return base.CheckIsReadyToSendEmail();
		}
	}
}
