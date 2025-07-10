using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Transactions.Module.Order;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(OverrideWhsOrderFulfillmentRuleActionMethod))]
	public class OverrideWhsOrderFulfillmentRuleActionMethodTest : OperationalActionMethodTest<OverrideWhsOrderFulfillmentRuleActionMethod>
	{
		#region TestNewControl

		public void TestNewControl()
		{
			var actionMethod = new OverrideWhsOrderFulfillmentRuleActionMethod();
			AssertEquals("Expecting 'Has Control' flag is set correctly", true, actionMethod.HasControl);

			using (var newControl = actionMethod.NewGuiControl())
			{
				AssertNotNull(newControl);
				AssertType<OverrideWhsOrderFulfilmentRuleReasonControl>("Expecting correct new control type", newControl);
			}
		}

		#endregion

		protected override OverrideWhsOrderFulfillmentRuleActionMethod NewMethod()
		{
			return new OverrideWhsOrderFulfillmentRuleActionMethod();
		}
	}
}
