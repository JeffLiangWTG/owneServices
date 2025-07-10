using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class CusInBondHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAtLeastOneBillExists()
		{
			header.ValidationModes = ValidationModes.InventoryRecord;
			var bill = header.Bills.AddNew();
			header.Validation.ValidateAll();
			AssertNoRowMessageError(header, ValidationConstants.Header.AtLeastOneBillExists);

			bill.Delete();
			header.Validation.ValidateAll();
			AssertHasRowMessageError(header, ValidationConstants.Header.AtLeastOneBillExists);

			header.ValidationModes = ValidationModes.None;
			header.Validation.ValidateAll();
			AssertNoRowMessageError(header, ValidationConstants.Header.AtLeastOneBillExists);
		}

		public void TestCheckBH_VoyageNumber()
		{
			header.ValidationModes = ValidationModes.InventoryRecord;
			header.BH_VoyageNumber = ZString.Empty;
			AssertHasMessageErrorContaining(header.BH_VoyageNumberInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_VoyageNumber = "VDD";
			AssertNoMessageErrorContaining(header.BH_VoyageNumberInfo, MandatoryValidation.YouHaveNotEntered);

			header.ValidationModes = ValidationModes.None;
			header.BH_VoyageNumber = ZString.Empty;
			AssertHasMessageErrorContaining(header.BH_VoyageNumberInfo, MandatoryValidation.YouHaveNotEntered);

			header.ValidationModes = ValidationModes.InventoryRecord;
			header.BH_VoyageNumber = "VDD456";
			AssertHasWarningContaining(header.BH_VoyageNumberInfo, ValidationConstants.Header.VoyageNumberLengthExceeded.ToString());

			header.BH_VoyageNumber = "VDD45";
			AssertNoWarningContaining(header.BH_VoyageNumberInfo, ValidationConstants.Header.VoyageNumberLengthExceeded.ToString());
		}

		public void TestCheckBH_ETA()
		{
			header.ValidationModes = ValidationModes.InventoryRecord;
			header.BH_ETA = ZDateTime.Empty;
			AssertHasMessageErrorContaining(header.BH_ETAInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_ETA = ZDateTime.Now;
			AssertNoMessageErrorContaining(header.BH_ETAInfo, MandatoryValidation.YouHaveNotEntered);

			header.ValidationModes = ValidationModes.None;
			header.BH_ETA = ZDateTime.Empty;
			AssertHasMessageErrorContaining(header.BH_ETAInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBH_RL_NKPortUnlading()
		{
			header.ValidationModes = ValidationModes.InventoryRecord;
			header.BH_RL_NKPortUnlading = ZString.Empty;
			AssertNoMessageErrors(header.BH_RL_NKPortUnladingInfo);

			header.BH_RL_NKPortUnlading = "AU!!@";
			AssertNoMessageErrors(header.BH_RL_NKPortUnladingInfo);
			AssertHasWarningContaining(header.BH_RL_NKPortUnladingInfo, ListValidation.InvalidCodeMessage);

			header.BH_RL_NKPortUnlading = "AUSYD";
			AssertNoMessageErrors(header.BH_RL_NKPortUnladingInfo);
			AssertNoWarningContaining(header.BH_RL_NKPortUnladingInfo, ListValidation.InvalidCodeMessage);

			header.ValidationModes = ValidationModes.None;
			header.BH_RL_NKPortUnlading = "AU!!@";
			AssertNoMessageErrors(header.BH_RL_NKPortUnladingInfo);
			AssertHasWarningContaining(header.BH_RL_NKPortUnladingInfo, ListValidation.InvalidCodeMessage);
		}

		public void TestCheckBH_PortUnladingDCode()
		{
			header.ValidationModes = ValidationModes.InventoryRecord;
			header.BH_PortUnladingDCode = ZString.Empty;
			AssertHasMessageErrorContaining(header.BH_PortUnladingDCodeInfo, MandatoryValidation.YouHaveNotEntered);

			header.BH_PortUnladingDCode = "Z!";
			AssertNoMessageErrorContaining(header.BH_PortUnladingDCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(header.BH_PortUnladingDCodeInfo, ListValidation.InvalidCodeMessageError);

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Z1Z3", "Test Name", startDate, endDate);
			newFactory.Save();

			header.BH_PortUnladingDCode = "Z1Z3";
			AssertNoMessageErrorContaining(header.BH_PortUnladingDCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(header.BH_PortUnladingDCodeInfo, ListValidation.InvalidCodeMessageError);

			header.ValidationModes = ValidationModes.None;
			header.BH_PortUnladingDCode = "Z!";
			AssertNoMessageErrorContaining(header.BH_PortUnladingDCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(header.BH_PortUnladingDCodeInfo, ListValidation.InvalidCodeMessageError);

			header.BH_PortUnladingDCode = ZString.Empty;
			AssertHasMessageErrorContaining(header.BH_PortUnladingDCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(header.BH_PortUnladingDCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBH_ImportConveyanceName()
		{
			header.ValidationModes = ValidationModes.InventoryRecord;
			header.BH_LloydsNumber = ZString.Empty;
			header.BH_ImportConveyanceName = ZString.Empty;
			AssertHasMessageError(header.BH_ImportConveyanceNameInfo, ValidationConstants.Header.ImportingConveyanceRequiresWhenLloydsNumberIsEmpty.ToString());
			header.BH_LloydsNumber = "39234";
			AssertNoMessageError(header.BH_ImportConveyanceNameInfo, ValidationConstants.Header.ImportingConveyanceRequiresWhenLloydsNumberIsEmpty.ToString());
			header.BH_ImportConveyanceName = "TOWER BRIDGE";
			AssertNoMessageError(header.BH_ImportConveyanceNameInfo, ValidationConstants.Header.ImportingConveyanceRequiresWhenLloydsNumberIsEmpty.ToString());
			AssertHasWarningContaining(header.BH_ImportConveyanceNameInfo, ValidationConstants.Header.VesselNameNotBeSentOnlyLloydsNumberBeSent.ToString());
			header.BH_LloydsNumber = ZString.Empty;
			AssertNoMessageError(header.BH_ImportConveyanceNameInfo, ValidationConstants.Header.ImportingConveyanceRequiresWhenLloydsNumberIsEmpty.ToString());
			AssertNoWarningContaining(header.BH_ImportConveyanceNameInfo, ValidationConstants.Header.VesselNameNotBeSentOnlyLloydsNumberBeSent.ToString());
			header.BH_ImportConveyanceName = "GAC BP MIDDLE EAST AND AFRICA HUB";
			AssertHasWarningContaining(header.BH_ImportConveyanceNameInfo, ValidationConstants.Header.VesselNameOnlySendFirst23Characters.ToString());

			header.ValidationModes = ValidationModes.None;
			header.BH_ImportConveyanceName = ZString.Empty;
			AssertHasMessageError(header.BH_ImportConveyanceNameInfo, ValidationConstants.Header.ImportingConveyanceRequiresWhenLloydsNumberIsEmpty.ToString());
		}

		public void TestCheckBH_FIRMS()
		{
			header.ValidationModes = ValidationModes.InventoryRecord;
			header.BH_FIRMS = ZString.Empty;
			AssertNoMessageErrors(header.BH_FIRMSInfo);

			header.BH_FIRMS = "Z!";
			AssertHasMessageErrorContaining(header.BH_FIRMSInfo, ListValidation.InvalidCodeMessageError);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "Z1Z3", "Misaka", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			header.BH_FIRMS = "Z1Z3";
			AssertNoMessageErrorContaining(header.BH_FIRMSInfo, ListValidation.InvalidCodeMessageError);

			header.ValidationModes = ValidationModes.None;
			header.BH_FIRMS = "Z!";
			AssertNoMessageErrorContaining(header.BH_FIRMSInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBH_ImportTransportMode()
		{
			header.ValidationModes = ValidationModes.InventoryRecord;
			header.BH_ImportTransportMode = ZString.Empty;
			AssertHasMessageErrorContaining(header.BH_ImportTransportModeInfo, MandatoryValidation.YouHaveNotEntered);

			header.BH_ImportTransportMode = "Z!";
			AssertNoMessageErrorContaining(header.BH_ImportTransportModeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(header.BH_ImportTransportModeInfo, ListValidation.InvalidCodeMessageError);

			header.BH_ImportTransportMode = TransportTypeList.Codes.VesselContainer;
			AssertNoMessageErrorContaining(header.BH_ImportTransportModeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(header.BH_ImportTransportModeInfo, ListValidation.InvalidCodeMessageError);

			header.ValidationModes = ValidationModes.None;
			header.BH_ImportTransportMode = "Z!";
			AssertNoMessageErrorContaining(header.BH_ImportTransportModeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(header.BH_ImportTransportModeInfo, ListValidation.InvalidCodeMessageError);

			header.BH_ImportTransportMode = ZString.Empty;
			AssertHasMessageErrorContaining(header.BH_ImportTransportModeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(header.BH_ImportTransportModeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBH_CarrierSCAC()
		{
			header.ValidationModes = ValidationModes.InventoryRecord;
			header.BH_CarrierSCAC = ZString.Empty;
			AssertHasMessageErrorContaining(header.BH_CarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);

			header.BH_CarrierSCAC = "Z!";
			AssertNoMessageErrorContaining(header.BH_CarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(header.BH_CarrierSCACInfo, ListValidation.InvalidCodeMessageError);

			var carrier = Factory.New<US.Business.USCarrierCombined>();
			carrier.UI_Code = "Z1Z3";
			header.BH_CarrierSCAC = "Z1Z3";
			AssertNoMessageError(header.BH_CarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(header.BH_CarrierSCACInfo, ListValidation.InvalidCodeMessageError);

			header.ValidationModes = ValidationModes.None;
			header.BH_CarrierSCAC = "Z!";
			AssertNoMessageError(header.BH_CarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(header.BH_CarrierSCACInfo, ListValidation.InvalidCodeMessageError);

			header.BH_CarrierSCAC = ZString.Empty;
			AssertHasMessageErrorContaining(header.BH_CarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(header.BH_CarrierSCACInfo, ListValidation.InvalidCodeMessageError);

			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			header.BH_CarrierSCAC = "TEST";
			AssertHasMessageErrorContaining(header.BH_CarrierSCACInfo, ValidationConstants.Header.SCACOfVesselOperatorShouldBeSubmitted);

			AssertHasMessageErrorContaining(header.BH_CarrierSCACInfo, "Value should be the SCAC code of the NVOCC filing the AMS message");
		}

		public void TestCheckBH_ImportConveyanceCountry()
		{
			header.ValidationModes = ValidationModes.InventoryRecord;
			header.BH_ImportConveyanceCountry = ZString.Empty;
			AssertHasMessageErrorContaining(header.BH_ImportConveyanceCountryInfo, MandatoryValidation.YouHaveNotEntered);

			header.BH_ImportConveyanceCountry = "Z!";
			AssertNoMessageErrorContaining(header.BH_ImportConveyanceCountryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(header.BH_ImportConveyanceCountryInfo, ListValidation.InvalidCodeMessageError);

			header.BH_ImportConveyanceCountry = Core.Constants.CountryCodes.Australia;
			AssertNoMessageErrorContaining(header.BH_ImportConveyanceCountryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(header.BH_ImportConveyanceCountryInfo, ListValidation.InvalidCodeMessageError);

			header.ValidationModes = ValidationModes.None;
			header.BH_ImportConveyanceCountry = "Z!";
			AssertNoMessageErrorContaining(header.BH_ImportConveyanceCountryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(header.BH_ImportConveyanceCountryInfo, ListValidation.InvalidCodeMessageError);

			header.BH_ImportConveyanceCountry = ZString.Empty;
			AssertHasMessageErrorContaining(header.BH_ImportConveyanceCountryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(header.BH_ImportConveyanceCountryInfo, ListValidation.InvalidCodeMessageError);
		}

		protected CusInBondHeader header;
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusInBondHeader>();
		}
	}
}
