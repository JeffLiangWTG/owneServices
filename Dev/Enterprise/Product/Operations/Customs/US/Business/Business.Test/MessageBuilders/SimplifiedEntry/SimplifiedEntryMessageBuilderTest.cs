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
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Constants = Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class SimplifiedEntryMessageBuilderTest : TestCaseWithFactory
	{
		public void TestSendEndToEndWithSanctionsData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedStates);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Russia, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Russia);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Universal.Constants.TariffTypes.HarmonizedSystem);
			var conditionType = helper.CreateOrGetExistingRefCusConditionType(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, UniversalReferenceConstants.TariffConditionTypes.Codes.Mining);

			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.UnitedStates, tariffType.PK, "7601103000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var condition = helper.CreateOrGetExistingRefCusCondition(Core.Constants.CountryCodes.UnitedStates, conditionType.PK, tariff.PK, "Mining Info", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusApplicability(condition, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = "";
			declaration.JE_DeclarationReference = "B12345678";
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.Invoices.AddNew();

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var nmfsLine = invoiceLine1.NMFSLines.AddNew();
			invoiceLine1.US_NMFSCOAInd = OGAIndicatorList.Codes.Declared;
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.COA;
			DeclarationTestHelper.SetupNMFSCOAData(nmfsLine, invoiceLine1, Factory);
			var fishing1 = invoiceLine1.FishingInformations.AddNew();
			fishing1.US_MethodOfHarvest = "VESSEL";
			fishing1.US_VesselName = "VESSEL NAME";
			fishing1.US_VesselCountry = "GB";
			fishing1.US_VesselIMO = "321546";
			fishing1.US_HarvestedCountry = "AU";
			var fishing2 = invoiceLine1.FishingInformations.AddNew();
			fishing2.US_MethodOfHarvest = "HCF";
			fishing2.US_HarvestedCountry = "TH";
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			var mining2 = invoiceLine2.MiningInformations.AddNew();
			mining2.CountryOfMining = "DE";
			var mining3 = invoiceLine2.MiningInformations.AddNew();
			mining3.CountryOfMining = "FR";
			var invoiceLine3 = declaration.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "7601103000";
			invoiceLine3.US_SupTariff = "99038803";
			invoiceLine3.US_UC_NKCountryOfOrigin = "RU";
			invoiceLine3.US_DisclaimSanctions = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			var message = builder.PopulateMessage();
			AssertMultilineASCIIEquals(@"B         SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  <E#PLCH>                    00000000000                               
SE13CARGOWISE SUPPORT                                                           
SE20CR B12345678                                                                
SE20SCEY                                                                        
SE40001US                                                                       
SE601010101010                                                                  
SE6201FSHNG INFOMETHOD OF HARVEST                                               
SE63VESSEL                                                                      
SE6201FSHNG INFOVESSEL NAME                                                     
SE63VESSEL NAME                                                                 
SE6201FSHNG INFOVESSEL FLAG                                                     
SE63GB                                                                          
SE6201FSHNG INFOVESSEL IMO                                                      
SE63321546                                                                      
SE6201FSHNG INFOCOUNTRY OF HARVEST                                              
SE63AU                                                                          
SE6202FSHNG INFOMETHOD OF HARVEST                                               
SE63HCF                                                                         
SE6202FSHNG INFOCOUNTRY OF HARVEST                                              
SE63TH                                                                          
OI                                                                              
PG01001NMFCOA    Y                                                              
PG02P                                                                           
PG05                                                              ADD           
PG06HCFAUCAR                 02152023        GIL                                
PG142NM4123456789                                                               
PG22Y894            COA1                                                        
SE40002                                                                         
SE60                                                                            
SE6201MINE INFO COUNTRY OF MINING                                               
SE63DE                                                                          
SE6202MINE INFO COUNTRY OF MINING                                               
SE63FR                                                                          
SE40003RU                                                                       
SE6099038803                                                                    
SE607601103000          Y                                                       
Y         SE", message.EM_FormattedMessageText);
		}

		public void TestSendEndToEndWithNMFSCOAData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = "";
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.Invoices.AddNew();

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var nmfsLine = invoiceLine1.NMFSLines.AddNew();
			invoiceLine1.US_NMFSCOAInd = OGAIndicatorList.Codes.Declared;
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.COA;
			DeclarationTestHelper.SetupNMFSCOAData(nmfsLine, invoiceLine1, Factory);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			var message = builder.PopulateMessage();

			AssertContains(@"OI                                                                              
PG01001NMFCOA    Y                                                              
PG02P                                                                           
PG05                                                              ADD           
PG06HCFAUCAR                 02152023        GIL                                
PG142NM4123456789                                                               
PG22Y894            COA1                                                        ", message.EM_FormattedMessageText);
		}

		[TestDate(2009, 1, 3)]
		public void TestBuildMessageWithXVVSets()
		{
			var testHelper = new Chapter98HelperTest();
			var declaration = GetMergedDeclaration();
			declaration.JE_TransportMode = "SEA";
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			var invoice = declaration.Invoices[0];
			invoice.JZ_InvoiceAmount = 2000m;
			var invoiceLine01 = invoice.InvoiceLines[0];
			var invoiceLine02 = invoice.InvoiceLines.AddNew();
			var invoiceLine03 = invoice.InvoiceLines.AddNew();

			invoiceLine01.US_SetInd = "X";
			invoiceLine02.US_SetInd = "V";
			invoiceLine03.US_SetInd = "V";
			invoiceLine02.JI_ParentID = invoiceLine01.PK;
			invoiceLine03.JI_ParentID = invoiceLine02.PK;
			Assert(invoiceLine02.IsVParentLine);
			Assert(invoiceLine03.IsVChildLine);

			invoiceLine01.JI_Tariff = testHelper.TestCTariff.UE_Tariff;
			invoiceLine03.JI_Tariff = testHelper.TestCTariff.UE_Tariff;
			invoiceLine01.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine02.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine03.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff;

			invoiceLine01.JI_LinePrice = 0;
			invoiceLine02.JI_LinePrice = 1000m;
			invoiceLine03.JI_LinePrice = 1000m;
			invoiceLine01.JI_CustomsQuantity = 100m;
			invoiceLine02.JI_CustomsQuantity = 80m;
			invoiceLine03.JI_CustomsQuantity = 60m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			var message = builder.PopulateMessage();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals(@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 06EI 69-9999999JC11800000020008888                           
SE13CARGOWISE SUPPORT                                                           
SE20CR B00001000                                                                
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30EX IAN TEST EXPORTER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30SH IAN TEST SHIPPER                                                         
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30DR IAN TEST DISTRIBUTOR                                                     
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30PK IAN TEST PACKAGER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE40001AU                                                                       
SE41       00000000                                                             
SE6099038801                                                                    
SE607601103000                                                                  
SE40002XY                                                                       
SE41       00000000                                                             
SE6099038801                                                                    
SE6099038801                                                                    
SE6076011030000000001000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, false))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals(@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 06EI 69-9999999JC11800000020008888                           
SE13CARGOWISE SUPPORT                                                           
SE20CR B00001000                                                                
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE40001AU                                                                       
SE41       00000000                                                             
SE6099038801                                                                    
SE607601103000                                                                  
SE40002XY                                                                       
SE41       00000000                                                             
SE6099038801                                                                    
SE6099038801                                                                    
SE6076011030000000001000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
		}

		public void TestSendCustomsValueOnClassificationTariffWhenSTNRuleApplies()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "XJ5";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_FormattedTariff = "7404.00.3055";
			invoiceLine.US_SupTariff = "9903.88.03";
			invoiceLine.JI_LinePrice = 1750m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(entry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			var builder = new SimplifiedEntryMessageBuilder(entry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			var messageText = builder.PopulateMessage().EM_FormattedMessageText;
			var expectedSE60 = @"SE6099038803                                                                    
SE6074040030550000001750                                                        ";
			AssertContains("Customs Value is on Classification Tariff", expectedSE60, messageText);
		}

		public void TestTIB9813SecondaryTariffLineSE60Order()
		{
			var testHelper = new Chapter98HelperTest();
			testHelper.ParentLine.US_SupTariff = testHelper.Test9813Tariff.UE_Tariff;
			testHelper.ChildLine.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff;
			testHelper.ParentLine.JI_Tariff = ZString.Empty;
			testHelper.ChildLine.JI_Tariff = testHelper.TestCTariff.UE_Tariff;
			testHelper.ParentLine.JI_LinePrice = ZDecimal.Zero;
			testHelper.ChildLine.JI_LinePrice = 5000m;
			testHelper.Charpter98Job.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			testHelper.Charpter98Job.US_EnableCRL = true;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var collection = new ImportMessageSendingActionCollection(testHelper.Charpter98Job, ImportMessageSendingMessageType.Original);
			var seEntry = testHelper.Charpter98Job.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			action.US_SE_ContactName = "AMY XIANG";
			action.US_SE_ContactPhone = "123456789";
			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			var messageText = builder.PopulateMessage().EM_FormattedMessageText;
			Assert(messageText.Contains(@"SE609813000520                                                                  
SE6099038801                                                                    
SE6076011030000000005000                                                        "));
		}

		public void TestMessageBlock_ConveyanceNameTooLong()
		{
			var generator = new ABIOutputBlockControlGenerator();
			var se11 = new ASESE11();
			string conveyanceName = "HANJIN UNITED KINGDOM";

			se11.ConveyanceNameOrFTZZoneID = conveyanceName;
			generator.AddMessageBlocks(new MessageBlock[] { se11 });

			AssertNoExceptionThrown(() => generator.Serialise());

			bool conveyanceNameTruncated = generator.MessageBlocks[0].GetSerialisedValues().Count(item => item.Value == conveyanceName.Substring(0, 20)) == 1;
			AssertEquals("ConveyanceName length must equal 20 characters.", true, conveyanceNameTruncated);
		}

		public void TestEndToEndTest()
		{
			var declaration = GetMergedDeclaration();
			declaration.US_US_NKCentralizedExamSite = "A002";
			declaration.InvoiceLines[0].JI_Description = "WOMENSWEAR CASCADING FLRL MINI IPAD FOLD 40% POLYURETHANE, 30% POLYPROPYLENE, 30% POLYESTER";
			var line = declaration.InvoiceLines[0];
			line.US_ZoneStatus = ZoneStatusList.Codes.Domestic;

			DeclarationTestHelper.SetupBillsForSimplifiedEntryDeclaration(declaration);
			var primaryMasterBill = declaration.PrimaryMasterBill;
			primaryMasterBill.CU_NoOfPacks = 12;
			primaryMasterBill.CU_PackType = DeclarationTestHelper.BillUS_ManifestUQForTest;

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			action.US_SE_ContactName = "AMY XIANG";
			action.US_SE_ContactPhone = "123456789";

			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));

			var message = builder.PopulateMessage();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals(@"Expected Message:
			- for Consignee, only SE30 record required and only number should be sent, no name and address
			- blocks 30-36 should be generated for other entity types",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01EI 69-9999999JC40800000150008888                           
SE11           A002                    123W                                     
SE13AMY XIANG                               123456789                           
SE15H    HOUSEBILL3                                        00000011     N       
SE15M    TESTMB1                                                        N       
SE15H    TESTHB1                                                        N       
SE15S    TESTSUBHB1                                        00000130     N       
SE15M    TESTMASTERBI                                                   N       
SE15H    TESTHOUSEBIL                                      00000012     N       
SE20CR B00001000                                                                
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30EX IAN TEST EXPORTER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30SH IAN TEST SHIPPER                                                         
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30DR IAN TEST DISTRIBUTOR                                                     
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30PK IAN TEST PACKAGER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, false))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals(@"Expected Message:
			- for Consignee, only SE30 record required and only number should be sent, no name and address
			- blocks 30-36 should be generated for other entity types except for GBI types",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01EI 69-9999999JC40800000150008888                           
SE11           A002                    123W                                     
SE13AMY XIANG                               123456789                           
SE15H    HOUSEBILL3                                        00000011     N       
SE15M    TESTMB1                                                        N       
SE15H    TESTHB1                                                        N       
SE15S    TESTSUBHB1                                        00000130     N       
SE15M    TESTMASTERBI                                                   N       
SE15H    TESTHOUSEBIL                                      00000012     N       
SE20CR B00001000                                                                
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}

			var rcvMessage = Factory.New<MQEDIMessage>();
			rcvMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			rcvMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			rcvMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			rcvMessage.EM_MessageNum = "HYEDUSCMT_196571";
			rcvMessage.EM_MessageText =
@"B001101SV9SX                                               HYEDUSCMT_196571     " +
"SE10ASV9  00000014 01EI 58-12345678911800000100001101  1101                     " +
"SE15RAPLUMST0802186                                        00000010     N       " +
"SE20CR B00173079                                                                " +
"SE9002   SE DATA ACCEPTED                                                       " +
"Y  1101SV9SX00004";
			rcvMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-1);
			seEntry.Messages.Add(rcvMessage);

			line.US_ZoneStatus = ZString.Empty;
			declaration.US_SESplitRel = SplitShipmentReleaseCodeList.Codes.RequestSpecialPermit;
			primaryMasterBill.US_SESplitShip = true;
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			var sendingAction = new EntryHeaderMessageSendingAction(declaration.ActiveEntryHeaders.SimplifiedEntry, ImportMessageStatusList.MessageType.ACECargoRelease, actions);
			sendingAction.US_SE_ContactName = "Johnny B. Broker";
			sendingAction.US_SE_ContactPhone = "555-0100";
			sendingAction.US_SE_DISIndicator = true;
			sendingAction.US_SE_DISIDRefNo = "TEST.TXT";

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				builder = new SimplifiedEntryMessageBuilder(declaration.ActiveEntryHeaders.SimplifiedEntry, UpdateActionCode.Replace, ACEEntrySummaryMessageSendingOption.New(sendingAction));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals("Replace Message",
	@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10RXJ5  00000014 01EI 69-9999999JC40800000150008888 2                         
SE11           A002                    123W                                     
SE13JOHNNY B. BROKER                        555-0100          11                
SE15H    HOUSEBILL3                                        00000011     N       
SE15M    TESTMB1                                                        N       
SE15H    TESTHB1                                                        N       
SE15S    TESTSUBHB1                                        00000130     N       
SE15M    TESTMASTERBI                                                   N       
SE15H    TESTHOUSEBIL                                      00000012     N       
SE20CR B00001000                                                                
SE20DISTEST.TXT                                                                 
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30EX IAN TEST EXPORTER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30SH IAN TEST SHIPPER                                                         
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30DR IAN TEST DISTRIBUTOR                                                     
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30PK IAN TEST PACKAGER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE40001AU WOMENSWEAR CASCADING FLRL MINI IPAD FOLD 40% POLYURETHANE, 30% POLYPRO
SE6084717040650000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, false))
			{
				builder = new SimplifiedEntryMessageBuilder(declaration.ActiveEntryHeaders.SimplifiedEntry, UpdateActionCode.Replace, ACEEntrySummaryMessageSendingOption.New(sendingAction));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals("Replace Message",
	@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10RXJ5  00000014 01EI 69-9999999JC40800000150008888 2                         
SE11           A002                    123W                                     
SE13JOHNNY B. BROKER                        555-0100          11                
SE15H    HOUSEBILL3                                        00000011     N       
SE15M    TESTMB1                                                        N       
SE15H    TESTHB1                                                        N       
SE15S    TESTSUBHB1                                        00000130     N       
SE15M    TESTMASTERBI                                                   N       
SE15H    TESTHOUSEBIL                                      00000012     N       
SE20CR B00001000                                                                
SE20DISTEST.TXT                                                                 
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE40001AU WOMENSWEAR CASCADING FLRL MINI IPAD FOLD 40% POLYURETHANE, 30% POLYPRO
SE6084717040650000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			sendingAction = new EntryHeaderMessageSendingAction(declaration.ActiveEntryHeaders.SimplifiedEntry, ImportMessageStatusList.MessageType.ACECargoRelease, actions);
			sendingAction.US_SE_ContactName = "Johnny B. Broker";
			sendingAction.US_SE_ContactPhone = "555-0100";
			sendingAction.US_SE_ReasonCode = "01";
			sendingAction.US_SE_MultipleDispositionsIndic = true;
			sendingAction.US_SE_DISIndicator = true;
			sendingAction.US_SE_DISIDRefNo = "TEST.TXT";
			builder = new SimplifiedEntryMessageBuilder(declaration.ActiveEntryHeaders.SimplifiedEntry, UpdateActionCode.Delete, ACEEntrySummaryMessageSendingOption.New(sendingAction));

			message = builder.PopulateMessage();
			AssertMultilineASCIIEquals("For delete message block 10 with Entry Filer and Entry Number and block 13 should be sent",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10DXJ5  00000014 01EI 69-9999999JC40800000150008888 2                         
SE13JOHNNY B. BROKER                        555-0100       01111                
SE20CR B00001000                                                                
SE20DISTEST.TXT                                                                 
Y  8888XJ5SE", message.EM_FormattedMessageText);
		}

		public void TestBuildDeleteMessageWithoutAcceptedMessage()
		{
			var declaration = GetMergedDeclaration();
			declaration.US_US_NKCentralizedExamSite = "A002";
			declaration.InvoiceLines[0].JI_Description = "WOMENSWEAR CASCADING FLRL MINI IPAD FOLD 40% POLYURETHANE, 30% POLYPROPYLENE, 30% POLYESTER";
			var line = declaration.InvoiceLines[0];
			line.US_ZoneStatus = ZoneStatusList.Codes.Domestic;

			DeclarationTestHelper.SetupBillsForSimplifiedEntryDeclaration(declaration);
			var primaryMasterBill = declaration.PrimaryMasterBill;
			primaryMasterBill.CU_NoOfPacks = 12;
			primaryMasterBill.CU_PackType = DeclarationTestHelper.BillUS_ManifestUQForTest;

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			action.US_SE_ContactName = "AMY XIANG";
			action.US_SE_ContactPhone = "123456789";

			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));

			var message = builder.PopulateMessage();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals(@"Expected Message:
			- for Consignee, only SE30 record required and only number should be sent, no name and address
			- blocks 30-36 should be generated for other entity types",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01EI 69-9999999JC40800000150008888                           
SE11           A002                    123W                                     
SE13AMY XIANG                               123456789                           
SE15H    HOUSEBILL3                                        00000011     N       
SE15M    TESTMB1                                                        N       
SE15H    TESTHB1                                                        N       
SE15S    TESTSUBHB1                                        00000130     N       
SE15M    TESTMASTERBI                                                   N       
SE15H    TESTHOUSEBIL                                      00000012     N       
SE20CR B00001000                                                                
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30EX IAN TEST EXPORTER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30SH IAN TEST SHIPPER                                                         
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30DR IAN TEST DISTRIBUTOR                                                     
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30PK IAN TEST PACKAGER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, false))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals(@"Expected Message:
			- for Consignee, only SE30 record required and only number should be sent, no name and address
			- blocks 30-36 should be generated for other entity types except for GBI types",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01EI 69-9999999JC40800000150008888                           
SE11           A002                    123W                                     
SE13AMY XIANG                               123456789                           
SE15H    HOUSEBILL3                                        00000011     N       
SE15M    TESTMB1                                                        N       
SE15H    TESTHB1                                                        N       
SE15S    TESTSUBHB1                                        00000130     N       
SE15M    TESTMASTERBI                                                   N       
SE15H    TESTHOUSEBIL                                      00000012     N       
SE20CR B00001000                                                                
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}

			line.US_ZoneStatus = ZString.Empty;
			declaration.US_SESplitRel = SplitShipmentReleaseCodeList.Codes.RequestSpecialPermit;
			primaryMasterBill.US_SESplitShip = true;
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			var sendingAction = new EntryHeaderMessageSendingAction(declaration.ActiveEntryHeaders.SimplifiedEntry, ImportMessageStatusList.MessageType.ACECargoRelease, actions);
			sendingAction.US_SE_ContactName = "Johnny B. Broker";
			sendingAction.US_SE_ContactPhone = "555-0100";
			sendingAction.US_SE_DISIndicator = true;
			sendingAction.US_SE_DISIDRefNo = "TEST.TXT";

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				builder = new SimplifiedEntryMessageBuilder(declaration.ActiveEntryHeaders.SimplifiedEntry, UpdateActionCode.Replace, ACEEntrySummaryMessageSendingOption.New(sendingAction));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals("Replace Message",
	@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10RXJ5  00000014 01EI 69-9999999JC40800000150008888 2                         
SE11           A002                    123W                                     
SE13JOHNNY B. BROKER                        555-0100          11                
SE15H    HOUSEBILL3                                        00000011     N       
SE15M    TESTMB1                                                        N       
SE15H    TESTHB1                                                        N       
SE15S    TESTSUBHB1                                        00000130     N       
SE15M    TESTMASTERBI                                                   N       
SE15H    TESTHOUSEBIL                                      00000012     N       
SE20CR B00001000                                                                
SE20DISTEST.TXT                                                                 
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30EX IAN TEST EXPORTER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30SH IAN TEST SHIPPER                                                         
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30DR IAN TEST DISTRIBUTOR                                                     
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30PK IAN TEST PACKAGER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE40001AU WOMENSWEAR CASCADING FLRL MINI IPAD FOLD 40% POLYURETHANE, 30% POLYPRO
SE6084717040650000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, false))
			{
				builder = new SimplifiedEntryMessageBuilder(declaration.ActiveEntryHeaders.SimplifiedEntry, UpdateActionCode.Replace, ACEEntrySummaryMessageSendingOption.New(sendingAction));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals("Replace Message",
	@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10RXJ5  00000014 01EI 69-9999999JC40800000150008888 2                         
SE11           A002                    123W                                     
SE13JOHNNY B. BROKER                        555-0100          11                
SE15H    HOUSEBILL3                                        00000011     N       
SE15M    TESTMB1                                                        N       
SE15H    TESTHB1                                                        N       
SE15S    TESTSUBHB1                                        00000130     N       
SE15M    TESTMASTERBI                                                   N       
SE15H    TESTHOUSEBIL                                      00000012     N       
SE20CR B00001000                                                                
SE20DISTEST.TXT                                                                 
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE40001AU WOMENSWEAR CASCADING FLRL MINI IPAD FOLD 40% POLYURETHANE, 30% POLYPRO
SE6084717040650000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			sendingAction = new EntryHeaderMessageSendingAction(declaration.ActiveEntryHeaders.SimplifiedEntry, ImportMessageStatusList.MessageType.ACECargoRelease, actions);
			sendingAction.US_SE_ContactName = "Johnny B. Broker";
			sendingAction.US_SE_ContactPhone = "555-0100";
			sendingAction.US_SE_ReasonCode = "01";
			sendingAction.US_SE_MultipleDispositionsIndic = true;
			sendingAction.US_SE_DISIndicator = true;
			sendingAction.US_SE_DISIDRefNo = "TEST.TXT";
			builder = new SimplifiedEntryMessageBuilder(declaration.ActiveEntryHeaders.SimplifiedEntry, UpdateActionCode.Delete, ACEEntrySummaryMessageSendingOption.New(sendingAction));

			message = builder.PopulateMessage();
			AssertMultilineASCIIEquals("For delete message block 10 with Entry Filer and Entry Number and block 13 should be sent",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10DXJ5  00000014 01EI 69-9999999JC40800000150008888 2                         
SE13JOHNNY B. BROKER                        555-0100       01111                
SE20CR B00001000                                                                
SE20DISTEST.TXT                                                                 
Y  8888XJ5SE", message.EM_FormattedMessageText);
		}

		public void TestBuildDeletionMessageWithAcceptedMesssage()
		{
			var declaration = GetMergedDeclaration();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_US_NKCentralizedExamSite = "A002";
			declaration.InvoiceLines[0].JI_Description = "WOMENSWEAR CASCADING FLRL MINI IPAD FOLD 40% POLYURETHANE, 30% POLYPROPYLENE, 30% POLYESTER";
			var line = declaration.InvoiceLines[0];
			line.US_ZoneStatus = ZoneStatusList.Codes.Domestic;

			DeclarationTestHelper.SetupBillsForSimplifiedEntryDeclaration(declaration);
			var primaryMasterBill = declaration.PrimaryMasterBill;
			primaryMasterBill.CU_NoOfPacks = 12;
			primaryMasterBill.CU_PackType = DeclarationTestHelper.BillUS_ManifestUQForTest;

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			action.US_SE_ContactName = "AMY XIANG";
			action.US_SE_ContactPhone = "123456789";

			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));

			var message = builder.PopulateMessage();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals(@"Expected Message:
			- for Consignee, only SE30 record required and only number should be sent, no name and address
			- blocks 30-36 should be generated for other entity types",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01EI 69-9999999JC40800000150008888                           
SE11           A002                    123W                                     
SE13AMY XIANG                               123456789                           
SE15H    HOUSEBILL3                                        00000011     N       
SE15M    TESTMB1                                                        N       
SE15H    TESTHB1                                                        N       
SE15S    TESTSUBHB1                                        00000130     N       
SE15M    TESTMASTERBI                                                   N       
SE15H    TESTHOUSEBIL                                      00000012     N       
SE20CR B00001000                                                                
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30EX IAN TEST EXPORTER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30SH IAN TEST SHIPPER                                                         
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30DR IAN TEST DISTRIBUTOR                                                     
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30PK IAN TEST PACKAGER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, false))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals(@"Expected Message:
			- for Consignee, only SE30 record required and only number should be sent, no name and address
			- blocks 30-36 should be generated for other entity types except for GBI types",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01EI 69-9999999JC40800000150008888                           
SE11           A002                    123W                                     
SE13AMY XIANG                               123456789                           
SE15H    HOUSEBILL3                                        00000011     N       
SE15M    TESTMB1                                                        N       
SE15H    TESTHB1                                                        N       
SE15S    TESTSUBHB1                                        00000130     N       
SE15M    TESTMASTERBI                                                   N       
SE15H    TESTHOUSEBIL                                      00000012     N       
SE20CR B00001000                                                                
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}

			var rcvMessage = Factory.New<MQEDIMessage>();
			rcvMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			rcvMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			rcvMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			rcvMessage.EM_MessageNum = "HYEDUSCMT_196571";
			rcvMessage.EM_MessageText =
