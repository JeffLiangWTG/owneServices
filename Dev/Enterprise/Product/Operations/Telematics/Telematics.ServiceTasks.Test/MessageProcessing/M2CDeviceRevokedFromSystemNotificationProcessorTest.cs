using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.MobileServices.Common;
using CargoWise.MobileServices.Common.Messages.EHub;
using CargoWise.Types;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks.MessageProcessing;

namespace Enterprise.Telematics.ServiceTasks.Test.MessageProcessing
{
	class M2CDeviceRevokedFromSystemNotificationProcessorTest : TestCaseWithFactory
	{
		public void TestDeactivatesDeviceWithIdentifier()
		{
			processor.Process(Factory, string.Empty, CreateMessage(deviceIdentifier));
			var device = Factory.Load<GlbDevice>(deviceToDeactivatePK);
			AssertEquals("Device should have been deactivated", false, device.V3_IsActive);
			AssertEquals("Status should remain registered", GlbDeviceStatusList.Codes.Registered, device.V3_Status);
		}

		public void TestOtherDevicesRemainActive()
		{
			processor.Process(Factory, string.Empty, CreateMessage(deviceIdentifier));
			var device = Factory.Load<GlbDevice>(anotherInnocentDevicePK);
			AssertEquals("Device should remain active", true, device.V3_IsActive);
			AssertEquals("Status should remain registered", GlbDeviceStatusList.Codes.Registered, device.V3_Status);
		}

		public void TestUnregisterBYOD()
		{
			processor.Process(Factory, string.Empty, CreateMessage(byodIdentifier));
			var device = Factory.Load<GlbDevice>(byodPK);
			AssertEquals("Device should remain active", true, device.V3_IsActive);
			AssertEquals("Status should change to unregistered", GlbDeviceStatusList.Codes.Unregistered, device.V3_Status);
		}

		#region Implementation

		byte[] deviceIdentifier;
		byte[] byodIdentifier;
		ZGuid deviceToDeactivatePK;
		ZGuid anotherInnocentDevicePK;
		ZGuid byodPK;
		M2CDeviceRevokedFromSystemNotificationProcessor processor;

		static EHubMessageContainer CreateMessage(byte[] deviceIdentifier)
		{
			return new EHubMessageContainer
			{
				message_type = EHubMessageType.M2CDeviceRevokedFromSystemNotification,
				message_data = new M2CDeviceRevokedFromSystemNotificationMessage
				{
					device_identifier = deviceIdentifier
				}.Serialize()
			};
		}

		protected override void SetUp()
		{
			base.SetUp();

			deviceIdentifier = new byte[] { 0x01, 0x04, 0x07, 0xFF };
			byodIdentifier = new byte[] { 0x04, 0x04, 0x07, 0xFF };

			var deviceToDeactivate = Factory.New<GlbDevice>();
			deviceToDeactivate.V3_HumanReadableIdentifier = "TT00000001";
			deviceToDeactivate.V3_Status = GlbDeviceStatusList.Codes.Registered;
			deviceToDeactivate.V3_IsActive = true;
			deviceToDeactivate.V3_MobileServicesIdentifier = deviceIdentifier;
			deviceToDeactivate.V3_Model = "GLaDOS v3.1";
			deviceToDeactivatePK = deviceToDeactivate.PK;

			var anotherInnocentDevice = Factory.New<GlbDevice>();
			anotherInnocentDevice.V3_HumanReadableIdentifier = "TT00000002";
			anotherInnocentDevice.V3_Status = GlbDeviceStatusList.Codes.Registered;
			anotherInnocentDevice.V3_IsActive = true;
			anotherInnocentDevice.V3_MobileServicesIdentifier = new Guid().ToByteArray();
			anotherInnocentDevice.V3_Model = "Turret";
			anotherInnocentDevicePK = anotherInnocentDevice.PK;

			var byod = Factory.New<GlbDevice>();
			byod.V3_HumanReadableIdentifier = "TT00000003";
			byod.V3_Status = GlbDeviceStatusList.Codes.Registered;
			byod.V3_IsBYOD = true;
			byod.V3_IsActive = true;
			byod.V3_MobileServicesIdentifier = byodIdentifier;
			byod.V3_Model = "CCTV";
			byodPK = byod.PK;

			Factory.Save();

			processor = new M2CDeviceRevokedFromSystemNotificationProcessor();
		}

		#endregion
	}
}
