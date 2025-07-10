using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusProfileQuestionAttribute))]
	class RefCusProfileQuestionAttributeTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var profileType = Helper.CreateRefCusProfileType("PP0", "HSN", Core.Constants.CountryCodes.Brazil, "dummy Description 0");
			var question = Helper.CreateRefCusProfileQuestion(profileType, "Test 1", "ATT1", "Text 1", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
			var attribute = question.Attributes.AddNew();
			attribute.XQ3_Name = "Name";
			attribute.XQ3_Value = "Value";
			return attribute;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		UniversalReferenceTestDataHelper Helper => helper ??= new UniversalReferenceTestDataHelper(Factory);
		UniversalReferenceTestDataHelper helper;
	}
}
