using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using PipelineTesting = Winterdom.BizTalk.PipelineTesting;

namespace CargoWise.eHub.Core.Tests.PipelineComponents
{
	[TestClass]
	public class MessageAssembleComponentTest : BaseComponentTest
	{
		delegate IBaseMessage AssembleDelegate(IPipelineContext piplineContext, IBaseMessage message);

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAssembleMessage()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.SimpleXMLWithNamespace.xml");
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.PromoteProperty<BTS.SourceParty>("SenderID");
			message.Context.PromoteProperty<BTS.DestinationParty>("RecipientID");
			message.Context.PromoteProperty<EnvelopeTrackingID>("111111111");
			message.Context.PromoteProperty<MessageTrackingID>("222222222");
			message.Context.PromoteProperty<InternalTrackingID>("333333333");
			message.Context.PromoteProperty<TransformSetID>("5D8568D5-C391-4278-8C32-68F490E7D305");

			var messageFF = MessageFactory.CreateMessage();
			messageFF.AddPart("native", MessageFactory.CreateMessagePart(), true);
			messageFF.BodyPart.Data = GetEmbeddedResource("TestFiles.FSUAssembleResult.txt");
			messageFF.Context = PipelineUtil.CloneMessageContext(message.Context);

			var accessor = MockRepository.StrictMock<ITransformAccessor>();
			string charset;
			Expect.Call(accessor.IsFlatFile("http://cargowise.com/ehub/clients/edi/2010/06#FSU", out charset)).Return(true);
			Expect.Call(accessor.IsFlatFile("http://cargowise.com/ehub/clients/edi/2010/06#FSU", out charset)).Return(false);
			Expect.Call(accessor.IsEDI("http://cargowise.com/ehub/clients/edi/2010/06#FSU")).Return(false);
            Expect.Call(accessor.IsJson("http://cargowise.com/ehub/clients/edi/2010/06#FSU")).Return(false);
			string postAssembleWrapper = null;
			Expect.Call(accessor.IsPostAssembleMapping("http://cargowise.com/ehub/clients/edi/2010/06#FSU", out postAssembleWrapper)).Return(false).Repeat.Twice();

			var assembler = MockRepository.StrictMock<IFFAssembleHelper>();
			Expect.Call(assembler.Assemble(pipelineContext, message)).Return(messageFF);

			var msgHelper = MockRepository.StrictMock<IMessageHelper>();
			Expect.Call(msgHelper.IsAutoSubscriptionRequired(message)).Return(false);
			Expect.Call(msgHelper.IsAutoSubscriptionRequired(message)).Return(true);
			IBaseMessage subsMsg = null;
			Expect.Call(() => msgHelper.InsertAutoSubscriptionsForSender(null)).IgnoreArguments().Do(new Action<IBaseMessage>((m) => subsMsg = m));

			var component = MockRepository.PartialMock<MessageAssembleComponent>();
			Expect.Call(component.GetTransformationAccessor()).Return(accessor).Repeat.Twice();
			Expect.Call(component.GetFFAssembleHelper()).Return(assembler);
			Expect.Call(component.GetNewGuid()).Return(new Guid("b2a38b9d-8d5b-42d3-a7bf-a725da25931c")).Repeat.Twice();
			Expect.Call(component.GetCurrentUTCString()).Return("2001-01-20 13:14:15.503").Repeat.Twice();
			Expect.Call(component.GetMessageHelper()).Return(msgHelper).Repeat.Any();

			MockRepository.ReplayAll();

			component.Execute(pipelineContext, message);

			component.Enabled = true;
			message.Context.PromoteProperty<BTS.MessageType>("http://cargowise.com/ehub/clients/edi/2010/06#FSU");
			var result = component.Execute(pipelineContext, message);
			AssertXmlStream(GetEmbeddedResource("TestFiles.OutboxInsertFF.xml"), result.BodyPart.Data);

			message.BodyPart.Data.Position = 0;
			result = component.Execute(pipelineContext, message);
			AssertXmlStream(GetEmbeddedResource("TestFiles.OutboxInsertXML.xml"), result.BodyPart.Data);
			AssertXmlStream(GetEmbeddedResource("TestFiles.SimpleXMLWithNamespace.xml"), subsMsg.BodyPart.Data);
			Assert.AreEqual(result.Context.CountProperties, subsMsg.Context.CountProperties);
			CollectionAssert.AreEquivalent(GetProperties(result), GetProperties(subsMsg));

