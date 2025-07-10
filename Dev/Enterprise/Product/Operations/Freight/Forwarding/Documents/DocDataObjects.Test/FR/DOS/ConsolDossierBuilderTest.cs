using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Forwarding.Documents.Testing.AssertionHelper;
using ConsolDocumentDataStoreNames = Enterprise.Freight.Forwarding.Documents.DocDataObjects.ConsolDocumentDataStoreNames;
using RegistrationNumber = Enterprise.DocumentVisualizer.DocDataObjects.RegistrationNumber;

namespace Enterprise.Freight.Forwarding.Documents.FR.Testing
{
	sealed class ConsolDossierBuilderTest : TestCaseWithFactory
	{
		public void TestBuild_Import()
		{
			var consol = CreateConsol();

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ConsolDocumentDataStoreNames.DossierImport
			};

			var builder = new ConsolDossierBuilder(consol, parameters);
			var dossier = builder.Build();

			AssertNotNull(dossier);

			Assert("IsImport", dossier.IsImport);
			Assert("ImplicitAcknowledgement", dossier.ImplicitAcknowledgement);
			Assert("ImplicitBAET", dossier.ImplicitBAET);

			AssertEquals("PCS", FrenchPortsConstants.PCS.MGI, dossier.PCS);
			AssertEquals("OperationalPort", "FR0MG", dossier.OperationalPort.Code);

			AssertEquals("ConsolNumber", consol.JK_UniqueConsignRef, dossier.ConsolNumber);
			AssertEquals("ThirdPartyReference", consol.JK_UniqueConsignRef, dossier.ThirdPartyReference);
			AssertEquals("BillOfLading", consol.JK_MasterBillNum, dossier.BillOfLading);
			AssertEquals("CarrierBookingReference", consol.JK_BookingReference, dossier.CarrierBookingReference);
			AssertEquals("ContainerMode", Constants.ContainerModes.FCL, dossier.ContainerMode.Code);
			AssertEquals("ShipmentType", Constants.AgentType.Agent, dossier.ShipmentType.Code);

