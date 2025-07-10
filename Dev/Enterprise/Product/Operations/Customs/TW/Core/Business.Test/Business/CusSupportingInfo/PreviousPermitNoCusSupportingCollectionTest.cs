using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(PreviousPermitNoCusSupportingCollection))]
	sealed class PreviousPermitNoCusSupportingCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<PreviousPermitNoCusSupporting>
	{
		protected override Customs.Business.CusSupportingInfoCollection<PreviousPermitNoCusSupporting> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new PreviousPermitNoCusSupportingCollection(jobComInvoice);
		}
	}
}
