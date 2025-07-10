using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	public class UEMInboundInterchangeProcessorTest : IncomingInterchangeProcessorTest
	{
		public void TestApplicationCodes()
		{
			var applicationCodes = uemInboundInterchangeProcessorForTest.GetApplicationCodes();
			AssertEquals("Number of ApplicationCodes.", 1, applicationCodes.Length);
			AssertEquals("UEM - USExportManifest", EDIInterchange.ApplicationCodes.USExportManifest, applicationCodes[0]);
		}

		public void TestIsNoBranchFilter()
		{
			AssertEquals("IsNoBranchFilter", true, uemInboundInterchangeProcessorForTest.GetIsNoBranchFilter());
		}

		public void TestGetMessageCreator()
		{
			var messageCreator = uemInboundInterchangeProcessorForTest.GetMessageCreator_Exposed(null);
			AssertEquals("UEMInboundMessageCreator", "UEMInboundMessageCreator", messageCreator.GetType().Name);
		}

		public void TestUEMInboundMessageCreator()
		{
			var interchange = Factory.New<UEMEDIInterchange>();
			interchange.EI_Status = EDIMessageStatusList.Codes.Queued;
			interchange.EI_GB = Env.CurrentBranchPK;
			interchange.EI_BodyText = "Test Body";

			var messageCreator = uemInboundInterchangeProcessorForTest.GetMessageCreator_Exposed(interchange);
			messageCreator.CreateMessagesForInterchange(interchange);

			AssertEquals("EI_Status", EDIInterchange.Status.Queued, interchange.EI_Status);

			var message = interchange.ContainedMessages[0];

			CombineAssertions("Create UEMEDIMessage through UEMEDIInterchange.", () =>
			{
				AssertNotNull("UEMEDIMessage", message);
				AssertEquals("ContainedMessages count", 1, interchange.ContainedMessages.Count);
				AssertEquals("Message was added to EDIInterchange ContainedMessages.", interchange.ContainedMessages[0].PK, message.PK);
				AssertEquals("EM_MessageType", MessageTypeList.Codes.ExportManifestResponse, message.EM_MessageType);
				AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
				AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
				AssertEquals("EM_GB", Env.CurrentBranchPK, message.EM_GB);
				AssertEquals("EM_MessageText", "Test Body", message.EM_MessageText);
				AssertEquals("EM_MessageNum", "00001", message.EM_MessageNum);
			});
		}

		public void TestUEMInboundMessageCreatorWithInvalidInterchange()
		{
			var interchange = Factory.New<EDIInterchange>();
			var messageCreator = uemInboundInterchangeProcessorForTest.GetMessageCreator_Exposed(interchange);
			MessageProcessException exception = null;
			try
			{
				messageCreator.CreateMessagesForInterchange(interchange);
			}
			catch (MessageProcessException ex)
			{
				exception = ex;
			}

			AssertNotNull("A MessageProcessException was catched.", exception);
			AssertEquals("Invalid interchange type.", "Interchange type should be UEMEDIInterchange.", exception.Message);
		}

		protected override void SetUp()
		{
			base.SetUp();
			uemInboundInterchangeProcessorForTest = new UEMInboundInterchangeProcessorForTest(new LoggingInformation());
		}

		UEMInboundInterchangeProcessorForTest uemInboundInterchangeProcessorForTest;
	}

	class UEMInboundInterchangeProcessorForTest : UEMInboundInterchangeProcessor
	{
		public UEMInboundInterchangeProcessorForTest(LoggingInformation logger) : base(logger)
		{
		}

		public string[] GetApplicationCodes()
		{
			return ApplicationCodes;
		}

		public bool GetIsNoBranchFilter()
		{
			return IsNoBranchFilter;
		}

		public IInboundMessageCreator GetMessageCreator_Exposed(EDIInterchange interchange)
		{
			return GetMessageCreator(interchange);
		}
	}
}
