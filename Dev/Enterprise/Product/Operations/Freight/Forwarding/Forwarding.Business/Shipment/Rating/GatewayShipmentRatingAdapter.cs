using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business
{
	public class GatewayShipmentRatingAdapter : ForwardingShipmentRatingAdapter
	{
		protected internal GatewayShipmentRatingAdapter(ForwardingConsol gatewayConsol, ForwardingShipment parent)
			: base(parent)
		{
			Argument.NotNull(gatewayConsol, "gatewayConsol");
			this.gatewayConsol = gatewayConsol;
		}

		/// <summary>
		/// Contract numbers are not used for ICT.
		/// Note: ICT entries don't have TI_ContractNumber values so ShouldAddContractNumberQueryFilter does not affect the entries querying.
		/// </summary>
		/// <returns>Default configuration</returns>
		public override IContractNumberConfiguration GetContractNumberConfiguration(CostSell costOrSell) => new DefaultContractNumberConfiguration();

		public override IEnumerable<ZString> ClientContractNumbers => Enumerable.Empty<ZString>();

		public override DataUpdateResult UpdateClientContractNumber(IEnumerable<string> newNumbers) => DataUpdateResult.NoAction;

		readonly ForwardingConsol gatewayConsol;

		public override AdapterType AdapterType => AdapterType.Shipment;

		protected override JobHeader ParentJob
		{
			get { return gatewayConsol.Job; }
		}

		public override ILocation GetVia(CostSell costOrSell) => gatewayConsol.LoadPort;

		public override DebtorOrgCollection DebtorOrgs
		{
			get
			{
				var collection = new DebtorOrgCollection();

				foreach (var org in base.DebtorOrgs.Where(d => d.RatingDebtorOrgTypes == RatingDebtorOrgTypes.CNR || d.RatingDebtorOrgTypes == RatingDebtorOrgTypes.CNE))
				{
					collection.Add(org);
				}

				var localClient = ParentJob?.LocalCharges;
				if (localClient != null)
				{
					collection[RatingDebtorOrgTypes.LC] = localClient;
				}

				var sendingAgent = gatewayConsol.SendingForwarder;
				if (sendingAgent != null)
				{
					collection[RatingDebtorOrgTypes.SAG] = sendingAgent;
				}
				else if (localClient != null)
				{
					collection[RatingDebtorOrgTypes.SAG] = localClient;
				}

				var receivingAgent = gatewayConsol.ReceivingForwarder;
				if (receivingAgent != null)
				{
					collection[RatingDebtorOrgTypes.RAG] = receivingAgent;
				}
				else if (ParentJob?.AgentCollect != null)
				{
					collection[RatingDebtorOrgTypes.RAG] = ParentJob.AgentCollect;
				}

				return collection;
			}
		}

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get
			{
				var result = new ChargeCodeGroupCollection();
				result.AddRange(Env.Registry.Rating.FreightRatedCodes);

				return result;
			}
		}

		protected override ShipmentRateLineConditionsSupporter GetConditionsSupporterCore()
		{
			return new GatewayShipmentRateLineConditionsSupporter(gatewayConsol, Parent);
		}

		protected override SpotRateInfo GetSellSpotRateInfoCore()
		{
			if (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.Value)
			{
				if (parent.JS_GatewayFreightSellRate > 0)
				{
					var money = new Money(parent.JS_GatewayFreightSellRate, parent.GatewayFreightSellRateCurrency);

					return new SpotRateInfo(
						money,
						parent.JS_FreightGatewaySellRateAutoratingMode,
						AutoratedValueType.GatewaySell)
					{ Creditor = gatewayConsol.Creditor };
				}
				return new SpotRateInfo(Money.Invalid, FreightRateAutoratingModes.Code.StandardRate, AutoratedValueType.GatewaySell);
			}

			return base.GetSellSpotRateInfoCore();
		}

		protected override SpotRateInfo GetCostSpotRateInfoCore()
		{
			if (!RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.Value)
			{
				return base.GetCostSpotRateInfoCore();
			}

			return new SpotRateInfo(Money.Invalid, FreightRateAutoratingModes.Code.StandardRate, AutoratedValueType.Cost);
		}

		public override ServiceLevelRatingInformation ServiceLevel
		{
			get
			{
				var serviceLevels = new List<ServiceLevelInfo>(base.ServiceLevel.ServiceLevelData);

				if (!gatewayConsol.JK_RS_NKGatewayServiceLevel.IsEmpty)
				{
					serviceLevels.Add(new ServiceLevelInfo(gatewayConsol.JK_RS_NKGatewayServiceLevel, ServiceLevelType.Gateway));
				}

				return new ServiceLevelRatingInformation(serviceLevels.ToArray());
			}
		}

		#region IGateway

		public override ILocation PlannedLoad(CostSell costSell) => costSell == CostSell.Revenue ? gatewayConsol.LoadPort : gatewayConsol.DischargePort;

		public override ILocation PlannedDischarge(CostSell costSell) => costSell == CostSell.Revenue ? gatewayConsol.DischargePort : Parent.DischargePort;

		IGateway GatewayConsolAsGateway => gatewayConsol;

		public override IGatewayBillingSupporter GatewayBillingSupporter => GatewayConsolAsGateway.GatewayBillingSupporter;

		public override bool IsIntercompanyTariffApplicable(BillingType billingType, CostSell costSell)
		{
			if (!RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.Value || !gatewayConsol.IsGateway())
			{
				return false;
			}

			return (costSell == CostSell.Revenue || billingType == BillingType.Invoicing);
		}

		public override ZBool ShouldRemoveNonIntercompanyTariffFRTEntries(BillingType billingType) => false;

		public override ZBool IsGatewaySellApplicableToGatewayConsol(CostSell costSell)
		{
			if (costSell == CostSell.Revenue && SortedGatewayAgentPKs.Any() && SortedGatewayAgentPKs.First() != GlbBranch.CurrentBranch.OrgProxy?.PK)
			{
				return false;
			}

			return true;
		}

		public override ZBool IsContainerNegotiatedCostApplicable(CostSell costSell) => false;

		#region SuppressResourceStringsCheckRegion 

		public override ZString GatewayAgentTypeFilteredReason(string agentType, ZGuid gatewayAgentPk, BillingType billingType, CostSell costSell)
		{
			if (costSell == CostSell.Revenue)
			{
				var gatewayAgentIndexFromCriteria = SortedGatewayAgentPKs.FindIndex(x => x == gatewayAgentPk);
				if (gatewayAgentIndexFromCriteria > -1)
				{
					var (sendingAgent, receivingAgent) = GatewayBillingSupporter.GatewayAgent();

					if (GatewayAgentType.IsSendingAgent(agentType) && (sendingAgent == null || sendingAgent.PK != gatewayAgentPk))
					{
						return FormattableString.Invariant($"Gateway Agent Type {agentType} can only be used for Consol's sending agent");
					}

					if (GatewayAgentType.IsReceivingAgent(agentType) && (receivingAgent == null || receivingAgent.PK != gatewayAgentPk))
					{
						return FormattableString.Invariant($"Gateway Agent Type {agentType} can only be used for Consol's receiving agent");
					}

					if (agentType == GatewayAgentType.Codes.SendingAgent && gatewayConsol.JK_PrepaidCollect != PaymentType.Prepaid)
					{
						return FormattableString.Invariant($"Gateway agent type {agentType} cannot be used for Gateway revenue when Consol's payment term is not prepaid");
					}

					if (agentType == GatewayAgentType.Codes.ReceivingAgent && gatewayConsol.JK_PrepaidCollect != PaymentType.Collect)
					{
						return FormattableString.Invariant($"Gateway agent type {agentType} cannot be used for Gateway revenue when Consol's payment term is not collect");
					}

					if (billingType != BillingType.Invoicing && agentType == GatewayAgentType.Codes.ReceivingAgentForImport)
					{
						return FormattableString.Invariant($"Gateway agent type {agentType} can use only for revenue under Gateway Invoicing");
					}
				}
				else
				{
					return FormattableString.Invariant($"Shipment gateways is empty or does not contain Consol's agent");
				}
			}
			else if (billingType == BillingType.Invoicing)
			{
				if (GatewayAgentType.IsReceivingAgent(agentType))
				{
					return FormattableString.Invariant($"Gateway agent type {agentType} cannot be used for gateway shipment autorating");
				}
			}

			return string.Empty;
		}

		public override ZString GatewayServiceLevelFilteredReason(ZString gatewayServiceLevel, ZGuid gatewayAgentPk)
			=> gatewayServiceLevel.IsEmpty || gatewayServiceLevel == GetStandardServiceLevel(gatewayConsol.JK_RS_NKGatewayServiceLevel)
					? ZString.Empty
					: GetGatewayServiceLevelFilteredMessage(gatewayServiceLevel);

		public override bool ContinueWithDefaultCosting(BillingType billingType) => gatewayConsol.ContinueAutorateCosting(billingType);

		public override List<ZGuid> GatewayAgentPKsForIntercompanyTariff
		{
			get
			{
				if (gatewayConsol.IsGateway())
				{
					if (ShouldGetCostsFromInterCompanyTariff())
					{
						return new List<ZGuid> { gatewayConsol.ReceivingForwarderPK };
					}
				}

				return new List<ZGuid>();
			}
		}

		bool ShouldGetCostsFromInterCompanyTariff()
		{
			return gatewayConsol.IsSendingAgentGTT()
				&& (gatewayConsol.IsReceivingAgentGTA() || gatewayConsol.IsReceivingAgentGTT())
				&& SortedGatewayAgentPKs.Contains(gatewayConsol.ReceivingForwarderPK)
				&& gatewayConsol.SendingForwarder.IsProxyOrg(GlbCompany.CurrentCompany);
		}

		public override IDictionary<ZString, IList<ZGuid>> LoginGatewayAgentRoles
		{
			get
			{
				var result = new Dictionary<ZString, IList<ZGuid>>();
				var loginCompanyOrgProxies = GlbCompany.CurrentCompany.GetAllOrgProxiesIncludingBranches();
				var sortedGatewayAgentPKs = SortedGatewayAgentPKs;
				var (sendingAgent, receivingAgent) = GatewayBillingSupporter.GatewayAgent();

				if (loginCompanyOrgProxies.Contains(sendingAgent?.PK ?? ZGuid.Empty))
				{
					result.SafeAdd(gatewayConsol.JK_SendingForwarderHandlingType, sendingAgent.PK);
				}

				if (loginCompanyOrgProxies.Contains(receivingAgent?.PK ?? ZGuid.Empty))
				{
					result.SafeAdd(gatewayConsol.JK_ReceivingForwarderHandlingType, receivingAgent.PK);
				}

				if (!loginCompanyOrgProxies.Intersect(sortedGatewayAgentPKs).Any())
				{
					var orgPKs = GetNonGatewayOrgPKs();

					if (orgPKs.Any())
					{
						result.SafeAdd(GatewayLoginAgentRole.Code.NotGateway, orgPKs);
					}
				}

				return result;
			}
		}

		#endregion

		#endregion
	}
}
