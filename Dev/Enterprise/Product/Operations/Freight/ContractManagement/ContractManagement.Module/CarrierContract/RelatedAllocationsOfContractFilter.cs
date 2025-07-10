using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ContractManagement.Module
{
	public class RelatedAllocationsOfContractFilter : ModuleGuidForeignCollectionFilter
	{
		public RelatedAllocationsOfContractFilter(ZString description, BusinessObjectFactory factory)
			: base(description,
				  ModuleIDs.ContractAllocationRoutes,
				  RatingContractSchema.PK,
				  RatingContractAllocationLineSchema.RCA_RCT_RatingContract,
				  new RatingContractAllocationLineCollection(factory),
				  typeof(RatingContract))
		{
		}
	}
}
