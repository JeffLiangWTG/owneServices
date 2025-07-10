using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(CandidateStmNoteCollectionWithRelatedElements))]
	class CandidateStmNoteCollectionWithRelatedElementsTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = Factory.NewWithValidTestData<HRJobApplication>();
			return new CandidateStmNoteCollectionWithRelatedElements(parent);
		}
	}
}
