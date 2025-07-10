using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(InvoiceHeaderFilteredActiveCollection))]
	sealed class InvoiceHeaderFilteredActiveCollectionTest : ActiveBusinessObjectCollectionTestCase<InvoiceHeaderFilteredActiveCollection>
	{
		public void TestAddInvoiceChangesInBondRelatedRecords()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = ZBool.True;
			declaration.US_EnableAII = ZBool.False;
			var invoice = Factory.New<JobComInvoiceHeader>();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			invoiceLine.JI_Tariff = "1010101010";
			invoiceLine.JI_JZ = invoice.PK;
			var message = Factory.New<EDIMessage>();
			invoice.Messages.Add(message);
			var inBondRelatedRecords = declaration.InBondRelatedRecords;
			AssertEquals(0, inBondRelatedRecords.Count);
			declaration.FilteredInvoices.Add(invoice);
			AssertEquals(1, inBondRelatedRecords.Count);
			AssertNotNull(inBondRelatedRecords.GetElementWrapping(invoice));
		}

		public void TestMatchesFilterCore()
		{
			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader reconOriginalEntry = reconDeclaration.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice1 = reconOriginalEntry.Invoice;
			ReconOriginalEntryHeader reconOriginalEntry2 = reconDeclaration.OriginalEntries.AddNew();
			JobComInvoiceHeader invoice2 = reconOriginalEntry2.Invoice;
			AssertEquals(2, reconDeclaration.FilteredInvoices.Count);
			reconDeclaration.SelectedOriginalEntry = reconOriginalEntry.CH_PK;
			AssertEquals("should have been filtered", 1, reconDeclaration.FilteredInvoices.Count);
			AssertEquals("only invoice1 should be in the collection", invoice1, reconDeclaration.FilteredInvoices[0]);
		}

		public void TestSuspendAdditionallyForImportZeroValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			var collection = declaration.FilteredInvoices;
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
			var declaration = Factory.New<JobDeclaration>();
			var collection = declaration.FilteredInvoices;
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
			var declaration = Factory.New<JobDeclaration>();
			var collection = declaration.FilteredInvoices;
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

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_JE = declaration.PK;
			return invoice;
		}

		protected override InvoiceHeaderFilteredActiveCollection GetCollectionToTest() => Factory.New<JobDeclaration>().FilteredInvoices;

		void PopulateCollection(InvoiceHeaderFilteredActiveCollection collection, ZShort[] values = null)
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
	}
}
