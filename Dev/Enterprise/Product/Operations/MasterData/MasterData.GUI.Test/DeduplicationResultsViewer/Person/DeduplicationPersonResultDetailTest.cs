using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Test
{
	[TestedType(typeof(DeduplicationPersonResultDetail))]
	public class DeduplicationPersonResultDetailTest : NonPersistentBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestDoNotLoadCandidatesForEmptyResult()
		{
			var resultDetail = new DeduplicationPersonResultDetailForTest();
			resultDetail.SetIsEmptyOrNotForTesting(isEmpty: true);

			resultDetail.LoadCandidatesAndUpdateSelectedItem();

			AssertEquals(0, resultDetail.CandidatesCount);
			AssertEquals(0, resultDetail.DuplicationCandidates.Count);
		}

		public void TestNoDuplicates()
		{
			var resultDetail = new DeduplicationPersonResultDetailForTest();
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

			var resultDetail = new DeduplicationPersonResultDetailForTest();
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

			var resultDetail = new DeduplicationPersonResultDetailForTest();
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

		[ExpectNoExceptions]
		public void TestSetSelectedCandidatePK_WithInvalidZGuid()
		{
			var resultDetail = new DeduplicationPersonResultDetailForTest();

			resultDetail.SelectedCandidatePK = ZGuid.Invalid;

			AssertEquals(ZGuid.Empty, resultDetail.SelectedCandidatePK);
		}

		DuplicationPersonCandidate CreateNewCandidate() => (DuplicationPersonCandidate)Activator.CreateInstance(typeof(DuplicationPersonCandidate), nonPublic: true);
	}

	public class DeduplicationPersonResultDetailForTest : DeduplicationPersonResultDetail
	{
		public DeduplicationPersonResultDetailForTest() : base(new DeduplicationGlbPerson(new BusinessObjectFactory().New<GlbPerson>()), Enumerable.Empty<DeduplicationGlbPerson>(), Enumerable.Empty<ScoringResult>(), Enumerable.Empty<PatternMatchingResultModel>(),
			() => { })
		{
		}

		public void SetIsEmptyOrNotForTesting(bool isEmpty) => IsEmpty = isEmpty;

		protected override IEnumerable<DuplicationPersonCandidate> GetDuplicationCandidatesCore() => DuplicationCandidatesForTest;

		public IEnumerable<DuplicationPersonCandidate> DuplicationCandidatesForTest { get; set; } = Enumerable.Empty<DuplicationPersonCandidate>();

		protected override void SetPotentialDuplicationModelInfo(DuplicationPersonCandidate item)
		{
		}
	}
}
