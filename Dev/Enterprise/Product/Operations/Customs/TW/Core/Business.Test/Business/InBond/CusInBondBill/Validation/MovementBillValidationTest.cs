using CargoWise.Types;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class MovementBillValidationTest : CusInBondBillValidationAbstractTest<MovementBillValidation>
	{
		public void TestCheckB0_ReferenceID()
		{
			CusInBondHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.SEA;
			var arrivalBill = CusInBondHeader.ArrivalBill;
			var movementBill = CusInBondHeader.MovementBill;
			var targetInfo = movementBill.B0_ReferenceIDInfo;
			CombineAssertions("When it display on UI, then validation", () =>
			{
				movementBill.B0_ReferenceID = "XXXX";
				AssertNoMessageError(targetInfo, ValidationConstants.CusInBondBill.LengthForSoNo);
				movementBill.B0_ReferenceID = "XX";
				AssertHasMessageError(targetInfo, ValidationConstants.CusInBondBill.LengthForSoNo);
				movementBill.B0_ReferenceID = ZString.Empty;
				AssertNoMessageError(targetInfo, ValidationConstants.CusInBondBill.LengthForSoNo);
				movementBill.B0_ReferenceID = "!@12";
				AssertHasMessageError(targetInfo, ValidationConstants.CusInBondBill.NumbersAndLettersOnlyForSoNo);
				movementBill.B0_ReferenceID = "XX12";
				AssertNoMessageError(targetInfo, ValidationConstants.CusInBondBill.NumbersAndLettersOnlyForSoNo);
			});

			CusInBondHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AIR;
			CombineAssertions("When it hidden on UI, then skip validation", () =>
			{
				movementBill.B0_ReferenceID = "XX";
				AssertNoMessageError(targetInfo, ValidationConstants.CusInBondBill.LengthForSoNo);
				movementBill.B0_ReferenceID = "!@12";
				AssertNoMessageError(targetInfo, ValidationConstants.CusInBondBill.NumbersAndLettersOnlyForSoNo);
			});
		}

		public void TestCheckB0_MasterBillNumber()
		{
			var mAWBLengthWarningMessage = "The MAWB should contain 11 digits.";
			var expectedCheckDigit = "Invalid check digit. The last digit should be '4'";
			var invalidMAWBFormatWarningMessage = "The MAWB can only contain numbers.";
			var movementHeader = CusInBondHeader.MovementHeader;
			var movementBill = CusInBondHeader.MovementBill;
			var targetInfo = movementBill.B0_MasterBillNumberInfo;
			movementHeader.BM_ExportLadenOn = "";
			movementBill.B0_MasterBillNumber = "111";
			AssertHasMessageError(targetInfo, mAWBLengthWarningMessage);

			movementBill.B0_MasterBillNumber = "08298739457";
			CombineAssertions(() =>
			{
				AssertNoMessageError(targetInfo, mAWBLengthWarningMessage);
				AssertHasMessageError(targetInfo, expectedCheckDigit);
			});

			movementBill.B0_MasterBillNumber = "08298739454";
			CombineAssertions(() =>
			{
				AssertNoMessageError(targetInfo, expectedCheckDigit);
				AssertNoWarning(targetInfo, BillValidator.AirlinePrefixValidationMessage);
			});

			movementHeader.BM_ConveyanceNumber = "QF112";
			movementBill.Validation.ValidateB0_MasterBillNumber();
			AssertHasWarning(targetInfo, BillValidator.AirlinePrefixValidationMessage);

			movementHeader.BM_ConveyanceNumber = "HW652";
			movementBill.Validation.ValidateB0_MasterBillNumber();
			AssertNoWarning(targetInfo, BillValidator.AirlinePrefixValidationMessage);

			movementBill.B0_MasterBillNumber = "082987394X4";
			AssertHasMessageError(targetInfo, invalidMAWBFormatWarningMessage);

			movementBill.B0_MasterBillNumber = "08298739454";
			AssertNoMessageError(targetInfo, invalidMAWBFormatWarningMessage);

			movementHeader.BM_ExportLadenOn = "11";
			movementBill.B0_MasterBillNumber = "111";
			AssertNoMessageError(targetInfo, mAWBLengthWarningMessage);

			movementBill.B0_MasterBillNumber = "08298739457";
			AssertNoMessageError(targetInfo, expectedCheckDigit);

			movementHeader.BM_ConveyanceNumber = "QF112";
			movementBill.Validation.ValidateB0_MasterBillNumber();
			AssertNoWarning(targetInfo, BillValidator.AirlinePrefixValidationMessage);

			movementBill.B0_MasterBillNumber = "082987394X4";
			AssertNoMessageError(targetInfo, invalidMAWBFormatWarningMessage);
		}
	}
}
