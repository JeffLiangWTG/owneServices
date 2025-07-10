using CargoWise.EntityFramework;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ContractManagement.Business
{
	public class CarrierContractCollection : ActiveBusinessObjectCollection<RatingContract>, ICarrierContractCollection
	{
		public CarrierContractCollection(BusinessObjectFactory factory)
			: base(factory, new ZQuery(RatingContractSchema.RCT_ContractType, Core.Constants.RatingContractTypes.Provider))
		{
		}

		protected override void OnAdded(RatingContract contract)
		{
			base.OnAdded(contract);
			contract.RCT_ContractType = Core.Constants.RatingContractTypes.Provider;
		}

		IRatingContract ICarrierContractCollection.this[int i] => this[i];
	}
}
