using System;
using System.CodeDom;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Products.CACustoms.Schemas;
using CargoWise.eHub.Core.Tests.PipelineComponents;
using Microsoft.BizTalk.Component.Interop;
using Rhino.Mocks;
using Microsoft.BizTalk.Message.Interop;
using System.Collections;
using System.Data.SqlClient;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Products.CACustoms.PipelineComponents;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.Core.Transforms.Helper;

namespace CargoWise.eHub.Products.CACustoms.Tests
{
	[TestClass]
	public class CACEdiDissasemblerExtensionTest : BaseComponentTest
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Tester()
		{
			var tester = new CACEdiDissasemblerExtension();
			var classID = Guid.Empty;
			tester.GetClassID(out classID);
			Assert.AreEqual<Guid>(new Guid("D2F4B0A2-775D-4D13-84A4-2F2738B155A7"), classID);
			Assert.AreEqual<string>("1.0", tester.Version);
			Assert.AreEqual<string>("", tester.Description);
			Assert.AreEqual<string>("EDI Disassembler Extension for CACustoms", tester.Name);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestPreserveRawMessageBeforeProcessing()
		{
			var fullMessage = "UNB+UNOC:3+INETCECPT+HYETSTTST+200924:1748+2'UNG+GOVCBR+IIDT+U10207V1+20200924:1748+1+UN+D:13A'Hello Québec!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~ ¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ;";
			TestCACEdiDisassembleExtension_EDIFACTProcessingMessage(fullMessage);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestPreserveRawMessageBeforeProcessing_NewLine()
		{
			var fullMessage = TestHelper.GetEmbeddedResourceAsString("PipelineComponents.TestFiles.CACustoms_EDIFACTProcessing_Newline.txt");
			TestCACEdiDisassembleExtension_EDIFACTProcessingMessage(fullMessage);
		}

		public void TestCACEdiDisassembleExtension_EDIFACTProcessingMessage(string messageContent)
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream(Encoding.UTF8.GetBytes(messageContent));
			message.Context = MessageFactory.CreateMessageContext();

			var newMessage1 = MessageFactory.CreateMessage();
			newMessage1.AddPart("newMessage1", MessageFactory.CreateMessagePart(), true);
			newMessage1.BodyPart.Data = new MemoryStream(Encoding.UTF8.GetBytes("A"));
			newMessage1.Context = MessageFactory.CreateMessageContext();
			newMessage1.Context.WriteProperty<BTS.MessageType>("http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006#EFACT_D13A_GOVCB");
			newMessage1.Context.Write("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema", "TESTING");

			var newMessage2 = MessageFactory.CreateMessage();
			newMessage2.AddPart("newMessage2", MessageFactory.CreateMessagePart(), true);
			newMessage2.BodyPart.Data = new MemoryStream(Encoding.UTF8.GetBytes("B"));
			newMessage2.Context = MessageFactory.CreateMessageContext();
			newMessage2.Context.WriteProperty<BTS.MessageType>("http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006#EFACT_D13A_GOVCBR");
			newMessage2.Context.Write("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema", "TESTING");

			var newMessage3 = MessageFactory.CreateMessage();
			newMessage3.AddPart("newMessage3", MessageFactory.CreateMessagePart(), true);
			newMessage3.BodyPart.Data = new MemoryStream(Encoding.UTF8.GetBytes("C"));
			newMessage3.Context = MessageFactory.CreateMessageContext();
			newMessage3.Context.Write("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema", "TESTING");

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			SetupResult.For(pipelineContext.PipelineName).Return("pipelineName");

			var component = MockRepository.PartialMock<CACEdiDissasemblerExtension>();
			Expect.Call(() => component.BaseDisassemble(pipelineContext, message));
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(newMessage1);
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(newMessage2);
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(newMessage3);
			Expect.Call(component.BaseGetNext(pipelineContext)).Return(null);

			var partyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			partyAccessor.Expect(m=>m.GetClientSystemIDFromClientSystemRegistration("CACustomsSystemReference", "TESTING")).Return("HYETST");
			Expect.Call(component.GetPartyAccessor()).Return(partyAccessor);

			var transformAccessorMock = MockRepository.StrictMock<ITransformAccessor>();
			transformAccessorMock.Expect(x =>
					x.CallActionProcedure(
						Arg<string>.Is.Equal("SelectFirstActiveeHubClientPerSystem"),
						Arg<string>.Is.Null,
						Arg<string[]>.Matches(para => para.Length == 4 && para[0] == "@enterpriseCode" && para[2] == "@serverCode")))
				.Return(null)
				.WhenCalled(method =>
				{
					var enterpriseCode = ((string[])method.Arguments[2])[1];
					var serverCode = ((string[])method.Arguments[2])[3];
					if (enterpriseCode == "HYE" && serverCode == "TST")
					{
						method.ReturnValue = "HYECW1TST";
						return;
					}

					method.ReturnValue = string.Empty;
				});
			Expect.Call(component.GetTransformAccessor()).Return(transformAccessorMock);

			MockRepository.ReplayAll();

			component.Disassemble(pipelineContext, message);
			var result = component.GetNext(pipelineContext);
			Assert.AreEqual(null, result.Context.ReadPropertyString<RawMessage>());
			Assert.AreEqual(newMessage1, result);
			Assert.AreEqual("HYECW1TST", result.Context.ReadPropertyString<BTS.DestinationParty>());
			result = component.GetNext(pipelineContext);
			Assert.AreEqual(messageContent, result.Context.ReadPropertyString<RawMessage>());
			Assert.AreEqual(newMessage2, result);
			Assert.AreEqual("HYECW1TST", result.Context.ReadPropertyString<BTS.DestinationParty>());
			result = component.GetNext(pipelineContext);
			Assert.AreEqual(null, result.Context.ReadPropertyString<RawMessage>());
			Assert.AreEqual(newMessage3, result);
			Assert.AreEqual("HYECW1TST", result.Context.ReadPropertyString<BTS.DestinationParty>());
			Assert.IsNull(component.GetNext(pipelineContext));

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCACEdiDisassembleExtensionTest_EDIFACTMessage()
		{
			var messageText = "UNB+UNOA:3+INETCECPP+HYETSTTST+201007:1938+1044985'UNG+CUSRES+CCR+U10207V1+201007:1938+457880+UN+D:96A'UNH+1+CUSRES:D:96A:UN'BGM+:::911+10105001998631+11'LOC+22+0440:129::9440'DTM+58:202010071938:203'GIS+4'RFF+XC:22EJC0828244'UNT+7+1'UNH+2+CUSRES:D:96A:UN'BGM+:::911+10105002002946+11'LOC+22+0440:129::9440'DTM+58:202010071938:203'GIS+4'RFF+XC:22EJC0828775'UNT+7+2'UNH+3+CUSRES:D:96A:UN'BGM+:::911+10105002004027+11'LOC+22+0440:129::9440'DTM+58:202010071938:203'GIS+4'RFF+XC:22EJC0828713'UNT+7+3'UNE+3+457880'UNZ+1+1044985'";
			var expected = @"<CanadianCustomsReply xmlns=""http://cargowise.com/ehub/products/canadiancustoms"" xmlns:ns0=""http://cargowise.com/ehub/core/2011/02"">
	<Reference>HYETSTTST - INETCECPP</Reference>
	<Content>VU5CK1VOT0E6MytJTkVUQ0VDUFArSFlFVFNUVFNUKzIwMTAwNzoxOTM4KzEwNDQ5ODUnVU5HK0NVU1JFUytDQ1IrVTEwMjA3VjErMjAxMDA3OjE5MzgrNDU3ODgwK1VOK0Q6OTZBJ1VOSCsxK0NVU1JFUzpEOjk2QTpVTidCR00rOjo6OTExKzEwMTA1MDAxOTk4NjMxKzExJ0xPQysyMiswNDQwOjEyOTo6OTQ0MCdEVE0rNTg6MjAyMDEwMDcxOTM4OjIwMydHSVMrNCdSRkYrWEM6MjJFSkMwODI4MjQ0J1VOVCs3KzEnVU5IKzIrQ1VTUkVTOkQ6OTZBOlVOJ0JHTSs6Ojo5MTErMTAxMDUwMDIwMDI5NDYrMTEnTE9DKzIyKzA0NDA6MTI5Ojo5NDQwJ0RUTSs1ODoyMDIwMTAwNzE5Mzg6MjAzJ0dJUys0J1JGRitYQzoyMkVKQzA4Mjg3NzUnVU5UKzcrMidVTkgrMytDVVNSRVM6RDo5NkE6VU4nQkdNKzo6OjkxMSsxMDEwNTAwMjAwNDAyNysxMSdMT0MrMjIrMDQ0MDoxMjk6Ojk0NDAnRFRNKzU4OjIwMjAxMDA3MTkzODoyMDMnR0lTKzQnUkZGK1hDOjIyRUpDMDgyODcxMydVTlQrNyszJ1VORSszKzQ1Nzg4MCdVTlorMSsxMDQ0OTg1Jw==</Content>
</CanadianCustomsReply>";
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream(Encoding.UTF8.GetBytes(messageText));
			message.Context = MessageFactory.CreateMessageContext();

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			SetupResult.For(pipelineContext.PipelineName).Return("pipelineName");
			SetupResult.For(pipelineContext.GetMessageFactory()).Return(MessageFactory);
			var subscriptionAccessorMock = MockRepository.StrictMock<ISubscriptionAccessor>();
			subscriptionAccessorMock.Expect(_ => _.SelectSubscribedClients(message)).IgnoreArguments().Repeat.Once().Return(new[] {"TSTTSTTST", "HYETSTTST"});
			var transformAccessorMock = MockRepository.StrictMock<ITransformAccessor>();
			transformAccessorMock.Expect(x =>
					x.CallActionProcedure(
						Arg<string>.Is.Equal("SelectFirstActiveeHubClientPerSystem"),
						Arg<string>.Is.Null,
						Arg<string[]>.Matches(para =>para.Length == 4 && para[0] == "@enterpriseCode" && para[2] == "@serverCode")))
				.Return(null)
				.WhenCalled(method =>
				{
					var enterpriseCode = ((string[])method.Arguments[2])[1];
					var serverCode = ((string[])method.Arguments[2])[3];
					if (enterpriseCode == "HYE" && serverCode == "TST")
					{
						method.ReturnValue = "HYECW1TST";
						return;
					}

					method.ReturnValue = string.Empty;
				});

			var component = MockRepository.PartialMock<CACEdiDissasemblerExtension>();
			component.ProcessSubscriptions = true;
			component.Expect(_ => _.BaseDisassemble(pipelineContext, message)).Repeat.Never();
			component.Expect(_ => _.GetSubscriptionAccessor()).Repeat.Once().Return(subscriptionAccessorMock);
			component.Expect(x => x.GetTransformAccessor()).Repeat.Times(2).Return(transformAccessorMock);

			var partyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			partyAccessor.Expect(m => m.GetClientSystemIDFromClientSystemRegistration("CACustomsSystemReference", "TESTING")).Return("");
			Expect.Call(component.GetPartyAccessor()).Return(partyAccessor);

			MockRepository.ReplayAll();

			component.Disassemble(pipelineContext, message);
			var result = component.GetNext(pipelineContext);
			var resultText = result.BodyPart.GetOriginalDataStream().ReadToEnd();
			Assert.AreEqual(expected, resultText);
			Assert.AreEqual("HYECW1TST", result.Context.ReadPropertyString<BTS.DestinationParty>());
			Assert.AreEqual("http://cargowise.com/ehub/products/canadiancustoms#CanadianCustomsReply", result.Context.ReadPropertyString<BTS.MessageType>());

			result = component.GetNext(pipelineContext);
			Assert.IsNull(result);

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCACEdiDisassembleExtensionTest_EDIFACTProcessing_MissingRecipient()
		{
			var fullMessage = "UNB+UNOC:3+INETCECPT+HYETSTTST+200924:1748+2'UNG+GOVCBR+IIDT+U10207V1+20200924:1748+1+UN+D:13A'Hello Québec!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~ ¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ;";
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream(Encoding.UTF8.GetBytes(fullMessage));
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.Write("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06", "00000000-0000-0000-0000-000000000001");
			message.Context.Write("MessageTrackingID", "http://cargowise.com/ehub/tracking/2010/06", "00000000-0000-0000-0000-000000000001");

			var newMessage1 = MessageFactory.CreateMessage();
			newMessage1.AddPart("newMessage1", MessageFactory.CreateMessagePart(), true);
			newMessage1.BodyPart.Data = new MemoryStream(Encoding.UTF8.GetBytes("A"));
			newMessage1.Context = MessageFactory.CreateMessageContext();
			newMessage1.Context.WriteProperty<BTS.MessageType>("http://schemas.microsoft.com/BizTalk/EDI/EDIFACT/2006#EFACT_D13A_GOVCB");
			newMessage1.Context.Write("UNB3_1", "http://schemas.microsoft.com/Edi/PropertySchema", "HYETSTTST");

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			SetupResult.For(pipelineContext.PipelineName).Return("pipelineName");

			var component = MockRepository.PartialMock<CACEdiDissasemblerExtension>();
			var exceptionsAccessor = MockRepository.StrictMock<IExceptionsAccessor>();
			exceptionsAccessor.Expect(x => x.SubmitErrorAndUpdateStatus(
				Arg<Guid>.Is.Anything,
				Arg<string>.Is.Equal("BIZ"),
				Arg<string>.Is.Equal("Failure"),
				Arg<string>.Matches(description =>
					description.Contains("\r\n----------BIZTALK PROPERTIES----------\r\n")
					&& description.Contains("http://cargowise.com/ehub/processing/2010/06#CanadianCustomsReplyReference = HYETSTTST")),
				Arg<Guid>.Is.Equal(new Guid("00000000-0000-0000-0000-000000000001")),
				Arg<Guid>.Is.Anything,
				Arg<Guid>.Is.Equal(new Guid("00000000-0000-0000-0000-000000000001")),
				Arg<Guid>.Is.Anything,
				Arg<SqlConnection>.Is.Anything,
				Arg<bool>.Is.Equal(false))).Repeat.Once();

			component.Stub(x => x.GetExceptionsAccessor()).Return(exceptionsAccessor);
			component.Expect(x => x.BaseDisassemble(pipelineContext, message));
			component.Expect(x => x.BaseGetNext(pipelineContext)).Return(newMessage1);

			var partyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			partyAccessor.Expect(m => m.GetClientSystemIDFromClientSystemRegistration("CACustomsSystemReference", "TESTING")).Return("");
			Expect.Call(component.GetPartyAccessor()).Return(partyAccessor);

			MockRepository.ReplayAll();
			component.Disassemble(pipelineContext, message);
			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestCACEdiDisassembleExtensionTest_EDIFACTMessage_MissingRecipient()
		{
			var fullMessage = "UNB+UNOA:3+INETCECPT+HYETSTTST+201007:1938+1044985'UNG+CUSRES+CCR+U10207V1+201007:1938+457880+UN+D:96A'UNH+1+CUSRES:D:96A:UN'BGM+:::911+10105001998631+11'LOC+22+0440:129::9440'DTM+58:202010071938:203'GIS+4'RFF+XC:22EJC0828244'UNT+7+1'UNH+2+CUSRES:D:96A:UN'BGM+:::911+10105002002946+11'LOC+22+0440:129::9440'DTM+58:202010071938:203'GIS+4'RFF+XC:22EJC0828775'UNT+7+2'UNH+3+CUSRES:D:96A:UN'BGM+:::911+10105002004027+11'LOC+22+0440:129::9440'DTM+58:202010071938:203'GIS+4'RFF+XC:22EJC0828713'UNT+7+3'UNE+3+457880'UNZ+1+1044985'";
			var message = MessageFactory.CreateMessage();
			message.AddPart("message", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream(Encoding.UTF8.GetBytes(fullMessage));
			message.Context = MessageFactory.CreateMessageContext();
			message.Context.Write("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06", "00000000-0000-0000-0000-000000000001");
			message.Context.Write("MessageTrackingID", "http://cargowise.com/ehub/tracking/2010/06", "00000000-0000-0000-0000-000000000001");

			var pipelineContext = MockRepository.StrictMock<IPipelineContext>();
			SetupResult.For(pipelineContext.PipelineName).Return("pipelineName");

			var component = MockRepository.PartialMock<CACEdiDissasemblerExtension>();
			component.ProcessSubscriptions = true;
			var exceptionsAccessor = MockRepository.StrictMock<IExceptionsAccessor>();
			exceptionsAccessor.Expect(x => x.SubmitErrorAndUpdateStatus(
				Arg<Guid>.Is.Anything,
				Arg<string>.Is.Equal("BIZ"),
				Arg<string>.Is.Equal("Failure"),
				Arg<string>.Matches(description =>
					description.Contains("\r\n----------BIZTALK PROPERTIES----------\r\n")
					&& description.Contains("http://cargowise.com/ehub/processing/2010/06#CanadianCustomsReplyReference = HYETSTTST - INETCECPT")),
				Arg<Guid>.Is.Equal(new Guid("00000000-0000-0000-0000-000000000001")),
				Arg<Guid>.Is.Anything,
				Arg<Guid>.Is.Equal(new Guid("00000000-0000-0000-0000-000000000001")),
				Arg<Guid>.Is.Anything,
				Arg<SqlConnection>.Is.Anything,
				Arg<bool>.Is.Equal(false))).Repeat.Once();
			var subscriptionAccessor = MockRepository.StrictMock<ISubscriptionAccessor>();
			subscriptionAccessor.Expect(_ => _.SelectSubscribedClients(message)).IgnoreArguments().Repeat.Once().Return(new string[0]);

			component.Stub(x => x.GetExceptionsAccessor()).Return(exceptionsAccessor);
			component.Stub(x => x.GetSubscriptionAccessor()).Return(subscriptionAccessor);

			var partyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			partyAccessor.Expect(m => m.GetClientSystemIDFromClientSystemRegistration("CACustomsSystemReference", "TESTING")).Return("");
			Expect.Call(component.GetPartyAccessor()).Return(partyAccessor);

			MockRepository.ReplayAll();
			component.Disassemble(pipelineContext, message);
			MockRepository.VerifyAll();
		}
	}
}
