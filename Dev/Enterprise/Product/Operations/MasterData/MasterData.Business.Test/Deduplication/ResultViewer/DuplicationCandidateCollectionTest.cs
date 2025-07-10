using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Test
{
	public abstract class DuplicationCandidateCollectionTest<TCandidate> : NonPersistentBusinessObjectCollectionTestCase<DuplicationCandidateCollection<TCandidate>>
		where TCandidate : DuplicationCandidate
	{
		public void TestAllowNew()
		{
			var result = new DuplicationCandidateCollection<TCandidate>();
			AssertEquals(false, result.AllowNew);
		}

		protected override DuplicationCandidateCollection<TCandidate> GetCollectionToTest()
		{
			return new DuplicationCandidateCollection<TCandidate>();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return (TCandidate)Activator.CreateInstance(typeof(TCandidate), nonPublic: true);
		}
	}

	[TestedType(typeof(DuplicationCandidateCollection<DuplicationOrganisationCandidate>))]
	public class DuplicationOrganisationCandidateCollectionTest : DuplicationCandidateCollectionTest<DuplicationOrganisationCandidate>
	{
	}

	[TestedType(typeof(DuplicationCandidateCollection<DuplicationPersonCandidate>))]
	public class DuplicationPersonCandidateCollectionTest : DuplicationCandidateCollectionTest<DuplicationPersonCandidate>
	{
	}
}
