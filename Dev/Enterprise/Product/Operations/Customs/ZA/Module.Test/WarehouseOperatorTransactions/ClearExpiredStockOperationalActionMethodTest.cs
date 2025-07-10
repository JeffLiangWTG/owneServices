using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(ClearExpiredStockOperationalActionMethod))]
	class ClearExpiredStockOperationalActionMethodTest : OperationalActionMethodTest<ClearExpiredStockOperationalActionMethod>
	{
		protected override ClearExpiredStockOperationalActionMethod NewMethod() => new ClearExpiredStockOperationalActionMethod();

		public new void TestNewGuiControl()
		{
			AssertEquals("HasControl", expected: true, Method.HasControl);
			using (var control = Method.NewGuiControl())
			{
				AssertNotNull("NewGuiControl() as ClearExpiredStockOperationalActionUserControl", control as ClearExpiredStockOperationalActionUserControl);
			}
		}
	}
}
