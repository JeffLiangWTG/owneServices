using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AsycudaBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCustomsStatusList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.UnitedStates);
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AMSAirDispositionCode, "AMSAD", dataGrouping.ZZZ_DataGrouping);
			var date = ZDateTime.UtcNow;
			var startDate = date.AddDays(-10);
			var endDate = date.AddDays(10);
			var code1 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Z1", "Z1 DESC", startDate, endDate);
			var code2 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Z2", "Z2 DESC", startDate, endDate);
			var code3 = helper.CreateNewOrGetExistingCusCodeList(dataGrouping.ZZZ_DataGrouping, codeType.ZZK_CodeType, "Z3", "Z3 DESC", startDate, endDate);
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var list = bill.Lookups.CustomsStatusList;
			AssertEquals(3, list.Count);
			AssertEquals(code1.ZZD_Description, list.GetDescriptionFromCode(code1.ZZD_Code));
			AssertEquals(code2.ZZD_Description, list.GetDescriptionFromCode(code2.ZZD_Code));
			AssertEquals(code3.ZZD_Description, list.GetDescriptionFromCode(code3.ZZD_Code));
		}

		public void TestCustomsEntryNumberTypes()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var list = bill.Lookups.CustomsEntryNumberTypes;
			AssertCodeDescriptionPairList(list,
				(ACEManifestBillEntryNumberTypes.Codes.Informal, ACEManifestBillEntryNumberTypes.Descriptions.Informal),
				(ACEManifestBillEntryNumberTypes.Codes.Sec321a, ACEManifestBillEntryNumberTypes.Descriptions.Sec321a),
				(ACEManifestBillEntryNumberTypes.Codes.HeadnotesHTS, ACEManifestBillEntryNumberTypes.Descriptions.HeadnotesHTS),
				(ACEManifestBillEntryNumberTypes.Codes.Gifts, ACEManifestBillEntryNumberTypes.Descriptions.Gifts),
				(ACEManifestBillEntryNumberTypes.Codes.GoodsReturned, ACEManifestBillEntryNumberTypes.Descriptions.GoodsReturned),
				(ACEManifestBillEntryNumberTypes.Codes.GiftsPossessions, ACEManifestBillEntryNumberTypes.Descriptions.GiftsPossessions),
				(ACEManifestBillEntryNumberTypes.Codes.PersonalShipment, ACEManifestBillEntryNumberTypes.Descriptions.PersonalShipment));

			header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
			var newList = bill.Lookups.CustomsEntryNumberTypes;
			AssertEquals(list.GetHashCode(), newList.GetHashCode());
		}

		[TestDate(2024, 07, 23)]
		public void TestTariffCollection()
		{
			var universalTestHelper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypeHSN = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "HSN");
			tariffTypeHSN.ZZI_Description = "UnitedStates Harmonized Tariff";
			var atTariffType = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, "AT");
			atTariffType.ZZI_Description = "Alcohol Tax";
			Factory.Save();
			var tariff1 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffTypeHSN.PK, "01012100001", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-1));
			var tariff2 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffTypeHSN.PK, "01012100002", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-1));
			var tariff3 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffTypeHSN.PK, "01012100003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff4 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, tariffTypeHSN.PK, "01012100004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff5 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.France, tariffTypeHSN.PK, "01012100005", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff6 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.France, tariffTypeHSN.PK, "01012100006", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff7 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, atTariffType.PK, "01012100007", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tariff8 = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.UnitedStates, atTariffType.PK, "01012100008", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
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
	}
}
