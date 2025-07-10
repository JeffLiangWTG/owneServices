using CargoWise.EntityFramework;

namespace Enterprise.Freight.Integration
{
	public interface IDetentionDatesUpdater
	{
		void UpdateContainerDetentionDateFromSailing(BusinessObject businessObject);
	}
}
