using System;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class ContainersUserControl : ZUserControl
	{
		public ContainersUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (!this.IsDesignMode())
			{
				workflowFormHelper = new AgencyContainerWorkflowFormHelper(FCLContainersGrid);
				workflowFormHelper.Hook();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && workflowFormHelper != null)
			{
				workflowFormHelper.Dispose();
				workflowFormHelper = null;
			}

			base.Dispose(disposing);
		}

		AgencyContainerWorkflowFormHelper workflowFormHelper;
	}
}
