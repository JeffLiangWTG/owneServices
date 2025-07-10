using System;
using CargoWise.Types;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class MiscOptionsUserControl : BaseMiscOptionsUserControl
	{
		public MiscOptionsUserControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			JE_RS_NKServiceLevelBoundFindBox.Visible = JobDeclaration?.ServiceLevelVisible ?? ZBool.False;
		}

		protected override void ChangeControlsVisibility()
		{
			PaidByDropEdit.Visible = true;
		}
	}
}
