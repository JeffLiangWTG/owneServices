using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.ProcessManagement.Business;

namespace Enterprise.ProcessManagement.Module.Test
{
	public class CustomerServiceTicketFilterControlTest : TestCaseWithFactory
	{
		public void TestCustomLabels()
		{
			ProcessManagementRegistry.Instance.SelectionCriterion1Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "We are number 1");
			ProcessManagementRegistry.Instance.SelectionCriterion2Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "We are number 2");
			ProcessManagementRegistry.Instance.SelectionCriterion3Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "We are number 3");
			ProcessManagementRegistry.Instance.SelectionCriterion4Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "We are number 4");
			ProcessManagementRegistry.Instance.SelectionCriterion5Caption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "We are number 5");

			using (var control = new CustomerServiceTicketFilterControl(null, new CustomerServiceTicketFilterBusinessObject()))
			{
				ZGridColumnInfo criterion1Column = null;
				ZGridColumnInfo criterion2Column = null;
				ZGridColumnInfo criterion3Column = null;
				ZGridColumnInfo criterion4Column = null;
				ZGridColumnInfo criterion5Column = null;

				foreach (var columnStyle in control.Grid.ColumnStyles)
				{
					var textColumn = columnStyle as ZGridColumnInfo;
					if (textColumn != null)
					{
						switch (textColumn.ColumnName)
						{
							case AutoWorkRequest.Schema.WKR_SelectionCriteria1:
								criterion1Column = textColumn;
								AssertEquals("We are number 1", textColumn.Caption);
								break;
							case AutoWorkRequest.Schema.WKR_SelectionCriteria2:
								criterion2Column = textColumn;
								AssertEquals("We are number 2", textColumn.Caption);
								break;
							case AutoWorkRequest.Schema.WKR_SelectionCriteria3:
								criterion3Column = textColumn;
								AssertEquals("We are number 3", textColumn.Caption);
								break;
							case AutoWorkRequest.Schema.WKR_SelectionCriteria4:
								criterion4Column = textColumn;
								AssertEquals("We are number 4", textColumn.Caption);
								break;
							case AutoWorkRequest.Schema.WKR_SelectionCriteria5:
								criterion5Column = textColumn;
								AssertEquals("We are number 5", textColumn.Caption);
								break;
						}
					}
				}

				AssertNotNull("column exists", criterion1Column);
				AssertNotNull("column exists", criterion2Column);
				AssertNotNull("column exists", criterion3Column);
				AssertNotNull("column exists", criterion4Column);
				AssertNotNull("column exists", criterion5Column);
			}
		}
	}
}
