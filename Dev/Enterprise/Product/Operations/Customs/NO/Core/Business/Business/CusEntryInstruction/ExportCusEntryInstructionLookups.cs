using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business;

public sealed class ExportCusEntryInstructionLookups(CusEntryInstruction cusEntryInstruction) : CusEntryInstructionLookups(cusEntryInstruction)
{
	public override CodeDescriptionPairList StyleList => Factory.GetCachedValue<ExportCusEntryInstructionTypes>();
	public override CodeDescriptionPairList EntrySubStyleList => Factory.GetCachedValue<ExportDeclarationSubTypes>();
}
