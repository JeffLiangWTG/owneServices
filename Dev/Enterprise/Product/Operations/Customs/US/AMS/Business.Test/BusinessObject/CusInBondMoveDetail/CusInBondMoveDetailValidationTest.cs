using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.AMS.Messaging.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class CusInBondMoveDetailValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckB9_ExportDate()
		{
			var moveHeader = header.MovementHeader;
			moveHeader.ValidationModes = ValidationModes.InBondExportation;
			AssertEquals(true, moveHeader.IsAMSMovement);
			moveDetail.B9_ExportDate = ZDateTime.Empty;
			AssertNoMessageErrorContaining(moveDetail.B9_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);

			var inBondMoveHeader = header.InBondMovementHeaders.AddNew();
			inBondMoveHeader.ValidationModes = ValidationModes.InBondExportation;
			moveDetail = inBondMoveHeader.MovementDetails.AddNew();
			moveDetail.B9_ExportDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(moveDetail.B9_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);

			moveDetail.B9_ExportDate = ZDateTime.BrettsBirthday;
			AssertNoMessageErrorContaining(moveDetail.B9_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);

			inBondMoveHeader.ValidationModes = ValidationModes.InBondArrival;
			moveDetail.B9_ExportDate = ZDateTime.Empty;
			AssertNoMessageErrorContaining(moveDetail.B9_ExportDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckB9_ExportLadenOn()
		{
			var moveHeader = header.MovementHeader;
			moveHeader.ValidationModes = ValidationModes.InBondExportation;
			AssertEquals(true, moveHeader.IsAMSMovement);
			moveDetail.B9_ExportLadenOn = ZString.Empty;
			AssertNoMessageErrorContaining(moveDetail.B9_ExportLadenOnInfo, MandatoryValidation.YouHaveNotEntered);

			var inBondMoveHeader = header.InBondMovementHeaders.AddNew();
			inBondMoveHeader.ValidationModes = ValidationModes.InBondExportation;
			moveDetail = inBondMoveHeader.MovementDetails.AddNew();
			moveDetail.B9_ExportLadenOn = ZString.Empty;
			AssertHasMessageErrorContaining(moveDetail.B9_ExportLadenOnInfo, MandatoryValidation.YouHaveNotEntered);

			moveDetail.B9_ExportLadenOn = "BOB'S VESSEL";
			AssertNoMessageErrorContaining(moveDetail.B9_ExportLadenOnInfo, MandatoryValidation.YouHaveNotEntered);

			inBondMoveHeader.ValidationModes = ValidationModes.InBondArrival;
			moveDetail.B9_ExportLadenOn = ZString.Empty;
			AssertNoMessageErrorContaining(moveDetail.B9_ExportLadenOnInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestValidateAtLeastOneContainerIsRequired()
		{
			var inBondMoveHeader = header.InBondMovementHeaders.AddNew();
			var inBondMoveDetail = inBondMoveHeader.MovementDetails.AddNew(bill.PK);
			bill.ValidationModes = ValidationModes.InventoryRecord;
			moveDetail.Validation.ValidateAll();
			AssertHasRowMessageError(moveDetail, ValidationConstants.MoveDetail.AtLeastOneContainerIsRequired);
			AssertNoRowMessageError(inBondMoveDetail, ValidationConstants.MoveDetail.AtLeastOneContainerIsRequired);

			var container = moveDetail.Containers.AddNew();
			moveDetail.Validation.ValidateAll();
			AssertNoRowMessageError(moveDetail, ValidationConstants.MoveDetail.AtLeastOneContainerIsRequired);
			container.Delete();
			moveDetail.Validation.ValidateAll();
			AssertHasRowMessageError(moveDetail, ValidationConstants.MoveDetail.AtLeastOneContainerIsRequired);
			bill.ValidationModes = ValidationModes.None;
			moveDetail.Validation.ValidateAll();
			AssertNoRowMessageError(moveDetail, ValidationConstants.MoveDetail.AtLeastOneContainerIsRequired);

			bill.ValidationModes = ValidationModes.PermitToTransfer;
			moveDetail.Validation.ValidateAll();
			AssertNoRowMessageError(moveDetail, ValidationConstants.MoveDetail.AtLeastOneContainerIsRequired);
		}

		public void TestCheckB9_InBoundQty()
		{
			var header = Factory.New<CusInBondHeader>();
			header.ValidationModes = ValidationModes.PermitToTransfer;
			var bill = header.Bills.AddNew();
			bill.B0_ManifestQty = 9;
			var inBondMoveHeader = header.InBondMovementHeaders.AddNew();
			var inBondMoveDetail = inBondMoveHeader.MovementDetails.AddNew(bill.PK);
			inBondMoveDetail.B9_InBoundQty = 11;
			AssertNoMessageError(inBondMoveDetail.B9_InBoundQtyInfo, ValidationConstants.MoveDetail.PTTQtyCannotExceedBOLQty.ToString());

			var pttMoveHeader = header.PTTMovements.AddNew();
			var pttMoveDetail = pttMoveHeader.MovementDetails.AddNew(bill.PK);
			pttMoveDetail.B9_InBoundQty = 11;
			AssertHasMessageError(pttMoveDetail.B9_InBoundQtyInfo, ValidationConstants.MoveDetail.PTTQtyCannotExceedBOLQty.ToString());
			pttMoveDetail.B9_InBoundQty = 5;
			AssertNoMessageError(pttMoveDetail.B9_InBoundQtyInfo, ValidationConstants.MoveDetail.PTTQtyCannotExceedBOLQty.ToString());

			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			pttMoveDetail.B9_InBoundQty = 11;
			AssertNoMessageError(pttMoveDetail.B9_InBoundQtyInfo, ValidationConstants.MoveDetail.PTTQtyCannotExceedBOLQty.ToString());
			pttMoveHeader.BM_InBondCarrierID = "234";
			pttMoveDetail.B9_InBoundQty = 11;
			AssertHasMessageError(pttMoveDetail.B9_InBoundQtyInfo, ValidationConstants.MoveDetail.PTTQtyCannotExceedBOLQty.ToString());
			pttMoveDetail.B9_InBoundQty = 5;
			AssertNoMessageError(pttMoveDetail.B9_InBoundQtyInfo, ValidationConstants.MoveDetail.PTTQtyCannotExceedBOLQty.ToString());
		}

		public void TestCheckB9_B0()
		{
			var header = Factory.New<CusInBondHeader>();
			header.ValidationModes = ValidationModes.InventoryRecord;
			var bill = header.Bills.AddNew();
			var ref1 = bill.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes._2K, "ABC2K");
			var ref2 = bill.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.CSK, "ABCCSK");
			var ref3 = bill.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OB, "ABCOB23");
			var ref4 = bill.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.OL, "ABCOL34");
			var ref5 = bill.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.RC, "ABCRC87");
			var ref6 = bill.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.S7, "ABCS763");
			var ref7 = bill.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.SI, "ABCSI34");
			var ref8 = bill.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.ULC, "ABCULC39");
			var ref9 = bill.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.UT, "ABCUT46");
			var ref10 = bill.ShipmentReferenceDetails.AddNew(BillReferenceList.Codes.WY, "ABCWY936");
			var messageWarning = ValidationConstants.MoveDetail.InvalidReferencesForInBond("CSK, OB, OL, RC, S7, ULC, UT");
			var moveDetail = bill.MovementDetail;
			moveDetail.Validation.ValidateB9_B0();
			AssertNoWarning(moveDetail.B9_B0Info, messageWarning.ToString());
			var inBondMoveHeader = header.InBondMovementHeaders.AddNew();
			var inBondMoveDetail = inBondMoveHeader.MovementDetails.AddNew(bill.PK);
			AssertHasWarning(inBondMoveDetail.B9_B0Info, messageWarning.ToString());

			header.ValidationModes = ValidationModes.VesselArrival;
			inBondMoveDetail.Validation.ValidateB9_B0();
			AssertNoWarning(inBondMoveDetail.B9_B0Info, messageWarning.ToString());

			header.ValidationModes = ValidationModes.SubsequentInBond;
			inBondMoveDetail.Validation.ValidateB9_B0();
			AssertHasWarning(inBondMoveDetail.B9_B0Info, messageWarning.ToString());
		}

		public void TestCheckOneMoveDetailPerBillInBondMovement()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var amsMovement = header.MovementHeader;
			var amsMovementDetail = bill.MovementDetail;
			var pptMovement = header.PTTMovements.AddNew();
			var pptMovementDetail = pptMovement.MovementDetails.AddNew(bill.PK);
			var inbondMovement1 = header.InBondMovementHeaders.AddNew();
			var inbondMovement1Detail = inbondMovement1.MovementDetails.AddNew(bill.PK);
			AssertNoError(inbondMovement1Detail.B9_B0Info, ValidationConstants.MoveDetail.BillOfLadingShouldHaveOneMasterInBondOnly);
			var inbondMovement2 = header.InBondMovementHeaders.AddNew();
			var inbondMovement2Detail = inbondMovement2.MovementDetails.AddNew(bill.PK);
			inbondMovement1Detail.B9_B0 = bill.PK;
			AssertHasError(inbondMovement1Detail.B9_B0Info, ValidationConstants.MoveDetail.BillOfLadingShouldHaveOneMasterInBondOnly);
		}

		public void TestCheckB9_ForeignDestPortKCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var foreignPort = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "X23X1", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.Common);
			Factory.Save();

			header.ValidationModes = ValidationModes.SubsequentInBond;
			var bill = header.Bills.AddNew();
			var moveHeader = header.MovementHeader;
			AssertEquals(true, moveHeader.IsAMSMovement);
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			moveDetail.B9_ForeignDestPortKCode = ZString.Empty;
			AssertNoMessageErrorContaining(moveDetail.B9_ForeignDestPortKCodeInfo, MandatoryValidation.YouHaveNotEntered);

			var inBondMoveHeader = header.InBondMovementHeaders.AddNew();
			moveDetail = inBondMoveHeader.MovementDetails.AddNew(bill.PK);

			foreach (var code in new[] { InbondCommonTypeList.Codes._2TransportandExport, InbondCommonTypeList.Codes._3ImmediateExport })
			{
				inBondMoveHeader.BM_InBondEntryType = code;
				moveDetail.B9_ForeignDestPortKCode = ZString.Empty;
				AssertHasMessageErrorContaining(moveDetail.B9_ForeignDestPortKCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(moveDetail.B9_ForeignDestPortKCodeInfo, ListValidation.InvalidCodeMessageError);
				AssertNoMessageError(moveDetail.B9_ForeignDestPortKCodeInfo, ValidationConstants.MoveDetail.ForeignDestinationIsOnlyRequiredFor62Or63EntryType.ToString());

				moveDetail.B9_ForeignDestPortKCode = "ZS#$";
				AssertNoMessageErrorContaining(moveDetail.B9_ForeignDestPortKCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining(moveDetail.B9_ForeignDestPortKCodeInfo, ListValidation.InvalidCodeMessageError);
				AssertNoMessageError(moveDetail.B9_ForeignDestPortKCodeInfo, ValidationConstants.MoveDetail.ForeignDestinationIsOnlyRequiredFor62Or63EntryType.ToString());

				moveDetail.B9_ForeignDestPortKCode = "X23X1";
				AssertNoMessageErrorContaining(moveDetail.B9_ForeignDestPortKCodeInfo, MandatoryValidation.YouHaveNotEntered);
				AssertNoMessageErrorContaining(moveDetail.B9_ForeignDestPortKCodeInfo, ListValidation.InvalidCodeMessageError);
				AssertNoMessageError(moveDetail.B9_ForeignDestPortKCodeInfo, ValidationConstants.MoveDetail.ForeignDestinationIsOnlyRequiredFor62Or63EntryType.ToString());
			}

			inBondMoveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._1ImmediateTransport;
			moveDetail.B9_ForeignDestPortKCode = "X23X1";
			AssertNoMessageErrorContaining(moveDetail.B9_ForeignDestPortKCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(moveDetail.B9_ForeignDestPortKCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(moveDetail.B9_ForeignDestPortKCodeInfo, ValidationConstants.MoveDetail.ForeignDestinationIsOnlyRequiredFor62Or63EntryType.ToString());

			moveDetail.B9_ForeignDestPortKCode = ZString.Empty;
			AssertNoMessageErrorContaining(moveDetail.B9_ForeignDestPortKCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(moveDetail.B9_ForeignDestPortKCodeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageError(moveDetail.B9_ForeignDestPortKCodeInfo, ValidationConstants.MoveDetail.ForeignDestinationIsOnlyRequiredFor62Or63EntryType.ToString());
		}

		public void TestCheckB9_MonetaryValue()
		{
			header.ValidationModes = ValidationModes.SubsequentInBond;
			var moveHeader = header.MovementHeader;
			AssertEquals(true, moveHeader.IsAMSMovement);
			moveHeader.BM_InBondEntryType = InbondCommonTypeList.Codes._2TransportandExport;
			moveDetail.B9_MonetaryValue = ZDecimal.Zero;
			AssertNoMessageError(moveDetail.B9_MonetaryValueInfo, ValidationConstants.MoveDetail.MonetaryValueIsRequired.ToString());

			var inBondMoveHeader = header.InBondMovementHeaders.AddNew();
			moveDetail = inBondMoveHeader.MovementDetails.AddNew();
			foreach (ICodeDescription pair in new InbondCommonTypeList())
			{
				inBondMoveHeader.BM_InBondEntryType = pair.Code;
				moveDetail.B9_MonetaryValue = ZDecimal.Zero;
				AssertHasMessageError(moveDetail.B9_MonetaryValueInfo, ValidationConstants.MoveDetail.MonetaryValueIsRequired.ToString());

				moveDetail.B9_MonetaryValue = -12m;
				AssertHasMessageError(moveDetail.B9_MonetaryValueInfo, ValidationConstants.MoveDetail.MonetaryValueIsRequired.ToString());

				moveDetail.B9_MonetaryValue = 12m;
				AssertNoMessageError(moveDetail.B9_MonetaryValueInfo, ValidationConstants.MoveDetail.MonetaryValueIsRequired.ToString());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			bill = header.Bills.AddNew();
			moveDetail = bill.MovementDetail;
		}
		CusInBondHeader header;
		CusInBondBill bill;
		CusInBondMoveDetail moveDetail;
	}
}
