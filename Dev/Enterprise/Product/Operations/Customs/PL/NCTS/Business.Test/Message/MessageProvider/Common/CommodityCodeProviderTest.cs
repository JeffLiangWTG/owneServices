using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CommodityCodeProviderTest : Customs.Business.Testing.DataProviderTestCase<CommodityCodeProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null NctsCommonCargoDesc", "Value cannot be null.\r\nParameter name: goodsItem", () => new CommodityCodeProvider(null));
			AssertExceptionThrown<ArgumentNullException>("Null NctsHeader", "Value cannot be null.\r\nParameter name: NctsCommonCargoDesc.Header", () => new CommodityCodeProvider(Factory.New<NctsDepartureCargoDesc>()));
		});
	}

	public void TestHarmonizedSystemSubHeadingCode()
	{
		CombineAssertions(() =>
		{
			goodsItem.BY_HarmonisedTariff = "12345678";
			AssertEquals("Takes only 6 first digits", "123456", Provider.HarmonizedSystemSubHeadingCode);

			goodsItem.BY_HarmonisedTariff = "1234";
			AssertEquals("Takes all available chars from 6 first digits", "1234", Provider.HarmonizedSystemSubHeadingCode);

			goodsItem.BY_HarmonisedTariff = null;
			AssertEquals("Does not fail on null", string.Empty, Provider.HarmonizedSystemSubHeadingCode);
		});
	}

		[TestDate(2022, 07, 01)]
		public void TestCombinedNomenclatureCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112, "CL112 Desc.");
			helper.CreateCusCodeList("EUN", EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL148, "AT", "Austria", new ZDateTime(2022, 01, 01), new ZDateTime(2022, 12, 31));
			helper.CreateCusCodeList("EUN", UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL112, "CY", "Cyprus", new ZDateTime(2022, 01, 01), new ZDateTime(2022, 12, 31));
			Factory.Save();

		var customsOffice = header.MovementHeader.CustomsOffices.AddNew();
		customsOffice.CY_Code = EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
		goodsItem.BY_HarmonisedTariff = "ABCDEF1234";

		CombineAssertions(() =>
		{
			customsOffice.CY_Data = "AT12345";
			AssertEquals("Departure office not in CL112 country", "12", Provider.CombinedNomenclatureCode);

			customsOffice.CY_Data = "CY12345";
			AssertNullOrEmpty("Departure office in CL112 country", GetProvider().CombinedNomenclatureCode);
		});
	}

	protected override CommodityCodeProvider GetProvider()
	{
		return new CommodityCodeProvider(goodsItem);
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(NctsMovementType.Codes.Departure);
		goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
	}

	NctsHeader header;
	NctsDepartureCargoDesc goodsItem;
}
