using ECB = Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class EntryInstructionProvider : ECB.EntryInstructionProvider
	{
		public EntryInstructionProvider(JobDeclaration declaration, CusEntryInstructionComparer comparer) : base(declaration, comparer)
		{
		}

		protected new JobDeclaration ParentDeclaration => base.ParentDeclaration as JobDeclaration;

		protected override ECB.ICusEntryInstructionCollection<ECB.CusEntryInstruction> GetNewCusEntryInstructionCollectionCore() => new CusEntryInstructionCollection(ParentDeclaration);

		public new CusEntryInstructionCollection CustomsEntryInstructions => (CusEntryInstructionCollection)base.CustomsEntryInstructions;
	}
}
