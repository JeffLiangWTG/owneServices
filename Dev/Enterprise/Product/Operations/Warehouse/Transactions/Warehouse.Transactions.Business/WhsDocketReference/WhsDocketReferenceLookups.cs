using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDocketReferenceLookups : AutoWhsDocketReferenceLookups
	{
		public WhsDocketReferenceLookups(AutoWhsDocketReference parent)
			: base(parent)
		{
		}

		public ICodeDescriptionPairListWithDefaultCode ReferenceTypes
		{
			get { return Factory.GetCachedValue("WarehouseDataRegistry.Instance.AdditionalReferenceType", () => WarehouseDataRegistry.Instance.AdditionalReferenceType.Value); }
		}
	}
}
