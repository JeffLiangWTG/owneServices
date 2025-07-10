using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.GUI
{
	public abstract class ManualDataExportProgressFormBehaviour : INotifications, INotificationSubscriberQueryUser
	{
		public void Apply(IManualDataExportProgressForm progressForm)
		{
			this.progressForm = Argument.NotNull(progressForm, "progressForm");
			ApplyCore(this.progressForm);
		}

		IManualDataExportProgressForm progressForm;

		protected abstract void ApplyCore(IManualDataExportProgressForm progressForm);

		protected bool HasNotificationErrors { get; private set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void INotifications.Add(INotification notification)
		{
			if (progressForm != null && notification != null && (!string.IsNullOrWhiteSpace(notification.Message) || notification is NewlineNotification))
			{
				if (notification.Type != null && notification.Type.IsFatal)
				{
					HasNotificationErrors = true;
				}

				progressForm.NotificationsTextBox.AppendText(string.Concat(notification.Message, System.Environment.NewLine));
				Application.DoEvents();
			}
		}

		#region INotificationSubscriberQueryUser Members

		public void QueryUser(IQueryUserEventArgs e)
		{
			new NotificationSubscriberGuiHelper().QueryUser(e);
		}

		#endregion
	}
}
