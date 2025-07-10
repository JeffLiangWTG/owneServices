using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USCarrierCombinedValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUI_AirwayBillPrefix()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_ModeOfTransportation = TransportModeCodes.Codes.VesselNonContainer;
			carrier.UI_AirwayBillPrefix = "!";
			AssertNoWarning(carrier.UI_AirwayBillPrefixInfo, USCarrierCombinedValidation.InvalidAirWaybillPrefix);

			carrier.UI_ModeOfTransportation = TransportModeCodes.Codes.AirNonContainer;
			AssertHasWarning(carrier.UI_AirwayBillPrefixInfo, USCarrierCombinedValidation.InvalidAirWaybillPrefix);

			carrier.UI_AirwayBillPrefix = "A";
			AssertHasWarning(carrier.UI_AirwayBillPrefixInfo, USCarrierCombinedValidation.InvalidAirWaybillPrefix);

			carrier.UI_AirwayBillPrefix = "A1";
			AssertHasWarning(carrier.UI_AirwayBillPrefixInfo, USCarrierCombinedValidation.InvalidAirWaybillPrefix);

			carrier.UI_AirwayBillPrefix = "A1C";
			AssertNoWarning(carrier.UI_AirwayBillPrefixInfo, USCarrierCombinedValidation.InvalidAirWaybillPrefix);

			carrier.UI_AirwayBillPrefix = "A 1";
			AssertHasWarning(carrier.UI_AirwayBillPrefixInfo, USCarrierCombinedValidation.InvalidAirWaybillPrefix);

			carrier.UI_AirwayBillPrefix = "A";
			AssertNoErrors(carrier.UI_AirwayBillPrefixInfo);
		}

		public void TestCheckUI_Name()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Name = ZString.Empty;
			AssertHasErrorContaining(carrier.UI_NameInfo, MandatoryValidation.MustBeEntered);
			carrier.UI_Name = "HELLO WORLD";
			AssertNoErrorContaining(carrier.UI_NameInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckUI_AddressIsNotEmpty()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Address = ZString.Empty;
			AssertNoErrors(carrier.UI_AddressInfo);
		}

		public void TestCheckUI_ModeOfTransportationIsNotEmpty()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_ModeOfTransportation = ZString.Empty;
			AssertNoErrors(carrier.UI_ModeOfTransportationInfo);
		}

		public void TestCheckUI_Code()
		{
			var carrier1 = Factory.New<Internal.USCarrier>();
			carrier1.USC_Code = "AB@3";
			var carrier2 = Factory.New<RefDbEntUS.USCCarrier>();
			carrier2.UI_Code = "AB@4";
			var carrier3 = Factory.New<USCarrierCombined>();
			carrier3.UI_Code = "AB@4";
			AssertNoError(carrier3.UI_CodeInfo, USCarrierCombinedValidation.DuplicateCarrierCode);
			carrier3.UI_Code = "AB@3";
			AssertHasError(carrier3.UI_CodeInfo, USCarrierCombinedValidation.DuplicateCarrierCode);
			AssertNoErrorContaining(carrier3.UI_CodeInfo, MandatoryValidation.MustBeEntered);
			carrier3.UI_Code = ZString.Empty;
			AssertHasErrorContaining(carrier3.UI_CodeInfo, MandatoryValidation.MustBeEntered);
		}
	}
}
