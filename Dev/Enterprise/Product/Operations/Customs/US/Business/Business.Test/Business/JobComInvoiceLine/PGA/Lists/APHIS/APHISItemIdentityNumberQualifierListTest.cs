using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business.Testing
{
	class APHISItemIdentityNumberQualifierListTest : CodeDescriptionPairListTest
	{
		public void TestGetListForProgram()
		{
			var fullList = new APHISItemIdentityNumberQualifierList();
			fullList.RemoveCode(APHISItemIdentityNumberQualifierList.Codes.BQG);
			var list1 = APHISItemIdentityNumberQualifierList.GetListWithoutBouquet(Factory);
			var list2 = APHISItemIdentityNumberQualifierList.GetListWithoutBouquet(Factory);
			AssertEquals("Data should be cached", list1, list2);
			AssertList<APHISItemIdentityNumberQualifierList>(list1, fullList.ToArray().Select(x => x.Code).ToArray());
			AssertEquals("Tattoo", list1.GetDescriptionFromCode("TO"));
		}
	}
}
