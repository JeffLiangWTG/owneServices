using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class CusInBondMoveDetailValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckB9_BM()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondBill bill = header.Bills.AddNew();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = Factory.New<CusInBondMoveDetail>();
			moveDetail.B9_B0 = bill.PK;
			moveDetail.B9_BM = moveHeader.PK;
			AssertNoError(moveDetail.B9_BMInfo, ValidationConstants.MoveDetail.CannotAddMovementDetailsWhilePendingCustoms);
			AssertNoError(moveDetail.B9_BMInfo, ValidationConstants.MoveDetail.CannotAddMovementDetailsWhenInBondHasBeenSubmitted);
			moveHeader.BM_CustomsStatus = Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
			moveDetail.B9_BM = moveHeader.PK;
			AssertHasError(moveDetail.B9_BMInfo, ValidationConstants.MoveDetail.CannotAddMovementDetailsWhilePendingCustoms);
			AssertNoError(moveDetail.B9_BMInfo, ValidationConstants.MoveDetail.CannotAddMovementDetailsWhenInBondHasBeenSubmitted);
			moveHeader.BM_CustomsStatus = Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.ClearDepartureOriginal;
			moveDetail.B9_BM = moveHeader.PK;
			AssertNoError(moveDetail.B9_BMInfo, ValidationConstants.MoveDetail.CannotAddMovementDetailsWhilePendingCustoms);
			AssertHasError(moveDetail.B9_BMInfo, ValidationConstants.MoveDetail.CannotAddMovementDetailsWhenInBondHasBeenSubmitted);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			moveHeader.BM_CustomsStatus = Enterprise.Customs.Common.US.ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
			moveDetail.B9_BM = moveHeader.PK;
			AssertHasError(moveDetail.B9_BMInfo, ValidationConstants.MoveDetail.CannotAddMovementDetailsWhilePendingCustoms);
			AssertNoError(moveDetail.B9_BMInfo, ValidationConstants.MoveDetail.CannotAddMovementDetailsWhenInBondHasBeenSubmitted);
		}

		public void TestCheckB9_InBoundQty()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondBill bill = header.Bills.AddNew();
			bill.B0_ManifestQty = 10;
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = Factory.New<CusInBondMoveDetail>();
			moveDetail.B9_B0 = bill.PK;
			moveDetail.B9_BM = moveHeader.PK;
			moveDetail.B9_InBoundQty = 11;
			AssertHasMessageError(moveDetail.B9_InBoundQtyInfo, ValidationConstants.MoveDetail.InBondQtyGreaterThanBillManifestQty.ToString());
			moveDetail.B9_B0 = ZGuid.Empty;
			AssertNoMessageError(moveDetail.B9_InBoundQtyInfo, ValidationConstants.MoveDetail.InBondQtyGreaterThanBillManifestQty.ToString());
			moveDetail.B9_B0 = bill.PK;
			AssertNoMessageError(moveDetail.B9_InBoundQtyInfo, ValidationConstants.MoveDetail.InBondQtyGreaterThanBillManifestQty.ToString());
			AssertEquals(10, moveDetail.B9_InBoundQty);
			moveDetail.B9_InBoundQty = 11;
			AssertHasMessageError(moveDetail.B9_InBoundQtyInfo, ValidationConstants.MoveDetail.InBondQtyGreaterThanBillManifestQty.ToString());
			moveDetail.B9_InBoundQty = 10;
			AssertNoMessageError(moveDetail.B9_InBoundQtyInfo, ValidationConstants.MoveDetail.InBondQtyGreaterThanBillManifestQty.ToString());
			AssertNoMessageErrorContaining(moveDetail.B9_InBoundQtyInfo, MandatoryValidation.YouHaveNotEntered);
			moveDetail.B9_InBoundQty = 0;
			AssertHasMessageErrorContaining(moveDetail.B9_InBoundQtyInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			moveDetail.B9_InBoundQty = 0;
			AssertNoMessageErrorContaining(moveDetail.B9_InBoundQtyInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckB9_PreviousITNumber()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_FTZMove = ZBool.True;
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_PreviousITNumber = "IT32342";
			AssertHasMessageError(moveDetail.B9_PreviousITNumberInfo, ITNumberValidator.Constants.PreviousEntryNumber.Invalid);
			moveDetail.B9_PreviousITNumber = "123234223";
			AssertNoMessageError(moveDetail.B9_PreviousITNumberInfo, ITNumberValidator.Constants.PreviousEntryNumber.Invalid);
			AssertHasWarning(moveDetail.B9_PreviousITNumberInfo, Enterprise.Customs.US.Business.ValidationConstants.AllocateInBondNumber.InvalidCheckDigit("6"));
			moveDetail.B9_PreviousITNumber = "123234226";
			AssertNoWarning(moveDetail.B9_PreviousITNumberInfo, Enterprise.Customs.US.Business.ValidationConstants.AllocateInBondNumber.InvalidCheckDigit("6"));
			AssertNoNotifications(moveDetail.B9_PreviousITNumberInfo);
			moveDetail.B9_PreviousITNumber = "VAB12345675";
			AssertNoNotifications(moveDetail.B9_PreviousITNumberInfo);
			moveDetail.B9_PreviousITNumber = "08112345678";
			AssertNoNotifications(moveDetail.B9_PreviousITNumberInfo);
			AssertNoMessageError(moveDetail.B9_PreviousITNumberInfo, CusInBondMoveDetailValidation.LengthExceeded);
			moveDetail.B9_PreviousITNumber = "081123456783";
			AssertHasMessageError(moveDetail.B9_PreviousITNumberInfo, ITNumberValidator.Constants.PreviousEntryNumber.Invalid);
			AssertHasMessageError(moveDetail.B9_PreviousITNumberInfo, CusInBondMoveDetailValidation.LengthExceeded);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			moveDetail.B9_PreviousITNumber = "081123456783";
			AssertHasMessageError(moveDetail.B9_PreviousITNumberInfo, ITNumberValidator.Constants.PreviousEntryNumber.Invalid);
			AssertHasMessageError(moveDetail.B9_PreviousITNumberInfo, CusInBondMoveDetailValidation.LengthExceeded);
		}

		public void TestValidateAtLeastOneContainerIsRequired()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.Validation.ValidateAll();
			AssertNoRowMessageError(moveDetail, ValidationConstants.MoveDetail.AtLeastOneContainerIsRequired);
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			moveDetail.Validation.ValidateAll();
			AssertHasRowMessageError(moveDetail, ValidationConstants.MoveDetail.AtLeastOneContainerIsRequired);
			CusInBondContainer container = moveDetail.Containers.AddNew();
			moveDetail.Validation.ValidateAll();
			AssertNoRowMessageError(moveDetail, ValidationConstants.MoveDetail.AtLeastOneContainerIsRequired);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			moveDetail.Containers.DeleteAll();
			moveDetail.Validation.ValidateAll();
			AssertNoRowMessageError(moveDetail, ValidationConstants.MoveDetail.AtLeastOneContainerIsRequired);
		}

		public void TestValidateMovementDetailOfABillIsNotDuplicated()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondBill bill1 = header.Bills.AddNew();
			CusInBondBill bill2 = header.Bills.AddNew();
			CusInBondMoveHeader moveHeader1 = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail1 = moveHeader1.MovementDetails.AddNew();
			moveDetail1.B9_B0 = bill1.PK;
			CusInBondMoveDetail moveDetail2 = moveHeader1.MovementDetails.AddNew();
			moveDetail2.B9_B0 = bill1.PK;
			AssertHasError(moveDetail2.B9_B0Info, ValidationConstants.MoveDetail.MovementDetailForBillIsDuplicated);
			moveDetail2.B9_B0 = bill2.PK;
			AssertNoError(moveDetail2.B9_B0Info, ValidationConstants.MoveDetail.MovementDetailForBillIsDuplicated);
			moveDetail2.B9_B0 = ZGuid.Empty;
			AssertNoError(moveDetail2.B9_B0Info, ValidationConstants.MoveDetail.MovementDetailForBillIsDuplicated);
			moveDetail2.B9_B0 = ZGuid.Invalid;
			AssertNoError(moveDetail2.B9_B0Info, ValidationConstants.MoveDetail.MovementDetailForBillIsDuplicated);
			CusInBondMoveHeader moveHeader2 = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail3 = moveHeader2.MovementDetails.AddNew();
			moveDetail3.B9_B0 = bill2.PK;
			moveDetail2.B9_B0 = bill2.PK;
			AssertNoError(moveDetail2.B9_B0Info, ValidationConstants.MoveDetail.MovementDetailForBillIsDuplicated);
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			moveDetail1.B9_B0 = bill1.PK;
			moveDetail2.B9_B0 = bill1.PK;
			AssertHasError(moveDetail2.B9_B0Info, ValidationConstants.MoveDetail.MovementDetailForBillIsDuplicated);
		}

		public void TestCheckB9_FirstSecondaryNotifyParty()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "Z1Z1";
			Factory.Save();
			var header = Factory.New<CusInBondHeader>();
			header.BH_FTZMove = ZBool.False;
			header.BH_FIRMS = "1236";
			var moveHeader = header.MovementHeaders.AddNew();
			DeclarationTestHelper.SetEntryFilerCode("SV9");
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, header.Branch.PK.ToGuid(), Guid.Empty, "2501");
			USCustomsDataRegistry.Instance.BRecordOfficeCode.SetValue(header.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "12");
			var moveDetail = moveHeader.MovementDetails.AddNew();
			AssertNoWarning(moveDetail.B9_FirstSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyIsNotSubmitter.ToString());
			moveDetail.B9_FirstSecondaryNotifyParty = ZString.Empty;
			AssertHasWarning(moveDetail.B9_FirstSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyIsNotSubmitter.ToString());
			moveDetail.B9_FirstSecondaryNotifyParty = "Z!Z!";
			AssertHasMessageError(moveDetail.B9_FirstSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValid.ToString());
			AssertNoMessageError(moveDetail.B9_FirstSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValidFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			AssertNoMessageError(moveDetail.B9_FirstSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.FIRMSNotMatchJobFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			moveDetail.B9_FirstSecondaryNotifyParty = "2705XJ501";
			AssertNoMessageError(moveDetail.B9_FirstSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValid.ToString());
			AssertNoMessageError(moveDetail.B9_FirstSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValidFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			AssertNoMessageError(moveDetail.B9_FirstSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.FIRMSNotMatchJobFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			moveDetail.B9_FirstSecondaryNotifyParty = "Z1Z1";
			AssertNoMessageError(moveDetail.B9_FirstSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValid.ToString());
			AssertNoMessageError(moveDetail.B9_FirstSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValidFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			AssertNoMessageError(moveDetail.B9_FirstSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.FIRMSNotMatchJobFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			moveDetail.B9_FirstSecondaryNotifyParty = "1325";
			AssertHasMessageError(moveDetail.B9_FirstSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValid.ToString());
			AssertNoMessageError(moveDetail.B9_FirstSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValidFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			AssertNoMessageError(moveDetail.B9_FirstSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.FIRMSNotMatchJobFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			header.BH_FTZMove = ZBool.True;
			moveDetail.B9_FirstSecondaryNotifyParty = "1325";
			AssertNoMessageError(moveDetail.B9_FirstSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValid.ToString());
			AssertNoMessageError(moveDetail.B9_FirstSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValidFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			AssertHasMessageError(moveDetail.B9_FirstSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.FIRMSNotMatchJobFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			moveDetail.B9_FirstSecondaryNotifyParty = "Z!Z!";
			AssertNoMessageError(moveDetail.B9_FirstSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValid.ToString());
			AssertHasMessageError(moveDetail.B9_FirstSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValidFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			AssertNoMessageError(moveDetail.B9_FirstSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.FIRMSNotMatchJobFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			moveDetail.B9_FirstSecondaryNotifyParty = "Z!Z!";
			AssertNoMessageError(moveDetail.B9_FirstSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValid.ToString());
		}

		public void TestCheckB9_SecondSecondaryNotifyParty()
		{
			USCarrierCombined carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "Z1Z1";
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_FTZMove = ZBool.False;
			header.BH_FIRMS = "1236";
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_SecondSecondaryNotifyParty = ZString.Empty;
			AssertNoNotifications(moveDetail.B9_SecondSecondaryNotifyPartyInfo);
			string messageError = ValidationConstants.MoveDetail.GetSecondaryNotifyPartyIsSpecifiedOutOfOrderMessage("Second", "First");
			moveDetail.B9_SecondSecondaryNotifyParty = "Z!Z!";
			AssertHasMessageError(moveDetail.B9_SecondSecondaryNotifyPartyInfo, messageError);
			AssertNoMessageError(moveDetail.B9_SecondSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValid.ToString());
			AssertNoMessageError(moveDetail.B9_SecondSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValidFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			AssertNoMessageError(moveDetail.B9_SecondSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.FIRMSNotMatchJobFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			moveDetail.B9_FirstSecondaryNotifyParty = "Z1Z1";
			AssertNoMessageError(moveDetail.B9_SecondSecondaryNotifyPartyInfo, messageError);
			AssertHasMessageError(moveDetail.B9_SecondSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValid.ToString());
			AssertNoMessageError(moveDetail.B9_SecondSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValidFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			AssertNoMessageError(moveDetail.B9_SecondSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.FIRMSNotMatchJobFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			moveDetail.B9_SecondSecondaryNotifyParty = "2705XJ501";
			AssertNoMessageError(moveDetail.B9_SecondSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValid.ToString());
			AssertNoMessageError(moveDetail.B9_SecondSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValidFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			AssertNoMessageError(moveDetail.B9_SecondSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.FIRMSNotMatchJobFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			moveDetail.B9_SecondSecondaryNotifyParty = "Z1Z1";
			AssertNoMessageError(moveDetail.B9_SecondSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValid.ToString());
			AssertNoMessageError(moveDetail.B9_SecondSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValidFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			AssertNoMessageError(moveDetail.B9_SecondSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.FIRMSNotMatchJobFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			header.BH_FTZMove = ZBool.True;
			moveDetail.B9_SecondSecondaryNotifyParty = "1325";
			AssertNoMessageError(moveDetail.B9_SecondSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValid.ToString());
			AssertNoMessageError(moveDetail.B9_SecondSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValidFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			AssertHasMessageError(moveDetail.B9_SecondSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.FIRMSNotMatchJobFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			moveDetail.B9_SecondSecondaryNotifyParty = "Z!Z!";
			AssertNoMessageError(moveDetail.B9_SecondSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValid.ToString());
			AssertHasMessageError(moveDetail.B9_SecondSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValidFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			AssertNoMessageError(moveDetail.B9_SecondSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.FIRMSNotMatchJobFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			moveDetail.B9_SecondSecondaryNotifyParty = "Z!Z!";
			AssertNoMessageError(moveDetail.B9_SecondSecondaryNotifyPartyInfo, messageError);
		}

		public void TestCheckB9_ThirdSecondaryNotifyParty()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "Z1Z1";
			var header = Factory.New<CusInBondHeader>();
			header.BH_FTZMove = ZBool.False;
			header.BH_FIRMS = "1236";
			var moveHeader = header.MovementHeaders.AddNew();
			var moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_ThirdSecondaryNotifyParty = ZString.Empty;
			AssertNoNotifications(moveDetail.B9_ThirdSecondaryNotifyPartyInfo);
			AssertNoWarning(moveDetail.B9_FirstSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyIsNotSubmitter.ToString());
			var messageError = ValidationConstants.MoveDetail.GetSecondaryNotifyPartyIsSpecifiedOutOfOrderMessage("Third", "Second");
			moveDetail.B9_ThirdSecondaryNotifyParty = "Z!Z!";
			AssertHasMessageError(moveDetail.B9_ThirdSecondaryNotifyPartyInfo, messageError.ToString());
			AssertNoMessageError(moveDetail.B9_ThirdSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValid.ToString());
			AssertNoMessageError(moveDetail.B9_ThirdSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValidFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			AssertNoMessageError(moveDetail.B9_ThirdSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.FIRMSNotMatchJobFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			moveDetail.B9_SecondSecondaryNotifyParty = "Z1Z1";
			AssertNoMessageError(moveDetail.B9_ThirdSecondaryNotifyPartyInfo, messageError.ToString());
			AssertHasMessageError(moveDetail.B9_ThirdSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValid.ToString());
			AssertNoMessageError(moveDetail.B9_ThirdSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValidFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			AssertNoMessageError(moveDetail.B9_ThirdSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.FIRMSNotMatchJobFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			moveDetail.B9_ThirdSecondaryNotifyParty = "2705XJ501";
			AssertNoMessageError(moveDetail.B9_ThirdSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValid.ToString());
			AssertNoMessageError(moveDetail.B9_ThirdSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValidFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			AssertNoMessageError(moveDetail.B9_ThirdSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.FIRMSNotMatchJobFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			moveDetail.B9_ThirdSecondaryNotifyParty = "Z1Z1";
			AssertNoMessageError(moveDetail.B9_ThirdSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValid.ToString());
			AssertNoMessageError(moveDetail.B9_ThirdSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValidFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			AssertNoMessageError(moveDetail.B9_ThirdSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.FIRMSNotMatchJobFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			header.BH_FTZMove = ZBool.True;
			moveDetail.B9_ThirdSecondaryNotifyParty = "1325";
			AssertNoMessageError(moveDetail.B9_ThirdSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValid.ToString());
			AssertNoMessageError(moveDetail.B9_ThirdSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValidFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			AssertHasMessageError(moveDetail.B9_ThirdSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.FIRMSNotMatchJobFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			moveDetail.B9_ThirdSecondaryNotifyParty = "Z!Z!";
			AssertNoMessageError(moveDetail.B9_ThirdSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValid.ToString());
			AssertHasMessageError(moveDetail.B9_ThirdSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValidFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			AssertNoMessageError(moveDetail.B9_ThirdSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.FIRMSNotMatchJobFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			moveDetail.B9_ThirdSecondaryNotifyParty = "Z!Z!";
			AssertNoMessageError(moveDetail.B9_ThirdSecondaryNotifyPartyInfo, messageError.ToString());
			header.Bills.AddNew();
			moveDetail.B9_B0 = header.Bills[0].PK;
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.VesselNonContainer;
			DeclarationTestHelper.SetEntryFilerCode("SV9");
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(Guid.Empty, header.Branch.PK.ToGuid(), Guid.Empty, "2501");
			moveDetail.B9_ThirdSecondaryNotifyParty = "";
			AssertHasWarning(moveDetail.B9_FirstSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyIsNotSubmitter.ToString());
			moveDetail.B9_ThirdSecondaryNotifyParty = moveDetail.SubmitterABICode;
			AssertNoWarning(moveDetail.B9_FirstSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyIsNotSubmitter.ToString());
		}

		public void TestCheckB9_FourthSecondaryNotifyParty()
		{
			USCarrierCombined carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "Z1Z1";
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			header.BH_FTZMove = ZBool.False;
			header.BH_FIRMS = "1236";
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			moveDetail.B9_FourthSecondaryNotifyParty = ZString.Empty;
			AssertNoNotifications(moveDetail.B9_FourthSecondaryNotifyPartyInfo);
			string messageError = ValidationConstants.MoveDetail.GetSecondaryNotifyPartyIsSpecifiedOutOfOrderMessage("Fourth", "Third");
			moveDetail.B9_FourthSecondaryNotifyParty = "Z!Z!";
			AssertHasMessageError(moveDetail.B9_FourthSecondaryNotifyPartyInfo, messageError);
			AssertNoMessageError(moveDetail.B9_FourthSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValid.ToString());
			AssertNoMessageError(moveDetail.B9_FourthSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValidFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			AssertNoMessageError(moveDetail.B9_FourthSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.FIRMSNotMatchJobFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			moveDetail.B9_ThirdSecondaryNotifyParty = "Z1Z1";
			AssertNoMessageError(moveDetail.B9_FourthSecondaryNotifyPartyInfo, messageError);
			AssertHasMessageError(moveDetail.B9_FourthSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValid.ToString());
			AssertNoMessageError(moveDetail.B9_FourthSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValidFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			AssertNoMessageError(moveDetail.B9_FourthSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.FIRMSNotMatchJobFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			moveDetail.B9_FourthSecondaryNotifyParty = "2705XJ501";
			AssertNoMessageError(moveDetail.B9_FourthSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValid.ToString());
			AssertNoMessageError(moveDetail.B9_FourthSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValidFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			AssertNoMessageError(moveDetail.B9_FourthSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.FIRMSNotMatchJobFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			moveDetail.B9_FourthSecondaryNotifyParty = "Z1Z1";
			AssertNoMessageError(moveDetail.B9_FourthSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValid.ToString());
			AssertNoMessageError(moveDetail.B9_FourthSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValidFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			AssertNoMessageError(moveDetail.B9_FourthSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.FIRMSNotMatchJobFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			header.BH_FTZMove = ZBool.True;
			moveDetail.B9_FourthSecondaryNotifyParty = "1325";
			AssertNoMessageError(moveDetail.B9_FourthSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValid.ToString());
			AssertNoMessageError(moveDetail.B9_FourthSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValidFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			AssertHasMessageError(moveDetail.B9_FourthSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.FIRMSNotMatchJobFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			moveDetail.B9_FourthSecondaryNotifyParty = "Z!Z!";
			AssertNoMessageError(moveDetail.B9_FourthSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValid.ToString());
			AssertHasMessageError(moveDetail.B9_FourthSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.SecondaryNotifyPartyNotValidFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			AssertNoMessageError(moveDetail.B9_FourthSecondaryNotifyPartyInfo, ValidationConstants.MoveDetail.FIRMSNotMatchJobFIRMSForFTZorBondedWarehouseWithdrawals.ToString());
			header.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AirNonContainer;
			moveDetail.B9_FourthSecondaryNotifyParty = "Z!Z!";
			AssertNoMessageError(moveDetail.B9_FourthSecondaryNotifyPartyInfo, messageError);
		}

		public void TestValidateIsAMSForTransportType30_WithPreviousITNumber()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_ImportTransportMode = TransportModeCodes.Codes.TruckNonContainer;
			header.BH_HeaderType = InBondHeaderTypeList.Codes.FullData;
			var moveHeader = header.MovementHeader;
			var moveDetail1 = moveHeader.MovementDetails.AddNew();
			moveDetail1.B9_PreviousITNumber = "08112345678";
			var moveDetail2 = header.MovementHeader.MovementDetails.AddNew();
			moveDetail1.Validation.ValidateAll();
			moveDetail2.Validation.ValidateAll();
			AssertHasMessageError(moveDetail1.B9_PreviousITNumberInfo, ValidationConstants.Header.InvalidHeaderTypeForTransportModeWithPreviousITNumber.ToString());
			AssertNoMessageError(moveDetail2.B9_PreviousITNumberInfo, ValidationConstants.Header.InvalidHeaderTypeForTransportModeWithPreviousITNumber.ToString());
			header.BH_HeaderType = InBondHeaderTypeList.Codes.AMS;
			moveDetail1.Validation.ValidateAll();
			moveDetail2.Validation.ValidateAll();
			AssertNoMessageError(moveDetail1.B9_PreviousITNumberInfo, ValidationConstants.Header.InvalidHeaderTypeForTransportModeWithPreviousITNumber.ToString());
			AssertNoMessageError(moveDetail2.B9_PreviousITNumberInfo, ValidationConstants.Header.InvalidHeaderTypeForTransportModeWithPreviousITNumber.ToString());
		}
	}
}
