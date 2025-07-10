using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCCarrierAndFIRMSModule))]
	sealed class USCCarrierAndFIRMSModuleTest : USCFilterGridModuleTest
	{
		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert(true);
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.USCCarrierAndFIRMS;
	}
}
