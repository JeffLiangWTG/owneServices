using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(ZZRefCusRulingModule))]
	public class ZZRefCusRulingModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.Universal.ZZRefCusRuling;
		}

		public void TestItemsIsAllowed()
		{
			using (var module = new ZZRefCusProcedureModule())
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
