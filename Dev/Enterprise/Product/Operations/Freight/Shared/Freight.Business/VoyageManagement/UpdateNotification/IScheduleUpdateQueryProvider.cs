using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public interface IScheduleUpdateQueryProvider
	{
		bool ShouldUpdateAgencyShipmentDatesFromATD { get; }
		bool ShouldUpdateRelatedShipmentsETD { get; }
		bool ShouldUpdateRelatedShipmentsETA { get; }
		void ShowInformation(ZString message);
		bool ShouldSendDelayAlerts(bool hasDelayedImportsVessels, bool hasDelayedExportsVessels);
	}
}
