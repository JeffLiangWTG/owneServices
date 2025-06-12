using System;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Transactions;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.Products.Core.PipelineComponents;
using CargoWise.eHub.Products.Core.PropertySchemas;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.Core.Tests
{
	[TestClass]
	public class MessageReferenceResolveProductComponentTests : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestMessageReferenceResolveProductComponent()
		{
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("Main", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream(Encoding.UTF8.GetBytes("Test message"));
			message.Context.WriteProperty<MessageTrackingID>(Guid.NewGuid());

			var exceptionAccessor = MockRepository.GenerateStrictMock<IExceptionsAccessor>();
			exceptionAccessor.Expect(x => x.SubmitErrorAndUpdateStatus(Arg<Guid>.Is.Anything, Arg<string>.Is.Equal("BIZ"), Arg<string>.Is.Equal("Fai"), Arg<string>.Is.Equal("Could not resolve recipient with application code: NZC and reference: 12."), Arg<Guid>.Is.Equal(Guid.Empty), Arg<Guid>.Is.Equal(Guid.Empty), Arg<Guid>.Is.Equal(Guid.Parse(message.Context.ReadPropertyString<MessageTrackingID>())), Arg<Guid>.Is.Equal(Guid.Empty), Arg<SqlConnection>.Is.Equal(null)));

			var registryAccessor = MockRepository.GenerateStrictMock<IRegistryAccessor>();
			registryAccessor.Expect(x => x.ResolveMessageReference("12", "NZC")).Return("");
			registryAccessor.Expect(x => x.ResolveMessageReference("123", "NZC")).Return("recipient1");

			var component = MockRepository.GeneratePartialMock<MessageReferenceResolveProductComponent>();
			component.Expect(x => x.GetTransactionScope(Arg<IPipelineContext>.Is.Anything))
				.Do((Func<IPipelineContext, TransactionScope>)delegate(IPipelineContext dummy) { return new TransactionScope(); }); // need to recreate TransactionScope each call
			component.Expect(x => x.GetRegistryAccessor()).Return(registryAccessor).Repeat.Any();
			component.Expect(x => x.GetExceptionAccessor()).Return(exceptionAccessor).Repeat.Any();

			component.Enabled = false;
			component.SenderID = "";
			component.Execute(pipelineContext, message);

			component.Enabled = true;
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ArgumentNullException), "Specify SenderID in pipeline settings");

			component.SenderID = "sender";
			component.RecipientID = "recipient";
			component.ResolveRecipient = false;
			component.Execute(pipelineContext, message);
			Assert.AreEqual("recipient", message.Context.ReadPropertyString<BTS.DestinationParty>());

			component.ResolveRecipient = true;
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ArgumentNullException), "CustomerReference field was not promoted.");

			message.Context.WriteProperty<Reference>("12");
			AssertException(() => { component.Execute(pipelineContext, message); }, typeof(ArgumentNullException), "ApplicationCode field was not promoted.");

			message.Context.WriteProperty<ApplicationCode>("NZC");
			component.Execute(pipelineContext, message);

			message.Context.WriteProperty<Reference>("123");
			var result = component.Execute(pipelineContext, message);
			Assert.AreEqual("recipient1", message.Context.ReadPropertyString<BTS.DestinationParty>());
			Assert.AreEqual("sender", message.Context.ReadPropertyString<BTS.SourceParty>());
			Assert.AreEqual("recipient1", message.Context.ReadPropertyString<BTS.DestinationParty>());
			Assert.AreEqual(message, result);

			exceptionAccessor.VerifyAllExpectations();
			registryAccessor.VerifyAllExpectations();
			component.VerifyAllExpectations();
		}
	}
}

