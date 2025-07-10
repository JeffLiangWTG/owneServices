using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ZMilestonesUserControl : ZMilestonesOrExceptionsUserControl, IRequiresWorkflowSecurity
	{
		public ZMilestonesUserControl()
		{
			InitializeComponent();
			SetupContextMenuOnActions();
			P9_ActualDateDateEdit.AllowOutsideOfParent();
		}

		void SetupContextMenuOnActions()
		{
			UniversalXmlSampleSavingContextMenuManager.AttachToGrid(TriggersGrid);
			WorkflowDiagnosticContextMenuManager.AttachToTriggerGrid(MilestoneGrid);
			ObjectFactory.Get<IValidateCommunicationModesMenuItemProvider>().AddItemToGrid(MilestoneGrid);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.CheckAndConsumeWorkflowLicenceIfAllowed();
		}

		#region MilestonesGrid

		protected internal class MilestonesGrid : ZGrid
		{
			protected override Image GetRowNotificationImageFromListIndex(int listIndex)
			{
				ProcessTask milestone = (ProcessTask)List[listIndex];
				INotificationType notification = milestone != null ? milestone.GetHighestSeverityNotificationType() : null;
				return milestone != null && milestone.IsMilestoneOverdueAndHasOpenException && (notification == null || !notification.IsFatal)
						? Icons.GetIcon(IconTypes.RedAlarmBell).ToBitmap()
						: base.GetRowNotificationImageFromListIndex(listIndex);
			}

			CurrencyManager currencyManager;

			public override void SetDataBinding(object dataSource, string dataMember, string tableName)
			{
				if (currencyManager != null)
				{
					currencyManager.PositionChanged -= ListManager_PositionChanged;
					currencyManager.ListChanged -= CurrencyManager_ListChanged;
					currencyManager.DataError -= CurrencyManager_DataError;
				}

				if (dataSource != null)
				{
					currencyManager = BindingContext?[dataSource, dataMember] as CurrencyManager;
					if (currencyManager != null)
					{
						AddPositionTrackInfo(currencyManager.Position, currencyManager.Count, "SetDataBinding");
						currencyManager.PositionChanged += ListManager_PositionChanged;
						currencyManager.ListChanged += CurrencyManager_ListChanged;
						currencyManager.DataError += CurrencyManager_DataError;
					}
				}

				base.SetDataBinding(dataSource, dataMember, tableName);
			}

			void CurrencyManager_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
			{
				AddPositionTrackInfo(currencyManager.Position, currencyManager.Count, $"CurrencyManager_ListChanged, PropDesc: {e.PropertyDescriptor},ListChangedType: {e.ListChangedType},NewIndex: {e.NewIndex},OldIndex: {e.OldIndex}");
				ReportErrorWhenPositionIsIncorrect();
			}

			void CurrencyManager_DataError(object sender, BindingManagerDataErrorEventArgs e)
			{
				dataError = e.Exception;
			}

			Exception dataError;
			readonly List<string> positionTrackList = new List<string>();

			void ListManager_PositionChanged(object sender, EventArgs e)
			{
				AddPositionTrackInfo(currencyManager.Position, currencyManager.Count, "ListManager_PositionChanged");
				ReportErrorWhenPositionIsIncorrect();
			}

			void AddPositionTrackInfo(int position, int count, string message = "")
			{
				positionTrackList.Add($"Position: {position}, Count: {count}, {message}");
			}

			void ReportErrorWhenPositionIsIncorrect()
			{
				if (currencyManager.Position == -1 && currencyManager.Count > 0)
				{
					StringBuilder milestoneData = new StringBuilder();
					if (List.Count > 0 && List[0] is ProcessTask milestone)
					{
						milestoneData.AppendLine($"{ProcessTask.Schema.PK} {milestone.PK}");
						milestoneData.AppendLine($"{ProcessTask.Schema.P9_ParentTableCode} {milestone.P9_ParentTableCode}");
						milestoneData.AppendLine($"{ProcessTask.Schema.P9_ParentID} {milestone.P9_ParentID}");
					}

					var positionTracks = string.Join("\r\n", positionTrackList);
					var message = FormattableString.Invariant($@"ListManager position is -1, it will cause the TriggersGrid IndexOutOfRangeException during the data binding.
Position Changes Track:
{positionTracks}

Milestone Data:
{milestoneData}

CurrenyManger Count:
{currencyManager.Count}

List Count:
{List.Count}

Set Position to -1 Stack Trace:
{new StackTrace().ToDebugString()}

Data Error: {dataError}");

					var name = dataError == null ? "null" : dataError.GetType().Name;
					ErrorReporter.ReportOnce($"ZGrid_Milestones_PositionChanged_Incorrect_{name}", message);

					positionTrackList.Clear();
				}
			}
		}

		#endregion

		#region ReferenceCode Column Caption

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (!string.IsNullOrEmpty(dataMember))
			{
				throw new ArgumentException("dataMember not supported", nameof(dataMember));
			}
			IWorkflowProvider provider = (IWorkflowProvider)dataSource;
			if (provider != null)
			{
				ZDropEditColumnStyleInfo referenceCodeColumn = (ZDropEditColumnStyleInfo)MilestoneGrid.GetColumnStyle(ProcessTask.Schema.ReferenceCode);
				if (referenceCodeColumn != null && !provider.WorkflowItems.RequiresReferenceCode)
				{
					MilestoneGrid.ColumnStyles.Remove(referenceCodeColumn);
				}
				if (referenceCodeColumn != null)
				{
					referenceCodeColumn.Caption = provider.WorkflowItems.ReferenceCodeCaption;
				}
			}

			base.SetDataBinding(dataSource, dataMember);
			IBusinessObjectCollection collection = MilestoneGrid.List as IBusinessObjectCollection;
			if (collection != null)
			{
				if (((IRequiresWorkflowSecurity)this).ViewOnly)
				{
					collection.IncrementReadOnlyIncludingChildren();
				}
				CreateMilestonesLinkLabel.Enabled = !collection.ReadOnly;
			}
		}

		#endregion

		#region Navigate to WorkflowItem

		public virtual void NavigateToWorkflowItem(ProcessTask milestone)
		{
			if (MilestoneGrid != null && MilestoneGrid.ListManager != null)
			{
				for (int i = 0; i < this.MilestoneGrid.ListManager.Count; i++)
				{
					ProcessTask currentTask = (ProcessTask)MilestoneGrid.List[i];
					if (milestone.PK == currentTask.PK)
					{
						MilestoneGrid.ListManager.Position = i;
						break;
					}
				}
			}
		}

		#endregion

		#region IRequireWorkflowSecurity Members

		string IRequiresWorkflowSecurity.WorkflowItemEditCheckpointCode
		{
			get { return SecurityCore.WorkflowMilestonesAutoGeneratedCode; }
		}

		string IRequiresWorkflowSecurity.WorkflowItemJustViewCheckpointCode
		{
			get { return SecurityCore.WorkflowMilestonesJustViewAutoGeneratedCode; }
		}

		ModuleIdentifier IRequiresWorkflowSecurity.WorkflowProviderModuleId
		{
			get { return null; }
		}

		bool IRequiresWorkflowSecurity.ViewOnly { get; set; }

		#endregion

		#region Implementation

		protected override ZGrid Grid => MilestoneGrid;

		protected override ColourLegend ColourLegend => colourLegend;

		protected override KSplitContainer SplitContainer => milestoneGridSplitContainer;

		protected override ZLabel TasksHintLabel => milestoneHintLabel;

		void CreateMilestonesLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var providerProxy = (IWorkflowProvider)MilestoneGrid.List;
			var provider = (IWorkflowProvider)providerProxy.WorkflowItems.Parent;

			if (provider.WorkflowItems.Milestones.Count > 0)
			{
				Globals.Message.Show(Res.GetString("9df6735c-398a-45c2-a8db-2fb6d68e78b9", "You can only create milestones from the template if you have no milestones already entered."), Res.GetString("33177b10-8cff-4e3f-b148-ce536441282f", "Milestone Creation"), MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			else
			{
				provider.ApplyWorkflowTemplates(TemplateApplicationParameters.ApplySpecificEntityTypes(TemplateEntityType.Milestones, jobAttributesMayHaveChangedSinceLastTemplateApplication: true));

				if (provider.WorkflowItems.Milestones.Count == 0)
				{
					Globals.Message.Show(Res.GetString("c1b1be19-f43d-4b56-a06c-b1c004f851a1", "No Milestones were added from templates for the specified details."), Res.GetString("33177b10-8cff-4e3f-b148-ce536441282f", "Milestone Creation"), MessageBoxButtons.OK, MessageBoxIcon.Information);
				}

				provider.WorkflowItems.MilestonesIncludingRelatedSortable.CollectionToFilter.Load();
				provider.WorkflowItems.MilestonesIncludingRelatedSortable.Rebuild();
			}
		}

#if DEBUG
		public LinkLabel CreateMilestonesLinkLabelForTestOnly
		{
			get { return CreateMilestonesLinkLabel; }
		}
#endif

		#endregion
	}
}
