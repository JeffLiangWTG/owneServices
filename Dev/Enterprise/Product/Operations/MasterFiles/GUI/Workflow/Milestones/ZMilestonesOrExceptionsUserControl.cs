using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	[SuppressFormDesignerAnalysis]
	public class ZMilestonesOrExceptionsUserControl : ZUserControl, IWorkflowTasksControl
	{
		protected ZMilestonesOrExceptionsUserControl()
		{
		}

		protected override void OnCreateControl()
		{
			base.OnCreateControl();
			if (!DesignModeFinder.IsDesigning)
			{
				Grid.ColourDeciding += Grid_ColourDeciding;
			}
		}

		[Browsable(false)]
		protected virtual ZGrid Grid
		{
			get { throw new InvalidOperationException("You must override Grid in class " + GetType().Name); }
		}

		protected virtual ColourLegend ColourLegend
		{
			get { throw new InvalidOperationException("You must override ColourLegend in class " + GetType().Name); }
		}

		protected virtual ZLabel TasksHintLabel => throw new InvalidOperationException("You know the drill. Override this in class" + GetType().Name);

		protected virtual KSplitContainer SplitContainer => throw new InvalidOperationException("You know the drill. Override this in class" + GetType().Name);

		#region Implementation

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			dataBindingDisposable?.Dispose();
			base.SetDataBinding(dataSource, dataMember);

			var workflowProvider = WorkflowProvider;
			if (workflowProvider != null)
			{
				dataBindingDisposable = this.SetupControl(WorkflowProvider, saveOrder: true);
			}
		}

		IDisposable dataBindingDisposable;

		IWorkflowProviderCollection WorkflowProvider
		{
			get
			{
				BindingManagerBase bindingManager = BindingSource.DataSource == null ? null : BindingContext[BindingSource.DataSource, BindingSource.DataMember];
				CurrencyManager listManager = bindingManager as CurrencyManager;
				IWorkflowProviderCollection result = null;
				if (listManager != null)
				{
					result = listManager.List as IWorkflowProviderCollection;
				}
				if (result == null && bindingManager?.Position != 0)
				{
					result = bindingManager.GetCurrent() as IWorkflowProviderCollection;
				}
				return result;
			}
		}

		ZGrid IWorkflowItemsControl.TasksGrid => Grid;

		ZLabel IWorkflowTasksControl.TasksHintLabel => TasksHintLabel;

		KSplitContainer IWorkflowTasksControl.TasksHintSplitContainer => SplitContainer;

		void Grid_ColourDeciding(object sender, ColourDecidingEventArgs e)
		{
			ProcessTask item = (ProcessTask)e.ObjectAtRow;
			if (item.Parent != null && item.Parent != WorkflowProvider?.WorkflowItems.Parent)
			{
				BusinessObject parent = (BusinessObject)item.Parent;
				e.Colour = ColourLegend.GetColour(parent.PK, parent.HumanReadableName);
			}
		}

		protected override void Dispose(bool disposing)
		{
			dataBindingDisposable?.Dispose();
			base.Dispose(disposing);
		}

		#endregion
	}
}
