using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class AESTIROutgoingMessageProcessor : CBPOutgoingMessageProcessor
	{
		public AESTIROutgoingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override ZQuery MessageFilter
		{
			get
			{
				if (messageFilter == null)
				{
					messageFilter = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.USCustomsExport);
					messageFilter.AddToFilter(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.AES.CommodityShipment);
				}
				return messageFilter;
			}
		}
		ZQuery messageFilter;
	}
}
