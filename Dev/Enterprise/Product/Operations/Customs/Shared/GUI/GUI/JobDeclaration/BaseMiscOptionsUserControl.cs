using Enterprise.Customs.Business;

namespace Enterprise.Customs.GUI
{
	public partial class BaseMiscOptionsUserControl : BaseCustomsEntryUserControl
	{
		public BaseMiscOptionsUserControl()
		{
			InitializeComponent();
		}

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			ChangeControlVisibilityOnMessageTypeChanged();
			ChangeControlsVisibility();
		}

		protected virtual void ChangeControlVisibilityOnMessageTypeChanged()
		{
		}

		protected override void ChangeControlsVisibility()
		{
			PaidByDropEdit.Visible = JobDeclaration.JE_ApplicationCode == DeclarationApplicationCodeList.Codes.Interfaced;
		}
	}
}
