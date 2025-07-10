using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.Telematics.Data.Packets.V2.Commercial.Ingoing;

namespace Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.PayloadProcessors
{
	public class DeviceAlertPayloadRecordProcessor : IPayloadRecordProcessor<DeviceAlertPayloadRecord>
	{
		public int Process(BusinessObjectFactory factory, GlbDevice device, DeviceAlertPayloadRecord record)
		{
			var query = new ZQuery(TelDeviceAlertSchema.TDA_V3_Device, device.PK);
			query.AddToFilter(TelDeviceAlertSchema.TDA_Type, SQLComparisonOperator.Equal, record.AlertType);
			query.AddToFilter(TelDeviceAlertSchema.TDA_IsAcknowledged, SQLComparisonOperator.Equal, false);
			var businessObject = factory.LoadTop1<TelDeviceAlert>(query);

			if (businessObject == null)
			{
				businessObject = factory.New<TelDeviceAlert>();
				businessObject.TDA_V3_Device = device.PK;
				businessObject.TDA_Type = record.AlertType;
				businessObject.TDA_Notes = record.AlertNotes;
				businessObject.TDA_FirstOccurrenceTimeUtc = new ZDateTime(record.DateTimeOffset.UtcDateTime);
			}

			businessObject.TDA_LastOccurrenceTimeUtc = new ZDateTime(record.DateTimeOffset.UtcDateTime);
			businessObject.TDA_Occurrences++;

			return 1;
		}
	}
}
