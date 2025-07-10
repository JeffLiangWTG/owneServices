namespace Enterprise.Customs.US.Business.Testing
{
	sealed class JobComInvoiceHeaderDocumentWrapperTest : Customs.Business.Testing.BaseJobComInvoiceHeaderTestForDocumentWrapper
	{
		public override void TestCalcExWorks()
		{
			Assert("No ExWorks amount for US", true);
		}

		public override void TestCalcOtherCharges1()
		{
			Assert("No Other charges for US", true);
		}

		public override void TestCalcOtherCharges2()
		{
			Assert("No Other charges for US", true);
		}

		protected override Customs.Business.BaseJobDeclaration GetNewDeclaration() => JobDeclaration.New(Factory);
	}
}
