using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using Microsoft.BizTalk.Component;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Component.Utilities;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class EdiDisassemblerExtensionTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEdiDisassemblerExtension()
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.EdiDisassembleExtensionTestMsg.txt");
			message.Context = MessageFactory.CreateMessageContext();

			var newMessage1 = MessageFactory.CreateMessage();
			newMessage1.AddPart("newMessage1", MessageFactory.CreateMessagePart(), true);
			newMessage1.BodyPart.Data = GetEmbeddedResource("TestFiles.EdiDisassembleExtensionNewMsg1.xml");
			newMessage1.Context = MessageFactory.CreateMessageContext();

			var newMessage2 = MessageFactory.CreateMessage();
			newMessage2.AddPart("newMessage2", MessageFactory.CreateMessagePart(), true);
			newMessage2.BodyPart.Data = GetEmbeddedResource("TestFiles.EdiDisassembleExtensionNewMsg2.xml");
			newMessage2.Context = MessageFactory.CreateMessageContext();

			var newMessage3 = MessageFactory.CreateMessage();
			newMessage3.AddPart("newMessage3", MessageFactory.CreateMessagePart(), true);
			newMessage3.BodyPart.Data = GetEmbeddedResource("TestFiles.EdiDisassembleExtensionNewMsg3.xml");
			newMessage3.Context = MessageFactory.CreateMessageContext();

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();

			var msgHelper = MockRepository.StrictMock<IMessageHelper>();
			Expect.Call(delegate() { msgHelper.EnqueueMessage(null, null, null, null, false, false); }).IgnoreArguments()
				.Callback(new Func<IPipelineContext, Queue, IBaseMessage, IBaseMessage, bool, bool, bool>((p, q, m, u, c, d) => { q.Enqueue(m); return true; }))
				.Repeat.Any();

			var component = MockRepository.PartialMock<EdiDissasemblerExtension>();
			Expect.Call(() => component.BaseDisassemble(pipelineContext, message));
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(newMessage1);
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(newMessage2);
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(newMessage3);
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(null);
			Expect.Call(component.GetMessageHelper()).Return(msgHelper).Repeat.Any();

			MockRepository.ReplayAll();

			component.Disassemble(pipelineContext, message);
			Assert.AreEqual(newMessage1, component.GetNext(pipelineContext));
			Assert.AreEqual(newMessage2, component.GetNext(pipelineContext));
			Assert.AreEqual(newMessage3, component.GetNext(pipelineContext));
			Assert.IsNull(component.GetNext(pipelineContext));

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestDebatchByXPath()
		{
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.ConsolsInternalWithTwoConsolsAndTwoContainersEach.xml");

			var pipelineContext = new PipelineContext();

			var dasmXmlHelper = MockRepository.StrictMock<IXmlDisassembleHelper>();
			Expect.Call(dasmXmlHelper.Disassemble(pipelineContext, message, null, null))
				.Return(message);
			Expect.Call(dasmXmlHelper.Disassemble(pipelineContext, message))
				.Return(message);

			var msgHelper = MockRepository.StrictMock<IMessageHelper>();
			Expect.Call(delegate() { msgHelper.EnqueueMessage(null, null, null, null, false, false); }).IgnoreArguments()
				.Callback(new Func<IPipelineContext, Queue, IBaseMessage, IBaseMessage, bool, bool, bool>((p, q, m, u, c, d) => { q.Enqueue(m); return true; }))
				.Repeat.Any();

			var component = MockRepository.PartialMock<SchemaResolveDisassembleComponent>();
			Expect.Call(component.GetXmlDisassembler()).Return(dasmXmlHelper).Repeat.Any();
			Expect.Call(component.GetMessageHelper()).Return(msgHelper).Repeat.Any();

			MockRepository.ReplayAll();

			component.DisassembleType = SchemaResolveDisassembleComponent.DasmType.XML;
			component.XPathDebatchEnabled = true;
			component.XPathToDebatch = "/*[local-name()='ConsolsInternal' and namespace-uri()='http://www.edi.com.au/EnterpriseService/']/*[local-name()='Payload' and namespace-uri()='http://www.edi.com.au/EnterpriseService/']/*[local-name()='Consols' and namespace-uri()='http://www.edi.com.au/EnterpriseService/']/*[local-name()='Consol' and namespace-uri()='http://www.edi.com.au/EnterpriseService/']/*[local-name()='ConsolDetail' and namespace-uri()='http://www.edi.com.au/EnterpriseService/']/*[local-name()='Containers' and namespace-uri()='http://www.edi.com.au/EnterpriseService/']/*[local-name()='Container' and namespace-uri()='http://www.edi.com.au/EnterpriseService/']";
			component.Disassemble(pipelineContext, message);

			var result = component.GetNext(pipelineContext);
			Assert.IsNotNull(result);
			AssertXmlStream(GetEmbeddedResource("TestFiles.ConsolsInternalWithFirstConsolAndFirstContainer.xml"), result.BodyPart.Data);

			result = component.GetNext(pipelineContext);
			Assert.IsNotNull(result);
			AssertXmlStream(GetEmbeddedResource("TestFiles.ConsolsInternalWithWithFirstConsolAndSecondContainer.xml"), result.BodyPart.Data);

			result = component.GetNext(pipelineContext);
			Assert.IsNotNull(result);
			AssertXmlStream(GetEmbeddedResource("TestFiles.ConsolsInternalWithWithSecondConsolAndFirstContainer.xml"), result.BodyPart.Data);

			result = component.GetNext(pipelineContext);
			Assert.IsNotNull(result);
			AssertXmlStream(GetEmbeddedResource("TestFiles.ConsolsInternalWithWithSecondConsolAndSecondContainer.xml"), result.BodyPart.Data);

			Assert.IsNull(component.GetNext(pipelineContext));

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestEnvelopeDasm()
		{
			var envMessage = MessageFactory.CreateMessage();
			envMessage.Context = MessageFactory.CreateMessageContext();
			envMessage.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			envMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.Message_EnvelopeDasm.xml");

			var bodyMessage = MessageFactory.CreateMessage();
			bodyMessage.Context = MessageFactory.CreateMessageContext();
			bodyMessage.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			bodyMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.Message_EnvelopeDasmBody.xml");

			var pipelineContext = new PipelineContext();

			var envSchema = new Schema("CargoWise.eHub.Gateway.Schemas.SendRequestStream, CargoWise.eHub.Gateway.Schemas, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350");
			var dasmComp = MockRepository.StrictMock<IDisassemblerComponent>();
			Expect.Call(dasmComp.GetNext(pipelineContext)).Return(bodyMessage);
			Expect.Call(dasmComp.GetNext(pipelineContext)).Throw(new XmlDasmException(-1061153688, string.Empty));

			var dasmXmlHelper = MockRepository.PartialMock<XmlDisassembleHelper>(dasmComp);
			Expect.Call(dasmXmlHelper.Disassemble(pipelineContext, envMessage, envSchema, null))
				.Callback(new Func<IPipelineContext, IBaseMessage, Schema, Schema, bool>((p, m, e, b) => { dasmXmlHelper.GetNext(p); return true; }))
				.Return(bodyMessage);
			Expect.Call(dasmXmlHelper.Disassemble(pipelineContext, bodyMessage))
				.Return(bodyMessage).Repeat.Twice();

			var msgHelper = MockRepository.StrictMock<IMessageHelper>();
			Expect.Call(delegate() { msgHelper.EnqueueMessage(null, null, null, null, false, false); }).IgnoreArguments()
				.Callback(new Func<IPipelineContext, Queue, IBaseMessage, IBaseMessage, bool, bool, bool>((p, q, m, u, c, d) => { q.Enqueue(m); return true; }))
				.Repeat.Any();

			var component = MockRepository.PartialMock<SchemaResolveDisassembleComponent>();
			Expect.Call(component.GetXmlDisassembler()).Return(dasmXmlHelper).Repeat.Any();
			Expect.Call(component.GetMessageHelper()).Return(msgHelper).Repeat.Any();

			MockRepository.ReplayAll();

			component.DisassembleType = SchemaResolveDisassembleComponent.DasmType.XML;
			component.EnvelopeSchema = envSchema;
			component.FlatFileDataXPath = "/*[local-name()='Message']";
			component.Disassemble(pipelineContext, envMessage);

			var result = component.GetNext(pipelineContext);
			Assert.IsNotNull(result);
			Assert.IsNull(component.GetNext(pipelineContext));

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_SchemaResolveDisassembleComponent()
		{
			var component = new EdiDissasemblerExtension();
			Assert.AreEqual(string.Empty, component.Description);
			Assert.AreEqual("EDI Disassembler Extension", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
			var guid = new Guid();
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("F2E12BD7-DEE6-4A40-B775-8D9B452AF881"), guid);

			var propertyBag = MockRepository.StrictMock<IPropertyBag>();
			string processSubscriptionsProp = null;
			bool processSubscriptionsValue = false;
			object processSubscriptionsPtr = false;
			string discardUnsubscribedMessagesProp = null;
			bool discardUnsubscribedMessagesValue = false;
			object discardUnsubscribedMessagesPtr = false;
			string normalizeLineEndingsProp = null;
			bool normalizeLineEndingsValue = false;
			object normalizeLineEndingsPtr = false;
			object dummy;

			Expect.Call(() => propertyBag.Write(null, null)).IgnoreArguments().Repeat.Times(15);
			Expect.Call(() => propertyBag.Write("ProcessSubscriptions", ref processSubscriptionsPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { processSubscriptionsProp = propName; processSubscriptionsValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Write("DiscardUnsubscribedMessages", ref discardUnsubscribedMessagesPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { discardUnsubscribedMessagesProp = propName; discardUnsubscribedMessagesValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Write("NormalizeLineEndings", ref normalizeLineEndingsPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { normalizeLineEndingsProp = propName; normalizeLineEndingsValue = (bool)ptrVar; }));

			Expect.Call(() => propertyBag.Read("ProcessSubscriptions", out processSubscriptionsPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));
			Expect.Call(() => propertyBag.Read("DiscardUnsubscribedMessages", out discardUnsubscribedMessagesPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));
			Expect.Call(() => propertyBag.Read("NormalizeLineEndings", out normalizeLineEndingsPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));
			Expect.Call(() => propertyBag.Read(null, out dummy, 0)).IgnoreArguments().Repeat.Times(15);

			MockRepository.ReplayAll();

			component.ProcessSubscriptions = false;

			component.Save(propertyBag, true, true);

			Assert.AreEqual("ProcessSubscriptions", processSubscriptionsProp);

			Assert.AreEqual(false, processSubscriptionsValue);

			component.Load(propertyBag, 0);

			Assert.IsTrue(component.ProcessSubscriptions);

			MockRepository.VerifyAll();
		}

		/// <summary>
		/// Verify that properties set on component in designer are not reset when empty port 
		/// binding properties are loaded.
		/// </summary>
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void EdiDisassemblerExtension_DesignerPropertiesLoadTest()
		{
			string designerProperties =
@"<Properties>
	<ProcessSubscriptions vt=""11"">1</ProcessSubscriptions>
	<DiscardUnsubscribedMessages vt=""11"">1</DiscardUnsubscribedMessages>
</Properties>";

			string bindingProperties =
@"<Properties/>";

			var xmlRdr = XmlReader.Create(new MemoryStream(Encoding.UTF8.GetBytes(designerProperties)));
			xmlRdr.Read();
			IPropertyBag designerPropertyBag = new Winterdom.BizTalk.PipelineTesting.InstConfigPropertyBag(xmlRdr);

			var ediDisassembler = new EdiDissasemblerExtension();

			ediDisassembler.Load(designerPropertyBag, 0);

			Assert.IsTrue(ediDisassembler.ProcessSubscriptions);
			Assert.IsTrue(ediDisassembler.DiscardUnsubscribedMessages);

			xmlRdr = XmlReader.Create(new MemoryStream(Encoding.UTF8.GetBytes(bindingProperties)));
			xmlRdr.Read();
			IPropertyBag bindingsPropertyBag = new Winterdom.BizTalk.PipelineTesting.InstConfigPropertyBag(xmlRdr);

			ediDisassembler.Load(bindingsPropertyBag, 0);

			Assert.IsTrue(ediDisassembler.ProcessSubscriptions);
			Assert.IsTrue(ediDisassembler.DiscardUnsubscribedMessages);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void EdiDisassemblerExtension_AS2WithMultipleSubscriptions_ShouldEnqueueSubscribedMessages()
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream();
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.Write("MessageTrackingID", "http://cargowise.com/ehub/tracking/2010/06", "1079fa9d-ec9c-444e-988f-99afe9358e63");
			message.Context.Write("MessageType", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "Shipping Instruction");

			var mdn = MessageFactory.CreateMessage();
			mdn.Context.WriteProperty<EdiIntAS.IsAS2PayloadMessage>(false);

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			Expect.Call(pipelineContext.GetMessageFactory()).Return(MessageFactory).Repeat.Twice();

			var subscriptionAccessor = MockRepository.StrictMock<ISubscriptionAccessor>();
			Expect.Call(subscriptionAccessor.SelectSubscribedClients(message)).Return(new string[] { "SUBSCRIBER", "SUBSCRIBER2" });

			var mockContext = MockRepository.GenerateMock<eServices.eHubDataModel.eHubTransactions.eHubTransactionsContext>();
			var testClients = new TestDbSet<eServices.eHubDataModel.eHubTransactions.eHubClient>();
			mockContext.Stub(x => x.eHubClients).Return(testClients);
			mockContext.Stub(x => x.SqlQuery<DateTime>(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything)).Return(new[] { DateTime.UtcNow });

			var msgHelper = MockRepository.PartialMock<MessageHelper>();
			msgHelper.Expect(x => x.GetSubscriptionAccessor()).Return(subscriptionAccessor);
			msgHelper.Stub(x => x.GetDBContext()).Return(mockContext);

			var component = MockRepository.PartialMock<EdiDissasemblerExtension>();
			Expect.Call(() => component.BaseDisassemble(pipelineContext, message));
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(message);
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(mdn);
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(null);
			Expect.Call(component.GetMessageHelper()).Return(msgHelper).Repeat.Any();

			MockRepository.ReplayAll();

			component.ProcessSubscriptions = true;

			component.Disassemble(pipelineContext, message);

			var outmsg1 = component.GetNext(pipelineContext);
			Assert.AreEqual(message, outmsg1);
			Assert.AreEqual("SUBSCRIBER", outmsg1.Context.Read("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties"));
			var outmsg2 = component.GetNext(pipelineContext);
			Assert.IsNotNull(outmsg2);
			Assert.AreEqual("SUBSCRIBER2", outmsg2.Context.Read("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties"));

			subscriptionAccessor.AssertWasNotCalled(x => x.SelectSubscribedClients(mdn));
			Assert.AreSame(mdn, component.GetNext(pipelineContext));
		}
		
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void EdiDisassemblerExtension_AS2WithMultipleSubscriptionsAndIsSystemGeneratedEdiAck_ShouldEnqueueMessage()
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream();
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.Write("MessageTrackingID", "http://cargowise.com/ehub/tracking/2010/06", "1079fa9d-ec9c-444e-988f-99afe9358e63");
			message.Context.WriteProperty<EDI.IsSystemGeneratedAck>(true);

			var mdn = MessageFactory.CreateMessage();
			mdn.Context.WriteProperty<EdiIntAS.IsAS2PayloadMessage>(false);

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			Expect.Call(pipelineContext.GetMessageFactory()).Return(MessageFactory).Repeat.Twice();

			var subscriptionAccessor = MockRepository.StrictMock<ISubscriptionAccessor>();
			Expect.Call(subscriptionAccessor.SelectSubscribedClients(message)).Return(new string[] { "SUBSCRIBER", "SUBSCRIBER2" });


			var component = MockRepository.PartialMock<EdiDissasemblerExtension>();
			Expect.Call(() => component.BaseDisassemble(pipelineContext, message));
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(message);
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(mdn);
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(null);

			MockRepository.ReplayAll();

			component.ProcessSubscriptions = true;

			component.Disassemble(pipelineContext, message);

			var outmsg1 = component.GetNext(pipelineContext);
			subscriptionAccessor.AssertWasNotCalled(x => x.SelectSubscribedClients(message));
			Assert.AreEqual(message, outmsg1);

			subscriptionAccessor.AssertWasNotCalled(x => x.SelectSubscribedClients(mdn));
			Assert.AreSame(mdn, component.GetNext(pipelineContext));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void EdiDisassemblerExtension_AS2WithMultipleSubscriptionsAndHasEmptyMessageType_ShouldEnqueueMessage()
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream();
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.Write("MessageTrackingID", "http://cargowise.com/ehub/tracking/2010/06", "1079fa9d-ec9c-444e-988f-99afe9358e63");
			message.Context.Write("MessageType", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "");

			var mdn = MessageFactory.CreateMessage();
			mdn.Context.WriteProperty<EdiIntAS.IsAS2PayloadMessage>(false);

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			Expect.Call(pipelineContext.GetMessageFactory()).Return(MessageFactory).Repeat.Twice();

			var subscriptionAccessor = MockRepository.StrictMock<ISubscriptionAccessor>();
			Expect.Call(subscriptionAccessor.SelectSubscribedClients(message)).Return(new string[] { "SUBSCRIBER", "SUBSCRIBER2" });


			var component = MockRepository.PartialMock<EdiDissasemblerExtension>();
			Expect.Call(() => component.BaseDisassemble(pipelineContext, message));
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(message);
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(mdn);
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(null);

			MockRepository.ReplayAll();

			component.ProcessSubscriptions = true;

			component.Disassemble(pipelineContext, message);

			var outmsg1 = component.GetNext(pipelineContext);
			subscriptionAccessor.AssertWasNotCalled(x => x.SelectSubscribedClients(message));
			Assert.AreEqual(message, outmsg1);

			subscriptionAccessor.AssertWasNotCalled(x => x.SelectSubscribedClients(mdn));
			Assert.AreSame(mdn, component.GetNext(pipelineContext));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void EdiDisassemblerExtension_AS2WithMultipleSubscriptionsAndHasNullMessageType_ShouldEnqueueMessage()
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream();
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.Write("MessageTrackingID", "http://cargowise.com/ehub/tracking/2010/06", "1079fa9d-ec9c-444e-988f-99afe9358e63");
			message.Context.Write("MessageType", "http://schemas.microsoft.com/BizTalk/2003/system-properties", null);

			var mdn = MessageFactory.CreateMessage();
			mdn.Context.WriteProperty<EdiIntAS.IsAS2PayloadMessage>(false);

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			Expect.Call(pipelineContext.GetMessageFactory()).Return(MessageFactory).Repeat.Twice();

			var subscriptionAccessor = MockRepository.StrictMock<ISubscriptionAccessor>();
			Expect.Call(subscriptionAccessor.SelectSubscribedClients(message)).Return(new string[] { "SUBSCRIBER", "SUBSCRIBER2" });


			var component = MockRepository.PartialMock<EdiDissasemblerExtension>();
			Expect.Call(() => component.BaseDisassemble(pipelineContext, message));
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(message);
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(mdn);
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(null);

			MockRepository.ReplayAll();

			component.ProcessSubscriptions = true;

			component.Disassemble(pipelineContext, message);

			var outmsg1 = component.GetNext(pipelineContext);
			subscriptionAccessor.AssertWasNotCalled(x => x.SelectSubscribedClients(message));
			Assert.AreEqual(message, outmsg1);

			subscriptionAccessor.AssertWasNotCalled(x => x.SelectSubscribedClients(mdn));
			Assert.AreSame(mdn, component.GetNext(pipelineContext));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void EdiDisassemblerExtension_NormalizeLineEndings()
		{
			var expected = @"UNB+UNOC:1+YANGMING:ZZZ+CARGOWISE:ZZZ+190827:1227+17677'UNH+17677+CONTRL:99B:D:UN'UCI+42950+CARGOWISE:ZZZ+YANGMING:ZZZ+8'UCM+42950+IFTMIN:D:99B:UN:2.0+8'UNT+4+17677'UNZ+1+17677'";
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.EdiDisassembleExtensionTestMsg_TestNormalizeLineEndings.txt");
			message.Context = MessageFactory.CreateMessageContext();

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			Expect.Call(pipelineContext.GetMessageFactory()).Return(MessageFactory).Repeat.Twice();

			var msgHelper = MockRepository.StrictMock<IMessageHelper>();
			Expect.Call(delegate() { msgHelper.EnqueueMessage(null, null, null, null, false, false); }).IgnoreArguments()
				.Callback(new Func<IPipelineContext, Queue, IBaseMessage, IBaseMessage, bool, bool, bool>((p, q, m, u, c, d) => { q.Enqueue(m); return true; }))
				.Repeat.Any();

			var component = MockRepository.PartialMock<EdiDissasemblerExtension>();
			Expect.Call(() => component.BaseDisassemble(pipelineContext, message));
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(message);
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(null);
			Expect.Call(component.GetMessageHelper()).Return(msgHelper).Repeat.Any();

			MockRepository.ReplayAll();

			component.NormalizeLineEndings = true;

			component.Disassemble(pipelineContext, message);

			var outmsg1 = component.GetNext(pipelineContext);
			Assert.AreEqual(message, outmsg1);
			Assert.AreEqual(expected, new StreamReader(outmsg1.BodyPart.Data).ReadToEnd());
		}
	}
}

