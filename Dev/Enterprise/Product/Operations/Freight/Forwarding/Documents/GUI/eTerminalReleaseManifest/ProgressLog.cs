using System.Collections.Generic;
using System.Drawing;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;
using Enterprise.ZArchitecture.GUI;
using NotificationType = CargoWise.EntityFramework.NotificationType;

namespace Enterprise.Freight.Forwarding.Documents.GUI
{
	public sealed partial class ProgressLog : ZUserControl, IProgressNotifications
	{
		public ProgressLog()
		{
			hyperlinkActions = new HyperlinkActionCollection();

			InitializeComponent();
			logTextBox.LinkClicked += RichTextActionManager.CreateClickHandler(hyperlinkActions, true);
		}

		readonly HyperlinkActionCollection hyperlinkActions;

		#region IProgressNotifications

		public void SetProgressMax(int max)
		{
			this.BeginInvokeSafe(() =>
			{
				Clear();
				progressBar.Value = 0;
				progressBar.Maximum = max;
			});
		}

		public void BumpProgress()
		{
			this.BeginInvokeSafe(() =>
			{
				progressBar.Value++;
			});
		}

		public void Add(INotification notification)
		{
			Notify(notification.Type, notification.Message);
		}

		public void NotifyFormat(INotificationType notificationType, string format, params object[] args)
		{
			Notifications.Add(new Notification(notificationType, string.Format(format, args)));

			var builder = new RtfStringBuilder(hyperlinkActions);
			builder.SetForgroundColour(ColourForErrorLevel(notificationType));
			builder.AppendFormat(format, args);
			builder.AppendNewLine();
			builder.SetForgroundColour(null);
			this.BeginInvokeSafe(() =>
			{
				RichTextActionManager.AppendBuilderContent(logTextBox, builder);
			});
		}

		public void Notify(INotificationType notificationType, string message)
		{
			Notifications.Add(new Notification(notificationType, message));

			var builder = new RtfStringBuilder(hyperlinkActions);
			builder.SetForgroundColour(ColourForErrorLevel(notificationType));
			builder.Append(message);
			builder.AppendNewLine();
			builder.SetForgroundColour(null);
			this.BeginInvokeSafe(() =>
			{
				RichTextActionManager.AppendBuilderContent(logTextBox, builder);
			});
		}

		public ICollection<INotification> Notifications
		{
			get { return notifications ?? (notifications = new List<INotification>()); }
		}

		List<INotification> notifications;

		#endregion

		void Clear()
		{
			logTextBox.Clear();
			hyperlinkActions.Clear();
		}

		Color? ColourForErrorLevel(INotificationType notificationType)
		{
			if (Equals(notificationType, NotificationType.Information))
			{
				return Color.DarkViolet;
			}

			if (Equals(notificationType, NotificationType.Warning))
			{
				return Color.DarkOrange;
			}

			if (Equals(notificationType, NotificationType.Error))
			{
				return Color.DarkRed;
			}

			if (Equals(notificationType, NotificationType.MessageError))
			{
				return Color.DarkBlue;
			}

			return null;
		}
	}
}
