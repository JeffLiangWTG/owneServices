using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Test
{
	[TestedType(typeof(DeduplicationOrganisationResultDetail))]
	public class DeduplicationOrganisationResultDetailTest : NonPersistentBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestDoNotLoadCandidatesForEmptyResult()
		{
			var resultDetail = new DeduplicationOrganisationResultDetailForTest();
			resultDetail.SetIsEmptyOrNotForTesting(isEmpty: true);

			resultDetail.LoadCandidatesAndUpdateSelectedItem();

			AssertEquals(0, resultDetail.CandidatesCount);
			AssertEquals(0, resultDetail.DuplicationCandidates.Count);
		}

		public void TestNoDuplicates()
		{
			var resultDetail = new DeduplicationOrganisationResultDetailForTest();
			resultDetail.SetIsEmptyOrNotForTesting(isEmpty: false);

			resultDetail.LoadCandidatesAndUpdateSelectedItem();

			AssertEquals(0, resultDetail.CandidatesCount);
			AssertEquals(0, resultDetail.DuplicationCandidates.Count);
		}

		public void TestDefaultCandidateWhenLoadCandidates()
		{
			var candidate1PK = Guid.NewGuid();
			var candidate2PK = Guid.NewGuid();
			var candidate1 = CreateNewCandidate();
			candidate1.TargetPK = candidate1PK;
			var candidate2 = CreateNewCandidate();
			candidate2.TargetPK = candidate2PK;

			var resultDetail = new DeduplicationOrganisationResultDetailForTest();
			resultDetail.SetIsEmptyOrNotForTesting(isEmpty: false);
			resultDetail.DuplicationCandidatesForTest = new[] { candidate1, candidate2 };

			var updated = false;

			resultDetail.RegisterNotifyPropertyChangeEvent(nameof(resultDetail.SelectedCandidatePK), "UpdateForTest", UpdateForTest);

			AssertEquals(false, updated);
			AssertEquals(Guid.Empty, resultDetail.SelectedCandidatePK);

			resultDetail.LoadCandidatesAndUpdateSelectedItem();

			AssertEquals(candidate1PK, resultDetail.SelectedCandidatePK);
			AssertEquals(true, updated);

			void UpdateForTest()
			{
				updated = true;
			}
		}

		public void TestSpecificCandidateWhenLoadCandidates()
		{
			var candidate1PK = Guid.NewGuid();
			var candidate2PK = Guid.NewGuid();
			var candidate1 = CreateNewCandidate();
			candidate1.TargetPK = candidate1PK;
			var candidate2 = CreateNewCandidate();
			candidate2.TargetPK = candidate2PK;

			var resultDetail = new DeduplicationOrganisationResultDetailForTest();
			resultDetail.SetIsEmptyOrNotForTesting(isEmpty: false);
			resultDetail.DuplicationCandidatesForTest = new[] { candidate1, candidate2 };
			resultDetail.SelectedCandidatePK = candidate2PK;

			var updated = false;

			resultDetail.RegisterNotifyPropertyChangeEvent(nameof(resultDetail.SelectedCandidatePK), "UpdateForTest", UpdateForTest);

			AssertEquals(false, updated);
			AssertEquals(candidate2PK, resultDetail.SelectedCandidatePK);

			resultDetail.LoadCandidatesAndUpdateSelectedItem();

			AssertEquals(candidate2PK, resultDetail.SelectedCandidatePK);
			AssertEquals(true, updated);

			void UpdateForTest()
			{
				updated = true;
			}
		}

		public void TestShowIgnoreAllButtonWhenNoIgnoreCandidate()
		{
			var candidate1PK = Guid.NewGuid();
			var candidate2PK = Guid.NewGuid();
			var candidate1 = new DuplicationOrganisationCandidate(null, new DeduplicationOrgHeader("Test1"));
			candidate1.TargetPK = candidate1PK;
			var candidate2 = new DuplicationOrganisationCandidate(null, new DeduplicationOrgHeader("Test2"));
			candidate2.TargetPK = candidate2PK;

			var adminResultDetail = new DeduplicationOrganisationResultDetailForTest(false, false, false);
			adminResultDetail.SetIsEmptyOrNotForTesting(isEmpty: false);
			adminResultDetail.DuplicationCandidatesForTest = new[] { candidate1, candidate2 };
			adminResultDetail.SelectedCandidatePK = candidate1PK;

			var resultDetail = new DeduplicationOrganisationResultDetailForTest();
			resultDetail.SetIsEmptyOrNotForTesting(isEmpty: false);
			resultDetail.DuplicationCandidatesForTest = new[] { candidate1, candidate2 };
			resultDetail.SelectedCandidatePK = candidate1PK;

			adminResultDetail.LoadCandidatesAndUpdateSelectedItem();
			resultDetail.LoadCandidatesAndUpdateSelectedItem();

			AssertEquals(true, adminResultDetail.ShowIgnoreAllButton);
			AssertEquals(true, adminResultDetail.ShowNotMatchButton);
			AssertEquals(false, adminResultDetail.ShowRemoveAllIgnoresButton);
			AssertEquals(false, resultDetail.ShowIgnoreAllButton);
		}

		[ExpectNoExceptions]
		public void TestGetCurrentSelectedCandidateSafely()
		{
			var candidate1PK = Guid.NewGuid();
			var candidate2PK = Guid.NewGuid();
			var candidate1 = CreateNewCandidate();
			candidate1.TargetPK = candidate1PK;
			var candidate2 = CreateNewCandidate();
			candidate2.TargetPK = candidate2PK;

			var resultDetail = new DeduplicationOrganisationResultDetailForTestCurrentSelectedCandidate();
			resultDetail.DuplicationCandidates.AddRange(candidate1, candidate2);

			var pks = new [] { candidate1PK, candidate2PK };
			var index = 0;
			resultDetail.SelectedCandidatePKGetter = new Func<ZGuid>(() => pks[index++]);

			AssertSame(candidate1, resultDetail.CurrentSelectedCandidate);
		}

		DuplicationOrganisationCandidate CreateNewCandidate() => (DuplicationOrganisationCandidate)Activator.CreateInstance(typeof(DuplicationOrganisationCandidate), nonPublic: true);
	}

	public class DeduplicationOrganisationResultDetailForTest : DeduplicationOrganisationResultDetail
	{
		public DeduplicationOrganisationResultDetailForTest() : this(new DeduplicationOrgHeader("TEST"))
		{
		}

		public DeduplicationOrganisationResultDetailForTest(DeduplicationOrgHeader master)
			: base(master, Enumerable.Empty<DeduplicationOrgHeader>(), Enumerable.Empty<ScoringResult>(), Enumerable.Empty<PatternMatchingResultModel>(), () => { })
		{
		}

		public DeduplicationOrganisationResultDetailForTest(
			bool isExcludingInactiveResults,
			bool isExcludingOtherCountries,
			bool isShowIgnoredResults,
			Dictionary<ScoringResult, PatternMatchingResult> resultDictionary = null)
			: base(new DeduplicationOrgHeader("TEST"),
				Enumerable.Empty<DeduplicationOrgHeader>(),
				Enumerable.Empty<ScoringResult>(),
				Enumerable.Empty<PatternMatchingResultModel>(),
				isExcludingInactiveResults,
				isExcludingOtherCountries,
				isShowIgnoredResults,
				resultDictionary)
		{
		}

		public void SetIsEmptyOrNotForTesting(bool isEmpty) => IsEmpty = isEmpty;

		protected override IEnumerable<DuplicationOrganisationCandidate> GetDuplicationCandidatesCore() => DuplicationCandidatesForTest;

		public IEnumerable<DuplicationOrganisationCandidate> DuplicationCandidatesForTest { get; set; } = Enumerable.Empty<DuplicationOrganisationCandidate>();

		protected override void SetPotentialDuplicationModelInfo(DuplicationOrganisationCandidate item)
		{
		}

		public int FilterCallNumber;

		public override void FilterAndUpdate()
		{
			FilterCallNumber++;
			base.FilterAndUpdate();
		}
	}

	class DeduplicationOrganisationResultDetailForTestCurrentSelectedCandidate : DeduplicationOrganisationResultDetailForTest
	{
		public DeduplicationOrganisationResultDetailForTestCurrentSelectedCandidate()
			: base()
		{
		}

		public new DuplicationOrganisationCandidate CurrentSelectedCandidate => base.CurrentSelectedCandidate;

		public override ZGuid SelectedCandidatePK
		{
			get => SelectedCandidatePKGetter.Invoke();
			set => base.SelectedCandidatePK = value;
		}

		public Func<ZGuid> SelectedCandidatePKGetter { get; set; }
	}
}
