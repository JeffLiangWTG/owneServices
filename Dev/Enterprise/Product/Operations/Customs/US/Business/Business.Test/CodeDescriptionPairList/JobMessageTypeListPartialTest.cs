using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class JobMessageTypeListTest : TestCaseWithFactory
	{
		public void TestIsImport()
		{
			Assert(JobMessageTypeList.IsImport(JobMessageTypeList.Codes.Import));
			Assert(JobMessageTypeList.IsImport(JobMessageTypeList.Codes.ImportByExternalBroker));
			Assert(JobMessageTypeList.IsImport(JobMessageTypeList.Codes.Miscellaneous));
			Assert(!JobMessageTypeList.IsImport(JobMessageTypeList.Codes.Export));
		}

		public void TestGetListWithAdvanceShippingNotice()
		{
			var jobMessageTypeList1 = JobMessageTypeList.GetListWithAdvanceShippingNotice(Factory);
			Assert("ASN should be added", jobMessageTypeList1.ContainsCode("ASN"));
			var jobMessageTypeList2 = JobMessageTypeList.GetListWithAdvanceShippingNotice(Factory);
			Assert("ASN should be added", jobMessageTypeList2.ContainsCode("ASN"));

			AssertEquals("MessageTypeCodeList should be cached", jobMessageTypeList1, jobMessageTypeList2);
		}
	}
}
