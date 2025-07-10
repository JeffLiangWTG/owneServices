using CargoWise.Definitions;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Module.Testing
{
	[TestedType(typeof(eManifestOperationalActionSupporter))]
	sealed class eManifestOperationalActionSupporterTest : OperationalActionSupporterTest<eManifestOperationalActionSupporter>
	{
		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.eManifest, Supporter.BusinessContext);
		}

		public void TestRootType()
		{
			AssertEquals(typeof(Trip), Supporter.RootType);
		}

		protected override ModuleIdentifier ModuleID => ModuleIDs.Customs.US.eManifest;
	}
}