@"B001101SV9SX                                               HYEDUSCMT_196571     " +
"SE10ASV9  00000014 01EI 58-12345678911800000100001101  1101                     " +
"SE15RAPLUMST0802186                                        00000010     N       " +
"SE20CR B00173079                                                                " +
"SE9002   SE DATA ACCEPTED                                                       " +
"Y  1101SV9SX00004";
			rcvMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-1);
			seEntry.Messages.Add(rcvMessage);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			var sendingAction = new EntryHeaderMessageSendingAction(declaration.ActiveEntryHeaders.SimplifiedEntry, ImportMessageStatusList.MessageType.ACECargoRelease, actions);
			sendingAction.US_SE_ContactName = "Johnny B. Broker";
			sendingAction.US_SE_ContactPhone = "555-0100";
			sendingAction.US_SE_ReasonCode = "01";
			sendingAction.US_SE_MultipleDispositionsIndic = true;
			sendingAction.US_SE_DISIndicator = true;
			sendingAction.US_SE_DISIDRefNo = "TEST.TXT";

			builder = new SimplifiedEntryMessageBuilder(declaration.ActiveEntryHeaders.SimplifiedEntry, UpdateActionCode.Delete, ACEEntrySummaryMessageSendingOption.New(sendingAction));

			message = builder.PopulateMessage();
			AssertMultilineASCIIEquals("For delete message block 10 with Entry Filer and Entry Number and block 13 should be sent",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10DXJ5  00000014 02EI 69-9999999JC40800000150008888                           
SE13JOHNNY B. BROKER                        555-0100       0111                 
SE20CR B00001000                                                                
SE20DISTEST.TXT                                                                 
Y  8888XJ5SE", message.EM_FormattedMessageText);
		}

		[TestDate(2009, 1, 3)]
		public void TestBuildMessageWithSecondaryLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6104.22.0010";
			AssertNotNull(invoiceLine.ImportTariff);

			invoiceLine.JI_CustomsQuantity = 150m;
			invoiceLine.JI_CustomsSecondQuantity = 250m;

			var invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine2.JI_Tariff = "6102.20.0020";
			AssertNotNull(invoiceLine2.ImportTariff);

			invoiceLine2.JI_CustomsQuantity = 150m;
			invoiceLine2.JI_CustomsSecondQuantity = 250m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("PreCondition", invoiceLine.CusEntryLine.CL_LineNumber, invoiceLine2.CusEntryLine.CL_LineNumber);
			Assert("PreCondition", invoiceLine.CusEntryLine.ChildSecondaryEntryLines.Contains(invoiceLine2.CusEntryLine));

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			AssertEquals(1, actions.Count);

			var action = actions[0];
			action.US_SendMessage = true;
			actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.SimplifiedEntry.Messages[0];
			var se60s = message.MessageBlock.MessageBlocks.FindAll(x => x is ASESE60).Cast<ASESE60>();
			AssertEquals(2, se60s.Count());
			AssertEquals("6104220010", se60s.ElementAt(0).HTSNumber);
			AssertEquals("6102200020", se60s.ElementAt(1).HTSNumber);
		}

		public void TestBuildMessageWithSecondaryLinesForSupTariff()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;

			declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "6104.22.0010";
			invoiceLine.SupTariffFormatted = "9802.00.4040";
			invoiceLine.JI_CustomsQuantity = 150m;
			invoiceLine.JI_CustomsSecondQuantity = 250m;
			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;

			var atfLine = invoiceLine.ATFLines.AddNew();
			atfLine.US_Quantity = 100m;
			atfLine.US_CategoryCode = "API";
			atfLine.US_ExtendedDescription = "DISCRIPTION";
			atfLine.US_FFLNumber = "1-23-456-78-9A-01234";
			atfLine.US_FELNumber = "1-23-456-78-9A-01234";
			atfLine.US_PermitNumber = "123456789";
			atfLine.US_AECANumber = "A-12-345-6789";
			atfLine.US_Model = "MODEL";
			atfLine.US_CaliberGaugeSize = "CALIBER";
			atfLine.US_BarrelLength = 1800m;
			atfLine.US_OverallLength = 1800m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			AssertEquals(1, actions.Count);

			var action = actions[0];
			action.US_SendMessage = true;
			actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.SimplifiedEntry.Messages[0];
			var messageText = message.EM_MessageText;

			var pg10s = message.MessageBlock.MessageBlocks.FindAll(x => x is AEPAPG10).Cast<AEPAPG10>();
			AssertEquals("Invoice line with SupTariff should only generate 1 PGA block", 1, pg10s.Count());
		}

		public void TestSendPGADetails()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableCRL = true;
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			declaration.US_PreparerDistrictPort = "3910";
			declaration.US_SchDEntry = "4601";

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8407909060";
			AssertNotNull(invoiceLine.ImportTariff);

			invoiceLine.JI_CustomsQuantity = 150m;
			invoiceLine.JI_Description = "Test";
			var vehicleLine = invoiceLine.VehicleLines.AddNew();
			vehicleLine.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			vehicleLine.US_ModelYear = "2010";
			vehicleLine.US_ImportCode = "A";
			vehicleLine.US_VehicleModel = "TEST";

			var vehicleDetailLine = vehicleLine.VehicleAndEngineDetails.AddNew();
			vehicleDetailLine.US_BuildMonth = "01";
			vehicleDetailLine.US_BuildYear = "2010";
			vehicleDetailLine.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			vehicleDetailLine.US_IdentityNumber = "324978423789";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			AssertEquals(1, actions.Count);

			var action = actions[0];
			action.US_SendMessage = true;
			actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.SimplifiedEntry.Messages[0];
			var oi = message.MessageBlock.MessageBlocks.FindAll(x => x is MessageBuildingBlocks.Common.AENSOI);
			AssertNotNull(oi);

			var messageText = message.EM_MessageText;
			AssertContains("B  4601XJ5SE                                  3910XJ5  1   ", messageText);
		}

		public void TestBuildMessageWithAdditionalDetails()
		{
			var declaration = GetMergedDeclaration();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EstimatedEntryDate = ZDate.BrettsBirthday;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondAmount = 150;
			declaration.US_SuretyCode = "891";
			declaration.ImporterWrapper.ZO_KnwImpInd = "Y";
			declaration.US_ExpConsign = "Y";

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease,
				collection);
			action.US_SE_ContactName = "AMY XIANG";
			action.US_SE_ContactPhone = "123456789";

			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			var message = builder.PopulateMessage();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals("Test with additional details",
				@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01EI 69-9999999JC40900000150008888                           
SE11                                   123W                                     
SE13AMY XIANG                               123456789                           
SE20CR B00001000                                                                
SE20V1 891                                                                      
SE20AMT150                                                                      
SE20EXPY                                                                        
SE20KIIY                                                                        
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30EX IAN TEST EXPORTER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30SH IAN TEST SHIPPER                                                         
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30DR IAN TEST DISTRIBUTOR                                                     
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30PK IAN TEST PACKAGER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE40001AU                                                                       
SE6084717040650000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, false))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals("Test with additional details",
				@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01EI 69-9999999JC40900000150008888                           
SE11                                   123W                                     
SE13AMY XIANG                               123456789                           
SE20CR B00001000                                                                
SE20V1 891                                                                      
SE20AMT150                                                                      
SE20EXPY                                                                        
SE20KIIY                                                                        
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE40001AU                                                                       
SE6084717040650000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
		}

		public void TestEndToEndTestWithLineLevelEntities()
		{
			var declaration = GetMergedDeclaration();

			var ultimateConsignee = Factory.New<OrgHeader>();

			ultimateConsignee.OH_Code = "UC" + new Random().Next(1000000).ToString();
			ultimateConsignee.OH_FullName = "ultimateConsignee1";
			ultimateConsignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EncryptedConsigneeNumber, "-23GGFRD234");

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_FullName = "Manufacturer For Line 1";
			manufacturer.OH_Code = "MAN" + new Random().Next(1000000).ToString();
			var address = manufacturer.Addresses.AddNew();
			address.OA_Address1 = "Test man for line 1";
			address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "BHNHJKMEQU6LFR");

			var invoiceLine1 = declaration.InvoiceLines[0];
			invoiceLine1.JI_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;
			invoiceLine1.JI_OA_ManufacturerAddress = address.PK;
			invoiceLine1.US_UC_NKCountryOfOrigin = "BH";

			var foreignExporter = Factory.New<OrgHeader>();
			foreignExporter.OH_FullName = "Foreign Exporter";
			foreignExporter.OH_Code = "FE" + new Random().Next(1000000).ToString();
			address = foreignExporter.Addresses.AddNew();
			address.OA_Address1 = "Test FE for line 1";
			invoiceLine1.JI_OA_ExporterAddress = address.PK;

			var soldToParty = Factory.New<OrgHeader>();
			soldToParty.OH_FullName = "Sold To Party";
			soldToParty.OH_Code = "STP" + new Random().Next(1000000).ToString();
			soldToParty.OH_RL_NKClosestPort = "MXMEX";
			address = soldToParty.MainAddress;
			address.OA_Address1 = "Sold To Party Address 1";
			address.OA_Address2 = "STP Address 2";
			address.OA_City = "Mexico city";
			address.OA_PostCode = "05064";
			soldToParty.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "01-17900612");
			invoiceLine1.JI_OA_SoldToPartyAddress = soldToParty.MainAddress.PK;

			var ultimateConsignee2 = Factory.New<OrgHeader>();
			ultimateConsignee2.OH_Code = "UC" + new Random().Next(1000000).ToString();
			ultimateConsignee2.OH_FullName = "ultimateConsignee2";
			ultimateConsignee2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "BBBBBB123");

			var manufacturer2 = Factory.New<OrgHeader>();
			manufacturer2.OH_FullName = "Manufacturer For Line 2";
			manufacturer2.OH_Code = "MAN" + new Random().Next(1000000).ToString();
			var address2 = manufacturer2.Addresses.AddNew();
			address2.OA_Address1 = "Test man for line 2";
			address2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "FR034FREQU6LBH");

			var invoiceLine2 = declaration.Invoices[0].InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "3920995000";
			invoiceLine2.JI_OA_ConsigneeAddress = ultimateConsignee2.MainAddress.PK;
			invoiceLine2.JI_OA_ManufacturerAddress = address2.PK;
			invoiceLine2.US_UC_NKCountryOfOrigin = "FR";

			var invoiceHeader2 = declaration.Invoices.AddNew();

			var header2_consignee = Factory.New<OrgHeader>();
			header2_consignee.OH_Code = "IMP" + new Random().Next(1000000).ToString();
			header2_consignee.OH_FullName = "Consignee for Inv Header 2";
			header2_consignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-12345678");
			invoiceHeader2.JZ_OA_ConsigneeAddress = header2_consignee.MainAddress.PK;

			var header2_manufacturer = Factory.New<OrgHeader>();
			header2_manufacturer.OH_FullName = "Test Manufacturer for Inv Header 2";
			header2_manufacturer.OH_Code = "MAN" + new Random().Next(1000000).ToString();
			header2_manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "FRHJK78HJHUI");
			invoiceHeader2.JZ_OA_ManufacturerAddress = header2_manufacturer.MainAddress.PK;

			var header2_seller = Factory.New<OrgHeader>();
			header2_seller.OH_FullName = "Seller for Inv Header 2";
			header2_seller.OH_Code = "FE" + new Random().Next(1000000).ToString();
			header2_seller.MainAddress.OA_Address1 = "123/33 Street 1";
			invoiceHeader2.JZ_OA_SellerAddress = header2_seller.MainAddress.PK;

			var header2_soldToParty = Factory.New<OrgHeader>();
			header2_soldToParty.OH_FullName = "Sold To Party for Inv Header 2";
			header2_soldToParty.OH_Code = "STP" + new Random().Next(1000000).ToString();
			header2_soldToParty.OH_RL_NKClosestPort = "USLAX";
			header2_soldToParty.MainAddress.OA_Address1 = "228/45 FLOWER ST";
			header2_soldToParty.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "98-12345612");
			invoiceHeader2.JZ_OA_SoldToPartyAddress = header2_soldToParty.MainAddress.PK;

			var invoiceLine_Header2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine_Header2.JI_Tariff = "3920995000";
			invoiceLine_Header2.JI_OA_ConsigneeAddress = ultimateConsignee2.MainAddress.PK;
			invoiceLine_Header2.JI_OA_ManufacturerAddress = address2.PK;
			invoiceLine_Header2.US_UC_NKCountryOfOrigin = "FR";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			action.US_SE_ContactName = "AMY XIANG";
			action.US_SE_ContactPhone = "123456789";

			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));

			var message = builder.PopulateMessage();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals(@"Expected Message: 
			 - Other Organizations should be on line level, because they are different from Invoice Orgs",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01EI 69-9999999JC40800000150008888                           
SE11                                   123W                                     
SE13AMY XIANG                               123456789                           
SE20CR B00001000                                                                
SE40001BH                                                                       
SE50MF MANUFACTURER FOR LINE 1                                                  
SE5515TEST MAN FOR LINE 1                                                       
SE56                                                           US               
SE50CN ULTIMATECONSIGNEE1                                                       
SE5515***ADDRESS NOT ON FILE***                                                 
SE56                                                           US               
SE50BY                                       01-17900612                        
SE50SE TEST SELLING PARTY(SELLER)                                               
SE5515***ADDRESS NOT ON FILE***                                                 
SE56                                                           US               
SE50ST IAN TEST SHIP TO PARTY                                                   
SE5515***ADDRESS NOT ON FILE***                                                 
SE56                                                           US               
SE50EX IAN TEST EXPORTER                                                        
SE5515***ADDRESS NOT ON FILE***                                                 
SE56                                                           US               
SE50SH IAN TEST SHIPPER                                                         
SE5515***ADDRESS NOT ON FILE***                                                 
SE56                                                           US               
SE50DR IAN TEST DISTRIBUTOR                                                     
SE5515***ADDRESS NOT ON FILE***                                                 
SE56                                                           US               
SE50PK IAN TEST PACKAGER                                                        
SE5515***ADDRESS NOT ON FILE***                                                 
SE56                                                           US               
SE6084717040650000015000                                                        
SE40002FR                                                                       
SE50MF MANUFACTURER FOR LINE 2                                                  
SE5515TEST MAN FOR LINE 2                                                       
SE56                                                           US               
SE50CN                                       BBBBBB123                          
SE50BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE5515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE56GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE50SE TEST SELLING PARTY(SELLER)                                               
SE5515***ADDRESS NOT ON FILE***                                                 
SE56                                                           US               
SE50ST IAN TEST SHIP TO PARTY                                                   
SE5515***ADDRESS NOT ON FILE***                                                 
SE56                                                           US               
SE50EX IAN TEST EXPORTER                                                        
SE5515***ADDRESS NOT ON FILE***                                                 
SE56                                                           US               
SE50SH IAN TEST SHIPPER                                                         
SE5515***ADDRESS NOT ON FILE***                                                 
SE56                                                           US               
SE50DR IAN TEST DISTRIBUTOR                                                     
SE5515***ADDRESS NOT ON FILE***                                                 
SE56                                                           US               
SE50PK IAN TEST PACKAGER                                                        
SE5515***ADDRESS NOT ON FILE***                                                 
SE56                                                           US               
SE603920995000                                                                  
SE40003FR                                                                       
SE50MF MANUFACTURER FOR LINE 2                                                  
SE5515TEST MAN FOR LINE 2                                                       
SE56                                                           US               
SE50CN                                       BBBBBB123                          
SE50BY                                       98-12345612                        
SE50SE SELLER FOR INV HEADER 2                                                  
SE5515123/33 STREET 1                                                           
SE56                                                           US               
SE50ST ULTIMATECONSIGNEE2                                                       
SE5515***ADDRESS NOT ON FILE***                                                 
SE56                                                           US               
SE603920995000                                                                  
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, false))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals(@"Expected Message: 
			 - Other Organizations should be on line level, because they are different from Invoice Orgs",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01EI 69-9999999JC40800000150008888                           
SE11                                   123W                                     
SE13AMY XIANG                               123456789                           
SE20CR B00001000                                                                
SE40001BH                                                                       
SE50MF MANUFACTURER FOR LINE 1                                                  
SE5515TEST MAN FOR LINE 1                                                       
SE56                                                           US               
SE50CN ULTIMATECONSIGNEE1                                                       
SE5515***ADDRESS NOT ON FILE***                                                 
SE56                                                           US               
SE50BY                                       01-17900612                        
SE50SE TEST SELLING PARTY(SELLER)                                               
SE5515***ADDRESS NOT ON FILE***                                                 
SE56                                                           US               
SE50ST IAN TEST SHIP TO PARTY                                                   
SE5515***ADDRESS NOT ON FILE***                                                 
SE56                                                           US               
SE6084717040650000015000                                                        
SE40002FR                                                                       
SE50MF MANUFACTURER FOR LINE 2                                                  
SE5515TEST MAN FOR LINE 2                                                       
SE56                                                           US               
SE50CN                                       BBBBBB123                          
SE50BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE5515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE56GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE50SE TEST SELLING PARTY(SELLER)                                               
SE5515***ADDRESS NOT ON FILE***                                                 
SE56                                                           US               
SE50ST IAN TEST SHIP TO PARTY                                                   
SE5515***ADDRESS NOT ON FILE***                                                 
SE56                                                           US               
SE603920995000                                                                  
SE40003FR                                                                       
SE50MF MANUFACTURER FOR LINE 2                                                  
SE5515TEST MAN FOR LINE 2                                                       
SE56                                                           US               
SE50CN                                       BBBBBB123                          
SE50BY                                       98-12345612                        
SE50SE SELLER FOR INV HEADER 2                                                  
SE5515123/33 STREET 1                                                           
SE56                                                           US               
SE50ST ULTIMATECONSIGNEE2                                                       
SE5515***ADDRESS NOT ON FILE***                                                 
SE56                                                           US               
SE603920995000                                                                  
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
		}

		public void TestBuildMessageForEntryType06()
		{
			var declaration = GetMergedDeclaration();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EntryDateElectionCode = "W";
			declaration.US_PresentationDate = ZDate.BrettsBirthday;
			declaration.JE_MasterBill = "TESTBILL";
			declaration.US_FTZNo = "123456";
			declaration.JE_PrimaryITNumber = "V2365425144";
			declaration.US_ITDate = new ZDate(2016, 06, 29);
			declaration.JE_TotalNoOfPacks = 100;

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "5678", "Some Domestic Port", startDate, endDate);
			newFactory.Save();

			declaration.US_SchDArrival = "5678";

			var line = declaration.InvoiceLines[0];
			line.US_ZoneStatus = "D";

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease,
				collection);
			action.US_SE_ContactName = "AMY XIANG";
			action.US_SE_ContactPhone = "123456789";
			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));

			var message = builder.PopulateMessage();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals("Test with additional details",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 06EI 69-9999999JC40800000150008888  5678                     
SE11W091871        FTZ123456           123W                                     
SE13AMY XIANG                               123456789                           
SE15I    V2365425144                                                    N       
SE15R    TESTBILL                                          00000100     N       
SE20CR B00001000                                                                
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30EX IAN TEST EXPORTER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30SH IAN TEST SHIPPER                                                         
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30DR IAN TEST DISTRIBUTOR                                                     
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30PK IAN TEST PACKAGER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, false))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals("Test with additional details",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 06EI 69-9999999JC40800000150008888  5678                     
SE11W091871        FTZ123456           123W                                     
SE13AMY XIANG                               123456789                           
SE15I    V2365425144                                                    N       
SE15R    TESTBILL                                          00000100     N       
SE20CR B00001000                                                                
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
		}

		public void TestBuildMessageForEntryType21()
		{
			var declaration = GetMergedDeclaration();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_EntryDateElectionCode = "W";
			declaration.US_EstimatedEntryDate = ZDate.BrettsBirthday;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_BondAmount = 150;
			declaration.US_SuretyCode = "KNZ";
			declaration.US_WHSEntryFilerCode = "ABC";
			declaration.US_WHSEntryNumber = "12345678";

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			action.US_SE_ContactName = "AMY XIANG";
			action.US_SE_ContactPhone = "123456789";

			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			var message = builder.PopulateMessage();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals("Test with additional details",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 21EI 69-9999999JC40900000150008888                           
SE11W091871                            123W                                     
SE13AMY XIANG                               123456789                           
SE20CR B00001000                                                                
SE20V1 KNZ                                                                      
SE20AMT150                                                                      
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30EX IAN TEST EXPORTER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30SH IAN TEST SHIPPER                                                         
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30DR IAN TEST DISTRIBUTOR                                                     
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30PK IAN TEST PACKAGER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE40001AU                                                                       
SE6084717040650000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, false))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals("Test with additional details",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 21EI 69-9999999JC40900000150008888                           
SE11W091871                            123W                                     
SE13AMY XIANG                               123456789                           
SE20CR B00001000                                                                
SE20V1 KNZ                                                                      
SE20AMT150                                                                      
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE40001AU                                                                       
SE6084717040650000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
		}

		public void TestBuildMessageForEntryType86()
		{
			var declaration = GetMergedDeclaration();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.LowValue;
			declaration.JE_DateOfArrival = ZDateTime.BrettsBirthday;
			declaration.US_EntryDate = ZDateTime.BrettsBirthday.AddDays(-1);

			var entry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var action = new EntryHeaderMessageSendingAction(entry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			action.US_SE_ContactName = "AMY XIANG";
			action.US_SE_ContactPhone = "123456789";

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.EDA86, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, false))
			{
				var builder = new SimplifiedEntryMessageBuilder(entry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				var message = builder.PopulateMessage();

				AssertMultilineASCIIEquals(
	@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 86EI 69-9999999JC40000000150008888                           
SE11                                   123W                                     
SE13AMY XIANG                               123456789                           
SE20CR B00001000                                                                
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE40001AU                                                                       
SE6084717040650000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.EDA86, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				var builder = new SimplifiedEntryMessageBuilder(entry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				var message = builder.PopulateMessage();

				AssertMultilineASCIIEquals(
	@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 86EI 69-9999999JC40000000150008888                           
SE11                                   123W                                     
SE13AMY XIANG                               123456789                           
SE20CR B00001000                                                                
SE20EDA091871                                                                   
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE40001AU                                                                       
SE6084717040650000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
		}

		public void TestOnlyLineLevelEntities()
		{
			var declaration = GetMergedDeclaration();
			declaration.JE_OA_ConsigneeAddress = ZGuid.Empty;
			var invoice = declaration.Invoices[0];
			invoice.JZ_OA_ConsigneeAddress = ZGuid.Empty;
			invoice.JZ_OA_ManufacturerAddress = ZGuid.Empty;
			invoice.JZ_OA_SellerAddress = ZGuid.Empty;
			invoice.JZ_OA_SoldToPartyAddress = ZGuid.Empty;

			var ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.OH_Code = "UC" + new Random().Next(1000000).ToString();
			ultimateConsignee.OH_FullName = "ultimateConsignee1";
			ultimateConsignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EncryptedConsigneeNumber, "-23GGFRD234");
			ultimateConsignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "138888-12345");

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_FullName = "Manufacturer For Line 1";
			manufacturer.OH_Code = "MAN" + new Random().Next(1000000).ToString();
			var address = manufacturer.Addresses.AddNew();
			address.OA_Address1 = "Test man for line 1";
			address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "BHNHJKMEQU6LFR");

			var invoiceLine1 = declaration.InvoiceLines[0];
			invoiceLine1.JI_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;
			invoiceLine1.JI_OA_ManufacturerAddress = address.PK;
			invoiceLine1.US_UC_NKCountryOfOrigin = "BH";

			var soldToParty = Factory.New<OrgHeader>();
			soldToParty.OH_FullName = "AUTO ELECTRICAL DISTRIBUTORS PTY LTD TEST TEST TES";
			soldToParty.OH_Code = "STP" + new Random().Next(1000000).ToString();
			soldToParty.OH_RL_NKClosestPort = "MXMEX";
			address = soldToParty.MainAddress;
			address.OA_Address1 = "UNIT 1, 210 ROBINSON ROAD GEEBUN DOWNTOWN FOR TEST";
			address.OA_Address2 = "GEEBUNG, QLD GEEBUN DOWNTOWN FOR TEST GEEBUN DOWNT";
			address.OA_City = "GEEBUN CITY FOR LENGHT TE";
			address.OA_PostCode = "4034654521";
			soldToParty.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "167013-12345");
			invoiceLine1.JI_OA_SoldToPartyAddress = soldToParty.MainAddress.PK;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			action.US_SE_ContactName = "AMY XIANG";
			action.US_SE_ContactPhone = "123456789";

			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));

			var message = builder.PopulateMessage();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals(@"Expected Message: 
			 - Organizations should be on line level, because nothing on Invoice Header level",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01EI 69-9999999JC40800000150008888                           
SE11                                   123W                                     
SE13AMY XIANG                               123456789                           
SE20CR B00001000                                                                
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30EX IAN TEST EXPORTER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30SH IAN TEST SHIPPER                                                         
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30DR IAN TEST DISTRIBUTOR                                                     
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30PK IAN TEST PACKAGER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE40001BH                                                                       
SE50MF MANUFACTURER FOR LINE 1                                                  
SE5515TEST MAN FOR LINE 1                                                       
SE56                                                           US               
SE50CN                                    ANI138888-12345                       
SE50BY                                    ANI167013-12345                       
SE6084717040650000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, false))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals(@"Expected Message: 
			 - Organizations should be on line level, because nothing on Invoice Header level",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01EI 69-9999999JC40800000150008888                           
SE11                                   123W                                     
SE13AMY XIANG                               123456789                           
SE20CR B00001000                                                                
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE40001BH                                                                       
SE50MF MANUFACTURER FOR LINE 1                                                  
SE5515TEST MAN FOR LINE 1                                                       
SE56                                                           US               
SE50CN                                    ANI138888-12345                       
SE50BY                                    ANI167013-12345                       
SE6084717040650000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
		}

		public void TestBuildMessage()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.JE_VesselName = "VESSEL";
			declaration.JE_VoyageFlightNo = "V123W";
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			declaration.US_US_NKCentralizedExamSite = "A002";
			declaration.US_US_NKLocationOfGoods = "S004";
			declaration.JE_RL_NKPortOfLoading = "PERTD";
			declaration.US_SchDEntry = "8888";
			declaration.JE_RL_NKOrigin = "PERTD";
			declaration.US_SchDLoading = "5678";
			declaration.JE_DateOfArrival = ZDate.Today;

			declaration.JE_MasterBill = "MB4534535";
			var primaryMasterBill = declaration.PrimaryMasterBill;
			primaryMasterBill.CU_NoOfPacks = 12;
			primaryMasterBill.CU_PackType = DeclarationTestHelper.BillUS_ManifestUQForTest;

			var houseBill1 = primaryMasterBill.ChildBills.AddNew();
			houseBill1.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill1.CU_HouseBill = "HouseBill1";
			houseBill1.CU_NoOfPacks = 49;
			houseBill1.CU_PackType = DeclarationTestHelper.BillUS_ManifestUQForTest;

			var houseBill2 = primaryMasterBill.ChildBills.AddNew();
			houseBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.CU_HouseBill = "HB5465646";
			houseBill2.CU_NoOfPacks = 7;
			houseBill2.CU_PackType = DeclarationTestHelper.BillUS_ManifestUQForTest;

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CRUX432890";
			container1.CO_Seal = "SEAL1234567890123456";

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "APLU001890";
			container2.CO_Seal = "SE456789";

			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "OTT1001890";
			container3.CO_Seal = "SE456789";

			var container4 = declaration.CusContainers.AddNew();
			container4.CO_ContainerNumber = "APLU007854";
			container4.CO_Seal = "SE45671";
			var packGroup1 = declaration.PackingGroups.AddNew();
			packGroup1.CR_CU_HouseBill = primaryMasterBill.PK;
			packGroup1.CR_CO_Container = container2.PK;

			var packGroup2 = declaration.PackingGroups.AddNew();
			packGroup2.CR_CU_HouseBill = houseBill2.PK;
			packGroup2.CR_CO_Container = container3.PK;

			var packGroup3 = declaration.PackingGroups.AddNew();
			packGroup3.CR_CU_HouseBill = houseBill1.PK;
			packGroup3.CR_CO_Container = container3.PK;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8471704065";
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_Weight = 120m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			action.US_SE_ContactName = "AMY XIANG";
			action.US_SE_ContactPhone = "123456789";
			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));

			var message = builder.PopulateMessage();
			AssertMultilineASCIIEquals("Test with additional details",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01               11800000150008888                           
SE11       S004A002VESSEL              V123W                                    
SE13AMY XIANG                               123456789                           
SE15M    MB4534535                                                      N       
SE15H    HOUSEBILL1                                        00000049     N       
SE17OTT1001890                                                                  
SE15M    MB4534535                                                      N       
SE15H    HB5465646                                         00000007     N       
SE17OTT1001890                                                                  
SE20CR B00001000                                                                
SE40001AU                                                                       
SE6084717040650000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);

			var itNo1 = houseBill1.ITAndSplitDetails.AddNew();
			itNo1.US_ITNumber = "APLU231232";

			message = builder.PopulateMessage();
			AssertMultilineASCIIEquals("Test with additional details",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01               11800000150008888                           
SE11       S004A002VESSEL              V123W                                    
SE13AMY XIANG                               123456789                           
SE15I    APLU231232                                                     N       
SE15M    MB4534535                                                      N       
SE15H    HOUSEBILL1                                        00000049     N       
SE17OTT1001890                                                                  
SE15M    MB4534535                                                      N       
SE15H    HB5465646                                         00000007     N       
SE17OTT1001890                                                                  
SE20CR B00001000                                                                
SE40001AU                                                                       
SE6084717040650000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
		}

		[TestDate(2014, 02, 06)]
		public void TestBuildWithITNumbers()
		{
			USCustomsDataRegistry.Instance.EnableNCTAsDefaultContainerModeForAirOrTruckDeclaration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.JE_VoyageFlightNo = "V123W";
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			declaration.US_US_NKCentralizedExamSite = "A002";
			declaration.US_US_NKLocationOfGoods = "S004";
			declaration.JE_RL_NKPortOfLoading = "PERTD";
			declaration.US_SchDEntry = "8888";
			declaration.JE_RL_NKOrigin = "PERTD";
			declaration.US_SchDLoading = "5678";

			declaration.JE_MasterBill = "MB4534535";
			var primaryMasterBill = declaration.PrimaryMasterBill;
			primaryMasterBill.CU_NoOfPacks = 19;
			primaryMasterBill.CU_PackType = DeclarationTestHelper.BillUS_ManifestUQForTest;

			var houseBill1 = primaryMasterBill.ChildBills.AddNew();
			houseBill1.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill1.CU_HouseBill = "HouseBill1";
			houseBill1.CU_NoOfPacks = 49;
			houseBill1.CU_PackType = DeclarationTestHelper.BillUS_ManifestUQForTest;

			var itNumber1 = houseBill1.ITAndSplitDetails.AddNew();
			itNumber1.US_ITNumber = "123456789";
			itNumber1.US_NoOfPacks = 12;

			var itNumber2 = houseBill1.ITAndSplitDetails.AddNew();
			itNumber2.US_ITNumber = "V7841541564";
			itNumber2.US_NoOfPacks = 7;

			var houseBill2 = primaryMasterBill.ChildBills.AddNew();
			houseBill2.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.CU_HouseBill = "HB5465646";
			houseBill2.CU_NoOfPacks = 7;
			houseBill2.CU_PackType = DeclarationTestHelper.BillUS_ManifestUQForTest;
			houseBill2.US_SESplitShip = true;

			var itNumber3 = houseBill2.ITAndSplitDetails.AddNew();
			itNumber3.US_ITNumber = "001456900";
			itNumber3.US_NoOfPacks = 1;
			itNumber3.US_ArrivalDate = ZDateTime.Today.AddDays(1);
			itNumber3.US_CarrierCode = "D0";
			itNumber3.US_FlightNumber = "001A";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8471704065";
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_Weight = 120m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			action.US_SE_ContactName = "AMY XIANG";
			action.US_SE_ContactPhone = "123456789";

			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));

			var message = builder.PopulateMessage();
			AssertMultilineASCIIEquals("Test with additional details",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01                 800000150008888                           
SE11       S004A002                    123W                                     
SE13AMY XIANG                               123456789          1                
SE15I    123456789                                                      N       
SE15M    MB4534535                                                      N       
SE15H    HOUSEBILL1                                        00000012     N       
SE15I    V7841541564                                                    N       
SE15M    MB4534535                                                      N       
SE15H    HOUSEBILL1                                        00000007     N       
SE15I    001456900                                                      N       
SE15M    MB4534535                                                      N       
SE15H    HB5465646                                                      N       
SE16D0  001A 02071400000001PK                                                   
SE20CR B00001000                                                                
SE40001AU                                                                       
SE6084717040650000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
		}

		[TestDate(2014, 02, 06)]
		public void TestBuildWithSplitDetails()
		{
			USCustomsDataRegistry.Instance.EnableNCTAsDefaultContainerModeForAirOrTruckDeclaration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.JE_VoyageFlightNo = "V123W";
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			declaration.US_US_NKCentralizedExamSite = "A002";
			declaration.US_US_NKLocationOfGoods = "S004";
			declaration.JE_RL_NKPortOfLoading = "PERTD";
			declaration.US_SchDEntry = "8888";
			declaration.JE_RL_NKOrigin = "PERTD";
			declaration.US_SchDLoading = "5678";

			declaration.JE_MasterBill = "MB4534535";
			var primaryMasterBill = declaration.PrimaryMasterBill;
			primaryMasterBill.CU_NoOfPacks = 12;
			primaryMasterBill.CU_PackType = DeclarationTestHelper.BillUS_ManifestUQForTest;

			var houseBill1 = primaryMasterBill.ChildBills.AddNew();
			houseBill1.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill1.CU_HouseBill = "HouseBill1";
			houseBill1.CU_NoOfPacks = 49;
			houseBill1.CU_PackType = DeclarationTestHelper.BillUS_ManifestUQForTest;
			houseBill1.US_SESplitShip = true;

			var details1 = houseBill1.ITAndSplitDetails.AddNew();
			details1.US_NoOfPacks = 12;
			details1.US_FlightNumber = "002W";
			details1.US_CarrierCode = "A0";
			details1.US_ArrivalDate = ZDateTime.Today.AddDays(1);

			var details2 = houseBill1.ITAndSplitDetails.AddNew();
			details2.US_NoOfPacks = 7;
			details2.US_FlightNumber = "456I";
			details2.US_CarrierCode = "D0";
			details2.US_ArrivalDate = ZDateTime.Today.AddDays(2);

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8471704065";
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_Weight = 120m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			action.US_SE_ContactName = "AMY XIANG";
			action.US_SE_ContactPhone = "123456789";

			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));

			var message = builder.PopulateMessage();
			AssertMultilineASCIIEquals("Test with additional details",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01                 800000150008888                           
SE11       S004A002                    123W                                     
SE13AMY XIANG                               123456789          1                
SE15M    MB4534535                                                      N       
SE15H    HOUSEBILL1                                                     N       
SE16A0  002W 02071400000012PK                                                   
SE16D0  456I 02081400000007PK                                                   
SE20CR B00001000                                                                
SE40001AU                                                                       
SE6084717040650000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
		}

		[TestDate(2015, 06, 25)]
		public void TestReasonCodeForSECancellation()
		{
			USCustomsDataRegistry.Instance.EnableNCTAsDefaultContainerModeForAirOrTruckDeclaration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.JE_VoyageFlightNo = "V123W";
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			declaration.US_SchDEntry = "8888";
			declaration.US_SchDLoading = "5678";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8471704065";
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_Weight = 120m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));

			var message = builder.PopulateMessage();
			AssertEquals(CusEntryHeaderMessageTypeList.Codes.ACECargoRelease, message.EM_MessageType);

			var rcvMessage = Factory.New<MQEDIMessage>();
			rcvMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			rcvMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			rcvMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse;
			rcvMessage.EM_MessageNum = "HYEDUSCMT_196571";
			rcvMessage.EM_MessageText =
