using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Web;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Business.Testing
{
	public class MilestoneGridConfrigurationHelperTest : TestCaseWithFactory
	{
		public void TestGetMilestonesColumns()
		{
			Assert("Precondition: default registry values",
					 WebDataRegistry.Instance.MilestoneVisibility.Value == MilestoneVisibilityList.Codes.All &&
					 WebDataRegistry.Instance.MilestoneDatesVisibility.Value == MilestoneDatesVisibilityList.Codes.All);

			IEnumerator<ZTemplateColumn> enumerator = ConfigurationHelper.GetMilestonesColumns("dummy.mock.").GetEnumerator();
			enumerator.MoveNext();
			AssertColumnDetails(typeof(ZTextEditColumn), enumerator, "dummy+mock+LastMilestone+Description", "Last Milestone Desc.");
			AssertColumnDetails(typeof(ZDateTimeColumn), enumerator, "dummy+mock+LastMilestone+DisplayDate", "Last Milestone Date");
			AssertColumnDetails(typeof(ZTextEditColumn), enumerator, "dummy+mock+NextMilestone+Description", "Next Milestone Desc.");
			Assert("Should be 4 columns", !enumerator.MoveNext());
			AssertColumnDetails(typeof(ZDateTimeColumn), enumerator, "dummy+mock+NextMilestone+EstimatedDate", "Next Milestone Date");

			WebDataRegistry.Instance.MilestoneDatesVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneDatesVisibilityList.Codes.ShowActualDateWithFallbackToEstimated);
			enumerator = ConfigurationHelper.GetMilestonesColumns("dummy.").GetEnumerator();
			enumerator.MoveNext();
			AssertColumnDetails(typeof(ZTextEditColumn), enumerator, "dummy+LastMilestone+Description", "Last Milestone Desc.");
			AssertColumnDetails(typeof(ZDateTimeColumn), enumerator, "dummy+LastMilestone+DisplayDate", "Last Milestone Date");
			AssertColumnDetails(typeof(ZTextEditColumn), enumerator, "dummy+NextMilestone+Description", "Next Milestone Desc.");
			Assert("Should be 4 columns", !enumerator.MoveNext());
			AssertColumnDetails(typeof(ZDateTimeColumn), enumerator, "dummy+NextMilestone+EstimatedDate", "Next Milestone Date");

			WebDataRegistry.Instance.MilestoneDatesVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneDatesVisibilityList.Codes.None);
			enumerator = ConfigurationHelper.GetMilestonesColumns("dummy").GetEnumerator();
			enumerator.MoveNext();
			AssertColumnDetails(typeof(ZTextEditColumn), enumerator, "dummy+LastMilestone+Description", "Last Milestone Desc.");
			Assert("Should be 2 columns", !enumerator.MoveNext());
			AssertColumnDetails(typeof(ZTextEditColumn), enumerator, "dummy+NextMilestone+Description", "Next Milestone Desc.");

			WebDataRegistry.Instance.MilestoneDatesVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneDatesVisibilityList.Codes.All);
			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.CompletedMilestonesOnly);
			enumerator = ConfigurationHelper.GetMilestonesColumns("dummy").GetEnumerator();
			enumerator.MoveNext();
			AssertColumnDetails(typeof(ZTextEditColumn), enumerator, "dummy+LastMilestone+Description", "Last Milestone Desc.");
			Assert("Should be 2 columns", !enumerator.MoveNext());
			AssertColumnDetails(typeof(ZDateTimeColumn), enumerator, "dummy+LastMilestone+DisplayDate", "Last Milestone Date");

			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.None);
			enumerator = ConfigurationHelper.GetMilestonesColumns("dummy").GetEnumerator();
			Assert("Should be zero columns", !enumerator.MoveNext());
		}

		public void TestConfigureMilestonesGrid()
		{
			Assert("Precondition: default registry values",
					 WebDataRegistry.Instance.MilestoneVisibility.Value == MilestoneVisibilityList.Codes.All &&
					 WebDataRegistry.Instance.MilestoneDatesVisibility.Value == MilestoneDatesVisibilityList.Codes.All &&
					 WebDataRegistry.Instance.MilestoneStatusVisibility.Value == MilestoneStatusVisibilityList.Codes.All);

			ZDataGrid testGrid = new ZDataGrid();
			ZCollapsablePanel testPanel = new ZCollapsablePanel();
			ConfigurationHelper.ConfigureMilestonesGrid(testGrid, testPanel);
			AssertEquals(4, testGrid.Columns.Count);
			Assert(testPanel.Visible);
			AssertColumnDetails(typeof(ZTextEditColumn), (ZTemplateColumn)testGrid.Columns[0], "ParentCode", "Parent Job");
			AssertColumnDetails(typeof(ZTextEditColumn), (ZTemplateColumn)testGrid.Columns[1], "Description", "Description");
			AssertColumnDetails(typeof(ZTimelineColumn), (ZTemplateColumn)testGrid.Columns[2], "ActualDateWithSuppression", "EstimatedDateWithSuppression", "Date");
			AssertColumnDetails(typeof(ZTextEditColumn), (ZTemplateColumn)testGrid.Columns[3], "Status", "Status");

			WebDataRegistry.Instance.MilestoneDatesVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneDatesVisibilityList.Codes.ShowActualDateWithFallbackToEstimated);
			testGrid = new ZDataGrid();
			testPanel = new ZCollapsablePanel();
			ConfigurationHelper.ConfigureMilestonesGrid(testGrid, testPanel);
			AssertEquals(4, testGrid.Columns.Count);
			Assert(testPanel.Visible);
			AssertColumnDetails(typeof(ZTextEditColumn), (ZTemplateColumn)testGrid.Columns[0], "ParentCode", "Parent Job");
			AssertColumnDetails(typeof(ZTextEditColumn), (ZTemplateColumn)testGrid.Columns[1], "Description", "Description");
			AssertColumnDetails(typeof(ZDateTimeColumn), (ZTemplateColumn)testGrid.Columns[2], "DisplayDate", "Date");
			AssertColumnDetails(typeof(ZTextEditColumn), (ZTemplateColumn)testGrid.Columns[3], "Status", "Status");

			WebDataRegistry.Instance.MilestoneDatesVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneDatesVisibilityList.Codes.None);
			testGrid = new ZDataGrid();
			testPanel = new ZCollapsablePanel();
			ConfigurationHelper.ConfigureMilestonesGrid(testGrid, testPanel);
			AssertEquals(3, testGrid.Columns.Count);
			Assert(testPanel.Visible);
			AssertColumnDetails(typeof(ZTextEditColumn), (ZTemplateColumn)testGrid.Columns[0], "ParentCode", "Parent Job");
			AssertColumnDetails(typeof(ZTextEditColumn), (ZTemplateColumn)testGrid.Columns[1], "Description", "Description");
			AssertColumnDetails(typeof(ZTextEditColumn), (ZTemplateColumn)testGrid.Columns[2], "Status", "Status");

			WebDataRegistry.Instance.MilestoneStatusVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneStatusVisibilityList.Codes.None);
			testGrid = new ZDataGrid();
			testPanel = new ZCollapsablePanel();
			ConfigurationHelper.ConfigureMilestonesGrid(testGrid, testPanel);
			AssertEquals(2, testGrid.Columns.Count);
			Assert(testPanel.Visible);
			AssertColumnDetails(typeof(ZTextEditColumn), (ZTemplateColumn)testGrid.Columns[0], "ParentCode", "Parent Job");
			AssertColumnDetails(typeof(ZTextEditColumn), (ZTemplateColumn)testGrid.Columns[1], "Description", "Description");

			WebDataRegistry.Instance.MilestoneVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneVisibilityList.Codes.None);
			testPanel = new ZCollapsablePanel();
			ConfigurationHelper.ConfigureMilestonesGrid(testGrid, testPanel);
			Assert(!testPanel.Visible);
		}

		public void TestConfigureLegendLabels()
		{
			ZTextLabel pendingLegendLabel = new ZTextLabel();
			ZTextLabel overdueLegendLabel = new ZTextLabel();
			ZTextLabel completedLegendLabel = new ZTextLabel();
			ZTextLabel completedLateLegendLabel = new ZTextLabel();

			AssertEquals("Precondition: default registry value", MilestoneStatusVisibilityList.Codes.All, WebDataRegistry.Instance.MilestoneStatusVisibility.Value);
			ConfigurationHelper.ConfigureLegendLabels(pendingLegendLabel, overdueLegendLabel, completedLegendLabel, completedLateLegendLabel);
			AssertLabeDetails(pendingLegendLabel, true, "Estimated date has not passed yet", TimelineHelper.TimeLineLegendClass + " " + TimelineHelper.PendingClass);
			AssertLabeDetails(overdueLegendLabel, true, "Estimated date has passed and an actual date has yet to be entered", TimelineHelper.TimeLineLegendClass + " " + TimelineHelper.OverdueClass);
			AssertLabeDetails(completedLegendLabel, true, "Actual date is equal to or less than estimated date", TimelineHelper.TimeLineLegendClass + " " + TimelineHelper.CompletedClass);
			AssertLabeDetails(completedLateLegendLabel, true, "Actual date is greater than estimated date", TimelineHelper.TimeLineLegendClass + " " + TimelineHelper.CompletedLateClass);

			WebDataRegistry.Instance.MilestoneStatusVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneStatusVisibilityList.Codes.CompletedOnly);
			ConfigurationHelper.ConfigureLegendLabels(pendingLegendLabel, overdueLegendLabel, completedLegendLabel, completedLateLegendLabel);
			AssertEquals(false, pendingLegendLabel.Visible);
			AssertEquals(false, overdueLegendLabel.Visible);
			AssertLabeDetails(completedLegendLabel, true, "Actual date has been entered", TimelineHelper.TimeLineLegendClass + " " + TimelineHelper.CompletedClass);
			AssertEquals(false, completedLateLegendLabel.Visible);

			WebDataRegistry.Instance.MilestoneStatusVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneStatusVisibilityList.Codes.CompletedAndPendingOnly);
			ConfigurationHelper.ConfigureLegendLabels(pendingLegendLabel, overdueLegendLabel, completedLegendLabel, completedLateLegendLabel);
			AssertLabeDetails(pendingLegendLabel, true, "Actual date has yet to be entered", TimelineHelper.TimeLineLegendClass + " " + TimelineHelper.PendingClass);
			AssertEquals(false, overdueLegendLabel.Visible);
			AssertLabeDetails(completedLegendLabel, true, "Actual date has been entered", TimelineHelper.TimeLineLegendClass + " " + TimelineHelper.CompletedClass);
			AssertEquals(false, completedLateLegendLabel.Visible);

			WebDataRegistry.Instance.MilestoneStatusVisibility.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MilestoneStatusVisibilityList.Codes.None);
			ConfigurationHelper.ConfigureLegendLabels(pendingLegendLabel, overdueLegendLabel, completedLegendLabel, completedLateLegendLabel);
			AssertEquals(false, pendingLegendLabel.Visible);
			AssertEquals(false, overdueLegendLabel.Visible);
			AssertEquals(false, completedLegendLabel.Visible);
			AssertEquals(false, completedLateLegendLabel.Visible);
		}

		#region Implementation

		void AssertLabeDetails(ZTextLabel label, bool visible, string toolTip, string css)
		{
			AssertEquals(visible, label.Visible);
			AssertEquals(toolTip, label.ToolTip);
			AssertEquals(css, label.CssClass);
		}

		void AssertColumnDetails(Type columnType, IEnumerator<ZTemplateColumn> enumerator, string bindTo, string headerText)
		{
			AssertColumnDetails(columnType, enumerator.Current, bindTo, headerText);
			enumerator.MoveNext();
		}

		void AssertColumnDetails(Type columnType, ZTemplateColumn column, string bindToActual, string bindToEstimated, string headerText)
		{
			Assert(column is ZTimelineColumn);
			AssertEquals(bindToEstimated, ((ZTimelineColumn)column).BindToEstimated);
			AssertColumnDetails(columnType, column, bindToActual, headerText);
		}

		void AssertColumnDetails(Type columnType, ZTemplateColumn column, string bindTo, string headerText)
		{
			AssertEquals(columnType, column.GetType());
			AssertEquals(bindTo, column.BindTo);
			AssertEquals(headerText, column.HeaderText);
		}

		#endregion
	}
}
