using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentRatingAdapter : ShipmentRatingAdapter<ForwardingShipment>,
		IAutoRatingCustomsInfo,
		IAutoRatingCompanyTariffLevelProvider,
		IGateway
	{
		protected internal ForwardingShipmentRatingAdapter(ForwardingShipment parent) : base(parent) { }

		public override ZString PaymentTermOverride => Parent.JS_PaymentTermAutoratingOverride;

		public override ZString AircraftType
		{
			get
			{
				var airRoutingLegs = Parent.TransportsIncludingRelated.Cast<Transport>().Where(t => t.TransportMode == TransportModes.Air).ToList();

				if (!airRoutingLegs.Any())
				{
					return ZString.Empty;
				}

				return airRoutingLegs.All(t => t.JW_IsCargoOnly)
					? Constants.AircraftType.CAO
					: Constants.AircraftType.PAX;
			}
		}

		protected IAutoRatingCustomsInfo Declaration
		{
			get
			{
				if (declaration == null)
				{
					var supporter = Parent.GetDeclaration() as IRatingSupporterWithAdapter;
					declaration = supporter?.RatingAdapter as IAutoRatingCustomsInfo;
				}

				return declaration;
			}
		}

		IAutoRatingCustomsInfo declaration;

		#region Contract Numbers

		public override IContractNumberConfiguration GetContractNumberConfiguration(CostSell costOrSell)
		{
			return new ContractNumberConfiguration(costOrSell, parent);
		}

		class ContractNumberConfiguration : IContractNumberConfiguration
		{
			public ContractNumberConfiguration(CostSell costOrSell, ForwardingShipment parent)
			{
				this.costOrSell = costOrSell;
				this.parent = parent;
			}
			readonly CostSell costOrSell;
			readonly ForwardingShipment parent;

			public bool ShouldAddContractNumberQueryFilter => true;

			public bool ShouldApplySpecificAdapterContractNumberFilter => costOrSell == CostSell.Revenue || (parent.Consols?.Any() ?? false);

			public bool ShouldIgnoreJobClientContractNumbers => false;

			// This is currently only called for costing so no need to check costOrSell. The behaviour is undefined for revenue.
			public bool ShouldIgnoreJobCarrierContractNumbers => false;

			public bool ShouldMatchJobBlankContractNumber => false;

			public bool ShouldUseCarrierContractDateFilter => false;
		}

		/// <summary>
		/// Returns the client contract number from the job header if it is not blank, or an empty list if blank or no job header.
		/// </summary>
		public override IEnumerable<ZString> ClientContractNumbers
		{
			get
			{
				var number = Parent.ShipmentJobHeader?.JH_ClientContractNumber ?? ZString.Empty;
				return !number.IsEmpty ? new[] { number } : Enumerable.Empty<ZString>();
			}
		}

		#endregion

		public override ILocation PlannedLoad(CostSell costSell) => parent.LoadPort;

		public override ILocation PlannedDischarge(CostSell costSell) => parent.DischargePort;

		#region IAutoRatingCustomsInfo

		EntryInfoCollection IAutoRatingCustomsInfo.Entries => Declaration?.Entries;

		ZString IAutoRatingCustomsInfo.MessageSubType => Declaration?.MessageSubType ?? ZString.Empty;

		InvoiceInfoCollection IAutoRatingCustomsInfo.Invoices => Declaration?.Invoices;

		ZInt IAutoRatingCustomsInfo.SubHeaderCount => Declaration?.SubHeaderCount ?? 0;

		public ZString MessageType => Declaration?.MessageType ?? ZString.Empty;

		public InvoiceInfoCollection TariffsPerInvoice => Declaration?.TariffsPerInvoice;

		public InvoiceInfoCollection TariffsPerShipment => Declaration?.TariffsPerShipment;

		#endregion

		public override ILocation RateOrigin
		{
			get { return parent.FreightRateOrigin; }
		}

		public override ILocation RateDestination
		{
			get { return parent.FreightRateDestination; }
		}

		protected override SpotRateInfo GetCostSpotRateInfoCore()
		{
			if (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.Value
				&& parent.JS_GatewayFreightSellRate > 0 && parent.Gateways.Any() &&
				(parent.JS_FreightGatewaySellRateAutoratingMode == FreightRateAutoratingModes.Code.FreightPlusRate
				|| parent.JS_FreightGatewaySellRateAutoratingMode == FreightRateAutoratingModes.Code.AllInRate))
			{
				OrgHeader creditor = null;
				var money = new Money(parent.JS_GatewayFreightSellRate, parent.GatewayFreightSellRateCurrency);
				var consol = parent.GetFirstOrCorrectConsol();

				if (consol != null)
				{
					creditor = consol.Creditor;
				}

				return new SpotRateInfo(
						money,
						parent.JS_FreightGatewaySellRateAutoratingMode,
						AutoratedValueType.GatewaySell)
				{ Creditor = creditor };
			}

			return base.GetCostSpotRateInfoCore();
		}

		protected override SpotRateInfo GetSellSpotRateInfoCore()
		{
			if (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.Value && parent.JS_UnitFreightRate > 0)
			{
				var money = new Money(parent.JS_UnitFreightRate, parent.FrtRateCurrency);
				return new SpotRateInfo(money, parent.JS_FreightSpotRateAutoratingMode, AutoratedValueType.SpotRate);
			}

			return base.GetSellSpotRateInfoCore();
		}

		public override ChargeCodeGroupCollection ChargeCodeGroups
		{
			get
			{
				var collection = base.ChargeCodeGroups;

				if (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.Value && parent.Gateways.Any())
				{
					var firstGatewayAgentHandlingType = GetGatewayAgentHandlingType(parent.Gateways.First().ForwarderPK);

					if (!parent.Consols.Any() || firstGatewayAgentHandlingType == AgentStatusList.Codes.GatewayAgentWithTariff)
					{
						collection.CostChargesFilter = ChargeCodeFilter.AutorateNonConsolLevelOnly;
					}
					else if (firstGatewayAgentHandlingType == AgentStatusList.Codes.GatewayAgent &&
						IsSingleShipmentAttachedToSingleConsol &&
						!parent.Consols[0].HasConsolCosts(GlbCompany.CurrentCompany))
					{
						collection.CostChargesFilter = ChargeCodeFilter.AutorateAll;
					}
				}

				return collection;
			}
		}

		string GetGatewayAgentHandlingType(ZGuid gatewayAgentPK)
		{
			var sortedConsols = Parent.Consols.Cast<ForwardingConsol>().ToArray();
			MovementLegComparer.SortMovementLegsByPorts(sortedConsols);

			foreach (var consol in sortedConsols)
			{
				var (sendingAgent, receivingAgent) = ((IGateway)consol).GatewayBillingSupporter.GatewayAgent();

				if (sendingAgent?.PK == gatewayAgentPK)
				{
					return consol.JK_SendingForwarderHandlingType;
				}

				if (receivingAgent?.PK == gatewayAgentPK)
				{
					return consol.JK_ReceivingForwarderHandlingType;
				}
			}

			return string.Empty;
		}

		bool IsSingleShipmentAttachedToSingleConsol =>
			parent.Consols.Count == 1 &&
			parent.Consols[0].Shipments.Count == 1;

		int IAutoRatingCompanyTariffLevelProvider.TariffLevel
		{
			get
			{
				if (Parent.JS_CompanyTariffLevelOverrideInfo.ReadOnly)
				{
					var doNotOverrideCompanyTariffLevel = 0;
					return doNotOverrideCompanyTariffLevel;
				}

				return Convert.ToInt32(Parent.JS_CompanyTariffLevelOverride);
			}
		}

		#region IGateway

		public virtual ZString GatewayAgentTypeFilteredReason(string agentType, ZGuid gatewayAgentPk, BillingType billingType, CostSell costSell)
		{
			if (costSell == CostSell.Cost)
			{
				var applicableAgentTypes = new List<string> { GatewayAgentType.Codes.SendingAgent, GatewayAgentType.Codes.ReceivingAgent, "" };
				if (!applicableAgentTypes.Contains(agentType))
				{
					return FormattableString.Invariant($"Gateway agent type {agentType} cannot be used for shipment autorating"); // Filter log message
				}

				var sortedConsols = Parent.Consols.Cast<ForwardingConsol>().ToArray();
				MovementLegComparer.SortMovementLegsByPorts(sortedConsols);
				var rateValidation = string.Empty;

				var lastAgent = SortedGatewayAgentPKs.LastOrDefault();
				var isAgentTheLastReceivingAgent = !lastAgent.IsEmpty
													&& gatewayAgentPk == lastAgent
													&& sortedConsols.LastOrDefault()?.ReceivingForwarderPK == lastAgent;

				foreach (var consol in sortedConsols)
				{
					var isSendingForwarder = consol.SendingForwarderPK == gatewayAgentPk;
					var isReceivingForwarder = consol.ReceivingForwarderPK == gatewayAgentPk;

					if (agentType.IsNullOrEmpty()
						|| (isSendingForwarder && agentType == GatewayAgentType.Codes.SendingAgent && (consol.JK_PrepaidCollect == PaymentType.Prepaid || string.IsNullOrEmpty(consol.JK_PrepaidCollect)))
						|| (isReceivingForwarder && agentType == GatewayAgentType.Codes.ReceivingAgent && consol.JK_PrepaidCollect == PaymentType.Collect))
					{
						return string.Empty;
					}

					if (isAgentTheLastReceivingAgent && agentType == GatewayAgentType.Codes.SendingAgent)
					{
						return string.Empty;
					}

					if (isSendingForwarder)
					{
						rateValidation = FormattableString.Invariant($"Gateway agent type {agentType} cannot be used for shipment autorating because of sending agent of attached Consol {consol.JK_UniqueConsignRef} with following payment {consol.JK_PrepaidCollect}"); // Filter log message
					}
					else if (isReceivingForwarder)
					{
						rateValidation = FormattableString.Invariant($"Gateway agent type {agentType} cannot be used for shipment autorating because of receiving agent of attached Consol {consol.JK_UniqueConsignRef} with following payment {consol.JK_PrepaidCollect}"); // Filter log message
					}
				}

				return rateValidation;
			}

			return string.Empty;
		}

		public virtual ZString GatewayServiceLevelFilteredReason(ZString gatewayServiceLevel, ZGuid gatewayAgentPk)
		{
			if (gatewayServiceLevel.IsEmpty || gatewayAgentPk.IsEmpty)
			{
				return ZString.Empty;
			}

			OrgHeader referenceOrg;
			var rateGatewayServiceLevel = GetStandardServiceLevel(gatewayServiceLevel);
			var handlingTypes = new List<ZString>()
				{
					AgentStatusList.Codes.GatewayAgent,
					AgentStatusList.Codes.GatewayAgentWithTariff,
				};

			var sortedConsols = Parent.Consols.Cast<ForwardingConsol>().ToArray();
			MovementLegComparer.SortMovementLegsByPorts(sortedConsols);

			foreach (var consol in sortedConsols)
			{
				referenceOrg = null;
				if (consol.JK_PrepaidCollect == PaymentType.Prepaid && handlingTypes.Contains(consol.JK_SendingForwarderHandlingType))
				{
					referenceOrg = consol.SendingForwarder;
				}
				else if (consol.JK_PrepaidCollect == PaymentType.Collect && handlingTypes.Contains(consol.JK_ReceivingForwarderHandlingType))
				{
					referenceOrg = consol.ReceivingForwarder;
				}

				if
				(
					referenceOrg != null && referenceOrg.PK == gatewayAgentPk &&
					GetStandardServiceLevel(consol.JK_RS_NKGatewayServiceLevel) == rateGatewayServiceLevel
				)
				{
					return ZString.Empty;
				}
			}

			return GetGatewayServiceLevelFilteredMessage(gatewayServiceLevel);
		}

		public virtual ZString ShipmentGatewayServiceLevel => Parent.JS_RS_NKGatewayServiceLevel;

		protected ZString GetStandardServiceLevel(ZString serviceLevel)
			=> serviceLevel.IsEmpty ? new ZString("STD") : serviceLevel;

		protected ZString GetGatewayServiceLevelFilteredMessage(ZString gatewayServiceLevel) =>
			Res.GetString("68AC6766-AEBD-46C2-8101-DD0609C6C8B4", "Gateway Service Level '{0}' cannot be used for shipment autorating", gatewayServiceLevel);

		public virtual bool ContinueWithDefaultCosting(BillingType billingType)
		{
			if (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.Value
				&& parent.Gateways.Count > 0
				&& billingType == BillingType.Invoicing
				&& DiscardGettingCost())
			{
				return false;
			}

			return true;
		}

		bool DiscardGettingCost()
		{
			var currentCompanyCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			if (parent.JobDirection == Directions.Import)
			{
				foreach (var consol in parent.Consols.Cast<ForwardingConsol>().ToArray())
				{
					var sendingAgent = consol.SendingForwarder;
					var receivingAgent = consol.ReceivingForwarder;
					var (sendingGatewayAgent, receivingGatewayAgent) = consol.GatewayAgent();

					if (sendingGatewayAgent != null && // When Sending Agent is a Gateway Agent, means it's already checked for being a proxy Organization of current login Company
						receivingGatewayAgent == null &&
						receivingAgent != null &&
						receivingAgent.IsProxyOrgOfAnyCompany() &&
						receivingAgent.CountryCode == currentCompanyCountryCode &&
						sendingAgent.CountryCode == currentCompanyCountryCode &&
						!sendingAgent.CompanyProxies(false).Intersect(receivingAgent.CompanyProxies(false)).Any())
					{
						return true;
					}
				}
			}

			if (parent.JobDirection == Directions.Export)
			{
				foreach (var consol in parent.Consols.Cast<ForwardingConsol>().ToArray())
				{
					var sendingAgent = consol.SendingForwarder;
					var receivingAgent = consol.ReceivingForwarder;
					var (sendingGatewayAgent, receivingGatewayAgent) = consol.GatewayAgent();

					if (receivingGatewayAgent != null && // When Receiving Agent is a Gateway Agent, means it's already checked for being a proxy Organization of current login Company
						sendingGatewayAgent == null &&
						sendingAgent != null &&
						sendingAgent.IsProxyOrgOfAnyCompany() &&
						sendingAgent.CountryCode == currentCompanyCountryCode &&
						receivingAgent.CountryCode == currentCompanyCountryCode &&
						!receivingAgent.CompanyProxies(false).Intersect(sendingAgent.CompanyProxies(false)).Any())
					{
						return true;
					}
				}
			}

			return false;
		}

		bool IsForwardingShipment(BillingType billingType) =>
			GatewayBillingSupporter == null && billingType == BillingType.Invoicing;

		public virtual ZBool ShouldRemoveNonIntercompanyTariffFRTEntries(BillingType billingType)
		{
			if (!RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.Value || !IsForwardingShipment(billingType))
			{
				return false;
			}

			var isEngagedGateway = parent.Gateways.Any();

			foreach (var consol in Parent.Consols.Cast<ForwardingConsol>())
			{
				var (sendingAgent, receivingAgent) = consol.GatewayAgent();

				if ((sendingAgent != null && consol.JK_SendingForwarderHandlingType == AgentStatusList.Codes.GatewayAgent) ||
					(receivingAgent != null && consol.JK_ReceivingForwarderHandlingType == AgentStatusList.Codes.GatewayAgent))
				{
					return false;
				}

				isEngagedGateway |=
					(sendingAgent != null && consol.JK_SendingForwarderHandlingType == AgentStatusList.Codes.GatewayAgentWithTariff) ||
					(receivingAgent != null && consol.JK_ReceivingForwarderHandlingType == AgentStatusList.Codes.GatewayAgentWithTariff);
			}

			return isEngagedGateway;
		}

		public virtual List<ZGuid> SortedGatewayAgentPKs => SortedGatewayAgents.Select(x => x.PK).ToList();

		protected IEnumerable<OrgHeader> SortedGatewayAgents => parent.Gateways
			.OrderBy(x => x.JSG_Sequence)
			.Select(x => x.Forwarder).Where(x => x != null);

		public virtual List<ZGuid> GatewayAgentPKsForIntercompanyTariff => SortedGatewayAgentPKs;

		public virtual IDictionary<ZString, IList<ZGuid>> LoginGatewayAgentRoles
		{
			get
			{
				var result = new Dictionary<ZString, IList<ZGuid>>();
				var loginCompanyOrgProxies = GlbCompany.CurrentCompany.GetAllOrgProxiesIncludingBranches();
				var sortedGatewayAgentPKs = SortedGatewayAgentPKs;
				var sortedConsols = Parent.Consols.Cast<ForwardingConsol>().ToArray();
				MovementLegComparer.SortMovementLegsByPorts(sortedConsols);

				foreach (var consol in sortedConsols)
				{
					var (sendingAgent, receivingAgent) = ((IGateway)consol).GatewayBillingSupporter.GatewayAgent();

					if (loginCompanyOrgProxies.Contains(sendingAgent?.PK ?? ZGuid.Empty))
					{
						result.SafeAdd(consol.JK_SendingForwarderHandlingType, sendingAgent.PK);
					}

					if (loginCompanyOrgProxies.Contains(receivingAgent?.PK ?? ZGuid.Empty))
					{
						result.SafeAdd(consol.JK_ReceivingForwarderHandlingType, receivingAgent.PK);
					}
				}

				if (!loginCompanyOrgProxies.Intersect(sortedGatewayAgentPKs).Any())
				{
					result.SafeAdd(GatewayLoginAgentRole.Code.NotGateway, GetNonGatewayOrgPKs());
				}

				if (Parent.Consols.Count == 0 && sortedGatewayAgentPKs.Count > 0)
				{
					result.SafeAdd(GatewayLoginAgentRole.Code.Gateway, sortedGatewayAgentPKs);
				}

				return result;
			}
		}

		public virtual ZString GatewayLoginRole
		{
			get
			{
				var pickupAgent = Parent.PickupAgent;
				if (pickupAgent != null)
				{
					if (pickupAgent.IsProxyOrg(GlbCompany.CurrentCompany))
					{
						return GatewayPickupAgentStatus.Code.PickupAgent;
					}

					if (!pickupAgent.IsProxyOrg(GlbCompany.CurrentCompany) && pickupAgent.IsProxyOrgOfAnyCompany(excludeCurrentLoginCompany: true))
					{
						return GatewayPickupAgentStatus.Code.NotPickupAgent;
					}
				}

				return ZString.Empty;
			}
		}

		protected IEnumerable<ZGuid> GetNonGatewayOrgPKs()
		{
			IEnumerable<OrgHeader> orgs;
			var agents = SortedGatewayAgents.ToList();

			var shipmentOrigin = parent.Origin?.Country?.Code ?? null;
			var shipmentDestination = parent.Destination?.Country?.Code ?? null;

			switch (JobDirection)
			{
				case Directions.Export:
					orgs = agents.Where(x => x.CountryCode == shipmentOrigin);
					break;
				case Directions.Import:
					orgs = agents.Where(x => x.CountryCode == shipmentDestination);
					break;
				case Directions.Domestic:
					orgs = agents.Where(x =>
						x.CountryCode == shipmentOrigin &&
						x.CountryCode == shipmentDestination);
					break;
				case Directions.CrossTrade:
					orgs = agents.Where(x =>
						x.CountryCode != shipmentOrigin &&
						x.CountryCode != shipmentDestination);
					break;
				default:
					orgs = Enumerable.Empty<OrgHeader>();
					break;
			}

			return orgs.Select(x => x.PK).ToList();
		}

		public virtual List<ILocation> SortedOverridenPlannedLoad
		{
			get
			{
				var locations = new List<ILocation>();
				var applicableAgentType = new List<string> { AgentStatusList.Codes.GatewayAgent, AgentStatusList.Codes.GatewayAgentWithTariff };
				var sortedConsols = Parent.Consols.Cast<ForwardingConsol>().ToArray();
				MovementLegComparer.SortMovementLegsByPorts(sortedConsols);

				if (sortedConsols.Any())
				{
					locations.AddRange(sortedConsols.Select(x => x.LoadPort).Where(x => x != null));
					locations.AddRange(sortedConsols.Select(x => x.DischargePort).Where(x => x != null));

					locations.AddRange(sortedConsols.Where(x =>
							applicableAgentType.Contains(x.JK_SendingForwarderHandlingType) &&
							SortedGatewayAgentPKs.Contains(x.SendingForwarderPK))
						.Select(x => x.SendingForwarderAddress.RelatedPortCode));
				}

				return locations.Where(x => x != null).Distinct().ToList();
			}
		}

		public virtual List<ILocation> SortedOverridenPlannedDischarge
		{
			get
			{
				var locations = new List<ILocation>();
				var applicableAgentType = new List<string> { AgentStatusList.Codes.GatewayAgent, AgentStatusList.Codes.GatewayAgentWithTariff };
				var sortedConsols = Parent.Consols.Cast<ForwardingConsol>().ToArray();
				MovementLegComparer.SortMovementLegsByPorts(sortedConsols);
				var reversedSortedConsols = sortedConsols.Reverse();

				if (reversedSortedConsols.Any())
				{
					locations.AddRange(reversedSortedConsols.Select(x => x.DischargePort).Where(x => x != null));

					locations.AddRange(reversedSortedConsols.Where(x =>
							applicableAgentType.Contains(x.JK_ReceivingForwarderHandlingType) &&
							SortedGatewayAgentPKs.Contains(x.ReceivingForwarderPK))
							.Select(x => x.ReceivingForwarderAddress.RelatedPortCode));
				}

				return locations.Where(x => x != null).Distinct().ToList();
			}
		}

		public virtual IGatewayBillingSupporter GatewayBillingSupporter => null;

		public virtual ZBool IsGatewaySellApplicableToGatewayConsol(CostSell costSell) => true;

		public virtual ZBool IsContainerNegotiatedCostApplicable(CostSell costSell) => true;

		public virtual List<ZGuid> SortedControllingCustomerPKs
		{
			get
			{
				var sortedControllingCustomerPKs = new HashSet<ZGuid>();

				if (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.Value)
				{
					var controllingCustomer = parent.ControllingCustomer;
					if (controllingCustomer != null)
					{
						sortedControllingCustomerPKs.Add(controllingCustomer.PK);
					}

					if (parent.IsCrossTrade())
					{
						AddSortedControllingCustomersForCrossTrade(sortedControllingCustomerPKs);
					}
					else
					{
						var localClientPk = parent.JobHeader?.LocalChargesAddr?.OA_OH ?? ZGuid.Empty;
						if (!localClientPk.IsEmpty)
						{
							sortedControllingCustomerPKs.Add(localClientPk);
						}
					}
				}

				return sortedControllingCustomerPKs.ToList();
			}
		}

		void AddSortedControllingCustomersForCrossTrade(ISet<ZGuid> sortedControllingCustomerPKs)
		{
			if (parent.IsPrepaid)
			{
				var savedLocalClientsForOriginCountry = GetAllSavedLocalClientForCountry(Origin?.Country);

				if (savedLocalClientsForOriginCountry.Count > 0)
				{
					sortedControllingCustomerPKs.UnionWith(savedLocalClientsForOriginCountry);
				}
				else
				{
					var localClientPK = GetLocalClientForExport(JobInvoicingConsumerTypes.Shipment);

					if (!localClientPK.IsEmpty)
					{
						sortedControllingCustomerPKs.Add(localClientPK);
					}
				}

				var consignor = parent.Consignor;
				if (consignor != null)
				{
					var iftRelatedParty = consignor.ConsignorRelatedParties?.FirstOrDefault(x => x is OrgRelatedParty t &&
										t.PR_PartyType == RelatedPartyTypeList.Codes.InvoiceFreightJobsTo) as OrgRelatedParty;

					if (iftRelatedParty != null)
					{
						sortedControllingCustomerPKs.Add(iftRelatedParty.PR_OH_RelatedParty);
					}

					sortedControllingCustomerPKs.Add(consignor.PK);
				}
			}
			else if (parent.IsCollect)
			{
				var savedLocalClientsForDestinationCountry = GetAllSavedLocalClientForCountry(Destination?.Country);

				if (savedLocalClientsForDestinationCountry.Count > 0)
				{
					sortedControllingCustomerPKs.UnionWith(savedLocalClientsForDestinationCountry);
				}
				else
				{
					var localClientPK = GetLocalClientForImport(JobInvoicingConsumerTypes.Shipment);

					if (!localClientPK.IsEmpty)
					{
						sortedControllingCustomerPKs.Add(localClientPK);
					}
				}

				var consignee = parent.Consignee;
				if (consignee != null)
				{
					var iftRelatedParty = consignee.ConsigneeRelatedParties?.FirstOrDefault(x => x is OrgRelatedParty t &&
										t.PR_PartyType == RelatedPartyTypeList.Codes.InvoiceFreightJobsTo) as OrgRelatedParty;

					if (iftRelatedParty != null)
					{
						sortedControllingCustomerPKs.Add(iftRelatedParty.PR_OH_RelatedParty);
					}

					sortedControllingCustomerPKs.Add(parent.Consignee.PK);
				}
			}
		}

		List<ZGuid> GetAllSavedLocalClientForCountry(RefCountry country)
		{
			var allSavedLocalClients = new List<ZGuid>();

			if (country == null)
			{
				return allSavedLocalClients;
			}

			var companiesInSameCountry = GlbCompany.GetActiveCompanies(country.Code).Select(x => x.PK).ToArray();

			if (companiesInSameCountry.Length > 0)
			{
				allSavedLocalClients = Parent.Factory.Load<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, Parent.PK).AddToFilter(JobHeaderSchema.JH_GC, companiesInSameCountry)).Select(x => x.LocalChargesPK).ToList();
			}

			return allSavedLocalClients;
		}

		ZGuid GetLocalClientForExport(JobInvoicingConsumerType jobType)
		{
			var result = ZGuid.Empty;

			if (jobType == JobInvoicingConsumerTypes.CFSShipment)
			{
				result = parent.InvoicingSupporter.SendingAgent?.PK ?? ZGuid.Empty;
			}

			if (result.IsEmpty)
			{
				result = Consignor?.GetRelatedBillToParty(jobType, false).PK ?? ZGuid.Empty;
			}

			return result;
		}

		ZGuid GetLocalClientForImport(JobInvoicingConsumerType jobType) => Consignee?.GetRelatedBillToParty(jobType, true).PK ?? ZGuid.Empty;

		public virtual bool IsIntercompanyTariffApplicable(BillingType billingType, CostSell costSell)
		{
			return costSell == CostSell.Cost;
		}

		#endregion

		#region IJobDataUpdater

		public override CanUpdateCarrierContractNumberResult CanUpdateCarrierContractNumber(IEnumerable<string> contractNumbers, IDialogService dialogService = null, bool isManualCostSelected = false)
		{
			var consol = Parent.GetFirstOrCorrectConsol();
			return consol != null
				? ((IJobDataUpdater)consol.RatingAdapter).CanUpdateCarrierContractNumber(contractNumbers, dialogService, isManualCostSelected)
				: base.CanUpdateCarrierContractNumber(contractNumbers, dialogService);
		}

		public override DataUpdateResult UpdateCarrierContractNumber(UpdateCarrierContractNumberToken token)
		{
			var consol = Parent.GetFirstOrCorrectConsol();
			return consol != null
				? ((IJobDataUpdater)consol.RatingAdapter).UpdateCarrierContractNumber(token)
				: DataUpdateResult.NoAction;
		}

		public override DataUpdateResult UpdateClientContractNumber(IEnumerable<string> newNumbers)
		{
			var jobHeader = Parent.ShipmentJobHeader;
			if (jobHeader != null)
			{
				var nonBlankContractNumbers = newNumbers.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToArray();
				if (nonBlankContractNumbers.Length == 1)
				{
					var newContractNumber = nonBlankContractNumbers.First();
					if (!string.Equals(jobHeader.JH_ClientContractNumber, newContractNumber, StringComparison.OrdinalIgnoreCase))
					{
						jobHeader.JH_ClientContractNumber = newContractNumber;
						return DataUpdateResult.Updated;
					}
				}
			}

			return DataUpdateResult.NoAction;
		}

		#endregion
	}
}
