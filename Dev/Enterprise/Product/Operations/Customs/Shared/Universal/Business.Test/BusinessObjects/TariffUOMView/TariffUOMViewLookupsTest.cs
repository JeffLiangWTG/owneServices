using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Testing
{
	internal class TariffUOMViewLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTypeList()
		{
			var uom = Factory.New<TariffUOMView>();
			var typeList = uom.Lookups.TypeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "CU1, CU2, CU3", typeList.CodesAsString);
				AssertSame("Cached", uom.Lookups.TypeList, typeList);
			}

			);
		}

		public void TestTypeList_IsSystem()
		{
			var uom = Factory.New<TariffUOMView>();
			uom.ZZ8_IsSystem = true;
			AssertEquals("Count", 0, uom.Lookups.TypeList.Count);
		}

		public void TestUOMList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs UQ");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "TNE", "TNE DESC", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KG", "KG DESC", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "HSN");
			Factory.Save();
			var cusTariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "123456789", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			var uom = cusTariff.UnitsOfMeasure.AddNew();
			var uomList = uom.Lookups.UOMList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "KG, TNE", uomList.CodesAsString);
				AssertSame("Cached", uom.Lookups.UOMList, uomList);
			}

			);
		}

		public void TestUOMList_IsSystem()
		{
			var uom = Factory.New<TariffUOMView>();
			uom.ZZ8_IsSystem = true;
			AssertEquals("Count", 0, uom.Lookups.UOMList.Count);
		}
	}
}
