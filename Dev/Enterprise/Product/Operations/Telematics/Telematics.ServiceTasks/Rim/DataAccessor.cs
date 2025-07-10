using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Telematics.ServiceTasks.Rim
{
	class DataAccessor : IDataAccessor
	{
		public DataAccessor(ILogger logger)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public IEnumerable<IDeviceData> GetData(IFactory factory)
		{
			_ = factory ?? throw new ArgumentNullException(nameof(factory));

			var devices = GetDevices();
			if (devices.Length == 0)
			{
				logger.Log(LogType.Debug, "No devices registered in RIM program.");
			}

			return LoadData();

			(ZGuid PK, ZString V3_HardwareIdentifier, ZDateTimeOffset TE_StartTime)[] GetDevices()
			{
				var edgeQuery = new ZQuery();
				edgeQuery.AddToFilter(TelEdgeSchema.TE_EntityTableCodeTo, "V3");
				edgeQuery.AddToFilter(JoinCondition.And, TelEdgeSchema.TE_RelationshipType, TelEdgeRelationshipTypes.Codes.RIM);
				edgeQuery.AddToFilter(JoinCondition.And, TelEdgeSchema.TE_EndTime, null);
				var edges = factory.Load<TelEdge>(edgeQuery);

				var deviceQuery = new ZQuery();
				deviceQuery.AddToFilter(GlbDeviceSchema.V3_HardwareKind, SQLComparisonOperator.Equal, GlbDeviceKindCodes.WTGEmbedded);
				deviceQuery.AddToFilter(GlbDeviceSchema.PK, SQLComparisonOperator.Equal, edges.Select(edge => edge.TE_EntityIdTo));
				return factory.Load<GlbDevice>(deviceQuery)
					.Join(
						edges,
						device => device.PK, edge => edge.TE_EntityIdTo,
						(device, edge) => (device, edge))
					.Select(tuple => (tuple.device.PK, tuple.device.V3_HardwareIdentifier, tuple.edge.TE_StartTime))
					.ToArray();
			}

			IEnumerable<IDeviceData> LoadData()
			{
				foreach (var (pk, hardwareIdentifier, startTime) in devices)
				{
					var locations = GetLocations(pk, startTime.ToUtcZDateTime());
					yield return new DeviceData
					{
						DeviceId = hardwareIdentifier,
						Locations = locations,
					};
				}
			}

			GlbDeviceLocation[] GetLocations(ZGuid devicePk, ZDateTime startTime)
			{
				var val = new ZDBOnlyQuery(typeof(GlbDeviceLocation));
				val.AddToFilter(GlbDeviceLocationSchema.V2_V3_Device, devicePk);
				val.AddToFilter(GlbDeviceLocationSchema.V2_RimReported, false);
				val.AddToFilter(GlbDeviceLocationSchema.V2_MeasurementTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, startTime);
				return factory.Load<GlbDeviceLocation>(val);
			}
		}

		readonly ILogger logger;

		class DeviceData : IDeviceData
		{
			public string DeviceId { get; set; }
			public IEnumerable<GlbDeviceLocation> Locations { get; set; }
		}
	}
}
