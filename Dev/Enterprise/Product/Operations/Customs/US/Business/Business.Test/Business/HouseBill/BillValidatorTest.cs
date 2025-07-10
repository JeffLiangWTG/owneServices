using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class BillValidatorTest : Customs.Business.Testing.BillValidatorTestClass
	{
		public void TestDuplicateJE_MasterBillWarning()
		{
			var jobDec = Factory.NewWithValidTestData<JobDeclaration>();
			jobDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDec.JE_TransportMode = TransportTypeList.Codes.Truck;
			jobDec.JE_MasterBillIssuerSCAC = "5555";
			jobDec.JE_MasterBill = "33377776661";
			jobDec.JE_DeclarationReference = "MBJOB1";
			Factory.Save();

			var newDec = Factory.NewWithValidTestData<JobDeclaration>();
			newDec.JE_TransportMode = TransportTypeList.Codes.Truck;
			newDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			newDec.JE_MasterBillIssuerSCAC = "5555";
			newDec.JE_MasterBill = "33377776661";
			newDec.JE_DeclarationReference = "MBJOB2";

			var message = BillValidator.AlreadyContainsMasterBill(jobDec.JobNumber, GlbCompany.CurrentCompany.CompanyName, GlbBranch.CurrentBranch.GB_BranchName);
			newDec.Validation.ValidateJE_MasterBill();
			AssertHasWarningContaining(newDec.JE_MasterBillInfo, message);

			newDec.JE_MasterBillIssuerSCAC = "8888";
			newDec.Validation.ValidateJE_MasterBill();
			AssertNoWarningContaining(newDec.JE_MasterBillInfo, message);

			var jobDec2 = Factory.NewWithValidTestData<JobDeclaration>();
			jobDec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDec2.JE_TransportMode = TransportTypeList.Codes.Air;
			jobDec2.JE_MasterBillIssuerSCAC = "5555";
			jobDec2.JE_MasterBill = "33377776661";
			jobDec2.JE_DeclarationReference = "MBJOBAIR1";
			Factory.Save();

			var newDec2 = Factory.NewWithValidTestData<JobDeclaration>();
			newDec2.JE_TransportMode = TransportTypeList.Codes.Air;
			newDec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			newDec2.JE_MasterBillIssuerSCAC = "5555";
			newDec2.JE_MasterBill = "33377776661";
			newDec2.JE_DeclarationReference = "MBJOBAIR2";

			message = BillValidator.AlreadyContainsMasterBill(jobDec2.JobNumber, GlbCompany.CurrentCompany.CompanyName, GlbBranch.CurrentBranch.GB_BranchName);
			newDec2.Validation.ValidateJE_MasterBill();
			AssertHasWarningContaining(newDec2.JE_MasterBillInfo, message);

			newDec2.JE_MasterBillIssuerSCAC = "8888";
			newDec2.Validation.ValidateJE_MasterBill();
			AssertNoWarningContaining(newDec2.JE_MasterBillInfo, message);
		}

		public void TestGetTruncatedBillNumber()
		{
			AssertEquals("TruncatedBillNumber", "H1234BL5679D", BillValidator.GetTruncatedBillNumber("H1-23?4\tB L5679DB"));
		}

		#region Static Assert for use in other test classes

		public static void AssertValidate(ZPropertyInfo billInfo, string billType)
		{
			var billInvalidCharactersWarningMessage = billType + BillValidator.Constants.BillInvalidCharacters;
			var billTooLongWarningMessage = billType +
				string.Format(BillValidator.Constants.BillTooLong, BillValidator.Constants.MaximumBillLength) +
				BillValidator.Constants.BillPrefix;

			billInfo.Value = new ZString("H1234BL56789122");
			TestCaseWithFactory.AssertHasWarning(billInfo, billTooLongWarningMessage);
			TestCaseWithFactory.AssertNoWarning(billInfo, billInvalidCharactersWarningMessage);

			billInfo.Value = new ZString("H1?34 L56789");
			TestCaseWithFactory.AssertNoWarning(billInfo, billTooLongWarningMessage);
			TestCaseWithFactory.AssertHasWarning(billInfo, billInvalidCharactersWarningMessage);

			billInfo.Value = new ZString("H1234BL56789");
			TestCaseWithFactory.AssertNoWarning(billInfo, billTooLongWarningMessage);
			TestCaseWithFactory.AssertNoWarning(billInfo, billInvalidCharactersWarningMessage);
		}

		#endregion
	}
}
