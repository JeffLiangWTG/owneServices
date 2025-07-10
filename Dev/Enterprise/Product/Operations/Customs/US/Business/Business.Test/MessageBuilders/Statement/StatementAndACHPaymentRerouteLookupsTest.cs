using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class StatementAndACHPaymentRerouteLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestZ9_RerouteTypeList()
		{
			var reroute = new StatementAndACHPaymentReroute();
			AssertEquals(typeof(StatementTypeList), reroute.Lookups.Z9_RerouteTypeList.GetType());
			AssertEquals(2, reroute.Lookups.Z9_RerouteTypeList.Count);
		}

		public void TestZ9_MessageTypeList()
		{
			var reroute = new StatementAndACHPaymentReroute();
			AssertEquals(typeof(JobApplicationCodeList), reroute.Lookups.Z9_MessageTypeList.GetType());
			AssertEquals(2, reroute.Lookups.Z9_MessageTypeList.Count);
		}

		public void TestZ9_ProcessingPortList()
		{
			var reroute = new StatementAndACHPaymentReroute();
			AssertEquals(typeof(ZZRefCusCodeListCombinedCollection), reroute.Lookups.Z9_ProcessingPortList.GetType());
		}
	}
}
