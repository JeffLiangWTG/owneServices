using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(NZCommodityConstituentCollection))]
	public class NZCommodityConstituentCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return CommodityConstituents;
		}

		NZCommodityConstituentCollection CommodityConstituents
		{
			get { return fCommodityConstituents ?? (fCommodityConstituents = InvoiceLine.CommodityConstituents); }
		}
		NZCommodityConstituentCollection fCommodityConstituents;

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
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
					invoiceHeader = declaration.Invoices.AddNew();
				}

				return invoiceHeader;
			}
		}
		JobComInvoiceHeader invoiceHeader;

		#endregion
	}
}
