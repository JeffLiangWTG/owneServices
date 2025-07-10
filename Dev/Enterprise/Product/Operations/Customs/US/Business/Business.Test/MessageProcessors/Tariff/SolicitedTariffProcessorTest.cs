using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	class SolicitedTariffProcessorTest : ABIProcessorTest<ACSSolicitedTariffProcessor, APLA, APLB, APLY>
	{
		public void TestProcessEmptyBlocksInFRMessage()
		{
			var tariff = CreateTariffForTest();

			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.ExtractReferenceFiles;
			message.EM_MessageNum = "~15000";
			message.EM_MessageText = "B011704286FI                                               YASYUSPRD_249424     F1100103                                                                        Y  1704286FI00001";
			message.EM_Status = "SNT";
			Factory.Save();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_SystemCreateUser = "~BP";

			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse;
			message2.EM_MessageNum = "~15000";
			message2.EM_MessageText = "B013005062FR                                               TLRPDXPDX_2188       F1109901                                                                        V13333333333R1001200930211KG       1WHEAT/MESLIN-DURUM WHEAT,O    000000670000  Y  1704286FI00001";
			message2.EM_Status = "QUE";
			Factory.Save();

			new USRIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newTariff = newFactory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "3333333333"));
			AssertTariffProcessedByEmptyBlocks(newTariff);
		}

		public void TestProcessEmptyBlocksInWRMessage()
		{
			var tariff = CreateTariffForTest();

			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.ExtractReferenceFiles;
			message.EM_MessageNum = "~15000";
			message.EM_MessageText = "B011704286FI                                               YASYUSPRD_249424     F1100103                                                                        Y  1704286FI00001";
			message.EM_Status = "SNT";
			Factory.Save();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_SystemCreateUser = "~BP";

			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem;
			message2.EM_MessageNum = "~15000";
			message2.EM_MessageText = "B013005062FR                                               TLRPDXPDX_2188       F1109901                                                                        V13333333333R1001200930211KG       1WHEAT/MESLIN-DURUM WHEAT,O    000000670000  Y  1704286FI00001";
			message2.EM_Status = "QUE";
			Factory.Save();

			new USRIncomingMessageProcessor().ExecuteBatch();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newTariff = newFactory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "3333333333"));
			AssertTariffProcessedByEmptyBlocks(newTariff);
		}

		USCTariff CreateTariffForTest()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "3333333333";
			tariff.UE_DateFrom = new ZDate(2020, 10, 01);
			tariff.UE_DateTo = new ZDate(2021, 09, 30);

			// V2 block
			tariff.UE_Column1RateAdValorem = 11m;
			tariff.UE_Column1RateOther = 12m;

			tariff.UE_Column2RateSpecific = 10m;
			tariff.UE_Column2RateAdValorem = 13m;
			tariff.UE_Column2RateOther = 14m;

			tariff.UE_CountervailingDutyFlag = true;
			tariff.UE_AdditionalTariffNumberIndicator = true;
			tariff.UE_PermitLicenseIndicator = "R";

			// V3 block
			tariff.UE_GSPExcludedCountries = "CA";
			tariff.UE_AntiDumping = true;
			tariff.UE_QuotaIndicator = true;
			tariff.UE_TextileCategoryNumber = "123";
			tariff.UE_SPICode = "A";

			// V4 block
			tariff.UE_ISOCountryofOriginEditCode = "AU";
			var tariffValue = tariff.GetOrCreateNewTariffValue();
			tariffValue.UA_ValueEditCode = "X";
			tariffValue.UA_ValueLowBounds = 1m;
			tariffValue.UA_ValueHighBounds = 3m;
			var tariffDateRestriction1 = tariff.TariffDateRestrictions.CreateWithRestrictionDatesIfNotExist("1", 601, 831);
			var tariffDateRestriction2 = tariff.TariffDateRestrictions.CreateWithRestrictionDatesIfNotExist("2", 701, 930);
			var tariffQuantity = tariff.GetOrCreateNewTariffQuantity();
			tariffQuantity.UQ_QuantityEditCode = "B";
			tariffQuantity.UQ_LowerBound = 5m;
			tariffQuantity.UQ_UpperBound = 7m;

			// V56789ABCEFGHIJK block
			var dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_DutyElement = "T";
			dutyRate.UD_AdValoremSpecialRate = 0.2m;
			dutyRate.UD_ISOCountryCode = "DE";
			dutyRate.UD_OtherSpecialRate = 0.3m;
			dutyRate.UD_SpecificSpecialRate = 0.4m;
			dutyRate.UD_TaxFeeAdvalorem = 7m;
			dutyRate.UD_TaxFeeClassCode = "C";
			dutyRate.UD_TaxFeeComputationCode = "X";
			dutyRate.UD_TaxFeeFlag = "Y";
			dutyRate.UD_TaxFeeSpecificRate = 0.5m;

			// VL block
			tariff.UE_PGACodes = "J";

			Factory.Save();
			return tariff;
		}

		void AssertTariffProcessedByEmptyBlocks(USCTariff tariff)
		{
			AssertEquals(0m, tariff.UE_Column1RateAdValorem);
			AssertEquals(0m, tariff.UE_Column1RateOther);
			AssertEquals(0m, tariff.UE_Column2RateSpecific);
			AssertEquals(0m, tariff.UE_Column2RateAdValorem);
			AssertEquals(0m, tariff.UE_Column2RateOther);
			AssertEquals(false, tariff.UE_CountervailingDutyFlag);
			AssertEquals(false, tariff.UE_AdditionalTariffNumberIndicator);
			AssertEquals(ZString.Empty, tariff.UE_PermitLicenseIndicator);

			AssertEquals(ZString.Empty, tariff.UE_GSPExcludedCountries);
			AssertEquals(false, tariff.UE_AntiDumping);
			AssertEquals(false, tariff.UE_QuotaIndicator);
			AssertEquals(ZString.Empty, tariff.UE_TextileCategoryNumber);
			AssertEquals(ZString.Empty, tariff.UE_SPICode);

			AssertEquals(ZString.Empty, tariff.UE_ISOCountryofOriginEditCode);
			var factory = tariff.Factory;
			var query = new ZQuery(USCTariffValueSchema.UA_UE, tariff.PK);
			query.FetchOnlyFromLocalCache = !tariff.IsInDatabase;
			var tariffValues = factory.Load<USCTariffValue>(query).ToList();
			AssertEquals(0, tariffValues.Count);
			AssertEquals(0, tariff.TariffDateRestrictions.Count);
			query = new ZQuery(USCTariffQuantitySchema.UQ_UE, tariff.PK);
			query.FetchOnlyFromLocalCache = !tariff.IsInDatabase;
			var tariffQuantities = factory.Load<USCTariffQuantity>(query).ToList();
			AssertEquals(0, tariffQuantities.Count);

			AssertEquals(0, tariff.DutyRates.Count);

			AssertEquals(ZString.Empty, tariff.UE_PGACodes);
		}

		public void TestProcessCS00243244()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.ExtractReferenceFiles;
			message.EM_MessageNum = "~15000";
			message.EM_MessageText = "B011704286FI                                               YASYUSPRD_249424     F1100103                                                                        Y  1704286FI00001";
			message.EM_Status = "SNT";
			Factory.Save();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_SystemCreateUser = "~BP";

			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse;
			message2.EM_MessageNum = "~15000";
			message2.EM_MessageText = FailedQueryResponseMessage.Replace("\r\n", "");
			message2.EM_Status = "QUE";

			Factory.Save();

			new USRIncomingMessageProcessor().ExecuteBatch();

			message2.Reload();
			AssertEquals("SHould have succeeded", MQEDIMessage.Status.Received, message2.EM_Status);
		}

		[NUnit.Framework.TestDate(2016, 12, 1)]
		public void TestFailureWithACEMessage()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQuery;
			message.EM_MessageNum = "HYEDUSCMT_175424";
			message.EM_MessageText = "B  3910SV9HB                                               HYEDUSCMT_175424     F1101612                                                                        Y  3910SV9HB";
			message.EM_Status = "SNT";
			Factory.Save();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_SystemCreateUser = "~BP";

			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse;
			message2.EM_MessageNum = "HYEDUSCMT_175424";
			message2.EM_MessageText = "B003910SV9HZ                                               HYEDUSCMT_175424     F110161200000000009999999999NOT ON FILE OR EXPIRED                              Y  3910SV9HZ00000                                                               ";
			message2.EM_Status = "QUE";

			var htsDataVersion = USCDataVersion.GetLastHTSAttempt(Factory);
			htsDataVersion.UZ_Version = 1612;

			Factory.Save();

			new USRIncomingMessageProcessor().ExecuteBatch();

			htsDataVersion = USCDataVersion.GetLastHTSAttempt(new BusinessObjectFactory());
			Assert(htsDataVersion.UZ_Note.StartsWith("Failure"));
		}

		#region Response Message Text

		const string FailedQueryResponseMessage = @"B011704286FR                                               YASYUSPRD_249424     
