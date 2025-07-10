using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(AviationFuelTypeCollection))]
	class AviationFuelTypeCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<AviationFuelType>
	{
		protected override Customs.Business.CusSupportingInfoCollection<AviationFuelType> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new AviationFuelTypeCollection(jobComInvoice);
		}
	}
}
