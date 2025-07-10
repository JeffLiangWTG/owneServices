using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgRateTariffLevelLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestApplicableDirections()
		{
			OrgCompanyDataLookupsTest.InsertCompanyTariff(1, "Tariff 1");
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgRateTariffLevel level = Factory.New<OrgRateTariffLevel>();
			level.P7_GC = GlbCompany.CurrentCompany.PK;
			level.P7_OH = org.PK;
			OrgRateTariffLevelLookups lookups = new OrgRateTariffLevelLookups(level);
			Factory.Save();

			AssertEquals(17, lookups.TariffTypes.Count);

			foreach (CodeDescriptionPair pair in lookups.TariffTypes)
			{
				lookups.Parent.P7_TariffType = pair.Code;
				OrgCompanyDataLookupsTest.AssertDirections(pair.Code, lookups.Directions);
			}
		}

		public void TestModes()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgRateTariffLevel level = Factory.New<OrgRateTariffLevel>();
			level.P7_GC = GlbCompany.CurrentCompany.PK;
			level.P7_OH = org.PK;
			OrgRateTariffLevelLookups lookups = new OrgRateTariffLevelLookups(level);
			Factory.Save();

			AssertEquals(1, lookups.TariffTypes.Count);

			foreach (CodeDescriptionPair pair in lookups.TariffTypes)
			{
				lookups.Parent.P7_TariffType = pair.Code;
				OrgCompanyDataLookupsTest.AssertModes(pair.Code, true, lookups.Modes);
			}

			OrgCompanyDataLookupsTest.InsertCompanyTariff(1, "Tariff 1");
			Factory.ClearQueryCache(Enterprise.ZArchitecture.Schema.RatingHeaderSchema.Constants.TableName);

			org = Factory.NewWithValidTestData<OrgHeader>();
			level = Factory.New<OrgRateTariffLevel>();
			level.P7_GC = GlbCompany.CurrentCompany.PK;
			level.P7_OH = org.PK;
			lookups = new OrgRateTariffLevelLookups(level);
			Factory.Save();

			AssertEquals(17, lookups.TariffTypes.Count);

			foreach (CodeDescriptionPair pair in lookups.TariffTypes)
			{
				lookups.Parent.P7_TariffType = pair.Code;
				OrgCompanyDataLookupsTest.AssertModes(pair.Code, true, lookups.Modes);
			}
		}
	}
}

