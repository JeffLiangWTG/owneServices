using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business;

public abstract class CusEntryInstructionLookups : Customs.Business.CusEntryInstructionLookups
{
	protected CusEntryInstructionLookups(CusEntryInstruction cusEntryInstruction)
		: base(cusEntryInstruction)
	{
	}

	new CusEntryInstruction Parent => base.Parent as CusEntryInstruction;

	public CodeDescriptionPairList CopyStatusList => Factory.GetCachedValue<NODeclarationCopyStatus>();

	public CodeDescriptionPairList ProcedureList
	{
		get
		{
			var result = new CodeDescriptionPairList();
			if (Parent.JobDeclaration is { } jobDeclaration)
			{
				var style = Parent.CEI_Style;
				var messageType = jobDeclaration.JE_MessageType;
				result = Factory.GetCachedValue(string.Join("-", "NO", nameof(ProcedureList), style, messageType), () =>
				{
					var list = new CodeDescriptionPairList();
					RefCusProcedureLoader.GetAllApplicableProcedures(Factory, Parent)
						.ForEach(p => list.AddPair(p.Key, p.Value.ZZ6_Description));
					return list;
				});
			}
			return result;
		}
	}
}
