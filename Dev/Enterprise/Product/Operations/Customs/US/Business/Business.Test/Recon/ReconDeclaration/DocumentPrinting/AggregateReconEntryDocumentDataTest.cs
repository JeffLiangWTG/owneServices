using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AggregateReconEntryDocumentData))]
	sealed class AggregateReconEntryDocumentDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPopulateFromEntry1And2()
		{
			ReconOriginalEntryHeader entry1 = ReconDec.OriginalEntries.AddNew();
			entry1.CH_OrigEntryReference = "MM079139194";
			entry1.US_SchDEntry = "3301";
			ReconOriginalEntryHeader entry2 = ReconDec.OriginalEntries.AddNew();
			entry2.CH_OrigEntryReference = "MM079139195";
			entry2.US_SchDEntry = "8301";
			AggregateReconEntryDocumentData aggregateReconData = new AggregateReconEntryDocumentData(entry1, entry2);
			AssertEquals("US_EntryNumber1", "MM079139194", aggregateReconData.US_EntryNumber1);
			AssertEquals("US_PortCode1", "3301", aggregateReconData.US_PortCode1);
			AssertEquals("US_EntryNumber2", "MM079139195", aggregateReconData.US_EntryNumber2);
			AssertEquals("US_PortCode2", "8301", aggregateReconData.US_PortCode2);
		}

		protected override BusinessObject GetNewBusinessObject() => new AggregateReconEntryDocumentData(ReconDec.OriginalEntries.AddNew(), null);

		ReconDeclaration reconDec;
		ReconDeclaration ReconDec => reconDec ?? (reconDec = new ReconDeclaration(Factory.New<JobDeclaration>()));
	}
}
