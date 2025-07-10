using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.AIS
{
	public class PortMatcher : IPortMatcher
	{
		public PortMatcher()
			: this(ObjectFactory.Get<IAisWebApiClient>())
		{
		}

		public PortMatcher(IAisWebApiClient aisWebApiClient)
		{
			this.aisWebApiClient = aisWebApiClient;
		}

		public async Task<IPortMatches> MatchAsync(IVesselMovementsUrlModel request, CancellationToken cancellationToken = default)
		{
			if (request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			var carrierPortCallsCollection = await MakeRequestsAsync(request, cancellationToken).ConfigureAwait(false);

			if (carrierPortCallsCollection.IsNullOrEmpty())
			{
				return new PortMatches();
			}

			TimeSpan CalculateTimeDeviationDuration(PortMatch portMatch)
			{
				if (!portMatch.ArrivalTime.HasValue)
				{
					return TimeSpan.MaxValue;
				}

				var timeDeviation = request.ArrivalTime - portMatch.ArrivalTime.Value.DateTime;
				return timeDeviation.Duration();
			}

			var portCallsOrdered = carrierPortCallsCollection
				.Select(MapPortCallToPortMatch)
				.Where(match => match.ArrivalTime.HasValue)
				.OrderByDescending(match => match.ArrivalTime)
				.ToArray();

			var dischargeCountry = GetCountry(request.ArrivalPortUnloco);

			var closestMatchAmongPortCalls =
				portCallsOrdered
					.Where(match => match.Unloco == request.ArrivalPortUnloco)
					.MinBySafe(CalculateTimeDeviationDuration)
				?? portCallsOrdered
					.Where(match => GetCountry(match.Unloco) == dischargeCountry)
					.MinBySafe(CalculateTimeDeviationDuration);

			if (closestMatchAmongPortCalls == null)
			{
				return new PortMatches();
			}

			var lastForeignPort = portCallsOrdered
				.SkipWhile(match => match != closestMatchAmongPortCalls)
				.SkipWhile(match => GetCountry(match.Unloco) == dischargeCountry)
				.FirstOrDefault();

			var firstArrivalPort = portCallsOrdered
				.SkipWhile(match => match != closestMatchAmongPortCalls)
				.TakeWhile(match => GetCountry(match.Unloco) == dischargeCountry)
				.LastOrDefault();

			return new PortMatches(lastForeignPort, firstArrivalPort);
		}

#if DEBUG
		protected virtual bool IsTest => Globals.IsTest;
#endif

		async Task<ICollection<PortCall>> MakeRequestsAsync(IVesselMovementsUrlModel request, CancellationToken cancellationToken)
		{
#if DEBUG
			if (IsTest)
			{
				return default;
			}
#endif
			var timeFrom = request.DepartureTime.AddDays(-DateRangeOffset);
			var timeTo = request.ArrivalTime.AddDays(DateRangeOffset);
			var carrierPortCallsResponse = await aisWebApiClient
				.PortCallsAsync(
					request.LloydsNumber,
					null,
					null,
					timeFrom.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
					timeTo.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
					request.CarrierCode,
					null,
					null,
					null,
					cancellationToken)
				.ConfigureAwait(false);

			return carrierPortCallsResponse?.Items;
		}

		static PortMatch MapPortCallToPortMatch(PortCall portCall)
		{
			var arrivalTime = portCall.Ata ?? portCall.Eta;
			var departureTime = portCall.Atd ?? portCall.Etd;
			return new PortMatch(portCall.Port.Unloco, arrivalTime, departureTime);
		}

		static string GetCountry(ZString unloco)
		{
			return unloco.SubstringSafe(0, 2);
		}

		public void Dispose()
		{
			aisWebApiClient?.Dispose();
		}

		const int DateRangeOffset = 7;
		readonly IAisWebApiClient aisWebApiClient;
	}
}
