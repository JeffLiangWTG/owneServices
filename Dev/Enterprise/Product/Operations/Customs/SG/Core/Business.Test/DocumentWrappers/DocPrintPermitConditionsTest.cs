using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging;
using Enterprise.Customs.SG.V4.Business.PermitPrinting;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Edifact;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(DocPrintPermitConditions))]
	sealed class DocPrintPermitConditionsTest : DocumentWrapperTestCase
	{
		public void TestCode()
		{
			AssertEquals("Y99", docSGPrintPermitConditions.Code.Trim());
		}

		public void TestMessage()
		{
			AssertEquals("Y99 - SPECIMEN PERMIT ONLY", docSGPrintPermitConditions.Message.Trim());
		}

		#region Implementation

		DocPrintPermitConditions docSGPrintPermitConditions
		{
			get
			{
				if (fDocSGPrintPermitConditions == null)
				{
					fDocSGPrintPermitConditions = DocPrintPermitConditions.New(PrintPermitConditions, Factory);
				}
				return fDocSGPrintPermitConditions;
			}
		}

		DocPrintPermitConditions fDocSGPrintPermitConditions;

		PrintPermitConditions PrintPermitConditions
		{
			get
			{
				if (fPrintPermitConditions == null)
				{
					CUSPMT cuspmt = new CUSPMT();
					ZString strSample = "UNH+CWISEB00002114+CUSPMT:0:1:RT:040+OUTUPT'BGM+962:::DRT+12247970000Z        200708011421+32'CST++3+AME'LOC+11+JZ'LOC+88+PPZ'LOC+9+SGSIN'LOC+12+IDJKT'LOC+164+JW'LOC+234+PPW'LOC+36+ID'DTM+136:20070801:102'DTM+178:20070801:102'GEI+5+:Y'MEA+ABK++PKG:1.0000'MEA+AAH++TNE:0.2200'MEA+AAN++:35000.00'EQD+CN+ABCD1234:1+:::FCL40012'EQD+CN+EFGH5678:2+:::FCL45012'SEL+NA'SEL+NA'FTX+ACF+++CHANGE OUTWARD AWB'RFF+ABT:OT7H000194M'DTM+160:20070801:102'DTM+9:200708011458:203'DTM+273:2007080120070815:718'DTM+182:20070801:102'FTX+CUS+++028000000000MF'RFF+AEA:SC'FTX+CCI+++Y99 - SPECIMEN PERMIT ONLY'FTX+CCI+++Z01 - APPROVED BY SINGAPORE CUSTOMS.'FTX+CCI+++Y95 - PLS CHECK AGAIN THE DECLARED - :1) HS CODES\\DESCRIPTION, OR 2) ITEM QUANTITY OR VALUE, : OR 3) ITEM VALUE WHICH EXCEEDED $1 MILLION. IF WRONG, : PLEASE CANCEL THIS CCP WITHIN 48 HOURS.'FTX+CCI+++Z20 - AMENDMENT APPROVED BY SINGAPORE CUSTOMS ON CONDITION:THAT THE EARLIER APPROVED PERMIT HAS NOT BEEN USED AND:THIS SUPERCEDES THE PREVIOUS PERMIT.'FTX+CCI+++GA APPROVED BY CUSTOMS SUBJECT TO THE DECLARANT COMPLYING:WITH THE FOLLOWING CONDITION(S) FOR THE PERMIT TO BE VALID.:FAILURE TO COMPLY WITH THE CONDITION(S) IS AN OFFENCE.'FTX+CCI+++P1 YOU ARE REQUIRED TO PRODUCE THE CONTAINER(S) AT:CUSTOMS CHECKPOINT(S)FOR CLEARANCE UNLESS DIRECTED:TO<GREEN>LANE.FOR CONTAINER(S)TO BE SCANNED,PLEASE PRODUCE:FOR SCANNING BY ICA AT TANJONG PAGAR/PASIR PANJANG SCANNING'FTX+CCI+++STATION AS DIRECTED.'FTX+CCI+++AX GOODS RELEASED FROM THE 1ST CUSTOMS CHECKPOINT MUST BE:PRODUCED AT THE 2ND CHECKPOINT WITHIN 24 HOURS. OTHERWISE,:THEY MUST BE STORED AT A PLACE APPROVED BY A PROPER OFFICER:OF CUSTOMS.'FTX+CCI+++A6 IF THE PERMIT IS NOT USED, IT MUST BE CANCELLED OR:RE-VALIDATED NOT LATER THAN 24 HOURS OF ITS EXPIRY.'FTX+CCI+++EEE - END OF CARGO CLEARANCE PERMIT.'TDT+3++1+++++VOY123:::MARS'TDT+12++1+:::CV++++VOY1234:::VENUS'DOC+704+OB939488'DOC+741+OUTWARDMB'NAD+DT++GARY O?'DEA'CTA+IC+:P213111'COM+?+61 2 8001 2206:TE'NAD+AE+12247970000Z++EDI DEMONSTRATION SYSTEM SG'NAD+EX+10005470000Z++COLD STORAGE SINGAPORE (1983) PTE L:TD+SINGAPORE SINGAPORE'NAD+CG+11306920000Z++AB SHIPPING PTE LTD'NAD+CA+11306920000Z++AB SHIPPING PTE LTD'NAD+BB'RFF+DAN:D'UNS+D'CST+1+95030091'FTX+AAA+++TOY TYPEWRITERS'LOC+27+CN'MEA+AAF++NMB:100.0000'MOA+63:225900.00'CST+2+95030092'FTX+AAA+++SKIPPING ROPES'LOC+27+CN'MEA+AAF++NMB:550.0000'MOA+63:49950.00'UNS+S'CNT+5:2'CNT+6:1'CNT+22:1'TAX+1'MOA+63:275850.00'UNT+69+CWISEB00002114'";
					cuspmt.Parse(new UNOACharacterSet(), strSample);
					fPrintPermitConditions = new PrintPermitConditions(cuspmt.CustomsConditions[0], Factory);
				}
				return fPrintPermitConditions;
			}
		}
		PrintPermitConditions fPrintPermitConditions;

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			DocPrintPermitConditions result = DocPrintPermitConditions.New(PrintPermitConditions, Factory);
			return new DocumentWrapper[] { result };
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return DocPrintPermitConditions.New(PrintPermitConditions, Factory);
		}

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Singapore);
			base.SetUp();
		}

		#endregion
	}
}
