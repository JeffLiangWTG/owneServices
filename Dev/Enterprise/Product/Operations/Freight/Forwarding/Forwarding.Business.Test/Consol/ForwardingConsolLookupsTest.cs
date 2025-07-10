using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingConsolLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBillOfLadingBillStatusList()
		{
			var billOfLadingBillStatusList = Lookups.BillOfLadingBillStatusList;

			AssertEquals("OBR, OBT, OBA, STP, SUR", billOfLadingBillStatusList.CodesAsString);
			AssertEquals("Original Bill Received", billOfLadingBillStatusList[0].Description);
			AssertEquals("Original Bill Transferred", billOfLadingBillStatusList[1].Description);
			AssertEquals("Original Bill Amendment in Progress", billOfLadingBillStatusList[2].Description);
			AssertEquals("Switched To Paper", billOfLadingBillStatusList[3].Description);
			AssertEquals("Surrendered", billOfLadingBillStatusList[4].Description);
		}

		ForwardingConsolLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = new ForwardingConsolLookups(Factory.New<ForwardingConsol>());
				}

				return lookups;
			}
		}
		ForwardingConsolLookups lookups;
	}
}
