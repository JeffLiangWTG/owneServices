using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusEntrySnapshotCollection<CusEntrySnapshot>))]
	sealed class CusEntrySnapshotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusEntrySnapshotCollection<CusEntrySnapshot>(Entry);
		}

		CusEntryHeader Entry => entry ?? (entry = Factory.New<BaseJobDeclaration>().CustomsEntryHeaders.AddNew());
		CusEntryHeader entry;
		public void TestAddNewForMessageType()
		{
			var coll = new CusEntrySnapshotCollection<CusEntrySnapshot>(Entry);
			var snpashot = coll.AddNew("ABC");
			AssertEquals((short)1, snpashot.CES_VersionNumber);
			AssertEquals("ABC", snpashot.CES_MessageType);
			AssertEquals(AccumulativeAmendment.EntrySnapshotStatus.Current, snpashot.CES_Status);
		}

		public void TestGetLatestSnapshotIn()
		{
			var coll = new CusEntrySnapshotCollection<CusEntrySnapshot>(Entry);
			var snapshot1 = coll.AddNew("ABC");
			snapshot1.CES_Status = AccumulativeAmendment.EntrySnapshotStatus.Lodged;
			Factory.Save();

			var snapshot2 = coll.AddNew("ABC");
			snapshot2.CES_Status = AccumulativeAmendment.EntrySnapshotStatus.Lodged;
			Factory.Save();
			snapshot2.CES_SystemCreateTimeUtc = snapshot1.CES_SystemCreateTimeUtc.AddHours(1);

			var snapshot3 = coll.AddNew("ABC");
			snapshot3.CES_Status = AccumulativeAmendment.EntrySnapshotStatus.Current;
			snapshot3.CES_SystemCreateTimeUtc = snapshot2.CES_SystemCreateTimeUtc.AddHours(1);
			Factory.Save();

			var snapshot = coll.GetLatestSnapshotIn("ABC", AccumulativeAmendment.EntrySnapshotStatus.Lodged);
			AssertEquals(snapshot2, snapshot);

			snapshot = coll.GetLatestSnapshotIn("ABC");
			AssertEquals(snapshot3, snapshot);
		}

		public void TestGetFirstSnapshotIn()
		{
			var coll = new CusEntrySnapshotCollection<CusEntrySnapshot>(Entry);
			var snpashot1 = coll.AddNew("ABC");
			snpashot1.CES_Status = AccumulativeAmendment.EntrySnapshotStatus.Lodged;
			Factory.Save();
			var snpashot2 = coll.AddNew("ABC");
			snpashot2.CES_Status = AccumulativeAmendment.EntrySnapshotStatus.Lodged;
			Factory.Save();
			snpashot2.CES_SystemCreateTimeUtc = snpashot1.CES_SystemCreateTimeUtc.AddHours(1);
			var snpashot3 = coll.AddNew("ABC");
			Factory.Save();
			snpashot3.CES_SystemCreateTimeUtc = snpashot2.CES_SystemCreateTimeUtc.AddHours(1);

			var snpashot = coll.GetFirstSnapshotIn("ABC", AccumulativeAmendment.EntrySnapshotStatus.Current);
			AssertEquals((short)3, snpashot.CES_VersionNumber);
			AssertEquals("ABC", snpashot.CES_MessageType);
			AssertEquals(snpashot3, snpashot);
		}

		public void TestDoesSnapshotExist()
		{
			AssertEquals(false, Entry.Snapshots.DoesSnapshotExist("ABC"));

			var snapshot = Entry.Snapshots.AddNew("ABC");
			AssertEquals(false, Entry.Snapshots.DoesSnapshotExist("ABC"));

			snapshot.CES_Status = AccumulativeAmendment.EntrySnapshotStatus.Lodged;
			AssertEquals(true, Entry.Snapshots.DoesSnapshotExist("ABC"));
		}

		public void TestLoadCusEntrySnapshotCollection()
		{
			var coll = new CusEntrySnapshotCollection<CusEntrySnapshot>(Entry);
			var snapshot1 = coll.AddNew("ABC");
			snapshot1.CES_Status = AccumulativeAmendment.EntrySnapshotStatus.Current;
			var snapshot2 = coll.AddNew("ABC");
			snapshot2.CES_Status = AccumulativeAmendment.EntrySnapshotStatus.Lodged;
			var snapshot3 = coll.AddNew("ABC");
			snapshot3.CES_Status = AccumulativeAmendment.EntrySnapshotStatus.Deleted;
			Factory.Save();

			coll = (CusEntrySnapshotCollection<CusEntrySnapshot>)GetCollectionToTest();
			coll.Load();
			AssertEquals(2, coll.Count);
		}
	}
}
