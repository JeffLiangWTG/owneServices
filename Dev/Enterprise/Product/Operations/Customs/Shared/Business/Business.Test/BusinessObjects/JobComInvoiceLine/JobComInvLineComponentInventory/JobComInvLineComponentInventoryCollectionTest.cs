using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(JobComInvLineComponentInventoryCollection))]
	sealed class JobComInvLineComponentInventoryCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var inventory = Factory.New<JobComInvLineComponentInventory>();
			inventory.JIV_JI = InvoiceLine.PK;
			return inventory;
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(JobComInvLineComponentInventoryCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new JobComInvLineComponentInventoryCollection(InvoiceLine);
		}

		BaseJobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew()); }
		}
		BaseJobComInvoiceLine invoiceLine;

		BaseJobComInvoiceHeader InvoiceHeader
		{
			get { return invoiceHeader ?? (invoiceHeader = Factory.New<BaseJobComInvoiceHeader>()); }
		}
		BaseJobComInvoiceHeader invoiceHeader;
	}
}
