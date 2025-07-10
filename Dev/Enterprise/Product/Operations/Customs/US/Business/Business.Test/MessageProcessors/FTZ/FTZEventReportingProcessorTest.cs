using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class FTZEventReportingProcessorTest : ABIProcessorTest<FTZAdmissionProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		protected override void EndToEndCore()
		{
			var declaration = Factory.New<JobDeclaration>();
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var request = mock.Object;
			request.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			request.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			request.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZPermitToTransfer;
			request.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.FTZEventReporting;
			request.EM_MessageNum = "115039";
			request.EM_MessageText = "B012501SV9FZ                                               115563               102XXXAJSOCT0111                      F 0000000001PA            W004PHLY        11               15300011100000009                                              20COMMENTS                                                                      Y  2501SV9FZ00003";
			declaration.Messages.Add(request);

			var response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			response.EM_MessageNum = "115039";
			response.EM_MessageText = "B012501SV9NF                                               115951               90A153000112000000132501Y                                                       91B6 2  XXXAJSOCT0111                      1201052123                           9502116 PTT MESSAGE DATA ACCEPTED                                               Y  2501SV9NF00001";
			declaration.Messages.Add(response);
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("PTT MESSAGE DATA ACCEPTED", sentMail.Body);
			AssertContains("FZ Event Reporting Response for B00001000", sentMail.Subject);
			declaration.Reload();
			AssertEquals("Dispositions", 1, declaration.FTZDispositionCodes.Count);
			AssertEquals("Dispositions", DispositionList.Codes.B6, declaration.FTZDispositionCodes[0].US_Code);
		}

		public void TestNF90BlockHas3AlpahnumeticSubZoneAnd3AlpahnumeticSiteID()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSOF");
			var port = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "1101", "PHILADELPHIA, PA", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var request = mock.Object;
			request.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			request.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			request.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZPermitToTransfer;
			request.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.FTZEventReporting;
			request.EM_MessageNum = "115039";
			request.EM_MessageText = "B012501SV9FZ                                               115563               102XXXAJSOCT0111                      F 0000000001PA            W004PHLY        11               15300011100000009                                              20COMMENTS                                                                      Y  2501SV9FZ00003";
			declaration.Messages.Add(request);

			Factory.Save();

			var response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			response.EM_MessageNum = "115039";
			response.EM_MessageText = "B012501SV9NF                                               115653               90ANEWNNNNEW12000000091101N                                                     91B7 2  XXXAJSOCT0111                      1201051742                           102XXXAJSOCT0111                      F 0000000001FI            W004PHL         9501028 INVALID FIRMS CODE                                                      11               15300011200000009                                              9501103 ADMISSION NOT ON FILE                                                   9502120 TRANSACTION DATA REJECTED                                               Y  2501SV9NF00005";

			declaration.Messages.Add(response);
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];

			AssertContains("<tr><td>Admission Type</td><td>Regular Admission</td></tr><tr><td>Zone ID</td><td>NEWNNNNEW</td></tr><tr><td>Control Number</td><td>00000009</td></tr><tr><td>Year</td><td>12</td></tr><tr><td>Port</td><td>1101 PHILADELPHIA, PA</td></tr><tr><td>Direct Delivery</td><td>N</td></tr>", sentMail.Body);
		}

		public void TestPTTResponse()
		{
			var declaration = Factory.New<JobDeclaration>();
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var request = mock.Object;
			request.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			request.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			request.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZPermitToTransfer;
			request.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.FTZEventReporting;
			request.EM_MessageNum = "115039";
			request.EM_MessageText = "B012501SV9FZ                                               115563               102XXXAJSOCT0111                      F 0000000001PA            W004PHLY        11               15300011100000009                                              20COMMENTS                                                                      Y  2501SV9FZ00003";
			declaration.Messages.Add(request);

			var response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			response.EM_MessageNum = "115039";
			response.EM_MessageText = "B012501SV9NF                                               115563               90A153000111000000092501N                                                       91B7 2  XXXAJSOCT0111                      1112201737                           102XXXAJSOCT0111                      F 0000000001PA            W004PHLY        9501132 PTT REJECTED - OPERATOR NOT ACCEPTED                                    9502120 TRANSACTION DATA REJECTED                                               Y  2501SV9NF00003                                                               ";
			declaration.Messages.Add(response);
			Factory.Save();

			#region Message Interpretation

			AssertEquals(
@"--------------AABIOutputB---------------
 Processing District Port Code (4-7)    :2501
 Filer Code (8-10)                      :SV9
 Application Identifier Code (11-12)    :NF
 Filer Preparers User Data Text (60-80) :115563

----------------FTZNF90-----------------
 Admission Type (3-3)              :A
 Zone I D (4-10)                   :1530001
 Calendar Year (11-12)             :11
 Control Number (13-20)            :00000009
 Port Code (21-24)                 :2501
 Direct Delivery Indicator (25-25) :N

----------------FTZNF91-----------------
 Disposition Code (3-5)    :B7
 Reference Qualifier (6-8) :2
 Reference I D (9-43)      :XXXAJSOCT0111
 Action Date (44-49)       :20-Dec-11
 Action Time (50-53)       :1737

----------------FTZFZ10-----------------
 Action Qualifier (3-3)       :2
 Identification Number (4-38) :XXXAJSOCT0111
 Action Code (39-40)          :F
 Received Quantity (41-50)    :1
 Delivery Code (51-52)        :PA
 F I R M S (65-68)            :W004
 Airport Code (69-71)         :PHL

----------------FTZNF95-----------------
 Narrative Message Type Code (3-4) :01
 Error Code (5-7)                  :132
 Remarks (8-67)                    :PTT REJECTED - OPERATOR NOT ACCEPTED

----------------FTZNF95-----------------
 Narrative Message Type Code (3-4) :02
 Error Code (5-7)                  :120
 Remarks (8-67)                    :TRANSACTION DATA REJECTED

--------------AABIOutputY---------------
 Processing District Port Code (4-7)    :2501
 Filer Code (8-10)                      :SV9
 Application Identifier Code (11-12)    :NF
 Output Transaction Image Count (13-17) :5

", response.EM_MessageInterpretation);

			#endregion

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("PTT REJECTED - OPERATOR NOT ACCEPTED", sentMail.Body);

			response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			response.EM_MessageNum = "115039";
			response.EM_MessageText = "B012501SV9NF                                               115653               90 15300011200000009   0                                                        91B7 2  XXXAJSOCT0111                      1201051742                           102XXXAJSOCT0111                      F 0000000001FI            W004PHL         9501028 INVALID FIRMS CODE                                                      11               15300011200000009                                              9501103 ADMISSION NOT ON FILE                                                   9502120 TRANSACTION DATA REJECTED                                               Y  2501SV9NF00005";
			declaration.Messages.Add(response);
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("ADMISSION NOT ON FILE", sentMail.Body);
			declaration.Reload();
			AssertEquals("Calendar Year should be empty - PTT is not accepted", "", declaration.FTZYear);
			AssertEquals("Dispositions", 2, declaration.FTZDispositionCodes.Count);
			AssertEquals("Dispositions", DispositionList.Codes.B7, declaration.FTZDispositionCodes[0].US_Code);
		}

		public void TestPTTResponseWithAdditionalDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var request = mock.Object;
			request.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			request.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			request.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZPermitToTransfer;
			request.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.FTZEventReporting;
			request.EM_MessageNum = "115039";
			request.EM_MessageText = "B013901SV9FZ                                               HYEDUSCMT_162056     10277739991291                        F 0000000003FI11-987654300W235JFKN        11               15300011500000754                                              Y  3901SV9FZ00002";
			declaration.Messages.Add(request);

			var response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			response.EM_MessageNum = "115039";
			response.EM_MessageText = "B013901SV9NF                                               HYEDUSCMT_162056     " +
"90A153000115000007542501N                                                       " +
"91B7 2  77739991291                        1504162024                           " +
"11               15300011500000754                                              " +
"9501172 INVALID BILL KEY FOR SPLIT BILL- VERIFY                                 " +
"9501173 MASTER, HOUSE, CARR, FLIT, AND ARVL-DT                                  " +
"9502120 TRANSACTION DATA REJECTED                                               " +
"Y  3901SV9NF00004";

			declaration.Messages.Add(response);
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("INVALID BILL KEY FOR SPLIT BILL- VERIFY", sentMail.Body);
		}

		public void TestGoodsArrivalResponse1()
		{
			var declaration = Factory.New<JobDeclaration>();
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var request = mock.Object;
			request.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			request.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			request.EM_MessageSubType = EM_MessageSubTypeList.Codes.FZArrival;
			request.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.FTZEventReporting;
			request.EM_MessageNum = "115039";
			request.EM_MessageText = "B012501SV9FZ                                               115979               103XXXAJSOCT0117                      I                         W004            Y  2501SV9FZ00001";
			declaration.Messages.Add(request);

			var response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			response.EM_MessageNum = "115039";
			response.EM_MessageText = "B012501SV9NF                                               115964               90        48           0                                                        91B7 3  156001005                          1201052210                           103156001005                          I                         W004            9501142 INBOND ASSOC WITH MULTIPLE ADMISSIONS                                   9502120 TRANSACTION DATA REJECTED                                               Y  2501SV9NF00003";
			declaration.Messages.Add(response);
			Factory.Save();

			#region Message Interpretation

			AssertEquals(
@"--------------AABIOutputB---------------
 Processing District Port Code (4-7)    :2501
 Filer Code (8-10)                      :SV9
 Application Identifier Code (11-12)    :NF
 Filer Preparers User Data Text (60-80) :115964

----------------FTZNF90-----------------
 Calendar Year (11-12) :48
 Port Code (21-24)     :0

----------------FTZNF91-----------------
 Disposition Code (3-5)    :B7
 Reference Qualifier (6-8) :3
 Reference I D (9-43)      :156001005
 Action Date (44-49)       :05-Jan-12
 Action Time (50-53)       :2210

----------------FTZFZ10-----------------
 Action Qualifier (3-3)       :3
 Identification Number (4-38) :156001005
 Action Code (39-40)          :I
 F I R M S (65-68)            :W004

----------------FTZNF95-----------------
 Narrative Message Type Code (3-4) :01
 Error Code (5-7)                  :142
 Remarks (8-67)                    :INBOND ASSOC WITH MULTIPLE ADMISSIONS

----------------FTZNF95-----------------
 Narrative Message Type Code (3-4) :02
 Error Code (5-7)                  :120
 Remarks (8-67)                    :TRANSACTION DATA REJECTED

--------------AABIOutputY---------------
 Processing District Port Code (4-7)    :2501
 Filer Code (8-10)                      :SV9
 Application Identifier Code (11-12)    :NF
 Output Transaction Image Count (13-17) :5

", response.EM_MessageInterpretation);

			#endregion

			//message 1, rejected
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("FZ Event Reporting Response (Failure) for B00001000", sentMail.Subject);
			AssertContains("INBOND ASSOC WITH MULTIPLE ADMISSIONS", sentMail.Body);

			//message 2, rejected
			response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			response.EM_MessageNum = "115039";
			response.EM_MessageText = "B012501SV9NF                                               116011               90        48           0                                                        91B7 3  77739100025                        1201081552                           10377739100025                        I                         W004            9501026 INBOND NUMBER MISSING/INVALID                                           9502120 TRANSACTION DATA REJECTED                                               90A1530001114D6CAA8C2501N                                                       91B6 3  156001005                          1201081552                           9502119 PHYSICAL ARRIVAL DATA ACCEPTED                                          Y  2501SV9NF00004";
			declaration.Messages.Add(response);
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("FZ Event Reporting Response (Failure) for B00001000", sentMail.Subject);
			AssertContains("INBOND NUMBER MISSING/INVALID", sentMail.Body);
			AssertContains("TRANSACTION DATA REJECTED", sentMail.Body);
			AssertContains("PHYSICAL ARRIVAL DATA ACCEPTED", sentMail.Body);

			//message 3, accepted
			response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			response.EM_MessageNum = "115039";
			response.EM_MessageText = "B012501SV9NF                                               115978               90A1530001114D6CAA8C2501N                                                       91B6 3  156001005                          1201052307                           9502119 PHYSICAL ARRIVAL DATA ACCEPTED                                          Y  2501SV9NF00001";
			declaration.Messages.Add(response);
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("FZ Event Reporting Response for B00001000", sentMail.Subject);
			AssertContains("PHYSICAL ARRIVAL DATA ACCEPTED", sentMail.Body);
			response.Reload();
			AssertEquals("Should be attached to Declaration", declaration.PK, response.EM_LinkUniqueID);
			AssertEquals("Dispositions", 4, declaration.FTZDispositionCodes.Count);
			AssertEquals("Dispositions", DispositionList.Codes.B7, declaration.FTZDispositionCodes[1].US_Code);
			AssertEquals("Dispositions", DispositionList.Codes.B6, declaration.FTZDispositionCodes[2].US_Code);
		}

		public void TestConcurrenceResponses()
		{
			var declaration = Factory.New<JobDeclaration>();
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var request = mock.Object;
			request.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			request.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			request.EM_MessageSubType = EM_MessageSubTypeList.Codes.FZConcurrence;
			request.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.FTZEventReporting;
			request.EM_MessageNum = "116903";
			request.EM_MessageText = "B013910SV9FZ                                               116903               103888800032                          C           FI                            Y  3910SV9FZ00001";
			declaration.Messages.Add(request);

			var response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			response.EM_MessageNum = "116903";
			response.EM_MessageText = "B013910SV9NF                                               116903               90        48           0                                                        91B7 3  888800032                          1202141849                           103888800032                          C           FI                            9501143 BILL ON INBOND ON MULTIPLE ADMISSIONS                                   9502120 TRANSACTION DATA REJECTED                                               Y  3910SV9NF00003";
			declaration.Messages.Add(response);
			Factory.Save();

			//message 1, rejected
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("FZ Event Reporting Response (Failure) for B00001000", sentMail.Subject);
			AssertContains("BILL ON INBOND ON MULTIPLE ADMISSIONS", sentMail.Body);

			//message 2, accepted
			response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			response.EM_MessageNum = "116903";
			response.EM_MessageText = "B013910SV9NF                                               116908               90O153000112000000392501N                                                       91B6 1  15300011200000039                  1202142147                           9502115 ZONE CONCURRENCE DATA ACCEPTED                                          Y  3910SV9NF00001";
			declaration.Messages.Add(response);
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("FZ Event Reporting Response for B00001000", sentMail.Subject);
			AssertContains("ZONE CONCURRENCE DATA ACCEPTED", sentMail.Body);
		}

		public void TestUnconcurrenceResponses()
		{
			var declaration = Factory.New<JobDeclaration>();
			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var request = mock.Object;
			request.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			request.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			request.EM_MessageSubType = EM_MessageSubTypeList.Codes.FZUnconcurrence;
			request.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.FTZEventReporting;
			request.EM_MessageNum = "110703";
			request.EM_MessageText = "B013910SV9FZ                                               110703               101888800032                          J                                         13CARGOWISE SUPPORT                       893357966      02                     14IB 123456782                                                                  Y  1101SV9FZ00003";
			declaration.Messages.Add(request);

			var response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			response.EM_MessageNum = "110703";
			response.EM_MessageText = "B013910SV9NF                                               110703               90          20           0                                                      91B7 3  888800032                          1202141849                           103888800032                          J           FI                            13CARGOWISE SUPPORT                       893357966      02                     14IB 123456782                                                                  9501120 TRANSACTION DATA REJECTED                                               Y  3910SV9NF00003";
			declaration.Messages.Add(response);
			Factory.Save();

			//message 1, rejected
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			var sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("FZ Event Reporting Response (Failure) for B00001000", sentMail.Subject);
			AssertContains("TRANSACTION DATA REJECTED", sentMail.Body);

			//message 2, accepted
			response = Factory.New<MQEDIMessage>();
			response.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			response.EM_MessageNum = "110703";
			response.EM_MessageText = "B013910SV9NF                                               110703               90O153000112000000392501N                                                       91B6 1  15300011200000039                  1202142147                           9502120 UNCONCURRENCE DATA ACCEPTED                                             Y  3910SV9NF00001";
			declaration.Messages.Add(response);
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();
			sentMail = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("FZ Event Reporting Response for B00001000", sentMail.Subject);
			AssertContains("UNCONCURRENCE DATA ACCEPTED", sentMail.Body);
		}

		public void TestFTZStatusUpdateMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.FTZZoneID = "0250C00";
			declaration.FTZYear = "15";
			declaration.FTZControlNumber = "AAL0024N";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2710119000";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var addMock = Factory.NewMoq<MQEDIMessage>();
			addMock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var admissionAddTransmit = addMock.Object;
			admissionAddTransmit.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			admissionAddTransmit.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			admissionAddTransmit.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData;
			admissionAddTransmit.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZAdmissionAdd;
			admissionAddTransmit.EM_MessageNum = "HYEDUSDAT_1889";
			admissionAddTransmit.EM_MessageText = "B011234XJ5FT                                               HYEDUSDAT_1889       10A0250C0015AAL0024N5203NXJ5         13-150279800                               20A10MSBVNORD OPTIMISER V.2415  2415           2015090920150916520320150916     40MSBV090915NOP005                                       0000000001VE30771M902  50000012710191600    VE000000000000   000000000000                              5100008365270000004017050000000000N00050213                                     60JOB#43838 - JET FUEL - KEROSENE TYPE;        IM 13-150279800                  Y  1234XJ5FT00006";
			declaration.Messages.Add(admissionAddTransmit);

			var admissionAddResponse = Factory.New<MQEDIMessage>();
			admissionAddResponse.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			admissionAddResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			admissionAddResponse.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			admissionAddResponse.EM_Status = EDIMessage.Status.Queued;
			admissionAddResponse.EM_MessageNum = "HYEDUSDAT_1889";
			admissionAddResponse.EM_MessageText = "B011234BERNF                                               HYEDUSDAT_1889       90A0250C0015AAL0024N5203N                                                       91B1 1  0250C0015AAL0024N                  1509152329                           95  083 ZONE ADMISSION DATA ACCEPTED                                            Y  1234BERNF00001";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			var newFactory = new BusinessObjectFactory();
			var loadedDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			var loadedResponseMessage = newFactory.Load<MQEDIMessage>(admissionAddResponse.PK);
			AssertEquals(FTZMessageStatusList.Codes.ClearFTZAdmissionAdd, loadedDeclaration.AdmissionStatus);
			AssertEquals(EM_MessageSubTypeList.Descriptions.FTZAdmissionAdd, loadedResponseMessage.MessageTypeDescription);

			admissionAddResponse = Factory.New<MQEDIMessage>();
			admissionAddResponse.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			admissionAddResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			admissionAddResponse.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			admissionAddResponse.EM_Status = EDIMessage.Status.Queued;
			admissionAddResponse.EM_MessageNum = "HYEDUSDAT_1889";
			admissionAddResponse.EM_MessageText = "B001234BERNF                                               HYEDUSDAT_1889       90A0250C0015AAL0024N5203N                                                       91B4 1  0250C0015AAL0024N                  1509152330                           95     FTZ PAPERLESS ADMISSION                                                  Y  1234BERNF00003";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			newFactory = new BusinessObjectFactory();
			loadedDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			loadedResponseMessage = newFactory.Load<MQEDIMessage>(admissionAddResponse.PK);
			AssertEquals(FTZMessageStatusList.Codes.ClearFTZAdmissionAdd, loadedDeclaration.AdmissionStatus);
			AssertEquals(EM_MessageSubTypeList.Descriptions.FTZStatusUpdate, loadedResponseMessage.MessageTypeDescription);

			var deleteMock = Factory.NewMoq<MQEDIMessage>();
			deleteMock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var admissionDeleteTransmit = deleteMock.Object;
			admissionDeleteTransmit.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			admissionDeleteTransmit.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			admissionDeleteTransmit.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData;
			admissionDeleteTransmit.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZAdmissionDelete;
			admissionDeleteTransmit.EM_MessageNum = "HYEDUSDAT_1890";
			admissionDeleteTransmit.EM_MessageText = "B011234XJ5FT                                               HYEDUSDAT_1890       10D0250C0015AAL0024N5203NXJ5         13-150279800                               Y  1234XJ5FT00001";
			declaration.Messages.Add(admissionDeleteTransmit);

			var admissionDeleteResponse = Factory.New<MQEDIMessage>();
			admissionDeleteResponse.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			admissionDeleteResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			admissionDeleteResponse.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			admissionDeleteResponse.EM_Status = EDIMessage.Status.Queued;
			admissionDeleteResponse.EM_MessageNum = "HYEDUSDAT_1890";
			admissionDeleteResponse.EM_MessageText = "B011234XJ5NF                                               HYEDUSDAT_1890       90 0250C0015AAL0024N5203N                                                       91B1 1  0250C0015AAL0024N                  1509161447                           95  084 ZONE ADMISSION DELETED                                                  Y  1234XJ5NF00001";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			newFactory = new BusinessObjectFactory();
			loadedDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			loadedResponseMessage = newFactory.Load<MQEDIMessage>(admissionDeleteResponse.PK);
			AssertEquals(FTZMessageStatusList.Codes.ClearFTZAdmissionDelete, loadedDeclaration.AdmissionStatus);
			AssertEquals(EM_MessageSubTypeList.Descriptions.FTZAdmissionDelete, loadedResponseMessage.MessageTypeDescription);

			var newAddMock = Factory.NewMoq<MQEDIMessage>();
			newAddMock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			admissionAddTransmit = newAddMock.Object;
			admissionAddTransmit.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			admissionAddTransmit.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			admissionAddTransmit.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData;
			admissionAddTransmit.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZAdmissionAdd;
			admissionAddTransmit.EM_MessageNum = "HYEDUSDAT_1891";
			admissionAddTransmit.EM_MessageText = "B011234XJ5FT                                               HYEDUSDAT_1891       10A0250C0015AAL0024N5203NXJ5         13-150279800                               20A10MSBVNORD OPTIMISER V.2415  2415           2015090920150916520320150916     40MSBV090915NOP005                                       0000000001VE30771M902  50000012710191600    VE000000000000   000000000000                              5100008365270000004017050000000000N00050213                                     60JOB#43838 - JET FUEL - KEROSENE TYPE;        IM 13-150279800                  Y  1234XJ5FT00006";
			declaration.Messages.Add(admissionAddTransmit);

			admissionAddResponse = Factory.New<MQEDIMessage>();
			admissionAddResponse.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			admissionAddResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			admissionAddResponse.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			admissionAddResponse.EM_Status = EDIMessage.Status.Queued;
			admissionAddResponse.EM_MessageNum = "HYEDUSDAT_1891";
			admissionAddResponse.EM_MessageText = "B011234BERNF                                               HYEDUSDAT_1891       90A0250C0015AAL0024N5203N                                                       91B1 1  0250C0015AAL0024N                  1509152329                           95  083 ZONE ADMISSION DATA ACCEPTED                                            Y  1234BERNF00001";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			newFactory = new BusinessObjectFactory();
			loadedDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			loadedResponseMessage = newFactory.Load<MQEDIMessage>(admissionAddResponse.PK);
			AssertEquals(FTZMessageStatusList.Codes.ClearFTZAdmissionAdd, loadedDeclaration.AdmissionStatus);
			AssertEquals(EM_MessageSubTypeList.Descriptions.FTZAdmissionAdd, loadedResponseMessage.MessageTypeDescription);

			admissionDeleteResponse = Factory.New<MQEDIMessage>();
			admissionDeleteResponse.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			admissionDeleteResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			admissionDeleteResponse.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			admissionDeleteResponse.EM_Status = EDIMessage.Status.Queued;
			admissionDeleteResponse.EM_MessageNum = "HYEDUSDAT_1890";
			admissionDeleteResponse.EM_MessageText = "B001234XJ5NF                                               HYEDUSDAT_1893       90A0250C0015AAL0024N5203N                                                       9155 2  MSBV090915NOP004                   1509170953MSBV                       95     CARR AMEND ADD                                                           Y  1234XJ5NF00003";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			newFactory = new BusinessObjectFactory();
			loadedDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			loadedResponseMessage = newFactory.Load<MQEDIMessage>(admissionDeleteResponse.PK);
			AssertEquals(FTZMessageStatusList.Codes.ClearFTZAdmissionAdd, loadedDeclaration.AdmissionStatus);
			AssertEquals(EM_MessageSubTypeList.Descriptions.FTZStatusUpdate, loadedResponseMessage.MessageTypeDescription);
		}

		public void TestFTZResponseShouldNotBeTreatedAsStatusUpdate()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.FTZZoneID = "2810065";
			declaration.FTZYear = "18";
			declaration.FTZControlNumber = "00794059";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2710119000";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var addMock = Factory.NewMoq<MQEDIMessage>();
			addMock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var admissionAddTransmit = addMock.Object;
			admissionAddTransmit.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			admissionAddTransmit.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			admissionAddTransmit.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData;
			admissionAddTransmit.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZAdmissionAdd;
			admissionAddTransmit.EM_MessageNum = "GCBMIAMIA_20478";
			admissionAddTransmit.EM_MessageText = "B0152019K9FT                                               GCBMIAMIA_20478      10A281006518007940595201Y9K9         42-153900600                               20A30LUISLUIS DELIVERIES INC    0000           2018032120180321520620180321     40LUIS437512261                                          0000000002US     LA72  41437512261                                                                     50000013211000000    US000000056000KG                                           5100000005600000000028750000000750N00000000                                     60DRUMS OF PREPARED DRIERS ZIRCONIUM 24%       IM 42-153900600                  60                                             MIDUSCHEINT3550MIA               Y  52019K9FT00008";
			declaration.Messages.Add(admissionAddTransmit);

			var admissionAddResponse = Factory.New<MQEDIMessage>();
			admissionAddResponse.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			admissionAddResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			admissionAddResponse.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			admissionAddResponse.EM_Status = EDIMessage.Status.Queued;
			admissionAddResponse.EM_MessageNum = "GCBMIAMIA_20478";
			admissionAddResponse.EM_MessageText = "B0052019K9NF                                               GCBMIAMIA_20478      90A281006518007940595201Y                                                       91B2 1  28100651800794059                  1803211833    LA72                   40LUIS437512261                                          0000000002US     LA72  50000013211000000    US000000056000KG                                           5100000005600000000028750000000750N00000000                                     95  27AIMPROBABLE COUNTRY                                                       95  083ZONE ADMISSION DATA ACCEPTED                                             9503   DATA ACCEPTED WITH WARNINGS                                              Y  52019K9NF00000";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			var newFactory = new BusinessObjectFactory();
			var loadedDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			var loadedResponseMessage = newFactory.Load<MQEDIMessage>(admissionAddResponse.PK);
			AssertEquals(FTZMessageStatusList.Codes.ClearFTZAdmissionAddWithWarnings, loadedDeclaration.AdmissionStatus);
			AssertEquals(EM_MessageSubTypeList.Descriptions.FTZAdmissionAdd, loadedResponseMessage.MessageTypeDescription);
		}

		public void TestCS00399387_B15716A006852()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.FTZZoneID = "0390001";
			declaration.FTZYear = "16";
			declaration.FTZControlNumber = "TOS01012";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2710119000";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var addMock = Factory.NewMoq<MQEDIMessage>();
			addMock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var admissionAddTransmit = addMock.Object;
			admissionAddTransmit.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			admissionAddTransmit.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			admissionAddTransmit.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData;
			admissionAddTransmit.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZAdmissionAdd;
			admissionAddTransmit.EM_MessageNum = "CEVHOUHOU_184175";
			admissionAddTransmit.EM_MessageText = "B015501004FT                                               CEVHOUHOU_184175     10A039000116TOS010125501N004         33-079534400                               20A40KE  KOREAN AIR LINES CO. IN031            2016011520160116550120160116     4018090727781                        93633406            0000000003PH     T301  50000018471704065    PH000000005200NO 000000000000                              5100000000250000000059380000000087N00000000                                     60HDD                                          IM 33-079534400                  60                                             MIDJPTOSCOR11TOK                 4018090727781                        93633418            0000000011PH     T301  50000018471704065    PH000000009800NO 000000000000                              5100000000430000000103240000000152N00000000                                     60CA07069-B200 MBE2147RC AL11SX SFF 1          IM 33-079534400                  60                                             MIDJPTOSCOR11TOK                 4018090727884                        93633424            0000000001PH     T301  50000018471704065    PH000000235000NO 000000000000                              5100000003080000000733200000001077N00000000                                     60HDD                                          IM 33-079534400                  60                                             MIDJPTOSCOR11TOK                 Y  5501004FT00017";
			declaration.Messages.Add(admissionAddTransmit);

			var admissionAddResponse = Factory.New<MQEDIMessage>();
			admissionAddResponse.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			admissionAddResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			admissionAddResponse.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			admissionAddResponse.EM_Status = EDIMessage.Status.Queued;
			admissionAddResponse.EM_MessageNum = "CEVHOUHOU_184175";
			admissionAddResponse.EM_MessageText = "B015501004NF                                               CEVHOUHOU_184175     90A039000116TOS010125501N                                                       91B1 1  039000116TOS01012                  1601161354                           95  083 ZONE ADMISSION DATA ACCEPTED                                            Y  5501004NF00001";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			var newFactory = new BusinessObjectFactory();
			var loadedDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			var cesLog = loadedDeclaration.Logs.MostRecentLogByEventTime(Enterprise.ZArchitecture.Business.AutoEvents.CustomsEntryStatus);
			AssertEquals(FTZMessageStatusList.Codes.ClearFTZAdmissionAdd, cesLog.SL_Reference);
			Assert("CFA (clear message) log is not cancelled", !cesLog.SL_IsCancelled);

			cesLog.Cancel();
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Enterprise.ZArchitecture.Business.AutoEvents.EditedARecordCode);
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "CFA Event Cancelled");
			AssertNotNull("When CFA log is cancelled, a new EDT log is added which provide more information", loadedDeclaration.Logs.HasLogWith(query));
		}

		public void TestCS00399387_B15716A002994()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
			declaration.FTZZoneID = "0390001";
			declaration.FTZYear = "16";
			declaration.FTZControlNumber = "TOS01015";

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2710119000";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var addMock = Factory.NewMoq<MQEDIMessage>();
			addMock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			var admissionAddTransmit = addMock.Object;
			admissionAddTransmit.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			admissionAddTransmit.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			admissionAddTransmit.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.FTZAdmissionData;
			admissionAddTransmit.EM_MessageSubType = EM_MessageSubTypeList.Codes.FTZAdmissionAdd;
			admissionAddTransmit.EM_MessageNum = "CEVHOUHOU_165742";
			admissionAddTransmit.EM_MessageText = "B015501004FT                                               CEVHOUHOU_165742     10A039000116TOS010115501N004         33-079534400                               20A40KE  KOREAN AIR LINES CO. IN031            2016010820160109550120160109     4018090728551                        93633293            0000000006PH     T301  50000018471704065    PH000000006500NO 000000000000                              5100000000290000000063440000000250N00000000                                     60CA07173-B400 MBF2600RC AL12SE 600GB          IM 33-079534400                  60                                             MIDJPTOSCOR11TOK                 Y  5501004FT00007";
			declaration.Messages.Add(admissionAddTransmit);

			var admissionAddResponse = Factory.New<MQEDIMessage>();
			admissionAddResponse.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			admissionAddResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			admissionAddResponse.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.ForeignTradeZone;
			admissionAddResponse.EM_Status = EDIMessage.Status.Queued;
			admissionAddResponse.EM_MessageNum = "CEVHOUHOU_165742";
			admissionAddResponse.EM_MessageText = "B015501004NF                                               CEVHOUHOU_165742     90A039000116TOS010115501N                                                       91B1 1  039000116TOS01011                  1601091510                           95  083 ZONE ADMISSION DATA ACCEPTED                                            Y  5501004NF00001";
			Factory.Save();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			var newFactory = new BusinessObjectFactory();
			var loadedDeclaration = newFactory.Load<JobDeclaration>(declaration.PK);
			var cesLog = loadedDeclaration.Logs.MostRecentLogByEventTime(Enterprise.ZArchitecture.Business.AutoEvents.CustomsEntryStatus);
			AssertEquals(FTZMessageStatusList.Codes.ClearFTZAdmissionAdd, cesLog.SL_Reference);
			Assert("CFA (clear message) log is not cancelled", !cesLog.SL_IsCancelled);

			cesLog.Cancel();
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Enterprise.ZArchitecture.Business.AutoEvents.EditedARecordCode);
			query.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "CFA Event Cancelled");
			AssertNotNull("When CFA log is cancelled, a new EDT log is added which provide more information", loadedDeclaration.Logs.HasLogWith(query));
		}
	}
}
