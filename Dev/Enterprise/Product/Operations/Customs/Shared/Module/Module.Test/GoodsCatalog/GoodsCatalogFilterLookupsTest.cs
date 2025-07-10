using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Module.Testing
{
	public class GoodsCatalogFilterLookupsTest : TestCaseWithFactory
	{
		public void TestTypeList()
		{
			AssertEquals("TypeList", typeof(GoodsCatalogTypeList), lookups.TypeList.GetType());
			AssertContainsExactElementsInAnyOrder(new string[] { "EXP", "IMP" }, filterBizObj.Lookups.TypeList.GetAllCodes());
		}

		public void TestStatusTypeList()
		{
			AssertEquals("StatusTypeList", typeof(CodeDescriptionPairList), lookups.StatusTypeList.GetType());
		}

		public void TestMessageStatusList()
		{
			AssertEquals("MessageStatusList", typeof(CodeDescriptionPairList), filterBizObj.Lookups.MessageStatusList().GetType());
			AssertContainsExactElementsInAnyOrder(new string[] { "ACC", "ACD", "ACO", "ACR", "AWC", "AWD", "AWO", "AWR", "CLC", "CLD", "CLO", "CLR", "ERC", "ERD", "ERO", "ERR", "NOT", "SNT", "UNK" }, filterBizObj.Lookups.MessageStatusList().GetAllCodes());
		}

		protected override void SetUp()
		{
			base.SetUp();
			filterBizObj = new GoodsCatalogFilterBusinessObject();
			lookups = new GoodsCatalogFilterLookups(filterBizObj);
		}

		GoodsCatalogFilterBusinessObject filterBizObj;
		GoodsCatalogFilterLookups lookups;
	}
}
