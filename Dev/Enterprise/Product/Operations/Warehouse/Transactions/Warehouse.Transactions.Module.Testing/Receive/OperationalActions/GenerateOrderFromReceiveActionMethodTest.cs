using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(GenerateOrderFromReceiveActionMethod))]
	public class GenerateOrderFromReceiveActionMethodTest : OperationalActionMethodTest<GenerateOrderFromReceiveActionMethod>
	{
		#region TestNewControl

		public void TestNewControl()
		{
			var actionMethod = new GenerateOrderFromReceiveActionMethod();
			AssertEquals("Expecting 'Has Control' flag is set correctly", true, actionMethod.HasControl);

			using (var newControl = actionMethod.NewGuiControl())
			{
				AssertNotNull(newControl);
				AssertType<GenerateOrderFromInventoryOrReceiveControl>("Expecting correct new control type", newControl);
			}
		}

		#endregion

		protected override GenerateOrderFromReceiveActionMethod NewMethod()
		{
			return new GenerateOrderFromReceiveActionMethod();
		}
	}
}
