using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.GUI.Test
{
	public class RateLineModuleFiltersTest : RatingTestCase
	{
		public void TestConstructor()
		{
			var rateLineFilters = new RateLineModuleFilters(Factory, RatingConstants.RatingHeaderTypes.ClientRate, null, false);
			AssertEquals(RatingConstants.RatingHeaderTypes.ClientRate, rateLineFilters.RatingHeaderType);
			AssertEquals("", rateLineFilters.RateCategory);
			var sampleRateTypes = RateType.CFS | RateType.ContainerYard | RateType.Forwarding | RateType.Shipping | RateType.Warehouse;
			AssertEquals("all rate types", sampleRateTypes, sampleRateTypes & rateLineFilters.RateTypeFromCategory);

			rateLineFilters = new RateLineModuleFilters(Factory, RatingConstants.RatingHeaderTypes.Costing, RatingConstants.RateCategory.FCL, false);
			AssertEquals(RatingConstants.RatingHeaderTypes.Costing, rateLineFilters.RatingHeaderType);
			AssertEquals(RatingConstants.RateCategory.FCL, rateLineFilters.RateCategory);
			AssertEquals(RateType.Forwarding, RateType.Forwarding & rateLineFilters.RateTypeFromCategory);
		}

		public void TestFilterDescriptions()
		{
			var filterCollection = new ModuleFilterCollection();
			var rateLineFilters = new RateLineModuleFilters(Factory, RatingConstants.RatingHeaderTypes.ClientRate, null, false);
			rateLineFilters.AddForRateEntryFilter(filterCollection);
			AssertFilterDescription(RateLineModuleFilters.Constants.Codes.ActualPercentage, RateLineModuleFilters.Constants.Description.ActualPercentage, filterCollection);
			AssertFilterDescription(RateLineModuleFilters.Constants.Codes.UseOnlyActualWeightMeasure, RateLineModuleFilters.Constants.Description.UseOnlyActualWeightMeasure, filterCollection);
			AssertFilterDescription(RateLineModuleFilters.Constants.Codes.Condition, RateLineModuleFilters.Constants.Description.Condition, filterCollection);
			AssertFilterDescription(RateLineModuleFilters.Constants.Codes.ContainerOwnership, RateLineModuleFilters.Constants.Description.ContainerOwnership, filterCollection);
			AssertFilterDescription(RateLineModuleFilters.Constants.Codes.ConversionFactor, RateLineModuleFilters.Constants.Description.ConversionFactor, filterCollection);
			AssertFilterDescription(RateLineModuleFilters.Constants.Codes.Currency, RateLineModuleFilters.Constants.Description.Currency, filterCollection);
			AssertFilterDescription(RateLineModuleFilters.Constants.Codes.ChargeCode, RateLineModuleFilters.Constants.Description.ChargeCode, filterCollection);
			AssertFilterDescription(RateLineModuleFilters.Constants.Codes.FeeChargeType, RateLineModuleFilters.Constants.Description.FeeChargeType, filterCollection);
			AssertFilterDescription(RateLineModuleFilters.Constants.Codes.FeeChargeLevel, RateLineModuleFilters.Constants.Description.FeeChargeLevel, filterCollection);
			AssertFilterDescription(RateLineModuleFilters.Constants.Codes.Rounding, RateLineModuleFilters.Constants.Description.Rounding, filterCollection);
			AssertFilterDescription(RateLineModuleFilters.Constants.Codes.HasOverrideChargeDescription, RateLineModuleFilters.Constants.Description.HasOverrideChargeDescription, filterCollection);
			AssertFilterDescription(RateLineModuleFilters.Constants.Codes.IsJobLevelCharge, RateLineModuleFilters.Constants.Description.IsJobLevelCharge, filterCollection);
			AssertFilterDescription(RateLineModuleFilters.Constants.Codes.UnitFactor, RateLineModuleFilters.Constants.Description.UnitFactor, filterCollection);
			AssertFilterDescription(RateLineModuleFilters.Constants.Codes.UnitMultiple, RateLineModuleFilters.Constants.Description.UnitMultiple, filterCollection);
			AssertFilterDescription(RateLineModuleFilters.Constants.Codes.Units, RateLineModuleFilters.Constants.Description.Units, filterCollection);
			AssertFilterDescription(RateLineModuleFilters.Constants.Codes.StartDate, RateLineModuleFilters.Constants.Description.StartDate, filterCollection);
			AssertFilterDescription(RateLineModuleFilters.Constants.Codes.EndDate, RateLineModuleFilters.Constants.Description.EndDate, filterCollection);
			AssertFilterDescription(RateLineModuleFilters.Constants.Codes.EffectiveOn, RateLineModuleFilters.Constants.Description.EffectiveOn, filterCollection);
			AssertFilterDescription(RateLineModuleFilters.Constants.Codes.ShowExpired, RateLineModuleFilters.Constants.Description.ShowExpired, filterCollection);

			void AssertFilterDescription(string code, MultilingualString description, ModuleFilterCollection filters)
			{
				var filter = filters.Single(x => x.Code == code);
				AssertEquals(code, description, filter.MultilingualDescription);
			}
		}

		public void TestChargeCodeFilter()
		{
			var client = NewClient;

			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("ZZZ");
			var globalClientRate = Helper.NewGlobalClientRate(client);
			var globalEntry1 = globalClientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "GBLON", "", removeLines: true);
			var globalLine1 = globalEntry1.AddFlatRateLine("ZZZ", 1);
			Factory.Save();

			var localChargeCode = Helper.ChargeCodes.GetExisting("ZZZ");
			var localClientRate = Helper.NewClientRate(client);
			var localEntry1 = localClientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "GBLON", "", removeLines: true);
			var localEntry2 = localClientRate.AddRateEntry(RatingConstants.RateCategory.ORG, Core.Constants.RateMode.LCL, "CNSHA", "", removeLines: true);
			var localLine1 = localEntry1.AddFlatRateLine("ZZZ", 1);
			var localLine2 = localEntry2.AddFlatRateLine("FRT", 1);
			Factory.Save();

			var collection = localClientRate.ORGRateEntriesForBinding;
			collection.Load(new ZQuery());
			AssertCollectionContains(localEntry1, collection);
			AssertCollectionContains(localEntry2, collection);
			AssertCollectionContains("global entry is loaded", globalEntry1, collection);
			AssertEquals(3, collection.Count);

			var filterCollection = new ModuleFilterCollection();
			var rateLineFilters = new RateLineModuleFilters(Factory, RatingConstants.RatingHeaderTypes.ClientRate, null, false);
			rateLineFilters.AddForRateEntryFilter(filterCollection);
			var chargeCodeFilter = (ModuleGuidFilter)filterCollection.Single(x => x.Code == RateLineModuleFilters.Constants.Codes.ChargeCode);
			chargeCodeFilter.IsActive = true;

			// Filter on "ZZZ"
			chargeCodeFilter.Property = localChargeCode.PK;
			collection.Load(filterCollection.GetFilterQuery(new[] { chargeCodeFilter }));
			AssertCollectionContains(localEntry1, collection);
			AssertCollectionNotContains("local entry with other charge code is not loaded", localEntry2, collection);
			AssertCollectionContains("global entry is loaded", globalEntry1, collection);
			AssertEquals(2, collection.Count);

			// Filter on "FRT"
			chargeCodeFilter.Property = Helper.ChargeCodes["FRT"].PK;
			collection.Load(filterCollection.GetFilterQuery(new[] { chargeCodeFilter }));
			AssertCollectionNotContains("local entry with other charge code is not loaded", localEntry1, collection);
			AssertCollectionContains(localEntry2, collection);
			AssertCollectionNotContains("global entry is not loaded", globalEntry1, collection);
			AssertEquals(1, collection.Count);
		}

		#region UnitFactorFilter

		public void TestUnitFactorFilter_CTN()
		{
			foreach (var category in UnitFactorRateLinesLookupsTest.UnitFactorCNTExpectedList)
			{
				var actualFactors = GetUnitFactors(RatingConstants.RatingHeaderTypes.ClientRate, category);
				AssertCollectionContains(UnitFactorList.Codes.CTN, actualFactors);

				actualFactors = GetUnitFactors(RatingConstants.RatingHeaderTypes.Tariff, category);
				AssertCollectionContains(UnitFactorList.Codes.CTN, actualFactors);

				actualFactors = GetUnitFactors(RatingConstants.RatingHeaderTypes.Quote, category);
				AssertCollectionContains(UnitFactorList.Codes.CTN, actualFactors);

				actualFactors = GetUnitFactors(RatingConstants.RatingHeaderTypes.Costing, category);
				AssertCollectionNotContains(UnitFactorList.Codes.CTN, actualFactors);
			}
		}

		public void TestUnitFactorFilter_InnerPack()
		{
			var forwardingCategories = new[]
			{
				RatingConstants.RateCategory.AIR,
				RatingConstants.RateCategory.FCL,
				RatingConstants.RateCategory.LCL,
				RatingConstants.RateCategory.ORG,
				RatingConstants.RateCategory.DST,
			};

			var supportedRateTypes = new[]
			{
				RatingConstants.RatingHeaderTypes.ClientRate,
				RatingConstants.RatingHeaderTypes.Tariff,
				RatingConstants.RatingHeaderTypes.Quote
			};

			foreach (var rateType in supportedRateTypes)
			{
				foreach (var category in forwardingCategories)
				{
					var actualFactors = GetUnitFactors(rateType, category);
					AssertCollectionContains(UnitFactorList.Codes.InnerPack, actualFactors);
				}
			}

			var actualFactors2 = GetUnitFactors(RatingConstants.RatingHeaderTypes.Costing, RatingConstants.RateCategory.AIR);
			AssertCollectionNotContains(
				"It is not supported for costing",
				UnitFactorList.Codes.InnerPack,
				actualFactors2
			);

			actualFactors2 = GetUnitFactors(RatingConstants.RatingHeaderTypes.ClientRate, RatingConstants.RateCategory.WHS);
			AssertCollectionNotContains(
				"It is not supported in non forwarding rates",
				UnitFactorList.Codes.InnerPack,
				actualFactors2
			);
		}

		public void TestUnitFactorFilter_Client()
		{
			AssertUnitFactorFilter(RatingConstants.RatingHeaderTypes.ClientRate, RatingConstants.RateCategory.WHS,
				$"{UnitFactorList.Codes.BCN}, {UnitFactorList.Codes.SCN}, {UnitFactorList.Codes.ProductLine}, {UnitFactorList.Codes.PackageLine}");

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertUnitFactorFilter(RatingConstants.RatingHeaderTypes.ClientRate, RatingConstants.RateCategory.WHS,
					$"{UnitFactorList.Codes.BCN}, {UnitFactorList.Codes.SCN}, {UnitFactorList.Codes.PacksWeight}, {UnitFactorList.Codes.ProductLine}, {UnitFactorList.Codes.PackageLine}");
			}
		}

		public void TestUnitFactorFilter_CompanyTariff()
		{
			AssertUnitFactorFilter(RatingConstants.RatingHeaderTypes.Tariff, RatingConstants.RateCategory.WHS,
				$"{UnitFactorList.Codes.BCN}, {UnitFactorList.Codes.SCN}, {UnitFactorList.Codes.ProductLine}, {UnitFactorList.Codes.PackageLine}");

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertUnitFactorFilter(RatingConstants.RatingHeaderTypes.Tariff, RatingConstants.RateCategory.WHS,
					$"{UnitFactorList.Codes.BCN}, {UnitFactorList.Codes.SCN}, {UnitFactorList.Codes.PacksWeight}, {UnitFactorList.Codes.ProductLine}, {UnitFactorList.Codes.PackageLine}");
			}
		}

		public void TestUnitFactorFilter_Quote()
		{
			AssertUnitFactorFilter(RatingConstants.RatingHeaderTypes.Quote, RatingConstants.RateCategory.WHS,
				$"{UnitFactorList.Codes.BCN}, {UnitFactorList.Codes.SCN}, {UnitFactorList.Codes.ProductLine}, {UnitFactorList.Codes.PackageLine}");

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertUnitFactorFilter(RatingConstants.RatingHeaderTypes.Quote, RatingConstants.RateCategory.WHS,
					$"{UnitFactorList.Codes.BCN}, {UnitFactorList.Codes.SCN}, {UnitFactorList.Codes.PacksWeight}, {UnitFactorList.Codes.ProductLine}, {UnitFactorList.Codes.PackageLine}");
			}
		}

		public void TestUnitFactorFilter_Costing()
		{
			AssertUnitFactorFilter(RatingConstants.RatingHeaderTypes.Costing, RatingConstants.RateCategory.WHS,
				$"{UnitFactorList.Codes.ProductLine}, {UnitFactorList.Codes.PackageLine}");

			using (RatingDataRegistry.Instance.EnableWarehouseUnitFactorRatesDevelopment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertUnitFactorFilter(RatingConstants.RatingHeaderTypes.Costing, RatingConstants.RateCategory.WHS,
				$"{UnitFactorList.Codes.PacksWeight}, {UnitFactorList.Codes.ProductLine}, {UnitFactorList.Codes.PackageLine}");
			}
		}

		public void TestUnitFactorFilter_IntercompanyTariff()
		{
			AssertUnitFactorFilter(RatingConstants.RatingHeaderTypes.IntercompanyTariff, RatingConstants.RateCategory.WHS,
				$"{UnitFactorList.Codes.SAM}");
		}

		IEnumerable<string> GetUnitFactors(string ratingHeaderType, string rateCategory)
		{
			var filterCollection = new ModuleFilterCollection();
			var rateLineFilters = new RateLineModuleFilters(Factory, ratingHeaderType, rateCategory, false);
			rateLineFilters.AddForRateEntryFilter(filterCollection);

			var unitFactorFilter = (ModuleTextFilter)filterCollection.Single(x => x.Code == RateLineModuleFilters.Constants.Codes.UnitFactor);
			return ((ReadOnlyCodeDescriptionPairList)unitFactorFilter.List).GetAllCodes();
		}

		void AssertUnitFactorFilter(string ratingHeaderType, string rateCategory, string expectedUnitFactors)
		{
			var filterCollection = new ModuleFilterCollection();
			var rateLineFilters = new RateLineModuleFilters(Factory, ratingHeaderType, rateCategory, false);
			rateLineFilters.AddForRateEntryFilter(filterCollection);

			var unitFactorFilter = (ModuleTextFilter)filterCollection.Single(x => x.Code == RateLineModuleFilters.Constants.Codes.UnitFactor);
			AssertEquals(expectedUnitFactors, ((ReadOnlyCodeDescriptionPairList)unitFactorFilter.List).CodesAsString);
		}

		#endregion
	}
}