			AssertAddressData(consol.ShippingLineAddress, dossier.Carrier);
			AssertAddressData(consol.ReceivingForwarderAddress, dossier.ReceivingForwarder);
			AssertAddressData(consol.SendingForwarderAddress, dossier.SendingForwarder);
			AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, dossier.SendingParty);
		}

		public void TestBuild_Export()
		{
			var consol = CreateConsol();

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ConsolDocumentDataStoreNames.DossierExport
			};

			var builder = new ConsolDossierBuilder(consol, parameters);
			var dossier = builder.Build();

			AssertNotNull(dossier);

			Assert("IsExport", !dossier.IsImport);
			Assert("ImplicitAcknowledgement", dossier.ImplicitAcknowledgement);
			Assert("ImplicitBAET", dossier.ImplicitBAET);

			AssertEquals("PCS", FrenchPortsConstants.PCS.Soget, dossier.PCS);
			AssertEquals("OperationalPort", "FR0SO", dossier.OperationalPort.Code);

			AssertEquals("ConsolNumber", consol.JK_UniqueConsignRef, dossier.ConsolNumber);
			AssertEquals("ThirdPartyReference", consol.JK_UniqueConsignRef, dossier.ThirdPartyReference);
			AssertEquals("BillOfLading", consol.JK_MasterBillNum, dossier.BillOfLading);
			AssertEquals("CarrierBookingReference", consol.JK_BookingReference, dossier.CarrierBookingReference);
			AssertEquals("ContainerMode", Constants.ContainerModes.FCL, dossier.ContainerMode.Code);
			AssertEquals("ShipmentType", Constants.AgentType.Agent, dossier.ShipmentType.Code);

			AssertAddressData(consol.ShippingLineAddress, dossier.Carrier);
			AssertAddressData(consol.ReceivingForwarderAddress, dossier.ReceivingForwarder);
			AssertAddressData(consol.SendingForwarderAddress, dossier.SendingForwarder);
			AssertAddressData(GlbBranch.CurrentBranch.OrgProxy.MainAddress, dossier.SendingParty);
		}

		public void TestPCSAndOperationalPortWithFallback_Import()
		{
			var consol = CreateConsol();

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ConsolDocumentDataStoreNames.DossierImport
			};

			var builder = new ConsolDossierBuilder(consol, parameters);
			var dossier = builder.Build();
			AssertEquals("IsImport", true, dossier.IsImport);
			AssertEquals("PCS", FrenchPortsConstants.PCS.MGI, dossier.PCS);
			AssertEquals("OperationalPort", "FR0MG", dossier.OperationalPort.Code);

			consol.ArrivalCTOAddress.Delete();
			dossier = builder.Build();
			AssertEquals("PCS", ZString.Empty, dossier.PCS);
			AssertHasMessageError("PCS Error", dossier.PCSInfo, "PCS Code is required. (Maintain > Locations > UNLOCO of Operational Port > Local Codes - Usage PCS)");
			AssertEquals("OperationalPort", "FRPRA", dossier.OperationalPort.Code);

			var lastSeaLeg = consol.Transports.OfType<Freight.Business.Transport>().Where(t => t.TransportMode == Core.Constants.TransportModes.Sea).ToArray().LastOrDefault();
			lastSeaLeg.JW_RL_NKDiscPort = string.Empty;
			dossier = builder.Build();
			AssertEquals("PCS", ZString.Empty, dossier.PCS);
			AssertHasMessageError("PCS Error", dossier.PCSInfo, "PCS Code is required. (Maintain > Locations > UNLOCO of Operational Port > Local Codes - Usage PCS)");
			AssertEquals("OperationalPort", ZString.Empty, dossier.OperationalPort.Code);
			AssertHasMessageError("OperationalPort Error", ((Unloco)dossier.OperationalPort)?.CodeInfo, "Operational Port is required, UNLOCO missing from Consol > Arrival > CTO Address or Discharge Port of the last SEA leg.");
		}

		public void TestPCSAndOperationalPortWithFallback_Export()
		{
			var consol = CreateConsol();

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ConsolDocumentDataStoreNames.DossierExport
			};

			var builder = new ConsolDossierBuilder(consol, parameters);
			var dossier = builder.Build();
			AssertEquals("IsImport", false, dossier.IsImport);
			AssertEquals("PCS", FrenchPortsConstants.PCS.Soget, dossier.PCS);
			AssertEquals("OperationalPort", "FR0SO", dossier.OperationalPort.Code);

			consol.DepartureCTOAddress.Delete();
			dossier = builder.Build();
			AssertEquals("PCS", ZString.Empty, dossier.PCS);
			AssertHasMessageError("PCS Error", dossier.PCSInfo, "PCS Code is required. (Maintain > Locations > UNLOCO of Operational Port > Local Codes - Usage PCS)");
			AssertEquals("OperationalPort", "AUSYD", dossier.OperationalPort.Code);

			var firstSeaLeg = consol.Transports.OfType<Freight.Business.Transport>().Where(t => t.TransportMode == Core.Constants.TransportModes.Sea).ToArray().FirstOrDefault();
			firstSeaLeg.JW_RL_NKLoadPort = string.Empty;
			dossier = builder.Build();
			AssertEquals("PCS", ZString.Empty, dossier.PCS);
			AssertHasMessageError("PCS Error", dossier.PCSInfo, "PCS Code is required. (Maintain > Locations > UNLOCO of Operational Port > Local Codes - Usage PCS)");
			AssertEquals("OperationalPort", ZString.Empty, dossier.OperationalPort.Code);
			AssertHasMessageError("OperationalPort Error", ((Unloco)dossier.OperationalPort)?.CodeInfo, "Operational Port is required, UNLOCO missing from Consol > Departure > CTO Address or Load Port of the first SEA leg.");
		}

		public void TestPopulateSendingPartyImport()
		{
			var branchProxy = Factory.NewWithValidTestData<OrgHeader>();
			branchProxy.MainAddress.OA_RN_NKCountryCode = GlbBranch.CurrentBranch.BaseCountry?.Code ?? ZString.Empty;
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchProxy.PK;

			Factory.Save();

			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ConsolDocumentDataStoreNames.DossierImport
			};

			var builder = new ConsolDossierBuilder(consol, parameters);
			var dossier = builder.Build();

			AssertAddressData(branchProxy.MainAddress, dossier.SendingParty);

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			dossier = builder.Build();

			AssertAddressData(GlbCompany.CurrentCompany.OrgProxy.MainAddress, dossier.SendingParty);
		}

		public void TestPopulateSendingPartyExport()
		{
			var branchProxy = Factory.NewWithValidTestData<OrgHeader>();
			branchProxy.MainAddress.OA_RN_NKCountryCode = GlbBranch.CurrentBranch.BaseCountry?.Code ?? ZString.Empty;
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchProxy.PK;

			Factory.Save();

			var consol = CreateConsol(isImport: false);
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ConsolDocumentDataStoreNames.DossierExport
			};

			var builder = new ConsolDossierBuilder(consol, parameters);
			var dossier = builder.Build();

			AssertAddressData(branchProxy.MainAddress, dossier.SendingParty);

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			dossier = builder.Build();

			AssertAddressData(GlbCompany.CurrentCompany.OrgProxy.MainAddress, dossier.SendingParty);
		}

		public void TestPopulateThirdPartyImport()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ConsolDocumentDataStoreNames.DossierImport
			};

			var builder = new ConsolDossierBuilder(consol, parameters);
			var dossier = builder.Build();

			AssertNotNull(dossier);
			AssertAddressData(consol.ReceivingForwarderAddress, dossier.ThirdParty);
			AssertEquals("ThirdPartySON", dossier.ReceivingForwarderSON.Value, dossier.ThirdPartySON.Value);
			AssertEquals("ThirdPartyCI5", dossier.ReceivingForwarderCI5.Value, dossier.ThirdPartyCI5.Value);

			AssertPopulateAPPlusCodesUseRegistry(true, "ThirdPartyCI5", "ThirdPartySON", ConsolDocumentDataStoreNames.DossierImport);
		}

		public void TestPopulateThirdPartyExport()
		{
			var consol = CreateConsol(isImport: false);
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ConsolDocumentDataStoreNames.DossierExport
			};

			var builder = new ConsolDossierBuilder(consol, parameters);
			var dossier = builder.Build();

			AssertNotNull(dossier);
			AssertAddressData(consol.SendingForwarderAddress, dossier.ThirdParty);
			AssertEquals("ThirdPartySON", dossier.SendingForwarderSON.Value, dossier.ThirdPartySON.Value);
			AssertEquals("ThirdPartyCI5", dossier.SendingForwarderCI5.Value, dossier.ThirdPartyCI5.Value);

			AssertPopulateAPPlusCodesUseRegistry(false, "ThirdPartyCI5", "ThirdPartySON", ConsolDocumentDataStoreNames.DossierExport);
		}

		void AssertPopulateAPPlusCodesUseRegistry(bool isImport, string ci5PropertyName, string sPropertyName, string exportOrImport)
		{
			var consol = CreateConsol(isImport: isImport);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = exportOrImport
			};

			var builder = new ConsolDossierBuilder(consol, parameters);

			var operationalAddress = Factory.NewWithValidTestData<OrgAddress>();

			if (exportOrImport == ConsolDocumentDataStoreNames.DossierImport)
			{
				consol.JK_OA_ArrivalCTOAddress = operationalAddress.PK;
			}
			else
			{
				consol.JK_OA_DepartureCTOAddress = operationalAddress.PK;
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
			code.Port = unloco.Code;
			code.PCS = FrenchPortSystemCodeList.Codes.MGI;

			using (Freight.Business.PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCodes))
			{
				AssertEquals($"{ci5PropertyName} should be from registry", "FORWARDER", ((RegistrationNumber)builder.Build()[ci5PropertyName]).Value);
			}

			code.PCS = FrenchPortSystemCodeList.Codes.SOGET;
			using (Freight.Business.PortMessagingRegistry.Instance.CommunitySystemCodesOfForwarderAndAgent.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryCodes))
			{
				AssertEquals($"{sPropertyName} should be from registry", "FORWARDER", ((RegistrationNumber)builder.Build()[sPropertyName]).Value);
			}
		}

		public void TestPopulateAPPlusCodesImport()
		{
			var consol = CreateConsol();

			CreateAddresses(consol);

			AssertPopulateAPPlusCodes(consol, consol.ShippingLineAddress, "CarrierSON", "CarrierCI5",
				ConsolDocumentDataStoreNames.DossierImport);
			AssertPopulateAPPlusCodes(consol, consol.ReceivingForwarderAddress, "ReceivingForwarderSON",
				"ReceivingForwarderCI5", ConsolDocumentDataStoreNames.DossierImport);
			AssertPopulateAPPlusCodes(consol, consol.SendingForwarderAddress, "SendingForwarderSON",
				"SendingForwarderCI5", ConsolDocumentDataStoreNames.DossierImport);
			AssertPopulateAPPlusCodes(consol, GlbBranch.CurrentBranch.OrgProxy.MainAddress, "SendingPartySON",
				"SendingPartyCI5", ConsolDocumentDataStoreNames.DossierImport);

			AssertPopulateAPPlusCodesUseRegistry(true, "SendingPartyCI5", "SendingPartySON", ConsolDocumentDataStoreNames.DossierImport);
		}

		public void TestPopulateAPPlusCodesExport()
		{
			var consol = CreateConsol(isImport: false);

			CreateAddresses(consol);

			AssertPopulateAPPlusCodes(consol, consol.ShippingLineAddress, "CarrierSON", "CarrierCI5",
				ConsolDocumentDataStoreNames.DossierExport);
			AssertPopulateAPPlusCodes(consol, consol.ReceivingForwarderAddress, "ReceivingForwarderSON",
				"ReceivingForwarderCI5", ConsolDocumentDataStoreNames.DossierExport);
			AssertPopulateAPPlusCodes(consol, consol.SendingForwarderAddress, "SendingForwarderSON",
				"SendingForwarderCI5", ConsolDocumentDataStoreNames.DossierExport);
			AssertPopulateAPPlusCodes(consol, GlbBranch.CurrentBranch.OrgProxy.MainAddress, "SendingPartySON",
				"SendingPartyCI5", ConsolDocumentDataStoreNames.DossierExport);

			AssertPopulateAPPlusCodesUseRegistry(false, "SendingPartyCI5", "SendingPartySON", ConsolDocumentDataStoreNames.DossierImport);
		}

		void AssertPopulateAPPlusCodes(ForwardingConsol consol, OrgAddress address, string sonPropertyName, string ci5PropertyName, string exportOrImport)
		{
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = exportOrImport
			};

			var sonCode1 = address.Header.CustomsCodes.AddNew();
			sonCode1.OK_CodeType = OrgCusCode.FranceCodeTypes.SON;
			sonCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			sonCode1.OK_CustomsRegNo = "S001";

			var ci5Code1 = address.Header.CustomsCodes.AddNew();
			ci5Code1.OK_CodeType = OrgCusCode.FranceCodeTypes.CI5;
			ci5Code1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			ci5Code1.OK_CustomsRegNo = "C001";

			var builder = new ConsolDossierBuilder(consol, parameters);
			var dossier = builder.Build();

			AssertEquals($"{sonPropertyName} should be from org", "S001", ((RegistrationNumber)dossier[sonPropertyName]).Value);
			AssertEquals($"{ci5PropertyName} should be from org", "C001", ((RegistrationNumber)dossier[ci5PropertyName]).Value);

			var sonCode2 = address.CustomsCodes.AddNew();
			sonCode2.OK_CodeType = OrgCusCode.FranceCodeTypes.SON;
			sonCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			sonCode2.OK_CustomsRegNo = "S002";

			var ci5Code2 = address.CustomsCodes.AddNew();
			ci5Code2.OK_CodeType = OrgCusCode.FranceCodeTypes.CI5;
			ci5Code2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Reunion;
			ci5Code2.OK_CustomsRegNo = "C002";

			dossier = builder.Build();

			AssertEquals($"{sonPropertyName} should be from address", "S002", ((RegistrationNumber)dossier[sonPropertyName]).Value);
			AssertEquals($"{ci5PropertyName} should be from address", "C002", ((RegistrationNumber)dossier[ci5PropertyName]).Value);

			address.Header.CustomsCodes.RemoveAndDeleteAll();
			address.CustomsCodes.DeleteAll();

			dossier = builder.Build();

			AssertEquals(string.Empty, ((RegistrationNumber)dossier[sonPropertyName]).Value);
			AssertEquals(string.Empty, ((RegistrationNumber)dossier[ci5PropertyName]).Value);
		}

		public void TestPopulateConfirmationReference()
		{
			var consol = CreateConsol(isImport: true);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ConsolDocumentDataStoreNames.DossierImport
			};
			CreateEvent(consol, AutoEvents.MessageSent);

			var builder = new ConsolDossierBuilder(consol, parameters);
			var dossier = builder.Build();

			AssertEquals("MAA event hasn't been received", string.Empty, dossier.ConfirmationReference);

			CreateEvent(consol, AutoEvents.MessageAccepted, "X00001");
			dossier = builder.Build();

			AssertEquals("Populated from RFN parameter of MAA event", "X00001", dossier.ConfirmationReference);

			CreateEvent(consol, AutoEvents.MessageWithdrawCancelRequest);
			CreateEvent(consol, AutoEvents.MessageWithdrawCancelAccepted);
			dossier = builder.Build();

			AssertEquals("Return empty as withdraw is finished", string.Empty, dossier.ConfirmationReference);
		}

		void CreateEvent(ForwardingConsol consol, Event @event, string reference = "")
		{
			var parameters = new List<KeyValuePair<string, string>>();
			parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType, FrenchPortsConstants.DocumentNames.DOSImport));

			if (!string.IsNullOrEmpty(reference))
			{
				parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, reference));
			}

			consol.Logs.CreateOrRecreateEventLog(@event, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, parameters.ToArray());

			Factory.Save();
			Thread.Sleep(1);
		}

		public void TestValidateBillOfLading()
		{
			var consol = CreateConsol();

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ConsolDocumentDataStoreNames.DossierImport
			};
			var dossier = new ConsolDossierBuilder(consol, parameters).Build();

			dossier.BillOfLading = "B0001";
			Assert(dossier.IsImport);
			AssertNoMessageError("BOL is required for import", dossier.BillOfLadingInfo, "Bill of Lading (BOL) Number is required.");

			dossier.BillOfLading = string.Empty;
			AssertHasMessageError("BOL is required for import", dossier.BillOfLadingInfo, "Bill of Lading (BOL) Number is required.");

			consol = CreateConsol(isImport: false);
			parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ConsolDocumentDataStoreNames.DossierExport
			};
			dossier = new ConsolDossierBuilder(consol, parameters).Build();

			dossier.BillOfLading = string.Empty;
			Assert(!dossier.IsImport);
			AssertNoMessageError("BOL is only required for import", dossier.BillOfLadingInfo, "Bill of Lading (BOL) Number is required.");
		}

		public void TestValidateCarrierBookingReference()
		{
			var consol = CreateConsol(isImport: false);

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ConsolDocumentDataStoreNames.DossierExport
			};
			var dossier = new ConsolDossierBuilder(consol, parameters).Build();

			dossier.CarrierBookingReference = "B01234567891234567";
			Assert(!dossier.IsImport);
			AssertNoMessageError("Carrier Booking Reference is required for export", dossier.CarrierBookingReferenceInfo, "Carrier Booking Reference is required.");
			AssertHasMessageError("17 maximum length", dossier.CarrierBookingReferenceInfo, "Carrier Booking Reference is too long. Maximum 17 characters allowed.");

			dossier.CarrierBookingReference = "B0123456789";
			Assert(!dossier.IsImport);
			AssertNoMessageError("Carrier Booking Reference is required for export", dossier.CarrierBookingReferenceInfo, "Carrier Booking Reference is required.");

			dossier.CarrierBookingReference = string.Empty;
			AssertHasMessageError("Carrier Booking Reference is required for export", dossier.CarrierBookingReferenceInfo, "Carrier Booking Reference is required.");

			consol = CreateConsol();
			parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ConsolDocumentDataStoreNames.DossierImport
			};
			dossier = new ConsolDossierBuilder(consol, parameters).Build();

			dossier.BillOfLading = string.Empty;
			Assert(dossier.IsImport);
			AssertNoMessageError("Carrier Booking Reference is only required for export", dossier.BillOfLadingInfo, "Carrier Booking Reference is required.");
		}

		public void TestValidateAddresses()
		{
			var consol = CreateConsol();

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ConsolDocumentDataStoreNames.DossierImport
			};
			var dossier = new ConsolDossierBuilder(consol, parameters).Build();

			dossier.Carrier.CompanyName = ZString.Empty;
			dossier.Carrier.Country.Code = ZString.Empty;
			dossier.Carrier.AddressLine1 = ZString.Empty;

			dossier.SendingParty.CompanyName = ZString.Empty;
			dossier.SendingParty.Country.Code = ZString.Empty;
			dossier.SendingParty.AddressLine1 = ZString.Empty;

			dossier.ThirdParty.CompanyName = ZString.Empty;
			dossier.ThirdParty.Country.Code = ZString.Empty;
			dossier.ThirdParty.AddressLine1 = ZString.Empty;

			AssertHasMessageError(dossier.Carrier.CompanyNameInfo, "Carrier name and address is required.");
			AssertHasMessageError(dossier.SendingParty.CompanyNameInfo, "Sending Party name and address is required.");
			AssertHasMessageError(dossier.ThirdParty.CompanyNameInfo, "Third Party name and address is required.");

			dossier.Carrier.CompanyName = "Nier";
			dossier.Carrier.Country.Code = "CN";
			dossier.Carrier.AddressLine1 = "Nier Address1";

			dossier.SendingParty.CompanyName = "Tom";
			dossier.SendingParty.Country.Code = "CN";
			dossier.SendingParty.AddressLine1 = "Tom Address1";

			dossier.ThirdParty.CompanyName = "Jerry";
			dossier.ThirdParty.Country.Code = "CN";
			dossier.ThirdParty.AddressLine1 = "Jerry Address1";

			AssertNoMessageError(dossier.Carrier.CompanyNameInfo, "Carrier name and address is required.");
			AssertNoMessageError(dossier.SendingParty.CompanyNameInfo, "Sending Party name and address is required.");
			AssertNoMessageError(dossier.ThirdParty.CompanyNameInfo, "Third Party name and address is required.");
		}

		public void TestMissingProviderIDValidations()
		{
			var consol = CreateConsol();
			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ConsolDocumentDataStoreNames.DossierImport
			};
			var dossier = new ConsolDossierBuilder(consol, parameters).Build();

			AssertRequireAPPlusIDValidation(dossier.FormattedCarrierProviderIDInfo, dossier.PCS, OrgCusCode.FranceCodeTypes.SON, "Carrier", configPath: "Consol > Carrier", isOrganization: true);
			AssertRequireAPPlusIDValidation(dossier.FormattedSendingPartyProviderIDInfo, dossier.PCS, OrgCusCode.FranceCodeTypes.SON, "Sending Party", "Forwarder", "Org. Proxy");
			AssertRequireAPPlusIDValidation(dossier.FormattedThirdPartyProviderIDInfo, dossier.PCS, OrgCusCode.FranceCodeTypes.SON, "Third Party", "Forwarder", dossier.IsImport ? "Consol > Receiving Agent" : "Consol > Sending Agent");
		}

		public void TestValidateFieldsWithMaxLength()
		{
			var consol = CreateConsol();

			var parameters = new DummyDocDataObjectParameters
			{
				DataStoreName = ConsolDocumentDataStoreNames.DossierExport
			};
			var dossier = new ConsolDossierBuilder(consol, parameters).Build();

			dossier.ThirdPartyReference = "B01234567891234567";
			dossier.CarrierBookingReference = "B01234567891234567";
			dossier.Notes = new string('A', 211);

			AssertHasMessageError("17 maximum length", dossier.ThirdPartyReferenceInfo, "Consol Number (which is used as Reference for this message) is too long.  Maximum 17 characters are allowed.");
			AssertHasMessageError("17 maximum length", dossier.CarrierBookingReferenceInfo, "Carrier Booking Reference is too long. Maximum 17 characters allowed.");
			AssertHasMessageError("210 maximum length", dossier.NotesInfo, "Notes field value is too long.  Maximum 210 characters allowed.");

			dossier.ThirdPartyReference = "B0123456789";
			dossier.CarrierBookingReference = "B0123456789";
			dossier.Notes = new string('A', 210);

			AssertNoMessageError("17 maximum length", dossier.ThirdPartyReferenceInfo, "Consol Number (which is used as Reference for this message) is too long.  Maximum 17 characters are allowed.");
			AssertNoMessageError("17 maximum length", dossier.CarrierBookingReferenceInfo, "Carrier Booking Reference is too long. Maximum 17 characters allowed.");
			AssertNoMessageError("210 maximum length", dossier.NotesInfo, "Notes field value is too long.  Maximum 210 characters allowed.");
		}

		#region Implement

		ForwardingConsol CreateConsol(bool isAddressAvailableToCreate = true, bool isImport = true)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			RefUNLOCO portFRMGI = Factory.NewWithValidTestData<RefUNLOCO>();
			portFRMGI.RL_Code = "FR0MG";
			RefLocoMap mapFRMGI = portFRMGI.RefLocoMaps.AddNew();
			mapFRMGI.RY_RN = Core.Constants.CountryGuids.France;
			mapFRMGI.RY_SystemUsage = "PCS";
			mapFRMGI.RY_LocalPortCode = FrenchPortsConstants.PCS.MGI;

			RefUNLOCO portFRSoget = Factory.NewWithValidTestData<RefUNLOCO>();
			portFRSoget.RL_Code = "FR0SO";
			RefLocoMap mapFRSoget = portFRSoget.RefLocoMaps.AddNew();
			mapFRSoget.RY_RN = Core.Constants.CountryGuids.France;
			mapFRSoget.RY_SystemUsage = "PCS";
			mapFRSoget.RY_LocalPortCode = FrenchPortsConstants.PCS.Soget;

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
			consol.JK_MasterBillNum = "BOL_Reference";

			if (isAddressAvailableToCreate)
			{
				CreateAddresses(consol);
			}

			return consol;
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

			var departureCTOAddress = Factory.New<OrgHeader>();
			departureCTOAddress.OH_FullName = "I'm Handling the Stuff to be send";
			departureCTOAddress.MainAddress.OA_RL_NKRelatedPortCode = "FR0SO";
			departureCTOAddress.MainAddress.Address1 = "Unit 100";
			departureCTOAddress.MainAddress.Address2 = "55 Why Lane";
			departureCTOAddress.MainAddress.City = "En-France";
			departureCTOAddress.MainAddress.Postcode = "10000";
			departureCTOAddress.MainAddress.OA_RN_NKCountryCode = "FR";

			consol.JK_OA_DepartureCTOAddress = departureCTOAddress.MainAddress.PK;

			var arrivalCTOAddress = Factory.New<OrgHeader>();
			arrivalCTOAddress.OH_FullName = "I'm Handling the Stuff to be received";
			arrivalCTOAddress.MainAddress.OA_RL_NKRelatedPortCode = "FR0MG";
			arrivalCTOAddress.MainAddress.Address1 = "Unit 900";
			arrivalCTOAddress.MainAddress.Address2 = "55 Why Lane";
			arrivalCTOAddress.MainAddress.City = "EN-France";
			arrivalCTOAddress.MainAddress.Postcode = "90000";
			arrivalCTOAddress.MainAddress.OA_RN_NKCountryCode = "FR";

			consol.JK_OA_ArrivalCTOAddress = arrivalCTOAddress.MainAddress.PK;
		}

		#endregion
	}
}
