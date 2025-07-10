using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ContractManagement.Business
{
	public class AllocationDistributionCollection : ActiveBusinessObjectCollection<RatingContractAllocationLine>, IAllocationDistributionCollection
	{
		IRatingContractAllocationLine IAllocationDistributionCollection.this[int i] => this[i];

		readonly RatingContractAllocationLine parentAllocationLine;

		public AllocationDistributionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public AllocationDistributionCollection(RatingContractAllocationLine parent)
			: base(parent.Factory, GetCollectionRelationship(parent))
		{
			parentAllocationLine = parent;
		}

		protected override void OnAdded(RatingContractAllocationLine allocationLine)
		{
			base.OnAdded(allocationLine);
			allocationLine.RCA_RCA_ParentAllocationRoute = parentAllocationLine.PK;
		}

		static CollectionRelationship GetCollectionRelationship(BusinessObject parent)
		{
			var query = new ZQuery(RatingContractAllocationLineSchema.RCA_RCA_ParentAllocationRoute, parent.PK);
			return new CollectionRelationship(typeof(RatingContractAllocationLine), query);
		}
	}
}
