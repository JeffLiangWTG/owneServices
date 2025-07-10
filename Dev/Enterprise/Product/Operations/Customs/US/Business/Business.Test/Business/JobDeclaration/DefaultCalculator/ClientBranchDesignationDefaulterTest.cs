using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class ClientBranchDesignationDefaulterTest : TestCaseWithFactory
	{
		public void TestClientBranchDesignationDefaulted()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();

			USCustomsDataRegistry.Instance.ClientBranchDesignation.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "XX");

			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			AssertEquals("", declaration.US_ClientBranchDesignation);

			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndImporter;
			AssertEquals("XX", declaration.US_ClientBranchDesignation);

			declaration.US_PaymentType = "";
			AssertEquals("", declaration.US_ClientBranchDesignation);

			USCustomsDataRegistry.Instance.ClientBranchDesignation.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), "01");

			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			AssertEquals("01", declaration.US_ClientBranchDesignation);
		}
	}
}
