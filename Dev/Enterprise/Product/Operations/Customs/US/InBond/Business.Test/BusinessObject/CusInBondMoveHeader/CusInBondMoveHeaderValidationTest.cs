using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	[TestedType(typeof(CusInBondMoveHeaderValidation))]
	sealed class CusInBondMoveHeaderValidationTest : US.Business.Testing.CusInBondMoveHeaderValidationTest
	{
		public void TestCheckWhenEnterSplitCarrierSCACOrSplitFlightNo()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			var moveHeader = header.MovementHeaders.AddNew();
			header.BH_CarrierSCAC = "C4";
			header.BH_VoyageNumber = "234";
			moveHeader.Validation.ValidateAll();
			AssertEquals(false, moveHeader.BM_SplitFlightNoInfo.Notifications.HasErrors());
			AssertEquals(false, moveHeader.BM_SplitCarrierSCACInfo.Notifications.HasErrors());
			moveHeader.BM_SplitFlightNo = "123";
			moveHeader.Validation.ValidateAll();
			AssertHasMessageErrorContaining(moveHeader.BM_SplitCarrierSCACInfo, ValidationConstants.MoveHeader.ShouldEnterSplitCarrierCode.ToString());
			moveHeader.BM_SplitFlightNo = string.Empty;
			moveHeader.BM_SplitCarrierSCAC = "CA";
			moveHeader.Validation.ValidateAll();
			AssertHasMessageErrorContaining(moveHeader.BM_SplitFlightNoInfo, ValidationConstants.MoveHeader.SholudEnterSplitFlightNo.ToString());
		}

		public void TestCheckBM_ExportLadenOn()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.TruckNonContainer;
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			header.ValidationModes = ValidationModes.AirEntireInBondExportation;
			moveHeader.BM_ExportTransportMode = InBondTransportModeCodes.Codes.VesselNonContainer;
			moveHeader.BM_ExportLadenOn = "ABCDEFGHIJKLMNOPQRSTUVXYZZZZZZ";
			AssertHasWarningContaining(moveHeader.BM_ExportLadenOnInfo, ValidationConstants.Header.ConveyanceNameLengthExceed.ToString());
			moveHeader.BM_ExportLadenOn = "ABCDEFG";
			AssertNoWarningContaining(moveHeader.BM_ExportLadenOnInfo, ValidationConstants.Header.ConveyanceNameLengthExceed.ToString());
			header.BH_HeaderType = InBondHeaderTypeList.Codes.DocumentOnly;
			moveHeader.Validation.ValidateBM_ExportLadenOn();
			AssertNoWarningContaining(moveHeader.BM_ExportLadenOnInfo, ValidationConstants.Header.ConveyanceNameLengthExceed.ToString());
		}

		public void TestCheckBM_MoveToFTZ()
		{
			var helper = new WhsDataTestHelper(Factory);
			var whsWarehouse = helper.GetNewWhsWarehouse(helper.Warehouse2.MainAddress.PK, true, "W#@", WarehouseTypes.Codes.FreeTradeZone);
			Factory.Save();
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = helper.Importer.MainAddress.PK;
			header.BH_FTZMove = true;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse2.MainAddress.PK;
			AssertEquals("SupportsBondedWarehousing", true, moveHeader.SupportsBondedWarehousing);
			AssertEquals("IsMoveToFTZRequired", true, moveHeader.IsMoveToFTZRequired);
			var moveToFTZIsRequired = ValidationConstants.MoveHeader.MoveToFTZIndicatorIsRequired.ToString();
			moveHeader.Validation.ValidateBM_MoveToFTZ();
			AssertHasMessageError(moveHeader.BM_MoveToFTZInfo, moveToFTZIsRequired);
			header.BH_FTZMove = false;
			moveHeader.Validation.ValidateBM_MoveToFTZ();
			AssertNoMessageError(moveHeader.BM_MoveToFTZInfo, moveToFTZIsRequired);
			header.BH_FTZMove = true;
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			moveHeader.Validation.ValidateBM_MoveToFTZ();
			AssertNoMessageError(moveHeader.BM_MoveToFTZInfo, moveToFTZIsRequired);
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse2.MainAddress.PK;
			moveHeader.BM_MoveToFTZ = YesNoDefaultList.Codes.No;
			moveHeader.Validation.ValidateBM_MoveToFTZ();
			AssertNoMessageError(moveHeader.BM_MoveToFTZInfo, moveToFTZIsRequired);
			moveHeader.BM_MoveToFTZ = YesNoDefaultList.Codes.Yes;
			moveHeader.Validation.ValidateBM_MoveToFTZ();
			AssertNoMessageErrors(moveHeader.BM_MoveToFTZInfo);
			moveHeader.BM_MoveToFTZ = ZString.Empty;
			AssertHasMessageError(moveHeader.BM_MoveToFTZInfo, moveToFTZIsRequired);
			var moveToFTZIndicatorIsNotApplicable = ValidationConstants.MoveHeader.MoveToFTZIndicatorIsNotApplicable.ToString();
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			moveHeader.BM_MoveToFTZ = YesNoDefaultList.Codes.No;
			moveHeader.Validation.ValidateBM_MoveToFTZ();
			AssertNoMessageError(moveHeader.BM_MoveToFTZInfo, moveToFTZIsRequired);
			AssertHasMessageError(moveHeader.BM_MoveToFTZInfo, moveToFTZIndicatorIsNotApplicable);
			moveHeader.BM_MoveToFTZ = ZString.Empty;
			AssertNoMessageError(moveHeader.BM_MoveToFTZInfo, moveToFTZIsRequired);
			AssertNoMessageError(moveHeader.BM_MoveToFTZInfo, moveToFTZIndicatorIsNotApplicable);
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._3ImmediateExport;
			moveHeader.BM_MoveToFTZ = YesNoDefaultList.Codes.No;
			moveHeader.Validation.ValidateBM_MoveToFTZ();
			AssertNoMessageError(moveHeader.BM_MoveToFTZInfo, moveToFTZIsRequired);
			AssertHasMessageError(moveHeader.BM_MoveToFTZInfo, moveToFTZIndicatorIsNotApplicable);
			moveHeader.BM_MoveToFTZ = ZString.Empty;
			AssertNoMessageError(moveHeader.BM_MoveToFTZInfo, moveToFTZIsRequired);
			AssertNoMessageError(moveHeader.BM_MoveToFTZInfo, moveToFTZIndicatorIsNotApplicable);
		}

		public void TestCheckBM_SplitCarrierSCAC()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_SplitCarrierSCAC = "";
			AssertHasMessageErrorContaining(moveHeader.BM_SplitCarrierSCACInfo, ValidationConstants.MoveHeader.SplitCarrierCodeForAIR.ToString());
			moveHeader.BM_SplitCarrierSCAC = "Z!Z!";
			AssertHasMessageErrorContaining(moveHeader.BM_SplitCarrierSCACInfo, ValidationConstants.Header.CarrierCodeNotValidForTransportMode.ToString());
			moveHeader.BM_SplitCarrierSCAC = "GF";
			AssertEquals(false, moveHeader.BM_SplitCarrierSCACInfo.Notifications.HasErrors());
		}

		public void TestCheckAirCarrierCode()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			var refAirline = Factory.LoadTop1<RefAirline>(new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, "A2"));
			if (refAirline == null)
			{
				refAirline = Factory.New<RefAirline>();
				refAirline.RM_EagleAddedAirlinePrefixOrAccountingCode = "A8";
				refAirline.RM_TwoCharacterCode = "A2";
			}

			refAirline.RM_ThreeLetterCode = "";
			header.BH_CarrierSCAC = "A2";
			header.ThreeLetterAirCarrierCode = ZString.Empty;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_InBondCarrierSCAC = "A2";
			moveHeader.ThreeLetterInBondAirCarrierCode = "";
			moveHeader.Validation.ValidateAll();
			AssertHasMessageError(moveHeader.ThreeLetterInBondAirCarrierCodeInfo, ValidationConstants.Header.CodeInvalidMessage.ToString());
			moveHeader.ThreeLetterInBondAirCarrierCode = "123";
			moveHeader.Validation.ValidateAll();
			AssertNoMessageError(moveHeader.ThreeLetterInBondAirCarrierCodeInfo, ValidationConstants.Header.CodeInvalidMessage.ToString());
			moveHeader.BM_SplitCarrierSCAC = "A2";
			moveHeader.ThreeLetterSplitAirCarrierCode = "";
			moveHeader.Validation.ValidateAll();
			AssertHasMessageError(moveHeader.ThreeLetterSplitAirCarrierCodeInfo, ValidationConstants.Header.CodeInvalidMessage.ToString());
			moveHeader.ThreeLetterSplitAirCarrierCode = "HDA";
			moveHeader.Validation.ValidateAll();
			AssertNoMessageError(moveHeader.ThreeLetterSplitAirCarrierCodeInfo, ValidationConstants.Header.CodeInvalidMessage.ToString());
			header.ThreeLetterAirCarrierCode = "ICE";
			moveHeader.BM_SplitCarrierSCAC = "";
			moveHeader.ThreeLetterSplitAirCarrierCode = "";
			moveHeader.Validation.ValidateAll();
			AssertNoMessageError(moveHeader.ThreeLetterSplitAirCarrierCodeInfo, ValidationConstants.Header.CodeInvalidMessage.ToString());
		}

		public void TestCheckBM_SplitFlightNo()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_SplitFlightNo = "";
			AssertHasMessageErrorContaining(moveHeader.BM_SplitFlightNoInfo, ValidationConstants.MoveHeader.SplitFlightForAIR.ToString());
			moveHeader.BM_SplitFlightNo = "Z!*!";
			AssertHasMessageErrorContaining(moveHeader.BM_SplitFlightNoInfo, ValidationConstants.Header.InvalidFlightNumber.ToString());
			moveHeader.BM_SplitFlightNo = "1234567";
			AssertHasMessageErrorContaining(moveHeader.BM_SplitFlightNoInfo, ValidationConstants.Header.VoyageTripNumberLengthExceeded.ToString());
			moveHeader.BM_SplitFlightNo = "1234";
			AssertEquals(false, header.BH_CarrierSCACInfo.Notifications.HasErrors());
		}

		public void TestCheckBM_InBondEntryType()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_InBondEntryType = ZString.Empty;
			AssertHasMessageErrorContaining(moveHeader.BM_InBondEntryTypeInfo, MandatoryValidation.YouHaveNotEntered);
			moveHeader.BM_InBondEntryType = "Z!";
			AssertNoMessageErrorContaining(moveHeader.BM_InBondEntryTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(moveHeader.BM_InBondEntryTypeInfo, ListValidation.InvalidCodeMessageError);
			foreach (CodeDescriptionPair pair in Factory.GetCachedValue<InbondCommonTypeList>())
			{
				moveHeader.BM_InBondEntryType = pair.Code;
				AssertNoMessageErrorContaining(moveHeader.BM_InBondEntryTypeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(moveHeader.BM_InBondEntryTypeInfo, ListValidation.InvalidCodeMessageError);
			}

			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			moveHeader.BM_InBondEntryType = ZString.Empty;
			AssertHasMessageErrorContaining(moveHeader.BM_InBondEntryTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBM_ArrivalDate()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			header.ValidationModes = ValidationModes.InBondLevelArrival;
			moveHeader.ShouldSend = true;
			AssertNoMessageErrors(moveHeader.BM_FIRMSInfo);
			moveHeader.BM_ArrivalDate = ZDateTime.Today;
			AssertHasMessageErrors(moveHeader.BM_FIRMSInfo);
			AssertNoMessageError(moveHeader.BM_ArrivalDateInfo, ValidationConstants.MoveHeader.ArrivalDateExceedsTodaysDate.ToString());
			moveHeader.BM_ArrivalDate = ZDateTime.Today.AddDays(2);
			AssertHasMessageError(moveHeader.BM_ArrivalDateInfo, ValidationConstants.MoveHeader.ArrivalDateExceedsTodaysDate.ToString());
		}

		public void TestCheckBM_FIRMS()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "INC~", "Misaka", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			header.ValidationModes = ValidationModes.InBondLevelArrival;
			moveHeader.ShouldSend = true;
			moveHeader.BM_FIRMS = "";
			AssertHasMessageError(moveHeader.BM_FIRMSInfo, "You have not entered a FIRMS.");
			moveHeader.BM_FIRMS = "~~";
			AssertNoMessageError(moveHeader.BM_FIRMSInfo, "You have not entered a FIRMS.");
			AssertHasMessageErrorContaining(moveHeader.BM_FIRMSInfo, ListValidation.InvalidCodeMessageError);
			moveHeader.BM_FIRMS = "INC~";
			AssertNoMessageErrorContaining(moveHeader.BM_FIRMSInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBM_FIRMS_WhenAirMode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "INC~", "Misaka", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeaders.AddNew();
			header.ValidationModes = ValidationModes.InBondLevelArrival;
			moveHeader.ShouldSend = true;
			moveHeader.BM_FIRMS = "";
			moveHeader.Validation.ValidateBM_FIRMS();
			AssertHasMessageErrors(moveHeader.BM_FIRMSInfo);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			moveHeader.Validation.ValidateBM_FIRMS();
			AssertNoMessageErrors(moveHeader.BM_FIRMSInfo);
		}

		public void TestCheckBM_OA_WarehouseAddress()
		{
			var helper = new WhsDataTestHelper(Factory);
			var importer = helper.Importer;
			var warehouseAddress2 = helper.Warehouse.Addresses.AddNew();
			warehouseAddress2.OA_Address1 = "ADDRESS 1";
			helper.GetNewWhsWarehouse(helper.Warehouse.MainAddress.PK, true, "HO2");
			var header = Factory.New<CusInBondHeader>();
			header.BH_OA_Importer = importer.MainAddress.PK;
			header.BH_FTZMove = true;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreated;
			var noWarehouseMessage = ValidationConstants.MoveHeader.NoWarehouseForThisAddress(helper.Importer.OH_Code);
			var warehouseShouldBeInsideHeaderCountryMessage = ValidationConstants.MoveHeader.WarehouseShouldBeInsideHeaderCountry(helper.Importer.OH_Code, "United States");
			helper.Warehouse.OH_RL_NKClosestPort = "AUSYD";
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			AssertNoWarning(moveHeader.BM_OA_WarehouseAddressInfo, noWarehouseMessage.ToString());
			AssertHasWarning(moveHeader.BM_OA_WarehouseAddressInfo, warehouseShouldBeInsideHeaderCountryMessage.ToString());
			AssertNoError(moveHeader.BM_OA_WarehouseAddressInfo, ValidationConstants.BondedWarehouse.WarehouseTransactionExistsNeedsCancel);
			helper.Warehouse.OH_RL_NKClosestPort = "USCHI";
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			AssertNoWarning(moveHeader.BM_OA_WarehouseAddressInfo, noWarehouseMessage.ToString());
			AssertNoWarning(moveHeader.BM_OA_WarehouseAddressInfo, warehouseShouldBeInsideHeaderCountryMessage.ToString());
			AssertNoError(moveHeader.BM_OA_WarehouseAddressInfo, ValidationConstants.BondedWarehouse.WarehouseTransactionExistsNeedsCancel);
			moveHeader.BM_OA_WarehouseAddress = warehouseAddress2.PK;
			AssertHasWarning(moveHeader.BM_OA_WarehouseAddressInfo, noWarehouseMessage.ToString());
			AssertNoError(moveHeader.BM_OA_WarehouseAddressInfo, ValidationConstants.BondedWarehouse.WarehouseTransactionExistsNeedsCancel);
			Factory.Save();
			moveHeader.BM_OA_WarehouseAddress = helper.Warehouse.MainAddress.PK;
			AssertNoWarning(moveHeader.BM_OA_WarehouseAddressInfo, noWarehouseMessage.ToString());
			AssertNoWarning(moveHeader.BM_OA_WarehouseAddressInfo, warehouseShouldBeInsideHeaderCountryMessage.ToString());
			AssertHasError(moveHeader.BM_OA_WarehouseAddressInfo, ValidationConstants.BondedWarehouse.WarehouseTransactionExistsNeedsCancel);
			moveHeader.BM_OA_WarehouseAddress = warehouseAddress2.PK;
			AssertHasWarning(moveHeader.BM_OA_WarehouseAddressInfo, noWarehouseMessage.ToString());
			AssertNoError(moveHeader.BM_OA_WarehouseAddressInfo, ValidationConstants.BondedWarehouse.WarehouseTransactionExistsNeedsCancel);
			moveHeader.BM_OA_WarehouseAddress = ZGuid.Empty;
			AssertHasMessageError(moveHeader.BM_OA_WarehouseAddressInfo, ValidationConstants.MoveHeader.BondedWarehouseIsRequired.ToString());
			importer.CompanyData.OB_IMUsedBondedWhs = false;
			header.BH_FTZMove = true;
			AssertNoMessageError(moveHeader.BM_OA_WarehouseAddressInfo, ValidationConstants.MoveHeader.BondedWarehouseIsRequired.ToString());
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			header.BH_FTZMove = false;
			AssertNoMessageError(moveHeader.BM_OA_WarehouseAddressInfo, ValidationConstants.MoveHeader.BondedWarehouseIsRequired.ToString());
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			header.BH_FTZMove = true;
			moveHeader.BM_OA_WarehouseAddress = warehouseAddress2.PK;
			AssertNoMessageError(moveHeader.BM_OA_WarehouseAddressInfo, ValidationConstants.MoveHeader.BondedWarehouseIsRequired.ToString());
		}

		public void TestCheckBM_ExportTransportMode()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			header.ValidationModes = ValidationModes.InBondLevelExportation;
			moveHeader.ShouldSend = true;
			moveHeader.BM_ExportLadenOn = "12";
			moveHeader.BM_ExportTransportMode = "Z!";
			AssertHasMessageErrorContaining(moveHeader.BM_ExportTransportModeInfo, ListValidation.InvalidCodeMessageError);
			moveHeader.BM_ExportTransportMode = InBondTransportModeCodes.Codes.VesselNonContainer;
			AssertNoMessageErrorContaining(moveHeader.BM_ExportTransportModeInfo, ListValidation.InvalidCodeMessageError);
			moveHeader.BM_ExportTransportMode = InBondTransportModeCodes.Codes.VesselContainer;
			AssertNoMessageErrorContaining(moveHeader.BM_ExportTransportModeInfo, ListValidation.InvalidCodeMessageError);
			header.ValidationModes = ValidationModes.AirEntireInBondExportation;
			moveHeader.BM_ExportTransportMode = "Z!";
			AssertHasMessageErrorContaining(moveHeader.BM_ExportTransportModeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBM_PortOfPresentationCode()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_PortOfPresentationCode = "";
			AssertHasWarning(moveHeader.BM_PortOfPresentationCodeInfo, ValidationConstants.MoveHeader.PortOfPresentation.ToString());
			moveHeader.BM_PortOfPresentationCode = "2704";
			AssertNoWarning(moveHeader.BM_PortOfPresentationCodeInfo, ValidationConstants.MoveHeader.PortOfPresentation.ToString());
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			moveHeader.BM_PortOfPresentationCode = "";
			AssertNoWarning(moveHeader.BM_PortOfPresentationCodeInfo, ValidationConstants.MoveHeader.PortOfPresentation.ToString());
		}

		public void TestCheckBM_TOLCarrierCode()
		{
			USCarrierCombined carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "SC1Z";
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.ValidationModes = ValidationModes.InBondLevelTransferOfLiability;
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_TOLCarrierCode = "Z!Z!";
			AssertHasMessageErrorContaining(moveHeader.BM_TOLCarrierCodeInfo, ListValidation.InvalidCodeMessageError);
			moveHeader.BM_TOLCarrierCode = "SC1Z";
			AssertNoMessageErrorContaining(moveHeader.BM_TOLCarrierCodeInfo, ListValidation.InvalidCodeMessageError);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.DocumentOnly;
			moveHeader.BM_TOLCarrierCode = "Z!Z!";
			AssertHasWarningContaining(moveHeader.BM_TOLCarrierCodeInfo, ListValidation.InvalidCodeMessage);
			moveHeader.BM_TOLCarrierCode = "SC1Z";
			AssertNoWarningContaining(moveHeader.BM_TOLCarrierCodeInfo, ListValidation.InvalidCodeMessage);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			moveHeader.BM_TOLCarrierCode = "Z!Z!";
			AssertNoMessageErrorContaining(moveHeader.BM_TOLCarrierCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBM_TOLCarrierID()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_TOLCarrierID = "123-12-1234";
			AssertNoMessageError(moveHeader.BM_TOLCarrierIDInfo, ValidationConstants.MoveHeader.InBondCarrierIDValid.ToString());
			moveHeader.BM_TOLCarrierID = "12-3456789XY";
			AssertNoMessageError(moveHeader.BM_TOLCarrierIDInfo, ValidationConstants.MoveHeader.InBondCarrierIDValid.ToString());
			moveHeader.BM_TOLCarrierID = "061234-12345";
			AssertNoMessageError(moveHeader.BM_TOLCarrierIDInfo, ValidationConstants.MoveHeader.InBondCarrierIDValid.ToString());
			moveHeader.BM_TOLCarrierID = "BND1234567";
			AssertHasMessageError(moveHeader.BM_TOLCarrierIDInfo, ValidationConstants.MoveHeader.InBondCarrierIDValid.ToString());
		}

		public void TestCheckBM_TOLStateCode()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_TOLStateCode = ZString.Empty;
			AssertNoMessageError(moveHeader.BM_TOLStateCodeInfo, ValidationConstants.MoveHeader.TOLStateCodeIsRequired.ToString());
			moveHeader.BM_TOLCityName = "LOS ANGELES";
			AssertHasMessageError(moveHeader.BM_TOLStateCodeInfo, ValidationConstants.MoveHeader.TOLStateCodeIsRequired.ToString());
			moveHeader.BM_TOLStateCode = "CA";
			AssertNoMessageError(moveHeader.BM_TOLStateCodeInfo, ValidationConstants.MoveHeader.TOLStateCodeIsRequired.ToString());
			moveHeader.BM_TOLStateCode = "XY";
			AssertHasMessageError(moveHeader.BM_TOLStateCodeInfo, "The code you have selected is not in the list.");
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			moveHeader.BM_TOLCityName = "LOS ANGELES";
			AssertNoMessageError(moveHeader.BM_TOLStateCodeInfo, ValidationConstants.MoveHeader.TOLStateCodeIsRequired.ToString());
		}

		public void TestCheckBM_InBondCarrierID()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader1 = header.MovementHeaders.AddNew();
			moveHeader1.BM_InBondCarrierID = "ID23";
			CusInBondMoveHeader moveHeader2 = header.MovementHeaders.AddNew();
			header.BH_PostDepartureOnly = true;
			moveHeader2.BM_InBondCarrierID = ZString.Empty;
			AssertNoMessageErrorContaining(moveHeader2.BM_InBondCarrierIDInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_PostDepartureOnly = false;
			moveHeader2.BM_InBondCarrierID = ZString.Empty;
			AssertHasMessageErrorContaining(moveHeader2.BM_InBondCarrierIDInfo, MandatoryValidation.YouHaveNotEntered);
			moveHeader2.BM_InBondCarrierID = "123-12-1234";
			AssertNoMessageErrorContaining(moveHeader2.BM_InBondCarrierIDInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(moveHeader2.BM_InBondCarrierIDInfo, ValidationConstants.MoveHeader.InBondCarrierIDValid.ToString());
			moveHeader2.BM_InBondCarrierID = "12-3456789XY";
			AssertNoMessageError(moveHeader2.BM_InBondCarrierIDInfo, ValidationConstants.MoveHeader.InBondCarrierIDValid.ToString());
			moveHeader2.BM_InBondCarrierID = "061234-12345";
			AssertNoMessageError(moveHeader2.BM_InBondCarrierIDInfo, ValidationConstants.MoveHeader.InBondCarrierIDValid.ToString());
			moveHeader2.BM_InBondCarrierID = "BND1234567";
			AssertHasMessageError(moveHeader2.BM_InBondCarrierIDInfo, ValidationConstants.MoveHeader.InBondCarrierIDValid.ToString());
			CustomsDataRegistry.Instance.PowerOfAttorneyNotificationType.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Customs.Business.PowerOfAttorneyNotificationTypeList.Codes.MessageError);
			PowerOfAttorneyValidator poa = new PowerOfAttorneyValidator();
			moveHeader2.BM_OA_InBondCarrier = ZGuid.Empty;
			moveHeader2.BM_InBondCarrierID = "ZZ";
			string receivedDateRequired = PowerOfAttorneyValidator.GetReceivedDateRequiredString(poa.CountrySpecificNameForPOA, "In-Bond");
			string poaNotValid = ValidationConstants.MoveHeader.GetNoPOADocumentForString(poa.CountrySpecificNameForPOA, "ZZ");
			string multipleCarriedIDsPOAWarning = ValidationConstants.MoveHeader.GetMultipleCarrierIDsPOAWarning(poa.CountrySpecificNameForPOA);
			AssertNoMessageErrorContaining(moveHeader2.BM_InBondCarrierIDInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(moveHeader2.BM_InBondCarrierIDInfo, receivedDateRequired);
			AssertHasMessageError(moveHeader2.BM_InBondCarrierIDInfo, poaNotValid);
			AssertNoWarning(moveHeader2.BM_InBondCarrierIDInfo, multipleCarriedIDsPOAWarning);
			JobRequiredDocument poaDocument = header.RequiredDocuments.AddNew(Core.Constants.RefDocTypes.PowerOfAttorney);
			poaDocument.EQ_DocDescription = "Power of Attorney";
			poaDocument.EQ_DocPeriod = Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment;
			poaDocument.EQ_DateReceived = ZDateTimeOffset.Empty;
			poaDocument.EQ_ValidToDate = ZDateTime.Empty;
			moveHeader2.Validation.ValidateBM_InBondCarrierID();
			AssertHasMessageErrorContaining(moveHeader2.BM_InBondCarrierIDInfo, receivedDateRequired);
			AssertNoMessageError(moveHeader2.BM_InBondCarrierIDInfo, poaNotValid);
			AssertHasWarning(moveHeader2.BM_InBondCarrierIDInfo, multipleCarriedIDsPOAWarning);
			poaDocument.EQ_DateReceived = ZDateTimeOffset.Today.AddMonths(-2);
			moveHeader2.Validation.ValidateBM_InBondCarrierID();
			AssertNoMessageErrorContaining(moveHeader2.BM_InBondCarrierIDInfo, receivedDateRequired);
			AssertNoMessageError(moveHeader2.BM_InBondCarrierIDInfo, poaNotValid);
			AssertHasWarning(moveHeader2.BM_InBondCarrierIDInfo, multipleCarriedIDsPOAWarning);
			OrgHeader importer = Factory.New<OrgHeader>();
			moveHeader2.BM_OA_InBondCarrier = importer.MainAddress.PK;
			poaDocument.EQ_DateReceived = ZDateTimeOffset.Empty;
			moveHeader2.Validation.ValidateBM_InBondCarrierID();
			AssertNoMessageErrorContaining(moveHeader2.BM_InBondCarrierIDInfo, receivedDateRequired);
			AssertNoMessageError(moveHeader2.BM_InBondCarrierIDInfo, poaNotValid);
			AssertNoWarning(moveHeader2.BM_InBondCarrierIDInfo, multipleCarriedIDsPOAWarning);
			moveHeader2.BM_OA_InBondCarrier = ZGuid.Empty;
			AssertHasMessageErrorContaining(moveHeader2.BM_InBondCarrierIDInfo, receivedDateRequired);
			AssertNoMessageError(moveHeader2.BM_InBondCarrierIDInfo, poaNotValid);
			AssertHasWarning(moveHeader2.BM_InBondCarrierIDInfo, multipleCarriedIDsPOAWarning);
			poaDocument.EQ_OH_DocumentOwner = importer.PK;
			moveHeader2.BM_InBondCarrierID = "ZZ";
			AssertNoMessageErrorContaining(moveHeader2.BM_InBondCarrierIDInfo, receivedDateRequired);
			AssertHasMessageError(moveHeader2.BM_InBondCarrierIDInfo, poaNotValid);
			AssertNoWarning(moveHeader2.BM_InBondCarrierIDInfo, multipleCarriedIDsPOAWarning);
			OrgCusCode cusCode = importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "ZZ");
			moveHeader2.BM_InBondCarrierID = "ZZ";
			AssertHasMessageErrorContaining(moveHeader2.BM_InBondCarrierIDInfo, receivedDateRequired);
			AssertNoMessageError(moveHeader2.BM_InBondCarrierIDInfo, poaNotValid);
			AssertNoWarning(moveHeader2.BM_InBondCarrierIDInfo, multipleCarriedIDsPOAWarning);
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.CBPAssignedNumber;
			moveHeader2.BM_InBondCarrierID = "ZZ";
			AssertHasMessageErrorContaining(moveHeader2.BM_InBondCarrierIDInfo, receivedDateRequired);
			AssertNoMessageError(moveHeader2.BM_InBondCarrierIDInfo, poaNotValid);
			AssertNoWarning(moveHeader2.BM_InBondCarrierIDInfo, multipleCarriedIDsPOAWarning);
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.SocialSecurityNumber;
			moveHeader2.BM_InBondCarrierID = "ZZ";
			AssertHasMessageErrorContaining(moveHeader2.BM_InBondCarrierIDInfo, receivedDateRequired);
			AssertNoMessageError(moveHeader2.BM_InBondCarrierIDInfo, poaNotValid);
			AssertNoWarning(moveHeader2.BM_InBondCarrierIDInfo, multipleCarriedIDsPOAWarning);
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.ManufacturerID;
			moveHeader2.BM_InBondCarrierID = "ZZ";
			AssertNoMessageErrorContaining(moveHeader2.BM_InBondCarrierIDInfo, receivedDateRequired);
			AssertHasMessageError(moveHeader2.BM_InBondCarrierIDInfo, poaNotValid);
			AssertNoWarning(moveHeader2.BM_InBondCarrierIDInfo, multipleCarriedIDsPOAWarning);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.ManufacturerID;
			moveHeader2.BM_InBondCarrierID = "ZZ";
			AssertNoMessageErrorContaining(moveHeader2.BM_InBondCarrierIDInfo, receivedDateRequired);
			AssertHasMessageError(moveHeader2.BM_InBondCarrierIDInfo, poaNotValid);
			AssertNoWarning(moveHeader2.BM_InBondCarrierIDInfo, multipleCarriedIDsPOAWarning);
			header.ValidationModes = ValidationModes.AirEntireInBondArrival;
			cusCode.OK_CodeType = OrgCusCode.USACodeTypes.ManufacturerID;
			moveHeader2.BM_InBondCarrierID = "ZZ";
			AssertNoMessageError(moveHeader2.BM_InBondCarrierIDInfo, poaNotValid);
		}

		public void TestCheckBM_MonetaryValue()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_MonetaryValue = ZDecimal.Zero;
			AssertNoMessageError(moveHeader.BM_MonetaryValueInfo, ValidationConstants.MoveHeader.MonetaryValueIsRequiredFor62Or63EntryType.ToString());
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			AssertHasMessageError(moveHeader.BM_MonetaryValueInfo, ValidationConstants.MoveHeader.MonetaryValueIsRequiredFor62Or63EntryType.ToString());
			header.BH_PostDepartureOnly = true;
			moveHeader.BM_MonetaryValue = ZDecimal.Zero;
			AssertNoMessageError(moveHeader.BM_MonetaryValueInfo, ValidationConstants.MoveHeader.MonetaryValueIsRequiredFor62Or63EntryType.ToString());
			header.BH_PostDepartureOnly = false;
			moveHeader.BM_MonetaryValue = 10m;
			AssertNoMessageError(moveHeader.BM_MonetaryValueInfo, ValidationConstants.MoveHeader.MonetaryValueIsRequiredFor62Or63EntryType.ToString());
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._3ImmediateExport;
			AssertNoMessageError(moveHeader.BM_MonetaryValueInfo, ValidationConstants.MoveHeader.MonetaryValueIsRequiredFor62Or63EntryType.ToString());
			moveHeader.BM_MonetaryValue = ZDecimal.Zero;
			AssertHasMessageError(moveHeader.BM_MonetaryValueInfo, ValidationConstants.MoveHeader.MonetaryValueIsRequiredFor62Or63EntryType.ToString());
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			AssertNoMessageError(moveHeader.BM_MonetaryValueInfo, ValidationConstants.MoveHeader.MonetaryValueIsRequiredFor62Or63EntryType.ToString());
			header.BH_HeaderType = InBondHeaderTypeList.Codes.DocumentOnly;
			moveHeader.BM_MonetaryValue = ZDecimal.Zero;
			AssertHasWarning(moveHeader.BM_MonetaryValueInfo, "Value In Whole Dollars cannot be zero.");
			moveHeader.BM_MonetaryValue = 1000m;
			AssertNoWarning(moveHeader.BM_MonetaryValueInfo, "Value In Whole Dollars cannot be zero.");
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			moveHeader.BM_MonetaryValue = ZDecimal.Zero;
			AssertHasMessageError(moveHeader.BM_MonetaryValueInfo, "Value In Whole Dollars cannot be zero.");
			moveHeader.BM_MonetaryValue = 1000m;
			AssertNoMessageError(moveHeader.BM_MonetaryValueInfo, "Value In Whole Dollars cannot be zero.");
			moveHeader.BM_MonetaryValue = 123456789m;
			AssertHasMessageError(moveHeader.BM_MonetaryValueInfo, ValidationConstants.MoveHeader.MaxValueExceeded.ToString());
			header.BH_HeaderType = InBondHeaderTypeList.Codes.DocumentOnly;
			moveHeader.BM_MonetaryValue = 123456788m;
			AssertNoMessageError(moveHeader.BM_MonetaryValueInfo, ValidationConstants.MoveHeader.MaxValueExceeded.ToString());
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			moveHeader.BM_MonetaryValue = ZDecimal.Zero;
			AssertHasMessageError(moveHeader.BM_MonetaryValueInfo, "Value In Whole Dollars cannot be zero.");
			header.ValidationModes = ValidationModes.AirEntireInBondArrival;
			moveHeader.BM_MonetaryValue = ZDecimal.Zero;
			AssertNoMessageError(moveHeader.BM_MonetaryValueInfo, "Value In Whole Dollars cannot be zero.");
		}

		public void TestCheckBM_BTAIndicator()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_BTAIndicator = "!";
			AssertHasMessageErrorContaining(moveHeader.BM_BTAIndicatorInfo, ListValidation.InvalidCodeMessageError);
			foreach (string indicator in new string[] { YesNoDefaultList.Codes.Yes, YesNoDefaultList.Codes.No, "" })
			{
				moveHeader.BM_BTAIndicator = indicator;
				AssertNoMessageErrorContaining(moveHeader.BM_BTAIndicatorInfo, ListValidation.InvalidCodeMessageError);
			}

			header.BH_PostDepartureOnly = true;
			moveHeader.BM_BTAIndicator = "";
			AssertNoMessageErrorContaining(moveHeader.BM_BTAIndicatorInfo, "You have not entered a BTA Indicator.");
			header.BH_PostDepartureOnly = false;
			moveHeader.BM_BTAIndicator = "";
			AssertHasMessageErrorContaining(moveHeader.BM_BTAIndicatorInfo, "You have not entered a BTA Indicator.");
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			moveHeader.BM_BTAIndicator = "!";
			AssertNoMessageErrorContaining(moveHeader.BM_BTAIndicatorInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBM_InBondCarrierSCAC()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "SC1Z";
			var header = Factory.New<CusInBondHeader>();
			header.BH_FTZMove = ZBool.False;
			var moveHeader = header.MovementHeaders.AddNew();
			header.BH_FIRMS = ZString.Empty;
			moveHeader.BM_InBondCarrierSCAC = ZString.Empty;
			AssertHasMessageErrorContaining(moveHeader.BM_InBondCarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_PostDepartureOnly = true;
			moveHeader.BM_InBondCarrierSCAC = ZString.Empty;
			AssertNoMessageErrorContaining(moveHeader.BM_InBondCarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_PostDepartureOnly = false;
			moveHeader.BM_InBondCarrierSCAC = "Z!Z!";
			AssertNoMessageErrorContaining(moveHeader.BM_InBondCarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(moveHeader.BM_InBondCarrierSCACInfo, ListValidation.InvalidCodeMessageError);
			moveHeader.BM_InBondCarrierSCAC = carrier.UI_Code;
			AssertNoMessageErrorContaining(moveHeader.BM_InBondCarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(moveHeader.BM_InBondCarrierSCACInfo, ListValidation.InvalidCodeMessageError);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.DocumentOnly;
			moveHeader.BM_InBondCarrierSCAC = "Z!Z!";
			AssertHasWarningContaining(moveHeader.BM_InBondCarrierSCACInfo, ListValidation.InvalidCodeMessage);
			moveHeader.BM_InBondCarrierSCAC = carrier.UI_Code;
			AssertNoWarningContaining(moveHeader.BM_InBondCarrierSCACInfo, ListValidation.InvalidCodeMessage);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			moveHeader.BM_InBondCarrierSCAC = ZString.Empty;
			AssertHasMessageErrorContaining(moveHeader.BM_InBondCarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			header.ValidationModes = ValidationModes.AirEntireInBondArrival;
			moveHeader.BM_InBondCarrierSCAC = "Z!Z!";
			AssertHasMessageErrorContaining(moveHeader.BM_InBondCarrierSCACInfo, ListValidation.InvalidCodeMessageError);
			moveHeader.BM_InBondCarrierSCAC = "Z!Z!";
			AssertHasMessageErrorContaining(moveHeader.BM_InBondCarrierSCACInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckThreeLetterInBondAirCarrierCode()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_InBondCarrierSCAC = "A1";
			moveHeader.ThreeLetterInBondAirCarrierCode = ZString.Empty;
			moveHeader.Validation.ValidateAll();
			AssertEquals(ZString.Empty, moveHeader.ThreeLetterInBondAirCarrierCode);
			AssertHasMessageError("The 2-Letter code should not exist in the Carrier table", moveHeader.BM_InBondCarrierSCACInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError("A 3-Letter Code should not be located using a 2-letter code lookup", moveHeader.ThreeLetterInBondAirCarrierCodeInfo, ValidationConstants.Header.CodeInvalidMessage.ToString());
			var carrier1 = Factory.New<USCarrierCombined>();
			carrier1.UI_Code = "A1";
			carrier1.UI_ModeOfTransportation = InBondTransportModeCodes.Codes.AirNonContainer;
			var refAirline1 = Factory.New<RefAirline>();
			refAirline1.RM_EagleAddedAirlinePrefixOrAccountingCode = "123";
			refAirline1.RM_TwoCharacterCode = "A1";
			refAirline1.RM_ThreeLetterCode = "AAA";
			moveHeader.Validation.ValidateAll();
			AssertEquals("The 3-Letter Code should have been auto-filled from a 2-letter code lookup", "AAA", moveHeader.ThreeLetterInBondAirCarrierCode);
			AssertNoMessageError("The 2-Letter code should exist in the Carrier table", moveHeader.BM_InBondCarrierSCACInfo, ValidationConstants.Header.CarrierCodeNotValidForTransportMode.ToString());
			AssertNoMessageError("A 3-Letter Code should be located using a 2-letter code lookup", moveHeader.ThreeLetterInBondAirCarrierCodeInfo, ValidationConstants.Header.CodeInvalidMessage.ToString());
			AssertNoWarning("A warning should not appear when the 3-Letter code is determined by the system", moveHeader.BM_InBondCarrierSCACInfo, ValidationConstants.Header.CodeIgnoredMessage.ToString());
			refAirline1.RM_ThreeLetterCode = "";
			moveHeader.Validation.ValidateAll();
			AssertEquals(ZString.Empty, moveHeader.ThreeLetterInBondAirCarrierCode);
			AssertHasMessageError("An error should show when the 2-letter code is invalid and the 3-Letter code is not set", moveHeader.ThreeLetterInBondAirCarrierCodeInfo, ValidationConstants.Header.CodeInvalidMessage.ToString());
			var carrier2 = Factory.New<USCarrierCombined>();
			carrier2.UI_Code = "CA";
			carrier2.UI_ModeOfTransportation = InBondTransportModeCodes.Codes.AirNonContainer;
			moveHeader.BM_InBondCarrierSCAC = "CA";
			moveHeader.ThreeLetterInBondAirCarrierCode = ZString.Empty;
			moveHeader.Validation.ValidateAll();
			AssertEquals(ZString.Empty, moveHeader.ThreeLetterInBondAirCarrierCode);
			AssertNoMessageError("An error should not show when the 2-letter code is valid and the 3-Letter code is not set", moveHeader.ThreeLetterInBondAirCarrierCodeInfo, ValidationConstants.Header.CodeInvalidMessage.ToString());
			AssertNoWarning("A warning should not appear when the 2-letter code is valid and the 3-Letter code is not set", moveHeader.BM_InBondCarrierSCACInfo, ValidationConstants.Header.CodeIgnoredMessage.ToString());
			var refAirline2 = Factory.New<RefAirline>();
			refAirline2.RM_EagleAddedAirlinePrefixOrAccountingCode = "456";
			refAirline2.RM_TwoCharacterCode = "CA";
			refAirline2.RM_ThreeLetterCode = "DDD";
			moveHeader.BM_InBondCarrierSCAC = "CA";
			moveHeader.ThreeLetterInBondAirCarrierCode = "DDD";
			moveHeader.Validation.ValidateAll();
			AssertHasWarning("A warning should appear when both the 2-letter and the 3-Letter codes are set by the user", moveHeader.BM_InBondCarrierSCACInfo, ValidationConstants.Header.CodeIgnoredMessage.ToString());
			moveHeader.BM_InBondCarrierSCAC = ZString.Empty;
			moveHeader.Validation.ValidateAll();
			AssertEquals("DDD", moveHeader.ThreeLetterInBondAirCarrierCode);
			AssertNoMessageErrorContaining(moveHeader.BM_InBondCarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			moveHeader.BM_InBondCarrierSCAC = ZString.Empty;
			moveHeader.ThreeLetterInBondAirCarrierCode = "DEF";
			moveHeader.Validation.ValidateAll();
			AssertEquals("DEF", moveHeader.ThreeLetterInBondAirCarrierCode);
			AssertHasMessageErrorContaining(moveHeader.ThreeLetterInBondAirCarrierCodeInfo, ValidationConstants.Header.CarrierCodeNotValidForTransportMode.ToString());
			moveHeader.BM_InBondCarrierSCAC = ZString.Empty;
			moveHeader.ThreeLetterInBondAirCarrierCode = ZString.Empty;
			moveHeader.Validation.ValidateAll();
			AssertHasMessageErrorContaining(moveHeader.BM_InBondCarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckThreeLetterSplitAirCarrierCode()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_SplitCarrierSCAC = "A1";
			moveHeader.ThreeLetterSplitAirCarrierCode = ZString.Empty;
			moveHeader.Validation.ValidateAll();
			AssertEquals(ZString.Empty, moveHeader.ThreeLetterSplitAirCarrierCode);
			AssertHasMessageError("The 2-Letter code should not exist in the Carrier table", moveHeader.BM_SplitCarrierSCACInfo, ValidationConstants.Header.CarrierCodeNotValidForTransportMode.ToString());
			AssertHasMessageError("A 3-Letter Code should not be located using a 2-letter code lookup", moveHeader.ThreeLetterSplitAirCarrierCodeInfo, ValidationConstants.Header.CodeInvalidMessage.ToString());
			var carrier1 = Factory.New<USCarrierCombined>();
			carrier1.UI_Code = "A1";
			carrier1.UI_ModeOfTransportation = InBondTransportModeCodes.Codes.AirNonContainer;
			var refAirline1 = Factory.New<RefAirline>();
			refAirline1.RM_EagleAddedAirlinePrefixOrAccountingCode = "123";
			refAirline1.RM_TwoCharacterCode = "A1";
			refAirline1.RM_ThreeLetterCode = "AAA";
			moveHeader.Validation.ValidateAll();
			AssertEquals("The 3-Letter Code should have been auto-filled from a 2-letter code lookup", "AAA", moveHeader.ThreeLetterSplitAirCarrierCode);
			AssertNoMessageError("The 2-Letter code should exist in the Carrier table", moveHeader.BM_SplitCarrierSCACInfo, ValidationConstants.Header.CarrierCodeNotValidForTransportMode.ToString());
			AssertNoMessageError("A 3-Letter Code should be located using a 2-letter code lookup", moveHeader.ThreeLetterSplitAirCarrierCodeInfo, ValidationConstants.Header.CodeInvalidMessage.ToString());
			AssertNoWarning("A warning should not appear when the 3-Letter code is determined by the system", moveHeader.BM_SplitCarrierSCACInfo, ValidationConstants.Header.CodeIgnoredMessage.ToString());
			refAirline1.RM_ThreeLetterCode = "";
			moveHeader.Validation.ValidateAll();
			AssertEquals(ZString.Empty, moveHeader.ThreeLetterSplitAirCarrierCode);
			AssertHasMessageError("An error should show when the 2-letter code is invalid and the 3-Letter code is not set", moveHeader.ThreeLetterSplitAirCarrierCodeInfo, ValidationConstants.Header.CodeInvalidMessage.ToString());
			var carrier2 = Factory.New<USCarrierCombined>();
			carrier2.UI_Code = "CA";
			carrier2.UI_ModeOfTransportation = InBondTransportModeCodes.Codes.AirNonContainer;
			moveHeader.BM_SplitCarrierSCAC = "CA";
			moveHeader.ThreeLetterSplitAirCarrierCode = ZString.Empty;
			moveHeader.Validation.ValidateAll();
			AssertEquals(ZString.Empty, moveHeader.ThreeLetterSplitAirCarrierCode);
			AssertNoMessageError("An error should not show when the 2-letter code is valid and the 3-Letter code is not set", moveHeader.ThreeLetterSplitAirCarrierCodeInfo, ValidationConstants.Header.CodeInvalidMessage.ToString());
			AssertNoWarning("A warning should not appear when the 2-letter code is valid and the 3-Letter code is not set", moveHeader.BM_SplitCarrierSCACInfo, ValidationConstants.Header.CodeIgnoredMessage.ToString());
			var refAirline2 = Factory.New<RefAirline>();
			refAirline2.RM_EagleAddedAirlinePrefixOrAccountingCode = "456";
			refAirline2.RM_TwoCharacterCode = "CA";
			refAirline2.RM_ThreeLetterCode = "DDD";
			moveHeader.BM_SplitCarrierSCAC = "CA";
			moveHeader.ThreeLetterSplitAirCarrierCode = "DDD";
			moveHeader.Validation.ValidateAll();
			AssertHasWarning("A warning should appear when both the 2-letter and the 3-Letter codes are set by the user", moveHeader.BM_SplitCarrierSCACInfo, ValidationConstants.Header.CodeIgnoredMessage.ToString());
			moveHeader.BM_SplitCarrierSCAC = ZString.Empty;
			moveHeader.Validation.ValidateAll();
			AssertEquals("DDD", moveHeader.ThreeLetterSplitAirCarrierCode);
			AssertNoMessageErrorContaining(moveHeader.BM_SplitCarrierSCACInfo, ValidationConstants.MoveHeader.SplitCarrierCodeForAIR.ToString());
			moveHeader.BM_SplitCarrierSCAC = ZString.Empty;
			moveHeader.ThreeLetterSplitAirCarrierCode = "DEF";
			moveHeader.Validation.ValidateAll();
			AssertEquals("DEF", moveHeader.ThreeLetterSplitAirCarrierCode);
			AssertHasMessageErrorContaining(moveHeader.ThreeLetterSplitAirCarrierCodeInfo, ValidationConstants.Header.CarrierCodeNotValidForTransportMode.ToString());
			moveHeader.BM_SplitCarrierSCAC = ZString.Empty;
			moveHeader.ThreeLetterSplitAirCarrierCode = ZString.Empty;
			moveHeader.Validation.ValidateAll();
			AssertHasMessageErrorContaining(moveHeader.BM_SplitCarrierSCACInfo, ValidationConstants.MoveHeader.SplitCarrierCodeForAIR.ToString());
		}

		public void TestCheckBM_DestinationPortCode()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_PortUnladingDCode = "3901";
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_DestinationPortCode = ZString.Empty;
			AssertHasMessageErrorContaining(moveHeader.BM_DestinationPortCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(moveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationShouldMatchPortOfArrivalFor63EntryType.ToString());
			moveHeader.BM_DestinationPortCode = "Z!Z!";
			AssertNoMessageErrorContaining(moveHeader.BM_DestinationPortCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(moveHeader.BM_DestinationPortCodeInfo, ListValidation.InvalidCodeMessageError);

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Z12Z", "Test Name", startDate, endDate);
			newFactory.Save();

			moveHeader.BM_DestinationPortCode = "Z12Z";
			AssertNoMessageErrorContaining(moveHeader.BM_DestinationPortCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(moveHeader.BM_DestinationPortCodeInfo, ListValidation.InvalidCodeMessageError);
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._3ImmediateExport;
			moveHeader.BM_DestinationPortCode = "3901";
			AssertNoMessageError(moveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationShouldMatchPortOfArrivalFor63EntryType.ToString());
			moveHeader.BM_DestinationPortCode = "Z12Z";
			AssertHasMessageError(moveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationShouldMatchPortOfArrivalFor63EntryType.ToString());
			header.BH_PostDepartureOnly = true;
			moveHeader.Validation.ValidateBM_DestinationPortCode();
			AssertNoMessageError(moveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationShouldMatchPortOfArrivalFor63EntryType.ToString());
			header.BH_PostDepartureOnly = false;
			var moveDetails = moveHeader.MovementDetails.AddNew();
			moveDetails.B9_PreviousITNumber = "123456789";
			moveHeader.Validation.ValidateBM_DestinationPortCode();
			AssertNoMessageError(moveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationShouldMatchPortOfArrivalFor63EntryType.ToString());
			moveDetails.B9_PreviousITNumber = ZString.Empty;
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			AssertNoMessageError(moveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationShouldMatchPortOfArrivalFor63EntryType.ToString());
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			AssertNoMessageError(moveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationShouldMatchPortOfArrivalFor63EntryType.ToString());
			moveHeader.BM_DestinationPortCode = "3901";
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			AssertHasMessageError(moveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationShouldNotMatchPortOfArrivalFor61EntryType.ToString());
			header.BH_PostDepartureOnly = true;
			moveHeader.Validation.ValidateBM_DestinationPortCode();
			AssertNoMessageError(moveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationShouldNotMatchPortOfArrivalFor61EntryType.ToString());
			header.BH_PostDepartureOnly = false;
			header.BH_PortUnladingDCode = ZString.Empty;
			moveHeader.BM_DestinationPortCode = ZString.Empty;
			AssertNoMessageError(moveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationShouldNotMatchPortOfArrivalFor61EntryType.ToString());
			header.BH_PortUnladingDCode = "3901";
			moveHeader.BM_DestinationPortCode = "Z12Z";
			AssertNoMessageError(moveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationShouldNotMatchPortOfArrivalFor61EntryType.ToString());
			moveHeader.BM_DestinationPortCode = "3901";
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			AssertNoMessageError(moveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationShouldNotMatchPortOfArrivalFor61EntryType.ToString());
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._3ImmediateExport;
			AssertNoMessageError(moveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationShouldNotMatchPortOfArrivalFor61EntryType.ToString());
			header.BH_HeaderType = InBondHeaderTypeList.Codes.DocumentOnly;
			moveHeader.BM_DestinationPortCode = "Z!Z!";
			AssertHasWarningContaining(moveHeader.BM_DestinationPortCodeInfo, ListValidation.InvalidCodeMessage);
			moveHeader.BM_DestinationPortCode = "Z12Z";
			AssertNoWarningContaining(moveHeader.BM_DestinationPortCodeInfo, ListValidation.InvalidCodeMessage);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			moveHeader.BM_DestinationPortCode = ZString.Empty;
			AssertHasMessageErrorContaining(moveHeader.BM_DestinationPortCodeInfo, MandatoryValidation.YouHaveNotEntered);
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._3ImmediateExport;
			moveHeader.BM_DestinationPortCode = "Z12Z";
			AssertHasMessageError(moveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationShouldMatchPortOfArrivalFor63EntryType.ToString());
			header.BH_PostDepartureOnly = true;
			moveHeader.Validation.ValidateBM_DestinationPortCode();
			AssertNoMessageError(moveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationShouldMatchPortOfArrivalFor63EntryType.ToString());
			header.BH_PostDepartureOnly = false;
			moveHeader.BM_DestinationPortCode = "3901";
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			AssertHasMessageError(moveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationShouldNotMatchPortOfArrivalFor61EntryType.ToString());
			header.BH_PostDepartureOnly = true;
			moveHeader.Validation.ValidateBM_DestinationPortCode();
			AssertNoMessageError(moveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationShouldNotMatchPortOfArrivalFor61EntryType.ToString());
			header.BH_PostDepartureOnly = false;
			moveHeader.BM_DestinationPortCode = "3901";
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			AssertHasMessageError(moveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationCanNotMatchPortOfArrivalFor62EntryType.ToString());
			header.BH_PostDepartureOnly = true;
			moveHeader.Validation.ValidateBM_DestinationPortCode();
			AssertNoMessageError(moveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationCanNotMatchPortOfArrivalFor62EntryType.ToString());
		}

		public void TestCheckBM_ForeignDestPortKCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var foreignPort = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Z!123", "Z!123", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.Common);
			var foreignPort1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "R!456", "R!456", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort1.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.InBond);
			Factory.Save();
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = ZString.Empty;
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			moveHeader.BM_ForeignDestPortKCode = ZString.Empty;
			AssertNoMessageErrorContaining(moveHeader.BM_ForeignDestPortKCodeInfo, MandatoryValidation.YouHaveNotEntered);
			moveHeader.BM_ForeignDestPortKCode = "Z!123";
			AssertNoMessageErrorContaining(moveHeader.BM_ForeignDestPortKCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(moveHeader.BM_ForeignDestPortKCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(moveHeader.BM_ForeignDestPortKCodeInfo, ValidationConstants.MoveHeader.ForeignDestinationIsOnlyRequiredFor62Or63EntryType.ToString());
			moveHeader.BM_ForeignDestPortKCode = "R!456";
			AssertNoMessageErrorContaining(moveHeader.BM_ForeignDestPortKCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(moveHeader.BM_ForeignDestPortKCodeInfo, ValidationConstants.MoveHeader.ForeignDestinationIsOnlyRequiredFor62Or63EntryType.ToString());
			moveHeader.BM_ForeignDestPortKCode = "Z!Z!";
			AssertNoMessageErrorContaining(moveHeader.BM_ForeignDestPortKCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(moveHeader.BM_ForeignDestPortKCodeInfo, ValidationConstants.MoveHeader.ForeignDestinationIsOnlyRequiredFor62Or63EntryType.ToString());
			foreach (ZString entryType in new ZString[] { InbondCommonTypeList.Codes._2TransportandExport, InbondCommonTypeList.Codes._3ImmediateExport })
			{
				moveHeader.BM_InBondEntryType = entryType;
				moveHeader.BM_ForeignDestPortKCode = ZString.Empty;
				AssertHasMessageErrorContaining(moveHeader.BM_ForeignDestPortKCodeInfo, MandatoryValidation.YouHaveNotEntered);
				moveHeader.BM_ForeignDestPortKCode = "Z!123";
				AssertNoMessageErrorContaining(moveHeader.BM_ForeignDestPortKCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(moveHeader.BM_ForeignDestPortKCodeInfo, ListValidation.InvalidCodeMessageError);
				AssertNoMessageError(moveHeader.BM_ForeignDestPortKCodeInfo, ValidationConstants.MoveHeader.ForeignDestinationIsOnlyRequiredFor62Or63EntryType.ToString());
				moveHeader.BM_ForeignDestPortKCode = "R!456";
				AssertNoMessageErrorContaining(moveHeader.BM_ForeignDestPortKCodeInfo, ListValidation.InvalidCodeMessageError);
				AssertNoMessageError(moveHeader.BM_ForeignDestPortKCodeInfo, ValidationConstants.MoveHeader.ForeignDestinationIsOnlyRequiredFor62Or63EntryType.ToString());
				moveHeader.BM_ForeignDestPortKCode = "Z!Z!";
				AssertHasMessageErrorContaining(moveHeader.BM_ForeignDestPortKCodeInfo, ListValidation.InvalidCodeMessageError);
				AssertNoMessageError(moveHeader.BM_ForeignDestPortKCodeInfo, ValidationConstants.MoveHeader.ForeignDestinationIsOnlyRequiredFor62Or63EntryType.ToString());
			}

			foreach (ZString transportMode in new ZString[] {
				InBondTransportModeCodes.Codes.RailContainer,
				InBondTransportModeCodes.Codes.RailNonContainer,
				InBondTransportModeCodes.Codes.TruckContainer,
				InBondTransportModeCodes.Codes.TruckNonContainer
			})
			{
				header.BH_ImportTransportMode = transportMode;
				foreach (ZString entryType in new ZString[] { InbondCommonTypeList.Codes._2TransportandExport, InbondCommonTypeList.Codes._3ImmediateExport })
				{
					moveHeader.BM_InBondEntryType = entryType;
					moveHeader.BM_ForeignDestPortKCode = "Z!123";
					AssertNoMessageErrorContaining(moveHeader.BM_ForeignDestPortKCodeInfo, ListValidation.InvalidCodeMessageError);
					AssertNoMessageError(moveHeader.BM_ForeignDestPortKCodeInfo, ValidationConstants.MoveHeader.ForeignDestinationIsOnlyRequiredFor62Or63EntryType.ToString());
					moveHeader.BM_ForeignDestPortKCode = "R!456";
					AssertNoMessageErrorContaining(moveHeader.BM_ForeignDestPortKCodeInfo, ListValidation.InvalidCodeMessageError);
					AssertNoMessageError(moveHeader.BM_ForeignDestPortKCodeInfo, ValidationConstants.MoveHeader.ForeignDestinationIsOnlyRequiredFor62Or63EntryType.ToString());
					moveHeader.BM_ForeignDestPortKCode = "Z!Z!";
					AssertHasMessageErrorContaining(moveHeader.BM_ForeignDestPortKCodeInfo, ListValidation.InvalidCodeMessageError);
					AssertNoMessageError(moveHeader.BM_ForeignDestPortKCodeInfo, ValidationConstants.MoveHeader.ForeignDestinationIsOnlyRequiredFor62Or63EntryType.ToString());
				}
			}

			header.BH_HeaderType = InBondHeaderTypeList.Codes.DocumentOnly;
			moveHeader.BM_ForeignDestPortKCode = "R!456";
			AssertNoWarningContaining(moveHeader.BM_ForeignDestPortKCodeInfo, ListValidation.InvalidCodeMessage);
			moveHeader.BM_ForeignDestPortKCode = "Z!Z!";
			AssertHasWarningContaining(moveHeader.BM_ForeignDestPortKCodeInfo, ListValidation.InvalidCodeMessage);
			moveHeader.BM_ForeignDestPortKCode = "Z!123";
			AssertNoMessageErrors(moveHeader.BM_ForeignDestPortKCodeInfo);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			moveHeader.BM_ForeignDestPortKCode = "R!456";
			AssertNoMessageError(moveHeader.BM_ForeignDestPortKCodeInfo, ValidationConstants.MoveHeader.ForeignDestinationIsOnlyRequiredFor62Or63EntryType.ToString());
		}

		public void TestCheckBM_RL_NKForeignDestPort()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "!ZZZ";
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			header.BH_HeaderType = InBondHeaderTypeList.Codes.DocumentOnly;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_RL_NKForeignDestPort = "!ZZ1";
			AssertHasWarningContaining(moveHeader.BM_RL_NKForeignDestPortInfo, ListValidation.InvalidCodeMessage);
			moveHeader.BM_RL_NKForeignDestPort = "!ZZZ";
			AssertNoWarningContaining(moveHeader.BM_RL_NKForeignDestPortInfo, ListValidation.InvalidCodeMessage);
			var mxUnloco = Factory.New<RefUNLOCO>();
			mxUnloco.RL_Code = "MXSCX";
			var auUnloco = Factory.New<RefUNLOCO>();
			auUnloco.RL_Code = "AUSYD";
			moveHeader.BM_RL_NKForeignDestPort = "MXSCX";
			AssertHasMessageErrorContaining(moveHeader.BM_RL_NKForeignDestPortInfo, ValidationConstants.MoveHeader.MexicanPedimentoNumberRequired.ToString());
			moveHeader.BM_RL_NKForeignDestPort = "AUSYD";
			AssertNoMessageErrorContaining(moveHeader.BM_RL_NKForeignDestPortInfo, ValidationConstants.MoveHeader.MexicanPedimentoNumberRequired.ToString());
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var bill = moveHeader.Header.Bills.AddNew();
			moveDetail.B9_B0 = bill.PK;
			var additionalRef = bill.AdditionalReferences.AddNew();
			additionalRef.BR_Qualifier = "FEN";
			additionalRef.BR_ReferenceNum = "2222";
			moveHeader.BM_RL_NKForeignDestPort = "MXSCX";
			AssertNoMessageErrorContaining(moveHeader.BM_RL_NKForeignDestPortInfo, ValidationConstants.MoveHeader.MexicanPedimentoNumberRequired.ToString());
			moveHeader.BM_RL_NKForeignDestPort = "AUSYD";
			AssertNoMessageErrorContaining(moveHeader.BM_RL_NKForeignDestPortInfo, ValidationConstants.MoveHeader.MexicanPedimentoNumberRequired.ToString());
		}

		public void TestCheckBM_PedimentoNumber()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = TransportModeCodes.Codes.AirNonContainer;
			var moveHeader = header.MovementHeaders.AddNew();
			moveHeader.BM_PedimentoNumber = "1234";
			AssertHasMessageError(moveHeader.BM_PedimentoNumberInfo, PedimentoNumberValidator.PedimentoNumberRightFormat);
			moveHeader.BM_PedimentoNumber = "123456789012345";
			AssertNoMessageError(moveHeader.BM_PedimentoNumberInfo, PedimentoNumberValidator.PedimentoNumberRightFormat);
		}

		protected override US.Business.CusInBondMoveHeader CreateNewCusInBondMoveHeader(Customs.Business.CusInBondHeader header) => ((CusInBondHeader)header).MovementHeaders.AddNew();

		protected override Customs.Business.CusInBondHeader CreateNewCusInBondHeader() => Factory.New<CusInBondHeader>();
	}
}
