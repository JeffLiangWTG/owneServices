using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Registry.Business.RatingFeatureHelper.CarrierConnect;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingConsolInvoicingSupporter))]
	sealed class ForwardingConsolInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		public void TestGetContainerSpotRates_GatewaySellSpotRatePreferred()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var gatewayAgentPort = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			gatewayAgentPort.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			gatewayAgentPort.O5_PortOrCountry = "AUBNE";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var shipment = consol.Shipments.AddNew();
			var container = consol.Containers.AddNew();
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;

			var packline = shipment.OuterPackLines.AddNew();
			packline.JL_JC = container.PK;

			container.JC_GatewaySellSpotRate = 150m;
			container.JC_SellSpotRate = 180m;

			AssertNoWarnings(container.JC_GatewaySellSpotRateModeInfo);
			var job = new JobHeader.Loader(shipment).TryLoadOrCreate();
			job.LocalChargesPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			var measures = (RateableMeasureSet)shipment.RatingAdapter.RateableMeasures;

			AssertNotNull(shipment.Job);
			var spotRates = measures.GetContainerSpotRates_ForTest();
			AssertEquals("Should get SpotRates", 1, spotRates.Count);
			var spotCostRate = spotRates[0].CostSpotRate;
			var spotSellRate = spotRates[0].SellSpotRate;

			AssertEquals("Should use One Off Freight Reight for Sell", 180m, spotSellRate.Rate.Amount);
			AssertEquals("Should use GatewaySellRate for SpotCost", 150m, spotCostRate.Rate.Amount);
			AssertEquals("Correct SpotRateMode", Constants.FreightRateAutoratingModes.Code.FreightPlusRate, spotCostRate.AutoratedMode);
			AssertEquals("Correct creditor", GlbCompany.CurrentCompany.OrgProxy.PK, spotCostRate.Creditor.PK);
		}

		public void TestCanCreateInvoicingJob()
		{
			var consol_NonGW = Factory.NewWithValidTestData<ForwardingConsol>();
			consol_NonGW.JK_AgentType = Constants.AgentType.Agent;
			consol_NonGW.JK_RL_NKLoadPort = "AUSYD";

			Assert("Precondition: non gateway consol IsGateway", !((IGateway)consol_NonGW).GatewayBillingSupporter.IsGatewayBillingEnabled());

			var consol_GW = Factory.New<ForwardingConsol>();
			consol_GW.JK_TransportMode = Constants.TransportModes.Sea;
			consol_GW.JK_AgentType = Constants.AgentType.Agent;
			consol_GW.JK_RL_NKLoadPort = "AUSYD";
			consol_GW.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol_GW.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var port = consol_GW.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			port.O5_OA_AgentOfficeAddress = consol_GW.JK_OA_SendingForwarderAddress;
			port.O5_PortOrCountry = "AUSYD";
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			var gatewayConsolJob = new JobHeader.Loader(consol_GW).TryLoadOrCreate();

			Assert("Precondition: gateway consol IsGateway", ((IGateway)consol_GW).GatewayBillingSupporter.IsGatewayBillingEnabled());

			Assert("non gateway consol", !new ForwardingConsolInvoicingSupporter(consol_NonGW).CanCreateInvoicingJob);
			Assert("gateway consol", new ForwardingConsolInvoicingSupporter(consol_GW).CanCreateInvoicingJob);
		}

		public void TestJobInvoicingDefaultChargeGroup()
		{
			AssertEquals("DefaultChargeGroup should be Empty", ZString.Empty, Supporter.DefaultChargeGroup);
		}

		public void TestGatewayBranchValidation()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var port = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			port.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			port.O5_PortOrCountry = "AUSYD";
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			var gatewayConsolJob = new JobHeader.Loader(consol).TryLoadOrCreate();
			Factory.Save();

			var gateway = (IGateway)consol;

			Assert("Precondition", consol.IsGateway());
			AssertEquals("Precondition: Branch org proxy is same as the gateway agent", gatewayConsolJob.Branch.GB_OH_OrgProxy, gateway.GatewayBillingSupporter.GatewayAgent().sendingAgent.PK);
			AssertNoErrorContaining(gatewayConsolJob.JH_GBInfo, "Gateway Job Header must match the branch code relating to the Gateway Agent's Org. Proxy.");

			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			gatewayConsolJob.JH_GB = newBranch.PK;
			AssertNotEquals("Precondition: Branch org proxy is not the gateway agent", gatewayConsolJob.Branch.GB_OH_OrgProxy, gateway.GatewayBillingSupporter.GatewayAgent().sendingAgent.PK);
			AssertHasErrorContaining(gatewayConsolJob.JH_GBInfo, "Gateway Job Header must match the branch code relating to the Gateway Agent's Org. Proxy.");

			newBranch.GB_OH_OrgProxy = gateway.GatewayBillingSupporter.GatewayAgent().sendingAgent.PK;
			gatewayConsolJob.JH_GB = newBranch.PK;
			AssertEquals("Precondition: Branch org proxy is same as the gateway agent", gatewayConsolJob.Branch.GB_OH_OrgProxy, gateway.GatewayBillingSupporter.GatewayAgent().sendingAgent.PK);
			AssertNoErrorContaining(gatewayConsolJob.JH_GBInfo, "Gateway Job Header must match the branch code relating to the Gateway Agent's Org. Proxy.");

			gatewayConsolJob.JH_GB = ZGuid.Empty;
			AssertHasErrorContaining(gatewayConsolJob.JH_GBInfo, "Gateway Job Header must match the branch code relating to the Gateway Agent's Org. Proxy.");
		}

		public void TestGatewayBranchValidation_MustHaveGatewayAgent()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_ReceivingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

			var port = consol.ReceivingForwarder.AppointedGatewayAgentPorts.AddNew();
			port.O5_OA_AgentOfficeAddress = consol.JK_OA_ReceivingForwarderAddress;
			port.O5_PortOrCountry = "AU";
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var gateway = (IGateway)consol;

			AssertNoErrors("Precondition: consol is valid for gateway billing", consol.JK_AgentTypeInfo);
			AssertEquals("Precondition: consol has gateway agent", consol.ReceivingForwarderPK, gateway.GatewayBillingSupporter.GatewayAgent().receivingAgent.PK);

			var gatewayConsolJob = new JobHeader.Loader(consol).TryLoadOrCreate();
			Factory.Save();

			var errorMessage = "Gateway Job Header must have a Gateway Agent. Please check either the Sending or Receiving Agent is a valid Gateway Agent for this company.";

			AssertNoErrorContaining(gatewayConsolJob.JH_GBInfo, errorMessage);

			port.O5_PortOrCountry = "GB";
			gatewayConsolJob.Validation.ValidateJH_GB();

			AssertEquals("Pre-condition", ((IOrgHeader)null, (IOrgHeader)null), gateway.GatewayBillingSupporter.GatewayAgent());
			AssertHasErrorContaining(gatewayConsolJob.JH_GBInfo, errorMessage);
		}

		public void TestGatewayBranchValidation_TwoGatewayAgents()
		{
			var org1 = GlbCompany.CurrentCompany.Factory.NewWithValidTestData<OrgHeader>();
			var org2 = GlbCompany.CurrentCompany.Factory.NewWithValidTestData<OrgHeader>();
			var branch1 = GlbCompany.CurrentCompany.ActiveBranches.First(x => x.GB_Code == "SYD");
			branch1.GB_OH_OrgProxy = org1.PK;
			var branch2 = GlbCompany.CurrentCompany.ActiveBranches.First(x => x.GB_Code == "BNE");
			branch2.GB_OH_OrgProxy = org2.PK;
			GlbCompany.CurrentCompany.Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = org2.MainAddress.PK;

			var sendingPort = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			sendingPort.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			sendingPort.O5_PortOrCountry = "CNSHA";
			sendingPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			sendingPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var receivingPort = consol.ReceivingForwarder.AppointedGatewayAgentPorts.AddNew();
			receivingPort.O5_OA_AgentOfficeAddress = consol.JK_OA_ReceivingForwarderAddress;
			receivingPort.O5_PortOrCountry = "AUSYD";
			receivingPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			receivingPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			consol.JK_AgentType = Constants.AgentType.Agent;

			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var gateway = (IGateway)consol;

			AssertNoErrors("Precondition: consol is valid for gateway billing", consol.JK_AgentTypeInfo);
			AssertEquals("Precondition: consol has gateway agent", consol.ReceivingForwarderPK, gateway.GatewayBillingSupporter.GatewayAgent().receivingAgent.PK);
			AssertEquals("Precondition: consol has gateway agent", consol.SendingForwarderPK, gateway.GatewayBillingSupporter.GatewayAgent().sendingAgent.PK);

			var gatewayConsolJob = new JobHeader.Loader(consol).TryLoadOrCreate();
			gatewayConsolJob.JH_GB = branch1.PK;
			AssertEquals(gateway.GatewayBillingSupporter.GatewayAgent().sendingAgent.PK, gatewayConsolJob.Branch.GB_OH_OrgProxy);
			AssertNoError(gatewayConsolJob.JH_GBInfo, "Gateway Job Header must match the branch code relating to the Gateway Agent's Org. Proxy.");

			gatewayConsolJob.JH_GB = branch2.PK;
			AssertEquals(gateway.GatewayBillingSupporter.GatewayAgent().receivingAgent.PK, gatewayConsolJob.Branch.GB_OH_OrgProxy);
			AssertNoError(gatewayConsolJob.JH_GBInfo, "Gateway Job Header must match the branch code relating to the Gateway Agent's Org. Proxy.");

			gatewayConsolJob.JH_GB = ZGuid.Empty;
			AssertHasError(gatewayConsolJob.JH_GBInfo, "Gateway Job Header must match the branch code relating to the Gateway Agent's Org. Proxy.");

			gatewayConsolJob.JH_GB = Factory.NewWithValidTestData<GlbBranch>().PK;
			AssertHasError(gatewayConsolJob.JH_GBInfo, "Gateway Job Header must match the branch code relating to the Gateway Agent's Org. Proxy.");
		}

		public void TestConstantPropertiesImplementation()
		{
			CombineAssertions(delegate
			{
				AssertEquals("GetReasonNotToAllowChangeOnCostDetails", null, Supporter.GetReasonNotToAllowChangeOnCostDetails(ZGuid.NewZGuid()));
				AssertEquals("AuditSecurity", Env.Security.MaintainConsolAuditBilling, Supporter.AuditSecurity);
				AssertEquals("EditSecurityCheckpoint", Env.Security.None, Supporter.EditSecurityCheckpoint);
				AssertEquals("EditSecurityLock", false, Supporter.EditSecurityLock);
				AssertEquals("JobInvoicingSecurity", Env.Security.MaintainConsolJobInvoicing, Supporter.JobInvoicingSecurity);
				AssertEquals("OperationsBranch", GlbBranch.CurrentBranch, Supporter.OperationsBranch);
				AssertEquals("OverriddenDepartmentPK", GlbDepartment.CurrentDepartment.PK.ToGuid(), Supporter.OverriddenDepartmentPK);
			});
		}

		public void TestGatewayConsolDoesNotOverrideDepartment()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var port = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			port.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			port.O5_PortOrCountry = "AUSYD";
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var consolSupporter = new ForwardingConsolInvoicingSupporter(consol);
			AssertEquals(JobInvoicingConsumerTypes.GatewayConsol, consolSupporter.ConsumerType);
			Assert(consolSupporter.OverriddenDepartmentPK.IsEmpty);
		}

		public void TestConsumerType()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			var consolSupporter = new ForwardingConsolInvoicingSupporter(consol);

			AssertEquals(JobInvoicingConsumerTypes.ForwardingConsol, consolSupporter.ConsumerType);

			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var port = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			port.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			port.O5_PortOrCountry = "AUSYD";
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			Assert(consol.IsGateway());
			AssertEquals(JobInvoicingConsumerTypes.GatewayConsol, consolSupporter.ConsumerType);
		}

		public void TestConsolNumber()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_UniqueConsignRef = "C0001";
			var consolSupporter = new ForwardingConsolInvoicingSupporter(consol);

			AssertEquals("C0001", consolSupporter.ConsolNumber);
		}

		public void TestModeAndNumberProperties()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_MasterBillNum = "OBEYTHEMASTA";

			ForwardingConsolInvoicingSupporter supporter = new ForwardingConsolInvoicingSupporter(consol);
			AssertEquals("ConsolType", Constants.AgentType.Agent, supporter.ConsolType);
			AssertEquals("ContainerMode", Constants.ContainerModes.Loose, supporter.ContainerMode);
			AssertEquals("TransportMode", Constants.TransportModes.Air, supporter.TransportMode);
			AssertEquals("MasterBillNumber", "OBEYTHEMASTA", supporter.MasterBillNumber);
		}

		public void TestLocationProperties()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			ForwardingConsolInvoicingSupporter supporter = new ForwardingConsolInvoicingSupporter(consol);
			AssertEquals("Destination", "USLAX", supporter.Destination.RL_Code);
			AssertEquals("Origin", "AUSYD", supporter.Origin.RL_Code);
		}

		public void TestOrganizationProperties()
		{
			OrgHeader receivingForwarder = Factory.New<OrgHeader>();
			OrgHeader sendingForwarder = Factory.New<OrgHeader>();
			OrgHeader creditor = Factory.New<OrgHeader>();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			ForwardingConsolInvoicingSupporter supporter = new ForwardingConsolInvoicingSupporter(consol);
			AssertEquals("Consignee", receivingForwarder, supporter.Consignee);
			AssertEquals("Consignor", sendingForwarder, supporter.Consignor);

			AssertEquals("ReceivingAgent", receivingForwarder, supporter.ReceivingAgent);
			AssertEquals("SendingAgent", sendingForwarder, supporter.SendingAgent);
			AssertEquals("Creditor", creditor, supporter.GetDefaultCreditor(new DefaultCreditorSetting(null, "")));
		}

		public void TestOrganizationProperties_Creditor()
		{
			var receivingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			receivingAgent.OH_IsCreditor = true;
			shipper.OH_IsCreditor = true;
			creditor.OH_IsCreditor = true;

			Factory.Save();

			var shippingAgentPort = shipper.CarrierAppointedAgentPorts_Agency.AddNew();
			shippingAgentPort.O5_OA_AgentOfficeAddress = shipper.MainAddress.PK;
			shippingAgentPort.O5_PortOrCountry = "AUSYD";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;
			consol.JK_OA_ShippingLineAddress = shipper.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			var gatewayAgentPort = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			gatewayAgentPort.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var supporter = new ForwardingConsolInvoicingSupporter(consol);

			AssertNotNull(consol.SendingForwarder);
			Assert("Precondition", consol.IsGateway());
			AssertEquals("Should still be creditor as consol is neither prepaid nor collect", creditor.PK, supporter.GetDefaultCreditor(new DefaultCreditorSetting(null, "")).PK);

			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			AssertEquals("Should still be creditor for collect consol", creditor.PK, supporter.GetDefaultCreditor(new DefaultCreditorSetting(null, "")).PK);

			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			AssertEquals("Should still be creditor for prepaid consol", creditor.PK, supporter.GetDefaultCreditor(new DefaultCreditorSetting(null, "")).PK);
		}

		public void TestGetReasonNotToAllowAutoRateWhenPaymentTermPresent()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;

			var supporter = new ForwardingConsolInvoicingSupporter(consol);

			AssertEquals(true, string.IsNullOrEmpty(supporter.GetReasonNotToAllowAutoRate()));
		}

		public void TestGetReasonNotToAllowAutoRateWhenPaymentTermAbsent()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";

			var supporter = new ForwardingConsolInvoicingSupporter(consol);

			AssertEquals("Payment Term is mandatory for Autorating Consol.", supporter.GetReasonNotToAllowAutoRate());
		}

		public void TestGetReasonNotToAllowAutoRateWhenNoShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.TransportModes.RollOnRollOff;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";

			var cWCarrierConnectFeatureRule = new CWCarrierConnectFeatureRule { Enabled = true };
			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out cWCarrierConnectFeatureRule)).Returns(true);
			featureControlMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.CargoWiseCarrierConnect, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

			ObjectFactory.Substitute(featureControlMock.Object);

			using (RatingDataRegistry.Instance.CargoWiseCarrierConnectForJobAutorating.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var supporter = new ForwardingConsolInvoicingSupporter(consol);
				AssertEquals("Autorating unavailable for an empty Consol. Please add cargo into Consol to continue.", supporter.GetReasonNotToAllowAutoRate());
			}
		}

		public void TestGetReasonNotToAllowAutoRateWhenNoContainer()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CNSHA";

			var cWCarrierConnectFeatureRule = new CWCarrierConnectFeatureRule { Enabled = true };
			var featureDataMock = new Mock<IFeatureData>();
			var featureControlMock = new Mock<IFeatureControlManager>();
			featureDataMock.Setup(x => x.TryDeserializeParameterAsJson(out cWCarrierConnectFeatureRule)).Returns(true);
			featureControlMock.Setup(x => x.GetFeatureDataAsync(CargoWise.Definitions.LicenceFeatureCodeList.Codes.CargoWiseCarrierConnect, CancellationToken.None)).Returns(Task.FromResult(featureDataMock.Object));

			ObjectFactory.Substitute(featureControlMock.Object);

			using (RatingDataRegistry.Instance.CargoWiseCarrierConnectForJobAutorating.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var supporter = new ForwardingConsolInvoicingSupporter(consol);
				AssertEquals("Autorating unavailable for an empty Consol. Please add container into Consol to continue.", supporter.GetReasonNotToAllowAutoRate());
			}
		}

		public void TestMeasureProperties()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_TotalShipmentChargeableUnit = "KG";

			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualVolume = 0.1;
			shipment1.JS_ActualWeight = 100;
			shipment1.JS_ActualChargeable = 110;

			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ActualVolume = 0.2;
			shipment2.JS_ActualWeight = 200;
			shipment2.JS_ActualChargeable = 220;

			ForwardingConsolInvoicingSupporter supporter = new ForwardingConsolInvoicingSupporter(consol);
			AssertEquals("ActualChargeable", 330m, supporter.ActualChargeable);
			AssertEquals("ActualChargeableUnit", "KG", supporter.ActualChargeableUnit);
			AssertEquals("ActualVolume", 0.3m, supporter.ActualVolume);
			AssertEquals("ActualVolumeUnit", "M3", supporter.ActualVolumeUnit);
			AssertEquals("ActualWeight", 300m, supporter.ActualWeight);
			AssertEquals("ActualWeightUnit", "KG", supporter.ActualWeightUnit);
		}

		public void TestDateTimeProperties()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.Transports.MostInterestingTransport.JW_ETD = new ZDateTime(2011, 06, 01);
			consol.Transports.MostInterestingTransport.JW_ETA = new ZDateTime(2011, 07, 01);

			var supporter = consol.InvoicingSupporter;
			AssertEquals("ETD", new ZDateTime(2011, 06, 01), supporter.ETD);
			AssertEquals("ATD", new ZDateTime(2011, 06, 01), supporter.ATD);
			AssertEquals("ETA", new ZDateTime(2011, 07, 01), supporter.ETA);
			AssertEquals("ATA", new ZDateTime(2011, 07, 01), supporter.ATA);

			consol.Transports.MostInterestingTransport.JW_ATD = new ZDateTime(2011, 06, 02);
			consol.Transports.MostInterestingTransport.JW_ATA = new ZDateTime(2011, 07, 02);

			AssertEquals("ETD", new ZDateTime(2011, 06, 01), supporter.ETD);
			AssertEquals("ATD", new ZDateTime(2011, 06, 02), supporter.ATD);
			AssertEquals("ETA", new ZDateTime(2011, 07, 01), supporter.ETA);
			AssertEquals("ATA", new ZDateTime(2011, 07, 02), supporter.ATA);
			AssertEquals("ATL", ZDateTime.Empty, supporter.ArrivalAtLoadPort);
			AssertEquals("Estimated ATL", ZDateTime.Empty, supporter.EstimatedArrivalAtLoadPort);

			ZDateTime today = ZDateTime.Today;
			ZDateTime sailing_E_ATL = today.AddDays(20);
			ZDateTime sailing_A_ATL = today.AddDays(21);

			var vessel = RefVessel.LookupVesselByName("APL IVORY", Factory).First();

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_VoyageFlight = "12";
			voyage.JV_RV_NKVessel = vessel.RV_FK;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";

			var sailing = voyage.Sailings.GetSailingFromLoadAndDischarge("AUSYD", "USLAX");
			AssertNotNull("Sailing should exist", sailing);

			var departureTransport = consol.Transports.MostInterestingTransport;

			departureTransport.JW_IsLinked = false;
			AssertEquals("DepartureTransport is NOT Linked", false, departureTransport.JW_IsLinked);
			AssertEquals("DepartureTransport has NO Sailing", null, departureTransport.Sailing);
			AssertEquals("ATL: Departure Transport is NOT Linked - Empty", ZDateTime.Empty, supporter.ArrivalAtLoadPort);
			AssertEquals("Estimated ATL: Departure Transport is NOT Linked - Empty", ZDateTime.Empty, supporter.EstimatedArrivalAtLoadPort);

			departureTransport.JW_IsLinked = true;
			departureTransport.JW_JX = sailing.PK;
			AssertEquals("DepartureTransport is Linked", true, departureTransport.JW_IsLinked);
			AssertEquals("DepartureTransport has Sailing", sailing, departureTransport.Sailing);
			AssertEquals("Sailing A_ATL: Empty", ZDateTime.Empty, departureTransport.Sailing.Origin.JA_A_ARV);
			AssertEquals("Sailing E_ATL: Empty", ZDateTime.Empty, departureTransport.Sailing.Origin.JA_E_ARV);
			AssertEquals("ATL: Sailing A_ATL and E_ATL are Empty - Empty", ZDateTime.Empty, supporter.ArrivalAtLoadPort);
			AssertEquals("Estimated ATL: Sailing A_ATL and E_ATL are Empty - Empty", ZDateTime.Empty, supporter.EstimatedArrivalAtLoadPort);

			origin.JA_E_ARV = sailing_E_ATL;
			AssertEquals("Sailing A_ATL: Empty", ZDateTime.Empty, departureTransport.Sailing.Origin.JA_A_ARV);
			AssertEquals("Sailing E_ATL: sailing_E_ATL", sailing_E_ATL, departureTransport.Sailing.Origin.JA_E_ARV);
			AssertEquals("ATL: Sailing A_ATL is Empty - Sailing E_ATL", sailing_E_ATL, supporter.ArrivalAtLoadPort);
			AssertEquals("Estimated ATL: Sailing A_ATL is Empty - Sailing E_ATL", sailing_E_ATL, supporter.EstimatedArrivalAtLoadPort);

			origin.JA_A_ARV = sailing_A_ATL;
			AssertEquals("Sailing A_ATL: sailing_A_ATL", sailing_A_ATL, departureTransport.Sailing.Origin.JA_A_ARV);
			AssertEquals("Sailing E_ATL: sailing_E_ATL", sailing_E_ATL, departureTransport.Sailing.Origin.JA_E_ARV);
			AssertEquals("ATL: Sailing A_ATL", sailing_A_ATL, supporter.ArrivalAtLoadPort);
			AssertEquals("Estimated ATL: Sailing A_ATL", sailing_E_ATL, supporter.EstimatedArrivalAtLoadPort);
		}

		public void TestMiscProperties()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			ForwardingConsolInvoicingSupporter supporter = new ForwardingConsolInvoicingSupporter(consol);
			AssertEquals(consol.IsDomesticFreight, supporter.IsDomestic);
			AssertEquals(consol.IsExport(), supporter.IsExport);
			AssertEquals(consol.IsImport(), supporter.IsImport);
			AssertEquals(consol.IsGateway(), supporter.CreateAccountingJobOnSavingOfOperationsJob);
		}

		public void TestContainerCount()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.Containers.AddNew().JC_ContainerCount = 4;
			consol.Containers.AddNew().JC_ContainerCount = 8;

			AssertEquals("Precondition", 2, consol.Containers.Count);
			AssertEquals("Precondition", 12, consol.JK_Calc_ContainerCount);

			JobInvoicingSupporter supporter = new ForwardingConsolInvoicingSupporter(consol);
			AssertEquals("ContainerCount", 12, supporter.ContainerCount);
		}

		public void TestTEUCount()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.Containers.AddNew().JC_ContainerCount = 4;
			consol.Containers.AddNew().JC_ContainerCount = 8;

			AssertEquals("Precondition", 2, consol.Containers.Count);
			AssertEquals("Precondition", new ZDecimal(4 + 8), consol.Containers.TEUCount);

			JobInvoicingSupporter supporter = new ForwardingConsolInvoicingSupporter(consol);
			AssertEquals("TEUCount", 12m, supporter.TEUCount);

			RefContainer refContainer = Factory.New<RefContainer>();
			refContainer.RC_TEU = 10;
			consol.Containers[0].JC_RC = refContainer.PK;

			AssertEquals("Precondition", new ZDecimal(4 * 10 + 8), consol.Containers.TEUCount);
			AssertEquals("TEUCount", consol.Containers.TEUCount, supporter.TEUCount);
		}

		public void TestPaymentType()
		{
			var consol = Factory.New<ForwardingConsol>();
			var consolSupporter = new ForwardingConsolInvoicingSupporter(consol);

			AssertEquals("Pre-condition", ZString.Empty, consol.JK_PrepaidCollect);
			AssertEquals(ZString.Empty, consolSupporter.PaymentType);

			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;

			AssertEquals(Constants.PaymentType.Prepaid, consolSupporter.PaymentType);

			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;

			AssertEquals(Constants.PaymentType.Collect, consolSupporter.PaymentType);
		}

		public void TestPaymentTerm()
		{
			var consol = Factory.New<ForwardingConsol>();
			var consolSupporter = new ForwardingConsolInvoicingSupporter(consol);

			AssertEquals("Pre-condition", ZString.Empty, consol.JK_PrepaidCollect);
			AssertEquals(0, consolSupporter.PaymentTerm.PaymentTermInfoCollection.Count);

			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			AssertEquals(2, consolSupporter.PaymentTerm.PaymentTermInfoCollection.Count);
			AssertEquals(Constants.PaymentType.Prepaid, consolSupporter.PaymentTerm.GetPaymentTermInfo(CostSell.Revenue).Value);

			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
			AssertEquals(2, consolSupporter.PaymentTerm.PaymentTermInfoCollection.Count);
			AssertEquals(Constants.PaymentType.Collect, consolSupporter.PaymentTerm.GetPaymentTermInfo(CostSell.Revenue).Value);
		}

		public void TestVoyageVesselOrFlightDate()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Transports[0].JW_VoyageFlight = "TEST54321";
			consol.Transports[0].JW_Vessel = "ENTERPRISE";

			var supporter = new ForwardingConsolInvoicingSupporter(consol);
			AssertEquals("TEST54321/ENTERPRISE", supporter.VoyageVesselOrFlightDate);
		}

		public void TestValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr()
		{
			var branchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			var proxyBranch = CreateProxyBranch("ABC", GlbCompany.CurrentCompany, branchOrgProxy);

			var gatewayConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			gatewayConsol.JK_TransportMode = Constants.TransportModes.Sea;
			gatewayConsol.JK_AgentType = Constants.AgentType.Agent;
			gatewayConsol.JK_RL_NKLoadPort = "AUSYD";
			gatewayConsol.JK_RL_NKDischargePort = "USLAX";
			gatewayConsol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			gatewayConsol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var port = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			port.O5_PortOrCountry = "AUSYD";
			port.O5_OA_AgentOfficeAddress = gatewayConsol.JK_OA_SendingForwarderAddress;
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			gatewayConsol.SendingForwarder.AppointedGatewayAgentPorts.Add(port);
			var supporter = new ForwardingConsolInvoicingSupporter(gatewayConsol);
			Factory.Save();

			var gateway = (IGateway)gatewayConsol;

			AssertEquals("Precondition: Sending agent is  gateway agent", gatewayConsol.SendingForwarder.PK, gateway.GatewayBillingSupporter.GatewayAgent().sendingAgent.PK);
			var expectedErrorMessage = "Prepaid Agent has to be a proxy of the Gateway Agents Company.";
			Assert("No error", supporter.ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(gatewayConsol.SendingForwarder.PK).IsEmpty);
			AssertEquals("Should found error", expectedErrorMessage, supporter.ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(ZGuid.Empty));
			AssertEquals("Should found error", expectedErrorMessage, supporter.ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(Factory.NewWithValidTestData<OrgHeader>().PK));

			Assert("No error", supporter.ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(branchOrgProxy.PK).IsEmpty);

			proxyBranch.GB_IsActive = false;
			Factory.Save();
			AssertEquals("Should found error", expectedErrorMessage, supporter.ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(branchOrgProxy.PK));

			var anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
			anotherCompany.GC_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().PK;
			Factory.Save();

			gatewayConsol.JK_OA_SendingForwarderAddress = anotherCompany.OrgProxy.MainAddress.PK;
			gatewayConsol.JK_SendingForwarderHandlingType = ZString.Empty;
			gatewayConsol.JK_OA_ReceivingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			gatewayConsol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var rcvPort = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			rcvPort.O5_PortOrCountry = "USLAX";
			rcvPort.O5_OA_AgentOfficeAddress = gatewayConsol.JK_OA_ReceivingForwarderAddress;
			rcvPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			rcvPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			gatewayConsol.ReceivingForwarder.AppointedGatewayAgentPorts.Add(rcvPort);
			supporter = new ForwardingConsolInvoicingSupporter(gatewayConsol);
			Factory.Save();

			AssertNull("Precondition: Sending agent is not gateway agent", gateway.GatewayBillingSupporter.GatewayAgent().sendingAgent);
			AssertNotEquals("Precondition: Sending agent is not gateway agent", gatewayConsol.SendingForwarder.PK, gateway.GatewayBillingSupporter.GatewayAgent().receivingAgent.PK);
			AssertEquals("Precondition: Receiving agent is gateway agent", gatewayConsol.ReceivingForwarder.PK, gateway.GatewayBillingSupporter.GatewayAgent().receivingAgent.PK);
			Assert("No error", supporter.ValidateOnInvoicingSupporter_JH_OA_LocalChargesAddr(ZGuid.Empty).IsEmpty);
		}

		public void TestValidateOnInvoicingSupporter_JH_OA_AgentCollectAddr()
		{
			var branchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			var proxyBranch = CreateProxyBranch("ABC", GlbCompany.CurrentCompany, branchOrgProxy);

			var gatewayConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			gatewayConsol.JK_TransportMode = Constants.TransportModes.Sea;
			gatewayConsol.JK_AgentType = Constants.AgentType.Agent;
			gatewayConsol.JK_RL_NKLoadPort = "AUSYD";
			gatewayConsol.JK_RL_NKDischargePort = "USLAX";
			gatewayConsol.JK_OA_ReceivingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			gatewayConsol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var port = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			port.O5_PortOrCountry = "USLAX";
			port.O5_OA_AgentOfficeAddress = gatewayConsol.JK_OA_ReceivingForwarderAddress;
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			gatewayConsol.ReceivingForwarder.AppointedGatewayAgentPorts.Add(port);

			var supporter = new ForwardingConsolInvoicingSupporter(gatewayConsol);
			Factory.Save();

			var gateway = (IGateway)gatewayConsol;

			AssertEquals("Precondition: Receiving agent is gateway agent", gatewayConsol.ReceivingForwarder.PK, gateway.GatewayBillingSupporter.GatewayAgent().receivingAgent.PK);

			var expectedErrorMessage = "Collect Agent has to be a proxy of the Gateway Agents Company.";
			Assert("No error", supporter.ValidateOnInvoicingSupporter_JH_OA_AgentCollectAddr(gatewayConsol.ReceivingForwarder.PK).IsEmpty);
			AssertEquals("Should found error", expectedErrorMessage, supporter.ValidateOnInvoicingSupporter_JH_OA_AgentCollectAddr(ZGuid.Empty));
			AssertEquals("Should found error", expectedErrorMessage, supporter.ValidateOnInvoicingSupporter_JH_OA_AgentCollectAddr(Factory.NewWithValidTestData<OrgHeader>().PK));

			Assert("No error", supporter.ValidateOnInvoicingSupporter_JH_OA_AgentCollectAddr(branchOrgProxy.PK).IsEmpty);

			proxyBranch.GB_IsActive = false;
			Factory.Save();
			AssertEquals("Should found error", expectedErrorMessage, supporter.ValidateOnInvoicingSupporter_JH_OA_AgentCollectAddr(branchOrgProxy.PK));

			var anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
			anotherCompany.GC_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().PK;
			Factory.Save();

			gatewayConsol.JK_OA_ReceivingForwarderAddress = anotherCompany.OrgProxy.MainAddress.PK;
			gatewayConsol.JK_ReceivingForwarderHandlingType = ZString.Empty;
			gatewayConsol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			gatewayConsol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			var sendPort = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			sendPort.O5_PortOrCountry = "AUSYD";
			sendPort.O5_OA_AgentOfficeAddress = gatewayConsol.JK_OA_SendingForwarderAddress;
			sendPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			sendPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			gatewayConsol.SendingForwarder.AppointedGatewayAgentPorts.Add(sendPort);
			supporter = new ForwardingConsolInvoicingSupporter(gatewayConsol);
			Factory.Save();

			AssertNull("Precondition: Receiving agent is not gateway agent", gateway.GatewayBillingSupporter.GatewayAgent().receivingAgent);
			AssertNotEquals("Precondition: Receiving agent is not gateway agent", gatewayConsol.ReceivingForwarder.PK, gateway.GatewayBillingSupporter.GatewayAgent().sendingAgent.PK);
			AssertEquals("Precondition: Sending agent is gateway agent", gatewayConsol.SendingForwarder.PK, gateway.GatewayBillingSupporter.GatewayAgent().sendingAgent.PK);
			Assert("No error", supporter.ValidateOnInvoicingSupporter_JH_OA_AgentCollectAddr(ZGuid.Empty).IsEmpty);
		}

		public void TestCanGetDefaultDebtorWithoutChargeCode()
		{
			var newRegValue = new GatewayChargeDefaultDebtorConfigurationCollection();
			newRegValue.Add(new GatewayChargeDefaultDebtorConfiguration()
			{
				ConsolDirection = "ALL",
				ConsolTransportMode = "ALL",
				ChargeGroup = "ORG",
				ConsolPaymentTerm = "ALL",
				RelatedJob = "ALL",
				PreviousSendingAgent = "ALL",
				Debtor = "RGT"
			});
			AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultDebtorConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newRegValue);

			newRegValue = new GatewayChargeDefaultDebtorConfigurationCollection();
			newRegValue.Add(new GatewayChargeDefaultDebtorConfiguration()
			{
				ConsolDirection = "ALL",
				ConsolTransportMode = "ALL",
				ChargeGroup = "ALL",
				ConsolPaymentTerm = "ALL",
				RelatedJob = "ALL",
				PreviousSendingAgent = "ALL",
				Debtor = "RGT"
			});
			var allConfig = AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultDebtorConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, newRegValue);

			var branchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			CreateProxyBranch("ABC", GlbCompany.CurrentCompany, branchOrgProxy);

			var gatewayConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			gatewayConsol.JK_TransportMode = Constants.TransportModes.Sea;
			gatewayConsol.JK_AgentType = Constants.AgentType.Agent;
			gatewayConsol.JK_RL_NKLoadPort = "AUSYD";
			gatewayConsol.JK_RL_NKDischargePort = "USLAX";
			gatewayConsol.JK_OA_ReceivingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			gatewayConsol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var port = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			port.O5_PortOrCountry = "USLAX";
			port.O5_OA_AgentOfficeAddress = gatewayConsol.JK_OA_ReceivingForwarderAddress;
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			gatewayConsol.ReceivingForwarder.AppointedGatewayAgentPorts.Add(port);

			var relatedShipment = gatewayConsol.Shipments.AddNew();

			var supporter = new ForwardingConsolInvoicingSupporter(gatewayConsol);
			Factory.Save();

			AssertEquals("Precondition: Receiving agent is gateway agent", gatewayConsol.ReceivingForwarder.PK, ((IGateway)gatewayConsol).GatewayBillingSupporter.GatewayAgent().receivingAgent.PK);
			AssertNotNull("Precondition: valid debtor returned for non-null charge code", supporter.GetDefaultDebtor(Factory.New<AccChargeCode>(), null, relatedShipment.JobNumber));

			AssertNull("Doesn't return debtor by default", supporter.GetDefaultDebtor(null, null, relatedShipment.JobNumber));
			gatewayConsol.SetContext(BusinessContext.GetDefaultDebtorWithoutChargeCode);
			AssertNotNull("Returns debtor when has correct context", supporter.GetDefaultDebtor(null, null, relatedShipment.JobNumber));

			allConfig.Dispose();
			AssertNull("Only uses ALL charge group registry entries when charge code is empty", supporter.GetDefaultDebtor(null, null, relatedShipment.JobNumber));
		}

		public void TestGetDefaultDebtorForGWConsolBasedOnDirection()
		{
			var newRegValue = new GatewayChargeDefaultDebtorConfigurationCollection();
			newRegValue.Add(new GatewayChargeDefaultDebtorConfiguration()
			{
				ConsolDirection = "IMP",
				ConsolTransportMode = "ALL",
				ChargeGroup = "ALL",
				ConsolPaymentTerm = "ALL",
				RelatedJob = "ALL",
				PreviousSendingAgent = "ALL",
				Debtor = "RGT"
			});
			newRegValue.Add(new GatewayChargeDefaultDebtorConfiguration()
			{
				ConsolDirection = "DOM",
				ConsolTransportMode = "ALL",
				ChargeGroup = "ALL",
				ConsolPaymentTerm = "ALL",
				RelatedJob = "ALL",
				PreviousSendingAgent = "ALL",
				Debtor = "SGT"
			});
			newRegValue.Add(new GatewayChargeDefaultDebtorConfiguration()
			{
				ConsolDirection = "EXP",
				ConsolTransportMode = "ALL",
				ChargeGroup = "ALL",
				ConsolPaymentTerm = "ALL",
				RelatedJob = "SHP",
				PreviousSendingAgent = "ALL",
				Debtor = "SPA"
			});
			newRegValue.Add(new GatewayChargeDefaultDebtorConfiguration()
			{
				ConsolDirection = "OTH",
				ConsolTransportMode = "ALL",
				ChargeGroup = "ALL",
				ConsolPaymentTerm = "ALL",
				RelatedJob = "SHP",
				PreviousSendingAgent = "ALL",
				Debtor = "SDA"
			});
			AccountingMasterFilesRegistry.Instance.GatewayChargeDefaultDebtorConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newRegValue);

			var branchOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			CreateProxyBranch("ABC", GlbCompany.CurrentCompany, branchOrgProxy);

			var gatewayConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			gatewayConsol.JK_TransportMode = Constants.TransportModes.Sea;
			gatewayConsol.JK_AgentType = Constants.AgentType.Agent;
			gatewayConsol.JK_RL_NKLoadPort = "USLAX";
			gatewayConsol.JK_RL_NKDischargePort = "AUSYD";
			gatewayConsol.JK_OA_ReceivingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			gatewayConsol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var port = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			port.O5_PortOrCountry = "AUSYD";
			port.O5_OA_AgentOfficeAddress = gatewayConsol.JK_OA_ReceivingForwarderAddress;
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			gatewayConsol.ReceivingForwarder.AppointedGatewayAgentPorts.Add(port);

			var shipment = gatewayConsol.Shipments.AddNew();
			var pickupAgent = Factory.NewWithValidTestData<OrgHeader>();
			pickupAgent.MainAddress.OA_Address1 = "PA MAIN ADDRESS";
			var deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			deliveryAgent.MainAddress.OA_Address1 = "DA MAIN ADDRESS";
			shipment.PickupAgentDocumentaryAddress.OrganisationPK = pickupAgent.PK;
			shipment.JS_OH_DeliveryAgent = deliveryAgent.PK;

			var supporter = new ForwardingConsolInvoicingSupporter(gatewayConsol);
			Factory.Save();

			AssertEquals("Expect result based on Import direction", GlbCompany.CurrentCompany.OrgProxy.PK, supporter.GetDefaultDebtor(Factory.New<AccChargeCode>(), null, shipment.JobNumber).PK);

			gatewayConsol.JK_RL_NKLoadPort = "AUMEL";
			gatewayConsol.JK_OA_SendingForwarderAddress = branchOrgProxy.MainAddress.PK;
			AssertEquals("Expect result based on Domestic direction", branchOrgProxy.PK, supporter.GetDefaultDebtor(Factory.New<AccChargeCode>(), null, shipment.JobNumber).PK);

			gatewayConsol.JK_RL_NKLoadPort = "AUSYD";
			gatewayConsol.JK_RL_NKDischargePort = "USLAX";
			gatewayConsol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			gatewayConsol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			AssertEquals("Expect result based on Export direction", pickupAgent.PK, supporter.GetDefaultDebtor(Factory.New<AccChargeCode>(), null, shipment.JobNumber).PK);

			gatewayConsol.JK_RL_NKLoadPort = "NZALK";
			gatewayConsol.JK_RL_NKDischargePort = "USLAX";
			gatewayConsol.JK_OA_ReceivingForwarderAddress = branchOrgProxy.MainAddress.PK;
			gatewayConsol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			port = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
			port.O5_PortOrCountry = "USLAX";
			port.O5_OA_AgentOfficeAddress = gatewayConsol.JK_OA_ReceivingForwarderAddress;
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			gatewayConsol.ReceivingForwarder.AppointedGatewayAgentPorts.Add(port);
			AssertEquals("Expect result based on Other direction", deliveryAgent.PK, supporter.GetDefaultDebtor(Factory.New<AccChargeCode>(), null, shipment.JobNumber).PK);
		}

		#region Implementation

		ForwardingConsolInvoicingSupporter Supporter
		{
			get { return supporter ?? (supporter = GetNewBusinessObject().InvoicingSupporter as ForwardingConsolInvoicingSupporter); }
		}

		ForwardingConsolInvoicingSupporter supporter;

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			ForwardingConsol forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			return forwardingConsol;
		}

		GlbBranch CreateProxyBranch(string code, GlbCompany company, OrgHeader organisationProxy)
		{
			var proxyBranch = Factory.NewWithValidTestData<GlbBranch>();
			proxyBranch.GB_Code = code;
			proxyBranch.GB_GC = company.PK;
			var unlocoQuery = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, company.GC_RN_NKCountryCode);
			unlocoQuery.OrderBy = RefUNLOCOSchema.Constants.RL_Code;
			proxyBranch.GB_RL_NKHomePort = Factory.LoadTop1<RefUNLOCO>(unlocoQuery).Code;
			proxyBranch.GB_OH_OrgProxy = organisationProxy.PK;
			return proxyBranch;
		}

		#endregion
	}
}
