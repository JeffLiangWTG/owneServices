using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class SalesEnquiryFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestCustomFieldsColumns()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = SalesEnquiryWorkflowDescriptor.WorkflowTypeCode;

			var templateDefinition = template.GenCustomColumnDefinitions.AddNew();
			templateDefinition.XC_Name = "CustomString";
			templateDefinition.XC_Type = AddOnColumnDataType.Codes.String;
			Factory.Save();

			var collection = new SalesEnquiryCollection(Factory);
			var filter = new SalesEnquiryFilterBusinessObject();

			using (var form = new ZForm())
			using (var filterControl = new SalesEnquiryFilterControl(collection, filter))
			{
				form.Controls.Add(filterControl);
				form.Show();

				AssertNotNull(filterControl.FilteredGrid.Columns[CustomPropertyHelper.GeneratePropertyIdentifier("CustomString", typeof(ZString))]);
			}
		}
	}
}
