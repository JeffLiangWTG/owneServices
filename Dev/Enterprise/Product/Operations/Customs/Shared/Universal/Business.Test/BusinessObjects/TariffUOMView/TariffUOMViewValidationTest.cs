using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Testing
{
	internal class TariffUOMViewValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZZ8_Type()
		{
			const string errorMessage = "There has to be a CU1 UOM";
			const string errorMessage2 = "There has to be a CU2 UOM";
			var cusTariff = CreateTariffUOMView();
			var uom = cusTariff.UnitsOfMeasure.AddNew();
			CombineAssertions(() =>
			{
				uom.ZZ8_Type = UOMTypeList.Codes.CU2;
				AssertHasError("No CU1 UOM", uom.ZZ8_TypeInfo, errorMessage);
				var uom2 = cusTariff.UnitsOfMeasure.AddNew();
				uom2.ZZ8_Type = UOMTypeList.Codes.CU1;
				uom.Validation.ValidateZZ8_Type();
				AssertNoError("Has CU1 UOM", uom.ZZ8_TypeInfo, errorMessage);
				uom.ZZ8_Type = UOMTypeList.Codes.CU3;
				AssertHasError("No CU2 UOM", uom.ZZ8_TypeInfo, errorMessage2);
				uom2.ZZ8_Type = UOMTypeList.Codes.CU2;
				uom.Validation.ValidateZZ8_Type();
				AssertNoError("Has CU2 UOM", uom.ZZ8_TypeInfo, errorMessage2);
			}

			);
		}

		public void TestCheckZZ8_Type_ListValidation()
		{
			var cusTariff = CreateTariffUOMView();
			var uom = cusTariff.UnitsOfMeasure.AddNew();
			CombineAssertions(() =>
			{
				uom.ZZ8_Type = "AA";
				AssertHasErrorContaining("AA", uom.ZZ8_TypeInfo, ListValidation.InvalidCodeError);
				uom.ZZ8_Type = UOMTypeList.Codes.CU1;
				AssertNoErrorContaining("CU1", uom.ZZ8_TypeInfo, ListValidation.InvalidCodeError);
			}

			);
		}

		public void TestCheckZZ8_Type_IsSystem()
		{
			var cusTariff = CreateTariffUOMView();
			var uom = cusTariff.UnitsOfMeasure.AddNew();
			uom.ZZ8_IsSystem = true;
			CombineAssertions(() =>
			{
				uom.ZZ8_Type = UOMTypeList.Codes.CU2;
				AssertNoErrors("No CU1 UOM", uom.ZZ8_TypeInfo);
				uom.ZZ8_Type = UOMTypeList.Codes.CU3;
				AssertNoErrors("No CU2 UOM", uom.ZZ8_TypeInfo);
				uom.ZZ8_Type = "AA";
				AssertNoErrors("Invalid type", uom.ZZ8_TypeInfo);
			}

			);
		}

		public void TestCheckZZ8_UOM()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs UQ");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KG", "KG DESC", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();
			var cusTariff = CreateTariffUOMView();
			var uom = cusTariff.UnitsOfMeasure.AddNew();
			CombineAssertions(() =>
			{
				uom.Validation.ValidateZZ8_UOM();
				AssertHasErrorContaining("ZZ8_UOM is empty", uom.ZZ8_UOMInfo, MandatoryValidation.MustBeEntered);
				uom.ZZ8_UOM = "AA";
				AssertNoErrorContaining("ZZ8_UOM isn't empty", uom.ZZ8_UOMInfo, MandatoryValidation.MustBeEntered);
				AssertHasErrorContaining("ZZ8_UOM isn't valid", uom.ZZ8_UOMInfo, ListValidation.InvalidCodeError);
				uom.ZZ8_UOM = "KG";
				AssertNoErrorContaining("ZZ8_UOM is valid", uom.ZZ8_UOMInfo, ListValidation.InvalidCodeError);
			}

			);
		}

		public void TestCheckZZ8_UOM_ListIsEmpty()
		{
			var cusTariff = CreateTariffUOMView();
			var uom = cusTariff.UnitsOfMeasure.AddNew();
			CombineAssertions(() =>
			{
				uom.Validation.ValidateZZ8_UOM();
				AssertHasErrorContaining("ZZ8_UOM is empty", uom.ZZ8_UOMInfo, MandatoryValidation.MustBeEntered);
				uom.ZZ8_UOM = "AA";
				AssertNoErrors("ZZ8_UOM isn't empty", uom.ZZ8_UOMInfo);
			}

			);
		}

		public void TestCheckZZ8_UOM_Unique()
		{
			const string message = "Unit must be unique";
			var cusTariff = CreateTariffUOMView();
			var uom = cusTariff.UnitsOfMeasure.AddNew();
			uom.ZZ8_UOM = "AA";
			CombineAssertions(() =>
			{
				AssertNoErrorContaining("ZZ8_UOM is unique", uom.ZZ8_UOMInfo, message);
				var uom2 = cusTariff.UnitsOfMeasure.AddNew();
				uom2.ZZ8_UOM = "AA";
				AssertHasErrorContaining("ZZ8_UOM isn't unique", uom2.ZZ8_UOMInfo, message);
			}

			);
		}

		public void TestCheckZZ8_UOM_IsSystem()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs UQ");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KG", "KG DESC", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();
			var cusTariff = CreateTariffUOMView();
			var uom = cusTariff.UnitsOfMeasure.AddNew();
			uom.ZZ8_IsSystem = true;
			CombineAssertions(() =>
			{
				uom.Validation.ValidateZZ8_UOM();
				AssertHasErrorContaining("ZZ8_UOM is empty", uom.ZZ8_UOMInfo, MandatoryValidation.MustBeEntered);
				uom.ZZ8_UOM = "AA";
				AssertNoErrors("ZZ8_UOM isn't valid", uom.ZZ8_UOMInfo);
			}

			);
		}

		TariffView CreateTariffUOMView()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "HSN");
			Factory.Save();
			return helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "123456789", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
		}
	}
}
