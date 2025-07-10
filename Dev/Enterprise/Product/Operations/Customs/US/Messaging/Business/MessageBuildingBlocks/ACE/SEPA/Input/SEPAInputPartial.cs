namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[OutputBlock("PE10")]
	public partial class SEPAPE10
	{
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNotice)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse)]
	[OutputBlock("PE15")]
	public partial class SEPAPE15
	{
	}
}
