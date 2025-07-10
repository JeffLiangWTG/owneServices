using CargoWise.Common;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class MessageSenderBaseOnlyTest : TestCaseWithFactory
	{
		public void TestOnSave()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var messageSender = new MessageSenderTestClass(dec);
			AssertEquals(true, messageSender.SaveMessageExposed());
			var lastKeyReported = ErrorReporter.LastKeyReported;
			AssertEquals(string.Empty, lastKeyReported);
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		class MessageSenderTestClass : MessageSender
		{
			public MessageSenderTestClass(BaseJobDeclaration job)
				: base(job)
			{
			}

			public bool SaveMessageExposed()
			{
				return SaveMessage();
			}

			protected override bool Prepare()
			{
				return true;
			}

			protected override bool GenerateMessage()
			{
				return true;
			}
		}
	}
}
