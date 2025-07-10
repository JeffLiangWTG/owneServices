using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.OperationalAction.Testing
{
	sealed class OperationalActionBulkENSMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendWithContactInfo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_CertifyCargoRelease = true;
			declaration.JE_DeclarationReference = "B000006";
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_PSC = false;
			var log = new DummyOperationalActionSectionLog();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			var sender = new OperationalActionBulkENSMessageSender(declaration, "KNZTEST", "17189588551");
			sender.OperationalActionSendMessage(true, log);
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("Message should be sent", 1, entry.Messages.Count);
			var message = entry.Messages[0];
			var se13 = (ASESE13)message.MessageBlock.MessageBlocks.FirstOrDefault(x => x is ASESE13);
			AssertNotNull(se13);
			AssertEquals("KNZTEST", se13.ContactName);
			AssertEquals("17189588551", se13.ContactPhone);
		}

		[TestDate(2012, 10, 30)]
		public void TestValidateIncludingChildren()
		{
			var referenceTesthelper = new UniversalReferenceTestDataHelper(Factory);
			referenceTesthelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			referenceTesthelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "A001", "Test Name", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var helper = new DeclarationTestHelper();
			var declaration = helper.GetMergedDutiableDeclaration(Factory);
			helper.SetUpImportDeclarationData(declaration);

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = false;
			declaration.US_EnableAII = true;
			declaration.IOROrgPK = declaration.Importer.PK;
			declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.NotApplicable;
			declaration.US_US_NKLocationOfGoods = "A001";
			declaration.JE_MasterBillIssuerSCAC = "APLU";
			declaration.US_7501Purchased = YesNoDefaultList.Codes.No;
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			declaration.US_BondCalcCode = SEBCalculationList.Codes.MAN;
			declaration.US_SuretyCode = "891";
			declaration.US_BondProducerAccNo = "OTT1";
			declaration.US_EntryDate = ZDateTime.Today;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(2);
			declaration.JE_RL_NKPortOfArrival = "USLAX";
			declaration.US_SchDEntry = declaration.US_SchDArrival;
			declaration.JE_DeclarationReference = "B000005";
			declaration.US_CargoReleaseType = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableAII = true;
			declaration.JE_VoyageFlightNo = "US123";

			var orgResult = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeaderWrapper.New(orgResult).ZO_IsEINNumberVerifiedIndicator = "Y";
			orgResult.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199000", Core.Constants.CountryCodes.UnitedStates);
			declaration.JE_OA_SoldToPartyAddress = orgResult.MainAddress.PK;

			declaration.JE_MasterBill = "08155555555";
			var bill = declaration.PrimaryMasterBill;
			bill.CU_NoOfPacks = 10m;
			bill.CU_PackType = ABIUnitOfMeasureList.Codes.Kilograms;

			var invoice = declaration.Invoices[0];
			invoice.JZ_InvoiceNumber = "10";

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_Code = "MAN" + new Random().Next(1000000).ToString();
			manufacturer.OH_FullName = "Mr manufacturer";
			manufacturer.OH_IsConsignor = true;
			manufacturer.OH_RL_NKClosestPort = "AUSYD";
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "AUSOUPAC195PAD", GlbCompany.CurrentCompany.Country);
			manufacturer.MainAddress.OA_City = "TET";
			invoice.JZ_OA_SupplierAddress = manufacturer.MainAddress.PK;
			invoice.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			invoice.US_TransactionsRelated = YesNoDefaultList.Codes.No;

			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.US_SPI = "N/A";
			invoiceLine.JI_Weight = 650m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_NetWeight = 650m;
			invoiceLine.JI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.US_DestinationState = USStatesList.Codes.Alabama;

			declaration.RunPreSaveValidationWithFetchHints();
			var notification = declaration.NotificationsIncludingChildren.ToUniqueMessageListString();

			AssertMultilineASCIIEquals(
				"All warnings and message errors including for AII",
				@"Warning - US_US_NKLocationOfGoods: FIRMS location is not in the same district as Port Of Entry.
Warning - CO_ContainerNumber: A container should be linked to at least one invoice line. Please go to the invoice line tab -> containers sub tab and select appropriate container(s)
Warning - JW_RL_NKLoadPort: TH123 does not have a sea port.
Warning - JE_OA_ConsigneeAddress: You have entered a foreign-based Ultimate Consignee with a foreign-based Importer of Record.
Warning - JE_OH_Importer: This organization may not be on the Customs File. The organization can be added to the Customs File by sending a message from the Customs Messaging menu on the Organization Form. Alternatively, if this organization has previously been added to the Customs File, please indicate this on the Organization Record (Organization > Customs Messaging > Importer/Consignee File (CBPF-5106) Add).
Warning - JE_TotalWeight: Total gross weight on declaration must not be less than the sum of Customs Quantities of individual lines.
Warning - JE_VesselName: Vessel Name is too long. Messages will be sent using the first 20 characters only.
Warning - IOROrgPK: This organization may not be on the Customs File. The organization can be added to the Customs File by sending a message from the Customs Messaging menu on the Organization Form. Alternatively, if this organization has previously been added to the Customs File, please indicate this on the Organization Record (Organization > Customs Messaging > Importer/Consignee File (CBPF-5106) Add).",
				notification);

			var log = new DummyOperationalActionSectionLog();
			var sender = new OperationalActionBulkENSMessageSender(declaration);
			sender.OperationalActionSendMessage(false, log);
			AssertEquals(ZString.Empty, log.MessagesString());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("Message sent even declaration has message errors for AII", 1, entry.Messages.Count);

			entry.Messages.RemoveAndDeleteAllFromTest();
			declaration.InvoiceLines[0].US_SPI = ZString.Empty;
			sender.OperationalActionSendMessage(true, log);
			AssertEquals(true, declaration.HasMessageErrors);
			var msgErrors = declaration.NotificationsIncludingChildren.GetNotifications(CargoWise.EntityFramework.NotificationType.MessageError).ToUniqueMessageListString();
			Assert("Message error from child should be included", msgErrors.Contains("US_SPI: "));
			AssertEquals("Message should be sent", 1, entry.Messages.Count);

			entry.Messages.RemoveAndDeleteAllFromTest();
			declaration.JE_MergeBy = ZString.Empty;
			sender.OperationalActionSendMessage(true, log);
			Assert(log.MessagesString().Contains("WARNING: Job [HL B000005]:\nWARNING: Error - JE_MergeBy: Please enter a merge-by option."));
			AssertEquals(0, entry.Messages.Count);
		}

		public void TestValidateIncludingImportSendingActionValidation()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			//declaration is on statement
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			declaration.AllocateEntryNumber("TEST");
			var statementHeader = Factory.New<CusStatementHeader>();
			var statementLine = statementHeader.StatementLines.AddNew();
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_EntryNum = "TEST";
			Factory.Save();

			var sender = new OperationalActionBulkENSMessageSender(declaration);
			var log = new DummyOperationalActionSectionLog();
			sender.OperationalActionSendMessage(true, log);
			var messageErrorText = ValidationConstants.EntrySummary.AlreadyOnStatement("entry summary");
			AssertContains(messageErrorText, log.MessagesString());

			//declaration filled by other filer 1
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_PSC = true;
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			sender = new OperationalActionBulkENSMessageSender(declaration);
			log = new DummyOperationalActionSectionLog();
			sender.OperationalActionSendMessage(true, log);
			AssertContains(ValidationConstants.EntrySummary.PSCFilingOfEntriesFiledByOtherBroker, log.MessagesString());

			//declaration filled by other filer 2
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_PSC = true;
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_BRDRefNo = "B111";
			declaration.JE_DeclarationReference = "B000005";
			declaration.US_EnableENS = true;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			var entryNumberForTest = "00004677";
			declaration.AllocateEntryNumber(entryNumberForTest);
			var message = Factory.NewWithValidTestData<MQEDIMessage>();
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummary;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageNum = "11";
			message.EM_SystemCreateTimeUtc = ZDateTime.Now;
			entry.Messages.Add(message);

			message.EM_MessageText =
				"B018888XJ5EI                                               " + EDIMessage.MessageNumberPlaceHolder +
				"10A888891-01319900091-013199000                 8070207   XJ5 " + entryNumberForTest + "01891  CO " +
				"20     APL EMERALD         10280908310800005701             V123W083108L888     22            OBL13                               00000001PK         OTT1       30                                  01              1                   OTT1    40001PE00000100000000009000                    000000005060267                  50 08045060400000033000000000500000KG                               PE082208N   51                                                                              60                                        XYBEREQU6LON                          62080450604010800005512                                                         62          49900002100                                                         891080000000551249900000002500                                                  9000000033000           0                       0000000801200000010000          Y  8888XJ5EI00059000000033000";

			var incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.EntrySummaryResponse;
			incomingMessage.EM_MessageNum = "11";
			incomingMessage.EM_MessageText = AcceptedER;
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;
			entry.Messages.Add(incomingMessage);

			sender = new OperationalActionBulkENSMessageSender(declaration);
			log = new DummyOperationalActionSectionLog();
			sender.OperationalActionSendMessage(true, log);
			AssertContains(ValidationConstants.EntrySummary.BrokerReferenceNumberDifferentAndReplacementShouldBeSent("00005701"), log.MessagesString());

			//ACE entry cancelled
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			entry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryCanceled;
			sender = new OperationalActionBulkENSMessageSender(declaration);
			log = new DummyOperationalActionSectionLog();
			sender.OperationalActionSendMessage(true, log);
			AssertContains(ValidationConstants.EntrySummary.HasBeenCancelled, log.MessagesString());
		}

		public void TestLogCustomsCommencedForENS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_DeclarationReference = "B000005";
			declaration.US_EntryFilerCode = "XJ5";
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;

			var sender = new OperationalActionBulkENSMessageSender(declaration);
			var log = new DummyOperationalActionSectionLog();
			sender.OperationalActionSendMessage(true, log);
			Assert(declaration.LogsOfDeclarationOrShipment.HasLogWith(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.CustomsCommencedCode)));
		}

		public void TestMerge()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.JE_DeclarationReference = "B000005";
			declaration.US_EntryFilerCode = "XJ5";

			var sender = new OperationalActionBulkENSMessageSender(declaration);
			var log = new DummyOperationalActionSectionLog();
			sender.OperationalActionSendMessage(true, log);
			Assert(log.MessagesString().Contains("WARNING: Job [HL B000005]: You can't merge this entry because there are no invoice headers."));

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			log = new DummyOperationalActionSectionLog();
			sender.OperationalActionSendMessage(true, log);
			AssertEquals(ZString.Empty, declaration.MergeManager.InvalidOperationText);
			AssertNotNull(declaration.ActiveEntryHeaders.EntrySummaryEntry);
		}

		public void TestSendMessageWithWareHouse()
		{
			DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XJ5");

			var data = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
			addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var currentStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "test@test.com.au";

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMP";
			importer.MiscServ.OM_IMPartAttrib1Name = "VIN1";
			importer.MiscServ.OM_IMPartAttrib1Type = "NON";
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			importer.OH_IsWarehouseClient = true;

			var warehouse1 = Factory.New<OrgHeader>();
			warehouse1.OH_Code = "W1";
			warehouse1.OH_RL_NKClosestPort = "USCHI";
			warehouse1.MainAddress.LocalControlledPremisesID = "23423";

			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_DeclarationReference = "B00000001";
			declaration.WarehouseDocAddress.E2_OA_Address = warehouse1.MainAddress.PK;

			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_LookupCode = "~~1L";
			classification.CC_TariffNum = "4901.10.00 01";

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "~~1";
			part.OP_StockKeepingUnit = "NO";
			part.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			var importPivot = part.PivotsForBinding.AddNew();
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			importPivot.CI_CC = classification.PK;

			var warehouse = Factory.LoadTop1<IWhsWarehouse>(new ZQuery(WhsWarehouseSchema.WW_WarehouseCode, "WHS"));
			if (warehouse == null)
			{
				var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
				warehouse = (IWhsWarehouse)helper.CreateWarehouse(warehouse1.MainAddress.OA_Address1, "WH1", "BOND");
				warehouse.WW_OA_WarehouseAddress = warehouse1.MainAddress.PK;
				warehouse.WW_IsBondedWarehouse = true;
				warehouse.WW_IsVirtualWarehouse = true;
				((IWhsArea)warehouse.Areas[0]).WA_AreaType = "BON";
				warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
				warehouse.WW_AutoPrintPackingSlip = false;
			}

			declaration.Invoices.DeleteAll();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_CustomsUnitQty = "KG";

			invoice.JZ_InvoiceAmount = 250 * 100m;
			invoiceLine.JI_InvoiceQuantity = 0m;
			invoiceLine.JI_CustomsQuantity = 250 * 10m;
			invoiceLine.JI_LinePrice = 250 * 100m;
			invoiceLine.JI_BondedWhsQuantity = 10m;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_WHSEntryFilerCode = "XJ5";
			declaration.US_WHSEntryNumber = "ENT323";
			invoiceLine.US_WHSEntryLineNo = 1;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			declaration.DoMerge();
			Factory.Save();

			var sender = new OperationalActionBulkENSMessageSender(declaration);
			var log = new DummyOperationalActionSectionLog();
			sender.OperationalActionSendMessage(true, log);

			Assert("Should has message error because invoice quantity is 0.", log.MessagesString().Contains("An Invoice Line marked for Inventory Management must have an invoice quantity and an invoice unit of quantity; not all Invoice Lines marked for Inventory Management have an invoice quantity and an invoice unit of quantity specified."));
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("should not send message", entry.Messages.Count, 0);

			invoiceLine.JI_InvoiceQuantity = 500m;
			Factory.Save();
			sender = new OperationalActionBulkENSMessageSender(declaration);
			log = new DummyOperationalActionSectionLog();

			var whsHelper = new WhsDataTestHelper(Factory).WhsHelper;
			using (whsHelper.UseAllocationEngineMock())
			{
				sender.OperationalActionSendMessage(true, log);
			}

			entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			Assert(log.MessagesString().Contains("You do not have enough stock to fulfill shortfalls"));
			AssertEquals("should not send message", entry.Messages.Count, 0);

			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_CargoReleaseType = "ACE";
			Factory.Save();
			declaration.WarehouseTransactionStatus = Enterprise.Customs.Business.WarehouseTransactionStatusList.Codes.OutwardCanceled;
			sender = new OperationalActionBulkENSMessageSender(declaration);
			log = new DummyOperationalActionSectionLog();

			using (whsHelper.UseAllocationEngineMock())
			{
				sender.OperationalActionSendMessage(true, log);
			}

			entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("should send message", entry.Messages.Count, 1);
		}

		public void TestCreateDocPrintingDataForEntrySummary()
		{
			DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XJ5");

			var data = (RegistryItemSet)ObjectFactory.Get("RegistryItemSet_AccountingConfigurationRegistry");
			var addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry = (BooleanRegistryItem)data.FindByName("AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob");
			addJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var currentStaff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentStaff.GS_EmailAddress = "test@test.com.au";

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMP";
			importer.MiscServ.OM_IMPartAttrib1Name = "VIN1";
			importer.MiscServ.OM_IMPartAttrib1Type = "NON";
			importer.CompanyData.OB_IMUsedBondedWhs = true;

			var warehouse = Factory.New<OrgHeader>();
			warehouse.OH_Code = "W1";
			warehouse.OH_RL_NKClosestPort = "USCHI";
			warehouse.MainAddress.LocalControlledPremisesID = "23423";

			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_DeclarationReference = "B00000001";
			declaration.WarehouseDocAddress.E2_OA_Address = warehouse.MainAddress.PK;

			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
			classification.CC_LookupCode = "~~1L";
			classification.CC_TariffNum = "4901.10.00 01";

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "~~1";
			part.OP_StockKeepingUnit = "NO";
			part.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, OrgPartRelation.RelationshipTypes.Owner);
			var importPivot = part.PivotsForBinding.AddNew();
			importPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			importPivot.CI_CC = classification.PK;

			declaration.Invoices.DeleteAll();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_CustomsUnitQty = "KG";

			invoice.JZ_InvoiceAmount = 250 * 100m;
			invoiceLine.JI_InvoiceQuantity = 0m;
			invoiceLine.JI_CustomsQuantity = 250 * 10m;
			invoiceLine.JI_LinePrice = 250 * 100m;
			invoiceLine.JI_BondedWhsQuantity = 10m;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_WHSEntryFilerCode = "XJ5";
			declaration.US_WHSEntryNumber = "ENT323";
			invoiceLine.US_WHSEntryLineNo = 1;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			declaration.DoMerge();
			Factory.Save();

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_CargoReleaseType = "ACE";
			Factory.Save();
			declaration.WarehouseTransactionStatus = Enterprise.Customs.Business.WarehouseTransactionStatusList.Codes.OutwardCanceled;
			var sender = new OperationalActionBulkENSMessageSender(declaration);
			var log = new DummyOperationalActionSectionLog();
			sender.OperationalActionSendMessage(true, log);
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("should send message", entry.Messages.Count, 1);

			IEnumerable<US7501DocPrinting> docPrintings = entry.US7501DocPrintingData.Find(x => x.US_MsgPK == entry.Messages[0].PK);
			var docPrintingData = new List<US7501DocPrinting>(new TypedEnumerable<US7501DocPrinting>(docPrintings));

			AssertEquals(1, docPrintingData.Count);
		}

		public void TestCertifyCRL()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_CertifyCargoRelease = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.CR;
			declaration.JE_DeclarationReference = "B000005";
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			AssertEquals("Precondition - no ENS entry", null, declaration.ActiveEntryHeaders.EntrySummaryEntry);

			var sender = new OperationalActionBulkENSMessageSender(declaration);
			var log = new DummyOperationalActionSectionLog();
			sender.OperationalActionSendMessage(true, log);
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertCertificationFlag(entry.Messages, "Should be certified here, because no certification has been done", 1);
			entry.Messages.RemoveAndDeleteAll();

			declaration.US_EnableCRL = false;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Now;

			sender.OperationalActionSendMessage(true, log);
			AssertCertificationFlag(entry.Messages, "Should not be certified here, because Release Date exists", 0);

			declaration.JE_EntryAuthorisationDate = ZDateTime.Empty;
			declaration.ReleaseStatus = CRLReleaseStatusList.Codes.REL;
			sender.OperationalActionSendMessage(true, log);
			AssertCertificationFlag(entry.Messages, "Should not be certified here, because Release Status = REL", 0);

			declaration.ReleaseStatus = CRLReleaseStatusList.Codes.NRL;
			entry.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.Certified;
			sender.OperationalActionSendMessage(true, log);
			AssertCertificationFlag(entry.Messages, "Should not be certified here, because Entry Cert Status = Certified", 0);

			entry.US_CRLCertStatus = CargoReleaseCertificationStatusList.Codes.CertificationSentAckPending;
			sender.OperationalActionSendMessage(true, log);
			AssertCertificationFlag(entry.Messages, "Should not be certified here, because Entry pending certification", 0);

			entry.US_CRLCertStatus = ZString.Empty;
			declaration.US_CertifyCargoRelease = false;
			sender.OperationalActionSendMessage(true, log);
			AssertCertificationFlag(entry.Messages, "Should not be certified here, because declaration US_CertifyCargoRelease = false", 0);

			declaration.US_CertifyCargoRelease = true;
			sender.OperationalActionSendMessage(true, log);
			AssertCertificationFlag(entry.Messages, "Should be certified", 1);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_CertifyCargoRelease = true;
			declaration.JE_DeclarationReference = "B000006";
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_PSC = true;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_APHISInd = OGAIndicatorList.Codes.Disclaimed;

			sender = new OperationalActionBulkENSMessageSender(declaration);
			sender.OperationalActionSendMessage(true, log);
			entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("No Messages sent, because PSC = true", 0, entry.Messages.Count);

			declaration.US_PSC = false;
			sender.OperationalActionSendMessage(true, log);
			AssertEquals("Message should be sent", 1, entry.Messages.Count);
			var message = entry.Messages[0];

			var block10 = message.MessageBlock.MessageBlocks.OfType<AENS10>().FirstOrDefault();
			AssertEquals("Should be certified", "A", block10.CargoReleaseCertificationRequestIndicator);
			AssertEquals("FDA Status should not be set because no FDA data", ZString.Empty, entry.Declaration.FDAMsgStatus);
			AssertContains(@"50           0000000000 0000000000                                              
OI                                                                              
PG01001APHAVS                                                                   
6249900000000                                                                   ", message.EM_FormattedMessageText);
		}

		public void TestFDAStatusAndDocPrintDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.CR;
			declaration.JE_DeclarationReference = "B000005";
			declaration.US_EntryFilerCode = "XJ5";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			var fda = invoiceLine.FDAs.AddNew();
			fda.US_FDACommercialDesc = "TEST";
			fda.US_FDAQty1 = 12m;

			var sender = new OperationalActionBulkENSMessageSender(declaration);
			var log = new DummyOperationalActionSectionLog();
			sender.OperationalActionSendMessage(true, log);
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("FDA Status should be set because Certify Cargo Release and has FDA data", FDAStatusList.Codes.AWA, entry.Declaration.FDAMsgStatus);
			Assert("US7501DocPrintingData should have been created", entry.US7501DocPrintingData.Count > 0);

			var docData = entry.US7501DocPrintingData[0];
			AssertEquals("Invoice Line PK should be saved", invoiceLine.PK, docData.US_InvoiceLinePK);
		}

		public void TestRunApportionmentInCaseWhenADeclarationIsImportedViaXUS()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_CertifyCargoRelease = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.CR;
			declaration.JE_DeclarationReference = "B000005";
			declaration.US_EntryFilerCode = "XJ5";
			declaration.TopGroupInvoice.Charges.AddNew("OFT", 50m, "USD");

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			invoiceLine.ApportionedCharges.RemoveAndDeleteAll();
			invoice.GroupCharges.RemoveAndDeleteAll();

			string message;
			Assert("Precondition - apportionment imbalance", !declaration.Invoices.AreChargesBalancedForInvoices(out message));
			declaration.ApportionmentDirty = false;

			var sender = new OperationalActionBulkENSMessageSender(declaration);
			var log = new DummyOperationalActionSectionLog();
			sender.OperationalActionSendMessage(true, log);

			Assert("Should have run an apportionment", declaration.Invoices.AreChargesBalancedForInvoices(out message));
		}

		void AssertCertificationFlag(EDIMessageCollection messages, string comments, ZInt expectedResult)
		{
			AssertEquals("Message should be sent", 1, messages.Count);
			var message = messages[0];

			var block30 = message.MessageBlock.MessageBlocks.OfType<ENS30>().FirstOrDefault();
			AssertEquals(comments, expectedResult, block30.ReleaseCertificationCode);
			messages.RemoveAndDeleteAllFromTest();
		}

		const string AcceptedER = "B018888XJ5ER                                               53972                E08888XJ5 70033754B00154240ACCEPTED - RECORDS REQUIRED             21808        E08888XJ5 70033754B00154240CERT-RELEASE CERTIFIED VIA SUMMARY      218082A5     Y  8888XJ5ER00002            000000143723";
	}
}
