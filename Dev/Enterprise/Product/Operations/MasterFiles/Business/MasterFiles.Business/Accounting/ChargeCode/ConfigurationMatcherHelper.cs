using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Integration.Accounting;

namespace Enterprise.MasterFiles.Business
{
	public static class ConfigurationMatcherHelper
	{
		public class ConfigurationMatcherParameters
		{
			public CostSell CostOrSell;
			public ZString JobType = AccountingMasterFilesConstants.JobTypes.NonJobRelated;
			public Directions Direction;
			public ZString TransportMode;
			public ILocation Origin;
			public ILocation Destination;
			public GlbBranch Branch;

			public RefCountry Country => Branch?.Company?.Country ?? GlbCompany.CurrentCompany.Country;
		}

		public static Directions RecalculateDirectionBasedOnOriginAndDestination(this AccChargeCode chargeCode, ConfigurationMatcherParameters parameters)
		{
			Directions result = parameters.Direction;
			if (parameters.Origin != null &&
				parameters.Destination != null &&
				parameters.Origin.Country != null &&
				parameters.Destination.Country != null
				&& !chargeCode.IsGlobal)
			{
				var countryCode = chargeCode.Company.GC_RN_NKCountryCode;
				if (parameters.Origin.Country.PK == parameters.Destination.Country.PK && parameters.Origin.Country.Code == countryCode)
				{
					result = Directions.Domestic;
				}
				else if (chargeCode.Company.Country.IsPartOfEuropeanUnion)
				{
					if (parameters.Origin.Country.Code == countryCode ||
						parameters.Destination.Country.Code == countryCode)
					{
						if (parameters.Origin.Country.IsPartOfEuropeanUnion && !parameters.Destination.Country.IsPartOfEuropeanUnion)
						{
							result = Directions.Export;
						}
						else if (parameters.Destination.Country.IsPartOfEuropeanUnion && !parameters.Origin.Country.IsPartOfEuropeanUnion)
						{
							result = Directions.Import;
						}
					}
					else
					{
						result = Directions.CrossTrade;
					}
				}
				else if (parameters.Origin.Country.Code != countryCode &&
						parameters.Destination.Country.Code != countryCode)
				{
					result = Directions.CrossTrade;
				}
			}
			return result;
		}

		public static IZType[] GetDirectionFallBackCodes(ZString directionValue, ConfigurationMatcherParameters parameters)
		{
			List<IZType> result = new List<IZType>();
			result.Add(directionValue);

			bool shouldIncludeOtherByDefault = parameters.Country.IsPartOfEuropeanUnion &&
												parameters.Origin != null && parameters.Origin.Country != null &&
												parameters.Destination != null && parameters.Destination.Country != null &&
												(parameters.Origin.Country.Code == parameters.Country.Code ||
												parameters.Destination.Country.Code == parameters.Country.Code) &&
												(parameters.Direction == Directions.Import || parameters.Direction == Directions.Export);
			if (shouldIncludeOtherByDefault)
			{
				result.Add((ZString)AccChargeTaxOverride.DirectionType_Other);
			}

			result.Add((ZString)AccChargeTaxOverride.ALL);

			return result.ToArray();
		}

		public static ConfigurationMatcherParameters GetParameters(CostSell costOrSell, ZString jobType, Directions direction, ZString transportMode, GlbBranch branch, ILocation origin, ILocation destination)
		{
			return new ConfigurationMatcherParameters
			{
				CostOrSell = costOrSell,
				JobType = jobType,
				Direction = direction,
				TransportMode = transportMode,
				Origin = origin,
				Destination = destination,
				Branch = branch
			};
		}
	}
}
