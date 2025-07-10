using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsWorkOrderLookups : WhsComponentOrderLookups
	{
		public WhsWorkOrderLookups(WhsWorkOrder parent)
			: base(parent)
		{
		}

		protected override CodeDescriptionPairList SubTypesCore => new CodeLists.WorkOrderType();
	}
}
