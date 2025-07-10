using System.Linq;
using CargoWise.Common;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class ForwardingConsolExtensions
	{
		public static bool IsGateway(this ForwardingConsol consol)
		{
			return ((IGateway)consol)?.GatewayBillingSupporter.IsGatewayBillingEnabled() ?? false;
		}

		public static bool IsSendingAgentGTT(this ForwardingConsol consol)
		{
			return (consol?.JK_SendingForwarderHandlingType ?? string.Empty) == AgentStatusList.Codes.GatewayAgentWithTariff;
		}

		public static bool IsSendingAgentGTA(this ForwardingConsol consol)
		{
			return (consol?.JK_SendingForwarderHandlingType ?? string.Empty) == AgentStatusList.Codes.GatewayAgent;
		}

		public static bool IsReceivingAgentGTT(this ForwardingConsol consol)
		{
			return (consol?.JK_ReceivingForwarderHandlingType ?? string.Empty) == AgentStatusList.Codes.GatewayAgentWithTariff;
		}

		public static bool IsReceivingAgentGTA(this ForwardingConsol consol)
		{
			return (consol?.JK_ReceivingForwarderHandlingType ?? string.Empty) == AgentStatusList.Codes.GatewayAgent;
		}

		public static bool ContinueAutorateCosting(this ForwardingConsol consol, BillingType billingType)
		{
			Argument.NotNull(consol, nameof(consol));

			if (!RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.Value)
			{
				return true;
			}

			var companyOrgProxiesGuids = GlbCompany.CurrentCompany.GetAllOrgProxiesIncludingBranches().ToList();

			if (consol.IsGateway())
			{
				if (billingType == BillingType.Invoicing) // Autorate Costs under Gateway Invoicing
				{
					if (consol.IsSendingAgentGTT() && companyOrgProxiesGuids.Contains(consol.SendingForwarderPK))
					{
						return true;
					}

					if (consol.IsReceivingAgentGTA() && companyOrgProxiesGuids.Contains(consol.ReceivingForwarderPK))
					{
						return false;
					}

					if (consol.IsSendingAgentGTA() && companyOrgProxiesGuids.Contains(consol.SendingForwarderPK))
					{
						return false;
					}
				}
				else if (billingType == BillingType.Apportionment) // Autorate Costs under Job Invoicing 
				{
					if (consol.IsReceivingAgentGTT() && companyOrgProxiesGuids.Contains(consol.ReceivingForwarderPK))
					{
						return false;
					}

					if (consol.IsSendingAgentGTT() && companyOrgProxiesGuids.Contains(consol.SendingForwarderPK))
					{
						return false;
					}
				}
			}

			return true;
		}

		public static string EFreightStatus(this ForwardingConsol consol, RefAirline carrier)
		{
			var result = string.Empty;

			if (carrier != null && carrier.HasSignedEAWBAgreement)
			{
				var rule = consol != null
					? carrier.EFreightStatusCollection.GetBestMatchRule(consol.LoadPort, consol.DischargePort)
					: null;

				var status = rule != null
					? rule.RME_EFreightStatus.ToString()
					: AWBSpecialHandlingCodeDescriptionPairList.Codes.EFreightConsignmentWithNoAccompanyingPaperDocuments;

				if (status != Core.Constants.EFreightStatus.Code.NON)
				{
					result = status;
				}
			}
			return result;
		}

		public static bool FillGatewayBillingTabForAutorating(this ForwardingConsol consol)
		{
			Argument.NotNull(consol, nameof(consol));

			if (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.Value && consol.IsGateway())
			{
				var companyOrgProxiesGuids = GlbCompany.CurrentCompany.GetAllOrgProxiesIncludingBranches().ToList();

				if (consol.IsSendingAgentGTT() && companyOrgProxiesGuids.Contains(consol.SendingForwarderPK))
				{
					return true;
				}

				if (consol.IsReceivingAgentGTT() && companyOrgProxiesGuids.Contains(consol.ReceivingForwarderPK))
				{
					return true;
				}
			}

			return false;
		}
	}
}
