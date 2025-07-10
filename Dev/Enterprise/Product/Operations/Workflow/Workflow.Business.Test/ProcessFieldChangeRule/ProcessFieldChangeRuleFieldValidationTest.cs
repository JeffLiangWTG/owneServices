using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business.Test
{
	internal class ProcessFieldChangeRuleFieldValidationTest : BusinessObjectValidationTestCase
	{
		public void TestPFL_FieldName()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "SHP";
			var field = (ProcessFieldChangeRuleField)rule.Fields.AddNew();
			field.Validation.ValidatePFL_FieldName();
			AssertHasError(field.PFL_FieldNameInfo, "Please enter a value.");
			field.PFL_FieldName = "XXY";
			AssertHasError(field.PFL_FieldNameInfo, "Enter a valid selection.");
			field.PFL_FieldName = JobShipmentSchema.Constants.JS_HouseBill;
			AssertNoError(field.PFL_FieldNameInfo, "Enter a valid selection.");

			var field2 = (ProcessFieldChangeRuleField)rule.Fields.AddNew();
			field2.PFL_FieldName = JobShipmentSchema.Constants.JS_HouseBill;
			AssertHasError(field2.PFL_FieldNameInfo, JobShipmentSchema.Constants.JS_HouseBill + " is already included");
		}
	}
}
