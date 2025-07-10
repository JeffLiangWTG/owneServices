using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json.Linq;
using WTG.Telematics.Common.Conversion;

namespace Enterprise.Telematics.ServiceTasks.TelematicsEquipment
{
	public class TelEquipmentDeviceLinker : IDbDeviceLinker
	{
		public TelEquipmentDeviceLinker(ILogger logger)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public void LinkDeviceToSubEquipment(BusinessObjectFactory factory, GlbDevice device, JObject vehicleInfoJson, DateTimeOffset time)
		{
			var hardwareId = vehicleInfoJson["hardwareId"].Value<string>();

			var query = new ZQuery(TelSubEquipmentSchema.TSE_Id, hardwareId);
			var equipment = factory.LoadTop1<TelSubEquipment>(query);
			if (equipment == null)
			{
				logger.Log(LogType.Error, $"MSID: {BinaryDataConverter.ByteArrayToHexString(device.V3_MobileServicesIdentifier)} cannot link to non-existant sub-equipment of id {hardwareId}");
				return;
			}
			device.V3_HardwareIdentifier = hardwareId;
		}
		readonly ILogger logger;
	}
}
