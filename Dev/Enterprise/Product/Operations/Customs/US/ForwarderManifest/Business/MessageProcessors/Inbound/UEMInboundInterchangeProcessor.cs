using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class UEMInboundInterchangeProcessor : InboundInterchangeProcessor
	{
		public UEMInboundInterchangeProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string[] ApplicationCodes => new string[] { EDIInterchange.ApplicationCodes.USExportManifest };
		protected override bool IsNoBranchFilter => true;

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange) => new UEMInboundMessageCreator();
	}

	class UEMInboundMessageCreator : IInboundMessageCreator
	{
		public UEMInboundMessageCreator()
		{
		}

		void IInboundMessageCreator.CreateMessagesForInterchange(EDIInterchange interchange)
		{
			if (interchange is UEMEDIInterchange uemInterchange)
			{
				uemInterchange.CreateMessageFromInterchange();
			}
			else
			{
				throw new MessageProcessException("Interchange type should be UEMEDIInterchange.");
			}
		}
	}
}
