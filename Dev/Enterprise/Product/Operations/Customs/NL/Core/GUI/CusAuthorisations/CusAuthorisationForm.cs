using Enterprise.Customs.NL.Business;

namespace Enterprise.Customs.NL.GUI;

public partial class CusAuthorisationForm : Customs.GUI.CusAuthorisationForm
{
	public CusAuthorisationForm(CusAuthorisationHeader header) : base(header)
	{
		InitializeComponent();
		RemoveUnneededColums();
	}

	void RemoveUnneededColums()
	{
		AuthorisationRuleGrid.ColumnStyles.Remove(AuthorisationRuleGrid.GetColumnStyle("CPR_ValueFrom"));
	}
}

