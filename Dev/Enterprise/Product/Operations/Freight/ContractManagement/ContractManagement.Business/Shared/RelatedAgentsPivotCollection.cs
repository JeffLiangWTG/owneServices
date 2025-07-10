using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ContractManagement.Business
{
	public class RelatedAgentsPivotCollection : PivotBusinessObjectCollection<AllocationRouteAgentPivot>, IRelatedAgentsPivotCollection<AllocationRouteAgentPivot>
	{
		public RelatedAgentsPivotCollection(RatingContractAllocationLine master)
			: base(master, GetCollectionRelationship(master))
		{
		}

		public IReadOnlyCollection<IOrgHeader> GetAllAgents()
		{
			return (this as IEnumerable<IPivotBusinessObject>)?
				.Select(pivot => pivot.Relation2Object)
				.OfType<IOrgHeader>()
				.ToArray() ?? Array.Empty<IOrgHeader>();
		}

		static CollectionRelationship GetCollectionRelationship(BusinessObject master)
		{
			var query = new ZQuery(AllocationRouteAgentPivotSchema.ARA_RCA_AllocationLine, master.PK);
			return new CollectionRelationship(typeof(AllocationRouteAgentPivot), query);
		}
	}
}
