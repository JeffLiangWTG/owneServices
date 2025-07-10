using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(HighTechLicenseCusSupportingCollection))]
	sealed class HighTechLicenseCusSupportingCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<HighTechLicenseCusSupporting>
	{
		protected override Customs.Business.CusSupportingInfoCollection<HighTechLicenseCusSupporting> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new HighTechLicenseCusSupportingCollection(jobComInvoice);
		}
	}
}
