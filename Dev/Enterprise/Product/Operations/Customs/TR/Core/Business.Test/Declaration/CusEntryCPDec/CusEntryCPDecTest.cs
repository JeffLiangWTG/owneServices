using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryCPDec))]
	class CusEntryCPDecTest : EnterpriseBusinessObjectTestCase
	{
		public void TestQuestion()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Turkey);
			helper.CreateCusCodeType("TRCUQ", "TurkeyCustomsQuestion", Core.Constants.CountryCodes.Turkey);
			helper.CreateCusCodeType("TRCUW", "TurkeyCustomsWarning", Core.Constants.CountryCodes.Turkey);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "TRCUQ", "111", "Question 111", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Turkey, "TRCUW", "222", "Warning 222", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var cpDec1 = Factory.New<CusEntryCPDec>();
			cpDec1.ON_QuestionType = "Q";
			cpDec1.ON_CPDecNum = 111;
			AssertEquals("Question 111", cpDec1.Description);

			var cpDec2 = Factory.New<CusEntryCPDec>();
			cpDec2.ON_QuestionType = "W";
			cpDec2.ON_CPDecNum = 222;
			AssertEquals("Warning 222", cpDec2.Description);
		}

		public void TestQuestionTypeDescription()
		{
			var cusEntryCPDec = Factory.New<CusEntryCPDec>();
			cusEntryCPDec.ON_QuestionType = "Q";
			AssertEquals("Question", cusEntryCPDec.QuestionTypeDescription);
			cusEntryCPDec.ON_QuestionType = "W";
			AssertEquals("Warning", cusEntryCPDec.QuestionTypeDescription);
		}

		public void TestQuestionTypeDescriptionCaption()
		{
			var cusEntryCPDec = Factory.New<CusEntryCPDec>();
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(cusEntryCPDec.QuestionTypeDescriptionInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Question Type Description", resourceStringData.Caption);
				AssertEquals("Short Caption", "Type Desc.", resourceStringData.ShortCaption);
			});
		}
	}
}
