using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class WorkflowCustomFieldsGridEditableInializerOrderTest : TestCaseWithFactory
	{
		#region Setup

		void PrepareTemplate()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "XXX";
			CreateDefinition(template, "C13", 1, AddOnColumnDataType.Codes.ComboBox);
			CreateDefinition(template, "C12", 1);
			CreateDefinition(template, "C11", 1);
			CreateDefinition(template, "C22", 2);
			CreateDefinition(template, "C21", 2);
			Factory.Save();
		}

		GenCustomColumnDefinition CreateDefinition(ProcessTaskTemplate template, string name, int sequence, string type = AddOnColumnDataType.Codes.String)
		{
			var result = template.GenCustomColumnDefinitions.AddNew();
			result.XC_Type = type;
			result.XC_Name = name;
			result.XC_DisplaySequence = sequence;
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			PrepareTemplate();
		}

		void AssertEqualColumn(string caption, string columnName, ZGridColumnInfo columnStyle, Type propertyDescriptorType)
		{
			WorkflowCustomFieldsGridEditableInitializerTest.AssertEqualColumn(caption, columnName, columnStyle, propertyDescriptorType);
		}

		#endregion

		public void TestColumnOrder()
		{
			// Test custom fields are ordered by their column definition order.
			using (var grid = new ZGrid())
			{
				grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo { ColumnName = "_abc", Caption = "Abc" });

				AssertEquals("Precondition", 1, grid.ColumnStyles.Count);

				WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(grid, new DummyBusinessObjectCollection(Factory), "XXX");

				AssertEquals(7, grid.ColumnStyles.Count);

				AssertEqualColumn("Abc", "_abc", (ZGridColumnInfo)grid.ColumnStyles[0], null);
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[0].GetType());

				AssertEqualColumn("C11", CustomPropertyHelper.GeneratePropertyIdentifier("C11", typeof(ZString)), (ZGridColumnInfo)grid.ColumnStyles[1], typeof(WorkflowReadonlyCustomPropertyDescriptor));
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[1].GetType());

				AssertEqualColumn("C12", CustomPropertyHelper.GeneratePropertyIdentifier("C12", typeof(ZString)), (ZGridColumnInfo)grid.ColumnStyles[2], typeof(WorkflowReadonlyCustomPropertyDescriptor));
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[2].GetType());
				AssertEquals("C12", ((WorkflowReadonlyCustomPropertyDescriptor)((IOverridablePropertyDescriptor)grid.ColumnStyles[2]).PropertyDescriptor).CustomColumnDefinitionName);

				AssertEqualColumn("C13 Code", CustomPropertyHelper.GeneratePropertyIdentifier("C13" + AddOnColumnDataType.PartIdentifier + "1", typeof(ZString)), (ZGridColumnInfo)grid.ColumnStyles[3], typeof(WorkflowReadonlyCustomPropertyDescriptor));
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[3].GetType());
				AssertEquals("C13PART1", ((WorkflowReadonlyCustomPropertyDescriptor)((IOverridablePropertyDescriptor)grid.ColumnStyles[3]).PropertyDescriptor).CustomColumnDefinitionName);

				AssertEqualColumn("C13 Description", CustomPropertyHelper.GeneratePropertyIdentifier("C13" + AddOnColumnDataType.PartIdentifier + "2", typeof(ZString)), (ZGridColumnInfo)grid.ColumnStyles[4], typeof(WorkflowReadonlyCustomPropertyDescriptor));
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[4].GetType());
				AssertEquals("C13PART2", ((WorkflowReadonlyCustomPropertyDescriptor)((IOverridablePropertyDescriptor)grid.ColumnStyles[4]).PropertyDescriptor).CustomColumnDefinitionName);

				AssertEqualColumn("C21", CustomPropertyHelper.GeneratePropertyIdentifier("C21", typeof(ZString)), (ZGridColumnInfo)grid.ColumnStyles[5], typeof(WorkflowReadonlyCustomPropertyDescriptor));
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[5].GetType());

				AssertEqualColumn("C22", CustomPropertyHelper.GeneratePropertyIdentifier("C22", typeof(ZString)), (ZGridColumnInfo)grid.ColumnStyles[6], typeof(WorkflowReadonlyCustomPropertyDescriptor));
				AssertEquals(typeof(ZTextBoxColumnStyleInfo), grid.ColumnStyles[6].GetType());
			}
		}
	}
}
