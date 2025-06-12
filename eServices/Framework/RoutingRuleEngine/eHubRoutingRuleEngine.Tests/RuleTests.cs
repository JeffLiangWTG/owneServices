using System;
using System.Collections.Generic;
using System.Linq;
using eServices.eHubDataModel.Common;
using eServices.eHubDataModel.eHubTransactions;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace eServices.eHubRoutingRuleEngine.Tests
{
	[TestClass]
	public class RuleTests : TestBase
	{
		[TestMethod]
		public void Rule_Create()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockClient = MockRepository.GenerateMock<eHubClient>();
			var recipient = new eHubClient();
			var providers = new List<eHubServiceProvider>();
			mockClient.Stub(x => x.eHubRoutingRule).PropertyBehavior();
			mockClient.Stub(x => x.eHubServiceProviders_Service).Return(providers);

			var rule = new Rule(mockContext, mockClient) { MatchMultiple = true, ErrorCode = "RIJ", ErrorDescription = "Rejected" };

			rule.Facts.Add(new Fact() { Name = "FACT1", Type = "XPATH", Query = "//*[1]" });
			rule.Facts.Add(new Fact() { Name = "FACT2", Type = "PROPERTY", Query = "NS#NAME" });
			rule.Facts.Add(new Fact() { Name = "FACT3", Type = "SQL", Query = "SELECT *" });

			Assert.IsNotNull(mockClient.eHubRoutingRule);
			Assert.AreEqual(mockClient.eHubRoutingRule.RR_Group_MatchMultiple, true);
			Assert.AreEqual(mockClient.eHubRoutingRule.RR_Failed_ErrorCode, "RIJ");
			Assert.AreEqual(mockClient.eHubRoutingRule.RR_Failed_ErrorDescription, "Rejected");

			CollectionAssert.AreEqual(new List<Tuple<string, string, string>>()
				{
					new Tuple<string, string, string>("FACT1", "XPATH", "//*[1]"),
					new Tuple<string, string, string>("FACT2", "PROPERTY", "NS#NAME"),
					new Tuple<string, string, string>("FACT3", "SQL", "SELECT *")
				},
				mockClient.eHubRoutingRule.eHubRoutingRuleFacts.Select(f => new Tuple<string, string, string>(f.RX_Name, f.RX_Type, f.RX_Query)).ToList()
			);
		}

		[TestMethod]
		public void Rule_Create_Fail()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var testRules = new TestDbSet<eHubRoutingRule>();
			var testClient = new eHubClient() { CC_ID = "CLIENTID", eHubRoutingRule = new eHubRoutingRule() };
			mockContext.Stub(x => x.eHubRoutingRules).Return(testRules);

			AssertException<ArgumentNullException>(() => new Rule(null, testClient), x => x.ParamName == "context");
			AssertException<ArgumentNullException>(() => new Rule(mockContext, (eHubClient)null), x => x.ParamName == "client");
			AssertException<InvalidOperationException>(() => new Rule(mockContext, testClient), x => x.Message == "Unable to create rule. Rule already exists for client ID 'CLIENTID'.");
		}

		[TestMethod]
		public void Rule_GetAndDelete()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var testRules = new TestDbSet<eHubRoutingRule>();
			var testFacts = new TestDbSet<eHubRoutingRuleFact>();
			var testClientNoRule = new eHubClient();
			var testClientRecipient = new eHubClient();
			var testClientSuccess = new eHubClient();
			var testClientFailed = new eHubClient();
			var testClientRule = new eHubClient()
			{
				eHubRoutingRule = testRules.Add(new eHubRoutingRule
				{
					RR_Group_MatchMultiple = false,
					eHubRoutingRules_Group = testRules.AddRange(new[]
					{
						new eHubRoutingRule
						{
							RR_Condition_Expression = "EXPR1",
							eHubClient_Recipient = testClientRecipient,
							RR_Group_Ordering = 1000
						},
						new eHubRoutingRule
						{
							RR_Condition_Expression = "EXPR2",
							eHubRoutingRule_Success = testRules.Add(new eHubRoutingRule { RR_Condition_Expression = "EXPR3", eHubClient_Recipient = testClientSuccess }),
							eHubRoutingRule_Failed = testRules.Add(new eHubRoutingRule { RR_Condition_Expression = "EXPR4", eHubClient_Recipient = testClientFailed }),
							RR_Group_Ordering = 2000
						}
					}).ToList(),
					RR_Failed_ErrorCode = "ERR",
					RR_Failed_ErrorDescription = "Error",
					eHubRoutingRuleFacts = new List<eHubRoutingRuleFact> 
					{ 
						testFacts.Add(new eHubRoutingRuleFact { RX_Name = "FACT1", RX_Type = "XPATH", RX_Query = "//*[1]" }),
						testFacts.Add(new eHubRoutingRuleFact { RX_Name = "FACT2", RX_Type = "PROPERTY", RX_Query = "NS#NAME" }),
						testFacts.Add(new eHubRoutingRuleFact { RX_Name = "FACT3", RX_Type = "SQL", RX_Query = "SELECT *" })
					}
				})
			};
			testClientRule.eHubRoutingRule.eHubClients = new List<eHubClient> { testClientRule };
			mockContext.Stub(x => x.eHubRoutingRules).Return(testRules);
			mockContext.Stub(x => x.eHubRoutingRuleFacts).Return(testFacts);

			var ruleNone = Rule.GetForEditing(mockContext, testClientNoRule);

			Assert.IsNull(ruleNone);

			var ruleFound = Rule.GetForEditing(mockContext, testClientRule);

			Assert.IsNotNull(ruleFound);
			Assert.AreEqual(2, ruleFound.SubRules.Count);
			Assert.IsInstanceOfType(ruleFound.SubRules.ElementAt(0), typeof(Condition));
			Assert.IsInstanceOfType(ruleFound.SubRules.ElementAt(1), typeof(Condition));
			Assert.IsInstanceOfType(ruleFound.SubRules.ElementAt(1).SuccessSubRule, typeof(Condition));
			Assert.IsInstanceOfType(ruleFound.SubRules.ElementAt(1).FailedSubRule, typeof(Condition));
			Assert.AreEqual(3, ruleFound.Facts.Count);
			Assert.AreSame(testClientRecipient, (ruleFound.SubRules.ElementAt(0) as Condition).Recipient);
			Assert.AreSame(testClientSuccess, (ruleFound.SubRules.ElementAt(1).SuccessSubRule as Condition).Recipient);
			Assert.AreSame(testClientFailed, (ruleFound.SubRules.ElementAt(1).FailedSubRule as Condition).Recipient);

			Rule.Delete(mockContext, testClientRule);

			Assert.AreEqual(0, testRules.Count());
			Assert.AreEqual(0, testFacts.Count());
		}

		[TestMethod]
		public void Rule_FindGroupRule()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var testRules = new TestDbSet<eHubRoutingRule>();
			var testClientRule = new eHubClient();
			var theLookingGroup = new eHubRoutingRule
					{
						RR_Group_MatchMultiple = true,
						eHubRoutingRules_Group = testRules.AddRange(new[]
						{
							new eHubRoutingRule
							{
								RR_Condition_Expression = "[@SourceParty,Equal,TESTSENDER__1]",
								RR_Group_Ordering = 1000
							},
							new eHubRoutingRule
							{
								RR_Condition_Expression = "([@SourceParty,StartsWith,TEST] || [@SCAC,NotEqual,BBBB]) && [@SCAC,Contains,Z]",
								RR_Group_Ordering = 2000,
								eHubRoutingRule_Success = new eHubRoutingRule
								{
									RR_Group_MatchMultiple = true,
									RR_Group_Name = "TheSuccessGroup"
								},
								eHubRoutingRule_Failed = new eHubRoutingRule
								{
									RR_Group_MatchMultiple = true,
									RR_Group_Name = "TheFailedGroup"
								}
							}
						}).ToList(),
						RR_Group_Name = "TheLookingGroup"
					};
			testClientRule.eHubRoutingRule = testRules.Add(new eHubRoutingRule
			{
				RR_Group_MatchMultiple = true,
				eHubRoutingRules_Group = testRules.AddRange(new[]
				{
					new eHubRoutingRule
					{
						RR_Group_MatchMultiple = true,
						eHubRoutingRules_Group = testRules.AddRange(new[]
						{
							new eHubRoutingRule
							{
								RR_Condition_Expression = "[@SourceParty,Equal,TESTSENDER__1]",
								RR_Group_Ordering = 1000
							}
						}).ToList(),
						RR_Group_Name = "TST_Group"
					},
					theLookingGroup
				}).ToList(),
			});
			mockContext.Stub(x => x.eHubRoutingRules).Return(testRules);

			var ruleFound = Rule.GetForEditing(mockContext, testClientRule);
			Assert.IsNotNull(ruleFound);
			var result = ruleFound.FindGroupRule("TheLookingGroup");
			Assert.IsNotNull(result);
			Assert.AreEqual(theLookingGroup, result.eHubRoutingRule);
			result = ruleFound.FindGroupRule("TheSuccessGroup");
			Assert.IsNotNull(result);
			result = ruleFound.FindGroupRule("TheFailedGroup");
			Assert.IsNotNull(result);
			result = ruleFound.FindGroupRule("RandomGroupNotExist");
			Assert.IsNull(result);
		}

		[TestMethod]
		public void Rule_Evaluate()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockPropertyResolver = MockRepository.GenerateMock<IFactResolver>();
			var mockXpathResolver = MockRepository.GenerateMock<IFactResolver>();
			var mockSqlResolver = MockRepository.GenerateMock<IFactResolver>();
			var logEntries = new List<string>();
			var mockLogger = MockRepository.GenerateMock<ILog>();
			InitialiseLoggerStubs(mockLogger, logEntries);
			var testRules = new TestDbSet<eHubRoutingRule>();
			var testFacts = new TestDbSet<eHubRoutingRuleFact>();
			mockContext.Stub(x => x.eHubRoutingRules).Return(testRules);
			mockContext.Stub(x => x.eHubRoutingRuleFacts).Return(testFacts);
			var clientInttra = new eHubClient() { CC_ID = "INTTRA_SI" };
			var clientService = new eHubClient { CC_ID = "SHIPPING_INSTRUCTION" };
			var providerInttra = new eHubServiceProvider { eHubClient_Service = clientService, eHubClient_Provider = clientInttra };
			clientService.eHubRoutingRule = testRules.Add(new eHubRoutingRule
			{
				RR_Group_MatchMultiple = true,
				eHubRoutingRules_Group = testRules.AddRange(new[]
				{
					new eHubRoutingRule
					{
						RR_Condition_Expression = "[@SourceParty,EndsWith,1]",
						eHubRoutingRule_Success = testRules.Add(new eHubRoutingRule
						{
							RR_Group_MatchMultiple = true,
							eHubServiceProvider = providerInttra,
							eHubRoutingRules_Group = testRules.AddRange(new[]
							{
								new eHubRoutingRule
								{
									RR_Condition_Expression = "[@SourceParty,Equal,TESTSENDER__1]",
									eHubServiceProvider = providerInttra,
									RR_Group_Ordering = 1000
								},
								new eHubRoutingRule
								{
									RR_Condition_Expression = "([@SourceParty,StartsWith,TEST] || [@SCAC,NotEqual,BBBB]) && [@SCAC,Contains,Z]",
									eHubServiceProvider = providerInttra,
									RR_Group_Ordering = 2000
								}
							}).ToList(),
						}),
						eHubRoutingRule_Failed = testRules.Add(new eHubRoutingRule
						{
							RR_Condition_Expression = "[@SCAC,IsMatch,^A.*$]",
							RR_Failed_ErrorCode = "IRJ",
							RR_Failed_ErrorDescription = "Shipping Instruction Withdrawal|not supported by carrier, contact carrier direct"
						})
					}
				}).ToList(),
				eHubRoutingRuleFacts = new List<eHubRoutingRuleFact> 
				{ 
					testFacts.Add(new eHubRoutingRuleFact { RX_Name = "SourceParty", RX_Type = "PROPERTY", RX_Query = "http://schemas.microsoft.com/BizTalk/2003/system-properties#SourceParty" }),
					testFacts.Add(new eHubRoutingRuleFact { RX_Name = "SCAC", RX_Type = "XPATH", RX_Query = "//s0:TransportLegCollection/s0:TransportLeg[s0:LegType/text()='Main']/s0:Carrier/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='CCC']/s0:Value/text()" }),
				}
			});
			clientService.eHubServiceProviders_Service = new List<eHubServiceProvider> { providerInttra };
			mockPropertyResolver.Expect(x => x.Resolve(Arg<Fact[]>.Is.Anything)).Callback(new Func<Fact[], bool>(f => f.Count() == 2 && f[0].Name == "SourceParty" && f[1].Name == "SCAC")).Repeat.Twice();
			mockXpathResolver.Expect(x => x.Resolve(Arg<Fact[]>.Is.Anything)).Callback(new Func<Fact[], bool>(f => f.Count() == 2 && f[0].Name == "SourceParty" && f[1].Name == "SCAC")).Repeat.Twice();
			mockSqlResolver.Expect(x => x.Resolve(Arg<Fact[]>.Is.Anything)).Callback(new Func<Fact[], bool>(f => f.Count() == 2 && f[0].Name == "SourceParty" && f[1].Name == "SCAC")).Repeat.Twice();

			var rule = Rule.GetForReading(clientService);

			rule.Facts[0].Value = "TESTSENDER__1";
			rule.Facts[1].Value = "AAAA";

			var result1 = rule.Evaluate(mockContext, new IFactResolver[] { mockPropertyResolver, mockXpathResolver, mockSqlResolver }, mockLogger);

			rule.Facts[0].Value = "TESTSENDER__2";

			var result2 = rule.Evaluate(mockContext, new IFactResolver[] { mockPropertyResolver, mockXpathResolver, mockSqlResolver }, mockLogger);

			mockPropertyResolver.VerifyAllExpectations();
			mockXpathResolver.VerifyAllExpectations();
			mockSqlResolver.VerifyAllExpectations();

			CollectionAssert.AreEquivalent(new[] { new Tuple<eHubClient, string, string, string>(clientInttra, null, null, null) },
											result1.Select(r => new Tuple<eHubClient, string, string, string>(r.Recipient, r.ErrorCode, r.ErrorDescription, r.Value)).ToArray());
			CollectionAssert.AreEquivalent(new[] { new Tuple<eHubClient, string, string, string>(null, "IRJ", "Shipping Instruction Withdrawal|not supported by carrier, contact carrier direct", null) },
											result2.Select(r => new Tuple<eHubClient, string, string, string>(r.Recipient, r.ErrorCode, r.ErrorDescription, r.Value)).ToArray());

			CollectionAssert.AreEquivalent(new[] {
				"[INFO ] Evaluating rule for client 'SHIPPING_INSTRUCTION'",
				"[INFO ] Resolved facts for evaluation: [@SourceParty=TESTSENDER__1], [@SCAC=AAAA]",
				"[DEBUG] Evaluating group rule.",
				"[DEBUG] Evaluating condition rule: [@SourceParty,EndsWith,1]",
				"[TRACE] Parsing expression text: [@SourceParty,EndsWith,1]",
				"[TRACE] Parsed condition: facts.Item[\"@SourceParty\"].EndsWith(\"1\")",
				"[TRACE] Parsed expression: facts.Item[\"@SourceParty\"].EndsWith(\"1\")",
				"[DEBUG] Condition rule evaluated to 'true'",
				"[DEBUG] Evaluating success sub-rule",
				"[DEBUG] Evaluating group rule.",
				"[DEBUG] Evaluating condition rule: ([@SourceParty,StartsWith,TEST] || [@SCAC,NotEqual,BBBB]) && [@SCAC,Contains,Z]",
				"[TRACE] Parsing expression text: ([@SourceParty,StartsWith,TEST] || [@SCAC,NotEqual,BBBB]) && [@SCAC,Contains,Z]",
				"[TRACE] Parsed condition: facts.Item[\"@SourceParty\"].StartsWith(\"TEST\")",
				"[TRACE] Parsed condition: (facts.Item[\"@SCAC\"] != \"BBBB\")",
				"[TRACE] Parsed group: (facts.Item[\"@SourceParty\"].StartsWith(\"TEST\") Or (facts.Item[\"@SCAC\"] != \"BBBB\"))",
				"[TRACE] Parsed condition: facts.Item[\"@SCAC\"].Contains(\"Z\")",
				"[TRACE] Parsed expression: ((facts.Item[\"@SourceParty\"].StartsWith(\"TEST\") Or (facts.Item[\"@SCAC\"] != \"BBBB\")) And facts.Item[\"@SCAC\"].Contains(\"Z\"))",
				"[DEBUG] Condition rule evaluated to 'false'",
				"[DEBUG] Evaluating condition rule: [@SourceParty,Equal,TESTSENDER__1]",
				"[TRACE] Parsing expression text: [@SourceParty,Equal,TESTSENDER__1]",
				"[TRACE] Parsed condition: (facts.Item[\"@SourceParty\"] == \"TESTSENDER__1\")",
				"[TRACE] Parsed expression: (facts.Item[\"@SourceParty\"] == \"TESTSENDER__1\")",
				"[DEBUG] Condition rule evaluated to 'true'",
				"[DEBUG] Evaluating provider 'INTTRA_SI'",
				"[DEBUG] Provider evaluation results = [ { Recipient: 'INTTRA_SI' } ]",
				"[DEBUG] Group rule evaluated results = [ { Recipient: 'INTTRA_SI' } ]",
				"[DEBUG] Group rule evaluated results = [ { Recipient: 'INTTRA_SI' } ]",
				"[INFO ] Rule evaluation results = [ { Recipient: 'INTTRA_SI' } ]",
				"[DEBUG] Finished evaluating rule for client 'SHIPPING_INSTRUCTION'",
				"[INFO ] Evaluating rule for client 'SHIPPING_INSTRUCTION'",
				"[INFO ] Resolved facts for evaluation: [@SourceParty=TESTSENDER__2], [@SCAC=AAAA]",
				"[DEBUG] Evaluating group rule.",
				"[DEBUG] Evaluating condition rule: [@SourceParty,EndsWith,1]",
				"[DEBUG] Condition rule evaluated to 'false'",
				"[DEBUG] Evaluating failed sub-rule",
				"[DEBUG] Evaluating condition rule: [@SCAC,IsMatch,^A.*$]",
				"[TRACE] Parsing expression text: [@SCAC,IsMatch,^A.*$]",
				"[TRACE] Parsed condition: IsMatch(facts.Item[\"@SCAC\"], \"^A.*$\")",
				"[TRACE] Parsed expression: IsMatch(facts.Item[\"@SCAC\"], \"^A.*$\")",
				"[DEBUG] Condition rule evaluated to 'true'",
				"[DEBUG] Condition rule result = { ErrorCode: 'IRJ', ErrorDescription: 'Shipping Instruction Withdrawal|not supported by carrier, contact carrier direct' }",
				"[DEBUG] Group rule evaluated results = [ { ErrorCode: 'IRJ', ErrorDescription: 'Shipping Instruction Withdrawal|not supported by carrier, contact carrier direct' } ]",
				"[INFO ] Rule evaluation results = [ { ErrorCode: 'IRJ', ErrorDescription: 'Shipping Instruction Withdrawal|not supported by carrier, contact carrier direct' } ]",
				"[DEBUG] Finished evaluating rule for client 'SHIPPING_INSTRUCTION'"
			}, logEntries, "Log output does not match expected value. Actual log:\r\n" + String.Join("\r\n", logEntries) + "\r\n");
		}

		[TestMethod]
		public void Rule_Evaluate_FactNotExisting()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockPropertyResolver = MockRepository.GenerateMock<IFactResolver>();
			var mockXpathResolver = MockRepository.GenerateMock<IFactResolver>();
			var mockSqlResolver = MockRepository.GenerateMock<IFactResolver>();
			var logEntries = new List<string>();
			var mockLogger = MockRepository.GenerateMock<ILog>();
			InitialiseLoggerStubs(mockLogger, logEntries);
			var testRules = new TestDbSet<eHubRoutingRule>();
			var testFacts = new TestDbSet<eHubRoutingRuleFact>();
			mockContext.Stub(x => x.eHubRoutingRules).Return(testRules);
			mockContext.Stub(x => x.eHubRoutingRuleFacts).Return(testFacts);
			var clientInttra = new eHubClient() { CC_ID = "INTTRA_SI" };
			var clientService = new eHubClient { CC_ID = "SHIPPING_INSTRUCTION" };
			var providerInttra = new eHubServiceProvider { eHubClient_Service = clientService, eHubClient_Provider = clientInttra };
			clientService.eHubRoutingRule = testRules.Add(new eHubRoutingRule
			{
				RR_PK = Guid.Parse("6BB85629-1E02-4341-ADAA-D7CBE4188062"),
				RR_Group_MatchMultiple = true,
				eHubRoutingRules_Group = testRules.AddRange(new[]
				{
					new eHubRoutingRule
					{
						RR_PK= Guid.Parse("A5825F49-1BE8-4261-BE90-0E2FCC404D0D"),
						RR_Condition_Expression = "[@SourceParty,EndsWith,1]",
						eHubRoutingRule_Success = testRules.Add(new eHubRoutingRule
						{
							RR_PK= Guid.Parse("6DB68D6E-347A-4089-95F4-D8F76F42AF48"),
							RR_Group_MatchMultiple = true,
							eHubServiceProvider = providerInttra,
							eHubRoutingRules_Group = testRules.AddRange(new[]
							{
								new eHubRoutingRule
								{
									RR_PK= Guid.Parse("BDDAD13C-6B12-4E22-AFD0-8930C6F5A1EE"),
									RR_Condition_Expression = "[@SourceParty,Equal,TESTSENDER__1]",
									eHubServiceProvider = providerInttra,
									RR_Group_Ordering = 1000
								},
								new eHubRoutingRule
								{
									RR_PK= Guid.Parse("37A884F8-88B6-4909-96D7-61327816442E"),
									RR_Condition_Expression = "([@SourceParty,StartsWith,TEST] || [@SCAC,NotEqual,BBBB]) && [@SCAC,Contains,Z]",
									eHubServiceProvider = providerInttra,
									RR_Group_Ordering = 2000
								}
							}).ToList(),
						}),
						eHubRoutingRule_Failed = testRules.Add(new eHubRoutingRule
						{
							RR_PK= Guid.Parse("9834C548-CA5E-4582-A635-3B0B5B64C39D"),
							RR_Condition_Expression = "[@SCAC,IsMatch,^A.*$]",
							RR_Failed_ErrorCode = "IRJ",
							RR_Failed_ErrorDescription = "Shipping Instruction Withdrawal|not supported by carrier, contact carrier direct"
						})
					}
				}).ToList(),
				eHubRoutingRuleFacts = new List<eHubRoutingRuleFact> 
				{ 
					testFacts.Add(new eHubRoutingRuleFact { RX_Name = "SourceParty", RX_Type = "PROPERTY", RX_Query = "http://schemas.microsoft.com/BizTalk/2003/system-properties#SourceParty" })
				}
			});

			var rule = Rule.GetForReading(clientService);

			rule.Facts[0].Value = "TESTSENDER__1";

			try
			{
				var result1 = rule.Evaluate(mockContext, new IFactResolver[] { mockPropertyResolver, mockXpathResolver, mockSqlResolver }, mockLogger);
			}
			catch (RoutingRuleException ex)
			{
				StringAssert.Contains(ex.ToString(), @"eServices.eHubRoutingRuleEngine.RoutingRuleException: Routing Rule Engine: Exception evaluating rule for client 'SHIPPING_INSTRUCTION' - Could not find matching Fact when evaluating rule: ([@SourceParty,StartsWith,TEST] || [@SCAC,NotEqual,BBBB]) && [@SCAC,Contains,Z]");
				StringAssert.Contains(ex.ToString(), @"---> eServices.eHubRoutingRuleEngine.RoutingRuleException: Current row having init error: RR_PK='6bb85629-1e02-4341-adaa-d7cbe4188062', RR_Condition_Expression='', RR_Failed_ErrorCode='', RR_Failed_ErrorDescription='', RR_Failed_RR_SubRule='', RR_Group_MatchMultiple='True', RR_Group_Name='', RR_Group_Ordering='', RR_Group_RR_GroupRule='', RR_LastUpdateUTC='', RR_Result_Value='', RR_Success_CC_Recipient='', RR_Success_RR_SubRule='', RR_Success_SP_Provider=''");
				StringAssert.Contains(ex.ToString(), @"---> System.AggregateException: One or more errors occurred. ---> eServices.eHubRoutingRuleEngine.RoutingRuleException: Current row having init error: RR_PK='a5825f49-1be8-4261-be90-0e2fcc404d0d', RR_Condition_Expression='[@SourceParty,EndsWith,1]', RR_Failed_ErrorCode='', RR_Failed_ErrorDescription='', RR_Failed_RR_SubRule='', RR_Group_MatchMultiple='', RR_Group_Name='', RR_Group_Ordering='', RR_Group_RR_GroupRule='', RR_LastUpdateUTC='', RR_Result_Value='', RR_Success_CC_Recipient='', RR_Success_RR_SubRule='', RR_Success_SP_Provider=''");
				StringAssert.Contains(ex.ToString(), @"---> eServices.eHubRoutingRuleEngine.RoutingRuleException: Current row having init error: RR_PK='6db68d6e-347a-4089-95f4-d8f76f42af48', RR_Condition_Expression='', RR_Failed_ErrorCode='', RR_Failed_ErrorDescription='', RR_Failed_RR_SubRule='', RR_Group_MatchMultiple='True', RR_Group_Name='', RR_Group_Ordering='', RR_Group_RR_GroupRule='', RR_LastUpdateUTC='', RR_Result_Value='', RR_Success_CC_Recipient='', RR_Success_RR_SubRule='', RR_Success_SP_Provider=''");
				StringAssert.Contains(ex.ToString(), @"---> System.AggregateException: One or more errors occurred. ---> eServices.eHubRoutingRuleEngine.RoutingRuleException: Current row having init error: RR_PK='37a884f8-88b6-4909-96d7-61327816442e', RR_Condition_Expression='([@SourceParty,StartsWith,TEST] || [@SCAC,NotEqual,BBBB]) && [@SCAC,Contains,Z]', RR_Failed_ErrorCode='', RR_Failed_ErrorDescription='', RR_Failed_RR_SubRule='', RR_Group_MatchMultiple='', RR_Group_Name='', RR_Group_Ordering='2000', RR_Group_RR_GroupRule='', RR_LastUpdateUTC='', RR_Result_Value='', RR_Success_CC_Recipient='', RR_Success_RR_SubRule='', RR_Success_SP_Provider=''");
				StringAssert.Contains(ex.ToString(), @"---> eServices.eHubRoutingRuleEngine.RoutingRuleException: Could not find matching Fact when evaluating rule: ([@SourceParty,StartsWith,TEST] || [@SCAC,NotEqual,BBBB]) && [@SCAC,Contains,Z]");
			}
		}

		[TestMethod]
		public void Rule_Evaluate_MatchExpression_NilRule()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockPropertyResolver = MockRepository.GenerateMock<IFactResolver>();
			var mockXpathResolver = MockRepository.GenerateMock<IFactResolver>();
			var mockSqlResolver = MockRepository.GenerateMock<IFactResolver>();
			var logEntries = new List<string>();
			var mockLogger = MockRepository.GenerateMock<ILog>();
			InitialiseLoggerStubs(mockLogger, logEntries);
			var testRules = new TestDbSet<eHubRoutingRule>();
			var testFacts = new TestDbSet<eHubRoutingRuleFact>();
			mockContext.Stub(x => x.eHubRoutingRules).Return(testRules);
			mockContext.Stub(x => x.eHubRoutingRuleFacts).Return(testFacts);
			var clientInttra = new eHubClient() { CC_ID = "INTTRA_SI" };
			var clientService = new eHubClient { CC_ID = "SHIPPING_INSTRUCTION" };
			var providerInttra = new eHubServiceProvider { eHubClient_Service = clientService, eHubClient_Provider = clientInttra };
			clientService.eHubRoutingRule = testRules.Add(new eHubRoutingRule
			{
				RR_Group_MatchMultiple = true,
				eHubRoutingRules_Group = testRules.AddRange(new[]
				{
					new eHubRoutingRule
					{
						RR_Condition_Expression = "[@SourceParty,EndsWith,1]",
						eHubRoutingRule_Success = testRules.Add(new eHubRoutingRule
						{
							RR_Group_MatchMultiple = true,
							eHubRoutingRules_Group = testRules.AddRange(new[]
							{
								new eHubRoutingRule
								{
									RR_Group_MatchMultiple = false,
									eHubServiceProvider = providerInttra,
									eHubRoutingRules_Group = testRules.AddRange(new[]
									{
										new eHubRoutingRule
										{
											RR_Condition_Expression = "[@SourceParty,StartsWith,TEST]",
										},
										new eHubRoutingRule
										{
											RR_Condition_Expression = "[@SourceParty,StartsWith,TEST] || [@SCAC,NotEqual,BBBB]",
											RR_Failed_ErrorCode = "IRJ",
											RR_Failed_ErrorDescription = "Expect this error is not called"
										}
									}).ToList(),
								},
								new eHubRoutingRule
								{
									RR_Condition_Expression = "([@SourceParty,StartsWith,TEST] || [@SCAC,NotEqual,BBBB]) && [@SCAC,Contains,Z]",
									eHubServiceProvider = providerInttra,
									RR_Group_Ordering = 2000
								}
							}).ToList(),
						}),
						eHubRoutingRule_Failed = testRules.Add(new eHubRoutingRule
						{
							RR_Condition_Expression = "[@SCAC,IsMatch,^A.*$]",
							RR_Failed_ErrorCode = "IRJ",
							RR_Failed_ErrorDescription = "Shipping Instruction Withdrawal|not supported by carrier, contact carrier direct"
						})
					}
				}).ToList(),
				eHubRoutingRuleFacts = new List<eHubRoutingRuleFact> 
				{ 
					testFacts.Add(new eHubRoutingRuleFact { RX_Name = "SourceParty", RX_Type = "PROPERTY", RX_Query = "http://schemas.microsoft.com/BizTalk/2003/system-properties#SourceParty" }),
					testFacts.Add(new eHubRoutingRuleFact { RX_Name = "SCAC", RX_Type = "XPATH", RX_Query = "//s0:TransportLegCollection/s0:TransportLeg[s0:LegType/text()='Main']/s0:Carrier/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='CCC']/s0:Value/text()" }),
				}
			});
			clientService.eHubServiceProviders_Service = new List<eHubServiceProvider> { providerInttra };
			mockPropertyResolver.Expect(x => x.Resolve(Arg<Fact[]>.Is.Anything)).Callback(new Func<Fact[], bool>(f => f.Count() == 2 && f[0].Name == "SourceParty" && f[1].Name == "SCAC")).Repeat.Once();
			mockXpathResolver.Expect(x => x.Resolve(Arg<Fact[]>.Is.Anything)).Callback(new Func<Fact[], bool>(f => f.Count() == 2 && f[0].Name == "SourceParty" && f[1].Name == "SCAC")).Repeat.Once();
			mockSqlResolver.Expect(x => x.Resolve(Arg<Fact[]>.Is.Anything)).Callback(new Func<Fact[], bool>(f => f.Count() == 2 && f[0].Name == "SourceParty" && f[1].Name == "SCAC")).Repeat.Once();

			var rule = Rule.GetForReading(clientService);

			rule.Facts[0].Value = "TESTSENDER__1";
			rule.Facts[1].Value = "ZZZZ";

			var result1 = rule.Evaluate(mockContext, new IFactResolver[] { mockPropertyResolver, mockXpathResolver, mockSqlResolver }, mockLogger);

			mockPropertyResolver.VerifyAllExpectations();
			mockXpathResolver.VerifyAllExpectations();
			mockSqlResolver.VerifyAllExpectations();

			CollectionAssert.AreEquivalent(new[] {
				"[INFO ] Evaluating rule for client 'SHIPPING_INSTRUCTION'",
				"[INFO ] Resolved facts for evaluation: [@SourceParty=TESTSENDER__1], [@SCAC=ZZZZ]",
				"[DEBUG] Evaluating group rule.",
				"[DEBUG] Evaluating condition rule: [@SourceParty,EndsWith,1]",
				"[TRACE] Parsing expression text: [@SourceParty,EndsWith,1]",
				"[TRACE] Parsed condition: facts.Item[\"@SourceParty\"].EndsWith(\"1\")",
				"[TRACE] Parsed expression: facts.Item[\"@SourceParty\"].EndsWith(\"1\")",
				"[DEBUG] Condition rule evaluated to 'true'",
				"[DEBUG] Evaluating success sub-rule",
				"[DEBUG] Evaluating group rule.",
				"[DEBUG] Evaluating condition rule: ([@SourceParty,StartsWith,TEST] || [@SCAC,NotEqual,BBBB]) && [@SCAC,Contains,Z]",
				"[TRACE] Parsing expression text: ([@SourceParty,StartsWith,TEST] || [@SCAC,NotEqual,BBBB]) && [@SCAC,Contains,Z]",
				"[TRACE] Parsed condition: facts.Item[\"@SourceParty\"].StartsWith(\"TEST\")",
				"[TRACE] Parsed condition: (facts.Item[\"@SCAC\"] != \"BBBB\")",
				"[TRACE] Parsed group: (facts.Item[\"@SourceParty\"].StartsWith(\"TEST\") Or (facts.Item[\"@SCAC\"] != \"BBBB\"))",
				"[TRACE] Parsed condition: facts.Item[\"@SCAC\"].Contains(\"Z\")",
				"[TRACE] Parsed expression: ((facts.Item[\"@SourceParty\"].StartsWith(\"TEST\") Or (facts.Item[\"@SCAC\"] != \"BBBB\")) And facts.Item[\"@SCAC\"].Contains(\"Z\"))",
				"[DEBUG] Condition rule evaluated to 'true'",
				"[DEBUG] Evaluating provider 'INTTRA_SI'",
				"[DEBUG] Provider evaluation results = [ { Recipient: 'INTTRA_SI' } ]",
				"[DEBUG] Evaluating group rule.",
				"[DEBUG] Evaluating condition rule: [@SourceParty,StartsWith,TEST]",
				"[TRACE] Parsing expression text: [@SourceParty,StartsWith,TEST]",
				"[TRACE] Parsed condition: facts.Item[\"@SourceParty\"].StartsWith(\"TEST\")",
				"[TRACE] Parsed expression: facts.Item[\"@SourceParty\"].StartsWith(\"TEST\")",
				"[DEBUG] Condition rule evaluated to 'true'",
				"[DEBUG] Condition rule result = nil",
				"[DEBUG] Group rule evaluated results = [  ]",
				"[DEBUG] Group rule evaluated results = [ { Recipient: 'INTTRA_SI' } ]",
				"[DEBUG] Group rule evaluated results = [ { Recipient: 'INTTRA_SI' } ]",
				"[INFO ] Rule evaluation results = [ { Recipient: 'INTTRA_SI' } ]",
				"[DEBUG] Finished evaluating rule for client 'SHIPPING_INSTRUCTION'",
			}, logEntries, "Log output does not match expected value. Actual log:\r\n" + String.Join("\r\n", logEntries) + "\r\n");

			CollectionAssert.AreEquivalent(new[] { new Tuple<eHubClient, string, string, string>(clientInttra, null, null, null) },
											result1.Select(r => new Tuple<eHubClient, string, string, string>(r.Recipient, r.ErrorCode, r.ErrorDescription, r.Value)).ToArray());
		}

		[TestMethod]
		public void Rule_Evaluate_Grouping()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockPropertyResolver = MockRepository.GenerateMock<IFactResolver>();
			var mockXpathResolver = MockRepository.GenerateMock<IFactResolver>();
			var mockSqlResolver = MockRepository.GenerateMock<IFactResolver>();
			var logEntries = new List<string>();
			var mockLogger = MockRepository.GenerateMock<ILog>();
			InitialiseLoggerStubs(mockLogger, logEntries);
			var testRules = new TestDbSet<eHubRoutingRule>();
			var testFacts = new TestDbSet<eHubRoutingRuleFact>();
			mockContext.Stub(x => x.eHubRoutingRules).Return(testRules);
			mockContext.Stub(x => x.eHubRoutingRuleFacts).Return(testFacts);

			var ccAKL = new eHubClient { CC_ID = "SPM_NZAKL", CC_FriendlyName = "Shipping Port Messaging - Aukland", CC_OwnerCategory = "Service Provider", CC_SystemCategory = "Third Party" };

			var ccService = new eHubClient()
			{
				CC_ID = "SHIPPING_PORT_MESSAGE",
				eHubRoutingRule = testRules.Add(new eHubRoutingRule
				{
					RR_Group_MatchMultiple = false,
					eHubRoutingRules_Group = testRules.AddRange(new[]
					{
						new eHubRoutingRule
						{
							RR_Condition_Expression = "(([@RecipientRole,Equal,PEM] || [@RecipientRole,Equal,PER]) && [@PortOfLoading,Equal,NZAKL]) || (([@RecipientRole,Equal,PIM] || [@RecipientRole,Equal,PIR]) && [@PortOfDischarge,Equal,NZAKL])",
							eHubClient_Recipient = ccAKL
						}
					}).ToList(),
					eHubRoutingRuleFacts = new List<eHubRoutingRuleFact> 
					{ 
						testFacts.Add(new eHubRoutingRuleFact { RX_Name = "RecipientRole", RX_Type = "XPATH", RX_Query = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='DataContext']/*[local-name()='DocumentaryOverride']/*[local-name()='RecipientRole']" }),
						testFacts.Add(new eHubRoutingRuleFact { RX_Name = "PortOfLoading", RX_Type = "XPATH", RX_Query = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='PortOfLoading']" }),
						testFacts.Add(new eHubRoutingRuleFact { RX_Name = "PortOfDischarge", RX_Type = "XPATH", RX_Query = "/*[local-name()='UniversalInterchange']/*[local-name()='Body']/*[local-name()='UniversalShipment']/*[local-name()='Shipment']/*[local-name()='PortOfDischarge']" }),
					}
				})
			};

			var rule = Rule.GetForReading(ccService);

			rule.Facts[0].Value = "PEM";
			rule.Facts[1].Value = "NZAKL";

			var result = rule.Evaluate(mockContext, new IFactResolver[] { mockPropertyResolver, mockXpathResolver, mockSqlResolver }, mockLogger);

			CollectionAssert.AreEquivalent(new[] {
				"[INFO ] Evaluating rule for client 'SHIPPING_PORT_MESSAGE'",
				"[INFO ] Resolved facts for evaluation: [@RecipientRole=PEM], [@PortOfLoading=NZAKL], [@PortOfDischarge=]",
				"[DEBUG] Evaluating group rule.",
				"[DEBUG] Evaluating condition rule: (([@RecipientRole,Equal,PEM] || [@RecipientRole,Equal,PER]) && [@PortOfLoading,Equal,NZAKL]) || (([@RecipientRole,Equal,PIM] || [@RecipientRole,Equal,PIR]) && [@PortOfDischarge,Equal,NZAKL])",
				"[TRACE] Parsing expression text: (([@RecipientRole,Equal,PEM] || [@RecipientRole,Equal,PER]) && [@PortOfLoading,Equal,NZAKL]) || (([@RecipientRole,Equal,PIM] || [@RecipientRole,Equal,PIR]) && [@PortOfDischarge,Equal,NZAKL])",
				"[TRACE] Parsed condition: (facts.Item[\"@RecipientRole\"] == \"PEM\")",
				"[TRACE] Parsed condition: (facts.Item[\"@RecipientRole\"] == \"PER\")",
				"[TRACE] Parsed group: ((facts.Item[\"@RecipientRole\"] == \"PEM\") Or (facts.Item[\"@RecipientRole\"] == \"PER\"))",
				"[TRACE] Parsed condition: (facts.Item[\"@PortOfLoading\"] == \"NZAKL\")",
				"[TRACE] Parsed group: (((facts.Item[\"@RecipientRole\"] == \"PEM\") Or (facts.Item[\"@RecipientRole\"] == \"PER\")) And (facts.Item[\"@PortOfLoading\"] == \"NZAKL\"))",
				"[TRACE] Parsed condition: (facts.Item[\"@RecipientRole\"] == \"PIM\")",
				"[TRACE] Parsed condition: (facts.Item[\"@RecipientRole\"] == \"PIR\")",
				"[TRACE] Parsed group: ((facts.Item[\"@RecipientRole\"] == \"PIM\") Or (facts.Item[\"@RecipientRole\"] == \"PIR\"))",
				"[TRACE] Parsed condition: (facts.Item[\"@PortOfDischarge\"] == \"NZAKL\")",
				"[TRACE] Parsed group: (((facts.Item[\"@RecipientRole\"] == \"PIM\") Or (facts.Item[\"@RecipientRole\"] == \"PIR\")) And (facts.Item[\"@PortOfDischarge\"] == \"NZAKL\"))",
				"[TRACE] Parsed expression: ((((facts.Item[\"@RecipientRole\"] == \"PEM\") Or (facts.Item[\"@RecipientRole\"] == \"PER\")) And (facts.Item[\"@PortOfLoading\"] == \"NZAKL\")) Or (((facts.Item[\"@RecipientRole\"] == \"PIM\") Or (facts.Item[\"@RecipientRole\"] == \"PIR\")) And (facts.Item[\"@PortOfDischarge\"] == \"NZAKL\")))",
				"[DEBUG] Condition rule evaluated to 'true'",
				"[DEBUG] Condition rule result = { Recipient: 'SPM_NZAKL' }",
				"[DEBUG] Group rule evaluated results = [ { Recipient: 'SPM_NZAKL' } ]",
				"[INFO ] Rule evaluation results = [ { Recipient: 'SPM_NZAKL' } ]",
				"[DEBUG] Finished evaluating rule for client 'SHIPPING_PORT_MESSAGE'"
			}, logEntries, "Log output does not match expected value. Actual log:\r\n" + String.Join("\r\n", logEntries) + "\r\n");
		}

		[TestMethod]
		public void Rule_Evaluate_ProviderRule()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockPropertyResolver = MockRepository.GenerateMock<IFactResolver>();
			var mockXpathResolver = MockRepository.GenerateMock<IFactResolver>();
			var mockSqlResolver = MockRepository.GenerateMock<IFactResolver>();
			var logEntries = new List<string>();
			var mockLogger = MockRepository.GenerateMock<ILog>();
			InitialiseLoggerStubs(mockLogger, logEntries);
			var testRules = new TestDbSet<eHubRoutingRule>();
			var testFacts = new TestDbSet<eHubRoutingRuleFact>();
			var testProviders = new TestDbSet<eHubServiceProvider>();
			var testClientRegos = new TestDbSet<eHubClientRegistration>();
			var testOperRegos = new TestDbSet<eHubServiceOperatorRegistration>();
			mockContext.Stub(x => x.eHubRoutingRules).Return(testRules);
			mockContext.Stub(x => x.eHubRoutingRuleFacts).Return(testFacts);
			mockContext.Stub(x => x.eHubServiceProviders).Return(testProviders);
			mockContext.Stub(x => x.eHubClientRegistrations).Return(testClientRegos);
			mockContext.Stub(x => x.eHubServiceOperatorRegistrations).Return(testOperRegos);

			var regTypeClient = new eHubRegistrationType { RT_ID = "GTNEXUS", RT_RegistrantType = "Client" };
			testClientRegos.Add(new eHubClientRegistration
			{
				eHubRegistrationType = regTypeClient,
				eHubClient = new eHubClient { CC_ID = "TESTSENDER__1" },
				CX_Code = "XXX"
			});
			var regTypeOper = new eHubRegistrationType { RT_ID = "INTTRA_SCAC", RT_RegistrantType = "ServiceOperator" };
			testOperRegos.Add(new eHubServiceOperatorRegistration
			{
				eHubRegistrationType = regTypeOper,
				eHubServiceOperator = new eHubServiceOperator { SO_ID = "AAAA" },
				SR_Code = "YYY"
			});

			var clientGTNexus = new eHubClient { CC_ID = "GTNEXUS" };
			var clientInttra = new eHubClient { CC_ID = "INTTRA_SI" };
			var clientService = new eHubClient { CC_ID = "SHIPPING_INSTRUCTION" };
			var providerGTNexus = testProviders.Add(new eHubServiceProvider
			{
				eHubClient_Service = clientService,
				eHubClient_Provider = clientGTNexus,
				eHubServiceProviderRequiredRegistrations = new List<eHubServiceProviderRequiredRegistration>
				{
					new eHubServiceProviderRequiredRegistration
					{
						SX_LookupFactName = "SourceParty",
						eHubRegistrationType = regTypeClient
					}
				}
			});
			var providerInttra = testProviders.Add(new eHubServiceProvider
			{
				eHubClient_Service = clientService,
				eHubClient_Provider = clientInttra,
				eHubRoutingRule = testRules.Add(new eHubRoutingRule
				{
					RR_Group_MatchMultiple = false,
					eHubRoutingRules_Group = testRules.AddRange(new []
					{
						new eHubRoutingRule
						{
							RR_Condition_Expression = "[@ActionPurpose,Equal,WTH]",
							RR_Failed_ErrorCode = "IRJ",
							RR_Failed_ErrorDescription = "Shipping Instruction Withdrawal|not supported by carrier, contact carrier direct"
						}
					}).ToList(),
					eHubClient_Recipient = clientInttra
				}),
				eHubServiceProviderRequiredRegistrations = new List<eHubServiceProviderRequiredRegistration>
				{
					new eHubServiceProviderRequiredRegistration {
						SX_LookupFactName = "SCAC",
						eHubRegistrationType = regTypeOper
					}
				}
			});

			clientService.eHubRoutingRule = testRules.Add(new eHubRoutingRule
			{
				RR_Group_MatchMultiple = false,
				eHubRoutingRules_Group = testRules.AddRange(new[]
				{
					new eHubRoutingRule
					{
						RR_Condition_Expression = "[@SourceParty,Equal,TESTSENDER__1]",
						eHubServiceProvider = providerGTNexus,
						RR_Group_Ordering = 1000
					},
					new eHubRoutingRule
					{
						RR_Condition_Expression = "[@SCAC,Equal,AAAA]",
						eHubServiceProvider = providerInttra,
						RR_Group_Ordering = 2000
					}
				}).ToList(),
				eHubServiceProvider = providerInttra,
				RR_Failed_ErrorCode = "IRJ",
				RR_Failed_ErrorDescription = "Shipping Instruction|function not supported, contact WiseTechGlobal to setup service",
				eHubRoutingRuleFacts = new List<eHubRoutingRuleFact> 
				{ 
					testFacts.Add(new eHubRoutingRuleFact { RX_Name = "SourceParty", RX_Type = "PROPERTY", RX_Query = "http://schemas.microsoft.com/BizTalk/2003/system-properties#SourceParty" }),
					testFacts.Add(new eHubRoutingRuleFact { RX_Name = "SCAC", RX_Type = "XPATH", RX_Query = "//s0:TransportLegCollection/s0:TransportLeg[s0:LegType/text()='Main']/s0:Carrier/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='CCC']/s0:Value/text()" }),
					testFacts.Add(new eHubRoutingRuleFact { RX_Name = "ActionPurpose", RX_Type = "XPATH", RX_Query = "//s0:Workflow/s0:ActionPurpose/text()" }),
				}
			});
			clientService.eHubRoutingRule.eHubClients = new List<eHubClient> { clientService };
			clientService.eHubServiceProviders_Service.Add(providerGTNexus);
			clientService.eHubServiceProviders_Service.Add(providerInttra);

			var rule = Rule.GetForReading(clientService);

			rule.Facts[0].Value = "TESTSENDER__1";
			rule.Facts[1].Value = "AAAA";
			rule.Facts[2].Value = "NEW";

			var result = rule.Evaluate(mockContext, new IFactResolver[] { mockPropertyResolver, mockXpathResolver, mockSqlResolver }, mockLogger);

			rule.Facts[0].Value = "TESTSENDER__2";
			rule.Facts[2].Value = "WTH";

			result = rule.Evaluate(mockContext, new IFactResolver[] { mockPropertyResolver, mockXpathResolver, mockSqlResolver }, mockLogger);

			rule.Facts[1].Value = "BBBB";

			result = rule.Evaluate(mockContext, new IFactResolver[] { mockPropertyResolver, mockXpathResolver, mockSqlResolver }, mockLogger);

			CollectionAssert.AreEqual(new[] {
				"[INFO ] Evaluating rule for client 'SHIPPING_INSTRUCTION'",
				"[INFO ] Resolved facts for evaluation: [@SourceParty=TESTSENDER__1], [@SCAC=AAAA], [@ActionPurpose=NEW]",
				"[DEBUG] Evaluating group rule.",
				"[DEBUG] Evaluating condition rule: [@SourceParty,Equal,TESTSENDER__1]",
				"[TRACE] Parsing expression text: [@SourceParty,Equal,TESTSENDER__1]",
				"[TRACE] Parsed condition: (facts.Item[\"@SourceParty\"] == \"TESTSENDER__1\")",
				"[TRACE] Parsed expression: (facts.Item[\"@SourceParty\"] == \"TESTSENDER__1\")",
				"[DEBUG] Condition rule evaluated to 'true'",
				"[DEBUG] Evaluating provider 'GTNEXUS'",
				"[DEBUG] Provider evaluation results = [ { Recipient: 'GTNEXUS' } ]",
				"[DEBUG] Group rule evaluated results = [ { Recipient: 'GTNEXUS' } ]",
				"[INFO ] Rule evaluation results = [ { Recipient: 'GTNEXUS' } ]",
				"[DEBUG] Finished evaluating rule for client 'SHIPPING_INSTRUCTION'",
				"[INFO ] Evaluating rule for client 'SHIPPING_INSTRUCTION'",
				"[INFO ] Resolved facts for evaluation: [@SourceParty=TESTSENDER__2], [@SCAC=AAAA], [@ActionPurpose=WTH]",
				"[DEBUG] Evaluating group rule.",
				"[DEBUG] Evaluating condition rule: [@SourceParty,Equal,TESTSENDER__1]",
				"[DEBUG] Condition rule evaluated to 'false'",
				"[DEBUG] Evaluating condition rule: [@SCAC,Equal,AAAA]",
				"[TRACE] Parsing expression text: [@SCAC,Equal,AAAA]",
				"[TRACE] Parsed condition: (facts.Item[\"@SCAC\"] == \"AAAA\")",
				"[TRACE] Parsed expression: (facts.Item[\"@SCAC\"] == \"AAAA\")",
				"[DEBUG] Condition rule evaluated to 'true'",
				"[DEBUG] Evaluating provider 'INTTRA_SI'",
				"[DEBUG] Evaluating Provider specific rules for 'INTTRA_SI'",
				"[DEBUG] Evaluating group rule.",
				"[DEBUG] Evaluating condition rule: [@ActionPurpose,Equal,WTH]",
				"[TRACE] Parsing expression text: [@ActionPurpose,Equal,WTH]",
				"[TRACE] Parsed condition: (facts.Item[\"@ActionPurpose\"] == \"WTH\")",
				"[TRACE] Parsed expression: (facts.Item[\"@ActionPurpose\"] == \"WTH\")",
				"[DEBUG] Condition rule evaluated to 'true'",
				"[DEBUG] Condition rule result = { ErrorCode: 'IRJ', ErrorDescription: 'Shipping Instruction Withdrawal|not supported by carrier, contact carrier direct' }",
				"[DEBUG] Group rule evaluated results = [ { ErrorCode: 'IRJ', ErrorDescription: 'Shipping Instruction Withdrawal|not supported by carrier, contact carrier direct' } ]",
				"[DEBUG] Provider evaluation results = [ { ErrorCode: 'IRJ', ErrorDescription: 'Shipping Instruction Withdrawal|not supported by carrier, contact carrier direct' } ]",
				"[DEBUG] Group rule evaluated results = [ { ErrorCode: 'IRJ', ErrorDescription: 'Shipping Instruction Withdrawal|not supported by carrier, contact carrier direct' } ]",
				"[INFO ] Rule evaluation results = [ { ErrorCode: 'IRJ', ErrorDescription: 'Shipping Instruction Withdrawal|not supported by carrier, contact carrier direct' } ]",
				"[DEBUG] Finished evaluating rule for client 'SHIPPING_INSTRUCTION'",
				"[INFO ] Evaluating rule for client 'SHIPPING_INSTRUCTION'",
				"[INFO ] Resolved facts for evaluation: [@SourceParty=TESTSENDER__2], [@SCAC=BBBB], [@ActionPurpose=WTH]",
				"[DEBUG] Evaluating group rule.",
				"[DEBUG] Evaluating condition rule: [@SourceParty,Equal,TESTSENDER__1]",
				"[DEBUG] Condition rule evaluated to 'false'",
				"[DEBUG] Evaluating condition rule: [@SCAC,Equal,AAAA]",
				"[DEBUG] Condition rule evaluated to 'false'",
				"[DEBUG] Evaluating provider 'INTTRA_SI'",
				"[DEBUG] Missing required registration for Provider 'INTTRA_SI'",
				"[DEBUG] Provider evaluation results = [  ]",
				"[DEBUG] Group rule evaluated results = [ { ErrorCode: 'IRJ', ErrorDescription: 'Shipping Instruction|function not supported, contact WiseTechGlobal to setup service' } ]",
				"[INFO ] Rule evaluation results = [ { ErrorCode: 'IRJ', ErrorDescription: 'Shipping Instruction|function not supported, contact WiseTechGlobal to setup service' } ]",
				"[DEBUG] Finished evaluating rule for client 'SHIPPING_INSTRUCTION'"
			}, logEntries, "Log output does not match expected value. Actual log:\r\n" + String.Join("\r\n", logEntries) + "\r\n");
		}

		[TestMethod]
		public void Rule_Evaluate_ComputedFacts()
		{
			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockPropertyResolver = MockRepository.GenerateMock<IFactResolver>();
			var mockXpathResolver = MockRepository.GenerateMock<IFactResolver>();
			var mockSqlResolver = MockRepository.GenerateMock<IFactResolver>();
			var logEntries = new List<string>();
			var mockLogger = MockRepository.GenerateMock<ILog>();
			InitialiseLoggerStubs(mockLogger, logEntries);
			var testRules = new TestDbSet<eHubRoutingRule>();
			var testFacts = new TestDbSet<eHubRoutingRuleFact>();
			mockContext.Stub(x => x.eHubRoutingRules).Return(testRules);
			mockContext.Stub(x => x.eHubRoutingRuleFacts).Return(testFacts);
			var clientInttra = new eHubClient() { CC_ID = "INTTRA_SI" };
			var clientService = new eHubClient()
			{
				CC_ID = "SHIPPING_INSTRUCTION",
				eHubRoutingRule = testRules.Add(new eHubRoutingRule
				{
					RR_Group_MatchMultiple = false,
					eHubRoutingRules_Group = testRules.AddRange(new[]
					{
						new eHubRoutingRule 
						{ 
							RR_Condition_Expression = "[@SourceParty,Equal,TESTSENDER__2]", 
							eHubClient_Recipient = clientInttra,
							RR_Group_Ordering = 1000
						},
						new eHubRoutingRule 
						{ 
							RR_Condition_Expression = "[@Sender,StartsWith,AA]", 
							eHubClient_Recipient = clientInttra,
							RR_Group_Ordering = 2000
						}
					}).ToList(),
					eHubRoutingRuleFacts = new List<eHubRoutingRuleFact> 
					{ 
						testFacts.Add(new eHubRoutingRuleFact { RX_Name = "SourceParty", RX_Type = "PROPERTY", RX_Query = "http://schemas.microsoft.com/BizTalk/2003/system-properties#SourceParty" }),
						testFacts.Add(new eHubRoutingRuleFact { RX_Name = "ForwardingAgent", RX_Type = "XPATH", RX_Query = "//s0:OrganizationAddressCollection/s0:OrganizationAddress[s0:AddressType/text()='ForwardingAgent']/s0:RegistrationNumberCollection/s0:RegistrationNumber[s0:Type/text()='HID']/s0:Value/text()" }),
						testFacts.Add(new eHubRoutingRuleFact { RX_Name = "Sender", RX_Type = "COMPUTED", 
							eHubRoutingRule_Compute = testRules.Add(new eHubRoutingRule 
							{ 
								RR_Group_MatchMultiple = false,
								eHubRoutingRules_Group = testRules.AddRange(new[] { new eHubRoutingRule { RR_Condition_Expression = "[@ForwardingAgent,NotEqual,]", RR_Result_Value = "@ForwardingAgent" }}).ToList(),
								RR_Result_Value = "@SourceParty"
							}),
						})
					}
				})
			};
			mockPropertyResolver.Expect(x => x.Resolve(Arg<Fact[]>.Is.Anything)).Callback(new Func<Fact[], bool>(f => f.Count() == 2 && f[0].Name == "SourceParty" && f[1].Name == "ForwardingAgent")).Repeat.Twice();
			mockXpathResolver.Expect(x => x.Resolve(Arg<Fact[]>.Is.Anything)).Callback(new Func<Fact[], bool>(f => f.Count() == 2 && f[0].Name == "SourceParty" && f[1].Name == "ForwardingAgent")).Repeat.Twice();
			mockSqlResolver.Expect(x => x.Resolve(Arg<Fact[]>.Is.Anything)).Callback(new Func<Fact[], bool>(f => f.Count() == 2 && f[0].Name == "SourceParty" && f[1].Name == "ForwardingAgent")).Repeat.Twice();

			var rule = Rule.GetForReading(clientService);

			rule.Facts[0].Value = "TESTSENDER__1";
			rule.Facts[1].Value = "";

			var result1 = rule.Evaluate(mockContext, new IFactResolver[] { mockPropertyResolver, mockXpathResolver, mockSqlResolver }, mockLogger);

			rule.Facts[1].Value = "AAAA";

			var result2 = rule.Evaluate(mockContext, new IFactResolver[] { mockPropertyResolver, mockXpathResolver, mockSqlResolver }, mockLogger);

			mockPropertyResolver.VerifyAllExpectations();
			mockXpathResolver.VerifyAllExpectations();
			mockSqlResolver.VerifyAllExpectations();

			CollectionAssert.AreEquivalent(new[] {
				"[INFO ] Evaluating rule for client 'SHIPPING_INSTRUCTION'",
				"[DEBUG] Raw facts: [@SourceParty=TESTSENDER__1], [@ForwardingAgent=]",
				"[DEBUG] Evaluating group rule.",
				"[DEBUG] Evaluating condition rule: [@ForwardingAgent,NotEqual,]",
				"[TRACE] Parsing expression text: [@ForwardingAgent,NotEqual,]",
				"[TRACE] Parsed condition: (facts.Item[\"@ForwardingAgent\"] != \"\")",
				"[TRACE] Parsed expression: (facts.Item[\"@ForwardingAgent\"] != \"\")",
				"[DEBUG] Condition rule evaluated to 'false'",
				"[DEBUG] Group rule evaluated results = [ { Value: 'TESTSENDER__1' } ]",
				"[DEBUG] Computed facts: [@Sender=TESTSENDER__1]",
				"[INFO ] Resolved facts for evaluation: [@SourceParty=TESTSENDER__1], [@ForwardingAgent=], [@Sender=TESTSENDER__1]",
				"[DEBUG] Evaluating group rule.",
				"[DEBUG] Evaluating condition rule: [@SourceParty,Equal,TESTSENDER__2]",
				"[TRACE] Parsing expression text: [@SourceParty,Equal,TESTSENDER__2]",
				"[TRACE] Parsed condition: (facts.Item[\"@SourceParty\"] == \"TESTSENDER__2\")",
				"[TRACE] Parsed expression: (facts.Item[\"@SourceParty\"] == \"TESTSENDER__2\")",
				"[DEBUG] Condition rule evaluated to 'false'",
				"[DEBUG] Evaluating condition rule: [@Sender,StartsWith,AA]",
				"[TRACE] Parsing expression text: [@Sender,StartsWith,AA]",
				"[TRACE] Parsed condition: facts.Item[\"@Sender\"].StartsWith(\"AA\")",
				"[TRACE] Parsed expression: facts.Item[\"@Sender\"].StartsWith(\"AA\")",
				"[DEBUG] Condition rule evaluated to 'false'",
				"[DEBUG] Group rule evaluated results = [  ]",
				"[INFO ] Rule evaluation results = [  ]",
				"[DEBUG] Finished evaluating rule for client 'SHIPPING_INSTRUCTION'",
				"[INFO ] Evaluating rule for client 'SHIPPING_INSTRUCTION'",
				"[DEBUG] Raw facts: [@SourceParty=TESTSENDER__1], [@ForwardingAgent=AAAA]",
				"[DEBUG] Evaluating group rule.",
				"[DEBUG] Evaluating condition rule: [@ForwardingAgent,NotEqual,]",
				"[DEBUG] Condition rule evaluated to 'true'",
				"[DEBUG] Condition rule result = { Value: 'AAAA' }",
				"[DEBUG] Group rule evaluated results = [ { Value: 'AAAA' } ]",
				"[DEBUG] Computed facts: [@Sender=AAAA]",
				"[INFO ] Resolved facts for evaluation: [@SourceParty=TESTSENDER__1], [@ForwardingAgent=AAAA], [@Sender=AAAA]",
				"[DEBUG] Evaluating group rule.",
				"[DEBUG] Evaluating condition rule: [@SourceParty,Equal,TESTSENDER__2]",
				"[DEBUG] Condition rule evaluated to 'false'",
				"[DEBUG] Evaluating condition rule: [@Sender,StartsWith,AA]",
				"[DEBUG] Condition rule evaluated to 'true'",
				"[DEBUG] Condition rule result = { Recipient: 'INTTRA_SI' }",
				"[DEBUG] Group rule evaluated results = [ { Recipient: 'INTTRA_SI' } ]",
				"[INFO ] Rule evaluation results = [ { Recipient: 'INTTRA_SI' } ]",
				"[DEBUG] Finished evaluating rule for client 'SHIPPING_INSTRUCTION'"
			}, logEntries, "Log output does not match expected value. Actual log:\r\n" + String.Join("\r\n", logEntries) + "\r\n");
		}

		[TestMethod]
		public void Rule_Evaluiate_Invalid_RRConditionExpression_ShouldThrowRoutingRuleException()
		{
			var clientServiceId = "SHIPPING_INSTRUCTION";
			var invalidRRExpression = "@SourceParty,Equal,TESTSENDER__1";

			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockPropertyResolver = MockRepository.GenerateMock<IFactResolver>();
			var mockXpathResolver = MockRepository.GenerateMock<IFactResolver>();
			var mockSqlResolver = MockRepository.GenerateMock<IFactResolver>();
			var logEntries = new List<string>();
			var mockLogger = MockRepository.GenerateMock<ILog>();
			var testRules = new TestDbSet<eHubRoutingRule>();
			var testFacts = new TestDbSet<eHubRoutingRuleFact>();
			var clientInttra = new eHubClient() { CC_ID = "INTTRA_SI" };
			var clientService = new eHubClient { CC_ID = clientServiceId };
			var providerInttra = new eHubServiceProvider { eHubClient_Service = clientService, eHubClient_Provider = clientInttra };

			mockContext.Stub(x => x.eHubRoutingRules).Return(testRules);
			mockContext.Stub(x => x.eHubRoutingRuleFacts).Return(testFacts);

			clientService.eHubRoutingRule = testRules.Add(new eHubRoutingRule
			{
				RR_Group_MatchMultiple = true,
				eHubRoutingRules_Group = testRules.AddRange(new[]
				{
					new eHubRoutingRule
						{
							RR_Condition_Expression = invalidRRExpression,
							eHubServiceProvider = providerInttra,
							RR_Group_Ordering = 1000
						}
				}).ToList(),
				eHubRoutingRuleFacts = new List<eHubRoutingRuleFact>
				{
					testFacts.Add(new eHubRoutingRuleFact { RX_Name = "SourceParty", RX_Type = "PROPERTY", RX_Query = "http://schemas.microsoft.com/BizTalk/2003/system-properties#SourceParty" })
				}
			});
			clientService.eHubServiceProviders_Service = new List<eHubServiceProvider> { providerInttra };
			var rule = Rule.GetForReading(clientService);
			rule.Facts[0].Value = "TESTSENDER__1";

			try
            {
				var result1 = rule.Evaluate(mockContext, new IFactResolver[] { mockPropertyResolver, mockXpathResolver, mockSqlResolver }, mockLogger);
			}
			catch (RoutingRuleException ex)
            {
				Assert.AreEqual(ex.Message, String.Format("Routing Rule Engine: Exception evaluating rule for client '{0}' - Invalid Routing Rule Condition Expression – {1}. Regex match with condition @ ^\\[.+?,.+?,.*?\\] is not successful", clientServiceId, invalidRRExpression));
            }
		}

		[TestMethod]
		public void Rule_Evaluiate_InValid_ComplexRRConditionExpression_ShouldThrowRoutingRuleException()
		{
			var clientServiceId = "SHIPPING_INSTRUCTION";
			var invalidRRExpression = "([@SourceParty,StartsWith,TEST] && (@DestinationParty,Equal,TESTSENDER__1)) || [@SourceParty,Contains,1]";

			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockPropertyResolver = MockRepository.GenerateMock<IFactResolver>();
			var mockXpathResolver = MockRepository.GenerateMock<IFactResolver>();
			var mockSqlResolver = MockRepository.GenerateMock<IFactResolver>();
			var logEntries = new List<string>();
			var mockLogger = MockRepository.GenerateMock<ILog>();
			var testRules = new TestDbSet<eHubRoutingRule>();
			var testFacts = new TestDbSet<eHubRoutingRuleFact>();
			var clientInttra = new eHubClient() { CC_ID = "INTTRA_SI" };
			var clientService = new eHubClient { CC_ID = clientServiceId };
			var providerInttra = new eHubServiceProvider { eHubClient_Service = clientService, eHubClient_Provider = clientInttra };

			mockContext.Stub(x => x.eHubRoutingRules).Return(testRules);
			mockContext.Stub(x => x.eHubRoutingRuleFacts).Return(testFacts);

			clientService.eHubRoutingRule = testRules.Add(new eHubRoutingRule
			{
				RR_Group_MatchMultiple = true,
				eHubRoutingRules_Group = testRules.AddRange(new[]
				{
					new eHubRoutingRule
						{
							RR_Condition_Expression = invalidRRExpression,
							eHubServiceProvider = providerInttra,
							RR_Group_Ordering = 1000
						}
				}).ToList(),
				eHubRoutingRuleFacts = new List<eHubRoutingRuleFact>
				{
					testFacts.Add(new eHubRoutingRuleFact { RX_Name = "SourceParty", RX_Type = "PROPERTY", RX_Query = "http://schemas.microsoft.com/BizTalk/2003/system-properties#SourceParty" }),
					testFacts.Add(new eHubRoutingRuleFact { RX_Name = "DestinationParty", RX_Type = "PROPERTY", RX_Query = "http://schemas.microsoft.com/BizTalk/2003/system-properties#SourceParty" })
				}
			});
			clientService.eHubServiceProviders_Service = new List<eHubServiceProvider> { providerInttra };
			var rule = Rule.GetForReading(clientService);
			rule.Facts[0].Value = "TESTSENDER__1";
			rule.Facts[1].Value = "TESTSENDER__1";

			var exceptionMessage = "";
			try
			{
				var result1 = rule.Evaluate(mockContext, new IFactResolver[] { mockPropertyResolver, mockXpathResolver, mockSqlResolver }, mockLogger);
			}
			catch (RoutingRuleException ex)
			{
				exceptionMessage = ex.Message;
				Assert.AreEqual(ex.Message, String.Format("Routing Rule Engine: Exception evaluating rule for client '{0}' - Invalid Routing Rule Condition Expression – {1}. Regex match with condition @ ^\\[.+?,.+?,.*?\\] is not successful", clientServiceId, invalidRRExpression));
			}
			Assert.AreNotEqual(exceptionMessage, "");
		}

		[TestMethod]
		public void Rule_Evaluate_ServiceProviderNotRegisteredAndAllowFallback_ShouldFallbackNextGroupRecipient()
		{
			const bool allowFallback = true;

			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockPropertyResolver = MockRepository.GenerateMock<IFactResolver>();
			var mockXpathResolver = MockRepository.GenerateMock<IFactResolver>();
			var mockSqlResolver = MockRepository.GenerateMock<IFactResolver>();
			var logEntries = new List<string>();
			var mockLogger = MockRepository.GenerateMock<ILog>();
			InitialiseLoggerStubs(mockLogger, logEntries);
			var testRules = new TestDbSet<eHubRoutingRule>();
			var testFacts = new TestDbSet<eHubRoutingRuleFact>();
			var testProviders = new TestDbSet<eHubServiceProvider>();
			var testClientRegos = new TestDbSet<eHubClientRegistration>();
			mockContext.Stub(x => x.eHubRoutingRules).Return(testRules);
			mockContext.Stub(x => x.eHubRoutingRuleFacts).Return(testFacts);
			mockContext.Stub(x => x.eHubServiceProviders).Return(testProviders);
			mockContext.Stub(x => x.eHubClientRegistrations).Return(testClientRegos);
			var testSender = new eHubClient { CC_ID = "TEST_SENDER" };

			var regTypeClient1 = new eHubRegistrationType { RT_ID = "ClientRT1", RT_RegistrantType = "Client" };
			testClientRegos.Add(new eHubClientRegistration
			{
				eHubRegistrationType = regTypeClient1,
				eHubClient = testSender,
				CX_Code = "XXX",
				CX_Qualifier = "Invalid_Qualifier",
			});
			var regTypeClient2 = new eHubRegistrationType { RT_ID = "ClientRT2", RT_RegistrantType = "Client" };
			testClientRegos.Add(new eHubClientRegistration
			{
				eHubRegistrationType = regTypeClient2,
				eHubClient = testSender,
				CX_Code = "YYY",
				CX_Qualifier = "Valid_Qualifier",
			});

			var firstProviderClient = new eHubClient { CC_ID = "FirstClient" };
			var fallbackProviderClient = new eHubClient { CC_ID = "FallbackClient" };
			var clientService = new eHubClient { CC_ID = "SHIPPING_INSTRUCTION" };

			var firstProvider = testProviders.Add(new eHubServiceProvider
			{
				eHubClient_Service = clientService,
				eHubClient_Provider = firstProviderClient,
				eHubServiceProviderRequiredRegistrations = new List<eHubServiceProviderRequiredRegistration>
				{
					new eHubServiceProviderRequiredRegistration
					{
						SX_LookupFactName = "SourceParty",
						eHubRegistrationType = regTypeClient1,
						SX_QualifierFactName = "EventBranch",
					}
				},
				SP_AllowFallback = allowFallback,
			});
			var fallbackProvider = testProviders.Add(new eHubServiceProvider
			{
				eHubClient_Service = clientService,
				eHubClient_Provider = fallbackProviderClient,
				eHubServiceProviderRequiredRegistrations = new List<eHubServiceProviderRequiredRegistration>
				{
					new eHubServiceProviderRequiredRegistration {
						SX_LookupFactName = "SourceParty",
						eHubRegistrationType = regTypeClient2,
						SX_QualifierFactName = "EventBranch"
					}
				},
				SP_AllowFallback = allowFallback,
			});

			var firstGroup = testRules.Add(new eHubRoutingRule
			{
				RR_Group_MatchMultiple = false,
				eHubRoutingRules_Group = testRules.AddRange(new[] { new eHubRoutingRule
				{
					RR_Condition_Expression = $"[@SourceParty,Equal,{testSender.CC_ID}]",
					eHubServiceProvider = firstProvider,
					RR_Group_Ordering = 1000
				} }).ToList(),
				RR_Group_Ordering = 1000,
			});
			var fallbackGroup = testRules.Add(new eHubRoutingRule
			{
				RR_Group_MatchMultiple = false,
				eHubRoutingRules_Group = testRules.AddRange(new[] { new eHubRoutingRule
				{
					RR_Condition_Expression = $"[@SourceParty,Equal,{testSender.CC_ID}]",
					eHubServiceProvider = fallbackProvider,
					RR_Group_Ordering = 1000
				} }).ToList(),
				RR_Group_Ordering = 2000,
			});

			clientService.eHubRoutingRule = testRules.Add(new eHubRoutingRule
			{
				RR_Group_MatchMultiple = false,
				eHubRoutingRules_Group = testRules.AddRange(new[] { firstGroup, fallbackGroup }).ToList(),
				eHubRoutingRuleFacts = new List<eHubRoutingRuleFact>
				{
					testFacts.Add(new eHubRoutingRuleFact { RX_Name = "SourceParty", RX_Type = "PROPERTY", RX_Query = "http://schemas.microsoft.com/BizTalk/2003/system-properties#SourceParty" }),
					testFacts.Add(new eHubRoutingRuleFact { RX_Name = "EventBranch", RX_Type = "XPATH", RX_Query = "//s0:Workflow/s0:ActionPurpose/text()" }),
				}
			});
			clientService.eHubRoutingRule.eHubClients = new List<eHubClient> { clientService };
			clientService.eHubServiceProviders_Service.Add(firstProvider);
			clientService.eHubServiceProviders_Service.Add(fallbackProvider);

			var rule = Rule.GetForReading(clientService);
			rule.Facts[0].Value = testSender.CC_ID;
			rule.Facts[1].Value = "Valid_Qualifier";

			var result = rule.Evaluate(mockContext, new IFactResolver[] { mockPropertyResolver, mockXpathResolver, mockSqlResolver }, mockLogger);

			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(fallbackProviderClient, result[0].Recipient);
			Assert.IsNull(result[0].Value);
			Assert.IsNull(result[0].ErrorCode);
			Assert.IsNull(result[0].ErrorDescription);
		}

		[TestMethod]
		public void Rule_Evaluate_ServiceProviderNotRegisteredAndNotAllowFallback_ShouldReturnRejectedResult()
		{
			const bool allowFallback = false;

			var mockContext = MockRepository.GenerateMock<eHubTransactionsContext>();
			var mockPropertyResolver = MockRepository.GenerateMock<IFactResolver>();
			var mockXpathResolver = MockRepository.GenerateMock<IFactResolver>();
			var mockSqlResolver = MockRepository.GenerateMock<IFactResolver>();
			var logEntries = new List<string>();
			var mockLogger = MockRepository.GenerateMock<ILog>();
			InitialiseLoggerStubs(mockLogger, logEntries);
			var testRules = new TestDbSet<eHubRoutingRule>();
			var testFacts = new TestDbSet<eHubRoutingRuleFact>();
			var testProviders = new TestDbSet<eHubServiceProvider>();
			var testClientRegos = new TestDbSet<eHubClientRegistration>();
			mockContext.Stub(x => x.eHubRoutingRules).Return(testRules);
			mockContext.Stub(x => x.eHubRoutingRuleFacts).Return(testFacts);
			mockContext.Stub(x => x.eHubServiceProviders).Return(testProviders);
			mockContext.Stub(x => x.eHubClientRegistrations).Return(testClientRegos);
			var testSender = new eHubClient { CC_ID = "TEST_SENDER" };

			var regTypeClient1 = new eHubRegistrationType { RT_ID = "ClientRT1", RT_RegistrantType = "Client" };
			testClientRegos.Add(new eHubClientRegistration
			{
				eHubRegistrationType = regTypeClient1,
				eHubClient = testSender,
				CX_Code = "XXX",
				CX_Qualifier = "Invalid_Qualifier",
			});
			var regTypeClient2 = new eHubRegistrationType { RT_ID = "ClientRT2", RT_RegistrantType = "Client" };
			testClientRegos.Add(new eHubClientRegistration
			{
				eHubRegistrationType = regTypeClient2,
				eHubClient = testSender,
				CX_Code = "YYY",
				CX_Qualifier = "Valid_Qualifier",
			});

			var firstProviderClient = new eHubClient { CC_ID = "FirstClient" };
			var fallbackProviderClient = new eHubClient { CC_ID = "FallbackClient" };
			var clientService = new eHubClient { CC_ID = "SHIPPING_INSTRUCTION" };

			var firstProvider = testProviders.Add(new eHubServiceProvider
			{
				eHubClient_Service = clientService,
				eHubClient_Provider = firstProviderClient,
				eHubServiceProviderRequiredRegistrations = new List<eHubServiceProviderRequiredRegistration>
				{
					new eHubServiceProviderRequiredRegistration
					{
						SX_LookupFactName = "SourceParty",
						eHubRegistrationType = regTypeClient1,
						SX_QualifierFactName = "EventBranch",
					}
				},
				SP_AllowFallback = allowFallback,
			});
			var fallbackProvider = testProviders.Add(new eHubServiceProvider
			{
				eHubClient_Service = clientService,
				eHubClient_Provider = fallbackProviderClient,
				eHubServiceProviderRequiredRegistrations = new List<eHubServiceProviderRequiredRegistration>
				{
					new eHubServiceProviderRequiredRegistration {
						SX_LookupFactName = "SourceParty",
						eHubRegistrationType = regTypeClient2,
						SX_QualifierFactName = "EventBranch"
					}
				},
				SP_AllowFallback = allowFallback,
			});

			var firstGroup = testRules.Add(new eHubRoutingRule
			{
				RR_Group_MatchMultiple = false,
				eHubRoutingRules_Group = testRules.AddRange(new[] { new eHubRoutingRule
				{
					RR_Condition_Expression = $"[@SourceParty,Equal,{testSender.CC_ID}]",
					eHubServiceProvider = firstProvider,
					RR_Group_Ordering = 1000
				} }).ToList(),
				RR_Group_Ordering = 1000,
			});
			var fallbackGroup = testRules.Add(new eHubRoutingRule
			{
				RR_Group_MatchMultiple = false,
				eHubRoutingRules_Group = testRules.AddRange(new[] { new eHubRoutingRule
				{
					RR_Condition_Expression = $"[@SourceParty,Equal,{testSender.CC_ID}]",
					eHubServiceProvider = fallbackProvider,
					RR_Group_Ordering = 1000
				} }).ToList(),
				RR_Group_Ordering = 2000,
			});

			clientService.eHubRoutingRule = testRules.Add(new eHubRoutingRule
			{
				RR_Group_MatchMultiple = false,
				eHubRoutingRules_Group = testRules.AddRange(new[] { firstGroup, fallbackGroup }).ToList(),
				eHubRoutingRuleFacts = new List<eHubRoutingRuleFact>
				{
					testFacts.Add(new eHubRoutingRuleFact { RX_Name = "SourceParty", RX_Type = "PROPERTY", RX_Query = "http://schemas.microsoft.com/BizTalk/2003/system-properties#SourceParty" }),
					testFacts.Add(new eHubRoutingRuleFact { RX_Name = "EventBranch", RX_Type = "XPATH", RX_Query = "//s0:Workflow/s0:ActionPurpose/text()" }),
				}
			});
			clientService.eHubRoutingRule.eHubClients = new List<eHubClient> { clientService };
			clientService.eHubServiceProviders_Service.Add(firstProvider);
			clientService.eHubServiceProviders_Service.Add(fallbackProvider);

			var rule = Rule.GetForReading(clientService);
			rule.Facts[0].Value = testSender.CC_ID;
			rule.Facts[1].Value = "Valid_Qualifier";

			var result = rule.Evaluate(mockContext, new IFactResolver[] { mockPropertyResolver, mockXpathResolver, mockSqlResolver }, mockLogger);

			Assert.AreEqual(1, result.Count);
			Assert.IsNull(result[0].Recipient);
			Assert.IsNull(result[0].Value);
			Assert.AreEqual("IRJ", result[0].ErrorCode);
			Assert.AreEqual("Department=WiseTechGlobal|Reason=You are not registered with this FirstClient for this message type. Contact WTG to register.", result[0].ErrorDescription);
		}
	}
}
