using Enterprise.MailManager.Business;
using Enterprise.MailManager.ExternalMailInterface;

namespace Enterprise.Recruitment.Testing.ServiceTasks
{
	sealed class MockIMailSender : ISmtpSender
	{
		public delegate void SendBehaviour(MailItem mailItem);
		public SendBehaviour SendAction;

		public void Send(MailItem mailItem) => SendAction?.Invoke(mailItem);

		public RejectedRecipientInfo[] GetRejectedRecipients()
			=> RejectedRecipients ?? System.Array.Empty<RejectedRecipientInfo>();

		public RejectedRecipientInfo[] RejectedRecipients;

		public void Dispose()
		{ }

		public string GetSendingInfo() => "From: user Server: server";
	}
}
