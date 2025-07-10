using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class CusEntryHeaderMessageStatusSubsetCollectionTest<TCollection> : SubsetBusinessObjectCollectionTestCase<TCollection, CusEntryHeader> where TCollection : CusEntryHeaderMessageStatusSubsetCollection
	{
		public void TestIsThisPartOfTheCollection()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			CusEntryHeader entryOriginal = testDec.CustomsEntryHeaders.AddNew();
			entryOriginal.CH_Status = "O";

			CusEntryHeader entryAmendment = testDec.CustomsEntryHeaders.AddNew();
			entryAmendment.CH_Status = "A";

			CusEntryHeader entryWithdraw = testDec.CustomsEntryHeaders.AddNew();
			entryWithdraw.CH_Status = "W";

			DummyEntryHeaderMessageStatusCollection testCollection = new DummyEntryHeaderMessageStatusCollection(testDec.ActiveEntryHeaders);
			AssertEquals("Initially this collection has all", 3, testCollection.Count);

			testCollection.MessageStatusFilter = EntryMessageStatusFilterType.CanSendOriginal;
			AssertEquals("There should be only one item", 1, testCollection.Count);
			AssertEquals("Entry Original should be there", entryOriginal, testCollection[0]);

			testCollection.MessageStatusFilter = EntryMessageStatusFilterType.CanSendAmendment;
			AssertEquals("There should be only one item", 1, testCollection.Count);
			AssertEquals("Entry Amendment should be there", entryAmendment, testCollection[0]);

			testCollection.MessageStatusFilter = EntryMessageStatusFilterType.All;
			AssertEquals("There should be all entryheaders", 3, testCollection.Count);
		}

		public class DummyEntryHeaderMessageStatusCollection : CusEntryHeaderMessageStatusSubsetCollection
		{
			public DummyEntryHeaderMessageStatusCollection(ActiveCusEntryHeaderCollection allEntryHeaders) : base(allEntryHeaders)
			{
			}

			protected override bool CanSendAmendmentForThisEntry(CusEntryHeader entryHeader)
			{
				return entryHeader.CH_Status == "A";
			}

			protected override bool CanSendOriginalForThisEntry(CusEntryHeader entryHeader)
			{
				return entryHeader.CH_Status == "O";
			}

			protected override bool CanSendWithdrawForThisEntry(CusEntryHeader entryHeader)
			{
				return entryHeader.CH_Status == "W";
			}
		}
	}
}
