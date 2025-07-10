using CargoWise.EntityFramework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(ItemPackagingCollection))]
	public class NZItemPackagingCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return ItemPackages;
		}

		ItemPackagingCollection ItemPackages
		{
			get { return itemPackages ?? (itemPackages = InvoiceLine.ItemPackages); }
		}
		ItemPackagingCollection itemPackages;

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
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					invoiceHeader = declaration.Invoices.AddNew();
				}

				return invoiceHeader;
			}
		}
		JobComInvoiceHeader invoiceHeader;

		#endregion
	}
}
