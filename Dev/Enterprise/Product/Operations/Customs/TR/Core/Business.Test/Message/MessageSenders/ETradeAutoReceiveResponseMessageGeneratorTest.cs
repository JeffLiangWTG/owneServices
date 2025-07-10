using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	class ETradeAutoReceiveResponseMessageGeneratorTest : TestCaseWithFactory
	{
		public void TestMessageType()
		{
			var originalMessage = Factory.New<ETradeEDIMessage>();
			originalMessage.EM_MessageType = TRMessageTypes.Codes.TRS;
			var sender = GetGenerator(originalMessage);
			AssertEquals("MessageType", TRMessageTypes.Codes.T1S, sender.MessageType);

			originalMessage.EM_MessageType = TRMessageTypes.Codes.TRD;
			sender = GetGenerator(originalMessage);
			AssertEquals("MessageType", TRMessageTypes.Codes.T1D, sender.MessageType);

			originalMessage.EM_MessageType = TRMessageTypes.Codes.TCD;
			sender = GetGenerator(originalMessage);
			AssertEquals("MessageType", TRMessageTypes.Codes.T2D, sender.MessageType);

			originalMessage.EM_MessageType = TRMessageTypes.Codes.TRE;
			sender = GetGenerator(originalMessage);
			AssertEquals("MessageType", TRMessageTypes.Codes.T1E, sender.MessageType);
		}

		public void TestMessageText()
		{
			var group = Factory.New<GlbGroup>();
			var staff = group.Staff.AddNew();
			staff.GS_Code = "YE";
			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";
			var originalMessage = Factory.New<ETradeEDIMessage>();
			originalMessage.EM_SystemCreateUser = "YE";

			originalMessage.EM_MessageType = TRMessageTypes.Codes.TRS;
			var sender = GetGenerator("b5d51907-d1bf-4852-91c6-5c33e535a167", originalMessage);
			AssertEquals("MessageText", TRMessageTestHelper.GetFileText("ETrade.ExportRegistrationNo.ServisCevabiSorgulama.xml"), sender.MessageTextExposed);

			originalMessage.EM_MessageType = TRMessageTypes.Codes.TCD;
			sender = GetGenerator("4142285b-6b4f-4eb8-9bfa-6baa3b884b06", originalMessage);
			AssertEquals("MessageText", TRMessageTestHelper.GetFileText("ETrade.ImportSendforComplementaryDeclaration.ServisCevabiSorgulama.xml"), sender.MessageTextExposed);

			originalMessage.EM_MessageType = TRMessageTypes.Codes.TRE;
			sender = GetGenerator("4142285b-6b4f-4eb8-9bfa-6baa3b884b06", originalMessage);
			AssertEquals("MessageText", TRMessageTestHelper.GetFileText("ETrade.TemporaryRegistration.ServisCevabiSorgulama.xml"), sender.MessageTextExposed);

			originalMessage.EM_MessageType = TRMessageTypes.Codes.TRD;
			sender = GetGenerator("4142285b-6b4f-4eb8-9bfa-6baa3b884b06", originalMessage);
			AssertEquals("MessageText", TRMessageTestHelper.GetFileText("ETrade.ImportDischargeList.ServisCevabiSorgulama.xml"), sender.MessageTextExposed);
		}

		ETradeAutoReceiveResponseMessageGeneratorForTest GetGenerator(ETradeEDIMessage originalMessage)
		{
			return GetGenerator(ZGuid.NewZGuid().ToString(), originalMessage);
		}

		ETradeAutoReceiveResponseMessageGeneratorForTest GetGenerator(ZString guid, ETradeEDIMessage originalMessage)
		{
			var header = Factory.New<Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader>();
			var provider = new ETradeAutoReceiveResponseMessageProvider(header);
			return new ETradeAutoReceiveResponseMessageGeneratorForTest(provider, guid, originalMessage);
		}
	}

	class ETradeAutoReceiveResponseMessageGeneratorForTest : ETradeAutoReceiveResponseMessageGenerator
	{
		public ETradeAutoReceiveResponseMessageGeneratorForTest(IMessageSender sender, ZString guid, EDIMessage originalMessage) : base(sender, guid, originalMessage)
		{
		}

		public ZString MessageTextExposed => base.MessageText;
	}
}
