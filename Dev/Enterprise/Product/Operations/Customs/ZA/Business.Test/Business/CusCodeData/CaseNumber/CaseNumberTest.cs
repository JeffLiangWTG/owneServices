using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(CaseNumber))]
	sealed class CaseNumberTest : Customs.Business.Testing.CusCodeDataTest<CaseNumber>
	{
		public void TestStatusDescription()
		{
			var caseNumber = Factory.New<CaseNumber>();
			caseNumber.Document_Status = DocumentStatusCodes.Codes.PND;
			AssertEquals(DocumentStatusCodes.Descriptions.PND, caseNumber.Description);
			caseNumber.Document_Status = DocumentStatusCodes.Codes.SNT;
			AssertEquals(DocumentStatusCodes.Descriptions.SNT, caseNumber.Description);
			caseNumber.Document_Status = DocumentStatusCodes.Codes.FAL;
			AssertEquals(DocumentStatusCodes.Descriptions.FAL, caseNumber.Description);
		}

		public void TestCaseNumberDefaultValues()
		{
			var cusEntryInstruction = Factory.New<CusEntryInstruction>();
			var caseNumber = cusEntryInstruction.CaseNumbers.AddNew();
			Assert(caseNumber.CY_ParentTableCode == CusEntryInstructionSchema.Constants.Prefix);
			Assert(caseNumber.CY_Type == CusCodeDataTypeList.Codes.CaseNumber);
			Assert(caseNumber.CY_Code == CaseNumberTypeList.Codes.DocumentInspectionCases);
		}

		public void TestDescription_ReadOnly()
		{
			var caseNumber = Factory.New<CaseNumber>();
			Assert(caseNumber.Description_ReadOnly);
		}

		public void TestDocument_Status()
		{
			var cusEntryInstruction = Factory.New<CusEntryInstruction>();
			var caseNumber = cusEntryInstruction.CaseNumbers.AddNew();
			caseNumber.Document_Status = DocumentStatusCodes.Codes.PND;
			AssertEquals(DocumentStatusCodes.Codes.PND, cusEntryInstruction.CaseNumbers[0].Document_Status);
			caseNumber.Document_Status = DocumentStatusCodes.Codes.FAL;
			AssertEquals(DocumentStatusCodes.Codes.FAL, cusEntryInstruction.CaseNumbers[0].Document_Status);
			caseNumber.Document_Status = DocumentStatusCodes.Codes.SNT;
			AssertEquals(DocumentStatusCodes.Codes.SNT, cusEntryInstruction.CaseNumbers[0].Document_Status);
			AssertEquals(true, cusEntryInstruction.CaseNumbers[0].Document_StatusInfo.ReadOnly);
		}
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			return entryInstruction.CaseNumbers.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<CaseNumber>();
	}
}
