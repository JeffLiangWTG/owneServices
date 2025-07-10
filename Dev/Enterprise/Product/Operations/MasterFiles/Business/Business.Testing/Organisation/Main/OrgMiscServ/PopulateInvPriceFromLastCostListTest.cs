using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class PopulateInvPriceFromLastCostListTest : TestCase
	{
		public void TestPopulateInvPriceFromLastCostList()
		{
			PopulateInvPriceFromLastCostList testOM_EXInvPriceFromLastCost_List = new PopulateInvPriceFromLastCostList();
			Assert("OM_IMAutoPopulateOwnerRef_List.Count == 3", testOM_EXInvPriceFromLastCost_List.Count == 3);
			Assert("LastCostList should have option Yes", testOM_EXInvPriceFromLastCost_List.ContainsCode(PopulateInvPriceFromLastCostList.Codes.Yes));
			Assert("LastCostList should have option No", testOM_EXInvPriceFromLastCost_List.ContainsCode(PopulateInvPriceFromLastCostList.Codes.No));
			Assert("LastCostList should have a blank option", testOM_EXInvPriceFromLastCost_List.ContainsCode(PopulateInvPriceFromLastCostList.Codes.Blank));
		}
	}
}
