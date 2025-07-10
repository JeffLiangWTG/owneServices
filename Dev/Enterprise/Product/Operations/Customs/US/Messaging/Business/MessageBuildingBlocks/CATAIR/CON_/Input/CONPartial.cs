namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ConsigneeNameAddressQuery)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ConsigneeNameAddressQueryResponse)]
	[OutputBlock("K3")]
	public partial class CONK3 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ConsigneeNameAddressQuery)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ConsigneeNameAddressQueryResponse)]
	[OutputBlock("K4")]
	public partial class CONK4 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ConsigneeNameAddressAdd)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ConsigneeNameAddressAddResponse)]
	[OutputBlock("I3")]
	public partial class CONI3 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ConsigneeNameAddressAdd)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ConsigneeNameAddressAddResponse)]
	[OutputBlock("I4")]
	public partial class CONI4 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactionsResponse)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ConsigneeNameAddressAdd)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ConsigneeNameAddressAddResponse)]
	[OutputBlock("I7")]
	public partial class CONI7 : MessageBlock { }
}