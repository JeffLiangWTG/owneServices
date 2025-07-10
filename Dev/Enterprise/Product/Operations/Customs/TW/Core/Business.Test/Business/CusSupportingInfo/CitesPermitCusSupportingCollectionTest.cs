using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CitesPermitCusSupportingCollection))]
	sealed class CitesPermitCusSupportingCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<CitesPermitCusSupporting>
	{
		protected override Customs.Business.CusSupportingInfoCollection<CitesPermitCusSupporting> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new CitesPermitCusSupportingCollection(jobComInvoice);
		}
	}
}
