namespace Enterprise.Customs.US.Business.Testing
{
	sealed class JobComInvoiceHeaderApportionTest : Customs.Business.Testing.BaseJobComInvoiceHeaderApportionTest
	{
		public override void TestApportionExWorksAmount()
		{
			Assert("US does not have ExWorks Amount", true);
		}

		public override void TestApportionOtherCharges()
		{
			Assert("US does not have Other amount", true);
		}

		protected override Customs.Business.BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();
	}
}