@"B001101SV9SX                                               HYEDUSCMT_196571     " +
"SE10ASV9  71032807 01EI 58-12345678911800000100001101  1101                     " +
"SE15RAPLUMST0802186                                        00000010     N       " +
"SE20CR B00173079                                                                " +
"SE9002   SE DATA ACCEPTED                                                       " +
"Y  1101SV9SX00004";
			rcvMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-1);
			seEntry.Messages.Add(rcvMessage);

			collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Deletion);
			action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			action.US_SE_ReasonCode = ReasonCodeList.Codes.EntryReplacedByFTZ;
			action.US_SE_ReferenceNo = "1530001150124563";
			action.US_SE_MultipleDispositionsIndic = true;

			builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Delete, ACEEntrySummaryMessageSendingOption.New(action));
			message = builder.PopulateMessage();
			AssertMultilineASCIIEquals("Reason Code, Ref Number and indicator are submitted, SE11 block should not be generated",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10DXJ5  00000014 01                 800000150008888                           
SE13CARGOWISE SUPPORT                                      041                  
SE20FTZ1530001150124563                                                         
Y  8888XJ5SE", message.EM_FormattedMessageText);

			seEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);

			builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Replace, ACEEntrySummaryMessageSendingOption.New(action));
			message = builder.PopulateMessage();
			AssertMultilineASCIIEquals("Reason Code, Ref Number and indicator should not be submitted",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10RXJ5  00000014 01                 800000150008888                           
SE11                                   123W                                     
SE13CARGOWISE SUPPORT                                                           
SE20CR B00001000                                                                
SE40001AU                                                                       
SE6084717040650000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
		}

		public void TestLocationOfGoodsAndCBPBondedWarehouse()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.US_US_NKLocationOfGoods = "A001";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1500m;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1500m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;

			var warehouseOrg = Factory.New<OrgHeader>();
			warehouseOrg.OH_Code = "TESTWHS";
			warehouseOrg.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "B001");
			declaration.WarehouseDocAddress.OrganisationPK = warehouseOrg.PK;

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			AssertEquals(2, actions.Count);

			actions.OfType<ImportMessageSendingAction>().ForEach(x => x.US_SendMessage = true);
			actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var seMessage = (MQEDIMessage)declaration.ActiveEntryHeaders.SimplifiedEntry.Messages[0];
			var se11Block = seMessage.MessageBlock.MessageBlocks.OfType<ASESE11>().FirstOrDefault();
			AssertNotNull(se11Block);
			AssertEquals("A001", se11Block.LocationOfGoodsFIRMS);
			AssertEquals("B001", se11Block.CBPBondedWarehouseFIRMS);

			var ensMessage = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
			var ens20Block = ensMessage.MessageBlock.MessageBlocks.OfType<AENS20>().FirstOrDefault();
			AssertNotNull(ens20Block);
			AssertEquals("B001", ens20Block.LocationOfGoodsCode);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions.OfType<ImportMessageSendingAction>().ForEach(x => x.US_SendMessage = true);
			actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			seMessage = (MQEDIMessage)declaration.ActiveEntryHeaders.SimplifiedEntry.Messages[1];
			se11Block = seMessage.MessageBlock.MessageBlocks.OfType<ASESE11>().FirstOrDefault();
			AssertEquals("B001", se11Block.LocationOfGoodsFIRMS);
			AssertEquals(ZString.Empty, se11Block.CBPBondedWarehouseFIRMS);

			ensMessage = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[1];
			ens20Block = ensMessage.MessageBlock.MessageBlocks.OfType<AENS20>().FirstOrDefault();
			AssertEquals("B001", ens20Block.LocationOfGoodsCode);

			warehouseOrg.MainAddress.CustomsCodes.DeleteAll();
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions.OfType<ImportMessageSendingAction>().ForEach(x => x.US_SendMessage = true);
			actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			seMessage = (MQEDIMessage)declaration.ActiveEntryHeaders.SimplifiedEntry.Messages[2];
			se11Block = seMessage.MessageBlock.MessageBlocks.OfType<ASESE11>().FirstOrDefault();
			AssertEquals("A001", se11Block.LocationOfGoodsFIRMS);
			AssertEquals(ZString.Empty, se11Block.CBPBondedWarehouseFIRMS);

			ensMessage = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[2];
			ens20Block = ensMessage.MessageBlock.MessageBlocks.OfType<AENS20>().FirstOrDefault();
			AssertEquals("A001", ens20Block.LocationOfGoodsCode);
		}

		public void TestSendCargoReleaseUpdateMessage()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.JE_VoyageFlightNo = "V123W";
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			declaration.US_SchDEntry = "8888";
			declaration.US_SchDLoading = "5678";
			declaration.JE_MasterBillIssuerSCAC = "A2";
			declaration.JE_MasterBill = "001238456874";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8471704065";
			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_Weight = 120m;

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

			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			action.US_SE_ActionType = ACECargoReleaseActionType.Codes.Update;
			action.US_SE_ContactName = "IAN CHEN";
			action.US_SE_ContactPhone = "12345678";

			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Update, ACEEntrySummaryMessageSendingOption.New(action));
			var message = builder.PopulateMessage();
			AssertMultilineASCIIEquals("Only SE10/SE11/SE15/SE16/SE17/SE20 blocks can be reported",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10UXJ5  00000014 01                 800000150008888                           
SE11                                   V123W                                    
SE13IAN CHEN                                12345678           1                
SE15I    123456789                                                      N       
SE15RA2  001238456874                                                   N       
SE16A2  V123W      00000100PC                                                   
SE171223345612                                                                  
SE20CR B00001000                                                                
Y  8888XJ5SE", message.EM_FormattedMessageText);
		}

		public void TestConveyanceNameInSE11()
		{
			var declaration = GetMergedDeclaration();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.JE_MasterBill = "0260A01";
			declaration.US_FTZNo = "123456";

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions.OfType<ImportMessageSendingAction>().ForEach(x => x.US_SendMessage = true);
			actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var seMessage = (MQEDIMessage)declaration.ActiveEntryHeaders.SimplifiedEntry.Messages[0];
			var se11Block = seMessage.MessageBlock.MessageBlocks.OfType<ASESE11>().FirstOrDefault();
			AssertNotNull(se11Block);
			AssertEquals("FTZ123456", se11Block.ConveyanceNameOrFTZZoneID);

			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_VesselName = "AAAAA";

			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions.OfType<ImportMessageSendingAction>().ForEach(x => x.US_SendMessage = true);
			actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			seMessage = (MQEDIMessage)declaration.ActiveEntryHeaders.SimplifiedEntry.Messages[1];
			se11Block = seMessage.MessageBlock.MessageBlocks.OfType<ASESE11>().FirstOrDefault();
			AssertNotNull(se11Block);
			AssertEquals("AAAAA", se11Block.ConveyanceNameOrFTZZoneID);
		}

		public void TestCarrierCodeForFixedTransportInSE16()
		{
			var declaration = GetMergedDeclaration();
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.JE_MasterBill = "12345678";
			declaration.US_PipelineName = "222333";
			declaration.US_UI_NKCarrierSCAC = "AAEU";
			declaration.US_US_NKLocationOfGoods = "A001";

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions.OfType<ImportMessageSendingAction>().ForEach(x => x.US_SendMessage = true);
			actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var seMessage = (MQEDIMessage)declaration.ActiveEntryHeaders.SimplifiedEntry.Messages[0];
			var se16Block = seMessage.MessageBlock.MessageBlocks.OfType<ASESE16>().FirstOrDefault();
			AssertNotNull(se16Block);
			AssertEquals("AAEU", se16Block.CarrierCode);

			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.US_NonAMS = true;

			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions.OfType<ImportMessageSendingAction>().ForEach(x => x.US_SendMessage = true);
			actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			seMessage = (MQEDIMessage)declaration.ActiveEntryHeaders.SimplifiedEntry.Messages[1];
			se16Block = seMessage.MessageBlock.MessageBlocks.OfType<ASESE16>().FirstOrDefault();
			AssertNotNull(se16Block);
			AssertEquals("AAEU", se16Block.CarrierCode);
		}

		public void TestQuantityIsNotSentInSE15IfSE16Generated()
		{
			var declaration = GetMergedDeclaration();
			declaration.JE_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			declaration.JE_MasterBill = "12345678";
			declaration.US_US_NKLocationOfGoods = "A001";
			declaration.Bills[0].CU_NoOfPacks = 12345;

			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions.OfType<ImportMessageSendingAction>().ForEach(x => x.US_SendMessage = true);
			actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var seMessage = (MQEDIMessage)declaration.ActiveEntryHeaders.SimplifiedEntry.Messages[0];
			var se15Block = seMessage.MessageBlock.MessageBlocks.OfType<ASESE15>().FirstOrDefault();
			AssertNotNull(se15Block);
			AssertEquals(0, se15Block.Quantity);
			var se16Block = seMessage.MessageBlock.MessageBlocks.OfType<ASESE16>().FirstOrDefault();
			AssertNotNull(se16Block);
			AssertEquals(12345, se16Block.Quantity);

			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions.OfType<ImportMessageSendingAction>().ForEach(x => x.US_SendMessage = true);
			actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			seMessage = (MQEDIMessage)declaration.ActiveEntryHeaders.SimplifiedEntry.Messages[1];
			se15Block = seMessage.MessageBlock.MessageBlocks.OfType<ASESE15>().FirstOrDefault();
			AssertNotNull(se15Block);
			AssertEquals(12345, se15Block.Quantity);
			se16Block = seMessage.MessageBlock.MessageBlocks.OfType<ASESE16>().FirstOrDefault();
			AssertNull(se16Block);
		}

		public void TestBuildMessageWithConsigneeNameAndAddressSE30()
		{
			#region Org

			var supplierOrg = Factory.New<OrgHeader>();
			supplierOrg.OH_Code = "PIEROCTRR";
			supplierOrg.OH_FullName = "PIERRE ROCLE";
			supplierOrg.OH_RL_NKClosestPort = "FRTRR";
			supplierOrg.MainAddress.OA_Address1 = "10 BLD GARIBALDI";
			supplierOrg.PrimaryRegistrationNumber.Number = string.Empty;

			var importerOrg = Factory.New<OrgHeader>();
			importerOrg.OH_Code = "KRAFOO1";
			importerOrg.OH_FullName = "KRAFT FOODS INTERNATIONAL";
			importerOrg.OH_RL_NKClosestPort = "USCHI";
			importerOrg.MainAddress.OA_Address1 = "65 DEERFIELD RD";
			importerOrg.OH_IsConsignee = true;
			importerOrg.PrimaryRegistrationNumber.Number = "13-147927000";

			var emptyNumberOrg = Factory.New<OrgHeader>();
			emptyNumberOrg.OH_Code = "EMPTYORG";
			emptyNumberOrg.OH_FullName = "AFS FREIGHT MANAGEMENT P/L";
			emptyNumberOrg.OH_RL_NKClosestPort = "AUMEL";
			emptyNumberOrg.MainAddress.OA_Address1 = "27-29 MIAC BLDG";
			emptyNumberOrg.PrimaryRegistrationNumber.Number = string.Empty;
			emptyNumberOrg.OH_IsConsignee = true;

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_FullName = "MANUFACTURER1";
			manufacturer.OH_Code = "MAN" + new Random().Next(1000000).ToString();
			var address = manufacturer.Addresses.AddNew();
			address.OA_Address1 = "1 TEST MANUFACTURER";
			address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "BHNHJKMEQU6LFR");

			var soldToParty = Factory.New<OrgHeader>();
			soldToParty.OH_FullName = "SOLD TO PARTY";
			soldToParty.OH_Code = "STP" + new Random().Next(1000000).ToString();
			soldToParty.OH_RL_NKClosestPort = "MXMEX";
			soldToParty.MainAddress.OA_Address1 = "71 SOLD TO PARTY";
			#endregion

			var declaration = GetMergedDeclaration();

			declaration.JE_OH_Supplier = supplierOrg.PK;
			declaration.JE_OH_Importer = importerOrg.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.ValidationModes = ValidationModes.CargoRelease;

			declaration.JE_OA_ConsigneeAddress = emptyNumberOrg.MainAddress.PK;
			declaration.JE_OA_ManufacturerAddress = address.PK;
			declaration.JE_OA_SoldToPartyAddress = soldToParty.MainAddress.PK;

			var invoiceHeader = declaration.Invoices[0];
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.DeliveredDutyPaid;
			invoiceHeader.JZ_InvoiceAmount = 9800;
			invoiceHeader.JZ_OA_ConsigneeAddress = emptyNumberOrg.MainAddress.PK;
			invoiceHeader.JZ_OA_SoldToPartyAddress = soldToParty.MainAddress.PK;
			invoiceHeader.JZ_OA_SellerAddress = importerOrg.MainAddress.PK;
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines[0];
			invoiceLine1.US_SupTariff = "3920.99.5000";
			invoiceLine1.US_UC_NKCountryOfOrigin = "BH";
			invoiceLine1.JI_OA_SoldToPartyAddress = soldToParty.MainAddress.PK;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			var message = builder.PopulateMessage();

			var se30s = message.MessageBlock.MessageBlocks.FindAll(x => x is ASESE30).Cast<ASESE30>();
			foreach (var msgline in se30s)
			{
				if (msgline.EntityCode == EntityCodeList.Codes.Consignee && msgline.EntityIdentifier.IsEmpty)
				{
					AssertEquals("Consignee Name + Address", "AFS FREIGHT MANAGEMENT P/L", msgline.EntityName);
				}
			}
		}

		public void TestBuildMessageWithConsigneeNameAndAddressSE50()
		{
			var declaration = GetMergedDeclaration();

			var ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.OH_Code = "UC" + new Random().Next(1000000).ToString();
			ultimateConsignee.PrimaryRegistrationNumber.Number = "ABCDEFG";
			ultimateConsignee.OH_FullName = "CONSIGNEE HAS NUMBER";

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_FullName = "Manufacturer For Line 1";
			manufacturer.OH_Code = "MAN" + new Random().Next(1000000).ToString();
			var address = manufacturer.Addresses.AddNew();
			address.OA_Address1 = "Test man for line 1";
			address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "BHNHJKMEQU6LFR");

			var invoiceLine1 = declaration.InvoiceLines[0];
			invoiceLine1.JI_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;
			invoiceLine1.JI_OA_ManufacturerAddress = address.PK;
			invoiceLine1.US_UC_NKCountryOfOrigin = "BH";

			var foreignExporter = Factory.New<OrgHeader>();
			foreignExporter.OH_FullName = "Foreign Exporter";
			foreignExporter.OH_Code = "FE" + new Random().Next(1000000).ToString();
			address = foreignExporter.Addresses.AddNew();
			address.OA_Address1 = "Test FE for line 1";
			invoiceLine1.JI_OA_ExporterAddress = address.PK;

			var soldToParty = Factory.New<OrgHeader>();
			soldToParty.OH_FullName = "Sold To Party";
			soldToParty.OH_Code = "STP" + new Random().Next(1000000).ToString();
			soldToParty.OH_RL_NKClosestPort = "MXMEX";
			address = soldToParty.MainAddress;
			address.OA_Address1 = "Sold To Party Address 1";
			address.OA_Address2 = "STP Address 2";
			address.OA_City = "Mexico city";
			address.OA_PostCode = "05064";
			soldToParty.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "01-17900612");
			invoiceLine1.JI_OA_SoldToPartyAddress = soldToParty.MainAddress.PK;

			var ultimateConsignee2 = Factory.New<OrgHeader>();
			ultimateConsignee2.OH_Code = "UC" + new Random().Next(1000000).ToString();
			ultimateConsignee2.OH_FullName = "SHOULD HAVE NAME";

			var manufacturer2 = Factory.New<OrgHeader>();
			manufacturer2.OH_FullName = "Manufacturer For Line 2";
			manufacturer2.OH_Code = "MAN" + new Random().Next(1000000).ToString();
			var address2 = manufacturer2.Addresses.AddNew();
			address2.OA_Address1 = "Test man for line 2";
			address2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "FR034FREQU6LBH");

			var invoiceLine2 = declaration.Invoices[0].InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "3920995000";
			invoiceLine2.JI_OA_ConsigneeAddress = ultimateConsignee2.MainAddress.PK;
			invoiceLine2.JI_OA_ManufacturerAddress = address2.PK;
			invoiceLine2.US_UC_NKCountryOfOrigin = "FR";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));

			var message = builder.PopulateMessage();

			var se50s = message.MessageBlock.MessageBlocks.FindAll(x => x is ASESE50).Cast<ASESE50>();

			foreach (var msgline in se50s)
			{
				if (msgline.EntityCode == EntityCodeList.Codes.Consignee && msgline.EntityIdentifier.IsEmpty)
				{
					AssertEquals("SE50 Consignee Name + Address", "SHOULD HAVE NAME", msgline.EntityName);
				}
			}
		}

		public void TestBuildPGAMessageBlocksForSecondaryTariffLine()
		{
			var tariff0 = Factory.New<USCTariff>();
			tariff0.UE_Tariff = "9608500000";
			tariff0.UE_PGACodes = "EP8";
			tariff0.UE_DateFrom = ZDate.Today.AddMonths(-1);
			tariff0.UE_DateTo = ZDate.Today.AddMonths(1);

			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = "9608100000";
			tariff1.UE_PGACodes = "EP8";
			tariff1.UE_DateFrom = ZDate.Today.AddMonths(-1);
			tariff1.UE_DateTo = ZDate.Today.AddMonths(1);

			var declaration = GetMergedDeclaration();
			var invoiceLine0 = declaration.InvoiceLines[0];
			invoiceLine0.JI_Tariff = tariff0.UE_Tariff;
			invoiceLine0.JI_Description = "TEST PARENT TSCA";
			invoiceLine0.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine0.US_TSCAIndicator = TSCAIndicatorList.Codes.TSCAPositive;
			invoiceLine0.US_TSCAODSCertIndividual = PartyTypeList.Codes.Importer;
			invoiceLine0.US_FDAContactName = "PARENT IMPORTER";
			invoiceLine0.InvoiceHeader.US_TSCASignDate = new DateTime(2016, 11, 30);

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = tariff1.UE_Tariff;
			invoiceLine1.JI_ParentID = invoiceLine0.PK;
			invoiceLine1.JI_Description = "TEST SECONDARY TSCA";
			invoiceLine1.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine1.US_TSCAIndicator = TSCAIndicatorList.Codes.TSCANegative;
			invoiceLine1.US_TSCAODSCertIndividual = PartyTypeList.Codes.CustomsBroker;
			invoiceLine1.US_FDAContactName = "SECONDARY BROKER";
			invoiceLine1.InvoiceHeader.US_TSCASignDate = new DateTime(2016, 11, 30);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			action.US_SE_ContactName = "AMY XIANG";
			action.US_SE_ContactPhone = "123456789";

			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			var message = builder.PopulateMessage();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals("PGA Message Generated Correctly",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01EI 69-9999999JC40800000150008888                           
SE11                                   123W                                     
SE13AMY XIANG                               123456789                           
SE20CR B00001000                                                                
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30EX IAN TEST EXPORTER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30SH IAN TEST SHIPPER                                                         
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30DR IAN TEST DISTRIBUTOR                                                     
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30PK IAN TEST PACKAGER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE40001AU TEST PARENT TSCA                                                      
SE6096085000000000015000                                                        
OI        TEST PARENT TSCA                                                      
PG01001EPATS1                                                                   
PG02P                                                                           
PG22             CI     Y11302016                                               
PG21CI PARENT IMPORTER                                                          
SE609608100000                                                                  
OI        TEST SECONDARY TSCA                                                   
PG01002EPATS1                                                                   
PG02P                                                                           
PG22             CI     Y11302016                                               
PG21CI SECONDARY BROKER                                                         
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, false))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals("PGA Message Generated Correctly",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01EI 69-9999999JC40800000150008888                           
SE11                                   123W                                     
SE13AMY XIANG                               123456789                           
SE20CR B00001000                                                                
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE40001AU TEST PARENT TSCA                                                      
SE6096085000000000015000                                                        
OI        TEST PARENT TSCA                                                      
PG01001EPATS1                                                                   
PG02P                                                                           
PG22             CI     Y11302016                                               
PG21CI PARENT IMPORTER                                                          
SE609608100000                                                                  
OI        TEST SECONDARY TSCA                                                   
PG01002EPATS1                                                                   
PG02P                                                                           
PG22             CI     Y11302016                                               
PG21CI SECONDARY BROKER                                                         
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
		}

		public void TestBuildAdditionalBondDetailsInMessage()
		{
			var declaration = GetMergedDeclaration();
			declaration.US_BondType2 = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_ADDCVDSuretyCode = "892";
			declaration.US_BondAmount2 = 100m;
			declaration.US_BondProducerAccNo2 = "897423";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			action.US_SE_ContactName = "NOBODY";
			action.US_SE_ContactPhone = "123456789";

			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			var message = builder.PopulateMessage();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals("SE12 block generated correctly",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01EI 69-9999999JC40800000150008888                           
SE11                                   123W                                     
SE129A 8920000000100897423                                                      
SE13NOBODY                                  123456789                           
SE20CR B00001000                                                                
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30EX IAN TEST EXPORTER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30SH IAN TEST SHIPPER                                                         
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30DR IAN TEST DISTRIBUTOR                                                     
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30PK IAN TEST PACKAGER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE40001AU                                                                       
SE6084717040650000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, false))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals("SE12 block generated correctly",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01EI 69-9999999JC40800000150008888                           
SE11                                   123W                                     
SE129A 8920000000100897423                                                      
SE13NOBODY                                  123456789                           
SE20CR B00001000                                                                
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE40001AU                                                                       
SE6084717040650000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
		}

		[TestDate(2016, 07, 05)]
		public void TestSendImmediateDeliveryMessage()
		{
			var declaration = GetMergedDeclaration();
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_ImmediateDelivery = true;
			declaration.US_EntryDateElectionCode = EntryDateElectionCodeList.Codes.PresentationDate;
			declaration.US_PresentationDate = ZDateTime.Today;

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			action.US_SE_ContactName = "AMY XIANG";
			action.US_SE_ContactPhone = "123456789";

			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));

			var message = builder.PopulateMessage();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals("Test send immediate delivery message",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01EI 69-9999999JC40800000150008888                           
SE11P070516                            123W                                    Y
SE13AMY XIANG                               123456789                           
SE20CR B00001000                                                                
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30EX IAN TEST EXPORTER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30SH IAN TEST SHIPPER                                                         
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30DR IAN TEST DISTRIBUTOR                                                     
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30PK IAN TEST PACKAGER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE40001AU                                                                       
SE6084717040650000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, false))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals("Test send immediate delivery message",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 01EI 69-9999999JC40800000150008888                           
SE11P070516                            123W                                    Y
SE13AMY XIANG                               123456789                           
SE20CR B00001000                                                                
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE40001AU                                                                       
SE6084717040650000015000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
		}

		[TestDate(2022, 02, 11)]
		public void TestGlobalBusinessIdentifiers()
		{
			#region Setup Declarations

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "ABC" + new Random().Next(1000000).ToString();
			importer.OH_FullName = "Test Importer";

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_Code = "SHP" + new Random().Next(1000000).ToString();
			shipper.OH_FullName = "Test Shipper";
			var shipperAddress = shipper.Addresses.AddNew();
			shipperAddress.OA_Address1 = "Shipper Address";
			shipperAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.LegalEntityIdentifier, "1234567890ABCDEFGHIJ", Core.Constants.CountryCodes.UnitedStates);

			var packager = Factory.New<OrgHeader>();
			packager.OH_Code = "PKG" + new Random().Next(1000000).ToString();
			packager.OH_FullName = "Test Packager";
			var packagerAddress = packager.Addresses.AddNew();
			packagerAddress.OA_Address1 = "Packager Address";
			packagerAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.GlobalLocationNumber, "1234567890123", Core.Constants.CountryCodes.UnitedStates);

			var declaration = Factory.New<JobDeclaration>();
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_OH_Importer = importer.PK;
			declaration.IOROrgPK = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.JE_OA_ShipperAddress = shipperAddress.PK;
			declaration.JE_OA_PackagerAddress = packager.MainAddress.PK;

			var invoiceHeader1 = declaration.Invoices.AddNew();
			invoiceHeader1.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader1.JZ_InvoiceAmount = 15000m;
			var invoiceHeader2 = declaration.Invoices.AddNew();
			invoiceHeader2.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader2.JZ_InvoiceAmount = 10000m;
			invoiceHeader2.JZ_OA_PackagerAddress = packagerAddress.PK;

			var invoiceLine1 = invoiceHeader1.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "8471704065";
			invoiceLine1.JI_LinePrice = 15000m;
			invoiceLine1.JI_CustomsQuantity = 1m;
			invoiceLine1.US_UC_NKCountryOfOrigin = "AU";
			var invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "7201000000";
			invoiceLine2.JI_LinePrice = 10000m;
			invoiceLine2.JI_CustomsQuantity = 1m;
			invoiceLine2.US_UC_NKCountryOfOrigin = "HK";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			#endregion

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			var message = builder.PopulateMessage();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals("GBI blocks generated", @"
B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  <E#PLCH> 01               4000000025000                               
SE13CARGOWISE SUPPORT                                                           
SE20CR                                                                          
SE30CN TEST IMPORTER                                                            
SE3515                                                                          
SE36                                                           US               
SE30SH TEST SHIPPER                                                             
SE31LEI 1234567890ABCDEFGHIJ                                                    
SE3515SHIPPER ADDRESS                                                           
SE36                                                           US               
SE40001AU                                                                       
SE50PK TEST PACKAGER                                                            
SE5515                                                                          
SE56                                                           US               
SE6084717040650000015000                                                        
SE40002HK                                                                       
SE50PK TEST PACKAGER                                                            
SE51GLN 1234567890123                                                           
SE5515PACKAGER ADDRESS                                                          
SE56                                                           US               
SE6072010000000000010000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, false))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals("GBI blocks generated", @"
B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  <E#PLCH> 01               4000000025000                               
SE13CARGOWISE SUPPORT                                                           
SE20CR                                                                          
SE30CN TEST IMPORTER                                                            
SE3515                                                                          
SE36                                                           US               
SE40001AU                                                                       
SE6084717040650000015000                                                        
SE40002HK                                                                       
SE6072010000000000010000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
		}

		[TestDate(2019, 10, 09)]
		public void TestASESE60WithSetX()
		{
			var testHelper = new Chapter98HelperTest();
			var declaration = GetMergedDeclaration();
			declaration.JE_TransportMode = "SEA";
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			var invoice = declaration.Invoices[0];
			invoice.JZ_InvoiceAmount = 2000m;
			var invoiceLine01 = invoice.InvoiceLines[0];
			var invoiceLine02 = invoice.InvoiceLines.AddNew();
			var invoiceLine03 = invoice.InvoiceLines.AddNew();

			invoiceLine01.US_SetInd = "X";
			invoiceLine02.US_SetInd = "Y";
			invoiceLine03.US_SetInd = "Y";
			invoiceLine02.JI_ParentID = invoiceLine01.PK;
			invoiceLine03.JI_ParentID = invoiceLine01.PK;
			Assert(invoiceLine02.IsSetVLine);
			Assert(invoiceLine03.IsSetVLine);

			invoiceLine01.JI_Tariff = testHelper.TestCTariff.UE_Tariff;
			invoiceLine02.JI_Tariff = testHelper.TestCTariff.UE_Tariff;
			invoiceLine03.JI_Tariff = testHelper.TestCTariff.UE_Tariff;
			invoiceLine01.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine02.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff;
			invoiceLine03.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff;

			invoiceLine01.JI_LinePrice = 0;
			invoiceLine02.JI_LinePrice = 1000m;
			invoiceLine03.JI_LinePrice = 1000m;
			invoiceLine01.JI_CustomsQuantity = 100m;
			invoiceLine02.JI_CustomsQuantity = 80m;
			invoiceLine03.JI_CustomsQuantity = 60m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var seEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			var action = new EntryHeaderMessageSendingAction(seEntry, ImportMessageStatusList.MessageType.ACECargoRelease, collection);
			var builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
			var message = builder.PopulateMessage();
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals("Line Price should be ZERO for the Entry Line with Set Index X.",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 06EI 69-9999999JC11800000020008888                           
SE13CARGOWISE SUPPORT                                                           
SE20CR B00001000                                                                
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30EX IAN TEST EXPORTER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30SH IAN TEST SHIPPER                                                         
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30DR IAN TEST DISTRIBUTOR                                                     
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30PK IAN TEST PACKAGER                                                        
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE40001AU                                                                       
SE41       00000000                                                             
SE6099038801                                                                    
SE607601103000                                                                  
SE40002XY                                                                       
SE41       00000000                                                             
SE6099038801                                                                    
SE6076011030000000001000                                                        
SE40003XY                                                                       
SE41       00000000                                                             
SE6099038801                                                                    
SE6076011030000000001000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, false))
			{
				builder = new SimplifiedEntryMessageBuilder(seEntry, UpdateActionCode.Add, ACEEntrySummaryMessageSendingOption.New(action));
				message = builder.PopulateMessage();
				AssertMultilineASCIIEquals("Line Price should be ZERO for the Entry Line with Set Index X.",
@"B  8888XJ5SE                                               <<MSGNO PLACEHOLDER>>
SE10AXJ5  00000014 06EI 69-9999999JC11800000020008888                           
SE13CARGOWISE SUPPORT                                                           
SE20CR B00001000                                                                
SE30MF TEST MANUFACTURER                                                        
SE3515NEW ADDRESS                                                               
SE36                                                           US               
SE30CN                                       BBBBBB123                          
SE30BY AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE3515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE36GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE30SE TEST SELLING PARTY(SELLER)                                               
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE30ST IAN TEST SHIP TO PARTY                                                   
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
SE40001AU                                                                       
SE41       00000000                                                             
SE6099038801                                                                    
SE607601103000                                                                  
SE40002XY                                                                       
SE41       00000000                                                             
SE6099038801                                                                    
SE6076011030000000001000                                                        
SE40003XY                                                                       
SE41       00000000                                                             
SE6099038801                                                                    
SE6076011030000000001000                                                        
Y  8888XJ5SE", message.EM_FormattedMessageText);
			}
		}

		[ExpectNoExceptions]
		public void TestASESE56CityNameMaxLength()
		{
			var ace56 = new ASESE56() { CityName = "CHAMPIGNY SUR MARNE CHAMPIGNY SUR MARNE" };
			ace56.Serialise();
		}

		[ExpectNoExceptions]
		public void TestASESE36CityNameMaxlength()
		{
			var ace36 = new ASESE36() { CityName = "NIANSANLI,YIWU, ZHEJIAN NIANSANLI,YIWU, ZHEJIA" };
			ace36.Serialise();
		}

		[ExpectNoExceptions]
		public void TestAEPAPG28CanDimensions()
		{
			var pg28 = new AEPAPG28() { CanDimensions1 = "300000", CanDimensions2 = "500000", CanDimension3 = "600000" };
			pg28.Serialise();
		}

		[ExpectNoExceptions]
		public void TestAEPAPG10CommodityCharacteristicDescription()
		{
			var pg10 = new AEPAPG10() { CommodityCharacteristicDescription = "This is Free form description of the item, either to supplement the above data elements or in place of the above." };
			pg10.Serialise();
		}

		JobDeclaration GetMergedDeclaration()
		{
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

			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_Code = "SHP" + new Random().Next(1000000).ToString();
			shippingLine.OH_FullName = "Test Shipping Line";
			shippingLine.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "OTT1");

			declaration.JE_OH_ShippingLine = shippingLine.PK;
			declaration.JE_RL_NKPortOfLoading = "PERTD";
			declaration.US_SchDEntry = "8888";
			declaration.JE_RL_NKOrigin = "PERTD";
			declaration.US_SchDLoading = "5678";

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

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			return declaration;
		}
	}
}
