using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class WorkflowCustomFieldsGridEditableInitializerTest : TestCaseWithFactory
	{
		public void TestDisplayActiveCustomFieldsAndHideInactiveColumnFields()
		{
			var customField1 = new Mock<ICustomColumnDefinition>();
			customField1.Setup(x => x.Identifier).Returns(Guid.NewGuid());
			customField1.Setup(x => x.Type).Returns(AddOnColumnDataType.Codes.String);
			customField1.Setup(x => x.Name).Returns("FIELD1");
			customField1.Setup(x => x.NameLocalized).Returns("FIELD1");
			customField1.Setup(x => x.MaxLength).Returns(10);
			customField1.Setup(x => x.Sequence).Returns(1);
			customField1.Setup(x => x.IsDeleted).Returns(false);
			customField1.Setup(x => x.IsRuleActive).Returns(false);
			customField1.Setup(x => x.GetRules()).Returns(Array.Empty<ICustomAddOnRule>());

			var customField2 = new Mock<ICustomColumnDefinition>();
			customField2.Setup(x => x.Identifier).Returns(Guid.NewGuid());
			customField2.Setup(x => x.Type).Returns(AddOnColumnDataType.Codes.String);
			customField2.Setup(x => x.Name).Returns("FIELD2");
			customField2.Setup(x => x.NameLocalized).Returns("FIELD2");
			customField2.Setup(x => x.MaxLength).Returns(10);
			customField2.Setup(x => x.Sequence).Returns(2);
			customField2.Setup(x => x.IsDeleted).Returns(false);
			customField2.Setup(x => x.IsRuleActive).Returns(false);
			customField2.Setup(x => x.GetRules()).Returns(Array.Empty<ICustomAddOnRule>());

			var customField3 = new Mock<ICustomColumnDefinition>();
			customField3.Setup(x => x.Identifier).Returns(Guid.NewGuid());
			customField3.Setup(x => x.Type).Returns(AddOnColumnDataType.Codes.String);
			customField3.Setup(x => x.Name).Returns("FIELD3");
			customField3.Setup(x => x.NameLocalized).Returns("FIELD3");
			customField3.Setup(x => x.MaxLength).Returns(10);
			customField3.Setup(x => x.Sequence).Returns(3);
			customField3.Setup(x => x.IsDeleted).Returns(false);
			customField3.Setup(x => x.IsRuleActive).Returns(false);
			customField3.Setup(x => x.GetRules()).Returns(Array.Empty<ICustomAddOnRule>());

			var customField4 = new Mock<ICustomColumnDefinition>();
			customField4.Setup(x => x.Identifier).Returns(Guid.NewGuid());
			customField4.Setup(x => x.Type).Returns(AddOnColumnDataType.Codes.String);
			customField4.Setup(x => x.Name).Returns("FIELD4");
			customField4.Setup(x => x.NameLocalized).Returns("FIELD4");
			customField4.Setup(x => x.MaxLength).Returns(10);
			customField4.Setup(x => x.Sequence).Returns(4);
			customField4.Setup(x => x.IsDeleted).Returns(false);
			customField4.Setup(x => x.IsRuleActive).Returns(false);
			customField4.Setup(x => x.GetRules()).Returns(Array.Empty<ICustomAddOnRule>());

			var customFields1 = new Dictionary<string, ICustomColumnDefinition>
				{
					{ "__FIELD1__prop__ZString", customField1.Object },
					{ "__FIELD2__prop__ZString", customField2.Object }
				};

			var customFields2 = new Dictionary<string, ICustomColumnDefinition>
				{
					{ "__FIELD1__prop__ZString", customField1.Object },
					{ "__FIELD3__prop__ZString", customField3.Object },
					{ "__FIELD4__prop__ZString", customField4.Object }
				};

			using (var form = new ZChildForm())
			{
				var grid = new ZGrid();
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "_abc", Caption = "Abc" });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "_bcd", Caption = "Bcd" });
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "_cde", Caption = "Cde" });
				form.Controls.Add(grid);
				var dummies = new DummyBusinessObjectCollection(Factory);
				dummies.AddNew();
				dummies.AddNew();
				dummies.AddNew();
				form.Show();
				grid.SetDataBinding(dummies, "");

				AssertEquals("Precondition", 3, grid.ColumnStyles.Count);
				var dataGridCell = new DataGridCell(1, 2);
				grid.CurrentCell = dataGridCell;
				AssertEquals(dataGridCell, grid.CurrentCell);
				WorkflowCustomFieldsGridEditableInitializer.DisplayActiveCustomFieldsAndHideInactiveColumnFields(grid, dummies, null, customFields1);
				AssertEquals(5, grid.ColumnStyles.Count);
				AssertEqualColumn("Abc", "_abc", (ZGridColumnInfo)grid.ColumnStyles[0], null);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[0].GetType());
				AssertEqualColumn("Bcd", "_bcd", (ZGridColumnInfo)grid.ColumnStyles[1], null);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[1].GetType());
				AssertEqualColumn("Cde", "_cde", (ZGridColumnInfo)grid.ColumnStyles[2], null);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[2].GetType());
				AssertEqualColumn("FIELD1", CustomPropertyHelper.GeneratePropertyIdentifier("FIELD1", typeof(ZString)), (ZGridColumnInfo)grid.ColumnStyles[3], typeof(WorkflowCustomPropertyDescriptor));
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[3].GetType());
				AssertEqualColumn("FIELD2", CustomPropertyHelper.GeneratePropertyIdentifier("FIELD2", typeof(ZString)), (ZGridColumnInfo)grid.ColumnStyles[4], typeof(WorkflowCustomPropertyDescriptor));
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[4].GetType());
				AssertEquals(dataGridCell, grid.CurrentCell);

				WorkflowCustomFieldsGridEditableInitializer.DisplayActiveCustomFieldsAndHideInactiveColumnFields(grid, dummies, customFields1, customFields2);
				AssertEquals(6, grid.ColumnStyles.Count);
				AssertEqualColumn("Abc", "_abc", (ZGridColumnInfo)grid.ColumnStyles[0], null);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[0].GetType());
				AssertEqualColumn("Bcd", "_bcd", (ZGridColumnInfo)grid.ColumnStyles[1], null);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[1].GetType());
				AssertEqualColumn("Cde", "_cde", (ZGridColumnInfo)grid.ColumnStyles[2], null);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[2].GetType());
				AssertEqualColumn("FIELD1", CustomPropertyHelper.GeneratePropertyIdentifier("FIELD1", typeof(ZString)), (ZGridColumnInfo)grid.ColumnStyles[3], typeof(WorkflowCustomPropertyDescriptor));
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[3].GetType());
				AssertEqualColumn("FIELD3", CustomPropertyHelper.GeneratePropertyIdentifier("FIELD3", typeof(ZString)), (ZGridColumnInfo)grid.ColumnStyles[4], typeof(WorkflowCustomPropertyDescriptor));
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[4].GetType());
				AssertEqualColumn("FIELD4", CustomPropertyHelper.GeneratePropertyIdentifier("FIELD4", typeof(ZString)), (ZGridColumnInfo)grid.ColumnStyles[5], typeof(WorkflowCustomPropertyDescriptor));
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[5].GetType());
			}
		}

		public void TestSetFetchForView_NoExceptionThrownWhenUsingFetchHint()
		{
			using (var grid = new ZGrid())
			{
				var collection = new DummyBusinessObjectCollection(Factory);
				collection.Add(DummyBusinessObject.New(Factory));

				WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(grid, collection, "XXX");
				collection.FetchStrategy.FetchForView(new BusinessObject[] { collection[0] }, new TableColumn[] { new TableColumn("", "C11") });

				AssertNoExceptionThrown(() => collection.Factory.Load<GenCustomColumnDefinition>(new ZQuery(GenCustomColumnDefinitionSchema.XC_Name, "C11")));
			}
		}

		public void TestAddWorkflowCustomFieldsColumns()
		{
			using (ZGrid grid = new ZGrid())
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "_abc", Caption = "Abc" });

				AssertEquals("Precondition", 1, grid.ColumnStyles.Count);

				WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(grid, new DummyBusinessObjectCollection(Factory), "XXX");

				AssertEquals(5, grid.ColumnStyles.Count);

				AssertEqualColumn("Abc", "_abc", (ZGridColumnInfo)grid.ColumnStyles[0], null);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[0].GetType());

				AssertEqualColumn("C11", CustomPropertyHelper.GeneratePropertyIdentifier("C11", typeof(ZString)), (ZGridColumnInfo)grid.ColumnStyles[1], typeof(WorkflowReadonlyCustomPropertyDescriptor));
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[1].GetType());

				AssertEqualColumn("C12", CustomPropertyHelper.GeneratePropertyIdentifier("C12", typeof(ZInt)), (ZGridColumnInfo)grid.ColumnStyles[2], typeof(WorkflowReadonlyCustomPropertyDescriptor));
				AssertEquals(typeof(ZCalcEditColumnStyleInfo), grid.ColumnStyles[2].GetType());

				AssertEqualColumn("C21", CustomPropertyHelper.GeneratePropertyIdentifier("C21", typeof(ZDateTime)), (ZGridColumnInfo)grid.ColumnStyles[3], typeof(WorkflowReadonlyCustomPropertyDescriptor));
				AssertEquals(typeof(ZDateEditColumnStyleInfo), grid.ColumnStyles[3].GetType());

				AssertEqualColumn("C22", CustomPropertyHelper.GeneratePropertyIdentifier("C22", typeof(ZBool)), (ZGridColumnInfo)grid.ColumnStyles[4], typeof(WorkflowReadonlyCustomPropertyDescriptor));
				AssertEquals(typeof(ZCheckBoxColumnStyleInfo), grid.ColumnStyles[4].GetType());
			}

			using (ZGrid grid1 = new ZGrid())
			{
				grid1.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "_abc", Caption = "Abc" });
				AssertEquals("Precondition", 1, grid1.ColumnStyles.Count);
				WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(grid1, new DummyBusinessObjectCollection(Factory), "ZZZ", false, company.PK, branch.PK, department.PK);
				AssertEquals(5, grid1.ColumnStyles.Count);

				AssertEqualColumn("C41", CustomPropertyHelper.GeneratePropertyIdentifier("C41", typeof(ZString)), (ZGridColumnInfo)grid1.ColumnStyles[1], typeof(WorkflowReadonlyCustomPropertyDescriptor));
				AssertEqualColumn("C51", CustomPropertyHelper.GeneratePropertyIdentifier("C51", typeof(ZString)), (ZGridColumnInfo)grid1.ColumnStyles[2], typeof(WorkflowReadonlyCustomPropertyDescriptor));
				AssertEqualColumn("Wombo Combo Code", CustomPropertyHelper.GeneratePropertyIdentifier("Wombo ComboPART1", typeof(ZString)), (ZGridColumnInfo)grid1.ColumnStyles[3], typeof(WorkflowReadonlyCustomPropertyDescriptor));
				AssertEqualColumn("Wombo Combo Description", CustomPropertyHelper.GeneratePropertyIdentifier("Wombo ComboPART2", typeof(ZString)), (ZGridColumnInfo)grid1.ColumnStyles[4], typeof(WorkflowReadonlyCustomPropertyDescriptor));
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid1.ColumnStyles[1].GetType());
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid1.ColumnStyles[2].GetType());
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid1.ColumnStyles[3].GetType());
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid1.ColumnStyles[4].GetType());
			}

			using (ZGrid grid2 = new ZGrid())
			{
				grid2.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "_abc", Caption = "Abc" });
				AssertEquals("Precondition", 1, grid2.ColumnStyles.Count);
				WorkflowCustomFieldsGridEditableInitializer.AddWorkflowCustomFieldsColumns(grid2, new DummyBusinessObjectCollection(Factory), "ZZZ", false, false, company.PK, branch.PK, department.PK);
				AssertEquals(5, grid2.ColumnStyles.Count);

				AssertEqualColumn("C41", CustomPropertyHelper.GeneratePropertyIdentifier("C41", typeof(ZString)), (ZGridColumnInfo)grid2.ColumnStyles[1], typeof(WorkflowCustomPropertyDescriptor));
				AssertEqualColumn("C51", CustomPropertyHelper.GeneratePropertyIdentifier("C51", typeof(ZString)), (ZGridColumnInfo)grid2.ColumnStyles[2], typeof(WorkflowCustomPropertyDescriptor));
				AssertEqualColumn("Wombo Combo Code", CustomPropertyHelper.GeneratePropertyIdentifier("Wombo ComboPART1", typeof(ZString)), (ZGridColumnInfo)grid2.ColumnStyles[3], typeof(WorkflowCustomPropertyDescriptor));
				AssertEqualColumn("Wombo Combo Description", CustomPropertyHelper.GeneratePropertyIdentifier("Wombo ComboPART2", typeof(ZString)), (ZGridColumnInfo)grid2.ColumnStyles[4], typeof(WorkflowCustomPropertyDescriptor));
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid2.ColumnStyles[1].GetType());
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid2.ColumnStyles[2].GetType());
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid2.ColumnStyles[3].GetType());
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid2.ColumnStyles[4].GetType());
			}
		}

		public void TestAddWorkflowCustomFieldsColumnsWithRules()
		{
			using (ZGrid grid2 = new ZGrid())
			{
				grid2.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "_abc", Caption = "Abc" });
				AssertEquals("Precondition", 1, grid2.ColumnStyles.Count);
				WorkflowCustomFieldsGridEditableInitializer.AddWorkflowCustomFieldsColumns(grid2, new DummyBusinessObjectCollection(Factory), "WWW");
				AssertEquals(2, grid2.ColumnStyles.Count);

				AssertEqualColumn("C91", CustomPropertyHelper.GeneratePropertyIdentifier("C91", typeof(ZString)), (ZGridColumnInfo)grid2.ColumnStyles[1], typeof(WorkflowCustomPropertyDescriptor));
				AssertEquals("Should use multi control", typeof(ZMultiControlColumnStyleInfo), grid2.ColumnStyles[1].GetType());
			}
		}

		public void TestAddWorkflowCustomFieldsColumns_HandlesCustomFieldsWithSameNameButDifferentType()
		{
			// Arrange
			ProcessTaskTemplate template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "DUP";

			GenCustomColumnDefinition def1 = template1.GenCustomColumnDefinitions.AddNew();
			def1.XC_Name = "REE";
			def1.XC_Type = AddOnColumnDataType.Codes.String;

			GenCustomColumnDefinition def2 = template1.GenCustomColumnDefinitions.AddNew();
			def2.XC_Name = "REE";
			def2.XC_Type = AddOnColumnDataType.Codes.Integer;

			GenCustomColumnDefinition def3 = template1.GenCustomColumnDefinitions.AddNew();
			def3.XC_Name = "REE";
			def3.XC_Type = AddOnColumnDataType.Codes.Boolean;

			GenCustomColumnDefinition def4 = template1.GenCustomColumnDefinitions.AddNew();
			def4.XC_Name = "REE";
			def4.XC_Type = AddOnColumnDataType.Codes.Byte;

			GenCustomColumnDefinition def5 = template1.GenCustomColumnDefinitions.AddNew();
			def5.XC_Name = "REE";
			def5.XC_Type = AddOnColumnDataType.Codes.Byte;

			Factory.Save();

			var expectedColumns = new[] { def1, def2, def3, def4 };

			AssertEquals($"All NameMultilingual variables to have a type code", 4, expectedColumns.Select(c => c.NameMultilingual.Contains(c.XC_Type)).Count());

			CustomFields_AppendTypeToName_TestHelper(false, template1, expectedColumns);
			CustomFields_AppendTypeToName_TestHelper(true, template1, expectedColumns);
		}

		public void TestAddWorkflowCustomFieldsColumns_HandlesCustomFieldsWithSameNameButDifferentTypeReadOnly()
		{
			// Arrange
			ProcessTaskTemplate template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "DUP";

			GenCustomColumnDefinition def1 = template1.GenCustomColumnDefinitions.AddNew();
			def1.XC_Name = "REE";
			def1.XC_Type = AddOnColumnDataType.Codes.String;
			def1.ReadOnly = true;

			GenCustomColumnDefinition def2 = template1.GenCustomColumnDefinitions.AddNew();
			def2.XC_Name = "REE";
			def2.XC_Type = AddOnColumnDataType.Codes.Integer;
			def2.ReadOnly = true;

			GenCustomColumnDefinition def3 = template1.GenCustomColumnDefinitions.AddNew();
			def3.XC_Name = "REE";
			def3.XC_Type = AddOnColumnDataType.Codes.Boolean;
			def3.ReadOnly = true;

			GenCustomColumnDefinition def4 = template1.GenCustomColumnDefinitions.AddNew();
			def4.XC_Name = "RE E";
			def4.XC_Type = AddOnColumnDataType.Codes.Byte;
			def4.ReadOnly = true;

			GenCustomColumnDefinition def5 = template1.GenCustomColumnDefinitions.AddNew();
			def5.XC_Name = "REE";
			def5.XC_Type = AddOnColumnDataType.Codes.Byte;
			def5.ReadOnly = true;

			Factory.Save();

			var expectedColumns = new[] { def1, def2, def3, def4, def5 };

			using (ZGrid grid = new ZGrid())
			{
				AssertNoExceptionThrown(() => WorkflowCustomFieldsGridEditableInitializer.AddWorkflowCustomFieldsColumns(grid, new DummyBusinessObjectCollection(Factory), template1.P0_ProcessType, true, true));
			}
		}

		public void TestNoExceptionThrownIfTypeIsEmpty()
		{
			using (new DisposableAction(ErrorReporter.Clear))
			{
				ProcessTaskTemplate template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
				template1.P0_ProcessType = "DUP";

				GenCustomColumnDefinition emptyDef = template1.GenCustomColumnDefinitions.AddNew();
				emptyDef.XC_Name = "";
				emptyDef.XC_Type = "";

				GenCustomColumnDefinition nonEmptyDef = template1.GenCustomColumnDefinitions.AddNew();
				nonEmptyDef.XC_Name = "test";
				nonEmptyDef.XC_Type = AddOnColumnDataType.Codes.String;

				Factory.Save();

				using (ZGrid grid = new ZGrid())
				{
					AssertNoExceptionThrown(() => WorkflowCustomFieldsGridEditableInitializer.AddWorkflowCustomFieldsColumns(grid, new DummyBusinessObjectCollection(Factory), template1.P0_ProcessType, true, true));
				}
				AssertNotNull("Error Report exists for invalid values", ErrorReporter.LastExceptionReported);
				AssertEquals("LastExceptionReported.Message", "Unable to load Custom Field due to invalid values.", ErrorReporter.LastMessageReported);
			}
		}

		void CustomFields_AppendTypeToName_TestHelper(bool isReadOnly, ProcessTaskTemplate template, GenCustomColumnDefinition[] expectedColumns)
		{
			using (ZGrid grid = new ZGrid())
			{
				// Act
				WorkflowCustomFieldsGridEditableInitializer.AddWorkflowCustomFieldsColumns(grid, new DummyBusinessObjectCollection(Factory), template.P0_ProcessType, true, isReadOnly);

				// Assert
				AssertEquals(4, grid.ColumnStyles.Count);
				var gridColumns = grid.ColumnStyles.Cast<ZGridColumnInfo>().ToList();
				expectedColumns.ForEach(expectedColumn =>
				{
					AssertEquals(
						$"Expected 1 column in grid with name of type {AddOnColumnDataType.GetTypeFromCode(expectedColumn.XC_Type).Name} in columnName",
						1,
						gridColumns.Count(gridColumn => gridColumn.ColumnName.Contains(AddOnColumnDataType.GetTypeFromCode(expectedColumn.XC_Type).Name)));

					AssertEquals(
						$"Expected 1 column in grid captioned {expectedColumn.NameMultilingual}",
						1,
						gridColumns.Count(gridColumn => gridColumn.Caption.Contains(expectedColumn.XC_Type)));
				});
			}
		}

		public void TestAddWorkflowCustomFieldsColumnsWithRules_MultipleCustomFieldsWithSameNameAndType()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "SSS";

			var def1 = template1.GenCustomColumnDefinitions.AddNew();
			def1.XC_Name = "CustomField";
			def1.XC_Type = AddOnColumnDataType.Codes.String;

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "SSS";

			var def2 = template2.GenCustomColumnDefinitions.AddNew();
			def2.XC_Name = "CustomField";
			def2.XC_Type = AddOnColumnDataType.Codes.String;

			Factory.Save();

			using (var grid = new ZGrid())
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "_abc", Caption = "Abc" });
				AssertEquals("Precondition", 1, grid.ColumnStyles.Count);
				WorkflowCustomFieldsGridEditableInitializer.AddWorkflowCustomFieldsColumns(grid, new DummyBusinessObjectCollection(Factory), "SSS");
				AssertEquals(2, grid.ColumnStyles.Count);

				AssertEqualColumn("CustomField", "__CUSTOMFIELD__prop__ZString", (ZGridColumnInfo)grid.ColumnStyles[1], typeof(WorkflowCustomPropertyDescriptor));
				AssertEquals("Should use text box control", typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[1].GetType());
			}

			var rule = Factory.New<GenCustomAddOnRule>();
			rule.XR_IsActive = true;
			rule.XR_Code = "CustomField";
			rule.XR_SourceCode = @"<sourceCode>
  <rules>
    <rule code='InvalidCode'>
      <details>
        <codeDescriptionList>
          <codeDescription code='C1' description='Desc 1' />
          <codeDescription code='C2' description='Desc 2' />
          <codeDescription code='C3' description='Desc 3' />
          <codeDescription code='C4' description='Desc 4' />
        </codeDescriptionList>
      </details>
    </rule>
  </rules>
