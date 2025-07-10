namespace Enterprise.Customs.PL.Business.Declaration.Testing;

class AddInfoCusEntryInstructionLookupsTest : EU.Business.Declaration.Testing.AddInfoCusEntryInstructionLookupsTest
{
	public void TestLookups() => AssertType<AddInfoCusEntryInstructionLookups>(new AddInfoCusEntryInstruction(Factory.New<CusEntryInstruction>()).Lookups);

	public void TestValidation() => AssertType<AddInfoCusEntryInstructionValidation>(new AddInfoCusEntryInstruction(Factory.New<CusEntryInstruction>()).Validation);

	public void TestParent() => AssertType<AddInfoCusEntryInstruction>(new AddInfoCusEntryInstruction(Factory.New<CusEntryInstruction>()).Lookups.Parent);

	public void TestEadPrintOutList()
	{
		var instruction = new AddInfoCusEntryInstruction(Factory.New<CusEntryInstruction>());
		AssertEquals("Poland should contain specified list from EadPrintOutList",
			"0, 1, 2",
			instruction.Lookups.EadPrintOutList.CodesAsString);
	}

	public void TestTemporaryLocationCodeTypeList()
	{
		var instruction = new AddInfoCusEntryInstruction(Factory.New<CusEntryInstruction>());
		AssertEquals("Poland should contain specified list from TemporaryLocationCodeTypeList",
			"CODE, DESC",
			instruction.Lookups.TemporaryLocationCodeTypeList.CodesAsString);
	}
}