			MockRepository.VerifyAll();
		}

        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestAssembleMessage_JSON()
        {
            var pipelineContext = new PipelineContext();
            var message = MessageFactory.CreateMessage();
            message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
            message.BodyPart.Data = GetEmbeddedResource("TestFiles.JSONMessage.xml");
            message.Context = MessageFactory.CreateMessageContext();
            message.Context.PromoteProperty<BTS.SourceParty>("SenderID");
            message.Context.PromoteProperty<BTS.DestinationParty>("RecipientID");
            message.Context.PromoteProperty<EnvelopeTrackingID>("111111111");
            message.Context.PromoteProperty<MessageTrackingID>("222222222");
            message.Context.PromoteProperty<InternalTrackingID>("333333333");

            var accessor = MockRepository.GenerateMock<ITransformAccessor>();
            string charset;
            accessor.Stub(x => x.IsFlatFile("http://wisetechgloba.com/2016/json/root", out charset)).Return(false);
            accessor.Stub(x => x.IsEDI("http://wisetechgloba.com/2016/json/root")).Return(false);
            accessor.Stub(x => x.IsJson("http://wisetechgloba.com/2016/json/root")).Return(true);
            string postAssembleWrapper = null;
            accessor.Stub(x => x.IsPostAssembleMapping("http://wisetechgloba.com/2016/json/root", out postAssembleWrapper)).Return(false);

            var msgHelper = MockRepository.GenerateMock<IMessageHelper>();
            msgHelper.Stub(x => x.IsAutoSubscriptionRequired(message)).Return(false);

            var component = MockRepository.GeneratePartialMock<MessageAssembleComponent>();
            component.Stub(x => x.GetTransformationAccessor()).Return(accessor);
            component.Stub(x => x.GetNewGuid()).Return(new Guid("b2a38b9d-8d5b-42d3-a7bf-a725da25931c"));
            component.Stub(x => x.GetCurrentUTCString()).Return("2001-01-20 13:14:15.503");
            component.Stub(x => x.GetMessageHelper()).Return(msgHelper);

            component.Enabled = true;
            message.Context.PromoteProperty<BTS.MessageType>("http://wisetechgloba.com/2016/json/root");
            var result = component.Execute(pipelineContext, message);
            AssertXmlStream(GetEmbeddedResource("TestFiles.JSONMessageExecutedAssembler.xml"), result.BodyPart.Data);
        }

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAssembleMessage_JSON_RemovedAllNamespaces()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.JSONMessage_MultipleNamespaces.xml");
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.PromoteProperty<BTS.SourceParty>("SenderID");
			message.Context.PromoteProperty<BTS.DestinationParty>("RecipientID");
			message.Context.PromoteProperty<EnvelopeTrackingID>("111111111");
			message.Context.PromoteProperty<MessageTrackingID>("222222222");
			message.Context.PromoteProperty<InternalTrackingID>("333333333");

			var accessor = MockRepository.GenerateMock<ITransformAccessor>();
			string charset;
			accessor.Stub(x => x.IsFlatFile("http://wisetechgloba.com/2016/json/root", out charset)).Return(false);
			accessor.Stub(x => x.IsEDI("http://wisetechgloba.com/2016/json/root")).Return(false);
			accessor.Stub(x => x.IsJson("http://wisetechgloba.com/2016/json/root")).Return(true);
			string postAssembleWrapper = null;
			accessor.Stub(x => x.IsPostAssembleMapping("http://wisetechgloba.com/2016/json/root", out postAssembleWrapper)).Return(false);

			var msgHelper = MockRepository.GenerateMock<IMessageHelper>();
			msgHelper.Stub(x => x.IsAutoSubscriptionRequired(message)).Return(false);

			var component = MockRepository.GeneratePartialMock<MessageAssembleComponent>();
			component.Stub(x => x.GetTransformationAccessor()).Return(accessor);
			component.Stub(x => x.GetNewGuid()).Return(new Guid("b2a38b9d-8d5b-42d3-a7bf-a725da25931c"));
			component.Stub(x => x.GetCurrentUTCString()).Return("2001-01-20 13:14:15.503");
			component.Stub(x => x.GetMessageHelper()).Return(msgHelper);

