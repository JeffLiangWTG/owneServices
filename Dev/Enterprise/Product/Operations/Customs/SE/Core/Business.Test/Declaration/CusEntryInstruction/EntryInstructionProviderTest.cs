using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SE.Business.Declaration.Testing
{
	[TestedType(typeof(EntryInstructionProvider))]
	sealed class EntryInstructionProviderTest : TestCaseWithFactory
	{
		public void TestCusEntryInstructionCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstructionProvider = new EntryInstructionProvider(declaration);
			AssertType<EU.Business.Declaration.CusEntryInstructionCollection<CusEntryInstruction>>(entryInstructionProvider.CustomsEntryInstructions);
		}
	}
}
