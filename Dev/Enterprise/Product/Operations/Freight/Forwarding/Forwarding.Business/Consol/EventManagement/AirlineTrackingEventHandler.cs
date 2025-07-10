using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using EventConstants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Forwarding.Business
{
	static public class AirlineTrackingEventHandler
	{
		static readonly Regex FlightNumberWithCarrierPrefixRegex = new ("^([A-Z]{2}|[0-9][A-Z]|[A-Z][0-9])[0-9]{1,4}[A-Z]{0,1}$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

		static public bool IsAirTrackingEvent(this ForwardingConsol consol, IStmALog log)
		{
			return log != null
				&& consol != null
				&& !log.SL_IsCancelled
				&& consol.IsAir
				&& new[]
				{
					Events.HandedOverCode,
					Events.BookingConfirmedCode,
					Events.FreightUnloadedCode,
					Events.DepartureCode,
					Events.ArrivalCode
				}.Contains((string)log.SL_SE_NKEvent);
		}

		static public void HandleAirTrackingEvent(this ForwardingConsol consol, IStmALog log)
		{
			if (IsHandOverEvent(log))
			{
				UpdateShipmentDeliveredTime(consol, log);
			}
			else if (IsBookingConfirmationEvent(log))
			{
				consol.HandleBookingConfirmations(consol.BookingConfirmations, log, false);
			}
			else if (IsActualEvent(log))
			{
				consol.HandleActualEvents(consol.ActualEvents, log, false);
			}
		}

		#region HandOver Event

		static bool IsHandOverEvent(IStmALog log) =>
			log.SL_SE_NKEvent == Events.HandedOverCode && !log.SL_IsEstimate;

		static void UpdateShipmentDeliveredTime(ForwardingConsol consol, IStmALog log)
		{
			consol.ShipmentDeliveredTime = log.SL_EventTime;
		}

		static public void UpdateShipmentDeliveredTime(this ForwardingConsol consol)
		{
			if (consol == null || !consol.IsAir)
			{
				return;
			}

			var handOverLog = consol.Logs.MostRecentLogByEventTime(Events.HandedOver,
				new ZQuery(StmALogSchema.SL_IsEstimate, false));
			if (handOverLog != null)
			{
				UpdateShipmentDeliveredTime(consol, handOverLog);
			}
		}

		#endregion

		#region BookingConfirmation Event

		static bool IsBookingConfirmationEvent(IStmALog log) =>
			(log.SL_SE_NKEvent == Events.BookingConfirmedCode && !log.SL_IsEstimate)
			|| (log.SL_SE_NKEvent == Events.ArrivalCode && log.SL_IsEstimate);

		static void HandleBookingConfirmations(this ForwardingConsol consol, BookingConfirmationCollection bookingConfirmations, IStmALog log, bool loading)
		{
			if (log.Parameters.ContainsKey(EventConstants.EventReferenceParameters.Codes.From)
				&& log.Parameters.ContainsKey(EventConstants.EventReferenceParameters.Codes.To)
				&& log.Parameters.ContainsKey(EventConstants.EventReferenceParameters.Codes.VoyageFlightNumber)
				&& log.Parameters.ContainsKey(EventConstants.EventReferenceParameters.Codes.FlightDate)
				&& log.Parameters.ContainsKey(EventConstants.EventReferenceParameters.Codes.Total))
			{
				string loadPort, dischargePort;
				string flightNumber;
				log.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.From, out loadPort);
				log.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.To, out dischargePort);
				log.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.VoyageFlightNumber, out flightNumber);

				if (log.SL_Reference.ToLower().StartsWith((NoResString)"invalid flight number reported by airline") || DoesNotHaveCorrectFlightNumberFormat(flightNumber))
				{
					return;
				}

				var flightTime = ParseFlightTime(log);
				var scheduleDate = ParseFlightSchduleDate(log);
				var arrivalTime = ParseArrivalTime(log);

				int bookedPiece;
				if (!TryParsePieceValue(log, EventConstants.EventReferenceParameters.Codes.Partial, out bookedPiece))
				{
					TryParsePieceValue(log, EventConstants.EventReferenceParameters.Codes.Total, out bookedPiece);
				}

				var bookingConfirmation = bookingConfirmations.Cast<BookingConfirmation>()
					.FirstOrDefault(b => IsSameFlight(b.VoyageFlight, flightNumber, log.SL_SE_NKEvent == Events.ArrivalCode ? b.ArrivalTime : b.ScheduleDate, scheduleDate));
				if (bookingConfirmation != null)
				{
					bookingConfirmation.EventCode = log.SL_SE_NKEvent;
					bookingConfirmation.EventIsEstimate = log.SL_IsEstimate;
					bookingConfirmation.LoadPort = loadPort;
					bookingConfirmation.DischargePort = dischargePort;

					UpdateBookedPieces(bookingConfirmation, log, bookedPiece);
					UpdateBookingFlightTime(bookingConfirmation, log, flightTime, arrivalTime);
				}
				else if (log.SL_SE_NKEvent == Events.ArrivalCode)
				{
					return;
				}
				else
				{
					bookingConfirmation = new BookingConfirmation()
					{
						EventCode = log.SL_SE_NKEvent,
						EventIsEstimate = log.SL_IsEstimate,
						LoadPort = loadPort,
						DischargePort = dischargePort,
						VoyageFlight = flightNumber,
						ScheduleDate = scheduleDate
					};

					UpdateBookedPieces(bookingConfirmation, log, bookedPiece);
					UpdateBookingFlightTime(bookingConfirmation, log, flightTime, arrivalTime);

					bookingConfirmations.Add(bookingConfirmation);
				}

				if (!loading && FreightDataRegistry.Instance.AutomaticUpdatingofPlannedLegs_Air.Value)
				{
					consol.MatchAndUpdateRoutingLeg(bookingConfirmation, () => true);
				}
			}
		}

		static void UpdateBookedPieces(BookingConfirmation bookingConfirmation, IStmALog log, int bookedPieces)
		{
			if (log.SL_SE_NKEvent == Events.BookingConfirmedCode)
			{
				bookingConfirmation.BookedPieces = bookedPieces;
			}
		}

		static void UpdateBookingFlightTime(BookingConfirmation bookingConfirmation, IStmALog log, ZDateTime flightTime, ZDateTime arrivalTime)
		{
			if (log.SL_SE_NKEvent == Events.BookingConfirmedCode)
			{
				bookingConfirmation.DepartureTime = flightTime;
				bookingConfirmation.ArrivalTime = arrivalTime;
			}
			else if (log.SL_SE_NKEvent == Events.ArrivalCode)
			{
				if (!log.SL_IsEstimate)
				{
					bookingConfirmation.ArrivalTime = arrivalTime;
				}
			}
			else
			{
				bookingConfirmation.DepartureTime = flightTime;
			}
		}

		static bool IsSameFlight(ZString flightNumber1, ZString flightNumber2, ZDateTime scheduleDate1, ZDateTime scheduleDate2)
		{
			var date1 = scheduleDate1.TimeOfDay.Ticks > 0 ? scheduleDate1.Date.ToDateTime() : scheduleDate1;
			var date2 = scheduleDate2.TimeOfDay.Ticks > 0 ? scheduleDate2.Date.ToDateTime() : scheduleDate2;

			return flightNumber1.EqualsIgnoringCase(flightNumber2) && date1 == date2;
		}

		static bool DoesNotHaveCorrectFlightNumberFormat(this ZString flightNumber)
		{
			var cleanFlightNumber = flightNumber
				.Replace(" ", string.Empty)
				.Replace("/", string.Empty)
				.Replace("\\", string.Empty)
				.Replace("-", string.Empty);

			if (!string.IsNullOrEmpty(cleanFlightNumber) && cleanFlightNumber.Length >= 2)
			{
				cleanFlightNumber = cleanFlightNumber.Substring(0, 2) + cleanFlightNumber.Substring(2).TrimStart('0');
			}

			return string.IsNullOrEmpty(cleanFlightNumber) || !FlightNumberWithCarrierPrefixRegex.IsMatch(cleanFlightNumber);
		}

		static public IEnumerable<BookingConfirmation> LoadBookingConfirmations(this ForwardingConsol consol)
		{
			if (consol == null || !consol.IsAir)
			{
				return Enumerable.Empty<BookingConfirmation>();
			}

			var subArvQuery = new ZDBOnlyQuery(typeof(StmALog));
			subArvQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ArrivalCode);
			subArvQuery.AddToFilter(StmALogSchema.SL_IsEstimate, true);

			var subBkcQuery = new ZDBOnlyQuery(typeof(StmALog));
			subBkcQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.BookingConfirmedCode);
			subBkcQuery.AddToFilter(StmALogSchema.SL_IsEstimate, false);
			subBkcQuery.AddToFilter(subArvQuery, JoinCondition.Or);

			var query = new ZDBOnlyQuery(typeof(StmALog));
			query.AddToFilter(StmALogSchema.SL_Parent, consol.PK);
			query.AddToFilter(subBkcQuery);
			query.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			query.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc;

			var logs = consol.Logs.Factory.Load<StmALog>(query);

			var bookingConfirmations = new BookingConfirmationCollection(consol.Factory);

			foreach (var log in logs)
			{
				consol.HandleBookingConfirmations(bookingConfirmations, log, true);
			}

			return bookingConfirmations.Cast<BookingConfirmation>().ToArray();
		}

		#endregion

		#region Actual Event

		static bool IsActualEvent(IStmALog log) =>
			new[]
			{
				Events.FreightUnloadedCode,
				Events.DepartureCode,
				Events.ArrivalCode,
			}.Contains((string)log.SL_SE_NKEvent)
			&& !log.SL_IsEstimate;

		static void HandleActualEvents(this ForwardingConsol consol, ActualEventCollection actualEvents, IStmALog log, bool loading)
		{
			if (log.Parameters.ContainsKey(EventConstants.EventReferenceParameters.Codes.From)
				&& log.Parameters.ContainsKey(EventConstants.EventReferenceParameters.Codes.To)
				&& log.Parameters.ContainsKey(EventConstants.EventReferenceParameters.Codes.VoyageFlightNumber)
				&& log.Parameters.ContainsKey(EventConstants.EventReferenceParameters.Codes.FlightDate)
				&& log.Parameters.ContainsKey(EventConstants.EventReferenceParameters.Codes.Total))
			{
				string loadPort, dischargePort;
				string flightNumber;
				log.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.From, out loadPort);
				log.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.To, out dischargePort);
				log.Parameters.TryGetValue(EventConstants.EventReferenceParameters.Codes.VoyageFlightNumber, out flightNumber);

				var flightTime = ParseFlightTime(log);
				var scheduleDate = ParseFlightSchduleDate(log);

				int partialPiece;
				var isPartial = true;
				if (!TryParsePieceValue(log, EventConstants.EventReferenceParameters.Codes.Partial, out partialPiece))
				{
					isPartial = false;
					TryParsePieceValue(log, EventConstants.EventReferenceParameters.Codes.Total, out partialPiece);
				}

				var actualEvent = actualEvents.Cast<ActualEvent>()
					   .FirstOrDefault(a => IsSameFlight(a.VoyageFlight, flightNumber, a.ScheduleDate, scheduleDate) && a.EventCode.EqualsIgnoringCase(log.SL_SE_NKEvent));

				if (actualEvent == null && !log.SL_SE_NKEvent.EqualsIgnoringCase(Events.DepartureCode))
				{
					if (log.SL_SE_NKEvent.EqualsIgnoringCase(Events.FreightUnloadedCode))
					{
						actualEvent = actualEvents.Cast<ActualEvent>()
							.FirstOrDefault(a => IsSameFlight(a.VoyageFlight, flightNumber, a.ScheduleDate, scheduleDate) && a.EventCode.EqualsIgnoringCase(Events.ArrivalCode));
					}

					if (actualEvent == null)
					{
						var relatedBookingConfirmation =
							consol.BookingConfirmations.Cast<BookingConfirmation>().LastOrDefault(
								bkc => IsSameFlight(bkc.VoyageFlight, flightNumber, bkc.ArrivalTime, scheduleDate));

						if (relatedBookingConfirmation != null)
						{
							actualEvent = actualEvents.Cast<ActualEvent>().FirstOrDefault(actualEvent =>
									IsSameFlight(actualEvent.VoyageFlight, relatedBookingConfirmation.VoyageFlight, actualEvent.ScheduleDate, relatedBookingConfirmation.DepartureTime)
										&& actualEvent.EventCode.EqualsIgnoringCase(Events.DepartureCode));
						}
					}
				}

				var needToAddPartialPieces = false;
				if (isPartial && log.SL_SE_NKEvent == Events.FreightUnloadedCode)
				{
					needToAddPartialPieces = actualEvent != null
						&& actualEvent.EventCode == Events.FreightUnloadedCode
						&& actualEvent.IsPartial;
				}

				if (actualEvent != null)
				{
					actualEvent.EventCode = log.SL_SE_NKEvent;
					actualEvent.LoadPort = loadPort;
					actualEvent.DischargePort = dischargePort;
					actualEvent.IsPartial = isPartial;
					actualEvent.ScheduleDate = scheduleDate;

					UpdateOrAddPartialPieces(actualEvent, log, needToAddPartialPieces, partialPiece);
					UpdateActualFlightTime(actualEvent, log, flightTime);
				}
				else
				{
					actualEvent = new ActualEvent()
					{
						EventCode = log.SL_SE_NKEvent,
						LoadPort = loadPort,
						DischargePort = dischargePort,
						VoyageFlight = flightNumber,
						IsPartial = isPartial,
						ScheduleDate = scheduleDate
					};

					UpdateOrAddPartialPieces(actualEvent, log, needToAddPartialPieces, partialPiece);
					UpdateActualFlightTime(actualEvent, log, flightTime);

					actualEvents.Add(actualEvent);
				}

				if (!loading && FreightDataRegistry.Instance.AutomaticUpdatingofPlannedLegs_Air.Value)
				{
					consol.MatchAndUpdateRoutingLeg(actualEvent, () => true, isAutomaticUpdating: true);
				}
			}
		}

		static void UpdateOrAddPartialPieces(ActualEvent actualEvent, IStmALog log, bool needToAddPartialPieces, int partialPiece)
		{
			if (log.SL_SE_NKEvent == Events.DepartureCode)
			{
				actualEvent.ShippedPieces = partialPiece;
			}
			else if (log.SL_SE_NKEvent == Events.ArrivalCode)
			{
				actualEvent.ArrivedPieces = partialPiece;
			}
			else if (log.SL_SE_NKEvent == Events.FreightUnloadedCode)
			{
				actualEvent.UnloadedPieces = needToAddPartialPieces
					? actualEvent.UnloadedPieces + partialPiece
					: partialPiece;
			}
		}

		static void UpdateActualFlightTime(ActualEvent actualEvent, IStmALog log, ZDateTime flightTime)
		{
			if (log.SL_SE_NKEvent == Events.DepartureCode)
			{
				actualEvent.DepartedTime = flightTime;
			}
			else if (log.SL_SE_NKEvent == Events.ArrivalCode)
			{
				actualEvent.ArrivedTime = flightTime;
			}
		}

		static public IEnumerable<ActualEvent> LoadActualEvents(this ForwardingConsol consol)
		{
			if (consol == null || !consol.IsAir)
			{
				return Enumerable.Empty<ActualEvent>();
			}

			var subQuery = new ZDBOnlyQuery(typeof(StmALog));
			subQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.DepartureCode);
			subQuery.AddToFilter(JoinCondition.Or, StmALogSchema.SL_SE_NKEvent, Events.ArrivalCode);
			subQuery.AddToFilter(JoinCondition.Or, StmALogSchema.SL_SE_NKEvent, Events.FreightUnloadedCode);

			var query = new ZDBOnlyQuery(typeof(StmALog));
			query.AddToFilter(StmALogSchema.SL_Parent, consol.PK);
			query.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			query.AddToFilter(subQuery);
			query.AddToFilter(StmALogSchema.SL_IsEstimate, false);
			query.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc;

			var logs = consol.Logs.Factory.Load<StmALog>(query);

			var actualEvents = new ActualEventCollection(consol.Factory);

			foreach (var log in logs)
			{
				consol.HandleActualEvents(actualEvents, log, true);
			}

			return actualEvents.Cast<ActualEvent>().ToArray();
		}

		#endregion

		#region MatchAndUpdateRoutingLeg

		static public bool MatchAndUpdateRoutingLeg(this ForwardingConsol consol, BookingConfirmation bookingConfirmation, Func<bool> needUpdate)
		{
			var matchedTransport = consol.MatchRoutingLeg(bookingConfirmation.LoadPort, bookingConfirmation.DischargePort);

			var matched = matchedTransport != null;
			if (matched && needUpdate())
			{
				var oldValue = FormattableString.Invariant($"{matchedTransport.JW_VoyageFlight},{matchedTransport.JW_STD.ToLongTimeString()},{matchedTransport.JW_STA.ToLongTimeString()}");
				var newValue = FormattableString.Invariant($"{bookingConfirmation.VoyageFlight},{bookingConfirmation.DepartureTime.ToLongTimeString()},{bookingConfirmation.ArrivalTime.ToLongTimeString()}");

				matchedTransport.JW_VoyageFlight = bookingConfirmation.VoyageFlight;

				if (bookingConfirmation.DepartureTime.IsValid)
				{
					matchedTransport.JW_STD = bookingConfirmation.DepartureTime;
				}

				if (bookingConfirmation.ArrivalTime.IsValid)
				{
					matchedTransport.JW_STA = bookingConfirmation.ArrivalTime;
				}

				consol.AddChangeOfIdentifierEventLog(oldValue, newValue, (NoResString)"Updated from Booking Confirmation", bookingConfirmation.EventCode, bookingConfirmation.EventIsEstimate);
			}

			return matched;
		}

		static public bool MatchAndUpdateRoutingLeg(this ForwardingConsol consol, ActualEvent actualEvent, Func<bool> needUpdate, bool isAutomaticUpdating = false)
		{
			var matchedTransport = consol.MatchRoutingLeg(actualEvent.LoadPort, actualEvent.DischargePort);

			var matched = matchedTransport != null;
			if (matched && needUpdate())
			{
				var oldValue = FormattableString.Invariant($"{matchedTransport.JW_VoyageFlight},{matchedTransport.JW_ATD.ToLongTimeString()},{matchedTransport.JW_ATA.ToLongTimeString()}");
				var newValue = FormattableString.Invariant($"{actualEvent.VoyageFlight},{actualEvent.DepartedTime.ToLongTimeString()},{actualEvent.ArrivedTime.ToLongTimeString()}");

				matchedTransport.JW_VoyageFlight = actualEvent.VoyageFlight;

				if (actualEvent.DepartedTime.IsValid)
				{
					matchedTransport.JW_ATD = actualEvent.DepartedTime;
					matchedTransport.SkipAddATDEvent = isAutomaticUpdating || matchedTransport.SkipAddATDEvent;
				}

				if (actualEvent.ArrivedTime.IsValid)
				{
					matchedTransport.JW_ATA = actualEvent.ArrivedTime;
					matchedTransport.SkipAddATAEvent = isAutomaticUpdating || matchedTransport.SkipAddATDEvent;
				}

				consol.AddChangeOfIdentifierEventLog(oldValue, newValue, (NoResString)"Updated from Actual Events", actualEvent.EventCode, updatedByEventIsEstimate: false);
			}

			return matched;
		}

		static Transport MatchRoutingLeg(this ForwardingConsol consol, ZString loadPort, ZString dischargePort)
		{
			return consol.Transports.Cast<Transport>().
				FirstOrDefault(t => t.IsAir && t.JW_RL_NKLoadPort == loadPort && t.JW_RL_NKDiscPort == dischargePort);
		}

		static void AddChangeOfIdentifierEventLog(this ForwardingConsol consol,
			string oldValue, string newValue, string reason, string updatedByEventCode = null, bool updatedByEventIsEstimate = false)
		{
			var parameters = new Dictionary<string, string>();
			parameters[EventConstants.EventReferenceParameters.Codes.Type] = (NoResString)"Routing Leg";
			parameters[EventConstants.EventReferenceParameters.Codes.Old] = oldValue;
			parameters[EventConstants.EventReferenceParameters.Codes.New] = newValue;
			parameters[EventConstants.EventReferenceParameters.Codes.Reason] = reason;

			if (!string.IsNullOrEmpty(updatedByEventCode))
			{
				parameters[EventConstants.EventReferenceParameters.Codes.EventCode] = updatedByEventCode;
			}

			if (updatedByEventIsEstimate)
			{
				parameters[EventConstants.EventReferenceParameters.Codes.Estimate] = updatedByEventIsEstimate ? "Y" : "N";
			}
			
			consol.Logs.CreateOrRecreateEventLog(Events.ChangeOfIdentifier, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, parameters.ToArray());
		}

		#endregion

		#region PropagateTransportEventToConsol

		static public void PropagateTransportEventToConsol(this ForwardingConsol consol, IStmALog log)
		{
			consol.Logs.CreateOrRecreateEventLog(Events.All[log.SL_SE_NKEvent],
				log.SL_IsEstimate ? EstimateActual.Estimate : EstimateActual.Actual,
				log.SL_EventTimeOffset,
				StmALog.GetFreeTextFromReference(log.SL_Reference),
				log.Parameters.ToArray());
		}

		#endregion

		#region Implementation

		static bool TryParsePieceValue(IStmALog log, string parameterName, out int piece)
		{
			piece = 0;

			if (!log.Parameters.ContainsKey(parameterName))
			{
				return false;
			}

			return int.TryParse(log.Parameters[parameterName], out piece);
		}

		static bool TryParseDateValue(IStmALog log, string parameterName, out ZDateTime value)
		{
			value = ZDateTime.Empty;

			if (!log.Parameters.ContainsKey(parameterName))
			{
				return false;
			}

			return ZDateTime.TryParseISO8601Date(log.Parameters[parameterName], out value);
		}

		static ZDateTime ParseFlightTime(IStmALog log)
		{
			if (log.SL_SE_NKEvent == Events.BookingConfirmedCode)
			{
				ZDateTime flightTime;
				TryParseDateValue(log, EventConstants.EventReferenceParameters.Codes.FlightDate, out flightTime);

				return flightTime;
			}
			else if (log.SL_SE_NKEvent == Events.DepartureCode
				|| log.SL_SE_NKEvent == Events.ArrivalCode)
			{
				return log.SL_EventTime;
			}

			return ZDateTime.Empty;
		}

		static ZDateTime ParseArrivalTime(IStmALog log)
		{
			if (log.SL_SE_NKEvent == Events.BookingConfirmedCode)
			{
				ZDateTime flightTime;
				TryParseDateValue(log, EventConstants.EventReferenceParameters.Codes.EstimatedTimeOfArrival, out flightTime);

				return flightTime;
			}
			else if (log.SL_SE_NKEvent == Events.ArrivalCode)
			{
				return log.SL_EventTime;
			}

			return ZDateTime.Empty;
		}

		static ZDateTime ParseFlightSchduleDate(IStmALog log)
		{
			ZDateTime flightTime;
			TryParseDateValue(log, EventConstants.EventReferenceParameters.Codes.FlightDate, out flightTime);
			return flightTime.Date.ToZDateTime();
		}

		#endregion
	}
}
