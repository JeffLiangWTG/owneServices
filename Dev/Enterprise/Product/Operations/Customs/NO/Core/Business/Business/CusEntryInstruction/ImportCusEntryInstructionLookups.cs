using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business;

public sealed class ImportCusEntryInstructionLookups(CusEntryInstruction cusEntryInstruction) : CusEntryInstructionLookups(cusEntryInstruction)
{
	public override CodeDescriptionPairList StyleList => Factory.GetCachedValue<ImportCusEntryInstructionTypes>();
	public override CodeDescriptionPairList EntrySubStyleList => Factory.GetCachedValue<ImportDeclarationSubTypes>();
}
