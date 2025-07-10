using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(FixedMultipleChoiceSingleAnswerCapturer.MultipleChoiceOptionBizO))]
	sealed class MultipleChoiceOptionBizOTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var answer = Factory.NewWithValidTestData<VoteExamSurveyAnswer>();
			var answerCapturer = new FixedMultipleChoiceSingleAnswerCapturer(answer);
			return new FixedMultipleChoiceSingleAnswerCapturer.MultipleChoiceOptionBizO(answerCapturer, "yes");
		}
	}
}
