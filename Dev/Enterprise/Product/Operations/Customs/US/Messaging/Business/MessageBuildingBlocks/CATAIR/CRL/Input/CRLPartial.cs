namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[OutputBlock("H1")]
	public partial class CRLH1 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[OutputBlock("H2")]
	public partial class CRLH2 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[OutputBlock("H5")]
	public partial class CRLH5 : MessageBlock { }

	public partial class CRLH2 : MessageBlock { }
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[OutputBlock("HA")]
	public partial class CRLHA : MessageBlock { }
}