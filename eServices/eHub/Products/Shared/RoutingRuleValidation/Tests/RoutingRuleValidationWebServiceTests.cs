using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Common.Logging;
using eServices.eHubDataModel.Common;
using eServices.eHubDataModel.eHubTransactions;
using eServices.eHubRoutingRuleEngine;
using NUnit.Framework;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.Shared.RoutingRuleValidation.WebService.Tests
{
	[TestFixture]
	public class RoutingRuleValidationWebServiceTests
	{
		private RoutingRuleValidationWebService routingRuleValidationWS;

		[SetUp]
		public void SetUp()
		{
			var mockContext = GetMockContext();
			var mockLogger = MockRepository.GenerateMock<ILog>();

			routingRuleValidationWS = new RoutingRuleValidationWebService(mockContext, mockLogger);
		}

		[Test]
		public void TestEvaluate_InjectedAndMessageFactResolversResolved_RecipientIsGEITaiwan()
		{
			var propertyFacts = new Dictionary<string, string> { { "SourceParty", "WTLDTWAY2" } };
			var msgAsByteArray = GetMessageFromFile("GLB_ELEC_INVOICING_Taiwan.txt");

			var results = routingRuleValidationWS.Evaluate(new RoutingEvaluationGenericInput { RuleId = "GLB_ELEC_INVOICING", PropertyFacts = propertyFacts, Message = msgAsByteArray });

			StringAssert.Contains("GEI_TAIWAN", results[0].RecipientId);
		}

		[Test]
		public void TestEvaluate_InjectedAndMessageFactResolversResolved_RecipientIsGEIFiji()
		{
			var propertyFacts = new Dictionary<string, string> { { "SourceParty", "WTLDTWAY2" } };
			var msgAsByteArray = GetMessageFromFile("GLB_ELEC_INVOICING_Fiji.txt");

			var results = routingRuleValidationWS.Evaluate(new RoutingEvaluationGenericInput { RuleId = "GLB_ELEC_INVOICING", PropertyFacts = propertyFacts, Message = msgAsByteArray });

			StringAssert.Contains("GEI_FIJI", results[0].RecipientId);
		}

		[Test]
		public void TestEvaluate_NoRoutingRuleFound_ReturnErrorCodeAndDescription()
		{
			var propertyFacts = new Dictionary<string, string> { { "SourceParty", "WTLDTWAY2" } };
			var msgAsByteArray = GetMessageFromFile("GLB_ELEC_INVOICING_NoMessagingSystem.txt");

			var results = routingRuleValidationWS.Evaluate(new RoutingEvaluationGenericInput { RuleId = "GLB_ELEC_INVOICING", PropertyFacts = propertyFacts, Message = msgAsByteArray });

			StringAssert.Contains("IRJ", results[0].ErrorCode);
			StringAssert.Contains("You are not registered with this service. Please contact WTG to register.", results[0].ErrorDescription);
		}

		[Test]
		public void TestEvaluate_MessageFactResolverWithPropertyMessageType_RecipientWTLDTWAY2()
		{
			var propertyFacts = new Dictionary<string, string> { { "SourceParty", "WTLDTWAY2" } };
			var msgAsByteArray = GetMessageFromFile("GLB_ELEC_INVOICING_PropertyMessageTypeTest.txt");

			var results = routingRuleValidationWS.Evaluate(new RoutingEvaluationGenericInput { RuleId = "GLB_ELEC_INVOICING", PropertyFacts = propertyFacts, Message = msgAsByteArray });

			StringAssert.Contains("WTLDTWAY2", results[0].RecipientId);
		}

		[Test]
		public void TestResult_Serialize()
		{
			var propertyFacts = new Dictionary<string, string> { { "SourceParty", "WTLDTWAY2" } };
			var msgAsByteArray = GetMessageFromFile("GLB_ELEC_INVOICING_Fiji.txt");

			var results = routingRuleValidationWS.Evaluate(new RoutingEvaluationGenericInput { RuleId = "GLB_ELEC_INVOICING", PropertyFacts = propertyFacts, Message = msgAsByteArray });

			string serializedResults;
			using (var memoryStream = new MemoryStream())
			{
				var serializer = new DataContractSerializer(typeof(CargoWise.eHub.Shared.RoutingRuleEngine.Result[]));
				serializer.WriteObject(memoryStream, results);

				memoryStream.Seek(0, SeekOrigin.Begin);

				using (var streamReader = new StreamReader(memoryStream))
				{
					serializedResults = streamReader.ReadToEnd();
				}
			}

			StringAssert.Contains("<ArrayOfResult xmlns=\"http://schemas.datacontract.org/2004/07/CargoWise.eHub.Shared.RoutingRuleEngine\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\"><Result><ErrorCode i:nil=\"true\"/><ErrorDescription i:nil=\"true\"/><RecipientId>GEI_FIJI</RecipientId><Value i:nil=\"true\"/></Result></ArrayOfResult>", serializedResults);
		}

		[Test]
		public void TestEvaluate_RuleFactory_GetForReading_IsCalled()
		{
			var mockContext = GetMockContext();
			var mockLogger = MockRepository.GenerateMock<ILog>();

			var mockRule = MockRepository.GenerateMock<IRule>();
			mockRule.Stub(_ => _.Evaluate(Arg<eHubTransactionsContext>.Is.Anything, Arg<IFactResolver[]>.Is.Anything, Arg<ILog>.Is.Anything))
				.Return(new Collection<Result> { MockRepository.GenerateMock<Result>() });

			var mockRuleFactory = MockRepository.GenerateMock<IRoutingRuleFactory>();
			mockRuleFactory.Stub(_ => _.GetForReading(Arg<eHubClient>.Is.Anything))
				.Return(mockRule);

			routingRuleValidationWS = new RoutingRuleValidationWebService(mockContext, mockLogger, mockRuleFactory);

			var propertyFacts = new Dictionary<string, string> { { "SourceParty", "WTLDTWAY2" } };
			var msgAsByteArray = GetMessageFromFile("GLB_ELEC_INVOICING_Taiwan.txt");

			var results = routingRuleValidationWS.Evaluate(new RoutingEvaluationGenericInput { RuleId = "GLB_ELEC_INVOICING", PropertyFacts = propertyFacts, Message = msgAsByteArray });

			mockRuleFactory.AssertWasCalled(_ => _.GetForReading(Arg<eHubClient>.Is.Anything));
		}

		public byte[] GetMessageFromFile(string fileName)
		{
			var resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames()
				.Single(str => str.EndsWith(fileName));

			string message;

			using (var stream = GetType().Assembly.GetManifestResourceStream(resourceName))
			{
				if (stream == null) throw new InvalidOperationException();
				using (var reader = new StreamReader(stream))
				{
					message = reader.ReadToEnd();
				}
			}

			return Encoding.UTF8.GetBytes(message);
		}

		public eHubTransactionsContext GetMockContext()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var testRules = new TestDbSet<eHubRoutingRule>();
			var testFacts = new TestDbSet<eHubRoutingRuleFact>();
			var testClients = new TestDbSet<eHubClient>();
			mockContext.Stub(x => x.eHubRoutingRules).Return(testRules);
			mockContext.Stub(x => x.eHubRoutingRuleFacts).Return(testFacts);
			mockContext.Stub(x => x.eHubClients).Return(testClients);
			mockContext.Stub(x => x.SqlQuery<DateTime>(Arg<string>.Is.Anything, Arg<object[]>.Is.Anything)).Return(new[] { DateTime.UtcNow });

			var geiTaiwanClient = new eHubClient { CC_ID = "GEI_TAIWAN", CC_FriendlyName = "Global Electronic Invoicing - Taiwan", CC_OwnerCategory = "Service Provider", CC_SystemCategory = "Third Party" };
			var geiFijiClient = new eHubClient { CC_ID = "GEI_FIJI", CC_FriendlyName = "Global Electronic Invoicing - Fiji", CC_OwnerCategory = "Service Provider", CC_SystemCategory = "Third Party" };
			var wiseTechTestClient = new eHubClient { CC_ID = "WTLDTWAY2", CC_FriendlyName = "WiseTech Test (Internal) Licenses", CC_OwnerCategory = "Client", CC_SystemCategory = "Enterprise" };

			var globalElectronicInvoicingService = new eHubClient
			{
				CC_ID = "GLB_ELEC_INVOICING",
				CC_RR = Guid.NewGuid(),
				eHubRoutingRule = testRules.Add(new eHubRoutingRule
				{
					RR_Group_MatchMultiple = false,
					eHubRoutingRules_Group = testRules.AddRange(new[]
					{
						new eHubRoutingRule
						{
							RR_Condition_Expression = "[@MessagingSystem,Equal,PropertyMessageTypeTest] && [@MessageType,Equal,http://cargowise.com/ehub/products/Accounting/GlobalElectronicInvoicing#GlobalElectronicInvoicing]",
							eHubClient_Recipient = wiseTechTestClient,
							RR_Group_Ordering = 300,
							RR_LastUpdateUTC = DateTime.UtcNow
						},
						new eHubRoutingRule
						{
							RR_Condition_Expression = "[@MessagingSystem,Equal,Taiwan electronic invoicing system]",
							eHubClient_Recipient = geiTaiwanClient,
							RR_Group_Ordering = 200,
							RR_LastUpdateUTC = DateTime.UtcNow
						},
						new eHubRoutingRule
						{
							RR_Condition_Expression = "[@MessagingSystem,Equal,Fiji electronic invoicing system]",
							eHubClient_Recipient = geiFijiClient,
							RR_Group_Ordering = 100,
							RR_LastUpdateUTC = DateTime.UtcNow
						}
					}).ToList(),
					eHubRoutingRuleFacts = new List<eHubRoutingRuleFact>
					{
						testFacts.Add(new eHubRoutingRuleFact{ RX_Name = "MessagingSystem", RX_Type = "XPATH", RX_Query = "/*[local-name()='GlobalElectronicInvoicing']/*[local-name()='Header']/*[local-name()='ElectronicInvoiceBatchRequest']/*[local-name()='MessagingSystem']" }),
						testFacts.Add(new eHubRoutingRuleFact{ RX_Name = "SourceParty", RX_Type = "PROPERTY", RX_Query = "http://schemas.microsoft.com/BizTalk/2003/system-properties#SourceParty" }),
						testFacts.Add(new eHubRoutingRuleFact{ RX_Name = "MessageType", RX_Type = "PROPERTY", RX_Query = "http://schemas.microsoft.com/BizTalk/2003/system-properties#MessageType" })
					},
					RR_Failed_ErrorCode = "IRJ",
					RR_Failed_ErrorDescription = "You are not registered with this service. Please contact WTG to register.",
					RR_Group_Ordering = 1000,
					RR_LastUpdateUTC = DateTime.UtcNow
				})
			};

			testClients.AddRange(new[] { geiTaiwanClient, geiFijiClient, globalElectronicInvoicingService, wiseTechTestClient });

			return mockContext;
		}
	}
}
