using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Common;
using Enterprise.Customs.US.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class CargoReleaseMessageBuilderTest : TestCaseWithFactory
	{
		public void TestStatus()
		{
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CargoReleaseMessageBuilder builder = new CargoReleaseMessageBuilder(entry, UpdateActionCode.Add, false);
			builder.PopulateMessage();
			AssertEquals("Status should not be calculated at this point as for amendment detection, it also generate a message using the same builder", ZString.Empty, entry.CH_Status);
		}

		public void TestGenerateCRLH1()
		{
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			declaration.US_EntryType = EntryTypeList.Codes.ImmediateExportation;
			var builder = new CargoReleaseBlockBuilder(entry, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions);

			declaration.US_EntryType = EntryTypeList.Codes.ImmediateExportation;
			declaration.US_UI_NKCarrierSCAC = "8888";
			declaration.US_EntryDate = new ZDateTime(2007, 3, 11, 3, 32, 32);
			declaration.US_PresentationDate = new ZDateTime(2007, 3, 12, 3, 32, 32);

			CusEntryLine entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.Add(builder.GenerateCRLH1(UpdateActionCode.Add, false));
			AssertEquals("CLRH1 segment", "H1A8888XJ5 <E#PLCH>69-9999999JC100311078         031207     8888123463891       ", block.MessageBlocks[0].Serialise());

			declaration.US_PresentationDate = ZDateTime.Empty;
			block.MessageBlocks.Add(builder.GenerateCRLH1(UpdateActionCode.Add, false));
			AssertEquals("CLRH1 segment", "H1A8888XJ5 <E#PLCH>69-9999999JC100311078                    8888123463891       ", block.MessageBlocks[1].Serialise());

			entry.US_UseConsigneeNameAddress = true;
			block.MessageBlocks.Add(builder.GenerateCRLH1(UpdateActionCode.Add, false));
			AssertEquals("CLRH1 segment", "H1A8888XJ5 <E#PLCH>69-9999999JC100311078                    8888123463891       ", block.MessageBlocks[2].Serialise());

			importer.CustomsCodes.RemoveAll();
			block.MessageBlocks.Add(builder.GenerateCRLH1(UpdateActionCode.Add, false));
			AssertEquals("CLRH1 segment", "H1A8888XJ5 <E#PLCH>            100311078                    8888123463891 1     ", block.MessageBlocks[3].Serialise());
		}

		public void TestGenerateCRLH2()
		{
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			var builder = new CargoReleaseBlockBuilder(entry, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions);

			CusEntryLine entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entry.US_UseConsigneeNameAddress = false;

			entry.Declaration.US_US_NKLocationOfGoods = "LOCA";
			declaration.JE_DeclarationReference = "B00112121";
			declaration.US_EntryDateElectionCode = "P";
			BlockControlGenerator block = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block.MessageBlocks.Add(builder.GenerateCRLH2());
			AssertEquals("H2LOCA    69-9999999JCPV123W0000000000B00112121            DONGSNICKNAMEISRICHI ", block.MessageBlocks[0].Serialise());

			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_VoyageFlightNo = "V123W";
			BlockControlGenerator block2 = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block2.MessageBlocks.Add(builder.GenerateCRLH2());
			AssertEquals("H2LOCA    69-9999999JCP123W 0000000000B00112121                                 ", block2.MessageBlocks[0].Serialise());

			entry.US_UseConsigneeNameAddress = true;
			BlockControlGenerator block3 = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block3.MessageBlocks.Add(builder.GenerateCRLH2());
			AssertEquals("H2LOCA    69-9999999JCP123W 0000000000B00112121                                 ", block3.MessageBlocks[0].Serialise());

			entry.ImportUltimateConsignee.CustomsCodes.RemoveAll();
			OrgCusCode ultimateConsigneeECNCode = entry.ImportUltimateConsignee.CustomsCodes.AddNew();
			ultimateConsigneeECNCode.OK_CodeType = OrgCusCode.USACodeTypes.EncryptedConsigneeNumber;
			ultimateConsigneeECNCode.OK_CustomsRegNo = "-T16WHSL5CXR";

			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_VoyageFlightNo = "V123W";
			declaration.JE_VesselName = "SOMEVESSEL";
			BlockControlGenerator block4 = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block4.MessageBlocks.Add(builder.GenerateCRLH2());
			AssertEquals("H2LOCA    -T16WHSL5CXRPV123W0000000000B00112121            SOMEVESSEL           ", block4.MessageBlocks[0].Serialise());

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.JE_MasterBill = "FTZ123A";
			declaration.US_FTZNo = "FTZ123B";
			BlockControlGenerator block5 = new ABIInputBlockControlGenerator(GlbBranch.CurrentBranch);
			block5.MessageBlocks.Add(builder.GenerateCRLH2());
			AssertEquals("H2LOCA    -T16WHSL5CXRPV123W0000000000B00112121            FTZ123B              ", block5.MessageBlocks[0].Serialise());
		}

		public void TestGenerateCRLHA()
		{
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			var builder = new CargoReleaseBlockBuilder(entry, ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions);

			declaration.US_ITDate = new ZDateTime(2007, 3, 12, 3, 32, 32);
			declaration.Bills.RemoveAndDeleteAll();

			Bill masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "TestMB1";

			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "TestHB1";
			houseBill.CU_CU_ParentBill = masterBill.PK;

			Bill subHouseBill = declaration.Bills.AddNew();
			subHouseBill.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			subHouseBill.CU_BillNum = "TestSubHB1";
			subHouseBill.CU_CU_ParentBill = houseBill.PK;

			subHouseBill.ITNumber = "inboNum1";
			subHouseBill.CU_NoOfPacks = 103;
			subHouseBill.CU_PackType = "CTN";
			masterBill.US_UI_NKBillIssuerSCAC = "OTT1";
			houseBill.US_UI_NKBillIssuerSCAC = "OTT1";

			foreach (MessageBlock block in builder.GenerateCRLHA())
			{
				AssertEquals("CLRHA segment", "HAINBONUM1    TESTMB1     TESTHB1     TESTSUBHB1  00000103CTN  031207OTT1OTT1   ", block.Serialise());
			}
		}

		[TestDate(2008, 9, 11)]
		public void TestGenerateEntryLine()
		{
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders[0].Messages.AddNew(typeof(EDIMessage));
			CargoReleaseMessageBuilder builder = new CargoReleaseMessageBuilder(entry, UpdateActionCode.Add, true);

			// setup H5 block information
			CusEntryLine entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entryLine.CL_AdValoremTariff = invoiceLine.JI_Tariff;

			entryLine.CL_LineNumber = 1;
			invoiceHeader.US_UC_NKCountryOfOrigin = "US";
			entryLine.CL_CustomsValue = 10000m;
			MakeAndAssertFCCTariff(builder);
			MakeAndAssertDOTTariff(builder);
			MakeAndAssertFDATariff(entry);

			builder = new CargoReleaseMessageBuilder(entry, UpdateActionCode.Add, false);
			MakeAndAssertFCCTariff(builder);
			MakeAndAssertDOTTariff(builder);
			MakeAndAssertFDATariff(entry);
		}

		public void TestEndToEndTestWithLineLevelUltimateConsignee()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = "";
			declaration.US_EnableCRL = true;

			declaration.US_EntryFilerCode = "XJ5";

			OrgHeader ultimateConsignee = Factory.New<OrgHeader>();
			ultimateConsignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "AAAAAA123");

			OrgHeader ultimateConsignee2 = Factory.New<OrgHeader>();
			ultimateConsignee2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "BBBBBB123");

			OrgHeader ultimateConsignee3 = Factory.New<OrgHeader>();
			ultimateConsignee3.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "CCCCCC123");

			declaration.JE_OA_ConsigneeAddress = ultimateConsignee.MainAddress.PK;
			declaration.Invoices.AddNew();

			JobComInvoiceLine invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_OA_ConsigneeAddress = ultimateConsignee2.MainAddress.PK;

			JobComInvoiceLine invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_OA_ConsigneeAddress = ultimateConsignee3.MainAddress.PK;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CargoReleaseMessageBuilder builder = new CargoReleaseMessageBuilder(declaration.CustomsEntryHeaders[0], UpdateActionCode.Add, true);

			MQEDIMessage message = builder.PopulateMessage();

			AssertEquals("EIN number for declaration ultimate consignee should not be part of messages", false, message.EM_MessageText.Contains("AAAAAA123"));
			AssertEquals("EIN number for invoiceLine1 ultimate consignee should be part of messages", true, message.EM_MessageText.Contains("BBBBBB123"));
			AssertEquals("EIN number for invoiceLine2 ultimate consignee should be part of messages", true, message.EM_MessageText.Contains("CCCCCC123"));
		}

		public void TestEndToEndTestWithPGAData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EntryType = "";
			declaration.US_EnableCRL = true;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.Invoices.AddNew();

			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			var pGA = invoiceLine1.LaceyActLines.AddNew();
			DeclarationTestHelper.CreateLaceyActData(pGA);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var builder = new CargoReleaseMessageBuilder(declaration.CustomsEntryHeaders[0], UpdateActionCode.Add, true);

			var message = builder.PopulateMessage();

			AssertEquals(@"B018888XJ5HI                                               <<MSGNO PLACEHOLDER>>
H1A    XJ5 <E#PLCH>                    01                                       
H2                          0000000000                                          
H5001                                                                           
OI        SOFTWOOD PULPWOOD                                                     
PG01001AP                                                                       
PG04 SPRUCE                                              000000010000M3   020000
PG05PICEA                 GLAUCA                                                
PG06HRVCA                                                                       
PG25                                               000000010000                 
Y  8888XJ5HI00009", message.EM_FormattedMessageText);
		}

		public void TestPGADataBeforeOGAData()
		{
			declaration.US_EntryFilerCode = "XJ5";

			PGA pGA = invoiceLine.LaceyActLines.AddNew();
			DeclarationTestHelper.CreateLaceyActData(pGA);

			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);

			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			DOT dot = invoiceLine.DOTs.AddNew();
			dot.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._05;
			dot.US_DOTCommercialDesc = "O VH,>4<=6CYL,IN VL>2.8<=3";
			dot.US_DOTClarCode = ClarificationCodeList.Codes.Vehicle;
			DOTVIN dotVin = dot.DOTVINs.AddNew();
			dotVin.US_DOTYear = 1999;
			dotVin.US_DOTVIN = "IEWR87EWRKJWER87";

			invoiceLine.US_FCCIndicator = OGAIndicatorList.Codes.Declared;
			FCC fcc = invoiceLine.FCCs.AddNew();
			fcc.US_FCCID = "HELLO";
			fcc.US_FCCImpCondNo = FCCImportConditionNumberList.Codes._03;
			fcc.US_FCCModel = "MODEL";
			fcc.US_FCCQty = 1;
			fcc.US_FCCTradeName = "TRADE NAME";

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CargoReleaseMessageBuilder builder = new CargoReleaseMessageBuilder(declaration.CustomsEntryHeaders[0], UpdateActionCode.Add, true);

			MQEDIMessage message = builder.PopulateMessage();

			AssertEquals("PGA blocks should be before other OGA blocks",
						@"B018888XJ5HI                                               <<MSGNO PLACEHOLDER>>
H1A8888XJ5 <E#PLCH>69-9999999JC10      81                   OTT1123401891       
H2        69-9999999JC V123W0000000000                     DONGSNICKNAMEISRICHI 
H5001  8471704065XYBEREQU6LON                                                   
OI        SOFTWOOD PULPWOOD                                                     
PG01001AP                                                                       
PG04 SPRUCE                                              000000010000M3   020000
PG05PICEA                 GLAUCA                                                
PG06HRVCA                                                                       
PG25                                               000000010000                 
OI        O VH,>4<=6CYL,IN VL>2.8<=3                                            
DT0100105Y                          V                                           
DT02                              1999IEWR87EWRKJWER87                          
OI        TEST                                                                  
FD0100124DCS18   XY                            XYBEREQU6LON   XYBEREQU6LON      
FD020000900000KG                                                                
FD030000000000                                                                  
FD04              JESSIE JAM3273958841                                          
Y  8888XJ5HI00017",
						message.EM_FormattedMessageText);
		}

		public void TestDisclaimingDataBeforeOtherOGAData()
		{
			declaration.US_EntryFilerCode = "XJ5";

			PGA pGA = invoiceLine.LaceyActLines.AddNew();
			DeclarationTestHelper.CreateLaceyActData(pGA);

			invoiceLine.JI_Tariff = "3924905600";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			CargoReleaseMessageBuilder builder = new CargoReleaseMessageBuilder(declaration.CustomsEntryHeaders[0], UpdateActionCode.Add, true);

			MQEDIMessage message = builder.PopulateMessage();

			AssertEquals("Disclaiming blocks before other OGA blocks",
						@"B018888XJ5HI                                               <<MSGNO PLACEHOLDER>>
H1A8888XJ5 <E#PLCH>69-9999999JC10      81                   OTT1123401891       
H2        69-9999999JC V123W0000000000                     DONGSNICKNAMEISRICHI 
H5001  3924905600XYBEREQU6LON                                                   
OA  FD0                                                                         
OI        SOFTWOOD PULPWOOD                                                     
PG01001AP                                                                       
PG04 SPRUCE                                              000000010000M3   020000
PG05PICEA                 GLAUCA                                                
PG06HRVCA                                                                       
PG25                                               000000010000                 
Y  8888XJ5HI00010",
						message.EM_FormattedMessageText);
		}

		public void TestEndToEndTest()
		{
			DeclarationTestHelper.SetEntryFilerCode("JD6");
			declaration.US_EntryFilerCode = "XJ5";
			declaration.Bills.RemoveAndDeleteAll();

			Bill masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "TestMB1";
			masterBill.US_UI_NKBillIssuerSCAC = "OTT1";
			declaration.JE_MasterBill = "TestMB1";
			declaration.US_SchDLoading = "5678";

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var uniRefHelper = new UniversalReferenceTestDataHelper(newFactory);
			uniRefHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			uniRefHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "5678", "Some Domestic Port", startDate, endDate);
			newFactory.Save();

			declaration.US_SchDArrival = "5678";

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Liquid;
			declaration.IOROrgPK = declaration.JE_OH_Importer;
			declaration.US_PaymentType = "";
			declaration.US_UC_NKCountryOfExport = "PE";

			invoiceHeader.JZ_InvoiceAmount = 15000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_Weight = 120m;
			masterBill.CU_NoOfPacks = 130;
			masterBill.CU_PackType = "PCS";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			entry.US_UseConsigneeNameAddress = false;

			CargoReleaseMessageBuilder builder = new CargoReleaseMessageBuilder(entry, UpdateActionCode.Add, true);

			MQEDIMessage message = builder.PopulateMessage();
			AssertMultilineASCIIEquals("Expected Message",
@"B018888XJ5HI                                               <<MSGNO PLACEHOLDER>>
H1A8888XJ5 <E#PLCH>69-9999999JC10      81                   OTT1567801891       
H2        69-9999999JC V123W0000015000                     DONGSNICKNAMEISRICHI 
HA            TESTMB1                             00000130PCS        OTT1       
H5001AU8471704065XYBEREQU6LON               0000015000                          
Y  8888XJ5HI00004", message.EM_FormattedMessageText);

			entry.US_UseConsigneeNameAddress = true;
			message = builder.PopulateMessage();
			AssertMultilineASCIIEquals("expected message",
@"B018888XJ5HI                                               <<MSGNO PLACEHOLDER>>
H1A8888XJ5 <E#PLCH>69-9999999JC10      81                   OTT1567801891       
H2        69-9999999JC V123W0000015000                     DONGSNICKNAMEISRICHI 
HA            TESTMB1                             00000130PCS        OTT1       
H5001AU8471704065XYBEREQU6LON               0000015000                          
Y  8888XJ5HI00004", message.EM_FormattedMessageText);

			//FDA tariffs
			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			helper.SetUpFDARequiredData(invoiceLine2, manufacturer, Factory);

			invoiceLine2.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			invoiceLine2.JI_Description = "Description";
			invoiceLine2.US_FDAIndicator = OGAIndicatorList.Codes.Declared;

			invoiceLine2.FDAs.AddNew();
			invoiceLine2.FDAs[0].US_FDACargoStorageCode = CargoStorageCodeList.Codes.AmbientTemperature;
			invoiceLine2.FDAs[0].US_TradeBrandName = "TRADE NAME";
			invoiceLine2.FDAs[0].US_FDAQty1 = 150;
			invoiceLine2.FDAs[0].US_FDAMeasure1 = FDABaseUQList.Codes.KG;

			declaration.DoMerge();

			message = builder.PopulateMessage();
			AssertEquals("FD01001" + invoiceLine2.FDAs[0].US_FDAProductCode + "A  XYAPA5678                     XYBEREQU6LON   XYBEREQU6LON      ", helper.GetTypes<OGAFD01>(message)[0].Serialise());
			AssertEquals("FD020000015000KG                                                                ", helper.GetTypes<OGAFD02>(message)[0].Serialise());
			AssertEquals("FD030000000000            TRADE NAME                                            ", helper.GetTypes<OGAFD03>(message)[0].Serialise());

			invoiceLine.JI_Tariff = "3504005000";
			invoiceLine.ImportTariff.UE_OGACodes = "FD1";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;

			declaration.DoMerge();

			message = builder.PopulateMessage();
			AssertEquals("OA  FD0                                                                         ", helper.GetTypes<OGAOA>(message)[0].Serialise());
			Assert("Message did not de-serialise correctly", message.EM_MessageInterpretation.Length > 0);

			string expectedOGADataMessage = @"B018888XJ5HI                                               <<MSGNO PLACEHOLDER>>
H1A8888XJ5 <E#PLCH>69-9999999JC10      81                   OTT1567801891       
H2        69-9999999JC V123W0000015000                     DONGSNICKNAMEISRICHI 
HA            TESTMB1                             00000130PCS        OTT1       
H5001AU3504005000XYBEREQU6LON               0000015000                          
OA  FD0                                                                         
H5002XY0804504040XYBEREQU6LON                                                   
OI        TEST                                                                  
FD0100124DCS18A  XYAPA5678                     XYBEREQU6LON   XYBEREQU6LON      
FD020000015000KG                                                                
FD030000000000            TRADE NAME                                            
FD04              JESSIE JAM3273958841                                          
FD05CSHPE                                                                       
FD05OFTI                                                                        
FD05PFR12345678901                                                              
FD05PFTG                                                                        
FD05SASCA                                                                       
FD05SCCUS                                                                       
FD05SCNTEST IMPORTER                                                            
FD05SEMNONE                                                                     
FD05SFX0000000000                                                               
FD05VFTV123W                                                                    
FD05BOLOTT1TESTMB1                                                              
FD05TEMJESSIE.JAMES@LONGCOMPANYN                                                
OI        DESCRIPTION                                                           
FD01002            APA5678                     XYBEREQU6LON   XYBEREQU6LON      
FD02                                                                            
FD030000000000                                                                  
FD04              JESSIE JAM3273958841                                          
FD05CSHPE                                                                       
FD05OFTI                                                                        
FD05SASCA                                                                       
FD05SCCUS                                                                       
FD05SCNTEST IMPORTER                                                            
FD05SEMNONE                                                                     
FD05SFX0000000000                                                               
FD05VFTV123W                                                                    
FD05BOLOTT1TESTMB1                                                              
FD05TEMJESSIE.JAMES@LONGCOMPANYN                                                
Y  8888XJ5HI00038";
			AssertMultilineASCIIEquals("Disclaimed block should be before declared OGA data", expectedOGADataMessage, message.EM_FormattedMessageText);
		}

		public void TestEndToEndTestPostFinalRuleImplementation()
		{
			DeclarationTestHelper.SetEntryFilerCode("JD6");
			declaration.US_EntryFilerCode = "XJ5";
			declaration.Bills.RemoveAndDeleteAll();

			Bill masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "TestMB1";
			masterBill.US_UI_NKBillIssuerSCAC = "OTT1";
			declaration.JE_MasterBill = "TestMB1";
			declaration.US_SchDLoading = "5678";

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var uniRefHelper = new UniversalReferenceTestDataHelper(newFactory);
			uniRefHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			uniRefHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "5678", "Some Domestic Port", startDate, endDate);
			newFactory.Save();

			declaration.US_SchDArrival = "5678";

			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Liquid;
			declaration.IOROrgPK = declaration.JE_OH_Importer;
			declaration.US_PaymentType = "";
			declaration.US_UC_NKCountryOfExport = "PE";

			invoiceHeader.JZ_InvoiceAmount = 15000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			invoiceLine.JI_LinePrice = 15000m;
			invoiceLine.JI_CustomsQuantity = 1m;
			invoiceLine.US_UC_NKCountryOfOrigin = "AU";
			invoiceLine.JI_Weight = 120m;
			masterBill.CU_NoOfPacks = 130;
			masterBill.CU_PackType = "PCS";

			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			CusEntryHeader entry = declaration.CustomsEntryHeaders[0];
			entry.US_UseConsigneeNameAddress = false;

			CargoReleaseMessageBuilder builder = new CargoReleaseMessageBuilder(entry, UpdateActionCode.Add, true);

			MQEDIMessage message = builder.PopulateMessage();
			AssertMultilineASCIIEquals("Expected Message",
@"B018888XJ5HI                                               <<MSGNO PLACEHOLDER>>
H1A8888XJ5 <E#PLCH>69-9999999JC10      81                   OTT1567801891       
H2        69-9999999JC V123W0000015000                     DONGSNICKNAMEISRICHI 
HA            TESTMB1                             00000130PCS        OTT1       
H5001AU8471704065XYBEREQU6LON               0000015000                          
Y  8888XJ5HI00004", message.EM_FormattedMessageText);

			entry.US_UseConsigneeNameAddress = true;
			message = builder.PopulateMessage();
			AssertMultilineASCIIEquals("expected message",
@"B018888XJ5HI                                               <<MSGNO PLACEHOLDER>>
H1A8888XJ5 <E#PLCH>69-9999999JC10      81                   OTT1567801891       
H2        69-9999999JC V123W0000015000                     DONGSNICKNAMEISRICHI 
HA            TESTMB1                             00000130PCS        OTT1       
H5001AU8471704065XYBEREQU6LON               0000015000                          
Y  8888XJ5HI00004", message.EM_FormattedMessageText);

			//FDA tariffs
			JobComInvoiceLine invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			helper.SetUpFDARequiredData(invoiceLine2, manufacturer, Factory);

			invoiceLine2.JI_Tariff = "0712902000";
			invoiceLine2.ImportTariff.UE_OGACodes = "FD2";
			invoiceLine2.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			invoiceLine2.JI_Description = "Description";
			invoiceLine2.US_FDAIndicator = OGAIndicatorList.Codes.Declared;

			invoiceLine2.FDAs.AddNew();
			invoiceLine2.FDAs[0].US_FDACargoStorageCode = CargoStorageCodeList.Codes.AmbientTemperature;
			invoiceLine2.FDAs[0].US_TradeBrandName = "TRADE NAME";
			invoiceLine2.FDAs[0].US_FDAQty1 = 150;
			invoiceLine2.FDAs[0].US_FDAMeasure1 = FDABaseUQList.Codes.KG;

			declaration.DoMerge();

			message = builder.PopulateMessage();
			AssertEquals("FD01001" + invoiceLine2.FDAs[0].US_FDAProductCode + "A  XYAPA5678                     XYBEREQU6LON   XYBEREQU6LON      ", helper.GetTypes<OGAFD01>(message)[0].Serialise());
			AssertEquals("FD020000015000KG                                                                ", helper.GetTypes<OGAFD02>(message)[0].Serialise());
			AssertEquals("FD030000000000            TRADE NAME                                            ", helper.GetTypes<OGAFD03>(message)[0].Serialise());

			invoiceLine.JI_Tariff = "3504005000";
			invoiceLine.ImportTariff.UE_OGACodes = "FD2";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;

			declaration.DoMerge();

			message = builder.PopulateMessage();
			AssertEquals("OA  FD0                                                                         ", helper.GetTypes<OGAOA>(message)[0].Serialise());
			Assert("Message did not de-serialise correctly", message.EM_MessageInterpretation.Length > 0);

			string expectedOGADataMessage = @"B018888XJ5HI                                               <<MSGNO PLACEHOLDER>>
H1A8888XJ5 <E#PLCH>69-9999999JC10      81                   OTT1567801891       
H2        69-9999999JC V123W0000015000                     DONGSNICKNAMEISRICHI 
HA            TESTMB1                             00000130PCS        OTT1       
H5001AU3504005000XYBEREQU6LON               0000015000                          
OA  FD0                                                                         
H5002XY0804504040XYBEREQU6LON                                                   
OI        TEST                                                                  
FD0100124DCS18A  XYAPA5678                     XYBEREQU6LON   XYBEREQU6LON      
FD020000015000KG                                                                
FD030000000000            TRADE NAME                                            
FD04              JESSIE JAM3273958841                                          
FD05CSHPE                                                                       
FD05OFTI                                                                        
FD05PFR12345678901                                                              
FD05PFTG                                                                        
FD05SASCA                                                                       
FD05SCCUS                                                                       
FD05SCNTEST IMPORTER                                                            
FD05SEMNONE                                                                     
FD05SFX0000000000                                                               
FD05VFTV123W                                                                    
FD05BOLOTT1TESTMB1                                                              
FD05TEMJESSIE.JAMES@LONGCOMPANYN                                                
OI        DESCRIPTION                                                           
FD01002            APA5678                     XYBEREQU6LON   XYBEREQU6LON      
FD02                                                                            
FD030000000000                                                                  
FD04              JESSIE JAM3273958841                                          
FD05CSHPE                                                                       
FD05OFTI                                                                        
FD05SASCA                                                                       
FD05SCCUS                                                                       
FD05SCNTEST IMPORTER                                                            
FD05SEMNONE                                                                     
FD05SFX0000000000                                                               
FD05VFTV123W                                                                    
FD05BOLOTT1TESTMB1                                                              
FD05TEMJESSIE.JAMES@LONGCOMPANYN                                                
Y  8888XJ5HI00038";
			AssertMultilineASCIIEquals("Disclaimed block should be before declared OGA data", expectedOGADataMessage, message.EM_FormattedMessageText);
		}

		protected override void SetUp()
		{
			base.SetUp();
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			helper = new DeclarationTestHelper(Factory);

			importer = Factory.New<OrgHeader>();
			importer.OH_Code = "ABC" + new Random().Next(1000000).ToString();
			importer.OH_FullName = "Test Importer";
			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "69-9999999JC");

			importer.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.CBPAssignedNumber, "91-013199000");

			shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_Code = "SHP" + new Random().Next(1000000).ToString();
			shippingLine.OH_FullName = "Test Shipping Line";
			shippingLine.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "OTT1");

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.IOROrgPK = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.CR;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;

			declaration.JE_VesselName = "DONGSNICKNAMEISRICHIEBECAUSEHECONSI";
			declaration.JE_VoyageFlightNo = "V123W";
			declaration.US_BondType = BondTypeList.Codes.ContinuousBond;
			declaration.US_SuretyCode = "891";
			declaration.JE_OH_ShippingLine = shippingLine.PK;

			declaration.JE_RL_NKPortOfLoading = "PERTD";
			declaration.US_SchDEntry = "8888";
			declaration.US_SchDArrival = "1234";
			declaration.JE_RL_NKOrigin = "PERTD";

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoiceHeader.JZ_OH_Buyer = importer.PK;
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8471704065";

			manufacturer = Factory.New<OrgHeader>();
			invoiceHeader.JZ_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			manufacturer.OH_FullName = "Test Manufacturer";
			manufacturer.OH_Code = "MAN" + new Random().Next(1000000).ToString();
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "XYBEREQU6LON");
		}

		void MakeAndAssertFCCTariff(CargoReleaseMessageBuilder builder)
		{
			invoiceLine.JI_Tariff = "8528219001";
			invoiceLine.ImportTariff.UE_OGACodes = "FC4";
			invoiceLine.FCCs.AddNew();

			MQEDIMessage message = builder.PopulateMessage();
			declaration.CustomsEntryHeaders[0].Messages[0].EM_MessageText = message.EM_MessageText;
			helper.MessageMustNotContainElement<OGAFC01>(message);
			helper.MessageMustNotContainElement<OGAFC02>(message);
			AssertNotContains("FCC is only sent on the Entry Summary - never on the Cargo Release", "FCO", message.EM_MessageText);
		}

		void MakeAndAssertDOTTariff(CargoReleaseMessageBuilder builder)
		{
			invoiceLine.JI_Tariff = "8706005000";
			invoiceLine.Declaration.CustomsEntryHeaders[0].MergedLines[0].CL_AdValoremTariff = invoiceLine.JI_Tariff;
			invoiceLine.ImportTariff.UE_OGACodes = "DT1";

			MQEDIMessage message = builder.PopulateMessage();
			helper.MessageMustNotContainElement<OGADT01>(message);

			invoiceLine.JI_Tariff = "8703230042";
			invoiceLine.Declaration.CustomsEntryHeaders[0].MergedLines[0].CL_AdValoremTariff = invoiceLine.JI_Tariff;
			invoiceLine.ImportTariff.UE_OGACodes = "DT2";
			invoiceLine.US_DOTIndicator = OGAIndicatorList.Codes.Declared;
			DOT dot = invoiceLine.DOTs.AddNew();
			dot.US_DOTTireBrandName = "TRADE NAME";
			dot.US_DOTBoxNo = DepartmentOfTransportBoxNumberList.Codes._2A;
			dot.US_DOTPassport = "124HJ";
			dot.US_DOTCountryOfOrigin = "AU";
			dot.US_DOTBondSuretyCode = "111";
			dot.US_DOTPriorApproval = true;
			dot.US_DOTImpSubstStatement = true;
			dot.US_DOTClarCode = ClarificationCodeList.Codes.Vehicle;
			dot.US_DOTTireID = "ZZZ";
			dot.US_DOTCommercialDesc = "DESCRIPTION";

			message = builder.PopulateMessage();
			helper.MessageMustContainElement<OGADT01>(message);
			helper.MessageMustNotContainElement<OGADT02>(message);
			AssertEquals("H5001US8703230042XYBEREQU6LON               0000010000                          ", helper.GetTypes<CRLH5>(message)[0].Serialise());
			AssertEquals("OI        DESCRIPTION                                                           ", helper.GetTypes<AENSOI>(message)[0].Serialise());
			AssertEquals("DT010012AY124HJ              AU111YYVZZZTRADE NAME                              ", helper.GetTypes<OGADT01>(message)[0].Serialise());
		}

		void MakeAndAssertFDATariff(CusEntryHeader entry)
		{
			CargoReleaseMessageBuilder builder1 = new CargoReleaseMessageBuilder(entry, UpdateActionCode.Add, true);

			invoiceLine.JI_Tariff = "8509800045";
			entry.MergedLines[0].CL_AdValoremTariff = invoiceLine.JI_Tariff;
			invoiceLine.ImportTariff.UE_OGACodes = "FD1";

			MQEDIMessage message = builder1.PopulateMessage();
			helper.MessageMustNotContainElement<OGAFD01>(message);
			helper.MessageMustNotContainElement<OGAFD02>(message);
			helper.MessageMustNotContainElement<OGAFD03>(message);
			AssertEquals("H5001US8509800045XYBEREQU6LON               0000010000                          ", helper.GetTypes<CRLH5>(message)[0].Serialise());

			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);

			// FD2 & FD4 required
			invoiceLine.JI_Tariff = "2008200090";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			entry.MergedLines[0].CL_AdValoremTariff = invoiceLine.JI_Tariff;
			helper.SetUpFDARequiredData(invoiceLine, manufacturer, Factory);
			invoiceLine.FDAs[0].US_FDACommercialDesc = "Description";
			invoiceLine.FDAs[0].US_TradeBrandName = "TRADE NAME";

			message = builder1.PopulateMessage();
			AssertEquals("H5001US2008200090XYBEREQU6LON               0000010000                          ", helper.GetTypes<CRLH5>(message)[0].Serialise());
			AssertEquals("OI        DESCRIPTION                                                           ", helper.GetTypes<AENSOI>(message)[0].Serialise());
			AssertEquals("FD01001" + invoiceLine.FDAs[0].US_FDAProductCode + "   XYAPA1234                     XYBEREQU6LON   XYBEREQU6LON      ", helper.GetTypes<OGAFD01>(message)[0].Serialise());
			AssertEquals("FD020000900000KG                                                                ", helper.GetTypes<OGAFD02>(message)[0].Serialise());
			AssertEquals("FD030000000000            TRADE NAME                                            ", helper.GetTypes<OGAFD03>(message)[0].Serialise());

			// Tariff which required BTA data - FD4
			invoiceLine.JI_Tariff = "2008193010";
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			entry.MergedLines[0].CL_AdValoremTariff = invoiceLine.JI_Tariff;
			invoiceLine.ImportTariff.UE_OGACodes = "FD4";

			message = builder1.PopulateMessage();
			helper.MessageMustContainElement<OGAFD01>(message);
			helper.MessageMustContainElement<OGAFD02>(message);
			helper.MessageMustContainElement<OGAFD03>(message);
			helper.MessageMustContainElement<OGAFD04>(message);
			message.EM_MessageText.Contains("H5001US2008193010XYBEREQU6LON               0000010000                          OI        DESCRIPTION                                                           FD0100134AA.AA   XYAPA1234                     XYBEREQU6LON   XYBEREQU6LON      FD02                                                                            FD030000000000            TRADE NAME                                            FD04              WALTER WOO8473645600");

			message = builder1.PopulateMessage();
			AssertEquals("H5001US2008193010XYBEREQU6LON               0000010000                          ", helper.GetTypes<CRLH5>(message)[0].Serialise());
			AssertEquals("OI        DESCRIPTION                                                           ", helper.GetTypes<AENSOI>(message)[0].Serialise());
			AssertEquals("FD01001" + invoiceLine.FDAs[0].US_FDAProductCode + "   XYAPA1234                     XYBEREQU6LON   XYBEREQU6LON      ", helper.GetTypes<OGAFD01>(message)[0].Serialise());

			AssertEquals("FD020000900000KG                                                                ", helper.GetTypes<OGAFD02>(message)[0].Serialise());
			AssertEquals("FD030000000000            TRADE NAME                                            ", helper.GetTypes<OGAFD03>(message)[0].Serialise());
			AssertEquals("FD04              JESSIE JAM3273958841                                          ", helper.GetTypes<OGAFD04>(message)[0].Serialise());
			AssertEquals("FD05CSHPE                                                                       ", helper.GetTypes<OGAFD05>(message)[0].Serialise());
			AssertEquals("FD05OFTI                                                                        ", helper.GetTypes<OGAFD05>(message)[1].Serialise());
		}

		DeclarationTestHelper helper;
		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;
		OrgHeader importer;
		OrgHeader manufacturer;
		OrgHeader shippingLine;
	}
}
