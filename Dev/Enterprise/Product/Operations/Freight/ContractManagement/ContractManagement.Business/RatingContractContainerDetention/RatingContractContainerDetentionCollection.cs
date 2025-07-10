using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ContractManagement.Business
{
	public class RatingContractContainerDetentionCollection : ActiveBusinessObjectCollection<RatingContractContainerDetention>, IRatingContractContainerDetentionCollection
	{
		IRatingContractContainerDetention IRatingContractContainerDetentionCollection.this[int i] => this[i];

		public RatingContractContainerDetentionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public RatingContractContainerDetentionCollection(RatingContract parent)
			: base(parent.Factory, parent, new ZQuery(), RatingContractContainerDetentionSchema.RCD_RCT)
		{
		}
	}
}
