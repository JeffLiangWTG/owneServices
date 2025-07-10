namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input
{
	[OutputBlock("FD10Q")]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.FoodandDrugAdministrationEstablishmentIdentifier)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.FoodandDrugAdministrationEstablishmentIdentifierResponse)]
	public partial class OGQFDPP10 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.FoodandDrugAdministrationEstablishmentIdentifier)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.FoodandDrugAdministrationEstablishmentIdentifierResponse)]
	[OutputBlock("FD20")]
	public partial class OGQFDPP20 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.ProductCodeBuilderUpdateQuery)]
	public partial class OGQFDMJ61 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.FishAndWildlifeService)]
	public partial class OGQFW101 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.FishAndWildlifeService)]
	public partial class OGQFW102 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.FoodandDrugAdministrationEstablishmentIdentifier)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.FoodandDrugAdministrationEstablishmentIdentifierResponse)]
	[OutputBlock("FD21")]
	public partial class OGQFDPP21 : MessageBlock { }

	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.FoodandDrugAdministrationEstablishmentIdentifier)]
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.FoodandDrugAdministrationEstablishmentIdentifierResponse)]
	[OutputBlock("FD22")]
	public partial class OGQFDPP22 : MessageBlock { }
}