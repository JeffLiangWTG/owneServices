using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CustomsUniversal = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RateFormulaEditHelper))]
	class RateFormulaEditHelperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsFree_Caption()
		{
			AssertEquals("Free", DataBoundResourceStrings.GetDataForProperty(new RateFormulaEditHelper(null).IsFreeInfo).Caption);
		}

		public void TestIsFreeFormat_Caption()
		{
			AssertEquals("Free Format", DataBoundResourceStrings.GetDataForProperty(new RateFormulaEditHelper(null).IsFreeFormatInfo).Caption);
		}

		public void TestIsPercentageOfCustomsValue_Caption()
		{
			AssertEquals("% of Customs Value", DataBoundResourceStrings.GetDataForProperty(new RateFormulaEditHelper(null).IsPercentageOfCustomsValueInfo).Caption);
		}

		public void TestIsRatePerUnit_Caption()
		{
			AssertEquals("Rate Per Unit", DataBoundResourceStrings.GetDataForProperty(new RateFormulaEditHelper(null).IsRatePerUnitInfo).Caption);
		}

		public void TestIsPercentageOfCustomsValueAndRatePerUnit_Caption()
		{
			AssertEquals("% of Customs Value + Rate Per Unit", DataBoundResourceStrings.GetDataForProperty(new RateFormulaEditHelper(null).IsPercentageOfCustomsValueAndRatePerUnitInfo).Caption);
		}

		public void TestIsPercentageOfCustomsValueWithAMinimumOfRatePerUnit_Caption()
		{
			AssertEquals("% of Customs Value with a Minimum of Rate Per Unit", DataBoundResourceStrings.GetDataForProperty(new RateFormulaEditHelper(null).IsPercentageOfCustomsValueWithAMinimumOfRatePerUnitInfo).Caption);
		}

		public void TestFormula_ReadOnly()
		{
			var rateFormulaEditHelper = new RateFormulaEditHelper(null);
			rateFormulaEditHelper.IsFreeFormat = true;
			AssertEquals(false, rateFormulaEditHelper.Formula_ReadOnly);
			rateFormulaEditHelper.IsFreeFormat = false;
			AssertEquals(true, rateFormulaEditHelper.Formula_ReadOnly);
		}

		public void TestPercentageOfCustomsValueAvailable()
		{
			var rateFormulaEditHelper = new RateFormulaEditHelper(null);
			rateFormulaEditHelper.IsFreeFormat = true;
			AssertEquals(false, rateFormulaEditHelper.PercentageOfCustomsValueAvailable);
			rateFormulaEditHelper.IsPercentageOfCustomsValue = true;
			AssertEquals(true, rateFormulaEditHelper.PercentageOfCustomsValueAvailable);
			rateFormulaEditHelper.IsPercentageOfCustomsValue = false;
			rateFormulaEditHelper.IsPercentageOfCustomsValueAndRatePerUnit = true;
			AssertEquals(true, rateFormulaEditHelper.PercentageOfCustomsValueAvailable);
			rateFormulaEditHelper.IsPercentageOfCustomsValueAndRatePerUnit = false;
			rateFormulaEditHelper.IsPercentageOfCustomsValueWithAMinimumOfRatePerUnit = true;
			AssertEquals(true, rateFormulaEditHelper.PercentageOfCustomsValueAvailable);
		}

		public void TestRatePerUnitAvailable()
		{
			var rateFormulaEditHelper = new RateFormulaEditHelper(null);
			rateFormulaEditHelper.IsFreeFormat = true;
			AssertEquals(false, rateFormulaEditHelper.RatePerUnitAvailable);
			rateFormulaEditHelper.IsRatePerUnit = true;
			AssertEquals(true, rateFormulaEditHelper.RatePerUnitAvailable);
			rateFormulaEditHelper.IsRatePerUnit = false;
			rateFormulaEditHelper.IsPercentageOfCustomsValueAndRatePerUnit = true;
			AssertEquals(true, rateFormulaEditHelper.RatePerUnitAvailable);
			rateFormulaEditHelper.IsPercentageOfCustomsValueAndRatePerUnit = false;
			rateFormulaEditHelper.IsPercentageOfCustomsValueWithAMinimumOfRatePerUnit = true;
			AssertEquals(true, rateFormulaEditHelper.RatePerUnitAvailable);
		}

		public void TestValidateAll()
		{
			var tariff = CreateTariff();
			var rateFormulaEditHelper = new RateFormulaEditHelper(tariff);
			rateFormulaEditHelper.Unit = "";
			rateFormulaEditHelper.PercentageOfCustomsValue = -1;
			rateFormulaEditHelper.RatePerUnit = -1;
			rateFormulaEditHelper.ClearAllNotifications();
			rateFormulaEditHelper.ValidateAll();
			AssertEquals("", rateFormulaEditHelper.Notifications.ToUniqueMessageListString());
			rateFormulaEditHelper.IsPercentageOfCustomsValue = true;
			rateFormulaEditHelper.ValidateAll();
			AssertEquals("Error - PercentageOfCustomsValue: The % Customs Value should be greater than Zero and be no more than 1000.", rateFormulaEditHelper.Notifications.ToUniqueMessageListString());
			rateFormulaEditHelper.ClearAllNotifications();
			rateFormulaEditHelper.IsRatePerUnit = true;
			rateFormulaEditHelper.ValidateAll();
			AssertEquals("Error - RatePerUnit: The Rate Per Unit should be greater than Zero and be no more than 10000.\nError - Unit: Please enter a Unit.", rateFormulaEditHelper.Notifications.ToUniqueMessageListString());
		}

		public void TestValidatePercentageOfCustomsValue()
		{
			var rateFormulaEditHelper = new RateFormulaEditHelper(null);
			var error = "The % Customs Value should be greater than Zero and be no more than 1000.";
			rateFormulaEditHelper.IsPercentageOfCustomsValue = true;
			rateFormulaEditHelper.PercentageOfCustomsValue = 0;
			AssertHasError(rateFormulaEditHelper.PercentageOfCustomsValueInfo, error);
			rateFormulaEditHelper.PercentageOfCustomsValue = 1001;
			AssertHasError(rateFormulaEditHelper.PercentageOfCustomsValueInfo, error);
			rateFormulaEditHelper.PercentageOfCustomsValue = 25.12;
			AssertNoErrors(rateFormulaEditHelper.PercentageOfCustomsValueInfo);
			rateFormulaEditHelper.IsPercentageOfCustomsValue = false;
			rateFormulaEditHelper.PercentageOfCustomsValue = 0;
			AssertNoErrors(rateFormulaEditHelper.PercentageOfCustomsValueInfo);
		}

		public void TestValidateRatePerUnit()
		{
			var rateFormulaEditHelper = new RateFormulaEditHelper(null);
			var error = "The Rate Per Unit should be greater than Zero and be no more than 10000.";
			rateFormulaEditHelper.IsRatePerUnit = true;
			rateFormulaEditHelper.RatePerUnit = 0;
			AssertHasError(rateFormulaEditHelper.RatePerUnitInfo, error);
			rateFormulaEditHelper.RatePerUnit = 10001;
			AssertHasError(rateFormulaEditHelper.RatePerUnitInfo, error);
			rateFormulaEditHelper.RatePerUnit = 25.1234;
			AssertNoErrors(rateFormulaEditHelper.RatePerUnitInfo);
			rateFormulaEditHelper.IsRatePerUnit = false;
			rateFormulaEditHelper.RatePerUnit = 0;
			AssertNoErrors(rateFormulaEditHelper.RatePerUnitInfo);
		}

		public void TestValidateUnit()
		{
			var tariff = CreateTariff();
			var rateFormulaEditHelper = new RateFormulaEditHelper(null);
			rateFormulaEditHelper.IsRatePerUnit = true;
			rateFormulaEditHelper.Unit = "";
			AssertHasErrorContaining(rateFormulaEditHelper.UnitInfo, MandatoryValidation.MustBeEntered);
			rateFormulaEditHelper.Unit = "NO";
			AssertHasErrorContaining(rateFormulaEditHelper.UnitInfo, ListValidation.InvalidCodeError);
			rateFormulaEditHelper.IsRatePerUnit = false;
			rateFormulaEditHelper.ValidateUnit();
			AssertNoErrors(rateFormulaEditHelper.UnitInfo);
			rateFormulaEditHelper = new RateFormulaEditHelper(tariff);
			rateFormulaEditHelper.IsRatePerUnit = true;
			rateFormulaEditHelper.Unit = "NO";
			AssertNoErrors(rateFormulaEditHelper.UnitInfo);
			rateFormulaEditHelper.Unit = "XX";
			AssertHasErrorContaining(rateFormulaEditHelper.UnitInfo, ListValidation.InvalidCodeError);
			rateFormulaEditHelper.Unit = "";
			AssertHasErrorContaining(rateFormulaEditHelper.UnitInfo, MandatoryValidation.MustBeEntered);
			rateFormulaEditHelper.IsRatePerUnit = false;
			rateFormulaEditHelper.ValidateUnit();
			AssertNoErrors(rateFormulaEditHelper.UnitInfo);
		}

		public void TestValidateFormula_NoUnitList()
		{
			var tariff = CreateTariff(false);
			var rateFormulaEditHelper = new RateFormulaEditHelper(tariff);
			rateFormulaEditHelper.IsFreeFormat = true;
			AssertFormulaParserError(rateFormulaEditHelper);
			AssertFormulaCountrySpecificValueError(rateFormulaEditHelper);
			var error = "The Formula is not valid due to the error(s): Unit of Measure code:";
			AssertEquals("The units of the tariff's UOMS", "", rateFormulaEditHelper.UnitList.CodesAsString);
			rateFormulaEditHelper.Formula = "MAX(5*VFD,[B])";
			AssertHasErrorContaining("UnitOfMeasure Value error 1", rateFormulaEditHelper.FormulaInfo, error);
			rateFormulaEditHelper.Formula = "10000000003 * [KG]";
			AssertHasErrorContaining("UnitOfMeasure Value error 2", rateFormulaEditHelper.FormulaInfo, error);
		}

		public void TestValidateFormula_HasUnitList()
		{
			var tariff = CreateTariff(true);
			var rateFormulaEditHelper = new RateFormulaEditHelper(tariff);
			rateFormulaEditHelper.IsFreeFormat = true;
			AssertFormulaParserError(rateFormulaEditHelper);
			AssertFormulaCountrySpecificValueError(rateFormulaEditHelper);
			var error = "The Formula is not valid due to the error(s): Unit of Measure code:";
			AssertEquals("The units of the tariff's UOMS", "NO", rateFormulaEditHelper.UnitList.CodesAsString);
			rateFormulaEditHelper.Formula = "MAX(100,[KG])";
			AssertHasErrorContaining("Kg is not in the valid unit list", rateFormulaEditHelper.FormulaInfo, error);
			rateFormulaEditHelper.Formula = "MAX(100,[NO])";
			AssertNoErrors("valid unit Value NO", rateFormulaEditHelper.FormulaInfo);
		}

		void AssertFormulaParserError(RateFormulaEditHelper rateFormulaEditHelper)
		{
			var error = "The Formula is not valid due to the error(s): Syntax error at";
			rateFormulaEditHelper.Formula = "99 / (1 && 3)";
			AssertHasErrorContaining("Parse error 1", rateFormulaEditHelper.FormulaInfo, error);
			rateFormulaEditHelper.Formula = "{}";
			AssertHasErrorContaining("Parse error 2", rateFormulaEditHelper.FormulaInfo, error);
			rateFormulaEditHelper.Formula = "3 * [KG";
			AssertHasErrorContaining("Parse error 3", rateFormulaEditHelper.FormulaInfo, error);
			rateFormulaEditHelper.Formula = "15 * 0.5 + 10";
			AssertNoErrors(rateFormulaEditHelper.FormulaInfo);
		}

		void AssertFormulaCountrySpecificValueError(RateFormulaEditHelper rateFormulaEditHelper)
		{
			var error = "The Formula is not valid due to the error(s): Country Specific Value";
			rateFormulaEditHelper.Formula = "A*B";
			AssertHasErrorContaining("Country Specific Value error 1", rateFormulaEditHelper.FormulaInfo, error);
			rateFormulaEditHelper.Formula = "MAX(A,B)";
			AssertHasErrorContaining("Country Specific Value error 2", rateFormulaEditHelper.FormulaInfo, error);
			rateFormulaEditHelper.Formula = "MAX(100,5*VFD)";
			AssertNoErrors("valid reserved word VFD", rateFormulaEditHelper.FormulaInfo);
		}

		public void TestUnitList_HasUoms()
		{
			var tariff = CreateTariff(true);
			var rateFormulaEditHelper = new RateFormulaEditHelper(tariff);
			AssertEquals("The valid UOM code", "NO", rateFormulaEditHelper.UnitList.CodesAsString);
		}

		public void TestUnitList_NoUoms()
		{
			var tariff = CreateTariff(false);
			var rateFormulaEditHelper = new RateFormulaEditHelper(tariff);
			AssertEquals(0, rateFormulaEditHelper.UnitList.Count);
		}

		public void TestUnitList_NoUnitProvider()
		{
			var rateFormulaEditHelper = new RateFormulaEditHelper(null);
			AssertEquals(0, rateFormulaEditHelper.UnitList.Count);
		}

		TariffView CreateTariff(bool createUOM = true)
		{
			var country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(CustomsUniversal.RefCusCodeListTypes.Codes.CustomsUQ, "CUSUQ");
			helper.CreateNewOrGetExistingCusCodeList(country, CustomsUniversal.RefCusCodeListTypes.Codes.CustomsUQ, "KG", "Kilogram", new ZDateTime(2016, 1, 1), new ZDateTime(2079, 06, 06));
			helper.CreateNewOrGetExistingCusCodeList(country, CustomsUniversal.RefCusCodeListTypes.Codes.CustomsUQ, "NO", "Number", new ZDateTime(2016, 1, 1), new ZDateTime(2079, 06, 06));
			var tariffType = helper.CreateNewOrGetExistingTariffType(country, "DTY");
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(country, tariffType.PK, "99999999", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "My Description", isSystem: false);
			if (createUOM)
			{
				helper.CreateTariffUOM(tariff, "CU1", "NO");
				helper.CreateTariffUOM(tariff, "CU2", "ZZ"); // Invalid uom
			}

			return tariff;
		}

		public void TestFormula_Free()
		{
			var rateFormulaEditHelper = new RateFormulaEditHelper(null);
			AssertEquals("", rateFormulaEditHelper.Formula);
			rateFormulaEditHelper.IsFree = true;
			AssertEquals("0", rateFormulaEditHelper.Formula);
		}

		public void TestFormula_PercentageOfCustomsValue()
		{
			var rateFormulaEditHelper = new RateFormulaEditHelper(null);
			rateFormulaEditHelper.IsPercentageOfCustomsValue = true;
			rateFormulaEditHelper.PercentageOfCustomsValue = 25.25;
			AssertEquals("0.2525*VFD", rateFormulaEditHelper.Formula);
		}

		public void TestFormula_RatePerUnit()
		{
			var rateFormulaEditHelper = new RateFormulaEditHelper(null);
			rateFormulaEditHelper.IsRatePerUnit = true;
			rateFormulaEditHelper.RatePerUnit = 25.25;
			rateFormulaEditHelper.Unit = "KG";
			AssertEquals("25.25*[KG]", rateFormulaEditHelper.Formula);
		}

		public void TestFormula_PercentageOfCustomsValueWithAMinimumOfRatePerUnit()
		{
			var rateFormulaEditHelper = new RateFormulaEditHelper(null);
			rateFormulaEditHelper.IsPercentageOfCustomsValueWithAMinimumOfRatePerUnit = true;
			rateFormulaEditHelper.PercentageOfCustomsValue = 25.25;
			rateFormulaEditHelper.RatePerUnit = 25.25;
			rateFormulaEditHelper.Unit = "KG";
			AssertEquals("MAX(0.2525*VFD, 25.25*[KG])", rateFormulaEditHelper.Formula);
		}

		public void TestFormula_PercentageOfCustomsValueAndRatePerUnit()
		{
			var rateFormulaEditHelper = new RateFormulaEditHelper(null);
			rateFormulaEditHelper.IsPercentageOfCustomsValueAndRatePerUnit = true;
			rateFormulaEditHelper.PercentageOfCustomsValue = 25.25;
			rateFormulaEditHelper.RatePerUnit = 25.25;
			rateFormulaEditHelper.Unit = "KG";
			AssertEquals("(0.2525*VFD) + (25.25*[KG])", rateFormulaEditHelper.Formula);
		}

		protected override BusinessObject GetNewBusinessObject() => new RateFormulaEditHelper(null);
	}
}
