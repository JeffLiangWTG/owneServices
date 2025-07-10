using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.AMS.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestsSubclassesOf(typeof(CommonCusInBondBillValidation))]
	abstract class CommonCusInBondBillValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckB0_Firms()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "KD23", "Misaka", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.DistrictPortCode, "3902");
			Factory.Save();

			bill.B0_Firms = ZString.Empty;
			bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.IntangiblesRail;
			AssertNoMessageErrorContaining(bill.B0_FirmsInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.B0_FirmsInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_Firms = "Z!";
			AssertNoMessageErrorContaining(bill.B0_FirmsInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(bill.B0_FirmsInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_Firms = "KD23";
			AssertNoMessageErrorContaining(bill.B0_FirmsInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.B0_FirmsInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.InternationalMailDirectDischargeAtMailFacility;
			AssertNoMessageErrorContaining(bill.B0_FirmsInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.B0_FirmsInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_Firms = "Z!";
			AssertNoMessageErrorContaining(bill.B0_FirmsInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(bill.B0_FirmsInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_Firms = ZString.Empty;
			AssertHasMessageErrorContaining(bill.B0_FirmsInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.B0_FirmsInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_BillStatus = BillOfLadingStatusIndicatorList.Codes.InternationalMailInBondToInternationalMailFacility;
			AssertHasMessageErrorContaining(bill.B0_FirmsInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.B0_FirmsInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_Firms = "Z!";
			AssertNoMessageErrorContaining(bill.B0_FirmsInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(bill.B0_FirmsInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_Firms = "KD23";
			AssertNoMessageErrorContaining(bill.B0_FirmsInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.B0_FirmsInfo, ListValidation.InvalidCodeMessageError);
			bill.ValidationModes = ValidationModes.None;
			bill.B0_Firms = "Z!";
			AssertNoMessageErrorContaining(bill.B0_FirmsInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.B0_FirmsInfo, ListValidation.InvalidCodeMessageError);
			bill.B0_Firms = ZString.Empty;
			AssertNoMessageErrorContaining(bill.B0_FirmsInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(bill.B0_FirmsInfo, ListValidation.InvalidCodeMessageError);
			bill.ValidationModes = ValidationModes.PermitToTransfer;
			bill.B0_Firms = ZString.Empty;
			AssertHasMessageErrorContaining(bill.B0_FirmsInfo, MandatoryValidation.YouHaveNotEntered);
			bill.B0_Firms = "KD23";
			AssertNoMessageErrorContaining(bill.B0_FirmsInfo, MandatoryValidation.YouHaveNotEntered);
			header.BH_PortUnladingDCode = "2704";
			bill.Validation.ValidateB0_Firms();
			AssertHasMessageError(bill.B0_FirmsInfo, ValidationConstants.Bill.FIRMSDoesNotMatchDDPP.ToString());
			header.BH_PortUnladingDCode = "3902";
			bill.Validation.ValidateB0_Firms();
			AssertNoMessageError(bill.B0_FirmsInfo, ValidationConstants.Bill.FIRMSDoesNotMatchDDPP.ToString());
			bill.ValidationModes = ValidationModes.InventoryRecord;
			header.BH_PortUnladingDCode = "2704";
			bill.Validation.ValidateB0_Firms();
			AssertHasWarning(bill.B0_FirmsInfo, ValidationConstants.Bill.FIRMSDoesNotMatchDDPP.ToString());
			header.BH_PortUnladingDCode = "3902";
			bill.Validation.ValidateB0_Firms();
			AssertNoWarning(bill.B0_FirmsInfo, ValidationConstants.Bill.FIRMSDoesNotMatchDDPP.ToString());
		}

		public void TestCheckB0_MasterBillNumberWithoutSCAC()
		{
			bill.ValidationModes = ValidationModes.InventoryRecord;
			bill.B0_MasterBillNumber = ZString.Empty;
			bill.B0_IssuerCode = "ABCE";
			AssertHasMessageErrorContaining(bill.B0_MasterBillNumberInfo, MandatoryValidation.YouHaveNotEntered);
			bill.B0_MasterBillNumber = " HB240417 ";
			AssertNoMessageError(bill.B0_MasterBillNumberInfo, ValidationConstants.Bill.MasterNumberIsInvalidWithSCACCode.ToString());
			bill.B0_MasterBillNumber = " ABCEHB240417 ";
			AssertHasMessageError(bill.B0_MasterBillNumberInfo, ValidationConstants.Bill.MasterNumberIsInvalidWithSCACCode.ToString());
		}

		protected CusInBondHeader header;
		protected CusInBondMoveHeader moveHeader;
		protected CusInBondBill bill;
		protected CusInBondMoveDetail moveDetail;
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusInBondHeader>();
			moveHeader = header.MovementHeader;
			bill = CreateBill(header);
			moveDetail = bill.MovementDetail;
		}

		protected abstract CusInBondBill CreateBill(CusInBondHeader header);
	}
}
