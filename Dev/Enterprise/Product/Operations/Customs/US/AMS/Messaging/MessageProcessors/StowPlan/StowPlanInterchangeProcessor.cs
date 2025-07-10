using System.Linq;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	public class StowPlanInterchangeProcessor : US.Messaging.Business.InboundInterchangeProcessor
	{
		public StowPlanInterchangeProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override bool IsNoBranchFilter => true;
		protected override bool SupportEnvironmentSwitch => true;

		protected override string[] ApplicationCodes
		{
			get { return new string[] { EDIInterchange.ApplicationCodes.StowPlan }; }
		}

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
		{
			return messageCreator ?? (messageCreator = new MessageCreator());
		}
		IInboundMessageCreator messageCreator;

		class MessageCreator : IInboundMessageCreator
		{
			public void CreateMessagesForInterchange(EDIInterchange interchange)
			{
				interchange.PopulateInterchangeFromString(
					interchange.EI_BodyText,
					EDIInterchange.ApplicationCodes.StowPlan,
					true,
					true);
				interchange.EI_FromInfo.Value = interchange.EI_FromInfo.OriginalValue;
				interchange.EI_ToInfo.Value = interchange.EI_ToInfo.OriginalValue;
				interchange.EI_InterchangeType = EDIInterchange.ApplicationCodes.StowPlan;
				interchange.SpawnMessagesFromInterchageTextAndMarkAsReceived(throwExceptionIfInDatabase: false);
				var message = (EDIMessage)interchange.ContainedMessages.FirstOrDefault();
				if (message != null)
				{
					message.EM_ApplicationCode = EDIMessage.ApplicationCodes.StowPlan;
					message.EM_GB = interchange.EI_GB;
				}
			}
		}
	}
}
