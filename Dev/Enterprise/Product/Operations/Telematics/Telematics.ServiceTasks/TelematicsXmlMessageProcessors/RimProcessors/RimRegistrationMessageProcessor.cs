using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.Telematics.Data.CargoWiseOne;

namespace Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.RimProcessors
{
	class RimRegistrationMessageProcessor : ITcaRegistrationProcessor<RimRegistrationMessage>
	{
		public RimRegistrationMessageProcessor(ILogger logger)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		readonly ILogger logger;

		public int Process(BusinessObjectFactory factory, RimRegistrationMessage message)
		{
			var device = factory.LoadTop1<GlbDevice>(new ZQuery(GlbDeviceSchema.V3_HardwareIdentifier, message.DeviceId));
			if (device == null)
			{
				logger.Log(LogType.Error, $"Device with HID: {message.DeviceId} does not have an associated GlbDevice");
				return 0;
			}
			var existingRegistration = TcaRegistrationHelper.GetExistingRegistration(factory, device);
			if (existingRegistration != null)
			{
				return 0;
			}

			var divot = TcaRegistrationHelper.GetDivot(factory, device, message);
			if (divot == null)
			{
				logger.Log(LogType.Error, $"Device with HID: {message.DeviceId} does not have linked RefEquipment and GlbDevice");
				return 0;
			}

			var newRegistration = factory.New<TelEdge>();
			newRegistration.TE_EntityIdTo = device.PK;
			newRegistration.TE_EntityIdFrom = divot.PK;
			newRegistration.TE_RelationshipType = TelEdgeRelationshipTypes.Codes.RIM;
			newRegistration.TE_StartTime = message.DeviceAssignmentTime;
			newRegistration.TE_EntityTableCodeFrom = TelEdgeEntityTableCodes.Codes.RQ;
			newRegistration.TE_EntityTableCodeTo = TelEdgeEntityTableCodes.Codes.V3;

			return 1;
		}
	}
}
