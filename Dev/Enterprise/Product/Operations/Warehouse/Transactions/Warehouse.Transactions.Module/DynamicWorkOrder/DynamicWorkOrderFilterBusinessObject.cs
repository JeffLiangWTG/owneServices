using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class DynamicWorkOrderFilterBusinessObject : WhsComponentOrderFilterBusinessObject
	{
		protected override bool IncludeTransportCoFilter => false;

		protected override bool IncludeConsigneeFilter => false;

		protected override bool IncludeDeliveryRouteFilters => false;

		protected override bool IncludeCustomAttribFilters => false;

		protected override CodeDescriptionPairList OrderTypes => new DynamicWorkOrderType();
	}
}
