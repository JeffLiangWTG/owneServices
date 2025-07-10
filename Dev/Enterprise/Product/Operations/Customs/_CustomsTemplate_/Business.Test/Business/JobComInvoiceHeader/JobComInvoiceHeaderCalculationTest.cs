namespace Enterprise.Customs._CustomsTemplate_.Business.Testing
{
	class JobComInvoiceHeaderCalculationTest : Customs.Business.Testing.BaseJobComInvoiceHeaderCalculationTest
	{
		protected override Customs.Business.BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();
	}
}
