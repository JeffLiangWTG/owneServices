using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ExportAddInfoCusEntryInstructionValidation : AddInfoCusEntryInstructionValidation
{
	public ExportAddInfoCusEntryInstructionValidation(AddInfoCusEntryInstruction parent) : base(parent)
	{
	}

	CusEntryInstruction ParentInstruction => Parent?.Parent;

	#region ZG_EADPrintOut

	protected override void CheckZG_EADPrintOut()
	{
		base.CheckZG_EADPrintOut();

		CheckRuleR523();
	}

	void CheckRuleR523()
	{
		if ((Parent.ZG_EADPrintOut == EadPrintOutList.Codes._1) &&
			!(ParentInstruction.JobDeclaration.JE_OfficeOfEntryExit.Left(2) == CountryCodes.Poland))
		{
			Parent.ZG_EADPrintOutInfo.AddMessageError(Res.GetString("ExportAddInfoCusEntryInstructionValidation|CheckRuleR523", "(R523) Invalid value for EAD generation (printout) type. Code 1 is allowed only when Exit Customs office code starts with PL."));
		}
	}

	#endregion
}
