using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.ZA;
using Enterprise.Customs.Universal.Messaging.CUSCAR;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	class AsycudaBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMessageStatusList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertSame(Factory.GetCachedValue<ZAMessageStatusList>(), bill.Lookups.MessageStatusList);
		}

		public void TestCargoReleaseStatusList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CustomsStatus");
			var goodsReleased = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "1", "Goods released", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(goodsReleased.PK, "AQM", "desc.");
			var goodsStoppedDetained = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "2", "Goods stopped / detained", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(goodsStoppedDetained.PK, "AQM", "desc.");
			var conditionalReleased = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "3", "Goods may move under Customs transfer (Customs Intervention) - Conditional Release", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(conditionalReleased.PK, "AQM", "desc.");
			var releasedToStatesWarehouse = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "50", "Released to States Warehouse", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(releasedToStatesWarehouse.PK, "AQM", "desc.");
			var other = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "51", "Other (Overboard, destroyed, lost etc)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(other.PK, "AQM", "desc.");
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			var bill = header.Bills.AddNew();
			var list = bill.Lookups.CargoReleaseStatusList;
			AssertEquals(true, list.ContainsCode("1"));
			AssertEquals(true, list.ContainsCode("2"));
			AssertEquals(true, list.ContainsCode("3"));
			AssertEquals(true, list.ContainsCode("50"));
			AssertEquals(true, list.ContainsCode("51"));
		}

		public void TestCustomsStatusList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "CustomsManifestStatus");
			var za8 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "8", "Proceed to Border", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var za9 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "9", "Already on Customs system", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var au9 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "9", "Already on Customs system", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CustomsStatus");
			var goodsReleased = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "1", "Goods released", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(goodsReleased.PK, "AQM", "desc.");
			var goodsStoppedDetained = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "2", "Goods stopped / detained", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(goodsStoppedDetained.PK, "AQM", "desc.");
			var conditionalReleased = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "3", "Goods may move under Customs transfer (Customs Intervention) - Conditional Release", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(conditionalReleased.PK, "AQM", "desc.");
			var releasedToStatesWarehouse = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "50", "Released to States Warehouse", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(releasedToStatesWarehouse.PK, "AQM", "desc.");
			var other = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "51", "Other (Overboard, destroyed, lost etc)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(other.PK, "AQM", "desc.");
			Factory.Save();
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = nameof(ManifestDocumentType.AQM);
			var bill = header.Bills.AddNew();
			var list = bill.Lookups.CustomsStatusList;
			AssertEquals(true, list.ContainsCode("1"));
			AssertEquals(true, list.ContainsCode("2"));
			AssertEquals(true, list.ContainsCode("3"));
			AssertEquals(true, list.ContainsCode("50"));
			AssertEquals(true, list.ContainsCode("51"));
			header.AMA_ManifestType = nameof(ManifestDocumentType.ALH);
			list = bill.Lookups.CustomsStatusList;
			AssertEquals("Proceed to Border", list.GetDescriptionFromCode("8"));
			AssertEquals("Already on Customs system", list.GetDescriptionFromCode("9"));
			AssertEquals(false, list.ContainsCode("10"));
			AssertEquals(false, list.ContainsCode("50"));
		}

		readonly string[] expectedEntryNumberTypes = new[] { "ABT", "AFM" };

		public void TestLRNTypeList()
		{
			var bill = Factory.NewWithValidTestData<AsycudaManifestHeader>().Bills.AddNew();
			AssertContainsExactElementsInAnyOrder(expectedEntryNumberTypes, bill.Lookups.CustomsEntryNumberTypes.GetAllCodes());
		}
	}
}
