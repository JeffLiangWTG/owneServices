using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(StocktakeController))]
	public class StocktakeControllerBasherTest : WhsControllerBaseBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.WhsStocktake;
		}

		#region TestSecurity

		public void TestSecurity()
		{
			AssertEquals(Env.Security.None, Controller.GetCheckPointForDelete(null));
			AssertEquals(Env.Security.WhsStocktakeView, Controller.GetCheckPointForView(null));
			AssertEquals(Env.Security.WhsStocktakeNew, Controller.GetCheckPointForNew(null));
			AssertEquals(Env.Security.WhsStocktakeEdit, Controller.GetCheckPointForEdit(null));
		}

		#endregion

	}
}
