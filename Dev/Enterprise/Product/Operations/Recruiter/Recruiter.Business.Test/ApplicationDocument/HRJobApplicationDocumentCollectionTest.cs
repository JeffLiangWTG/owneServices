
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(HRJobApplicationDocumentCollection))]
	sealed class HRJobApplicationDocumentCollectionTest : ActiveBusinessObjectCollectionTestCase<HRJobApplicationDocumentCollection>
	{
		protected override HRJobApplicationDocumentCollection GetCollectionToTest()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			var application = applicant.Applications.AddNew();
			return new HRJobApplicationDocumentCollection(application);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<HRJobApplicationDocument>();
		}
	}
}
