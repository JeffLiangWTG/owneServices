using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class EntryInstructionProviderTest : TestCaseWithFactory
{
	public void TestCusEntryInstructionCollection()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertType<EU.Business.Declaration.CusEntryInstructionCollection<CusEntryInstruction>>(declaration.CustomsEntryInstructions);
	}

	public void TestCusEntryInstructionComparer()
	{
		var declaration = Factory.New<JobDeclaration>();
		AssertType<CusEntryInstructionComparer>(declaration.CustomsEntryInstructionProvider.EntryInstructionComparer);
	}
}
