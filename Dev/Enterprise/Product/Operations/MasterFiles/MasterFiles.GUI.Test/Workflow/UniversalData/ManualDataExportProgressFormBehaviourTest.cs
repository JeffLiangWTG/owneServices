using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestsSubclassesOf(typeof(ManualDataExportProgressFormBehaviour), ExcludePrivate = true)]
	public abstract class ManualDataExportProgressFormBehaviourTest<T> : TestCaseWithFactory
			where T : ManualDataExportProgressFormBehaviour
	{
		public abstract void TestApply();

		[RequiresSTA]
		public void TestNotifications()
		{
			using (var form = new DummyManualDataExportProgressForm())
			{
				var behaviour = GetNewBehaviour();
				behaviour.Apply(form);

				var notificationsConsumer = (INotifications)behaviour;
				var notification = new Mock<INotification>();
				var notificationType = new Mock<INotificationType>();

				notification.Setup(m => m.Message).Returns("some error has occurred");
				notification.Setup(m => m.Type).Returns(notificationType.Object);

				notificationsConsumer.Add(notification.Object);

				notificationType.Verify(nt => nt.IsFatal, Times.AtLeastOnce());
				notification.Verify(n => n.Message, Times.AtLeastOnce);

				AssertMultilineASCIIEquals("notifications",
@"some error has occurred
", form.NotificationsTextBox.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestNotificationsWhenBehaviourHasNotBeenApplied()
		{
			var behaviour = GetNewBehaviour();

			var notificationsConsumer = (INotifications)behaviour;
			var notification = new Mock<INotification>();
			var notificationType = new Mock<INotificationType>();

			notification.Setup(m => m.Type).Returns(notificationType.Object);

			notificationsConsumer.Add(notification.Object);

			notificationType.Verify(m => m.IsFatal, Times.Never);
			notification.Verify(n => n.Message, Times.Never);
		}

		protected abstract T GetNewBehaviour();

		protected override void SetUp()
		{
			AssertNotNull(DummyWorkflowDescriptor.Instance);
			base.SetUp();
		}
	}
}
