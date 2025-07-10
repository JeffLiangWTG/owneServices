using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.SG.Registry;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging.RefundInfo;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.DocumentEngine;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.PermitPrinting.Testing
{
	sealed class PrintPermitProcessorTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2021, 05, 01)]
		public void TestPdfAttachment()
		{
			var attachment = PintProcessor.GenerateDocumentToPDFAttachment(Message);
			AssertEquals("Permit.pdf", attachment.DisplayName);
			AssertNotNull(attachment.Data);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2021, 05, 10)]
		public void TestSignDocumentInMultipleTimes()
		{
			for (var i = 0; i < 3; i++)
			{
				var processor = new PrintPermitProcessorForTest();
				var attachment = processor.GenerateDocumentToPDFAttachment(Message);
				using (var pdfStream = new MemoryStream(attachment.Data))
				{
					var document = PdfSharp.Pdf.IO.PdfReader.Open(pdfStream);
					AssertEquals("Should use the cached signature and make sure the certificate data is always valid.", "CN=SG Customs CargoWise, O=WiseTech Global Ltd, L=Singapore, S=Singapore, C=SG", document.AcroForm.Fields[0].Name);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2021, 05, 20)]
		public void TestSignDocumentWithoutCertificate()
		{
			using (SGCustomsDataRegistry.Instance.PermitPrintingCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<byte>()))
			{
				var processor = new PrintPermitProcessorForTest();
				var attachment = processor.GenerateDocumentToPDFAttachment(Message);
				using (var pdfStream = new MemoryStream(attachment.Data))
				{
					var document = PdfSharp.Pdf.IO.PdfReader.Open(pdfStream);
					AssertNull("There is no valid certificate, so the document is not signed.", document.AcroForm);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDoSimplePrint()
		{
			PintProcessor.PrintTask = null;
			PintProcessor.DoSimplePrint(Message);
			AssertNotNull(PintProcessor.PrintTask);
			AssertEquals(1, PintProcessor.PrintTask.Count);
		}

		public void TestDoPrintTask()
		{
			PintProcessor.PrintTask = null;
			DocumentPack docPack = new DocumentPack();
			PintProcessor.DoPrintTask(docPack, false);
			AssertNotNull(PintProcessor.PrintTask);
			AssertEquals(1, PintProcessor.PrintTask.Count);
			AssertEquals(docPack, PintProcessor.PrintTask[0]);
			PintProcessor.PrintTask = null;
			PintProcessor.DoPrintTask(docPack, true);
			AssertNotNull(PintProcessor.PrintTask);
			AssertEquals(1, PintProcessor.PrintTask.Count);
			AssertEquals(docPack, PintProcessor.PrintTask[0]);
			Assert(PintProcessor.PrintTask[0].DeliveryInstructions.PrintMultiDocPack);
			AssertContains("Cargo Clearance Permit(s) from", PintProcessor.PrintTask[0].EmailSubjectForConsolidateReports);
		}

		public void TestPrintLastPermit()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			DocumentPack docPack = new DocumentPack();
			CusEntryHeader cusEntryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			EDIMessage message = cusEntryHeader.Messages.AddNew();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.SGCustomsTradenet4;
			message.EM_MessageType = Cuspmt09bMessageProcessor.MessageType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_IsActive = true;
			message.EM_SystemLastEditTimeUtc = ZDateTime.BrettsBirthday;
			PintProcessor.PrintLastPermit(declaration, docPack);
			AssertEquals(0, docPack.Count);
			message.EM_Status = EDIMessage.Status.Received;
			PintProcessor.PrintLastPermit(declaration, docPack);
			AssertEquals(1, docPack.Count);
			AssertEquals("Cargo Clearance Permits", docPack[0].DocumentName);
			docPack = new DocumentPack();
			message.EM_Status = EDIMessage.Status.Cancelled;
			PintProcessor.PrintLastPermit(declaration, docPack);
			AssertEquals(0, docPack.Count);
			var message1 = cusEntryHeader.Messages.AddNew();
			message1.EM_ApplicationCode = ApplicationCodeList.Codes.SGCustomsTradenet4;
			message1.EM_MessageType = Cuspmt09bMessageProcessor.MessageType;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_Status = EDIMessage.Status.Received;
			message1.EM_IsActive = true;
			message1.EM_SystemLastEditTimeUtc = ZDateTime.Today;
			var message2 = cusEntryHeader.Messages.AddNew();
			message2.EM_ApplicationCode = ApplicationCodeList.Codes.SGCustomsTradenet4;
			message2.EM_MessageType = Cuspmt09bMessageProcessor.MessageType;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_Status = EDIMessage.Status.Received;
			message2.EM_IsActive = true;
			message2.EM_SystemLastEditTimeUtc = ZDateTime.Today.AddDays(1);
			message2.EM_MessageText = "UNH+CWISES00013531+CUSPMT:0:1:RT:040+IPTUPT'BGM+961+15400820000E        200712117530+32'CST+++FRF'GEI+5+:Y'FTX+ACD+++RF35'FTX+ABO+++1) IMPORTER UNDER MES:2) REPLACEMENT PERMIT NO.ME7L070489W'RFF+ABT:IG7L136324A'DTM+160:20071229:102'DTM+9:200712290433:203'RFF+AGA:RFA2007120301'FTX+AER+++REFUND APPROVED BY SINGAPORE CUSTOMS.'NAD+DT++RAHIM ABD'CTA+IC+:S1481110I'COM+65490821:TE'UNS+D'UNS+S'CNT+6:1'TAX+7+GST'MOA+530:506.96'UNT+20+CWISES00013531'UNZ+1+91221838'";
			docPack = new DocumentPack();
			PintProcessor.PrintLastPermit(declaration, docPack);
			message.EM_Status = EDIMessage.Status.Received;
			AssertEquals(1, docPack.Count);
			docPack = new DocumentPack();
			message.EM_Status = EDIMessage.Status.Cancelled;
			PintProcessor.PrintLastPermit(declaration, docPack);
			AssertEquals(1, docPack.Count);
			docPack = new DocumentPack();
			message1.EM_Status = EDIMessage.Status.Cancelled;
			PintProcessor.PrintLastPermit(declaration, docPack);
			AssertEquals(0, docPack.Count);
			docPack = new DocumentPack();
			message.EM_Status = EDIMessage.Status.Cancelled;
			message1.EM_Status = EDIMessage.Status.Acknowledged;
			PintProcessor.PrintLastPermit(declaration, docPack);
			AssertEquals(0, docPack.Count);
			docPack = new DocumentPack();
			message.EM_Status = EDIMessage.Status.Received;
			message1.EM_Status = EDIMessage.Status.Cancelled;
			message.EM_MessageType = Cusres09bMessageProcessor.MessageType;
			PintProcessor.PrintLastPermit(declaration, docPack);
			AssertEquals(0, docPack.Count);
			docPack = new DocumentPack();
			message.EM_Status = EDIMessage.Status.Received;
			message1.EM_Status = EDIMessage.Status.Cancelled;
			message.EM_MessageType = Cuspmt09bMessageProcessor.MessageType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			PintProcessor.PrintLastPermit(declaration, docPack);
			AssertEquals(0, docPack.Count);
		}

		public void TestPrintLargeAmendmentPermitDoesntCauseException()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			DocumentPack docPack = new DocumentPack();
			CusEntryHeader cusEntryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			EDIMessage message = cusEntryHeader.Messages.AddNew();
			message.EM_MessageType = Cuspmt09bMessageProcessor.MessageType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_IsActive = true;
			message.EM_SystemLastEditTimeUtc = ZDateTime.Today;
			PintProcessor.PrintLastPermit(declaration, docPack);
			AssertEquals(0, docPack.Count);
			message.EM_Status = EDIMessage.Status.Received;
			PintProcessor.PrintLastPermit(declaration, docPack);
			AssertEquals(1, docPack.Count);
			AssertEquals("Cargo Clearance Permits", docPack[0].DocumentName);
			docPack = new DocumentPack();
			message.EM_Status = EDIMessage.Status.Cancelled;
			PintProcessor.PrintLastPermit(declaration, docPack);
			AssertEquals(0, docPack.Count);
			EDIMessage message1 = cusEntryHeader.Messages.AddNew();
			message1.EM_MessageType = Cuspmt09bMessageProcessor.MessageType;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_Status = EDIMessage.Status.Received;
			message1.EM_IsActive = true;
			message1.EM_SystemLastEditTimeUtc = ZDateTime.Today;
			EDIMessage message2 = cusEntryHeader.Messages.AddNew();
			message2.EM_MessageType = Cuspmt09bMessageProcessor.MessageType;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_Status = EDIMessage.Status.Received;
			message2.EM_IsActive = true;
			message2.EM_SystemLastEditTimeUtc = ZDateTime.Today.AddDays(1);
			message2.EM_MessageText = largeAmendmentMessage;
			docPack = new DocumentPack();
			PintProcessor.PrintLastPermit(declaration, docPack);
			message.EM_Status = EDIMessage.Status.Received;
			AssertEquals(1, docPack.Count);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIsGeneralDocument()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.SGCustomsTradenet4;
			message.EM_MessageText = messageText1;
			Assert(PrintPermitProcessor.IsGeneralDocument(message));
			message.EM_MessageText = messageText2;
			Assert(!PrintPermitProcessor.IsGeneralDocument(message));
			var xmlMessage = Factory.New<SGXmlEDIMessage>();
			xmlMessage.EM_MessageText = LoadFromFile(@"XML\INPPMT.XML");
			Assert(!PrintPermitProcessor.IsGeneralDocument(xmlMessage));
			xmlMessage.EM_MessageText = LoadFromFile(@"XML\IPTUPT.XML");
			xmlMessage.TradenetResponse.OutboundMessage.InPaymentUpdatePermit.Update.UpdateIndicatorCode = SGConstants.UpdateIndicators.FRF;
			Assert(PrintPermitProcessor.IsGeneralDocument(xmlMessage));
			xmlMessage.TradenetResponse.OutboundMessage.InPaymentUpdatePermit.Update.UpdateIndicatorCode = SGConstants.UpdateIndicators.CNL;
			Assert(!PrintPermitProcessor.IsGeneralDocument(xmlMessage));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetRefund()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.SGCustomsTradenet4;
			message.EM_MessageText = messageText1;
			AssertType<IPTUPT>(PrintPermitProcessor.GetRefund(message, Factory).Refund);
			var xmlMessage = Factory.New<SGXmlEDIMessage>();
			xmlMessage.EM_MessageText = LoadFromFile(@"XML\IPTUPT.XML");
			AssertType<BaseTradeNetRefund>(PrintPermitProcessor.GetRefund(xmlMessage, Factory).Refund);
		}

		public void TestIsTN4_1Permit()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.SGCustomsTradenet4;
			message.EM_MessageText = tn4_1MessageText;
			Assert(PrintPermitProcessor.IsTN4_1Permit(message));
			message.EM_MessageText = messageText2;
			Assert(!PrintPermitProcessor.IsTN4_1Permit(message));
			message = Factory.New<SGXmlEDIMessage>();
			Assert("The version of SGX Message should always be 4.1.", PrintPermitProcessor.IsTN4_1Permit(message));
		}

		#region Implementation
		const string messageText1 = @"UNH+CWISES00013531+CUSPMT:0:1:RT:040+IPTUPT'BGM+961+15400820000E        200712117530+32'CST+++FRF'GEI+5+:Y'FTX+ACD+++RF35'FTX+ABO+++1) IMPORTER UNDER MES:2) REPLACEMENT PERMIT NO.ME7L070489W'RFF+ABT:IG7L136324A'DTM+160:20071229:102'DTM+9:200712290433:203'RFF+AGA:RFA2007120301'FTX+AER+++REFUND APPROVED BY SINGAPORE CUSTOMS.'NAD+DT++RAHIM ABD'CTA+IC+:S1481110I'COM+65490821:TE'UNS+D'UNS+S'CNT+6:1'TAX+7+GST'MOA+530:506.96'UNT+20+CWISES00013531'UNZ+1+91221838'";
		const string messageText2 = @"UNH+1+CUSPMT:0:1:RT:040+INPPMT'BGM+962:::APS+XXXXXXXXE01T        200609130001+11'CST++4'LOC+9+USTXT'LOC+11+CZ:::CHANGI FTZ'LOC+88+CZ:::CHANGI FTZ'DTM+178:20060912:102'GEI+5+:Y'MEA+ABK++PKG:6.0000'MEA+AAH++KGM:612.0000'FTX+AAI+++INV?:94139979/94139978/94138259/ 94137934/94130844/94130846 94137934/94130844/94130846'RFF+MR:E05T.E05T001'RFF+MR:E03T.E03T001'RFF+ACE:ME6B000001Z'RFF+ABT:ME6I309961C'DTM+9:200609130015:203'DTM+160:20060913:102'DTM+273:2006091320060926:718'RFF+AEA:SC'FTX+CCI+++Z01 - APPROVED BY SINGAPORE CUSTOMS.'FTX+CCI+++GA APPROVED BY CUSTOMS SUBJECT TO THE DECLARANT COMPLYING:WITH THE FOLLOWING CONDITION(S) FOR THE PERMIT TO BE VALID.:FAILURE TO COMPLY WITH THE CONDITION(S) IS AN OFFENCE.'FTX+CCI+++MA THE GOODS AND THIS PERMIT MUST BE PRODUCED FOR CUSTOMS:CLEARANCE AT A FREE TRADE ZONE OUT GATE, WOODLANDS/TUAS:CHECKPOINT OR WOODLANDS TRAIN CHECKPOINT UNLESS IT IS:DIRECTED TO THE GREEN LANE AT THE TIME OF CLEARANCE.'FTX+CCI+++A6 IF THE PERMIT IS NOT USED, IT MUST BE CANCELLED OR:RE-VALIDATED NOT LATER THAN 24 HOURS OF ITS EXPIRY.'FTX+CCI+++GK YOU ARE ACCOUNTABLE TO THE INLAND REVENUE AUTHORITY OF:SINGAPORE FOR THE GST PAYABLE ON THE DECLARED GOODS.'FTX+CCI+++   ********** END OF CARGO CLEARANCE PERMIT **********'TDT+3++4+++++BR 6207'DOC+704+69578143170'NAD+AE+XXXXXXXXE01T++TESTING1 INTERNET'NAD+CG+11770720000D++SINGAPORE AIRPORT TERMINAL SERVICES'NAD+IM+XXXXXXXXE01T++TESTING1 INTERNET'NAD+BB'RFF+DAN:D'NAD+DT++TESTING1'CTA+IC+:2666666Z'COM+63111111:TE'NAD+FW+XXXXXXXXE01T++TESTING1 INTERNET'UNS+D'DMS+INVOICE DETAILS'MOA+39:251550.29:USD'CUX+++1.571000'TOD+++CIF'NAD+SU++.+1'DOC+380+.'DTM+3:20060912:102'ALC+C'MOA+64:1014.20:USD'CUX+++1.571000'ALC+C'MOA+70:3967.79:SGD'PCD+5:1.000'CST+1+84314390'FTX+AAA+++OILWELL EQUIPMENT SPARE PARTS.'FTX+PRD+++UNBRANDED:NA'LOC+27+US'MEA+AAF++LOT:1.0000'MOA+63:400746.60'MOA+41:400746.60'MOA+146:400746.60:SGD'RFF+IV:.'DOC+703+2TW4279'TAX+7++++:::5'MOA+124:20037.33'UNS+S'CNT+5:1'TAX+1'MOA+63:400746.60'TAX+7'MOA+124:20037.33'TAX+2'MOA+9:20037.33'UNT+71+1'";
		const string largeAmendmentMessage = @"UNH+CWISEB00004063+CUSPMT:0:1:RT:040+INPUPT'BGM+962:::APS+198703865Z          201012165427+32'CST++3+AME'LOC+11+PPZ'LOC+88+LW325:::AIR MARKET EXPRESS (S) PTE LTD - LW325  NO.22 LOYANG LANE  SINGAPORE 508931'LOC+9+UAILK'LOC+164+PPW'DTM+178:20101114:102'GEI+5+:Y'MEA+ABK++UNT:1.0000'MEA+AAH++TNE:19.4328'EQD+CN+FCIU9066009:1+:::FCL40019'SEL+YMLC737758'FTX+ACF+++DISCREPANCY ON VOLUME'RFF+ABT:II0K219629I'DTM+160:20101216:102'DTM+9:201012161700:203'DTM+273:2010111120101125:718'DTM+182:20101111:102'FTX+CUS+++107110000000MF:103106000000MF:121107000000DF:119105000000MF:163900000000MF'FTX+CUS+++107104000000MF:107106000000MF:103108000000MF:122107000000DF:150103000000MF'FTX+CUS+++107109000000MF:147109000000MF:147104000000MF:147107000000MF:147112000000MF'FTX+CUS+++105112000000MF:105104000000MF:150108000000MF:103110000000MF:150111000000MF'FTX+CUS+++147102000000MF:105103000000MF:107102000000MF:119104000000MF:103104000000MF'FTX+CUS+++119103000000MF:150107000000MF:119102000000MF:147101000000MF:147111000000MF'FTX+CUS+++105101000000MF:103111000000MF:105110000000MF:103101000000MF:105111000000MF'FTX+CUS+++150105000000MF:147108000000MF:150106000000MF:105108000000MF:105107000000MF'FTX+CUS+++147103000000MF:105102000000MF:150112000000MF:105109000000MF:150104000000MF'FTX+CUS+++147106000000MF:105105000000MF:164900000000MF:119107000000MF:103109000000MF'FTX+CUS+++103103000000MF:150102000000MF:103102000000MF:150101000000MF:107107000000MF'FTX+CUS+++150110000000MF:107111000000MF:107103000000MF:107101000000MF:103107000000MF'FTX+CUS+++120107000000MF:150109000000MF:107108000000MF:103112000000MF:147105000000MF'FTX+CUS+++147110000000MF:107105000000MF:105106000000MF:103105000000MF:107112000000MF'RFF+MS:06'RFF+DM:IP10H1335'RFF+AEA:CA'DTM+182:201012161700:203'FTX+REG+++A20- APPROVED BY AVA (PROCESSED FOOD). THIS CCP ALSO SERVES : AS AN AVA PERMIT FOR ITEMS UNDER AVA CONTROL SUBJECT TO  :COMPLIANCE WITH THE SALE OF FOOD ACT AND THE FOOD  :REGULATIONS. PLEASE NOTE THAT MEAT AND SEAFOOD PRODUCTS ARE 'FTX+REG+++ NOT ALLOWED TO DECLARE UNDER THIS PERMIT. FAILURE TO :COMPLY  MAY BE SUBJECTED TO A FINE NOT EXCEEDING $10,000 :AND/OR  IMPRISONMENT NOT EXCEEDING 3 MONTHS.:FOR ALL ITEMS.'RFF+AEA:SC'FTX+CCI+++Z20 - AMENDMENT APPROVED BY SINGAPORE CUSTOMS ON CONDITION :THAT THE EARLIER APPROVED PERMIT HAS NOT BEEN USED AND THIS:SUPERCEDES THE PREVIOUS PERMIT.'FTX+CCI+++Z10 - APPROVED BY SINGAPORE CUSTOMS SUBJECT TO THE:CONDITIONTHAT YOU COMPLY WITH THE REQUIREMENTS OF THE:COMPETENT      AUTHORITY.'FTX+CCI+++Y95 - PLS CHECK AGAIN THE DECLARED - 1) HS :CODES\DESCRIPTION, OR 2) ITEM QUANTITY OR VALUE, OR 3) ITEM :VALUE WHICH EXCEEDED $1 MILLION. IF WRONG, PLEASE CANCEL :THIS CARGO CLEARANCE PERMIT WITHIN 48 HOURS.'FTX+CCI+++GA APPROVED BY CUSTOMS SUBJECT TO THE DECLARANT COMPLYING:WITH THE FOLLOWING CONDITION(S) FOR THE PERMIT TO BE VALID.:FAILURE TO COMPLY WITH THE CONDITION(S) IS AN OFFENCE.'FTX+CCI+++P1 THE CONTAINER(S) MUST BE PRODUCED AT CUSTOMS :CHECKPOINT(S) FOR CLEARANCE UNLESS DIRECTED TO ?'GREEN?' :LANE. FOR CONTAINER(S) TO BE SCANNED, PLEASE PRODUCE FOR :SCANNING BY ICA AT SCANNING STATION AS DIRECTED.'FTX+CCI+++AY GOODS NOT EXPORTED/TRANSHIPPED OR BONDED IN A  :BONDED/LICENSED WAREHOUSE OR RECEIVED BY THE CLAIMANT ON :THE SAME DAY OF REMOVAL MUST BE STORED AT A PLACE APPROVED :BY A PROPER OFFICER OF CUSTOMS.'FTX+CCI+++A6 IF THE PERMIT IS NOT USED, IT MUST BE CANCELLED OR:RE-VALIDATED NOT LATER THAN 24 HOURS OF ITS EXPIRY.'FTX+CCI+++B2 AN E-FILE APPLICATION MUST BE MADE TO COMPANY COMPLIANCE :BRANCH FOR SUPERVISION OF UNSTUFFING OF CONTAINERS. :(WEBSITE?: HTTP?://WWW.CUSTOMS.GOV.SG)'FTX+CCI+++A9 THE GOODS MUST BE PRODUCED TO THE CHECKPOINT IN A:BONDED TRUCK/CONTAINER FOR SEALING. THE TRUCK/CONTAINER:MUST BE ABLE TO BE SECURED TO THE SATISFACTION OF THE:PROPER OFFICER OF CUSTOMS'FTX+CCI+++EEE - END OF CARGO CLEARANCE PERMIT.'TDT+3++1+++++0018E:::YM KEELUNG'DOC+704+YMLUM968002038'NAD+DT++JOANNE TAN'CTA+IC+:S1675235E'COM+62600118:TE'NAD+AE+198703865Z++AIR MARKET EXPRESS (S) PTE LTD'NAD+IM+201009758D++EPHRAIM COMMERCE ADVISORY PTE LTD'NAD+CG+199907099D++YANGMING SHIPPING (SINGAPORE) PTE:LTD'NAD+FW+199907099D++YANGMING SHIPPING (SINGAPORE) PTE:LTD'UNS+D'DMS+INVOICE DETAILS'MOA+39:11616.49:USD'CUX+++1.291000'TOD+++CFR'NAD+SE'DOC+380+INVOICE'DTM+3:20101023:102'ALC+C'MOA+70:116.15:USD'PCD+5:1.000'CUX+++1.291000'CST+1+22042111'FTX+AAA+++NIVA WHITE SEMISWEET SPARKLING WINE:- 11.5%'FTX+PRD+++UNBRANDED'LOC+27+UA'LOC+18+AMLA0580'MEA+AAF++LTR:720.0000'MEA+AAE++LTR:720.0000'MEA+AAI++LTR:0.7500'MEA+ABA++LTR:720.9600'MEA+AAG++LPA:11.500'PAC+80+3+CTN'PAC+12+2+BOT'MOA+63:1176.64'MOA+41:0.00'RFF+IV:INVOICE'RFF+AEA:ZBP0BJ0QVDG'DOC+703+YMLUM968002038'TAX+7++++:::7'MOA+124:488.08'TAX+5++++LPA:::70.0000'MOA+161:5796.00'CST+2+22042111'FTX+AAA+++NIVA RED SEMISWEET SPARKLING WINE -:11.5%'FTX+PRD+++UNBRANDED'LOC+27+UA'LOC+18+AMLA0581'MEA+AAF++LTR:2106.0000'MEA+AAE++LTR:2106.0000'MEA+AAI++LTR:0.7500'MEA+ABA++LTR:2162.8800'MEA+AAG++LPA:11.500'PAC+234+3+CTN'PAC+12+2+BOT'MOA+63:3800.33'MOA+41:0.00'RFF+IV:INVOICE'RFF+AEA:ZBP0BH0QVDG'DOC+703+YMLUM968002038'TAX+7++++:::7'MOA+124:1452.75'TAX+5++++LPA:::70.0000'MOA+161:16953.30'CST+3+22042111'FTX+AAA+++NIVA CHARDONNAY FINE DRY WHITE WINE:- 11.5%'FTX+PRD+++UNBRANDED'LOC+27+UA'LOC+18+AMLA0582'MEA+AAF++LTR:428.4000'MEA+AAE++LTR:428.4000'MEA+AAI++LTR:0.7000'MEA+ABA++LTR:426.0000'MEA+AAG++LPA:11.500'PAC+51+3+CTN'PAC+12+2+BOT'MOA+63:585.20'MOA+41:0.00'RFF+IV:INVOICE'RFF+AEA:ZBP0BJ0QVDG'DOC+703+YMLUM968002038'TAX+7++++:::7'MOA+124:282.37'TAX+5++++LPA:::70.0000'MOA+161:3448.62'CST+4+22042111'FTX+AAA+++NIVA CABERNET FINE DRY RED WINE -:12%'FTX+PRD+++UNBRANDED'LOC+27+UA'LOC+18+AMLA0583'MEA+AAF++LTR:848.4000'MEA+AAE++LTR:848.4000'MEA+AAI++LTR:0.7000'MEA+ABA++LTR:852.0000'MEA+AAG++LPA:12.000'PAC+101+3+CTN'PAC+12+2+BOT'MOA+63:1198.55'MOA+41:0.00'RFF+IV:INVOICE'RFF+AEA:ZBP0BH0QVDG'DOC+703+YMLUM968002038'TAX+7++++:::7'MOA+124:582.76'TAX+5++++LPA:::70.0000'MOA+161:7126.56'CST+5+22042111'FTX+AAA+++NIVA PRINCESS OF THE NIGHT:SEMISWEET RED WINE - 12%'FTX+PRD+++UNBRANDED'LOC+27+UA'LOC+18+AMLA0584'MEA+AAF++LTR:999.6000'MEA+AAE++LTR:999.6000'MEA+AAI++LTR:0.7000'MEA+ABA++LTR:1022.4000'MEA+AAG++LPA:12.000'PAC+119+3+CTN'PAC+12+2+BOT'MOA+63:1370.67'MOA+41:0.00'RFF+IV:INVOICE'RFF+AEA:ZBP0BH0QVDG'DOC+703+YMLUM968002038'TAX+7++++:::7'MOA+124:683.71'TAX+5++++LPA:::70.0000'MOA+161:8396.64'CST+6+22042111'FTX+AAA+++NIVA BULL?'S BLOOD SEMISWEET RED:WINE - 12%'FTX+PRD+++UNBRANDED'LOC+27+UA'LOC+18+AMLA0585'MEA+AAF++LTR:2520.0000'MEA+AAE++LTR:2520.0000'MEA+AAI++LTR:0.7000'MEA+ABA++LTR:2556.0000'MEA+AAG++LPA:12.000'PAC+300+3+CTN'PAC+12+2+BOT'MOA+63:3426.68'MOA+41:0.00'RFF+IV:INVOICE'RFF+AEA:ZBP0BH0QVDG'DOC+703+YMLUM968002038'TAX+7++++:::7'MOA+124:1721.63'TAX+5++++LPA:::70.0000'MOA+161:21168.00'CST+7+22042111'FTX+AAA+++NIVA TAMYANKA PREMIUM SEMISWEET:WHITE WITE - 11.5%'FTX+PRD+++UNBRANDED'LOC+27+UA'LOC+18+AMLA0586'MEA+AAF++LTR:2518.6000'MEA+AAE++LTR:2518.6000'MEA+AAI++LTR:0.7000'MEA+ABA++LTR:2556.0000'MEA+AAG++LPA:11.500'PAC+3598+3+BOT'MOA+63:3426.68'MOA+41:0.00'RFF+IV:INVOICE'RFF+AEA:ZBP0BJ0QVDG'DOC+703+YMLUM968002038'TAX+7++++:::7'MOA+124:1659.10'TAX+5++++LPA:::70.0000'MOA+161:20274.73'CST+8+22042112'FTX+AAA+++UKRAINIAN KAGOR DESSERT RED WINE -:16%'FTX+PRD+++UNBRANDED'LOC+27+UA'LOC+18+AMLA0587'MEA+AAF++LTR:16.8000'MEA+AAE++LTR:16.8000'MEA+AAI++LTR:0.7000'MEA+ABA++LTR:17.0400'MEA+AAG++LPA:16.000'PAC+2+3+CTN'PAC+12+2+BOT'MOA+63:35.33'MOA+41:0.00'RFF+IV:INVOICE'RFF+AEA:ZBP0BH0QVDH'DOC+703+YMLUM968002038'TAX+7++++:::7'MOA+124:15.64'TAX+5++++LPA:::70.0000'MOA+161:188.16'CST+9+22042112'FTX+AAA+++THE SUN IN THE WINEGLASS DESSERT:WHITE WINE - 16%'FTX+PRD+++UNBRANDED'LOC+27+UA'LOC+18+AMLA0588'MEA+AAF++LTR:16.8000'MEA+AAE++LTR:16.8000'MEA+AAI++LTR:0.7000'MEA+ABA++LTR:17.0400'MEA+AAG++LPA:16.000'PAC+2+3+CTN'PAC+12+2+BOT'MOA+63:34.02'MOA+41:0.00'RFF+IV:INVOICE'RFF+AEA:ZBP0BJ0QVDH'DOC+703+YMLUM968002038'TAX+7++++:::7'MOA+124:15.55'TAX+5++++LPA:::70.0000'MOA+161:188.16'CST+10+22042112'FTX+AAA+++LIDIYA DESSERT ROSE WINE - 16%'FTX+PRD+++UNBRANDED'LOC+27+UA'LOC+18+AMLA0589'MEA+AAF++LTR:16.8000'MEA+AAE++LTR:16.8000'MEA+AAI++LTR:0.7000'MEA+ABA++LTR:17.0400'MEA+AAG++LPA:16.000'PAC+2+3+CTN'PAC+12+2+BOT'MOA+63:34.39'MOA+41:0.00'RFF+IV:INVOICE'RFF+AEA:ZBP0BH0QVDH'DOC+703+YMLUM968002038'TAX+7++++:::7'MOA+124:15.58'TAX+5++++LPA:::70.0000'MOA+161:188.16'CST+11+22042112'FTX+AAA+++IZABELLA DESSERT RED WINE - 16%'FTX+PRD+++UNBRANDED'LOC+27+UA'LOC+18+AMLA0590'MEA+AAF++LTR:16.8000'MEA+AAE++LTR:16.8000'MEA+AAI++LTR:0.7000'MEA+ABA++LTR:17.0400'MEA+AAG++LPA:16.000'PAC+2+3+CTN'PAC+12+2+BOT'MOA+63:34.39'MOA+41:0.00'RFF+IV:INVOICE'RFF+AEA:ZBP0BH0QVDH'DOC+703+YMLUM968002038'TAX+7++++:::7'MOA+124:15.58'TAX+5++++LPA:::70.0000'MOA+161:188.16'CST+12+22042111'FTX+AAA+++MERLOT FINE DRY RED WINE - 12%'FTX+PRD+++UNBRANDED'LOC+27+UA'LOC+18+AMLA0591'MEA+AAF++LTR:16.8000'MEA+AAE++LTR:16.8000'MEA+AAI++LTR:0.7000'MEA+ABA++LTR:17.0400'MEA+AAG++LPA:12.000'PAC+2+3+CTN'PAC+12+2+BOT'MOA+63:23.96'MOA+41:0.00'RFF+IV:INVOICE'RFF+AEA:ZBP0BH0QVDG'DOC+703+YMLUM968002038'TAX+7++++:::7'MOA+124:11.56'TAX+5++++LPA:::70.0000'MOA+161:141.12'UNS+S'CNT+5:12'CNT+6:1'CNT+22:70'TAX+1'MOA+63:15146.84'TAX+7'MOA+124:6944.31'TAX+5'MOA+161:84057.61'UNT+333+CWISEB00004063'";
		const string tn4_1MessageText = @"UNH+1+CUSPMT:0:1:RT:041+OUTPMT'BGM+962:::DRT+XXXXXXXXE47T     201102140002+11'CST++5'LOC+11+O:::OTHERS'LOC+12+MYJHB'LOC+36+MY'LOC+88+LHQ:::WOODLANDS CHECKPOINT,WOODLANDS ROAD'DTM+136:20110218:102'DTM+416:20110214085700SST:304'GEI+5+:Y'MEA+ABK++CAR:150'MEA+AAH++TNE:2.250'RFF+ABT:OO6I101741A'DTM+148:20110214001550SST:304'DTM+273:2011021420110222:718'RFF+AEA:SC'FTX+CCI++GA+APPROVED BY CUSTOMS SUBJECT TO THE DECLARANT COMPLYING WITH THE FOLLOWING CONDITION(S) FOR THE PERMIT TO BE VALID. FAILURE TO COMPLY WITH THE CONDITION(S) IS AN OFFENCE.'FTX+CCI++H1+THE GOODS AND THIS PERMIT MUST BE PRODUCED FOR CUSTOMS CLEARANCE/ENDORSEMENT AT WOODLANDS CHECKPOINT/TUAS CHECKPOINT'FTX+CCI++AY+GOODS NOT EXPORTED/TRANSHIPPED OR BONDED IN A LICENSED WAREHOUSE OR RECEIVED BY THE CLAIMANT ON THE SAME DAY OF REMOVAL MUST BE STORED AT A PLACE APPROVED BY A PROPER OFFICER OF CUSTOMS.'FTX+CCI++A6+IF THE PERMIT IS NOT USED, IT MUST BE CANCELLED OR RE-VALIDATED NOT LATER THAN 24 HOURS OF ITS EXPIRY.'FTX+CCI++B4+GOODS MUST BE STORED IN A PROPER CARGO HOLD/HATCH.'FTX+CCI++H3+AT THE TIME OF EXPORT, THE MALAYSIAN DUTY PAYMENT PERMIT SHALL BE PRODUCED TO CUSTOMS AT WOODLANDS CHECKPOINT. CHECKPOINT OFFICER IS TO INSERT THE MALAYSIAN DUTY PAYMENT PERMIT NUMBER(S) ON THIS OUTWARD PERMIT.'FTX+CCI++H4+IF THE GOODS ARE NOT COVERED BY A MALAYSIAN DUTY PAYMENT PERMIT, YOU MUST SUBMIT A COPY OF MALAYSIAN INWARD PERMIT TO PERMITS COMPLIANCE UNIT,CUSTOMS HQ WITHIN 4 WORKING DAYS OF EXPORT.'FTX+CCI++EEE+END OF CARGO CLEARANCE PERMIT.'TDT+12++3'NAD+AE+XXXXXXXXE47T++TESTING47:::::1'NAD+BB'RFF+DAN:D'NAD+DT'CTA+IC+1619574Z:TESTING NAME 47'COM+12345678:TE'NAD+EX+XXXXXXXXE47T++TESTING47:::::2'NAD+IM+XXXXXXXXE47T++TESTING47:::::2'UNS+D'CST+1+22083010'FTX+AAA+++NOBLESSE SCOTCH WHISKY 12/0.75'FTX+PRD+++NOBLESSE'LOC+18+JLLA9980'LOC+27+GB'MEA+AAF++LTR:1350.0000'MEA+AAE++LTR:1350.0000'MEA+AAI++LTR:0.7500'PAC+150+3+CAR'PAC+12+2+BOT'MOA+63:7200.00'UNS+S'CNT+5:1'TAX+1'MOA+62:7200.00'UNT+51+1'";

		string LoadFromFile(string fileName)
		{
			return File.ReadAllText(BaseSourcePath + @"Enterprise\Product\Operations\Customs\SG\Core\Business.Test\Messaging\CustomsMessaging\MessageProcessors\TestMessages\" + fileName);
		}

		public class PrintPermitProcessorForTest : PrintPermitProcessor
		{
			protected override void RunPrintTask(PrintTask printTask)
			{
				this.printTask = printTask;
			}

			public PrintTask PrintTask
			{
				get
				{
					return printTask;
				}

				set
				{
					printTask = value;
				}
			}

			PrintTask printTask;
		}

		PrintPermitProcessorForTest PintProcessor
		{
			get
			{
				if (fPintProcessor == null)
				{
					fPintProcessor = new PrintPermitProcessorForTest();
				}

				return fPintProcessor;
			}
		}

		PrintPermitProcessorForTest fPintProcessor;
		EDIMessage Message
		{
			get
			{
				if (message == null)
				{
					message = Factory.New<EDIMessage>();
					message.EM_MessageText = LoadFromFile("EDI_TNPPMT_REM.txt");
					message.EM_ApplicationCode = EDIMessage.ApplicationCodes.SingaporeTradenet4;
					message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				}

				return message;
			}
		}

		EDIMessage message;
		#endregion
	}
}
