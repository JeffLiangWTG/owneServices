using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class BondedFactoryLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestOrganisationsLookups()
		{
			var bondedFactory = Factory.New<BondedFactory>();
			NUnit.Framework.Assert.That(bondedFactory.Lookups.Organisations.GetType(), NUnit.Framework.Is.EqualTo(typeof(BondedFactoryOrgHeaderCollection)), "BondedFactoryOrgHeaderCollection");
		}

		[ExpectNoExceptions]
		public void TestCustomsCodesList()
		{
			var bondedFactory = Factory.New<BondedFactory>();
			var customsCodesList = bondedFactory.Lookups.CustomsCodesList;
			NUnit.Framework.Assert.That(customsCodesList.ContainsCode(OrgCusCode.TaiwanCodeTypes.EPZ), NUnit.Framework.Is.True, "customsCodesList must contains EPZ");
			NUnit.Framework.Assert.That(customsCodesList.ContainsCode(OrgCusCode.TaiwanCodeTypes.CBF), NUnit.Framework.Is.True, "customsCodesList must contains CBF");
			NUnit.Framework.Assert.That(customsCodesList.ContainsCode(OrgCusCode.TaiwanCodeTypes.FTZ), NUnit.Framework.Is.True, "customsCodesList must contains FTZ");
		}
	}
}
