using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class CusInBondEventLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestIncidentCodeList()
		{
			var cusInBondEvent = Factory.New<CusInBondEventForTest>();
			var lookups = cusInBondEvent.Lookups;
			var list = lookups.IncidentCodeList;
			CombineAssertions(() =>
			{
				AssertEquals("List", "1, 2, 3, 4, 5, 6", list.CodesAsString);
				AssertSame("Cached", list, lookups.IncidentCodeList);
			});
		}

		class CusInBondEventForTest : CusInBondEvent
		{
			public CusInBondEventForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}
	}
}
