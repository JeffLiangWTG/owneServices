using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging.Testing;
using Enterprise.Edifact;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.Business.CustomsMessaging.D09B.Testing
{
	public class Tcodec09bTest : TestCaseWithFactory
	{
		public void TestMessageConstructor()
		{
			var tcodec09bMessage = new Tcodec09b(CofODataProvider);
			AssertNotNull(tcodec09bMessage);
		}

		public void TestMessageSubType()
		{
			AssertEquals(CUSDECEDIMessage.Declaration, MessageBuilder.MessageSubType);
		}

		public void TestGenerateUNH()
		{
			var msg = MessageBuilder.EdifactMessage;
			AssertEquals("UNH+1+TCODEC:0:1:RT:041+COODEC'", msg.UNH.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateBGM()
		{
			var msg = MessageBuilder.EdifactMessage;
			AssertEquals("BGM+860:::COO+<<MSGNO PLACEHOLDER>>+9'", msg.BGM.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateCST()
		{
			CofODataProvider.ApplicationProductType = ApplicationProductTypeCodeList.Codes.NA;
			var msg = MessageBuilder.EdifactMessage;
			AssertEquals("CST++NA'", msg.CST.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateGEI()
		{
			var msg = MessageBuilder.EdifactMessage;
			AssertEquals("GEI+5+:Y'", msg.Group2[0].GEI.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateSG1()
		{
			var msg = MessageBuilder.EdifactMessage;
			AssertEquals("Base should generate the Message Sender Segment Group 1", 1, msg.Group1.Count);
			AssertEquals("RFF+MS:.'", msg.Group1[0].RFF.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateNonAirSeaTDTSegment()
		{
			CofODataProvider.OutwardTransportCode = 3;
			var msg = MessageBuilder.EdifactMessage;
			AssertEquals("TDT+20++3'", msg.Group2[0].TDT.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateSG3()
		{
			AgentInfoTestClass testDeclarant = new AgentInfoTestClass();
			testDeclarant.Name = "Test SG Broker";
			testDeclarant.Phone = "64 85721111";
			testDeclarant.Code = "v13t001";
			CofODataProvider.Declarant = testDeclarant;
			var msg = MessageBuilder.EdifactMessage;
			AssertEquals("CO message should generate the Declaring Agent Segment & Declarant Segments (Group 3)", 2, msg.Group3.Count);
			AssertEquals("NAD+AE+AAA374M++EAGLE DATAMATION INTERNATIONAL:::::1'", msg.Group3[0].NAD.ToString(new UNOASGCharacterSet()));
			var declarantSG3 = msg.Group3[1];
			AssertEquals("Declarant NAD", 1, declarantSG3.NAD.Count);
			AssertEquals("Declarant CTA", 1, declarantSG3.CTA.Count);
			AssertEquals("Declarant COM", 1, declarantSG3.COM.Count);
			AssertEquals("NAD+DT'", declarantSG3.NAD.ToString(new UNOASGCharacterSet()));
			AssertEquals("CTA+IC+V13T001:TEST SG BROKER'", declarantSG3.CTA.ToString(new UNOASGCharacterSet()));
			AssertEquals("COM+64 85721111:TE'", declarantSG3.COM.ToString(new UNOASGCharacterSet()));
		}

		public void TestCertificateItemFTXSegments()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].CustomsValue = 25000m;
			items[0].ItemDescription = "15 Packages";
			CofODataProvider.CertItems = items;
			var msg = MessageBuilder.EdifactMessage;
			AssertEquals("FTX+AAZ+++15 PACKAGES'", msg.Group5[0].FTX.ToString(new UNOASGCharacterSet()));
		}

		public void TestCertificateItemWhereCarriageReturnUsed()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].CustomsValue = 25000m;
			items[0].ItemDescription = @"(505 CTNS) CONTAINING 8059 CPS OF
PRINTED BOOKS
""CIF LE HAVRE""
01 TITLE: WHAT KNOT? (EQUMWHKT) - 1ST FEBRUARY 2008
02 TITLE: WHAT KNOT? (EQUMWHKT) - 1ST FEBRUARY 2008
(LOGISTA FRANCE)
03 TITLE: WHAT KNOT? (EQUMWHKT) - 1ST FEBRUARY 2008
(HACHETTE-LIVRE)
DELIVERY ADDRESS:
___________
CEPL SOLADIS
RUE DE COURCY
27380 FLEURY LES ANDELLES
FRANCE
CONTACT:M.LEVRAULT
TEL: +33(0) 1 30 662288
EMAIL:RDVRECEPTION@HACHETTE.FR
NOTIFY PARTY:
GEODIS OVERSEAS
9 RUE FERRER
BP 1345, 76065 LE HAVRE CEDEX
FRANCE CONTACT:ROZEN GODEY
TEL: +33(0)235 538929
FAX: +33(0)235 538925
CONSIGNEE EMAIL: PCOULAND@HACHETTE-LIVRE.FR";
			CofODataProvider.CertItems = items;
			var msg = MessageBuilder.EdifactMessage;
			AssertEquals("FTX+AAZ+++(505 CTNS) CONTAINING 8059 CPS OF:PRINTED BOOKS:\"CIF LE HAVRE\":01 TITLE?: WHAT KNOT?? (EQUMWHKT) -:1ST FEBRUARY 2008'FTX+AAZ+++02 TITLE?: WHAT KNOT?? (EQUMWHKT) -:1ST FEBRUARY 2008:(LOGISTA FRANCE):03 TITLE?: WHAT KNOT?? (EQUMWHKT) -:1ST FEBRUARY 2008'FTX+AAZ+++(HACHETTE-LIVRE):DELIVERY ADDRESS?::___________:CEPL SOLADIS:RUE DE COURCY'FTX+AAZ+++27380 FLEURY LES ANDELLES:FRANCE:CONTACT?:M.LEVRAULT:TEL?: ?+33(0) 1 30 662288:EMAIL?:RDVRECEPTION@HACHETTE.FR'FTX+AAZ+++NOTIFY PARTY?::GEODIS OVERSEAS:9 RUE FERRER:BP 1345, 76065 LE HAVRE CEDEX:FRANCE CONTACT?:ROZEN GODEY'FTX+AAZ+++TEL?: ?+33(0)235 538929:FAX?: ?+33(0)235 538925:CONSIGNEE EMAIL?::PCOULAND@HACHETTE-LIVRE.FR'", msg.Group5[0].FTX.ToString(new UNOASGCharacterSet()));
		}

		public void TestItemDescRevertsToSimpleSplittingIfCannotBeFormatted()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].CustomsValue = 25000m;
			items[0].ItemDescription = @"PURE BRED BREEDING HORSES ASSES MULES & HINNIES (NMB)
ADD SOME FOMATTING
ADD SOME MORE TEXT

=====================

AND DO SOME MORE STUFF AND DO SOME MORE STUFF AND DO SOME MORE STUFF

AND MORE

AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND
AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND
AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REREST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE UP THE REST OF THE CHARS AND THEN USE THIS IS THE LAST PIECE OF TEXT";
			CofODataProvider.CertItems = items;
			var msg = MessageBuilder.EdifactMessage;
			AssertEquals("FTX+AAZ+++PURE BRED BREEDING HORSES ASSES MUL:ES & HINNIES (NMB) ADD SOME FOMATTI:NG ADD SOME MORE TEXT  ============:=========  AND DO SOME MORE STUFF A:ND DO SOME MORE STUFF AND DO SOME M'FTX+AAZ+++ORE STUFF  AND MORE  AND THEN USE U:P THE REST OF THE CHARS AND THEN US:E UP THE REST OF THE CHARS AND AND :THEN USE UP THE REST OF THE CHARS A:ND THEN USE UP THE REST OF THE CHAR'FTX+AAZ+++S AND AAAAAAAAAAAAAAAAAAAAAAAAAAAAA:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA:AA AND THEN USE UP THE REST OF THE :CHARS AND THEN USE UP THE REST OF T'FTX+AAZ+++HE CHARS AND THEN USE UP THE REST O:F THE CHARS AND THEN USE UP THE RES:T OF THE CHARS AND THEN USE UP THE :REST OF THE CHARS AND THEN USE UP T:HE REST OF THE CHARS AND THEN USE U'FTX+AAZ+++P THE REST OF THE CHARS AND THEN US:E UP THE REST OF THE CHARS AND THEN: USE UP THE REST OF THE CHARS AND T:HEN USE UP THE REST OF THE CHARS AN:D THEN USE UP THE REST OF THE CHARS'FTX+AAZ+++ AND THEN USE UP THE REST OF THE CH:ARS AND THEN USE UP THE REST OF THE: CHARS AND THEN USE UP THE REREST O:F THE CHARS AND THEN USE UP THE RES:T OF THE CHARS AND THEN USE UP THE 'FTX+AAZ+++REST OF THE CHARS AND THEN USE UP T:HE REST OF THE CHARS AND THEN USE U:P THE REST OF THE CHARS AND THEN US:E UP THE REST OF THE CHARS AND THEN: USE UP THE REST OF THE CHARS AND T'FTX+AAZ+++HEN USE UP THE REST OF THE CHARS AN:D THEN USE UP THE REST OF THE CHARS: AND THEN USE UP THE REST OF THE CH:ARS AND THEN USE UP THE REST OF THE: CHARS AND THEN USE UP THE REST OF 'FTX+AAZ+++THE CHARS AND THEN USE UP THE REST :OF THE CHARS AND THEN USE UP THE RE:ST OF THE CHARS AND THEN USE UP THE: REST OF THE CHARS AND THEN USE UP :THE REST OF THE CHARS AND THEN USE 'FTX+AAZ+++THIS IS THE LAST PIECE OF TEXT'", msg.Group5[0].FTX.ToString(new UNOASGCharacterSet()));
		}

		public void TestItemDescRevertsToNonFormattedToFitAllData()
		{
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].CustomsValue = 25000m;
			items[0].ItemDescription = @"THIS SPECIFICATION PROVIDES THE DEFINITION OF THE TRADE DECLARATION AND APPLICATION OF CERTIFICATE OF ORIGIN IE. TCODEC
(COODEC) MESSAGE TO BE USED IN ELECTRONIC DATA INTERCHANGE (EDI) BETWEEN TRADING PARTNERS INVOLVED IN ADMINISTRATION, COMMERCE
AND TRANSPORT.
TCODEC IS A LOCAL MESSAGE, PENDING SUBMISSION TO UN FOR APPROVAL AS A NEW UN STANDARD MESSAGE (UNSM).
THIS DOCUMENT SHALL BE USED AS A BASELINE FOR THE INTERFACE SOFTWARE DESIGN AND SHALL BE AGREED UPON BY REPRESENTATIVE FROM IE
SINGAPORE AND CRIMSONLOGIC PTE LTD.

THE DOCUMENT SHALL PROVIDE THE FOLLOWING PRINCIPLES AND DETAILS FOR THE FOLLOWING:
- CRITERIA FOR PASSING INFORMATION FROM TRADERS, FREIGHT FORWARDERS, CARGO AGENTS AND SHIPPING AGENTS TO IESGP, CA OR CHAMBERS.
- MESSAGE FORMAT.
THE SEGMENTS, COMPOSITE DATA ELEMENTS, DATA ELEMENTS AND CODES USED IN THIS DOCUMENT ARE BASED ON THE RESPECTIVE DIRECTORIES IN
THE UN/EDIFACT D.05B DIRECTORY. REFER TO REFERENCES 5 TO 16 FOR MORE DETAILS.

THE MESSAGE SPECIFICATION PROVIDED IN THIS DOCUMENT ARE INTENDED FOR USE FOR THE EXCHANGE OF INFORMATION BETWEEN THE TRADING
PARTNERS IN INTERNATIONAL TRADE. THIS MESSAGE MAY BE APPLIED FOR BOTH NATIONAL AND INTERNATIONAL TRADE. IT IS BASED ON
UNIVERSAL PRACTICE AND IS NOT DEPENDENT ON THE TYPE OF BUSINESS OR INDUSTRY.

THE MESSAGE SPECIFICATION PROVIDED IN THIS DOCUMENT ARE INTENDED FOR USE FOR THE EXCHANGE OF INFORMATION BETWEEN THE TRADING
PARTNERS IN INTERNATIONAL TRADE. THIS MESSAGE MAY BE APPLIED FOR BOTH NATIONAL AND INTERNATIONAL TRADE. IT IS BASED ON
UNIVERSAL PRACTICE AND IS NOT DEPENDENT ON THE TYPE OF BUSINESS OR INDUSTRY.THIS MESSAGE INCORPORATES THE NECESSARY TRADE, TRANSPORT, STATISTICAL AND APPLICANT/DECLARANT INFORMATION ON THE APPLICATION.";
			CofODataProvider.CertItems = items;
			var msg = MessageBuilder.EdifactMessage;
			AssertEquals("FTX+AAZ+++THIS SPECIFICATION PROVIDES THE DEF:INITION OF THE TRADE DECLARATION AN:D APPLICATION OF CERTIFICATE OF ORI:GIN IE. TCODEC (COODEC) MESSAGE TO :BE USED IN ELECTRONIC DATA INTERCHA'FTX+AAZ+++NGE (EDI) BETWEEN TRADING PARTNERS :INVOLVED IN ADMINISTRATION, COMMERC:E AND TRANSPORT. TCODEC IS A LOCAL :MESSAGE, PENDING SUBMISSION TO UN F:OR APPROVAL AS A NEW UN STANDARD ME'FTX+AAZ+++SSAGE (UNSM). THIS DOCUMENT SHALL B:E USED AS A BASELINE FOR THE INTERF:ACE SOFTWARE DESIGN AND SHALL BE AG:REED UPON BY REPRESENTATIVE FROM IE: SINGAPORE AND CRIMSONLOGIC PTE LTD'FTX+AAZ+++.  THE DOCUMENT SHALL PROVIDE THE F:OLLOWING PRINCIPLES AND DETAILS FOR: THE FOLLOWING?: - CRITERIA FOR PASS:ING INFORMATION FROM TRADERS, FREIG:HT FORWARDERS, CARGO AGENTS AND SHI'FTX+AAZ+++PPING AGENTS TO IESGP, CA OR CHAMBE:RS. - MESSAGE FORMAT. THE SEGMENTS,: COMPOSITE DATA ELEMENTS, DATA ELEM:ENTS AND CODES USED IN THIS DOCUMEN:T ARE BASED ON THE RESPECTIVE DIREC'FTX+AAZ+++TORIES IN THE UN/EDIFACT D.05B DIRE:CTORY. REFER TO REFERENCES 5 TO 16 :FOR MORE DETAILS.  THE MESSAGE SPEC:IFICATION PROVIDED IN THIS DOCUMENT: ARE INTENDED FOR USE FOR THE EXCHA'FTX+AAZ+++NGE OF INFORMATION BETWEEN THE TRAD:ING PARTNERS IN INTERNATIONAL TRADE:. THIS MESSAGE MAY BE APPLIED FOR B:OTH NATIONAL AND INTERNATIONAL TRAD:E. IT IS BASED ON UNIVERSAL PRACTIC'FTX+AAZ+++E AND IS NOT DEPENDENT ON THE TYPE :OF BUSINESS OR INDUSTRY.  THE MESSA:GE SPECIFICATION PROVIDED IN THIS D:OCUMENT ARE INTENDED FOR USE FOR TH:E EXCHANGE OF INFORMATION BETWEEN T'FTX+AAZ+++HE TRADING PARTNERS IN INTERNATIONA:L TRADE. THIS MESSAGE MAY BE APPLIE:D FOR BOTH NATIONAL AND INTERNATION:AL TRADE. IT IS BASED ON UNIVERSAL :PRACTICE AND IS NOT DEPENDENT ON TH'FTX+AAZ+++E TYPE OF BUSINESS OR INDUSTRY.THIS: MESSAGE INCORPORATES THE NECESSARY: TRADE, TRANSPORT, STATISTICAL AND :APPLICANT/DECLARANT INFORMATION ON :THE APPLICATION.'", msg.Group5[0].FTX.ToString(new UNOASGCharacterSet()));
		}

		public void TestDeclaringAgentSegment()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			var msg = MessageBuilder.EdifactMessage;
			AssertEquals("NAD+AE+AAA374M++EAGLE DATAMATION INTERNATIONAL:::::1'", msg.Group3[0].NAD.ToString(new UNOASGCharacterSet()));
		}

		public void TestExporterName()
		{
			OrganisationTestClass exporter = new OrganisationTestClass();
			exporter.UEN = "200102977K";
			exporter.Name = "HYDRO ALUMINIUM MALAYSIA SDN BHD C/O OIA GLOBAL LOGISTICS (S) PTE LTD";
			CofODataProvider.Exporter = exporter;
			var msg = MessageBuilder.EdifactMessage;
			AssertEquals("NAD+EX+200102977K++HYDRO ALUMINIUM MALAYSIA SDN BHD C/:O OIA GLOBAL LOGISTICS (S) PTE LTD::::2'", msg.Group3[2].NAD.ToString(new UNOASGCharacterSet()));
		}

		public void TestDetailsSectionUNS()
		{
			var msg = MessageBuilder.EdifactMessage;
			AssertEquals("UNS+D'", msg.UNS1.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateMessageUNS()
		{
			var msg = MessageBuilder.EdifactMessage;
			AssertEquals("UNS+S'", msg.UNS2.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateMessageCNT()
		{
			var msg = MessageBuilder.EdifactMessage;
			AssertEquals("CNT+5:0'", msg.CNT.ToString(new UNOASGCharacterSet()));
		}

		public void TestGenerateMessageUNT()
		{
			var msg = MessageBuilder.EdifactMessage;
			AssertEquals("UNT+16+1'", msg.UNT.ToString(new UNOASGCharacterSet()));
		}

		public void TestMessageText()
		{
			var msg = MessageBuilder.EdifactMessage;
			AssertEquals(msg.ToString(new UNOASGCharacterSet()), MessageBuilder.MessageText);
		}

		public void TestMessageSegmentCount()
		{
			var msg = MessageBuilder.EdifactMessage;
			AssertEquals(msg.CountIncludingUNT, MessageBuilder.MessageSegmentCount);
		}

		public void TestAllSegmentsGenerated()
		{
			SGCUSDECTestClass.SGCUSDECTestData testData = new SGCUSDECTestClass.SGCUSDECTestData();
			SetCOTestData(CofODataProvider);
			CofODataProvider.CertificateType1 = "1";
			CofODataProvider.ApplicationProductType = ApplicationProductTypeCodeList.Codes.TX;
			CofODataProvider.CurrencyCode = "USD";
			CofODataProvider.AdditionalInformation = "MALAYSIA FORM D NO.KL2007/2/2155 DATED 10/01/2007";
			CofODataProvider.AdditionalDetails1 = "Certificate Additional Details";
			CofODataProvider.NumberOfCopies1 = 3;
			CofODataProvider.PercCommContent1 = 15;
			CofODataProvider.DonorCountryCode = "CA";
			CofODataProvider.YearOfEntry = 2007;
			var msg = MessageBuilder.EdifactMessage;
			var messageTextResult = msg.ToString(new UNOASGCharacterSet());
			Assert("Message Header", messageTextResult.Contains("UNH+1+TCODEC:0:1:RT:041+COODEC'"));
			Assert(messageTextResult.Contains("BGM+860:::COO+"));
			Assert("Product Type", messageTextResult.Contains("CST++TX'"));
			Assert("Mesasge Sender", messageTextResult.Contains("RFF+MS:V13T.V13T001'"));
			Assert("Additional Info", messageTextResult.Contains("FTX+DCL+++MALAYSIA FORM D NO.KL2007/2/2155:DATED 10/01/2007'"));
			Assert("Previous Permit No.", messageTextResult.Contains("RFF+ACE:BA97985267'"));
			Assert("Transport Segment", messageTextResult.Contains("TDT+20+18W+1+++++:::PACIFIC STAR'"));
			Assert("Departure Date", messageTextResult.Contains("DTM+136:20070116:102'"));
			Assert("Entry Year", messageTextResult.Contains("DTM+247:2007:602'"));
			Assert("Broker declaration segment", messageTextResult.Contains("GEI+5+:Y"));
			Assert("Port of Discharge", messageTextResult.Contains("LOC+11+USLAX'"));
			Assert("Destination Country", messageTextResult.Contains("LOC+36+US'"));
			Assert("Donor Country", messageTextResult.Contains("LOC+166+CA'"));
			Assert("Declaring Agent", messageTextResult.Contains("NAD+AE+AAA374M++EAGLE DATAMATION INTERNATIONAL:::::1'"));
			Assert("Declarant Segments", messageTextResult.Contains("NAD+DT'CTA+IC+V13T001:CHOW LEW WEE'COM+?+64 9760 2847:TE'"));
			Assert("Consignee", messageTextResult.Contains("NAD+CN+++JOHN SMITH:::::2+"));
			Assert("Exporter", messageTextResult.Contains("NAD+EX+12093857483R++SINGAPORE EXPORTERS PTE LTD:::::2+"));
			Assert("Carrier Agent", messageTextResult.Contains("NAD+CA+16003857483R++SINGAPORE PACIFIC LINE:::::1'"));
			Assert("Detail seperator", messageTextResult.Contains("UNS+D'"));
			Assert(messageTextResult.Contains("DMS++860'"));
			Assert("Certificate Type & No. of Copies", messageTextResult.Contains("DOC+860:::1+1++3'"));
			Assert("Cmwlth Pref. Content", messageTextResult.Contains("PCD+6:15'"));
			Assert("Reference Currency", messageTextResult.Contains("CUX+2:USD'"));
			Assert("Additional Export Info", messageTextResult.Contains("FTX+AAZ+++CERTIFICATE ADDITIONAL DETAILS'"));
			AssertEquals("Should be 3 Item/Line details (SG5)", 3, msg.Group5.Count);
			AssertMultilineEquals("Line 1 segments", "LIN++4'CST+1+48063000'MEA+AAF++:144.0000'MEA+AAX++TNE:144.0000'PCI+28+MARKS AND NUMBERS:FOR CERTIFICATE:OF ORIGIN - LINE:0'MOA+63:25000.00'MOA+66:750.00'LOC+27+SG'FTX+AAZ+++TRACING PAPERS'RFF+IV:INV01031'DTM+3:20061220:102'CST++480630'PCD+6:35'FTX+ABS+++SINGLE:COUNTRY:CONTENT'GIN+AT+15'", msg.Group5[0].ToString(new UNOASGCharacterSet()), '\'');
			AssertMultilineEquals("Line 2 segments", "LIN++4'CST+2+48083000'MEA+AAF++:250.0000'MEA+AAX++TNE:250.0000'PCI+28+MARKS AND NUMBERS:FOR CERTIFICATE:OF ORIGIN - LINE:1'MOA+63:25000.00'MOA+66:1095.00'LOC+27+SG'FTX+AAZ+++OTHER KRAFT PAPER CREPED OR:CRINKLED'RFF+IV:INV01031'DTM+3:20061220:102'CST++480830'PCD+6:35'FTX+ABS+++SINGLE:COUNTRY:CONTENT'GIN+AT+15'", msg.Group5[1].ToString(new UNOASGCharacterSet()), '\'');
			AssertMultilineEquals("Line 3 segments", "LIN++4'CST+3+48091010'MEA+AAF++:500.0000'MEA+AAX++TNE:500.0000'PCI+28+MARKS AND NUMBERS:FOR CERTIFICATE:OF ORIGIN - LINE:2'MOA+63:25000.00'MOA+66:1575.00'LOC+27+SG'FTX+AAZ+++CARBON PAPER IN ROLLS OR SHEETS'RFF+IV:INV01031'DTM+3:20061220:102'CST++480910'PCD+6:35'FTX+ABS+++SINGLE:COUNTRY:CONTENT'GIN+AT+15'", msg.Group5[2].ToString(new UNOASGCharacterSet()), '\'');
			Assert("Summary Segment seperator", messageTextResult.Contains("UNS+S'"));
			Assert("Control Total", messageTextResult.Contains("CNT+5:3'"));
			AssertEquals("Message Trailer - segment count", 74, msg.CountIncludingUNT);
			Assert("Message Trailer", messageTextResult.Contains("UNT+74+1'"));
		}

		public void TestWith2Certificates()
		{
			SGCUSDECTestClass.SGCUSDECTestData testData = new SGCUSDECTestClass.SGCUSDECTestData();
			SetCOTestData(CofODataProvider);
			CofODataProvider.CertificateType1 = "1";
			CofODataProvider.CertificateType2 = "16";
			CofODataProvider.ApplicationProductType = ApplicationProductTypeCodeList.Codes.NH;
			CofODataProvider.CurrencyCode = "USD";
			CofODataProvider.AdditionalInformation = "MALAYSIA FORM D NO.KL2007/2/2155 DATED 10/01/2007";
			CofODataProvider.AdditionalDetails1 = "Certificate Additional Details - first certificate";
			CofODataProvider.AdditionalDetails2 = "Additional Details - second certificate";
			CofODataProvider.TransportDetails1 = "Certificate Transport Details - first certificate";
			CofODataProvider.NumberOfCopies1 = 3;
			CofODataProvider.NumberOfCopies2 = 0;
			CofODataProvider.PercCommContent1 = 15;
			CofODataProvider.DonorCountryCode = "CA";
			CofODataProvider.YearOfEntry = 2007;
			var msg = MessageBuilder.EdifactMessage;
			var messageTextResult = msg.ToString(new UNOASGCharacterSet());
			Assert("Message Header", messageTextResult.Contains("UNH+1+TCODEC:0:1:RT:041+COODEC'"));
			Assert("Product Type", messageTextResult.Contains("CST++NH'"));
			Assert("Broker declaration segment", messageTextResult.Contains("GEI+5+:Y"));
			Assert("Port of Discharge", messageTextResult.Contains("LOC+11+USLAX'"));
			Assert("Destination Country", messageTextResult.Contains("LOC+36+US'"));
			Assert("Donor Country", messageTextResult.Contains("LOC+166+CA'"));
			Assert("Declaring Agent", messageTextResult.Contains("NAD+AE+AAA374M++EAGLE DATAMATION INTERNATIONAL:::::1'"));
			Assert("Declarant Segments", messageTextResult.Contains("NAD+DT'CTA+IC+V13T001:CHOW LEW WEE'COM+?+64 9760 2847:TE'"));
			Assert("Consignee", messageTextResult.Contains("NAD+CN+++JOHN SMITH:::::2+1556 SMITH ST LOS ANGELES'"));
			Assert("Exporter", messageTextResult.Contains("NAD+EX+12093857483R++SINGAPORE EXPORTERS PTE LTD:::::2+GWANG ZO BUILDING C'"));
			Assert("Carrier Agent", messageTextResult.Contains("NAD+CA+16003857483R++SINGAPORE PACIFIC LINE:::::1'"));
			Assert("Detail seperator", messageTextResult.Contains("UNS+D'"));
			AssertEquals("Segment Group 4 count", 1, msg.Group4.Count);
			AssertEquals("Group4 DOC segment count", 2, msg.Group4[0].DOC.Count);
			AssertEquals("First Certificate DOC details", "DOC+860:::1+1++3'", msg.Group4[0].DOC[0].ToString(new UNOASGCharacterSet()));
			AssertEquals("Second Certificate DOC details", "DOC+860:::16+2'", msg.Group4[0].DOC[1].ToString(new UNOASGCharacterSet()));
			AssertEquals("Group4 PCD segment count", 1, msg.Group4[0].PCD.Count);
			AssertEquals("Certificate Cmwlth Pref. Content", "PCD+6:15'", msg.Group4[0].PCD.ToString(new UNOASGCharacterSet()));
			AssertEquals("Group4 CUX segment count", 1, msg.Group4[0].CUX.Count);
			AssertEquals("Certificate Reference Currency", "CUX+2:USD'", msg.Group4[0].CUX.ToString(new UNOASGCharacterSet()));
			AssertEquals("Group4 FTX segment count", 2, msg.Group4[0].FTX.Count);
			AssertEquals("First Additional Export Info", "FTX+AAZ+++CERTIFICATE ADDITIONAL DETAILS -:FIRST CERTIFICATE'", msg.Group4[0].FTX[0].ToString(new UNOASGCharacterSet()));
			AssertEquals("First Transport Details Info", "FTX+TDT+++CERTIFICATE TRANSPORT DETAILS -:FIRST CERTIFICATE'", msg.Group4[0].FTX[1].ToString(new UNOASGCharacterSet()));
		}

		public void TestCertificateSendSGD()
		{
			SGCUSDECTestClass.SGCUSDECTestData testData = new SGCUSDECTestClass.SGCUSDECTestData();
			SetCOTestData(CofODataProvider);
			CofODataProvider.CertificateType1 = "1";
			CofODataProvider.CurrencyCode = "SGD";
			ItemsTestClass[] items = new ItemsTestClass[1];
			items[0] = new ItemsTestClass();
			items[0].CustomsValue = 25000m;
			items[0].ItemDescription = "15 Packages";
			CofODataProvider.CertItems = items;
			var msg = MessageBuilder.EdifactMessage;
			AssertEquals("Group4 CUX segment count", 1, msg.Group4[0].CUX.Count);
			AssertEquals("Certificate Reference Currency", "CUX+2:SGD'", msg.Group4[0].CUX.ToString(new UNOASGCharacterSet()));
		}

		#region Implementation
		protected CusCertificateTestClass CofODataProvider
		{
			get
			{
				if (fCofODataProvider == null)
				{
					fCofODataProvider = new CusCertificateTestClass();
				}

				return fCofODataProvider;
			}
		}

		CusCertificateTestClass fCofODataProvider;
		Tcodec09b MessageBuilder
		{
			get
			{
				if (fMessageBuilder == null)
				{
					fMessageBuilder = new Tcodec09b(CofODataProvider);
				}

				return fMessageBuilder;
			}
		}

		Tcodec09b fMessageBuilder;

		void ResetMessageBuilder()
		{
			fMessageBuilder = null;
		}

		public SGCUSDECTestClass SetCOTestData(CusCertificateTestClass cofODataProvider)
		{
			cofODataProvider.DeclarantId = "V13T001";
			AgentInfoTestClass declarant = new AgentInfoTestClass();
			declarant.Name = "Chow Lew Wee";
			declarant.Code = "V13T001";
			declarant.Phone = "+64 9760 2847";
			cofODataProvider.Declarant = declarant;
			cofODataProvider.CargoPackingType = "1";
			cofODataProvider.PortOfLoading = "SGSIN";
			cofODataProvider.PortOfDischarge = "USLAX";
			cofODataProvider.OutwardMasterBill = "OBL00384758";
			cofODataProvider.OutwardTransportCode = SGConstants.TransportCodes.Sea;
			cofODataProvider.OutwardTransportIdentifier = "PACIFIC STAR";
			cofODataProvider.OutwardJourneyIdentifier = "18W";
			cofODataProvider.IsImport = false;
			cofODataProvider.IsSea = true;
			cofODataProvider.DepartureDate = new ZDate(2007, 01, 16);
			cofODataProvider.TotalGrossWeight = 85.75m;
			cofODataProvider.TotalGrossWeightUnitOfQty = Core.Constants.Weight.Tonnes;
			cofODataProvider.TotalOuterPack = 4;
			cofODataProvider.TotalOuterPackUnitOfQty = Core.Constants.PkgUnit.Carton;
			cofODataProvider.TradersRemarksForMessage = new ZString[] { "Test Declaration Message: exch rate 0.9565" };
			cofODataProvider.PreviousPermitNumber = "BA97985267";
			cofODataProvider.CountryOfFinalDestination = "US";
			OrganisationTestClass consignee = new OrganisationTestClass();
			consignee.Address = new OrganisationAddressTestClass("1556 smith st Los angeles");
			consignee.Name = "John Smith";
			cofODataProvider.Consignee = consignee;
			OrganisationTestClass exporter = new OrganisationTestClass();
			exporter.Address = new OrganisationAddressTestClass("Gwang Zo building c");
			exporter.UEN = "12093857483R";
			exporter.Name = "Singapore Exporters Pte Ltd";
			cofODataProvider.Exporter = exporter;
			OrganisationTestClass carrierAgent = new OrganisationTestClass();
			carrierAgent.Address = new OrganisationAddressTestClass("Keppel Warves");
			carrierAgent.UEN = "16003857483R";
			carrierAgent.Name = "Singapore Pacific Line";
			cofODataProvider.OutwardCarrierAgent = carrierAgent;
			ItemsTestClass[] invoices = new ItemsTestClass[1];
			invoices[0] = new ItemsTestClass();
			invoices[0].InvoicePK = ZGuid.NewZGuid();
			invoices[0].InvoiceCurrency = "SGD";
			invoices[0].IncoTerm = "FOB";
			invoices[0].InvoiceDate = new ZDate(2006, 12, 20);
			OrgHeader supplier = new BusinessObjectFactory().New<OrgHeader>();
			supplier.OH_FullName = "PACECO INDUSTRIAL SUPPLIES PTE LTD";
			supplier.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.CentralRegistrationNumber, "14242880000R");
			supplier.CustomsCodes.AddNew(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, "14242880000R");
			invoices[0].Supplier = new EntryOrganisationsInfo(supplier);
			invoices[0].InvoiceNumber = "INV01031";
			invoices[0].InvoiceCurrExchangeRate = 1;
			invoices[0].InvoiceTotalAmount = 23012;
			cofODataProvider.Invoices = invoices;
			serialNumber = 0;
			ItemsTestClass[] items = new ItemsTestClass[3];
			items[0] = new ItemsTestClass();
			InitItem(items[0], "48063000", "TRACING PAPERS", 144, 750);
			items[1] = new ItemsTestClass();
			InitItem(items[1], "48083000", "OTHER KRAFT PAPER CREPED OR CRINKLED", 250, 1095);
			items[2] = new ItemsTestClass();
			InitItem(items[2], "48091010", "CARBON PAPER IN ROLLS OR SHEETS", 500, 1575);
			cofODataProvider.CertItems = items;
			return cofODataProvider;
		}

		void InitItem(ItemsTestClass item, string certHSCode, string description, int qty, decimal unitPrice)
		{
			item.BrandName = "Spicers";
			item.CountryOfOriginCode = "SG";
			item.DutyAmount = 12;
			item.DutyUnitRate = 7;
			item.DutyUnitRateUnit = "PCE";
			item.DGIndicator = "N";
			item.E_SDNPIndicator = "Y";
			item.GSTPayable = 111;
			item.HSCode = certHSCode;
			item.CertHSCode = ((ZString)certHSCode).Left(6);
			item.HSQuantity = qty;
			item.InvoiceNumber = "INV01031";
			item.InvoiceDate = new ZDate(2006, 12, 20);
			item.IsBasedOnRates = true;
			item.IsDangerous = false;
			item.IsLiquor = false;
			item.IsMotorVehicle = false;
			item.IsTobacco = false;
			item.MarksAndNumbers = "Marks and numbers for certificate of Origin - line " + serialNumber.ToString();
			item.TextileCategoryCode = "15";
			item.TotalDutiableQuantity = 13;
			item.TotalDutiableQuantityUnitType = UnitOfQuantityCodeList.Codes.KGM;
			serialNumber++;
			item.SerialNumber = serialNumber.ToString();
			item.ItemQuantityUnitType = UnitOfQuantityCodeList.Codes.TNE;
			item.ItemQuantity = qty;
			item.UnitDutiableQuantity = 17;
			item.CustomsValue = 25000m;
			item.LSPValue = 2.28m;
			item.ItemValue = unitPrice;
			item.ItemDescription = description;
			item.PercentageContent = 35;
			item.OriginCriterion1 = "SINGLE";
			item.OriginCriterion2 = "COUNTRY";
			item.OriginCriterion3 = "CONTENT";
		}

		int serialNumber;
		#endregion
	}
}
