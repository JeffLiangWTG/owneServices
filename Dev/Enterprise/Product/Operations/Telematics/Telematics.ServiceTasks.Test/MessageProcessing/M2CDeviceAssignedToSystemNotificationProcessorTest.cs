using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.MobileServices.Common;
using CargoWise.MobileServices.Common.Messages;
using CargoWise.MobileServices.Common.Messages.EHub;
using CargoWise.Types;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks.MessageProcessing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Telematics.ServiceTasks.Test.MessageProcessing
{
	class M2CDeviceAssignedToSystemNotificationProcessorTest : TestCaseWithFactory
	{
		public void TestUpdatesExistingDevice()
		{
			processor.Process(Factory, string.Empty, CreateMessage(deviceAlreadyInSystemIdentifier));
			var device = Factory.Load<GlbDevice>(deviceAlreadyInSystemPK);

			AssertEquals("CW00000123", device.V3_HumanReadableIdentifier);
			AssertEquals("World Domination Bot", device.V3_Model);
			AssertEquals("Device should be flagged as active", true, device.V3_IsActive);
			AssertEquals("Device kind", GlbDeviceKindCodes.AppleMobile, device.V3_HardwareKind);
			AssertEquals("Device identifier", "some-iphone", device.V3_HardwareIdentifier);
		}

		public void TestCreatesNewDevice()
		{
			var device = GlbDevice.FindDeviceByMobileServicesIdentifier(Factory, deviceNotAlreadyInSystemIdentifier);
			AssertNull("Sanity check - device should not exist", device);

			processor.Process(Factory, string.Empty, CreateMessage(deviceNotAlreadyInSystemIdentifier));
			Factory.Save();

			device = GlbDevice.FindDeviceByMobileServicesIdentifier(Factory, deviceNotAlreadyInSystemIdentifier);
			AssertNotNull("Should have created a new device", device);
			AssertEquals("Device should remain active", true, device.V3_IsActive);
			AssertEquals("CW00000123", device.V3_HumanReadableIdentifier);
			AssertEquals("World Domination Bot", device.V3_Model);
			AssertEquals("Device should be flagged as active", true, device.V3_IsActive);
			Assert("Device Mobile Services ID should match Mobile Services ID", deviceNotAlreadyInSystemIdentifier.SequenceEqual((byte[])device.V3_MobileServicesIdentifier));
			AssertEquals("Device kind", GlbDeviceKindCodes.AppleMobile, device.V3_HardwareKind);
			AssertEquals("Device identifier", "some-iphone", device.V3_HardwareIdentifier);
		}

		public void TestBYODRegistationResponseMessage()
		{
			var byod = Factory.New<GlbDevice>();
			byod.V3_IsBYOD = true;
			byod.V3_HumanReadableIdentifier = "AAA999";
			byod.V3_Status = GlbDeviceStatusList.Codes.PendingRegistration;
			byod.V3_Model = "planet 13";
			Factory.Save();

			processor.Process(Factory, string.Empty, CreateMessage(deviceNotAlreadyInSystemIdentifier, true, "AAA999"));
			AssertEquals(GlbDeviceStatusList.Codes.Registered, byod.V3_Status);
			AssertEquals(deviceNotAlreadyInSystemIdentifier, byod.V3_MobileServicesIdentifier);
			AssertEquals(0, byod.Notes.DatabaseCount);
		}

		public void TestBYODRegistationResponseMessage_UnexpectedStatus()
		{
			var byod = Factory.New<GlbDevice>();
			byod.V3_IsBYOD = true;
			byod.V3_HumanReadableIdentifier = "AAA999";
			byod.V3_Status = GlbDeviceStatusList.Codes.Unregistered;
			byod.V3_Model = "planet 13";
			Factory.Save();

			processor.Process(Factory, string.Empty, CreateMessage(deviceNotAlreadyInSystemIdentifier, true, "AAA999"));
			AssertEquals(GlbDeviceStatusList.Codes.Registered, byod.V3_Status);
			AssertEquals(deviceNotAlreadyInSystemIdentifier, byod.V3_MobileServicesIdentifier);
			AssertEquals(1, byod.Notes.DatabaseCount);
			var note = byod.Notes.GetAllNotes().Cast<StmNote>().First();
			AssertEquals(StmNoteDescription.Pub, note.ST_NoteType);
			AssertEquals("Unexpected Device Status", note.ST_Description);
			AssertEquals("BYOD status expected to be 'Pending Registration' but was 'Unregistered' instead.", note.ST_NoteDataAsText);
		}

		#region Implementation

		byte[] deviceAlreadyInSystemIdentifier;
		byte[] deviceNotAlreadyInSystemIdentifier;
		ZGuid deviceAlreadyInSystemPK;
		M2CDeviceAssignedToSystemNotificationProcessor processor;

		static EHubMessageContainer CreateMessage(byte[] deviceIdentifier, bool isBYOD = false, string byodClientIdentifier = "World Domination Bot")
		{
			return new EHubMessageContainer
			{
				message_type = EHubMessageType.M2CDeviceAssignedToSystemNotification,
				message_data = new M2CDeviceAssignedToSystemNotificationMessage
				{
					device_identifier = deviceIdentifier,
					device_friendly_identifier = "CW00000123",
					device_manufacturer = "WiseTech Global",
					device_model = "World Domination Bot",
					is_byod = isBYOD,
					device_client_identifier = byodClientIdentifier,
					device_key = new DeviceKey
					{
						kind = DeviceKind.AppleMobile,
						identifier = "some-iphone",
					}
				}.Serialize()
			};
		}

		protected override void SetUp()
		{
			base.SetUp();

			deviceAlreadyInSystemIdentifier = new byte[] { 0x01, 0x04, 0x07, 0xFF }; // No significance
			deviceNotAlreadyInSystemIdentifier = new byte[] { 0x07, 0x07, 0xAA, 0xED }; // No significance

			var deviceAlreadyInSystem = Factory.New<GlbDevice>();
			deviceAlreadyInSystem.V3_HumanReadableIdentifier = "TT00000001";
			deviceAlreadyInSystem.V3_IsActive = false;
			deviceAlreadyInSystem.V3_MobileServicesIdentifier = deviceAlreadyInSystemIdentifier;
			deviceAlreadyInSystem.V3_Model = "GLaDOS v3.1";
			deviceAlreadyInSystemPK = deviceAlreadyInSystem.PK;

			Factory.Save();

			processor = new M2CDeviceAssignedToSystemNotificationProcessor();
		}

		protected override void TearDown()
		{
			processor = null;
			deviceAlreadyInSystemIdentifier = null;
			deviceNotAlreadyInSystemIdentifier = null;

			base.TearDown();
		}

		#endregion
	}
}
