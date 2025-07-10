using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Universal.Testing
{
	internal class ZZRefCusMapCombinedValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateDateRangeIsTurnedOff()
		{
			var refCusMapType = Factory.New<RefCusMapType>();
			refCusMapType.ZZP_MapType = "XXX";
			refCusMapType.ZZP_Direction = "INW";
			refCusMapType.ZZP_Description = "XXX";
			var refCusMap = Factory.New<ZZRefCusMapCombined>();
			refCusMap.ZZM_IsSystem = false;
			refCusMap.ZZM_ZZP_NKMapType = "XXX";
			refCusMap.ZZM_ZZZ_NKDataGrouping = ZString.Empty;
			refCusMap.Validation.ValidateAll();
			AssertNoErrors(refCusMap.ZZM_StartDateInfo);
			AssertNoErrors(refCusMap.ZZM_EndDateInfo);
		}

		public void TestCheckZZM_ZZZ_NKDataGrouping()
		{
			var refCusMapType = Factory.New<RefCusMapType>();
			refCusMapType.ZZP_MapType = "XXX";
			refCusMapType.ZZP_Direction = "INW";
			refCusMapType.ZZP_Description = "XXX";
			var refCusMap = Factory.New<ZZRefCusMapCombined>();
			refCusMap.ZZM_IsSystem = false;
			refCusMap.ZZM_ZZP_NKMapType = "XXX";
			refCusMap.ZZM_ZZZ_NKDataGrouping = ZString.Empty;
			AssertMandatoryValidationError(refCusMap.ZZM_ZZZ_NKDataGroupingInfo, true);
			AssertListValidationInvalidCodeError(refCusMap.ZZM_ZZZ_NKDataGroupingInfo, false);
			var errorMessage = ZZRefCusMapCombinedValidation.MapTypeIsNotValidForCountryOrGrouping("XXX", "ZA");
			refCusMap.ZZM_ZZZ_NKDataGrouping = "!#";
			AssertMandatoryValidationError(refCusMap.ZZM_ZZZ_NKDataGroupingInfo, false);
			AssertListValidationInvalidCodeError(refCusMap.ZZM_ZZZ_NKDataGroupingInfo, true);
			AssertNoError(refCusMap.ZZM_ZZZ_NKDataGroupingInfo, errorMessage);
			refCusMap.ZZM_ZZZ_NKDataGrouping = "ZA";
			AssertMandatoryValidationError(refCusMap.ZZM_ZZZ_NKDataGroupingInfo, false);
			AssertListValidationInvalidCodeError(refCusMap.ZZM_ZZZ_NKDataGroupingInfo, false);
			AssertHasErrorContaining(refCusMap.ZZM_ZZZ_NKDataGroupingInfo, errorMessage);
			var refCusMap1 = Factory.New<RefCusMap>();
			refCusMap1.ZZM_ZZZ_NKDataGrouping = "ZA";
			refCusMap1.ZZM_ZZP_NKMapType = "XXX";
			refCusMap.Validation.ValidateZZM_ZZZ_NKDataGrouping();
			AssertNoErrorContaining(refCusMap.ZZM_ZZZ_NKDataGroupingInfo, errorMessage);
		}

		public void TestCheckZZM_ZZP_NKMapType()
		{
			var refCusMapType = Factory.LoadTop1<RefCusMapType>(new ZQuery(RefCusMapTypeSchema.ZZP_MapType, "CSTA")) ?? Factory.New<RefCusMapType>();
			refCusMapType.ZZP_MapType = "CSTA";
			refCusMapType.ZZP_Direction = "INW";
			refCusMapType.ZZP_Description = "Customs Status";
			refCusMapType.ZZP_IsReadonly = false;
			var refCusMap = Factory.New<ZZRefCusMapCombined>();
			refCusMap.ZZM_IsSystem = false;
			refCusMap.ZZM_ZZP_NKMapType = ZString.Empty;
			AssertMandatoryValidationError(refCusMap.ZZM_ZZP_NKMapTypeInfo, true);
			AssertListValidationInvalidCodeError(refCusMap.ZZM_ZZP_NKMapTypeInfo, false);
			refCusMap.ZZM_ZZP_NKMapType = "!#";
			AssertMandatoryValidationError(refCusMap.ZZM_ZZP_NKMapTypeInfo, false);
			AssertListValidationInvalidCodeError(refCusMap.ZZM_ZZP_NKMapTypeInfo, true);
			refCusMap.ZZM_ZZP_NKMapType = "CSTA";
			AssertMandatoryValidationError(refCusMap.ZZM_ZZP_NKMapTypeInfo, false);
			AssertListValidationInvalidCodeError(refCusMap.ZZM_ZZP_NKMapTypeInfo, false);
		}

		public void TestCheckZZM_CustomsValue()
		{
			var refCusMap = Factory.New<ZZRefCusMapCombined>();
			refCusMap.ZZM_IsSystem = false;
			refCusMap.ZZM_CustomsValue = ZString.Empty;
			AssertMandatoryValidationError(refCusMap.ZZM_CustomsValueInfo, true);
			refCusMap.ZZM_CustomsValue = "1";
			AssertMandatoryValidationError(refCusMap.ZZM_CustomsValueInfo, false);
		}

		public void TestCheckZZM_CW1orCommercialValue()
		{
			var refCusMap = Factory.New<ZZRefCusMapCombined>();
			refCusMap.ZZM_IsSystem = false;
			refCusMap.ZZM_CW1orCommercialValue = ZString.Empty;
			AssertMandatoryValidationError(refCusMap.ZZM_CW1orCommercialValueInfo, true);
			refCusMap.ZZM_CW1orCommercialValue = "1";
			AssertMandatoryValidationError(refCusMap.ZZM_CW1orCommercialValueInfo, false);
		}

		public void TestValidateDuplicateValue()
		{
			var factory = new BusinessObjectFactory();
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateCusMapType("CSTA", "INW", "Customs Status", false);
			helper.CreateCusMapType("ZADOC", "OUT", "ZA Supporting Document Types", false);
			helper.CreateCusMapType("REL", "BTH", "Related Party Indicator", false);
			helper.CreateCusMap("CSTA", "CLR", "1", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap("ZADOC", "TTT", "TTT", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			helper.CreateCusMap("REL", "N", "N", startDate, endDate, Core.Constants.CountryCodes.SouthAfrica);
			factory.Save();
			var refCusMap1 = Factory.New<ZZRefCusMapCombined>();
			refCusMap1.ZZM_IsSystem = false;
			refCusMap1.ZZM_ZZP_NKMapType = "CSTA";
			refCusMap1.ZZM_ZZZ_NKDataGrouping = "ZA";
			refCusMap1.ZZM_CW1orCommercialValue = "CLR";
			refCusMap1.ZZM_CustomsValue = "1";
			var refCusMap2 = Factory.New<ZZRefCusMapCombined>();
			refCusMap2.ZZM_IsSystem = false;
			refCusMap2.ZZM_ZZP_NKMapType = "CSTA";
			refCusMap2.ZZM_ZZZ_NKDataGrouping = "ZA";
			refCusMap2.ZZM_CW1orCommercialValue = "STD";
			refCusMap2.ZZM_CustomsValue = "2";
			CombineAssertions(() =>
			{
				AssertNoErrorContaining(refCusMap2.ZZM_CustomsValueInfo, "Duplicate Customs Value");
				AssertNoErrorContaining(refCusMap2.ZZM_CW1orCommercialValueInfo, "Duplicate CW1 or Commercial Value");
			}

			);
			CombineAssertions(() =>
			{
				refCusMap2.ZZM_CustomsValue = "1";
				AssertHasError(refCusMap2.ZZM_CustomsValueInfo, "Duplicate Customs Value '1' is not allowed for Mapping Type 'CSTA'.");
				AssertNoErrorContaining(refCusMap2.ZZM_CW1orCommercialValueInfo, "Duplicate CW1 or Commercial Value");
			}

			);
			CombineAssertions(() =>
			{
				refCusMap1.ZZM_ZZP_NKMapType = "ZADOC";
				refCusMap2.ZZM_ZZP_NKMapType = "ZADOC";
				AssertNoErrorContaining(refCusMap2.ZZM_CustomsValueInfo, "Duplicate Customs Value");
				AssertNoErrorContaining(refCusMap2.ZZM_CW1orCommercialValueInfo, "Duplicate CW1 or Commercial Value");
			}

			);
			CombineAssertions(() =>
			{
				refCusMap2.ZZM_CW1orCommercialValue = "CLR";
				AssertNoErrorContaining(refCusMap2.ZZM_CustomsValueInfo, "Duplicate Customs Value");
				AssertHasError(refCusMap2.ZZM_CW1orCommercialValueInfo, "Duplicate CW1 or Commercial Value 'CLR' is not allowed for Mapping Type 'ZADOC'.");
			}

			);
			CombineAssertions(() =>
			{
				refCusMap1.ZZM_ZZP_NKMapType = "REL";
				refCusMap2.ZZM_ZZP_NKMapType = "REL";
				AssertHasErrorContaining(refCusMap2.ZZM_CustomsValueInfo, "Duplicate Customs Value");
				AssertHasErrorContaining(refCusMap2.ZZM_CW1orCommercialValueInfo, "Duplicate CW1 or Commercial Value");
				AssertHasError(refCusMap2.ZZM_CustomsValueInfo, "Duplicate Customs Value '1' is not allowed for Mapping Type 'REL'.");
				AssertHasError(refCusMap2.ZZM_CW1orCommercialValueInfo, "Duplicate CW1 or Commercial Value 'CLR' is not allowed for Mapping Type 'REL'.");
			}

			);
			CombineAssertions(() =>
			{
				refCusMap2.ZZM_CustomsValue = "2";
				refCusMap2.ZZM_CW1orCommercialValue = "STD";
				AssertNoErrorContaining(refCusMap2.ZZM_CustomsValueInfo, "Duplicate Customs Value");
				AssertNoErrorContaining(refCusMap2.ZZM_CW1orCommercialValueInfo, "Duplicate CW1 or Commercial Value");
			}

			);
		}
	}
}
