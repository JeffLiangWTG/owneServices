using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(MedicalInstrumentOrFoodCusSupportingCollection))]
	sealed class MedicalInstrumentOrFoodCusSupportingCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<MedicalInstrumentOrFoodCusSupporting>
	{
		protected override CusSupportingInfoCollection<MedicalInstrumentOrFoodCusSupporting> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new MedicalInstrumentOrFoodCusSupportingCollection(jobComInvoice);
		}
	}
}
