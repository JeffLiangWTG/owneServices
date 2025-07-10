using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;

namespace Enterprise.Freight.Forwarding.Documents.FR.Testing
{
	sealed class ShipmentDossierBuilderTest : TestCaseWithFactory
	{
		public void TestBuild_Import()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HOUSE_111";
			CreateUnlocosAndReleaseDepots(shipment);
			var consol = CreateConsol();
			shipment.Consols.Add(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.DossierImport
			};

			var builder = new ShipmentDossierBuilder(shipment, parameters);
			var dossier = builder.Build();

			AssertNotNull(dossier);

			Assert("IsImport", dossier.IsImport);
			Assert("ImplicitAcknowledgement", dossier.ImplicitAcknowledgement);
			Assert("ImplicitBAET", dossier.ImplicitBAET);
			AssertEquals("PCS", FrenchPortsConstants.PCS.MGI, dossier.PCS);
			AssertEquals("OperationalPort", "FR0MG", dossier.OperationalPort.Code);
			AssertEquals("ShipmentNumber", shipment.JS_UniqueConsignRef, dossier.ShipmentNumber);
			AssertEquals("ThirdParty_Reference", shipment.JS_UniqueConsignRef, dossier.ThirdPartyReference);
			AssertEquals("BillOfLading", shipment.JS_HouseBill, dossier.BillOfLading);

