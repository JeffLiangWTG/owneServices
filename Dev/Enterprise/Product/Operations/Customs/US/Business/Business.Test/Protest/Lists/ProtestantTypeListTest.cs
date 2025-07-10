using Enterprise.Customs.US.Business.Protest;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ProtestantTypeListTest : TestCase
	{
		public void TestRequiresProtestantAddressDetails()
		{
			AssertEquals(false, ProtestantTypeList.RequiresProtestantAddressDetails(ProtestantTypeList.Codes.DrawbackClaimant));
			AssertEquals(true, ProtestantTypeList.RequiresProtestantAddressDetails(ProtestantTypeList.Codes.ForeignExporterProducer));
			AssertEquals(false, ProtestantTypeList.RequiresProtestantAddressDetails(ProtestantTypeList.Codes.ImporterConsignee));
			AssertEquals(true, ProtestantTypeList.RequiresProtestantAddressDetails(ProtestantTypeList.Codes.Other));
			AssertEquals(false, ProtestantTypeList.RequiresProtestantAddressDetails(ProtestantTypeList.Codes.ShipsMaster));
			AssertEquals(false, ProtestantTypeList.RequiresProtestantAddressDetails(ProtestantTypeList.Codes.Surety));
			AssertEquals(false, ProtestantTypeList.RequiresProtestantAddressDetails(ProtestantTypeList.Codes.VesselOperator));
		}
	}
}
