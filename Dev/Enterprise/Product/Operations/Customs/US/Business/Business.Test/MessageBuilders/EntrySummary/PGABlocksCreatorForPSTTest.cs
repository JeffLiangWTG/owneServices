using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class PGABlocksCreatorForPSTTest : PGABlocksCreatorTest
	{
		[TestDate(2014, 11, 01)]
		public void TestProductTypePS1AndWithoutDetailLines()
		{
			SetUpData();

			pesticide.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
			pesticide.US_NotifyParty = PartyTypeList.Codes.Importer;
			var action = GetAction(declaration);
			pesticide.US_PSTLabelsSent = true;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        ABCDEFG DESC                                                          
PG01001EPAPS1   Y                        130.026                                
PG02P                                                                           
PG24GEN     TEST REMARKS TEXT                                                   
PG24EP5RD                                                                       
PG07UC-HDO                                                                      
PG19EPN   1234567                                                               
PG19EPN   0123456                                                               
PG19CB                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CB                                                                          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR                    04123456       IOR EMAIL                          
PG55CI NP                                                                       
PG19DEQ                  IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21DEQIOR                    04123456       IOR EMAIL                          
PG19LG                   TESTLOCAT                                              
PG20                                                             US             
PG21LG                                                                          
PG22 944         CI EP3 Y11012014                                               
PG261000000012300BG                                                             
PG262000000045600BR                                                             
PG29KG 000000078900                                                             ", message.EM_FormattedMessageText);
		}

		[TestDate(2014, 11, 01)]
		public void TestProductTypePS1AndWithOneDetailLine()
		{
			SetUpData();

			pesticide.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
			pesticide.US_NotifyParty = PartyTypeList.Codes.Importer;
			var detailLine = pesticide.PesticideLines.AddNew();
			detailLine.US_LPCOType = ProductCodeQualifiersList.Codes.ChemicalAbstractServicesNumber;
			detailLine.US_LPCONumber = "HHHHHHH";
			detailLine.US_NameOfActiveIngredient = "DDDDDDDDDD";
			detailLine.US_ActiveIngredientPercentage = 99.9999;

			var action = GetAction(declaration);
			pesticide.US_PSTLabelsSent = true;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        ABCDEFG DESC                                                          
PG01001EPAPS1   Y                        130.026                                
PG02PCAS HHHHHHH                                                                
PG04YDDDDDDDDDD                                                          0999999
PG24GEN     TEST REMARKS TEXT                                                   
PG24EP5RD                                                                       
PG07UC-HDO                                                                      
PG19EPN   1234567                                                               
PG19EPN   0123456                                                               
PG19CB                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CB                                                                          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR                    04123456       IOR EMAIL                          
PG55CI NP                                                                       
PG19DEQ                  IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21DEQIOR                    04123456       IOR EMAIL                          
PG19LG                   TESTLOCAT                                              
PG20                                                             US             
PG21LG                                                                          
PG22 944         CI EP3 Y11012014                                               
PG261000000012300BG                                                             
PG262000000045600BR                                                             
PG29KG 000000078900                                                             ", message.EM_FormattedMessageText);
		}

		[TestDate(2014, 11, 01)]
		public void TestProductTypePS1AndWithMultipleDetailLine()
		{
			SetUpData();

			pesticide.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
			pesticide.US_NotifyParty = PartyTypeList.Codes.Importer;
			var detailLineOne = pesticide.PesticideLines.AddNew();
			detailLineOne.US_LPCOType = ProductCodeQualifiersList.Codes.ChemicalAbstractServicesNumber;
			detailLineOne.US_LPCONumber = "NUMBERA";
			detailLineOne.US_NameOfActiveIngredient = "INNREDIENTA";
			detailLineOne.US_ActiveIngredientPercentage = 45.999;

			var detailLineTwo = pesticide.PesticideLines.AddNew();
			detailLineTwo.US_LPCOType = ProductCodeQualifiersList.Codes.ChemicalAbstractServicesNumber;
			detailLineTwo.US_LPCONumber = "NUMBERB";
			detailLineTwo.US_NameOfActiveIngredient = "INNREDIENTB";
			detailLineTwo.US_ActiveIngredientPercentage = 54.001;

			var action = GetAction(declaration);
			pesticide.US_PSTLabelsSent = true;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        ABCDEFG DESC                                                          
PG01001EPAPS1   Y                        130.026                                
PG02P                                                                           
PG24GEN     TEST REMARKS TEXT                                                   
PG24EP5RD                                                                       
PG07UC-HDO                                                                      
PG19EPN   1234567                                                               
PG19EPN   0123456                                                               
PG19CB                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CB                                                                          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR                    04123456       IOR EMAIL                          
PG55CI NP                                                                       
PG19DEQ                  IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21DEQIOR                    04123456       IOR EMAIL                          
PG19LG                   TESTLOCAT                                              
PG20                                                             US             
PG21LG                                                                          
PG22 944         CI EP3 Y11012014                                               
PG261000000012300BG                                                             
PG262000000045600BR                                                             
PG29KG 000000078900                                                             
PG02CCAS NUMBERA                                                                
PG04YINNREDIENTA                                                         0459990
PG02CCAS NUMBERB                                                                
PG04YINNREDIENTB                                                         0540010", message.EM_FormattedMessageText);
		}

		[TestDate(2014, 11, 01)]
		public void TestProductTypePS2WithoutDetailLines()
		{
			SetUpData();

			pesticide.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
			pesticide.US_NotifyParty = PartyTypeList.Codes.Importer;
			pesticide.US_ProductType = PSTProductTypeList.Codes.PS2;

			var action = GetAction(declaration);
			pesticide.US_PSTLabelsSent = true;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        ABCDEFG DESC                                                          
PG01001EPAPS2   Y                                                               
PG02P                                                                           
PG24GEN     TEST REMARKS TEXT                                                   
PG24EP5RD                                                                       
PG07UC-HDO                                                                      
PG19EPN   1234567                                                               
PG19EPN   0123456                                                               
PG19CB                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CB                                                                          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR                    04123456       IOR EMAIL                          
PG55CI NP                                                                       
PG19DEQ                  IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21DEQIOR                    04123456       IOR EMAIL                          
PG19LG                   TESTLOCAT                                              
PG20                                                             US             
PG21LG                                                                          
PG22 944         CI EP3 Y11012014                                               
PG261000000012300BG                                                             
PG262000000045600BR                                                             
PG29KG 000000078900                                                             ", message.EM_FormattedMessageText);
		}

		[TestDate(2014, 11, 01)]
		public void TestProductTypePS3WithoutDetailLines()
		{
			SetUpData();

			pesticide.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
			pesticide.US_NotifyParty = PartyTypeList.Codes.Importer;
			pesticide.US_ProductType = PSTProductTypeList.Codes.PS3;

			var action = GetAction(declaration);
			pesticide.US_PSTLabelsSent = true;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        ABCDEFG DESC                                                          
PG01001EPAPS3   Y                        130.026                                
PG02P                                                                           
PG24GEN     TEST REMARKS TEXT                                                   
PG24EP5RD                                                                       
PG07UC-HDO                                                                      
PG19EPN   1234567                                                               
PG19EPN   0123456                                                               
PG19CB                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CB                                                                          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR                    04123456       IOR EMAIL                          
PG55CI NP                                                                       
PG19DEQ                  IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21DEQIOR                    04123456       IOR EMAIL                          
PG19LG                   TESTLOCAT                                              
PG20                                                             US             
PG21LG                                                                          
PG22 944         CI EP3 Y11012014                                               
PG261000000012300BG                                                             
PG262000000045600BR                                                             
PG29KG 000000078900                                                             ", message.EM_FormattedMessageText);
		}

		[TestDate(2014, 11, 01)]
		public void TestProductTypePS3AndWithOneDetailLine()
		{
			SetUpData();

			pesticide.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
			pesticide.US_NotifyParty = PartyTypeList.Codes.Importer;
			pesticide.US_ProductType = PSTProductTypeList.Codes.PS3;

			var detailLine = pesticide.PesticideLines.AddNew();
			detailLine.US_LPCOType = ProductCodeQualifiersList.Codes.ChemicalAbstractServicesNumber;
			detailLine.US_LPCONumber = "HHHHHHH";
			detailLine.US_NameOfActiveIngredient = "DDDDDDDDDD";
			detailLine.US_ActiveIngredientPercentage = 99.9999;

			var action = GetAction(declaration);
			pesticide.US_PSTLabelsSent = true;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        ABCDEFG DESC                                                          
PG01001EPAPS3   Y                        130.026                                
PG02PCAS HHHHHHH                                                                
PG04YDDDDDDDDDD                                                          0999999
PG24GEN     TEST REMARKS TEXT                                                   
PG24EP5RD                                                                       
PG07UC-HDO                                                                      
PG19EPN   1234567                                                               
PG19EPN   0123456                                                               
PG19CB                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CB                                                                          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR                    04123456       IOR EMAIL                          
PG55CI NP                                                                       
PG19DEQ                  IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21DEQIOR                    04123456       IOR EMAIL                          
PG19LG                   TESTLOCAT                                              
PG20                                                             US             
PG21LG                                                                          
PG22 944         CI EP3 Y11012014                                               
PG261000000012300BG                                                             
PG262000000045600BR                                                             
PG29KG 000000078900                                                             ", message.EM_FormattedMessageText);
		}

		public void TestProductTypePS3AndWithMoreThanOneDetailLine()
		{
			SetUpData();

			pesticide.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
			pesticide.US_NotifyParty = PartyTypeList.Codes.Importer;
			pesticide.US_ProductType = PSTProductTypeList.Codes.PS3;
			pesticide.US_UnregReasonCode = "DSP";
			((IPSTData)pesticide).CertifySignatureDate = new ZDate(2017, 03, 08);

			var detailLine = pesticide.PesticideLines.AddNew();
			detailLine.US_LPCOType = ProductCodeQualifiersList.Codes.ChemicalAbstractServicesNumber;
			detailLine.US_LPCONumber = "HHHHHHH";
			detailLine.US_NameOfActiveIngredient = "DDDDDDDDDD";
			detailLine.US_ActiveIngredientPercentage = 99.9999;

			var detailLine2 = pesticide.PesticideLines.AddNew();
			detailLine2.US_LPCOType = ProductCodeQualifiersList.Codes.AccessionNumber;
			detailLine2.US_LPCONumber = "AAAAA";
			detailLine2.US_NameOfActiveIngredient = "BBBBB";
			detailLine2.US_ActiveIngredientPercentage = 8.25;

			var action = GetAction(declaration);
			pesticide.US_PSTLabelsSent = true;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var messageText = message.EM_FormattedMessageText;
			Assert(messageText.Contains(@"OI        ABCDEFG DESC                                                          
PG01001EPAPS3   Y                        130.026                                
PG02P                                                                           
PG24GEN     TEST REMARKS TEXT                                                   
PG24EP5DSP                                                                      
PG07UC-HDO                                                                      
PG19EPN   1234567                                                               
PG19EPN   0123456                                                               
PG19CB                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CB                                                                          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR                    04123456       IOR EMAIL                          
PG55CI NP                                                                       
PG19DEQ                  IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21DEQIOR                    04123456       IOR EMAIL                          
PG19LG                   TESTLOCAT                                              
PG20                                                             US             
PG21LG                                                                          
PG22 944         CI EP3 Y03082017                                               
PG261000000012300BG                                                             
PG262000000045600BR                                                             
PG29KG 000000078900                                                             
PG02CCAS HHHHHHH                                                                
PG04YDDDDDDDDDD                                                          0999999
PG02CACC AAAAA                                                                  
PG04YBBBBB                                                               0082500"));
		}

		[TestDate(2014, 11, 01)]
		public void TestCertifyingIndividualIsBRK()
		{
			SetUpData();

			pesticide.US_CertifyingIndividual = PartyTypeList.Codes.CustomsBroker;
			pesticide.US_NotifyParty = PartyTypeList.Codes.Importer;
			var detailLine = pesticide.PesticideLines.AddNew();
			detailLine.US_LPCOType = ProductCodeQualifiersList.Codes.ChemicalAbstractServicesNumber;
			detailLine.US_LPCONumber = "HHHHHHH";
			detailLine.US_NameOfActiveIngredient = "DDDDDDDDDD";
			detailLine.US_ActiveIngredientPercentage = 99.9999;

			var action = GetAction(declaration);
			pesticide.US_PSTLabelsSent = true;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        ABCDEFG DESC                                                          
PG01001EPAPS1   Y                        130.026                                
PG02PCAS HHHHHHH                                                                
PG04YDDDDDDDDDD                                                          0999999
PG24GEN     TEST REMARKS TEXT                                                   
PG24EP5RD                                                                       
PG07UC-HDO                                                                      
PG19EPN   1234567                                                               
PG19EPN   0123456                                                               
PG19CB                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CB                                                                          
PG55CI                                                                          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR                    04123456       IOR EMAIL                          
PG55NP                                                                          
PG19DEQ                  IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21DEQIOR                    04123456       IOR EMAIL                          
PG19LG                   TESTLOCAT                                              
PG20                                                             US             
PG21LG                                                                          
PG22 944         CI EP3 Y11012014                                               
PG261000000012300BG                                                             
PG262000000045600BR                                                             
PG29KG 000000078900                                                             ", message.EM_FormattedMessageText);
		}

		[TestDate(2014, 11, 01)]
		public void TestCertifyingIndividualIsSHP()
		{
			SetUpData();

			pesticide.US_CertifyingIndividual = PartyTypeList.Codes.Shipper;
			pesticide.US_NotifyParty = PartyTypeList.Codes.Importer;
			var detailLine = pesticide.PesticideLines.AddNew();
			detailLine.US_LPCOType = ProductCodeQualifiersList.Codes.ChemicalAbstractServicesNumber;
			detailLine.US_LPCONumber = "HHHHHHH";
			detailLine.US_NameOfActiveIngredient = "DDDDDDDDDD";
			detailLine.US_ActiveIngredientPercentage = 99.9999;

			var action = GetAction(declaration);
			pesticide.US_PSTLabelsSent = true;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        ABCDEFG DESC                                                          
PG01001EPAPS1   Y                        130.026                                
PG02PCAS HHHHHHH                                                                
PG04YDDDDDDDDDD                                                          0999999
PG24GEN     TEST REMARKS TEXT                                                   
PG24EP5RD                                                                       
PG07UC-HDO                                                                      
PG19EPN   1234567                                                               
PG19EPN   0123456                                                               
PG19CB                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CB                                                                          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR                    04123456       IOR EMAIL                          
PG55NP                                                                          
PG19DEQ                  IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21DEQIOR                    04123456       IOR EMAIL                          
PG55CI                                                                          
PG19LG                   TESTLOCAT                                              
PG20                                                             US             
PG21LG                                                                          
PG22 944         CI EP3 Y11012014                                               
PG261000000012300BG                                                             
PG262000000045600BR                                                             
PG29KG 000000078900                                                             ", message.EM_FormattedMessageText);
		}

		[TestDate(2014, 11, 01)]
		public void TestNotifyPartyIsBRK()
		{
			SetUpData();

			pesticide.US_CertifyingIndividual = PartyTypeList.Codes.Shipper;
			pesticide.US_NotifyParty = PartyTypeList.Codes.CustomsBroker;
			var detailLine = pesticide.PesticideLines.AddNew();
			detailLine.US_LPCOType = ProductCodeQualifiersList.Codes.ChemicalAbstractServicesNumber;
			detailLine.US_LPCONumber = "HHHHHHH";
			detailLine.US_NameOfActiveIngredient = "DDDDDDDDDD";
			detailLine.US_ActiveIngredientPercentage = 99.9999;

			var action = GetAction(declaration);
			pesticide.US_PSTLabelsSent = true;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        ABCDEFG DESC                                                          
PG01001EPAPS1   Y                        130.026                                
PG02PCAS HHHHHHH                                                                
PG04YDDDDDDDDDD                                                          0999999
PG24GEN     TEST REMARKS TEXT                                                   
PG24EP5RD                                                                       
PG07UC-HDO                                                                      
PG19EPN   1234567                                                               
PG19EPN   0123456                                                               
PG19CB                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CB                                                                          
PG55NP                                                                          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR                    04123456       IOR EMAIL                          
PG19DEQ                  IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21DEQIOR                    04123456       IOR EMAIL                          
PG55CI                                                                          
PG19LG                   TESTLOCAT                                              
PG20                                                             US             
PG21LG                                                                          
PG22 944         CI EP3 Y11012014                                               
PG261000000012300BG                                                             
PG262000000045600BR                                                             
PG29KG 000000078900                                                             ", message.EM_FormattedMessageText);
		}

		[TestDate(2014, 11, 01)]
		public void TestNotifyPartyIsBlank()
		{
			SetUpData();

			pesticide.US_CertifyingIndividual = PartyTypeList.Codes.Shipper;
			pesticide.US_NotifyParty = ZString.Empty;
			var detailLine = pesticide.PesticideLines.AddNew();
			detailLine.US_LPCOType = ProductCodeQualifiersList.Codes.ChemicalAbstractServicesNumber;
			detailLine.US_LPCONumber = "HHHHHHH";
			detailLine.US_NameOfActiveIngredient = "DDDDDDDDDD";
			detailLine.US_ActiveIngredientPercentage = 99.9999;

			var action = GetAction(declaration);
			pesticide.US_PSTLabelsSent = true;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"OI        ABCDEFG DESC                                                          
PG01001EPAPS1   Y                        130.026                                
PG02PCAS HHHHHHH                                                                
PG04YDDDDDDDDDD                                                          0999999
PG24GEN     TEST REMARKS TEXT                                                   
PG24EP5RD                                                                       
PG07UC-HDO                                                                      
PG19EPN   1234567                                                               
PG19EPN   0123456                                                               
PG19CB                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET    
PG20ALBION  QLD                                                  AU4010         
PG21CB                                                                          
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR                    04123456       IOR EMAIL                          
PG19DEQ                  IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21DEQIOR                    04123456       IOR EMAIL                          
PG55CI                                                                          
PG19LG                   TESTLOCAT                                              
PG20                                                             US             
PG21LG                                                                          
PG22 944         CI EP3 Y11012014                                               
PG261000000012300BG                                                             
PG262000000045600BR                                                             
PG29KG 000000078900                                                             ", message.EM_FormattedMessageText);
		}

		public void TestPSTMultiplePG26s()
		{
			SetUpData();

			pesticide.US_NoOfUnit1 = 1;
			pesticide.US_NoOfUnit2 = 0;
			pesticide.US_NoOfUnit3 = 2;
			pesticide.US_NoOfUnit4 = 0;
			pesticide.US_NoOfUnit5 = 3;
			pesticide.US_NoOfUnit6 = 0;

			pesticide.US_UQ1 = "A1";
			pesticide.US_UQ2 = "";
			pesticide.US_UQ3 = "A3";
			pesticide.US_UQ4 = "";
			pesticide.US_UQ5 = "A5";
			pesticide.US_UQ6 = "";

			var action = GetAction(declaration);
			pesticide.US_PSTLabelsSent = true;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			AssertContains(
@"PG261000000000100A1                                                             
PG262                                                                           
PG263000000000200A3                                                             
PG264000000000300A5                                                             
", message.EM_FormattedMessageText);

			pesticide.US_NoOfUnit1 = 1;
			pesticide.US_NoOfUnit2 = 2;
			pesticide.US_NoOfUnit3 = 3;
			pesticide.US_NoOfUnit4 = 4;
			pesticide.US_NoOfUnit5 = 5;
			pesticide.US_NoOfUnit6 = 6;

			pesticide.US_UQ2 = "A2";
			pesticide.US_UQ4 = "A4";
			pesticide.US_UQ6 = "A6";

			action = GetAction(declaration);
			pesticide.US_PSTLabelsSent = true;
			builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			message = builder.PopulateMessage();
			AssertContains(
@"PG261000000000100A1                                                             
PG262000000000200A2                                                             
PG263000000000300A3                                                             
PG264000000000400A4                                                             
PG265000000000500A5                                                             
PG266000000000600A6                                                             
", message.EM_FormattedMessageText);
		}

		[TestDate(2014, 11, 01)]
		public void TestCustomsBrokerDetails()
		{
			SetUpData();
			Enterprise.Customs.US.Business.Testing.DeclarationTestHelper.SetEntryFilerCode("XJ5");

			pesticide.US_CertifyingIndividual = PartyTypeList.Codes.Importer;
			pesticide.US_NotifyParty = PartyTypeList.Codes.Importer;
			var orgProxy = declaration.Branch.OrgProxy;
			UpdateAddress(orgProxy.MainAddress, "Proxy Address Line 1", "Proxy Address Line 2", "Chicago City", "CH", "86954-3251", "+1 641 8564 8653", "+1 641 8564 8654", "info@proxy.com");
			var customsAddress = AddCustomsAddress(orgProxy, "Proxy Customs Address Line 1", "Proxy Customs Address Line 2", "Michigan City", "IL", "596508654", "+1 642 8564 8653", "+1 642 8564 8654", "customs@proxy.com");
			var contact = orgProxy.Contacts.AddNew();
			contact.OC_ContactName = "BOB THE BUILDER";
			contact.OC_Email = "BOB@WHERE.COM";
			declaration.US_FDAContactName = "WENDY THE DESTROYER";
			declaration.US_FDAContactPhoneNo = "+164285648734";
			declaration.US_FDAContactEmail = "WENDY@WHERE.COM";
			var action = GetAction(declaration);
			pesticide.US_PSTLabelsSent = true;
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var expectedMessage = @"OI        ABCDEFG DESC                                                          
PG01001EPAPS1   Y                        130.026                                
PG02P                                                                           
PG24GEN     TEST REMARKS TEXT                                                   
PG24EP5RD                                                                       
PG07UC-HDO                                                                      
PG19EPN   1234567                                                               
PG19EPN   0123456                                                               
PG19CB                   EDI CUSTOMS BROKERS             PROXY CUSTOMS ADDRESS L
PG60AD1INE 1                                                                    
PG20PROXY CUSTOMS ADDRESS LINE 2         MICHIGAN CITY        IL US596508654    
PG21CB WENDY THE DESTROYER    164285648734   WENDY@WHERE.COM                    
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR                    04123456       IOR EMAIL                          
PG55CI NP                                                                       
PG19DEQ                  IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21DEQIOR                    04123456       IOR EMAIL                          
PG19LG                   TESTLOCAT                                              
PG20                                                             US             
PG21LG                                                                          
PG22 944         CI EP3 Y11012014                                               
PG261000000012300BG                                                             
PG262000000045600BR                                                             
PG29KG 000000078900                                                             ";
			Assert(message.EM_FormattedMessageText.Contains(expectedMessage));
			invoiceLine.InvoiceHeader.US_PSTSignDate = ZDateTime.Empty;
			customsAddress.Delete();
			message = builder.PopulateMessage();
			AssertContains("Hyphen removed from post code", "PG20PROXY ADDRESS LINE 2                 CHICAGO CITY            AU869543251    ", message.EM_MessageText);
			expectedMessage = @"OI        ABCDEFG DESC                                                          
PG01001EPAPS1   Y                        130.026                                
PG02P                                                                           
PG24GEN     TEST REMARKS TEXT                                                   
PG24EP5RD                                                                       
PG07UC-HDO                                                                      
PG19EPN   1234567                                                               
PG19EPN   0123456                                                               
PG19CB                   EDI CUSTOMS BROKERS             PROXY ADDRESS LINE 1   
PG20PROXY ADDRESS LINE 2                 CHICAGO CITY            AU869543251    
PG21CB WENDY THE DESTROYER    164285648734   WENDY@WHERE.COM                    
PG19IM                   IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21IM IOR                    04123456       IOR EMAIL                          
PG55CI NP                                                                       
PG19DEQ                  IMPORTER OF RECORD              IOR ADDRESS 1          
PG20IOR ADDRESS 2                        SYDNEY               NSWUS2017         
PG21DEQIOR                    04123456       IOR EMAIL                          
PG19LG                   TESTLOCAT                                              
PG20                                                             US             
PG21LG                                                                          
PG22 944         CI EP3 Y11012014                                               
PG261000000012300BG                                                             
PG262000000045600BR                                                             
PG29KG 000000078900                                                             ";
			Assert(message.EM_FormattedMessageText.Contains(expectedMessage));
		}

		public void TestCertifyingDateIsSetToCurrentDateIfEmpty()
		{
			SetUpData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "ACE";
			declaration.US_CertifyCargoRelease = true;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = "SE";

			invoiceLine.InvoiceHeader.US_PSTSignDate = ZDateTime.Empty;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var today = ZDate.Today;
			AssertContains("PG22 944         CI EP3 Y" + today.ToString("MMddyyyy"), message.EM_FormattedMessageText); //11212016

			invoiceLine.InvoiceHeader.US_PSTSignDate = new ZDateTime(2016, 11, 29);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			action = GetAction(declaration);
			builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			message = builder.PopulateMessage();
			AssertContains("PG22 944         CI EP3 Y11292016", message.EM_FormattedMessageText);
		}

		protected override void SetUpData()
		{
			base.SetUpData();

			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.US_CertifyCargoRelease = true;

			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "TV";
			declaration.JE_GS_NKCusAgent = broker.GS_Code;
			broker.GS_FullName = "BROKER";
			broker.GS_WorkPhone = "04 010101";
			broker.GS_EmailAddress = "BROKER EMAIL";
			broker.GS_FaxNum = "BROKER FAX";

			var ior = Factory.New<OrgHeader>();
			declaration.IOROrgPK = ior.PK;
			ior.OH_FullName = "IMPORTER OF RECORD";
			var iorAddress = ior.MainAddress;
			iorAddress.OA_Address1 = "IOR ADDRESS 1";
			iorAddress.OA_Address2 = "IOR ADDRESS 2";
			iorAddress.OA_City = "SYDNEY";
			iorAddress.OA_RL_NKRelatedPortCode = "US2CW";
			iorAddress.OA_State = "NSW";
			iorAddress.OA_PostCode = "2017";
			DeclarationTestHelper.AddPGAContact(ior, "IOR", "", "04 123456", "IOR EMAIL", "IOR FAX");

			invoiceLine.JI_Description = "ABCDEFG DESC";
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			ClearPGAContactInfo(declaration);
			pesticide = invoiceLine.PSTLines.AddNew();
			pesticide.US_ProductType = PSTProductTypeList.Codes.PS1;
			pesticide.US_IntendedUseCode = PSTIntendedUseCodesList.Codes._130026;
			pesticide.US_UnregReasonCode = PSTRemarksCodeList.Codes.RD;
			pesticide.US_UnregReasonRemarks = "TEST REMARKS TEXT";
			pesticide.US_BrandName = "UC-HDO";
			pesticide.US_ProducerEstNo = "0123456";
			pesticide.US_ProducerEstNoForeign = "1234567";
			pesticide.US_OA_ShipperAddress = ior.MainAddress.PK;

			var locationOrg = Factory.New<OrgHeader>();
			locationOrg.OH_FullName = "TESTLOCAT";

			pesticide.US_OA_ExaminationLocation = locationOrg.MainAddress.PK;
			pesticide.US_NoOfUnit1 = 123m;
			pesticide.US_UQ1 = ShippingOrPackingingUnitList.Codes.Bag;
			pesticide.US_NoOfUnit2 = 456m;
			pesticide.US_UQ2 = ShippingOrPackingingUnitList.Codes.Bar;
			pesticide.US_NetWeight = 789m;
			pesticide.US_WeightUQ = Core.Constants.Weight.Kilograms;
		}
		Pesticide pesticide;
	}
}
