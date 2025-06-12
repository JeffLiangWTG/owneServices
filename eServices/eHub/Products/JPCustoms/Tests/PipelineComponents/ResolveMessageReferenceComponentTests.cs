using System;
using System.Data.SqlClient;
using System.Transactions;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.Products.JPCustoms.PipelineComponents;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.JPCustoms.Tests
{
	[TestClass]
	public class ResolveMessageReferenceComponentTests : PipelineComponentBaseTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ResolveMessageReferenceComponent_Properties()
		{
			var component = new ResolveMessageReferenceComponentTest();

			Assert.AreEqual("Resolve Message Reference Component", component.Description);
			Assert.AreEqual("ResolveMessageReferenceComponent", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ResolveMessageReferenceComponent_Enabled()
		{
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			var component = MockRepository.PartialMock<ResolveMessageReferenceComponentTest>();

			MockRepository.ReplayAll();
			var originalStream = message.BodyPart.Data;
			component.Enabled = false;
			component.Execute(pipelineContext, message);
			Assert.IsNull(message.Context.Read("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties"));
			Assert.AreEqual(originalStream, message.BodyPart.Data);
			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ResolveMessageReferenceComponent_RecipientID()
		{
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			var component = MockRepository.PartialMock<ResolveMessageReferenceComponentTest>();

			MockRepository.ReplayAll();
			var originalStream = message.BodyPart.Data;
			component.Enabled = true;
			component.RecipientID = "Recipient1";
			component.ResolveRecipient = false;
			component.Execute(pipelineContext, message);
			Assert.AreEqual("Recipient1", message.Context.Read("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties"));
			Assert.AreEqual(originalStream, message.BodyPart.Data);
			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ResolveMessageReferenceComponent_ResolveRecipient_UsingNewCredential()
		{
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			message.BodyPart.Data = GetEmbeddedResource("TestFiles.JPCustomsReplyMessage.txt");
			message.Context = MessageFactory.CreateMessageContext();

			var component = MockRepository.GeneratePartialMock<ResolveMessageReferenceComponentTest>();

			var registryAccessor = MockRepository.GenerateMock<IRegistryAccessor>();
			component.Stub(x => x.GetRegistryAccessor()).Return(registryAccessor).Repeat.Any();
			registryAccessor.Stub(x => x.ResolveMessageReference("11ABB", "JPC")).Return("");
			var dataModelAccessor = MockRepository.GenerateMock<JPCustomsEhubClientIDDataModelAccessor>();
			component.Stub(x => x.GetJPCustomsDataModelAccessor()).Return(dataModelAccessor);
			dataModelAccessor.Stub(x => x.GetEHubClientIDFromCredentials("11ABB")).Return("HYEDAUIKB");

			MockRepository.ReplayAll();

			message.Context.Write("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "11ABB");

			component.Enabled = true;
			component.ApplicationCode = "JPC";
			component.RecipientID = "BlaBla";
			component.ResolveRecipient = true;

			component.Execute(pipelineContext, message);

			Assert.AreEqual("HYEDAUIKB", message.Context.Read("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties"));
			Assert.AreEqual(0, message.BodyPart.Data.Position);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ResolveMessageReferenceComponent_ResolveRecipient()
		{
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			message.BodyPart.Data = GetEmbeddedResource("TestFiles.JPCustomsReplyMessage.txt");
			message.Context = MessageFactory.CreateMessageContext();

			var component = MockRepository.GeneratePartialMock<ResolveMessageReferenceComponentTest>();

			var registryAccessor = MockRepository.GenerateMock<IRegistryAccessor>();
			component.Stub(x => x.GetRegistryAccessor()).Return(registryAccessor).Repeat.Any();
			registryAccessor.Stub(x => x.ResolveMessageReference("11ABB", "JPC")).Return("HYEDAUIKB");
			var dataModelAccessor = MockRepository.GenerateMock<JPCustomsEhubClientIDDataModelAccessor>();
			component.Stub(x => x.GetJPCustomsDataModelAccessor()).Return(dataModelAccessor);
			dataModelAccessor.Stub(x => x.GetEHubClientIDFromCredentials("11ABB")).Return("");

			MockRepository.ReplayAll();

			message.Context.Write("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "11ABB");

			component.Enabled = true;
			component.ApplicationCode = "JPC";
			component.RecipientID = "BlaBla";
			component.ResolveRecipient = true;

			component.Execute(pipelineContext, message);

			Assert.AreEqual("HYEDAUIKB", message.Context.Read("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties"));
			Assert.AreEqual(0, message.BodyPart.Data.Position);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(ApplicationException), "Unable to resolve recipient. BTS.DestinationParty property was empty string.")]
		public void ResolveMessageReferenceComponent_ResolveRecipient_DestinationPartyNotPromotedException()
		{
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			message.BodyPart.Data = GetEmbeddedResource("TestFiles.JPCustomsReplyMessage.txt");
			message.Context = MessageFactory.CreateMessageContext();

			var component = MockRepository.PartialMock<ResolveMessageReferenceComponentTest>();

			var registryAccessor = MockRepository.StrictMock<IRegistryAccessor>();
			Expect.Call(component.GetRegistryAccessor()).Return(registryAccessor).Repeat.Any();
			Expect.Call(registryAccessor.ResolveMessageReference("11ABB", "JPC")).Return("HYEDAUIKB");

			MockRepository.ReplayAll();

			component.Enabled = true;
			component.ApplicationCode = "JPC";
			component.RecipientID = "BlaBla";
			component.ResolveRecipient = true;

			component.Execute(pipelineContext, message);

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		[ExpectedException(typeof(ApplicationException), "Unable to resolve recipient. ApplicationCode property is empty for ResolveMessageReferenceComponent.")]
		public void ResolveMessageReferenceComponent_ResolveRecipient_ApplicationCodeEmptyException()
		{
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			message.BodyPart.Data = GetEmbeddedResource("TestFiles.JPCustomsReplyMessage.txt");
			message.Context = MessageFactory.CreateMessageContext();

			var component = MockRepository.PartialMock<ResolveMessageReferenceComponentTest>();

			var registryAccessor = MockRepository.StrictMock<IRegistryAccessor>();
			Expect.Call(component.GetRegistryAccessor()).Return(registryAccessor).Repeat.Any();
			Expect.Call(registryAccessor.ResolveMessageReference("11ABB", "JPC")).Return("HYEDAUIKB");

			MockRepository.ReplayAll();

			component.Enabled = true;
			component.RecipientID = "BlaBla";
			component.ResolveRecipient = true;

			message.Context.Write("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "11ABB");

			component.Execute(pipelineContext, message);

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ResolveMessageReferenceComponent_ResolveRecipient_UnableReolveReference()
		{
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);

			message.BodyPart.Data = GetEmbeddedResource("TestFiles.JPCustomsReplyMessage.txt");
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.Write("MessageTrackingID", "http://cargowise.com/ehub/tracking/2010/06", Guid.NewGuid());

			var component = MockRepository.GeneratePartialMock<ResolveMessageReferenceComponentTest>();

			var exceptionAccessor = MockRepository.GenerateMock<IExceptionsAccessor>();
			exceptionAccessor.Expect(_ => _.SubmitErrorAndUpdateStatus(Arg<Guid>.Is.Anything, Arg<string>.Is.Equal("BIZ"), Arg<string>.Is.Equal("Fai"), Arg<string>.Is.Equal("Could not resolve recipient with given username: " + "11ABB"), Arg<Guid>.Is.Equal(Guid.Empty), Arg<Guid>.Is.Equal(Guid.Empty), Arg<Guid>.Is.Equal(message.Context.Read("MessageTrackingID", "http://cargowise.com/ehub/tracking/2010/06")), Arg<Guid>.Is.Equal(Guid.Empty), Arg<SqlConnection>.Is.Equal(null)));

			var registryAccessor = MockRepository.GenerateMock<IRegistryAccessor>();
			component.Stub(x => x.GetRegistryAccessor()).Return(registryAccessor).Repeat.Any();
			registryAccessor.Stub(x => x.ResolveMessageReference("11ABB", "JPC")).Return("");
			var dataModelAccessor = MockRepository.GenerateMock<JPCustomsEhubClientIDDataModelAccessor>();
			component.Stub(x => x.GetJPCustomsDataModelAccessor()).Return(dataModelAccessor);
			dataModelAccessor.Stub(x => x.GetEHubClientIDFromCredentials("11ABB")).Return("");
			component.Expect(x => x.GetExceptionAccessor()).Return(exceptionAccessor).Repeat.Any();

			MockRepository.ReplayAll();

			component.Enabled = true;
			component.ApplicationCode = "JPC";
			component.RecipientID = "BlaBla";
			component.ResolveRecipient = true;

			message.Context.Write("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "11ABB");

			component.Execute(pipelineContext, message);

			exceptionAccessor.VerifyAllExpectations();
			component.VerifyAllExpectations();
		}
	}

	public class ResolveMessageReferenceComponentTest : ResolveMessageReferenceComponent
	{
		protected override TransactionScope GetTransactionScope(Microsoft.BizTalk.Component.Interop.IPipelineContext context)
		{
			return new TransactionScope();
		}
	}
}
