using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	public class SPTSBillValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckB0_MasterBillNumber()
		{
			var bill = Factory.New<SPTSBill>();

			CombineAssertions("Test for Checking The Entrance of The Field", () =>
			{
				bill.B0_MasterBillNumber = "MBN123";
				AssertNoMessageErrorContaining(bill.B0_MasterBillNumberInfo, MandatoryValidation.YouHaveNotEntered);

				bill.B0_MasterBillNumber = ZString.Empty;
				AssertHasMessageErrorContaining(bill.B0_MasterBillNumberInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckB0_ReferenceQualifier()
		{
			var bill = Factory.New<SPTSBill>();

			CombineAssertions("Test for Checking The Entrance of The Field", () =>
			{
				bill.B0_ReferenceQualifier = SPTSDeclarationTypeList.Codes.TD;
				AssertNoMessageErrorContaining(bill.B0_ReferenceQualifierInfo, ListValidation.InvalidCodeMessageError);

				bill.B0_ReferenceQualifier = "XXX";
				AssertHasMessageErrorContaining(bill.B0_ReferenceQualifierInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckB0_ReferenceID()
		{
			var bill = Factory.New<SPTSBill>();

			CombineAssertions("Test for Checking The Entrance of The Field", () =>
			{
				bill.B0_ReferenceQualifier = SPTSDeclarationTypeList.Codes.K;
				bill.B0_ReferenceID = ZString.Empty;
				AssertNoMessageErrorContaining(bill.B0_ReferenceIDInfo, "You have not entered a Registration No.");

				bill.B0_ReferenceID = "12345";
				AssertNoMessageErrorContaining(bill.B0_ReferenceIDInfo, "You have not entered a Registration No.");

				bill.B0_ReferenceQualifier = SPTSDeclarationTypeList.Codes.TD;
				bill.B0_ReferenceID = ZString.Empty;
				AssertHasMessageErrorContaining(bill.B0_ReferenceIDInfo, "You have not entered a Registration No.");

				bill.B0_ReferenceQualifier = ZString.Empty;
				bill.B0_ReferenceID = "12345";
				AssertNoMessageErrorContaining(bill.B0_ReferenceIDInfo, "You have not entered a Registration No.");
			});
		}
	}
}
