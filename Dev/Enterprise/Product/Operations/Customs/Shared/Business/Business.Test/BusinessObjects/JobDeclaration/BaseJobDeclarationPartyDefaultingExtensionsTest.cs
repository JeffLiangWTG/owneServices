using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseJobDeclarationPartyDefaultingExtensionsTest : TestCaseWithFactory
	{
		public void TestDefaultMasterBillFromAirLine()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

			var airline = Factory.New<RefAirline>();
			airline.RM_TwoCharacterCode = "XX";

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "aaa";
			airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "333";
			org1.OH_RL_NKClosestPort = "DEHAM";
			org1.MiscServ.OM_RM_Airline = airline.PK;

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			declaration.JE_MasterBill = "";
			declaration.JE_VoyageFlightNo = "XX1234";
			CombineAssertions(() =>
			{
				AssertEquals("defaulting JE_MasterBill", "333", declaration.JE_MasterBill);
				declaration.JE_VoyageFlightNo = "";
				declaration.JE_MasterBill = "123456789";
				declaration.JE_VoyageFlightNo = "XX1234";
				AssertEquals("no defaulting JE_MasterBill if already exists", "123456789", declaration.JE_MasterBill);
			});
		}

		public void TestDefaultCarrierFromAirLine()
		{
			AssertDefaultCarrierFromAirLine(RelatedPartyDefaultingTypeList.Codes.Both, JobMessageTypeList.Codes.Import, true);
			AssertDefaultCarrierFromAirLine(RelatedPartyDefaultingTypeList.Codes.Both, JobMessageTypeList.Codes.Export, true);
			AssertDefaultCarrierFromAirLine(RelatedPartyDefaultingTypeList.Codes.Import, JobMessageTypeList.Codes.Import, true);
			AssertDefaultCarrierFromAirLine(RelatedPartyDefaultingTypeList.Codes.Import, JobMessageTypeList.Codes.Export, false);
			AssertDefaultCarrierFromAirLine(RelatedPartyDefaultingTypeList.Codes.Export, JobMessageTypeList.Codes.Import, false);
			AssertDefaultCarrierFromAirLine(RelatedPartyDefaultingTypeList.Codes.Export, JobMessageTypeList.Codes.Export, true);
			AssertDefaultCarrierFromAirLine(RelatedPartyDefaultingTypeList.Codes.Disabled, JobMessageTypeList.Codes.Import, false);
			AssertDefaultCarrierFromAirLine(RelatedPartyDefaultingTypeList.Codes.Disabled, JobMessageTypeList.Codes.Export, false);
		}

		public void AssertDefaultCarrierFromAirLine(string registryValue, string declarationMessageType, bool expected)
		{
			using (CustomsDataRegistry.Instance.RelatedPartyDefaultingCarrier.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();

				var airline = Factory.New<RefAirline>();
				airline.RM_TwoCharacterCode = "AA";

				var org1 = Factory.New<OrgHeader>();
				org1.OH_Code = "aaa";
				airline.RM_EagleAddedAirlinePrefixOrAccountingCode = "000";
				org1.OH_RL_NKClosestPort = "DEHAM";
				org1.MiscServ.OM_RM_Airline = airline.PK;

				var airline2 = Factory.New<RefAirline>();
				airline2.RM_TwoCharacterCode = "BB";

				var org2 = Factory.New<OrgHeader>();
				org2.OH_Code = "bbb";
				airline2.RM_EagleAddedAirlinePrefixOrAccountingCode = "666";
				org2.OH_RL_NKClosestPort = "AUSYD";
				org2.MiscServ.OM_RM_Airline = airline2.PK;

				declaration.JE_MessageType = declarationMessageType;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				declaration.JE_VoyageFlightNo = "AA1234";
				declaration.JE_MasterBill = "00012345";
				AssertEquals("defaulting, by JE_MasterBill", expected, org1.PK == declaration.JE_OH_ShippingLine);
				declaration.JE_VoyageFlightNo = "";
				declaration.JE_OH_ShippingLine = Guid.Empty;
				declaration.JE_VoyageFlightNo = "AA1234";
				AssertEquals("defaulting, by JE_VoyageFlightNo", expected, org1.PK == declaration.JE_OH_ShippingLine);

				declaration.JE_MasterBill = "";
				declaration.JE_OH_ShippingLine = Guid.Empty;
				declaration.JE_VoyageFlightNo = "BB1234";
				AssertEquals("defaulting when only match 2 code flight number", expected, org2.PK == declaration.JE_OH_ShippingLine);

				airline.Delete();
				org1.Delete();
				airline2.Delete();
				org2.Delete();
			}
		}

		public void TestDefaultWarehouseDocAddress_Import()
		{
			AssertDefaultWarehouseDocAddress_Import(RelatedPartyDefaultingTypeList.Codes.Both, true);
			AssertDefaultWarehouseDocAddress_Import(RelatedPartyDefaultingTypeList.Codes.Import, true);
			AssertDefaultWarehouseDocAddress_Import(RelatedPartyDefaultingTypeList.Codes.Export, false);
			AssertDefaultWarehouseDocAddress_Import(RelatedPartyDefaultingTypeList.Codes.Disabled, false);
		}

		void AssertDefaultWarehouseDocAddress_Import(string registryValue, bool expected)
		{
			using (CustomsDataRegistry.Instance.RelatedPartyDefaultingBondedWarehouse.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var relatedParty = org.AllRelatedParties.AddNew();
				relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.Warehouse;
				relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				relatedParty.PR_GC = GlbCompany.CurrentCompany.PK;
				relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				declaration.JE_OH_Importer = org.PK;

				AssertEquals("DefaultWarehouseDocAddress import, by JE_OH_Importer", expected, GlbCompany.CurrentCompany.GC_OH_OrgProxy == declaration.WarehouseDocAddress.OrganisationPK);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.WarehouseDocAddress.OrganisationPK = Guid.Empty;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("DefaultWarehouseDocAddress import, by JE_MessageType", expected, GlbCompany.CurrentCompany.GC_OH_OrgProxy == declaration.WarehouseDocAddress.OrganisationPK);
			}
		}

		public void TestDefaultWarehouseDocAddress_Export()
		{
			AssertDefaultWarehouseDocAddress_Export(RelatedPartyDefaultingTypeList.Codes.Both, true);
			AssertDefaultWarehouseDocAddress_Export(RelatedPartyDefaultingTypeList.Codes.Import, false);
			AssertDefaultWarehouseDocAddress_Export(RelatedPartyDefaultingTypeList.Codes.Export, true);
			AssertDefaultWarehouseDocAddress_Export(RelatedPartyDefaultingTypeList.Codes.Disabled, false);
		}

		void AssertDefaultWarehouseDocAddress_Export(string registryValue, bool expected)
		{
			using (CustomsDataRegistry.Instance.RelatedPartyDefaultingBondedWarehouse.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var relatedParty = org.AllRelatedParties.AddNew();
				relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.Warehouse;
				relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				relatedParty.PR_GC = GlbCompany.CurrentCompany.PK;
				relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				declaration.JE_OH_Supplier = org.PK;

				AssertEquals("DefaultWarehouseDocAddress export, trigger by JE_OH_Supplier", expected, GlbCompany.CurrentCompany.GC_OH_OrgProxy == declaration.WarehouseDocAddress.OrganisationPK);
			}
		}

		public void TestDefaultExternalBroker_ImportEnabled()
		{
			using (CustomsDataRegistry.Instance.RelatedPartyDefaultingExternalBroker.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RelatedPartyDefaultingTypeList.Codes.Import))
			{
				var importer = Factory.NewWithValidTestData<OrgHeader>();
				var orgRelatedParty = CreateOrgRelatedParty(Factory, importer, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, "USLAX");
				var declaration = CreateDeclarationForRelatedParty(Factory, JobMessageTypeList.Codes.Import, "AUSYD", "USLAX", importer.PK, ZGuid.Empty);
				declaration.JE_OH_ExternalBroker = ZGuid.Empty;
				declaration.DefaultExternalBroker(false, true);
				AssertEquals(orgRelatedParty.PK, declaration.JE_OH_ExternalBroker);
			}
		}

		public void TestDefaultExternalBroker_ImportBoth()
		{
			using (CustomsDataRegistry.Instance.RelatedPartyDefaultingExternalBroker.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RelatedPartyDefaultingTypeList.Codes.Both))
			{
				var importer = Factory.NewWithValidTestData<OrgHeader>();
				var orgRelatedParty = CreateOrgRelatedParty(Factory, importer, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, "USLAX");
				var declaration = CreateDeclarationForRelatedParty(Factory, JobMessageTypeList.Codes.Import, "AUSYD", "USLAX", importer.PK, ZGuid.Empty);
				declaration.JE_OH_ExternalBroker = ZGuid.Empty;
				declaration.DefaultExternalBroker(false, true);
				AssertEquals(orgRelatedParty.PK, declaration.JE_OH_ExternalBroker);
			}
		}

		public void TestDefaultExternalBroker_ImportDisabled()
		{
			using (CustomsDataRegistry.Instance.RelatedPartyDefaultingExternalBroker.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RelatedPartyDefaultingTypeList.Codes.Disabled))
			{
				var importer = Factory.NewWithValidTestData<OrgHeader>();
				CreateOrgRelatedParty(Factory, importer, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, "USLAX");
				var declaration = CreateDeclarationForRelatedParty(Factory, JobMessageTypeList.Codes.Import, "AUSYD", "USLAX", importer.PK, ZGuid.Empty);
				declaration.JE_OH_ExternalBroker = ZGuid.Empty;
				declaration.DefaultExternalBroker(false, true);
				AssertEquals(ZGuid.Empty, declaration.JE_OH_ExternalBroker);
			}
		}

		public void TestDefaultExternalBroker_ExportEnabled()
		{
			using (CustomsDataRegistry.Instance.RelatedPartyDefaultingExternalBroker.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RelatedPartyDefaultingTypeList.Codes.Export))
			{
				var supplier = Factory.NewWithValidTestData<OrgHeader>();
				var orgRelatedParty = CreateOrgRelatedParty(Factory, supplier, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, "AUSYD");
				var declaration = CreateDeclarationForRelatedParty(Factory, JobMessageTypeList.Codes.Export, "AUSYD", "USLAX", ZGuid.Empty, supplier.PK);
				declaration.JE_OH_ExternalBroker = ZGuid.Empty;
				declaration.DefaultExternalBroker(true, false);
				AssertEquals(orgRelatedParty.PK, declaration.JE_OH_ExternalBroker);
			}
		}

		public void TestDefaultExternalBroker_ExportBoth()
		{
			using (CustomsDataRegistry.Instance.RelatedPartyDefaultingExternalBroker.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RelatedPartyDefaultingTypeList.Codes.Both))
			{
				var supplier = Factory.NewWithValidTestData<OrgHeader>();
				var orgRelatedParty = CreateOrgRelatedParty(Factory, supplier, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, "AUSYD");
				var declaration = CreateDeclarationForRelatedParty(Factory, JobMessageTypeList.Codes.Export, "AUSYD", "USLAX", ZGuid.Empty, supplier.PK);
				declaration.JE_OH_ExternalBroker = ZGuid.Empty;
				declaration.DefaultExternalBroker(true, false);
				AssertEquals(orgRelatedParty.PK, declaration.JE_OH_ExternalBroker);
			}
		}

		public void TestDefaultExternalBroker_ExportDisabled()
		{
			using (CustomsDataRegistry.Instance.RelatedPartyDefaultingExternalBroker.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RelatedPartyDefaultingTypeList.Codes.Disabled))
			{
				var supplier = Factory.NewWithValidTestData<OrgHeader>();
				CreateOrgRelatedParty(Factory, supplier, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Pickup, "AUSYD");
				var declaration = CreateDeclarationForRelatedParty(Factory, JobMessageTypeList.Codes.Export, "AUSYD", "USLAX", ZGuid.Empty, supplier.PK);
				declaration.JE_OH_ExternalBroker = ZGuid.Empty;
				declaration.DefaultExternalBroker(true, false);
				AssertEquals(ZGuid.Empty, declaration.JE_OH_ExternalBroker);
			}
		}

		public void TestDefaultForwarder_Import()
		{
			AssertDefaultForwarder_Import(RelatedPartyDefaultingTypeList.Codes.Both, true);
			AssertDefaultForwarder_Import(RelatedPartyDefaultingTypeList.Codes.Import, true);
			AssertDefaultForwarder_Import(RelatedPartyDefaultingTypeList.Codes.Export, false);
			AssertDefaultForwarder_Import(RelatedPartyDefaultingTypeList.Codes.Disabled, false);
		}

		void AssertDefaultForwarder_Import(string registryValue, bool expected)
		{
			using (CustomsDataRegistry.Instance.RelatedPartyDefaultingForwarder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var orgRelated = Factory.New<OrgHeader>();
				orgRelated.OH_Code = "DXZENTBNE";
				orgRelated.OH_FullName = "DXZ Enterprises";
				orgRelated.OH_RL_NKClosestPort = "AUBNE";
				orgRelated.OH_IsForwarder = true;
				orgRelated.MainAddress.OA_Address1 = "1 Street St";

				var agentPortAMD = orgRelated.AppointedAgentPorts.AddNew();
				agentPortAMD.O5_PortOrCountry = "USLAX";
				agentPortAMD.O5_SeaAgentStatus = AgentStatusList.Codes.Appointed;
				agentPortAMD.O5_OA_AgentOfficeAddress = orgRelated.MainAddress.PK;
				agentPortAMD.O5_AgentDirection = AgentDirectionList.Codes.Import;

				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var relatedParty = org.AllRelatedParties.AddNew();
				relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ReceivingAgent;
				relatedParty.PR_OH_RelatedParty = orgRelated.PK;
				relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
				relatedParty.PR_FreightTransportMode = Core.Constants.TransportModes.Sea;
				relatedParty.PR_Location = "USLAX";
				relatedParty.PR_FreightContainerMode = Core.Constants.ContainerModes.Bulk;

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
				declaration.JE_RL_NKFinalDestination = "USLAX";
				declaration.JE_OH_Importer = org.PK;

				AssertEquals("Default Forwarder import, by JE_OH_Importer", expected, orgRelated.PK == declaration.JE_OH_Forwarder);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_OH_Forwarder = ZGuid.Empty;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("DefaultForwarder import, by JE_MessageType", expected, orgRelated.PK == declaration.JE_OH_Forwarder);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				declaration.JE_OH_Forwarder = ZGuid.Empty;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("DefaultForwarder import, by JE_TransportMode", expected, orgRelated.PK == declaration.JE_OH_Forwarder);

				declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
				declaration.JE_OH_Forwarder = ZGuid.Empty;
				declaration.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
				AssertEquals("DefaultForwarder import, by JE_ContainerMode", expected, orgRelated.PK == declaration.JE_OH_Forwarder);

				declaration.JE_RL_NKFinalDestination = "AUSYD";
				declaration.JE_OH_Forwarder = ZGuid.Empty;
				declaration.JE_RL_NKFinalDestination = "USLAX";
				AssertEquals("DefaultForwarder import, by JE_RL_NKFinalDestination", expected, orgRelated.PK == declaration.JE_OH_Forwarder);

				relatedParty.PR_Location = "";
				declaration.JE_RL_NKFinalDestination = "AUSYD";
				declaration.JE_OH_Forwarder = ZGuid.Empty;
				declaration.JE_RL_NKFinalDestination = "USLAX";
				AssertEquals("DefaultForwarder, when related party location is empty", expected, orgRelated.PK == declaration.JE_OH_Forwarder);
			}
		}

		public void TestDefaultForwarder_Export()
		{
			AssertDefaultForwarder_Export(RelatedPartyDefaultingTypeList.Codes.Both, true);
			AssertDefaultForwarder_Export(RelatedPartyDefaultingTypeList.Codes.Import, false);
			AssertDefaultForwarder_Export(RelatedPartyDefaultingTypeList.Codes.Export, true);
			AssertDefaultForwarder_Export(RelatedPartyDefaultingTypeList.Codes.Disabled, false);
		}

		void AssertDefaultForwarder_Export(string registryValue, bool expected)
		{
			using (CustomsDataRegistry.Instance.RelatedPartyDefaultingForwarder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var orgRelated = Factory.New<OrgHeader>();
				orgRelated.OH_Code = "DXZENTBNE";
				orgRelated.OH_FullName = "DXZ Enterprises";
				orgRelated.OH_RL_NKClosestPort = "AUBNE";
				orgRelated.OH_IsForwarder = true;
				orgRelated.MainAddress.OA_Address1 = "1 Street St";

				var agentPortAMD = orgRelated.AppointedAgentPorts.AddNew();
				agentPortAMD.O5_PortOrCountry = "USLAX";
				agentPortAMD.O5_AirAgentStatus = AgentStatusList.Codes.Published;
				agentPortAMD.O5_OA_AgentOfficeAddress = orgRelated.MainAddress.PK;
				agentPortAMD.O5_AgentDirection = AgentDirectionList.Codes.Export;
				Factory.Save();

				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var relatedParty = org.AllRelatedParties.AddNew();
				relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.SendingAgent;
				relatedParty.PR_OH_RelatedParty = orgRelated.PK;
				relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
				relatedParty.PR_FreightTransportMode = Core.Constants.TransportModes.Air;
				relatedParty.PR_Location = "USLAX";

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				declaration.JE_RL_NKOrigin = "USLAX";
				declaration.JE_OH_Supplier = org.PK;

				AssertEquals("Default Forwarder export", expected, orgRelated.PK == declaration.JE_OH_Forwarder);
			}
		}

		public void TestDefaultForwarder_Fallback()
		{
			AssertDefaultForwarder_Fallback("USLAX", AgentStatusList.Codes.Appointed);
			AssertDefaultForwarder_Fallback("US", AgentStatusList.Codes.Published);
			AssertDefaultForwarder_Fallback("US", AgentStatusList.Codes.Appointed);
		}

		void AssertDefaultForwarder_Fallback(string portOrCountry, string airAgentStatus)
		{
			using (CustomsDataRegistry.Instance.RelatedPartyDefaultingForwarder.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RelatedPartyDefaultingTypeList.Codes.Export))
			{
				var orgRelated = Factory.New<OrgHeader>();
				orgRelated.OH_Code = "DXZENTBNE";
				orgRelated.OH_FullName = "DXZ Enterprises";
				orgRelated.OH_RL_NKClosestPort = "AUBNE";
				orgRelated.OH_IsForwarder = true;
				orgRelated.MainAddress.OA_Address1 = "1 Street St";

				var agentPortAMD = orgRelated.AppointedAgentPorts.AddNew();
				agentPortAMD.O5_PortOrCountry = "AUSYD";
				agentPortAMD.O5_AirAgentStatus = AgentStatusList.Codes.Published;
				agentPortAMD.O5_OA_AgentOfficeAddress = orgRelated.MainAddress.PK;
				agentPortAMD.O5_AgentDirection = AgentDirectionList.Codes.Export;

				var orgRelated2 = Factory.New<OrgHeader>();
				orgRelated2.OH_Code = "DXZENTBNX";
				orgRelated2.OH_FullName = "DXZ Enterprises X";
				orgRelated2.OH_RL_NKClosestPort = "AUBNE";
				orgRelated2.OH_IsForwarder = true;
				orgRelated2.MainAddress.OA_Address1 = "2 Street St";

				var agentPortAMD2 = orgRelated2.AppointedAgentPorts.AddNew();
				agentPortAMD2.O5_PortOrCountry = "USLAX";
				agentPortAMD2.O5_AirAgentStatus = airAgentStatus;
				agentPortAMD2.O5_OA_AgentOfficeAddress = orgRelated2.MainAddress.PK;
				agentPortAMD2.O5_AgentDirection = AgentDirectionList.Codes.Export;

				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var relatedParty = org.AllRelatedParties.AddNew();
				relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.SendingAgent;
				relatedParty.PR_OH_RelatedParty = orgRelated.PK;
				relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
				relatedParty.PR_FreightTransportMode = Core.Constants.TransportModes.Air;
				relatedParty.PR_Location = "USLAX";

				var relatedParty2 = org.AllRelatedParties.AddNew();
				relatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.SendingAgent;
				relatedParty2.PR_OH_RelatedParty = orgRelated2.PK;
				relatedParty2.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;
				relatedParty2.PR_FreightTransportMode = Core.Constants.TransportModes.Air;
				relatedParty2.PR_Location = "USLAX";

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				declaration.JE_RL_NKOrigin = "USLAX";
				declaration.JE_OH_Supplier = org.PK;

				AssertEquals("Default Forwarder export", orgRelated2.PK, declaration.JE_OH_Forwarder);
			}
		}

		public void TestDefaultCTO_Import()
		{
			AssertDefaultCTO_Import(RelatedPartyDefaultingTypeList.Codes.Both, true);
			AssertDefaultCTO_Import(RelatedPartyDefaultingTypeList.Codes.Import, true);
			AssertDefaultCTO_Import(RelatedPartyDefaultingTypeList.Codes.Export, false);
			AssertDefaultCTO_Import(RelatedPartyDefaultingTypeList.Codes.Disabled, false);
		}

		void AssertDefaultCTO_Import(string registryValue, bool expected)
		{
			using (CustomsDataRegistry.Instance.RelatedPartyDefaultingCTO.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var carrier = Factory.NewWithValidTestData<OrgHeader>();
				var port = carrier.CarrierAppointedAgentPorts_AirCTO.AddNew();
				var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
				port.O5_PortOrCountry = "AUSYD";
				port.O5_OA_AgentOfficeAddress = orgAddress.PK;
				declaration.JE_OH_ShippingLine = carrier.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = Constants.TransportModes.Air;
				declaration.JE_RL_NKFinalDestination = "AUSYD";

				var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
				var port2 = carrier2.CarrierAppointedAgentPorts_Stevedore.AddNew();
				var orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
				port2.O5_PortOrCountry = "AUSYD";
				port2.O5_OA_AgentOfficeAddress = orgAddress2.PK;
				port2.O5_TerminalType = StevedoreTerminalType.Codes.BulkTerminal;

				CombineAssertions(() =>
				{
					AssertEquals("from CarrierAppointedAgentPorts_AirCTO", expected, orgAddress.PK.ToString() == declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address.ToString());

					declaration.JE_TransportMode = Constants.TransportModes.Sea;
					declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = ZGuid.Empty;
					declaration.JE_OH_ShippingLine = carrier2.PK;
					AssertEquals("from CarrierAppointedAgentPorts_Stevedore, by JE_OH_ShippingLine", expected, orgAddress2.PK.ToString() == declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address.ToString());

					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = ZGuid.Empty;
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					AssertEquals("from CarrierAppointedAgentPorts_Stevedore, by JE_MessageType", expected, orgAddress2.PK.ToString() == declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address.ToString());

					declaration.JE_RL_NKFinalDestination = "USLAX";
					declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = ZGuid.Empty;
					declaration.JE_RL_NKFinalDestination = "AUSYD";
					AssertEquals("from CarrierAppointedAgentPorts_Stevedore, by JE_RL_NKFinalDestination", expected, orgAddress2.PK.ToString() == declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address.ToString());
				});
			}
		}

		public void TestDefaultCTO_Export()
		{
			AssertDefaultCTO_Export(RelatedPartyDefaultingTypeList.Codes.Both, true);
			AssertDefaultCTO_Export(RelatedPartyDefaultingTypeList.Codes.Import, false);
			AssertDefaultCTO_Export(RelatedPartyDefaultingTypeList.Codes.Export, true);
			AssertDefaultCTO_Export(RelatedPartyDefaultingTypeList.Codes.Disabled, false);
		}

		void AssertDefaultCTO_Export(string registryValue, bool expected)
		{
			using (CustomsDataRegistry.Instance.RelatedPartyDefaultingCTO.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var carrier = Factory.NewWithValidTestData<OrgHeader>();
				var port = carrier.CarrierAppointedAgentPorts_AirCTO.AddNew();
				var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
				port.O5_PortOrCountry = "AUSYD";
				port.O5_OA_AgentOfficeAddress = orgAddress.PK;
				declaration.JE_OH_ShippingLine = carrier.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = Constants.TransportModes.Air;
				declaration.JE_RL_NKOrigin = "AUSYD";

				var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
				var port2 = carrier2.CarrierAppointedAgentPorts_Stevedore.AddNew();
				var orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
				port2.O5_PortOrCountry = "AUSYD";
				port2.O5_OA_AgentOfficeAddress = orgAddress2.PK;
				port2.O5_TerminalType = StevedoreTerminalType.Codes.BulkTerminal;

				CombineAssertions(() =>
				{
					AssertEquals("from CarrierAppointedAgentPorts_AirCTO", expected, orgAddress.PK.ToString() == declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address.ToString());

					declaration.JE_TransportMode = Constants.TransportModes.Sea;
					declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = ZGuid.Empty;
					declaration.JE_OH_ShippingLine = carrier2.PK;
					AssertEquals("from CarrierAppointedAgentPorts_Stevedore", expected, orgAddress2.PK.ToString() == declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address.ToString());

					declaration.JE_RL_NKOrigin = "USLAX";
					declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address = ZGuid.Empty;
					declaration.JE_RL_NKOrigin = "AUSYD";
					AssertEquals("from CarrierAppointedAgentPorts_Stevedore, by JE_RL_NKOrigin", expected, orgAddress2.PK.ToString() == declaration.ContainerTerminalOperatorDocAddress.E2_OA_Address.ToString());
				});
			}
		}

		public void TestDefaultDepot_ImportEnabled()
		{
			using (CustomsDataRegistry.Instance.RelatedPartyDefaultingDepot.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RelatedPartyDefaultingTypeList.Codes.Import))
			{
				var importer = Factory.NewWithValidTestData<OrgHeader>();
				var orgRelatedParty = CreateOrgRelatedParty(Factory, importer, RelatedPartyTypeList.Codes.ClientCFS, RelatedPartyDirectionList.Codes.Delivery, "USLAX");
				var declaration = CreateDeclarationForRelatedParty(Factory, JobMessageTypeList.Codes.Import, "AUSYD", "USLAX", importer.PK, ZGuid.Empty);
				declaration.DepotDocAddress.OrganisationPK = ZGuid.Empty;
				declaration.DefaultDepot(false, true);
				AssertEquals(orgRelatedParty.PK, declaration.DepotDocAddress.OrganisationPK);
			}
		}

		public void TestDefaultDepot_ImportEnabled_DepotAddressOverride()
		{
			using (CustomsDataRegistry.Instance.RelatedPartyDefaultingDepot.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RelatedPartyDefaultingTypeList.Codes.Import))
			{
				var importer = Factory.NewWithValidTestData<OrgHeader>();
				CreateOrgRelatedParty(Factory, importer, RelatedPartyTypeList.Codes.ClientCFS, RelatedPartyDirectionList.Codes.Delivery, "USLAX");
				var declaration = CreateDeclarationForRelatedParty(Factory, JobMessageTypeList.Codes.Import, "AUSYD", "USLAX", importer.PK, ZGuid.Empty);
				declaration.DepotDocAddress.OrganisationPK = ZGuid.Empty;
				declaration.DepotDocAddress.E2_AddressOverride = true;
				declaration.DefaultDepot(false, true);
				AssertEquals(ZGuid.Empty, declaration.DepotDocAddress.OrganisationPK);
			}
		}

		public void TestDefaultDepot_ImportBoth()
		{
			using (CustomsDataRegistry.Instance.RelatedPartyDefaultingDepot.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RelatedPartyDefaultingTypeList.Codes.Both))
			{
				var importer = Factory.NewWithValidTestData<OrgHeader>();
				var orgRelatedParty = CreateOrgRelatedParty(Factory, importer, RelatedPartyTypeList.Codes.ClientCFS, RelatedPartyDirectionList.Codes.Delivery, "USLAX");
				var declaration = CreateDeclarationForRelatedParty(Factory, JobMessageTypeList.Codes.Import, "AUSYD", "USLAX", importer.PK, ZGuid.Empty);
				declaration.DepotDocAddress.OrganisationPK = ZGuid.Empty;
				declaration.DefaultDepot(false, true);
				AssertEquals(orgRelatedParty.PK, declaration.DepotDocAddress.OrganisationPK);
			}
		}

		public void TestDefaultDepot_ImportDisabled()
		{
			using (CustomsDataRegistry.Instance.RelatedPartyDefaultingDepot.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RelatedPartyDefaultingTypeList.Codes.Disabled))
			{
				var importer = Factory.NewWithValidTestData<OrgHeader>();
				CreateOrgRelatedParty(Factory, importer, RelatedPartyTypeList.Codes.ClientCFS, RelatedPartyDirectionList.Codes.Delivery, "USLAX");
				var declaration = CreateDeclarationForRelatedParty(Factory, JobMessageTypeList.Codes.Import, "AUSYD", "USLAX", importer.PK, ZGuid.Empty);
				declaration.DepotDocAddress.OrganisationPK = ZGuid.Empty;
				declaration.DefaultDepot(false, true);
				AssertEquals(ZGuid.Empty, declaration.DepotDocAddress.OrganisationPK);
			}
		}

		public void TestDefaultDepot_ExportEnabled()
		{
			using (CustomsDataRegistry.Instance.RelatedPartyDefaultingDepot.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RelatedPartyDefaultingTypeList.Codes.Export))
			{
				var supplier = Factory.NewWithValidTestData<OrgHeader>();
				var orgRelatedParty = CreateOrgRelatedParty(Factory, supplier, RelatedPartyTypeList.Codes.ClientCFS, RelatedPartyDirectionList.Codes.Pickup, "AUSYD");
				var declaration = CreateDeclarationForRelatedParty(Factory, JobMessageTypeList.Codes.Export, "AUSYD", "USLAX", ZGuid.Empty, supplier.PK);
				declaration.DepotDocAddress.OrganisationPK = ZGuid.Empty;
				declaration.DefaultDepot(true, false);
				AssertEquals(orgRelatedParty.PK, declaration.DepotDocAddress.OrganisationPK);
			}
		}

		public void TestDefaultDepot_ExportBoth()
		{
			using (CustomsDataRegistry.Instance.RelatedPartyDefaultingDepot.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RelatedPartyDefaultingTypeList.Codes.Both))
			{
				var supplier = Factory.NewWithValidTestData<OrgHeader>();
				var orgRelatedParty = CreateOrgRelatedParty(Factory, supplier, RelatedPartyTypeList.Codes.ClientCFS, RelatedPartyDirectionList.Codes.Pickup, "AUSYD");
				var declaration = CreateDeclarationForRelatedParty(Factory, JobMessageTypeList.Codes.Export, "AUSYD", "USLAX", ZGuid.Empty, supplier.PK);
				declaration.DepotDocAddress.OrganisationPK = ZGuid.Empty;
				declaration.DefaultDepot(true, false);
				AssertEquals(orgRelatedParty.PK, declaration.DepotDocAddress.OrganisationPK);
			}
		}

		public void TestDefaultDepot_ExportDisabled()
		{
			using (CustomsDataRegistry.Instance.RelatedPartyDefaultingDepot.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RelatedPartyDefaultingTypeList.Codes.Disabled))
			{
				var supplier = Factory.NewWithValidTestData<OrgHeader>();
				CreateOrgRelatedParty(Factory, supplier, RelatedPartyTypeList.Codes.ClientCFS, RelatedPartyDirectionList.Codes.Pickup, "AUSYD");
				var declaration = CreateDeclarationForRelatedParty(Factory, JobMessageTypeList.Codes.Export, "AUSYD", "USLAX", ZGuid.Empty, supplier.PK);
				declaration.DepotDocAddress.OrganisationPK = ZGuid.Empty;
				declaration.DefaultDepot(true, false);
				AssertEquals(ZGuid.Empty, declaration.DepotDocAddress.OrganisationPK);
			}
		}

		public void TestDefaultContainerYard_Import()
		{
			AssertDefaultContainerYard_Import(RelatedPartyDefaultingTypeList.Codes.Both, true);
			AssertDefaultContainerYard_Import(RelatedPartyDefaultingTypeList.Codes.Import, true);
			AssertDefaultContainerYard_Import(RelatedPartyDefaultingTypeList.Codes.Export, false);
			AssertDefaultContainerYard_Import(RelatedPartyDefaultingTypeList.Codes.Disabled, false);
		}

		void AssertDefaultContainerYard_Import(string registryValue, bool expected)
		{
			using (CustomsDataRegistry.Instance.RelatedPartyDefaultingContainerYard.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var carrier = Factory.NewWithValidTestData<OrgHeader>();
				var port = carrier.CarrierAppointedAgentPorts_ContainerYardPark.AddNew();
				var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
				port.O5_PortOrCountry = "AUSYD";
				port.O5_OA_AgentOfficeAddress = orgAddress.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = Constants.TransportModes.Sea;
				declaration.JE_OH_ShippingLine = carrier.PK;
				declaration.JE_RL_NKFinalDestination = "AUSYD";

				AssertEquals("CarrierAppointedAgentPorts_ContainerYardPark, by JE_RL_NKFinalDestination", expected, orgAddress.PK == declaration.ContainerYardDocAddress.E2_OA_Address);

				declaration.JE_OH_ShippingLine = ZGuid.Empty;
				declaration.ContainerYardDocAddress.E2_OA_Address = ZGuid.Empty;
				declaration.JE_OH_ShippingLine = carrier.PK;
				AssertEquals("CarrierAppointedAgentPorts_ContainerYardPark, by JE_OH_ShippingLine", expected, orgAddress.PK == declaration.ContainerYardDocAddress.E2_OA_Address);

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.ContainerYardDocAddress.E2_OA_Address = ZGuid.Empty;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("CarrierAppointedAgentPorts_ContainerYardPark, by JE_MessageType", expected, orgAddress.PK == declaration.ContainerYardDocAddress.E2_OA_Address);

				declaration.JE_RL_NKFinalDestination = "USLAX";
				declaration.ContainerYardDocAddress.E2_OA_Address = ZGuid.Empty;
				declaration.JE_RL_NKFinalDestination = "AUSYD";
				AssertEquals("CarrierAppointedAgentPorts_ContainerYardPark, by JE_RL_NKFinalDestination", expected, orgAddress.PK == declaration.ContainerYardDocAddress.E2_OA_Address);
			}
		}

		public void TestDefaultContainerYard_Export()
		{
			AssertDefaultContainerYard_Export(RelatedPartyDefaultingTypeList.Codes.Both, true);
			AssertDefaultContainerYard_Export(RelatedPartyDefaultingTypeList.Codes.Import, false);
			AssertDefaultContainerYard_Export(RelatedPartyDefaultingTypeList.Codes.Export, true);
			AssertDefaultContainerYard_Export(RelatedPartyDefaultingTypeList.Codes.Disabled, false);
		}

		void AssertDefaultContainerYard_Export(string registryValue, bool expected)
		{
			using (CustomsDataRegistry.Instance.RelatedPartyDefaultingContainerYard.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var carrier = Factory.NewWithValidTestData<OrgHeader>();
				var port = carrier.CarrierAppointedAgentPorts_ContainerYardPark.AddNew();
				var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
				port.O5_PortOrCountry = "AUSYD";
				port.O5_OA_AgentOfficeAddress = orgAddress.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_TransportMode = Constants.TransportModes.Sea;
				declaration.JE_OH_ShippingLine = carrier.PK;
				declaration.JE_RL_NKOrigin = "AUSYD";

				AssertEquals("CarrierAppointedAgentPorts_ContainerYardPark", expected, orgAddress.PK == declaration.ContainerYardDocAddress.E2_OA_Address);

				declaration.JE_RL_NKOrigin = "USLAX";
				declaration.ContainerYardDocAddress.E2_OA_Address = ZGuid.Empty;
				declaration.JE_RL_NKOrigin = "AUSYD";
				AssertEquals("CarrierAppointedAgentPorts_ContainerYardPark, by JE_RL_NKFinalDestination", expected, orgAddress.PK == declaration.ContainerYardDocAddress.E2_OA_Address);
			}
		}

		public void TestDefaultControllingCustomer_Disabled()
		{
			using (FreightDataRegistry.Instance.DefaultShipmentControllingCustomer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (FreightDataRegistry.Instance.DefaultControllingCustomerRule.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CreateEntityPrecedenceRuleForDefaultControllingCustomer()))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var billToParty = Factory.NewWithValidTestData<OrgHeader>();
				var relatedParty = billToParty.AllRelatedParties.AddNew();
				relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ControllingCustomer;
				relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				relatedParty.PR_GC = GlbCompany.CurrentCompany.PK;
				relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;

				var job = new JobHeader.Loader(declaration).TryLoadOrCreate();
				job.JH_OA_LocalChargesAddr = billToParty.MainAddress.PK;

				declaration.RunPreSaveValidation();
				AssertNotEquals(GlbCompany.CurrentCompany.OrgProxy.PK.ToString(), declaration.JE_OH_ControllingCustomer.ToString());
			}
		}

		public void TestDefaultControllingCustomer_BillToParty()
		{
			using (FreightDataRegistry.Instance.DefaultShipmentControllingCustomer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.DefaultControllingCustomerRule.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CreateEntityPrecedenceRuleForDefaultControllingCustomer()))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var billToParty = Factory.NewWithValidTestData<OrgHeader>();
				var relatedParty = billToParty.AllRelatedParties.AddNew();
				relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ControllingCustomer;
				relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				relatedParty.PR_GC = GlbCompany.CurrentCompany.PK;
				relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;

				var job = new JobHeader.Loader(declaration).TryLoadOrCreate();
				job.JH_OA_LocalChargesAddr = billToParty.MainAddress.PK;

				declaration.RunPreSaveValidation();
				AssertEquals(GlbCompany.CurrentCompany.OrgProxy.PK.ToString(), declaration.JE_OH_ControllingCustomer.ToString());
			}
		}

		public void TestDefaultControllingCustomer_BookingParty()
		{
			using (FreightDataRegistry.Instance.DefaultShipmentControllingCustomer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.DefaultControllingCustomerRule.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CreateEntityPrecedenceRuleForDefaultControllingCustomer()))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_IsBooking = true;
				shipment.JS_IsForwardRegistered = false;
				declaration.JE_JS = shipment.PK;
				var billToParty = Factory.NewWithValidTestData<OrgHeader>();
				var bookingClient = Factory.NewWithValidTestData<OrgHeader>();
				var relatedParty = bookingClient.AllRelatedParties.AddNew();
				relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ControllingCustomer;
				relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				relatedParty.PR_GC = GlbCompany.CurrentCompany.PK;
				relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;

				var job = new JobHeader.Loader(declaration).TryLoadOrCreate();
				job.JH_OA_LocalChargesAddr = billToParty.MainAddress.PK;

				var quotedBookingBuilder = ObjectFactory.Get<IQuotedBookingBuilder>();
				var quotedBooking = quotedBookingBuilder.InitializeFrom(shipment.PK, Factory);
				quotedBooking.ClientPK = bookingClient.PK;

				declaration.RunPreSaveValidation();
				AssertEquals(GlbCompany.CurrentCompany.OrgProxy.PK.ToString(), declaration.JE_OH_ControllingCustomer.ToString());
			}
		}

		public void TestDefaultControllingCustomer_Consignee()
		{
			using (FreightDataRegistry.Instance.DefaultShipmentControllingCustomer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.DefaultControllingCustomerRule.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CreateEntityPrecedenceRuleForDefaultControllingCustomer()))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var billToParty = Factory.NewWithValidTestData<OrgHeader>();
				var consignee = Factory.NewWithValidTestData<OrgHeader>();
				var consigneesIFTOrg = Factory.NewWithValidTestData<OrgHeader>();
				var consigneeIFTrelatedParty = consignee.AllRelatedParties.AddNew();
				consigneeIFTrelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo;
				consigneeIFTrelatedParty.PR_OH_RelatedParty = consigneesIFTOrg.PK;
				consigneeIFTrelatedParty.PR_GC = GlbCompany.CurrentCompany.PK;
				consigneeIFTrelatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;

				var consigneeIFTControllingCustomerRelatedParty = consigneesIFTOrg.AllRelatedParties.AddNew();
				consigneeIFTControllingCustomerRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ControllingCustomer;
				consigneeIFTControllingCustomerRelatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				consigneeIFTControllingCustomerRelatedParty.PR_GC = GlbCompany.CurrentCompany.PK;
				consigneeIFTControllingCustomerRelatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;

				var consignor = Factory.NewWithValidTestData<OrgHeader>();

				var job = new JobHeader.Loader(declaration).TryLoadOrCreate();
				job.JH_OA_LocalChargesAddr = billToParty.MainAddress.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_OH_Importer = consignee.PK;

				declaration.RunPreSaveValidation();
				AssertEquals(GlbCompany.CurrentCompany.OrgProxy.PK.ToString(), declaration.JE_OH_ControllingCustomer.ToString());
			}
		}

		public void TestDefaultControllingCustomer_Consignor()
		{
			using (FreightDataRegistry.Instance.DefaultShipmentControllingCustomer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.DefaultControllingCustomerRule.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CreateEntityPrecedenceRuleForDefaultControllingCustomer()))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var billToParty = Factory.NewWithValidTestData<OrgHeader>();
				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				var consignorsIFTOrg = Factory.NewWithValidTestData<OrgHeader>();
				var consignorIFTrelatedParty = consignor.AllRelatedParties.AddNew();
				consignorIFTrelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo;
				consignorIFTrelatedParty.PR_OH_RelatedParty = consignorsIFTOrg.PK;
				consignorIFTrelatedParty.PR_GC = GlbCompany.CurrentCompany.PK;
				consignorIFTrelatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;

				var consignorIFTControllingCustomerRelatedParty = consignorsIFTOrg.AllRelatedParties.AddNew();
				consignorIFTControllingCustomerRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ControllingCustomer;
				consignorIFTControllingCustomerRelatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				consignorIFTControllingCustomerRelatedParty.PR_GC = GlbCompany.CurrentCompany.PK;
				consignorIFTControllingCustomerRelatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;

				var consignee = Factory.NewWithValidTestData<OrgHeader>();

				var job = new JobHeader.Loader(declaration).TryLoadOrCreate();
				job.JH_OA_LocalChargesAddr = billToParty.MainAddress.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_OH_Supplier = consignor.PK;
				declaration.JE_ShipmentIncoTerm = Constants.IncoTerms.CostAndFreight;

				declaration.RunPreSaveValidation();
				AssertEquals(GlbCompany.CurrentCompany.OrgProxy.PK.ToString(), declaration.JE_OH_ControllingCustomer.ToString());
			}
		}

		public void TestDefaultControllingAgent_Disabled()
		{
			using (FreightDataRegistry.Instance.DefaultShipmentControllingAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (FreightDataRegistry.Instance.DefaultControllingAgentRule.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CreateEntityPrecedenceRuleForDefaultControllingAgent()))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
				var relatedParty = controllingCustomer.AllRelatedParties.AddNew();
				relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ControllingAgent;
				relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				relatedParty.PR_GC = GlbCompany.CurrentCompany.PK;
				relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Sales;
				declaration.JE_OH_ControllingCustomer = controllingCustomer.PK;

				declaration.RunPreSaveValidation();
				AssertNotEquals(GlbCompany.CurrentCompany.OrgProxy.PK.ToString(), declaration.JE_OH_ControllingAgent.ToString());
			}
		}

		public void TestDefaultControllingAgent_ControllingCustomer()
		{
			using (FreightDataRegistry.Instance.DefaultShipmentControllingAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.DefaultControllingAgentRule.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CreateEntityPrecedenceRuleForDefaultControllingAgent()))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
				var relatedParty = controllingCustomer.AllRelatedParties.AddNew();
				relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ControllingAgent;
				relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				relatedParty.PR_GC = GlbCompany.CurrentCompany.PK;
				relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Sales;
				declaration.JE_OH_ControllingCustomer = controllingCustomer.PK;

				declaration.RunPreSaveValidation();
				AssertEquals(GlbCompany.CurrentCompany.OrgProxy.PK.ToString(), declaration.JE_OH_ControllingAgent.ToString());
			}
		}

		public void TestDefaultControllingAgent_BillToParty()
		{
			using (FreightDataRegistry.Instance.DefaultShipmentControllingAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.DefaultControllingAgentRule.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CreateEntityPrecedenceRuleForDefaultControllingAgent()))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var billToParty = Factory.NewWithValidTestData<OrgHeader>();
				var relatedParty = billToParty.AllRelatedParties.AddNew();
				relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ControllingAgent;
				relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				relatedParty.PR_GC = GlbCompany.CurrentCompany.PK;
				relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Sales;

				var job = new JobHeader.Loader(declaration).TryLoadOrCreate();
				job.JH_OA_LocalChargesAddr = billToParty.MainAddress.PK;

				declaration.RunPreSaveValidation();
				AssertEquals(GlbCompany.CurrentCompany.OrgProxy.PK.ToString(), declaration.JE_OH_ControllingAgent.ToString());
			}
		}

		public void TestDefaultControllingAgent_BookingParty()
		{
			using (FreightDataRegistry.Instance.DefaultShipmentControllingAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.DefaultControllingAgentRule.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CreateEntityPrecedenceRuleForDefaultControllingAgent()))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_IsBooking = true;
				shipment.JS_IsForwardRegistered = false;
				declaration.JE_JS = shipment.PK;
				var billToParty = Factory.NewWithValidTestData<OrgHeader>();
				var bookingClient = Factory.NewWithValidTestData<OrgHeader>();
				var relatedParty = bookingClient.AllRelatedParties.AddNew();
				relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ControllingAgent;
				relatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				relatedParty.PR_GC = GlbCompany.CurrentCompany.PK;
				relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Sales;

				var job = new JobHeader.Loader(declaration).TryLoadOrCreate();
				job.JH_OA_LocalChargesAddr = billToParty.MainAddress.PK;

				var quotedBookingBuilder = ObjectFactory.Get<IQuotedBookingBuilder>();
				var quotedBooking = quotedBookingBuilder.InitializeFrom(shipment.PK, Factory);
				quotedBooking.ClientPK = bookingClient.PK;

				declaration.RunPreSaveValidation();
				AssertEquals(GlbCompany.CurrentCompany.OrgProxy.PK.ToString(), declaration.JE_OH_ControllingAgent.ToString());
			}
		}

		public void TestDefaultControllingAgent_Consignee()
		{
			using (FreightDataRegistry.Instance.DefaultShipmentControllingAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.DefaultControllingAgentRule.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CreateEntityPrecedenceRuleForDefaultControllingAgent()))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var billToParty = Factory.NewWithValidTestData<OrgHeader>();
				var consignee = Factory.NewWithValidTestData<OrgHeader>();
				var consigneesIFTOrg = Factory.NewWithValidTestData<OrgHeader>();
				var consigneeIFTrelatedParty = consignee.AllRelatedParties.AddNew();
				consigneeIFTrelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo;
				consigneeIFTrelatedParty.PR_OH_RelatedParty = consigneesIFTOrg.PK;
				consigneeIFTrelatedParty.PR_GC = GlbCompany.CurrentCompany.PK;
				consigneeIFTrelatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Pickup;

				var consigneeIFTControllingCustomerRelatedParty = consigneesIFTOrg.AllRelatedParties.AddNew();
				consigneeIFTControllingCustomerRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ControllingAgent;
				consigneeIFTControllingCustomerRelatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				consigneeIFTControllingCustomerRelatedParty.PR_GC = GlbCompany.CurrentCompany.PK;
				consigneeIFTControllingCustomerRelatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Sales;

				var consignor = Factory.NewWithValidTestData<OrgHeader>();

				var job = new JobHeader.Loader(declaration).TryLoadOrCreate();
				job.JH_OA_LocalChargesAddr = billToParty.MainAddress.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_OH_Importer = consignee.PK;

				declaration.RunPreSaveValidation();
				AssertEquals(GlbCompany.CurrentCompany.OrgProxy.PK.ToString(), declaration.JE_OH_ControllingAgent.ToString());
			}
		}

		public void TestDefaultControllingAgent_Consignor()
		{
			using (FreightDataRegistry.Instance.DefaultShipmentControllingAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.DefaultControllingAgentRule.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CreateEntityPrecedenceRuleForDefaultControllingAgent()))
			{
				var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var billToParty = Factory.NewWithValidTestData<OrgHeader>();
				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				var consignorsIFTOrg = Factory.NewWithValidTestData<OrgHeader>();
				var consignorIFTrelatedParty = consignor.AllRelatedParties.AddNew();
				consignorIFTrelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo;
				consignorIFTrelatedParty.PR_OH_RelatedParty = consignorsIFTOrg.PK;
				consignorIFTrelatedParty.PR_GC = GlbCompany.CurrentCompany.PK;
				consignorIFTrelatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;

				var consignorIFTControllingCustomerRelatedParty = consignorsIFTOrg.AllRelatedParties.AddNew();
				consignorIFTControllingCustomerRelatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ControllingAgent;
				consignorIFTControllingCustomerRelatedParty.PR_OH_RelatedParty = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				consignorIFTControllingCustomerRelatedParty.PR_GC = GlbCompany.CurrentCompany.PK;
				consignorIFTControllingCustomerRelatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Sales;

				var consignee = Factory.NewWithValidTestData<OrgHeader>();

				var job = new JobHeader.Loader(declaration).TryLoadOrCreate();
				job.JH_OA_LocalChargesAddr = billToParty.MainAddress.PK;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				declaration.JE_OH_Supplier = consignor.PK;
				declaration.JE_ShipmentIncoTerm = Constants.IncoTerms.CostAndFreight;

				declaration.RunPreSaveValidation();
				AssertEquals(GlbCompany.CurrentCompany.OrgProxy.PK.ToString(), declaration.JE_OH_ControllingAgent.ToString());
			}
		}

		EntityPrecedenceRule CreateEntityPrecedenceRuleForDefaultControllingCustomer()
		{
			var entityPrecedenceRule = new EntityPrecedenceRule();
			entityPrecedenceRule.SelectedItems.Add(new EntityPrecedenceRuleItem { Code = Constants.DefaultControllingCustomerOrgTypes.Code.BillToParty, Description = Constants.DefaultControllingCustomerOrgTypes.Description.BillToParty });
			entityPrecedenceRule.SelectedItems.Add(new EntityPrecedenceRuleItem { Code = Constants.DefaultControllingCustomerOrgTypes.Code.BookingParty, Description = Constants.DefaultControllingCustomerOrgTypes.Description.BookingParty });
			entityPrecedenceRule.SelectedItems.Add(new EntityPrecedenceRuleItem { Code = Constants.DefaultControllingCustomerOrgTypes.Code.ConsignorConsignee, Description = Constants.DefaultControllingCustomerOrgTypes.Description.ConsignorConsignee });
			return entityPrecedenceRule;
		}

		EntityPrecedenceRule CreateEntityPrecedenceRuleForDefaultControllingAgent()
		{
			var entityPrecedenceRule = new EntityPrecedenceRule();
			entityPrecedenceRule.SelectedItems.Add(new EntityPrecedenceRuleItem { Code = Constants.DefaultControllingAgentOrgTypes.Code.ControllingCustomer, Description = Constants.DefaultControllingAgentOrgTypes.Description.ControllingCustomer });
			entityPrecedenceRule.SelectedItems.Add(new EntityPrecedenceRuleItem { Code = Constants.DefaultControllingAgentOrgTypes.Code.BillToParty, Description = Constants.DefaultControllingAgentOrgTypes.Description.BillToParty });
			entityPrecedenceRule.SelectedItems.Add(new EntityPrecedenceRuleItem { Code = Constants.DefaultControllingAgentOrgTypes.Code.BookingParty, Description = Constants.DefaultControllingAgentOrgTypes.Description.BookingParty });
			entityPrecedenceRule.SelectedItems.Add(new EntityPrecedenceRuleItem { Code = Constants.DefaultControllingAgentOrgTypes.Code.ConsignorConsignee, Description = Constants.DefaultControllingAgentOrgTypes.Description.ConsignorConsignee });
			return entityPrecedenceRule;
		}

		internal static OrgHeader CreateOrgRelatedParty(BusinessObjectFactory factory, OrgHeader organisation, string partyType, string freightDirection, string location)
		{
			var orgRelated = factory.New<OrgHeader>();
			orgRelated.OH_Code = "DXZENTBNE";
			orgRelated.OH_FullName = "DXZ Enterprises";
			orgRelated.OH_IsForwarder = true;
			orgRelated.MainAddress.OA_Address1 = "1 Street St";

			var relatedParty = organisation.AllRelatedParties.AddNew();
			relatedParty.PR_PartyType = partyType;
			relatedParty.PR_OH_RelatedParty = orgRelated.PK;
			relatedParty.PR_FreightDirection = freightDirection;
			relatedParty.PR_FreightTransportMode = Constants.TransportModes.Sea;
			relatedParty.PR_Location = location;
			relatedParty.PR_FreightContainerMode = Constants.ContainerModes.Bulk;
			return orgRelated;
		}

		internal static BaseJobDeclaration CreateDeclarationForRelatedParty(BusinessObjectFactory factory, string messageType, string origin, string destination, ZGuid importerPk, ZGuid supplierPk)
		{
			var declaration = factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_TransportMode = Constants.TransportModes.Sea;
			declaration.JE_ContainerMode = Constants.ContainerModes.Bulk;
			declaration.JE_RL_NKOrigin = origin;
			declaration.JE_RL_NKFinalDestination = destination;
			declaration.JE_OH_Importer = importerPk;
			declaration.JE_OH_Supplier = supplierPk;
			return declaration;
		}
	}
}
