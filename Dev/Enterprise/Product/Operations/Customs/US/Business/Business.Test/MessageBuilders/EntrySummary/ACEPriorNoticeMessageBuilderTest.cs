using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class ACEPriorNoticeMessageBuilderTest : TestCaseWithFactory
	{
		public void TestGenerateMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "SV9";
			declaration.ImportEntryNumber = "00000005";
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.US_EnableENS = true;
			declaration.US_EnableSPN = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.BLN;
			declaration.JE_MasterBill = "MB001002";
			declaration.PrimaryMasterBill.US_UI_NKBillIssuerSCAC = "XXXD";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "MESSAGE ON MORE THAN 70 CHARACTERSMESSAGE ON MORE THAN 70 CHARACTERSMESSAGE ON MORE THAN 70 CHARACTERS";

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddMonths(1);
			tariff.UE_PGACodes = "FD4";

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_FDAIndicator = "D";
			invoiceLine.JI_LinePrice = 1000m;
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_FEE;
			fda.US_BrandName = "TEST FOOD";
			fda.US_Description = "SOMETHING";
			fda.US_ProdCountry = "FR";
			fda.US_ProducerType = ProducerFirmTypeList.Codes.M;
			fda.US_InvCurrValue = ZDecimal.Zero;
			var affirmationCodes = fda.AffirmationCodes.AddNew();
			affirmationCodes.CY_Code = ACE_AffirmationOfComplianceList.Codes.FCE;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.US_FDAIndicator = "D";
			invoiceLine2.JI_Description = "Test 2";

			invoiceLine2.JI_Tariff = tariff.UE_Tariff;
			invoiceLine2.JI_LinePrice = 500m;
			var fda2 = invoiceLine2.ACE_FDALines.AddNew();
			fda2.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda2.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;
			fda2.US_BrandName = "TEST FOOD";
			fda2.US_Description = "NATURAL FOOD";
			fda2.US_ProdCountry = "FR";
			fda2.US_ShipmentCountry = "IT";
			fda2.US_InvCurrValue = ZDecimal.Zero;

			var priorNoticeWrapper = new ACEStandAlonePriorNoticeWrapper(declaration);
			AssertEquals(1, priorNoticeWrapper.PriorNoticeHeaders.Count);
			var priorNoticeHeader = priorNoticeWrapper.PriorNoticeHeaders[0];
			new ACEPriorNoticeMessageBuilder(priorNoticeHeader, "A").GenerateMessage();
			AssertEquals(1, declaration.Messages.Count);
			AssertMultilineASCIIEquals("Expected Message",
@"B         PE                                               <<MSGNO PLACEHOLDER>>
PE10AABOLXXXDMB001002                                      RXXXD0110            
OI        MESSAGE ON MORE THAN 70 CHARACTERSMESSAGE ON MORE THAN 70 CHARACTERSME
PG01001FDAFOOFEE                                                                
PG02PFDP                                                                        
PG0639 FR                                                                       
PG07TEST FOOD                                                                   
PG10                   SOMETHING                                                
PG19PNT                  EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
OI        TEST 2                                                                
PG01002FDAFOONSF                                                                
PG02PFDP                                                                        
PG06262FR                                                                       
PG06CSHIT                                                                       
PG07TEST FOOD                                                                   
PG10                   NATURAL FOOD                                             
PG19PNT                  EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
Y         PE", declaration.Messages[0].EM_FormattedMessageText);

			declaration.Messages.RemoveAndDeleteAll();
			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.ENT;
			priorNoticeWrapper = new ACEStandAlonePriorNoticeWrapper(declaration);
			AssertEquals(1, priorNoticeWrapper.PriorNoticeHeaders.Count);
			priorNoticeHeader = priorNoticeWrapper.PriorNoticeHeaders[0];
			new ACEPriorNoticeMessageBuilder(priorNoticeHeader, "A").GenerateMessage();
			AssertEquals(1, declaration.Messages.Count);
			AssertMultilineASCIIEquals("Expected Message",
@"B         PE                                               <<MSGNO PLACEHOLDER>>
PE10AAENTSV9 00000005                                       XXXD0110            
PE15RXXXDMB001002                                                               
OI        MESSAGE ON MORE THAN 70 CHARACTERSMESSAGE ON MORE THAN 70 CHARACTERSME
PG01001FDAFOOFEE                                                                
PG02PFDP                                                                        
PG0639 FR                                                                       
PG07TEST FOOD                                                                   
PG10                   SOMETHING                                                
PG19PNT                  EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
OI        TEST 2                                                                
PG01002FDAFOONSF                                                                
PG02PFDP                                                                        
PG06262FR                                                                       
PG06CSHIT                                                                       
PG07TEST FOOD                                                                   
PG10                   NATURAL FOOD                                             
PG19PNT                  EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
Y         PE", declaration.Messages[0].EM_FormattedMessageText);

			declaration.JE_HouseBill = "HB00000001";
			declaration.PrimaryHouseBill.US_UI_NKBillIssuerSCAC = "APLU";
			declaration.Messages.RemoveAndDeleteAll();

			priorNoticeWrapper = new ACEStandAlonePriorNoticeWrapper(declaration);
			AssertEquals(1, priorNoticeWrapper.PriorNoticeHeaders.Count);
			priorNoticeHeader = priorNoticeWrapper.PriorNoticeHeaders[0];
			new ACEPriorNoticeMessageBuilder(priorNoticeHeader, "A").GenerateMessage();
			AssertEquals(1, declaration.Messages.Count);
			AssertMultilineASCIIEquals("Expected Message",
@"B         PE                                               <<MSGNO PLACEHOLDER>>
PE10AAENTSV9 00000005                                       XXXD0110            
PE15MXXXDMB001002                                                               
PE15HAPLUHB00000001                                                             
OI        MESSAGE ON MORE THAN 70 CHARACTERSMESSAGE ON MORE THAN 70 CHARACTERSME
PG01001FDAFOOFEE                                                                
PG02PFDP                                                                        
PG0639 FR                                                                       
PG07TEST FOOD                                                                   
PG10                   SOMETHING                                                
PG19PNT                  EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
OI        TEST 2                                                                
PG01002FDAFOONSF                                                                
PG02PFDP                                                                        
PG06262FR                                                                       
PG06CSHIT                                                                       
PG07TEST FOOD                                                                   
PG10                   NATURAL FOOD                                             
PG19PNT                  EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
Y         PE", declaration.Messages[0].EM_FormattedMessageText);

			declaration.Messages.RemoveAndDeleteAll();
			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.BLN;

			var houseBill = declaration.Bills.CreatePrimaryBill(Customs.Business.BillTypeList.Codes.HouseBill);
			houseBill.US_UI_NKBillIssuerSCAC = "AAGC";
			houseBill.CU_BillNum = "HB00000002";

			priorNoticeWrapper = new ACEStandAlonePriorNoticeWrapper(declaration);
			AssertEquals(1, priorNoticeWrapper.PriorNoticeHeaders.Count);
			priorNoticeHeader = priorNoticeWrapper.PriorNoticeHeaders[0];
			new ACEPriorNoticeMessageBuilder(priorNoticeHeader, "A").GenerateMessage();
			AssertEquals(1, declaration.Messages.Count);
			AssertMultilineASCIIEquals("Expected Message",
@"B         PE                                               <<MSGNO PLACEHOLDER>>
PE10AABOLXXXDMB001002                                      MXXXD0110            
PE15HAPLUHB00000001                                                             
PE15HAAGCHB00000002                                                             
OI        MESSAGE ON MORE THAN 70 CHARACTERSMESSAGE ON MORE THAN 70 CHARACTERSME
PG01001FDAFOOFEE                                                                
PG02PFDP                                                                        
PG0639 FR                                                                       
PG07TEST FOOD                                                                   
PG10                   SOMETHING                                                
PG19PNT                  EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
OI        TEST 2                                                                
PG01002FDAFOONSF                                                                
PG02PFDP                                                                        
PG06262FR                                                                       
PG06CSHIT                                                                       
PG07TEST FOOD                                                                   
PG10                   NATURAL FOOD                                             
PG19PNT                  EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
Y         PE", declaration.Messages[0].EM_FormattedMessageText);
		}

		public void TestGenerateMessageForFTZ()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = US.Business.JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.US_EnableSPN = true;
			declaration.US_F_PNMode = PriorNoticeModeCodeList.Codes.P;
			declaration.FTZAdmissionNumber = "2140000|17|00000001";
			declaration.JE_MasterBill = "MB001002";
			declaration.JE_MasterBillIssuerSCAC = "XXXD";
			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.BLN;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "STANDALONE PRIOR NOTICE MESSAGE";
			invoiceLine.US_FDAIndicator = "D";
			invoiceLine.JI_LinePrice = 1000m;
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_FEE;
			fda.US_BrandName = "TEST FOOD";
			fda.US_Description = "SOMETHING";
			fda.US_ProdCountry = "FR";
			fda.US_ProducerType = ProducerFirmTypeList.Codes.M;
			fda.US_InvCurrValue = ZDecimal.Zero;
			fda.US_FDAForcePN = true;
			var affirmationCodes = fda.AffirmationCodes.AddNew();
			affirmationCodes.CY_Code = ACE_AffirmationOfComplianceList.Codes.FCE;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.US_FDAIndicator = "D";
			invoiceLine2.JI_Description = "Test 2";
			invoiceLine2.JI_LinePrice = 500m;
			var fda2 = invoiceLine2.ACE_FDALines.AddNew();
			fda2.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda2.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;
			fda2.US_BrandName = "TEST FOOD";
			fda2.US_Description = "NATURAL FOOD";
			fda2.US_ProdCountry = "FR";
			fda2.US_ShipmentCountry = "IT";
			fda2.US_InvCurrValue = ZDecimal.Zero;
			fda2.US_FDAForcePN = true;

			var priorNoticeWrapper = new ACEStandAlonePriorNoticeWrapper(declaration);
			AssertEquals(1, priorNoticeWrapper.PriorNoticeHeaders.Count);
			var priorNoticeHeader = priorNoticeWrapper.PriorNoticeHeaders[0];
			new ACEPriorNoticeMessageBuilder(priorNoticeHeader, "A").GenerateMessage();
			AssertEquals(1, declaration.Messages.Count);
			AssertMultilineASCIIEquals("Expected Message",
@"B         PE                                               <<MSGNO PLACEHOLDER>>
PE10AABOLXXXDMB001002                                      RXXXD8111            
OI        STANDALONE PRIOR NOTICE MESSAGE                                       
PG01001FDAFOOFEE                                                                
PG02PFDP                                                                        
PG0639 FR                                                                       
PG07TEST FOOD                                                                   
PG10                   SOMETHING                                                
PG19PNT                  EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
OI        TEST 2                                                                
PG01002FDAFOONSF                                                                
PG02PFDP                                                                        
PG06262FR                                                                       
PG06CSHIT                                                                       
PG07TEST FOOD                                                                   
PG10                   NATURAL FOOD                                             
PG19PNT                  EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
Y         PE", declaration.Messages[0].EM_FormattedMessageText);

			declaration.Messages.RemoveAndDeleteAll();
			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.FTZ;
			priorNoticeWrapper = new ACEStandAlonePriorNoticeWrapper(declaration);
			AssertEquals(1, priorNoticeWrapper.PriorNoticeHeaders.Count);
			priorNoticeHeader = priorNoticeWrapper.PriorNoticeHeaders[0];
			new ACEPriorNoticeMessageBuilder(priorNoticeHeader, "A").GenerateMessage();
			AssertEquals(1, declaration.Messages.Count);
			AssertMultilineASCIIEquals("Expected Message",
@"B         PE                                               <<MSGNO PLACEHOLDER>>
PE10AAFTZ    21400001700000001                              XXXD8111            
PE15RXXXDMB001002                                                               
OI        STANDALONE PRIOR NOTICE MESSAGE                                       
PG01001FDAFOOFEE                                                                
PG02PFDP                                                                        
PG0639 FR                                                                       
PG07TEST FOOD                                                                   
PG10                   SOMETHING                                                
PG19PNT                  EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
OI        TEST 2                                                                
PG01002FDAFOONSF                                                                
PG02PFDP                                                                        
PG06262FR                                                                       
PG06CSHIT                                                                       
PG07TEST FOOD                                                                   
PG10                   NATURAL FOOD                                             
PG19PNT                  EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
Y         PE", declaration.Messages[0].EM_FormattedMessageText);
		}

		public void TestGenerateMessageWithFME()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EntryFilerCode = "SV9";
			declaration.ImportEntryNumber = "00000005";
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.NonContainerised;
			declaration.US_EnableENS = true;
			declaration.US_EnableSPN = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.BLN;
			declaration.JE_MasterBill = "MB001002";
			declaration.PrimaryMasterBill.US_UI_NKBillIssuerSCAC = "XXXD";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "STANDALONE PRIOR NOTICE MESSAGE";

			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddMonths(1);
			tariff.UE_PGACodes = "FD4";

			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_FDAIndicator = "D";
			invoiceLine.JI_LinePrice = 1000m;
			var fda = invoiceLine.ACE_FDALines.AddNew();
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_FEE;
			fda.US_BrandName = "TEST FOOD";
			fda.US_Description = "SOMETHING";
			fda.US_ProdCountry = "FR";
			fda.US_ProducerType = ProducerFirmTypeList.Codes.M;
			fda.US_InvCurrValue = ZDecimal.Zero;
			var affirmationCodes = fda.AffirmationCodes.AddNew();
			affirmationCodes.CY_Code = ACE_AffirmationOfComplianceList.Codes.FCE;

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.US_FDAIndicator = "D";
			invoiceLine2.JI_Description = "Test 2";

			invoiceLine2.JI_Tariff = tariff.UE_Tariff;
			invoiceLine2.JI_LinePrice = 500m;
			var fda2 = invoiceLine2.ACE_FDALines.AddNew();
			fda2.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda2.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_NSF;
			fda2.US_BrandName = "TEST FOOD";
			fda2.US_Description = "NATURAL FOOD";
			fda2.US_ProdCountry = "FR";
			fda2.US_ShipmentCountry = "IT";
			fda2.US_InvCurrValue = ZDecimal.Zero;
			fda2.US_FME = FDAPriorNoticeExemptCodeList.Codes.K;

			var priorNoticeWrapper = new ACEStandAlonePriorNoticeWrapper(declaration);
			AssertEquals(1, priorNoticeWrapper.PriorNoticeHeaders.Count);
			var priorNoticeHeader = priorNoticeWrapper.PriorNoticeHeaders[0];
			new ACEPriorNoticeMessageBuilder(priorNoticeHeader, "A").GenerateMessage();
			AssertEquals(1, declaration.Messages.Count);
			AssertMultilineASCIIEquals("Expected Message",
				@"B         PE                                               <<MSGNO PLACEHOLDER>>
PE10AABOLXXXDMB001002                                      RXXXD0110            
OI        STANDALONE PRIOR NOTICE MESSAGE                                       
PG01001FDAFOOFEE                                                                
PG02PFDP                                                                        
PG0639 FR                                                                       
PG07TEST FOOD                                                                   
PG10                   SOMETHING                                                
PG19PNT                  EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
OI        TEST 2                                                                
PG01002FDAFOONSF                                                                
PG02PFDP                                                                        
PG06262FR                                                                       
PG06CSHIT                                                                       
PG07TEST FOOD                                                                   
PG10                   NATURAL FOOD                                             
PG19PNT                  EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG19PK                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG23FME  K                                                                      
Y         PE", declaration.Messages[0].EM_FormattedMessageText);
		}
	}
}
