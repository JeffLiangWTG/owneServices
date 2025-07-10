using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChargeCodeCarrierIataMappingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateACI_IATAChargeCodeMap()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsAirLine = true;

			var iataMapping1 = Factory.NewWithValidTestData<AccChargeCodeCarrierIataMapping>();
			iataMapping1.ACI_OH_Carrier = carrier.PK;
			iataMapping1.ACI_IATAChargeCodeMap = string.Empty;
			AssertHasErrors("ACI_IATAChargeCodeMap should have errors", iataMapping1.ACI_IATAChargeCodeMapInfo);

			iataMapping1.ACI_IATAChargeCodeMap = "XX";
			AssertHasErrors("ACI_IATAChargeCodeMap should have errors", iataMapping1.ACI_IATAChargeCodeMapInfo);

			iataMapping1.ACI_IATAChargeCodeMap = Core.Constants.AWB.ChargeCodes.AC;
			AssertNoErrors("ACI_IATAChargeCodeMap should have no errors", iataMapping1.ACI_IATAChargeCodeMapInfo);
		}

		public void TestValidateACI_OH_Carrier()
		{
			var uSCompany = Factory.NewWithValidTestData<GlbCompany>();
			uSCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			var carrier1 = CreateCarrier("CARRIER1");
			var carrier2 = CreateCarrier("CARRIER2");

			var chargeCode = CreateAccChargeCode("CC1", "CC 1", uSCompany.PK, Core.Constants.ChargeType.Disbursement);
			var iataMapping1 = CreateAccChargeCodeCarrierIataMapping(chargeCode, carrier1, Core.Constants.AWB.ChargeCodes.AC);
			AssertNoErrors(iataMapping1.ACI_OH_CarrierInfo);

			var expectedMessage = $"More than one IATA codes exist for Airline organization '{carrier1.OH_Code}'.";
			var iataMapping2 = CreateAccChargeCodeCarrierIataMapping(chargeCode, carrier1, Core.Constants.AWB.ChargeCodes.DB);
			AssertHasError(iataMapping2.ACI_OH_CarrierInfo, expectedMessage);

			iataMapping2.ACI_OH_Carrier = carrier2.PK;
			AssertNoErrors(iataMapping2.ACI_OH_CarrierInfo);

			iataMapping1.Validation.ValidateACI_OH_Carrier();
			AssertNoErrors(iataMapping1.ACI_OH_CarrierInfo);
		}

		#region Implementation

		OrgHeader CreateCarrier(ZString code)
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsAirLine = true;
			carrier.OH_Code = code;

			return carrier;
		}

		AccChargeCode CreateAccChargeCode(ZString code, ZString description, ZGuid companyPK, ZString chargeType)
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = code;
			chargeCode.AC_Desc = description;
			chargeCode.AC_GC = companyPK;
			chargeCode.AC_ChargeType = chargeType;

			return chargeCode;
		}

		AccChargeCodeCarrierIataMapping CreateAccChargeCodeCarrierIataMapping(AccChargeCode chargeCode, OrgHeader carrier, ZString iataChargeCode)
		{
			var iataMapping = chargeCode.AccChargeCodeCarrierIataMappings.AddNew();
			iataMapping.ACI_OH_Carrier = carrier.PK;
			iataMapping.ACI_IATAChargeCodeMap = iataChargeCode;

			return iataMapping;
		}

		#endregion
	}
}
