namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output.Abstract
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("N1")]
	public partial class CTNN1 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("N3")]
	public partial class CTNN3 : MessageBlock { }
}

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("N2")]
	public partial class CTNN2 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CourtesyNoticeofLiquidation)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("N4")]
	public partial class CTNN4 : MessageBlock { }
}
