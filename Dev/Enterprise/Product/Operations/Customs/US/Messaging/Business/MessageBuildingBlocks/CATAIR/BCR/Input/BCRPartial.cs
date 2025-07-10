namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("01")]
	public partial class BCR01 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("02")]
	public partial class BCR02 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoRelease)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BorderCargoReleaseResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("0M")]
	public partial class BCR0M : MessageBlock { }
}
