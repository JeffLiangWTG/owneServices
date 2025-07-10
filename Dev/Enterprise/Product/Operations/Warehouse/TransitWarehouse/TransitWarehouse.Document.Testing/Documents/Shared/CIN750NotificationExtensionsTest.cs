using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Document.Testing
{
	public abstract class CIN750NotificationExtensionsTest<TExtensions, TNotification> : TestCaseWithFactory where TExtensions : CIN750NotificationExtensions<TNotification> where TNotification : CIN750Notification
	{
		public void TestGeneralInterfaces()
		{
			var notifications = new Mock<IUserNotifications>();
			var extensions = GetExtensions();
			Assert(extensions.ContinueWithSendingMessage(notifications.Object).Value);
			Assert(!extensions.ContinueWithSendingMessageAmendment(notifications.Object).Value);
			Assert(!extensions.ContinueWithSendingMessageWithdrawal(notifications.Object).Value);
			Assert(!extensions.ContinueWithResetToOriginal(notifications.Object).Value);
			Assert(!extensions.IsSendingAmendment().Value);
		}

		[TestDate(2024, 4, 29, 14, 34, 0)]
		public void TestCINMessageNote_Succeed()
		{
			var notifications = new Mock<IUserNotifications>();
			var extensions = GetExtensions();
			Assert(extensions.ContinueWithSendingMessage(notifications.Object).Value);
			AssertEquals(ExpectedCINMessageNote_Succeed, ActualCINMessageNote);
		}

		[TestDate(2024, 4, 29, 14, 34, 0)]
		public void TestCINMessageNote_Failed()
		{
			var notifications = new Mock<IUserNotifications>();
			var extensions = GetExtensions(succeed: false);
			AssertEquals(false, extensions.ContinueWithSendingMessage(notifications.Object).Value);
			AssertEquals(ExpectedCINMessageNote_Failed, ActualCINMessageNote);
		}

		protected abstract TExtensions GetExtensions(bool succeed = true);

		protected virtual ZString ExpectedCINMessageNote_Succeed { get; }
		protected virtual ZString ExpectedCINMessageNote_Failed { get; }

		#region Implement

		protected WhsTransitTestHelper Helper => helper ?? (helper = new WhsTransitTestHelper(Factory));
		WhsTransitTestHelper helper;

		ZString ActualCINMessageNote => Consignment.Notes.FindByDescription(PredefinedNoteTypes.Instance.CIN750MessageNotes.Description).FirstOrDefault()?.ST_NoteText ?? ZString.Empty;
		protected virtual IStmNoteParent Consignment { get; }

		#endregion
	}
}
