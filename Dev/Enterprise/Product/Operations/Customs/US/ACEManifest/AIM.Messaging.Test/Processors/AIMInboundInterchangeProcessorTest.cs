using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.US.AIM.Messaging.Testing
{
	public class AIMInboundInterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestApplicationCodes()
		{
			var processor = new AIMInboundInterchangeProcessorForTest(new LoggingInformation());
			var applicationCodes = processor.ApplicationCodesExposed;
			AssertCollectionContains(ApplicationCodeList.Codes.USAMA, applicationCodes);
			AssertEquals("Code Count", 1, applicationCodes.Length);
		}

		public void TestIsNoBranchFilter()
		{
			var processor = new AIMInboundInterchangeProcessorForTest(new LoggingInformation());
			AssertEquals("IsNoBranchFilter", true, processor.IsNoBranchFilterExposed);
		}

		public void TestProcessInterchange()
		{
			var creationCompany = Factory.New<GlbCompany>();
			creationCompany.GC_RN_NKCountryCode = "NZ";
			var creationBranch = creationCompany.Branches.AddNew();
			creationBranch.GB_Code = "CRB";
			creationBranch.GB_RL_NKHomePort = "NZAKL";

			var processingBranch = Factory.New<GlbBranch>();
			processingBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			processingBranch.GB_Code = "PRB";
			processingBranch.GB_RL_NKHomePort = "AUPER";
			Factory.Save();

			var msgText = @"FER
QF101/12DEC
081-11223344-HAWB123/123456
ERR/001ERRORDESCRIPTION
ERR/002ANOTHERERROR
";

			AIMEDIInterchange interchange;
			using (Environment.DisposableEnvironment.ForBranch(creationBranch.PK.ToGuid()))
			{
				interchange = Factory.New<AIMEDIInterchange>();
				interchange.EI_InterchangeType = EDIMessageTypeList.Codes.FHL;
				interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
				interchange.EI_Status = EDIInterchange.Status.Queued;
				interchange.EI_From = "USC";
				interchange.EI_To = "SV9";
				interchange.EI_HeaderText = "WASUCCR\x0D\x0A.BCBTSV9";
				interchange.EI_BodyText = msgText;
				Factory.Save();
			}
			AssertEquals("EI_GB", creationBranch.PK, interchange.EI_GB);

			using (Environment.DisposableEnvironment.ForBranch(processingBranch.PK.ToGuid()))
			{
				var logger = new LoggingInformation();
				var processor = new AIMInboundInterchangeProcessorForTest(logger);
				processor.ExecuteBatch();
				Factory.Save();
			}

			interchange.Reload();
			AssertEquals("EI_Status", EDIMessage.Status.Received, interchange.EI_Status);

			var message = (AIMEDIMessage)interchange.ContainedMessages[0];
			AssertEquals("EM_ReceiveTransmit", message.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			AssertEquals("EM_Status", message.EM_Status, EDIMessage.Status.Queued);
			AssertEquals("EM_MessageText", msgText, message.EM_MessageText);
			AssertEquals("EM_MessageType", message.EM_MessageType, EDIMessageTypeList.Codes.FHL);
			AssertEquals("EM_MessageSubType", message.EM_MessageSubType, Constants.AIMMessageSubTypes.FER);
			AssertEquals("EM_GB", interchange.EI_GB, message.EM_GB);
			Assert("EM_MessageNum", !message.EM_MessageNum.IsEmpty);
		}
	}

	class AIMInboundInterchangeProcessorForTest : AIMInboundInterchangeProcessor
	{
		public AIMInboundInterchangeProcessorForTest(LoggingInformation logger) : base(logger)
		{
		}

		public string[] ApplicationCodesExposed => ApplicationCodes;
		public bool IsNoBranchFilterExposed => IsNoBranchFilter;
	}
}
