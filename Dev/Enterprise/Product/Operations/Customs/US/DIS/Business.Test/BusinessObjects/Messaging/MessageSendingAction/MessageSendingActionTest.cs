using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US.DIS;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	[TestedType(typeof(MessageSendingAction))]
	sealed class MessageSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDocumentDescription()
		{
			var helper = new DISUniversalReferenceHelper(Factory);
			var codeEPA01 = helper.CreateDisCodeEntry("APH01", "APH_STAT");
			Factory.Save();
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.FormGroups).Returns(System.Array.Empty<ZString>());
			usDISHost.Setup(m => m.RequiredDocumentsProvider).Returns(((IDocsAndCartageParent)new TestHelper(Factory).GetJobDeclaration()).RequiredDocumentsProvider);
			var eDoc1PK = ZGuid.NewZGuid();
			var eDoc1 = new Mock<IeDoc>();
			eDoc1.Setup(m => m.UniqueKey).Returns(eDoc1PK);
			eDoc1.Setup(m => m.DocType).Returns(new ZString("DT1"));
			eDoc1.Setup(m => m.FileName).Returns(new ZString("Example.txt"));
			eDoc1.Setup(m => m.DateAdded).Returns(ZDateTime.BrettsBirthday);
			eDoc1.Setup(m => m.Description).Returns(new ZString("description1"));
			eDoc1.Setup(m => m.IsDeleted).Returns(ZBool.False);
			var eDoc2PK = ZGuid.NewZGuid();
			var eDoc2 = new Mock<IeDoc>();
			eDoc2.Setup(m => m.UniqueKey).Returns(eDoc2PK);
			eDoc2.Setup(m => m.DocType).Returns(new ZString("DT2"));
			eDoc2.Setup(m => m.FileName).Returns(new ZString("Example2.txt"));
			eDoc2.Setup(m => m.DateAdded).Returns(ZDateTime.BrettsBirthday.AddDays(1));
			eDoc2.Setup(m => m.Description).Returns(new ZString("description2"));
			eDoc2.Setup(m => m.IsDeleted).Returns(ZBool.False);
			usDISHost.Setup(m => m.EDocs).Returns(new IeDoc[] { eDoc1.Object, eDoc2.Object });
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			disDocument.DocumentLabel = "APH01";
			disDocument.EDocsDocumentPK = eDoc1PK;
			var action = new MessageSendingAction(disDocument);
			AssertEquals("APH_STAT" + "/Example.txt", action.DocumentDescription);
			usDISHost.VerifyAll();
			eDoc1.VerifyAll();
			eDoc2.VerifyAll();
		}

		public void TestMessageType()
		{
			var usDISHost = new Mock<IUSDISHost>();
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var action = new MessageSendingAction(disDocument);
			AssertEquals("Add", action.MessageType);
			disDocument.Status = StatusList.Codes.AOS;
			AssertEquals("Add", action.MessageType);
			disDocument.Status = StatusList.Codes.COS;
			AssertEquals("Replace", action.MessageType);
			action.SendWithdrawal = true;
			AssertEquals("Withdrawal", action.MessageType);
			usDISHost.VerifyAll();
		}

		public void TestIsWaitingForResponse()
		{
			var usDISHost = new Mock<IUSDISHost>();
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var action = new MessageSendingAction(disDocument);
			Assert(!action.IsWaitingForResponse);
			disDocument.Status = StatusList.Codes.AOS;
			Assert(action.IsWaitingForResponse);
			usDISHost.VerifyAll();
		}

		public void TestReadOnly()
		{
			var usDISHost = new Mock<IUSDISHost>();
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var action = new MessageSendingAction(disDocument);
			disDocument.Status = StatusList.Codes.AOS;
			Assert(action.SendWithdrawalInfo.ReadOnly);
			Assert(action.WithdrawalReasonInfo.ReadOnly);
			Assert(action.WithdrawalCommentInfo.ReadOnly);
			disDocument.Status = StatusList.Codes.COS;
			Assert(!action.SendWithdrawalInfo.ReadOnly);
			Assert(action.WithdrawalReasonInfo.ReadOnly);
			Assert(action.WithdrawalCommentInfo.ReadOnly);
			action.SendWithdrawal = true;
			Assert(!action.SendWithdrawalInfo.ReadOnly);
			Assert(!action.WithdrawalReasonInfo.ReadOnly);
			Assert(!action.WithdrawalCommentInfo.ReadOnly);
			usDISHost.VerifyAll();
		}

		public void TestClearWithdrawalDetails()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.MessageSendingWarning).Returns("");
			usDISHost.Setup(m => m.MessageSendingError).Returns("");
			usDISHost.Setup(m => m.Factory).Returns(Factory);
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			var action = new MessageSendingAction(disDocument);
			action.SendWithdrawal = true;
			action.WithdrawalReason = "ABC";
			action.WithdrawalComment = "Comment";
			action.Send = true;
			Assert(!action.SendWithdrawal);
			AssertEquals(ZString.Empty, action.WithdrawalReason);
			AssertEquals(ZString.Empty, action.WithdrawalComment);
			usDISHost.VerifyAll();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var jobDeclaration = new TestHelper(Factory).GetJobDeclaration();
			var hostWrapper = new DISHostWrapper((IUSDISHost)jobDeclaration);
			return new MessageSendingAction(new DISDocument(hostWrapper));
		}
	}
}
