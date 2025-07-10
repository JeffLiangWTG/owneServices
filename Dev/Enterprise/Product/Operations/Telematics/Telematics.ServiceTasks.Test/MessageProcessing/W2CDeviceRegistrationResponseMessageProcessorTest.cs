using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.MobileServices.Common;
using CargoWise.MobileServices.Common.Messages.EHub;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks.MessageProcessing;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Telematics.ServiceTasks.Test.MessageProcessing
{
	class W2CDeviceRegistrationResponseMessageProcessorTest : TestCaseWithFactory
	{
		public void TestMessageType()
		{
			AssertEquals(EHubMessageType.W2CDeviceRegistrationResponse, processor.MessageType);
		}

		[ExpectNoExceptions]
		public void TestProcess_BYODMissing()
		{
			var message = CreateMessage("SOMEID", DeviceRegistrationRequestError.Duplicated);
			processor.Process(Factory, string.Empty, message);

			loggerMock.Verify(l => l.Log(LogType.Warning, "Message recieved for BYOD with a client identifier 'SOMEID' but such device could not be found."), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestProcess_Duplicated()
		{
			var message = CreateMessage("TT00000001", DeviceRegistrationRequestError.Duplicated);
			processor.Process(Factory, string.Empty, message);

			AssertEquals(GlbDeviceStatusList.Codes.RegistrationFailure, byod.V3_Status);
			var note = byod.Notes.GetAllNotes().Cast<StmNote>().First();
			AssertEquals(StmNoteDescription.Pub, note.ST_NoteType);
			AssertEquals("Registration Failure", note.ST_Description);
			AssertEquals("BYOD with this client identifier is already registered.", note.ST_NoteDataAsText);

			loggerMock.Verify(l => l.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Never);
		}

		[ExpectNoExceptions]
		public void TestProcess_DetailsChanged()
		{
			var message = CreateMessage("TT00000001", DeviceRegistrationRequestError.DetailsChanged);
			processor.Process(Factory, string.Empty, message);

			AssertEquals(GlbDeviceStatusList.Codes.RegistrationFailure, byod.V3_Status);
			var note = byod.Notes.GetAllNotes().Cast<StmNote>().First();
			AssertEquals(StmNoteDescription.Pub, note.ST_NoteType);
			AssertEquals("Registration Failure", note.ST_Description);
			AssertEquals("BYOD details have changed which is not allowed. Deregister this device and create a new BYOD.", note.ST_NoteDataAsText);

			loggerMock.Verify(l => l.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Never);
		}

		[ExpectNoExceptions]
		public void TestProcess_NotFound()
		{
			var message = CreateMessage("TT00000001", DeviceRegistrationRequestError.NotFound);
			processor.Process(Factory, string.Empty, message);

			AssertEquals(GlbDeviceStatusList.Codes.DeregistrationFailure, byod.V3_Status);
			var note = byod.Notes.GetAllNotes().Cast<StmNote>().First();
			AssertEquals(StmNoteDescription.Pub, note.ST_NoteType);
			AssertEquals("De-registration Failure", note.ST_Description);
			AssertEquals("Unable to deregister BYOD - device with this client identifier is not registered.", note.ST_NoteDataAsText);

			loggerMock.Verify(l => l.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Never);
		}

		static EHubMessageContainer CreateMessage(string clientIdentifier, DeviceRegistrationRequestError errorCode)
		{
			return new EHubMessageContainer
			{
				message_type = EHubMessageType.W2CDeviceRegistrationResponse,
				message_data = new W2CDeviceRegistrationResponseMessage
				{
					device_client_identifier = clientIdentifier,
					error_code = errorCode,
				}.Serialize()
			};
		}

		W2CDeviceRegistrationResponseMessageProcessor processor;
		Mock<ILogger> loggerMock;
		GlbDevice byod;

		protected override void SetUp()
		{
			base.SetUp();

			loggerMock = new Mock<ILogger>();
			processor = new W2CDeviceRegistrationResponseMessageProcessor(loggerMock.Object);

			byod = Factory.New<GlbDevice>();
			byod.V3_IsBYOD = true;
			byod.V3_HumanReadableIdentifier = "TT00000001";
			byod.V3_Model = "GLaDOS v3.1";
		}
	}
}
