using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class FTZAdmissionProcessorTest : ABIProcessorTest<FTZAdmissionProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		protected override void EndToEndCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Messages.Add(CreateRequestMessage());
			declaration.Messages.Add(CreateResponseMessage("B012501SV9NF                                               115039               90A153000111FETLJKOU2501N                                                       91B1 1  153000111FETLJKOU                  1111072333                           95  083 ZONE ADMISSION DATA ACCEPTED                                            Y  2501SV9NF00001"));
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("ZONE ADMISSION DATA ACCEPTED", sentMail.Body);
			AssertContains("FTZ Admission Response for B00001000", sentMail.Subject);
			AssertNotContains("<th>Carrier Code</th><th>Flight No</th><th>Arrival Date</th>", sentMail.Body);
			declaration.Reload();
			AssertEquals("Status", FTZMessageStatusList.Codes.ClearFTZAdmissionAdd, declaration.AdmissionStatus);
			AssertEquals("No specific dispositions received", 0, declaration.FTZDispositionCodes.Count);
		}

		public void TestNF10_01Message()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.FTZ;

			var messageNum = "HYEDUSDAT_0413";

			var messageTrx = Factory.New<MQEDIMessage>();
			messageTrx.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			messageTrx.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			messageTrx.EM_SystemCreateTimeUtc = new ZDateTime(2022, 04, 13);
			messageTrx.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData;
			messageTrx.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZAdmissionAdd;

			messageTrx.EM_MessageText =
@"B  1101SV9FT                                               HYEDUSDAT_0413       
10R4011231A121TST00253Y4901NSV90903ST1  073802-00093L99158-123456789            
11CRAIG SEELIG                            2155551234     01                     
20A40ABC CLIENT REP TEST AIR CAR0899           2021042920210429490120210429     
4077704293973                        BQN000000031        0000000100AR35705      
50000013920790500    AR000000100000KG                                           
5100000010000000000050120000000000N00000000                                     
60TEST                                         MIDUSCRAIMPCON                   
Y  1101SV9FT00007";
			dec.Messages.Add(messageTrx);
			Factory.Save();

			messageTrx.EM_MessageNum = messageNum;
			Factory.Save();

			var messageRcv = Factory.New<MQEDIMessage>();
			messageRcv.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			messageRcv.EM_SystemCreateTimeUtc = new ZDateTime(2022, 04, 13);
			messageRcv.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageRcv.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			messageRcv.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZAdmissionAdd;
			messageRcv.EM_MessageNum = messageNum;
			messageRcv.EM_MessageText = "B  1101SV9NF                                               HYEDUSDAT_0413       90A0610010A122SAS051844909N                                                     91B7 1  0610010A122SAS05184                2203281054    LBY0                   10A0610010A122SAS05184Y4909NB3M         66-053115000LBY066-094189600            95  009NO FTZ OPERATOR BOND ON FILE FOR ZONE                                    9501082ZONE ADMISSION DATA REJECTED                                             Y  1101SV9NF00000";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			var newFactory = new BusinessObjectFactory();
			var loadedDeclaration = newFactory.Load<JobDeclaration>(dec.PK);
			AssertEquals("NF Message is matched", 2, loadedDeclaration.Messages.Count);
			var receivedMsg = loadedDeclaration.Messages.OfType<MQEDIMessage>().FirstOrDefault(x => x.EM_MessageType == ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone);
			AssertContains("FTZFT10_01", humanFrendly, receivedMsg.EM_MessageInterpretation);
		}
		readonly string humanFrendly =
@"--------------AABIOutputB---------------
 Processing District Port Code (4-7)    :1101
 Filer Code (8-10)                      :SV9
 Application Identifier Code (11-12)    :NF
 Filer Preparers User Data Text (60-80) :HYEDUSDAT_0413

---------------FTZNF90_01---------------
 Admission Type (3-3)              :A
 Zone I D (4-12)                   :0610010A1
 Calendar Year (13-14)             :22
 Control Number (15-22)            :SAS05184
 Port Code (23-26)                 :4909
 Direct Delivery Indicator (27-27) :N

----------------FTZNF91-----------------
 Disposition Code (3-5)    :B7
 Reference Qualifier (6-8) :1
 Reference I D (9-43)      :0610010A122SAS05184
 Action Date (44-49)       :28-Mar-22
 Action Time (50-53)       :1054
 F I R M S I D (58-61)     :LBY0

---------------FTZFT10_01---------------
 Action Code (3-3)                                         :A
 Zone I D (4-12)                                           :0610010A1
 Calendar Year (13-14)                                     :22
 Control Number (15-22)                                    :SAS05184
 Expanded Zone I D Indicator (23-23)                       :Y
 Port Code (24-27)                                         :4909
 Direct Delivery Indicator (28-28)                         :N
 A B I Filer Code (29-31)                                  :B3M
 Zone Operator Identifierformerly I R S Identifier (41-52) :66-053115000
 F I R M S Identifier (53-56)                              :LBY0
 Applicant For Admission (57-68)                           :66-094189600

----------------FTZNF95-----------------
 Error Code (5-7) :009
 Remarks (8-67)   :NO FTZ OPERATOR BOND ON FILE FOR ZONE

----------------FTZNF95-----------------
 Narrative Message Type Code (3-4) :01
 Error Code (5-7)                  :082
 Remarks (8-67)                    :ZONE ADMISSION DATA REJECTED

