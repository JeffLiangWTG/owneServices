using System;
using System.Data.SqlClient;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.Products.AirMessaging.PipelineComponents;
using CargoWise.eServices.Encryption.Server.Decryptor;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Tests;

namespace CargoWise.eHub.Products.AirMessaging.Tests
{
    [TestClass]
	public class AirMessageContextWriterComponentTest : BaseComponentTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAirMessagingSend_LookupFTP_NotExistFTPConnection()
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.GLSHKAirEDITestMessage.txt");
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.WriteProperty<BTS.SourceParty>("T_____AUC");
			message.Context.WriteProperty<OverrideEmailSubject>("RHKAGT021330005 HKG85");
			message.Context.WriteProperty<BTS.DestinationParty>("GLSHK_Test");
			message.Context.WriteProperty<MessageTrackingID>(new Guid("7FED5143-C7B1-4CA5-B35E-87397B99C614"));
            message.Context.WriteProperty<BTS.MessageType>("FWB");
			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			var clientRegistrationAccessor = MockRepository.StrictMock<IClientRegistrationAccessor>();
			clientRegistrationAccessor.Expect(x => x.ReadAttr1Password1FirstOrDefault("T_____AUC", "GLSHK", null, "RHKAGT021330005/HKG85", null, null, null, flag2: 1)).Repeat.Once().Return(null);

			var exceptionAccessor = MockRepository.StrictMock<IExceptionsAccessor>();
			exceptionAccessor.Expect(x => x.SubmitErrorAndUpdateStatus(Arg<Guid>.Is.Anything, Arg<string>.Is.Equal("BIZ"), Arg<string>.Is.Equal("Fai"), Arg<string>.Is.Equal("Could not find connection detail with PIMA: RHKAGT021330005/HKG85"),
				Arg<Guid>.Is.Anything, Arg<Guid>.Is.Anything, Arg<Guid>.Is.Equal(new Guid("7FED5143-C7B1-4CA5-B35E-87397B99C614")), Arg<Guid>.Is.Equal(new Guid("7FED5143-C7B1-4CA5-B35E-87397B99C614")), Arg<SqlConnection>.Is.Anything)).Repeat.Once();

			var component = MockRepository.PartialMock<AirMessageContextWriterComponent>();
            component.FTPConfigurationID = "GLSHK";
            component.Expect(x => x.GetExceptionAccessor()).Return(exceptionAccessor);
			component.Expect(x => x.GetClientRegistrationAccessor()).Return(clientRegistrationAccessor);

			MockRepository.ReplayAll();

			component.Execute(pipelineContext, message);
			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAirMessagingSend_Success()
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.GLSHKAirEDITestMessage.txt");
			message.Context = MessageFactory.CreateMessageContext();
			var trackingID = new Guid("7FED5143-C7B1-4CA5-B35E-87397B99C614");
			message.Context.WriteProperty<BTS.SourceParty>("T_____AUC");
			message.Context.WriteProperty<BTS.DestinationParty>("GLSHK_Test");
			message.Context.WriteProperty<OverrideEmailSubject>("RHKAGT021330005 HKG85");
			message.Context.WriteProperty<MessageTrackingID>(trackingID);
            message.Context.WriteProperty<BTS.MessageType>("FHL");
            message.Context.WriteProperty<OverrideFilename>("FHL_17631640442");

            var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			var clientRegistrationAccessor = MockRepository.StrictMock<IClientRegistrationAccessor>();
			clientRegistrationAccessor.Expect(x => x.ReadAttr1Password1FirstOrDefault("T_____AUC", "GLSHK", null, "RHKAGT021330005/HKG85", null, null, null, flag2: 1))
				.Repeat.Once().Return(new[] { "ftp://user@host:21/path", "OumLItdgX01g/X/EUtKX9ulBnb0HfXViPIloEtFKgjUZun/CDjnFsOdpeqN92W0F3P1M8HYJlAmsk1+OjCIJnefL8y8UAXcXFpgNgINkD7DgXHcj3BAd0MZq6shu9e0QYROQ9QBEO4fRVlAMxnlCb/l4J4OsoxOF/JImvvzp+Io=" });

