using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.Agency.Documents.DocDataObjects
{
	public static class UnlocoExtensions
	{
		public static string GetDetailedPortName(IRefUNLOCO refUnloco)
		{
			if (refUnloco is RefUNLOCO typedRefUnloco)
			{
				var result = new ZStringBuilder();
				result.AppendIfNotEmpty(typedRefUnloco.RL_PortName);
				if ((typedRefUnloco.Country?.Code ?? ZString.Empty) == Core.Constants.CountryCodes.UnitedStates)
				{
					result.AppendIfNotEmpty(typedRefUnloco.CountryStates?.RW_Code);
				}

				result.AppendIfNotEmpty(typedRefUnloco.Country?.Description ?? ZString.Empty);
				ZString detailedPortName = result.ToStringWithDelimiterBetweenAppends(", ");

				return detailedPortName.SubstringSafe(0, AutoRefUNLOCO.Schema.RL_PortNameMaxLength);
			}

			return refUnloco?.RL_PortName ?? string.Empty;
		}

		public static Unloco CreateFromUNLOCO(IContext context, UNLOCO universalUnloco, bool useDetailedPortName = false)
		{
			var result = new Unloco(context.Factory, context.Unlocos, context.Countries);
			result.Code = (universalUnloco?.Code).GetValueOrDefault();

			var universalUnlocoName = (universalUnloco?.Name).GetValueOrDefault();
			if (!universalUnlocoName.IsEmpty)
			{
				result.Name = universalUnlocoName;
			}

			var countryCode = (universalUnloco?.Code).GetValueOrDefault().SubstringSafe(0, 2);
			if (!countryCode.IsEmpty)
			{
				result.Country.Code = countryCode;
			}

			return useDetailedPortName
				? result.WithCustomNameProvider(GetDetailedPortName)
				: result;
		}
	}
}
