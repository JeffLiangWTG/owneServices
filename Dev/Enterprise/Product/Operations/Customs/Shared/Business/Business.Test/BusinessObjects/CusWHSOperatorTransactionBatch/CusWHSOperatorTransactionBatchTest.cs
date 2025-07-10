using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusWHSOperatorTransactionBatch))]
	sealed class CusWHSOperatorTransactionBatchTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSupportsCloneCore()
		{
			var batch = Factory.NewWithValidTestData<CusWHSOperatorTransactionBatch>();
			AssertEquals("Clone support - CusWHSOperatorTransactionBatch", expected: true, batch.SupportsClone());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var batch = Factory.NewWithValidTestData<CusWHSOperatorTransactionBatch>();
			return batch;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}
	}
}
