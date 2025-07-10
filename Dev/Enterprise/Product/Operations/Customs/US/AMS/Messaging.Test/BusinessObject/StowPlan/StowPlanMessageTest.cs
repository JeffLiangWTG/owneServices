using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Messaging.Testing
{
	[TestedType(typeof(StowPlanMessage))]
	class StowPlanMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCountAcceptedContainersWhichPreviouslyNotAccepted()
		{
			var voyage = Factory.New<JobVoyage>();
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(1);
			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			destination.JB_E_ARV = ZDateTime.Today.AddDays(10);
			var transmitMsg1 = Factory.New<StowPlanMessage>();
			transmitMsg1.EM_MessageNum = "8074";
			transmitMsg1.EM_MessageText = @"UNH+8074+BAPLIE:D:95B:UN:SMDG20
BGM++8074+9
DTM+137:130812:101
TDT+20+1299+++ALPU:172:166+++2111111:146:11:NVO STEP 1
LOC+5+AUMEL:139:6
LOC+61+USLAX:139:6
DTM+133:130613:101
DTM+132:130616:101
LOC+147+1112233::5
MEA+WT++KG:3830
LOC+9+DEHAM
LOC+11+NZABY
EQD+CN+ANLU8463790+:::42G0
NAD+CA+0UAFORLON:172:166
DGS+IMD+0004A
LOC+147+3331122::5
MEA+WT++KG:3832
LOC+9+DEHAM
LOC+11+NZABY
EQD+CN+MOLU0383939+:::42G0
NAD+CA+0UAFORLON:172:166
DGS+IMD+0004A
UNT+23+8074".Replace("\r\n", "'");
			transmitMsg1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			transmitMsg1.EM_LinkedObject = destination;
			transmitMsg1.EM_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-4);
			var receiveMsg1 = Factory.New<StowPlanMessage>();
			receiveMsg1.EM_MessageText = @"UNH+1319961+CUSRES:D:05B:UN
BGM+294+8074
TDT+20+1299+1++8CAR:172:ZZZ:NVO STEP 1+++2111111:146:11:NVO STEP 1
RFF+AAA
DTM+133:20130613
LOC+5+AUMEL
RFF+AAA
DTM+132:20130616
LOC+61+USLAX
ERP+1
ERC+SBB
FTX+AAH+++VESSEL ARRIVAL DATE IN THE PAST
ERP+1
ERC+SBT
FTX+AAH+++VESSEL DEPARTURE DATE MORE THAN 30 DAYS IN THE PAST
ERP+1
ERC+S03
FTX+AAH+++ACCEPTED WITH WARNINGS
UNT+19+1319961".Replace("\r\n", "'");
			receiveMsg1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			receiveMsg1.EM_LinkedObject = destination;
			receiveMsg1.EM_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-3);
			var transmitMsg2 = Factory.New<StowPlanMessage>();
			transmitMsg2.EM_MessageNum = "8077";
			transmitMsg2.EM_MessageText = @"UNH+8077+BAPLIE:D:95B:UN:SMDG20
BGM++8077+9
DTM+137:131001:101
TDT+20+1299+++ALPU:172:166+++2111111:146:11:NVO STEP 1
LOC+5+AUMEL:139:6
LOC+61+USLAX:139:6
DTM+133:130613:101
DTM+132:130616:101
LOC+147+1112233::5
MEA+WT++KG:3830
LOC+9+DEHAM
LOC+11+NZABY
EQD+CN+ANLU8463790+:::42G0
NAD+CA+0UAFORLON:172:166
DGS+IMD+0004A
LOC+147+3331122::5
MEA+WT++KG:3832
LOC+9+DEHAM
LOC+11+NZABY
EQD+CN+MOLU0383939+:::42G0
NAD+CA+0UAFORLON:172:166
DGS+IMD+0004A
LOC+147+2233387::5
MEA+WT++KG:3
LOC+9+DEHAM
LOC+11+NZABY
EQD+CN+APLU1234564+:::4210
NAD+CA+ABC100065:172:166
DGS+IMD+0004A
UNT+30+8077".Replace("\r\n", "'");
			transmitMsg2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			transmitMsg2.EM_LinkedObject = destination;
			transmitMsg2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-2);
			var receiveMsg2 = Factory.New<StowPlanMessage>();
			receiveMsg2.EM_MessageText = @"UNH+1319961+CUSRES:D:05B:UN
BGM+294+8077
TDT+20+1299+1++8CAR:172:ZZZ:NVO STEP 1+++2111111:146:11:NVO STEP 1
RFF+AAA
DTM+133:20130613
LOC+5+AUMEL
RFF+AAA
DTM+132:20130616
LOC+61+USLAX
ERP+1
ERC+SBB
FTX+AAH+++VESSEL ARRIVAL DATE IN THE PAST
ERP+1
ERC+SBT
FTX+AAH+++VESSEL DEPARTURE DATE MORE THAN 30 DAYS IN THE PAST
ERP+1
ERC+S03
FTX+AAH+++ACCEPTED WITH WARNINGS
UNT+19+1319961".Replace("\r\n", "'");
			receiveMsg2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			receiveMsg2.EM_LinkedObject = destination;
			receiveMsg2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-1);
			AssertEquals(2, receiveMsg1.CountAcceptedContainersWhichPreviouslyNotAccepted());
			AssertEquals(1, receiveMsg2.CountAcceptedContainersWhichPreviouslyNotAccepted());
		}
	}
}
