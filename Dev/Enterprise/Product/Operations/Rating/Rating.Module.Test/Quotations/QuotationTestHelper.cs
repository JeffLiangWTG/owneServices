using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Rating.Module.Test.Quotations
{
	public static class QuotationTestHelper
	{
		public static void CreateQuotationWorkflowWithCustomFields(BusinessObjectFactory factory)
		{
			ProcessTaskTemplate processTaskTemplate = factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate.P0_ProcessType = "QTN";
			GenCustomColumnDefinition customField1 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "custom text";
			customField1.XC_Type = AddOnColumnDataType.Codes.String;
			GenCustomColumnDefinition customField2 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField2.XC_Name = "custom int";
			customField2.XC_Type = AddOnColumnDataType.Codes.Integer;
			GenCustomColumnDefinition customField3 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField3.XC_Name = "custom decimal";
			customField3.XC_Type = AddOnColumnDataType.Codes.Decimal;
			GenCustomColumnDefinition customField4 = processTaskTemplate.GenCustomColumnDefinitions.AddNew();
			customField4.XC_Name = "custom datetime";
			customField4.XC_Type = AddOnColumnDataType.Codes.Datetime;
			ProcessTaskTemplate processTaskTemplate2 = factory.NewWithValidTestData<ProcessTaskTemplate>();
			processTaskTemplate2.P0_ProcessType = "SHP";
			GenCustomColumnDefinition customField5 = processTaskTemplate2.GenCustomColumnDefinitions.AddNew();
			customField5.XC_Name = "custom shipment string";
			customField5.XC_Type = AddOnColumnDataType.Codes.String;
			factory.Save();
			WorkflowCustomFieldsFilter.ClearCache();
		}
	}
}
