using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class AccumulativeAmendmentManagerTest : TestCaseWithFactory
	{
		public void TestCreateNewSnapshot()
		{
			var entry = Factory.New<CusEntryHeader>();
			entry.CH_MessageType = messageType;

			var query = new ZQuery(CusEntrySnapshotSchema.CES_MessageType, messageType);
			AssertEquals("Pre-Condition: no snapshot exists", 0, Factory.Load<CusEntrySnapshot>(query).Length);

			using (var stream = new CargoWise.IO.Shim.SubStreamableStream())
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, messageType, stream);
				AssertEquals("1 snapshot exists", 1, Factory.Load<CusEntrySnapshot>(query).Length);
			}
		}

		public void TestAcceptCurrentSnapshot()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var snapshot1 = entry1.Snapshots.AddNew(messageType);
			var snapshot2 = entry1.Snapshots.AddNew("BBB");
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var snapshot3 = entry2.Snapshots.AddNew(messageType);
			var snapshot4 = entry2.Snapshots.AddNew("BBB");
			Factory.Save();

			AssertEquals("Pre-Condition: Snapshot1 Status", EntrySnapshotStatus.Current, snapshot1.CES_Status);
			AssertEquals("Pre-Condition: Snapshot2 Status", EntrySnapshotStatus.Current, snapshot2.CES_Status);
			AssertEquals("Pre-Condition: Snapshot3 Status", EntrySnapshotStatus.Current, snapshot3.CES_Status);
			AssertEquals("Pre-Condition: Snapshot4 Status", EntrySnapshotStatus.Current, snapshot4.CES_Status);

			AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry1, messageType);
			AssertEquals("Snapshot1 Status", EntrySnapshotStatus.Lodged, snapshot1.CES_Status);
			AssertEquals("Snapshot2 Status", EntrySnapshotStatus.Current, snapshot2.CES_Status);
			AssertEquals("Snapshot3 Status", EntrySnapshotStatus.Current, snapshot3.CES_Status);
			AssertEquals("Snapshot4 Status", EntrySnapshotStatus.Current, snapshot4.CES_Status);
		}

		public void TestRejectCurrentSnapshot()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var snapshot1 = entry1.Snapshots.AddNew(messageType);
			var snapshot2 = entry1.Snapshots.AddNew("BBB");
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var snapshot3 = entry2.Snapshots.AddNew(messageType);
			var snapshot4 = entry2.Snapshots.AddNew("BBB");
			Factory.Save();

			var queryA = new ZQuery(CusEntrySnapshotSchema.CES_MessageType, messageType);
			var queryB = new ZQuery(CusEntrySnapshotSchema.CES_MessageType, "BBB");

			AssertEquals("Pre-Condition: 2 [AAA] snapshots exist", 2, Factory.Load<CusEntrySnapshot>(queryA).Length);
			AssertEquals("Pre-Condition: 2 [BBB] snapshots exist", 2, Factory.Load<CusEntrySnapshot>(queryB).Length);

			AccumulativeAmendmentManager.DeleteCurrentSnapshot(entry1, messageType);
			var snapshots = Factory.Load<CusEntrySnapshot>(queryA);
			AssertEquals("1 [AAA] snapshot for entry1 has been deleted", 1, snapshots.Length);
			AssertEquals(entry2.PK, snapshots.First().CES_CH_EntryHeader);
			AssertEquals("2 [BBB] snapshots still exist", 2, Factory.Load<CusEntrySnapshot>(queryB).Length);
		}

		public void TestDeleteLatestLodgedSnapshot()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var snapshot = entry.Snapshots.AddNew("AAA");
			AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, "AAA");
			AssertEquals("Pre-Condition: 1 [AAA] snapshots exist", 1, Factory.Load<CusEntrySnapshot>(new ZQuery()).Length);

			AccumulativeAmendmentManager.DeleteLatestLodgedSnapshot(entry, "AAA");
			AssertEquals("[AAA] snapshot deleted", 0, Factory.Load<CusEntrySnapshot>(new ZQuery()).Length);
			Assert(snapshot.IsDeleted);
		}

		public void TestMarkLodgedSnapshotAsDeleted()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var snapshot1 = entry1.Snapshots.AddNew(messageType);
			AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry1, messageType);
			var snapshot2 = entry1.Snapshots.AddNew("BBB");
			AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry1, "BBB");
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var snapshot3 = entry2.Snapshots.AddNew(messageType);
			AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry2, messageType);
			var snapshot4 = entry2.Snapshots.AddNew("BBB");
			AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry2, "BBB");
			Factory.Save();

			var queryA = new ZQuery(CusEntrySnapshotSchema.CES_MessageType, messageType);
			var queryB = new ZQuery(CusEntrySnapshotSchema.CES_MessageType, "BBB");
			var queryC = new ZQuery(CusEntrySnapshotSchema.CES_Status, EntrySnapshotStatus.Deleted);

			AssertEquals("Pre-Condition: 2 [AAA] snapshots exist", 2, Factory.Load<CusEntrySnapshot>(queryA).Length);
			AssertEquals("Pre-Condition: 2 [BBB] snapshots exist", 2, Factory.Load<CusEntrySnapshot>(queryB).Length);
			AssertEquals("Pre-Condition: 0 [AAA, BBB] CES_Status is DEL.", 0, Factory.Load<CusEntrySnapshot>(queryC).Length);

			AccumulativeAmendmentManager.MarkLodgedSnapshotAsDeleted(entry1, messageType);
			AssertEquals("2 [AAA] snapshots still exist", 2, Factory.Load<CusEntrySnapshot>(queryA).Length);
			AssertEquals("2 [BBB] snapshots still exist", 2, Factory.Load<CusEntrySnapshot>(queryB).Length);
			AssertCollectionNotContains(snapshot1, entry1.Snapshots);

			var deleteSnapshot = Factory.Load<CusEntrySnapshot>(queryC);
			AssertEquals(1, deleteSnapshot.Length);
			AssertEquals(entry1.PK, deleteSnapshot[0].CES_CH_EntryHeader);
			AssertEquals(messageType, deleteSnapshot[0].CES_MessageType);
		}

		const string messageType = "AAA";
	}
}
