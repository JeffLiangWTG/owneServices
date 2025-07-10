using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business.Tests
{
	public class MergeOrganizationUtilsTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var utils = new MergeOrganizationUtilsForTest();
			var participant = DeduplicationUtils.DebuggerHubInstance.FindParticipant("MergeOrganization");
			AssertEquals(utils.DebuggerParticipantExposed, participant);
		}

		public void TestGetViewSource()
		{
			var orgMaster = Factory.New<OrgHeader>();
			orgMaster.OH_FullName = "A Org With Name";
			orgMaster.MainAddress.Address1 = "Sydney NSW 2052";

			var orgTarget = Factory.New<OrgHeader>();
			orgTarget.OH_FullName = "Something else";
			orgTarget.MainAddress.Address1 = "Palm Beach NSW 2108";

			var utils = new MergeOrganizationUtils();
			var viewSource = utils.GetViewSource(orgMaster, orgTarget);

			CombineAssertions(() =>
			{
				AssertEquals(orgMaster.PK.ToGuid(), viewSource.Master.PK);
				AssertEquals(orgMaster.OH_FullName, viewSource.Master.OH_FullName);

				AssertEquals(1, viewSource.Target.Count());
				AssertEquals(orgTarget.PK.ToGuid(), viewSource.Target.First().PK);
				AssertEquals(orgTarget.OH_FullName, viewSource.Target.First().OH_FullName);

				AssertEquals(orgTarget.PK, viewSource.SelectedItemPK);

				AssertEquals(1, viewSource.Results.Count());
				AssertEquals(orgMaster.PK, viewSource.Results.First().MasterPK);
				AssertEquals(orgTarget.PK, viewSource.Results.First().TargetPK);

				AssertEquals(1, viewSource.ResultModels.Count());
				AssertEquals(orgTarget.PK, viewSource.ResultModels.First().OrgPK);
				AssertEquals(orgTarget.PK, viewSource.ResultModels.First().ParentID);
			});
		}

		class MergeOrganizationUtilsForTest : MergeOrganizationUtils
		{
			public IDeduplicationDebuggerParticipant DebuggerParticipantExposed => DebuggerParticipant;
		}
	}
}
