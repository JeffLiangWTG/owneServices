using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class JobComInvoiceHeaderCalculationTest : BaseJobComInvoiceHeaderCalculationTest
	{
		protected override BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();
	}
}
