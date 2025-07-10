using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Business
{
	[ModuleID(ModuleId.TradeLane)]
	public class JobTradeLaneCollection : ActiveBusinessObjectCollection<JobTradeLane>, Integration.IJobTradeLaneCollection
	{
		public JobTradeLaneCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public JobTradeLaneCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship)
		{
		}
	}
}
