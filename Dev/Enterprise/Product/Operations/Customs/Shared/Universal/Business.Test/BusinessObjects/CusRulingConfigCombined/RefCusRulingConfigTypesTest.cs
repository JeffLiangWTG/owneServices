using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;

namespace Enterprise.Customs.Universal.Testing
{
	class RefCusRulingConfigTypesTest : TestCaseWithFactory
	{
		public void TestGetUppercaseCodeList()
		{
			var list = RefCusRulingConfigTypes.GetUppercaseCodeList(Factory);
			foreach (ICodeDescription refCusRulingConfigType in new RefCusRulingConfigTypes())
			{
				Assert(list.ContainsCode(refCusRulingConfigType.Code));
			}

			AssertSame(list, RefCusRulingConfigTypes.GetUppercaseCodeList(Factory));
		}
	}
}
