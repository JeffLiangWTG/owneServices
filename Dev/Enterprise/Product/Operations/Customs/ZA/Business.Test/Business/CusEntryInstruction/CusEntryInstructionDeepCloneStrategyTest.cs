using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class CusEntryInstructionDeepCloneStrategyTest : TestCaseWithFactory
	{
		public void TestCloneEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_CustomsOfficeOverride = "ABC";
			entryInstruction.CEI_Style = "11";

			Factory.Save();

			var cloneStrategy = new CusEntryInstructionDeepCloneStrategy(entryInstruction, Customs.Business.CloneType.TemplateCopy, new CargoWise.EntityFramework.BusinessObjectFactory());
			var cloneEntryInstruction = (CusEntryInstruction)cloneStrategy.Clone();

			CombineAssertions(() =>
			{
				AssertEquals("CEI_CustomsOfficeOverride should be copied", "ABC", cloneEntryInstruction.CEI_CustomsOfficeOverride);
				AssertEquals("CEI_Style should be 11", "11", cloneEntryInstruction.CEI_Style);
			});
		}

		public void TestCaseNumbersNotCopied()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CaseNumbers.AddNew(CaseNumberTypeList.Codes.SupportingDocsRequired, "CAS123");
			var cloneEntryInstruction = (CusEntryInstruction)new CusEntryInstructionDeepCloneStrategy(entryInstruction, Customs.Business.CloneType.TemplateCopy, Factory).Clone();
			AssertEquals("Case numbers should not be cloned", 0, cloneEntryInstruction.CaseNumbers.Count);
		}
	}
}
