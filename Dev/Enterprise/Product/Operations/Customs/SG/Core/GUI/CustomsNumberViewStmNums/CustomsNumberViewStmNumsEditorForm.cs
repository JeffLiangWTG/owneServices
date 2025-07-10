using Enterprise.Customs.SG.V4.Business;

namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class CustomsNumberViewStmNumsEditorForm : MasterFiles.GUI.CustomsNumberViewStmNumsCompanyEditorForm
	{
		public CustomsNumberViewStmNumsEditorForm(SGCustomsNumberViewStmNumsWrapper stmNums)
			: base(stmNums)
		{
			InitializeComponent();
		}
	}
}
