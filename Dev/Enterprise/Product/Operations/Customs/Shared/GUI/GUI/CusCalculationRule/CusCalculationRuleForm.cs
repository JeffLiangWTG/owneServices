using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class CusCalculationRuleForm : ZChildForm
	{
		public CusCalculationRuleForm()
		{
			InitializeComponent();
		}

		public override string FormCaption => ResString.GetMultilingualString("4A14FB63-6D86-4677-9943-8F58F35EB057", "Customs Calculation Rule");
	}
}
