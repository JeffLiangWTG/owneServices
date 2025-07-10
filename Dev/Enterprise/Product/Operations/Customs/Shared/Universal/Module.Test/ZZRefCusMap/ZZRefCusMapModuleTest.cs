using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Module.Testing
{
	[TestedType(typeof(ZZRefCusMapModule))]
	class ZZRefCusMapModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.Universal.ZZRefCusMap;
		}

		public void TestItemsIsAllowed()
		{
			using (var module = new ZZRefCusMapModule())
			{
				AssertEquals("View is allowed.", true, module.AllowView);
				AssertEquals("New is allowed.", true, module.AllowNew);
				AssertEquals("Edit is allowed.", true, module.AllowEdit);
				AssertEquals("Delete is allowed.", true, module.AllowDelete);
				AssertEquals("UniversalCopy is not allowed.", false, module.AllowUniversalCopy);
			}
		}

		#region GetIsSystemDefinedDefaultProperty
		protected override string GetIsSystemDefinedDefaultProperty()
		{
			return "";
		}

		#endregion

	}
}
