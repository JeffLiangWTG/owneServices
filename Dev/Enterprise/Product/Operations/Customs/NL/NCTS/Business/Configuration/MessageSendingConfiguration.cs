using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

sealed class MessageSendingConfiguration : EU.NCTS.Business.MessageSendingConfiguration
{
	protected override NctsHeaderMessageSendingObjectParent GetNewNctsHeaderMessageSendingObjectParentCore(EU.NCTS.Business.NctsHeader header) => new MessageSendingActionParent(header);
}
