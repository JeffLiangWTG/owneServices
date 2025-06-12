using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.eHub.Core.PropertySchemas;
using CargoWise.eHub.Core.Tests.PipelineComponents;
using CargoWise.eHub.Products.OceanCarrierMessaging.PipelineComponents;
using CargoWise.eHub.Shared.BizTalk.PipelineHelpers;
using Common.Logging;
using Common.Logging.Simple;
using eServices.eHubDataModel.Common;
using eServices.eHubDataModel.eHubTransactions;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.Test.BizTalk.PipelineObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests.PipelineComponents
{
  [TestClass]
	public class InboxDisassembleAndRouteTests : BaseComponentTest
	{
		ILog logger;
		eHubTransactionsContext mockContext;

		readonly TestDbSet<eHubClient> eHubClients = new TestDbSet<eHubClient>
		{
			new eHubClient
			{
				CC_ID = "SHIPPING_INSTRUCTION",
				CC_RR = Guid.NewGuid(),
				eHubRoutingRule = new eHubRoutingRule
				{
					RR_Failed_ErrorCode = "IRJ",
					RR_Failed_ErrorDescription = "Rejected",
					RR_Group_MatchMultiple = false,
					RR_LastUpdateUTC = DateTime.UtcNow,
					eHubRoutingRules_Group = new List<eHubRoutingRule>
					{
						new eHubRoutingRule
						{
							RR_Condition_Expression = "[@CarrierHandlingAgent,Equal,C1H1]",
							eHubClient_Recipient = new eHubClient {CC_ID = "HANDLING_AGENT"}
						},
						new eHubRoutingRule
						{
							RR_Condition_Expression = "[@CarrierHandlingAgent,Equal,C1HA]",
							eHubClient_Recipient = new eHubClient {CC_ID = "HANDLING_AGENT_1"}
						},
						new eHubRoutingRule
						{
							RR_Condition_Expression = "[@CarrierBookingAgent,Equal,C1BA]",
							eHubClient_Recipient = new eHubClient {CC_ID = "BOOKING_AGENT"}
						},
						new eHubRoutingRule
						{
							RR_Condition_Expression = "[@CarrierBookingAgent,Equal,C1B1] && ([@CarrierAgent,Equal,DefaultCarrier] || [@CarrierAgent,Equal,])",
							eHubClient_Recipient = new eHubClient {CC_ID = "BOOKING_AGENT_1"}
						},
						new eHubRoutingRule
						{
							RR_Condition_Expression = "[@CarrierBookingAgent,Equal,C1B1] && [@CarrierAgent,Equal,CarrierBookingAgent]",
							eHubClient_Recipient = new eHubClient {CC_ID = "BOOKING_AGENT_2"}
						},
						new eHubRoutingRule
						{
							RR_Condition_Expression = "[@CarrierBookingAgent,Equal,C1B2] && [@CarrierAgent,Equal,CarrierBookingAgent]",
							eHubClient_Recipient = new eHubClient {CC_ID = "BOOKING_AGENT_3"}
						}
					},
					eHubRoutingRuleFacts = new List<eHubRoutingRuleFact>
					{
						new eHubRoutingRuleFact { RX_Name = "CarrierHandlingAgent", RX_Type = "XPATH", RX_Query = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][*[local-name()='AddressType']/text()='CarrierHandlingAgent']/*[local-name()='RegistrationNumberCollection']/*[local-name()='RegistrationNumber'][*[local-name()='Type']/text()='C1C']/*[local-name()='Value']" },
						new eHubRoutingRuleFact { RX_Name = "CarrierBookingAgent", RX_Type = "XPATH", RX_Query = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][*[local-name()='AddressType']/text()='CarrierBookingAgent']/*[local-name()='RegistrationNumberCollection']/*[local-name()='RegistrationNumber'][*[local-name()='Type']/text()='C1C']/*[local-name()='Value']" },
						new eHubRoutingRuleFact { RX_Name = "CarrierAgent", RX_Type = "INJECTED" }
					}
				}
			},
			new eHubClient
			{
				CC_ID = "OCM_Splitting",
				CC_RR = Guid.NewGuid(),
				eHubRoutingRule = new eHubRoutingRule
				{
					RR_Group_Name = "SplitingBlackList",
					RR_Group_MatchMultiple = false,
					RR_LastUpdateUTC = DateTime.UtcNow,
					eHubRoutingRules_Group = new List<eHubRoutingRule>
					{
						new eHubRoutingRule
						{
							RR_Condition_Expression = "[@Port,Equal,CNNGB] && [@Purpose,Equal,WTH]",
						},
						new eHubRoutingRule
						{
							RR_Condition_Expression = "[@Port,Equal,CNNBG] && [@Purpose,Equal,WTH]",
						},
						new eHubRoutingRule
						{
							RR_Condition_Expression = "[@Port,Equal,CNNBO] && [@Purpose,Equal,WTH]",
						}
					},
					eHubRoutingRuleFacts = new List<eHubRoutingRuleFact>
					{
						new eHubRoutingRuleFact { RX_Name = "Port", RX_Type = "XPATHNAV", RX_Query = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][*[local-name()='Key']/text()='OperationalPort_Code']/*[local-name()='Value']" },
						new eHubRoutingRuleFact { RX_Name = "Purpose", RX_Type = "XPATH", RX_Query = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='DataContext']/*[local-name()='DocumentaryOverride']/*[local-name()='Purpose']" },
						new eHubRoutingRuleFact { RX_Name = "UShipmentNamespace", RX_Type = "XPATH", RX_Query = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/namespace::*[name()='']" },
						new eHubRoutingRuleFact { RX_Name = "SubShipmentCount", RX_Type = "XPATHNAVFUNC", RX_Query = "count(/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='SubShipmentCollection']/*[local-name()='SubShipment'])" },
						new eHubRoutingRuleFact { RX_Name = "ContainerCount", RX_Type = "XPATHNAVFUNC", RX_Query = "count(/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='ContainerCollection']/*[local-name()='Container'])" }
					},
					eHubRoutingRule_Failed = new eHubRoutingRule
					{
						RR_Group_Name = "SplittingWhiteList",
						RR_Group_MatchMultiple = false,
						eHubRoutingRules_Group = new List<eHubRoutingRule>
						{
							new eHubRoutingRule
							{
								RR_Condition_Expression = "[@SubShipmentCount,NotEqual,0]&&[@SubShipmentCount,NotEqual,1]",
								eHubClient_Recipient = new eHubClient {CC_ID = "OCM_SUBSHIPMENT_SPLIT"}
							},
							new eHubRoutingRule
							{
								RR_Condition_Expression = "[@UShipmentNamespace,Equal,http://www.cargowise.com/Schemas/Universal/2012/11/ContainerGrossWeightVerification/2] && [@ContainerCount,NotEqual,0]&&[@ContainerCount,NotEqual,1]",
								eHubClient_Recipient = new eHubClient {CC_ID = "OCM_CONTAINER_SPLIT"}
							}
						}
					}
				}
			},
			new eHubClient
			{
				CC_ID = "OCM_MutipleRecipientsCopying",
				CC_RR = Guid.NewGuid(),
				eHubRoutingRule = new eHubRoutingRule
				{
					RR_Group_MatchMultiple = false,
					RR_LastUpdateUTC = DateTime.UtcNow,
					eHubRoutingRules_Group = new List<eHubRoutingRule>
					{
						new eHubRoutingRule
						{
							RR_Condition_Expression = "[@SourceParty,IsMatch,^DFO.*$] && [@SCACUniShip,IsMatch,^(ONEY|HLCU)$] && [@Port,IsMatch,^(CNNGB|CNNBO|CNNBG)$]",
							RR_Result_Value = "CarrierBookingAgent"
						}
					},
					eHubRoutingRuleFacts = new List<eHubRoutingRuleFact>
					{
						new eHubRoutingRuleFact { RX_Name = "Port", RX_Type = "XPATHNAV", RX_Query = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='AddInfoCollection']/*[local-name()='AddInfo'][*[local-name()='Key']/text()='OperationalPort_Code']/*[local-name()='Value']" },
						new eHubRoutingRuleFact { RX_Name = "SCACUniShip", RX_Type = "XPATHNAVFUNC", RX_Query = "translate(/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='OrganizationAddressCollection']/*[local-name()='OrganizationAddress'][*[local-name()='AddressType']/text()='ShippingLineAddress']/*[local-name()='RegistrationNumberCollection']/*[local-name()='RegistrationNumber'][*[local-name()='Type']/text()='CCC' and *[local-name()='CountryOfIssue']/text()='US']/*[local-name()='Value'], 'abcdefghijklmnopqrstuvwxyz', 'ABCDEFGHIJKLMNOPQRSTUVWXYZ')" },
						new eHubRoutingRuleFact { RX_Name = "SourceParty", RX_Type = "PROPERTY", RX_Query = "http://schemas.microsoft.com/BizTalk/2003/system-properties#SourceParty" },
					}
				}
			}
		};

		readonly MessageFactory messageFactory = new MessageFactory();
		PipelineContext pipelineContext;
		IBaseMessage message;

		[TestInitialize]
		public void TestInitialize()
		{
			logger = new TraceLogger(false, this.GetType().Name, LogLevel.All, true, false, false, null);
			InboxDisassembleAndRoute.GetLogger = (p) => logger;

			mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			mockContext.Stub(x => x.eHubClients).Return(eHubClients);
			mockContext.Stub(x => x.SqlQuery<DateTime>(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything)).Return(new[] { DateTime.UtcNow });
			InboxDisassembleAndRoute.GetDbContext = () => mockContext;

			pipelineContext = new PipelineContext();
			message = messageFactory.CreateMessage();
			message.Context = messageFactory.CreateMessageContext();
			message.AddPart("body", messageFactory.CreateMessagePart(), true);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void InboxDisassembleAndRoute_Default()
		{
			using (var sqlMessageStream = Assembly.GetExecutingAssembly()
				.GetManifestResourceStream(this.GetType().Namespace + ".TestFiles.SqlMessage_Default.xml"))
			{
				message.BodyPart.Data = sqlMessageStream;

				var component = new InboxDisassembleAndRoute();

				component.Disassemble(pipelineContext, message);

				var result = GetMessagesFromQueue(pipelineContext, component);

				Assert.IsNotNull(result);
				Assert.AreEqual(1, result.Count);
				Assert.AreEqual("8fb1e522-5d6b-418e-86bc-f8acb103a9e8",
					result[0].Context.ReadPropertyString<InternalTrackingID>());
				Assert.AreEqual("e291e463-331a-4dfd-9aaf-68523e8c825c",
					result[0].Context.ReadPropertyString<MessageTrackingID>());
				Assert.AreEqual("cfa3d48d-dd71-4bbd-838b-721a5aafed78",
					result[0].Context.ReadPropertyString<EnvelopeTrackingID>());
				Assert.AreEqual("EMAIL SUBJECT", result[0].Context.ReadPropertyString<OverrideEmailSubject>());
				Assert.AreEqual("FILE_NAME", result[0].Context.ReadPropertyString<OverrideFilename>());
				Assert.AreEqual("38329", result[0].Context.ReadPropertyString<UncompressedLength>());
				Assert.IsTrue(result[0].Context.IsPromoted("NextStep", "http://cargowise.com/ehub/routing/2010/06"));
				Assert.AreEqual("http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange",
					result[0].Context.ReadPropertyString<BTS.MessageType>());
				Assert.IsTrue(result[0].Context.IsPromoted("MessageType",
					"http://schemas.microsoft.com/BizTalk/2003/system-properties"));

				Assert.AreEqual("SHIPPING_INSTRUCTION", result[0].Context.ReadPropertyString<BTS.SourceParty>());
				Assert.AreEqual("HYEDAUUAT", result[0].Context.ReadPropertyString<BTS.DestinationParty>());
				Assert.AreEqual("IRJ", result[0].Context.ReadPropertyString<ErrorCode>());
				Assert.AreEqual("Rejected", result[0].Context.ReadPropertyString<ErrorDescription>());
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void InboxDisassembleAndRoute_MutipleRecipientsCopying()
		{
			using (var sqlMessageStream = Assembly.GetExecutingAssembly()
				.GetManifestResourceStream(this.GetType().Namespace + ".TestFiles.SqlMessage_MutipleRecipientsCopying.xml"))
			{
				message.BodyPart.Data = sqlMessageStream;
				message.Context.WriteProperty<BTS.SourceParty>("DFODAUUAT");
				var component = new InboxDisassembleAndRoute();

				component.Disassemble(pipelineContext, message);

				var result = GetMessagesFromQueue(pipelineContext, component);

				Assert.AreEqual(2, result.Count);
				Assert.AreEqual("DFODAUUAT", result[0].Context.ReadPropertyString<BTS.SourceParty>());
				Assert.AreEqual("BOOKING_AGENT_1", result[0].Context.ReadPropertyString<BTS.DestinationParty>());
				Assert.AreEqual("DFODAUUAT", result[1].Context.ReadPropertyString<BTS.SourceParty>());
				Assert.AreEqual("BOOKING_AGENT_2", result[1].Context.ReadPropertyString<BTS.DestinationParty>());
				AssertMessageContent(".TestFiles.SqlMessage_MutipleRecipientsCopying_Output.xml", result[0]);
				AssertMessageContent(".TestFiles.SqlMessage_MutipleRecipientsCopying_Output.xml", result[1]);
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void InboxDisassembleAndRoute_MutipleRecipientsCopying_DefaultCarrierRejected()
		{
			using (var sqlMessageStream = Assembly.GetExecutingAssembly()
							 .GetManifestResourceStream(this.GetType().Namespace + ".TestFiles.SqlMessage_MutipleRecipientsCopying_DefaultCarrierRejected.xml"))
			{
				message.BodyPart.Data = sqlMessageStream;
				message.Context.WriteProperty<BTS.SourceParty>("DFODAUUAT");
				var component = new InboxDisassembleAndRoute();

				component.Disassemble(pipelineContext, message);

				var result = GetMessagesFromQueue(pipelineContext, component);

				Assert.AreEqual(2, result.Count);
				Assert.AreEqual("SHIPPING_INSTRUCTION", result[0].Context.ReadPropertyString<BTS.SourceParty>());
				Assert.AreEqual("DFODAUUAT", result[0].Context.ReadPropertyString<BTS.DestinationParty>());
				Assert.AreEqual("IRJ", result[0].Context.ReadPropertyString<ErrorCode>());
				Assert.AreEqual("Rejected", result[0].Context.ReadPropertyString<ErrorDescription>());

				Assert.AreEqual("DFODAUUAT", result[1].Context.ReadPropertyString<BTS.SourceParty>());
				Assert.AreEqual("BOOKING_AGENT_3", result[1].Context.ReadPropertyString<BTS.DestinationParty>());
				Assert.AreEqual("IRJ", result[0].Context.ReadPropertyString<ErrorCode>());
				Assert.AreEqual("Rejected", result[0].Context.ReadPropertyString<ErrorDescription>());

				AssertMessageContent(".TestFiles.SqlMessage_MutipleRecipientsCopying_DefaultCarrierRejected_Output.xml", result[0]);
				AssertMessageContent(".TestFiles.SqlMessage_MutipleRecipientsCopying_DefaultCarrierRejected_Output.xml", result[1]);
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void InboxDisassembleAndRoute_ShippingOrder()
		{
			using (var sqlMessageStream = Assembly.GetExecutingAssembly()
				.GetManifestResourceStream(this.GetType().Namespace + ".TestFiles.SqlMessage_ShippingOrder.xml"))
			{
				message.BodyPart.Data = sqlMessageStream;
				var component = new InboxDisassembleAndRoute();

				component.Disassemble(pipelineContext, message);

				var result = GetMessagesFromQueue(pipelineContext, component);

				Assert.AreEqual(1, result.Count);
				Assert.AreEqual("HYEDAUUAT", result[0].Context.ReadPropertyString<BTS.SourceParty>());
				Assert.AreEqual("BOOKING_AGENT_1", result[0].Context.ReadPropertyString<BTS.DestinationParty>());
				AssertMessageContent(".TestFiles.SqlMessage_ShippingOrder_Output.xml", result[0]);
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void InboxDisassembleAndRoute_eManifest()
		{
			using (var sqlMessageStream = Assembly.GetExecutingAssembly()
				.GetManifestResourceStream(this.GetType().Namespace + ".TestFiles.SqlMessage_eManifest.xml"))
			{
				message.BodyPart.Data = sqlMessageStream;

				var component = new InboxDisassembleAndRoute();

				component.Disassemble(pipelineContext, message);

				var result = GetMessagesFromQueue(pipelineContext, component);

				Assert.AreEqual(1, result.Count);
				Assert.AreEqual("HYEDAUUAT", result[0].Context.ReadPropertyString<BTS.SourceParty>());
				Assert.AreEqual("HANDLING_AGENT_1", result[0].Context.ReadPropertyString<BTS.DestinationParty>());
				AssertMessageContent(".TestFiles.SqlMessage_eManifest_Output.xml", result[0]);
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void InboxDisassembleAndRoute_VGM_Handling()
		{
			using (var sqlMessageStream = Assembly.GetExecutingAssembly()
				.GetManifestResourceStream(this.GetType().Namespace + ".TestFiles.SqlMessage_VGM_Handling.xml"))
			{
				message.BodyPart.Data = sqlMessageStream;

				var component = new InboxDisassembleAndRoute();

				component.Disassemble(pipelineContext, message);

				var result = GetMessagesFromQueue(pipelineContext, component);

				Assert.AreEqual(1, result.Count);
				Assert.AreEqual("HYEDAUUAT", result[0].Context.ReadPropertyString<BTS.SourceParty>());
				Assert.AreEqual("HANDLING_AGENT", result[0].Context.ReadPropertyString<BTS.DestinationParty>());
				AssertMessageContent(".TestFiles.SqlMessage_VGM_Handling_Output.xml", result[0]);
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void InboxDisassembleAndRoute_VGM_Booking()
		{
			using (var sqlMessageStream = Assembly.GetExecutingAssembly()
				.GetManifestResourceStream(this.GetType().Namespace + ".TestFiles.SqlMessage_VGM_Booking.xml"))
			{
				message.BodyPart.Data = sqlMessageStream;

				var component = new InboxDisassembleAndRoute();

				component.Disassemble(pipelineContext, message);

				var result = GetMessagesFromQueue(pipelineContext, component);

				Assert.AreEqual(1, result.Count);
				Assert.AreEqual("HYEDAUUAT", result[0].Context.ReadPropertyString<BTS.SourceParty>());
				Assert.AreEqual("BOOKING_AGENT", result[0].Context.ReadPropertyString<BTS.DestinationParty>());
				AssertMessageContent(".TestFiles.SqlMessage_VGM_Booking_Output.xml", result[0]);
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void InboxDisassembleAndRoute_SplittingBlackList()
		{
			using (var sqlMessageStream = Assembly.GetExecutingAssembly()
				.GetManifestResourceStream(this.GetType().Namespace + ".TestFiles.SqlMessage_Splitting_BlackList.xml"))
			{
				message.BodyPart.Data = sqlMessageStream;

				var component = new InboxDisassembleAndRoute();

				component.Disassemble(pipelineContext, message);

				var result = GetMessagesFromQueue(pipelineContext, component);

				Assert.AreEqual(1, result.Count);
				Assert.AreEqual("SHIPPING_INSTRUCTION", result[0].Context.ReadPropertyString<BTS.SourceParty>());
				Assert.AreEqual("HYEDAUUAT", result[0].Context.ReadPropertyString<BTS.DestinationParty>());
				Assert.AreEqual("IRJ", result[0].Context.ReadPropertyString<ErrorCode>());
				Assert.AreEqual("Rejected", result[0].Context.ReadPropertyString<ErrorDescription>());
				AssertMessageContent(".TestFiles.SqlMessage_Splitting_BlackList_Output.xml", result[0]);
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void InboxDisassembleAndRoute_SplittingWhiteList_SubShipment()
		{
			using (var sqlMessageStream = Assembly.GetExecutingAssembly()
				.GetManifestResourceStream(this.GetType().Namespace + ".TestFiles.SqlMessage_Splitting_WhiteList_SubShipment.xml"))
			{
				message.BodyPart.Data = sqlMessageStream;

				var component = new InboxDisassembleAndRoute();

				component.Disassemble(pipelineContext, message);

				var result = GetMessagesFromQueue(pipelineContext, component);

				Assert.AreEqual(1, result.Count);
				Assert.AreEqual("HYEDAUUAT", result[0].Context.ReadPropertyString<BTS.SourceParty>());
				Assert.AreEqual("OCM_SUBSHIPMENT_SPLIT", result[0].Context.ReadPropertyString<BTS.DestinationParty>());
				AssertMessageContent(".TestFiles.SqlMessage_Splitting_WhiteList_SubShipment_Output.xml", result[0]);
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void InboxDisassembleAndRoute_SplittingWhiteList_Container()
		{
			using (var sqlMessageStream = Assembly.GetExecutingAssembly()
				.GetManifestResourceStream(this.GetType().Namespace + ".TestFiles.SqlMessage_Splitting_WhiteList_Container.xml"))
			{
				message.BodyPart.Data = sqlMessageStream;

				var component = new InboxDisassembleAndRoute();

				component.Disassemble(pipelineContext, message);

				var result = GetMessagesFromQueue(pipelineContext, component);

				Assert.AreEqual(1, result.Count);
				Assert.AreEqual("HYEDAUUAT", result[0].Context.ReadPropertyString<BTS.SourceParty>());
				Assert.AreEqual("OCM_CONTAINER_SPLIT", result[0].Context.ReadPropertyString<BTS.DestinationParty>());
				AssertMessageContent(".TestFiles.SqlMessage_Splitting_WhiteList_Container_Output.xml", result[0]);
			}
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void InboxDisassemble_CW1toCW1_Response()
		{
			using (var sqlMessageStream = Assembly.GetExecutingAssembly()
				.GetManifestResourceStream(this.GetType().Namespace + ".TestFiles.ResponseMessage_CW1toCW1.xml"))
			{
				message.BodyPart.Data = sqlMessageStream;

				var component = new InboxDisassembleAndRoute();

				component.Disassemble(pipelineContext, message);

				var result = GetMessagesFromQueue(pipelineContext, component);

				Assert.AreEqual(1, result.Count);
				Assert.AreEqual("HYEDAUUAT", result[0].Context.ReadPropertyString<BTS.SourceParty>());
				Assert.AreEqual("CARGOWISE_BC", result[0].Context.ReadPropertyString<BTS.DestinationParty>());
				Assert.AreEqual("EMAIL SUBJECT", result[0].Context.ReadPropertyString<OverrideEmailSubject>());
				Assert.AreEqual("FILE_NAME", result[0].Context.ReadPropertyString<OverrideFilename>());
				Assert.AreEqual("7929", result[0].Context.ReadPropertyString<UncompressedLength>());
				Assert.IsTrue(result[0].Context.IsPromoted("NextStep", "http://cargowise.com/ehub/routing/2010/06"));
				Assert.AreEqual("http://www.cargowise.com/Schemas/Universal/2011/11#UniversalInterchange",
					result[0].Context.ReadPropertyString<BTS.MessageType>());
				Assert.IsTrue(result[0].Context.IsPromoted("MessageType",
					"http://schemas.microsoft.com/BizTalk/2003/system-properties"));
			}
		}

		#region Implementation
		private List<IBaseMessage> GetMessagesFromQueue(PipelineContext pipelineContext, InboxDisassembleAndRoute component)
		{
			var messages = new List<IBaseMessage>();
			IBaseMessage msg;
			while ((msg = component.GetNext(pipelineContext)) != null)
				messages.Add(msg);
			return messages;
		}

		private void AssertMessageContent(string filePath, IBaseMessage actualMessage)
		{
			var expectedStream = Assembly.GetExecutingAssembly()
				.GetManifestResourceStream(this.GetType().Namespace + filePath);
			var actualStream = actualMessage.BodyPart.GetOriginalDataStream();

			AssertXmlStream(expectedStream, actualStream);
		}
		#endregion
	}
}
