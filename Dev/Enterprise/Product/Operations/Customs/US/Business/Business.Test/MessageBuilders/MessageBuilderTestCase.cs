using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.Testing;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	#region MessageBuildersSendingToCustomsTest
#if SendTestMessagesToCustoms

	class MessageBuildersSendingToCustomsTest : TestCaseWithSetup
	{
		[DeveloperOnlyTest]
		public void TestAutomatedClearinghouse()
		{
			DynamicMock<IPaymentAuthorisation> paymentMock = new DynamicMock<IPaymentAuthorisation>();
			paymentMock.ExpectAndReturnAlways("PayersUnitNumber", "101009");
			paymentMock.ExpectAndReturnAlways("PaymentType", 1);
			paymentMock.ExpectAndReturnAlways("StatementFiler", "JSD");
			paymentMock.ExpectAndReturnAlways("StatementBillNumber", "43131398549");
			paymentMock.ExpectAndReturnAlways("PaymentAmount", 435.23m);
			paymentMock.ExpectAndReturnAlways("ProcessingPortCode", "8888");
			IPaymentAuthorisation payment = paymentMock.Object;
			AutomatedClearinghouseMessageBuilder builder = new AutomatedClearinghouseMessageBuilder(Factory);
			MQEDIMessage message = builder.Generate<ACHQT>(ApplicationIdentifierCodeList.Codes.AutomatedClearinghouse, payment);
			Factory.Save();
			AssertEquals(ApplicationIdentifierCodeList.Codes.AutomatedClearinghouse, message.EM_MessageType);
		}

		[DeveloperOnlyTest]
		public void TestCargoManifestStatusQuery()
		{
			JobDeclaration declaration = CreateDeclaration();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableINB = true;
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "001821000";

			OrgHeader issuer = CreateOrganisation("ISS", "ISSUER");
			issuer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "APLU", GlbCompany.CurrentCompany.CountryCode);
			bill.US_UI_NKBillIssuerSCAC = "APLU";
			Factory.Save();

			CusEntryHeader entrySummary = declaration.CustomsEntryHeaders.AddNew();
			entrySummary.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entrySummary.CH_BGMReference = declaration.JE_DeclarationReference + "/QueryForEntry";

			CargoManifestStatusQueryMessageBuilder builder = new CargoManifestStatusQueryMessageBuilder(CargoManifestStatusQueryActionList.Codes.Entry, entrySummary);
			EDIMessage queryForEntrymessage = builder.GenerateMessages();
			AssertEquals(ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatus, queryForEntrymessage.EM_MessageType);
			AssertEquals(EM_MessageSubTypeList.Codes.CargoManifestEntryQuery, queryForEntrymessage.EM_MessageSubType);

			CusEntryHeader inbondEntry = declaration.CustomsEntryHeaders.AddNew();
			inbondEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			inbondEntry.CH_BGMReference = declaration.JE_DeclarationReference + "/InBondQuery";

			builder = new CargoManifestStatusQueryMessageBuilder(CargoManifestStatusQueryActionList.Codes.InBond, inbondEntry);
			EDIMessage inBondQueryMessage = builder.GenerateMessages();
			AssertEquals(ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatus, inBondQueryMessage.EM_MessageType);
			AssertEquals(EM_MessageSubTypeList.Codes.CargoManifestInBondQuery, inBondQueryMessage.EM_MessageSubType);

			builder = new CargoManifestStatusQueryMessageBuilder(CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill, bill);
			EDIMessage oceanRailTruckBill = builder.GenerateMessages();
			AssertEquals(ApplicationIdentifierCodeList.Codes.QueryCurrentEntryStatus, oceanRailTruckBill.EM_MessageType);
			AssertEquals(EM_MessageSubTypeList.Codes.CargoManifestBillOfLadingQuery, oceanRailTruckBill.EM_MessageSubType);
			Factory.Save();
		}

		[DeveloperOnlyTest]
		public void TestDrawbackSummary()
		{
			DynamicMock<IDrawbackSummary> mock = new DynamicMock<IDrawbackSummary>();
			mock.ExpectAndReturnAlways("Factory", Factory);
			mock.ExpectAndReturnAlways("EntryFilerCode", "SV9");
			mock.ExpectAndReturnAlways("ProcessingOfficeCode", "12");

			mock.ExpectAndReturnAlways("DeleteCode", " ");
			mock.ExpectAndReturnAlways("ClaimNumber", "12365245211");
			mock.ExpectAndReturnAlways("ClaimType", 41);
			mock.ExpectAndReturnAlways("ClaimPort", "3901");
			mock.ExpectAndReturnAlways("EstimatedClaimDate", new ZDate(2008, 03, 24));
			mock.ExpectAndReturnAlways("ClaimantIdentification", "GFTR12365245");
			mock.ExpectAndReturnAlways("DrawbackTeam", " ");
			mock.ExpectAndReturnAlways("BondType", " ");
			mock.ExpectAndReturnAlways("SuretyCode", " ");
			mock.ExpectAndReturnAlways("NAFTAClaimIndicator", " ");
			mock.ExpectAndReturnAlways("GovernmentClaimIndicator", " ");
			mock.ExpectAndReturnAlways("AcceleratedClaimIndicator", " ");
			mock.ExpectAndReturnAlways("ExporterSummaryProcedureIndicator", " ");
			mock.ExpectAndReturnAlways("WaiverOfPriorNoticeIndicator", " ");
			mock.ExpectAndReturnAlways("PreInspectionIndicator", " ");
			mock.ExpectAndReturnAlways("AgentBroker", " ");
			mock.ExpectAndReturnAlways("BrokerReferenceNumber", " ");
			mock.ExpectAndReturnAlways("LicensePort", " ");

			mock.ExpectAndReturnAlways("FirstContractNumber", "33-33333-333");
			mock.ExpectAndReturnAlways("Description", " ");
			mock.ExpectAndReturnAlways("EarliestExportDate", new ZDate(2008, 03, 24));
			mock.ExpectAndReturnAlways("PetroleumClaimIndicator", "1");
			mock.ExpectAndReturnAlways("NAFTADrawbackCountryCode", "CA");

			//D12
			List<IDrawbackContractNumber> contracts = new List<IDrawbackContractNumber>();
			DynamicMock<IDrawbackContractNumber> mockContract = new DynamicMock<IDrawbackContractNumber>();
			mockContract.ExpectAndReturnAlways("ContractNumberCode", "33-33333-333");
			IDrawbackContractNumber contr = mockContract.Object;
			contracts.Add(contr);
			mock.ExpectAndReturnAlways("ExtraContractNumbers", contracts);

			//D20
			List<IDrawbackTrailerTariff> tariffs = new List<IDrawbackTrailerTariff>();
			DynamicMock<IDrawbackTrailerTariff> mockTariff = new DynamicMock<IDrawbackTrailerTariff>();
			mockTariff.ExpectAndReturnAlways("FirstTariffNumber", "5555555555");
			mockTariff.ExpectAndReturnAlways("AdditionalTariffNumber", "8888888888");
			mockTariff.ExpectAndReturnAlways("AdditionalTariffNumber1", " ");
			mockTariff.ExpectAndReturnAlways("AdditionalTariffNumber2", " ");
			mockTariff.ExpectAndReturnAlways("AdditionalTariffNumber3", " ");
			IDrawbackTrailerTariff tar = mockTariff.Object;
			tariffs.Add(tar);
			mock.ExpectAndReturnAlways("TrailerTariffs", tariffs);

			//D25
			List<IDrawbackTrailerScheduleBNumber> scheduleBNumbers = new List<IDrawbackTrailerScheduleBNumber>();
			DynamicMock<IDrawbackTrailerScheduleBNumber> mockscheduleB = new DynamicMock<IDrawbackTrailerScheduleBNumber>();
			mockscheduleB.ExpectAndReturnAlways("FirstScheduleBNumber", "9999999999");
			mockscheduleB.ExpectAndReturnAlways("AdditionalScheduleBNumber", " ");
			mockscheduleB.ExpectAndReturnAlways("AdditionalScheduleBNumber1", " ");
			mockscheduleB.ExpectAndReturnAlways("AdditionalScheduleBNumber2", " ");
			mockscheduleB.ExpectAndReturnAlways("AdditionalScheduleBNumber3", " ");
			IDrawbackTrailerScheduleBNumber sch = mockscheduleB.Object;
			scheduleBNumbers.Add(sch);
			mock.ExpectAndReturnAlways("TrailerScheduleBNumbers", scheduleBNumbers);

			//D30
			List<IDrawbackImportClaim> importClaims = new List<IDrawbackImportClaim>();
			DynamicMock<IDrawbackImportClaim> mocksClaim = new DynamicMock<IDrawbackImportClaim>();
			mocksClaim.ExpectAndReturnAlways("DrawbackImportEntry", "75212547521");
			mocksClaim.ExpectAndReturnAlways("DrawbackImportEntryPort", "8888");
			mocksClaim.ExpectAndReturnAlways("DrawbackImportEntryDate", new ZDate(2008, 03, 24));
			mocksClaim.ExpectAndReturnAlways("CMCDIndicator", "E");
			mocksClaim.ExpectAndReturnAlways("DrawbackClaimDuty", 0m);
			mocksClaim.ExpectAndReturnAlways("DrawbackClaimTax", 0m);
			IDrawbackImportClaim claim = mocksClaim.Object;
			importClaims.Add(claim);
			mock.ExpectAndReturnAlways("ImportClaims", importClaims);

			//D50
			List<IDrawbackNAFTATariff> naftas = new List<IDrawbackNAFTATariff>();
			DynamicMock<IDrawbackNAFTATariff> mocksNAFTA = new DynamicMock<IDrawbackNAFTATariff>();
			mocksNAFTA.ExpectAndReturnAlways("NAFTACountryImportEntry", "12348765");
			mocksNAFTA.ExpectAndReturnAlways("NAFTACountryImportEntryDate", new ZDate(2008, 03, 24));
			mocksNAFTA.ExpectAndReturnAlways("NAFTACountryTariffNumber", "80110011");
			mocksNAFTA.ExpectAndReturnAlways("NAFTACountryDutyRate", 0m);
			mocksNAFTA.ExpectAndReturnAlways("NAFTACountryImportDuty", 0m);
			mocksNAFTA.ExpectAndReturnAlways("EquivalentUSDollarAmountOfNAFTACountryDuty", 0m);
			IDrawbackNAFTATariff naf = mocksNAFTA.Object;
			naftas.Add(naf);
			mocksClaim.ExpectAndReturnAlways("DrawbackNAFTATariffs", naftas);

			//D40
			List<IDrawbackManufactureClaim> manClaims = new List<IDrawbackManufactureClaim>();
			DynamicMock<IDrawbackManufactureClaim> mocksManufClaim = new DynamicMock<IDrawbackManufactureClaim>();
			mocksManufClaim.ExpectAndReturnAlways("CertificateOfManufactureNumber", "CM563256");
			mocksManufClaim.ExpectAndReturnAlways("CertificateOfManufacturePort", "0401");
			mocksManufClaim.ExpectAndReturnAlways("DrawbackClaimDuty", 0m);
			mocksManufClaim.ExpectAndReturnAlways("DrawbackClaimTax", 0m);
			mocksManufClaim.ExpectAndReturnAlways("DrawbackManufactureQuantity", 156m);
			mocksManufClaim.ExpectAndReturnAlways("DrawbackManufactureUnitOfMeasure", "KG");

			//D41
			mocksManufClaim.ExpectAndReturnAlways("DescriptionForBlock41", "DESCRIPTION");

			IDrawbackManufactureClaim manClaim = mocksManufClaim.Object;
			manClaims.Add(manClaim);
			mock.ExpectAndReturnAlways("ManufactureClaims", manClaims);

			//D50
			List<IDrawbackNAFTATariff> naftas2 = new List<IDrawbackNAFTATariff>();
			mocksManufClaim.ExpectAndReturnAlways("DrawbackNAFTATariffs", naftas2);

			//D90
			mock.ExpectAndReturnAlways("TotalClaimDuty", 0m);
			mock.ExpectAndReturnAlways("TotalClaimTax", 0m);
			mock.ExpectAndReturnAlways("TotalNAFTACountryImportDuty", 0m);
			mock.ExpectAndReturnAlways("TotalUSDollarEquivalentOfNAFTACountryDuty", 0m);

			IDrawbackSummary drawbackSummary = mock.Object;

			DrawbackSummaryMessageBuilder builder = new DrawbackSummaryMessageBuilder("A", drawbackSummary);
			builder.Generate();
			Factory.Save();
			ZQuery query = new ZQuery(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.DrawbackSummary);
			query.AddToFilter(EDIMessageSchema.EM_MessageText, SQLComparisonOperator.Contains, "12365245211");
			query.FetchOnlyFromLocalCache = true;
			AssertNotNull("A drawback message should have been created for claim number '12365245211'", Factory.LoadTop1<EDIMessage>(query));
		}

		[DeveloperOnlyTest]
		public void TestEntryDateUpdate()
		{
			JobDeclaration declaration = CreateDeclaration();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_SchDEntry = "8899";
			Factory.Save();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			EntryHeaderMessageSendingAction action = new EntryHeaderMessageSendingAction(entry, ImportMessageStatusList.MessageType.EntrySummary, new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.EntryDateUpdate));
			action.US_RevisedEntryDate = new ZDateTime(2007, 7, 7);
			MQEDIMessage entryDateUpdateMessage = new EntryDateUpdateBuilder(action).PopulateMessage();
			AssertEquals(ApplicationIdentifierCodeList.Codes.EntryDateUpdateTransaction, entryDateUpdateMessage.EM_MessageType);
			AssertEquals(EM_MessageSubTypeList.Codes.EntryDateUpdate, entryDateUpdateMessage.EM_MessageSubType);
			Factory.Save();
		}

		[DeveloperOnlyTest]
		public void TestEntrySummaryQuery()
		{
			JobDeclaration declaration = CreateDeclaration();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_SchDEntry = "9999";
			Factory.Save();
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			EntryHeaderMessageSendingAction action = new EntryHeaderMessageSendingAction(entry, ImportMessageStatusList.MessageType.EntrySummary, new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.EntrySummaryQuery));
			action.US_CollectionBillInformationCode = CollectionBillInformationCodesList.Codes._3;
			EntrySummaryQueryMessageBuilder builder = new EntrySummaryQueryMessageBuilder(action);
			MQEDIMessage message = builder.PopulateMessage();
			AssertEquals(ApplicationIdentifierCodeList.Codes.QueryEntrySummary, message.EM_MessageType);
			AssertEquals(EM_MessageSubTypeList.Codes.EntrySummaryQuery, message.EM_MessageSubType);
			Factory.Save();
		}

		[DeveloperOnlyTest]
		public void TestEstablishmentIdentifierADD()
		{
			OrgHeader org = CreateOrganisation("ESB", "ESTABLISHMENT");
			OrgHeaderWrapper wrapper = OrgHeaderWrapper.New(org);
			EstablishmentIdentifierBuilder builder = new EstablishmentIdentifierBuilder(new FDAEstablishmentAddMessageData(wrapper), EstablishmentIdentifierBuilder.Action.A);
			builder.Generate();
			Factory.Save();
			ZQuery query = new ZQuery(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.FoodandDrugAdministrationEstablishmentIdentifier);
			query.AddToFilter(EDIMessageSchema.EM_MessageText, SQLComparisonOperator.Contains, "MR ESTABLISHMENT");
			query.FetchOnlyFromLocalCache = true;
			AssertNotNull("An establishment identifier message should have been created for 'MR ESTABLISHMENT'", Factory.LoadTop1<EDIMessage>(query));
		}

		[DeveloperOnlyTest]
		public void TestImporterADD()
		{
			OrgHeader organisation = CreateOrganisation("IMP", "IMPORTER");
			OrgCusCode cusCode = organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-1234567AB");
			OrgHeaderWrapper organisationWrapper = OrgHeaderWrapper.New(organisation);
			organisationWrapper.ZO_ImporterType = ImporterTypeList.Codes.Corporation;
			OrgAddressMessageData messageData = new OrgAddressMessageData(organisationWrapper, OrgMessageType.CBPF5106Add);
			ImporterADDBuilder builder = new ImporterADDBuilder(messageData);

			OrgAddress mainAddress = organisation.MainAddress;
			mainAddress.OA_Address1 = "This is the first address line that is long";
			mainAddress.OA_Address2 = "Second Address Line";
			mainAddress.OA_City = "CityCityCityCity";
			mainAddress.OA_State = "StateStateState";
			mainAddress.OA_RL_NKRelatedPortCode = "KRPUS";
			mainAddress.OA_PostCode = "608-214";

			OrgAddress mailingAddress = organisation.Addresses.AddNew(OrgAddressType.Postal, true);
			mailingAddress.OA_RL_NKRelatedPortCode = "USCHI";
			mailingAddress.OA_Address1 = "111 Main Road";
			mailingAddress.OA_Address2 = "The second address Line that is very very long";
			mailingAddress.OA_City = "City";
			mailingAddress.OA_State = "IL";
			mailingAddress.OA_PostCode = "60010";
			mailingAddress.OA_RL_NKRelatedPortCode = "USCHI";

			MQEDIMessage message = builder.Generate();
			Factory.Save();
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.AddCBPFormCBPF5106DatatotheImporterFile, message.EM_MessageType);
		}

		[DeveloperOnlyTest]
		public void TestFDACorrection()
		{
			var declaration = CreateDeclaration();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 15000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.Charges.AddNew("OFT", 500m, "USD");

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0804506040";
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 5000m;
			invoiceLine.JI_Weight = 100;

			invoiceLine.FDAs.AddNew();
			invoiceLine.FDAs[0].US_FDACommercialDesc = "FRESH MANGOES,ENT 6/1-8/31";
			invoiceLine.FDAs[0].US_FDAValue = 15000m;

			invoiceLine.FDAs[0].US_FDAQty1 = 5000m;
			invoiceLine.FDAs[0].US_FDAMeasure1 = "KG";

			invoiceLine.FDAs[0].US_FDAQty2 = 2m;
			invoiceLine.FDAs[0].US_FDAMeasure2 = "CT";

			invoiceLine.FDAs[0].US_FDAProductCode = "21SYB05";
			invoiceLine.FDAs[0].US_UC_NKFDAProduction = "PE";

			declaration.DoMerge();
			Factory.Save();
			var ensEntry = declaration.CustomsEntryHeaders[0];
			new FDACorrectionMessageBuilder(declaration).Generate();
			Factory.Save();
			var message = ensEntry.Messages[0];
			AssertEquals(ApplicationIdentifierCodeList.Codes.OtherAgencyEntryDataUpdate, message.EM_MessageType);
		}

		[DeveloperOnlyTest]
		public void TestInbond()
		{
			JobDeclaration declaration = CreateDeclaration();
			declaration.US_EnableINB = true;
			declaration.US_EntryType = "";
			OrgHeader InBondCarrier = CreateOrganisation("CAR", "CARRIER");
			InBondCarrier.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "9999");
			InBondCarrier.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "888888888888");
			//((OrgImpAddInfo)InBondCarrier.CountryData.ImpAddInfo).ZO_AMSCarrierIndicator = YesNoDefaultList.Codes.Yes;

			declaration.US_InbondType = EntryTypeList.Codes.ImmediateExportation;
			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.US_SchDUSDestination = "USCHI";
			declaration.JE_MasterBill = "08155555555";
			Bill bill = declaration.PrimaryMasterBill;
			bill.US_SequenceNo = 1;
			bill.US_PreviousITNo = "777777777777";
			bill.US_InBondQty = 1;
			bill.US_GoodsValueInLocalCurrency = 2000m;

			OrgHeader issuer = CreateOrganisation("ISS", "ISSUER");
			issuer.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "4444", GlbCompany.CurrentCompany.CountryCode);
			bill.US_UI_NKBillIssuerSCAC = "4444";

			declaration.JE_OH_ShippingLine = InBondCarrier.PK;
			declaration.US_ImportConveyanceName = "ImportingConveyanceName";
			declaration.JE_VoyageFlightNo = "55555";
			declaration.JE_VesselName = "APL EMERALD";
			declaration.JE_RL_NKPortOfArrival = "USCHI";
			declaration.US_EntryDate = new ZDateTime(2005, 12, 31);

			HouseBillRefNo refNo = bill.ReferenceNos.AddNew();
			refNo.CY_Code = "M";
			refNo.CY_Data = "22222";

			refNo = bill.ReferenceNos.AddNew();
			refNo.CY_Code = "H";
			refNo.CY_Data = "33333";

			bill.CU_NoOfPacks = 20;
			bill.CU_PackType = "CTNS";
			bill.US_Weight = 44m;
			bill.US_WeightUQ = Core.Constants.Weight.Tonnes;
			bill.US_Volume = 1000m;
			bill.US_VolumeUQ = Core.Constants.Volume.CubicYards;
			bill.US_PreReceiptPlace = "New Zealand";

			bill.ForeignShipperAddress.E2_AddressOverride = true;
			bill.ForeignShipperAddress.E2_CompanyName = "Fast Forwarding Around The World Pty Ltd";
			bill.ForeignShipperAddress.E2_Address1 = "123456 Foreign Shipper St";
			bill.ForeignShipperAddress.E2_City = "Chicago";
			bill.ForeignShipperAddress.E2_State = "IL";
			bill.ForeignShipperAddress.E2_RN_NKCountryCode = "US";
			bill.ForeignShipperAddress.E2_Phone = "584 2514 2523";

			bill.ConsigneeAddress.E2_AddressOverride = true;
			bill.ConsigneeAddress.E2_CompanyName = "Fast Forwarding Around The World Pty Ltd";
			bill.ConsigneeAddress.E2_Address1 = "123456 Foreign Shipper St";
			bill.ConsigneeAddress.E2_City = "Chicago";
			bill.ConsigneeAddress.E2_State = "IL";
			bill.ConsigneeAddress.E2_RN_NKCountryCode = "US";
			bill.ConsigneeAddress.E2_Phone = "584 2514 2523";

			bill.NotifyPartyAddress.E2_AddressOverride = true;
			bill.NotifyPartyAddress.E2_CompanyName = "Fast Forwarding Around The World Pty Ltd";
			bill.NotifyPartyAddress.E2_Address1 = "123456 Foreign Shipper St";
			bill.NotifyPartyAddress.E2_City = "Chicago";
			bill.NotifyPartyAddress.E2_State = "IL";
			bill.NotifyPartyAddress.E2_RN_NKCountryCode = "US";
			bill.NotifyPartyAddress.E2_Phone = "584 2514 2523";

			CusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRUX123456";
			container.CO_Seal = "777777777777777";
			container.CO_SecondSeal = "888888888888888";
			container.CO_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40FR").PK;

			PackingGroup packGroup = (PackingGroup)bill.PackingGroups.AddNew();
			packGroup.CR_CO_Container = container.PK;

			Package package = declaration.Packages.AddNew();
			package.CW_HouseBill = bill.CU_BillUniqueCode;
			package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
			package.CW_MarksAndNos = "A".PadRight(45, 'A');

			JobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";
			invoice1.JZ_InvoiceAmount = 11000m;

			JobComInvoiceLine invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line1.JI_Tariff = "1111.11.11 11";
			invoice1Line1.JI_LinePrice = 5000m;
			invoice1Line1.JI_NetWeight = 200m;
			invoice1Line1.JI_NetWeightUQ = Core.Constants.Weight.Tonnes;
			invoice1Line1.US_ManifestQty = 10;
			invoice1Line1.JI_Description = "The description of the cargo";
			invoice1Line1.ContainersForInvoiceLines[0].IsForInvoiceLine = true;

			JobComInvoiceLine invoice1Line2 = invoice1.JobComInvoiceLines.AddNew();
			invoice1Line2.JI_Tariff = "1111.11.11 11";
			invoice1Line2.JI_LinePrice = 6000m;
			invoice1Line2.JI_NetWeight = 3000m;
			invoice1Line2.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoice1Line2.US_ManifestQty = 20;
			invoice1Line2.JI_Description = "The description of the cargo2";
			invoice1Line2.ContainersForInvoiceLines[0].IsForInvoiceLine = true;

			UNDGDataItem dg1 = invoice1Line1.UNDGs.AddNew();
			dg1.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;

			UNDGDataItem dg2 = invoice1Line2.UNDGs.AddNew();
			dg2.DI_DG = UNDGSubstanceLoader.LoadSubstance(Factory, "0004", "b", "IMO").First().PK;

			OrgContact contact = issuer.Contacts.AddNew();
			contact.OC_ContactName = "Contact me if it explodes";
			dg1.DI_OC_DGContact = contact.PK;
			dg2.DI_OC_DGContact = contact.PK;

			invoice1Line1.US_HazMatClassDesc = "classification or division of label requirements";

			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceAmount = 5000m;
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.US_UC_NKCountryOfExport = "CH";

			JobComInvoiceLine invoice2Line1 = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line1.JI_Tariff = "1902194000";	//Spaghetti
			invoice2Line1.JI_LinePrice = 5000M;
			invoice2Line1.US_UC_NKCountryOfOrigin = "CH";
			invoice2Line1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoice2Line1.JI_NetWeight = 1500m;
			invoice2Line1.JI_NetWeightUQ = "KG";
			invoice2Line1.ContainersForInvoiceLines[0].IsForInvoiceLine = true;

			//only at the X or parent invoice lines
			invoice2Line1.US_ManifestQty = 10;
			invoice2Line1.JI_Description = "Sets of Spaghetti with dried mushrooms and tomato paste";

			JobComInvoiceLine invoice2Line1a = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line1a.JI_Tariff = "1902194000";	//Spaghetti
			invoice2Line1a.JI_LinePrice = 2400M;
			invoice2Line1a.US_UC_NKCountryOfOrigin = "CH";
			invoice2Line1a.JI_CustomsQuantity = 5m;
			invoice2Line1a.JI_Weight = 50m;
			invoice2Line1a.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoice2Line1a.JI_NetWeight = 500m;
			invoice2Line1a.JI_NetWeightUQ = "KG";
			invoice2Line1a.ContainersForInvoiceLines[0].IsForInvoiceLine = true;

			JobComInvoiceLine invoice2Line2 = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line2.JI_Tariff = "0712311000";	// Mushrooms
			invoice2Line2.JI_LinePrice = 1300m;
			invoice2Line2.JI_CustomsQuantity = 5m;
			invoice2Line2.US_UC_NKCountryOfOrigin = "FR";
			invoice2Line2.JI_Weight = 50m;
			invoice2Line2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoice2Line2.JI_NetWeight = 100m;
			invoice2Line2.JI_NetWeightUQ = "KG";
			invoice2Line2.ContainersForInvoiceLines[0].IsForInvoiceLine = true;

			JobComInvoiceLine invoice2Line3 = invoice2.JobComInvoiceLines.AddNew();
			invoice2Line3.JI_Tariff = "2002908020";	// Tomato Paste
			invoice2Line3.JI_LinePrice = 1300m;
			invoice2Line3.JI_CustomsQuantity = 5m;
			invoice2Line3.US_UC_NKCountryOfOrigin = "IT";
			invoice2Line3.JI_Weight = 50m;
			invoice2Line3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoice2Line3.JI_NetWeight = 900m;
			invoice2Line3.JI_NetWeightUQ = "KG";
			invoice2Line3.ContainersForInvoiceLines[0].IsForInvoiceLine = true;
			Factory.Save();

			declaration.DoMerge();
			Factory.Save();
			CusEntryHeader inbondEntry = declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.InBondDeparture)[0];
			InBondQPMessageBuilder builder = new InBondQPMessageBuilder(inbondEntry, InBondQPMessageType.Original);
			MQEDIMessage message = builder.PopulateMessage();
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.InbondTransaction, message.EM_MessageType);
			Factory.Save();
		}

		[DeveloperOnlyTest]
		public void TestInBondWPArriveExportTOL()
		{
			DummyIInBondWP dummy = Factory.New<DummyIInBondWP>();

			dummy.InBondNumber = "111111111";
			dummy.MasterBillIssuerCode = "2222";
			dummy.MasterBillNumber = "333333333333";
			dummy.ContainerNumber = "4444";

			dummy.ArrivalDateTime = new ZDateTime(2006, 12, 10, 13, 1, 1);
			dummy.ScheduleDPortOfArrival = "2222";

			dummy.InBondCarrierCode = "3333";
			dummy.BondedCarrierID = "4444";
			dummy.CityName = "City Name Which Is Very Very long";
			dummy.StateCode = "IL";
			dummy.TransportMode = TransportModeCodes.Codes.AirContainer;
			dummy.ExportConveyance = "Export conveyance";

			new InBondWPMQEDIMessageBuilder().Generate(dummy, InBondWPActionCodeList.Codes.ArriveEntireInBondAtDestination);

			ZQuery query = new ZQuery(EDIMessageSchema.EM_MessageType, ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability);
			query.AddToFilter(EDIMessageSchema.EM_MessageSubType, EM_MessageSubTypeList.Codes.InBondArrival);
			query.FetchOnlyFromLocalCache = true;
			AssertNotNull("An Inbond Arrival message should have been created", Factory.LoadTop1<EDIMessage>(query));

			dummy.ExportDateTime = new ZDateTime(2006, 12, 10, 13, 1, 1);
			dummy.PortOfExport = "2222";

			dummy.InBondCarrierCode = "3333";
			dummy.BondedCarrierID = "4444";
			dummy.CityName = "City Name Which Is Very Very long";
			dummy.StateCode = "IL";
			dummy.TransportMode = TransportModeCodes.Codes.AirContainer;
			dummy.ExportConveyance = "Export conveyance";

			new InBondWPMQEDIMessageBuilder().Generate(dummy, InBondWPActionCodeList.Codes.ExportEntireInBondFromDestinationPort);
			Factory.Save();
			query = new ZQuery(EDIMessageSchema.EM_MessageType, ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability);
			query.AddToFilter(EDIMessageSchema.EM_MessageSubType, EM_MessageSubTypeList.Codes.InBondExportation);
			query.FetchOnlyFromLocalCache = true;
			AssertNotNull("An Inbond Exportation message should have been created", Factory.LoadTop1<EDIMessage>(query));
		}

		[DeveloperOnlyTest]
		public void TestManufacturerIdentifierAdd()
		{
			OrgHeader org = CreateOrganisation("MAN", "MANUFACTURER");
			org.MainAddress.OA_RL_NKRelatedPortCode = "CAPRW";
			org.MainAddress.OA_City = "Prince William";
			org.MainAddress.OA_State = "Nova Scotia";
			OrgHeaderWrapper wrapper = OrgHeaderWrapper.New(org);
			ManufacturerAddMessageData messageData = new ManufacturerAddMessageData(wrapper);
			MQEDIMessage message = ManufacturerIdentifierAddBuilder.Generate(messageData);
			Factory.Save();
			AssertEquals(ApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressAdd, message.EM_MessageType);
		}

		[TestDate(2016, 3, 11)]
		[DeveloperOnlyTest]
		public void TestManufacturerIdentifierQuery()
		{
			ManufacturerQueryMessageData messageData = Factory.New<ManufacturerQueryMessageData>();
			messageData.US_MID = "123";
			MQEDIMessage message = ManufacturerIdentifierQueryBuilder.Generate(messageData);
			Factory.Save();
			AssertEquals(ApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressQuery, message.EM_MessageType);
			AssertEquals(messageData, message.EM_LinkUniqueID);

			messageData.US_AutoCreateOrganization = true;
			message = ManufacturerIdentifierQueryBuilder.Generate(messageData);
			Factory.Save();
			AssertEquals(ApplicationIdentifierCodeList.Codes.ManufacturerNameandAddressQuery, message.EM_MessageType);
			AssertEquals(messageData, message.EM_LinkUniqueID);
		}

		[DeveloperOnlyTest]
		public void TestProtestAmendmentAddenda()
		{
			IProtest protest = Protest.Protest.GetProtestForAmendmentOrAddendaTransaction(Factory);
			var protestAmendmentBuilder = new ProtestAmendmentAddendaMessageBuilder(protest, ApplicationIdentifierCodeList.Codes.ProtestAmendment, true, "1");
			protestAmendmentBuilder.Generate();
			var query = new ZQuery(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.ProtestAmendment);
			query.FetchOnlyFromLocalCache = true;
			AssertNotNull("A Protest Amendment message should have been created", Factory.LoadTop1<EDIMessage>(query));

			var protestAddendaBuilder = new ProtestAmendmentAddendaMessageBuilder(protest, ApplicationIdentifierCodeList.Codes.ProtestAddenda, true);
			protestAddendaBuilder.Generate();
			query = new ZQuery(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.ProtestAddenda);
			query.FetchOnlyFromLocalCache = true;
			AssertNotNull("A Protest Addenda message should have been created", Factory.LoadTop1<EDIMessage>(query));
			Factory.Save();
		}

		[DeveloperOnlyTest]
		public void TestProtestInitialFiling()
		{
				IProtest protest = Protest.Protest.GetCompleteProtestObject(Factory);
				new ProtestInitialFilingMessageBuilder(protest, true, true, "1").Generate();

				var query = new ZQuery(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.ProtestInitialFiling);
				query.FetchOnlyFromLocalCache = true;
				AssertNotNull("A Protest Initial Filing message should have been created", Factory.LoadTop1<EDIMessage>(query));
				Factory.Save();
		}

		[DeveloperOnlyTest]
		public void TestProtestServiceRequest()
		{
			var protest = Protest.Protest.GetProtestForFullServiceRequest(Factory);
			var action = new ProtestMessageSendingAction(protest);
			action.IsWithdrawal = true;
			action.CertificationSignature = true;
			action.InquiryRequestIndicator = "1";
			new ProtestServiceRequestMessageBuilder(action).Generate();

			var query = new ZQuery(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.ProtestServiceRequest);
			query.FetchOnlyFromLocalCache = true;
			AssertNotNull("A Protest Service Request message should have been created", Factory.LoadTop1<EDIMessage>(query));
			Factory.Save();
		}

		[DeveloperOnlyTest]
		public void TestReconciliation()
		{
			DynamicMock<IReconciliation> mock = new DynamicMock<IReconciliation>();

			mock.ExpectAndReturnAlways("Branch", Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK));
			// R10
			mock.ExpectAndReturnAlways("Factory", Factory);
			mock.ExpectAndReturnAlways("EntryNumber", "EN");
			mock.ExpectAndReturnAlways("Port", "2304");
			mock.ExpectAndReturnAlways("ImporterID", "IID");
			mock.ExpectAndReturnAlways("SuretyCode", "SCD");
			mock.ExpectAndReturnAlways("EstimatedReconciliationEntrySummaryDate", ZDate.BrettsBirthday);
			mock.ExpectAndReturnAlways("IssueCode", "IC");
			mock.ExpectAndReturnAlways("AggregateReconciliationIndicator", true);
			mock.ExpectAndReturnAlways("IncreaseRefundIndicator", "2");
			mock.ExpectAndReturnAlways("EarliestImportDate", new ZDate(2000, 1, 1));
			mock.ExpectAndReturnAlways("EarliestEntrySummaryDate", new ZDate(2000, 1, 2));
			mock.ExpectAndReturnAlways("AgentBrokerReferenceID", "ABR");
			mock.ExpectAndReturnAlways("BrokerReferenceNumber", "BRN");

			// R15 + R16
			mock.ExpectAndReturnAlways("ImportEntrySource", 2);
			mock.ExpectAndReturnAlways("TextComment", new ZString('A', 75) + new ZString('B', 76));

			mock.ExpectAndReturnAlways("PaymentTypeIndicator", PaymentTypeList.Codes.IndividualBasis);
			mock.ExpectAndReturnAlways("PreliminaryStatementPrintDate", ZDate.BrettsBirthday);
			mock.ExpectAndReturnAlways("ClientBranchDesignation", "");

			mock.ExpectAndReturnAlways("DutyPaymentAmount", 0);
			mock.ExpectAndReturnAlways("TaxPaymentAmount", 0);
			mock.ExpectAndReturnAlways("FeePaymentAmount", 0);
			mock.ExpectAndReturnAlways("InterestPaymentAmount", 0);
			mock.ExpectAndReturnAlways("TeamNumber", new ZString("R1R"));

			List<IReconciliationImportEntry> importEntries = new List<IReconciliationImportEntry>();
			DynamicMock<IReconciliationImportEntry> importEntryMock = new DynamicMock<IReconciliationImportEntry>();
			importEntryMock.ExpectAndReturnAlways("ImportEntryFilerCodeNumber", "IEFCN");
			importEntryMock.ExpectAndReturnAlways("Port", "3901");
			importEntryMock.ExpectAndReturnAlways("OriginalDuty", 100m);
			importEntryMock.ExpectAndReturnAlways("EstimatedReconciliationDuty", 120m);
			importEntryMock.ExpectAndReturnAlways("OriginalTax", 200m);
			importEntryMock.ExpectAndReturnAlways("EstimatedReconciliationTax", 220m);
			importEntryMock.ExpectAndReturnAlways("EstimatedReconciliationInterest", 30m);

			importEntryMock.ExpectAndReturnAlways("Fees", new IReconciliationImportEntryFee[0]);

			importEntries.Add(importEntryMock.Object);

			mock.ExpectAndReturnAlways("ImportEntries", importEntries);

			IReconciliation reconciliation = mock.Object;
			new ReconciliationMessageBuilder("A", reconciliation).Generate();

			ZQuery query = new ZQuery(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.ReconciliationEntryFiling);
			query.FetchOnlyFromLocalCache = true;
			AssertNotNull("A Reconciliation Entry Filing message should have been created", Factory.LoadTop1<EDIMessage>(query));
			Factory.Save();
		}

		[DeveloperOnlyTest]
		public void TetsStatementUpdate()
		{
			CusStatementHeader statementHeader = Factory.New<CusStatementHeader>();
			statementHeader.B2_ProcessPort = "8888";
			statementHeader.B2_ProcessDate = new ZDateTime(2007, 9, 19);
			CusStatementLine statementLine = statementHeader.StatementLines.AddNew();
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_EntryNum = "10001127";
			StatementDeleteAndSendingActionCollection coll = new StatementDeleteAndSendingActionCollection(statementHeader);
			StatementDeleteTransactionWrapper wrapper = new StatementDeleteTransactionWrapper(statementLine);

			wrapper.PreliminaryStatementPrintDate = new ZDateTime(2007, 9, 19);
			wrapper.PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			wrapper.PeriodicStatementMonth = MonthList.Codes._09;

			StatementUpdateMessageBuilder builder = new StatementUpdateMessageBuilder(wrapper);
			MQEDIMessage message = builder.PopulateMessage();
			AssertEquals(ApplicationIdentifierCodeList.Codes.StatementDeleteTransaction, message.EM_MessageType);
			Factory.Save();
		}

		[DeveloperOnlyTest]
		public void TestRequestToExtendTIB()
		{
			JobDeclaration declaration = CreateDeclaration();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			declaration.US_SchDEntry = "8888";
			Factory.Save();
			new RequestToExtendTIBMessageBuilder(entry).GenerateMessages();
			MQEDIMessage message = (MQEDIMessage)entry.Messages[0];
			AssertEquals(ApplicationIdentifierCodeList.Codes.TemporaryImportationBondEntrySummaries, message.EM_MessageType);
			Factory.Save();
		}

		[DeveloperOnlyTest]
		public void TestBillOfLadingUpdate()
		{
			OrgHeader consignee = CreateOrganisation("IMP", "CONSIGNEE");
			consignee.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.CBPAssignedNumber, "91-013199000", Core.Constants.CountryCodes.UnitedStates);

			OrgHeader exportForwarder = CreateOrganisation("FWD", "EXPORT FORWARDER");
			exportForwarder.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "EXF1", Core.Constants.CountryCodes.UnitedStates);

			OrgHeader importForwarder = CreateOrganisation("FWD", "IMPORT FORWARDER");
			importForwarder.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "IMF1", Core.Constants.CountryCodes.UnitedStates);

			OrgHeader shippingLine = CreateOrganisation("SHI", "SHIPPINGLINE");
			shippingLine.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT1", Core.Constants.CountryCodes.UnitedStates);

			OrgHeader manufacturer = CreateOrganisation("MAN", "MANUFACTURER");
			OrgContact contact = manufacturer.Contacts.AddNew();
			contact.OC_ContactName = "Walter Doodleberry";
			contact.OC_Phone = "(847) 364 5600";
			OrgCusCode manufacturerCode = manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "XYBEREQU6LON");

			JobDeclaration declaration = CreateDeclaration();
			declaration.JE_OH_Importer = consignee.PK;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;

			declaration.JE_VesselName = "APL EMERALD";
			declaration.JE_VoyageFlightNo = "V123W";
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			declaration.JE_RL_NKPortOfArrival = "USSFO";
			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			declaration.US_SuretyCode = "891";
			declaration.JE_ExportDate = ZDateTime.Today.AddDays(-7);
			declaration.US_UI_NKCarrierSCAC = "OTT1";
			declaration.US_SchDEntry = "8888";
			declaration.JE_OH_ShippingLine = shippingLine.PK;

			declaration.JE_MasterBill = "M1234123";
			declaration.PrimaryMasterBill.US_UI_NKBillIssuerSCAC = "OTT1";
			declaration.JE_HouseBill = "H1234123";

			declaration.PrimaryHouseBill.US_UI_NKBillIssuerSCAC = "EXF1";
			Bill subHouseBill = declaration.PrimaryHouseBill.ChildBills.AddNew();
			subHouseBill.CU_BillNum = "SH123345";
			subHouseBill.ITNumber = "IT12433";
			subHouseBill.CU_NoOfPacks = 1;
			subHouseBill.CU_PackType = "PCS";

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV1";
			invoiceHeader.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			invoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 50m, "USD");

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "4421909720";
			invoiceLine.JI_Weight = 9000m;
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_InvoiceQuantity = 10000m;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_CustomsQuantity = 70m;

			Bill masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill2.CU_BillNum = "2M123456";
			masterBill2.US_UI_NKBillIssuerSCAC = "EXF1";

			Bill houseBill2 = masterBill2.ChildBills.AddNew();
			houseBill2.CU_BillNum = "2H123456";
			houseBill2.ITNumber = "IT12345";
			houseBill2.US_UI_NKBillIssuerSCAC = "IMF1";
			houseBill2.CU_NoOfPacks = 14;
			houseBill2.CU_PackType = "KG";

			JobComInvoiceHeader invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceNumber = "INV2";
			invoiceHeader2.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			invoiceHeader2.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 50m, "USD");

			JobComInvoiceLine invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "4421909720";
			invoiceLine2.JI_Weight = 9000m;
			invoiceLine2.JI_LinePrice = 10000m;
			invoiceLine2.JI_InvoiceQuantity = 10000m;
			invoiceLine2.JI_InvoiceUQ = "NO";
			invoiceLine2.JI_CustomsQuantity = 70m;

			declaration.DoMerge();
			Factory.Save();

			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			BLUMessageSendingAction action = new BLUMessageSendingAction(declaration, new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.BillOfLadingUpdate));
			action.US_RequestBillOfLadingResult = true;
			BillOfLadingUpdateBuilder builder = new BillOfLadingUpdateBuilder(action);

			MQEDIMessage message = builder.PopulateMessage();
			AssertEquals(ApplicationIdentifierCodeList.Codes.BillofLadingUpdate, message.EM_MessageType);
			AssertEquals(EM_MessageSubTypeList.Codes.BillOfLadingUpdate, message.EM_MessageSubType);
			Factory.Save();
		}

		[DeveloperOnlyTest]
		public void TestCargoRelease()
		{
			DeclarationTestHelper helper = new DeclarationTestHelper(Factory);

			OrgHeader importer = CreateOrganisation("IMP", "IMPORTER");
			OrgCusCode importerEmployerID = importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "69-9999999JC");
			OrgCusCode importerCustomsCode = importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "91-013199000");

			OrgHeader shippingLine = CreateOrganisation("SHP", "SHIPPING LINE");
			OrgCusCode scacCode = shippingLine.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "OTT1");

			OrgHeader manufacturer = CreateOrganisation("MAN", "MANUFACTURER");
			manufacturer.OH_FullName = "Test Manufacturer";
			manufacturer.OH_Code = "MAN" + new Random().Next(1000000).ToString();
			OrgCusCode manufacturerCode = manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "XYBEREQU6LON");
			//OrgContact contact = manufacturer.Contacts.AddNew();
			//contact.OC_ContactName = "Walter Wood";
			//contact.OC_Phone = "(847) 364 5600";

			JobDeclaration declaration = CreateDeclaration();
			declaration.JE_OH_Importer = importer.PK;
			declaration.IOROrgPK = importer.PK;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;

			declaration.JE_VesselName = "APL";
			declaration.JE_VoyageFlightNo = "V123W";
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			declaration.US_SuretyCode = "891";
			declaration.US_UI_NKCarrierSCAC = "OTT1";
			declaration.JE_OH_ShippingLine = shippingLine.PK;

			declaration.US_SchDEntry = "8888";
			declaration.US_SchDArrival = "1234";
			declaration.JE_RL_NKOrigin = "PERTD";

			Bill masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "TestMB1";
			declaration.JE_MasterBill = "TestMB1";
			masterBill.US_UI_NKBillIssuerSCAC = "OTT1";
			declaration.US_SchDLoading = "5650";
			declaration.US_SchDArrival = "5653";
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Liquid;
			declaration.IOROrgPK = declaration.JE_OH_Importer;
			declaration.US_PaymentType = "";

			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_OH_Buyer = importer.PK;
			invoiceHeader.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8471704065";
			invoiceLine.FDAs.AddNew();
			invoiceLine.FDAs[0].US_TradeBrandName = "TRADE NAME";
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_Weight = 120m;
			masterBill.CU_NoOfPacks = 130;
			masterBill.CU_PackType = "PCS";

			declaration.DoMerge();
			CusEntryHeader entry = declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.CargoRelease)[0];
			entry.US_UseConsigneeNameAddress = false;

			CargoReleaseMessageBuilder builder = new CargoReleaseMessageBuilder(entry, UpdateActionCode.Add, true);
			MQEDIMessage message = builder.PopulateMessage();
			AssertEquals(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions, message.EM_MessageType);

			entry.US_UseConsigneeNameAddress = true;
			MQEDIMessage message2 = builder.PopulateMessage();
			AssertEquals(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions, message2.EM_MessageType);

			//FDA tariffs
			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			helper.SetUpFDARequiredData(invoiceLine2, manufacturer, Factory);

			invoiceLine2.JI_Tariff = "0712902000";
			invoiceLine2.ImportTariff.UE_OGACodes = "FD2";
			invoiceLine2.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			invoiceLine2.JI_Description = "Description";

			CusEntryLine entryLine2 = entry.MergedLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.FDAs.AddNew();
			invoiceLine2.FDAs[0].US_FDACargoStorageCode = CargoStorageCodeList.Codes.AmbientTemperature;
			invoiceLine2.FDAs[0].US_TradeBrandName = "TRADE NAME";
			invoiceLine2.FDAs[0].US_FDAQty1 = 150;
			invoiceLine2.FDAs[0].US_FDAMeasure1 = FDABaseUQList.Codes.KG;

			MQEDIMessage message3 = builder.PopulateMessage();
			AssertEquals(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions, message3.EM_MessageType);

			invoiceLine.JI_Tariff = "3504005000";

			MQEDIMessage message4 = builder.PopulateMessage();
			AssertEquals(ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions, message4.EM_MessageType);
			Factory.Save();
		}

		[DeveloperOnlyTest]
		public void TestRequestStatement()
		{
			new MiscellaneousMessageRequester(Factory).RequestStatement<DSTQR>(ApplicationIdentifierCodeList.Codes.ABIStatementACHPaymentReroute, "", "", "", false, ZDate.Today, true, true, true, true);
			ZQuery query = new ZQuery(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.ABIStatementACHPaymentReroute);
			query.FetchOnlyFromLocalCache = true;
			AssertNotNull("A statemene reroute message should have been created", Factory.LoadTop1<EDIMessage>(query));
			Factory.Save();
		}

		[DeveloperOnlyTest]
		public void TestRequestADDCVDCases()
		{
			JobDeclaration declaration = CreateDeclaration();

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.US_UC_NKCountryOfOrigin = "AU";

			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine line3 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine line4 = declaration.InvoiceLines.AddNew();

			line1.JI_Tariff = USCTariff.CottonFeeApplicable;
			line1.US_ADDCaseNo = "A1";

			line2.JI_Tariff = line1.JI_Tariff;
			line2.US_ADDCaseNo = line1.US_ADDCaseNo;

			line3.JI_Tariff = line1.JI_Tariff;
			line3.US_ADDCaseNo = "A2";

			line4.JI_Tariff = line1.JI_Tariff;
			line4.US_CVDCaseNo = "C1";

			new ReferenceFileRequester(Factory).RequestADDCVDs(declaration);

			AssertEquals("There should be three messages generated", 3, declaration.Messages.Count);
			AssertEquals(ApplicationIdentifierCodeList.Codes.AntidumpingCountervailingDutyQuery, declaration.Messages[0].EM_MessageType);
			Factory.Save();
		}

		[DeveloperOnlyTest]
		public void TestRequestTariffs()
		{
			JobDeclaration declaration = CreateDeclaration();

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine line1 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine line2 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine line3 = declaration.InvoiceLines.AddNew();
			JobComInvoiceLine line4 = declaration.InvoiceLines.AddNew();

			line1.JI_Tariff = USCTariff.CottonFeeApplicable;
			line2.JI_Tariff = line1.JI_Tariff;
			line3.JI_Tariff = USCTariff.DOTMayBeApplicable;
			line4.JI_Tariff = "00012000";

			new ReferenceFileRequester(Factory).RequestTariffs(declaration, false);

			AssertEquals("There should be a message generated", 1, declaration.Messages.Count);
			AssertEquals(ApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQuery, declaration.Messages[0].EM_MessageType);
			AssertEquals(true, declaration.Messages[0].EM_MessageText.Contains("00012000"));
			AssertEquals(false, declaration.Messages[0].EM_MessageText.Contains(USCTariff.CottonFeeApplicable));
			AssertEquals(false, declaration.Messages[0].EM_MessageText.Contains(USCTariff.DOTMayBeApplicable));
			Factory.Save();
		}

		[TestDate(2006, 9, 18)]
		[DeveloperOnlyTest]
		public void TestRequestTariffByUpdate()
		{
			MQEDIMessage message = new ReferenceFileRequester(Factory).RequestTariffByUpdate(1357);
			AssertEquals(ApplicationIdentifierCodeList.Codes.ExtractReferenceFiles, message.EM_MessageType);
			Assert(message.EM_MessageText.Contains("1357"));
			Factory.Save();
		}

		[TestDate(2016, 03, 08)]
		[DeveloperOnlyTest]
		public void TestRequestImportSpecialistTeamAssignmentFile()
		{
			new ReferenceFileRequester(Factory).RequestImportSpecialistTeamAssignmentFile();
			ZQuery query = new ZQuery(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.ExtractReferenceFiles);
			query.AddToFilter(EDIMessageSchema.EM_MessageText, SQLComparisonOperator.Contains, "F112");
			query.FetchOnlyFromLocalCache = true;
			AssertNotNull("An Extract ReferenceFiles message should have been created", Factory.LoadTop1<EDIMessage>(query));
			Factory.Save();
		}

		[TestDate(2006, 9, 18)]
		[DeveloperOnlyTest]
		public void TestRequestExchangeRates()
		{
			new ReferenceFileRequester(Factory).RequestExchangeRates();
			ZQuery query = new ZQuery(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.ExtractReferenceFiles);
			query.AddToFilter(EDIMessageSchema.EM_MessageText, SQLComparisonOperator.Contains, "F108");
			query.FetchOnlyFromLocalCache = true;
			AssertNotNull("An Extract ReferenceFiles message should have been created", Factory.LoadTop1<EDIMessage>(query));
			Factory.Save();
		}

		[DeveloperOnlyTest]
		public void TestRequestImporterBond()
		{

			const string importerNumber = "12-345678901";
			new ImporterNumberRequester(Factory).RequestImporterBond(null, importerNumber);
			ZQuery query = new ZQuery(EDIMessageSchema.EM_MessageType, ACEApplicationIdentifierCodeList.Codes.QueryImporterBond);
			query.AddToFilter(EDIMessageSchema.EM_MessageText, SQLComparisonOperator.Contains, importerNumber);
			query.FetchOnlyFromLocalCache = true;
			AssertNotNull("A Query Importer Bond message should have been created", Factory.LoadTop1<EDIMessage>(query));
			Factory.Save();
		}

		[TestDate(2006, 9, 18)]
		[DeveloperOnlyTest]
		public void TestRequestCountry()
		{

			new ReferenceFileRequester(Factory).RequestCountry();
			ZQuery query = new ZQuery(EDIMessageSchema.EM_MessageType, ApplicationIdentifierCodeList.Codes.ExtractReferenceFiles);
			query.AddToFilter(EDIMessageSchema.EM_MessageText, SQLComparisonOperator.Contains, "F102");
			query.FetchOnlyFromLocalCache = true;
			AssertNotNull("An Extract ReferenceFiles message should have been created", Factory.LoadTop1<EDIMessage>(query));
			Factory.Save();
		}

		JobDeclaration CreateDeclaration()
		{
			OrgHeader importer = CreateOrganisation("ABC", "Importer");
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "91-013199000");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.IOROrgPK = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			return declaration;
		}

		OrgHeader CreateOrganisation(ZString shortCode, ZString name)
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = shortCode + new Random().Next(1000000).ToString();
			org.OH_FullName = "MR " + name;
			org.MainAddress.OA_Address1 = "Address 1";
			org.MainAddress.OA_City = "City";
			org.MainAddress.OA_State = "State";
			org.MainAddress.OA_PostCode = "PostCode";
			return org;
		}
	}