			component.Enabled = true;
			message.Context.PromoteProperty<BTS.MessageType>("http://wisetechgloba.com/2016/json/root");
			var result = component.Execute(pipelineContext, message);
			var expected = GetEmbeddedResource("TestFiles.JSONMessage_RemovedMultipleNamespaces.json").ReadToEnd();
			var encodedJSON = XElement.Load(result.BodyPart.Data).Element("Content").Value.Replace("<![CDATA[", "").Replace("]]", "");
			var actual = new MemoryStream(Encoding.UTF8.GetBytes(encodedJSON)).DecodeAndDecompress().ReadToEnd();
			Assert.AreEqual(expected, actual);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAssembleMessageWithCDATA()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.MessageWithCDATA.xml");
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.PromoteProperty<BTS.SourceParty>("SenderID");
			message.Context.PromoteProperty<BTS.DestinationParty>("RecipientID");
			message.Context.PromoteProperty<EnvelopeTrackingID>("111111111");
			message.Context.PromoteProperty<MessageTrackingID>("222222222");
			message.Context.PromoteProperty<InternalTrackingID>("333333333");

			var accessor = MockRepository.StrictMock<ITransformAccessor>();

			var msgHelper = MockRepository.StrictMock<IMessageHelper>();
			Expect.Call(msgHelper.IsAutoSubscriptionRequired(message)).Return(false);

			var component = MockRepository.PartialMock<MessageAssembleComponent>();
			component.Enabled = true;

			Expect.Call(component.GetNewGuid()).Return(new Guid("b2a38b9d-8d5b-42d3-a7bf-a725da25931c"));
			Expect.Call(component.GetCurrentUTCString()).Return("2001-01-20 13:14:15.503");
			Expect.Call(component.GetMessageHelper()).Return(msgHelper).Repeat.Any();

			MockRepository.ReplayAll();

			var result = component.Execute(pipelineContext, message);
			AssertXmlStream(GetEmbeddedResource("TestFiles.MessageWithCDATA_Output.xml"), result.BodyPart.Data);

