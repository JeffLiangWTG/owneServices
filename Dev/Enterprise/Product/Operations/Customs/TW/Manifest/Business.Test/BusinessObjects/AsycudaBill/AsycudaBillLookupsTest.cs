using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class AsycudaBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestFinalDestinations()
		{
			var bill = Factory.New<AsycudaBill>();
			var list = bill.Lookups.FinalDestinations;
			AssertNotNull(list);
			Assert(list.FilterBusinessObjectDefaults.ContainsDefaultFor("CountryState:Property1"));
			AssertEquals("TW", list.FilterBusinessObjectDefaults["CountryState:Property1"].Value);
		}

		public void TestLocations()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_CustomsOffice = "AA";
			var bill = header.Bills.AddNew();
			var list = bill.Lookups.Locations;
			AssertType<ZZRefCusCodeListCombinedCollection>("Type", list);

			var collection = list as ZZRefCusCodeListCombinedCollection;
			CombineAssertions(() =>
			{
				AssertEquals("Attribute Name:Property", "CUSTOMSOFFICE", collection.FilterBusinessObjectDefaults["Attribute Name:Property"].Value);
				AssertEquals("Attribute Value:Property", "AA", collection.FilterBusinessObjectDefaults["Attribute Value:Property"].Value);
				AssertEquals("List Type:Property", "FAC", collection.FilterBusinessObjectDefaults["List Type:Property"].Value);
				AssertEquals("Effective Date:Property1", ZDateTime.Today, collection.FilterBusinessObjectDefaults["Effective Date:Property1"].Value);
				AssertContainsExactElementsInExactOrder("Country/Region or Grouping", new[] { "TW" }, collection.DataGroupingCodes);
			});
		}

		public void TestTWShipmentTypes()
		{
			var bill = Factory.New<AsycudaBill>();
			var list = bill.Lookups.TWShipmentTypes;
			var expectedList = new TWManifestShipmentTypes();
			AssertContainsExactElementsInAnyOrder(expectedList, list);
		}

		public void TestBagNumberList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertEquals("Should not have any element", 0, bill.Lookups.BagNumberList.Count);
			bill.BagNumber = "1111";
			AssertEquals("Should have 1 element", 1, bill.Lookups.BagNumberList.Count);
			AssertEquals("the element should be 1111", "1111", bill.Lookups.BagNumberList[0].Code);

			bill = header.Bills.AddNew();
			bill.BagNumber = "1111";
			bill = header.Bills.AddNew();
			bill.BagNumber = "2222";
			bill = header.Bills.AddNew();
			bill.BagNumber = "3333";

			var expectedList = new CodeDescriptionPairList();
			expectedList.AddPair("1111");
			expectedList.AddPair("2222");
			expectedList.AddPair("3333");

			bill = header.Bills.AddNew();
			AssertEquals("Should have 3 element", 3, bill.Lookups.BagNumberList.Count);
			AssertContainsExactElementsInAnyOrder("Should have all entered Bag Number from other bill but not repeated", expectedList, bill.Lookups.BagNumberList);
		}

		public void TestPackageTypeList()
		{
			new TestTWCreator(Factory).CreateInvoiceUQ();
			var bill = Factory.New<AsycudaBill>();
			var list = bill.Lookups.PackageTypeList;
			AssertEquals(1, list.Count);
			AssertEquals("AMP", list[0].Code);
			AssertEquals("Ampere", list[0].Description);
		}

		public void TestMessageStatusList()
		{
			var bill = Factory.New<AsycudaBill>();
			AssertContainsExactElementsInAnyOrder(new TWMessageStatusCodeList(), bill.Lookups.MessageStatusList);
		}

		[TestDate(2022, 07, 08)]
		public void TestTariffCollection()
		{
			var universalTestHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeHSN = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			tariffTypeHSN.ZZI_Description = "Taiwan Harmonized Tariff";
			var atTariffType = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "AT");
			atTariffType.ZZI_Description = "Alcohol Tax";
			Factory.Save();
			var tariff1 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeHSN.PK, "01012100001", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-1));
			var tariff2 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeHSN.PK, "01012100002", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-1));
			var tariff3 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeHSN.PK, "01012100003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff4 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeHSN.PK, "01012100004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff5 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.France, tariffTypeHSN.PK, "01012100005", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff6 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.France, tariffTypeHSN.PK, "01012100006", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff7 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, atTariffType.PK, "01012100007", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff8 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, atTariffType.PK, "01012100008", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var bill = Factory.New<AsycudaBill>();
			var lookups = new AsycudaBillLookups(bill);
			var collection = lookups.TariffCollection;
			collection.Load();
			AssertEquals(2, collection.Count);

			bill.ABL_Tariff = "01012100003";
			lookups = new AsycudaBillLookups(bill);
			collection = lookups.TariffCollection;
			var filter = collection.CompleteFilter;
			AssertEquals(false, tariff1.MatchesFilter(filter));
			AssertEquals(false, tariff2.MatchesFilter(filter));
			AssertEquals(true, tariff3.MatchesFilter(filter));
			AssertEquals(true, tariff4.MatchesFilter(filter));
			AssertEquals(false, tariff5.MatchesFilter(filter));
			AssertEquals(false, tariff6.MatchesFilter(filter));
			AssertEquals(false, tariff7.MatchesFilter(filter));
			AssertEquals(false, tariff8.MatchesFilter(filter));
		}

		public void TestUNDGSubstances()
		{
			var bill = Factory.New<AsycudaBill>();
			var lookups = new AsycudaBillLookups(bill);
			AssertEquals("UNDGSubstances", typeof(UNDGSubstanceCollection), lookups.UNDGSubstances.GetType());
		}
	}
}
