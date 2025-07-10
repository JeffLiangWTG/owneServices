using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class EntryInstructionProvider : EU.Business.Declaration.EntryInstructionProvider
{
	public EntryInstructionProvider(EU.Business.Declaration.JobDeclaration declaration, CusEntryInstructionComparer comparer) : base(declaration, comparer)
	{
	}

	public EntryInstructionProvider(EU.Business.Declaration.JobDeclaration declaration) : base(declaration)
	{
	}

	protected new JobDeclaration ParentDeclaration => base.ParentDeclaration as JobDeclaration;

	protected override ICusEntryInstructionCollection<Customs.Business.CusEntryInstruction> GetNewCusEntryInstructionCollectionCore() => new EU.Business.Declaration.CusEntryInstructionCollection<CusEntryInstruction>(ParentDeclaration);

	public new ICusEntryInstructionCollection<CusEntryInstruction> CustomsEntryInstructions => (ICusEntryInstructionCollection<CusEntryInstruction>)base.CustomsEntryInstructions;
}
