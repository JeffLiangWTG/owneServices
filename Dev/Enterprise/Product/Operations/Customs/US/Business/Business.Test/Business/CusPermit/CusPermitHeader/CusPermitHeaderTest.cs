using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CusPermitHeader))]
	sealed class CusPermitHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAvailablePermitTypes()
		{
			Assert("FTZ Permit Type Added", permitHeader.Lookups.PermitTypes.ContainsCode("FTZ"));
		}

		public void TestAvailablePermitTransactionTypes()
		{
			Assert("FTZ Transaction Type Added", permitHeader.CusPermitLineTransactions.AddNew().Lookups.PermitTransactionTypes.ContainsCode("FTZ"));
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = Factory.NewWithValidTestData<CusPermitHeader>();
			header.CusPermitLineTransactions.DeleteAll();
			return header;
		}

		CusPermitHeader permitHeader;
		protected override void SetUp()
		{
			base.SetUp();
			permitHeader = Factory.NewWithValidTestData<CusPermitHeader>();
		}
	}
}
