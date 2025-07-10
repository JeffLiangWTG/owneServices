using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Recruiter.Business;
using Enterprise.Recruitment.Common;
using Enterprise.Recruitment.Module.CandidateManagement;
using NUnit.Framework;

namespace Enterprise.Recruitment.Testing.Module
{
	[TestedType(typeof(CandidateBusinessObjectCollection))]
	sealed class CandidateBusinessObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CandidateBusinessObjectCollection>
	{
		protected override CandidateBusinessObjectCollection GetCollectionToTest() => new CandidateBusinessObjectCollection(Factory);
		protected override BusinessObject GetNewElementToAddToTheCollection() => new Candidate(Factory, Factory.NewWithValidTestData<HRJobApplication>().PK);
	}
}
