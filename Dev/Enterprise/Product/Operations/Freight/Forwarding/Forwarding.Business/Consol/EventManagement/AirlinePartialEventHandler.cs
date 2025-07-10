using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Constants = CargoWise.EventReference.Constants;
using EventReferenceParameterTypes = Enterprise.Core.Constants.EventReferenceParameterTypes;

namespace Enterprise.Freight.Forwarding.Business
{
	public sealed class AirlinePartialEventHandler : PartialEventHandler
	{
		protected override bool Equals(IStmALog log1, IStmALog log2)
		{
			if (!log1.Parameters.ContainsKey(Constants.EventReferenceParameters.Codes.VoyageFlightNumber)
				|| !log2.Parameters.ContainsKey(Constants.EventReferenceParameters.Codes.VoyageFlightNumber)
				|| !log1.Parameters.ContainsKey(Constants.EventReferenceParameters.Codes.FlightDate)
				|| !log2.Parameters.ContainsKey(Constants.EventReferenceParameters.Codes.FlightDate)
				|| log1.Parameters[Constants.EventReferenceParameters.Codes.VoyageFlightNumber] != log2.Parameters[Constants.EventReferenceParameters.Codes.VoyageFlightNumber])
			{
				return false;
			}

			DateTime flightDate1;

			if (!DateTime.TryParse(log1.Parameters[Constants.EventReferenceParameters.Codes.FlightDate], out flightDate1))
			{
				return false;
			}

			DateTime flightDate2;

			if (!DateTime.TryParse(log2.Parameters[Constants.EventReferenceParameters.Codes.FlightDate], out flightDate2))
			{
				return false;
			}

			return flightDate1.Year == flightDate2.Year
				&& flightDate1.Month == flightDate2.Month
				&& flightDate1.Day == flightDate2.Day
				&& log1.Parameters.TryGetValue(Constants.EventReferenceParameters.Codes.Partial, out string partialParameter1)
				&& log2.Parameters.TryGetValue(Constants.EventReferenceParameters.Codes.Partial, out string partialParameter2)
				&& partialParameter1 == partialParameter2
				&& log1.SL_EventTime == log2.SL_EventTime;
		}

		protected override IEnumerable<KeyValuePair<string, string>> GetParametersForCompletionLog(IEnumerable<IStmALog> logs)
		{
			var log = logs.First();

			if (log.Parameters.ContainsKey(Constants.EventReferenceParameters.Codes.Location))
			{
				yield return new KeyValuePair<string, string>(
					Constants.EventReferenceParameters.Codes.Location,
					log.Parameters[Constants.EventReferenceParameters.Codes.Location]);
			}

			if (log.Parameters.ContainsKey(Constants.EventReferenceParameters.Codes.Facility))
			{
				yield return new KeyValuePair<string, string>(
					Constants.EventReferenceParameters.Codes.Facility,
					log.Parameters[Constants.EventReferenceParameters.Codes.Facility]);
			}

			yield return new KeyValuePair<string, string>(Constants.EventReferenceParameters.Codes.Type, EventReferenceParameterTypes.Complete);
		}
	}
}
