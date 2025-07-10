using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.GUI;
using Enterprise.Registry.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WiseRates.Api.Model;
using static Enterprise.Core.Constants;
using static Enterprise.Rating.Business.RatingConstants;
using RateMode = Enterprise.Core.Constants.RateMode;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class GatewayBillingWithIntercompanyTariffsIntegrationTest : BaseRatingIntegrationTest
	{
		#region Test Autorate Revenue

		public void TestAutorateRevenue_BaseSendingAgent()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var intercompanyTariff = Helper.NewIntercompanyTariff(GlbCompany.CurrentCompany.OrgProxy);
			CreateFlatRate(intercompanyTariff, "FRT", 33m, "NZAKL", "AUSYD");

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: GlbCompany.CurrentCompany.OrgProxy, receivingAgent: null);
			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", false);

			Factory.Save();

			AssertEquals("Shipment's gateways should contain consol sending agent", consol.SendingForwarder.PK, shipment.Gateways[0].Forwarder.PK);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 33m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, GlbCompany.CurrentCompany.OrgProxy, autorateRevenue: true, autorateCosts: false);
		}

		public void TestAutorateRevenue_BaseReceivingAgent()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var intercompanyTariff = Helper.NewIntercompanyTariff(GlbCompany.CurrentCompany.OrgProxy);
			CreateFlatRate(intercompanyTariff, "FRT", 33m, "NZAKL", "AUSYD");

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: null, receivingAgent: GlbCompany.CurrentCompany.OrgProxy);
			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", true);

			Factory.Save();

			AssertEquals("Shipment's gateways should contain consol receiving agent", consol.ReceivingForwarder.PK, shipment.Gateways[0].Forwarder.PK);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 33m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, null, consol.ReceivingForwarder, autorateRevenue: true, autorateCosts: false);
		}

		public void TestAutorateRevenue_BaseSendingAndReceivingAgentForPrepaidConsol()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			Helper.ChargeCodes.CreateGlobalCharge("FRT2");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateBranchProxy("USCHS");
			Factory.Save();

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(sendingIntercompanyTariff, "FRT1", 66m, "NZAKL", "AUSYD", gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT2", 33m, "NZAKL", "AUSYD", gatewayAgentType: "");

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "NZAKL", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			//SAG Gateway Agent Type is applied only for Consol = PPD and Service Provider = Sending Agent
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", true);

			Factory.Save();

			AssertEquals("Consol should be Prepaid", PaymentType.Prepaid, consol.JK_PrepaidCollect);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT2",
					JR_OSSellAmt = 33m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 66m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, consol.SendingForwarder, autorateRevenue: true, autorateCosts: false);
		}

		public void TestAutorateRevenue_BaseSendingAndReceivingAgentForCollectConsol()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			Helper.ChargeCodes.CreateGlobalCharge("FRT2");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateBranchProxy("USCHS");
			Factory.Save();

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(sendingIntercompanyTariff, "FRT1", 66m, "NZAKL", "AUSYD", gatewayAgentType: "");
			CreateFlatRate(receivingIntercompanyTariff, "FRT2", 33m, "NZAKL", "AUSYD", gatewayAgentType: GatewayAgentType.Codes.ReceivingAgent);

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "NZAKL", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			//RAG Gateway Agent Type is applied only for Consol = CCX and Service Provider = Receiving Agent
			consol.JK_PrepaidCollect = PaymentType.Collect;

			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", true);

			Factory.Save();

			AssertEquals("Consol should be Collect", PaymentType.Collect, consol.JK_PrepaidCollect);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT2",
					JR_OSSellAmt = 33m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 66m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, consol.SendingForwarder, autorateRevenue: true, autorateCosts: false);
		}

		public void TestAutorateRevenue_WhenSendingRateProviderIsSetAsReceivingOrViceVersa_ShouldNotBringRates()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			Helper.ChargeCodes.CreateGlobalCharge("FRT2");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateBranchProxy("USCHS");
			Factory.Save();

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(sendingIntercompanyTariff, "FRT1", 66m, "NZAKL", "AUSYD", gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT2", 33m, "NZAKL", "AUSYD", gatewayAgentType: GatewayAgentType.Codes.ReceivingAgent);

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: receivingAgent.proxy, receivingAgent: sendingAgent);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "NZAKL", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			CreateGatewayShipment(consol, "NZAKL", "AUSYD", true);

			Factory.Save();

			var expected = Array.Empty<AssertionCharge>();

			AutorateAndAssertWithUserContextOverride(expected, consol, consol.SendingForwarder, autorateRevenue: true, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(consol,
				"Information: RateEntry Filtered Intercompany Tariff PROXYUSCHS reason: Gateway Agent Type RAG can only be used for Consol's sending agent.",
				"Information: RateEntry Filtered Intercompany Tariff EDICUS reason: Gateway Agent Type SAG can only be used for Consol's sending agent.");
		}

		public void TestAutorateRevenue_PossibleMatchesForm_ShouldNotBeTriggered()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var intercompanyTariff = Helper.NewIntercompanyTariff(GlbCompany.CurrentCompany.OrgProxy);
			var rateEntry = CreateFlatRate(intercompanyTariff, "FRT", 33m, "NZAKL", "AUSYD");
			var transportProvider = Factory.NewWithValidTestData<OrgHeader>();
			rateEntry.TI_OH_TransportProvider = transportProvider.PK;

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: GlbCompany.CurrentCompany.OrgProxy, receivingAgent: null);
			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", false);

			Factory.Save();

			AssertEquals("Shipment's gateways should contain consol sending agent", consol.SendingForwarder.PK, shipment.Gateways[0].Forwarder.PK);

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(form =>
			{
				//As per UPN, Autorating Notification popup is only applied to client rate/company tariff, so we are turning it off for Gateway Billing.
				//https://myaccount-portal.cargowise.com/my-account/Documents/UpdateNotes/CargoWiseUpdateNote20130525b.pdf
				AssertEquals("PossibleMatchesForm should not display", false, form is PossibleMatchesForm);
			});

			AutorateAndAssertWithUserContextOverride
			(
				expected: Array.Empty<AssertionCharge>(),
				consol,
				GlbCompany.CurrentCompany.OrgProxy,
				autorateRevenue: true,
				autorateCosts: false
			);

			AssertNull("PossibleMatchesForm should not display", ZFormModaliser.LastFormShownDialogForTest as PossibleMatchesForm);
		}

		public void TestAutorateRevenue_ShouldNotFindRateWhenRateIsForReceivingAndConsolSetForSendingAgent()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var intercompanyTariff = Helper.NewIntercompanyTariff(GlbCompany.CurrentCompany.OrgProxy);
			CreateFlatRate(intercompanyTariff, "FRT", 33m, "NZAKL", "AUSYD", gatewayAgentType: GatewayAgentType.Codes.ReceivingAgent);

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: GlbCompany.CurrentCompany.OrgProxy, receivingAgent: null);
			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", false);

			Factory.Save();

			AssertEquals("Shipment's gateways should contain consol sending agent", consol.SendingForwarder.PK, shipment.Gateways[0].Forwarder.PK);

			var expected = Array.Empty<AssertionCharge>();

			AutorateAndAssertWithUserContextOverride(expected, consol, GlbCompany.CurrentCompany.OrgProxy, autorateRevenue: true, autorateCosts: false);

			AssertAutoratingAuditLogNoteContainsLines(consol,
				"Information: AUTORATING PROFIT SHARE FOR Shipment SHP00001",
				"Information: RatingHeader Found Intercompany Tariff EDICUS Entries: 1",
				"Information: RateEntry Filtered Intercompany Tariff EDICUS reason: Gateway Agent Type RAG can only be used for Consol's receiving agent.");
		}

		public void TestAutorateRevenue_ShouldNotFindRateWhenRateIsForSendingAndConsolSetForReceivingAgent()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var intercompanyTariff = Helper.NewIntercompanyTariff(GlbCompany.CurrentCompany.OrgProxy);
			CreateFlatRate(intercompanyTariff, "FRT", 33, "NZAKL", "AUSYD", gatewayAgentType: GatewayAgentType.Codes.SendingAgent);

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: null, receivingAgent: GlbCompany.CurrentCompany.OrgProxy);
			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", true);

			Factory.Save();

			AssertEquals("Shipment's gateways should contain consol receiving agent", consol.ReceivingForwarder.PK, shipment.Gateways[0].Forwarder.PK);

			var expected = Array.Empty<AssertionCharge>();

			AutorateAndAssertWithUserContextOverride(expected, consol, null, consol.ReceivingForwarder, autorateRevenue: true, autorateCosts: false);
		}

		public void TestAutorateRevenue_ShouldSetCreditorEmpty_WhenCalculateSellAmountIsZero()
		{
			var charge = Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			charge.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;

			var intercompanyTariff = Helper.NewIntercompanyTariff(GlbCompany.CurrentCompany.OrgProxy);
			CreateFlatRate(intercompanyTariff, "FRT1", 33, "NZAKL", "AUSYD");

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: GlbCompany.CurrentCompany.OrgProxy, null);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", true);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_LocalSellAmt = 33m,
					SellAccountCode = GlbCompany.CurrentCompany.OrgProxy.OH_Code,
					JR_LocalCostAmt = 0,
					CostAccountCode = string.Empty,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, null, null, autorateRevenue: true, autorateCosts: false);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAutorateRevenue_ShouldSetDebtor_WhenSellAmountGreaterThanZero()
		{
			ShouldSetDebtor_WhenSellAmountIsNotZero(33);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAutorateRevenue_ShouldSetDebtor_WhenSellAmountLessThanZero()
		{
			ShouldSetDebtor_WhenSellAmountIsNotZero(-50);
		}

		void ShouldSetDebtor_WhenSellAmountIsNotZero(ZDecimal amount)
		{
			var (agent, branch) = CreateCompanyAndBranchProxy("AUSYD");
			var melburneAgent = CreateBranchProxy("AUMEL", branch.Company);
			var (agent1, _) = CreateCompanyAndBranchProxy("NZAUK");
			var charge = Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			charge.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;

			var intercompanyTariff = Helper.NewIntercompanyTariff(agent);
			CreateFlatRate(intercompanyTariff, "FRT1", amount, "AUSYD", "NZAKL");

			var consol = CreateForwardingConsolWithGatewayAgents("AUSYD", "NZAKL", sendingAgent: agent, receivingAgent: agent1, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "AUSYD", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment = CreateGatewayShipment(consol, "AUSYD", "NZAKL", true, consignor: melburneAgent.proxy, consignee: melburneAgent.proxy, branch: branch);
			shipment.PickupAgentPK = melburneAgent.proxy.PK;
			shipment.ConsignorPickupAddress.OrganisationPK = melburneAgent.proxy.PK;

			Factory.Save();

			var configuration = new GatewayChargeDefaultDebtorConfiguration()
			{
				ConsolDirection = "ALL",
				ConsolTransportMode = "ALL",
				ChargeGroup = "ALL",
				ConsolPaymentTerm = "ALL",
				RelatedJob = GatewayRelatedJob.Codes.RelatedToShipment,
				PreviousSendingAgent = GatewayPreviousSendingAgent.Codes.NoPrevSendingAgent,
				Debtor = GatewayDebtor.Codes.ShipmentPickupAgent,
			};

			var configuration1 = new GatewayChargeDefaultDebtorConfiguration()
			{
				ConsolDirection = "ALL",
				ConsolTransportMode = "ALL",
				ChargeGroup = "ALL",
				ConsolPaymentTerm = "ALL",
				RelatedJob = GatewayRelatedJob.Codes.RelatedToShipment,
				PreviousSendingAgent = GatewayPreviousSendingAgent.Codes.SendingAgent,
				Debtor = GatewayDebtor.Codes.ShipmentPickupAgent,
			};

			var configuration2 = new GatewayChargeDefaultDebtorConfiguration()
			{
				ConsolDirection = "ALL",
				ConsolTransportMode = "ALL",
				ChargeGroup = "ALL",
				ConsolPaymentTerm = "ALL",
				RelatedJob = GatewayRelatedJob.Codes.RelatedToShipment,
				PreviousSendingAgent = GatewayPreviousSendingAgent.Codes.GatewayAgent,
				Debtor = GatewayDebtor.Codes.PreviousSendingAgent,
			};

			var configuration3 = new GatewayChargeDefaultDebtorConfiguration()
			{
				ConsolDirection = "ALL",
				ConsolTransportMode = "ALL",
				ChargeGroup = "ALL",
				ConsolPaymentTerm = "ALL",
				RelatedJob = GatewayRelatedJob.Codes.RelatedToShipment,
				PreviousSendingAgent = GatewayPreviousSendingAgent.Codes.GatewayAgentWithFT,
				Debtor = GatewayDebtor.Codes.PreviousSendingAgent,
			};

			var configuration4 = new GatewayChargeDefaultDebtorConfiguration()
			{
				ConsolDirection = "ALL",
				ConsolTransportMode = "ALL",
				ChargeGroup = "ALL",
				ConsolPaymentTerm = "ALL",
				RelatedJob = GatewayRelatedJob.Codes.NotRelatedToJob,
				PreviousSendingAgent = "ALL",
				Debtor = GatewayDebtor.Codes.ReceivingAgent,
			};

			var configuration5 = new GatewayChargeDefaultDebtorConfiguration()
			{
				ConsolDirection = "ALL",
				ConsolTransportMode = "ALL",
				ChargeGroup = ChargeCodeGroupList.Codes.Destination,
				ConsolPaymentTerm = "ALL",
				RelatedJob = GatewayRelatedJob.Codes.RelatedToShipment,
				PreviousSendingAgent = "ALL",
				Debtor = GatewayDebtor.Codes.ShipmentDeliveryAgent,
			};

			var collection = new GatewayChargeDefaultDebtorConfigurationCollection();
			collection.AddRange(configuration, configuration1, configuration2, configuration3, configuration4, configuration5);

			using (AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultDebtorConfiguration.SetTemporaryValue(
				Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT1",
						JR_LocalSellAmt = amount,
						SellAccountCode = melburneAgent.proxy.OH_Code,
						JR_LocalCostAmt = 0,
						CostAccountCode = string.Empty,
						RelatedJobNumber = shipment.JS_UniqueConsignRef
					}
				};

				using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					melburneAgent.proxy.OH_IsDebtor = true;
					Factory.Save();

					AutorateAndAssertWithUserContextOverride(expected, consol, agent, autorateCosts: false, autorateRevenue: true);
				}
			}
		}

		public void TestAutorateRevenue_ShouldSetCreditorDebtor_WhenICTHasExplicitZeroAmount()
		{
			var charge = Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			charge.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;

			var intercompanyTariff = Helper.NewIntercompanyTariff(GlbCompany.CurrentCompany.OrgProxy);
			CreateFlatRate(intercompanyTariff, "FRT1", 0, "NZAKL", "AUSYD");

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: GlbCompany.CurrentCompany.OrgProxy, null);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", true);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_LocalSellAmt = 0m,
					SellAccountCode = GlbCompany.CurrentCompany.OrgProxy.OH_Code,
					JR_LocalCostAmt = 0,
					CostAccountCode = GlbCompany.CurrentCompany.OrgProxy.OH_Code,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, null, null, autorateRevenue: true, autorateCosts: false);
		}

		public void TestAutorateRevenue_ShouldNotFindRateWhenShipmentGatewaysDoesNotContainSendingAgent()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var intercompanyTariff = Helper.NewIntercompanyTariff(GlbCompany.CurrentCompany.OrgProxy);
			CreateFlatRate(intercompanyTariff, "FRT", 33, "NZAKL", "AUSYD");

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: GlbCompany.CurrentCompany.OrgProxy, receivingAgent: null);
			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", false);

			shipment.Gateways.DeleteAll();

			Factory.Save();

			AssertEquals("Shipment's gateways should be empty", 0, shipment.Gateways.Count);

			var expected = Array.Empty<AssertionCharge>();

			AutorateAndAssertWithUserContextOverride(expected, consol, GlbCompany.CurrentCompany.OrgProxy, autorateRevenue: true, autorateCosts: false);

			var orgHeader = Helper.NewOrgHeader();
			var anotherGateway = shipment.Gateways.AddNew();
			anotherGateway.JSG_OA_ForwarderAddress = orgHeader.MainAddress.PK;

			Factory.Save();

			CombineAssertions("Gateway should not equal to consol sending agent", () =>
			{
				AssertEquals(orgHeader.PK, anotherGateway.Forwarder.PK);
				AssertNotEquals(consol.SendingForwarder.PK, anotherGateway.Forwarder.PK);
			});

			AutorateAndAssertWithUserContextOverride(expected, consol, GlbCompany.CurrentCompany.OrgProxy, autorateRevenue: true, autorateCosts: false);
		}

		public void TestAutorateRevenue_SendingGatewayAgentTakePriorityOverBlank()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var intercompanyTariff = Helper.NewIntercompanyTariff(GlbCompany.CurrentCompany.OrgProxy);
			CreateFlatRate(intercompanyTariff, "FRT", 33m, "NZAKL", "AUSYD");
			CreateFlatRate(intercompanyTariff, "FRT", 66m, "NZAKL", "AUSYD", gatewayAgentType: GatewayAgentType.Codes.SendingAgent);

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: GlbCompany.CurrentCompany.OrgProxy, receivingAgent: null);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", false);

			Factory.Save();

			AssertEquals("Consol should be Prepaid", PaymentType.Prepaid, consol.JK_PrepaidCollect);
			AssertEquals("First gateway should be equal to consol sending agent", consol.SendingForwarder.PK, shipment.Gateways[0].Forwarder.PK);
			AssertEquals("Precondition: First Gateway", (byte)1, shipment.Gateways[0].JSG_Sequence);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 66m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, GlbCompany.CurrentCompany.OrgProxy, autorateRevenue: true, autorateCosts: false);

			AssertAutoratingAuditLogNoteContainsLines(consol,
				"Information: AUTORATING PROFIT SHARE FOR Shipment SHP00001",
				"Information: RatingHeader Found Intercompany Tariff EDICUS Entries: 2",
				"Information: RateLine Filtered FRT-FLT-Intercompany Tariff EDICUS	reason:	overridden by FRT-FLT-Intercompany Tariff EDICUS by Gateway Agent Type comparer");
		}

		public void TestAutorateRevenue_OriginTakesPriorityOtherLocationFields()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT1");

			var sendingAgent = CreateBranchProxy("DEFRA");
			var receivingAgent = CreateBranchProxy("USCHS");

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent.proxy);
			CreateFlatRate(intercompanyTariff, "FRT1", 66m, "CHBSL", "");
			CreateFlatRate(intercompanyTariff, "FRT1", 44m, "", "USHOU", "DEBRE");

			var consol = CreateForwardingConsolWithGatewayAgents("DEBRE", "USCHS", sendingAgent: sendingAgent.proxy, receivingAgent: receivingAgent.proxy);
			var shipment = GetShipmentWithSendingReceivingAgents(consol);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 66m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, consol.SendingForwarder, null, sendingAgent.branch, autorateRevenue: true, autorateCosts: false);
		}

		public void TestAutorateRevenue_ShipmentPlannedLoadIsNOTUsedForSearchingAgainstRates()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT1");

			var sendingAgent = CreateBranchProxy("DEFRA");
			var receivingAgent = CreateBranchProxy("USCHS");

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent.proxy);
			CreateFlatRate(intercompanyTariff, "FRT1", 66m, "", "", "DEBRE");
			CreateFlatRate(intercompanyTariff, "FRT1", 44m, "CHBSL", "", "BEBRU");

			var consol = CreateForwardingConsolWithGatewayAgents("DEBRE", "USCHS", sendingAgent: sendingAgent.proxy, receivingAgent: receivingAgent.proxy);
			var shipment = GetShipmentWithSendingReceivingAgents(consol);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 66m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, consol.SendingForwarder, null, sendingAgent.branch, autorateRevenue: true, autorateCosts: false);
		}

		public void TestAutorateRevenue_ShipmentPlannedDischargeIsNOTUsedForSearchingAgainstRates()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT1");

			var sendingAgent = CreateBranchProxy("DEFRA");
			var receivingAgent = CreateBranchProxy("USCHS");

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent.proxy);
			CreateFlatRate(intercompanyTariff, "FRT1", 66m, "", "", "DEBRE");
			CreateFlatRate(intercompanyTariff, "FRT1", 44m, "CHBSL", "", plannedDischarge: "USLGB");

			var consol = CreateForwardingConsolWithGatewayAgents("DEBRE", "USCHS", sendingAgent: sendingAgent.proxy, receivingAgent: receivingAgent.proxy);
			var shipment = GetShipmentWithSendingReceivingAgents(consol);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 66m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, consol.SendingForwarder, null, sendingAgent.branch, autorateRevenue: true, autorateCosts: false);
		}

		public void TestAutorateRevenue_DestinationTakesPriorityOverPlannedLoadDischargeLocationFields()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT1");

			var sendingAgent = CreateBranchProxy("DEFRA");
			var receivingAgent = CreateBranchProxy("USCHS");

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent.proxy);
			CreateFlatRate(intercompanyTariff, "FRT1", 66m, "", "US");
			CreateFlatRate(intercompanyTariff, "FRT1", 44m, "", "", "DEBRE", "USCHS");

			var consol = CreateForwardingConsolWithGatewayAgents("DEBRE", "USCHS", sendingAgent: sendingAgent.proxy, receivingAgent: receivingAgent.proxy);
			var shipment = GetShipmentWithSendingReceivingAgents(consol);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 66m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, consol.SendingForwarder, null, sendingAgent.branch, autorateRevenue: true, autorateCosts: false);
		}

		public void TestAutorateRevenue_OriginTakesPriorityOverOtherLocationFieldsEvenDestinationHasMoreSpecificValue()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT1");

			var sendingAgent = CreateBranchProxy("DEFRA");
			var receivingAgent = CreateBranchProxy("USCHS");

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent.proxy);
			CreateFlatRate(intercompanyTariff, "FRT1", 66m, "CH", "");
			CreateFlatRate(intercompanyTariff, "FRT1", 44m, "", "USHOU");

			var consol = CreateForwardingConsolWithGatewayAgents("DEBRE", "USCHS", sendingAgent: sendingAgent.proxy, receivingAgent: receivingAgent.proxy);
			var shipment = GetShipmentWithSendingReceivingAgents(consol);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 66m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, consol.SendingForwarder, null, sendingAgent.branch, autorateRevenue: true, autorateCosts: false);
		}

		public void TestAutorateRevenue_PlannedLoadTakesPriorityOverPlannedDischarge()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT1");

			var sendingAgent = CreateBranchProxy("DEFRA");
			var receivingAgent = CreateBranchProxy("USCHS");

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent.proxy);
			CreateFlatRate(intercompanyTariff, "FRT1", 66m, "", "", plannedLoad: "DE");
			CreateFlatRate(intercompanyTariff, "FRT1", 44m, "", "", plannedDischarge: "USCHS");

			var consol = CreateForwardingConsolWithGatewayAgents("DEBRE", "USCHS", sendingAgent: sendingAgent.proxy, receivingAgent: receivingAgent.proxy);
			var shipment = GetShipmentWithSendingReceivingAgents(consol);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 66m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, consol.SendingForwarder, null, sendingAgent.branch, autorateRevenue: true, autorateCosts: false);
		}

		public void TestAutorateRevenue_OriginTakesPriorityOverOtherLocationFields()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT1");

			var sendingAgent = CreateBranchProxy("DEFRA");
			var receivingAgent = CreateBranchProxy("USCHS");

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent.proxy);
			CreateFlatRate(intercompanyTariff, "FRT1", 66m, "CHZRH", "");
			CreateFlatRate(intercompanyTariff, "FRT1", 44m, "", "USHOU");

			var consol = CreateForwardingConsolWithGatewayAgents("DEBRE", "USCHS", sendingAgent: sendingAgent.proxy, receivingAgent.proxy);
			var shipment = GetShipmentWithSendingReceivingAgents(consol);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 66m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, consol.SendingForwarder, null, sendingAgent.branch, autorateRevenue: true, autorateCosts: false);
		}

		public void TestAutorateRevenue_DestinationTakesPriorityOverPlannedLoadDischargeWithSpecificValue()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT1");

			var sendingAgent = CreateBranchProxy("DEFRA");
			var receivingAgent = CreateBranchProxy("USCHS");

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent.proxy);
			CreateFlatRate(intercompanyTariff, "FRT1", 66m, "", "USHOU");
			CreateFlatRate(intercompanyTariff, "FRT1", 44m, "", "", "DEBRE");

			var consol = CreateForwardingConsolWithGatewayAgents("DEBRE", "USCHS", sendingAgent: sendingAgent.proxy, receivingAgent: receivingAgent.proxy);
			var shipment = GetShipmentWithSendingReceivingAgents(consol);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 66m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, consol.SendingForwarder, null, sendingAgent.branch, autorateRevenue: true, autorateCosts: false);
		}

		public void TestAutorateRevenue_PlannedLoadTakesPriorityOverPlannedDischargeWithSpecificValue()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT1");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			sendingAgent.MainAddress.OA_RL_NKRelatedPortCode = "DEFRA";
			var receivingAgent = CreateBranchProxy("USCHS");

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(intercompanyTariff, "FRT1", 66m, "", "", "DEFRA");
			CreateFlatRate(intercompanyTariff, "FRT1", 44m, "", "", plannedDischarge: "USCHS");

			var consol = CreateForwardingConsolWithGatewayAgents("DEBRE", "USCHS", sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			var shipment = GetShipmentWithSendingReceivingAgents(consol);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 66m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, consol.SendingForwarder, autorateRevenue: true, autorateCosts: false);
		}

		public void TestAutorateRevenue_PlannedLoadDoesntMatchChargesOnNonGatewayShipment()
		{
			var clientRate = Helper.NewClientRate(Consignor);

			var rateEntryA = clientRate.AddRateEntry(RateCategory.FCL, RateMode.SEA, "AUSYD", "");
			rateEntryA.TI_PlannedLoadLRC = "AUMEL";
			rateEntryA.RateLines.RemoveAndDeleteAll();
			rateEntryA.AddFlatRateLine("FRT", 100);

			var rateEntryB = clientRate.AddRateEntry(RateCategory.FCL, RateMode.SEA, "AUSYD", "");
			rateEntryB.TI_PlannedLoadLRC = "AUBNE";
			rateEntryB.RateLines.RemoveAndDeleteAll();
			rateEntryB.AddFlatRateLine("FRT", 200);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "DEHAM";
			shipment.JS_RL_NKLoadPort = "AUBNE";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "DEHAM";

			Factory.Save();

			var expected = new[] { new AssertionCharge { ChargeCode = "FRT", JR_OSSellAmt = 200 } };
			AutorateAndAssert(expected, shipment, Consignor, autorateCosts: false);
		}

		public void TestAutorateRevenue_PlannedDischargeDoesntMatchChargesOnNonGatewayShipment()
		{
			var clientRate = Helper.NewClientRate(Consignor);

			var rateEntryA = clientRate.AddRateEntry(RateCategory.FCL, RateMode.SEA, "", "DEBER");
			rateEntryA.TI_PlannedDischargeLRC = "DEFRA";
			rateEntryA.RateLines.RemoveAndDeleteAll();
			rateEntryA.AddFlatRateLine("FRT", 100);

			var rateEntryB = clientRate.AddRateEntry(RateCategory.FCL, RateMode.SEA, "", "DEBER");
			rateEntryB.TI_PlannedDischargeLRC = "DEHAM";
			rateEntryB.RateLines.RemoveAndDeleteAll();
			rateEntryB.AddFlatRateLine("FRT", 200);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDestination = "DEBER";
			shipment.JS_RL_NKDischargePort = "DEFRA";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "DEHAM";

			Factory.Save();

			var expected = new[] { new AssertionCharge { ChargeCode = "FRT", JR_OSSellAmt = 100 } };
			AutorateAndAssert(expected, shipment, Consignor, autorateCosts: false);
		}

		public void TestAutorateRevenue_GatewayAgentTypeTakesPriorityOverLocationsChecking()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT1");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			sendingAgent.MainAddress.OA_RL_NKRelatedPortCode = "DEFRA";
			var receivingAgent = CreateBranchProxy("USCHS");

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(intercompanyTariff, "FRT1", 66m, "", "US", gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(intercompanyTariff, "FRT1", 44m, "CHBSL", "");

			var consol = CreateForwardingConsolWithGatewayAgents("DEBRE", "USCHS", sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			var shipment = GetShipmentWithSendingReceivingAgents(consol);

			Factory.Save();

			AssertEquals("Consol should be Prepaid", PaymentType.Prepaid, consol.JK_PrepaidCollect);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 66m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, consol.SendingForwarder, autorateRevenue: true, autorateCosts: false);
		}

		public void TestAutorateRevenue_SendingAgentPortTakesPriorityOverShipmentPlannedLoad()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT1");

			var sendingAgent = CreateBranchProxy("DEFRA");
			var receivingAgent = CreateBranchProxy("USCHS");

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent.proxy);
			CreateFlatRate(intercompanyTariff, "FRT1", 66m, "", "", "DEFRA");
			CreateFlatRate(intercompanyTariff, "FRT1", 44m, "", "", "BEBRU");

			var consol = CreateForwardingConsolWithGatewayAgents("DEBRE", "USCHS", sendingAgent: sendingAgent.proxy, receivingAgent: receivingAgent.proxy);
			var shipment = GetShipmentWithSendingReceivingAgents(consol);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 66m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, consol.SendingForwarder, null, sendingAgent.branch, autorateRevenue: true, autorateCosts: false);
		}

		public void TestAutorateRevenue_ConsolFirstLoadPortTakesPriorityOverSendingAgnetPort()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			var sendingAgent = CreateBranchProxy("DEFRA");
			var receivingAgent = CreateBranchProxy("USCHS");

			Factory.Save();

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent.proxy);
			CreateFlatRate(intercompanyTariff, "FRT1", 66m, "", "", "DEBRE");
			CreateFlatRate(intercompanyTariff, "FRT1", 44m, "", "", "DEFRA");

			var consol = CreateForwardingConsolWithGatewayAgents("DEBRE", "USCHS", sendingAgent: sendingAgent.proxy, receivingAgent: receivingAgent.proxy);
			var shipment = GetShipmentWithSendingReceivingAgents(consol);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 66m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, consol.SendingForwarder, null, sendingAgent.branch, autorateRevenue: true, autorateCosts: false);
		}

		public void TestAutorateRevenue_ClientRatesShouldNotBeLoaded()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;

			var clientRate = Helper.NewClientRate(sendingAgent);
			CreateFlatRate(clientRate, "FRT", 44m, "NZAKL", "AUSYD");

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(intercompanyTariff, "FRT", 33m, "NZAKL", "AUSYD");

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: sendingAgent, receivingAgent: null);
			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", false);

			Factory.Save();

			AssertEquals("Shipment's gateways should contain consol sending agent", consol.SendingForwarder.PK, shipment.Gateways[0].Forwarder.PK);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 33m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, sendingAgent, autorateRevenue: true, autorateCosts: false);

			AssertAutoratingAuditLogNotContains(consol, "Client Rate", "Any Client Rate should NOT have been loaded.");
		}

		public void TestAutorateRevenue_ClientRatesShouldNotBeLoaded_UnderGatewayInvoicingWhenLoginAsGTAWithExportShipment()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.HongKong))
			{
				Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
				CreateGlobalCharge("FRT");

				var sendingAgent = CreateBranchProxy("HKHKG");
				var receivingAgent = CreateBranchProxy("SGSIN");

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sendingAgent.branch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					sendingAgent.proxy.OH_IsCreditor = true;
					receivingAgent.proxy.OH_IsCreditor = true;

					var clientRate = Helper.NewClientRate(sendingAgent.proxy);
					CreateFlatRate(clientRate, "FRT", 44m, "HK", "");
				}

				var tariff = Helper.NewIntercompanyTariff(sendingAgent.proxy);
				CreateFlatRate(tariff, "FRT", 33m, "HK", "");

				TransportProvider1.OH_IsCreditor = true;

				var consol = CreateForwardingConsolWithGatewayAgents("HKHKG", "SGSIN", sendingAgent.proxy, receivingAgent.proxy, checkIsItGateway: true);
				var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("HKHKG", "CNSZX", true, consol);
				shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;

				Factory.Save();

				var expected = Array.Empty<AssertionCharge>();

				AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.branch, autorateRevenue: true, autorateCosts: false);
				AssertAutoratingAuditLogNotContains(consol, "Client Rate", "Any Client Rate should NOT have been loaded.");
			}
		}

		public void TestAutorateRevenue_IntercompanyTariffShouldBeLoaded_UnderGatewayInvoicingWhenLoginAsGTTWithDomesticShipment()
		{
			CreateGlobalCharge("FRT");

			var sendingAgent = CreateCompanyAndBranchProxy("HKHKG");
			var receivingAgent = CreateCompanyAndBranchProxy("AUSYD");

			var tariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);
			CreateFlatRate(tariff, "FRT", 33m, "AUBNE", "AUMEL");

			var consol = CreateForwardingConsolWithGatewayAgents("HKHKG", "AUSYD", sendingAgent.proxy, receivingAgent.proxy, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, "AUSYD", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("AUBNE", "AUMEL", true, consol);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge { ChargeCode = "FRT", JR_OSSellAmt = 33m },
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: receivingAgent.branch, autorateRevenue: true, autorateCosts: false);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAutorateRevenue_ControllingCustomer_WhenShipmentIsPrepaid_PriotiryIsControllingCustomer_LocalClient_ConsignorIFTRelatedParties_Consignor()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var (agent1, branch1) = CreateCompanyAndBranchProxy("ZAJNB");
			var (agent2, branch2) = CreateCompanyAndBranchProxy("HKHKG");
			var (agent3, _) = CreateCompanyAndBranchProxy("SGSIN");
			var (agent4, _) = CreateCompanyAndBranchProxy("THBKK");

			var orgIFC = Factory.NewWithValidTestData<OrgRelatedParty>();
			orgIFC.PR_PartyType = RelatedPartyTypeList.Codes.InvoiceFreightJobsTo;

			var orgICT = Factory.NewWithValidTestData<OrgRelatedParty>();
			orgICT.PR_PartyType = RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo;

			var intercompanyTariff = Helper.NewIntercompanyTariff(agent1);
			CreateFlatRate(intercompanyTariff, "FRT", 100m, "HKHKG", "SGSIN").TI_OH_ControllingCustomer = Consignor.PK;
			CreateFlatRate(intercompanyTariff, "FRT", 200m, "HKHKG", "SGSIN");

			var consol = CreateForwardingConsolWithGatewayAgents("ZAJNB", "HKHKG", sendingAgent: agent1, receivingAgent: agent2, checkIsItGateway: false);
			var consol2 = CreateForwardingConsolWithGatewayAgents("HKHKG", "SGSIN", agent2, agent3, checkIsItGateway: false);
			var consol3 = CreateForwardingConsolWithGatewayAgents("SGSIN", "THBKK", agent3, agent4, checkIsItGateway: false);

			var shipment = CreateGatewayShipment(consol, "HKHKG", "SGSIN", isCollect: false);
			shipment.Consols.Add(consol2);
			shipment.Consols.Add(consol3);

			shipment.Consignor.ConsignorRelatedParties.Add(orgIFC);
			shipment.Consignor.ConsignorRelatedParties.Add(orgICT);
			shipment.JobHeader.JH_OA_LocalChargesAddr = ZGuid.Empty;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var job = CreateJob(shipment, shipment.JS_UniqueConsignRef);
				shipment.JobHeader.JH_OA_LocalChargesAddr = agent2.MainAddress.PK;
				Factory.Save();
			}

			AssertEquals("Shipment should be Prepaid", shipment.IsPrepaid, true);
			AssertEquals("Shipment Controlling Customer should be empty", shipment.ControllingCustomerNameOrPK, Guid.Empty.ToString());

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 100m,
				},
			};

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("Shipment should be cross trade", shipment.IsCrossTrade(), true);
				AssertEquals(shipment.Consignor, Consignor);
				AutorateAndAssertWithUserContextOverride(expected, consol, agent1, autorateCosts: false, autorateRevenue: true);
			}

			CreateFlatRate(intercompanyTariff, "FRT", 80m, "HKHKG", "SGSIN").TI_OH_ControllingCustomer = orgIFC.PR_OH_RelatedParty;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 80m,
				},
			};

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("Shipment should be cross trade", shipment.IsCrossTrade(), true);
				AssertEquals(shipment.Consignor, Consignor);
				AutorateAndAssertWithUserContextOverride(expected, consol, agent1, autorateCosts: false, autorateRevenue: true);
			}

			CreateFlatRate(intercompanyTariff, "FRT", 60m, "HKHKG", "SGSIN").TI_OH_ControllingCustomer = agent2.PK;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 60m,
				},
			};

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("Shipment should be cross trade", shipment.IsCrossTrade(), true);
				AutorateAndAssertWithUserContextOverride(expected, consol, agent1, autorateCosts: false, autorateRevenue: true);
			}

			var anyCustomer2 = Helper.NewOrgHeader("ANY_CLIENT_2");
			CreateFlatRate(intercompanyTariff, "FRT", 40m, "HKHKG", "SGSIN").TI_OH_ControllingCustomer = anyCustomer2.PK;
			shipment.ControllingCustomerNameOrPK = anyCustomer2.PK.ToString();

			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 40m,
				},
			};

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("Shipment should be cross trade", shipment.IsCrossTrade(), true);
				AutorateAndAssertWithUserContextOverride(expected, consol, agent1, autorateCosts: false, autorateRevenue: true);
			}
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAutorateRevenue_ControllingCustomer_WhenShipmentIsCollect_PriotiryIsControllingCustomer_LocalClient_ConsigneeIFTRelatedParties_Consignee()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var (agent1, branch1) = CreateCompanyAndBranchProxy("ZAJNB");
			var (agent2, _) = CreateCompanyAndBranchProxy("HKHKG");
			var (agent3, branch3) = CreateCompanyAndBranchProxy("SGSIN");
			var (agent4, _) = CreateCompanyAndBranchProxy("THBKK");

			var orgIFC = Factory.NewWithValidTestData<OrgRelatedParty>();
			orgIFC.PR_PartyType = RelatedPartyTypeList.Codes.InvoiceFreightJobsTo;

			var orgICT = Factory.NewWithValidTestData<OrgRelatedParty>();
			orgICT.PR_PartyType = RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo;

			var intercompanyTariff = Helper.NewIntercompanyTariff(agent1);
			CreateFlatRate(intercompanyTariff, "FRT", 100m, "HKHKG", "SGSIN").TI_OH_ControllingCustomer = Consignee.PK;
			CreateFlatRate(intercompanyTariff, "FRT", 200m, "HKHKG", "SGSIN");

			var consol = CreateForwardingConsolWithGatewayAgents("ZAJNB", "HKHKG", sendingAgent: agent1, receivingAgent: agent2, checkIsItGateway: false);
			var consol2 = CreateForwardingConsolWithGatewayAgents("HKHKG", "SGSIN", agent2, agent3, checkIsItGateway: false);
			var consol3 = CreateForwardingConsolWithGatewayAgents("SGSIN", "THBKK", agent3, agent4, checkIsItGateway: false);

			var shipment = CreateGatewayShipment(consol, "HKHKG", "SGSIN", isCollect: true);
			shipment.Consols.Add(consol2);
			shipment.Consols.Add(consol3);

			shipment.Consignee.ConsigneeRelatedParties.Add(orgIFC);
			shipment.Consignee.ConsigneeRelatedParties.Add(orgICT);
			shipment.JobHeader.JH_OA_LocalChargesAddr = ZGuid.Empty;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch3.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var job = CreateJob(shipment, shipment.JS_UniqueConsignRef);
				shipment.JobHeader.JH_OA_LocalChargesAddr = agent3.MainAddress.PK;
				Factory.Save();
			}

			Factory.Save();

			AssertEquals("Shipment should be Collect", shipment.IsCollect, true);
			AssertEquals("Shipment Controlling Customer should be empty", shipment.ControllingCustomerNameOrPK, Guid.Empty.ToString());

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 100m,
				},
			};

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("Shipment should be cross trade", shipment.IsCrossTrade(), true);
				AssertEquals(shipment.Consignee, Consignee);
				AutorateAndAssertWithUserContextOverride(expected, consol, agent1, autorateCosts: false, autorateRevenue: true);
			}

			CreateFlatRate(intercompanyTariff, "FRT", 80m, "HKHKG", "SGSIN").TI_OH_ControllingCustomer = orgIFC.PR_OH_RelatedParty;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 80m,
				},
			};

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("Shipment should be cross trade", shipment.IsCrossTrade(), true);
				AssertEquals(shipment.Consignee, Consignee);
				AutorateAndAssertWithUserContextOverride(expected, consol, agent1, autorateCosts: false, autorateRevenue: true);
			}

			CreateFlatRate(intercompanyTariff, "FRT", 60m, "HKHKG", "SGSIN").TI_OH_ControllingCustomer = agent3.PK;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 60m,
				},
			};

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("Shipment should be cross trade", shipment.IsCrossTrade(), true);
				AutorateAndAssertWithUserContextOverride(expected, consol, agent1, autorateCosts: false, autorateRevenue: true);
			}

			var anyCustomer2 = Helper.NewOrgHeader("ANY_CLIENT_2");
			CreateFlatRate(intercompanyTariff, "FRT", 40m, "HKHKG", "SGSIN").TI_OH_ControllingCustomer = anyCustomer2.PK;
			shipment.ControllingCustomerNameOrPK = anyCustomer2.PK.ToString();

			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 40m,
				},
			};

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("Shipment should be cross trade", shipment.IsCrossTrade(), true);
				AutorateAndAssertWithUserContextOverride(expected, consol, agent1, autorateCosts: false, autorateRevenue: true);
			}
		}

		public void TestAutorateRevenue_ControllingCustomer_MatchesNeitherControllingCustomerNorLocalClientOnShipment_FallbackToPlainRate()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var anyCustomer1 = Helper.NewOrgHeader("ANY_CLIENT_1");
			var anyCustomer2 = Helper.NewOrgHeader("ANY_CLIENT_2");
			var controllingCustomer = Helper.NewOrgHeader("CONT_CLIENT");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(intercompanyTariff, "FRT", 100m, "NZAKL", "AUSYD").TI_OH_ControllingCustomer = controllingCustomer.PK;
			CreateFlatRate(intercompanyTariff, "FRT", 200m, "NZAKL", "AUSYD");

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: sendingAgent, receivingAgent: null);

			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", false);
			shipment.JobHeader.JH_OA_LocalChargesAddr = anyCustomer1.MainAddress.PK;
			shipment.ControllingCustomerNameOrPK = anyCustomer2.PK.ToString();
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 200m,
				},
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, sendingAgent, autorateCosts: false, autorateRevenue: true);
		}

		public void TestAutorateRevenue_ControllingCustomer_DoesNotMatchControllingCustomerAndThereIsNoLocalClientOnShipment_FallbackToPlainRate()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var anyCustomer1 = Helper.NewOrgHeader("ANY_CLIENT_1");
			var anyCustomer2 = Helper.NewOrgHeader("ANY_CLIENT_2");
			var controllingCustomer = Helper.NewOrgHeader("CONT_CLIENT");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(intercompanyTariff, "FRT", 100m, "NZAKL", "AUSYD").TI_OH_ControllingCustomer = controllingCustomer.PK;
			CreateFlatRate(intercompanyTariff, "FRT", 200m, "NZAKL", "AUSYD");

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: sendingAgent, receivingAgent: null);

			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", false);
			shipment.JobHeader.JH_OA_LocalChargesAddr = ZGuid.Empty;
			shipment.ControllingCustomerNameOrPK = anyCustomer2.PK.ToString();
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 200m,
				},
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, sendingAgent, autorateCosts: false, autorateRevenue: true);
		}

		public void TestAutorateRevenue_ControllingCustomer_MatchesShipmentLocalClient()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var localClient = Helper.NewOrgHeader("LOCALCLIENT");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(intercompanyTariff, "FRT", 100m, "NZAKL", "AUSYD").TI_OH_ControllingCustomer = localClient.PK;
			CreateFlatRate(intercompanyTariff, "FRT", 200m, "NZAKL", "AUSYD");

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: sendingAgent, receivingAgent: null);
			CreateGatewayShipment(consol, "NZAKL", "AUSYD", false).JobHeader.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 100m,
				},
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, sendingAgent, autorateCosts: false, autorateRevenue: true);
		}

		public void TestAutorateRevenue_ControllingCustomer_MatchesShipmentControllingCustomer()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var controllingCustomer = Helper.NewOrgHeader("CONT_CLIENT");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(intercompanyTariff, "FRT", 100m, "NZAKL", "AUSYD").TI_OH_ControllingCustomer = controllingCustomer.PK;
			CreateFlatRate(intercompanyTariff, "FRT", 200m, "NZAKL", "AUSYD");

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: sendingAgent, receivingAgent: null);
			CreateGatewayShipment(consol, "NZAKL", "AUSYD", false).ControllingCustomerNameOrPK = controllingCustomer.PK.ToString();
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 100m,
				},
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, sendingAgent, autorateCosts: false, autorateRevenue: true);
		}

		public void TestAutorateRevenue_ControllingCustomer_MatchesShipmentControllingCustomer_AsPriority()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var anyCustomer = Helper.NewOrgHeader("ANY_CLIENT_1");
			var controllingCustomer = Helper.NewOrgHeader("CONT_CLIENT");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(intercompanyTariff, "FRT", 100m, "NZAKL", "AUSYD").TI_OH_ControllingCustomer = controllingCustomer.PK;
			CreateFlatRate(intercompanyTariff, "FRT", 200m, "NZAKL", "AUSYD").TI_OH_ControllingCustomer = anyCustomer.PK;
			CreateFlatRate(intercompanyTariff, "FRT", 300m, "NZAKL", "AUSYD");

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: sendingAgent, receivingAgent: null);
			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", false);
			shipment.JobHeader.JH_OA_LocalChargesAddr = anyCustomer.MainAddress.PK;
			shipment.ControllingCustomerNameOrPK = controllingCustomer.PK.ToString();
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 100m,
				},
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, sendingAgent, autorateCosts: false, autorateRevenue: true);
		}

		public void TestAutorateRevenue_ControllingCustomer_MatchesShipmentControllingCustomer_WithAgentRate()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var controllingCustomer = Helper.NewOrgHeader("CONT_CLIENT");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(intercompanyTariff, "FRT", 100m, "NZAKL", "AUSYD").TI_OH_ControllingCustomer = controllingCustomer.PK;
			CreateFlatRate(intercompanyTariff, "FRT", 300m, "NZAKL", "AUSYD", gatewayAgentType: GatewayAgentType.Codes.SendingAgent).TI_OH_ControllingCustomer = controllingCustomer.PK;
			CreateFlatRate(intercompanyTariff, "FRT", 400m, "NZAKL", "AUSYD", gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(intercompanyTariff, "FRT", 200m, "NZAKL", "AUSYD");

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: sendingAgent, receivingAgent: null);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", false);
			shipment.ControllingCustomerNameOrPK = controllingCustomer.PK.ToString();
			Factory.Save();

			AssertEquals("Consol should be Prepaid", PaymentType.Prepaid, consol.JK_PrepaidCollect);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 300m,
				},
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, sendingAgent, autorateCosts: false, autorateRevenue: true);
		}

		public void TestAutorateRevenue_StandardFreightCosts_NotShownInLog()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;

			var nonCompanyBranch = CreateNonProxyBranch("AUSYD").branch;
			var allowedDepartment = Factory.NewWithValidTestData<AccAllowedBranchDepartmentCombo>();
			nonCompanyBranch.AllowedDepartments.Add(allowedDepartment);

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(intercompanyTariff, "FRT", 200m, "NZAKL", "AUSYD");

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: sendingAgent, receivingAgent: nonCompanyBranch.OrgProxy);
			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", false);
			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate();

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "NS";
			staff.GS_LoginName = "New Staff";
			Factory.Save();

			using (Env.SetTemporaryUserContext("New Staff", nonCompanyBranch.PK.ToGuid(), allowedDepartment.PK.ToGuid()))
			{
				Helper.NewCosting(null);
				Helper.NewCosting(sendingAgent);
				Factory.Save();
			}

			using (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AutorateAndAssertWithUserContextOverride(expected: null, consol, sendingAgent, autorateCosts: false, autorateRevenue: true, branch: nonCompanyBranch, job: shipmentJob);
				AssertAutoratingAuditLogNoteNOTContainsLinesWithUserContextOverride(consol, nonCompanyBranch, "Information: RatingHeader Found Standard Costs (TACT/General Rates) Entries: 0");
				AssertAutoratingAuditLogNoteNOTContainsLinesWithUserContextOverride(consol, nonCompanyBranch, "Information: RatingHeader Found Costing EDICUS Entries: 0");
			}

			using (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AutorateAndAssertWithUserContextOverride(expected: null, consol, sendingAgent, autorateCosts: false, autorateRevenue: true, branch: nonCompanyBranch, job: shipmentJob);
				AssertAutoratingAuditLogNoteNOTContainsLinesWithUserContextOverride(consol, nonCompanyBranch, "Information: RatingHeader Found Standard Costs (TACT/General Rates) Entries: 0");
				AssertAutoratingAuditLogNoteNOTContainsLinesWithUserContextOverride(consol, nonCompanyBranch, "Information: RatingHeader Found Costing EDICUS Entries: 0");
			}
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAutorateRevenue_CreditorShouldBeSetAsServiceProvider()
		{
			CreateGlobalCharge("GWDWD", chargeGroup: ChargeCodeGroupList.Codes.Destination, isConsolLevel: true);
			CreateGlobalCharge("GWFMP", isConsolLevel: true);

			OrgHeader agent1 = null;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.China))
			{
				agent1 = Helper.CreateCreditor("OrgCNSZX");
				agent1.OH_IsCreditor = true;
				agent1.MainAddress.OA_RL_NKRelatedPortCode = "CNSZX";
			}
			var (agent2, _) = CreateCompanyAndBranchProxy("HKHKG");
			var (agent3, branch3) = CreateCompanyAndBranchProxy("BEANR");
			var (agent4, _) = CreateCompanyAndBranchProxy("BRSSZ");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch3.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;
			}

			var tariff = Helper.NewIntercompanyTariff(agent3);
			CreateFlatRate(tariff, "GWFMP", 200m, "", "BE", gatewayAgentType: GatewayAgentType.Codes.ReceivingAgent);
			var dstEntry = tariff.AddRateEntryWithFlatRateLine(RateCategory.DST, RateMode.LSE, "", "BE", "GWDWD", 100m);
			dstEntry.TI_GatewayAgentType = GatewayAgentType.Codes.ReceivingAgent;

			var consol1 = CreateForwardingConsolWithGatewayAgents("HKHKG", "BEANR", agent2, agent3, checkIsItGateway: false);
			consol1.JK_PrepaidCollect = PaymentType.Collect;

			SetUpAppointedGatewayAgentPorts(consol1.SendingForwarderAddress, "HKHKG", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol1.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(consol1.ReceivingForwarderAddress, "BEANR", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol1.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consol2 = CreateForwardingConsolWithGatewayAgents("BEANR", "BRSSZ", agent3, agent4, checkIsItGateway: false);
			consol2.JK_PrepaidCollect = PaymentType.Prepaid;
			consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol2.JK_ReceivingForwarderHandlingType = "";

			var shipment1 = CreateGatewayShipmentWithMultipleGatewayConsols("CNSZX", "BRSSZ", false, consol1, consol2);
			shipment1.JS_INCO = IncoTerms.DeliveredAtPlace;

			var shipment2 = CreateGatewayShipmentWithMultipleGatewayConsols("CNSZX", "BEANR", false, consol1);
			shipment2.JS_INCO = IncoTerms.ExWorks;

			var shipment3 = CreateGatewayShipmentWithMultipleGatewayConsols("HKHKG", "BRSSZ", false, consol1, consol2);
			shipment3.JS_INCO = IncoTerms.ExWorks;

			var shipment4 = CreateGatewayShipmentWithMultipleGatewayConsols("HKHKG", "BEANR", false, consol1);
			shipment4.JS_INCO = IncoTerms.CostAndFreight;

			var shipment5 = CreateGatewayShipmentWithMultipleGatewayConsols("BEANR", "BRSSZ", false, consol2);
			shipment5.JS_INCO = IncoTerms.DeliveredAtPlace;

			AssertNotNull("Data setup: Create shipment job with mutex", new JobHeader.Loader(shipment1).TryCreateWithMutex(branch3));
			AssertNotNull("Data setup: Create shipment job with mutex", new JobHeader.Loader(shipment2).TryCreateWithMutex(branch3));
			AssertNotNull("Data setup: Create shipment job with mutex", new JobHeader.Loader(shipment3).TryCreateWithMutex(branch3));
			AssertNotNull("Data setup: Create shipment job with mutex", new JobHeader.Loader(shipment4).TryCreateWithMutex(branch3));
			AssertNotNull("Data setup: Create shipment job with mutex", new JobHeader.Loader(shipment5).TryCreateWithMutex(branch3));

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "GWDWD",
					CostAccountCode = agent3.OH_Code,
					RelatedJobNumber = shipment2.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "GWFMP",
					CostAccountCode = agent3.OH_Code,
					RelatedJobNumber = shipment2.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "GWDWD",
					CostAccountCode = agent3.OH_Code,
					RelatedJobNumber = shipment4.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "GWFMP",
					CostAccountCode = agent3.OH_Code,
					RelatedJobNumber = shipment4.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol1, Consignor, branch: branch3, autorateRevenue: true, autorateCosts: false);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAutorateRevenue_BothDebtorAndCreditorAreOrgProxies_WhenCostAmountIsZeroAndCreditorIsNotSet_NoErrorShouldBeAdded()
		{
			var freightCharge = CreateGlobalCharge("FRT");
			freightCharge.AC_MarginPercentage = ZDecimal.Zero;

			var (sendingAgent, branch1) = CreateCompanyAndBranchProxy("NZAKL");

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(intercompanyTariff, "FRT", 100m, "NZAKL", "AUSYD");

			var (pickupAgent, _) = CreateBranchProxy("NZCHC", branch1.Company);
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				pickupAgent.OH_IsDebtor = true;
			}

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent, GlbCompany.CurrentCompany.OrgProxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "NZAKL", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", false, false);
			shipment.JS_ActualWeight = 200m;
			shipment.PickupAgentPK = pickupAgent.PK;

			AssertNotNull("Data setup: Create shipment job with mutex", new JobHeader.Loader(shipment).TryCreateWithMutex(branch1));

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 100m,
					JR_OSCostAmt = 0m,
					CostAccountCode = "",
					SellAccountCode = pickupAgent.OH_Code,
					RevenueCalculationDescription = "FRT: Base Rate AUD 100.00"
				}
			};

			var configuration = new GatewayChargeDefaultDebtorConfiguration()
			{
				ConsolDirection = "ALL",
				ConsolTransportMode = "ALL",
				ChargeGroup = "ALL",
				ConsolPaymentTerm = "ALL",
				RelatedJob = GatewayRelatedJob.Codes.RelatedToShipment,
				PreviousSendingAgent = "ALL",
				Debtor = GatewayDebtor.Codes.ShipmentPickupAgent,
			};

			var collection = new GatewayChargeDefaultDebtorConfigurationCollection();
			collection.AddRange(configuration);

			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			using (AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultDebtorConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				AutorateAndAssertWithUserContextOverride(expected, consol, sendingAgent, branch: branch1, autorateCosts: false, autorateRevenue: true);
			}
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAutorateRevenue_BothDebtorAndCreditorAreOrgProxies_ChargeIsMargin_OnlyDebtorShouldBeSet()
		{
			var freightCharge = CreateGlobalCharge("FRT");

			var (sendingAgent, branch1) = CreateCompanyAndBranchProxy("NZAKL");

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(intercompanyTariff, "FRT", 100m, "NZAKL", "AUSYD");

			var (pickupAgent, _) = CreateBranchProxy("NZCHC", branch1.Company);
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				pickupAgent.OH_IsDebtor = true;
			}

			var configuration = new GatewayChargeDefaultDebtorConfiguration()
			{
				ConsolDirection = "ALL",
				ConsolTransportMode = "ALL",
				ChargeGroup = "ALL",
				ConsolPaymentTerm = "ALL",
				RelatedJob = GatewayRelatedJob.Codes.RelatedToShipment,
				PreviousSendingAgent = "ALL",
				Debtor = GatewayDebtor.Codes.ShipmentPickupAgent,
			};

			var collection = new GatewayChargeDefaultDebtorConfigurationCollection();
			collection.AddRange(configuration);

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent, GlbCompany.CurrentCompany.OrgProxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "NZAKL", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", false, false);
			shipment.JS_ActualWeight = 200m;
			shipment.PickupAgentPK = pickupAgent.PK;

			AssertNotNull("Data setup: Create shipment job with mutex", new Job.Loader(shipment).TryCreateWithMutex(branch1));

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 100m,
					JR_OSCostAmt = 100m,
					CostAccountCode = string.Empty,
					SellAccountCode = pickupAgent.OH_Code,
					RevenueCalculationDescription = "FRT: Base Rate AUD 100.00"
				}
			};

			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			using (AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultDebtorConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				AutorateAndAssertWithUserContextOverride(expected, consol, sendingAgent, branch: branch1, autorateCosts: false, autorateRevenue: true);
			}
		}

		public void TestAutorateConsolRevenue_FallbackToMatchJobRateOriginRateDestination()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("ODOC");
			Helper.ChargeCodes.CreateGlobalCharge("ODOC", chargeGroup: ChargeCodeGroupList.Codes.Origin);
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("DDOC");
			Helper.ChargeCodes.CreateGlobalCharge("DDOC", chargeGroup: ChargeCodeGroupList.Codes.Destination);

			var intercompanyTariff = Helper.NewIntercompanyTariff(GlbCompany.CurrentCompany.OrgProxy);
			CreateFlatRate(intercompanyTariff, "FRT", 100, "", "NL");
			var orgEntry = CreateFlatRate(intercompanyTariff, "ODOC", 50, "CN", "");
			orgEntry.TI_RateCategory = RateCategory.ORG;
			var dstEntry = CreateFlatRate(intercompanyTariff, "DDOC", 60, "", "NL");
			dstEntry.TI_RateCategory = RateCategory.DST;

			Factory.Save();

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: null, receivingAgent: GlbCompany.CurrentCompany.OrgProxy);
			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", true);
			shipment.JS_INCO = IncoTerms.ExWorks;
			shipment.JS_RL_NKFreightRateOrigin = "CNSHA";
			shipment.JS_RL_NKFreightRateDestination = "NLRTM";

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_LocalSellAmt = 100,
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_LocalSellAmt = 50,
				},new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_LocalSellAmt = 60,
				},
			};

			AutorateAndAssertWithUserContextOverride(expectedCharges, consol, null, consol.ReceivingForwarder, autorateRevenue: true, autorateCosts: false);
		}

		public void TestAutorateConsolRevenue_OriginAndDestinationOfRateEntryHasPriorityOverRateOriginAndRateDestinationOfRateEntry()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("ODOC");
			Helper.ChargeCodes.CreateGlobalCharge("ODOC", chargeGroup: ChargeCodeGroupList.Codes.Origin);
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("DDOC");
			Helper.ChargeCodes.CreateGlobalCharge("DDOC", chargeGroup: ChargeCodeGroupList.Codes.Destination);

			var intercompanyTariff = Helper.NewIntercompanyTariff(GlbCompany.CurrentCompany.OrgProxy);

			var frtEntry1 = CreateFlatRate(intercompanyTariff, "FRT", 100, "NZAKL", "");
			frtEntry1.TI_RateOrigin = "CNSHA";
			frtEntry1.TI_RateDestination = "AU";
			var frtEntry2 = CreateFlatRate(intercompanyTariff, "FRT", 110, "CNSHA", "AUBNE");
			frtEntry2.TI_RateOrigin = "CNSHA";
			frtEntry2.TI_RateDestination = "AU";

			var orgEntry1 = CreateFlatRate(intercompanyTariff, "ODOC", 50, "NZAKL", "AU");
			orgEntry1.TI_RateCategory = RateCategory.ORG;
			orgEntry1.TI_RateOrigin = "CN";
			orgEntry1.TI_RateDestination = "AU";
			var orgEntry2 = CreateFlatRate(intercompanyTariff, "ODOC", 55, "NZ", "AU");
			orgEntry2.TI_RateCategory = RateCategory.ORG;
			orgEntry2.TI_RateOrigin = "CNSHA";
			orgEntry2.TI_RateDestination = "AUBNE";

			var dstEntry1 = CreateFlatRate(intercompanyTariff, "DDOC", 60, "", "AUSYD");
			dstEntry1.TI_RateCategory = RateCategory.DST;
			var dstEntry2 = CreateFlatRate(intercompanyTariff, "DDOC", 65, "", "AUBNE");
			dstEntry2.TI_RateCategory = RateCategory.DST;

			Factory.Save();

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: null, receivingAgent: GlbCompany.CurrentCompany.OrgProxy);
			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", true);
			shipment.JS_INCO = IncoTerms.ExWorks;
			shipment.JS_RL_NKFreightRateOrigin = "CNSHA";
			shipment.JS_RL_NKFreightRateDestination = "AUBNE";

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_LocalSellAmt = 110,
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_LocalSellAmt = 50,
				},new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_LocalSellAmt = 60,
				},
			};

			AutorateAndAssertWithUserContextOverride(expectedCharges, consol, null, consol.ReceivingForwarder, autorateRevenue: true, autorateCosts: false);

			var expectedLogLines = new[]
			{
				"Information: RateLine Filtered FRT-FLT-Intercompany Tariff EDICUS\treason:\toverridden by FRT-FLT-Intercompany Tariff EDICUS by Origin Destination comparer",
				"Information: RateLine Filtered ODOC-FLT-Intercompany Tariff EDICUS\treason:\toverridden by ODOC-FLT-Intercompany Tariff EDICUS by Origin Destination comparer",
				"Information: RateLine Filtered DDOC-FLT-Intercompany Tariff EDICUS\treason:\toverridden by DDOC-FLT-Intercompany Tariff EDICUS by Origin Destination comparer",
			};
			AssertAutoratingAuditLogNoteContainsLines(consol, "Should have rate line removal reasons:", expectedLogLines);
		}

		public void TestAutorateRevenue_RateOriginTakesPriorityOverRateDestination()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var intercompanyTariff = Helper.NewIntercompanyTariff(GlbCompany.CurrentCompany.OrgProxy);

			var frtEntry1 = CreateFlatRate(intercompanyTariff, "FRT", 10, "NZAKL", "AUSYD");
			frtEntry1.TI_RateOrigin = "CNSHA";
			frtEntry1.TI_RateDestination = "";
			var frtEntry2 = CreateFlatRate(intercompanyTariff, "FRT", 20, "NZAKL", "AUSYD");
			frtEntry2.TI_RateOrigin = "";
			frtEntry2.TI_RateDestination = "USLAX";

			Factory.Save();

			var consol = CreateForwardingConsolWithGatewayAgents("CNSHA", "USLAX", sendingAgent: null, receivingAgent: GlbCompany.CurrentCompany.OrgProxy);
			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", true);
			shipment.JS_INCO = IncoTerms.ExWorks;
			shipment.JS_RL_NKFreightRateOrigin = "CNSHA";
			shipment.JS_RL_NKFreightRateDestination = "USLAX";

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_LocalSellAmt = 10,
				}
			};

			AutorateAndAssertWithUserContextOverride(expectedCharges, consol, null, consol.ReceivingForwarder, autorateRevenue: true, autorateCosts: false);

			var expectedLogLines = new[]
			{
				"Information: RateLine Filtered FRT-FLT-Intercompany Tariff EDICUS\treason:\toverridden by FRT-FLT-Intercompany Tariff EDICUS by Rate Origin Rate Destination comparer"
			};
			AssertAutoratingAuditLogNoteContainsLines(consol, "Should have rate line removal reasons:", expectedLogLines);
		}

		public void TestAutorateConsolRevenue_RateOriginAndRateDestinationOfRateEntryHasPriorityOverPlannedLoadAndPlannedDischargeOfRateEntry()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("ODOC");
			Helper.ChargeCodes.CreateGlobalCharge("ODOC", chargeGroup: ChargeCodeGroupList.Codes.Origin);
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("DDOC");
			Helper.ChargeCodes.CreateGlobalCharge("DDOC", chargeGroup: ChargeCodeGroupList.Codes.Destination);

			var intercompanyTariff = Helper.NewIntercompanyTariff(GlbCompany.CurrentCompany.OrgProxy);

			var frtEntry1 = CreateFlatRate(intercompanyTariff, "FRT", 10, "NZAKL", "AUSYD");
			frtEntry1.TI_PlannedLoadLRC = "CNSHA";
			frtEntry1.TI_PlannedDischargeLRC = "USLAX";
			var frtEntry2 = CreateFlatRate(intercompanyTariff, "FRT", 20, "NZAKL", "AUSYD");
			frtEntry2.TI_RateOrigin = "CNSHA";
			frtEntry2.TI_RateDestination = "USLAX";

			var orgEntry1 = CreateFlatRate(intercompanyTariff, "ODOC", 30, "NZAKL", "AUSYD");
			orgEntry1.TI_RateCategory = RateCategory.ORG;
			orgEntry1.TI_PlannedLoadLRC = "CNSHA";
			orgEntry1.TI_PlannedDischargeLRC = "";
			var orgEntry2 = CreateFlatRate(intercompanyTariff, "ODOC", 40, "NZAKL", "AUSYD");
			orgEntry2.TI_RateCategory = RateCategory.ORG;
			orgEntry2.TI_RateOrigin = "CNSHA";
			orgEntry2.TI_RateDestination = "";

			var dstEntry1 = CreateFlatRate(intercompanyTariff, "DDOC", 50, "NZAKL", "AUSYD");
			dstEntry1.TI_RateCategory = RateCategory.DST;
			dstEntry1.TI_PlannedLoadLRC = "";
			dstEntry1.TI_PlannedDischargeLRC = "USLAX";
			var dstEntry2 = CreateFlatRate(intercompanyTariff, "DDOC", 60, "NZAKL", "AUSYD");
			dstEntry2.TI_RateCategory = RateCategory.DST;
			dstEntry2.TI_RateOrigin = "";
			dstEntry2.TI_RateDestination = "USLAX";

			Factory.Save();

			var consol = CreateForwardingConsolWithGatewayAgents("CNSHA", "USLAX", sendingAgent: null, receivingAgent: GlbCompany.CurrentCompany.OrgProxy);
			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", true);
			shipment.JS_INCO = IncoTerms.ExWorks;
			shipment.JS_RL_NKFreightRateOrigin = "CNSHA";
			shipment.JS_RL_NKFreightRateDestination = "USLAX";

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_LocalSellAmt = 20,
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_LocalSellAmt = 40,
				},new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_LocalSellAmt = 60,
				},
			};

			AutorateAndAssertWithUserContextOverride(expectedCharges, consol, null, consol.ReceivingForwarder, autorateRevenue: true, autorateCosts: false);

			var expectedLogLines = new[]
			{
				"Information: RateLine Filtered FRT-FLT-Intercompany Tariff EDICUS\treason:\toverridden by FRT-FLT-Intercompany Tariff EDICUS by Rate Origin Rate Destination comparer",
				"Information: RateLine Filtered ODOC-FLT-Intercompany Tariff EDICUS\treason:\toverridden by ODOC-FLT-Intercompany Tariff EDICUS by Rate Origin Rate Destination comparer",
				"Information: RateLine Filtered DDOC-FLT-Intercompany Tariff EDICUS\treason:\toverridden by DDOC-FLT-Intercompany Tariff EDICUS by Rate Origin Rate Destination comparer"
			};
			AssertAutoratingAuditLogNoteContainsLines(consol, "Should have rate line removal reasons:", expectedLogLines);
		}

		public void TestAutorateConsolRevenue_MoreSpecificRateOriginAndRateDestinationIsPrefered()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("DDOC");
			Helper.ChargeCodes.CreateGlobalCharge("DDOC", chargeGroup: ChargeCodeGroupList.Codes.Destination);
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("BAF");
			Helper.ChargeCodes.CreateGlobalCharge("BAF");

			var intercompanyTariff = Helper.NewIntercompanyTariff(GlbCompany.CurrentCompany.OrgProxy);

			var dstEntry1 = CreateFlatRate(intercompanyTariff, "DDOC", 60, "NZAKL", "AUSYD");
			dstEntry1.TI_RateCategory = RateCategory.DST;
			dstEntry1.TI_RateOrigin = "CNSHA";
			dstEntry1.TI_RateDestination = "US";
			var dstEntry2 = CreateFlatRate(intercompanyTariff, "DDOC", 65, "NZAKL", "AUSYD");
			dstEntry2.TI_RateCategory = RateCategory.DST;
			dstEntry2.TI_RateOrigin = "CN";
			dstEntry2.TI_RateDestination = "USLAX";

			var bafEntry1 = CreateFlatRate(intercompanyTariff, "BAF", 100, "NZAKL", "AUSYD");
			bafEntry1.TI_RateOrigin = "CNSHA";
			bafEntry1.TI_RateDestination = "";
			var bafEntry2 = CreateFlatRate(intercompanyTariff, "BAF", 110, "NZAKL", "AUSYD");
			bafEntry2.TI_RateOrigin = "CN";
			bafEntry2.TI_RateDestination = "USLAX";

			Factory.Save();

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: null, receivingAgent: GlbCompany.CurrentCompany.OrgProxy);
			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", true);
			shipment.JS_INCO = IncoTerms.ExWorks;
			shipment.JS_RL_NKFreightRateOrigin = "CNSHA";
			shipment.JS_RL_NKFreightRateDestination = "USLAX";

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 65m
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 100m
				}
			};

			AutorateAndAssertWithUserContextOverride(expectedCharges, consol, null, consol.ReceivingForwarder, autorateRevenue: true, autorateCosts: false);

			var expectedLogLines = new[]
			{
				"Information: RateLine Filtered BAF-FLT-Intercompany Tariff EDICUS\treason:\toverridden by BAF-FLT-Intercompany Tariff EDICUS by Rate Origin Rate Destination comparer",
				"Information: RateLine Filtered DDOC-FLT-Intercompany Tariff EDICUS\treason:\toverridden by DDOC-FLT-Intercompany Tariff EDICUS by Rate Origin Rate Destination comparer",
			};
			AssertAutoratingAuditLogNoteContainsLines(consol, "Should have rate line removal reasons:", expectedLogLines);
		}

		public void TestAutorateConsolRevenue_JobOriginDestinationTakePrecedenceOverRateOriginRateDestination()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("ODOC");
			Helper.ChargeCodes.CreateGlobalCharge("ODOC", chargeGroup: ChargeCodeGroupList.Codes.Origin);
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("DDOC");
			Helper.ChargeCodes.CreateGlobalCharge("DDOC", chargeGroup: ChargeCodeGroupList.Codes.Destination);

			var intercompanyTariff = Helper.NewIntercompanyTariff(GlbCompany.CurrentCompany.OrgProxy);

			CreateFlatRate(intercompanyTariff, "FRT", 100, "", "NL");
			CreateFlatRate(intercompanyTariff, "FRT", 110, "", "AUSYD");

			var orgEntry1 = CreateFlatRate(intercompanyTariff, "ODOC", 50, "CN", "");
			orgEntry1.TI_RateCategory = RateCategory.ORG;
			var orgEntry2 = CreateFlatRate(intercompanyTariff, "ODOC", 55, "NZAKL", "");
			orgEntry2.TI_RateCategory = RateCategory.ORG;

			var dstEntry1 = CreateFlatRate(intercompanyTariff, "DDOC", 60, "", "NL");
			dstEntry1.TI_RateCategory = RateCategory.DST;
			var dstEntry2 = CreateFlatRate(intercompanyTariff, "DDOC", 65, "", "AUSYD");
			dstEntry2.TI_RateCategory = RateCategory.DST;

			Factory.Save();

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: null, receivingAgent: GlbCompany.CurrentCompany.OrgProxy);
			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", true);
			shipment.JS_INCO = IncoTerms.ExWorks;
			shipment.JS_RL_NKFreightRateOrigin = "CNSHA";
			shipment.JS_RL_NKFreightRateDestination = "NLRTM";

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_LocalSellAmt = 110,
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_LocalSellAmt = 55,
				},new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_LocalSellAmt = 65,
				},
			};

			AutorateAndAssertWithUserContextOverride(expectedCharges, consol, null, consol.ReceivingForwarder, autorateRevenue: true, autorateCosts: false);

			var expectedLogLines = new[]
			{
				"Information: RateLine Filtered FRT-FLT-Intercompany Tariff EDICUS\treason:\toverridden by FRT-FLT-Intercompany Tariff EDICUS by Origin Destination comparer",
				"Information: RateLine Filtered ODOC-FLT-Intercompany Tariff EDICUS\treason:\toverridden by ODOC-FLT-Intercompany Tariff EDICUS by Origin Destination comparer",
				"Information: RateLine Filtered DDOC-FLT-Intercompany Tariff EDICUS\treason:\toverridden by DDOC-FLT-Intercompany Tariff EDICUS by Origin Destination comparer",
			};
			AssertAutoratingAuditLogNoteContainsLines(consol, "Should have rate line removal reasons:", expectedLogLines);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateRevenueFromGatewayInvoicingMenu_ShouldReturnIntercompanyTariffWithRIMGatewayAgentTypeWhenLoginAsReceivingAgentandIsGTT()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("BAF");
			Helper.ChargeCodes.CreateGlobalCharge("BAF");

			var origin = "NZAKL";
			var destination = "AUSYD";
			var receivingAgent = GlbCompany.CurrentCompany.OrgProxy;

			var intercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent);
			CreateFlatRate(intercompanyTariff, "FRT", 33m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.ReceivingAgentForImport);
			CreateFlatRate(intercompanyTariff, "BAF", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: null, receivingAgent: receivingAgent);
			consol.JK_OA_ShippingLineAddress = receivingAgent.MainAddress.PK;

			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 33m,
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 66m,
				}
			};

			CombineAssertions(() =>
			{
				AssertEquals("Shipment gateways should contain consol receiving agent", consol.ReceivingForwarder.PK, shipment.Gateways[0].Forwarder.PK);
				AssertEquals("Receivng Agent type should be GTT", consol.JK_ReceivingForwarderHandlingType, AgentStatusList.Codes.GatewayAgentWithTariff);
			});

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: receivingAgent.Branch, autorateRevenue: true, autorateCosts: false);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateRevenueFromGatewayInvoicingMenu_ShouldReturnIntercompanyTariffWithGatewayAgentTypeIsBlankOrRIMWhenLoginAsReceivingAgentandIsGTA()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("BAF");
			Helper.ChargeCodes.CreateGlobalCharge("BAF");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("CAF");
			Helper.ChargeCodes.CreateGlobalCharge("CAF");

			var origin = "NZAKL";
			var destination = "AUSYD";
			var receivingAgent = GlbCompany.CurrentCompany.OrgProxy;

			var intercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent);
			CreateFlatRate(intercompanyTariff, "FRT", 33m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.ReceivingAgentForImport);
			CreateFlatRate(intercompanyTariff, "BAF", 66m, origin, destination);
			CreateFlatRate(intercompanyTariff, "CAF", 99m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: null, receivingAgent: receivingAgent);
			consol.JK_OA_ShippingLineAddress = receivingAgent.MainAddress.PK;

			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgent);

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 33m,
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 66m,
				}
			};

			CombineAssertions(() =>
			{
				AssertEquals("Shipment gateways should contain consol receiving agent", consol.ReceivingForwarder.PK, shipment.Gateways[0].Forwarder.PK);
				AssertEquals("Receiving Agent type should be GTA", consol.JK_ReceivingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
			});

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: receivingAgent.Branch, autorateRevenue: true, autorateCosts: false);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateRevenueFromGatewayInvoicingMenu_WhenConsolIsNotCCX_ShuolReturnRIMandNotReturnRAGInterCompanyTariff()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("BAF");
			Helper.ChargeCodes.CreateGlobalCharge("BAF");

			var origin = "NZAKL";
			var destination = "AUSYD";
			var receivingAgent = GlbCompany.CurrentCompany.OrgProxy;

			var intercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent);
			CreateFlatRate(intercompanyTariff, "FRT", 33m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.ReceivingAgentForImport);
			CreateFlatRate(intercompanyTariff, "BAF", 66m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.ReceivingAgent);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: null, receivingAgent: receivingAgent);
			consol.JK_OA_ShippingLineAddress = receivingAgent.MainAddress.PK;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgent);

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 33m,
				}
			};

			CombineAssertions(() =>
			{
				AssertEquals("Shipment gateways should contain consol receiving agent", consol.ReceivingForwarder.PK, shipment.Gateways[0].Forwarder.PK);
				AssertEquals("Receiving Agent type should be GTA", consol.JK_ReceivingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
				AssertEquals("Consol should be Prepaid", PaymentType.Prepaid, consol.JK_PrepaidCollect);
			});

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: receivingAgent.Branch, autorateRevenue: true, autorateCosts: false);

			AssertAutoratingAuditLogNoteContainsLines(consol,
				"Information: AUTORATING REVENUE FOR Consol C00001000",
				"Information: RatingHeader Found Intercompany Tariff EDICUS Entries: 2",
				"Information: RateEntry Filtered Intercompany Tariff EDICUS reason: Gateway agent type RAG cannot be used for Gateway revenue when Consol's payment term is not collect.");
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateRevenueFromGatewayInvoicingMenu_WhenConsolIsNotPPD_ShouldReturnEmptyAndShouldNotReturnSAGIntercompanyTariff()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("BAF");
			Helper.ChargeCodes.CreateGlobalCharge("BAF");

			var origin = "NZAKL";
			var destination = "AUSYD";
			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(intercompanyTariff, "FRT", 33m, origin, destination);
			CreateFlatRate(intercompanyTariff, "BAF", 66m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: null);
			consol.JK_OA_ShippingLineAddress = sendingAgent.MainAddress.PK;
			consol.JK_PrepaidCollect = PaymentType.Collect;

			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgent);

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 33m,
				}
			};

			CombineAssertions(() =>
			{
				AssertEquals("Shipment gateways should contain consol sending agent", consol.SendingForwarder.PK, shipment.Gateways[0].Forwarder.PK);
				AssertEquals("Consol should be Collect", PaymentType.Collect, consol.JK_PrepaidCollect);
			});

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: true, autorateCosts: false);

			AssertAutoratingAuditLogNoteContainsLines(consol,
				"Information: AUTORATING REVENUE FOR Consol C00001000",
				"Information: RatingHeader Found Intercompany Tariff EDICUS Entries: 2",
				"Information: RateEntry Filtered Intercompany Tariff EDICUS reason: Gateway agent type SAG cannot be used for Gateway revenue when Consol's payment term is not prepaid.");
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateRevenueFromGatewayInvoicingMenu_WhenConsolIsPPD_ShouldReturnEmptyAndSAGIntercompanyTariff()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("BAF");
			Helper.ChargeCodes.CreateGlobalCharge("BAF");

			var origin = "NZAKL";
			var destination = "AUSYD";
			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(intercompanyTariff, "FRT", 33m, origin, destination);
			CreateFlatRate(intercompanyTariff, "BAF", 66m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: null);
			consol.JK_OA_ShippingLineAddress = sendingAgent.MainAddress.PK;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgent);

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 33m,
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 66m,
				}
			};

			CombineAssertions(() =>
			{
				AssertEquals("Shipment gateways should contain consol sending agent", consol.SendingForwarder.PK, shipment.Gateways[0].Forwarder.PK);
				AssertEquals("Consol should be Prepaid", PaymentType.Prepaid, consol.JK_PrepaidCollect);
			});

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: true, autorateCosts: false);
		}

		public void TestAutorateRevenue_WhenCreateJournalWithGatewayBillingSellCharge_ThenCostRateAuditIsCascadedFromConsol()
		{
			var charge = Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			charge.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;

			var intercompanyTariff = Helper.NewIntercompanyTariff(GlbCompany.CurrentCompany.OrgProxy);
			CreateFlatRate(intercompanyTariff, "FRT1", 33, "NZAKL", "AUSYD");

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: GlbCompany.CurrentCompany.OrgProxy, null);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", true);

			Factory.Save();
			var consolRevenueCalculationDescription = "FRT1: Base Rate AUD 33.00";
			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_LocalSellAmt = 33m,
					RevenueCalculationDescription = consolRevenueCalculationDescription,
					SellAccountCode = GlbCompany.CurrentCompany.OrgProxy.OH_Code,
					JR_LocalCostAmt = 0,
					CostAccountCode = string.Empty,
					RelatedJobNumber = shipment.JS_UniqueConsignRef,
				}
			};
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			{
				AutorateAndAssertWithUserContextOverride(expected, consol, null, null, autorateRevenue: true, autorateCosts: false);
				Factory.Save();
			}

			var shipmentJob = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job;
			var shipmentCostCalculationDescription = string.Format("Cascaded from Consol {0} > Gateway Billing charge {1} > Revenue Rate Audit:\r\n\r\n" + consolRevenueCalculationDescription, consol.JobNumber, "FRT1");
			var expected1 = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_LocalSellAmt = 0,
					JR_LocalCostAmt = 33m,
					SellAccountCode = string.Empty,
					CostAccountCode = GlbCompany.CurrentCompany.OrgProxy.OH_Code,
					RevenueCalculationDescription = string.Empty,
					CostCalculationDescription = shipmentCostCalculationDescription, // Revenue on consol will be cost on shipment
				}
			};
			AssertCharges(expected1, shipmentJob);
		}

		public void TestAutorateRevenue_ChargeGroupingRollUp_MustControlDisplayingCalculationDescription_InsteadOf_AutoJobRevenueJournals()
		{
			var charge = Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			charge.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;

			var intercompanyTariff = Helper.NewIntercompanyTariff(GlbCompany.CurrentCompany.OrgProxy);
			CreateFlatRate(intercompanyTariff, "FRT1", 33, "NZAKL", "AUSYD");

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: GlbCompany.CurrentCompany.OrgProxy, null);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", true);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_LocalSellAmt = 33m,
					RevenueCalculationDescription = "FRT1: Base Rate AUD 33.00",
					SellAccountCode = GlbCompany.CurrentCompany.OrgProxy.OH_Code,
					JR_LocalCostAmt = 0,
					CostAccountCode = string.Empty,
					RelatedJobNumber = shipment.JS_UniqueConsignRef,
				}
			};

			var invoiceRollupOrGroup = OrganisationRegistry.Instance.InvoiceRollupOrGroup.Value.GetBestMatch(OrgConstants.ServiceDirection.Code.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code);
			var invoiceRollupOrGroupCollection = new InvoiceRollupOrGroupCollection();
			invoiceRollupOrGroupCollection.Add(invoiceRollupOrGroup);

			invoiceRollupOrGroup.InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.None;
			expected[0].JR_Desc = "FRT1 Global Charge - Shipment S00001000"; // Without Calculation Description

			using (AutoJRJRegistryStatusHelper.SetAutoJRJDisabled_ForTestOnly())
			using (OrganisationRegistry.Instance.InvoiceRollupOrGroup.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, invoiceRollupOrGroupCollection))
			{
				AutorateAndAssertWithUserContextOverride(expected, consol, null, null, autorateRevenue: true, autorateCosts: false);
			}

			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			using (OrganisationRegistry.Instance.InvoiceRollupOrGroup.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, invoiceRollupOrGroupCollection))
			{
				AutorateAndAssertWithUserContextOverride(expected, consol, null, null, autorateRevenue: true, autorateCosts: false);
			}

			invoiceRollupOrGroup.InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.All;
			expected[0].JR_Desc = "FRT1 Global Charge - Base Rate AUD 33.00 - Shipment S00001000"; // With Calculation Description

			using (AutoJRJRegistryStatusHelper.SetAutoJRJDisabled_ForTestOnly())
			using (OrganisationRegistry.Instance.InvoiceRollupOrGroup.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, invoiceRollupOrGroupCollection))
			{
				AutorateAndAssertWithUserContextOverride(expected, consol, null, null, autorateRevenue: true, autorateCosts: false);
			}

			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			using (OrganisationRegistry.Instance.InvoiceRollupOrGroup.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, invoiceRollupOrGroupCollection))
			{
				AutorateAndAssertWithUserContextOverride(expected, consol, null, null, autorateRevenue: true, autorateCosts: false);
			}
		}

		public void TestAutorateRevenueFromGatewayInvoicingMenu_JobIsNotOpenedOnShipments_RelatedJobNumberShouldBePopulated()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("DDOC");
			Helper.ChargeCodes.CreateGlobalCharge("DDOC");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;

			Helper.NewIntercompanyTariff(sendingAgent).AddRateEntryWithFlatRateLine("DST", "LSE", "", "DEHAM", "DDOC", 3m);

			var consol = CreateForwardingConsolWithGatewayAgents("AUSYD", "DEHAM", sendingAgent: sendingAgent, receivingAgent: null);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "AUSYD", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol.JK_OA_ShippingLineAddress = sendingAgent.MainAddress.PK;
			consol.JK_PrepaidCollect = PaymentType.Collect;

			var sydneyShipment = CreateGatewayShipment(consol, "AUSYD", "DEHAM", isCollect: true, withJob: false);

			var (_, aucklandBranch) = CreateCompanyAndBranchProxy("NZAKL");

			ForwardingShipment aucklandShipment;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, aucklandBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				aucklandShipment = CreateGatewayShipment(consol, "NZAKL", "DEHAM", isCollect: true, withJob: false);
			}

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 3m,
					RelatedJobNumber = sydneyShipment.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 3m,
					RelatedJobNumber = aucklandShipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: true, autorateCosts: false);
		}

		#endregion

		#region Test Autorate Costs

		[TestDate(2019, 11, 12)]
		public void TestAutorateCost_CreditorOverride()
		{
			// Creation of Company and Branch Proxy should be at start
			// because CreditorOverride would validate Creditor that access NULL Env.CurrentCompany.PK (can't load from new Factory)
			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateCompanyAndBranchProxy("USCHS");
			Factory.Save();

			// Set Creditor1 & Creditor2 to be payable on all companies because GlobalChargeCode would validate its LocalChargeCodes > CreditorOverride > Creditor
			var activeCompanies = GlbCompany.GetActiveCompanies();
			foreach (var company in activeCompanies)
			{
				if (company?.FirstActiveBranch != null)
				{
					using (company.FirstActiveBranch.SetAsTemporaryContext())
					{
						var currentFactory = new BusinessObjectFactory();
						var currentCreditor1 = currentFactory.Load<OrgHeader>(TestObjectCreator.Creditor1.PK);
						currentCreditor1.OH_IsCreditor = true;
						var currentCreditor2 = currentFactory.Load<OrgHeader>(TestObjectCreator.Creditor2.PK);
						currentCreditor2.OH_IsCreditor = true;
						currentFactory.Save();
					}
				}
			}

			var accChargeCode1 = Helper.ChargeCodes.CreateGlobalCharge("FRT1");

			TestObjectCreator.CreateChargeCreditorOverride(accChargeCode1, JobInvoicingConsumerTypes.ShipmentCode, direction: "ALL", transportMode: "ALL", paymentTerm: PaymentType.Collect, creditor: TestObjectCreator.Creditor1.PK);
			TestObjectCreator.CreateChargeCreditorOverride(accChargeCode1, JobInvoicingConsumerTypes.GatewayConsolCode, direction: "ALL", transportMode: "ALL", paymentTerm: PaymentType.Collect, creditor: TestObjectCreator.Creditor2.PK);

			Factory.Save();

			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);
			CreateFlatRate(receivingIntercompanyTariff, "FRT1", 10m, "NZAKL", "AUSYD");

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, sendingAgent, "NZAKL", "AUSYD", "FRT1", 20m);

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "NZAKL", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", true);

			Factory.Save();

			AutorateAndAssertWithUserContextOverride
			(
				expected: new[] {
					new AssertionCharge
					{
						ChargeCode = "FRT1",
						JR_OSSellAmt = 10m,
						RelatedJobNumber = shipment.JS_UniqueConsignRef,
						CostAccountCode = TestObjectCreator.Creditor2.OH_Code,
					},
					new AssertionCharge
					{
						ChargeCode = "FRT1",
						JR_OSSellAmt = 20m,
						CostAccountCode = TestObjectCreator.Creditor2.OH_Code,
					},
				},
				consol,
				null,
				branch: sendingAgent.Branch,
				autorateRevenue: false,
				autorateCosts: true
			);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		#region Autorate Cost from Gateway Invoicing Menu

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ShouldNotReturnAnyCostWhenLoginAsReceivingAgentandIsGTA()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var origin = "NZAKL";
			var destination = "AUSYD";
			var receivingAgent = GlbCompany.CurrentCompany.OrgProxy;

			var intercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent);
			CreateFlatRate(intercompanyTariff, "FRT", 33m, origin, destination);

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, receivingAgent, origin, destination, "BAF", 15);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: null, receivingAgent: receivingAgent);
			consol.JK_OA_ShippingLineAddress = receivingAgent.MainAddress.PK;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			Factory.Save();

			var expected = Array.Empty<AssertionCharge>();

			CombineAssertions(() =>
			{
				AssertEquals("Shipment's gateways should contain consol receiving agent", consol.ReceivingForwarder.PK, shipment.Gateways[0].Forwarder.PK);
				AssertEquals("Receiving Agent type should be GTA", consol.JK_ReceivingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
			});

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: receivingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ShouldNotReturnAnyCostWhenReceivingAgentIsGTA_AndUnderSameCompanyWithCurrentLogin()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var branchWithProxy = CreateBranchProxy("AUSYD");

			var origin = "NZAKL";
			var destination = "AUSYD";
			var receivingAgent = GlbCompany.CurrentCompany.OrgProxy;

			var intercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent);
			CreateFlatRate(intercompanyTariff, "FRT", 33m, origin, destination);

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, receivingAgent, origin, destination, "BAF", 15);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: null, receivingAgent: receivingAgent);
			consol.JK_OA_ShippingLineAddress = receivingAgent.MainAddress.PK;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			Factory.Save();

			var expected = Array.Empty<AssertionCharge>();

			CombineAssertions(() =>
			{
				AssertEquals("Shipment's gateways should contain consol receiving agent", consol.ReceivingForwarder.PK, shipment.Gateways[0].Forwarder.PK);
				AssertEquals("Receiving Agent type should be GTA", consol.JK_ReceivingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
			});

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: branchWithProxy.branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ShouldReturnCostFromCostingModuleWhenLoginAsReceivingAgentandIsGTT()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var origin = "NZAKL";
			var destination = "AUSYD";
			var receivingAgent = GlbCompany.CurrentCompany.OrgProxy;

			var intercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent);
			CreateFlatRate(intercompanyTariff, "FRT", 33m, origin, destination);

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, receivingAgent, origin, destination, "BAF", 15);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: null, receivingAgent: receivingAgent);
			consol.JK_OA_ShippingLineAddress = receivingAgent.MainAddress.PK;

			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 15m,
				}
			};

			CombineAssertions(() =>
			{
				AssertEquals("Shipment's gateways should contain consol receiving agent", consol.ReceivingForwarder.PK, shipment.Gateways[0].Forwarder.PK);
				AssertEquals("Receivng Agent type should be GTT", consol.JK_ReceivingForwarderHandlingType, AgentStatusList.Codes.GatewayAgentWithTariff);
			});

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: receivingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ShouldAddCostIfThereIsTheSameSpotCost()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var receivingAgent = GlbCompany.CurrentCompany.OrgProxy;

			var rateLine = CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, receivingAgent, origin, destination, "BAF", 15);
			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: null, receivingAgent: receivingAgent);
			consol.JK_OA_ShippingLineAddress = receivingAgent.MainAddress.PK;

			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			CreateGatewayShipment(consol, origin, destination, true);

			using (var job = consol.Job as Job ?? new JobHeader.Loader(consol).TryLoadOrCreate() as Job)
			{
				var chargeCollection = job.Charges;

				var charge = chargeCollection.AddNew();
				charge.JR_AC = rateLine.ChargeCode.PK;
				charge.JR_LocalCostAmt = 55;
				charge.JR_IsSpotCost = true;

				Factory.Save();

				Assert("Should become readonly when JR_IsSpotCost set to true", charge.JR_Calc_CostRatingBehaviorInfo.ReadOnly);

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "BAF",
						JR_OSSellAmt = 15m
					},
					new AssertionCharge
					{
						ChargeCode = "BAF", // We should keep Spot Cost
						JR_OSSellAmt = 55m,
					}
				};

				AutorateAndAssertWithUserContextOverride(expected, consol, null, autorateRevenue: false, autorateCosts: true, job: job);
			}
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ShouldNotReturnAnyCostFromCostingWhenLoginAsSendingAgentAndIsGTA()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var origin = "NZAKL";
			var destination = "AUSYD";
			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(intercompanyTariff, "FRT", 33m, origin, destination);

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, sendingAgent, origin, destination, "BAF", 15);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: null);
			consol.JK_OA_ShippingLineAddress = sendingAgent.MainAddress.PK;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			Factory.Save();

			var expected = Array.Empty<AssertionCharge>();

			CombineAssertions(() =>
			{
				AssertEquals("Receivng Agent type should be empty", consol.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
			});

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ShouldNotReturnAnyCostFromCostingWhenSendingAgentIsGTA_AndUnderSameCompanyWithCurrentLogin()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			var branchWithProxy = CreateBranchProxy("AUSYD");

			var origin = "NZAKL";
			var destination = "AUSYD";
			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(intercompanyTariff, "FRT", 33m, origin, destination);

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, sendingAgent, origin, destination, "BAF", 15);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: null);
			consol.JK_OA_ShippingLineAddress = sendingAgent.MainAddress.PK;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			Factory.Save();

			var expected = Array.Empty<AssertionCharge>();

			CombineAssertions(() =>
			{
				AssertEquals("Receivng Agent type should be empty", consol.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
			});

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: branchWithProxy.branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ShouldReturnCostFromCostingWhenLoginAsSendingAgentAndIsGTT()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var origin = "NZAKL";
			var destination = "AUSYD";
			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(intercompanyTariff, "FRT", 33m, origin, destination);

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, sendingAgent, origin, destination, "BAF", 15);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: null);
			consol.JK_OA_ShippingLineAddress = sendingAgent.MainAddress.PK;

			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 15m,
				}
			};

			CombineAssertions(() =>
			{
				AssertEquals("Receivng Agent type should be empty", consol.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgentWithTariff);
			});

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ShouldReturnCostFromCostingAndIntercompanyTariffWhenLoginAsSendingAgentAndIsGTTAndReceivingAgentIsGTA()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			Helper.ChargeCodes.CreateGlobalCharge("FRT3");
			Helper.ChargeCodes.CreateGlobalCharge("FRT4");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateCompanyAndBranchProxy("USCHS");
			Factory.Save();

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(sendingIntercompanyTariff, "FRT1", 66m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT3", 44m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT4", 55m, origin, destination);

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, sendingAgent, origin, destination, "BAF", 15);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 15m,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT3",
					JR_OSSellAmt = 44m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "FRT4",
					JR_OSSellAmt = 55m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AssertEquals("Shipment's gateways should contain consol sending agent", consol.SendingForwarder.PK, shipment.Gateways[0].Forwarder.PK);
			AssertEquals("Receivng Agent type should be GTA", consol.JK_ReceivingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ShouldReturnCostFromCostingAndIntercompanyTariffWhenLoginAsSendingAgentAndIsGTTAndReceivingAgentIsGTABothUnderSameCompany()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			Helper.ChargeCodes.CreateGlobalCharge("FRT3");
			Helper.ChargeCodes.CreateGlobalCharge("FRT4");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateBranchProxy("USCHS");
			Factory.Save();

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(sendingIntercompanyTariff, "FRT1", 66m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT3", 44m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT4", 55m, origin, destination);

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, sendingAgent, origin, destination, "BAF", 15);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 15m,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT3",
					JR_OSSellAmt = 44m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "FRT4",
					JR_OSSellAmt = 55m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AssertEquals("Shipment's gateways should contain consol sending agent", consol.SendingForwarder.PK, shipment.Gateways[0].Forwarder.PK);
			AssertEquals("Receivng Agent type should be GTA", consol.JK_ReceivingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		/// <summary>
		/// This test is very specific for checking that we don't call RatesService(WiseRates) for shipment attached to Gateway Consol
		/// Do NOT use this as a base for new tests unless you interested in RatesService
		/// </summary>
		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ShouldNotReturnAnyWiseRateCosts()
		{
			var carrier = TransportProvider1;
			carrier.OH_IsShippingProvider = true;
			var shipmentChargeCode = Helper.ChargeCodes.New("FRT1", "FRT charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var origin = "NZAKL";
			var destination = "AUSYD";
			var receivingAgent = GlbCompany.CurrentCompany.OrgProxy;

			var cont20GP = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "20GP");

			var wiseCost20GP = CreateTestRate(TransportModes.Air, ContainerModes.FCL, origin, destination, "", "", cont20GP.RC_Code, "Emirates", NewClient.OH_Code, "");
			wiseCost20GP.Charges.Add(CreatePerUnitCharge(shipmentChargeCode.AC_Code, QuantityUnit.CN, CurrencyCodes.Australia, 8m));

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: null, receivingAgent: receivingAgent);
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.CreditorPK = carrier.PK;
			consol.JK_ConsolMode = ContainerModes.FCL;

			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			var packline1 = shipment.OuterPackLines.AddNew();

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "KIKI4100011";
			container1.JC_RC = cont20GP.PK;
			container1.JC_ContainerCount = 1;
			container1.JC_ContainerMode = ContainerModes.FCL;
			container1.PackLines.Add(packline1);

			Factory.Save();

			var expected = Array.Empty<AssertionCharge>();

			using (DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetEnabled()))
			{
				var testContext = new MockRatesServiceContext(new RatesSearchResponse
				{
					Rates = new[] { wiseCost20GP },
					Carriers = new[]
					{
						new RefCarrier { Code = "Emirates", SCACCode = carrier.SCACCode, Name = carrier.OH_FullName }
					},
					ChargeCodes = GetChargeCodesFromRates(wiseCost20GP)
				}, useCW1RatesProvider: true);

				AutorateAndAssertRatesService("Should not send request to rates service for Gateway shipments", expected, consol, testContext);
				var testLogger = testContext.TestLogger;
				var actualInfos = string.Join("\r\n", testLogger.Infos);
				AssertContains("Info:Rates Service: No Search Request is sent to Rates Service for Gateway shipments", actualInfos);
			}
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ShouldReturnCostsFromCostingWhenSendingAgentIsGTTAndReceivingAgentIsGTABothUnderSameCompanyAndLoginAsReceivingAgent()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			Helper.ChargeCodes.CreateGlobalCharge("FRT3");
			Helper.ChargeCodes.CreateGlobalCharge("FRT4");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateBranchProxy("USCHS");
			Factory.Save();

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(sendingIntercompanyTariff, "FRT1", 66m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT3", 44m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT4", 55m, origin, destination);

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, sendingAgent, origin, destination, "BAF", 15);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 15m,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT3",
					JR_OSSellAmt = 44m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "FRT4",
					JR_OSSellAmt = 55m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AssertEquals("Shipment's gateways should contain consol sending agent", consol.SendingForwarder.PK, shipment.Gateways[0].Forwarder.PK);
			AssertEquals("Receivng Agent type should be GTA", consol.JK_ReceivingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: receivingAgent.branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ShouldReturnCostsFromCostingWhenSendingAgentIsGTTAndReceivingAgentIsGTABothUnderSameCompanyWithCurrentLogin()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			Helper.ChargeCodes.CreateGlobalCharge("FRT3");
			Helper.ChargeCodes.CreateGlobalCharge("FRT4");

			var branchWithProxy = CreateBranchProxy("AUSYD");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateBranchProxy("USCHS");
			Factory.Save();

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(sendingIntercompanyTariff, "FRT1", 66m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT3", 44m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT4", 55m, origin, destination);

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, sendingAgent, origin, destination, "BAF", 15);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 15m,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT3",
					JR_OSSellAmt = 44m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "FRT4",
					JR_OSSellAmt = 55m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AssertEquals("Shipment's gateways should contain consol sending agent", consol.SendingForwarder.PK, shipment.Gateways[0].Forwarder.PK);
			AssertEquals("Receivng Agent type should be GTA", consol.JK_ReceivingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: branchWithProxy.branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ShouldReturnCostFromCostingAndIntercompanyTariffWhenLoginAsSendingAgentAndIsGTTAndReceivingAgentIsGTT()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			Helper.ChargeCodes.CreateGlobalCharge("FRT3");
			Helper.ChargeCodes.CreateGlobalCharge("FRT4");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateCompanyAndBranchProxy("USCHS");
			Factory.Save();

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(sendingIntercompanyTariff, "FRT1", 66m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT3", 44m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT4", 55m, origin, destination);

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, sendingAgent, origin, destination, "BAF", 15);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 15m,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT3",
					JR_OSSellAmt = 44m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "FRT4",
					JR_OSSellAmt = 55m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AssertEquals("Shipment's gateways should contain consol sending agent", consol.SendingForwarder.PK, shipment.Gateways[0].Forwarder.PK);
			AssertEquals("Receivng Agent type should be GTT", consol.JK_ReceivingForwarderHandlingType, AgentStatusList.Codes.GatewayAgentWithTariff);
			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ShouldNotReturnIntercompanyTariffWhenLoginAsSendingAgentAndIsGTTAndReceivingAgentIsGTT_WhenPrepaidAndCFR()
		{
			AssertByIncoterm(IncoTerms.CostAndFreight);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ShouldNotReturnIntercompanyTariffWhenLoginAsSendingAgentAndIsGTTAndReceivingAgentIsGTT_WhenPrepaidAndEXW()
		{
			AssertByIncoterm(IncoTerms.ExWorks);
		}

		void AssertByIncoterm(ZString incoterm)
		{
			var origin = "AUSYD";
			var destination = "NZAKL";
			var wrongOrigin = "AUMEL";
			var wrongDestination = "NZCHC";

			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			Helper.ChargeCodes.CreateGlobalCharge("FRT2");
			Factory.Save();

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateCompanyAndBranchProxy(destination);

			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);
			CreateFlatRate(receivingIntercompanyTariff, "FRT1", 33m, "", wrongOrigin);
			CreateFlatRate(receivingIntercompanyTariff, "FRT2", 44m, "", origin);
			Factory.Save();

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consignor1 = Helper.NewOrgHeader(closestPort: wrongOrigin);
			consignor1.OH_IsConsignor = true;
			var consignee1 = Helper.NewOrgHeader(closestPort: wrongDestination);
			consignee1.OH_IsConsignee = true;
			var consignor2 = Helper.NewOrgHeader(closestPort: origin);
			consignor2.OH_IsConsignor = true;
			var consignee2 = Helper.NewOrgHeader(closestPort: destination);
			consignee2.OH_IsConsignee = true;

			var shipment1 = CreateGatewayShipment(consol, wrongOrigin, wrongDestination, true, false, consignor1, consignee1);
			shipment1.JS_INCO = incoterm;
			var shipment2 = CreateGatewayShipment(consol, origin, destination, true, false, consignor2, consignee2);
			shipment2.JS_INCO = incoterm;

			var expected = Array.Empty<AssertionCharge>();

			AssertEquals("Shipment's gateways should contain consol sending agent", consol.SendingForwarder.PK, shipment1.Gateways[0].Forwarder.PK);
			AssertEquals("Shipment's gateways should contain consol sending agent", consol.SendingForwarder.PK, shipment2.Gateways[0].Forwarder.PK);
			AssertEquals("Receivng Agent type should be GTT", consol.JK_ReceivingForwarderHandlingType, AgentStatusList.Codes.GatewayAgentWithTariff);
			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ShouldReturnCostFromCostingAndIntercompanyTariffWhenLoginAsSendingAgentAndIsGTTAndReceivingAgentIsGTANotFirstGatewayInShipment()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			Helper.ChargeCodes.CreateGlobalCharge("FRT3");
			Helper.ChargeCodes.CreateGlobalCharge("FRT4");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateCompanyAndBranchProxy("USCHS");

			Factory.Save();

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(sendingIntercompanyTariff, "FRT1", 66m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT3", 44m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT4", 55m, origin, destination);

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, sendingAgent, origin, destination, "BAF", 15);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			var orgHeader = Helper.NewOrgHeader();
			var anotherGateway = shipment.Gateways.AddNew();
			anotherGateway.JSG_OA_ForwarderAddress = orgHeader.MainAddress.PK;
			anotherGateway.JSG_Sequence = 1;
			shipment.Gateways[0].JSG_Sequence = 2;
			shipment.Gateways[1].JSG_Sequence = 3;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 15m,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT3",
					JR_OSSellAmt = 44m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "FRT4",
					JR_OSSellAmt = 55m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			CombineAssertions("Shipment's gateways should contain consol sending agent as succedding gateway", () =>
			{
				AssertEquals(consol.SendingForwarder.PK, shipment.Gateways[0].Forwarder.PK);
				AssertEquals((byte)2, shipment.Gateways[0].JSG_Sequence);
			});

			AssertEquals("Receivng Agent type should be GTA", consol.JK_ReceivingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ShouldReturnCostFromCostingAndIntercompanyTariffWhenLoginAsSendingAgentAndIsGTTAndReceivingAgentIsGTANotFirstGatewayInShipmentBothUnderSameCompany()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			Helper.ChargeCodes.CreateGlobalCharge("FRT3");
			Helper.ChargeCodes.CreateGlobalCharge("FRT4");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateBranchProxy("USCHS");

			Factory.Save();

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(sendingIntercompanyTariff, "FRT1", 66m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT3", 44m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT4", 55m, origin, destination);

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, sendingAgent, origin, destination, "BAF", 15);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			var orgHeader = Helper.NewOrgHeader();
			var anotherGateway = shipment.Gateways.AddNew();
			anotherGateway.JSG_OA_ForwarderAddress = orgHeader.MainAddress.PK;
			anotherGateway.JSG_Sequence = 1;
			shipment.Gateways[0].JSG_Sequence = 2;
			shipment.Gateways[1].JSG_Sequence = 3;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 15m,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT3",
					JR_OSSellAmt = 44m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "FRT4",
					JR_OSSellAmt = 55m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			CombineAssertions("Shipment's gateways should contain consol sending agent as succedding gateway", () =>
			{
				AssertEquals(consol.SendingForwarder.PK, shipment.Gateways[0].Forwarder.PK);
				AssertEquals((byte)2, shipment.Gateways[0].JSG_Sequence);
			});

			AssertEquals("Receivng Agent type should be GTA", consol.JK_ReceivingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ShouldSetDebtorEmpty_WhenCalculateSellAmountIsZero()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			var charge = Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			charge.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateBranchProxy("USCHS");

			Factory.Save();

			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);
			CreateFlatRate(receivingIntercompanyTariff, "FRT1", 33m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			var orgHeader = Helper.NewOrgHeader();
			var anotherGateway = shipment.Gateways.AddNew();
			anotherGateway.JSG_OA_ForwarderAddress = orgHeader.MainAddress.PK;
			anotherGateway.JSG_Sequence = 1;
			shipment.Gateways[0].JSG_Sequence = 2;
			shipment.Gateways[1].JSG_Sequence = 3;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_LocalSellAmt = 0m,
					SellAccountCode = string.Empty,
					JR_LocalCostAmt = 33,
					CostAccountCode = receivingAgent.proxy.OH_Code,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		public void TestAutorateCostFromGatewayInvoicingMenu_ShouldSetCreditorDebtor_WhenICTHasExplicitZeroAmount()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			var charge = Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			charge.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateBranchProxy("USCHS");

			Factory.Save();

			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);
			CreateFlatRate(receivingIntercompanyTariff, "FRT1", 0m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			var orgHeader = Helper.NewOrgHeader();
			var anotherGateway = shipment.Gateways.AddNew();
			anotherGateway.JSG_OA_ForwarderAddress = orgHeader.MainAddress.PK;
			anotherGateway.JSG_Sequence = 1;
			shipment.Gateways[0].JSG_Sequence = 2;
			shipment.Gateways[1].JSG_Sequence = 3;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_LocalSellAmt = 0m,
					SellAccountCode = receivingAgent.proxy.OH_Code,
					JR_LocalCostAmt = 0,
					CostAccountCode = receivingAgent.proxy.OH_Code,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		public void TestAutorateCostFromGatewayInvoicingMenu_ShouldNotDefaultCreditor_WhenCreditorDoesNotHavePayablesEnabled()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			var charge = Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			charge.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateBranchProxy("USCHS");

			Factory.Save();

			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);
			CreateFlatRate(receivingIntercompanyTariff, "FRT1", 0m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol.JK_OA_CreditorAddress = receivingAgent.proxy.MainAddress.PK;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			var orgHeader = Helper.NewOrgHeader();
			var anotherGateway = shipment.Gateways.AddNew();
			anotherGateway.JSG_OA_ForwarderAddress = orgHeader.MainAddress.PK;
			anotherGateway.JSG_Sequence = 1;
			shipment.Gateways[0].JSG_Sequence = 2;
			shipment.Gateways[1].JSG_Sequence = 3;

			receivingAgent.proxy.OH_IsCreditor = false;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					CostAccountCode = ZString.Empty,
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ShouldNotReturnAnyCostWhenSendingAgentIsGTTAndReceivingAgentIsGTANotFirstGatewayInShipmentAndLoginAsReceivingAgentBothUnderSameCompany()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			Helper.ChargeCodes.CreateGlobalCharge("FRT3");
			Helper.ChargeCodes.CreateGlobalCharge("FRT4");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateBranchProxy("USCHS");

			Factory.Save();

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(sendingIntercompanyTariff, "FRT1", 66m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT3", 44m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT4", 55m, origin, destination);

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, sendingAgent, origin, destination, "BAF", 15);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			var orgHeader = Helper.NewOrgHeader();
			var anotherGateway = shipment.Gateways.AddNew();
			anotherGateway.JSG_OA_ForwarderAddress = orgHeader.MainAddress.PK;
			anotherGateway.JSG_Sequence = 1;
			shipment.Gateways[0].JSG_Sequence = 2;
			shipment.Gateways[1].JSG_Sequence = 3;

			Factory.Save();

			var expected = Array.Empty<AssertionCharge>();

			CombineAssertions("Shipment's gateways should contain consol sending agent as succedding gateway", () =>
			{
				AssertEquals(consol.SendingForwarder.PK, shipment.Gateways[0].Forwarder.PK);
				AssertEquals((byte)2, shipment.Gateways[0].JSG_Sequence);
			});

			AssertEquals("Receivng Agent type should be GTA", consol.JK_ReceivingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: receivingAgent.branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ShouldFilterRecevingGatewayAgentType()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			Helper.ChargeCodes.CreateGlobalCharge("FRT3");
			Helper.ChargeCodes.CreateGlobalCharge("FRT4");
			Helper.ChargeCodes.CreateGlobalCharge("FRT5");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateCompanyAndBranchProxy("USCHS");
			Factory.Save();

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(sendingIntercompanyTariff, "FRT1", 66m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT3", 44m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT4", 55m, origin, destination);
			CreateFlatRate(receivingIntercompanyTariff, "FRT5", 77m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.ReceivingAgent);

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, sendingAgent, origin, destination, "BAF", 15);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 15m,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT3",
					JR_OSSellAmt = 44m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "FRT4",
					JR_OSSellAmt = 55m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AssertEquals("Shipment's gateways should contain consol sending agent", consol.SendingForwarder.PK, shipment.Gateways[0].Forwarder.PK);
			AssertEquals("Receivng Agent type should be GTA", consol.JK_ReceivingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
			AssertAutoratingAuditLogNoteContainsLines(consol,
	"Information: AUTORATING COSTS FOR Shipment S00001000",
	"Information: RatingHeader Found Intercompany Tariff PROXYUSCHS Entries: 3",
	"Information: RateEntry Filtered Intercompany Tariff PROXYUSCHS reason: Gateway agent type RAG cannot be used for gateway shipment autorating.");
		}
		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ShouldFilterReceivingForImportGatewayAgentType()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			Helper.ChargeCodes.CreateGlobalCharge("FRT3");
			Helper.ChargeCodes.CreateGlobalCharge("FRT4");
			Helper.ChargeCodes.CreateGlobalCharge("FRT5");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateCompanyAndBranchProxy("USCHS");
			Factory.Save();

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(sendingIntercompanyTariff, "FRT1", 66m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT3", 44m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT4", 55m, origin, destination);
			CreateFlatRate(receivingIntercompanyTariff, "FRT5", 77m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.ReceivingAgentForImport);

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, sendingAgent, origin, destination, "BAF", 15);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 15m,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT3",
					JR_OSSellAmt = 44m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "FRT4",
					JR_OSSellAmt = 55m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AssertEquals("Shipment gateways should contain consol sending agent", consol.SendingForwarder.PK, shipment.Gateways[0].Forwarder.PK);
			AssertEquals("Receivng Agent type should be GTA", consol.JK_ReceivingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
			AssertAutoratingAuditLogNoteContainsLines(consol,
	"Information: AUTORATING COSTS FOR Shipment S00001000",
	"Information: RatingHeader Found Intercompany Tariff PROXYUSCHS Entries: 3",
	"Information: RateEntry Filtered Intercompany Tariff PROXYUSCHS reason: Gateway agent type RIM cannot be used for gateway shipment autorating.");
		}

		public void TestAutorateCostFromGatewayInvoicingMenu_ShouldUseShipmentOriginRateOverConsolLastDischargeForPlannedLoad()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			Helper.ChargeCodes.CreateGlobalCharge("FRT2");
			Helper.ChargeCodes.CreateGlobalCharge("FRT3");
			Helper.ChargeCodes.CreateGlobalCharge("FRT4");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateCompanyAndBranchProxy("USCHS");
			Factory.Save();

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(sendingIntercompanyTariff, "FRT1", 66m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT3", 44m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT4", 55m, origin, destination);
			CreateFlatRate(receivingIntercompanyTariff, "FRT2", 77m, origin: "", destination: "", plannedLoad: origin);

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, sendingAgent, origin, destination, "BAF", 15);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 15m,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT3",
					JR_OSSellAmt = 44m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "FRT4",
					JR_OSSellAmt = 55m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "FRT2",
					JR_OSSellAmt = 77m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AssertEquals("Shipment's gateways should contain consol sending agent", consol.SendingForwarder.PK, shipment.Gateways[0].Forwarder.PK);
			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_PlannedLoadShouldUseConsolLastDischarge()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			Helper.ChargeCodes.CreateGlobalCharge("FRT2");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateCompanyAndBranchProxy("USCHS");
			Factory.Save();

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(sendingIntercompanyTariff, "FRT1", 66m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT2", 33m, origin: "", destination: "", plannedLoad: "AUMEL");

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, sendingAgent, origin, destination: "", "BAF", 15);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, "AUMEL", sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 15m,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT2",
					JR_OSSellAmt = 33m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ConsolLastDischargeTakesPriorityOverReceivingAgentPortForPlannedLoad()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			Helper.ChargeCodes.CreateGlobalCharge("FRT2");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateCompanyAndBranchProxy("USCHS");
			Factory.Save();

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(sendingIntercompanyTariff, "FRT1", 66m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT2", 33m, origin: "", destination: "", plannedLoad: receivingAgent.proxy.MainAddress.OA_RL_NKRelatedPortCode);
			CreateFlatRate(receivingIntercompanyTariff, "FRT2", 44m, origin: "", destination: "", plannedLoad: "AUMEL");

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, sendingAgent, origin, destination: "", "BAF", 15);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, "AUMEL", sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 15m,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT2",
					JR_OSSellAmt = 44m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_PlannedDischargeShouldUseShipmentPlannedDischarge()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			Helper.ChargeCodes.CreateGlobalCharge("FRT2");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateCompanyAndBranchProxy("USCHS");
			Factory.Save();

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(sendingIntercompanyTariff, "FRT1", 66m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT2", 33m, origin: "", destination: "", plannedDischarge: "AUMEL");

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, sendingAgent, origin, destination: destination, "BAF", 15);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);
			shipment.JS_RL_NKDischargePort = "AUMEL";

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 15m,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT2",
					JR_OSSellAmt = 33m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_PlannedDischargeShouldUseShipmentPlannedDischargeAndPlannedLoadShouldUseConsolLastDischarge()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			Helper.ChargeCodes.CreateGlobalCharge("FRT2");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateCompanyAndBranchProxy("USCHS");
			Factory.Save();

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(sendingIntercompanyTariff, "FRT1", 66m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT2", 33m, origin: "", destination: "", plannedLoad: "AUBNE", plannedDischarge: "AUMEL");

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, sendingAgent, origin, destination: "AUBNE", "BAF", 15);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, "AUBNE", sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);
			shipment.JS_RL_NKDischargePort = "AUMEL";

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 15m,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT2",
					JR_OSSellAmt = 33m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_PlannedDischargeShouldFallbackToConsolLastDischarge_WhenConsolSendingAgentWasInShipmentGateways()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			Helper.ChargeCodes.CreateGlobalCharge("FRT2");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateCompanyAndBranchProxy("USCHS");
			Factory.Save();

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(sendingIntercompanyTariff, "FRT1", 66m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT2", 33m, origin: "", destination: "", plannedDischarge: "AUMEL");

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, sendingAgent, origin, destination: "", "BAF", 15);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, "AUMEL", sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			var orgHeader = Helper.NewOrgHeader(); // just to make sure sending agent is not the first gateway in shipment
			var anotherGateway = shipment.Gateways.AddNew();
			anotherGateway.JSG_OA_ForwarderAddress = orgHeader.MainAddress.PK;
			anotherGateway.JSG_Sequence = 1;
			shipment.Gateways[0].JSG_Sequence = 2;
			shipment.Gateways[1].JSG_Sequence = 3;

			Factory.Save();

			CombineAssertions("Shipment's gateways should contain consol sending agent as succedding gateway", () =>
			{
				AssertEquals(consol.SendingForwarder.PK, shipment.Gateways[0].Forwarder.PK);
				AssertEquals((byte)2, shipment.Gateways[0].JSG_Sequence);
			});

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 15m,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT2",
					JR_OSSellAmt = 33m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ShipmentPlannedDischargeTakesPriorityOverConsolLastDischarge()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			Helper.ChargeCodes.CreateGlobalCharge("FRT2");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateCompanyAndBranchProxy("USCHS");
			Factory.Save();

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(sendingIntercompanyTariff, "FRT1", 66m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT2", 33m, origin: "", destination: "", plannedDischarge: "AUMEL");
			CreateFlatRate(receivingIntercompanyTariff, "FRT2", 44m, origin: "", destination: "", plannedDischarge: "AUBNE");

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, sendingAgent, origin, destination: "", "BAF", 15);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, "AUMEL", sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);
			shipment.JS_RL_NKDischargePort = "AUBNE";

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 15m,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT2",
					JR_OSSellAmt = 44m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_PlannedDischargeShouldFallbackToConsolLastDischarge_WhenReceivingAgentIsInShipmentGateways()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			Helper.ChargeCodes.CreateGlobalCharge("FRT2");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateCompanyAndBranchProxy("USCHS");
			Factory.Save();

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(sendingIntercompanyTariff, "FRT1", 66m, "NZAKL", "AUSYD", gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT2", 33m, origin: "", destination: "", plannedDischarge: "AUMEL");

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, sendingAgent, "NZAKL", "AUSYD", "BAF", 15);

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUMEL", sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "NZAKL", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, "AUSYD", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			CreateGatewayShipment(consol, "NZAKL", "AUSYD", true);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT2",
					JR_OSSellAmt = 33m,
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_PlannedDischargeShouldFallbackToConsolLastDischarge_ChargeWithLargerOrderInShipmentGatewaysTakesPriority()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			Helper.ChargeCodes.CreateGlobalCharge("FRT2");
			Helper.ChargeCodes.CreateGlobalCharge("FRT3");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateCompanyAndBranchProxy("USCHS");
			Factory.Save();

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(sendingIntercompanyTariff, "FRT1", 66m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT2", 33m, origin: "", destination: "", plannedDischarge: "AUMEL");
			CreateFlatRate(receivingIntercompanyTariff, "FRT2", 44m, origin: "", destination: "", plannedDischarge: "AUFRE");

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, sendingAgent, origin, destination: "", "BAF", 15);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, "AUMEL", sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);

			var orgHeader = Helper.NewOrgHeader(); // just to make sure sending agent is not the first gateway in shipment
			var anotherGateway = shipment.Gateways.AddNew();
			anotherGateway.JSG_OA_ForwarderAddress = orgHeader.MainAddress.PK;
			anotherGateway.JSG_Sequence = 3;

			var consol1 = CreateForwardingConsolWithGatewayAgents("AUBNE", "AUFRE", sendingAgent: orgHeader, receivingAgent: null, checkIsItGateway: false);
			shipment.Consols.Add(consol1);

			Factory.Save();

			CombineAssertions("Shipment's gateways should contain consol sending agent as First gateway", () =>
			{
				AssertEquals(consol.SendingForwarder.PK, shipment.Gateways[0].Forwarder.PK);
				AssertEquals((byte)1, shipment.Gateways[0].JSG_Sequence);

				AssertEquals(orgHeader.PK, shipment.Gateways[2].Forwarder.PK);
				AssertEquals((byte)3, shipment.Gateways[2].JSG_Sequence);
			});

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 15m,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT2",
					JR_OSSellAmt = 44m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_PlannedLoadHasPriorityOverPlannedDischarge()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			Helper.ChargeCodes.CreateGlobalCharge("FRT2");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateCompanyAndBranchProxy("USCHS");
			Factory.Save();

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(sendingIntercompanyTariff, "FRT1", 66m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			CreateFlatRate(receivingIntercompanyTariff, "FRT2", 33m, origin: "", destination: "", plannedLoad: "AUMEL");
			CreateFlatRate(receivingIntercompanyTariff, "FRT2", 44m, origin: "", destination: "", plannedDischarge: "AUBNE");

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, sendingAgent, origin, destination: "", "BAF", 15);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, "AUMEL", sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);
			shipment.JS_RL_NKDischargePort = "AUBNE";

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 15m,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT2",
					JR_OSSellAmt = 33m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ControllingCustomer_MatchesNeitherControllingCustomerNorLocalClientOnShipment_FallbackToPlainRate()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			Helper.ChargeCodes.CreateGlobalCharge("FRT1");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateCompanyAndBranchProxy("USCHS");
			var anyCustomer1 = Helper.NewOrgHeader("ANY_CLIENT_1");
			var anyCustomer2 = Helper.NewOrgHeader("ANY_CLIENT_2");
			var controllingCustomer = Helper.NewOrgHeader("CONT_CLIENT");
			Factory.Save();

			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(receivingIntercompanyTariff, "FRT1", 100m, "NZAKL", "AUSYD").TI_OH_ControllingCustomer = controllingCustomer.PK;
			CreateFlatRate(receivingIntercompanyTariff, "FRT1", 200m, "NZAKL", "AUSYD");

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", true);
			shipment.JobHeader.JH_OA_LocalChargesAddr = anyCustomer1.MainAddress.PK;
			shipment.ControllingCustomerNameOrPK = anyCustomer2.PK.ToString();

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 200m,
				},
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ControllingCustomer_DoesNotMatchControllingCustomerAndThereIsNoLocalClientOnShipment_FallbackToPlainRate()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			Helper.ChargeCodes.CreateGlobalCharge("FRT1");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateCompanyAndBranchProxy("USCHS");
			var anyCustomer1 = Helper.NewOrgHeader("ANY_CLIENT_1");
			var anyCustomer2 = Helper.NewOrgHeader("ANY_CLIENT_2");
			var controllingCustomer = Helper.NewOrgHeader("CONT_CLIENT");
			Factory.Save();

			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(receivingIntercompanyTariff, "FRT1", 100m, "NZAKL", "AUSYD").TI_OH_ControllingCustomer = controllingCustomer.PK;
			CreateFlatRate(receivingIntercompanyTariff, "FRT1", 200m, "NZAKL", "AUSYD");

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", true);
			shipment.JobHeader.JH_OA_LocalChargesAddr = ZGuid.Empty;
			shipment.ControllingCustomerNameOrPK = anyCustomer2.PK.ToString();

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 200m,
				},
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAutorateCost_ControllingCustomer_WhenShipmentIsPrepaid_PriotiryIsControllingCustomer_LocalClient_ConsignorIFTRelatedParties_Consignor()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			var (agent1, branch1) = CreateCompanyAndBranchProxy("ZAJNB");
			var (agent2, branch2) = CreateCompanyAndBranchProxy("HKHKG");
			var (agent3, _) = CreateCompanyAndBranchProxy("SGSIN");
			var (agent4, _) = CreateCompanyAndBranchProxy("THBKK");

			var orgIFC = Factory.NewWithValidTestData<OrgRelatedParty>();
			orgIFC.PR_PartyType = RelatedPartyTypeList.Codes.InvoiceFreightJobsTo;
			var orgICT = Factory.NewWithValidTestData<OrgRelatedParty>();
			orgICT.PR_PartyType = RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo;

			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(agent2);

			CreateFlatRate(receivingIntercompanyTariff, "FRT1", 100m, "HKHKG", "SGSIN").TI_OH_ControllingCustomer = Consignor.PK;
			CreateFlatRate(receivingIntercompanyTariff, "FRT1", 200m, "HKHKG", "SGSIN");

			var consol = CreateForwardingConsolWithGatewayAgents("ZAJNB", "HKHKG", sendingAgent: agent1, receivingAgent: agent2, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "ZAJNB", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			var consol2 = CreateForwardingConsolWithGatewayAgents("HKHKG", "SGSIN", agent2, agent3, checkIsItGateway: false);
			var consol3 = CreateForwardingConsolWithGatewayAgents("SGSIN", "THBKK", agent3, agent4, checkIsItGateway: false);

			var shipment = CreateGatewayShipment(consol, "HKHKG", "SGSIN", false);
			shipment.Consols.Add(consol2);
			shipment.Consols.Add(consol3);

			shipment.Consignor.ConsignorRelatedParties.Add(orgIFC);
			shipment.Consignor.ConsignorRelatedParties.Add(orgICT);
			shipment.JobHeader.JH_OA_LocalChargesAddr = ZGuid.Empty;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var job = CreateJob(shipment, shipment.JS_UniqueConsignRef);
				shipment.JobHeader.JH_OA_LocalChargesAddr = agent2.MainAddress.PK;
				Factory.Save();
			}

			AssertEquals("Shipment should be Prepaid", shipment.IsPrepaid, true);
			AssertEquals("Shipment Controlling Customer should be empty", shipment.ControllingCustomerNameOrPK, Guid.Empty.ToString());

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 100m,
				},
			};

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("Shipment should be cross trade", shipment.IsCrossTrade(), true);
				AssertEquals(shipment.Consignor, Consignor);
				AutorateAndAssertWithUserContextOverride(expected, consol, agent1, autorateCosts: true, autorateRevenue: false);
			}

			CreateFlatRate(receivingIntercompanyTariff, "FRT1", 80m, "HKHKG", "SGSIN").TI_OH_ControllingCustomer = orgIFC.PR_OH_RelatedParty;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 80m,
				},
			};

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("Shipment should be cross trade", shipment.IsCrossTrade(), true);
				AssertEquals(shipment.Consignor, Consignor);
				AutorateAndAssertWithUserContextOverride(expected, consol, agent1, autorateCosts: true, autorateRevenue: false);
			}

			CreateFlatRate(receivingIntercompanyTariff, "FRT1", 60m, "HKHKG", "SGSIN").TI_OH_ControllingCustomer = agent2.PK;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 60m,
				},
			};

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("Shipment should be cross trade", shipment.IsCrossTrade(), true);
				AutorateAndAssertWithUserContextOverride(expected, consol, agent1, autorateCosts: true, autorateRevenue: false);
			}

			var anyCustomer2 = Helper.NewOrgHeader("ANY_CLIENT_2");
			CreateFlatRate(receivingIntercompanyTariff, "FRT1", 40m, "HKHKG", "SGSIN").TI_OH_ControllingCustomer = anyCustomer2.PK;
			shipment.ControllingCustomerNameOrPK = anyCustomer2.PK.ToString();

			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 40m,
				},
			};

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("Shipment should be cross trade", shipment.IsCrossTrade(), true);
				AutorateAndAssertWithUserContextOverride(expected, consol, agent1, autorateCosts: true, autorateRevenue: false);
			}
		}

		[TestDate(2019, 11, 12)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAutorateCost_ControllingCustomer_WhenShipmentIsCollect_PriotiryIsControllingCustomer_LocalClient_ConsignorIFTRelatedParties_Consignee()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			var (agent1, branch1) = CreateCompanyAndBranchProxy("ZAJNB");
			var (agent2, _) = CreateCompanyAndBranchProxy("HKHKG");
			var (agent3, branch3) = CreateCompanyAndBranchProxy("SGSIN");
			var (agent4, _) = CreateCompanyAndBranchProxy("THBKK");

			var orgIFC = Factory.NewWithValidTestData<OrgRelatedParty>();
			orgIFC.PR_PartyType = RelatedPartyTypeList.Codes.InvoiceFreightJobsTo;
			var orgICT = Factory.NewWithValidTestData<OrgRelatedParty>();
			orgICT.PR_PartyType = RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo;

			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(agent2);

			CreateFlatRate(receivingIntercompanyTariff, "FRT1", 100m, "HKHKG", "SGSIN").TI_OH_ControllingCustomer = Consignee.PK;
			CreateFlatRate(receivingIntercompanyTariff, "FRT1", 200m, "HKHKG", "SGSIN");

			var consol = CreateForwardingConsolWithGatewayAgents("ZAJNB", "HKHKG", sendingAgent: agent1, receivingAgent: agent2, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "ZAJNB", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consol2 = CreateForwardingConsolWithGatewayAgents("HKHKG", "SGSIN", agent2, agent3, checkIsItGateway: false);
			var consol3 = CreateForwardingConsolWithGatewayAgents("SGSIN", "THBKK", agent3, agent4, checkIsItGateway: false);

			var shipment = CreateGatewayShipment(consol, "HKHKG", "SGSIN", true);
			shipment.Consols.Add(consol2);
			shipment.Consols.Add(consol3);

			shipment.Consignee.ConsigneeRelatedParties.Add(orgIFC);
			shipment.Consignee.ConsigneeRelatedParties.Add(orgICT);
			shipment.JobHeader.JH_OA_LocalChargesAddr = ZGuid.Empty;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch3.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var job = CreateJob(shipment, shipment.JS_UniqueConsignRef);
				shipment.JobHeader.JH_OA_LocalChargesAddr = agent3.MainAddress.PK;
				Factory.Save();
			}

			Factory.Save();

			AssertEquals("Shipment should be Prepaid", shipment.IsCollect, true);
			AssertEquals("Shipment Controlling Customer should be empty", shipment.ControllingCustomerNameOrPK, Guid.Empty.ToString());

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 100m,
				},
			};

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("Shipment should be cross trade", shipment.IsCrossTrade(), true);
				AssertEquals(shipment.Consignee, Consignee);
				AutorateAndAssertWithUserContextOverride(expected, consol, agent1, autorateCosts: true, autorateRevenue: false);
			}

			CreateFlatRate(receivingIntercompanyTariff, "FRT1", 80m, "HKHKG", "SGSIN").TI_OH_ControllingCustomer = orgIFC.PR_OH_RelatedParty;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 80m,
				},
			};

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("Shipment should be cross trade", shipment.IsCrossTrade(), true);
				AssertEquals(shipment.Consignee, Consignee);
				AutorateAndAssertWithUserContextOverride(expected, consol, agent1, autorateCosts: true, autorateRevenue: false);
			}

			CreateFlatRate(receivingIntercompanyTariff, "FRT1", 60m, "HKHKG", "SGSIN").TI_OH_ControllingCustomer = agent3.PK;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 60m,
				},
			};

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("Shipment should be cross trade", shipment.IsCrossTrade(), true);
				AutorateAndAssertWithUserContextOverride(expected, consol, agent1, autorateCosts: true, autorateRevenue: false);
			}

			var anyCustomer2 = Helper.NewOrgHeader("ANY_CLIENT_2");
			CreateFlatRate(receivingIntercompanyTariff, "FRT1", 40m, "HKHKG", "SGSIN").TI_OH_ControllingCustomer = anyCustomer2.PK;
			shipment.ControllingCustomerNameOrPK = anyCustomer2.PK.ToString();

			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 40m,
				},
			};

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("Shipment should be cross trade", shipment.IsCrossTrade(), true);
				AutorateAndAssertWithUserContextOverride(expected, consol, agent1, autorateCosts: true, autorateRevenue: false);
			}
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ControllingCustomer_MatchesShipmentControllingCustomer()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			Helper.ChargeCodes.CreateGlobalCharge("FRT1");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateCompanyAndBranchProxy("USCHS");
			var controllingCustomer = Helper.NewOrgHeader("CONT_CLIENT");
			Factory.Save();

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(receivingIntercompanyTariff, "FRT1", 100m, "NZAKL", "AUSYD").TI_OH_ControllingCustomer = controllingCustomer.PK;
			CreateFlatRate(receivingIntercompanyTariff, "FRT1", 200m, "NZAKL", "AUSYD");

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			CreateGatewayShipment(consol, "NZAKL", "AUSYD", true).ControllingCustomerNameOrPK = controllingCustomer.PK.ToString();

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 100m,
				},
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ControllingCustomer_MatchesShipmentControllingCustomer_AsPriority()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			Helper.ChargeCodes.CreateGlobalCharge("FRT1");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateCompanyAndBranchProxy("USCHS");
			var anyCustomer = Helper.NewOrgHeader("ANY_CLIENT_1");
			var controllingCustomer = Helper.NewOrgHeader("CONT_CLIENT");
			Factory.Save();

			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(receivingIntercompanyTariff, "FRT1", 100m, "NZAKL", "AUSYD").TI_OH_ControllingCustomer = controllingCustomer.PK;
			CreateFlatRate(receivingIntercompanyTariff, "FRT1", 200m, "NZAKL", "AUSYD").TI_OH_ControllingCustomer = anyCustomer.PK;
			CreateFlatRate(receivingIntercompanyTariff, "FRT1", 300m, "NZAKL", "AUSYD");

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", true);
			shipment.JobHeader.JH_OA_LocalChargesAddr = anyCustomer.MainAddress.PK;
			shipment.ControllingCustomerNameOrPK = controllingCustomer.PK.ToString();

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 100m,
				},
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 11, 12)]
		public void TestAutorateCostFromGatewayInvoicingMenu_ControllingCustomer_MatchesShipmentControllingCustomer_WithAgentRate()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			Helper.ChargeCodes.CreateGlobalCharge("CAT");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateCompanyAndBranchProxy("USCHS");
			var controllingCustomer = Helper.NewOrgHeader("CONT_CLIENT");
			Factory.Save();

			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			CreateFlatRate(receivingIntercompanyTariff, "FRT1", 100m, "NZAKL", "AUSYD").TI_OH_ControllingCustomer = controllingCustomer.PK;
			CreateFlatRate(receivingIntercompanyTariff, "FRT1", 200m, "NZAKL", "AUSYD");
			CreateFlatRate(receivingIntercompanyTariff, "CAT", 10m, "NZAKL", "AUSYD", gatewayAgentType: GatewayAgentType.Codes.SendingAgent).TI_OH_ControllingCustomer = controllingCustomer.PK;
			CreateFlatRate(receivingIntercompanyTariff, "CAT", 20m, "NZAKL", "AUSYD", gatewayAgentType: GatewayAgentType.Codes.SendingAgent);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", true);
			shipment.ControllingCustomerNameOrPK = controllingCustomer.PK.ToString();

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 100m,
				},
				new AssertionCharge
				{
					ChargeCode = "CAT",
					JR_OSSellAmt = 10m,
				},
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		RateLine CreateRateEntryWithRateLine(string ratingHeaderType, OrgHeader client, ZString origin, ZString destination, ZString chargeCode, ZDecimal baseRate,
			string category = TransportModes.Air, string mode = ContainerModes.Loose)
		{
			RatingHeader ratingHeader = null;
			switch (ratingHeaderType)
			{
				case RatingHeaderTypes.ClientRate:
					ratingHeader = Helper.NewClientRate(client);
					break;
				case RatingHeaderTypes.Costing:
					ratingHeader = Helper.NewCosting(client);
					break;
				case RatingHeaderTypes.Tariff:
					ratingHeader = Factory.New<CompanyTariff>();
					break;
				case RatingHeaderTypes.IntercompanyTariff:
					ratingHeader = Helper.NewIntercompanyTariff(client);
					break;
				case RatingHeaderTypes.Quote:
					ratingHeader = Helper.NewQuote(client);
					break;
			}

			var rateEntry = ratingHeader.AddRateEntry(category, mode, origin, destination);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine(chargeCode, FlatCalculator.Code, ZString.Empty, CurrencyCodes.Australia);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = baseRate;
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode(rateLine.ChargeCode);

			return rateLine;
		}

		#endregion

		#region Autorate Shipment

		[TestDate(2021, 01, 01)]
		public void TestAutorateShipmentCost_Export_ShipmentIsNotAttachedToConsol_LoginCompanyIsDifferentToFirstGatewayAgent_LoadICTFromFirstGatewayAgent()
		{
			CreateGlobalCharge("FRT1");
			CreateGlobalCharge("BAF1");

			Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "AU", "US", "ORG1", 50);

			var (agent1, _) = CreateCompanyAndBranchProxy("CHBSL");
			agent1.OH_IsCreditor = true;
			var tariff1 = Helper.NewIntercompanyTariff(agent1);
			CreateFlatRate(tariff1, "FRT1", 100m, "AUMEL", "USLAX");

			var (agent2, _) = CreateCompanyAndBranchProxy("DEBRE");
			agent2.OH_IsCreditor = true;
			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "BAF1", 120m, "AUMEL", "USLAX");

			TransportProvider1.OH_IsCreditor = true;
			AssertEquals("Current Company Org Proxy is on AUBNE", "AUBNE", GlbBranch.CurrentBranch.GB_RL_NKHomePort);

			var shipment = CreateStandaloneGatewayShipment("AUMEL", "USLAX", true, agent1.PK, agent2.PK);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					CostAccountCode = agent1.OH_Code,
					JR_OSCostAmt = 100m,
				},
				new AssertionCharge
				{
					ChargeCode = "ORG1",
					CostAccountCode = TransportProvider1.OH_Code,
					JR_OSCostAmt = 50m
				}
			};

			// BAF1 should not be loaded
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, autorateRevenue: false);
		}

		[TestDate(2021, 01, 01)]
		public void TestAutorateShipmentCost_Export_ShipmentIsNotAttachedToConsol_LoginUnderFirstGatewayAgentWithSameBranch_LoadICTFromFirstGatewayAgent()
		{
			CreateGlobalCharge("FRT1");

			Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);

			var orgProxy = GlbCompany.CurrentCompany.OrgProxy;

			Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "AU", "US", "ORG1", 50);

			var tariff1 = Helper.NewIntercompanyTariff(orgProxy);
			CreateFlatRate(tariff1, "FRT1", 100m, "AUBNE", "USLAX");

			TransportProvider1.OH_IsCreditor = true;
			AssertEquals("Current Company Org Proxy is on AUBNE", "AUBNE", GlbBranch.CurrentBranch.GB_RL_NKHomePort);

			var shipment = CreateStandaloneGatewayShipment("AUBNE", "USLAX", true, orgProxy.PK);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					CostAccountCode = orgProxy.OH_Code,
					JR_OSCostAmt = 100m,
				},
				new AssertionCharge
				{
					ChargeCode = "ORG1",
					CostAccountCode = TransportProvider1.OH_Code,
					JR_OSCostAmt = 50m
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, autorateRevenue: false);
		}

		[TestDate(2021, 01, 01)]
		public void TestAutorateShipmentCost_Export_ShipmentIsNotAttachedToConsol_WhenAgetTypeIsRIM_ShouldNotLoadICTFromFirstGatewayAgent()
		{
			CreateGlobalCharge("FRT1");

			Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);

			var orgProxy = GlbCompany.CurrentCompany.OrgProxy;

			Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "AU", "US", "ORG1", 50);

			var tariff1 = Helper.NewIntercompanyTariff(orgProxy);
			CreateFlatRate(tariff1, "FRT1", 100m, "AUBNE", "USLAX", gatewayAgentType: "RIM");

			TransportProvider1.OH_IsCreditor = true;
			AssertEquals("Current Company Org Proxy is on AUBNE", "AUBNE", GlbBranch.CurrentBranch.GB_RL_NKHomePort);

			var shipment = CreateStandaloneGatewayShipment("AUBNE", "USLAX", true, orgProxy.PK);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ORG1",
					CostAccountCode = TransportProvider1.OH_Code,
					JR_OSCostAmt = 50m
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, autorateRevenue: false);
			AssertAutoratingAuditLogNoteContainsLinesWithUserContextOverride(shipment, GlbBranch.CurrentBranch,
				"Information: RateEntry Filtered Intercompany Tariff EDICUS reason: Gateway agent type RIM cannot be used for shipment autorating.");
		}

		[TestDate(2021, 01, 01)]
		public void TestAutorateShipmentCost_Export_ShipmentIsNotAttachedToConsol_LoginUnderFirstGatewayAgentButDifferentBranch_LoadICTFromFirstGatewayAgent()
		{
			CreateGlobalCharge("FRT1");
			var orgProxy = GlbCompany.CurrentCompany.OrgProxy;

			var tariff1 = Helper.NewIntercompanyTariff(orgProxy);
			CreateFlatRate(tariff1, "FRT1", 100m, "AUMEL", "USLAX");

			AssertEquals("Current Company Org Proxy is on AUBNE", "AUBNE", GlbBranch.CurrentBranch.GB_RL_NKHomePort);

			var shipment = CreateStandaloneGatewayShipment("AUMEL", "USLAX", true, orgProxy.PK);
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					CostAccountCode = orgProxy.OH_Code,
					JR_OSCostAmt = 100m
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, autorateRevenue: false);
		}

		[TestDate(2021, 01, 01)]
		public void TestAutorateShipmentCost_WhenGatewayAgentAddressIsEmpty_ShouldNotThrowAnException()
		{
			CreateGlobalCharge("FRT1");
			var orgProxy = GlbCompany.CurrentCompany.OrgProxy;

			var tariff1 = Helper.NewIntercompanyTariff(orgProxy);
			CreateFlatRate(tariff1, "FRT1", 100m, "AUMEL", "USLAX");

			AssertEquals("Current Company Org Proxy is on AUBNE", "AUBNE", GlbBranch.CurrentBranch.GB_RL_NKHomePort);

			var shipment = CreateStandaloneGatewayShipment("AUMEL", "USLAX", true, orgProxy.PK);
			Factory.Save();

			shipment.Gateways[0].JSG_OA_ForwarderAddress = ZGuid.Empty;

			var expected = Array.Empty<AssertionCharge>();

			AssertNoExceptionThrown(() => AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, autorateRevenue: false));
		}

		[TestDate(2019, 12, 05)]
		public void TestAutorateShipmentCost_Export_LoginAsNonGateway_LoadIntercompanyTariffForNextGateway()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("CAF");
			CreateGlobalCharge("BAF");

			var (agent1, branch1) = CreateNonProxyBranch("CHBSL");
			var (agent2, _) = CreateCompanyAndBranchProxy("BEANR");
			var (agent3, _) = CreateCompanyAndBranchProxy("DEBRE");
			var (agent4, _) = CreateCompanyAndBranchProxy("USCHS");

			var chargeCode = Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			chargeCode.AC_GC = branch1.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;

				Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "US", "ORG1", 50);
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent1);
			CreateFlatRate(tariff1, "FRT", 100m, "CHBSL", "USHOU");

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "CAF", 110m, "CHBSL", "USHOU");

			var tariff3 = Helper.NewIntercompanyTariff(agent3);
			CreateFlatRate(tariff3, "BAF", 120m, "CHBSL", "USHOU");

			TransportProvider1.OH_IsCreditor = true;

			var shipment = CreateStandaloneGatewayShipment("CHBSL", "USHOU", true, agent2.PK, agent3.PK, agent4.PK);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "CAF",
					CostAccountCode = agent2.OH_Code
				},
				new AssertionCharge
				{
					ChargeCode = "ORG1",
					CostAccountCode = TransportProvider1.OH_Code
				}
			};

			// NON gateway - EXP - Cost for next gateway
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		[TestDate(2019, 12, 05)]
		public void TestAutorateShipmentCost_Export_LoginAsNonGateway_LoadIntercompanyTariffForNextGateway_EntriesWithSameChargeCode_ShouldPreferJobOriginToJobRateOrigin()
		{
			CreateGlobalCharge("FRT");

			var (agent1, branch1) = CreateNonProxyBranch("CHBSL");
			var (agent2, branch2) = CreateCompanyAndBranchProxy("BEANR");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
			}

			var tariff = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff, "FRT", 100m, "CHBSL", "USHOU");
			CreateFlatRate(tariff, "FRT", 150m, "BEANR", "USHOU");

			TransportProvider1.OH_IsCreditor = true;

			Factory.Save();

			var shipment = CreateStandaloneGatewayShipment("CHBSL", "USHOU", true, agent2.PK);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			shipment.JS_RL_NKFreightRateOrigin = "BEANR";

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 100m
				},
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);

			AssertAutoratingAuditLogNoteContainsLinesWithUserContextOverride(shipment, branch1,
				"Information: RateLine Filtered FRT-FLT-Intercompany Tariff PROXYBEANR\treason:\toverridden by FRT-FLT-Intercompany Tariff PROXYBEANR by Origin Destination comparer");
		}

		[TestDate(2019, 12, 05)]
		public void TestAutorateShipmentCost_Export_LoginAsNonGateway_LoadIntercompanyTariffForNextGateway_EntriesWithSameChargeCode_ShouldPreferJobDestinationToJobRateDestination()
		{
			CreateGlobalCharge("FRT");

			var (agent1, branch1) = CreateNonProxyBranch("CHBSL");
			var (agent2, branch2) = CreateCompanyAndBranchProxy("BEANR");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
			}

			var tariff = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff, "FRT", 100m, "CHBSL", "USHOU");
			CreateFlatRate(tariff, "FRT", 150m, "CHBSL", "USCHI");

			TransportProvider1.OH_IsCreditor = true;

			Factory.Save();

			var shipment = CreateStandaloneGatewayShipment("CHBSL", "USHOU", true, agent2.PK);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			shipment.JS_RL_NKFreightRateDestination = "USCHI";

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 100m
				},
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);

			AssertAutoratingAuditLogNoteContainsLinesWithUserContextOverride(shipment, branch1,
				"Information: RateLine Filtered FRT-FLT-Intercompany Tariff PROXYBEANR\treason:\toverridden by FRT-FLT-Intercompany Tariff PROXYBEANR by Origin Destination comparer");
		}

		[TestDate(2019, 12, 05)]
		public void TestAutorateShipmentCost_Export_LoginAsNonGateway_LoadIntercompanyTariffForNextGateway_FallbackToRateOriginDestination()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("BAF");

			var (agent1, branch1) = CreateNonProxyBranch("CHBSL");
			var (agent2, branch2) = CreateCompanyAndBranchProxy("BEANR");
			var (agent3, branch3) = CreateCompanyAndBranchProxy("DEBRE");

			var chargeCode = Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			chargeCode.AC_GC = branch1.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;

				Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "US", "ORG1", 50);
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent1);
			CreateFlatRate(tariff1, "FRT", 100m, "CHBRL", "USHOT");

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "BAF", 110m, "CHBRL", "USHOT");

			TransportProvider1.OH_IsCreditor = true;

			var shipment = CreateStandaloneGatewayShipment("CHBSL", "USHOU", true, agent2.PK, agent3.PK);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			shipment.JS_RL_NKFreightRateOrigin = "CHBRL";
			shipment.JS_RL_NKFreightRateDestination = "USHOT";
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF"
				},
				new AssertionCharge
				{
					ChargeCode = "ORG1"
				}
			};

			// Baf should come through since we get the rate for next gateway and this rate match with rate origin and destination
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		[TestDate(2019, 12, 05)]
		public void TestAutorateShipmentCost_Export_LoginAsNonGateway_LoadIntercompanyTariffForNextGateway_FallbackToShipmentPlannedLoadAndDischarge()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("BAF");
			CreateGlobalCharge("CAF");
			CreateGlobalCharge("WAR");

			var (agent1, branch1) = CreateNonProxyBranch("CHBSL");
			var (agent2, _) = CreateCompanyAndBranchProxy("BEANR");
			var (agent3, _) = CreateCompanyAndBranchProxy("DEBRE");
			var (agent4, _) = CreateCompanyAndBranchProxy("USCHS");

			var chargeCode = Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			chargeCode.AC_GC = branch1.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;
				agent4.OH_IsCreditor = true;

				Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "US", "ORG1", 50);
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent1);
			CreateFlatRate(tariff1, "FRT", 100m, "CHBSL", "USHOU");

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			var entry2 = CreateFlatRate(tariff2, "BAF", 110m, "CHBSL", "USHOU");
			entry2.TI_PlannedLoadLRC = "CHABL";
			entry2.TI_PlannedDischargeLRC = "USCHS";

			var tariff3 = Helper.NewIntercompanyTariff(agent3);
			var entry3 = CreateFlatRate(tariff3, "CAF", 120m, "CHBSL", "US");
			entry3.TI_PlannedLoadLRC = "CH";

			var tariff4 = Helper.NewIntercompanyTariff(agent4);
			var entry4 = CreateFlatRate(tariff4, "WAR", 130m, "CHBSL", "USHOU");
			entry4.TI_PlannedLoadLRC = "CHBRN";
			entry4.TI_PlannedDischargeLRC = "USCHS";

			TransportProvider1.OH_IsCreditor = true;

			var shipment = CreateStandaloneGatewayShipment("CHBSL", "USHOU", true, agent2.PK, agent3.PK, agent4.PK);
			shipment.JS_RL_NKLoadPort = "CHBRN";
			shipment.JS_RL_NKDischargePort = "USCHS";
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ORG1"
				}
			};

			//only agent2 is a valid service provider however its plannedload and planned discharge are invalid
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		[TestDate(2019, 12, 05)]
		public void TestAutorateShipmentCost_Export_AttachedToNonGatewayConsol_LoginAsNonGateway_LoadIntercompanyTariffForNextGateway()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("CAF");
			CreateGlobalCharge("BAF");

			var (agent1, branch1) = CreateNonProxyBranch("CHBSL");
			var (agent2, _) = CreateCompanyAndBranchProxy("BEANR");
			var (agent3, _) = CreateCompanyAndBranchProxy("DEBRE");

			var chargeCode = Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			chargeCode.AC_GC = branch1.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;

				Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "US", "ORG1", 50);
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent1);
			CreateFlatRate(tariff1, "FRT", 100m, "CHBSL", "USHOU");

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "CAF", 110m, "CHBSL", "USHOU");

			var tariff3 = Helper.NewIntercompanyTariff(agent3);
			CreateFlatRate(tariff3, "BAF", 120m, "CHBSL", "USHOU");

			TransportProvider1.OH_IsCreditor = true;

			var shipment = CreateStandaloneGatewayShipment("CHBSL", "USHOU", true, agent2.PK, agent3.PK);
			var consol = CreateForwardingConsol(TransportModes.Air, "CHBSL", "USHOU", TransportProvider1, shipment);
			consol.CreditorPK = TransportProvider1.PK;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "CAF"
				},
				new AssertionCharge
				{
					ChargeCode = "ORG1"
				}
			};

			//only agent 2 is a valid service provider for intercompany tarif
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		[TestDate(2019, 12, 05)]
		public void TestAutorateShipmentCost_Import_ShouldNotLoadIntercompanyTariff()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("CAF");
			Helper.ChargeCodes.CreateGlobalCharge("BAF");

			var (agent1, branch1) = CreateCompanyAndBranchProxy("CHBSL");
			var (agent2, branch2) = CreateCompanyAndBranchProxy("BEANR");
			var (agent3, branch3) = CreateCompanyAndBranchProxy("DEBRE");
			var (agent4, branch4) = CreateCompanyAndBranchProxy("USCHS");

			var chargeCode = Helper.ChargeCodes.New("DST1", "DST charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination);
			chargeCode.AC_GC = branch4.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch4.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
				Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RateCategory.DST, RateMode.ALL, "CH", "US", "DST1", 50);
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent1);
			CreateFlatRate(tariff1, "FRT", 100m, "CHBSL", "USHOU");

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "CAF", 110m, "CHBSL", "USHOU");

			var tariff3 = Helper.NewIntercompanyTariff(agent3);
			CreateFlatRate(tariff3, "BAF", 120m, "CHBSL", "USHOU");

			TransportProvider1.OH_IsCreditor = true;

			var shipment = CreateStandaloneGatewayShipment("CHBSL", "USHOU", true, agent1.PK, agent2.PK, agent3.PK, agent4.PK);
			shipment.DocsAndCartage.DeliveryCartageCoPK = TransportProvider1.PK;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DST1"
				}
			};

			// Should NOT load intercompany tarrif. Only normal costing is applied
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch4, autorateRevenue: false);
		}

		[TestDate(2019, 12, 05)]
		public void TestAutorateShipmentCost_CrossTrade_NoGatewayAgents_LoadCostsFromCostingModule()
		{
			var (agent, branch) = CreateBranchProxy("DEBRE");

			var chargeCode = Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			chargeCode.AC_GC = branch.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "US", "ORG1", 50);
			}

			TransportProvider1.OH_IsCreditor = true;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "CHBSL", "USHOU", 1000);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ORG1"
				}
			};

			// Should load intercompany tarif for import shipment same as export
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch, autorateRevenue: false);
		}

		[TestDate(2019, 12, 05)]
		public void TestAutorateShipmentCost_CrossTrade_LoginAsGTT_ShouldNotLoadAnyCosts()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("CAF");
			Helper.ChargeCodes.CreateGlobalCharge("BAF");

			var (agent1, _) = CreateCompanyAndBranchProxy("CHBSL");
			var (agent2, _) = CreateCompanyAndBranchProxy("BEANR");
			var (agent3, branch3) = CreateCompanyAndBranchProxy("DEBRE");
			var (agent4, _) = CreateCompanyAndBranchProxy("USCHS");

			var chargeCode = Helper.ChargeCodes.New("DST1", "DST charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination);
			chargeCode.AC_GC = branch3.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch3.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
				Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RateCategory.DST, RateMode.ALL, "CH", "US", "DST1", 50);
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent1);
			CreateFlatRate(tariff1, "FRT", 100m, "CHBSL", "USHOU");

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "CAF", 110m, "CHBSL", "USHOU");

			var tariff3 = Helper.NewIntercompanyTariff(agent3);
			CreateFlatRate(tariff3, "BAF", 120m, "CHBSL", "USHOU");

			TransportProvider1.OH_IsCreditor = true;

			var consol1 = CreateForwardingConsolWithGatewayAgents("CHBSL", "BEANR", agent1, agent2, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol1.ReceivingForwarderAddress, "BEANR", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol1.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consol2 = CreateForwardingConsolWithGatewayAgents("BEANR", "DEBRE", agent2, agent3, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol2.SendingForwarderAddress, "BEANR", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(consol2.ReceivingForwarderAddress, "DEBRE", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol2.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consol3 = CreateForwardingConsolWithGatewayAgents("DEBRE", "USCHS", agent3, agent4, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol3.SendingForwarderAddress, "DEBRE", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol3.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(consol3.ReceivingForwarderAddress, "USCHS", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol3.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			TransportProvider1.OH_IsCreditor = true;

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHBSL", "USHOU", true, consol1, consol1, consol2, consol3);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;

			// Should not load any cost neither costing module not intercompany tariff since shipment is cross trade and has gateway agents
			AutorateAndAssertWithUserContextOverride(null, shipment, Consignor, branch: branch3, autorateRevenue: false);
		}

		[TestDate(2019, 12, 16)]
		public void TestAutorateShipmentCost_Export_LoginAsGTA_LoadIntercompanyTariffForNextGateway()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("CAF");
			CreateGlobalCharge("BAF");
			CreateGlobalCharge("WAR");

			var (agent1, _) = CreateCompanyAndBranchProxy("AUSYD");
			var (agent2, branch2) = CreateCompanyAndBranchProxy("CHBSL");
			var (agent3, _) = CreateCompanyAndBranchProxy("BEANR");
			var (agent4, _) = CreateCompanyAndBranchProxy("DEBRE");
			var (agent5, _) = CreateCompanyAndBranchProxy("USCHS");
			var (agent6, _) = CreateCompanyAndBranchProxy("USHOU");

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "FRT", 100m, "AUSYD", "USHOU");

			var tariff3 = Helper.NewIntercompanyTariff(agent3);
			CreateFlatRate(tariff3, "CAF", 110m, "AUSYD", "USHOU");

			var tariff4 = Helper.NewIntercompanyTariff(agent4);
			CreateFlatRate(tariff4, "BAF", 120m, "AUSYD", "USHOU");

			var tariff5 = Helper.NewIntercompanyTariff(agent5);
			CreateFlatRate(tariff5, "WAR", 120m, "AUSYD", "USHOU");

			var consol1 = CreateForwardingConsolWithGatewayAgents("AUSYD", "CHBSL", agent1, agent2, checkIsItGateway: false);
			consol1.JK_SendingForwarderHandlingType = "";

			var consol2 = CreateForwardingConsolWithGatewayAgents("CHBSL", "BEANR", agent2, agent3, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol2.ReceivingForwarderAddress, "BEANR", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol2.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consol3 = CreateForwardingConsolWithGatewayAgents("BEANR", "DEBRE", agent3, agent4, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol3.SendingForwarderAddress, "BEANR", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol3.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(consol3.ReceivingForwarderAddress, "DEBRE", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol3.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consol4 = CreateForwardingConsolWithGatewayAgents("DEBRE", "USCHS", agent4, agent5, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol4.SendingForwarderAddress, "DEBRE", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol4.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(consol4.ReceivingForwarderAddress, "USCHS", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol4.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consol5 = CreateForwardingConsolWithGatewayAgents("USCHS", "USHOU", agent5, agent6, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol5.SendingForwarderAddress, "USCHS", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol5.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol5.JK_ReceivingForwarderHandlingType = "";

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("AUSYD", "USHOU", false, consol2, consol3, consol4, consol5);

			Factory.Save();

			AssertEquals(agent2.PK, shipment.Gateways[0].ForwarderPK);
			AssertEquals(agent3.PK, shipment.Gateways[1].ForwarderPK);
			AssertEquals(agent4.PK, shipment.Gateways[2].ForwarderPK);
			AssertEquals(agent5.PK, shipment.Gateways[3].ForwarderPK);

			var expected = new[]
			{
				new AssertionCharge { ChargeCode = "CAF" },
			};

			// Login agent role is GTA, should get cost from next gateway which is agent3
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch2, autorateRevenue: false);
		}

		[TestDate(2019, 12, 16)]
		public void TestAutorateShipmentCost_Export_LoginAsGTA_CreditorIsSetToProvider()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Switzerland))
			{
				CreateGlobalCharge("FRT1");

				var (agent1, branch1) = CreateBranchProxy("CHZUR");
				var (agent2, _) = CreateBranchProxy("CHBSL");
				var (agent3, branch3) = CreateBranchProxy("BEANR");

				var chargeCode = Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
				chargeCode.AC_GC = branch3.Company.PK;

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch3.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					agent1.OH_IsCreditor = true;
					agent2.OH_IsCreditor = true;
					agent3.OH_IsCreditor = true;

					Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "US", "ORG1", 50);
				}

				var tariff = Helper.NewIntercompanyTariff(agent3);
				CreateFlatRate(tariff, "FRT1", 100m, "CHZUR", "USHOU");

				var consol2 = CreateForwardingConsolWithGatewayAgents("CHBSL", "BEANR", agent2, agent3, checkIsItGateway: false);
				SetUpAppointedGatewayAgentPorts(consol2.ReceivingForwarderAddress, "BEANR", AgentStatusList.Codes.GatewayAgentWithTariff);
				consol2.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

				TransportProvider1.OH_IsCreditor = true;

				var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "USHOU", false, consol2);
				shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;

				Factory.Save();

				AssertEquals(agent2.PK, shipment.Gateways[0].ForwarderPK);
				AssertEquals(agent3.PK, shipment.Gateways[1].ForwarderPK);

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT1",
						CostAccountCode = agent3.OH_Code
					},
					new AssertionCharge
					{
						ChargeCode = "ORG1",
						CostAccountCode = TransportProvider1.OH_Code
					}
				};

				AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
			}
		}

		[TestDate(2019, 12, 16)]
		public void TestAutorateShipmentCost_ShipmentPlannedLoadDischargeShouldBeTakenIntoConsiderationNotConsolPlannedLoadDischarge()
		{
			CreateGlobalCharge("FRT");

			var (agent1, branch1) = CreateNonProxyBranch("CHBSL");
			var (agent2, _) = CreateCompanyAndBranchProxy("BEANR");
			var (agent3, _) = CreateCompanyAndBranchProxy("DEBRE");
			var (agent4, _) = CreateCompanyAndBranchProxy("USCHS");
			var (agent5, _) = CreateCompanyAndBranchProxy("USHOU");

			var chargeCode = Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			chargeCode.AC_GC = branch1.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;
				agent4.OH_IsCreditor = true;
				agent5.OH_IsCreditor = true;

				Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "US", "ORG1", 50);
			}

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "FRT", 100m, "", "", plannedLoad: "CHBSL", plannedDischarge: "USCHS"); // current consol planned load/discharge
			CreateFlatRate(tariff2, "FRT", 200m, "", "", plannedLoad: "CHBSM", plannedDischarge: "USCHS"); // shipment planned load/discharge

			var consol1 = CreateForwardingConsolWithGatewayAgents("CHBSL", "BEANR", agent1, agent2, checkIsItGateway: false);
			consol1.JK_SendingForwarderHandlingType = "";

			var consol2 = CreateForwardingConsolWithGatewayAgents("BEANR", "DEBRE", agent2, agent3, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol2.SendingForwarderAddress, "BEANR", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(consol2.ReceivingForwarderAddress, "DEBRE", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol2.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consol3 = CreateForwardingConsolWithGatewayAgents("DEBRE", "USCHS", agent3, agent4, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol3.SendingForwarderAddress, "DEBRE", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol3.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(consol3.ReceivingForwarderAddress, "USCHS", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol3.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consol4 = CreateForwardingConsolWithGatewayAgents("USCHS", "USHOU", agent4, agent5, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol4.SendingForwarderAddress, "USCHS", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol4.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol4.JK_ReceivingForwarderHandlingType = "";

			TransportProvider1.OH_IsCreditor = true;

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHBSL", "USHOU", false, consol1, consol2, consol3, consol4);
			shipment.JS_RL_NKLoadPort = "CHBSM";
			shipment.JS_RL_NKDischargePort = "USCHS";
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 200m
				},
				new AssertionCharge
				{
					ChargeCode = "ORG1"
				}
			};

			// FRT = 200 should come through as it matches the shipment planned load and planned discharge
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		[TestDate(2019, 12, 16)]
		public void TestAutorateShipmentCost_PlannedLoadAndDischargeFallBack()
		{
			CreateGlobalCharge("FRT");

			var (agent1, branch1) = CreateNonProxyBranch("THBKK");
			var (agent2, _) = CreateCompanyAndBranchProxy("SGSIN");
			var (agent3, _) = CreateCompanyAndBranchProxy("NLRTM");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent2.OH_IsCreditor = true;
			}

			var tariff = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff, "FRT", 100m, "", "", plannedLoad: "TH", plannedDischarge: "SG");
			CreateFlatRate(tariff, "FRT", 200m, "", "", plannedLoad: "THBKK", plannedDischarge: "SGSIN");

			var consol1 = CreateForwardingConsolWithGatewayAgents("THBKK", "SGSIN", agent1, agent2, checkIsItGateway: false);
			consol1.JK_PrepaidCollect = "CCX";
			consol1.JK_SendingForwarderHandlingType = "";
			SetUpAppointedGatewayAgentPorts(consol1.SendingForwarderAddress, "SGSIN", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol1.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consol2 = CreateForwardingConsolWithGatewayAgents("SGSIN", "NLRTM", agent2, agent3, checkIsItGateway: false);
			consol2.JK_PrepaidCollect = "PPD";
			SetUpAppointedGatewayAgentPorts(consol2.SendingForwarderAddress, "SGSIN", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol2.JK_ReceivingForwarderHandlingType = "";

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("THBKK", "NLRTM", false, consol1, consol2);
			shipment.JS_INCO = "DAP";

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 200m
				}
			};

			// FRT 200 is more specific
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		[TestDate(2019, 12, 16)]
		public void TestAutorateShipmentCost_Export_LoginAsGTT_LoadIntercompanyTariffForCurrentGateway()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("CAF");
			CreateGlobalCharge("BAF");

			var (agent1, branch1) = CreateCompanyAndBranchProxy("CHZUR");
			var (agent2, branch2) = CreateBranchProxy("CHBSL", branch1.Company);
			var (agent3, _) = CreateCompanyAndBranchProxy("USCHS");
			var (agent4, _) = CreateCompanyAndBranchProxy("USHOU");

			var chargeCode = Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			chargeCode.AC_GC = branch2.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
				agent4.OH_IsCreditor = true;

				Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "US", "ORG1", 50);
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent1);
			CreateFlatRate(tariff1, "FRT", 100m, "CHZUR", "USHOU");

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "CAF", 110m, "CHZUR", "USHOU");

			var tariff3 = Helper.NewIntercompanyTariff(agent3);
			CreateFlatRate(tariff3, "BAF", 120m, "CHZUR", "USHOU");

			TransportProvider1.OH_IsCreditor = true;

			var consol1 = CreateForwardingConsolWithGatewayAgents("CHZUR", "CHBSL", agent1, agent2, checkIsItGateway: false);
			consol1.JK_SendingForwarderHandlingType = "";
			consol1.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			SetUpAppointedGatewayAgentPorts(consol1.ReceivingForwarderAddress, "CHBSL", AgentStatusList.Codes.GatewayAgentWithTariff);

			var consol2 = CreateForwardingConsolWithGatewayAgents("CHBSL", "USCHS", agent2, agent3, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol2.ReceivingForwarderAddress, "USCHS", AgentStatusList.Codes.GatewayAgentWithTariff);
			SetUpAppointedGatewayAgentPorts(consol2.SendingForwarderAddress, "CHBSL", AgentStatusList.Codes.GatewayAgentWithTariff);

			consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol2.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consol3 = CreateForwardingConsolWithGatewayAgents("USCHS", "USHOU", agent3, agent4, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol3.SendingForwarderAddress, "USCHS", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol3.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol3.JK_ReceivingForwarderHandlingType = "";

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "USHOU", false, consol1, consol2, consol3);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			Factory.Save();

			var shipmentGateWaysPKs = shipment.Gateways.Select(x => x.ForwarderPK);

			CombineAssertions("Shipment's gateways should not contain none gateway sending/receiving agent", () =>
			{
				AssertCollectionNotContains(agent1.PK, shipmentGateWaysPKs);
				AssertCollectionNotContains(agent4.PK, shipmentGateWaysPKs);
				AssertCollectionContains(agent2.PK, shipmentGateWaysPKs);
				AssertCollectionContains(agent3.PK, shipmentGateWaysPKs);
			});

			//FRT doesn't come through because Shipment's gateways dose not contain agent1 so we don't get cost from inter company tariff with agent 1 service provider
			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "CAF"
				},
				new AssertionCharge
				{
					ChargeCode = "ORG1"
				}
			};

			// Export, Login as GTT, service provider will be the current
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch2, autorateRevenue: false);
		}

		[TestDate(2019, 12, 16)]
		public void TestAutorateShipmentCost_Export_WhenSendingAgentOfFirstConsol_IsNotOrgProxyInAnyCompanyOrBranches_LoginAsGTT_LoadIntercompanyTariffForCurrentGateway()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Switzerland))
			{
				CreateGlobalCharge("FRT1");
				CreateGlobalCharge("CAF1");
				CreateGlobalCharge("BAF1");

				var agent1 = Factory.NewWithValidTestData<OrgHeader>();
				var (agent2, branch2) = CreateBranchProxy("CHBSL", GlbCompany.CurrentCompany);
				var (agent3, _) = CreateCompanyAndBranchProxy("USCHS");
				var (agent4, _) = CreateCompanyAndBranchProxy("USHOU");

				var chargeCode = Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
				chargeCode.AC_GC = branch2.Company.PK;

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					agent1.OH_IsCreditor = true;
					agent2.OH_IsCreditor = true;
					agent3.OH_IsCreditor = true;

					GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
					Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "US", "ORG1", 50);
				}

				var tariff1 = Helper.NewIntercompanyTariff(agent1);
				CreateFlatRate(tariff1, "FRT1", 100m, "CHZUR", "USHOU");

				var tariff2 = Helper.NewIntercompanyTariff(agent2);
				CreateFlatRate(tariff2, "CAF1", 110m, "CHZUR", "USHOU");

				var tariff3 = Helper.NewIntercompanyTariff(agent3);
				CreateFlatRate(tariff3, "BAF1", 120m, "CHZUR", "USHOU");

				TransportProvider1.OH_IsCreditor = true;

				var consol1 = CreateForwardingConsolWithGatewayAgents("CHZUR", "CHBSL", agent1, agent2, checkIsItGateway: false);
				consol1.JK_SendingForwarderHandlingType = "";
				consol1.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
				SetUpAppointedGatewayAgentPorts(consol1.ReceivingForwarderAddress, "CHBSL", AgentStatusList.Codes.GatewayAgentWithTariff);

				var consol2 = CreateForwardingConsolWithGatewayAgents("CHBSL", "USCHS", agent2, agent3, checkIsItGateway: false);
				SetUpAppointedGatewayAgentPorts(consol2.ReceivingForwarderAddress, "USCHS", AgentStatusList.Codes.GatewayAgentWithTariff);
				SetUpAppointedGatewayAgentPorts(consol2.SendingForwarderAddress, "CHBSL", AgentStatusList.Codes.GatewayAgentWithTariff);
				consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
				consol2.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

				var consol3 = CreateForwardingConsolWithGatewayAgents("USCHS", "USHOU", agent3, agent4, checkIsItGateway: false);
				SetUpAppointedGatewayAgentPorts(consol3.SendingForwarderAddress, "USCHS", AgentStatusList.Codes.GatewayAgentWithTariff);
				consol3.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
				consol3.JK_ReceivingForwarderHandlingType = "";

				var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "USHOU", false, consol1, consol2, consol3);
				shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;

				Factory.Save();

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "CAF1"
					},
					new AssertionCharge
					{
						ChargeCode = "ORG1"
					}
				};

				AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch2, autorateRevenue: false);
			}
		}

		[TestDate(2019, 12, 16)]
		public void TestAutorateShipmentCost_CrossTrade_LoginAsGTA_ShouldReturnFromNextGateway()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("CAF");
			CreateGlobalCharge("BAF");

			var (agent1, _) = CreateCompanyAndBranchProxy("CHZUR");
			var (agent2, _) = CreateCompanyAndBranchProxy("BEANR");
			var (agent3, branch3) = CreateCompanyAndBranchProxy("DEBRE");
			var (agent4, _) = CreateCompanyAndBranchProxy("USHOU");

			var chargeCode = Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			chargeCode.AC_GC = branch3.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch3.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
				Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "US", "ORG1", 50);
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff1, "FRT", 100m, "CHZUR", "USHOU");

			var tariff2 = Helper.NewIntercompanyTariff(agent3);
			CreateFlatRate(tariff2, "CAF", 110m, "CHZUR", "USHOU");

			var tariff3 = Helper.NewIntercompanyTariff(agent4);
			CreateFlatRate(tariff3, "BAF", 120m, "CHZUR", "USHOU");

			TransportProvider1.OH_IsCreditor = true;

			var consol1 = CreateForwardingConsolWithGatewayAgents("CHZUR", "BEANR", agent1, agent2, checkIsItGateway: false);
			consol1.JK_SendingForwarderHandlingType = "";
			consol1.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			SetUpAppointedGatewayAgentPorts(consol1.ReceivingForwarderAddress, "BEANR", AgentStatusList.Codes.GatewayAgent);

			var consol2 = CreateForwardingConsolWithGatewayAgents("BEANR", "DEBRE", agent2, agent3, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol2.SendingForwarderAddress, "BEANR", AgentStatusList.Codes.GatewayAgent);
			consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			SetUpAppointedGatewayAgentPorts(consol2.ReceivingForwarderAddress, "DEBRE", AgentStatusList.Codes.GatewayAgent);
			consol2.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var consol3 = CreateForwardingConsolWithGatewayAgents("DEBRE", "USHOU", agent3, agent4, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol3.SendingForwarderAddress, "DEBRE", AgentStatusList.Codes.GatewayAgent);
			consol3.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			SetUpAppointedGatewayAgentPorts(consol3.ReceivingForwarderAddress, "USHOU", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol3.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "USHOU", false, consol1, consol2, consol3);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSCostAmt = 120m
				},
				new AssertionCharge
				{
					ChargeCode = "ORG1"
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch3, autorateRevenue: false);
		}

		[TestDate(2019, 12, 16)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAutorateShipmentCost_Export_WhenSendingReceivingAgentUnderDifferentCompanyWithinSameCountry_ReceivingAgentUnderLoginCompany_ShouldNotLoadCosts()
		{
			#region Logic Reference
			/*
			GIVEN
				Shipment is Export (User Login Company > Country = Shipment > Origin > Country) AND
				Shipment > Consol > Sending Agent and Receiving Agent and User Login Company under same Country AND
				Shipment > Consol > Sending Agent and Receiving Agent under different Companies AND
				Shipment > Consol > Sending Agent is NOT G/W AND
				Shipment > Consol > Receiving Agent is G/W AND
				User Login Company is under the same Company of Shipment > Consol > Receiving Agent
			WHEN
				Shipment > Job Invoicing > Cost Autoration
				(multiple menu option can trigger Autoration of Costing)
			THEN
				No cost from Costing or Intercompany Tariffs expected

			Ref: PRJ00034557 to describe the Autorating behavior of Shipment engaging G/W services.
			*/
			#endregion

			var (agent1, _) = CreateCompanyAndBranchProxy("CHZUR");
			var (agent2, branch2) = CreateCompanyAndBranchProxy("CHBSL");
			var (agent3, _) = CreateCompanyAndBranchProxy("USCHS");
			var (agent4, _) = CreateCompanyAndBranchProxy("USHOU");

			var chargeCode = Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			chargeCode.AC_GC = branch2.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
				Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "US", "ORG1", 50);
			}

			TransportProvider1.OH_IsCreditor = true;

			var consol1 = CreateForwardingConsolWithGatewayAgents("CHZUR", "CHBSL", agent1, agent2, checkIsItGateway: false);
			consol1.JK_SendingForwarderHandlingType = "";
			consol1.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			SetUpAppointedGatewayAgentPorts(consol1.ReceivingForwarderAddress, "CHBSL", AgentStatusList.Codes.GatewayAgentWithTariff);

			var consol2 = CreateForwardingConsolWithGatewayAgents("CHBSL", "USCHS", agent2, agent3, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol2.ReceivingForwarderAddress, "USCHS", AgentStatusList.Codes.GatewayAgentWithTariff);
			SetUpAppointedGatewayAgentPorts(consol2.SendingForwarderAddress, "CHBSL", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol2.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consol3 = CreateForwardingConsolWithGatewayAgents("USCHS", "USHOU", agent3, agent4, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol3.SendingForwarderAddress, "USCHS", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol3.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol3.JK_ReceivingForwarderHandlingType = "";

			var shipment = CreateStandaloneGatewayShipment("CHZUR", "USHOU", false, agent2.PK, agent3.PK);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge { ChargeCode = "ORG1" } // Since there is no Consol attached to the Shipment yet
			};
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch2, autorateRevenue: false);

			shipment.Consols.AddRange(consol2, consol3);
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge { ChargeCode = "ORG1" } // Since attached Consols (consol2 & consol3) don't meet the criteria
			};
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch2, autorateRevenue: false);

			shipment.Consols.AddRange(consol1);
			Factory.Save();

			expected = Array.Empty<AssertionCharge>(); // becuase consol1 attached to the Shipment
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch2, autorateRevenue: false);
		}

		[TestDate(2019, 12, 16)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAutorateShipmentCost_Import_WhenSendingReceivingAgentUnderDifferentCompanyWithinSameCountry_SendingAgentUnderLoginCompany_ShouldNotLoadCosts()
		{
			#region Logic Reference
			/*
			GIVEN
				Shipment is Import (User Login Company > Country = Shipment > Destination > Country) AND
				Shipment > Consol > Sending Agent and Receiving Agent and User Login Company under same Country AND
				Shipment > Consol > Sending Agent and Receiving Agent under different Companies AND
				Shipment > Consol > Sending Agent is G/W AND
				Shipment > Consol > Receiving Agent is NOT G/W AND
				User Login Company is under the same Company of Shipment > Consol > Sending Agent
			WHEN
				Shipment > Job Invoicing > Cost Autoration
				(multiple menu option can trigger Autoration of Costing)
			THEN
				No cost from Costing or Intercompany Tariffs expected

			Ref: PRJ00034557 to describe the Autorating behavior of Shipment engaging G/W services.
			*/
			#endregion

			var (agent1, _) = CreateCompanyAndBranchProxy("CHZUR");
			var (agent2, _) = CreateCompanyAndBranchProxy("CHBSL");
			var (agent3, branch3) = CreateCompanyAndBranchProxy("USCHS");
			var (agent4, _) = CreateCompanyAndBranchProxy("USHOU");

			var chargeCode = Helper.ChargeCodes.New("DST1", "DST charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination);
			chargeCode.AC_GC = branch3.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch3.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
				Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RateCategory.DST, RateMode.ALL, "CH", "US", "DST1", 50);
			}

			TransportProvider1.OH_IsCreditor = true;

			var consol1 = CreateForwardingConsolWithGatewayAgents("CHZUR", "CHBSL", agent1, agent2, checkIsItGateway: false);
			consol1.JK_SendingForwarderHandlingType = "";
			consol1.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			SetUpAppointedGatewayAgentPorts(consol1.ReceivingForwarderAddress, "CHBSL", AgentStatusList.Codes.GatewayAgentWithTariff);

			var consol2 = CreateForwardingConsolWithGatewayAgents("CHBSL", "USCHS", agent2, agent3, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol2.ReceivingForwarderAddress, "USCHS", AgentStatusList.Codes.GatewayAgentWithTariff);
			SetUpAppointedGatewayAgentPorts(consol2.SendingForwarderAddress, "CHBSL", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol2.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consol3 = CreateForwardingConsolWithGatewayAgents("USCHS", "USHOU", agent3, agent4, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol3.SendingForwarderAddress, "USCHS", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol3.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol3.JK_ReceivingForwarderHandlingType = "";

			var shipment = CreateStandaloneGatewayShipment("CHZUR", "USHOU", false, agent2.PK, agent3.PK);
			shipment.JS_OH_ImportBroker = TransportProvider1.PK;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge { ChargeCode = "DST1" } // Since there is no Consol attached to the Shipment yet
			};
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch3, autorateRevenue: false);

			shipment.Consols.AddRange(consol1, consol2);
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge { ChargeCode = "DST1" } // Since attached Consols (consol1 & consol2) don't meet the criteria
			};
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch3, autorateRevenue: false);

			shipment.Consols.AddRange(consol3);
			Factory.Save();

			expected = Array.Empty<AssertionCharge>(); // becuase consol3 attached to the Shipment
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch3, autorateRevenue: false);
		}

		[TestDate(2020, 04, 01)]
		public void TestAutorateShipmentCost_Export_ShipmentHasGatewayAgent_AttachedToNonGatewayConsol_WhenServiceProviderOfCostingNotInternalOrganization_ShouldFilterNormalFRTCosts()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("CAF");
			CreateGlobalCharge("BAF");

			var (agent1, branch1) = CreateNonProxyBranch("CHBSL");
			var (agent2, _) = CreateCompanyAndBranchProxy("BEANR");
			var (agent3, _) = CreateCompanyAndBranchProxy("DEBRE");

			Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin).AC_GC = branch1.Company.PK;
			Helper.ChargeCodes.New("FRT1", "FRT charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight).AC_GC = branch1.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;

				var costing = Helper.NewCosting(TransportProvider1);
				costing.AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "US", "ORG1", 50);
				costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "CH", "US", "FRT1", 60);
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent1);
			CreateFlatRate(tariff1, "FRT", 100m, "CHBSL", "USHOU");

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "CAF", 110m, "CHBSL", "USHOU");

			var tariff3 = Helper.NewIntercompanyTariff(agent3);
			CreateFlatRate(tariff3, "BAF", 120m, "CHBSL", "USHOU");

			TransportProvider1.OH_IsCreditor = true;

			var shipment = CreateStandaloneGatewayShipment("CHBSL", "USHOU", true, agent2.PK, agent3.PK);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;

			var consol = CreateForwardingConsol(TransportModes.Air, "CHBSL", "USHOU", TransportProvider1, shipment);
			consol.CreditorPK = TransportProvider1.PK;
			Factory.Save();

			AssertEquals("Gateway Agent with Order 1", agent2.PK, shipment.Gateways[0].ForwarderPK);
			AssertEquals("Gateway Agent with Order 2", agent3.PK, shipment.Gateways[1].ForwarderPK);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "CAF"
				},
				new AssertionCharge
				{
					ChargeCode = "ORG1"
				}
			};

			//FRT1 should be filtered.
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);

			AssertAutoratingAuditLogNoteContainsLinesWithUserContextOverride(shipment, branch1,
				"Information: RateLine Filtered FRT1-FLT-Costing TRASPROV1	reason:	replaced by Intercompany tariff");
		}

		[TestDate(2020, 04, 01)]
		public void TestAutorateShipmentCost_Export_ShipmentHasGatewayAgent_AttachedToNonGatewayConsol_WhenServiceProviderOfCostingIsInternalOrganization_ShouldLoadNormalFRTCosts()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("CAF");
			CreateGlobalCharge("BAF");
			CreateGlobalCharge("FRT1");

			var (_, branch1) = CreateNonProxyBranch("CHBSL");
			var (agent2, _) = CreateCompanyAndBranchProxy("BEANR");
			var (agent3, _) = CreateCompanyAndBranchProxy("DEBRE");

			Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin).AC_GC = branch1.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;

				var costing = Helper.NewCosting(agent3);
				costing.AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "US", "ORG1", 50);
				var rateEntry = costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "CH", "US", "FRT", 60);
				var rateLine = rateEntry.AddRateLine("FRT1", FlatCalculator.Code);
				rateLine.GetCalculator<FlatCalculator>().BaseRate = 80m;
			}

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "FRT1", 100m, "CHBSL", "USHOU");
			CreateFlatRate(tariff2, "CAF", 110m, "CHBSL", "US");

			var tariff3 = Helper.NewIntercompanyTariff(agent3);
			CreateFlatRate(tariff3, "BAF", 120m, "CHBSL", "USHOU");

			var shipment = CreateStandaloneGatewayShipment("CHBSL", "USHOU", true, agent2.PK, agent3.PK);
			shipment.DocsAndCartage.PickupCartageCoPK = agent3.PK;

			var consol = CreateForwardingConsol(TransportModes.Air, "CHBSL", "USHOU", TransportProvider1, shipment);
			consol.CreditorPK = agent3.PK;
			consol.JK_OA_ShippingLineAddress = agent3.MainAddress.PK;
			Factory.Save();

			AssertEquals("Gateway Agent with Order 1", agent2.PK, shipment.Gateways[0].ForwarderPK);
			AssertEquals("Gateway Agent with Order 2", agent3.PK, shipment.Gateways[1].ForwarderPK);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT", // From costing
					JR_OSCostAmt = 60m
				},
				new AssertionCharge
				{
					ChargeCode = "CAF"
				},
				new AssertionCharge
				{
					ChargeCode = "ORG1"
				},
				new AssertionCharge
				{
					ChargeCode = "FRT1", // Priority goes to Intercompany Tariffs
					JR_OSCostAmt = 100m
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		[TestDate(2020, 04, 01)]
		public void TestAutorateShipmentCost_Export_ShipmentHasGatewayAgent_AttachedToGatewayConsol_SendingAgentIsNonGateway_WhenServiceProviderOfCostingNotInternalOrganization_ShouldFilterNormalFRTCosts()
		{
			CreateGlobalCharge("FRT");

			var (agent1, branch1) = CreateCompanyAndBranchProxy("CHZUR");
			var (agent2, _) = CreateCompanyAndBranchProxy("CHBSL");
			var (agent3, _) = CreateCompanyAndBranchProxy("BEANR");
			var (agent4, _) = CreateCompanyAndBranchProxy("DEBRE");

			Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin).AC_GC = branch1.Company.PK;
			Helper.ChargeCodes.New("FRT1", "FRT charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight).AC_GC = branch1.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent2.OH_IsCreditor = true;

				var costing = Helper.NewCosting(TransportProvider1);
				costing.AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "DE", "ORG1", 50);
				costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "CH", "DE", "FRT1", 60);
			}

			var tariff = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff, "FRT", 100m, "CHZUR", "DEBRE");

			var consol1 = CreateForwardingConsolWithGatewayAgents("CHZUR", "CHBSL", agent1, agent2, checkIsItGateway: false);
			consol1.CreditorPK = TransportProvider1.PK;
			consol1.JK_SendingForwarderHandlingType = "";

			var consol2 = CreateForwardingConsolWithGatewayAgents("CHBSL", "BEANR", agent2, agent3, checkIsItGateway: false);

			var consol3 = CreateForwardingConsolWithGatewayAgents("BEANR", "DEBRE", agent3, agent4, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol3.SendingForwarderAddress, "BEANR", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol3.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			TransportProvider1.OH_IsCreditor = true;

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "DEBRE", false, consol1, consol2, consol3);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;

			Factory.Save();

			AssertEquals("Gateway Agent with Order 1", agent2.PK, shipment.Gateways[0].ForwarderPK);
			AssertEquals("Gateway Agent with Order 2", agent3.PK, shipment.Gateways[1].ForwarderPK);
			AssertEquals("Gateway Agent with Order 3", agent4.PK, shipment.Gateways[2].ForwarderPK);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 100m
				},
				new AssertionCharge
				{
					ChargeCode = "ORG1"
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);

			AssertAutoratingAuditLogNoteContainsLinesWithUserContextOverride(shipment, branch1,
				"Information: RateLine Filtered FRT1-FLT-Costing TRASPROV1	reason:	replaced by Intercompany tariff");
		}

		[TestDate(2020, 04, 01)]
		public void TestAutorateShipmentCost_Export_ShipmentHasGatewayAgent_AttachedToGatewayConsol_SendingAgentIsNonGateway_WhenServiceProviderOfCostingIsInternalOrganization_ShouldLoadNormalFRTCosts()
		{
			CreateGlobalCharge("FRT");

			var (agent1, branch1) = CreateCompanyAndBranchProxy("CHZUR");
			var (agent2, _) = CreateCompanyAndBranchProxy("CHBSL");
			var (agent3, _) = CreateCompanyAndBranchProxy("BEANR");
			var (agent4, _) = CreateCompanyAndBranchProxy("DEBRE");

			Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin).AC_GC = branch1.Company.PK;
			Helper.ChargeCodes.New("FRT1", "FRT charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight).AC_GC = branch1.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent2.OH_IsCreditor = true;

				var costing = Helper.NewCosting(agent2);
				costing.AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "DE", "ORG1", 50);
				costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "CH", "DE", "FRT1", 60);
			}

			var tariff = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff, "FRT", 100m, "CHZUR", "DEBRE");

			var consol1 = CreateForwardingConsolWithGatewayAgents("CHZUR", "CHBSL", agent1, agent2, checkIsItGateway: false);
			consol1.CreditorPK = agent2.PK;
			consol1.JK_OA_ShippingLineAddress = agent2.MainAddress.PK;
			consol1.JK_SendingForwarderHandlingType = "";

			var consol2 = CreateForwardingConsolWithGatewayAgents("CHBSL", "BEANR", agent2, agent3, checkIsItGateway: false);

			var consol3 = CreateForwardingConsolWithGatewayAgents("BEANR", "DEBRE", agent3, agent4, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol3.SendingForwarderAddress, "BEANR", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol3.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "DEBRE", false, consol1, consol2, consol3);
			shipment.DocsAndCartage.PickupCartageCoPK = agent2.PK;

			Factory.Save();

			AssertEquals("Gateway Agent with Order 1", agent2.PK, shipment.Gateways[0].ForwarderPK);
			AssertEquals("Gateway Agent with Order 2", agent3.PK, shipment.Gateways[1].ForwarderPK);
			AssertEquals("Gateway Agent with Order 3", agent4.PK, shipment.Gateways[2].ForwarderPK);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 100m
				},
				new AssertionCharge
				{
					ChargeCode = "ORG1"
				},
				new AssertionCharge
				{
					ChargeCode = "FRT1"
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		[TestDate(2020, 04, 01)]
		public void TestAutorateShipmentCost_Export_ShipmentHasGatewayAgent_AttachedToGatewayConsol_ReceivingAgentIsNonGateway_WhenServiceProviderOfCostingNotInternalOrganization_ShouldFilterNormalFRTCosts()
		{
			CreateGlobalCharge("FRT");

			var (agent1, branch1) = CreateCompanyAndBranchProxy("CHZUR");
			var (agent2, _) = CreateCompanyAndBranchProxy("CHBSL");
			var (agent3, _) = CreateCompanyAndBranchProxy("BEANR");
			var (agent4, _) = CreateCompanyAndBranchProxy("DEBRE");

			Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin).AC_GC = branch1.Company.PK;
			Helper.ChargeCodes.New("FRT1", "FRT charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight).AC_GC = branch1.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent2.OH_IsCreditor = true;

				var costing = Helper.NewCosting(TransportProvider1);
				costing.AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "DE", "ORG1", 50);
				costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "CH", "DE", "FRT1", 60);
			}

			var tariff = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff, "FRT", 100m, "CHZUR", "DEBRE");

			var consol1 = CreateForwardingConsolWithGatewayAgents("CHZUR", "CHBSL", agent1, agent2, checkIsItGateway: false);
			consol1.CreditorPK = TransportProvider1.PK;
			consol1.JK_SendingForwarderHandlingType = "";

			var consol2 = CreateForwardingConsolWithGatewayAgents("CHBSL", "BEANR", agent2, agent3, checkIsItGateway: false);

			var consol3 = CreateForwardingConsolWithGatewayAgents("BEANR", "DEBRE", agent3, agent4, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol3.ReceivingForwarderAddress, "DEBRE", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol3.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			TransportProvider1.OH_IsCreditor = true;

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "DEBRE", false, consol1, consol2, consol3);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;

			Factory.Save();

			AssertEquals("Gateway Agent with Order 1", agent2.PK, shipment.Gateways[0].ForwarderPK);
			AssertEquals("Gateway Agent with Order 2", agent3.PK, shipment.Gateways[1].ForwarderPK);
			AssertEquals("Gateway Agent with Order 3", agent4.PK, shipment.Gateways[2].ForwarderPK);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 100m
				},
				new AssertionCharge
				{
					ChargeCode = "ORG1"
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);

			AssertAutoratingAuditLogNoteContainsLinesWithUserContextOverride(shipment, branch1,
				"Information: RateLine Filtered FRT1-FLT-Costing TRASPROV1	reason:	replaced by Intercompany tariff");
		}

		[TestDate(2020, 04, 01)]
		public void TestAutorateShipmentCost_Export_ShipmentHasGatewayAgent_AttachedToGatewayConsol_ReceivingAgentIsGTT_WhenServiceProviderOfCostingIsInternalOrganization_ShouldLoadNormalFRTCosts()
		{
			CreateGlobalCharge("FRT");

			var (agent1, branch1) = CreateCompanyAndBranchProxy("CHZUR");
			var (agent2, _) = CreateCompanyAndBranchProxy("CHBSL");
			var (agent3, _) = CreateCompanyAndBranchProxy("BEANR");
			var (agent4, _) = CreateCompanyAndBranchProxy("DEBRE");

			Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin).AC_GC = branch1.Company.PK;
			Helper.ChargeCodes.New("FRT1", "FRT charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight).AC_GC = branch1.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent2.OH_IsCreditor = true;

				var costing = Helper.NewCosting(agent2);
				costing.AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "DE", "ORG1", 50);
				costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "CH", "DE", "FRT1", 60);
			}

			var tariff = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff, "FRT", 100m, "CHZUR", "DEBRE");

			var consol1 = CreateForwardingConsolWithGatewayAgents("CHZUR", "CHBSL", agent1, agent2, checkIsItGateway: false);
			consol1.CreditorPK = agent2.PK;
			consol1.JK_OA_ShippingLineAddress = agent2.MainAddress.PK;
			consol1.JK_SendingForwarderHandlingType = "";

			var consol2 = CreateForwardingConsolWithGatewayAgents("CHBSL", "BEANR", agent2, agent3, checkIsItGateway: false);

			var consol3 = CreateForwardingConsolWithGatewayAgents("BEANR", "DEBRE", agent3, agent4, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol3.ReceivingForwarderAddress, "DEBRE", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol3.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "DEBRE", false, consol1, consol2, consol3);
			shipment.DocsAndCartage.PickupCartageCoPK = agent2.PK;

			Factory.Save();

			AssertEquals("Gateway Agent with Order 1", agent2.PK, shipment.Gateways[0].ForwarderPK);
			AssertEquals("Gateway Agent with Order 2", agent3.PK, shipment.Gateways[1].ForwarderPK);
			AssertEquals("Gateway Agent with Order 3", agent4.PK, shipment.Gateways[2].ForwarderPK);

			var expected = new[]
			{
				new AssertionCharge { ChargeCode = "ORG1" },
				new AssertionCharge { ChargeCode = "FRT1" },
				new AssertionCharge { ChargeCode = "FRT" },
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		[TestDate(2020, 04, 01)]
		public void TestAutorateShipmentCost_Export_ShipmentGatewayAgentsInfoIsManuallyRemoved_AttachedToGatewayConsol_SendingAgentIsGTT_WhenServiceProviderOfCostingIsInternalOrganization_ShouldLoadNormalFRTCosts()
		{
			CreateGlobalCharge("FRT");

			var (agent1, branch1) = CreateCompanyAndBranchProxy("CHZUR");
			var (agent2, _) = CreateCompanyAndBranchProxy("CHBSL");
			var (agent3, _) = CreateCompanyAndBranchProxy("BEANR");
			var (agent4, _) = CreateCompanyAndBranchProxy("DEBRE");

			Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin).AC_GC = branch1.Company.PK;
			Helper.ChargeCodes.New("FRT1", "FRT charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight).AC_GC = branch1.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent2.OH_IsCreditor = true;

				var costing = Helper.NewCosting(agent2);
				costing.AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "DE", "ORG1", 50);
				costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "CH", "DE", "FRT1", 60);
			}

			var tariff = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff, "FRT", 100m, "CHZUR", "DEBRE");

			var consol1 = CreateForwardingConsolWithGatewayAgents("CHZUR", "CHBSL", agent1, agent2, checkIsItGateway: false);
			consol1.CreditorPK = agent2.PK;
			consol1.JK_OA_ShippingLineAddress = agent2.MainAddress.PK;
			consol1.JK_SendingForwarderHandlingType = "";

			var consol2 = CreateForwardingConsolWithGatewayAgents("CHBSL", "BEANR", agent2, agent3, checkIsItGateway: false);

			var consol3 = CreateForwardingConsolWithGatewayAgents("BEANR", "DEBRE", agent3, agent4, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol3.SendingForwarderAddress, "BEANR", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol3.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "DEBRE", false, consol1, consol2, consol3);
			shipment.DocsAndCartage.PickupCartageCoPK = agent2.PK;

			Factory.Save();

			AssertEquals("Gateway Agent with Order 1", agent2.PK, shipment.Gateways[0].ForwarderPK);
			AssertEquals("Gateway Agent with Order 2", agent3.PK, shipment.Gateways[1].ForwarderPK);
			AssertEquals("Gateway Agent with Order 3", agent4.PK, shipment.Gateways[2].ForwarderPK);

			shipment.Gateways.DeleteAll();
			Factory.Save();

			AssertEquals("", false, shipment.Gateways.Count > 0);
			AssertEquals("Shipment should still have 3 consols attached", 3, shipment.Consols.Count);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ORG1"
				},
				new AssertionCharge
				{
					ChargeCode = "FRT1"
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		[TestDate(2020, 04, 01)]
		public void TestAutorateShipmentCost_Export_ShipmentGatewayAgentsInfoIsManuallyRemoved_AttachedToGatewayConsol_ReceivingAgentIsGTT_WhenServiceProviderOfCostingIsInternalOrganization_ShouldLoadNormalFRTCosts()
		{
			CreateGlobalCharge("FRT");

			var (agent1, branch1) = CreateCompanyAndBranchProxy("CHZUR");
			var (agent2, _) = CreateCompanyAndBranchProxy("CHBSL");
			var (agent3, _) = CreateCompanyAndBranchProxy("BEANR");
			var (agent4, _) = CreateCompanyAndBranchProxy("DEBRE");

			Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin).AC_GC = branch1.Company.PK;
			Helper.ChargeCodes.New("FRT1", "FRT charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Freight).AC_GC = branch1.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent2.OH_IsCreditor = true;

				var costing = Helper.NewCosting(agent2);
				costing.AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "DE", "ORG1", 50);
				costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "CH", "DE", "FRT1", 60);
			}

			var tariff = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff, "FRT", 100m, "CHZUR", "DEBRE");

			var consol1 = CreateForwardingConsolWithGatewayAgents("CHZUR", "CHBSL", agent1, agent2, checkIsItGateway: false);
			consol1.CreditorPK = agent2.PK;
			consol1.JK_OA_ShippingLineAddress = agent2.MainAddress.PK;
			consol1.JK_SendingForwarderHandlingType = "";

			var consol2 = CreateForwardingConsolWithGatewayAgents("CHBSL", "BEANR", agent2, agent3, checkIsItGateway: false);

			var consol3 = CreateForwardingConsolWithGatewayAgents("BEANR", "DEBRE", agent3, agent4, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol3.ReceivingForwarderAddress, "DEBRE", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol3.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "DEBRE", false, consol1, consol2, consol3);
			shipment.DocsAndCartage.PickupCartageCoPK = agent2.PK;

			Factory.Save();

			AssertEquals("Gateway Agent with Order 1", agent2.PK, shipment.Gateways[0].ForwarderPK);
			AssertEquals("Gateway Agent with Order 2", agent3.PK, shipment.Gateways[1].ForwarderPK);
			AssertEquals("Gateway Agent with Order 3", agent4.PK, shipment.Gateways[2].ForwarderPK);

			shipment.Gateways.DeleteAll();
			Factory.Save();

			AssertEquals("", false, shipment.Gateways.Count > 0);
			AssertEquals("Shipment should still have 3 consols attached", 3, shipment.Consols.Count);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ORG1"
				},
				new AssertionCharge
				{
					ChargeCode = "FRT1"
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		[TestDate(2020, 04, 01)]
		public void TestAutorateShipmentCost_WhenConsolDischargeIsInValid_ShouldIgnoreLocation()
		{
			CreateGlobalCharge("BAF");
			CreateGlobalCharge("CAF");

			var (agent1, branch1) = CreateCompanyAndBranchProxy("CHZUR");
			var (agent2, _) = CreateCompanyAndBranchProxy("DEHAM");
			var (agent3, _) = CreateCompanyAndBranchProxy("BEANR");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff1, "BAF", 100m, "CHZUR", "BEANR", "", "DEHAM");

			var tariff3 = Helper.NewIntercompanyTariff(agent3);
			CreateFlatRate(tariff3, "CAF", 100m, "CHZUR", "BEANR", "", "BEANR");

			// CHZUR - DEHAM - BEANR
			var consol1 = CreateForwardingConsolWithGatewayAgents("CHZUR", "DEHAM", agent1, agent2, checkIsItGateway: false);
			var consol2 = CreateForwardingConsolWithGatewayAgents("DEHAM", "BEANR", agent2, agent3, checkIsItGateway: false);

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "BEANR", false, consol1, consol2);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
				}
			};
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);

			consol1.JK_RL_NKDischargePort = "?????";
			AutorateAndAssertWithUserContextOverride(null, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		[TestDate(2020, 04, 01)]
		public void TestAutorateShipmentCost_Export_LoginAsNonGateway_WhenConsolAddressRelatedPortIsInValid_ShouldIgnoreLocation()
		{
			CreateGlobalCharge("BAF");
			CreateGlobalCharge("CAF");

			var (agent1, branch1) = CreateCompanyAndBranchProxy("CHZUR");
			var (agent2, _) = CreateCompanyAndBranchProxy("DEHAM");
			var (agent3, _) = CreateCompanyAndBranchProxy("BEANR");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;
			}

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "BAF", 100m, "CHZUR", "BEANR", "HKHKG");

			var tariff3 = Helper.NewIntercompanyTariff(agent3);
			CreateFlatRate(tariff3, "CAF", 100m, "CHZUR", "BEANR", "CNSHA");

			// CHZUR - DEHAM - BEANR
			var consol1 = CreateForwardingConsolWithGatewayAgents("CHZUR", "DEHAM", agent1, agent2, checkIsItGateway: false);
			consol1.ReceivingForwarderAddress.OA_RL_NKRelatedPortCode = "HKHKG";

			var consol2 = CreateForwardingConsolWithGatewayAgents("DEHAM", "BEANR", agent2, agent3, checkIsItGateway: false);
			consol2.ReceivingForwarderAddress.OA_RL_NKRelatedPortCode = "CNSHA";

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "BEANR", false, consol1, consol2);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
				}
			};
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);

			consol1.ReceivingForwarderAddress.OA_RL_NKRelatedPortCode = "?????";
			AutorateAndAssertWithUserContextOverride(null, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		[TestDate(2019, 12, 05)]
		public void TestAutorateShipmentCost_Export_LoginAsNonGateway_ConflictingCharges_IntercompanyTariffTakesPriorityOverGlobalLocalCosting()
		{
			CreateGlobalCharge("ORG", FlatCalculator.Code, "ORG");
			CreateGlobalCharge("CAF");
			CreateGlobalCharge("BAF");

			var (agent1, branch1) = CreateNonProxyBranch("CHBSL");
			var (agent2, _) = CreateCompanyAndBranchProxy("BEANR");
			var (agent3, _) = CreateCompanyAndBranchProxy("DEBRE");
			var (agent4, _) = CreateCompanyAndBranchProxy("USCHS");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;

				Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(TransportModes.Air, ContainerModes.Loose, "CH", "US", "ORG", 50m);
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent1);
			CreateFlatRate(tariff1, "CAF", 100m, "CHBSL", "USHOU");

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "ORG", 110m, "CHBSL", "USHOU");

			var tariff3 = Helper.NewIntercompanyTariff(agent3);
			CreateFlatRate(tariff3, "BAF", 120m, "CHBSL", "USHOU");

			TransportProvider1.OH_IsCreditor = true;

			var shipment = CreateStandaloneGatewayShipment("CHBSL", "USHOU", true, agent2.PK, agent3.PK, agent4.PK);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ORG",
					CostAccountCode = agent2.OH_Code
				}
			};

			var expectedLogNotes = new[]
			{
				"Information: RateLine Found ORG-FLT-Intercompany Tariff PROXYBEANR",
				"Information: RateLine Found ORG-FLT-Costing TRASPROV1",
				"Information: RateLine Filtered ORG-FLT-Costing TRASPROV1	reason:	overridden by ORG-FLT-Intercompany Tariff PROXYBEANR due to Local Rates overriding Global Rates",
			};

			var notExpectedLogNote = "Information: RateLine Filtered ORG-FLT-Intercompany Tariff PROXYBEANR	reason:	overridden by ORG-FLT-Costing TRASPROV1 by Creditors comparer";

			AutorateAndAssertWithUserContextOverride(expectedCharges, shipment, Consignor, branch: branch1, autorateRevenue: false);
			AssertAutoratingAuditLogNoteContainsLinesWithUserContextOverride(shipment, branch1, expectedLogNotes);
			AssertAutoratingAuditLogNoteNOTContainsLinesWithUserContextOverride(shipment, branch1, notExpectedLogNote);
		}

		[TestDate(2019, 12, 05)]
		public void TestAutorateShipmentCost_Export_LoginAsNonGateway_ControllingCustomer_MatchesNeitherControllingCustomerNorLocalClientOnShipment_FallbackToPlainRate()
		{
			CreateGlobalCharge("FRT");
			var anyCustomer = Helper.NewOrgHeader("ANY_CLIENT_2");
			var controllingCustomer = Helper.NewOrgHeader("CONT_CLIENT");

			var (_, branch1) = CreateCompanyAndBranchProxy("CHBSL");
			var (receivingAgent, _) = CreateCompanyAndBranchProxy("BEANR");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				receivingAgent.OH_IsCreditor = true;
			}

			var tariff = Helper.NewIntercompanyTariff(receivingAgent);
			CreateFlatRate(tariff, "FRT", 100m, "CHBSL", "USHOU").TI_OH_ControllingCustomer = controllingCustomer.PK;
			CreateFlatRate(tariff, "FRT", 200m, "CHBSL", "USHOU");

			Factory.Save();

			var shipment = CreateStandaloneGatewayShipment("CHBSL", "USHOU", true, receivingAgent.PK);
			shipment.ControllingCustomerNameOrPK = anyCustomer.PK.ToString();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 200m
				},
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 12, 05)]
		public void TestAutorateShipmentCost_Export_LoginAsNonGateway_ControllingCustomer_MatchesShipmentControllingCustomer()
		{
			CreateGlobalCharge("FRT");
			var controllingCustomer = Helper.NewOrgHeader("CONT_CLIENT");

			var (_, branch1) = CreateNonProxyBranch("CHBSL");
			var (receivingAgent, _) = CreateCompanyAndBranchProxy("BEANR");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				receivingAgent.OH_IsCreditor = true;
			}

			var tariff = Helper.NewIntercompanyTariff(receivingAgent);
			CreateFlatRate(tariff, "FRT", 100m, "CHBSL", "USHOU").TI_OH_ControllingCustomer = controllingCustomer.PK;
			CreateFlatRate(tariff, "FRT", 200m, "CHBSL", "USHOU");

			Factory.Save();

			var shipment = CreateStandaloneGatewayShipment("CHBSL", "USHOU", true, receivingAgent.PK);
			shipment.ControllingCustomerNameOrPK = controllingCustomer.PK.ToString();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 100m
				},
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2019, 12, 05)]
		public void TestAutorateShipmentCost_Export_LoginAsNonGateway_ControllingCustomer_MatchesShipmentControllingCustomer_AsPriority()
		{
			CreateGlobalCharge("FRT");
			var localClient = Helper.NewOrgHeader("ABCD");
			var controllingCustomer = Helper.NewOrgHeader("CONT_CLIENT");

			var (_, branch1) = CreateNonProxyBranch("CHBSL");
			var (receivingAgent, _) = CreateCompanyAndBranchProxy("BEANR");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				receivingAgent.OH_IsCreditor = true;
			}

			var tariff = Helper.NewIntercompanyTariff(receivingAgent);
			CreateFlatRate(tariff, "FRT", 100m, "CHBSL", "USHOU").TI_OH_ControllingCustomer = controllingCustomer.PK;
			CreateFlatRate(tariff, "FRT", 200m, "CHBSL", "USHOU").TI_OH_ControllingCustomer = localClient.PK;
			CreateFlatRate(tariff, "FRT", 300m, "CHBSL", "USHOU");

			Factory.Save();

			var shipment = CreateStandaloneGatewayShipment("CHBSL", "USHOU", true, receivingAgent.PK);
			shipment.ControllingCustomerNameOrPK = controllingCustomer.PK.ToString();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 100m
				},
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, localClient, branch: branch1, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2020, 07, 03)]
		public void TestAutorateShipmentCost_Export_WhenSendingAgentAndRecievingAgentOfConsolAreGTT_ShouldReturnSendingAgentCost()
		{
			CreateGlobalCharge("FRT1");
			var (sendingAgent, branch1) = CreateCompanyAndBranchProxy("AUSYD");
			var (receivingAgent, _) = CreateCompanyAndBranchProxy("NZAKL");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				sendingAgent.OH_IsCreditor = true;
				receivingAgent.OH_IsCreditor = true;
			}

			var sendingAgentIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(sendingAgentIntercompanyTariff, "FRT1", 100m, "AUMEL", "NZCHC");

			var recievingAgentIntecompanyTariff = Helper.NewIntercompanyTariff(receivingAgent);
			CreateFlatRate(recievingAgentIntecompanyTariff, "FRT1", 110m, "AUMEL", "NZCHC");

			var consol = CreateForwardingConsolWithGatewayAgents("AUSYD", "NZAKL", sendingAgent, receivingAgent, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "AUSYD", AgentStatusList.Codes.GatewayAgentWithTariff);
			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, "NZAKL", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("AUMEL", "NZCHC", false, consol);
			Factory.Save();

			AssertEquals("Gateway Agent with Order 1", sendingAgent.PK, shipment.Gateways[0].ForwarderPK);
			AssertEquals("Gateway Agent with Order 2", receivingAgent.PK, shipment.Gateways[1].ForwarderPK);

			var expected = new[]
			{
					new AssertionCharge
					{
						ChargeCode = "FRT1",
						JR_OSSellAmt = 100m
					},
			};
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		GatewayChargeDefaultDebtorConfigurationCollection GatewayChargeDefaultDebtorConfigurationForExportSetup()
		{
			var configuration = new GatewayChargeDefaultDebtorConfiguration()
			{
				ChargeGroup = "ALL",
				ConsolPaymentTerm = PaymentTerms.Prepaid,
				RelatedJob = GatewayRelatedJob.Codes.RelatedToShipment,
				PreviousSendingAgent = "ALL",
				Debtor = GatewayDebtor.Codes.ShipmentPickupAgent,
				ConsolDirection = "ALL",
				ConsolTransportMode = "ALL"
			};

			var configuration2 = new GatewayChargeDefaultDebtorConfiguration()
			{
				ChargeGroup = "DST",
				ConsolPaymentTerm = "ALL",
				RelatedJob = GatewayRelatedJob.Codes.RelatedToShipment,
				PreviousSendingAgent = "ALL",
				Debtor = GatewayDebtor.Codes.ShipmentDeliveryAgent,
				ConsolDirection = "ALL",
				ConsolTransportMode = "ALL"
			};

			var collection = new GatewayChargeDefaultDebtorConfigurationCollection();
			collection.AddRange(configuration, configuration2);

			return collection;
		}

		GatewayChargeDefaultInvoiceTargetJobConfigurationCollection GatewayChargeDefaultInvoiceTargetJobConfigurationForExportSetup()
		{
			var configuration1 = new GatewayChargeDefaultInvoiceTargetJobConfiguration()
			{
				ConsolDirection = "EXP",
				ConsolTransportMode = "AIR",
				PreviousSendingAgentType = "GTA",
				InvoiceTargetJobType = "REL"
			};

			var configuration2 = new GatewayChargeDefaultInvoiceTargetJobConfiguration()
			{
				ConsolDirection = "EXP",
				ConsolTransportMode = "AIR",
				PreviousSendingAgentType = "GTT",
				InvoiceTargetJobType = "REL"
			};

			var collection = new GatewayChargeDefaultInvoiceTargetJobConfigurationCollection();
			collection.AddRange(configuration1, configuration2);

			return collection;
		}

		public void TestAutorateShipmentCost_Export_WhenSendingAgent_OnlyRateWhenConsolPaymentTermIsPrepaidOrBlank()
		{
			CreateGlobalCharge("DST1", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination);
			CreateGlobalCharge("DST2", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination);
			var (sendingAgent, branch) = CreateBranchProxy("AUSYD");
			var receivingAgent = CreateNonProxyBranch("NZAKL").org;
			var localClient = Helper.NewOrgHeader();

			var sendingAgentIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(sendingAgentIntercompanyTariff, "DST1", 100m, "", "NZCHC", gatewayAgentType: "SAG");
			CreateFlatRate(sendingAgentIntercompanyTariff, "DST2", 110m, "", "NZCHC", gatewayAgentType: "RAG");

			var consol = CreateForwardingConsolWithGatewayAgents("AUSYD", "NZAKL", sendingAgent, receivingAgent);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol.JK_ReceivingForwarderHandlingType = string.Empty;
			Factory.Save();

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("AUSYD", "NZCHC", true, consol);
			shipment.JS_INCO = IncoTerms.FreeOnBoard;
			shipment.PickupAgentPK = sendingAgent.PK;
			shipment.JS_OH_DeliveryAgent = receivingAgent.PK;
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultDebtorConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GatewayChargeDefaultDebtorConfigurationForExportSetup()))
			using (AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultInvoiceTargetJobConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GatewayChargeDefaultInvoiceTargetJobConfigurationForExportSetup()))
			{
				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "DST1",
						JR_OSSellAmt = 100m
					}
				};
				consol.JK_PrepaidCollect = PaymentType.Prepaid;
				AutorateAndAssertWithUserContextOverride(expected, shipment, localClient, branch: branch, autorateRevenue: false);
				AssertAutoratingAuditLogNoteContainsLinesWithUserContextOverride(shipment, branch,
					"Information: RateEntry Filtered Intercompany Tariff PROXYAUSYD reason: Gateway agent type RAG cannot be used for shipment autorating because of sending agent of attached Consol C00001000 with following payment PPD.");

				consol.JK_PrepaidCollect = string.Empty;
				AutorateAndAssertWithUserContextOverride(expected, shipment, localClient, branch: branch, autorateRevenue: false);
				AssertAutoratingAuditLogNoteContainsLinesWithUserContextOverride(shipment, branch,
					"Information: RateEntry Filtered Intercompany Tariff PROXYAUSYD reason: Gateway agent type RAG cannot be used for shipment autorating because of sending agent of attached Consol C00001000 with following payment .");

				consol.JK_PrepaidCollect = PaymentType.Collect;
				AutorateAndAssertWithUserContextOverride(expected: null, shipment, localClient, branch: branch, autorateRevenue: false);
				AssertAutoratingAuditLogNoteContainsLinesWithUserContextOverride(shipment, branch,
					"Information: RateEntry Filtered Intercompany Tariff PROXYAUSYD reason: Gateway agent type SAG cannot be used for shipment autorating because of sending agent of attached Consol C00001000 with following payment CCX.",
					"Information: RateEntry Filtered Intercompany Tariff PROXYAUSYD reason: Gateway agent type RAG cannot be used for shipment autorating because of sending agent of attached Consol C00001000 with following payment CCX.");
			}
		}

		public void TestAutorateShipmentCost_Export_WhenReceivingAgent_OnlyRateWhenConsolPaymentTermIsCollect()
		{
			CreateGlobalCharge("DST1", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination);
			CreateGlobalCharge("DST2", UnitCalculator.Code, ChargeCodeGroupList.Codes.Destination);
			var (agent1, branch1) = CreateNonProxyBranch("AUSYD");
			var agent2 = CreateBranchProxy("HKHKG").proxy;
			var (agent3, _) = CreateNonProxyBranch("USLAX");
			var localClient = Helper.NewOrgHeader();

			var receivingAgentIntercompanyTariff = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(receivingAgentIntercompanyTariff, "DST1", 100m, "", "USLAX", gatewayAgentType: "SAG");
			CreateFlatRate(receivingAgentIntercompanyTariff, "DST2", 110m, "", "USLAX", gatewayAgentType: "RAG");

			var consol1 = CreateForwardingConsolWithGatewayAgents("AUSYD", "HKHKG", agent1, agent2);
			consol1.JK_SendingForwarderHandlingType = string.Empty;
			consol1.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consol2 = CreateForwardingConsolWithGatewayAgents("HKHKG", "USLAX", agent2, agent3);
			consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol2.JK_ReceivingForwarderHandlingType = string.Empty;

			Factory.Save();

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("AUSYD", "USLAX", true, consol1, consol2);
			shipment.JS_INCO = IncoTerms.FreeOnBoard;
			shipment.PickupAgentPK = agent1.PK;
			shipment.JS_OH_DeliveryAgent = agent2.PK;
			Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultDebtorConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GatewayChargeDefaultDebtorConfigurationForExportSetup()))
			using (AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultInvoiceTargetJobConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GatewayChargeDefaultInvoiceTargetJobConfigurationForExportSetup()))
			{
				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "DST2",
						JR_OSSellAmt = 110m
					}
				};

				consol1.JK_PrepaidCollect = PaymentType.Collect;
				consol2.JK_PrepaidCollect = PaymentType.Collect;
				AutorateAndAssertWithUserContextOverride(expected, shipment, localClient, branch: branch1, autorateRevenue: false);

				expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "DST1",
						JR_OSSellAmt = 100m
					}
				};

				consol1.JK_PrepaidCollect = string.Empty;
				consol2.JK_PrepaidCollect = string.Empty;
				// DST 1 can be loaded for second consol
				AutorateAndAssertWithUserContextOverride(expected, shipment, localClient, branch: branch1, autorateRevenue: false);
				AssertAutoratingAuditLogNoteContainsLinesWithUserContextOverride(shipment, branch1,
					"Information: RateEntry Filtered Intercompany Tariff PROXYHKHKG reason: Gateway agent type RAG cannot be used for shipment autorating because of sending agent of attached Consol C00001001 with following payment .");

				consol1.JK_PrepaidCollect = PaymentType.Prepaid;
				consol2.JK_PrepaidCollect = PaymentType.Prepaid;
				// DST 1 can be loaded for second consol
				AutorateAndAssertWithUserContextOverride(expected, shipment, localClient, branch: branch1, autorateRevenue: false);
				AssertAutoratingAuditLogNoteContainsLinesWithUserContextOverride(shipment, branch1,
					"Information: RateEntry Filtered Intercompany Tariff PROXYHKHKG reason: Gateway agent type RAG cannot be used for shipment autorating because of sending agent of attached Consol C00001001 with following payment PPD.");
			}
		}

		public void TestAutoRateShipmentCost_WhenAJRJIsEnabled_ConsolRevenueChargeIsPosted_ShipmentChargeShouldNotBeDuplicated()
		{
			var charge = CreateGlobalCharge("FRT1");
			charge.AC_MarginPercentage = 0m;

			var sendingAgent = CreateBranchProxy("USCHS");
			var receivingAgent = CreateNonProxyBranch("AUSYD");
			sendingAgent.proxy.OH_IsDebtor = true;
			sendingAgent.proxy.OH_IsCreditor = true;

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent.proxy);
			CreateFlatRate(intercompanyTariff, "FRT1", 66m, "USCHS", "AUSYD");

			var consol = CreateForwardingConsolWithGatewayAgents("USCHS", "AUSYD", sendingAgent: sendingAgent.proxy, receivingAgent.org);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "USCHS", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, "USCHS", "AUSYD", false);

			Factory.Save();

			var consolJob = new Job.Loader(consol).TryLoadOrCreateWithoutMutexForTestOnly();
			consolJob.JH_GB = sendingAgent.branch.PK;
			consolJob.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FES")).PK;

			var expected = new[]
			{
					new AssertionCharge
					{
						ChargeCode = "FRT1",
						JR_OSSellAmt = 66m,
						RelatedJobNumber = shipment.JS_UniqueConsignRef,
					}
			};

			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(Env.CurrentCompanyPK))
			{
				AutorateAndAssertWithUserContextOverride(expected, consol, consol.SendingForwarder, null, sendingAgent.branch, autorateRevenue: true, autorateCosts: false, job: consolJob);

				var revenueCharge = consolJob.Charges[0];
				revenueCharge.JR_OH_SellAccount = sendingAgent.proxy.PK;

				Factory.Save();

				AssertEquals("Revenue charge is posted", true, revenueCharge.IsRevenuePosted);

				var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate();
				var costCharge = shipmentJob.Charges[0];

				AssertEquals("cost side is posted", true, costCharge.IsCostPosted);
				AssertEquals("Rating Behaviour is STP", JobChargeLookups.StopFromAutorating, costCharge.JR_Calc_CostRatingBehavior);
				AssertEquals(revenueCharge.JR_OH_SellAccount, costCharge.JR_OH_CostAccount);
				AssertEquals(revenueCharge.JR_OSSellAmt, costCharge.JR_OSCostAmt);

				expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT1",
						JR_OSCostAmt = 66m,
						CostAccountCode = sendingAgent.proxy.OH_Code
					}
				};

				AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, null, sendingAgent.branch, autorateRevenue: false, autorateCosts: true, job: shipmentJob);
				AssertAutoratingAuditLogNoteContainsLines(shipment,
					"Information: AUTORATING COSTS FOR Shipment S00001000",
					"Information: Skipping Rate 'FRT1: Base Rate AUD 66.00' as an apportioned or posted charge with the same code already exists on the Job with 'Rating Behavior' set to STP.",
					"No costs were changed or created.");
			}
		}

		[TestDate(2021, 02, 02)]
		public void TestAutorateShipmentCost_Export_LoginAsGTA_ShipmentIsNotAttachedToAllRequiredConsols_ShouldStillGetICTFromNextGateway()
		{
			CreateGlobalCharge("FRT1");
			CreateGlobalCharge("BAF1");

			var agent1 = GlbCompany.CurrentCompany.OrgProxy;
			agent1.OH_IsCreditor = true;
			var tariff1 = Helper.NewIntercompanyTariff(agent1);
			CreateFlatRate(tariff1, "FRT1", 50m, "AUMEL", "USGLB");

			var (agent2, _) = CreateCompanyAndBranchProxy("USLAX");
			agent2.OH_IsCreditor = true;
			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "BAF1", 100m, "AUMEL", "USLGB");

			AssertEquals("Current Company Org Proxy is on AUBNE", "AUBNE", GlbBranch.CurrentBranch.GB_RL_NKHomePort);

			var consol = CreateForwardingConsolWithGatewayAgents("AUMEL", "AUBNE", null, receivingAgent: agent1);
			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, "AUBNE", AgentStatusList.Codes.GatewayAgent);
			consol.JK_SendingForwarderHandlingType = null;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var shipment = CreateGatewayShipment(consol, "AUMEL", "USLGB", false, false);
			shipment.Gateways.AddNew().ForwarderPK = agent2.PK;
			Factory.Save();

			AssertEquals(2, shipment.Gateways.Count);
			AssertEquals(agent1.PK, shipment.Gateways[0].ForwarderPK);
			AssertEquals(agent2.PK, shipment.Gateways[1].ForwarderPK);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF1",
					CostAccountCode = agent2.OH_Code,
					JR_OSCostAmt = 100m,
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, autorateRevenue: false);
		}

		[TestDate(2021, 02, 02)]
		public void TestAutorateShipmentCost_Export_LoginAsNonGateway_NextGatewayAgentIsGTA_ShipmentIsNotAttachedToAllRequiredConsols_ShouldGetICTFromNextGateway()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("BAF");

			var (agent1, branch1) = CreateNonProxyBranch("NZAKL");
			var (agent2, _) = CreateCompanyAndBranchProxy("AUSYD");
			var (agent3, _) = CreateCompanyAndBranchProxy("USLAX");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;
			}

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "FRT", 50m, "NZAKL", "USLGB");

			var tariff3 = Helper.NewIntercompanyTariff(agent3);
			CreateFlatRate(tariff3, "BAF", 100m, "NZAKL", "USLGB");

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: agent1, receivingAgent: agent2, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, "AUSYD", AgentStatusList.Codes.GatewayAgent);
			consol.JK_SendingForwarderHandlingType = "";
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var shipment = CreateGatewayShipment(consol, "NZAKL", "USLGB", false, false);
			shipment.Gateways.AddNew().ForwarderPK = agent3.PK;
			Factory.Save();

			AssertEquals(2, shipment.Gateways.Count);
			AssertEquals(agent2.PK, shipment.Gateways[0].ForwarderPK);
			AssertEquals(agent3.PK, shipment.Gateways[1].ForwarderPK);

			var expected = new[]
			{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						CostAccountCode = agent2.OH_Code,
						JR_OSCostAmt = 50m
					}
				};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		[TestDate(2021, 02, 02)]
		public void TestAutorateShipmentCost_CrossTrade_LoginAsGTA_ShipmentIsNotAttachedToAllRequiredConsols_ShouldGetICTFromNextGateway()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("BAF");

			var (agent1, branch1) = CreateNonProxyBranch("NZAKL");
			var (agent2, branch2) = CreateCompanyAndBranchProxy("AUSYD");
			var (agent3, _) = CreateCompanyAndBranchProxy("USLAX");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;
			}

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "FRT", 50m, "NZAKL", "USLGB");

			var tariff3 = Helper.NewIntercompanyTariff(agent3);
			CreateFlatRate(tariff3, "BAF", 100m, "NZAKL", "USLGB");

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: agent1, receivingAgent: agent2, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, "AUSYD", AgentStatusList.Codes.GatewayAgent);
			consol.JK_SendingForwarderHandlingType = "";
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var shipment = CreateGatewayShipment(consol, "NZAKL", "USLGB", false, false);
			shipment.Gateways.AddNew().ForwarderPK = agent3.PK;
			Factory.Save();

			AssertEquals(2, shipment.Gateways.Count);
			AssertEquals(agent2.PK, shipment.Gateways[0].ForwarderPK);
			AssertEquals(agent3.PK, shipment.Gateways[1].ForwarderPK);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					CostAccountCode = agent3.OH_Code,
					JR_OSCostAmt = 100m
				}
			};

			// login As AU
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch2, autorateRevenue: false);
		}

		[TestDate(2021, 02, 02)]
		public void TestAutorateShipmentCost_Export_LoginAsNonGateway_NextGatewayAgentIsGTT_ShipmentIsNotAttachedToAllRequiredConsols_ShouldGetICTFromNextGateway()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("BAF");

			var (agent1, branch1) = CreateNonProxyBranch("NZAKL");
			var (agent2, _) = CreateCompanyAndBranchProxy("AUSYD");
			var (agent3, _) = CreateCompanyAndBranchProxy("USLAX");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;
			}

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "FRT", 50m, "NZAKL", "USLGB");

			var tariff3 = Helper.NewIntercompanyTariff(agent3);
			CreateFlatRate(tariff3, "BAF", 100m, "NZAKL", "USLGB");

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: agent1, receivingAgent: agent2, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, "AUSYD", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = "";
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, "NZAKL", "USLGB", false, false);
			shipment.Gateways.AddNew().ForwarderPK = agent3.PK;
			Factory.Save();

			AssertEquals(2, shipment.Gateways.Count);
			AssertEquals(agent2.PK, shipment.Gateways[0].ForwarderPK);
			AssertEquals(agent3.PK, shipment.Gateways[1].ForwarderPK);

			var expected = new[]
			{
					new AssertionCharge
					{
						ChargeCode = "FRT",
						CostAccountCode = agent2.OH_Code,
						JR_OSCostAmt = 50m
					}
				};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		[TestDate(2021, 02, 02)]
		public void TestAutorateShipmentCost_Export_WhenReceivingAgentIsTheLastGatewayAgentOnGatewayGrid_ICTWithGatwayAgentTypeSAGCanBeLoaded()
		{
			CreateGlobalCharge("FRT");

			var (agent1, branch1) = CreateNonProxyBranch("GBMNC");
			var (agent2, _) = CreateBranchProxy("GBLON", branch1.Company);
			var (agent3, _) = CreateCompanyAndBranchProxy("USMIA");

			var receivingAgentIntercompanyTariff = Helper.NewIntercompanyTariff(agent3);
			CreateFlatRate(receivingAgentIntercompanyTariff, "FRT", 100m, "", "US", gatewayAgentType: "SAG");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;
			}

			var consol1 = CreateForwardingConsolWithGatewayAgents("GBMNC", "GBLON", agent1, agent2, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol1.ReceivingForwarderAddress, "GB", AgentStatusList.Codes.GatewayAgent);
			consol1.JK_SendingForwarderHandlingType = string.Empty;
			consol1.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var consol2 = CreateForwardingConsolWithGatewayAgents("GBLHR", "USMIA", agent2, agent3, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol2.ReceivingForwarderAddress, "US", AgentStatusList.Codes.GatewayAgent);
			consol2.JK_PrepaidCollect = PaymentType.Prepaid;
			consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol2.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("GBMNC", "USORL", true, consol1, consol2);
			shipment.JS_INCO = IncoTerms.DeliveredAtPlace;
			Factory.Save();

			AssertEquals(2, shipment.Gateways.Count);
			AssertEquals(agent2.PK, shipment.Gateways[0].ForwarderPK);
			AssertEquals(agent3.PK, shipment.Gateways[1].ForwarderPK);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					CostAccountCode = agent3.OH_Code,
					JR_OSCostAmt = 100m
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		[TestDate(2021, 01, 01)]
		public void TestAutorateShipmentCost_ShouldNotFilterRateWhenConsolHasContractNumber_ShouldNotPublishNumberBack()
		{
			CreateGlobalCharge("FRT1");
			var (sendingAgent, branch1) = CreateCompanyAndBranchProxy("AUSYD");
			var (receivingAgent, _) = CreateCompanyAndBranchProxy("NZAKL");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				sendingAgent.OH_IsCreditor = true;
				receivingAgent.OH_IsCreditor = true;
			}

			var sendingAgentIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(sendingAgentIntercompanyTariff, "FRT1", 100m, "AUMEL", "NZCHC");

			var consol = CreateForwardingConsolWithGatewayAgents("AUSYD", "NZAKL", sendingAgent, receivingAgent, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "AUSYD", AgentStatusList.Codes.GatewayAgentWithTariff);
			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, "NZAKL", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol.JK_CarrierContractNumber = "ABC";
			consol.Numbers.AddNewIfNotExist(CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON, "ABC");

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("AUMEL", "NZCHC", false, consol);
			Factory.Save();

			var mockedDialogService = new Mock<IDialogService>();
			mockedDialogService
				.Setup(x => x.SelectSingleCarrierContractNumber(new[] { "" }))
				.Returns(new SingleCarrierContractNumberSelectionResult(""));
			var testLogger = new TestInteractor();
			var cw1RatesProvider = new CW1RatesProvider(Factory, testLogger);
			var testRatingContext = CreateRatingContextWithDialogService(testLogger, mockedDialogService.Object, cw1RatesProvider: cw1RatesProvider);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 100m
				},
			};

			// The registry is to force the autorating to update the number on the consol
			using (FreightConfigurationRegistry.Instance.IgnoreAndReplaceCarrierContractNumbersDuringAutorating.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false, ratingContext: testRatingContext);
			}

			AssertEquals(
				"Should not populate number from ICT rates back to consol",
				"ABC",
				consol.JK_CarrierContractNumber
			);

			mockedDialogService.Verify(x => x.SelectSingleClientContractNumber(It.IsAny<string[]>()), Times.Never);
		}

		#region Pickup Agent

		[TestDate(2021, 06, 06)]
		public void TestAutorateShipmentCost_CrossTrade_LoginAsNonGateway_PickupAgent_ShouldLoadIntercompanyTariff()
		{
			CreateGlobalCharge("CAF");
			CreateGlobalCharge("BAF");

			var agentAU = CreateCompanyAndBranchProxy("AUSYD");
			var agentNZ = CreateCompanyAndBranchProxy("NZAKL");
			var agentUS = CreateCompanyAndBranchProxy("USLAX");
			var agentHK = CreateCompanyAndBranchProxy("HKHKG");
			var agentSG = CreateCompanyAndBranchProxy("SGSIN");

			var tariffNZ = Helper.NewIntercompanyTariff(agentNZ.proxy);
			CreateFlatRate(tariffNZ, "CAF", 3m, "AUSYD", "HKHKG");

			var tariffUS = Helper.NewIntercompanyTariff(agentUS.proxy);
			CreateFlatRate(tariffUS, "BAF", 5m, "AUSYD", "HKHKG");

			var consol1 = CreateForwardingConsolWithGatewayAgents("AUSYD", "NZAKL", agentAU.proxy, agentNZ.proxy, checkIsItGateway: false);
			consol1.JK_SendingForwarderHandlingType = "";
			SetUpAppointedGatewayAgentPorts(consol1.ReceivingForwarderAddress, "NZAKL", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol1.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consol2 = CreateForwardingConsolWithGatewayAgents("NZAKL", "USLAX", agentNZ.proxy, agentUS.proxy, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol2.SendingForwarderAddress, "NZAKL", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			SetUpAppointedGatewayAgentPorts(consol2.ReceivingForwarderAddress, "USLAX", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol2.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consol3 = CreateForwardingConsolWithGatewayAgents("USLAX", "HKHKG", agentUS.proxy, agentHK.proxy, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol3.SendingForwarderAddress, "USLAX", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol3.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol3.JK_ReceivingForwarderHandlingType = "";

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("AUSYD", "HKHKG", isCollect: true, consol1, consol2, consol3);
			shipment.PickupAgentPK = agentSG.proxy.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge { ChargeCode = "CAF" },
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: agentSG.branch, autorateCosts: true, autorateRevenue: false);
		}

		[TestDate(2021, 06, 06)]
		public void TestAutorateShipmentCost_CrossTrade_LoginAsNonGateway_NotPickupAgent_ShouldNotLoadIntercompanyTariff()
		{
			CreateGlobalCharge("CAF");
			CreateGlobalCharge("BAF");

			var agentAU = CreateCompanyAndBranchProxy("AUSYD");
			var agentNZ = CreateCompanyAndBranchProxy("NZAKL");
			var agentUS = CreateCompanyAndBranchProxy("USLAX");
			var agentHK = CreateCompanyAndBranchProxy("HKHKG");
			var agentSG = CreateNonProxyBranch("SGSIN");

			var tariffNZ = Helper.NewIntercompanyTariff(agentNZ.proxy);
			CreateFlatRate(tariffNZ, "CAF", 3m, "AUSYD", "HKHKG");

			var tariffUS = Helper.NewIntercompanyTariff(agentUS.proxy);
			CreateFlatRate(tariffUS, "BAF", 5m, "AUSYD", "HKHKG");

			var consol1 = CreateForwardingConsolWithGatewayAgents("AUSYD", "NZAKL", agentAU.proxy, agentNZ.proxy, checkIsItGateway: false);
			consol1.JK_SendingForwarderHandlingType = "";
			SetUpAppointedGatewayAgentPorts(consol1.ReceivingForwarderAddress, "NZAKL", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol1.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consol2 = CreateForwardingConsolWithGatewayAgents("NZAKL", "USLAX", agentNZ.proxy, agentUS.proxy, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol2.SendingForwarderAddress, "NZAKL", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			SetUpAppointedGatewayAgentPorts(consol2.ReceivingForwarderAddress, "USLAX", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol2.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consol3 = CreateForwardingConsolWithGatewayAgents("USLAX", "HKHKG", agentUS.proxy, agentHK.proxy, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol3.SendingForwarderAddress, "USLAX", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol3.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol3.JK_ReceivingForwarderHandlingType = "";

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("AUSYD", "HKHKG", isCollect: true, consol1, consol2, consol3);
			shipment.PickupAgentPK = agentHK.proxy.PK;

			Factory.Save();

			var expected = Array.Empty<AssertionCharge>();

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: agentSG.branch, autorateCosts: true, autorateRevenue: false);
		}

		[TestDate(2021, 06, 06)]
		public void TestAutorateShipmentCost_Export_LoginAsNonGateway_NotPickupAgent_ShouldNotLoadIntercompanyTariff()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("CAF");
			CreateGlobalCharge("BAF");

			var (agent1, branch1) = CreateCompanyAndBranchProxy("CHZUR");
			var (agent2, _) = CreateCompanyAndBranchProxy("CHBSL");
			var (agent3, _) = CreateCompanyAndBranchProxy("USCHS");
			var (agent4, _) = CreateCompanyAndBranchProxy("USHOU");

			var pickupAgent = agent2;

			var chargeCode = Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			chargeCode.AC_GC = branch1.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;
				agent4.OH_IsCreditor = true;

				Helper.NewCosting(pickupAgent).AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "US", "ORG1", 50);
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent1);
			CreateFlatRate(tariff1, "FRT", 100m, "CHZUR", "USHOU");

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "CAF", 110m, "CHZUR", "USHOU");

			var tariff3 = Helper.NewIntercompanyTariff(agent3);
			CreateFlatRate(tariff3, "BAF", 120m, "CHZUR", "USHOU");

			pickupAgent.OH_IsCreditor = true;

			var consol1 = CreateForwardingConsolWithGatewayAgents("CHZUR", "CHBSL", agent1, agent2, checkIsItGateway: false);
			consol1.JK_SendingForwarderHandlingType = "";
			consol1.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			SetUpAppointedGatewayAgentPorts(consol1.ReceivingForwarderAddress, "CHBSL", AgentStatusList.Codes.GatewayAgentWithTariff);

			var consol2 = CreateForwardingConsolWithGatewayAgents("CHBSL", "USCHS", agent2, agent3, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol2.ReceivingForwarderAddress, "USCHS", AgentStatusList.Codes.GatewayAgentWithTariff);
			SetUpAppointedGatewayAgentPorts(consol2.SendingForwarderAddress, "CHBSL", AgentStatusList.Codes.GatewayAgentWithTariff);

			consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol2.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consol3 = CreateForwardingConsolWithGatewayAgents("USCHS", "USHOU", agent3, agent4, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol3.SendingForwarderAddress, "USCHS", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol3.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol3.JK_ReceivingForwarderHandlingType = "";

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "USHOU", false, consol1, consol2, consol3);
			shipment.PickupAgentPK = pickupAgent.PK;
			Factory.Save();

			var shipmentGateWaysPKs = shipment.Gateways.Select(x => x.ForwarderPK);

			CombineAssertions("Shipment gateways should not contain none gateway sending/receiving agent", () =>
			{
				AssertCollectionNotContains(agent1.PK, shipmentGateWaysPKs);
				AssertCollectionNotContains(agent4.PK, shipmentGateWaysPKs);
				AssertCollectionContains(agent2.PK, shipmentGateWaysPKs);
				AssertCollectionContains(agent3.PK, shipmentGateWaysPKs);
			});

			// Should load normal Costing but shouldn't load ICT
			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ORG1"
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		[TestDate(2021, 06, 06)]
		public void TestAutorateShipmentCost_Export_LoginAsGTT_NotPickupAgent_ShouldNotLoadIntercompanyTariff()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("CAF");
			CreateGlobalCharge("BAF");

			var (agent1, branch1) = CreateCompanyAndBranchProxy("CHZUR");
			var (agent2, branch2) = CreateBranchProxy("CHBSL", branch1.Company);
			var (agent3, _) = CreateCompanyAndBranchProxy("USCHS");
			var (agent4, _) = CreateCompanyAndBranchProxy("USHOU");

			var pickupAgent = agent3;

			var chargeCode = Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			chargeCode.AC_GC = branch2.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;
				agent4.OH_IsCreditor = true;

				Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "US", "ORG1", 50);
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent1);
			CreateFlatRate(tariff1, "FRT", 100m, "CHZUR", "USHOU");

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "CAF", 110m, "CHZUR", "USHOU");

			var tariff3 = Helper.NewIntercompanyTariff(agent3);
			CreateFlatRate(tariff3, "BAF", 120m, "CHZUR", "USHOU");

			pickupAgent.OH_IsCreditor = true;
			TransportProvider1.OH_IsCreditor = true;

			var consol1 = CreateForwardingConsolWithGatewayAgents("CHZUR", "CHBSL", agent1, agent2, checkIsItGateway: false);
			consol1.JK_SendingForwarderHandlingType = "";
			consol1.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			SetUpAppointedGatewayAgentPorts(consol1.ReceivingForwarderAddress, "CHBSL", AgentStatusList.Codes.GatewayAgentWithTariff);

			var consol2 = CreateForwardingConsolWithGatewayAgents("CHBSL", "USCHS", agent2, agent3, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol2.ReceivingForwarderAddress, "USCHS", AgentStatusList.Codes.GatewayAgentWithTariff);
			SetUpAppointedGatewayAgentPorts(consol2.SendingForwarderAddress, "CHBSL", AgentStatusList.Codes.GatewayAgentWithTariff);

			consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol2.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consol3 = CreateForwardingConsolWithGatewayAgents("USCHS", "USHOU", agent3, agent4, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol3.SendingForwarderAddress, "USCHS", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol3.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol3.JK_ReceivingForwarderHandlingType = "";

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "USHOU", false, consol1, consol2, consol3);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			shipment.PickupAgentPK = pickupAgent.PK;
			Factory.Save();

			var shipmentGateWaysPKs = shipment.Gateways.Select(x => x.ForwarderPK);

			CombineAssertions("Shipment gateways should not contain none gateway sending/receiving agent", () =>
			{
				AssertCollectionNotContains(agent1.PK, shipmentGateWaysPKs);
				AssertCollectionNotContains(agent4.PK, shipmentGateWaysPKs);
				AssertCollectionContains(agent2.PK, shipmentGateWaysPKs);
				AssertCollectionContains(agent3.PK, shipmentGateWaysPKs);
			});

			// Should load normal Costing but shouldn't load ICT
			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ORG1"
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch2, autorateRevenue: false);
		}

		[TestDate(2021, 06, 06)]
		public void TestAutorateShipmentCost_CrossTrade_ShipmentIsNotAttachedToConsol_PickupAgent_ShouldLoadIntercompanyTariff()
		{
			CreateGlobalCharge("FRT1");
			CreateGlobalCharge("BAF1");

			var (agent1, branch1) = CreateCompanyAndBranchProxy("BEANR");
			var tariff1 = Helper.NewIntercompanyTariff(agent1);
			CreateFlatRate(tariff1, "FRT1", 100m, "AUMEL", "USLAX");

			var (agent2, _) = CreateCompanyAndBranchProxy("DEBRE");
			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "BAF1", 120m, "AUMEL", "USLAX");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
			}

			var shipment = CreateStandaloneGatewayShipment("AUMEL", "USLAX", true, agent1.PK, agent2.PK);
			shipment.PickupAgentPK = agent1.PK;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					CostAccountCode = agent1.OH_Code,
					JR_OSCostAmt = 100m
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		[TestDate(2021, 06, 06)]
		public void TestAutorateShipmentCost_CrossTrade_ShipmentIsNotAttachedToConsol_NotPickupAgent_ShouldNotLoadIntercompanyTariff()
		{
			CreateGlobalCharge("FRT1");
			CreateGlobalCharge("BAF1");

			var (agent1, branch1) = CreateCompanyAndBranchProxy("BEANR");
			var tariff1 = Helper.NewIntercompanyTariff(agent1);
			CreateFlatRate(tariff1, "FRT1", 100m, "AUMEL", "USLAX");

			var (agent2, _) = CreateCompanyAndBranchProxy("DEBRE");
			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "BAF1", 120m, "AUMEL", "USLAX");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
			}

			var shipment = CreateStandaloneGatewayShipment("AUMEL", "USLAX", true, agent1.PK, agent2.PK);
			shipment.PickupAgentPK = agent2.PK;
			Factory.Save();

			var expected = Array.Empty<AssertionCharge>();

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		[TestDate(2021, 06, 06)]
		public void TestAutorateShipmentCost_Export_ShipmentIsNotAttachedToConsol_LoginCompanyIsDifferentToFirstGatewayAgent_NotPickupAgent_ShouldNotLoadIntercompanyTariff()
		{
			CreateGlobalCharge("FRT1");
			CreateGlobalCharge("BAF1");

			Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "AU", "US", "ORG1", 50);

			var (agent1, _) = CreateCompanyAndBranchProxy("CHBSL");
			agent1.OH_IsCreditor = true;
			var tariff1 = Helper.NewIntercompanyTariff(agent1);
			CreateFlatRate(tariff1, "FRT1", 100m, "AUMEL", "USLAX");

			var (agent2, _) = CreateCompanyAndBranchProxy("DEBRE");
			agent2.OH_IsCreditor = true;
			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "BAF1", 120m, "AUMEL", "USLAX");

			TransportProvider1.OH_IsCreditor = true;
			AssertEquals("Current Company Org Proxy is on AUBNE", "AUBNE", GlbBranch.CurrentBranch.GB_RL_NKHomePort);

			var shipment = CreateStandaloneGatewayShipment("AUMEL", "USLAX", true, agent1.PK, agent2.PK);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			shipment.PickupAgentPK = agent1.PK;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ORG1",
					CostAccountCode = TransportProvider1.OH_Code,
					JR_OSCostAmt = 50m
				}
			};

			// FRT1 & BAF1 should not be loaded
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, autorateRevenue: false);
		}

		[TestDate(2021, 06, 06)]
		public void TestAutorateShipmentCost_Export_ShipmentIsNotAttachedToConsol_LoginUnderFirstGatewayAgentWithSameBranch_NotPickupAgent_ShouldNotLoadIntercompanyTariff()
		{
			CreateGlobalCharge("FRT1");

			Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);

			var (pickupAgent, _) = CreateCompanyAndBranchProxy("AUBNE");
			var orgProxy = GlbCompany.CurrentCompany.OrgProxy;

			Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "AU", "US", "ORG1", 50);

			var tariff1 = Helper.NewIntercompanyTariff(orgProxy);
			CreateFlatRate(tariff1, "FRT1", 100m, "AUBNE", "USLAX");

			TransportProvider1.OH_IsCreditor = true;
			AssertEquals("Current Company Org Proxy is on AUBNE", "AUBNE", GlbBranch.CurrentBranch.GB_RL_NKHomePort);

			var shipment = CreateStandaloneGatewayShipment("AUBNE", "USLAX", true, orgProxy.PK);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			shipment.PickupAgentPK = pickupAgent.PK;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ORG1",
					CostAccountCode = TransportProvider1.OH_Code,
					JR_OSCostAmt = 50m
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, autorateRevenue: false);
		}

		[TestDate(2021, 06, 06)]
		public void TestAutorateShipmentCost_Export_ShipmentIsNotAttachedToConsol_LoginUnderFirstGatewayAgentButDifferentBranch_NotPickupAgent_ShouldNotLoadIntercompanyTariff()
		{
			CreateGlobalCharge("FRT1");
			var (pickupAgent, _) = CreateCompanyAndBranchProxy("AUMEL");
			var orgProxy = GlbCompany.CurrentCompany.OrgProxy;

			var tariff1 = Helper.NewIntercompanyTariff(orgProxy);
			CreateFlatRate(tariff1, "FRT1", 100m, "AUMEL", "USLAX");

			AssertEquals("Current Company Org Proxy is on AUBNE", "AUBNE", GlbBranch.CurrentBranch.GB_RL_NKHomePort);

			var shipment = CreateStandaloneGatewayShipment("AUMEL", "USLAX", true, orgProxy.PK);
			shipment.PickupAgentPK = pickupAgent.PK;
			Factory.Save();

			var expected = Array.Empty<AssertionCharge>();

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, autorateRevenue: false);
		}

		[TestDate(2021, 06, 06)]
		public void TestAutorateShipmentCost_Export_LoginAsGTA_PickupAgent_ShouldLoadIntercompanyTariff()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("CAF");

			var (agent1, branch1) = CreateCompanyAndBranchProxy("BEANR");
			var (agent2, branch2) = CreateCompanyAndBranchProxy("SGSIN");
			var nonGatewayAgent = CreateNonProxyBranch("AUSYD").org;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent1);
			CreateFlatRate(tariff1, "FRT", 100m, "BEANR", "AUSYD");

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "CAF", 110m, "BEANR", "AUSYD");

			var consol1 = CreateForwardingConsolWithGatewayAgents("BEANR", "SGSIN", agent1, agent2, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol1.SendingForwarderAddress, "BEANR", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol1.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			SetUpAppointedGatewayAgentPorts(consol1.ReceivingForwarderAddress, "SGSIN", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol1.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consol2 = CreateForwardingConsolWithGatewayAgents("SGSIN", "AUSYD", agent2, nonGatewayAgent, checkIsItGateway: false);
			consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol2.JK_ReceivingForwarderHandlingType = "";

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("BEANR", "AUSYD", false, consol1, consol2);
			shipment.PickupAgentPK = agent1.PK;
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { agent1.PK, agent2.PK }, shipment.Gateways.Select(x => x.ForwarderPK).ToArray());

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "CAF"
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		[TestDate(2021, 06, 06)]
		public void TestAutorateShipmentCost_Export_LoginAsGTA_NotPickupAgent_ShouldLoadIntercompanyTariffForNextGatewayAgent()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("CAF");

			var (agent1, branch1) = CreateCompanyAndBranchProxy("BEANR");
			var (agent2, branch2) = CreateCompanyAndBranchProxy("SGSIN");
			var nonGatewayAgent = CreateNonProxyBranch("AUSYD").org;

			var (pickupAgent, _) = CreateCompanyAndBranchProxy("DEHAM");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
				pickupAgent.OH_IsCreditor = true;
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent1);
			CreateFlatRate(tariff1, "FRT", 100m, "BEANR", "AUSYD");

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "CAF", 110m, "BEANR", "AUSYD");

			var consol1 = CreateForwardingConsolWithGatewayAgents("BEANR", "SGSIN", agent1, agent2, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol1.SendingForwarderAddress, "BEANR", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol1.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			SetUpAppointedGatewayAgentPorts(consol1.ReceivingForwarderAddress, "SGSIN", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol1.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consol2 = CreateForwardingConsolWithGatewayAgents("SGSIN", "AUSYD", agent2, nonGatewayAgent, checkIsItGateway: false);
			consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol2.JK_ReceivingForwarderHandlingType = "";

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("BEANR", "AUSYD", false, consol1, consol2);
			shipment.PickupAgentPK = pickupAgent.PK;
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { agent1.PK, agent2.PK }, shipment.Gateways.Select(x => x.ForwarderPK).ToArray());

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "CAF"
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		[TestDate(2021, 06, 06)]
		public void TestAutorateShipmentCost_CrossTrade_LoginAsGTA_PickupAgent_ShouldLoadIntercompanyTariff()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("BAF");

			var (agent1, branch1) = CreateNonProxyBranch("NZAKL");
			var (agent2, branch2) = CreateCompanyAndBranchProxy("AUSYD");
			var (agent3, _) = CreateCompanyAndBranchProxy("USLAX");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;
			}

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "FRT", 50m, "NZAKL", "USLGB");

			var tariff3 = Helper.NewIntercompanyTariff(agent3);
			CreateFlatRate(tariff3, "BAF", 100m, "NZAKL", "USLGB");

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: agent1, receivingAgent: agent2, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, "AUSYD", AgentStatusList.Codes.GatewayAgent);
			consol.JK_SendingForwarderHandlingType = "";
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var shipment = CreateGatewayShipment(consol, "NZAKL", "USLGB", false, false);
			shipment.Gateways.AddNew().ForwarderPK = agent3.PK;
			shipment.PickupAgentPK = agent2.PK;
			Factory.Save();

			AssertEquals(2, shipment.Gateways.Count);
			AssertEquals(agent2.PK, shipment.Gateways[0].ForwarderPK);
			AssertEquals(agent3.PK, shipment.Gateways[1].ForwarderPK);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					CostAccountCode = agent3.OH_Code,
					JR_OSCostAmt = 100m
				}
			};

			// login As AU
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch2, autorateRevenue: false);
		}

		[TestDate(2021, 06, 06)]
		public void TestAutorateShipmentCost_CrossTrade_LoginAsGTA_NotPickupAgent_ShouldLoadIntercompanyTariff()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("BAF");

			var (agent1, branch1) = CreateNonProxyBranch("NZAKL");
			var (agent2, branch2) = CreateCompanyAndBranchProxy("AUSYD");
			var (agent3, _) = CreateCompanyAndBranchProxy("USLAX");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;
			}

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "FRT", 50m, "NZAKL", "USLGB");

			var tariff3 = Helper.NewIntercompanyTariff(agent3);
			CreateFlatRate(tariff3, "BAF", 100m, "NZAKL", "USLGB");

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: agent1, receivingAgent: agent2, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, "AUSYD", AgentStatusList.Codes.GatewayAgent);
			consol.JK_SendingForwarderHandlingType = "";
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var shipment = CreateGatewayShipment(consol, "NZAKL", "USLGB", false, false);
			shipment.Gateways.AddNew().ForwarderPK = agent3.PK;
			shipment.PickupAgentPK = agent3.PK;
			Factory.Save();

			AssertEquals(2, shipment.Gateways.Count);
			AssertEquals(agent2.PK, shipment.Gateways[0].ForwarderPK);
			AssertEquals(agent3.PK, shipment.Gateways[1].ForwarderPK);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					CostAccountCode = agent3.OH_Code,
					JR_OSCostAmt = 100m
				}
			};

			// login As AU
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch2, autorateRevenue: false);
		}

		[TestDate(2021, 06, 06)]
		public void TestAutorateShipmentCost_CrossTrade_LoginAsGTT_PickupAgent_ShouldLoadIntercompanyTariffForFirstGatewayAgent()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("BAF");

			var (agent1, branch1) = CreateCompanyAndBranchProxy("SGSIN");
			var (agent2, _) = CreateCompanyAndBranchProxy("DEFRA");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent1);
			CreateFlatRate(tariff1, "FRT", 50m, "MYPEN", "DEFRA");

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "BAF", 100m, "MYPEN", "DEFRA");

			var consol = CreateForwardingConsolWithGatewayAgents("SGSIN", "DEFRA", sendingAgent: agent1, receivingAgent: agent2, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "SGSIN", AgentStatusList.Codes.GatewayAgentWithTariff);
			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, "DEFRA", AgentStatusList.Codes.GatewayAgentWithTariff);

			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, "MYPEN", "DEFRA", false, false);
			shipment.PickupAgentPK = agent1.PK;
			Factory.Save();

			AssertEquals(2, shipment.Gateways.Count);
			AssertEquals(agent1.PK, shipment.Gateways[0].ForwarderPK);
			AssertEquals(agent2.PK, shipment.Gateways[1].ForwarderPK);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					CostAccountCode = agent1.OH_Code,
					JR_OSCostAmt = 50m
				}
			};

			// login As SG
			using (DataRegistryRating.Instance.RatesServiceRateSelector.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled()))
			{
				AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
			}
		}

		[TestDate(2021, 06, 06)]
		public void TestAutorateShipmentCost_CrossTrade_LoginAsRecevingGTT_PickupAgentIsReceiving_ShouldLoadIntercompanyTariffForFirstGatewayAgent()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("BAF");

			var (agent1, branch1) = CreateCompanyAndBranchProxy("SGSIN");
			var (agent2, branch2) = CreateCompanyAndBranchProxy("DEFRA");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent1);
			CreateFlatRate(tariff1, "FRT", 50m, "SGSIN", "MYPEN");

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "BAF", 100m, "SGSIN", "MYPEN");

			var consol = CreateForwardingConsolWithGatewayAgents("SGSIN", "DEFRA", sendingAgent: agent1, receivingAgent: agent2, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "SGSIN", AgentStatusList.Codes.GatewayAgentWithTariff);
			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, "DEFRA", AgentStatusList.Codes.GatewayAgentWithTariff);

			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, "SGSIN", "MYPEN", false, false);
			shipment.PickupAgentPK = agent2.PK;
			Factory.Save();

			AssertEquals(2, shipment.Gateways.Count);
			AssertEquals(agent1.PK, shipment.Gateways[0].ForwarderPK);
			AssertEquals(agent2.PK, shipment.Gateways[1].ForwarderPK);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 50m
				}
			};

			// login As DE
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch2, autorateRevenue: false);
		}

		#endregion

		#endregion

		#region Autorate Costs from Job Invoicing Menu

		public void TestBaseAutorateCost()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: null);
			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
				}
			};

			var expectedCharge = new AssertionCharge() { ChargeCode = "FRT", JR_OSCostAmt = 100m };
			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, new [] { expectedCharge } }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: sendingAgent.Branch, autorateCosts: true, autorateRevenue: false);
		}

		public void TestAutorateCostFromJobInvoicingMenu_WhenReceivingAgentIsGTASendingAgentIsNullAndLoginAsReceivingAgent_ShouldReturnCostingPerExistingBehavior()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var receivingAgent = GlbCompany.CurrentCompany.OrgProxy;
			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: null, receivingAgent: receivingAgent);
			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			AssertEquals("Receiving Agent type should be GTA", consol.JK_ReceivingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
				}
			};

			var expectedCharge = new AssertionCharge() { ChargeCode = "FRT", JR_OSCostAmt = 100m };
			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, new [] { expectedCharge } }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: receivingAgent.Branch, autorateCosts: true, autorateRevenue: false);
		}

		public void TestAutorateCostFromJobInvoicingMenu_WhenReceivingAgentIsGTASendingAgentNotNullAndLoginAsReceivingAgent_ShouldReturnCostingPerExistingBehavior()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var receivingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var sendingAgent = CreateBranchProxy("USCHS");

			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent.proxy, receivingAgent: receivingAgent);
			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			AssertEquals("Receiving Agent type should be GTA", consol.JK_ReceivingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
				}
			};

			var expectedCharge = new AssertionCharge() { ChargeCode = "FRT", JR_OSCostAmt = 100m };
			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, new [] { expectedCharge } }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: receivingAgent.Branch, autorateCosts: true, autorateRevenue: false);
		}

		public void TestAutorateCostFromJobInvoicingMenu_WhenReceivingAgentIsGTTSendingAgentIsNullAndLoginAsReceivingAgent_ShouldNotReturnAnyCost()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var receivingAgent = GlbCompany.CurrentCompany.OrgProxy;
			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: null, receivingAgent: receivingAgent);
			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			AssertEquals("Receiving Agent type should be GTT", consol.JK_ReceivingForwarderHandlingType, AgentStatusList.Codes.GatewayAgentWithTariff);

			var expectedCosts = Array.Empty<AssertionCost>();

			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, Array.Empty<AssertionCharge>() }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: receivingAgent.Branch, autorateCosts: true, autorateRevenue: false);
		}

		public void TestAutorateCostFromJobInvoicingMenu_WhenReceivingAgentIsGTTSendingAgentNotNullAndLoginAsReceivingAgent_ShouldNotReturnAnyCost()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var receivingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var sendingAgent = CreateBranchProxy("USCHS");

			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent.proxy, receivingAgent: receivingAgent);
			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			AssertEquals("Receiving Agent type should be GTT", consol.JK_ReceivingForwarderHandlingType, AgentStatusList.Codes.GatewayAgentWithTariff);

			var expectedCosts = Array.Empty<AssertionCost>();

			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, Array.Empty<AssertionCharge>() }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: receivingAgent.Branch, autorateCosts: true, autorateRevenue: false);
		}

		public void TestAutorateCostFromJobInvoicingMenu_WhenReceivingAgentIsGTTSendingAgentNotNullAndBothUnderSameCompanyWithCurrentLogin_ShouldNotReturnAnyCost()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var receivingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var sendingAgent = CreateBranchProxy("USCHS");
			var branchWithProxy = CreateBranchProxy("AUSYD");

			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent.proxy, receivingAgent: receivingAgent);
			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			AssertEquals("Receiving Agent type should be GTT", consol.JK_ReceivingForwarderHandlingType, AgentStatusList.Codes.GatewayAgentWithTariff);

			var expectedCosts = Array.Empty<AssertionCost>();

			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, Array.Empty<AssertionCharge>() }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: branchWithProxy.branch, autorateCosts: true, autorateRevenue: false);
		}

		public void TestAutorateCostFromJobInvoicingMenu_WhenReceivingAgentIsEmptySendingAgentIsNullAndLoginAsReceivingAgent_ShouldReturnCostingPerExistingBehavior()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var receivingAgent = GlbCompany.CurrentCompany.OrgProxy;

			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: null, receivingAgent: receivingAgent);
			consol.JK_ReceivingForwarderHandlingType = "";

			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			AssertEquals("Receiving Agent type should be empty", consol.JK_ReceivingForwarderHandlingType, string.Empty);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
				}
			};

			var expectedCharge = new AssertionCharge() { ChargeCode = "FRT", JR_OSCostAmt = 100m };
			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, new [] { expectedCharge } }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: receivingAgent.Branch, autorateCosts: true, autorateRevenue: false);
		}

		public void TestAutorateCostFromJobInvoicingMenu_WhenReceivingAgentIsEmptySendingAgentNotNullAndLoginAsReceivingAgent_ShouldReturnCostingPerExistingBehavior()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var receivingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var sendingAgent = CreateBranchProxy("USCHS");

			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent.proxy, receivingAgent: receivingAgent);
			consol.JK_ReceivingForwarderHandlingType = "";

			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			AssertEquals("Receiving Agent type should be empty", consol.JK_ReceivingForwarderHandlingType, string.Empty);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
				}
			};

			var expectedCharge = new AssertionCharge() { ChargeCode = "FRT", JR_OSCostAmt = 100m };
			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, new [] { expectedCharge } }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: receivingAgent.Branch, autorateCosts: true, autorateRevenue: false);
		}

		public void TestAutorateCostFromJobInvoicingMenu_WhenSendingAgentIsGTAReceivingAgentIsNullAndLoginAsSendingAgent_ShouldReturnCostingPerExistingBehavior()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;

			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: null);
			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			AssertEquals("Sending Agent type should be GTA", consol.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
				}
			};

			var expectedCharge = new AssertionCharge() { ChargeCode = "FRT", JR_OSCostAmt = 100m };
			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, new [] { expectedCharge } }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: sendingAgent.Branch, autorateCosts: true, autorateRevenue: false);
		}

		public void TestAutorateCostFromJobInvoicingMenu_WhenSendingAgentIsGTAReceivingAgentNotNullAndLoginAsSendingAgent_ShouldReturnCostingPerExistingBehavior()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateBranchProxy("USCHS");

			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			AssertEquals("Sending Agent type should be GTA", consol.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
				}
			};

			var expectedCharge = new AssertionCharge() { ChargeCode = "FRT", JR_OSCostAmt = 100m };
			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, new [] { expectedCharge } }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: sendingAgent.Branch, autorateCosts: true, autorateRevenue: false);
		}

		public void TestAutorateCostFromJobInvoicingMenu_WhenSendingAgentIsGTTReceivingAgentIsNullAndLoginAsSendingAgent_ShouldNotReturnAnyCost()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;

			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: null);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			AssertEquals("Sending Agent type should be GTT", consol.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgentWithTariff);

			var expectedCosts = Array.Empty<AssertionCost>();

			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, Array.Empty<AssertionCharge>() }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: sendingAgent.Branch, autorateCosts: true, autorateRevenue: false);
		}

		public void TestAutorateCostFromJobInvoicingMenu_WhenSendingAgentIsGTTReceivingAgentIsNotNullAndLoginAsSendingAgent_ShouldNotReturnAnyCost()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateBranchProxy("USCHS");

			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			AssertEquals("Sending Agent type should be GTT", consol.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgentWithTariff);

			var expectedCosts = Array.Empty<AssertionCost>();

			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, Array.Empty<AssertionCharge>() }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: sendingAgent.Branch, autorateCosts: true, autorateRevenue: false);
		}

		public void TestAutorateCostFromJobInvoicingMenu_WhenSendingAgentIsGTTReceivingAgentIsNotNullAndBothUnderSameCompanyWithCurrentLogin_ShouldNotReturnAnyCost()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateBranchProxy("USCHS");
			var branchWithProxy = CreateBranchProxy("AUSYD");

			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			AssertEquals("Sending Agent type should be GTT", consol.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgentWithTariff);

			var expectedCosts = Array.Empty<AssertionCost>();

			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, Array.Empty<AssertionCharge>() }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: branchWithProxy.branch, autorateCosts: true, autorateRevenue: false);
		}

		public void TestAutorateCostFromJobInvoicingMenu_WhenSendingAgentIsEmptyReceivingAgentIsNullAndLoginAsSendingAgent_ShouldReturnCostingPerExistingBehavior()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;

			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: null);
			consol.JK_SendingForwarderHandlingType = "";

			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			AssertEquals("Sending Agent type should be empty", consol.JK_SendingForwarderHandlingType, string.Empty);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
				}
			};

			var expectedCharge = new AssertionCharge() { ChargeCode = "FRT", JR_OSCostAmt = 100m };
			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, new [] { expectedCharge } }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: sendingAgent.Branch, autorateCosts: true, autorateRevenue: false);
		}

		public void TestAutorateCostFromJobInvoicingMenu_WhenSendingAgentIsEmptyReceivingAgentNotNullAndLoginAsSendingAgent_ShouldReturnCostingPerExistingBehavior()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateBranchProxy("USCHS");

			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			consol.JK_SendingForwarderHandlingType = "";
			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			AssertEquals("Sending Agent type should be GTA", consol.JK_SendingForwarderHandlingType, string.Empty);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
				}
			};

			var expectedCharge = new AssertionCharge() { ChargeCode = "FRT", JR_OSCostAmt = 100m };
			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, new [] { expectedCharge } }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: sendingAgent.Branch, autorateCosts: true, autorateRevenue: false);
		}

		#endregion

		#region Autorate Costs and Revenue from Job Invoicing Menu

		public void TestBaseAutorateCostAndRevenue()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: null);
			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
				}
			};

			var expectedCharge1 = new AssertionCharge() { ChargeCode = "FRT", JR_OSCostAmt = 100m };
			var expectedCharge2 = new AssertionCharge() { ChargeCode = "BAF", JR_OSCostAmt = 200m };
			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, new [] { expectedCharge1, expectedCharge2 } }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: sendingAgent.Branch, autorateCosts: true, autorateRevenue: true);
		}

		public void TestAutorateCostAndRevenueFromJobInvoicingMenu_WhenReceivingAgentIsGTASendingAgentIsNullAndLoginAsReceivingAgent_ShouldReturnCostingPerExistingBehavior()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var receivingAgent = GlbCompany.CurrentCompany.OrgProxy;
			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: null, receivingAgent: receivingAgent);
			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			AssertEquals("Receiving Agent type should be GTA", consol.JK_ReceivingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
				}
			};

			var expectedCharge1 = new AssertionCharge() { ChargeCode = "FRT", JR_OSCostAmt = 100m };
			var expectedCharge2 = new AssertionCharge() { ChargeCode = "BAF", JR_OSCostAmt = 200m };
			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, new [] { expectedCharge1, expectedCharge2 } }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: receivingAgent.Branch, autorateCosts: true, autorateRevenue: true);
		}

		public void TestAutorateCostAndRevenueFromJobInvoicingMenu_WhenReceivingAgentIsGTASendingAgentNotNullAndLoginAsReceivingAgent_ShouldReturnCostingPerExistingBehavior()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var receivingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var sendingAgent = CreateBranchProxy("USCHS");

			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent.proxy, receivingAgent: receivingAgent);
			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			AssertEquals("Receiving Agent type should be GTA", consol.JK_ReceivingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
				}
			};

			var expectedCharge1 = new AssertionCharge() { ChargeCode = "FRT", JR_OSCostAmt = 100m };
			var expectedCharge2 = new AssertionCharge() { ChargeCode = "BAF", JR_OSCostAmt = 200m };
			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, new [] { expectedCharge1, expectedCharge2 } }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: receivingAgent.Branch, autorateCosts: true, autorateRevenue: true);
		}

		public void TestAutorateCostAndRevenueFromJobInvoicingMenu_WhenReceivingAgentIsGTTSendingAgentIsNullAndLoginAsReceivingAgent_ShouldNotReturnAnyCost()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var receivingAgent = GlbCompany.CurrentCompany.OrgProxy;
			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: null, receivingAgent: receivingAgent);
			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			AssertEquals("Receiving Agent type should be GTT", consol.JK_ReceivingForwarderHandlingType, AgentStatusList.Codes.GatewayAgentWithTariff);

			var expectedCosts = Array.Empty<AssertionCost>();
			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, Array.Empty<AssertionCharge>() }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: receivingAgent.Branch, autorateCosts: true, autorateRevenue: true);
		}

		public void TestAutorateCostAndRevenueFromJobInvoicingMenu_WhenReceivingAgentIsGTTSendingAgentNotNullAndLoginAsReceivingAgent_ShouldNotReturnAnyCost()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var receivingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var sendingAgent = CreateBranchProxy("USCHS");

			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent.proxy, receivingAgent: receivingAgent);
			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			AssertEquals("Receiving Agent type should be GTT", consol.JK_ReceivingForwarderHandlingType, AgentStatusList.Codes.GatewayAgentWithTariff);

			var expectedCosts = Array.Empty<AssertionCost>();
			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, Array.Empty<AssertionCharge>() }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: receivingAgent.Branch, autorateCosts: true, autorateRevenue: true);
		}

		public void TestAutorateCostAndRevenueFromJobInvoicingMenu_WhenReceivingAgentIsEmptySendingAgentIsNullAndLoginAsReceivingAgent_ShouldReturnCostingPerExistingBehavior()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var receivingAgent = GlbCompany.CurrentCompany.OrgProxy;
			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: null, receivingAgent: receivingAgent);
			consol.JK_ReceivingForwarderHandlingType = "";

			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			AssertEquals("Receiving Agent type should be empty", consol.JK_ReceivingForwarderHandlingType, string.Empty);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
				}
			};

			var expectedCharge1 = new AssertionCharge() { ChargeCode = "FRT", JR_OSCostAmt = 100m };
			var expectedCharge2 = new AssertionCharge() { ChargeCode = "BAF", JR_OSCostAmt = 200m };
			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, new [] { expectedCharge1, expectedCharge2 } }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: receivingAgent.Branch, autorateCosts: true, autorateRevenue: true);
		}

		public void TestAutorateCostAndRevenueFromJobInvoicingMenu_WhenReceivingAgentIsEmptySendingAgentNotNullAndLoginAsReceivingAgent_ShouldReturnCostingPerExistingBehavior()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var receivingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var sendingAgent = CreateBranchProxy("USCHS");

			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent.proxy, receivingAgent: receivingAgent);
			consol.JK_ReceivingForwarderHandlingType = "";

			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			AssertEquals("Receiving Agent type should be empty", consol.JK_ReceivingForwarderHandlingType, string.Empty);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
				}
			};

			var expectedCharge1 = new AssertionCharge() { ChargeCode = "FRT", JR_OSCostAmt = 100m };
			var expectedCharge2 = new AssertionCharge() { ChargeCode = "BAF", JR_OSCostAmt = 200m };
			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, new [] { expectedCharge1, expectedCharge2 } }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: receivingAgent.Branch, autorateCosts: true, autorateRevenue: true);
		}

		public void TestAutorateCostAndRevenueFromJobInvoicingMenu_WhenSendingAgentIsGTAReceivingAgentIsNullAndLoginAsSendingAgent_ShouldReturnCostingPerExistingBehavior()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;

			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: null);
			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			AssertEquals("Sending Agent type should be GTA", consol.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
				}
			};

			var expectedCharge1 = new AssertionCharge() { ChargeCode = "FRT", JR_OSCostAmt = 100m };
			var expectedCharge2 = new AssertionCharge() { ChargeCode = "BAF", JR_OSCostAmt = 200m };
			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, new [] { expectedCharge1, expectedCharge2 } }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: sendingAgent.Branch, autorateCosts: true, autorateRevenue: true);
		}

		public void TestAutorateCostAndRevenueFromJobInvoicingMenu_WhenSendingAgentIsGTAReceivingAgentNotNullAndLoginAsSendingAgent_ShouldReturnCostingPerExistingBehavior()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateBranchProxy("USCHS");

			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			AssertEquals("Sending Agent type should be GTA", consol.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
				}
			};

			var expectedCharge1 = new AssertionCharge() { ChargeCode = "FRT", JR_OSCostAmt = 100m };
			var expectedCharge2 = new AssertionCharge() { ChargeCode = "BAF", JR_OSCostAmt = 200m };
			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, new [] { expectedCharge1, expectedCharge2 } }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: sendingAgent.Branch, autorateCosts: true, autorateRevenue: true);
		}

		public void TestAutorateCostAndRevenueFromJobInvoicingMenu_WhenSendingAgentIsGTTReceivingAgentIsNullAndLoginAsSendingAgent_ShouldNotReturnAnyCost()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;

			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: null);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			AssertEquals("Sending Agent type should be GTT", consol.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgentWithTariff);

			var expectedCosts = Array.Empty<AssertionCost>();

			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, Array.Empty<AssertionCharge>() }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: sendingAgent.Branch, autorateCosts: true, autorateRevenue: true);
		}

		public void TestAutorateCostAndRevenueFromJobInvoicingMenu_WhenSendingAgentIsGTTReceivingAgentNotNullAndLoginAsSendingAgent_ShouldNotReturnAnyCost()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateBranchProxy("USCHS");

			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			AssertEquals("Sending Agent type should be GTT", consol.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgentWithTariff);

			var expectedCosts = Array.Empty<AssertionCost>();

			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, Array.Empty<AssertionCharge>() }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: sendingAgent.Branch, autorateCosts: true, autorateRevenue: true);
		}

		public void TestAutorateCostAndRevenueFromJobInvoicingMenu_WhenSendingAgentIsEmptyReceivingAgentIsNullAndLoginAsSendingAgent_ShouldReturnCostingPerExistingBehavior()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;

			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: null);
			consol.JK_SendingForwarderHandlingType = "";

			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			AssertEquals("Sending Agent type should be empty", consol.JK_SendingForwarderHandlingType, string.Empty);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
				}
			};

			var expectedCharge1 = new AssertionCharge() { ChargeCode = "FRT", JR_OSCostAmt = 100m };
			var expectedCharge2 = new AssertionCharge() { ChargeCode = "BAF", JR_OSCostAmt = 200m };
			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, new [] { expectedCharge1, expectedCharge2 } }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: sendingAgent.Branch, autorateCosts: true, autorateRevenue: true);
		}

		public void TestAutorateCostAndRevenueFromJobInvoicingMenu_WhenSendingAgentIsEmptyReceivingAgentNotNullAndLoginAsSendingAgent_ShouldReturnCostingPerExistingBehavior()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";
			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateBranchProxy("USCHS");

			Helper.ChargeCodes.CreateGlobalCharge("ICTFRT");
			var shipmentChargeCode = Helper.ChargeCodes["BAF"];
			var consolChargeCode = Helper.ChargeCodes["FRT"];
			shipmentChargeCode.AC_DepartmentFilterList = "ALL";
			consolChargeCode.AC_DepartmentFilterList = "ALL";
			shipmentChargeCode.AC_IsGroupageCharge = false;

			Factory.Save();

			var costing = Helper.NewCosting(null);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, origin, "", "FRT", 100m);
			costing.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "", "AUMEL", "BAF", 200m);

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			CreateFlatRate(sendingIntercompanyTariff, "ICTFRT", 66m, origin, destination);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			consol.JK_SendingForwarderHandlingType = "";

			var shipment = CreateGatewayShipment(consol, origin, "AUMEL", true, true);

			Factory.Save();

			AssertEquals("Sending Agent type should be empty", consol.JK_SendingForwarderHandlingType, string.Empty);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
				}
			};

			var expectedCharge1 = new AssertionCharge() { ChargeCode = "FRT", JR_OSCostAmt = 100m };
			var expectedCharge2 = new AssertionCharge() { ChargeCode = "BAF", JR_OSCostAmt = 200m };
			var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
			{
				{ shipment, new [] { expectedCharge1, expectedCharge2 } }
			};

			AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: sendingAgent.Branch, autorateCosts: true, autorateRevenue: true);
		}

		public void TestAutorateCostAndRevenueFromJobInvoicingMenu_ClientRatesShouldBeLoaded_WhenLoginAsGTAWithExportShipment()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.HongKong))
			{
				Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
				CreateGlobalCharge("FRT");

				var sendingAgent = CreateBranchProxy("HKHKG");
				var receivingAgent = CreateBranchProxy("SGSIN");

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sendingAgent.branch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					sendingAgent.proxy.OH_IsCreditor = true;
					receivingAgent.proxy.OH_IsCreditor = true;

					var clientRate1 = Helper.NewClientRate(sendingAgent.proxy);
					CreateFlatRate(clientRate1, "FRT", 44m, "HK", "");

					var clientRate2 = Helper.NewClientRate(receivingAgent.proxy);
					CreateFlatRate(clientRate2, "FRT", 55m, "HK", "");
				}

				var tariff = Helper.NewIntercompanyTariff(sendingAgent.proxy);
				CreateFlatRate(tariff, "FRT", 33m, "HK", "");

				TransportProvider1.OH_IsCreditor = true;

				var consol = CreateForwardingConsolWithGatewayAgents("HKHKG", "SGSIN", sendingAgent.proxy, receivingAgent.proxy, checkIsItGateway: true);
				var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("HKHKG", "CNSZX", true, consol);
				shipment.JS_INCO = ZString.Empty;
				shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
				CreateJob(shipment, shipment.JS_UniqueConsignRef);

				Factory.Save();

				var expectedCosts = Array.Empty<AssertionCost>();
				var expectedCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>
				{
					{
						shipment,
						new[]
						{
							new AssertionCharge { ChargeCode = "FRT", JR_OSSellAmt = 55m }
						}
					},
				};

				AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride("", expectedCharges, expectedCosts, consol, branch: sendingAgent.branch, autorateCosts: true, autorateRevenue: true);
				AssertAutoratingAuditLogContains(consol, "RatingHeader Found Client Rate", "Client Rate should have been loaded.");
				AssertAutoratingAuditLogContains(consol, "FRT charge from Client Rate", "Client Rate should have been loaded.");
			}
		}

		public void TestAutorateRevenueFromGatewayInvoicingMenu_ChargesShouldBeCreatedForShipmentsIfNotPosted()
		{
			var charge = CreateGlobalCharge("FRT1");
			charge.AC_MarginPercentage = 0m;

			var sendingAgent = CreateBranchProxy("USCHS");
			var receivingAgent = CreateNonProxyBranch("AUSYD");
			sendingAgent.proxy.OH_IsDebtor = true;
			sendingAgent.proxy.OH_IsCreditor = true;

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent.proxy);
			CreateFlatRate(intercompanyTariff, "FRT1", 66m, "USCHS", "AUSYD");

			var consol = CreateForwardingConsolWithGatewayAgents("USCHS", "AUSYD", sendingAgent: sendingAgent.proxy, receivingAgent.org);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "USCHS", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment1 = CreateGatewayShipment(consol, "USCHS", "AUSYD", false);

			Factory.Save();

			var consolJob = new Job.Loader(consol).TryLoadOrCreateWithoutMutexForTestOnly();
			consolJob.JH_GB = sendingAgent.branch.PK;
			consolJob.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FES")).PK;

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 66m,
					RelatedJobNumber = shipment1.JS_UniqueConsignRef,
				}
			};

			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(Env.CurrentCompanyPK))
			{
				AutorateAndAssertWithUserContextOverride(expected, consol, consol.SendingForwarder, null, sendingAgent.branch, autorateRevenue: true, autorateCosts: false, job: consolJob);

				var revenueCharge = consolJob.Charges[0];
				revenueCharge.JR_OH_SellAccount = sendingAgent.proxy.PK;

				Factory.Save();

				AssertEquals("Revenue charge is posted", true, revenueCharge.IsRevenuePosted);

				var shipmentJob = new Job.Loader(shipment1).TryLoadOrCreate();
				var costCharge = shipmentJob.Charges[0];

				AssertEquals("cost side is posted", true, costCharge.IsCostPosted);
				AssertEquals("Rating Behaviour is STP", JobChargeLookups.StopFromAutorating, costCharge.JR_Calc_CostRatingBehavior);
				AssertEquals(revenueCharge.JR_OH_SellAccount, costCharge.JR_OH_CostAccount);
				AssertEquals(revenueCharge.JR_OSSellAmt, costCharge.JR_OSCostAmt);
			}

			var shipment2 = CreateGatewayShipment(consol, "USCHS", "AUSYD", false);
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 66m,
					RelatedJobNumber = shipment1.JS_UniqueConsignRef,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT1", // FRT1 charge should be calculated for shipment2 regardless of being posted for shipment1.
					JR_OSSellAmt = 66m,
					RelatedJobNumber = shipment2.JS_UniqueConsignRef,
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, consol.SendingForwarder, null, sendingAgent.branch, autorateRevenue: true, autorateCosts: false, job: consolJob);
		}

		#endregion

		#endregion

		#region Handling Consol Level Charges

		public void TestAutorateShipmentCost_ConsolLevelCharge_ShouldNotBeLoadedForIntercompanyTariff()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("CAF", isConsolLevel: true);
			CreateGlobalCharge("BAF");

			var (agent1, branch1) = CreateNonProxyBranch("CHBSL");
			var (agent2, _) = CreateCompanyAndBranchProxy("BEANR");
			var (agent3, _) = CreateCompanyAndBranchProxy("DEBRE");
			var (agent4, _) = CreateCompanyAndBranchProxy("USCHS");

			var chargeCode = Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			chargeCode.AC_GC = branch1.Company.PK;

			RateEntry costEntry;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;

				costEntry = Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "US", "ORG1", 50);
			}

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff2, "CAF", 110m, "CHBSL", "USHOU").TI_RateStartDate = costEntry.TI_RateStartDate;
			CreateFlatRate(tariff2, "FRT", 100m, "CHBSL", "US").TI_RateStartDate = costEntry.TI_RateStartDate;

			var tariff3 = Helper.NewIntercompanyTariff(agent3);
			CreateFlatRate(tariff3, "BAF", 120m, "CHBSL", "USHOU").TI_RateStartDate = costEntry.TI_RateStartDate;

			TransportProvider1.OH_IsCreditor = true;

			var shipment = CreateStandaloneGatewayShipment("CHBSL", "USHOU", true, agent2.PK, agent3.PK, agent4.PK);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					CostAccountCode = agent2.OH_Code
				},
				new AssertionCharge
				{
					ChargeCode = "ORG1",
					CostAccountCode = TransportProvider1.OH_Code
				}
			};

			var expectedLogNoteLine =
				$"Information: RateLine Filtered CAF-FLT-Intercompany Tariff PROXYBEANR	reason:	CAF charge code is flagged as Consol Level. Consol Level Costs don't apply to {shipment.RatingAdapter.JobID}.";

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
			AssertAutoratingAuditLogNoteContainsLinesWithUserContextOverride(shipment, branch1, expectedLogNoteLine);
		}

		public void TestAutorateShipmentCost_ConsolLevelCharge_WhenNoConsolAttachedToShipment()
		{
			CreateGlobalCharge("ICTS", isConsolLevel: false);
			CreateGlobalCharge("ICTC", isConsolLevel: true);

			var (agent1, branch1) = CreateNonProxyBranch("CHBSL");
			var (agent2, _) = CreateCompanyAndBranchProxy("BEANR");
			var (agent3, _) = CreateCompanyAndBranchProxy("DEBRE");
			var (agent4, _) = CreateCompanyAndBranchProxy("USCHS");

			var chargeCodeS = Helper.ChargeCodes.New("ORGS", "ORG Non-Consol Level", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			chargeCodeS.AC_GC = branch1.Company.PK;
			chargeCodeS.AC_IsGroupageCharge = false;
			var chargeCodeC = Helper.ChargeCodes.New("ORGC", "ORG Consol Level", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			chargeCodeC.AC_GC = branch1.Company.PK;
			chargeCodeC.AC_IsGroupageCharge = true;

			RateEntry costEntry;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;

				costEntry = Helper.NewCosting(TransportProvider1).AddRateEntry(RateCategory.ORG, RateMode.ALL, "CH", "US");
				costEntry.RateLines.RemoveAndDeleteAll();
				costEntry.AddRateLine("ORGS", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 5m;
				costEntry.AddRateLine("ORGC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 6m;
			}

			var ictEntry = Helper.NewIntercompanyTariff(agent2).AddRateEntry(RateCategory.AIR, RateMode.LSE, "CH", "US");
			ictEntry.RateLines.RemoveAndDeleteAll();
			ictEntry.AddRateLine("ICTS", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 3m;
			ictEntry.AddRateLine("ICTC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 4m;
			ictEntry.TI_RateStartDate = costEntry.TI_RateStartDate;

			TransportProvider1.OH_IsCreditor = true;

			var shipment = CreateStandaloneGatewayShipment("CHBSL", "USHOU", true, agent2.PK, agent3.PK, agent4.PK);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge { ChargeCode = "ICTS" },
				new AssertionCharge { ChargeCode = "ORGS" },
			};

			AssertEquals("Pre-Condition", shipment.Consols.Count, 0);

			// Consol Level Charge should NOT be loaded to Shipment
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		public void TestAutorateShipmentCost_ConsolLevelCharge_WhenFirstGatewayAgentInShipmentGatewayIsGTT()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Switzerland))
			{
				CreateGlobalCharge("ICTS", isConsolLevel: false);
				CreateGlobalCharge("ICTC", isConsolLevel: true);

				var agent1 = Factory.NewWithValidTestData<OrgHeader>();
				var (agent2, branch2) = CreateBranchProxy("CHBSL", GlbCompany.CurrentCompany);
				var (agent3, _) = CreateCompanyAndBranchProxy("USCHS");
				var (agent4, _) = CreateCompanyAndBranchProxy("USHOU");

				var chargeCode1 = Helper.ChargeCodes.New("ORGS", "ORG Non-Consol Level", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
				chargeCode1.AC_GC = branch2.Company.PK;
				chargeCode1.AC_IsGroupageCharge = false;
				var chargeCode2 = Helper.ChargeCodes.New("ORGC", "ORG Consol Level", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
				chargeCode2.AC_GC = branch2.Company.PK;
				chargeCode2.AC_IsGroupageCharge = true;

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					agent1.OH_IsCreditor = true;
					agent2.OH_IsCreditor = true;
					agent3.OH_IsCreditor = true;

					GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
					var costEntry = Helper.NewCosting(TransportProvider1).AddRateEntry(RateCategory.ORG, RateMode.ALL, "CH", "US");
					costEntry.RateLines.RemoveAndDeleteAll();
					costEntry.AddRateLine("ORGS", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 5m;
					costEntry.AddRateLine("ORGC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 6m;
				}

				var ictEntry = Helper.NewIntercompanyTariff(agent2).AddRateEntry(RateCategory.AIR, RateMode.LSE, "CH", "US");
				ictEntry.RateLines.RemoveAndDeleteAll();
				ictEntry.AddRateLine("ICTS", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 3m;
				ictEntry.AddRateLine("ICTC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 4m;

				TransportProvider1.OH_IsCreditor = true;

				var consol1 = CreateForwardingConsolWithGatewayAgents("CHZUR", "CHBSL", agent1, agent2, checkIsItGateway: false);
				consol1.JK_SendingForwarderHandlingType = "";
				consol1.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
				SetUpAppointedGatewayAgentPorts(consol1.ReceivingForwarderAddress, "CHBSL", AgentStatusList.Codes.GatewayAgentWithTariff);

				var consol2 = CreateForwardingConsolWithGatewayAgents("CHBSL", "USCHS", agent2, agent3, checkIsItGateway: false);
				SetUpAppointedGatewayAgentPorts(consol2.ReceivingForwarderAddress, "USCHS", AgentStatusList.Codes.GatewayAgentWithTariff);
				SetUpAppointedGatewayAgentPorts(consol2.SendingForwarderAddress, "CHBSL", AgentStatusList.Codes.GatewayAgentWithTariff);
				consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
				consol2.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

				var consol3 = CreateForwardingConsolWithGatewayAgents("USCHS", "USHOU", agent3, agent4, checkIsItGateway: false);
				SetUpAppointedGatewayAgentPorts(consol3.SendingForwarderAddress, "USCHS", AgentStatusList.Codes.GatewayAgentWithTariff);
				consol3.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
				consol3.JK_ReceivingForwarderHandlingType = "";

				var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "USHOU", false, consol1, consol2, consol3);
				shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;

				Factory.Save();

				var expected = new[]
				{
					new AssertionCharge { ChargeCode = "ICTS" },
					new AssertionCharge { ChargeCode = "ORGS" },
				};

				AssertEquals("Pre-Condition", shipment.Gateways[0].ForwarderPK, agent2.PK);
				AssertEquals("Pre-Condition", consol1.ReceivingForwarderPK, agent2.PK);
				AssertEquals("Pre-Condition", consol1.JK_ReceivingForwarderHandlingType, AgentStatusList.Codes.GatewayAgentWithTariff);

				// Consol Level Charge should NOT be loaded to Shipment
				AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch2, autorateRevenue: false);
			}
		}

		public void TestAutorateShipmentCost_ConsolLevelCharge_SingleConsol_SingleShipment_WhenFirstGatewayAgentInShipmentGatewaysIsGTA()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Switzerland))
			{
				CreateGlobalCharge("ICTS", isConsolLevel: false);
				CreateGlobalCharge("ICTC", isConsolLevel: true);

				var (agent1, branch1) = CreateBranchProxy("CHZUR");
				var (agent2, _) = CreateBranchProxy("CHBSL");
				var (agent3, branch3) = CreateBranchProxy("BEANR");

				var chargeCode1 = Helper.ChargeCodes.New("ORGS", "ORG Non-Consol Level", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
				chargeCode1.AC_GC = branch3.Company.PK;
				chargeCode1.AC_IsGroupageCharge = false;
				var chargeCode2 = Helper.ChargeCodes.New("ORGC", "ORG Consol Level", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
				chargeCode2.AC_GC = branch3.Company.PK;
				chargeCode2.AC_IsGroupageCharge = true;

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch3.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					agent1.OH_IsCreditor = true;
					agent2.OH_IsCreditor = true;
					agent3.OH_IsCreditor = true;

					var costEntry = Helper.NewCosting(TransportProvider1).AddRateEntry(RateCategory.ORG, RateMode.ALL, "CH", "US");
					costEntry.RateLines.RemoveAndDeleteAll();
					costEntry.AddRateLine("ORGS", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 5m;
					costEntry.AddRateLine("ORGC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 6m;
				}

				var ictEntry = Helper.NewIntercompanyTariff(agent3).AddRateEntry(RateCategory.AIR, RateMode.LSE, "CH", "US");
				ictEntry.RateLines.RemoveAndDeleteAll();
				ictEntry.AddRateLine("ICTS", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 3m;
				ictEntry.AddRateLine("ICTC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 4m;

				var consol = CreateForwardingConsolWithGatewayAgents("CHBSL", "BEANR", agent2, agent3, checkIsItGateway: false);
				SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, "BEANR", AgentStatusList.Codes.GatewayAgentWithTariff);
				consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
				consol.JK_ConsolMode = ContainerModes.FCL;

				TransportProvider1.OH_IsCreditor = true;

				var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "USHOU", false, consol);
				shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
				shipment.JS_PackingMode = ContainerModes.LCL;

				Factory.Save();

				AssertEquals(agent2.PK, shipment.Gateways[0].ForwarderPK);
				AssertEquals(agent3.PK, shipment.Gateways[1].ForwarderPK);

				var expected = new[]
				{
					new AssertionCharge { ChargeCode = "ICTS" },
					new AssertionCharge { ChargeCode = "ORGS" },
					new AssertionCharge { ChargeCode = "ORGC" }, // Consol Level Charge
				};

				AssertEquals("Pre-Condition", shipment.Gateways[0].ForwarderPK, agent2.PK);
				AssertEquals("Pre-Condition", consol.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);

				// Consol Level Charge can be loaded to Shipment
				AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
			}
		}

		public void TestAutorateShipmentCost_ConsolLevelCharge_SingleConsol_MultiShipments_WhenFirstGatewayAgentInShipmentGatewaysIsGTA()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Switzerland))
			{
				CreateGlobalCharge("ICTS", isConsolLevel: false);
				CreateGlobalCharge("ICTC", isConsolLevel: true);

				var (agent1, branch1) = CreateBranchProxy("CHZUR");
				var (agent2, _) = CreateBranchProxy("CHBSL");
				var (agent3, branch3) = CreateBranchProxy("BEANR");

				var chargeCode1 = Helper.ChargeCodes.New("ORGS", "ORG Non-Consol Level", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
				chargeCode1.AC_GC = branch3.Company.PK;
				chargeCode1.AC_IsGroupageCharge = false;
				var chargeCode2 = Helper.ChargeCodes.New("ORGC", "ORG Consol Level", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
				chargeCode2.AC_GC = branch3.Company.PK;
				chargeCode2.AC_IsGroupageCharge = true;

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch3.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					agent1.OH_IsCreditor = true;
					agent2.OH_IsCreditor = true;
					agent3.OH_IsCreditor = true;

					var costEntry = Helper.NewCosting(TransportProvider1).AddRateEntry(RateCategory.ORG, RateMode.ALL, "CH", "US");
					costEntry.RateLines.RemoveAndDeleteAll();
					costEntry.AddRateLine("ORGS", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 5m;
					costEntry.AddRateLine("ORGC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 6m;
				}

				var ictEntry = Helper.NewIntercompanyTariff(agent3).AddRateEntry(RateCategory.AIR, RateMode.LSE, "CH", "US");
				ictEntry.RateLines.RemoveAndDeleteAll();
				ictEntry.AddRateLine("ICTS", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 3m;
				ictEntry.AddRateLine("ICTC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 4m;

				var consol = CreateForwardingConsolWithGatewayAgents("CHBSL", "BEANR", agent2, agent3, checkIsItGateway: false);
				SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "CHBSL", AgentStatusList.Codes.GatewayAgent);
				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, "BEANR", AgentStatusList.Codes.GatewayAgentWithTariff);
				consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
				consol.JK_ConsolMode = ContainerModes.FCL;

				TransportProvider1.OH_IsCreditor = true;

				var shipment1 = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "USHOU", false, consol);
				shipment1.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
				shipment1.JS_PackingMode = ContainerModes.LCL;

				var shipment2 = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "USHOU", false, consol);
				shipment2.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
				shipment2.JS_PackingMode = ContainerModes.LCL;

				Factory.Save();

				AssertEquals("Assumption: Single-Consol", shipment1.Consols.Count, 1);
				AssertGreaterThan("Assumption: Multi-Shipment", consol.Shipments.Count, 1);
				CombineAssertions("Assumption: First Gateway Agent in shipment's Gateways is GTA", () =>
				{
					AssertEquals(shipment1.Gateways[0].ForwarderPK, agent2.PK);
					AssertEquals(consol.SendingForwarderPK, agent2.PK);
					AssertEquals(consol.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
				});

				var expected = new[]
				{
					new AssertionCharge { ChargeCode = "ICTS" },
					new AssertionCharge { ChargeCode = "ORGS" },
				};

				// Consol Level Charge should NOT be loaded to Shipment
				AutorateAndAssertWithUserContextOverride(expected, shipment1, Consignor, branch: branch1, autorateRevenue: false);
			}
		}

		public void TestAutorateShipmentCost_ConsolLevelCharge_MultiConsol_SingleShipments_WhenFirstGatewayAgentInShipmentGatewaysIsGTA()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Switzerland))
			{
				CreateGlobalCharge("ICTS", isConsolLevel: false);
				CreateGlobalCharge("ICTC", isConsolLevel: true);

				var (agent1, branch1) = CreateBranchProxy("CHZUR");
				var (agent2, _) = CreateBranchProxy("CHBSL");
				var (agent3, branch3) = CreateBranchProxy("BEANR");
				var (agent4, _) = CreateBranchProxy("HKHKG");

				var chargeCode1 = Helper.ChargeCodes.New("ORGS", "ORG Non-Consol Level", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
				chargeCode1.AC_GC = branch3.Company.PK;
				chargeCode1.AC_IsGroupageCharge = false;
				var chargeCode2 = Helper.ChargeCodes.New("ORGC", "ORG Consol Level", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
				chargeCode2.AC_GC = branch3.Company.PK;
				chargeCode2.AC_IsGroupageCharge = true;

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch3.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					agent1.OH_IsCreditor = true;
					agent2.OH_IsCreditor = true;
					agent3.OH_IsCreditor = true;

					var costEntry = Helper.NewCosting(TransportProvider1).AddRateEntry(RateCategory.ORG, RateMode.ALL, "CH", "US");
					costEntry.RateLines.RemoveAndDeleteAll();
					costEntry.AddRateLine("ORGS", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 5m;
					costEntry.AddRateLine("ORGC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 6m;
				}

				var ictEntry = Helper.NewIntercompanyTariff(agent3).AddRateEntry(RateCategory.AIR, RateMode.LSE, "CH", "US");
				ictEntry.RateLines.RemoveAndDeleteAll();
				ictEntry.AddRateLine("ICTS", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 3m;
				ictEntry.AddRateLine("ICTC", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 4m;

				var consol1 = CreateForwardingConsolWithGatewayAgents("CHBSL", "BEANR", agent2, agent3, checkIsItGateway: false);
				SetUpAppointedGatewayAgentPorts(consol1.SendingForwarderAddress, "CHBSL", AgentStatusList.Codes.GatewayAgent);
				consol1.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				SetUpAppointedGatewayAgentPorts(consol1.ReceivingForwarderAddress, "BEANR", AgentStatusList.Codes.GatewayAgentWithTariff);
				consol1.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
				consol1.JK_ConsolMode = ContainerModes.FCL;

				var consol2 = CreateForwardingConsolWithGatewayAgents("BEANR", "HKHKG", agent3, agent4, checkIsItGateway: false);
				SetUpAppointedGatewayAgentPorts(consol2.SendingForwarderAddress, "BEANR", AgentStatusList.Codes.GatewayAgentWithTariff);
				consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				consol2.JK_ReceivingForwarderHandlingType = "";
				consol2.JK_ConsolMode = ContainerModes.FCL;

				TransportProvider1.OH_IsCreditor = true;

				var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "USHOU", false, consol1, consol2);
				shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
				shipment.JS_PackingMode = ContainerModes.LCL;

				Factory.Save();

				AssertGreaterThan("Assumption: Multi-Consol", shipment.Consols.Count, 1);
				CombineAssertions("Assumption: Single-Shipment", () =>
				{
					AssertEquals(consol1.Shipments.Count, 1);
					AssertEquals(consol2.Shipments.Count, 1);
				});
				CombineAssertions("Assumption: First Gateway Agent in shipment's gateways is GTA", () =>
				{
					AssertEquals(shipment.Gateways[0].ForwarderPK, agent2.PK);
					AssertEquals(consol1.SendingForwarderPK, agent2.PK);
					AssertEquals(consol1.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
				});

				var expected = new[]
				{
					new AssertionCharge { ChargeCode = "ICTS" },
					new AssertionCharge { ChargeCode = "ORGS" },
				};

				// Consol Level Charge should NOT be loaded to Shipment
				AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
			}
		}

		#endregion

		#region Test GatewaySell and Negotiated Costs

		[TestDate(2019, 12, 05)]
		public void TestAutorateRevenue_MultipleShipment()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("BAF");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("CAF");

			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("BAF");
			Helper.ChargeCodes.CreateGlobalCharge("CAF");

			var intercompanyTariff = Helper.NewIntercompanyTariff(GlbCompany.CurrentCompany.OrgProxy);
			CreateFlatRate(intercompanyTariff, "FRT", 33m, "NZAKL", "AUSYD");
			CreateFlatRate(intercompanyTariff, "BAF", 66m, "", "AUMEL");
			CreateFlatRate(intercompanyTariff, "CAF", 99m, "NZCHC", "");

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: GlbCompany.CurrentCompany.OrgProxy, receivingAgent: null);
			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", false);
			var shipment1 = CreateGatewayShipment(consol, "", "AUMEL", false);
			var shipment2 = CreateGatewayShipment(consol, "NZCHC", "", false);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 33m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 66m,
					RelatedJobNumber = shipment1.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "CAF",
					JR_OSSellAmt = 99m,
					RelatedJobNumber = shipment2.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, GlbCompany.CurrentCompany.OrgProxy, autorateRevenue: true, autorateCosts: false);
		}

		[TestDate(2019, 01, 01)]
		public void TestAutorateRevenueAndCosts_FreightAutorateStandardModeWithGatewaySell_UsingIntercompanyTariff()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("BAF");

			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("BAF");
			Factory.Save();

			var origin = "AUBNE";
			var destination = "USLAX";
			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var rateEntry = intercompanyTariff.AddRateEntry(RateCategory.AIR, RateMode.LSE, origin, destination);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 20m;

			var rateLine2 = rateEntry.AddRateLine("BAF", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 20m;

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: null);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, destination, false, false);
			shipment.JS_FreightGatewaySellRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_PackingMode = ContainerModes.Loose;
			shipment.JS_ActualWeight = 20;
			shipment.JS_ActualVolume = 2;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 6666.66m,
					JR_OSCostAmt = 6666.66m,
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 20.00m,
					JR_OSCostAmt = 20.00m,
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, sendingAgent);
		}

		[TestDate(2019, 01, 01)]
		public void TestAutorateRevenue_FreightAutorateAllInclusiveModeWithGatewaySell_UsingIntercompanyTariff()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("BAF");

			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("BAF");

			var intercompanyTariff = Helper.NewIntercompanyTariff(GlbCompany.CurrentCompany.OrgProxy);
			var rateEntry = intercompanyTariff.AddRateEntry(RateCategory.AIR, RateMode.LSE, "NZAKL", "AUSYD");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("FRT", FlatCalculator.Code);
			rateLine1.GetCalculator<FlatCalculator>().BaseRate = 56m;

			var rateLine2 = rateEntry.AddRateLine("BAF", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 68m;

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: GlbCompany.CurrentCompany.OrgProxy, receivingAgent: null);
			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", false);
			shipment.JS_FreightGatewaySellRateAutoratingMode = FreightRateAutoratingModes.Code.AllInRate;
			shipment.JS_GatewayFreightSellRate = 50m;
			shipment.JS_ActualWeight = 20;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 1000m
				}
			};
			var expectedLogLines = new[] { @"Information: RatingHeader Found Intercompany Tariff EDICUS Entries: 1",
"Information: RateLine Found FRT-Job Gateway Sell" };

			// Only Gateway sell should be found
			AutorateAndAssertWithUserContextOverride(expected, consol, GlbCompany.CurrentCompany.OrgProxy, autorateCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(consol, "Log should contain expected lines", expectedLogLines);
		}

		[TestDate(2019, 01, 01)]
		public void TestAutorateRevenue_FreightAutorateFreightPlusModeWithGatewaySell_UsingIntercompanyTariff()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("BAF");

			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("BAF");

			var intercompanyTariff = Helper.NewIntercompanyTariff(GlbCompany.CurrentCompany.OrgProxy);
			var rateEntry = intercompanyTariff.AddRateEntry(RateCategory.AIR, RateMode.LSE, "NZAKL", "AUSYD");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("FRT", FlatCalculator.Code);
			rateLine1.GetCalculator<FlatCalculator>().BaseRate = 99m;

			var rateLine2 = rateEntry.AddRateLine("BAF", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 88m;

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: GlbCompany.CurrentCompany.OrgProxy, receivingAgent: null);
			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", false);
			shipment.JS_FreightGatewaySellRateAutoratingMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			shipment.JS_ActualWeight = 20;
			shipment.JS_GatewayFreightSellRate = 10m;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 200m
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 88m
				},
			};
			var expectedLogLines = new[] { @"Information: RateLine Found FRT-Job Gateway Sell",
"Information: RateLine Found FRT-FLT-Intercompany Tariff EDICUS",
"Information: RateLine Filtered FRT-FLT-Intercompany Tariff EDICUS	reason:	replaced by Job Negotiated Cost/Gateway Sell/One Off Freight Rate" };

			AutorateAndAssertWithUserContextOverride(expected, consol, GlbCompany.CurrentCompany.OrgProxy);
			AssertAutoratingAuditLogNoteContainsLines(consol, "Log should contain expected lines", expectedLogLines);
		}

		[TestDate(2019, 12, 05)]
		public void TestAutorateShipmentCost_Export_LoginAsNonGateway_NegotiatedCostShouldNOTFilterIntercompanyTariff()
		{
			CreateGlobalCharge("FRT");

			var (agent1, branch1) = CreateNonProxyBranch("CHBSL");
			var (agent2, _) = CreateCompanyAndBranchProxy("BEANR");

			var chargeCode = Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			chargeCode.AC_GC = branch1.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				Env.Registry.FreightChargeCode = Helper.ChargeCodes["FRT"].PK.ToGuid();
				agent2.OH_IsCreditor = true;
				Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "US", "ORG1", 50);
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent2);
			CreateFlatRate(tariff1, "FRT", 100m, "CHBSL", "USHOU");

			TransportProvider1.OH_IsCreditor = true;

			var shipment = CreateStandaloneGatewayShipment("CHBSL", "USHOU", true, agent2.PK);
			shipment.JS_FreightCostRateAutoratingMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			shipment.JS_FreightCostRate = 50;
			shipment.JS_RX_NKFreightCostRateCurrency = "USD";
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ORG1"
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 100m
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 50000m,
					JR_RX_NKCostCurrency = "USD"
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		[TestDate(2019, 12, 05)]
		public void TestAutorateShipmentCost_Export_LoginAsNonGateway_ContainerNegotiatedCostShouldNOTFilterIntercompanyTariff()
		{
			CreateGlobalCharge("FRT");

			var (_, branch1) = CreateNonProxyBranch("CHBSL");
			var (agent2, _) = CreateCompanyAndBranchProxy("BEANR");
			var (agent3, _) = CreateCompanyAndBranchProxy("DEBRE");

			var chargeCode = Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			chargeCode.AC_GC = branch1.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				Env.Registry.FreightChargeCode = Helper.ChargeCodes["FRT"].PK.ToGuid();
				agent2.OH_IsCreditor = true;
				Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "US", "ORG1", 50);
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent2);
			var entry = CreateFlatRate(tariff1, "FRT", 800m, "CHBSL", "USHOU");
			entry.TI_Mode = RateMode.ULD;
			entry.TI_RC = Helper.Containers["LD-7"].PK;

			TransportProvider1.OH_IsCreditor = true;

			var shipment = CreateStandaloneGatewayShipment("CHBSL", "USHOU", true, agent2.PK, agent3.PK);
			shipment.JS_PackingMode = ContainerModes.ULD;

			var consol = CreateForwardingConsol(TransportModes.Air, "CHBSL", "USHOU", TransportProvider1, shipment);
			consol.CreditorPK = TransportProvider1.PK;

			var container = consol.Containers.AddNew();
			container.JC_RC = Helper.Containers["LD-7"].PK;
			container.JC_ContainerCount = 5;
			container.JC_CostSpotRate = 30m;
			container.JC_CostSpotRateMode = FreightRateAutoratingModes.Code.FreightPlusRate;

			shipment.OuterPackLines.RemoveAndDeleteAll();
			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = 10;
			packline.JL_JC = container.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ORG1"
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 150m,
					CostCalculationDescription = "FRT: 5 LD-7 Container(s) @ CHF 30.00/Container",
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 800m,
					CostCalculationDescription = "FRT: Base Rate AUD 800.00",
				},
			};

			var expectedLogLines = new[] { @"RateLine Found FRT-FLT-LD-7-Intercompany Tariff PROXYBEANR",
"Information: RateLine Found FRT-Container Negotiated Cost" };
			var notExpectedLogLine =
"Information: RateLine Filtered FRT-FLT-LD-7-Intercompany Tariff PROXYBEANR	reason:	failed similarity check";

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
			AssertAutoratingAuditLogNoteContainsLinesWithUserContextOverride(shipment, branch: branch1, expectedLogLines);
			AssertAutoratingAuditLogNoteNOTContainsLinesWithUserContextOverride(shipment, branch: branch1, notExpectedLogLine);
		}

		[TestDate(2022, 01, 01)]
		public void TestAutorateCostAndRevenueFromGatewayInvoicingMenu_ContainerNegotiatedCostShouldOnlyBeLoadedForConsol()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;

			var tariff = Helper.NewIntercompanyTariff(sendingAgent);
			tariff.AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.ULD, "AUMEL", "NZAKL", "FRT", 55m, container: "LD-7");

			var consol = CreateForwardingConsolWithGatewayAgents("AUSYD", "NZAKL", sendingAgent, null, false);
			consol.JK_ConsolMode = ContainerModes.ULD;
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "AUSYD", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol.JK_ReceivingForwarderHandlingType = "";

			var container = consol.Containers.AddNew();
			container.JC_RC = Helper.Containers["LD-7"].PK;
			container.JC_ContainerCount = 5;
			container.JC_CostSpotRate = 30m;
			container.JC_CostSpotRateMode = FreightRateAutoratingModes.Code.FreightPlusRate;

			var shipment = CreateGatewayShipment(consol, "AUMEL", "NZAKL", false, true, consignee: Consignor);
			shipment.OuterPackLines.RemoveAndDeleteAll();
			shipment.OuterPackLines.AddNew().JL_JC = container.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 55m,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 150m
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, consol.SendingForwarder, autorateCosts: true, autorateRevenue: true);
		}

		[TestDate(2019, 12, 05)]
		public void TestAutorateShipmentCost_Export_LoginAsNonGateway_GatewaySellCostShouldNotOverrideIntercompanyTariff_WhenGatewaySellIsSTD()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("BAF");
			CreateGlobalCharge("CAF");
			CreateGlobalCharge("ODOC", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);

			var (agent1, branch1) = CreateNonProxyBranch("CHBSL");
			var (agent2, _) = CreateCompanyAndBranchProxy("BEANR");
			var (agent3, _) = CreateCompanyAndBranchProxy("DEBRE");
			var (agent4, _) = CreateCompanyAndBranchProxy("USCHS");

			var chargeCode = Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			chargeCode.AC_GC = branch1.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;

				var costing = Helper.NewCosting(TransportProvider1);
				costing.AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "US", "ORG1", 50);
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent1);
			var rateEntry = tariff1.AddRateEntry(RateCategory.AIR, RateMode.LSE, "CHBSL", "USHOU");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 14m;
			var rateLine2 = rateEntry.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.KG);
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 15m;

			var rateEntry3 = tariff1.AddRateEntry(RateCategory.ORG, RateMode.LSE, "CHBSL", "USHOU");
			var rateLine3 = rateEntry3.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.KG);
			rateLine3.GetCalculator<UnitCalculator>().PerUnit = 17m;

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			var rateEntry2 = tariff2.AddRateEntry(RateCategory.AIR, RateMode.LSE, "CHBSL", "USHOU");
			rateEntry2.RateLines.RemoveAndDeleteAll();
			var rateLine4 = rateEntry2.AddRateLine("CAF", UnitCalculator.Code, QuantityUnit.KG);
			rateLine4.GetCalculator<UnitCalculator>().PerUnit = 16m;

			TransportProvider1.OH_IsCreditor = true;

			var shipment = CreateStandaloneGatewayShipment("CHBSL", "USHOU", true, agent2.PK, agent3.PK, agent4.PK);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			shipment.JS_FreightCostRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_ActualWeight = 150m;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ORG1",
					CostAccountCode = TransportProvider1.OH_Code
				},
				new AssertionCharge
				{
					ChargeCode = "CAF",
					CostAccountCode = agent2.OH_Code,
					JR_OSSellAmt = 2400m, // 150 * 16
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		[TestDate(2019, 12, 05)]
		public void TestAutorateShipmentCost_GatewaySellCostShouldOverrideOnlyIntercompanyTariffFRTChargeCode_WhenGatewaySellIsFRT()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("BAF");
			CreateGlobalCharge("ODOC", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);

			var (agent1, branch1) = CreateNonProxyBranch("CHBSL");
			var (agent2, _) = CreateCompanyAndBranchProxy("BEANR");
			var (agent3, _) = CreateCompanyAndBranchProxy("DEBRE");
			var (agent4, _) = CreateCompanyAndBranchProxy("USCHS");

			var chargeCode = Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			chargeCode.AC_GC = branch1.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;

				Env.Registry.FreightChargeCode = Helper.ChargeCodes["FRT"].PK.ToGuid();

				var costing = Helper.NewCosting(TransportProvider1);
				costing.AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "US", "ORG1", 50);
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent2);
			var rateEntry = tariff1.AddRateEntry(RateCategory.AIR, RateMode.LSE, "CHBSL", "USHOU");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 14m;
			var rateLine2 = rateEntry.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.KG);
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 15m;

			var rateEntry1 = tariff1.AddRateEntry(RateCategory.ORG, RateMode.LSE, "CHBSL", "USHOU");
			var rateLine3 = rateEntry1.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.KG);
			rateLine3.GetCalculator<UnitCalculator>().PerUnit = 17m;

			TransportProvider1.OH_IsCreditor = true;

			var shipment = CreateStandaloneGatewayShipment("CHBSL", "USHOU", true, agent2.PK, agent3.PK, agent4.PK);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;

			shipment.JS_FreightGatewaySellRateAutoratingMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			shipment.JS_GatewayFreightSellRate = 25m;
			shipment.JS_RX_NKGatewayFreightSellRateCurrency = "AUD";

			shipment.JS_FreightCostRateAutoratingMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			shipment.JS_FreightCostRate = 45m;
			shipment.JS_RX_NKFreightCostRateCurrency = "AUD"; //Gateway Sell takes priority over cost

			shipment.JS_ActualWeight = 150m;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ORG1",
					CostAccountCode = TransportProvider1.OH_Code
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 3750m, // 150 * 25 instead of 150 * 14
				},
				new AssertionCharge
				{
					ChargeCode = "BAF",
					CostAccountCode = agent2.OH_Code,
					JR_OSSellAmt = 2250m, // 150 * 15
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					CostAccountCode = agent2.OH_Code,
					JR_OSSellAmt = 2550m, // 150 * 17
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		[TestDate(2019, 12, 05)]
		public void TestAutorateShipmentCost_GatewaySellCostShouldAddFRTChargeCode_WhenGatewaySellIsFRT()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("BAF");
			CreateGlobalCharge("ODOC", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);

			var (agent1, branch1) = CreateNonProxyBranch("CHBSL");
			var (agent2, _) = CreateCompanyAndBranchProxy("BEANR");
			var (agent3, _) = CreateCompanyAndBranchProxy("DEBRE");
			var (agent4, _) = CreateCompanyAndBranchProxy("USCHS");

			var chargeCode = Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			chargeCode.AC_GC = branch1.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;

				Env.Registry.FreightChargeCode = Helper.ChargeCodes["FRT"].PK.ToGuid();

				var costing = Helper.NewCosting(TransportProvider1);
				costing.AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "US", "ORG1", 50);
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent2);
			var rateEntry = tariff1.AddRateEntry(RateCategory.ORG, RateMode.LSE, "CHBSL", "USHOU");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine3 = rateEntry.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.KG);
			rateLine3.GetCalculator<UnitCalculator>().PerUnit = 17m;

			TransportProvider1.OH_IsCreditor = true;

			var shipment = CreateStandaloneGatewayShipment("CHBSL", "USHOU", true, agent2.PK, agent3.PK, agent4.PK);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			shipment.JS_FreightGatewaySellRateAutoratingMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			shipment.JS_GatewayFreightSellRate = 25m;
			shipment.JS_RX_NKGatewayFreightSellRateCurrency = "AUD";
			shipment.JS_ActualWeight = 150m;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ORG1",
					CostAccountCode = TransportProvider1.OH_Code
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 3750m, // 150 * 25 instead of 150 * 14
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					CostAccountCode = agent2.OH_Code,
					JR_OSSellAmt = 2550m, // 150 * 17
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		[TestDate(2019, 12, 05)]
		public void TestAutorateShipmentCost_GatewaySellCostShouldOverrideIntercompanyTariffFRTChargeGroup_WhenGatewaySellIsAIN()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("BAF");
			CreateGlobalCharge("ODOC", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);

			var (agent1, branch1) = CreateCompanyAndBranchProxy("CHBSL");
			var (agent2, _) = CreateCompanyAndBranchProxy("BEANR");
			var (agent3, _) = CreateCompanyAndBranchProxy("DEBRE");
			var (agent4, _) = CreateCompanyAndBranchProxy("USCHS");

			var chargeCode = Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			chargeCode.AC_GC = branch1.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;

				Env.Registry.FreightChargeCode = Helper.ChargeCodes["FRT"].PK.ToGuid();

				var costing = Helper.NewCosting(TransportProvider1);
				costing.AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "US", "ORG1", 50);
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent2);
			var rateEntry = tariff1.AddRateEntry(RateCategory.AIR, RateMode.LSE, "CHBSL", "USHOU");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 14m;
			var rateLine2 = rateEntry.AddRateLine("BAF", UnitCalculator.Code, QuantityUnit.KG);
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 15m;

			var rateEntry1 = tariff1.AddRateEntry(RateCategory.ORG, RateMode.LSE, "CHBSL", "USHOU");
			var rateLine3 = rateEntry1.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.KG);
			rateLine3.GetCalculator<UnitCalculator>().PerUnit = 17m;

			TransportProvider1.OH_IsCreditor = true;

			var shipment = CreateStandaloneGatewayShipment("CHBSL", "USHOU", true, agent2.PK, agent3.PK, agent4.PK);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			shipment.JS_FreightGatewaySellRateAutoratingMode = FreightRateAutoratingModes.Code.AllInRate;
			shipment.JS_GatewayFreightSellRate = 25m;
			shipment.JS_RX_NKGatewayFreightSellRateCurrency = "AUD";
			shipment.JS_ActualWeight = 150m;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ORG1",
					CostAccountCode = TransportProvider1.OH_Code
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 3750m, // 150 * 25 instead of 150 * 14
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					CostAccountCode = agent2.OH_Code,
					JR_OSSellAmt = 2550m, // 150 * 17
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		[TestDate(2019, 12, 05)]
		public void TestAutorateShipmentCost_GatewaySellCostShouldAddFRTChargeCode_WhenGatewaySellIsAIN()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("BAF");
			CreateGlobalCharge("ODOC", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);

			var (agent1, branch1) = CreateCompanyAndBranchProxy("CHBSL");
			var (agent2, _) = CreateCompanyAndBranchProxy("BEANR");
			var (agent3, _) = CreateCompanyAndBranchProxy("DEBRE");
			var (agent4, _) = CreateCompanyAndBranchProxy("USCHS");

			var chargeCode = Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			chargeCode.AC_GC = branch1.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;

				Env.Registry.FreightChargeCode = Helper.ChargeCodes["FRT"].PK.ToGuid();

				var costing = Helper.NewCosting(TransportProvider1);
				costing.AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "US", "ORG1", 50);
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent2);
			var rateEntry = tariff1.AddRateEntry(RateCategory.ORG, RateMode.LSE, "CHBSL", "USHOU");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.KG);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 17m;

			TransportProvider1.OH_IsCreditor = true;

			var shipment = CreateStandaloneGatewayShipment("CHBSL", "USHOU", true, agent2.PK, agent3.PK, agent4.PK);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			shipment.JS_FreightGatewaySellRateAutoratingMode = FreightRateAutoratingModes.Code.AllInRate;
			shipment.JS_GatewayFreightSellRate = 25m;
			shipment.JS_RX_NKGatewayFreightSellRateCurrency = "AUD";
			shipment.JS_ActualWeight = 150m;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ORG1",
					CostAccountCode = TransportProvider1.OH_Code
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 3750m, // 150 * 25
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					CostAccountCode = agent2.OH_Code,
					JR_OSSellAmt = 2550m, // 150 * 17
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAutorateRevenueForConsol_ShouldLoadIntercompanyTarrif_WhenGatewaySellIsSTD()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			CreateGlobalCharge("FRT1");

			var (agent1, branch1) = CreateCompanyAndBranchProxy("CHZUR");
			var (agent2, branch2) = CreateCompanyAndBranchProxy("CHBSL");
			var (agent3, _) = CreateCompanyAndBranchProxy("BEANR");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;

				Env.Registry.FreightChargeCode = Helper.ChargeCodes["FRT"].PK.ToGuid();
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent1);
			var rateEntry = tariff1.AddRateEntry(RateCategory.AIR, RateMode.LSE, "CHZUR", "");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 10m;
			var rateLine2 = rateEntry.AddRateLine("FRT1", UnitCalculator.Code, QuantityUnit.KG);
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 11m;

			var consol1 = CreateForwardingConsolWithGatewayAgents("CHZUR", "CHBSL", agent1, agent2, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol1.SendingForwarderAddress, "CHZUR", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol1.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consol2 = CreateForwardingConsolWithGatewayAgents("CHBSL", "BEANR", agent2, agent3, checkIsItGateway: false);

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "USHOU", false, consol1, consol2);
			shipment.JS_FreightGatewaySellRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_RX_NKGatewayFreightSellRateCurrency = "AUD";
			shipment.JS_ActualWeight = 150m;

			AssertNotNull("Data setup: Create shipment job with mutex", new JobHeader.Loader(shipment).TryCreateWithMutex(branch1));

			Factory.Save();

			CombineAssertions("First gateway should not equal to Consol 2 sending agent", () =>
			{
				AssertEquals("Shipment's gateways should contain Consol 1 sending agent", consol1.SendingForwarderPK, shipment.Gateways[0].Forwarder.PK);
				AssertEquals("Sending Agent type of Consol 1 should be GTT", consol1.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgentWithTariff);
				AssertEquals("Precondition: First Gateway", (byte)1, shipment.Gateways[0].JSG_Sequence);

				AssertEquals("Shipment's gateways should contain Consol 2 sending agent", consol2.SendingForwarderPK, shipment.Gateways[1].Forwarder.PK);
				AssertEquals("Sending Agent type of Consol 2 should be GTA", consol2.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
				AssertEquals("Precondition: Second Gateway", (byte)2, shipment.Gateways[1].JSG_Sequence);
			});

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 1500m, // 150 * 10
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 1650m,  // 150 * 11
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol1, consol1.SendingForwarder, null, branch1, autorateRevenue: true, autorateCosts: false);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAutorateRevenueForConsol_ShouldOvrrideIntercompanyTarrifFRTChargeCode_ForOnlyFirstGateway_WhenGatewaySellIsFRT()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			CreateGlobalCharge("FRT1");
			CreateGlobalCharge("ODOC", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);

			var (agent1, branch1) = CreateCompanyAndBranchProxy("CHZUR");
			var (agent2, _) = CreateCompanyAndBranchProxy("CHBSL");
			var (agent3, _) = CreateCompanyAndBranchProxy("BEANR");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;

				Env.Registry.FreightChargeCode = Helper.ChargeCodes["FRT"].PK.ToGuid();
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent1);
			var rateEntry = tariff1.AddRateEntry(RateCategory.AIR, RateMode.LSE, "CHZUR", "");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 10m;
			var rateLine2 = rateEntry.AddRateLine("FRT1", UnitCalculator.Code, QuantityUnit.KG);
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 11m;

			var rateEntry1 = tariff1.AddRateEntry(RateCategory.ORG, RateMode.LSE, "CHZUR", "");
			var rateLine3 = rateEntry1.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.KG);
			rateLine3.GetCalculator<UnitCalculator>().PerUnit = 17m;

			var consol1 = CreateForwardingConsolWithGatewayAgents("CHZUR", "CHBSL", agent1, agent2, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol1.SendingForwarderAddress, "CHZUR", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol1.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consol2 = CreateForwardingConsolWithGatewayAgents("CHBSL", "BEANR", agent2, agent3, checkIsItGateway: false);

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "USHOU", false, consol1, consol2);
			shipment.JS_FreightGatewaySellRateAutoratingMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			shipment.JS_GatewayFreightSellRate = 25m;
			shipment.JS_RX_NKGatewayFreightSellRateCurrency = "AUD";
			shipment.JS_ActualWeight = 150m;

			AssertNotNull("Data setup: Create shipment job with mutex", new JobHeader.Loader(shipment).TryCreateWithMutex(branch1));

			Factory.Save();

			CombineAssertions("First gateway should not equal to Consol 2 sending agent", () =>
			{
				AssertEquals("Shipment's gateways should contain Consol 1 sending agent", consol1.SendingForwarderPK, shipment.Gateways[0].Forwarder.PK);
				AssertEquals("Sending Agent type of Consol 1 should be GTT", consol1.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgentWithTariff);
				AssertEquals("Precondition: First Gateway", (byte)1, shipment.Gateways[0].JSG_Sequence);

				AssertEquals("Shipment's gateways should contain Consol 2 sending agent", consol2.SendingForwarderPK, shipment.Gateways[1].Forwarder.PK);
				AssertEquals("Sending Agent type of Consol 2 should be GTA", consol2.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
				AssertEquals("Precondition: Second Gateway", (byte)2, shipment.Gateways[1].JSG_Sequence);
			});

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 3750m, // 150 * 25 instead of 150 * 10
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 1650m,  // 150 * 11 (Should not use 25 per unit for FRT1 freight charge code group)
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 2550m,  // 150 * 17 (Should not use 25 per unit for WAR)
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol1, consol1.SendingForwarder, null, branch1, autorateRevenue: true, autorateCosts: false);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAutorateRevenueForConsol_ShouldOvrrideIntercompanyTarrifFRTChargeGroup_ForOnlyFirstGateway_WhenGatewaySellIsAIN()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			CreateGlobalCharge("FRT1");
			CreateGlobalCharge("ODOC", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);

			var (agent1, branch1) = CreateCompanyAndBranchProxy("CHZUR");
			var (agent2, _) = CreateCompanyAndBranchProxy("CHBSL");
			var (agent3, _) = CreateCompanyAndBranchProxy("BEANR");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;

				Env.Registry.FreightChargeCode = Helper.ChargeCodes["FRT"].PK.ToGuid();
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent1);
			var rateEntry = tariff1.AddRateEntry(RateCategory.AIR, RateMode.LSE, "CHZUR", "");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 10m;
			var rateLine2 = rateEntry.AddRateLine("FRT1", UnitCalculator.Code, QuantityUnit.KG);
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 11m;

			var rateEntry1 = tariff1.AddRateEntry(RateCategory.ORG, RateMode.LSE, "CHZUR", "");
			var rateLine3 = rateEntry1.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.KG);
			rateLine3.GetCalculator<UnitCalculator>().PerUnit = 17m;

			var consol1 = CreateForwardingConsolWithGatewayAgents("CHZUR", "CHBSL", agent1, agent2, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol1.SendingForwarderAddress, "CHZUR", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol1.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consol2 = CreateForwardingConsolWithGatewayAgents("CHBSL", "BEANR", agent2, agent3, checkIsItGateway: false);

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "USHOU", false, consol1, consol2);
			shipment.JS_FreightGatewaySellRateAutoratingMode = FreightRateAutoratingModes.Code.AllInRate;
			shipment.JS_GatewayFreightSellRate = 25m;
			shipment.JS_RX_NKGatewayFreightSellRateCurrency = "AUD";
			shipment.JS_ActualWeight = 150m;

			AssertNotNull("Data setup: Create shipment job with mutex", new JobHeader.Loader(shipment).TryCreateWithMutex(branch1));

			Factory.Save();

			CombineAssertions("First gateway should not equal to Consol 2 sending agent", () =>
			{
				AssertEquals("Shipment's gateways should contain Consol 1 sending agent", consol1.SendingForwarderPK, shipment.Gateways[0].Forwarder.PK);
				AssertEquals("Sending Agent type of Consol 1 should be GTT", consol1.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgentWithTariff);
				AssertEquals("Precondition: First Gateway", (byte)1, shipment.Gateways[0].JSG_Sequence);

				AssertEquals("Shipment's gateways should contain Consol 2 sending agent", consol2.SendingForwarderPK, shipment.Gateways[1].Forwarder.PK);
				AssertEquals("Sending Agent type of Consol 2 should be GTA", consol2.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
				AssertEquals("Precondition: Second Gateway", (byte)2, shipment.Gateways[1].JSG_Sequence);
			});

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 3750m, // 150 * 25 instead of 150 * 10
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 2550m,  // 150 * 17 (Should not use 25 per unit for WAR)
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol1, consol1.SendingForwarder, null, branch1, autorateRevenue: true, autorateCosts: false);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAutorateRevenueForConsol_ShouldAddFRTChargeCode_ForOnlyFirstGateway_WhenGatewaySellIsFRT()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			CreateGlobalCharge("FRT1");
			CreateGlobalCharge("ODOC", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);

			var (agent1, branch1) = CreateCompanyAndBranchProxy("CHZUR");
			var (agent2, _) = CreateCompanyAndBranchProxy("CHBSL");
			var (agent3, _) = CreateCompanyAndBranchProxy("BEANR");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;

				Env.Registry.FreightChargeCode = Helper.ChargeCodes["FRT"].PK.ToGuid();
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent1);
			var rateEntry = tariff1.AddRateEntry(RateCategory.ORG, RateMode.LSE, "CHZUR", "");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.KG);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 17m;

			var consol1 = CreateForwardingConsolWithGatewayAgents("CHZUR", "CHBSL", agent1, agent2, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol1.SendingForwarderAddress, "CHZUR", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol1.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consol2 = CreateForwardingConsolWithGatewayAgents("CHBSL", "BEANR", agent2, agent3, checkIsItGateway: false);

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "USHOU", false, consol1, consol2);
			shipment.JS_FreightGatewaySellRateAutoratingMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			shipment.JS_GatewayFreightSellRate = 25m;
			shipment.JS_RX_NKGatewayFreightSellRateCurrency = "AUD";
			shipment.JS_ActualWeight = 150m;

			AssertNotNull("Data setup: Create shipment job with mutex", new JobHeader.Loader(shipment).TryCreateWithMutex(branch1));

			Factory.Save();

			CombineAssertions("First gateway should not equal to Consol 2 sending agent", () =>
			{
				AssertEquals("Shipment's gateways should contain Consol 1 sending agent", consol1.SendingForwarderPK, shipment.Gateways[0].Forwarder.PK);
				AssertEquals("Sending Agent type of Consol 1 should be GTT", consol1.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgentWithTariff);
				AssertEquals("Precondition: First Gateway", (byte)1, shipment.Gateways[0].JSG_Sequence);

				AssertEquals("Shipment's gateways should contain Consol 2 sending agent", consol2.SendingForwarderPK, shipment.Gateways[1].Forwarder.PK);
				AssertEquals("Sending Agent type of Consol 2 should be GTA", consol2.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
				AssertEquals("Precondition: Second Gateway", (byte)2, shipment.Gateways[1].JSG_Sequence);
			});

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 3750m, // add FRT for 150 * 25
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 2550m,  // 150 * 17 (Should not use 25 per unit for WAR)
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol1, consol1.SendingForwarder, null, branch1, autorateRevenue: true, autorateCosts: false);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAutorateRevenueForConsol_ShouldLoadIntercompanyTarrif_WhenGatewayIsNotFirstAndGatewaySellIsFRT()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			CreateGlobalCharge("FRT1");
			CreateGlobalCharge("WAR", FlatCalculator.Code, ChargeCodeGroupList.Codes.Insurance);

			var (agent1, _) = CreateCompanyAndBranchProxy("CHZUR");
			var (agent2, branch2) = CreateCompanyAndBranchProxy("BEANR");
			var (agent3, _) = CreateCompanyAndBranchProxy("DEBRE");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;

				Env.Registry.FreightChargeCode = Helper.ChargeCodes["FRT"].PK.ToGuid();
			}

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			var rateEntry = tariff2.AddRateEntry(RateCategory.AIR, RateMode.LSE, "CHZUR", "");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 10m;
			var rateLine2 = rateEntry.AddRateLine("FRT1", UnitCalculator.Code, QuantityUnit.KG);
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 11m;
			var rateLine3 = rateEntry.AddRateLine("WAR", UnitCalculator.Code, QuantityUnit.KG);
			rateLine3.GetCalculator<UnitCalculator>().PerUnit = 17m;

			var consol1 = CreateForwardingConsolWithGatewayAgents("CHZUR", "BEANR", agent1, agent2, checkIsItGateway: false);
			var consol2 = CreateForwardingConsolWithGatewayAgents("BEANR", "DEBRE", agent2, agent3, checkIsItGateway: false);

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "USHOU", false, consol1, consol2);
			shipment.JS_FreightGatewaySellRateAutoratingMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			shipment.JS_GatewayFreightSellRate = 25m;
			shipment.JS_RX_NKGatewayFreightSellRateCurrency = "AUD";
			shipment.JS_ActualWeight = 150m;

			AssertNotNull("Data setup: Create shipment job with mutex", new JobHeader.Loader(shipment).TryCreateWithMutex(branch2));

			Factory.Save();

			CombineAssertions("First gateway should not equal to Consol 2 sending agent", () =>
			{
				AssertEquals("Shipment's gateways should contain Consol 1 sending agent", consol1.SendingForwarderPK, shipment.Gateways[0].Forwarder.PK);
				AssertEquals("Sending Agent type of Consol 1 should be GTA", consol1.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
				AssertEquals("Precondition: First Gateway", (byte)1, shipment.Gateways[0].JSG_Sequence);

				AssertEquals("Shipment's gateways should contain Consol 2 sending agent", consol2.SendingForwarderPK, shipment.Gateways[1].Forwarder.PK);
				AssertEquals("Sending Agent type of Consol 2 should be GTA", consol2.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
				AssertEquals("Precondition: Second Gateway", (byte)2, shipment.Gateways[1].JSG_Sequence);
			});

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 1500m, // 150 * 10 [Should not use 25 per unit]
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 1650m, // 150 * 11
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "WAR",
					JR_OSSellAmt = 2550m, // 150 * 17
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol2, consol2.SendingForwarder, null, branch2, autorateRevenue: true, autorateCosts: false);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAutorateRevenueForConsol_ShouldNotAddFRTChargeCode_WhenGatewayIsNotFirstAndGatewaySellIsFRT()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			CreateGlobalCharge("FRT1");
			CreateGlobalCharge("ODOC", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);

			var (agent1, _) = CreateCompanyAndBranchProxy("CHZUR");
			var (agent2, branch2) = CreateCompanyAndBranchProxy("BEANR");
			var (agent3, _) = CreateCompanyAndBranchProxy("DEBRE");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;

				Env.Registry.FreightChargeCode = Helper.ChargeCodes["FRT"].PK.ToGuid();
			}

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			var rateEntry = tariff2.AddRateEntry(RateCategory.AIR, RateMode.LSE, "CHZUR", "");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine3 = rateEntry.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.KG);
			rateLine3.GetCalculator<UnitCalculator>().PerUnit = 17m;

			var consol1 = CreateForwardingConsolWithGatewayAgents("CHZUR", "BEANR", agent1, agent2, checkIsItGateway: false);
			var consol2 = CreateForwardingConsolWithGatewayAgents("BEANR", "DEBRE", agent2, agent3, checkIsItGateway: false);

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "USHOU", false, consol1, consol2);
			shipment.JS_FreightGatewaySellRateAutoratingMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			shipment.JS_GatewayFreightSellRate = 25m;
			shipment.JS_RX_NKGatewayFreightSellRateCurrency = "AUD";
			shipment.JS_ActualWeight = 150m;

			AssertNotNull("Data setup: Create shipment job with mutex", new JobHeader.Loader(shipment).TryCreateWithMutex(branch2));

			Factory.Save();

			CombineAssertions("First gateway should not equal to Consol 2 sending agent", () =>
			{
				AssertEquals("Shipment's gateways should contain Consol 1 sending agent", consol1.SendingForwarderPK, shipment.Gateways[0].Forwarder.PK);
				AssertEquals("Sending Agent type of Consol 1 should be GTA", consol1.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
				AssertEquals("Precondition: First Gateway", (byte)1, shipment.Gateways[0].JSG_Sequence);

				AssertEquals("Shipment's gateways should contain Consol 2 sending agent", consol2.SendingForwarderPK, shipment.Gateways[1].Forwarder.PK);
				AssertEquals("Sending Agent type of Consol 2 should be GTA", consol2.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
				AssertEquals("Precondition: Second Gateway", (byte)2, shipment.Gateways[1].JSG_Sequence);
			});

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 2550m, // 150 * 17
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol2, consol2.SendingForwarder, null, branch2, autorateRevenue: true, autorateCosts: false);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAutorateRevenueForConsol_ShouldAddFRTChargeCode_ForOnlyFirstGateway_WhenGatewaySellIsAIN()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			CreateGlobalCharge("WAR", FlatCalculator.Code, ChargeCodeGroupList.Codes.Insurance);

			var (agent1, branch1) = CreateCompanyAndBranchProxy("CHZUR");
			var (agent2, _) = CreateCompanyAndBranchProxy("CHBSL");
			var (agent3, _) = CreateCompanyAndBranchProxy("BEANR");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;

				Env.Registry.FreightChargeCode = Helper.ChargeCodes["FRT"].PK.ToGuid();
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent1);
			var rateEntry = tariff1.AddRateEntry(RateCategory.AIR, RateMode.LSE, "CHZUR", "");
			var rateLine = rateEntry.AddRateLine("WAR", UnitCalculator.Code, QuantityUnit.KG);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 17m;

			var consol1 = CreateForwardingConsolWithGatewayAgents("CHZUR", "CHBSL", agent1, agent2, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol1.SendingForwarderAddress, "CHZUR", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol1.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var consol2 = CreateForwardingConsolWithGatewayAgents("CHBSL", "BEANR", agent2, agent3, checkIsItGateway: false);

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "USHOU", false, consol1, consol2);
			shipment.JS_FreightGatewaySellRateAutoratingMode = FreightRateAutoratingModes.Code.AllInRate;
			shipment.JS_GatewayFreightSellRate = 25m;
			shipment.JS_RX_NKGatewayFreightSellRateCurrency = "AUD";
			shipment.JS_ActualWeight = 150m;

			AssertNotNull("Data setup: Create shipment job with mutex", new JobHeader.Loader(shipment).TryCreateWithMutex(branch1));

			Factory.Save();

			CombineAssertions("First gateway should not equal to Consol 2 sending agent", () =>
			{
				AssertEquals("Shipment's gateways should contain Consol 1 sending agent", consol1.SendingForwarderPK, shipment.Gateways[0].Forwarder.PK);
				AssertEquals("Sending Agent type of Consol 1 should be GTT", consol1.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgentWithTariff);
				AssertEquals("Precondition: First Gateway", (byte)1, shipment.Gateways[0].JSG_Sequence);

				AssertEquals("Shipment's gateways should contain Consol 2 sending agent", consol2.SendingForwarderPK, shipment.Gateways[1].Forwarder.PK);
				AssertEquals("Sending Agent type of Consol 2 should be GTA", consol2.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
				AssertEquals("Precondition: Second Gateway", (byte)2, shipment.Gateways[1].JSG_Sequence);
			});

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 3750m, // add FRT for 150 * 25
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "WAR",
					JR_OSSellAmt = 2550m, // add FRT for 150 * 17
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol1, consol1.SendingForwarder, null, branch1, autorateRevenue: true, autorateCosts: false);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAutorateRevenueForConsol_ShouldLoadIntercompanyTarrif_WhenGatewayIsNotFirstAndGatewaySellIsAIN()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			CreateGlobalCharge("FRT1");
			CreateGlobalCharge("WAR", FlatCalculator.Code, ChargeCodeGroupList.Codes.Insurance);

			var (agent1, _) = CreateCompanyAndBranchProxy("CHZUR");
			var (agent2, branch2) = CreateCompanyAndBranchProxy("BEANR");
			var (agent3, _) = CreateCompanyAndBranchProxy("DEBRE");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;

				Env.Registry.FreightChargeCode = Helper.ChargeCodes["FRT"].PK.ToGuid();
			}

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			var rateEntry = tariff2.AddRateEntry(RateCategory.AIR, RateMode.LSE, "CH", "");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG);
			rateLine1.GetCalculator<UnitCalculator>().PerUnit = 10m;
			var rateLine2 = rateEntry.AddRateLine("FRT1", UnitCalculator.Code, QuantityUnit.KG);
			rateLine2.GetCalculator<UnitCalculator>().PerUnit = 11m;
			var rateLine3 = rateEntry.AddRateLine("WAR", UnitCalculator.Code, QuantityUnit.KG);
			rateLine3.GetCalculator<UnitCalculator>().PerUnit = 17m;

			var consol1 = CreateForwardingConsolWithGatewayAgents("CHZUR", "BEANR", agent1, agent2, checkIsItGateway: false);
			var consol2 = CreateForwardingConsolWithGatewayAgents("BEANR", "DEBRE", agent2, agent3, checkIsItGateway: false);

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "USHOU", false, consol1, consol2);
			shipment.JS_FreightGatewaySellRateAutoratingMode = FreightRateAutoratingModes.Code.AllInRate;
			shipment.JS_GatewayFreightSellRate = 25m;
			shipment.JS_RX_NKGatewayFreightSellRateCurrency = "AUD";
			shipment.JS_ActualWeight = 150m;

			AssertNotNull("Data setup: Create shipment job with mutex", new JobHeader.Loader(shipment).TryCreateWithMutex(branch2));

			Factory.Save();

			CombineAssertions("First gateway should not equal to Consol 2 sending agent", () =>
			{
				AssertEquals("Shipment's gateways should contain Consol 1 sending agent", consol1.SendingForwarderPK, shipment.Gateways[0].Forwarder.PK);
				AssertEquals("Sending Agent type of Consol 1 should be GTA", consol1.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
				AssertEquals("Precondition: First Gateway", (byte)1, shipment.Gateways[0].JSG_Sequence);

				AssertEquals("Shipment's gateways should contain Consol 2 sending agent", consol2.SendingForwarderPK, shipment.Gateways[1].Forwarder.PK);
				AssertEquals("Sending Agent type of Consol 2 should be GTA", consol2.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
				AssertEquals("Precondition: Second Gateway", (byte)2, shipment.Gateways[1].JSG_Sequence);
			});

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 1500m, // 150 * 10 [Should not use 25 per unit]
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					JR_OSSellAmt = 1650m, // 150 * 11
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "WAR",
					JR_OSSellAmt = 2550m, // 150 * 17
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol2, consol2.SendingForwarder, null, branch2, autorateRevenue: true, autorateCosts: false);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAutorateRevenueForConsol_ShouldNotAddFRTChargeCode_WhenGatewayIsNotFirstAndGatewaySellIsAIN()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			CreateGlobalCharge("ODOC", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);

			var (agent1, _) = CreateCompanyAndBranchProxy("CHZUR");
			var (agent2, branch2) = CreateCompanyAndBranchProxy("BEANR");
			var (agent3, _) = CreateCompanyAndBranchProxy("DEBRE");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				agent2.OH_IsCreditor = true;
				agent3.OH_IsCreditor = true;

				Env.Registry.FreightChargeCode = Helper.ChargeCodes["FRT"].PK.ToGuid();
			}

			var tariff2 = Helper.NewIntercompanyTariff(agent2);
			var rateEntry = tariff2.AddRateEntry(RateCategory.ORG, RateMode.LSE, "CHZUR", "");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine3 = rateEntry.AddRateLine("ODOC", UnitCalculator.Code, QuantityUnit.KG);
			rateLine3.GetCalculator<UnitCalculator>().PerUnit = 17m;

			var consol1 = CreateForwardingConsolWithGatewayAgents("CHZUR", "BEANR", agent1, agent2, checkIsItGateway: false);
			var consol2 = CreateForwardingConsolWithGatewayAgents("BEANR", "DEBRE", agent2, agent3, checkIsItGateway: false);

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("CHZUR", "USHOU", false, consol1, consol2);
			shipment.JS_FreightGatewaySellRateAutoratingMode = FreightRateAutoratingModes.Code.AllInRate;
			shipment.JS_GatewayFreightSellRate = 25m;
			shipment.JS_RX_NKGatewayFreightSellRateCurrency = "AUD";
			shipment.JS_ActualWeight = 150m;

			AssertNotNull("Data setup: Create shipment job with mutex", new JobHeader.Loader(shipment).TryCreateWithMutex(branch2));

			Factory.Save();

			CombineAssertions("First gateway should not equal to Consol 2 sending agent", () =>
			{
				AssertEquals("Shipment's gateways should contain Consol 1 sending agent", consol1.SendingForwarderPK, shipment.Gateways[0].Forwarder.PK);
				AssertEquals("Sending Agent type of Consol 1 should be GTA", consol1.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
				AssertEquals("Precondition: First Gateway", (byte)1, shipment.Gateways[0].JSG_Sequence);

				AssertEquals("Shipment's gateways should contain Consol 2 sending agent", consol2.SendingForwarderPK, shipment.Gateways[1].Forwarder.PK);
				AssertEquals("Sending Agent type of Consol 2 should be GTA", consol2.JK_SendingForwarderHandlingType, AgentStatusList.Codes.GatewayAgent);
				AssertEquals("Precondition: Second Gateway", (byte)2, shipment.Gateways[1].JSG_Sequence);
			});

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					JR_OSSellAmt = 2550m, // 150 * 17
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol2, consol2.SendingForwarder, null, branch2, autorateRevenue: true, autorateCosts: false);
		}

		[TestDate(2019, 12, 05)]
		public void TestAutorateRevenue_JobServicesShouldNotFilterIntercompanyTariff()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("DLAB");

			var intercompanyTariff = Helper.NewIntercompanyTariff(GlbCompany.CurrentCompany.OrgProxy);
			CreateFlatRate(intercompanyTariff, "FRT", 33m, "NZAKL", "AUSYD");
			intercompanyTariff.AddRateEntryWithFlatRateLine(RateCategory.DST, RateMode.ALL, "NZAKL", "AUSYD", "DLAB", 350);

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: GlbCompany.CurrentCompany.OrgProxy, receivingAgent: null);
			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", false);
			shipment.DocsAndCartage.JP_DeliveryLabourTime = ZDateTime.MinSmallDateTimeValue.AddHours(3);
			shipment.DocsAndCartage.JP_DeliveryLabourCharge = 50m;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 33m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "DLAB",
					JR_OSSellAmt = 350m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "DLAB",
					JR_OSSellAmt = 150m,
					RevenueCalculationDescription = "DLAB: 3 Hour(s) @ AUD 50.00/Hour"
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, GlbCompany.CurrentCompany.OrgProxy);
		}

		[TestDate(2020, 10, 05)]
		public void TestAutorateConsolCostAndRevenueFromGatewayInvoicing_SpotRateIsNotAddedForConsol_GatewaySellIsAddedAsRevenue()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("BAF");

			var (sendingAgent, branch1) = CreateCompanyAndBranchProxy("NZAKL");

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var entry = CreateFlatRate(intercompanyTariff, "FRT", 100m, "NZAKL", "AUSYD");
			entry.AddRateLine("BAF", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 200m;

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent, GlbCompany.CurrentCompany.OrgProxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "NZAKL", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, "AUSYD", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", false);
			shipment.JS_FreightGatewaySellRateAutoratingMode = FreightRateAutoratingModes.Code.AllInRate;
			shipment.JS_GatewayFreightSellRate = 30m;
			shipment.JS_RX_NKGatewayFreightSellRateCurrency = "AUD";
			shipment.JS_ActualWeight = 2000m;

			shipment.JS_UnitFreightRate = 50m;
			shipment.JS_RX_NKFrtRateCurrency = CurrencyCodes.Australia;
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.AllInRate;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 60000m,
					RevenueCalculationDescription = @"FRT: 2000 Kilogram(s) @ AUD 30.00/KG
FRT Global Charge
Gateway Sell is applicable for Shipment S00001000.",
					CostCalculationDescription = ""
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, sendingAgent, branch: branch1, autorateCosts: true, autorateRevenue: true);
		}

		[TestDate(2020, 10, 05)]
		public void TestAutorateShipmentCostAndRevenue_SpotRateShouldBeAddedAsRevenue_GatewaySellShouldBeAddedAsCost()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("BAF");
			CreateGlobalCharge("ODOC", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);

			var (sendingAgent, branch1) = CreateCompanyAndBranchProxy("NZAKL");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "NZ", "AU", "ODOC", 50);
			}

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var entry = CreateFlatRate(intercompanyTariff, "FRT", 100m, "NZAKL", "AUSYD");
			entry.AddRateLine("BAF", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 200m;

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent, GlbCompany.CurrentCompany.OrgProxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "NZAKL", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, "AUSYD", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", false);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			shipment.JS_FreightGatewaySellRateAutoratingMode = FreightRateAutoratingModes.Code.FreightPlusRate;
			shipment.JS_GatewayFreightSellRate = 25m;
			shipment.JS_RX_NKGatewayFreightSellRateCurrency = "AUD";
			shipment.JS_ActualWeight = 100m;

			shipment.JS_UnitFreightRate = 40m;
			shipment.JS_RX_NKFrtRateCurrency = CurrencyCodes.Australia;
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.FreightPlusRate;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 200m,
					CostCalculationDescription = @"BAF: Base Rate NZD 200.00
BAF Global Charge
Charge located in PROXYNZAKL Intercompany Tariff with the following details:"
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 4000m,
					JR_OSCostAmt = 2500m,
					CostCalculationDescription = @"FRT: 100 Kilogram(s) @ AUD 25.00/KG
FRT Global Charge
Gateway Sell is applicable for Shipment S00001000.",

					RevenueCalculationDescription = @"FRT: 100 Kilogram(s) @ AUD 40.00/KG
FRT Global Charge
One Off Freight Rate is applicable for Shipment S00001000."
				},
				new AssertionCharge
				{
					ChargeCode = "ODOC",
					CostAccountCode = TransportProvider1.OH_Code
				},
			};

			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, null, branch1, autorateRevenue: true, autorateCosts: true);
			AssertAutoratingAuditLogNoteContainsLinesWithUserContextOverride(shipment, branch1,
				"Information: RateLine Filtered FRT-FLT-Intercompany Tariff PROXYNZAKL	reason:	replaced by Job Negotiated Cost/Gateway Sell/One Off Freight Rate");
		}

		[TestDate(2020, 10, 05)]
		public void TestAutorateConsolRevenueFromGatewayInvoicing_SpotRateIsNotAddedForConsol()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("BAF");

			var (sendingAgent, branch1) = CreateCompanyAndBranchProxy("NZAKL");

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var entry = CreateFlatRate(intercompanyTariff, "FRT", 100m, "NZAKL", "AUSYD");
			entry.AddRateLine("BAF", FlatCalculator.Code).GetCalculator<FlatCalculator>().BaseRate = 200m;

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent, GlbCompany.CurrentCompany.OrgProxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "NZAKL", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, "AUSYD", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", false);
			shipment.JS_ActualWeight = 2000m;

			shipment.JS_UnitFreightRate = 50m;
			shipment.JS_RX_NKFrtRateCurrency = CurrencyCodes.Australia;
			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.FreightPlusRate;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 200m,
					CostAccountCode = sendingAgent.OH_Code,
					RevenueCalculationDescription = "BAF: Base Rate NZD 200.00"
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 100m,
					CostAccountCode = sendingAgent.OH_Code,
					RevenueCalculationDescription = "FRT: Base Rate AUD 100.00"
				}
			};

			AutorateAndAssertWithUserContextOverride(expected, consol, sendingAgent, branch: branch1, autorateCosts: true, autorateRevenue: true);
		}

		#endregion

		#region VED calcualtor for Gateway

		[TestDate(2015, 07, 15)]
		public void TestConsolCalculateEqualizationPivotsAndAutoRateForNongatewayConsol_ShouldFillConsolCostingTab()
		{
			var actualLog = new DummyOperationalActionSectionLog();
			var applicator = new ConsolEqualizeAndAutorateActionMethodApplicator(Factory);
			var equalizationSetupObjects = SetupEqualizationCostAndObjects_AIR();
			applicator.InitialiseBeforeIndividiualBatchRun();

			var consol = equalizationSetupObjects[0] as ForwardingConsol;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			using (SetJR_DescDisplayAllText())
			{
				AssertEquals("Consol should be none Gateway", false, consol.IsGatewayBillingEnabled());
				AssertEquals("Use Gateway Billing Tab should be false", false, consol.FillGatewayBillingTabForAutorating());

				applicator.Apply(actualLog, equalizationSetupObjects);

				var expectedLog = @"INFO: Volume Equalization Discount results:
	Pivot Weight per LD-8 as per contract '010001' is 1000 KG
	C00001 - AUSYD - NZAKL: 1800 KG in 1xLD-8
	Average Weight: (1800)/1 = 1800.00 KG
	Pivot Weight achieved.";

				AssertOperationLogContainsLines(actualLog, "Log should contain equalisation result", expectedLog);
				AssertEquals("Consol costs should have value", true, consol.HasConsolCosts(GlbCompany.CurrentCompany));
				Factory.Save();
			}
		}

		[TestDate(2015, 07, 15)]
		public void TestConsolCalculateEqualizationPivotsAndAutoRateForGatewayConsol_ShouldFillGatewayBillingTab()
		{
			var agent = GlbCompany.CurrentCompany.OrgProxy;
			var (agent1, _) = CreateCompanyAndBranchProxy("NZAKL");

			var actualLog = new DummyOperationalActionSectionLog();
			var applicator = new ConsolEqualizeAndAutorateActionMethodApplicator(Factory);
			var equalizationSetupObjects = SetupEqualizationCostAndObjects_AIR();
			applicator.InitialiseBeforeIndividiualBatchRun();

			var consol = equalizationSetupObjects[0] as ForwardingConsol;

			consol.JK_OA_SendingForwarderAddress = agent.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "AUSYD", AgentStatusList.Codes.GatewayAgentWithTariff);

			consol.JK_OA_ReceivingForwarderAddress = agent1.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, "NZAKL", AgentStatusList.Codes.GatewayAgentWithTariff);

			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			using (SetJR_DescDisplayAllText())
			{
				using (var job = new JobHeader.Loader(consol).TryLoadOrCreateWithoutMutexForTestOnly())
				{
					Factory.Save();

					AssertEquals("Consol should be Gateway", true, consol.IsGatewayBillingEnabled());
					AssertEquals("Use Gateway Billing Tab should be true", true, consol.FillGatewayBillingTabForAutorating());

					applicator.Apply(actualLog, equalizationSetupObjects);

					var expectedLog = @"INFO: Volume Equalization Discount results:
	Pivot Weight per LD-8 as per contract '010001' is 1000 KG
	C00001 - AUSYD - NZAKL: 1800 KG in 1xLD-8
	Average Weight: (1800)/1 = 1800.00 KG
	Pivot Weight achieved.";

					AssertOperationLogContainsLines(actualLog, "Log should contain equalisation result", expectedLog);
					AssertEquals("Consol costs should not have value", false, consol.HasConsolCosts(GlbCompany.CurrentCompany));
				}
			}
		}

		BusinessObject[] SetupEqualizationCostAndObjects_AIR()
		{
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				#region Setup Equalization Rate July 1-15

				Helper.ChargeCodes["FRT"].AC_DepartmentFilterList = "ALL";
				var creditor = TransportProvider1;
				creditor.OH_IsCreditor = true;
				creditor.OH_IsShippingLine = true;
				creditor.OH_IsShippingProvider = true;

				var costing = Factory.New<Costing>();
				costing.TH_OH = creditor.PK;

				#region Cost for LD-8 Containers

				var costEntryLD8 = costing.AddRateEntry(RateCategory.AIR, RateMode.ULD, "AUSYD", "NZAKL", "", "LD-8");
				costEntryLD8.TI_RX_NKCurrency = "AUD";
				costEntryLD8.TI_ContractNumber = "010001";
				costEntryLD8.TI_RateStartDate = new ZDate(2015, 7, 1);
				costEntryLD8.TI_RateEndDate = new ZDate(2015, 7, 15);
				costEntryLD8.RateLines.RemoveAndDeleteAll();
				var rateLineLD8 = costEntryLD8.AddRateLine("FRT", EqualizationCalculator.Code, QuantityUnit.KG);
				var ld8plusRateLineItem = rateLineLD8.RateLineItems.Cast<RateLineItem>().FirstOrDefault(x => x.RateOperatorIsPlus());
				ld8plusRateLineItem.TM_Break = 1000;
				ld8plusRateLineItem.TM_FlatAmount = 15m;
				ld8plusRateLineItem.TM_RelevantValue = 20m;

				#endregion

				Factory.Save();

				#endregion

				var lD8_Container = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "LD-8").PK;
				var pA5_Container = Factory.LoadFromNaturalKey<MasterFiles.Business.RefContainer>(RefContainerSchema.RC_Code, "PA-5").PK;

				#region Consol Shipped July 2nd - 2600 x 2 x LD8 and 200 x PA5

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_AgentType = AgentType.Agent;
				consol.JK_AWBServiceLevel = "STD";
				consol.JK_ConsolMode = ContainerModes.ULD;
				consol.JK_OA_ShippingLineAddress = creditor.MainAddress.PK;
				consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "NZAKL";
				consol.JK_UniqueConsignRef = "C00001";
				consol.JK_CarrierContractNumber = "010001";

				var consignee = Factory.NewWithValidTestData<OrgHeader>();
				var consignor = Factory.NewWithValidTestData<OrgHeader>();

				var contLD8c1_1 = consol.Containers.AddNew();
				contLD8c1_1.JC_RC = lD8_Container;

				var s1c1 = consol.Shipments.AddNew();
				s1c1.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
				s1c1.JS_TransportMode = TransportModes.Air;
				s1c1.JS_PackingMode = ContainerModes.ULD;
				s1c1.JS_ShipmentType = ShipmentTypes.StandardHouse;
				s1c1.JS_RL_NKOrigin = "AUSYD";
				s1c1.JS_RL_NKDestination = "NZAKL";
				s1c1.JS_INCO = "CFR";
				s1c1.JS_OH_DeliveryAgent = consignee.PK;
				s1c1.JS_ActualVolume = 1;
				s1c1.ConsignorPK = consignor.PK;
				s1c1.ConsigneePK = consignee.PK;
				s1c1.JS_E_DEP = new ZDate(2015, 7, 2);
				s1c1.JS_E_ARV = new ZDateTime(2015, 7, 2, 6, 0, 0);

				var plc1s1_1 = s1c1.OuterPackLines.AddNew();
				plc1s1_1.JL_ActualWeight = 1000m;
				plc1s1_1.JL_JC = contLD8c1_1.PK;

				var s2c1 = consol.Shipments.AddNew();
				s2c1.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
				s2c1.JS_TransportMode = TransportModes.Air;
				s2c1.JS_PackingMode = ContainerModes.ULD;
				s2c1.JS_ShipmentType = ShipmentTypes.StandardHouse;
				s2c1.JS_RL_NKOrigin = "AUSYD";
				s2c1.JS_RL_NKDestination = "USLAX";
				s2c1.JS_INCO = "CFR";
				s2c1.JS_OH_DeliveryAgent = consignee.PK;
				s2c1.ConsignorPK = consignor.PK;
				s2c1.ConsigneePK = consignee.PK;
				s2c1.JS_E_DEP = new ZDate(2015, 7, 2);
				s2c1.JS_E_ARV = new ZDateTime(2015, 7, 2, 6, 0, 0);
				s2c1.JS_ActualVolume = 2;

				var plc1s2_1 = s2c1.OuterPackLines.AddNew();
				plc1s2_1.JL_ActualWeight = 800m;
				plc1s2_1.JL_JC = contLD8c1_1.PK;

				#endregion

				Factory.Save();

				return new BusinessObject[] { consol };
			}
		}

		//This registry setting ensure that JR_Desc displays not only the the charge code description but all information in the following format;
		//International Freight - 1 20GP Container(s) @ USD 2000.00/Container
		IDisposable SetJR_DescDisplayAllText()
		{
			var defaultValue = OrganisationRegistry.Instance.InvoiceRollupOrGroup.Value.GetBestMatch(OrgConstants.ServiceDirection.Code.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgConstants.ModesForGroupOrSubTotal.Codes.All, OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code);
			defaultValue.InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.All;

			var collection = new InvoiceRollupOrGroupCollection();
			collection.Add(defaultValue);

			return OrganisationRegistry.Instance.InvoiceRollupOrGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		#endregion

		#region Payment Term Override

		[TestDate(2020, 04, 26)]
		public void TestAutorateRevenueFromIntercompanyTariffsForConsolConsideringPaymentTermOverride()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");
			Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var intercompanyTariff = Helper.NewIntercompanyTariff(GlbCompany.CurrentCompany.OrgProxy);
			var entry1 = CreateFlatRate(intercompanyTariff, "FRT", 33m, "NZAKL", "AUSYD");
			entry1.TI_PaymentTerm = "PPD";

			var entry2 = CreateFlatRate(intercompanyTariff, "FRT", 43m, "NZAKL", "AUSYD");
			entry2.TI_PaymentTerm = "CCX";

			var entry3 = CreateFlatRate(intercompanyTariff, "FRT", 53m, "NZAKL", "AUSYD");
			entry3.TI_PaymentTerm = "";

			var consol = CreateForwardingConsolWithGatewayAgents("NZAKL", "AUSYD", sendingAgent: GlbCompany.CurrentCompany.OrgProxy, receivingAgent: null);
			var shipment = CreateGatewayShipment(consol, "NZAKL", "AUSYD", false);
			shipment.JS_PaymentTermAutoratingOverride = "PPD";

			Factory.Save();

			AssertEquals("Shipment's gateways should contain consol sending agent", consol.SendingForwarder.PK, shipment.Gateways[0].Forwarder.PK);

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 33m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				}
			};

			//We expect that autorating match rate 1 and 3 but finally select rate 1 cause it exactly matches shipment payment term override
			AutorateAndAssertWithUserContextOverride(expected, consol, GlbCompany.CurrentCompany.OrgProxy, autorateRevenue: true, autorateCosts: false);
		}

		[TestDate(2020, 04, 26)]
		public void TestAutorateCostFromIntercompanyTariffsForConsolConsideringPaymentTermOverride()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			Helper.ChargeCodes.CreateGlobalCharge("FRT2");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			var receivingAgent = CreateCompanyAndBranchProxy("USCHS");

			Factory.Save();

			var sendingIntercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			var receivingIntercompanyTariff = Helper.NewIntercompanyTariff(receivingAgent.proxy);

			var sendingRate1 = CreateFlatRate(sendingIntercompanyTariff, "FRT1", 66m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			sendingRate1.TI_PaymentTerm = "CCX";

			var sendingRate2 = CreateFlatRate(sendingIntercompanyTariff, "FRT1", 46m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			sendingRate2.TI_PaymentTerm = "PPD";

			var receivingRate1 = CreateFlatRate(receivingIntercompanyTariff, "FRT2", 33m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			receivingRate1.TI_PaymentTerm = "CCX";

			var receivingRate2 = CreateFlatRate(receivingIntercompanyTariff, "FRT2", 83m, origin, destination, gatewayAgentType: GatewayAgentType.Codes.SendingAgent);
			receivingRate2.TI_PaymentTerm = "PPD";

			CreateRateEntryWithRateLine(RatingHeaderTypes.Costing, sendingAgent, origin, destination, "BAF", 15);

			var consol = CreateForwardingConsolWithGatewayAgents(origin, destination, sendingAgent: sendingAgent, receivingAgent: receivingAgent.proxy);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment = CreateGatewayShipment(consol, origin, destination, true);
			shipment.JS_PaymentTermAutoratingOverride = "CCX";

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "BAF",
					JR_OSSellAmt = 15m,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT2",
					JR_OSSellAmt = 33m,
					RelatedJobNumber = shipment.JS_UniqueConsignRef
				},
			};

			//We expect that autorating match receivingRate1 cause this one only matches with shipment payment term override
			AutorateAndAssertWithUserContextOverride(expected, consol, null, branch: sendingAgent.Branch, autorateRevenue: false, autorateCosts: true);
		}

		[TestDate(2020, 04, 26)]
		public void TestAutorateShipmentCost_LoginAsNonGateway_ConsideringPaymentTermOverride()
		{
			CreateGlobalCharge("FRT");

			var (agent1, branch1) = CreateNonProxyBranch("CHBSL");
			var (agent2, _) = CreateCompanyAndBranchProxy("BEANR");

			var chargeCode = Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			chargeCode.AC_GC = branch1.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent2.OH_IsCreditor = true;

				Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "CH", "US", "ORG1", 50);
			}

			var tariff1 = Helper.NewIntercompanyTariff(agent2);
			var entry1 = CreateFlatRate(tariff1, "FRT", 100m, "CHBSL", "USHOU");
			entry1.TI_PaymentTerm = "PPD";

			var entry2 = CreateFlatRate(tariff1, "FRT", 200m, "CHBSL", "USHOU");
			entry2.TI_PaymentTerm = "CCX";

			var entry3 = CreateFlatRate(tariff1, "FRT", 300m, "CHBSL", "USHOU");
			entry3.TI_PaymentTerm = "";

			TransportProvider1.OH_IsCreditor = true;

			var shipment = CreateStandaloneGatewayShipment("CHBSL", "USHOU", true, agent2.PK);
			shipment.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			shipment.JS_PaymentTermAutoratingOverride = "CCX";
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 200m,
					CostAccountCode = agent2.OH_Code
				},
				new AssertionCharge
				{
					ChargeCode = "ORG1",
					JR_OSCostAmt = 50m,
					CostAccountCode = TransportProvider1.OH_Code
				}
			};

			//We expect that autorating match entry2 and entry3 but finally choose entry2 cause it exactly match shipment payment term override
			AutorateAndAssertWithUserContextOverride(expected, shipment, Consignor, branch: branch1, autorateRevenue: false);
		}

		#endregion

		#region Autorate ASM shipments

		[TestDate(2021, 05, 01)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAutorateConsolRevenue_FromGatewayInvoicing_ShouldFilterSAMRatesForAssemblyMaster_ShouldFilterNonSAMRatesForASMSubShipments()
		{
			CreateGlobalCharge("FRT");
			CreateGlobalCharge("SFRT");

			var (agent1, branch1) = CreateCompanyAndBranchProxy("BEANR");
			var (agent2, _) = CreateCompanyAndBranchProxy("SGSIN");

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
			}

			var tariff = Helper.NewIntercompanyTariff(agent1);
			var rateEntry = CreateFlatRate(tariff, "FRT", 100m, "BEANR", "SGSIN");
			rateEntry.AddFlatRateLine("SFRT", 200m, unitFactor: "SAM");

			var gatewayConsol = CreateForwardingConsolWithGatewayAgents("BEANR", "SGSIN", agent1, agent2, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(gatewayConsol.SendingForwarderAddress, "BEANR", AgentStatusList.Codes.GatewayAgentWithTariff);
			gatewayConsol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(gatewayConsol.ReceivingForwarderAddress, "SGSIN", AgentStatusList.Codes.GatewayAgentWithTariff);
			gatewayConsol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment1 = CreateGatewayShipmentWithMultipleGatewayConsols("BEANR", "SGSIN", false, gatewayConsol);
			shipment1.JS_UniqueConsignRef = "S000001";

			var shipment2 = CreateGatewayShipmentWithMultipleGatewayConsols("BEANR", "SGSIN", false, gatewayConsol);
			shipment2.JS_UniqueConsignRef = "S000002";
			shipment2.JS_ShipmentType = ShipmentTypes.AssemblyMaster;

			var subShipment1 = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "BEANR", "SGSIN", 1000);
			subShipment1.JS_UniqueConsignRef = "SS00001";
			subShipment1.JS_ShipmentType = ShipmentTypes.StandardHouse;
			subShipment1.JS_JS_ColoadMasterShipment = shipment2.PK;

			var subShipment2 = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "BEANR", "SGSIN", 2000);
			subShipment2.JS_UniqueConsignRef = "SS00002";
			subShipment2.JS_ShipmentType = ShipmentTypes.AssemblyMaster;
			subShipment2.JS_JS_ColoadMasterShipment = shipment2.PK;

			var subShipment3 = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "BEANR", "SGSIN", 3000);
			subShipment3.JS_UniqueConsignRef = "SS00003";
			subShipment3.JS_ShipmentType = ShipmentTypes.StandardHouse;
			subShipment3.JS_JS_ColoadMasterShipment = subShipment2.PK;

			AssertNotNull("Data setup: Create shipment job with mutex", new JobHeader.Loader(shipment1).TryCreateWithMutex(branch1));
			AssertNotNull("Data setup: Create shipment job with mutex", new JobHeader.Loader(shipment2).TryCreateWithMutex(branch1));
			AssertNotNull("Data setup: Create shipment job with mutex", new JobHeader.Loader(subShipment1).TryCreateWithMutex(branch1));
			AssertNotNull("Data setup: Create shipment job with mutex", new JobHeader.Loader(subShipment2).TryCreateWithMutex(branch1));
			AssertNotNull("Data setup: Create shipment job with mutex", new JobHeader.Loader(subShipment3).TryCreateWithMutex(branch1));

			Factory.Save();

			var allShipments = ((ForwardingConsolRatingAdapter)gatewayConsol.RatingAdapter).AllShipments;
			AssertEquals(5, allShipments.Count());

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					RelatedJobNumber = shipment1.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "FRT",
					RelatedJobNumber = shipment2.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "SFRT",
					RelatedJobNumber = shipment1.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "SFRT",
					RelatedJobNumber = subShipment1.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "SFRT",
					RelatedJobNumber = subShipment2.JS_UniqueConsignRef
				},
				new AssertionCharge
				{
					ChargeCode = "SFRT",
					RelatedJobNumber = subShipment3.JS_UniqueConsignRef
				}
			};

			// should filter SAM charges for Assembly Master shipment
			AutorateAndAssertWithUserContextOverride(expected, gatewayConsol, gatewayConsol.SendingForwarder, null, branch1, autorateRevenue: true, autorateCosts: false);
		}

		[TestDate(2021, 05, 01)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAutorateConsolRevenueAndCost_FromGatewayInvoicing_ShouldNotLoadNormalCostsForSubShipments()
		{
			CreateGlobalCharge("FRT1");
			CreateGlobalCharge("FRT2", isConsolLevel: true);

			var (agent1, branch1) = CreateCompanyAndBranchProxy("BEANR");
			var (agent2, _) = CreateCompanyAndBranchProxy("SGSIN");

			var charge1 = Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			charge1.AC_GC = branch1.Company.PK;

			var charge2 = Helper.ChargeCodes.New("ODOC1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			charge2.AC_GC = branch1.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;

				Helper.NewCosting(null).AddRateEntryWithFlatRateLine(RateCategory.AIR, RateMode.LSE, "BE", "SG", "FRT2", 60);
				Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "BE", "SG", "ORG1", 50);
			}

			var tariff = Helper.NewIntercompanyTariff(agent1);
			CreateFlatRate(tariff, "FRT1", 100m, "BEANR", "SGSIN");

			TransportProvider1.OH_IsCreditor = true;

			var gatewayConsol = CreateForwardingConsolWithGatewayAgents("BEANR", "SGSIN", agent1, agent2, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(gatewayConsol.SendingForwarderAddress, "BEANR", AgentStatusList.Codes.GatewayAgentWithTariff);
			gatewayConsol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(gatewayConsol.ReceivingForwarderAddress, "SGSIN", AgentStatusList.Codes.GatewayAgentWithTariff);
			gatewayConsol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment1 = CreateGatewayShipment(gatewayConsol, "BEANR", "SGSIN", false, branch: branch1);

			var shipment2 = CreateGatewayShipment(gatewayConsol, "BEANR", "SGSIN", false, branch: branch1);
			shipment2.JS_ShipmentType = ShipmentTypes.AssemblyMaster;

			var subShipment1 = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "BEANR", "SGSIN", 1000);
			subShipment1.JS_ShipmentType = ShipmentTypes.StandardHouse;
			subShipment1.JS_JS_ColoadMasterShipment = shipment2.PK;

			var subShipment2 = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "BEANR", "SGSIN", 2000);
			subShipment2.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			subShipment2.JS_ShipmentType = ShipmentTypes.AssemblyMaster;
			subShipment2.JS_JS_ColoadMasterShipment = shipment2.PK;

			var subShipment3 = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "BEANR", "SGSIN", 3000);
			subShipment3.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			subShipment3.JS_ShipmentType = ShipmentTypes.StandardHouse;
			subShipment3.JS_JS_ColoadMasterShipment = subShipment2.PK;

			Factory.Save();

			var allShipments = ((ForwardingConsolRatingAdapter)gatewayConsol.RatingAdapter).AllShipments;
			AssertEquals(5, allShipments.Count());

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					RelatedJobNumber = shipment1.JS_UniqueConsignRef,
					JR_LocalSellAmt = 100m,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT1",
					RelatedJobNumber = shipment2.JS_UniqueConsignRef,
					JR_LocalSellAmt = 100m,
				},
				new AssertionCharge
				{
					ChargeCode = "FRT2",
					RelatedJobNumber = "", // this is consol cost
					JR_LocalCostAmt = 60m,
				}
			};

			// this is existing behaviour- should not create any charge for sub shipments
			AutorateAndAssertWithUserContextOverride(expected, gatewayConsol, gatewayConsol.SendingForwarder, null, branch1, autorateRevenue: true, autorateCosts: true);
		}

		[TestDate(2021, 05, 01)]
		public void TestAutorateShipmentCost_ShouldLoadIntercompanyTariffSAMRatesAndNormalCostsForSubShipments()
		{
			CreateGlobalCharge("FRT1");
			CreateGlobalCharge("SFRT");

			var (agent1, branch1) = CreateCompanyAndBranchProxy("BEANR");
			var (agent2, _) = CreateCompanyAndBranchProxy("SGSIN");

			var chargeCode = Helper.ChargeCodes.New("ORG1", "ORG charge", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			chargeCode.AC_GC = branch1.Company.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				agent1.OH_IsCreditor = true;
				Helper.NewCosting(TransportProvider1).AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "BE", "SG", "ORG1", 50);
			}

			var tariff = Helper.NewIntercompanyTariff(agent1);
			var rateEntry = CreateFlatRate(tariff, "FRT1", 100m, "BEANR", "SGSIN");
			rateEntry.AddFlatRateLine("SFRT", 200m, unitFactor: "SAM");

			TransportProvider1.OH_IsCreditor = true;

			var gatewayConsol = CreateForwardingConsolWithGatewayAgents("BEANR", "SGSIN", agent1, agent2, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(gatewayConsol.SendingForwarderAddress, "BEANR", AgentStatusList.Codes.GatewayAgentWithTariff);
			gatewayConsol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(gatewayConsol.ReceivingForwarderAddress, "SGSIN", AgentStatusList.Codes.GatewayAgentWithTariff);
			gatewayConsol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var shipment1 = CreateGatewayShipment(gatewayConsol, "BEANR", "SGSIN", false);
			shipment1.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;

			var shipment2 = CreateGatewayShipment(gatewayConsol, "BEANR", "SGSIN", false);
			shipment2.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			shipment2.JS_ShipmentType = ShipmentTypes.AssemblyMaster;

			var subShipment1 = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "BEANR", "SGSIN", 1000);
			subShipment1.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			subShipment1.JS_ShipmentType = ShipmentTypes.StandardHouse;
			subShipment1.JS_JS_ColoadMasterShipment = shipment2.PK;

			var subShipment2 = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "BEANR", "SGSIN", 2000);
			subShipment2.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			subShipment2.JS_ShipmentType = ShipmentTypes.AssemblyMaster;
			subShipment2.JS_JS_ColoadMasterShipment = shipment2.PK;

			var subShipment3 = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "BEANR", "SGSIN", 3000);
			subShipment3.DocsAndCartage.PickupCartageCoPK = TransportProvider1.PK;
			subShipment3.JS_ShipmentType = ShipmentTypes.StandardHouse;
			subShipment3.JS_JS_ColoadMasterShipment = subShipment2.PK;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
				},
				new AssertionCharge
				{
					ChargeCode = "SFRT",
				},
				new AssertionCharge
				{
					ChargeCode = "ORG1",
				}
			};
			// should get all ICT rates(both SAM and non-SAM rates) and normal costs for STD shipment
			AutorateAndAssertWithUserContextOverride(expected, shipment1, Consignor, branch: branch1, autorateRevenue: false);

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT1",
				},
				new AssertionCharge
				{
					ChargeCode = "ORG1",
				}
			};
			// should get ICT(non-SAM rates) and normal costs for Top level ASM
			AutorateAndAssertWithUserContextOverride(expected, shipment2, Consignor, branch: branch1, autorateRevenue: false);
			AssertAutoratingAuditLogNoteContainsLinesWithUserContextOverride(shipment2, branch1,
				"Information: RateLine Filtered SFRT-FLT-Intercompany Tariff PROXYBEANR	reason:	'top-level' Assembly Master lead shipment(s) are not applicable for Unit Factor of SAM.");

			// should get ICT(SAM Rates) and normal costs for sub shipments
			AssertSubShipment(subShipment1);
			AssertSubShipment(subShipment2);
			AssertSubShipment(subShipment3);

			void AssertSubShipment(ForwardingShipment forwardingShipment)
			{
				expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "ORG1",
					},
					new AssertionCharge
					{
						ChargeCode = "SFRT",
					}
				};
				AutorateAndAssertWithUserContextOverride(expected, forwardingShipment, Consignor, branch: branch1, autorateRevenue: false);
				AssertAutoratingAuditLogNoteContainsLinesWithUserContextOverride(forwardingShipment, branch1,
					"Information: RateLine Filtered FRT1-FLT-Intercompany Tariff PROXYBEANR	reason:	Assembly Master sub-shipment(s) are not applicable for Unit Factor not setting as SAM.");
			}
		}

		#endregion

		#region Autorate Shipment Cost & Revenue

		[TestDate(2022, 1, 1)]
		public void TestAutorateShipmentCostAndRevenue_WhenICTAndClientRateHaveTheSameChargeCode_DebtorShouldBeSetProperly()
		{
			var globalCharge = CreateGlobalCharge("GOTHC", chargeGroup: ChargeCodeGroupList.Codes.Loading);
			globalCharge.AC_MarginPercentage = 0m;
			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;
			sendingAgent.OH_IsCreditor = true;

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent);
			intercompanyTariff.AddRateEntryWithFlatRateLine(RateCategory.ORG, RateMode.ALL, "AUSYD", "HKHKG", globalCharge.AC_Code, 100m);

			Factory.Save();

			var localCharge = Helper.ChargeCodes["GOTHC"];
			Assert("When we create a global charge code, local charge code will be automatically created for that charge", localCharge.AC_GC == GlbCompany.CurrentCompany.PK);

			IncoTermRegistry.Instance.ChargeAgentAlwaysCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, localCharge.PK.ToString());

			var groupClient = Helper.NewOrgHeader();
			groupClient.OH_IsDebtor = true;

			var receivingAgent = Helper.NewOrgHeader(closestPort: "HKHKG");
			receivingAgent.OH_IsDebtor = true;

			var orgManagementRelatedParty = receivingAgent.RelatedManagementSubsidiaryRelations.AddNew();
			orgManagementRelatedParty.PR_OH_Parent = receivingAgent.PK;
			orgManagementRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;
			orgManagementRelatedParty.PR_OH_RelatedParty = groupClient.PK;

			receivingAgent.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 0).P7_ApplyGroupRate = true;

			var clientRate = Helper.NewClientRate(groupClient);
			var entry = clientRate.AddRateEntry(RateCategory.ORG, RateMode.ALL, "AUSYD", "HKHKG");
			entry.RateLines.RemoveAndDeleteAll();
			entry.AddFlatRateLine(localCharge.AC_Code, 120m);

			var consol = CreateForwardingConsolWithGatewayAgents("AUSYD", "HKHKG", sendingAgent, null, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "AUSYD", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;

			var shipment = CreateGatewayShipmentWithMultipleGatewayConsols("AUSYD", "HKHKG", false, consol);
			shipment.JS_INCO = "FOB";
			Factory.Save();

			var job = CreateJob(shipment, shipment.JS_UniqueConsignRef);
			job.PlugInData = shipment;
			job.LocalChargesPK = Consignor.PK;
			job.AgentCollectPK = receivingAgent.PK;

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = localCharge.AC_Code,
					JR_OSSellAmt = 120m,
					JR_OSCostAmt = 100m,
					CostAccountCode = sendingAgent.OH_Code,
					SellAccountCode = receivingAgent.OH_Code
				}
			};

			AutorateAndAssert(expected, shipment, Consignor, job: job);
		}

		#endregion

		#region Autorate Consol Cost/Revenue

		public void TestAutorateRevenue_BothCreditorAndDebtorAreOrgProxies_ShouldSetDebtorCorrectly_ChargeIsMJR()
		{
			var (sendingAgent, sydBranch) = CreateCompanyAndBranchProxy("AUSYD");
			var (receivingAgent, _) = CreateBranchProxy("AUBNE", sydBranch.Company);
			var (pickupAgent, melBranch) = CreateBranchProxy("AUMEL", sydBranch.Company);

			var configuration = new GatewayChargeDefaultDebtorConfiguration()
			{
				ConsolDirection = "ALL",
				ConsolTransportMode = "ALL",
				ChargeGroup = "ALL",
				ConsolPaymentTerm = "ALL",
				RelatedJob = GatewayRelatedJob.Codes.RelatedToShipment,
				PreviousSendingAgent = GatewayPreviousSendingAgent.Codes.All,
				Debtor = GatewayDebtor.Codes.ShipmentPickupAgent,
			};

			var collection = new GatewayChargeDefaultDebtorConfigurationCollection();
			collection.AddRange(configuration);

			var charge = Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			charge.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;

			var tariff1 = Helper.NewIntercompanyTariff(sendingAgent); // revenue
			CreateFlatRate(tariff1, "FRT1", 10, "AUMEL", "NZAKL");

			var tariff2 = Helper.NewIntercompanyTariff(receivingAgent); // cost
			CreateFlatRate(tariff2, "FRT1", 7, "AUMEL", "NZAKL");

			var consol = CreateForwardingConsolWithGatewayAgents("AUSYD", "AUBNE", sendingAgent, receivingAgent, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "AUSYD", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, "AUBNE", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment = CreateGatewayShipment(consol, "AUMEL", "NZAKL", false, false);
			shipment.PickupAgentPK = pickupAgent.PK;

			Factory.Save();

			var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA"));

			using (AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultDebtorConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sydBranch.PK.ToGuid(), gatewayDepartment.PK.ToGuid()))
			using (var shipmentJob = new Job.Loader(shipment).TryCreateWithMutex())
			using (var consolJob = new Job.Loader(consol).TryCreateWithMutex())
			{
				sydBranch.Company.GC_IsGSTRegistered = false;
				shipmentJob.JH_GB = melBranch.PK; // setting shipment branch helps to set the Creditor of the charge to proxy of this branch through setting internal job number
				consolJob.JH_GB = sydBranch.PK;
				consolJob.JH_GE = gatewayDepartment.PK;

				pickupAgent.OH_IsDebtor = true;
				receivingAgent.OH_IsCreditor = true;
				sendingAgent.OH_IsCreditor = true;

				Factory.Save();

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT1",
						JR_LocalSellAmt = 10,
						SellAccountCode = pickupAgent.OH_Code,
						JR_LocalCostAmt = 0,
						CostAccountCode = string.Empty,
						RelatedJobNumber = shipment.JS_UniqueConsignRef,
					}
				};

				// autorate revenue Only => should not set creditor
				AutorateAndAssertWithUserContextOverride(expected, consol, sendingAgent, autorateCosts: false, autorateRevenue: true);

				var jobCharge = consolJob.Charges[0];
				Assert("cost side has not been overriden", !jobCharge.JR_CostRatingOverride);
				Assert("Cost has not been rated", !jobCharge.JR_CostRated);

				expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT1",
						JR_LocalSellAmt = 10m,
						SellAccountCode = pickupAgent.OH_Code,
						JR_LocalCostAmt = 0m,
						CostAccountCode = string.Empty,
						RelatedJobNumber = shipment.JS_UniqueConsignRef
					}
				};

				// re-autorate revenue - cost side was not autorated before and so as both creditor and debtor are org proxy, only debtor should be set
				AutorateAndAssertWithUserContextOverride(expected, consol, sendingAgent, autorateCosts: false, autorateRevenue: true, job: consolJob);
			}
		}

		public void TestAutorateRevenue_BothCreditorAndDebtorAreOrgProxies_ShouldSetDebtorCorrectly_ChargeIsMargin()
		{
			var (sendingAgent, sydBranch) = CreateCompanyAndBranchProxy("AUSYD");
			var (receivingAgent, _) = CreateBranchProxy("AUBNE", sydBranch.Company);
			var (pickupAgent, melBranch) = CreateBranchProxy("AUMEL", sydBranch.Company);

			var configuration = new GatewayChargeDefaultDebtorConfiguration()
			{
				ConsolDirection = "ALL",
				ConsolTransportMode = "ALL",
				ChargeGroup = "ALL",
				ConsolPaymentTerm = "ALL",
				RelatedJob = GatewayRelatedJob.Codes.RelatedToShipment,
				PreviousSendingAgent = GatewayPreviousSendingAgent.Codes.All,
				Debtor = GatewayDebtor.Codes.ShipmentPickupAgent,
			};

			var collection = new GatewayChargeDefaultDebtorConfigurationCollection();
			collection.AddRange(configuration);

			Helper.ChargeCodes.CreateGlobalCharge("FRT1"); //by default, it's margin charge

			var tariff1 = Helper.NewIntercompanyTariff(sendingAgent);
			var rateEntry1 = CreateFlatRate(tariff1, "FRT1", 0, "AUMEL", "NZAKL");

			var tariff2 = Helper.NewIntercompanyTariff(receivingAgent);
			CreateFlatRate(tariff2, "FRT1", 0, "AUMEL", "NZAKL");

			var consol = CreateForwardingConsolWithGatewayAgents("AUSYD", "AUBNE", sendingAgent, receivingAgent, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "AUSYD", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, "AUBNE", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment = CreateGatewayShipment(consol, "AUMEL", "NZAKL", false, false);
			shipment.PickupAgentPK = pickupAgent.PK;

			Factory.Save();

			var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA"));

			using (AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultDebtorConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sydBranch.PK.ToGuid(), gatewayDepartment.PK.ToGuid()))
			using (var shipmentJob = new Job.Loader(shipment).TryCreateWithMutex())
			using (var consolJob = new Job.Loader(consol).TryCreateWithMutex())
			{
				sydBranch.Company.GC_IsGSTRegistered = false;
				shipmentJob.JH_GB = melBranch.PK; // setting shipment branch helps to set the Creditor of the charge to proxy of this branch through setting internal job number
				consolJob.JH_GB = sydBranch.PK;
				consolJob.JH_GE = gatewayDepartment.PK;

				pickupAgent.OH_IsDebtor = true;
				receivingAgent.OH_IsCreditor = true;
				sendingAgent.OH_IsCreditor = true;

				Factory.Save();

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT1",
						JR_LocalSellAmt = 0,
						SellAccountCode = pickupAgent.OH_Code,
						JR_LocalCostAmt = 0,
						CostAccountCode = string.Empty,
						RelatedJobNumber = shipment.JS_UniqueConsignRef,
					}
				};

				// autorate revenue, only debtor should be set as creditor has a conflict
				AutorateAndAssertWithUserContextOverride(expected, consol, sendingAgent, autorateCosts: false, autorateRevenue: true);

				rateEntry1.RateLines[0].GetCalculator<FlatCalculator>().BaseRate = 10m;
				Factory.Save();

				expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT1",
						JR_LocalSellAmt = 10m,
						SellAccountCode = pickupAgent.OH_Code,
						JR_LocalCostAmt = 10m,
						CostAccountCode = string.Empty,
						RelatedJobNumber = shipment.JS_UniqueConsignRef
					}
				};

				// re-autorate revenue, although now cost side has an amount due to margin charge, but only debtor should be set as creditor has a conflict
				AutorateAndAssertWithUserContextOverride(expected, consol, sendingAgent, autorateCosts: false, autorateRevenue: true, job: consolJob);
			}
		}

		public void TestAutorateCost_ThenCostAndRevenue_BothCreditorAndDebtorAreOrgProxies_ShouldSetCreditorDebtorCorrectly()
		{
			var (sendingAgent, sydBranch) = CreateCompanyAndBranchProxy("AUSYD");
			var (receivingAgent, _) = CreateBranchProxy("AUBNE", sydBranch.Company);
			var (pickupAgent, melBranch) = CreateBranchProxy("AUMEL", sydBranch.Company);

			var configuration = new GatewayChargeDefaultDebtorConfiguration()
			{
				ConsolDirection = "ALL",
				ConsolTransportMode = "ALL",
				ChargeGroup = "ALL",
				ConsolPaymentTerm = "ALL",
				RelatedJob = GatewayRelatedJob.Codes.RelatedToShipment,
				PreviousSendingAgent = GatewayPreviousSendingAgent.Codes.All,
				Debtor = GatewayDebtor.Codes.ShipmentPickupAgent,
			};

			var collection = new GatewayChargeDefaultDebtorConfigurationCollection();
			collection.AddRange(configuration);

			var frtCharge = Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			frtCharge.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;

			var tariff1 = Helper.NewIntercompanyTariff(sendingAgent); // revenue
			CreateFlatRate(tariff1, "FRT1", 10, "AUMEL", "NZAKL");

			var tariff2 = Helper.NewIntercompanyTariff(receivingAgent); // cost
			CreateFlatRate(tariff2, "FRT1", 7, "AUMEL", "NZAKL");

			var consol = CreateForwardingConsolWithGatewayAgents("AUSYD", "AUBNE", sendingAgent, receivingAgent, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "AUSYD", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, "AUBNE", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment = CreateGatewayShipment(consol, "AUMEL", "NZAKL", false, false);
			shipment.PickupAgentPK = pickupAgent.PK;

			Factory.Save();

			var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA"));

			using (AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultDebtorConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sydBranch.PK.ToGuid(), gatewayDepartment.PK.ToGuid()))
			using (var consolJob = new Job.Loader(consol).TryCreateWithMutex(sydBranch))
			using (new Job.Loader(shipment).TryCreateWithMutex(sydBranch))
			{
				sydBranch.Company.GC_IsGSTRegistered = false;
				consolJob.JH_GB = sydBranch.PK;
				consolJob.JH_GE = gatewayDepartment.PK;

				pickupAgent.OH_IsDebtor = true;
				receivingAgent.OH_IsCreditor = true;
				sendingAgent.OH_IsCreditor = true;

				Factory.Save();

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT1",
						JR_LocalCostAmt = 7m,
						CostAccountCode = pickupAgent.OH_Code,
						JR_LocalSellAmt = 0m,
						SellAccountCode = string.Empty,
						JR_GB_InternalBranch = melBranch.PK,
						RelatedJobNumber = shipment.JS_UniqueConsignRef,
					}
				};

				// only creditor should be set
				AutorateAndAssertWithUserContextOverride(expected, consol, sendingAgent, autorateCosts: true, autorateRevenue: false, job: consolJob);

				var jobCharge = consolJob.Charges[0];
				jobCharge.JR_LocalCostAmt = 7.8m;

				AssertEquals(true, jobCharge.JR_CostRated);
				AssertEquals("as we manually overriden the cost amount, then the flag should be true", true, jobCharge.JR_CostRatingOverride);

				jobCharge.JR_Calc_CostRatingBehavior = JobChargeLookups.ReAutorateCharge;

				expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT1",
						JR_LocalSellAmt = 10m,
						JR_LocalCostAmt = 7m,
						CostAccountCode = pickupAgent.OH_Code,
						SellAccountCode = string.Empty,
						RelatedJobNumber = shipment.JS_UniqueConsignRef,
					}
				};

				// since both creditor and debtor are org proxies, only creditor can be set
				AutorateAndAssertWithUserContextOverride(expected, consol, sendingAgent, autorateCosts: true, autorateRevenue: true, job: consolJob);
			}
		}

		public void TestAutorateRevenue_ThenCostAndRevenue_BothCreditorAndDebtorAreOrgProxies_ShouldSetCreditorDebtorCorrectly()
		{
			var (sendingAgent, bneBranch) = CreateCompanyAndBranchProxy("AUBNE");
			var (receivingAgent, _) = CreateBranchProxy("AUSYD", bneBranch.Company);
			var (pickupAgent, melBranch) = CreateBranchProxy("AUMEL", bneBranch.Company);

			var configuration = new GatewayChargeDefaultDebtorConfiguration()
			{
				ConsolDirection = "ALL",
				ConsolTransportMode = "ALL",
				ChargeGroup = "ALL",
				ConsolPaymentTerm = "ALL",
				RelatedJob = GatewayRelatedJob.Codes.RelatedToShipment,
				PreviousSendingAgent = GatewayPreviousSendingAgent.Codes.All,
				Debtor = GatewayDebtor.Codes.ShipmentPickupAgent,
			};

			var collection = new GatewayChargeDefaultDebtorConfigurationCollection();
			collection.AddRange(configuration);

			var frtCharge = Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			frtCharge.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;

			var tariff1 = Helper.NewIntercompanyTariff(sendingAgent); // revenue
			CreateFlatRate(tariff1, "FRT1", 10, "AUMEL", "NZAKL");

			var tariff2 = Helper.NewIntercompanyTariff(receivingAgent); // cost
			CreateFlatRate(tariff2, "FRT1", 7, "AUMEL", "NZAKL");

			var consol = CreateForwardingConsolWithGatewayAgents("AUBNE", "AUSYD", sendingAgent, receivingAgent, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "AUBNE", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, "AUSYD", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment = CreateGatewayShipment(consol, "AUMEL", "NZAKL", false, false);
			shipment.PickupAgentPK = pickupAgent.PK;

			Factory.Save();

			var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA"));

			using (AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultDebtorConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, bneBranch.PK.ToGuid(), gatewayDepartment.PK.ToGuid()))
			using (var shipmentJob = new Job.Loader(shipment).TryCreateWithMutex())
			using (var consolJob = new Job.Loader(consol).TryCreateWithMutex())
			{
				bneBranch.Company.GC_IsGSTRegistered = false;
				shipmentJob.JH_GB = melBranch.PK; // setting shipment branch helps to set the Creditor of the charge to proxy of this branch through setting internal job number
				consolJob.JH_GB = bneBranch.PK;
				consolJob.JH_GE = gatewayDepartment.PK;

				pickupAgent.OH_IsDebtor = true;
				receivingAgent.OH_IsCreditor = true;
				sendingAgent.OH_IsCreditor = true;

				Factory.Save();

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT1",
						JR_LocalCostAmt = 0m,
						CostAccountCode = string.Empty,
						JR_LocalSellAmt = 10m,
						SellAccountCode = pickupAgent.OH_Code,
						JR_GB_InternalBranch = melBranch.PK,
						RelatedJobNumber = shipment.JS_UniqueConsignRef,
					}
				};

				// creditor should not be set as we only autorated revenue and charge is MJA
				AutorateAndAssertWithUserContextOverride(expected, consol, sendingAgent, autorateCosts: false, autorateRevenue: true, job: consolJob);

				expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT1",
						JR_LocalSellAmt = 10m,
						JR_LocalCostAmt = 7m,
						CostAccountCode = pickupAgent.OH_Code,
						SellAccountCode = string.Empty,
						RelatedJobNumber = shipment.JS_UniqueConsignRef,
					}
				};

				// since both creditor and debtor are org proxies, now debtor should be removed and only creditor can be set
				AutorateAndAssertWithUserContextOverride(expected, consol, sendingAgent, autorateCosts: true, autorateRevenue: true, job: consolJob);
			}
		}

		public void TestAutorateRevenue_WithExistingCostCharge_ShouldSetCreditorDebtorCorrectly()
		{
			var (sendingAgent, sydBranch) = CreateCompanyAndBranchProxy("AUSYD");
			var (receivingAgent, _) = CreateBranchProxy("AUBNE", sydBranch.Company);
			var (pickupAgent, melBranch) = CreateBranchProxy("AUMEL", sydBranch.Company);

			var configuration = new GatewayChargeDefaultDebtorConfiguration()
			{
				ConsolDirection = "ALL",
				ConsolTransportMode = "ALL",
				ChargeGroup = "ALL",
				ConsolPaymentTerm = "ALL",
				RelatedJob = GatewayRelatedJob.Codes.RelatedToShipment,
				PreviousSendingAgent = GatewayPreviousSendingAgent.Codes.All,
				Debtor = GatewayDebtor.Codes.ShipmentPickupAgent,
			};

			var collection = new GatewayChargeDefaultDebtorConfigurationCollection();
			collection.AddRange(configuration);

			var frtCharge = Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			frtCharge.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;

			var tariff1 = Helper.NewIntercompanyTariff(sendingAgent); // revenue
			CreateFlatRate(tariff1, "FRT1", 10, "AUMEL", "NZAKL");

			var tariff2 = Helper.NewIntercompanyTariff(receivingAgent); // cost
			CreateFlatRate(tariff2, "FRT1", 7, "AUMEL", "NZAKL");

			var consol = CreateForwardingConsolWithGatewayAgents("AUSYD", "AUBNE", sendingAgent, receivingAgent, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "AUSYD", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, "AUBNE", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment = CreateGatewayShipment(consol, "AUMEL", "NZAKL", false, false);
			shipment.PickupAgentPK = pickupAgent.PK;

			Factory.Save();

			var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA"));

			using (AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultDebtorConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sydBranch.PK.ToGuid(), gatewayDepartment.PK.ToGuid()))
			using (var shipmentJob = new Job.Loader(shipment).TryCreateWithMutex())
			using (var consolJob = new Job.Loader(consol).TryCreateWithMutex())
			{
				sydBranch.Company.GC_IsGSTRegistered = false;
				shipmentJob.JH_GB = melBranch.PK; // setting shipment branch helps to set the Creditor of the charge to proxy of this branch through setting internal job number
				consolJob.JH_GB = sydBranch.PK;
				consolJob.JH_GE = gatewayDepartment.PK;

				pickupAgent.OH_IsDebtor = true;
				TransportProvider1.OH_IsCreditor = true;

				ZQuery filter = new ZQuery(); // creating global charge, will create charge code for each company as well. here we want to load the one for the login company as the same is used in AutoRating
				filter.AddToFilter(AccChargeCodeSchema.AC_Code, "FRT1");
				filter.AddToFilter(AccChargeCodeSchema.AC_GC, sydBranch.Company.PK);

				var fRTChargeInAUCompany = Factory.LoadTop1<AccChargeCode>(filter);

				var jobCharge = consolJob.Charges.AddNew();
				jobCharge.JR_AC = fRTChargeInAUCompany.PK;
				jobCharge.JR_LocalCostAmt = 7.8m;
				jobCharge.JR_OH_CostAccount = TransportProvider1.PK;
				jobCharge.JR_Calc_RelatedJobNumber = shipment.JS_UniqueConsignRef;

				jobCharge.JR_Calc_SellRatingBehavior = JobChargeLookups.ReAutorateCharge;

				Factory.Save();

				Assert("Creditor is not OrgProxy", !jobCharge.CostAccountIsOrgProxy);

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT1",
						JR_LocalCostAmt = 7.8m,
						CostAccountCode = TransportProvider1.OH_Code,
						JR_LocalSellAmt = 10m,
						SellAccountCode = pickupAgent.OH_Code,
						JR_GB_InternalBranch = melBranch.PK,
						RelatedJobNumber = shipment.JS_UniqueConsignRef,
					}
				};

				// since creditor is not an orxproxy, then debtor can be set properly
				AutorateAndAssertWithUserContextOverride(expected, consol, sendingAgent, autorateCosts: false, autorateRevenue: true, job: consolJob);
			}
		}

		public void TestAutorateCostAndRevenue_BothCreditorAndDebtorAreOrgProxiesButNotInTheSameCompany_ShouldSetBothCreditorAndDebtor()
		{
			var (sendingAgent, sydBranch) = CreateCompanyAndBranchProxy("AUSYD");
			var (receivingAgent, _) = CreateCompanyAndBranchProxy("SGSIN");
			var (pickupAgent, melBranch) = CreateNonProxyBranch("AUMEL", sydBranch.Company);

			var configuration = new GatewayChargeDefaultDebtorConfiguration()
			{
				ConsolDirection = "ALL",
				ConsolTransportMode = "ALL",
				ChargeGroup = "ALL",
				ConsolPaymentTerm = "ALL",
				RelatedJob = GatewayRelatedJob.Codes.RelatedToShipment,
				PreviousSendingAgent = GatewayPreviousSendingAgent.Codes.All,
				Debtor = GatewayDebtor.Codes.ShipmentPickupAgent,
			};

			var collection = new GatewayChargeDefaultDebtorConfigurationCollection();
			collection.AddRange(configuration);

			var charge = Helper.ChargeCodes.CreateGlobalCharge("FRT1");
			charge.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;

			var intercompanyTariff = Helper.NewIntercompanyTariff(sendingAgent); // revenue
			CreateFlatRate(intercompanyTariff, "FRT1", 20, "AUMEL", "SGSIN");

			var tariff2 = Helper.NewIntercompanyTariff(receivingAgent); // cost
			CreateFlatRate(tariff2, "FRT1", 30, "AUMEL", "SGSIN");

			var consol = CreateForwardingConsolWithGatewayAgents("AUSYD", "SGSIN", sendingAgent, receivingAgent, checkIsItGateway: false);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "AUSYD", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, "SGSIN", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var shipment = CreateGatewayShipment(consol, "AUMEL", "SGSIN", false, false);
			shipment.PickupAgentPK = pickupAgent.PK;

			Factory.Save();

			var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA"));

			using (AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultDebtorConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, sydBranch.PK.ToGuid(), gatewayDepartment.PK.ToGuid()))
			using (var shipmentJob = new Job.Loader(shipment).TryCreateWithMutex())
			using (var consolJob = new Job.Loader(consol).TryCreateWithMutex())
			{
				sydBranch.Company.GC_IsGSTRegistered = false;
				shipmentJob.JH_GB = melBranch.PK; // setting shipment branch helps to set the Creditor of the charge to proxy of this branch through setting internal job number
				consolJob.JH_GB = sydBranch.PK;
				consolJob.JH_GE = gatewayDepartment.PK;

				pickupAgent.OH_IsDebtor = true;
				receivingAgent.OH_IsCreditor = true;
				sendingAgent.OH_IsCreditor = true;
				sendingAgent.OH_IsDebtor = true;

				Factory.Save();

				var expected = new[]
				{
					new AssertionCharge
					{
						ChargeCode = "FRT1",
						JR_LocalSellAmt = 20m,
						SellAccountCode = pickupAgent.OH_Code,
						JR_LocalCostAmt = 30m,
						CostAccountCode = receivingAgent.OH_Code,
						RelatedJobNumber = shipment.JS_UniqueConsignRef,
					}
				};

				AutorateAndAssertWithUserContextOverride(expected, consol, sendingAgent, autorateCosts: true, autorateRevenue: true);
			}
		}

		public void TestAutorateRevenueAndCost_GatewayConsol_ShipmentJobShouldBeCreated()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("DDOC");
			Helper.ChargeCodes.CreateGlobalCharge("DDOC");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;

			Helper.NewIntercompanyTariff(sendingAgent).AddRateEntryWithFlatRateLine("DST", "LSE", "", "DEHAM", "DDOC", 3m);

			var consol = CreateForwardingConsolWithGatewayAgents("AUSYD", "DEHAM", sendingAgent: sendingAgent, receivingAgent: null);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "AUSYD", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol.JK_OA_ShippingLineAddress = sendingAgent.MainAddress.PK;
			consol.JK_PrepaidCollect = PaymentType.Collect;

			var sydneyShipment = CreateGatewayShipment(consol, "AUSYD", "DEHAM", isCollect: true, withJob: false);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 3m,
					RelatedJobNumber = sydneyShipment.JS_UniqueConsignRef
				}
			};

			AutorateAndAssert(expected, consol, null, autorateRevenue: true, autorateCosts: true);

			var jobToDispose = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, sydneyShipment.PK));

			AssertNotNull("Job shipment should be created to make sure Internal Job default correctly", jobToDispose);

			jobToDispose.Dispose();
		}

		public void TestJobShipment_WithGatewayBilling_IsCreatedByApportionmentsListingLoading_AndDispose()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("DDOC");
			Helper.ChargeCodes.CreateGlobalCharge("DDOC");

			var sendingAgent = GlbCompany.CurrentCompany.OrgProxy;

			Helper.NewIntercompanyTariff(sendingAgent).AddRateEntryWithFlatRateLine("DST", "LSE", "", "DEHAM", "DDOC", 3m);

			var consol = CreateForwardingConsolWithGatewayAgents("AUSYD", "DEHAM", sendingAgent: sendingAgent, receivingAgent: null);
			SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, "AUSYD", AgentStatusList.Codes.GatewayAgentWithTariff);
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol.JK_OA_ShippingLineAddress = sendingAgent.MainAddress.PK;
			consol.JK_PrepaidCollect = PaymentType.Collect;

			var sydneyShipment = CreateGatewayShipment(consol, "AUSYD", "DEHAM", isCollect: true, withJob: false);

			Factory.Save();

			Assert(consol.IsGatewayBillingEnabled());

			var apportionmentsListing = consol.GetApportionments(true);
			apportionmentsListing.LoadChildShipmentsAndAcquireMutexesWhereRequired();

			var jobShipment = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, sydneyShipment.PK));

			AssertNotNull("apportionmentsListing.LoadChildShipmentsAndAcquireMutexesWhereRequired() will create jobheader for shipment", jobShipment);

			// In production behaviour, when LoadChildShipmentsAndAcquireMutexesWhereRequired run, job will be created in memory. User can choose to save it or not to save it.
			// if job is saved, no diposal needed. If job is not saved by either reload the form or click NO on Save dialog, job will be disposed. For test purpose, job will be disposed manually

			jobShipment.Dispose();
		}
		#endregion

		#region Helper Methods

		AccChargeCode CreateGlobalCharge(
			ZString code,
			string calculator = FlatCalculator.Code,
			string chargeGroup = ChargeCodeGroupList.Codes.Freight,
			bool isConsolLevel = false)
		{
			var chargeCode = Helper.ChargeCodes.CreateGlobalCharge(code, calculator, chargeGroup);
			chargeCode.AC_IsGroupageCharge = isConsolLevel;

			return chargeCode;
		}

		void AutorateAndAssertWithUserContextOverride<T>(
			IEnumerable<AssertionCharge> expected,
			T jobParent,
			OrgHeader localClient,
			OrgHeader agent = null,
			GlbBranch branch = null,
			bool autorateRevenue = true,
			bool autorateCosts = true,
			string[] expectedErrors = null,
			Job job = null,
			IRatingContext ratingContext = null)
			where T : IJobHeaderParent, IBusiness
		{
			AssertAutorating(
				branch,
				() => AutorateAndAssert(expected, jobParent, localClient, agent, job, autorateRevenue: autorateRevenue,
					autorateCosts: autorateCosts, expectedErrors: expectedErrors, ratingContext: ratingContext));

			if (jobParent is ForwardingConsol consol)
			{
				foreach (var shipment in consol.CostSupporter?.ShipmentsList)
				{
					var jobToDisposes = Factory.Load<Job>(new ZQuery(JobHeaderSchema.JH_ParentID, shipment.PK));

					if (jobToDisposes.Length > 0)
					{
						foreach (var jobToDispose in jobToDisposes)
						{
							jobToDispose.Dispose();
						}
					}
				}
			}
		}

		void AssertAutoratingAuditLogNoteContainsLinesWithUserContextOverride(BusinessObject obj, GlbBranch branch, params string[] expectedLines) =>
			AssertAutorating(branch, () => AssertAutoratingAuditLogNoteContainsLines(obj, "Expect a message", expectedLines));

		void AssertAutoratingAuditLogNoteNOTContainsLinesWithUserContextOverride(BusinessObject obj, GlbBranch branch, string expectedNote) =>
			AssertAutorating(branch, () => AssertAutoratingAuditLogNotContains(obj, expectedNote));

		void AutorateJobInvoicingCostAndRevenueAndAssertWithUserContextOverride(
			string message,
			Dictionary<IJobInvoicingPlugIn,
			IEnumerable<AssertionCharge>> expectedCharges,
			IEnumerable<AssertionCost> expectedCosts,
			IGenericJobCostPlugIn costsSupporter,
			GlbBranch branch = null,
			bool autorateRevenue = true,
			bool autorateCosts = true,
			bool deleteExistingCosts = true,
			string[] expectedWarnings = null,
			string[] expectedErrors = null)
			=>
				AssertAutorating(
					branch,
					() => AutoCostAndAssert(message, expectedCharges, expectedCosts, costsSupporter, autorateCosts: autorateCosts,
						autorateRevenue: autorateRevenue, deleteExistingCosts: deleteExistingCosts,
						expectedWarnings: expectedWarnings, expectedErrors: expectedErrors));

		void AssertAutorating(GlbBranch branch, Action assertionMethod)
		{
			var branchPK = branch != null ? branch.PK.ToGuid() : Env.CurrentBranchPK;
			var nonMiscDepartmentPK = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA")).PK.ToGuid();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchPK, nonMiscDepartmentPK))
			{
				assertionMethod();
			}
		}

		void AutorateAndAssertRatesService<T>(string message, IEnumerable<AssertionCharge> expected, T jobParent, MockRatesServiceContext ratingContext, OrgHeader agent = null, Job job = null, bool expectNoErrorsLogged = true) where T : IJobHeaderParent, IBusiness
		{
			UnitTestUserNotification.Instance.ClearMessages();
			var createNewJob = job == null;

			using (job = job ?? new JobHeader.Loader(jobParent).TryLoadOrCreate() as Job)
			{
				if (createNewJob)
				{
					job.Charges.RemoveAndDeleteAll();
					job.ExchangeRates.RemoveAndDeleteAll();
					job.JH_OA_LocalChargesAddr = NewClient.MainAddress.PK;

					if (agent != null)
					{
						job.JH_OA_AgentCollectAddr = agent.MainAddress.PK;
					}
				}

				var starter = new AutoRatingStarter(new[] { (IBusiness)jobParent }, ratingContext);
				starter.ExecuteAutorating(AutoRateOptions.AutorateCosts.With(billingType: BillingType.Invoicing));

				if (expectNoErrorsLogged)
				{
					AssertContainsExactElementsInAnyOrder("Autorating is supposed to log no Errors", Enumerable.Empty<string>(), ratingContext.TestLogger.Errors);
				}

				message = string.Concat(message, "\r\n", ratingContext.TestLogger);
				AssertCharges(message, expected, job);
			}
		}

		(OrgHeader org, GlbBranch branch) CreateNonProxyBranch(string unloco, GlbCompany company = null)
		{
			var org = Helper.CreateCreditor("Org" + unloco);
			org.OH_IsDebtor = true;
			org.MainAddress.OA_RL_NKRelatedPortCode = unloco;
			Factory.Save();

			var countryCode = unloco.Substring(0, 2);

			if (company == null)
			{
				company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_Name = countryCode + " Company";
				company.GC_RN_NKCountryCode = countryCode;
				company.GC_OH_OrgProxy = Helper.NewOrgHeader().PK;
			}

			var newBranch = company.Branches.AddNew();
			newBranch.GB_Code = unloco.Substring(1, 3);
			newBranch.GB_BranchName = unloco + " Branch";
			newBranch.GB_RL_NKHomePort = unloco;
			Factory.Save();

			return (org, newBranch);
		}

		(OrgHeader proxy, GlbBranch branch) CreateBranchProxy(string unloco, GlbCompany company = null)
		{
			var proxy = Helper.CreateCreditor("PROXY" + unloco);
			proxy.OH_IsDebtor = true;
			proxy.MainAddress.OA_RL_NKRelatedPortCode = unloco;
			Factory.Save();

			var newBranch = company?.Branches.AddNew() ?? GlbCompany.CurrentCompany.Branches.AddNew();
			newBranch.GB_Code = unloco.Substring(1, 3);
			newBranch.GB_BranchName = unloco + " Branch";
			newBranch.GB_OH_OrgProxy = proxy.PK;
			newBranch.GB_RL_NKHomePort = unloco;
			newBranch.Factory.Save();

			return (proxy, newBranch);
		}

		(OrgHeader proxy, GlbBranch branch) CreateCompanyAndBranchProxy(string unloco)
		{
			var countryCode = unloco.Substring(0, 2);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = countryCode + " Company";
			company.GC_RN_NKCountryCode = countryCode;

			var newBranch = company.Branches.AddNew();
			newBranch.GB_Code = company.GC_Code;
			newBranch.GB_BranchName = unloco + " Branch";
			newBranch.GB_RL_NKHomePort = unloco;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, newBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var proxy = Helper.CreateCreditor("PROXY" + unloco);
				proxy.OH_IsDebtor = true;
				proxy.MainAddress.OA_RL_NKRelatedPortCode = unloco;
				newBranch.GB_OH_OrgProxy = proxy.PK;

				return (proxy, newBranch);
			}
		}

		ForwardingConsol CreateForwardingConsolWithGatewayAgents(string origin
			, string destination
			, OrgHeader sendingAgent
			, OrgHeader receivingAgent
			, bool checkIsItGateway = true)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.JK_PrepaidCollect = PaymentType.Collect;

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = origin;
			transport.JW_RL_NKDiscPort = destination;
			transport.JW_ETD = ZDateTime.Now.AddDays(10);
			transport.JW_ETA = ZDateTime.Now.AddDays(13);
			transport.JW_VoyageFlight = "QF105";

			if (sendingAgent != null)
			{
				consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;
				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, AgentStatusList.Codes.GatewayAgent);
			}

			if (receivingAgent != null)
			{
				consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;
				consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, destination, AgentStatusList.Codes.GatewayAgent);
			}

			consol.JK_AgentType = AgentType.Agent;

			if (checkIsItGateway)
			{
				Assert("Pre-condition: consol.IsGateway", consol.IsGateway());

				var gatewayAgents = ((IGateway)consol).GatewayBillingSupporter.GatewayAgent();
				if (sendingAgent != null && sendingAgent.IsProxyOrg(GlbCompany.CurrentCompany))
				{
					AssertEquals("Pre-condition: sendingAgent is Gateway", sendingAgent.PK, gatewayAgents.sendingAgent.PK);
				}

				if (receivingAgent != null && receivingAgent.IsProxyOrg(GlbCompany.CurrentCompany))
				{
					AssertEquals("Pre-condition: receivingAgent is Gateway", receivingAgent.PK, gatewayAgents.receivingAgent.PK);
				}
			}

			return consol;
		}

		ForwardingShipment CreateGatewayShipment(ForwardingConsol gatewayConsol, string origin, string destination, bool isCollect, bool withJob = true, OrgHeader consignor = null, OrgHeader consignee = null, GlbBranch branch = null)
		{
			var shipment = gatewayConsol.Shipments.AddNew();
			shipment.ConsignorPK = (consignor ?? Consignor).PK;
			shipment.ConsigneePK = (consignee ?? Consignee).PK;

			shipment.JS_INCO = isCollect ? IncoTerms.ExWorks : IncoTerms.DeliveredDutyPaid;
			shipment.JS_RL_NKOrigin = !origin.IsNullOrEmpty() ? (ZString)origin : gatewayConsol.JK_RL_NKLoadPort;
			shipment.JS_RL_NKDestination = !destination.IsNullOrEmpty() ? (ZString)destination : gatewayConsol.JK_RL_NKDischargePort;

			if (withJob)
			{
				var job = CreateJob(shipment, shipment.JS_UniqueConsignRef, branch);

				if (shipment.IsCrossTrade() && AccountingMasterFilesRegistry.Instance.EnableCrossTradeDebtorDefaultingFunctionality.Value)
				{
					AssertEquals(consignor ?? Consignor, job.LocalCharges);
				}
				else
				{
					AssertEquals(consignee ?? Consignee, job.LocalCharges);
				}
			}

			return shipment;
		}

		ForwardingShipment CreateStandaloneGatewayShipment(string origin, string destination, bool isCollect, params ZGuid[] gatewayAgents)
		{
			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, origin, destination, 1000);
			shipment.JS_INCO = isCollect ? IncoTerms.ExWorks : IncoTerms.DeliveredDutyPaid;
			foreach (var agent in gatewayAgents)
			{
				shipment.Gateways.AddNew().ForwarderPK = agent;
			}

			return shipment;
		}

		ForwardingShipment CreateGatewayShipmentWithMultipleGatewayConsols(string origin, string destination, bool isCollect, params ForwardingConsol[] gatewayConsols)
		{
			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, origin, destination, 1000);
			shipment.JS_INCO = isCollect ? IncoTerms.ExWorks : IncoTerms.DeliveredDutyPaid;
			shipment.Consols.AddRange(gatewayConsols);

			return shipment;
		}

		void SetUpAppointedGatewayAgentPorts(OrgAddress gatewayAgentAddress, ZString location, string handlingType)
		{
			var gatewayAgent = gatewayAgentAddress.Header;
			var agentPorts = gatewayAgent.AppointedGatewayAgentPorts.Cast<OrgAppointedAgentPorts>().FirstOrDefault(x => x.O5_PortOrCountry == location)
							?? gatewayAgent.AppointedGatewayAgentPorts.AddNew();
			agentPorts.O5_OA_AgentOfficeAddress = gatewayAgentAddress.PK;
			agentPorts.O5_PortOrCountry = location;
			agentPorts.O5_AgentDirection = AgentDirectionList.Codes.Both;
			agentPorts.O5_SeaAgentStatus = handlingType;
			agentPorts.O5_AirAgentStatus = handlingType;
			agentPorts.O5_RailAgentStatus = handlingType;
			agentPorts.O5_RoadAgentStatus = handlingType;
		}

		ForwardingShipment GetShipmentWithSendingReceivingAgents(ForwardingConsol consol)
		{
			var orgProxy = CreateBranchProxy("AUSYD");
			var shipment = CreateGatewayShipment(consol, "CHBSL", "USHOU", true);
			var shipmentGateway = shipment.Gateways.AddNew();
			shipmentGateway.JSG_OA_ForwarderAddress = orgProxy.proxy.MainAddress.PK;
			shipmentGateway.JSG_Sequence = 1;
			shipment.Gateways[0].JSG_Sequence = 2;
			shipment.Gateways[1].JSG_Sequence = 3;

			CombineAssertions("First gateway should not equal to consol sending agent", () =>
			{
				AssertEquals(orgProxy.proxy.PK, shipment.Gateways[2].Forwarder.PK);
				AssertEquals("Precondition: First Gateway", (byte)1, shipment.Gateways[2].JSG_Sequence);
				AssertEquals(consol.SendingForwarder.PK, shipment.Gateways[0].Forwarder.PK);
				AssertEquals("Precondition: Second Gateway", (byte)2, shipment.Gateways[0].JSG_Sequence);
				AssertEquals(consol.ReceivingForwarder.PK, shipment.Gateways[1].Forwarder.PK);
				AssertEquals("Precondition: Third Gateway", (byte)3, shipment.Gateways[1].JSG_Sequence);
			});

			shipment.JS_RL_NKFreightRateOrigin = "CHZRH";
			shipment.JS_RL_NKFreightRateDestination = "USHOU";
			shipment.JS_RL_NKLoadPort = "BEBRU";
			shipment.JS_RL_NKDischargePort = "USLGB";

			return shipment;
		}

		RateEntry CreateFlatRate(RatingHeader ratingHeader, ZString chargeCode, ZDecimal baseRate, string origin, string destination, string plannedLoad = "", string plannedDischarge = "", string gatewayAgentType = "")
		{
			var rateEntry = ratingHeader.AddRateEntryWithFlatRateLine(TransportModes.Air, ContainerModes.Loose, origin, destination, chargeCode, baseRate, CurrencyCodes.Australia);
			rateEntry.TI_GatewayAgentType = gatewayAgentType;
			rateEntry.TI_PlannedLoadLRC = plannedLoad;
			rateEntry.TI_PlannedDischargeLRC = plannedDischarge;

			return rateEntry;
		}

		#endregion

		#region SetUp/TearDown

		protected override void SetUp()
		{
			base.SetUp();

			var priorities = new RatesPrioritiesCollection();
			priorities.AddNew(RatingDebtorOrgTypes.SAG);
			priorities.AddNew(RatingDebtorOrgTypes.RAG);

			tempPriorities = RatingDataRegistry.Instance.GatewayCollectPriorities.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, priorities);
			tempAutorateGatewayBilling = RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			tempRatesServiceSearch = DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled());
		}

		protected override void TearDown()
		{
			tempPriorities.Dispose();
			tempAutorateGatewayBilling.Dispose();
			tempRatesServiceSearch.Dispose();

			base.TearDown();
		}

		IDisposable tempPriorities = DisposableAction.NoAction;
		IDisposable tempAutorateGatewayBilling = DisposableAction.NoAction;
		IDisposable tempRatesServiceSearch = DisposableAction.NoAction;

		#endregion
	}
}
