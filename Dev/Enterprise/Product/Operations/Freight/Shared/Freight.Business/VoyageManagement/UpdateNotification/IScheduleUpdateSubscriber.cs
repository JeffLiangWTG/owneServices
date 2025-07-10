using CargoWise.Types;

namespace Enterprise.Freight.Business
{
	public interface IScheduleUpdateSubscriber
	{
		void ATDChanged(IScheduleUpdateServices services, VoyageOrigin origin, ZDateTime oldATD);
		void ETDChanged(IScheduleUpdateServices services, VoyageOrigin origin, ZDateTime oldETD);
		void ETAChanged(IScheduleUpdateServices services, VoyageDestination destination, ZDateTime oldETA);
	}
}
