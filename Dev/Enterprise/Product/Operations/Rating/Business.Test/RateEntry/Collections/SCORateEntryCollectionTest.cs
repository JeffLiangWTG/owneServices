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
	public class SCORateEntryCollectionTest : RatingTestCase
	{
		public void TestSetDefaults()
		{
			var quote = Factory.New<Quote>();
			var entry = quote.AddRateEntry(RatingConstants.RateCategory.SCO);
			AssertEquals("TI_RateCategory Default", RatingConstants.RateCategory.SCO, entry.TI_RateCategory);
			AssertEquals("TI_Mode Default", Core.Constants.RateMode.SEA, entry.TI_Mode);
			AssertEquals("Unit Default", RatingConstants.Units.CN, entry.Unit);
			AssertEquals("TI_RX_NKSaleCurrency Default", "USD", entry.TI_RX_NKCurrency);
		}

		public void TestChargesDefaultFromRegistry()
		{
			var quote = Factory.New<Quote>();
			var entry = quote.AddRateEntry(RatingConstants.RateCategory.SCO);
			AssertEquals("SCO Rate Line Created", 1, entry.RateLines.Count);
			AssertEquals(UnitCalculator.Code, entry.RateLines[0].TL_RateCalculator);
		}

		public void TestSCOFreightDefaultCodes()
		{
			var chargeCode1PK = Helper.ChargeCodes["FRT"].PK;
			var chargeCode2PK = Helper.ChargeCodes["WAR"].PK;
			var chargeCode3PK = Helper.ChargeCodes["PS"].PK;
			var chargeCodes = string.Join(",", chargeCode1PK, chargeCode2PK, chargeCode3PK);

			using (RatingDataRegistry.Instance.ShippingContainerisedDefaultCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chargeCodes))
			{
				var quote = Helper.NewQuote(Helper.NewOrgHeader());
				var entry = quote.AddRateEntry(RatingConstants.RateCategory.SCO);
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

			using (RatingDataRegistry.Instance.ShippingContainerisedDefaultCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, localChargeCode.PK.ToString()))
			{
				var globalClientRate = Helper.NewGlobalClientRate(Helper.NewOrgHeader());
				var entry = globalClientRate.AddRateEntry(RatingConstants.RateCategory.SCO);
				var message = "Local Charge code should be converted to global charge code";
				AssertEquals(message, 1, entry.RateLines.Cast<RateLine>().Count(l => l.TL_AC == globalChargeCode.PK));
			}
		}
	}

	[TestedType(typeof(SCORateEntryCollection))]
	public class SCORateEntryBizObjCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new SCORateEntryCollection(Factory.New<Quote>(), Factory);
		}
	}
}
