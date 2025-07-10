using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.OnlineSailingSchedules.Testing
{
	public class LegValidationTests : TestCaseWithFactory
	{
		public void TestValidateVesselName_EmptyValue()
		{
			var leg = new Leg(Factory);
			leg.LegType = GssConstants.SeaLegType;
			var validation = new LegValidation(leg);

			validation.ValidateVesselName();
			AssertHasError(leg.VesselNameInfo, "Vessel Name cannot be empty for sea legs.");

			leg.VesselName = "Santa Maria";
			AssertNoErrors(leg.VesselNameInfo);
		}

		public void ValidateVesselName_NoValidationError_When_LegTypeIsNonSea()
		{
			var leg = new Leg(Factory);
			leg.LegType = GssConstants.RailLegType;
			var validation = new LegValidation(leg);

			validation.ValidateVesselName();
			AssertNoError(leg.VesselNameInfo, "Vessel Name cannot be empty for sea legs.");
		}

		public void TestValidateVesselName_VesselNameLengthIsExceeded()
		{
			var leg = new Leg(Factory);
			leg.LegType = GssConstants.SeaLegType;
			leg.VesselName = "123456789012345678901234567890123456";
			AssertHasError(leg.VesselNameInfo, "Maximum length of Vessel Name has been exceeded.");

			leg.VesselName = "Santa Maria";
			AssertNoErrors(leg.VesselNameInfo);
		}

		public void TestValidateVesselName_MoreThanOneVesselFoundByLloydsNumber()
		{
			var leg = new Leg(Factory);
			leg.LegType = GssConstants.SeaLegType;
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_LloydsNumber = "9308390";

			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_LloydsNumber = "9308390";

			leg.LloydsNumber = "9308390";
			leg.VesselName = "Santa Maria";
			AssertHasError(leg.LloydsNumberInfo, "More than one vessel found with IMO Number 9308390.");

			leg.LloydsNumber = "9463085";
			AssertNoErrors(leg.LloydsNumberInfo);
		}

		public void TestValidateVesselName_InactiveVessel()
		{
			var leg = new Leg(Factory);
			leg.LegType = GssConstants.SeaLegType;
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "9308390";
			vessel.RV_Name = "Santa MariaZZ";
			vessel.RV_IsActive = false;

			leg.LloydsNumber = "9308390";
			leg.VesselName = "Santa MariaZZ";
			AssertHasError(leg.VesselNameInfo, "Vessel with Vessel Name Santa MariaZZ exists in database and is inactive. Please go to Reference Files->Vessels and activate the vessel to be able to import schedule for it.");

			leg.LloydsNumber = "9463085";
			leg.VesselName = "Santa MariaZZ";
			AssertHasError(leg.VesselNameInfo, "Vessel with Vessel Name Santa MariaZZ exists in database and is inactive. Please go to Reference Files->Vessels and activate the vessel to be able to import schedule for it.");
		}

		public void TestValidateVesselName_NewVesselNameCanBeCreated()
		{
			var leg = new Leg(Factory);
			leg.LegType = GssConstants.SeaLegType;
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "9308390";
			vessel.RV_Name = "Santa MariaZZ";

			leg.LloydsNumber = "9308390";
			leg.VesselName = "TmpName";
			AssertHasWarning(leg.VesselNameInfo, "Vessel name of the vessel found by IMO number differs from the one provided by schedule service. Use right click popup menu to create vessel with Vessel Name from schedule service.");
		}

		public void TestValidateVesselName_LloydsNumbersAreDifferent()
		{
			var leg = new Leg(Factory);
			leg.LegType = GssConstants.SeaLegType;
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "9308390";
			vessel.RV_Name = "Santa MariaZZ";

			leg.LloydsNumber = "9463085";
			leg.VesselName = "Santa MariaZZ";
			AssertHasError(leg.LloydsNumberInfo, "IMO Number of vessel found by vessel name differs from the one provided by schedule service. Use right click popup menu to update existing vessel with IMO from schedule service.");
		}

		public void TestValidateLloydsNumber_EmptyValue()
		{
			var leg = new Leg(Factory);
			leg.LegType = GssConstants.SeaLegType;
			var validation = new LegValidation(leg);

			validation.ValidateLloydsNumber();
			AssertHasError(leg.LloydsNumberInfo, "IMO Number cannot be empty for sea legs.");

			leg.LloydsNumber = "9308390";
			AssertNoErrors(leg.LloydsNumberInfo);
		}

		public void ValidateLloydsNumber_NoValidationError_When_LegTypeIsNonSea()
		{
			var leg = new Leg(Factory);
			leg.LegType = GssConstants.RoadLegType;
			var validation = new LegValidation(leg);

			validation.ValidateLloydsNumber();
			AssertNoError(leg.LloydsNumberInfo, "IMO Number cannot be empty for sea legs.");
		}

		public void TestValidateLloydsNumber_LloydsNumberLengthIsWrong()
		{
			var leg = new Leg(Factory);
			leg.LegType = GssConstants.SeaLegType;
			leg.LloydsNumber = "12345678";
			AssertHasWarning(leg.LloydsNumberInfo, "IMO number must be 7 characters in length");
		}

		public void TestValidateLloydsNumber_LloydsNumberInvalidCheckDigit()
		{
			var leg = new Leg(Factory);
			leg.LegType = GssConstants.SeaLegType;
			leg.LloydsNumber = "1234560";
			AssertHasWarning(leg.LloydsNumberInfo, "Invalid check-digit in IMO number");
		}

		public void TestValidateLloydsNumber_LloydsNumberIsNotNumeric()
		{
			var leg = new Leg(Factory);
			leg.LegType = GssConstants.SeaLegType;
			leg.LloydsNumber = "ABC4567";
			AssertHasWarning(leg.LloydsNumberInfo, "IMO number must be completely numeric (nothing but numbers)");
		}

		public void TestValidateLloydsNumber_MoreThanOneVesselFoundByLloydsNumber()
		{
			var leg = new Leg(Factory);
			leg.LegType = GssConstants.SeaLegType;
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_LloydsNumber = "9308390";

			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_LloydsNumber = "9308390";

			leg.LloydsNumber = "9308390";
			AssertHasError(leg.LloydsNumberInfo, "More than one vessel found with IMO Number 9308390.");

			leg.LloydsNumber = "9463085";
			AssertNoErrors(leg.LloydsNumberInfo);
		}

		public void TestValidateLloydsNumber_LloydsNumbersAreDifferent()
		{
			var leg = new Leg(Factory);
			leg.LegType = GssConstants.SeaLegType;
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "9308390";
			vessel.RV_Name = "Santa MariaZZ";

			leg.VesselName = "Santa MariaZZ";
			leg.LloydsNumber = "9463085";
			AssertHasError(leg.LloydsNumberInfo, "IMO Number of vessel found by vessel name differs from the one provided by schedule service. Use right click popup menu to update existing vessel with IMO from schedule service.");
		}

		public void TestValidateVoyageCode_ValidationError_WhenLegTypeIsSeaAndEmpty()
		{
			var leg = new Leg(Factory);
			leg.LegType = GssConstants.SeaLegType;
			var validation = new LegValidation(leg);

			validation.ValidateVoyageCode();

			AssertHasError(leg.VoyageCodeInfo, "Voyage Code cannot be empty for sea legs.");
		}

		public void TestValidateVoyageCode_ValidationError_WhenLegTypeIsSeaAndTooLong()
		{
			var leg = new Leg(Factory);
			leg.LegType = GssConstants.SeaLegType;

			leg.VoyageCode = "12345678901";

			AssertHasError(leg.VoyageCodeInfo, "Maximum length of Voyage Code has been exceeded.");
		}

		public void TestValidateVoyageCode_NoValidationError_WhenLegTypeIsSeaAndNotTooLong()
		{
			var leg = new Leg(Factory);
			leg.LegType = GssConstants.SeaLegType;

			leg.VoyageCode = "1234567890";

			AssertNoErrors(leg.VoyageCodeInfo);
		}

		public void TestValidateVoyageCode_NoValidationError_When_LegTypeIsNonSeaAndEmpty()
		{
			var leg = new Leg(Factory);
			leg.LegType = GssConstants.RailLegType;
			var validation = new LegValidation(leg);

			validation.ValidateVoyageCode();

			AssertNoErrors(leg.VoyageCodeInfo);
		}

		public void TestValidateVoyageCode_NoValidationError_When_LegTypeIsNonSeaAndNotTooLong()
		{
			var leg = new Leg(Factory);
			leg.LegType = GssConstants.RailLegType;

			leg.VoyageCode = "123456789";

			AssertNoErrors(leg.VoyageCodeInfo);
		}

		public void TestValidateVoyageCode_ValidationError_When_LegTypeIsNonSeaAndTooLong()
		{
			var leg = new Leg(Factory);
			leg.LegType = GssConstants.RailLegType;

			leg.VoyageCode = "12345678901";

			AssertHasError(leg.VoyageCodeInfo, "Maximum length of Voyage Code has been exceeded.");
		}

		public void TestValidateDeparture()
		{
			var leg = new Leg(Factory);
			leg.LegType = GssConstants.SeaLegType;
			var validation = new LegValidation(leg);

			validation.ValidateDeparture();
			AssertHasNotifications("Departure cannot be empty for sea legs.", leg.DepartureInfo);

			leg.Departure = DateTime.Today;
			AssertNoNotifications("Departure cannot be empty for sea legs.", leg.DepartureInfo);
		}

		public void TestValidateArrival()
		{
			var leg = new Leg(Factory);
			var validation = new LegValidation(leg);

			validation.ValidateArrival();
			AssertNoNotifications("Arrival cannot be empty for sea legs.", leg.ArrivalInfo);

			leg.Arrival = DateTime.Today;
			AssertNoNotifications("Arrival cannot be empty for sea legs.", leg.ArrivalInfo);
		}

		public void TestValidateOriginPortUnloco()
		{
			var leg = new Leg(Factory);
			var validation = new LegValidation(leg);

			validation.ValidateOriginPortUnloco();
			AssertHasNotifications("Please enter a value.", leg.OriginPortUnlocoInfo);

			leg.OriginPortUnloco = "AAAAA";
			AssertHasNotifications("Enter a valid selection.", leg.OriginPortUnlocoInfo);

			leg.OriginPortUnloco = "AUSYD";
			AssertNoNotifications(leg.OriginPortUnlocoInfo);
		}

		public void TestValidateDestinationPortUnloco()
		{
			var leg = new Leg(Factory);
			var validation = new LegValidation(leg);

			validation.ValidateDestinationPortUnloco();
			AssertHasNotifications("Please enter a value.", leg.DestinationPortUnlocoInfo);

			leg.DestinationPortUnloco = "AAAAA";
			AssertHasNotifications("Enter a valid selection.", leg.DestinationPortUnlocoInfo);

			leg.DestinationPortUnloco = "AUSYD";
			AssertNoNotifications(leg.DestinationPortUnlocoInfo);
		}

		public void TestValidateCarrierSCAC()
		{
			var leg = new Leg(Factory);
			leg.CarrierSCAC = "";

			AssertHasNotifications("Carrier SCAC must not be empty.", leg.CarrierSCACInfo);

			leg = new Leg(Factory);
			leg.CarrierSCAC = "ABCDE";

			AssertHasError(leg.CarrierSCACInfo, "No organization found for SCAC ABCDE.");
			AssertHasError(leg.CarrierSCACInfo, "SCAC code must consist of 4 symbols.");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "OrgCode";
			var orgCusCode = Factory.NewWithValidTestData<OrgCusCode>();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			orgCusCode.OK_CustomsRegNo = "ABCD";
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			orgCusCode.OK_OH = org.PK;
			Factory.Save();

			leg = new Leg(Factory);
			leg.CarrierSCAC = "ABCD";

			AssertNoErrors(leg.CarrierSCACInfo);

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "OrgCode2";
			var orgCusCode2 = Factory.NewWithValidTestData<OrgCusCode>();
			orgCusCode2.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			orgCusCode2.OK_CustomsRegNo = "ABCD";
			orgCusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
			orgCusCode2.OK_OH = org2.PK;
			Factory.Save();

			leg = new Leg(Factory);
			leg.CarrierSCAC = "ABCD";

			AssertHasError(leg.CarrierSCACInfo, "Multiple organizations with SCAC ABCD found.");
		}

		public void TestValidateCarrierSCAC_FiltersOutInactiveOrganisations_SingleOrg()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsActive = true;
			org.OH_Code = "any";

			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode.OK_CustomsRegNo = "ABCD";
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			Factory.Save();

			var route = new RouteTestHelper(Factory).CreateRoute(carrierName: "ABCD Carrier", voyageNumber: "1", vesselName: "2", loadPort: "AUSYD", dischargePort: "AUBNE");
			AssertNoErrors(route);

			org.OH_IsActive = false;

			Factory.Save();

			route = new RouteTestHelper(Factory).CreateRoute(carrierName: "ABCD Carrier", voyageNumber: "1", vesselName: "2", loadPort: "AUSYD", dischargePort: "AUBNE");
			AssertHasError(route.CarrierSCACInfo, "No organization found for SCAC ABCD.");
		}

		public void TestValidateCarrierSCAC_FiltersOutInactiveOrganisations_MultipleOrgs()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsActive = true;
			org1.OH_Code = "any1";

			var cusCode1 = org1.CustomsCodes.AddNew();
			cusCode1.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode1.OK_CustomsRegNo = "ABCD";
			cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_IsActive = true;
			org2.OH_Code = "any2";

			var cusCode2 = org2.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			cusCode2.OK_CustomsRegNo = "ABCD";
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;

			Factory.Save();

			var route = new RouteTestHelper(Factory).CreateRoute(carrierName: "ABCD Carrier", voyageNumber: "1", vesselName: "2", loadPort: "AUSYD", dischargePort: "AUBNE");
			AssertHasError(route.CarrierSCACInfo, "Multiple organizations with SCAC ABCD found.");

			org2.OH_IsActive = false;

			Factory.Save();

			route = new RouteTestHelper(Factory).CreateRoute(carrierName: "ABCD Carrier", voyageNumber: "1", vesselName: "2", loadPort: "AUSYD", dischargePort: "AUBNE");
			AssertNoErrors(route);
		}

		public void TestValidateClonedLeg()
		{
			var vessel1 = Factory.NewWithValidTestData<RefVessel>();
			vessel1.RV_LloydsNumber = "9308390";

			var vessel2 = Factory.NewWithValidTestData<RefVessel>();
			vessel2.RV_LloydsNumber = "9308390";

			var leg = new Leg(Factory);
			leg.LegType = GssConstants.SeaLegType;
			leg.LloydsNumber = "9308390";
			leg.VesselName = "Santa Maria";

			var clone = leg.Clone(Factory);
			clone.Validation.ValidateLloydsNumber();
			AssertHasError(clone.LloydsNumberInfo, "More than one vessel found with IMO Number 9308390.");
		}
	}
}
