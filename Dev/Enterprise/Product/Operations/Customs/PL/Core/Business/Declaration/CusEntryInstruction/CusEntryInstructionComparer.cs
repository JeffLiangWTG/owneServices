namespace Enterprise.Customs.PL.Business.Declaration;

public class CusEntryInstructionComparer : Customs.Business.CusEntryInstructionComparer
{
	protected override int CompareCore(Customs.Business.CusEntryInstruction x, Customs.Business.CusEntryInstruction y)
	{
		var result = x.CEI_SubStyle.CompareTo(y.CEI_SubStyle);
		if (result == 0)
		{
			result = x.CEI_Procedure.CompareTo(y.CEI_Procedure);
		}

		if (result == 0)
		{
			result = x.CEI_Description.CompareTo(y.CEI_Description);
		}

		return result;
	}
}