			var component = MockRepository.PartialMock<AirMessageContextWriterComponent>();
            component.FTPConfigurationID = "GLSHK";
            component.Expect(x => x.GetClientRegistrationAccessor()).Return(clientRegistrationAccessor);

			MockRepository.ReplayAll();
			component.Execute(pipelineContext, message);

			Assert.AreEqual("ftp://user@host:21/path", message.Context.ReadPropertyString<BTS.OutboundTransportLocation>());
			Assert.AreEqual("user", message.Context.Read("User", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties"));
			Assert.AreEqual(EhubServerDecryptor.Decrypt("OumLItdgX01g/X/EUtKX9ulBnb0HfXViPIloEtFKgjUZun/CDjnFsOdpeqN92W0F3P1M8HYJlAmsk1+OjCIJnefL8y8UAXcXFpgNgINkD7DgXHcj3BAd0MZq6shu9e0QYROQ9QBEO4fRVlAMxnlCb/l4J4OsoxOF/JImvvzp+Io="), message.Context.Read("Password", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties"));
            MockRepository.VerifyAll();
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestAirMessagingSend_Success_SFTP()
        {
            var message = MessageFactory.CreateMessage();
            message.AddPart("message", MessageFactory.CreateMessagePart(), true);
            message.BodyPart.Data = GetEmbeddedResource("TestFiles.GLSHKAirEDITestMessage.txt");
            message.Context = MessageFactory.CreateMessageContext();
            var trackingID = new Guid("7FED5143-C7B1-4CA5-B35E-87397B99C614");
            message.Context.WriteProperty<BTS.SourceParty>("T_____AUC");
            message.Context.WriteProperty<BTS.DestinationParty>("Tradevan");
            message.Context.WriteProperty<OverrideEmailSubject>("RHKAGT021330005 HKG85");
            message.Context.WriteProperty<MessageTrackingID>(trackingID);
            message.Context.WriteProperty<BTS.MessageType>("FHL");
            message.Context.WriteProperty<OverrideFilename>("FHL_17631640442");

            var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
            var clientRegistrationAccessor = MockRepository.StrictMock<IClientRegistrationAccessor>();
            clientRegistrationAccessor.Expect(x => x.ReadAttr1Password1FirstOrDefault("T_____AUC", "Tradevan", null, "RHKAGT021330005/HKG85", null, null, null, flag2: 1))
                .Repeat.Once().Return(new[] { "sftp://user@host:21/path", "OumLItdgX01g/X/EUtKX9ulBnb0HfXViPIloEtFKgjUZun/CDjnFsOdpeqN92W0F3P1M8HYJlAmsk1+OjCIJnefL8y8UAXcXFpgNgINkD7DgXHcj3BAd0MZq6shu9e0QYROQ9QBEO4fRVlAMxnlCb/l4J4OsoxOF/JImvvzp+Io=" });

            var component = MockRepository.PartialMock<AirMessageContextWriterComponent>();
            component.FTPConfigurationID = "Tradevan";
            component.Expect(x => x.GetClientRegistrationAccessor()).Return(clientRegistrationAccessor);

            MockRepository.ReplayAll();
            component.Execute(pipelineContext, message);

            Assert.AreEqual("sftp://user@host:21/path", message.Context.ReadPropertyString<BTS.OutboundTransportLocation>());
            Assert.AreEqual("user", message.Context.Read("User", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties"));
            Assert.AreEqual("user", message.Context.Read("UserName", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties"));
			Assert.AreEqual(EhubServerDecryptor.Decrypt("OumLItdgX01g/X/EUtKX9ulBnb0HfXViPIloEtFKgjUZun/CDjnFsOdpeqN92W0F3P1M8HYJlAmsk1+OjCIJnefL8y8UAXcXFpgNgINkD7DgXHcj3BAd0MZq6shu9e0QYROQ9QBEO4fRVlAMxnlCb/l4J4OsoxOF/JImvvzp+Io="), message.Context.Read("Password", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties"));
            Assert.AreEqual("FHL.17631640442", message.Context.ReadPropertyString<OverrideFilename>());
            MockRepository.VerifyAll();
        }
    }
}

