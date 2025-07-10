using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business;

sealed class CarrierShipmentJobDatesProvider(CarrierShipmentRateQueryBusinessObject parent) : JobDatesProvider<CarrierShipmentRateQueryBusinessObject>(parent)
{
	protected override ZDateTime GetCostingAutoratingDateOverrideCore()
	{
		return Parent.ReadyDate;
	}
}
