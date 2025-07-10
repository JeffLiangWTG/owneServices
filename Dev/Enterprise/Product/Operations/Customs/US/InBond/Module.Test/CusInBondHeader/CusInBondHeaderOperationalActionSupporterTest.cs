using CargoWise.Definitions;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Module.Testing
{
	[TestedType(typeof(CusInBondHeaderOperationalActionSupporter))]
	sealed class CusInBondHeaderOperationalActionSupporterTest : OperationalActionSupporterTest<CusInBondHeaderOperationalActionSupporter>
	{
		public void TestBusinessContext()
		{
			AssertEquals(BusinessContext.CusInBondHeader, Supporter.BusinessContext);
		}

		public void TestRootType()
		{
			AssertEquals(typeof(CusInBondHeader), Supporter.RootType);
		}

		protected override ModuleIdentifier ModuleID => ModuleIDs.Customs.US.InBond;
	}
}
