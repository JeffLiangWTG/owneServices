using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.General
{
	partial class StmALogCreateEventUserControl : ZStmALogAddUserControl
	{
		protected override void InitializeComponent()
		{
			base.InitializeComponent();
			SL_EventTimeBoundDateEdit.Visible = false;
		}
	}
}
