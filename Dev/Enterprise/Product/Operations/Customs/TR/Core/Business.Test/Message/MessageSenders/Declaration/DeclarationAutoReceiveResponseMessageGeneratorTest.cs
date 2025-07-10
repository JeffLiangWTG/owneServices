using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Customs.TR.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business.Testing
{
	public class DeclarationAutoReceiveResponseMessageGeneratorTest : TestCaseWithFactory
	{
		public void TestMessageType()
		{
			var sender = GetSender();
			AssertEquals("MessageType", TRMessageTypes.Codes.DK1, sender.MessageType);

			sender = GetSenderDTE(ZGuid.NewZGuid().ToString());
			AssertEquals("MessageType", TRMessageTypes.Codes.DT1, sender.MessageType);
		}

		public void TestWhoSendThisMessage()
		{
			var group = Factory.New<GlbGroup>();
			var staff = group.Staff.AddNew();
			staff.GS_Code = "KNZ";
			var originalMessage = Factory.New<TRImportExportMessage>();
			originalMessage.EM_SystemCreateUser = "KNZ";
			var sender = GetSender("344d5a52-938e-4fc4-b38b-c58d2f03319d", originalMessage);
			AssertEquals("MessageOwner", "KNZ", sender.WhoSendThisMessageExposed.GS_Code);
		}

		public void TestMessageText()
		{
			var group = Factory.New<GlbGroup>();
			var staff = group.Staff.AddNew();
			staff.GS_Code = "KNZ";
			var user = TRGlbStaffWrapper.Get(staff).TRBPassword;
			user.GP_UserID = "20201224104";
			user.CurrentDecryptedPassword = "12345678";
			var originalMessage = Factory.New<TRImportExportMessage>();
			originalMessage.EM_SystemCreateUser = "KNZ";
			var sender = GetSender("b5d51907-d1bf-4852-91c6-5c33e535a167", originalMessage);
			AssertEquals("MessageText", TRMessageTestHelper.GetFileText("Declaration.Expected.IslemSonucGetir2.xml"), sender.MessageTextExposed);
		}

		DeclarationAutoReceiveResponseMessageGenerator GetSender(TRImportExportMessage originalMessage = null)
		{
			return GetSender(ZGuid.NewZGuid().ToString(), originalMessage);
		}

		ImportExportAutoReceiveResponseMessageSenderFortest GetSender(ZString guid, TRImportExportMessage originalMessage = null)
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var provider = new DeclarationAutoReceiveResponseMessageProvider(cusEntryHeader);
			return new ImportExportAutoReceiveResponseMessageSenderFortest(provider, guid, originalMessage);
		}

		DeclarationAutoReceiveResponseMessageGenerator GetSenderDTE(ZString guid, TRImportExportMessage originalMessage = null)
		{
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var provider = new DeclarationAutoReceiveResponseMessageProvider(cusEntryHeader);
			return new DeclarationAutoReceiveResponseMessageGenerator(provider, guid, originalMessage, "DTE");
		}
	}

	class ImportExportAutoReceiveResponseMessageSenderFortest : DeclarationAutoReceiveResponseMessageGenerator
	{
		public ImportExportAutoReceiveResponseMessageSenderFortest(IMessageSender sender, ZString guid, EDIMessage originalMessage) : base(sender, guid, originalMessage)
		{
		}

		public ZString MessageTextExposed => base.MessageText;

		public GlbStaff WhoSendThisMessageExposed => base.WhoSendThisMessage;
	}
}
