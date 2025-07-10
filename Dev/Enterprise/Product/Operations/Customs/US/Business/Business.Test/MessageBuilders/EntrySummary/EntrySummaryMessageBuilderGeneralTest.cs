using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class EntrySummaryMessageBuilderGeneralTest : EntrySummaryMessageBuilderAbstractTest
	{
		public void TestMailEntryShouldHave22Block()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.US_EnableENS = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.Mail;
			declaration.JE_MasterBill = "012589633";
			declaration.JE_HouseBill = "12121212";
			declaration.JE_TotalNoOfPacks = 1;
			declaration.JE_TotalNoOfPacksPackType = "PC";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "N111";
			var invoiceline = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 100m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var ensMessage = new EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, true).PopulateMessage();
			ENS22 ens22 = (ENS22)ensMessage.MessageBlock.MessageBlocks.Find(x => x.MandatoryCharacters == "22");
			AssertNotNull("ens 22 was created", ens22);
		}

		public void TestZoneStatusSentForFTZOnly()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 5000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 5000.00m;
			invoiceLine.JI_Tariff = "1902.19.40 00";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.PrivilegedForeign;
			invoiceLine.US_PrivilegedStatusDate = new ZDateTime(2009, 1, 1);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			MQEDIMessage ensMessage = new EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, true).PopulateMessage();

			ENS40 ens40 = (ENS40)ensMessage.MessageBlock.MessageBlocks.Find(x => x.MandatoryCharacters == "40");
			AssertEquals("", ens40.ZoneStatus);
			AssertEquals(ZDate.Empty, ens40.PrivilegedStatusFilingDate);

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableENS = true;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			ensMessage = new EntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, UpdateActionCode.Add, true).PopulateMessage();

			ens40 = (ENS40)ensMessage.MessageBlock.MessageBlocks.Find(x => x.MandatoryCharacters == "40");
			AssertEquals(ZoneStatusList.Codes.PrivilegedForeign, ens40.ZoneStatus);
			AssertEquals(new ZDate(2009, 1, 1), ens40.PrivilegedStatusFilingDate);
		}

		[TestDate(2016, 03, 27)]
		public void TestInvoiceSequenceInENS40()
		{
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;

			JobComInvoiceHeader invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice2.JZ_InvoiceAmount = 20000m;
			invoice2.JZ_OH_Supplier = manufacturer.PK;
			invoice2.US_TransactionsRelated = YesNoDefaultList.Codes.No;
			invoice2.JZ_InvoiceNumber = "87324";
			invoice2.Charges.AddNew("OFT", 50m, "USD");
			invoice2.US_UC_NKCountryOfOrigin = "AU";
			invoice2.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			AssertEquals("InvoiceSequence is set", (short)1, invoiceHeader.JZ_InvoiceDisplaySequence);
			AssertEquals("InvoiceSequence is set", (short)2, invoice2.JZ_InvoiceDisplaySequence);

			JobComInvoiceLine invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 10000m;
			invoiceLine2.JI_Tariff = "8483.40.5010";
			invoiceLine2.JI_CustomsQuantity = 1m;
			invoiceLine2.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Japan;
			invoiceLine2.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.Japan;
			invoiceLine2.JI_Description = "Goods";
			invoiceLine2.US_SPI = SPICompleteList.MoreCodes.NotApplicable;

			OrgHeader manufacturer1 = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer1.OH_Code = "MAN" + new Random().Next(1000000).ToString();
			manufacturer1.OH_FullName = "MR MANUFACTURER";
			manufacturer1.OH_IsConsignor = true;
			manufacturer1.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "JPMIYIND441TOY");
			invoiceLine2.JI_OA_ManufacturerAddress = manufacturer1.MainAddress.PK;

			JobComInvoiceLine invoiceLine3 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 10000m;
			invoiceLine3.JI_Tariff = "8483.40.5010";
			invoiceLine3.JI_CustomsQuantity = 1m;
			invoiceLine3.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.KoreaSouth;
			invoiceLine3.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.KoreaSouth;
			invoiceLine3.JI_Description = "Goods";
			invoiceLine3.US_SPI = SPICompleteList.MoreCodes.NotApplicable;

			OrgHeader manufacturer2 = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer2.OH_Code = "MAN" + new Random().Next(1000000).ToString();
			manufacturer2.OH_FullName = "MR MANUFACTURER";
			manufacturer2.OH_IsConsignor = true;
			manufacturer2.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "KRJUHCOR3325KIM");
			invoiceLine3.JI_OA_ManufacturerAddress = manufacturer2.MainAddress.PK;

			MergeAndSend("InvSeq in ENS40");

			MQEDIMessage message = (MQEDIMessage)declaration.CustomsEntryHeaders[0].Messages[0];

			List<MessageBlock> result = message.MessageBlock.MessageBlocks.FindAll(x => x.MandatoryCharacters == "40");
			AssertEquals(3, result.Count);

			ENS40 ens40 = (ENS40)result[0];
			AssertEquals("first line should have a delimiter as it is the last line item", "INV001", ens40.InvoiceDelimiter);

			ens40 = (ENS40)result[1];
			AssertEquals("second line should not have a delimiter as it is not the last line item of the invoice", "", ens40.InvoiceDelimiter);

			ens40 = (ENS40)result[2];
			AssertEquals("INV002", ens40.InvoiceDelimiter);

			invoice2.Delete();
			MergeAndSend("InvSeq in ENS40");

			message = (MQEDIMessage)declaration.CustomsEntryHeaders[0].Messages[1];
			result = message.MessageBlock.MessageBlocks.FindAll(x => x.MandatoryCharacters == "40");
			AssertEquals(1, result.Count);
			ens40 = (ENS40)result[0];
			AssertEquals("first line should not have any delimiter as there is only one invoice", "", ens40.InvoiceDelimiter);
		}

		[TestDate(2016, 03, 27)]
		public void TestWhenMPFIsLessThanPointZeroOne()
		{
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.No;
			invoiceLine.JI_LinePrice = 0.25m;
			invoiceLine.JI_Tariff = "8483.40.5010";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Japan;//MPF required.
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("CusEntryLine Fee", 0m, invoiceLine.CusEntryLine.Fees.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));
			AssertEquals(true, invoiceLine.CusEntryLine.US_HasMPF);
			AssertEquals("Minimum has been calculated", 25m, invoiceLine.CusEntryLine.Header.Charges.GetAmount(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing));

			MergeAndSend("MPF less than 0.01$");

			helper.MessageMustContainElement<ENS89>(declaration);
		}

		[TestDate(2016, 03, 27)]
		public void TestForHandCarry()
		{
			declaration.JE_TransportMode = TransportTypeList.Codes.PassengerHandCarried;
			invoiceLine.JI_Tariff = "8483.40.5010";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Japan;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			declaration.JE_MasterBill = "QF00002356";
			declaration.JE_VoyageFlightNo = "QF214";
			declaration.JE_HouseBill = "Hand Carry";
			declaration.PrimaryHouseBill.CU_NoOfPacks = 1;
			declaration.PrimaryHouseBill.CU_PackType = "PK";
			declaration.JE_MasterBillIssuerSCAC = "";

			MergeAndSend("HandCarry");

			var message = (MQEDIMessage)declaration.CustomsEntryHeaders[0].Messages[0];
			var ens20 = message.MessageBlock.MessageBlocks.OfType<ENS20>().FirstOrDefault();
			AssertNotNull(ens20);
			AssertEquals("For HandCarry, Customs requires Flight No and MasterBill", "QF214", ens20.VoyageFlightTripManifestNumber);
		}

		[TestDate(2008, 12, 31)]
		public void Test05DutyFree()
		{
			/*05) 99025115                    TARIFF NUMBER 1
						5112113060                  TARIFF NUMBER 2
						W#####A##                   WOOL LICENSE NUMBER
			 
						FILL IN NUMBERS IN LICENSE NUMBER WHERE # APPEAR*/

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionQuotaVisa;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.No;
			declaration.JE_RL_NKPortOfLoading = "SGSIN";
			declaration.US_SchDLoading = "55900";
			invoiceLine.US_SupTariff = "99025115";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_WoolLicenceNo = "W" + (ZDate.Today.Year % 100).ToString().PadLeft(2, '0') + "345A67";
			invoiceLine.US_UC_NKCountryOfOrigin = "SG";
			invoiceLine.US_SPI = SpecialProgramList.Codes.SG;
			invoiceLine.JI_Tariff = "5112113060";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 3000m;
			invoiceLine.JI_CustomsSecondQuantity = 100m;
			invoiceLine.JI_CustomsQuantity = 555.56m;
			invoiceLine.JI_Weight = 10m;
			manufacturer.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "SGBEREQU6LON", Core.Constants.CountryCodes.UnitedStates);
			MergeAndSend("500");
		}

		[TestDate(2016, 03, 27)]
		public void TestOGADataInCorrectSequence()
		{
			declaration.US_EnableCRL = true;
			declaration.US_SchDLoading = "55900";
			declaration.JE_RL_NKPortOfLoading = "SGSIN";
			declaration.JE_RL_NKOrigin = "SGSIN";
			declaration.US_EntryDate = ZDateTime.Today;
			declaration.US_US_NKLocationOfGoods = "J123";
			declaration.US_EstimatedEntryDate = new ZDateTime(2016, 3, 19);

			invoiceLine.US_UC_NKCountryOfOrigin = "SG";
			invoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewMayBeRequiredTariff;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.ImportTariff.UE_OGACodes = "FD1FC3";
			invoiceLine.US_SPI = SpecialProgramList.Codes.SG;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;

			MergeAndSend("OGASequence", true, false);

			CusEntryHeader[] entries = declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.EntrySummary);
			AssertEquals(1, entries.Length);
			CusEntryHeader ensEntry = entries[0];
			AssertEquals("Messages", 1, ensEntry.Messages.Count);
			MQEDIMessage message = (MQEDIMessage)ensEntry.Messages[0];
			string oGASegmentsExpected = @"OA  FD0                                                                         60                                        AUSOUPAC195PAD                        ";
			Assert("Disclaimed OGA data must be immediately after tariff & before declared OGA data", message.EM_MessageText.Contains(oGASegmentsExpected));
		}

		[TestDate(2010, 8, 9)]
		public void TestPGADataBeforeOGAData()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableCRL = true;
			declaration.US_SchDLoading = "55900";
			declaration.JE_RL_NKPortOfLoading = "SGSIN";
			declaration.JE_RL_NKOrigin = "SGSIN";
			declaration.US_EntryDate = ZDateTime.Today;
			declaration.US_US_NKLocationOfGoods = "J123";
			declaration.US_EstimatedEntryDate = new ZDateTime(2010, 3, 19);

			invoiceLine.US_UC_NKCountryOfOrigin = "SG";
			invoiceLine.JI_Tariff = USCTariff.FDAAdmissibilityReviewMayBeRequiredTariff;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.ImportTariff.UE_OGACodes = "FD1FC3";
			invoiceLine.US_SPI = SpecialProgramList.Codes.SG;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;

			var dot = invoiceLine.DOTs.AddNew();
			dot.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._05;
			dot.US_DOTCommercialDesc = "O VH,>4<=6CYL,IN VL>2.8<=3";
			dot.US_DOTClarCode = ClarificationCodeList.Codes.Vehicle;
			dot.US_DOTPassport = "PASS";

			var dotVin = dot.DOTVINs.AddNew();
			dotVin.US_DOTYear = 1999;
			dotVin.US_DOTVIN = "IEWR87EWRKJWER87";
			dotVin.US_DOTMake = "MAKE1";
			dotVin.US_DOTModel = "LANTRA";

			var pGA = invoiceLine.LaceyActLines.AddNew();
			pGA = DeclarationTestHelper.CreateLaceyActData(pGA);
			pGA.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
			pGA.US_PGAContactEmail = "Test@abc.com";
			pGA.US_PGAContactName = "C Name";
			pGA.US_PGAContactPhoneNo = "2584122225";

			MergeAndSend("PGAAndOGASequence", true, false);
			helper.MessageMustContainElement<PGAPG01>(declaration);
			helper.MessageMustContainElement<OGADT01>(declaration);

			var entries = declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.EntrySummary);
			AssertEquals(1, entries.Length);
			var ensEntry = entries[0];
			AssertEquals("Messages", 1, ensEntry.Messages.Count);
			var message = (MQEDIMessage)ensEntry.Messages[0];
			AssertEquals("PGA blocks should be before other OGA blocks & Disclaiming blocks before all OGA declaring blocks",
				@"B018888XJ5EI                                               EDIEDIDAT_1          
10A888891-01319900091-013199000                 8031910   XJ5 0000001401891  IL 
20     APL EMERALD         108888080910                     V123W080910J123     
22            OBLPGAANDOGA                        00000001PK         AAAA       
30                                  01              1                   AAAA    
40001SG00000100000000009000                    000000005055976                  
50 2853000095          000000000100KG                               SG080210NSG 
OA  FD0                                                                         
OI        SOFTWOOD PULPWOOD                                                     
PG01001AP                                                                       
PG04 SPRUCE                                              000000010000M3   020000
PG05PICEA                 GLAUCA                                                
PG06HRVCA                                                                       
PG25                                               000000010000                 
OI        O VH,>4<=6CYL,IN VL>2.8<=3                                            
DT0100105YPASS               SG     V                                           
DT02MAKE1          LANTRA         1999IEWR87EWRKJWER87                          
60                                        AUSOUPAC195PAD                        
90                      0                                  00000010000          
Y  8888XJ5EI00018", message.EM_FormattedMessageText);
		}

		[TestDate(2010, 8, 9)]
		public void TestOGADisclaimingAndDeclaringBlocksForSecondaryLines()
		{
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableENS = true;
			declaration.JE_RL_NKPortOfLoading = "SGSIN";
			declaration.JE_RL_NKOrigin = "SGSIN";
			declaration.US_EntryDate = ZDateTime.Today;
			declaration.US_US_NKLocationOfGoods = "J123";
			declaration.US_EstimatedEntryDate = new ZDateTime(2010, 3, 19);

			invoiceLine.US_UC_NKCountryOfOrigin = "SG";
			invoiceLine.JI_Tariff = "2401.10.6590";
			invoiceLine.ImportTariff.UE_OGACodes = "FD1";
			invoiceLine.US_SupTariff = "9910.24.05";
			invoiceLine.JI_CustomsQuantity = 10m;
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_SPI = SpecialProgramList.Codes.SG;
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;

			var pGA = invoiceLine.LaceyActLines.AddNew();
			pGA = DeclarationTestHelper.CreateLaceyActData(pGA);
			pGA.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
			pGA.US_PGAContactEmail = "Test@abc.com";
			pGA.US_PGAContactName = "C Name";
			pGA.US_PGAContactPhoneNo = "2584122225";

			MergeAndSend("PGAAndOGASequence", true, false);
			helper.MessageMustContainElement<PGAPG01>(declaration);

			var entries = declaration.ActiveEntryHeaders.GetEntryWithType(ImportMessageStatusList.MessageType.EntrySummary);
			AssertEquals(1, entries.Length);
			var ensEntry = entries[0];
			AssertEquals("Messages", 1, ensEntry.Messages.Count);
			var message = (MQEDIMessage)ensEntry.Messages[0];

			Assert(message.EM_FormattedMessageText.Contains(@"702401106590           000000001000KG                                         SG
OA  FD0                                                                         
OI        SOFTWOOD PULPWOOD                                                     
PG01001AP                                                                       
PG04 SPRUCE                                              000000010000M3   020000
PG05PICEA                 GLAUCA                                                
PG06HRVCA                                                                       
PG25                                               000000010000                 "));
		}

		[TestDate(2009, 6, 14)]
		public void TestRemoteLocationFilingEntry()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var port1 = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1101", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			var port2 = refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "2704", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			refHelper.CreateTransportModeForCusCodeList(port1.PK, TransportTypeList.Codes.Sea);
			var attributeNameUnlading = refHelper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Unlading, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			refHelper.CreateNewOrGetExistingCusCodeListAttribute(port1.PK, attributeNameUnlading.ZXE_Name, "Y");
			Factory.Save();

			const string CorrectPreparerPortCode = "8888";

			declaration.US_SchDEntry = "1101";
			declaration.US_SchDArrival = "1101";
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			declaration.US_IsInvoiceByRequest = true;
			declaration.US_PaymentType = PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode;
			declaration.US_PreliminaryStatementPrintDate = ZDateTime.Today.AddDays(2);
			declaration.US_EntryFilerCode = "BAS";
			declaration.US_PreparerDistrictPort = CorrectPreparerPortCode;
			declaration.US_EntryFilerCode = "BAS";
			declaration.US_PreparerOfficeCode = "13";
			declaration.US_SchDExam = "2704";
			declaration.US_CertifyCargoRelease = true;
			declaration.US_US_NKLocationOfGoods = "F629";
			declaration.US_EntryDate = ZDateTime.Now;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;

			MergeAndSend("180971");
			ENS30 ens30 = helper.GetTypes<ENS30>(declaration)[0];
			AssertEquals("2704", ens30.DesignatedExamPort);

			APLB aplb = helper.GetTypes<APLB>(declaration)[0];
			AssertEquals("1101", aplb.ProcessingDistrictPortCode);
			AssertEquals(CorrectPreparerPortCode, aplb.PreparerDistrictPort);
			AssertEquals("BAS", aplb.PreparerFilerCode);
			AssertEquals("13", aplb.PreparerOfficeCode);
		}

		[TestDate(2016, 03, 27)]
		public void TestNonRLFWhereStatementProcessingPortEqualPortofEntryNonRLFisYes()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1101", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			USCustomsDataRegistry.Instance.StatementProcessingPortEqualPortofEntryNonRLF.SetValue(Guid.Empty, declaration.Branch.PK.ToGuid(), Guid.Empty, true);

			declaration.US_SchDEntry = "1101";
			declaration.US_EntryFilerCode = "BRP";
			declaration.US_US_NKLocationOfGoods = "F629";
			declaration.US_ITDate = ZDateTime.Now;
			declaration.JE_PrimaryITNumber = "111271845";
			declaration.US_EntryDate = ZDateTime.Now;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			MergeAndSend("180971");

			APLB aplb = helper.GetTypes<APLB>(declaration)[0];
			AssertEquals("1101", aplb.ProcessingDistrictPortCode);
		}

		[TestDate(2016, 03, 27)]
		public void TestNonRLFWhereStatementProcessingPortEqualPortofEntryNonRLFisNo()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1101", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			USCustomsDataRegistry.Instance.StatementProcessingPortEqualPortofEntryNonRLF.SetValue(Guid.Empty, declaration.Branch.PK.ToGuid(), Guid.Empty, false);

			declaration.US_SchDEntry = "1101";
			declaration.US_EntryFilerCode = "BRP";
			declaration.US_US_NKLocationOfGoods = "F629";
			declaration.US_EntryDate = ZDateTime.Now;
			declaration.US_ITDate = ZDateTime.Now;
			declaration.JE_PrimaryITNumber = "111271845";
			MergeAndSend("180971");

			APLB aplb = helper.GetTypes<APLB>(declaration)[0];
			AssertEquals("8888", aplb.ProcessingDistrictPortCode);
		}

		public void TestExWarehouseEndToEndTest()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_EnableENS = true;

			declaration.JE_ExportDate = new ZDateTime(2007, 07, 01);
			declaration.US_EstimatedEntryDate = new ZDateTime(2007, 07, 10);
			declaration.US_SchDEntry = "8888";
			declaration.US_DestinationState = "DC";
			declaration.US_US_NKLocationOfGoods = "Z104";
			declaration.US_WHSDistrictPortCode = "8888";
			declaration.US_WHSEntryFilerCode = "XJ5";
			declaration.US_WHSEntryNumber = "10000103";

			OrgHeader importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_Code = "IMP" + new Random().Next(1000000).ToString();
			importer.OH_FullName = "Mr Importer";
			importer.OH_IsConsignee = true;
			importer.OH_RL_NKClosestPort = "USCHI";
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "91-013199000");

			declaration.JE_OH_Importer = importer.PK;
			declaration.IOROrgPK = importer.PK;
			declaration.US_PaymentType = PaymentTypeList.Codes.IndividualBasis;
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			declaration.US_SuretyCode = "891";

			declaration.US_SchDArrival = "";
			declaration.JE_RL_NKPortOfLoading = "AU";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 6000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			OrgHeader manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_Code = "MAN" + new Random().Next(1000000).ToString();
			manufacturer.OH_FullName = "Mr Manufacturer";
			manufacturer.OH_IsConsignor = true;
			manufacturer.OH_RL_NKClosestPort = "AUSYD";
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "ECPEFCIA113MAN");

			invoice.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			invoice.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 100m);

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2208204000";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_LinePrice = 6000m;
			invoiceLine.JI_CustomsQuantity = 600m;
			invoiceLine.JI_Weight = 10m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.RunPreSaveValidation();
			declaration.DoMerge();

			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			entry.CH_BGMReference = "General EXW End To End";
			MQEDIMessage message = new EntrySummaryMessageBuilder(declaration.CustomsEntryHeaders[0], UpdateActionCode.Add, false).PopulateMessage();
			AssertMultilineASCIIEquals("Ex-Warehouse message generated",
@"B018888XJ5EI                                               <<MSGNO PLACEHOLDER>>
10A888891-01319900091-013199000                 8071007   XJ5 <E#PLCH>31891  DC 
20                                                                     Z104     
30                  XJ510000103888800               1                           
40001AU00000060000000000010                    0000000100                       
50 2208204000          000000060000PFL                              AU070107N   
60                                        ECPEFCIA113MAN  0000213979            
62          49900001260                                                         
8949900000002500                                                                
90           000002139790                       0000000250000000006000          
Y  8888XJ5EI00009            000000213979"

				, message.EM_FormattedMessageText);
			Factory.Save();
		}

		public void TestFDALineNumberForMultiEntryLines()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;//sends FDA details

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 5000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 5000.00m;
			invoiceLine.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;
			invoiceLine.JI_Tariff = "1902.19.40 00";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			declaration.JE_MasterBill = "MULTENTRYLINE";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);
			invoiceLine.FDAs[0].US_FDAProductCode = "04AVN12";

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 3000m;
			invoiceLine2.JI_Tariff = "1902.19.40 00";
			invoiceLine2.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine2.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine2.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			helper.SetUpFDARequiredData(invoiceLine2, null, Factory);
			invoiceLine2.FDAs[0].US_FDAProductCode = "04AVN12";

			JobComInvoiceLine invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_LinePrice = 2000m;
			invoiceLine3.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.V;
			invoiceLine3.JI_Tariff = "0712.31.10 00";
			invoiceLine3.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine3.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			helper.SetUpFDARequiredData(invoiceLine3, null, Factory);
			invoiceLine3.FDAs[0].US_FDAProductCode = "25SVC99";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("One entry", 2, declaration.CustomsEntryHeaders.Count);

			CusEntryHeader ensEntry = declaration.CustomsEntryHeaders.Find(
				new ZQuery(CusEntryHeaderSchema.CH_MessageType, CusEntryHeaderMessageTypeList.Codes.EntrySummary))[0];
			AssertEquals("Three entry lines", 3, ensEntry.MergedLines.Count);

			new EntrySummaryMessageBuilder(ensEntry, UpdateActionCode.Add, true).PopulateMessage();

			AssertEquals("FDA Line No should be assigned from 1 incrementally within each entry line", 1, invoiceLine.FDAs[0].US_FDALineNo);
			AssertEquals("FDA Line No should be assigned from 1 incrementally within each entry line", 1, invoiceLine2.FDAs[0].US_FDALineNo);
			AssertEquals("FDA Line No should be assigned from 1 incrementally within each entry line", 1, invoiceLine3.FDAs[0].US_FDALineNo);

			declaration.JE_MergeBy = MasterFiles.Business.OrgConstants.MergeInvoiceLines.Tariff;

			invoiceLine.US_SecondarySPI = ZString.Empty;
			invoiceLine2.JI_ParentID = ZGuid.Empty;
			invoiceLine3.JI_ParentID = ZGuid.Empty;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("There should be two entry lines", 3, ensEntry.MergedLines.Count);
			new EntrySummaryMessageBuilder(ensEntry, UpdateActionCode.Add, true).PopulateMessage();

			AssertEquals("invoice line 1 should be FDA line1", 1, invoiceLine.FDAs[0].US_FDALineNo);
			AssertEquals("invoice line 2 should be FDA line1", 1, invoiceLine2.FDAs[0].US_FDALineNo);
			AssertEquals("invoice line3 assigned to a different entry line", 1, invoiceLine3.FDAs[0].US_FDALineNo);
		}

		[TestDate(2009, 6, 1)]
		public void TestForOtherExciseTax()
		{
			invoiceLine.JI_Tariff = "2203.00.00 30";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 15000m;
			invoiceLine.JI_Weight = 1000m;
			invoiceLine.US_UC_NKCountryOfExport = "MX";//MPF is exempt
			invoiceLine.US_UC_NKCountryOfOrigin = "MX";
			invoiceLine.ConsigneeOrgAddress.OH_RL_NKClosestPort = "USLAX";
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;
			declaration.JE_MasterBill = "OTHEREXCISETAX";
			helper.SetUpFDARequiredData(invoiceLine, null, Factory);

			MergeAndSend("other excise tax");
			AssertEquals("022 should not be part of ENS89", false, declaration.CustomsEntryHeaders[0].Messages[0].EM_MessageText.Contains("89022"));

			ICusEntryHeader entry = invoiceLine.CusEntryLine.Header;
			AssertEquals("Total Grand Fee:only MPF", 25m, entry.GrandTotalFee);
		}

		[TestDate(2016, 03, 27)]
		public void TestForBSharpSPIForCanada()
		{
			invoiceLine.JI_Tariff = "8512.30.0030";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 52m;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.US_UC_NKCountryOfExport = "CA";//MPF is exempt
			invoiceLine.US_UC_NKCountryOfOrigin = "XO";
			manufacturerCode.OK_CustomsRegNo = "XOUBFFOO2421OAK";
			invoiceLine.US_SPI = "B#";
			MergeAndSend("B# SPI for CA");
			CusEntryHeader entry = invoiceLine.CusEntryLine.Header;
			AssertEquals("Total Grand Fee:MPF is exempt", 0m, entry.GrandTotalFee);
			AssertEquals("Duty free", 0m, entry.TotalDutyAmount);
		}

		[TestDate(2016, 03, 27)]
		public void TestForCSharpSPIForCanada()
		{
			invoiceLine.JI_Tariff = "8483.40.5010";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 150m;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.US_UC_NKCountryOfExport = "CA";//MPF is exempt
			invoiceLine.US_UC_NKCountryOfOrigin = "XO";
			invoiceLine.US_SPI = "C#";
			manufacturerCode.OK_CustomsRegNo = "XOUBFFOO2421OAK";
			MergeAndSend("C# SPI for CA");
			CusEntryHeader entry = invoiceLine.CusEntryLine.Header;
			AssertEquals("Total Grand Fee:MPF is exempt", 0m, entry.GrandTotalFee);
			AssertEquals("Duty free", 0m, entry.TotalDutyAmount);
		}

		[TestDate(2009, 6, 1)]
		public void TestForJAstetickSPI()
		{
			invoiceLine.JI_Tariff = "5804.30.0010";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 5656.00000m;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.US_UC_NKCountryOfExport = "EC";
			invoiceLine.CountryOfExport_US.UC_SPIEndDate = ZDateTime.Empty;
			invoiceLine.US_UC_NKCountryOfOrigin = "EC";
			invoiceLine.US_SPI = "J";
			manufacturer.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "ECBEREQU6LON", Core.Constants.CountryCodes.UnitedStates);
			MergeAndSend("J* SPI");
			CusEntryHeader entry = invoiceLine.CusEntryLine.Header;
			AssertEquals("Total Grand Fee:MPF is not exempt", 25m, entry.GrandTotalFee);
			AssertEquals("Duty free", 0m, entry.TotalDutyAmount);
		}

		[TestDate(2016, 03, 27)]
		public void TestForEAstetickSPI()
		{
			invoiceLine.JI_Tariff = "6211.20.1515";
			invoiceLine.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine.JI_CustomsQuantity = 150.00000m;
			invoiceLine.JI_Weight = 100m;
			invoiceLine.JI_CustomsSecondQuantity = 21.00000m;
			invoiceLine.US_UC_NKCountryOfExport = "VC";
			invoiceLine.US_UC_NKCountryOfOrigin = "VC";
			invoiceLine.US_SPI = "E";
			manufacturer.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "VCBEREQU6LON", Core.Constants.CountryCodes.UnitedStates);
			MergeAndSend("E* SPI");
			CusEntryHeader entry = invoiceLine.CusEntryLine.Header;
			AssertEquals("Total Grand Fee:MPF is exempt", 0m, entry.GrandTotalFee);
			AssertEquals("Duty free", 0m, entry.TotalDutyAmount);
		}

		[TestDate(2016, 03, 27)]
		public void TestSendMPFEvenIfAmountIsZero()
		{
			invoiceLine.JI_LinePrice = 5282m;
			invoiceLine.US_SPI = SPICompleteList.MoreCodes.NotApplicable;

			JobComInvoiceLine invoiceLine2 = invoiceLine.InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "4823.20.9000";
			invoiceLine2.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine2.JI_LinePrice = 1m;
			invoiceLine2.JI_CustomsQuantity = 2m;
			invoiceLine2.JI_Weight = 1m;
			invoiceLine2.US_UC_NKCountryOfOrigin = "KR";

			invoiceLine.InvoiceHeader.JZ_InvoiceAmount = 5283m;

			MergeAndSend("A mandatory MPF");
			MQEDIMessage sentMsg = (MQEDIMessage)invoiceLine.CusEntryLine.Header.Messages[0];

			ABIInputBlockControlGenerator messageBlock = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			messageBlock.Deserialise(BlockPadder.Pad(sentMsg.EM_MessageText));

			bool found62WithZeroMPF = false;
			foreach (MessageBlock block in messageBlock.MessageBlocks)
			{
				if (block is ENS62 && ((ENS62)block).ClassCode == Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing &&
					((ENS62)block).UserFeeAmount == 0m)
				{
					found62WithZeroMPF = true;
					break;
				}
			}

			Assert("Should have created ens62 with zero MPF amount as it is mandatory", found62WithZeroMPF);
		}

		[TestDate(2016, 03, 27)]
		public void TestHMFBecomesExemptWhenTotalFeeIsLessThanThreeDollars()
		{
			invoiceLine.JI_LinePrice = 250m;

			JobComInvoiceLine invoiceLine2 = invoiceLine.InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "4823.20.9000";
			invoiceLine2.JI_Description = "COMMERCIAL DESCRIPTION";
			invoiceLine2.JI_LinePrice = 1m;
			invoiceLine2.JI_CustomsQuantity = 2m;
			invoiceLine2.JI_Weight = 1m;
			invoiceLine2.US_UC_NKCountryOfOrigin = "AU";

			invoiceLine.US_SPI = "AU";
			invoiceLine2.US_SPI = "AU";

			invoiceLine.InvoiceHeader.JZ_InvoiceAmount = 251m;
			invoiceLine.Declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.Yes;

			MergeAndSend("HMF becomes exempt");
			MQEDIMessage sentMsg = (MQEDIMessage)invoiceLine.CusEntryLine.Header.Messages[0];

			ABIInputBlockControlGenerator messageBlock = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			messageBlock.Deserialise(BlockPadder.Pad(sentMsg.EM_MessageText));

			bool foundEmpty89 = false;
			foreach (MessageBlock block in messageBlock.MessageBlocks)
			{
				if (block is ENS89 && block.IsEmpty)
				{
					foundEmpty89 = true;
					break;
				}
			}

			Assert("Should have created ens89 with zero amount to match line HMF", foundEmpty89);
		}

		[TestDate(2010, 2, 14)]
		public void TestADDCaseAgainstSupTariff()
		{
			USCACCase uscCase = Factory.New<USCACCase>();
			uscCase.U5_CaseNumber = "A570832002";
			uscCase.U5_ISOCountryCode = "CN";
			uscCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			uscCase.U5_CaseStatusDate = ZDateTime.Today;

			uscCase.CaseTariffs.AddNew().U9_TariffNumber = "81049000";
			uscCase.CaseTariffs.AddNew().U9_TariffNumber = "81043000";
			uscCase.CaseTariffs.AddNew().U9_TariffNumber = "81041100";
			uscCase.CaseTariffs.AddNew().U9_TariffNumber = "98170090";

			var rate1 = uscCase.CaseRates.AddNew();
			rate1.U6_AdValoremRate = 1.06m;
			rate1.U6_EffectiveDate = ZDateTime.Today;
			Factory.Save();

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionADDCVD;
			invoiceLine.US_SupTariff = "9817.00.9040";
			invoiceLine.JI_Tariff = "7219.32.0042";
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.JI_LinePrice = 6360.00m;
			invoiceLine.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.OnceOff;
			invoiceLine.US_ADDCaseNo = "A570832002";

			// as per the recent reference file
			AssertNotNull(invoiceLine.AntidumpingDutyCase);
			USCACCaseRate rate = invoiceLine.AntidumpingDutyCase.GetDepositRate(invoiceLine.EffectiveDateForDutyRate);
			if (rate == null || rate.U6_AdValoremRate != 1.1173m)
			{
				rate = invoiceLine.AntidumpingDutyCase.CaseRates.AddNew();
				rate.U6_AdValoremRate = 1.1173m;
				rate.U6_EffectiveDate = new ZDateTime(2009, 12, 14);
			}

			invoiceLine.US_MiscPermitNo = "0CN123456";
			invoiceLine.US_UC_NKCountryOfOrigin = "CN";

			MergeAndSend("ADD with 9817");

			AssertEquals(6741.60m, declaration.ActiveEntryHeaders.EntrySummaryEntry.TotalAntidumpingDuty);
		}

		[TestDate(2009, 3, 20)]
		public void TestPGAData()
		{
			var pGA = invoiceLine.LaceyActLines.AddNew();
			DeclarationTestHelper.CreateLaceyActData(pGA);

			declaration.US_IsInvoiceByRequest = false;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			EntrySummaryMessageBuilder builder = new EntrySummaryMessageBuilder(declaration.CustomsEntryHeaders[0], UpdateActionCode.Add, true);
			MQEDIMessage message = builder.PopulateMessage();
			ZString expectedResult = @"B018888XJ5EI                                               <<MSGNO PLACEHOLDER>>
10A888891-01319900091-013199000                 8         XJ5 <E#PLCH>01891  IL 
20     APL EMERALD         108888                           V123W               
22                                                00000001PK         AAAA       
30                                  01              1                   AAAA    
40001AU00000100000000009000                    000000005060267                  
50 44219097200000033000000000007000GR                               AU031309N   
OI        SOFTWOOD PULPWOOD                                                     
PG01001AP                                                                       
PG04 SPRUCE                                              000000010000M3   020000
PG05PICEA                 GLAUCA                                                
PG06HRVCA                                                                       
PG25                                               000000010000                 
60                                        AUSOUPAC195PAD                        
62          49900002100                                                         
8949900000002500                                                                
9000000033000           0                       0000000250000000010000          
Y  8888XJ5EI00016000000033000";
			AssertMultilineASCIIEquals("PGA Data output expected", expectedResult, message.EM_FormattedMessageText);
		}

		[TestDate(2012, 03, 05)]
		public void TestEntryType06()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var port = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "3901", "Test Name", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTransportModeForCusCodeList(port.PK, TransportTypeList.Codes.Sea);
			var attributeNameUnlading = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Unlading, "Desc", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedStates);
			helper.CreateNewOrGetExistingCusCodeListAttribute(port.PK, attributeNameUnlading.ZXE_Name, "Y");
			Factory.Save();

			var addCase = Factory.New<USCACCase>();
			addCase.U5_CaseNumber = "ADD";
			addCase.U5_ISOCountryCode = "CN";
			addCase.U5_CaseStatus = ACCaseStatusList.Codes.AC;
			addCase.U5_CaseStatusDate = ZDateTime.MinSmallDateTimeValue;
			addCase.U5_ManufacturerMID = ZString.Empty;
			addCase.CaseTariffs.AddNew().U9_TariffNumber = "8466939585";
			var rate1 = addCase.CaseRates.AddNew();
			rate1.U6_EffectiveDate = ZDateTime.Today;
			rate1.U6_AdValoremRate = 1m;
			Factory.Save();
			/* CS00169175
			 * 8466.93.9585                    TARIFF NUMBER 
			 * Consumption FTZ
			 * Transport Mode, Discharge, Bill Data are not required*/

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_IsHMFApplicable = YesNoDefaultList.Codes.No;
			declaration.US_7501Purchased = YesNoDefaultList.Codes.No;
			declaration.US_LiveEntryIndicator = YesNoDefaultList.Codes.Yes;
			declaration.US_SchDEntry = "3901";
			declaration.US_SchDArrival = ZString.Empty;
			declaration.JE_MasterBill = "FTZ001A";
			declaration.US_FTZNo = "FTZ001B";
			declaration.US_US_NKLocationOfGoods = "S002";
			declaration.US_SchDArrival = "3901";

			invoiceLine.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.China;
			invoiceLine.US_UC_NKCountryOfExport = Core.Constants.CountryCodes.China;
			invoiceLine.JI_Tariff = "8466939585";
			invoiceLine.US_ManifestQty = 100;
			invoiceLine.JI_Description = "TEST CONSUMPTION FTZ";
			invoiceLine.JI_LinePrice = 3000m;
			invoiceLine.JI_CustomsQuantity = 555.56m;
			invoiceLine.JI_Weight = 10m;
			invoiceLine.US_ZoneStatus = ZoneStatusList.Codes.NonPrivilegedForeign;
			manufacturer.MainAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.ManufacturerID, "CHBEREQU6LON", Core.Constants.CountryCodes.UnitedStates);
			invoiceLine.US_ADCVDStat = ADDCVDNonReimbursementList.Codes.OnceOff;
			invoiceLine.US_ADDCaseNo = "ADD";
			invoiceLine.US_ADDDepositRateIndicator = DepositRateIndicatorList.Codes.AdValorem;
			MergeAndSend("501");

			var ens20 = declaration.ActiveEntryHeaders.EntrySummaryEntry.Messages[0].MessageBlock.MessageBlocks.OfType<ENS20>().FirstOrDefault();
			AssertEquals("3901", ens20.DistrictPortOfUnlading);
		}

		protected override void PopulateMessage(CusEntryHeader entryHeader, bool certifyCargoRelease)
		{
			base.PopulateMessage(entryHeader, certifyCargoRelease);
			entryHeader.CH_BGMReference = "General " + entryHeader.CH_BGMReference;
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
			new FeeCalculationHelperTest().PrepareFeeAndTexData();
		}
	}
}
