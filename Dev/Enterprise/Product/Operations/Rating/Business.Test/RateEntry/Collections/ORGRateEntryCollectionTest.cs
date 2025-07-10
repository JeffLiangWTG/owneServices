using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class ORGRateEntryCollectionTest : RatingTestCase
	{
		protected virtual string Category => RatingConstants.RateCategory.ORG;

		protected virtual string Mode => Core.Constants.RateMode.ALL;

		public void TestSetDefaults()
		{
			var testQuote = Factory.New<Quote>();
			var testORGEntry = testQuote.GetRateEntryCollectionForCategory(Category).AddNew();
			AssertEquals("TI_RateCategory Default", Category, testORGEntry.TI_RateCategory);
			AssertEquals("TI_Mode Default", Mode, testORGEntry.TI_Mode);
			AssertEquals("TI_RX_NKSaleCurrency Default", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, testORGEntry.TI_RX_NKCurrency);
		}

		public void TestOriginChargesDefaultFromRegistry_NullOrigin()
		{
			var quote = Factory.New<Quote>();
			var entry = quote.AddRateEntry(Category);
			AssertEquals("Should not add any charge codes when DST rate entry has no destination", 0, entry.RateLines.Count);
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

			using (RatingDataRegistry.Instance.AirOriginDefaultChargeCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, collection))
			{
				var globalClientRate = Helper.NewGlobalClientRate(Helper.NewOrgHeader());
				var entry = Factory.New<RateEntry>();
				entry.TI_OriginLRC = "AUSYD";
				entry.TI_Mode = Core.Constants.RateMode.AIR;
				globalClientRate.GetRateEntryCollectionForCategory(Category).Add(entry);

				var message = "Local Charge code should be converted to global charge code";
				AssertEquals(message, 1, entry.RateLines.Cast<RateLine>().Count(l => l.TL_AC == globalChargeCode.PK));
			}
		}

		public void TestDefaultChargeCodes_GlobalClientRate_FallsBackToIntercompanyMapping()
		{
			AccChargeCode localChargeCode, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out _, out localChargeCode, out globalChargeCode);
			localChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			globalChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			Factory.Save();

			var collection = new LocationsChargesCollection();
			Helper.AddLocationCharge(collection, "AUSYD", localChargeCode.PK);

			using (RatingDataRegistry.Instance.AirOriginDefaultChargeCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, collection))
			{
				var globalClientRate = Helper.NewGlobalClientRate(Helper.NewOrgHeader());
				var entry = Factory.New<RateEntry>();
				entry.TI_OriginLRC = "AUSYD";
				entry.TI_Mode = Core.Constants.RateMode.AIR;
				globalClientRate.GetRateEntryCollectionForCategory(Category).Add(entry);

				var message = "Local Charge code should be converted to global charge code";
				AssertEquals(message, 1, entry.RateLines.Cast<RateLine>().Count(l => l.TL_AC == globalChargeCode.PK));
			}
		}

		public void TestOriginChargesDefaultFromRegistry()
		{
			var filter = new ZQuery(AccChargeCodeSchema.AC_GC, Env.CurrentCompanyPK) { MaximumRows = 3 };
			filter.AddToFilter(AccChargeCodeSchema.AC_ChargeGroup, ChargeCodeGroupList.Codes.Freight);
			var chargeCodes = Factory.Load<AccChargeCode>(filter);

			var airCollection = new LocationsChargesCollection();
			Helper.AddLocationCharge(airCollection, "USNYC", chargeCodes[0].PK);
			Helper.AddLocationCharge(airCollection, "USNYC", chargeCodes[1].PK);
			Helper.AddLocationCharge(airCollection, "US", chargeCodes[0].PK);

			var seaCollection = new LocationsChargesCollection();
			Helper.AddLocationCharge(seaCollection, "USLAX", chargeCodes[0].PK);
			Helper.AddLocationCharge(seaCollection, "USLAX", chargeCodes[1].PK);
			Helper.AddLocationCharge(seaCollection, "USLAX", chargeCodes[2].PK);

			RatingDataRegistry.Instance.AirOriginDefaultChargeCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, airCollection);
			RatingDataRegistry.Instance.SeaOriginDefaultChargeCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, seaCollection);

			var testRate = Factory.NewWithValidTestData<ClientRate>();
			var entry = Factory.New<RateEntry>();
			entry.TI_OriginLRC = "USNYC";
			entry.TI_Mode = "AIR";
			testRate.GetRateEntryCollectionForCategory(Category).Add(entry);
			AssertEquals("Two lines must be defaulted from the registry", 2, entry.RateLines.Count);

			entry = Factory.New<RateEntry>();
			entry.TI_OriginLRC = "USLAX";
			entry.TI_Mode = "AIR";
			testRate.GetRateEntryCollectionForCategory(Category).Add(entry);
			AssertEquals("One line must be defaulted from the registry", 1, entry.RateLines.Count);

			entry = Factory.New<RateEntry>();
			entry.TI_OriginLRC = "UABGD";
			entry.TI_Mode = "AIR";
			testRate.GetRateEntryCollectionForCategory(Category).Add(entry);
			AssertEquals("No lines must be defaulted from the registry", 0, entry.RateLines.Count);

			entry = Factory.New<RateEntry>();
			entry.TI_OriginLRC = "USLAX";
			entry.TI_Mode = "SEA";
			testRate.GetRateEntryCollectionForCategory(Category).Add(entry);
			AssertEquals("3 lines must be defaulted from the registry", 3, entry.RateLines.Count);
		}

		public void TestDefaultChargeCodes_GlobalClientRate_IntercompanyFallBack()
		{
			var globalChargeCodeCode = "GLBFRT";

			AccChargeCode localChargeCode, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out _, out localChargeCode, out globalChargeCode);
			globalChargeCode.AC_Code = globalChargeCodeCode;
			globalChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			localChargeCode.Delete();

			var localChargeCodePK = Helper.ChargeCodes["FRT"].PK;
			var globalChargeCodeMapIntercompany = (BusinessObject)Factory.New<IGlobalChargeCodeMapIntercompany>();
			globalChargeCodeMapIntercompany[AccGlobalChargeCodeMapSchema.YG_Code] = globalChargeCodeCode;

			var globalChargeCodeMapPivotIntercompany = (BusinessObject)Factory.New<IGlobalChargeCodeMapPivotIntercompany>();
			globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_YG] = globalChargeCodeMapIntercompany.PK;
			globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_AC] = localChargeCodePK;
			using (globalChargeCodeMapPivotIntercompany.GetValidationSuspender())
			{
				globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_TYPE] = LedgerTypes.AccountsReceivable;

				Factory.Save();

				var collection = new LocationsChargesCollection();
				Helper.AddLocationCharge(collection, "AUSYD", localChargeCodePK);

				using (RatingDataRegistry.Instance.SeaOriginDefaultChargeCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, collection))
				{
					var globalClientRate = Helper.NewGlobalClientRate(Helper.NewOrgHeader());
					var entry = Factory.New<RateEntry>();
					entry.TI_OriginLRC = "AUSYD";
					globalClientRate.GetRateEntryCollectionForCategory(Category).Add(entry);

					var message = "Local Charge code should be converted to global charge code via matching AR intercompany mapping";
					AssertEquals(message, 1, entry.RateLines.Cast<RateLine>().Count(l => l.TL_AC == globalChargeCode.PK));
				}
			}
		}

		public void TestDefaultChargeCodes_GlobalCosting_IntercompanyFallBack()
		{
			var globalChargeCodeCode = "GLBFRT";

			AccChargeCode localChargeCode, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out _, out localChargeCode, out globalChargeCode);
			globalChargeCode.AC_Code = globalChargeCodeCode;
			globalChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			localChargeCode.Delete();

			var localChargeCodePK = Helper.ChargeCodes["FRT"].PK;
			var globalChargeCodeMapIntercompany = (BusinessObject)Factory.New<IGlobalChargeCodeMapIntercompany>();
			globalChargeCodeMapIntercompany[AccGlobalChargeCodeMapSchema.YG_Code] = globalChargeCodeCode;

			var globalChargeCodeMapPivotIntercompany = (BusinessObject)Factory.New<IGlobalChargeCodeMapPivotIntercompany>();
			globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_YG] = globalChargeCodeMapIntercompany.PK;
			globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_AC] = localChargeCodePK;
			using (globalChargeCodeMapPivotIntercompany.GetValidationSuspender())
			{
				globalChargeCodeMapPivotIntercompany[AccGlobalChargeCodeMapPivotSchema.YP_TYPE] = LedgerTypes.AccountsPayable;

				Factory.Save();

				var collection = new LocationsChargesCollection();
				Helper.AddLocationCharge(collection, "AUSYD", localChargeCodePK);

				using (RatingDataRegistry.Instance.SeaOriginDefaultChargeCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, collection))
				{
					var globalClientRate = Helper.NewGlobalCosting(Helper.NewOrgHeader());
					var entry = Factory.New<RateEntry>();
					entry.TI_OriginLRC = "AUSYD";
					globalClientRate.GetRateEntryCollectionForCategory(Category).Add(entry);

					var message = "Local Charge code should be converted to global charge code via matching AP intercompany mapping";
					AssertEquals(message, 1, entry.RateLines.Cast<RateLine>().Count(l => l.TL_AC == globalChargeCode.PK));
				}
			}
		}
	}

	[TestedType(typeof(ORGRateEntryCollection))]
	public class ORGRateEntryBizObjCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ORGRateEntryCollection(Factory.New<Quote>(), Factory);
		}
	}
}
