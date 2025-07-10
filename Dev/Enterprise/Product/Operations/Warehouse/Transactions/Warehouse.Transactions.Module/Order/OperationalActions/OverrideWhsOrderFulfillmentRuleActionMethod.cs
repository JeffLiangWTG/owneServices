using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Module.Order;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class OverrideWhsOrderFulfillmentRuleActionMethod : OperationalActionMethod
	{
		public OverrideWhsOrderFulfillmentRuleActionMethod()
			: base(new ZGuid("E93E8F5C-613B-40A7-A266-F426CB1E2628"))
		{
		}

		public override bool HasControl => true;

		public override IComponent NewGuiControl() => new OverrideWhsOrderFulfilmentRuleReasonControl();

		public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
			=> new OverrideWhsOrderFulfillmentRuleActionMethodApplicator(factory);

		public override string Name => Res.GetString("91EAA27B-A1A9-4417-98A3-1B502EF2E6DC", "Override Warehouse Order Fulfillment Rule");

		public override string Description => Name;
	}
}
