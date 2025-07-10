using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(NZCommodityCollection))]
	public class NZCommodityCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return CommodityLines;
		}

		NZCommodityCollection CommodityLines
		{
			get { return commodityLines ?? (commodityLines = InvoiceLine.CommodityLines); }
		}
		NZCommodityCollection commodityLines;

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
