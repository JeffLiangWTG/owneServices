using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(OceanBillCusInBondBillValidation))]
	class OceanBillCusInBondBillValidationTest : CommonCusInBondBillValidationTest
	{
		public void TestCheckB0_IssuerCode()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_MasterBillNumber = ZString.Empty;
			bill.B0_IssuerCode = ZString.Empty;
			AssertNoMessageErrorContaining(bill.B0_IssuerCodeInfo, MandatoryValidation.YouHaveNotEntered);
			bill.B0_MasterBillNumber = "SDF";
			AssertHasMessageErrorContaining(bill.B0_IssuerCodeInfo, MandatoryValidation.YouHaveNotEntered);
			bill.B0_IssuerCode = "Z!";
			AssertNoMessageErrorContaining(bill.B0_IssuerCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(bill.B0_IssuerCodeInfo, ListValidation.InvalidCodeMessageError);
			var carrier = Factory.New<US.Business.USCarrierCombined>();
			carrier.UI_Code = "Z1Z3";
			bill.B0_IssuerCode = "Z1Z3";
			AssertNoMessageError(bill.B0_IssuerCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(bill.B0_IssuerCodeInfo, ListValidation.InvalidCodeMessageError);
			bill.ValidationModes = ValidationModes.None;
			bill.B0_IssuerCode = "Z!";
			AssertNoMessageError(bill.B0_IssuerCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(bill.B0_IssuerCodeInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_IssuerCode = ZString.Empty;
			AssertNoMessageError(bill.B0_IssuerCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(bill.B0_IssuerCodeInfo, ListValidation.InvalidCodeMessageError);
			foreach (var mode in new[] { ValidationModes.ChangeEstDateOfArrival, ValidationModes.VesselArrival, ValidationModes.VesselDeparture })
			{
				bill.ValidationModes = mode;
				bill.B0_IssuerCode = ZString.Empty;
				AssertNoMessageErrorContaining(bill.B0_IssuerCodeInfo, MandatoryValidation.YouHaveNotEntered);
			}

			bill.ValidationModes = ValidationModes.SubsequentInBond;
			bill.B0_IssuerCode = ZString.Empty;
			AssertHasMessageErrorContaining(bill.B0_IssuerCodeInfo, MandatoryValidation.YouHaveNotEntered);
			bill.B0_MasterBillNumber = ZString.Empty;
			AssertNoMessageErrorContaining(bill.B0_IssuerCodeInfo, MandatoryValidation.YouHaveNotEntered);
			var nvoccBill = header.Bills.AddNew();
			nvoccBill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.HouseBill;
			bill.B0_IssuerCode = ZString.Empty;
			AssertHasMessageErrorContaining(bill.B0_IssuerCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckB0_MasterBillNumber()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_IssuerCode = ZString.Empty;
			bill.B0_MasterBillNumber = ZString.Empty;
			AssertNoMessageErrorContaining(bill.B0_MasterBillNumberInfo, MandatoryValidation.YouHaveNotEntered);
			bill.B0_IssuerCode = "OTT1";
			AssertHasMessageErrorContaining(bill.B0_MasterBillNumberInfo, MandatoryValidation.YouHaveNotEntered);
			bill.B0_MasterBillNumber = "HB201";
			AssertNoMessageErrorContaining(bill.B0_MasterBillNumberInfo, MandatoryValidation.YouHaveNotEntered);
			bill.ValidationModes = ValidationModes.None;
			bill.B0_MasterBillNumber = ZString.Empty;
			AssertNoMessageErrorContaining(bill.B0_MasterBillNumberInfo, MandatoryValidation.YouHaveNotEntered);
			foreach (var mode in new[] { ValidationModes.ChangeEstDateOfArrival, ValidationModes.VesselArrival, ValidationModes.VesselDeparture })
			{
				bill.ValidationModes = mode;
				bill.B0_MasterBillNumber = ZString.Empty;
				AssertNoMessageErrorContaining(bill.B0_MasterBillNumberInfo, MandatoryValidation.YouHaveNotEntered);
			}

			bill.ValidationModes = ValidationModes.SubsequentInBond;
			bill.B0_MasterBillNumber = ZString.Empty;
			AssertHasMessageErrorContaining(bill.B0_MasterBillNumberInfo, MandatoryValidation.YouHaveNotEntered);
			var messageError = ValidationConstants.Bill.BillOfLadingNumberIsTooLong("1234567890123");
			bill.B0_MasterBillNumber = "1234567890123";
			AssertHasMessageError(bill.B0_MasterBillNumberInfo, messageError.ToString());
			bill.B0_MasterBillNumber = "123#$456789012";
			AssertNoMessageError(bill.B0_MasterBillNumberInfo, messageError.ToString());
			AssertHasMessageError(bill.B0_MasterBillNumberInfo, ValidationConstants.Bill.BillOfLadingNumberAlphanumeric.ToString());
			bill.B0_MasterBillNumber = "123456789012";
			AssertNoMessageErrors(bill.B0_MasterBillNumberInfo);
			var nvoccBill = header.Bills.AddNew();
			nvoccBill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.HouseBill;
			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_IssuerCode = ZString.Empty;
			bill.B0_MasterBillNumber = ZString.Empty;
			AssertHasMessageErrorContaining(bill.B0_MasterBillNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		protected override CusInBondBill CreateBill(CusInBondHeader header)
		{
			header.BH_TransitDirection = DirectionTypeList.Codes.NVOCC;
			return header.OceanBill;
		}
	}
}
