using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(ExportNonBelnOperationalActionMethod))]
	class ExportNonBelnOperationalActionMethodTest : OperationalActionMethodTest<ExportNonBelnOperationalActionMethod>
	{
		protected override ExportNonBelnOperationalActionMethod NewMethod() => new ExportNonBelnOperationalActionMethod();

		public new void TestNewGuiControl()
		{
			AssertEquals("HasControl", expected: true, Method.HasControl);
			using (var control = Method.NewGuiControl())
			{
				AssertNotNull("NewGuiControl() as ExportOperationalActionUserControl", control as ExportOperationalActionUserControl);
			}
		}

		public void TestIsRunAgainDisabled()
		{
			Assert("Run Again option was not disabled", Method.IsRunAgainDisabled);
		}
	}
}
