using CargoWise.Types;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusEntryInstructionDocumentValidationTest : CusCodeDataValidationTest
	{
		public new void TestCheckCY_Code()
		{
			var cusEntryInstructionDocument = Factory.New<CusEntryInstructionDocument>();
			cusEntryInstructionDocument.CY_Code = ZString.Empty;
			AssertNoMessageErrors("No message error on CY_Code", cusEntryInstructionDocument.CY_CodeInfo);
			cusEntryInstructionDocument.CY_Code = "XXXX";
			AssertNoMessageErrors("No message error on CY_Code", cusEntryInstructionDocument.CY_CodeInfo);
		}

		public void TestCheckCY_Data()
		{
			var cusEntryInstructionDocument = Factory.New<CusEntryInstructionDocument>();
			ValidationTestHelper.AssertErrorIfNotEntered(cusEntryInstructionDocument.CY_DataInfo);
		}
	}
}