			MockRepository.VerifyAll();
		}


		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAssembleMessage_AssembleEDI()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.SimpleXMLWithNamespace.xml");
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.PromoteProperty<BTS.SourceParty>("SenderID");
			message.Context.PromoteProperty<BTS.DestinationParty>("RecipientID");
			message.Context.PromoteProperty<EnvelopeTrackingID>("111111111");
			message.Context.PromoteProperty<MessageTrackingID>("222222222");
			message.Context.PromoteProperty<InternalTrackingID>("333333333");
			message.Context.PromoteProperty<BTS.MessageType>("http://cargowise.com/ehub/clients/edi/2010/06#FSU");

			var messageEDI = MessageFactory.CreateMessage();
			messageEDI.AddPart("EDI", MessageFactory.CreateMessagePart(), true);
			messageEDI.BodyPart.Data = GetEmbeddedResource("TestFiles.X12.txt");
			messageEDI.Context = PipelineUtil.CloneMessageContext(message.Context);

			var accessor = MockRepository.StrictMock<ITransformAccessor>();
			string charset;
			Expect.Call(accessor.IsFlatFile("http://cargowise.com/ehub/clients/edi/2010/06#FSU", out charset)).Return(false);
			Expect.Call(accessor.IsEDI("http://cargowise.com/ehub/clients/edi/2010/06#FSU")).Return(true);
			string postAssembleWrapper = null;
			Expect.Call(accessor.IsPostAssembleMapping("http://cargowise.com/ehub/clients/edi/2010/06#FSU", out postAssembleWrapper)).Return(false);

			var ediAssembler = MockRepository.StrictMock<XmlEdiAssembleHelper>();
			Expect.Call(ediAssembler.Assemble(pipelineContext, message)).Do(new AssembleDelegate((IPipelineContext pContext, IBaseMessage sourceMessage) =>
			{
				Assert.AreEqual("RecipientID", sourceMessage.Context.ReadPropertyString<EDI.DestinationPartyName>());
				return messageEDI;
			}));

			var msgHelper = MockRepository.StrictMock<IMessageHelper>();
			Expect.Call(msgHelper.IsAutoSubscriptionRequired(message)).Return(true);
			IBaseMessage subsMsg = null;
			Expect.Call(() => msgHelper.InsertAutoSubscriptionsForSender(null)).IgnoreArguments().Do(new Action<IBaseMessage>((m) => subsMsg = m));

			var component = MockRepository.PartialMock<MessageAssembleComponent>();
			Expect.Call(component.GetTransformationAccessor()).Return(accessor);
			Expect.Call(component.GetEDIAssembler()).Return(ediAssembler);
			Expect.Call(component.GetNewGuid()).Return(new Guid("b2a38b9d-8d5b-42d3-a7bf-a725da25931c"));
			Expect.Call(component.GetCurrentUTCString()).Return("2001-01-20 13:14:15.503");
			Expect.Call(component.GetMessageHelper()).Return(msgHelper).Repeat.Any();

			MockRepository.ReplayAll();

			component.Enabled = true;
			var result = component.Execute(pipelineContext, message);
			AssertXmlStream(GetEmbeddedResource("TestFiles.OutboxInsertEDI.xml"), result.BodyPart.Data);
			AssertXmlStream(GetEmbeddedResource("TestFiles.SimpleXMLWithNamespace.xml"), subsMsg.BodyPart.Data);
			Assert.AreEqual(result.Context.CountProperties, subsMsg.Context.CountProperties);
			CollectionAssert.AreEquivalent(GetProperties(result), GetProperties(subsMsg));

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAssembleMessage_AssembleEDIOverride()
		{
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.SimpleXMLWithNamespace.xml");
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.PromoteProperty<BTS.SourceParty>("SenderID");
			message.Context.PromoteProperty<BTS.DestinationParty>("RecipientID");
			message.Context.PromoteProperty<EnvelopeTrackingID>("111111111");
			message.Context.PromoteProperty<MessageTrackingID>("222222222");
			message.Context.PromoteProperty<InternalTrackingID>("333333333");
			message.Context.PromoteProperty<BTS.MessageType>("http://cargowise.com/ehub/clients/edi/2010/06#FSU");
			message.Context.PromoteProperty<EdiOverride.OverrideEDIHeader>("true");
			message.Context.PromoteProperty<EDI.DestinationPartyName>("Override");

			var messageEDI = MessageFactory.CreateMessage();
			messageEDI.AddPart("EDI", MessageFactory.CreateMessagePart(), true);
			messageEDI.BodyPart.Data = GetEmbeddedResource("TestFiles.X12.txt");
			messageEDI.Context = PipelineUtil.CloneMessageContext(message.Context);

			var accessor = MockRepository.StrictMock<ITransformAccessor>();
			string charset;
			Expect.Call(accessor.IsFlatFile("http://cargowise.com/ehub/clients/edi/2010/06#FSU", out charset)).Return(false);
			Expect.Call(accessor.IsEDI("http://cargowise.com/ehub/clients/edi/2010/06#FSU")).Return(true);
			string postAssembleWrapper = null;
			Expect.Call(accessor.IsPostAssembleMapping("http://cargowise.com/ehub/clients/edi/2010/06#FSU", out postAssembleWrapper)).Return(false);

			var ediAssembler = MockRepository.StrictMock<XmlEdiAssembleHelper>();
			Expect.Call(ediAssembler.Assemble(pipelineContext, message)).Do(new AssembleDelegate((IPipelineContext pContext, IBaseMessage sourceMessage) =>
			{
				Assert.IsNull(sourceMessage.Context.ReadPropertyString<EDI.DestinationPartyName>());
				return messageEDI;
			}));

			var msgHelper = MockRepository.StrictMock<IMessageHelper>();
			Expect.Call(msgHelper.IsAutoSubscriptionRequired(message)).Return(true);
			IBaseMessage subsMsg = null;
			Expect.Call(() => msgHelper.InsertAutoSubscriptionsForSender(null)).IgnoreArguments().Do(new Action<IBaseMessage>((m) => subsMsg = m));

			var component = MockRepository.PartialMock<MessageAssembleComponent>();
			Expect.Call(component.GetTransformationAccessor()).Return(accessor);
			Expect.Call(component.GetEDIAssembler()).Return(ediAssembler);
			Expect.Call(component.GetNewGuid()).Return(new Guid("b2a38b9d-8d5b-42d3-a7bf-a725da25931c"));
			Expect.Call(component.GetCurrentUTCString()).Return("2001-01-20 13:14:15.503");
			Expect.Call(component.GetMessageHelper()).Return(msgHelper).Repeat.Any();

			MockRepository.ReplayAll();

			component.Enabled = true;
			var result = component.Execute(pipelineContext, message);
			AssertXmlStream(GetEmbeddedResource("TestFiles.OutboxInsertEDI.xml"), result.BodyPart.Data);
			AssertXmlStream(GetEmbeddedResource("TestFiles.SimpleXMLWithNamespace.xml"), subsMsg.BodyPart.Data);
			Assert.AreEqual(result.Context.CountProperties, subsMsg.Context.CountProperties);
			CollectionAssert.AreEquivalent(GetProperties(result), GetProperties(subsMsg));

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAssembleMessage_EDIFACTOverridesAndEarlyTermination()
		{
			// UNA6
			AssertEDIFACTOverride(
				"*", null, null, null, true,
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++0++0'UNH+30+IFTMBF:D:99B:UN:2.0'",
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++0++0'UNH+30+IFTMBF:D:99B:UN:2.0'"
			);
			AssertEDIFACTOverride(
				null, "", null, null, true,
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++0++0'UNH+30+IFTMBF:D:99B:UN:2.0'",
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++0++0'UNH+30+IFTMBF:D:99B:UN:2.0'"
			);
			AssertEDIFACTOverride(
				"*", "", null, null, true,
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++0++0'UNH+30+IFTMBF:D:99B:UN:2.0'",
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++0++0*UNH+30+IFTMBF:D:99B:UN:2.0*"
			);
			AssertEDIFACTOverride(
				"'", "\r\n", null, null, true,
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++0++0'UNH+30+IFTMBF:D:99B:UN:2.0'",
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++0++0'\r\nUNH+30+IFTMBF:D:99B:UN:2.0'\r\n"
			);

			// UNB9
			AssertEDIFACTOverride(
				null, null, "a", null, true,
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++0++0'UNH+30+IFTMBF:D:99B:UN:2.0'",
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++a++0'UNH+30+IFTMBF:D:99B:UN:2.0'"
			);
			AssertEDIFACTOverride(
				null, null, "", null, true,
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++0++0'UNH+30+IFTMBF:D:99B:UN:2.0'",
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++++0'UNH+30+IFTMBF:D:99B:UN:2.0'"
			);
			AssertEDIFACTOverride(
				null, null, "", null, false,
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++0++0'UNH+30+IFTMBF:D:99B:UN:2.0'",
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++++0'UNH+30+IFTMBF:D:99B:UN:2.0'"
			);

			// UNB11
			AssertEDIFACTOverride(
				null, null, null, "a", true,
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++0++0'UNH+30+IFTMBF:D:99B:UN:2.0'",
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++0++a'UNH+30+IFTMBF:D:99B:UN:2.0'"
			);
			AssertEDIFACTOverride(
				null, null, null, "", true,
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++0++0'UNH+30+IFTMBF:D:99B:UN:2.0'",
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++0'UNH+30+IFTMBF:D:99B:UN:2.0'"
			);
			AssertEDIFACTOverride(
				null, null, null, "", false,
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++0++0'UNH+30+IFTMBF:D:99B:UN:2.0'",
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++0++'UNH+30+IFTMBF:D:99B:UN:2.0'"
			);

			// all
			AssertEDIFACTOverride(
				null, null, null, null, true,
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++0++0'UNH+30+IFTMBF:D:99B:UN:2.0'",
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++0++0'UNH+30+IFTMBF:D:99B:UN:2.0'"
			);
			AssertEDIFACTOverride(
				null, null, "", "", true,
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+1'UNH+30+IFTMBF:D:99B:UN:2.0'",
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+1'UNH+30+IFTMBF:D:99B:UN:2.0'"
			);
			AssertEDIFACTOverride(
				"*", "\r\n", "a", "b", true,
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++0++0'UNH+30+IFTMBF:D:99B:UN:2.0'",
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++a++b*\r\nUNH+30+IFTMBF:D:99B:UN:2.0*\r\n"
			);
			AssertEDIFACTOverride(
				"'", "\r\n", "", "", true,
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++0++0'UNH+30+IFTMBF:D:99B:UN:2.0'",
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30'\r\nUNH+30+IFTMBF:D:99B:UN:2.0'\r\n"
			);

			// Special characters
			AssertEDIFACTOverride(
				null, null, null, null, true,
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++0++0'UNH+30+IFTMBF:D:99B:UN:2.0'NAD+HI+CGWGEODISSENRK:160:86+NORRKOPING AIRPORT:PO BOX 163+GEODIS WILSON SWEDEN AB+NORRKOPING AIRPORT:PO BOX 163:Norrkoping Östergötlands län:60103 Sweden+Norrkoping+Östergötl+60103+SE'",
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++0++0'UNH+30+IFTMBF:D:99B:UN:2.0'NAD+HI+CGWGEODISSENRK:160:86+NORRKOPING AIRPORT:PO BOX 163+GEODIS WILSON SWEDEN AB+NORRKOPING AIRPORT:PO BOX 163:Norrkoping Östergötlands län:60103 Sweden+Norrkoping+Östergötl+60103+SE'"
			);

			AssertEDIFACTOverride(
				null, null, null, null, true,
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++0++0'UNH+30+IFTMBF:D:99B:UN:2.0'NAD+HI+CGWGEODISBRSAO:160:86+AVENIDA FRANCISCO MATARAZZO, 1350:SP, BARRA FUNDA+GEODIS GERENCIAMENTO DE FRETES DO:BRASIL LTDA+AVENIDA FRANCISCO MATARAZZO, 1350:SP, BARRA FUNDA:Sao Paulo São Paulo 05001100:Brazil+Sao Paulo+São Paulo+05001100+BR'",
				"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++0++0'UNH+30+IFTMBF:D:99B:UN:2.0'NAD+HI+CGWGEODISBRSAO:160:86+AVENIDA FRANCISCO MATARAZZO, 1350:SP, BARRA FUNDA+GEODIS GERENCIAMENTO DE FRETES DO:BRASIL LTDA+AVENIDA FRANCISCO MATARAZZO, 1350:SP, BARRA FUNDA:Sao Paulo São Paulo 05001100:Brazil+Sao Paulo+São Paulo+05001100+BR'"
			);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestAssembleMessage_RaceCondition()
		{
			for (int i = 0; i < 1000; i++)
			{
				AssertEDIFACTOverride(
					"*", "\r\n", "a", "b", true,
					"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++0++0'UNH+30+IFTMBF:D:99B:UN:2.0'",
					"UNB+UNOC:1+CARGOWISE:ZZZ+INTTRANG2:ZZZ+140911:1450+30++++a++b*\r\nUNH+30+IFTMBF:D:99B:UN:2.0*\r\n"
				);
			}
		}

		public void AssertEDIFACTOverride(string UNA6, string UNA6suffix, string UNB9, string UNB11, bool earlyTerminate, String input, String output)
		{
			var mockMessageAssembleComp = MockRepository.GeneratePartialMock<MessageAssembleComponent>();
			var mockMessageHelper = MockRepository.GenerateStub<IMessageHelper>();
			var mockTransformationAccessor = MockRepository.GenerateStub<ITransformAccessor>();
			var mockEdiAssembler = MockRepository.GenerateStub<XmlEdiAssembleHelper>();
			var message = PipelineTesting.MessageHelper.CreateFromStream(new MemoryStream(Encoding.Default.GetBytes(input)));
			var pipeline = PipelineTesting.PipelineFactory.CreateEmptySendPipeline();

			message.Context.WriteProperty<BTS.MessageType>("MESSAGETYPE");
			message.Context.PromoteProperty<EdiOverride.OverrideEDIHeader>("true");
			if (earlyTerminate)
				message.Context.WriteProperty<EarlyTerminateEdifactUnb>("true");
			if (UNA6 != null)
				message.Context.PromoteProperty<EdiOverride.UNA6>(UNA6);
			if (UNA6suffix != null)
				message.Context.PromoteProperty<EdiOverride.UNA6Suffix>(UNA6suffix);
			if (UNB9 != null)
				message.Context.PromoteProperty<EdiOverride.UNB9>(UNB9);
			if (UNB11 != null)
				message.Context.PromoteProperty<EdiOverride.UNB11>(UNB11);

			pipeline.AddComponent(mockMessageAssembleComp, PipelineTesting.PipelineStage.Assemble);
			mockTransformationAccessor.Stub(x => x.IsEDI("MESSAGETYPE")).Return(true);
			mockEdiAssembler.Stub(x => x.Assemble(Arg<IPipelineContext>.Is.Anything, Arg.Is(message))).Return(message);
			mockMessageAssembleComp.Enabled = true;
			mockMessageAssembleComp.Stub(x => x.GetMessageHelper()).Return(mockMessageHelper);
			mockMessageAssembleComp.Stub(x => x.GetTransformationAccessor()).Return(mockTransformationAccessor);
			mockMessageAssembleComp.Stub(x => x.GetEDIAssembler()).Return(mockEdiAssembler);

			var outputMessage = pipeline.Execute(new[] { message });

			var xnav = new XPathDocument(outputMessage.BodyPart.GetOriginalDataStream()).CreateNavigator();
			var msg = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(xnav.SelectSingleNode("/*[local-name()='OutboxInsert']/Content").Value)).DecodeAndDecompress(), Encoding.Default).ReadToEnd();
			Assert.AreEqual(output, msg);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCommonInterfacesImplementation_MessageAssembleComponent()
		{
			var component = new MessageAssembleComponent();
			Assert.AreEqual("Assemble message in xml or native format depending on message type.", component.Description);
			Assert.AreEqual("Message Assembler", component.Name);
			Assert.AreEqual("1.0", component.Version);
			Assert.AreEqual(IntPtr.Zero, component.Icon);
			Assert.IsNull(component.Validate(null));
			var guid = new Guid();
			component.GetClassID(out guid);
			Assert.AreEqual(new Guid("5D365F8C-36B0-4198-8A55-131609D6B943"), guid);

			var propertyBag = MockRepository.StrictMock<IPropertyBag>();

			string enabledProp = null;
			object enabledPtr = false;
			bool enabledValue = true;

			Expect.Call(() => propertyBag.Write("Enabled", ref enabledPtr))
				.Do(new WritePropertyBagDelegate((string propName, ref object ptrVar) => { enabledProp = propName; enabledValue = (bool)ptrVar; }));

			Expect.Call(() => propertyBag.Read("Enabled", out enabledPtr, 0)).
				Do(new ReadPropertyBagDelegate((string propName, out object ptrVar, int errorLog) => { ptrVar = true; }));

			MockRepository.ReplayAll();

			component.Enabled = false;
			component.Save(propertyBag, true, true);
			Assert.AreEqual("Enabled", enabledProp);
			Assert.IsFalse(enabledValue);

			component.Load(propertyBag, 0);
			Assert.IsTrue(component.Enabled);

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestMessageAssembleComponent_PostAssembleAssemble()
		{
			var mockMessageAssembleComp = MockRepository.GeneratePartialMock<MessageAssembleComponent>();
			var mockMessageHelper = MockRepository.GenerateStub<IMessageHelper>();
			var mockTransformationAccessor = MockRepository.GenerateStub<ITransformAccessor>();
			var mockFFAssembler = MockRepository.GenerateStub<IFFAssembleHelper>();
			var mockMultipleTransformationComponent = MockRepository.GenerateMock<IComponent>();
			var message1 = PipelineTesting.MessageHelper.CreateFromStream(new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE1")));
			message1.Context.WriteProperty<BTS.MessageType>("NAMESPACE1#MESSAGETYPE1");
			var message2 = PipelineTesting.MessageHelper.CreateFromStream(new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE2")));
			message2.Context.WriteProperty<BTS.MessageType>("NAMESPACE2#MESSAGETYPE2");
			var pipeline = PipelineTesting.PipelineFactory.CreateEmptySendPipeline();
			pipeline.AddComponent(mockMessageAssembleComp, PipelineTesting.PipelineStage.Assemble);
			string charset = null;
			mockTransformationAccessor.Stub(x => x.IsFlatFile("NAMESPACE1#MESSAGETYPE1", out charset)).Return(true);
			string postAssembleWrapper = null;
			mockTransformationAccessor.Stub(x => x.IsPostAssembleMapping("NAMESPACE1#MESSAGETYPE1", out postAssembleWrapper)).Return(true);
			mockFFAssembler.Stub(x => x.Assemble(Arg<IPipelineContext>.Is.Anything, Arg.Is(message1))).Return(message1);
			mockMessageHelper.Stub(x => x.IsAutoSubscriptionRequired(Arg<IBaseMessage>.Is.Anything)).Return(false);
			mockMultipleTransformationComponent.Stub(x => x.Execute(Arg<IPipelineContext>.Is.Anything, Arg<IBaseMessage>.Is.Anything))
				.Callback(new Func<IPipelineContext, IBaseMessage, bool>((c, m) =>
				{
					string msgText = m.BodyPart.Data.ReadToEnd();
					string msgType = m.Context.ReadPropertyString<BTS.MessageType>();
					return msgText == "<ns0:MESSAGETYPE1 xmlns:ns0=\"NAMESPACE1\"><![CDATA[MESSAGE1]]></ns0:MESSAGETYPE1>" && msgType == "NAMESPACE1#MESSAGETYPE1";
				}))
				.Return(message2);
			mockMessageAssembleComp.Enabled = true;
			mockMessageAssembleComp.Stub(x => x.GetMessageHelper()).Return(mockMessageHelper);
			mockMessageAssembleComp.Stub(x => x.GetTransformationAccessor()).Return(mockTransformationAccessor);
			mockMessageAssembleComp.Stub(x => x.GetFFAssembleHelper()).Return(mockFFAssembler);
			mockMessageAssembleComp.Stub(x => x.GetMultipleTransformationComponent()).Return(mockMultipleTransformationComponent);

			var outputMessage = pipeline.Execute(new[] { message1 });

			Assert.AreSame(outputMessage, message2);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestMessageAssembleComponent_PostAssembleWrapper()
		{
			var mockMessageAssembleComp = MockRepository.GeneratePartialMock<MessageAssembleComponent>();
			var mockMessageHelper = MockRepository.GenerateStub<IMessageHelper>();
			var mockTransformationAccessor = MockRepository.GenerateStub<ITransformAccessor>();
			var mockFFAssembler = MockRepository.GenerateStub<IFFAssembleHelper>();
			var mockMultipleTransformationComponent = MockRepository.GenerateMock<IComponent>();
			var message1 = PipelineTesting.MessageHelper.CreateFromStream(new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE1")));
			message1.Context.WriteProperty<BTS.MessageType>("NAMESPACE1#MESSAGETYPE1");
			var message2 = PipelineTesting.MessageHelper.CreateFromStream(new MemoryStream(Encoding.UTF8.GetBytes("MESSAGE2")));
			message2.Context.WriteProperty<BTS.MessageType>("NAMESPACE2#MESSAGETYPE2");
			var pipeline = PipelineTesting.PipelineFactory.CreateEmptySendPipeline();
			pipeline.AddComponent(mockMessageAssembleComp, PipelineTesting.PipelineStage.Assemble);
			string charset = null;
			mockTransformationAccessor.Stub(x => x.IsFlatFile("NAMESPACE1#MESSAGETYPE1", out charset)).Return(true);
			string postAssembleWrapper = null;
			mockTransformationAccessor.Stub(x => x.IsPostAssembleMapping("NAMESPACE1#MESSAGETYPE1", out postAssembleWrapper)).Return(true).OutRef("WRAPPERNS#WRAPPERNAME");
			mockFFAssembler.Stub(x => x.Assemble(Arg<IPipelineContext>.Is.Anything, Arg.Is(message1))).Return(message1);
			mockMessageHelper.Stub(x => x.IsAutoSubscriptionRequired(Arg<IBaseMessage>.Is.Anything)).Return(false);
			mockMultipleTransformationComponent.Stub(x => x.Execute(Arg<IPipelineContext>.Is.Anything, Arg<IBaseMessage>.Is.Anything))
				.Callback(new Func<IPipelineContext, IBaseMessage, bool>((c, m) =>
				{
					string msgText = m.BodyPart.Data.ReadToEnd();
					string msgType = m.Context.ReadPropertyString<BTS.MessageType>();
					return msgText == "<ns0:WRAPPERNAME xmlns:ns0=\"WRAPPERNS\"><![CDATA[MESSAGE1]]></ns0:WRAPPERNAME>" && msgType == "WRAPPERNS#WRAPPERNAME";
				}))
				.Return(message2);
			mockMessageAssembleComp.Enabled = true;
			mockMessageAssembleComp.Stub(x => x.GetMessageHelper()).Return(mockMessageHelper);
			mockMessageAssembleComp.Stub(x => x.GetTransformationAccessor()).Return(mockTransformationAccessor);
			mockMessageAssembleComp.Stub(x => x.GetFFAssembleHelper()).Return(mockFFAssembler);
			mockMessageAssembleComp.Stub(x => x.GetMultipleTransformationComponent()).Return(mockMultipleTransformationComponent);

			var outputMessage = pipeline.Execute(new[] { message1 });

			Assert.AreSame(outputMessage, message2);
		}
	}
}
