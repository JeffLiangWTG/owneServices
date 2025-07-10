using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class ACEBillOfLadingUpdateMessageBuilderTest : TestCaseWithFactory
	{
		public void TestEndToEndTest()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "5678", "Some Domestic Port", startDate, endDate);
			newFactory.Save();

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "ABC" + new Random().Next(1000000).ToString();
			importer.OH_FullName = "Test Importer";
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "69-9999999JC");
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "91-013199000");

			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_OH_Importer = importer.PK;
			declaration.IOROrgPK = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.JE_VesselName = "VESSEL";
			declaration.JE_VoyageFlightNo = "V123W";
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			declaration.JE_RL_NKPortOfLoading = "PERTD";
			declaration.US_SchDEntry = "8888";
			declaration.JE_RL_NKOrigin = "PERTD";
			declaration.US_SchDLoading = "5678";
			declaration.US_SchDArrival = "5678";
			declaration.AdditionalReferenceNumbers.AddNewIfNotExist(Common.UnitedStatesAdditionalReferenceNumberTypes.Codes.RRN, "12345678");

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_OH_Buyer = importer.PK;
			invoiceHeader.JZ_InvoiceAmount = 15000m;
			DeclarationTestHelper.SetupOrganizationsForACEInvoice(invoiceHeader, Factory);

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8471704065";
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_Weight = 120m;

			DeclarationTestHelper.SetupBillsForSimplifiedEntryDeclaration(declaration);
			var primaryMasterBill = declaration.PrimaryMasterBill;
			primaryMasterBill.CU_NoOfPacks = 12;
			primaryMasterBill.CU_PackType = DeclarationTestHelper.BillUS_ManifestUQForTest;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			seEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			Factory.Save();

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			var action = (EntryHeaderMessageSendingAction)actions[0];

			var message = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Update, ACEEntrySummaryMessageSendingOption.New(action)).PopulateMessage();
			AssertMultilineASCIIEquals(@"Expected ACE Cargo Release Bill Of Lading Update Message",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10UXJ5  00000014 01EI 69-9999999JC40800000150008888  5678                     
SE11                                   123W                                     
SE13CARGOWISE SUPPORT                                                           
SE15H    HOUSEBILL3                                        00000011     N       
SE15M    TESTMB1                                                        N       
SE15H    TESTHB1                                                        N       
SE15S    TESTSUBHB1                                        00000130     N       
SE15M    TESTMASTERBI                                                   N       
SE15H    TESTHOUSEBIL                                      00000012     N       
SE20CR B00001000                                                                
SE20RRN12345678                                                                 
Y  8888XJ5SE", message.EM_FormattedMessageText);
		}

		public void TestSendBillsOnly()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "ABC" + new Random().Next(1000000).ToString();
			importer.OH_FullName = "Test Importer";
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "69-9999999JC");

			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8471704065";
			invoiceLine.JI_LinePrice = 15000m;

			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "MB001001";
			masterBill.US_UI_NKBillIssuerSCAC = "OTT1";

			var houseBill = masterBill.ChildBills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "HB001002";
			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "APLU001001";
			var container4 = declaration.CusContainers.AddNew();
			container4.CO_ContainerNumber = "POIY154654";

			declaration.PackingGroups.RemoveAndDeleteAll();
			var packgroup1 = houseBill.PackingGroups.AddNew();
			packgroup1.CR_CO_Container = container3.PK;

			var packgroup2 = houseBill.PackingGroups.AddNew();
			packgroup2.CR_CO_Container = container4.PK;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			seEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			Factory.Save();

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			var action = (EntryHeaderMessageSendingAction)actions[0];

			var message = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Update, ACEEntrySummaryMessageSendingOption.New(action)).PopulateMessage();
			AssertMultilineASCIIEquals(@"No containers sent in the message, only bills",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10UXJ5  00000014 01               4100000015000                               
SE13CARGOWISE SUPPORT                                                           
SE15M    MB001001                                                       N       
SE15H    HB001002                                                       N       
SE20CR B00001000                                                                
Y  8888XJ5SE", message.EM_FormattedMessageText);
		}

		public void TestEntryNumberPlaceholder()
		{
			var declaration = GetMergedDeclaration();
			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			var message = builder.PopulateMessage();
			AssertMultilineASCIIEquals(@"No Entry Number generated yet",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  <E#PLCH> 01               4100000015000                               
SE13CARGOWISE SUPPORT                                                           
SE20CR                                                                          
SE40001                                                                         
SE6084717040650000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);

			Factory.Save();

			message.Reload();
			AssertNotContains(@"Placeholder replaced with an entry number assigned", "<E#PLCH>", message.EM_FormattedMessageText);
		}

		public void TestConveyanceNameForTransportModeFIXInSE16()
		{
			var declaration = GetMergedDeclaration();
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.JE_MasterBill = "TESTMASTER";
			declaration.US_PipelineName = "123456789";

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions.OfType<ImportMessageSendingAction>().ForEach(x => x.US_SendMessage = true);
			actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var seMessage = (MQEDIMessage)declaration.ActiveEntryHeaders.SimplifiedEntry.Messages[0];
			var se16Block = seMessage.MessageBlock.MessageBlocks.OfType<ASESE16>().FirstOrDefault();
			AssertNotNull(se16Block);
			AssertEquals("123456789", se16Block.ConveyanceName);
		}

		public void TestBillOfLadingNumberForBatchTicketNumberInSE15()
		{
			var declaration = GetMergedDeclaration();
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.JE_MasterBill = "BATCHTICKET";
			declaration.US_UI_NKCarrierSCAC = "A001";

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions.OfType<ImportMessageSendingAction>().ForEach(x => x.US_SendMessage = true);
			actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var seMessage = (MQEDIMessage)declaration.ActiveEntryHeaders.SimplifiedEntry.Messages[0];
			var se15Block = seMessage.MessageBlock.MessageBlocks.OfType<ASESE15>().FirstOrDefault();
			AssertNotNull(se15Block);
			AssertEquals("A001BATCHTICKET", se15Block.BillOfLadingNumber);
		}

		public void TestConditionsToSendSE16()
		{
			var declaration = GetMergedDeclaration();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.JE_MasterBill = "TESTMASTER";
			declaration.JE_MasterBillIssuerSCAC = "A2";
			declaration.US_EnableENS = false;
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions.OfType<ImportMessageSendingAction>().ForEach(x => x.US_SendMessage = true);
			actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var seMessage = (MQEDIMessage)declaration.ActiveEntryHeaders.SimplifiedEntry.Messages[0];
			var se16Block = seMessage.MessageBlock.MessageBlocks.OfType<ASESE16>().FirstOrDefault();
			AssertNull(se16Block);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_GeneralOrderNo = "12345567";
			declaration.JE_MasterBillIssuerSCAC = "A3";

			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions.OfType<ImportMessageSendingAction>().ForEach(x => x.US_SendMessage = true);
			actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			seMessage = (MQEDIMessage)declaration.ActiveEntryHeaders.SimplifiedEntry.Messages[1];
			se16Block = seMessage.MessageBlock.MessageBlocks.OfType<ASESE16>().FirstOrDefault();
			AssertNotNull(se16Block);
			AssertEquals("A3", se16Block.CarrierCode);

			declaration.US_GeneralOrderNo = ZString.Empty;
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.JE_MasterBillIssuerSCAC = "A4";

			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions.OfType<ImportMessageSendingAction>().ForEach(x => x.US_SendMessage = true);
			actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			seMessage = (MQEDIMessage)declaration.ActiveEntryHeaders.SimplifiedEntry.Messages[2];
			se16Block = seMessage.MessageBlock.MessageBlocks.OfType<ASESE16>().FirstOrDefault();
			AssertNotNull(se16Block);
			AssertEquals("A4", se16Block.CarrierCode);

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.US_NonAMS = true;
			declaration.JE_MasterBillIssuerSCAC = "A5";

			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions.OfType<ImportMessageSendingAction>().ForEach(x => x.US_SendMessage = true);
			actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			seMessage = (MQEDIMessage)declaration.ActiveEntryHeaders.SimplifiedEntry.Messages[3];
			se16Block = seMessage.MessageBlock.MessageBlocks.OfType<ASESE16>().FirstOrDefault();
			AssertNotNull(se16Block);
			AssertEquals("A5", se16Block.CarrierCode);

			declaration.US_NonAMS = false;

			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions.OfType<ImportMessageSendingAction>().ForEach(x => x.US_SendMessage = true);
			actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			seMessage = (MQEDIMessage)declaration.ActiveEntryHeaders.SimplifiedEntry.Messages[4];
			se16Block = seMessage.MessageBlock.MessageBlocks.OfType<ASESE16>().FirstOrDefault();
			AssertNull(se16Block);
		}

		public void TestBillTypeIndicatorForExpressTracking()
		{
			var declaration = GetMergedDeclaration();
			declaration.JE_MasterBill = "MB1234567";
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions.OfType<ImportMessageSendingAction>().ForEach(x => x.US_SendMessage = true);
			actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var seMessage = (MQEDIMessage)seEntry.Messages[0];
			var se15Block = seMessage.MessageBlock.MessageBlocks.OfType<ASESE15>().FirstOrDefault();
			AssertNotNull(se15Block);
			AssertEquals(SEBillTypesList.Codes.RegularBill, se15Block.BillTypeIndicator);
			AssertEquals("MB1234567", se15Block.BillOfLadingNumber);

			seEntry.Messages.RemoveAndDeleteAll();
			declaration.JE_MasterBillExpressTracking = true;
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions.OfType<ImportMessageSendingAction>().ForEach(x => x.US_SendMessage = true);
			actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			seMessage = (MQEDIMessage)seEntry.Messages[0];
			se15Block = seMessage.MessageBlock.MessageBlocks.OfType<ASESE15>().FirstOrDefault();
			AssertNotNull(se15Block);
			AssertEquals(SEBillTypesList.Codes.ExpressTracking, se15Block.BillTypeIndicator);
			AssertEquals("MB1234567", se15Block.BillOfLadingNumber);

			seEntry.Messages.RemoveAndDeleteAll();
			declaration.JE_MasterBillExpressTracking = false;
			declaration.JE_MasterBill = "MB1234567";
			declaration.JE_HouseBill = "HB898784378";
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions.OfType<ImportMessageSendingAction>().ForEach(x => x.US_SendMessage = true);
			actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			seMessage = (MQEDIMessage)seEntry.Messages[0];
			se15Block = seMessage.MessageBlock.MessageBlocks.OfType<ASESE15>().FirstOrDefault();
			AssertNotNull(se15Block);
			AssertEquals(SEBillTypesList.Codes.MasterBill, se15Block.BillTypeIndicator);
			AssertEquals("MB1234567", se15Block.BillOfLadingNumber);

			seEntry.Messages.RemoveAndDeleteAll();
			declaration.JE_MasterBillExpressTracking = true;
			declaration.LowestBills.Rebuild();
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions.OfType<ImportMessageSendingAction>().ForEach(x => x.US_SendMessage = true);
			actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			seMessage = (MQEDIMessage)seEntry.Messages[0];
			se15Block = seMessage.MessageBlock.MessageBlocks.OfType<ASESE15>().FirstOrDefault();
			AssertNotNull(se15Block);
			AssertEquals(SEBillTypesList.Codes.ExpressTracking, se15Block.BillTypeIndicator);
			AssertEquals("MB1234567", se15Block.BillOfLadingNumber);
		}

		[TestDate(2015, 11, 2)]
		public void TestSendMessageForEntryTypeFTZAndReWarehouse()
		{
			USCustomsDataRegistry.Instance.EnableNCTAsDefaultContainerModeForAirOrTruckDeclaration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.JE_MasterBill = "0012345678";
			declaration.US_FTZNo = "123456";
			declaration.US_UI_NKCarrierSCAC = "AA";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "2402108850";
			tariff.UE_DateFrom = ZDate.Today.AddMonths(-1);
			tariff.UE_DateTo = ZDate.Today.AddMonths(1);

			var invoiceLineOne = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineOne.JI_LinePrice = 15000m;
			invoiceLineOne.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			invoiceLineOne.US_ManifestQty = 123;
			invoiceLineOne.US_FTZCurrentTariff = "2402108850";

			var bill = declaration.Bills[0];
			bill.CU_NoOfPacks = 123m;
			bill.CU_PackType = "PC";
			bill.US_SESplitShip = true;

			var itDetails = bill.ITAndSplitDetails.AddNew();
			itDetails.US_ITNumber = "123456789";
			itDetails.US_NoOfPacks = 100;

			var container = bill.Containers.AddNew();
			container.CO_ContainerNumber = "1223345612";
			container.CO_JE = declaration.PK;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			var message = builder.PopulateMessage();
			AssertMultilineASCIIEquals(@"Transport Mode shuld not be empty in SE10 block, SE15/SE16/S17 should be generated as Entry Type is 06",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 06               4100000015000                               
SE11               FTZ123456                                                    
SE13CARGOWISE SUPPORT                                          1                
SE15I    123456789                                                      N       
SE15R    0012345678                                                     N       
SE16AA             00000100PC                                                   
SE20CR B00001000                                                                
SE40001                                                                         
SE41P      00000123                                                             
SE60          0000015000                                                        
SE612402108850                                                                  
Y  8888XJ5SE", message.EM_FormattedMessageText);

			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			message = builder.PopulateMessage();
			AssertMultilineASCIIEquals(@"For entry type 22, the bill type indicator should be filled 'I'",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 22               4100000015000                               
SE13CARGOWISE SUPPORT                                          1                
SE15I    123456789                                         00000100     N       
SE20CR B00001000                                                                
SE40001                                                                         
SE60          0000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);

			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			message = builder.PopulateMessage();
			AssertMultilineASCIIEquals(@"For entry type 22, the bill type indicator should be filled 'I'",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 22               3100000015000                               
SE13CARGOWISE SUPPORT                                          1                
SE15I    123456789                                                      N       
SE15RAA  0012345678                                                     N       
SE16AA             00000100PC                                                   
SE20CR B00001000                                                                
SE40001                                                                         
SE60          0000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);

			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_DomesticCargo = true;
			collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			message = builder.PopulateMessage();
			AssertMultilineASCIIEquals(@"For entry type 21, the bill type indicator should be filled 'DCI'",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 21               3100000015000                               
SE13CARGOWISE SUPPORT                                          1                
SE15I    123456789                                                      N       
SE15RAA  0012345678                                                     N       
SE16AA             00000100PC                                                   
SE20CR B00001000                                                                
SE20DCIY                                                                        
SE40001                                                                         
SE60          0000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
		}

		public void TestSendMessageForHandCarried()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.PassengerHandCarried;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_UI_NKCarrierSCAC = "A2";
			declaration.JE_DateOfArrival = new ZDate(2016, 06, 29);
			declaration.US_EnableCRL = true;
			declaration.JE_TotalNoOfPacksPackType = "PK";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLineOne = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineOne.JI_LinePrice = 15000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			var message = builder.PopulateMessage();
			AssertMultilineASCIIEquals(@"Hand Carried (MOT60) message generated correctly",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01               6000000015000                               
SE13CARGOWISE SUPPORT                                                           
SE15R    HANDCARRIED                                                    Y       
SE16A2       06291600000001PK                                                   
SE20CR B00001000                                                                
SE40001                                                                         
SE60          0000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
		}

		public void TestSendMessageForAUTPEDROAWithSCAC()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Auto;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MasterBillIssuerSCAC = "4545";
			declaration.US_UI_NKCarrierSCAC = "4545";
			declaration.JE_DateOfArrival = new ZDate(2016, 06, 29);
			declaration.US_EnableCRL = true;
			declaration.JE_TotalNoOfPacksPackType = "PK";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLineOne = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineOne.JI_LinePrice = 15000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			var message = builder.PopulateMessage();
			AssertMultilineASCIIEquals(@"Auto (MOT32) message generated correctly",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01               3200000015000                               
SE13CARGOWISE SUPPORT                                                           
SE15R4545AUTO                                                           Y       
SE164545     06291600000001PK                                                   
SE20CR B00001000                                                                
SE40001                                                                         
SE60          0000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);

			declaration.JE_TransportMode = TransportTypeList.Codes.Pedestrian;
			collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			message = builder.PopulateMessage();
			AssertMultilineASCIIEquals(@"PEDESTRIAN (MOT33) message generated correctly",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01               3300000015000                               
SE13CARGOWISE SUPPORT                                                           
SE15R4545PEDESTRIAN                                                     Y       
SE164545     06291600000001PK                                                   
SE20CR B00001000                                                                
SE40001                                                                         
SE60          0000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			message = builder.PopulateMessage();
			AssertMultilineASCIIEquals(@"ROAD, OTHER (MOT34) message generated correctly",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01               3400000015000                               
SE13CARGOWISE SUPPORT                                                           
SE15R4545ROADOTHER                                                      Y       
SE164545     06291600000001PK                                                   
SE20CR B00001000                                                                
SE40001                                                                         
SE60          0000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
		}

		public void TestSendMessageForAUTPEDROAWithoutSCAC()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Auto;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_UI_NKCarrierSCAC = "";
			declaration.JE_DateOfArrival = new ZDate(2016, 06, 29);
			declaration.US_EnableCRL = true;
			declaration.JE_TotalNoOfPacksPackType = "PK";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLineOne = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLineOne.JI_LinePrice = 15000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			var message = builder.PopulateMessage();
			AssertMultilineASCIIEquals(@"Auto (MOT32) message generated correctly",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01               3200000015000                               
SE13CARGOWISE SUPPORT                                                           
SE15RN/A AUTO                                                           Y       
SE16N/A      06291600000001PK                                                   
SE20CR B00001000                                                                
SE40001                                                                         
SE60          0000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);

			declaration.JE_TransportMode = TransportTypeList.Codes.Pedestrian;
			collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			message = builder.PopulateMessage();
			AssertMultilineASCIIEquals(@"PEDESTRIAN (MOT33) message generated correctly",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01               3300000015000                               
SE13CARGOWISE SUPPORT                                                           
SE15RN/A PEDESTRIAN                                                     Y       
SE16N/A      06291600000001PK                                                   
SE20CR B00001000                                                                
SE40001                                                                         
SE60          0000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);

			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			message = builder.PopulateMessage();
			AssertMultilineASCIIEquals(@"ROAD, OTHER (MOT34) message generated correctly",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01               3400000015000                               
SE13CARGOWISE SUPPORT                                                           
SE15RN/A ROADOTHER                                                      Y       
SE16N/A      06291600000001PK                                                   
SE20CR B00001000                                                                
SE40001                                                                         
SE60          0000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
		}

		public void TestSendMessageForSE20ConsolidatedEntrySummary()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.CargRlsCES, Core.Constants.CountryCodes.UnitedStates, ZDateTime.Today, true))
			{
				DeclarationTestHelper.SetEntryFilerCode("XJ5");
				DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
				declaration.JE_DeclarationReference = "~TestDec";
				declaration.US_EnableCRL = true;
				declaration.US_EntryFilerCode = "XJ5";
				declaration.DecEntryNumber = "71200347";

				var declaration2 = Factory.New<JobDeclaration>();
				declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration2.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
				declaration2.US_EnableCRL = true;
				declaration2.US_ConsolidatedJobNumber = "~TestDec";

				var invoiceHeader = declaration2.Invoices.AddNew();
				var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();

				declaration2.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				Factory.Save();

				var collection = new ImportMessageSendingActionCollection(declaration2, ImportMessageSendingMessageType.Original);
				var seEntry = declaration2.ActiveEntryHeaders.SimplifiedEntry;
				var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
				var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				var message = builder.PopulateMessage();

				AssertContains("SE20CESXJ571200347                                                              ", message.EM_FormattedMessageText);
			}
		}

		public void TestPerishableFlagForSE20()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EnableCRL = true;

			var invoiceHeader = declaration.Invoices.AddNew();

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "AMYT";
			commodity.RH_IsPerishable = true;

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_RH_NKCommodity_Code = commodity.RH_Code;

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			var message = builder.PopulateMessage();

			AssertContains("SE20PERY                                                                        ", message.EM_FormattedMessageText);
		}

		public void TestSendMessageWithSplitDetailsOfDeclarationBillLevel()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_EnableCRL = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			declaration.JE_MasterBill = "MB4534535";
			var bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill1.US_UI_NKBillIssuerSCAC = "APLU";
			bill1.CU_BillNum = "MB4534535";
			bill1.CU_NoOfPacks = 10;
			bill1.CU_PackType = ShippingOrPackingingUnitList.Codes.Case;

			var details1 = bill1.ITAndSplitDetails.AddNew();
			details1.US_ITNumber = "777777770";
			details1.US_NoOfPacks = 2;
			var details2 = bill1.ITAndSplitDetails.AddNew();
			details2.US_ITNumber = "777777781";
			details2.US_NoOfPacks = 3;
			var details3 = bill1.ITAndSplitDetails.AddNew();
			details3.US_ITNumber = "777777792";
			details3.US_NoOfPacks = 5;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			var message = builder.PopulateMessage();

			AssertMultilineASCIIEquals(@"SE15 block qty should not be sum of split details qty",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014                    00000000000                               
SE13CARGOWISE SUPPORT                                                           
SE15I    777777770                                                      N       
SE15RAPLUMB4534535                                         00000002     N       
SE15I    777777781                                                      N       
SE15RAPLUMB4534535                                         00000003     N       
SE15I    777777792                                                      N       
SE15RAPLUMB4534535                                         00000005     N       
SE15R    MB4534535                                                      N       
SE20CR B00001000                                                                
SE40001                                                                         
SE60                                                                            
Y  8888XJ5SE", message.EM_FormattedMessageText);
		}

		JobDeclaration GetMergedDeclaration()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_EnableENS = false;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8471704065";
			invoiceLine.JI_LinePrice = 15000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			return declaration;
		}
	}
}
