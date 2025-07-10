using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ReconRefundedCharge))]
	sealed class ReconRefundedChargeTest : Customs.Business.Testing.CusCodeDataTest<ReconRefundedCharge>
	{
		public void TestSetDefaultValues()
		{
			var charge = Factory.New<ReconRefundedCharge>();
			AssertEquals(CusCodeDataTypeList.Codes.ReconRefundedCharge, charge.CY_Type);
		}

		protected override IEnumerable<ReconRefundedCharge> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var reconDeclaration = new ReconDeclaration(factory.New<JobDeclaration>());
			yield return reconDeclaration.AggregateRefundedFees.AddNewIfNotExists("324");
			yield return reconDeclaration.AggregateRefundedFees.AddNewIfNotExists("499");
			yield return reconDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew().ReconRefundedFees.AddNewIfNotExists("834");
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var reconDeclaration = new ReconDeclaration(factory.New<JobDeclaration>());
			return reconDeclaration.AggregateRefundedFees.AddNewIfNotExists("324");
		}
	}
}
