using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.CMDMessaging.Testing
{
	internal class CMDInboundTest : TestCase
	{
		public void TestIsValid()
		{
			CMDInbound inbound = new CMDInbound("asdfasdf");
			AssertEquals(false, inbound.IsValid);
			inbound = new CMDInbound(FNAMessage);
			AssertEquals(true, inbound.IsValid);
			inbound = new CMDInbound(FMAMessage);
			AssertEquals(true, inbound.IsValid);
			inbound = new CMDInbound(CMAMessage);
			AssertEquals(true, inbound.IsValid);
			inbound = new CMDInbound(OtherMessage1);
			AssertEquals(false, inbound.IsValid);
			inbound = new CMDInbound(OtherMessage2);
			AssertEquals(false, inbound.IsValid);
			inbound = new CMDInbound(CMAMessage2);
			AssertEquals("Not enough lines, not a valid CMA", false, inbound.IsValid);
			// Check we can handle messages whose CIMP identified is prefixed with CMD, e.g. CMDFNA
			inbound = new CMDInbound("CMD" + FNAMessage);
			Assert(inbound.IsValid);
			AssertEquals(CMDInbound.Constants.FNA, inbound.StandardMessageIdentifier);
			inbound = new CMDInbound("CMD" + FMAMessage);
			Assert(inbound.IsValid);
			AssertEquals(CMDInbound.Constants.FMA, inbound.StandardMessageIdentifier);
			inbound = new CMDInbound("CMD" + CMAMessage);
			Assert(inbound.IsValid);
			AssertEquals(CMDInbound.Constants.CMA, inbound.StandardMessageIdentifier);
		}

		public void TestMasterBillNumber()
		{
			CMDInbound inbound = new CMDInbound(OtherMessage1);
			AssertEquals("Should be empty, not a valid reply message", "", inbound.MasterBillNumber);
			inbound = new CMDInbound(OtherMessage2);
			AssertEquals("Should be empty, not a valid reply message", "", inbound.MasterBillNumber);
			inbound = new CMDInbound(FNAMessage);
			AssertEquals("088-12345101", inbound.MasterBillNumber);
			inbound = new CMDInbound(FMAMessage);
			AssertEquals("", inbound.MasterBillNumber);
			inbound = new CMDInbound(CMAMessage);
			AssertEquals("088-12345104", inbound.MasterBillNumber);
			inbound = new CMDInbound(CMAMessage2);
			AssertEquals("Should be empty, not a valid CMA message", "", inbound.MasterBillNumber);
		}

		public void TestHouseBillNumber()
		{
			CMDInbound inbound = new CMDInbound(OtherMessage1);
			AssertEquals("Should be empty, not a valid reply message", "", inbound.HouseBillNumber);
			inbound = new CMDInbound(OtherMessage2);
			AssertEquals("Should be empty, not a valid reply message", "", inbound.HouseBillNumber);
			inbound = new CMDInbound(FNAMessage);
			AssertEquals("SERIAL123", inbound.HouseBillNumber);
			inbound = new CMDInbound(FMAMessage);
			AssertEquals("", inbound.HouseBillNumber);
			inbound = new CMDInbound(CMAMessage);
			AssertEquals("SERIAL024", inbound.HouseBillNumber);
			inbound = new CMDInbound(CMAMessage2);
			AssertEquals("Should be empty, not a valid CMA message", "", inbound.HouseBillNumber);
		}

		public void TestSender()
		{
			CMDInbound inbound = new CMDInbound(FNAMessage);
			AssertEquals("CIAS", inbound.Sender);
			inbound = new CMDInbound(CMAMessage);
			AssertEquals("SATS", inbound.Sender);
			inbound = new CMDInbound(FMAMessage);
			AssertEquals("CCN", inbound.Sender);
		}

		public void TestMessageText()
		{
			CMDInbound inbound = new CMDInbound(FNAMessage);
			AssertEquals(FNAMessage, inbound.MessageText);
			inbound = new CMDInbound(FMAMessage);
			AssertEquals(FMAMessage, inbound.MessageText);
			inbound = new CMDInbound(FNAMessage);
			AssertEquals(FNAMessage, inbound.MessageText);
		}

		public void TestMessageTextWithoutRoutingInfo()
		{
			CMDInbound inbound = new CMDInbound(FNAMessage);
			AssertEquals(FNAMessageWithoutRoutingInfo, inbound.MessageTextWithoutRoutingInfo);
			inbound = new CMDInbound(FMAMessage);
			AssertEquals(FMAMessageWithoutRoutingInfo, inbound.MessageTextWithoutRoutingInfo);
			inbound = new CMDInbound(CMAMessage);
			AssertEquals(CMAMessageWithoutRoutingInfo, inbound.MessageTextWithoutRoutingInfo);
			inbound = new CMDInbound(CMAMessage + (char)4);
			AssertEquals(CMAMessageWithoutRoutingInfo, inbound.MessageTextWithoutRoutingInfo);
		}

		public void TestIsErrorMessage()
		{
			CMDInbound inbound = new CMDInbound(FNAMessage);
			AssertEquals(true, inbound.IsErrorMessage);
			inbound = new CMDInbound(FMAMessage);
			AssertEquals(false, inbound.IsErrorMessage);
			inbound = new CMDInbound(CMAMessage);
			AssertEquals(false, inbound.IsErrorMessage);
		}

		#region FNA Message
		const string FNAMessage = "FNA\r\n" + "CIAS\r\n" + "FNA\r\n" + "ACK/503 INVALID CRIA NO - SND\r\n" + "CMD/2\r\n" + "A/N/N\r\n" + "MWB/088-12345101JKTJTY/T77K234.99\r\n" + "/AS PER MANIFEST\r\n" + "FLT/QF8898/01JAN\r\n" + "HWB/SERIAL123/89/L56.78\r\n" + "/LUXURY CAR A/HAR001\r\n" + "/ELEPHANT WITH TRUNKS/HAR005\r\n" + "/BIG ELEPHANTS/HAR110\r\n" + "/SMALL ELEPHANTS/HAR111\r\n" + "SHP/FREIGHT SENDER 101\r\n" + "/111 BOURKE ROAD ALEXANDRIA NSW 2000 AUSTRALIA CNE/FREIGHT RECEIVER 102\r\n" + "/101 BLABLABLA STREET YUMMMMM CAIRO XXY131Z EGYPT\r\n" + "TPT/TDB/TDB0000101\r\n" + "/TDB0000102\r\n" + "/TDB0000103\r\n" + "/TDB0000104\r\n" + "CED/Y/CED0000101/CED0000102/CED0000103\r\n" + "IMW/111-12345678\r\n" + "EXP/TS/BLABLABLA\r\n" + "SND/TEST USER/TEST COMPANY 123\r\n" + "/TEST/TEST/TEST\r\n";
		const string FNAMessageWithoutRoutingInfo = "FNA\r\n" + "ACK/503 INVALID CRIA NO - SND\r\n" + "CMD/2\r\n" + "A/N/N\r\n" + "MWB/088-12345101JKTJTY/T77K234.99\r\n" + "/AS PER MANIFEST\r\n" + "FLT/QF8898/01JAN\r\n" + "HWB/SERIAL123/89/L56.78\r\n" + "/LUXURY CAR A/HAR001\r\n" + "/ELEPHANT WITH TRUNKS/HAR005\r\n" + "/BIG ELEPHANTS/HAR110\r\n" + "/SMALL ELEPHANTS/HAR111\r\n" + "SHP/FREIGHT SENDER 101\r\n" + "/111 BOURKE ROAD ALEXANDRIA NSW 2000 AUSTRALIA CNE/FREIGHT RECEIVER 102\r\n" + "/101 BLABLABLA STREET YUMMMMM CAIRO XXY131Z EGYPT\r\n" + "TPT/TDB/TDB0000101\r\n" + "/TDB0000102\r\n" + "/TDB0000103\r\n" + "/TDB0000104\r\n" + "CED/Y/CED0000101/CED0000102/CED0000103\r\n" + "IMW/111-12345678\r\n" + "EXP/TS/BLABLABLA\r\n" + "SND/TEST USER/TEST COMPANY 123\r\n" + "/TEST/TEST/TEST\r\n";
		#endregion
		#region FMA Message
		const string FMAMessage = "FMA\r\n" + "CCN\r\n" + "FMA\r\n" + "CMD/2\r\n" + "A/N/N\r\n";
		const string FMAMessageWithoutRoutingInfo = "FMA\r\n" + "CMD/2\r\n" + "A/N/N\r\n";
		#endregion
		#region CMA Message
		const string CMAMessage = "CMA\r\n" + "SATS\r\n" + "CMA\r\n" + "CMD/2\r\n" + "A/N/N\r\n" + "MWB/088-12345104JKTJTY/T77K234.99\r\n" + "/AS PER MANIFEST\r\n" + "FLT/QF8898/01JAN\r\n" + "HWB/SERIAL024/89/L56.78\r\n" + "/LUXURY CAR A/HAR001\r\n" + "/ELEPHANT WITH TRUNKS/HAR005\r\n" + "/BIG ELEPHANTS/HAR110\r\n" + "/SMALL ELEPHANTS/HAR111\r\n" + "SHP/FREIGHT SENDER 101\r\n" + "/111 BOURKE ROAD ALEXANDRIA NSW 2000 AUSTRALIA CNE/FREIGHT RECEIVER 102\r\n" + "/101 BLABLABLA STREET YUMMMMM CAIRO XXY131Z EGYPT\r\n" + "TPT/TDB/TDB0000101\r\n" + "/TDB0000102\r\n" + "/TDB0000103\r\n" + "/TDB0000104\r\n" + "CED/Y/CED0000101/CED0000102/CED0000103\r\n" + "IMW/111-12345678\r\n" + "EXP/TS/BLABLABLA\r\n" + "SND/TEST USER/TEST COMPANY 123\r\n" + "/TEST/TEST/TEST\r\n";
		const string CMAMessageWithoutRoutingInfo = "CMA\r\n" + "CMD/2\r\n" + "A/N/N\r\n" + "MWB/088-12345104JKTJTY/T77K234.99\r\n" + "/AS PER MANIFEST\r\n" + "FLT/QF8898/01JAN\r\n" + "HWB/SERIAL024/89/L56.78\r\n" + "/LUXURY CAR A/HAR001\r\n" + "/ELEPHANT WITH TRUNKS/HAR005\r\n" + "/BIG ELEPHANTS/HAR110\r\n" + "/SMALL ELEPHANTS/HAR111\r\n" + "SHP/FREIGHT SENDER 101\r\n" + "/111 BOURKE ROAD ALEXANDRIA NSW 2000 AUSTRALIA CNE/FREIGHT RECEIVER 102\r\n" + "/101 BLABLABLA STREET YUMMMMM CAIRO XXY131Z EGYPT\r\n" + "TPT/TDB/TDB0000101\r\n" + "/TDB0000102\r\n" + "/TDB0000103\r\n" + "/TDB0000104\r\n" + "CED/Y/CED0000101/CED0000102/CED0000103\r\n" + "IMW/111-12345678\r\n" + "EXP/TS/BLABLABLA\r\n" + "SND/TEST USER/TEST COMPANY 123\r\n" + "/TEST/TEST/TEST\r\n";
		const string CMAMessage2 = "CMA\r\nSATS";
		#endregion
		#region Other Messages
		const string OtherMessage1 = "BLA\r\n" + "CCN\r\n" + "BLA\r\n" + "ACK/503 INVALID CRIA NO - SND\r\n" + "CMD/2\r\n" + "A/N/N\r\n";
		const string OtherMessage2 = "FSU\r\n" + "CCN\r\n" + "FSU\r\n" + "ACK/503 INVALID CRIA NO - SND\r\n" + "CMD/2\r\n" + "A/N/N\r\n";
		#endregion
	}
}
