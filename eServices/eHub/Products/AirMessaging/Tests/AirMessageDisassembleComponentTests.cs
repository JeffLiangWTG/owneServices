using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.eHub.Clients.EDI.Schemas.CargoIMP;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Core.PipelineComponents;
using CargoWise.eHub.Core.PipelineComponents.Tools;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.DataAccess.Integration;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.AirMessaging.PipelineComponents;
using CargoWise.eHub.Products.AirMessaging.Schemas;
using Common.Logging;
using Common.Logging.Simple;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Component.Utilities;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using Winterdom.BizTalk.PipelineTesting.Simple;
using System.Configuration;
using Microsoft.BizTalk.Streaming;

namespace Tests
{
	[TestClass]
	public class TraxonDisassembleComponentTests : BaseComponentTests
	{
		private static readonly string ISACXPathPattern = "/*[local-name()='FMA_FNA' and namespace-uri()='http://cargowise.com/ehub/clients/edi/2011/09']/*[local-name()='OriginalMessage' and namespace-uri()='']/*[local-name()='ISAC' and namespace-uri()='']";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void MessageDisassemble()
		{
			ConfigurationManager.AppSettings["IssueManagerUri"] = "";
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream();

			var ffMessage = MessageFactory.CreateMessage();
			ffMessage.Context = MessageFactory.CreateMessageContext();
			ffMessage.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			ffMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.FSU1.xml");

			var ffMessageEmptyMessageType = MessageFactory.CreateMessage();
			ffMessageEmptyMessageType.Context = MessageFactory.CreateMessageContext();
			ffMessageEmptyMessageType.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			ffMessageEmptyMessageType.BodyPart.Data = GetEmbeddedResource("TestFiles.FSU1_EmptyMessageType.xml");

			var ffMessageEmptyBody = MessageFactory.CreateMessage();
			ffMessageEmptyBody.Context = MessageFactory.CreateMessageContext();
			ffMessageEmptyBody.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			ffMessageEmptyBody.BodyPart.Data = GetEmbeddedResource("TestFiles.FSU1_EmptyBody.xml");

			var ffMessageWrongSchema = MessageFactory.CreateMessage();
			ffMessageWrongSchema.Context = MessageFactory.CreateMessageContext();
			ffMessageWrongSchema.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			ffMessageWrongSchema.BodyPart.Data = GetEmbeddedResource("TestFiles.FSU1_WrongMessageSchema.xml");

			var ffDisassembler = MockRepository.StrictMock<IFFDisassembleHelper>();
			ffDisassembler.Expect(x => x.Disassemble(pipelineContext, message, new SchemaWithNone("TraxonName, namespace"))).Return(ffMessage).Repeat.Times(2);
			ffDisassembler.Expect(x => x.Disassemble(pipelineContext, message, new SchemaWithNone("TraxonName, namespace"))).Return(ffMessageEmptyBody);
			ffDisassembler.Expect(x => x.Disassemble(pipelineContext, message, new SchemaWithNone("TraxonName, namespace"))).Return(ffMessageWrongSchema);
			ffDisassembler.Expect(x => x.Disassemble(pipelineContext, message, new SchemaWithNone("TraxonName, namespace"))).Return(ffMessage);

			var component = MockRepository.PartialMock<AirMessageDisassembleComponent>();
			component.Expect(x => x.GetFFDisassembler()).Return(ffDisassembler).Repeat.Any();

			var contextEmpty = MockRepository.PartialMock<BTEnvelopContext>();
			contextEmpty.SenderID = null;
			contextEmpty.RecipientID = null;
			component.Expect(x => x.CreateEnvelopContext(null)).IgnoreArguments().Return(contextEmpty);

			var contextEmptyMessageType = MockRepository.PartialMock<BTEnvelopContext>();
			contextEmptyMessageType.SenderID = "Sender1";
			contextEmptyMessageType.RecipientID = "Recipient1";
			component.Expect(x => x.CreateEnvelopContext(null)).IgnoreArguments().Return(contextEmptyMessageType);

			var contextEmptyInternalMessage = MockRepository.PartialMock<BTEnvelopContext>();
			contextEmptyInternalMessage.SenderID = "Sender1";
			contextEmptyInternalMessage.RecipientID = "Recipient1";
			contextEmptyInternalMessage.MessageType = "FSU";
			component.Expect(x => x.CreateEnvelopContext(null)).IgnoreArguments().Return(contextEmptyInternalMessage);

			var contextWrongMessageType = MockRepository.PartialMock<BTEnvelopContext>();
			contextWrongMessageType.SenderID = "Sender1";
			contextWrongMessageType.RecipientID = "Recipient1";
			contextWrongMessageType.MessageType = "Bla";
			contextWrongMessageType.MessageVersion = "12";
			contextWrongMessageType.InternalMessage = "Test Data";
			component.Expect(x => x.CreateEnvelopContext(null)).IgnoreArguments().Return(contextWrongMessageType);

			var context = MockRepository.PartialMock<BTEnvelopContext>();
			context.SenderID = "Sender1";
			context.RecipientID = "Recipient1";
			context.MessageType = "FSU";
			context.InternalMessage = "UNB+IATA:1+REUAIR08AFR:PIMA+REUAGT89CARGOW/SYD01:PIMA+101120:1015+ICREF+0'UNH+MSGREF+CIMFSU:12'FSU/12\r\n239-12345675FRAMRU/T1K14.1\r\nRCF/MK059/20NOV1300/MRU/T1K14.1//A1117-P\r\n'UNT+3+MSGREF'UNZ+1+ICREF'";
			component.Expect(x => x.CreateEnvelopContext(null)).IgnoreArguments().Return(context);

			var logger = MockRepository.GeneratePartialMock<ConsoleOutLogger>("Test", LogLevel.Trace, true, false, false, "s");
			component.Stub(x => x.GetPipelineLogger(Arg<IBaseMessage>.Is.Anything)).Return(logger);
			
			var inboxAccessor = MockRepository.Stub<IInboxAccessor>();
			inboxAccessor.Expect(x => x.InsertToInbox(null, Guid.Empty, Guid.Empty, MessageStatus.Processing, null)).IgnoreArguments().Repeat.Times(9);
			component.Expect(x => x.GetInboxAccessor()).Return(inboxAccessor).Repeat.Times(9);

			var exceptionsAccessor = MockRepository.Stub<IExceptionsAccessor>();
			exceptionsAccessor.Expect(x => x.SubmitErrorAndUpdateStatus(
				Arg<Guid>.Is.Anything,
				Arg<string>.Is.Equal("BIZ"),
				Arg<string>.Is.Equal("Failure"), 
				Arg<string>.Matches(description => description.Contains("\r\n----------BIZTALK PROPERTIES----------\r\n")),
				Arg<Guid>.Is.Anything,
				Arg<Guid>.Is.Anything,
				Arg<Guid>.Is.Anything,
				Arg<Guid>.Is.Anything,
				Arg<SqlConnection>.Is.Anything,
				Arg<bool>.Is.Equal(false))).Repeat.Times(8);
			component.Expect(x => x.GetExceptionsAccessor()).Return(exceptionsAccessor).Repeat.Times(8);

			logger.Expect(x => x.ErrorFormat(Arg<string>.Is.Equal("Report details:\r\n{0}"), Arg<object[]>.Is.Anything)).Repeat.Never();

			component.Expect(x => x.DisassembleInternalMessage(pipelineContext, context, null, null, new SchemaWithNone("FSUName, namespace"))).IgnoreArguments();
			MockRepository.ReplayAll();

			component.Disassemble(pipelineContext, message);
			Assert.AreEqual(message, component.GetNext(pipelineContext));

			component.Enabled = true;

			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "ServiceProviderID must be specified.");
			component.ServiceProviderID = "Traxon";

			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "EnvelopeMessageSchema must be specified.");
			component.EnvelopeMessageSchema = new Schema("TraxonName, namespace");

			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "FSUMessageSchema must be specified.");
			component.FSUMessageSchema = new Schema("FSUName, namespace");

			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "FMAFNAMessageSchema must be specified.");
			component.FMAFNAMessageSchema = new Schema("FMAFNAName, namespace");

			ffMessage.BodyPart.Data.Position = 0;
			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "The message sender can not be recognized.");

			ffMessage.BodyPart.Data.Position = 0;
			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "MessageType shoudn't be empty.");

			ffMessage.BodyPart.Data.Position = 0;
			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "InternalMessage shoudn't be empty.");

			ffMessage.BodyPart.Data.Position = 0;
			AssertException(() => { component.Disassemble(pipelineContext, message); }, typeof(ApplicationException), "Body Message Schema 'Bla' not recognized.");

			ffMessage.BodyPart.Data.Position = 0;
			component.Disassemble(pipelineContext, message);

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void MessageDisassemble_XmlCharacter()
		{
			ConfigurationManager.AppSettings["IssueManagerUri"] = "";
			var envelopePrepend = @"<ns0:Body xmlns:ns0=""http://Envelope"">";
			var envelopeAppend = "</ns0:Body>";
			var internalMessage = GetEmbeddedResource("TestFiles.FSU1_XmlCharacter_InternalMessage.txt").ReadToEnd();
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream();

			var ffMessage = MessageFactory.CreateMessage();
			ffMessage.Context = MessageFactory.CreateMessageContext();
			ffMessage.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			ffMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.FSU1_XmlCharacter.xml");

			var ffDisassembler = MockRepository.StrictMock<IFFDisassembleHelper>();
			ffDisassembler.Expect(x => x.Disassemble(pipelineContext, message, new SchemaWithNone("CargoIMP, namespace"))).Return(ffMessage);
			ffDisassembler.Expect(x => x.Disassemble(Arg<PipelineContext>.Is.Anything, Arg<IBaseMessage>.Is.Anything, Arg<SchemaWithNone>.Matches(sc => sc.SchemaName.Equals("FSUName, namespace")))).Return(ffMessage);

			var subscriptionAccessor = MockRepository.StrictMock<ISubscriptionAccessor>();
			subscriptionAccessor.Expect(x => x.SelectSubscriptions(null)).IgnoreArguments().Return(new SubscriptionInfo[] 
			{
				new SubscriptionInfo { Subscriber = "Recipient2", ReferenceType = "EnvelopePrepend", Reference = envelopePrepend },
				new SubscriptionInfo { Subscriber = "Recipient2", ReferenceType = "EnvelopeAppend", Reference = envelopeAppend }
			});

			var partyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			partyAccessor.Stub(x => x.GetAirlineCodeFromPrefix(Arg<string>.Is.Anything)).Return("CX");

			var component = MockRepository.PartialMock<AirMessageDisassembleComponent>();
			component.Expect(x => x.GetFFDisassembler()).Return(ffDisassembler).Repeat.Any();
			component.Expect(x => x.GetSubsriptionAccessor()).Return(subscriptionAccessor).Repeat.Any();
			component.Expect(x => x.GetPartyAccessor()).Return(partyAccessor).Repeat.Any();

			var context = MockRepository.PartialMock<BTEnvelopContext>();
			context.SenderID = "Sender1";
			context.RecipientID = "Recipient1";
			context.MessageType = "FSU";
			context.InternalMessage = internalMessage;
			component.Expect(x => x.CreateEnvelopContext(null)).IgnoreArguments().Return(context);

			var logger = MockRepository.GeneratePartialMock<ConsoleOutLogger>("Test", LogLevel.Trace, true, false, false, "s");
			component.Stub(x => x.GetPipelineLogger(Arg<IBaseMessage>.Is.Anything)).Return(logger);

			var inboxAccessor = MockRepository.Stub<IInboxAccessor>();
			inboxAccessor.Expect(x => x.InsertToInbox(null, Guid.Empty, Guid.Empty, MessageStatus.Processing, null)).IgnoreArguments().Repeat.Once();
			component.Expect(x => x.GetInboxAccessor()).Return(inboxAccessor).Repeat.Once();

			MockRepository.ReplayAll();

			component.Enabled = true;
			component.ReplaceRecipientOnSubscription = true;
			component.ProcessSubscriptions = true;
			component.ServiceProviderID = "Descartes";
			component.EnvelopeMessageSchema = new Schema("CargoIMP, namespace");
			component.FMAFNAMessageSchema = new Schema("FMAFNAName, namespace");
			component.FSUMessageSchema = new Schema("FSUName, namespace");

			component.Disassemble(pipelineContext, message);
			var outputMessage = component.GetNext(pipelineContext);

			var expectedContent = envelopePrepend + internalMessage.Replace("&", "&amp;") + envelopeAppend;

			Assert.AreEqual("http://Envelope#Body", outputMessage.Context.ReadPropertyString<BTS.MessageType>());
			Assert.AreEqual("Sender1", outputMessage.Context.ReadPropertyString<BTS.SourceParty>());
			Assert.AreEqual(expectedContent, outputMessage.BodyPart.Data.ReadToEnd());

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void MessageDisassemble_ExceptionAccessor_Fail()
		{
			ConfigurationManager.AppSettings["IssueManagerUri"] = "";
			var pipelineContext = new PipelineContext();

			var ffMessage = MessageFactory.CreateMessage();
			ffMessage.Context = MessageFactory.CreateMessageContext();
			ffMessage.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			ffMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.FSU1.xml");
			ffMessage.Context.WriteProperty<BTS.SourceParty>("Sender1");

			var component = MockRepository.PartialMock<AirMessageDisassembleComponent>();

			var logger = MockRepository.GeneratePartialMock<ConsoleOutLogger>("Test", LogLevel.Trace, true, false, false, "s");
			component.Stub(x => x.GetPipelineLogger(Arg<IBaseMessage>.Is.Anything)).Return(logger);

			var inboxAccessor = MockRepository.Stub<IInboxAccessor>();
			inboxAccessor.Expect(x => x.InsertToInbox(null, Guid.Empty, Guid.Empty, MessageStatus.Processing, null)).IgnoreArguments().Repeat.Once().Throw(new Exception("Exception1"));
			component.Expect(x => x.GetInboxAccessor()).Return(inboxAccessor).Repeat.Once();

			var exceptionsAccessor = MockRepository.Stub<IExceptionsAccessor>();
			exceptionsAccessor.Expect(x => x.SubmitErrorAndUpdateStatus(
				Arg<Guid>.Is.Anything,
				Arg<string>.Is.Equal("BIZ"),
				Arg<string>.Is.Equal("Failure"),
				Arg<string>.Matches(description => description.Contains("\r\n----------BIZTALK PROPERTIES----------\r\n")),
				Arg<Guid>.Is.Anything,
				Arg<Guid>.Is.Anything,
				Arg<Guid>.Is.Anything,
				Arg<Guid>.Is.Anything,
				Arg<SqlConnection>.Is.Anything,
				Arg<bool>.Is.Equal(false))).Repeat.Once().Throw(new Exception("Exception2"));
			component.Expect(x => x.GetExceptionsAccessor()).Return(exceptionsAccessor).Repeat.Once();

			MockRepository.ReplayAll();

			component.Enabled = true;

			AssertException(() => component.Disassemble(pipelineContext, ffMessage), typeof(AggregateException), "Errors occurred when processing Sender1's message.");
			logger.AssertWasCalled(x => x.ErrorFormat(Arg<string>.Is.Equal("Report details:\r\n{0}"), Arg<object[]>.Matches(parameters => 
				parameters[0].ToString().Contains("<Message>Errors occurred when processing Sender1's message.</Message>") && parameters[0].ToString().Contains("<Message>Exception1</Message>") && parameters[0].ToString().Contains("<Message>Exception2</Message>"))));

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void MessageDisassemble_Debatch()
		{
			ConfigurationManager.AppSettings["IssueManagerUri"] = "";
			var mockAirMessageDisassembler = MockRepository.GeneratePartialMock<AirMessageDisassembleComponent>();
			var mockFFDisassembler = MockRepository.GenerateMock<IFFDisassembleHelper>();
			var pipelineContext = new PipelineContext();
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("body", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("CCSJ.TestFiles.Text.CCSJ_FMA_FNA_batch.txt");
			var initialTrackingID = Guid.NewGuid();
			var envelopeMessage = MessageFactory.CreateMessage();
			envelopeMessage.Context = MessageFactory.CreateMessageContext();
			envelopeMessage.AddPart("body", MessageFactory.CreateMessagePart(), true);
			envelopeMessage.BodyPart.Data = GetEmbeddedResource("CCSJ.TestFiles.Text.CCSJ_FMA_FNA_batch.xml");
			envelopeMessage.Context.WriteProperty<MessageTrackingID>(initialTrackingID);
			mockAirMessageDisassembler.Enabled = true;
			mockAirMessageDisassembler.ServiceProviderID = "CCSJ";
			mockAirMessageDisassembler.EnvelopeMessageSchema = new Schema("CCSJReplyMessage");
			mockAirMessageDisassembler.FMAFNAMessageSchema = new Schema("FMA_FNA");
			mockAirMessageDisassembler.CMDCMAMessageSchema = new Schema("CMD_CMA");
			mockAirMessageDisassembler.FSUMessageSchema = new Schema("FSU");
			mockAirMessageDisassembler.BatchedMessageXpath = "/*[local-name()='CCSJBatch']/*[local-name()='CCSJ']";
			mockAirMessageDisassembler.Stub(x => x.CreateEnvelopContext(null)).IgnoreArguments().Do(new Func<IBaseMessage, EnvelopContext>(m =>
			{
				var xnav = new XPathDocument(new XmlTextReader(m.BodyPart.Data)).CreateNavigator();
				var envContext = MockRepository.GenerateMock<EnvelopContext>();
				envContext.SenderID = xnav.SelectSingleNode("/*[local-name()='CCSJ']/Header/SenderAirline").Value;
				envContext.RecipientID = xnav.SelectSingleNode("/*[local-name()='CCSJ']/Header/Recipient").Value;
				envContext.InternalMessage = xnav.SelectSingleNode("/*[local-name()='CCSJ']/Data").Value.TrimEnd();
				envContext.MessageType = envContext.InternalMessage.Remove(3);
				envContext.ClientAWB = ConextHelper.BuildAWBFromMessage(envContext.InternalMessage);
				return envContext;
			}));
			var partyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			partyAccessor.Stub(x => x.GetAirlineCodeFromPrefix(Arg<string>.Is.Anything)).Return("CX");

			mockAirMessageDisassembler.Stub(x => x.GetPartyAccessor()).Return(partyAccessor);
			mockAirMessageDisassembler.Stub(x => x.GetFFDisassembler()).Return(mockFFDisassembler);
			mockFFDisassembler.Stub(x => x.Disassemble(Arg.Is(pipelineContext), Arg.Is(message), Arg<SchemaWithNone>.Matches(y => y.SchemaName == "CCSJReplyMessage"))).Return(envelopeMessage).Repeat.Once();
			mockFFDisassembler.Stub(x => x.Disassemble(Arg.Is(pipelineContext), Arg<IBaseMessage>.Is.Anything, Arg<SchemaWithNone>.Is.Anything)).Do(new Func<IPipelineContext, IBaseMessage, SchemaWithNone, IBaseMessage>((p, m, s) => { return m; }));
			mockAirMessageDisassembler.Stub(x => x.InsertSubscriptionValue(null, null, null, null, null, null)).IgnoreArguments().Repeat.Times(7);
			mockAirMessageDisassembler.Stub(x => x.InsertToInbox(null, null, Guid.Empty)).IgnoreArguments().Repeat.Times(7);

			mockAirMessageDisassembler.Disassemble(pipelineContext, message);

			var actualMessages = new List<Tuple<string,string,string,string>>();
			var trackingIDs = new List<Guid>();
			IBaseMessage dequeuedMessage = mockAirMessageDisassembler.GetNext(pipelineContext);
			while (dequeuedMessage != null)
			{
				using (var sr = new StreamReader(dequeuedMessage.BodyPart.Data))
					actualMessages.Add(new Tuple<string,string,string,string>(
						dequeuedMessage.Context.ReadPropertyString<BTS.SourceParty>(),
						dequeuedMessage.Context.ReadPropertyString<BTS.DestinationParty>(),
						dequeuedMessage.Context.ReadPropertyString<BTS.MessageType>(),
						sr.ReadToEnd()
					));
				trackingIDs.Add(new Guid(dequeuedMessage.Context.ReadPropertyString<MessageTrackingID>()));
				dequeuedMessage = mockAirMessageDisassembler.GetNext(pipelineContext);
			}

			Assert.AreEqual(initialTrackingID, trackingIDs[0]);
			CollectionAssert.AllItemsAreUnique(trackingIDs);
			CollectionAssert.AreEqual(new List<Tuple<string, string, string, string>>() 
			{
				new Tuple<string,string,string,string>("KE", "1630540TYO82", "http://cargowise.com/ehub/clients/edi/2011/09#FMA_FNA", "FMA\r\nACK/FWB RCVD 16JUN 0005\r\nFWB/16\r\n180-67148502FUKSIN/T14K115"),
				new Tuple<string,string,string,string>("MSB", "1630540OSA82", "http://cargowise.com/ehub/clients/edi/2011/09#FMA_FNA", "FNA\r\nACK/DESTINATION NOT AUTHORISED TO RECEIVE MESSAGE\r\nFWB/16\r\n217-97809983FUKBKK/T2K9.1\r\nFLT/TG0649/16\r\nRTG/BKKTG\r\nSHP\r\n/YUSEN LOGISTICS CO.  LTD. FUKUOKA C\r\n/FUKUOKA AIRPORT INT L CARGO BLDG. 5\r\n/FUKUOKA/FUKUOKA\r\n/JP/812-0005/TE/81924777810\r\nCNE\r\n/YUSEN LOGISTICS  THAILAND  CO. LTD.\r\n/G 11 12 14 15 20TH FL OCEAN INSURAN\r\n/BANGKOK 10500/THAILAND\r\n/TH//TE/662134775559\r\nAGT//1630540/0974\r\n/YUSEN LOGISTICS CO. LTD.\r\n/FUKUOKA  JAPAN\r\nCVD/JPY/PP/PP/NVD/NCV/XXX\r\nRTD/1/P2/K9.1/CQ/W45/R1100/T49500\r\n/NG/CONSOLIDATION AS PER\r\n/2/NV/MC0.12\r\n/3/NS/2\r\nOTH/P/MYC2835\r\nPPD/WT49500\r\n/OC2835/CT52335\r\nCER/YUSEN LOGISTICS CO. \r\nISU/16JUN14/FUKUOKA\r\nREF//C01698280/FFW/CWIDYASYJCPRD/FUK"),
				new Tuple<string,string,string,string>("RHK", "1630540TYO82", "http://cargowise.com/ehub/clients/edi/2011/09#FMA_FNA", "FNA\r\nACK/REPLY TO CIMFWB VER 16. MSG DESTINED TO RHKAIR01TPEFMCI\r\n/RECEPIENT NOT EXPECTED TO RECEIVING THIS KIND OF MESSAGE.\r\nFWB/16\r\n083-98371000NGOBLZ/T3K39.8"),
				new Tuple<string,string,string,string>("KZ", "1630540OSA82", "http://cargowise.com/ehub/clients/edi/2011/09#FMA_FNA", "FMA\r\nACK/FWB RCVD 16 JUN 2014 0242\r\nFWB/16\r\n933-70097775NRTSIN/T1K8.7"),
				new Tuple<string,string,string,string>("SQ", "1630540TYO82", "http://cargowise.com/ehub/clients/edi/2011/09#FMA_FNA", "FMA\r\nACK/FHL RECEIVED 16JUN2014 1042HRS\r\n/HBS YJP01604411 NRTSUB 3 K30.9  TERMINAL\r\nFHL/4\r\nMBI/618-79337053NRTSUB/T3K30.9"),
				new Tuple<string,string,string,string>("FX", "1630540OSA82", "http://cargowise.com/ehub/clients/edi/2011/09#FMA_FNA", "FNA\r\nACK/HAB NUMBER ALREADY EXISTS - YJP00966980 \r\nFHL/4\r\nMBI/023-78476370NRTMNL/T3K50.8\r\nHBS/YJP00966980/NRTMNL/3/K50.8//HARDENER\r\nTXT/HARDENER  COUNTRY OF ORIGIN  JAPAN\r\nOCI/JP/DNR/D/1263C\r\nSHP/ELEMATEC CORPORATION\r\n/SUMITOMO FUDOSAN MITA TWIN BLD WEST\r\n/MINATO-KU/TOKYO\r\n/JP/108-6325/TE/81334549560\r\nCNE/TOMS MANUFACTURING CORPORATION\r\n/BLOCK 1  LOT 2  DAIICHI INDUSTRIAL \r\n/SILANG/CAVITE\r\n/PH/4118/TE/63464303550\r\nCVD/JPY/CP/NVD/NCV/XXX"),
				new Tuple<string,string,string,string>("FX", "1630540TYO82", "http://cargowise.com/ehub/clients/edi/2011/09#FMA_FNA", "FNA\r\nACK/CNE - INVALID ADDRESS\r\nFWB/14\r\n023-78476370NRTMNL/T3K50.8\r\nFLT/FX5153/14/FX5771/16\r\nRTG/CANFX/MNLFX\r\nSHP\r\n/YUSEN LOGISTICS CO.  LTD. EAST JP A\r\n/1340-49  OYADO  IWAYAMA  SHIBAYAMA-\r\n/SANBU-GUN/CHIBA\r\n/JP/289-1608/TE/0479709400\r\nCNE\r\n/YUSEN LOGISTICS PHILIPPINES  INC.\r\n/  4 P. MAYUGA STREET  MIA ROAD  TAM\r\n/PHILIPPINES/NCR\r\n/PH//TE/6327840888\r\nAGT//1630540/0996\r\n/YUSEN LOGISTICS CO. LTD.\r\n/NARITA  JAPAN\r\nACC/GEN/IXF-1\r\nCVD/JPY/PP/PP/NVD/NCV/XXX\r\nRTD/1/P3/K50.8/CQ/W51/R700/T35700\r\n/NG/CONSOLIDATION AS PER\r\n/2/NG/VOL 0.156 M3\r\n/3/NS/3\r\nOTH/P/MYC3978\r\n/P/RAC7000\r\n/P/RAA3000\r\nPPD/WT35700\r\n/OA3000/OC10978/CT49678\r\nCER/YUSEN LOGISTICS CO. \r\nISU/14JUN14/NARITA APT TOKYO\r\nOSI/DANGEROUS GOODS AS PER ATTACHED SHIPPER S DECLARATION- CARGO AIR\r\n/CRAFT ONLY\r\nREF//C01713155/FFW/CWIDYASYJCPRD/NRT\r\nOCI/JP/DNR/D/1263C")
			}, actualMessages);
			mockAirMessageDisassembler.AssertWasCalled(x => x.InsertSubscriptionValue(Arg.Is("AIRAWB"), Arg.Is("KE"), Arg.Is("1630540TYO82"), Arg.Text.StartsWith("18067148502"), Arg<string>.Is.Anything, Arg<string>.Is.Anything));
			mockAirMessageDisassembler.AssertWasCalled(x => x.InsertSubscriptionValue(Arg.Is("AIRAWB"), Arg.Is("MSB"), Arg.Is("1630540OSA82"), Arg.Text.StartsWith("21797809983"), Arg<string>.Is.Anything, Arg<string>.Is.Anything));
			mockAirMessageDisassembler.AssertWasCalled(x => x.InsertSubscriptionValue(Arg.Is("AIRAWB"), Arg.Is("RHK"), Arg.Is("1630540TYO82"), Arg.Text.StartsWith("08398371000"), Arg<string>.Is.Anything, Arg<string>.Is.Anything));
			mockAirMessageDisassembler.AssertWasCalled(x => x.InsertSubscriptionValue(Arg.Is("AIRAWB"), Arg.Is("KZ"), Arg.Is("1630540OSA82"), Arg.Text.StartsWith("93370097775"), Arg<string>.Is.Anything, Arg<string>.Is.Anything));
			mockAirMessageDisassembler.AssertWasCalled(x => x.InsertSubscriptionValue(Arg.Is("AIRAWB"), Arg.Is("SQ"), Arg.Is("1630540TYO82"), Arg.Text.StartsWith("61879337053"), Arg<string>.Is.Anything, Arg<string>.Is.Anything));
			mockAirMessageDisassembler.AssertWasCalled(x => x.InsertSubscriptionValue(Arg.Is("AIRAWB"), Arg.Is("FX"), Arg.Is("1630540OSA82"), Arg.Text.StartsWith("02378476370"), Arg<string>.Is.Anything, Arg<string>.Is.Anything));
			mockAirMessageDisassembler.AssertWasCalled(x => x.InsertSubscriptionValue(Arg.Is("AIRAWB"), Arg.Is("FX"), Arg.Is("1630540TYO82"), Arg.Text.StartsWith("02378476370"), Arg<string>.Is.Anything, Arg<string>.Is.Anything));

		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void MessageDisassemble_NormalizeLineEndings()
		{
			ConfigurationManager.AppSettings["IssueManagerUri"] = "";
			var mockAirMessageDisassembler = MockRepository.GeneratePartialMock<AirMessageDisassembleComponent>();
			var mockFFDisassembler = MockRepository.GenerateMock<IFFDisassembleHelper>();
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("body", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("Delta.TestFiles.DeltaFSU.txt");

			mockAirMessageDisassembler.Enabled = true;
			mockAirMessageDisassembler.ServiceProviderID = "Delta";
			mockAirMessageDisassembler.EnvelopeMessageSchema = new Schema("DeltaReplyMessage");
			mockAirMessageDisassembler.FMAFNAMessageSchema = new Schema("FMA_FNA");
			mockAirMessageDisassembler.CMDCMAMessageSchema = new Schema("CMD_CMA");
			mockAirMessageDisassembler.FSUMessageSchema = new Schema("FSU");
			mockAirMessageDisassembler.NormalizeLineEndings = true;

			var partyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			partyAccessor.Stub(x => x.GetAirlineCodeFromPrefix(Arg<string>.Is.Anything)).Return("CX");

			mockAirMessageDisassembler.Stub(x => x.GetPartyAccessor()).Return(partyAccessor);
			mockAirMessageDisassembler.Stub(x => x.GetFFDisassembler()).Return(mockFFDisassembler);
			mockFFDisassembler.Stub(x => x.Disassemble(Arg.Is(pipelineContext), 
					Arg<IBaseMessage>.Matches(m => m == message && m.BodyPart.GetOriginalDataStream() is NormalizedLineEndingsStream), 
					Arg<SchemaWithNone>.Matches(y => y.SchemaName == "DeltaReplyMessage")))
				.Do(new Func<IPipelineContext, IBaseMessage, SchemaWithNone, IBaseMessage>((p, m, s) =>
				{
					var envelopeMessage = MessageFactory.CreateMessage();
					envelopeMessage.AddPart("body", MessageFactory.CreateMessagePart(), true);
					envelopeMessage.BodyPart.Data = SchemaTester<DeltaReplyMessage>.ParseFF(m.BodyPart.GetOriginalDataStream());
					return envelopeMessage;
				})).Repeat.Once();
			mockAirMessageDisassembler.Stub(x => x.CreateEnvelopContext(Arg<IBaseMessage>.Is.Anything))
				.Do(new Func<IBaseMessage, EnvelopContext>(m =>
				{
					var envContext = MockRepository.GenerateMock<EnvelopContext>();
					var promotedValue = new DeltaPromotedValue();
					envContext.InternalMessage = promotedValue.Find(m, "InternalMessage");
					envContext.SenderPIMA = promotedValue.Find(m, "SenderPIMA");
					envContext.RecipientPIMA = promotedValue.Find(m, "RecipientPIMA");
					envContext.SenderID = "SenderID";
					envContext.RecipientID = "RecipientID";
					envContext.ExtractMessageTypeAndVersion();
					return envContext;
				}));
			mockFFDisassembler.Stub(x => x.Disassemble(Arg.Is(pipelineContext), Arg<IBaseMessage>.Is.NotSame(message), Arg<SchemaWithNone>.Is.Anything))
				.Do(new Func<IPipelineContext, IBaseMessage, SchemaWithNone, IBaseMessage>((p, m, s) =>
				{
					var internalMessage = MessageFactory.CreateMessage();
					internalMessage.AddPart("body", MessageFactory.CreateMessagePart(), true);
					internalMessage.BodyPart.Data = SchemaTester<FSU_FSA12>.ParseFF(m.BodyPart.GetOriginalDataStream());
					return internalMessage;
				}));
			mockAirMessageDisassembler.Stub(x => x.InsertToInbox(Arg.Is(message), Arg<string>.Is.Null, Arg<Guid>.Is.Anything));

			mockAirMessageDisassembler.Disassemble(pipelineContext, message);

			var dequeuedMessage = mockAirMessageDisassembler.GetNext(pipelineContext);

			Assert.IsNotNull(dequeuedMessage);
			Assert.AreEqual("http://cargowise.com/ehub/clients/edi/2010/12#FSU_FSA12", dequeuedMessage.Context.ReadPropertyString<BTS.MessageType>());
			Assert.IsTrue(XNode.DeepEquals(XElement.Load(dequeuedMessage.BodyPart.GetOriginalDataStream()),
				XElement.Load(GetEmbeddedResource("Delta.TestFiles.DeltaFSUInternal.xml"))));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void GetInternalMessageText()
		{
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream();

			string text = "internal message text";
			var internalMessage = AirMessageDisassembleComponent.GetInternalMessage(pipelineContext, message, text);
			Assert.AreEqual(text, internalMessage.BodyPart.Data.ReadToEnd());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void MessageDisassembleDecartesFSA()
		{
			ConfigurationManager.AppSettings["IssueManagerUri"] = "";
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("text", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.FSA.txt");

			var ffMessage = MessageFactory.CreateMessage();
			ffMessage.Context = MessageFactory.CreateMessageContext();
			ffMessage.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			ffMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.FSA_internal.xml");

			var component = MockRepository.PartialMock<AirMessageDisassembleComponent>();

			var context = new DescartesEnvelopContext();
			context.SenderID = "Sender1";
			context.RecipientID = "Recipient1";
			context.MessageType = "FSA";
			context.InternalMessage = @"FSA/6
105-40085161CPHPVG/T9K215.6
BKD/AY9096/05AUG/CPHHEL/T9K215.6/S1900/S2130-S";
			component.Expect(x => x.CreateEnvelopContext(null)).IgnoreArguments().Return(context);

			component.Expect(x => x.DisassembleInternalMessage(null, null, null, null, new Schema(""))).IgnoreArguments();
			component.Expect(x => x.InsertToInbox(null, null, Guid.Empty)).IgnoreArguments();

			MockRepository.ReplayAll();

			component.Enabled = true;
			component.ServiceProviderID = "Descartes";
			component.EnvelopeMessageSchema = new Schema("");
			component.FSUMessageSchema = new Schema("CargoWise.eHub.Clients.EDI.Schemas.CargoIMP.FSU_FSA12, CargoWise.eHub.Clients.EDI.Schemas.CargoImp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350");
			component.FMAFNAMessageSchema = new Schema("CargoWise.eHub.Clients.EDI.Schemas.CargoIMP.FMA_FNA, CargoWise.eHub.Clients.EDI.Schemas.CargoImp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350");
			component.CMDCMAMessageSchema = new Schema("CargoWise.eHub.Clients.EDI.Schemas.CargoIMP.CMD_CMA, CargoWise.eHub.Clients.EDI.Schemas.CargoImp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350");

			message.Context.WriteProperty<BTS.DestinationParty>("Recipient1");
			message.Context.WriteProperty<BTS.SourceParty>("Source1");

			component.Disassemble(pipelineContext, message);

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DisassembleInternalMessageFSU()
		{
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream();

			var messageFSUInternal = MessageFactory.CreateMessage();
			messageFSUInternal.Context = MessageFactory.CreateMessageContext();
			messageFSUInternal.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			messageFSUInternal.BodyPart.Data = GetEmbeddedResource("TestFiles.FSU_Internal.xml");

			var ffDisassembler = MockRepository.StrictMock<IFFDisassembleHelper>();
			ffDisassembler.Expect(x => x.Disassemble(pipelineContext, message, new SchemaWithNone("FSUName, namespace"))).Return(messageFSUInternal);

			var partyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			partyAccessor.Stub(x => x.GetAirlineCodeFromPrefix(Arg<string>.Is.Anything)).Return("CX");

			var component = MockRepository.PartialMock<AirMessageDisassembleComponent>();
			component.Expect(x => x.GetFFDisassembler()).Return(ffDisassembler).Repeat.Any();
			component.Expect(x => x.GetPartyAccessor()).Return(partyAccessor).Repeat.Any();
			var context = MockRepository.PartialMock<BTEnvelopContext>();

			MockRepository.ReplayAll();

			context.SenderID = "Sender1";
			context.RecipientID = "Recipient1";
			context.MessageType = "FSU";

			component.DisassembleInternalMessage(pipelineContext, context, message, message, new SchemaWithNone("FSUName, namespace"));

			Assert.AreEqual("Sender1", messageFSUInternal.Context.ReadPropertyString<BTS.SourceParty>());
			Assert.AreEqual("Recipient1", messageFSUInternal.Context.ReadPropertyString<BTS.DestinationParty>());
			Assert.AreEqual("http://cargowise.com/ehub/clients/edi/2010/12#FSU_FSA12", messageFSUInternal.Context.ReadPropertyString<BTS.MessageType>());

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DisassembleInternalMessageFSA()
		{
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream();

			var messageFSAInternal = MessageFactory.CreateMessage();
			messageFSAInternal.Context = MessageFactory.CreateMessageContext();
			messageFSAInternal.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			messageFSAInternal.BodyPart.Data = GetEmbeddedResource("TestFiles.FSA_internal.xml");

			var ffDisassembler = MockRepository.StrictMock<IFFDisassembleHelper>();
			ffDisassembler.Expect(x => x.Disassemble(pipelineContext, message, new SchemaWithNone("FSAName, namespace"))).Return(messageFSAInternal);

			var partyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			partyAccessor.Stub(x => x.GetAirlineCodeFromPrefix(Arg<string>.Is.Anything)).Return("CX");

			var component = MockRepository.PartialMock<AirMessageDisassembleComponent>();
			component.Expect(x => x.GetFFDisassembler()).Return(ffDisassembler).Repeat.Any();
			component.Expect(x => x.GetPartyAccessor()).Return(partyAccessor).Repeat.Any();
			var context = MockRepository.PartialMock<BTEnvelopContext>();

			MockRepository.ReplayAll();

			context.SenderID = "Sender1";
			context.RecipientID = "Recipient1";
			context.MessageType = "FSA";

			component.DisassembleInternalMessage(pipelineContext, context, message, message, new SchemaWithNone("FSAName, namespace"));

			Assert.AreEqual("Sender1", messageFSAInternal.Context.ReadPropertyString<BTS.SourceParty>());
			Assert.AreEqual("Recipient1", messageFSAInternal.Context.ReadPropertyString<BTS.DestinationParty>());
			Assert.AreEqual("http://cargowise.com/ehub/clients/edi/2010/12#FSU_FSA12", messageFSAInternal.Context.ReadPropertyString<BTS.MessageType>());

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DisassembleInternalMessageFMA()
		{
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream();

			var messageFMAFNAInternal = MessageFactory.CreateMessage();
			messageFMAFNAInternal.Context = MessageFactory.CreateMessageContext();
			messageFMAFNAInternal.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			messageFMAFNAInternal.BodyPart.Data = GetEmbeddedResource("TestFiles.FMA_FNA_internal.xml");

			var ffDisassembler = MockRepository.StrictMock<IFFDisassembleHelper>();
			ffDisassembler.Expect(x => x.Disassemble(pipelineContext, message, new SchemaWithNone("FMAFNAName, namespace"))).Return(messageFMAFNAInternal);

			var partyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			partyAccessor.Stub(x => x.GetAirlineCodeFromPrefix(Arg<string>.Is.Anything)).Return("CX");

			var component = MockRepository.PartialMock<AirMessageDisassembleComponent>();
			component.Expect(x => x.GetFFDisassembler()).Return(ffDisassembler).Repeat.Any();
			component.Expect(x => x.GetPartyAccessor()).Return(partyAccessor).Repeat.Any();
			var context = MockRepository.PartialMock<BTEnvelopContext>();

			MockRepository.ReplayAll();

			context.SenderID = "Sender1";
			context.RecipientID = "Recipient1";
			context.MessageType = "FMA";

			component.DisassembleInternalMessage(pipelineContext, context, message, message, new SchemaWithNone("FMAFNAName, namespace"));

			Assert.AreEqual("Sender1", messageFMAFNAInternal.Context.ReadPropertyString<BTS.SourceParty>());
			Assert.AreEqual("Recipient1", messageFMAFNAInternal.Context.ReadPropertyString<BTS.DestinationParty>());
			Assert.AreEqual("http://cargowise.com/ehub/clients/edi/2011/09#FMA_FNA", messageFMAFNAInternal.Context.ReadPropertyString<BTS.MessageType>());

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DisassembleInternalMessageFNA()
		{
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream();

			var messageFMAFNAInternal = MessageFactory.CreateMessage();
			messageFMAFNAInternal.Context = MessageFactory.CreateMessageContext();
			messageFMAFNAInternal.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			messageFMAFNAInternal.BodyPart.Data = GetEmbeddedResource("TestFiles.FMA_FNA_internal.xml");

			var ffDisassembler = MockRepository.StrictMock<IFFDisassembleHelper>();
			ffDisassembler.Expect(x => x.Disassemble(pipelineContext, message, new SchemaWithNone("FMAFNAName, namespace"))).Return(messageFMAFNAInternal);

			var partyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			partyAccessor.Stub(x => x.GetAirlineCodeFromPrefix(Arg<string>.Is.Anything)).Return("CX");

			var component = MockRepository.PartialMock<AirMessageDisassembleComponent>();
			component.Expect(x => x.GetFFDisassembler()).Return(ffDisassembler).Repeat.Any();
			component.Expect(x => x.GetPartyAccessor()).Return(partyAccessor).Repeat.Any();
			var context = MockRepository.PartialMock<BTEnvelopContext>();

			MockRepository.ReplayAll();

			context.SenderID = "Sender1";
			context.RecipientID = "Recipient1";
			context.MessageType = "FNA";

			component.DisassembleInternalMessage(pipelineContext, context, message, message, new SchemaWithNone("FMAFNAName, namespace"));

			Assert.AreEqual("Sender1", messageFMAFNAInternal.Context.ReadPropertyString<BTS.SourceParty>());
			Assert.AreEqual("Recipient1", messageFMAFNAInternal.Context.ReadPropertyString<BTS.DestinationParty>());
			Assert.AreEqual("http://cargowise.com/ehub/clients/edi/2011/09#FMA_FNA", messageFMAFNAInternal.Context.ReadPropertyString<BTS.MessageType>());

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DisassembleInternalMessageCMDFNA()
		{
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream();

			var messageCMDFNAInternal = MessageFactory.CreateMessage();
			messageCMDFNAInternal.Context = MessageFactory.CreateMessageContext();
			messageCMDFNAInternal.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			messageCMDFNAInternal.BodyPart.Data = GetEmbeddedResource("TestFiles.CMD_FNA_internal.xml");

			var ffDisassembler = MockRepository.StrictMock<IFFDisassembleHelper>();
			ffDisassembler.Expect(x => x.Disassemble(pipelineContext, message, new SchemaWithNone("CMDFNAName, namespace"))).Return(messageCMDFNAInternal);
			var partyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			partyAccessor.Stub(x => x.GetAirlineCodeFromPrefix(Arg<string>.Is.Anything)).Return("CX");

			var component = MockRepository.PartialMock<AirMessageDisassembleComponent>();
			component.Expect(x => x.GetFFDisassembler()).Return(ffDisassembler).Repeat.Any();
			component.Expect(x => x.GetPartyAccessor()).Return(partyAccessor).Repeat.Any();
			var context = MockRepository.PartialMock<BTEnvelopContext>();

			MockRepository.ReplayAll();

			context.SenderID = "Sender1";
			context.RecipientID = "Recipient1";
			context.MessageType = "FNA";
			context.OriginalMessageType = "CMD";

			component.DisassembleInternalMessage(pipelineContext, context, message, message, new SchemaWithNone("CMDFNAName, namespace"));

			Assert.AreEqual("Sender1", messageCMDFNAInternal.Context.ReadPropertyString<BTS.SourceParty>());
			Assert.AreEqual("Recipient1", messageCMDFNAInternal.Context.ReadPropertyString<BTS.DestinationParty>());
			Assert.AreEqual("http://cargowise.com/ehub/clients/edi/2011/09#CMD_FMA_FNA", messageCMDFNAInternal.Context.ReadPropertyString<BTS.MessageType>());

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DisassembleInternalMessageCMA()
		{
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = new MemoryStream();

			var messageCMAInternal = MessageFactory.CreateMessage();
			messageCMAInternal.Context = MessageFactory.CreateMessageContext();
			messageCMAInternal.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			messageCMAInternal.BodyPart.Data = GetEmbeddedResource("TestFiles.CMA_Internal.xml");

			var ffDisassembler = MockRepository.StrictMock<IFFDisassembleHelper>();
			ffDisassembler.Expect(x => x.Disassemble(pipelineContext, message, new SchemaWithNone("CMAName, namespace"))).Return(messageCMAInternal);

			var partyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			partyAccessor.Stub(x => x.GetAirlineCodeFromPrefix(Arg<string>.Is.Anything)).Return("CX");

			var component = MockRepository.PartialMock<AirMessageDisassembleComponent>();
			component.Expect(x => x.GetFFDisassembler()).Return(ffDisassembler).Repeat.Any();
			component.Expect(x => x.GetPartyAccessor()).Return(partyAccessor).Repeat.Any();
			var context = MockRepository.PartialMock<BTEnvelopContext>();

			MockRepository.ReplayAll();

			context.SenderID = "Sender1";
			context.RecipientID = "Recipient1";
			context.MessageType = "CMA";

			component.DisassembleInternalMessage(pipelineContext, context, message, message, new SchemaWithNone("CMAName, namespace"));

			Assert.AreEqual("Sender1", messageCMAInternal.Context.ReadPropertyString<BTS.SourceParty>());
			Assert.AreEqual("Recipient1", messageCMAInternal.Context.ReadPropertyString<BTS.DestinationParty>());
			Assert.AreEqual("http://cargowise.com/ehub/clients/edi/2011/09#CMD_CMA", messageCMAInternal.Context.ReadPropertyString<BTS.MessageType>());

			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DisassembleInternalMessageSubscription()
		{
			var pipelineContext = new PipelineContext();

			var envelopeMessage = MessageFactory.CreateMessage();
			envelopeMessage.Context = MessageFactory.CreateMessageContext();
			envelopeMessage.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			envelopeMessage.BodyPart.Data = new MemoryStream();

			var internalMessage = MessageFactory.CreateMessage();
			internalMessage.Context = MessageFactory.CreateMessageContext();
			internalMessage.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			internalMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.FSA_internal.xml");

			var ffDisassembler = MockRepository.GenerateMock<IFFDisassembleHelper>();
			ffDisassembler.Stub(x => x.Disassemble(pipelineContext, envelopeMessage, new SchemaWithNone("FSAName, namespace"))).Return(internalMessage);

			var subscriptionAccessor = MockRepository.GenerateMock<ISubscriptionAccessor>();
			subscriptionAccessor.Stub(x => x.SelectSubscriptions(internalMessage)).Return(new[] { new SubscriptionInfo() {
				ID = Guid.NewGuid(),
				Type = "",
				Provider = "",
				Subscriber = "SubscriptionRecipient",
				Value = "", 
				Reference = "",
				ReferenceType = "",
				Subscribed = DateTime.Now,
				Expiry = DateTime.Now
			} });

			var partyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			var component = MockRepository.GeneratePartialMock<AirMessageDisassembleComponent>();
			component.ProcessSubscriptions = true;
			component.Stub(x => x.GetFFDisassembler()).Return(ffDisassembler);
			component.Stub(x => x.GetSubsriptionAccessor()).Return(subscriptionAccessor);
			component.Stub(x => x.GetPartyAccessor()).Return(partyAccessor);

			var context = MockRepository.PartialMock<BTEnvelopContext>();
			context.SenderID = "Sender1";
			context.RecipientID = "Recipient1";
			context.MessageType = "FSA";

			component.DisassembleInternalMessage(pipelineContext, context, envelopeMessage, envelopeMessage, new SchemaWithNone("FSAName, namespace"));
			var resultMsg1 = component.GetNext(pipelineContext);
			var resultMsg2 = component.GetNext(pipelineContext);
			var resultNull = component.GetNext(pipelineContext);

			Assert.IsNotNull(resultMsg1);
			Assert.IsNotNull(resultMsg2);
			Assert.IsNull(resultNull);

			Assert.AreSame(internalMessage, resultMsg1);
			Assert.AreNotSame(internalMessage, resultMsg2);

			Assert.AreEqual("Sender1", resultMsg1.Context.ReadPropertyString<BTS.SourceParty>());
			Assert.AreEqual("Recipient1", resultMsg1.Context.ReadPropertyString<BTS.DestinationParty>());

			Assert.AreEqual("Sender1", resultMsg2.Context.ReadPropertyString<BTS.SourceParty>());
			Assert.AreEqual("SubscriptionRecipient", resultMsg2.Context.ReadPropertyString<BTS.DestinationParty>());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DisassembleInternalMessageSubscriptionWithRecipientNotFound()
		{
			var pipelineContext = new PipelineContext();

			var envelopeMessage = MessageFactory.CreateMessage();
			envelopeMessage.Context = MessageFactory.CreateMessageContext();
			envelopeMessage.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			envelopeMessage.BodyPart.Data = new MemoryStream();

			var internalMessage = MessageFactory.CreateMessage();
			internalMessage.Context = MessageFactory.CreateMessageContext();
			internalMessage.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			internalMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.FSA_internal.xml");

			var ffDisassembler = MockRepository.GenerateMock<IFFDisassembleHelper>();
			ffDisassembler.Stub(x => x.Disassemble(pipelineContext, envelopeMessage, new SchemaWithNone("FSAName, namespace"))).Return(internalMessage);

			var subscriptionAccessor = MockRepository.GenerateMock<ISubscriptionAccessor>();
			subscriptionAccessor.Stub(x => x.SelectSubscriptions(internalMessage)).Return(new[] { new SubscriptionInfo() {
				ID = Guid.NewGuid(),
				Type = "",
				Provider = "",
				Subscriber = "SubscriptionRecipient",
				Value = "", 
				Reference = "",
				ReferenceType = "",
				Subscribed = DateTime.Now,
				Expiry = DateTime.Now
			} });

			var partyAccessor = MockRepository.GenerateMock<IPartyAccessor>();

			var component = MockRepository.GeneratePartialMock<AirMessageDisassembleComponent>();
			component.ProcessSubscriptions = true;
			component.Stub(x => x.GetFFDisassembler()).Return(ffDisassembler);
			component.Stub(x => x.GetSubsriptionAccessor()).Return(subscriptionAccessor);
			component.Stub(x => x.GetPartyAccessor()).Return(partyAccessor);

			var context = MockRepository.PartialMock<BTEnvelopContext>();
			context.SenderID = "Sender1";
			context.RecipientID = "";
			context.MessageType = "FSA";

			component.DisassembleInternalMessage(pipelineContext, context, envelopeMessage, envelopeMessage, new SchemaWithNone("FSAName, namespace"));
			var resultMsg1 = component.GetNext(pipelineContext);
			var resultMsg2 = component.GetNext(pipelineContext);
			var resultNull = component.GetNext(pipelineContext);

			Assert.IsNotNull(resultMsg1);
			Assert.IsNotNull(resultMsg2);
			Assert.IsNull(resultNull);

			Assert.AreNotSame(internalMessage, resultMsg1);
			Assert.AreNotSame(internalMessage, resultMsg2);

			Assert.AreEqual("Sender1", resultMsg1.Context.ReadPropertyString<BTS.SourceParty>());
			Assert.AreEqual("", resultMsg1.Context.ReadPropertyString<BTS.DestinationParty>());
			Assert.AreEqual("FailedMessage", resultMsg1.Context.ReadPropertyString<ErrorReport.ErrorType>());
			Assert.AreEqual("The message recipient can not be recognized.", resultMsg1.Context.ReadPropertyString<ErrorReport.Description>());
			Assert.AreEqual("AlertMessage", resultMsg1.Context.ReadPropertyString<BTS.MessageType>());

			Assert.AreEqual("Sender1", resultMsg2.Context.ReadPropertyString<BTS.SourceParty>());
			Assert.AreEqual("SubscriptionRecipient", resultMsg2.Context.ReadPropertyString<BTS.DestinationParty>());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DisassembleInternalMessageSubscriptionAddEnvelope()
		{
			var pipelineContext = new PipelineContext();

			var envelopeMessage = MessageFactory.CreateMessage();
			envelopeMessage.Context = MessageFactory.CreateMessageContext();
			envelopeMessage.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			envelopeMessage.BodyPart.Data = new MemoryStream();

			var internalMessage = MessageFactory.CreateMessage();
			internalMessage.Context = MessageFactory.CreateMessageContext();
			internalMessage.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			internalMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.FSA_internal.xml");

			var ffDisassembler = MockRepository.GenerateMock<IFFDisassembleHelper>();
			ffDisassembler.Stub(x => x.Disassemble(pipelineContext, envelopeMessage, new SchemaWithNone("FSAName, namespace"))).Return(internalMessage);

			var partyAccessor = MockRepository.GenerateMock<IPartyAccessor>();

			var subscriptionAccessor = MockRepository.GenerateMock<ISubscriptionAccessor>();
			subscriptionAccessor.Stub(x => x.SelectSubscriptions(internalMessage)).Return(new[] { 
				new SubscriptionInfo() {
					ID = Guid.NewGuid(),
					Type = "",
					Provider = "",
					Subscriber = "SubscriptionRecipient",
					Value = "", 
					Reference = @"<ns0:FMS xmlns:ns0=""http://CargoWise.eHub.Products.AirMessaging.Schemas.FMSEnvelope""><Header><SenderID>{0}</SenderID><RecipientID>{1}</RecipientID></Header><Body>",
					ReferenceType = "EnvelopePrepend",
					Subscribed = DateTime.Now,
					Expiry = DateTime.Now
				},
				new SubscriptionInfo() {
					ID = Guid.NewGuid(),
					Type = "",
					Provider = "",
					Subscriber = "SubscriptionRecipient",
					Value = "", 
					Reference = "</Body></ns0:FMS>",
					ReferenceType = "EnvelopeAppend",
					Subscribed = DateTime.Now,
					Expiry = DateTime.Now
				} 
			});

			var component = MockRepository.GeneratePartialMock<AirMessageDisassembleComponent>();
			component.ProcessSubscriptions = true;
			component.Stub(x => x.GetFFDisassembler()).Return(ffDisassembler);
			component.Stub(x => x.GetSubsriptionAccessor()).Return(subscriptionAccessor);
			component.Stub(x => x.GetPartyAccessor()).Return(partyAccessor);

			var context = MockRepository.PartialMock<BTEnvelopContext>();
			context.SenderID = "Sender1";
			context.RecipientID = "Recipient1";
			context.MessageType = "FSA";

			component.DisassembleInternalMessage(pipelineContext, context, envelopeMessage, envelopeMessage, new SchemaWithNone("FSAName, namespace"));
			var resultMsg1 = component.GetNext(pipelineContext);
			var resultMsg2 = component.GetNext(pipelineContext);
			var resultNull = component.GetNext(pipelineContext);

			Assert.IsNotNull(resultMsg1);
			Assert.IsNotNull(resultMsg2);
			Assert.IsNull(resultNull);

			Assert.AreSame(internalMessage, resultMsg1);
			Assert.AreNotSame(internalMessage, resultMsg2);

			Assert.AreEqual("Sender1", resultMsg1.Context.ReadPropertyString<BTS.SourceParty>());
			Assert.AreEqual("Recipient1", resultMsg1.Context.ReadPropertyString<BTS.DestinationParty>());
			Assert.AreEqual("http://cargowise.com/ehub/clients/edi/2010/12#FSU_FSA12", resultMsg1.Context.ReadPropertyString<BTS.MessageType>());

            Assert.AreEqual("Sender1", resultMsg2.Context.ReadPropertyString<BTS.SourceParty>());
            Assert.AreEqual("SubscriptionRecipient", resultMsg2.Context.ReadPropertyString<BTS.DestinationParty>());
			Assert.AreEqual("http://CargoWise.eHub.Products.AirMessaging.Schemas.FMSEnvelope#FMS", resultMsg2.Context.ReadPropertyString<BTS.MessageType>());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DisassembleInternalMessageWithRecipientFromSubscription()
		{
			var pipelineContext = new PipelineContext();

			var envelopeMessage = MessageFactory.CreateMessage();
			envelopeMessage.Context = MessageFactory.CreateMessageContext();
			envelopeMessage.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			envelopeMessage.BodyPart.Data = new MemoryStream();

			var internalMessage = MessageFactory.CreateMessage();
			internalMessage.Context = MessageFactory.CreateMessageContext();
			internalMessage.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			internalMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.FSA_internal.xml");

			var ffDisassembler = MockRepository.GenerateMock<IFFDisassembleHelper>();
			ffDisassembler.Stub(x => x.Disassemble(pipelineContext, envelopeMessage, new SchemaWithNone("FSAName, namespace"))).Return(internalMessage);

			var subscriptionAccessor = MockRepository.GenerateMock<ISubscriptionAccessor>();
			subscriptionAccessor.Stub(x => x.SelectSubscriptions(internalMessage)).Return(Array.Empty<SubscriptionInfo>());

			var component = MockRepository.GeneratePartialMock<AirMessageDisassembleComponent>();
			component.ProcessSubscriptions = true;
			component.Stub(x => x.GetFFDisassembler()).Return(ffDisassembler);
			component.Stub(x => x.GetSubsriptionAccessor()).Return(subscriptionAccessor);

			var context = MockRepository.PartialMock<BTEnvelopContext>();
			context.SenderID = "Sender1";
			context.RecipientID = "AIR_CARGO_MESSAGING";
			context.MessageType = "FSA";

			component.DisassembleInternalMessage(pipelineContext, context, envelopeMessage, envelopeMessage, new SchemaWithNone("FSAName, namespace"));
			var resultMsg1 = component.GetNext(pipelineContext);
			var resultNull = component.GetNext(pipelineContext);

			Assert.IsNotNull(resultMsg1);
			Assert.IsNull(resultNull);

			Assert.AreEqual("Sender1", resultMsg1.Context.ReadPropertyString<BTS.SourceParty>());
			Assert.AreEqual("AIR_CARGO_MESSAGING", resultMsg1.Context.ReadPropertyString<BTS.DestinationParty>());
			Assert.AreEqual("http://CargoWise.eHub.Products.AirMessaging.Schemas.AirCargoMessaging#AirCargoMessage", resultMsg1.Context.ReadPropertyString<BTS.MessageType>());
			var something = resultMsg1.BodyPart.Data.ReadToEnd();
			Assert.AreEqual("<ns0:AirCargoMessage xmlns:ns0=\"http://CargoWise.eHub.Products.AirMessaging.Schemas.AirCargoMessaging\"><Header><SenderID>Sender1</SenderID><RecipientID>AIR_CARGO_MESSAGING</RecipientID></Header><Body></Body></ns0:AirCargoMessage>", resultMsg1.BodyPart.Data.ReadToEnd());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DisassembleInternalMessage_ReplaceRecipient()
		{
			DisassembleInternalMessage_ReplaceRecipient_TestCase("CW1AAATST", false, null, true, true);					// Default process to send to CW1 and subscription
			DisassembleInternalMessage_ReplaceRecipient_TestCase("CW1AAATST", true, null, false, true);					// Send everything to subscription only
			DisassembleInternalMessage_ReplaceRecipient_TestCase("CW1AAATST", false, "CW1AAATST", false, true);			// Send to subscription only just for specified recipient
			DisassembleInternalMessage_ReplaceRecipient_TestCase("CW1AAATST", true, "CW1AAATST", true, true);			// Send to subscriprion only except for specified recipient
			DisassembleInternalMessage_ReplaceRecipient_TestCase("CW1AAATST", true, "CW1OTHER,CW1*TST", true, true);	//   - with overrides for multiple IDs and wildcards
			DisassembleInternalMessage_ReplaceRecipient_TestCase("CW1AAATST", true, "CW1OTHER", false, true);			//   - with non-matching override
			DisassembleInternalMessage_ReplaceRecipient_TestCase(null, false, null, true, true);						// Unsolicited message rejected and sent to subscriptions
			DisassembleInternalMessage_ReplaceRecipient_TestCase(null, true, null, false, true);						// Unsolicited message sent to subscriptions only
			DisassembleInternalMessage_ReplaceRecipient_TestCase(null, false, "CW1*", true, true);						// Unsolicited message rejected and sent to subscriptions, ignoring override
			DisassembleInternalMessage_ReplaceRecipient_TestCase(null, true, "CW1*", false, true);						// Unsolicited message sent to subscriptions only, ignoring override
		}

		private void DisassembleInternalMessage_ReplaceRecipient_TestCase(string recipient, bool replaceRecipientOnSubscription, string replaceRecipientOverride, bool sentToCW1, bool sentToSubscribers)
		{
			var pipelineContext = new PipelineContext();

			var envelopeMessage = MessageFactory.CreateMessage();
			envelopeMessage.Context = MessageFactory.CreateMessageContext();
			envelopeMessage.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			envelopeMessage.BodyPart.Data = new MemoryStream();

			var internalMessage = MessageFactory.CreateMessage();
			internalMessage.Context = MessageFactory.CreateMessageContext();
			internalMessage.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			internalMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.FSA_internal.xml");

			var ffDisassembler = MockRepository.GenerateMock<IFFDisassembleHelper>();
			ffDisassembler.Stub(x => x.Disassemble(pipelineContext, envelopeMessage, new SchemaWithNone("FSAName, namespace"))).Return(internalMessage);

			var partyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			partyAccessor.Stub(x => x.GetAirlineCodeFromPrefix("127")).Return("ED");
			var subscriptionAccessor = MockRepository.GenerateMock<ISubscriptionAccessor>();
			subscriptionAccessor.Stub(x => x.SelectSubscriptions(internalMessage)).Return(new[] { new SubscriptionInfo() {
				ID = Guid.NewGuid(),
				Type = "",
				Provider = "",
				Subscriber = "SUBSCRIPTION",
				Value = "", 
				Reference = "",
				ReferenceType = "",
				Subscribed = DateTime.Now,
				Expiry = DateTime.Now
			} });

			var component = MockRepository.GeneratePartialMock<AirMessageDisassembleComponent>();
			component.ProcessSubscriptions = true;
			component.ReplaceRecipientOnSubscription = replaceRecipientOnSubscription;
			component.ReplaceRecipientOverride = replaceRecipientOverride;
			component.Stub(x => x.GetFFDisassembler()).Return(ffDisassembler);
			component.Stub(x => x.GetSubsriptionAccessor()).Return(subscriptionAccessor);
			component.Stub(x => x.GetPartyAccessor()).Return(partyAccessor);

			var clients = new TestDbSet<eHubClient> { new eHubClient { CC_PK = new Guid("11111111-1111-1111-1111-111111111111"), CC_ID = "SENDER" } };
			if (recipient != null) clients.Add(new eHubClient { CC_PK = new Guid("22222222-2222-2222-2222-222222222222"), CC_ID = recipient });
			var subscriptionTypes = new TestDbSet<eHubSubscriptionType> { new eHubSubscriptionType { ST_PK = new Guid("13EABE7D-4CB4-46B8-9028-05280272BD57"), ST_ID = "AIRAWB", ST_ExpiryDays = 180 } };
			var subscriptionValues = new TestDbSet<eHubSubscriptionValue>();
			var dbContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			dbContext.Stub(x => x.eHubClients).Return(clients);
			dbContext.Stub(x => x.eHubSubscriptionTypes).Return(subscriptionTypes);
			dbContext.Stub(x => x.eHubSubscriptionValues).Return(subscriptionValues);
			dbContext.Expect(x =>x.Dispose());
			component.Stub(x => x.GetEHubTransactionsContext()).Return(dbContext);

			var context = MockRepository.PartialMock<BTEnvelopContext>();
			context.SenderID = "SENDER";
			context.RecipientID = recipient;
			context.MessageType = "FSA";
			context.ClientAWB = "12767424512";
			context.RecipientPIMA = "DFWDGHDVCS";

			component.DisassembleInternalMessage(pipelineContext, context, envelopeMessage, envelopeMessage, new SchemaWithNone("FSAName, namespace"));

			var resultMsgs = new List<IBaseMessage>();
			IBaseMessage resultMsg;
			while ((resultMsg = component.GetNext(pipelineContext)) != null)
				resultMsgs.Add(resultMsg);

			Assert.AreEqual(resultMsgs.Any(msg => msg.Context.ReadPropertyString<BTS.DestinationParty>() == recipient), sentToCW1);
			Assert.AreEqual(resultMsgs.Any(msg => msg.Context.ReadPropertyString<BTS.DestinationParty>() == "SUBSCRIPTION"), sentToSubscribers);
			Assert.AreEqual(resultMsgs.Any(msg => msg.Context.ReadPropertyString<OverrideFilename>() == "FSA_12767424512__ED_DFWDGHDVCS"), sentToSubscribers);
			dbContext.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void DisassembleInternalMessage_InsertSubscriptionError()
		{
			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.FSA_internal.xml");

			var pipelineContext = new PipelineContext();
			var ffDisassembler = MockRepository.GenerateMock<IFFDisassembleHelper>();
			ffDisassembler.Stub(x => x.Disassemble(pipelineContext, message, new SchemaWithNone("FSAName, namespace"))).Return(message);

			var context = MockRepository.PartialMock<BTEnvelopContext>();
			context.SenderID = "SENDER";
			context.RecipientID = "RECIPIENT";
			context.MessageType = "FSA";
			context.ClientAWB = "12767424512";

			var partyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			partyAccessor.Stub(x => x.GetAirlineCodeFromPrefix(Arg<string>.Is.Anything)).Return("CX");
			var subscriptionAccessor = MockRepository.GenerateMock<ISubscriptionAccessor>();
			subscriptionAccessor.Stub(x => x.SelectSubscriptions(message)).Return(new SubscriptionInfo[] { });

			var logger = MockRepository.GeneratePartialMock<ConsoleOutLogger>("Test", LogLevel.Trace, true, false, false, "s");
			var exception = new InvalidOperationException("Outer Exception", new InvalidOperationException("Inner Exception"));

			var component = MockRepository.GeneratePartialMock<AirMessageDisassembleComponent>();
			component.ProcessSubscriptions = true;
			component.ReplaceRecipientOnSubscription = true;
			component.Stub(x => x.GetFFDisassembler()).Return(ffDisassembler);
			component.Stub(x => x.GetSubsriptionAccessor()).Return(subscriptionAccessor);
			component.Stub(x => x.GetPartyAccessor()).Return(partyAccessor);
			component.Stub(x => x.InsertSubscriptionValue(null, null, null, null, null, null)).IgnoreArguments().Throw(exception);
			component.logger = logger;

			component.DisassembleInternalMessage(pipelineContext, context, message, message, new SchemaWithNone("FSAName, namespace"));

			logger.AssertWasCalled(x => x.Warn("Error creating Air Messaging Waybill subscription.", exception));

			var resultMsgs = new List<IBaseMessage>();
			IBaseMessage resultMsg;
			while ((resultMsg = component.GetNext(pipelineContext)) != null) resultMsgs.Add(resultMsg);
			CollectionAssert.AreEqual(new[] { message }, resultMsgs);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void MessageDisassemble_GLSHK_HKISAC()
		{
			ConfigurationManager.AppSettings["IssueManagerUri"] = "";
			var mockAirMessageDisassembler = MockRepository.GeneratePartialMock<AirMessageDisassembleComponent>();
			var mockFFDisassembler = MockRepository.GenerateMock<IFFDisassembleHelper>();
			var mockOutboxAccessor = MockRepository.GenerateMock<IOutboxAccessor>();
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("body", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("GLSHK.TestFiles.GLSHKISAC.txt");
			message.Context.WriteProperty<BTS.SourceParty>("Sender1");
			var messageText = message.BodyPart.Data.ReadToEnd();
			message.BodyPart.Data.SeekBegin();

			var envelopeMessage = ForwardOnlyStreamMessage("GLSHK.TestFiles.GLSHKISAC.xml");

			var internalMessage = ForwardOnlyStreamMessage("GLSHK.TestFiles.ISAC_FMA_FNA.xml");

			mockAirMessageDisassembler.Enabled = true;
			mockAirMessageDisassembler.ISACXPath = ISACXPathPattern;
			mockAirMessageDisassembler.RawMessageType = "HKCustoms";
			mockAirMessageDisassembler.ServiceProviderID = "GLSHK";
			mockAirMessageDisassembler.EnvelopeMessageSchema = new Schema("GLSHKReplyMessage");
			mockAirMessageDisassembler.FMAFNAMessageSchema = new Schema("FMA_FNA");
			mockAirMessageDisassembler.CMDCMAMessageSchema = new Schema("CMD_CMA");
			mockAirMessageDisassembler.FSUMessageSchema = new Schema("FSU");
			mockAirMessageDisassembler.Stub(x => x.CreateEnvelopContext(null)).IgnoreArguments().Do(new Func<IBaseMessage, EnvelopContext>(m =>
			{
				var xnav = new XPathDocument(new XmlTextReader(m.BodyPart.Data)).CreateNavigator();
				var envContext = MockRepository.GenerateMock<EnvelopContext>();
				envContext.RecipientID = "Recipient1";
				envContext.InternalMessage = xnav.SelectSingleNode("/*[local-name()='GLSHK' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.GLSHKReplyMessage']/*[local-name()='Body' and namespace-uri()='']/*[local-name()='Message' and namespace-uri()='']").Value;
				envContext.MessageType = xnav.SelectSingleNode("/*[local-name()='GLSHK' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.GLSHKReplyMessage']/*[local-name()='MessageHeader' and namespace-uri()='']/*[local-name()='MessageType' and namespace-uri()='']/*[local-name()='Type' and namespace-uri()='']").Value.Remove(0, 3);
				return envContext;
			}));
			mockAirMessageDisassembler.Stub(x => x.GetFFDisassembler()).Return(mockFFDisassembler).Repeat.Any();
			mockFFDisassembler.Stub(x => x.Disassemble(Arg.Is(pipelineContext), Arg.Is(message), Arg<SchemaWithNone>.Matches(y => y.SchemaName == "GLSHKReplyMessage")))
				.Return(envelopeMessage).Repeat.Once();

			mockFFDisassembler.Stub(x => x.Disassemble(Arg.Is(pipelineContext), Arg<IBaseMessage>.Is.Anything, Arg<SchemaWithNone>.Matches(y => y.SchemaName == "FMA_FNA")))
				.Return(internalMessage).Repeat.Once();
			
			mockAirMessageDisassembler.Stub(x => x.InsertToInbox(null, null, Guid.Empty)).IgnoreArguments().Repeat.Times(1);
			mockAirMessageDisassembler.Stub(x => x.GetOutboxAccessor()).Return(mockOutboxAccessor).Repeat.Any();

			MockRepository.ReplayAll();
			mockAirMessageDisassembler.Disassemble(pipelineContext, message);
			mockOutboxAccessor.AssertWasCalled(x => x.InsertToOutbox(
				Arg<string>.Is.Equal("Sender1"),
				Arg<Guid>.Is.Equal(Guid.Empty),
				Arg<eHubGatewayMessage>.Matches(m =>
					m.SchemaName == "HKCustoms" && m.ClientID == "Recipient1" && m.MessageStream.DecodeAndDecompress().ReadToEnd().Equals(messageText)),
				Arg<Guid>.Is.Anything));
			MockRepository.VerifyAll();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void ShouldSendRawMessage()
		{
			ConfigurationManager.AppSettings["IssueManagerUri"] = "";
			var mockAirMessageDisassembler = MockRepository.GeneratePartialMock<AirMessageDisassembleComponent>();
			mockAirMessageDisassembler.FMAFNAMessageSchema = new Schema("FMA_FNA");

			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.AddPart("body", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("GLSHK.TestFiles.GLSHKISAC.txt");

			var xnav = new XPathDocument(new XmlTextReader(GetEmbeddedResource("GLSHK.TestFiles.GLSHKISAC.xml"))).CreateNavigator();
			var envContext = MockRepository.GenerateMock<EnvelopContext>();
			envContext.RecipientID = "Recipient1";
			envContext.InternalMessage = xnav.SelectSingleNode("/*[local-name()='GLSHK' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.GLSHKReplyMessage']/*[local-name()='Body' and namespace-uri()='']/*[local-name()='Message' and namespace-uri()='']").Value;
			envContext.MessageType = xnav.SelectSingleNode("/*[local-name()='GLSHK' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.GLSHKReplyMessage']/*[local-name()='MessageHeader' and namespace-uri()='']/*[local-name()='MessageType' and namespace-uri()='']/*[local-name()='Type' and namespace-uri()='']").Value.Remove(0, 3);

			ISACXPathIsEmpty(mockAirMessageDisassembler, pipelineContext, message, envContext);
			SchemaIsNotMatch(mockAirMessageDisassembler, pipelineContext, message, envContext);
			ISACSectionNotFound(mockAirMessageDisassembler, pipelineContext, message, envContext);
			ISACWithAllMatched(mockAirMessageDisassembler, pipelineContext, message, envContext);
		}

		private void ISACXPathIsEmpty(AirMessageDisassembleComponent mockAirMessageDisassembler, PipelineContext pipelineContext, IBaseMessage message, EnvelopContext envContext)
		{
			mockAirMessageDisassembler.ISACXPath = string.Empty;
			var internalMessage = ForwardOnlyStreamMessage("GLSHK.TestFiles.ISAC_FMA_FNA.xml");

			var result = mockAirMessageDisassembler.ShouldSendRawMessage(pipelineContext,
				mockAirMessageDisassembler.FMAFNAMessageSchema,
				internalMessage,
				message,
				envContext);

			Assert.IsFalse(result);
			Assert.AreEqual(0, internalMessage.BodyPart.Data.Position);
		}

		private void SchemaIsNotMatch(AirMessageDisassembleComponent mockAirMessageDisassembler, PipelineContext pipelineContext, IBaseMessage message, EnvelopContext envContext)
		{
			mockAirMessageDisassembler.ISACXPath = ISACXPathPattern;
			var internalMessage = ForwardOnlyStreamMessage("GLSHK.TestFiles.ISAC_FMA_FNA.xml");

			var result = mockAirMessageDisassembler.ShouldSendRawMessage(pipelineContext,
				new Schema("CMD_CMA"),
				internalMessage,
				message,
				envContext);

			Assert.IsFalse(result);
			Assert.AreEqual(0, internalMessage.BodyPart.Data.Position);
		}

		private void ISACSectionNotFound(AirMessageDisassembleComponent mockAirMessageDisassembler, PipelineContext pipelineContext, IBaseMessage message, EnvelopContext envContext)
		{
			mockAirMessageDisassembler.ISACXPath = ISACXPathPattern;
			var internalMessage = ForwardOnlyStreamMessage("GLSHK.TestFiles.NONE_ISAC_FMA_FNA.xml");
			var result = mockAirMessageDisassembler.ShouldSendRawMessage(pipelineContext,
				mockAirMessageDisassembler.FMAFNAMessageSchema,
				internalMessage,
				message,
				envContext);

			Assert.IsFalse(result);
			Assert.AreEqual(0, internalMessage.BodyPart.Data.Position);
		}

		private void ISACWithAllMatched(AirMessageDisassembleComponent mockAirMessageDisassembler, PipelineContext pipelineContext, IBaseMessage message, EnvelopContext envContext)
		{
			mockAirMessageDisassembler.ISACXPath = ISACXPathPattern;
			var internalMessage = ForwardOnlyStreamMessage("GLSHK.TestFiles.ISAC_FMA_FNA.xml");

			mockAirMessageDisassembler.Stub(x => x.InsertRawMessageToOutbox(
				null, null, null
				)).IgnoreArguments()
				.Do(new Action<IBaseMessage, IPipelineContext, EnvelopContext>((m, p, e) => { }))
				.Repeat.Any();

			mockAirMessageDisassembler.Expect(x => x.InsertRawMessageToOutbox(
				Arg.Is(message),
				Arg.Is(pipelineContext),
				Arg.Is(envContext)))
				.Repeat.Once();

			var result = mockAirMessageDisassembler.ShouldSendRawMessage(pipelineContext,
				mockAirMessageDisassembler.FMAFNAMessageSchema,
				internalMessage,
				message,
				envContext);

			MockRepository.ReplayAll();
			Assert.IsTrue(result);
			MockRepository.VerifyAll();
		}

		private IBaseMessage ForwardOnlyStreamMessage(string xmlResource)
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("body", MessageFactory.CreateMessagePart(), true);
			ForwardOnlyEventingReadStream readOnlyStream = new XmlTranslatorStream(new XmlTextReader(GetEmbeddedResource(xmlResource)));
			message.BodyPart.Data = readOnlyStream;
			return message;
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void InsertSubscriptionValue()
		{
			var clients = new TestDbSet<eHubClient>
			{
				new eHubClient { CC_PK = new Guid("11111111-1111-1111-1111-111111111111"), CC_ID = "PROVIDER" },
				new eHubClient { CC_PK = new Guid("22222222-2222-2222-2222-222222222222"), CC_ID = "SUBSCRIBER" }
			};
			var subscriptionTypes = new TestDbSet<eHubSubscriptionType> { new eHubSubscriptionType { ST_PK = new Guid("13EABE7D-4CB4-46B8-9028-05280272BD57"), ST_ID = "AIRAWB", ST_ExpiryDays = 180 } };
			var subscriptionValues = new TestDbSet<eHubSubscriptionValue>();
			var context = MockRepository.GenerateMock<eHubTransactionsContext>();
			var start = DateTime.UtcNow;
			context.Stub(x => x.eHubClients).Return(clients);
			context.Stub(x => x.eHubSubscriptionTypes).Return(subscriptionTypes);
			context.Stub(x => x.eHubSubscriptionValues).Return(subscriptionValues);
			var disassembler = MockRepository.GeneratePartialMock<AirMessageDisassembleComponent>();
			disassembler.Stub(x => x.GetEHubTransactionsContext()).Return(context);

			disassembler.InsertSubscriptionValue("AIRAWB", "PROVIDER", "SUBSCRIBER", "VALUE", "REFERENCE", "TYPE");

			CollectionAssert.AreEqual(new[]
			{
				new Tuple<Guid, Guid, Guid, string, string, string, bool>(new Guid("13EABE7D-4CB4-46B8-9028-05280272BD57"), new Guid("11111111-1111-1111-1111-111111111111"), new Guid("22222222-2222-2222-2222-222222222222"), "VALUE", "REFERENCE", "TYPE", true)
			}, subscriptionValues.Select(x => new Tuple<Guid, Guid, Guid, string, string, string, bool>(x.SV_ST, x.SV_CC_Sender, x.SV_CC_Recipient, x.SV_Value, x.SV_Reference, x.SV_ReferenceType, x.SV_ExpiryUTC >= start.AddDays(180))).ToArray());
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020,VM")]
		public void MessageDisassemble_KillProcessIfConfigMissing()
		{
			ConfigurationManager.AppSettings["IssueManagerUri"] = null;
			var pipelineContext = new PipelineContext();
			var anyMessage = MessageFactory.CreateMessage();
			anyMessage.Context = MessageFactory.CreateMessageContext();
			anyMessage.AddPart("xml", MessageFactory.CreateMessagePart(), true);
			anyMessage.BodyPart.Data = GetEmbeddedResource("TestFiles.FSU1.xml");
			anyMessage.Context.WriteProperty<BTS.SourceParty>("Sender1");

			var component = MockRepository.GeneratePartialMock<AirMessageDisassembleComponent>();
			component.Enabled = true;
			AssertException(() => component.Disassemble(pipelineContext, anyMessage), typeof(AggregateException), "Errors occurred when processing message, Can not find the config file, kill the process to restart the application pool");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void MessageDisassemble_LogBeforeAndAfterInsertToInbox()
		{
			ConfigurationManager.AppSettings["IssueManagerUri"] = "";
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("text", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("TestFiles.FSA.txt");

			var logger = MockRepository.GenerateMock<ILog>();
			logger.Expect(_ => _.IsTraceEnabled).Return(true).Repeat.Any();
			logger.Expect(_ => _.TraceFormat(
				Arg<string>.Is.Equal("InsertToInbox started, Sender: {0}, EmailSubject: {1}, FileName: {2}, MessageStreamPosition: {3}, MessageTrackingId: {4}"),
				Arg<object[]>.Matches(array => "Source1".Equals(array[0]) && "EmailSubject".Equals(array[1]) && "FileName".Equals(array[2])))
			).Repeat.Once();

			var inboxAccessor = MockRepository.GenerateMock<IInboxAccessor>();
			inboxAccessor
				.Expect(_ => _.InsertToInbox(string.Empty, Guid.Empty, Guid.Empty, MessageStatus.Processing, null))
				.IgnoreArguments().Repeat.Once()
				.Do(new Action<string, Guid, Guid, MessageStatus, eHubGatewayMessage>((senderID, envelopeTrackingID, inboxPK, messageStatus, msg) =>
				{
					logger.Expect(_ => _.Trace("InsertToInbox ended")).Repeat.Once();
				}));

			var context = new DescartesEnvelopContext();
			context.SenderID = "Sender1";
			context.RecipientID = "Recipient1";
			context.MessageType = "FSA";
			context.InternalMessage = @"FSA/6
105-40085161CPHPVG/T9K215.6
BKD/AY9096/05AUG/CPHHEL/T9K215.6/S1900/S2130-S";

			var component = MockRepository.PartialMock<AirMessageDisassembleComponent>();
			component.Enabled = true;
			component.ServiceProviderID = "Descartes";
			component.EnvelopeMessageSchema = new Schema("");
			component.FSUMessageSchema = new Schema("CargoWise.eHub.Clients.EDI.Schemas.CargoIMP.FSU_FSA12, CargoWise.eHub.Clients.EDI.Schemas.CargoImp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350");
			component.FMAFNAMessageSchema = new Schema("CargoWise.eHub.Clients.EDI.Schemas.CargoIMP.FMA_FNA, CargoWise.eHub.Clients.EDI.Schemas.CargoImp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350");
			component.CMDCMAMessageSchema = new Schema("CargoWise.eHub.Clients.EDI.Schemas.CargoIMP.CMD_CMA, CargoWise.eHub.Clients.EDI.Schemas.CargoImp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350");
			message.Context.WriteProperty<BTS.DestinationParty>("Recipient1");
			message.Context.WriteProperty<BTS.SourceParty>("Source1");
			message.Context.WriteProperty<OverrideEmailSubject>("EmailSubject");
			message.Context.WriteProperty<OverrideFilename>("FileName");

			component.Expect(x => x.CreateEnvelopContext(null)).IgnoreArguments().Return(context);
			component.Expect(x => x.DisassembleInternalMessage(null, null, null, null, new Schema(""))).IgnoreArguments();
			component.Expect(_ => _.GetInboxAccessor()).Return(inboxAccessor).Repeat.Once();
			component.Expect(_ => _.GetPipelineLogger(message)).Return(logger).Repeat.Once();

			MockRepository.ReplayAll();

			component.Disassemble(pipelineContext, message);

			MockRepository.VerifyAll();
			logger.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void MessageDisassemble_LogBeforeAndAfterInsertToOutbox()
		{
			ConfigurationManager.AppSettings["IssueManagerUri"] = "";
			var mockAirMessageDisassembler = MockRepository.GeneratePartialMock<AirMessageDisassembleComponent>();
			var mockFFDisassembler = MockRepository.GenerateMock<IFFDisassembleHelper>();
			var mockOutboxAccessor = MockRepository.GenerateMock<IOutboxAccessor>();
			var partyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			var logger = MockRepository.GenerateMock<ILog>();
			var pipelineContext = new PipelineContext();

			var message = MessageFactory.CreateMessage();
			message.Context = MessageFactory.CreateMessageContext();
			message.AddPart("body", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("GLSHK.TestFiles.GLSHKISAC.txt");
			message.Context.WriteProperty<BTS.SourceParty>("Sender1");
			message.Context.WriteProperty<OverrideEmailSubject>("emailSubjectMock");
			message.Context.WriteProperty<OverrideFilename>("filenameMock");
			var messageText = message.BodyPart.Data.ReadToEnd();
			message.BodyPart.Data.SeekBegin();

			var envelopeMessage = MessageFactory.CreateMessage();
			envelopeMessage.Context = MessageFactory.CreateMessageContext();
			envelopeMessage.AddPart("body", MessageFactory.CreateMessagePart(), true);
			envelopeMessage.BodyPart.Data = GetEmbeddedResource("GLSHK.TestFiles.GLSHKISAC.xml");

			var internalMessage = MessageFactory.CreateMessage();
			internalMessage.Context = PipelineUtil.CloneMessageContext(envelopeMessage.Context);
			internalMessage.AddPart("body", MessageFactory.CreateMessagePart(), true);
			internalMessage.BodyPart.Data = GetEmbeddedResource("GLSHK.TestFiles.ISAC_FMA_FNA.xml");

			mockAirMessageDisassembler.Enabled = true;
			mockAirMessageDisassembler.ISACXPath = ISACXPathPattern;
			mockAirMessageDisassembler.RawMessageType = "HKCustoms";
			mockAirMessageDisassembler.ServiceProviderID = "GLSHK";
			mockAirMessageDisassembler.EnvelopeMessageSchema = new Schema("GLSHKReplyMessage");
			mockAirMessageDisassembler.FMAFNAMessageSchema = new Schema("FMA_FNA");
			mockAirMessageDisassembler.CMDCMAMessageSchema = new Schema("CMD_CMA");
			mockAirMessageDisassembler.FSUMessageSchema = new Schema("FSU");
			mockAirMessageDisassembler.Stub(x => x.CreateEnvelopContext(null)).IgnoreArguments().Do(new Func<IBaseMessage, EnvelopContext>(m =>
			{
				var xnav = new XPathDocument(new XmlTextReader(m.BodyPart.Data)).CreateNavigator();
				var envContext = MockRepository.GenerateMock<EnvelopContext>();
				envContext.RecipientID = "Recipient1";
				envContext.InternalMessage = xnav.SelectSingleNode("/*[local-name()='GLSHK' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.GLSHKReplyMessage']/*[local-name()='Body' and namespace-uri()='']/*[local-name()='Message' and namespace-uri()='']").Value;
				envContext.MessageType = xnav.SelectSingleNode("/*[local-name()='GLSHK' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.GLSHKReplyMessage']/*[local-name()='MessageHeader' and namespace-uri()='']/*[local-name()='MessageType' and namespace-uri()='']/*[local-name()='Type' and namespace-uri()='']").Value.Remove(0, 3);
				envContext.ClientAWB = "75361257843";
				envContext.RecipientPIMA = xnav.SelectSingleNode("/*[local-name()='GLSHK' and namespace-uri()='http://CargoWise.eHub.Products.AirMessaging.Schemas.GLSHKReplyMessage']/*[local-name()='InterchangeHeader' and namespace-uri()='']/*[local-name()='Recipient' and namespace-uri()='']/*[local-name()='PIMA' and namespace-uri()='']").Value;
				return envContext;
			}));
			logger.Stub(_ => _.IsTraceEnabled).Return(true).Repeat.Any();
			logger.Expect(_ => _.TraceFormat(
				Arg<string>.Is.Equal("InsertToOutbox started SenderId: {0}, RecipientId: {1}, EmailSubject: {2}, Filename: {3}, MessageTrackingID: {4}"), 
				Arg<object[]>.Matches(array => "Sender1".Equals(array[0]) && "Recipient1".Equals(array[1]) && "emailSubjectMock".Equals(array[2]) && "FNA_75361257843__CX_RHKAGT021330984/HKG85".Equals(array[3])))
			).Repeat.Once();
			mockAirMessageDisassembler.Stub(x => x.GetFFDisassembler()).Return(mockFFDisassembler);
			mockAirMessageDisassembler.Stub(x => x.GetPipelineLogger(message)).Return(logger).Repeat.Any();
			mockFFDisassembler.Stub(x => x.Disassemble(Arg.Is(pipelineContext), Arg.Is(message), Arg<SchemaWithNone>.Matches(y => y.SchemaName == "GLSHKReplyMessage"))).Return(envelopeMessage).Repeat.Once();
			mockFFDisassembler.Stub(x => x.Disassemble(Arg.Is(pipelineContext), Arg<IBaseMessage>.Is.Anything, Arg<SchemaWithNone>.Matches(y => y.SchemaName == "FMA_FNA")))
				.Return(internalMessage);

			partyAccessor.Stub(x => x.GetAirlineCodeFromPrefix("753")).Return("CX");

			mockOutboxAccessor.Expect(x => x.InsertToOutbox(
				Arg<string>.Is.Equal("Sender1"),
				Arg<Guid>.Is.Equal(Guid.Empty),
				Arg<eHubGatewayMessage>.Matches(m => m.SchemaName == "HKCustoms" && m.ClientID == "Recipient1" && m.MessageStream.DecodeAndDecompress().ReadToEnd().Equals(messageText)),
				Arg<Guid>.Is.Anything)).Repeat.Once()
				.Do(new Action<string, Guid, eHubGatewayMessage, Guid>((sender, envelopeTrackingID, msg, inboxPK) =>
				{
					logger.Expect(_ => _.Trace("InsertToOutbox ended")).Repeat.Once();
				}));

			mockAirMessageDisassembler.Stub(x => x.InsertToInbox(null, null, Guid.Empty)).IgnoreArguments().Repeat.Times(1);
			mockAirMessageDisassembler.Stub(x => x.GetOutboxAccessor()).Return(mockOutboxAccessor).Repeat.Any();
			mockAirMessageDisassembler.Stub(x => x.GetPartyAccessor()).Return(partyAccessor).Repeat.Once();

			MockRepository.ReplayAll();
			mockAirMessageDisassembler.Disassemble(pipelineContext, message);

			MockRepository.VerifyAll();
			logger.VerifyAllExpectations();
		}

		[TestMethod]
		public void checkISACXPathInFNAFMASchema()
		{
			var message = MessageFactory.CreateMessage();
			message.AddPart("body", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("GLSHK.TestFiles.ISAC_FMA_FNA.xml");

			var isacSetion = PromotedValue.FindByXPath(message.BodyPart.GetOriginalDataStream(), ISACXPathPattern);

			Assert.IsFalse(string.IsNullOrEmpty(isacSetion));

			message = MessageFactory.CreateMessage();
			message.AddPart("body", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("GLSHK.TestFiles.NONE_ISAC_FMA_FNA.xml");
			isacSetion = PromotedValue.FindByXPath(message.BodyPart.GetOriginalDataStream(), ISACXPathPattern);
			Assert.IsTrue(string.IsNullOrEmpty(isacSetion));
		}

		[TestMethod]
		public void GLSKPromoteFind()
		{
			var promotedValue = new GLSHKPromotedValue();
			var message = MessageFactory.CreateMessage();
			message.AddPart("body", MessageFactory.CreateMessagePart(), true);
			message.BodyPart.Data = GetEmbeddedResource("GLSHK.TestFiles.GLSHKISAC.xml");

			var senderPIMA = promotedValue.Find(message, "SenderPIMA");
			Assert.AreEqual(senderPIMA, "RHKAPT01HKGSTCR");

			AssertException(() => { promotedValue.Find(message, "ReferenceNumber"); },
				typeof(KeyNotFoundException), "The given key was not present in the dictionary.");
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void GetMessageAggregationInformation_WithEnvelope_ShouldReturnFilename()
		{
			var context = MockRepository.PartialMock<BTEnvelopContext>();
			context.SenderID = "Sender1";
			context.RecipientID = "Recipient1";
			context.MessageType = "FSU";
			context.ClientAWB = "92451234551";
			context.RecipientPIMA = "RHKAPT01HKGSTCR";

			var partyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			partyAccessor.Stub(x => x.GetAirlineCodeFromPrefix(Arg<string>.Is.Anything)).Return("CX");

			var component = MockRepository.GeneratePartialMock<AirMessageDisassembleComponent>();
			component.Stub(x => x.GetPartyAccessor()).Return(partyAccessor);

			component.GetMessageAggregationInformation(context);
			Assert.AreEqual("FSU_92451234551__CX_RHKAPT01HKGSTCR", component.GetMessageAggregationInformation(context));
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void GetMessageAggregationInformation_EmptyClientAWB_ShouldReturnFilename()
		{
			var context = MockRepository.PartialMock<BTEnvelopContext>();
			context.SenderID = "Sender1";
			context.RecipientID = "Recipient1";
			context.MessageType = "FSU";
			context.RecipientPIMA = "RHKAPT01HKGSTCR";

			var partyAccessor = MockRepository.GenerateMock<IPartyAccessor>();
			partyAccessor.Stub(x => x.GetAirlineCodeFromPrefix(Arg<string>.Is.Anything)).Return("CX");

			var component = MockRepository.GeneratePartialMock<AirMessageDisassembleComponent>();
			component.Stub(x => x.GetPartyAccessor()).Return(partyAccessor);

			component.GetMessageAggregationInformation(context);
			Assert.AreEqual("FSU____RHKAPT01HKGSTCR", component.GetMessageAggregationInformation(context));
		}
	}
}
