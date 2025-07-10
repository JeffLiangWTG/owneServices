using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class QueryADDCVDMessageSender : MessageSender
	{
		public QueryADDCVDMessageSender(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override bool Prepare()
		{
			return true;
		}

		protected override bool GenerateMessage()
		{
			if (new ReferenceFileRequester(Job.Factory).RequestADDCVDs(Job))
			{
				return true;
			}
			else
			{
				if (!Job.IsACE)
				{
					Job.MessageInitiator.WarnUserAboutSomething(NotSupportedByCBP, "Send Messages");
				}
				else
				{
					Job.MessageInitiator.WarnUserAboutSomething(NoADDCVDQueryMessagesSent, "Send Messages");
				}
				return false;
			}
		}
		public const string NoADDCVDQueryMessagesSent = "No query messages sent. Make sure you have entered ADD/CVD case numbers and/or Tariff.";
		public const string NotSupportedByCBP = "No query messages sent. This message of ACS Job is no longer supported by CBP.";

		protected override string SuccessfulSendNotification
		{
			get { return SuccessfulSendMessage; }
		}
		const string SuccessfulSendMessage = "ADD/CVD requests have been sent and the messages are attached to this job.";
	}
}
