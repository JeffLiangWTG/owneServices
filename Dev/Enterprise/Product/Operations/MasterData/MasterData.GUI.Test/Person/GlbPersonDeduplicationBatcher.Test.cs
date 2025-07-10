using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.MasterData.GUI.Tests
{
	class GlbPersonDeduplicationBatcherTest : TestCaseWithFactory
	{
		public void TestScoreResultsCorrectly()
		{
			SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GetTestData(out var master, out var targets, out var previousScores, 0, false);
			AssertScoreResultsCorrectly(master, targets, previousScores);

			GetTestData(out master, out targets, out previousScores, 1, false);
			AssertScoreResultsCorrectly(master, targets, previousScores);

			var batcher = new GlbPersonDeduplicationBatcherForTest(master, targets, previousScores, Array.Empty<PatternMatchingResultModel>());
			GetTestData(out master, out targets, out previousScores, batcher.MaxScoringResultForTest * 2 + 1, true);
			AssertScoreResultsCorrectly(master, targets, previousScores);
		}

		void AssertScoreResultsCorrectly(DeduplicationGlbPerson master, List<DeduplicationGlbPerson> targets, List<ScoringResult> previousScores)
		{
			var batcher = new GlbPersonDeduplicationBatcherForTest(master, targets, previousScores, Array.Empty<PatternMatchingResultModel>());
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
			AssertContainsExactElementsInAnyOrder("Should contains same targets", targets.Select(u => u.PER_PK), scoreResults.Select(u => u.TargetPK));
		}

		void GetTestData(out DeduplicationGlbPerson masterPerson, out List<DeduplicationGlbPerson> targetPersons, out List<ScoringResult> previousScores, int targetsCount, bool hasPreviousScores)
		{
			var person = Factory.New<GlbPerson>();
			person.PER_FullName = "Peter Dwyer";
			person.PER_HomeAddress1 = "Bnt Crescent";
			person.PER_MobilePhone = "0449743938";
			((IDeduplicatable)person).ShouldRunDeduplication = true;

			var contactMaster = person.ContactCollection.AddNew();
			var orgHeaderMaster = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeaderTarget = Factory.NewWithValidTestData<OrgHeader>();
			contactMaster.OC_ContactName = "Peter Dwyer";
			contactMaster.OC_Phone = "0449743938";
			contactMaster.OC_OH = orgHeaderMaster.PK;

			masterPerson = (DeduplicationGlbPerson)person.CreateIGlbPerson();

			targetPersons = new List<DeduplicationGlbPerson>();
			for (var index = 0; index < targetsCount; index++)
			{
				var target = Factory.New<GlbPerson>();
				target.PER_FullName = "Peter Dwyer" + index;
				target.Address1 = "River Drive";
				target.PER_MobilePhone = "0449743938";

				var contactTarget = target.ContactCollection.AddNew();
				contactTarget.OC_OH = orgHeaderTarget.PK;
				contactTarget.OC_ContactName = "Peter Dwyer" + index;
				contactTarget.OC_Phone = "0449743938";

				targetPersons.Add((DeduplicationGlbPerson)target.CreateIGlbPerson());
			}

			if (hasPreviousScores)
			{
				previousScores = new List<ScoringResult>
				{
					new ScoringResult()
					{
						TargetPK = targetPersons[0].PER_PK
					}
				};
			}
			else
			{
				previousScores = null;
			}
		}

		public class GlbPersonDeduplicationBatcherForTest : GlbPersonDeduplicationBatcher
		{
			public GlbPersonDeduplicationBatcherForTest(IGlbPerson header, IEnumerable<IGlbPerson> targetGlows, IEnumerable<ScoringResult> previousScores, PatternMatchingResultModel[] resultModels) : base(header as IDeduplicationGlowObject, targetGlows.Cast<IDeduplicationGlowObject>(), previousScores, resultModels)
			{
			}

			public int MaxScoringResultForTest => base.MaxScoringResult;
		}
	}
}
