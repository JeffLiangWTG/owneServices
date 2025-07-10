using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public sealed class PartialEventsInfo : NonPersistentBusinessObject, IObsoleteValidation
	{
		public PartialEventsInfo(IStmALogParent logParent, ZString[] eventCodes, ZString location)
		{
			this.logParent = logParent;
			this.eventCodes = eventCodes ?? System.Array.Empty<ZString>();
			this.location = location;
		}

		readonly IStmALogParent logParent;
		readonly ZString[] eventCodes;
		readonly ZString location;

		#region Schema

		static class Schema
		{
			public const string Details = "Details";
			public const string Total = "Total";
		}

		#endregion

		#region Properties

		public ZString Details
		{
			get
			{
				EnsureValuesAreInitialized();
				return details;
			}
		}

		ZString details;

		public ZPropertyInfo DetailsInfo
		{
			get { return GetZPropertyInfo(Schema.Details); }
		}

		public ZString Total
		{
			get
			{
				EnsureValuesAreInitialized();
				return total;
			}
		}

		ZString total;

		public ZPropertyInfo TotalInfo
		{
			get { return GetZPropertyInfo(Schema.Total); }
		}

		public bool IsEmpty
		{
			get { return !PartialLogs.Any(); }
		}

		#endregion

		#region PartialLogs

		PartialLogDetails[] PartialLogs
		{
			get { return partialLogs ?? (partialLogs = GetPartialLogs().ToArray()); }
		}

		PartialLogDetails[] partialLogs;

		IEnumerable<PartialLogDetails> GetPartialLogs()
		{
			if (logParent == null)
			{
				return Enumerable.Empty<PartialLogDetails>();
			}

			return logParent
				.Logs
				.GetAllLogs()
				.Cast<StmALog>()
				.Where(log => !log.IsCancelled
							  && eventCodes.Contains(log.SL_SE_NKEvent)
							  && log.Parameters.ContainsKey(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Partial)
							  && log.Parameters.ContainsKey(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Total)
							  && log.Parameters.ContainsKey(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location)
							  && log.Parameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location] == location)
				.OrderBy(log => log.SL_EventTime)
				.Select(log => new PartialLogDetails(log));
		}

		#endregion

		#region Values

		bool areValuesInitialized;

		void EnsureValuesAreInitialized()
		{
			if (!areValuesInitialized)
			{
				InitializeValues();
				areValuesInitialized = true;
			}
		}

		void InitializeValues()
		{
			details = string.Join(System.Environment.NewLine, GetPartialEventsDetails(PartialLogs));
			total = GetPartialEventsTotal(PartialLogs);
		}

		IEnumerable<string> GetPartialEventsDetails(IEnumerable<PartialLogDetails> ienumerablePartialLogs)
		{
			foreach (var log in ienumerablePartialLogs)
			{
				var locationInfo = log.Location;
				var flightInfo = string.Format("{0} {1}", log.FlighNumber, log.FlightDate).Trim();
				var piecesInfo = Res.GetString("d498146f-214a-44ee-b4a5-bb65b927ff68", "{0} of {1} pieces", log.Partial, log.Total);

				yield return string.Join(" ", locationInfo, flightInfo, piecesInfo);
			}
		}

		string GetPartialEventsTotal(PartialLogDetails[] partialLogsInput)
		{
			var groupedTotals = partialLogsInput
				.GroupBy(logs => logs.Total)
				.ToArray();

			if (!groupedTotals.Any())
			{
				return string.Empty;
			}

			int sumPartials = partialLogsInput.Sum(log => log.Partial);
			int maxTotal = groupedTotals.Max(t => t.Key);

			if (groupedTotals.Skip(1).Any())
			{
				TotalInfo.AddWarningWithoutValidationCheck(Res.GetString("1cc64add-6e60-4331-83e8-813d1f246edc", "Event totals are inconsistent."));
			}

			return Res.GetString("02a68dfe-5009-4d57-ac06-f3093a69a099", "Running Total: {0} of {1}", sumPartials, maxTotal);
		}

		#endregion

		#region Nested Types

		sealed class PartialLogDetails
		{
			public PartialLogDetails(StmALog log)
			{
				this.log = log;
			}

			readonly StmALog log;

			public ZString FlighNumber
			{
				get
				{
					if (!flightNumber.HasValue)
					{
						flightNumber = GetValueOrDefault(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.VoyageFlightNumber);
					}

					return flightNumber.Value;
				}
			}

			ZString? flightNumber;

			public ZString FlightDate
			{
				get
				{
					if (!flightDate.HasValue)
					{
						flightDate = GetValueOrDefault(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.FlightDate);
					}

					return flightDate.Value;
				}
			}

			ZString? flightDate;

			public ZString Location
			{
				get
				{
					if (!location.HasValue)
					{
						location = GetValueOrDefault(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location);
					}

					return location.Value;
				}
			}

			ZString? location;

			public ZInt Partial
			{
				get
				{
					if (!partial.HasValue)
					{
						var paramValue = GetValueOrDefault(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Partial);

						partial = ZInt.ParseSafe(paramValue, 0);
					}

					return partial.Value;
				}
			}

			ZInt? partial;

			public ZInt Total
			{
				get
				{
					if (!total.HasValue)
					{
						var paramValue = GetValueOrDefault(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Total);

						total = ZInt.ParseSafe(paramValue, 0);
					}

					return total.Value;
				}
			}

			ZInt? total;

			ZString GetValueOrDefault(string parameterName)
			{
				return log.Parameters.ContainsKey(parameterName)
					? (ZString)log.Parameters[parameterName]
					: ZString.Empty;
			}
		}

		#endregion
	}
}
