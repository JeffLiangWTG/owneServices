using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonConsolForwardersListTest : BaseFreightTest
	{
		[SnailTest]
		public void TestSendingForwarderListFilter()
		{
			OrgHeader c_AIR_HAN_BTH = GetForwarderAgentAddress(AgentStatusHandles, Country.Code, AgentDirectionBoth, Constants.TransportModes.Air).Header;
			OrgHeader f_AIR_APP_EXP = GetForwarderAgentAddress(AgentStatusAppointed, FirstLoco.Code, AgentDirectionExport, Constants.TransportModes.Air).Header;
			OrgHeader c_AIR_PUB_IMP = GetForwarderAgentAddress(AgentStatusPublished, Country.Code, AgentDirectionImport, Constants.TransportModes.Air).Header;

			OrgHeader s_SEA_PUB_BTH = GetForwarderAgentAddress(AgentStatusPublished, SecondLoco.Code, AgentDirectionBoth, Constants.TransportModes.Sea).Header;
			OrgHeader c_SEA_HAN_EXP = GetForwarderAgentAddress(AgentStatusHandles, Country.Code, AgentDirectionExport, Constants.TransportModes.Sea).Header;
			OrgHeader f_SEA_APP_IMP = GetForwarderAgentAddress(AgentStatusAppointed, FirstLoco.Code, AgentDirectionImport, Constants.TransportModes.Sea).Header;

			PrepareConsolForwarderList(Constants.AgentType.Direct, FirstLoco.Code, "", "");
			Assert("List should be forwarder list.", Consol.SendingForwarderList is ForwarderCollection);
			Assert("Forwarder list should be filtered by Loading port.", Consol.SendingForwarderList.FilterBusinessObjectDefaults.ContainsDefaultFor("Main UNLOCO" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertForwarderList(Consol.SendingForwarderList, "Forwarders for first port should be in the list", c_AIR_HAN_BTH, f_AIR_APP_EXP, c_SEA_HAN_EXP);

			PrepareConsolForwarderList(Constants.AgentType.Direct, SecondLoco.Code, "", "");
			AssertForwarderList(Consol.SendingForwarderList, "Forwarders for second port should be in the list", c_AIR_HAN_BTH, s_SEA_PUB_BTH, c_SEA_HAN_EXP);

			PrepareConsolForwarderList(Constants.AgentType.Agent, FirstLoco.Code, "", "");
			AssertForwarderList(Consol.SendingForwarderList, "Forwarders for first port should be in the list", c_AIR_HAN_BTH, f_AIR_APP_EXP, c_SEA_HAN_EXP);

			PrepareConsolForwarderList(Constants.AgentType.Agent, SecondLoco.Code, "", "");
			AssertForwarderList(Consol.SendingForwarderList, "Forwarders for second port should be in the list", c_AIR_HAN_BTH, s_SEA_PUB_BTH, c_SEA_HAN_EXP);

			PrepareConsolForwarderList(Constants.AgentType.Agent, FirstLoco.Code, "", Constants.TransportModes.Air);
			AssertForwarderList(Consol.SendingForwarderList, "Air Forwarder for first port should be in the list", c_AIR_HAN_BTH, f_AIR_APP_EXP);

			PrepareConsolForwarderList(Constants.AgentType.Agent, FirstLoco.Code, "", Constants.TransportModes.Sea);
			AssertForwarderList(Consol.SendingForwarderList, "Sea Forwarder for first port should be in the list", c_SEA_HAN_EXP);

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = c_AIR_PUB_IMP.PK;
			PrepareConsolForwarderList(Constants.AgentType.Agent, "", "", Constants.TransportModes.Sea);
			AssertForwarderList(Consol.SendingForwarderList, "Current Branch's Org", c_AIR_PUB_IMP);
			Assert("Forwarders List + Current Branch's Org", Consol.SendingForwarderList.Count > 1);
		}

		[SnailTest]
		public void TestReceivingForwarderListFilter()
		{
			OrgHeader c_AIR_HAN_BTH = GetForwarderAgentAddress(AgentStatusHandles, Country.Code, AgentDirectionBoth, Constants.TransportModes.Air).Header;
			OrgHeader f_AIR_APP_EXP = GetForwarderAgentAddress(AgentStatusAppointed, FirstLoco.Code, AgentDirectionExport, Constants.TransportModes.Air).Header;
			OrgHeader c_AIR_PUB_IMP = GetForwarderAgentAddress(AgentStatusPublished, Country.Code, AgentDirectionImport, Constants.TransportModes.Air).Header;

			OrgHeader s_SEA_PUB_BTH = GetForwarderAgentAddress(AgentStatusPublished, SecondLoco.Code, AgentDirectionBoth, Constants.TransportModes.Sea).Header;
			OrgHeader c_SEA_HAN_EXP = GetForwarderAgentAddress(AgentStatusHandles, Country.Code, AgentDirectionExport, Constants.TransportModes.Sea).Header;
			OrgHeader f_SEA_APP_IMP = GetForwarderAgentAddress(AgentStatusAppointed, FirstLoco.Code, AgentDirectionImport, Constants.TransportModes.Sea).Header;

			PrepareConsolForwarderList(Constants.AgentType.Direct, "", FirstLoco.RL_Code, "");
			Assert("List should be forwarder list.", Consol.ReceivingForwarderList is ForwarderCollection);
			Assert("Forwarder list should be filtered by Discharge port.", Consol.ReceivingForwarderList.FilterBusinessObjectDefaults.ContainsDefaultFor("Main UNLOCO" + FilterBusinessObjectDefault.FilterPropertyDelimiter + "Property"));
			AssertForwarderList(Consol.ReceivingForwarderList, "Forwarders for first port should be in the list", c_AIR_HAN_BTH, c_AIR_PUB_IMP, f_SEA_APP_IMP);

			PrepareConsolForwarderList(Constants.AgentType.Direct, "", SecondLoco.RL_Code, "");
			AssertForwarderList(Consol.ReceivingForwarderList, "Forwarders for second port should be in the list", c_AIR_HAN_BTH, c_AIR_PUB_IMP, s_SEA_PUB_BTH);

			PrepareConsolForwarderList(Constants.AgentType.Agent, "", FirstLoco.RL_Code, "");
			AssertForwarderList(Consol.ReceivingForwarderList, "Forwarders for first port should be in the list", c_AIR_HAN_BTH, c_AIR_PUB_IMP, f_SEA_APP_IMP);

			PrepareConsolForwarderList(Constants.AgentType.Agent, "", SecondLoco.RL_Code, "");
			AssertForwarderList(Consol.ReceivingForwarderList, "Forwarders for second port should be in the list", c_AIR_HAN_BTH, c_AIR_PUB_IMP, s_SEA_PUB_BTH);

			PrepareConsolForwarderList(Constants.AgentType.Agent, "", FirstLoco.RL_Code, Constants.TransportModes.Air);
			AssertForwarderList(Consol.ReceivingForwarderList, "Air Forwarder for first port should be in the list", c_AIR_HAN_BTH, c_AIR_PUB_IMP);

			PrepareConsolForwarderList(Constants.AgentType.Agent, "", FirstLoco.RL_Code, Constants.TransportModes.Sea);
			AssertForwarderList(Consol.ReceivingForwarderList, "Sea Forwarder for first port should be in the list", f_SEA_APP_IMP);

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = f_AIR_APP_EXP.PK;
			PrepareConsolForwarderList(Constants.AgentType.Agent, "", "", Constants.TransportModes.Sea);
			Consol.ReceivingForwarderList.Load();
			AssertForwarderList(Consol.ReceivingForwarderList, "No location filter, current Branch Org should be in list.", f_AIR_APP_EXP);
		}

		[SnailTest]
		public void TestSendingForwarderListWhenValidationOfAgentsIsSuppressed()
		{
			using (FreightDataRegistry.Instance.SuppressValidationOfConsolsSendingOrReceivingAgents.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var org = GetForwarderAgentAddress(ZString.Empty, FirstLoco.RL_Code, ZString.Empty, Constants.TransportModes.Sea).Header;

				PrepareConsolForwarderList(Constants.AgentType.Agent, "", FirstLoco.RL_Code, Constants.TransportModes.Sea);
				AssertForwarderList(Consol.SendingForwarderList, "Should NOT filter Appointed agent ports when registry is enabled.", org);
			}

			using (FreightDataRegistry.Instance.SuppressValidationOfConsolsSendingOrReceivingAgents.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var org2 = GetForwarderAgentAddress(ZString.Empty, FirstLoco.RL_Code, ZString.Empty, Constants.TransportModes.Air).Header;

				PrepareConsolForwarderList(Constants.AgentType.Agent, "", FirstLoco.RL_Code, Constants.TransportModes.Air);
				Consol.SendingForwarderList.Load();
				AssertCollectionNotContains("Should filter Appointed agent ports when registry is disabled.", org2, Consol.SendingForwarderList);
			}
		}

		[SnailTest]
		public void TestReceivingForwarderListWhenValidationOfAgentsIsSuppressed()
		{
			using (FreightDataRegistry.Instance.SuppressValidationOfConsolsSendingOrReceivingAgents.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var org = GetForwarderAgentAddress(ZString.Empty, FirstLoco.RL_Code, ZString.Empty, Constants.TransportModes.Sea).Header;

				PrepareConsolForwarderList(Constants.AgentType.Agent, "", FirstLoco.RL_Code, Constants.TransportModes.Sea);
				AssertForwarderList(Consol.ReceivingForwarderList, "Should NOT filter Appointed agent ports when registry is enabled.", org);
			}

			using (FreightDataRegistry.Instance.SuppressValidationOfConsolsSendingOrReceivingAgents.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var org2 = GetForwarderAgentAddress(ZString.Empty, FirstLoco.RL_Code, ZString.Empty, Constants.TransportModes.Air).Header;

				PrepareConsolForwarderList(Constants.AgentType.Agent, "", FirstLoco.RL_Code, Constants.TransportModes.Air);
				Consol.ReceivingForwarderList.Load();
				AssertCollectionNotContains("Should filter Appointed agent ports when registry is disabled.", org2, Consol.ReceivingForwarderList);
			}
		}

		[SnailTest]
		public void TestCreditorListFilter()
		{
			OrgHeader c_AIR_HAN_BTH = GetForwarderAgentAddress(AgentStatusHandles, Country.Code, AgentDirectionBoth, Constants.TransportModes.Air).Header;
			OrgHeader f_AIR_APP_EXP = GetForwarderAgentAddress(AgentStatusAppointed, FirstLoco.Code, AgentDirectionExport, Constants.TransportModes.Air).Header;
			OrgHeader c_AIR_PUB_IMP = GetForwarderAgentAddress(AgentStatusPublished, Country.Code, AgentDirectionImport, Constants.TransportModes.Air).Header;

			OrgHeader s_SEA_PUB_BTH = GetForwarderAgentAddress(AgentStatusPublished, SecondLoco.Code, AgentDirectionBoth, Constants.TransportModes.Sea).Header;
			OrgHeader c_SEA_HAN_EXP = GetForwarderAgentAddress(AgentStatusHandles, Country.Code, AgentDirectionExport, Constants.TransportModes.Sea).Header;
			OrgHeader f_SEA_APP_IMP = GetForwarderAgentAddress(AgentStatusAppointed, FirstLoco.Code, AgentDirectionImport, Constants.TransportModes.Sea).Header;

			PrepareConsolForwarderList(Constants.AgentType.CoLoad, FirstLoco.RL_Code, "", "");
			AssertForwarderList(Consol.CreditorList, "Forwarders for first port should be in the list", FirstSeaForwarder, FirstAirForwarder);

			PrepareConsolForwarderList(Constants.AgentType.CoLoad, SecondLoco.RL_Code, "", "");
			AssertForwarderList(Consol.CreditorList, "Forwarders for second port should be in the list", SecondSeaForwarder, SecondAirForwarder);

			PrepareConsolForwarderList(Constants.AgentType.CoLoad, SecondLoco.RL_Code, "", "");
			AssertForwarderList(Consol.CreditorList, "Forwarders for second port should be in the list", SecondSeaForwarder, SecondAirForwarder);

			PrepareConsolForwarderList(Constants.AgentType.CoLoad, FirstLoco.RL_Code, "", Constants.TransportModes.Air);
			AssertForwarderList(Consol.CreditorList, "Air Forwarder for first port should be in the list", FirstAirForwarder);

			PrepareConsolForwarderList(Constants.AgentType.CoLoad, FirstLoco.RL_Code, "", Constants.TransportModes.Sea);
			AssertForwarderList(Consol.CreditorList, "Sea Forwarder for first port should be in the list", FirstSeaForwarder);

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = SecondSeaForwarder.PK;

			PrepareConsolForwarderList(Constants.AgentType.CoLoad, "", "", Constants.TransportModes.Sea);
			AssertForwarderList(Consol.CreditorList, "Current Branch's Org", SecondSeaForwarder);
			Assert("Forwarders List + Current Branch's Org", Consol.CreditorList.Count > 1);
		}

		[SnailTest]
		public void TestCreditorListFilter_ForGatewayCoLoad()
		{
			OrgHeader c_AIR_HAN_BTH = GetForwarderAgentAddress(AgentStatusHandles, Country.Code, AgentDirectionBoth, Constants.TransportModes.Air).Header;
			OrgHeader f_AIR_APP_EXP = GetForwarderAgentAddress(AgentStatusAppointed, FirstLoco.Code, AgentDirectionExport, Constants.TransportModes.Air).Header;
			OrgHeader c_AIR_PUB_IMP = GetForwarderAgentAddress(AgentStatusPublished, Country.Code, AgentDirectionImport, Constants.TransportModes.Air).Header;

			OrgHeader s_SEA_PUB_BTH = GetForwarderAgentAddress(AgentStatusPublished, SecondLoco.Code, AgentDirectionBoth, Constants.TransportModes.Sea).Header;
			OrgHeader c_SEA_HAN_EXP = GetForwarderAgentAddress(AgentStatusHandles, Country.Code, AgentDirectionExport, Constants.TransportModes.Sea).Header;
			OrgHeader f_SEA_APP_IMP = GetForwarderAgentAddress(AgentStatusAppointed, FirstLoco.Code, AgentDirectionImport, Constants.TransportModes.Sea).Header;

			Consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			PrepareConsolForwarderList(Constants.AgentType.CoLoad, FirstLoco.RL_Code, "", "");
			AssertForwarderList(Consol.CreditorList, "Forwarders for first port should be in the list", FirstSeaForwarder, FirstAirForwarder);

			PrepareConsolForwarderList(Constants.AgentType.CoLoad, SecondLoco.RL_Code, "", "");
			AssertForwarderList(Consol.CreditorList, "Forwarders for second port should be in the list", SecondSeaForwarder, SecondAirForwarder);

			PrepareConsolForwarderList(Constants.AgentType.CoLoad, SecondLoco.RL_Code, "", "");
			AssertForwarderList(Consol.CreditorList, "Forwarders for second port should be in the list", SecondSeaForwarder, SecondAirForwarder);

			PrepareConsolForwarderList(Constants.AgentType.CoLoad, FirstLoco.RL_Code, "", Constants.TransportModes.Air);
			AssertForwarderList(Consol.CreditorList, "Air Forwarder for first port should be in the list", FirstAirForwarder);

			PrepareConsolForwarderList(Constants.AgentType.CoLoad, FirstLoco.RL_Code, "", Constants.TransportModes.Sea);
			AssertForwarderList(Consol.CreditorList, "Sea Forwarder for first port should be in the list", FirstSeaForwarder);

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = SecondSeaForwarder.PK;

			PrepareConsolForwarderList(Constants.AgentType.CoLoad, "", "", Constants.TransportModes.Sea);
			AssertForwarderList(Consol.CreditorList, "Current Branch's Org", SecondSeaForwarder);
			Assert("Forwarders List + Current Branch's Org", Consol.CreditorList.Count > 1);
		}

		#region Test Creditor Label

		public void TestCreditorLabelAndReadOnly()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			AssertEquals("JK_OA_CreditorAddress should never be read only", false, consol.JK_OA_CreditorAddressInfo.ReadOnly);
			consol.JK_AgentType = Constants.AgentType.Agent;
			AssertEquals("JK_OA_CreditorAddress should never be read only", false, consol.JK_OA_CreditorAddressInfo.ReadOnly);
		}

		#endregion

		#region Test Creditor Defaulting

		public void TestDefaultCreditorBasedOnOtherOrganisations_ForImport()
		{
			OrgHeader receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			receivingForwarder.OH_IsCreditor = true;
			OrgHeader anotherForwarder = Factory.NewWithValidTestData<OrgHeader>();
			anotherForwarder.OH_IsCreditor = true;
			anotherForwarder.OH_IsForwarder = true;

			ShippingCompany1.OH_Code = "ORG1";
			ShippingCompany2.OH_Code = "ORG2";

			Factory.Save();

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RL_NKLoadPort = "NZAKA";
			AssertEquals("Preconditions: Import Consol expected.", true, consol.IsImport());
			AssertEquals("Preconditions: Creditor is empty.", ZGuid.Empty, consol.JK_OA_CreditorAddress);
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = ForwardingCompany1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			AssertEquals("Creditor should not have been defaulted.", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = ForwardingCompany1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			AssertEquals("Creditor should not have been defaulted.", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ShippingCompany2.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = ForwardingCompany1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_SendingForwarderAddress = ForwardingCompany1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			OrgCarrierAppointedAgentPorts agentPort = ShippingCompany1.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort.O5_PortOrCountry = "AUSYD";
			agentPort.O5_OA_AgentOfficeAddress = ShippingCompany3.MainAddress.PK;
			agentPort.AgentOfficeAddress.Header.OH_IsCreditor = true;
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			AssertEquals("Creditor should have defaulted to ShippingLine based on last discharge port.", ShippingCompany1.MainAddress.PK, consol.JK_OA_CreditorAddress);

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			agentPort.O5_PortOrCountry = "AU";
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			AssertEquals("Creditor should have still defaulted to ShippingLine's based on last discharge port.", ShippingCompany1.PK, consol.CreditorPK);

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			agentPort.O5_PortOrCountry = "US";
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			AssertEquals("Creditor should have defaulted to ShippingLine.", ShippingCompany1.MainAddress.PK, consol.JK_OA_CreditorAddress);
			consol.JK_OA_ShippingLineAddress = ShippingCompany2.MainAddress.PK;
			AssertEquals("Creditor should be updated to a new ShippingLine if the previous one is changed.", ShippingCompany2.MainAddress.PK, consol.JK_OA_CreditorAddress);

			ShippingCompany1.OH_IsCreditor = false;
			agentPort = ShippingCompany1.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort.O5_PortOrCountry = "AUSYD";
			agentPort.O5_OA_AgentOfficeAddress = ShippingCompany2.MainAddress.PK;
			agentPort.AgentOfficeAddress.Header.OH_IsCreditor = true;

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			AssertEquals("Creditor should not be defaulted.", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			ShippingCompany1.OH_IsCreditor = false;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			agentPort.O5_PortOrCountry = "AU";
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			AssertEquals("Creditor should not be defaulted.", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			agentPort.AgentOfficeAddress.Header.OH_IsCreditor = false;
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			AssertEquals("Creditor should not be defaulted.", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			ShippingCompany1.OH_IsCreditor = true;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			AssertEquals("Creditor should have defaulted to ShippingLine.", ShippingCompany1.MainAddress.PK, consol.JK_OA_CreditorAddress);
		}

		public void TestDefaultCreditorBasedOnOtherOrganisations_WhenPrepaidCollectChanged()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			ShippingCompany1.OH_Code = "ORG1";

			Factory.Save();

			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RL_NKLoadPort = "NZAKA";
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			AssertEquals("Creditor should not yet be set.", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
			AssertEquals("Creditor should have defaulted to ShippingLine if the previous one is changed.", ShippingCompany1.MainAddress.PK, consol.JK_OA_CreditorAddress);
		}

		public void TestDefaultCreditorBasedOnOtherOrganisations_ForImportDomestic()
		{
			OrgHeader receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			receivingForwarder.OH_IsCreditor = true;
			OrgHeader anotherForwarder = Factory.NewWithValidTestData<OrgHeader>();
			anotherForwarder.OH_IsCreditor = true;
			anotherForwarder.OH_IsForwarder = true;

			ShippingCompany1.OH_Code = "ORG1";
			ShippingCompany2.OH_Code = "ORG2";

			Factory.Save();

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			consol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.HomePort.RL_RN_NKCountryCode + "XXX";
			AssertEquals("Preconditions: Import Consol not expected.", false, consol.IsImport());
			AssertEquals("Preconditions: Domestic Consol expected.", true, consol.IsDomestic());
			AssertEquals("Preconditions: Creditor is empty.", ZGuid.Empty, consol.JK_OA_CreditorAddress);
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = ForwardingCompany1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.PK;
			AssertEquals("Creditor should not have been defaulted.", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = ForwardingCompany1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			AssertEquals("Creditor should not have been defaulted.", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ShippingCompany2.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = ForwardingCompany1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			OrgCarrierAppointedAgentPorts agentPort = ShippingCompany1.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort.O5_PortOrCountry = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			agentPort.O5_OA_AgentOfficeAddress = ShippingCompany3.MainAddress.PK;
			agentPort.AgentOfficeAddress.Header.OH_IsCreditor = true;
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			AssertEquals("Creditor should have defaulted to ShippingLine based on last discharge port.", ShippingCompany1.MainAddress.PK, consol.JK_OA_CreditorAddress);

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			agentPort.O5_PortOrCountry = GlbBranch.CurrentBranch.HomePort.RL_RN_NKCountryCode;
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			AssertEquals("Creditor should have defaulted to ShippingLine last discharge port.", ShippingCompany1.MainAddress.PK, consol.JK_OA_CreditorAddress);

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			agentPort.O5_PortOrCountry = "US";
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			AssertEquals("Creditor should have defaulted to ShippingLine.", ShippingCompany1.MainAddress.PK, consol.JK_OA_CreditorAddress);
			consol.JK_OA_ShippingLineAddress = ShippingCompany2.MainAddress.PK;
			AssertEquals("Creditor should be updated to a new ShippingLine if the previous one is changed.", ShippingCompany2.MainAddress.PK, consol.JK_OA_CreditorAddress);

			ShippingCompany1.OH_IsCreditor = false;
			agentPort = ShippingCompany1.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort.O5_PortOrCountry = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			agentPort.O5_OA_AgentOfficeAddress = ShippingCompany2.MainAddress.PK;
			agentPort.AgentOfficeAddress.Header.OH_IsCreditor = true;

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			AssertEquals("Creditor should not be defaulted", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			ShippingCompany1.OH_IsCreditor = false;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			agentPort.O5_PortOrCountry = "AU";
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			AssertEquals("Creditor should not be defaulted", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			agentPort.AgentOfficeAddress.Header.OH_IsCreditor = false;
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			AssertEquals("Creditor should not be defaulted", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			ShippingCompany1.OH_IsCreditor = true;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			AssertEquals("Creditor should have defaulted to ShippingLine.", ShippingCompany1.MainAddress.PK, consol.JK_OA_CreditorAddress);
		}

		public void TestDefaultCreditorBasedOnOtherOrganisations_ForExport()
		{
			OrgHeader receivingForwarder1 = Factory.NewWithValidTestData<OrgHeader>();
			receivingForwarder1.OH_IsCreditor = true;
			receivingForwarder1.OH_IsForwarder = true;
			ShippingCompany1.OH_Code = "ORG1";
			ShippingCompany2.OH_Code = "ORG2";

			Factory.Save();

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKDischargePort = "NZAKA";
			consol.JK_RL_NKLoadPort = "AUSYD";
			AssertEquals("Preconditions: Export Consol expected.", true, consol.IsExport());
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = ForwardingCompany1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder1.MainAddress.PK;
			AssertEquals("Creditor should not have been defaulted.", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = ForwardingCompany1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder1.MainAddress.PK;
			AssertEquals("Creditor should not have been defaulted.", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			OrgCarrierAppointedAgentPorts agentPort = ShippingCompany2.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort.O5_PortOrCountry = "AUSYD";
			agentPort.O5_OA_AgentOfficeAddress = ShippingCompany3.MainAddress.PK;
			agentPort.AgentOfficeAddress.Header.OH_IsCreditor = true;
			consol.JK_OA_ShippingLineAddress = ShippingCompany2.MainAddress.PK;
			AssertEquals("Creditor should have defaulted to ShippingLine based on first load port.", ShippingCompany2.MainAddress.PK, consol.JK_OA_CreditorAddress);

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			agentPort.O5_PortOrCountry = "AU";
			consol.JK_OA_ShippingLineAddress = ShippingCompany2.MainAddress.PK;
			AssertEquals("Creditor should have defaulted to ShippingLine based on first load port.", ShippingCompany2.MainAddress.PK, consol.JK_OA_CreditorAddress);

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			agentPort.O5_PortOrCountry = "US";
			consol.JK_OA_ShippingLineAddress = ShippingCompany2.MainAddress.PK;
			AssertEquals("Creditor should have defaulted to ShippingLine.", ShippingCompany2.MainAddress.PK, consol.JK_OA_CreditorAddress);

			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			AssertEquals("Creditor should have defaulted to shipping line if the previous one is changed.", ShippingCompany1.MainAddress.PK, consol.JK_OA_CreditorAddress);

			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			ShippingCompany1.OH_IsCreditor = false;
			agentPort = ShippingCompany1.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort.O5_PortOrCountry = "AUSYD";
			agentPort.O5_OA_AgentOfficeAddress = ShippingCompany2.MainAddress.PK;
			agentPort.AgentOfficeAddress.Header.OH_IsCreditor = true;

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			AssertEquals("Creditor should not have been defaulted.", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			ShippingCompany1.OH_IsCreditor = false;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			agentPort.O5_PortOrCountry = "AU";
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			AssertEquals("Creditor should not have been defaulted.", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			agentPort.AgentOfficeAddress.Header.OH_IsCreditor = false;
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			AssertEquals("Creditor should not be defaulted.", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			ShippingCompany1.OH_IsCreditor = true;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			AssertEquals("Creditor should have defaulted to ShippingLine.", ShippingCompany1.MainAddress.PK, consol.JK_OA_CreditorAddress);
		}

		public void TestDefaultCreditorBasedOnOtherOrganisations_ForExportDomestic()
		{
			OrgHeader receivingForwarder1 = Factory.NewWithValidTestData<OrgHeader>();
			receivingForwarder1.OH_IsCreditor = true;
			receivingForwarder1.OH_IsForwarder = true;

			ShippingCompany1.OH_Code = "ORG1";
			ShippingCompany2.OH_Code = "ORG2";

			Factory.Save();

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_RL_NKDischargePort = GlbBranch.CurrentBranch.HomePort.RL_RN_NKCountryCode + "XXX";
			consol.JK_RL_NKLoadPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			AssertEquals("Preconditions: Export Consol not expected.", false, consol.IsExport());
			AssertEquals("Preconditions: Domestic Consol expected.", true, consol.IsDomestic());
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = ForwardingCompany1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder1.MainAddress.PK;
			AssertEquals("Creditor should not have been defaulted.", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = ForwardingCompany1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder1.MainAddress.PK;
			AssertEquals("Creditor should not have been defaulted.", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			OrgCarrierAppointedAgentPorts agentPort = ShippingCompany2.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort.O5_PortOrCountry = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			agentPort.O5_OA_AgentOfficeAddress = ShippingCompany3.MainAddress.PK;
			agentPort.AgentOfficeAddress.Header.OH_IsCreditor = true;
			consol.JK_OA_ShippingLineAddress = ShippingCompany2.MainAddress.PK;
			AssertEquals("Creditor should have defaulted to ShippingLine based on first load port.", ShippingCompany2.MainAddress.PK, consol.JK_OA_CreditorAddress);

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			agentPort.O5_PortOrCountry = GlbBranch.CurrentBranch.HomePort.RL_RN_NKCountryCode;
			consol.JK_OA_ShippingLineAddress = ShippingCompany2.MainAddress.PK;
			AssertEquals("Creditor should have defaulted to ShippingLine based on first load port.", ShippingCompany2.MainAddress.PK, consol.JK_OA_CreditorAddress);

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			agentPort.O5_PortOrCountry = "US";
			consol.JK_OA_ShippingLineAddress = ShippingCompany2.MainAddress.PK;
			AssertEquals("Creditor should have defaulted to shipping line.", ShippingCompany2.MainAddress.PK, consol.JK_OA_CreditorAddress);
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			AssertEquals("Creditor should be updated to a new shipping line if the previous one is changed.", ShippingCompany1.MainAddress.PK, consol.JK_OA_CreditorAddress);

			consol.JK_PrepaidCollect = Constants.PaymentType.Collect;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;

			ShippingCompany1.OH_IsCreditor = false;
			agentPort = ShippingCompany1.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort.O5_PortOrCountry = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			agentPort.O5_OA_AgentOfficeAddress = ShippingCompany2.MainAddress.PK;
			agentPort.AgentOfficeAddress.Header.OH_IsCreditor = true;

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_PrepaidCollect = Constants.PaymentType.Prepaid;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			AssertEquals("Creditor should not have been defaulted.", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			ShippingCompany1.OH_IsCreditor = false;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			agentPort.O5_PortOrCountry = "AU";
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			AssertEquals("Creditor should not have been defaulted.", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			agentPort.AgentOfficeAddress.Header.OH_IsCreditor = false;
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			AssertEquals("Creditor should not be defaulted.", ZGuid.Empty, consol.JK_OA_CreditorAddress);

			ShippingCompany1.OH_IsCreditor = true;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;
			AssertEquals("Creditor should have defaulted to Shipping Line.", ShippingCompany1.MainAddress.PK, consol.JK_OA_CreditorAddress);
		}

		public void TestCreditorList()
		{
			OrgHeader aCreditor = CreateOrgHeader();
			aCreditor.OH_IsCreditor = true;
			aCreditor.OH_FullName = "Sample Creditor";
			aCreditor.MainAddress.OA_Address1 = "Creditors Address";
			aCreditor.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			aCreditor.CompanyData.OB_IsCreditor = true;
			aCreditor.CompanyData.OB_GC = GlbCompany.CurrentCompany.PK;

			OrgHeader aCoLoaderForwarder = CreateOrgHeader();
			aCoLoaderForwarder.OH_IsForwarder = true;

			OrgHeader aCoLoaderNVOCCCarrier = CreateOrgHeader();
			aCoLoaderNVOCCCarrier.OH_IsShippingProvider = true;
			aCoLoaderNVOCCCarrier.OH_IsSeaWholesaler = true;

			OrgHeader aCoLoaderCarrier = CreateOrgHeader();
			aCoLoaderCarrier.OH_IsShippingProvider = true;
			aCoLoaderCarrier.OH_IsSeaWholesaler = false;

			OrgAppointedAgentPorts appAgPorts = aCoLoaderForwarder.AppointedAgentPorts.AddNew();
			appAgPorts.O5_PortOrCountry = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			appAgPorts.O5_AirAgentStatus = AgentStatusList.Codes.Handles;
			appAgPorts.O5_SeaAgentStatus = AgentStatusList.Codes.Handles;
			appAgPorts.O5_OA_AgentOfficeAddress = aCoLoaderForwarder.MainAddress.PK;

			Factory.Save();

			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.CreditorList.Load();

			AssertEquals("AgentType == CoLoad - Forwarder should be included in list", true, consol.CreditorList.Contains(aCoLoaderForwarder.PK));
			AssertEquals("AgentType == CoLoad - NVOCC Carrier should be included in list", true, consol.CreditorList.Contains(aCoLoaderNVOCCCarrier.PK));
			AssertEquals("AgentType == CoLoad - Non NVOCC Carrier should be included in list", false, consol.CreditorList.Contains(aCoLoaderCarrier.PK));
			AssertEquals("AgentType == CoLoad - Creditor should not be included in list", false, consol.CreditorList.Contains(aCreditor.PK));

			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.CreditorList.Load();

			AssertEquals("AgentType == Agent - Creditor should be included in list", true, consol.CreditorList.Contains(aCreditor.PK));
			AssertEquals("AgentType == Agent - Forwarder should not be included in list", false, consol.CreditorList.Contains(aCoLoaderForwarder.PK));
			AssertEquals("AgentType == Agent - NVOCC Carrier should not be included in list", false, consol.CreditorList.Contains(aCoLoaderNVOCCCarrier.PK));
			AssertEquals("AgentType == Agent - Non NVOCC Carrier should not be included in list", false, consol.CreditorList.Contains(aCoLoaderCarrier.PK));
		}

		#endregion

		#region Implementation

		RefCountry Country;
		RefUNLOCO FirstLoco;
		RefUNLOCO SecondLoco;
		OrgHeader FirstSeaForwarder;
		OrgHeader SecondSeaForwarder;
		OrgHeader FirstAirForwarder;
		OrgHeader SecondAirForwarder;
		CommonConsol Consol;
		ZGuid OldOrgProxyPK;

		protected override void SetUp()
		{
			OldOrgProxyPK = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			base.SetUp();
			SetUpForwarderListObjects();
			Consol = Factory.New<CommonConsol>();
		}

		protected override void TearDown()
		{
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = OldOrgProxyPK;
			base.TearDown();
		}

		void SetUpForwarderListObjects()
		{
			Country = Factory.New<RefCountry>();
			Country.RN_Code = "XX";
			Country.RN_Desc = "Test XX country";

			FirstLoco = Factory.New<RefUNLOCO>();
			FirstLoco.RL_Code = "XXZZZ";
			FirstLoco.RL_PortName = "Test ZZZ port";

			SecondLoco = Factory.New<RefUNLOCO>();
			SecondLoco.RL_Code = "XXYYY";
			SecondLoco.RL_PortName = "Test YYY port";

			FirstSeaForwarder = Factory.New<OrgHeader>();
			FirstSeaForwarder.OH_FullName = "First Sea forwarder";
			FirstSeaForwarder.MainAddress.OA_Address1 = "Test address 1";
			FirstSeaForwarder.OH_RL_NKClosestPort = FirstLoco.RL_Code;
			FirstSeaForwarder.OH_IsForwarder = true;
			OrgAppointedAgentPorts fSAppAgPorts = FirstSeaForwarder.AppointedAgentPorts.AddNew();
			fSAppAgPorts.O5_PortOrCountry = FirstLoco.RL_Code;
			fSAppAgPorts.O5_SeaAgentStatus = AgentStatusList.Codes.Handles;
			fSAppAgPorts.O5_AirAgentStatus = AgentStatusList.Codes.Handles;
			fSAppAgPorts.O5_OA_AgentOfficeAddress = FirstSeaForwarder.MainAddress.PK;

			SecondSeaForwarder = Factory.New<OrgHeader>();
			SecondSeaForwarder.OH_FullName = "Second Sea forwarder";
			SecondSeaForwarder.MainAddress.OA_Address1 = "Test address 1";
			SecondSeaForwarder.OH_RL_NKClosestPort = SecondLoco.RL_Code;
			SecondSeaForwarder.OH_IsForwarder = true;
			OrgAppointedAgentPorts sSAppAgPorts = SecondSeaForwarder.AppointedAgentPorts.AddNew();
			sSAppAgPorts.O5_PortOrCountry = SecondLoco.RL_Code;
			sSAppAgPorts.O5_AirAgentStatus = AgentStatusList.Codes.Handles;
			sSAppAgPorts.O5_SeaAgentStatus = AgentStatusList.Codes.Handles;
			sSAppAgPorts.O5_OA_AgentOfficeAddress = SecondSeaForwarder.MainAddress.PK;

			FirstAirForwarder = Factory.New<OrgHeader>();
			FirstAirForwarder.OH_FullName = "First Air forwarder";
			FirstAirForwarder.MainAddress.OA_Address1 = "Test address 1";
			FirstAirForwarder.OH_RL_NKClosestPort = FirstLoco.RL_Code;
			FirstAirForwarder.OH_IsForwarder = true;
			OrgAppointedAgentPorts fAAppAgPorts = FirstAirForwarder.AppointedAgentPorts.AddNew();
			fAAppAgPorts.O5_PortOrCountry = FirstLoco.RL_Code;
			fAAppAgPorts.O5_AirAgentStatus = AgentStatusList.Codes.Handles;
			fAAppAgPorts.O5_SeaAgentStatus = AgentStatusList.Codes.Handles;
			fAAppAgPorts.O5_OA_AgentOfficeAddress = FirstAirForwarder.MainAddress.PK;

			SecondAirForwarder = Factory.New<OrgHeader>();
			SecondAirForwarder.OH_FullName = "Second Air forwarder";
			SecondAirForwarder.MainAddress.OA_Address1 = "Test address 1";
			SecondAirForwarder.OH_RL_NKClosestPort = SecondLoco.RL_Code;
			SecondAirForwarder.OH_IsForwarder = true;
			OrgAppointedAgentPorts sAAppAgPorts = SecondAirForwarder.AppointedAgentPorts.AddNew();
			sAAppAgPorts.O5_PortOrCountry = SecondLoco.RL_Code;
			sAAppAgPorts.O5_AirAgentStatus = AgentStatusList.Codes.Handles;
			sAAppAgPorts.O5_SeaAgentStatus = AgentStatusList.Codes.Handles;
			sAAppAgPorts.O5_OA_AgentOfficeAddress = SecondAirForwarder.MainAddress.PK;

			Factory.Save();
		}

		void PrepareConsolForwarderList(ZString agentType, ZString loadingPort, ZString dischargePort, ZString transportMode)
		{
			Consol.JK_AgentType = agentType;
			Consol.JK_RL_NKLoadPort = loadingPort;
			Consol.JK_RL_NKDischargePort = dischargePort;

			Consol.JK_TransportMode = transportMode;
		}

		void AssertForwarderList(OrgHeaderCollection list, string message, params OrgHeader[] listMembers)
		{
			list.Load();
			if (listMembers.Length == 0)
			{
				AssertEquals(message, 0, list.Count);
			}
			else
			{
				foreach (OrgHeader org in listMembers)
				{
					AssertEquals(message, true, list.Contains(org.PK));
				}
			}
		}

		#endregion

	}
}
