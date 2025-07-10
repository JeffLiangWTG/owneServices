using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	class NctsWarehouseToOpenLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestIncotermList()
		{
			var document = Factory.New<NctsWarehouseToOpen>();

			AssertSame(Factory.GetCachedValue<IncotermCodeList>(), document.Lookups.IncotermList);
		}

		public void TestSubTypeList()
		{
			var document = Factory.New<NctsWarehouseToOpen>();

			AssertSame(Factory.GetCachedValue<PaymentTypeList>(), document.Lookups.SubTypeList);
		}

		public void TestPackagesTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
			helper.CreateCusCodeList(CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "ABC", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			helper.CreateCusCodeList(CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "DEF", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
			helper.CreateCusCodeList(CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "JKL", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));

			Factory.Save();
			var document = Factory.New<NctsWarehouseToOpen>();
			CombineAssertions(() =>
			{
				Assert("Code ABC exists", document.Lookups.PackagesTypeList.CodesAsString.Contains("ABC"));
				Assert("Code DEF exists", document.Lookups.PackagesTypeList.CodesAsString.Contains("DEF"));
				Assert("Code JKL not exists", !document.Lookups.PackagesTypeList.CodesAsString.Contains("JKL"));
			});
		}

		public void TestNatureOfBusinessList()
		{
			var yesterday = ZDateTime.Now.AddDays(-1);
			var tomorrow = ZDateTime.Now.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var natureOfBusinessType = "TRNOB";
			helper.CreateNewOrGetExistingCusCodeType(natureOfBusinessType, natureOfBusinessType);
			helper.CreateNewOrGetExistingCusCodeList("TR", natureOfBusinessType, "99", "Diger Ticari Islemler", yesterday, tomorrow);
			helper.CreateNewOrGetExistingCusCodeList("TR", natureOfBusinessType, "22", "Iade Edilen Esyanin Degistirilmesi", yesterday, tomorrow);
			Factory.Save();

			var document = Factory.New<NctsWarehouseToOpen>();
			CombineAssertions(() =>
			{
				AssertCollectionContains("Code 22", "22", document.Lookups.NatureOfBusinessList.GetAllCodes());
				AssertCollectionContains("Code 99", "99", document.Lookups.NatureOfBusinessList.GetAllCodes());
			});
		}

		public void TestLookups()
		{
			var document = Factory.New<NctsWarehouseToOpen>();

			AssertEquals("Ncts Previous Document Lookups", typeof(NctsWarehouseToOpenLookups), document.Lookups.GetType());
		}
	}
}
