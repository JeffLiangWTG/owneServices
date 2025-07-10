using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Freight.Agency.Module.Testing
{
	internal abstract class AgencyShipmentActionSupporterTest<ShipmentT, SupporterT> : OperationalActionSupporterTest<SupporterT> where SupporterT : AgencyShipmentActionSupporter
	{
	}
}
