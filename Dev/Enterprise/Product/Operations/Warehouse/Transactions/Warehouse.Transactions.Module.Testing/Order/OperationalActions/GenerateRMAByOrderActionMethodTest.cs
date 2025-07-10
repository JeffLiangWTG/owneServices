using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(GenerateRMAByOrdersActionMethod))]
	public class GenerateRMAByOrderActionMethodTest : OperationalActionMethodTest<GenerateRMAByOrdersActionMethod>
	{
		#region TestNewControl

		public void TestNewControl()
		{
			var actionMethod = new GenerateRMAByOrdersActionMethod();
			AssertEquals("Expecting 'Has Control' flag is set correctly", true, actionMethod.HasControl);

			using (var newControl = actionMethod.NewGuiControl())
			{
				AssertNotNull(newControl);
				AssertType<GenerateRMAReferenceReceivesByOrdersControl>("Expecting correct new control type", newControl);
			}
		}

		#endregion

		protected override GenerateRMAByOrdersActionMethod NewMethod()
		{
			return new GenerateRMAByOrdersActionMethod();
		}
	}
}
