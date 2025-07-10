using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FTZAddInfoBillValidationTest : CommonImportAddInfoBillValidationTest
	{
		public void TestCheckUS_VolumeUQ()
		{
			AssertUS_VolumeUQ(bill);
		}

		public void TestUS_UC_NKCountryOfExport()
		{
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			bill.US_UC_NKCountryOfExport = ZString.Empty;
			AssertHasMessageErrorContaining(bill.US_UC_NKCountryOfExportInfo, MandatoryValidation.YouHaveNotEntered);
			bill.US_UC_NKCountryOfExport = "~!";
			AssertHasMessageErrorContaining(bill.US_UC_NKCountryOfExportInfo, ListValidation.InvalidCodeMessageError);
			bill.US_UC_NKCountryOfExport = "AU";
			AssertNoMessageErrors(bill.US_UC_NKCountryOfExportInfo);
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.StatusChange;
			bill.US_UC_NKCountryOfExport = ZString.Empty;
			AssertNoMessageErrors(bill.US_UC_NKCountryOfExportInfo);
		}

		public void TestCheckUS_SchDLoading()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			bill.US_SchDLoading = "#$";
			AssertHasMessageErrorContaining(bill.US_SchDLoadingInfo, ListValidation.InvalidCodeMessageError);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = ContainerModeList.Codes.NonContainerized;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			bill.US_SchDLoading = ZString.Empty;
			AssertHasMessageErrorContaining(bill.US_SchDLoadingInfo, MandatoryValidation.YouHaveNotEntered);
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.StatusChange;
			bill.US_SchDLoading = ZString.Empty;
			AssertNoMessageErrors(bill.US_SchDLoadingInfo);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			bill.US_SchDLoading = ZString.Empty;
			AssertNoMessageErrors(bill.US_SchDLoadingInfo);
		}

		public void TestCheckUS_US_NKLocationOfGoods()
		{
			declaration.US_F_DirectDelivery = true;
			declaration.US_F_IncludePTT = true;
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "98-12345612");
			bill.US_F_OH_PTTCarrier = orgHeader.PK;
			bill.US_US_NKLocationOfGoods = ZString.Empty;
			var errorText = string.Format(ValidationConstants.FTZ.DataRequiredWhenPTTIncluded, "FIRMS Code");
			AssertHasMessageError(bill.US_US_NKLocationOfGoodsInfo, errorText);
			bill.US_US_NKLocationOfGoods = "S002";
			AssertNoMessageError(bill.US_US_NKLocationOfGoodsInfo, errorText);
			declaration.ValidationModes = ValidationModes.FTZPTTValidationMode;
			errorText = string.Format(ValidationConstants.FTZ.DataRequired, "FIRMS Code");
			bill.US_US_NKLocationOfGoods = ZString.Empty;
			AssertHasMessageError(bill.US_US_NKLocationOfGoodsInfo, errorText);
			bill.US_US_NKLocationOfGoods = "S001";
			AssertNoMessageError(bill.US_US_NKLocationOfGoodsInfo, errorText);
		}

		public void TestCheckUS_F_OH_PTTCarrier()
		{
			var orgHeader = Factory.New<OrgHeader>();
			bill.US_F_OH_PTTCarrier = orgHeader.PK;
			var errorText = string.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired, "Carrier");
			AssertHasMessageErrorContaining(bill.US_F_OH_PTTCarrierInfo, errorText);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "123901-1234");
			bill.US_F_OH_PTTCarrier = orgHeader.PK;
			AssertNoMessageErrorContaining(bill.US_F_OH_PTTCarrierInfo, errorText);
			errorText = string.Format(ValidationConstants.FTZ.DataRequiredWhenPTTIncluded, "Carrier");
			declaration.US_F_IncludePTT = true;
			bill.US_F_OH_PTTCarrier = ZGuid.Empty;
			AssertHasMessageError(bill.US_F_OH_PTTCarrierInfo, errorText);
			bill.US_F_OH_PTTCarrier = orgHeader.PK;
			AssertNoMessageError(bill.US_F_OH_PTTCarrierInfo, errorText);
			declaration.ValidationModes = ValidationModes.FTZPTTValidationMode;
			errorText = string.Format(ValidationConstants.FTZ.DataRequired, "Carrier");
			bill.US_F_OH_PTTCarrier = ZGuid.Empty;
			AssertHasMessageError(bill.US_F_OH_PTTCarrierInfo, errorText);
			bill.US_F_OH_PTTCarrier = orgHeader.PK;
			AssertNoMessageError(bill.US_F_OH_PTTCarrierInfo, errorText);
		}

		public void TestCheckUS_SESplitShip()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.JE_DateOfArrival = ZDateTime.Today.AddDays(2);
			declaration.US_UI_NKCarrierSCAC = "APLU";
			declaration.JE_VoyageFlightNo = "001S";
			declaration.JE_MasterBill = "MB03052101";
			declaration.JE_HouseBill = "HB20210503";
			var bill = declaration.PrimaryHouseBill;
			bill.US_SESplitShip = true;
			AssertHasMessageError(bill.US_SESplitShipInfo, FTZAddInfoBillValidation.AtLeastOneSplitDetailMustBeEntered);
			bill.ITAndSplitDetails.AddNew();
			bill.AddInfoValidation.ValidateUS_SESplitShip();
			AssertNoMessageError(bill.US_SESplitShipInfo, FTZAddInfoBillValidation.AtLeastOneSplitDetailMustBeEntered);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			bill = declaration.Bills.AddNew();
		}

		JobDeclaration declaration;
		Bill bill;
	}
}
