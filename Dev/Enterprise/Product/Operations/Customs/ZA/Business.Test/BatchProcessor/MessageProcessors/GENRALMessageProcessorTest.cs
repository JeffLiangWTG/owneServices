using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class GENRALMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcessGenericMessage()
		{
			const string GENRAL_Message = @"UNH+00000000155033+GENRAL:D:16A:UN:ZZZ01'BGM+719:::+4:TST001+55'DTM+706:20211018161937:204'NAD+MR+20507309WTG'FTX+AAI+++Message 1:Message 2:Message 3:Message 4:Message 5'UNT+19+00000000155033'";
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			var testMessage = Factory.NewWithValidTestData<GENRALMessageForTest>();
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = GENRAL_Message;
			testMessage.EM_MessageNum = "IN1";
			Factory.Save();
			CombineAssertions("New", () =>
			{
				new GENRALMessageProcessor(logger).ProcessMessage(testMessage);
				Factory.Save();
				AssertEquals("EM_Status should be 'PRS'", EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals("EM_MessageOwner should be '20507309WTG' (NAD)", "20507309WTG", testMessage.EM_MessageOwner);
				AssertEquals("EM_MessageSubType should be 'TST' (BGM)", "TST", testMessage.EM_MessageSubType);
				AssertContains("EM_MessageInterpretation", @"<body style='font-family: arial;'>
<h3>GENRAL message</h3>
<p><b><pre><i>Message 1
Message 2
Message 3
Message 4
Message 5</i></pre></b></p>
</body>", testMessage.EM_MessageInterpretation);
			});
		}

		public void TestProcessUCRMessage()
		{
			const string GENRAL_Message = "UNH+1+GENRAL:D:16A:UN:GEN001'BGM+961:::SACU Import(s) unmatched UCR+1:UCR001+55'" +
				"DTM+706:20220407141706:204'" +
				"NAD+MR+00505655TST'" +
				"FTX+AAI+++Unmatched SACU UCR?: LRN=00505655GOL20220330132167, UCR=1SZ123456780TCUSSZLAV22E5314S, Arrived=2022-04-04 21?:41:Unmatched SACU UCR?: LRN=00505655GOL20220307129483, UCR=1SZ123456780TCUSSZLAV22E3773S, Arrived=2022-03-10 08?:14:Unmatched SACU UCR?: LRN=00505655GOL20220330132174, UCR=1SZ123456780TCUSSZLAV22E5319S, Arrived=2022-04-06 20?:07:Unmatched SACU UCR?: LRN=00505655GOL20220401132648, UCR=1SZ123456780TCUSSZLAV22E5619S, Arrived=2022-04-04 18?:11:Unmatched SACU UCR?: LRN=00505655GOL20220307129470, UCR=1SZ123456780TCUSSZLAV22E3764S, Arrived=2022-03-09 19?:51'" +
				"FTX+AAI+++Unmatched SACU UCR?: LRN=00505655GOL20220317130822, UCR=1SZ123456780TCUSSZLAV22E4664S, Arrived=2022-03-21 12?:23:Unmatched SACU UCR?: LRN=00505655GOL20220401132644, UCR=1SZ123456780TCUSSZLAV22E5617S, Arrived=2022-04-04 17?:05:Unmatched SACU UCR?: LRN=00505655GOL20220319131032, UCR=1SZ123456780TCUSSZLAV22E4674S, Arrived=2022-03-21 12?:21:Unmatched SACU UCR?: LRN=00505655GOL20220308129710, UCR=1SZ123456780TCUSSZLAV22E3940S, Arrived=2022-03-15 15?:06:Unmatched SACU UCR?: LRN=00505655GOL20220307129485, UCR=1SZ123456780TCUSSZLAV22E3769S, Arrived=2022-03-10 07?:12'" +
				"UNT+9+1'";
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			var testMessage = Factory.NewWithValidTestData<GENRALMessageForTest>();
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = GENRAL_Message;
			testMessage.EM_MessageNum = "IN1";
			Factory.Save();
			CombineAssertions(() =>
			{
				new GENRALMessageProcessor(logger).ProcessMessage(testMessage);
				Factory.Save();
				AssertEquals("EM_Status should be 'PRS'", EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals("EM_MessageOwner should be '00505655TST' (NAD)", "00505655TST", testMessage.EM_MessageOwner);
				AssertEquals("EM_MessageSubType should be 'UCR' (BGM)", GenralMessageSubTypeList.Codes.UCR, testMessage.EM_MessageSubType);
				AssertContains("EM_MessageInterpretation", @"<body style='font-family: arial;'>
<h3>GENRAL message - Unmatched UCR Numbers</h3>
<table>
<tr>
<th style=""border: none;"" />
<th>LRN</th>
<th>UCR</th>
<th>Arrived</th>
</tr>
<tr>
<td>1</td>
<td>00505655GOL20220330132167</td>
<td>1SZ123456780TCUSSZLAV22E5314S</td>
<td>2022-04-04 21:41</td>
</tr>
<tr>
<td>2</td>
<td>00505655GOL20220307129483</td>
<td>1SZ123456780TCUSSZLAV22E3773S</td>
<td>2022-03-10 08:14</td>
</tr>
<tr>
<td>3</td>
<td>00505655GOL20220330132174</td>
<td>1SZ123456780TCUSSZLAV22E5319S</td>
<td>2022-04-06 20:07</td>
</tr>
<tr>
<td>4</td>
<td>00505655GOL20220401132648</td>
<td>1SZ123456780TCUSSZLAV22E5619S</td>
<td>2022-04-04 18:11</td>
</tr>
<tr>
<td>5</td>
<td>00505655GOL20220307129470</td>
<td>1SZ123456780TCUSSZLAV22E3764S</td>
<td>2022-03-09 19:51</td>
</tr>
<tr>
<td>6</td>
<td>00505655GOL20220317130822</td>
<td>1SZ123456780TCUSSZLAV22E4664S</td>
<td>2022-03-21 12:23</td>
</tr>
<tr>
<td>7</td>
<td>00505655GOL20220401132644</td>
<td>1SZ123456780TCUSSZLAV22E5617S</td>
<td>2022-04-04 17:05</td>
</tr>
<tr>
<td>8</td>
<td>00505655GOL20220319131032</td>
<td>1SZ123456780TCUSSZLAV22E4674S</td>
<td>2022-03-21 12:21</td>
</tr>
<tr>
<td>9</td>
<td>00505655GOL20220308129710</td>
<td>1SZ123456780TCUSSZLAV22E3940S</td>
<td>2022-03-15 15:06</td>
</tr>
<tr>
<td>10</td>
<td>00505655GOL20220307129485</td>
<td>1SZ123456780TCUSSZLAV22E3769S</td>
<td>2022-03-10 07:12</td>
</tr>
</table>
</body>", testMessage.EM_MessageInterpretation);
			});
		}

		public void TestProcessPENMessage()
		{
			const string GENRAL_Message = "UNH+1+GENRAL:D:16A:UN:GEN001'" +
				"BGM+719:::RCG CUSCAR ePenalty details+4:PEN001+55'" +
				"DTM+706:20220623040939:204'" +
				"NAD+MR+20507309WTG'" +
				"FTX+AAI+++Type=HOUSE, HWB=20507309216713299, TDN=    MAEU216713299, LRN=20507309CTN20220511529205, Mode=SEA, Dir=IMP, EvalDate=2022-06-20:Type=HOUSE, HWB=20507309EDC0441850, TDN=    MAEU216992672, LRN=00281124DBN20220510290364, Mode=SEA, Dir=IMP, EvalDate=2022-06-20:Type=HOUSE, HWB=20507309217867177, TDN=    MAEU217867177, LRN=21662297DBN20220511530100, Mode=SEA, Dir=IMP, EvalDate=2022-06-20:Type=HOUSE, HWB=20507309217341636, TDN=    MAEU217341636, LRN=20507309DBN20220511530195, Mode=SEA, Dir=IMP, EvalDate=2022-06-20:Type=HOUSE, HWB=20507309EDC0441840, TDN=    MAEU217249456, LRN=00281124DBN20220511290847, Mode=SEA, Dir=IMP, EvalDate=2022-06-20'" +
				"FTX+AAI+++Type=HOUSE, HWB=20507309CLT0055147, TDN=    MSC MEDUU4838964, LRN=20533463DBN20220511049273, Mode=SEA, Dir=IMP, EvalDate=2022-06-20:Type=HOUSE, HWB=20507309AOI0051476A, TDN=057-07665265, LRN=20533463JSA20220603050325, Mode=AIR, Dir=IMP, EvalDate=2022-06-20:Type=HOUSE, HWB=20507309AOI0051476B, TDN=057-07665265, LRN=20533463JSA20220603050326, Mode=AIR, Dir=IMP, EvalDate=2022-06-20:Type=HOUSE, HWB=205073096ZAV0182757, TDN=071-44648494, LRN=21407472JSA20220606055195, Mode=AIR, Dir=IMP, EvalDate=2022-06-20:Type=HOUSE, HWB=20507309HA90142310, TDN=    DAL SHAMF6677, LRN=20507309PEZ20220510528911, Mode=SEA, Dir=IMP, EvalDate=2022-06-21'" +
				"UNT+10+1'";
			var logger = InboundInterchangeProcessorTest.GetNewLoggerForTesting() as LoggingInformationForTesting;
			var testMessage = Factory.NewWithValidTestData<GENRALMessageForTest>();
			testMessage.EM_ReceiveTransmit = "RCV";
			testMessage.EM_ApplicationCode = "ZAC";
			testMessage.EM_Status = "QUE";
			testMessage.EM_MessageText = GENRAL_Message;
			testMessage.EM_MessageNum = "IN1";
			Factory.Save();
			CombineAssertions(() =>
			{
				new GENRALMessageProcessor(logger).ProcessMessage(testMessage);
				Factory.Save();
				AssertEquals("EM_Status should be 'PRS'", EDIMessage.Status.ProcessedOK, testMessage.EM_Status);
				AssertEquals("EM_MessageOwner should be '20507309WTG' (NAD)", "20507309WTG", testMessage.EM_MessageOwner);
				AssertEquals("EM_MessageSubType should be 'PEN' (BGM)", GenralMessageSubTypeList.Codes.PEN, testMessage.EM_MessageSubType);
				AssertContains("EM_MessageInterpretation", @"<body style='font-family: arial;'>
<h3>GENRAL message - RCG Reporting Deviations</h3>
<table>
<tr>
<th style=""border: none;"" />
<th>Type</th>
<th>HWB</th>
<th>TDN</th>
<th>LRN</th>
<th>Mode</th>
<th>Direction</th>
<th>Evaluation Date</th>
</tr>
<tr>
<td>1</td>
<td>HOUSE</td>
<td>20507309216713299</td>
<td>MAEU216713299</td>
<td>20507309CTN20220511529205</td>
<td>SEA</td>
<td>IMP</td>
<td>2022-06-20</td>
</tr>
<tr>
<td>2</td>
<td>HOUSE</td>
<td>20507309EDC0441850</td>
<td>MAEU216992672</td>
<td>00281124DBN20220510290364</td>
<td>SEA</td>
<td>IMP</td>
<td>2022-06-20</td>
</tr>
<tr>
<td>3</td>
<td>HOUSE</td>
<td>20507309217867177</td>
<td>MAEU217867177</td>
<td>21662297DBN20220511530100</td>
<td>SEA</td>
<td>IMP</td>
<td>2022-06-20</td>
</tr>
<tr>
<td>4</td>
<td>HOUSE</td>
<td>20507309217341636</td>
<td>MAEU217341636</td>
<td>20507309DBN20220511530195</td>
<td>SEA</td>
<td>IMP</td>
<td>2022-06-20</td>
</tr>
<tr>
<td>5</td>
<td>HOUSE</td>
<td>20507309EDC0441840</td>
<td>MAEU217249456</td>
<td>00281124DBN20220511290847</td>
<td>SEA</td>
<td>IMP</td>
<td>2022-06-20</td>
</tr>
<tr>
<td>6</td>
<td>HOUSE</td>
<td>20507309CLT0055147</td>
<td>MSC MEDUU4838964</td>
<td>20533463DBN20220511049273</td>
<td>SEA</td>
<td>IMP</td>
<td>2022-06-20</td>
</tr>
<tr>
<td>7</td>
<td>HOUSE</td>
<td>20507309AOI0051476A</td>
<td>057-07665265</td>
<td>20533463JSA20220603050325</td>
<td>AIR</td>
<td>IMP</td>
<td>2022-06-20</td>
</tr>
<tr>
<td>8</td>
<td>HOUSE</td>
<td>20507309AOI0051476B</td>
<td>057-07665265</td>
<td>20533463JSA20220603050326</td>
<td>AIR</td>
<td>IMP</td>
<td>2022-06-20</td>
</tr>
<tr>
<td>9</td>
<td>HOUSE</td>
<td>205073096ZAV0182757</td>
<td>071-44648494</td>
<td>21407472JSA20220606055195</td>
<td>AIR</td>
<td>IMP</td>
<td>2022-06-20</td>
</tr>
<tr>
<td>10</td>
<td>HOUSE</td>
<td>20507309HA90142310</td>
<td>DAL SHAMF6677</td>
<td>20507309PEZ20220510528911</td>
<td>SEA</td>
<td>IMP</td>
<td>2022-06-21</td>
</tr>
</table>
</body>", testMessage.EM_MessageInterpretation);
			});
		}
	}
}
