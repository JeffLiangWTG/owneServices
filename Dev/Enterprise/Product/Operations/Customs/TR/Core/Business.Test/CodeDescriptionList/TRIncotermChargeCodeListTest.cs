using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class TRIncotermChargeCodeListTest : TestCaseWithFactory
	{
		public void TestIsLocalCharge()
		{
			CombineAssertions("TRIncotermChargeCodeList.IsLocalCharge", () =>
			{
				AssertEquals("COM not Local charge.", false, TRIncotermChargeCodeList.IsLocalCharge(TRIncotermChargeCodeList.Codes.COM));
				AssertEquals("DEM not Local charge.", false, TRIncotermChargeCodeList.IsLocalCharge(TRIncotermChargeCodeList.Codes.DEM));
				AssertEquals("INT not Local charge.", false, TRIncotermChargeCodeList.IsLocalCharge(TRIncotermChargeCodeList.Codes.INT));
				AssertEquals("LBC is Local charge.", true, TRIncotermChargeCodeList.IsLocalCharge(TRIncotermChargeCodeList.Codes.LBC));
				AssertEquals("LDC is Local charge.", true, TRIncotermChargeCodeList.IsLocalCharge(TRIncotermChargeCodeList.Codes.LDC));
				AssertEquals("LCC is Local charge.", true, TRIncotermChargeCodeList.IsLocalCharge(TRIncotermChargeCodeList.Codes.LocalCultureCharge));
				AssertEquals("LEC is Local charge.", true, TRIncotermChargeCodeList.IsLocalCharge(TRIncotermChargeCodeList.Codes.LocalEnvironmentCharge));
				AssertEquals("LRU is Local charge.", true, TRIncotermChargeCodeList.IsLocalCharge(TRIncotermChargeCodeList.Codes.LocalResourceUtilizationSupportFundCharge));
				AssertEquals("LTC not Local charge.", false, TRIncotermChargeCodeList.IsLocalCharge(TRIncotermChargeCodeList.Codes.LocalTotalCharges));
				AssertEquals("LOT is Local charge.", true, TRIncotermChargeCodeList.IsLocalCharge(TRIncotermChargeCodeList.Codes.LOT));
				AssertEquals("LPC is Local charge.", true, TRIncotermChargeCodeList.IsLocalCharge(TRIncotermChargeCodeList.Codes.LPC));
				AssertEquals("LSC is Local charge.", true, TRIncotermChargeCodeList.IsLocalCharge(TRIncotermChargeCodeList.Codes.LSC));
				AssertEquals("OBS not Local charge.", false, TRIncotermChargeCodeList.IsLocalCharge(TRIncotermChargeCodeList.Codes.Observation));
				AssertEquals("OFT not Local charge.", false, TRIncotermChargeCodeList.IsLocalCharge(TRIncotermChargeCodeList.Codes.OFT));
				AssertEquals("ONS not Local charge.", false, TRIncotermChargeCodeList.IsLocalCharge(TRIncotermChargeCodeList.Codes.ONS));
				AssertEquals("OTH not Local charge.", false, TRIncotermChargeCodeList.IsLocalCharge(TRIncotermChargeCodeList.Codes.OTH));
				AssertEquals("ROY not Local charge.", false, TRIncotermChargeCodeList.IsLocalCharge(TRIncotermChargeCodeList.Codes.ROY));
				AssertEquals("SUR not Local charge.", false, TRIncotermChargeCodeList.IsLocalCharge(TRIncotermChargeCodeList.Codes.Surveillance));
				AssertEquals("TFC not Local charge.", false, TRIncotermChargeCodeList.IsLocalCharge(TRIncotermChargeCodeList.Codes.TotalForeignCharges));

				AssertEquals("Unrecognized string not Local charge.", false, TRIncotermChargeCodeList.IsLocalCharge("XXX"));
				AssertEquals("Empty string not Local charge.", false, TRIncotermChargeCodeList.IsLocalCharge(string.Empty));
			});
		}

		public void TestIsForeignCharge()
		{
			CombineAssertions("TRIncotermChargeCodeList.IsForeignCharge", () =>
			{
				AssertEquals("COM is foreign charge.", true, TRIncotermChargeCodeList.IsForeignCharge(TRIncotermChargeCodeList.Codes.COM));
				AssertEquals("DEM is foreign charge.", true, TRIncotermChargeCodeList.IsForeignCharge(TRIncotermChargeCodeList.Codes.DEM));
				AssertEquals("INT is foreign charge.", true, TRIncotermChargeCodeList.IsForeignCharge(TRIncotermChargeCodeList.Codes.INT));
				AssertEquals("LBC not foreign charge.", false, TRIncotermChargeCodeList.IsForeignCharge(TRIncotermChargeCodeList.Codes.LBC));
				AssertEquals("LDC not foreign charge.", false, TRIncotermChargeCodeList.IsForeignCharge(TRIncotermChargeCodeList.Codes.LDC));
				AssertEquals("LCC not foreign charge.", false, TRIncotermChargeCodeList.IsForeignCharge(TRIncotermChargeCodeList.Codes.LocalCultureCharge));
				AssertEquals("LEC not foreign charge.", false, TRIncotermChargeCodeList.IsForeignCharge(TRIncotermChargeCodeList.Codes.LocalEnvironmentCharge));
				AssertEquals("LRU not foreign charge.", false, TRIncotermChargeCodeList.IsForeignCharge(TRIncotermChargeCodeList.Codes.LocalResourceUtilizationSupportFundCharge));
				AssertEquals("LTC not foreign charge.", false, TRIncotermChargeCodeList.IsForeignCharge(TRIncotermChargeCodeList.Codes.LocalTotalCharges));
				AssertEquals("LOT not foreign charge.", false, TRIncotermChargeCodeList.IsForeignCharge(TRIncotermChargeCodeList.Codes.LOT));
				AssertEquals("LPC not foreign charge.", false, TRIncotermChargeCodeList.IsForeignCharge(TRIncotermChargeCodeList.Codes.LPC));
				AssertEquals("LSC not foreign charge.", false, TRIncotermChargeCodeList.IsForeignCharge(TRIncotermChargeCodeList.Codes.LSC));
				AssertEquals("OBS is foreign charge.", true, TRIncotermChargeCodeList.IsForeignCharge(TRIncotermChargeCodeList.Codes.Observation));
				AssertEquals("OFT not foreign charge.", false, TRIncotermChargeCodeList.IsForeignCharge(TRIncotermChargeCodeList.Codes.OFT));
				AssertEquals("ONS not foreign charge.", false, TRIncotermChargeCodeList.IsForeignCharge(TRIncotermChargeCodeList.Codes.ONS));
				AssertEquals("OTH is foreign charge.", true, TRIncotermChargeCodeList.IsForeignCharge(TRIncotermChargeCodeList.Codes.OTH));
				AssertEquals("ROY is foreign charge.", true, TRIncotermChargeCodeList.IsForeignCharge(TRIncotermChargeCodeList.Codes.ROY));
				AssertEquals("SUR is foreign charge.", true, TRIncotermChargeCodeList.IsForeignCharge(TRIncotermChargeCodeList.Codes.Surveillance));
				AssertEquals("TFC not foreign charge.", false, TRIncotermChargeCodeList.IsForeignCharge(TRIncotermChargeCodeList.Codes.TotalForeignCharges));

				AssertEquals("Unrecognized string not foreign charge.", false, TRIncotermChargeCodeList.IsForeignCharge("XXX"));
				AssertEquals("Empty string not foreign charge.", false, TRIncotermChargeCodeList.IsForeignCharge(string.Empty));
			});
		}

		public void TestIsTotalCharge()
		{
			CombineAssertions("TRIncotermChargeCodeList.IsTotalCharge", () =>
			{
				AssertEquals("COM not Total charge.", false, TRIncotermChargeCodeList.IsTotalCharge(TRIncotermChargeCodeList.Codes.COM));
				AssertEquals("DEM not Total charge.", false, TRIncotermChargeCodeList.IsTotalCharge(TRIncotermChargeCodeList.Codes.DEM));
				AssertEquals("INT not Total charge.", false, TRIncotermChargeCodeList.IsTotalCharge(TRIncotermChargeCodeList.Codes.INT));
				AssertEquals("LBC not Total charge.", false, TRIncotermChargeCodeList.IsTotalCharge(TRIncotermChargeCodeList.Codes.LBC));
				AssertEquals("LDC not Total charge.", false, TRIncotermChargeCodeList.IsTotalCharge(TRIncotermChargeCodeList.Codes.LDC));
				AssertEquals("LCC not Total charge.", false, TRIncotermChargeCodeList.IsTotalCharge(TRIncotermChargeCodeList.Codes.LocalCultureCharge));
				AssertEquals("LEC not Total charge.", false, TRIncotermChargeCodeList.IsTotalCharge(TRIncotermChargeCodeList.Codes.LocalEnvironmentCharge));
				AssertEquals("LRU not Total charge.", false, TRIncotermChargeCodeList.IsTotalCharge(TRIncotermChargeCodeList.Codes.LocalResourceUtilizationSupportFundCharge));
				AssertEquals("LTC is Total charge.", true, TRIncotermChargeCodeList.IsTotalCharge(TRIncotermChargeCodeList.Codes.LocalTotalCharges));
				AssertEquals("LOT not Total charge.", false, TRIncotermChargeCodeList.IsTotalCharge(TRIncotermChargeCodeList.Codes.LOT));
				AssertEquals("LPC not Total charge.", false, TRIncotermChargeCodeList.IsTotalCharge(TRIncotermChargeCodeList.Codes.LPC));
				AssertEquals("LSC not Total charge.", false, TRIncotermChargeCodeList.IsTotalCharge(TRIncotermChargeCodeList.Codes.LSC));
				AssertEquals("OBS not Total charge.", false, TRIncotermChargeCodeList.IsTotalCharge(TRIncotermChargeCodeList.Codes.Observation));
				AssertEquals("OFT not Total charge.", false, TRIncotermChargeCodeList.IsTotalCharge(TRIncotermChargeCodeList.Codes.OFT));
				AssertEquals("ONS not Total charge.", false, TRIncotermChargeCodeList.IsTotalCharge(TRIncotermChargeCodeList.Codes.ONS));
				AssertEquals("OTH not Total charge.", false, TRIncotermChargeCodeList.IsTotalCharge(TRIncotermChargeCodeList.Codes.OTH));
				AssertEquals("ROY not Total charge.", false, TRIncotermChargeCodeList.IsTotalCharge(TRIncotermChargeCodeList.Codes.ROY));
				AssertEquals("SUR not Total charge.", false, TRIncotermChargeCodeList.IsTotalCharge(TRIncotermChargeCodeList.Codes.Surveillance));
				AssertEquals("TFC is Total charge.", true, TRIncotermChargeCodeList.IsTotalCharge(TRIncotermChargeCodeList.Codes.TotalForeignCharges));

				AssertEquals("Unrecognized string not Total charge.", false, TRIncotermChargeCodeList.IsTotalCharge("XXX"));
				AssertEquals("Empty string not Total charge.", false, TRIncotermChargeCodeList.IsTotalCharge(string.Empty));
			});
		}
	}
}
