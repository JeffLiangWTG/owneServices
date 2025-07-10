using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Recruiter.Business.LearningCentreQuestionCollection;

namespace Enterprise.Recruiter.Business.Testing
{
	[TestedType(typeof(LearningCentreQuestionCollectionForImport))]
	sealed class LearningCentreQuestionCollectionForImportTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var campaign = Factory.NewWithValidTestData<LearningCentreCampaign>();
			return new LearningCentreQuestionCollectionForImport(campaign);
		}
	}
}
