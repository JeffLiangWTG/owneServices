using System.Linq;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ExportCusEntryLineValidation : CusEntryLineValidation
{
	public ExportCusEntryLineValidation(EU.Business.Declaration.CusEntryLine parent) : base(parent)
	{
	}

	public override void ValidateAll()
	{
		base.ValidateAll();
		CheckRuleR0022E();
	}

	void CheckRuleR0022E()
	{
		if (Parent.PreviousDocuments.Count(x => x.CSI_Code == PreviousDocumentCodes.AAD) > 1)
		{
			Parent.AddRowMessageError(Res.GetString("PLExportCusEntryLineValidation|R0022EMessageError", "(R0022E) Only one AAD previous document is allowed for Entry Line"));
		}
	}
}
