using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.Module
{
	public partial class ReconFilterStripControl : ZFilterStripControl<MasterFiles.Module.WorkflowFilterStrip>
	{
		public ReconFilterStripControl()
		{
			InitializeComponent();
			AddAdditionalFields();
		}

		public ReconFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
			AddAdditionalFields();
		}

		protected override ZFilterStrip NewZFilterStrip() => new ReconFilterStrip();

		protected void ReOrderGridColumns(ZGrid grid)
		{
			var newColumnOrder = GetNewColumnOrderForGrid(grid);

			if (newColumnOrder != null && newColumnOrder.Count > 0)
			{
				grid.ColumnStyles.Clear();
				grid.ColumnStyles.AddRange(newColumnOrder);
			}
		}

		protected ZString[] ColumnNamesInSortOrder
		{
			get
			{
				if (columnNamesInSortOrder == null)
				{
					var columns = new ArrayList();
					columns.Add((ZString)JobDeclaration.Schema.JE_GB);
					var columnStyles = FilteredGrid.ColumnStyles;
					foreach (ZGridColumnInfo column in columnStyles)
					{
						if (column.ColumnName != JobDeclaration.Schema.JE_GB)
						{
							columns.Add((ZString)column.ColumnName);
						}
					}
					columnNamesInSortOrder = (ZString[])columns.ToArray(typeof(ZString));
				}

				return columnNamesInSortOrder;
			}
		}
		ZString[] columnNamesInSortOrder;

		void AddAdditionalFields()
		{
			var lastMilestoneEventTextBox = new ZTextBoxColumnStyleInfo();
			lastMilestoneEventTextBox.Caption = "Last Milestone Event";
			lastMilestoneEventTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			lastMilestoneEventTextBox.ColumnName = "WorkflowItems+Milestones+LastMilestone+P9_SE_NKMilestoneEvent";
			ControlDpiScalingHelper.SetWidth(ref lastMilestoneEventTextBox, 120, true);
			lastMilestoneEventTextBox.IsVisible = false;
			FilteredGrid.ColumnStyles.Add(lastMilestoneEventTextBox);

			var lastMilestoneDescTextBox = new ZTextBoxColumnStyleInfo();
			lastMilestoneDescTextBox.Caption = "Last Milestone Desc.";
			lastMilestoneDescTextBox.ColumnName = "WorkflowItems+Milestones+LastMilestone+P9_Description";
			ControlDpiScalingHelper.SetWidth(ref lastMilestoneDescTextBox, 120, true);
			lastMilestoneDescTextBox.IsVisible = false;
			FilteredGrid.ColumnStyles.Add(lastMilestoneDescTextBox);

			var lastMilestoneATDDateEdit = new ZDateTimeOffsetEditColumnStyleInfo();
			lastMilestoneATDDateEdit.Caption = "Last Milestone ATD";
			lastMilestoneATDDateEdit.ColumnName = "WorkflowItems+Milestones+LastMilestone+P9_ActualDateForBinding";
			ControlDpiScalingHelper.SetWidth(ref lastMilestoneATDDateEdit, 120, true);
			lastMilestoneATDDateEdit.IsVisible = false;
			FilteredGrid.ColumnStyles.Add(lastMilestoneATDDateEdit);

			var nextMilestoneEventTextBox = new ZTextBoxColumnStyleInfo();
			nextMilestoneEventTextBox.Caption = "Next Milestone Event";
			nextMilestoneEventTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			nextMilestoneEventTextBox.ColumnName = "WorkflowItems+Milestones+NextMilestone+P9_SE_NKMilestoneEvent";
			ControlDpiScalingHelper.SetWidth(ref nextMilestoneEventTextBox, 120, true);
			nextMilestoneEventTextBox.IsVisible = false;
			FilteredGrid.ColumnStyles.Add(nextMilestoneEventTextBox);

			var nextMilestoneDescTextBox = new ZTextBoxColumnStyleInfo();
			nextMilestoneDescTextBox.Caption = "Next Milestone Desc.";
			nextMilestoneDescTextBox.ColumnName = "WorkflowItems+Milestones+NextMilestone+P9_Description";
			ControlDpiScalingHelper.SetWidth(ref nextMilestoneDescTextBox, 120, true);
			nextMilestoneDescTextBox.IsVisible = false;
			FilteredGrid.ColumnStyles.Add(nextMilestoneDescTextBox);

			var nextMilestoneETDDateEdit = new ZDateTimeOffsetEditColumnStyleInfo();
			nextMilestoneETDDateEdit.Caption = "Next Milestone ETD";
			nextMilestoneETDDateEdit.ColumnName = "WorkflowItems+Milestones+NextMilestone+P9_ScheduledDateForBinding";
			ControlDpiScalingHelper.SetWidth(ref nextMilestoneETDDateEdit, 120, true);
			nextMilestoneETDDateEdit.IsVisible = false;
			FilteredGrid.ColumnStyles.Add(nextMilestoneETDDateEdit);

			var isAggregateIndicator = new ZTextBoxColumnStyleInfo();
			isAggregateIndicator.Caption = "Is Aggregate";
			isAggregateIndicator.IsVisible = true;
			isAggregateIndicator.ColumnName = "US_IsAggregate";
			isAggregateIndicator.ToolTip = "Is Aggregate";
			FilteredGrid.ColumnStyles.Add(isAggregateIndicator);

			var branchGuidFindBoxColumn = new ZGuidFindBoxColumnStyleInfo();
			branchGuidFindBoxColumn.BindToList = "Lookups.BranchList";
			branchGuidFindBoxColumn.Caption = "Branch";
			branchGuidFindBoxColumn.ColumnName = "JE_GB";
			branchGuidFindBoxColumn.IsVisible = true;
			branchGuidFindBoxColumn.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbBranch;
			FilteredGrid.ColumnStyles.Add(branchGuidFindBoxColumn);

			var anticipatedLiquidationDateEdit = new ZDateEditColumnStyleInfo();
			anticipatedLiquidationDateEdit.Caption = "Anticip. Liquidation Date";
			anticipatedLiquidationDateEdit.IsVisible = false;
			anticipatedLiquidationDateEdit.ColumnName = ReconDeclaration.Schema.US_AnticipatedLiquidationDate;
			anticipatedLiquidationDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			ControlDpiScalingHelper.SetWidth(ref anticipatedLiquidationDateEdit, 140, true);
			anticipatedLiquidationDateEdit.ToolTip = "Anticipated Liquidation Date";
			FilteredGrid.ColumnStyles.Add(anticipatedLiquidationDateEdit);

			var liquidationDateEdit = new ZDateEditColumnStyleInfo();
			liquidationDateEdit.Caption = "Liquidation Date";
			liquidationDateEdit.IsVisible = false;
			liquidationDateEdit.ColumnName = ReconDeclaration.Schema.US_LiquidationDate;
			liquidationDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			ControlDpiScalingHelper.SetWidth(ref liquidationDateEdit, 140, true);
			liquidationDateEdit.ToolTip = "Liquidation Date";
			FilteredGrid.ColumnStyles.Add(liquidationDateEdit);

			ReOrderGridColumns(FilteredGrid);
		}

		ArrayList GetNewColumnOrderForGrid(ZGrid grid)
		{
			var columnNames = ColumnNamesInSortOrder;
			var result = new ArrayList(columnNames.Length);

			foreach (string columnName in columnNames)
			{
				var column = grid.GetColumnStyle(columnName);
				result.Add(column);
			}

			return result;
		}
	}
}

