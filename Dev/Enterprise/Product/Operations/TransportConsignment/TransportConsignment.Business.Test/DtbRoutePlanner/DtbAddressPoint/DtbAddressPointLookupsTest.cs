using CargoWise.EntityFramework.Testing;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbAddressPointLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestAddresses

		public void TestAddresses()
		{
			var instruction = Factory.New<DtbConsignmentInstruction>();
			var lookups1 = new DtbAddressPointLookups(new DtbAddressPoint(instruction.Address));
			var lookups2 = new DtbAddressPointLookups(new DtbAddressPoint(instruction.Address));

			AssertNotNull(lookups1.Addresses);
			AssertEquals("Collection should be factory cached.", lookups1.Addresses, lookups2.Addresses);
		}

		#endregion
	}
}
