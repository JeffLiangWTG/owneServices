using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class PGABlocksCreatorForTTBTest : PGABlocksCreatorTest
	{
		public void TestExemptionCodeTZ1InPG14()
		{
			SetUpData();

			invoiceLine.JI_Tariff = "2208905000";
			invoiceLine.JI_Description = "TEQUILA";
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Beverage;
			ttbLine.US_ProcessingCode = "T02";
			ttbLine.US_PermitExemptionCode = TTBExemptionCodeList.Codes.TTBEX1;

			var colaAndCert1 = ttbLine.COLAAndCertificates.AddNew();
			colaAndCert1.US_COLAExemptionCode = TTBExemptionCodeList.Codes.TTBEX12;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var expectedMessage = @"
50           0000000000 0000010000 000000000000PFL                              
OI        TEQUILA                                                               
PG01001TTBBERT02                                                                
PG02P                                                                           
PG14 TZ3                                                               TTBEX1   
PG14 TZ1                                                               TTBEX12  
6249900003464                                                                   
";
			AssertContains("ACE Entry Summary message including TTB data", expectedMessage, message.EM_FormattedMessageText);
		}

		//An importer is receiving a shipment containing two different brands of tequila, which are entered on a single entry line.
		//Upon entering HTS Code 2208.90.50.00 in ACE, the filer will be prompted to complete the relevant TTB Message Sets.
		public void TestTequilaFromDifferentProducers()
		{
			SetUpData();
			invoiceLine.JI_Tariff = "2208905000";
			invoiceLine.JI_Description = "TEQUILA";
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.DistilledSpirits;
			ttbLine.US_ProcessingCode = TTBDSPProcessingCodeList.Codes.T17;
			ttbLine.US_PermitNumber = "MD-S-99999";
			var colaAndCert1 = ttbLine.COLAAndCertificates.AddNew();
			colaAndCert1.US_COLA = "11419999999999";
			colaAndCert1.HasForeignCertificate = true;
			colaAndCert1.US_ForeignCertificateCountry = Core.Constants.CountryCodes.Mexico;
			var colaAndCert2 = ttbLine.COLAAndCertificates.AddNew();
			colaAndCert2.US_COLA = "11417897897897";
			colaAndCert2.HasForeignCertificate = true;
			colaAndCert2.US_ForeignCertificateCountry = Core.Constants.CountryCodes.Mexico;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"
50           0000000000 0000010000 000000000000PFL                              
OI        TEQUILA                                                               
PG01001TTBDSPT17                                                                
PG02P                                                                           
PG14 TZ3MD-S-99999                                                              
PG14 TZ111417897897897                                                          
PG50                                                                            
PG22Y11                                                                         
PG22Y863                                                                        
PG51                                                                            
PG14 TZ111419999999999                                                          
PG50                                                                            
PG22Y11                                                                         
PG22Y863                                                                        
PG51                                                                            
PG13                                   ISOMX                                    
PG50                                                                            
PG14 TZ4                                                                        
PG51                                                                            
6249900003464                                                                   
";
			AssertContains("ACE Entry Summary message including TTB data", expectedMessage, message.EM_FormattedMessageText);
		}

		//An importer is receiving a shipment of cigars in bulk, that is, not in consumer packages. This shipment is to be released
		//from customs custody without payment of tax and transported to a tobacco product manufacturer holding a TTB permit and bond
		//under the provisions of the Internal Revenue Code. Upon entering HTS Code 2402.10.80.50 in ACE, the filer will be prompted
		//to complete the relevant TTB Message Sets
		public void TestLargeCigarsNotPackagedForRetail()
		{
			SetUpData();
			var consingee = Factory.New<OrgHeader>();
			consingee.OH_Code = "IMP324KDS";
			consingee.OH_FullName = "YUMMY TOBACCO MANUFACTURING";
			consingee.OH_RL_NKClosestPort = "USOHY";
			consingee.MainAddress.OA_Address1 = "1 BUCKEYE AVE";
			consingee.MainAddress.OA_City = "WESTERVILLE";
			consingee.MainAddress.OA_State = "OH";
			consingee.MainAddress.OA_PostCode = "44081";
			consingee.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "34811-0000001", Core.Constants.CountryCodes.UnitedStates);
			invoiceLine.JI_Tariff = "2402106000";
			invoiceLine.JI_Description = "LARGE CIGARS";
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Tobacco;
			ttbLine.US_ProcessingCode = TTBTOBProcessingCodeList.Codes.T42;
			ttbLine.US_PermitNumber = "OH-TI-99999";
			ttbLine.US_IsReleaseUnderBond = true;
			ttbLine.US_OA_ConsigneeAddress = consingee.MainAddress.PK;
			ttbLine.US_NumberForIRC = "TP-OH-77777";

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"
50           0000000000 0000010000 000000000000K  000000000000KG                
OI        LARGE CIGARS                                                          
PG01001TTBTOBT42                                                                
PG02P                                                                           
PG14 TZ3OH-TI-99999                                                             
PG14 TZ5TP-OH-77777                                                             
PG50                                                                            
PG19CN 34834811-0000001  YUMMY TOBACCO MANUFACTURING     1 BUCKEYE AVE          
PG20                                     WESTERVILLE          OH US44081        
PG22Y165                                          IRC                           
PG51                                                                            
6249900003464                                                                   
";
			AssertContains("ACE Entry Summary message including TTB data", expectedMessage, message.EM_FormattedMessageText);
		}

		//An importer is receiving a shipment of 400 large cigars and 100 small cigars packaged for sale at retail.
		//Due to the customs value of the cigars, all of the 500 cigars fall under the same 10-digit HTS code, 2402.10.60.00,
		//even though the importer’s sale price is not the same for all of the cigars. (Note: This HTS Code does not distinguish
		//between small and large cigars.) Of the 400 large cigars, there are 100 each with sale prices of 39.5 cents, 49.5 cents,
		//89 cents, and 99 cents. Upon entering HTS Code 2402.10.60.00 in ACE, the filer will be prompted to complete the relevant
		//TTB Message Sets.
		public void TestMixtureOfSmallAndLargeCigars()
		{
			SetUpData();
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			invoiceLine.JI_Tariff = "2402106000";
			invoiceLine.JI_Description = "CIGARS";
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Tobacco;
			ttbLine.US_ProcessingCode = TTBTOBProcessingCodeList.Codes.T43;
			ttbLine.US_PermitNumber = "OH-TI-99999";
			var cigar1 = ttbLine.Cigars.AddNew();
			cigar1.US_Quantity = 100;
			cigar1.US_UnitPrice = 39.5m;
			var cigar2 = ttbLine.Cigars.AddNew();
			cigar2.US_Quantity = 100;
			cigar2.US_UnitPrice = 49.5m;
			var cigar3 = ttbLine.Cigars.AddNew();
			cigar3.US_Quantity = 200;
			cigar3.IsMaximumRate = ZBool.True;
			var cigar4 = ttbLine.Cigars.AddNew();
			cigar4.US_Quantity = 100;
			cigar4.US_IsSmall = true;

			var invoiceLine2 = invoiceLine.InvoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2403993070";
			invoiceLine2.JI_Description = "OTHER MANUFACTURED TOBACCO";
			invoiceLine2.US_TTBInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine2.US_TTBDisclaimReason = PGADisclaimReasonList.Codes.A;
			invoiceLine2.JI_LinePrice = 10000m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"502402106000 0000014000 0000010000 000000000000K  000000000000KG                
OI        CIGARS                                                                
PG01001TTBTOBT43                                                                
PG02P                                                                           
PG14 TZ3OH-TI-99999                                                             
PG50                                                                            
PG22                                              000100@.39500                 
PG22                                              000100@.49500                 
PG22                                              000200@MAXIMUM RATE           
PG22                                              000100 SMALL CIGARS           
PG51                                                                            
600180000000000                                                                 
";
			AssertContains("ACE Entry Summary message including TTB data", expectedMessage, message.EM_FormattedMessageText);

			expectedMessage = @"502403993070 0000000000 0000010000 000000000000KG                               
OI        OTHER MANUFACTURED TOBACCO                                            
PG01001TTBTOB                                                                  A
6249900003464                                                                   
";
			AssertContains("ACE Entry Summary message including TTB data", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestFilingTheQuantityOfCigaretteTubes()
		{
			SetUpData();
			invoiceLine.JI_Tariff = "4813100000";
			invoiceLine.JI_Description = "CIGARETTE TUBES";
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Tobacco;
			ttbLine.US_ProcessingCode = TTBTOBProcessingCodeList.Codes.T51;
			ttbLine.US_QuantityInPCS = 500000m;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"50           0000000000 0000010000 000000000000KG                               
OI        CIGARETTE TUBES                                                       
PG01001TTBTOBT51                                                                
PG02P                                                                           
PG29PCS000050000000                                                             
6249900003464                                                                   
";
			AssertContains("ACE Entry Summary message including TTB data", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestSendingCOLAExemptionCode()
		{
			SetUpData();

			invoiceLine.JI_Tariff = "4813100000";
			invoiceLine.JI_Description = "CIGARETTE TUBES";
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			ttbLine.US_ProgramCode = TTBProgramCodeList.Codes.Wine;
			ttbLine.US_ProcessingCode = TTBWINProcessingCodeList.Codes.T04;
			ttbLine.US_PermitNumber = "PR-I-765";

			var colaLine = ttbLine.COLAAndCertificates.AddNew();
			colaLine.US_COLAExemptionCode = TTBExemptionCodeList.Codes.TTBEX7;
			colaLine.US_ForeignCertificateCountry = Core.Constants.CountryCodes.HongKong;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"OI        CIGARETTE TUBES                                                       
PG01001TTBWINT04                                                                
PG02P                                                                           
PG14 TZ3PR-I-765                                                                
PG14 TZ1                                                               TTBEX7   
PG50                                                                            
PG22Y268                                                                        
PG51                                                                            
";
			AssertContains("ACE Entry Summary message including TTB COLA Exemption data", expectedMessage, message.EM_FormattedMessageText);
		}

		protected override void SetUpData()
		{
			base.SetUpData();

			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();

			declaration.JE_DateOfArrival = ZDateTime.Today;
			declaration.US_CertifyCargoRelease = true;

			var ior = Factory.New<OrgHeader>();
			declaration.IOROrgPK = ior.PK;
			ior.OH_FullName = "IMPORTER OF RECORD";
			var iorAddress = ior.MainAddress;
			iorAddress.OA_Address1 = "IOR ADDRESS 1";
			iorAddress.OA_Address2 = "IOR ADDRESS 2";
			iorAddress.OA_City = "SYDNEY";
			iorAddress.OA_State = "NSW";
			iorAddress.OA_PostCode = "2017";
			DeclarationTestHelper.AddPGAContact(ior, "IOR", "ALEXANDER THE GREATEST OF ALL", "04 123456", "IOR EMAIL", "IOR FAX");

			invoiceLine.JI_OA_ExporterAddress = ior.MainAddress.PK;
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;
			ttbLine = invoiceLine.TTBLines.AddNew();
		}
		TTBLine ttbLine;
	}
}
