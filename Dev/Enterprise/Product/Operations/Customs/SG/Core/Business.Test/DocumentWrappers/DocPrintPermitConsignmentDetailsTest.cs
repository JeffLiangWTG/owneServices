using CargoWise.Types;
using Enterprise.Customs.SG.Business.CustomsMessaging.D09B;
using Enterprise.Customs.SG.V4.Business.CustomsMessaging;
using Enterprise.Customs.SG.V4.Business.Messaging.Tradenet;
using Enterprise.Customs.SG.V4.Business.PermitPrinting;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Edifact;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(DocPrintPermitConsignmentDetails))]
	sealed class DocPrintPermitConsignmentDetailsTest : DocumentWrapperTestCase
	{
		#region IPrintPermitConsignment

		public void TestSerialNb()
		{
			AssertEquals("1", docConsignmentDetails.SerialNb);
		}

		public void TestHSCode()
		{
			AssertEquals("1", docConsignmentDetails.SerialNb);
		}

		public void TestBrandName()
		{
			AssertEquals("UNBRANDED", docConsignmentDetails.BrandName);
		}

		public void TestManufacturerName()
		{
			AssertEquals("", docConsignmentDetails.ManufacturerName);
		}

		public void TestHSQuantity()
		{
			AssertEquals("1.0000", docConsignmentDetails.HSQuantity);
		}

		public void TestMarking()
		{
			AssertEquals("", docConsignmentDetails.Marking);
		}

		public void TestCityOfOrigin()
		{
			AssertEquals("CN", docConsignmentDetails.CityOfOrigin);
		}

		public void TestModel()
		{
			AssertEquals("", docConsignmentDetails.Model);
		}

		public void TestDutQuantity()
		{
			AssertEquals("", docConsignmentDetails.DutQuantity);
		}

		public void TestInwardMawbObl()
		{
			AssertEquals("", docConsignmentDetails.InwardMawbObl);
		}

		public void TestInwardHawbHbl()
		{
			AssertEquals("", docConsignmentDetails.InwardHawbHbl);
		}

		public void TestOutwardMawbObl()
		{
			AssertEquals("", docConsignmentDetails.OutwardMawbObl);
		}

		public void TestOutwardHawbHbl()
		{
			AssertEquals("YAS54634064", docConsignmentDetails.OutwardHawbHbl);
		}

		public void TestGoodsDescription()
		{
			AssertEquals("SE300 ULTRA SN# 20006 SOLDER PASTE INSPECTION SYSTEM INCLUDING ACCESSORIES  ", docConsignmentDetails.GoodsDescription);
		}

		public void TestPackingAndGoodsDescWithLongText()
		{
			void AssertPackingAndGoodsDescWithLongText(string longGoodsDesc, string expectedGoodsDesc)
			{
				var printPermitConsignmentDetails = new PrintPermitConsignmentDetails(new BaseTradeNetPermitItem(new Item() { GoodsDescription = longGoodsDesc }, null, false), Factory);
				var docPrintPermitConsignmentDetails = DocPrintPermitConsignmentDetails.New(printPermitConsignmentDetails, Factory);

				var viewResultInDocument = new ZStringBuilder();
				var packingAndGoodsDesc = docPrintPermitConsignmentDetails.PackingAndGoodsDesc;

				while (packingAndGoodsDesc.Length > 0)
				{
					var currentLine = packingAndGoodsDesc.SubstringSafe(0, 50);
					viewResultInDocument.AppendLine(currentLine);

					packingAndGoodsDesc = packingAndGoodsDesc.SubstringSafe(currentLine.Length, packingAndGoodsDesc.Length - 50);
				}

				AssertEquals(@"Item Description: 50 characters per line. Do not cut words and should print in the next line.", expectedGoodsDesc, viewResultInDocument.ToString());
			}

			var longGoodsDescWithSpace = @"BOOT LACES AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA";
			var expectedViewResult = @"BOOT LACES AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA    
AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA      
AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA      
AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA      
AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA      
AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA      
AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA      
AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA      
AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA      
AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA      
AAAAAAAA AAAAA AAAAAAAA AAAAA AAAAAAAA AAAAA      
AAAAAAAA AAAAA                                    
";

			AssertPackingAndGoodsDescWithLongText(longGoodsDescWithSpace, expectedViewResult);

			var longGoodsDescWithoutSpace = @"123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890";
			expectedViewResult = @"12345678901234567890123456789012345678901234567890
12345678901234567890123456789012345678901234567890
12345678901234567890123456789012345678901234567890
12345678901234567890123456789012345678901234567890
12345678901234567890123456789012345678901234567890
12345678901234567890                              
";

			AssertPackingAndGoodsDescWithLongText(longGoodsDescWithoutSpace, expectedViewResult);
		}

		public void TestUnitPrice()
		{
			AssertEquals("0.0000", docConsignmentDetails.UnitPrice);
		}

		public void TestCustomsDutyPayable()
		{
			AssertEquals("0.00", docConsignmentDetails.CustomsDutyPayable);
		}

		public void TestExciseDutyPayable()
		{
			AssertEquals("0.00", docConsignmentDetails.ExciseDutyPayable);
		}

		public void TestCurrentLotNb()
		{
			AssertEquals("", docConsignmentDetails.CurrentLotNb);
		}

		public void TestPreviousLotNb()
		{
			AssertEquals("", docConsignmentDetails.PreviousLotNb);
		}

		public void TestCifFobLspValue()
		{
			AssertEquals("352800.00", docConsignmentDetails.CifFobLspValue);
		}

		public void TestLspAmount()
		{
			AssertEquals("1575.90", docConsignmentDetails.LspAmount);
		}

		public void TestGstAmount()
		{
			AssertEquals("0.00", docConsignmentDetails.GstAmount);
		}

		public void TestCASCProductCode()
		{
			ZString expectedOutput = "1         MISC                                              1.0000  NMB\r\n";
			AssertEquals(expectedOutput, docConsignmentDetails.CASCProductCode);
		}

		public void TestMultipleCASCProductCode()
		{
			ZString expectedOutput = @"1         MISC                                              1.0000  NMB
2         HSAPROCOD                                         5.0000  NMB
";
			AssertEquals(expectedOutput, MultiProductConsignmentDetails.CASCProductCode);
		}

		public void TestCACSProductQty()
		{
			AssertEquals("1.0000", docConsignmentDetails.CASCProductQty);
		}

		public void TestEngineNbChassisNb()
		{
			AssertEquals("", docConsignmentDetails.EngineNbChassisNb);
		}

		public void TestOuterPackQty()
		{
			AssertEquals("", docConsignmentDetails.OuterPackQty);
		}

		public void TestPackingAndGoodsDesc()
		{
			var expectedOutput = @"SE300 ULTRA SN# 20006 SOLDER PASTE INSPECTION     SYSTEM INCLUDING ACCESSORIES                      ";
			AssertEquals("When no packing values, only Goods Description should be returned.", expectedOutput, docConsignmentDetails.PackingAndGoodsDesc);
			expectedOutput = "      13 CAR                                             2 BOT                                      CORTE REAL PLATINUM (3 BOT BOX) PROMOTION ITEM    ";
			AssertEquals("With Packing values; Outer Pack & In Pack values need to be on line 1, (padded length 50 chars), Inner Pack & Inmost Pack on line 2, (padded length 50 chars), with Goods description starting on the line after valid Package values.", expectedOutput, PackageConsignmentDetails.PackingAndGoodsDesc);
		}

		public void TestCustomsLineValues()
		{
			AssertEquals("HSQuantity", "1000.0000", CustomsValueConsignmentDetails.HSQuantity);
			AssertEquals("HSQuantity", "KGM", CustomsValueConsignmentDetails.HSQuantityUnit);
			AssertEquals("CifFobLspValue", "20000.00", CustomsValueConsignmentDetails.CifFobLspValue);
			AssertEquals("LspAmount", "0.00", CustomsValueConsignmentDetails.LspAmount);
			AssertEquals("GstAmount", "26040.00", CustomsValueConsignmentDetails.GstAmount);
			AssertEquals("DutQuantity", "1.0000", CustomsValueConsignmentDetails.DutQuantity);
			AssertEquals("DutQuantityUnit", "KGM", CustomsValueConsignmentDetails.DutQuantityUnit);
			AssertEquals("DutQuantity", "20.0000", CustomsValueConsignmentDetails.UnitPrice);
			AssertEquals("DutQuantityUnit", "SGD", CustomsValueConsignmentDetails.UnitPriceCurrency);
			AssertEquals("ExciseDutyPayable", "352000.00", CustomsValueConsignmentDetails.ExciseDutyPayable);
			AssertEquals("CustomsDutyPayable", "0.00", CustomsValueConsignmentDetails.CustomsDutyPayable);
			AssertEquals("OtherTaxPayable", "0.00", CustomsValueConsignmentDetails.OtherTaxPayable);

			AssertEquals("LineValue1 - should print CifFobLspValue for this permit", "20000.00", CustomsValueConsignmentDetails.LineValue1);
			AssertEquals("LineUnit1 - blank for this permit", "", CustomsValueConsignmentDetails.LineUnit1);
			AssertEquals("LineValue2 - should print GstAmount for this permit", "26040.00", CustomsValueConsignmentDetails.LineValue2);
			AssertEquals("LineUnit2 - blank for this permit", "", CustomsValueConsignmentDetails.LineUnit2);
			AssertEquals("LineValue3 - should print DutQuantity for this permit", "1.0000", CustomsValueConsignmentDetails.LineValue3);
			AssertEquals("LineUnit3 - should print DutQuantityUnit for this permit", "KGM", CustomsValueConsignmentDetails.LineUnit3);
			AssertEquals("LineValue4 - should print UnitPrice for this permit", "20.0000", CustomsValueConsignmentDetails.LineValue4);
			AssertEquals("LineUnit4 - should print UnitPriceCurrency for this permit", "SGD", CustomsValueConsignmentDetails.LineUnit4);
			AssertEquals("LineValue5 - should print ExciseDutyPayable for this permit", "352000.00", CustomsValueConsignmentDetails.LineValue5);
			AssertEquals("LineUnit5 - blank for this permit", "", CustomsValueConsignmentDetails.LineUnit5);
			AssertEquals("LineValue6 - blank for this permit", "", CustomsValueConsignmentDetails.LineValue6);
			AssertEquals("LineValue7 - blank for this permit", "", CustomsValueConsignmentDetails.LineValue7);
			AssertEquals("LineValue8 - blank for this permit", "", CustomsValueConsignmentDetails.LineValue8);
		}

		#endregion

		#region Implementation

		DocPrintPermitConsignmentDetails docConsignmentDetails
		{
			get
			{
				if (fDocConsignmentDetails == null)
				{
					fDocConsignmentDetails = DocPrintPermitConsignmentDetails.New(ConsignmentDetails, Factory);
				}
				return fDocConsignmentDetails;
			}
		}
		DocPrintPermitConsignmentDetails fDocConsignmentDetails;

		PrintPermitConsignmentDetails ConsignmentDetails
		{
			get
			{
				if (fConsignmentDetails == null)
				{
					CUSPMT cuspmt = new CUSPMT();
					ZString strSample = "UNH+1+CUSPMT:0:1:RT:040+OUTPMT'BGM+962:::TCS+XXXXXXXXE47T        200609223517+11'CST++2'LOC+11+O:::OTHERS'LOC+12+TWTPE'LOC+36+TW'LOC+88+CZ:::CHANGI FTZ,CHANGI'DTM+136:20060924:102'GEI+5+:Y'MEA+ABK++CTN:2.0000'MEA+AAH++KGM:976.0000'FTX+AAI+++ITEM ABOVE IS FOR  DEMO PURPOSE.RETURNED AFTER DEMONSTRATION.'RFF+ABT:OO6I001822A'DTM+160:20060922:102'DTM+273:2006092220061005:718'DTM+9:200609221120:203'RFF+AEA:SC'FTX+CCI+++GA APPROVED BY CUSTOMS SUBJECT TO THE DECLARANT COMPLYING:WITH THE FOLLOWING CONDITION(S) FOR THE PERMIT TO BE VALID.:FAILURE TO COMPLY WITH THE CONDITION(S) IS AN OFFENCE.'FTX+CCI+++A3 THE GOODS AND THIS PERMIT MUST BE PRODUCED FOR CUSTOMS:CLEARANCE/ENDORSEMENT AT AN AIRPORT CUSTOMS CHECKPOINT.'FTX+CCI+++AY GOODS NOT EXPORTED/TRANSHIPPED OR BONDED IN A:LICENSED WAREHOUSE OR RECEIVED BY THE CLAIMANT ON THE SAME:DAY OF REMOVAL MUST BE STORED AT A PLACE APPROVED BY A:PROPER OFFICER OF CUSTOMS.'FTX+CCI+++A6 IF THE PERMIT IS NOT USED, IT MUST BE CANCELLED OR:RE-VALIDATED NOT LATER THAN 24 HOURS OF ITS EXPIRY.'FTX+CCI+++N1 YOU MUST PRODUCE THE GOODS TO CUSTOMS CHECKPOINT FOR:VERIFICATION.'FTX+CCI+++   ********** END OF CARGO CLEARANCE PERMIT **********'TDT+12++4+++++UP0012'DOC+741+40684411854'NAD+AE+XXXXXXXXE47T++TESTING47'NAD+CA+12772770000W++CHANGI INTERNATIONAL AIRPORT SVCS'NAD+EX+XXXXXXXXE47T++TESTING47+TESTADD'NAD+BB'RFF+DAN:D'NAD+DT++USER47'CTA+IC+:1619574Z'COM+12345678:TE'NAD+CN+++ABLETEK INC.+NO.11-1, TZU-CHIANG 1ST ROAD CHUNG-:LI INDUSTRIAL PARK,:                  CHUNG-LI, TAIWAN'NAD+FW+XXXXXXXXE47T++TESTING47'UNS+D'CST+1+84798930'FTX+AAA+++SE300 ULTRA SN# 20006 SOLDER PASTE:INSPECTION SYSTEM INCLUDING:ACCESSORIES'FTX+PRD+++UNBRANDED'LOC+27+CN'MEA+AAF++NMB:1.0000'MEA+ABA++NMB:1.0000'MOA+63:352800.00'MOA+41:1575.90'RFF+AEA:MISC'DOC+714+YAS54634064'UNS+S'CNT+5:1'TAX+1'MOA+63:352800.00'UNT+50+1'";
					cuspmt.Parse(new UNOACharacterSet(), strSample);
					fConsignmentDetails = new PrintPermitConsignmentDetails(cuspmt.ConsignmentDetails[0], Factory);
				}
				return fConsignmentDetails;
			}
		}
		PrintPermitConsignmentDetails fConsignmentDetails;

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			DocPrintPermitConsignmentDetails result = DocPrintPermitConsignmentDetails.New(ConsignmentDetails, Factory);
			return new DocumentWrapper[] { result };
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return DocPrintPermitConsignmentDetails.New(ConsignmentDetails, Factory);
		}

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Singapore);
			base.SetUp();
		}

		DocPrintPermitConsignmentDetails MultiProductConsignmentDetails
		{
			get
			{
				if (fMultiProductConsignmentDetails == null)
				{
					fMultiProductConsignmentDetails = DocPrintPermitConsignmentDetails.New(MultiProductCodeConsignmentDetails, Factory);
				}
				return fMultiProductConsignmentDetails;
			}
		}
		DocPrintPermitConsignmentDetails fMultiProductConsignmentDetails;

		PrintPermitConsignmentDetails MultiProductCodeConsignmentDetails
		{
			get
			{
				if (fMultiProductCodeConsignmentDetails == null)
				{
					CUSPMT cuspmt = new CUSPMT();
					ZString strSample = "UNH+1+CUSPMT:0:1:RT:040+OUTPMT'BGM+962:::TCS+XXXXXXXXE47T        200609223517+11'CST++2'LOC+11+O:::OTHERS'LOC+12+TWTPE'LOC+36+TW'LOC+88+CZ:::CHANGI FTZ,CHANGI'DTM+136:20060924:102'GEI+5+:Y'MEA+ABK++CTN:2.0000'MEA+AAH++KGM:976.0000'FTX+AAI+++ITEM ABOVE IS FOR  DEMO PURPOSE.RETURNED AFTER DEMONSTRATION.'RFF+ABT:OO6I001822A'DTM+160:20060922:102'DTM+273:2006092220061005:718'DTM+9:200609221120:203'RFF+AEA:SC'FTX+CCI+++GA APPROVED BY CUSTOMS SUBJECT TO THE DECLARANT COMPLYING:WITH THE FOLLOWING CONDITION(S) FOR THE PERMIT TO BE VALID.:FAILURE TO COMPLY WITH THE CONDITION(S) IS AN OFFENCE.'FTX+CCI+++A3 THE GOODS AND THIS PERMIT MUST BE PRODUCED FOR CUSTOMS:CLEARANCE/ENDORSEMENT AT AN AIRPORT CUSTOMS CHECKPOINT.'FTX+CCI+++AY GOODS NOT EXPORTED/TRANSHIPPED OR BONDED IN A:LICENSED WAREHOUSE OR RECEIVED BY THE CLAIMANT ON THE SAME:DAY OF REMOVAL MUST BE STORED AT A PLACE APPROVED BY A:PROPER OFFICER OF CUSTOMS.'FTX+CCI+++A6 IF THE PERMIT IS NOT USED, IT MUST BE CANCELLED OR:RE-VALIDATED NOT LATER THAN 24 HOURS OF ITS EXPIRY.'FTX+CCI+++N1 YOU MUST PRODUCE THE GOODS TO CUSTOMS CHECKPOINT FOR:VERIFICATION.'FTX+CCI+++   ********** END OF CARGO CLEARANCE PERMIT **********'TDT+12++4+++++UP0012'DOC+741+40684411854'NAD+AE+XXXXXXXXE47T++TESTING47'NAD+CA+12772770000W++CHANGI INTERNATIONAL AIRPORT SVCS'NAD+EX+XXXXXXXXE47T++TESTING47+TESTADD'NAD+BB'RFF+DAN:D'NAD+DT++USER47'CTA+IC+:1619574Z'COM+12345678:TE'NAD+CN+++ABLETEK INC.+NO.11-1, TZU-CHIANG 1ST ROAD CHUNG-:LI INDUSTRIAL PARK,:                  CHUNG-LI, TAIWAN'NAD+FW+XXXXXXXXE47T++TESTING47'UNS+D'CST+1+84798930'FTX+AAA+++SE300 ULTRA SN# 20006 SOLDER PASTE:INSPECTION SYSTEM INCLUDING:ACCESSORIES'FTX+PRD+++UNBRANDED'LOC+27+CN'MEA+AAF++NMB:1.0000'MEA+ABA++NMB:1.0000'MEA+ABA++NMB:5.0000'MOA+63:352800.00'MOA+41:1575.90'RFF+AEA:MISC'RFF+AEA:HSAPROCOD'DOC+714+YAS54634064'UNS+S'CNT+5:1'TAX+1'MOA+63:352800.00'UNT+52+1'";
					cuspmt.Parse(new UNOACharacterSet(), strSample);
					fMultiProductCodeConsignmentDetails = new PrintPermitConsignmentDetails(cuspmt.ConsignmentDetails[0], Factory);
				}
				return fMultiProductCodeConsignmentDetails;
			}
		}
		PrintPermitConsignmentDetails fMultiProductCodeConsignmentDetails;

		DocPrintPermitConsignmentDetails PackageConsignmentDetails
		{
			get
			{
				if (fPackageConsignmentDetails == null)
				{
					fPackageConsignmentDetails = DocPrintPermitConsignmentDetails.New(PackageCodeConsignmentDetails, Factory);
				}
				return fPackageConsignmentDetails;
			}
		}
		DocPrintPermitConsignmentDetails fPackageConsignmentDetails;

		PrintPermitConsignmentDetails PackageCodeConsignmentDetails
		{
			get
			{
				if (fPackageCodeConsignmentDetails == null)
				{
					var cuspmt = new Cuspmt09b();
					ZString strSample = "UNH+1+CUSPMT:0:1:RT:041+IPTPMT'BGM+962:::DNG+XXXXXXXXE47T     201102140204+11'CST++5'LOC+9+ESBCN'LOC+11+KZ:::KEPPEL FTZ,KEPPEL ROAD, SINGAPORE'LOC+88+KW:::KEPPEL WHARVES'DTM+178:20110218:102'DTM+416:20110214103100SST:304'GEI+5+:Y'MEA+ABK++CAR:13'MEA+AAH++TNE:0.090'EQD+CN+OOLU7462538:1+1:::LCL40'SEL+L356612'FTX+AAI+++SHIPPER SEAL ?:-  L356612/ES11393'RFF+MS:06'RFF+DM:IP03H0407'RFF+ABT:ID6I100233B'DTM+148:20110214001550SST:304'DTM+273:2011021420110222:718'RFF+AEA:CA'DTM+444:20110214001548SST:304'FTX+REG++A20+APPROVED BY AVA(FCD) SUBJECT TO COMPLIANCE WITH THE SALE OF FOOD ACT AND THE FOOD REGULATIONS. PRODUCTS,CONTAINING MEAT OR SEAFOOD WOULD REQUIRE A       LICENCE UNDER THE WHOLESOME MEAT AND FISH ACT. FAILURE TO COMPLY MAY BE         SUBJECTED TO A FINE NOT EXCEEDING $10,000 AND/OR IMPRISONMENT NOT EXCEEDING 3   MONTHS. THIS CCP ALSO SERVES AS AN AVA(FCD) PERMIT FOR ITEMS UNDER AVA(FCD)?'S   CONTROL ONLY. FOR ALL ITEMS'RFF+AEA:SC'FTX+CCI++GA+APPROVED BY CUSTOMS SUBJECT TO THE DECLARANT COMPLYING WITH THE          FOLLOWING CONDITION(S) FOR THE PERMIT TO BE VALID. FAILURE TO COMPLY WITH THE   CONDITION(S) IS AN OFFENCE.'FTX+CCI++G7+SUCCESSFUL GIRO DEDUCTION  OF THE AMOUNT TO  BE PAID FROM THE DECLARING  AGENT?'S ACCOUNT. YOU MUST HAVE ENOUGH FUNDS IN YOUR BANK ACCOUNT TO MEET        PAYMENT BEFORE MAKING THE DECLARATION.'FTX+CCI++GX+YOU ARE REQUIRED TO MAKE GOOD THE DUTY/GST SHOULD THE GIRO DEDUCTION     FAIL. CUSTOMS MAY INVOKE YOUR BG FOR RECOVERY OF THE DUTY/GST. A PENALTY CHARGE MAY BE IMPOSED BY CUSTOMS FOR AN UNSUCCESSFUL GIRO DEDUCTION.'FTX+CCI++TX+THE GOODS DECLARED IN THIS PERMIT ARE IMPORTED/EXPORTED BY A TAXABLE     PERSON'FTX+CCI++A2+THE GOODS AND THIS PERMIT MUST BE PRODUCED FOR CUSTOMS                   CLEARANCE/ENDORSEMENT AT A FREE TRADE ZONE \"OUT\" GATE UNLESS IT IS DIRECTED TO  THE \"GREEN LANE\" AT THE TIME OF CLEARANCE.'FTX+CCI++GQ+IF YOU HAVE NOT PAID THE DUTY/GST WITHIN THE VALIDITY PERIOD OF THE      PERMIT, YOU MUST APPLY FOR CANCELLATION OF PERMIT BEFORE ITS EXPIRY DATE.       EXTENSION OF VALIDITY PERIOD WILL ONLY BE ALLOWED FOR PERMIT WHERE PAYMENT HAD  BEEN MADE.'FTX+CCI++EEE+END OF CARGO CLEARANCE PERMIT.'TDT+3+36E36+1+++++:::SANDRA AZUL'DOC+704+OOLU62238710'NAD+AE+XXXXXXXXE47T++TESTING47:::::1'NAD+CG+11878780000H++ORIENT OVERSEAS CONTAINER LINE LTD:::::1'NAD+IM+XXXXXXXXE47T++TESTING47:::::2'NAD+BB'RFF+DAN:D'NAD+DT'CTA+IC+1619574Z:TESTING NAME 47'COM+12345678:TE'NAD+FW+XXXXXXXXE47T++TESTING47:::::1'UNS+D'DMS+INVOICE DETAILS'MOA+39:488.28:EUR'CUX+++2.000000'TOD+++CIF'NAD+SU++1+SUPPLIER NAME'DOC+380+2.438'DTM+3:20060828:102'ALC+C'MOA+304:9.77:SGD'PCD+5:1.000'CST+1+22042111'FTX+AAA+++CORTE REAL PLATINUM (3 BOT BOX) PROMOTION ITEM'FTX+PRD+++BODEGASVINAEXTREMENA:NA'LOC+27+ES'MEA+AAF++LTR:29.2500'MEA+AAE++LTR:29.2500'MEA+AAI++LTR:0.7500'MEA+ABA++LTR:29.2500'PAC+13+3+CAR'PAC+2+1+BOT'MOA+63:976.56'RFF+IV:2.438'RFF+AEA:ZBP0BH0QVDG'DOC+703+ESBSGS609301H03'TAX+5+:::PRF+++LTR:::95'MOA+161:277.88'TAX+7++++:::7'MOA+369:68.36'UNS+S'CNT+5:1'TAX+1'MOA+63:976.56'TAX+5'MOA+161:277.88'TAX+7'MOA+369:68.36'TAX+2'MOA+9:340.60'UNT+81+1'";
					cuspmt.Parse(new UNOACharacterSet(), strSample);
					fPackageCodeConsignmentDetails = new PrintPermitConsignmentDetails(cuspmt.ConsignmentDetails[0], Factory);
				}
				return fPackageCodeConsignmentDetails;
			}
		}
		PrintPermitConsignmentDetails fPackageCodeConsignmentDetails;

		DocPrintPermitConsignmentDetails CustomsValueConsignmentDetails
		{
			get
			{
				if (fCustomsValueConsignmentDetails == null)
				{
					fCustomsValueConsignmentDetails = DocPrintPermitConsignmentDetails.New(CustomsValueCodeConsignmentDetails, Factory);
				}
				return fCustomsValueConsignmentDetails;
			}
		}
		DocPrintPermitConsignmentDetails fCustomsValueConsignmentDetails;

		PrintPermitConsignmentDetails CustomsValueCodeConsignmentDetails
		{
			get
			{
				if (fCustomsValueCodeConsignmentDetails == null)
				{
					var cuspmt = new Cuspmt09b();
					ZString strSample = "UNH+CWISEB00003141+CUSPMT:0:1:RT:041+INPPMT'BGM+962:::SHO+198800784N       201111016677+11'CST++5'LOC+11+CZ'LOC+88+AP006:::DUTY FREE SHOP'DTM+416:20111101155443SST:304'GEI+5+:Y'MEA+ABK++PAT:50'MEA+AAH++KGM:1000.000'FTX+AAI+++VAV TESTING?: TN_INPDEC_SHO_12'RFF+ABT:IN1K001363J'DTM+148:20111101162311SST:304'DTM+273:2011110120111115:718'RFF+ACE:OO1I002091P'RFF+AEA:SC'FTX+CCI++GA+APPROVED BY CUSTOMS SUBJECT TO THE DECLARANT COMPLYING WITH THE FOLLOWINGCONDITION(S) FOR THE PERMIT TO BE VALID. FAILURE TO COMPLY WITH THE CONDITION(S)IS AN OFFENCE.'FTX+CCI++A3+THE GOODS AND THIS PERMIT WITH INVOICES, BL/AWB, ETC MUST BE PRODUCED FORCUSTOMS CLEARANCE/ENDORSEMENT AT AN AIRPORT CUSTOMS CHECKPOINT.'FTX+CCI++EEE+END OF CARGO CLEARANCE PERMIT.'NAD+DT'CTA+IC+P0111117:QA TEST USER 1'COM+88889999:TE'NAD+AE+198800784N++SG DEMO COMPANY:::::1'NAD+IM+198800784N++CRIMSONLOGIC PTE LTD - (CUSTOMER:SERVICE CENTRE)::::2'NAD+FW+199702247W++EAGLE DATAMATION INTERNATIONAL PTE LTD:::::1'UNS+D'DMS+INVOICE DETAILS'MOA+39:0.00:SGD'NAD+SE'DOC+380+W'DTM+3:20111101:102'CST+1+24039930'FTX+AAA+++MANUFACTURED TOBACCO SUBSTITUTES (KGM)'FTX+PRD+++SHO BRAND'LOC+27+DE'LOC+18+SHO012'MEA+AAF++KGM:1000.0000'MEA+AAE++KGM:1000.0000'MEA+AAI++KGM:1.0000'PAC+50+3+PAT'PAC+10+2+BOX'PAC+2+1+CTN'MOA+63:20000.00'MOA+146:20.00:SGD'RFF+IV:W'TAX+7++++:::7'MOA+369:26040.00'TAX+5++++KGM:::352.0000'MOA+161:352000.00'UNS+S'CNT+5:1'TAX+1'MOA+63:20000.00'TAX+7'MOA+369:26040.00'TAX+5'MOA+161:352000.00'UNT+57+CWISEB00003141'";
					cuspmt.Parse(new UNOACharacterSet(), strSample);
					fCustomsValueCodeConsignmentDetails = new PrintPermitConsignmentDetails(cuspmt.ConsignmentDetails[0], Factory);
				}
				return fCustomsValueCodeConsignmentDetails;
			}
		}
		PrintPermitConsignmentDetails fCustomsValueCodeConsignmentDetails;

		#endregion
	}
}