			AssertAddressData(consol.ShippingLineAddress, dossier.Carrier);
			AssertAddressData(consol.ReceivingForwarderAddress, dossier.ReceivingForwarder);
			AssertAddressData(consol.SendingForwarderAddress, dossier.SendingForwarder);
			AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, dossier.SendingParty);
			AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, dossier.Agent);
		}

		public void TestBuild_Export()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HOUSE_111";
			CreateUnlocosAndReleaseDepots(shipment);
			var consol = CreateConsol(isImport: false);
			shipment.Consols.Add(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.DossierExport
			};

			var builder = new ShipmentDossierBuilder(shipment, parameters);
			var dossier = builder.Build();

			AssertNotNull(dossier);

			Assert("IsImport", !dossier.IsImport);
			Assert("ImplicitAcknowledgement", dossier.ImplicitAcknowledgement);
			Assert("ImplicitBAET", dossier.ImplicitBAET);
			AssertEquals("PCS", FrenchPortsConstants.PCS.Soget, dossier.PCS);
			AssertEquals("OperationalPort", "FR0SO", dossier.OperationalPort.Code);
			AssertEquals("ShipmentNumber", shipment.JS_UniqueConsignRef, dossier.ShipmentNumber);
			AssertEquals("ThirdParty_Reference", shipment.JS_UniqueConsignRef, dossier.ThirdPartyReference);
			AssertEquals("BillOfLading", string.Empty, dossier.BillOfLading);

			AssertAddressData(consol.ShippingLineAddress, dossier.Carrier);
			AssertAddressData(consol.ReceivingForwarderAddress, dossier.ReceivingForwarder);
			AssertAddressData(consol.SendingForwarderAddress, dossier.SendingForwarder);
			AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, dossier.SendingParty);
			AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, dossier.Agent);
		}

		public void TestPCSAndOperationalPortWithFallback_Import()
		{
			var shipment = Factory.New<ForwardingShipment>();
			CreateUnlocosAndReleaseDepots(shipment);
			var consol = CreateConsol();
			shipment.Consols.Add(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.DossierImport
			};

			var builder = new ShipmentDossierBuilder(shipment, parameters);
			var dossier = builder.Build();
			AssertEquals("IsImport", true, dossier.IsImport);
			AssertEquals("PCS", FrenchPortsConstants.PCS.MGI, dossier.PCS);
			AssertEquals("OperationalPort", "FR0MG", dossier.OperationalPort.Code);

			shipment.ImportReleaseDepot.Delete();
			dossier = builder.Build();
			AssertEquals("PCS", ZString.Empty, dossier.PCS);
			AssertHasMessageError("PCS Error", dossier.PCSInfo, "PCS Code is required. (Maintain > Locations > UNLOCO of Operational Port > Local Codes - Usage PCS)");
			AssertHasMessageError("OperationalPort Error", ((Unloco)dossier.OperationalPort)?.CodeInfo, "Operational Port is required, UNLOCO missing from Shipment > Delivery > CFS.");
		}

		public void TestPCSAndOperationalPortWithFallback_Export()
		{
			var shipment = Factory.New<ForwardingShipment>();
			CreateUnlocosAndReleaseDepots(shipment);
			var consol = CreateConsol(isImport: false);
			shipment.Consols.Add(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.DossierExport
			};

			var builder = new ShipmentDossierBuilder(shipment, parameters);
			var dossier = builder.Build();
			AssertEquals("IsImport", false, dossier.IsImport);
			AssertEquals("PCS", FrenchPortsConstants.PCS.Soget, dossier.PCS);
			AssertEquals("OperationalPort", "FR0SO", dossier.OperationalPort.Code);

			shipment.ExportReceivingDepot.Delete();
			dossier = builder.Build();
			AssertEquals("PCS", ZString.Empty, dossier.PCS);
			AssertHasMessageError("PCS Error", dossier.PCSInfo, "PCS Code is required. (Maintain > Locations > UNLOCO of Operational Port > Local Codes - Usage PCS)");
			AssertHasMessageError("OperationalPort Error", ((Unloco)dossier.OperationalPort)?.CodeInfo, "Operational Port is required, UNLOCO missing from Shipment > Pickup > CFS.");
		}

		public void TestBuildNoConsol_Import()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "FRPAR";
			shipment.JS_RL_NKDestination = "AUSYD";
			CreateUnlocosAndReleaseDepots(shipment);

			shipment.JS_OH_DeliveryAgent = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment.PickupAgentDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.DossierImport
			};

			var builder = new ShipmentDossierBuilder(shipment, parameters);
			var dossier = builder.Build();

			AssertNotNull(dossier);

			AssertEquals("ShipmentNumber", shipment.JS_UniqueConsignRef, dossier.ShipmentNumber);
			AssertEquals("ThirdParty_Reference", shipment.JS_UniqueConsignRef, dossier.ThirdPartyReference);

			AssertEquals("Carrier", null, dossier.Carrier);
			AssertEquals("ReceivingForwarder", null, dossier.ReceivingForwarder);
			AssertEquals("SendingForwarder", null, dossier.SendingForwarder);

			AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, dossier.SendingParty);
			AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, dossier.Agent);
			AssertAddressData(shipment.DeliveryAgent.MainAddress, dossier.ThirdParty);
		}

		public void TestBuildNoConsol_Export()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "FRPAR";
			CreateUnlocosAndReleaseDepots(shipment);

			shipment.JS_OH_DeliveryAgent = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment.PickupAgentDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.DossierExport
			};

			var builder = new ShipmentDossierBuilder(shipment, parameters);
			var dossier = builder.Build();

			AssertNotNull(dossier);

			AssertEquals("ShipmentNumber", shipment.JS_UniqueConsignRef, dossier.ShipmentNumber);
			AssertEquals("ThirdParty_Reference", shipment.JS_UniqueConsignRef, dossier.ThirdPartyReference);

			AssertEquals("Carrier", null, dossier.Carrier);
			AssertEquals("ReceivingForwarder", null, dossier.ReceivingForwarder);
			AssertEquals("SendingForwarder", null, dossier.SendingForwarder);

			AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, dossier.SendingParty);
			AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, dossier.Agent);
			AssertAddressData(shipment.PickupAgent.MainAddress, dossier.ThirdParty);
		}

		public void TestPopulateSendingAndAgentImport()
		{
			var branchProxy = Factory.NewWithValidTestData<OrgHeader>();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchProxy.PK;

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			var consol = CreateConsol();
			shipment.Consols.Add(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.DossierImport
			};

			var builder = new ShipmentDossierBuilder(shipment, parameters);

			var dossier = builder.Build();
			if (branchProxy.MainAddress.OA_RN_NKCountryCode.IsEmpty)
			{
				branchProxy.MainAddress.OA_RN_NKCountryCode = GlbBranch.CurrentBranch.BaseCountry?.Code ?? ZString.Empty;
			}
			AssertAddressData(branchProxy.MainAddress, dossier.SendingParty);
			AssertAddressData(branchProxy.MainAddress, dossier.Agent);

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;

			dossier = builder.Build();
			AssertAddressData(GlbCompany.CurrentCompany.OrgProxy.MainAddress, dossier.SendingParty);
			AssertAddressData(GlbCompany.CurrentCompany.OrgProxy.MainAddress, dossier.Agent);
		}

		public void TestPopulateSendingAndAgentExport()
		{
			var branchProxy = Factory.NewWithValidTestData<OrgHeader>();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchProxy.PK;

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			var consol = CreateConsol(isImport: false);
			shipment.Consols.Add(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.DossierExport
			};

			var builder = new ShipmentDossierBuilder(shipment, parameters);

			var dossier = builder.Build();
			if (branchProxy.MainAddress.OA_RN_NKCountryCode.IsEmpty)
			{
				branchProxy.MainAddress.OA_RN_NKCountryCode = GlbBranch.CurrentBranch.BaseCountry?.Code ?? ZString.Empty;
			}
			AssertAddressData(branchProxy.MainAddress, dossier.SendingParty);
			AssertAddressData(branchProxy.MainAddress, dossier.Agent);

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;

			dossier = builder.Build();
			AssertAddressData(GlbCompany.CurrentCompany.OrgProxy.MainAddress, dossier.SendingParty);
			AssertAddressData(GlbCompany.CurrentCompany.OrgProxy.MainAddress, dossier.Agent);
		}

		public void TestPopulateFormattedThirdPartyProviderID()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.DossierImport
			};

			var builder = new ShipmentDossierBuilder(shipment, parameters);
			var deliveryAgentOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			shipment.JS_OH_DeliveryAgent = deliveryAgentOrgHeader.PK;

			var deliverAgentAddress = shipment.DeliveryAgent.MainAddress;

			var sonCode1 = deliverAgentAddress.Header.CustomsCodes.AddNew();
			sonCode1.OK_CodeType = OrgCusCode.FranceCodeTypes.SON;
			sonCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			sonCode1.OK_CustomsRegNo = "S001";

			var sowCode1 = deliverAgentAddress.Header.CustomsCodes.AddNew();
			sowCode1.OK_CodeType = OrgCusCode.FranceCodeTypes.SOW;
			sowCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			sowCode1.OK_CustomsRegNo = "S001";

			var soaCode1 = deliverAgentAddress.Header.CustomsCodes.AddNew();
			soaCode1.OK_CodeType = OrgCusCode.FranceCodeTypes.SOA;
			soaCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			soaCode1.OK_CustomsRegNo = "S001";

			var ci5Code1 = deliverAgentAddress.Header.CustomsCodes.AddNew();
			ci5Code1.OK_CodeType = OrgCusCode.FranceCodeTypes.CI5;
			ci5Code1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			ci5Code1.OK_CustomsRegNo = "C001";

			AssertNullOrEmpty(builder.Build().FormattedThirdPartyProviderID);

			var operationalAddress = Factory.NewWithValidTestData<OrgAddress>();

			shipment.JS_OA_ImportReleaseDepot = operationalAddress.PK;

			var query = new ZQuery(RefUNLOCOSchema.RL_Code, "FRFR2");
			var unloco = Factory.LoadTop1<RefUNLOCO>(query);

			if (unloco == null)
			{
				unloco = Factory.NewWithValidTestData<RefUNLOCO>();
				unloco.RL_Code = "FRFR2";
			}

			RefLocoMap mapFRMGI = unloco.RefLocoMaps.AddNew();
			mapFRMGI.RY_RN = Core.Constants.CountryGuids.France;
			mapFRMGI.RY_SystemUsage = "PCS";
			mapFRMGI.RY_LocalPortCode = FrenchPortsConstants.PCS.MGI;

			operationalAddress.OA_RL_NKRelatedPortCode = unloco.Code;

			AssertEquals("CI5: C001", builder.Build().FormattedThirdPartyProviderID);

			var consol = CreateConsol();
			shipment.Consols.Add(consol);

			var receivingForwarderOrgAddress = Factory.NewWithValidTestData<OrgAddress>();

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarderOrgAddress.PK;

			var sonCode2 = receivingForwarderOrgAddress.Header.CustomsCodes.AddNew();
			sonCode2.OK_CodeType = OrgCusCode.FranceCodeTypes.SON;
			sonCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			sonCode2.OK_CustomsRegNo = "S002";

			var sowCode2 = receivingForwarderOrgAddress.Header.CustomsCodes.AddNew();
			sowCode2.OK_CodeType = OrgCusCode.FranceCodeTypes.SOW;
			sowCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			sowCode2.OK_CustomsRegNo = "S002";

			var soaCode2 = receivingForwarderOrgAddress.Header.CustomsCodes.AddNew();
			soaCode2.OK_CodeType = OrgCusCode.FranceCodeTypes.SOA;
			soaCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			soaCode2.OK_CustomsRegNo = "S002";

			var ci5Code2 = receivingForwarderOrgAddress.Header.CustomsCodes.AddNew();
			ci5Code2.OK_CodeType = OrgCusCode.FranceCodeTypes.CI5;
			ci5Code2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			ci5Code2.OK_CustomsRegNo = "C002";

			builder = new ShipmentDossierBuilder(shipment, parameters);
			AssertEquals("CI5: C002", builder.Build().FormattedThirdPartyProviderID);
		}

		public void TestPopulateAddressesImport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = CreateConsol();
			shipment.Consols.Add(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.DossierImport
			};
			var builder = new ShipmentDossierBuilder(shipment, parameters);
			var dossier = builder.Build();
			AssertNotNull(dossier);

			AssertAddressData(consol.ShippingLineAddress, dossier.Carrier);
			AssertAddressData(consol.ReceivingForwarderAddress, dossier.ReceivingForwarder);
			AssertAddressData(consol.SendingForwarderAddress, dossier.SendingForwarder);
			AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, dossier.SendingParty);
			AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, dossier.Agent);
			AssertAddressData(consol.ReceivingForwarderAddress, dossier.ThirdParty);
		}

		public void TestPopulateAddressesExport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = CreateConsol(isImport: false);
			shipment.Consols.Add(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.DossierExport
			};
			var builder = new ShipmentDossierBuilder(shipment, parameters);
			var dossier = builder.Build();
			AssertNotNull(dossier);

			AssertAddressData(consol.ShippingLineAddress, dossier.Carrier);
			AssertAddressData(consol.ReceivingForwarderAddress, dossier.ReceivingForwarder);
			AssertAddressData(consol.SendingForwarderAddress, dossier.SendingForwarder);
			AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, dossier.SendingParty);
			AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, dossier.Agent);
			AssertAddressData(consol.SendingForwarderAddress, dossier.ThirdParty);
		}

		public void TestPopulateAPPlusCodesImport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = CreateConsol();
			shipment.Consols.Add(consol);

			CreateAddresses(consol);

			AssertPopulateAPPlusCodes(shipment, consol.ShippingLineAddress, "CarrierSON", "CarrierCI5", ShipmentDocumentDataStoreNames.DossierImport);
			AssertPopulateAPPlusCodes(shipment, consol.ReceivingForwarderAddress, "ReceivingForwarderSON", "ReceivingForwarderCI5", ShipmentDocumentDataStoreNames.DossierImport);
			AssertPopulateAPPlusCodes(shipment, consol.SendingForwarderAddress, "SendingForwarderSON", "SendingForwarderCI5", ShipmentDocumentDataStoreNames.DossierImport);
			AssertPopulateAPPlusCodes(shipment, GlbBranch.CurrentBranch.OrgProxy.MainAddress, "SendingPartySON", "SendingPartyCI5", ShipmentDocumentDataStoreNames.DossierImport);
			AssertPopulateAPPlusCodes(shipment, GlbBranch.CurrentBranch.OrgProxy.MainAddress, "AgentSOA", "AgentCI5", ShipmentDocumentDataStoreNames.DossierImport);
			AssertPopulateAPPlusCodes(shipment, consol.ReceivingForwarderAddress, "ThirdPartySON", "ThirdPartyCI5", ShipmentDocumentDataStoreNames.DossierImport);

			AssertPopulateAPPlusCodesUseRegistry(true, "ThirdPartyCI5", "ThirdPartySON", ShipmentDocumentDataStoreNames.DossierImport, true);
			AssertPopulateAPPlusCodesUseRegistry(true, "AgentCI5", "AgentSOA", ShipmentDocumentDataStoreNames.DossierImport, false);
			AssertPopulateAPPlusCodesUseRegistry(true, "SendingPartyCI5", "SendingPartySON", ShipmentDocumentDataStoreNames.DossierImport, true);
		}

		public void TestPopulateAPPlusCodesExport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = CreateConsol(isImport: false);
			shipment.Consols.Add(consol);

			CreateAddresses(consol);

			AssertPopulateAPPlusCodes(shipment, consol.ShippingLineAddress, "CarrierSON", "CarrierCI5", ShipmentDocumentDataStoreNames.DossierExport);
			AssertPopulateAPPlusCodes(shipment, consol.ReceivingForwarderAddress, "ReceivingForwarderSON", "ReceivingForwarderCI5", ShipmentDocumentDataStoreNames.DossierExport);
			AssertPopulateAPPlusCodes(shipment, consol.SendingForwarderAddress, "SendingForwarderSON", "SendingForwarderCI5", ShipmentDocumentDataStoreNames.DossierExport);
			AssertPopulateAPPlusCodes(shipment, GlbBranch.CurrentBranch.OrgProxy.MainAddress, "SendingPartySON", "SendingPartyCI5", ShipmentDocumentDataStoreNames.DossierExport);
			AssertPopulateAPPlusCodes(shipment, GlbBranch.CurrentBranch.OrgProxy.MainAddress, "AgentSOA", "AgentCI5", ShipmentDocumentDataStoreNames.DossierExport);
			AssertPopulateAPPlusCodes(shipment, consol.SendingForwarderAddress, "ThirdPartySON", "ThirdPartyCI5", ShipmentDocumentDataStoreNames.DossierExport);

			AssertPopulateAPPlusCodesUseRegistry(false, "ThirdPartyCI5", "ThirdPartySON", ShipmentDocumentDataStoreNames.DossierExport, true);
			AssertPopulateAPPlusCodesUseRegistry(false, "AgentCI5", "AgentSOA", ShipmentDocumentDataStoreNames.DossierExport, false);
			AssertPopulateAPPlusCodesUseRegistry(false, "SendingPartyCI5", "SendingPartySON", ShipmentDocumentDataStoreNames.DossierExport, true);
		}

		void AssertPopulateAPPlusCodesUseRegistry(bool isImport, string ci5PropertyName, string sPropertyName, string exportOrImport, bool isForwarderCode)
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = CreateConsol(isImport: isImport);
			shipment.Consols.Add(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = exportOrImport
			};

			var builder = new ShipmentDossierBuilder(shipment, parameters);

			var operationalAddress = Factory.NewWithValidTestData<OrgAddress>();

			if (exportOrImport == ShipmentDocumentDataStoreNames.DossierImport)
			{
				shipment.JS_OA_ImportReleaseDepot = operationalAddress.PK;
			}
			else
			{
				shipment.JS_OA_ExportReceivingDepot = operationalAddress.PK;
			}

			var query = new ZQuery(RefUNLOCOSchema.RL_Code, "FRFR2");
			var unloco = Factory.LoadTop1<RefUNLOCO>(query);

			if (unloco == null)
			{
				unloco = Factory.NewWithValidTestData<RefUNLOCO>();
				unloco.RL_Code = "FRFR2";
			}

			operationalAddress.OA_RL_NKRelatedPortCode = unloco.Code;

			var registryCodes = new Freight.Business.CommunitySystemCodesOfForwarderAndAgentCollection();
			var code = registryCodes.AddNew();
			code.AgentCode = "AGENT";
			code.ForwarderCode = "FORWARDER";
			code.Port = "FRFR2";
			code.PCS = FrenchPortSystemCodeList.Codes.MGI;

			using (Freight.Business.PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCodes))
			{
				AssertEquals($"{ci5PropertyName} should be from registry", isForwarderCode ? "FORWARDER" : "AGENT", ((RegistrationNumber)builder.Build()[ci5PropertyName]).Value);
			}

			code.PCS = FrenchPortSystemCodeList.Codes.SOGET;
			using (Freight.Business.PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCodes))
			{
				AssertEquals($"{sPropertyName} should be from registry", isForwarderCode ? "FORWARDER" : "AGENT", ((RegistrationNumber)builder.Build()[sPropertyName]).Value);
			}
		}

		void AssertPopulateAPPlusCodes(ForwardingShipment shipment, OrgAddress address, string sPropertyName, string ci5PropertyName, string exportOrImport)
		{
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = exportOrImport
			};

			var builder = new ShipmentDossierBuilder(shipment, parameters);
			var sonCode1 = address.Header.CustomsCodes.AddNew();
			sonCode1.OK_CodeType = OrgCusCode.FranceCodeTypes.SON;
			sonCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			sonCode1.OK_CustomsRegNo = "S001";

			var sowCode1 = address.Header.CustomsCodes.AddNew();
			sowCode1.OK_CodeType = OrgCusCode.FranceCodeTypes.SOW;
			sowCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			sowCode1.OK_CustomsRegNo = "S001";

			var soaCode1 = address.Header.CustomsCodes.AddNew();
			soaCode1.OK_CodeType = OrgCusCode.FranceCodeTypes.SOA;
			soaCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			soaCode1.OK_CustomsRegNo = "S001";

			var ci5Code1 = address.Header.CustomsCodes.AddNew();
			ci5Code1.OK_CodeType = OrgCusCode.FranceCodeTypes.CI5;
			ci5Code1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			ci5Code1.OK_CustomsRegNo = "C001";

			var dossier = builder.Build();

			AssertEquals($"{sPropertyName} should be from org", "S001", ((RegistrationNumber)dossier[sPropertyName]).Value);
			AssertEquals($"{ci5PropertyName} should be from org", "C001", ((RegistrationNumber)dossier[ci5PropertyName]).Value);

			var sonCode2 = address.CustomsCodes.AddNew();
			sonCode2.OK_CodeType = OrgCusCode.FranceCodeTypes.SON;
			sonCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			sonCode2.OK_CustomsRegNo = "S002";

			var sowCode2 = address.CustomsCodes.AddNew();
			sowCode2.OK_CodeType = OrgCusCode.FranceCodeTypes.SOW;
			sowCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			sowCode2.OK_CustomsRegNo = "S002";

			var soaCode2 = address.CustomsCodes.AddNew();
			soaCode2.OK_CodeType = OrgCusCode.FranceCodeTypes.SOA;
			soaCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			soaCode2.OK_CustomsRegNo = "S002";

			var ci5Code2 = address.CustomsCodes.AddNew();
			ci5Code2.OK_CodeType = OrgCusCode.FranceCodeTypes.CI5;
			ci5Code2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			ci5Code2.OK_CustomsRegNo = "C002";

			dossier = builder.Build();

			AssertEquals($"{sPropertyName} should be from address", "S002", ((RegistrationNumber)dossier[sPropertyName]).Value);
			AssertEquals($"{ci5PropertyName} should be from address", "C002", ((RegistrationNumber)dossier[ci5PropertyName]).Value);

			address.Header.CustomsCodes.RemoveAndDeleteAll();
			address.CustomsCodes.DeleteAll();

			dossier = builder.Build();

			AssertEquals(string.Empty, ((RegistrationNumber)dossier[sPropertyName]).Value);
			AssertEquals(string.Empty, ((RegistrationNumber)dossier[ci5PropertyName]).Value);
		}

		public void TestPopulateConfirmationReference()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = CreateConsol(isImport: true);
			shipment.Consols.Add(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.DossierImport
			};
			CreateEvent(shipment, AutoEvents.MessageSent);

			var builder = new ShipmentDossierBuilder(shipment, parameters);
			var dossier = builder.Build();

			AssertEquals("MAA event hasn't been received", string.Empty, dossier.ConfirmationReference);

			CreateEvent(shipment, AutoEvents.MessageAccepted, "X00001");
			dossier = builder.Build();

			AssertEquals("Populated from RFN parameter of MAA event", "X00001", dossier.ConfirmationReference);

			CreateEvent(shipment, AutoEvents.MessageWithdrawCancelRequest);
			CreateEvent(shipment, AutoEvents.MessageWithdrawCancelAccepted);
			dossier = builder.Build();

			AssertEquals("Return empty as withdraw is finished", string.Empty, dossier.ConfirmationReference);
		}

		public void TestPopulateECVReference()
		{
			var shipment = CreateShipment("SH0001001");
			var consol = CreateConsol("C00001001", isImport: true);
			shipment.Consols.Add(consol);

			var notSendAgentRefs = new ZString[] { "ERC_1", "ERC_2", "ERC_3" }.ToList();
			var dossier = GetNewShipmentDossierBuilder(shipment, notSendAgentRefs).Build();
			AssertEquals("ECVReference not empty", "ECV_1, ECV_2, ECV_3", dossier.ECVReference);

			shipment.OuterPackLines[0].AdditionalReferenceNumbers.GetFirstReferenceNumberByType(ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC).CE_EntryStatus = Events.MessageSentCode;
			notSendAgentRefs = new ZString[] { "ERC_2", "ERC_3" }.ToList();
			dossier = GetNewShipmentDossierBuilder(shipment, notSendAgentRefs).Build();
			AssertEquals("ECVReference not empty, because CRESA MSN event present", "ECV_2, ECV_3", dossier.ECVReference);

			var shipmentThatAlreadySendRefs = CreateShipment("SH0001002");
			var consolThatAlreadySendRefs = CreateConsol("C00001002", isImport: true);
			shipmentThatAlreadySendRefs.Consols.Add(consolThatAlreadySendRefs);

			foreach (CusEntryNumber entryNumber in shipmentThatAlreadySendRefs.OuterPackLines.Cast<ForwardingPackLine>().SelectMany(p => p.AdditionalReferenceNumbers))
			{
				entryNumber.CE_EntryStatus = Events.MessageSentCode;
			}
			shipmentThatAlreadySendRefs.Factory.Save();

			notSendAgentRefs = Array.Empty<ZString>().ToList();
			dossier = GetNewShipmentDossierBuilder(shipment, notSendAgentRefs).Build();
			AssertEquals("ECVReference empty, because refs already sent on other shipment", string.Empty, dossier.ECVReference);

			dossier.ECVReference = string.Empty;
			dossier.ValidateAllIncludingChildren();
			AssertNoErrors("ECV Reference is not required is this is an import shipment.", dossier.ECVReferenceInfo);
		}

		public void TestPopulateAgentReference_Import()
		{
			var shipment = CreateShipment("SH0001001");
			var consol = CreateConsol("C00001001", isImport: true);
			shipment.Consols.Add(consol);

			var notSendAgentRefs = Array.Empty<ZString>().ToList();
			var dossier = GetNewShipmentDossierBuilder(shipment, notSendAgentRefs).Build();
			AssertEquals("AgentReference should be shipment number when dossier is import", shipment.JS_UniqueConsignRef, dossier.AgentReference);

			notSendAgentRefs = new ZString[] { "ERC_1", "ERC_2", "ERC_3" }.ToList();
			dossier = GetNewShipmentDossierBuilder(shipment, notSendAgentRefs).Build();
			AssertEquals("AgentReference should be shipment number when dossier is import", shipment.JS_UniqueConsignRef, dossier.AgentReference);

			shipment.OuterPackLines[0].AdditionalReferenceNumbers.GetFirstReferenceNumberByType(ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC).CE_EntryNum = "ERC_2";
			notSendAgentRefs = new ZString[] { "ERC_2", "ERC_3" }.ToList();
			dossier = GetNewShipmentDossierBuilder(shipment, notSendAgentRefs).Build();
			AssertEquals("AgentReference not empty, because ERC referencenumbers on packinglines", shipment.JS_UniqueConsignRef, dossier.AgentReference);

			shipment.OuterPackLines[2].AdditionalReferenceNumbers.GetFirstReferenceNumberByType(ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC).CE_EntryStatus = Events.MessageSentCode;
			notSendAgentRefs = new ZString[] { "ERC_2" }.ToList();
			dossier = GetNewShipmentDossierBuilder(shipment, notSendAgentRefs).Build();
			AssertEquals("AgentReference not empty, because ERC referencenumbers on packinglines", shipment.JS_UniqueConsignRef, dossier.AgentReference);

			var shipmentThatAlreadySendRefs = CreateShipment("SH0001002");
			var consolThatAlreadySendRefs = CreateConsol("C00001002", isImport: true);
			shipmentThatAlreadySendRefs.Consols.Add(consolThatAlreadySendRefs);

			shipmentThatAlreadySendRefs.OuterPackLines[0].AdditionalReferenceNumbers.GetFirstReferenceNumberByType(ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC).CE_EntryStatus = Events.MessageSentCode;
			shipmentThatAlreadySendRefs.OuterPackLines[1].AdditionalReferenceNumbers.GetFirstReferenceNumberByType(ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC).CE_EntryStatus = Events.MessageSentCode;
			shipmentThatAlreadySendRefs.OuterPackLines[2].AdditionalReferenceNumbers.GetFirstReferenceNumberByType(ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC).CE_EntryStatus = Events.MessageSentCode;
			shipmentThatAlreadySendRefs.Factory.Save();

			notSendAgentRefs = Array.Empty<ZString>().ToList();
			dossier = GetNewShipmentDossierBuilder(shipment, notSendAgentRefs).Build();
			AssertEquals("AgentReference should not empty,because dossier is import,even if has sent to another shipment", shipment.JS_UniqueConsignRef, dossier.AgentReference);

			CreateEvent(shipment, AutoEvents.MessageSent, documentName: FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA);
			notSendAgentRefs = Array.Empty<ZString>().ToList();
			dossier = GetNewShipmentDossierBuilder(shipment, notSendAgentRefs).Build();
			AssertEquals("AgentReference should not empty,because dossier is import", shipment.JS_UniqueConsignRef, dossier.AgentReference);
		}

		public void TestPopulateAgentReference_Export()
		{
			var shipment = CreateShipment("SH0001001");
			var consol = CreateConsol("C00001001", isImport: false);
			shipment.Consols.Add(consol);

			var notSendAgentRefs = Array.Empty<ZString>().ToList();
			var dossier = GetNewShipmentDossierBuilder(shipment, notSendAgentRefs, false).Build();
			AssertNullOrEmpty(dossier.AgentReference);
			AssertHasMessageError(dossier.AgentReferenceInfo, "Agent Reference is required.");

			notSendAgentRefs = new ZString[] { "ERC_1", "ERC_2", "ERC_3" }.ToList();
			dossier = GetNewShipmentDossierBuilder(shipment, notSendAgentRefs, false).Build();
			AssertEquals("ERC_1, ERC_2, ERC_3", dossier.AgentReference);

			shipment.OuterPackLines[0].AdditionalReferenceNumbers.GetFirstReferenceNumberByType(ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC).CE_EntryNum = "ERC_2";
			notSendAgentRefs = new ZString[] { "ERC_2", "ERC_3" }.ToList();
			dossier = GetNewShipmentDossierBuilder(shipment, notSendAgentRefs, false).Build();
			AssertEquals("ERC_2, ERC_3", dossier.AgentReference);

			shipment.OuterPackLines[2].AdditionalReferenceNumbers.GetFirstReferenceNumberByType(ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC).CE_EntryStatus = Events.MessageSentCode;
			notSendAgentRefs = new ZString[] { "ERC_2" }.ToList();
			dossier = GetNewShipmentDossierBuilder(shipment, notSendAgentRefs, false).Build();
			AssertEquals("ERC_2", dossier.AgentReference);

			var shipmentThatAlreadySendRefs = CreateShipment("SH0001002");
			var consolThatAlreadySendRefs = CreateConsol("C00001002", isImport: false);
			shipmentThatAlreadySendRefs.Consols.Add(consolThatAlreadySendRefs);

			shipmentThatAlreadySendRefs.OuterPackLines[0].AdditionalReferenceNumbers.GetFirstReferenceNumberByType(ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC).CE_EntryStatus = Events.MessageSentCode;
			shipmentThatAlreadySendRefs.OuterPackLines[1].AdditionalReferenceNumbers.GetFirstReferenceNumberByType(ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC).CE_EntryStatus = Events.MessageSentCode;
			shipmentThatAlreadySendRefs.OuterPackLines[2].AdditionalReferenceNumbers.GetFirstReferenceNumberByType(ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC).CE_EntryStatus = Events.MessageSentCode;
			shipmentThatAlreadySendRefs.Factory.Save();

			notSendAgentRefs = Array.Empty<ZString>().ToList();
			dossier = GetNewShipmentDossierBuilder(shipment, notSendAgentRefs, false).Build();
			AssertNullOrEmpty(dossier.AgentReference);

			CreateEvent(shipment, AutoEvents.MessageSent, documentName: FrenchPortsConstants.DocumentNames.GoodsReceivedCRESA);
			notSendAgentRefs = Array.Empty<ZString>().ToList();
			dossier = GetNewShipmentDossierBuilder(shipment, notSendAgentRefs, false).Build();
			AssertEquals("SH0001001", dossier.AgentReference);

			notSendAgentRefs = new ZString[] { "ERC_2", "ERC_3" }.ToList();
			dossier = GetNewShipmentDossierBuilder(shipment, notSendAgentRefs, false).Build();
			AssertEquals("SH0001001", dossier.AgentReference);
		}

		ShipmentDossierBuilder GetNewShipmentDossierBuilder(ForwardingShipment shipment, object data, bool isImport = true)
		{
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = isImport ? ShipmentDocumentDataStoreNames.DossierImport : ShipmentDocumentDataStoreNames.DossierExport,
				Data = data
			};
			return new ShipmentDossierBuilder(shipment, parameters);
		}

		void CreateEvent(ForwardingShipment shipment, Event @event, string reference = "",
			string messageType = CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType,
			string documentName = FrenchPortsConstants.DocumentNames.DOSImport)
		{
			var parameters = new List<KeyValuePair<string, string>>();
			parameters.Add(new KeyValuePair<string, string>(messageType, documentName));

			if (!string.IsNullOrEmpty(reference))
			{
				parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, reference));
			}

			shipment.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, parameters.ToArray());

			Factory.Save();
			Thread.Sleep(1);
		}

		#region Validations

		public void TestValidateAddresses()
		{
			var shipment = Factory.New<ForwardingShipment>();
			CreateUnlocosAndReleaseDepots(shipment);
			var consol = CreateConsol();
			shipment.Consols.Add(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.DossierImport
			};
			var dossier = new ShipmentDossierBuilder(shipment, parameters).Build();

			dossier.SendingParty.CompanyName = ZString.Empty;
			dossier.SendingParty.Country.Code = ZString.Empty;
			dossier.SendingParty.AddressLine1 = ZString.Empty;
			dossier.Agent.CompanyName = ZString.Empty;
			dossier.Agent.Country.Code = ZString.Empty;
			dossier.Agent.AddressLine1 = ZString.Empty;

			dossier.ThirdParty.CompanyName = ZString.Empty;
			dossier.ThirdParty.Country.Code = ZString.Empty;
			dossier.ThirdParty.AddressLine1 = ZString.Empty;

			AssertHasMessageError(dossier.SendingParty.CompanyNameInfo, "Sending Party name and address is required.");
			AssertHasMessageError(dossier.Agent.CompanyNameInfo, "Agent name and address is required.");
			AssertHasMessageError(dossier.ThirdParty.CompanyNameInfo, "Third Party name and address is required.");

			dossier.SendingParty.CompanyName = "Tom";
			dossier.SendingParty.Country.Code = "CN";
			dossier.SendingParty.AddressLine1 = "Tom Address1";

			dossier.Agent.CompanyName = "Bugs";
			dossier.Agent.Country.Code = "CN";
			dossier.Agent.AddressLine1 = "Bugs Address1";

			dossier.ThirdParty.CompanyName = "Jerry";
			dossier.ThirdParty.Country.Code = "CN";
			dossier.ThirdParty.AddressLine1 = "Jerry Address1";

			AssertNoMessageError(dossier.SendingParty.CompanyNameInfo, "Sending Party name and address is required.");
			AssertNoMessageError(dossier.Agent.CompanyNameInfo, "Agent name and address is required.");
			AssertNoMessageError(dossier.ThirdParty.CompanyNameInfo, "Third Party name and address is required.");
		}

		public void TestECVReferenceValidate()
		{
			var shipment = Factory.New<ForwardingShipment>();
			CreateUnlocosAndReleaseDepots(shipment);
			var consol = CreateConsol(isImport: false);
			shipment.Consols.Add(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.DossierExport
			};
			var dossier = new ShipmentDossierBuilder(shipment, parameters).Build();

			dossier.ECVReference = ZString.Empty;
			AssertHasMessageError(dossier.ECVReferenceInfo, "ECV Ref is required when dossier's status is not imported.\r\nExport Ref Number missing from Shipment > Packing > Pack Line.");

			parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.DossierImport
			};

			dossier = new ShipmentDossierBuilder(shipment, parameters).Build();
			AssertNoMessageError(dossier.ECVReferenceInfo, "ECV Ref is required when dossier's status is not imported.\r\nExport Ref Number missing from Shipment > Packing > Pack Line.");
		}

		public void TestMissingProviderIDValidations()
		{
			var shipment = Factory.New<ForwardingShipment>();
			CreateUnlocosAndReleaseDepots(shipment);
			var consol = CreateConsol();
			shipment.Consols.Add(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.DossierImport
			};
			var dossier = new ShipmentDossierBuilder(shipment, parameters).Build();

			AssertRequireAPPlusIDValidation(dossier.FormattedSendingPartyProviderIDInfo, dossier.PCS, OrgCusCode.FranceCodeTypes.SON, "Sending Party", "Forwarder", "Org. Proxy");
			AssertRequireAPPlusIDValidation(dossier.FormattedAgentProviderIDInfo, dossier.PCS, OrgCusCode.FranceCodeTypes.SOA, "Agent", "Agent", "Org. Proxy");
			AssertRequireAPPlusIDValidation(dossier.FormattedThirdPartyProviderIDInfo, dossier.PCS, OrgCusCode.FranceCodeTypes.SON, "Third Party", "Forwarder", dossier.IsImport ? "Shipment > Delivery > Delivery Agent" : "Shipment > Pickup > Pickup Agent", extraConfigPath: dossier.IsImport ? "Consol > Receiving Agent" : "Consol > Sending Agent");
		}

		public void TestValidateFieldsWithMaxLength_Export()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = CreateConsol(isImport: false);
			shipment.Consols.Add(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.DossierExport
			};
			var dossier = new ShipmentDossierBuilder(shipment, parameters).Build();

			dossier.ThirdPartyReference = "B01234567891234567";
			dossier.Notes = new string('A', 211);

			AssertHasMessageError("17 maximum length", dossier.ThirdPartyReferenceInfo, "Shipment Number (which is used as a Reference for this message) is too long.  Maximum 17 characters are allowed.");
			AssertHasMessageError("210 maximum length", dossier.NotesInfo, "Notes field value is too long.  Maximum 210 characters allowed.");

			dossier.ThirdPartyReference = "B0123456789";
			dossier.Notes = new string('A', 210);

			AssertNoMessageError("17 maximum length", dossier.ThirdPartyReferenceInfo, "Shipment Number (which is used as a Reference for this message) is too long.  Maximum 17 characters are allowed.");
			AssertNoMessageError("210 maximum length", dossier.NotesInfo, "Notes field value is too long.  Maximum 210 characters allowed.");
		}

		public void TestValidateFieldsWithMaxLength_Import()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consol = CreateConsol();
			shipment.Consols.Add(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.DossierImport
			};
			var dossier = new ShipmentDossierBuilder(shipment, parameters).Build();

			dossier.ThirdPartyReference = "B01234567891234567";
			dossier.Notes = new string('A', 211);

			AssertHasMessageError("17 maximum length", dossier.ThirdPartyReferenceInfo, "Shipment Number (which is used as a Reference for this message) is too long.  Maximum 17 characters are allowed.");
			AssertHasMessageError("210 maximum length", dossier.NotesInfo, "Notes field value is too long.  Maximum 210 characters allowed.");

			dossier.ThirdPartyReference = "B0123456789";
			dossier.Notes = new string('A', 210);

			AssertNoMessageError("17 maximum length", dossier.ThirdPartyReferenceInfo, "Shipment Number (which is used as a Reference for this message) is too long.  Maximum 17 characters are allowed.");
			AssertNoMessageError("210 maximum length", dossier.NotesInfo, "Notes field value is too long.  Maximum 210 characters allowed.");
		}

		public void TestValidateBillOfLading()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HOUSE_111";
			var consol = CreateConsol(isImport: true);
			shipment.Consols.Add(consol);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.DossierImport
			};
			var dossier = new ShipmentDossierBuilder(shipment, parameters).Build();

			Assert(dossier.IsImport);
			AssertNoMessageError("HBL is required for import", dossier.BillOfLadingInfo, "House Bill of Lading (HBL) Number is required.");

			shipment.JS_HouseBill = string.Empty;
			dossier = new ShipmentDossierBuilder(shipment, parameters).Build();
			AssertHasMessageError("HBL is required for import", dossier.BillOfLadingInfo, "House Bill of Lading (HBL) Number is required.");

			shipment.JS_HouseBill = "HOUSE_111";
			consol = CreateConsol(isImport: false);
			parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ShipmentDocumentDataStoreNames.DossierExport
			};
			dossier = new ShipmentDossierBuilder(shipment, parameters).Build();

			Assert(!dossier.IsImport);
			AssertNoMessageError("HBL is required for import", dossier.BillOfLadingInfo, "House Bill of Lading (HBL) Number is required.");

			shipment.JS_HouseBill = string.Empty;
			dossier = new ShipmentDossierBuilder(shipment, parameters).Build();
			AssertNoMessageError("HBL is required for import", dossier.BillOfLadingInfo, "House Bill of Lading (HBL) Number is required.");
		}

		#endregion

		#region Implement

		ForwardingConsol CreateConsol(string consolNumber = "C00001000", bool isAddressAvailableToCreate = true, bool isImport = true)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = consolNumber;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			if (isImport)
			{
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "FRPRA";
			}
			else
			{
				consol.JK_RL_NKLoadPort = "FRPRA";
				consol.JK_RL_NKDischargePort = "AUSYD";
			}

			consol.JK_BookingReference = "BookingReference";
			consol.JK_MasterBillNum = "BOL";

			if (isAddressAvailableToCreate)
			{
				CreateAddresses(consol);
			}

			return consol;
		}

		ForwardingShipment CreateShipment(string shipmentNumber = "SH0001000")
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = shipmentNumber;
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.SeaWaybill;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "AUMEL";
			shipment.JS_RL_NKDischargePort = "FRMRS";
			shipment.JS_RL_NKDestination = "FRNCE";
			shipment.JS_RL_NKLoadPort = "FRPAR";
			shipment.JS_HBLContainerPackModeOverride = Constants.HBLDeliveryModes.Codes.CFS_CY;
			shipment.JS_INCO = Constants.IncoTerms.CostAndFreight;
			shipment.JS_ShippedOnBoard = "SHP";
			shipment.JS_ShippedOnBoardDate = ZDate.Today;
			shipment.JS_E_DEP = ZDate.Today.AddDays(1);
			shipment.JS_E_ARV = ZDate.Today.AddDays(5);
			shipment.JS_ConsolReference = "WhiskyTreasure";
			shipment.JS_F3_NKPackType = "PLT";
			shipment.JS_GoodsDescription = "goods description";
			shipment.DetailedGoodsDescriptionNoteText = ZString.Empty;
			shipment.JS_MarksAndNumbers = "marks & numbers";
			shipment.JS_BookingReference = "BKG000001";
			shipment.JS_UnitOfWeight = "T";
			shipment.JS_UnitOfVolume = "D3";

			shipment.OuterPackLines.RemoveAndDeleteAll();

			var packline1 = shipment.OuterPackLines.AddNew();
			packline1.JL_PackageCount = 4;
			packline1.JL_F3_NKPackType = "PLT";
			packline1.JL_ActualWeight = 400;
			packline1.JL_ActualWeightUQ = "KG";
			packline1.JL_ActualVolume = 300;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_Length = 1000;
			packline1.JL_Width = 1000;
			packline1.JL_Height = 300;
			packline1.JL_UnitOfDimension = "CM";
			packline1.JL_HarmonisedCode = "WHISKY";
			packline1.JL_RefNumber = "AMR-57";
			packline1.JL_ExportRefNumber = "ECV_1";
			packline1.JL_DetailedDescription = "Amrut Indian Peated Single Malt Detailed Description";
			packline1.JL_MarksAndNumbers = "MarksAndNumbers1";
			packline1.JL_Description = "ALCOHOLIC BEVERAGES 57%";
			packline1.JL_LastKnownTransitWarehouseStatus = "RCV";

			var ecv1 = Factory.New<CusEntryNumber>();
			ecv1.Parent = packline1;
			ecv1.CE_EntryStatus = string.Empty;
			ecv1.CE_EntryNum = "ECV_1";
			ecv1.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN;
			ecv1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			ecv1.CE_Category = "PRT";

			var erc1 = packline1.AdditionalReferenceNumbers.AddNew();
			erc1.CE_EntryStatus = string.Empty;
			erc1.CE_EntryNum = "ERC_1";
			erc1.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC;
			erc1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var packline2 = shipment.OuterPackLines.AddNew();
			packline2.JL_PackageCount = 6;
			packline2.JL_F3_NKPackType = "PLT";
			packline2.JL_ActualWeight = 600;
			packline2.JL_ActualWeightUQ = "KG";
			packline2.JL_ActualVolume = 450;
			packline2.JL_ActualVolumeUQ = "M3";
			packline2.JL_Length = 15;
			packline2.JL_Width = 10;
			packline2.JL_Height = 3;
			packline2.JL_UnitOfDimension = "M";
			packline2.JL_HarmonisedCode = "WHISKY";
			packline2.JL_RefNumber = "AMR-43";
			packline2.JL_ExportRefNumber = "ECV_2";
			packline2.JL_DetailedDescription = "Amrut Indian Chill Filter Single Malt Detailed Description";
			packline2.JL_MarksAndNumbers = "MarksAndNumbers2";
			packline2.JL_Description = "ALCOHOLIC BEVERAGES 43%";
			packline2.JL_LastKnownTransitWarehouseStatus = "RCV";

			var ecv2 = Factory.New<CusEntryNumber>();
			ecv2.Parent = packline2;
			ecv2.CE_EntryStatus = string.Empty;
			ecv2.CE_EntryNum = "ECV_2";
			ecv2.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN;
			ecv2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			ecv2.CE_Category = "PRT";

			var erc2 = packline2.AdditionalReferenceNumbers.AddNew();
			erc2.CE_EntryStatus = string.Empty;
			erc2.CE_EntryNum = "ERC_2";
			erc2.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC;
			erc2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var packline3 = shipment.OuterPackLines.AddNew();
			packline3.JL_PackageCount = 6;
			packline3.JL_F3_NKPackType = "PLT";
			packline3.JL_ActualWeight = 600;
			packline3.JL_ActualWeightUQ = "KG";
			packline3.JL_ActualVolume = 450;
			packline3.JL_ActualVolumeUQ = "M3";
			packline3.JL_Length = 15;
			packline3.JL_Width = 10;
			packline3.JL_Height = 3;
			packline3.JL_UnitOfDimension = "M";
			packline3.JL_HarmonisedCode = "WHISKY";
			packline3.JL_RefNumber = "AMR-43";
			packline3.JL_ExportRefNumber = "ECV_3";
			packline3.JL_DetailedDescription = "Amrut Indian Chill Filter Single Malt Detailed Description";
			packline3.JL_MarksAndNumbers = "MarksAndNumbers2";
			packline3.JL_Description = "ALCOHOLIC BEVERAGES 43%";
			packline3.JL_LastKnownTransitWarehouseStatus = "RCV";

			var ecv3 = Factory.New<CusEntryNumber>();
			ecv3.Parent = packline3;
			ecv3.CE_EntryStatus = string.Empty;
			ecv3.CE_EntryNum = "ECV_3";
			ecv3.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.PAN;
			ecv3.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			ecv3.CE_Category = "PRT";

			var erc3 = packline3.AdditionalReferenceNumbers.AddNew();
			erc3.CE_EntryStatus = string.Empty;
			erc3.CE_EntryNum = "ERC_3";
			erc3.CE_EntryType = ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC;
			erc3.CE_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Handsome";
			contact.OC_Phone = "1234567";

			Factory.Save();

			return shipment;
		}

		void CreateAddresses(ForwardingConsol consol)
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "DK";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var bookingAgent = Factory.New<OrgHeader>();
			bookingAgent.OH_FullName = "I'm Handling Stuff";
			bookingAgent.OH_RL_NKClosestPort = "AUSYD";
			bookingAgent.MainAddress.Address1 = "Unit 2";
			bookingAgent.MainAddress.Address2 = "60 What Lane";
			bookingAgent.MainAddress.City = "Sydney";
			bookingAgent.MainAddress.Postcode = "2023";
			bookingAgent.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address = bookingAgent.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "YUMMY";
			receivingForwarder.OH_RL_NKClosestPort = "AUSYD";
			receivingForwarder.MainAddress.Address1 = "Unit 200";
			receivingForwarder.MainAddress.Address2 = "55 Why Lane";
			receivingForwarder.MainAddress.City = "Sydney";
			receivingForwarder.MainAddress.Postcode = "2000";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "YUMMY";
			sendingForwarder.OH_RL_NKClosestPort = "AUSYD";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Sydney";
			sendingForwarder.MainAddress.Postcode = "2000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
		}

		void CreateUnlocosAndReleaseDepots(ForwardingShipment shipment)
		{
			RefUNLOCO portFRSoget = Factory.NewWithValidTestData<RefUNLOCO>();
			portFRSoget.RL_Code = "FR0SO";
			RefLocoMap mapFRSoget = portFRSoget.RefLocoMaps.AddNew();
			mapFRSoget.RY_RN = Core.Constants.CountryGuids.France;
			mapFRSoget.RY_SystemUsage = "PCS";
			mapFRSoget.RY_LocalPortCode = FrenchPortsConstants.PCS.Soget;

			var exportReceivingDepot = Factory.New<OrgHeader>();
			exportReceivingDepot.OH_FullName = "I'm the receiving release depot";
			exportReceivingDepot.MainAddress.Address1 = "Unit 100";
			exportReceivingDepot.MainAddress.Address2 = "55 Why Lane";
			exportReceivingDepot.MainAddress.City = "En-France";
			exportReceivingDepot.MainAddress.Postcode = "10000";
			exportReceivingDepot.MainAddress.OA_RN_NKCountryCode = "FR";
			exportReceivingDepot.MainAddress.OA_RL_NKRelatedPortCode = "FR0SO";

			shipment.JS_OA_ExportReceivingDepot = exportReceivingDepot.MainAddress.PK;

			RefUNLOCO portFRMGI = Factory.NewWithValidTestData<RefUNLOCO>();
			portFRMGI.RL_Code = "FR0MG";
			RefLocoMap mapFRMGI = portFRMGI.RefLocoMaps.AddNew();
			mapFRMGI.RY_RN = Core.Constants.CountryGuids.France;
			mapFRMGI.RY_SystemUsage = "PCS";
			mapFRMGI.RY_LocalPortCode = FrenchPortsConstants.PCS.MGI;

			var importReleaseDepot = Factory.New<OrgHeader>();
			importReleaseDepot.OH_FullName = "I'm the import release depot";
			importReleaseDepot.MainAddress.Address1 = "Unit 900";
			importReleaseDepot.MainAddress.Address2 = "55 Why Lane";
			importReleaseDepot.MainAddress.City = "EN-France";
			importReleaseDepot.MainAddress.Postcode = "90000";
			importReleaseDepot.MainAddress.OA_RN_NKCountryCode = "FR";
			importReleaseDepot.MainAddress.OA_RL_NKRelatedPortCode = "FR0MG";

			shipment.JS_OA_ImportReleaseDepot = importReleaseDepot.MainAddress.PK;
		}

		#endregion
	}
}
