using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.MobileServices.Common;
using CargoWise.MobileServices.Common.Messages.EHub;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Telematics.ServiceTasks.Test.MessageTypeProcessors
{
	// Case for this test:
	// 1. Create GlbDevice in Factory
	// 2. Receive location - look up in SQL by MobileServices ID
	// 3. Crash - GlbDevice only existed in Factory, not in DB.
	public class MobileServicesEHubMessageProcessor_RunsSuccesfullyWhenCreatingDeviceAndLocationsFromSameEHubMessage : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestRunsWithoutErrors()
		{
			var mockLogger = new Mock<ILogger>();
			var task = new EHubMessageProcessor(mockLogger.Object, Factory);
			task.Run(CancellationToken.None);

			mockLogger.Verify(x => x.Log(LogType.Error, It.IsAny<string>()), Times.Never());
			mockLogger.Verify(x => x.Log(LogType.Error, It.IsAny<string>(), It.IsAny<Exception>()), Times.Never());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		protected override void SetUp()
		{
			base.SetUp();

			var messages = new List<EHubMessageContainer>
			{
				new EHubMessageContainer
				{
					message_type = EHubMessageType.M2CDeviceAssignedToSystemNotification,
					message_data = new M2CDeviceAssignedToSystemNotificationMessage
					{
						device_friendly_identifier = "AAA",
						device_identifier = new byte[] { 0x01, 0x02, 0x99, 0x01 },
						device_manufacturer = "AAA",
						device_model = "AAA"
					}.Serialize()
				},
				new EHubMessageContainer
				{
					message_type = EHubMessageType.M2CDeviceLocationDataNotification,
					message_data = new M2CDeviceLocationDataNotificationMessage
					{
						device_identifier = new byte[] { 0x01, 0x02, 0x99, 0x01 },
						device_locations =
						{
							new M2CDeviceLocationDataNotificationMessage.DeviceLocation
							{
								accuracy_in_metres = 1,
								altitude_in_metres_decimal = "1",
								compass_heading_degrees_decimal = "1",
								latitude_decimal = "1",
								longitude_decimal = "1",
								sample_quantity = 1,
								speed_kmph_decimal = "1",
								time_from_utc = DateTime.UtcNow.AsPosixTime(),
								time_to_utc = DateTime.UtcNow.AsPosixTime()
							}
						}
					}.Serialize()
				}
			};

			var interchangeBody = EHubMessageSerializer.SerializeToLimit(messages, EHubMessageSerializer.DefaultMessageSizeLimitInBytes);
			AssertEquals("Should have consumed all messages in the list", 0, messages.Count);

			EHubHelpers.CreateEHubMessage(
				Factory,
				ApplicationCodeList.Codes.Telematics,
				EDIMessageStatusList.Codes.Queued,
				true,
				EDIMessageTypeList.Codes.XDC,
				TelematicsMessageList.Codes.ProtobufData,
				interchangeBody.ToString(),
				ReceiveTransmitList.Codes.Receive);

			Factory.Save();
		}
	}
}
