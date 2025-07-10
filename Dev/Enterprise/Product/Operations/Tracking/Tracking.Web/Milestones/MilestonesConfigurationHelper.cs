using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Web;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	public static class MilestonesConfigurationHelper
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

				var bindToLast = bindToPrefix + TrackingMilestoneCollection.Schema.LastMilestone + '+';

				yield return new ZTextEditColumn(ConfigurationHelper.ColumnHeaders.LastMilestoneDesc, bindToLast + TrackingMilestone.Schema.Description);

				if (WebDataRegistry.Instance.MilestoneDatesVisibility.Value != MilestoneDatesVisibilityList.Codes.None)
				{
					yield return new ZDateTimeColumn(ConfigurationHelper.ColumnHeaders.LastMilestoneDate, bindToLast + TrackingMilestone.Schema.DisplayDate, ZDateTimePickerFormat.Long);
				}

				if (WebDataRegistry.Instance.MilestoneVisibility.Value == MilestoneVisibilityList.Codes.All)
				{
					var bindToNext = bindToPrefix + TrackingMilestoneCollection.Schema.NextMilestone + '+';

					yield return new ZTextEditColumn(ConfigurationHelper.ColumnHeaders.NextMilestoneDesc, bindToNext + TrackingMilestone.Schema.Description);

					if (WebDataRegistry.Instance.MilestoneDatesVisibility.Value != MilestoneDatesVisibilityList.Codes.None)
					{
						yield return new ZDateTimeColumn(ConfigurationHelper.ColumnHeaders.NextMilestoneDate, bindToNext + TrackingMilestone.Schema.EstimatedDate, ZDateTimePickerFormat.Long);
					}
				}
			}

#pragma warning disable IDE0004 // Remove Unnecessary Cast
			ZBindToChecker.CheckBindTo((TrackingMilestone)((TrackingMilestoneCollection)null).LastMilestone);
			ZBindToChecker.CheckBindTo((TrackingMilestone)((TrackingMilestoneCollection)null).NextMilestone);
