using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class JobTradeLaneVoyageCollection : ActiveBusinessObjectCollection<JobTradeLaneVoyage>
	{
		public JobTradeLaneVoyageCollection(JobVoyage voyage)
			: base(voyage)
		{
		}
	}
}
