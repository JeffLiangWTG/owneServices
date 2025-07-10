using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	class RefEquipmentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateConveyance()
		{
			var messageError = @"Conveyance is invalid and has following errors:
You have not entered a Conveyance Type;
You have not entered a Vehicle Identification Number (VIN);
You have not entered a License Plate Number;
You have not entered a State/Province of Registration;
You have not entered a Country of Registration.";
			const string warning = @"Conveyance has the following warnings:
The length of data entered into Conveyance ACE ID (11) exceeds the maximum allowed. Only the first 10 characters will be transmitted to Customs;
The length of data entered into Conveyance ID (24) exceeds the maximum allowed. Only the first 23 characters will be transmitted to Customs.";
			var conveyance = trip.Conveyance;
			conveyance.BJ_RQ_Equipment = refEquipment.PK;
			AssertHasMessageError("Mandatory data not entered", conveyance.BJ_RQ_EquipmentInfo, messageError);
			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.RC_Code = "TST";
			refContainer.SetCountrySpecificContainerCode(ConveyanceTypes.Codes.PickupTruck, Enterprise.Core.Constants.CountryCodes.UnitedStates);
			refEquipment.RQ_RC_RoadContainerType = refContainer.PK;
			SetRefNumber(refEquipment, ConveyanceReferences.Codes.ACEId, 11);
			conveyance.BJ_ACEID = new ZString().PadRight(11, '1');
			conveyance.Validation.ValidateBJ_RQ_Equipment();
			AssertNoMessageError("ACE id is entered", conveyance.BJ_RQ_EquipmentInfo, messageError);
			refContainer.SetCountrySpecificContainerCode(EquipmentTypes.Codes.Container20FtSeaClosedTop, Enterprise.Core.Constants.CountryCodes.UnitedStates);
			refEquipment.RQ_VIN = "1234567890";
			refEquipment.RQ_Registration = "BBDD11";
			refEquipment.RQ_RegState = USStatesList.Codes.Illinois;
			refEquipment.RQ_RN_NKRegistrationCountry = branch.Country.Code;
			refEquipment.RQ_GateTransponder1 = "789543218";
			SetRefNumber(refEquipment, ConveyanceReferences.Codes.CarrierId, 24);
			messageError = @"Conveyance is invalid and has following errors:
The equipment you have selected has an invalid e-Manifest Conveyance Type.
Please go to Reference Files -> Containers and specify proper US Container Code against container with code 'TST'";
			conveyance.Validation.ValidateBJ_RQ_Equipment();
			AssertHasMessageErrorContaining(conveyance.BJ_RQ_EquipmentInfo, messageError);
			refContainer.SetCountrySpecificContainerCode(ZString.Empty, Enterprise.Core.Constants.CountryCodes.UnitedStates);
			messageError = @"Conveyance is invalid and has following errors:
The equipment you have selected is missing e-Manifest Conveyance Type.
Please go to Reference Files -> Containers and specify proper US Container Code against container with code 'TST'";
			conveyance.Validation.ValidateBJ_RQ_Equipment();
			AssertHasMessageErrorContaining(conveyance.BJ_RQ_EquipmentInfo, messageError);
			refContainer.SetCountrySpecificContainerCode(ConveyanceTypes.Codes.PickupTruck, Enterprise.Core.Constants.CountryCodes.UnitedStates);
			conveyance.Validation.ValidateBJ_RQ_Equipment();
			AssertNoMessageErrorContaining(conveyance.BJ_RQ_EquipmentInfo, messageError);
			AssertHasWarningContaining(conveyance.BJ_RQ_EquipmentInfo, warning);
			SetRefNumber(refEquipment, ConveyanceReferences.Codes.CarrierId, 23);
			SetRefNumber(refEquipment, ConveyanceReferences.Codes.ACEId, 10);
			conveyance.Validation.ValidateBJ_RQ_Equipment();
			AssertNoWarningContaining(conveyance.BJ_RQ_EquipmentInfo, warning);
		}

		public void TestValidateEquipmentWhenLicensePlatesRequired()
		{
			var messageError = @"Equipment is invalid and has following errors:
You have not entered an Equipment Type.";
			const string warning = @"Equipment has the following warnings:
The length of data entered into Equipment ACE ID (11) exceeds the maximum allowed. Only the first 10 characters will be transmitted to Customs.";
			refEquipment.RQ_IsVehicle = true;
			var equipment = trip.Equipment.AddNew();
			equipment.BJ_RQ_Equipment = refEquipment.PK;
			AssertHasMessageError("Mandatory data not entered", equipment.BJ_RQ_EquipmentInfo, messageError);
			messageError = @"Equipment is invalid and has following errors:
The equipment you have selected has an invalid e-Manifest Equipment Type.
Please go to Reference Files -> Containers and specify proper US Container Code against container with code 'TST'";
			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.SetCountrySpecificContainerCode(ConveyanceTypes.Codes.ArmoredTruck, Enterprise.Core.Constants.CountryCodes.UnitedStates);
			refContainer.RC_Code = "TST";
			refEquipment.RQ_RC_RoadContainerType = refContainer.PK;
			equipment.Validation.ValidateBJ_RQ_Equipment();
			AssertHasMessageErrorContaining(equipment.BJ_RQ_EquipmentInfo, messageError);
			refContainer.SetCountrySpecificContainerCode(ZString.Empty, Enterprise.Core.Constants.CountryCodes.UnitedStates);
			messageError = @"Equipment is invalid and has following errors:
The equipment you have selected is missing e-Manifest Equipment Type.
Please go to Reference Files -> Containers and specify proper US Container Code against container with code 'TST'";
			equipment.Validation.ValidateBJ_RQ_Equipment();
			AssertHasMessageErrorContaining(equipment.BJ_RQ_EquipmentInfo, messageError);
			messageError = @"Equipment is invalid and has following errors:
You have not entered a License Plate Number;
You have not entered a State/Province of Registration;
You have not entered a Country of Registration.";
			refContainer.SetCountrySpecificContainerCode(EquipmentTypes.Codes.LivestockTrailer, Enterprise.Core.Constants.CountryCodes.UnitedStates);
			equipment.Validation.ValidateBJ_RQ_Equipment();
			AssertHasMessageErrorContaining(equipment.BJ_RQ_EquipmentInfo, messageError);
			SetRefNumber(refEquipment, ConveyanceReferences.Codes.ACEId, 10);
			equipment.Validation.ValidateBJ_RQ_Equipment();
			AssertHasMessageErrorContaining(equipment.BJ_RQ_EquipmentInfo, messageError);
			refEquipment.Certificates.DeleteAll();
			refEquipment.RQ_Registration = "BA12YY";
			refEquipment.RQ_RegState = USStatesList.Codes.Illinois;
			refEquipment.RQ_RN_NKRegistrationCountry = branch.Country.Code;
			equipment.BJ_ACEID = new ZString().PadRight(11, '1');
			equipment.Validation.ValidateBJ_RQ_Equipment();
			AssertNoMessageErrorContaining(equipment.BJ_RQ_EquipmentInfo, messageError);
			AssertHasWarning("Ref numbers too long warning", equipment.BJ_RQ_EquipmentInfo, warning);
			equipment.BJ_ACEID = new ZString().PadRight(10, '1');
			equipment.Validation.ValidateBJ_RQ_Equipment();
			AssertNoWarning("Ref numbers are valid", equipment.BJ_RQ_EquipmentInfo, warning);
		}

		public void TestValidateEquipmentWhetLicensePlatesNotRequired()
		{
			var messageError = @"Equipment is invalid and has following errors:
You have not entered an Equipment Type.";
			const string warning = @"Equipment has the following warnings:
The length of data entered into Equipment ACE ID (11) exceeds the maximum allowed. Only the first 10 characters will be transmitted to Customs.";
			refEquipment.RQ_IsVehicle = false;
			var equipment = trip.Equipment.AddNew();
			equipment.BJ_RQ_Equipment = refEquipment.PK;
			AssertHasMessageError("Mandatory data not entered", equipment.BJ_RQ_EquipmentInfo, messageError);
			messageError = @"Equipment is invalid and has following errors:
You have not entered an Equipment Number.";
			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.SetCountrySpecificContainerCode(EquipmentTypes.Codes.Container20FtSeaClosedTop, Enterprise.Core.Constants.CountryCodes.UnitedStates);
			refEquipment.RQ_RC_RoadContainerType = refContainer.PK;
			equipment.Validation.ValidateBJ_RQ_Equipment();
			AssertHasMessageError("Mandatory data not entered", equipment.BJ_RQ_EquipmentInfo, messageError);
			refEquipment.RQ_Registration = "68465464";
			equipment.BJ_ACEID = new ZString().PadRight(11, '1');
			equipment.Validation.ValidateBJ_RQ_Equipment();
			AssertNoMessageError("Mandatory data entered", equipment.BJ_RQ_EquipmentInfo, messageError);
			AssertHasWarning("Ref numbers too long warning", equipment.BJ_RQ_EquipmentInfo, warning);
			equipment.BJ_ACEID = new ZString().PadRight(10, '1');
			equipment.Validation.ValidateBJ_RQ_Equipment();
			AssertNoWarning("Ref numbers are valid", equipment.BJ_RQ_EquipmentInfo, warning);
		}

		protected override void SetUp()
		{
			base.SetUp();
			trip = Factory.New<Trip>();
			refEquipment = Factory.New<RefEquipment>();
			branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "USLAX";
		}

		GlbBranch branch;
		RefEquipment refEquipment;
		Trip trip;

		static void SetRefNumber(RefEquipment refEquipment, string type, int length)
		{
			var cert = refEquipment.Certificates.FirstOrDefault(c => c.XZ_Type == type) ?? refEquipment.Certificates.AddNew();
			cert.XZ_Type = type;
			cert.XZ_RefNumber = new ZString().PadRight(length, '1');
		}
	}
}
