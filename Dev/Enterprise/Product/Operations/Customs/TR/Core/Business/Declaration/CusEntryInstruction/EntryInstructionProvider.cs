using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class EntryInstructionProvider : EU.Business.Declaration.EntryInstructionProvider
	{
		public EntryInstructionProvider(EU.Business.Declaration.JobDeclaration declaration) : base(declaration)
		{
		}

		protected override ICusEntryInstructionCollection<Customs.Business.CusEntryInstruction> GetNewCusEntryInstructionCollectionCore() => new EU.Business.Declaration.CusEntryInstructionCollection<CusEntryInstruction>(ParentDeclaration);
	}
}
