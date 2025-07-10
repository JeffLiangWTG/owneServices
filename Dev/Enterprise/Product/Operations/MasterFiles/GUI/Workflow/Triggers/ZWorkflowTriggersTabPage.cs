using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	sealed class ZWorkflowTriggersTabPage : ZBindingTabPage
	{
		public ZWorkflowTriggersTabPage()
		{
		}

		protected override void SetDataBindingCore(object dataSource, string dataMember)
		{
			if (!string.IsNullOrEmpty(dataMember))
			{
				throw new ArgumentException("dataMember is not supported for this control", nameof(dataMember));
			}
			if (Controls.Count == 0)
			{
				Controls.Add(WorkflowTriggersUserControl);
			}
			workflowProvider = (IWorkflowProvider)dataSource;

			var collectionForBinding = workflowProvider.WorkflowItems.TriggersIncludingRelated;

			WorkflowTriggersUserControl.SetDataBinding(collectionForBinding, "");
			base.SetDataBindingCore(collectionForBinding, "");
		}

		public ZWorkflowTriggersUserControl WorkflowTriggersUserControl
		{
			get
			{
				if (workflowTriggersUserControl == null)
				{
					workflowTriggersUserControl = new ZWorkflowTriggersUserControl();
					workflowTriggersUserControl.Dock = DockStyle.Fill;
				}
				return workflowTriggersUserControl;
			}
		}
		ZWorkflowTriggersUserControl workflowTriggersUserControl;

		IWorkflowProvider workflowProvider;
	}
}
