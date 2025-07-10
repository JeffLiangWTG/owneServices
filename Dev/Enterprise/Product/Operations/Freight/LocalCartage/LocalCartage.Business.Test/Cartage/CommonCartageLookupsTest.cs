using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CommonCartageLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAddressLists()
		{
			var commonCartage = Factory.New<CommonCartage>();
			commonCartage.JJ_E3_NKJobType = "ISMY";
			AssertEquals(commonCartage.Lookups.CTOList, commonCartage.Lookups.FirstAddressList);
			AssertEquals(commonCartage.Lookups.DepotList, commonCartage.Lookups.SecondAddressList);
			AssertEquals(commonCartage.Lookups.ContainerYardList, commonCartage.Lookups.ThirdAddressList);
			AssertEquals(commonCartage.Lookups.ConsigneeList, commonCartage.Lookups.FourthAddressList);
			commonCartage.JJ_E3_NKJobType = "ESMY";
			AssertEquals(commonCartage.Lookups.ConsignorList, commonCartage.Lookups.FirstAddressList);
			AssertEquals(commonCartage.Lookups.DepotList, commonCartage.Lookups.SecondAddressList);
			AssertEquals(commonCartage.Lookups.ContainerYardList, commonCartage.Lookups.ThirdAddressList);
			AssertEquals(commonCartage.Lookups.CTOList, commonCartage.Lookups.FourthAddressList);
			commonCartage.JJ_E3_NKJobType = "ISFC";
			AssertEquals(commonCartage.Lookups.CTOList, commonCartage.Lookups.FirstAddressList);
			AssertEquals(commonCartage.Lookups.ConsigneeList, commonCartage.Lookups.SecondAddressList);
			AssertEquals(commonCartage.Lookups.OrgHeaderList, commonCartage.Lookups.ThirdAddressList);
			AssertEquals(commonCartage.Lookups.OrgHeaderList, commonCartage.Lookups.FourthAddressList);
		}
	}
}
