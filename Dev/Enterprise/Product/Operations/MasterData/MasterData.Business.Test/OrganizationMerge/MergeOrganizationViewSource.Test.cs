using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.Business.Tests
{
	public class MergeOrganizationViewSourceTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var orgMaster = new DeduplicationOrgHeader(Factory.New<OrgHeader>());
			var orgTargets = new[] { new DeduplicationOrgHeader(Factory.New<OrgHeader>()) };
			var scoreResults = new[] { TargetScorerController.Score(orgMaster, orgTargets[0], false) };
			var resultModels = new[]
			{
				new PatternMatchingResultModel
				{
					OrgPK = orgTargets[0].OH_PK,
					ParentID = orgTargets[0].OH_PK
				}
			};

			var viewSource = new MergeOrganizationViewSource(orgMaster, orgTargets, scoreResults, resultModels, orgTargets[0].OH_PK);

			CombineAssertions(() =>
			{
				AssertEquals(orgMaster, viewSource.Master);
				AssertEquals(orgTargets, viewSource.Target);
				AssertEquals(scoreResults, viewSource.Results);
				AssertEquals(resultModels, viewSource.ResultModels);
				AssertEquals(orgTargets[0].OH_PK, viewSource.SelectedItemPK);
			});
		}
	}
}
