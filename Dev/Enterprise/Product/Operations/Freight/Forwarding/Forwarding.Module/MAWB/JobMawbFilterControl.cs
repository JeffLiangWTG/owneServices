using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Module
{
	public partial class JobMawbFilterControl : ZFilterStripControl<JobMawbModuleStrip>
	{
		public JobMawbFilterControl()
		{
			InitializeComponent();
		}

		public JobMawbFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		#region Allocate

		public void HandleAllocateClick(object sender, EventArgs e)
		{
			bool anyAllocatedWaybills = false;
			bool anyBorrowedOutWaybills = false;

			foreach (JobMawb job in FilteredGrid.SelectedElements)
			{
				if (!job.JM_OH_AllocatedTo.IsEmpty)
				{
					anyAllocatedWaybills = true;
				}
				if (!job.JM_OA_From.IsEmpty)
				{
					anyBorrowedOutWaybills = true;
				}
			}

			string caption = Res.GetString("c54376ae-7f91-4bfb-925f-c47b8e9eeb3d", "Selection contains already borrowed Waybills");
			string message = Res.GetString("926b76b4-3a8f-403e-b723-3aeb0664cd92", "Your selection contained Waybills which have already been borrowed. Are you sure you want to continue?");

			if (!anyBorrowedOutWaybills)
			{
				if (anyAllocatedWaybills && (Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes))
				{
					LaunchAllocationForm(FilteredGrid.SelectedElements);
				}
				if (!anyBorrowedOutWaybills && !anyAllocatedWaybills)
				{
					LaunchAllocationForm(FilteredGrid.SelectedElements);
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("f1734b66-b911-4bdf-8068-72e9b23299b0", "You cannot borrow out borrowed waybills. Please reselect."), Res.GetString("a3f976dd-c38c-4ecf-85db-ae638539d0d5", "Borrowed Out Waybills exist in selection"), MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		void LaunchAllocationForm(BusinessObject[] jobMawbs)
		{
			AllocateSelectionJobMawb jobMawbAllocateSelection = new AllocateSelectionJobMawb(new BusinessObjectFactory(), jobMawbs);
			ZFormModaliser.ShowDialogAndDispose(new AllocateSelectionJobMawbForm(jobMawbAllocateSelection));
		}

		#endregion
	}
}
