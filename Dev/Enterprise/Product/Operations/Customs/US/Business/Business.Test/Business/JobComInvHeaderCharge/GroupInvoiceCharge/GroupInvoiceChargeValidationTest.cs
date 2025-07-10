using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class GroupInvoiceChargeValidationTest : TestCaseWithFactory
	{
		public void TestCheckJ7_FullOrPartialApportionment()
		{
			charge.J7_FullOrPartialApportionment = "~";
			AssertHasMessageError(charge.J7_FullOrPartialApportionmentInfo, GroupInvoiceChargeValidation.FullOrPartialApportionmentShouldBeInList);
			charge.J7_FullOrPartialApportionment = charge.Lookups.ApportionmentTypeList[0].Code;
			AssertNoMessageError(charge.J7_FullOrPartialApportionmentInfo, GroupInvoiceChargeValidation.FullOrPartialApportionmentShouldBeInList);
		}

		public void TestIsCIFComponentUsed()
		{
			Assert(new GroupInvoiceChargeValidationForTest(Factory.New<GroupInvoiceCharge>()).IsCIFComponentUsedExposed);
		}

		JobDeclaration declaration;
		GroupInvoiceCharge charge;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			charge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			declaration.US_EnableAII = true;
		}

		sealed class GroupInvoiceChargeValidationForTest : GroupInvoiceChargeValidation
		{
			public GroupInvoiceChargeValidationForTest(GroupInvoiceCharge invoiceLineCharge) : base(invoiceLineCharge)
			{
			}

			internal bool IsCIFComponentUsedExposed => IsCIFComponentUsed;
		}
	}
}
