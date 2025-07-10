using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
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
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;
using Constants = Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class ACEEntrySummaryMessageBuilderTest : EntrySummaryMessageBuilderAbstractTest
	{
		[TestDate(2025, 04, 24)]
		public void TestSendEndToEndWithSanctionsData()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			invoiceLine.US_SupTariff = "9903.01.20";
			invoiceLine.US_UC_NKCountryOfOrigin = "RU";
			invoiceLine.US_DisclaimSanctions = true;

			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			var fishing1 = invoiceLine1.FishingInformations.AddNew();
			fishing1.US_MethodOfHarvest = "VESSEL";
			fishing1.US_VesselName = "VESSEL NAME";
			fishing1.US_VesselCountry = "GB";
			fishing1.US_VesselIMO = "321546";
			fishing1.US_HarvestedCountry = "AU";
			var fishing2 = invoiceLine1.FishingInformations.AddNew();
			fishing2.US_MethodOfHarvest = "HCF";
			fishing2.US_HarvestedCountry = "TH";

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			var mining2 = invoiceLine2.MiningInformations.AddNew();
			mining2.CountryOfMining = "DE";
			var mining3 = invoiceLine2.MiningInformations.AddNew();
			mining3.CountryOfMining = "FR";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			var action = new EntryHeaderMessageSendingAction(entry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			action.US_CertifyCargoRelease = true;
			var messageBuilder = new ACEEntrySummaryMessageBuilder(entry, ACEEntrySummaryMessageSendingOption.New(action), UpdateActionCode.Replace);
			var message = messageBuilder.PopulateMessage();

			AssertMultilineASCIIEquals(@"B  3902XJ5AE                                               <<MSGNO PLACEHOLDER>>
10RXXX  <E#PLCH> 3902            0110  A          2042925                       
1191-01319900091-013199000                                  IL                  
20AAAA3902          APL EMERALD                                                 
SE13CARGOWISE SUPPORT                                                           
21V123W                                                                         
2200000001PK                                                                    
318B 891                                                                        
319A    00000000101234                                                          
SE20SCEY                                                                        
SE30ST MR IMPORTER                                                              
SE3515                                                                          
SE36                                                           US               
40  001 RUAU041725        0000000050602670000009000    N                        
44COMMERCIAL DESCRIPTION                                                        
47MTHLIATHA191NAK                                                               
47S91-013199000                                                                 
SE50MF MR MANUFACTURER                                                          
SE5515                                                                          
SE56                                                           AU               
5099030120   0000000000 0000000000                                              
504421909720 0000033000 0000010000 000000007000GR                               
SE60Y                                                                           
6250100001250                                                                   
6249900003464                                                                   
40  002 THAU041725        0000000000602670000000000    N                        
47MTHLIATHA191NAK                                                               
47S91-013199000                                                                 
SE50MF MR MANUFACTURER                                                          
SE5515                                                                          
SE56                                                           AU               
50           0000000000 0000000000                                              
6249900000000                                                                   
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
40  003 THAU041725        0000000000602670000000000    N                        
47MTHLIATHA191NAK                                                               
47S91-013199000                                                                 
SE50MF MR MANUFACTURER                                                          
SE5515                                                                          
SE56                                                           AU               
50           0000000000 0000000000                                              
6249900000000                                                                   
SE6201MINE INFO COUNTRY OF MINING                                               
SE63DE                                                                          
SE6202MINE INFO COUNTRY OF MINING                                               
SE63FR                                                                          
895010000000125049900000003464                                                  
9000000033000 00000004714 00000000000 00000000000 00000000000 00000000000       
Y  3902XJ5AE", message.EM_FormattedMessageText);
		}

		public void TestENS54ForAdditionalDeclarationType09()
		{
			invoiceLine.US_SupTariff = CusEntryLine.Tariff99034529;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true);
			var message = messageBuilder.PopulateMessage();
			var ens54 = message.MessageBlock.MessageBlocks.OfType<AENS54>().FirstOrDefault();
			AssertNotNull(ens54);
			AssertEquals(AdditionalDeclarationTypeCodeList.Codes._09, ens54.ImportersAdditionalDeclarationTypeCode);
			AssertEquals(CusEntryLine._201BIFACCERT, ens54.ImportersAdditionalDeclarationInformation);
		}

		public void TestENS54ForAdditionalDeclarationType10()
		{
			invoiceLine.US_SupTariff = CusEntryLine.Tariff99039109;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true);
			var message = messageBuilder.PopulateMessage();
			var ens54 = message.MessageBlock.MessageBlocks.OfType<AENS54>().FirstOrDefault();
			AssertNotNull(ens54);
			AssertEquals(AdditionalDeclarationTypeCodeList.Codes._10, ens54.ImportersAdditionalDeclarationTypeCode);
			AssertEquals(CusEntryLine._301STSCERT, ens54.ImportersAdditionalDeclarationInformation);
		}

		public void TestENS54ForADDOrCVDCertificationClaimed()
		{
			invoiceLine.US_ADD_Cert = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true);
			var message = messageBuilder.PopulateMessage();
			var ens54 = message.MessageBlock.MessageBlocks.OfType<AENS54>().FirstOrDefault();
			AssertNotNull(ens54);
			AssertEquals(AdditionalDeclarationTypeCodeList.Codes._06, ens54.ImportersAdditionalDeclarationTypeCode);
			AssertEquals("ADCVD CERT", ens54.ImportersAdditionalDeclarationInformation);

			invoiceLine.US_ADD_Cert = false;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true);
			message = messageBuilder.PopulateMessage();
			ens54 = message.MessageBlock.MessageBlocks.OfType<AENS54>().FirstOrDefault();
			AssertNull(ens54);
		}

		public void TestPopulateADCVDNonReimbursementStatementWhenChildLineExists()
		{
			invoiceLine.US_IsParent = true;
			var childLine01 = invoiceHeader.JobComInvoiceLines.AddNew();
			childLine01.US_ADCVDStat = ZString.Empty;
			childLine01.JI_ParentID = invoiceLine.PK;
			var childLine02 = invoiceHeader.JobComInvoiceLines.AddNew();
			childLine02.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.OnceOff;
			childLine02.JI_ParentID = invoiceLine.PK;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true);
			var message = messageBuilder.PopulateMessage();
			var aENS40 = message.MessageBlock.MessageBlocks.OfType<AENS40>().FirstOrDefault();
			AssertContains("Y", aENS40.ADCVDNonReimbursementStatement);

			invoiceLine.US_SetInd = "X";
			childLine01.US_SetInd = "V";
			childLine02.US_SetInd = "V";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true);
			message = messageBuilder.PopulateMessage();
			aENS40 = message.MessageBlock.MessageBlocks.OfType<AENS40>().FirstOrDefault();
			AssertNotContains("Y", aENS40.ADCVDNonReimbursementStatement);
		}

		[TestDate(2018, 08, 09)]
		public void TestSendMessageForXVVSets()
		{
			var testHelper = new Chapter98HelperTest();

			testHelper.ParentLine.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff;
			testHelper.ParentLine.US_SetInd = "X";

			testHelper.ChildLine.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff;
			testHelper.ChildLine.JI_Tariff = testHelper.TestCTariff.UE_Tariff;
			testHelper.ChildLine.US_SetInd = "V";
			testHelper.ChildLine.JI_LinePrice = 100m;

			var incoineHeader = testHelper.ChildLine.InvoiceHeader;
			var invoiceLine3 = incoineHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_ParentID = testHelper.ChildLine.PK;
			invoiceLine3.US_SetInd = "V";
			invoiceLine3.JI_Tariff = "8204200000";
			invoiceLine3.JI_LinePrice = 200m;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			var entry = testHelper.Charpter98Job.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertMultilineASCIIEquals(@"B  8888XJ5AE                                               <<MSGNO PLACEHOLDER>>
10AXJ5  <E#PLCH>     B00001000       0X                                         
11                                                                              
20                                                                              
40  001X                  0000000000     0000000000    N                        
5099038801   0000012000 0000000000                                              
507601103000 0000021000 0000000300 000000000000KG                               
6249900000104                                                                   
40  002V                  0000000000     0000000000    N                        
5099038801   0000000000 0000000000                                              
507601103000 0000000000 0000000100 000000000000KG                               
507601103000 0000000000 0000000000 000000000000KG                               
508204200000 0000000000 0000000200 000000000000X                                
8949900000002567                                                                
9000000033000 00000002567 00000000000 00000000000 00000000000 00000000000       
Y  8888XJ5AE", message.EM_FormattedMessageText);
		}

		[TestDate(2025, 02, 01)]
		public void TestSendMessageForXVVSetsWithMultipleProvTariffs()
		{
			#region Setup Tariffs
			var tariff99038803 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038803", "7", 0.25m, ZString.Empty);
			var tariff99030123 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030123", "0", 0m, ZString.Empty);
			var tariff8424201000 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "8424201000", "7", 0.029m, "X");
			var tariff9503000013 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "9503000013", "7", 0m, "NO");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("US", "HSN");
			var dutyRateType = helper.CreateNewOrGetExistingRateType("US", "DTY", "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			var tradeGroup = helper.CreateTradeGroup("CN", "CN", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var tariffView99030123 = helper.CreateTariff("US", hsnTariffType.PK, "99030123", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var rate99030123 = helper.CreateRate(tariffView99030123, rateCode.PK, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "0");
			helper.CreateCusApplicability(rate99030123, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute("RULE", "A99", tariffView99030123);

			Factory.Save();
			#endregion

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			invoiceHeader.InvoiceLines.RemoveAndDeleteAll();

			var xInvoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			xInvoiceLine.US_SetInd = "X";
			xInvoiceLine.JI_Tariff = tariff8424201000.UE_Tariff;
			xInvoiceLine.US_SupTariff = tariff99038803.UE_Tariff;
			xInvoiceLine.SupFormattedAdditionalTariff1 = tariff99030123.UE_Tariff;
			xInvoiceLine.JI_LinePrice = 0m;
			xInvoiceLine.JI_CustomsQuantity = 10m;
			xInvoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			xInvoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;

			var vInvoiceLineParent = invoiceHeader.JobComInvoiceLines.AddNew();
			vInvoiceLineParent.JI_ParentID = xInvoiceLine.PK;
			vInvoiceLineParent.US_SetInd = "V";
			vInvoiceLineParent.JI_Tariff = tariff8424201000.UE_Tariff;
			vInvoiceLineParent.US_SupTariff = tariff99038803.UE_Tariff;
			vInvoiceLineParent.SupFormattedAdditionalTariff1 = tariff99030123.UE_Tariff;
			vInvoiceLineParent.JI_LinePrice = 10000m;
			vInvoiceLineParent.JI_CustomsQuantity = 10m;
			vInvoiceLineParent.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			vInvoiceLineParent.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;

			var vInvoiceLineChild = invoiceHeader.JobComInvoiceLines.AddNew();
			vInvoiceLineChild.JI_ParentID = xInvoiceLine.PK;
			vInvoiceLineChild.US_SetInd = "V";
			vInvoiceLineChild.JI_Tariff = tariff9503000013.UE_Tariff;
			vInvoiceLineChild.US_SupTariff = tariff99030123.UE_Tariff;
			vInvoiceLineChild.JI_LinePrice = 2000m;
			vInvoiceLineChild.JI_CustomsQuantity = 10m;
			vInvoiceLineChild.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			vInvoiceLineChild.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertMultilineASCIIEquals(@"B  3902XJ5AE                                               <<MSGNO PLACEHOLDER>>
10ASV9  <E#PLCH> 3902B00001000   0110 X           2020625                       
1191-01319900091-013199000                                  IL                  
20AAAA3902          APL EMERALD                                                 
21V123W                                                                         
2200000001PK                                                                    
318B 891                                                                        
319A    00000000101234                                                          
40  001XCNCN012525        0000000050602670000000000    N                        
47MTHLIATHA191NAK                                                               
47C91-013199000                                                                 
47S91-013199000                                                                 
5099030123   0000000000 0000000000                                              
5099038803   0000300000 0000000000                                              
508424201000 0000034800 0000012000 000000001000X                                
6249900004157                                                                   
6250100001500                                                                   
40  002VCNCN012525        0000000042602670000000000    N                        
47MTHLIATHA191NAK                                                               
47C91-013199000                                                                 
47S91-013199000                                                                 
5099030123   0000000000 0000000000                                              
5099038803   0000000000 0000000000                                              
508424201000 0000000000 0000010000 000000001000X                                
40  003VCNCN012525        0000000008602670000000000    N                        
47MTHLIATHA191NAK                                                               
47C91-013199000                                                                 
47S91-013199000                                                                 
5099030123   0000000000 0000000000                                              
509503000013 0000000000 0000002000 000000001000NO                               
894990000000415750100000001500                                                  
9000000334800 00000005657 00000000000 00000000000 00000000000 00000000000       
Y  3902XJ5AE", message.EM_FormattedMessageText);
		}

		[TestDate(2025, 03, 01)]
		public void TestSendMessageForSupAdditionalTariffs()
		{
			#region Setup Tariffs

			var tariff99030120 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99030120", "7", 0.1m, "NO");
			var tariff99038802 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "99038802", "7", 0.25m, "M3");
			var tariff4823690040 = USCTariffTest.CreateNewTariffIfNotExist(Factory, "4823690040", "7", 0m, "KG");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("US", "HSN");
			var dutyRateType = helper.CreateNewOrGetExistingRateType("US", "DTY", "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			var tradeGroup = helper.CreateTradeGroup("CN", "CN", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var tariffView99030120 = helper.CreateTariff("US", hsnTariffType.PK, "99030120", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var rate99030120 = helper.CreateRate(tariffView99030120, rateCode.PK, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "0");
			helper.CreateCusApplicability(rate99030120, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute("RULE", "A99", tariffView99030120);

			var tariffView99038802 = helper.CreateTariff("US", hsnTariffType.PK, "99038802", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			var rate99038802 = helper.CreateRate(tariffView99038802, rateCode.PK, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "0");
			helper.CreateCusApplicability(rate99038802, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateTariffAttribute("RULE", "A99", tariffView99038802);

			#endregion

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_FormattedTariff = "4823.69.0040";
			invoiceLine.SupTariffFormatted = "9903.01.20";
			invoiceLine.SupFormattedAdditionalTariff1 = "9903.88.02";
			invoiceLine.JI_CustomsQuantity = 500m;
			invoiceLine.US_SupQty1 = 100m;
			invoiceLine.US_SupAdditionalTariff1Qty = 150m;
			invoiceLine.JI_LinePrice = 5000m;
			invoiceLine.US_SupGoodsValue = 200m;
			invoiceLine.US_SupAdditionalTariff1GoodsValue = 500m;
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertMultilineASCIIEquals(@"B  8888XJ5AE                                               <<MSGNO PLACEHOLDER>>
10AXJ5  <E#PLCH>     B00001001   03  0X                                         
11                                                                              
20                                                                              
40  001 CN                0000000000     0000000000    N                        
5099038802   0000012500 0000000500 000000015000M3                               
5099030120   0000002000 0000000200 000000010000NO                               
504823690040 0000000000 0000005000 000000050000KG                               
6249900001732                                                                   
8949900000002567                                                                
9000000014500 00000002567 00000000000 00000000000 00000000000 00000000000       
Y  8888XJ5AE", message.EM_FormattedMessageText);
		}

		public void TestENS53And88ForTIB()
		{
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_SchDEntry = "2704";
			invoiceLine.JI_Tariff = "8431390010";
			invoiceLine.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.OnceOff;
			invoiceLine.US_ADDCaseNo = "A588201000";
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;

			if (invoiceLine.AntidumpingDutyCase == null)
			{
				SetUpADDCVDCaseIfTestingNotSendingToCustoms("A588201000", new ZDateTime(1989, 5, 15), "JP", "8431390010", 0.4583m, 0m);
			}

			invoiceLine.US_IsBondedADD = false;
			invoiceLine.US_UC_NKCountryOfOrigin = "JP";
			invoiceLine.JI_OA_ExporterAddress = manufacturer.MainAddress.PK;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true);
			var message = messageBuilder.PopulateMessage();

			var ens53 = message.MessageBlock.MessageBlocks.OfType<AENS53>().FirstOrDefault();
			AssertNotNull(ens53);
			AssertEquals(45.83m, ens53.CaseDepositRate);

			var ens88 = message.MessageBlock.MessageBlocks.OfType<AENS88>().FirstOrDefault();
			AssertNotNull(ens88);
		}

		public void TestCoffeeFeeWith61()
		{
			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1234", "TEST NAME", startDate, endDate);
			var attributeNameState = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.State, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			var attributeState = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameState.ZXE_Name, USStateList.Codes.PuertoRico);
			newFactory.Save();

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.BrettsBirthday;
			tariff.UE_DateTo = ZDateTime.MaxSmallDateTime;
			tariff.UE_ShortDescription = "Cofee Fee Test";
			tariff.UE_Unit1 = "KG";

			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Coffee;
			dutyRate.UD_TaxFeeFlag = "1";
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateFirstQuantity;
			dutyRate.UD_TaxFeeSpecificRate = 0.5m;
			Factory.Save();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;

			var filer = new EntryFiler();
			filer.EntryFilerCode = "XJ5";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

			declaration.US_EntryFilerCode = "XJ5";
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = "SEA";

			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_CertifyCargoRelease = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceNumber = "INV-Cot";
			invoiceHeader.JZ_InvoiceAmount = 9901.25m;

			var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = tariff.UE_Tariff;
			invoiceLine1.JI_CustomsQuantity = 840m;
			invoiceLine1.JI_LinePrice = 2000m;
			invoiceLine1.JI_Weight = 3285.59m;
			invoiceLine1.JI_WeightUQ = "KG";
			invoiceLine1.JI_CustomsSecondQuantity = 3285.59m;
			declaration.US_SchDEntry = "1234";
			Factory.Save();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var messageBlock = (AENS61)message.MessageBlock.MessageBlocks.Find(x => x is AENS61);

			AssertNotNull(messageBlock);
			AssertEquals(messageBlock.AccountingClassCode, Core.Constants.USCustoms.FeeCodes.Coffee);
			AssertEquals(messageBlock.UserFeeAmount, 420m);

			var message90Block = (AENS90)message.MessageBlock.MessageBlocks.Find(x => x is AENS90);
			AssertNotNull(message90Block);
			AssertEquals(message90Block.GrandTotalOtherRevenueAmount, 420m);

			dutyRate.UD_TaxFeeSpecificRate = 0m;
			Factory.Save();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			message = builder.PopulateMessage();

			messageBlock = (AENS61)message.MessageBlock.MessageBlocks.Find(x => x is AENS61);

			AssertNotNull(messageBlock);
			AssertEquals(messageBlock.AccountingClassCode, Core.Constants.USCustoms.FeeCodes.Coffee);
			AssertEquals(messageBlock.UserFeeAmount, 0m);

			var messageBlock89 = (AENS89)message.MessageBlock.MessageBlocks.Find(x => x is AENS89);

			AssertNotNull(messageBlock89);
			AssertEquals("No Coffee 672 class code in AENS89 block", false, messageBlock89.AccountingClassCode1 == Core.Constants.USCustoms.FeeCodes.Coffee
				|| messageBlock89.AccountingClassCode2 == Core.Constants.USCustoms.FeeCodes.Coffee
				|| messageBlock89.AccountingClassCode3 == Core.Constants.USCustoms.FeeCodes.Coffee
				|| messageBlock89.AccountingClassCode4 == Core.Constants.USCustoms.FeeCodes.Coffee
				|| messageBlock89.AccountingClassCode5 == Core.Constants.USCustoms.FeeCodes.Coffee
				);

			AssertEquals("311 code generated in AENS89 block", true, messageBlock89.AccountingClassCode1 == Core.Constants.USCustoms.FeeCodes.MerchandiseInformal);
		}

		public void TestIRTaxWith98()
		{
			invoiceLine.JI_Tariff = "2208905000";
			invoiceLine.JI_LinePrice = 28143.72m;
			invoiceLine.US_SupTariff = "9802005060";
			invoiceLine.JI_CustomsQuantity = 1821.60m;

			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var builder = new ACEEntrySummaryMessageBuilderForTesting(invoiceLine.Declaration.ActiveEntryHeaders.EntrySummaryEntry, true, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var messageBlock = (AENS60)message.MessageBlock.MessageBlocks.Find(x => x is AENS60);
			AssertNotNull("60 with Excise Tax is constructed", messageBlock);
			Assert("60 with Excise Tax is constructed", messageBlock.IRTaxAmount > 0);
			AssertEquals("016", messageBlock.AccountingClassCode);

			invoiceLine.JI_CustomsQuantity = 0.01m;
			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			builder = new ACEEntrySummaryMessageBuilderForTesting(invoiceLine.Declaration.ActiveEntryHeaders.EntrySummaryEntry, true, true, UpdateActionCode.Add);
			message = builder.PopulateMessage();
			messageBlock = (AENS60)message.MessageBlock.MessageBlocks.Find(x => x is AENS60);
			AssertNotNull("60 with Excise Tax is constructed", messageBlock);
			AssertEquals("016", messageBlock.AccountingClassCode);
		}

		public void TestADDSpecificRate()
		{
			var caseRecord = Factory.LoadFromNaturalKey<USCACCase>(USCACCaseSchema.U5_CaseNumber, "A570904091");
			if (caseRecord == null)
			{
				caseRecord = Factory.New<USCACCase>();
				caseRecord.U5_CaseNumber = "A570904091";
				caseRecord.U5_CaseStatus = "AC";
				caseRecord.U5_CaseStatusDate = new ZDateTime(2007, 4, 27);
			}

			caseRecord.CaseTariffs.DeleteAll();
			var caseTariff = caseRecord.CaseTariffs.AddNew();
			caseTariff.U9_TariffNumber = "3802100000";

			caseRecord.CaseRates.DeleteAll();
			var caseRate = caseRecord.CaseRates.AddNew();
			caseRate.U6_Unit = "KG";
			caseRate.U6_SpecificRate = 0.44m;
			caseRate.U6_EffectiveDate = new ZDateTime(2012, 11, 09);

			invoiceLine.US_ADDCaseNo = "A570904091";
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			invoiceLine.US_ADDQty = 89m;
			invoiceLine.US_UC_NKCountryOfExport = "CN";
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";

			invoiceLine.Declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var builder = new ACEEntrySummaryMessageBuilderForTesting(invoiceLine.Declaration.ActiveEntryHeaders.EntrySummaryEntry, true, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var messageBlock = (AENS53)message.MessageBlock.MessageBlocks.Find(x => x is AENS53);
			AssertNotNull(messageBlock);
			AssertEquals("should not send as %", 0.44m, messageBlock.CaseDepositRate);
		}

		[TestDate(2018, 08, 09)]
		public void TestSendMessageForXVVCombined()
		{
			var testHelper = new Chapter98HelperTest();

			testHelper.ParentLine.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff;
			testHelper.ParentLine.US_SetInd = "X";

			testHelper.ChildLine.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff;
			testHelper.ChildLine.JI_Tariff = testHelper.TestCTariff.UE_Tariff;
			testHelper.ChildLine.US_SetInd = "V";
			testHelper.ChildLine.JI_LinePrice = 100m;

			var incoineHeader = testHelper.ChildLine.InvoiceHeader;
			var invoiceLine3 = incoineHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_ParentID = testHelper.ParentLine.PK;
			invoiceLine3.US_SetInd = "V";
			invoiceLine3.JI_Tariff = "8204200000";
			invoiceLine3.JI_LinePrice = 200m;

			var invoiceLine4 = incoineHeader.InvoiceLines.AddNew();
			invoiceLine4.JI_ParentID = testHelper.ParentLine.PK;
			invoiceLine4.US_SetInd = "V";
			invoiceLine4.JI_Tariff = "8205400000";
			invoiceLine4.JI_LinePrice = 300m;

			var invoiceLine5 = incoineHeader.InvoiceLines.AddNew();
			invoiceLine5.JI_ParentID = testHelper.ParentLine.PK;
			invoiceLine5.US_SetInd = "V";
			invoiceLine5.JI_Tariff = "4202999000";
			invoiceLine5.JI_LinePrice = 400m;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			var entry = testHelper.Charpter98Job.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertMultilineASCIIEquals(@"B  8888XJ5AE                                               <<MSGNO PLACEHOLDER>>
10AXJ5  <E#PLCH>     B00001000       0X                                         
11                                                                              
20                                                                              
40  001X                  0000000000     0000000000    N                        
5099038801   0000040000 0000000000                                              
507601103000 0000070000 0000001000 000000000000KG                               
6249900000346                                                                   
40  002V                  0000000000     0000000000    N                        
5099038801   0000000000 0000000000                                              
507601103000 0000000000 0000000100 000000000000KG                               
40  003V                  0000000000     0000000000    N                        
508204200000 0000000000 0000000200 000000000000X                                
40  004V                  0000000000     0000000000    N                        
508205400000 0000000000 0000000300 000000000000X                                
40  005V                  0000000000     0000000000    N                        
504202999000 0000000000 0000000400                                              
8949900000002567                                                                
9000000110000 00000002567 00000000000 00000000000 00000000000 00000000000       
Y  8888XJ5AE", message.EM_FormattedMessageText);
		}

		/// <summary>
		[TestDate(2009, 1, 5)]
		/// Submit an entry summary for which the merchandise is subject to the US – Israel Free Trade Area Agreement.:US – Israel Free Trade Area Agreement
		/// Country of Origin Code	EG
		/// Country of Export Code	EG
		/// HTS Number	6203424051
		/// Eligible for the US-Israel Free Trade Area Agreement	Yes
		/// </summary>
		public void Test02()
		{
			if (!sendTestMessagesToCustoms)
			{
				declaration.US_EstimatedEntryDate = new ZDateTime(2009, 1, 5);// Duty rate date will be this and tariff will be found
			}

			invoiceLine.JI_Tariff = "6203424051";
			invoiceLine.US_UC_NKCountryOfExport = "EG";
			invoiceLine.US_UC_NKCountryOfOrigin = "EG";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.N;
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			invoiceLine.JI_CustomsQuantity = 5684m;
			invoiceLine.JI_CustomsSecondQuantity = 542m;

			manufacturerCode.OK_CustomsRegNo = "EGPEPCOLCAI";

			MergeAndSend("ACE02");

			AssertEquals("Precondition", BondTypeList.Codes.ContinuousBond, declaration.US_BondType);
			var a31 = ((MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0]).MessageBlock.MessageBlocks.Find(x => x is AENS31);
			AssertEquals(BondDesignationCodeList.Codes.BasicBond, ((AENS31)a31).BondDesignationTypeCode);
		}

		/// <summary>
		/// Submit an entry summary indicating that the entry and entry summary are being filed at the time of entry.:Live Entry Indicator
		/// Country of Origin Code	HK
		/// HTS Number	3506990000
		/// Live Entry Indicator	Yes
		/// </summary>
		public void Test03()
		{
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;
			invoiceLine.JI_Tariff = "3506990000";
			invoiceLine.US_UC_NKCountryOfExport = "HK";
			invoiceLine.US_UC_NKCountryOfOrigin = "HK";
			invoiceLine.JI_CustomsQuantity = 10m;
			MergeAndSend("ACE03");
		}

		/// <summary>
		/// Submit an entry summary with the following surety and bond information.:Single Entry Bond with Surety and Bond Information
		/// Importer of Record Number	91-013199000
		/// Surety Code	891
		/// Bond Type Code	9
		/// Bond Amount	$150,000
		/// Bond Producer Account Number	AB12345678
		/// </summary>
		public void Test04()
		{
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;
			declaration.US_BondType = BondTypeList.Codes.SingleTransactionBond;
			declaration.US_SuretyCode = "891";
			declaration.US_BondCalcCode = SEBCalculationList.Codes.MAN;
			declaration.US_BondAmount = 150000m;
			declaration.US_BondProducerAccNo = "AB12345678";

			invoiceLine.JI_LinePrice = 50000m;
			invoiceLine.JI_Tariff = "3506990000";
			invoiceLine.JI_CustomsQuantity = 10m;

			MergeAndSend("ACE04");
		}

		/// <summary>
		/// Submit an entry summary with the following line item data.:Census Warning Override
		/// HTS Number	8483105000
		/// Value of Goods Amount	$32,424
		/// Quantity	1
		/// Unit of Measure	KG
		/// Census Warning Condition Override Code 	04
		/// </summary>
		public void Test05()
		{
			invoiceLine.JI_Tariff = "8483105000";
			invoiceLine.US_UC_NKCountryOfExport = "HK";
			invoiceLine.US_UC_NKCountryOfOrigin = "HK";

			invoiceLine.JI_LinePrice = 32424m;
			invoiceLine.JI_CustomsQuantity = 1m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			invoiceLine.CensusWarningOverrides.AddNew(CensusWarningCodeList.Codes.HighValueDividedByQty1, CensusOverrideCodeList.Codes._04);

			MergeAndSend("ACE05");
		}

		/// <summary>
		/// Submit an entry summary with the following trade agreement product claim code.:Product Claim Code “M”
		/// Country of Origin Code	CN
		/// HTS Number	5701104000
		/// Product Claim Code	M
		/// </summary>
		public void Test06()
		{
			invoiceLine.JI_Tariff = "5701104000";
			invoiceLine.US_UC_NKCountryOfExport = "CN";
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.M;
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_CustomsSecondQuantity = 100m;//tariff requires SecondQuantity

			MergeAndSend("ACE06");
		}

		/// <summary>
		/// Submit an entry summary with the following data.:MOT/Port of Unlading
		/// Mode of Transportation Code	11
		///	District/Port of Unlading	3205
		/// In-Transit Number	111176391
		/// Master Bill of Lading Number	KEEA879348
		/// House Bill of Lading Issuer Code	EXDO
		/// House Bill of Lading Number	KASEA901105
		/// </summary>
		public void Test07()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var port = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3205", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_SchDArrival = "3205";
			AssertHasMessageError(declaration.US_SchDArrivalInfo, ValidationConstants.Declaration.InvalidPortForTransportMode(TransportTypeList.Codes.Sea));
			declaration.JE_MasterBill = "KEEA879348";
			declaration.JE_HouseBill = "KASEA901105";
			declaration.PrimaryHouseBill.US_UI_NKBillIssuerSCAC = "EXDO";
			declaration.PrimaryHouseBill.CU_NoOfPacks = 1m;
			declaration.PrimaryHouseBill.CU_PackType = "PK";
			declaration.JE_PrimaryITNumber = "111176391";
			declaration.US_ITDate = ZDateTime.Today.AddDays(-7);

			MergeAndSend("ACE07", false, true);
		}

		/// <summary>
		/// Submit an entry summary with the following ATPDEA data.  Replace the # characters in the ATPDEA Certificate Number with numeric characters.:Andean Trade Promotion and Drug Eradication Act (ATPDEA)
		/// Country of Origin Code	CO
		/// HTS Number 1, Line 1	98211119
		/// HTS Number 2, Line 1	6212109020
		/// ATPDEA Certificate Number	#AN######
		/// </summary>
		public void Test08_Failed()
		{
			invoiceLine.JI_Tariff = "6212109020";
			invoiceLine.US_SupTariff = "98211119";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsSecondQuantity = 532m;
			invoiceLine.US_UC_NKCountryOfExport = "CO";
			invoiceLine.US_UC_NKCountryOfOrigin = "CO";
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._07, "0AN123456");
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			MergeAndSend("ACE08", false, true);// 98211119 is an expired tariff Check with Phyllis
		}

		/// <summary>
		/// Submit an entry summary with the following data.:In-Transit Number
		/// In-Transit Number	V1111124247
		/// Manifested Quantity	287
		/// Manifested Quantity Unit of Measure Code	AE
		/// </summary>
		public void Test09()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var port = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3201", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTransportModeForCusCodeList(port.PK, TransportTypeList.Codes.Sea);
			var attributeNameUnlading = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Unlading, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			helper.CreateNewOrGetExistingCusCodeListAttribute(port.PK, attributeNameUnlading.ZXE_Name, "Y");
			Factory.Save();

			declaration.JE_PrimaryITNumber = "V1111124247";
			declaration.US_ITDate = ZDateTime.Today.AddDays(-7);
			declaration.US_SchDArrival = "3201";

			declaration.PrimaryMasterBill.CU_NoOfPacks = 287m;
			declaration.PrimaryMasterBill.CU_PackType = "AE";
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine.JI_Tariff = "3506990000";
			invoiceLine.JI_CustomsQuantity = 10m;//tariff requires customs quantity
			MergeAndSend("ACE09");
		}

		/// <summary>
		/// Submit an entry summary with the following data.:Central America Free Trade Agreement (CAFTA)
		/// (If also certifying for cargo release, remember to provide appropriate FDA data based on the HTS Number.)
		/// HTS Number	0709902000
		/// Country of Origin	HN
		/// Value of Goods Amount	$105,000
		/// Eligible as a CAFTA Originating Claim	Yes
		/// </summary>
		public void Test10()
		{
			invoiceLine.JI_Tariff = "0709902000";
			invoiceLine.US_UC_NKCountryOfExport = "HN";
			invoiceLine.US_UC_NKCountryOfOrigin = "HN";
			invoiceLine.JI_LinePrice = 105000m;
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.P;
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USCHI";

			var fda = invoiceLine.FDAs.AddNew();
			fda.US_FDAProductCode = "24FHY06";
			fda.US_UC_NKFDAProduction = "HN";
			fda.US_PFT = "M";
			fda.US_PFR = "12345678901";
			fda.US_FDAQty1 = 1000m;
			fda.US_FDAMeasure1 = "KG";

			declaration.JE_OH_FDASubmitter = declaration.IOROrgPK;
			declaration.FDASubmitter.MainAddress.OA_City = "Chicago";
			declaration.FDASubmitter.MainAddress.OA_Address1 = "87432 Main Road";
			declaration.FDASubmitter.MainAddress.OA_PostCode = "60125";
			declaration.FDASubmitter.MainAddress.OA_Phone = "5555555555";
			DeclarationTestHelper.AddPGAContact(declaration.FDASubmitter, "John", "Smith", "5555555555", null, null);
			var wrapper = OrgHeaderWrapper.New(declaration.FDASubmitter);
			wrapper.ZO_SubmitterFirmType = SubmitterFirmTypeList.Codes.I;
			Factory.Save();

			declaration.US_FDAContactName = "Smith John";
			declaration.US_FDAContactPhoneNo = "5555555555";
			declaration.US_FDAContactEmail = "smith.john@somewhere.com";
			declaration.US_FDAADTA = ZDateTime.Today.AddDays(-1).AddHours(10).AddMinutes(20);
			declaration.US_US_NKLocationOfGoods = "A001";
			declaration.US_EntryDate = ZDateTime.Today;
			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.US_CertifyCargoRelease = true;
			declaration.US_FDAAPC = "3902";
			declaration.US_SchDArrival = "3902";

			MergeAndSend("ACE10", true, true);
		}

		/// <summary>
		/// Submit an entry summary with the following in-transit information.:In-Transit Date Validation
		/// Date of Importation	02/01/YY
		/// Date of Exportation	01/21/YY
		/// In-Transit Date	01/22/YY
		/// In-Transit Number	111271425
		/// </summary>
		public void Test11()
		{
			declaration.JE_ExportDate = new ZDateTime(ZDateTime.Today.Year, 1, 21);
			declaration.JE_DateOfArrival = new ZDateTime(ZDateTime.Today.Year, 2, 1);
			declaration.US_ITDate = new ZDateTime(ZDateTime.Today.Year, 1, 22);
			declaration.JE_PrimaryITNumber = "111271425";
			declaration.US_SchDArrival = "2102";
			declaration.US_CertifyCargoRelease = true;

			MergeAndSend("ACE11", true, true);
		}

		/// <summary>
		/// Submit an entry summary with the following in-transit information.:Estimated Date of Arrival Validation
		/// Date of Importation	01/25/YY
		/// Date of Exportation	02/10/YY
		/// In-Transit Date	02/01/YY
		/// In-Transit Number	115581395
		/// </summary>
		public void Test12()
		{
			declaration.JE_ExportDate = new ZDateTime(ZDateTime.Today.Year, 2, 10);
			declaration.JE_DateOfArrival = new ZDateTime(ZDateTime.Today.Year, 1, 25);
			declaration.US_ITDate = new ZDateTime(ZDateTime.Today.Year, 1, 2);
			declaration.JE_PrimaryITNumber = "115581395";
			declaration.US_SchDArrival = "2102";
			declaration.US_CertifyCargoRelease = true;

			MergeAndSend("ACE12", true, true);
		}

		/// <summary>
		/// Made to Measure Suits
		/// HTS Number	6203122010
		/// Country of Origin Code	HK
		/// Product Claim Code	G
		/// </summary>
		[TestDate(2009, 1, 5)]
		public void Test13()
		{
			if (!sendTestMessagesToCustoms)
			{
				declaration.US_EstimatedEntryDate = new ZDateTime(2009, 1, 5);
			}

			invoiceLine.JI_Tariff = "6203122010";
			invoiceLine.US_UC_NKCountryOfExport = "HK";
			invoiceLine.US_UC_NKCountryOfOrigin = "HK";
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.G;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.No;
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_CustomsSecondQuantity = 500m;
			MergeAndSend("ACE13");
		}

		/// <summary>
		/// Multiple Bills of Lading
		/// In-Transit Number	111271845
		/// Master Bill Number	9786543
		/// House Bill Number	15075
		/// Sub-House Bill Number 1	H273
		/// Manifested Quantity – Bill 1	5 CTNS
		/// Sub-House Bill Number 2	J878
		/// Manifested Quantity – Bill 1	10 CTNS
		/// </summary>
		public void Test14()
		{
			declaration.JE_MasterBill = "9786543";
			declaration.JE_HouseBill = "15075";
			declaration.PrimaryHouseBill.US_UI_NKBillIssuerSCAC = "APLU";
			declaration.PrimaryMasterBill.US_UI_NKBillIssuerSCAC = "APLU";
			declaration.US_ITDate = ZDateTime.Today.AddDays(1);
			declaration.US_SchDArrival = "3201";
			invoiceLine.JI_Tariff = "3506990000";

			var subHouse1 = declaration.Bills.AddNew();
			subHouse1.CU_CU_ParentBill = declaration.PrimaryHouseBill.PK;
			subHouse1.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHouse1.CU_BillNum = "H273";
			subHouse1.CU_NoOfPacks = 15m;
			subHouse1.CU_PackType = "CTN";
			subHouse1.ITAndSplitDetails.AddNew().US_ITNumber = "111271845";

			var subHouse2 = declaration.Bills.AddNew();
			subHouse2.CU_CU_ParentBill = declaration.PrimaryHouseBill.PK;
			subHouse2.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHouse2.CU_BillNum = "J878";
			subHouse2.CU_NoOfPacks = 110m;
			subHouse2.CU_PackType = "CTN";
			subHouse2.ITAndSplitDetails.AddNew().US_ITNumber = "111271845";

			MergeAndSend("ACE14", true, true);
		}

		/// <summary>
		/// Submit an entry summary with the following commodity and diamond certificate information.:Diamond Certificate
		/// HTS Number	7102211020
		/// Diamond Certificate Number	EC0003830
		/// </summary>
		public void Test15()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._06, "EC0003830", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			Factory.Save();

			invoiceLine.JI_Tariff = "7102211020";
			invoiceLine.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._06, "EC0003830");
			invoiceLine.JI_CustomsQuantity = 10m;//tariff requires customs quantity
			MergeAndSend("ACE15");
		}

		/// <summary>
		/// Submit a “Formal” entry summary with the following carrier information.:Airline Carrier Code
		/// Mode of Transport Code	40
		/// Carrier Code	*F
		/// </summary>
		public void Test16()
		{
			SetupAirCarrierAndTariff("ACE16");
		}

		/// <summary>
		/// Submit a “Formal” entry summary for which the payment will be designated for Periodic Monthly Statement (PMS) processing. :PMS Statement Designation
		/// Payment Type Code	6 or 7
		/// Preliminary Statement Print Date	10 days from the current date
		/// Periodic Statement Month	Month following the current month 
		/// Statement Client Branch Identifier	Leave blank
		/// </summary>
		public void Test17()
		{
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByPeriodicPrintDateAndFilerDate;
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(10);

			while (declaration.US_PreliminaryStatementPrintDateInfo.HasMessageError(WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHoliday) ||
				declaration.US_PreliminaryStatementPrintDateInfo.HasMessageError(WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHolidayOrWeekend))
			{
				declaration.US_PreliminaryStatementPrintDate = declaration.US_PreliminaryStatementPrintDate.AddDays(1);
			}

			int nextMonth = ZDateTime.Today.Month == 12 ? 1 : ZDateTime.Today.Month + 1;
			declaration.US_PeriodicStatementMM = nextMonth.ToString().PadLeft(2, '0');
			invoiceLine.JI_Tariff = "3506990000";
			invoiceLine.JI_CustomsQuantity = 10m;//tariff requires customs quantity
			MergeAndSend("ACE17");
		}

		/// <summary>
		/// Submit a delete action for the entry summary accepted in Scenario Number 016.:Deletion of an Entry Summary
		/// Summary Filing Action Request Code	D
		/// Specific Data Records to Perform Deletion of Entry Summary submitted in Scenario Number 016.	Entry Summary data used in Scenario Number 016.
		/// </summary>
		public void Test18()
		{
			SetupAirCarrierAndTariff("ACE18");
			AssertNotNull(declaration.ActiveEntryHeaders.EntrySummaryEntry);

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var builder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			message.EM_MessageNum = "HYEDUSCMT_196571";
			message.EM_SystemCreateTimeUtc = ZDateTime.Today.AddHours(-3);

			var rcvAddDone = GetReceivedMessage(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse, EDIMessage.Status.Received);
			rcvAddDone.EM_SystemCreateTimeUtc = ZDateTime.Today.AddHours(-2);
			rcvAddDone.EM_MessageNum = "HYEDUSCMT_196571";
			rcvAddDone.EM_MessageText = @"B001101SV9AX                                               HYEDUSCMT_196571     " +
"E0 SUMMRY 000001 REF ID: SV9 71032807 B00173079    171                          " +
"E1A 995   SUMMARY HAS BEEN ADDED                  SV9  7103280700100B00173079   " +
"Y  1101SV9AX00002";
			declaration.ActiveEntryHeaders[0].Messages.Add(rcvAddDone);

			MQEDIMessage deleteMessage = new ACEEntrySummaryMessageBuilderForTesting(declaration.ActiveEntryHeaders.EntrySummaryEntry, false, true, UpdateActionCode.Delete).PopulateMessage();
			deleteMessage.EM_Status = MQEDIMessage.Status.Pending;// should be manually set to QUE when it is responded
			Factory.Save();

			var ens10 = deleteMessage.MessageBlock.MessageBlocks.OfType<AENS10>().FirstOrDefault();

			AssertEquals("D", ens10.SummaryFilingActionRequestCode);
			AssertNull("10 record only required for deletion message", deleteMessage.MessageBlock.MessageBlocks.OfType<AENS40>().FirstOrDefault());
		}

		/// <summary>
		/// Submit a Warehouse Entry Type 21 entry summary to move merchandise from a FTZ into a warehouse.:Invalid Entry Type for ACE
		/// HTS Number	2208204000
		/// Entry Type Code	21
		/// FTZ Identifier	FTZ222X
		/// FDA Product Code	32CCT04
		/// </summary>
		public void Test19()
		{
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			invoiceLine.JI_Tariff = "2208204000";
			//FTZ222X where does this go?

			var fda = invoiceLine.FDAs.AddNew();
			fda.US_FDAProductCode = "32CCT04";
			fda.US_UC_NKFDAProduction = invoiceLine.US_UC_NKCountryOfOrigin;
			fda.US_PFT = "M";
			fda.US_PFR = "12345678901";
			fda.US_FDAQty1 = 1000m;
			fda.US_FDAMeasure1 = "KG";

			declaration.US_FDAContactName = "Smith John";
			declaration.US_FDAContactPhoneNo = "5555555555";
			declaration.US_FDAContactEmail = "smith.john@somewhere.com";
			declaration.US_FDAADTA = ZDateTime.Today.AddDays(-1).AddHours(10).AddMinutes(20);
			declaration.US_US_NKLocationOfGoods = "A000";
			declaration.US_EntryDate = ZDateTime.Today;
			declaration.JE_DateOfArrival = ZDateTime.Today;

			declaration.US_ForeignTradeZone = "FTZ222X";
			declaration.US_CertifyCargoRelease = true;

			MergeAndSend("ACE19", true, true);// entry type not supported yet
		}

		/// <summary>
		/// Replacement of an Entry Summary
		/// Step 1: Submit a “Formal” Entry Summary of your choosing indicating the Broker Reference Number as “020” 
		/// and set this entry to be paid on a daily statement.  This Entry Summary must be accepted by ACE.
		/// 
		/// Step 2:
		/// After ACE Acceptance: You must resubmit the same Entry Summary as a Replacement in order to change the entry data to the following elements:
		/// Summary Filing Action Request Code	R
		/// HTS Number	9014805000
		/// Value of Goods Amount	$53,000
		/// Quantity	1
		/// Country of Origin Code	GB
		/// Country of Export Code	GB
		/// Broker Reference Number	020REPLCE
		/// </summary>
		public void Test20()
		{
			invoiceLine.JI_Tariff = "9014805000";
			invoiceLine.JI_LinePrice = 53000m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfExport = "GB";
			invoiceLine.US_UC_NKCountryOfOrigin = "GB";

			mockDeclaration.Setup(m => m.BrokerReferenceNumberCore).Returns("020");
			MergeAndSend("ACE20");

			MQEDIMessage message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
			var ens10 = message.MessageBlock.MessageBlocks.OfType<AENS10>().FirstOrDefault();
			AssertEquals("Broker reference number", "020", ens10.BrokerReferenceNumber);

			mockDeclaration.Setup(m => m.BrokerReferenceNumberCore).Returns("020REPLCE");

			message = new ACEEntrySummaryMessageBuilderForTesting(declaration.ActiveEntryHeaders.EntrySummaryEntry, false, true, UpdateActionCode.Replace).PopulateMessage();
			message.EM_Status = MQEDIMessage.Status.Pending;// Should be set manually to QUE when the first message is accepted
			ens10 = message.MessageBlock.MessageBlocks.OfType<AENS10>().FirstOrDefault();
			AssertEquals("Broker reference number", "020REPLCE", ens10.BrokerReferenceNumber);
		}

		/// <summary>
		/// Submit a “Formal” entry summary for which the merchandise is subject to the Chile FTA.:Chile Free Trade Agreement
		/// HTS Number 1, Line 1	99119585
		/// HTS Number 2, Line 1	0811908080
		/// Country of Origin Code	CL
		/// </summary>
		[TestDate(2009, 1, 5)]
		public void Test21()
		{
			if (!sendTestMessagesToCustoms)
			{
				declaration.US_EstimatedEntryDate = new ZDateTime(2009, 1, 5);
			}

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;
			invoiceLine.JI_Tariff = "0811908080";
			invoiceLine.US_SupTariff = "99119585";
			invoiceLine.US_UC_NKCountryOfOrigin = "CL";
			invoiceLine.US_UC_NKCountryOfExport = "CL";
			invoiceLine.US_SPI = SpecialProgramList.Codes.CL;

			MergeAndSend("ACE21", false, true);// entry type is not supported yet
		}

		/// <summary>
		/// Submit a “Formal” entry summary for a shipment of spaghetti meals which have been determined to be a set :Sets under GRI 3(b) or 3(c) X & V – Article Set Indicator
		/// under General Rules of Interpretation 3(b) or 3(c).
		/// 
		/// Total Shipment Value	$5,000
		/// HTS Number – 1 (Spaghetti)	1902194000
		/// Value of Goods Amount – Spaghetti	$2,400
		/// Country of Origin – Spaghetti	Switzerland
		/// HTS Number – 2 (Dried Mushrooms)	0712311000
		/// Value of Goods Amount –Mushrooms	$1,300
		/// Country of Origin – Mushrooms	France
		/// HTS Number – 3  (Tomato Paste)	2002908020
		/// Value of Goods Amount –Tomato Paste	$1,300
		/// Country of Origin – Tomato Paste	Italy
		/// Country of Export 	Switzerland
		/// </summary>
		public void Test22()
		{
			invoiceLine.InvoiceHeader.US_FDAContactName = "Test Contact";
			invoiceLine.InvoiceHeader.US_FDAContactPhoneNo = "0648529756";
			invoiceLine.US_SetInd = SetIndicatorList.Codes.X;
			invoiceLine.JI_Tariff = "1902194000";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_LinePrice = 5000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CH";
			invoiceLine.US_UC_NKCountryOfExport = "CH";
			invoiceLine.US_SPI = "N/A";

			foreach (var child in invoiceLine.ChildLines.ToArray())
			{
				child.Delete();
			}

			var firstVLine = invoiceLine.AddSecondaryInvoiceLine();
			firstVLine.US_SetInd = SetIndicatorList.Codes.V;
			firstVLine.JI_Tariff = "1902194000";
			firstVLine.JI_CustomsQuantity = 10m;
			firstVLine.JI_LinePrice = 2400m;
			firstVLine.US_UC_NKCountryOfOrigin = "CH";
			firstVLine.US_UC_NKCountryOfExport = "CH";
			firstVLine.US_SPI = "N/A";

			var secondVLine = invoiceLine.AddSecondaryInvoiceLine();
			secondVLine.US_SetInd = SetIndicatorList.Codes.V;
			secondVLine.JI_Tariff = "0712311000";
			secondVLine.JI_CustomsQuantity = 10m;
			secondVLine.JI_LinePrice = 1300m;
			secondVLine.US_UC_NKCountryOfOrigin = "FR";
			secondVLine.US_UC_NKCountryOfExport = "FR";
			secondVLine.US_SPI = "N/A";

			var thirdVLine = invoiceLine.AddSecondaryInvoiceLine();
			thirdVLine.US_SetInd = SetIndicatorList.Codes.V;
			thirdVLine.JI_Tariff = "2002908020";
			thirdVLine.JI_CustomsQuantity = 10m;
			thirdVLine.JI_LinePrice = 1300m;
			thirdVLine.US_UC_NKCountryOfOrigin = "CH";
			thirdVLine.US_UC_NKCountryOfExport = "CH";
			thirdVLine.US_SPI = "N/A";

			MergeAndSend("ACE22");
		}

		/// <summary>
		/// Submit an entry summary with the following information.:Census Warning
		/// Country of Origin Code	CL
		/// Country of Export Code	CL
		/// HTS Number	4703110000
		/// Quantity	24,555,800 CTN
		/// Gross Shipping Weight	245,939
		/// Charges Amount	$17,810
		/// Value of Goods Amount	$2,054,587
		/// </summary>
		public void Test23()
		{
			invoiceLine.JI_Tariff = "4703110000";
			invoiceLine.JI_LinePrice = 2054587m;
			invoiceLine.JI_CustomsQuantity = 24555800m;
			invoiceLine.JI_Weight = 245939m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.US_UC_NKCountryOfExport = "CL";
			invoiceLine.US_UC_NKCountryOfOrigin = "CL";
			invoiceLine.US_SPI = "CL";

			invoiceLine.InvoiceHeader.Charges[0].J7_Amount = 17810m;

			MergeAndSend("ACE23");
		}

		/// <summary>
		/// Submit a Census Warning Override transaction to override the Census Warning received in the previous Scenario.:Census Warning Override
		/// </summary>
		public void Test24()
		{
			Assert("TODO for Census Override message", true);
		}

		/// <summary>
		/// Submit an entry summary for which the merchandise is subject to tax deferment and IRC Tax computation.:Deferred Tax 
		/// HTS Number	2208202000
		/// Value of Goods Amount	$297,900
		/// Quantity	125000 PFL
		/// Country of Origin Code	XC
		/// Trade Agreement/Special Program Claim Code	CA
		/// Deferred Tax Indicator	Yes
		/// IR Accounting Class Code	016
		/// </summary>
		public void Test25()
		{
			invoiceLine.InvoiceHeader.US_FDAContactName = "Test Contact";
			invoiceLine.InvoiceHeader.US_FDAContactPhoneNo = "0648529756";

			invoiceLine.JI_Tariff = "2208202000";
			invoiceLine.JI_LinePrice = 297900m;
			invoiceLine.JI_CustomsQuantity = 125000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "XC";
			invoiceLine.US_UC_NKCountryOfExport = "CA";
			invoiceLine.US_SPI = "CA";
			declaration.US_TaxDeferIndicator = TaxDeferIndicatorList.Codes.DeferredTax;
			manufacturerCode.OK_CustomsRegNo = "XCKELELE3990BUR";

			MergeAndSend("ACE25");
		}

		/// <summary>
		/// Submit an entry summary that requires the reporting of a steel license.:Steel License
		/// HTS Number	7222110005
		/// Country of Origin Code	KR
		/// </summary>
		public void Test26()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._01, "0KR123456", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			Factory.Save();

			invoiceLine.JI_Tariff = "7222110005";
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.US_UC_NKCountryOfExport = "KR";
			invoiceLine.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._01, "0KR123456");
			invoiceLine.JI_CustomsQuantity = 1m;
			MergeAndSend("ACE26");
		}

		/// <summary>
		/// Submit an entry summary for which the following merchandise (Coffee Maker) is subject to FDA reporting.:FDA Entry
		/// HTS Number	8516710020
		/// FDA Product Code	52AOJ51
		/// Value of Goods Amount	$737,953
		/// Country of Origin Code	CN
		/// Quantity	55468 NO
		/// FDA Quantities	277.34 CS, 200PCS
		/// FDA Actual Manufacturer Number	TWNICSAN435TAI
		/// </summary>
		public void Test27()
		{
			CreateNewOrGetExistingCusCodeListForFIRMSType("A001");
			invoiceLine.JI_Tariff = "8516710020";
			invoiceLine.JI_LinePrice = 737953m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.US_UC_NKCountryOfExport = "TW";
			invoiceLine.JI_CustomsQuantity = 55468m;
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USCHI";

			var fda = invoiceLine.FDAs.AddNew();
			fda.US_FDAProductCode = "52AOJ51";
			fda.US_UC_NKFDAProduction = "TW";
			fda.US_PFT = "M";
			fda.US_PFR = "12345678901";
			fda.US_FDAQty1 = 200m;
			fda.US_FDAMeasure1 = "PCS";
			fda.US_FDAQty2 = 277.34m;
			fda.US_FDAMeasure2 = "CS";

			declaration.JE_OH_FDASubmitter = declaration.IOROrgPK;
			declaration.FDASubmitter.MainAddress.OA_City = "Chicago";
			declaration.FDASubmitter.MainAddress.OA_Address1 = "87432 Main Road";
			declaration.FDASubmitter.MainAddress.OA_PostCode = "60125";
			declaration.FDASubmitter.MainAddress.OA_Phone = "5555555555";
			DeclarationTestHelper.AddPGAContact(declaration.FDASubmitter, "John", "Smith", "5555555555", null, null);
			var wrapper = OrgHeaderWrapper.New(declaration.FDASubmitter);
			wrapper.ZO_SubmitterFirmType = SubmitterFirmTypeList.Codes.I;
			Factory.Save();

			declaration.US_FDAContactName = "Smith John";
			declaration.US_FDAContactPhoneNo = "5555555555";
			declaration.US_FDAContactEmail = "smith.john@somewhere.com";
			declaration.US_FDAADTA = ZDateTime.Today.AddDays(-1).AddHours(10).AddMinutes(20);
			declaration.US_US_NKLocationOfGoods = "A001";
			declaration.US_EntryDate = ZDateTime.Today;
			declaration.JE_DateOfArrival = ZDateTime.Today;

			var coll = new Freight.Business.TransportCollection(declaration);
			coll.Load();
			coll.RemoveAndDeleteAll();

			MergeAndSend("ACE27");
		}

		/// <summary>
		/// Submit an entry summary that requires the reporting of additional duties as follows.:Additional Duty Reporting
		/// HTS Number 1, Line 1	99034110
		/// HTS Number 2, Line 1	6404191560
		/// Country of Origin Code	JP
		/// Quantity	1,000 PRS
		/// Value of Goods Amount	$10,000
		/// </summary>
		[TestDate(2009, 1, 5)]
		public void Test28_Failed()
		{
			if (!sendTestMessagesToCustoms)
			{
				declaration.US_EstimatedEntryDate = new ZDateTime(2009, 1, 5);
			}

			invoiceLine.JI_Tariff = "6404191560";//HTS unkown Check with Phyllis
			invoiceLine.US_SupTariff = "99034110";
			invoiceLine.US_UC_NKCountryOfOrigin = "JP";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_LinePrice = 10000m;

			MergeAndSend("ACE28");
		}

		/// <summary>
		/// Submit an entry summary using rail as the method of transportation.:Full Bill Data for Rail AMS Shipment
		/// Mode of Transportation Code	20
		/// Carrier	Canadian National RR (CNRU)
		/// Master Bill Number	194135
		/// District/Port of Unlading	3802
		/// </summary>
		public void Test29()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var port = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3802", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var attributeNameUnlading = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Unlading, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			helper.CreateNewOrGetExistingCusCodeListAttribute(port.PK, attributeNameUnlading.ZXE_Name, "Y");
			Factory.Save();

			declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			AssertEquals("PreCondition", "20", declaration.JE_Calc_USTransportMode);
			declaration.US_UI_NKCarrierSCAC = "CNRU";
			declaration.JE_MasterBill = "194135";
			declaration.PrimaryMasterBill.CU_NoOfPacks = 1;
			declaration.PrimaryMasterBill.CU_PackType = "PK";
			declaration.US_SchDArrival = "3802";
			declaration.US_EntryMode = EntryModeList.Codes.Paired;
			declaration.JE_PrimaryITNumber = "V2365425140";
			declaration.US_ITDate = ZDateTime.Today;
			invoiceLine.JI_Tariff = "3506990000";
			invoiceLine.JI_CustomsQuantity = 1m;
			MergeAndSend("ACE29");
		}

		/// <summary>
		/// Submit an entry summary for which Trade Agreement/Special Program Claim Code of “Z” was used.:Freely Associated States
		/// Country of Origin Code	FM
		/// HTS Number	2401106130
		/// Trade Agreement/Special Program Claim Code	Z
		/// Value of Goods Amount	$10,000
		/// </summary>
		public void Test30()
		{
			invoiceLine.JI_Tariff = "2401106130";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "FM";
			invoiceLine.US_UC_NKCountryOfExport = "FM";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.Z;
			invoiceLine.JI_LinePrice = 10000m;

			MergeAndSend("ACE30");
		}

		/// <summary>
		/// Submit an entry summary for which Trade Agreement/Special Program Claim Code of “L” was used.:Uruguay Round Concessions -Dyes
		/// Country of Origin Code	GB
		/// HTS Number	2914704000
		/// Trade Agreement/Special Program Claim Code	L
		/// Value of Goods Amount	$10,000
		/// Quantity	5,000 KG
		/// </summary>
		public void Test31()
		{
			invoiceLine.JI_Tariff = "2914704000";
			invoiceLine.US_UC_NKCountryOfOrigin = "GB";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.L;
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 5000m;

			MergeAndSend("ACE31");
		}

		/// <summary>
		/// Submit an entry summary for which 3 line items are transmitted with 3 different states of destination.:State of Destination with Multiple Lines
		/// HTS Number, Line 1	1205100090
		/// Value of Goods Amount, Line 1	$10,000
		/// State of Ultimate Destination	Montana
		/// HTS Number, Line 2	3306900000
		/// Value of Goods Amount, Line 2	$5,000
		/// State of Ultimate Destination	New York
		/// HTS Number, Line 3	3702100060
		/// Value of Goods Amount, Line 3	$15,000
		/// State of Ultimate Destination	Washington
		/// </summary>
		public void Test32()
		{
			invoiceLine.InvoiceHeader.US_FDAContactName = "Test Contact";
			invoiceLine.InvoiceHeader.US_FDAContactPhoneNo = "0648529756";
			invoiceLine.JI_Tariff = "1205100090";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.US_DestinationState = USStatesList.Codes.Montana;

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "3306900000";
			invoiceLine2.JI_CustomsQuantity = 1m;
			invoiceLine2.JI_LinePrice = 5000m;
			invoiceLine2.US_DestinationState = USStatesList.Codes.NewYork;
			invoiceLine2.US_SPI = SPICompleteList.MoreCodes.NotApplicable;

			var invoiceLine3 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "3702100060";
			invoiceLine3.JI_CustomsQuantity = 1m;
			invoiceLine3.JI_LinePrice = 15000m;
			invoiceLine3.US_DestinationState = USStatesList.Codes.Washington;
			invoiceLine3.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			MergeAndSend("ACE32");

			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
			var ens11 = message.MessageBlock.MessageBlocks.OfType<AENS11>().FirstOrDefault();
			AssertEquals("State from the highest value", USStatesList.Codes.Washington, ens11.USStateOfDestinationCode);
		}

		/// <summary>
		/// Submit an entry summary for which charges are required.: Charges Amount
		/// Country of Origin Code	GB
		/// HTS Number	3001900110
		/// Value of Goods Amount	$10,000
		/// Charges Amount	$200
		/// </summary>
		public void Test33()
		{
			invoiceLine.JI_Tariff = "3001900110";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "GB";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.InvoiceHeader.Charges[0].J7_Amount = 200m;

			MergeAndSend("ACE33");
		}

		/// <summary>
		/// Submit an entry summary for which the entry date must be between January 1 and May 31.: HTS Number/Date Restriction
		/// HTS Number	0809402000
		/// Date of Importation	Current date
		/// Estimated Entry Date	Current date
		/// </summary>
		public void Test34()
		{
			invoiceLine.InvoiceHeader.US_FDAContactName = "Test Contact";
			invoiceLine.InvoiceHeader.US_FDAContactPhoneNo = "0648529756";
			invoiceLine.JI_Tariff = "0809402000";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_CustomsUnitQty = "KG";

			var dateToSet = ZDateTime.Today;

			declaration.US_EntryDate = dateToSet;
			declaration.US_EstimatedEntryDate = dateToSet;

			while (declaration.US_EstimatedEntryDateInfo.HasMessageErrors())
			{
				dateToSet = dateToSet.AddDays(1);
				declaration.US_EntryDate = dateToSet;
				declaration.US_EstimatedEntryDate = dateToSet;
			}

			bool expectedMessageErrors = dateToSet.Month > 5;

			var coll = new Freight.Business.TransportCollection(declaration);
			coll.Load();
			coll.RemoveAndDeleteAll();

			MergeAndSend("ACE34", false, expectedMessageErrors);
		}

		/// <summary>
		/// Submit an entry summary for which NAFTA Preferential treatment is being claimed (Regional value content calculated using the net cost method).:NAFTA
		/// Country of Origin Code	CA
		/// HTS Number	1004000010
		/// NAFTA Net Cost Indicator	Y
		/// Entry Type 	01
		/// Value of Goods Amount	$10,000
		/// Quantity 	85,000 KG
		/// </summary>
		public void Test35()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			invoiceLine.JI_Tariff = "1004000010";
			invoiceLine.US_IsNAFTANet = true;
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 85000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CA";

			MergeAndSend("ACE35");
		}

		/// <summary>
		/// Submit an entry summary with the following Country of Origin and Country of Export Codes.:Restricted Country
		/// HTS Number	6402191541
		/// Country of Origin Code	CU
		/// Country of Export Code	MX
		/// 
		/// Phyllis said it is supposed to fail
		/// </summary>
		public void Test36_Failed()
		{
			invoiceLine.JI_Tariff = "6402191541";
			invoiceLine.US_UC_NKCountryOfOrigin = "CU";// Check with Phyllis: Restricted country
			invoiceLine.US_UC_NKCountryOfExport = "MX";

			MergeAndSend("ACE36", false, true);
		}

		/// <summary>
		/// Submit an entry summary for a shipment of knife sets with the following data.:Knife Sets
		/// There are 4000 sets.  Each set consists of 5 knives. 4 knives in the set are classified in 8211929045 and are valued at $1.00 each.  
		/// The other knife in the set is classified in 8211930030 and is valued at $2.00 each. 
		/// HTS Number  (Set Provision for Knives)	8211100000
		/// </summary>
		public void Test37()
		{
			invoiceLine.JI_Tariff = "8211100000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_InvoiceQuantity = 4000m;
			invoiceLine.JI_CustomsQuantity = 4000m;
			invoiceLine.JI_LinePrice = 0m;

			JobComInvoiceLine line2 = invoiceLine.AddSecondaryInvoiceLine();
			line2.JI_Tariff = "8211929045"; // .4c per unit + 6.1% = 20000 *.004 + 24000 * .061 = $1544
			line2.JI_Description = "COMMERCIAL DESCRIPTION";
			line2.JI_CustomsQuantity = 16000m;
			line2.JI_LinePrice = 16000m;

			JobComInvoiceLine line3 = invoiceLine.AddSecondaryInvoiceLine();
			line3.JI_Tariff = "8211930030"; // .3c per unit + 5.4% = (20000 *.03) + (24000 * .054) = $1896
			line3.JI_Description = "COMMERCIAL DESCRIPTION";
			line3.JI_CustomsQuantity = 4000m;
			line3.JI_LinePrice = 8000m;

			MergeAndSend("ACE37");
		}

		/// <summary>
		/// Submit an entry summary using the following dates and other information. : Due Date Hierarchy
		/// In-Transit Date (XX= Last Year)	December 31, 20XX
		/// Date of Exportation (XX= Last Year)	December 31, 20XX
		/// Date of Importation (XX= Last Year)	December 31, 20XX
		/// Mode of Transportation Code	40
		/// In-Transit Number	430042900
		/// HTS Number	1901104500
		/// Quantity 	10,000 KG
		/// Country of Origin Code	SG
		/// Trade Agreement/Special Program Claim Code	SG
		/// Value of Goods Amount	$10,000
		/// </summary>
		[TestDate(2010, 12, 1)]//1901104500 not valid after 2009 in a test DB
		public void Test38()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var port = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2402", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTransportModeForCusCodeList(port.PK, TransportTypeList.Codes.Air);
			var attributeNameUnlading = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Unlading, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			helper.CreateNewOrGetExistingCusCodeListAttribute(port.PK, attributeNameUnlading.ZXE_Name, "Y");
			Factory.Save();

			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.JE_MasterBill = "00178778781";
			declaration.JE_PrimaryITNumber = "430042900";

			int year = 2009;
			if (sendTestMessagesToCustoms)
			{
				year = ZDateTime.Today.Year - 1;
			}

			declaration.US_ITDate = new ZDateTime(year, 12, 31);
			declaration.JE_ExportDate = declaration.US_ITDate;
			declaration.US_EntryDate = declaration.US_ITDate;
			declaration.US_UI_NKCarrierSCAC = "AA";
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "AA345";
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.JE_MasterBillIssuerSCAC = "AA";
			declaration.US_SchDArrival = "2402";
			declaration.US_UI_NKCarrierSCAC = "AA";
			declaration.US_FDAContactName = "Julian";
			declaration.US_FDAContactPhoneNo = "6143432432";

			AssertEquals("PreCondition", TransportModeCodes.Codes.AirNonContainer, declaration.JE_Calc_USTransportMode);

			invoiceLine.JI_Tariff = "1901104500";
			invoiceLine.JI_CustomsQuantity = 10000m;
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "SG";
			invoiceLine.US_UC_NKCountryOfExport = "SG";
			invoiceLine.US_SPI = "SG";

			var coll = new Freight.Business.TransportCollection(declaration);
			coll.Load();
			coll.RemoveAndDeleteAll();

			MergeAndSend("ACE38");
		}

		/// <summary>
		/// Submit an entry summary for a personal shipment as follows. : Personal Shipment
		/// HTS Number 	7419993000
		/// Country of Origin Code	HK
		/// Entry Type Code	11
		/// Personal Shipment	Yes
		/// Value of Goods Amount	$3,000
		/// </summary>
		public void Test39()
		{
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.US_ConsolidatedInformalIndicator = ConsolidatedInformalList.Codes.Personal;
			invoiceLine.JI_LinePrice = 3000m;
			invoiceLine.JI_Tariff = "7419993000";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "HK";
			invoiceLine.US_UC_NKCountryOfExport = "HK";

			MergeAndSend("ACE39");
		}

		/// <summary>
		/// Submit an entry summary for a shipment of commercial samples.:Commercial Samples
		/// HTS Number	6205202016
		/// Country of Origin Code	JP
		/// Value of Goods Amount	$225
		/// Entry Type Code 	11
		/// Commercial Sample	Yes
		/// </summary>
		[TestDate(2010, 10, 21)]
		public void Test41()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._19, "8JP123456", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			Factory.Save();

			if (!sendTestMessagesToCustoms)
			{
				declaration.US_EstimatedEntryDate = new ZDateTime(2009, 1, 5);// Duty rate date will be this and tariff will be found
			}

			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.US_ConsolidatedInformalIndicator = ConsolidatedInformalList.Codes.Samples;

			invoiceLine.JI_Tariff = "6205202016";
			invoiceLine.JI_CustomsSecondQuantity = 1m;

			invoiceLine.JI_LinePrice = 225m;
			invoiceLine.US_UC_NKCountryOfOrigin = "JP";
			invoiceLine.US_UC_NKCountryOfExport = "JP";
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.No;

			var permit = invoiceLine.LicenceAndPermits.AddNew();
			permit.CY_Code = "19";
			permit.CY_Data = "8JP123456";

			MergeAndSend("ACE41");
		}

		/// <summary>
		/// Submit an entry summary for a shipment of watches:
		/// There are 59,600 complete watches.  Report this quantity for each tariff number.
		/// HTS Number 1	9101118010
		/// Value of Goods Amount, HTS Number 1	$1,490,200
		/// HTS Number 2	9101118020
		/// Value of Goods Amount, HTS Number 2	$601,690
		/// HTS Number 3	9101118030
		/// Value of Goods Amount, HTS Number 3	$790,840
		/// HTS Number 4	9101118040
		/// Value of Goods Amount, HTS Number 4	$500,612
		/// </summary>
		public void Test42()
		{
			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				invoiceLine.JI_Tariff = "9101118010";
				invoiceLine.JI_CustomsQuantity = 59600m;
				invoiceLine.JI_LinePrice = 1490200m;

				var secondLine1 = invoiceLine.AddSecondaryInvoiceLine();
				secondLine1.JI_Tariff = "9101118020";
				secondLine1.JI_CustomsQuantity = 59600m;
				secondLine1.JI_LinePrice = 601690m;

				var secondLine2 = invoiceLine.AddSecondaryInvoiceLine();
				secondLine2.JI_Tariff = "9101118030";
				secondLine2.JI_CustomsQuantity = 59600m;
				secondLine2.JI_LinePrice = 790840m;

				var secondLine3 = invoiceLine.AddSecondaryInvoiceLine();
				secondLine3.JI_Tariff = "9101118040";
				secondLine3.JI_CustomsQuantity = 59600m;
				secondLine3.JI_LinePrice = 500612m;
			}

			MergeAndSend("ACE42");
		}

		/// <summary>
		/// Submit an entry summary for a shipment of goods from Morocco.:Morocco Free Trade Agreement
		/// HTS Number	9404902000
		/// Country of Origin	Morocco
		/// Country of Export	Morocco
		/// Eligible for the Morocco Free Trade Agreement	Yes
		/// </summary>
		public void Test43()
		{
			invoiceLine.JI_Tariff = "9404902000";
			invoiceLine.US_UC_NKCountryOfOrigin = "MA";
			invoiceLine.US_UC_NKCountryOfExport = "MA";
			invoiceLine.US_SPI = "MA";
			MergeAndSend("ACE43");
		}

		/// <summary>
		/// Submit an entry summary for a tool set.:Quantity and Unit of Measure Reporting on GRI (1) Sets
		/// HTS Number 1	8206000000
		/// HTS Number 2	8203204000
		/// </summary>
		[TestDate(2010, 10, 21)]
		public void Test44()
		{
			if (!sendTestMessagesToCustoms)
			{
				declaration.US_EstimatedEntryDate = new ZDateTime(2009, 1, 5);// Duty rate date will be this and tariff will be found
			}

			invoiceLine.JI_Tariff = "8206000000";
			invoiceLine.JI_CustomsQuantity = 1000m;
			invoiceLine.JI_LinePrice = 0m;

			var secondary = invoiceLine.AddSecondaryInvoiceLine();
			secondary.JI_Tariff = "8203204000";
			secondary.JI_CustomsQuantity = 1000m;
			secondary.JI_LinePrice = 10000m;

			MergeAndSend("ACE44");
		}

		/// <summary>
		/// Submit an entry summary for a shipment of goods from Guam.:U.S. Insular Possession
		/// HTS Number	7101223000
		/// Country of Origin Code	GU
		/// Trade Agreement/Special Program Claim Code	Y
		/// </summary>
		public void Test45()
		{
			invoiceLine.JI_Tariff = "7101223000";
			invoiceLine.US_UC_NKCountryOfOrigin = "GU";
			invoiceLine.US_UC_NKCountryOfExport = "GU";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.Y;

			MergeAndSend("ACE45");
		}

		/// <summary>
		/// Submit an entry summary with the MOT and HTS Number as follows.:Mail MOT
		/// HTS Number	9106100000
		/// Value of Goods Amount	$1,189
		/// Quantity	120 NO
		/// Mode of Transportation Code	50
		/// </summary>
		public void Test46()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			declaration.JE_VoyageFlightNo = "1M";
			declaration.JE_DateOfArrival = ZDateTime.Today;
			AssertEquals("PreCondition", TransportModeCodes.Codes.Mail, declaration.JE_Calc_USTransportMode);
			declaration.US_UI_NKCarrierSCAC = ZString.Empty;
			declaration.JE_MasterBillIssuerSCAC = ZString.Empty;

			invoiceLine.JI_Tariff = "9106100000";
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			invoiceLine.JI_LinePrice = 1189m;
			invoiceLine.JI_CustomsQuantity = 120m;

			MergeAndSend("ACE46");

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var message = (MQEDIMessage)entry.Messages[0];
			var ens34 = message.MessageBlock.MessageBlocks.OfType<AENS34>().FirstOrDefault();
			AssertNotNull(ens34);

			var ens34s = message.MessageBlock.MessageBlocks.OfType<AENS34>();
			AssertEquals(1, ens34s.Count());

			declaration.US_PSC = true;
			var pscReasons = new PSCReasonCodeCollection(entry);
			var pscReason = pscReasons.AddNew();
			pscReason.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.Entry;
			pscReason.Reason1 = PSCHeaderReasonList.Codes.H01;
			pscReason.Reason2 = PSCHeaderReasonList.Codes.H02;
			pscReason.Reason3 = PSCHeaderReasonList.Codes.H99;

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var action = new EntryHeaderMessageSendingAction(entry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			action.US_CertifyCargoRelease = true;
			action.US_AcknowledgeAndSign = true;
			var messageBuilder = new ACEEntrySummaryMessageBuilder(entry, ACEEntrySummaryMessageSendingOption.New(action), "This is a test PSC explanation text", pscReasons, UpdateActionCode.Add);
			var message2 = messageBuilder.PopulateMessage();
			var ens34Index = message2.MessageBlock.MessageBlocks.FindIndex(x => x is AENS34);
			var ens35Index = message2.MessageBlock.MessageBlocks.FindIndex(x => x is AENS35);
			var ens36Index = message2.MessageBlock.MessageBlocks.FindIndex(x => x is AENS36);

			Assert("36s constructed after 35s", ens36Index > ens35Index);
			Assert("35s constructed after 34s", ens35Index > ens34Index);
		}

		/// <summary>
		/// Submit an entry summary with country codes as follows.:Country of Export - US
		/// HTS Number	6704190000
		/// Country of Origin Code	KR
		/// Country of Export Code	US
		/// 
		/// Phyllis said it is supposed to fail
		/// </summary>
		public void Test47_Failed()
		{
			invoiceLine.JI_Tariff = "6704190000";
			invoiceLine.US_UC_NKCountryOfExport = "US";
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";

			MergeAndSend("ACE47", false, true);// Country of export US on an import entry
		}

		/// <summary>
		/// Submit an entry summary for a NAFTA shipment as follows.:Canadian NAFTA
		/// HTS Number, Line 1	8538908080
		/// Value of Goods Amount, Line 1	$2,000
		/// Country of Origin Code, Line 1	XA
		/// Country of Export Code, Line 1	CA
		/// Trade Agreement/Special Program Claim Code, Line 1	CA
		/// HTS Number, Line 2	8538908080
		/// Value of Goods Amount, Line 2	$1,500
		/// Country of Origin Code, Line 2	TW
		/// Country of Export Code, Line 2	CA
		/// </summary>
		public void Test48()
		{
			manufacturerCode.OK_CustomsRegNo = "XATACENGBRO";// Checking with Phyllis...

			invoiceLine.JI_Tariff = "8538908080";
			invoiceLine.JI_LinePrice = 2000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "XA";
			invoiceLine.US_UC_NKCountryOfExport = "CA";
			invoiceLine.US_SPI = "CA";

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "8538908080";
			invoiceLine2.JI_LinePrice = 1500m;
			invoiceLine2.US_UC_NKCountryOfExport = "CA";
			invoiceLine2.US_UC_NKCountryOfOrigin = "TW";
			invoiceLine2.US_SPI = "N/A";

			MergeAndSend("ACE48");
		}

		/// <summary>
		/// Submit an entry summary as follows.:Special Program Claim Code “W”
		/// HTS Number 	0210992000
		/// Quantity	10,000 KG
		/// Country of Origin Code	VC
		/// Trade Agreement/Special Program Claim Code	W
		/// </summary>
		public void Test49()
		{
			invoiceLine.JI_Tariff = "0210992000";
			invoiceLine.JI_CustomsQuantity = 10000m;
			invoiceLine.US_UC_NKCountryOfExport = "VC";
			invoiceLine.US_UC_NKCountryOfOrigin = "VC";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.W;

			MergeAndSend("ACE49");
		}

		/// <summary>
		/// Submit an entry summary as follows.:Currency Conversion
		/// HTS Number 	3103100010
		/// Country of Origin Code	IT
		/// Value of Goods Amount (Foreign Currency)	5,000 Euros
		/// Quantity	100 T
		/// </summary>
		[TestDate(2017, 5, 17)]
		public void Test50()
		{
			invoiceLine.JI_Tariff = "3103100010";
			invoiceLine.JI_LinePrice = 5000m;
			invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = "EUR";

			invoiceLine.US_UC_NKCountryOfOrigin = "IT";
			invoiceLine.US_UC_NKCountryOfExport = "IT";
			invoiceLine.JI_CustomsQuantity = 100m;
			invoiceLine.JI_WeightUQ = invoiceLine.JI_CustomsUnitQty;
			invoiceLine.JI_Weight = invoiceLine.JI_CustomsQuantity;

			MergeAndSend("ACE50");
		}

		/// <summary>
		/// Submit an entry summary with multiple countries as follows.
		/// HTS Number, Line 1	3103100020
		/// Country of Origin Code, Line 1	TW
		/// Country of Export Code, Line 1	TW
		/// Quantity, Line 1	10 T
		/// HTS Number, Line 2	3103100020
		/// Country of Origin Code, Line 2	KR
		/// Country of Export Code, Line 2	KR
		/// Quantity, Line 2	10 T
		/// </summary>
		[TestDate(2017, 5, 17)]
		public void Test51()
		{
			invoiceLine.JI_Tariff = "3103100020";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_WeightUQ = invoiceLine.JI_CustomsUnitQty;
			invoiceLine.JI_Weight = invoiceLine.JI_CustomsQuantity;
			invoiceLine.US_UC_NKCountryOfExport = "TW";
			invoiceLine.US_UC_NKCountryOfOrigin = "TW";

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "3103100020";
			invoiceLine2.JI_CustomsQuantity = 10m;
			invoiceLine2.JI_WeightUQ = invoiceLine2.JI_CustomsUnitQty;
			invoiceLine2.JI_Weight = invoiceLine2.JI_CustomsQuantity;
			invoiceLine2.US_UC_NKCountryOfExport = "KR";
			invoiceLine2.US_UC_NKCountryOfOrigin = "KR";

			MergeAndSend("ACE51");
		}

		/// <summary>
		/// Submit an entry summary with the following data.:African Growth and Opportunity Act (AGOA) Entry 1
		/// HTS Number 	98191124
		/// Country of Origin Code	ZA
		/// Additional HTS Number	6204624021
		/// Value of Goods Amount	$4,564
		/// </summary>
		[TestDate(2010, 10, 21)]
		public void Test52_Failed()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._19, "8ZA123456", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			Factory.Save();

			if (!sendTestMessagesToCustoms)
			{
				declaration.US_EstimatedEntryDate = new ZDateTime(2009, 1, 5);// Duty rate date will be this and tariff will be found
			}

			invoiceLine.JI_Tariff = "6204624021";
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			invoiceLine.US_SupTariff = "98191124";// HTS Unknown Check with Phyllis

			var licence = invoiceLine.LicenceAndPermits.AddNew();
			licence.CY_Data = "8ZA123456";
			licence.CY_Code = "19";
			invoiceLine.JI_LinePrice = 4564m;
			invoiceLine.US_UC_NKCountryOfOrigin = "ZA";
			invoiceLine.US_UC_NKCountryOfExport = "ZA";
			invoiceLine.US_CottonFeeExempt = YesNoDefaultList.Codes.No;

			var coll = new Freight.Business.TransportCollection(declaration);
			coll.Load();
			coll.RemoveAndDeleteAll();

			MergeAndSend("ACE52");
		}

		/// <summary>
		/// Submit an entry summary for watches previously exported to Greenland to be repaired.  :Watch Assemblies
		/// Repairs were only performed on the watch movements; the value of the repairs to the movements was $3,406.  
		/// HTS Number 1	9802004040
		/// HTS Number - (Movements)	9102111010
		/// Value of Goods Amount (Movements)	$1,852
		/// HTS Number - (Cases)	9102111020
		/// Value of Goods Amount (Cases)	$2,619
		/// HTS Number - (Bracelets)	9102111030
		/// Value of Goods Amount (Bracelets)	$1,345
		/// HTS Number - (Batteries)	9102111040
		/// Value of Goods Amount (Batteries)	$204
		/// Quantity (All HTS numbers)	1,000
		/// </summary>
		public void Test53()
		{
			using (declaration.SuspendDefaultingSecondaryTariffLines())
			{
				invoiceLine.JI_Tariff = "9102111010";
				invoiceLine.US_SupTariff = "9802004040";
				invoiceLine.JI_LinePrice = 3406m;
				invoiceLine.US_98GoodsValue = 1852m;
				invoiceLine.JI_CustomsQuantity = 1000m;
				invoiceLine.US_SPI = "AU";

				var secondary1 = invoiceLine.AddSecondaryInvoiceLine();
				secondary1.JI_Tariff = "9102111020";
				secondary1.US_98GoodsValue = 2619m;
				secondary1.JI_CustomsQuantity = 1000m;
				secondary1.JI_LinePrice = 1m;
				secondary1.US_SPI = "AU";

				var secondary2 = invoiceLine.AddSecondaryInvoiceLine();
				secondary2.JI_Tariff = "9102111030";
				secondary2.US_98GoodsValue = 1345m;
				secondary2.JI_CustomsQuantity = 1000m;
				secondary2.JI_LinePrice = 1m;
				secondary2.US_SPI = "AU";

				var secondary3 = invoiceLine.AddSecondaryInvoiceLine();
				secondary3.JI_Tariff = "9102111040";
				secondary3.US_98GoodsValue = 204m;
				secondary3.JI_CustomsQuantity = 1000m;
				secondary3.JI_LinePrice = 1m;
				secondary3.US_SPI = "AU";
			}

			MergeAndSend("ACE53");
		}

		/// <summary>
		/// Submit an entry summary for which the merchandise is subject to ATPA as follows.:Andean Trade Preference Act (ATPA)
		/// HTS Number 	2401106130
		/// Country of Origin Code	CO
		/// Country of Export Code	CO
		/// Trade Agreement/Special Program Claim Code	J
		/// </summary>
		public void Test55()
		{
			invoiceLine.JI_Tariff = "2401106130";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CO";
			invoiceLine.US_UC_NKCountryOfExport = "CO";
			invoiceLine.US_SPI = PrimarySpecProgramIndicatorList.Codes.J;

			MergeAndSend("ACE55");
		}

		/// <summary>
		/// Submit an entry summary with the following information:AGOA Entry 2
		/// HTS Number 	4113903000
		/// Country of Origin Code	ZA
		/// Trade Agreement/Special Program Claim Code	D
		/// </summary>
		public void Test56()
		{
			invoiceLine.JI_Tariff = "4113903000";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfExport = "ZA";
			invoiceLine.US_UC_NKCountryOfOrigin = "ZA";
			invoiceLine.US_SPI = "N/A";

			MergeAndSend("ACE56");

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var message = (MQEDIMessage)entry.Messages[0];

			var ens44 = message.MessageBlock.MessageBlocks.OfType<AENS44>().FirstOrDefault();
			AssertNotNull(ens44);
		}

		/// <summary>
		/// B-Record Filer Authentication
		/// Create and transmit a separate block of data (A-Z, B-Y records) for application identifier AE which includes a valid set of entry summary 10-90 Records in addition to the following: 
		/// Processing District/Port Code (B-Record)	8888
		/// Filer Code (B-Record)	888
		/// </summary>
		public void Test58()
		{
			mockDeclaration.Protected().Setup<ZString>("ProcessingDistrictPortCore").Returns("8888");

			declaration.US_EntryFilerCode = "888";
			invoiceLine.JI_Tariff = "3506990000";
			invoiceLine.JI_CustomsQuantity = 1m;

			MergeAndSend("ACE58");
		}

		// Test59 is a redundant one. If other cases work, this should work
		// B-Record Application Type/Filer Authentication
		// Please contact your assigned client representative immediately prior to transmitting this data.  Create and transmit a separate block of data (A-Z, B-Y Records) for application identifier AE which includes a valid set of entry summary 10-90 Records in addition to the following: 
		// Filer Code (B-Record)	Your Filer Code
		// Application Identifier Code (B-Record)	AE

		/// <summary>
		/// Submit an entry summary with the following information:
		/// HTS Number 	4113903000
		/// Country of Origin	Japan
		/// Value of Goods Amount	$12,225
		/// First Sale Indicator	F
		/// </summary>

		public void Test60()
		{
			declaration.JE_ExportDate = new ZDateTime(ZDateTime.Today.Year, 1, 25);
			declaration.JE_DateOfArrival = new ZDateTime(ZDateTime.Today.Year, 2, 10);
			invoiceLine.JI_Tariff = "4113903000";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "JP";
			invoiceLine.JI_LinePrice = 12225m;
			invoiceLine.US_FirstSale = YesNoDefaultList.Codes.Yes;
			MergeAndSend("ACE60");
			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
			var ens40 = message.MessageBlock.MessageBlocks.OfType<AENS40>().FirstOrDefault();
			AssertEquals("First Sale removed from 40", " ", ens40.Serialise().Substring(58, 1));
		}

		public void TestAENS40MessageContent_WhenNonReimbursableStatementEntered()
		{
			declaration.JE_ExportDate = new ZDateTime(ZDateTime.Today.Year, 1, 25);
			declaration.JE_DateOfArrival = new ZDateTime(ZDateTime.Today.Year, 2, 10);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			invoiceLine.InvoiceHeader.US_FDAContactName = "Test Contact";
			invoiceLine.InvoiceHeader.US_FDAContactPhoneNo = "0648529756";
			invoiceLine.JI_Tariff = "1901909095";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "IT";
			invoiceLine.JI_OA_ExporterAddress = manufacturer.MainAddress.PK;
			invoiceLine.JI_LinePrice = 12225m;
			invoiceLine.US_FirstSale = YesNoDefaultList.Codes.Yes;
			invoiceLine.US_ADDCaseNo = "A475818001";
			invoiceLine.US_ADDDepositRateIndicator = "A";
			invoiceLine.US_CVDCaseNo = "C475819017";
			invoiceLine.US_CVDDepositRateIndicator = "A";
			invoiceLine.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.Declared;
			invoiceLine.US_ADDDecID = "36";

			if (invoiceLine.AntidumpingDutyCase == null)
			{
				SetUpADDCVDCaseIfTestingNotSendingToCustoms("A475818001", ZDateTime.BrettsBirthday, "IT", "1901909095", 0.5m, 0m);
			}

			if (invoiceLine.CountervailingDutyCase == null)
			{
				SetUpADDCVDCaseIfTestingNotSendingToCustoms("C475819017", ZDateTime.BrettsBirthday, "IT", "1901909095", 0.5m, 0m);
			}
			MergeAndSend("ACE40");
			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
			var ens40 = message.MessageBlock.MessageBlocks.OfType<AENS40>().FirstOrDefault();

			AssertEquals("AENS40 A D C V D NON REIMBURSEMENT STATEMENT (60-60) don't exist", "Y", ens40.ADCVDNonReimbursementStatement);
		}

		public void Test60a()
		{
			declaration.JE_ExportDate = new ZDateTime(ZDateTime.Today.Year, 1, 25);
			declaration.JE_DateOfArrival = new ZDateTime(ZDateTime.Today.Year, 2, 10);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			invoiceLine.InvoiceHeader.US_FDAContactName = "Test Contact";
			invoiceLine.InvoiceHeader.US_FDAContactPhoneNo = "0648529756";
			invoiceLine.JI_Tariff = "1901909095";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "IT";
			invoiceLine.JI_OA_ExporterAddress = manufacturer.MainAddress.PK;
			invoiceLine.JI_LinePrice = 12225m;
			invoiceLine.US_FirstSale = YesNoDefaultList.Codes.Yes;
			invoiceLine.US_ADDCaseNo = "A475818001";
			invoiceLine.US_ADDDepositRateIndicator = "A";
			invoiceLine.US_CVDCaseNo = "C475819017";
			invoiceLine.US_CVDDepositRateIndicator = "A";
			invoiceLine.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.OnceOff;

			if (invoiceLine.AntidumpingDutyCase == null)
			{
				SetUpADDCVDCaseIfTestingNotSendingToCustoms("A475818001", ZDateTime.BrettsBirthday, "IT", "1901909095", 0.5m, 0m);
			}

			if (invoiceLine.CountervailingDutyCase == null)
			{
				SetUpADDCVDCaseIfTestingNotSendingToCustoms("C475819017", ZDateTime.BrettsBirthday, "IT", "1901909095", 0.5m, 0m);
			}
			MergeAndSend("ACE60a");

			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
			var ens40 = message.MessageBlock.MessageBlocks.OfType<AENS40>().FirstOrDefault();
			AssertEquals("First Sale removed from 40", " ", ens40.Serialise().Substring(58, 1));
			AssertEquals("40 Not Once Off", "Y", ens40.Serialise().Substring(59, 1));
		}

		public void Test61()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._18, "8CB200018", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			Factory.Save();

			declaration.JE_ExportDate = new ZDateTime(ZDateTime.Today.Year, 1, 25);
			declaration.JE_DateOfArrival = new ZDateTime(ZDateTime.Today.Year, 2, 10);
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			invoiceLine.InvoiceHeader.US_FDAContactName = "Test Contact";
			invoiceLine.InvoiceHeader.US_FDAContactPhoneNo = "0648529756";
			invoiceLine.JI_Tariff = "1901909095";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "IT";
			invoiceLine.JI_OA_ExporterAddress = manufacturer.MainAddress.PK;
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._18, "8CB200018");
			invoiceLine.US_ADDCaseNo = "A475818001";
			invoiceLine.US_ADDDepositRateIndicator = "A";
			invoiceLine.US_ADDDecID = "112HY";
			invoiceLine.US_CVDCaseNo = "C475819017";
			invoiceLine.US_CVDDepositRateIndicator = "A";
			invoiceLine.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.Declared;

			if (invoiceLine.AntidumpingDutyCase == null)
			{
				SetUpADDCVDCaseIfTestingNotSendingToCustoms("A475818001", ZDateTime.BrettsBirthday, "IT", "1901909095", 0.5m, 0m);
			}

			if (invoiceLine.CountervailingDutyCase == null)
			{
				SetUpADDCVDCaseIfTestingNotSendingToCustoms("C475819017", ZDateTime.BrettsBirthday, "IT", "1901909095", 0.5m, 0m);
			}
			MergeAndSend("ACE61");

			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
			var ens40 = message.MessageBlock.MessageBlocks.OfType<AENS40>().FirstOrDefault();
			AssertEquals("40 Not Declared", "Y", ens40.Serialise().Substring(59, 1));
		}

		/// <summary>
		/// CBTPA Certificate
		/// Submit an entry summary with the following data for a Caribbean Basin Trade Partnership Act (CBTPA) qualifying shipment:
		/// HTS Number	6212103000
		/// Country of Origin	Barbados
		/// Value of Goods Amount	$10,000
		/// CBTPA Certificate Number	8CB200018
		/// </summary>

		public void Test62()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, "US Permit License Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPermitLicenseType, LicencePermitTypeList.Codes._18, "8CB200018", ZDateTime.Today, ZDateTime.Today.AddDays(5));
			Factory.Save();

			declaration.JE_ExportDate = new ZDateTime(ZDateTime.Today.Year, 1, 25);
			declaration.JE_DateOfArrival = new ZDateTime(ZDateTime.Today.Year, 2, 10);
			invoiceLine.JI_Tariff = "6212103000";
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "BB";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes._18, "8CB200018");
			MergeAndSend("ACE62");
		}

		/// <summary>
		/// DOT Form Data
		/// Submit an entry summary with the following Department of Transportation (DOT) form HS-7 data concerning the import of a vehicle:
		/// HTS Number	8703210000
		/// Country of Origin	DE
		/// Commercial Description	Snowmobile
		/// HS-7 Box 8	Yes
		/// HS-7 Clarification Code	V
		/// HS-7 Make of Vehicle	 ABC
		/// HS-7 Model	UTV-001
		/// HS-7 Year	2008
		/// HS-7 VIN	 TEST-VIN-NUMBER
		/// </summary>
		[TestDate(2016, 03, 27)]
		public void Test64()
		{
			invoiceLine.JI_Tariff = "8703210000";
			invoiceLine.US_SupTariff = "98178502";
			invoiceLine.US_UC_NKCountryOfOrigin = "DE";
			invoiceLine.JI_Description = "Snowmobile";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;

			var dot = invoiceLine.DOTs.AddNew();
			dot.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._08;
			dot.US_DOTClarCode = ClarificationCodeList.Codes.Vehicle;
			dot.US_DOTImpSubstStatement = true;// Check with Phyllis

			var vin = dot.DOTVINs.AddNew();
			vin.US_DOTMake = "ABC";
			vin.US_DOTModel = "UTV-001";
			vin.US_DOTYear = 2008;
			vin.US_DOTVIN = "TEST-VIN-NUMBER";

			MergeAndSend("ACE64");
		}

		/// <summary>
		/// Informal Entry - Commercial Sales Sample
		/// Submit an entry summary with the following information for a shipment of a commercial sample:
		/// Country of Origin	CA
		/// HTS Number	98110060
		/// Value of Goods Amount	$1
		/// Entry Type 	11
		/// </summary>
		public void Test65_Failed()
		{
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.US_ConsolidatedInformalIndicator = ConsolidatedInformalList.Codes.Samples;

			invoiceLine.JI_Tariff = "98110060";// HTS not known Check with Phyllis
			invoiceLine.JI_LinePrice = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CA";

			MergeAndSend("ACE65");
		}

		/// <summary>
		/// Submit an entry summary as follows:
		/// HTS Number 	8514908000
		/// Quantity	10,000 
		/// Country of Origin Code	CA
		/// Consolidated Summary Indicator	Y
		/// Release Entry Number (1)	00100501
		/// Release Entry Number (2)	00100502
		/// Release Entry Number (3)	00100503
		/// </summary>
		[TestDate(2018, 11, 20)]
		public void Test66()
		{
			invoiceLine.JI_Tariff = "8514908000";
			invoiceLine.JI_CustomsQuantity = 10000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CA";

			declaration.US_ConsolACE = true;

			var coll = new Freight.Business.TransportCollection(declaration);
			coll.Load();
			coll.RemoveAndDeleteAll();

			var releaseDec = Factory.New<JobDeclaration>();
			releaseDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			releaseDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			releaseDec.US_EntryFilerCode = "SV9";
			releaseDec.US_SchDEntry = declaration.US_SchDEntry;
			releaseDec.IOROrgPK = declaration.IOROrgPK;
			releaseDec.JE_TransportMode = declaration.JE_TransportMode;
			releaseDec.JE_VesselName = declaration.JE_VesselName;
			releaseDec.JE_VoyageFlightNo = declaration.JE_VoyageFlightNo;
			releaseDec.ConsigneeAddressOrgPK = declaration.ConsigneeAddressOrgPK;
			releaseDec.ReleaseStatus = CRLReleaseStatusList.Codes.REL;
			releaseDec.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			releaseDec.JE_EntryAuthorisationDate = new ZDateTime(2018, 11, 8);
			var entryHeader = releaseDec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			entryHeader.EntryNumber = "00100501";
			var entryNumber1 = entryHeader.CusEntryNumber;
			entryNumber1.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			var releaseDec2 = Factory.New<JobDeclaration>();
			releaseDec2.JE_MessageType = JobMessageTypeList.Codes.Import;
			releaseDec2.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			releaseDec2.US_EntryFilerCode = "SV9";
			releaseDec2.US_SchDEntry = declaration.US_SchDEntry;
			releaseDec2.IOROrgPK = declaration.IOROrgPK;
			releaseDec2.JE_TransportMode = declaration.JE_TransportMode;
			releaseDec2.JE_VesselName = declaration.JE_VesselName;
			releaseDec2.JE_VoyageFlightNo = declaration.JE_VoyageFlightNo;
			releaseDec2.ConsigneeAddressOrgPK = declaration.ConsigneeAddressOrgPK;
			releaseDec2.ReleaseStatus = CRLReleaseStatusList.Codes.REL;
			releaseDec2.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			releaseDec2.JE_EntryAuthorisationDate = new ZDateTime(2018, 11, 8);
			var entryHeader2 = releaseDec2.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			entryHeader2.EntryNumber = "00100502";
			var entryNumber2 = entryHeader2.CusEntryNumber;
			entryNumber2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			var releaseDec3 = Factory.New<JobDeclaration>();
			releaseDec3.JE_MessageType = JobMessageTypeList.Codes.Import;
			releaseDec3.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			releaseDec3.US_EntryFilerCode = "SV9";
			releaseDec3.US_SchDEntry = declaration.US_SchDEntry;
			releaseDec3.IOROrgPK = declaration.IOROrgPK;
			releaseDec3.JE_TransportMode = declaration.JE_TransportMode;
			releaseDec3.JE_VesselName = declaration.JE_VesselName;
			releaseDec3.JE_VoyageFlightNo = declaration.JE_VoyageFlightNo;
			releaseDec3.ConsigneeAddressOrgPK = declaration.ConsigneeAddressOrgPK;
			releaseDec3.ReleaseStatus = CRLReleaseStatusList.Codes.REL;
			releaseDec3.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			releaseDec3.JE_EntryAuthorisationDate = new ZDateTime(2018, 11, 8);
			var entryHeader3 = releaseDec3.CustomsEntryHeaders.AddNew();
			entryHeader3.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			entryHeader3.EntryNumber = "00100503";
			var entryNumber3 = entryHeader3.CusEntryNumber;
			entryNumber2.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			var invoice = declaration.Invoices[0];
			invoice.US_ReleaseEntryNumber = "SV900100501";

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "Inv123";
			invoice2.JZ_InvoiceAmount = 1000m;
			invoice2.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;
			invoice2.US_ReleaseEntryNumber = "SV900100501";
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 1000m;
			invoiceLine2.JI_CustomsQuantity = 100m;
			invoiceLine2.JI_Tariff = "8514908000";
			invoiceLine2.US_UC_NKCountryOfOrigin = "CA";
			invoiceLine2.US_SPI = "N/A";

			var invoice3 = declaration.Invoices.AddNew();
			invoice3.JZ_InvoiceNumber = "Inv12345";
			invoice3.JZ_InvoiceAmount = 1000m;
			invoice3.US_TransactionsRelated = YesNoDefaultList.Codes.Yes;
			invoice3.US_ReleaseEntryNumber = "SV900100502";
			var invoiceLine3 = invoice3.InvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 1000m;
			invoiceLine3.JI_CustomsQuantity = 100m;
			invoiceLine3.JI_Tariff = "8514908000";
			invoiceLine3.US_UC_NKCountryOfOrigin = "CA";
			invoiceLine3.US_SPI = "N/A";
			Factory.Save();

			MergeAndSend("ACE66");

			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
			var blocks = message.MessageBlock.MessageBlocks.FindAll(x => x is AENS32);
			AssertEquals("Only one 32", 1, blocks.Count);

			var aens32 = blocks.FirstOrDefault() as AENS32;
			AssertEquals("00100501", aens32.ReleaseEntryNumber1);
			AssertEquals("00100502", aens32.ReleaseEntryNumber2);
			AssertEquals(ZString.Empty, aens32.ReleaseEntryNumber3);
			AssertEquals(ZString.Empty, aens32.ReleaseEntryNumber4);
			AssertEquals(ZString.Empty, aens32.ReleaseEntryNumber5);
			AssertEquals(ZString.Empty, aens32.ReleaseEntryNumber6);
		}

		/// <summary>
		/// Electronic Invoice Details
		/// Submit an entry summary as follows:
		/// HTS Number 	9405408000
		/// Quantity	804 NO
		/// Country of Origin Code	CN
		/// Electronic Invoice Indicator	Y
		/// Supplier ID Code	USACS1301WAS
		/// Invoice Number	880035
		/// Invoice Line Range - Begin	0001
		/// Invoice Line Range - End 	0004
		/// </summary>
		public void Test67()
		{
			invoiceLine.JI_Tariff = "9405408000";
			invoiceLine.JI_CustomsQuantity = 804m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";

			declaration.US_IsInvoiceByRequest = true;
			manufacturerCode.OK_CustomsRegNo = "USACS1301WAS";
			invoiceHeader.JZ_InvoiceNumber = "880035";
			invoiceHeader.US_IsLineGrouping = true;

			var range = invoiceLine.LineGroupingRanges[0];
			range.US_StartSequenceNo = 1;
			range.US_EndSequenceNo = 4;

			MergeAndSend("ACE67");

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var message = (MQEDIMessage)entry.Messages[0];

			Assert("44 should be constructed only if required", message.MessageBlock.MessageBlocks.OfType<AENS44>().Any());
		}

		/// <summary>
		/// Submit an entry summary with the following information as a commercial description:
		/// Country of Origin	CA
		/// HTS Number	3918101000
		/// Commercial Description	Vinyl Floor Tile – Marble Simulated, 12X12
		/// </summary>
		public void Test68()
		{
			invoiceLine.JI_Tariff = "3918101000";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_Description = "Vinyl Floor Tile – Marble Simulated, 12X12";
			invoiceLine.US_UC_NKCountryOfOrigin = "CA";

			MergeAndSend("ACE68");
		}

		/// <summary>
		/// Prototypes
		/// Submit an entry summary for an automobile prototype as follows:
		/// Value of Goods Amount	$500,000
		/// Quantity	1 NO
		/// Country of Origin Code	CA
		/// HTS Number 1	98178501
		/// HTS Number 2	8703330045
		/// </summary>
		[TestDate(2016, 03, 27)]
		public void Test69_Failed()
		{
			invoiceLine.JI_LinePrice = 500000m;
			invoiceLine.US_UC_NKCountryOfOrigin = "CA";
			invoiceLine.JI_Tariff = "8703330045";
			invoiceLine.US_SupTariff = "98178501";// HTS not known Check with Phyllis
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.JI_CustomsQuantity = 1m;

			var dot = invoiceLine.DOTs.AddNew();
			dot.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._2A;
			dot.US_DOTClarCode = ClarificationCodeList.Codes.Vehicle;

			var vin = dot.DOTVINs.AddNew();
			vin.US_DOTMake = "ABC";
			vin.US_DOTModel = "UTV-001";
			vin.US_DOTYear = ZDateTime.Today.Year;
			vin.US_DOTVIN = "TEST-VIN-NUMBER";

			MergeAndSend("ACE69");
		}

		/// <summary>
		/// Submit an entry summary for a future reconciliation as follows:Future Reconciliation
		/// Country of Origin Code	CA
		/// HTS Number 	8414513000
		/// NAFTA Reconciliation	Yes
		/// Reconciliation Issue Code	002
		/// </summary>
		public void Test70()
		{
			declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.ClassRecon;
			declaration.US_NAFTAReconIndicator = true;
			declaration.JE_DateOfArrival = declaration.JE_ExportDate.AddDays(5);
			invoiceLine.JI_Tariff = "8414513000";
			invoiceLine.US_UC_NKCountryOfOrigin = "CA";

			MergeAndSend("ACE70");
		}

		/// <summary>
		/// Submit an entry summary certifying it for cargo release as follows:Cargo Release Certification
		/// HTS Number	9106908500
		/// Country of Origin Code	KR
		/// Cargo Release Certification Request Indicator	Yes
		/// Subsequently, receive and process the cargo release transaction processing output as specified in the ACE ABI CATAIR chapter 
		/// “Cargo Release (Certified from an ACE Entry Summary)”, application identifier HD.  Contact the assigned client representative about these results.
		/// </summary>
		public void Test71()
		{
			CreateNewOrGetExistingCusCodeListForFIRMSType("A003");
			invoiceLine.JI_Tariff = "9106908500";
			invoiceLine.JI_CustomsSecondQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			var seller = Factory.NewWithValidTestData<OrgHeader>();
			invoiceLine.InvoiceHeader.JZ_OA_SellerAddress = seller.MainAddress.PK;

			declaration.US_US_NKLocationOfGoods = "A003";
			declaration.US_EntryDate = declaration.JE_ExportDate.AddDays(2);

			var coll = new Freight.Business.TransportCollection(declaration);
			coll.Load();
			coll.RemoveAndDeleteAll();
			declaration.US_CertifyCargoRelease = true;
			MergeAndSend("ACE71", true, false);
		}

		// Test72 is a manual process and contact Rep. to issue Entry Summary Status notification

		/// <summary>
		/// Submit an AD/CVD Case Information Query with the following case information:AD/CVD Case Information Query – Case Numbers
		/// Case Number #1	A475818001
		/// Case Number #2	C475819017
		/// Case Number #3	A427109040
		/// Case Number #4	A405803001
		/// Case Number #5	A588201000
		/// Case Number #6	C475819001
		/// Case Number #7	A570001002
		/// </summary>
		public void Test73()
		{
			var caseNumbers = new List<ZString>(new ZString[] { "A475818001", "C475819017", "A427109040", "A405803001", "A588201000", "C475819001", "A570001002" });

			var message = new ACEACQueryMessageBuilder().Generate(Factory, caseNumbers);

			AssertEquals(2, message.MessageBlock.MessageBlocks.FindAll(x => x is AADQQ1).Count);
			Assert(message.EM_MessageText.Contains("A475818001"));
			Assert(message.EM_MessageText.Contains("C475819017"));
			Assert(message.EM_MessageText.Contains("A427109040"));
			Assert(message.EM_MessageText.Contains("A405803001"));
			Assert(message.EM_MessageText.Contains("C475819001"));
			Assert(message.EM_MessageText.Contains("A570001002"));
		}

		/// <summary>
		/// Submit an AD/CVD Case Information Query with the following information:AD/CVD Case Information Query – HTS Number
		/// HTS Number	7210703000
		/// </summary>
		public void Test74()
		{
			var queryInput = new ACEACCaseQueryInput();
			queryInput.CaseStatus = "B";
			queryInput.HTSNumber = "7210703000";

			var message = new ACEACQueryMessageBuilder().Generate(Factory, queryInput);
			AssertEquals(1, message.MessageBlock.MessageBlocks.FindAll(x => x is AADQQ2).Count);
			Assert(message.EM_MessageText.Contains("7210703000"));
		}

		/// <summary>
		/// Submit an AD/CVD Case Information Query with the following information:AD/CVD Case Information Query – Date Since Last Update #1
		/// Date Since Last Update	Insert a date 2 weeks prior to the date of transmission
		/// </summary>
		public void Test75_Fail()
		{
			var queryInput = new ACEACCaseQuery(Factory);
			queryInput.US_DateSinceLastUpdate = ZDateTime.Today.AddDays(-14).Date;

			Assert(queryInput.US_DateSinceLastUpdateInfo.HasMessageError(ACEACCaseQueryValidation.MustBeWithin7Days));

			var message = new ACEACQueryMessageBuilder().Generate(Factory, queryInput);
			AssertEquals(1, message.MessageBlock.MessageBlocks.FindAll(x => x is AADQQ2).Count);
		}

		/// <summary>
		/// Submit an AD/CVD Case Information Query with the following case information:AD/CVD Case Information Query – Date Since Last Update #2
		/// Date Since Last Update	Insert a date 2 days prior to the date of transmission
		/// </summary>
		public void Test76()
		{
			var queryInput = new ACEACCaseQuery(Factory);
			queryInput.US_DateSinceLastUpdate = ZDateTime.Today.AddDays(-2).Date;
			Assert(!queryInput.US_DateSinceLastUpdateInfo.HasMessageErrors());

			var message = new ACEACQueryMessageBuilder().Generate(Factory, queryInput);
			AssertEquals(1, message.MessageBlock.MessageBlocks.FindAll(x => x is AADQQ2).Count);
		}

		/// <summary>
		/// Submit an entry summary with the following AD/CVD case information: Related Cases
		/// Entry Type	03
		/// Case Number 	A475818001
		/// Case Number	C475819017
		/// </summary>
		public void Test77()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;

			invoiceLine.InvoiceHeader.US_FDAContactName = "Test Contact";
			invoiceLine.InvoiceHeader.US_FDAContactPhoneNo = "0648529756";
			invoiceLine.JI_Tariff = "1901909095";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.OnceOff;
			invoiceLine.US_ADDCaseNo = "A475818001";
			invoiceLine.US_CVDCaseNo = "C475819017";
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			invoiceLine.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			invoiceLine.US_UC_NKCountryOfOrigin = "IT";
			invoiceLine.JI_OA_ExporterAddress = manufacturer.MainAddress.PK;

			if (invoiceLine.AntidumpingDutyCase == null)
			{
				SetUpADDCVDCaseIfTestingNotSendingToCustoms("A475818001", ZDateTime.BrettsBirthday, "IT", "1901909095", 0.5m, 0m);
			}

			if (invoiceLine.CountervailingDutyCase == null)
			{
				SetUpADDCVDCaseIfTestingNotSendingToCustoms("C475819017", ZDateTime.BrettsBirthday, "IT", "1901909095", 0.5m, 0m);
			}
			MergeAndSend("ACE77");
		}

		/// <summary>
		/// Submit an entry summary with the following AD/CVD case information: Case Status
		/// Entry Type	03
		/// Case Number 	A427109040
		/// </summary>
		public void Test78_Fail()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			invoiceLine.JI_Tariff = "7208403060";
			invoiceLine.US_ADDCaseNo = "A427109040";
			invoiceLine.US_UC_NKCountryOfOrigin = "FR";
			invoiceLine.JI_OA_ExporterAddress = manufacturer.MainAddress.PK;

			if (invoiceLine.AntidumpingDutyCase == null)
			{
				SetUpADDCVDCaseIfTestingNotSendingToCustoms("A427109040", ZDateTime.BrettsBirthday, "FR", "7208403060", 0.5m, 0m, ACCaseStatusList.Codes.IO, "START", ZDateTime.BrettsBirthday);
			}

			MergeAndSend("ACE78", false, true);// case is revoked
		}

		/// <summary>
		/// Submit an entry summary with the following AD/CVD case information: HTS Number and Case Number
		/// Entry Type	03
		/// Case Number 	A405803001
		/// HTS Number	3912390000
		/// </summary>
		public void Test79_Fail()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			invoiceLine.JI_Tariff = "3912390000";
			invoiceLine.US_ADDCaseNo = "A405803001";
			invoiceLine.JI_OA_ExporterAddress = manufacturer.MainAddress.PK;

			if (invoiceLine.AntidumpingDutyCase == null)
			{
				SetUpADDCVDCaseIfTestingNotSendingToCustoms("A405803001", ZDateTime.BrettsBirthday, "JP", "39123100", 0.5m, 0m);
			}

			MergeAndSend("ACE79", false, true);// not a valid tariff number. 3912.31.00 is a valid tariff number
		}

		/// <summary>
		/// Submit an entry summary with the following AD/CVD case information: Case with Ad Valorem Rate
		/// Entry Type	03
		/// Case Number 	A588201000
		/// Case Rate Type Qualifier Code	S
		/// </summary>
		public void Test80_Fail()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			invoiceLine.JI_Tariff = "8431390010";
			invoiceLine.US_ADDCaseNo = "A588201000";
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			invoiceLine.US_UC_NKCountryOfOrigin = "JP";
			invoiceLine.JI_OA_ExporterAddress = manufacturer.MainAddress.PK;

			if (invoiceLine.AntidumpingDutyCase == null)
			{
				SetUpADDCVDCaseIfTestingNotSendingToCustoms("A588201000", ZDateTime.BrettsBirthday, "JP", "8431390010", 0.5m, 0m);
			}

			MergeAndSend("ACE80", false, true);

			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
			var ens53 = message.MessageBlock.MessageBlocks.OfType<AENS53>().FirstOrDefault();
			AssertEquals("When S is sent, no amount should be sent", ZDecimal.Zero, ens53.ADCVDValueOfGoodsAmount);
		}

		public void TestWarehouse21WithADDCVD()
		{
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			invoiceLine.JI_Tariff = "8431390010";
			invoiceLine.US_ADDCaseNo = "A588201000";
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.Specific;
			invoiceLine.US_UC_NKCountryOfOrigin = "JP";
			invoiceLine.JI_OA_ExporterAddress = manufacturer.MainAddress.PK;

			if (invoiceLine.AntidumpingDutyCase == null)
			{
				SetUpADDCVDCaseIfTestingNotSendingToCustoms("A588201000", ZDateTime.BrettsBirthday, "JP", "8431390010", 0.5m, 0m);
			}

			MergeAndSend("Warehouse21WithADDCVD", false, true);

			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
			var ens88 = message.MessageBlock.MessageBlocks.OfType<AENS88>().FirstOrDefault();
			AssertNotNull(ens88);
		}

		/// <summary>
		/// Submit an entry summary with the following AD/CVD case information: 
		/// Entry Type	03
		/// Case Number 	A588201000
		/// Bond/Cash Claim Code	C
		/// </summary>
		public void Test81()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			invoiceLine.JI_Tariff = "8431390010";
			invoiceLine.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.OnceOff;
			invoiceLine.US_ADDCaseNo = "A588201000";
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;

			if (invoiceLine.AntidumpingDutyCase == null)
			{
				SetUpADDCVDCaseIfTestingNotSendingToCustoms("A588201000", new ZDateTime(1989, 5, 15), "JP", "8431390010", 0.4583m, 0m);
			}

			invoiceLine.US_IsBondedADD = false;
			invoiceLine.US_UC_NKCountryOfOrigin = "JP";
			invoiceLine.JI_OA_ExporterAddress = manufacturer.MainAddress.PK;

			MergeAndSend("ACE81");

			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
			var ens53 = message.MessageBlock.MessageBlocks.OfType<AENS53>().FirstOrDefault();

			AssertEquals("Should send rate as %", 45.83m, ens53.CaseDepositRate);
		}

		/// <summary>
		/// Submit an entry summary with the following AD/CVD case information: Case with Differing Values
		/// Entry Type 	03
		/// Case Number	A475818001
		/// Case Number	C475819001
		/// HTS Number	1901909095
		/// Country of Origin	IT
		/// Value of Goods Amount	$20,000
		/// Quantity	320
		/// Antidumping Value of Goods Amount	$15,000
		/// </summary>
		public void Test82()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			invoiceLine.InvoiceHeader.US_FDAContactName = "Test Contact";
			invoiceLine.InvoiceHeader.US_FDAContactPhoneNo = "0648529756";
			invoiceLine.JI_Tariff = "1901909095";
			invoiceLine.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.OnceOff;
			invoiceLine.US_ADDCaseNo = "A475818001";
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			invoiceLine.US_CVDCaseNo = "C475819001";
			invoiceLine.US_CVDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			invoiceLine.US_UC_NKCountryOfOrigin = "IT";
			invoiceLine.JI_LinePrice = 20000m;
			invoiceLine.JI_CustomsQuantity = 320m;
			invoiceLine.US_ADDDepositValue = 15000m;
			invoiceLine.JI_OA_ExporterAddress = manufacturer.MainAddress.PK;

			if (invoiceLine.AntidumpingDutyCase == null)
			{
				SetUpADDCVDCaseIfTestingNotSendingToCustoms("A475818001", ZDateTime.BrettsBirthday, "IT", "1901909095", 0.5m, 0m);
			}

			if (invoiceLine.CountervailingDutyCase == null)
			{
				SetUpADDCVDCaseIfTestingNotSendingToCustoms("C475819001", ZDateTime.BrettsBirthday, "IT", "1901909095", 0.5m, 0m);
			}

			MergeAndSend("ACE82");
		}

		/// <summary>
		/// Submit an entry summary with the following AD/CVD case information: Case and Deposit Rate
		/// Entry Type 	03
		/// Case Number	A570001002
		/// Country of Origin	CN
		/// </summary>
		public void Test83()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			invoiceLine.JI_Tariff = "2841610000";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.OnceOff;
			invoiceLine.US_ADDCaseNo = "A570001002";
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.JI_OA_ExporterAddress = manufacturer.MainAddress.PK;

			if (invoiceLine.AntidumpingDutyCase == null)
			{
				SetUpADDCVDCaseIfTestingNotSendingToCustoms("A570001002", ZDateTime.BrettsBirthday, "CN", "2841610000", 0.5m, 0m);
			}

			MergeAndSend("ACE83");
		}

		public void TestWhenPaidAndCorrectingEntrySummary()
		{
			invoiceLine.JI_Tariff = "3506990000";
			invoiceLine.US_UC_NKCountryOfExport = "HK";
			invoiceLine.US_UC_NKCountryOfOrigin = "HK";
			declaration.US_Paid = YesNoDefaultList.Codes.Yes;

			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(1);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entrySummary = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			Assert("PreCondition", ((IACECusEntryHeader)entrySummary).IsPaid);

			var message = new ACEEntrySummaryMessageBuilderForTesting(entrySummary, false, true, UpdateActionCode.Add).PopulateMessage();
			var ens10 = message.MessageBlock.MessageBlocks.OfType<AENS10>().FirstOrDefault();

			AssertEquals("Should not populate payment details when correcting rejected & paid 7501", "", ens10.PaymentTypeCode);
			AssertEquals("Should not populate payment details when correcting rejected & paid 7501", ZDateTime.Empty, ens10.PreliminaryStatementPrintDate);
		}

		public void TestDeleteMessaging()
		{
			declaration.JE_MasterBill = "OBL10000";
			declaration.JE_DeclarationReference = "B00155570";
			declaration.US_BondWaiverCode = "996";
			Assert("PreCondition", !declaration.US_PaymentType.IsEmpty);
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.JI_Tariff = "0709902000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 105000m;
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_Weight = 10m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var ens = declaration.ActiveEntryHeaders[0];
			var builder = new ACEEntrySummaryMessageBuilderForTesting(ens, true, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			message.EM_MessageNum = "HYEDUSCMT_196571";
			message.EM_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-3);
			var messageBlock = (AENS10)message.MessageBlock.MessageBlocks.Find(x => x is AENS10);
			AssertNotNull(messageBlock);

			var rcvAddDone = GetReceivedMessage(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse, EDIMessage.Status.Received);
			rcvAddDone.EM_SystemCreateTimeUtc = ZDateTime.Today.AddHours(-2);
			rcvAddDone.EM_MessageNum = "HYEDUSCMT_196571";
			rcvAddDone.EM_MessageText = @"B001101SV9AX                                               HYEDUSCMT_196571     " +
"E0 SUMMRY 000001 REF ID: SV9 71032807 B00173079    171                          " +
"E1A 995   SUMMARY HAS BEEN ADDED                  SV9  7103280700100B00173079   " +
"Y  1101SV9AX00002";
			ens.Messages.Add(rcvAddDone);

			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(ens, false, false, UpdateActionCode.Delete);
			var deleteMessage = messageBuilder.PopulateMessage();

			AssertEquals(1, deleteMessage.MessageBlock.MessageBlocks.Count);
			var ens10 = (AENS10)deleteMessage.MessageBlock.MessageBlocks[0];

			AssertEquals("UpdateActionCode", "D", ens10.SummaryFilingActionRequestCode);
			AssertEquals("DistrictPortOfEntry", "3902", ens10.DistrictPortOfEntry);
			AssertEquals("EntryType", "01", ens10.EntryTypeCode);
			Assert(!ens10.EntryFilerCode.IsEmpty);
			Assert(!ens10.EntryNumber.IsEmpty);

			Assert(ens10.BrokerReferenceNumber.IsEmpty);
			Assert(ens10.BondWaiverIndicator.IsEmpty);
			Assert(ens10.BondWaiverReasonCode.IsEmpty);
			Assert(ens10.CargoReleaseCertificationRequestIndicator.IsEmpty);
			Assert(ens10.LiveEntryIndicator.IsEmpty);
			Assert(ens10.ModeOfTransportationMOTCode.IsEmpty);
			Assert(ens10.DeferredTaxPaymentCode.IsEmpty);
			Assert(ens10.ElectronicInvoiceIndicator.IsEmpty);
			Assert(ens10.ElectronicSignature.IsEmpty);
			Assert(ens10.PaymentTypeCode.IsEmpty);
			Assert(ens10.PreliminaryStatementPrintDate.IsEmpty);
			Assert(ens10.ShipmentUsageTypeCode.IsEmpty);
		}

		public void TestBuildDeleteMessageWithoutResponseMsg()
		{
			declaration.JE_MasterBill = "OBL10000";
			declaration.JE_DeclarationReference = "B00155570";
			declaration.US_BondWaiverCode = "996";
			Assert("PreCondition", !declaration.US_PaymentType.IsEmpty);
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.JI_Tariff = "0709902000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 105000m;
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_Weight = 10m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var ens = declaration.ActiveEntryHeaders[0];
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(ens, false, false, UpdateActionCode.Delete);
			var deleteMessage = messageBuilder.PopulateMessage();

			AssertEquals(1, deleteMessage.MessageBlock.MessageBlocks.Count);
			var ens10 = (AENS10)deleteMessage.MessageBlock.MessageBlocks[0];

			AssertEquals("UpdateActionCode", "D", ens10.SummaryFilingActionRequestCode);
			AssertEquals("DistrictPortOfEntry", "3902", ens10.DistrictPortOfEntry);
			AssertEquals("EntryType", "01", ens10.EntryTypeCode);
			Assert(!ens10.EntryFilerCode.IsEmpty);
			Assert(!ens10.EntryNumber.IsEmpty);

			Assert(ens10.BrokerReferenceNumber.IsEmpty);
			Assert(ens10.BondWaiverIndicator.IsEmpty);
			Assert(ens10.BondWaiverReasonCode.IsEmpty);
			Assert(ens10.CargoReleaseCertificationRequestIndicator.IsEmpty);
			Assert(ens10.LiveEntryIndicator.IsEmpty);
			Assert(ens10.ModeOfTransportationMOTCode.IsEmpty);
			Assert(ens10.DeferredTaxPaymentCode.IsEmpty);
			Assert(ens10.ElectronicInvoiceIndicator.IsEmpty);
			Assert(ens10.ElectronicSignature.IsEmpty);
			Assert(ens10.PaymentTypeCode.IsEmpty);
			Assert(ens10.PreliminaryStatementPrintDate.IsEmpty);
			Assert(ens10.ShipmentUsageTypeCode.IsEmpty);
		}

		public void TestDeleteMessagingWithAcceptedMessage()
		{
			var declaration = GetMergedDeclaration("B00173079");
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;

			AssertNotNull(declaration.ActiveEntryHeaders[0]);
			var builder = new ACEEntrySummaryMessageBuilderForTesting(declaration.ActiveEntryHeaders[0], true, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			message.EM_MessageNum = "HYEDUSCMT_196571";
			message.EM_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-3);
			var messageBlock = (AENS10)message.MessageBlock.MessageBlocks.Find(x => x is AENS10);
			AssertNotNull(messageBlock);

			var rcvAddDone = GetReceivedMessage(ACEApplicationIdentifierCodeList.Codes.EntrySummaryResponse, EDIMessage.Status.Received);
			rcvAddDone.EM_SystemCreateTimeUtc = ZDateTime.Today.AddHours(-2);
			rcvAddDone.EM_MessageNum = "HYEDUSCMT_196571";
			rcvAddDone.EM_MessageText = @"B001101SV9AX                                               HYEDUSCMT_196571     " +
"E0 SUMMRY 000001 REF ID: SV9 71032807 B00173079    171                          " +
"E1A 995   SUMMARY HAS BEEN ADDED                  SV9  7103280700100B00173079   " +
"Y  1101SV9AX00002";
			declaration.ActiveEntryHeaders[0].Messages.Add(rcvAddDone);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(declaration.ActiveEntryHeaders[0], false, false, UpdateActionCode.Delete);
			var deleteMessage = messageBuilder.PopulateMessage();

			var ens10 = (AENS10)deleteMessage.MessageBlock.MessageBlocks.Find(x => x is AENS10);
			AssertEquals("UpdateActionCode", "D", ens10.SummaryFilingActionRequestCode);
			AssertEquals("EntryType", messageBlock.EntryTypeCode, ens10.EntryTypeCode);
			AssertEquals(messageBlock.EntryFilerCode, ens10.EntryFilerCode);
			AssertEquals(messageBlock.DistrictPortOfEntry, ens10.DistrictPortOfEntry);
			AssertEquals(messageBlock.EntryTypeCode, ens10.EntryTypeCode);
			AssertEquals(messageBlock.KnownImporterIndicator, ens10.KnownImporterIndicator);
		}

		public void TestCargoManifestGroupingRequired()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVDQuotaVisa;
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			invoiceLine.US_ManifestQty = 1000;

			var subHouse1 = declaration.Bills.AddNew();
			subHouse1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			subHouse1.CU_BillNum = "H273";

			var subHouse2 = declaration.Bills.AddNew();
			subHouse2.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHouse2.CU_BillNum = "J878";
			subHouse2.CU_NoOfPacks = 110m;
			subHouse2.CU_PackType = "CTN";
			subHouse2.ITAndSplitDetails.AddNew().US_ITNumber = "111271845";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var builder = new ACEEntrySummaryMessageBuilderForTesting(invoiceLine.Declaration.ActiveEntryHeaders.EntrySummaryEntry, true, true, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var messageBlock = (AENS22)message.MessageBlock.MessageBlocks.Find(x => x is AENS22);
			AssertNotNull(messageBlock);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableENS = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			builder = new ACEEntrySummaryMessageBuilderForTesting(invoiceLine.Declaration.ActiveEntryHeaders.EntrySummaryEntry, true, true, UpdateActionCode.Add);
			message = builder.PopulateMessage();
			messageBlock = (AENS22)message.MessageBlock.MessageBlocks.Find(x => x is AENS22);
			AssertNotNull("Cargo Manifest Grouping is allowed for entry types 06", messageBlock);

			var messageBlock41 = (AENS41)message.MessageBlock.MessageBlocks.Find(x => x is AENS41);
			AssertNotNull(messageBlock41);
			AssertEquals(1000, messageBlock41.FTZLineItemQuantity);
		}

		public void TestIncludePGABlocksForEntrySummary()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Description = "testing";
			var vehicleLine = invoiceLine.VehicleLines.AddNew();
			vehicleLine.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;

			var invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine2.US_VNEInd = OGAIndicatorList.Codes.Declared;
			var vehicleLine2 = invoiceLine2.VehicleLines.AddNew();
			vehicleLine2.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			AssertEquals(1, actions.Count);

			var action = actions[0];

			action.US_SendMessage = true;
			action.US_CertifyCargoRelease = true;
			actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
			var oiRecords = message.MessageBlock.MessageBlocks.FindAll(x => x is MessageBuildingBlocks.Common.AENSOI);
			AssertEquals("OI records for the main tariff", 2, oiRecords.Count);
		}

		public void TestIncludePGABlocksForACECargoReleaseCertification()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_VNEInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Description = "testing";
			var vehicleLine = invoiceLine.VehicleLines.AddNew();
			vehicleLine.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;

			var invoiceLine2 = invoiceLine.AddSecondaryInvoiceLine();
			invoiceLine2.US_VNEInd = OGAIndicatorList.Codes.Declared;
			var vehicleLine2 = invoiceLine2.VehicleLines.AddNew();
			vehicleLine2.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			AssertEquals(1, actions.Count);

			var action = actions[0];

			action.US_SendMessage = true;
			action.US_CertifyCargoRelease = true;
			actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
			var oiRecords = message.MessageBlock.MessageBlocks.FindAll(x => x is MessageBuildingBlocks.Common.AENSOI);
			AssertEquals("OI records for the main tariff and secondary tariffs", 2, oiRecords.Count);
		}

		public void TestExcludePGABlocksForACECargoReleaseCertification()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;

			declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			AssertEquals(1, actions.Count);

			var action = actions[0];

			action.US_SendMessage = true;
			action.US_CertifyCargoRelease = true;
			actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
			var oiRecords = message.MessageBlock.MessageBlocks.FindAll(x => x is MessageBuildingBlocks.Common.AENSOI);
			AssertEquals("OI records", 0, oiRecords.Count);
		}

		public void TestSendPSC()
		{
			declaration.US_PSC = true;
			declaration.US_EntryFilerCode = "ABC";
			declaration.ImportEntryNumber = "00000018";
			declaration.US_AccLiqReq = true;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;
			declaration.US_NAFTAReconIndicator = true;
			declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.ClassRecon;
			declaration.US_US_NKLocationOfGoods = "A001";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var pscReasons = new PSCReasonCodeCollection(entry);
			var pscReason = pscReasons.AddNew();
			pscReason.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.Entry;
			pscReason.Reason1 = PSCHeaderReasonList.Codes.H01;
			pscReason.Reason2 = PSCHeaderReasonList.Codes.H02;
			pscReason.Reason3 = PSCHeaderReasonList.Codes.H99;

			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			var action = new EntryHeaderMessageSendingAction(entry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			action.US_CertifyCargoRelease = true;
			action.US_AcknowledgeAndSign = true;
			var messageBuilder = new ACEEntrySummaryMessageBuilder(entry, ACEEntrySummaryMessageSendingOption.New(action), "This is a test PSC explanation text", pscReasons, UpdateActionCode.Add);
			var message = messageBuilder.PopulateMessage();

			var ens10 = message.MessageBlock.MessageBlocks.OfType<AENS10>().FirstOrDefault();
			AssertNotNull(ens10);
			AssertEquals(ZString.Empty, ens10.CargoReleaseCertificationRequestIndicator);
			AssertEquals(ZString.Empty, ens10.LiveEntryIndicator);
			AssertEquals(ZString.Empty, ens10.TradeAgreementReconciliationIndicator);
			AssertEquals(ZString.Empty, ens10.ReconciliationIssueCode);
			AssertEquals(ZString.Empty, ens10.PaymentTypeCode);
			AssertEquals(ZDate.Empty, ens10.PreliminaryStatementPrintDate);
			AssertEquals(ZString.Empty, ens10.PeriodicStatementMonth);
			AssertEquals(ZString.Empty, ens10.StatementClientBranchIdentifier);

			var ens20 = message.MessageBlock.MessageBlocks.OfType<AENS20>().FirstOrDefault();
			AssertNotNull(ens20);
			AssertEquals(ZString.Empty, ens20.LocationOfGoodsCode);

			var ens35 = message.MessageBlock.MessageBlocks.OfType<AENS35>().FirstOrDefault();
			AssertNotNull(ens35);
			AssertEquals("Header reason", PSCHeaderReasonList.Codes.H01, ens35.PostSummaryCorrectionHeaderReasonCode1);
			AssertEquals("Header reason", PSCHeaderReasonList.Codes.H02, ens35.PostSummaryCorrectionHeaderReasonCode2);
			AssertEquals("Header reason", PSCHeaderReasonList.Codes.H99, ens35.PostSummaryCorrectionHeaderReasonCode3);

			var ens36 = message.MessageBlock.MessageBlocks.OfType<AENS36>().FirstOrDefault();
			AssertEquals("explanation text", "This is a test PSC explanation text".ToUpper(), ens36.PSCFilingExplanationText);
		}

		public void TestConstructionOfCargoReleaseBlocks()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XXX";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var collection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			var action = new EntryHeaderMessageSendingAction(entry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			action.US_CertifyCargoRelease = true;
			action.US_AcknowledgeAndSign = true;
			action.US_SE_ContactName = "Joo Youm";
			action.US_SE_ContactPhone = "5555555555";
			action.US_SE_DISIndicator = true;
			action.US_SE_DISIDRefNo = "ABC432089u";
			var messageBuilder = new ACEEntrySummaryMessageBuilder(entry, ACEEntrySummaryMessageSendingOption.New(action), UpdateActionCode.Replace);
			var message = messageBuilder.PopulateMessage();

			var se13 = (ASESE13)message.MessageBlock.MessageBlocks.FirstOrDefault(x => x is ASESE13);
			AssertNotNull(se13);
			AssertEquals("JOO YOUM", se13.ContactName);
			AssertEquals("5555555555", se13.ContactPhone);
			AssertEquals("1", se13.DISIndicator);
			AssertEquals("", se13.ReasonCode);
			AssertEquals("", se13.MultipleCargoDispositionsIndicator);
			AssertEquals("", se13.SplitShipmentIndicator);

			var se20 = (ASESE20)message.MessageBlock.MessageBlocks.FirstOrDefault(x => x is ASESE20 && ((ASESE20)x).ReferenceIdentifierQualifier == ReferenceIdentifierCodeList.Codes.DISReferenceNumber);
			AssertNotNull(se20);
			AssertEquals("ABC432089U", se20.ReferenceIdentifier);

			var se61 = (ASESE61)message.MessageBlock.MessageBlocks.FirstOrDefault(x => x is ASESE61);
			AssertNull(se61);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableENS = true;
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "2402108850";
			tariff.UE_DateFrom = ZDate.Today.AddMonths(-1);
			tariff.UE_DateTo = ZDate.Today.AddMonths(1);

			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			invoiceLine.US_PrivilegedStatusDate = ZDateTime.Today;
			invoiceLine.US_FTZCurrentTariff = "2402108850";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			action = new EntryHeaderMessageSendingAction(entry, ImportMessageStatusList.MessageType.EntrySummary, collection);
			action.US_CertifyCargoRelease = true;
			action.US_AcknowledgeAndSign = true;

			messageBuilder = new ACEEntrySummaryMessageBuilder(entry, ACEEntrySummaryMessageSendingOption.New(action), UpdateActionCode.Replace);
			message = messageBuilder.PopulateMessage();

			se61 = (ASESE61)message.MessageBlock.MessageBlocks.FirstOrDefault(x => x is ASESE61);
			AssertNotNull(se61);
			AssertEquals("2402108850", se61.CurrentHTSNumberForPFStatusMerchandise);
		}

		public void Test89RecordWithMandatoryFee()
		{
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.No;
			invoiceLine.JI_Tariff = "0409.00.0025";
			AssertNotNull("PreCondition", invoiceLine.ImportTariff);
			AssertEquals("PreCondition", 1, invoiceLine.ImportTariff.GetRequiredFeeCodes().Count());
			invoiceLine.JI_CustomsQuantity = ZDecimal.Zero;
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.US_SPI = "AU";

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0201100590";
			invoiceLine2.JI_CustomsQuantity = ZDecimal.Zero;
			invoiceLine2.JI_LinePrice = 10000m;
			invoiceLine2.US_SPI = "AU";

			AssertEquals("PreCondition", 1, invoiceLine2.ImportTariff.GetRequiredFeeCodes().Count());

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Honey fee", 0m, invoiceLine.CusEntryLine.HoneyAmount);
			AssertEquals("Beef fee", 0m, invoiceLine2.CusEntryLine.BeefAmount);

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			var message = messageBuilder.PopulateMessage();

			var ens89 = message.MessageBlock.MessageBlocks.OfType<AENS89>().FirstOrDefault();

			AssertEquals("Honey & Beef fee code should exist with zero filled amounts. For other codes, space-filled", "890550000000000005300000000000                                                  ", ens89.Serialise());
		}

		public void TestPerishableFlagForSE20()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "AMYT";
			commodity.RH_IsPerishable = true;

			invoiceLine.JI_RH_NKCommodity_Code = commodity.RH_Code;

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			var message = messageBuilder.PopulateMessage();

			AssertContains("SE20PERY                                                                        ", message.EM_FormattedMessageText);
		}

		public void TestRemoteFilerForPSC()
		{
			mockDeclaration.Protected().Setup<ZString>("ProcessingDistrictPortCore");

			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EntryFiler() { EntryFilerCode = "SV9" });
			USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "3902");

			declaration.US_PSC = true;
			declaration.US_EntryFilerCode = "EEB";
			declaration.ImportEntryNumber = "01120001";
			declaration.US_SchDEntry = "3004";
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			declaration.US_PreparerDistrictPort = "3902";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			var message = messageBuilder.PopulateMessage();

			AssertContains("B  3004SV9AE                                  3902SV9  1   ", message.EM_MessageText);
		}

		public void TestADD_CVDWithoutPayableAmounts()
		{
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			invoiceLine.JI_Tariff = "3912390000";
			invoiceLine.US_ADDCaseNo = "A405803001";
			invoiceLine.JI_OA_ExporterAddress = manufacturer.MainAddress.PK;

			if (invoiceLine.AntidumpingDutyCase == null)
			{
				SetUpADDCVDCaseIfTestingNotSendingToCustoms("A405803001", ZDateTime.BrettsBirthday, "JP", "39123100", 0m, 0m);
			}

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			AssertEquals("PreCondition", 0m, entry.TotalAntidumpingDuty);

			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, false, false, UpdateActionCode.Add);
			var message = messageBuilder.PopulateMessage();

			Assert("Should have built a 88 record", message.MessageBlock.MessageBlocks.OfType<AENS88>().Any());
		}

		[TestDate(2013, 01, 18)]
		public void TestBondDetails()
		{
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			declaration.US_EntryType = EntryTypeList.Codes.InformalFreeDutiable;
			declaration.US_BondType = BondTypeList.Codes.NoBondRequired;
			declaration.US_BondType2 = BondTypeList.Codes.NoBondRequired;

			invoiceLine.JI_Tariff = "3917220000";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Australia;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			var message = messageBuilder.PopulateMessage();

			var ens10 = message.MessageBlock.MessageBlocks.OfType<AENS10>().FirstOrDefault();
			AssertEquals("10ASV9  <E#PLCH> 3902            11100XY          2012313                       ", ens10.Serialise());

			var ens31 = message.MessageBlock.MessageBlocks.OfType<AENS31>().FirstOrDefault();
			AssertNull(ens31);
		}

		[TestDate(2015, 04, 27)]
		public void TestBuildWithSplitDetails()
		{
			declaration.US_BondType2 = ZString.Empty;
			declaration.US_BondAmount2 = 0m;
			declaration.US_BondProducerAccNo2 = ZString.Empty;

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CertificationOptionsList.Codes.ACE;
			declaration.US_SESplitRel = SplitShipmentReleaseCodeList.Codes.HoldAll;

			var invoiceLine = declaration.InvoiceLines[0];
			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_FullName = "Manufacturer For Line 1";
			manufacturer.OH_Code = "MAN" + new Random().Next(1000000).ToString();
			var address = manufacturer.Addresses.AddNew();
			address.OA_Address1 = "Test man for line 1";
			address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "BHNHJKMEQU6LFR");
			invoiceLine.JI_OA_ManufacturerAddress = address.PK;

			declaration.JE_MasterBill = "MB4534535";
			var primaryMasterBill = declaration.PrimaryMasterBill;
			primaryMasterBill.CU_NoOfPacks = 12;
			primaryMasterBill.CU_PackType = DeclarationTestHelper.BillUS_ManifestUQForTest;

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "APLU001890";
			container2.CO_Seal = "SE456789";

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

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			var message = messageBuilder.PopulateMessage();
			AssertMultilineASCIIEquals("Expected Message should contain SE16 and SE17 blocks",
@"B  3902XJ5AE                                               <<MSGNO PLACEHOLDER>>
10ASV9  <E#PLCH> 3902B00001000   0111 XA          2050415                       
1191-01319900091-013199000                                  IL                  
20AAAA3902          APL EMERALD                                                 
SE13SUPPORT                                 123456                              
21V123W                                                                         
2200000049PK                                                                    
23MAAAAMB4534535                                                                
23H    HOUSEBILL1                                                               
SE16A0  002W 04281500000012PK                                                   
SE16D0  456I 04291500000007PK                                                   
SE17APLU001890                                                                  
318B 891                                                                        
SE20SSR1                                                                        
SE30ST MR IMPORTER                                                              
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
40  001 BHAU042015        0000000050602670000009000    N                        
44COMMERCIAL DESCRIPTION                                                        
47MBHNHJKMEQU6LFR                                                               
47S91-013199000                                                                 
SE50MF MANUFACTURER FOR LINE 1                                                  
SE5515TEST MAN FOR LINE 1                                                       
SE56                                                           US               
504421909720 0000033000 0000010000 000000007000GR                               
6249900003464                                                                   
6250100001250                                                                   
894990000000346450100000001250                                                  
9000000033000 00000004714 00000000000 00000000000 00000000000 00000000000       
Y  3902XJ5AE", message.EM_FormattedMessageText);
		}

		[TestDate(2019, 05, 23)]
		public void TestBuildWithSplitDetailsOfDeclarationBillLevel()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CertificationOptionsList.Codes.ACE;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

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

			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(declaration.ActiveEntryHeaders.EntrySummaryEntry, true, true, UpdateActionCode.Add);
			var message = messageBuilder.PopulateMessage();
			AssertMultilineASCIIEquals("block 22 qty should not be sum of split details qty",
@"B  3902XJ5AE                                               <<MSGNO PLACEHOLDER>>
10ASV9  <E#PLCH> 3902B00001000   0110 XA          2052819                       
1191-01319900091-013199000                                  IL                  
20AAAA3902          APL EMERALD                                                 
SE13SUPPORT                                 123456                              
21V123W                                                                         
2200000002CS                                                                    
23I    777777770                                                                
23MAPLUMB4534535                                                                
2200000003CS                                                                    
23I    777777781                                                                
23MAPLUMB4534535                                                                
2200000005CS                                                                    
23I    777777792                                                                
23MAPLUMB4534535                                                                
2200000001PK                                                                    
23MAAAAMB4534535                                                                
318B 891                                                                        
319A    00000000101234                                                          
SE30ST MR IMPORTER                                                              
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
40  001 AUAU051619        0000000050602670000009000    N                        
44COMMERCIAL DESCRIPTION                                                        
47MTHLIATHA191NAK                                                               
47S91-013199000                                                                 
SE50MF MR MANUFACTURER                                                          
SE5515***ADDRESS NOT ON FILE***                                                 
SE56                                                           AU               
504421909720 0000033000 0000010000 000000007000GR                               
6249900003464                                                                   
6250100001250                                                                   
40  002 THAU051619        0000000000602670000000000    N                        
47MTHLIATHA191NAK                                                               
47S91-013199000                                                                 
50           0000000000 0000000000                                              
6249900000000                                                                   
894990000000346450100000001250                                                  
9000000033000 00000004714 00000000000 00000000000 00000000000 00000000000       
Y  3902XJ5AE", message.EM_FormattedMessageText);
		}

		public void TestJobWithProvTariff9903()
		{
			var testHelper = new CombinedLinesHelperTest();
			var testJob = testHelper.CombinedJob;
			var invoiceLine1 = testHelper.InvoiceHeaderForCombined.InvoiceLines.AddNew();
			invoiceLine1.US_SupTariff = testHelper.Chapter98TestingHelper.Test99038001Tariff.UE_Tariff;
			invoiceLine1.JI_Tariff = testHelper.Chapter98TestingHelper.TestCTariff.UE_Tariff;
			invoiceLine1.JI_LinePrice = 10000m;

			testJob.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = testJob.ActiveEntryHeaders.EntrySummaryEntry;
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			var messageText = messageBuilder.PopulateMessage().EM_FormattedMessageText;
			Assert(messageText.Contains(@"5099038001   0000400000 0000000000                                              
507601103000 0000700000 0000010000 000000000000KG                               "));
		}

		public void TestTIB9813SecondaryTariffLineENS50Order()
		{
			var testHelper = new Chapter98HelperTest();
			testHelper.ParentLine.US_SupTariff = testHelper.Test9813Tariff.UE_Tariff;
			testHelper.ChildLine.US_SupTariff = testHelper.Test99038801Tariff.UE_Tariff;

			testHelper.ParentLine.JI_Tariff = ZString.Empty;
			testHelper.ChildLine.JI_Tariff = testHelper.TestCTariff.UE_Tariff;
			testHelper.ParentLine.JI_LinePrice = ZDecimal.Zero;
			testHelper.ChildLine.JI_LinePrice = 5000m;

			testHelper.Charpter98Job.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			testHelper.Charpter98Job.US_EnableENS = true;
			testHelper.Charpter98Job.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			var entry = testHelper.Charpter98Job.ActiveEntryHeaders.EntrySummaryEntry;
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			var messageText = messageBuilder.PopulateMessage().EM_FormattedMessageText;
			Assert(messageText.Contains(@"509813000520 0000000000 0000000000                                              
5099038801   0000000000 0000000000                                              
507601103000 0000000000 0000005000 000000000000KG                               "));
		}

		[TestDate(2014, 07, 25)]
		public void TestIncludeCargoReleaseDetailsOnHeaderLevel()
		{
			DeclarationTestHelper.SetupBillsForSimplifiedEntryDeclaration(declaration);
			var primaryMasterBill = declaration.PrimaryMasterBill;
			primaryMasterBill.CU_NoOfPacks = 12;
			primaryMasterBill.CU_PackType = DeclarationTestHelper.BillUS_ManifestUQForTest;
			declaration.US_CargoReleaseType = CertificationOptionsList.Codes.ACE;
			declaration.US_BondType2 = ZString.Empty;
			declaration.US_BondAmount2 = 0m;
			declaration.US_BondProducerAccNo2 = ZString.Empty;

			var soldToParty = Factory.New<OrgHeader>();
			soldToParty.OH_FullName = "Sold To Party for ENS Testing";
			soldToParty.OH_Code = "STP" + new Random().Next(1000000).ToString();
			soldToParty.OH_RL_NKClosestPort = "USLAX";
			soldToParty.MainAddress.OA_Address1 = "228/45 FLOWER ST";
			soldToParty.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-12301458");
			declaration.Invoices[0].JZ_OA_SoldToPartyAddress = soldToParty.MainAddress.PK;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			var message = messageBuilder.PopulateMessage();

			AssertMultilineASCIIEquals(@"Expected Message:
			- for Consignee, only SE30 record required and only number should be sent, no name and address
			- blocks 30-36 should be generated for other entity types",
@"B  3902XJ5AE                                               <<MSGNO PLACEHOLDER>>
10ASV9  <E#PLCH> 3902            0110 XA          2073014                       
1191-01319900091-013199000                                  IL                  
20AAAA3902          APL EMERALD                                                 
SE13SUPPORT                                 123456                              
21V123W                                                                         
2200000012PK                                                                    
2200000011KG                                                                    
23HOTT1HOUSEBILL3                                                               
2200000130PCS                                                                   
23MOTT1TESTMB1                                                                  
23HOTT1TESTHB1                                                                  
23S    TESTSUBHB1                                                               
2200000012NN                                                                    
23MXXXWTESTMASTERBI                                                             
23HXXXYTESTHOUSEBIL                                                             
318B 891                                                                        
SE30ST MR IMPORTER                                                              
SE3515                                                                          
SE36                                                           US               
40  001 AUAU071814        0000000050602670000009000    N                        
44COMMERCIAL DESCRIPTION                                                        
47MTHLIATHA191NAK                                                               
47S12-12301458                                                                  
SE50MF MR MANUFACTURER                                                          
SE5515                                                                          
SE56                                                           AU               
504421909720 0000033000 0000010000 000000007000GR                               
6249900003464                                                                   
6250100001250                                                                   
894990000000346450100000001250                                                  
9000000033000 00000004714 00000000000 00000000000 00000000000 00000000000       
Y  3902XJ5AE", message.EM_FormattedMessageText);

			messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Replace);
			message = messageBuilder.PopulateMessage();
			AssertMultilineASCIIEquals("Replace Message",
@"B  3902XJ5AE                                               <<MSGNO PLACEHOLDER>>
10RSV9  <E#PLCH> 3902            0110 XA          2073014                       
1191-01319900091-013199000                                  IL                  
20AAAA3902          APL EMERALD                                                 
SE13SUPPORT                                 123456                              
21V123W                                                                         
2200000012PK                                                                    
2200000011KG                                                                    
23HOTT1HOUSEBILL3                                                               
2200000130PCS                                                                   
23MOTT1TESTMB1                                                                  
23HOTT1TESTHB1                                                                  
23S    TESTSUBHB1                                                               
2200000012NN                                                                    
23MXXXWTESTMASTERBI                                                             
23HXXXYTESTHOUSEBIL                                                             
318B 891                                                                        
SE30ST MR IMPORTER                                                              
SE3515                                                                          
SE36                                                           US               
40  001 AUAU071814        0000000050602670000009000    N                        
44COMMERCIAL DESCRIPTION                                                        
47MTHLIATHA191NAK                                                               
47S12-12301458                                                                  
SE50MF MR MANUFACTURER                                                          
SE5515                                                                          
SE56                                                           AU               
504421909720 0000033000 0000010000 000000007000GR                               
6249900003464                                                                   
6250100001250                                                                   
894990000000346450100000001250                                                  
9000000033000 00000004714 00000000000 00000000000 00000000000 00000000000       
Y  3902XJ5AE", message.EM_FormattedMessageText);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "2402108850";
			tariff.UE_DateFrom = ZDate.Today.AddMonths(-1);
			tariff.UE_DateTo = ZDate.Today.AddMonths(1);

			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			invoiceLine.US_PrivilegedStatusDate = ZDateTime.Today;
			invoiceLine.US_FTZCurrentTariff = "2402108850";

			messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Replace);
			message = messageBuilder.PopulateMessage();
			AssertMultilineASCIIEquals("Replace Message",
@"B  3902XJ5AE                                               <<MSGNO PLACEHOLDER>>
10RSV9  <E#PLCH> 3902            0610 XA          2073014                       
1191-01319900091-013199000                                  IL                  
20AAAA3902          APL EMERALD                                                 
SE13SUPPORT                                 123456                              
21V123W                                                                         
2200000012PK                                                                    
2200000011KG                                                                    
23HOTT1HOUSEBILL3                                                               
2200000130PCS                                                                   
23MOTT1TESTMB1                                                                  
23HOTT1TESTHB1                                                                  
23S    TESTSUBHB1                                                               
2200000012NN                                                                    
23MXXXWTESTMASTERBI                                                             
23HXXXYTESTHOUSEBIL                                                             
318B 891                                                                        
SE30ST MR IMPORTER                                                              
SE3515                                                                          
SE36                                                           US               
40  001 AUAU071814        0000000050602670000009000    N                        
41P0725140000000000                                                             
44COMMERCIAL DESCRIPTION                                                        
47MTHLIATHA191NAK                                                               
47S12-12301458                                                                  
SE50MF MR MANUFACTURER                                                          
SE5515                                                                          
SE56                                                           AU               
504421909720 0000033000 0000010000 000000007000GR                               
SE612402108850                                                                  
6249900003464                                                                   
6250100001250                                                                   
894990000000346450100000001250                                                  
9000000033000 00000004714 00000000000 00000000000 00000000000 00000000000       
Y  3902XJ5AE", message.EM_FormattedMessageText);
		}

		[TestDate(2014, 07, 25)]
		public void TestIncludeCargoReleaseDetails()
		{
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

			declaration.US_BondType2 = ZString.Empty;
			declaration.US_BondAmount2 = 0m;
			declaration.US_BondProducerAccNo2 = ZString.Empty;

			var invoiceLine1 = declaration.InvoiceLines[0];
			invoiceLine1.JI_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;
			invoiceLine1.JI_OA_ManufacturerAddress = address.PK;

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
			invoiceHeader2.JZ_OA_SoldToPartyAddress = header2_soldToParty.MainAddress.PK;

			var invoiceLine_Header2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine_Header2.JI_Tariff = "3920995000";
			invoiceLine_Header2.JI_OA_ConsigneeAddress = ultimateConsignee2.MainAddress.PK;
			invoiceLine_Header2.JI_OA_ManufacturerAddress = address2.PK;
			declaration.US_CargoReleaseType = CertificationOptionsList.Codes.ACE;
			declaration.AdditionalReferenceNumbers.AddNewIfNotExist(Common.UnitedStatesAdditionalReferenceNumberTypes.Codes.RRN, "12345678");

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			var message = messageBuilder.PopulateMessage();

			AssertMultilineASCIIEquals(@"Expected Message: 
			 - Other Organizations should be on line level, because they are different from Invoice Orgs",
@"B  3902XJ5AE                                               <<MSGNO PLACEHOLDER>>
10ASV9  <E#PLCH> 3902B00001000   0110 XA          2073014                       
1191-01319900091-013199000                                  IL                  
20AAAA3902          APL EMERALD                                                 
SE13SUPPORT                                 123456                              
21V123W                                                                         
2200000001PK                                                                    
318B 891                                                                        
SE20RRN12345678                                                                 
40  001 BHAU071814        0000000050602670000009000    N                        
44COMMERCIAL DESCRIPTION                                                        
47MBHNHJKMEQU6LFR                                                               
SE50MF MANUFACTURER FOR LINE 1                                                  
SE5515TEST MAN FOR LINE 1                                                       
SE56                                                           US               
SE50CN                                    ANI138888-12345                       
SE50ST MR IMPORTER                                                              
SE5515***ADDRESS NOT ON FILE***                                                 
SE56                                                           US               
504421909720 0000033000 0000010000 000000007000GR                               
6249900003464                                                                   
6250100001250                                                                   
40  002 FRAU071814        0000000000602670000000000    N                        
47MFR034FREQU6LBH                                                               
47S91-013199000                                                                 
SE50MF MANUFACTURER FOR LINE 2                                                  
SE5515TEST MAN FOR LINE 2                                                       
SE56                                                           US               
SE50CN                                       BBBBBB123                          
SE50ST MR IMPORTER                                                              
SE5515***ADDRESS NOT ON FILE***                                                 
SE56                                                           US               
503920995000 0000000000 0000000000 000000000000KG                               
6249900000000                                                                   
40  003 FRAU071814        0000000000602670000000000    N                        
47MFR034FREQU6LBH                                                               
SE50MF MANUFACTURER FOR LINE 2                                                  
SE5515TEST MAN FOR LINE 2                                                       
SE56                                                           US               
SE50CN                                       BBBBBB123                          
SE50SE SELLER FOR INV HEADER 2                                                  
SE5515123/33 STREET 1                                                           
SE56                                                           US               
SE50ST CONSIGNEE FOR INV HEADER 2                                               
SE5515***ADDRESS NOT ON FILE***                                                 
SE56                                                           US               
503920995000 0000000000 0000000000 000000000000KG                               
6249900000000                                                                   
894990000000346450100000001250                                                  
9000000033000 00000004714 00000000000 00000000000 00000000000 00000000000       
Y  3902XJ5AE", message.EM_FormattedMessageText);
		}

		[TestDate(2014, 07, 25)]
		public void TestIncludeCargoReleaseDetailsOnlyLineLevel()
		{
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

			var soldToParty = Factory.New<OrgHeader>();
			soldToParty.OH_FullName = "AUTO ELECTRICAL DISTRIBUTORS PTY LTD TEST TEST TES";
			soldToParty.OH_Code = "STP" + new Random().Next(1000000).ToString();
			soldToParty.OH_RL_NKClosestPort = "MXMEX";
			address = soldToParty.MainAddress;
			address.OA_Address1 = "UNIT 1, 210 ROBINSON ROAD GEEBUN DOWNTOWN FOR TEST";
			address.OA_Address2 = "GEEBUNG, QLD GEEBUN DOWNTOWN FOR TEST GEEBUN DOWNT";
			address.OA_City = "GEEBUN CITY FOR LENGHT TE";
			address.OA_PostCode = "4034654521";
			invoiceLine1.JI_OA_SoldToPartyAddress = soldToParty.MainAddress.PK;
			declaration.US_CargoReleaseType = CertificationOptionsList.Codes.ACE;
			declaration.US_BondType2 = ZString.Empty;
			declaration.US_BondAmount2 = 0m;
			declaration.US_BondProducerAccNo2 = ZString.Empty;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			var message = messageBuilder.PopulateMessage();

			AssertMultilineASCIIEquals(@"Expected Message: 
			 - Organizations should be on line level, because nothing on Invoice Header level",
@"B  3902XJ5AE                                               <<MSGNO PLACEHOLDER>>
10ASV9  <E#PLCH> 3902B00001000   0110 XA          2073014                       
1191-01319900091-013199000                                  IL                  
20AAAA3902          APL EMERALD                                                 
SE13SUPPORT                                 123456                              
21V123W                                                                         
2200000001PK                                                                    
318B 891                                                                        
SE30ST MR IMPORTER                                                              
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
40  001 BHAU071814        0000000050602670000009000    N                        
44COMMERCIAL DESCRIPTION                                                        
47MBHNHJKMEQU6LFR                                                               
SE50MF MANUFACTURER FOR LINE 1                                                  
SE5515TEST MAN FOR LINE 1                                                       
SE56                                                           US               
SE50CN                                    ANI138888-12345                       
504421909720 0000033000 0000010000 000000007000GR                               
6249900003464                                                                   
6250100001250                                                                   
894990000000346450100000001250                                                  
9000000033000 00000004714 00000000000 00000000000 00000000000 00000000000       
Y  3902XJ5AE", message.EM_FormattedMessageText);

			declaration.JE_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			message = messageBuilder.PopulateMessage();

			AssertMultilineASCIIEquals(@"Expected Message: 
			 - Organizations should be on line level, because nothing on Invoice Header level",
@"B  3902XJ5AE                                               <<MSGNO PLACEHOLDER>>
10ASV9  <E#PLCH> 3902B00001000   0110 XA          2073014                       
1191-013199000138888-12345                                  IL                  
20AAAA3902          APL EMERALD                                                 
SE13SUPPORT                                 123456                              
21V123W                                                                         
2200000001PK                                                                    
318B 891                                                                        
SE30ST ULTIMATECONSIGNEE1                                                       
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
40  001 BHAU071814        0000000050602670000009000    N                        
44COMMERCIAL DESCRIPTION                                                        
47MBHNHJKMEQU6LFR                                                               
SE50MF MANUFACTURER FOR LINE 1                                                  
SE5515TEST MAN FOR LINE 1                                                       
SE56                                                           US               
504421909720 0000033000 0000010000 000000007000GR                               
6249900003464                                                                   
6250100001250                                                                   
894990000000346450100000001250                                                  
9000000033000 00000004714 00000000000 00000000000 00000000000 00000000000       
Y  3902XJ5AE", message.EM_FormattedMessageText);

			var ultimateConsignee2 = Factory.New<OrgHeader>();
			ultimateConsignee2.OH_Code = "UC" + new Random().Next(1000000).ToString();
			ultimateConsignee2.OH_FullName = "ultimateConsignee2";
			ultimateConsignee2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "BBBBBB123");

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_OA_ConsigneeAddress = ultimateConsignee2.MainAddress.PK;
			invoiceLine2.JI_OA_ManufacturerAddress = address.PK;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			declaration.US_US_NKCentralizedExamSite = "A002";
			messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			message = messageBuilder.PopulateMessage();

			AssertMultilineASCIIEquals(@"Expected Message: 
			 - Organizations should be on line level, because nothing on Invoice Header level",
@"B  3902XJ5AE                                               <<MSGNO PLACEHOLDER>>
10ASV9  <E#PLCH> 3902B00001000   0110 XA          2073014                       
1191-013199000138888-12345                                  IL                  
20AAAA3902          APL EMERALD                                                 
SE13SUPPORT                                 123456                              
21V123W                                                                         
2200000001PK                                                                    
318B 891                                                                        
SE20CESA002                                                                     
SE30ST ULTIMATECONSIGNEE1                                                       
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
40  001 BHAU071814        0000000050602670000009000    N                        
44COMMERCIAL DESCRIPTION                                                        
47MBHNHJKMEQU6LFR                                                               
SE50MF MANUFACTURER FOR LINE 1                                                  
SE5515TEST MAN FOR LINE 1                                                       
SE56                                                           US               
SE50CN                                    ANI138888-12345                       
504421909720 0000033000 0000010000 000000007000GR                               
6249900003464                                                                   
6250100001250                                                                   
40  002 MXAU071814        0000000000602670000000000    N                        
47S91-013199000                                                                 
SE50MF AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE5515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE56GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE50CN                                       BBBBBB123                          
50           0000000000 0000000000                                              
6249900000000                                                                   
894990000000346450100000001250                                                  
9000000033000 00000004714 00000000000 00000000000 00000000000 00000000000       
Y  3902XJ5AE", message.EM_FormattedMessageText);

			declaration.JE_PrimaryITNumber = "1234567890";
			declaration.US_ITDate = ZDateTime.Today;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			message = messageBuilder.PopulateMessage();

			AssertMultilineASCIIEquals(@"Expected Message: - In-Bond Number/Date should be included",
@"B  3902XJ5AE                                               <<MSGNO PLACEHOLDER>>
10ASV9  <E#PLCH> 3902B00001000   0110 XA          2073014                       
1191-013199000138888-12345                                  IL                  
20AAAA3902          APL EMERALD                    072514                       
SE13SUPPORT                                 123456                              
21V123W                                                                         
2200000001PK                                                                    
23I    1234567890                                                               
318B 891                                                                        
SE20CESA002                                                                     
SE30ST ULTIMATECONSIGNEE1                                                       
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
40  001 BHAU071814        0000000050602670000009000    N                        
44COMMERCIAL DESCRIPTION                                                        
47MBHNHJKMEQU6LFR                                                               
SE50MF MANUFACTURER FOR LINE 1                                                  
SE5515TEST MAN FOR LINE 1                                                       
SE56                                                           US               
SE50CN                                    ANI138888-12345                       
504421909720 0000033000 0000010000 000000007000GR                               
6249900003464                                                                   
6250100001250                                                                   
40  002 MXAU071814        0000000000602670000000000    N                        
47S91-013199000                                                                 
SE50MF AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE5515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE56GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE50CN                                       BBBBBB123                          
50           0000000000 0000000000                                              
6249900000000                                                                   
894990000000346450100000001250                                                  
9000000033000 00000004714 00000000000 00000000000 00000000000 00000000000       
Y  3902XJ5AE", message.EM_FormattedMessageText);

			declaration.JE_MasterBill = "MB1234567";
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.US_NonAMS = true;
			messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			message = messageBuilder.PopulateMessage();
			AssertMultilineASCIIEquals(@"Expected Message: - Non-AMS should be populated in SE20",
@"B  3902XJ5AE                                               <<MSGNO PLACEHOLDER>>
10ASV9  <E#PLCH> 3902B00001000   0110 XA          2073014                       
1191-013199000138888-12345                                  IL                  
20AAAA3902          APL EMERALD                                                 
SE13SUPPORT                                 123456                              
21V123W                                                                         
2200000001PK                                                                    
23MAAAAMB1234567                                                                
SE16AAAAV123W      00000001PK                                                   
318B 891                                                                        
SE20CESA002                                                                     
SE20NAMY                                                                        
SE30ST ULTIMATECONSIGNEE1                                                       
SE3515***ADDRESS NOT ON FILE***                                                 
SE36                                                           US               
40  001 BHAU071814        0000000050602670000009000    N                        
44COMMERCIAL DESCRIPTION                                                        
47MBHNHJKMEQU6LFR                                                               
SE50MF MANUFACTURER FOR LINE 1                                                  
SE5515TEST MAN FOR LINE 1                                                       
SE56                                                           US               
SE50CN                                    ANI138888-12345                       
504421909720 0000033000 0000010000 000000007000GR                               
6249900003464                                                                   
6250100001250                                                                   
40  002 MXAU071814        0000000000602670000000000    N                        
47S91-013199000                                                                 
SE50MF AUTO ELECTRICAL DISTRIBUTORS PTY LT                                      
SE5515UNIT 1, 210 ROBINSON ROAD GEEBUN DO15GEEBUNG, QLD GEEBUN DOWNTOWN FOR TE  
SE56GEEBUN CITY FOR LENGHT TE                   4034654521     MX               
SE50CN                                       BBBBBB123                          
50           0000000000 0000000000                                              
6249900000000                                                                   
894990000000346450100000001250                                                  
9000000033000 00000004714 00000000000 00000000000 00000000000 00000000000       
Y  3902XJ5AE", message.EM_FormattedMessageText);
		}

		[TestDate(2015, 06, 24)]
		public void TestACS_FDADetails()
		{
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;
			declaration.US_CertifyCargoRelease = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACS;
			declaration.US_BondType2 = ZString.Empty;
			declaration.US_BondAmount2 = 0m;
			declaration.US_BondProducerAccNo2 = ZString.Empty;

			var ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.OH_Code = "UC" + new Random().Next(1000000).ToString();
			ultimateConsignee.OH_FullName = "ultimateConsignee1";
			var consigneeAddress = ultimateConsignee.Addresses.AddNew();
			consigneeAddress.OA_Address1 = "TestConsignee1";
			ultimateConsignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "138888-12345");

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_FullName = "Manufacturer For Line 1";
			manufacturer.OH_Code = "MAN" + new Random().Next(1000000).ToString();
			var address = manufacturer.Addresses.AddNew();
			address.OA_Address1 = "Test man for line 1";
			address.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "BHNHJKMEQU6LFR");

			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.JI_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;
			invoiceLine.JI_OA_ShipToPartyAddress = consigneeAddress.PK;
			invoiceLine.JI_OA_ManufacturerAddress = address.PK;
			invoiceLine.JI_OA_ExporterAddress = address.PK;
			invoiceLine.JI_Description = "MEDICAL";
			invoiceLine.JI_LinePrice = 1500m;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;

			var fda = invoiceLine.FDAs.AddNew();
			fda.US_FDAProductCode = "AAui789";
			fda.US_FDAQty1 = 16m;
			fda.US_FDAMeasure1 = "BX";
			fda.US_FDACommercialDesc = "TEST MEDICAL DEVICE";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			var message = messageBuilder.PopulateMessage();

			AssertMultilineASCIIEquals(@"Expected Message: no duplicate OI record",
@"B  3902XJ5AE                                               <<MSGNO PLACEHOLDER>>
10ASV9  <E#PLCH> 3902B00001000   0110 XY          2062915                       
1191-01319900091-013199000                                  IL                  
20AAAA3902          APL EMERALD                                                 
21V123W                                                                         
2200000001PK                                                                    
318B 891                                                                        
40  001 BHAU061715        0000000050602670000009000    N                        
44MEDICAL                                                                       
47MBHNHJKMEQU6LFR                                                               
47C138888-12345                                                                 
47S91-013199000                                                                 
47EBHNHJKMEQU6LFR                                                               
504421909720 0000004950 0000001500 000000007000GR                               
OI        TEST MEDICAL DEVICE                                                   
FD01001AAUI789                                 BHNHJKMEQU6LFR THLIATHA191NAK    
FD020000001600BX                                                                
FD030000001500                                                                  
6249900000520                                                                   
6250100000188                                                                   
894990000000250050100000000188                                                  
9000000004950 00000002688 00000000000 00000000000 00000000000 00000000000       
Y  3902XJ5AE", message.EM_FormattedMessageText);
		}

		public void TestCertifyTemporaryImportationBond()
		{
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			declaration.US_SchDEntry = "2704";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true);
			var message = messageBuilder.PopulateMessage();

			var ens10 = message.MessageBlock.MessageBlocks.OfType<AENS10>().FirstOrDefault();
			AssertNotNull(ens10);
			AssertEquals("Y", ens10.TIBDeclarationIndicator);
		}

		public void TestSE20forCurrentLocationForEntryType21()
		{
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.FIRMSCode, "ABCD");
			declaration.WarehouseDocAddress.E2_OA_Address = manufacturer.MainAddress.PK;
			declaration.US_US_NKLocationOfGoods = "EFGD";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			var message = messageBuilder.PopulateMessage();
			var se20s = message.MessageBlock.MessageBlocks.OfType<ASESE20>();
			var se20 = se20s.FirstOrDefault(x => x.ReferenceIdentifierQualifier == ReferenceIdentifierCodeList.Codes.CurrentGoodsLocation);
			AssertEquals("EFGD", se20.ReferenceIdentifier);
		}

		public void TestSE20forGeneralOrderNumber()
		{
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_GeneralOrderNo = "123456789";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			var message = messageBuilder.PopulateMessage();
			var se20s = message.MessageBlock.MessageBlocks.OfType<ASESE20>();
			var se20 = se20s.FirstOrDefault(x => x.ReferenceIdentifierQualifier == ReferenceIdentifierCodeList.Codes.GeneralOrderNumber);
			AssertEquals("123456789", se20.ReferenceIdentifier);
		}

		public void TestSoftwoodLumberExportPriceAndCharges()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "44091020";
			tariff.UE_PermitLicenseIndicator = MiscellaneousPermitLicenseList.Codes.CanadaSoftwoodLumberExportNumber;
			tariff.UE_DateFrom = ZDateTime.Today;
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);

			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Empty;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			invoiceLine.US_LumberImporterDeclaration = YesNoDefaultList.Codes.Yes;
			invoiceLine.JI_Tariff = "44091020";
			invoiceLine.US_LumberExportPrice = 1000m;
			invoiceLine.US_LumberExportCharges = 2000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			var message = messageBuilder.PopulateMessage();
			var ens54Block = message.MessageBlock.MessageBlocks.OfType<AENS54>().FirstOrDefault();
			AssertEquals("5401Y00000010000000002000                                                       ", ens54Block.Serialise());
		}

		[TestDate(2015, 05, 15)]
		public void TestLaceyActExample1_1ProductManySpecies()
		{
			/*
			 Example 1 is an entry of used railroad ties with only one CBP line item. 
			 Each tie is made of maple or oak.  There are 100 kg of Acer saccharum from Canada, 
			 200 kg of Quercus rubra from Canada, 300 kg of Quercus alba from Canada, 
			 400 kg of Acer saccharum from the U.S., 500 kg of Quercus rubra from the U.S., 
			 and 600 kg of Quercus alba from the U.S. The value of the PGA line is $10,000, and the shipment is in two containers. */

			SetUpDataForLaceyAct();

			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "WHXU4101474", true);
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "WHXU4101475", true);

			invoiceLine.JI_Description = "USED RAILROAD TIES";

			var laceyAct = invoiceLine.LaceyActLines.AddNew();
			laceyAct.US_PGACommercialDescription = "USED MAPLE AND OAK RAILROAD TIES";
			laceyAct.US_InvCurrPGAValue = 10000m;
			laceyAct.US_PGALineValue = 10000m;
			laceyAct.US_UnknownBreakdown = true;
			laceyAct.US_CertifyingIndividual = PartyTypeList.Codes.Importer;

			DeclarationTestHelper.AssociateContainerWithLaceyActLine(laceyAct, "WHXU4101474");
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(laceyAct, "WHXU4101475");

			var constituentElement = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement.US_PGANameOfTheConstituentElement = "Maple Ties";
			constituentElement.US_PGAQuantityOfConstituentElement = 100m;
			constituentElement.US_PGAUnitOfMeasure = "KG";
			constituentElement.US_GenusName = "ACER SACCHARUM";
			constituentElement.US_SpeciesName = "SPF";
			constituentElement.US_UnknownBreakdownCountryCode = "CA";

			var constituentElement2 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement2.US_PGANameOfTheConstituentElement = "Oak Ties";
			constituentElement2.US_PGAQuantityOfConstituentElement = 200m;
			constituentElement2.US_PGAUnitOfMeasure = "KG";
			constituentElement2.US_GenusName = "QUERCUS RUBRA";
			constituentElement2.US_SpeciesName = "SPF";
			constituentElement2.US_UnknownBreakdownCountryCode = "CA";

			var constituentElement3 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement3.US_PGANameOfTheConstituentElement = "Oak Ties";
			constituentElement3.US_PGAQuantityOfConstituentElement = 300m;
			constituentElement3.US_PGAUnitOfMeasure = "KG";
			constituentElement3.US_GenusName = "QUERCUS ALBA";
			constituentElement3.US_SpeciesName = "SPF";
			constituentElement3.US_UnknownBreakdownCountryCode = "CA";

			var constituentElement4 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement4.US_PGANameOfTheConstituentElement = "Maple Ties";
			constituentElement4.US_PGAQuantityOfConstituentElement = 400m;
			constituentElement4.US_PGAUnitOfMeasure = "KG";
			constituentElement4.US_GenusName = "ACER SACCHARUM";
			constituentElement4.US_SpeciesName = "SPF";
			constituentElement4.US_UnknownBreakdownCountryCode = "US";

			var constituentElement5 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement5.US_PGANameOfTheConstituentElement = "Oak Ties";
			constituentElement5.US_PGAQuantityOfConstituentElement = 500m;
			constituentElement5.US_PGAUnitOfMeasure = "KG";
			constituentElement5.US_GenusName = "QUERCUS RUBRA";
			constituentElement5.US_SpeciesName = "SPF";
			constituentElement5.US_UnknownBreakdownCountryCode = "US";

			var constituentElement6 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement6.US_PGANameOfTheConstituentElement = "Oak Ties";
			constituentElement6.US_PGAQuantityOfConstituentElement = 600m;
			constituentElement6.US_PGAUnitOfMeasure = "KG";
			constituentElement6.US_GenusName = "QUERCUS ALBA";
			constituentElement6.US_SpeciesName = "SPF";
			constituentElement6.US_UnknownBreakdownCountryCode = "US";
			laceyAct.US_CertifyingIndividual = PartyTypeList.Codes.Importer;

			MergeAndSend("LCY1", true, false);

			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];

			AssertContains(
@"OI        USED RAILROAD TIES                                                    
PG01001APHAPL                                                                   
PG02P                                                                           
PG10                   USED MAPLE AND OAK RAILROAD TIES                         
PG04YMAPLE TIES                                         000000010000KG          
PG05ACER SACCHARUM        SPF                                                   
PG06HRVCA                                                                       
PG04YOAK TIES                                           000000020000KG          
PG05QUERCUS RUBRA         SPF                                                   
PG06HRVCA                                                                       
PG04YOAK TIES                                           000000030000KG          
PG05QUERCUS ALBA          SPF                                                   
PG06HRVCA                                                                       
PG04YMAPLE TIES                                         000000040000KG          
PG05ACER SACCHARUM        SPF                                                   
PG06HRVUS                                                                       
PG04YOAK TIES                                           000000050000KG          
PG05QUERCUS RUBRA         SPF                                                   
PG06HRVUS                                                                       
PG04YOAK TIES                                           000000060000KG          
PG05QUERCUS ALBA          SPF                                                   
PG06HRVUS                                                                       
PG19IM                                                   IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21IM JOHN Q IMPORTER        3015552232     JQI@IMP.COM                        
PG22             IM AP6 Y05152015                                               
PG25                                                    000000010000            
PG27WHXU4101474            WHXU4101475                                          ", message.EM_FormattedMessageText);
		}

		[TestDate(2015, 05, 15)]
		public void TestLaceyActExample2_3Products1Species()
		{
			/*
			Example 2 is an entry of pine desks with only one CBP line item and has three PGA line items representing 3 different styles of desk.
			All three PGA lines are under the same OI record (same commercial description).  
			1)	First PGA line are desks made of pine (Pinus taeda), with a quantity is 100 cubic meters, harvested in Canada, and valued at $10,000.
			2)    Second PGA line are desks made of pine (Pinus taeda), with a quantity is 200 cubic meters, harvested in Canada, and valued at $20,000.
			3)   Third PGA line are desks made of pine (Pinus taeda), valued at $30,000, with a quantity of 300 cubic meters harvested 
				in the United States and 400 cubic meters harvested in the Canada. 
			*/

			SetUpDataForLaceyAct();
			invoiceLine.JI_LinePrice = 60000m;
			invoiceLine.JI_Description = "Wooden Desk";

			var laceyAct = invoiceLine.LaceyActLines.AddNew();
			laceyAct.US_PGACommercialDescription = "Wooden Desk Model 0415";
			laceyAct.US_InvCurrPGAValue = 10000m;
			laceyAct.US_PGALineValue = 10000m;
			laceyAct.US_UnknownBreakdown = true;
			laceyAct.US_CertifyingIndividual = PartyTypeList.Codes.Importer;

			var constituentElement = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement.US_PGANameOfTheConstituentElement = "Pine Desk Tops Legs Side Drawers";
			constituentElement.US_PGAQuantityOfConstituentElement = 100m;
			constituentElement.US_PGAUnitOfMeasure = "M3";
			constituentElement.US_GenusName = "Pinus Taeda";
			constituentElement.US_SpeciesName = "SPF";
			constituentElement.US_UnknownBreakdownCountryCode = "CA";

			var laceyAct2 = invoiceLine.LaceyActLines.AddNew();
			laceyAct2.US_PGACommercialDescription = "Wooden Desk Model 0522";
			laceyAct2.US_InvCurrPGAValue = 20000m;
			laceyAct2.US_PGALineValue = 20000m;
			laceyAct2.US_CertifyingIndividual = PartyTypeList.Codes.Importer;

			var constituentElement2 = laceyAct2.PG04ConstituentElements.AddNew();
			constituentElement2.US_PGANameOfTheConstituentElement = "Pine Desk Tops Legs Side Drawers";
			constituentElement2.US_PGAQuantityOfConstituentElement = 200m;
			constituentElement2.US_PGAUnitOfMeasure = "M3";
			constituentElement2.US_GenusName = "Pinus Taeda";
			constituentElement2.US_SpeciesName = "SPF";
			constituentElement2.US_UnknownBreakdownCountryCode = "CA";

			var laceyAct3 = invoiceLine.LaceyActLines.AddNew();
			laceyAct3.US_PGACommercialDescription = "Pine Desk Model 0553";
			laceyAct3.US_InvCurrPGAValue = 30000m;
			laceyAct3.US_PGALineValue = 30000m;
			laceyAct3.US_CertifyingIndividual = PartyTypeList.Codes.Importer;

			var constituentElement3 = laceyAct3.PG04ConstituentElements.AddNew();
			constituentElement3.US_PGANameOfTheConstituentElement = "Pine Desk Tops Drawers";
			constituentElement3.US_PGAQuantityOfConstituentElement = 300m;
			constituentElement3.US_PGAUnitOfMeasure = "M3";
			constituentElement3.US_GenusName = "Pinus Taeda";
			constituentElement3.US_SpeciesName = "SPF";
			constituentElement3.US_UnknownBreakdownCountryCode = "US";

			var constituentElement4 = laceyAct3.PG04ConstituentElements.AddNew();
			constituentElement4.US_PGANameOfTheConstituentElement = "Pine Desk Tops Legs Side";
			constituentElement4.US_PGAQuantityOfConstituentElement = 400m;
			constituentElement4.US_PGAUnitOfMeasure = "M3";
			constituentElement4.US_GenusName = "Pinus Taeda";
			constituentElement4.US_SpeciesName = "SPF";
			constituentElement4.US_UnknownBreakdownCountryCode = "CA";
			invoiceHeader.US_LACEYACTSignDate = ZDateTime.Today;

			MergeAndSend("LCY1", true, false);

			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
			AssertContains(
@"OI        WOODEN DESK                                                           
PG01001APHAPL                                                                   
PG02P                                                                           
PG10                   WOODEN DESK MODEL 0415                                   
PG04YPINE DESK TOPS LEGS SIDE DRAWERS                   000000010000M3          
PG05PINUS TAEDA           SPF                                                   
PG06HRVCA                                                                       
PG19IM                                                   IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21IM JOHN Q IMPORTER        3015552232     JQI@IMP.COM                        
PG22             IM AP6 Y05152015                                               
PG25                                                    000000010000            
PG01002APHAPL                                                                   
PG02P                                                                           
PG10                   WOODEN DESK MODEL 0522                                   
PG04 PINE DESK TOPS LEGS SIDE DRAWERS                   000000020000M3          
PG05PINUS TAEDA           SPF                                                   
PG06HRVCA                                                                       
PG19IM                                                   IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21IM JOHN Q IMPORTER        3015552232     JQI@IMP.COM                        
PG22             IM AP6 Y05152015                                               
PG25                                                    000000020000            
PG01003APHAPL                                                                   
PG02P                                                                           
PG10                   PINE DESK MODEL 0553                                     
PG04 PINE DESK TOPS DRAWERS                             000000030000M3          
PG05PINUS TAEDA           SPF                                                   
PG06HRVUS                                                                       
PG04 PINE DESK TOPS LEGS SIDE                           000000040000M3          
PG05PINUS TAEDA           SPF                                                   
PG06HRVCA                                                                       
PG19IM                                                   IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21IM JOHN Q IMPORTER        3015552232     JQI@IMP.COM                        
PG22             IM AP6 Y05152015                                               
PG25                                                    000000030000            ", message.EM_FormattedMessageText);
		}

		[TestDate(2015, 05, 15)]
		public void TestLaceyActExample3_UnknownSpeciesBreakdown()
		{
			/*
			This sample is not common and requires the exercise of due care to determine applicability.

			Example 3 is an entry of softwood lumber made of three species of pine with a quantity of 300 cubic meters.* 
			After exercising due care the importer cannot determine specifically how much of each species exists 
			in this shipment, however, they know that 100 M3 was harvested in the United Kingdom and 200 M3 was harvested in France.
			The shipment is valued at $30,000 and is shipped in one container.

			*The statute requires a unique quantity for each species/country pairing. */

			SetUpDataForLaceyAct();
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "WHXU4101474", true);
			invoiceLine.JI_Description = "Softwood Pulpwood";

			var laceyAct = invoiceLine.LaceyActLines.AddNew();
			laceyAct.US_PGACommercialDescription = "European Union Pine Pulpwood";
			laceyAct.US_InvCurrPGAValue = 30000m;
			laceyAct.US_PGALineValue = 30000m;
			laceyAct.US_UnknownBreakdown = true;
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(laceyAct, "WHXU4101474");

			var constituentElement = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement.US_PGANameOfTheConstituentElement = "UK Pine Pulpwood";
			constituentElement.US_PGAQuantityOfConstituentElement = 33m;
			constituentElement.US_PGAUnitOfMeasure = "KG";
			constituentElement.US_GenusName = "Pinus Taeda";
			constituentElement.US_SpeciesName = "SPF";
			constituentElement.US_UnknownBreakdownCountryCode = "GB";

			var constituentElement2 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement2.US_PGANameOfTheConstituentElement = "UK Pine Pulpwood";
			constituentElement2.US_PGAQuantityOfConstituentElement = 33m;
			constituentElement2.US_PGAUnitOfMeasure = "KG";
			constituentElement2.US_GenusName = "Pinus Rigida";
			constituentElement2.US_SpeciesName = "SPF";
			constituentElement2.US_UnknownBreakdownCountryCode = "GB";

			var constituentElement3 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement3.US_PGANameOfTheConstituentElement = "UK Pine Pulpwood";
			constituentElement3.US_PGAQuantityOfConstituentElement = 34m;
			constituentElement3.US_PGAUnitOfMeasure = "KG";
			constituentElement3.US_GenusName = "Pinus Echinada";
			constituentElement3.US_SpeciesName = "SPF";
			constituentElement3.US_UnknownBreakdownCountryCode = "GB";

			var constituentElement4 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement4.US_PGANameOfTheConstituentElement = "French Pine Pulpwood";
			constituentElement4.US_PGAQuantityOfConstituentElement = 33m;
			constituentElement4.US_PGAUnitOfMeasure = "KG";
			constituentElement4.US_GenusName = "Pinus Taeda";
			constituentElement4.US_SpeciesName = "SPF";
			constituentElement4.US_UnknownBreakdownCountryCode = "FR";

			var constituentElement5 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement5.US_PGANameOfTheConstituentElement = "French Pine Pulpwood";
			constituentElement5.US_PGAQuantityOfConstituentElement = 33m;
			constituentElement5.US_PGAUnitOfMeasure = "KG";
			constituentElement5.US_GenusName = "Pinus Rigida";
			constituentElement5.US_SpeciesName = "SPF";
			constituentElement5.US_UnknownBreakdownCountryCode = "FR";

			var constituentElement6 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement6.US_PGANameOfTheConstituentElement = "French Pine Pulpwood";
			constituentElement6.US_PGAQuantityOfConstituentElement = 34m;
			constituentElement6.US_PGAUnitOfMeasure = "KG";
			constituentElement6.US_GenusName = "Pinus Echinada";
			constituentElement6.US_SpeciesName = "SPF";
			constituentElement6.US_UnknownBreakdownCountryCode = "FR";

			MergeAndSend("LCY1", true, true);

			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
			AssertContains(
@"OI        SOFTWOOD PULPWOOD                                                     
PG01001APHAPL                                                                   
PG02P                                                                           
PG10                   EUROPEAN UNION PINE PULPWOOD                             
PG04YUK PINE PULPWOOD                                   000000003300KG          
PG05PINUS TAEDA           SPF                                                   
PG06HRVGB                                                                       
PG04YUK PINE PULPWOOD                                   000000003300KG          
PG05PINUS RIGIDA          SPF                                                   
PG06HRVGB                                                                       
PG04YUK PINE PULPWOOD                                   000000003400KG          
PG05PINUS ECHINADA        SPF                                                   
PG06HRVGB                                                                       
PG04YFRENCH PINE PULPWOOD                               000000003300KG          
PG05PINUS TAEDA           SPF                                                   
PG06HRVFR                                                                       
PG04YFRENCH PINE PULPWOOD                               000000003300KG          
PG05PINUS RIGIDA          SPF                                                   
PG06HRVFR                                                                       
PG04YFRENCH PINE PULPWOOD                               000000003400KG          
PG05PINUS ECHINADA        SPF                                                   
PG06HRVFR                                                                       
PG19IM                                                   IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21IM JOHN Q IMPORTER        3015552232     JQI@IMP.COM                        
PG22             IM AP6 Y05152015                                               
PG25                                                    000000030000            ", message.EM_FormattedMessageText);
		}

		[TestDate(2015, 05, 15)]
		public void TestLaceyActExample3a_UnknownSpeciesBreakdown()
		{
			/*
			This sample is not common and requires the exercise of due care to determine applicability.
			
			Example 3 is an entry of softwood lumber made of three species of pine with a quantity of 300 cubic meters.* 
			After exercising due care the importer cannot determine specifically how much of each species exists in this 
			shipment, however, they know that 100 M3 was harvested in the United Kingdom and 200 M3 was harvested in France. 
			The shipment is valued at $30,000 and is shipped in one container. This example provides the use of multiple PG05 for one PG06.
			
			*The statute requires a unique quantity for each species/country pairing. */

			SetUpDataForLaceyAct();
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "WHXU4101474", true);
			invoiceLine.JI_Description = "Softwood Pulpwood";

			var laceyAct = invoiceLine.LaceyActLines.AddNew();
			laceyAct.US_PGACommercialDescription = "European Union Pine Pulpwood";
			laceyAct.US_InvCurrPGAValue = 30000m;
			laceyAct.US_PGALineValue = 30000m;
			laceyAct.US_UnknownBreakdown = true;
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(laceyAct, "WHXU4101474");

			var constituentElement = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement.US_PGANameOfTheConstituentElement = "UK Pine Pulpwood";
			constituentElement.US_PGAQuantityOfConstituentElement = 100m;
			constituentElement.US_PGAUnitOfMeasure = "KG";
			constituentElement.US_UnknownBreakdownCountryCode = "GB";
			constituentElement.US_GenusName = "Pinus Taeda";
			constituentElement.US_SpeciesName = "Recycled";

			var constituentElement2 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement2.US_PGANameOfTheConstituentElement = "UK Pine Pulpwood";
			constituentElement2.US_PGAQuantityOfConstituentElement = 100m;
			constituentElement2.US_PGAUnitOfMeasure = "KG";
			constituentElement2.US_UnknownBreakdownCountryCode = "GB";
			constituentElement2.US_GenusName = "Pinus Rigida";
			constituentElement2.US_SpeciesName = "Recycled";

			var constituentElement3 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement3.US_PGANameOfTheConstituentElement = "UK Pine Pulpwood";
			constituentElement3.US_PGAQuantityOfConstituentElement = 100m;
			constituentElement3.US_PGAUnitOfMeasure = "KG";
			constituentElement3.US_UnknownBreakdownCountryCode = "GB";
			constituentElement3.US_GenusName = "Pinus Echinada";
			constituentElement3.US_SpeciesName = "Recycled";

			var constituentElement4 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement4.US_PGANameOfTheConstituentElement = "French Pine Pulpwood";
			constituentElement4.US_PGAQuantityOfConstituentElement = 200m;
			constituentElement4.US_PGAUnitOfMeasure = "KG";
			constituentElement4.US_UnknownBreakdownCountryCode = "FR";
			constituentElement4.US_GenusName = "Pinus Taeda";
			constituentElement4.US_SpeciesName = "Recycled";

			var constituentElement5 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement5.US_PGANameOfTheConstituentElement = "French Pine Pulpwood";
			constituentElement5.US_PGAQuantityOfConstituentElement = 200m;
			constituentElement5.US_PGAUnitOfMeasure = "KG";
			constituentElement5.US_UnknownBreakdownCountryCode = "FR";
			constituentElement5.US_GenusName = "Pinus Rigida";
			constituentElement5.US_SpeciesName = "Recycled";

			var constituentElement6 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement6.US_PGANameOfTheConstituentElement = "French Pine Pulpwood";
			constituentElement6.US_PGAQuantityOfConstituentElement = 200m;
			constituentElement6.US_PGAUnitOfMeasure = "KG";
			constituentElement6.US_UnknownBreakdownCountryCode = "FR";
			constituentElement6.US_GenusName = "Pinus Echinada";
			constituentElement6.US_SpeciesName = "Recycled";

			MergeAndSend("LCY3a", true, true);

			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
			AssertContains(
@"OI        SOFTWOOD PULPWOOD                                                     
PG01001APHAPL                                                                   
PG02P                                                                           
PG10                   EUROPEAN UNION PINE PULPWOOD                             
PG04YUK PINE PULPWOOD                                   000000010000KG          
PG05PINUS TAEDA           RECYCLED                                              
PG06HRVGB                                                                       
PG04YUK PINE PULPWOOD                                   000000010000KG          
PG05PINUS RIGIDA          RECYCLED                                              
PG06HRVGB                                                                       
PG04YUK PINE PULPWOOD                                   000000010000KG          
PG05PINUS ECHINADA        RECYCLED                                              
PG06HRVGB                                                                       
PG04YFRENCH PINE PULPWOOD                               000000020000KG          
PG05PINUS TAEDA           RECYCLED                                              
PG06HRVFR                                                                       
PG04YFRENCH PINE PULPWOOD                               000000020000KG          
PG05PINUS RIGIDA          RECYCLED                                              
PG06HRVFR                                                                       
PG04YFRENCH PINE PULPWOOD                               000000020000KG          
PG05PINUS ECHINADA        RECYCLED                                              
PG06HRVFR                                                                       
PG19IM                                                   IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21IM JOHN Q IMPORTER        3015552232     JQI@IMP.COM                        
PG22             IM AP6 Y05152015                                               
PG25                                                    000000030000            
PG27WHXU4101474                                                                 ", message.EM_FormattedMessageText);
		}

		[TestDate(2015, 05, 15)]
		public void TestLaceyActExample4a_UnknownCountryBreakdown()
		{
			/*
			This Sample is not common and requires the exercise of due care to determine applicability.

			*The statute requires a unique quantity for each species/country pairing. 
			APHIS guidance states that in the case where the country is unknown (after the exercise of due care) 
			and the total number of possible countries is 10 or more, then the filer may use “**” in lieu of the Country of Harvest.

			Example 4a is an entry of softwood lumber with only one CBP line item and has two PGA line items.
			Both PGA lines are under the same OI record (same commercial description).  

			This line is lumber made of a species of pine, quantity is 200 cubic meters divided by the three possible 
			countries of harvest, each will be assigned a quantity of 66 M3, (after exercising due care the importer 
			cannot determine specifically how much of each species was harvested in each country). 
			The line is valued at $20,000, and is in one container.	*/

			SetUpDataForLaceyAct();
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "WHXU4101474", true);
			invoiceLine.JI_Description = "Softwood Pulpwood";

			var laceyAct = invoiceLine.LaceyActLines.AddNew();
			laceyAct.US_PGACommercialDescription = "European Union Softwood Pulpwood";
			laceyAct.US_InvCurrPGAValue = 20000m;
			laceyAct.US_PGALineValue = 20000m;
			laceyAct.US_UnknownBreakdown = true;
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(laceyAct, "WHXU4101474");

			var constituentElement = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement.US_PGANameOfTheConstituentElement = "Pine Pulpwood Unknown Country Breakdown";
			constituentElement.US_PGAQuantityOfConstituentElement = 66m;
			constituentElement.US_PGAUnitOfMeasure = "M3";
			constituentElement.US_UnknownBreakdownCountryCode = "GB";
			constituentElement.US_GenusName = "Pinus Echinada";
			constituentElement.US_SpeciesName = "Recycled";

			var constituentElement2 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement2.US_PGANameOfTheConstituentElement = "Pine Pulpwood Unknown Country Breakdown";
			constituentElement2.US_PGAQuantityOfConstituentElement = 66m;
			constituentElement2.US_PGAUnitOfMeasure = "M3";
			constituentElement2.US_UnknownBreakdownCountryCode = "FR";
			constituentElement2.US_GenusName = "Pinus Echinada";
			constituentElement2.US_SpeciesName = "Recycled";

			var constituentElement3 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement3.US_PGANameOfTheConstituentElement = "Pine Pulpwood Unknown Country Breakdown";
			constituentElement3.US_PGAQuantityOfConstituentElement = 66m;
			constituentElement3.US_PGAUnitOfMeasure = "M3";
			constituentElement3.US_UnknownBreakdownCountryCode = "DE";
			constituentElement3.US_GenusName = "Pinus Echinada";
			constituentElement3.US_SpeciesName = "Recycled";

			MergeAndSend("LCY1", true, true);

			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
			AssertContains(
@"OI        SOFTWOOD PULPWOOD                                                     
PG01001APHAPL                                                                   
PG02P                                                                           
PG10                   EUROPEAN UNION SOFTWOOD PULPWOOD                         
PG04YPINE PULPWOOD UNKNOWN COUNTRY BREAKDOWN            000000006600M3          
PG05PINUS ECHINADA        RECYCLED                                              
PG06HRVGB                                                                       
PG04YPINE PULPWOOD UNKNOWN COUNTRY BREAKDOWN            000000006600M3          
PG05PINUS ECHINADA        RECYCLED                                              
PG06HRVFR                                                                       
PG04YPINE PULPWOOD UNKNOWN COUNTRY BREAKDOWN            000000006600M3          
PG05PINUS ECHINADA        RECYCLED                                              
PG06HRVDE                                                                       
PG19IM                                                   IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21IM JOHN Q IMPORTER        3015552232     JQI@IMP.COM                        
PG22             IM AP6 Y05152015                                               
PG25                                                    000000020000            
PG27WHXU4101474                                                                 ", message.EM_FormattedMessageText);
		}

		[TestDate(2015, 05, 15)]
		public void TestLaceyActExample4b_UnknownCountryBreakdown()
		{
			/*
			This Sample is not common and requires the exercise of due care to determine applicability.

			*The statute requires a unique quantity for each species/country pairing. 
			APHIS guidance states that in the case where the country is unknown (after the exercise of due care) 
			and the total number of possible countries is 10 or more, then the filer may use “**” in lieu of the Country of Harvest.

			Example 4b is an entry of softwood lumber with only one CBP line item and has two PGA line items.
			Both PGA lines are under the same OI record (same commercial description).  

			This line is lumber made of three species of pine (100 m3 each); it is unknown* (after exercising due care)
			exactly where the pine was harvested. The country of harvest could have been one of eleven countries. 
			This shipment is valued at $30,000, and shipped in one container.
			*/

			SetUpDataForLaceyAct();
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "WHXU4101474", true);
			invoiceLine.JI_Description = "Softwood Pulpwood";

			var laceyAct = invoiceLine.LaceyActLines.AddNew();
			laceyAct.US_PGACommercialDescription = "European Union Softwood Pulpwood";
			laceyAct.US_InvCurrPGAValue = 30000m;
			laceyAct.US_PGALineValue = 30000m;
			laceyAct.US_UnknownBreakdown = true;
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(laceyAct, "WHXU4101474");

			var constituentElement = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement.US_PGANameOfTheConstituentElement = "Pine Pulpwood Unknown Country";
			constituentElement.US_PGAQuantityOfConstituentElement = 100m;
			constituentElement.US_PGAUnitOfMeasure = "M3";
			constituentElement.US_UnknownBreakdownCountryCode = "**";
			constituentElement.US_GenusName = "Pinus Echinada";
			constituentElement.US_SpeciesName = "PreAmendment";

			var constituentElement2 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement2.US_PGANameOfTheConstituentElement = "Pine Pulpwood Unknown Country";
			constituentElement2.US_PGAQuantityOfConstituentElement = 100m;
			constituentElement2.US_PGAUnitOfMeasure = "M3";
			constituentElement2.US_UnknownBreakdownCountryCode = "**";
			constituentElement2.US_GenusName = "Pinus Taeda";
			constituentElement2.US_SpeciesName = "PreAmendment";

			var constituentElement3 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement3.US_PGANameOfTheConstituentElement = "Pine Pulpwood Unknown Country";
			constituentElement3.US_PGAQuantityOfConstituentElement = 100m;
			constituentElement3.US_PGAUnitOfMeasure = "M3";
			constituentElement3.US_UnknownBreakdownCountryCode = "**";
			constituentElement3.US_GenusName = "Pinus Rigida";
			constituentElement3.US_SpeciesName = "PreAmendment";

			MergeAndSend("LCY4b", true, true);

			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
			AssertContains(
@"OI        SOFTWOOD PULPWOOD                                                     
PG01001APHAPL                                                                   
PG02P                                                                           
PG10                   EUROPEAN UNION SOFTWOOD PULPWOOD                         
PG04YPINE PULPWOOD UNKNOWN COUNTRY                      000000010000M3          
PG05PINUS ECHINADA        PREAMENDMENT                                          
PG06HRV**                                                                       
PG04YPINE PULPWOOD UNKNOWN COUNTRY                      000000010000M3          
PG05PINUS TAEDA           PREAMENDMENT                                          
PG06HRV**                                                                       
PG04YPINE PULPWOOD UNKNOWN COUNTRY                      000000010000M3          
PG05PINUS RIGIDA          PREAMENDMENT                                          
PG06HRV**                                                                       
PG19IM                                                   IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21IM JOHN Q IMPORTER        3015552232     JQI@IMP.COM                        
PG22             IM AP6 Y05152015                                               
PG25                                                    000000030000            
PG27WHXU4101474                                                                 ", message.EM_FormattedMessageText);
		}

		[TestDate(2015, 05, 15)]
		public void TestLaceyActExample5_UnknownCountryAndSpeciesBreakdown()
		{
			/*
			This Sample is not common and requires the exercise of due care to determine applicability.

			Example 5 is an entry of softwood lumber with only one CBP line item and has three PGA line items. 
			The PGA line is lumber made of three species of pine; it is unknown (after exercising due care) 
			how much of the 300 cubic meters of pine represents each of the three species, though they were 
			each harvested in Canada and United States. The quantity is divided between the PG06, Country of Harvest, 
			multiple PG06 may follow a PG05. This shipment is valued at $30,000, and shipped in one container.

			*The statute requires a unique quantity for each species/country pairing.
			*/

			SetUpDataForLaceyAct();
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "WHXU4101474", true);
			invoiceLine.JI_Description = "Softwood Pulpwood";

			var laceyAct = invoiceLine.LaceyActLines.AddNew();
			laceyAct.US_PGACommercialDescription = "North American Softwood Pulpwood";
			laceyAct.US_InvCurrPGAValue = 30000m;
			laceyAct.US_PGALineValue = 30000m;
			laceyAct.US_UnknownBreakdown = true;
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(laceyAct, "WHXU4101474");

			var constituentElement = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement.US_PGANameOfTheConstituentElement = "Pine Pulpwood";
			constituentElement.US_PGAQuantityOfConstituentElement = 50m;
			constituentElement.US_PGAUnitOfMeasure = "M3";
			constituentElement.US_UnknownBreakdownCountryCode = "CA";
			constituentElement.US_GenusName = "Pinus Echinada";
			constituentElement.US_SpeciesName = "Reclaimed";

			var constituentElement1 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement1.US_PGANameOfTheConstituentElement = "Pine Pulpwood";
			constituentElement1.US_PGAQuantityOfConstituentElement = 50m;
			constituentElement1.US_PGAUnitOfMeasure = "M3";
			constituentElement1.US_UnknownBreakdownCountryCode = "US";
			constituentElement1.US_GenusName = "Pinus Echinada";
			constituentElement1.US_SpeciesName = "Reclaimed";

			var constituentElement2 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement2.US_PGANameOfTheConstituentElement = "Pine Pulpwood";
			constituentElement2.US_PGAQuantityOfConstituentElement = 50m;
			constituentElement2.US_PGAUnitOfMeasure = "M3";
			constituentElement2.US_UnknownBreakdownCountryCode = "US";
			constituentElement2.US_GenusName = "Pinus Taeda";
			constituentElement2.US_SpeciesName = "Reclaimed";

			var constituentElement2_2 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement2_2.US_PGANameOfTheConstituentElement = "Pine Pulpwood";
			constituentElement2_2.US_PGAQuantityOfConstituentElement = 50m;
			constituentElement2_2.US_PGAUnitOfMeasure = "M3";
			constituentElement2_2.US_UnknownBreakdownCountryCode = "CA";
			constituentElement2_2.US_GenusName = "Pinus Taeda";
			constituentElement2_2.US_SpeciesName = "Reclaimed";

			var constituentElement3 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement3.US_PGANameOfTheConstituentElement = "Pine Pulpwood";
			constituentElement3.US_PGAQuantityOfConstituentElement = 50m;
			constituentElement3.US_PGAUnitOfMeasure = "M3";
			constituentElement3.US_UnknownBreakdownCountryCode = "CA";
			constituentElement3.US_GenusName = "Pinus Rigida";
			constituentElement3.US_SpeciesName = "Reclaimed";

			var constituentElement3_3 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement3_3.US_PGANameOfTheConstituentElement = "Pine Pulpwood";
			constituentElement3_3.US_PGAQuantityOfConstituentElement = 50m;
			constituentElement3_3.US_PGAUnitOfMeasure = "M3";
			constituentElement3_3.US_UnknownBreakdownCountryCode = "US";
			constituentElement3_3.US_GenusName = "Pinus Rigida";
			constituentElement3_3.US_SpeciesName = "Reclaimed";

			MergeAndSend("LCY5", true, true);

			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
			AssertContains(
@"OI        SOFTWOOD PULPWOOD                                                     
PG01001APHAPL                                                                   
PG02P                                                                           
PG10                   NORTH AMERICAN SOFTWOOD PULPWOOD                         
PG04YPINE PULPWOOD                                      000000005000M3          
PG05PINUS ECHINADA        RECLAIMED                                             
PG06HRVCA                                                                       
PG04YPINE PULPWOOD                                      000000005000M3          
PG05PINUS ECHINADA        RECLAIMED                                             
PG06HRVUS                                                                       
PG04YPINE PULPWOOD                                      000000005000M3          
PG05PINUS TAEDA           RECLAIMED                                             
PG06HRVUS                                                                       
PG04YPINE PULPWOOD                                      000000005000M3          
PG05PINUS TAEDA           RECLAIMED                                             
PG06HRVCA                                                                       
PG04YPINE PULPWOOD                                      000000005000M3          
PG05PINUS RIGIDA          RECLAIMED                                             
PG06HRVCA                                                                       
PG04YPINE PULPWOOD                                      000000005000M3          
PG05PINUS RIGIDA          RECLAIMED                                             
PG06HRVUS                                                                       
PG19IM                                                   IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21IM JOHN Q IMPORTER        3015552232     JQI@IMP.COM                        
PG22             IM AP6 Y05152015                                               
PG25                                                    000000030000            
PG27WHXU4101474                                                                 ", message.EM_FormattedMessageText);
		}

		[TestDate(2015, 05, 15)]
		public void TestLaceyActExample6_SpecialUseDesignation()
		{
			/*
			This Sample is not common and requires the exercise of due care to determine applicability.

			Example 6 is an entry of products whose component species are difficult to determine. 
			APHIS has created Special Use Designations (SUD’s) to streamline the declaration requirement 
			for cases where the importer has exercised their due care and concluded that the species 
			is impossible to determine. This entry contains 2 products. 

			The first product was manufactured in 1955 (prior to the Lacey Act reporting requirement) 
			and *after the exercise of due care the importer could not determine the species. 

			The second product is a seat made of pine and particleboard and (after the exercise of due care) 
			the importer could not determine the species. For the Pine portion of the chairs is made of
			100 M3 of Pinus strobus. For the particle board portion of the chairs, the quantity of plant 
			material harvested in Canada is 200 M3 and the quantity of plant material harvested in the United States is 300 M3.
		
			*The statute requires the species and country of harvest for each plant or plant product.
			*/

			SetUpDataForLaceyAct();
			invoiceLine.JI_Description = "Seats with Wood Frames";

			var laceyAct = invoiceLine.LaceyActLines.AddNew();
			laceyAct.US_PGACommercialDescription = "Seats with Wooden Frames";
			laceyAct.US_InvCurrPGAValue = 7000m;
			laceyAct.US_PGALineValue = 7000m;
			laceyAct.US_UnknownBreakdown = true;

			var constituentElement = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement.US_PGANameOfTheConstituentElement = "Vintage Seats";
			constituentElement.US_PGAQuantityOfConstituentElement = 100m;
			constituentElement.US_PGAUnitOfMeasure = "M3";
			constituentElement.US_UnknownBreakdownCountryCode = "CA";
			constituentElement.US_GenusName = "SPECIAL PREAMENDMENT";
			constituentElement.US_SpeciesName = "SPF";

			var laceyAct2 = invoiceLine.LaceyActLines.AddNew();
			laceyAct2.US_PGACommercialDescription = "Seats with Wooden Frames";
			laceyAct2.US_InvCurrPGAValue = 40000m;
			laceyAct2.US_PGALineValue = 40000m;

			var constituentElement2 = laceyAct2.PG04ConstituentElements.AddNew();
			constituentElement2.US_PGANameOfTheConstituentElement = "Pine veneer";
			constituentElement2.US_PGAQuantityOfConstituentElement = 100m;
			constituentElement2.US_PGAUnitOfMeasure = "M3";
			constituentElement2.US_UnknownBreakdownCountryCode = "CA";
			constituentElement2.US_GenusName = "PINUS STROBUS";
			constituentElement2.US_SpeciesName = "SPF";

			var constituentElement3 = laceyAct2.PG04ConstituentElements.AddNew();
			constituentElement3.US_PGANameOfTheConstituentElement = "Particleboard base";
			constituentElement3.US_PGAQuantityOfConstituentElement = 200m;
			constituentElement3.US_PGAUnitOfMeasure = "M3";
			constituentElement3.US_UnknownBreakdownCountryCode = "CA";
			constituentElement3.US_GenusName = "SPECIAL COMPOSITE";
			constituentElement3.US_SpeciesName = "SPF";

			var constituentElement4 = laceyAct2.PG04ConstituentElements.AddNew();
			constituentElement4.US_PGANameOfTheConstituentElement = "Particleboard base";
			constituentElement4.US_PGAQuantityOfConstituentElement = 300m;
			constituentElement4.US_PGAUnitOfMeasure = "M3";
			constituentElement4.US_UnknownBreakdownCountryCode = "US";
			constituentElement4.US_GenusName = "SPECIAL COMPOSITE";
			constituentElement4.US_SpeciesName = "SPF";

			MergeAndSend("LCY6", true, true);

			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
			AssertContains(
@"OI        SEATS WITH WOOD FRAMES                                                
PG01001APHAPL                                                                   
PG02P                                                                           
PG10                   SEATS WITH WOODEN FRAMES                                 
PG04YVINTAGE SEATS                                      000000010000M3          
PG05SPECIAL PREAMENDMENT  SPF                                                   
PG06HRVCA                                                                       
PG19IM                                                   IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21IM JOHN Q IMPORTER        3015552232     JQI@IMP.COM                        
PG22             IM AP6 Y05152015                                               
PG25                                                    000000007000            
PG01002APHAPL                                                                   
PG02P                                                                           
PG10                   SEATS WITH WOODEN FRAMES                                 
PG04 PINE VENEER                                        000000010000M3          
PG05PINUS STROBUS         SPF                                                   
PG06HRVCA                                                                       
PG04 PARTICLEBOARD BASE                                 000000020000M3          
PG05SPECIAL COMPOSITE     SPF                                                   
PG06HRVCA                                                                       
PG04 PARTICLEBOARD BASE                                 000000030000M3          
PG05SPECIAL COMPOSITE     SPF                                                   
PG06HRVUS                                                                       
PG19IM                                                   IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21IM JOHN Q IMPORTER        3015552232     JQI@IMP.COM                        
PG22             IM AP6 Y05152015                                               
PG25                                                    000000040000            ", message.EM_FormattedMessageText);
		}

		[TestDate(2015, 05, 15)]
		public void TestLaceyActExample7_ComponentLevelDeclaration()
		{
			/*
			Example 7 is a component-level declaration of an entry of wooden side chairs. This shipment is identical to the shipment in Example 8.

			The first line of chairs is made of a variety of different species with a two-species backing. 
			There are 800 m3 of Pinus radiata from Russia, 500 m3 of Betula platyphylla from Russia, 600 m3 of 
			Pinus radiata from Russia, 200 m3 of Betula platyphylla from Russia. 

			The second line of chairs is a fabric-covered seat containing another set of species. 
			There are 22000 m3 of Pinus radiata from New Zealand, 40000 m3 of Betula Alleghaniensis from Canada. 
			The fabric is made of synthetic (non-plant) material and does not require a Lacey Act declaration. 
			The value of the PGA line is $10,000, and the shipment is in one container.

			*/

			SetUpDataForLaceyAct();
			invoiceLine.JI_Description = "Wooden K.D Furniture";
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "WHXU4101474", true);

			var laceyAct = invoiceLine.LaceyActLines.AddNew();
			laceyAct.US_PGACommercialDescription = "Wooden K.D Furniture";
			laceyAct.US_InvCurrPGAValue = 10000m;
			laceyAct.US_PGALineValue = 10000m;
			laceyAct.US_UnknownBreakdown = true;
			laceyAct.ContainersForPGALine.RemoveAndDeleteAll();

			var constituentElement = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement.US_PGANameOfTheConstituentElement = "Chair Sides";
			constituentElement.US_PGAQuantityOfConstituentElement = 800m;
			constituentElement.US_PGAUnitOfMeasure = "M3";
			constituentElement.US_UnknownBreakdownCountryCode = "RU";
			constituentElement.US_GenusName = "Pinus radiata";
			constituentElement.US_SpeciesName = "Recycled";

			var constituentElement2 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement2.US_PGANameOfTheConstituentElement = "Chair Back";
			constituentElement2.US_PGAQuantityOfConstituentElement = 500m;
			constituentElement2.US_PGAUnitOfMeasure = "M3";
			constituentElement2.US_UnknownBreakdownCountryCode = "RU";
			constituentElement2.US_GenusName = "Betula platyphylla";
			constituentElement2.US_SpeciesName = "Recycled";

			var constituentElement3 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement3.US_PGANameOfTheConstituentElement = "Chair Seat";
			constituentElement3.US_PGAQuantityOfConstituentElement = 600m;
			constituentElement3.US_PGAUnitOfMeasure = "M3";
			constituentElement3.US_UnknownBreakdownCountryCode = "RU";
			constituentElement3.US_GenusName = "Pinus radiata";
			constituentElement3.US_SpeciesName = "Recycled";

			var constituentElement4 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement4.US_PGANameOfTheConstituentElement = "Chair Legs";
			constituentElement4.US_PGAQuantityOfConstituentElement = 200m;
			constituentElement4.US_PGAUnitOfMeasure = "M3";
			constituentElement4.US_UnknownBreakdownCountryCode = "RU";
			constituentElement4.US_GenusName = "Betula platyphylla";
			constituentElement4.US_SpeciesName = "Recycled";

			var laceyAct2 = invoiceLine.LaceyActLines.AddNew();
			laceyAct2.US_PGACommercialDescription = "Wooden K.D Furniture";
			laceyAct2.US_InvCurrPGAValue = 10000m;
			laceyAct2.US_PGALineValue = 10000m;
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(laceyAct2, "WHXU4101474");

			var constituentElement5 = laceyAct2.PG04ConstituentElements.AddNew();
			constituentElement5.US_PGANameOfTheConstituentElement = "Chair Legs";
			constituentElement5.US_PGAQuantityOfConstituentElement = 22000m;
			constituentElement5.US_PGAUnitOfMeasure = "M3";
			constituentElement5.US_UnknownBreakdownCountryCode = "NZ";
			constituentElement5.US_GenusName = "Pinus radiata";
			constituentElement5.US_SpeciesName = "Recycled";

			var constituentElement6 = laceyAct2.PG04ConstituentElements.AddNew();
			constituentElement6.US_PGANameOfTheConstituentElement = "Chair back";
			constituentElement6.US_PGAQuantityOfConstituentElement = 40000m;
			constituentElement6.US_PGAUnitOfMeasure = "M3";
			constituentElement6.US_UnknownBreakdownCountryCode = "CA";
			constituentElement6.US_GenusName = "Betula alleghaniensis";
			constituentElement6.US_SpeciesName = "Recycled";

			MergeAndSend("LCY6", true, true);

			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
			AssertContains(
@"OI        WOODEN K.D FURNITURE                                                  
PG01001APHAPL                                                                   
PG02P                                                                           
PG10                   WOODEN K.D FURNITURE                                     
PG04YCHAIR SIDES                                        000000080000M3          
PG05PINUS RADIATA         RECYCLED                                              
PG06HRVRU                                                                       
PG04YCHAIR BACK                                         000000050000M3          
PG05BETULA PLATYPHYLLA    RECYCLED                                              
PG06HRVRU                                                                       
PG04YCHAIR SEAT                                         000000060000M3          
PG05PINUS RADIATA         RECYCLED                                              
PG06HRVRU                                                                       
PG04YCHAIR LEGS                                         000000020000M3          
PG05BETULA PLATYPHYLLA    RECYCLED                                              
PG06HRVRU                                                                       
PG19IM                                                   IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21IM JOHN Q IMPORTER        3015552232     JQI@IMP.COM                        
PG22             IM AP6 Y05152015                                               
PG25                                                    000000010000            
PG27WHXU4101474                                                                 
PG01002APHAPL                                                                   
PG02P                                                                           
PG10                   WOODEN K.D FURNITURE                                     
PG04 CHAIR LEGS                                         000002200000M3          
PG05PINUS RADIATA         RECYCLED                                              
PG06HRVNZ                                                                       
PG04 CHAIR BACK                                         000004000000M3          
PG05BETULA ALLEGHANIENSIS RECYCLED                                              
PG06HRVCA                                                                       
PG19IM                                                   IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21IM JOHN Q IMPORTER        3015552232     JQI@IMP.COM                        
PG22             IM AP6 Y05152015                                               
PG25                                                    000000010000            
PG27WHXU4101474                                                                 ", message.EM_FormattedMessageText);
		}

		[TestDate(2015, 05, 15)]
		public void TestLaceyActExample8_ProductLevelDeclaration()
		{
			/*
			Example 8 is a product-level declaration of an entry of wooden side chairs. This shipment is identical to the shipment in Example 7.

			The first line of chairs is made of a variety of different species with a two-species backing.  
			There are 800 m3 of Pinus radiata from Russia, 500 m3 of Betula platyphylla from Russia, 600 m3 of 
			Pinus radiata from Russia, 200 m3 of Betula platyphylla from Russia. 

			The second line of chairs is a fabric-covered seat containing another set of species. 
			There are 22000 m3 of Pinus radiata from New Zealand, 40000 m3 of Betula Alleghaniensis from Canada. 
			The fabric is made of synthetic (non-plant) material and does not require a Lacey Act declaration. 
			The value of the PGA line is $10,000, and the shipment is in one container.

			*/

			SetUpDataForLaceyAct();
			invoiceLine.JI_Description = "Wooden K.D Furniture";
			DeclarationTestHelper.AddContainerForInvoiceLine(invoiceLine, "WHXU4101474", true);

			var laceyAct = invoiceLine.LaceyActLines.AddNew();
			laceyAct.US_PGACommercialDescription = "Wooden K.D Furniture";
			laceyAct.US_InvCurrPGAValue = 10000m;
			laceyAct.US_PGALineValue = 10000m;
			laceyAct.US_UnknownBreakdown = true;
			laceyAct.ContainersForPGALine.RemoveAndDeleteAll();

			var constituentElement = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement.US_PGANameOfTheConstituentElement = "Chair - Sides and Seat";
			constituentElement.US_PGAQuantityOfConstituentElement = 1400m;
			constituentElement.US_PGAUnitOfMeasure = "M3";
			constituentElement.US_UnknownBreakdownCountryCode = "RU";
			constituentElement.US_GenusName = "Pinus radiata";
			constituentElement.US_SpeciesName = "Recycled";

			var constituentElement2 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement2.US_PGANameOfTheConstituentElement = "Chair - Back and Legs";
			constituentElement2.US_PGAQuantityOfConstituentElement = 700m;
			constituentElement2.US_PGAUnitOfMeasure = "M3";
			constituentElement2.US_UnknownBreakdownCountryCode = "RU";
			constituentElement2.US_GenusName = "Betula platyphylla";
			constituentElement2.US_SpeciesName = "Recycled";

			var laceyAct2 = invoiceLine.LaceyActLines.AddNew();
			laceyAct2.US_PGACommercialDescription = "Wooden K.D Furniture";
			laceyAct2.US_InvCurrPGAValue = 10000m;
			laceyAct2.US_PGALineValue = 10000m;
			DeclarationTestHelper.AssociateContainerWithLaceyActLine(laceyAct2, "WHXU4101474");

			var constituentElement5 = laceyAct2.PG04ConstituentElements.AddNew();
			constituentElement5.US_PGANameOfTheConstituentElement = "Chair - Legs";
			constituentElement5.US_PGAQuantityOfConstituentElement = 22000m;
			constituentElement5.US_PGAUnitOfMeasure = "M3";
			constituentElement5.US_UnknownBreakdownCountryCode = "NZ";
			constituentElement5.US_GenusName = "Pinus radiata";
			constituentElement5.US_SpeciesName = "Recycled";

			var constituentElement6 = laceyAct2.PG04ConstituentElements.AddNew();
			constituentElement6.US_PGANameOfTheConstituentElement = "Chair - Back";
			constituentElement6.US_PGAQuantityOfConstituentElement = 40000m;
			constituentElement6.US_PGAUnitOfMeasure = "M3";
			constituentElement6.US_UnknownBreakdownCountryCode = "CA";
			constituentElement6.US_GenusName = "Betula alleghaniensis";
			constituentElement6.US_SpeciesName = "Recycled";

			MergeAndSend("LCY8", true, true);

			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
			AssertContains(
@"OI        WOODEN K.D FURNITURE                                                  
PG01001APHAPL                                                                   
PG02P                                                                           
PG10                   WOODEN K.D FURNITURE                                     
PG04YCHAIR - SIDES AND SEAT                             000000140000M3          
PG05PINUS RADIATA         RECYCLED                                              
PG06HRVRU                                                                       
PG04YCHAIR - BACK AND LEGS                              000000070000M3          
PG05BETULA PLATYPHYLLA    RECYCLED                                              
PG06HRVRU                                                                       
PG19IM                                                   IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21IM JOHN Q IMPORTER        3015552232     JQI@IMP.COM                        
PG22             IM AP6 Y05152015                                               
PG25                                                    000000010000            
PG27WHXU4101474                                                                 
PG01002APHAPL                                                                   
PG02P                                                                           
PG10                   WOODEN K.D FURNITURE                                     
PG04 CHAIR - LEGS                                       000002200000M3          
PG05PINUS RADIATA         RECYCLED                                              
PG06HRVNZ                                                                       
PG04 CHAIR - BACK                                       000004000000M3          
PG05BETULA ALLEGHANIENSIS RECYCLED                                              
PG06HRVCA                                                                       
PG19IM                                                   IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21IM JOHN Q IMPORTER        3015552232     JQI@IMP.COM                        
PG22             IM AP6 Y05152015                                               
PG25                                                    000000010000            
PG27WHXU4101474                                                                 ", message.EM_FormattedMessageText);
		}

		[TestDate(2015, 05, 15)]
		public void TestLaceyActExample9_DeclarationWithPermit()
		{
			/*
			Example 9 is an entry of pine desks with only one CBP line item and one PGA line item.  
			The desks are made of pine, with a quantity is 100 cubic meters, harvested in Canada, 
			and valued at $10,000. These desks have an inlay of 0.678 cubic meters of Cedrela oderata 
			harvested in Bolivia. This is a CITES species that requires a permit. The permit number is: 000473
			*/

			SetUpDataForLaceyAct();
			invoiceLine.JI_Description = "Wooden Desk Pine";

			var laceyAct = invoiceLine.LaceyActLines.AddNew();
			laceyAct.US_PGACommercialDescription = "Pine Desk Model 0415";
			laceyAct.US_InvCurrPGAValue = 10000m;
			laceyAct.US_PGALineValue = 10000m;
			laceyAct.US_UnknownBreakdown = true;
			laceyAct.US_CertifyingIndividual = PartyTypeList.Codes.Importer;

			var constituentElement = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement.US_PGANameOfTheConstituentElement = "Desk sides top legs and frame";
			constituentElement.US_PGAQuantityOfConstituentElement = 100m;
			constituentElement.US_PGAUnitOfMeasure = "M3";
			constituentElement.US_UnknownBreakdownCountryCode = "CA";
			constituentElement.US_GenusName = "Pinus Taeda";
			constituentElement.US_SpeciesName = "PreAmendment";

			var constituentElement2 = laceyAct.PG04ConstituentElements.AddNew();
			constituentElement2.US_PGANameOfTheConstituentElement = "Cedar Inlay CITES Protected";
			constituentElement2.US_PGAQuantityOfConstituentElement = 0.68m;
			constituentElement2.US_PGAUnitOfMeasure = "M3";
			constituentElement2.US_UnknownBreakdownCountryCode = "BO";
			constituentElement2.US_GenusName = "Cedrela oderata";
			constituentElement2.US_SpeciesName = "Recycled";

			MergeAndSend("LCY9", true, false);

			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
			AssertContains(
@"OI        WOODEN DESK PINE                                                      
PG01001APHAPL                                                                   
PG02P                                                                           
PG10                   PINE DESK MODEL 0415                                     
PG04YDESK SIDES TOP LEGS AND FRAME                      000000010000M3          
PG05PINUS TAEDA           PREAMENDMENT                                          
PG06HRVCA                                                                       
PG04YCEDAR INLAY CITES PROTECTED                        000000000068M3          
PG05CEDRELA ODERATA       RECYCLED                                              
PG06HRVBO                                                                       
PG19IM                                                   IMPORTER STREET 1      
PG20IMPORTER STREET 2                    LOS ANGELES          CA US96358        
PG21IM JOHN Q IMPORTER        3015552232     JQI@IMP.COM                        
PG22             IM AP6 Y05152015                                               
PG25                                                    000000010000            ", message.EM_FormattedMessageText);
		}

		[TestDate(2015, 05, 15)]
		public void TestLaceyActExample10_DisclaimerDeclaration()
		{
			/*
			Example 10 is a disclaimed declaration for an entry of metal pianos. 
			The Lacey Act Declaration is flagged for shipments of HTS 9201, however 
			there is no plant material in product and therefore does not require any further PGA information under the Lacey Act.
			*/

			SetUpDataForLaceyAct();
			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.JI_Description = "Cast Iron Grand Pianos";
			invoiceLine.US_LaceyDisclaimReason = PGADisclaimReasonList.Codes.C;

			MergeAndSend("LCY10", true, false);

			var message = (MQEDIMessage)declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0];
			Assert(message.EM_FormattedMessageText.Contains(
@"OI        CAST IRON GRAND PIANOS                                                
PG01001APHAPL                                                                  C"));
		}

		[TestDate(2016, 06, 29)]
		public void TestBuildMessageForEntryType06And22()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableENS = true;
			declaration.US_EntryDateElectionCode = "W";
			declaration.JE_EntryAuthorisationDate = ZDate.BrettsBirthday;
			declaration.JE_MasterBill = "FTZBILL";
			declaration.US_FTZNo = "123456";
			declaration.JE_PrimaryITNumber = "V2365425144";
			declaration.US_ITDate = new ZDate(2016, 06, 29);
			declaration.JE_TotalNoOfPacks = 100;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true);
			var message = messageBuilder.PopulateMessage();
			AssertMultilineASCIIEquals("Message generated correctly",
@"B  3902XJ5AE                                               <<MSGNO PLACEHOLDER>>
10ASV9  <E#PLCH> 3902            0610 X           2070516          FY           
1191-01319900091-013199000                                  IL123456            
20AAAA3902          APL EMERALD                    062916                       
21V123W                                                                         
2200000100PK                                                                    
23I    V2365425144                                                              
23MAAAAFTZBILL                                                                  
318B 891                                                                        
319A    00000000101234                                                          
40  001 AUAU062216        0000000050602670000009000    N                        
41       0000000000                                                             
44COMMERCIAL DESCRIPTION                                                        
47MTHLIATHA191NAK                                                               
47C91-013199000                                                                 
47S91-013199000                                                                 
504421909720 0000033000 0000010000 000000007000GR                               
6249900000000                                                                   
8949900000000000                                                                
9000000033000 00000000000 00000000000 00000000000 00000000000 00000000000       
Y  3902XJ5AE", message.EM_FormattedMessageText);

			declaration.US_EntryType = EntryTypeList.Codes.ReWarehouse;
			messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true);
			message = messageBuilder.PopulateMessage();
			AssertMultilineASCIIEquals("Message generated correctly",
@"B  3902XJ5AE                                               <<MSGNO PLACEHOLDER>>
10ASV9  <E#PLCH> 3902            2210 X           2070516           Y           
1191-01319900091-013199000                                  IL                  
20AAAA3902          APL EMERALD                    062916                       
21V123W                                                                         
2200000100PK                                                                    
23I    V2365425144                                                              
318B 891                                                                        
319A    00000000101234                                                          
40  001 AUAU062216        0000000050602670000009000    N                        
44COMMERCIAL DESCRIPTION                                                        
47MTHLIATHA191NAK                                                               
47C91-013199000                                                                 
47S91-013199000                                                                 
504421909720 0000033000 0000010000 000000007000GR                               
6249900000000                                                                   
8949900000000000                                                                
9000000033000 00000000000 00000000000 00000000000 00000000000 00000000000       
Y  3902XJ5AE", message.EM_FormattedMessageText);

			declaration.JE_TransportMode = TransportTypeList.Codes.Truck;
			messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true);
			message = messageBuilder.PopulateMessage();
			AssertMultilineASCIIEquals("Message generated correctly",
@"B  3902XJ5AE                                               <<MSGNO PLACEHOLDER>>
10ASV9  <E#PLCH> 3902            2230 X           2070516           Y           
1191-01319900091-013199000                     062916       IL                  
20AAAA3902062916                                   062916                       
2200000100PK                                                                    
23I    V2365425144                                                              
23MAAAAFTZBILL                                                                  
318B 891                                                                        
319A    00000000101234                                                          
40  001 AUAU062216        0000000050     0000009000    N                        
44COMMERCIAL DESCRIPTION                                                        
47MTHLIATHA191NAK                                                               
47C91-013199000                                                                 
47S91-013199000                                                                 
504421909720 0000033000 0000010000 000000007000GR                               
6249900000000                                                                   
8949900000000000                                                                
9000000033000 00000000000 00000000000 00000000000 00000000000 00000000000       
Y  3902XJ5AE", message.EM_FormattedMessageText);
		}

		[TestDate(2016, 9, 1)]
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

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			invoiceLine.JI_Tariff = tariff0.UE_Tariff;
			invoiceLine.JI_Description = "TEST PARENT TSCA";
			invoiceLine.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_TSCAIndicator = TSCAIndicatorList.Codes.TSCAPositive;
			invoiceLine.US_TSCAODSCertIndividual = PartyTypeList.Codes.Importer;
			invoiceLine.US_FDAContactName = "PARENT IMPORTER";

			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = tariff1.UE_Tariff;
			invoiceLine1.JI_ParentID = invoiceLine.PK;
			invoiceLine1.JI_Description = "TEST SECONDARY TSCA";
			invoiceLine1.US_TSCAInd = OGAIndicatorList.Codes.Declared;
			invoiceLine1.US_TSCAIndicator = TSCAIndicatorList.Codes.TSCANegative;
			invoiceLine1.US_TSCAODSCertIndividual = PartyTypeList.Codes.CustomsBroker;
			invoiceLine1.US_FDAContactName = "SECONDARY BROKER";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			var message = messageBuilder.PopulateMessage();
			AssertMultilineASCIIEquals("Message generated correctly",
@"B  3902XJ5AE                                               <<MSGNO PLACEHOLDER>>
10ASV9  <E#PLCH> 3902            0110 XA          2090616                       
1191-01319900091-013199000                                  IL                  
20AAAA3902          APL EMERALD                                                 
SE13SUPPORT                                 123456                              
21V123W                                                                         
2200000001PK                                                                    
318B 891                                                                        
319A    00000000101234                                                          
SE30ST MR IMPORTER                                                              
SE3515                                                                          
SE36                                                           US               
40  001 AUAU082516        0000000050602670000009000    N                        
44TEST PARENT TSCA                                                              
44TEST SECONDARY TSCA                                                           
47MTHLIATHA191NAK                                                               
47S91-013199000                                                                 
SE50MF MR MANUFACTURER                                                          
SE5515                                                                          
SE56                                                           AU               
509608500000 0000000000 0000010000                                              
OI        TEST PARENT TSCA                                                      
PG01001EPATS1                                                                   
PG02P                                                                           
PG22             CI     Y09012016                                               
PG21CI PARENT IMPORTER                                                          
509608100000 0000000000 0000000000                                              
OI        TEST SECONDARY TSCA                                                   
PG01002EPATS1                                                                   
PG02P                                                                           
PG22             CI     Y09012016                                               
PG21CI SECONDARY BROKER                                                         
6249900003464                                                                   
6250100001250                                                                   
894990000000346450100000001250                                                  
9000000000000 00000004714 00000000000 00000000000 00000000000 00000000000       
Y  3902XJ5AE", message.EM_FormattedMessageText);
		}

		[TestDate(2018, 05, 21)]
		public void TestSendProductExclusion()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "7201101000";
			invoiceLine.JI_Description = "TEST PRODUCT EXCLUSION";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			var message = messageBuilder.PopulateMessage();
			AssertMultilineASCIIEquals("Message generated correctly",
@"B  3902XJ5AE                                               <<MSGNO PLACEHOLDER>>
10ASV9  <E#PLCH> 3902            0110 XA          2052918                       
1191-01319900091-013199000                                  IL                  
20AAAA3902          APL EMERALD                                                 
SE13SUPPORT                                 123456                              
21V123W                                                                         
2200000001PK                                                                    
318B 891                                                                        
319A    00000000101234                                                          
SE30ST MR IMPORTER                                                              
SE3515                                                                          
SE36                                                           US               
40  001 AUAU051418        0000000050602670000009000    N                        
44COMMERCIAL DESCRIPTION                                                        
47MTHLIATHA191NAK                                                               
47S91-013199000                                                                 
SE50MF MR MANUFACTURER                                                          
SE5515                                                                          
SE56                                                           AU               
504421909720 0000033000 0000010000 000000007000GR                               
6249900003464                                                                   
6250100001250                                                                   
40  002 THAU051418        0000000000602670000000000    N                        
44TEST PRODUCT EXCLUSION                                                        
47MTHLIATHA191NAK                                                               
47S91-013199000                                                                 
SE50MF MR MANUFACTURER                                                          
SE5515                                                                          
SE56                                                           AU               
507201101000 0000000000 0000000000                                              
6249900000000                                                                   
894990000000346450100000001250                                                  
9000000033000 00000004714 00000000000 00000000000 00000000000 00000000000       
Y  3902XJ5AE", message.EM_FormattedMessageText);

			invoiceLine.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
			invoiceLine.US_ExclusionNumber = "STL123456";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			message = messageBuilder.PopulateMessage();
			AssertMultilineASCIIEquals("Message generated correctly",
@"B  3902XJ5AE                                               <<MSGNO PLACEHOLDER>>
10ASV9  <E#PLCH> 3902            0110 XA          2052918                       
1191-01319900091-013199000                                  IL                  
20AAAA3902          APL EMERALD                                                 
SE13SUPPORT                                 123456                              
21V123W                                                                         
2200000001PK                                                                    
318B 891                                                                        
319A    00000000101234                                                          
SE30ST MR IMPORTER                                                              
SE3515                                                                          
SE36                                                           US               
40  001 AUAU051418        0000000050602670000009000    N                        
44COMMERCIAL DESCRIPTION                                                        
47MTHLIATHA191NAK                                                               
47S91-013199000                                                                 
SE50MF MR MANUFACTURER                                                          
SE5515                                                                          
SE56                                                           AU               
504421909720 0000033000 0000010000 000000007000GR                               
6249900003464                                                                   
6250100001250                                                                   
40  002 THAU051418        0000000000602670000000000    N                        
44TEST PRODUCT EXCLUSION                                                        
47MTHLIATHA191NAK                                                               
47S91-013199000                                                                 
SE50MF MR MANUFACTURER                                                          
SE5515                                                                          
SE56                                                           AU               
507201101000 0000000000 0000000000                                              
5402STL123456                                                                   
6249900000000                                                                   
894990000000346450100000001250                                                  
9000000033000 00000004714 00000000000 00000000000 00000000000 00000000000       
Y  3902XJ5AE", message.EM_FormattedMessageText);
		}

		public void TestBrokerReference()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "7201101000";
			declaration.JE_DeclarationReference = "B00155570";
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;

			var filer = new EntryFiler();
			filer.EntryFilerCode = "SV9";
			USCustomsDataRegistry.Instance.EntryFiler.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, filer);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			var message = messageBuilder.PopulateMessage();
			var ens10Block = message.MessageBlock.MessageBlocks.OfType<AENS10>().FirstOrDefault();
			Factory.Save();
			AssertEquals(declaration.JE_DeclarationReference, ens10Block.BrokerReferenceNumber);

			declaration.US_PSC = false;
			declaration.US_BRDRefNo = "AAAA";
			messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			message = messageBuilder.PopulateMessage();
			ens10Block = message.MessageBlock.MessageBlocks.OfType<AENS10>().FirstOrDefault();
			AssertEquals(declaration.US_BRDRefNo, ens10Block.BrokerReferenceNumber);

			declaration.US_PSC = true;
			declaration.US_BRDRefNo = "";
			declaration.US_EntryFilerCode = "SV9";
			messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			message = messageBuilder.PopulateMessage();
			ens10Block = message.MessageBlock.MessageBlocks.OfType<AENS10>().FirstOrDefault();
			AssertEquals(declaration.JE_DeclarationReference, ens10Block.BrokerReferenceNumber);

			declaration.US_PSC = true;
			declaration.US_BRDRefNo = "AAA";
			declaration.US_EntryFilerCode = "SV9";
			messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			message = messageBuilder.PopulateMessage();
			ens10Block = message.MessageBlock.MessageBlocks.OfType<AENS10>().FirstOrDefault();
			AssertEquals(declaration.US_BRDRefNo, ens10Block.BrokerReferenceNumber);

			declaration.US_PSC = true;
			declaration.US_BRDRefNo = "";
			declaration.US_EntryFilerCode = "SV7";
			messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			message = messageBuilder.PopulateMessage();
			ens10Block = message.MessageBlock.MessageBlocks.OfType<AENS10>().FirstOrDefault();
			AssertEquals(ZString.Empty, ens10Block.BrokerReferenceNumber);

			declaration.US_PSC = true;
			declaration.US_BRDRefNo = "AAA";
			declaration.US_EntryFilerCode = "SV7";
			messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			message = messageBuilder.PopulateMessage();
			ens10Block = message.MessageBlock.MessageBlocks.OfType<AENS10>().FirstOrDefault();
			AssertEquals(declaration.US_BRDRefNo, ens10Block.BrokerReferenceNumber);
		}

		[TestDate(2017, 5, 11)]
		public void TestSE50InsteadOfAE47EntrySummary()
		{
			var shipToParty = Factory.NewWithValidTestData<OrgHeader>();
			var shipToPartyAddress = shipToParty.MainAddress;
			shipToPartyAddress.CompanyName = "ShipToPartyCompany";
			shipToPartyAddress.Address1 = "ShipToPartyAddress1";
			shipToPartyAddress.City = "ShipToPartyCity";
			shipToPartyAddress.Postcode = "STPPCode";

			var invoiceLine = declaration.InvoiceLines[0];
			declaration.JE_OA_ShipToPartyAddress = ZGuid.Empty;
			invoiceLine.InvoiceHeader.JZ_OA_ShipToPartyAddress = ZGuid.Empty;
			invoiceLine.JI_OA_ShipToPartyAddress = shipToPartyAddress.PK;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			var message = messageBuilder.PopulateMessage();
			AssertMultilineASCIIEquals("Expected Message should contain SE50-SE56 blocks but not AE47C block",
@"B  3902XJ5AE                                               <<MSGNO PLACEHOLDER>>
10ASV9  <E#PLCH> 3902            0110 XA          2051617                       
1191-01319900091-013199000                                  IL                  
20AAAA3902          APL EMERALD                                                 
SE13SUPPORT                                 123456                              
21V123W                                                                         
2200000001PK                                                                    
318B 891                                                                        
319A    00000000101234                                                          
40  001 AUAU050417        0000000050602670000009000    N                        
44COMMERCIAL DESCRIPTION                                                        
47MTHLIATHA191NAK                                                               
47S91-013199000                                                                 
SE50MF MR MANUFACTURER                                                          
SE5515                                                                          
SE56                                                           AU               
SE50ST SHIPTOPARTYCOMPANY                                                       
SE5515SHIPTOPARTYADDRESS1                                                       
SE56SHIPTOPARTYCITY                             STPPCODE                        
504421909720 0000033000 0000010000 000000007000GR                               
6249900003464                                                                   
6250100001250                                                                   
894990000000346450100000001250                                                  
9000000033000 00000004714 00000000000 00000000000 00000000000 00000000000       
Y  3902XJ5AE", message.EM_FormattedMessageText);
		}

		public void TestKRSteelExportCertificateInAE()
		{
			var declaration = GetMergedDeclaration("B00173079");
			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.US_UC_NKCountryOfOrigin = "KR";
			invoiceLine.LicenceAndPermits.AddNew(LicencePermitTypeList.Codes.KR, "1234567890");
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			var message = messageBuilder.PopulateMessage();
			AssertMultilineASCIIEquals("54 block with KR Export Steel Certificate number is generated",
@"B  8888XJ5AE                                               <<MSGNO PLACEHOLDER>>
10AXJ5  <E#PLCH>     B00173079   01410XA                     996                
11                                                                              
20                                                                              
SE13SUPPORT                                 123456                              
2200000000                                                                      
23M    OBL10000                                                                 
40  001 KR                0000000000     0000000010    N                        
44COMMERCIAL DESCRIPTION                                                        
500709902000 0000000015 0000105000 000000001000KG                               
54041234567890                                                                  
6249900036372                                                                   
8949900000036372                                                                
9000000000015 00000036372 00000000000 00000000000 00000000000 00000000000       
Y  8888XJ5AE", message.EM_FormattedMessageText);

			invoiceLine.US_ProductExclusion = AdditionalDeclarationTypeCodeList.Codes._02;
			invoiceLine.US_ExclusionNumber = "STL123456";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			message = messageBuilder.PopulateMessage();
			AssertMultilineASCIIEquals("Two 54 blocks are generated",
@"B  8888XJ5AE                                               <<MSGNO PLACEHOLDER>>
10AXJ5  <E#PLCH>     B00173079   01410XA                     996                
11                                                                              
20                                                                              
SE13SUPPORT                                 123456                              
2200000000                                                                      
23M    OBL10000                                                                 
40  001 KR                0000000000     0000000010    N                        
44COMMERCIAL DESCRIPTION                                                        
500709902000 0000000015 0000105000 000000001000KG                               
5402STL123456                                                                   
54041234567890                                                                  
6249900036372                                                                   
8949900000036372                                                                
9000000000015 00000036372 00000000000 00000000000 00000000000 00000000000       
Y  8888XJ5AE", message.EM_FormattedMessageText);
		}

		[TestDate(2019, 9, 5)]
		public void TestENS52OfChildEntryLine()
		{
			var invoiceLine01 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine02 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine03 = invoiceHeader.InvoiceLines.AddNew();

			invoiceLine01.US_IsParent = true;
			invoiceLine02.JI_ParentID = invoiceLine01.PK;

			var licence02 = invoiceLine02.LicenceAndPermits.AddNew();
			licence02.CY_Code = "02";
			licence02.CY_Data = "2222222";

			var licence03 = invoiceLine03.LicenceAndPermits.AddNew();
			licence03.CY_Code = "03";
			licence03.CY_Data = "3333333";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			var message = messageBuilder.PopulateMessage();
			AssertMultilineASCIIEquals("Child Entry Line's License should printed.",
@"B  3902XJ5AE                                               <<MSGNO PLACEHOLDER>>
10ASV9  <E#PLCH> 3902            0110 XA          2091019                       
1191-01319900091-013199000                                  IL                  
20AAAA3902          APL EMERALD                                                 
SE13SUPPORT                                 123456                              
21V123W                                                                         
2200000001PK                                                                    
318B 891                                                                        
319A    00000000101234                                                          
SE30ST MR IMPORTER                                                              
SE3515                                                                          
SE36                                                           US               
40  001 AUAU082919        0000000050602670000009000    N                        
44COMMERCIAL DESCRIPTION                                                        
47MTHLIATHA191NAK                                                               
47S91-013199000                                                                 
SE50MF MR MANUFACTURER                                                          
SE5515                                                                          
SE56                                                           AU               
504421909720 0000033000 0000010000 000000007000GR                               
6249900003464                                                                   
6250100001250                                                                   
40  002 THAU082919        0000000000602670000000000    N                        
47MTHLIATHA191NAK                                                               
47S91-013199000                                                                 
SE50MF MR MANUFACTURER                                                          
SE5515                                                                          
SE56                                                           AU               
50           0000000000 0000000000                                              
50           0000000000 0000000000                                              
52022222222                                                                     
6249900000000                                                                   
40  003 THAU082919        0000000000602670000000000    N                        
47MTHLIATHA191NAK                                                               
47S91-013199000                                                                 
SE50MF MR MANUFACTURER                                                          
SE5515                                                                          
SE56                                                           AU               
50           0000000000 0000000000                                              
52033333333                                                                     
6249900000000                                                                   
894990000000346450100000001250                                                  
9000000033000 00000004714 00000000000 00000000000 00000000000 00000000000       
Y  3902XJ5AE", message.EM_FormattedMessageText);
		}

		public void TestSendCustomsValueOnClassificationTariffWhenSTNRuleApplies()
		{
			var testHelper = new Chapter98HelperTest();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_FormattedTariff = "9102.11.3010";
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";
			invoiceLine.US_UC_NKCountryOfExport = "CN";
			invoiceLine.US_SupTariff = testHelper.Test99038815Tariff.UE_Tariff;
			invoiceLine.JI_LinePrice = 1750m;
			AssertEquals(3, invoiceLine.ChildLines.Count());

			var childLineOne = invoiceLine.ChildLines.First(x => x.JI_FormattedTariff == "9102.11.3020");
			AssertNotNull(childLineOne);
			childLineOne.JI_LinePrice = 150m;
			var childLineTwo = invoiceLine.ChildLines.First(x => x.JI_FormattedTariff == "9102.11.3030");
			AssertNotNull(childLineTwo);
			childLineTwo.JI_LinePrice = 250m;
			var childLineThree = invoiceLine.ChildLines.First(x => x.JI_FormattedTariff == "9102.11.3040");
			AssertNotNull(childLineThree);
			childLineThree.JI_LinePrice = 350m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			var message = messageBuilder.PopulateMessage();
			AssertMultilineASCIIEquals("Customs value 1750 should be sent on classification tariff (9102.11.3010)",
@"B  8888XJ5AE                                               <<MSGNO PLACEHOLDER>>
10AXJ5  <E#PLCH>                     0XA                                        
11                                                                              
20                                                                              
SE13SUPPORT                                 123456                              
40  001 CNCN              0000000000     0000000000    N                        
5099038815   0000037500 0000000000                                              
509102113010 0000000000 0000001750 000000000000NO                               
509102113020 0000000900 0000000150 000000000000NO                               
509102113030 0000000700 0000000250 000000000000NO                               
509102113040 0000001855 0000000350 000000000000NO                               
6249900000866                                                                   
6250100000313                                                                   
894990000000256750100000000313                                                  
9000000040955 00000002880 00000000000 00000000000 00000000000 00000000000       
Y  8888XJ5AE", message.EM_FormattedMessageText);
		}

		public void TestAlwaysSendCommercialDescription()
		{
			var tempCommercialDescription = "44COMMERCIAL DESCRIPTION                                                        ";
			invoiceHeader.InvoiceLines.AddNew();
			invoiceHeader.InvoiceLines.AddNew();
			invoiceHeader.InvoiceLines.AddNew();

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			var message = messageBuilder.PopulateMessage();
			AssertContains("Child Entry Line's License should printed.", tempCommercialDescription, message.EM_FormattedMessageText);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryMode = EntryModeList.Codes.Paired;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			message = messageBuilder.PopulateMessage();
			AssertContains("Child Entry Line's License should printed.", tempCommercialDescription, message.EM_FormattedMessageText);
		}

		public void TestDoNotSendMPFAmountInEntrySummaryWhenOverrideManually()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8512300030";
			var mpf = invoiceLine.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing);
			mpf.CY_IsOverridden = true;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			var message = messageBuilder.PopulateMessage();
			AssertMultilineASCIIEquals("MPF shouldn't be sent in entry summary because it's overrriden manually with zero amount",
@"B  8888XJ5AE                                               <<MSGNO PLACEHOLDER>>
10AXJ5  <E#PLCH>                 01  0XA                                        
11                                                                              
20                                                                              
SE13SUPPORT                                 123456                              
40  001                   0000000000     0000000000    N                        
508512300030 0000000000 0000000000 000000000000NO                               
9000000000000 00000000000 00000000000 00000000000 00000000000 00000000000       
Y  8888XJ5AE", message.EM_FormattedMessageText);

			mpf.CY_FeeAmount = 1m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			message = messageBuilder.PopulateMessage();
			AssertMultilineASCIIEquals("MPF should be sent in entry summary because it's amount is not zero, even it's overridden",
@"B  8888XJ5AE                                               <<MSGNO PLACEHOLDER>>
10AXJ5  <E#PLCH>                 01  0XA                                        
11                                                                              
20                                                                              
SE13SUPPORT                                 123456                              
40  001                   0000000000     0000000000    N                        
508512300030 0000000000 0000000000 000000000000NO                               
6249900000100                                                                   
8949900000002567                                                                
9000000000000 00000002567 00000000000 00000000000 00000000000 00000000000       
Y  8888XJ5AE", message.EM_FormattedMessageText);

			mpf.Delete();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			message = messageBuilder.PopulateMessage();
			AssertMultilineASCIIEquals("MPF should be sent in entry summary becasue it's not exempted",
@"B  8888XJ5AE                                               <<MSGNO PLACEHOLDER>>
10AXJ5  <E#PLCH>                 01  0XA                                        
11                                                                              
20                                                                              
SE13SUPPORT                                 123456                              
40  001                   0000000000     0000000000    N                        
508512300030 0000000000 0000000000 000000000000NO                               
6249900000000                                                                   
8949900000002567                                                                
9000000000000 00000002567 00000000000 00000000000 00000000000 00000000000       
Y  8888XJ5AE", message.EM_FormattedMessageText);
		}

		public void Test7And9CharactersFTZNumberFor06FTZ()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();

			declaration.US_FTZNo = "1234567";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true, true, UpdateActionCode.Add);
			var message = messageBuilder.PopulateMessage();
			var ens11 = message.MessageBlock.MessageBlocks.OfType<AENS11>().FirstOrDefault();
			AssertEquals(ZString.Empty, ens11.ForeignTradeZoneIdentifier);
			AssertEquals("1234567", ens11.NewForeignTradeZoneIdentifier);

			declaration.US_FTZNo = "123456789";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			message = messageBuilder.PopulateMessage();
			ens11 = message.MessageBlock.MessageBlocks.OfType<AENS11>().FirstOrDefault();
			AssertEquals(ZString.Empty, ens11.ForeignTradeZoneIdentifier);
			AssertEquals("123456789", ens11.NewForeignTradeZoneIdentifier);
		}

		public void TestAENS11NewForeignTradeZoneIdentifier()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableENS = true;
			declaration.US_FTZNo = "123456789";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true);
			var message = messageBuilder.PopulateMessage();
			var ens11 = message.MessageBlock.MessageBlocks.OfType<AENS11>().FirstOrDefault();
			AssertEquals("123456789", ens11.NewForeignTradeZoneIdentifier);
		}

		public void TestAluminumSmeltAndCastCountryDetail()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.US_Prim_NA = true;
			invoiceLine.US_RN_NKPrimCtry = Core.Constants.CountryCodes.Russia;
			invoiceLine.US_Sec_NA = true;
			invoiceLine.US_RN_NKSecCtry = Core.Constants.CountryCodes.China;
			invoiceLine.US_RN_NKCastCtry = Core.Constants.CountryCodes.Singapore;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entry, true);
			TestCase("N/ARUN/ACNSG");

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.ALUSMELT2024, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				TestCase("N  RUN  CNSG");

				invoiceLine.US_Prim_NA = false;
				invoiceLine.US_Sec_NA = false;
				TestCase("Y  RUY  CNSG");

				invoiceLine.US_Prim_NA = false;
				invoiceLine.US_RN_NKPrimCtry = ZString.Empty;
				invoiceLine.US_Sec_NA = false;
				TestCase("Y    Y  CNSG");

				invoiceLine.US_Prim_NA = false;
				invoiceLine.US_RN_NKSecCtry = ZString.Empty;
				invoiceLine.US_Sec_NA = false;
				TestCase("Y    Y    SG");
			}

			void TestCase(string expectedAdditionalInformation)
			{
				var message = messageBuilder.PopulateMessage();
				var ens54 = message.MessageBlock.MessageBlocks.OfType<AENS54>().FirstOrDefault();
				AssertEquals(AdditionalDeclarationTypeCodeList.Codes._07, ens54.ImportersAdditionalDeclarationTypeCode);
				AssertEquals(expectedAdditionalInformation, ens54.ImportersAdditionalDeclarationInformation);
			}
		}

		[TestDate(2022, 02, 11)]
		public void TestGlobalBusinessIdentifiers()
		{
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

			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_OH_Importer = importer.PK;
			declaration.IOROrgPK = importer.PK;

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

			var seEntry = declaration.ActiveEntryHeaders.EntrySummaryEntry;

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.GBIACTIVE, Core.Constants.CountryCodes.UnitedStates, ZDate.Today, true))
			{
				var builder = new ACEEntrySummaryMessageBuilderForTesting(seEntry, true, true, UpdateActionCode.Add);
				var message = builder.PopulateMessage();

				AssertMultilineASCIIEquals("GBI blocks generated", @"
B  3902XJ5AE                                               <<MSGNO PLACEHOLDER>>
10AXJ5  <E#PLCH> 3902            0110 XA                                        
11                                                          IL                  
20AAAA3902          APL EMERALD                                                 
SE13SUPPORT                                 123456                              
21V123W                                                                         
2200000001PK                                                                    
318B 891                                                                        
319A    00000000101234                                                          
SE30ST TEST IMPORTER                                                            
SE3515                                                                          
SE36                                                           US               
SE30SH TEST SHIPPER                                                             
SE31LEI 1234567890ABCDEFGHIJ                                                    
SE3515SHIPPER ADDRESS                                                           
SE36                                                           US               
40  001 AUAU020422        0000000050602670000009000    N                        
44COMMERCIAL DESCRIPTION                                                        
47MTHLIATHA191NAK                                                               
47S91-013199000                                                                 
SE50MF MR MANUFACTURER                                                          
SE5515                                                                          
SE56                                                           AU               
SE50PK TEST PACKAGER                                                            
SE5515                                                                          
SE56                                                           US               
504421909720 0000033000 0000010000 000000007000GR                               
6249900003464                                                                   
6250100001250                                                                   
40  002 AUAU020422        0000000000602670000000000    N                        
47MTHLIATHA191NAK                                                               
47S91-013199000                                                                 
SE50PK TEST PACKAGER                                                            
SE5515                                                                          
SE56                                                           US               
508471704065 0000000000 0000015000 000000000100NO                               
6249900005196                                                                   
6250100001875                                                                   
40  003 HKAU020422        0000000000602670000000000    N                        
47MTHLIATHA191NAK                                                               
47S91-013199000                                                                 
SE50PK TEST PACKAGER                                                            
SE51GLN 1234567890123                                                           
SE5515PACKAGER ADDRESS                                                          
SE56                                                           US               
507201000000 0000000000 0000010000 000000000100                                 
6249900003464                                                                   
6250100001250                                                                   
894990000001212450100000004375                                                  
9000000033000 00000016499 00000000000 00000000000 00000000000 00000000000       
Y  3902XJ5AE", message.EM_FormattedMessageText);
			}
		}

		protected override void SetDeclarationDataApplicationSpecific(JobDeclaration declaration)
		{
			var newFactory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3902", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTransportModeForCusCodeList(code.PK, TransportTypeList.Codes.Sea);
			var attributeNameUnlading = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Unlading, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNameUnlading.ZXE_Name, "Y");

			newFactory.Save();

			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "SV9";
			declaration.US_SchDEntry = "3902";
			declaration.US_SchDArrival = "3902";
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_CargoReleaseType = CertificationOptionsList.Codes.ACS;

			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(5);

			while (declaration.US_PreliminaryStatementPrintDateInfo.HasMessageError(WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHoliday) ||
				declaration.US_PreliminaryStatementPrintDateInfo.HasMessageError(WeekendsAndHolidaysValidator.DateCannotBeSetOnPublicHolidayOrWeekend))
			{
				declaration.US_PreliminaryStatementPrintDate = declaration.US_PreliminaryStatementPrintDate.AddDays(1);
			}

			declaration.US_ClientBranchDesignation = ZString.Empty;
			declaration.JE_OA_SoldToPartyAddress = declaration.IOR != null ? declaration.IOR.MainAddress.PK : ZGuid.Empty;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
		}

		protected override ZString GetFinalMessageErrorsToCheck(ZString messageError)
		{
			ZString result = base.GetFinalMessageErrorsToCheck(messageError);

			// For ACE certification, Customs has not provided valid MIDs for all countries
			result = result.Replace("Message Error - JI_OA_ManufacturerAddress: For textile tariffs, the Manufacturer ID (MID) must be from the Country of Origin of the goods. Please select a Manufacturer on the Invoice Header level with the correct MID or select a Manufacturer on the invoice line level, (on lines with textile tariffs, to match the Country of Origin on that line), with the correct MID or select a Supplier with a correct MID.", "");
			result = result.Replace("Message Error - JI_OA_ManufacturerAddress: For textile tariffs, the Manufacturer ID (MID) must be from the Country of Origin of the goods.", "");
			result = result.Replace("Message Error - JI_Tariff: For a Textile Entry the MID must match the Country of Origin.", "");
			result = result.Replace("Message Error - US_FDAManufacturerAddress: For Prior Notice, the Manufacturer ID (MID) must be from the Production Country of Origin of the goods.", "");
			result = result.Replace("Manufacturer Address: For textile tariffs, the Manufacturer ID (MID) must be from the Country of Origin of the goods. Please select a Manufacturer on the Invoice Header level with the correct MID or select a Manufacturer on the invoice line level, (on lines with textile tariffs, to match the Country of Origin on that line), with the correct MID or select a Supplier with a correct MID.", "");
			result = result.Replace("Country Of Origin: For textile tariffs, the Manufacturer ID (MID) must be from the Country of Origin of the goods.", "");
			result = result.Replace("Tariff: For a Textile Entry the MID must match the Country of Origin.", "");
			result = result.Replace("FDA Manufacturer Address: For Prior Notice, the Manufacturer ID (MID) must be from the Production Country of Origin of the goods.", "");

			return result;
		}

		protected override void PopulateMessage(CusEntryHeader entryHeader, bool certifyCargoRelease)
		{
			ACEEntrySummaryMessageBuilder messageBuilder = new ACEEntrySummaryMessageBuilderForTesting(entryHeader, certifyCargoRelease, true, UpdateActionCode.Add);
			MQEDIMessage message = messageBuilder.PopulateMessage();
			entryHeader.Messages.Add(message);
		}

		protected override void SetUp()
		{
			SetUpRefData();
			base.SetUp();

			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();

			mockDeclaration.Protected().Setup<ZString>("ProcessingDistrictPortCore").Returns("3902");

			manufacturerCode.OK_CustomsRegNo = "THLIATHA191NAK";
			importerCustomsCode.OK_CustomsRegNo = "91-013199000";

			this.invoiceHeader.US_FirstSale = YesNoDefaultList.Codes.Yes;
			this.declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			this.declaration.RecalculateValidationModesOnDeclaration();
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
		}

		protected override void FinalTearDown()
		{
			base.FinalTearDown();

			if (manufacturerCode != null)
			{
				manufacturerCode.OK_CustomsRegNo = "THLIATHA191NAK";
			}

			if (importerCustomsCode != null)
			{
				importerCustomsCode.OK_CustomsRegNo = "91-013199000";
			}
		}

		void SetupAirCarrierAndTariff(ZString reference)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var port = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3901", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTransportModeForCusCodeList(port.PK, TransportTypeList.Codes.Air);
			var attributeNameUnlading = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Unlading, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			helper.CreateNewOrGetExistingCusCodeListAttribute(port.PK, attributeNameUnlading.ZXE_Name, "Y");

			Factory.Save();

			declaration.US_SchDArrival = "3901";
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.JE_VoyageFlightNo = "RF432";
			AssertEquals("PreCondition", TransportModeCodes.Codes.AirNonContainer, declaration.JE_Calc_USTransportMode);

			scacCode.OK_CustomsRegNo = "*F";
			declaration.US_UI_NKCarrierSCAC = "*F";
			var carrier = Factory.LoadTop1<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_Code, "*F"));
			carrier.UI_ModeOfTransportation = "40";
			declaration.PrimaryMasterBill.US_UI_NKBillIssuerSCAC = "*F";

			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			invoiceLine.JI_Tariff = "3506990000";
			invoiceLine.JI_CustomsQuantity = 10m;//tariff requires customs quantity
			MergeAndSend(reference);
		}

		void SetUpADDCVDCaseIfTestingNotSendingToCustoms(ZString acCaseNumber, ZDateTime caseDate, ZString countryCode, ZString tariff, ZDecimal adValoremRate, ZDecimal specificRate)
		{
			SetUpADDCVDCaseIfTestingNotSendingToCustoms(acCaseNumber, caseDate, countryCode, tariff, adValoremRate, specificRate, "AC", "START", ZDateTime.BrettsBirthday);
		}

		void SetUpADDCVDCaseIfTestingNotSendingToCustoms(ZString acCaseNumber, ZDateTime caseDate, ZString countryCode, ZString tariff, ZDecimal adValoremRate, ZDecimal specificRate, ZString caseStatus, ZString suspensionAction, ZDateTime suspensionEffectiveDate)
		{
			if (!sendTestMessagesToCustoms)
			{
				var acCase = Factory.New<USCACCase>();
				acCase.U5_CaseNumber = acCaseNumber;
				acCase.U5_CaseStatus = caseStatus;
				acCase.U5_CaseStatusDate = caseDate;
				acCase.U5_ISOCountryCode = countryCode;

				var suspension1 = acCase.LiqSuspensions.AddNew();
				suspension1.UN_Action = suspensionAction;
				suspension1.UN_EffectiveDate = suspensionEffectiveDate;

				var acCaseTariff = acCase.CaseTariffs.AddNew();
				acCaseTariff.U9_TariffNumber = tariff;

				var acCaseRate = acCase.CaseRates.AddNew();
				acCaseRate.U6_AdValoremRate = adValoremRate;
				acCaseRate.U6_SpecificRate = specificRate;
				acCaseRate.U6_EffectiveDate = caseDate;
				Factory.Save();
			}
		}

		JobDeclaration GetMergedDeclaration(string referenceNumber)
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
			declaration.JE_DeclarationReference = referenceNumber;
			declaration.US_EnableENS = true;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;
			invoiceHeader.JZ_InvoiceAmount = 15000m;

			declaration.JE_MasterBill = "OBL10000";
			declaration.US_BondWaiverCode = "996";

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "0709902000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 105000m;
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_Weight = 10m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			return declaration;
		}

		MQEDIMessage GetReceivedMessage(string messageType, string status = EDIMessage.Status.Queued)
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = status;
			message.EM_MessageType = messageType;
			return message;
		}

		void SetUpDataForLaceyAct()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMP" + new Random().Next(1000000).ToString();
			importer.MainAddress.OA_Address1 = "IMPORTER STREET 1";
			importer.MainAddress.OA_Address2 = "IMPORTER STREET 2";
			importer.MainAddress.OA_Email = "importer@test.com";
			importer.MainAddress.OA_Phone = "6934568700";
			importer.MainAddress.OA_Fax = "";
			importer.MainAddress.OA_PostCode = "96358";
			importer.MainAddress.OA_RL_NKRelatedPortCode = "USLAX";
			importer.MainAddress.OA_City = "LOS ANGELES";
			importer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "936528466", Core.Constants.CountryCodes.UnitedStates);
			DeclarationTestHelper.AddPGAContact(importer, "JOHN Q", "IMPORTER", "3015552232", "JQI@imp.com", null);

			declaration.IOROrgPK = importer.PK;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndImporter;
			declaration.US_PreliminaryStatementPrintDate = new ZDateTime(2015, 05, 20);
			declaration.US_CertifyCargoRelease = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			var seller = Factory.New<OrgHeader>();
			seller.OH_FullName = "Seller for Inv Header 2";
			seller.OH_Code = "FE" + new Random().Next(1000000).ToString();
			seller.MainAddress.OA_Address1 = "123/33 Street 1";
			invoiceLine.InvoiceHeader.JZ_OA_SellerAddress = seller.MainAddress.PK;

			invoiceLine.US_LaceyIndicator = OGAIndicatorList.Codes.Declared;
		}

		void SetUpRefData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffHelper = new USCTariffTestCase(Factory);
			var startDate = ZDateTime.MinSmallDateTimeValue.Date;
			var endDate = ZDateTime.MaxSmallDateTimeValue.Date;

			var jpCountry = Factory.LoadTop1<USCCountry>(new ZQuery(USCCountrySchema.UC_Code, "JP"));
			if (jpCountry == null)
			{
				jpCountry = Factory.New<USCCountry>();
				jpCountry.UC_Code = "JP";
			}

			jpCountry.UC_SpecialTradeProgramsIndicator = "D";
			jpCountry.UC_SpecialTradeProgramsBeginDate = ZDateTime.Empty;
			jpCountry.UC_SpecialTradeProgramsEndDate = ZDateTime.Empty;

			var hsnTariffType = helper.CreateNewOrGetExistingTariffType("US", "HSN");
			var dutyRateType = helper.CreateNewOrGetExistingRateType("US", "DTY", "Duty");
			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "DTY", dutyRateType.PK);
			var tradeGroup = helper.CreateTradeGroup("JP", "JP", startDate, endDate);
			helper.AddCountry(tradeGroup, "JP", startDate, endDate);
			helper.AddCountry(tradeGroup, "AU", startDate, endDate);
			helper.AddCountry(tradeGroup, "ZA", startDate, endDate);
			helper.AddCountry(tradeGroup, "CA", startDate, endDate);
			Factory.Save();

			helper.CreateTariff("US", hsnTariffType.PK, "6404191560", startDate, endDate);
			var tariff9903 = helper.CreateTariff("US", hsnTariffType.PK, "99034110", startDate, endDate);
			tariffHelper.CreateNewTariffIfNotExists("6404191560", startDate, endDate, "KG", "", "", "");
			var us9903 = tariffHelper.CreateNewTariffIfNotExists("99034110", startDate, endDate, "KG", "", "", "");
			us9903.UE_AdditionalTariffNumberIndicator = ZBool.True;
			var rate9903 = helper.CreateRate(tariff9903, rateCode.PK, startDate, endDate, "0.25");
			helper.CreateCusApplicability(rate9903, tradeGroup, startDate, endDate);
			helper.CreateTariffRelationship(tariff9903.PK, hsnTariffType.PK, "64");
			helper.CreateTariffAttribute("RULE", "A99", tariff9903);

			helper.CreateTariff("US", hsnTariffType.PK, "6204624021", startDate, endDate);
			var tariff9819 = helper.CreateTariff("US", hsnTariffType.PK, "98191124", startDate, endDate);
			tariffHelper.CreateNewTariffIfNotExists("6204624021", startDate, endDate, "KG", "", "", "");
			var us9819 = tariffHelper.CreateNewTariffIfNotExists("98191124", startDate, endDate, "KG", "", "", "");
			us9819.UE_AdditionalTariffNumberIndicator = ZBool.True;
			var rate9819 = helper.CreateRate(tariff9819, rateCode.PK, startDate, endDate, "0.25");
			helper.CreateCusApplicability(rate9819, tradeGroup, startDate, endDate);
			helper.CreateTariffRelationship(tariff9819.PK, hsnTariffType.PK, "6204");
			helper.CreateTariffAttribute("RULE", "A99", tariff9819);

			helper.CreateTariff("US", hsnTariffType.PK, "8703330045", startDate, endDate);
			var tariff9817 = helper.CreateTariff("US", hsnTariffType.PK, "98178501", startDate, endDate);
			tariffHelper.CreateNewTariffIfNotExists("8703330045", startDate, endDate, "KG", "", "", "");
			var us9817 = tariffHelper.CreateNewTariffIfNotExists("98178501", startDate, endDate, "KG", "", "", "");
			us9817.UE_AdditionalTariffNumberIndicator = ZBool.True;
			var rate9817 = helper.CreateRate(tariff9817, rateCode.PK, startDate, endDate, "0.25");
			helper.CreateCusApplicability(rate9817, tradeGroup, startDate, endDate);
			helper.CreateTariffRelationship(tariff9817.PK, hsnTariffType.PK, "87");
			helper.CreateTariffAttribute("RULE", "A99", tariff9817);

			helper.CreateTariff("US", hsnTariffType.PK, "9102111010", startDate, endDate);
			var tariff9802 = helper.CreateTariff("US", hsnTariffType.PK, "9802004040", startDate, endDate);
			var us9102 = tariffHelper.CreateNewTariffIfNotExists("9102111010", startDate, endDate, "KG", "", "", "");
			us9102.UE_SPICode = "AU";
			var us9802 = tariffHelper.CreateNewTariffIfNotExists("9802004040", startDate, endDate, "KG", "", "", "");
			us9802.UE_AdditionalTariffNumberIndicator = ZBool.True;
			us9802.UE_SPICode = "AU";
			var rate9802 = helper.CreateRate(tariff9802, rateCode.PK, startDate, endDate, "0.25");
			helper.CreateCusApplicability(rate9802, tradeGroup, startDate, endDate);
			helper.CreateTariffRelationship(tariff9802.PK, hsnTariffType.PK, "9102");
			helper.CreateTariffAttribute("RULE", "A99", tariff9802);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			var foreignPort = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "60267", "Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			helper.CreateNewOrGetExistingCusCodeListAttribute(foreignPort.PK, RefCusCodeListAttributeTypes.Codes.PortValidType, ForeignPortTypeList.Codes.Common);
			Factory.Save();
		}

		void CreateNewOrGetExistingCusCodeListForFIRMSType(ZString code)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, "FIRMS");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, code, "Test Name", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			Factory.Save();
		}
	}
}
