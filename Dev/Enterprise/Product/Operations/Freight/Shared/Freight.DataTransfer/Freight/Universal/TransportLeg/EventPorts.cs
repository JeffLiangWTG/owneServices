using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.DataTransfer.Universal
{
	internal class EventLegPorts
	{
		internal EventLegPorts(IXmlEventValueObject xmlEvent, BusinessObjectFactory factory)
		{
			var origins = FindUNLOCO(xmlEvent.Context.LegOriginUNLOCO, xmlEvent.Context.OriginIATAAirportCode, factory);
			var destinations = FindUNLOCO(xmlEvent.Context.LegDestinationUNLOCO, xmlEvent.Context.DestinationIATAAirportCode, factory);

			var universalEvent = xmlEvent as Event;
			if (universalEvent?.EventParameters != null)
			{
				if (origins.IsNullOrEmpty() && IsDepartureEvents(xmlEvent))
				{
					origins = FindUNLOCOByCode(GetParameter(xmlEvent, EventReferenceParameters.Codes.Location), factory);
				}

				if (destinations.IsNullOrEmpty() && xmlEvent.EventType == Events.ArrivalCode)
				{
					destinations = FindUNLOCOByCode(GetParameter(xmlEvent, EventReferenceParameters.Codes.Location), factory);
				}
			}

			LegOrigins = origins;
			LegDestinations = destinations;
		}

		public IEnumerable<RefUNLOCO> LegOrigins { get; private set; }
		public IEnumerable<RefUNLOCO> LegDestinations { get; private set; }

		public bool HasAPort
		{
			get { return !LegOrigins.IsNullOrEmpty() || !LegDestinations.IsNullOrEmpty(); }
		}

		internal static bool IsDepartureEvents(IXmlEventValueObject xmlEvent)
		{
			string eventCode = xmlEvent.EventType;
			return eventCode == AutoEvents.DepartureCode
				|| eventCode == AutoEvents.FreightLoadedCode
				|| eventCode == AutoEvents.BookingConfirmedCode
				|| (eventCode == AutoEvents.StatusUpdatedCode
					&& (string.Equals(GetParameter(xmlEvent, EventReferenceParameters.Codes.Type), (NoResString)"Manifested", StringComparison.OrdinalIgnoreCase)
					|| string.Equals(GetParameter(xmlEvent, EventReferenceParameters.Codes.Type), (NoResString)"Prepared For Loading", StringComparison.OrdinalIgnoreCase)));
		}

		internal static string GetParameter(IXmlEventValueObject xmlEvent, string paramCode)
		{
			var result = string.Empty;

			if (xmlEvent is Event universalEvent)
			{
				try
				{
					result = EventParameters.GetEventParameter(paramCode, universalEvent.EventParameters, universalEvent.EventReference);
				}
				catch (Exception e) when (!e.IsCriticalException()) { }
			}

			return result;
		}

		static IEnumerable<RefUNLOCO> FindUNLOCOByIATACode(ZString iATACode, BusinessObjectFactory factory)
		{
			if (iATACode.IsEmpty && iATACode.Length != 3)
			{
				return Enumerable.Empty<RefUNLOCO>();
			}

			var query = new ZQuery(RefUNLOCOSchema.RL_IATA, iATACode);
			var unlocos = factory.Load<RefUNLOCO>(query);
			return unlocos;
		}

		static IEnumerable<RefUNLOCO> FindUNLOCOByUNLOCOCode(ZString uNLOCOCode, BusinessObjectFactory factory)
		{
			if (uNLOCOCode.IsEmpty)
			{
				return Enumerable.Empty<RefUNLOCO>();
			}

			var query = new ZQuery(RefUNLOCOSchema.RL_Code, uNLOCOCode);
			var unlocos = factory.Load<RefUNLOCO>(query);
			return unlocos;
		}

		static IEnumerable<RefUNLOCO> FindUNLOCO(ZString unlocoCode, ZString iataCode, BusinessObjectFactory factory)
		{
			var result = FindUNLOCOByUNLOCOCode(unlocoCode, factory);

			if (result.IsNullOrEmpty())
			{
				result = FindUNLOCOByIATACode(iataCode, factory);
			}

			return result;
		}

		static IEnumerable<RefUNLOCO> FindUNLOCOByCode(string code, BusinessObjectFactory factory)
		{
			if (string.IsNullOrWhiteSpace(code))
			{
				return Enumerable.Empty<RefUNLOCO>();
			}

			var result = FindUNLOCOByUNLOCOCode(code, factory);

			if (result.IsNullOrEmpty())
			{
				result = FindUNLOCOByIATACode(code, factory);
			}

			return result;
		}
	}
}
