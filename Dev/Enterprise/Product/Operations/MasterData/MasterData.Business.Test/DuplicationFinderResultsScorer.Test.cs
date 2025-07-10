using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.MasterData.Business.Tests
{
	public class DuplicationFinderResultsScorerTest : TestCaseWithFactory
	{
		public void TestScoreResultsReturnsProcessedRecordsAfterTimeout()
		{
			var regEnableDeduplicationFinder = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;
			var regDeduplicationMinimumConfidenceResult = OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.Value;
			try
			{
				//setup
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeDuplicationMinimumConfidenceRating.Codes.Undefined);

				//precondition
				AssertEquals(DeDuplicationMinimumConfidenceRating.Codes.Undefined, OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.Value);
				var setupData = new TestSetup(Factory);

				//act
				setupData.TimeoutInSeconds = 3;
				setupData.DelayInMillis = 2500;
				var duplicates = ScoreResultsForTesting(setupData);

				//assert
				AssertEquals(true, duplicates.Any());
				AssertLessThan(duplicates.Count(), 100);
			}
			finally
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regEnableDeduplicationFinder);
				OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regDeduplicationMinimumConfidenceResult);
			}
		}

		public void TestScoreResultsReturnsProcessedRecordsWithoutTimeout()
		{
			var regEnableDeduplicationFinder = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;
			var regDeduplicationMinimumConfidenceResult = OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.Value;
			try
			{
				//setup
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeDuplicationMinimumConfidenceRating.Codes.Undefined);

				//precondition
				AssertEquals(DeDuplicationMinimumConfidenceRating.Codes.Undefined, OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.Value);
				var setupData = new TestSetup(Factory);

				//act
				setupData.TimeoutInSeconds = 2;
				setupData.DelayInMillis = 0;
				var duplicates = ScoreResultsForTesting(setupData);

				//assert
				AssertEquals(100, duplicates.Count());
			}
			finally
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regEnableDeduplicationFinder);
				OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regDeduplicationMinimumConfidenceResult);
			}
		}

		public void TestScoreResultsStopsScoringWhenToldTo()
		{
			var regEnableDeduplicationFinder = OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.Value;
			var regDeduplicationMinimumConfidenceResult = OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.Value;
			try
			{
				//setup
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DeDuplicationMinimumConfidenceRating.Codes.Undefined);

				//precondition
				AssertEquals(DeDuplicationMinimumConfidenceRating.Codes.Undefined, OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.Value);
				var setupData = new TestSetup(Factory);
				AssertEquals(false, setupData.ShouldStopProcessing);

				//act
				setupData.TimeoutInSeconds = 2;
				setupData.DelayInMillis = 0;
				var duplicates = ScoreResultsForTesting(setupData);
				setupData.ShouldStopProcessing = true;

				//assert
				AssertEquals(true, setupData.ShouldStopProcessing);
			}
			finally
			{
				OrganisationsDataRegistry.Instance.EnableDeduplicationFinder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regEnableDeduplicationFinder);
				OrganisationsDataRegistry.Instance.DeduplicationMinimumConfidenceResult.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regDeduplicationMinimumConfidenceResult);
			}
		}

		public void TestScoreResultsExcludeEdgeScoreResults()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var targetHeader = Factory.New<OrgHeader>();
			var resultsScorer = new DuplicationFinderResultsScorer<IOrgHeader>(false, new DeduplicationOrgHeader(orgHeader), 200, new DeduplicationDebuggerParticipant(), new TestISupportDuplicationFinder());
			resultsScorer.SetExcludeScoreThresholdReader(() => 0.5);
			var results = resultsScorer.ScoringResults(
				new List<IOrgHeader> { new DeduplicationOrgHeader(targetHeader) },
				new List<PatternMatchingResultModel>(),
				u => u.OH_PK, u => u.OrgPK,
				u => (u.OH_Code, u.OH_FullName),
				(u, v) => new ScoringResult()
				{
					Score = 0.5
				},
				CancellationToken.None);

			AssertEquals("Should exclude the score equals 0.5 result", 0, results.Count());
		}

		#region Implementation

		IEnumerable<ScoringResult> ScoreResultsForTesting(TestSetup setupData)
		{
			var timeoutExecution = new DeduplicationTimeout<IEnumerable<ScoringResult>>(TimeSpan.FromSeconds(setupData.TimeoutInSeconds));
			var scoredResults = new List<ScoringResult>();
			try
			{
				scoredResults = timeoutExecution.DoWork(setupData.FindDuplications).ToList();
			}
			catch (TimeoutException)
			{
				setupData.ShouldStopProcessing = true;
				scoredResults = setupData.ScoringResults;
			}
			finally
			{
				AsyncHelper.WaitAllActiveTasksForTest();
			}

			return scoredResults;
		}

		#endregion
	}

	class TestSetup : ISupportDuplicationFinder
	{
		readonly DeduplicationOrgHeader[] TargetGlows;

		public TestSetup(BusinessObjectFactory factory)
		{
			ScoringResults = new List<ScoringResult>();
			Master = factory.NewWithValidTestData<OrgHeader>();
			Master.OH_Code = "ABX";
			Master.OH_FullName = "TOLL PTY LTD";
			Master.OH_RL_NKClosestPort = "AUSYD";

			var contact1 = Master.Contacts.AddNew();
			contact1.OC_ContactName = "Contact A";
			contact1.OC_Phone = "0449743938";

			var targets = new OrgHeader[100];

			for (var i = 0; i < targets.Length; i++)
			{
				targets[i] = factory.NewWithValidTestData<OrgHeader>();
				targets[i].OH_Code = $"ABW{i}";
				targets[i].OH_FullName = "TOLL PTY";
				targets[i].OH_RL_NKClosestPort = "AUSYD";
			}

			var contact2 = targets[0].Contacts.AddNew();
			contact2.OC_ContactName = "Contact AA";
			contact2.OC_Phone = "0449743938";

			factory.Save();

			PatternMatchingResultModels = new List<PatternMatchingResultModel>
			{
				new PatternMatchingResultModel
				{
					CountryCode = "AU",
					HashedValue = -383906410,
					OrgPK = targets[0].PK.ToGuid(),
					ParentID = contact2.PK.ToGuid(),
					ParentTablePrefix = "OC"
				}
			};

			var debuggerParticipant = new DeduplicationDebuggerParticipant();
			TargetGlows = new DeduplicationOrgHeader[100];

			for (var i = 0; i < targets.Length; i++)
			{
				TargetGlows[i] = new DeduplicationOrgHeader(targets[i]);
			}

			ResultsScorer = new DuplicationFinderResultsScorer<IOrgHeader>(false, new DeduplicationOrgHeader(Master), 200, debuggerParticipant, this);
		}

		public IEnumerable<ScoringResult> FindDuplications()
		{
			ResultsScorer.ScoringResults(TargetGlows, PatternMatchingResultModels, GetGlowPK, GetPatternMatchingResultModelParentPK, GenerateCacheSubKey, ScoreGlowModelWithDelay, CancellationToken.None);

			return ScoringResults;
		}

		public Guid GetPatternMatchingResultModelParentPK(PatternMatchingResultModel patternMatchingResultModel)
		{
			return patternMatchingResultModel.OrgPK;
		}

		public Guid GetGlowPK(IOrgHeader header)
		{
			return header.OH_PK;
		}

		public (string Part1, string Part2) GenerateCacheSubKey(IOrgHeader header)
		{
			return (header.OH_FullName, header.ClosestPort.RL_RN_NKCountryCode);
		}

		public ScoringResult ScoreGlowModelWithDelay(IOrgHeader master, IOrgHeader target)
		{
			Thread.Sleep(DelayInMillis);
			return new ScoringResult
			{
				Score = 0.01
			};
		}

		public Task FindDuplicates()
		{
			return Task.CompletedTask;
		}

		public void RequestToCancel()
		{
		}

		public List<ScoringResult> ScoringResults { get; }

		DuplicationFinderResultsScorer<IOrgHeader> ResultsScorer { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		public int TimeoutInSeconds { get; set; }
		public int DelayInMillis { get; set; }

		public OrgHeader Master { get; }

		public List<PatternMatchingResultModel> PatternMatchingResultModels { get; }

		public Type TargetType => typeof(OrgHeader);

		public bool IsProxied { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		public bool ShouldStopProcessing { get; set; }
		DuplicationStatus ISupportDuplicationFinder.LastRunStatus { get; set; }
	}

	class TestISupportDuplicationFinder : ISupportDuplicationFinder
	{
		public Type TargetType => null;

		public bool ShouldStopProcessing { get; set; }

		public List<ScoringResult> ScoringResults { get; } = new List<ScoringResult>();

		public DuplicationStatus LastRunStatus { get; set; }

		public Task FindDuplicates()
		{
			return Task.CompletedTask;
		}

		public void RequestToCancel()
		{
		}
	}
}