</sourceCode>";

			def1.XC_XR = rule.PK;

			Factory.Save();

			using (var grid = new ZGrid())
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "_abc", Caption = "Abc" });
				AssertEquals("Precondition", 1, grid.ColumnStyles.Count);
				WorkflowCustomFieldsGridEditableInitializer.AddWorkflowCustomFieldsColumns(grid, new DummyBusinessObjectCollection(Factory), "SSS");
				AssertEquals(2, grid.ColumnStyles.Count);

				AssertEqualColumn("CustomField", "__CUSTOMFIELD__prop__ZString", (ZGridColumnInfo)grid.ColumnStyles[1], typeof(WorkflowCustomPropertyDescriptor));
				AssertEquals("Should use multi control", typeof(ZMultiControlColumnStyleInfo), grid.ColumnStyles[1].GetType());
			}

			def1.XC_XR = ZGuid.Empty;
			def2.XC_XR = rule.PK;

			Factory.Save();

			using (var grid = new ZGrid())
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "_abc", Caption = "Abc" });
				AssertEquals("Precondition", 1, grid.ColumnStyles.Count);
				WorkflowCustomFieldsGridEditableInitializer.AddWorkflowCustomFieldsColumns(grid, new DummyBusinessObjectCollection(Factory), "SSS");
				AssertEquals(2, grid.ColumnStyles.Count);

				AssertEqualColumn("CustomField", "__CUSTOMFIELD__prop__ZString", (ZGridColumnInfo)grid.ColumnStyles[1], typeof(WorkflowCustomPropertyDescriptor));
				AssertEquals("Should use multi control", typeof(ZMultiControlColumnStyleInfo), grid.ColumnStyles[1].GetType());
			}

			def1.XC_XR = rule.PK;
			def2.XC_XR = rule.PK;

			Factory.Save();

			using (var grid = new ZGrid())
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "_abc", Caption = "Abc" });
				AssertEquals("Precondition", 1, grid.ColumnStyles.Count);
				WorkflowCustomFieldsGridEditableInitializer.AddWorkflowCustomFieldsColumns(grid, new DummyBusinessObjectCollection(Factory), "SSS");
				AssertEquals(2, grid.ColumnStyles.Count);

				AssertEqualColumn("CustomField", "__CUSTOMFIELD__prop__ZString", (ZGridColumnInfo)grid.ColumnStyles[1], typeof(WorkflowCustomPropertyDescriptor));
				AssertEquals("Should use multi control", typeof(ZMultiControlColumnStyleInfo), grid.ColumnStyles[1].GetType());
			}
		}

		public void TestDoNotKeepColumnDefinitionsInMemory()
		{
			using (var grid = new ZGrid())
			{
				var workingFactory = new BusinessObjectFactory();
				var collection = new DummyBusinessObjectCollection(workingFactory);

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "_abc", Caption = "Abc" });
				AssertEquals("Precondition", 1, grid.ColumnStyles.Count);
				WorkflowCustomFieldsGridEditableInitializer.AddWorkflowCustomFieldsColumns(grid, collection, "WWW");
				AssertEquals("Custom column should have been added", 2, grid.ColumnStyles.Count);
				AssertEqualColumn("C91", "__C91__prop__ZString", (ZGridColumnInfo)grid.ColumnStyles[1], typeof(WorkflowCustomPropertyDescriptor));

				AssertEquals("Should not have any column definition in main factory", 0, workingFactory.Load<GenCustomColumnDefinition>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
			}
		}

		public void TestAddWorkflowCustomFieldsColumns_LoadedBizos()
		{
			ProcessTaskTemplate template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "NOB";
			AddDefinition(template1, "SWAG", AddOnColumnDataType.Codes.String, false);
			AddDefinition(template1, "SWEG", AddOnColumnDataType.Codes.Boolean, false);
			AddDefinition(template1, "SW0G", AddOnColumnDataType.Codes.Integer, false);
			AddDefinition(template1, "SWUG", AddOnColumnDataType.Codes.String, false);

			ProcessTaskTemplate template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "NOB";
			template2.P0_SubType1 = "BOP";
			AddDefinition(template2, "SWAG", AddOnColumnDataType.Codes.Boolean, false);
			AddDefinition(template2, "SWIG", AddOnColumnDataType.Codes.Boolean, false);
			AddDefinition(template2, "SW0G", AddOnColumnDataType.Codes.Integer, false);
			AddDefinition(template2, "SWUG", AddOnColumnDataType.Codes.String, false);

			Factory.Save();

			using (PersistentFactoryCacheManager.Instance.TrackAllCreatedFactories_ForTest())
			using (var grid = new ZGrid())
			{
				var workingFactory = new BusinessObjectFactory();
				var collection = new DummyBusinessObjectCollection(workingFactory);
				WorkflowCustomFieldsGridEditableInitializer.AddWorkflowCustomFieldsColumns(grid, collection, "NOB");
				var factory = PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories().Single(f => f.NameForDebugging == nameof(WorkflowCustomFieldsGridEditableInitializer.AddWorkflowCustomFieldsColumns));
				AssertEquals("Only load 5 bizos, since there are only 6 distinct rows to be loaded.", 6, factory.Load<GenCustomColumnDefinition>(new ZQuery { FetchOnlyFromLocalCache = true }).Length);
			}
		}

		public void TestAddWorkflowCustomFieldsColumns_DbHits()
		{
			ProcessTaskTemplate template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "NOB";
			AddDefinition(template1, "SWAG", AddOnColumnDataType.Codes.String, true);
			AddDefinition(template1, "SWEG", AddOnColumnDataType.Codes.Boolean, false);
			AddDefinition(template1, "SW0G", AddOnColumnDataType.Codes.Integer, true);
			AddDefinition(template1, "SWUG", AddOnColumnDataType.Codes.String, true);

			Factory.Save();

			var expected = new Dictionary<string, int>
			{
				{ GenCustomAddOnRuleSchema.Constants.TableName, 1 },
				{ GenCustomColumnDefinitionSchema.Constants.TableName, 1 },
			};

			using (AssertDbHitsForAllFactories(expected, useOnlyNewFactories: true, thresholdForUnspecified: 0))
			using (var grid = new ZGrid())
			{
				var workingFactory = new BusinessObjectFactory();
				var collection = new DummyBusinessObjectCollection(workingFactory);

				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "_abc", Caption = "Abc" });
				AssertEquals("Precondition", 1, grid.ColumnStyles.Count);

				WorkflowCustomFieldsGridEditableInitializer.AddWorkflowCustomFieldsColumns(grid, collection, "NOB");

				AssertEquals("Custom column should have been added", 5, grid.ColumnStyles.Count);
			}
		}

		static void AddDefinition(ProcessTaskTemplate template, string name, string type, bool useRule)
		{
			GenCustomColumnDefinition definition = template.GenCustomColumnDefinitions.AddNew();
			definition.XC_Name = name;
			definition.XC_Type = type;

			if (useRule)
			{
				var rule = template.Factory.New<GenCustomAddOnRule>();
				rule.XR_IsActive = true;
				rule.XR_Code = name;
				rule.XR_SourceCode = @"<sourceCode>
  <rules>
    <rule code='InvalidCode'>
      <details>
        <codeDescriptionList>
          <codeDescription code='C1' description='Desc 1' />
          <codeDescription code='C2' description='Desc 2' />
          <codeDescription code='C3' description='Desc 3' />
          <codeDescription code='C4' description='Desc 4' />
        </codeDescriptionList>
      </details>
    </rule>
  </rules>
</sourceCode>";

				definition.XC_XR = rule.PK;
			}
		}

		internal static void AssertEqualColumn(string caption, string columnName, ZGridColumnInfo columnStyle, Type propertyDescriptorType)
		{
			AssertEquals(caption, columnStyle.Caption);
			AssertEquals(columnName, columnStyle.ColumnName);
			Assert(columnStyle.GroupName.IsEmpty());

			if (propertyDescriptorType != null)
			{
				AssertEquals(propertyDescriptorType, ((IOverridablePropertyDescriptor)columnStyle).PropertyDescriptor.GetType());
				AssertEquals(columnName.Split('_').Last(), ((IOverridablePropertyDescriptor)columnStyle).PropertyDescriptor.PropertyType.ToString().Split('.').Last());
			}
			else
			{
				AssertNull(((IOverridablePropertyDescriptor)columnStyle).PropertyDescriptor);
			}
		}

		#region Implemantation

		protected override void SetUp()
		{
			base.SetUp();

			PrepareTemplates();
		}

		GlbCompany company;
		GlbBranch branch;
		GlbDepartment department;

		void PrepareTemplates()
		{
			ProcessTaskTemplate template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "XXX";

			GenCustomColumnDefinition def11 = template1.GenCustomColumnDefinitions.AddNew();
			def11.XC_Name = "C11";
			def11.XC_Type = AddOnColumnDataType.Codes.String;

			GenCustomColumnDefinition def12 = template1.GenCustomColumnDefinitions.AddNew();
			def12.XC_Name = "C12";
			def12.XC_Type = AddOnColumnDataType.Codes.Integer;

			ProcessTaskTemplate template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "XXX";

			GenCustomColumnDefinition def21 = template2.GenCustomColumnDefinitions.AddNew();
			def21.XC_Name = "C21";
			def21.XC_Type = AddOnColumnDataType.Codes.Datetime;

			GenCustomColumnDefinition def22 = template2.GenCustomColumnDefinitions.AddNew();
			def22.XC_Name = "C22";
			def22.XC_Type = AddOnColumnDataType.Codes.Boolean;

			GenCustomColumnDefinition defDuplicate = template2.GenCustomColumnDefinitions.AddNew();
			defDuplicate.XC_Name = "C11";
			defDuplicate.XC_Type = AddOnColumnDataType.Codes.String;

			ProcessTaskTemplate template3 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template3.P0_ProcessType = "YYY";

			GenCustomColumnDefinition def31 = template3.GenCustomColumnDefinitions.AddNew();
			def31.XC_Name = "C31";
			def31.XC_Type = AddOnColumnDataType.Codes.String;

			var template4 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template4.P0_ProcessType = "ZZZ";
			template4.P0_GC = ZGuid.Empty;
			template4.P0_GB = ZGuid.Empty;
			template4.P0_GE = ZGuid.Empty;

			var def41 = template4.GenCustomColumnDefinitions.AddNew();
			def41.XC_Name = "C41";
			def41.XC_Type = AddOnColumnDataType.Codes.String;

			var defcombo = template4.GenCustomColumnDefinitions.AddNew();
			defcombo.XC_Name = "Wombo Combo";
			defcombo.XC_Type = AddOnColumnDataType.Codes.ComboBox;

			var template5 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template5.P0_ProcessType = "ZZZ";

			company = Factory.New<GlbCompany>();
			company.GC_Code = "@@@";
			branch = company.Branches.AddNew();
			branch.GB_Code = "###";
			department = Factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Code = "~~~";
			template5.P0_GC = company.PK;
			template5.P0_GB = branch.PK;
			template5.P0_GE = department.PK;

			var def51 = template5.GenCustomColumnDefinitions.AddNew();
			def51.XC_Name = "C51";
			def51.XC_Type = AddOnColumnDataType.Codes.String;

			var template6 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template6.P0_ProcessType = "ZZZ";
			template6.P0_GC = company.PK;
			template6.P0_GB = branch.PK;
			template6.P0_GE = Factory.NewWithValidTestData<GlbDepartment>().PK;

			var def61 = template6.GenCustomColumnDefinitions.AddNew();
			def61.XC_Name = "C61";
			def61.XC_Type = AddOnColumnDataType.Codes.String;

			var template7 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var newBranch = company.Branches.AddNew();
			newBranch.GB_Code = "%%%";
			template7.P0_ProcessType = "ZZZ";
			template7.P0_GC = company.PK;
			template7.P0_GB = newBranch.PK;
			template7.P0_GE = department.PK;

			var def71 = template7.GenCustomColumnDefinitions.AddNew();
			def71.XC_Name = "C71";
			def71.XC_Type = AddOnColumnDataType.Codes.String;

			var template8 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template8.P0_ProcessType = "ZZZ";
			template8.P0_GC = Factory.NewWithValidTestData<GlbCompany>().PK;
			template8.P0_GB = ZGuid.Empty;
			template8.P0_GE = department.PK;

			var def81 = template8.GenCustomColumnDefinitions.AddNew();
			def81.XC_Name = "C81";
			def81.XC_Type = AddOnColumnDataType.Codes.String;

			var template9 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template9.P0_ProcessType = "WWW";

			var def91 = template9.GenCustomColumnDefinitions.AddNew();
			def91.XC_Name = "C91";
			def91.XC_Type = AddOnColumnDataType.Codes.String;

			var rule91 = Factory.New<GenCustomAddOnRule>();
			rule91.XR_IsActive = true;
			rule91.XR_SourceCode = @"<sourceCode>
  <rules>
    <rule code='InvalidCode'>
      <details>
        <codeDescriptionList>
          <codeDescription code='C1' description='Desc 1' />
          <codeDescription code='C2' description='Desc 2' />
          <codeDescription code='C3' description='Desc 3' />
          <codeDescription code='C4' description='Desc 4' />
        </codeDescriptionList>
      </details>
    </rule>
  </rules>
</sourceCode>";

			def91.XC_XR = rule91.PK;

			Factory.Save();
		}

		#endregion
	}
}
