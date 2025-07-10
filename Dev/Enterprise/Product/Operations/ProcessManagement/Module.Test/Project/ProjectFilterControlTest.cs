using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ProcessManagement.Module.Test
{
	class ProjectFilterControlTest : TestCaseWithFactory
	{
		public void TestCustomFieldsColumns()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = new ProjectWorkflowDescriptor().Code;

			var templateDefinition = template.GenCustomColumnDefinitions.AddNew();
			templateDefinition.XC_Name = "CustomString";
			templateDefinition.XC_Type = AddOnColumnDataType.Codes.String;
			Factory.Save();

			var collection = new ProjectCollection(Factory);
			var filter = new ProjectFilterBusinessObject();

			using (var form = new ZForm())
			using (var filterControl = new ProjectFilterControl(collection, filter))
			{
				form.Controls.Add(filterControl);
				form.Show();

				AssertNotNull(filterControl.FilteredGrid.Columns[CustomPropertyHelper.GeneratePropertyIdentifier("CustomString", typeof(ZString))]);
			}
		}

		public void TestCustomLabels()
		{
			ProcessManagementRegistry.Instance.ProjectTypeLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "My Product");
			ProcessManagementRegistry.Instance.ProjectSubtypeLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "My Subactivity");
			ProcessManagementRegistry.Instance.ProjectModuleLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "My Area");

			using (var control = new ProjectFilterControl(null, new ProjectFilterBusinessObject()))
			{
				ZGridColumnInfo typeColumn = null;
				ZGridColumnInfo subTypeColumn = null;
				ZGridColumnInfo moduleColumn = null;
				foreach (var columnStyle in control.Grid.ColumnStyles)
				{
					var textColumn = columnStyle as ZGridColumnInfo;
					if (textColumn != null)
					{
						switch (textColumn.ColumnName)
						{
							case AutoWorkProject.Schema.WKP_Type:
							case Project.Schema.TypeDescription:
								typeColumn = textColumn;
								AssertEquals("My Product", textColumn.Caption);
								break;
							case AutoWorkProject.Schema.WKP_SubType:
							case Project.Schema.SubtypeDescription:
								subTypeColumn = textColumn;
								AssertEquals("My Subactivity", textColumn.Caption);
								break;
							case AutoWorkProject.Schema.WKP_Module:
							case Project.Schema.ModuleDescription:
								moduleColumn = textColumn;
								AssertEquals("My Area", textColumn.Caption);
								break;
						}
					}
				}
				AssertNotNull("column exists", typeColumn);
				AssertNotNull("column exists", moduleColumn);
				AssertNotNull("column exists", subTypeColumn);
			}
		}

		public void TestOpportunityColumns()
		{
			var opportunitySalesPerson = Factory.NewWithValidTestData<GlbStaff>();
			opportunitySalesPerson.GS_Code = "OPS";
			opportunitySalesPerson.GS_FullName = "Opp Sales Person";

			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity1.P8_OpportunityDescription = "What a great opportunity!";
			opportunity1.P8_OpportunityID = "O00001234";
			opportunity1.P8_GS_NKPrimarySalesPerson = opportunitySalesPerson.GS_Code;

			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity2.P8_OpportunityDescription = "Yet another great opportunity!";
			opportunity2.P8_OpportunityID = "O00004321";

			var project1 = Factory.NewWithValidTestData<Project>();
			project1.WKP_ProjectNumber = "PRJ00001111";
			project1.WKP_P8_Opportunity = opportunity1.PK;

			var project2 = Factory.NewWithValidTestData<Project>();
			project2.WKP_ProjectNumber = "PRJ00002222";
			project2.WKP_P8_Opportunity = opportunity2.PK;

			var project3 = Factory.NewWithValidTestData<Project>();
			project3.WKP_ProjectNumber = "PRJ00003333";

			Factory.Save();

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.Project))
			using (var form = (ZForm)module.ShowPopup())
			{
				var grid = module.DisplayGrid;
				grid.SetAllColumnsVisible(true);
				Application.DoEvents();

				var filterControl = form.FindSingle<ZFilterStripBaseControl>();
				filterControl.FirePerformSearch();

				const string opportunityIDColumnName = "Opportunity+P8_OpportunityID";
				AssertEquals("Opportunity ID", GetColumnCaption(grid, opportunityIDColumnName));
				AssertEquals("O00001234", GetCellText(grid, opportunityIDColumnName, 0));
				AssertEquals("O00004321", GetCellText(grid, opportunityIDColumnName, 1));
				AssertEquals(string.Empty, GetCellText(grid, opportunityIDColumnName, 2));

				const string opportunitySalesPersoneColumnName = "Opportunity+P8_GS_NKPrimarySalesPerson";
				AssertEquals("Opportunity Sales Person", GetColumnCaption(grid, opportunitySalesPersoneColumnName));
				AssertEquals("OPS", GetCellText(grid, opportunitySalesPersoneColumnName, 0));
				AssertEquals(string.Empty, GetCellText(grid, opportunitySalesPersoneColumnName, 1));
				AssertEquals(string.Empty, GetCellText(grid, opportunitySalesPersoneColumnName, 2));
			}

			string GetColumnCaption(ZFilterGrid grid, string columnName)
			{
				var columnStyleInfo = grid.ColumnStyles.Cast<ZGridColumnInfo>().Single(s => s.ColumnName == columnName);
				return columnStyleInfo.CaptionResourceString.Caption;
			}

			string GetCellText(ZFilterGrid grid, string columnName, int row)
			{
				var column = grid.Columns.IndexOf(s => s.ColumnName == columnName);
				return grid[row, column].ToString();
			}
		}
	}
}
