using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AES.Testing
{
	sealed class AESPGALineGeneratorTest : TestCaseWithFactory
	{
		public void TestGeneratePGALines()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.US_EntryFilerCode = "XJ5";

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			invoiceLine.US_EPAConsentNumber = "288127374";
			invoiceLine.US_HazWasteTrackingNo = "123456789ABC";
			invoiceLine.US_EPANetQty = 123m;
			invoiceLine.US_EPANetQtyUQ = "KG";
			invoiceLine.US_ExportCertificateNo = "ABC1234567";
			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;

			var atfLine = invoiceLine.ATFLines.AddNew();
			atfLine.US_Quantity = 5m;
			atfLine.US_CategoryCode = ATFCategoryCodeList.Codes.AW;
			atfLine.US_FFLNumber = "SA444";
			atfLine.US_FFLExemptionCode = ExemptionCodesCodeList.Codes._2;
			atfLine.US_PermitNumber = "TT";
			atfLine.US_PermitExemptionCode = ExemptionCodesCodeList.Codes._2;
			invoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;

			invoiceLine.US_FWSInd = OGAIndicatorList.Codes.Declared;
			invoiceLine.JI_Description = "CLEAR DESCRIPTION OF FWS";
			invoiceLine.ExportFWS.US_ConfirmationNum = "AAAAAAAAAA";
			invoiceLine.ExportFWS.US_TaxonomicSerialNumber = "BBBBBBBB";
			invoiceLine.ExportFWS.US_PurposeCode = "M";
			invoiceLine.ExportFWS.US_WildlifeDescriptionCode = "W";
			invoiceLine.ExportFWS.US_SpeciesOrigin = "US";
			invoiceLine.ExportFWS.US_WildlifeSource = "DOM";
			invoiceLine.ExportFWS.US_CertificationCode = "B";
			invoiceLine.ExportFWS.US_WildlifeCategoryCode = "D";
			invoiceLine.ExportFWS.US_USState = "CA";

			var nmfsLine = invoiceLine.NMFSLines.AddNew();
			nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			nmfsLine.US_ProcessingType = NMFSProductCategoryCodeList.Codes.Dressed;
			nmfsLine.US_Quantity = 1000m;
			nmfsLine.US_UnitOfMeasure = Core.Constants.Weight.Tonnes;
			nmfsLine.US_VesselCountry = Core.Constants.CountryCodes.Australia;
			nmfsLine.US_HarvestedCountry = "ZZ";
			nmfsLine.US_GeographicLocation = "A";
			nmfsLine.US_DocumentType = NMFSHMSDocumentIdentifierList.Codes.BluefinTunaCatchDocument;
			nmfsLine.US_IFTPPermitNumber = "SE52103";
			nmfsLine.US_DISDocumentID = "DIS23423";
			nmfsLine.US_CatchDocument = "AAAAAA";
			nmfsLine.US_ReExportNumber = "BBBB";
			invoiceLine.US_NMFSHMSInd = OGAIndicatorList.Codes.Declared;

			var exportDEA = invoiceLine.DEAHeaders.AddNew();
			exportDEA.US_DrugCode = "87EF";
			exportDEA.US_Weight = 1000.25m;
			exportDEA.US_UnitOfMeasure = Core.Constants.Weight.Kilograms;
			exportDEA.US_PermitNumber = "AB12937";
			exportDEA.US_RegistrationNumber = "83741FL34";
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;

			var ttbLine = invoiceLine.TTBLines.AddNew();
			ttbLine.US_NumberForIRC = "IRC NUM";
			ttbLine.US_Date = new ZDate(2016, 9, 22);
			ttbLine.US_SerialNumber = "1111";
			invoiceLine.US_TTBInd = OGAIndicatorList.Codes.Declared;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var entryLine = invoiceLine.CusEntryLine;

			var blocks = ExportPGABlocksCreator.BuildPGABlocks(entryLine);
			var messageBuilder = AESPGALineGenerator.GeneratePGALines(blocks);
			var result = new ZStringBuilder(messageBuilder).ToStringWithNewLineBetweenAppends();

			var expectedResult =
@"PGA Code: AMS
Export Certificate Number: ABC1234567

PGA Code: EPA
EPA Consent Number: 288127374  Hazardous Waste Manifest Tracking No.: 123456789ABC  EPA Net Quantity: 123 KG

PGA Code: ATF
Quantity: 5  Category Code: AW  FFL Number: SA444  FFL Exemption Code: 2  Permit Number: TT  Permit Exemption Code: 2

PGA Code: DEA
Drug Code: 87EF  Weight: 1000.25  Weight UQ: KG  Permit Number: AB12937  Registration Number: 83741FL34

PGA Code: FWS
eDecs Confirmation: AAAAAAAAAA  Taxonomic Serial Number: BBBBBBBB  Purpose Code: M  Wildlife Description Code: W  Wildlife Source: J  Wildlife Category Code: D  Certification Code: B  Species Origin: US  State: CA

PGA Code: TTB
Number for IRC: IRC NUM  Departure Date: 22-Sep-16  Serial Number: 1111

PGA Code: NMFS
Goods Description: CLEAR DESCRIPTION OF FWS  Program Type: HMS  Category Code: NDR  Document Type: 883  eBCD Number: DIS23423  Harvested Country: ZZ  Vessel Country: AU  IFTP Permit Number: SE52103  Total Weight: 1000000KG  Catch Document Number: AAAAAA  Re-Export Approval No.: BBBB
";
			AssertEquals(expectedResult, result);
		}

		public void TestGeneratePGALinesFromMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var outgoingMessage = Factory.NewWithValidTestData<AESTIREDIMessage>();
			outgoingMessage.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			outgoingMessage.EM_LinkUniqueID = entry.PK;
			outgoingMessage.EM_MessageType = ApplicationIdentifierCodeList.AES.CommodityShipment;
			outgoingMessage.EM_MessageText = "B                        SHIPPER DEFAULT - DO NOT USE!                          SC1N41AU      B0016941202      A                       2      1101         N    SC270                                   N                                       N01            ESHIPPER DEFAULT - DO NOT USE!                                   N02DEFAULT                                                                      N03TOKYO                      JP111111                                          N01            CCEVA IMPORTER 673636                                           NN021                                                                            N03CHICAGO                  ILUS                                                CL1OS 0001                                             0000000000 AC33FY        CL2             00000000000000000000   00000000000000000000     NLR             PGAAM110120                                                                     PGAEP1YDSAF        SDF         KG 0000000010                                    PGANM7                                                                      AMR PGANM8                                    N                                     PGANM9                                                                          PGANM7                                                                      HMS PGANM8                                    N                                     PGANM9                                                                          PGAAT6102120              1120120     1000010120AW                              PGADEA102100000120100000G  E                                                    PGADEA212 00000005550000KG E                                                    PGAFW71021021       1010120            MBALADWFW1AMP                            PGAFW8                                                                          PGATTB1012012        20180418                                                   CL1OS 0002HORSES, PUREBRED BREEDING, LIVE              0000000000 AC33FY        CL20101210000NO 00000000000000000000   00000000000000000000     NLR             PGAAM1254254                                                                    PGAEP1Y254254      254254      KG 0000254254                                    PGANM7HORSES, PUREBRED BREEDING, LIVE                                       AMR PGANM8NDR888                              Y                                     PGANM9                                                                          PGAAT6                                 000000000    HORSES, PUREBRED BREEDING,  PGADEA445200000042540000G  E425                                                 PGAFW7544254        254254254          MBALADWFW1AMP                            PGAFW8HORSES, PUREBRED BREEDING, LIVE                                           PGATTB254254254      20180418                                                   Y                        SHIPPER DEFAULT - DO NOT USE!";
			var aesPrintForAESTir = new AESMessagePrint(outgoingMessage);
			AssertEquals(2, aesPrintForAESTir.Commodities.Count);

			var printLineObject = aesPrintForAESTir.Commodities[0] as AESTIRMessageCommodityLine;
			var messageBuilder = AESPGALineGenerator.GeneratePGALines(printLineObject.PGABlock);
			var result = new ZStringBuilder(messageBuilder).ToStringWithNewLineBetweenAppends();

			var expectResult =
@"PGA Code: AMS
Export Certificate Number: 10120

PGA Code: EPA
EPA Consent Number: DSAF  Hazardous Waste Manifest Tracking No.: SDF  EPA Net Quantity: 10 KG

PGA Code: NMFS
Program Type: AMR

PGA Code: ATF
Quantity: 10120  Category Code: AW  FFL Number: 102120  FFL Exemption Code: 1  Permit Number: 120120  Permit Exemption Code: 1

PGA Code: DEA
Drug Code: 1021  Weight: 12010  Weight UQ: G

PGA Code: DEA
Drug Code: 212  Weight: 555  Weight UQ: KG

PGA Code: FWS
eDecs Confirmation: 1021021  Taxonomic Serial Number: 1010120  Purpose Code: M  Wildlife Description Code: BAL  Wildlife Source: W  Wildlife Category Code: AMP  Certification Code: FW1  Species Origin: AD

PGA Code: TTB
Number for IRC: 1012012  Departure Date: 18-Apr-18

PGA Code: NMFS
Program Type: HMS
";

			AssertEquals(expectResult, result);

			printLineObject = aesPrintForAESTir.Commodities[1] as AESTIRMessageCommodityLine;
			messageBuilder = AESPGALineGenerator.GeneratePGALines(printLineObject.PGABlock);
			result = new ZStringBuilder(messageBuilder).ToStringWithNewLineBetweenAppends();

			expectResult =
@"PGA Code: AMS
Export Certificate Number: 254254

PGA Code: EPA
EPA Consent Number: 254254  Hazardous Waste Manifest Tracking No.: 254254  EPA Net Quantity: 254254 KG


PGA Code: DEA
Drug Code: 4452  Weight: 4254  Weight UQ: G  Permit Number: 425

PGA Code: FWS
eDecs Confirmation: 544254  Taxonomic Serial Number: 254254254  Purpose Code: M  Wildlife Description Code: BAL  Wildlife Source: W  Wildlife Category Code: AMP  Certification Code: FW1  Species Origin: AD

PGA Code: TTB
Number for IRC: 254254254  Departure Date: 18-Apr-18

PGA Code: NMFS
Goods Description: HORSES, PUREBRED BREEDING, LIVE  Program Type: AMR  Category Code: NDR  Document Type: 888
";
			AssertEquals(expectResult, result);
		}
	}
}
