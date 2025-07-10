using System;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class BillOfLadingContainersPage : ZUserControl
	{
		public BillOfLadingContainersPage()
		{
			InitializeComponent();

			importReleaseOrderStatusStyleInfo.CaptionResourceString = CommonContainer.ImportReleaseOrderStatusStringData;
			importReleaseNumberStyleInfo.CaptionResourceString = CommonContainer.ImportReleaseNumberStringData;
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (!this.DesignMode)
			{
				ContainerCustomColumnAdder.Set(this.containersGrid, ContainerCustomColumnAdder.TargetGridType.Editable);

				this.workflowFormHelper = new AgencyContainerWorkflowFormHelper(this.containersGrid);
				this.workflowFormHelper.Hook();
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


