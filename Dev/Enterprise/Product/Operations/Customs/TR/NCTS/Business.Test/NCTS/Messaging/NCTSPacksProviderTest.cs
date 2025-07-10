using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	public class NCTSPacksProviderTest : TestCaseWithFactory
	{
		public void TestPacksMembers()
		{
			using (var helper = new NCTSMessageProviderTestHelper(Factory))
			{
				var header = helper.GetProviderNCTSHeader();
				var nctsHeaderProvider = new NCTSHeaderProvider(header);

				var goodsItems = nctsHeaderProvider.GoodsItems.FirstOrDefault();
				var packages = goodsItems.Packs.ToArray();

				CombineAssertions("Packs members for level 1", () =>
				{
					AssertEquals("MarksAndNumbers", "ABCD1234560", packages[0].MarksAndNumbers);
					AssertEquals("MarksAndNumbersLNG", TRMessageConstants.LanguageCode, packages[0].MarksAndNumbersLNG);
					AssertEquals("UnitTypeCode", "BI", packages[0].UnitTypeCode);
					AssertEquals("UnitCount", 10, packages[0].PackagesUnitCount);
					AssertEquals("NumOfPieGS25", ZString.Empty, packages[0].PiecesUnitCount);
				});

				CombineAssertions("Packs members for level 2", () =>
				{
					AssertEquals("MarksAndNumbers", "ABCD1234561", packages[1].MarksAndNumbers);
					AssertEquals("MarksAndNumbersLNG", TRMessageConstants.LanguageCode, packages[1].MarksAndNumbersLNG);
					AssertEquals("UnitTypeCode", "KG", packages[1].UnitTypeCode);
					AssertEquals("UnitCount", 11, packages[1].PackagesUnitCount);
					AssertEquals("NumOfPieGS25", ZString.Empty, packages[0].PiecesUnitCount);
				});
			}
		}
	}
}
