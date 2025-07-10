using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	sealed class PGABlocksCreatorForFWSTest : PGABlocksCreatorTest
	{
		public void TestDisclaim()
		{
			SetUpData();
			invoiceLine.JI_Tariff = "0304590003";
			invoiceLine.JI_Description = "SCALLOPED HAMMERHEAD SHARK";
			invoiceLine.FWSHeaders.RemoveAndDeleteAll();
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Disclaimed;
			invoiceLine.US_FWSDisclaimReason = PGADisclaimReasonList.Codes.A;

			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			var expectedMessage = @"50           0000000000 0000010000                                              
OI        SCALLOPED HAMMERHEAD SHARK                                            
PG01001FWSFWS                                                                  A
";
			AssertContains("ACE Entry Summary message including FWS data", expectedMessage, message.EM_FormattedMessageText);
		}

		public void TestFWSMessage()
		{
			SetUpData();
			declaration.US_EntryType = "02";
			invoiceLine.JI_Tariff = "0304590003";
			invoiceLine.JI_Description = "SCALLOPED HAMMERHEAD SHARK";
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;

			// PG01
			header.US_IsDocSubmitted = ZBool.True;
			header.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			header.US_ProductType = GlobalUniqueProductCodeQualifierList.Codes.SRV;
			header.US_ProductNumber = "SRV3242";

			// PG05 
			header.US_ScientificGenusName = "SPHYRNA";
			header.US_ScientificSpeciesName = "LEWINII";
			header.US_ScientificSubSpeciesName = "SCALLOPED";
			header.US_WildlifeCategoryCode = FWSWildlifeCategoryCodesList.Codes.FishOther;
			header.US_WildlifeDescriptionCode = FWSWildlifeDescriptionCodesList.Codes.LIV;

			header.US_Hybrid = FWSHybridTypeList.Codes.Intergeneric;
			// Second PG05 when hybrid
			header.US_Scientific2GenusName = "SPHYRNA";
			header.US_Scientific2SpeciesName = "MOKKARAN";
			header.US_Scientific2SubSpeciesName = "GREAT";

			// PG06
			header.US_SpeciesOrigin = Core.Constants.CountryCodes.SouthAfrica;

			// PG10
			header.US_WildlifeSource = FWSWildlifeSourceList.Codes.W;

			// PG14
			var license1 = header.Licenses.AddNew();
			license1.US_Type = FWSLicenseTypeList.Codes.ForeignWildlifeExportDocument;
			license1.US_Number = "382332KD";
			var license2 = header.Licenses.AddNew();
			license2.US_Type = FWSLicenseTypeList.Codes.FWSImportExportLicense;
			license2.US_Number = "83455333";

			// PG17
			header.US_CommoditySpecificName = "HAMMERHEAD SHARK A SCALLOPED";
			header.US_CommodityGeneralName = "HAMMERHEAD SHARK";
			header.US_IsLiveVenomous = true;
			header.US_CartonQty = 1;

			// PG19, PG20 and PG21
			header.US_OA_FWSImporterAddress = ImporterAddress.PK;
			header.US_OA_FWSExporterAddress = ExporterAddress.PK;

			var address = declaration.Branch.OrgProxy.MainAddress;
			address.CompanyName = "USA LOGISTICS COMPANY";
			address.OA_Address1 = "1501 WOODFIELD RD";
			address.OA_Address2 = "Address Line 2";
			address.OA_City = "SCHAUMBURG";
			address.OA_State = "IL";
			address.OA_PostCode = "60173";
			address.OA_RN_NKCountryCode = "US";
			address.OA_RL_NKRelatedPortCode = "USCHI";

			var invoice = invoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "US CHICAGO FACILITATOR";
			invoice.US_FDAContactPhoneNo = "3125551212";
			invoice.US_FDAContactEmail = "US.CHICAGO@USCORPORATION.COM";

			// PG24
			header.US_RemarksText = "HOW LONG SHOULD A REMARK BE TO BE CONSIDERED TOO LONG REMARKS?";

			// PG25
			header.US_Value = 300m;

			// PG27
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT1";
			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CONT2";
			var container3 = declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "CONT3";
			var containerPivots = invoiceLine.ContainersForInvoiceLinesForBindingOnly.Cast<NonPersistentCusContainer>().OrderBy(x => x.ContainerNumber).ToArray();
			containerPivots[0].IsForInvoiceLine = true;
			containerPivots[1].IsForInvoiceLine = false;
			containerPivots[2].IsForInvoiceLine = true;

			// PG29
			header.US_NetCommodityUQ = FWSUnitOfMeasureList.Codes.Meters;
			header.US_NetCommodity = 1.55m;

			// PG30
			header.US_FIRMS = "H323";
			declaration.JE_DateOfArrival = ZDateTime.BrettsBirthday;

			invoiceLine.InvoiceHeader.US_FWSSignDate = new ZDateTime(2015, 8, 20);
			var expectedMessage = @"OI        SCALLOPED HAMMERHEAD SHARK                                            
PG01001FWSFWSEDSY SRV SRV3242                                                   
PG02P                                                                           
PG05SPHYRNA               LEWINII               SCALLOPED         FSH    LIVA100
PG05SPHYRNA               MOKKARAN              GREAT             FSH    LIVA100
PG06267ZA                                                                       
PG10           W                                                                
PG14 FWE382332KD                                                                
PG14 FWL83455333                                                                
PG17HAMMERHEAD SHARK A SCALLOPED  HAMMERHEAD SHARK              Y00001          
PG19FWE                  FWS EXPORTER                    EXP ADDRESS 1          
PG20EXP ADDRESS 2                                                AU             
PG21FWE                                                                         
PG19CB                   USA LOGISTICS COMPANY           1501 WOODFIELD RD      
PG20ADDRESS LINE 2                       SCHAUMBURG           IL US60173        
PG21CB US CHICAGO FACILITATOR 3125551212     US.CHICAGO@USCORPORATION.COM       
PG19FWI78 FWE132123      FWS IMPORTER                    IMP ADDRESS 1          
PG20IMP ADDRESS 2                                             IL US             
PG21FWIBOB                    00123456       KNZ@ABC.COM                        
PG22             FWIFW3 Y08202015                                               
PG24GEN     HOW LONG SHOULD A REMARK BE TO BE CONSIDERED TOO LONG REMARKS?      
PG25                                                    000000000300            
PG27CONT1                  CONT3                                                
PG29MT 000000000155                                                             
PG30I09181971    4   H323                                                       ";

			var generatedMessage = GenerateMessageForProcessingCode(FWSProcessingCodeList.Codes.EDS);
			AssertContains("ACE Entry Summary message including FWS data with code EDS", expectedMessage, generatedMessage);
		}

		public void TestGeneratePG17()
		{
			SetUpData();
			header.US_CommodityGeneralName = "HAMMERHEAD SHARK";
			header.US_IsLiveVenomous = true;
			header.US_CartonQty = 1;

			var expectedMessage = @"PG17                              HAMMERHEAD SHARK              Y00001          ";
			var generatedMessage = GenerateMessageForProcessingCode(FWSProcessingCodeList.Codes.EDS);
			AssertContains("PG17 is generated even if no Commodity Specific Name is entered", expectedMessage, generatedMessage);
		}

		public void TestGeneratePG19()
		{
			SetUpData();

			declaration.Company.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.FWSeDecsAccountNumber, "FWE654321", Core.Constants.CountryCodes.UnitedStates);
			var expectedMessage = "PG19CB 78 FWE654321      EDI CUSTOMS BROKERS             10 HUTCHESON STREET";
			var generatedMessage = GenerateMessageForProcessingCode(FWSProcessingCodeList.Codes.EDS);
			AssertContains(expectedMessage, generatedMessage);

			declaration.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.FWSeDecsAccountNumber, "FWE123456", Core.Constants.CountryCodes.UnitedStates);
			expectedMessage = "PG19CB 78 FWE123456      EDI CUSTOMS BROKERS             10 HUTCHESON STREET";
			generatedMessage = GenerateMessageForProcessingCode(FWSProcessingCodeList.Codes.EDS);
			AssertContains(expectedMessage, generatedMessage);

			header.US_OA_FWSImporterAddress = ImporterAddress.PK;
			expectedMessage = "PG19CB                   EDI CUSTOMS BROKERS             10 HUTCHESON STREET";
			generatedMessage = GenerateMessageForProcessingCode(FWSProcessingCodeList.Codes.EDS);
			AssertContains(expectedMessage, generatedMessage);

			expectedMessage = "PG19FWI78 FWE132123      FWS IMPORTER                    IMP ADDRESS 1          ";
			AssertContains(expectedMessage, generatedMessage);
		}

		public void TestGeneratePG22()
		{
			SetUpData();
			invoiceLine.InvoiceHeader.US_FWSSignDate = new ZDateTime(2015, 8, 20);

			var address = declaration.Branch.OrgProxy.MainAddress;
			address.CompanyName = "USA LOGISTICS COMPANY";
			address.OA_Address1 = "1501 WOODFIELD RD";
			address.OA_Address2 = "Address Line 2";
			address.OA_City = "SCHAUMBURG";
			address.OA_State = "IL";
			address.OA_PostCode = "60173";
			address.OA_RN_NKCountryCode = "US";
			address.OA_RL_NKRelatedPortCode = "USCHI";

			var invoice = invoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "US CHICAGO FACILITATOR";
			invoice.US_FDAContactPhoneNo = "3125551212";
			invoice.US_FDAContactEmail = "US.CHICAGO@USCORPORATION.COM";

			header.US_OA_FWSImporterAddress = ImporterAddress.PK;
			header.US_OA_FWSExporterAddress = ExporterAddress.PK;

			var expectedCBBlock = @"PG19CB                   USA LOGISTICS COMPANY           1501 WOODFIELD RD      
PG20ADDRESS LINE 2                       SCHAUMBURG           IL US60173        
PG21CB US CHICAGO FACILITATOR 3125551212     US.CHICAGO@USCORPORATION.COM       
";
			var generatedMessage = GenerateMessageForProcessingCode(FWSProcessingCodeList.Codes.EDS);
			AssertContains("PGA22 created for GB Entity block", expectedCBBlock, generatedMessage);

			var expectedFWIBlock = @"PG19FWI78 FWE132123      FWS IMPORTER                    IMP ADDRESS 1          
PG20IMP ADDRESS 2                                             IL US             
PG21FWIBOB                    00123456       KNZ@ABC.COM                        
PG22             FWIFW3 Y08202015                                               
";
			generatedMessage = GenerateMessageForProcessingCode(FWSProcessingCodeList.Codes.EDS);
			AssertContains("PGA22 created for FWI Entity block", expectedFWIBlock, generatedMessage);

			var expectedFWEBlock = @"PG19FWE                  FWS EXPORTER                    EXP ADDRESS 1          
PG20EXP ADDRESS 2                                                AU             
PG21FWE                                                                         
";
			generatedMessage = GenerateMessageForProcessingCode(FWSProcessingCodeList.Codes.EDS);
			AssertContains("PGA22 created for FWE Entity block", expectedFWEBlock, generatedMessage);
		}

		public void TestEDSGeneratesCBBlock()
		{
			SetUpData();
			var address = declaration.Branch.OrgProxy.MainAddress;
			address.CompanyName = "USA LOGISTICS COMPANY";
			address.OA_Address1 = "1501 WOODFIELD RD";
			address.OA_Address2 = "Address Line 2";
			address.OA_City = "SCHAUMBURG";
			address.OA_State = "IL";
			address.OA_PostCode = "60173";
			address.OA_RN_NKCountryCode = "US";
			address.OA_RL_NKRelatedPortCode = "USCHI";

			var invoice = invoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "US CHICAGO FACILITATOR";
			invoice.US_FDAContactPhoneNo = "3125551212";
			invoice.US_FDAContactEmail = "US.CHICAGO@USCORPORATION.COM";

			var expectedCBBlock = @"PG19CB 78                USA LOGISTICS COMPANY           1501 WOODFIELD RD      
PG20ADDRESS LINE 2                       SCHAUMBURG           IL US60173        
PG21CB US CHICAGO FACILITATOR 3125551212     US.CHICAGO@USCORPORATION.COM       
";
			var generatedMessage = GenerateMessageForProcessingCode(FWSProcessingCodeList.Codes.EDS);
			AssertContains("ACE Entry Summary message includes PG21CB Block for code EDS", expectedCBBlock, generatedMessage);
		}

		public void TestLDSGeneratesPGA01PGA02PGA14BlocksOnly()
		{
			SetUpData();
			var address = declaration.Branch.OrgProxy.MainAddress;
			address.CompanyName = "USA LOGISTICS COMPANY";
			address.OA_Address1 = "1501 WOODFIELD RD";
			address.OA_Address2 = "Address Line 2";
			address.OA_City = "SCHAUMBURG";
			address.OA_State = "IL";
			address.OA_PostCode = "60173";
			address.OA_RN_NKCountryCode = "US";
			address.OA_RL_NKRelatedPortCode = "USCHI";

			var invoice = invoiceLine.InvoiceHeader;
			invoice.US_FDAContactName = "US CHICAGO FACILITATOR";
			invoice.US_FDAContactPhoneNo = "3125551212";
			invoice.US_FDAContactEmail = "US.CHICAGO@USCORPORATION.COM";

			var license = header.Licenses.AddNew();
			license.US_Type = "FWC";
			license.US_Number = "123456";

			var generatedMessage = GenerateMessageForProcessingCode(FWSProcessingCodeList.Codes.LDS);
			AssertContains("ACE Entry Summary message includes PGA01 Block for code LDS", "PG01", generatedMessage);
			AssertContains("ACE Entry Summary message includes PGA02 Block for code LDS", "PG02", generatedMessage);
			AssertContains("ACE Entry Summary message includes PGA14 Block for code LDS", "PG14", generatedMessage);
			AssertNotContains("ACE Entry Summary message includes PGA14 Block for code LDS", "PG19", generatedMessage);
			AssertNotContains("ACE Entry Summary message includes PGA14 Block for code LDS", "PG22", generatedMessage);
			AssertNotContains("ACE Entry Summary message includes PGA14 Block for code LDS", "PG30", generatedMessage);
		}

		public void TestCertifyingDateIsSetToCurrentDateIfEmpty()
		{
			SetUpData();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = "ACE";
			declaration.US_CertifyCargoRelease = true;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = "SE";
			header.US_ProcessingCode = FWSProcessingCodeList.Codes.EDS;
			invoiceLine.InvoiceHeader.US_FWSSignDate = ZDateTime.Empty;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var action = GetAction(declaration);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();
			var today = ZDate.Today;
			AssertContains("PG22             FWIFW3 Y" + today.ToString("MMddyyyy"), message.EM_FormattedMessageText); //11212016

			invoiceLine.InvoiceHeader.US_FWSSignDate = new ZDateTime(2016, 11, 29);
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			action = GetAction(declaration);
			builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			message = builder.PopulateMessage();
			AssertContains("PG22             FWIFW3 Y11292016", message.EM_FormattedMessageText);
		}

		protected override void SetUpData()
		{
			base.SetUpData();

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
			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;

			header = invoiceLine.FWSHeaders.AddNew();
		}

		ZString GenerateMessageForProcessingCode(ZString processingCode)
		{
			header.US_ProcessingCode = processingCode;

			var action = GetAction(declaration);
			action.US_DateOfDeclaration = new ZDateTime(2015, 8, 20);
			var builder = new ACEEntrySummaryMessageBuilder(declaration.ActiveEntryHeaders.EntrySummaryEntry, action, UpdateActionCode.Add);
			var message = builder.PopulateMessage();

			return message.EM_FormattedMessageText;
		}

		OrgAddress exporterAddress;
		OrgAddress ExporterAddress
		{
			get
			{
				if (exporterAddress == null)
				{
					var org = Factory.New<OrgHeader>();
					org.OH_FullName = "FWS EXPORTER";
					org.OH_RL_NKClosestPort = "AUSYD";
					exporterAddress = org.MainAddress;
					exporterAddress.OA_Address1 = "EXP ADDRESS 1";
					exporterAddress.OA_Address2 = "EXP ADDRESS 2";
					exporterAddress.OA_RL_NKRelatedPortCode = "AUSYD";
					exporterAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, "DUNS87433", Core.Constants.CountryCodes.UnitedStates);
				}
				return exporterAddress;
			}
		}

		OrgAddress importerAddress;
		OrgAddress ImporterAddress
		{
			get
			{
				if (importerAddress == null)
				{
					var org = Factory.New<OrgHeader>();
					org.OH_FullName = "FWS IMPORTER";
					org.OH_RL_NKClosestPort = "USCHI";
					var contact = org.Contacts.AddNew();
					contact.OC_Email = "KNZ@ABC.COM";
					contact.OC_ContactName = "BOB";
					contact.OC_Phone = "00123456";
					var alloc = contact.Allocations.AddNew();
					alloc.PC_Type = OrgConstants.ContactAllocationType.USPGA;
					importerAddress = org.MainAddress;
					importerAddress.OA_Address1 = "IMP ADDRESS 1";
					importerAddress.OA_Address2 = "IMP ADDRESS 2";
					importerAddress.OA_RL_NKRelatedPortCode = "USCHI";
					importerAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.USACodeTypes.FWSeDecsAccountNumber, "FWE132123", Core.Constants.CountryCodes.UnitedStates);
				}
				return importerAddress;
			}
		}

		FWSHeader header;
	}
}
