using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(GenerateOrderFromInventoryActionMethod))]
	public class GenerateOrderFromInventoryActionMethodTest : OperationalActionMethodTest<GenerateOrderFromInventoryActionMethod>
	{
		public void TestNewControl()
		{
			var actionMethod = new GenerateOrderFromInventoryActionMethod();
			AssertEquals("Expecting 'Has Control' flag is set correctly", true, actionMethod.HasControl);

			using (var newControl = actionMethod.NewGuiControl())
			{
				AssertNotNull(newControl);
				AssertType<GenerateOrderFromInventoryOrReceiveControl>("Expecting correct new control type", newControl);
			}
		}

		#region Implementation

		protected override GenerateOrderFromInventoryActionMethod NewMethod()
		{
			return new GenerateOrderFromInventoryActionMethod();
		}

		#endregion

	}
}
