using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeaderLookups))]
	sealed class AsycudaManifestHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLocations()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var list = header.Lookups.Locations;
			AssertType<ZZRefCusCodeListCombinedCollection>("Type", list);

			var collection = list as ZZRefCusCodeListCombinedCollection;
			CombineAssertions(() =>
			{
				AssertEquals("List Type:Property", "FAC", collection.FilterBusinessObjectDefaults["List Type:Property"].Value);
				AssertEquals("Effective Date:Property1", ZDateTime.Today, collection.FilterBusinessObjectDefaults["Effective Date:Property1"].Value);
				AssertContainsExactElementsInExactOrder("Country/Region or Grouping", new[] { "TW" }, collection.DataGroupingCodes);
			});
		}

		public void TestCompanies()
		{
			var lookups = new AsycudaManifestHeaderLookups(Factory.New<AsycudaManifestHeader>());
			CombineAssertions(() =>
			{
				var companies = lookups.Companies;
				AssertType<GlbCompanyCollection>(companies);
				AssertEquals(1, companies.Count);
				AssertEquals(GlbCompany.CurrentCompany.PK, companies[0].PK);
			});
		}

		public void TestMessageStatusList()
		{
			AssertSame(Factory.GetCachedValue<TWMessageStatusCodeList>(), Factory.New<AsycudaManifestHeader>().Lookups.MessageStatusList);
		}
	}
}
