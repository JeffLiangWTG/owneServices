using CargoWise.Types;

namespace Enterprise.MarketingManager.Business
{
	public interface IScheduleItemsProvider
	{
		ZString ScheduleStatus { get; }
		ZString TableCode { get; }
	}
}
