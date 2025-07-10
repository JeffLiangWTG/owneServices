using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.NO.Business;

class CusEntryInstructionCollection(JobDeclaration declaration) : Customs.Business.CusEntryInstructionCollection<CusEntryInstruction>(declaration)
{
	JobDeclaration Declaration { get; } = declaration;

	protected override void SetDefaultsForNewChild(BusinessObject child)
	{
		Argument.NotNull(child, nameof(child));
		base.SetDefaultsForNewChild(child);
		if (child is CusEntryInstruction instruction)
		{
			instruction.SetDefaultsForNewChild(Declaration);
		}
	}
}
