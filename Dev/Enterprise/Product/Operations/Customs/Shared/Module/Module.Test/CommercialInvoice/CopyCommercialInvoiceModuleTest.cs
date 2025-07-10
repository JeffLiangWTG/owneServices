using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CopyCommercialInvoiceModule))]
	sealed class CopyCommercialInvoiceModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.CopyCommercialInvoice;
		public override void TestAutoAddedMilestoneDateFilter()
		{
			Assert("Not applicable", true);
		}

		public override void TestControllersDefinedForAllCountriesModuleDefinedOn()
		{
			Assert("No controller available for this module", true);
		}

		protected override bool HasController()
		{
			return false;
		}
	}
}
