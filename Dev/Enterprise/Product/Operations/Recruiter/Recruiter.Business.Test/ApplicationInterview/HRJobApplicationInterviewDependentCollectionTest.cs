using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRJobApplicationInterviewDependentCollection))]
	sealed class HRJobApplicationInterviewDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			HRJobApplication parent = Factory.NewWithValidTestData<HRJobApplication>();
			return new HRJobApplicationInterviewDependentCollection(parent);
		}
	}
}
