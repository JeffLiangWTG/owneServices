using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US.DIS;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Moq;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class MessageSendingActionValidationTest : TestCaseWithFactory
	{
		public void TestCheckSend()
		{
			var usDISHost = new Mock<IUSDISHost>();
			usDISHost.Setup(m => m.MessageSendingWarning).Returns("Warning");
			usDISHost.Setup(m => m.MessageSendingError).Returns("Error");
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			disDocument.Status = StatusList.Codes.AOS;
			var action = new MessageSendingAction(disDocument);
			action.Send = true;
			AssertHasMessageError(action.SendInfo, MessageSendingActionValidation.PendingResponses);
			AssertHasWarning(action.SendInfo, "Warning");
			AssertHasMessageError(action.SendInfo, "Error");
			action.Send = false;
			AssertNoMessageError(action.SendInfo, MessageSendingActionValidation.PendingResponses);
			AssertNoWarning(action.SendInfo, "Warning");
			AssertNoMessageError(action.SendInfo, "Error");
			usDISHost.VerifyAll();
		}

		public void TestCheckSendWithdrawal()
		{
			var usDISHost = new Mock<IUSDISHost>();
			var hostWrapper = new DISHostWrapper(usDISHost.Object);
			var disDocument = new DISDocument(hostWrapper);
			disDocument.Status = StatusList.Codes.ARS;
			var action = new MessageSendingAction(disDocument);
			action.SendWithdrawal = true;
			AssertHasMessageError(action.SendWithdrawalInfo, MessageSendingActionValidation.PendingResponses);
			action.SendWithdrawal = false;
			AssertNoMessageError(action.SendWithdrawalInfo, MessageSendingActionValidation.PendingResponses);
			usDISHost.VerifyAll();
		}

		public void TestCheckSendWithInvalidFileName()
		{
			var jobDeclaration = new TestHelper(Factory).GetJobDeclaration();
			var declaration = (IUSDISHost)jobDeclaration;
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(jobDeclaration, "TST");
			var eDocs1 = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);
			var eDocs2 = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC×.pdf", "ABC", false);
			var hostWrapper = new DISHostWrapper(declaration);
			var disDocument = hostWrapper.DISDocuments.AddNew();
			disDocument.EDocsDocumentPK = eDocs1.UniqueKey;
			var action = new MessageSendingAction(disDocument);
			action.Send = true;
			AssertNoWarning(action.SendInfo, DISDocumentValidation.FileNameHasInvalidCharacters);
			disDocument.EDocsDocumentPK = eDocs2.UniqueKey;
			action = new MessageSendingAction(disDocument);
			action.Send = true;
			AssertHasWarningContaining(action.SendInfo, DISDocumentValidation.FileNameHasInvalidCharacters);
			action.Send = false;
			AssertNoWarningContaining(action.SendInfo, DISDocumentValidation.FileNameHasInvalidCharacters);
		}
	}
}
