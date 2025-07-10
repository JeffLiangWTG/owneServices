using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output;
using Enterprise.Environment;
using Moq.Protected;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	public class ACEPriorNoticeMessageProcessorTest : ABIProcessorTest<ACEPriorNoticeMessageProcessor, AABIOutputA, AABIOutputB, AABIOutputY>
	{
		protected override void EndToEndCore()
		{
			incomingMessage.EM_MessageText =
"B  3901SV9PX                                               HYEDUSCMT_165024     " +
"PE10AABOL 00000000012                                           APLU0110        " +
"PE9002   ACCEPTED                                50060012002                    " +
"OI        STANDALONE PRIOR NOTICE MESSAGE                                       " +
"PG01001FDAFOOFEE                                                                " +
"PG02PFDP                                                                        " +
"PG06262FR                                                                       " +
"PG07TEST FOOD                                                                   " +
"PG10                   SOMETHING                                                " +
"PG19PNT                  EDI CUSTOMS BROKERS             10 HUTCHESON STREET    " +
"PG20ALBION  QLD                                                  AU4010         " +
"PG25          1                                                                 " +
"Y  3901SV9PX";
			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new ABIIncomingMessageProcessor().ExecuteBatch();

			incomingMessage.Reload();
			AssertEquals("Should be processed successfully", "RCV", incomingMessage.EM_Status);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("<td>ACCEPTED</td>", email.Body);
			AssertEquals(ZString.Empty, declaration.FDAMsgStatus);
		}

		public void TestFailedResponse()
		{
			incomingMessage.EM_MessageText =

"B003901SV9PX                                               HYEDUSCMT_165024     " +
"PE10APAWB    001123456789                                           0111        " +
"PE9011PAIMISSING CARRIER INFORMATION PER PGA                                    " +
"PE9011TBDMSNG OR INVALID FILING TYPE CODE                                       " +
"PE9011PY8INVALID ENTRY NUMBER                                                   " +
"OI        LETTUCE NSF                                                           " +
"PE9011P42MORE THAN ONE OI UNDER SINGLE HTS                                      " +
"PG01002FDAFOOPRO                                                                " +
"PG02PFDP 02DGT02                                                                " +
"PG30A            1101                                                           " +
"PE9011PD5INVALID INSPEC/LAB TESTING DATE FORMAT                                 " +
"PE9011PN6MISSING REQUESTED DATE OR TIME PER PGA                                 " +
"PE9001   PE DATA REJECTED                                                       " +
"Y  3901SV9PX00000                                                               ";

			Factory.Save();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new ABIIncomingMessageProcessor().ExecuteBatch();
			incomingMessage.Reload();
			AssertEquals("Should be processed successfully", "RCV", incomingMessage.EM_Status);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("<td>PE DATA REJECTED</td>", email.Body);
			AssertEquals(ZString.Empty, declaration.FDAMsgStatus);
		}

		protected override ZInt EmailsExpectedAtCompletionOfEndToEndTest
		{
			get { return 1; }
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();

			var mock = Factory.NewMoq<MQEDIMessage>();
			mock.Protected().Setup("GetNumberFountainNumbersAndFillInPlaceHolders");

			outgoing = mock.Object;
			outgoing.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			outgoing.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoing.EM_Status = "SNT";
			outgoing.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.PriorNotice;
			outgoing.EM_MessageNum = "HYEDUSCMT_165024";
			declaration.Messages.Add(outgoing);

			incomingMessage = Factory.New<MQEDIMessage>();
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.USCustomsImport;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_Status = "QUE";
			incomingMessage.EM_MessageType = ACEApplicationIdentifierCodeList.Codes.PriorNoticeResponse;
			incomingMessage.EM_MessageNum = "HYEDUSCMT_165024";
		}

		MQEDIMessage incomingMessage;
		MQEDIMessage outgoing;
		JobDeclaration declaration;
	}
}
