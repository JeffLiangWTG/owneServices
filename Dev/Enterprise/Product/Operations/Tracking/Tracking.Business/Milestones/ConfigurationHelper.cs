using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Web;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Business
{
	public static class ConfigurationHelper
	{
		public static IEnumerable<ZTemplateColumn> GetMilestonesColumns(string bindToPrefix)
		{
			if (WebDataRegistry.Instance.MilestoneVisibility.Value != MilestoneVisibilityList.Codes.None)
			{
				bindToPrefix = bindToPrefix.Replace('.', '+');
				if (bindToPrefix.Length > 1 && bindToPrefix[bindToPrefix.Length - 1] != '+')
				{
					bindToPrefix = bindToPrefix + "+";
				}

				string bindToLast = bindToPrefix + TrackingMilestoneCollection.Schema.LastMilestone + '+';

				yield return new ZTextEditColumn(ColumnHeaders.LastMilestoneDesc, bindToLast + TrackingMilestone.Schema.Description) { ColumnKey = WebTracker.Grids.Milestones.LastMilestoneDescription };

				if (WebDataRegistry.Instance.MilestoneDatesVisibility.Value != MilestoneDatesVisibilityList.Codes.None)
				{
					yield return new ZDateTimeColumn(ColumnHeaders.LastMilestoneDate, bindToLast + TrackingMilestone.Schema.DisplayDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.Milestones.LastMilestoneDate };
				}

				if (WebDataRegistry.Instance.MilestoneVisibility.Value == MilestoneVisibilityList.Codes.All)
				{
					string bindToNext = bindToPrefix + TrackingMilestoneCollection.Schema.NextMilestone + '+';

					yield return new ZTextEditColumn(ColumnHeaders.NextMilestoneDesc, bindToNext + TrackingMilestone.Schema.Description) { ColumnKey = WebTracker.Grids.Milestones.NextMilestoneDescription };

					if (WebDataRegistry.Instance.MilestoneDatesVisibility.Value != MilestoneDatesVisibilityList.Codes.None)
					{
						yield return new ZDateTimeColumn(ColumnHeaders.NextMilestoneDate, bindToNext + TrackingMilestone.Schema.EstimatedDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.Milestones.NextMilestoneDate };
					}
				}
			}
			ZBindToChecker.CheckBindTo(((TrackingMilestoneCollection)null).LastMilestone);
			ZBindToChecker.CheckBindTo(((TrackingMilestoneCollection)null).NextMilestone);
		}

		public static void ConfigureMilestonesGrid(ZDataGrid grid)
		{
			ConfigureMilestonesGrid(grid, null, null);
		}

		public static void ConfigureMilestonesGrid(ZDataGrid grid, Control control)
		{
			ConfigureMilestonesGrid(grid, control, null);
		}

		public static void ConfigureMilestonesGrid(ZDataGrid grid, Control control, Label label)
		{
			ZCollapsablePanel panel = control as ZCollapsablePanel;
			if (WebDataRegistry.Instance.MilestoneVisibility.Value == MilestoneVisibilityList.Codes.None)
			{
				if (control != null)
				{
					control.Visible = false;
				}
				else
				{
					grid.Visible = false;
				}
			}
			else
			{
				ZString oldLabel = grid.Caption;
				if (panel != null || label != null)
				{
					oldLabel = panel == null ? label.Text : panel.Label;
				}
				if (WebDataRegistry.Instance.MilestoneVisibility.Value == MilestoneVisibilityList.Codes.LastCompletedMilestoneOnly)
				{
					ZString newLabel = Res.GetString("e2ff9ec9-ac18-4fba-9390-b1b990cb1175", "{0} (Last completed only)", oldLabel);
					if (panel != null)
					{ panel.Label = newLabel; }
					else
					{
						if (label == null)
						{ grid.Caption = newLabel; }
						else
						{ label.Text = newLabel; }
					}
				}

				if (WebDataRegistry.Instance.MilestoneVisibility.Value == MilestoneVisibilityList.Codes.CompletedMilestonesOnly)
				{
					ZString newLabel = Res.GetString("17a4d204-87a1-4501-951f-26757ba8c462", "{0} (Completed only)", oldLabel);
					if (panel != null)
					{ panel.Label = newLabel; }
					else
					{
						if (label == null)
						{ grid.Caption = newLabel; }
						else
						{ label.Text = newLabel; }
					}
				}

				foreach (var col in GetMilestonesColumns())
				{
					if (grid is ZGrid)
					{
						((ZGrid)grid).ColumnProvider.AddToDictionaryAsDefault(col as IUniqueKeyColumn);
					}
					else
					{
						grid.Columns.Add(col);
					}
				}
				if (grid is ZGrid)
				{
					((ZGrid)grid).RepopulateColumns();
				}
			}
			ZBindToChecker.CheckBindTo(((TrackingMilestone)null).Description);
			ZBindToChecker.CheckBindTo(((TrackingMilestone)null).ActualDate);
			ZBindToChecker.CheckBindTo(((TrackingMilestone)null).EstimatedDate);
			ZBindToChecker.CheckBindTo(((TrackingMilestone)null).DisplayDate);
			ZBindToChecker.CheckBindTo(((TrackingMilestone)null).Status);
			ZBindToChecker.CheckBindTo(((TrackingMilestone)null).ParentCode);
		}

		public static DataGridColumn[] GetMilestonesColumns()
		{
			var gridColumns = new List<DataGridColumn>();

			gridColumns.Add(new ZTextEditColumn(ColumnHeaders.ParentCode, TrackingMilestone.Schema.ParentCode) { ColumnKey = WebTracker.Grids.Milestones.ParentCode });
			gridColumns.Add(new ZTextEditColumn(ColumnHeaders.Description, TrackingMilestone.Schema.Description) { ColumnKey = WebTracker.Grids.Milestones.Description });

			switch (WebDataRegistry.Instance.MilestoneDatesVisibility.Value)
			{
				case MilestoneDatesVisibilityList.Codes.All:
					gridColumns.Add(new ZTimelineColumn(ColumnHeaders.Date, TrackingMilestone.Schema.ActualDateWithSuppression, TrackingMilestone.Schema.EstimatedDateWithSuppression, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.Milestones.Date });
					break;
				case MilestoneDatesVisibilityList.Codes.ShowActualDateWithFallbackToEstimated:
					gridColumns.Add(new ZDateTimeColumn(ColumnHeaders.Date, TrackingMilestone.Schema.DisplayDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.Milestones.Date });
					break;
			}

			if (WebDataRegistry.Instance.MilestoneStatusVisibility.Value != MilestoneStatusVisibilityList.Codes.None)
			{
				gridColumns.Add(new ZTextEditColumn(ColumnHeaders.Status, TrackingMilestone.Schema.Status) { ColumnKey = WebTracker.Grids.Milestones.Status });
			}
			return gridColumns.ToArray();
		}

		public static class ColumnHeaders
		{
			public static string Description { get { return Res.GetString("c4adc9dc-708f-425b-ae04-18de7a91eeec", "Description"); } }
			public static string Date { get { return Res.GetString("0a5b160d-3e3a-4323-a6f8-7ba7832221ac", "Date"); } }
			public static string Status { get { return Res.GetString("6c5a2a9f-74ab-485e-b4af-874cd2e9907e", "Status"); } }
			public static string LastMilestoneDesc { get { return Res.GetString("3caa3bc6-0cb3-41b5-ab4f-505c34696ceb", "Last Milestone Desc."); } }
			public static string LastMilestoneDate { get { return Res.GetString("4f720117-9a8f-401d-806d-574e9bf47a14", "Last Milestone Date"); } }
			public static string NextMilestoneDesc { get { return Res.GetString("7f5347db-5f31-4d9c-92b1-59e677240aaa", "Next Milestone Desc."); } }
			public static string NextMilestoneDate { get { return Res.GetString("1c15842b-8c09-4a92-a0d7-350032cfc4d6", "Next Milestone Date"); } }
			public static string ParentCode { get { return Res.GetString("423c3b1c-bfb5-4c1c-b031-27d2fd5ee4e2", "Parent Job"); } }
		}

		public static void ConfigureLegendLabels(ZTextLabel pendingLegendLabel, ZTextLabel overdueLegendLabel, ZTextLabel completedLegendLabel, ZTextLabel completedLateLegendLabel)
		{
			pendingLegendLabel.CssClass = TimelineHelper.TimeLineLegendClass + " " + TimelineHelper.PendingClass;
			pendingLegendLabel.ToolTip = Res.GetString("b56d9f24-e3c0-4f31-a433-06ade6af06b1", "Estimated date has not passed yet");

			overdueLegendLabel.CssClass = TimelineHelper.TimeLineLegendClass + " " + TimelineHelper.OverdueClass;
			overdueLegendLabel.ToolTip = Res.GetString("13bc8934-fdf7-4035-874b-24a55e43b5ee", "Estimated date has passed and an actual date has yet to be entered");

			completedLegendLabel.CssClass = TimelineHelper.TimeLineLegendClass + " " + TimelineHelper.CompletedClass;
			completedLegendLabel.ToolTip = Res.GetString("ce1e28a5-c155-42aa-827e-c11686a2c20b", "Actual date is equal to or less than estimated date");

			completedLateLegendLabel.CssClass = TimelineHelper.TimeLineLegendClass + " " + TimelineHelper.CompletedLateClass;
			completedLateLegendLabel.ToolTip = Res.GetString("fd1e235b-87b0-4def-883e-4ffb2e23c819", "Actual date is greater than estimated date");

			switch (WebDataRegistry.Instance.MilestoneStatusVisibility.Value)
			{
				case MilestoneStatusVisibilityList.Codes.All:
					pendingLegendLabel.Visible = true;
					overdueLegendLabel.Visible = true;
					completedLegendLabel.Visible = true;
					completedLateLegendLabel.Visible = true;
					break;
				case MilestoneStatusVisibilityList.Codes.CompletedOnly:
					pendingLegendLabel.Visible = false;
					overdueLegendLabel.Visible = false;
					completedLegendLabel.Visible = true;
					completedLegendLabel.ToolTip = Res.GetString("294e03a5-1782-4462-ac20-7144465ddf97", "Actual date has been entered");
					completedLateLegendLabel.Visible = false;
					break;
				case MilestoneStatusVisibilityList.Codes.CompletedAndPendingOnly:
					pendingLegendLabel.Visible = true;
					pendingLegendLabel.ToolTip = Res.GetString("7e601a67-74ba-4b89-984d-9a1eaac9cd0e", "Actual date has yet to be entered");
					overdueLegendLabel.Visible = false;
					completedLegendLabel.Visible = true;
					completedLegendLabel.ToolTip = Res.GetString("294e03a5-1782-4462-ac20-7144465ddf97", "Actual date has been entered");
					completedLateLegendLabel.Visible = false;
					break;
				case MilestoneStatusVisibilityList.Codes.None:
					pendingLegendLabel.Visible = false;
					overdueLegendLabel.Visible = false;
					completedLegendLabel.Visible = false;
					completedLateLegendLabel.Visible = false;
					break;
			}
		}
	}
}
