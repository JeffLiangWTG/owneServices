using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.TR.NCTS.Business.Test
{
	public class NctsHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestStampDutyStatusCodeListType()
		{
			AssertType<StampDutyStatusCodeList>(lookups.StampDutyStatusCodeList);
		}

		public void TestStampDutyStatusCodeListByElements()
		{
			AssertEquals("0, 1, 2, 3", lookups.StampDutyStatusCodeList.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			lookups = nctsHeader.Lookups;
		}

		NctsHeader nctsHeader;
		NctsHeaderLookups lookups;
	}
}
