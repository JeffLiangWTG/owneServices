using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Customs.US.AMS.Messaging.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	sealed class CusInbondBillAddRefLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestReferenceList()
		{
			var header = Factory.New<CusInBondHeader>();
			var bill = header.Bills.AddNew();
			var additinalReference = bill.ShipmentReferenceDetails.AddNew();
			var list = additinalReference.Lookups.ReferenceList;
			var expectedList = BillReferenceList.GetCachedValue(Factory);
			AssertEquals(expectedList.Count, list.Count);
			foreach (ICodeDescription pair in expectedList)
			{
				AssertEquals(pair.Code, pair.Description, list.GetDescriptionFromCode(pair.Code));
			}
		}
	}
}
