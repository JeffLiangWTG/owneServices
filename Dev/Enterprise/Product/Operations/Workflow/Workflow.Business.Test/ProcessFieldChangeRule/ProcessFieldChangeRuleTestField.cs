using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(ProcessFieldChangeRuleField))]
	public class ProcessFieldChangeRuleFieldTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPFL_FieldName_AutoMatchCase()
		{
			var rule = Factory.New<ProcessFieldChangeRule>();
			rule.PFR_ProcessType = "SHP";
			var field = rule.Fields.AddNew();
			field.PFL_FieldName = JobShipmentSchema.Constants.JS_GoodsDescription.ToLower();

			AssertNotEquals(JobShipmentSchema.Constants.JS_GoodsDescription.ToLower(), field.PFL_FieldName);
			AssertEquals(JobShipmentSchema.Constants.JS_GoodsDescription, field.PFL_FieldName);
		}
	}
}
