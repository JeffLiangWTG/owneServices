using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.Testing
{
	using RelatedPartyList = Enterprise.MasterFiles.Business.Customs.RelatedPartyList;
	public class RelationshipIndicatorListTest : TestCaseWithFactory
	{
		public void TestBaseListAndNZListHaveSameCodes()
		{
			RelationshipIndicatorList nzList = new RelationshipIndicatorList(false);
			RelatedPartyList baseList = new RelatedPartyList();

			AssertEquals("Count of both lists should match - The CODES at least have to match.", baseList.Count, nzList.Count);
			foreach (CodeDescriptionPair pair in nzList)
			{
				Assert("Base list didn't contain '" + pair.Code + "', this is a PROBLEM.", baseList.ContainsCode(pair.Code));
			}
			foreach (CodeDescriptionPair pair in baseList)
			{
				Assert("NZ list didn't contain '" + pair.Code + "', this is a PROBLEM.", nzList.ContainsCode(pair.Code));
			}
		}
	}
}
