namespace Enterprise.Customs._CustomsTemplate_.Business.Testing
{
	class JobComInvoiceHeaderApportionTest : Customs.Business.Testing.BaseJobComInvoiceHeaderApportionTest
	{
		protected override Customs.Business.BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();
	}
}
