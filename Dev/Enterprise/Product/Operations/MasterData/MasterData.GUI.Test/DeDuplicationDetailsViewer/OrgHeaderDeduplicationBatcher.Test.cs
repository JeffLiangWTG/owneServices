using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.EntityFramework.Testing;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterData.GUI.Tests
{
	class OrgHeaderDeduplicationBatcherTest : TestCaseWithFactory
	{
		public void TestScoreResultsCorrectly()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			GetTestData(out var master, out var targets, out var previousScores, 0, false);
			AssertScoreResultsCorrectly(master, targets, previousScores);

			GetTestData(out master, out targets, out previousScores, 1, false);
			AssertScoreResultsCorrectly(master, targets, previousScores);

			var batcher = new OrgHeaderDeduplicationBatcherForTest(master, targets, previousScores, Array.Empty<PatternMatchingResultModel>());
			GetTestData(out master, out targets, out previousScores, batcher.MaxScoringResultForTest * 2 + 1, true);
			AssertScoreResultsCorrectly(master, targets, previousScores);
		}

		void AssertScoreResultsCorrectly(DeduplicationOrgHeader master, List<DeduplicationOrgHeader> targets, List<ScoringResult> previousScores)
		{
			var batcher = new OrgHeaderDeduplicationBatcherForTest(master, targets, previousScores, Array.Empty<PatternMatchingResultModel>());
			var scoreResults = new List<ScoringResult>();
			scoreResults.AddRange(previousScores ?? new List<ScoringResult>());
			var loopTimes = (int)Math.Ceiling((targets.Count - (previousScores?.Count ?? 0)) * 1.0 / batcher.MaxScoringResultForTest);

			for (var times = 0; times < loopTimes; times++)
			{
				var results = batcher.ScoreResults();
				Assert("Should contain results", results.Count > 0);
				scoreResults.AddRange(results);
			}

			AssertEquals("Should not get any result", 0, batcher.ScoreResults().Count);
			AssertContainsExactElementsInAnyOrder("Should contains same targets", targets.Select(u => u.OH_PK), scoreResults.Select(u => u.TargetPK));
		}

		void GetTestData(out DeduplicationOrgHeader masterOrg, out List<DeduplicationOrgHeader> targetOrgs, out List<ScoringResult> previousScores, int targetsCount, bool hasPreviousScores)
		{
			var master = Factory.New<OrgHeader>();
			master.OH_Code = "MASTER";
			master.OH_FullName = "TOLL PTY LTD";
			master.OH_RL_NKClosestPort = "AUSYD";
			((IDeduplicatable)master).ShouldRunDeduplication = true;

			var contactMaster = master.Contacts.AddNew();
			contactMaster.OC_ContactName = "Contact T";
			contactMaster.OC_Phone = "0449743938";

			var addressMaster = master.Addresses.AddNew();
			addressMaster.Address1 = "Bnt Crescent";
			addressMaster.OA_Phone = "0449743938";

			masterOrg = new DeduplicationOrgHeader(master);

			targetOrgs = new List<DeduplicationOrgHeader>();
			for (var index = 0; index < targetsCount; index++)
			{
				var target = Factory.New<OrgHeader>();
				target.OH_Code = "TAR" + index;
				target.OH_FullName = "TOLL PTY LTD";
				target.OH_RL_NKClosestPort = "AUSYD";

				var contactTarget = target.Contacts.AddNew();
				contactTarget.OC_ContactName = "Contact T";
				contactTarget.OC_Phone = "0449743938";

				var addressTarget = target.Addresses.AddNew();
				addressTarget.Address1 = "River Drive";
				addressTarget.OA_Phone = "0449743938";

				targetOrgs.Add(new DeduplicationOrgHeader(target));
			}

			if (hasPreviousScores)
			{
				previousScores = new List<ScoringResult>
				{
					new ScoringResult()
					{
						TargetPK = targetOrgs[0].OH_PK
					}
				};
			}
			else
			{
				previousScores = null;
			}
		}

		public class OrgHeaderDeduplicationBatcherForTest : OrgHeaderDeduplicationBatcher
		{
			public OrgHeaderDeduplicationBatcherForTest(IOrgHeader header, IEnumerable<IOrgHeader> targetGlows, IEnumerable<ScoringResult> previousScores, PatternMatchingResultModel[] resultModels) : base(header as IDeduplicationGlowObject, targetGlows.Cast<IDeduplicationGlowObject>(), previousScores, resultModels)
			{
			}

			public int MaxScoringResultForTest => base.MaxScoringResult;
		}
	}
}
