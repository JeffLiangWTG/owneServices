using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.Telematics.Data.Packets.V2.Commercial.Ingoing;

namespace Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.PayloadProcessors
{
	class OdometerPayloadRecordProcessor : IPayloadRecordProcessor<OdometerPayloadRecord>
	{
		public OdometerPayloadRecordProcessor(ILogger logger)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public int Process(BusinessObjectFactory factory, GlbDevice device, OdometerPayloadRecord record)
		{
			var deviceQuery = new ZQuery(GlbDeviceOdometerSchema.GDO_V3_Device, SQLComparisonOperator.Equal, device.PK);
			var odometerQuery = new ZQuery(GlbDeviceOdometerSchema.GDO_OdometerKM, SQLComparisonOperator.Equal, (ZDecimal)record.Odometer);
			var sameOdometerQuery = new ZQuery(deviceQuery, JoinCondition.And, odometerQuery);
			var existingRecordWithSameOdometer = factory.LoadTop1<GlbDeviceOdometer>(sameOdometerQuery);

			if (existingRecordWithSameOdometer == null)
			{
				var odometer = factory.New<GlbDeviceOdometer>();
				odometer.GDO_V3_Device = device.PK;
				odometer.GDO_MeasurementTimeUtc = record.DateTimeOffset.UtcDateTime;
				odometer.GDO_OdometerKM = record.Odometer;
			}
			else if (existingRecordWithSameOdometer.GDO_MeasurementTimeUtc > record.DateTimeOffset.UtcDateTime)
			{
				var previousRecordQuery = new ZQuery(GlbDeviceOdometerSchema.GDO_MeasurementTimeUtc, SQLComparisonOperator.LessThan, existingRecordWithSameOdometer.GDO_MeasurementTimeUtc);

				var previousRecord = factory.LoadTop1<GlbDeviceOdometer>(new ZQuery(previousRecordQuery, JoinCondition.And, deviceQuery) { OrderBy = GlbDeviceOdometerSchema.Constants.GDO_MeasurementTimeUtc + " DESC" });
				if (previousRecord?.GDO_MeasurementTimeUtc >= record.DateTimeOffset.UtcDateTime)
				{
					logger.Log(LogType.Warning, FormattableString.Invariant($"Odometer record at time: {record.DateTimeOffset.ToString()} with value: {record.Odometer} was invalid"));
				}
				else
				{
					existingRecordWithSameOdometer.GDO_MeasurementTimeUtc = record.DateTimeOffset.UtcDateTime;
				}
			}

			return 1;
		}

		readonly ILogger logger;
	}
}
