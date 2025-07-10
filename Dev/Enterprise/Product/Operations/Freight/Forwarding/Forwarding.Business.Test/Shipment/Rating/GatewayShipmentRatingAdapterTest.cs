using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using FluentAssertions;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class GatewayShipmentRatingAdapterTest : BaseFreightTest
	{
		public void TestGetDebtors()
		{
			var consol = CreateGatewayConsol();
			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneePK = LocalConsignee.PK;
			shipment.ConsignorPK = LocalConsignor.PK;

			var adapter = new GatewayShipmentRatingAdapter(consol, shipment);

			AssertEquals(3, adapter.DebtorOrgs.Count);
			AssertEquals(adapter.DebtorOrgs[RatingDebtorOrgTypes.CNR], LocalConsignor);
			AssertEquals(adapter.DebtorOrgs[RatingDebtorOrgTypes.CNE], LocalConsignee);
			AssertEquals(adapter.DebtorOrgs[RatingDebtorOrgTypes.SAG], consol.SendingForwarder);

			using (var consolJob = new JobHeader.Loader(consol).TryLoadOrCreateWithMutex())
			{
				consolJob.JH_OA_LocalChargesAddr = LocalForwarder.MainAddress.PK;
				consolJob.JH_OA_AgentCollectAddr = OverseasLocalTransportCo.MainAddress.PK;

				AssertEquals(adapter.DebtorOrgs[RatingDebtorOrgTypes.CNR], LocalConsignor);
				AssertEquals(adapter.DebtorOrgs[RatingDebtorOrgTypes.CNE], LocalConsignee);
				AssertEquals(adapter.DebtorOrgs[RatingDebtorOrgTypes.LC], LocalForwarder);

				AssertEquals("Should prefer the consol's sending forwarder as the sending agent", adapter.DebtorOrgs[RatingDebtorOrgTypes.SAG], consol.SendingForwarder);
				AssertEquals("Receiving agent should come from the consol's agent", adapter.DebtorOrgs[RatingDebtorOrgTypes.RAG], OverseasLocalTransportCo);
				AssertEquals(false, adapter.DebtorOrgs.Any(x => x.RatingDebtorOrgTypes == RatingDebtorOrgTypes.AG));
			}
		}

		public void TestGetContractNumberConfiguration()
		{
			var consol = CreateGatewayConsol();
			var shipment = consol.Shipments.AddNew();

			var adapter = new GatewayShipmentRatingAdapter(consol, shipment);
			var configuration = adapter.GetContractNumberConfiguration(CostSell.Cost);
			configuration.ShouldAddContractNumberQueryFilter.Should().Be(true);
			configuration.ShouldApplySpecificAdapterContractNumberFilter.Should().Be(false);
			configuration.ShouldIgnoreJobClientContractNumbers.Should().Be(false);
			configuration.ShouldIgnoreJobCarrierContractNumbers.Should().Be(false);
			configuration.ShouldMatchJobBlankContractNumber.Should().Be(false);
			configuration.ShouldUseCarrierContractDateFilter.Should().Be(false);

			configuration = adapter.GetContractNumberConfiguration(CostSell.Revenue);
			configuration.ShouldAddContractNumberQueryFilter.Should().Be(true);
			configuration.ShouldApplySpecificAdapterContractNumberFilter.Should().Be(false);
			configuration.ShouldIgnoreJobClientContractNumbers.Should().Be(false);
			configuration.ShouldIgnoreJobCarrierContractNumbers.Should().Be(false);
			configuration.ShouldMatchJobBlankContractNumber.Should().Be(false);
			configuration.ShouldUseCarrierContractDateFilter.Should().Be(false);
		}

		public void TestClientContractNumbers()
		{
			var consol = CreateGatewayConsol();
			var shipment = consol.Shipments.AddNew();
			var jobHeader = new JobHeader.Loader(shipment).TryCreate();
			jobHeader.JH_ClientContractNumber = "TEST";

			var adapter = new GatewayShipmentRatingAdapter(consol, shipment);
			AssertEquals(0, adapter.ClientContractNumbers.Count());
		}

		public void TestUpdateClientContractNumbers_ShouldDoNothing()
		{
			var consol = CreateGatewayConsol();
			var shipment = consol.Shipments.AddNew();

			var adapter = new GatewayShipmentRatingAdapter(consol, shipment);
			AssertEquals(DataUpdateResult.NoAction, adapter.UpdateClientContractNumber(new[] { "BLAH" }));
		}

		public void TestAdapterTypeAndID()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = CreateGatewayConsol();
			shipment.Consols.Add(consol);
			var adapter = new GatewayShipmentRatingAdapter(consol, shipment);

			AssertEquals(AdapterType.Shipment, adapter.AdapterType);
			AssertEquals(shipment.JS_UniqueConsignRef, adapter.OperationalJobCode);
		}

		public void TestShipmentDoesNotHaveGatewayConsols()
		{
			var shipment = Factory.New<ForwardingShipment>();
			AssertExceptionThrown<ArgumentNullException>(() => new GatewayShipmentRatingAdapter(null, shipment));
		}

		public void TestGetVia()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = CreateGatewayConsol();
			shipment.Consols.Add(consol);
			var adapter = new GatewayShipmentRatingAdapter(consol, shipment);

			AssertEquals("AUBNE", adapter.GetVia(CostSell.Cost).Code);
			AssertEquals("AUBNE", adapter.GetVia(CostSell.Revenue).Code);
		}

		public void TestChargeCodeGroups()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = CreateGatewayConsol();
			var freightRatedCodes = Env.Registry.Rating.FreightRatedCodes;
			AssertContainsExactElementsInAnyOrder(freightRatedCodes, new GatewayShipmentRatingAdapter(consol, shipment).ChargeCodeGroups);
		}

		public void TestShipmentPlannedLoadPlannedDischarge()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKLoadPort = "BEBRU";
			shipment.JS_RL_NKDischargePort = "USLGB";

			var consol = CreateGatewayConsol();
			shipment.Consols.Add(consol);
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "AUMEL";

			var adapter = new GatewayShipmentRatingAdapter(consol, shipment);

			AssertEquals("AUMEL", adapter.PlannedLoad(CostSell.Cost).Code);
			AssertEquals("USLGB", adapter.PlannedDischarge(CostSell.Cost).Code);

			AssertEquals("AUSYD", adapter.PlannedLoad(CostSell.Revenue).Code);
			AssertEquals("AUMEL", adapter.PlannedDischarge(CostSell.Revenue).Code);
		}

		#region IGateway

		public void TestIGateway_IsIntercompanyTariffApplicable()
		{
			var port = Factory.New<OrgAppointedAgentPorts>();
			port.O5_PortOrCountry = "AUBNE";
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var orgProxyAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_OA_SendingForwarderAddress = orgProxyAddress;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(port);

			Assert("Pre-condition", consol.IsGateway());

			var shipment = Factory.New<ForwardingShipment>();
			shipment.Consols.Add(consol);

			var adapter = new GatewayShipmentRatingAdapter(consol, shipment);
			var gateway = (IGateway)adapter;

			using (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(false, gateway.IsIntercompanyTariffApplicable(BillingType.Invoicing, CostSell.Revenue));
			}

			using (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(false, gateway.IsIntercompanyTariffApplicable(BillingType.Apportionment, CostSell.Cost));
				AssertEquals(true, gateway.IsIntercompanyTariffApplicable(BillingType.Apportionment, CostSell.Revenue));
				AssertEquals(true, gateway.IsIntercompanyTariffApplicable(BillingType.Invoicing, CostSell.Cost));
			}
		}

		public void TestIGateway()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = Factory.New<ForwardingShipment>();
			var adapter = new GatewayShipmentRatingAdapter(consol, shipment);
			var gateway = (IGateway)adapter;
			var agentType = "test";
			var gatewayAgentPk = new ZGuid();

			AssertType<ForwardingConsolGatewayBillingSupporter>(gateway.GatewayBillingSupporter);
			AssertEquals(((IGateway)consol).GatewayBillingSupporter, gateway.GatewayBillingSupporter);

			AssertContainsExactElementsInAnyOrder(new List<ZGuid>(), gateway.SortedGatewayAgentPKs);

			AssertContinueAutoRateCosting(gateway, consol, BillingType.Apportionment);
			AssertContinueAutoRateCosting(gateway, consol, BillingType.Default);
			AssertContinueAutoRateCosting(gateway, consol, BillingType.EqualizeAndRate);
			AssertContinueAutoRateCosting(gateway, consol, BillingType.Invoicing);
			AssertContinueAutoRateCosting(gateway, consol, BillingType.PrintInvoicing);

			AssertEquals(false, gateway.ShouldRemoveNonIntercompanyTariffFRTEntries(BillingType.Default));

			AssertEquals(string.Empty, gateway.GatewayAgentTypeFilteredReason(agentType, gatewayAgentPk, BillingType.Default, CostSell.Cost));

			AssertContains($"Shipment gateways is empty or does not contain Consol's agent",
				gateway.GatewayAgentTypeFilteredReason(agentType, gatewayAgentPk, BillingType.Default, CostSell.Revenue));

			agentType = GatewayAgentType.Codes.ReceivingAgent;
			AssertContains
			(
				$"Gateway agent type {agentType} cannot be used for gateway shipment autorating",
				gateway.GatewayAgentTypeFilteredReason(agentType, gatewayAgentPk, BillingType.Invoicing, CostSell.Cost)
			);

			AssertContainsExactElementsInAnyOrder(new List<LocationWithSource>(), gateway.SortedOverridenPlannedLoad);
			AssertContainsExactElementsInAnyOrder(new List<LocationWithSource>(), gateway.SortedOverridenPlannedDischarge);

			Assert("should be false for gateway shipment adapter", !gateway.IsContainerNegotiatedCostApplicable(CostSell.Cost));
			Assert("should be false for gateway shipment adapter", !gateway.IsContainerNegotiatedCostApplicable(CostSell.Revenue));

			Assert("should be true for autorating cost of gateway shipmentadapter", gateway.IsGatewaySellApplicableToGatewayConsol(CostSell.Cost));
			Assert("should be true for autorating revenue of gateway shipment adapter since there is no gateway", gateway.IsGatewaySellApplicableToGatewayConsol(CostSell.Revenue));

			var gateway1 = shipment.Gateways.AddNew();
			gateway1.JSG_OA_ForwarderAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			gateway1.JSG_Sequence = 1;

			var gateway2 = shipment.Gateways.AddNew();
			gateway2.JSG_OA_ForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			gateway2.JSG_Sequence = 2;
			Assert("should be false for autorating revenue of gateway shipment adapter since the first gateway is not current company org proxy", !gateway.IsGatewaySellApplicableToGatewayConsol(CostSell.Revenue));

			shipment.Gateways.Delete(gateway1);
			Assert("should be true for autorating revenue of gateway shipment adapter since the only and first gateway is the same as current company org proxy", gateway.IsGatewaySellApplicableToGatewayConsol(CostSell.Revenue));
		}

		void AssertContinueAutoRateCosting(IGateway gateway, ForwardingConsol consol, BillingType billingType)
		{
			AssertEquals
			(
				ForwardingConsolExtensions.ContinueAutorateCosting(consol, billingType),
				gateway.ContinueWithDefaultCosting(BillingType.Apportionment)
			);
		}

		#endregion

		ForwardingConsol CreateGatewayConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.Addresses.First().PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var orgAppointedAgentPorts1 = Factory.New<OrgAppointedAgentPorts>();
			orgAppointedAgentPorts1.O5_PortOrCountry = "AUBNE";
			orgAppointedAgentPorts1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPorts1.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			consol.SendingForwarder.AppointedGatewayAgentPorts.Add(orgAppointedAgentPorts1);

			AssertEquals(true, consol.IsGateway());

			return consol;
		}
	}
}
