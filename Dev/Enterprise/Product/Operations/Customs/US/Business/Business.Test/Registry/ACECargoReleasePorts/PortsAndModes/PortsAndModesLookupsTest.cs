using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	sealed class PortsAndModesLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookups()
		{
			var portsAndModes = new PortsAndModes(null);
			AssertEquals(typeof(TransportTypeList), portsAndModes.Lookups.TransportModeList.GetType());
			AssertEquals(typeof(ZZRefCusCodeListCombinedCollection), portsAndModes.Lookups.PortList.GetType());
			AssertEquals(typeof(CertificationOptionsList), portsAndModes.Lookups.CertificationMethodsList.GetType());
		}
	}
}
