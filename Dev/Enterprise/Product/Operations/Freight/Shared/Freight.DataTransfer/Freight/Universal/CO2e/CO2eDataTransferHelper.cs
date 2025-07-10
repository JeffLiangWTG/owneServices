using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public static class CO2eDataTransferHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Provider name")]
		internal const string CO2eCalculationProvider = "WTG Greenhouse Gas Emission";

		public static bool IsCO2eResponse(this ITopLevelDataObject dataObject)
		{
			Argument.NotNull(dataObject, nameof(dataObject));
			var dataProvider = dataObject.DataContext?.DataProviderForCodeMapping;

			return dataProvider.GetValueOrDefault().EqualsIgnoringCase(CO2eCalculationProvider);
		}

		public static bool IsCO2eResponse(this UniversalEvent universalEvent)
		{
			return universalEvent?.EventParameters?.MessageType.GetValueOrDefault().EqualsIgnoringCase(CO2eCalculationProvider) ?? false;
		}

		public static bool IsCO2eResponseRejected(this UniversalEvent universalEvent, out string reason)
		{
			if (universalEvent != null && universalEvent.EventType.GetValueOrDefault().EqualsIgnoringCase(AutoEvents.InterchangeRejectedCode) && universalEvent.IsCO2eResponse())
			{
				reason = universalEvent.ContextCollection?.FirstOrDefault(c => string.Equals(c.Type, nameof(UniversalEvent.ContextTypes.FailureReason), System.StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty;
				return true;
			}
			reason = string.Empty;
			return false;
		}

		public static void OnCO2eRejectionEvent(this UniversalEvent eventAdded, ICO2eCalculationSupporter supporter)
		{
			if (eventAdded.IsCO2eResponseRejected(out var reason))
			{
				supporter.OnRejected(reason);
			}
		}

		public static ZString GetCodeWithFallbackToEmpty(this ICodeDataObject dataObject)
		{
			return dataObject == null ? ZString.Empty : dataObject.Code.GetValueOrDefault(ZString.Empty);
		}

		public static bool MatchesUNLOCODataObject(this IRefUNLOCO unloco, UNLOCO unlocoDO)
		{
			if (unloco == null)
			{
				return unlocoDO == null || unlocoDO.Code.GetValueOrDefault().Equals(ZString.Empty);
			}

			var code = unlocoDO == null ? ZString.Empty : unlocoDO.Code.GetValueOrDefault().ToUpper();
			return code.Length == 3 ? code.EqualsIgnoringCase(unloco.RL_IATA) : code.EqualsIgnoringCase(unloco.RL_Code);
		}

		public static TransportLeg ToTransportLegDO(this PrePostCarriageLegWrapper leg, IDataObjectWriterStrategy writerStrategy, BusinessObjectFactory factory)
		{
			if (leg.IsMainCarriage && leg.From?.IsPort == true && leg.To?.IsPort == true)
			{
				return new TransportLeg(writerStrategy)
				{
					LegOrder = 0,
					TransportMode = new TransportModeConverter().ToEnumValue(leg.TransportMode),
				};
			}
			var legData = new TransportLeg(writerStrategy)
			{
				LegOrder = 0,
				TransportMode = new TransportModeConverter().ToEnumValue(leg.TransportMode),
				DepartureFrom = leg.From.ToUXmlOrganizationAddress(writerStrategy, factory),
				ArrivalAt = leg.To.ToUXmlOrganizationAddress(writerStrategy, factory)
			};
			return legData;
		}

		public static OrganizationAddress ToUXmlOrganizationAddress(this IPrePostCarriageLocation location, IDataObjectWriterStrategy strategy, BusinessObjectFactory factory)
		{
			if (location is null || location.IsEmpty)
			{
				return null;
			}

			if (!location.IsPort)
			{
				return location.Address.ToUXmlOrganizationAddress(strategy, factory);
			}

			var unlocoLoader = new RefUNLOCO.Loader(factory);
			var address = new OrganizationAddress(strategy)
			{
				Port = UNLOCO.New(unlocoLoader.Load(location.UNLOCO)),
				AddressType = nameof(DocAddressType.None)
			};
			return address;
		}

		public static OrganizationAddress ToUXmlOrganizationAddress(this ISupportWebAddressValidation address, IDataObjectWriterStrategy strategy, BusinessObjectFactory factory)
		{
			var unlocoLoader = new RefUNLOCO.Loader(factory);
			var addressDO = new OrganizationAddress(strategy);
			addressDO.Postcode = address.Postcode;
			addressDO.City = address.City;
			addressDO.Country = Country.New(address.Country);
			addressDO.Port = UNLOCO.New(unlocoLoader.Load(address.ClosestPort));
			addressDO.GeoLocation = ValidAddress(address) ? GeoLocation.New(address.GeoLocation) : null;
			addressDO.AddressType = nameof(DocAddressType.None);
			return addressDO;
		}

		static bool ValidAddress(ISupportWebAddressValidation address)
		{
			string status = address.ValidationStatus;
			return status switch
			{
				AddressValidationStatus.Invalid or
				AddressValidationStatus.ToBeVerified or
				AddressValidationStatus.CountryNotAvailable => false,
				_ => address.GeoLocation != ZGeography.CreatePoint(0, 0)
			};
		}
	}
}