#endif
	#endregion

	#region MessageBuilderTestCase where SendTestMessagesToCustoms is defined
#if SendTestMessagesToCustoms

	abstract class MessageBuilderTestCase : TestCase
	{
		protected BusinessObjectFactory Factory;
		protected override void SetUp()
		{
			Factory = new BusinessObjectFactory();
			base.SetUp();
			sendTestMessagesToCustoms = true;
			startTime = DateTime.Now;
			importerPK = Enterprise.Customs.US.DataRegistry.Business.USCustomsRegistry.Instance.TestCaseWithSetupImporterPK.Value;
			shippingLinePK = Enterprise.Customs.US.DataRegistry.Business.USCustomsRegistry.Instance.TestCaseWithSetupShippingLinePK.Value;
			manufacturerPK = Enterprise.Customs.US.DataRegistry.Business.USCustomsRegistry.Instance.TestCaseWithSetupManufacturerPK.Value;
		}
		DateTime startTime;
		protected ZGuid importerPK;
		protected ZGuid shippingLinePK;
		protected ZGuid manufacturerPK;
		protected bool sendTestMessagesToCustoms;

		protected override void TearDown()
		{
			base.TearDown();
			ZQuery query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.USCustomsImport);
			query.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, startTime);
			query.FetchOnlyFromLocalCache = true;
			EDIMessage[] messages = Factory.Load<EDIMessage>(query);
			if (messages.Length > 0)
			{
				foreach (EDIMessage message in messages)
				{
					message.EM_ApplicationReference = "SendTestMessagesToCustoms";
				}
				Factory.Save();
			}
		}
	}

#endif
	#endregion

	#region MessageBuilderTestCase where SendTestMessagesToCustoms is not defined
#if !SendTestMessagesToCustoms

	abstract class MessageBuilderTestCase : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var port = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "8888", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			base.SetUp();
			sendTestMessagesToCustoms = false;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			importerPK = ZGuid.Empty;
			shippingLinePK = ZGuid.Empty;
			manufacturerPK = ZGuid.Empty;
		}
		protected ZGuid importerPK;
		protected ZGuid shippingLinePK;
		protected ZGuid manufacturerPK;
		protected bool sendTestMessagesToCustoms;
	}
#endif
	#endregion
}
