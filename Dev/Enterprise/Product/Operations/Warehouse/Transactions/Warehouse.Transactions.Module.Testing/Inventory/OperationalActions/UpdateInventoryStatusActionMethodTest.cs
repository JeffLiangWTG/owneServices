using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(UpdateInventoryHeldCodeActionMethod))]
	class UpdateInventoryStatusActionMethodTest : OperationalActionMethodTest<UpdateInventoryHeldCodeActionMethod>
	{
		public void TestNewGuiControlAndHasControl()
		{
			var actionMethod = NewMethod();
			using (var control = actionMethod.NewGuiControl())
			{
				AssertEquals(typeof(FindInventoryHeldCode), control.GetType());
			}
			Assert(actionMethod.HasControl);
		}

		protected override UpdateInventoryHeldCodeActionMethod NewMethod()
		{
			return new UpdateInventoryHeldCodeActionMethod();
		}
	}
}
