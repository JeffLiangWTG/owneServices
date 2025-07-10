using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.Declaration;

public class AddInfoCusEntryInstructionLookups : EU.Business.Declaration.AddInfoCusEntryInstructionLookups
{
	public AddInfoCusEntryInstructionLookups(AddInfoCusEntryInstruction parent) : base(parent)
	{
	}

	public new AddInfoCusEntryInstruction Parent => (AddInfoCusEntryInstruction)base.Parent;

	public CodeDescriptionPairList EadPrintOutList => Factory.GetCachedValue<EadPrintOutList>();

	public CodeDescriptionPairList TemporaryLocationCodeTypeList => Factory.GetCachedValue<TemporaryLocationCodeTypeList>();
}
