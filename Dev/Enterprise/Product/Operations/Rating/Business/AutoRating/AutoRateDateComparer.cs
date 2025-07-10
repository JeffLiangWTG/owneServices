using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business;

public class AutoRateDateComparer : IComparer<IAutoRateDate>
{
	readonly BusinessObjectFactory factory;

	public AutoRateDateComparer(BusinessObjectFactory factory)
	{
		this.factory = factory;
	}

	public int Compare(IAutoRateDate setting1, IAutoRateDate setting2)
	{
		if (setting1 is not null && setting2 is not null)
		{
			var compareRateType = CompareParameterGeneric(setting1.RateType, setting2.RateType, IsGenericRateType);
			if (compareRateType != 0)
			{
				return compareRateType;
			}

			var compareLocation = CompareLocationsStrings(setting1.Location, setting2.Location);
			if (compareLocation != 0)
			{
				return compareLocation;
			}

			var compareContainerMode = CompareParameterGeneric(setting1.ContainerMode, setting2.ContainerMode, IsGenericContainerMode);
			if (compareContainerMode != 0)
			{
				return compareContainerMode;
			}

			var compareMode = CompareParameterGeneric(setting1.Mode, setting2.Mode, IsGenericTransportMode);
			if (compareMode != 0)
			{
				return compareMode;
			}

			var compareDirection = CompareParameterGeneric(setting1.DirectionCode, setting2.DirectionCode, IsGenericDirectionCode);
			if (compareDirection != 0)
			{
				return compareDirection;
			}

			return CompareParameterGeneric(setting1.JobType, setting2.JobType, IsGenericJobType);
		}

		return 0;
	}

	bool IsGenericRateType(ZString value) => string.IsNullOrEmpty(value) || value == JobRateTypes.Codes.All;

	bool IsGenericContainerMode(ZString value) =>
		string.IsNullOrEmpty(value) || value == Constants.ContainerModes.All || value == Constants.ContainerModes.Other;

	bool IsGenericTransportMode(ZString value) => string.IsNullOrEmpty(value) || value == JobConfigurationSelectorLookups.ModeAdditionalCodes.All;

	bool IsGenericDirectionCode(ZString value) =>
		string.IsNullOrEmpty(value) || value == Constants.FreightShipmentDirection.Code.All || value == Constants.FreightShipmentDirection.Code.Other;

	bool IsGenericJobType(ZString value) => string.IsNullOrEmpty(value) || value == JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All;

	int CompareParameterGeneric(ZString value1, ZString value2, Func<ZString, bool> isGeneric)
	{
		if (isGeneric(value1) == isGeneric(value2))
		{
			return 0;
		}

		return isGeneric(value1) ? -1 : 1;
	}

	int CompareLocationsStrings(ZString lhs, ZString rhs) =>
		GetLocationRank(LocationHelper.GetLocationFromString(lhs, factory)).CompareTo(GetLocationRank(LocationHelper.GetLocationFromString(rhs, factory)));

	int GetLocationRank(ILocation location)
	{
		var rank = 0;

		if (location is RefUNLOCO)
		{
			rank = 8;
		}
		else if (location is RefCountry)
		{
			rank = 6;
		}
		else if (location is RefZoneHeader)
		{
			rank = 4;
		}

		return rank;
	}
}

