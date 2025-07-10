using CargoWise.Types;
using Enterprise.Customs.SG.Business.CustomsMessaging.D09B;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging;
using Enterprise.Customs.SG.V4.Business.PermitPrinting;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Edifact;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(DocSGPrintPermit))]
	sealed class DocSGPrintPermitTest : DocumentWrapperTestCase
	{
		public void TestHumanReadableName()
		{
			AssertEquals("Cargo Clearance Permit", docSGPrintPermit.HumanReadableName);
		}

		public void TestDefaultStringValueUsedInEmailSubjectLine()
		{
			AssertEquals("CCP", docSGPrintPermit.ToString());
		}

		public void TestDeclarationType()
		{
			AssertEquals("DIRECT (INCLUDING STORAGE IN FTZ)", docSGPrintPermit.DeclarationType);
		}

		public void TestArrivalDate()
		{
			AssertEquals("01/08/2007", docSGPrintPermit.ArrivalDate);
		}

		public void TestPermitNumber()
		{
			AssertEquals("OT7H000194M", docSGPrintPermit.PermitNumber);
		}

		public void TestUniqueRef()
		{
			AssertEquals("TradeNet 4.0 version message", "12247970000Z         20070801 1421", docSGPrintPermit.UniqueRef);
			AssertEquals("TradeNet 4.1 version message", "XXXXXXXXE01T 20110214 0001", tn4_1DocPermit.UniqueRef);
		}

		public void TestLongOblMawbValues()
		{
			AssertEquals("Inward OBL / MAWB with too many characters prints in different location ", "", tn4_1TNPDocPermit.InOBLMawbNb);
			AssertEquals("Inward OBL / MAWB with too many characters prints in different location ", "OB12345678901234567890123456", tn4_1TNPDocPermit.LongInOBLMawbNb);
			AssertEquals("Outward OBL / MAWB with too many characters prints in different location ", "", tn4_1TNPDocPermit.OutOBLMawbNb);
			AssertEquals("Outward OBL / MAWB with too many characters prints in different location ", "MMMMMMMMMMMMMWWWWWWWWWWMMMMMMMMMMMW", tn4_1TNPDocPermit.LongOutOBLMawbNb);
		}

		#region Page 1

		public void TestMessageType()
		{
			AssertEquals("OUT (WITH OR WITHOUT CERTIFICATE OF ORIGIN) PERMIT", docSGPrintPermit.MessageType);
		}

		public void TestImporter()
		{
			AssertEquals("", docSGPrintPermit.Importer);
		}

		public void TestExporter()
		{
			AssertEquals("COLD STORAGE SINGAPORE (1983) PTE LTD\n10005470000Z", docSGPrintPermit.Exporter);
		}

		public void TestHandlingAgent()
		{
			AssertEquals("", docSGPrintPermit.HandlingAgent);
		}

		public void TestPortOfLoading()
		{
			AssertEquals("SINGAPORE", docSGPrintPermit.PortOfLoading);
		}

		public void TestNextPortOfCall()
		{
			AssertEquals("FUJAIRAH", docSGPrintPermit.NextPortOfCall);
		}

		public void TestPortOfLoadingPortOfCall()
		{
			var key = $"{EntryHeader.PK}IsSeaStoreDeclaration";

			EntryHeader.Declaration.SG_IsSeaStore = true;
			Factory.ClearCachedValue<ZBool>(key);

			AssertEquals("FUJAIRAH", docSGPrintPermit.PortOfLoadingPortOfCall);

			EntryHeader.Declaration.SG_IsSeaStore = false;
			Factory.ClearCachedValue<ZBool>(key);

			AssertEquals("SINGAPORE", docSGPrintPermit.PortOfLoadingPortOfCall);
		}

		public void TestPortOfDischarge()
		{
			AssertEquals("JAKARTA, JAVA", docSGPrintPermit.PortOfDischarge);
		}

		public void TestFinalPortOfCall()
		{
			AssertEquals("HONG KONG", docSGPrintPermit.FinalPortOfCall);
		}

		public void TestPortOfDischargeFinalPortOfCall()
		{
			var key = $"{EntryHeader.PK}IsSeaStoreDeclaration";

			EntryHeader.Declaration.SG_IsSeaStore = true;
			Factory.ClearCachedValue<ZBool>(key);

			AssertEquals("HONG KONG", docSGPrintPermit.PortOfDischargeFinalPortOfCall);

			EntryHeader.Declaration.SG_IsSeaStore = false;
			Factory.ClearCachedValue<ZBool>(key);

			AssertEquals("JAKARTA, JAVA", docSGPrintPermit.PortOfDischargeFinalPortOfCall);
		}

		public void TestCountryOfFinalDest()
		{
			AssertEquals("INDONESIA", docSGPrintPermit.CountryOfFinalDest);
		}

		public void TestInwardCarrierAgent()
		{
			AssertEquals("AB SHIPPING PTE LTD", docSGPrintPermit.InwardCarrierAgent);
		}

		public void TestOutwardCarrierAgent()
		{
			AssertEquals("AB SHIPPING PTE LTD", docSGPrintPermit.OutwardCarrierAgent);
		}

		public void TestPlaceOfRelease()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities;
			helper.CreateCusCodeType(codeType, "Facilities");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("SGCType", "Desc.", codeType, Core.Constants.CountryCodes.Singapore, codeType);

			var place1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Singapore, codeType, "JZ", "JURONG FTZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var place2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Singapore, codeType, "CZ", "CHANGI FTZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateCusCodeListAttribute(place1.PK, "SGCType", "JZ");
			helper.CreateCusCodeListAttribute(place2.PK, "SGCType", "CZ");

			Factory.Save();

			AssertEquals("JURONG FTZ\nJZ", docSGPrintPermit.PlaceOfRelease);
			AssertEquals("", docSGPrintPermit.PlaceOfReleaseCode);
			AssertEquals("", docSGPrintPermit.PlaceOfReleaseName);

			AssertEquals("", tn4_1DocPermit.PlaceOfRelease);
			AssertEquals("CZ", tn4_1DocPermit.PlaceOfReleaseCode);
			AssertEquals("", tn4_1DocPermit.PlaceOfReleaseName);
		}

		public void TestValidityPeriodFrom()
		{
			AssertEquals("01/08/2007", docSGPrintPermit.ValidityPeriodFrom);
		}

		public void TestValidityPeriodTo()
		{
			AssertEquals("15/08/2007", docSGPrintPermit.ValidityPeriodTo);
		}

		public void TestTotalGrossWt()
		{
			AssertEquals("0.2200/TNE", docSGPrintPermit.TotalGrossWt);
		}

		public void TestTotalOuterPack()
		{
			AssertEquals("1.0000/PKG", docSGPrintPermit.TotalOuterPack);
		}

		public void TestTotalCustomsDUTPayable()
		{
			AssertEquals("0.00", docSGPrintPermit.TotalCustomsDUTPayable);
		}

		public void TestTotalExciseDUTPayable()
		{
			AssertEquals("0.00", docSGPrintPermit.TotalExciseDUTPayable);
		}

		public void TestTotalGstAmount()
		{
			AssertEquals("0.00", docSGPrintPermit.TotalGstAmount);
		}

		public void TestTotalAmountPayable()
		{
			AssertEquals("0.00", docSGPrintPermit.TotalAmountPayable);
		}

		public void TestCargoPackingType()
		{
			AssertEquals("CONTAINER", docSGPrintPermit.CargoPackingType);
		}

		public void TestInVesName()
		{
			AssertEquals("MARS", docSGPrintPermit.InVesName);
		}

		public void TestInVoyageFlightNumber()
		{
			AssertEquals("VOY123", docSGPrintPermit.InVoyageFlightNumber);
		}

		public void TestInOBLMawbNb()
		{
			AssertEquals("OB939488", docSGPrintPermit.InOBLMawbNb);
			AssertEquals("", docSGPrintPermit.LongInOBLMawbNb);
		}

		public void TestOutVesName()
		{
			AssertEquals("VENUS", docSGPrintPermit.OutVesName);
		}

		public void TestOutVesLocation()
		{
			AssertEquals("PPW", docSGPrintPermit.OutVesLocation);
		}

		public void TestOutVoyageFlightNumber()
		{
			AssertEquals("VOY1234", docSGPrintPermit.OutVoyageFlightNumber);
		}

		public void TestTowingVesselName()
		{
			AssertEquals("", docSGPrintPermit.TowingVesselName);
		}

		public void TestOutOBLMawbNb()
		{
			AssertEquals("OUTWARDMB", docSGPrintPermit.OutOBLMawbNb);
			AssertEquals("", docSGPrintPermit.LongOutOBLMawbNb);
		}

		public void TestDepartureDate()
		{
			AssertEquals("01/08/2007", docSGPrintPermit.DepartureDate);
		}

		public void TestLicenceNb()
		{
			AssertEquals(@"12345678901234567890123456789012345
67890123456789012345678901234567890
1234567890", docSGPrintPermit.LicenceNb);
		}

		public void TestCertificateNb()
		{
			AssertEquals("", docSGPrintPermit.CertificateNb);
		}

		public void TestPlaceOfReceipt()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities;
			helper.CreateCusCodeType(codeType, "Facilities");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("SGCType", "Desc.", codeType, Core.Constants.CountryCodes.Singapore, codeType);

			var place1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Singapore, codeType, "PPZ", "PASIR PANJANG FTZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var place2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Singapore, codeType, "CZ", "CHANGI FTZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateCusCodeListAttribute(place1.PK, "SGCType", "CZ");
			helper.CreateCusCodeListAttribute(place2.PK, "SGCType", "PPZ");

			Factory.Save();

			AssertEquals("PASIR PANJANG FTZ\nPPZ", docSGPrintPermit.PlaceOfReceipt);
			AssertEquals("", docSGPrintPermit.PlaceOfReceiptCode);
			AssertEquals("", docSGPrintPermit.PlaceOfReceiptName);

			AssertEquals("", tn4_1DocPermit.PlaceOfReceipt);
			AssertEquals("CZ", tn4_1DocPermit.PlaceOfReceiptCode);
			AssertEquals("", tn4_1DocPermit.PlaceOfReceiptName);
		}

		#endregion

		#region Page2
		public void TestConsignmentDetails()
		{
			AssertEquals(typeof(DocPrintPermitConsignmentDetails), docSGPrintPermit.ConsignmentDetails.TypeOfElements);
		}

		#endregion

		#region Page3
		public void TestTradersRemark()
		{
			AssertEquals(new ZString(""), docSGPrintPermit.TradersRemark);
		}

		public void TestContainerIdentifiers()
		{
			CUSPMT cuspmt = new CUSPMT();
			ZString strSample = "UNH+1+CUSPMT:0:1:RT:040+INPPMT'BGM+962:::SFZ+XXXXXXXXE01T        200609138006+11'CST++3'LOC+9+IDBAP'LOC+11+JZ:::JURONG FTZ'LOC+12+THBKK'LOC+36+TH'LOC+88+KZ:::KEPPEL FTZ'LOC+164+JW:::JURONG WHARVES'LOC+234+KW:::KEPPEL WHARVES'DTM+136:20060923:102'DTM+178:20060913:102'DTM+206:20060923:102'GEI+5+:Y'MEA+ABK++UNT:1.0000'MEA+AAH++TNE:25.5240'MEA+AAN++:18220.00'EQD+CN+NYKU6970773:1+:::FCL40025'SEL+EX232952'FTX+AAI+++SOUTHERN LIGHT'RFF+ABT:IT6I202036Z'DTM+9:200609131222:203'DTM+160:20060913:102'DTM+273:2006091320060923:718'RFF+AEA:SC'FTX+CCI+++Z01 - APPROVED BY SINGAPORE CUSTOMS.'FTX+CCI+++GA APPROVED BY CUSTOMS SUBJECT TO THE DECLARANT COMPLYING:WITH THE FOLLOWING CONDITION(S) FOR THE PERMIT TO BE VALID.:FAILURE TO COMPLY WITH THE CONDITION(S) IS AN OFFENCE.'FTX+CCI+++P1 YOU ARE REQUIRED TO PRODUCE THE CONTAINER(S) AT CUSTOMS:CHECKPOINT(S)FOR CLEARANCE UNLESS DIRECTED TO GREEN LANE.FOR:CONTAINER(S)TO BE SCANNED,PLEASE PRODUCE FOR SCANNING BY ICA:AT TANJONG PAGAR/PASIR PANJANG SCANNING STATION AS DIRECTED.'FTX+CCI+++AX GOODS RELEASED FROM THE 1ST CUSTOMS CHECKPOINT MUST BE:PRODUCED AT THE 2ND CHECKPOINT WITHIN 24 HOURS.  OTHERWISE,:THEY MUST BE STORED AT A PLACE APPROVED BY A PROPER OFFICER:OF CUSTOMS.'FTX+CCI+++A6 IF THE PERMIT IS NOT USED, IT MUST BE CANCELLED OR:RE-VALIDATED NOT LATER THAN 24 HOURS OF ITS EXPIRY.'FTX+CCI+++A9 THE GOODS MUST BE PRODUCED TO THE CHECKPOINT IN A:BONDED TRUCK/CONTAINER FOR SEALING.  THE TRUCK/CONTAINER:MUST BE ABLE TO BE SECURED TO THE SATISFACTION OF THE PROPER:OFFICER OF CUSTOMS'FTX+CCI+++   ********** END OF CARGO CLEARANCE PERMIT **********'TDT+3++1+++++J11SB06:::GOLDEN ANCHOR 1801'TDT+12++1+:::CV++++074N:::ACX IYO'DOC+704+3'DOC+741+NYKASGN92858'NAD+AE+XXXXXXXXE01T++TESTING1 INTERNET'NAD+CG+70581000000M++MARINA LOGISTICS (S) PTE LTD'NAD+CA+12723830000E++NYK LINE (ASIA) PTE LTD:TEL-62950123'NAD+IM+99999990000C++AL CYCLE CO LTD'NAD+BB'RFF+DAN:I'NAD+DT++TESTING1'CTA+IC+:2666666Z'COM+63111111:TE'NAD+CN+++AL CYCLE CO LTD+99/3 MOO 7 T SURASAK'UNS+D'CST+1+76020000'FTX+AAA+++ALUMINIUM CHIP,DROSS,WASTE'FTX+PRD+++UNBRANDED:NA'LOC+27+ID'MEA+AAF++TNE:25.1200'MOA+63:47160.15'MOA+41:47160.15'MOA+146:47160.15:SGD'TAX+7++++:::5'MOA+124:2358.01'UNS+S'CNT+5:1'TAX+7'MOA+124:2358.01'TAX+1'MOA+63:47160.15'TAX+2'MOA+9:2358.01'UNT+66+1'UNZ+1+91432559'";
			cuspmt.Parse(new UNOACharacterSet(), strSample);
			PrintPermit printPermitWithContainers = new PrintPermit(cuspmt, Factory);
			DocSGPrintPermit docPrintPermit = DocSGPrintPermit.New(printPermitWithContainers, Factory);
			AssertEquals(typeof(DocPrintPermitContainers), docPrintPermit.ContainerIdentifiers.TypeOfElements);
			AssertEquals(1, docPrintPermit.ContainerIdentifiers.Count);

			ZString containersMessageThreeContainers = @"UNH+CWISEB00002114+CUSPMT:0:1:RT:040+OUTUPT'BGM+962:::DRT+12247970000Z        200708011421+32'CST++3+AME'LOC+11+JZ'LOC+88+PPZ'LOC+9+SGSIN'LOC+12+IDJKT'LOC+164+JW'LOC+234+PPW'LOC+36+ID'DTM+136:20070801:102'DTM+178:20070801:102'GEI+5+:Y'MEA+ABK++PKG:1.0000'MEA+AAH++TNE:0.2200'MEA+AAN++:35000.00'EQD+CN+ABCD1234:1+:::FCL40012'EQD+CN+EFGH5678:2+:::FCL45012'EQD+CN+EFGH5679:3+:::FCL450012'SEL+SEAL1'SEL+SEAL2'FTX+ACF+++CHANGE OUTWARD AWB'RFF+ABT:OT7H000194M'DTM+160:20070801:102'DTM+9:200708011458:203'DTM+273:2007080120070815:718'DTM+182:20070801:102'FTX+CUS+++028000000000MF'RFF+AEA:SC'FTX+CCI+++Y99 - SPECIMEN PERMIT ONLY'FTX+CCI+++Z01 - APPROVED BY SINGAPORE CUSTOMS.'FTX+CCI+++Y95 - PLS CHECK AGAIN THE DECLARED - :1) HS CODES\\DESCRIPTION, OR 2) ITEM QUANTITY OR VALUE, : OR 3) ITEM VALUE WHICH EXCEEDED $1 MILLION. IF WRONG, : PLEASE CANCEL THIS CCP WITHIN 48 HOURS.'FTX+CCI+++Z20 - AMENDMENT APPROVED BY SINGAPORE CUSTOMS ON CONDITION:THAT THE EARLIER APPROVED PERMIT HAS NOT BEEN USED AND:THIS SUPERCEDES THE PREVIOUS PERMIT.'FTX+CCI+++GA APPROVED BY CUSTOMS SUBJECT TO THE DECLARANT COMPLYING:WITH THE FOLLOWING CONDITION(S) FOR THE PERMIT TO BE VALID.:FAILURE TO COMPLY WITH THE CONDITION(S) IS AN OFFENCE.'FTX+CCI+++P1 YOU ARE REQUIRED TO PRODUCE THE CONTAINER(S) AT:CUSTOMS CHECKPOINT(S)FOR CLEARANCE UNLESS DIRECTED:TO<GREEN>LANE.FOR CONTAINER(S)TO BE SCANNED,PLEASE PRODUCE:FOR SCANNING BY ICA AT TANJONG PAGAR/PASIR PANJANG SCANNING'FTX+CCI+++STATION AS DIRECTED.'FTX+CCI+++AX GOODS RELEASED FROM THE 1ST CUSTOMS CHECKPOINT MUST BE:PRODUCED AT THE 2ND CHECKPOINT WITHIN 24 HOURS. OTHERWISE,:THEY MUST BE STORED AT A PLACE APPROVED BY A PROPER OFFICER:OF CUSTOMS.'FTX+CCI+++A6 IF THE PERMIT IS NOT USED, IT MUST BE CANCELLED OR:RE-VALIDATED NOT LATER THAN 24 HOURS OF ITS EXPIRY.'FTX+CCI+++EEE - END OF CARGO CLEARANCE PERMIT.'TDT+3++1+++++VOY123:::MARS'TDT+12++1+:::CV++++VOY1234:::VENUS'DOC+704+OB939488'DOC+741+OUTWARDMB'NAD+DT++GARY O?'DEA'CTA+IC+:P213111'COM+?+61 2 8001 2206:TE'NAD+AE+12247970000Z++EDI DEMONSTRATION SYSTEM SG'NAD+EX+10005470000Z++COLD STORAGE SINGAPORE (1983) PTE L:TD+SINGAPORE SINGAPORE'NAD+CG+11306920000Z++AB SHIPPING PTE LTD'NAD+CA+11306920000Z++AB SHIPPING PTE LTD'NAD+BB'RFF+DAN:D'UNS+D'CST+1+95030091'FTX+AAA+++TOY TYPEWRITERS'LOC+27+CN'MEA+AAF++NMB:100.0000'MOA+63:225900.00'CST+2+95030092'FTX+AAA+++SKIPPING ROPES'LOC+27+CN'MEA+AAF++NMB:550.0000'MOA+63:49950.00'UNS+S'CNT+5:2'CNT+6:1'CNT+22:1'TAX+1'MOA+63:275850.00'UNT+69+CWISEB00002114'";
			cuspmt = new CUSPMT();
			cuspmt.Parse(new UNOACharacterSet(), containersMessageThreeContainers);
			printPermitWithContainers = new PrintPermit(cuspmt, Factory);
			docPrintPermit = DocSGPrintPermit.New(printPermitWithContainers, Factory);
			AssertEquals(typeof(DocPrintPermitContainers), docPrintPermit.ContainerIdentifiers.TypeOfElements);
			AssertEquals(2, docPrintPermit.ContainerIdentifiers.Count);
		}

		public void TestNameOfCompany()
		{
			AssertEquals("EDI DEMONSTRATION SYSTEM SG", docSGPrintPermit.NameOfCompany);
		}

		public void TestEntityIdentOfCompany()
		{
			AssertEquals("12247970000Z", docSGPrintPermit.EntityIdentOfCompany);
		}

		public void TestDeclarantName()
		{
			AssertEquals("GARY O DEA", docSGPrintPermit.DeclarantName);
		}

		public void TestDeclarantCode()
		{
			AssertEquals("P213111", docSGPrintPermit.DeclarantCode);
		}

		public void TestTelNb()
		{
			AssertEquals(" 61 2 8001 2206", docSGPrintPermit.TelNb);
		}

		public void TestCAConditions()
		{
			AssertEquals(0, docSGPrintPermit.CAConditions.Count);
		}

		public void TestCustomsConditions()
		{
			AssertEquals(10, docSGPrintPermit.CustomsConditions.Count);
		}

		public void TestAmendmentDate()
		{
			AssertEquals("01/08/2007", docSGPrintPermit.AmendmentDate);
		}

		public void TestAmendmentFields()
		{
			AssertEquals("OUTWARD OCEAN BILL OF LADING/OCEAN UNIQUE CARGO REFERENCE NUMBER/MAWB (MODIFICATION)" + System.Environment.NewLine + "TOTAL GROSS WEIGHT, QUANTITY (MODIFICATION)" + System.Environment.NewLine + "DECLARANT TELEPHONE NUMBER (MODIFICATION)" + System.Environment.NewLine + "DECLARANT ID (MODIFICATION)" + System.Environment.NewLine, docSGPrintPermit.AmendmentFields);
		}

		public void TestManufacturerName()
		{
			AssertEquals("", docSGPrintPermit.ManufacturerName);
		}

		public void TestCustomsProcedureCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(Core.Constants.CountryCodes.Singapore, "", "771", "", "4000", "1234567890123456789012345678901234567890 (TNP TTF)", "TNP");

			Factory.Save();

			AssertEquals(@"12345678901234567890123456789012345
67890", tn4_1TNPDocPermit.CustomsProcedureCodes);
		}

		#endregion

		#region Implementation

		#region TN4.0 Permit
		DocSGPrintPermit docSGPrintPermit
		{
			get
			{
				if (fDocSGPrintPermit == null)
				{
					fDocSGPrintPermit = DocSGPrintPermit.New(PrintPermit, Factory);
				}
				return fDocSGPrintPermit;
			}
		}
		DocSGPrintPermit fDocSGPrintPermit;

		PrintPermit PrintPermit
		{
			get
			{
				if (fPrintPermit == null)
				{
					CUSPMT cuspmt = new CUSPMT();
					ZString strSample = "UNH+CWISEB00002114+CUSPMT:0:1:RT:040+OUTUPT'BGM+962:::DRT+12247970000Z        200708011421+32'CST++3+AME'LOC+11+JZ'LOC+88+PPZ'LOC+9+SGSIN'LOC+12+IDJKT'LOC+61+AEFJR'LOC+130+HKHKG'LOC+164+JW'LOC+234+PPW'LOC+36+ID'DTM+136:20070801:102'DTM+178:20070801:102'GEI+5+:Y'MEA+ABK++PKG:1.0000'MEA+AAH++TNE:0.2200'MEA+AAN++:35000.00'EQD+CN+ABCD1234:1+:::FCL40012'EQD+CN+EFGH5678:2+:::FCL45012'SEL+NA'SEL+NA'FTX+ACF+++CHANGE OUTWARD AWB'RFF+ABT:OT7H000194M'DTM+160:20070801:102'DTM+9:200708011458:203'DTM+273:2007080120070815:718'DTM+182:20070801:102'RFF+DM:12345678901234567890123456789012345678901234567890123456789012345678901234567890'FTX+CUS+++028000000000MF:018000000000MF:050000000000MF:190000000000MF'RFF+AEA:SC'FTX+CCI+++Y99 - SPECIMEN PERMIT ONLY'FTX+CCI+++Z01 - APPROVED BY SINGAPORE CUSTOMS.'FTX+CCI+++Y95 - PLS CHECK AGAIN THE DECLARED - :1) HS CODES\\DESCRIPTION, OR 2) ITEM QUANTITY OR VALUE, : OR 3) ITEM VALUE WHICH EXCEEDED $1 MILLION. IF WRONG, : PLEASE CANCEL THIS CCP WITHIN 48 HOURS.'FTX+CCI+++Z20 - AMENDMENT APPROVED BY SINGAPORE CUSTOMS ON CONDITION:THAT THE EARLIER APPROVED PERMIT HAS NOT BEEN USED AND:THIS SUPERCEDES THE PREVIOUS PERMIT.'FTX+CCI+++GA APPROVED BY CUSTOMS SUBJECT TO THE DECLARANT COMPLYING:WITH THE FOLLOWING CONDITION(S) FOR THE PERMIT TO BE VALID.:FAILURE TO COMPLY WITH THE CONDITION(S) IS AN OFFENCE.'FTX+CCI+++P1 YOU ARE REQUIRED TO PRODUCE THE CONTAINER(S) AT:CUSTOMS CHECKPOINT(S)FOR CLEARANCE UNLESS DIRECTED:TO<GREEN>LANE.FOR CONTAINER(S)TO BE SCANNED,PLEASE PRODUCE:FOR SCANNING BY ICA AT TANJONG PAGAR/PASIR PANJANG SCANNING'FTX+CCI+++STATION AS DIRECTED.'FTX+CCI+++AX GOODS RELEASED FROM THE 1ST CUSTOMS CHECKPOINT MUST BE:PRODUCED AT THE 2ND CHECKPOINT WITHIN 24 HOURS. OTHERWISE,:THEY MUST BE STORED AT A PLACE APPROVED BY A PROPER OFFICER:OF CUSTOMS.'FTX+CCI+++A6 IF THE PERMIT IS NOT USED, IT MUST BE CANCELLED OR:RE-VALIDATED NOT LATER THAN 24 HOURS OF ITS EXPIRY.'FTX+CCI+++EEE - END OF CARGO CLEARANCE PERMIT.'TDT+3++1+++++VOY123:::MARS'TDT+12++1+:::CV++++VOY1234:::VENUS'DOC+704+OB939488'DOC+741+OUTWARDMB'NAD+DT++GARY O?'DEA'CTA+IC+:P213111'COM+?+61 2 8001 2206:TE'NAD+AE+12247970000Z++EDI DEMONSTRATION SYSTEM SG'NAD+EX+10005470000Z++COLD STORAGE SINGAPORE (1983) PTE L:TD+SINGAPORE SINGAPORE'NAD+CG+11306920000Z++AB SHIPPING PTE LTD'NAD+CA+11306920000Z++AB SHIPPING PTE LTD'NAD+BB'RFF+DAN:D'UNS+D'CST+1+95030091'FTX+AAA+++TOY TYPEWRITERS'LOC+27+CN'MEA+AAF++NMB:100.0000'MOA+63:225900.00'CST+2+95030092'FTX+AAA+++SKIPPING ROPES'LOC+27+CN'MEA+AAF++NMB:550.0000'MOA+63:49950.00'UNS+S'CNT+5:2'CNT+6:1'CNT+22:1'TAX+1'MOA+63:275850.00'UNT+69+CWISEB00002114'";
					cuspmt.Parse(new UNOACharacterSet(), strSample);
					fPrintPermit = new PrintPermit(cuspmt, Factory, EntryHeader.PK);
				}
				return fPrintPermit;
			}
		}
		PrintPermit fPrintPermit;
		#endregion

		#region TN4.1 Permit
		DocSGPrintPermit tn4_1DocPermit
		{
			get
			{
				if (ftn4_1DocPermit == null)
				{
					ftn4_1DocPermit = DocSGPrintPermit.New(tn4_1Permit, Factory);
				}
				return ftn4_1DocPermit;
			}
		}
		DocSGPrintPermit ftn4_1DocPermit;

		PrintPermit tn4_1Permit
		{
			get
			{
				if (ftn4_1Permit == null)
				{
					var cuspmt09B = new Cuspmt09b();
					var tn41msg = "UNH+1+CUSPMT:0:1:RT:041+INPPMT'BGM+962:::APS+XXXXXXXXE01T     201102140001+11'CST++5'LOC+9+USTXT'LOC+11+CZ:::CHANGI FTZ'LOC+88+CZ:::CHANGI FTZ'DTM+178:20110214:102'DTM+416:20110214001500SST:304'GEI+5+:Y'MEA+ABK++PKG:6'MEA+AAH++KGM:612.000'FTX+AAI+++INV?:94139979/94139978/94138259/ 94137934/94130844/94130846 94137934/94130844/94130846'RFF+MR:E05T.E05T001'RFF+MR:E03T.E03T001'RFF+ACE:ME6B000001Z'RFF+ABT:ME6I309961C'DTM+148:20110214001550SST:304'DTM+273:2011021420110222:718'RFF+AEA:SC'FTX+CCI++Z01+Z01 - APPROVED BY SINGAPORE CUSTOMS.'FTX+CCI++GA+GA - APPROVED BY CUSTOMS SUBJECT TO THE DECLARANT COMPLYING WITH THE FOLLOWING CONDITION(S) FOR THE PERMIT TO BE VALID. FAILURE TO COMPLY WITH THE CONDITION(S) IS AN OFFENCE.'FTX+CCI++MA+MA - THE GOODS AND THIS PERMIT MUST BE PRODUCED FOR CUSTOMS CLEARANCE AT A FREE TRADE ZONE \"OUT\" GATE, WOODLANDS/TUAS CHECKPOINT OR WOODLANDS TRAIN CHECKPOINT UNLESS IT IS DIRECTED TO THE \"GREEN LANE\" AT THE TIME OF CLEARANCE.'FTX+CCI++A6+A6 - IF THE PERMIT IS NOT USED, IT MUST BE CANCELLED OR RE-VALIDATED NOT LATER THAN 24 HOURS OF ITS EXPIRY.'FTX+CCI++GK+GK - YOU ARE ACCOUNTABLE TO THE INLAND REVENUE AUTHORITY OF SINGAPORE FOR THE GST PAYABLE ON THE DECLARED GOODS.'FTX+CCI++EEE+EEE - END OF CARGO CLEARANCE PERMIT.'TDT+3+BR 6207+4'DOC+704+69578143170'NAD+AE+XXXXXXXXE01T++TESTING1 INTERNET:::::1'NAD+CG+11770720000D++SINGAPORE AIRPORT TERMINAL SERVICES:::::1'NAD+IM+XXXXXXXXE01T++TESTING1 INTERNET:::::2'NAD+BB'RFF+DAN:D'NAD+DT'CTA+IC+2666666Z:TESTING NAME 1'COM+63111111:TE'NAD+FW+XXXXXXXXE01T++TESTING1 INTERNET:::::1'UNS+D'DMS+INVOICE DETAILS'MOA+39:251550.29:USD'CUX+++1.571000'TOD+++CIF'NAD+SU++.+1'DOC+380+.'DTM+3:20110214:102'ALC+C'MOA+64:1014.20:USD'CUX+++1.571000'ALC+C'MOA+70:3967.79:SGD'PCD+5:1.000'CST+1+84314390'FTX+AAA+++OILWELL EQUIPMENT SPARE PARTS.'FTX+PRD+++BRANDED X:NA'LOC+27+US'MEA+AAF++LOT:1.0000'MOA+63:400746.60'MOA+41:400746.60'MOA+146:400746.60:SGD'RFF+IV:.'DOC+703+2TW4279'TAX+7++++:::7'MOA+369:28052.26'UNS+S'CNT+5:1'TAX+1'MOA+63:400746.60'TAX+7'MOA+369:28052.26'TAX+2'MOA+9:20037.33'UNT+71+1'";
					cuspmt09B.Parse(new UNOACharacterSet(), tn41msg);
					ftn4_1Permit = new PrintPermit(cuspmt09B, Factory);
				}
				return ftn4_1Permit;
			}
		}
		PrintPermit ftn4_1Permit;

		DocSGPrintPermit tn4_1TNPDocPermit
		{
			get
			{
				if (ftn4_1TNPDocPermit == null)
				{
					ftn4_1TNPDocPermit = DocSGPrintPermit.New(tn4_1TNPPermit, Factory);
				}
				return ftn4_1TNPDocPermit;
			}
		}
		DocSGPrintPermit ftn4_1TNPDocPermit;

		PrintPermit tn4_1TNPPermit
		{
			get
			{
				if (ftn4_1TNPPermit == null)
				{
					var cuspmt09B = new Cuspmt09b();
					var tn41msg = "UNH+CWISEB00003041+CUSPMT:0:1:RT:041+TNPPMT'BGM+962:::IGM+199702247W       201110205476+11'CST++5'LOC+11+JZ'LOC+88+CZ'LOC+9+CNSHA'LOC+12+IDJKT'LOC+36+ID'DTM+136:20111020:102'DTM+178:20111020:102'DTM+416:20111020131906SST:304'GEI+5+:Y'MEA+ABK++PKG:5'MEA+AAH++TNE:0.920'FTX+AAI+++VENDOR TESTING OF LONG MAWB / HAWB'RFF+ABT:TT1J001750M'DTM+148:20111020131908SST:304'DTM+273:2011102020111122:718'RFF+AEA:CA'DTM+444:20111020131908SST:304'FTX+REG++Y98+APPLICATION IS APPROVED ON CONDITION THAT DECLARANT/AGENT HAS SATISFIED  HIMSELF/EXERCISED DUE DILIGENCE TO DETERMINE THAT THE GOODS ARE NOT CONTROLLED  UNDER STRATEGIC GOODS (CONTROL) ACT & A STRATEGIC GOODS PERMIT IS NOT REQUIRED. FOR ALL ITEMS.'RFF+AEA:SC'FTX+CCI++Y99+SPECIMEN PERMIT ONLY'FTX+CCI++Z01+APPROVED BY SINGAPORE CUSTOMS.'FTX+CCI++GA+APPROVED BY CUSTOMS SUBJECT TO THE DECLARANT COMPLYING WITH THE FOLLOWINGCONDITION(S) FOR THE PERMIT TO BE VALID. FAILURE TO COMPLY WITH THE CONDITION(S)IS AN OFFENCE.'FTX+CCI++A2+THE GOODS AND THIS PERMIT WITH INVOICES, BL/AWB, ETC MUST BE PRODUCED FORCUSTOMS CLEARANCE/ENDORSEMENT AT A FREE TRADE ZONE ?'OUT?' GATE UNLESS IT IS      DIRECTED TO THE ?'GREEN LANE?' AT THE TIME OF CLEARANCE.'FTX+CCI++AX+GOODS RELEASED FROM THE 1ST CUSTOMS CHECKPOINT MUST BE PRODUCED AT THE   2ND CHECKPOINT WITHIN 24 HOURS. OTHERWISE, THEY MUST BE STORED AT A PLACE       APPROVED BY A PROPER OFFICER OF CUSTOMS.'FTX+CCI++E3+GOODS REMOVED MUST BE SENT DIRECTLY TO THE PLACE OF RECEIPT AS DECLARED  AFTER CUSTOMS EXAMINATIONS. NO OPERATION OF ANY KIND (EG?:                       UNSTUFFING/STUFFING/REPACKING/STORAGE) IS ALLOWED ON THE WAY.'FTX+CCI++AG+I CERTIFY THAT I HAVE ON THE DATE AS INDICATED HEREUNDER RECEIVED THE    CARGO DECLARED. QTY IN PKGS RECD                                                SIGNATURE ...........................                                           .........   .......................                                             DATE TIME   FULL NAME & DESIGNATION'FTX+CCI++A6+IF THE PERMIT IS NOT USED, IT MUST BE CANCELLED OR RE-VALIDATED NOT LATERTHAN 24 HOURS OF ITS EXPIRY.'FTX+CCI++A3+THE GOODS AND THIS PERMIT WITH INVOICES, BL/AWB, ETC MUST BE PRODUCED FORCUSTOMS CLEARANCE/ENDORSEMENT AT AN AIRPORT CUSTOMS CHECKPOINT.'FTX+CCI++EEE+END OF CARGO CLEARANCE PERMIT.'RFF+ABE:7714000'TDT+3+228+1+++++:::JADE TRADER'TDT+12+415E+1+CV++++:::IBN BASSAM'NAD+DT'CTA+IC+P213111:GARY O?'DEA'COM+?+61 2 8001 2206:TE'NAD+AE+199702247W++SG DEMO COMPANY:::::1'NAD+CG+199201306R++AB SHIPPING PTE LTD:::::1'NAD+CA+199201306R++AB SHIPPING PTE LTD:::::1'NAD+FW+199702247W++EAGLE DATAMATION INTERNATIONAL PTE LTD:::::1'NAD+AH+199504953N++ANGLO-EASTERN AGENCY (SINGAPORE) PTE. LTD.:::::1'UNS+D'CST+1+85041000'FTX+AAA+++BALLASTS'FTX+PRD+++UNBRANDED'LOC+27+CN'MEA+AAF++NMB:85.0000'DOC+704+OB12345678901234567890123456'DOC+741+MMMMMMMMMMMMMWWWWWWWWWWMMMMMMMMMMMW'DOC+703+AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA'DOC+714+KKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKKK'CST+2+85042110'FTX+AAA+++STEP VOLTAGE REGULATORS'FTX+PRD+++UNBRANDED'LOC+27+CN'MEA+AAF++NMB:58.0000'UNS+S'CNT+5:2'UNT+64+CWISEB00003041'";
					cuspmt09B.Parse(new UNOACharacterSet(), tn41msg);
					ftn4_1TNPPermit = new PrintPermit(cuspmt09B, Factory, EntryHeader.PK);
				}
				return ftn4_1TNPPermit;
			}
		}
		PrintPermit ftn4_1TNPPermit;

		#endregion

		#region EntryHeader

		CusEntryHeader EntryHeader
		{
			get
			{
				if (entryHeader == null)
				{
					var declaration = Factory.NewWithValidTestData<JobDeclaration>();
					declaration.JE_ApplicationCode = SGConstants.TradeNetVersion.Four;
					declaration.SG_IsSeaStore = false;

					entryHeader = declaration.CustomsEntryHeaders.AddNew();
				}

				return entryHeader;
			}
		}
		CusEntryHeader entryHeader;

		#endregion

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			DocSGPrintPermit result = DocSGPrintPermit.New(PrintPermit, Factory);
			return new DocumentWrapper[] { result };
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return DocSGPrintPermit.New(PrintPermit, Factory);
		}

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Singapore);
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			CreatePortCusCodeList(helper, "IDJKT", "JAKARTA, JAVA");
			CreatePortCusCodeList(helper, "SGSIN", "SINGAPORE");
			CreatePortCusCodeList(helper, "AEFJR", "FUJAIRAH");
			CreatePortCusCodeList(helper, "HKHKG", "HONG KONG");

			Factory.Save();
		}

		void CreatePortCusCodeList(UniversalReferenceTestDataHelper helper, string port, string description)
		{
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Port");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, port, description, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		}

		#endregion
	}
}
