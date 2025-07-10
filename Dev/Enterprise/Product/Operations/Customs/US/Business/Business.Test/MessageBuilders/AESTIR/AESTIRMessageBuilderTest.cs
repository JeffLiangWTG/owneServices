using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business.AES;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Input;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class AESTIRMessageBuilderTest : MessageBuilderTestCase
	{
		public void TestEndToEnd()
		{
			JobDeclaration declaration = GetAlreadySetupDeclaration();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			Factory.Save();
			declaration.DoMerge();
			Factory.Save();

			AssertEquals(1, declaration.ActiveEntryHeaders.Count);
			AESTIRMessageBuilder builder = new AESTIRMessageBuilder(declaration.ActiveEntryHeaders[0], UpdateActionCode.Add);
			MQEDIMessage message = builder.PopulateMessage();
			AssertEquals(ApplicationIdentifierCodeList.AES.CommodityShipment, message.EM_MessageType);
			List<MessageBlock> messageBlocks = message.MessageBlock.MessageBlocks;
			AssertEquals(22, messageBlocks.Count);
			AssertSC1Record((AESCommShipSC1XP)messageBlocks[0], "A", "B00001000");
			AssertSC2Record((AESCommShipSC2XP)messageBlocks[1]);

			AssertSC3Record((AESCommShipSC3XP)messageBlocks[2], "", "", "TRNREF23423");
			AssertSC3Record((AESCommShipSC3XP)messageBlocks[3], "CRUX432890", "SL23", "");

			AssertEquals(1, declaration.Invoices.Count);
			JobComInvoiceHeader invoice = declaration.Invoices[0];
			AssertAESParty((AESCommShipN01XP)messageBlocks[4], (AESCommShipN02XP)messageBlocks[5], (AESCommShipN03XP)messageBlocks[6], AESTIRPartyTypeList.Codes.USPPI, invoice.Supplier, "12659768700", AESConstants.IDTypes.EmployerIdentificationNumber, "", "PICKUP CITY", "PI", Core.Constants.CountryCodes.UnitedStates, declaration.SupplierPickupAddress.Organisation);
			AssertAESParty((AESCommShipN01XP)messageBlocks[7], (AESCommShipN02XP)messageBlocks[8], (AESCommShipN03XP)messageBlocks[9], AESTIRPartyTypeList.Codes.ForwardingAgent, declaration.Forwarder, "63265985468", AESConstants.IDTypes.DUNS, "", "CITY", Core.Constants.CountryCodes.PuertoRico, Core.Constants.CountryCodes.UnitedStates);
			AssertAESParty((AESCommShipN01XP)messageBlocks[10], (AESCommShipN02XP)messageBlocks[11], (AESCommShipN03XP)messageBlocks[12], AESTIRPartyTypeList.Codes.UltimateConsignee, invoice.Importer, "96532144875", AESConstants.IDTypes.DUNS, "N", "CITY", "", Core.Constants.CountryCodes.Australia);
			AssertAESParty((AESCommShipN01XP)messageBlocks[13], (AESCommShipN02XP)messageBlocks[14], (AESCommShipN03XP)messageBlocks[15], AESTIRPartyTypeList.Codes.IntermediateConsignee, invoice.Consignee, "32656946980", AESConstants.IDTypes.DUNS, "", "CITY", "", Core.Constants.CountryCodes.Australia);

			AssertEquals(2, invoice.JobComInvoiceLines.Count);
			AssertCommodityLine(invoice.JobComInvoiceLines[0], (AESCommShipCL1XP)messageBlocks[16], (AESCommShipCL2XP)messageBlocks[17], (AESCommShipODTXP)messageBlocks[18], null);
			AssertCommodityLine(invoice.JobComInvoiceLines[1], (AESCommShipCL1XP)messageBlocks[19], (AESCommShipCL2XP)messageBlocks[20], null, (AESCommShipEV1XP)messageBlocks[21]);

			declaration.US_SoldEnRouteIndicator = YesNoDefaultList.Codes.Yes;
			declaration.DoMerge();
			Factory.Save();
			builder = new AESTIRMessageBuilder(declaration.ActiveEntryHeaders[0], UpdateActionCode.Replace);
			message = builder.PopulateMessage();
			AssertEquals(ApplicationIdentifierCodeList.AES.CommodityShipment, message.EM_MessageType);
			messageBlocks = message.MessageBlock.MessageBlocks;
			AssertEquals(22, messageBlocks.Count);
			AssertSC1Record((AESCommShipSC1XP)messageBlocks[0], "R", "B00001000");
			AssertSC2Record((AESCommShipSC2XP)messageBlocks[1]);

			AssertSC3Record((AESCommShipSC3XP)messageBlocks[2], "", "", "TRNREF23423");
			AssertSC3Record((AESCommShipSC3XP)messageBlocks[3], "CRUX432890", "SL23", "");

			AssertAESParty((AESCommShipN01XP)messageBlocks[4], (AESCommShipN02XP)messageBlocks[5], (AESCommShipN03XP)messageBlocks[6], AESTIRPartyTypeList.Codes.USPPI, declaration.Supplier, "12659768700", AESConstants.IDTypes.EmployerIdentificationNumber, "", "PICKUP CITY", "PI", Core.Constants.CountryCodes.UnitedStates, declaration.SupplierPickupAddress.Organisation);
			AssertAESParty((AESCommShipN01XP)messageBlocks[7], (AESCommShipN02XP)messageBlocks[8], (AESCommShipN03XP)messageBlocks[9], AESTIRPartyTypeList.Codes.ForwardingAgent, declaration.Forwarder, "63265985468", AESConstants.IDTypes.DUNS, "", "CITY", Core.Constants.CountryCodes.PuertoRico, Core.Constants.CountryCodes.UnitedStates);
			AssertAESParty((AESCommShipN01XP)messageBlocks[10], (AESCommShipN02XP)messageBlocks[11], (AESCommShipN03XP)messageBlocks[12], AESTIRPartyTypeList.Codes.UltimateConsignee, null, "", "", "Y", "SYDNEY", "", Core.Constants.CountryCodes.Australia);
			AssertAESParty((AESCommShipN01XP)messageBlocks[13], (AESCommShipN02XP)messageBlocks[14], (AESCommShipN03XP)messageBlocks[15], AESTIRPartyTypeList.Codes.IntermediateConsignee, declaration.IntermConsignee, "32656946980", AESConstants.IDTypes.DUNS, "", "CITY", "", Core.Constants.CountryCodes.Australia);

			AssertCommodityLine(invoice.JobComInvoiceLines[0], (AESCommShipCL1XP)messageBlocks[16], (AESCommShipCL2XP)messageBlocks[17], (AESCommShipODTXP)messageBlocks[18], null);
			AssertCommodityLine(invoice.JobComInvoiceLines[1], (AESCommShipCL1XP)messageBlocks[19], (AESCommShipCL2XP)messageBlocks[20], null, (AESCommShipEV1XP)messageBlocks[21]);

			invoice = declaration.Invoices[0];
			var usppi = invoice.USPPIDocAddress;
			usppi.E2_AddressOverride = true;
			usppi.E2_CompanyName = "USPPI OVERRIDE CO.";
			usppi.E2_Contact = "TEST CONTACT NAME";
			usppi.E2_Mobile = "+1 201-659-8745";
			usppi.E2_GovRegNumType = "EIN";
			usppi.E2_GovRegNum = "12-123659700";

			var pickupAddress = invoice.SupplierPickupAddress;
			pickupAddress.E2_AddressOverride = true;
			pickupAddress.E2_Address1 = "TEST ADDRESS 1";
			pickupAddress.E2_Address2 = "TEST ADDRESS 2";
			pickupAddress.E2_Postcode = "201100";
			pickupAddress.E2_RN_NKCountryCode = "CA";
			pickupAddress.E2_City = "TEST CITY";

			declaration.DoMerge();
			Factory.Save();
			builder = new AESTIRMessageBuilder(declaration.ActiveEntryHeaders[0], UpdateActionCode.Replace);
			message = builder.PopulateMessage();
			messageBlocks = message.MessageBlock.MessageBlocks;

			var n01xp = (AESCommShipN01XP)messageBlocks[4];
			var n02xp = (AESCommShipN02XP)messageBlocks[5];
			var n03xp = (AESCommShipN03XP)messageBlocks[6];

			AssertEquals("PartyType", AESTIRPartyTypeList.Codes.USPPI, n01xp.PartyType);
			AssertEquals("PartyID", "12123659700", n01xp.PartyID);
			AssertEquals("PartyIDType", AESConstants.IDTypes.EmployerIdentificationNumber, n01xp.PartyIDType);
			AssertEquals("ContactFirstName", "TEST", n01xp.ContactFirstName);
			AssertEquals("ContactLastName", "NAME", n01xp.ContactLastName);

			AssertEquals("AddressLine1", "TEST ADDRESS 1", n02xp.AddressLine1);
			AssertEquals("AddressLine2", "TEST ADDRESS 2", n02xp.AddressLine2);
			AssertEquals("PostalCode", "201100", n03xp.PostalCode);
			AssertEquals("PartyName", "USPPI OVERRIDE CO.", n01xp.PartyName);

			AssertEquals("City", "TEST CITY", n03xp.City);
			AssertEquals("CountryCode", "CA", n03xp.CountryCode);
		}

		public void TestUSPPIPickupAddress()
		{
			JobDeclaration declaration = GetAlreadySetupDeclaration();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			Factory.Save();
			declaration.DoMerge();
			Factory.Save();
			var pickupDocAddress = declaration.SupplierPickupAddress;
			var pickupOrgAddress = pickupDocAddress.Organisation.MainAddress;

			var org = declaration.Supplier;
			declaration.SupplierPickupAddress.E2_AddressOverride = true;
			declaration.DoMerge();
			Factory.Save();
			var builder = new AESTIRMessageBuilder(declaration.ActiveEntryHeaders[0], UpdateActionCode.Replace);
			var message = builder.PopulateMessage();
			var messageBlocks = message.MessageBlock.MessageBlocks;
			AssertEquals(22, messageBlocks.Count);
			var n01xp = (AESCommShipN01XP)messageBlocks[4];
			var n02xp = (AESCommShipN02XP)messageBlocks[5];
			var n03xp = (AESCommShipN03XP)messageBlocks[6];

			AssertEquals("PartyName", org.OH_FullName, n01xp.PartyName);
			ZString[] partContactNames = org.Contacts[0].OC_ContactName.Split(' ');
			AssertEquals(2, partContactNames.Length);
			AssertEquals("ContactFirstName", partContactNames[0], n01xp.ContactFirstName);
			AssertEquals("ContactLastName", partContactNames[1], n01xp.ContactLastName);

			AssertEquals("AddressLine1", pickupOrgAddress.OA_Address1.ToUpper(), n02xp.AddressLine1);
			AssertEquals("AddressLine2", pickupOrgAddress.OA_Address2.ToUpper(), n02xp.AddressLine2);
			AssertEquals("ContactPhoneNumber", org.Contacts[0].OC_Phone, n02xp.ContactPhoneNumber);
			AssertEquals("PostalCode", pickupOrgAddress.OA_PostCode.ToUpper(), n03xp.PostalCode);

			AssertEquals("City", pickupOrgAddress.OA_City, n03xp.City);
			AssertEquals("StateCode", pickupOrgAddress.OA_State.Left(2), n03xp.StateCode);
			AssertEquals("CountryCode", pickupOrgAddress.OA_RN_NKCountryCode, n03xp.CountryCode);
			AssertEquals("USPPIIRSIDType", "", n03xp.USPPIIRSIDType);
			AssertEquals("USPPIIRSNumber", "", n03xp.USPPIIRSNumber);

			declaration.SupplierPickupAddress.E2_AddressOverride = true;
			declaration.DoMerge();
			Factory.Save();
			builder = new AESTIRMessageBuilder(declaration.ActiveEntryHeaders[0], UpdateActionCode.Replace);
			message = builder.PopulateMessage();
			messageBlocks = message.MessageBlock.MessageBlocks;
			AssertEquals(22, messageBlocks.Count);
			n01xp = (AESCommShipN01XP)messageBlocks[4];
			n02xp = (AESCommShipN02XP)messageBlocks[5];
			n03xp = (AESCommShipN03XP)messageBlocks[6];

			AssertEquals("PartyName", org.OH_FullName, n01xp.PartyName);
			partContactNames = org.Contacts[0].OC_ContactName.Split(' ');
			AssertEquals(2, partContactNames.Length);
			AssertEquals("ContactFirstName", partContactNames[0], n01xp.ContactFirstName);
			AssertEquals("ContactLastName", partContactNames[1], n01xp.ContactLastName);

			AssertEquals("AddressLine1", pickupDocAddress.E2_Address1.ToUpper(), n02xp.AddressLine1);
			AssertEquals("AddressLine2", pickupDocAddress.E2_Address2.ToUpper(), n02xp.AddressLine2);
			AssertEquals("ContactPhoneNumber", org.Contacts[0].OC_Phone, n02xp.ContactPhoneNumber);
			AssertEquals("PostalCode", pickupDocAddress.E2_Postcode.ToUpper(), n03xp.PostalCode);

			AssertEquals("City", pickupDocAddress.E2_City, n03xp.City);
			AssertEquals("StateCode", pickupDocAddress.E2_State.Left(2), n03xp.StateCode);
			AssertEquals("CountryCode", pickupDocAddress.E2_RN_NKCountryCode, n03xp.CountryCode);
			AssertEquals("USPPIIRSIDType", "", n03xp.USPPIIRSIDType);
			AssertEquals("USPPIIRSNumber", "", n03xp.USPPIIRSNumber);
		}

		protected override void SetUp()
		{
			GlbStaff.CurrentUser.GS_FullName = "MR BOB SMITH";
			base.SetUp();
		}

		JobDeclaration GetAlreadySetupDeclaration()
		{
			OrgHeader supplier = CreateOrganisation("SUP", "SUPPLIER", "USLAX");
			supplier.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "126597687", Core.Constants.CountryCodes.UnitedStates);
			OrgHeader pickup = CreateOrganisationWithAddress("PIU", "PICKUP", "USCHI", "PICKUP ADDRESS 1", "PICKUP ADDRESS 2", "PICKUP CITY", "PUPOST", "PICKUP STATE");
			OrgHeader importer = CreateOrganisation("IMP", "IMPORTER", "AUSYD");
			importer.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "96532144875", Core.Constants.CountryCodes.UnitedStates);
			OrgHeader forwarder = CreateOrganisation("FWD", "FORWARDER", "PRGUY");
			forwarder.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "63265985468", Core.Constants.CountryCodes.UnitedStates);
			OrgHeader intermConsignee = CreateOrganisation("INT", "INTERM CONSIGNEE", "AUMEL");
			intermConsignee.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "32656946980", Core.Constants.CountryCodes.UnitedStates);
			OrgHeader shippingLine = CreateOrganisation("SHP", "SHIPPING LINE", "NZAKL");
			shippingLine.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "AACP", Core.Constants.CountryCodes.UnitedStates);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.SupplierPickupAddress.E2_OA_Address = pickup.MainAddress.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Forwarder = forwarder.PK;
			declaration.JE_OH_Consignee = intermConsignee.PK;
			declaration.JE_OH_ShippingLine = shippingLine.PK;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.JE_VesselName = "APL VESSEL TESTING";
			declaration.JE_VoyageFlightNo = "E123";
			declaration.JE_MasterBill = "MB5698463112";
			declaration.JE_HouseBill = "HB5698463112";
			declaration.JE_TotalNoOfPacks = 100;
			declaration.JE_TotalWeight = 150m;
			declaration.JE_TotalVolume = 13m;
			declaration.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;
			declaration.US_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Australia;
			declaration.US_StateOfOrigin = USStatesList.Codes.California;
			declaration.JE_ExportDate = new ZDateTime(2009, 12, 13);
			declaration.US_InbondType = InbondTypeList.Codes.IEForeignTradeZoneWithdrawal;
			declaration.US_ImportEntryNo = "56846684";
			declaration.US_ForeignTradeZone = "FZX23";
			declaration.US_RoutedTransaction = YesNoDefaultList.Codes.Yes;
			declaration.US_TransportReference = "TRNREF23423";
			declaration.US_HazardousCargo = YesNoDefaultList.Codes.Yes;
			declaration.US_SoldEnRouteIndicator = YesNoDefaultList.Codes.No;
			declaration.US_FirstPortOfCallCity = "SYDNEY";
			declaration.US_RN_NKFirstPortOfCallCountry = Core.Constants.CountryCodes.Australia;
			declaration.US_SchDLoading = DeclarationTestHelper.USLAXScheduleDOrK;
			declaration.US_SchDExport = DeclarationTestHelper.USCHIScheduleDOrK;
			declaration.US_SchDArrival = DeclarationTestHelper.AUSYDScheduleDOrK;
			declaration.US_CommodityFilingOption = AESCommodityFilingOptionList.Codes._2Predeparture;

			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRUX432890";
			container.CO_Seal = "SL23";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 15000m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1010101010";
			invoiceLine1.JI_Description = "DESCRIPTION 1";
			invoiceLine1.US_ExportCode = ExportInformationCodeList.Codes.OS;
			invoiceLine1.US_LicenseType = USAESLicenseCode.Codes.C33;
			invoiceLine1.US_LicenseNo = "LCN1234";
			invoiceLine1.JI_Weight = 145m;
			invoiceLine1.JI_CustomsQuantity = 24m;
			invoiceLine1.JI_CustomsUnitQty = AESUnitOfMeasureList.Codes.Barrels;
			invoiceLine1.JI_CustomsSecondQuantity = 46m;
			invoiceLine1.JI_CustomsSecondUnitQty = AESUnitOfMeasureList.Codes.Pairs;
			invoiceLine1.US_ECCN = "EC12";
			invoiceLine1.JI_LinePrice = 1500m;
			invoiceLine1.US_AESOriginIndicator = AESOriginIndicatorList.Codes.Domestic;
			invoiceLine1.US_DDTCITARExemptionNo = "123.11B";
			invoiceLine1.US_DDTCMilitaryEquipmentIndicator = YesNoDefaultList.Codes.Yes;
			invoiceLine1.US_DDTCPartyCertificationIndicator = YesNoDefaultList.Codes.Yes;
			invoiceLine1.US_DDTCQuantity = 142m;
			invoiceLine1.US_DDTCRegistrationNo = "REG123";
			invoiceLine1.US_DDTCUnit = DDTCUnitOfMeasureList.Codes.BulletsRounds;
			invoiceLine1.US_DDTCUSMLCategoryCode = USMLCategoryCodes.Codes.ClassifiedArticlesTechnicalDataAndDefenseServicesNotOtherwiseEnumerated;

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2020202020";
			invoiceLine2.JI_Description = "DESCRIPTION 2";
			invoiceLine2.US_ExportCode = ExportInformationCodeList.Codes.CH;
			invoiceLine2.US_LicenseType = USAESLicenseCode.Codes.C30;
			invoiceLine2.US_LicenseNo = "LCN5678";
			invoiceLine2.JI_Weight = 250m;
			invoiceLine2.JI_CustomsQuantity = 30m;
			invoiceLine2.JI_CustomsUnitQty = AESUnitOfMeasureList.Codes.Packs;
			invoiceLine2.JI_CustomsSecondQuantity = 60m;
			invoiceLine2.JI_CustomsSecondUnitQty = AESUnitOfMeasureList.Codes.Pieces;
			invoiceLine2.US_ECCN = "EC34";
			invoiceLine2.JI_LinePrice = 2600m;
			invoiceLine2.US_AESOriginIndicator = AESOriginIndicatorList.Codes.Foreign;
			invoiceLine2.US_IsUsedVehicle = true;
			invoiceLine2.US_VehicleIDType = VehicleIDTypeList.Codes.VIN;
			invoiceLine2.US_VehicleID = "VID12";
			invoiceLine2.US_VehicleTitleNo = "VTitleNo";
			invoiceLine2.US_VehicleTitleState = "MA";

			return declaration;
		}

		OrgHeader CreateOrganisationWithAddress(ZString shortCode, ZString name, ZString closestPort, ZString address1, ZString address2, ZString city, ZString postCode, ZString state)
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = shortCode + new Random().Next(1000000).ToString();
			org.OH_FullName = "MR " + name;
			org.OH_RL_NKClosestPort = closestPort;
			org.MainAddress.OA_Address1 = address1;
			org.MainAddress.OA_Address2 = address2;
			org.MainAddress.OA_City = city;
			org.MainAddress.OA_PostCode = postCode;
			org.MainAddress.OA_State = state;

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = shortCode + " SMITH";
			contact.OC_Phone = "1234567890";
			OrgDocument contactDocument = contact.Documents.AddNew();
			contactDocument.OD_DocumentGroup = ContactType.All.ToString();
			contactDocument.OD_DefaultContact = true;

			return org;
		}

		OrgHeader CreateOrganisation(ZString shortCode, ZString name, ZString closestPort)
		{
			return CreateOrganisationWithAddress(shortCode, name, closestPort, "Address 1", "Address 2", "City", "PostCode", "StateCode");
		}

		void AssertSC1Record(AESCommShipSC1XP messageBlock, ZString action, ZString shipmentReferenceNumber)
		{
			AssertEquals("RelatedCompanyIndicator", "Y", messageBlock.RelatedCompanyIndicator);
			AssertEquals("ModeOfTransportationCodeMOT", "11", messageBlock.ModeOfTransportationCodeMOT);
			AssertEquals("CountryOfUltimateDestinationCode", "AU", messageBlock.CountryOfUltimateDestinationCode);
			AssertEquals("USStateOfOriginCode", "CA", messageBlock.USStateOfOriginCode);
			AssertEquals("CarrierIDSCACIATA", "AACP", messageBlock.CarrierIDSCACIATA);
			AssertEquals("ShipmentFilingActionRequestIndicator", action, messageBlock.ShipmentFilingActionRequestIndicator);
			AssertEquals("ShipmentReferenceNumber", shipmentReferenceNumber, messageBlock.ShipmentReferenceNumber);
			AssertEquals("ConveyanceNameCarrierName", "APL VESSEL TESTING", messageBlock.ConveyanceNameCarrierName);
			AssertEquals("FilingOptionIndicator", AESCommodityFilingOptionList.Codes._2Predeparture, messageBlock.FilingOptionIndicator);
			AssertEquals("PortOfUnladingCode", DeclarationTestHelper.AUSYDScheduleDOrK, messageBlock.PortOfUnladingCode);
			AssertEquals("PortOfExportationCode", DeclarationTestHelper.USCHIScheduleDOrK, messageBlock.PortOfExportationCode);
			AssertEquals("EstimatedDateOfExport", new ZDate(2009, 12, 13), messageBlock.EstimatedDateOfExport);
			AssertEquals("HazardousMaterialIndicatorHAZMAT", "Y", messageBlock.HazardousMaterialIndicatorHAZMAT);
		}

		void AssertSC2Record(AESCommShipSC2XP messageBlock)
		{
			AssertEquals("InbondCode", InbondTypeList.Codes.IEForeignTradeZoneWithdrawal, messageBlock.InbondCode);
			AssertEquals("EntryNumber", "56846684", messageBlock.EntryNumber);
			AssertEquals("ForeignTradeZoneIdentifier", "FZX23", messageBlock.ForeignTradeZoneIdentifier);
			AssertEquals("RoutedExportTransactionIndicator", "Y", messageBlock.RoutedExportTransactionIndicator);
		}

		void AssertSC3Record(AESCommShipSC3XP messageBlock, ZString equipmentNumber, ZString sealNumber, ZString transportationReferenceNumber)
		{
			AssertEquals("EquipmentNumber", equipmentNumber, messageBlock.EquipmentNumber);
			AssertEquals("SealNumber", sealNumber, messageBlock.SealNumber);
			AssertEquals("TransportationReferenceNumber", transportationReferenceNumber, messageBlock.TransportationReferenceNumber);
		}

		void AssertAESParty(AESCommShipN01XP n01xp, AESCommShipN02XP n02xp, AESCommShipN03XP n03xp, ZString partyType, OrgHeader org, ZString partyID, ZString partyIDType, ZString toBeSoldEnRouteIndicator, ZString city, ZString stateCode, ZString countryCode, OrgHeader pickupOrg = null)
		{
			AssertEquals("PartyType", partyType, n01xp.PartyType);
			AssertEquals("PartyID", partyID, n01xp.PartyID);
			AssertEquals("PartyIDType", partyIDType, n01xp.PartyIDType);
			if (toBeSoldEnRouteIndicator == "Y")
			{
				AssertEquals("PartyName", AESConstants.SoldEnRouteName, n01xp.PartyName);
				AssertEquals("ContactFirstName", "", n01xp.ContactFirstName);
				AssertEquals("ContactLastName", "", n01xp.ContactLastName);

				AssertEquals("AddressLine1", "", n02xp.AddressLine1);
				AssertEquals("AddressLine2", "", n02xp.AddressLine2);
				AssertEquals("ContactPhoneNumber", "", n02xp.ContactPhoneNumber);
				AssertEquals("PostalCode", "", n03xp.PostalCode);
			}
			else
			{
				AssertEquals("PartyName", org.OH_FullName, n01xp.PartyName);
				ZString[] partContactNames = org.Contacts[0].OC_ContactName.Split(' ');
				AssertEquals(2, partContactNames.Length);
				AssertEquals("ContactFirstName", partContactNames[0], n01xp.ContactFirstName);
				AssertEquals("ContactLastName", partContactNames[1], n01xp.ContactLastName);

				AssertEquals("AddressLine1", pickupOrg != null ? pickupOrg.MainAddress.OA_Address1.ToUpper() : org.MainAddress.OA_Address1.ToUpper(), n02xp.AddressLine1);
				AssertEquals("AddressLine2", pickupOrg != null ? pickupOrg.MainAddress.OA_Address2.ToUpper() : org.MainAddress.OA_Address2.ToUpper(), n02xp.AddressLine2);
				AssertEquals("ContactPhoneNumber", org.Contacts[0].OC_Phone, n02xp.ContactPhoneNumber);
				AssertEquals("PostalCode", pickupOrg != null ? pickupOrg.MainAddress.OA_PostCode.ToUpper() : org.MainAddress.OA_PostCode.ToUpper(), n03xp.PostalCode);
			}

			AssertEquals("ToBeSoldEnRouteIndicator", toBeSoldEnRouteIndicator, n01xp.ToBeSoldEnRouteIndicator);
			AssertEquals("City", city, n03xp.City);
			AssertEquals("StateCode", stateCode, n03xp.StateCode);
			AssertEquals("CountryCode", countryCode, n03xp.CountryCode);
			AssertEquals("USPPIIRSIDType", "", n03xp.USPPIIRSIDType);
			AssertEquals("USPPIIRSNumber", "", n03xp.USPPIIRSNumber);
		}

		void AssertCommodityLine(JobComInvoiceLine invoiceLine, AESCommShipCL1XP cl1xp, AESCommShipCL2XP cl2xp, AESCommShipODTXP odtxp, AESCommShipEV1XP ev1xp1)
		{
			AssertEquals("ExportInformationCode", invoiceLine.US_ExportCode, cl1xp.ExportInformationCode);
			AssertEquals("LineNumber", (int)invoiceLine.JI_LineNo, cl1xp.LineNumber);
			AssertEquals("CommodityDescription", invoiceLine.JI_Description, cl1xp.CommodityDescription);
			AssertEquals("LicenseCodeLicenseExemptionCode", invoiceLine.US_LicenseType, cl1xp.LicenseCodeLicenseExemptionCode);
			AssertEquals("ForeignDomesticOriginIndicator", invoiceLine.US_AESOriginIndicator, cl1xp.ForeignDomesticOriginIndicator);

			AssertEquals("ScheduleBHTSNumber", invoiceLine.JI_Tariff, cl2xp.ScheduleBHTSNumber);
			AssertEquals("UnitOfMeasure1", invoiceLine.JI_CustomsUnitQty, cl2xp.UnitOfMeasure1);
			AssertEquals("Quantity1", invoiceLine.JI_CustomsQuantity, cl2xp.Quantity1);
			AssertEquals("ValueOfGoods", invoiceLine.JI_LinePriceInLocalCurrency, cl2xp.ValueOfGoods);
			AssertEquals("UnitOfMeasure2", invoiceLine.JI_CustomsSecondUnitQty, cl2xp.UnitOfMeasure2);
			AssertEquals("Quantity2", invoiceLine.JI_CustomsSecondQuantity, cl2xp.Quantity2);
			AssertEquals("ShippingWeight", invoiceLine.JI_Weight, cl2xp.ShippingWeight);
			AssertEquals("ExportControlClassificationNumberECCN", invoiceLine.US_ECCN, cl2xp.ExportControlClassificationNumberECCN);
			AssertEquals("ExportLicenseNumberCFRCitationAuthorizationSymbolKCP", invoiceLine.US_LicenseNo, cl2xp.ExportLicenseNumberCFRCitationAuthorizationSymbolKCPACM);

			if (odtxp != null)
			{
				AssertEquals("DDTCITARExemptionNumber", invoiceLine.US_DDTCITARExemptionNo, odtxp.DDTCITARExemptionNumber);
				AssertEquals("DDTCRegistrationNumber", invoiceLine.US_DDTCRegistrationNo, odtxp.DDTCRegistrationNumber);
				AssertEquals("DDTCSignificantMilitaryEquipmentSMEIndicator", invoiceLine.US_DDTCMilitaryEquipmentIndicator, odtxp.DDTCSignificantMilitaryEquipmentSMEIndicator);
				AssertEquals("DDTCEligiblePartyCertificationIndicator", invoiceLine.US_DDTCPartyCertificationIndicator, odtxp.DDTCEligiblePartyCertificationIndicator);
				AssertEquals("DDTCUSMLCategoryCode", invoiceLine.US_DDTCUSMLCategoryCode, odtxp.DDTCUSMLCategoryCode);
				AssertEquals("DDTCUnitOfMeasureCode", invoiceLine.US_DDTCUnit, odtxp.DDTCUnitOfMeasureCode);
				AssertEquals("DDTCQuantity", invoiceLine.US_DDTCQuantity, odtxp.DDTCQuantity);
			}

			if (ev1xp1 != null)
			{
				AssertEquals("VehicleIdentificationNumberVINProductID", invoiceLine.US_VehicleID, ev1xp1.VehicleIdentificationNumberVINProductID);
				AssertEquals("VehicleIDQualifier", invoiceLine.US_VehicleIDType, ev1xp1.VehicleIDQualifier);
				AssertEquals("VehicleTitleNumber", invoiceLine.US_VehicleTitleNo.ToUpper(), ev1xp1.VehicleTitleNumber);
				AssertEquals("VehicleTitleStateCode", invoiceLine.US_VehicleTitleState, ev1xp1.VehicleTitleStateCode);
			}
		}
	}
}
