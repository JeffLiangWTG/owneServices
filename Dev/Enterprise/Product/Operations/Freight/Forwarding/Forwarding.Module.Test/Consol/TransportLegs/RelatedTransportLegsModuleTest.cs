using Enterprise.Freight.Forwarding.Module;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Module.Testing
{
	[TestedType(typeof(RelatedTransportLegsModule))]
	class RelatedTransportLegsModuleTest : ZModuleBasherTest
	{
		public void TestAllowCopyFilterGridHyperlinkToClipboard()
		{
			using (RelatedTransportLegsModule transportModule = new RelatedTransportLegsModule())
			{
				Assert(!transportModule.AllowCopyFilterGridHyperlinkToClipboard);
			}
		}

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RelatedTransportLegs;
		}

		public override void TestControllersDefinedForAllCountriesModuleDefinedOn()
		{
			Assert("Controller not defined on purpose", true);
		}

		#endregion
	}
}
