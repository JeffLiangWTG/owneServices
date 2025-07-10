using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.BatchProcessor.Testing
{
	[TestedType(typeof(ZACInterchange))]
	public class ZACInterchangeTest : EDIInterchangeTest
	{
		public void TestProperties()
		{
			var interchange = Factory.New<ZACInterchange>();
			AssertEquals("EI_ApplicationCode", "ZAC", interchange.EI_ApplicationCode);
			AssertEquals("ShouldSendViaEHubCore", true, interchange.ShouldSendViaEHub);
		}

		public void TestGetMessageTypeToCreate()
		{
			var interchange = Factory.New<ZACInterchangeForTest>();
			var pairs = new Dictionary<string, Type>();
			pairs.Add(SARSEDIMessage.MessageTypeNames.CONTRL, typeof(CONTRLEDIMessage));
			pairs.Add(SARSEDIMessage.MessageTypeNames.CUSCAR, typeof(CUSCAREDIMessage));
			pairs.Add(SARSEDIMessage.MessageTypeNames.CUSRES, typeof(CUSRESEDIMessage));
			pairs.Add(SARSEDIMessage.MessageTypeNames.CUSRES_CALINF, typeof(CUSRESEDIMessage));
			pairs.Add(SARSEDIMessage.MessageTypeNames.CUSRES_COSTCO, typeof(CUSRESEDIMessage));
			pairs.Add(SARSEDIMessage.MessageTypeNames.CUSRES_CUSCAR, typeof(CUSRESEDIMessage));
			pairs.Add(SARSEDIMessage.MessageTypeNames.CUSRES_EXP_RA, typeof(CUSRESEDIMessage));
			pairs.Add(SARSEDIMessage.MessageTypeNames.CUSRES_GOVGIO, typeof(CUSRESEDIMessage));
			pairs.Add(SARSEDIMessage.MessageTypeNames.CUSRES_GIO, typeof(CUSRESEDIMessage));
			pairs.Add(SARSEDIMessage.MessageTypeNames.CUSRES_REQDOC, typeof(CUSRES_REQDOCEDIMessage));
			pairs.Add(SARSEDIMessage.MessageTypeNames.EXPORT, typeof(CUSRESEDIMessage));
			pairs.Add(SARSEDIMessage.MessageTypeNames.STATAC_DAILY, typeof(STATACEDIMessage));
			pairs.Add(SARSEDIMessage.MessageTypeNames.STATAC_DETAIL, typeof(STATACEDIMessage));
			// Add pair for new message type

			foreach (var pair in pairs)
			{
				interchange.EI_HeaderText = $"UNB+UNOB:4+SARSCART+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20161007:1522+3634++{pair.Key}'";
				AssertEquals($"Expected type for application reference {pair.Key}", pair.Value, interchange.GetMessageTypeToCreateExposed);
			}

			interchange.EI_HeaderText = "";
			AssertEquals("Expected type for application reference not found or not recognised", typeof(EDIMessage), interchange.GetMessageTypeToCreateExposed);
		}

		public void TestGENRALInterchangeCreatesGENRALMessage()
		{
			var interchange = Factory.New<ZACInterchangeForTest>();
			interchange.EI_HeaderText = $"UNB+UNOB:4+12345678ABC::ABCDEFGHIJKLMNOP:SPCAS2+SARSGENT+20211026:1157+1234567890++GENRAL++1++1'";
			AssertEquals($"Expected type for application reference {SARSEDIMessage.MessageTypeNames.GENRAL}", typeof(GENRALMessage), interchange.GetMessageTypeToCreateExposed);
		}

		public void TestGetCorrectMessageTypeFromInterchangeWithAndWithoutElementsAfterApplicationReference()
		{
			ZACInterchangeForTest interchange1;
			ZACInterchangeForTest interchange2;
			ZACInterchangeForTest interchange3;
			SetUpTwoRealInterchangesForTest(Factory, out interchange1, out interchange2, out interchange3);
			AssertEquals(typeof(CUSRESEDIMessage), interchange3.GetMessageTypeToCreateExposed);
			AssertEquals(typeof(CUSRESEDIMessage), interchange2.GetMessageTypeToCreateExposed);
			AssertEquals(typeof(CUSRESEDIMessage), interchange1.GetMessageTypeToCreateExposed);
		}

		public static void SetUpTwoRealInterchangesForTest(BusinessObjectFactory factory, out ZACInterchangeForTest interchange1, out ZACInterchangeForTest interchange2, out ZACInterchangeForTest interchange3)
		{
			var interchangeWithoutFutherElements1 = new[] { @"UNB+UNOB:4+SARSDECT+00300508WTG::DDDDDDHHHHHHHLLL:WTGAS2+20180625:1659+6++CUSRES'", @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+00300508JSA20180618000015:0'
DTM+178:20180613:102'
TDT+20+LU8753+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+75::ZZZ'
GIS+6:120:ZZZ:N'
NAD+AG+00300508'
RFF+BH:203659268HNY357'
RFF+AAS:972-25733643'
DTM+137:20180610:102'
RFF+ACD:24'
ERP+1:0'
ERC+1008::ZZZ'
FTX+AAO+++ FIELD(Transport Document Info/House Waybill Info) DESCR(972 Is Not A:Valid Option From Codelist MasterCargoCarrierAir)'
TAX+3+CUS:107:ZZZ'
MOA+161:0'
CNT+7:315.00'
CNT+11:1'
UNT+20+1'".Replace(System.Environment.NewLine, ""),
"UNZ+1+6'"
			};
			var interchangeWithFurtherElements2 = new[] { @"UNB+UNOB:4+SARSDEC+21504353WTG::L5J0K3O4A6A5E5R9:WTGAS2+20180618:2034+384++CUSRES+++SHIPPLERWTG'", @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+21504353JSA20180618000216:0'
DTM+178:20180618:102'
TDT+20+QR8775+4+++++::: '
LOC+22+JSA::ZZZ'
LOC+14+A1::ZZZ'
GIS+33:120:ZZZ:N'
NAD+AG+21504353'
RFF+BH:21504353GFMI/AE/0073/2018-19'
DTM+137:20180615:102'
RFF+AAS:157-39182172'
DTM+137:20180615:102'
RFF+ABT:JSA201806185096360'
DTM+137:20180618:102'
RFF+ACD:217'
RFF+AAV:277191023'
ERP+6:0'
ERC+100::ZZZ'
FTX+AAO+++21504353WTG-678352ed-7df8-443d-8e5e-3bfddb32778e'
TAX+3+CUS:107:ZZZ'
MOA+161:35164'
CNT+7:564.00'
CNT+11:24'
UNT+24+1'".Replace(System.Environment.NewLine, ""),
"UNZ+1+384'" };

			var interchangeForCUSRES_EXP_RA = new[] { @"UNB+UNOB:4+SARSDEC+50203456::O2X0N8R0J8G5V5E1:WTGAS2+20210821:1836+2844++CUSRES-EXP-RA+++TRASHIDURWTG'", @"UNH+1+CUSRES:D:96B:UN:ZZZ01'
BGM+962+20716711DBN20210821003066:0'
DTM+178:20210821:102'
TDT+20+ +3+++++:::JX55VTGP  JZ20KNGP  JZ20KPGP'
LOC+22+DBN::ZZZ'
GIS+1:120:ZZZ:N'
NAD+AG+20716711'
RFF+AAS:TMX0001/08/2021'
DTM+137:20210821:102'
RFF+ABT:DBN202108215046677'
DTM+137:20210821:102'
RFF+UCN:1ZA21742054CINVLI21LS023-1M'
RFF+ACD:EZC00000050000760'
TAX+3+CUS:107:ZZZ'
MOA+161:493533'
CNT+7:34974.00'
CNT+11:18'
UNT+18+1'".Replace(System.Environment.NewLine, ""),
"UNZ+1+2813'" };

			interchange3 = factory.New<ZACInterchangeForTest>();
			interchange2 = factory.New<ZACInterchangeForTest>();
			interchange1 = factory.New<ZACInterchangeForTest>();
			interchange3.EI_HeaderText = interchangeForCUSRES_EXP_RA[0];
			interchange2.EI_HeaderText = interchangeWithFurtherElements2[0];
			interchange1.EI_HeaderText = interchangeWithoutFutherElements1[0];
			interchange3.EI_BodyText = interchangeForCUSRES_EXP_RA[1];
			interchange2.EI_BodyText = interchangeWithFurtherElements2[1];
			interchange1.EI_BodyText = interchangeWithoutFutherElements1[1];
			interchange3.EI_FooterText = interchangeForCUSRES_EXP_RA[2];
			interchange2.EI_FooterText = interchangeWithFurtherElements2[2];
			interchange1.EI_FooterText = interchangeWithoutFutherElements1[2];
			interchange3.EI_InterchangeNum = "Z";
			interchange2.EI_InterchangeNum = "X";
			interchange1.EI_InterchangeNum = "Y";
			factory.Save();
			interchange3.EI_ReceiveTransmit = interchange2.EI_ReceiveTransmit = interchange1.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			factory.Save();
		}

		public void TestDateTimeOfPreparation()
		{
			var interchange = Factory.New<ZACInterchange>();

			interchange.EI_HeaderText = $"UNB+UNOB:4+SARSCART+00505655TST::AAAAAAAAAAAAAABB:CORAS2+20161007:1522+3634++{SARSEDIMessage.MessageTypeNames.CONTRL}++++1'";
			AssertEquals("Expected DateTimeOfPreparation", new ZDateTime(2016, 10, 07, 15, 22, 0), interchange.DateTimeOfPreparation);

			interchange.EI_HeaderText = "";
			AssertEquals("Expected DateTimeOfPreparation", ZDateTime.Empty, interchange.DateTimeOfPreparation);
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<ZACInterchange>();

		public class ZACInterchangeForTest : ZACInterchange
		{
			public ZACInterchangeForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			public Type GetMessageTypeToCreateExposed => GetMessageTypeToCreate("");
		}
	}
}
