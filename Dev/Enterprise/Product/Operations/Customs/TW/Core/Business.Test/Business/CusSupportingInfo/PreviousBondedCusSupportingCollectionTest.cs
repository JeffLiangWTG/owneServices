using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(PreviousBondedCusSupportingCollection))]
	sealed class PreviousBondedCusSupportingCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<PreviousBondedCusSupporting>
	{
		protected override Customs.Business.CusSupportingInfoCollection<PreviousBondedCusSupporting> GetCusSupportingInfoCollection()
		{
			var jobComInvoice = Factory.New<JobDeclaration>().InvoiceLines.AddNew();
			return new PreviousBondedCusSupportingCollection(jobComInvoice);
		}
	}
}
