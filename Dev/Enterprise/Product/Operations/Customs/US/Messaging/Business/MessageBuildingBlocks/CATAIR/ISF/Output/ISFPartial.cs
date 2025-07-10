namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingResponse)]
	public partial class ISFSF90 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingStatusAdvisory)]
	public partial class ISFSA10 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingStatusAdvisory)]
	public partial class ISFSA30 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ImporterSecurityFilingStatusAdvisory)]
	public partial class ISFSA50 : MessageBlock { }
}