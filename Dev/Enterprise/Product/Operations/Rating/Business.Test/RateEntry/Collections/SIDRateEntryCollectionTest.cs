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
	public class SIDRateEntryCollectionTest : RatingTestCase
	{
		public void TestSetDefaults()
		{
			var quote = Factory.New<Quote>();
			var entry = quote.AddRateEntry(RatingConstants.RateCategory.SED);
			AssertEquals("TI_RateCategory Default", RatingConstants.RateCategory.SED, entry.TI_RateCategory);
			AssertEquals("TI_Mode Default", Core.Constants.RateMode.SEA, entry.TI_Mode);
			AssertEquals("Unit Default", RatingConstants.Units.DY, entry.Unit);
			AssertEquals("TI_RX_NKSaleCurrency Default", "AUD", entry.TI_RX_NKCurrency);
		}

		public void TestDefaultChargeCodes_GlobalClientRate()
		{
			AccChargeCode localChargeCode, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out _, out localChargeCode, out globalChargeCode);
			localChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			globalChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			Factory.Save();

			using (LinerAgencyDataRegistry.Instance.ImportDetentionChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, localChargeCode.PK.ToGuid()))
			{
				var globalClientRate = Helper.NewGlobalClientRate(Helper.NewOrgHeader());
				var entry = globalClientRate.AddRateEntry(RatingConstants.RateCategory.SID);

				var message = "Local Charge code should be converted to global charge code";
				AssertEquals(message, 1, entry.RateLines.Cast<RateLine>().Count(l => l.TL_AC == globalChargeCode.PK));
			}
		}
	}

	[TestedType(typeof(SIDRateEntryCollection))]
	public class SIDRateEntryBizObjCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new SIDRateEntryCollection(Factory.New<Quote>(), Factory);
		}
	}
}
