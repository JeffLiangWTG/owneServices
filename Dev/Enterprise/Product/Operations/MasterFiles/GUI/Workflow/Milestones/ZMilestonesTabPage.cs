using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	internal class ZMilestonesTabPage : ZBindingTabPage
	{
		public virtual ZMilestonesUserControl MilestonesUserControl
		{
			get
			{
				if (milestonesUserControl == null)
				{
					milestonesUserControl = new ZMilestonesUserControl();
					milestonesUserControl.Dock = DockStyle.Fill;
				}
				return milestonesUserControl;
			}
		}
		ZMilestonesUserControl milestonesUserControl;

		protected override void SetDataBindingCore(object dataSource, string dataMember)
		{
			if (!string.IsNullOrEmpty(dataMember))
			{
				throw new ArgumentException("dataMember is not supported for this control", nameof(dataMember));
			}
			if (Controls.Count == 0)
			{
				Controls.Add(MilestonesUserControl);
			}

			workflowProvider = (IWorkflowProvider)dataSource;

			object milestonesDataSource = workflowProvider.WorkflowItems.MilestonesIncludingRelatedSortable;
			MilestonesUserControl.SetDataBinding(milestonesDataSource, "");
			base.SetDataBindingCore(milestonesDataSource, "");
		}

		IWorkflowProvider workflowProvider;
	}
}
