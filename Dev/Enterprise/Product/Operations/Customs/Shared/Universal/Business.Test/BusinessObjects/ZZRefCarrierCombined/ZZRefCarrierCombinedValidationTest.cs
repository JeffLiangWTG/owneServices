using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.Universal.Testing
{
	class ZZRefCarrierCombinedValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZZ4_Code()
		{
			carrier.ZZ4_CountryOrGrouping = "IT";
			carrier.ZZ4_Code = ZString.Empty;
			AssertHasErrorContaining(carrier.ZZ4_CodeInfo, MandatoryValidation.MustBeEntered);
			carrier.ZZ4_Code = "carrier";
			AssertNoErrorContaining(carrier.ZZ4_CodeInfo, MandatoryValidation.MustBeEntered);
			AssertHasError(carrier.ZZ4_CodeInfo, "This Code 'carrier' and Country/Region or Grouping 'IT' already exists.");
			carrier.ZZ4_Code = "carrier1";
			AssertNoErrors(carrier.ZZ4_CodeInfo);
		}

		public void TestCheckZZ4_CountryOrGrouping()
		{
			carrier.ZZ4_Code = "carrier";
			carrier.ZZ4_CountryOrGrouping = ZString.Empty;
			AssertHasErrorContaining(carrier.ZZ4_CountryOrGroupingInfo, MandatoryValidation.MustBeEntered);
			carrier.ZZ4_CountryOrGrouping = "CA";
			AssertHasErrorContaining(carrier.ZZ4_CountryOrGroupingInfo, ListValidation.InvalidCodeError);
			carrier.ZZ4_CountryOrGrouping = "IT";
			AssertNoErrorContaining(carrier.ZZ4_CountryOrGroupingInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(carrier.ZZ4_CountryOrGroupingInfo, ListValidation.InvalidCodeError);
			AssertHasError(carrier.ZZ4_CountryOrGroupingInfo, "This Code 'carrier' and Country/Region or Grouping 'IT' already exists.");
			carrier.ZZ4_CountryOrGrouping = "FR";
			AssertNoErrors(carrier.ZZ4_CountryOrGroupingInfo);
		}

		public void TestCheckZZ4_Description()
		{
			carrier.ZZ4_Description = ZString.Empty;
			AssertHasErrorContaining(carrier.ZZ4_DescriptionInfo, MandatoryValidation.MustBeEntered);
			carrier.ZZ4_Description = "carrier1";
			AssertNoErrors(carrier.ZZ4_DescriptionInfo);
		}

		public void TestCheckTransportMode()
		{
			var transportList = new RefCarrierTransportModeList();
			foreach (var transport in transportList.GetAllCodes())
			{
				var targetInfo = carrier.FindPropertyInfo(ZZRefCarrierCombined.GetTransportModePropertyName(transport));
				carrier.ZZ4_CountryOrGrouping = "CN";
				targetInfo.Value = ZBool.True;
				AssertNoErrors(targetInfo);
				targetInfo.Value = ZBool.False;
				AssertNoErrors(targetInfo);
				carrier.ZZ4_CountryOrGrouping = "ZA";
				targetInfo.Value = ZBool.True;
				AssertHasError(targetInfo, AttributeShouldBeSelected(transport, carrier.ZZ4_CountryOrGrouping));
				var attribute = carrier.Attributes.AddNew();
				attribute.ZZG_Name = transport;
				attribute.ZZG_Value = "aa";
				targetInfo.Value = ZBool.True;
				AssertNoErrors(targetInfo);
				targetInfo.Value = ZBool.False;
				AssertHasError(targetInfo, TransportModeShouldBeSelected(transport, carrier.ZZ4_CountryOrGrouping));
				attribute.ZZG_Name = ZString.Empty;
				targetInfo.Value = ZBool.False;
				AssertNoErrors(targetInfo);
			}
		}

		public void TestHasRequiredAttribute()
		{
			var targetInfo = carrier.ZZ4_CodeInfo;
			carrier.ZZ4_CountryOrGrouping = CountryCodes.SouthAfrica;
			carrier.ZZ4_Code = "1234";
			AssertHasError(targetInfo, "At least one attribute must be selected.");

			var attribute = carrier.Attributes.AddNew();
			attribute.ZZG_Name = RefCarrierAttributeNames.MASTER;
			attribute.ZZG_Value = RefCarrierAttributeNames.MASTER;
			carrier.Validation.ValidateZZ4_Code();
			AssertNoErrors(targetInfo);

			carrier.Attributes.DeleteAll();
			carrier.ZZ4_CountryOrGrouping = CountryCodes.France;
			carrier.ZZ4_Code = "1234";
			AssertNoErrors(targetInfo);
		}

		ZString TransportModeShouldBeSelected(ZString transport, ZString country) => $"{transport} Transport Modes should be selected when there is {transport} attribute for {country}";
		ZString AttributeShouldBeSelected(ZString transport, ZString country) => $"There should be {transport} attribute when {transport} Transport Modes is selected for {country}";
		protected override void SetUp()
		{
			base.SetUp();
			RefCarrierHelperTest.SetupMockRefCarrierConfig();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var parentGrouping = helper.CreateNewOrGetExistingDataGrouping("EUN", "EuropeanUnion");
			helper.CreateNewOrGetExistingDataGrouping("IT", "Italy", parentGrouping);
			helper.CreateNewOrGetExistingDataGrouping("FR", "France", parentGrouping);
			helper.CreateNewOrGetExistingDataGrouping("ZA", "SouthAfrica");
			Factory.Save();
			var carrierCode = Factory.New<RefCarrierCode>();
			carrierCode.ZZ4_Code = "Code";
			carrierCode.ZZ4_ZZZ_NKDataGrouping = "ZA";
			carrierCode.ZZ4_Description = "ZA Code";
			var attrA = carrierCode.Attributes.AddNew();
			attrA.ZZG_Name = "AIR";
			attrA.ZZG_Value = "Air";
			var attrB = carrierCode.Attributes.AddNew();
			attrB.ZZG_Name = "SEA";
			attrB.ZZG_Value = "Sea";
			var attrC = carrierCode.Attributes.AddNew();
			attrC.ZZG_Name = "ROA";
			attrC.ZZG_Value = "ROA";
			var attrD = carrierCode.Attributes.AddNew();
			attrD.ZZG_Name = "RAI";
			attrD.ZZG_Value = "RAI";
			ZZRefCarrierCombined duplication = Factory.New<ZZRefCarrierCombined>();
			duplication.ZZ4_Code = "carrier";
			duplication.ZZ4_CountryOrGrouping = "IT";
			duplication.ZZ4_Description = "test duplication";
			Factory.Save();
			carrier = Factory.New<ZZRefCarrierCombined>();
		}

		ZZRefCarrierCombined carrier;
	}
}
