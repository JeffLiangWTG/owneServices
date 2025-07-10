using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(DOTVINCollection))]
	public class DOTVINCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return DOTVINs;
		}

		DOTVINCollection DOTVINs
		{
			get { return dotvins ?? (dotvins = new DOTVINCollection(DOT)); }
		}
		DOTVINCollection dotvins;

		DOT DOT
		{
			get { return dot ?? (dot = InvoiceLine.DOTs.AddNew()); }
		}
		DOT dot;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Factory.New<JobComInvoiceLine>()); }
		}
		JobComInvoiceLine invoiceLine;

		#endregion
	}
}