F1100103                                                                        
V10804400000R0101010630011KG       1AVOCADOS, FRESH OR DRIED      000011200000  
V20804400000000000000000000000000000000033100000000000000000000000000000        
V30804400000                                        D E J A+CAILMX              
V50804400000MX000002600000000000000000000000000000                              
V10804400010A0701011231011KG       1AVOCADOS, HASS & HASS LIKE    000011200000  
V20804400010000000000000000000000000000033100000000000000000000000000000        
V30804400010                                        D E J A+CAILMX              
V50804400010MX000002600000000000000000000000000000                              
V10804400090A0701011231011KG       1AVOCADOS, OTHER               000011200000  
V20804400090000000000000000000000000000033100000000000000000000000000000        
V30804400090                                        D E J A+CAILMX              
V50804400090MX000002600000000000000000000000000000                              
V11109001000R0601000601011KG       7WHEAT GLUTEN USED AS ANIM.    000000000000  
V21109001000000001800000000000000000000000000000000020000000000000000000        
V31109001000                                    1   E J CAILMX                  
V11109001000A0602019999991KG       7WHEAT GLUTEN USED AS ANIM.    000000000000  
V21109001000000001800000000000000000000000000000000020000000000000000000        
V31109001000                                        A E J CAILMX                
V11109009000R0601000601011KG       7WHEAT GLUTEN, DRIED OR NOT    000000000000  
V21109009000000006800000000000000000000000000000000020000000000000000000        
V31109009000                                    1   E J CAILMX                  
V11109009000A0602019999991KG       7WHEAT GLUTEN, DRIED OR NOT    000000000000  
V21109009000000006800000000000000000000000000000000020000000000000000000        
V31109009000                                        A E J CAILMX                
V12106905200R0701999999991L        9FRUIT/VEG JUICE, SINGLE FR    000000000000  
V22106905200000100000000000000000000000000000000000100000000000000000000 R      
V32106905200SV                                 1    A*E J CAILMX                
V52106905200MX000000000000000100000000000000000000                              
V12106905400R0101989999991L        9FRUIT/VEG JUICE, MIX OF JU    000000000000  
V22106905400000100000000000000000000000000000000000100000000000000000000 R      
V32106905400                                        A E J CAILMX                
V52106905400MX000000000000000100000000000000000000                              
V12202903600R0101989999991L        9JUICE N/ORANGE FORT/VITS &    000000000000  
V22202903600000100000000000000000000000000000000000100000000000000000000 R      
V32202903600DO                                      A*E J CAILMX                
V52202903600MX000000000000000100000000000000000000                              
V12202903700R0101989999991L        9JUICE N/ORANG MIX OF JUIC     000000000000  
V22202903700000100000000000000000000000000000000000100000000000000000000 R      
V32202903700                                        A E J CAILMX                
V52202903700MX000000000000000100000000000000000000                              
V12206004500R0101009999991L        1RICE WINE/SAKE OTH FERMNTD    000003000000  
V22206004500000000000000000000000000000033000000000000000000000000000000        
V32206004500                                        A E J CAILMX                
V52206004500                                      017X1000000000000000000000000 
V12709002000R0101010630011BBL      1PETRLM OILS,TST 25D A.P.I. OR>000010500000  
V22709002000000000000000000000000000000021000000000000000000000000000000        
V32709002000                                        D R A+CAILMX                
V52709002000MX000002100000000000000000000000000000                              
V62709002000R 000002100000000000000000000000000000                              
V12709002010A0701011231011BBL      1CONDENSATE WHOLLY FROM NG     000010500000  
V22709002010000000000000000000000000000021000000000000000000000000000000        
V32709002010                                        D R A+CAILMX                
V52709002010MX000002100000000000000000000000000000                              
V62709002010R 000002100000000000000000000000000000                              
V12709002090A0701011231011BBL      1OTHER CRUDE,TST 25D API OR >  000010500000  
V22709002090000000000000000000000000000021000000000000000000000000000000        
V32709002090                                        D R A+CAILMX                
V52709002090MX000002100000000000000000000000000000                              
V62709002090R 000002100000000000000000000000000000                              
V12710004510R0101010630011BBL      1CONDENSATE DERIVD WHOLY FRM NG000010500000  
V22710004510000000000000000000000000000021000000000000000000000000000000        
V32710004510                                        D R A+CAILMX                
V52710004510MX000002100000000000000000000000000000                              
V62710004510R 000002100000000000000000000000000000                              
V12905170000R0101011231011KG       7DO,HEXA,& OCTA(DECAN-1-OL)    000000000000  
V22905170000000005000000000000000000000000000000000025000000000000000000        
V32905170000                                        D E J A+CAILMX              
V12921421500R0101011231011KG       4N-ETHYLANILINE;N,N-DIETHYLANIL000000700000  
V22921421500000010200000000000000000000015400000000060000000000000000000        
V32921421500IN                                      A*E J CAILMX                
V52921421500MX000000400000000003700000000000000000                              
V13402205000R0101899999991KG       7ORG,SURF ACTE AGNTS,RETAIL    000000000000  
V23402205000000000000000000000000000000000000000000025000000000000000000        
V33402205000                                                                    
V15109109000R1201001231001KG       7OTH WOOL/ANML HAIR=>85%, O    000000000000  
V25109109000000007200000000000000000000000000000000055500000000000000000        
V35109109000                                    1400CAILMX                      
V15109909000R1201001231001KG       7WOOL/FINE HAIR YARN, OTHER    000000000000  
V25109909000000007200000000000000000000000000000000055500000000000000000        
V35109909000                                    1400CAILMX                      
V15109909000R0101011231011KG       7WOOL/FINE HAIR YARN, OTHER    000000000000  
V25109909000000006900000000000000000000000000000000055500000000000000000        
V35109909000                                    1400CAILMX                      
V15503200015A0701019999991KG       7DUAL FBR W/ LOW MELT OUTER    000000000000  
V25503200015000004500000000000000000000000000000000025000000000000000000        
V35503200015                                        CAILMX                      
V15503200020R0101010630011KG       7FBR,SYN STAPLE,NT CRD,PLY,<3.3000000000000  
V25503200020000004500000000000000000000000000000000025000000000000000000        
V35503200020                                        CAILMX                      
V15503200025A0701019999991KG       7OTHER POLY FIBER <3.3 DECI    000000000000  
V25503200025000004500000000000000000000000000000000025000000000000000000        
V35503200025                                        CAILMX                      
V15503200040R0101010630011KG       7FBR,SYN STAPLE,NTCRD,PLY33-132000000000000  
V25503200040000004500000000000000000000000000000000025000000000000000000        
V35503200040                                   1    CAILMX                      
V15503200045A0701019999991KG       7OTHER 3.3 DTX UP TO 13.2 D    000000000000  
V25503200045000004500000000000000000000000000000000025000000000000000000        
V35503200045                                        CAILMX                      
V15503200060R0101010630011KG       7FBR,SYN STAPLE,NTCRD,PLY>=13.2000000000000  
V25503200060000004500000000000000000000000000000000025000000000000000000        
V35503200060                                   1    CAILMX                      
V15503200065A0701019999991KG       7POLY FBR 13.2 DECITEX OR M    000000000000  
V25503200065000004500000000000000000000000000000000025000000000000000000        
V35503200065                                        CAILMX                      
V16110202065R0101011231012DOZKG    7MEN'S/BOYS'PULLOVRS,ETC,KN/CRO000000000000  
V26110202065000017800000000000000000000000000000000050000000000000000000        
V36110202065                                    1338CAILMX                      
V56110202065MX00000000000099999999999900000000000005621000001138100000000000000 
V16204230060R0101989999992DOZKG    9ENS,OTHR, SYN,W/G,< 36% WL    000000000000  
V26204230060000100000000000000000000000000000000000100000000000000000000 R      
V36204230060                                    1659CAILMX                      
V56204230060MX000000000000000100000000000000000000                              
V17326908586R0701009999991KG       7ARTICLES OF IRON/STEEL:NSP    000000000000  
V27326908586000002900000000000000000000000000000000045000000000000000000        
V37326908586                                        A B E J CAILMX              
V18215993000R0101019999991PCS      7SPOONS UNDER $.25 EA,W/SS HNDL000000000000  
V28215993000000014000000000000000000000000000000000040000000000000000000        
V38215993000                                        D E J CAILMX                
V4821599300011600000000000000025000                                             
V18472909520R0101000630011NO       7OTH COIN/CURRENCY HAND MAC    000000000000  
V28472909520000001800000000000000000000000000000000035000000000000000000        
V38472909520                                        A E J CAILMX                
V18472909540A0701009999991NO       7OTHER DESKTOP NOTE HANDLER    000000000000  
V28472909540000001800000000000000000000000000000000035000000000000000000        
V38472909540                                        A E J CAILMX                
V18472909550R0101000630011NO       7OTHER,OFFICE MACHINES         000000000000  
V28472909550000001800000000000000000000000000000000035000000000000000000        
V38472909550                                        A E J CAILMX                
V18472909560A0701009999991NO       7OTHER CURRENCY & COIN HAND    000000000000  
V28472909560000001800000000000000000000000000000000035000000000000000000        
V38472909560                                        A E J CAILMX                
V18472909580A0701009999991NO       7OTHER OFFICE MACHINES         000000000000  
V28472909580000001800000000000000000000000000000000035000000000000000000        
V38472909580                                        A E J CAILMX                
V18512300030A0701019999991NO       7RADAR DETECTORS FOR VEHICL    000000000000  
V28512300030000002500000000000000000000000000000000035000000000000000000        
V38512300030                                        A B E J CAILMX              
V18531809038R0101000630011NO       7RADAR DETECTOR USE/MOTOR V    000000000000  
V28531809038000001300000000000000000000000000000000035000000000000000000        
V38531809038                                        A B C E J CAILMX            
V18543899695R0101999999991X        7ELECTRIC MACHINE,APPARTUS     000000000000  
V28543899695000002600000000000000000000000000000000035000000000000000000        
V38543899695                                        A B E J CAILMX              
V19201100005A0701019999991NO       7UPRIGHT PIANOS, USED          000000000000  
V29201100005000004700000000000000000000000000000000040000000000000000000        
V39201100005                                        A E J CAILMX                
V19201100010R0101010630011NO       7UPRIGHT PIANOS,CASE<111.76CM  000000000000  
V29201100010000004700000000000000000000000000000000040000000000000000000        
V39201100010                                        A E J CAILMX                
V19201100011A0701019999991NO       7UPRIGHT PIANOS,CASE<111.76    000000000000  
V29201100011000004700000000000000000000000000000000040000000000000000000        
V39201100011                                        A E J CAILMX                
V19201100020R0101010630011NO       7UPRGHT PIANO,>=111.76<121.92CM000000000000  
V29201100020000004700000000000000000000000000000000040000000000000000000        
V39201100020                                        A E J CAILMX                
V19201100021A0701019999991NO       7UPRGHT PIANO,>=111.76<121.    000000000000  
V29201100021000004700000000000000000000000000000000040000000000000000000        
V39201100021                                        A E J CAILMX                
V19201100030R0101010630011NO       7UPRGHT PIANO,>=121.92<129.54CM000000000000  
V29201100030000004700000000000000000000000000000000040000000000000000000        
V39201100030                                        A E J CAILMX                
V19201100031A0701019999991NO       7UPRGHT PIANO,>=121.92<129.    000000000000  
V29201100031000004700000000000000000000000000000000040000000000000000000        
V39201100031                                        A E J CAILMX                
V19201100040R0101010630011NO       7UPRIGHT PIANOS,CASE>=129.54CM 000000000000  
V29201100040000004700000000000000000000000000000000040000000000000000000        
V39201100040                                        A E J CAILMX                
V19201100041A0701019999991NO       7UPRIGHT PIANOS,CASE>=129.5    000000000000  
V29201100041000004700000000000000000000000000000000040000000000000000000        
V39201100041                                        A E J CAILMX                
V19201200005A0701019999991NO       7GRAND PIANOS, USED            000000000000  
V29201200005000004700000000000000000000000000000000040000000000000000000        
V39201200005                                        A E J CAILMX                
V19201200010R0101010630011NO       7GRAND PIANOS,CASE <152.40 CM  000000000000  
V29201200010000004700000000000000000000000000000000040000000000000000000        
V39201200010                                        A E J CAILMX                
V19201200011A0701019999991NO       7GRAND PIANOS,CASE <152.40     000000000000  
V29201200011000004700000000000000000000000000000000040000000000000000000        
V39201200011                                        A E J CAILMX                
V19201200020R0101010630011NO       7GRAND PIANO,>=152.40<167.64CM 000000000000  
V29201200020000004700000000000000000000000000000000040000000000000000000        
V39201200020                                        A E J CAILMX                
V19201200021A0701019999991NO       7GRAND PIANO,>=152.40<167.6    000000000000  
V29201200021000004700000000000000000000000000000000040000000000000000000        
V39201200021                                        A E J CAILMX                
V19201200030R0101010630011NO       7GRAND PIANO,>=167.64<180.34CM 000000000000  
V29201200030000004700000000000000000000000000000000040000000000000000000        
V39201200030                                        A E J CAILMX                
V19201200031A0701019999991NO       7GRAND PIANO,>=167.64<180.3    000000000000  
V29201200031000004700000000000000000000000000000000040000000000000000000        
V39201200031                                        A E J CAILMX                
V19201200040R0101010630011NO       7GRAND PIANO,>=180.34<195.58CM 000000000000  
V29201200040000004700000000000000000000000000000000040000000000000000000        
V39201200040                                        A E J CAILMX                
V19201200041A0701019999991NO       7GRAND PIANO,>=180.34<195.5    000000000000  
V29201200041000004700000000000000000000000000000000040000000000000000000        
V39201200041                                        A E J CAILMX                
V19201200050R0101010630011NO       7GRAND PIANOS,CASE>=195.58 CM  000000000000  
V29201200050000004700000000000000000000000000000000040000000000000000000        
V39201200050                                        A E J CAILMX                
V19201200051A0701019999991NO       7GRAND PIANOS,CASE>=195.58     000000000000  
V29201200051000004700000000000000000000000000000000040000000000000000000        
V39201200051                                        A E J CAILMX                
V19503900000R0101010630011X        7OTHER TOYS,PARTS/ACCES THEREOF000000000000  
V29503900000000000000000000000000000000000000000000070000000000000000000        
V19503900020A0701019999991NO       7INFLATABLE TOY BALLS, BALL    000000000000  
V29503900020000000000000000000000000000000000000000070000000000000000000        
V19503900080A0701019999991X        7OTHER                         000000000000  
V29503900080000000000000000000000000000000000000000070000000000000000000        
V198010060  R0101899999990         0EXP TEMP USE PUBLIC EXPO,F    000000000000  
V298010060  000000000000000000000000000000000000000000000000000000000000 R      
V398010060                                      1                               
V198170096  R0101939999990         0ARTICLES FOR BLIND, OTHER     000000000000  
V298170096  000000000000000000000000000000000000000000000000000000000000 R      
V199023265  R0101011231020         7DIRECT BLUE 199 SODIUM SALT   000000000000  
V299023265  000007400000000000000000000000000000000000000000000000000000 R      
V499023265                                           01                         
V199023265  D0101021231020         7DIRECT BLUE 199 SODIUM SAL    000000000000  
V199023267  R0101011231020         7DIRECT BLACK 195, CAS160512936000000000000  
V299023267  000006400000000000000000000000000000000000000000000000000000 R      
V499023267                                           01                         
V199023267  D0101021231020         7DIRECT BLACK 195, CAS16051    000000000000  
Y  1704286FR00223                                                               ";

		#endregion

		protected override void EndToEndCore()
		{
			var processor = new ACSSolicitedTariffProcessor();
			AddMessageBlocksToProcessor(processor, ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse,
"V10299999999R0701059999991KG       1LAMB,BONE IN, FROZEN, LOIN    000000700000",
"V20299999999000000000000000000000000000015400000000000000000000000000000",
"V30299999999                                        D E J A+AUCACLILJOMAMXSG",
"V10399999999R0101061231061KG       7STURGEON ROE, FROZEN          000000000000",
"V20399999999000015000000000000000000000000000000000030000000000000000000",
"V30399999999                                        A E J AUCACLILJOMAMXSG",
"V50399999999SG000000000000000009300000000000000000");

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse;
			processor.Message = responseMessage;
			processor.Process();

			var queryLamb = new ZQuery(USCTariffSchema.UE_Tariff, "0299999999");
			var tariffLamb = Factory.LoadTop1<USCTariff>(queryLamb);
			AssertNotNull(tariffLamb);
			AssertEquals(0, tariffLamb.DutyRates.Count);
			AssertEquals("D E J A+AUCACLILJOMAMXSG", tariffLamb.UE_SPICode);

			var querySturgeon = new ZQuery(USCTariffSchema.UE_Tariff, "0399999999");
			var tariffSturgeon = Factory.LoadTop1<USCTariff>(querySturgeon);
			AssertNotNull(tariffSturgeon);
			AssertEquals(1, tariffSturgeon.DutyRates.Count);
			AssertEquals("SG", tariffSturgeon.DutyRates[0].UD_ISOCountryCode);
			AssertEquals(0.093m, tariffSturgeon.DutyRates[0].UD_AdValoremSpecialRate);
		}

		public void TestUnnecessaryTariffsDeletedAndTariffsUpdated()
		{
			CreateTariff("6110909089", new ZDate(2009, 01, 01), new ZDate(2009, 01, 31));
			CreateTariff("6110909089", new ZDate(2009, 01, 01), new ZDate(2009, 12, 31));

			CreateTariff("7019524028", new ZDate(2009, 01, 01), new ZDate(2009, 01, 31));
			CreateTariff("7019524028", new ZDate(2009, 01, 01), new ZDate(2009, 12, 31));

			CreateTariff("6204423051", new ZDate(2006, 01, 01), new ZDate(2014, 12, 31));
			CreateTariff("99130461", new ZDate(2006, 01, 01), new ZDate(2021, 12, 31));

			Factory.Save();
			var tariffs = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "6110909089"));
			AssertEquals(2, tariffs.Length);

			tariffs = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "7019524028"));
			AssertEquals(2, tariffs.Length);

			tariffs = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "6204423051"));
			AssertEquals(1, tariffs.Length);
			AssertEquals("Precondition: Tariff Dates", tariffs[0].UE_DateFrom, new ZDate(2006, 01, 01));
			AssertEquals("Precondition: Tariff Dates", tariffs[0].UE_DateTo, new ZDate(2014, 12, 31));

			tariffs = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "99130461"));
			AssertEquals(1, tariffs.Length);
			AssertEquals("Precondition: Tariff Dates", tariffs[0].UE_DateFrom, new ZDate(2006, 01, 01));
			AssertEquals("Precondition: Tariff Dates", tariffs[0].UE_DateTo, new ZDate(2021, 12, 31));

			var processor = new ACSSolicitedTariffProcessor();
			AddMessageBlocksToProcessor(processor, ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse,
"V16204423051A0101091231091KG       7OTHER MEAT AND EDIBLE OFFA    000000000000  ",
"V26204423051000006400000000000000000000000000000000020000000000000000000        ",
"V36204423051                                        D E J P A+AUBHCACLILJOMAMXOM",
"V10208909000A0101109999991KG       7OTHER MEAT AND EDIBLE OFFA    000000000000  ",
"V20208909000000006400000000000000000000000000000000020000000000000000000        ",
"V30208909000                                        D E J P A+AUBHCACLILJOMAMXOM",
"VD0208909000PESG                                                                ",
"V16110909089A0201091231092DOZKG    7PULOVRS,TEXT,M-MD RST,OTH,    000000000000  ",
"V26110909089000006000000000000000000000000000000000060000000000000000000        ",
"V36110909089                                    1638E*P AUBHCACLILJOMAMXOMPESG  ",
"V10302695080A0101109999991KG       1DOLPHIN FISH (MAHI MAHI)      000000000000  ",
"V20302695080000000000000000000000000000002200000000000000000000000000000        ",
"V30302695080                                                                    ",
"V10303695085R0203070618101KG       1OTHER,FISH,OTHER,FRSH OR C    000000000000  ",
"V20303695085000000000000000000000000000002200000000000000000000000000000        ",
"V30303695085                                                                    ",
"V10401310500R0201091231091L        1MILK/CREAM >6% BUT <=45% F    000003200000  ",
"V20401310500000000000000000000000000000015000000000000000000000000000000        ",
"V30401310500                                    1   D E J P A+BHCACLILJOMAOMPESG",
"V17019524028A0201091231092M2 KG    7GLS FBR,WVN FAB N/COL,>215    000000000000  ",
"V27019524028000007300000000000000000000000000000000050000000000000000000        ",
"V37019524028                                    1622P AUBHCACLILJOMAMXOMPESG    ",
"V199130461  A0101091231092M2 KG    7GLS FBR,WVN FAB N/COL,>215    000000000000  ",
"V299130461  000007300000000000000000000000000000000050000000000000000000        ",
"V399130461                                      1622P AUBHCACLILJOMAMXOMPESG    ");

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse;
			processor.Message = responseMessage;
			processor.Process();
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			AssertProcessingResults("6110909089", newFactory, new ZDate(2009, 01, 01), new ZDate(2009, 01, 31), new ZDate(2009, 02, 01), new ZDate(2009, 12, 31));
			AssertProcessingResults("7019524028", newFactory, new ZDate(2009, 01, 01), new ZDate(2009, 01, 31), new ZDate(2009, 02, 01), new ZDate(2009, 12, 31));
			AssertProcessingResults("6204423051", newFactory, new ZDate(2006, 01, 01), new ZDate(2008, 12, 31), new ZDate(2009, 01, 01), new ZDate(2009, 12, 31));
			AssertProcessingResults("99130461", newFactory, new ZDate(2006, 01, 01), new ZDate(2008, 12, 31), new ZDate(2009, 01, 01), new ZDate(2009, 12, 31));
		}

		public void TestProcessingOfTariffsThatAreGoingToBeDeleted()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.ExtractReferenceFiles;
			message.EM_MessageNum = "~15000";
			message.EM_MessageText = "B011704286FI                                               YASYUSPRD_249424     F1100103                                                                        Y  1704286FI00001";
			message.EM_Status = "SNT";
			Factory.Save();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_SystemCreateUser = "~BP";

			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse;
			message2.EM_MessageNum = "~15000";
			message2.EM_MessageText = @"B003910SV9HZ                                               HYEDUSCMT_186490     
