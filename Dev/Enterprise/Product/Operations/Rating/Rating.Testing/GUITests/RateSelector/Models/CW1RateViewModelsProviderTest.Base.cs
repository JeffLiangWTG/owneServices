using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI;
using Enterprise.Rating.GUI.RateSelector;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Rating.GUI.RateSelector.Services;
using Enterprise.RatingTests.Testing;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Rating.Testing.GUITests.RateSelector.Models.CW1RateViewModelsProviderTest
{
	public abstract class BaseTest : BaseRatingIntegrationTest
	{
		public abstract bool IsContainerised { get; }

		#region When only RateService results are expected

		public void TestGetRates_CW1RatesShouldNotBeLoaded_WhenCGReferenceFilterUsed()
		{
			Helper.NewCosting(null).AddRateEntryWithFlatRateLine("AIR", "LSE", DefaultOrigin, DefaultDestination, "FRT", 100m);
			Factory.Save();

			using (_Rating.Start(new TestInteractor()))
			using (_Rating.StartCost())
			{
				var (provider, filter, _) = CreateProviderAndFilter();

				var cgReferenceTextFilter = filter.AddFilterStrip<WiseRatesModuleTextFilter>(RateEntryFilterUtility.Constants.Codes.CGReference);
				cgReferenceTextFilter.Property = "ABC";

				var rates = provider.GetRatesAsync(filter).GetAwaiter().GetResult().Cast<CW1RateViewModel>();

				AssertEquals("Expected no rates to be loaded when the CGReference filter is used.", 0, rates.Count());
			}
		}

		#endregion

		#region GetRates Assertion

		protected MemoryLogger AssertGetRatesCore(
			string message,
			IEnumerable<TestRate> existingRates,
			TestFilters filterValues,
			IEnumerable<object> expectedRates,
			IEnumerable<(string containerType, string commodityCode)> containerCommodityTabs = null,
			Func<CW1RateViewModel, object> propertiesToCompare = null)
		{
			containerCommodityTabs = containerCommodityTabs ?? Enumerable.Empty<(string, string)>();

			// Creating All Organizations
			var organizations = existingRates
				.SelectMany(x => new[] { x.ServiceProvider, x.Carrier })
				.Union(filterValues.ServiceProviders ?? Enumerable.Empty<string>())
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Distinct()
				.ToDictionary(x => x, x => Helper.NewOrgHeader(x));

			// Creating all Container Types
			var containerTypes = containerCommodityTabs.Select(x => x.containerType)
				.Union(existingRates.Select(x => x.ContainerType))
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Distinct()
				.ToDictionary(x => x, x => Helper.Containers[x]);

			// Creating all Commodity Codes
			var commodities = filterValues.Commodities
				.Union(containerCommodityTabs.Select(x => x.commodityCode)
					.Except(filterValues.Commodities.Select(x => x.Code))
					.Select(x => new TestCommodity { Code = x }))
				.Where(x => x != null && !string.IsNullOrWhiteSpace(x.Code) && x.Code != "GEN")
				.Distinct()
				.ToDictionary(x => x.Code, x => Helper.NewCommodity(x.Code, x.Group));

			// Creating Existing Rates
			foreach (var groupedByCosting in existingRates.GroupBy(x => x.ServiceProvider))
			{
				var serviceProvider = string.IsNullOrWhiteSpace(groupedByCosting.Key)
					? null
					: organizations[groupedByCosting.Key];
				var costing = Helper.NewCosting(serviceProvider);

				foreach (var groupedByEntry in groupedByCosting.ToList().GroupBy(x => x.RateEntryKey))
				{
					var firstEntryInGroup = groupedByEntry.First();
					if (serviceProvider != null && !string.IsNullOrWhiteSpace(firstEntryInGroup.CarrierServiceLevel) &&
						serviceProvider.MiscServ.CarrierServiceLevels.Cast<OrgCarrierServiceLevel>().All(x => x.PL_Code != firstEntryInGroup.CarrierServiceLevel))
					{
						var carrierServiceLevel = serviceProvider.MiscServ.CarrierServiceLevels.AddNew();
						carrierServiceLevel.PL_Code = firstEntryInGroup.CarrierServiceLevel;
						carrierServiceLevel.PL_CarrierServiceLevelDescription = "Carrier Service Level";
					}

					var rateMode = string.IsNullOrWhiteSpace(firstEntryInGroup.ContainerType) ? "LSE" : "ULD";

					var entry = costing.AddRateEntry(firstEntryInGroup.Category, rateMode, firstEntryInGroup.Origin ?? DefaultOrigin, firstEntryInGroup.Destination ?? DefaultDestination);
					entry.TI_OH_TransportProvider = string.IsNullOrWhiteSpace(firstEntryInGroup.Carrier) ? ZGuid.Empty : organizations[firstEntryInGroup.Carrier].PK;
					entry.TI_PL_NKCarrierServiceLevel = firstEntryInGroup.CarrierServiceLevel ?? "";
					entry.TI_ContractNumber = firstEntryInGroup.ContractNumber ?? "";
					entry.TI_RC = rateMode == "ULD" && containerTypes.TryGetValue(firstEntryInGroup.ContainerType, out var container) ? container.PK : ZGuid.Empty;
					entry.TI_RH_NKCommodityCode = firstEntryInGroup.CommodityCode ?? "";
					entry.TI_Mode = !string.IsNullOrWhiteSpace(firstEntryInGroup.Mode) ? firstEntryInGroup.Mode : rateMode;
					entry.RateLines.RemoveAndDeleteAll();

					foreach (var line in groupedByEntry.ToList())
					{
						entry.AddRateLine(line.ChargeCode, "FLT").GetCalculator<FlatCalculator>().BaseRate = line.BaseRate;
					}
				}
			}

			Factory.Save();

			using (_Rating.Start(new TestInteractor()))
			using (_Rating.StartCost())
			{
				var (provider, filter, logger) = CreateProviderAndFilter(
					filterValues.Origin,
					filterValues.Destination,
					filterValues.Mode,
					containerCommodityTabs.Select(x =>
						(containerTypes.TryGetValue(x.containerType, out var container) ? container.PK : ZGuid.Empty,
						x.commodityCode))
					);

				// Filling Filter Values
				foreach (var serviceProviderFilterValue in filterValues.ServiceProviders)
				{
					var serviceProviderFilter = filter.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Description.CarrierTransportProvider);
					((ModuleGuidFilter)serviceProviderFilter.CurrentModuleFilter).Property = organizations[serviceProviderFilterValue].PK;
				}
				foreach (var carrierServiceLevelFilterValue in filterValues.CarrierServiceLevels)
				{
					var carrierServiceLevelFilter = filter.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Codes.CarrierServiceLevel);
					((ModuleTextFilter)carrierServiceLevelFilter.CurrentModuleFilter).Property = carrierServiceLevelFilterValue;
				}
				foreach (var contractNumberFilterValue in filterValues.ContractNumbers)
				{
					var contractNumberFilter = filter.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Description.CarrierContractNumber);
					((ModuleTextFilter)contractNumberFilter.CurrentModuleFilter).Property = contractNumberFilterValue;
				}

				if (IsContainerised)
				{
					foreach (var universalCommodityGroupFilterValue in filterValues.Commodities.Select(x => x.Group).Where(x => !string.IsNullOrWhiteSpace(x)))
					{
						var universalCommodityGroupFilter = filter.FilterStrips.AddNew(RateEntryFilterUtility.Constants.Description.UniversalCommodityGroup);
						((ModuleTextFilter)universalCommodityGroupFilter.CurrentModuleFilter).Property = universalCommodityGroupFilterValue;
					}
				}

				// Main action (GetRates) to test
				var rates = provider.GetRatesAsync(filter).GetAwaiter().GetResult().Cast<CW1RateViewModel>();
				var actualRates = propertiesToCompare != null
					? rates.Select(propertiesToCompare)
					: rates;
				AssertObjectListContainsExpectedValues(expectedRates, actualRates);

				return logger;
			}
		}

		void AssertObjectListContainsExpectedValues(IEnumerable<object> expected, IEnumerable<object> actual)
		{
			foreach(var expectedItem in expected)
			{
				var found = actual.Any(actualItem => AreObjectsEqualByValue(expectedItem, actualItem));
				if (!found)
				{
					Assert($"Expected item not found: {expectedItem.ToJSON()}", false);
				}
			}
			Assert("All expected items found in actual list.", true);
		}

		public static bool AreObjectsEqualByValue(object obj1, object obj2)
		{
			if (obj1 == null || obj2 == null)
			{
				return obj1 == obj2;
			}

			var type1 = obj1.GetType();
			var type2 = obj2.GetType();

			foreach (var property1 in type1.GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				var property2 = type2.GetProperty(property1.Name);
				if (property2 == null)
				{
					continue;
				}

				var val1 = property1.GetValue(obj1);
				var val2 = property2.GetValue(obj2);

				if (val1 == null || val2 == null)
				{
					if (val1 != val2)
					{
						return false;
					}
					continue;
				}

				if (val1 is IEnumerable<decimal> decArr1 && val2 is IEnumerable<decimal> decArr2)
				{
					if (!decArr1.Select(NormalizeDecimal).OrderBy(x => x).SequenceEqual(decArr2.Select(NormalizeDecimal).OrderBy(x => x)))
					{
						return false;
					}
				}
				else if (!val1.Equals(val2))
				{
					return false;
				}
			}

			return true;
		}

		static decimal NormalizeDecimal(decimal d)
		{
			return d / 1.000000000000000000000000000000000m;
		}

		protected (CW1RateViewModelsProvider provider, RateSelectorFilterStripBusinessObject filter, MemoryLogger logger) CreateProviderAndFilter
			(
			string origin = DefaultOrigin,
			string destination = DefaultDestination,
			FreightMode mode = FreightMode.ULD,
			IEnumerable<(ZGuid containerTypePK, string commodityCode)> containerCommodityTabs = null
			)
		{
			var freightMode = IsContainerised ? mode : FreightMode.LSE;
			var logger = new MemoryLogger();
			var context = RatingContext.CreateForManualSelect(new LoggerDecorator(logger), new Mock<IDialogService>().Object);
			var provider = new CW1RateViewModelsProvider(context, logger);
			var criteria = new TestRatingCriteria(origin, destination, freightMode, 1500, 1, null);
			var filter = new RateSelectorFilterStripBusinessObject(criteria, logger);

			criteria.ShouldApplySpecificAdapterContractNumberFilter = true;

			if (IsContainerised)
			{
				int containerNumber = 0;
				foreach (var tab in containerCommodityTabs ?? Enumerable.Empty<(ZGuid, string)>())
				{
					criteria.RateableMeasures.AddContainerWithCommodityAndNumber(
						tab.containerTypePK,
						tab.commodityCode,
						(++containerNumber).ToString(),
						new MeasureInfo.ContainerInfo());
				}
			}

			return (provider, filter, logger);
		}

		protected class TestRate
		{
			public string Mode { get; set; }
			public string Origin { get; set; }
			public string Destination { get; set; }
			public string ServiceProvider { get; set; }
			public string Carrier { get; set; }
			public string CarrierServiceLevel { get; set; } = "";
			public string ContractNumber { get; set; } = "";
			public string ContainerType { get; set; } = "";
			public string CommodityCode { get; set; } = "";
			public string ChargeCode { get; set; } = "FRT";
			public decimal BaseRate { get; set; } = 1m;
			public string Category { get; set; } = "AIR";

			public string RateEntryKey =>
				string.Join("|", Origin, Destination, Carrier, CarrierServiceLevel, ContractNumber, CommodityCode, ContainerType);
		}

		protected class TestFilters
		{
			public string Origin { get; set; } = DefaultOrigin;
			public string Destination { get; set; } = DefaultDestination;
			public IEnumerable<string> ServiceProviders { get; set; } = Enumerable.Empty<string>();
			public IEnumerable<string> CarrierServiceLevels { get; set; } = Enumerable.Empty<string>();
			/// <summary>
			/// These ContractNumbers represent the ones visible on the filters
			/// in the UI. These are always in CAPITAL LETTERS
			/// </summary>
			public IEnumerable<string> ContractNumbers { get; set; } = Enumerable.Empty<string>();
			public IEnumerable<TestCommodity> Commodities { get; set; } = Enumerable.Empty<TestCommodity>();
			public FreightMode Mode { get; set; } = FreightMode.ULD;
		}

		protected class TestCommodity
		{
			public string Code { get; set; } = "";
			public string Group { get; set; } = "";
		}

		protected Func<CW1RateViewModel, object> CarrierSelector => rate => new
		{
			ServiceProvider = rate.ServiceProviderCode,
			Carrier = rate.CarrierCode,
			ChargeAmounts = rate.Lines.Select(l => (decimal)((FlatCalculator)l.Calculator).BaseRate)
		};

		protected Func<CW1RateViewModel, object> CarrierServiceLevelSelector => rate => new
		{
			rate.CarrierServiceLevel,
			ChargeAmounts = rate.Lines.Select(l => (decimal)((FlatCalculator)l.Calculator).BaseRate)
		};

		protected Func<CW1RateViewModel, object> ContractNumberSelector => rate => new
		{
			rate.ContractNumber,
			ChargeAmounts = rate.Lines.Select(l => (decimal)((FlatCalculator)l.Calculator).BaseRate).ToArray()
		};

		protected Func<CW1RateViewModel, object> CommodityCodeSelector => rate => new
		{
			CommodityCode = rate.Commodities,
			ChargeAmounts = rate.Lines.Select(l => (decimal)((FlatCalculator)l.Calculator).BaseRate).ToArray()
		};

		protected Func<CW1RateViewModel, object> ContainerCodeSelector => rate => new
		{
			rate.ContainerType,
			ChargeAmounts = rate.Lines.Select(l => (decimal)((FlatCalculator)l.Calculator).BaseRate).ToArray()
		};

		protected Func<CW1RateViewModel, object> EmptyContainerCodeSelector => rate => new
		{
			ChargeAmounts = rate.Lines.Select(l => (decimal)((FlatCalculator)l.Calculator).BaseRate).ToArray()
		};

		protected virtual Func<CW1RateViewModel, object> AllFieldsSelector => rate => new
		{
			ServiceProvider = rate.ServiceProviderCode,
			Carrier = rate.CarrierCode,
			ContractNumber = rate.ContractNumber,
			CarrierServiceLevel = rate.CarrierServiceLevel,
			ChargeAmounts = rate.Lines.Select(l => (decimal)((FlatCalculator)l.Calculator).BaseRate)
		};

		#endregion

		#region SetUp/TearDown

		protected override void MasterSetUp()
		{
			base.MasterSetUp();
		}

		protected override void SetUp()
		{
			base.SetUp();

			originalCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			GlbCompany.CurrentCompany.SetCurrency("USD");
		}

		protected override void TearDown()
		{
			base.TearDown();

			GlbCompany.CurrentCompany.SetCurrency(originalCurrency.Code);
		}

		protected override void FinalTearDown()
		{
			base.FinalTearDown();
		}

		RefCurrency originalCurrency;

		#endregion

		protected const string DefaultOrigin = "USLAX";
		protected const string DefaultDestination = "HKHKG";
	}
}
