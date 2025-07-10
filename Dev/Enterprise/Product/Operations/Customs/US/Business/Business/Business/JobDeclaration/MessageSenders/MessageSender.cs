using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	public abstract class MessageSender : Customs.Business.MessageSender
	{
		public MessageSender(JobDeclaration job)
			: base(job)
		{
		}

		public new JobDeclaration Job
		{
			get { return (JobDeclaration)base.Job; }
		}

		protected bool IsCreditCheckOKToSend()
		{
			var helper = new MessageManagerCreditCheckWithSecurityHelper(Job);
			if (!helper.IsCreditCheckOKToSend)
			{
				if (!helper.IsCreditCheckDoneOutsideCW1)
				{
					Job.MessageInitiator.WarnUserAboutSomething(helper.ReasonForNotAllowed, MessageCaption);
				}
				return false;
			}
			return true;
		}

		protected readonly string MessageCaption = "Send Messages";
	}
}
