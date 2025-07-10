using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(RefHarbourRateModule))]
	public class RefHarbourRateModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.Universal.RefHarbourRate;
		}

		public void TestRecordsViewOnlyByDefault()
		{
			using (var module = new RefHarbourRateModule())
			{
				AssertEquals("View is allowed.", true, module.AllowView);
				AssertEquals("New is not allowed.", false, module.AllowNew);
				AssertEquals("Edit is not allowed.", false, module.AllowEdit);
				AssertEquals("Delete is not allowed.", false, module.AllowDelete);
				AssertEquals("UniversalCopy is not allowed.", false, module.AllowUniversalCopy);
			}
		}
	}
}
