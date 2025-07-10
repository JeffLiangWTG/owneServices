using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class NCTSMessageProcessorTest : TestCaseWithFactory
	{
		public void TestApplicationCode()
		{
			AssertEquals(EDIMessage.ApplicationCodes.TRCustoms, processor.ApplicationCode);
		}

		public void TestGetCorrectBranchPK()
		{
			var header = Factory.New<Integration.Customs.TR.ICusInBondHeader>();
			var pk = ZGuid.NewZGuid();
			header.BH_GB = pk;
			AssertEquals(pk, processor.GetCorrectBranchPK((BusinessObject)header));
			AssertEquals(ZGuid.Empty, processor.GetCorrectBranchPK(null));
		}

		protected override void SetUp()
		{
			base.SetUp();

			processor = new NCTSMessageProcessorForTest(new LoggingInformation());
		}

		NCTSMessageProcessorForTest processor;

		class NCTSMessageProcessorForTest : NCTSMessageProcessor
		{
			public NCTSMessageProcessorForTest(LoggingInformation logger) : base(logger)
			{
			}

			protected override string MessageFriendlyNameCore => "NCTS Message Processor For Test";

			protected override ZString CreateEmailBodyCore(IMessageAttachee messageAttacheeBO, NCTSMessage message, bool isSuccess) => ZString.Empty;

			public new ZGuid GetCorrectBranchPK(BusinessObject linkedObject) => base.GetCorrectBranchPK(linkedObject);

			protected override string MailSubject(IMessageAttachee messageAttacheeBO, NCTSMessage message) => ZString.Empty;

			protected override bool ProcessMessageCore(NCTSMessage message) => true;
		}
	}
}
