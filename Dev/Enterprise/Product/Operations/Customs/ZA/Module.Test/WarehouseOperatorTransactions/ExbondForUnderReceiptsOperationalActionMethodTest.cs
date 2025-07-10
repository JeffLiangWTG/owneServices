using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(ExbondForUnderReceiptsOperationalActionMethod))]
	internal class ExbondForUnderReceiptsOperationalActionMethodTest : OperationalActionMethodTest<ExbondForUnderReceiptsOperationalActionMethod>
	{
		protected override ExbondForUnderReceiptsOperationalActionMethod NewMethod() => new ExbondForUnderReceiptsOperationalActionMethod();

		public new void TestNewGuiControl()
		{
			AssertEquals("HasControl", expected: true, Method.HasControl);
			using (var control = Method.NewGuiControl())
			{
				AssertNotNull("NewGuiControl() as ExportOperationalActionUserControl", control as ExportOperationalActionUserControl);
			}
		}
	}
}
