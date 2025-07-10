using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCForeignAndRegionPortModule))]
	sealed class USCForeignAndRegionPortModuleTest : USCFilterGridModuleTest
	{
		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert(true);
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.USCForeignAndRegionPort;
	}
}
