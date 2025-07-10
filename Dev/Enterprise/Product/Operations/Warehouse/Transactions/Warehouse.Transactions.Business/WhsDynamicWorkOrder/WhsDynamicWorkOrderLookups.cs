using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsDynamicWorkOrderLookups : WhsComponentOrderLookups
	{
		public WhsDynamicWorkOrderLookups(WhsDynamicWorkOrder parent)
			: base(parent)
		{
		}

		protected override CodeDescriptionPairList SubTypesCore => new CodeLists.DynamicWorkOrderType();
	}
}
