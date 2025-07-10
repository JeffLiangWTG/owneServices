using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(CandidateStmNoteDependentCollection))]
	class CandidateNoteDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = Factory.NewWithValidTestData<HRJobApplication>();
			return new CandidateStmNoteDependentCollection(parent, Factory);
		}
	}
}
