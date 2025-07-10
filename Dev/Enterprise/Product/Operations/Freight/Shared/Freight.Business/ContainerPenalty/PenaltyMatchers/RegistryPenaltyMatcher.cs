using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Enterprise.Registry.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	class RegistryPenaltyMatcher : IContainerPenaltyMatcher
	{
		public PenaltyMatcherType MatcherType => PenaltyMatcherType.Registry;

		public IContainerPenaltyMatchResult MatchDetention(IContainerPenaltyMatchFilter filter)
		{
			var registry = FreightDataRegistry.Instance;
			var freeDays = filter.Direction == ContainerDetentionDirection.Import
				? registry.DefaultContainerDetentionFreeDaysForImport.GetFallBackValueAtAllLevels(filter.Company == null ? Guid.Empty : filter.Company.PK.ToGuid(), Guid.Empty, Guid.Empty).ValidFreeDays
				: registry.DefaultContainerDetentionFreeDaysForExport.GetFallBackValueAtAllLevels(filter.Company == null ? Guid.Empty : filter.Company.PK.ToGuid(), Guid.Empty, Guid.Empty).ValidFreeDays;

			if (freeDays == null)
			{
				return null;
			}

			return new RegistryPenaltyResult(Convert.ToByte(freeDays.Value), GetFreeDayType(filter), ContainerPenaltyPenaltyType.Codes.Detention, ContainerPenaltyCreditorType.Codes.Carrier, null, null);
		}

		public IContainerPenaltyMatchResult MatchMDD(IContainerPenaltyMatchFilter filter)
		{
			return null;
		}

		public IEnumerable<IContainerPenaltyMatchResult> MatchPenalties(IEnumerable<IContainerPenaltyMatchFilter> detentionFilters, IEnumerable<IContainerPenaltyMatchFilter> storageFilters, IEnumerable<IContainerPenaltyMatchFilter> mddFilters)
		{
			var results = new List<IContainerPenaltyMatchResult>();

			foreach (var filter in detentionFilters)
			{
				if (MatchDetention(filter) is IContainerPenaltyMatchResult matchResult)
				{
					results.Add(matchResult);
				}
			}

			foreach (var filter in storageFilters)
			{
				filter.CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				if (MatchStorage(filter) is IContainerPenaltyMatchResult matchResult)
				{
					results.Add(matchResult);
				}
			}

			foreach (var filter in storageFilters)
			{
				filter.CreditorType = ContainerPenaltyCreditorType.Codes.CTO;
				if (MatchStorage(filter) is IContainerPenaltyMatchResult matchResult)
				{
					results.Add(matchResult);
				}
			}

			return results;
		}

		public IContainerPenaltyMatchResult MatchStorage(IContainerPenaltyMatchFilter filter)
		{
			if (!filter.CreditorType.IsEmpty
				&& filter.CreditorType != ContainerPenaltyCreditorType.Codes.CTO)
			{
				return null;
			}

			var registry = FreightDataRegistry.Instance;
			var freeDays = filter.Direction == ContainerDetentionDirection.Import
				? registry.DefaultFreeCTOStorageDaysForImport.GetFallBackValueAtAllLevels(filter.Company == null ? Guid.Empty : filter.Company.PK.ToGuid(), Guid.Empty, Guid.Empty).ValidFreeDays
				: registry.DefaultFreeCTOStorageDaysForExport.GetFallBackValueAtAllLevels(filter.Company == null ? Guid.Empty : filter.Company.PK.ToGuid(), Guid.Empty, Guid.Empty).ValidFreeDays;

			if (freeDays == null)
			{
				return null;
			}

			return new RegistryPenaltyResult(Convert.ToByte(freeDays.Value), GetFreeDayType(filter), ContainerPenaltyPenaltyType.Codes.Storage, filter.CreditorType, null, null);
		}

		ZString GetFreeDayType(IContainerPenaltyMatchFilter filter)
		{
			return filter.ProcessType == Core.Constants.ContainerPenaltyProcessType.Import
					? (ZString)ContainerDetentionFreeDayType.CTOAvailable
					: ZString.Empty;
		}

		record RegistryPenaltyResult(ZByte FreeDays, ZString FreeDayType, ZString PenaltyType, ZString CreditorType, IContainerPenaltyDayExclusion FreeDayExclusion, IContainerPenaltyDayExclusion DurationExclusion) : IContainerPenaltyMatchResult;
	}
}
