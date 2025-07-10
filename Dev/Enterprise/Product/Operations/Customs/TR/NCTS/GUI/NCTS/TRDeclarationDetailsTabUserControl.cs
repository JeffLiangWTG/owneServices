using System;

namespace Enterprise.Customs.TR.NCTS.GUI
{
	public partial class TRDeclarationDetailsTabUserControl : EU.NCTS.GUI.DeclarationDetailsTabUserControl
	{
		public TRDeclarationDetailsTabUserControl()
		{
			InitializeComponent();
			MoveToFTZVisibility();
		}

		protected void MoveToFTZInfo_ValueChanged(object sender, EventArgs e)
		{
			MoveToFTZVisibility();
		}

		void MoveToFTZVisibility()
		{
			BM_CustomsOfficeAtBorderDropEdit.Visible = BM_MoveToFTZCheckBox.Checked;
		}
	}
}
