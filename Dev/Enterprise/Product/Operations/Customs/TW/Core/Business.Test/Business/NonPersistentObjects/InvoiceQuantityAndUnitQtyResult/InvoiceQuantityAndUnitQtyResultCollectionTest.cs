using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(InvoiceQuantityAndUnitQtyResultCollection))]
	sealed class InvoiceQuantityAndUnitQtyResultCollectionTest : NonPersistentBusinessObjectCollectionTestCase<InvoiceQuantityAndUnitQtyResultCollection>
	{
		protected override InvoiceQuantityAndUnitQtyResultCollection GetCollectionToTest()
		{
			return new InvoiceQuantityAndUnitQtyResultCollection(InvoiceLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new InvoiceQuantityAndUnitQtyResult();
		}

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					invoiceLine = Factory.New<JobComInvoiceLine>();
				}

				return invoiceLine;
			}
		}

		JobComInvoiceLine invoiceLine;
		public void TestShouldRebuildElements()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			var line = header.JobComInvoiceLines.AddNew();
			line.JI_InvoiceQuantity = 1m;
			line.JI_InvoiceUQ = "ADT";
			line = header.JobComInvoiceLines.AddNew();
			line.JI_InvoiceQuantity = 2m;
			line.JI_InvoiceUQ = "ADT";
			line = header.JobComInvoiceLines.AddNew();
			line.JI_InvoiceQuantity = 1m;
			line.JI_InvoiceUQ = "BAG";
			var invoiceQuantityAndUnitQtyResultCollection = new InvoiceQuantityAndUnitQtyResultCollection(line);
			AssertEquals(0, invoiceQuantityAndUnitQtyResultCollection.Count);
			AssertEquals(true, invoiceQuantityAndUnitQtyResultCollection.ShouldRebuildElements());
			AssertEquals(2, invoiceQuantityAndUnitQtyResultCollection.Count);
			Assert(invoiceQuantityAndUnitQtyResultCollection.Cast<InvoiceQuantityAndUnitQtyResult>().Any(x => x.InvoiceUQ == "ADT" && x.InvoiceQuantity == 3m));
			Assert(invoiceQuantityAndUnitQtyResultCollection.Cast<InvoiceQuantityAndUnitQtyResult>().Any(x => x.InvoiceUQ == "BAG" && x.InvoiceQuantity == 1m));
			AssertEquals(false, invoiceQuantityAndUnitQtyResultCollection.ShouldRebuildElements());
			line.JI_InvoiceQuantity = 2m;
			AssertEquals(true, invoiceQuantityAndUnitQtyResultCollection.ShouldRebuildElements());
			AssertEquals(2, invoiceQuantityAndUnitQtyResultCollection.Count);
			Assert(invoiceQuantityAndUnitQtyResultCollection.Cast<InvoiceQuantityAndUnitQtyResult>().Any(x => x.InvoiceUQ == "ADT" && x.InvoiceQuantity == 3m));
			Assert(invoiceQuantityAndUnitQtyResultCollection.Cast<InvoiceQuantityAndUnitQtyResult>().Any(x => x.InvoiceUQ == "BAG" && x.InvoiceQuantity == 2m));
			line.JI_InvoiceUQ = "ADT";
			AssertEquals(true, invoiceQuantityAndUnitQtyResultCollection.ShouldRebuildElements());
			AssertEquals(1, invoiceQuantityAndUnitQtyResultCollection.Count);
			Assert(invoiceQuantityAndUnitQtyResultCollection.Cast<InvoiceQuantityAndUnitQtyResult>().Any(x => x.InvoiceUQ == "ADT" && x.InvoiceQuantity == 5m));
			header = declaration.Invoices.AddNew();
			line.JI_JZ = header.PK;
			AssertEquals(true, invoiceQuantityAndUnitQtyResultCollection.ShouldRebuildElements());
			AssertEquals(1, invoiceQuantityAndUnitQtyResultCollection.Count);
			Assert(invoiceQuantityAndUnitQtyResultCollection.Cast<InvoiceQuantityAndUnitQtyResult>().Any(x => x.InvoiceUQ == "ADT" && x.InvoiceQuantity == 2m));
		}

		public void TestGetHashCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			var line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_InvoiceQuantity = 1m;
			line1.JI_InvoiceUQ = "ADT";
			var invoiceQuantityAndUnitQtyResultCollection = new InvoiceQuantityAndUnitQtyResultCollection(line1);
			var expected = 1 ^ "ADT".GetHashCode() ^ 1m.GetHashCode();
			AssertEquals(expected, invoiceQuantityAndUnitQtyResultCollection.GetHashCode());

			var line2 = header.JobComInvoiceLines.AddNew();
			line2.JI_InvoiceQuantity = 2m;
			line2.JI_InvoiceUQ = "PCE";
			expected = 2.GetHashCode() ^ "ADT".GetHashCode() ^ 1m.GetHashCode() ^ "PCE".GetHashCode() ^ 2m.GetHashCode();
			invoiceQuantityAndUnitQtyResultCollection = new InvoiceQuantityAndUnitQtyResultCollection(line2);
			AssertEquals(expected, invoiceQuantityAndUnitQtyResultCollection.GetHashCode());
		}
	}
}
