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
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	public class AIRRateEntryCollectionTest : RatingTestCase
	{
		protected virtual string Category => RatingConstants.RateCategory.AIR;

		protected virtual string Mode => Core.Constants.RateMode.LSE;

		public void TestSetDefaults()
		{
			var quote = Helper.NewQuote(Helper.NewOrgHeader());
			var entry = quote.EntryCollections[Category].LazyLoadingCollection.AddNew();
			AssertEquals("TI_RateCategory Default", Category, entry.TI_RateCategory);
			AssertEquals("TI_Mode Default", Mode, entry.TI_Mode);
			AssertEquals("Unit Default", Env.Registry.FreightWeightUnit, entry.Unit);

			AssertEquals("Should have defaulted a rate line", 1, entry.RateLines.Count);
			AssertEquals(CombinedCalculator.Code, entry.RateLines[0].TL_RateCalculator);
			AssertEquals(RegistryConstants.Strings.DefaultFreightChargeCode, entry.RateLines[0].ChargeCode.AC_Code);
		}

		public void TestAIRFreightDefaultCodes()
		{
			var chargeCode1PK = Helper.ChargeCodes["FRT"].PK;
			var chargeCode2PK = Helper.ChargeCodes["WAR"].PK;
			var chargeCode3PK = Helper.ChargeCodes["PS"].PK;
			var chargeCodes = string.Join(",", chargeCode1PK, chargeCode2PK, chargeCode3PK);

			using (RatingDataRegistry.Instance.AIRFreightDefaultCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chargeCodes))
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

			using (RatingDataRegistry.Instance.AIRFreightDefaultCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, localChargeCode.PK.ToString()))
			{
				var globalClientRate = Helper.NewGlobalClientRate(Helper.NewOrgHeader());
				var entry = globalClientRate.AddRateEntry(Category);
				var message = "Local Charge code should be converted to global charge code";
				AssertEquals(message, 1, entry.RateLines.Cast<RateLine>().Count(l => l.TL_AC == globalChargeCode.PK));
			}
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

				using (RatingDataRegistry.Instance.AIRFreightDefaultCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, localChargeCodePK.ToString()))
				{
					var globalClientRate = Helper.NewGlobalClientRate(Helper.NewOrgHeader());
					var entry = globalClientRate.AddRateEntry(Category);
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

				using (RatingDataRegistry.Instance.AIRFreightDefaultCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, localChargeCodePK.ToString()))
				{
					var globalClientRate = Helper.NewGlobalCosting(null);
					var entry = globalClientRate.AddRateEntry(Category);
					var message = "Local Charge code should be converted to global charge code via matching AP intercompany mapping";
					AssertEquals(message, 1, entry.RateLines.Cast<RateLine>().Count(l => l.TL_AC == globalChargeCode.PK));
				}
			}
		}

		public void TestDefaultChargeCodes_GlobalCosting_MultipleIntercompanyFallBack()
		{
			AccChargeCode localChargeCode1, globalChargeCode1;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out _, out localChargeCode1, out globalChargeCode1, true, false, "GLBFRT", "GLBFRT");
			globalChargeCode1.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			AccChargeCode localChargeCode2, globalChargeCode2;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out _, out localChargeCode2, out globalChargeCode2, true, false, "ANOTHER1", "ANOTHER1");
			globalChargeCode2.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			localChargeCode1.Delete();
			localChargeCode2.Delete();

			var localChargeCodePK = Helper.ChargeCodes["FRT"].PK;
			var globalChargeCodeMapIntercompany1 = (BusinessObject)Factory.New<IGlobalChargeCodeMapIntercompany>();
			globalChargeCodeMapIntercompany1[AccGlobalChargeCodeMapSchema.YG_Code] = globalChargeCode1.AC_Code;

			var globalChargeCodeMapPivotIntercompany1 = (BusinessObject)Factory.New<IGlobalChargeCodeMapPivotIntercompany>();
			globalChargeCodeMapPivotIntercompany1[AccGlobalChargeCodeMapPivotSchema.YP_YG] = globalChargeCodeMapIntercompany1.PK;
			globalChargeCodeMapPivotIntercompany1[AccGlobalChargeCodeMapPivotSchema.YP_AC] = localChargeCodePK;
			using (globalChargeCodeMapPivotIntercompany1.GetValidationSuspender())
			{
				globalChargeCodeMapPivotIntercompany1[AccGlobalChargeCodeMapPivotSchema.YP_TYPE] = LedgerTypes.AccountsPayable;

				var globalChargeCodeMapIntercompany2 = (BusinessObject)Factory.New<IGlobalChargeCodeMapIntercompany>();
				globalChargeCodeMapIntercompany2[AccGlobalChargeCodeMapSchema.YG_Code] = globalChargeCode2.AC_Code;

				var globalChargeCodeMapPivotIntercompany2 = (BusinessObject)Factory.New<IGlobalChargeCodeMapPivotIntercompany>();
				globalChargeCodeMapPivotIntercompany2[AccGlobalChargeCodeMapPivotSchema.YP_YG] = globalChargeCodeMapIntercompany2.PK;
				globalChargeCodeMapPivotIntercompany2[AccGlobalChargeCodeMapPivotSchema.YP_AC] = localChargeCodePK;
				using (globalChargeCodeMapPivotIntercompany2.GetValidationSuspender())
				{
					globalChargeCodeMapPivotIntercompany2[AccGlobalChargeCodeMapPivotSchema.YP_TYPE] = LedgerTypes.AccountsPayable;

					Factory.Save();

					using (RatingDataRegistry.Instance.AIRFreightDefaultCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, localChargeCodePK.ToString()))
					{
						var globalClientRate = Helper.NewGlobalCosting(null);
						var entry = globalClientRate.AddRateEntry(Category);
						var message = "Should not try to fall back to intercompany mapping when mapping has multiple results";

						AssertEquals(message, 1, entry.RateLines.Cast<RateLine>().Count(l => l.TL_AC == localChargeCodePK));
					}
				}
			}
		}
	}

	[TestedType(typeof(AIRRateEntryCollection))]
	public class AIRRateEntryBizObjCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AIRRateEntryCollection(Factory.New<Quote>(), Factory);
		}
	}
}
