using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(NZCommodityItineraryCollection))]
	public class NZCommodityItineraryCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return CommodityItineraries;
		}

		NZCommodityItineraryCollection CommodityItineraries
		{
			get { return commodityLines ?? (commodityLines = InvoiceLine.CommodityItineraries); }
		}
		NZCommodityItineraryCollection commodityLines;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew();
				}
				return invoiceLine;
			}
		}
		JobComInvoiceLine invoiceLine;

		JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				if (invoiceHeader == null)
				{
					JobDeclaration declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					invoiceHeader = declaration.Invoices.AddNew();
				}

				return invoiceHeader;
			}
		}
		JobComInvoiceHeader invoiceHeader;

		#endregion
	}
}
