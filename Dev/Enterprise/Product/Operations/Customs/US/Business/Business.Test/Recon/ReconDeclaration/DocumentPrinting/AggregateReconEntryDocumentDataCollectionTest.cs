using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AggregateReconEntryDocumentDataCollection))]
	sealed class AggregateReconEntryDocumentDataCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AggregateReconEntryDocumentDataCollection>
	{
		/// <summary>
		/// Documents are displayed like the following
		/// 
		/// Entry Num		Port Code					Entry Num		Port Code
		/// 
		/// MM079231650			3301					MM079231657			7584
		/// MM079231653			2709					MM079231659			1234
		/// MM079231655			9087
		/// </summary>
		public void TestPopulateCollection()
		{
			ReconDeclaration reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			ReconOriginalEntryHeader entry1 = reconDec.OriginalEntries.AddNew();
			entry1.CH_OrigEntryReference = "MM079231659";
			ReconOriginalEntryHeader entry2 = reconDec.OriginalEntries.AddNew();
			entry2.CH_OrigEntryReference = "MM079231657";
			ReconOriginalEntryHeader entry3 = reconDec.OriginalEntries.AddNew();
			entry3.CH_OrigEntryReference = "MM079231653";
			ReconOriginalEntryHeader entry4 = reconDec.OriginalEntries.AddNew();
			entry4.CH_OrigEntryReference = "MM079231655";
			ReconOriginalEntryHeader entry5 = reconDec.OriginalEntries.AddNew();
			entry5.CH_OrigEntryReference = "MM079231650";
			AggregateReconEntryDocumentDataCollection coll = new AggregateReconEntryDocumentDataCollection(reconDec);
			AssertEquals("There should be 3 elements", 3, coll.Count);
			AssertEquals("First element:EntryNumber1", "MM079231650", coll[0].US_EntryNumber1);
			AssertEquals("First element:EntryNumber2", "MM079231657", coll[0].US_EntryNumber2);
			AssertEquals("Second element:EntryNumber1", "MM079231653", coll[1].US_EntryNumber1);
			AssertEquals("Second element:EntryNumber2", "MM079231659", coll[1].US_EntryNumber2);
			AssertEquals("Third element:EntryNumber1", "MM079231655", coll[2].US_EntryNumber1);
			AssertEquals("Third element:EntryNumber2", "", coll[2].US_EntryNumber2);
			reconDec.OriginalEntries.RemoveAndDelete(entry5);
			coll = new AggregateReconEntryDocumentDataCollection(reconDec);
			AssertEquals("There should be 3 elements", 2, coll.Count);
			AssertEquals("First element:EntryNumber1", "MM079231653", coll[0].US_EntryNumber1);
			AssertEquals("First element:EntryNumber2", "MM079231657", coll[0].US_EntryNumber2);
			AssertEquals("Second element:EntryNumber1", "MM079231655", coll[1].US_EntryNumber1);
			AssertEquals("Second element:EntryNumber2", "MM079231659", coll[1].US_EntryNumber2);
		}

		public void TestAllowNewAndAllowRemove()
		{
			AggregateReconEntryDocumentDataCollection coll = new AggregateReconEntryDocumentDataCollection(ReconDec);
			AssertEquals(false, coll.AllowNew);
			AssertEquals(false, coll.AllowRemove);
		}

		protected override AggregateReconEntryDocumentDataCollection GetCollectionToTest() => new AggregateReconEntryDocumentDataCollection(ReconDec);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new AggregateReconEntryDocumentData(ReconDec.OriginalEntries.AddNew(), null);

		ReconDeclaration reconDec;
		ReconDeclaration ReconDec => reconDec ?? (reconDec = new ReconDeclaration(Factory.New<JobDeclaration>()));
	}
}
