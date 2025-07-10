using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class InBondTransportModeCodesTest : TestCaseWithFactory
	{
		public void TestValidCodes()
		{
			var codes = InBondTransportModeCodes.GetCachedValue(Factory);
			AssertEquals(6, codes.Count);
			AssertEquals("Vessel, non container", true, codes.ContainsCode("10"));
			AssertEquals("Vessel Containerized", true, codes.ContainsCode("11"));
			AssertEquals("Rail", true, codes.ContainsCode("20"));
			AssertEquals("Truck", true, codes.ContainsCode("30"));
			AssertEquals("Air", true, codes.ContainsCode("40"));
			AssertEquals("Pipeline", true, codes.ContainsCode("70"));
		}

		public void TestIsSeaOrRail()
		{
			Assert(InBondTransportModeCodes.IsSeaOrRail("10"));
			Assert(!InBondTransportModeCodes.IsSeaOrRail("40"));
			Assert(InBondTransportModeCodes.IsSeaOrRail("11"));
			Assert(!InBondTransportModeCodes.IsSeaOrRail("30"));
			Assert(InBondTransportModeCodes.IsSeaOrRail("20"));
		}

		public void TestGetNonAMSCachedValue()
		{
			var codes = InBondTransportModeCodes.GetNonAMSCachedValue(Factory);
			AssertEquals(3, codes.Count);
			AssertEquals("Truck, Non-container", true, codes.ContainsCode("30"));
			AssertEquals("Air, Non-container", true, codes.ContainsCode("40"));
			AssertEquals("Fixed Transport Installations(Includes pipeline and powerhouse)", true, codes.ContainsCode("70"));
		}
	}
}
