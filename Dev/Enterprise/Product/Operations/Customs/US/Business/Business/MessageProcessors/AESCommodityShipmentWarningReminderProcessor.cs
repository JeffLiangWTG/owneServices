using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Output;

namespace Enterprise.Customs.US.Business.MessageProcessors
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.AES.CommodityShipmentWarningReminder, CBPEDIInterchange.ApplicationCodes.USCustomsExport)]
	public class AESCommodityShipmentWarningReminderProcessor : AESTIRProcessor<AESCommWarnAXN, AESCommWarnBXN, AESCommWarnYXN, AESCommWarnES1XN>
	{
	}
}
