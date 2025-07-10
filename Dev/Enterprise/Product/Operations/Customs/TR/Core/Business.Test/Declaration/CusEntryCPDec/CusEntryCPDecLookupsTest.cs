using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	class CusEntryCPDecLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestQuestionTypeList()
		{
			AssertContainsExactElementsInAnyOrder(new ICodeDescription[]
			{
				new CodeDescriptionPair("Q", "Question"),
				new CodeDescriptionPair("W", "Warning"),
			}, Lookups.QuestionTypeList);
		}

		public void TestAnswerCodeList()
		{
			AssertEquals(typeof(Universal.CodeDescriptionPairLists.YesNoList), Lookups.AnswerCodeList.GetType());
		}

		CusEntryCPDecLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					var cpDec = Factory.New<CusEntryCPDec>();
					lookups = new CusEntryCPDecLookups(cpDec);
				}
				return lookups;
			}
		}
		CusEntryCPDecLookups lookups;
	}
}