--------------AABIOutputY---------------
 Processing District Port Code (4-7)    :1101
 Filer Code (8-10)                      :SV9
 Application Identifier Code (11-12)    :NF
 Output Transaction Image Count (13-17) :5";

		public void TestNFMessageMatched()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.FTZ;

			var messageNum = "HYEDUSDAT_1889";

			var messageTrx = Factory.New<MQEDIMessage>();
			messageTrx.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			messageTrx.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			messageTrx.EM_SystemCreateTimeUtc = new ZDateTime(2015, 09, 09);
			messageTrx.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData;
			messageTrx.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZAdmissionAdd;
			dec.Messages.Add(messageTrx);
			Factory.Save();

			messageTrx.EM_MessageNum = messageNum;
			Factory.Save();

			var messageRcv = Factory.New<MQEDIMessage>();
			messageRcv.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			messageRcv.EM_SystemCreateTimeUtc = new ZDateTime(2015, 12, 09);
			messageRcv.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageRcv.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			messageRcv.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZAdmissionAdd;
			messageRcv.EM_MessageNum = messageNum;
			messageRcv.EM_MessageText = "B011234BERNF                                               HYEDUSDAT_1889       90A0250C0015AAL0024N5203N                                                       91B1 1  0250C0015AAL0024N                  1512152329                           95  083 ZONE ADMISSION DATA ACCEPTED                                            Y  1234BERNF00001";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();

			var newFactory = new BusinessObjectFactory();
			var loadedResponseMessage = newFactory.Load<MQEDIMessage>(messageRcv.PK);
			AssertNotNull(loadedResponseMessage);

			var loadedDeclaration = newFactory.Load<JobDeclaration>(dec.PK);
			AssertEquals("NF Message is matched", 2, loadedDeclaration.Messages.Count);
		}

		public void TestProcessNF96Block()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Messages.Add(CreateRequestMessage());
			declaration.Messages.Add(CreateResponseMessage(
				"B012501SV9NF                                               115039               " +
				"90A153000111FETLJKOU2501N                                                       " +
				"91B1 1  153000111FETLJKOU                  1111072333                           " +
				"95  083 ZONE ADMISSION DATA ACCEPTED                                            " +
				"9601C001QF12720150627                                                           " +
				"Y  2501SV9NF00001"));
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("<th>Carrier Code</th><th>Flight No</th><th>Arrival Date</th>", sentMail.Body);
		}

		public void TestResponseWithDispositions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "FTZ";
			declaration.Messages.Add(CreateRequestMessage());
			declaration.Messages.Add(CreateResponseMessage("B012501SV9NF                                               115039               " +
				"90A153000111FETLJKOU2501N                                                       " +
				"91B1 1  153000111FETLJKOU                  1111072333                           " +
				"91B1 1  153000111FETLJKOU                  1111072333                           " +
				"91B4 1  153000111FETLJKOU                                                       " +
				"95  083 ZONE ADMISSION DATA ACCEPTED                                            " +
				"Y  2501SV9NF00001"));
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			declaration.Reload();
			AssertEquals("Status", FTZMessageStatusList.Codes.ClearFTZAdmissionAdd, declaration.AdmissionStatus);
			AssertEquals(1, declaration.FTZDispositionCodes.Count);
			AssertEquals(DispositionList.Codes.B4, declaration.FTZDispositionCodes[0].US_Code);
			AssertEquals(ZDateTime.Empty, declaration.FTZDispositionCodes[0].US_DispositionDate);

			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			Assert(reLoadJob.JE_EntryAuthorisationDate.IsEmpty);
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Enterprise.ZArchitecture.Business.AutoEvents.CustomsCleared.Code);
			Assert(!reLoadJob.Logs.HasLogWith(query));
		}

		public void TestResponseWithDispositionsUpdateEntryAuthorisationDate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "FTZ";
			declaration.Messages.Add(CreateRequestMessage());
			declaration.Messages.Add(CreateResponseMessage("B012501SV9NF                                               115039               " +
				"90A153000111FETLJKOU2501N                                                       " +
				"91BF 1  153000111FETLJKOU                  1111072330                           " +
				"91BF 1  153000111FETLJKOU                  1111072333                           " +
				"95      ZONE ADMISSION DATA ACCEPTED                                            " +
				"Y  2501SV9NF00001"));
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			declaration.Reload();
			var reLoadJob = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals(reLoadJob.JE_EntryAuthorisationDate, new ZDateTime(2011, 11, 7, 23, 33, 00));
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Enterprise.ZArchitecture.Business.AutoEvents.CustomsCleared.Code);
			Assert(reLoadJob.Logs.HasLogWith(query));
		}

		public void TestAcceptedWithSensusWarning()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Messages.Add(CreateRequestMessage());
			declaration.Messages.Add(CreateResponseMessage("B018888XJ5NF                                               115039               90A153000111000000012501N                                                       91B2 1  15300011100000001                  1112111654                           20A41QF  QANTAS AIRWAYS LIMITED QF2332         2011110420111104250120111104     4008139100014                                            0000000001AU60267      950327T*CENSUS*INVALID AIR MOT                                                  95  083ZONE ADMISSION DATA ACCEPTED                                             Y  8888XJ5NF00004"));
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			declaration.Reload();
			AssertEquals("Status", FTZMessageStatusList.Codes.ClearFTZAdmissionAddWithWarnings, declaration.AdmissionStatus);
			AssertEquals("No specific dispositions received", 0, declaration.FTZDispositionCodes.Count);
		}

		public void TestPaperless()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FTZAdmissionNumber = "1530001|12|00000004";
			declaration.Messages.Add(CreateRequestMessage());
			declaration.Messages.Add(CreateResponseMessage("B002501SV9NF                                               115039               90A153000111000000042501Y                                                       91B4 1  15300011100000004                  1112112117                           95     FTZ PAPERLESS ADMISSION                                                  Y  2501SV9NF00003"));
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			declaration.Reload();
			AssertEquals("Paperless", YesNoDefaultList.Codes.Yes, declaration.US_PaperlessEntry);
			AssertEquals("Dispositions", 1, declaration.FTZDispositionCodes.Count);
			AssertEquals("Dispositions", DispositionList.Codes.B4, declaration.FTZDispositionCodes[0].US_Code);
		}

		public void TestRejectedResponse()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Messages.Add(CreateRequestMessage());
			declaration.Messages.Add(CreateResponseMessage("B012501SV9NF                                               115039               90        00        2501N                                                       91B3 1         00                          1111072208                           10A       00        2501NSV9         13-147927000                               9501004 INVALID ZONE ID                                                         9501082 ZONE ADMISSION DATA REJECTED                                            Y  2501SV9NF00003"));
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("ZONE ADMISSION DATA REJECTED", sentMail.Body);
			AssertContains("FTZ Admission Response (Failure) for B00001000", sentMail.Subject);
			declaration.Reload();
			AssertEquals("Status", FTZMessageStatusList.Codes.ErrorFTZAdmissionAdd, declaration.AdmissionStatus);
			AssertEquals("Calendar Year is empty - message not accepted", "", declaration.FTZYear);
			AssertEquals("No specific dispositions received", 0, declaration.FTZDispositionCodes.Count);
		}

		public void TestRejectedResponse2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Messages.Add(CreateRequestMessage());

			//message 1
			declaration.Messages.Add(CreateResponseMessage("B012501SV9NF                                               115294               90A153000111D61E76662501N                                                       91B3 1  153000111D61E7666                  1111212207                           4077739100025                                            0000000046DE     W004  41123330443                                                                     9501017 BILL/INBOND NOT ON FILE                                                 4256-999999900                                                                  9501132 PTT REJECTED - OPERATOR NOT ACCEPTED                                    9501027 CARRIER NOT BONDED                                                      4212-1234568AB                                                                  9501088 REC42 DUPLICATE FOR PERMIT TO TRANSFER                                  9501082 ZONE ADMISSION DATA REJECTED                                            Y  2501SV9NF00009"));
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];

			var text = @"<td>77739100025</td><td>123330443</td><td>BILL/INBOND NOT ON FILE</td></tr></table><br><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings""><th>Bill Number</th><th>IRS Identifier</th><th>Notification</th></tr></thead><tr><td>77739100025</td><td>56-999999900</td><td>PTT REJECTED - OPERATOR NOT ACCEPTED</td></tr><tr><td>77739100025</td><td>56-999999900</td><td>CARRIER NOT BONDED</td></tr><tr><td>77739100025</td><td>12-1234568AB</td><td>REC42 DUPLICATE FOR PERMIT TO TRANSFER</td></tr><tr><td>77739100025</td><td>12-1234568AB</td><td>ZONE ADMISSION DATA REJECTED</td>";
			AssertContains(text, sentMail.Body);

			//message 2
			declaration.Messages.Add(CreateResponseMessage("B012501SV9NF                                               115079               90A153000111ADCGKL422501N                                                       91B3 1  153000111ADCGKL42                  1111071943                           20A  CR                                                            21110411     9501068 INVALID MODE OF TRANSPORTATION                                          9501018 IMPORT DATE MISSING/INVALID                                             9501019 EXPORT DATE MISSING/INVALID                                             9501016 PORT OF UNLADING MISSING/INVALID                                        4077739100014                                                             W004  9501042 INVALID MANIFEST QUANTITY                                               9501064 INVALID COUNTRY OF EXPORT                                               9501082 ZONE ADMISSION DATA REJECTED                                            Y  2501SV9NF00009"));
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];

			text = @"<td>INVALID MODE OF TRANSPORTATION</td></tr><tr><td>018</td><td>IMPORT DATE MISSING/INVALID</td></tr><tr><td>019</td><td>EXPORT DATE MISSING/INVALID</td></tr><tr><td>016</td><td>PORT OF UNLADING MISSING/INVALID</td></tr><tr><td>042</td><td>INVALID MANIFEST QUANTITY</td></tr><tr><td>064</td><td>INVALID COUNTRY OF EXPORT</td></tr><tr><td>082</td><td>ZONE ADMISSION DATA REJECTED</td>";
			AssertContains(text, sentMail.Body);

			//message 3
			declaration.Messages.Add(CreateResponseMessage("B012501SV9NF                                               115309               90A1530001118E027FAD2501N                                                       91B3 1  1530001118E027FAD                  1111220152                           4077739100036                        00000HAWB001        0000000003DE     W004  50000018456301020      000000000001NO 000000000000                              9501156 INVALID COUNTRY CODE                                                    50000028456101010      000000000002NO 000000000000                              9501156 INVALID COUNTRY CODE                                                    50000038456301020      000000000003NO 000000000000                              9501156 INVALID COUNTRY CODE                                                    5100000000000000000000000000000000 00001000                                     9501049 INVALID ZONE STATUS                                                     50000048456101010   FCH000000000004NO 000000000000   123                        5100000000000000000000000000000023 00001000                                     9501049 INVALID ZONE STATUS                                                     4077739100036                        00000HAWB002        0000000004DE     W004  50000018456301020      000000000001NO 000000000000                              9501156 INVALID COUNTRY CODE                                                    50000028456101010      000000000002NO 000000000000                              9501156 INVALID COUNTRY CODE                                                    50000038456301020      000000000003NO 000000000000                              9501156 INVALID COUNTRY CODE                                                    5100000000000000000000000000000000 00001000                                     9501049 INVALID ZONE STATUS                                                     50000048456101010   FCH000000000004NO 000000000000   123                        5100000000000000000000000000000023 00001000                                     9501049 INVALID ZONE STATUS                                                     9501082 ZONE ADMISSION DATA REJECTED                                            Y  2501SV9NF00025"));
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];

			text = @"<td>77739100036</td><td>8456301020</td><td>INVALID COUNTRY CODE</td></tr><tr><td>77739100036</td><td>8456101010</td><td>INVALID COUNTRY CODE</td></tr><tr><td>77739100036</td><td>8456301020</td><td>INVALID COUNTRY CODE</td></tr><tr><td>77739100036</td><td>8456301020</td><td>INVALID ZONE STATUS</td></tr><tr><td>77739100036</td><td>8456101010</td><td>INVALID ZONE STATUS</td></tr><tr><td>77739100036</td><td>8456301020</td><td>INVALID COUNTRY CODE</td></tr><tr><td>77739100036</td><td>8456101010</td><td>INVALID COUNTRY CODE</td></tr><tr><td>77739100036</td><td>8456301020</td><td>INVALID COUNTRY CODE</td></tr><tr><td>77739100036</td><td>8456301020</td><td>INVALID ZONE STATUS</td></tr><tr><td>77739100036</td><td>8456101010</td><td>INVALID ZONE STATUS</td></tr><tr><td>77739100036</td><td>8456101010</td><td>ZONE ADMISSION DATA REJECTED</td>";
			AssertContains(text, sentMail.Body);
		}

		public void TestUnauthorizedResponse()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Messages.Add(CreateRequestMessage());
			declaration.Messages.Add(CreateResponseMessage("B013901SV9NF                                               115026               9501073 UNAUTHORIZED TO TRANSMIT                                                9501082 ZONE ADMISSION DATA REJECTED                                            Y  3901SV9NF00002"));
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("ZONE ADMISSION DATA REJECTED", sentMail.Body);
			AssertContains("FTZ Admission Response (Failure) for B00001000", sentMail.Subject);
		}

		public void TestCanSendReplace()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.FTZZoneID = "0350C00";
			declaration.FTZYear = "15";
			declaration.FTZControlNumber = "0005863P";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2710119000";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			//FTZ admission added and accepted
			var request = mock.Object;
			request.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			request.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			request.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData;
			request.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZAdmissionAdd;
			request.EM_MessageNum = "CBRNYCNYC_80053";
			request.EM_MessageText = "B015301BERFT                                               CBRNYCNYC_80053      10A0350C00150005863P1101NBER         61-168957400                               20A10GMCPGENMAR GEORGE T        VOY# 1503      2015042420150428110320150428     40GMCP056042415                                          0000000001CA15200      50000012709002090 CA XA000073490400BBL000000000000                              5100990715580000450000000001849678P05625000                                     60HIBERNIA CRUDE**PRODUCT: 454400**API: 35.0**VIM 61-168957400                  60OY#: 1503**VESSEL: GENMAR GEORGE T**PRELIMINAMIDXACHECAN500CAL                61RY                                                                            Y  5301BERFT00008";
			declaration.Messages.Add(request);

			var response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			response.EM_Status = EDIMessage.Status.Queued;
			response.EM_MessageNum = "CBRNYCNYC_80053";
			response.EM_MessageText = "B015301BERNF                                               CBRNYCNYC_80053      90A0350C00150005863P1101N                                                       91B1 1  0350C00150005863P                  1504281527                           95  083 ZONE ADMISSION DATA ACCEPTED                                            Y  5301BERNF00001";
			declaration.Messages.Add(response);
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			var reloadedDeclaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals("Most recent Log", "CFA", reloadedDeclaration.Logs.MostRecentLog.ReferenceFreeText);

			response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			response.EM_Status = EDIMessage.Status.Queued;
			response.EM_MessageNum = "CBRNYCNYC_80053";
			response.EM_MessageText = "B005301BERNF                                               CBRNYCNYC_80053      90A0350C00150005863P1101N                                                       91B4 1  0350C00150005863P                  1504281527                           95     FTZ PAPERLESS ADMISSION                                                  Y  5301BERNF00003";
			declaration.Messages.Add(response);
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			reloadedDeclaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Enterprise.ZArchitecture.Business.AutoEvents.CustomsEntryStatusCode);
			query.AddToFilter(StmALogSchema.SL_Reference, "CFA");

			AssertNotNull("CFA (clear message) log is not cancelled", reloadedDeclaration.Logs.HasLogWith(query));

			response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			response.EM_Status = EDIMessage.Status.Queued;
			response.EM_MessageNum = "CBRNYCNYC_80053";
			response.EM_MessageText = "B005301BERNF                                               CBRNYCNYC_80053      90A0350C00150005863P1101N                                                       91BF 1  0350C00150005863P                  1504281527                           95     FTZ ADMISSION AUTHORIZED                                                 Y  5301BERNF00003";
			declaration.Messages.Add(response);
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			reloadedDeclaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertNotNull("CFA (clear message) log is not cancelled", reloadedDeclaration.Logs.HasLogWith(query));

			//concurrence sent and accepted

			var concurrenceSent = mock.Object;
			concurrenceSent.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			concurrenceSent.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			concurrenceSent.EM_MessageSubType = EM_MessageSubTypeList.Codes.FZConcurrence;
			concurrenceSent.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.FTZEventReporting;
			concurrenceSent.EM_MessageNum = "CBRNYCNYC_80058";
			concurrenceSent.EM_MessageText = "B015301BERFZ                                               CBRNYCNYC_80058      1010350C00150005863P                  A 0000000001FI                            Y  5301BERFZ00001";
			declaration.Messages.Add(concurrenceSent);

			var concurrenceReceived = Factory.New<MQEDIMessage>();
			concurrenceReceived.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			concurrenceReceived.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			concurrenceReceived.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			concurrenceReceived.EM_Status = EDIMessage.Status.Queued;
			concurrenceReceived.EM_MessageNum = "CBRNYCNYC_80058";
			concurrenceReceived.EM_MessageText = "B015301BERNF                                               CBRNYCNYC_80058      90A0350C00150005863P1101N                                                       91B6 1  0350C00150005863P                  1504281536                           9502115 ZONE CONCURRENCE DATA ACCEPTED                                          Y  5301BERNF00001";
			declaration.Messages.Add(concurrenceReceived);
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			reloadedDeclaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertNotNull("CFA (clear message) log is not cancelled", reloadedDeclaration.Logs.HasLogWith(query));
			AssertEquals(FTZMessageStatusList.Codes.ClearConcurrence, reloadedDeclaration.FTZConcurrenceStatus);
			AssertEquals("FTZ Concurrence Cleared", "Y", reloadedDeclaration.US_F_BOCStatInfoFurnished);

			var concurrenceLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Enterprise.ZArchitecture.Business.AutoEvents.CustomsEntryStatusCode);
			concurrenceLogQuery.AddToFilter(StmALogSchema.SL_Reference, "CCC");
			AssertNotNull("CCC (concurrence clear message) log is not cancelled", reloadedDeclaration.Logs.HasLogWith(concurrenceLogQuery));

			response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			response.EM_Status = EDIMessage.Status.Queued;
			response.EM_MessageNum = "CBRNYCNYC_80053";
			response.EM_MessageText = "B005301BERNF                                               CBRNYCNYC_80053      90A0350C00150005863P1101N                                                       91B8 1  0350C00150005863P                  1504281536GMCP                       Y  5301BERNF00002";
			declaration.Messages.Add(response);
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			reloadedDeclaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertNotNull("CFA (clear message) log is not cancelled", reloadedDeclaration.Logs.HasLogWith(query));
			AssertNotNull("CCC (concurrence clear message) log is not cancelled", reloadedDeclaration.Logs.HasLogWith(concurrenceLogQuery));

			// delete sent and accepted
			var deleteSent = mock.Object;
			deleteSent.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			deleteSent.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			deleteSent.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData;
			deleteSent.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZAdmissionDelete;
			deleteSent.EM_MessageNum = "CBRNYCNYC_89031";
			deleteSent.EM_MessageText = "B015301BERFT                                               CBRNYCNYC_89031      10D0350C00150005863P1101NBER         61-168957400                               Y  5301BERFT00001";
			declaration.Messages.Add(deleteSent);

			var deleteReceived = Factory.New<MQEDIMessage>();
			deleteReceived.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			deleteReceived.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			deleteReceived.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			deleteReceived.EM_Status = EDIMessage.Status.Queued;
			deleteReceived.EM_MessageNum = "CBRNYCNYC_89031";
			deleteReceived.EM_MessageText = "B015301BERNF                                               CBRNYCNYC_89031      90 0350C00150005863P1101N                                                       91B1 1  0350C00150005863P                  1505111126                           95  084 ZONE ADMISSION DELETED                                                  Y  5301BERNF00001";
			declaration.Messages.Add(deleteReceived);
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			reloadedDeclaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);

			var cfaLog = reloadedDeclaration.Logs.Find(query);
			AssertEquals("Only one CFA (clear message) log exists", 1, cfaLog.Length);
			Assert("CFA (clear message) log is cancelled", cfaLog[0].SL_IsCancelled);

			var cccLog = reloadedDeclaration.Logs.Find(concurrenceLogQuery);
			AssertEquals("Only one CCC (concurrence clear message) log exists", 1, cccLog.Length);
			Assert("CCC (concurrence clear message) log is cancelled", cccLog[0].SL_IsCancelled);

			deleteReceived = Factory.New<MQEDIMessage>();
			deleteReceived.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			deleteReceived.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			deleteReceived.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			deleteReceived.EM_Status = EDIMessage.Status.Queued;
			deleteReceived.EM_MessageNum = "CBRNYCNYC_89031";
			deleteReceived.EM_MessageText = "B005301BERNF                                               CBRNYCNYC_89031      90A0350C00150005863P1101N                                                       91BH 1  0350C00150005863P                  1505111126                           95     FTZ ADMISSION DELETE                                                     Y  5301BERNF00003";
			declaration.Messages.Add(deleteReceived);
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			reloadedDeclaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			cfaLog = reloadedDeclaration.Logs.Find(query);
			AssertEquals("Only one CFA (clear message) log exists", 1, cfaLog.Length);
			Assert("CFA (clear message) log is cancelled", cfaLog[0].SL_IsCancelled);

			cccLog = reloadedDeclaration.Logs.Find(concurrenceLogQuery);
			AssertEquals("Only one CCC (concurrence clear message) log exists", 1, cccLog.Length);
			Assert("CCC (concurrence clear message) log is cancelled", cccLog[0].SL_IsCancelled);

			var deleteLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Enterprise.ZArchitecture.Business.AutoEvents.DeclarationCancellationApprovedCode);
			deleteLogQuery.AddToFilter(StmALogSchema.SL_Reference, "CFD");

			var deleteLog = reloadedDeclaration.Logs.Find(deleteLogQuery);
			AssertEquals("Only one CFD (delete message) log exists", 1, deleteLog.Length);
			Assert("CFD (delete message) log is not cancelled", !deleteLog[0].SL_IsCancelled);

			//ftz added with new admission number
			declaration.FTZControlNumber = "0005864P";
			request = mock.Object;
			request.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			request.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			request.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData;
			request.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZAdmissionAdd;
			request.EM_MessageNum = "CBRNYCNYC_89063";
			request.EM_MessageText = "B015301BERFT                                               CBRNYCNYC_89063      10A0350C00150005864P1101NBER         61-168957400                               20A10GMCPGENMAR GEORGE T        VOY# 1503      2015042420150428110320150428     40GMCP056042415                                          0000000001CA15200      50000012709002090 CA XA000073490400BBL000000000000                              5100990715580000450000000001849678P05625000                                     60HIBERNIA CRUDE**PRODUCT: 454400**API: 35.0**VIM 61-168957400                  60OY#: 1503**VESSEL: GENMAR GEORGE T**PRELIMINAMIDXACHECAN500CAL                61RY                                                                            Y  5301BERFT00008";
			declaration.Messages.Add(request);
			declaration.AdmissionStatus = FTZMessageStatusList.Codes.AwaitingFTZAdmissionAdd;

			response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			response.EM_Status = EDIMessage.Status.Queued;
			response.EM_MessageNum = "CBRNYCNYC_89063";
			response.EM_MessageText = "B015301BERNF                                               CBRNYCNYC_89063      90A0350C00150005864P1101N                                                       91B1 1  0350C00150005864P                  1505111134                           95  083 ZONE ADMISSION DATA ACCEPTED                                            Y  5301BERNF00001";
			declaration.Messages.Add(response);
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			reloadedDeclaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);

			cfaLog = reloadedDeclaration.Logs.Find(query);
			AssertEquals("Two CFA logs (clear message) should exist", 2, cfaLog.Length);
			Assert("CFA (clear message) log is cancelled", cfaLog[0].SL_IsCancelled);
			Assert("CFA (clear message) log is cancelled", !cfaLog[1].SL_IsCancelled);

			cccLog = reloadedDeclaration.Logs.Find(concurrenceLogQuery);
			AssertEquals("Only one CCC (concurrence clear message) log exists", 1, cccLog.Length);
			Assert("CCC (concurrence clear message) log is cancelled", cccLog[0].SL_IsCancelled);

			deleteLog = reloadedDeclaration.Logs.Find(deleteLogQuery);
			AssertEquals("Only one CFD (delete message) log exists", 1, deleteLog.Length);
			Assert("CFD (delete message) log is cancelled", deleteLog[0].SL_IsCancelled);

			response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			response.EM_Status = EDIMessage.Status.Queued;
			response.EM_MessageNum = "CBRNYCNYC_89063";
			response.EM_MessageText = "B005301BERNF                                               CBRNYCNYC_89063      90A0350C00150005864P1101N                                                       91B4 1  0350C00150005864P                  1505111139                           95     FTZ PAPERLESS ADMISSION                                                  Y  5301BERNF00003";
			declaration.Messages.Add(response);
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			reloadedDeclaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);

			cfaLog = reloadedDeclaration.Logs.Find(query);
			AssertEquals("Two CFA logs (clear message) should exist", 2, cfaLog.Length);
			Assert("CFA (clear message) log is cancelled", cfaLog[0].SL_IsCancelled);
			Assert("CFA (clear message) log is cancelled", !cfaLog[1].SL_IsCancelled);

			cccLog = reloadedDeclaration.Logs.Find(concurrenceLogQuery);
			AssertEquals("Only one CCC (concurrence clear message) log exists", 1, cccLog.Length);
			Assert("CCC (concurrence clear message) log is cancelled", cccLog[0].SL_IsCancelled);

			deleteLog = reloadedDeclaration.Logs.Find(deleteLogQuery);
			AssertEquals("Only one CFD (delete message) log exists", 1, deleteLog.Length);
			Assert("CFD (delete message) log is cancelled", deleteLog[0].SL_IsCancelled);

			response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			response.EM_Status = EDIMessage.Status.Queued;
			response.EM_MessageNum = "CBRNYCNYC_89063";
			response.EM_MessageText = "B005301BERNF                                               CBRNYCNYC_89063      90A0350C00150005864P1101N                                                       91BF 1  0350C00150005864P                  1505111139                           95     FTZ ADMISSION AUTHORIZED                                                 Y  5301BERNF00003";
			declaration.Messages.Add(response);
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			reloadedDeclaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			cfaLog = reloadedDeclaration.Logs.Find(query);
			AssertEquals("Two CFA logs (clear message) should exist", 2, cfaLog.Length);
			Assert("CFA (clear message) log is cancelled", cfaLog[0].SL_IsCancelled);
			Assert("CFA (clear message) log is cancelled", !cfaLog[1].SL_IsCancelled);

			cccLog = reloadedDeclaration.Logs.Find(concurrenceLogQuery);
			AssertEquals("Only one CCC (concurrence clear message) log exists", 1, cccLog.Length);
			Assert("CCC (concurrence clear message) log is cancelled", cccLog[0].SL_IsCancelled);

			deleteLog = reloadedDeclaration.Logs.Find(deleteLogQuery);
			AssertEquals("Only one CFD (delete message) log exists", 1, deleteLog.Length);
			Assert("CFD (delete message) log is cancelled", deleteLog[0].SL_IsCancelled);

			// concurrence sent and accepted
			concurrenceSent = mock.Object;
			concurrenceSent.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			concurrenceSent.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			concurrenceSent.EM_MessageSubType = EM_MessageSubTypeList.Codes.FZConcurrence;
			concurrenceSent.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.FTZEventReporting;
			concurrenceSent.EM_MessageNum = "CBRNYCNYC_89086";
			concurrenceSent.EM_MessageText = "B015301BERFZ                                               CBRNYCNYC_89086      1010350C00150005864P                  A 0000000001FI                            Y  5301BERFZ00001";
			declaration.Messages.Add(concurrenceSent);

			concurrenceReceived = Factory.New<MQEDIMessage>();
			concurrenceReceived.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			concurrenceReceived.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			concurrenceReceived.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			concurrenceReceived.EM_Status = EDIMessage.Status.Queued;
			concurrenceReceived.EM_MessageNum = "CBRNYCNYC_89086";
			concurrenceReceived.EM_MessageText = "B015301BERNF                                               CBRNYCNYC_89086      90A0350C00150005864P1101N                                                       91B6 1  0350C00150005864P                  1505111150                           9502115 ZONE CONCURRENCE DATA ACCEPTED                                          Y  5301BERNF00001";
			declaration.Messages.Add(concurrenceReceived);
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			reloadedDeclaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			cfaLog = reloadedDeclaration.Logs.Find(query);
			AssertEquals("Two CFA logs (clear message) should exist", 2, cfaLog.Length);
			Assert("CFA (clear message) log is cancelled", cfaLog[0].SL_IsCancelled);
			Assert("CFA (clear message) log is not cancelled", !cfaLog[1].SL_IsCancelled);

			concurrenceReceived = Factory.New<MQEDIMessage>();
			concurrenceReceived.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			concurrenceReceived.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			concurrenceReceived.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			concurrenceReceived.EM_Status = EDIMessage.Status.Queued;
			concurrenceReceived.EM_MessageNum = "CBRNYCNYC_89063";
			concurrenceReceived.EM_MessageText = "B005301BERNF                                               CBRNYCNYC_89063      90A0350C00150005864P1101N                                                       91B8 1  0350C00150005864P                  1505111150GMCP                       Y  5301BERNF00002";
			declaration.Messages.Add(concurrenceReceived);
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			reloadedDeclaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			cfaLog = reloadedDeclaration.Logs.Find(query);
			AssertEquals("Two CFA logs (clear message) should exist", 2, cfaLog.Length);
			Assert("CFA (clear message) log is cancelled", cfaLog[0].SL_IsCancelled);
			Assert("CFA (clear message) log is not cancelled", !cfaLog[1].SL_IsCancelled);

			//send add message and rejected
			request = mock.Object;
			request.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			request.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			request.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData;
			request.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZAdmissionAdd;
			request.EM_MessageNum = "CBRNYCNYC_117444";
			request.EM_MessageText = "B011001BERFT                                               CBRNYCNYC_117444     10A0350C00150005864P1101NBER         61-168957400                               20A10GMCPGENMAR GEORGE T        VOY# 1503      2015042420150602110320150602     40GMCP056042415                                          0000000001CA15200      50000012709002090 CA XA000023109800BBL000000000000                              5100313046820000148590120000582138P01857377                                     60HIBERNIA CRUDE**PRODUCT: 454400**API: 34.2**VIM 61-168957400                  60OY#: 1503**VESSEL: GENMAR GEORGE T**FINAL    MIDXACHECAN500CAL                40GMCP056042415A                                         0000000001CA15200      50000012709002090 CA XA000050319000BBL000000000000                              5100681624370000323538340001267540P04044229                                     60HIBERNIA CRUDE**PRODUCT: 454400**API: 34.2**VIM 61-168957400                  60OY#: 1503**VESSEL: GENMAR GEORGE T**FINAL    MIDXACHECAN500CAL                Y  1001BERFT00012";
			declaration.Messages.Add(request);

			response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			response.EM_Status = EDIMessage.Status.Queued;
			response.EM_MessageNum = "CBRNYCNYC_117444";
			response.EM_MessageText = "B011001BERNF                                               CBRNYCNYC_117444     90A0350C00150005864P1101N                                                       91B3 1  0350C00150005864P                  1506151124                           10A0350C00150005864P1101NBER         61-168957400                               9501002 DUPLICATE ADMISSION ON FILE                                             9501082 ZONE ADMISSION DATA REJECTED                                            Y  1001BERNF00003";
			declaration.Messages.Add(response);
			Factory.Save();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			reloadedDeclaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			cfaLog = reloadedDeclaration.Logs.Find(query);
			AssertEquals("Two CFA logs (clear message) should exist", 2, cfaLog.Length);
			Assert("CFA (clear message) log is cancelled", cfaLog[0].SL_IsCancelled);
			Assert("CFA (clear message) log is not cancelled", !cfaLog[1].SL_IsCancelled);
		}

		public void TestProcessNF92Block()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.JE_MasterBillIssuerSCAC = "QF";
			declaration.JE_MasterBill = "08157824211";
			declaration.JE_HouseBill = "SHAE21100063";
			declaration.FTZZoneID = "2050006";
			declaration.FTZYear = "21";
			declaration.FTZControlNumber = "FF000175";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8541100080";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var addMock = Factory.NewMoq<MQEDIMessage>();
			addMock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var admissionAddTransmit = addMock.Object;
			admissionAddTransmit.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			admissionAddTransmit.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			admissionAddTransmit.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData;
			admissionAddTransmit.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZAdmissionAdd;
			admissionAddTransmit.EM_MessageNum = "WO3LAXPRD_318348";
			admissionAddTransmit.EM_MessageText = "B012720WDNFT                                               WO3LAXPRD_318348     10A2050006  21FF000175N2704NWDN         95-480332700WA8495-480332700            20A40QF  QANTAS AIRWAYS LIMITED QF7521         2021101420211014272020211014     4008157824211                        SHAE21100063        0000000258CN57035      50000018541100080    CN001050050000NO                                           5100000017470000001110820000005242P00000000                                     60DIODES, OTHER, OTHER THAN PHOTOSENSITIVE OR LMIDHKMCCSEM1401HON               61IGHT-EMITING DIODES (LED)                                                     Y  2720WDNFT00007";
			declaration.Messages.Add(admissionAddTransmit);

			var statusUpdateResponse = Factory.New<MQEDIMessage>();
			statusUpdateResponse.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			statusUpdateResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			statusUpdateResponse.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			statusUpdateResponse.EM_Status = EDIMessage.Status.Queued;
			statusUpdateResponse.EM_MessageNum = "WO3LAXPRD_318348";
			statusUpdateResponse.EM_MessageText = "B002720WDNNF                                               WO3LAXPRD_318348     90A2050006  21FF0001752704N                                                     911F 2  08157824211SHAE21100063            2110141922    WA84                   92PID0000000109283366706                          QFA 7521           20211013   95     CBP LOCAL TRANSFER AUTHORIZED                                            Y  2720WDNNF00000";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			var newFactory = new BusinessObjectFactory();
			var loadedDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			var loadedMasterBill = loadedDeclaration.PrimaryMasterBill;
			AssertEquals("0000000109283366706", loadedMasterBill.USB_PermitToTransferID);
			var loadedHouseBill = loadedDeclaration.PrimaryHouseBill;
			AssertEquals("0000000109283366706", loadedHouseBill.USB_PermitToTransferID);
		}

		public void TestCancelPermitToTransferStatus()
		{
			DeclarationTestHelper.SetEntryFilerCode("WDN");
			DeclarationTestHelper.SetProcessingDistrictPortCode("2720");

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.JE_MasterBillIssuerSCAC = "QF";
			declaration.JE_MasterBill = "08157824211";
			declaration.JE_HouseBill = "SHAE21100063";
			declaration.FTZZoneID = "2050006";
			declaration.FTZYear = "21";
			declaration.FTZControlNumber = "FF000175";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "8541100080";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var addMock = Factory.NewMoq<MQEDIMessage>();
			addMock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var cancelPTTTransmit = addMock.Object;
			cancelPTTTransmit.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			cancelPTTTransmit.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			cancelPTTTransmit.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.FTZEventReporting;
			cancelPTTTransmit.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZCancelPermitToTransfer;
			cancelPTTTransmit.EM_MessageNum = "WO3LAXPRD_318348";
			cancelPTTTransmit.EM_MessageText = "B012720WDNFZ                                               WO3LAXPRD_318348     10208157824211SHAE21100063            K           FI                            11               205000621FF000175   QF                                         Y  2720WDNFZ00002";
			declaration.Messages.Add(cancelPTTTransmit);

			var cancelPTTAcceptedResponse = Factory.New<MQEDIMessage>();
			cancelPTTAcceptedResponse.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			cancelPTTAcceptedResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			cancelPTTAcceptedResponse.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			cancelPTTAcceptedResponse.EM_Status = EDIMessage.Status.Queued;
			cancelPTTAcceptedResponse.EM_MessageNum = "WO3LAXPRD_318348";
			cancelPTTAcceptedResponse.EM_MessageText = "B002720WDNNF                                               WO3LAXPRD_318348     90A2050006  21FF0001752704N                                                     91B6 2  08157824211SHAE21100063            2110141922    WA84                   9502224CANCEL PTT DATA ACCEPTED                                                 Y  2720WDNNF00000";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			var newFactory = new BusinessObjectFactory();
			var loadedDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals(FTZMessageStatusList.Codes.PermitToTransferCancelAccepted, loadedDeclaration.FTZPTTStatus);

			var cancelPTTNotAuthorizedResponse = Factory.New<MQEDIMessage>();
			cancelPTTNotAuthorizedResponse.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			cancelPTTNotAuthorizedResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			cancelPTTNotAuthorizedResponse.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			cancelPTTNotAuthorizedResponse.EM_Status = EDIMessage.Status.Queued;
			cancelPTTNotAuthorizedResponse.EM_MessageNum = "WO3LAXPRD_318348";
			cancelPTTNotAuthorizedResponse.EM_MessageText = "B002720WDNNF                                               WO3LAXPRD_318348     90A2050006  21FF0001752704N                                                     91B7 2  08157824211SHAE21100063            2110141922    WA84                   9501215CANCEL PTT NOT AUTHORIZED                                                Y  2720WDNNF00000";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			newFactory = new BusinessObjectFactory();
			loadedDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			AssertEquals(FTZMessageStatusList.Codes.PermitToTransferCancelUnauthorized, loadedDeclaration.FTZPTTStatus);
		}

		MQEDIMessage CreateRequestMessage()
		{
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var request = mock.Object;
			request.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			request.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			request.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData;
			request.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZAdmissionAdd;
			request.EM_MessageNum = "115039";
			request.EM_MessageText = "B013901SV9FT                                               115039               10A       11        3901YSV9                                                    20A                                                    21111101390121111105     40123456789                                                                     40123456789                                                                     5000001                000000000000   000000000000                              5100000000000000000000000000000000                                              613232                                                                          Y  3901SV9FT00007";
			return request;
		}

		MQEDIMessage CreateResponseMessage(ZString messageText)
		{
			var response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			response.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZAdmissionAdd;
			response.EM_Status = EDIMessage.Status.Queued;
			response.EM_MessageNum = "115039";
			response.EM_MessageText = messageText;
			return response;
		}
	}
}
