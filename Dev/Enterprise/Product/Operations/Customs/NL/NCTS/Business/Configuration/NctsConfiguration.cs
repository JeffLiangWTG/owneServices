using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public sealed class NctsConfiguration : EU.NCTS.Business.NctsConfiguration
{
	protected override INctsHeaderDeparturePhase5ValidationDecider GetHeaderDeparturePhase5ValidationDecider() => new NctsHeaderDeparturePhase5ValidationDecider();

	protected override EU.NCTS.Business.BillConfiguration GetNewBillConfiguration() => new BillConfiguration();

	protected override EU.NCTS.Business.MovementHeaderConfiguration GetNewMovementHeaderConfiguration() => new MovementHeaderConfiguration();

	protected override ZBool UseAdditionalDeclarationTypeCore => true;

	protected override EU.NCTS.Business.MessageSendingConfiguration GetNewMessageSendingConfiguration() => new MessageSendingConfiguration();

	protected override EU.NCTS.Business.LocationOfGoodsFromAuthorisationDefaulterConfiguration GetNewLocationOfGoodsFromAuthorisationDefaulterConfigurationCore() => new LocationOfGoodsFromAuthorisationDefaulterConfiguration();

	protected override EU.NCTS.Business.GoodsItemsConfiguration GetNewGoodsItemsConfiguration() => new GoodsItemsConfiguration();

	protected override EU.NCTS.Business.NctsPackageConfiguration GetNewNctsPackageConfiguration() => new NctsPackageConfiguration();
}
