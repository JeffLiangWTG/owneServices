using CargoWise.Types;

namespace Enterprise.Customs.US.AMS.Messaging.Business.StowPlan
{
	public interface IStowPlanVesselData
	{
		ZString VesselName { get; }
		ZString IMONumber { get; }
		ZString VesselOperator { get; }
	}
}
