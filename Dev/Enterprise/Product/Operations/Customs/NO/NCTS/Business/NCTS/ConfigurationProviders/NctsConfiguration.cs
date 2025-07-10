namespace Enterprise.Customs.NO.NCTS.Business;

public sealed class NctsConfiguration : EU.NCTS.Business.NctsConfiguration
{
	protected override EU.NCTS.Business.MessageSendingConfiguration GetNewMessageSendingConfiguration() => new MessageSendingConfiguration();
}
