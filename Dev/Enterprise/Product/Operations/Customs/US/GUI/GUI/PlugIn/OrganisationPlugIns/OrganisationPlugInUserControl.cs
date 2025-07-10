using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.GUI
{
	public partial class OrganisationPlugInUserControl : Customs.GUI.MessageUserControl
	{
		public OrganisationPlugInUserControl()
		{
			InitializeComponent();
			this.MessagesGrid.RemoveFromAvailableColumns(EDIMessageSchema.Constants.EM_MessageSubType);
			MessagesTabControl.SelectedTab = MessageDetailsTabPage;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (StatusesErrorsLabel.DataBindings[StatusLableVisibilityForBinding] == null && dataSource != null)
			{
				StatusesErrorsLabel.DataBindings.Add(new KBinding(StatusLableVisibilityForBinding, dataSource, "Messages.StatusesAndErrorsVisible", true, DataSourceUpdateMode.Never));
			}
		}
		const string StatusLableVisibilityForBinding = "IsVisibleForBinding";

		void StatusesErrorsLabel_VisibleChanged(object sender, EventArgs e)
		{
			if (this.StatusesErrorsLabel.Visible || (this.MessagesGrid.ListManager != null && this.MessagesGrid.ListManager.Count <= 0))
			{
				this.StatusesErrorsLabel.BringToFront();
			}
		}
	}
}
