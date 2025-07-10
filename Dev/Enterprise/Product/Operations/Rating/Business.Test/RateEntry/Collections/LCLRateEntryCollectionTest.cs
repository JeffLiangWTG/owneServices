using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class LCLRateEntryCollectionTest : RatingTestCase
	{
		protected virtual string Category => RatingConstants.RateCategory.LCL;

		protected virtual string Mode => RatingConstants.RateCategory.LCL;

		public void TestSetDefaults()
		{
			var quote = Factory.New<Quote>();
			var entry = quote.AddRateEntry(Category);
			AssertEquals("TI_RateCategory Default", Category, entry.TI_RateCategory);
			AssertEquals("TI_Mode Default", Mode, entry.TI_Mode);
			AssertEquals("Unit Default", Env.Registry.FreightVolumeUnit, entry.Unit);
			AssertEquals("TI_RX_NKSaleCurrency Default", "USD", entry.TI_RX_NKCurrency);

			AssertEquals("Should have defaulted a rate line", 1, entry.RateLines.Count);
			AssertEquals(MinimumOrPerUnitCalculator.Code, entry.RateLines[0].TL_RateCalculator);
			AssertEquals(RegistryConstants.Strings.DefaultFreightChargeCode, entry.RateLines[0].ChargeCode.AC_Code);
		}

		public void TestLCLFreightDefaultCodes()
		{
			var chargeCode1PK = Helper.ChargeCodes["FRT"].PK;
			var chargeCode2PK = Helper.ChargeCodes["WAR"].PK;
			var chargeCode3PK = Helper.ChargeCodes["PS"].PK;
			var chargeCodes = string.Join(",", chargeCode1PK, chargeCode2PK, chargeCode3PK);

			using (RatingDataRegistry.Instance.LCLFreightDefaultCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chargeCodes))
			{
				var quote = Helper.NewQuote(Helper.NewOrgHeader());
				var entry = quote.AddRateEntry(Category);
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

			using (RatingDataRegistry.Instance.LCLFreightDefaultCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, localChargeCode.PK.ToString()))
			{
				var globalClientRate = Helper.NewGlobalClientRate(Helper.NewOrgHeader());
				var entry = globalClientRate.AddRateEntry(Category);
				var message = "Local Charge code should be converted to global charge code";
				AssertEquals(message, 1, entry.RateLines.Cast<RateLine>().Count(l => l.TL_AC == globalChargeCode.PK));
			}
		}
	}

	[TestedType(typeof(LCLRateEntryCollection))]
	public class LCLRateEntryBizObjCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new LCLRateEntryCollection(Factory.New<Quote>(), Factory);
		}
	}
}
