using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CaseNumberValidation : CusCodeDataValidationTest
	{
		public void TestCheckCY_Data()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			var caseNumber1 = instruction.CaseNumbers.AddNew();
			caseNumber1.CY_Data = "";
			AssertHasMessageErrorContaining(caseNumber1.CY_DataInfo, "Please enter a case number.");
			caseNumber1.CY_Data = "XX";
			AssertNoNotifications(caseNumber1.CY_DataInfo);
			var caseNumber2 = instruction.CaseNumbers.AddNew();
			caseNumber2.CY_Data = "XX";
			AssertHasMessageErrorContaining(caseNumber2.CY_DataInfo, "This code is duplicated. Only one occurrence of each case number is allowed.");
			caseNumber2.CY_Data = "YY";
			AssertNoNotifications(caseNumber2.CY_DataInfo);
		}
	}
}
