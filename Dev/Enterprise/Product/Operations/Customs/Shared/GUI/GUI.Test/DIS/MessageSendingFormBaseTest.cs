using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US.DIS;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.GUI.Testing
{
	public abstract class MessageSendingFormBaseTest<T1, T2> : ZFormBasherTest
		where T1 : NonPersistentBusinessObject, IMessageSendingActionBase
		where T2 : XmlSerializableNonPersistentBusinessObject, IDISDocumentBase
	{
		public void TestCancelButton()
		{
			var disDocument = HostWrapper.DISDocuments.AddNew();
			var coll = GetCollection();
			using (var form = GetForm(coll))
			{
				form.Show();
				form.CancelButton1.PerformClick();
				Assert(!form.ProceedWithSend);
			}
		}

		public void TestSendButton()
		{
			var disDocument = HostWrapper.DISDocuments.AddNew();
			var coll = GetCollection();
			using (var form = GetForm(coll))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				coll[0].Send = false;
				form.SendButton.PerformClick();
				AssertContains("Please indicate which of the Documents in the grid you want to submit to Customs.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendButtonWithNotifications()
		{
			var disDocument = HostWrapper.DISDocuments.AddNew();
			disDocument.Status = StatusList.Codes.AOS;
			var coll = GetCollection();
			using (var form = GetForm(coll))
			{
				form.Show();
				coll[0].Send = true;
				Assert("PreCondition", coll.HasNotifications());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.SendButton.PerformClick();
				AssertContains("There are notifications. Are you sure you wish to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(form.ProceedWithSend);
			}
		}

		public void TestSendButtonWithNotificationsAndUserSayNo()
		{
			var disDocument = HostWrapper.DISDocuments.AddNew();
			disDocument.Status = StatusList.Codes.AOS;
			var coll = GetCollection();
			using (var form = GetForm(coll))
			{
				form.Show();
				coll[0].Send = true;
				Assert("PreCondition", coll.HasNotifications());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.SendButton.PerformClick();
				AssertContains("There are notifications. Are you sure you wish to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!form.ProceedWithSend);
			}
		}

		public void TestSendButtonWithErrors()
		{
			var docManagerSupport = (IDocManagerSupport)JobDeclaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(JobDeclaration, "TST");
			var eDocs1 = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);
			var eDocs2 = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC×.pdf", "ABC", false);
			var disDocument1 = HostWrapper.DISDocuments.AddNew();
			disDocument1.Status = StatusList.Codes.AOS;
			disDocument1.EDocsDocumentPK = eDocs1.UniqueKey;
			var disDocument2 = HostWrapper.DISDocuments.AddNew();
			disDocument2.Status = StatusList.Codes.AOS;
			disDocument2.EDocsDocumentPK = eDocs2.UniqueKey;
			var coll = GetCollection();
			using (var form = GetForm(coll))
			{
				form.Show();
				coll[0].Send = true;
				coll[1].Send = true;
				Assert("PreCondition", coll[1].HasErrors);
				form.SendButton.PerformClick();
				AssertContains("There are errors. You are not able to send the message(s).", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!form.ProceedWithSend);
				coll[1].Send = false;
				Assert("PreCondition", !coll[1].HasErrors);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.SendButton.PerformClick();
				Assert(form.ProceedWithSend);
			}
		}

		protected abstract MessageSendingFormBase<T1, T2> GetForm(MessageSendingActionCollectionBase<T1, T2> collection);
		protected abstract MessageSendingActionCollectionBase<T1, T2> GetCollection();
		protected abstract DISHostWrapperBase<T2> HostWrapper { get; }
		protected abstract BusinessObject JobDeclaration { get; }
	}
}
