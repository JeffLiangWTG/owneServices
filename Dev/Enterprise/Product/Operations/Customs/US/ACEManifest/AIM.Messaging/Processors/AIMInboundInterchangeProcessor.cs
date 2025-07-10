using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.US.AIM.Messaging
{
	public class AIMInboundInterchangeProcessor : InboundInterchangeProcessor
	{
		public AIMInboundInterchangeProcessor() : base()
		{
		}

		public AIMInboundInterchangeProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string[] ApplicationCodes => new string[] { EDIInterchange.ApplicationCodes.USAMA };
		protected override bool IsNoBranchFilter => true;

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange) => new AIMInboundMessageCreator();
	}

	class AIMInboundMessageCreator : IInboundMessageCreator
	{
		public AIMInboundMessageCreator()
		{
		}

		void IInboundMessageCreator.CreateMessagesForInterchange(EDIInterchange interchange)
		{
			var aimInterchange = (AIMEDIInterchange)interchange;
			aimInterchange.CreateMessageFromInterchange();
			aimInterchange.EI_Status = EDIMessage.Status.Received;
		}
	}
}
