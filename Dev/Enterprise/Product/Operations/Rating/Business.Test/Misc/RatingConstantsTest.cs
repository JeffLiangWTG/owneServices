using System.Linq;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class RatingConstantsTest : TestCase
	{
		public void TestInferForwardingRateCategory()
		{
			AssertEquals(RatingConstants.RateCategory.ORG, RatingConstants.InferForwardingRateCategory(ChargeCodeGroupList.Codes.Origin));
			AssertEquals(RatingConstants.RateCategory.ORG, RatingConstants.InferForwardingRateCategory(ChargeCodeGroupList.Codes.Loading));
			AssertEquals(RatingConstants.RateCategory.ORG, RatingConstants.InferForwardingRateCategory(ChargeCodeGroupList.Codes.OriginBrokerage));
			AssertEquals(RatingConstants.RateCategory.ORG, RatingConstants.InferForwardingRateCategory(ChargeCodeGroupList.Codes.OriginBrokerageOnly));
			AssertEquals(string.Empty, RatingConstants.InferForwardingRateCategory(ChargeCodeGroupList.Codes.CustomsDuty));

			AssertEquals(RatingConstants.RateCategory.DST, RatingConstants.InferForwardingRateCategory(ChargeCodeGroupList.Codes.Destination));
			AssertEquals(RatingConstants.RateCategory.DST, RatingConstants.InferForwardingRateCategory(ChargeCodeGroupList.Codes.Unloading));
			AssertEquals(RatingConstants.RateCategory.DST, RatingConstants.InferForwardingRateCategory(ChargeCodeGroupList.Codes.Brokerage));
			AssertEquals(RatingConstants.RateCategory.DST, RatingConstants.InferForwardingRateCategory(ChargeCodeGroupList.Codes.BrokerageOnly));

			AssertEquals(RatingConstants.RateCategory.AIR, RatingConstants.InferForwardingRateCategory(ChargeCodeGroupList.Codes.Freight, RatingConstants.TransportMode.AIR));
			AssertEquals(RatingConstants.RateCategory.AIR, RatingConstants.InferForwardingRateCategory(ChargeCodeGroupList.Codes.Insurance, RatingConstants.TransportMode.AIR));

			AssertEquals(RatingConstants.RateCategory.FCL, RatingConstants.InferForwardingRateCategory(ChargeCodeGroupList.Codes.Freight, RatingConstants.TransportMode.SEA, true));
			AssertEquals(RatingConstants.RateCategory.FCL, RatingConstants.InferForwardingRateCategory(ChargeCodeGroupList.Codes.Insurance, RatingConstants.TransportMode.SEA, true));

			AssertEquals(RatingConstants.RateCategory.LCL, RatingConstants.InferForwardingRateCategory(ChargeCodeGroupList.Codes.Freight, RatingConstants.TransportMode.SEA, false));
			AssertEquals(RatingConstants.RateCategory.LCL, RatingConstants.InferForwardingRateCategory(ChargeCodeGroupList.Codes.Insurance, RatingConstants.TransportMode.SEA, false));
		}

		public void TestTransportZonesSupport()
		{
			Assert(!RatingConstants.RateCategory.SupportsTransportZones(RatingConstants.RateCategory.AIR));
			Assert(!RatingConstants.RateCategory.SupportsTransportZones(RatingConstants.RateCategory.CST));
			Assert(!RatingConstants.RateCategory.SupportsTransportZones(RatingConstants.RateCategory.DST));
			Assert(!RatingConstants.RateCategory.SupportsTransportZones(RatingConstants.RateCategory.FCL));
			Assert(!RatingConstants.RateCategory.SupportsTransportZones(RatingConstants.RateCategory.LCL));
			Assert(!RatingConstants.RateCategory.SupportsTransportZones(RatingConstants.RateCategory.ORG));
			Assert(!RatingConstants.RateCategory.SupportsTransportZones(RatingConstants.RateCategory.PAC));
			Assert(!RatingConstants.RateCategory.SupportsTransportZones(RatingConstants.RateCategory.SCO));
			Assert(!RatingConstants.RateCategory.SupportsTransportZones(RatingConstants.RateCategory.SDE));
			Assert(!RatingConstants.RateCategory.SupportsTransportZones(RatingConstants.RateCategory.SED));
			Assert(!RatingConstants.RateCategory.SupportsTransportZones(RatingConstants.RateCategory.SID));
			Assert(!RatingConstants.RateCategory.SupportsTransportZones(RatingConstants.RateCategory.SNC));
			Assert(!RatingConstants.RateCategory.SupportsTransportZones(RatingConstants.RateCategory.SOR));
			Assert(!RatingConstants.RateCategory.SupportsTransportZones(RatingConstants.RateCategory.TRN));
			Assert(RatingConstants.RateCategory.SupportsTransportZones(RatingConstants.RateCategory.TBC));
			Assert(!RatingConstants.RateCategory.SupportsTransportZones(RatingConstants.RateCategory.UNP));
			Assert(!RatingConstants.RateCategory.SupportsTransportZones(RatingConstants.RateCategory.WHS));
			Assert(!RatingConstants.RateCategory.SupportsTransportZones(RatingConstants.RateCategory.TRW));
			Assert(!RatingConstants.RateCategory.SupportsTransportZones(RatingConstants.RateCategory.TWU));
			Assert(!RatingConstants.RateCategory.SupportsTransportZones(RatingConstants.RateCategory.CYD));
		}

		public void TestRateModeGeneralize()
		{
			foreach (var rateMode in Core.Constants.RateMode.RateModes)
			{
				Assert("RateMode cannot be empty", !string.IsNullOrEmpty(rateMode));

				var generalisedModes = RatingConstants.RateCategory.RateCategories.Select(category => RatingConstants.RateMode.Generalize(rateMode, category))
					.Concat(new[] { RatingConstants.RateMode.Generalize(rateMode, true) })
					.Concat(new[] { RatingConstants.RateMode.Generalize(rateMode, false) });

				foreach (var generalizedMode in generalisedModes)
				{
					AssertNotEquals("RateMode should be different when generalized", rateMode, generalizedMode);

					if (rateMode == Core.Constants.RateMode.ALL)
					{
						Assert("ALL should always generalize to string.Empty", string.IsNullOrEmpty(generalizedMode));
					}
					else
					{
						Assert("Generalized RateMode should still be a RateMode", Core.Constants.RateMode.RateModes.Any(mode2 => mode2 == generalizedMode));
					}
				}
			}
		}

		public void TestCheckTransitTimeIsASpecialValue()
		{
			AssertEquals(false, RatingConstants.TransitTimes.CheckIsSpecialValue("0"));
			AssertEquals(true, RatingConstants.TransitTimes.CheckIsSpecialValue("SMD"));
			AssertEquals(true, RatingConstants.TransitTimes.CheckIsSpecialValue("OVN"));
			AssertEquals(false, RatingConstants.TransitTimes.CheckIsSpecialValue("20"));
			AssertEquals(false, RatingConstants.TransitTimes.CheckIsSpecialValue("INVALID"));
		}

		public void TestTransitTimeConvertToInt()
		{
			AssertEquals(-1, RatingConstants.TransitTimes.ConvertToInt("Invalid"));
			AssertEquals(-1, RatingConstants.TransitTimes.ConvertToInt(""));
			AssertEquals(0, RatingConstants.TransitTimes.ConvertToInt("0"));
			AssertEquals(0, RatingConstants.TransitTimes.ConvertToInt("SMD"));
			AssertEquals(1, RatingConstants.TransitTimes.ConvertToInt("OVN"));
			AssertEquals(15, RatingConstants.TransitTimes.ConvertToInt("15"));
		}
	}
}
