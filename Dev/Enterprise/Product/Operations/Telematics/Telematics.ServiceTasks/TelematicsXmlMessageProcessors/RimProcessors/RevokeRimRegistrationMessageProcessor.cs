using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.Telematics.Data.CargoWiseOne;

namespace Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.RimProcessors
{
	public class RevokeRimRegistrationMessageProcessor : ITcaRegistrationProcessor<RevokeRimRegistrationMessage>
	{
		public RevokeRimRegistrationMessageProcessor(ILogger logger)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		readonly ILogger logger;

		public int Process(BusinessObjectFactory factory, RevokeRimRegistrationMessage message)
		{
			var device = factory.LoadTop1<GlbDevice>(new ZQuery(GlbDeviceSchema.V3_HardwareIdentifier, message.DeviceId));
			var existingRegistration = TcaRegistrationHelper.GetExistingRegistration(factory, device);
			if (existingRegistration == null)
			{
				return 0;
			}

			if (existingRegistration.TE_StartTime > message.DeviceRevokeTime)
			{
				logger.Log(LogType.Error, $"Invalid Rim Revoke for device:{message.DeviceId}. Attempted revoke at time {message.DeviceRevokeTime.ToString()} for device registered at {existingRegistration.TE_StartTime.ToString()}");
				return 0;
			}

			existingRegistration.TE_EndTime = message.DeviceRevokeTime;
			return 1;
		}
	}
}
