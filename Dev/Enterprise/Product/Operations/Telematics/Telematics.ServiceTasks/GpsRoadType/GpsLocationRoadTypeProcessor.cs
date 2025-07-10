using System;
using System.Net.Http;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Telematics.Business.Registry;
using Newtonsoft.Json;
using WTG.Foundation.Http;

namespace Enterprise.Telematics.ServiceTasks.GpsRoadType
{
	class GpsLocationRoadTypeProcessor
	{
		public GpsLocationRoadTypeProcessor(ILogger logger)
			: this(
				logger,
				new BusinessObjectFactory { NameForDebugging = "Telematics GPS Road Type Processor" },
				new GpsLocationAccessor(),
				new RoadTypeCalculator(ObjectFactory.Get<IHttpClientFactory>(), logger))
		{
		}

		internal GpsLocationRoadTypeProcessor(ILogger logger, BusinessObjectFactory factory, IGpsLocationAccessor gpsLocationAccessor, IRoadTypeCalculator roadTypeCalculator)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			factoryProvider = new BusinessObjectFactoryProvider(factory ?? throw new ArgumentNullException(nameof(factory)));
			this.roadTypeCalculator = roadTypeCalculator ?? throw new ArgumentNullException(nameof(roadTypeCalculator));
			this.gpsLocationAccessor = gpsLocationAccessor ?? throw new ArgumentNullException(nameof(gpsLocationAccessor));
		}

		public void Run(CancellationToken cancellationToken)
		{
			try
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					logger.Log(
						LogType.Information,
						$"Start processing Location records");

					var gpsLocations = gpsLocationAccessor
						.GetLocations(factoryProvider.Current);

					logger.Log(
						LogType.Information,
						$"Processing {gpsLocations.Count} Location records");

					roadTypeCalculator
						.MarkLocationRoadType(gpsLocations, cancellationToken);

					factoryProvider.SaveCurrentReclaimMemoryAndCreateNew();

					logger.Log(
						LogType.Information,
						$"Processed {gpsLocations.Count} Location records");

					if (gpsLocations.Count < TelematicsConfigurationRegistry.Instance.PublicPrivateRoadEndpointRecordBatchSize.Value)
					{
						break;
					}
				}
			}
			catch (Exception e) when (
				e is JsonSerializationException ||
				e is JsonReaderException ||
				e is GpsLocationRoadTypeDecodeException ||
				e is HttpRequestException)
			{
				logger.Log(
					LogType.Error,
					e.Message);
			}
		}

		readonly ILogger logger;
		readonly BusinessObjectFactoryProvider factoryProvider;
		readonly IRoadTypeCalculator roadTypeCalculator;
		readonly IGpsLocationAccessor gpsLocationAccessor;
	}
}
