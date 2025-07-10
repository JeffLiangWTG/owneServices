using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class TypeSafeJobComInvoiceGroupHeaderTest : TestCaseWithFactory
	{
		public void TestIncotermFactory()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(typeof(DeliveryTermAndChargeFactory), groupInvoice.IncoTermAndChargeFactory.GetType());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(typeof(CommonIncoTermAndCustomsChargeFactory), groupInvoice.IncoTermAndChargeFactory.GetType());
		}

		public void TestGroupInvoiceChargeCollection()
		{
			AssertEquals(typeof(JobComInvChargeCollection<GroupInvoiceCharge>), groupInvoice.Charges.GetType());
		}

		JobDeclaration declaration;
		JobComInvoiceGroupHeader groupInvoice;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			groupInvoice = declaration.JobComInvoiceGroupHeaders[0];
		}
	}
}
