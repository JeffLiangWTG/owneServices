using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusProfileQuestionAnswerList))]
	class RefCusProfileQuestionAnswerListTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var profileType = Helper.CreateRefCusProfileType("PP0", "HSN", Core.Constants.CountryCodes.Brazil, "dummy Description 0");
			var question = Helper.CreateRefCusProfileQuestion(profileType, "Test 1", "ATT1", "Text 1", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			var answer = question.Answers.AddNew();
			answer.XQ4_Value = "Value";
			answer.XQ4_Description = "Description";
			return answer;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		UniversalReferenceTestDataHelper Helper => helper ??= new UniversalReferenceTestDataHelper(Factory);
		UniversalReferenceTestDataHelper helper;
	}
}
