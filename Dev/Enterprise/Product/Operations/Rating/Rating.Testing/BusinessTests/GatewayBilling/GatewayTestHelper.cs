using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Gateway
{
	public class GatewayTestHelper : TestHelper
	{
		public GatewayTestHelper(BaseGatewayBillingIntegrationTest parentTestCase, BusinessObjectFactory factory) : base(factory)
		{
			ParentTestCase = parentTestCase;
		}

		public (OrgHeader proxy, GlbBranch branch) CreateCompanyAndBranchProxy(string unloco)
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
				var proxy = CreateCreditor("PROXY" + unloco);
				proxy.OH_IsDebtor = true;
				proxy.MainAddress.OA_RL_NKRelatedPortCode = unloco;
				newBranch.GB_OH_OrgProxy = proxy.PK;

				return (proxy, newBranch);
			}
		}

		public (OrgHeader proxy, GlbBranch branch) CreateNonProxyBranch(string unloco, GlbCompany company = null)
		{
			var org = CreateCreditor("Org" + unloco);
			org.OH_IsDebtor = true;
			org.MainAddress.OA_RL_NKRelatedPortCode = unloco;
			Factory.Save();

			var countryCode = unloco.Substring(0, 2);

			if (company == null)
			{
				company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_Name = countryCode + " Company";
				company.GC_RN_NKCountryCode = countryCode;
				company.GC_OH_OrgProxy = NewOrgHeader().PK;
			}

			var newBranch = company.Branches.AddNew();
			newBranch.GB_Code = unloco.Substring(1, 3);
			newBranch.GB_BranchName = unloco + " Branch";
			newBranch.GB_RL_NKHomePort = unloco;
			Factory.Save();

			return (org, newBranch);
		}

		public RateEntry CreateFlatRate(RatingHeader ratingHeader, ZString chargeCode, ZDecimal baseRate,
			string origin, string destination, string plannedLoad = "", string plannedDischarge = "",
			string gatewayAgentType = "", string rateCategory = RatingConstants.RateCategory.AIR, string rateMode = RateMode.LSE)
		{
			var rateEntry = ratingHeader.AddRateEntryWithFlatRateLine(rateCategory, rateMode, origin, destination, chargeCode, baseRate, CurrencyCodes.Australia);
			rateEntry.TI_GatewayAgentType = gatewayAgentType;
			rateEntry.TI_PlannedLoadLRC = plannedLoad;
			rateEntry.TI_PlannedDischargeLRC = plannedDischarge;

			return rateEntry;
		}

		public ForwardingConsol CreateForwardingConsolWithGatewayAgents(
			string origin,
			string destination,
			string transportMode = TransportModes.Air,
			string consolMode = ContainerModes.Loose,
			string paymentType = PaymentType.Collect)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = transportMode;
			consol.JK_ConsolMode = consolMode;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.JK_PrepaidCollect = paymentType;

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = origin;
			transport.JW_RL_NKDiscPort = destination;
			transport.JW_ETD = ZDateTime.Now.AddDays(10);
			transport.JW_ETA = ZDateTime.Now.AddDays(13);
			transport.JW_VoyageFlight = "QF105";

			consol.JK_AgentType = AgentType.Agent;

			return consol;
		}

		public ForwardingShipment CreateGatewayShipmentWithMultipleGatewayConsols(string transportMode, string origin, string destination, string incoterm, params ForwardingConsol[] gatewayConsols)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			shipment.JS_FreightSpotRateAutoratingMode = FreightRateAutoratingModes.Code.StandardRate;
			shipment.JS_ShipmentType = ShipmentTypes.StandardHouse;
			shipment.JS_TransportMode = transportMode;
			shipment.JS_PackingMode = transportMode == TransportModes.Air ? ContainerModes.Loose : ContainerModes.LCL;
			shipment.ConsignorPK = ParentTestCase.Consignor.PK;
			shipment.ConsigneePK = ParentTestCase.Consignee.PK;
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_ActualWeight = 1000m;
			shipment.JS_ActualVolume = 0m;
			shipment.JS_UnitOfWeight = Weight.Kilograms;
			shipment.JS_UnitOfVolume = Volume.CubicMetres;
			shipment.JS_INCO = incoterm;
			shipment.Consols.AddRange(gatewayConsols);

			return shipment;
		}

		public ForwardingShipment CreateGatewayShipment(string jobNumber, string transportMode, string origin, string destination, string incoterm)
		{
			var shipment = ParentTestCase.CreateForwardingShipment(
				transportMode,
				ParentTestCase.Consignor.PK,
				ParentTestCase.Consignee.PK,
				origin,
				destination,
				1000m);
			shipment.JS_INCO = incoterm;
			shipment.JS_UniqueConsignRef = jobNumber;

			return shipment;
		}

		BaseGatewayBillingIntegrationTest ParentTestCase { get; }
	}

	internal static class GatewayTestHelperExtensions
	{
		#region Rate Extensions

		public static T AddFlatRate<T>(this T ratingHeader,
			ZString chargeCode,
			ZDecimal baseRate,
			string origin,
			string destination,
			string plannedLoad = "",
			string plannedDischarge = "",
			string gatewayAgentType = "",
			string rateCategory = RatingConstants.RateCategory.AIR,
			string rateMode = RateMode.LSE)
				where T : RatingHeader
		{
			var rateEntry = ratingHeader.AddRateEntryWithFlatRateLine(rateCategory, rateMode, origin, destination, chargeCode, baseRate, CurrencyCodes.Australia);
			rateEntry.TI_GatewayAgentType = gatewayAgentType;
			rateEntry.TI_PlannedLoadLRC = plannedLoad;
			rateEntry.TI_PlannedDischargeLRC = plannedDischarge;

			return ratingHeader;
		}

		#endregion

		#region Consol Extensions

		public static ForwardingConsol SetSendingAgent(this ForwardingConsol consol, OrgHeader sendingAgent, string handlingType = null, string location = null)
		{
			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;
			if (!string.IsNullOrWhiteSpace(handlingType))
			{
				consol.JK_SendingForwarderHandlingType = handlingType;
				SetUpAppointedGatewayAgentPort(consol.SendingForwarderAddress, location ?? consol.JK_RL_NKLoadPort, handlingType);
			}

			return consol;
		}

		public static ForwardingConsol SetReceivingAgent(this ForwardingConsol consol, OrgHeader receivingAgent, string handlingType = null, string location = null)
		{
			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;
			if (!string.IsNullOrWhiteSpace(handlingType))
			{
				consol.JK_ReceivingForwarderHandlingType = handlingType;
				SetUpAppointedGatewayAgentPort(consol.ReceivingForwarderAddress, location ?? consol.JK_RL_NKDischargePort, handlingType);
			}

			return consol;
		}

		static void SetUpAppointedGatewayAgentPort(OrgAddress gatewayAgentAddress, string location, string handlingType)
		{
			var gatewayAgent = gatewayAgentAddress.Header;
			var agentPorts =
				gatewayAgent.AppointedGatewayAgentPorts
					.Cast<OrgAppointedAgentPorts>()
					.FirstOrDefault(x => x.O5_PortOrCountry == location)
				?? gatewayAgent.AppointedGatewayAgentPorts.AddNew();
			agentPorts.O5_OA_AgentOfficeAddress = gatewayAgentAddress.PK;
			agentPorts.O5_PortOrCountry = location;
			agentPorts.O5_AgentDirection = AgentDirectionList.Codes.Both;
			agentPorts.O5_SeaAgentStatus = handlingType;
			agentPorts.O5_AirAgentStatus = handlingType;
			agentPorts.O5_RailAgentStatus = handlingType;
			agentPorts.O5_RoadAgentStatus = handlingType;
		}

		#endregion

		#region Shipment Extensions

		public static ForwardingShipment AttachConsol(this ForwardingShipment shipment, params ForwardingConsol[] consols)
		{
			if (!consols.IsNullOrEmpty())
			{
				shipment.Consols.AddRange(consols);
			}
			return shipment;
		}

		public static ForwardingShipment AddGatewayAgent(this ForwardingShipment shipment, params OrgHeader[] agents)
		{
			if (agents != null)
			{
				shipment.AddGatewayAgent(agents.Select(x => x.PK).ToArray());
			}
			return shipment;
		}

		public static ForwardingShipment AddGatewayAgent(this ForwardingShipment shipment, params ZGuid[] agentPKs)
		{
			if (!agentPKs.IsNullOrEmpty())
			{
				foreach (var agentPK in agentPKs)
				{
					shipment.Gateways.AddNew().ForwarderPK = agentPK;
				}
			}
			return shipment;
		}

		#endregion
	}
}
