using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public class BaseJobComInvoiceHeaderCollectionTest : CountrySpecificTestCase
	{
		public void TestInvoiceCurrencies()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceGroupHeader groupHeader = testDec.JobComInvoiceGroupHeaders[0];

			BaseJobComInvoiceHeader invoice1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_RX_NKInvoice_Currency = "USD";

			string[] result = groupHeader.JobComInvoiceHeaders.InvoiceCurrencies;
			AssertEquals("One currency only", 1, result.Length);
			AssertEquals("One currency only", "USD", result[0]);

			BaseJobComInvoiceHeader invoice2 = groupHeader.JobComInvoiceHeaders.AddNew();
			result = groupHeader.JobComInvoiceHeaders.InvoiceCurrencies;
			AssertEquals("One currency only", 1, result.Length);
			AssertEquals("One currency only", "USD", result[0]);

			invoice2.JZ_RX_NKInvoice_Currency = "AUD";
			result = groupHeader.JobComInvoiceHeaders.InvoiceCurrencies;
			AssertEquals("two currencies only", 2, result.Length);
			AssertEquals("two currencies only", "USD", result[0]);
			AssertEquals("two currencies only", "AUD", result[1]);
		}

		public void TestInvoiceIncoterms()
		{
			var testDec = BaseJobDeclaration.New(Factory);
			var groupHeader = testDec.JobComInvoiceGroupHeaders[0];

			var invoice1 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_IncoTerm = invoice1.IncotermEquivalentToCFRForTesting;

			var result = groupHeader.JobComInvoiceHeaders.InvoiceIncoterms;
			AssertEquals("One incoterm only", 1, result.Length);
			AssertEquals("One incoterm only", invoice1.JZ_IncoTerm, result[0]);

			var invoice2 = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_IncoTerm = "";
			result = groupHeader.JobComInvoiceHeaders.InvoiceIncoterms;
			AssertEquals("One incoterm only", 1, result.Length);
			AssertEquals("One incoterm only", invoice1.JZ_IncoTerm, result[0]);

			invoice2.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			result = groupHeader.JobComInvoiceHeaders.InvoiceIncoterms;
			AssertEquals("two incoterms", 2, result.Length);

			bool invoice1IncotermFound = false;
			bool invoice2IncotermFound = false;
			foreach (var incoterm in result)
			{
				if (incoterm == invoice1.JZ_IncoTerm)
				{
					invoice1IncotermFound = true;
				}
				else if (incoterm == invoice2.JZ_IncoTerm)
				{
					invoice2IncotermFound = true;
				}
			}

			AssertEquals("two incoterm only", true, invoice1IncotermFound);
			AssertEquals("two incoterm only", true, invoice2IncotermFound);
		}

		public void TestClone()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader = testDec.Invoices.AddNew();
			invoiceHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100m, testDec.LocalCurrencyCode);

			BaseJobDeclaration decCloned = (BaseJobDeclaration)testDec.TemplateCopy();
			AssertEquals("Cloned Invoice charges", 1, decCloned.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].Charges.Count);
		}

		public void TestSuspendAdditionallyForImportZeroValues()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var collection = declaration.Invoices;

			// Adds headers to the collection
			PopulateCollection(collection);

			AssertEquals("Sequence Number reset to 1 without suspender", (ZShort)1, collection[0].JZ_InvoiceDisplaySequence);
			AssertEquals("Sequence Number reset to 2 without suspender", (ZShort)2, collection[1].JZ_InvoiceDisplaySequence);
			AssertEquals("Sequence Number reset to 3 without suspender", (ZShort)3, collection[2].JZ_InvoiceDisplaySequence);
			AssertEquals("Sequence Number reset to 4 without suspender", (ZShort)4, collection[3].JZ_InvoiceDisplaySequence);
			AssertEquals("Sequence Number reset to 5 without suspender", (ZShort)5, collection[4].JZ_InvoiceDisplaySequence);

			using (collection.SuspendAdditionallyForImport())
			{
				PopulateCollection(collection);

				AssertEquals("Sequence Number unaltered as 0", (ZShort)0, collection[0].JZ_InvoiceDisplaySequence);
				AssertEquals("Sequence Number unaltered as 0", (ZShort)0, collection[1].JZ_InvoiceDisplaySequence);
				AssertEquals("Sequence Number unaltered as 0", (ZShort)0, collection[2].JZ_InvoiceDisplaySequence);
				AssertEquals("Sequence Number unaltered as 0", (ZShort)0, collection[3].JZ_InvoiceDisplaySequence);
				AssertEquals("Sequence Number unaltered as 0", (ZShort)0, collection[4].JZ_InvoiceDisplaySequence);
			}

			AssertEquals("Sequence Number reset to 1 after suspender because of zero in source", (ZShort)1, collection[0].JZ_InvoiceDisplaySequence);
			AssertEquals("Sequence Number reset to 2 after suspender because of zero in source", (ZShort)2, collection[1].JZ_InvoiceDisplaySequence);
			AssertEquals("Sequence Number reset to 3 after suspender because of zero in source", (ZShort)3, collection[2].JZ_InvoiceDisplaySequence);
			AssertEquals("Sequence Number reset to 4 after suspender because of zero in source", (ZShort)4, collection[3].JZ_InvoiceDisplaySequence);
			AssertEquals("Sequence Number reset to 5 after suspender because of zero in source", (ZShort)5, collection[4].JZ_InvoiceDisplaySequence);
		}

		public void TestSuspendAdditionallyForImportNonZeroValues()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var collection = declaration.Invoices;

			// Adds headers to the collection
			PopulateCollection(collection, new ZShort[] { 37, 40, 70, 87, 93 });

			AssertEquals("Sequence Number reset to 1 without suspender", (ZShort)1, collection[0].JZ_InvoiceDisplaySequence);
			AssertEquals("Sequence Number reset to 2 without suspender", (ZShort)2, collection[1].JZ_InvoiceDisplaySequence);
			AssertEquals("Sequence Number reset to 3 without suspender", (ZShort)3, collection[2].JZ_InvoiceDisplaySequence);
			AssertEquals("Sequence Number reset to 4 without suspender", (ZShort)4, collection[3].JZ_InvoiceDisplaySequence);
			AssertEquals("Sequence Number reset to 5 without suspender", (ZShort)5, collection[4].JZ_InvoiceDisplaySequence);

			using (collection.SuspendAdditionallyForImport())
			{
				PopulateCollection(collection, new ZShort[] { 37, 40, 70, 87, 93 });

				AssertEquals("Sequence Number unaltered as 37", (ZShort)37, collection[0].JZ_InvoiceDisplaySequence);
				AssertEquals("Sequence Number unaltered as 40", (ZShort)40, collection[1].JZ_InvoiceDisplaySequence);
				AssertEquals("Sequence Number unaltered as 70", (ZShort)70, collection[2].JZ_InvoiceDisplaySequence);
				AssertEquals("Sequence Number unaltered as 87", (ZShort)87, collection[3].JZ_InvoiceDisplaySequence);
				AssertEquals("Sequence Number unaltered as 93", (ZShort)93, collection[4].JZ_InvoiceDisplaySequence);
			}

			AssertEquals("Sequence Number remains unaltered as 37", (ZShort)37, collection[0].JZ_InvoiceDisplaySequence);
			AssertEquals("Sequence Number remains unaltered as 40", (ZShort)40, collection[1].JZ_InvoiceDisplaySequence);
			AssertEquals("Sequence Number remains unaltered as 70", (ZShort)70, collection[2].JZ_InvoiceDisplaySequence);
			AssertEquals("Sequence Number remains unaltered as 87", (ZShort)87, collection[3].JZ_InvoiceDisplaySequence);
			AssertEquals("Sequence Number remains unaltered as 93", (ZShort)93, collection[4].JZ_InvoiceDisplaySequence);
		}

		public void TestSuspendAdditionallyForImportZeroAndNonZeroValues()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var collection = declaration.Invoices;

			// Adds headers to the collection
			PopulateCollection(collection, new ZShort[] { 0, 20, 37, 47, 84 });

			AssertEquals("Sequence Number 1 without suspender", (ZShort)1, collection[0].JZ_InvoiceDisplaySequence);
			AssertEquals("Sequence Number 2 without suspender", (ZShort)2, collection[1].JZ_InvoiceDisplaySequence);
			AssertEquals("Sequence Number 3 without suspender", (ZShort)3, collection[2].JZ_InvoiceDisplaySequence);
			AssertEquals("Sequence Number 4 without suspender", (ZShort)4, collection[3].JZ_InvoiceDisplaySequence);
			AssertEquals("Sequence Number 5 without suspender", (ZShort)5, collection[4].JZ_InvoiceDisplaySequence);

			using (collection.SuspendAdditionallyForImport())
			{
				PopulateCollection(collection, new ZShort[] { 0, 20, 37, 47, 84 });

				AssertEquals("Sequence Number unaltered as 0", (ZShort)0, collection[0].JZ_InvoiceDisplaySequence);
				AssertEquals("Sequence Number unaltered as 20", (ZShort)20, collection[1].JZ_InvoiceDisplaySequence);
				AssertEquals("Sequence Number unaltered as 37", (ZShort)37, collection[2].JZ_InvoiceDisplaySequence);
				AssertEquals("Sequence Number unaltered as 47", (ZShort)47, collection[3].JZ_InvoiceDisplaySequence);
				AssertEquals("Sequence Number unaltered as 84", (ZShort)84, collection[4].JZ_InvoiceDisplaySequence);
			}

			AssertEquals("Sequence Number reset to 1 after suspender because of zero in source", (ZShort)1, collection[0].JZ_InvoiceDisplaySequence);
			AssertEquals("Sequence Number reset to 2 after suspender because of zero in source", (ZShort)2, collection[1].JZ_InvoiceDisplaySequence);
			AssertEquals("Sequence Number reset to 3 after suspender because of zero in source", (ZShort)3, collection[2].JZ_InvoiceDisplaySequence);
			AssertEquals("Sequence Number reset to 4 after suspender because of zero in source", (ZShort)4, collection[3].JZ_InvoiceDisplaySequence);
			AssertEquals("Sequence Number reset to 5 after suspender because of zero in source", (ZShort)5, collection[4].JZ_InvoiceDisplaySequence);
		}

		#region Implementation

		void PopulateCollection(InvoiceHeaderActiveCollection collection, ZShort[] values = null)
		{
			collection.DeleteAll();

			collection.AddNew();
			collection.AddNew();
			collection.AddNew();
			collection.AddNew();
			collection.AddNew();

			if (values != null && values.Length == 5)
			{
				collection[0].JZ_InvoiceDisplaySequence = values[0];
				collection[1].JZ_InvoiceDisplaySequence = values[1];
				collection[2].JZ_InvoiceDisplaySequence = values[2];
				collection[3].JZ_InvoiceDisplaySequence = values[3];
				collection[4].JZ_InvoiceDisplaySequence = values[4];
			}
		}

		#endregion
	}
}
