using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing;

public abstract class AmendmentSnapshotManagerAbstractTest<TManager> : TestCaseWithFactory
	where TManager : AmendmentSnapshotManager
{
	[TestDate(2024, 5, 31)]
	public void TestCreateNew()
	{
		AssertEquals("Prerequisite: No snapshots exist.", 0, Factory.Load<CusEntrySnapshot>(new ZQuery()).Length);

		var entry = GetEntryHeaderForTesting();
		var manager = entry.GetNewAmendmentSnapshotManager();
		manager.CreateNewAndAccept();
		Factory.Save();
		CombineAssertions(() =>
		{
			var snapShots = Factory.Load<CusEntrySnapshot>(new ZQuery());
			AssertEquals("Snapshot should be created.", 1, snapShots.Length);
			var snapShot = snapShots[0];
			AssertEquals("CES_Status: ", EntrySnapshotStatus.Lodged, snapShot.CES_Status);
			AssertEquals("CES_MessageType: ", ExpectedMessageType, snapShot.CES_MessageType);
			AssertEquals("CES_CH_EntryHeader: ", entry.PK, snapShot.CES_CH_EntryHeader);
			AssertEquals("CES_VersionNumber: ", ExpectedVersionNumber, snapShot.CES_VersionNumber);
			this.AssertXMLEqualsByDiff("CES_SnapshotXml: ", ExpectedSnapshotXml, snapShot.CES_SnapshotXml, XmlDiffEquals.XmlCompareOptions.IgnoreWhitespace | XmlDiffEquals.XmlCompareOptions.IgnoreChildOrder);
		});
	}

	public void TestDeleteLatestLodged()
	{
		var entry = GetEntryHeaderForTesting();
		var manager = entry.GetNewAmendmentSnapshotManager();
		manager.CreateNewAndAccept();
		AssertEquals("Snapshot should be created.", 1, Factory.Load<CusEntrySnapshot>(new ZQuery()).Length);

		manager.DeleteLatestLodged();
		AssertEquals("Snapshot should be deleted.", 0, Factory.Load<CusEntrySnapshot>(new ZQuery()).Length);
	}

	public void TestHasLodgedSnapshot()
	{
		var entry = GetEntryHeaderForTesting();
		var manager = entry.GetNewAmendmentSnapshotManager();
		Assert("Should be false as no snapshots created ever.", !manager.HasLodgedSnapshot);

		manager.CreateNewAndAccept();
		Factory.Save();
		Assert("Should be true as a snapshot has been created and accepted.", manager.HasLodgedSnapshot);
	}

	public abstract void TestRevertToLastLodged_EntryHeaderSnapshotConflictPolicyIsOverride();

	public abstract void TestRevertToLastLodged_EntryHeaderSnapshotConflictPolicyIsSkip();

	protected abstract ZInt ExpectedVersionNumber { get; }

	protected abstract ZString ExpectedSnapshotXml { get; }

	protected abstract ZString ExpectedMessageType { get; }

	protected abstract CusEntryHeader GetEntryHeaderForTesting();
}
