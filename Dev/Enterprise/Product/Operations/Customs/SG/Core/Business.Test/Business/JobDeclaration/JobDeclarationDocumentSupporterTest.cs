using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging.RefundInfo;
using Enterprise.Customs.SG.V4.Business.PermitPrinting;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(JobDeclarationDocumentSupporter))]
	public class JobDeclarationDocumentSupporterTest : Customs.Business.Testing.BaseJobDeclarationDocumentSupportTest
	{
		public override void TestGetBODocDataProvidersNotFoundMessage()
		{
			base.TestGetBODocDataProvidersNotFoundMessage();
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("Permit message cannot be found.", declaration.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.SGPrintPermit), null));
			AssertEquals("Refund message cannot be found.", declaration.DocumentSupporter.GetBODocDataProvidersNotFoundMessage(new DataContextValueForTesting(Enterprise.Core.Constants.DataContext.SGRefundInfo), null));
		}

		public void TestIsDataContextSupported()
		{
			Assert(DocumentSupporterWithMessages.IsDataContextSupported(new DataContextValue(nameof(Core.Constants.DataContext.SGPrintPermit))));
			Assert(DocumentSupporterWithMessages.IsDataContextSupported(new DataContextValue(nameof(Core.Constants.DataContext.SGRefundInfo))));
			Assert(DocumentSupporterWithMessages.IsDataContextSupported(new DataContextValue(nameof(Core.Constants.DataContext.Declaration))));
		}

		public void TestGetDocumentWrappers()
		{
			AssertNull(DocumentSupporterWithoutMessages.GetDocumentWrappers(Core.Constants.DataContext.SGPrintPermit, null));
			AssertNull(DocumentSupporterWithoutMessages.GetDocumentWrappers(Core.Constants.DataContext.SGRefundInfo, null));
			var documentWrappers = DocumentSupporterWithMessages.GetDocumentWrappers(Core.Constants.DataContext.SGPrintPermit, null);
			AssertEquals(1, documentWrappers.Length);
			var documentWrapper = documentWrappers[0];
			Assert(documentWrapper.WrappedObject is PrintPermit);
			documentWrappers = DocumentSupporterWithRefundPermit.GetDocumentWrappers(Core.Constants.DataContext.SGRefundInfo, null);
			AssertEquals(1, documentWrappers.Length);
			documentWrapper = documentWrappers[0];
			Assert(documentWrapper.WrappedObject is RefundInfo);
		}

		public void TestGetDataStateBeforeRun_Permit()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = SGConstants.Permit;
			Factory.Save();
			var permitMenuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "Permit"));
			var checkpoint = Env.Security.FindOrCreateDocumentCheckpoint(permitMenuItem.PK.ToGuid(), permitMenuItem.SU_MenuNameMultilingual, ModuleIDs.Customs.JobDeclaration, Env.Security.Operations);
			checkpoint.IsAllowed = true;
			var dataState = DocumentSupporterWithoutMessages.GetDataStateBeforeRun(menuItem);
			AssertEquals(false, dataState.IsValid);
			AssertEquals("Permit cannot be printed because no permits have been received.", dataState.ErrorMessage);
			dataState = DocumentSupporterWithRefundPermit.GetDataStateBeforeRun(menuItem);
			AssertEquals(false, dataState.IsValid);
			AssertEquals("Permit cannot be printed because no permits have been received.", dataState.ErrorMessage);
			dataState = DocumentSupporterWithMessages.GetDataStateBeforeRun(menuItem);
			AssertEquals(true, dataState.IsValid);
			checkpoint.IsAllowed = false;
			dataState = DocumentSupporterWithMessages.GetDataStateBeforeRun(menuItem);
			AssertEquals(false, dataState.IsValid);
			AssertEquals(checkpoint.ErrorMessageForNotAllowed, dataState.ErrorMessage);
		}

		public void TestGetDataStateBeforeRun_Refund()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_MenuName = SGConstants.Refund;
			Factory.Save();
			var dataState = DocumentSupporterWithoutMessages.GetDataStateBeforeRun(menuItem);
			AssertEquals(false, dataState.IsValid);
			AssertEquals("Refund cannot be printed because no refund information have been received.", dataState.ErrorMessage);
		}

		public void TestLargeAmendedPermitDoesNotCauseException()
		{
			DocumentWrapper[] documentWrappers = DocumentSupporterWithAmendedPermit.GetDocumentWrappers(Core.Constants.DataContext.SGPrintPermit, null);
			AssertEquals(1, documentWrappers.Length);
			DocumentWrapper documentWrapper = documentWrappers[0];
			Assert(documentWrapper.WrappedObject is PrintPermit);
		}

		protected override bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			return ((documentCommand.MenuItemUniqueCode == "- : Customs" || documentCommand.SU_MenuName == "Landed Costing") || base.ExcludeDocumentCommandTest(documentCommand));
		}

		protected override Dictionary<string, int> MaxDBHitCounts
		{
			get
			{
				var maxHits = base.MaxDBHitCounts;
				maxHits["EDIMessage"] = 2;
				return maxHits;
			}
		}

		protected override IEnumerable<IDocumentSupportable> TopLevelBOsForRunningDocumentsTest
		{
			get
			{
				var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
				declaration.JE_MessageType = MessageTypeCodeList.Codes.INP;
				PopulateDeclaration(declaration, false, false);
				CreateNewPermitMessage(declaration, true, false);
				yield return declaration;
			}
		}

		public void TestRunningDocumentsShouldNotCauseExceptionForIPT()
		{
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			PopulateDeclaration(declaration, false, false);
			CreateNewPermitMessage(declaration, true, false);
			AssertRunningDocumentsForBusinessObject(declaration);
		}

		public void TestRunningDocumentsShouldNotCauseExceptionForOUT()
		{
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
			declaration.JE_MessageType = MessageTypeCodeList.Codes.OUT;
			PopulateDeclaration(declaration, false, false);
			CreateNewPermitMessage(declaration, true, false);
			AssertRunningDocumentsForBusinessObject(declaration);
		}

		public override void TestGetFilterValueMSGBKR()
		{
			Declaration.JE_MessageType = MessageTypeCodeForExportDeclaration;
			AssertEquals("For filter 'MSGBKR' result is 'EXP'", "EXP", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKR));
			Declaration.JE_MessageType = MessageTypeCodeForImportDeclaration;
			AssertEquals("For filter 'MSGBKR' result is 'IMP'", "IMP", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKR));
		}

		public override void TestGetFilterValueMSGBKRCTY()
		{
			Declaration.JE_MessageType = MessageTypeCodeForExportDeclaration;
			AssertEquals("For filter 'MSGBKR' result is 'EXP' + Current Country Code", "EXP" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTY));
			Declaration.JE_MessageType = MessageTypeCodeForImportDeclaration;
			AssertEquals("For filter 'MSGBKR' result is 'IMP' + Current Country Code", "IMP" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTY));
		}

		public override void TestGetFilterValueMSGBKRCTYMOD()
		{
			Declaration.JE_MessageType = MessageTypeCodeForExportDeclaration;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("For filter 'MSGBKRCTYMOD' result is 'EXP' + Current Country Code + \"AIR\"", "EXP" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AIR", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYMOD));
			Declaration.JE_MessageType = MessageTypeCodeForImportDeclaration;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("For filter 'MSGBKRCTYMOD' result is 'IMP' + Current Country Code + \"AIR\"", "IMP" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "AIR", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYMOD));
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("For filter 'MSGBKRCTYMOD' result is 'IMP' + Current Country Code + \"SEA\"", "IMP" + GlbCompany.CurrentCompany.GC_RN_NKCountryCode + "SEA", Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.MSGBKRCTYMOD));
		}

		public override void TestGetFilterValueBKRCTYAPP()
		{
			Declaration.JE_MessageType = MessageTypeCodeForExportDeclaration;
			Declaration.JE_ApplicationCode = "SG4";
			AssertEquals("For filter 'BKRCTYAPP' result is Current Country Code + JE_ApplicationCode", GlbCompany.CurrentCompany.GC_RN_NKCountryCode + Declaration.JE_ApplicationCode, Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.BKRCTYAPP));
			Declaration.JE_MessageType = MessageTypeCodeForImportDeclaration;
			Declaration.JE_ApplicationCode = "4.1";
			AssertEquals("For filter 'BKRCTYAPP' result is 'IMP' + Current Country Code", GlbCompany.CurrentCompany.GC_RN_NKCountryCode + Declaration.JE_ApplicationCode, Declaration.DocumentSupporter.GetFilterValue(DocumentFilters.BKRCTYAPP));
		}

		public new void TestTransportMode()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.SG.V4.Business.JobDeclaration.JobMessageTypeList.Codes.Import;
			var supporter = new JobDeclarationDocumentSupporter(declaration);
			declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_1_SEA;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Sea, supporter.TransportMode);
			declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_2_Rail;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Rail, supporter.TransportMode);
			declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_3_Road;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Road, supporter.TransportMode);
			declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_4_Air;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Air, supporter.TransportMode);
			declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_5_Mail;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Other, supporter.TransportMode);
			declaration.JE_TransportMode = TransportModeCodeList.Codes.TransportMode_7_Pipeline;
			AssertEquals("supporter.TransportMode", Core.Constants.TransportModes.Other, supporter.TransportMode);
		}

		#region Implementation
		protected override Customs.Business.BaseJobDeclaration GetJobDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}

		protected override ZString GetMainNameSpace()
		{
			return "Enterprise.Customs.SG.V4.Business.";
		}

		public override void TestGetDocBusinessObjects()
		{
			var declaration = (JobDeclaration)GetDocumentSupportableBusinessObject();
			var invoiceGroupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoiceHeader = invoiceGroupHeader.JobComInvoiceHeaders.AddNew();
			DocumentWrapper[] result = declaration.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Declaration, null);
			AssertEquals("Document wrapper for data context of declaration is of type DocDeclaration", "Enterprise.Customs.SG.V4.Business.DocDeclaration", result[0].GetType().ToString());
		}

		protected override ZString MessageTypeCodeForImportDeclaration
		{
			get
			{
				return MessageTypeCodeList.Codes.IPT;
			}
		}

		protected override ZString MessageTypeCodeForExportDeclaration
		{
			get
			{
				return MessageTypeCodeList.Codes.OUT;
			}
		}

		#endregion
		#region Declaration
		new JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					PopulateDeclaration(declaration, false, false);
				}

				return declaration;
			}
		}

		JobDeclaration declaration;
		void PopulateDeclaration(JobDeclaration declaration, bool isRefundMessage, bool isAmendedPermit)
		{
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JobComInvoiceLines.AddNew();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			CreateNewPermitMessage(declaration, isRefundMessage, isAmendedPermit);
		}

		void CreateNewPermitMessage(JobDeclaration declaration, bool isRefundMessage, bool isAmendedPermit)
		{
			var permitMessage = declaration.ActiveEntryHeaders[0].Messages.AddNew();
			permitMessage.EM_ApplicationCode = ApplicationCodeList.Codes.SGCustomsTradenet4;
			if (isRefundMessage)
			{
				SetRefundMessageText(permitMessage);
			}
			else if (isAmendedPermit)
			{
				SetAmendedPermitMessageText(permitMessage);
			}
			else
			{
				SetNormalPermitMessageText(permitMessage);
			}

			permitMessage.EM_MessageType = Cuspmt09bMessageProcessor.MessageType;
			permitMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			permitMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;
		}

		void SetRefundMessageText(EDIMessage message)
		{
			message.EM_MessageText = "UNH+CWISES00013531+CUSPMT:0:1:RT:040+IPTUPT'BGM+961+15400820000E        200712117530+32'CST+++FRF'GEI+5+:Y'FTX+ACD+++RF35'FTX+ABO+++1) IMPORTER UNDER MES:2) REPLACEMENT PERMIT NO.ME7L070489W'RFF+ABT:IG7L136324A'DTM+160:20071229:102'DTM+9:200712290433:203'RFF+AGA:RFA2007120301'FTX+AER+++REFUND APPROVED BY SINGAPORE CUSTOMS.'NAD+DT++RAHIM ABD'CTA+IC+:S1481110I'COM+65490821:TE'UNS+D'UNS+S'CNT+6:1'TAX+7+GST'MOA+530:506.96'UNT+20+CWISES00013531'UNZ+1+91221838'";
		}

		void SetAmendedPermitMessageText(EDIMessage message)
		{
			message.EM_MessageText = @"UNH+CWISEB00004063+CUSPMT:0:1:RT:040+INPUPT'BGM+962:::APS+198703865Z          201012165427+32'CST++3+AME'LOC+11+PPZ'LOC+88+LW325:::AIR MARKET EXPRESS (S) PTE LTD - LW325  NO.22 LOYANG LANE  SINGAPORE 508931'LOC+9+UAILK'LOC+164+PPW'DTM+178:20101114:102'GEI+5+:Y'MEA+ABK++UNT:1.0000'MEA+AAH++TNE:19.4328'EQD+CN+FCIU9066009:1+:::FCL40019'SEL+YMLC737758'FTX+ACF+++DISCREPANCY ON VOLUME'RFF+ABT:II0K219629I'DTM+160:20101216:102'DTM+9:201012161700:203'DTM+273:2010111120101125:718'DTM+182:20101111:102'FTX+CUS+++107110000000MF:103106000000MF:121107000000DF:119105000000MF:163900000000MF'FTX+CUS+++107104000000MF:107106000000MF:103108000000MF:122107000000DF:150103000000MF'FTX+CUS+++107109000000MF:147109000000MF:147104000000MF:147107000000MF:147112000000MF'FTX+CUS+++105112000000MF:105104000000MF:150108000000MF:103110000000MF:150111000000MF'FTX+CUS+++147102000000MF:105103000000MF:107102000000MF:119104000000MF:103104000000MF'FTX+CUS+++119103000000MF:150107000000MF:119102000000MF:147101000000MF:147111000000MF'FTX+CUS+++105101000000MF:103111000000MF:105110000000MF:103101000000MF:105111000000MF'FTX+CUS+++150105000000MF:147108000000MF:150106000000MF:105108000000MF:105107000000MF'FTX+CUS+++147103000000MF:105102000000MF:150112000000MF:105109000000MF:150104000000MF'FTX+CUS+++147106000000MF:105105000000MF:164900000000MF:119107000000MF:103109000000MF'FTX+CUS+++103103000000MF:150102000000MF:103102000000MF:150101000000MF:107107000000MF'FTX+CUS+++150110000000MF:107111000000MF:107103000000MF:107101000000MF:103107000000MF'FTX+CUS+++120107000000MF:150109000000MF:107108000000MF:103112000000MF:147105000000MF'FTX+CUS+++147110000000MF:107105000000MF:105106000000MF:103105000000MF:107112000000MF'RFF+MS:06'RFF+DM:IP10H1335'RFF+AEA:CA'DTM+182:201012161700:203'FTX+REG+++A20- APPROVED BY AVA (PROCESSED FOOD). THIS CCP ALSO SERVES : AS AN AVA PERMIT FOR ITEMS UNDER AVA CONTROL SUBJECT TO  :COMPLIANCE WITH THE SALE OF FOOD ACT AND THE FOOD  :REGULATIONS. PLEASE NOTE THAT MEAT AND SEAFOOD PRODUCTS ARE 'FTX+REG+++ NOT ALLOWED TO DECLARE UNDER THIS PERMIT. FAILURE TO :COMPLY  MAY BE SUBJECTED TO A FINE NOT EXCEEDING $10,000 :AND/OR  IMPRISONMENT NOT EXCEEDING 3 MONTHS.:FOR ALL ITEMS.'RFF+AEA:SC'FTX+CCI+++Z20 - AMENDMENT APPROVED BY SINGAPORE CUSTOMS ON CONDITION :THAT THE EARLIER APPROVED PERMIT HAS NOT BEEN USED AND THIS:SUPERCEDES THE PREVIOUS PERMIT.'FTX+CCI+++Z10 - APPROVED BY SINGAPORE CUSTOMS SUBJECT TO THE:CONDITIONTHAT YOU COMPLY WITH THE REQUIREMENTS OF THE:COMPETENT      AUTHORITY.'FTX+CCI+++Y95 - PLS CHECK AGAIN THE DECLARED - 1) HS :CODES\DESCRIPTION, OR 2) ITEM QUANTITY OR VALUE, OR 3) ITEM :VALUE WHICH EXCEEDED $1 MILLION. IF WRONG, PLEASE CANCEL :THIS CARGO CLEARANCE PERMIT WITHIN 48 HOURS.'FTX+CCI+++GA APPROVED BY CUSTOMS SUBJECT TO THE DECLARANT COMPLYING:WITH THE FOLLOWING CONDITION(S) FOR THE PERMIT TO BE VALID.:FAILURE TO COMPLY WITH THE CONDITION(S) IS AN OFFENCE.'FTX+CCI+++P1 THE CONTAINER(S) MUST BE PRODUCED AT CUSTOMS :CHECKPOINT(S) FOR CLEARANCE UNLESS DIRECTED TO ?'GREEN?' :LANE. FOR CONTAINER(S) TO BE SCANNED, PLEASE PRODUCE FOR :SCANNING BY ICA AT SCANNING STATION AS DIRECTED.'FTX+CCI+++AY GOODS NOT EXPORTED/TRANSHIPPED OR BONDED IN A  :BONDED/LICENSED WAREHOUSE OR RECEIVED BY THE CLAIMANT ON :THE SAME DAY OF REMOVAL MUST BE STORED AT A PLACE APPROVED :BY A PROPER OFFICER OF CUSTOMS.'FTX+CCI+++A6 IF THE PERMIT IS NOT USED, IT MUST BE CANCELLED OR:RE-VALIDATED NOT LATER THAN 24 HOURS OF ITS EXPIRY.'FTX+CCI+++B2 AN E-FILE APPLICATION MUST BE MADE TO COMPANY COMPLIANCE :BRANCH FOR SUPERVISION OF UNSTUFFING OF CONTAINERS. :(WEBSITE?: HTTP?://WWW.CUSTOMS.GOV.SG)'FTX+CCI+++A9 THE GOODS MUST BE PRODUCED TO THE CHECKPOINT IN A:BONDED TRUCK/CONTAINER FOR SEALING. THE TRUCK/CONTAINER:MUST BE ABLE TO BE SECURED TO THE SATISFACTION OF THE:PROPER OFFICER OF CUSTOMS'FTX+CCI+++EEE - END OF CARGO CLEARANCE PERMIT.'TDT+3++1+++++0018E:::YM KEELUNG'DOC+704+YMLUM968002038'NAD+DT++JOANNE TAN'CTA+IC+:S1675235E'COM+62600118:TE'NAD+AE+198703865Z++AIR MARKET EXPRESS (S) PTE LTD'NAD+IM+201009758D++EPHRAIM COMMERCE ADVISORY PTE LTD'NAD+CG+199907099D++YANGMING SHIPPING (SINGAPORE) PTE:LTD'NAD+FW+199907099D++YANGMING SHIPPING (SINGAPORE) PTE:LTD'UNS+D'DMS+INVOICE DETAILS'MOA+39:11616.49:USD'CUX+++1.291000'TOD+++CFR'NAD+SE'DOC+380+INVOICE'DTM+3:20101023:102'ALC+C'MOA+70:116.15:USD'PCD+5:1.000'CUX+++1.291000'CST+1+22042111'FTX+AAA+++NIVA WHITE SEMISWEET SPARKLING WINE:- 11.5%'FTX+PRD+++UNBRANDED'LOC+27+UA'LOC+18+AMLA0580'MEA+AAF++LTR:720.0000'MEA+AAE++LTR:720.0000'MEA+AAI++LTR:0.7500'MEA+ABA++LTR:720.9600'MEA+AAG++LPA:11.500'PAC+80+3+CTN'PAC+12+2+BOT'MOA+63:1176.64'MOA+41:0.00'RFF+IV:INVOICE'RFF+AEA:ZBP0BJ0QVDG'DOC+703+YMLUM968002038'TAX+7++++:::7'MOA+124:488.08'TAX+5++++LPA:::70.0000'MOA+161:5796.00'CST+2+22042111'FTX+AAA+++NIVA RED SEMISWEET SPARKLING WINE -:11.5%'FTX+PRD+++UNBRANDED'LOC+27+UA'LOC+18+AMLA0581'MEA+AAF++LTR:2106.0000'MEA+AAE++LTR:2106.0000'MEA+AAI++LTR:0.7500'MEA+ABA++LTR:2162.8800'MEA+AAG++LPA:11.500'PAC+234+3+CTN'PAC+12+2+BOT'MOA+63:3800.33'MOA+41:0.00'RFF+IV:INVOICE'RFF+AEA:ZBP0BH0QVDG'DOC+703+YMLUM968002038'TAX+7++++:::7'MOA+124:1452.75'TAX+5++++LPA:::70.0000'MOA+161:16953.30'CST+3+22042111'FTX+AAA+++NIVA CHARDONNAY FINE DRY WHITE WINE:- 11.5%'FTX+PRD+++UNBRANDED'LOC+27+UA'LOC+18+AMLA0582'MEA+AAF++LTR:428.4000'MEA+AAE++LTR:428.4000'MEA+AAI++LTR:0.7000'MEA+ABA++LTR:426.0000'MEA+AAG++LPA:11.500'PAC+51+3+CTN'PAC+12+2+BOT'MOA+63:585.20'MOA+41:0.00'RFF+IV:INVOICE'RFF+AEA:ZBP0BJ0QVDG'DOC+703+YMLUM968002038'TAX+7++++:::7'MOA+124:282.37'TAX+5++++LPA:::70.0000'MOA+161:3448.62'CST+4+22042111'FTX+AAA+++NIVA CABERNET FINE DRY RED WINE -:12%'FTX+PRD+++UNBRANDED'LOC+27+UA'LOC+18+AMLA0583'MEA+AAF++LTR:848.4000'MEA+AAE++LTR:848.4000'MEA+AAI++LTR:0.7000'MEA+ABA++LTR:852.0000'MEA+AAG++LPA:12.000'PAC+101+3+CTN'PAC+12+2+BOT'MOA+63:1198.55'MOA+41:0.00'RFF+IV:INVOICE'RFF+AEA:ZBP0BH0QVDG'DOC+703+YMLUM968002038'TAX+7++++:::7'MOA+124:582.76'TAX+5++++LPA:::70.0000'MOA+161:7126.56'CST+5+22042111'FTX+AAA+++NIVA PRINCESS OF THE NIGHT:SEMISWEET RED WINE - 12%'FTX+PRD+++UNBRANDED'LOC+27+UA'LOC+18+AMLA0584'MEA+AAF++LTR:999.6000'MEA+AAE++LTR:999.6000'MEA+AAI++LTR:0.7000'MEA+ABA++LTR:1022.4000'MEA+AAG++LPA:12.000'PAC+119+3+CTN'PAC+12+2+BOT'MOA+63:1370.67'MOA+41:0.00'RFF+IV:INVOICE'RFF+AEA:ZBP0BH0QVDG'DOC+703+YMLUM968002038'TAX+7++++:::7'MOA+124:683.71'TAX+5++++LPA:::70.0000'MOA+161:8396.64'CST+6+22042111'FTX+AAA+++NIVA BULL?'S BLOOD SEMISWEET RED:WINE - 12%'FTX+PRD+++UNBRANDED'LOC+27+UA'LOC+18+AMLA0585'MEA+AAF++LTR:2520.0000'MEA+AAE++LTR:2520.0000'MEA+AAI++LTR:0.7000'MEA+ABA++LTR:2556.0000'MEA+AAG++LPA:12.000'PAC+300+3+CTN'PAC+12+2+BOT'MOA+63:3426.68'MOA+41:0.00'RFF+IV:INVOICE'RFF+AEA:ZBP0BH0QVDG'DOC+703+YMLUM968002038'TAX+7++++:::7'MOA+124:1721.63'TAX+5++++LPA:::70.0000'MOA+161:21168.00'CST+7+22042111'FTX+AAA+++NIVA TAMYANKA PREMIUM SEMISWEET:WHITE WITE - 11.5%'FTX+PRD+++UNBRANDED'LOC+27+UA'LOC+18+AMLA0586'MEA+AAF++LTR:2518.6000'MEA+AAE++LTR:2518.6000'MEA+AAI++LTR:0.7000'MEA+ABA++LTR:2556.0000'MEA+AAG++LPA:11.500'PAC+3598+3+BOT'MOA+63:3426.68'MOA+41:0.00'RFF+IV:INVOICE'RFF+AEA:ZBP0BJ0QVDG'DOC+703+YMLUM968002038'TAX+7++++:::7'MOA+124:1659.10'TAX+5++++LPA:::70.0000'MOA+161:20274.73'CST+8+22042112'FTX+AAA+++UKRAINIAN KAGOR DESSERT RED WINE -:16%'FTX+PRD+++UNBRANDED'LOC+27+UA'LOC+18+AMLA0587'MEA+AAF++LTR:16.8000'MEA+AAE++LTR:16.8000'MEA+AAI++LTR:0.7000'MEA+ABA++LTR:17.0400'MEA+AAG++LPA:16.000'PAC+2+3+CTN'PAC+12+2+BOT'MOA+63:35.33'MOA+41:0.00'RFF+IV:INVOICE'RFF+AEA:ZBP0BH0QVDH'DOC+703+YMLUM968002038'TAX+7++++:::7'MOA+124:15.64'TAX+5++++LPA:::70.0000'MOA+161:188.16'CST+9+22042112'FTX+AAA+++THE SUN IN THE WINEGLASS DESSERT:WHITE WINE - 16%'FTX+PRD+++UNBRANDED'LOC+27+UA'LOC+18+AMLA0588'MEA+AAF++LTR:16.8000'MEA+AAE++LTR:16.8000'MEA+AAI++LTR:0.7000'MEA+ABA++LTR:17.0400'MEA+AAG++LPA:16.000'PAC+2+3+CTN'PAC+12+2+BOT'MOA+63:34.02'MOA+41:0.00'RFF+IV:INVOICE'RFF+AEA:ZBP0BJ0QVDH'DOC+703+YMLUM968002038'TAX+7++++:::7'MOA+124:15.55'TAX+5++++LPA:::70.0000'MOA+161:188.16'CST+10+22042112'FTX+AAA+++LIDIYA DESSERT ROSE WINE - 16%'FTX+PRD+++UNBRANDED'LOC+27+UA'LOC+18+AMLA0589'MEA+AAF++LTR:16.8000'MEA+AAE++LTR:16.8000'MEA+AAI++LTR:0.7000'MEA+ABA++LTR:17.0400'MEA+AAG++LPA:16.000'PAC+2+3+CTN'PAC+12+2+BOT'MOA+63:34.39'MOA+41:0.00'RFF+IV:INVOICE'RFF+AEA:ZBP0BH0QVDH'DOC+703+YMLUM968002038'TAX+7++++:::7'MOA+124:15.58'TAX+5++++LPA:::70.0000'MOA+161:188.16'CST+11+22042112'FTX+AAA+++IZABELLA DESSERT RED WINE - 16%'FTX+PRD+++UNBRANDED'LOC+27+UA'LOC+18+AMLA0590'MEA+AAF++LTR:16.8000'MEA+AAE++LTR:16.8000'MEA+AAI++LTR:0.7000'MEA+ABA++LTR:17.0400'MEA+AAG++LPA:16.000'PAC+2+3+CTN'PAC+12+2+BOT'MOA+63:34.39'MOA+41:0.00'RFF+IV:INVOICE'RFF+AEA:ZBP0BH0QVDH'DOC+703+YMLUM968002038'TAX+7++++:::7'MOA+124:15.58'TAX+5++++LPA:::70.0000'MOA+161:188.16'CST+12+22042111'FTX+AAA+++MERLOT FINE DRY RED WINE - 12%'FTX+PRD+++UNBRANDED'LOC+27+UA'LOC+18+AMLA0591'MEA+AAF++LTR:16.8000'MEA+AAE++LTR:16.8000'MEA+AAI++LTR:0.7000'MEA+ABA++LTR:17.0400'MEA+AAG++LPA:12.000'PAC+2+3+CTN'PAC+12+2+BOT'MOA+63:23.96'MOA+41:0.00'RFF+IV:INVOICE'RFF+AEA:ZBP0BH0QVDG'DOC+703+YMLUM968002038'TAX+7++++:::7'MOA+124:11.56'TAX+5++++LPA:::70.0000'MOA+161:141.12'UNS+S'CNT+5:12'CNT+6:1'CNT+22:70'TAX+1'MOA+63:15146.84'TAX+7'MOA+124:6944.31'TAX+5'MOA+161:84057.61'UNT+333+CWISEB00004063'";
		}

		void SetNormalPermitMessageText(EDIMessage message)
		{
			message.EM_MessageText = "UNH+1+CUSPMT:0:1:RT:040+INPPMT'BGM+962:::APS+XXXXXXXXE01T        200609130001+11'CST++4'LOC+9+USTXT'LOC+11+CZ:::CHANGI FTZ'LOC+88+CZ:::CHANGI FTZ'DTM+178:20060912:102'GEI+5+:Y'MEA+ABK++PKG:6.0000'MEA+AAH++KGM:612.0000'FTX+AAI+++INV?:94139979/94139978/94138259/ 94137934/94130844/94130846 94137934/94130844/94130846'RFF+MR:E05T.E05T001'RFF+MR:E03T.E03T001'RFF+ACE:ME6B000001Z'RFF+ABT:ME6I309961C'DTM+9:200609130015:203'DTM+160:20060913:102'DTM+273:2006091320060926:718'RFF+AEA:SC'FTX+CCI+++Z01 - APPROVED BY SINGAPORE CUSTOMS.'FTX+CCI+++GA APPROVED BY CUSTOMS SUBJECT TO THE DECLARANT COMPLYING:WITH THE FOLLOWING CONDITION(S) FOR THE PERMIT TO BE VALID.:FAILURE TO COMPLY WITH THE CONDITION(S) IS AN OFFENCE.'FTX+CCI+++MA THE GOODS AND THIS PERMIT MUST BE PRODUCED FOR CUSTOMS:CLEARANCE AT A FREE TRADE ZONE \"OUT\" GATE, WOODLANDS/TUAS:CHECKPOINT OR WOODLANDS TRAIN CHECKPOINT UNLESS IT IS:DIRECTED TO THE \"GREEN LANE\" AT THE TIME OF CLEARANCE.'FTX+CCI+++A6 IF THE PERMIT IS NOT USED, IT MUST BE CANCELLED OR:RE-VALIDATED NOT LATER THAN 24 HOURS OF ITS EXPIRY.'FTX+CCI+++GK YOU ARE ACCOUNTABLE TO THE INLAND REVENUE AUTHORITY OF:SINGAPORE FOR THE GST PAYABLE ON THE DECLARED GOODS.'FTX+CCI+++   ********** END OF CARGO CLEARANCE PERMIT **********'TDT+3++4+++++BR 6207'DOC+704+69578143170'NAD+AE+XXXXXXXXE01T++TESTING1 INTERNET'NAD+CG+11770720000D++SINGAPORE AIRPORT TERMINAL SERVICES'NAD+IM+XXXXXXXXE01T++TESTING1 INTERNET'NAD+BB'RFF+DAN:D'NAD+DT++TESTING1'CTA+IC+:2666666Z'COM+63111111:TE'NAD+FW+XXXXXXXXE01T++TESTING1 INTERNET'UNS+D'DMS+INVOICE DETAILS'MOA+39:251550.29:USD'CUX+++1.571000'TOD+++CIF'NAD+SU++.+1'DOC+380+.'DTM+3:20060912:102'ALC+C'MOA+64:1014.20:USD'CUX+++1.571000'ALC+C'MOA+70:3967.79:SGD'PCD+5:1.000'CST+1+84314390'FTX+AAA+++OILWELL EQUIPMENT SPARE PARTS.'FTX+PRD+++UNBRANDED:NA'LOC+27+US'MEA+AAF++LOT:1.0000'MOA+63:400746.60'MOA+41:400746.60'MOA+146:400746.60:SGD'RFF+IV:.'DOC+703+2TW4279'TAX+7++++:::5'MOA+124:20037.33'UNS+S'CNT+5:1'TAX+1'MOA+63:400746.60'TAX+7'MOA+124:20037.33'TAX+2'MOA+9:20037.33'UNT+71+1'";
		}

		JobDeclaration DeclarationWithRefundPermit
		{
			get
			{
				if (declarationWithRefundPermit == null)
				{
					declarationWithRefundPermit = Factory.New<JobDeclaration>();
					PopulateDeclaration(declarationWithRefundPermit, true, false);
				}

				return declarationWithRefundPermit;
			}
		}

		JobDeclaration declarationWithRefundPermit;
		JobDeclaration DeclarationWithAmendedPermit
		{
			get
			{
				if (declarationWithAmendedPermit == null)
				{
					declarationWithAmendedPermit = Factory.New<JobDeclaration>();
					PopulateDeclaration(declarationWithAmendedPermit, false, true);
				}

				return declarationWithAmendedPermit;
			}
		}

		JobDeclaration declarationWithAmendedPermit;
		#endregion
		#region Document Supporter
		JobDeclarationDocumentSupporter DocumentSupporterWithMessages
		{
			get
			{
				return documentSupporterWithMessages ?? (documentSupporterWithMessages = new JobDeclarationDocumentSupporter(Declaration));
			}
		}

		JobDeclarationDocumentSupporter documentSupporterWithMessages;
		JobDeclarationDocumentSupporter DocumentSupporterWithRefundPermit
		{
			get
			{
				return documentSupporterWithRefundPermit ?? (documentSupporterWithRefundPermit = new JobDeclarationDocumentSupporter(DeclarationWithRefundPermit));
			}
		}

		JobDeclarationDocumentSupporter documentSupporterWithRefundPermit;
		JobDeclarationDocumentSupporter DocumentSupporterWithoutMessages
		{
			get
			{
				return documentSupporterWithoutMessages ?? (documentSupporterWithoutMessages = new JobDeclarationDocumentSupporter(Factory.New<JobDeclaration>()));
			}
		}

		JobDeclarationDocumentSupporter documentSupporterWithoutMessages;
		JobDeclarationDocumentSupporter DocumentSupporterWithAmendedPermit
		{
			get
			{
				return documentSupporterWithAmendedPermit ?? (documentSupporterWithAmendedPermit = new JobDeclarationDocumentSupporter(DeclarationWithAmendedPermit));
			}
		}

		JobDeclarationDocumentSupporter documentSupporterWithAmendedPermit;
		#endregion
	}
}
