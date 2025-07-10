using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.US.ISF.Module.Testing
{
	sealed class ISFFilterControlTest : TestCaseWithFactory
	{
		public void TestWorkflowCustomFieldColums()
		{
			var processTaskTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = WorkflowDescriptors.CusISFHeaderWorkflowDescriptorCode;
			var customField1 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "UDF: custom text";
			customField1.XC_Type = AddOnColumnDataType.Codes.String;
			var customField2 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField2.XC_Name = "UDF: custom int";
			customField2.XC_Type = AddOnColumnDataType.Codes.Integer;
			var customField3 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField3.XC_Name = "UDF: custom decimal";
			customField3.XC_Type = AddOnColumnDataType.Codes.Decimal;
			var customField4 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField4.XC_Name = "UDF: custom datetime";
			customField4.XC_Type = AddOnColumnDataType.Codes.Datetime;
			Factory.Save();

			using (var control = new ISFFilterControl(new CusISFHeaderCollection(Factory), new ISFFilterBusinessObject()))
			{
				var workflowColumns = control.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(col => !string.IsNullOrEmpty(col.Caption) && col.Caption.StartsWith("UDF: ", StringComparison.Ordinal)).Select(col => col.Caption).ToArray();
				AssertContainsExactElementsInAnyOrder(new[] { "UDF: custom text", "UDF: custom int", "UDF: custom decimal", "UDF: custom datetime" }, workflowColumns);
			}
		}
	}
}
