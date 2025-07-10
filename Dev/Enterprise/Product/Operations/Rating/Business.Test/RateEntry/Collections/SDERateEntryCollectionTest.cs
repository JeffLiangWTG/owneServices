using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class SDERateEntryCollectionTest : RatingTestCase
	{
		public void TestSetDefaults()
		{
			var quote = Factory.New<Quote>();
			var entry = quote.AddRateEntry(RatingConstants.RateCategory.SDE);
			AssertEquals("TI_RateCategory Default", RatingConstants.RateCategory.SDE, entry.TI_RateCategory);
			AssertEquals("TI_Mode", Core.Constants.RateMode.ALL, entry.TI_Mode);
			AssertEquals("Unit", "", entry.Unit);
			AssertEquals("TI_RX_NKSaleCurrency Default", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, entry.TI_RX_NKCurrency);
		}

		public void TestDefaultChargeCodes_GlobalClientRate()
		{
			AccChargeCode localChargeCode, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out _, out localChargeCode, out globalChargeCode);
			localChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			globalChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			Factory.Save();

			var collection = new LocationsChargesCollection();
			Helper.AddLocationCharge(collection, "AUSYD", localChargeCode.PK);

			using (RatingDataRegistry.Instance.ShippingDestinationDefaultChargeCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, collection))
			{
				var globalClientRate = Helper.NewGlobalClientRate(Helper.NewOrgHeader());
				var entry = Factory.New<RateEntry>();
				entry.TI_DestinationLRC = "AUSYD";
				globalClientRate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.SDE).Add(entry);

				var message = "Local Charge code should be converted to global charge code";
				AssertEquals(message, 1, entry.RateLines.Cast<RateLine>().Count(l => l.TL_AC == globalChargeCode.PK));
			}
		}

		public void TestChargesDefaultFromRegistry()
		{
			var filter = new ZQuery();
			filter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			filter.AddToFilter(AccChargeCodeSchema.AC_ChargeGroup, ChargeCodeGroupList.Codes.Freight);
			filter.MaximumRows = 5;

			var chargeCodes = Factory.Load<AccChargeCode>(filter);

			var collection = new LocationsChargesCollection();
			Helper.AddLocationCharge(collection, "USNYC", chargeCodes[0].PK);
			Helper.AddLocationCharge(collection, "USNYC", chargeCodes[1].PK);
			Helper.AddLocationCharge(collection, "UABGD", chargeCodes[2].PK);
			Helper.AddLocationCharge(collection, "US", chargeCodes[3].PK);
			Helper.AddLocationCharge(collection, "MYBAG", chargeCodes[4].PK);

			RatingDataRegistry.Instance.ShippingDestinationDefaultChargeCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			var rate = Factory.NewWithValidTestData<ClientRate>();
			var entry = Factory.New<RateEntry>();
			entry.TI_DestinationLRC = "";
			rate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.SDE).Add(entry);
			Assert("Should not match any charges", !entry.RateLines.Any());

			entry = Factory.New<RateEntry>();
			entry.TI_DestinationLRC = "USLAX";
			rate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.SDE).Add(entry);
			AssertContainsExactElementsInAnyOrder("Should only match the 'US' charge",
				new ZGuid[] { chargeCodes[3].PK },
				Array.ConvertAll(entry.RateLines.ToArray<RateLine>(), (l) => l.TL_AC));

			entry = Factory.New<RateEntry>();
			entry.TI_DestinationLRC = "USNYC";
			rate.GetRateEntryCollectionForCategory(RatingConstants.RateCategory.SDE).Add(entry);
			AssertContainsExactElementsInAnyOrder("Should match the 'USNYC' charges",
				new ZGuid[] { chargeCodes[0].PK, chargeCodes[1].PK },
				Array.ConvertAll(entry.RateLines.ToArray<RateLine>(), (l) => l.TL_AC));
		}
	}

	[TestedType(typeof(SDERateEntryCollection))]
	public class SDERateEntryBizObjCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new SDERateEntryCollection(Factory.New<Quote>(), Factory);
		}
	}
}
