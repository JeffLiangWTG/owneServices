using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class SNCRateEntryCollectionTest : RatingTestCase
	{
		public void TestSetDefaults()
		{
			var quote = Factory.New<Quote>();
			var entry = quote.AddRateEntry(RatingConstants.RateCategory.SNC);
			AssertEquals("TI_RateCategory Default", RatingConstants.RateCategory.SNC, entry.TI_RateCategory);
			AssertEquals("TI_Mode Default", Core.Constants.RateMode.LCL, entry.TI_Mode);
			AssertEquals("Unit Default", Env.Registry.FreightVolumeUnit, entry.Unit);
			AssertEquals("TI_RX_NKSaleCurrency Default", "USD", entry.TI_RX_NKCurrency);
		}

		public void TestChargesDefaultFromRegistry()
		{
			var quote = Factory.New<Quote>();
			var entry = quote.AddRateEntry(RatingConstants.RateCategory.SCO);
			AssertEquals("SCO Rate Line Created", 1, entry.RateLines.Count);
		}

		public void TestSNCFreightDefaultCodes()
		{
			var chargeCode1PK = Helper.ChargeCodes["FRT"].PK;
			var chargeCode2PK = Helper.ChargeCodes["WAR"].PK;
			var chargeCode3PK = Helper.ChargeCodes["PS"].PK;
			var chargeCodes = string.Join(",", chargeCode1PK, chargeCode2PK, chargeCode3PK);

			using (RatingDataRegistry.Instance.ShippingNonContainerisedtDefaultCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chargeCodes))
			{
				var quote = Helper.NewQuote(Helper.NewOrgHeader());
				var entry = quote.AddRateEntry(RatingConstants.RateCategory.SNC);
				var lines = entry.RateLines.Cast<RateLine>().ToArray();

				AssertEquals(3, lines.Length);
				AssertEquals(1, lines.Count(l => l.TL_AC == chargeCode1PK));
				AssertEquals(1, lines.Count(l => l.TL_AC == chargeCode2PK));
				AssertEquals(1, lines.Count(l => l.TL_AC == chargeCode3PK));
			}
		}

		public void TestDefaultChargeCodes_GlobalClientRate()
		{
			AccChargeCode localChargeCode, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out _, out localChargeCode, out globalChargeCode);
			localChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			globalChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			Factory.Save();

			using (RatingDataRegistry.Instance.ShippingNonContainerisedtDefaultCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, localChargeCode.PK.ToString()))
			{
				var globalClientRate = Helper.NewGlobalClientRate(Helper.NewOrgHeader());
				var entry = globalClientRate.AddRateEntry(RatingConstants.RateCategory.SNC);
				var message = "Local Charge code should be converted to global charge code";
				AssertEquals(message, 1, entry.RateLines.Cast<RateLine>().Count(l => l.TL_AC == globalChargeCode.PK));
			}
		}
	}

	[TestedType(typeof(SNCRateEntryCollection))]
	public class SNCRateEntryBizObjCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new SNCRateEntryCollection(Factory.New<Quote>(), Factory);
		}
	}
}
