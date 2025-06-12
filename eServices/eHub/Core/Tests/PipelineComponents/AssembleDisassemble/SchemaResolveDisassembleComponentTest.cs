using System;
using System.Collections;
using System.IO;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.Core.PropertySchemas;
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
	public class SchemaResolveDisassembleComponentTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestSchemaResolveAndDisassemble()
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.X12.txt");
			message.Context = MessageFactory.CreateMessageContext();

			var newMessage1 = MessageFactory.CreateMessage();
			newMessage1.AddPart("newMessage1", MessageFactory.CreateMessagePart(), true);
			newMessage1.BodyPart.Data = GetEmbeddedResource("TestFiles.RetrieveResponse_FSU.xml");
			newMessage1.Context = MessageFactory.CreateMessageContext();

			var newMessage2 = MessageFactory.CreateMessage();
			newMessage2.AddPart("newMessage2", MessageFactory.CreateMessagePart(), true);
			newMessage2.BodyPart.Data = new MemoryStream();

			var newMessage3 = MessageFactory.CreateMessage();
			newMessage3.AddPart("newMessage3", MessageFactory.CreateMessagePart(), true);
			newMessage3.BodyPart.Data = GetEmbeddedResource("TestFiles.Message_Invalid.xml");
			newMessage3.Context = MessageFactory.CreateMessageContext();
			newMessage3.Context.WriteProperty<FFSchemaType>("FFSchema, FFSchemaAssembly");

			var newMessage4 = MessageFactory.CreateMessage();
			newMessage4.AddPart("newMessage4", MessageFactory.CreateMessagePart(), true);
			newMessage4.BodyPart.Data = GetEmbeddedResource("TestFiles.Message.xml");
			newMessage4.Context = MessageFactory.CreateMessageContext();
			newMessage4.Context.WriteProperty<FFSchemaType>("FFSchema, FFSchemaAssembly");

			var newMessage5 = MessageFactory.CreateMessage();
			newMessage5.AddPart("newMessage5", MessageFactory.CreateMessagePart(), true);
			newMessage5.BodyPart.Data = new MemoryStream();

			var newMessage6 = MessageFactory.CreateMessage();
			newMessage6.AddPart("newMessage6", MessageFactory.CreateMessagePart(), true);
			newMessage6.BodyPart.Data = new MemoryStream();

			var newMessage7 = MessageFactory.CreateMessage();
			newMessage7.AddPart("newMessage7", MessageFactory.CreateMessagePart(), true);
			newMessage7.BodyPart.Data = new MemoryStream();

			var newMessage8 = MessageFactory.CreateMessage();
			newMessage8.AddPart("newMessage8", MessageFactory.CreateMessagePart(), true);
			newMessage8.BodyPart.Data = GetEmbeddedResource("TestFiles.Message_EncodedContent.xml");
			newMessage8.Context = MessageFactory.CreateMessageContext();
			newMessage8.Context.WriteProperty<FFSchemaType>("FFSchema, FFSchemaAssembly");

			var newMessage9 = MessageFactory.CreateMessage();

			var envelopeSchema = new Schema("EnvelopeSchema, EnvelopeSchemaAssembly");
			var messageSchema = new Schema("MessageSchema, MessageSchemaAssembly");
			var ffSchema = new Schema("FFSchema, FFSchemaAssembly");

			var docSpec = MockRepository.StrictMock<IDocumentSpec>();
			Expect.Call(docSpec.DocSpecStrongName).Return(ffSchema.ToString()).Repeat.Twice();

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			Expect.Call(pipelineContext.GetDocumentSpecByType("FFSchema, FFSchemaAssembly")).Return(docSpec).Repeat.Twice();
			SetupResult.For(pipelineContext.PipelineName).Return("pipelineName");

			var dasmXmlHelper = MockRepository.StrictMock<IXmlDisassembleHelper>();
			Expect.Call(dasmXmlHelper.Disassemble(pipelineContext, message, envelopeSchema, messageSchema))
				.Return(newMessage1);
			Expect.Call(dasmXmlHelper.Disassemble(pipelineContext, newMessage1)).IgnoreArguments()
				.Return(newMessage1);
			Expect.Call(dasmXmlHelper.Disassemble(null, null)).IgnoreArguments()
				.Return(newMessage2);
			Expect.Call(dasmXmlHelper.Disassemble(pipelineContext, newMessage5))
				.Return(newMessage6);
			Expect.Call(dasmXmlHelper.Disassemble(pipelineContext, message, envelopeSchema, new Schema(string.Empty)))
				.Return(newMessage3);
			Expect.Call(dasmXmlHelper.Disassemble(pipelineContext, newMessage3))
				.Return(newMessage3);
			Expect.Call(dasmXmlHelper.Disassemble(pipelineContext, newMessage4))
				.Return(newMessage4).Repeat.Twice();
			Expect.Call(dasmXmlHelper.Disassemble(pipelineContext, newMessage8))
				.Return(newMessage8);
			Expect.Call(dasmXmlHelper.Disassemble(pipelineContext, message, envelopeSchema, new Schema(string.Empty)))
				.Return(newMessage4).Repeat.Twice();
			Expect.Call(dasmXmlHelper.Disassemble(pipelineContext, message, envelopeSchema, new Schema(string.Empty)))
				.Return(newMessage8);
			Expect.Call(dasmXmlHelper.Disassemble(pipelineContext, message, new Schema(string.Empty), new Schema(string.Empty)))
				.Return(newMessage5);
			Expect.Call(dasmXmlHelper.GetNext(pipelineContext)).Return(null).Repeat.Times(3);

			var dasmFFHelper = MockRepository.StrictMock<IFFDisassembleHelper>();
			Expect.Call(dasmFFHelper.Disassemble(null, null, null)).IgnoreArguments()
				.Return(newMessage5);
			Expect.Call(dasmFFHelper.Disassemble(null, null, null)).IgnoreArguments()
				.Return(newMessage7);
			Expect.Call(dasmFFHelper.Disassemble(null, null, null)).IgnoreArguments()
				.Return(newMessage9);

			var msgHelper = MockRepository.StrictMock<IMessageHelper>();
			Expect.Call(delegate() { msgHelper.EnqueueMessage(null, null, null, null, false, false); }).IgnoreArguments()
				.Callback(new Func<IPipelineContext, Queue, IBaseMessage, IBaseMessage, bool, bool, bool>((p, q, m, u, c, d) => { q.Enqueue(m); return true; }))
				.Repeat.Any();

			var component = MockRepository.StrictMock<SchemaResolveDisassembleComponent>();
			Expect.Call(component.GetXmlDisassembler()).Return(dasmXmlHelper).Repeat.Any();
			Expect.Call(component.GetFlatFileDisassembler()).Return(dasmFFHelper).Repeat.Any();
			Expect.Call(component.GetMessageHelper()).Return(msgHelper).Repeat.Any();

			MockRepository.ReplayAll();

			component.DisassembleType = SchemaResolveDisassembleComponent.DasmType.XML;
			component.EnvelopeSchema = envelopeSchema;
			component.MessageSchema = new Schema(string.Empty);
			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "Envelope Schema specified without MessageSchema or FlatFile Data XPath");

			component.EnvelopeSchema = new Schema(string.Empty);
			component.MessageSchema = messageSchema;
			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "Envelope Schema must be specified");

			component.DisassembleType = SchemaResolveDisassembleComponent.DasmType.FlatFile;
			component.EnvelopeSchema = envelopeSchema;
			component.MessageSchema = new Schema(string.Empty);
			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "Envelope Schema not required for FlatFile disassemble");

			component.EnvelopeSchema = new Schema(string.Empty);
			component.MessageSchema = new Schema(string.Empty);
			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "Message Schema must be specified for FlatFile disassemble");

			component.EnvelopeSchema = envelopeSchema;
			component.MessageSchema = messageSchema;
			component.DisassembleType = SchemaResolveDisassembleComponent.DasmType.XML;
			component.Disassemble(pipelineContext, message);
			Assert.AreEqual(newMessage2, component.GetNext(pipelineContext));
			Assert.IsNull(component.GetNext(pipelineContext));

			component.MessageSchema = new Schema(string.Empty);
			component.FlatFileDataXPath = "/*[local-name()='Message']";
			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "FlatFile data not found at expected XPath.  Please review the document structure and/or pipeline configuration.");

			component.Disassemble(pipelineContext, message);
			Assert.AreEqual(newMessage5, component.GetNext(pipelineContext));
			Assert.IsNull(component.GetNext(pipelineContext));

			component.FlatFileDataBaseDecodeEnabled = true;
			newMessage4.BodyPart.Data = GetEmbeddedResource("TestFiles.Message.xml");
			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "Flat File content does not seem to be Base-64 encoded. Try set FlatFileDataBaseDecodeEnabled to false.");

			component.Disassemble(pipelineContext, message);
			Assert.AreEqual(newMessage7, component.GetNext(pipelineContext));
			Assert.IsNull(component.GetNext(pipelineContext));

			component.EnvelopeSchema = new Schema(string.Empty);
			component.MessageSchema = new Schema(string.Empty);
			component.Disassemble(pipelineContext, message);
			Assert.AreEqual(newMessage6, component.GetNext(pipelineContext));
			Assert.IsNull(component.GetNext(pipelineContext));

			component.DisassembleType = SchemaResolveDisassembleComponent.DasmType.FlatFile;
			component.MessageSchema = ffSchema;
			component.Disassemble(pipelineContext, message);
			Assert.AreEqual(newMessage9, component.GetNext(pipelineContext));
			Assert.IsNull(component.GetNext(pipelineContext));

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestSchemaResolveAndDisassemble_JSONProccessedLikeXML()
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.JSONMessage.json");
			message.Context = MessageFactory.CreateMessageContext();

			var newMessage1 = MessageFactory.CreateMessage();
			newMessage1.AddPart("newMessage1", MessageFactory.CreateMessagePart(), true);
			newMessage1.BodyPart.Data = GetEmbeddedResource("TestFiles.JSONMessage.xml");
			newMessage1.Context = MessageFactory.CreateMessageContext();

			var messageSchema = new Schema("MessageSchema, MessageSchemaAssembly");

			var pipelineContext = MockRepository.GenerateMock<IPipelineContext>();
			pipelineContext.Stub(x => x.PipelineName).Return("pipelineName");

			var dasmXmlHelper = MockRepository.GenerateMock<IXmlDisassembleHelper>();
			dasmXmlHelper.Expect(x => x.Disassemble(pipelineContext, message, new Schema(string.Empty), new Schema(string.Empty)))
				.Return(message);
			dasmXmlHelper.Expect(x => x.Disassemble(pipelineContext, message)).IgnoreArguments()
				.Return(message);

			var msgHelper = MockRepository.GenerateMock<IMessageHelper>();
			msgHelper.Stub(x => x.EnqueueMessage(null, null, null, null, false, false)).IgnoreArguments()
				.Callback(new Func<IPipelineContext, Queue, IBaseMessage, IBaseMessage, bool, bool, bool>((p, q, m, u, c, d) => { q.Enqueue(m); return true; }))
				.Repeat.Any();

			var component = MockRepository.GenerateStrictMock<SchemaResolveDisassembleComponent>();
			component.Stub(x => x.GetXmlDisassembler()).Return(dasmXmlHelper).Repeat.Any();
			component.Stub(x => x.GetMessageHelper()).Return(msgHelper).Repeat.Any();

			component.DisassembleType = SchemaResolveDisassembleComponent.DasmType.JSON;
			component.EnvelopeSchema = new Schema(string.Empty);
			component.MessageSchema = new Schema(string.Empty);
			component.Disassemble(pipelineContext, message);
			AssertXmlStream(GetEmbeddedResource("TestFiles.JSONMessage.xml"), component.GetNext(pipelineContext).BodyPart.Data);
			Assert.IsNull(component.GetNext(pipelineContext));

			message.BodyPart.Data = GetEmbeddedResource("TestFiles.JSONMessage.json");
			component.EnvelopeSchema = new Schema(string.Empty);
			component.MessageSchema = messageSchema;
			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "Envelope Schema must be specified");

			message.BodyPart.Data = GetEmbeddedResource("TestFiles.JSONMessage_Invalid.json");
			component.EnvelopeSchema = new Schema(string.Empty);
			component.MessageSchema = new Schema(string.Empty);
			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "The json has multiple objects in the top level of root, please wrap inside an object such as \"{json:\"+data+\"}\" or \"{json:{'@xmlns':'cargowise.com/json/1', body:\"+data+\"}}\". Addionally, you could use \"Biztalk Receive Pipeline's AppendData and PrependData of Stream Wrapper stage\" to wrap the json data without modify the incoming message.");

			message.BodyPart.Data = GetEmbeddedResource("TestFiles.JSONMessage_Invalid1.json");
			component.EnvelopeSchema = new Schema(string.Empty);
			component.MessageSchema = new Schema(string.Empty);
			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "The json has multiple objects in the top level of root, please wrap inside an object such as \"{json:\"+data+\"}\" or \"{json:{'@xmlns':'cargowise.com/json/1', body:\"+data+\"}}\". Addionally, you could use \"Biztalk Receive Pipeline's AppendData and PrependData of Stream Wrapper stage\" to wrap the json data without modify the incoming message.");

			dasmXmlHelper.VerifyAllExpectations();
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
		public void TestEnvelopeDasm_JSON()
		{
			var envMessage = MessageFactory.CreateMessage();
			envMessage.Context = MessageFactory.CreateMessageContext();
			envMessage.AddPart("json", MessageFactory.CreateMessagePart(), true);
			envMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.JSONMessage_Envelope.json");

			var bodyMessage = MessageFactory.CreateMessage();
			bodyMessage.Context = MessageFactory.CreateMessageContext();
			bodyMessage.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			bodyMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.Message_EnvelopeDasmBody.xml");

			var pipelineContext = new PipelineContext();

			var envSchema = new Schema("CargoWise.eHub.Gateway.Schemas.SendRequestStream, CargoWise.eHub.Gateway.Schemas, Version=3.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350");
			var dasmComp = MockRepository.GenerateMock<IDisassemblerComponent>();
			dasmComp.Expect(x => x.GetNext(pipelineContext)).Return(bodyMessage).Repeat.Once();
			dasmComp.Expect(x => x.GetNext(pipelineContext)).Throw(new XmlDasmException(-1061153688, string.Empty)).Repeat.Once();

			var dasmXmlHelper = MockRepository.GenerateMock<XmlDisassembleHelper>(dasmComp);
			dasmXmlHelper.Expect(x => x.Disassemble(pipelineContext, envMessage, envSchema, null))
				.Callback(new Func<IPipelineContext, IBaseMessage, Schema, Schema, bool>((p, m, e, b) => { dasmXmlHelper.GetNext(p); return true; }))
				.Return(bodyMessage).Repeat.Once();
			dasmXmlHelper.Expect(x => x.Disassemble(pipelineContext, bodyMessage))
				.Return(bodyMessage).Repeat.Twice();

			var msgHelper = MockRepository.GenerateMock<IMessageHelper>();
			msgHelper.Stub(x => x.EnqueueMessage(null, null, null, null, false, false)).IgnoreArguments()
				.Callback(new Func<IPipelineContext, Queue, IBaseMessage, IBaseMessage, bool, bool, bool>((p, q, m, u, c, d) => { q.Enqueue(m); return true; }))
				.Repeat.Any();

			var component = MockRepository.GenerateMock<SchemaResolveDisassembleComponent>();
			component.Stub(x => x.GetXmlDisassembler()).Return(dasmXmlHelper).Repeat.Any();
			component.Stub(x => x.GetMessageHelper()).Return(msgHelper).Repeat.Any();

			component.DisassembleType = SchemaResolveDisassembleComponent.DasmType.JSON;
			component.EnvelopeSchema = envSchema;
			component.FlatFileDataXPath = "/*[local-name()='Message']";
			component.Disassemble(pipelineContext, envMessage);

			var result = component.GetNext(pipelineContext);
			Assert.IsNotNull(result);
			Assert.IsNull(component.GetNext(pipelineContext));

			dasmXmlHelper.VerifyAllExpectations();
			msgHelper.VerifyAllExpectations();
			dasmComp.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_SchemaResolveDisassembleComponent()
		{
			var component = new SchemaResolveDisassembleComponent();
			Assert.AreEqual(string.Empty, component.Description);
			Assert.AreEqual("Inbound Message Disassembler", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
			var guid = new Guid();
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("9D7A0A1B-F30D-4AA3-975D-3D122C74B8CB"), guid);

			var propertyBag = MockRepository.StrictMock<IPropertyBag>();
			string messageSchemaProp = null;
			string envelopeSchemaProp = null;
			string disassembleTypeProp = null;
			string flatFileDataXPathProp = null;
			string AS2EnabledProp = null;
			string flatFileDataBaseDecodeEnabledProp = null;
			string XPathDebatchEnabledProp = null;
			string xPathToDebatchProp = null;
			string processSubscriptionsProp = null;
			string flatFileLineSplitEnabledProp = null;
			string flatFileLineSplitPreserveHeadlineEnabledProp = null;
			string flatFileLineSplitNumberProp = null;
			string discardUnsubscribedMessagesProp = null;


			string messageSchemaValue = string.Empty;
			string envelopeSchemaValue = string.Empty;
			string disassembleTypeValue = string.Empty;
			string flatFileDataXPathValue = string.Empty;
			bool AS2EnabledValue = false;
			bool flatFileDataBaseDecodeEnabledValue = false;
			bool XPathDebatchEnabledValue = false;
			string xPathToDebatchValue = string.Empty;
			bool processSubscriptionsValue = false;
			bool flatFileLineSplitEnabledValue = false;
			bool flatFileLineSplitPreserveHeadlineEnabledValue = false;
			int flatFileLineSplitNumberValue = 0;
			bool discardUnsubscribedMessagesValue = false;


			object messageSchemaPtr = "Mess, Schema";
			object envelopeSchemaPtr = "Env, Schema";
			object disassembleTypePtr = "XML";
			object flatFileDataXPathPtr = "XPath";
			object AS2EnabledPtr = false;
			object flatFileDataBaseDecodeEnabledPtr = false;
			object XPathDebatchEnabledPtr = false;
			object xPathToDebatchPtr = "xPathToDebatch";
			object processSubscriptionsPtr = false;
			object flatFileLineSplitEnabledPtr = false;
			object flatFileLineSplitPreserveHeadlineEnabledPtr = false;
			object flatFileLineSplitNumberPtr = 0;
			object discardUnsubscribedMessagesPtr = false;


			Expect.Call(() => propertyBag.Write("MessageSchema", ref messageSchemaPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { messageSchemaProp = propName; messageSchemaValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("EnvelopeSchema", ref envelopeSchemaPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { envelopeSchemaProp = propName; envelopeSchemaValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("DisassembleType", ref disassembleTypePtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { disassembleTypeProp = propName; disassembleTypeValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("FlatFileDataXPath", ref flatFileDataXPathPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { flatFileDataXPathProp = propName; flatFileDataXPathValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("AS2Enabled", ref AS2EnabledPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { AS2EnabledProp = propName; AS2EnabledValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Write("FlatFileDataBaseDecodeEnabled", ref flatFileDataBaseDecodeEnabledPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { flatFileDataBaseDecodeEnabledProp = propName; flatFileDataBaseDecodeEnabledValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Write("XPathDebatchEnabled", ref XPathDebatchEnabledPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { XPathDebatchEnabledProp = propName; XPathDebatchEnabledValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Write("XPathToDebatch", ref xPathToDebatchPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { xPathToDebatchProp = propName; xPathToDebatchValue = (string)ptrVar; }));
			Expect.Call(() => propertyBag.Write("ProcessSubscriptions", ref processSubscriptionsPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { processSubscriptionsProp = propName; processSubscriptionsValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Write("FlatFileLineSplitEnabled", ref flatFileLineSplitEnabledPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { flatFileLineSplitEnabledProp = propName; flatFileLineSplitEnabledValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Write("FlatFileLineSplitPreserveHeadlineEnabled", ref flatFileLineSplitPreserveHeadlineEnabledPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { flatFileLineSplitPreserveHeadlineEnabledProp = propName; flatFileLineSplitPreserveHeadlineEnabledValue = (bool)ptrVar; }));
			Expect.Call(() => propertyBag.Write("FlatFileLineSplitNumber", ref flatFileLineSplitNumberPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { flatFileLineSplitNumberProp = propName; flatFileLineSplitNumberValue = (int)ptrVar; }));
			Expect.Call(() => propertyBag.Write("DiscardUnsubscribedMessages", ref discardUnsubscribedMessagesPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { discardUnsubscribedMessagesProp = propName; discardUnsubscribedMessagesValue = (bool)ptrVar; }));





			Expect.Call(() => propertyBag.Read("MessageSchema", out messageSchemaPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "Mess, Schema different"; }));
			Expect.Call(() => propertyBag.Read("EnvelopeSchema", out messageSchemaPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "Env, Schema different"; }));
			Expect.Call(() => propertyBag.Read("DisassembleType", out messageSchemaPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "FlatFile"; }));
			Expect.Call(() => propertyBag.Read("FlatFileDataXPath", out messageSchemaPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "XPath_different"; }));
			Expect.Call(() => propertyBag.Read("AS2Enabled", out messageSchemaPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));
			Expect.Call(() => propertyBag.Read("FlatFileDataBaseDecodeEnabled", out messageSchemaPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));
			Expect.Call(() => propertyBag.Read("XPathDebatchEnabled", out messageSchemaPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));
			Expect.Call(() => propertyBag.Read("XPathToDebatch", out messageSchemaPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = "xPathToDebatch_different"; }));
			Expect.Call(() => propertyBag.Read("ProcessSubscriptions", out processSubscriptionsPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));
			Expect.Call(() => propertyBag.Read("FlatFileLineSplitEnabled", out flatFileLineSplitEnabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));
			Expect.Call(() => propertyBag.Read("FlatFileLineSplitPreserveHeadlineEnabled", out flatFileLineSplitPreserveHeadlineEnabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));
			Expect.Call(() => propertyBag.Read("FlatFileLineSplitNumber", out flatFileLineSplitNumberPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = 100; }));
			Expect.Call(() => propertyBag.Read("DiscardUnsubscribedMessages", out discardUnsubscribedMessagesPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));




			MockRepository.ReplayAll();

			component.MessageSchema = new Schema("Mess, Schema");
			component.EnvelopeSchema = new Schema("Env, Schema");
			component.DisassembleType = SchemaResolveDisassembleComponent.DasmType.XML;
			component.FlatFileDataXPath = "XPath";
			component.AS2Enabled = false;
			component.FlatFileDataBaseDecodeEnabled = false;
			component.XPathDebatchEnabled = false;
			component.XPathToDebatch = "xPathToDebatch";
			component.ProcessSubscriptions = false;
			component.FlatFileLineSplitEnabled = false;
			component.FlatFileLineSplitPreserveHeadlineEnabled = false;
			component.FlatFileLineSplitNumber =0;
			component.DiscardUnsubscribedMessages = false;

			component.Save(propertyBag, true, true);

			Assert.AreEqual("MessageSchema", messageSchemaProp);
			Assert.AreEqual("EnvelopeSchema", envelopeSchemaProp);
			Assert.AreEqual("DisassembleType", disassembleTypeProp);
			Assert.AreEqual("FlatFileDataXPath", flatFileDataXPathProp);
			Assert.AreEqual("AS2Enabled", AS2EnabledProp);
			Assert.AreEqual("FlatFileDataBaseDecodeEnabled", flatFileDataBaseDecodeEnabledProp);
			Assert.AreEqual("XPathDebatchEnabled", XPathDebatchEnabledProp);
			Assert.AreEqual("XPathToDebatch", xPathToDebatchProp);
			Assert.AreEqual("ProcessSubscriptions", processSubscriptionsProp);
			Assert.AreEqual("DiscardUnsubscribedMessages", discardUnsubscribedMessagesProp);

			Assert.AreEqual("Mess, Schema", messageSchemaValue);
			Assert.AreEqual("Env, Schema", envelopeSchemaValue);
			Assert.AreEqual("XML", disassembleTypeValue);
			Assert.AreEqual("XPath", flatFileDataXPathValue);
			Assert.IsFalse(AS2EnabledValue);
			Assert.IsFalse(flatFileDataBaseDecodeEnabledValue);
			Assert.IsFalse(XPathDebatchEnabledValue);
			Assert.AreEqual("xPathToDebatch", xPathToDebatchValue);
			Assert.IsFalse(processSubscriptionsValue);
			Assert.IsFalse(flatFileLineSplitEnabledValue);
			Assert.IsFalse(flatFileLineSplitPreserveHeadlineEnabledValue);
			Assert.AreEqual(0, flatFileLineSplitNumberValue);
			Assert.IsFalse(discardUnsubscribedMessagesValue);

			component.Load(propertyBag, 0);

			Assert.AreEqual(new Schema("Mess, Schema different"), component.MessageSchema);
			Assert.AreEqual(new Schema("Env, Schema different"), component.EnvelopeSchema);
			Assert.AreEqual(SchemaResolveDisassembleComponent.DasmType.FlatFile, component.DisassembleType);
			Assert.AreEqual("XPath_different", component.FlatFileDataXPath);
			Assert.IsTrue(component.AS2Enabled);
			Assert.IsTrue(component.FlatFileDataBaseDecodeEnabled);
			Assert.IsTrue(component.XPathDebatchEnabled);
			Assert.AreEqual("xPathToDebatch_different", component.XPathToDebatch);
			Assert.IsTrue(component.ProcessSubscriptions);
			Assert.IsTrue(component.FlatFileLineSplitEnabled);
			Assert.IsTrue(component.FlatFileLineSplitPreserveHeadlineEnabled);
			Assert.AreEqual(100,component.FlatFileLineSplitNumber);
			Assert.IsTrue(component.DiscardUnsubscribedMessages);

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestFlatFileLineSplit()
		{
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("Body", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.FlatFileLineSplitTest.csv");
			message.Context.Write("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "FlatFileLineSplitTest.csv");

			var pipelineContext = new PipelineContext();

			var messageSchema = new SchemaWithNone("MessageSchema, MessageSchemaAssembly");

			var dasmFFHelper = MockRepository.StrictMock<IFFDisassembleHelper>();
			Func<IPipelineContext, IBaseMessage, SchemaWithNone, IBaseMessage> MockDisassemble = (x, y, z) => y;
			Expect.Call(dasmFFHelper.Disassemble(null, null, null)).IgnoreArguments().Repeat.Any().Do(MockDisassemble);

			var msgHelper = MockRepository.StrictMock<IMessageHelper>();
			Expect.Call(delegate() { msgHelper.EnqueueMessage(null, null, null, null, false, false); }).IgnoreArguments()
				.Callback(new Func<IPipelineContext, Queue, IBaseMessage, IBaseMessage, bool, bool, bool>((p, q, m, u, c, d) => { q.Enqueue(m); return true; }))
				.Repeat.Any();

			var component = MockRepository.PartialMock<SchemaResolveDisassembleComponent>();
			Expect.Call(component.GetFlatFileDisassembler()).Return(dasmFFHelper).Repeat.Any();
			Expect.Call(component.GetMessageHelper()).Return(msgHelper).Repeat.Any();

			MockRepository.ReplayAll();

			component.MessageSchema = messageSchema;
			component.DisassembleType = SchemaResolveDisassembleComponent.DasmType.FlatFile;
			component.FlatFileLineSplitEnabled = true;
			component.FlatFileLineSplitNumber = 5;
			component.FlatFileLineSplitPreserveHeadlineEnabled = true;

			component.Disassemble(pipelineContext, message);

			var result = component.GetNext(pipelineContext);
			Assert.IsNotNull(result);
			Assert.AreEqual("FlatFileLineSplitTest_1.csv", result.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06") as string);
			Assert.AreEqual(
				new StreamReader(GetEmbeddedResource("TestFiles.FlatFileLineSplitTestResult_1.csv")).ReadToEnd(),
				new StreamReader(result.BodyPart.Data).ReadToEnd()
			);
			Assert.AreEqual(GetEmbeddedResource("TestFiles.FlatFileLineSplitTestResult_1.csv").Length, result.BodyPart.Data.Length);
			result = component.GetNext(pipelineContext);
			Assert.IsNotNull(result);
			Assert.AreEqual("FlatFileLineSplitTest_2.csv", result.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06") as string);
			Assert.AreEqual(
				new StreamReader(GetEmbeddedResource("TestFiles.FlatFileLineSplitTestResult_2.csv")).ReadToEnd(),
				new StreamReader(result.BodyPart.Data).ReadToEnd()
			);
			Assert.AreEqual(GetEmbeddedResource("TestFiles.FlatFileLineSplitTestResult_2.csv").Length, result.BodyPart.Data.Length);
			result = component.GetNext(pipelineContext);
			Assert.IsNotNull(result);
			Assert.AreEqual("FlatFileLineSplitTest_3.csv", result.Context.Read("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06") as string);
			Assert.AreEqual(
				new StreamReader(GetEmbeddedResource("TestFiles.FlatFileLineSplitTestResult_3.csv")).ReadToEnd(),
				new StreamReader(result.BodyPart.Data).ReadToEnd()
			);
			Assert.AreEqual(GetEmbeddedResource("TestFiles.FlatFileLineSplitTestResult_3.csv").Length, result.BodyPart.Data.Length);
			result = component.GetNext(pipelineContext);
			Assert.IsNull(result);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestMessageContentSizeLimit()
		{
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("Body", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.Message_EncodedLargeSize.xml");
			message.Context.Write("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "Message_EncodedLargeSize.xml");
			message.Context.WriteProperty<FFSchemaType>("FFSchema, FFSchemaAssembly");

			var pipelineContext = new PipelineContext();

			var envelopeSchema = new Schema("EnvelopeSchema, EnvelopeSchemaAssembly");
			var messageSchema = new SchemaWithNone("MessageSchema, MessageSchemaAssembly");

			var dasmXmlHelper = MockRepository.StrictMock<IXmlDisassembleHelper>();
			Expect.Call(dasmXmlHelper.Disassemble(pipelineContext, message, envelopeSchema, messageSchema)).Return(message);
			Expect.Call(dasmXmlHelper.Disassemble(pipelineContext, message)).Return(message);

			var msgHelper = MockRepository.StrictMock<IMessageHelper>();
			Expect.Call(delegate () { msgHelper.EnqueueMessage(null, null, null, null, false, false); }).IgnoreArguments()
				.Callback(new Func<IPipelineContext, Queue, IBaseMessage, IBaseMessage, bool, bool, bool>((p, q, m, u, c, d) => { q.Enqueue(m); return true; }))
				.Repeat.Any();

			var component = MockRepository.PartialMock<SchemaResolveDisassembleComponent>();
			Expect.Call(component.GetXmlDisassembler()).Return(dasmXmlHelper).Repeat.Any();
			Expect.Call(component.GetMessageHelper()).Return(msgHelper).Repeat.Any();

			MockRepository.ReplayAll();

			component.MessageSchema = messageSchema;
			component.EnvelopeSchema = envelopeSchema;
			component.FlatFileDataBaseDecodeEnabled = true;
			component.FlatFileDataXPath = "/*[local-name()='Message']";

			try
			{
				component.Disassemble(pipelineContext, message);
				Assert.Fail("Should throw an exception");
			}
			catch (ApplicationException ex)
			{
				Assert.IsTrue(ex.Message.Contains(@"File content size is greater than the size limit"));
			}

		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestSubscriptionProcessing()
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream();
			message.Context = MessageFactory.CreateMessageContext();

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			Expect.Call(pipelineContext.GetMessageFactory()).Return(MessageFactory).Repeat.Twice();

			var dasmXmlHelper = MockRepository.StrictMock<IXmlDisassembleHelper>();
			Expect.Call(dasmXmlHelper.Disassemble(pipelineContext, message, new Schema(string.Empty), new Schema(string.Empty))).Return(message);
			Expect.Call(dasmXmlHelper.Disassemble(pipelineContext, message)).Return(message);

			var subscriptionAccessor = MockRepository.StrictMock<ISubscriptionAccessor>();
			Expect.Call(subscriptionAccessor.SelectSubscribedClients(message)).Return(new[] { "Subscriber1", "Subscriber2" });

			var mockContext = MockRepository.GenerateMock<eServices.eHubDataModel.eHubTransactions.eHubTransactionsContext>();
			var testClients = new TestDbSet<eServices.eHubDataModel.eHubTransactions.eHubClient>();
			mockContext.Stub(x => x.eHubClients).Return(testClients);
			mockContext.Stub(x => x.SqlQuery<DateTime>(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything)).Return(new[] { DateTime.UtcNow });

			var msgHelper = MockRepository.PartialMock<MessageHelper>();
			Expect.Call(msgHelper.GetSubscriptionAccessor()).Return(subscriptionAccessor);
			msgHelper.Stub(x => x.GetDBContext()).Return(mockContext);

			var component = MockRepository.StrictMock<SchemaResolveDisassembleComponent>();
			Expect.Call(component.GetXmlDisassembler()).Return(dasmXmlHelper).Repeat.Twice();
			Expect.Call(component.GetMessageHelper()).Return(msgHelper);

			MockRepository.ReplayAll();

			component.DisassembleType = SchemaResolveDisassembleComponent.DasmType.XML;
			component.EnvelopeSchema = new Schema(string.Empty);
			component.MessageSchema = new Schema(string.Empty);
			component.ProcessSubscriptions = true;
			component.Disassemble(pipelineContext, message);

			var outmsg1 = component.GetNext(pipelineContext);
			Assert.AreEqual(message, outmsg1);
			Assert.AreEqual("Subscriber1", outmsg1.Context.Read("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties"));
			var outmsg2 = component.GetNext(pipelineContext);
			Assert.IsNotNull(outmsg2);
			Assert.AreEqual("Subscriber2", outmsg2.Context.Read("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties"));
			Assert.IsNull(component.GetNext(pipelineContext));

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SchemaResolveDisassembleComponent_DiscardUnsubscribed()
		{
			string trackingID = "3dbf8fd9-033b-4026-b269-405634993a40";

			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream();
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.Write("MessageTrackingID", "http://cargowise.com/ehub/tracking/2010/06", trackingID);

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			Expect.Call(pipelineContext.GetMessageFactory()).Return(MessageFactory).Repeat.Twice();

			var dasmXmlHelper = MockRepository.StrictMock<IXmlDisassembleHelper>();
			Expect.Call(dasmXmlHelper.Disassemble(pipelineContext, message, new Schema(string.Empty), new Schema(string.Empty))).Return(message);
			Expect.Call(dasmXmlHelper.Disassemble(pipelineContext, message)).Return(message);

			var subscriptionAccessor = MockRepository.StrictMock<ISubscriptionAccessor>();
			Expect.Call(subscriptionAccessor.SelectSubscribedClients(message)).Return(new string[] { });

			var msgHelper = MockRepository.PartialMock<MessageHelper>();
			Expect.Call(msgHelper.GetSubscriptionAccessor()).Return(subscriptionAccessor);

			var outboxAccessor = MockRepository.StrictMock<IOutboxAccessor>();
			Expect.Call(() => outboxAccessor.UpdateInboxMessageDistributionStatus(trackingID));

			var component = MockRepository.StrictMock<SchemaResolveDisassembleComponent>();
			Expect.Call(component.GetXmlDisassembler()).Return(dasmXmlHelper).Repeat.Twice();
			Expect.Call(component.GetMessageHelper()).Return(msgHelper);
			Expect.Call(component.GetOutboxAccessor()).Return(outboxAccessor);

			MockRepository.ReplayAll();

			component.DisassembleType = SchemaResolveDisassembleComponent.DasmType.XML;
			component.EnvelopeSchema = new Schema(string.Empty);
			component.MessageSchema = new Schema(string.Empty);
			component.ProcessSubscriptions = true;
			component.DiscardUnsubscribedMessages = true;

			component.Disassemble(pipelineContext, message);

			Assert.IsNull(component.GetNext(pipelineContext));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void SchemaResolveDisassembleComponent_AS2WithSubscription()
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream();
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.Write("MessageTrackingID", "http://cargowise.com/ehub/tracking/2010/06", "1079fa9d-ec9c-444e-988f-99afe9358e63");
			var mdn = MessageFactory.CreateMessage();

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			Expect.Call(pipelineContext.GetMessageFactory()).Return(MessageFactory).Repeat.Twice();

			var dasmXmlHelper = MockRepository.StrictMock<IXmlDisassembleHelper>();
			Expect.Call(dasmXmlHelper.Disassemble(pipelineContext, message, null, null)).Return(message);
			Expect.Call(dasmXmlHelper.Disassemble(pipelineContext, message)).Return(message);

			var subscriptionAccessor = MockRepository.StrictMock<ISubscriptionAccessor>();
			Expect.Call(subscriptionAccessor.SelectSubscribedClients(message)).Return(new string[] { "SUBSCRIBER" });

			var mockContext = MockRepository.GenerateMock<eServices.eHubDataModel.eHubTransactions.eHubTransactionsContext>();
			var testClients = new TestDbSet<eServices.eHubDataModel.eHubTransactions.eHubClient>();
			mockContext.Stub(x => x.eHubClients).Return(testClients);
			mockContext.Stub(x => x.SqlQuery<DateTime>(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything)).Return(new[] { DateTime.UtcNow });

			var msgHelper = MockRepository.PartialMock<MessageHelper>();
			Expect.Call(msgHelper.GetSubscriptionAccessor()).Return(subscriptionAccessor);
			msgHelper.Stub(x => x.GetDBContext()).Return(mockContext);

			var as2Dasm = MockRepository.StrictMock<IAS2DisassembleHelper>();
			Expect.Call(as2Dasm.Disassemble(pipelineContext, message)).Return(mdn);

			var component = MockRepository.StrictMock<SchemaResolveDisassembleComponent>();
			Expect.Call(component.GetXmlDisassembler()).Return(dasmXmlHelper).Repeat.Twice();
			Expect.Call(component.GetMessageHelper()).Return(msgHelper);
			Expect.Call(component.GetAS2DasmHelper()).Return(as2Dasm);

			MockRepository.ReplayAll();

			component.DisassembleType = SchemaResolveDisassembleComponent.DasmType.XML;
			component.AS2Enabled = true;
			component.ProcessSubscriptions = true;

			component.Disassemble(pipelineContext, message);

			subscriptionAccessor.AssertWasNotCalled(x => x.SelectSubscribedClients(mdn));
			Assert.AreSame(mdn, component.GetNext(null));
			Assert.AreSame(message, component.GetNext(null));
		}
	}
}

