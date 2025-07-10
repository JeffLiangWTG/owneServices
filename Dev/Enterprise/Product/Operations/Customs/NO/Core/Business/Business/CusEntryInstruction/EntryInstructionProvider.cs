using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business;

public sealed class EntryInstructionProvider(JobDeclaration declaration) : Customs.Business.EntryInstructionProvider(declaration)
{
	new JobDeclaration ParentDeclaration { get; } = declaration;

	protected override ICusEntryInstructionCollection<Customs.Business.CusEntryInstruction> GetNewCusEntryInstructionCollectionCore() => new CusEntryInstructionCollection(ParentDeclaration);

	public new ICusEntryInstructionCollection<CusEntryInstruction> CustomsEntryInstructions => (CusEntryInstructionCollection<CusEntryInstruction>)base.CustomsEntryInstructions;
}
