using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AutoSendAESTIRMessageProcessor))]
	sealed class AutoSendAESTIRMessageProcessorTest : USAutoSendCustomsMessageProcessorTest
	{
		protected override void PrepareDeclaration(BaseJobDeclaration declaration)
		{
			var usDeclaration = (JobDeclaration)declaration;
			usDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		}

		protected override IProcessor CreateProcessor(BaseJobDeclaration declaration)
		{
			return new AutoSendAESTIRMessageProcessor((JobDeclaration)declaration);
		}

		protected override Customs.Business.CusEntryHeader GetEntryHeader(BaseJobDeclaration declaration)
		{
			return ((JobDeclaration)declaration).ActiveEntryHeaders.OfType<CusEntryHeader>().FirstOrDefault(x => x.CH_MessageType == CusEntryHeaderMessageTypeList.Codes.Export);
		}

		protected override void AssertEntryAndMessageResultForEndToEndTest(Customs.Business.CusEntryHeader entry)
		{
			AssertEquals("Original AES message should be generated", 1, entry.Messages.Count);
			AssertEquals(AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse, entry.CH_Status);
			var originalMessage = entry.Messages[0];
			AssertEquals(ApplicationIdentifierCodeList.AES.CommodityShipment, originalMessage.EM_MessageType);
			AssertEquals(EM_MessageSubTypeList.Codes.SEDAdd, originalMessage.EM_MessageSubType);
			AssertEquals(MQEDIMessage.Status.Queued, originalMessage.EM_Status);
		}

		protected override void SetEntryClearedStatus(Customs.Business.CusEntryHeader entry)
		{
			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
		}

		protected override ZString ExpectedMessageDescription => "SED";

		public void TestSendReplacement()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			entry.US_ShouldBeReportToCustoms = true;
			Factory.Save();

			IProcessor processor = new AutoSendAESTIRMessageProcessor(declaration);
			var notifications = new NotificationBuffer();
			processor.Process(notifications);
			AssertContains("message has been sent to customs for Job", notifications.AsString);

			AssertEquals("Replacement AES message should be generated", 1, entry.Messages.Count);
			var replaceMessage = entry.Messages[0];
			AssertEquals(ApplicationIdentifierCodeList.AES.CommodityShipment, replaceMessage.EM_MessageType);
			AssertEquals(EM_MessageSubTypeList.Codes.SEDReplace, replaceMessage.EM_MessageSubType);
			AssertEquals(MQEDIMessage.Status.Queued, replaceMessage.EM_Status);
		}
	}
}
