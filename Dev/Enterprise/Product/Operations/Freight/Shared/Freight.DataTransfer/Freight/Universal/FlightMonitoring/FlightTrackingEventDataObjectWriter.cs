using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.DataTransfer.Universal.FlightMonitoring
{
	public static class EventContextType
	{
		public const string TransportLeg = "TransportLeg";
	}

	public class FlightTrackingEventDataObjectWriter : EventDataObjectWriter
	{
		public FlightTrackingEventDataObjectWriter(IDataWritingManager writeManager, ITransportParent parent)
			: base(writeManager)
		{
			this.parent = parent;
		}
		readonly ITransportParent parent;

		protected override void PopulateDataObject(BaseStmALog logBO, UniversalEvent logData)
		{
			base.PopulateDataObject(logBO, logData);

			if (parent is CommonConsol consol && consol.IsAllDataValidForFlightTrackingSubscription)
			{
				if (!consol.JK_CoLoadMasterBill.IsEmpty)
				{
					logData.ContextCollection.Add(GetContext(UniversalEvent.ContextTypes.CoLoadBillNumber, consol.JK_CoLoadMasterBill));
				}

				if (!consol.JK_CoLoadBookingReference.IsEmpty)
				{
					logData.ContextCollection.Add(GetContext(UniversalEvent.ContextTypes.CoLoadBookingReference, consol.JK_CoLoadBookingReference));
				}

				if (!consol.Creditor.C1CCode.IsEmpty)
				{
					logData.ContextCollection.Add(GetContext(UniversalEvent.ContextTypes.CoLoadWithC1CCode, consol.Creditor.C1CCode));
				}

				if (!consol.Creditor.OH_FullName.IsEmpty)
				{
					logData.ContextCollection.Add(GetContext(UniversalEvent.ContextTypes.CoLoadWithName, consol.Creditor.OH_FullName));
				}
			}

			foreach (var transport in parent.Transports.OfType<Transport>())
			{
				if (transport.TransportMode == Core.Constants.TransportModes.Air)
				{
					logData.ContextCollection.Add(GetContext(transport));
				}
			}
		}

		Context GetContext(Transport transport)
		{
			var context = new Context()
			{
				Type = new ContextType() { Type = EventContextType.TransportLeg },
				Value = transport.JW_LegOrder.ToString(),
				SubContextCollection = new List<Context>()
			};

			var flightNumber = transport.JW_VoyageFlight;
			var portOfLoadingIATACode = transport.LoadPort?.RL_IATA ?? string.Empty;
			var etd = transport.JW_ETD;
			var portOfDischargeIATACode = transport.DiscPort?.RL_IATA ?? string.Empty;
			var eta = transport.JW_ETA;

			context.SubContextCollection.Add(GetContext(UniversalEvent.ContextTypes.TransportMode, transport.JW_TransportMode));

			if (!string.IsNullOrWhiteSpace(portOfLoadingIATACode))
			{
				context.SubContextCollection.Add(GetContext(UniversalEvent.ContextTypes.OriginIATAAirportCode, portOfLoadingIATACode));
			}

			if (!string.IsNullOrWhiteSpace(portOfDischargeIATACode))
			{
				context.SubContextCollection.Add(GetContext(UniversalEvent.ContextTypes.DestinationIATAAirportCode, portOfDischargeIATACode));
			}

			if (!eta.IsEmpty)
			{
				context.SubContextCollection.Add(GetContext(UniversalEvent.ContextTypes.EstimatedTimeOfArrival, eta.ToISO8601ShortDateString()));
			}

			if (!etd.IsEmpty)
			{
				context.SubContextCollection.Add(GetContext(UniversalEvent.ContextTypes.EstimatedTimeOfDeparture, etd.ToISO8601ShortDateString()));
			}

			if (!flightNumber.IsEmpty)
			{
				context.SubContextCollection.Add(GetContext(UniversalEvent.ContextTypes.FlightNumber, flightNumber));
			}

			return context;
		}

		Context GetContext(UniversalEvent.ContextTypes contextType, ZString contextValue)
		{
			return new Context
			{
				Type = new ContextType() { Type = contextType.ToString() },
				Value = contextValue
			};
		}
	}
}
