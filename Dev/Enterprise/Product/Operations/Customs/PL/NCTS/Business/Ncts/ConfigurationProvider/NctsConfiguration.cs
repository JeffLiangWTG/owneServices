using CargoWise.Types;

namespace Enterprise.Customs.PL.NCTS.Business;

public sealed class NctsConfiguration : EU.NCTS.Business.NctsConfiguration
{
	protected override EU.NCTS.Business.MovementHeaderConfiguration GetNewMovementHeaderConfiguration() => new MovementHeaderConfiguration();

	protected override EU.NCTS.Business.ValidationRuleConfiguration GetNewValidationRuleConfiguration() => new ValidationRuleConfiguration();

	protected override ZBool UseAdditionalDeclarationTypeCore => true;

	protected override ZBool UsePresentationDateTimeCore => true;

	protected override EU.NCTS.Business.MessageSendingConfiguration GetNewMessageSendingConfiguration() => new MessageSendingConfiguration();

	protected override ZBool UseLocalReferenceNumberIgnoreInDatabaseCheckCore => true;

	protected override EU.NCTS.Business.GuaranteeConfiguration GetNewGuaranteeConfiguration() => new GuaranteeConfiguration();

	protected override EU.NCTS.Business.CountryOfRoutingConfiguration GetNewCountryOfRoutingConfiguration() => new CountryOfRoutingConfiguration();

	protected override EU.NCTS.Business.NctsEuOfficeCodeConfiguration GetNewNctsEuOfficeCodeConfiguration() => new NctsEuOfficeCodeConfiguration();

	protected override EU.NCTS.Business.GoodsItemsConfiguration GetNewGoodsItemsConfiguration() => new GoodsItemsConfiguration();

	protected override EU.NCTS.Business.BillConfiguration GetNewBillConfiguration() => new BillConfiguration();

	protected override EU.NCTS.Business.INctsHeaderDeparturePhase5ValidationDecider GetHeaderDeparturePhase5ValidationDecider() => new NctsHeaderDeparturePhase5ValidationDecider();

	protected override EU.NCTS.Business.CommonPreviousDocumentConfiguration GetNewCommonPreviousDocumentConfiguration() => new CommonPreviousDocumentConfiguration();

	protected override EU.NCTS.Business.NctsPackageConfiguration GetNewNctsPackageConfiguration() => new NctsPackageConfiguration();
}
