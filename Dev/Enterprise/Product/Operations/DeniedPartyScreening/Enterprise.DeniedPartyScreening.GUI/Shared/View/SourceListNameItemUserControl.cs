using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public partial class SourceListNameItemUserControl : ZUserControl
	{
		public SourceListNameItemUserControl()
		{
			InitializeComponent();
		}

		SourceListNameWinModel SourceListNameWinModel { get; set; }

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource is SourceListNameWinModel sourceListNameWinModel)
			{
				SourceListNameWinModel = sourceListNameWinModel;

				base.SetDataBinding(SourceListNameWinModel, dataMember);
				OpenSourceListLinkLabel.Text = SourceListNameWinModel.Name;
			}
		}

		protected void OpenSourceListLinkLabel_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			SourceListNameWinModel?.OpenComplianceForm();
		}
	}
}
