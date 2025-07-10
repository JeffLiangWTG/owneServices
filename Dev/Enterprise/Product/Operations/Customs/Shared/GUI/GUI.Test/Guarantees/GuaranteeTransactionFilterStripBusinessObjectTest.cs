using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Guarantees.Testing
{
	[TestedType(typeof(GuaranteeTransactionFilterStripBusinessObject))]
	sealed class GuaranteeTransactionFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters()
		{
			var filter = new GuaranteeTransactionFilterStripBusinessObject();
			AssertNotNull(filter[GuaranteeTransactionFilterStripBusinessObject.FilterConstants.TransactionDate]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new GuaranteeTransactionFilterStripBusinessObject();
	}
}
