using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgRateTariffLevelCollection))]
	sealed class OrgRateTariffLevelCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestRelationshipFilterAndSetDefaultsForNewChild()
		{
			var tariffLevel = TestCollection.AddNew();
			AssertEquals(OrgHeader.PK, tariffLevel.P7_OH);
			AssertEquals(Env.CurrentCompanyPK, tariffLevel.P7_GC);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var org2 = factory2.Load<OrgHeader>(OrgHeader.PK);
			var testCollection2 = new OrgRateTariffLevelCollection(org2, Env.CurrentCompanyPK);
			testCollection2.Load();
			AssertEquals(1, testCollection2.Count);
			AssertEquals(tariffLevel.PK, testCollection2[0].PK);
		}

		public void TestLoadRelevant()
		{
			OrgCompanyDataLookupsTest.InsertCompanyTariff(1, "Base Company Tariff");
			OrgCompanyDataLookupsTest.InsertCompanyTariff(2, "Company Tariff #2");

			OrgHeader org = (new BusinessObjectFactory()).NewWithValidTestData<OrgHeader>();
			AssertNotNull(org.CompanyData);
			var oRGTariffLevel = org.Factory.New<OrgRateTariffLevel>();
			oRGTariffLevel.P7_OH = org.PK;
			oRGTariffLevel.P7_GC = org.CompanyData.OB_GC;
			oRGTariffLevel.P7_TariffType = "ORG";
			oRGTariffLevel.P7_TariffLevel = 1;
			var wHSTariffLevel = org.Factory.New<OrgRateTariffLevel>();
			wHSTariffLevel.P7_OH = org.PK;
			wHSTariffLevel.P7_GC = org.CompanyData.OB_GC;
			wHSTariffLevel.P7_TariffType = "WHS";
			wHSTariffLevel.P7_TariffLevel = 2;
			org.Factory.Save();

			OrgRateTariffLevelCollection collection = org.CompanyData.RateTariffLevels;
			Assert(!org.CompanyData.HasChanges);
			collection.Load();
			AssertEquals(2, collection.Count);
			AssertTariffLevel(collection[0], "ORG", 1);
			AssertTariffLevel(collection[1], "WHS", 2);
		}

		void AssertTariffLevel(OrgRateTariffLevel tariffLevel, string tariffType, int level)
		{
			AssertEquals(tariffType, tariffLevel.P7_TariffType);
			AssertEquals((byte)level, tariffLevel.P7_TariffLevel);
		}

		public void TestGetRelevantLevels()
		{
			OrgCompanyDataLookupsTest.InsertCompanyTariff(1, "Base Company Tariff");
			OrgCompanyDataLookupsTest.InsertCompanyTariff(2, "Company Tariff #2");

			OrgHeader org = new BusinessObjectFactory().NewWithValidTestData<OrgHeader>();
			OrgRateTariffLevelCollection collection = org.CompanyData.RateTariffLevels;
			org.CompanyData.RateTariffLevels.AddLevel("DEF", "ALL", "ALL", ZDate.Empty, ZDate.Empty, 1);
			org.CompanyData.RateTariffLevels.AddLevel("FRT", "ALL", "ALL", ZDate.Empty, ZDate.Empty, 2);
			org.CompanyData.RateTariffLevels.AddLevel("FRT", "IMP", "ALL", new ZDate(2020, 1, 2), ZDate.Empty, 3);
			org.CompanyData.RateTariffLevels.AddLevel("FRT", "IMP", "AIR", new ZDate(2020, 1, 1), new ZDate(2020, 1, 2), 4);
			org.CompanyData.RateTariffLevels.AddLevel("FRT", "EXP", "ALL", new ZDate(2020, 1, 2), ZDate.Empty, 5);
			org.CompanyData.RateTariffLevels.AddLevel("FRT", "EXP", "LSE", ZDate.Empty, new ZDate(2020, 1, 1), 6);

			TestCase(new List<int> { 1 }, "DEF", "ALL", "ALL", "ALL");
			TestCase(new List<int> { 1 }, "DEF", "IMP", "ALL", "ALL");
			TestCase(new List<int> { 1 }, "DEF", "EXP", "ALL", "ALL");
			TestCase(new List<int> { 1, 2 }, "FRT", "ALL", "ALL", "ALL");
			TestCase(new List<int> { 1, 2, 3 }, "FRT", "IMP", "ALL", "ALL");
			TestCase(new List<int> { 1, 2, 5 }, "FRT", "EXP", "ALL", "ALL");
			TestCase(new List<int> { 1, 2, 5, 6 }, "FRT", "EXP", "LSE", "AIR");
			TestCase(new List<int> { 1, 2, 3, 4 }, "FRT", "IMP", "LSE", "AIR");
			TestCase(new List<int> { 1 }, "DEF", "ALL", "ALL", "ALL", new ZDate(2020, 1, 1), new ZDate(2020, 1, 1));
			TestCase(new List<int> { 1, 2, 6 }, "FRT", "EXP", "LSE", "AIR", new ZDate(2020, 1, 1), new ZDate(2020, 1, 1));
			TestCase(new List<int> { 1, 2, 5 }, "FRT", "EXP", "LSE", "AIR", new ZDate(2020, 1, 2), new ZDate(2020, 1, 2));
			TestCase(new List<int> { 1, 2 }, "FRT", "IMP", "ALL", "ALL", new ZDate(2019, 1, 1), new ZDate(2020, 1, 1));
			TestCase(new List<int> { 1, 2, 4 }, "FRT", "IMP", "LSE", "AIR", new ZDate(2020, 1, 1), new ZDate(2020, 1, 1));
			TestCase(new List<int> { 1, 2, 3, 4 }, "FRT", "IMP", "LSE", "AIR", new ZDate(2020, 1, 1), new ZDate(2020, 1, 2));
			TestCase(new List<int> { 1, 2, 3, 4 }, "FRT", "IMP", "LSE", "AIR", new ZDate(2020, 1, 1), ZDate.Empty);
			TestCase(new List<int> { 1, 2, 3, 4 }, "FRT", "IMP", "LSE", "AIR", ZDate.Empty, new ZDate(2020, 1, 2));

			void TestCase(IEnumerable<int> expectedLevels, string type, string direction, string mode, string generalizedMode, ZDate startDate = default, ZDate endDate = default) {
				var tariffLevels = collection.GetRelevantLevels(type, direction, mode, generalizedMode, startDate, endDate);
				var levels = tariffLevels.Select(level => (int)level.P7_TariffLevel).ToList();

				AssertContainsExactElementsInAnyOrder(ZString.Format("Should contain the correct levels for: {0}, {1}, {2}, {3}, {4}, {5}", type, direction, mode, generalizedMode, startDate, endDate), expectedLevels, levels);
			}
		}

		public void TestDefaultLevel()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals(0, org.CompanyData.RateTariffLevels.DefaultLevel);

			Env.Registry.GlobalTariffDefault = 2;
			AssertEquals(2, org.CompanyData.RateTariffLevels.DefaultLevel);
		}

		public void TestDataRefreshBus()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var frtLevel = org.CompanyData.RateTariffLevels.AddNew();
			frtLevel.P7_OH = org.PK;
			frtLevel.P7_GC = org.CompanyData.OB_GC;
			frtLevel.P7_TariffType = "FRT";
			frtLevel.P7_TariffLevel = 1;
			Factory.Save();

			OrgHeader org2 = (new BusinessObjectFactory()).Load<OrgHeader>(org.PK);
			AssertEquals(1, (int)org2.CompanyData.RateTariffLevels.GetRelevantLevels("FRT", "ALL", "ALL", "ALL").FirstOrDefault().P7_TariffLevel);

			org.CompanyData.RateTariffLevels.SetLevel("FRT", 2);
			Factory.Save();

			AssertEquals(2, (int)org2.CompanyData.RateTariffLevels.GetRelevantLevels("FRT", "ALL", "ALL", "ALL").FirstOrDefault().P7_TariffLevel);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return TestCollection;
		}

		OrgRateTariffLevelCollection TestCollection
		{
			get { return fTestCollection ?? (fTestCollection = new OrgRateTariffLevelCollection(OrgHeader, Env.CurrentCompanyPK)); }
		}

		OrgRateTariffLevelCollection fTestCollection;

		OrgHeader OrgHeader
		{
			get { return orgHeader ?? (orgHeader = Factory.NewWithValidTestData<OrgHeader>()); }
		}

		OrgHeader orgHeader;

		#endregion
	}
}