#pragma warning restore IDE0004 // Remove Unnecessary Cast
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String comparison")]
		public static void ConfigureMilestonesGridColumnsVisibility(ZDataGrid grid, bool isInEditMode)
		{
			foreach (DataGridColumn gridColumn in grid.Columns)
			{
				ZTemplateColumn column = gridColumn as ZTemplateColumn;

				if (column != null)
				{
					if (column.ID == "Date_View" || column.ID == "Status")
					{
						column.Visible = !isInEditMode;
					}
					if (column.ID == "EDate_Edit")
					{
						column.Visible = isInEditMode;
						column.ReadOnly = SiteUser == null || !SiteUser.CanUpdateEstimatedMilestones;
					}
					if (column.ID == "ADate_Edit")
					{
						column.Visible = isInEditMode;
						column.ReadOnly = SiteUser == null || !SiteUser.CanUpdateActualMilestones;
					}
				}
			}
		}

		static TrackingSiteUser SiteUser
		{
			get { return WebEnv.AppInstance.SiteUser as TrackingSiteUser; }
		}

		public static void ConfigureMilestonesGrid(ZDataGrid grid, Control control)
		{
			ConfigureMilestonesGrid(grid, control, null);
		}

		public static void ConfigureMilestonesGrid(ZDataGrid grid, Control control, Label label)
		{
			ConfigureMilestonesGrid(grid, control, label, false);
		}

		public static void ConfigureMilestonesGrid(ZDataGrid grid, Control control, Label label, bool isInEditMode)
		{
			var panel = control as ZCollapsablePanel;
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
				var oldLabel = grid.Caption;
				if (panel != null || label != null)
				{
					oldLabel = panel == null ? label.Text : panel.Label;
				}
				if (WebDataRegistry.Instance.MilestoneVisibility.Value == MilestoneVisibilityList.Codes.LastCompletedMilestoneOnly)
				{
					var newLabel = Res.GetString("F2A33826-0D9E-4C7F-B52F-245AC6C68CE7", "{0} (Last completed only)", oldLabel);
					if (panel != null)
					{ panel.Label = newLabel; }
					else
					{
						if (label == null)
						{
							grid.Caption = newLabel;
						}
						else
						{
							label.Text = newLabel;
						}
					}
				}

				if (WebDataRegistry.Instance.MilestoneVisibility.Value == MilestoneVisibilityList.Codes.CompletedMilestonesOnly)
				{
					var newLabel = Res.GetString("C956F58F-DDEA-4C2C-8675-B7C56373E8B5", "{0} (Completed only)", oldLabel);
					if (panel != null)
					{
						panel.Label = newLabel;
					}
					else
					{
						if (label == null)
						{
							grid.Caption = newLabel;
						}
						else
						{
							label.Text = newLabel;
						}
					}
				}

				grid.Columns.Add(new ZTextEditColumn(ConfigurationHelper.ColumnHeaders.ParentCode, TrackingMilestone.Schema.ParentCode));
				grid.Columns.Add(new ZTextEditColumn(ConfigurationHelper.ColumnHeaders.Description, TrackingMilestone.Schema.Description));

				var siteUser = WebEnv.AppInstance.SiteUser as TrackingSiteUser;

				grid.Columns.Add(new MilestoneDateTimeColumn(Res.GetString("60910832-12a0-4048-8e94-77df2377d2b1", "Estimated Date"), TrackingMilestone.Schema.EstimatedDate, ZDateTimePickerFormat.Long) { ID = "EDate_Edit", ReadOnly = (siteUser == null || !siteUser.CanUpdateEstimatedMilestones) });
				grid.Columns.Add(new MilestoneDateTimeColumn(Res.GetString("6e8b2468-d527-4970-9564-f6f7a1ab7d43", "Actual Date"), TrackingMilestone.Schema.ActualDate, ZDateTimePickerFormat.Long) { ID = "ADate_Edit", ReadOnly = (siteUser == null || !siteUser.CanUpdateActualMilestones) });

				switch (WebDataRegistry.Instance.MilestoneDatesVisibility.Value)
				{
					case MilestoneDatesVisibilityList.Codes.All:
						grid.Columns.Add(new ZTimelineColumn(ConfigurationHelper.ColumnHeaders.Date, TrackingMilestone.Schema.ActualDate, TrackingMilestone.Schema.EstimatedDate, ZDateTimePickerFormat.Long) { ID = "Date_View" });
						break;
					case MilestoneDatesVisibilityList.Codes.ShowActualDateWithFallbackToEstimated:
						grid.Columns.Add(new ZDateTimeColumn(ConfigurationHelper.ColumnHeaders.Date, TrackingMilestone.Schema.DisplayDate, ZDateTimePickerFormat.Long) { ID = "Date_View" });
						break;
				}

				if (WebDataRegistry.Instance.MilestoneStatusVisibility.Value != MilestoneStatusVisibilityList.Codes.None)
				{
					grid.Columns.Add(new ZTextEditColumn(ConfigurationHelper.ColumnHeaders.Status, TrackingMilestone.Schema.Status) { ID = (NoResString)"Status" });
				}
			}

#pragma warning disable IDE0004 // Remove Unnecessary Cast Justification = "ZBindToChecker requires a redundant cast"
			ZBindToChecker.CheckBindTo((ZString)((TrackingMilestone)null).Description);
			ZBindToChecker.CheckBindTo((ZDateTimeOffset)((TrackingMilestone)null).ActualDate);
			ZBindToChecker.CheckBindTo((ZDateTimeOffset)((TrackingMilestone)null).EstimatedDate);
			ZBindToChecker.CheckBindTo((ZDateTimeOffset)((TrackingMilestone)null).DisplayDate);
			ZBindToChecker.CheckBindTo((ZString)((TrackingMilestone)null).Status);
			ZBindToChecker.CheckBindTo((ZString)((TrackingMilestone)null).ParentCode);
#pragma warning restore IDE0004 // Remove Unnecessary Cast

			ConfigureMilestonesGridColumnsVisibility(grid, isInEditMode);
		}
	}
}
