using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Module.Testing
{
	[TestedType(typeof(StatementsStampDutyFilterBusinessObject))]
	public class StatementsStampDutyFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters()
		{
			var filter = new StatementsStampDutyFilterBusinessObject();
			AssertNotNull(filter[StatementsStampDutyFilterBusinessObject.Schema.StatementNumber]);
			AssertNotNull(filter[StatementsStampDutyFilterBusinessObject.Schema.PaymentStatus]);
			AssertNotNull(filter[StatementsStampDutyFilterBusinessObject.Schema.StatementStatus]);
			AssertNotNull(filter[StatementsStampDutyFilterBusinessObject.Schema.DueDate]);
			AssertNotNull(filter[StatementsStampDutyFilterBusinessObject.Schema.PrintDate]);
			AssertNotNull(filter[StatementsStampDutyFilterBusinessObject.Schema.ProcessDate]);
			AssertNotNull(filter[StatementsStampDutyFilterBusinessObject.Schema.PaymentType]);
			AssertNotNull(filter[StatementsStampDutyFilterBusinessObject.Schema.StatementType]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new StatementsStampDutyFilterBusinessObject();
		}
	}
}
