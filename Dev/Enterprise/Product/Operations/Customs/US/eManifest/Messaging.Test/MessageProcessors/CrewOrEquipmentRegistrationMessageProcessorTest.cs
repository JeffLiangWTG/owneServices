using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.Customs.US.eManifest.Messaging.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Moq;

namespace Enterprise.Customs.US.eManifest.Messaging.MessageProcessors.Testing
{
	sealed class CrewOrEquipmentRegistrationMessageProcessorTest : ResponseMessageProcessorTest
	{
		public void TestInvalidMessage()
		{
			const string interchangeText = @"UNB+UNOA:4+CBP-ACE-TEST:ZZ+8CWS:ZZ+20120430:0233+253++ACE'UNH+255+MEDPID:D:02A:UN'GIS+23:::2'RFF+AAZ:LOCK'RFF+EQ:1234567890'RFF+ABZ:BA12YY'IHC+1+:::TF'FTX+AAI++AA001+14134'LOC+ZZZ+:162::US+:229::IL'UNT+16+255'UNZ+1+253'";
			const string errorMessage = "The message processor was unable to interpret received message.";
			message = MessagingTestHelper.CreateMessage(Factory, interchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_Status", EDIMessage.Status.Failed, message.EM_Status);
			AssertEquals("EM_LinkUniqueID", ZGuid.Empty, message.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", errorMessage, message.EM_MessageInterpretation);
			AssertEquals("EM_MessageNum", "255", message.EM_MessageNum);
			AssertEquals("EM_GB", GlbBranch.CurrentBranch.PK, message.EM_GB);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.AwaitingOriginal, trip.BH_MessageStatus);
			AssertContains("LastLog", "e-Manifest Response Message Processor: " + errorMessage, new StringCollectionX(logger.UserLogStrings).ToString());
			AssertEmail("e-Manifest Response Message Processor Error Report", errorMessage, message.EM_MessageText, string.Empty);
		}

		public void TestNoAssociatedTransmitMessageFound()
		{
			const string errorMessage = @"No associated transmit message has been found that matches the following details:
Application Code 'MAN', Message Number '255'.";
			trip.Messages[0].Delete();
			message = MessagingTestHelper.CreateMessage(Factory, CrewOrEquipmentRegistrationMessageWrapperTest.CrewAcceptedInterchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_Status", EDIMessage.Status.Failed, message.EM_Status);
			AssertEquals("EM_LinkUniqueID", ZGuid.Empty, message.EM_LinkUniqueID);
			AssertContains("EM_MessageInterpretation", errorMessage, message.EM_MessageInterpretation);
			AssertEquals("EM_MessageNum", "255", message.EM_MessageNum);
			AssertEquals("EM_GB", GlbBranch.CurrentBranch.PK, message.EM_GB);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.AwaitingOriginal, trip.BH_MessageStatus);
			AssertContains("LastLog", "e-Manifest Response Message Processor: " + errorMessage, new StringCollectionX(logger.UserLogStrings).ToString());
			AssertEmail("e-Manifest Response Message Processor Error Report", errorMessage, message.EM_MessageText, string.Empty);
		}

		public void TestAcceptedCrewRegistrationMessage()
		{
			var urlCreator = new Mock<IShowEditFormUrlCreator>();
			urlCreator.Setup(x => x.Create(It.IsAny<IControllerIDProvider>())).Returns("");
			ObjectFactory.Substitute(urlCreator.Object);
			message = MessagingTestHelper.CreateMessage(Factory, CrewOrEquipmentRegistrationMessageWrapperTest.CrewAcceptedInterchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_MessageSubType", EntryStatusList.Codes.Clear, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_MessageNum", "12", message.EM_MessageNum);
			AssertEquals("EM_GB", branch1.PK, message.EM_GB);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ClearOriginal, trip.BH_MessageStatus);
			AssertACEId(trip.CrewMembers[0], "14133");
			var expectedBody = resourceRetriever.GetString("Enterprise.Customs.US.eManifest.Messaging.Testing.TestFiles.CrewRegistrationAcceptedMessageInterpretation.html");
			AssertMultilineASCIIEquals("EM_MessageInterpretation", expectedBody, message.EM_MessageInterpretation.Replace("<tr", "\r\n<tr"));
			AssertEmail("Accepted Crew/Equipment ACE Registration Response for MAN0000001", expectedBody.Replace("\r\n<tr", "<tr"), message.EM_MessageText, "SecondUserToNotify@blah.com");
			message = MessagingTestHelper.CreateMessage(Factory, CrewOrEquipmentRegistrationMessageWrapperTest.CrewAcceptedInterchangeText2);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_MessageSubType", EntryStatusList.Codes.Clear, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_MessageNum", "14", message.EM_MessageNum);
			AssertEquals("EM_GB", branch1.PK, message.EM_GB);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ClearOriginal, trip.BH_MessageStatus);
			AssertACEId(trip.CrewMembers[1], "14134");
			expectedBody = resourceRetriever.GetString("Enterprise.Customs.US.eManifest.Messaging.Testing.TestFiles.CrewRegistrationAcceptedMessageInterpretation2.html");
			AssertMultilineASCIIEquals("EM_MessageInterpretation", expectedBody, message.EM_MessageInterpretation.Replace("<tr", "\r\n<tr"));
			AssertEmail("Accepted Crew/Equipment ACE Registration Response for MAN0000001", expectedBody.Replace("\r\n<tr", "<tr"), message.EM_MessageText, "SecondUserToNotify@blah.com");
		}

		public void TestErrorCrewRegistrationMessage()
		{
			var urlCreator = new Mock<IShowEditFormUrlCreator>();
			urlCreator.Setup(x => x.Create(It.IsAny<IControllerIDProvider>())).Returns("");
			ObjectFactory.Substitute(urlCreator.Object);
			message = MessagingTestHelper.CreateMessage(Factory, CrewOrEquipmentRegistrationMessageWrapperTest.CrewErrorInterchangeText);
			SaveFactoryAndExecuteBatch();
			AssertEquals("EM_Status", EDIMessage.Status.Received, message.EM_Status);
			AssertEquals("EM_MessageSubType", EntryStatusList.Codes.Error, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", trip.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_MessageNum", "16", message.EM_MessageNum);
			AssertEquals("EM_GB", branch1.PK, message.EM_GB);
			AssertEquals("BH_MessageStatus", MessageStatusList.Codes.ErrorOriginal, trip.BH_MessageStatus);
			var expectedBody = resourceRetriever.GetString("Enterprise.Customs.US.eManifest.Messaging.Testing.TestFiles.CrewRegistrationErrorMessageInterpretation.html");
			AssertMultilineASCIIEquals("EM_MessageInterpretation", expectedBody, message.EM_MessageInterpretation.Replace("<tr", "\r\n<tr"));
			AssertEmail("Error Crew/Equipment ACE Registration Response for MAN0000001", expectedBody.Replace("\r\n<tr", "<tr"), message.EM_MessageText, "SecondUserToNotify@blah.com");
		}

		protected override IProcessorForTest GetProcessorForTest()
		{
			var processor = new ProcessForTest(logger);
			message = MessagingTestHelper.CreateMessage(Factory, CrewOrEquipmentRegistrationMessageWrapperTest.CrewErrorInterchangeText);
			Factory.Save();
			processor.ProcessMessage(message);
			return processor;
		}

		protected override void SetUp()
		{
			base.SetUp();
			trip = MessagingTestHelper.GetTripAwaitingReply(Factory, "84f89ebe-e700-4010-ade4-907fcba389e5", "MAN0000001");
			CrewOrEquipmentRegistrationMessageBuilderTest.AddCrew(trip);
			var userToNotify2 = Factory.NewWithValidTestData<GlbStaff>();
			userToNotify2.GS_FullName = "SecondUserToNotify";
			userToNotify2.GS_EmailAddress = "SecondUserToNotify@blah.com";
			userToNotify2.GS_Code = "NU2";
			branch1 = Factory.NewWithValidTestData<GlbBranch>();
			trip.Messages.Add(MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.CrewOrEquipmentRegistration, "12", userToNotify2, branch1, ZDateTime.Now, CrewOrEquipmentRegistrationMessageBuilderTest.CrewRegistrationOriginal));
			trip.Messages.Add(MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.CrewOrEquipmentRegistration, "14", userToNotify2, branch1, ZDateTime.Now, CrewOrEquipmentRegistrationMessageBuilderTest.CrewRegistrationOriginal2));
			trip.Messages.Add(MessagingTestHelper.GetSentEDIMessage(Factory, MessageTypes.Codes.CrewOrEquipmentRegistration, "16", userToNotify2, branch1, ZDateTime.Now, CrewOrEquipmentRegistrationMessageBuilderTest.CrewRegistrationOriginal3));
			processor = new MessageProcessor { Logger = logger };
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
		}

		Trip trip;
		MessageProcessor processor;
		EDIMessage message;
		GlbBranch branch1;
		EmbeddedResourceRetriever resourceRetriever;

		void AssertACEId(CrewMember crewMember, string refNumber)
		{
			var cert = crewMember.Certificates.GetFirstCertificate(CrewACEIdTypes.Codes.Id);
			AssertEquals("XZ_Comment", CrewACEIdTypes.Descriptions.Id, cert.XZ_Comment);
			AssertEquals("XZ_RefNumber", refNumber, cert.XZ_RefNumber);
		}

		void SaveFactoryAndExecuteBatch()
		{
			Factory.Save();
			processor.ExecuteBatch();
			var factory = new BusinessObjectFactory();
			message = factory.Load<EDIMessage>(message.PK);
			trip = factory.Load<Trip>(trip.PK);
		}

		sealed class ProcessForTest : CrewOrEquipmentRegistrationMessageProcessor, IProcessorForTest
		{
			public ProcessForTest(LoggingInformation logger) : base(logger)
			{
			}

			public new string AcknowledgementEmailMode => base.AcknowledgementEmailMode;
			public new string ImpedimentEmailMode => base.ImpedimentEmailMode;
			public new string ErrorEmailMode => base.ErrorEmailMode;
			public new EDIMessage originalMessage => base.originalMessage;
		}
	}
}
