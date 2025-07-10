using System;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CompletionTriggerActionsUserControl : ZUserControl, IWorkflowItemsControl
	{
		public CompletionTriggerActionsUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			dataBindingContext?.Dispose();
			base.SetDataBinding(dataSource, dataMember);
			dataBindingContext = this.SetupSortOnRebuild(saveOrder: true);
		}

		IDisposable dataBindingContext;

		ZGrid IWorkflowItemsControl.TasksGrid => CompletionTriggerActionGrid;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			dataBindingContext?.Dispose();

			base.Dispose(disposing);
		}
	}
}
