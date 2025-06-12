using System;
using System.Data.SqlClient;
using CargoWise.eHub.Products.Core.PipelineComponents;
using CargoWise.eHub.Products.HKCustoms.PipelineComponents;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eServices.Encryption.Server.Decryptor;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.HKCustoms.Test.PipelineComponents
{
	[TestClass]
	public class HKCustomsSendTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestHKCustoms_InvalidConnectionDetail()
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.HKCustomsEDITestMessage.txt");
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.WriteProperty<BTS.SourceParty>("T_____AUC");
			message.Context.WriteProperty<BTS.DestinationParty>("GLSHK_Test");
			message.Context.WriteProperty<MessageTrackingID>(new Guid("7FED5143-C7B1-4CA5-B35E-87397B99C614"));
			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			var clientRegistrationAccessor = MockRepository.StrictMock<IClientRegistrationAccessor>();
			clientRegistrationAccessor.Expect(x => x.ReadAttr1Password1FirstOrDefault("T_____AUC", "HKC", null, "123456", null, null, null, flag2: 1)).Repeat.Once().Return(null);

			var exceptionAccessor = MockRepository.StrictMock<IExceptionsAccessor>();
			exceptionAccessor.Expect(x => x.SubmitErrorAndUpdateStatus(Arg<Guid>.Is.Anything, Arg<string>.Is.Equal("BIZ"), Arg<string>.Is.Equal("Fai"), Arg<string>.Is.Equal("Could not find connection detail with PIMA: 123456"),
				Arg<Guid>.Is.Anything, Arg<Guid>.Is.Anything, Arg<Guid>.Is.Equal(new Guid("7FED5143-C7B1-4CA5-B35E-87397B99C614")), Arg<Guid>.Is.Equal(new Guid("7FED5143-C7B1-4CA5-B35E-87397B99C614")), Arg<SqlConnection>.Is.Anything)).Repeat.Once();
			
			var component = MockRepository.PartialMock<HKCustomsSend>();
			component.Expect(x => x.GetExceptionAccessor()).Return(exceptionAccessor);
			component.Expect(x => x.GetClientRegistrationAccessor()).Return(clientRegistrationAccessor);

			MockRepository.ReplayAll();

			component.Execute(pipelineContext, message);
			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestHKCustoms_MessageContainsInvalidPIMA()
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.HKCustomsEDITestMessageInvalidPIMA.txt");
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.WriteProperty<BTS.SourceParty>("T_____AUC");
			message.Context.WriteProperty<BTS.DestinationParty>("GLSHK_Test");
			message.Context.WriteProperty<MessageTrackingID>(new Guid("7FED5143-C7B1-4CA5-B35E-87397B99C614"));

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();

			var exceptionAccessor = MockRepository.StrictMock<IExceptionsAccessor>();
			exceptionAccessor.Expect(x => x.SubmitErrorAndUpdateStatus(Arg<Guid>.Is.Anything, Arg<string>.Is.Equal("BIZ"), Arg<string>.Is.Equal("Fai"), Arg<string>.Is.Equal("Message does not contain valid PIMA"),
				Arg<Guid>.Is.Anything, Arg<Guid>.Is.Anything, Arg<Guid>.Is.Equal(new Guid("7FED5143-C7B1-4CA5-B35E-87397B99C614")), Arg<Guid>.Is.Anything, Arg<SqlConnection>.Is.Anything)).Repeat.Once();

			var component = MockRepository.PartialMock<HKCustomsSend>();
			component.Expect(x => x.GetExceptionAccessor()).Return(exceptionAccessor);

			MockRepository.ReplayAll();

			component.Execute(pipelineContext, message);
			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestHKCustoms_ExtractsMessageSuccessfully()
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.HKCustomsEDITestMessage.txt");
			message.Context = MessageFactory.CreateMessageContext();
			var trackingID = new Guid("7FED5143-C7B1-4CA5-B35E-87397B99C614");
			message.Context.WriteProperty<BTS.SourceParty>("T_____AUC");
			message.Context.WriteProperty<BTS.DestinationParty>("GLSHK_Test");
			message.Context.WriteProperty<MessageTrackingID>(trackingID);

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			var clientRegistrationAccessor = MockRepository.StrictMock<IClientRegistrationAccessor>();
			clientRegistrationAccessor.Expect(x => x.ReadAttr1Password1FirstOrDefault("T_____AUC", "HKC", null, "123456", null, null, null, flag2: 1))
				.Repeat.Once().Return(new[] { "ftp://user@host:21/path", "OumLItdgX01g/X/EUtKX9ulBnb0HfXViPIloEtFKgjUZun/CDjnFsOdpeqN92W0F3P1M8HYJlAmsk1+OjCIJnefL8y8UAXcXFpgNgINkD7DgXHcj3BAd0MZq6shu9e0QYROQ9QBEO4fRVlAMxnlCb/l4J4OsoxOF/JImvvzp+Io=" });
			
			var component = MockRepository.PartialMock<HKCustomsSend>();
			component.Expect(x => x.GetClientRegistrationAccessor()).Return(clientRegistrationAccessor);

			MockRepository.ReplayAll();
			component.Execute(pipelineContext, message);

			Assert.AreEqual("ftp://user@host:21/path", message.Context.ReadPropertyString<BTS.OutboundTransportLocation>());
			Assert.AreEqual("user", message.Context.Read("User", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties"));
			Assert.AreEqual("user", message.Context.Read("UserName", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties"));
			Assert.AreEqual(EhubServerDecryptor.Decrypt("OumLItdgX01g/X/EUtKX9ulBnb0HfXViPIloEtFKgjUZun/CDjnFsOdpeqN92W0F3P1M8HYJlAmsk1+OjCIJnefL8y8UAXcXFpgNgINkD7DgXHcj3BAd0MZq6shu9e0QYROQ9QBEO4fRVlAMxnlCb/l4J4OsoxOF/JImvvzp+Io="), message.Context.Read("Password", "http://cargowise.com/ehub/biztalkadapters/transferrer-properties"));
			MockRepository.VerifyAll();
		}
	}
}