V15208516060D1004161231392M2 KG    7WVFAB>85%COT,PRNT,PLWV#43-68PR000000000000  
V25208516060000011400000000000000000000000000000000027700000000000000000        
V35208516060                                    1315P AUBHCACLCOILJOKRMAMXOMPAPE
V45208516060   00000000000000000000 00000000 00000000    21500000000000000010000
V55208516060  00000000000000000000000000000000000005621000001195022000000000000 
VD5208516060SG                                                                  
Y  1101SV9WR00010".Replace("\r\n", "");
			message2.EM_Status = "QUE";

			Factory.Save();

			new USRIncomingMessageProcessor().ExecuteBatch();
			message2.Reload();
			AssertEquals("RCV", message2.EM_Status);
		}

		public void TestProcessAndSetOGACodes()
		{
			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.ExtractReferenceFiles;
			message.EM_MessageNum = "~15000";
			message.EM_MessageText = "B011704286FI                                               YASYUSPRD_249424     F1100103                                                                        Y  1704286FI00001";
			message.EM_Status = "SNT";
			Factory.Save();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_SystemCreateUser = "~BP";

			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem;
			message2.EM_MessageNum = "~15000";
			message2.EM_MessageText = ResponseWithTwoTariffs.Replace("\r\n", "");
			message2.EM_Status = "QUE";

			Factory.Save();

			new USRIncomingMessageProcessor().ExecuteBatch();

			var query = new ZQuery(USCTariffSchema.UE_Tariff, "6912005000");
			query.AddToFilter(USCTariffSchema.UE_DateFrom, new ZDateTime(2012, 10, 31));
			query.AddToFilter(USCTariffSchema.UE_DateTo, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
			var tariff = Factory.LoadTop1<USCTariff>(query);
			AssertEquals("MX", tariff.UE_ISOCountryofOriginEditCode);
			AssertEquals("", tariff.UE_OGACodes);

			query = new ZQuery(USCTariffSchema.UE_Tariff, "6914908000");
			query.AddToFilter(USCTariffSchema.UE_DateFrom, new ZDateTime(2012, 10, 31));
			query.AddToFilter(USCTariffSchema.UE_DateTo, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
			tariff = Factory.LoadTop1<USCTariff>(query);
			AssertEquals("", tariff.UE_OGACodes);
			AssertEquals("", tariff.UE_ISOCountryofOriginEditCode);
		}

		const string ResponseWithTwoTariffs = @"B011101SV9WR                                               HYEDUSCMT_148384     
W16912005000 1031129999991X        7CERAM HOUSE ART OTH NOT POR/CH000000000000  
W269120050000000060000000000000000000000000000000000505000000000000000001       
W36912005000                                        A E J P AUBHCACLCOILJOKRMAMX
W46912005000                                         MX                         
WD6912005000OMPAPESG                                                            
W06912005000100213           RANGE INCLUDES 0001 RECORDS                        
W16914908000 1031129999991X        7OTHER CERAMIC ARTICLES, NSPF  000000000000  
W26914908000000005600000000000000000000000000000000045000000000000000000        
W36914908000                                        A E J P AUBHCACLCOILJOKRMAMX
WD6914908000OMPAPESG                                                            
W06914908000100213           RANGE INCLUDES 0001 RECORDS                        
Y  1101SV9WR00010
";

		void AssertProcessingResults(string tariffNum, BusinessObjectFactory newFactory, ZDate dateFrom1, ZDate dateTo1, ZDate dateFrom2, ZDate dateTo2)
		{
			USCTariff[] tariffs = newFactory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, tariffNum));
			AssertEquals(2, tariffs.Length);

			AssertEquals(tariffs[0].UE_DateFrom, dateFrom1);
			AssertEquals(tariffs[0].UE_DateTo, dateTo1);
			AssertEquals(tariffs[1].UE_DateFrom, dateFrom2);
			AssertEquals(tariffs[1].UE_DateTo, dateTo2);
		}

		void CreateTariff(string tariffNum, ZDate dateFrom, ZDate dateTo)
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = tariffNum;
			tariff.UE_DateFrom = dateFrom;
			tariff.UE_DateTo = dateTo;
		}

		public void TestTariffDateExpired()
		{
			CreateTariff("0302795076", new ZDate(2007, 02, 03), new ZDate(2009, 12, 31));
			CreateTariff("0302795076", new ZDate(2007, 02, 03), new ZDate(2099, 12, 31));
			CreateTariff("0303695085", new ZDate(2007, 02, 03), new ZDate(2099, 12, 31));
			CreateTariff("0401310500", new ZDate(2010, 01, 01), new ZDate(2099, 12, 31));
			CreateTariff("0401310500", new ZDate(2009, 02, 01), new ZDate(2009, 12, 31));

			Factory.Save();

			var tariffs = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "0302795076"));
			AssertEquals(2, tariffs.Length);

			tariffs = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "0303695085"));
			AssertEquals(1, tariffs.Length);

			tariffs = Factory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "0401310500"));
			AssertEquals(2, tariffs.Length);

			var processor = new ACSSolicitedTariffProcessor();
			AddMessageBlocksToProcessor(processor, ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse,
"V10208909000A0101109999991KG       7OTHER MEAT AND EDIBLE OFFA    000000000000  ",
"V20208909000000006400000000000000000000000000000000020000000000000000000        ",
"V30208909000                                        D E J P A+AUBHCACLILJOMAMXOM",
"VD0208909000PESG                                                                ",
"V10302795076R0203071231091KG       1OTHER,FISH,OTHER,FRSH OR C    000000000000  ",
"V20302795076000000000000000000000000000002200000000000000000000000000000        ",
"V30302795076                                                                    ",
"V10302695080A0101109999991KG       1DOLPHIN FISH (MAHI MAHI)      000000000000  ",
"V20302695080000000000000000000000000000002200000000000000000000000000000        ",
"V30302695080                                                                    ",
"V10303695085R0203070618101KG       1OTHER,FISH,OTHER,FRSH OR C    000000000000  ",
"V20303695085000000000000000000000000000002200000000000000000000000000000        ",
"V30303695085                                                                    ",
"V10401310500R0201091231091L        1MILK/CREAM >6% BUT <=45% F    000003200000  ",
"V20401310500000000000000000000000000000015000000000000000000000000000000        ",
"V30401310500                                    1   D E J P A+BHCACLILJOMAOMPESG",
"V10401310500A0101109999992L  CKG   1MILK/CREAM >6% BUT <=45% F    000003200000  ",
"V20401310500000000000000000000000000000015000000000000000000000000000000        ",
"V30401310500                                    1   D E J P A+BHCACLILJOMAOMPESG");

			var responseMessage = Factory.New<MQEDIMessage>();
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageType = ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse;
			processor.Message = responseMessage;
			processor.Process();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			tariffs = newFactory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "0302795076"));
			AssertEquals(1, tariffs.Length);

			var tariff1Reloaded = tariffs[0];
			AssertEquals("0302795076", tariff1Reloaded.UE_Tariff);
			AssertEquals(new ZDate(2007, 02, 03), tariff1Reloaded.UE_DateFrom);
			AssertEquals("Expire Date should be changed", new ZDate(2009, 12, 31), tariff1Reloaded.UE_DateTo);

			tariffs = newFactory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "0303695085"));
			AssertEquals(1, tariffs.Length);

			var tariff2Reloaded = tariffs[0];
			AssertEquals("0303695085", tariff2Reloaded.UE_Tariff);
			AssertEquals(new ZDate(2007, 02, 03), tariff2Reloaded.UE_DateFrom);
			AssertEquals("Expire Date should be changed", new ZDate(2010, 06, 18), tariff2Reloaded.UE_DateTo);

			tariffs = newFactory.Load<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, "0401310500"));
			AssertEquals(2, tariffs.Length);

			var query = new ZQuery(USCTariffSchema.UE_Tariff, "0401310500");
			query.AddToFilter(USCTariffSchema.UE_DateFrom, new ZDate(2010, 01, 01));
			query.AddToFilter(USCTariffSchema.UE_DateTo, new ZDate(2099, 12, 31));
			var tariff3Reloaded = newFactory.LoadTop1<USCTariff>(query);
			AssertNotNull(tariff3Reloaded);

			query = new ZQuery(USCTariffSchema.UE_Tariff, "0401310500");
			query.AddToFilter(USCTariffSchema.UE_DateFrom, new ZDate(2009, 02, 01));
			query.AddToFilter(USCTariffSchema.UE_DateTo, new ZDate(2009, 12, 31));
			tariff3Reloaded = newFactory.LoadTop1<USCTariff>(query);
			AssertNotNull(tariff3Reloaded);
		}

		public void TestProcessOGACodesForDeletedTariff()
		{
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1001100090";
			tariff.UE_DateFrom = new ZDate(1999, 01, 01);
			tariff.UE_DateTo = new ZDate(1999, 12, 31);
			tariff.UE_OGACodes = "FD1";

			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = "1001902010";
			tariff2.UE_DateFrom = new ZDate(1999, 01, 01);
			tariff2.UE_DateTo = new ZDate(1999, 12, 31);
			tariff2.UE_OGACodes = "FD1";

			var message = Factory.New<MQEDIMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.ExtractReferenceFiles;
			message.EM_MessageNum = "~15000";
			message.EM_MessageText = "B011704286FI                                               YASYUSPRD_249424     F1100103                                                                        Y  1704286FI00001";
			message.EM_Status = "SNT";
			Factory.Save();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_SystemCreateUser = "~BP";

			var message2 = Factory.New<MQEDIMessage>();
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem;
			message2.EM_MessageNum = "~15000";
			message2.EM_MessageText = "B013005062FR                                               TLRPDXPDX_2188       F1109901                                                                        V11001100090D0101991231991KG       1WHEAT/MESLIN-DURUM WHEAT,O    000000670000  V11001100091A0101991231991KG       1WHT/MES,DW,OTH,GR1,V KL CT    000000670000  V21001100091000000000000000000000000000001500000000000000000000000000000        V31001100091                                        E J A+CAILMX                V51001100091MX000000300000000000000000000000000000                              V11001902010D0101991231991KG       1OTHER RED SPRING WHEAT GRA    000000420000  Y  1704286FI00001";
			message2.EM_Status = "QUE";

			Factory.Save();

			new USRIncomingMessageProcessor().ExecuteBatch();

			var newFactory = new BusinessObjectFactory();

			var query = new ZQuery(USCTariffSchema.UE_Tariff, "1001100090");
			query.AddToFilter(USCTariffSchema.UE_DateFrom, new ZDateTime(1999, 01, 01));
			query.AddToFilter(USCTariffSchema.UE_DateTo, new ZDateTime(1999, 12, 31));
			tariff = newFactory.LoadTop1<USCTariff>(query);
			AssertNull(tariff);

			query = new ZQuery(USCTariffSchema.UE_Tariff, "1001100091");
			query.AddToFilter(USCTariffSchema.UE_DateFrom, new ZDateTime(1999, 01, 01));
			query.AddToFilter(USCTariffSchema.UE_DateTo, new ZDateTime(1999, 12, 31));
			tariff = newFactory.LoadTop1<USCTariff>(query);
			AssertEquals("", tariff.UE_OGACodes);
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 0; }
		}
	}
}
