using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ProcessManagement.Business;

namespace Enterprise.ProcessManagement.Module.Test
{
	public class WorkItemFilterControlTest : TestCaseWithFactory
	{
		public void TestCustomLabels()
		{
			ProcessManagementRegistry.Instance.WorkItemTypeLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "My Product");
			ProcessManagementRegistry.Instance.WorkItemAreaLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "My Area");
			ProcessManagementRegistry.Instance.WorkItemActivityTypeLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "My Activity");
			ProcessManagementRegistry.Instance.WorkItemActivitySubTypeLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "My Subactivity");
			ProcessManagementRegistry.Instance.WorkItemPriorityLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "My Priority");

			using (var control = new WorkItemFilterControl(null, new WorkItemFilterBusinessObject()))
			{
				ZGridColumnInfo typeColumn = null;
				ZGridColumnInfo areaColumn = null;
				ZGridColumnInfo activityColumn = null;
				ZGridColumnInfo subactivityColumn = null;
				ZGridColumnInfo priorityColumn = null;

				foreach (var columnStyle in control.Grid.ColumnStyles)
				{
					var textColumn = columnStyle as ZGridColumnInfo;
					if (textColumn != null)
					{
						switch (textColumn.ColumnName)
						{
							case AutoWorkItem.Schema.WKI_WorkItemType:
							case WorkItemCommon.Schema.WorkItemTypeDescription:
								typeColumn = textColumn;
								AssertEquals("My Product", textColumn.Caption);
								break;
							case AutoWorkItem.Schema.WKI_WorkItemArea:
							case WorkItemCommon.Schema.AreaDescription:
								areaColumn = textColumn;
								AssertEquals("My Area", textColumn.Caption);
								break;
							case AutoWorkItem.Schema.WKI_ActivityType:
							case WorkItemCommon.Schema.ActivityTypeDescription:
								activityColumn = textColumn;
								AssertEquals("My Activity", textColumn.Caption);
								break;
							case AutoWorkItem.Schema.WKI_ActivitySubtype:
							case WorkItemCommon.Schema.ActivitySubtypeDescription:
								subactivityColumn = textColumn;
								AssertEquals("My Subactivity", textColumn.Caption);
								break;
							case AutoWorkItem.Schema.WKI_Priority:
							case WorkItemCommon.Schema.PriorityDescription:
								priorityColumn = textColumn;
								AssertEquals("My Priority", textColumn.Caption);
								break;
						}
					}
				}

				AssertNotNull("column exists", typeColumn);
				AssertNotNull("column exists", areaColumn);
				AssertNotNull("column exists", activityColumn);
				AssertNotNull("column exists", subactivityColumn);
				AssertNotNull("column exists", priorityColumn);
			}
		}
	}
}
