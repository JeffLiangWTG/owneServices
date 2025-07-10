using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ContractManagement.Business
{
	[ModuleID(ModuleId.ContractAllocationRoutes)]
	public class RatingContractAllocationLineCollection : ActiveBusinessObjectCollection<RatingContractAllocationLine>, IRatingContractAllocationLineCollection
	{
		IRatingContractAllocationLine IRatingContractAllocationLineCollection.this[int i] => this[i];

		public RatingContractAllocationLineCollection(BusinessObjectFactory factory)
			: base(factory, new ZQuery())
		{
		}

		public RatingContractAllocationLineCollection(RatingContract parent)
			: base(parent.Factory, parent, new ZQuery(), RatingContractAllocationLineSchema.RCA_RCT_RatingContract)
		{
		}
	}
}
