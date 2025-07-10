using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public partial class OpportunityManagementControl : ZUserControl
	{
		public OpportunityManagementControl()
		{
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				SetExtraCategoryColumnCaption();
			}
			OpportunitiesGrid.EditFormClosed += RefreshAssignedOrgPK;
			OpportunitiesGrid.InnerGrid.DataSourceChanged += OpportunitiesGridStatusChanged;
		}

		void RefreshAssignedOrgPK(object sender, EventArgs e)
		{
			if (SelectedOpportunity != null)
			{
				SelectedOpportunity.ResetAssignedOrgPKCache();
				SelectedOpportunity.Validation.ValidateAssignedOrgPK();
			}
		}

		void OpportunitiesGridStatusChanged(object sender, EventArgs e)
		{
			if (IsHandleCreated && !IsDisposed)
			{
				BeginInvoke(new MethodInvoker(HookStatusColumn));
			}
		}

		void HookStatusColumn()
		{
			var statusColumn = OpportunitiesGrid.InnerGrid.Columns[OrgOpportunitySchema.P8_Status.Name];
			if (statusColumn != null)
			{
				((ZDropEditColumnStyle)statusColumn.ColumnStyle).EditControl.TextChanged += PromptUserToReverseAllAgreementsIfChangingOpportunityToNonEffective;
			}
		}

		public void PromptUserToReverseAllAgreementsIfChangingOpportunityToNonEffective(object sender, EventArgs e)
		{
			var control = (Control)sender;
			var newStatus = control.Text;

			if (SelectedOpportunity is null)
			{
				return;
			}

			var previousStatus = (ZString)SelectedOpportunity.P8_StatusInfo.Value;

			if (!SelectedOpportunity.ShouldPromptUserToReverseAllAgreementsIfChangingOpportunityToNonEffective(newStatus, previousStatus))
			{
				return;
			}

			if (!SelectedOpportunity.PromptUserToReverseAllAgreementsIfChangingOpportunityToNonEffective(newStatus))
			{
				control.Text = (ZString)SelectedOpportunity.P8_StatusInfo.Value;
			}
		}

		public static OpportunityManagementControl New()
		{
			var overridden = OverridableNewDelegate.Value;
			return overridden != null ? overridden() : new OpportunityManagementControl();
		}

		protected delegate OpportunityManagementControl NewDelegate();

		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		#region Disposing

		protected override void Dispose(bool isNotFinalizing)
		{
			OpportunitiesGrid.EditFormClosed -= RefreshAssignedOrgPK;
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion

		#region Filtering

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (CurrentDataItem != null)
			{
				CurrentDataItem.FilterValidationError -= OpportunityFilterValidationMessage.Show;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				CurrentDataItem.FilterValidationError -= OpportunityFilterValidationMessage.Show;
				CurrentDataItem.FilterValidationError += OpportunityFilterValidationMessage.Show;
			}
		}

		void FindButton_Click(object sender, EventArgs e)
		{
			if (CurrentDataItem != null)
			{
				CurrentDataItem.LoadOpportunitiesWithFiltering();
			}
		}

		void ClearButton_Click(object sender, EventArgs e)
		{
			if (CurrentDataItem != null)
			{
				CurrentDataItem.ClearOpportunitiesFilterValues();
			}
		}

		#endregion

		#region Selected Opportunity

		OrgOpportunity SelectedOpportunity
		{
			get
			{
				return (OrgOpportunity)OpportunitiesGrid.InnerGrid.ListManager.GetCurrent();
			}
		}

		#endregion

		#region Grid Setup

		void SetExtraCategoryColumnCaption()
		{
			foreach (var columnStyle in OpportunitiesGrid.ColumnStyles.OfType<ZGridColumnInfo>())
			{
				var captionChanged = false;

				if (columnStyle.ColumnName == OrgOpportunitySchema.P8_PackageType.Name)
				{
					columnStyle.Caption = OrganisationsDataRegistry.Instance.ProductTypeLabel.Value;
					captionChanged = true;
				}
				else if (columnStyle.ColumnName == OrgOpportunitySchema.P8_DiscountAmount.Name)
				{
					columnStyle.Caption = OrganisationsDataRegistry.Instance.CurrentLabel.Value;
					captionChanged = true;
				}
				else if (columnStyle.ColumnName == OrgOpportunitySchema.P8_RentalMultiplier.Name)
				{
					columnStyle.Caption = OrganisationsDataRegistry.Instance.PotentialLabel.Value;
					captionChanged = true;
				}

				if (captionChanged)
				{
#if DEBUG
					TypeDescriptor.AddAttributes(columnStyle, new SuppressFormsLocalizedTestAttribute());
#endif
				}
			}
		}

		#endregion

		#region Navigate to WorkflowItem

		public void NavigateToWorkflowItem(ProcessTask workflowItem)
		{
			if (OpportunitiesGrid != null && OpportunitiesGrid.InnerGrid.ListManager != null)
			{
				for (int i = 0; i < OpportunitiesGrid.InnerGrid.ListManager.Count; i++)
				{
					OrgOpportunity opp = (OrgOpportunity)OpportunitiesGrid.InnerGrid.List[i];
					if (workflowItem.Parent != null && opp.PK == ((BusinessObject)workflowItem.Parent).PK)
					{
						OpportunitiesGrid.InnerGrid.ListManager.Position = i;
						OpportunitiesGrid.InnerGrid.Select(i);

						for (int j = 0; j < OppTasksControl.TasksGrid.ListManager.Count; j++)
						{
							ProcessTask currentTask = (ProcessTask)OppTasksControl.TasksGrid.List[j];
							if (workflowItem.PK == currentTask.PK)
							{
								OppTasksControl.TasksGrid.ListManager.Position = j;
								OppTasksControl.TasksGrid.Select(j);
								break;
							}
						}

						break;
					}
				}
			}
		}

		public void NavigateToWorkflowItem(IProcessHeader workflow)
		{
			// There is only a task grid, so if this is ever called, just pick the first task in the workflow.
			var task = workflow.Tasks.OfType<ProcessTask>().FirstOrDefault();

			if (task != null)
			{
				NavigateToWorkflowItem(task);
			}
		}

		#endregion

		#region ReadOnly

		public void SetControlReadOnly(bool shouldBeReadOnly)
		{
			OppTasksControl.SetControlReadOnly(shouldBeReadOnly);
		}

		#endregion

		#region Implementation

		public new OrgHeader CurrentDataItem
		{
			get { return (OrgHeader)base.CurrentDataItem; }
		}

		#endregion
	}
}
