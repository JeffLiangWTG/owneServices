namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.AirInbond)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.AirInbondResponse)]
	[OutputBlock("10")]
	public partial class AIRQX10 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.AirInbond)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.AirInbondResponse)]
	[OutputBlock("20")]
	public partial class AIRQX20 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.AirInbond)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.AirInbondResponse)]
	[OutputBlock("30")]
	public partial class AIRQX30 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.AirInbondUpdateTransferOfLiabilityPriorNotice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.AirInbondUpdateTransferOfLiabilityPriorNoticeResponse)]
	[OutputBlock("10")]
	public partial class AIRWX10 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.AirInbondUpdateTransferOfLiabilityPriorNotice)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.AirInbondUpdateTransferOfLiabilityPriorNoticeResponse)]
	[OutputBlock("20")]
	public partial class AIRWX20 : MessageBlock { }
}