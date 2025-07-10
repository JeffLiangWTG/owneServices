using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using WiseRates.Tools;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business
{
	internal class GatewayConfiguration
	{
		public GatewayConfiguration(RatingCriteria criteria)
		{
			this.criteria = Argument.NotNull(criteria, nameof(criteria));
			SetDefaultValues();
		}

		void SetDefaultValues()
		{
			if (!IsIntercompanyTariffApplicable)
			{
				continueAutoratingCostFromIntercompanyTariff = false;
				continueAutoratingRevenueFromIntercompanyTariff = false;
				stopAutoratingCostFromCosting = false;
			}
		}

		bool UseLegacyLogicForAutoratingCostFromIntercompanyTariff => !IsShipment || IsAutoratingUnderGatewayInvoicingMenu;

		public bool ContinueAutoratingCostFromIntercompanyTariff
		{
			get
			{
				if (!continueAutoratingCostFromIntercompanyTariff.HasValue)
				{
					continueAutoratingCostFromIntercompanyTariff = criteria.IsIntercompanyTariffApplicable(_Rating.BillingType, CostSell.Cost)
						&& (UseLegacyLogicForAutoratingCostFromIntercompanyTariff
							|| RegistryConfigurationItems.Any(x =>
								x.AutoratingRule == GatewayAutoratingRule.Code.AutoratingCost &&
								x.ICTServiceProvider != GatewayICTServiceProvider.Code.NotAutorate) &&
								RegistryConfigurationItems.All(x => x.AutoratingRule != GatewayAutoratingRule.Code.StopAutoratingCostFromICT));
				}

				return continueAutoratingCostFromIntercompanyTariff.Value;
			}
		}
		bool? continueAutoratingCostFromIntercompanyTariff;

		public IEnumerable<ZGuid> OrganizationsForAutoratingCostFromIntercompanyTariff
		{
			get
			{
				if (UseLegacyLogicForAutoratingCostFromIntercompanyTariff)
				{
					return criteria.GatewayAgentPKsForIntercompanyTariff;
				}

				var applicableConfigs = RegistryConfigurationItems
					.Where(x => x.AutoratingRule == GatewayAutoratingRule.Code.AutoratingCost)
					.ToList();

				return GetApplicapleServiceProvidersForIntercompanyTariff(applicableConfigs);
			}
		}

		bool UseLegacyLogicForAutoratingRevenueFromIntercompanyTariff => !IsShipment;

		public bool ContinueAutoratingRevenueFromIntercompanyTariff
		{
			get
			{
				if (!continueAutoratingRevenueFromIntercompanyTariff.HasValue)
				{
					continueAutoratingRevenueFromIntercompanyTariff = criteria.IsIntercompanyTariffApplicable(_Rating.BillingType, CostSell.Revenue)
						&& (UseLegacyLogicForAutoratingRevenueFromIntercompanyTariff
							|| RegistryConfigurationItems.Any(x =>
								x.AutoratingRule == GatewayAutoratingRule.Code.AutoratingRevenue &&
								x.ICTServiceProvider != GatewayICTServiceProvider.Code.NotAutorate));
				}

				return continueAutoratingRevenueFromIntercompanyTariff.Value;
			}
		}
		bool? continueAutoratingRevenueFromIntercompanyTariff;

		public IEnumerable<ZGuid> OrganizationsForAutoratingRevenueFromIntercompanyTariff
		{
			get
			{
				if (UseLegacyLogicForAutoratingRevenueFromIntercompanyTariff)
				{
					var (sendingAgent, receivingAgent) = criteria.GatewayBillingSupporter.GatewayAgent();
					var agentPKs = new HashSet<ZGuid>
					{
						sendingAgent?.PK ?? ZGuid.Empty,
						receivingAgent?.PK ?? ZGuid.Empty
					};

					return agentPKs.Where(x => !x.IsEmpty).ToList();
				}

				var applicableConfigs = RegistryConfigurationItems
					.Where(x => x.AutoratingRule == GatewayAutoratingRule.Code.AutoratingRevenue)
					.ToList();

				return GetApplicapleServiceProvidersForIntercompanyTariff(applicableConfigs);
			}
		}

		bool UseLegacyLogicForAutoratingCostFromCosting => !IsShipment;

		public bool StopAutoratingCostFromCosting
		{
			get
			{
				if (!stopAutoratingCostFromCosting.HasValue)
				{
					var continueWithDefaultCosting = criteria.ContinueWithDefaultCosting(_Rating.BillingType); // Legacy Logic

					stopAutoratingCostFromCosting = UseLegacyLogicForAutoratingCostFromCosting
						? !continueWithDefaultCosting
						: !continueWithDefaultCosting ||
							RegistryConfigurationItems.Any(x => x.AutoratingRule == GatewayAutoratingRule.Code.StopAutoratingCost);
				}
				return stopAutoratingCostFromCosting.Value;
			}
		}
		bool? stopAutoratingCostFromCosting;

		IEnumerable<AutoratingIntercompanyTariffsForGatewayJobConfiguration> RegistryConfigurationItems
		{
			get
			{
				if (registryConfigurationItems == null)
				{
					registryConfigurationItems = DataRegistryRating.Instance
						.AutoratingIntercompanyTariffsForGatewayJobConfiguration.Value
						.Cast<AutoratingIntercompanyTariffsForGatewayJobConfiguration>()
						.Where(x =>
							(x.ShipmentDirection == FreightShipmentDirection.Code.All || x.ShipmentDirection == ShipmentDirection) &&
							(x.LoginRole.IsEmpty || x.LoginRole == criteria.GatewayLoginRole) &&
							(criteria.LoginGatewayAgentRoles.ContainsKey(x.LoginAgentRole)) &&
							(x.AutoratingJob == AutoratingJob))
						.OrderByDescending(x => x.ShipmentDirection) //TODO: Pedram, why we need to order the list? 
						.ToList();

					if (registryConfigurationItems.Any(x => !x.LoginRole.IsEmpty))
					{
						registryConfigurationItems = registryConfigurationItems.Where(x => !x.LoginRole.IsEmpty).ToList();
					}
				}

				return registryConfigurationItems;
			}
		}
		IEnumerable<AutoratingIntercompanyTariffsForGatewayJobConfiguration> registryConfigurationItems;

		IEnumerable<ZGuid> GetApplicapleServiceProvidersForIntercompanyTariff(
			IEnumerable<AutoratingIntercompanyTariffsForGatewayJobConfiguration> applicableConfigurations)
		{
			var organizationPKs = new List<ZGuid>();

			foreach (var config in applicableConfigurations)
			{
				if (config.ICTServiceProvider == GatewayICTServiceProvider.Code.First)
				{
					var orgPK = criteria.SortedGatewayAgentPKs.FirstOrDefault();
					if (!orgPK.IsEmpty)
					{
						organizationPKs.UniqueAdd(orgPK);
					}

					continue;
				}

				if (config.ICTServiceProvider != GatewayICTServiceProvider.Code.NotAutorate &&
					criteria.LoginGatewayAgentRoles.TryGetValue(config.LoginAgentRole, out var orgPkList))
				{
					var isGatewayAgent = config.LoginAgentRole == AgentStatusList.Codes.GatewayAgentWithTariff || config.LoginAgentRole == AgentStatusList.Codes.GatewayAgent;
					var indexAddition = Convert.ToInt32(isGatewayAgent && config.ICTServiceProvider == GatewayICTServiceProvider.Code.NextGatewayOrganization);
					var index = isGatewayAgent ? criteria.SortedGatewayAgentPKs.FindIndex(x => orgPkList.Contains(x)) : 0;

					if (index >= 0)
					{
						var orgPK = criteria.SortedGatewayAgentPKs.Skip(index + indexAddition).FirstOrDefault();
						if (!orgPK.IsEmpty)
						{
							organizationPKs.UniqueAdd(orgPK);
						}
					}
				}
			}

			return organizationPKs;
		}

		string ShipmentDirection
		{
			get
			{
				switch (criteria.JobDirection)
				{
					case Directions.Export:
						return FreightShipmentDirection.Code.Export;
					case Directions.Import:
						return FreightShipmentDirection.Code.Import;
					case Directions.Domestic:
						return FreightShipmentDirection.Code.Domestic;
					default:
						return FreightShipmentDirection.Code.Other; // including CrossTrade
				}
			}
		}

		bool IsAutoratingUnderGatewayInvoicingMenu =>
			IsGateway && _Rating.BillingType == BillingType.Invoicing;

		string AutoratingJob => IsGateway
			? JobInvoicingConsumerTypes.ForwardingConsolCode
			: JobInvoicingConsumerTypes.ShipmentCode;

		bool IsIntercompanyTariffApplicable =>
			RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.Value &&
			(IsShipment || IsGateway);

		public bool IsGatewayShipment => IsShipment && IsGateway;
		public bool IsForwardingShipment => IsShipment && criteria.GatewayBillingSupporter == null;

		bool IsShipment => criteria.AdapterType == AdapterType.Shipment;

		public bool IsGateway => isGateway ?? (isGateway = criteria.GatewayBillingSupporter?.IsGatewayBillingEnabled() ?? false).Value;
		bool? isGateway;

		readonly RatingCriteria criteria;
	}
}
