using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.SE.Business.Declaration
{
	public class EntryInstructionProvider : EU.Business.Declaration.EntryInstructionProvider
	{
		public EntryInstructionProvider(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected new JobDeclaration ParentDeclaration => (JobDeclaration)base.ParentDeclaration;

		protected override Customs.Business.ICusEntryInstructionCollection<Customs.Business.CusEntryInstruction> GetNewCusEntryInstructionCollectionCore() => new CusEntryInstructionCollection<CusEntryInstruction>(ParentDeclaration);
	}
}
