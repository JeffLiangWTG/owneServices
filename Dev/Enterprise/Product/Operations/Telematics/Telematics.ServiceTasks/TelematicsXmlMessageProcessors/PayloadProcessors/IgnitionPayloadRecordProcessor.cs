using CargoWise.EntityFramework;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.Telematics.Data.Packets.V2.Commercial.Ingoing;

namespace Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.PayloadProcessors
{
	class IgnitionPayloadRecordProcessor : IPayloadRecordProcessor<IgnitionPayloadRecord>
	{
		public int Process(BusinessObjectFactory factory, GlbDevice device, IgnitionPayloadRecord record)
		{
			var deviceQuery = new ZQuery(GlbDeviceIgnitionSchema.GDI_V3_Device, SQLComparisonOperator.Equal, device.PK);
			var previousIgnitionRecordsQuery =
				new ZQuery(deviceQuery,
					JoinCondition.And,
					new ZQuery(GlbDeviceIgnitionSchema.GDI_MeasurementTimeUtc, SQLComparisonOperator.LessThan, record.DateTimeOffset.UtcDateTime))
				{
					OrderBy = GlbDeviceIgnitionSchema.Constants.GDI_MeasurementTimeUtc + " DESC"
				};

			var previousIgnitionRecord = factory.LoadTop1<GlbDeviceIgnition>(previousIgnitionRecordsQuery);

			var nextIgnitionRecordsQuery =
				new ZQuery(deviceQuery,
					JoinCondition.And,
					new ZQuery(GlbDeviceIgnitionSchema.GDI_MeasurementTimeUtc, SQLComparisonOperator.GreaterThan, record.DateTimeOffset.UtcDateTime))
				{
					OrderBy = GlbDeviceIgnitionSchema.Constants.GDI_MeasurementTimeUtc
				};

			var nextIgnitionRecord = factory.LoadTop1<GlbDeviceIgnition>(nextIgnitionRecordsQuery);

			if (nextIgnitionRecord != null)
			{
				nextIgnitionRecord.GDI_StateChange = nextIgnitionRecord.GDI_State != record.IgnitionState;
			}

			var ignition = factory.New<GlbDeviceIgnition>();
			ignition.GDI_V3_Device = device.PK;
			ignition.GDI_MeasurementTimeUtc = record.DateTimeOffset.UtcDateTime;
			ignition.GDI_State = record.IgnitionState;

			ignition.GDI_StateChange = previousIgnitionRecord == null
				|| previousIgnitionRecord.GDI_State != record.IgnitionState;

			return 1;
		}
	}
}
