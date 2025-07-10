using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ArrivalBillValidationTest : CusInBondBillValidationAbstractTest<ArrivalBillValidation>
	{
		public void TestCheckB0_ReferenceID()
		{
			CusInBondHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.SEA;
			var arrivalBills = CusInBondHeader.ArrivalBill;
			var movementBill = CusInBondHeader.MovementBill;
			var targetInfo = arrivalBills.B0_ReferenceIDInfo;
			CombineAssertions("When it display on UI, then validation", () =>
			{
				arrivalBills.B0_ReferenceID = "XXXX";
				AssertNoMessageError(targetInfo, ValidationConstants.CusInBondBill.LengthForMenifestNo);
				arrivalBills.B0_ReferenceID = "XX";
				AssertHasMessageError(targetInfo, ValidationConstants.CusInBondBill.LengthForMenifestNo);
				arrivalBills.B0_ReferenceID = ZString.Empty;
				AssertNoMessageError(targetInfo, ValidationConstants.CusInBondBill.LengthForMenifestNo);
				arrivalBills.B0_ReferenceID = "!@12";
				AssertHasMessageError(targetInfo, ValidationConstants.CusInBondBill.NumbersAndLettersOnlyForMenifestNo);
				arrivalBills.B0_ReferenceID = "XX12";
				AssertNoMessageError(targetInfo, ValidationConstants.CusInBondBill.NumbersAndLettersOnlyForMenifestNo);
			});

			CusInBondHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AIR;
			CombineAssertions("When it hidden on UI, then skip validation", () =>
			{
				arrivalBills.B0_ReferenceID = "XX";
				AssertNoMessageError(targetInfo, ValidationConstants.CusInBondBill.LengthForMenifestNo);
				arrivalBills.B0_ReferenceID = "!@12";
				AssertNoMessageError(targetInfo, ValidationConstants.CusInBondBill.NumbersAndLettersOnlyForMenifestNo);
			});
		}

		public void TestCheckB0_Weight()
		{
			var arrivalBill = CusInBondHeader.ArrivalBill;
			var targetInfo = arrivalBill.B0_WeightInfo;
			CusInBondHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AIR;
			arrivalBill.B0_WeightUQ = ZString.Empty;
			arrivalBill.B0_Weight = 0;
			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeZero);
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
			});
			arrivalBill.B0_WeightUQ = Weight.Kilograms;
			arrivalBill.B0_Weight = 0;
			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeZero);
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
			});
			arrivalBill.B0_Weight = -1;
			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeZero);
				AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
			});
			arrivalBill.B0_Weight = 1;
			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeZero);
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
			});
			CusInBondHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.SEA;
			arrivalBill.B0_Weight = ZDecimal.Zero;
			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeZero);
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
			});
			arrivalBill.B0_Weight = -1;
			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeZero);
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
			});
		}

		public void TestCheckB0_WeightUQ()
		{
			var arrivalBill = CusInBondHeader.ArrivalBill;
			var targetInfo = arrivalBill.B0_WeightUQInfo;
			CusInBondHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.AIR;
			arrivalBill.B0_Weight = 0;
			arrivalBill.B0_ManifestUQ = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, ValidationConstants.CusInBondBill.TotalGrossWeightUnitIsNotEmpty);
			arrivalBill.B0_Weight = 10;
			arrivalBill.B0_WeightUQ = ZString.Empty;
			AssertHasMessageErrorContaining(targetInfo, ValidationConstants.CusInBondBill.TotalGrossWeightUnitIsNotEmpty);
			arrivalBill.B0_Weight = -1;
			arrivalBill.B0_WeightUQ = Weight.Kilograms;
			AssertNoMessageErrorContaining(targetInfo, ValidationConstants.CusInBondBill.TotalGrossWeightUnitIsNotEmpty);
			arrivalBill.B0_Weight = 10;
			arrivalBill.B0_WeightUQ = Weight.Kilograms;
			AssertNoMessageErrorContaining(targetInfo, ValidationConstants.CusInBondBill.TotalGrossWeightUnitIsNotEmpty);

			CusInBondHeader.BH_ImportTransportMode = InBondTransportModeCodes.Codes.SEA;
			arrivalBill.B0_WeightUQ = Weight.Kilograms;
			AssertNoMessageErrorContaining(targetInfo, ValidationConstants.CusInBondBill.TotalGrossWeightUnitIsNotEmpty);
		}

		public void TestCheckB0_ManifestQty()
		{
			var arrivalBill = CusInBondHeader.ArrivalBill;
			var targetInfo = arrivalBill.B0_ManifestQtyInfo;
			arrivalBill.B0_ManifestQty = 0;
			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeZero);
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
			});
			arrivalBill.B0_ManifestQty = -1;
			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeZero);
				AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
			});
			arrivalBill.B0_ManifestQty = 1;
			CombineAssertions(() =>
			{
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeZero);
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.ValueCannotBeNegative);
			});
		}

		public void TestCheckB0_ManifestUQ()
		{
			var arrivalBill = CusInBondHeader.ArrivalBill;
			var targetInfo = arrivalBill.B0_ManifestUQInfo;
			arrivalBill.B0_ManifestQty = 0;
			arrivalBill.B0_ManifestUQ = ZString.Empty;
			AssertNoMessageErrorContaining(targetInfo, ValidationConstants.CusInBondBill.ManifestQuantityUnitIsNotEmpty);
			arrivalBill.B0_ManifestQty = 10;
			arrivalBill.B0_ManifestUQ = ZString.Empty;
			AssertHasMessageErrorContaining(targetInfo, ValidationConstants.CusInBondBill.ManifestQuantityUnitIsNotEmpty);
			arrivalBill.B0_ManifestUQ = Weight.Kilograms;
			AssertNoMessageErrorContaining(targetInfo, ValidationConstants.CusInBondBill.ManifestQuantityUnitIsNotEmpty);
			arrivalBill.B0_ManifestQty = 10;
			arrivalBill.B0_ManifestUQ = Weight.Kilograms;
			AssertNoMessageErrorContaining(targetInfo, ValidationConstants.CusInBondBill.ManifestQuantityUnitIsNotEmpty);
		}

		public void TestCheckB0_MasterBillNumberMandatory()
		{
			var arrivalBill = CusInBondHeader.ArrivalBill;
			var targetInfo = arrivalBill.B0_MasterBillNumberInfo;
			arrivalBill.B0_MasterBillNumber = ZString.Empty;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			arrivalBill.B0_MasterBillNumber = "1111";
			AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckB0_MasterBillNumber()
		{
			var mAWBLengthWarningMessage = "The MAWB should contain 11 digits.";
			var expectedCheckDigit = "Invalid check digit. The last digit should be '4'";
			var invalidMAWBFormatWarningMessage = "The MAWB can only contain numbers.";
			CusInBondHeader.BH_ImportTransportMode = "04";
			var arrivalBill = CusInBondHeader.ArrivalBill;
			var targetInfo = arrivalBill.B0_MasterBillNumberInfo;
			arrivalBill.B0_MasterBillNumber = "111";
			AssertHasMessageError(targetInfo, mAWBLengthWarningMessage);

			arrivalBill.B0_MasterBillNumber = "08298739457";
			CombineAssertions(() =>
			{
				AssertNoMessageError(targetInfo, mAWBLengthWarningMessage);
				AssertHasMessageError(targetInfo, expectedCheckDigit);
			});

			arrivalBill.B0_MasterBillNumber = "08298739454";
			CombineAssertions(() =>
			{
				AssertNoMessageError(targetInfo, expectedCheckDigit);
				AssertNoWarning(targetInfo, BillValidator.AirlinePrefixValidationMessage);
			});

			CusInBondHeader.BH_UniqueVoyageIdentifier = "QF112";
			arrivalBill.Validation.ValidateB0_MasterBillNumber();
			AssertHasWarning(targetInfo, BillValidator.AirlinePrefixValidationMessage);

			CusInBondHeader.BH_UniqueVoyageIdentifier = "HW652";
			arrivalBill.Validation.ValidateB0_MasterBillNumber();
			AssertNoWarning(targetInfo, BillValidator.AirlinePrefixValidationMessage);

			arrivalBill.B0_MasterBillNumber = "082987394X4";
			AssertHasMessageError(targetInfo, invalidMAWBFormatWarningMessage);

			arrivalBill.B0_MasterBillNumber = "08298739454";
			AssertNoMessageError(targetInfo, invalidMAWBFormatWarningMessage);

			CusInBondHeader.BH_ImportTransportMode = "01";
			arrivalBill.B0_MasterBillNumber = "111";
			AssertNoMessageError(targetInfo, mAWBLengthWarningMessage);

			arrivalBill.B0_MasterBillNumber = "08298739457";
			AssertNoMessageError(targetInfo, expectedCheckDigit);

			CusInBondHeader.BH_UniqueVoyageIdentifier = "QF112";
			arrivalBill.Validation.ValidateB0_MasterBillNumber();
			AssertNoWarning(targetInfo, BillValidator.AirlinePrefixValidationMessage);

			arrivalBill.B0_MasterBillNumber = "082987394X4";
			AssertNoMessageError(targetInfo, invalidMAWBFormatWarningMessage);
		}
	}
}
