using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business.RateSelector
{
	public class CW1RateCombinationContext
	{
		public CW1RateCombinationContext(IRateSelectorFilterValueProvider filter, ILogger logger)
		{
			Logger = logger;
			IsContainerised = filter.OriginalCriteria.IsContainerised;

			var commodityCodesFromFilters = filter.UniversalCommodityGroupsFromFilters
				.SelectMany(x => RefCommodityCode.GetCommodities(filter.Factory, x))
				.Select(x => x.RH_Code)
				.Distinct()
				.ToArray();
			var commodityCodesFromJob = filter.CommodityCodesFromJobContainers.Select(x => new ZString(x)).ToArray();
			IsCommodityCodesFromFilterEmpty = commodityCodesFromFilters.IsNullOrEmpty();
			CommodityCodesFromBothJobAndFilters = commodityCodesFromFilters.IsNullOrEmpty()
				? commodityCodesFromJob
				: commodityCodesFromJob.Intersect(commodityCodesFromFilters).ToArray();

			ContractNumbersFromFilters = filter.ContractNumbersFromFilters.Select(x => new ZString(x).ToUpperInvariant()).ToArray();
			IsContractNumberFilterEmpty = ContractNumbersFromFilters.IsNullOrEmpty();

			CarrierServiceLevelsFromFilters = filter.CarrierServiceLevelsFromFilters.Select(x => new ZString(x)).ToArray();
			IsCarrierServiceLevelFilterEmpty = CarrierServiceLevelsFromFilters.IsNullOrEmpty();

			CarrierPKsFromFilters = filter.CarrierPKs.ToArray();
			IsCarrierFilterEmpty = CarrierPKsFromFilters.IsNullOrEmpty();

			if (IsContainerised)
			{
				ContainerGroups = filter.OriginalCriteria.RateableMeasures
					.GetContainerTypeAndCommodityList()
					.GroupBy(g => (g.ContainerTypePk, g.CommodityCode))
					.Select(g => g.Key)
					.ToArray();
			}
			else
			{
				ContainerGroups = new[] { (ZGuid.Empty, ZString.Empty) };
			}

			FilterMethodsOrderByPriorities = GetFilterMethodsSortedByPriorities().ToArray();
		}

		public ILogger Logger { get; }

		public bool IsContainerised { get; }
		public ZString[] CommodityCodesFromBothJobAndFilters { get; }
		public ZString[] ContractNumbersFromFilters { get; }
		public ZString[] CarrierServiceLevelsFromFilters { get; }
		public ZGuid[] CarrierPKsFromFilters { get; }
		public bool IsCommodityCodesFromFilterEmpty { get; }
		public bool IsContractNumberFilterEmpty { get; }
		public bool IsCarrierServiceLevelFilterEmpty { get; }
		public bool IsCarrierFilterEmpty { get; }

		public (ZGuid ContainerTypePk, ZString CommodityCode)[] ContainerGroups { get; }

		public Func<IRateEntry, IEnumerable<IRateEntry>, bool>[] FilterMethodsOrderByPriorities { get; }

		#region IsApplicableAndMostSpecific

		IEnumerable<Func<IRateEntry, IEnumerable<IRateEntry>, bool>> GetFilterMethodsSortedByPriorities()
		{
			var filterNames = new List<string>();
			var priorities = new[] { RateEntry.Schema.TI_ContractNumber }
				.Concat(Env.Registry.Rating.FreightSearchPriorities.Split(','))
				.ToArray();

			foreach (var columnName in priorities)
			{
				if (RateEntrySchema.TI_ContractNumber.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase))
				{
					filterNames.Add((NoResString)"Contract Number");
					yield return (rate, rates) => IsApplicableAndMostSpecificBasedOnContractNumber(rate, rates);
				}
				else if (IsContainerised && RateEntrySchema.TI_RH_NKCommodityCode.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase))
				{
					filterNames.Add((NoResString)"Commodity Code");
					yield return (rate, rates) => IsApplicableAndMostSpecificBasedOnCommodityCode(rate, rates);
				}
				else if (RateEntrySchema.TI_RS_NKServiceLevel_NI.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase))
				{
					filterNames.Add((NoResString)"Carrier Service Level");
					yield return (rate, rates) => IsApplicableAndMostSpecificBasedOnCarrierServiceLevel(rate, rates);
				}
				else if (RateEntrySchema.TI_OH_TransportProvider.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase))
				{
					filterNames.Add((NoResString)"Carrier");
					yield return (rate, rates) => IsApplicableAndMostSpecificBasedOnCarrier(rate, rates);
				}
			}

			Logger?.Debug($"Filtering Priority: {string.Join(" => ", filterNames)}");
		}

		bool IsApplicableAndMostSpecificBasedOnContractNumber(IRateEntry rate, IEnumerable<IRateEntry> rates)
		{
			// ContractNumbersFromFilters is always in CAPITAL LETTERS
			var isApplicable =
				IsContractNumberFilterEmpty ||
				(!rate.TI_ContractNumber.IsEmpty && ContractNumbersFromFilters.Contains(rate.TI_ContractNumber.ToUpperInvariant())) ||
				(rate.TI_ContractNumber.IsEmpty && rates.All(x => x.TI_ContractNumber.IsEmpty));

			LogApplicableAndMostSpecific(rate, isApplicable, (NoResString)"Contract Number");

			return isApplicable;
		}

		bool IsApplicableAndMostSpecificBasedOnCommodityCode(IRateEntry rate, IEnumerable<IRateEntry> rates)
		{
			var isApplicable =
				IsCommodityCodesFromFilterEmpty ||
				(rate.IsSpecificCommodityCode() && CommodityCodesFromBothJobAndFilters.Contains(rate.TI_RH_NKCommodityCode)) ||
				(rate.IsGeneralCommodityCode() && rates.All(x => x.IsEmptyOrGeneralCommodityCode())) ||
				(rate.IsEmptyCommodityCode() && rates.All(x => x.IsEmptyCommodityCode()));

			LogApplicableAndMostSpecific(rate, isApplicable, (NoResString)"Commodity Code");

			return isApplicable;
		}

		bool IsApplicableAndMostSpecificBasedOnCarrierServiceLevel(IRateEntry rate, IEnumerable<IRateEntry> rates)
		{
			var isApplicable =
				IsCarrierServiceLevelFilterEmpty ||
				(!rate.TI_PL_NKCarrierServiceLevel.IsEmpty && CarrierServiceLevelsFromFilters.Contains(rate.TI_PL_NKCarrierServiceLevel)) ||
				(rate.TI_PL_NKCarrierServiceLevel.IsEmpty && rates.All(x => x.TI_PL_NKCarrierServiceLevel.IsEmpty));

			LogApplicableAndMostSpecific(rate, isApplicable, (NoResString)"Carrier Service Level");

			return isApplicable;
		}

		bool IsApplicableAndMostSpecificBasedOnCarrier(IRateEntry rate, IEnumerable<IRateEntry> rates)
		{
			var isApplicable =
				IsCarrierFilterEmpty ||
				(!rate.TI_OH_TransportProvider.IsEmpty && (CarrierPKsFromFilters.Contains(rate.TI_OH_TransportProvider) || CarrierPKsFromFilters.Contains(rate.ServiceProviderPK()))) ||
				(rate.TI_OH_TransportProvider.IsEmpty && rates.All(x => x.TI_OH_TransportProvider.IsEmpty));

			LogApplicableAndMostSpecific(rate, isApplicable, (NoResString)"Carrier");

			return isApplicable;
		}

		void LogApplicableAndMostSpecific(IRateEntry rate, bool isApplicable, string fieldDescription) =>
			Logger?.Debug($"Entry  {GetLogText(rate)}  is{(isApplicable ? " " : (NoResString)" not ")}applicable and most specific based on {fieldDescription}");

		#endregion

		static string GetLogText(IRateEntry entry)
		{
			return string.Format("{0}-{1}-{2}-{3}",
				entry.ParentRatingHeader.Header?.OH_Code ?? (NoResString)"Standard Cost", // used for log
				entry.TI_ContractNumber.IsEmpty ? "   " : entry.TI_ContractNumber.ToString(),
				entry.TI_PL_NKCarrierServiceLevel.IsEmpty ? "   " : entry.TI_PL_NKCarrierServiceLevel.ToString(),
				entry.TransportProvider?.OH_Code ?? "   ");
		}
	}
}
