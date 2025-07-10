using System.Linq;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	[TestedType(typeof(T1InterchangeUnpacker))]
	sealed class T1InterchangeUnpackerTest : InterchangeUnpackerTest<T1InterchangeUnpacker>
	{
		public void TestUnpack()
		{
			var branchPK = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).Branches.AddNew().PK;
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_GB = branchPK;
			interchange.EI_InterchangeNum = "ICS22023001";
			interchange.EI_BodyText = "Invalid Data";
			interchange.EI_InterchangeType = "TST";

			var unpacker = new T1InterchangeUnpacker(0);
			var logger = new LoggingInformation();
			var unpackResult = unpacker.Unpack(interchange, null, null, logger);

			CombineAssertions(() =>
			{
				AssertEquals(1, interchange.ContainedMessages.Count);
				Assert("Unpack Success", unpackResult.IsSuccess);

				var message = unpackResult.EdiMessages.Single();
				AssertSame(message, interchange.ContainedMessages[0]);
				AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Receive, message.EM_ReceiveTransmit);
				AssertEquals("EM_Status", EDIMessageStatusList.Codes.Queued, message.EM_Status);
				AssertEquals("EM_GB", branchPK, message.EM_GB);
				AssertEquals("EM_MessageNum", "ICS22023001", message.EM_MessageNum);
				AssertEquals("EM_MessageType", "TST", message.EM_MessageType);
				AssertEquals("EM_MessageData", "Invalid Data", message.EM_MessageText);
			});
		}

		public override void TestCorrectSubscribeToUCUSubscribers()
		{
			Assert("No need", true);
		}

		protected override string[] ApplicationCodes => throw new System.NotImplementedException();
	}
}
