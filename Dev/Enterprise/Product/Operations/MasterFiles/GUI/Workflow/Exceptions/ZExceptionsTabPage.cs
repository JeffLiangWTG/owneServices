using System;
using System.Windows.Forms;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	internal class ZExceptionsTabPage : ZBindingTabPage
	{
		public virtual ZExceptionsUserControl ExceptionsUserControl
		{
			get
			{
				if (exceptionsUserControl == null)
				{
					exceptionsUserControl = new ZExceptionsUserControl();
					exceptionsUserControl.Dock = DockStyle.Fill;
				}
				return exceptionsUserControl;
			}
		}
		ZExceptionsUserControl exceptionsUserControl;

		protected override void SetDataBindingCore(object dataSource, string dataMember)
		{
			if (!string.IsNullOrEmpty(dataMember))
			{
				throw new ArgumentException("dataMember is not supported for this control", nameof(dataMember));
			}
			if (Controls.Count == 0)
			{
				Controls.Add(ExceptionsUserControl);
			}

			workflowProvider = (IWorkflowProvider)dataSource;

			if (!(workflowProvider.WorkflowItems is IExceptionDurationSupporter))
			{
				ExceptionsUserControl.RemoveDurationRelatedFields();
			}

			ExceptionsUserControl.SetDataBinding(workflowProvider.WorkflowItems.ExceptionsIncludingRelated, "");
			base.SetDataBindingCore(workflowProvider.WorkflowItems.ExceptionsIncludingRelated, "");
		}

		IWorkflowProvider workflowProvider;

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				workflowProvider = null;
			}
			base.Dispose(isNotFinalizing);
		}
	}
}
