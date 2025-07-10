namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.TemporaryImportationBondEntrySummaries)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.TemporaryImportationBondEntrySummariesResponse)]
	[OutputBlock("XA")]
	public partial class TIBXA : MessageBlock { }
}