using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Telematics.ServiceTasks
{
	class SpeedLimitUpdater
	{
		public SpeedLimitUpdater(ILogger serviceLogger, BusinessObjectFactory businessObjectFactory, IHttpClientAdapter httpClientAdapter, IRequestPacker requestPacker, IResponseUnpacker responseUnpacker, IProgressLogger progressLogger)
		{
			this.serviceLogger = serviceLogger ?? throw new ArgumentNullException(nameof(serviceLogger));
			businessObjectFactoryProvider = new BusinessObjectFactoryProvider(businessObjectFactory ?? throw new ArgumentNullException(nameof(businessObjectFactory)));
			this.httpClientAdapter = httpClientAdapter ?? throw new ArgumentNullException(nameof(httpClientAdapter));
			this.requestPacker = requestPacker ?? throw new ArgumentNullException(nameof(requestPacker));
			this.responseUnpacker = responseUnpacker ?? throw new ArgumentNullException(nameof(responseUnpacker));
			this.progressLogger = progressLogger ?? throw new ArgumentNullException(nameof(progressLogger));
		}

		public void Run(CancellationToken cancellationToken)
		{
			var updatedSpeedLimitsCount = 0;
			try
			{
				progressLogger.Initialize();
				var devices = businessObjectFactoryProvider.Current.Load<GlbDevice>(new ZQuery());
				foreach (var device in devices)
				{
					cancellationToken.ThrowIfCancellationRequested();

					while (true)
					{
						var deviceLocations = LoadLocationsWithoutSpeedLimits(device.PK, businessObjectFactoryProvider.Current);
						if (deviceLocations.Length <= 0)
						{
							break;
						}

						if (!RunIteration(cancellationToken, deviceLocations))
						{
							break;
						}

						businessObjectFactoryProvider.SaveCurrentReclaimMemoryAndCreateNew();
						updatedSpeedLimitsCount += deviceLocations.Length;

						if (progressLogger.ShouldLog())
						{
							serviceLogger.Log(LogType.Information, FormattableString.Invariant($"Speed limits updated: {updatedSpeedLimitsCount} row(s)."));
						}
					}
				}
			}
			finally
			{
				if (updatedSpeedLimitsCount > 0)
				{
					serviceLogger.Log(LogType.Information, FormattableString.Invariant($"Total speed limits updated: {updatedSpeedLimitsCount} row(s)."));
				}
			}
		}

		bool RunIteration(CancellationToken cancellationToken, IEnumerable<GlbDeviceLocation> deviceLocations)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var grouppedLocations = deviceLocations
				.GroupBy(location => location.V2_Location)
				.ToList();

			string response;
			try
			{
				response = httpClientAdapter.GetStringAsync(requestPacker.CreateUri(grouppedLocations.Select(grouping => grouping.Key)), cancellationToken).GetAwaiter().GetResult();
			}
			catch (HttpRequestException exception)
			{
				serviceLogger.Log(LogType.Error, FormattableString.Invariant($"Google request exception: {exception.Message}"));
				return false;
			}

			var responseDictionary = responseUnpacker.Unpack(response);

			for (var i = 0; i < grouppedLocations.Count; i++)
			{
				var speedLimit = responseDictionary.TryGetValue(i, out var limit) ? limit : 0;
				var speedLimitState = speedLimit > 0 ? SpeedLimitStateWithLimit : SpeedLimitStateWithoutLimit;

				foreach (var deviceLocation in grouppedLocations[i])
				{
					deviceLocation.V2_SpeedLimitKmh = speedLimit;
					deviceLocation.V2_SpeedLimitState = speedLimitState;
				}
			}

			return true;
		}

		static GlbDeviceLocation[] LoadLocationsWithoutSpeedLimits(ZGuid devicePk, IFactory businessObjectFactory)
		{
			// TODO Should use a filtered index NR_RX__V2_PK on GlbDeviceLocation
			var zQuery = new ZQuery(GlbDeviceLocationSchema.V2_SpeedLimitState, SQLComparisonOperator.Equal, SpeedLimitStateToLoad)
			{
				MaximumRows = 100,
				OrderBy = GlbDeviceLocationSchema.Constants.V2_MeasurementTimeUtc
			};
			zQuery.AddToFilter(JoinCondition.And, GlbDeviceLocationSchema.V2_V3_Device, SQLComparisonOperator.Equal, devicePk);
			return businessObjectFactory.Load<GlbDeviceLocation>(zQuery);
		}

		internal const string SpeedLimitStateToLoad = "M";
		internal const string SpeedLimitStateWithLimit = "S";
		internal const string SpeedLimitStateWithoutLimit = "U";

		readonly BusinessObjectFactoryProvider businessObjectFactoryProvider;
		readonly IHttpClientAdapter httpClientAdapter;
		readonly IRequestPacker requestPacker;
		readonly IResponseUnpacker responseUnpacker;
		readonly IProgressLogger progressLogger;
		readonly ILogger serviceLogger;
	}

	public interface IProgressLogger
	{
		void Initialize();
		bool ShouldLog();
	}
}
