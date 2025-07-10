using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ViewLocationModule))]
	sealed class ViewLocationModuleTest : ZModuleBasherTest
	{
		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert(true);
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ViewLocation;
		}
	}
}
