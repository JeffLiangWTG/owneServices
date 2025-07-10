using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>))]
	public class EntryInstructionsCoreDetailsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>, CusEntryInstruction, EntryInstructionBasicDetailsControlBag>
	{
		protected override EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction> GetColumnLayoutBuilderForTesting() => new EntryInstructionsCoreDetailsLayoutBuilder<CusEntryInstruction>();

		protected override int ExpectedMaxColumns => 2;
	}
}
