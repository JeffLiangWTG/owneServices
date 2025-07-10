using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CusAuthorisationsRuleModuleFilterStrip))]
	sealed class CusAuthorisationsRuleModuleFilterStripTest : TestCaseWithFactory
	{
		public void TestCaptionRenderingEnabled()
		{
			using var control = new CusAuthorisationsRuleModuleFilterStrip();
			AssertEquals("CaptionRenderingEnabled", expected: true, control.CaptionRenderingEnabled);
		}
	}
}
