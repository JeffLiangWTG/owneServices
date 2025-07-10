using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FormalImportAddInfoBillValidationTest : CommonImportAddInfoBillValidationTest
	{
		public void TestIsEntrySummaryOrCargoReleaseValidationMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = false;
			var bill = declaration.Bills.AddNew();
			var addInfo = new AddInfoBill(bill.CU_AddInfoInfo);
			var validation = (FormalImportAddInfoBillValidation)addInfo.Validation;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", false, validation.IsEntrySummaryOrCargoReleaseValidationMode);
			declaration.US_EnableENS = true;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", true, validation.IsEntrySummaryOrCargoReleaseValidationMode);
			declaration.US_EnableCRL = true;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", true, validation.IsEntrySummaryOrCargoReleaseValidationMode);
			declaration.US_EnableENS = false;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", true, validation.IsEntrySummaryOrCargoReleaseValidationMode);
			declaration.US_EnableCRL = false;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", false, validation.IsEntrySummaryOrCargoReleaseValidationMode);
			declaration.US_CertifyCargoRelease = false;
			AssertEquals("IsEntrySummaryOrCargoReleaseValidationMode", false, validation.IsEntrySummaryOrCargoReleaseValidationMode);
		}

		public void TestUS_VolumeUQ()
		{
			AssertUS_VolumeUQ(bill);
		}

		public void TestSplitForParentBill()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_MasterBill = "OBL1";
			declaration.JE_HouseBill = "HB1";
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.PrimaryMasterBill.US_SESplitShip = true;
			AssertHasMessageError(declaration.PrimaryMasterBill.US_SESplitShipInfo, FormalImportAddInfoBillValidation.SplitForParentBill);
			declaration.PrimaryMasterBill.US_SESplitShip = false;
			AssertNoMessageError(declaration.PrimaryMasterBill.US_SESplitShipInfo, FormalImportAddInfoBillValidation.SplitForParentBill);
			declaration.PrimaryHouseBill.US_SESplitShip = true;
			AssertNoMessageError(declaration.PrimaryHouseBill.US_SESplitShipInfo, FormalImportAddInfoBillValidation.SplitForParentBill);
			declaration.PrimaryHouseBill.US_SESplitShip = false;
			AssertNoMessageError(declaration.PrimaryHouseBill.US_SESplitShipInfo, FormalImportAddInfoBillValidation.SplitForParentBill);
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.PrimaryMasterBill.US_SESplitShip = true;
			AssertNoMessageError(declaration.PrimaryMasterBill.US_SESplitShipInfo, FormalImportAddInfoBillValidation.SplitForParentBill);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_NonAMS = true;
			declaration.PrimaryMasterBill.US_SESplitShip = false;
			AssertNoMessageErrorContaining(declaration.PrimaryMasterBill.US_SESplitShipInfo, FormalImportAddInfoBillValidation.NonAMSBillCannotBeSplit);
			declaration.PrimaryMasterBill.US_SESplitShip = true;
			AssertHasMessageErrorContaining(declaration.PrimaryMasterBill.US_SESplitShipInfo, FormalImportAddInfoBillValidation.NonAMSBillCannotBeSplit);
			declaration.US_NonAMS = false;
			declaration.PrimaryMasterBill.US_SESplitShip = false;
			declaration.PrimaryMasterBill.US_SESplitShip = true;
			AssertNoMessageErrorContaining(declaration.PrimaryMasterBill.US_SESplitShipInfo, FormalImportAddInfoBillValidation.NonAMSBillCannotBeSplit);
		}

		public void TestCheckCU_VolumeAndUQ()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Bill bill = declaration.Bills.AddNew();
			bill.US_VolumeUQ = Core.Constants.Weight.Grams;
			AssertHasMessageErrorContaining(bill.US_VolumeUQInfo, ListValidation.InvalidCodeMessageError);
			bill.US_VolumeUQ = Core.Constants.Volume.CubicMetres;
			AssertNoMessageErrorContaining(bill.US_VolumeUQInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_UI_NKBillIssuerSCAC()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ValidationModes = ValidationModes.EntrySummary;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.AddInfoValidation.ValidateUS_UI_NKBillIssuerSCAC();
			AssertNoMessageError(bill.US_UI_NKBillIssuerSCACInfo, FormalImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			bill.AddInfoValidation.ValidateUS_UI_NKBillIssuerSCAC();
			AssertHasMessageError(bill.US_UI_NKBillIssuerSCACInfo, FormalImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);
			bill.US_UI_NKBillIssuerSCAC = "DDDF";
			AssertNoMessageError(bill.US_UI_NKBillIssuerSCACInfo, FormalImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);
			AssertHasMessageErrorContaining(bill.CU_BillNumInfo, MandatoryValidation.YouHaveNotEntered);
			bill.US_UI_NKBillIssuerSCAC = "~";
			AssertHasMessageError(bill.US_UI_NKBillIssuerSCACInfo, IssuerCarrierSCACValidator.InvalidSCACFormat);
			bill.US_UI_NKBillIssuerSCAC = "~F";
			AssertNoMessageError(bill.US_UI_NKBillIssuerSCACInfo, IssuerCarrierSCACValidator.InvalidSCACFormat);
			AssertHasMessageErrorContaining(bill.US_UI_NKBillIssuerSCACInfo, "Issuer" + IssuerCarrierSCACValidator.InvalidSCAC);
			bill.US_UI_NKBillIssuerSCAC = "";
			AssertNoMessageErrorContaining(bill.US_UI_NKBillIssuerSCACInfo, "Issuer" + IssuerCarrierSCACValidator.InvalidSCAC);
			var usCarrier = Factory.LoadTop1<USCarrierCombined>(new ZQuery());
			declaration.JE_TransportMode = TransportTypeList.ConvertFromTransportCode(usCarrier.UI_ModeOfTransportation);
			bill.US_UI_NKBillIssuerSCAC = usCarrier.UI_Code;
			AssertNoMessageError("Issuer SCAC should not have Message Error because Mode of transportation corresponding Value is Declaration transport mode.", bill.US_UI_NKBillIssuerSCACInfo, "Issuer" + IssuerCarrierSCACValidator.InvalidSCAC);
			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			bill.US_UI_NKBillIssuerSCAC = "";
			bill.AddInfoValidation.ValidateUS_UI_NKBillIssuerSCAC();
			AssertNoMessageErrorContaining(bill.US_UI_NKBillIssuerSCACInfo, FormalImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);
			bill.CU_BillNum = "HB1";
			bill.US_UI_NKBillIssuerSCAC = "AAAD";
			AssertNoMessageErrorContaining(bill.CU_BillNumInfo, MandatoryValidation.YouHaveNotEntered);
			bill.CU_BillNum = ZString.Empty;
			bill.US_UI_NKBillIssuerSCAC = "AAAD";
			AssertNoMessageErrorContaining(bill.CU_BillNumInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			bill.US_UI_NKBillIssuerSCAC = "";
			bill.AddInfoValidation.ValidateUS_UI_NKBillIssuerSCAC();
			AssertNoMessageErrorContaining(bill.US_UI_NKBillIssuerSCACInfo, FormalImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			bill.AddInfoValidation.ValidateUS_UI_NKBillIssuerSCAC();
			AssertHasMessageError(bill.US_UI_NKBillIssuerSCACInfo, FormalImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill.AddInfoValidation.ValidateUS_UI_NKBillIssuerSCAC();
			AssertNoMessageError(bill.US_UI_NKBillIssuerSCACInfo, FormalImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);
			bill.CU_BillNum = ZString.Empty;
			bill.US_UI_NKBillIssuerSCAC = "AAAD";
			AssertNoMessageErrorContaining(bill.CU_BillNumInfo, MandatoryValidation.YouHaveNotEntered);
			bill.CU_BillNum = "HB1";
			bill.US_UI_NKBillIssuerSCAC = "AAAD";
			AssertNoMessageErrorContaining(bill.CU_BillNumInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.Auto;
			bill.AddInfoValidation.ValidateUS_UI_NKBillIssuerSCAC();
			AssertHasMessageError(bill.US_UI_NKBillIssuerSCACInfo, FormalImportAddInfoBillValidation.IssuerSCACiSNotPermitted);
			declaration.JE_TransportMode = TransportTypeList.Codes.Pedestrian;
			bill.AddInfoValidation.ValidateUS_UI_NKBillIssuerSCAC();
			AssertHasMessageError(bill.US_UI_NKBillIssuerSCACInfo, FormalImportAddInfoBillValidation.IssuerSCACiSNotPermitted);
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			bill.AddInfoValidation.ValidateUS_UI_NKBillIssuerSCAC();
			AssertHasMessageError(bill.US_UI_NKBillIssuerSCACInfo, FormalImportAddInfoBillValidation.IssuerSCACiSNotPermitted);
			declaration.JE_TransportMode = TransportTypeList.Codes.PassengerHandCarried;
			bill.AddInfoValidation.ValidateUS_UI_NKBillIssuerSCAC();
			AssertHasMessageError(bill.US_UI_NKBillIssuerSCACInfo, FormalImportAddInfoBillValidation.IssuerSCACiSNotPermitted);
			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			bill.AddInfoValidation.ValidateUS_UI_NKBillIssuerSCAC();
			AssertNoMessageError(bill.US_UI_NKBillIssuerSCACInfo, FormalImportAddInfoBillValidation.IssuerSCACiSNotPermitted);
		}

		public void TestCheckUS_ExpressTracking()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_EnableCRL = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_MasterBill = "TEST111111";
			var newBill = declaration.Bills.AddNew();
			newBill.CU_BillType = SEBillTypesList.Codes.MasterBill;
			newBill.CU_BillNum = "TEST122222";
			newBill.US_ExpressTracking = true;
			AssertNoMessageErrorContaining(newBill.US_ExpressTrackingInfo, FormalImportAddInfoBillValidation.OnlyOneExpressTrackingNumberAllowed);
			declaration.JE_MasterBillExpressTracking = true;
			newBill.AddInfoValidation.ValidateUS_ExpressTracking();
			AssertHasMessageErrorContaining(newBill.US_ExpressTrackingInfo, FormalImportAddInfoBillValidation.OnlyOneExpressTrackingNumberAllowed);
			newBill.US_ExpressTracking = false;
			AssertNoMessageErrorContaining(newBill.US_ExpressTrackingInfo, FormalImportAddInfoBillValidation.OnlyOneExpressTrackingNumberAllowed);
			declaration.JE_MasterBillExpressTracking = false;
			declaration.US_EnableENS = true;
			AssertNoMessageErrorContaining(declaration.JE_MasterBillExpressTrackingInfo, FormalImportAddInfoBillValidation.ExpressTrackingNotAllowedForENS);
			declaration.JE_MasterBillExpressTracking = true;
			AssertHasMessageErrorContaining(declaration.JE_MasterBillExpressTrackingInfo, FormalImportAddInfoBillValidation.ExpressTrackingNotAllowedForENS);
			newBill.US_ExpressTracking = true;
			AssertHasMessageErrorContaining(newBill.US_ExpressTrackingInfo, FormalImportAddInfoBillValidation.ExpressTrackingNotAllowedForENS);
		}

		public void TestCheckUS_UI_NKBillIssuerSCACWhenBOLEntered()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.ValidationModes = ValidationModes.EntrySummary;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.JE_MasterBill = "342890342";
			AssertNotNull(declaration.PrimaryMasterBill);
			declaration.PrimaryMasterBill.US_UI_NKBillIssuerSCAC = ZString.Empty;
			AssertHasMessageError(declaration.PrimaryMasterBill.US_UI_NKBillIssuerSCACInfo, CommonImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);
			declaration.PrimaryMasterBill.CU_BillNum = ZString.Empty;
			declaration.PrimaryMasterBill.US_UI_NKBillIssuerSCAC = ZString.Empty;
			//should still be a problem
			AssertHasMessageError(declaration.PrimaryMasterBill.US_UI_NKBillIssuerSCACInfo, CommonImportAddInfoBillValidation.SCACCodeForMasterAndHouseBillRequiredIfSeaOrRail);
		}

		JobDeclaration declaration;
		Bill bill;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableINB = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "1";
			bill.US_AMSCarrierIndicator = YesNoDefaultList.Codes.No;
		}
	}
}
