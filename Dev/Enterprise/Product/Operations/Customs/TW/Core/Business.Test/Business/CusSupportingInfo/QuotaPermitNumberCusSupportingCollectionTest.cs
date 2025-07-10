using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(QuotaPermitNumberCusSupportingCollection))]
	sealed class QuotaPermitNumberCusSupportingCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<QuotaPermitNumberCusSupporting>
	{
		protected override Customs.Business.CusSupportingInfoCollection<QuotaPermitNumberCusSupporting> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new QuotaPermitNumberCusSupportingCollection(jobComInvoice);
		}
	}
}
