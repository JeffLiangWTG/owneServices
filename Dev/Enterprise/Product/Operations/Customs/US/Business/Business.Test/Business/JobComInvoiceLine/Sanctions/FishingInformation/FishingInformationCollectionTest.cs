using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FishingInformationCollection))]
	public class FishingInformationCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			Assert(InvoiceLine.FishingInformations.AllowNew);

			InvoiceLine.US_DisclaimSanctions = true;
			Assert(!InvoiceLine.FishingInformations.AllowNew);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new FishingInformationCollection(InvoiceLine);
		}

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Factory.New<JobComInvoiceLine>()); }
		}
		JobComInvoiceLine invoiceLine;
	}
}
