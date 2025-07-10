using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class RefCusPackListProviderTest : CargoWise.EntityFramework.Testing.TestCaseWithFactory
	{
		public void TestGetCustomsPackList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TWCIU", "Taiwan Packing Units of Measurement");
			helper.CreateCusCodeList("TW", "TWCIU", "CTN", "Carton", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList("TW", "TWCIU", "YDS", "Yards", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList("TW", "TWCIU", "DOZ", "Dozen", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList("TW", "TWCIU", "PCS", "Pieces", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeType("TWPUM", "Taiwan Customs Pack Units");
			helper.CreateCusCodeList("TW", "TWPUM", "AMP", "Ampere", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var provider = new RefCusPackListProvider();
			var list = provider.GetCustomsPackList(Factory, "PQU", "TW");
			AssertEquals("CTN, DOZ, PCS, YDS", list.CodesAsString);

			list = provider.GetCustomsPackList(Factory, "CIP", "TW");
			AssertEquals("AMP", list.CodesAsString);
		}

		public void TestGetPackConversionTypeList()
		{
			var provider = new RefCusPackListProvider();
			var list = provider.GetPackConversionTypeList(Factory);
			AssertEquals(", CIP, DTP, AMS, AFR, GMB, GMP, PKD, PQU", list.CodesAsString);
		}

		public void TestGetCustomsPackListTW()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TWPUM", "Taiwan Customs Pack Units.");
			helper.CreateCusCodeList("TW", "TWPUM", "AMP", "Ampere", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var provider = new RefCusPackListProvider();
			var actualList = provider.GetCIPCustomsPackList(Factory, ZString.Empty);
			var expectedList = TWRefCusCodeListTypes.GetCustomsPackUnitsList(Factory);
			AssertContainsExactElementsInAnyOrder(expectedList, actualList);
		}

		public void TestGetCommercialPackListTW()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("TWCIU", "Taiwan Commercial Pack Units.");
			helper.CreateCusCodeList("TW", "TWCIU", "AMP", "Ampere", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList("TW", "TWCIU", "YDS", "Yards", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList("TW", "TWCIU", "DOZ", "Dozen", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList("TW", "TWCIU", "PCS", "Pieces", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateOrGetLanguage("ZHT", "ChineseTraditional");
			helper.CreateCusCodeType("CUSUQ", "Customs Declaration Units of Quantity");
			var cusCodeList = helper.CreateCusCodeList("CN", "CUSUQ", "009", "头", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListLanguage(cusCodeList, "ZHT", "頭");
			helper.CreateCusCodeList("CN", "CUSUQ", "029", "井", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var provider = new RefCusPackListProvider();
			var list = provider.GetCommercialPackList(Factory, "CIP");
			AssertEquals("AMP, DOZ, PCS, YDS", list.CodesAsString);

			list = provider.GetCommercialPackList(Factory, "PQU");
			AssertEquals("009", list.CodesAsString);
		}

		public void TestLoaderTW()
		{
			var provider = MasterFiles.Business.RefCusPackListProvider.Loader.GetRefCusPackListProvider(Factory, Core.Constants.CountryCodes.Taiwan);
			AssertEquals("Enterprise.Customs.TW.Business.RefCusPackListProvider", provider.GetType().ToString());
		}

		public void TestGetDeclarationPackTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", "ASYCO", "PT1", "PT1 Desc", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList("TW", "TWCIU", "PT2", "PT2 Desc", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList("UNE", "UNPKG", "PT3", "PT3 Desc", new ZDateTime(1900, 1, 1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList("BR", "PKG", "PT4", "PT4 Desc", new ZDateTime(2000, 1, 1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var provider = new RefCusPackListProvider();
			var list = provider.GetDeclarationPackTypeList(Factory);

			AssertEquals("PT2", list.CodesAsString);
		}
	}
}
