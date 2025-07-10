using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingConsolGatewayBillingSupporter : IGatewayBillingSupporter
	{
		public ForwardingConsolGatewayBillingSupporter(ForwardingConsol consol)
		{
			this.consol = consol;
		}

		readonly ForwardingConsol consol;

		public bool IsGatewayBillingEnabled(ICompany iCompany)
		{
			var company = iCompany as GlbCompany ?? GlbCompany.CurrentCompany;
			if (consol.IsGatewayConsol)
			{
				if (GetConsolJob(company) != null)
				{
					return true;
				}

				var isGateway = GatewayAgent(company) != (null, null);
				return isGateway;
			}

			return false;
		}

		#region Gateway Agent

		public bool IsSendingAgentGateway(GlbCompany company, string sendingForwarderHandlingTypeOverride = null)
		{
			var sendingForwarderHandlingType = !string.IsNullOrEmpty(sendingForwarderHandlingTypeOverride)
				? new ZString(sendingForwarderHandlingTypeOverride)
				: consol.JK_SendingForwarderHandlingType;
			return IsGatewayAppointedOrgProxyForPort(consol.SendingForwarder, consol.LoadPort, sendingForwarderHandlingType, company, MatchSendingAgentDirection);
		}

		public bool IsReceivingAgentGateway(GlbCompany company, string receivingForwarderHandlingTypeOverride = null)
		{
			var receivingForwarderHandlingType = !string.IsNullOrEmpty(receivingForwarderHandlingTypeOverride)
				? new ZString(receivingForwarderHandlingTypeOverride)
				: consol.JK_ReceivingForwarderHandlingType;
			return IsGatewayAppointedOrgProxyForPort(consol.ReceivingForwarder, consol.DischargePort, receivingForwarderHandlingType, company, MatchReceivingAgentDirection);
		}

		public bool IsSendingAgentGateway(GlbCompany company, ZGuid sendingForwarderAddressOverride, ZString? sendingForwarderHandlingTypeOverride)
		{
			var sendingForwarder = !sendingForwarderAddressOverride.IsEmpty
				? consol.Factory.Load<OrgAddress>(sendingForwarderAddressOverride)?.Header
				: consol.SendingForwarder;
			var sendingHandlingType = sendingForwarderHandlingTypeOverride ?? consol.JK_SendingForwarderHandlingType;
			return IsGatewayAppointedOrgProxyForPort(sendingForwarder, consol.LoadPort, sendingHandlingType, company, MatchSendingAgentDirection);
		}

		public bool IsReceivingAgentGateway(GlbCompany company, ZGuid receivingForwarderAddressOverride, ZString? receivingForwarderHandlingTypeOverride)
		{
			var receivingForwarder = !receivingForwarderAddressOverride.IsEmpty
				? consol.Factory.Load<OrgAddress>(receivingForwarderAddressOverride)?.Header
				: consol.ReceivingForwarder;
			var receivingHandlingType = receivingForwarderHandlingTypeOverride ?? consol.JK_ReceivingForwarderHandlingType;
			return IsGatewayAppointedOrgProxyForPort(receivingForwarder, consol.DischargePort, receivingHandlingType, company, MatchReceivingAgentDirection);
		}

		public (IOrgHeader sendingAgent, IOrgHeader receivingAgent) GatewayAgent(ICompany iCompany = null)
		{
			IOrgHeader sendingAgentOrg = null;
			IOrgHeader receivingAgentOrg = null;
			var company = iCompany as GlbCompany ?? GlbCompany.CurrentCompany;

			var isSendingAgentGateway = IsSendingAgentGateway(company);
			var isReceivingAgentGateway = IsReceivingAgentGateway(company);

			if (isSendingAgentGateway)
			{
				sendingAgentOrg = consol.SendingForwarder;
			}
			if (isReceivingAgentGateway)
			{
				receivingAgentOrg = consol.ReceivingForwarder;
			}
			return (sendingAgent: sendingAgentOrg, receivingAgent: receivingAgentOrg);
		}

		#region Gateway Handling Types

		public ZString GetDefaultSendingForwarderAddressGatewayType(GlbCompany company = null)
		{
			return GetDefaultGatewayHandlingTypeForPort(consol.SendingForwarderAddress, consol.LoadPort, company, MatchSendingAgentDirection);
		}

		public ZString GetDefaultReceivingForwarderAddressGatewayType(GlbCompany company = null)
		{
			return GetDefaultGatewayHandlingTypeForPort(consol.ReceivingForwarderAddress, consol.DischargePort, company, MatchReceivingAgentDirection);
		}

		ZString GetDefaultGatewayHandlingTypeForPort(OrgAddress address, RefUNLOCO port, GlbCompany company, Func<OrgAppointedAgentPorts, bool> matchDirection)
		{
			if (address == null
				|| port == null)
			{
				return ZString.Empty;
			}

			company = company ?? GlbCompany.CurrentCompany;
			if (!OrganisationExistsAsOrgProxy(address.Header, company))
			{
				return ZString.Empty;
			}

			var portsWithValidDirections = address.Header
				.AppointedGatewayAgentPorts
				.OfType<OrgAppointedAgentPorts>()
				.Where(p => p.O5_OA_AgentOfficeAddress == address.PK
					&& !p.O5_PortOrCountry.IsEmpty
					&& matchDirection(p));

			var matchedPort = portsWithValidDirections.FirstOrDefault(x => x.O5_PortOrCountry == port.RL_Code);
			if (matchedPort != null)
			{
				var status = GetStatusForCurrentTransportMode(matchedPort);
				if (!status.IsEmpty)
				{
					return status;
				}
			}

			var matchedCountry = portsWithValidDirections.FirstOrDefault(x => x.O5_PortOrCountry == (port.Country?.Code ?? ZString.Empty));
			if (matchedCountry != null)
			{
				return GetStatusForCurrentTransportMode(matchedCountry);
			}

			return ZString.Empty;
		}

		#endregion

		#region Implementation

		bool OrganisationExistsAsOrgProxy(OrgHeader organisation, GlbCompany company)
		{
			if (organisation == null
				|| company == null)
			{
				return false;
			}

			return company.GC_OH_OrgProxy == organisation.PK
				|| company.ActiveBranches.Any(branch => branch.GB_IsActive && branch.GB_OH_OrgProxy == organisation.PK);
		}

		bool IsGatewayAppointedOrgProxyForPort(OrgHeader organization, RefUNLOCO port, string forwarderHandlingType, GlbCompany company, Func<OrgAppointedAgentPorts, bool> matchAgentDirection)
		{
			if (organization == null
				|| port == null
				|| company == null
				|| !company.GC_IsActive
				|| !FreightCodePairLists.GatewayForwarderHandlingTypeList().ContainsCode(forwarderHandlingType)
				|| !OrganisationExistsAsOrgProxy(organization, company))
			{
				return false;
			}

			return organization.AppointedGatewayAgentPorts
				.Cast<OrgAppointedAgentPorts>()
				.Any(x => (x.O5_PortOrCountry == port.RL_Code
					|| port.Country != null && x.O5_PortOrCountry == port.Country.Code)
					&& MatchConsolTransportMode(x)
					&& matchAgentDirection(x));
		}

		bool MatchConsolTransportMode(OrgAppointedAgentPorts agentPorts)
		{
			return !GetStatusForCurrentTransportMode(agentPorts).IsEmpty;
		}

		ZString GetStatusForCurrentTransportMode(OrgAppointedAgentPorts agentPorts)
		{
			if (agentPorts == null)
			{
				return ZString.Empty;
			}

			ZString status;
			switch (consol.JK_TransportMode)
			{
				case Core.Constants.TransportModes.Sea:
					status = agentPorts.O5_SeaAgentStatus;
					break;

				case Core.Constants.TransportModes.Air:
					status = agentPorts.O5_AirAgentStatus;
					break;

				case Core.Constants.TransportModes.Rail:
					status = agentPorts.O5_RailAgentStatus;
					break;

				case Core.Constants.TransportModes.Road:
					status = agentPorts.O5_RoadAgentStatus;
					break;

				default:
					return ZString.Empty;
			}

			if (consol.IsGatewayAgentOrGatewayAgentWithTariff(status))
			{
				return status;
			}

			return ZString.Empty;
		}

		bool MatchReceivingAgentDirection(OrgAppointedAgentPorts agentPorts)
		{
			return agentPorts.O5_AgentDirection == AgentDirectionList.Codes.Both
				|| agentPorts.O5_AgentDirection == AgentDirectionList.Codes.Import;
		}

		bool MatchSendingAgentDirection(OrgAppointedAgentPorts agentPorts)
		{
			return agentPorts.O5_AgentDirection == AgentDirectionList.Codes.Both
				|| agentPorts.O5_AgentDirection == AgentDirectionList.Codes.Export;
		}

		#endregion

		#region Consol Job

		internal JobHeader GetConsolJob(GlbCompany company)
		{
			return !consol.IsLegacyGateway ? GetCompanyJob(company) : null;
		}

		JobHeader GetCompanyJob(GlbCompany company)
		{
			var jobHeaderParent = consol as IJobHeaderParent;
			return company != null && jobHeaderParent != null ? new JobHeader.Loader(jobHeaderParent).Load(true, company, false) : null;
		}

		#endregion

		#region Is Legacy Gateway

		internal bool IsLegacyGateway
		{
			get
			{
				return consol.Factory.GetCachedValue(GetLegacyGatewayCacheKey(), () =>
				{
					var job = consol.Job;
					return job != null && job.IsInDatabase && job.IsGatewayLegacyJob;
				});
			}
		}

		string GetLegacyGatewayCacheKey() => string.Concat("HasLegacyGatewayJob", consol.PK);

		#endregion

		#region Non-Gateway Consol Costs Exist For Company

		internal bool NonGatewayConsolCostsExistForCompany(GlbCompany company)
		{
			var result = false;

			if (consol.HasConsolCosts(company))
			{
				result = true;

				var companyJob = GetCompanyJob(company);
				if (companyJob != null)
				{
					var gatewayChargeQuery = new ZQuery(JobChargeSchema.JR_JH, companyJob.PK);
					gatewayChargeQuery.AddToFilter(JobChargeSchema.JR_E6_GatewaySellHeader, SQLComparisonOperator.NotEqual, DBNull.Value);

					var gatewayCharges = consol.Factory.Load<JobCharge>(gatewayChargeQuery);
					if (gatewayCharges.Any())
					{
						var sellHeaderPKs = gatewayCharges.Select(x => x.JR_E6_GatewaySellHeader).ToArray();

						var consolCostQuery = new ZQuery(JobConsolCostSchema.E6_GC, company.PK);
						consolCostQuery.AddToFilter(JobConsolCostSchema.E6_ParentID, consol.PK);
						consolCostQuery.AddToFilter(JobConsolCostSchema.PK, SQLComparisonOperator.NotEqual, sellHeaderPKs);

						result = consol.Factory.LoadTop1<Enterprise.Integration.Accounting.IJobConsolCost>(consolCostQuery) != null;
					}
				}
			}

			return result;
		}

		#endregion

		internal GlbCompany[] AllGlbCompanies
		{
			get { return consol.Factory.GetCachedValue("GlbCompany", () => consol.Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_IsActive, true))); }
		}

		#endregion
	}
}
