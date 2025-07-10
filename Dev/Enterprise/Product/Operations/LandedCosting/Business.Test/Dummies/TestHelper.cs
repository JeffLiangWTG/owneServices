using System;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LandedCosting.Business.Testing
{
	public sealed class TestHelper
	{
		public TestHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public DummyIUltimateDistributee Ultimate1;
		public DummyIUltimateDistributee Ultimate2;
		public DummyIUltimateDistributee Ultimate3;
		public DummyLandedCostDistributeTo Distribute1;
		public DummyLandedCostDistributeTo Distribute2;
		public DummyLandedCostHeader DummyHeader;
		public LandCostInput LCInput1;
		public LandCostInput LCInput2;
		public LandCostInput LCInput3;

		public LandedCostHeader GetLCHeaderWithNothingButOneCostInput()
		{
			var collection = new LandedCostingGroupCollection();
			var landedCostingGroup = collection.AddNew();
			landedCostingGroup.GroupID = 1;
			landedCostingGroup.GroupName = "ONE";
			landedCostingGroup.CostDistributionCode = landedCostingGroup.CostDistributionList[0].Code;

			FreightDataRegistry.Instance.LandedCostingPreferences.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			var dummyHeader = factory.New<DummyLandedCostHeader>();
			dummyHeader.UltimateDistributeesExposed = Array.Empty<IUltimateDistributee>();

			var header = factory.New<LandedCostHeader>();
			header.LT_ParentID = dummyHeader.PK;
			header.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			LCInput3 = header.CostInputs.AddNew();
			LCInput3.LI_ParentTableCode = DummyBizoSchema.Constants.Prefix;
			LCInput3.LI_CostAmount = 10000m;
			LCInput3.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			LCInput3.LI_LandedCostGroup = 1;
			LCInput3.LI_DistributeCostBy = CostDistributionMechanismList.Codes.LineValue;

			return header;
		}

		public LandedCostHeader GetLCHeaderWithNoChargeRowsToDefault()
		{
			Ultimate1 = factory.New<DummyIUltimateDistributee>();
			Ultimate1.PKExposed = ZGuid.NewZGuid();
			Ultimate1.ActualExposed = 10m;
			Ultimate1.ActualWeightInKGExposed = 50m;
			Ultimate1.CostInLocalCurrencyExposed = 1m;

			Distribute1 = factory.New<DummyLandedCostDistributeTo>();
			Distribute1.UltimateDistributeesExposed = new IUltimateDistributee[] { Ultimate1 };

			var dummyChargeHolder = new DummyLandCostChargeHolder();
			dummyChargeHolder.ChargesToImportForLandedCostingExposed = Array.Empty<IDefaultLandedCostInput>();//no charge rows -> no cost input

			var dummyHeader = factory.New<DummyLandedCostHeader>();
			dummyHeader.CandidatesToDistributeCostToExposed = new ILandedCostDistributeTo[] { Distribute1 };
			dummyHeader.ExchangeRateHoldersExposed = Array.Empty<ILandedCostExchangeRateHolder>();
			dummyHeader.UltimateDistributeesExposed = new IUltimateDistributee[] { Ultimate1 };
			dummyHeader.ChargeHoldersExposed = new ILandedCostChargeHolder[] { dummyChargeHolder };

			var lCHeader = factory.New<LandedCostHeader>();
			lCHeader.LT_ParentID = dummyHeader.PK;
			lCHeader.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			return lCHeader;
		}

		public LandedCostHeader GetLCHeaderWithBackRoundingTest()
		{
			var collection = new LandedCostingGroupCollection();
			var landedCostingGroup = collection.AddNew();
			landedCostingGroup.GroupID = 1;
			landedCostingGroup.GroupName = "ONE";
			landedCostingGroup.CostDistributionCode = landedCostingGroup.CostDistributionList[0].Code;

			var landedCostingGroup2 = collection.AddNew();
			landedCostingGroup2.GroupID = 2;
			landedCostingGroup2.GroupName = "TWO";
			landedCostingGroup2.CostDistributionCode = landedCostingGroup.CostDistributionList[0].Code;

			var landedCostingGroup3 = collection.AddNew();
			landedCostingGroup3.GroupID = 3;
			landedCostingGroup3.GroupName = "THREE";
			landedCostingGroup3.CostDistributionCode = landedCostingGroup.CostDistributionList[0].Code;

			FreightDataRegistry.Instance.LandedCostingPreferences.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			Ultimate1 = factory.New<DummyIUltimateDistributee>();
			Ultimate2 = factory.New<DummyIUltimateDistributee>();
			Ultimate3 = factory.New<DummyIUltimateDistributee>();

			Ultimate1.PKExposed = ZGuid.NewZGuid();
			Ultimate1.ActualExposed = 10m;
			Ultimate1.ActualWeightInKGExposed = 50m;
			Ultimate1.CostInLocalCurrencyExposed = 100m;
			Ultimate1.TableCodeExposed = "XX";

			Ultimate2.PKExposed = ZGuid.NewZGuid();
			Ultimate2.ActualExposed = 30m;
			Ultimate2.ActualWeightInKGExposed = 50m;
			Ultimate2.CostInLocalCurrencyExposed = 100m;
			Ultimate2.TableCodeExposed = "XX";

			Ultimate3.PKExposed = ZGuid.NewZGuid();
			Ultimate3.ActualExposed = 10m;
			Ultimate3.ActualWeightInKGExposed = 50m;
			Ultimate3.CostInLocalCurrencyExposed = 100m;
			Ultimate3.TableCodeExposed = "XX";

			Distribute1 = factory.New<DummyLandedCostDistributeTo>();
			Distribute1.UltimateDistributeesExposed = new IUltimateDistributee[] { Ultimate1, Ultimate2, Ultimate3 };
			Distribute1.UniqueCodeExposed = "D1";

			DummyHeader = factory.New<DummyLandedCostHeader>();
			DummyHeader.CandidatesToDistributeCostToExposed = new ILandedCostDistributeTo[] { Distribute1 };
			DummyHeader.ExchangeRateHoldersExposed = Array.Empty<ILandedCostExchangeRateHolder>();
			DummyHeader.UltimateDistributeesExposed = new IUltimateDistributee[] { Ultimate1, Ultimate2, Ultimate3 };
			DummyHeader.ChargeHoldersExposed = Array.Empty<ILandedCostChargeHolder>();

			var lCHeader = factory.New<LandedCostHeader>();
			lCHeader.LT_ParentID = DummyHeader.PK;
			lCHeader.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			LCInput1 = lCHeader.CostInputs.AddNew();
			LCInput1.LinkedObjectUniqueCode = Distribute1.UniqueCode;
			LCInput1.LI_ParentID = Distribute1.PK;
			LCInput1.LI_ParentTableCode = DummyBizoSchema.Constants.Prefix;
			LCInput1.LI_CostAmount = 1000m;
			LCInput1.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			LCInput1.LI_LandedCostGroup = 1;//Registry item
			LCInput1.LI_DistributeCostBy = CostDistributionMechanismList.Codes.LineValue;

			return lCHeader;
		}

		public LandedCostHeader GetLCHeaderWithDistributionObjectsPluggedIn()
		{
			var collection = new LandedCostingGroupCollection();
			var landedCostingGroup = collection.AddNew();
			landedCostingGroup.GroupID = 1;
			landedCostingGroup.GroupName = "ONE";
			landedCostingGroup.CostDistributionCode = landedCostingGroup.CostDistributionList[0].Code;

			var landedCostingGroup2 = collection.AddNew();
			landedCostingGroup2.GroupID = 2;
			landedCostingGroup2.GroupName = "TWO";
			landedCostingGroup2.CostDistributionCode = landedCostingGroup.CostDistributionList[0].Code;

			var landedCostingGroup3 = collection.AddNew();
			landedCostingGroup3.GroupID = 3;
			landedCostingGroup3.GroupName = "THREE";
			landedCostingGroup3.CostDistributionCode = landedCostingGroup.CostDistributionList[0].Code;

			FreightDataRegistry.Instance.LandedCostingPreferences.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			Ultimate1 = factory.New<DummyIUltimateDistributee>();
			Ultimate2 = factory.New<DummyIUltimateDistributee>();
			Ultimate1.PKExposed = ZGuid.NewZGuid();
			Ultimate1.ActualExposed = 10m;
			Ultimate1.ActualWeightInKGExposed = 50m;
			Ultimate1.WeightExposed = 50m;
			Ultimate1.CostInLocalCurrencyExposed = 1m;
			Ultimate1.TableCodeExposed = JobOrderLineSchema.Constants.Prefix;

			Ultimate2.PKExposed = ZGuid.NewZGuid();
			Ultimate2.ActualExposed = 30m;
			Ultimate2.ActualWeightInKGExposed = 50m;
			Ultimate2.WeightExposed = 50m;
			Ultimate2.CostInLocalCurrencyExposed = 9m;
			Ultimate2.TableCodeExposed = JobOrderLineSchema.Constants.Prefix;

			Distribute1 = factory.New<DummyLandedCostDistributeTo>();
			Distribute1.UltimateDistributeesExposed = new IUltimateDistributee[] { Ultimate1 };
			Distribute1.UniqueCodeExposed = "D1";
			Distribute1.TableCodeExposed = JobOrderLineSchema.Constants.Prefix;

			Distribute2 = factory.New<DummyLandedCostDistributeTo>();
			Distribute2.UltimateDistributeesExposed = new IUltimateDistributee[] { Ultimate1, Ultimate2 };
			Distribute2.UniqueCodeExposed = "D2";
			Distribute2.TableCodeExposed = JobOrderLineSchema.Constants.Prefix;

			DummyHeader = factory.New<DummyLandedCostHeader>();
			DummyHeader.CandidatesToDistributeCostToExposed = new ILandedCostDistributeTo[] { Distribute1, Distribute2 };
			DummyHeader.ExchangeRateHoldersExposed = Array.Empty<ILandedCostExchangeRateHolder>();
			DummyHeader.UltimateDistributeesExposed = new IUltimateDistributee[] { Ultimate1, Ultimate2 };
			DummyHeader.ChargeHoldersExposed = Array.Empty<ILandedCostChargeHolder>();
			DummyHeader.TableCodeExposed = JobOrderHeaderSchema.Constants.Prefix;

			var lCHeader = factory.New<LandedCostHeaderForTesting>();
			lCHeader.LT_ParentID = DummyHeader.PK;
			lCHeader.LT_ParentTableCode = DummyHeader.TableCode;

			LCInput1 = lCHeader.CostInputs.AddNew();
			LCInput1.LinkedObjectUniqueCode = Distribute1.UniqueCode;
			LCInput1.LI_ParentID = Distribute1.PK;
			LCInput1.LI_ParentTableCode = Distribute1.TableCode;
			LCInput1.LI_CostAmount = 1000m;
			LCInput1.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			LCInput1.LI_LandedCostGroup = 1;//Registry item
			LCInput1.LI_DistributeCostBy = CostDistributionMechanismList.Codes.Actual;

			LCInput2 = lCHeader.CostInputs.AddNew();
			LCInput2.LinkedObjectUniqueCode = Distribute2.UniqueCode;
			LCInput2.LI_ParentID = Distribute2.PK;
			LCInput2.LI_CostAmount = 5000m;
			LCInput2.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			LCInput2.LI_ParentTableCode = Distribute1.TableCode;
			LCInput2.LI_LandedCostGroup = 2;
			LCInput2.LI_DistributeCostBy = CostDistributionMechanismList.Codes.ActualWeight;

			LCInput3 = lCHeader.CostInputs.AddNew();
			LCInput3.LinkedObjectUniqueCode = Distribute2.UniqueCode;
			LCInput3.LI_ParentID = Distribute2.PK;
			LCInput3.LI_ParentTableCode = Distribute1.TableCode;
			LCInput3.LI_CostAmount = 10000m;
			LCInput3.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			LCInput3.LI_LandedCostGroup = 3;
			LCInput3.LI_DistributeCostBy = CostDistributionMechanismList.Codes.LineValue;
			return lCHeader;
		}

		public OrgHeader SetAutoratingData()
		{
			var buyer = factory.NewWithValidTestData<OrgHeader>();
			buyer.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			var query = new ZQuery(AccChargeCodeSchema.AC_Code, "FRT");
			query.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

			var chargeCode = factory.LoadTop1<AccChargeCode>(query);

			var ratingHeader = factory.New(ObjectFactory.Get<Integration.Rating.IRating>().CompanyTariffRatingHeaderType);
			ratingHeader[RatingHeaderSchema.TH_Accepted] = new ZDateTime(2010, 1, 20);
			ratingHeader[RatingHeaderSchema.TH_RateType] = "GLB";
			ratingHeader[RatingHeaderSchema.TH_GlobalRateDescription] = "Base Company Tariff";
			ratingHeader[RatingHeaderSchema.TH_GC] = GlbCompany.CurrentCompany.PK;
			ratingHeader[RatingHeaderSchema.TH_GlobalRateLevel] = new ZByte(1);
			ratingHeader[RatingHeaderSchema.TH_SystemCreateTimeUtc] = DateTime.UtcNow;
			ratingHeader[RatingHeaderSchema.TH_SystemCreateUser] = "~BP";
			ratingHeader[RatingHeaderSchema.TH_SystemLastEditTimeUtc] = DateTime.UtcNow;
			ratingHeader[RatingHeaderSchema.TH_SystemLastEditUser] = "~BP";

			var rateEntry = factory.New(ObjectFactory.Get<Integration.Rating.IRating>().RateEntryType);
			rateEntry[RateEntrySchema.TI_TH] = ratingHeader.PK;
			rateEntry[RateEntrySchema.TI_GC_Publisher] = GlbCompany.CurrentCompany.PK;
			rateEntry[RateEntrySchema.TI_Mode] = "LCL";
			rateEntry[RateEntrySchema.TI_RateCategory] = "LCL";
			rateEntry[RateEntrySchema.TI_RateStartDate] = new ZDateTime(2010, 1, 19);
			rateEntry[RateEntrySchema.TI_RateEndDate] = new ZDateTime(2010, 7, 31);
			rateEntry[RateEntrySchema.TI_RX_NKCurrency] = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			rateEntry[RateEntrySchema.TI_RS_NKServiceLevel_NI] = "STD";
			rateEntry[RateEntrySchema.TI_OriginLRC] = "NZAKL";
			rateEntry[RateEntrySchema.TI_RH_NKCommodityCode] = "GEN";
			rateEntry[RateEntrySchema.TI_SystemCreateTimeUtc] = DateTime.UtcNow;
			rateEntry[RateEntrySchema.TI_SystemCreateUser] = "~BP";
			rateEntry[RateEntrySchema.TI_SystemLastEditTimeUtc] = DateTime.UtcNow;
			rateEntry[RateEntrySchema.TI_SystemLastEditUser] = "~BP";

			factory.Save();

			var rateLinePK = Guid.NewGuid();

			var sqlRateLine = $@"INSERT INTO {RateLinesSchema.Constants.SqlSchemaName}.{RateLinesSchema.Constants.TableName}
({RateLinesSchema.PK.Name}, {RateLinesSchema.TL_TI.Name}, {RateLinesSchema.TL_RateDesc.Name}, {RateLinesSchema.TL_RX_NKCurrency.Name},
{RateLinesSchema.TL_AC.Name}, {RateLinesSchema.TL_RateCalculator.Name}, {RateLinesSchema.TL_CompanyTariffLevel.Name}, {RateLinesSchema.TL_Rounding.Name}, {RateLinesSchema.TL_SystemCreateTimeUtc.Name}, {RateLinesSchema.TL_SystemCreateUser.Name}, {RateLinesSchema.TL_SystemLastEditTimeUtc.Name}, {RateLinesSchema.TL_SystemLastEditUser.Name})
VALUES ('{rateLinePK}', '{rateEntry.PK}', 'International Freight', '{GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency}', '{chargeCode.PK}', 'MIN', 1, 'DEF', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			Db.Connection.ExecuteNonQuery(sqlRateLine);

			var sqlRateLineItem = $@"INSERT INTO {RateLineItemsSchema.Constants.SqlSchemaName}.{RateLineItemsSchema.Constants.TableName}
({RateLineItemsSchema.PK.Name}, {RateLineItemsSchema.TM_TL.Name}, {RateLineItemsSchema.TM_Type.Name}, {RateLineItemsSchema.TM_Value.Name}, {RateLineItemsSchema.TM_SystemCreateTimeUtc.Name}, {RateLineItemsSchema.TM_SystemCreateUser.Name}, {RateLineItemsSchema.TM_SystemLastEditTimeUtc.Name}, {RateLineItemsSchema.TM_SystemLastEditUser.Name})
VALUES ('{Guid.NewGuid()}', '{rateLinePK}', 'MIN', 103, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			Db.Connection.ExecuteNonQuery(sqlRateLineItem);

			return buyer;
		}

		public LandedCostHistory GetHistory(LandedCostHeader lcHeader)
		{
			var lcHistory = factory.New<LandedCostHistoryForTesting>();
			lcHeader.Histories.Add(lcHistory);
			lcHistory.LH_ParentID = Ultimate1.PK;
			lcHistory.LH_ParentTableCode = Ultimate1.TableCode;

			return lcHistory;
		}

		readonly BusinessObjectFactory factory;
	}
}
