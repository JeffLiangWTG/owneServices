using CargoWise.EntityFramework.Testing;

namespace Enterprise.Workflow.Business.Test
{
	class EDIMessageContentFilterSpecValidationTest : BusinessObjectValidationTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			Schema = Factory.New<EDIMessageContentFilter>().UniversalEvent;
		}

		EDIMessageContentFilterSpec Schema { get; set; }

		public void TestSchemaElement()
		{
			Schema.FilterType = EDIMessageContentFilterTypes.Codes.Exclude;
			AssertMandatoryValidationError(Schema.FilterTypeInfo, false);
			Schema.FilterType = string.Empty;
			AssertMandatoryValidationError(Schema.FilterTypeInfo, true);
		}
	}
}
