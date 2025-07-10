using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(CusInBondMoveHeaderValidation))] //TODO: is it? CreateNewCusInBondMoveHeader does not create that type
	sealed class CusInBondMoveHeaderValidationTest : US.Business.Testing.CusInBondMoveHeaderValidationTest
	{
		public void TestCheckBM_ManifestSequenceNumber()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			var moveHeader = header.MovementHeader;
			moveHeader.ValidationModes = ValidationModes.InventoryRecordAmendment;
			moveHeader.BM_ManifestSequenceNumber = ZString.Empty;
			AssertHasMessageError(moveHeader.BM_ManifestSequenceNumberInfo, ValidationConstants.MoveHeader.ManifestSequenceNumberIsRequired.ToString());
			moveHeader.BM_ManifestSequenceNumber = "000001";
			AssertNoMessageError(moveHeader.BM_ManifestSequenceNumberInfo, ValidationConstants.MoveHeader.ManifestSequenceNumberIsRequired.ToString());
			moveHeader.ValidationModes = ValidationModes.InventoryRecord;
			moveHeader.BM_ManifestSequenceNumber = ZString.Empty;
			AssertNoMessageError(moveHeader.BM_ManifestSequenceNumberInfo, ValidationConstants.MoveHeader.ManifestSequenceNumberIsRequired.ToString());
			moveHeader.ValidationModes = ValidationModes.InventoryRecordAmendment;
			moveHeader.BM_ManifestSequenceNumber = ZString.Empty;
			AssertHasMessageError(moveHeader.BM_ManifestSequenceNumberInfo, ValidationConstants.MoveHeader.ManifestSequenceNumberIsRequired.ToString());
		}

		public void TestValidateInBondNumberForInBondMovementOnly()
		{
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var otherBranch = currentCompany.Branches.AddNew();
			otherBranch.GB_Code = "~Z~";
			var numberRange = USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange.GetValueWithoutFallback(Guid.Empty, otherBranch.PK.ToGuid(), Guid.Empty);
			numberRange.StartNumber = 0;
			AssertEquals(false, numberRange.IsRangeValidForNumberFountain);
			var branchErrorMessage = InBondNumberAvailabilityChecker.InBondNumberRangeNotSetup(((IRegistryItemInternals)USCustomsDataRegistry.Instance.CompanyOrBranchInBondNumberRange).Location);
			var header = Factory.New<CusInBondHeader>();
			header.BH_GB = otherBranch.PK;
			var amsMoveHeader = header.MovementHeader;
			AssertEquals(false, amsMoveHeader.IsInBondMovement);
			AssertNoMessageError(amsMoveHeader.InBondNumberInfo, branchErrorMessage);
			var pttMoveHeader = header.PTTMovements.AddNew();
			AssertEquals(false, pttMoveHeader.IsInBondMovement);
			AssertNoMessageError(pttMoveHeader.InBondNumberInfo, branchErrorMessage);
			var inBondMoveHeader = header.InBondMovementHeaders.AddNew();
			inBondMoveHeader.InBondNumber = ZString.Empty;
			AssertEquals(true, inBondMoveHeader.IsInBondMovement);
			AssertHasMessageError(inBondMoveHeader.InBondNumberInfo, branchErrorMessage);
		}

		public void TestValidateAtLeastOneMovementDetailsExist()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.ValidationModes = ValidationModes.InBondArrival;
			var moveHeader = header.MovementHeader;
			moveHeader.Validation.ValidateAll();
			AssertNoRowMessageError(moveHeader, ValidationConstants.MoveHeader.AtLeastOneMoveDetailIsRequired);
			var bill = header.Bills.AddNew();
			var inBondMoveHeader = header.InBondMovementHeaders.AddNew();
			inBondMoveHeader.Validation.ValidateAll();
			AssertHasRowMessageError(inBondMoveHeader, ValidationConstants.MoveHeader.AtLeastOneMoveDetailIsRequired);
			header.ValidationModes = ValidationModes.PermitToTransfer;
			inBondMoveHeader.Validation.ValidateAll();
			AssertNoRowMessageError(moveHeader, ValidationConstants.MoveHeader.AtLeastOneMoveDetailIsRequired);
			header.ValidationModes = ValidationModes.InBondExportation;
			inBondMoveHeader.Validation.ValidateAll();
			AssertHasRowMessageError(inBondMoveHeader, ValidationConstants.MoveHeader.AtLeastOneMoveDetailIsRequired);
			var moveDetail = inBondMoveHeader.MovementDetails.AddNew(bill.PK);
			inBondMoveHeader.Validation.ValidateAll();
			AssertNoRowMessageError(moveHeader, ValidationConstants.MoveHeader.AtLeastOneMoveDetailIsRequired);
		}

		public void TestCheckBM_InBondCarrierID()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			header.ValidationModes = ValidationModes.PermitToTransfer;
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeader;
			moveHeader.BM_InBondCarrierID = ZString.Empty;
			AssertNoNotifications(moveHeader.BM_InBondCarrierIDInfo);
			header.ValidationModes = ValidationModes.PermitToTransfer;
			var pttMoveHeader = header.PTTMovements.AddNew();
			pttMoveHeader.BM_InBondCarrierID = ZString.Empty;
			AssertHasMessageErrorContaining(pttMoveHeader.BM_InBondCarrierIDInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(pttMoveHeader.BM_InBondCarrierIDInfo, ValidationConstants.MoveHeader.InBondCarrierIDValid.ToString());
			AssertNoMessageError(pttMoveHeader.BM_InBondCarrierIDInfo, ValidationConstants.MoveHeader.AtLeastOneMoveDetailIsRequired.ToString());
			pttMoveHeader.BM_InBondCarrierID = "12-1ABC56789";
			AssertNoMessageErrorContaining(pttMoveHeader.BM_InBondCarrierIDInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(pttMoveHeader.BM_InBondCarrierIDInfo, ValidationConstants.MoveHeader.InBondCarrierIDValid.ToString());
			AssertHasMessageError(pttMoveHeader.BM_InBondCarrierIDInfo, ValidationConstants.MoveHeader.AtLeastOneMoveDetailIsRequired.ToString());
			var pttMoveDetail = pttMoveHeader.MovementDetails.AddNew(bill.PK);
			pttMoveHeader.BM_InBondCarrierID = "12-1ABC56789";
			AssertNoMessageErrorContaining(pttMoveHeader.BM_InBondCarrierIDInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(pttMoveHeader.BM_InBondCarrierIDInfo, ValidationConstants.MoveHeader.InBondCarrierIDValid.ToString());
			AssertNoMessageError(pttMoveHeader.BM_InBondCarrierIDInfo, ValidationConstants.MoveHeader.AtLeastOneMoveDetailIsRequired.ToString());
			var validCodes = new[] { "123-12-1234", "12-3456789XY", "061234-12345" };
			foreach (var code in validCodes)
			{
				pttMoveHeader.BM_InBondCarrierID = code;
				AssertNoMessageErrorContaining(pttMoveHeader.BM_InBondCarrierIDInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageError(pttMoveHeader.BM_InBondCarrierIDInfo, ValidationConstants.MoveHeader.InBondCarrierIDValid.ToString());
				AssertNoMessageError(pttMoveHeader.BM_InBondCarrierIDInfo, ValidationConstants.MoveHeader.AtLeastOneMoveDetailIsRequired.ToString());
			}

			var inBondMoveHeader = header.InBondMovementHeaders.AddNew();
			foreach (var validationMode in new[] { ValidationModes.InBondDiversion, ValidationModes.SubsequentInBond })
			{
				header.ValidationModes = validationMode;
				inBondMoveHeader.BM_InBondCarrierID = ZString.Empty;
				AssertHasMessageErrorContaining(inBondMoveHeader.BM_InBondCarrierIDInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageError(inBondMoveHeader.BM_InBondCarrierIDInfo, ValidationConstants.MoveHeader.InBondCarrierIDValid.ToString());
				inBondMoveHeader.BM_InBondCarrierID = "12-1ABC56789";
				AssertNoMessageErrorContaining(inBondMoveHeader.BM_InBondCarrierIDInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageError(inBondMoveHeader.BM_InBondCarrierIDInfo, ValidationConstants.MoveHeader.InBondCarrierIDValid.ToString());
				AssertNoMessageError(inBondMoveHeader.BM_InBondCarrierIDInfo, ValidationConstants.MoveHeader.AtLeastOneMoveDetailIsRequired.ToString());
				foreach (var code in validCodes)
				{
					inBondMoveHeader.BM_InBondCarrierID = code;
					AssertNoMessageErrorContaining(inBondMoveHeader.BM_InBondCarrierIDInfo, MandatoryValidation.YouHaveNotEntered);
					AssertNoMessageError(inBondMoveHeader.BM_InBondCarrierIDInfo, ValidationConstants.MoveHeader.InBondCarrierIDValid.ToString());
				}
			}
		}

		public void TestCheckBM_ArrivalDate()
		{
			Header.ValidationModes = ValidationModes.InBondArrival;
			var moveHeader = Header.MovementHeader;
			AssertEquals(true, moveHeader.IsAMSMovement);
			moveHeader.BM_ArrivalDate = ZDateTime.Empty;
			AssertNoMessageErrorContaining(moveHeader.BM_ArrivalDateInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(moveHeader.BM_ArrivalDateInfo, ValidationConstants.MoveHeader.ArrivalDateExceedsTodaysDate.ToString());
			var inBondMoveHeader = Header.InBondMovementHeaders.AddNew();
			inBondMoveHeader.BM_ArrivalDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(inBondMoveHeader.BM_ArrivalDateInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(inBondMoveHeader.BM_ArrivalDateInfo, ValidationConstants.MoveHeader.ArrivalDateExceedsTodaysDate.ToString());
			inBondMoveHeader.BM_ArrivalDate = ZDateTime.Now.AddMonths(1);
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_ArrivalDateInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(inBondMoveHeader.BM_ArrivalDateInfo, ValidationConstants.MoveHeader.ArrivalDateExceedsTodaysDate.ToString());
			inBondMoveHeader.BM_ArrivalDate = ZDateTime.BrettsBirthday;
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_ArrivalDateInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(inBondMoveHeader.BM_ArrivalDateInfo, ValidationConstants.MoveHeader.ArrivalDateExceedsTodaysDate.ToString());
		}

		public void TestCheckBM_TOLCarrierID()
		{
			Header.ValidationModes = ValidationModes.InBondTOL;
			var moveHeader = Header.MovementHeader;
			AssertEquals(true, moveHeader.IsAMSMovement);
			moveHeader.BM_TOLCarrierID = ZString.Empty;
			AssertNoMessageErrorContaining(moveHeader.BM_TOLCarrierIDInfo, MandatoryValidation.YouHaveNotEntered);
			var inBondMoveHeader = Header.InBondMovementHeaders.AddNew();
			inBondMoveHeader.BM_TOLCarrierID = ZString.Empty;
			AssertHasMessageErrorContaining(inBondMoveHeader.BM_TOLCarrierIDInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(inBondMoveHeader.BM_TOLCarrierIDInfo, ValidationConstants.MoveHeader.InBondCarrierIDValid.ToString());
			inBondMoveHeader.BM_TOLCarrierID = "12-1ABC56789";
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_TOLCarrierIDInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageError(inBondMoveHeader.BM_TOLCarrierIDInfo, ValidationConstants.MoveHeader.InBondCarrierIDValid.ToString());
			var validCodes = new[] { "123-12-1234", "12-3456789XY", "061234-12345" };
			foreach (var code in validCodes)
			{
				inBondMoveHeader.BM_TOLCarrierID = code;
				AssertNoMessageErrorContaining(inBondMoveHeader.BM_TOLCarrierIDInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageError(inBondMoveHeader.BM_TOLCarrierIDInfo, ValidationConstants.MoveHeader.InBondCarrierIDValid.ToString());
			}
		}

		public void TestCheckBM_TOLCarrierCode()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "X23X";
			Header.ValidationModes = ValidationModes.InBondTOL;
			var moveHeader = Header.MovementHeader;
			AssertEquals(true, moveHeader.IsAMSMovement);
			moveHeader.BM_TOLCarrierCode = ZString.Empty;
			AssertNoMessageErrorContaining(moveHeader.BM_TOLCarrierCodeInfo, MandatoryValidation.YouHaveNotEntered);
			var inBondMoveHeader = Header.InBondMovementHeaders.AddNew();
			inBondMoveHeader.BM_TOLCarrierCode = ZString.Empty;
			AssertHasMessageErrorContaining(inBondMoveHeader.BM_TOLCarrierCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_TOLCarrierCodeInfo, ListValidation.InvalidCodeMessageError);
			inBondMoveHeader.BM_TOLCarrierCode = "@#!@";
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_TOLCarrierCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(inBondMoveHeader.BM_TOLCarrierCodeInfo, ListValidation.InvalidCodeMessageError);
			inBondMoveHeader.BM_TOLCarrierCode = "X23X";
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_TOLCarrierCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_TOLCarrierCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBM_TOLCityName()
		{
			Header.ValidationModes = ValidationModes.InBondTOL;
			var moveHeader = Header.MovementHeader;
			AssertEquals(true, moveHeader.IsAMSMovement);
			moveHeader.BM_TOLCityName = ZString.Empty;
			AssertNoMessageErrorContaining(moveHeader.BM_TOLCityNameInfo, MandatoryValidation.YouHaveNotEntered);
			var inBondMoveHeader = Header.InBondMovementHeaders.AddNew();
			inBondMoveHeader.BM_TOLCityName = ZString.Empty;
			AssertHasMessageErrorContaining(inBondMoveHeader.BM_TOLCityNameInfo, MandatoryValidation.YouHaveNotEntered);
			inBondMoveHeader.BM_TOLCityName = "BOB'S CITY";
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_TOLCityNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBM_TOLDate()
		{
			Header.ValidationModes = ValidationModes.InBondTOL;
			var moveHeader = Header.MovementHeader;
			AssertEquals(true, moveHeader.IsAMSMovement);
			moveHeader.BM_TOLDate = ZDateTime.Empty;
			AssertNoMessageErrorContaining(moveHeader.BM_TOLDateInfo, MandatoryValidation.YouHaveNotEntered);
			var inBondMoveHeader = Header.InBondMovementHeaders.AddNew();
			inBondMoveHeader.BM_TOLDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(inBondMoveHeader.BM_TOLDateInfo, MandatoryValidation.YouHaveNotEntered);
			inBondMoveHeader.BM_TOLDate = ZDateTime.BrettsBirthday;
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_TOLDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckBM_TOLStateCode()
		{
			Header.ValidationModes = ValidationModes.InBondTOL;
			var moveHeader = Header.MovementHeader;
			AssertEquals(true, moveHeader.IsAMSMovement);
			moveHeader.BM_TOLCityName = "BOB'S CITY";
			moveHeader.BM_TOLStateCode = ZString.Empty;
			AssertNoMessageError(moveHeader.BM_TOLStateCodeInfo, ValidationConstants.MoveHeader.TOLStateCodeIsRequired.ToString());
			var inBondMoveHeader = Header.InBondMovementHeaders.AddNew();
			inBondMoveHeader.BM_TOLCityName = "BOB'S CITY";
			inBondMoveHeader.BM_TOLStateCode = ZString.Empty;
			AssertHasMessageError(inBondMoveHeader.BM_TOLStateCodeInfo, ValidationConstants.MoveHeader.TOLStateCodeIsRequired.ToString());
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_TOLStateCodeInfo, ListValidation.InvalidCodeMessageError);
			inBondMoveHeader.BM_TOLCityName = ZString.Empty;
			AssertNoMessageError(inBondMoveHeader.BM_TOLStateCodeInfo, ValidationConstants.MoveHeader.TOLStateCodeIsRequired.ToString());
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_TOLStateCodeInfo, ListValidation.InvalidCodeMessageError);
			inBondMoveHeader.BM_TOLStateCode = "@#";
			AssertNoMessageError(inBondMoveHeader.BM_TOLStateCodeInfo, ValidationConstants.MoveHeader.TOLStateCodeIsRequired.ToString());
			AssertHasMessageErrorContaining(inBondMoveHeader.BM_TOLStateCodeInfo, ListValidation.InvalidCodeMessageError);
			inBondMoveHeader.BM_TOLStateCode = "CA";
			AssertNoMessageError(inBondMoveHeader.BM_TOLStateCodeInfo, ValidationConstants.MoveHeader.TOLStateCodeIsRequired.ToString());
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_TOLStateCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBM_InBondEntryType()
		{
			Header.ValidationModes = ValidationModes.SubsequentInBond;
			var moveHeader = Header.MovementHeader;
			AssertEquals(true, moveHeader.IsAMSMovement);
			moveHeader.BM_InBondEntryType = ZString.Empty;
			AssertNoMessageErrorContaining(moveHeader.BM_InBondEntryTypeInfo, MandatoryValidation.YouHaveNotEntered);
			var inBondMoveHeader = Header.InBondMovementHeaders.AddNew();
			inBondMoveHeader.BM_InBondEntryType = ZString.Empty;
			AssertHasMessageErrorContaining(inBondMoveHeader.BM_InBondEntryTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_InBondEntryTypeInfo, ListValidation.InvalidCodeMessageError);
			inBondMoveHeader.BM_InBondEntryType = "Z#";
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_InBondEntryTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(inBondMoveHeader.BM_InBondEntryTypeInfo, ListValidation.InvalidCodeMessageError);
			inBondMoveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_InBondEntryTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_InBondEntryTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBM_InBondCarrierSCAC()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "X23X";
			Header.ValidationModes = ValidationModes.SubsequentInBond;
			var moveHeader = Header.MovementHeader;
			AssertEquals(true, moveHeader.IsAMSMovement);
			moveHeader.BM_InBondCarrierSCAC = ZString.Empty;
			AssertNoMessageErrorContaining(moveHeader.BM_InBondCarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			var inBondMoveHeader = Header.InBondMovementHeaders.AddNew();
			inBondMoveHeader.BM_InBondCarrierSCAC = ZString.Empty;
			AssertHasMessageErrorContaining(inBondMoveHeader.BM_InBondCarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_InBondCarrierSCACInfo, ListValidation.InvalidCodeMessageError);
			inBondMoveHeader.BM_InBondCarrierSCAC = "#@$#";
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_InBondCarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(inBondMoveHeader.BM_InBondCarrierSCACInfo, ListValidation.InvalidCodeMessageError);
			inBondMoveHeader.BM_InBondCarrierSCAC = "X23X";
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_InBondCarrierSCACInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_InBondCarrierSCACInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckBM_DestinationPortCode()
		{
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "X23X", "Test Name", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "X24X", "Test Name", startDate, endDate);
			Factory.Save();

			Header.ValidationModes = ValidationModes.SubsequentInBond;
			Header.BH_PortUnladingDCode = "X24X";
			var moveHeader = Header.MovementHeader;
			AssertEquals(true, moveHeader.IsAMSMovement);
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			moveHeader.BM_DestinationPortCode = ZString.Empty;
			AssertNoMessageErrorContaining(moveHeader.BM_DestinationPortCodeInfo, MandatoryValidation.YouHaveNotEntered);
			var inBondMoveHeader = Header.InBondMovementHeaders.AddNew();
			inBondMoveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			inBondMoveHeader.BM_DestinationPortCode = ZString.Empty;
			AssertHasMessageErrorContaining(inBondMoveHeader.BM_DestinationPortCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_DestinationPortCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(inBondMoveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationShouldNotMatchPortOfArrivalFor61EntryType.ToString());
			inBondMoveHeader.BM_DestinationPortCode = "ZS#$";
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_DestinationPortCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(inBondMoveHeader.BM_DestinationPortCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(inBondMoveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationShouldNotMatchPortOfArrivalFor61EntryType.ToString());
			inBondMoveHeader.BM_DestinationPortCode = "X24X";
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_DestinationPortCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_DestinationPortCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(inBondMoveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationShouldNotMatchPortOfArrivalFor61EntryType.ToString());
			var subInBondMoveHeader = Header.InBondMovementHeaders.AddNew();
			subInBondMoveHeader.BM_IsSubsequentInBond = ZBool.True;
			subInBondMoveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			subInBondMoveHeader.BM_DestinationPortCode = "X24X";
			AssertNoMessageErrorContaining(subInBondMoveHeader.BM_DestinationPortCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(subInBondMoveHeader.BM_DestinationPortCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(subInBondMoveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationShouldNotMatchPortOfArrivalFor61EntryType.ToString());
			inBondMoveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_DestinationPortCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_DestinationPortCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(inBondMoveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationShouldNotMatchPortOfArrivalFor61EntryType.ToString());
			inBondMoveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_DestinationPortCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_DestinationPortCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(inBondMoveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationShouldNotMatchPortOfArrivalFor61EntryType.ToString());
			inBondMoveHeader.BM_DestinationPortCode = "X23X";
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_DestinationPortCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_DestinationPortCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(inBondMoveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationShouldNotMatchPortOfArrivalFor61EntryType.ToString());
			Header.ValidationModes = ValidationModes.InBondArrival;
			inBondMoveHeader.BM_DestinationPortCode = "X24X";
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_DestinationPortCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_DestinationPortCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(inBondMoveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationShouldNotMatchPortOfArrivalFor61EntryType.ToString());
			inBondMoveHeader.BM_DestinationPortCode = "ZS#$";
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_DestinationPortCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(inBondMoveHeader.BM_DestinationPortCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(inBondMoveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationShouldNotMatchPortOfArrivalFor61EntryType.ToString());
			inBondMoveHeader.BM_DestinationPortCode = ZString.Empty;
			AssertHasMessageErrorContaining(inBondMoveHeader.BM_DestinationPortCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(inBondMoveHeader.BM_DestinationPortCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(inBondMoveHeader.BM_DestinationPortCodeInfo, ValidationConstants.MoveHeader.USDestinationShouldNotMatchPortOfArrivalFor61EntryType.ToString());
		}

		new CusInBondHeader Header => (CusInBondHeader)base.Header;

		protected override US.Business.CusInBondMoveHeader CreateNewCusInBondMoveHeader(Customs.Business.CusInBondHeader header)
		{
			var moveHeader = header.Factory.New<CusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			moveHeader.BM_SubApplicationCode = SubApplicationCodeList.Codes.MasterInBond;
			return moveHeader;
		}

		protected override Customs.Business.CusInBondHeader CreateNewCusInBondHeader() => Factory.New<CusInBondHeader>();
	}
}
