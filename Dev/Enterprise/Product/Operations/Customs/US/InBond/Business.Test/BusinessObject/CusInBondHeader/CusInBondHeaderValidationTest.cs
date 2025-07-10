using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;

#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class CusInBondHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestSplitCarrierWarnForDifferentValue()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			header.BH_VoyageNumber = "112";
			header.BH_CarrierSCAC = "A9";
			header.BH_ETA = ZDateTime.BrettsBirthday;
			AssertNoWarning(header.BH_VoyageNumberInfo, ValidationConstants.MoveHeader.SplitCarrierWarnForDifferentValue("Flight No.", "Split Flight No.").ToString());
			AssertNoWarning(header.BH_CarrierSCACInfo, ValidationConstants.MoveHeader.SplitCarrierWarnForDifferentValue("Carrier Code", "Split Carrier Code").ToString());
			var movementHeader1 = header.MovementHeaders.AddNew();
			var movementHeader2 = header.MovementHeaders.AddNew();
			movementHeader1.BM_SplitCarrierSCAC = "A2";
			movementHeader1.BM_SplitFlightNo = "258";
			movementHeader1.BM_ArrivalDate = ZDateTime.BrettsBirthday.AddDays(-10);
			movementHeader2.BM_GONumber = "123";
			AssertNoWarning(header.BH_VoyageNumberInfo, ValidationConstants.MoveHeader.SplitCarrierWarnForDifferentValue("Flight No.", "Split Flight No.").ToString());
			AssertNoWarning(header.BH_CarrierSCACInfo, ValidationConstants.MoveHeader.SplitCarrierWarnForDifferentValue("Carrier Code", "Split Carrier Code").ToString());
			movementHeader2.BM_SplitCarrierSCAC = "A3";
			movementHeader2.BM_SplitFlightNo = "259";
			movementHeader2.BM_ArrivalDate = ZDateTime.BrettsBirthday.AddDays(-9);
			AssertHasWarning(header.BH_VoyageNumberInfo, ValidationConstants.MoveHeader.SplitCarrierWarnForDifferentValue("Flight No.", "Split Flight No.").ToString());
			AssertHasWarning(header.BH_CarrierSCACInfo, ValidationConstants.MoveHeader.SplitCarrierWarnForDifferentValue("Carrier Code", "Split Carrier Code").ToString());
		}

		public void TestEnsureBH_OA_ImporterDoesNotRequirePowerOfAttorney()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_OverrideFreightDefaults = false;
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "TEST";
			OrgAddress importer = Factory.New<OrgAddress>();
			importer.OA_OH = org.PK;
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			header.BH_OA_Importer = importer.PK;
			AssertNoMessageErrors(header.BH_OA_ImporterInfo);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			header.BH_OA_Importer = importer.PK;
			AssertNoWarnings(header.BH_OA_ImporterInfo);
			JobRequiredDocument poaDocument = org.RequiredDocuments.AddNew("POA");
			poaDocument.EQ_DocType = "POA";
			poaDocument.EQ_DocDescription = "Power of Attorney";
			poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			poaDocument.EQ_DateReceived = ZDateTimeOffset.Now;
			poaDocument.EQ_ValidToDate = ZDateTime.Now.AddYears(1);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			header.BH_OA_Importer = importer.PK;
			AssertNoMessageErrors(header.BH_OA_ImporterInfo);
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, PowerOfAttorneyNotificationTypeList.Codes.Warning);
			header.BH_OA_Importer = importer.PK;
			AssertNoWarnings(header.BH_OA_ImporterInfo);
		}

		public void TestBondedWarehouseLicenceIsLogged()
		{
			var factory = new BusinessObjectFactory();
			var helper = new WhsDataTestHelper(factory);
			var warehouse = factory.New<OrgHeader>();
			warehouse.OH_Code = "W#@33";
			warehouse.MainAddress.OA_Address1 = "WAREHOUSE ADDRESS";
			var whsWarehouse = helper.GetNewWhsWarehouse(warehouse.MainAddress.PK, true, "W#@");
			var importer = factory.New<OrgHeader>();
			importer.OH_Code = "IM3@42";
			importer.OH_FullName = "BOB";
			importer.MainAddress.OA_Address1 = "BOB'S ADDRESS";
			importer.OH_IsConsignee = true;
			importer.CompanyData.OB_IMUsedBondedWhs = ZBool.True;
			var header = factory.New<CusInBondHeader>();
			var count = 0;
			header.BondedWarehouseLicenceLogin += new LicenceLoginEventHandler((object sender, LicenceLoginEventArgs e) =>
			{
				count++;
				e.LoginHasBeenAttempted = true;
				e.LicenceCheckPoint.Login(new Customs.Business.Testing.TestLicensedComponent());
			});
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselNonContainer;
			header.BH_FTZMove = ZBool.True;
			AssertEquals(true, header.IsABondedWarehousingImporter);
			AssertEquals("licence login count", 0, count);
			var moveHeader = header.MovementHeaders.AddNew();
			header.RunPreSaveValidation();
			AssertEquals(false, moveHeader.IsExBondAutomationEnabled);
			AssertEquals("licence login count", 0, count);
			moveHeader.BM_OA_WarehouseAddress = warehouse.MainAddress.PK;
			AssertEquals(true, moveHeader.IsExBondAutomationEnabled);
			AssertEquals("licence login count", 1, count);
			header.BH_FTZMove = ZBool.False;
			AssertEquals(false, moveHeader.IsExBondAutomationEnabled);
			AssertEquals("licence login count", 1, count);
			header.BH_FTZMove = ZBool.True;
			AssertEquals(true, moveHeader.IsExBondAutomationEnabled);
			AssertEquals("licence login count", 2, count);
			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			AssertEquals(true, moveHeader.IsExBondAutomationEnabled);
			AssertEquals("licence login count", 3, count);
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			AssertEquals(false, moveHeader.IsExBondAutomationEnabled);
			AssertEquals("licence login count", 3, count);
			header.BH_ImportTransportMode = TransportModeCodes.Codes.TruckContainer;
			AssertEquals(true, moveHeader.IsExBondAutomationEnabled);
			AssertEquals("licence login count", 4, count);
			header.BH_OA_Importer = ZGuid.Empty;
			AssertEquals(false, moveHeader.IsExBondAutomationEnabled);
			AssertEquals("licence login count", 4, count);
			var importer2 = factory.New<OrgHeader>();
			importer2.OH_Code = "WEM3@42";
			importer2.OH_FullName = "WENDY";
			importer2.MainAddress.OA_Address1 = "WENDY'S ADDRESS";
			importer2.OH_IsConsignee = true;
			importer2.CompanyData.OB_IMUsedBondedWhs = ZBool.False;
			header.BH_OA_Importer = importer2.MainAddress.PK;
			AssertEquals(false, moveHeader.IsExBondAutomationEnabled);
			AssertEquals("licence login count", 4, count);
			header.BH_OA_Importer = importer.MainAddress.PK;
			AssertEquals(true, moveHeader.IsExBondAutomationEnabled);
			AssertEquals("licence login count", 5, count);
			moveHeader.BM_OA_WarehouseAddress = importer.MainAddress.PK;
			AssertEquals(false, moveHeader.IsExBondAutomationEnabled);
			AssertEquals("licence login count", 5, count);
			moveHeader.BM_OA_WarehouseAddress = importer2.MainAddress.PK;
			AssertEquals(false, moveHeader.IsExBondAutomationEnabled);
			AssertEquals("licence login count", 5, count);
			moveHeader.BM_OA_WarehouseAddress = warehouse.MainAddress.PK;
			AssertEquals(true, moveHeader.IsExBondAutomationEnabled);
			AssertEquals("licence login count", 6, count);
		}

		public void TestCheckBH_OA_Importer()
		{
			var helper = new WhsDataTestHelper(Factory);
			var importer1 = Factory.New<OrgHeader>();
			importer1.OH_Code = "@#@";
			importer1.CompanyData.OB_IMUsedBondedWhs = true;
			var importer2 = helper.Importer;
			var importer2Address2 = importer2.Addresses.AddNew();
			importer2Address2.OA_Address1 = "ADDRESS 1";
			helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "HO2");
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer1.MainAddress.PK;
			header.BH_FTZMove = true;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
			AssertNoError(header.BH_OA_ImporterInfo, ValidationConstants.BondedWarehouse.WarehouseTransactionExistsNeedsCancel);
			header.BH_OA_Importer = importer2.MainAddress.PK;
			AssertNoError(header.BH_OA_ImporterInfo, ValidationConstants.BondedWarehouse.WarehouseTransactionExistsNeedsCancel);
			Factory.Save();
			header.BH_OA_Importer = importer1.MainAddress.PK;
			AssertHasError(header.BH_OA_ImporterInfo, ValidationConstants.BondedWarehouse.WarehouseTransactionExistsNeedsCancel);
			header.BH_OA_Importer = importer2Address2.PK;
			AssertNoError(header.BH_OA_ImporterInfo, ValidationConstants.BondedWarehouse.WarehouseTransactionExistsNeedsCancel);
		}

		public void TestCheckBH_FTZMove()
		{
			var helper = new WhsDataTestHelper(Factory);
			var importer = helper.Importer;
			helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "HO2");
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_FTZMove = true;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
			AssertNoError(header.BH_FTZMoveInfo, ValidationConstants.BondedWarehouse.WarehouseTransactionExistsNeedsCancel);
			header.BH_FTZMove = false;
			AssertNoError(header.BH_FTZMoveInfo, ValidationConstants.BondedWarehouse.WarehouseTransactionExistsNeedsCancel);
			header.BH_FTZMove = true;
			AssertNoError(header.BH_FTZMoveInfo, ValidationConstants.BondedWarehouse.WarehouseTransactionExistsNeedsCancel);
			Factory.Save();
			header.BH_FTZMove = false;
			AssertHasError(header.BH_FTZMoveInfo, ValidationConstants.BondedWarehouse.WarehouseTransactionExistsNeedsCancel);
			moveHeader.BM_WarehouseTransactionStatus = ZString.Empty;
			header.Validation.ValidateBH_FTZMove();
			AssertNoError(header.BH_FTZMoveInfo, ValidationConstants.BondedWarehouse.WarehouseTransactionExistsNeedsCancel);
		}

		public void TestCheckBH_HeaderType()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = "!";
			AssertHasMessageErrorContaining(header.BH_HeaderTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(header.BH_HeaderTypeInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			AssertNoMessageErrorContaining(header.BH_HeaderTypeInfo, ListValidation.InvalidCodeMessageError);
			header.BH_HeaderType = ZString.Empty;
			AssertHasMessageErrorContaining(header.BH_HeaderTypeInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			AssertNoNotifications(header.BH_HeaderTypeInfo);
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			header.BH_FTZMove = true;
			AssertNoErrors(header.BH_HeaderTypeInfo);
			header.BH_ImportTransportMode = TransportModeCodes.Codes.TruckNonContainer;
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			AssertHasMessageErrorContaining(header.BH_HeaderTypeInfo, ValidationConstants.Header.InvalidHeaderTypeForTransportModeWithoutPreviousITNumber.ToString());
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			AssertNoMessageErrorContaining(header.BH_HeaderTypeInfo, ValidationConstants.Header.InvalidHeaderTypeForTransportModeWithoutPreviousITNumber.ToString());
			var moveDetail = header.MovementHeader.MovementDetails.AddNew();
			header.MovementHeader.MovementDetails.AddNew();
			header.Validation.ValidateAll();
			AssertNoMessageErrorContaining(header.BH_HeaderTypeInfo, ValidationConstants.Header.InvalidHeaderTypeForTransportModeWithPreviousITNumber.ToString());
			moveDetail.B9_PreviousITNumber = "08112345678";
			header.Validation.ValidateAll();
			AssertHasMessageErrorContaining(header.BH_HeaderTypeInfo, ValidationConstants.Header.InvalidHeaderTypeForTransportModeWithPreviousITNumber.ToString());
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			AssertNoMessageErrorContaining(header.BH_HeaderTypeInfo, ValidationConstants.Header.InvalidHeaderTypeForTransportModeWithPreviousITNumber.ToString());
		}

		public void TestCheckBH_ETA()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			header.BH_ETA = new ZDateTime(2009, 10, 1);
			AssertNoMessageErrorContaining(header.BH_ETAInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_ETA = ZDateTime.Empty;
			AssertHasMessageErrorContaining(header.BH_ETAInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			AssertNoMessageErrorContaining(header.BH_ETAInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			header.BH_FTZMove = ZBool.True;
			AssertNoMessageErrorContaining(header.BH_ETAInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			header.ValidationModes = ValidationModes.AirInitiationAndDeletion;
			header.BH_ETA = ZDateTime.Empty;
			AssertHasMessageErrorContaining(header.BH_ETAInfo, MandatoryValidation.YouHaveNotEntered);
			header.ValidationModes = ValidationModes.AirEntireInBondArrival;
			header.BH_ETA = ZDateTime.Empty;
			AssertHasMessageErrorContaining(header.BH_ETAInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBH_ImportConveyanceCountry()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_FTZMove = ZBool.False;
			header.BH_ImportTransportMode = ZString.Empty;
			header.BH_ImportConveyanceCountry = "CA";
			AssertNoMessageErrorContaining(header.BH_ImportConveyanceCountryInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			header.BH_ImportConveyanceCountry = "";
			AssertNoMessageErrorContaining(header.BH_ImportConveyanceCountryInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			header.BH_ImportConveyanceCountry = "CA";
			header.BH_ImportConveyanceCountry = "";
			AssertHasMessageErrorContaining(header.BH_ImportConveyanceCountryInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			header.BH_ImportConveyanceCountry = "";
			AssertNoMessageErrorContaining(header.BH_ImportConveyanceCountryInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_ImportTransportMode = ZString.Empty;
			header.BH_FTZMove = ZBool.True;
			AssertNoMessageErrorContaining(header.BH_ImportConveyanceCountryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(header.BH_ImportConveyanceCountryInfo, ListValidation.InvalidCodeMessageError);
			header.BH_ImportConveyanceCountry = "Z!";
			AssertHasMessageErrorContaining(header.BH_ImportConveyanceCountryInfo, ListValidation.InvalidCodeMessageError);
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			header.BH_ImportConveyanceCountry = "Z!";
			AssertNoMessageErrorContaining(header.BH_ImportConveyanceCountryInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBH_ImportConveyanceName()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_FTZMove = ZBool.False;
			header.BH_ImportTransportMode = ZString.Empty;
			header.BH_ImportConveyanceName = "WHO IS THIS";
			AssertNoMessageErrorContaining(header.BH_ImportConveyanceNameInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_ImportConveyanceName = "";
			AssertHasMessageErrorContaining(header.BH_ImportConveyanceNameInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_PostDepartureOnly = true;
			header.BH_ImportConveyanceName = "";
			AssertNoMessageErrorContaining(header.BH_ImportConveyanceNameInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_PostDepartureOnly = false;
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirContainer;
			header.BH_ImportConveyanceName = "";
			AssertNoMessageErrorContaining(header.BH_ImportConveyanceNameInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			AssertNoMessageErrorContaining(header.BH_ImportConveyanceNameInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_ImportTransportMode = ZString.Empty;
			header.BH_FTZMove = ZBool.True;
			AssertNoMessageErrorContaining(header.BH_ImportConveyanceNameInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_FTZMove = ZBool.False;
			header.BH_ImportConveyanceName = "THIS IS A LONG IMP CONVEYANCE NAME";
			AssertHasWarningContaining(header.BH_ImportConveyanceNameInfo, ValidationConstants.Header.ConveyanceNameLengthExceed.ToString());
			header.BH_PostDepartureOnly = true;
			header.Validation.ValidateBH_ImportConveyanceName();
			AssertNoWarningContaining(header.BH_ImportConveyanceNameInfo, ValidationConstants.Header.ConveyanceNameLengthExceed.ToString());
			header.BH_HeaderType = InBondHeaderTypeList.Codes.DocumentOnly;
			header.BH_ImportConveyanceName = "AAAAAAAAAABBBBBBBBBBEEEEEEEEE";
			AssertNoWarningContaining(header.BH_ImportConveyanceNameInfo, ValidationConstants.Header.ConveyanceNameLengthExceed.ToString());
		}

		public void TestCheckBH_PortUnladingDCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3901", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = ZString.Empty;
			header.BH_PortUnladingDCode = "3901";
			AssertNoMessageErrorContaining(header.BH_PortUnladingDCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(header.BH_PortUnladingDCodeInfo, ListValidation.InvalidCodeMessageError);
			header.BH_PortUnladingDCode = "";
			AssertHasMessageErrorContaining(header.BH_PortUnladingDCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(header.BH_PortUnladingDCodeInfo, ListValidation.InvalidCodeMessageError);
			header.BH_PortUnladingDCode = "!z!!";
			AssertNoMessageErrorContaining(header.BH_PortUnladingDCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(header.BH_PortUnladingDCodeInfo, ListValidation.InvalidCodeMessageError);
			header.BH_PostDepartureOnly = true;
			header.BH_PortUnladingDCode = "";
			AssertNoMessageErrorContaining(header.BH_PortUnladingDCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(header.BH_PortUnladingDCodeInfo, ListValidation.InvalidCodeMessageError);
			header.BH_PortUnladingDCode = "!z!!";
			AssertNoMessageErrorContaining(header.BH_PortUnladingDCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(header.BH_PortUnladingDCodeInfo, ListValidation.InvalidCodeMessageError);
			header.BH_PostDepartureOnly = false;
			header.BH_ImportTransportMode = ZString.Empty;
			header.BH_HeaderType = InBondHeaderTypeList.Codes.DocumentOnly;
			header.BH_PortUnladingDCode = "!z!!";
			AssertHasWarningContaining(header.BH_PortUnladingDCodeInfo, ListValidation.InvalidCodeMessage);
			header.BH_PortUnladingDCode = "3901";
			AssertNoWarningContaining(header.BH_PortUnladingDCodeInfo, ListValidation.InvalidCodeMessage);
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			header.ValidationModes = ValidationModes.AirInitiationAndDeletion;
			header.BH_PortUnladingDCode = ZString.Empty;
			AssertHasMessageErrorContaining(header.BH_PortUnladingDCodeInfo, MandatoryValidation.YouHaveNotEntered);
			header.ValidationModes = ValidationModes.AirEntireInBondArrival;
			header.BH_PortUnladingDCode = ZString.Empty;
			AssertNoMessageErrorContaining(header.BH_PortUnladingDCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestVoyageNumberAndCarrierSCACWhenIsAir()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			AssertEquals(false, header.BH_VoyageNumberInfo.Notifications.HasErrors());
			AssertEquals(false, header.BH_CarrierSCACInfo.Notifications.HasErrors());
			header.BH_VoyageNumber = "";
			header.BH_CarrierSCAC = "BF";
			AssertEquals(false, header.BH_VoyageNumberInfo.Notifications.HasErrors());
			AssertEquals(false, header.BH_CarrierSCACInfo.Notifications.HasErrors());
			header.BH_VoyageNumber = "111";
			header.BH_CarrierSCAC = "";
			AssertEquals(false, header.BH_VoyageNumberInfo.Notifications.HasErrors());
			AssertEquals(false, header.BH_CarrierSCACInfo.Notifications.HasErrors());
			header.BH_VoyageNumber = "1234";
			header.BH_CarrierSCAC = "BF";
			AssertEquals(false, header.BH_VoyageNumberInfo.Notifications.HasErrors());
			AssertEquals(false, header.BH_CarrierSCACInfo.Notifications.HasErrors());
		}

		public void TestCheckBH_VoyageNumber()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_VoyageNumber = "V23423";
			AssertNoMessageErrorContaining(header.BH_VoyageNumberInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_VoyageNumber = "";
			AssertHasMessageErrorContaining(header.BH_VoyageNumberInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_PostDepartureOnly = true;
			header.BH_VoyageNumber = "";
			AssertNoMessageErrorContaining(header.BH_VoyageNumberInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_PostDepartureOnly = false;
			header.ValidationModes = ValidationModes.AirInitiationAndDeletion;
			header.BH_VoyageNumber = "";
			AssertHasMessageErrorContaining(header.BH_VoyageNumberInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_VoyageNumber = "AZ1234";
			AssertHasMessageError(header.BH_VoyageNumberInfo, ValidationConstants.Header.InvalidFlightNumber.ToString());
			header.BH_VoyageNumber = "123";
			AssertNoMessageError(header.BH_VoyageNumberInfo, ValidationConstants.Header.InvalidFlightNumber.ToString());
			header.BH_VoyageNumber = "123A";
			AssertNoMessageError(header.BH_VoyageNumberInfo, ValidationConstants.Header.InvalidFlightNumber.ToString());
			header.BH_VoyageNumber = "1234";
			AssertNoMessageError(header.BH_VoyageNumberInfo, ValidationConstants.Header.InvalidFlightNumber.ToString());
			header.BH_VoyageNumber = "1234A";
			AssertNoMessageError(header.BH_VoyageNumberInfo, ValidationConstants.Header.InvalidFlightNumber.ToString());
			header.ValidationModes = ValidationModes.AirEntireInBondExportation;
			header.BH_VoyageNumber = "";
			AssertNoMessageErrorContaining(header.BH_VoyageNumberInfo, MandatoryValidation.YouHaveNotEntered);
			header.ValidationModes = ValidationModes.Departure;
			header.BH_VoyageNumber = "1234567890";
			AssertHasMessageError(header.BH_VoyageNumberInfo, ValidationConstants.Header.VoyageTripNumberLengthExceeded.ToString());
			header.BH_VoyageNumber = "12345";
			AssertNoMessageError(header.BH_VoyageNumberInfo, ValidationConstants.Header.VoyageTripNumberLengthExceeded.ToString());
			var movementHeader1 = header.MovementHeaders.AddNew();
			var movementHeader2 = header.MovementHeaders.AddNew();
			movementHeader1.BM_SplitFlightNo = "001";
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			header.BH_VoyageNumber = "";
			AssertHasMessageErrorContaining(header.BH_VoyageNumberInfo, MandatoryValidation.YouHaveNotEntered);
			movementHeader2.BM_SplitFlightNo = "002";
			AssertNoMessageErrorContaining(header.BH_VoyageNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestValidateAtLeastOneBillExists()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.Validation.ValidateAll();
			AssertHasRowMessageError(header, ValidationConstants.Header.AtLeastOneBillExists);
			header.BH_PostDepartureOnly = true;
			header.Validation.ValidateAll();
			AssertNoRowMessageError(header, ValidationConstants.Header.AtLeastOneBillExists);
			header.BH_PostDepartureOnly = false;
			CusInBondBill bill = header.Bills.AddNew();
			header.Validation.ValidateAll();
			AssertNoRowMessageError(header, ValidationConstants.Header.AtLeastOneBillExists);
			header.Bills.DeleteAll();
			header.ValidationModes = ValidationModes.AirEntireInBondArrival;
			header.Validation.ValidateAll();
			AssertNoRowMessageError(header, ValidationConstants.Header.AtLeastOneBillExists);
		}

		public void TestCheckBH_ImportTransportMode()
		{
			var helper = new WhsDataTestHelper(Factory);
			var importer = helper.Importer;
			helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "HO2");
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_FTZMove = true;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
			header.BH_ImportTransportMode = ZString.Empty;
			AssertHasMessageErrorContaining(header.BH_ImportTransportModeInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_PostDepartureOnly = true;
			header.BH_ImportTransportMode = ZString.Empty;
			AssertNoMessageErrorContaining(header.BH_ImportTransportModeInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_PostDepartureOnly = false;
			header.BH_ImportTransportMode = "Z!";
			AssertNoMessageErrorContaining(header.BH_ImportTransportModeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(header.BH_ImportTransportModeInfo, ListValidation.InvalidCodeMessageError);
			foreach (CodeDescriptionPair pair in Factory.GetCachedValue<InBondTransportModeCodes>())
			{
				header.BH_ImportTransportMode = pair.Code;
				AssertNoMessageErrorContaining(header.BH_ImportTransportModeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(header.BH_ImportTransportModeInfo, ListValidation.InvalidCodeMessageError);
			}

			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			AssertNoError(header.BH_ImportTransportModeInfo, ValidationConstants.BondedWarehouse.WarehouseTransactionExistsNeedsCancel);
			Factory.Save();
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			AssertHasError(header.BH_ImportTransportModeInfo, ValidationConstants.BondedWarehouse.WarehouseTransactionExistsNeedsCancel);
			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselNonContainer;
			AssertNoError(header.BH_ImportTransportModeInfo, ValidationConstants.BondedWarehouse.WarehouseTransactionExistsNeedsCancel);
		}

		public void TestCheckBH_FIRMS()
		{
			var firmsHelper = new UniversalReferenceTestDataHelper(Factory);
			firmsHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			firmsHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "AAZ!", "Misaka", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			var code1 = firmsHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "JJ0!", "Misaka", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			firmsHelper.CreateNewOrGetExistingCusCodeListAttribute(code1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.FacilityType, FacilityTypeList.Codes.ContainerFreightStation_01);
			var code2 = firmsHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "JJ3!", "Misaka", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			firmsHelper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.FacilityType, FacilityTypeList.Codes.ForeignTradeZone_02);
			Factory.Save();

			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_FTZMove = ZBool.True;
			header.BH_FIRMS = ZString.Empty;
			AssertHasMessageErrorContaining(header.BH_FIRMSInfo, ValidationConstants.Header.FIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			header.BH_FIRMS = "Z!!!";
			AssertNoMessageErrorContaining(header.BH_FIRMSInfo, ValidationConstants.Header.FIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			AssertHasMessageErrorContaining(header.BH_FIRMSInfo, ListValidation.InvalidCodeMessageError);
			header.BH_FIRMS = "AAZ!";
			AssertNoMessageErrorContaining(header.BH_FIRMSInfo, ValidationConstants.Header.FIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			AssertNoMessageErrorContaining(header.BH_FIRMSInfo, ListValidation.InvalidCodeMessageError);
			header.BH_FTZMove = ZBool.False;
			header.BH_FIRMS = ZString.Empty;
			AssertNoNotifications(header.BH_FIRMSInfo);
			header.BH_FTZMove = ZBool.True;
			header.BH_HeaderType = InBondHeaderTypeList.Codes.DocumentOnly;
			header.BH_FIRMS = "Z!!!";
			AssertHasWarningContaining(header.BH_FIRMSInfo, ListValidation.InvalidCodeMessage);
			header.BH_FIRMS = "AAZ!";
			AssertNoWarningContaining(header.BH_FIRMSInfo, ListValidation.InvalidCodeMessage);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			header.BH_FIRMS = "ABCD";
			AssertHasMessageError(header.BH_FIRMSInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(header.BH_FIRMSInfo, ValidationConstants.Header.FIRMSCodeInvalidForInBond.ToString());
			header.BH_FIRMS = "JJ0!";
			AssertHasMessageError(header.BH_FIRMSInfo, ValidationConstants.Header.FIRMSCodeInvalidForInBond.ToString());
			header.BH_FIRMS = "JJ3!";
			AssertNoMessageError(header.BH_FIRMSInfo, ValidationConstants.Header.FIRMSCodeInvalidForInBond.ToString());
			firmsHelper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.FacilityType, FacilityTypeList.Codes.BondedWarehouse_04);
			header.BH_FIRMS = "JJ3!";
			AssertNoMessageError(header.BH_FIRMSInfo, ValidationConstants.Header.FIRMSCodeInvalidForInBond.ToString());
			firmsHelper.CreateNewOrGetExistingCusCodeListAttribute(code2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.FacilityType, FacilityTypeList.Codes.MultiUseBond_10);
			header.BH_FIRMS = "JJ3!";
			AssertNoMessageError(header.BH_FIRMSInfo, ValidationConstants.Header.FIRMSCodeInvalidForInBond.ToString());
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			header.BH_FTZMove = ZBool.True;
			header.BH_FIRMS = ZString.Empty;
			AssertNoMessageErrorContaining(header.BH_FIRMSInfo, ValidationConstants.Header.FIRMSForFTZorBondedWarehouseWithdrawals.ToString());
		}

		public void TestCheckBH_CarrierSCAC()
		{
			var carrier1 = Factory.New<USCarrierCombined>();
			carrier1.UI_Code = "SC1Z";
			carrier1.UI_ModeOfTransportation = TransportModeCodes.Codes.TruckNonContainer;
			var carrier2 = Factory.New<USCarrierCombined>();
			carrier2.UI_Code = "Z!Z!";
			carrier2.UI_ModeOfTransportation = TransportModeCodes.Codes.VesselContainer;
			var header = Factory.New<CusInBondHeader>();
			header.BH_FTZMove = ZBool.False;
			header.BH_ImportTransportMode = TransportModeCodes.Codes.TruckNonContainer;
			header.BH_CarrierSCAC = ZString.Empty;
			AssertHasMessageErrorContaining(header.BH_CarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_PostDepartureOnly = true;
			header.BH_CarrierSCAC = ZString.Empty;
			AssertNoMessageErrorContaining(header.BH_CarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_PostDepartureOnly = false;
			header.BH_CarrierSCAC = "Z!Z!";
			AssertNoMessageErrorContaining(header.BH_CarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(header.BH_CarrierSCACInfo, ValidationConstants.Header.CarrierCodeNotValidForTransportMode.ToString());
			header.BH_CarrierSCAC = carrier1.UI_Code;
			AssertNoMessageErrorContaining(header.BH_CarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(header.BH_CarrierSCACInfo, ValidationConstants.Header.CarrierCodeNotValidForTransportMode.ToString());
			header.BH_HeaderType = InBondHeaderTypeList.Codes.DocumentOnly;
			header.BH_CarrierSCAC = "Z!Z!";
			AssertHasWarningContaining(header.BH_CarrierSCACInfo, ValidationConstants.Header.CarrierCodeNotValidForTransportMode.ToString());
			header.BH_CarrierSCAC = "SC1Z";
			AssertNoWarningContaining(header.BH_CarrierSCACInfo, ValidationConstants.Header.CarrierCodeNotValidForTransportMode.ToString());
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselNonContainer;
			header.BH_CarrierSCAC = "APLU";
			AssertNoMessageErrorContaining(header.BH_CarrierSCACInfo, ValidationConstants.Header.CarrierCodeNotValidForTransportMode.ToString());
			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			AssertNoMessageErrorContaining(header.BH_CarrierSCACInfo, ValidationConstants.Header.CarrierCodeNotValidForTransportMode.ToString());
			header.ValidationModes = ValidationModes.AirInitiationAndDeletion;
			header.BH_CarrierSCAC = ZString.Empty;
			AssertHasMessageErrorContaining(header.BH_CarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			header.ValidationModes = ValidationModes.AirEntireInBondExportation;
			header.BH_CarrierSCAC = ZString.Empty;
			AssertNoMessageErrorContaining(header.BH_CarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_CarrierSCAC = "1234";
			AssertHasMessageError(header.BH_CarrierSCACInfo, ValidationConstants.Header.InvalidAirCarrierCodeLength.ToString());
			header.BH_CarrierSCAC = "123";
			AssertNoMessageError(header.BH_CarrierSCACInfo, ValidationConstants.Header.InvalidAirCarrierCodeLength.ToString());
			var movementHeader1 = header.MovementHeaders.AddNew();
			var movementHeader2 = header.MovementHeaders.AddNew();
			movementHeader1.BM_SplitCarrierSCAC = "A2";
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			header.BH_CarrierSCAC = ZString.Empty;
			AssertHasMessageErrorContaining(header.BH_CarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			movementHeader2.BM_SplitCarrierSCAC = "A2";
			AssertNoMessageErrorContaining(header.BH_CarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckThreeLetterAirCarrierCode()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			header.MovementHeaders.AddNew();
			header.BH_CarrierSCAC = "A1";
			header.ThreeLetterAirCarrierCode = ZString.Empty;
			header.Validation.ValidateAll();
			AssertEquals(ZString.Empty, header.ThreeLetterAirCarrierCode);
			AssertHasMessageError("The 2-Letter code should not exist in the Carrier table", header.BH_CarrierSCACInfo, ValidationConstants.Header.CarrierCodeNotValidForTransportMode.ToString());
			AssertHasMessageError("A 3-Letter Code should not be located using a 2-letter code lookup", header.ThreeLetterAirCarrierCodeInfo, ValidationConstants.Header.CodeInvalidMessage.ToString());
			var carrier1 = Factory.New<USCarrierCombined>();
			carrier1.UI_Code = "A1";
			carrier1.UI_ModeOfTransportation = TransportModeCodes.Codes.AirNonContainer;
			var refAirline1 = Factory.New<RefAirline>();
			refAirline1.RM_EagleAddedAirlinePrefixOrAccountingCode = "123";
			refAirline1.RM_TwoCharacterCode = "A1";
			refAirline1.RM_ThreeLetterCode = "AAA";
			header.Validation.ValidateAll();
			AssertEquals("The 3-Letter Code should have been auto-filled from a 2-letter code lookup", "AAA", header.ThreeLetterAirCarrierCode);
			AssertNoMessageError("The 2-Letter code should exist in the Carrier table", header.BH_CarrierSCACInfo, ValidationConstants.Header.CarrierCodeNotValidForTransportMode.ToString());
			AssertNoMessageError("A 3-Letter Code should be located using a 2-letter code lookup", header.ThreeLetterAirCarrierCodeInfo, ValidationConstants.Header.CodeInvalidMessage.ToString());
			AssertNoWarning("A warning should not appear when the 3-Letter code is determined by the system", header.BH_CarrierSCACInfo, ValidationConstants.Header.CodeIgnoredMessage.ToString());
			refAirline1.RM_ThreeLetterCode = "";
			header.Validation.ValidateAll();
			AssertEquals(ZString.Empty, header.ThreeLetterAirCarrierCode);
			AssertHasMessageError("An error should show when the 2-letter code is invalid and the 3-Letter code is not set", header.ThreeLetterAirCarrierCodeInfo, ValidationConstants.Header.CodeInvalidMessage.ToString());
			var carrier2 = Factory.New<USCarrierCombined>();
			carrier2.UI_Code = "CA";
			carrier2.UI_ModeOfTransportation = TransportModeCodes.Codes.AirNonContainer;
			header.BH_CarrierSCAC = "CA";
			header.ThreeLetterAirCarrierCode = ZString.Empty;
			header.Validation.ValidateAll();
			AssertEquals(ZString.Empty, header.ThreeLetterAirCarrierCode);
			AssertNoMessageError("An error should not show when the 2-letter code is valid and the 3-Letter code is not set", header.ThreeLetterAirCarrierCodeInfo, ValidationConstants.Header.CodeInvalidMessage.ToString());
			AssertNoWarning("A warning should not appear when the 2-letter code is valid and the 3-Letter code is not set", header.BH_CarrierSCACInfo, ValidationConstants.Header.CodeIgnoredMessage.ToString());
			var refAirline2 = Factory.New<RefAirline>();
			refAirline2.RM_EagleAddedAirlinePrefixOrAccountingCode = "456";
			refAirline2.RM_TwoCharacterCode = "CA";
			refAirline2.RM_ThreeLetterCode = "DDD";
			header.BH_CarrierSCAC = "CA";
			header.ThreeLetterAirCarrierCode = "DDD";
			header.Validation.ValidateAll();
			AssertHasWarning("A warning should appear when both the 2-letter and the 3-Letter codes are set by the user", header.BH_CarrierSCACInfo, ValidationConstants.Header.CodeIgnoredMessage.ToString());
			header.BH_CarrierSCAC = ZString.Empty;
			header.Validation.ValidateAll();
			AssertEquals("DDD", header.ThreeLetterAirCarrierCode);
			AssertNoMessageErrorContaining(header.BH_CarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_CarrierSCAC = ZString.Empty;
			header.ThreeLetterAirCarrierCode = "DEF";
			header.Validation.ValidateAll();
			AssertEquals("DEF", header.ThreeLetterAirCarrierCode);
			AssertHasMessageErrorContaining(header.ThreeLetterAirCarrierCodeInfo, ValidationConstants.Header.CarrierCodeNotValidForTransportMode.ToString());
			header.BH_CarrierSCAC = ZString.Empty;
			header.ThreeLetterAirCarrierCode = ZString.Empty;
			header.Validation.ValidateAll();
			AssertHasMessageErrorContaining(header.BH_CarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestAirTranportModeNotValidForFTZWarehouse()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_FTZMove = true;
			AssertNoError(header.BH_ImportTransportModeInfo, ValidationConstants.MoveDetail.NoAirTransportForWHSFTZ);
			header.BH_ImportTransportMode = "40";
			AssertHasMessageError(header.BH_ImportTransportModeInfo, ValidationConstants.MoveDetail.NoAirTransportForWHSFTZ.ToString());
			header.BH_ImportTransportMode = "10";
			AssertNoMessageError(header.BH_ImportTransportModeInfo, ValidationConstants.MoveDetail.NoAirTransportForWHSFTZ.ToString());
			header.BH_ImportTransportMode = "40";
			AssertHasMessageError(header.BH_ImportTransportModeInfo, ValidationConstants.MoveDetail.NoAirTransportForWHSFTZ.ToString());
			header.BH_FTZMove = false;
			AssertNoError(header.BH_ImportTransportModeInfo, ValidationConstants.MoveDetail.NoAirTransportForWHSFTZ);
		}

		public void TestAirTranportModeCombineNonAMSInvalid()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			AssertHasMessageError(header.BH_HeaderTypeInfo, ValidationConstants.Header.NonAMSInvalidForTransportModeAir.ToString());
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			AssertNoMessageError(header.BH_HeaderTypeInfo, ValidationConstants.Header.NonAMSInvalidForTransportModeAir.ToString());
			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselNonContainer;
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			AssertNoMessageError(header.BH_HeaderTypeInfo, ValidationConstants.Header.NonAMSInvalidForTransportModeAir.ToString());
			header.BH_ImportTransportMode = TransportModeCodes.Codes.VesselContainer;
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			AssertNoMessageError(header.BH_HeaderTypeInfo, ValidationConstants.Header.NonAMSInvalidForTransportModeAir.ToString());
			header.BH_ImportTransportMode = TransportModeCodes.Codes.RailNonContainer;
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			AssertNoMessageError(header.BH_HeaderTypeInfo, ValidationConstants.Header.NonAMSInvalidForTransportModeAir.ToString());
			header.BH_ImportTransportMode = TransportModeCodes.Codes.TruckNonContainer;
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			AssertNoMessageError(header.BH_HeaderTypeInfo, ValidationConstants.Header.NonAMSInvalidForTransportModeAir.ToString());
			header.BH_ImportTransportMode = TransportModeCodes.Codes.FixedTransportInstallations;
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			AssertNoMessageError(header.BH_HeaderTypeInfo, ValidationConstants.Header.NonAMSInvalidForTransportModeAir.ToString());
		}
	}
}
