using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusProfileQuestionPathway))]
	class RefCusProfileQuestionPathwayTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var profileType = Helper.CreateRefCusProfileType("PP0", "HSN", Core.Constants.CountryCodes.Brazil, "dummy Description 0");
			var questionParent = Helper.CreateRefCusProfileQuestion(profileType, "Parent Test 1", "ATTP1", "Parent Test 1", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			var questionChild = Helper.CreateRefCusProfileQuestion(profileType, "Child Test 1", "ATTC1", "Child Test 1", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			return Helper.CreateRefCusProfileQuestionPathway(questionParent, questionChild, "Question Pathway 1", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1), "([Answer] = 3 | [Answer] = 4 | [Answer] = 5)");
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		UniversalReferenceTestDataHelper Helper => helper ??= new UniversalReferenceTestDataHelper(Factory);
		UniversalReferenceTestDataHelper helper;
	}
}
