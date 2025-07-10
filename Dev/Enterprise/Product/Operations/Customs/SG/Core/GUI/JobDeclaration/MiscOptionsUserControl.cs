using System;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class MiscOptionsUserControl : BaseMiscOptionsUserControl
	{
		public MiscOptionsUserControl()
			: base()
		{
			InitializeComponent();
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			PaidByDropEdit.Visible = false;
		}

		void MiscOptionsUserControl_Load(object sender, EventArgs e)
		{
			relatedDeclarationsUserControl2.SetDataBinding(JobDeclaration, "");
		}
	}
}
