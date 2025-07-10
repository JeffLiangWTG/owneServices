using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class RefCusPackListProviderTest : TestCaseWithFactory
	{
		public void TestGetDeclarationPackTypeList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList("UNE", "UNPKG", "PT1", "PT1 Desc", new ZDateTime(2010, 1, 1), ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList("UNE", "UNPKG", "PT2", "PT2 Desc", new ZDateTime(2010, 1, 1), new ZDateTime(2010, 1, 1));

			Factory.Save();

			var list = new RefCusPackListProvider().GetDeclarationPackTypeList(Factory);
			AssertEquals("PT1", list.CodesAsString);
		}
	}
}
