using System;
using System.Drawing;
using System.Windows.Forms;

namespace CargoWise.eAdaptorSampleWebClient
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
		}

		private void btMessageFileBrowse_Click(object sender, EventArgs e)
		{
			if (MessageFileDialog.ShowDialog() != DialogResult.OK) return;
			FilePath.Text = MessageFileDialog.FileName;
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
//			MessageFileDialog.InitialDirectory = Path.Combine(Application.StartupPath, @"");
		}

		private void PingCommand_Click(object sender, EventArgs e)
		{
			if (!ValidateSenderId()) return;
			if (!ValidatePassword()) return;
			ValidateServiceAddress();
			AddNotification(new Notification("", Notification.MessageType.Information));
		}

		private void SendMessage_Click(object sender, EventArgs e)
		{
			if (!ValidateMessageFileName()) return;
			if (!ValidateRecipientId()) return;
			if (!ValidateSenderId()) return;
			if (!ValidatePassword()) return;
			if (!ValidateServiceAddress(true)) return;

			AddNotification(new Notification(String.Format("Send Message {0} to {1}", MessageFileDialog.FileName.Trim(), RecipientId.Text.Trim()), Notification.MessageType.Information));

			try
			{
				eAdaptorSampleWebClient.SendMessage(
					ServiceAddress.Text.Trim(),
					MessageFileDialog.FileName.Trim(),
					RecipientId.Text.Trim(),
					SenderId.Text.Trim(),
					Password.Text);

					AddNotification(new Notification("Send message successful.", Notification.MessageType.Confirmation));
			}
			catch (Exception ex)
			{
				AddNotification(new Notification(String.Format("Error during message sending: {0}", ex.Message), Notification.MessageType.Error));
			}

			AddNotification(new Notification("", Notification.MessageType.Information));
		}

		private bool ValidateMessageFileName()
		{
			if(String.IsNullOrEmpty(MessageFileDialog.FileName.Trim()))
			{
				AddNotification(new Notification("Please select Message FileName", Notification.MessageType.Warning));
				return false;
			}
			return true;
		}

		private bool ValidateRecipientId()
		{
			var code = RecipientId.Text.Trim();
			if (String.IsNullOrEmpty(code))
			{
				AddNotification(new Notification("Please specify Recipient Id", Notification.MessageType.Warning));
				return false;
			}

			if (code.Length != 9)
			{
				AddNotification(new Notification("Recipient Id should be 9 character code", Notification.MessageType.Warning));
				return false;
			}

			return true;
		}

		bool ValidateSenderId()
		{
			var code = SenderId.Text.Trim();
			if (String.IsNullOrEmpty(code))
			{
				AddNotification(new Notification("Please specify Sender Id", Notification.MessageType.Warning));
				return false;
			}

			return true;
		}

		bool ValidatePassword()
		{
			return true;
		}

		bool ValidateServiceAddress(bool SuppressSuccessMessages = false)
		{
			var serviceAddress = ServiceAddress.Text.Trim();
			if (String.IsNullOrEmpty(serviceAddress))
			{
				AddNotification(new Notification("Please specify ServiceAddress", Notification.MessageType.Warning));
				return false;
			}
			if (!SuppressSuccessMessages)  AddNotification(new Notification(String.Format("Ping {0}", serviceAddress), Notification.MessageType.Information));

			try
			{
				if (eAdaptorSampleWebClient.Ping(serviceAddress, SenderId.Text.Trim(), Password.Text))
				{
					if(!SuppressSuccessMessages) AddNotification(new Notification("Ping successful.", Notification.MessageType.Confirmation));
					return true;
				}
				else
				{
					AddNotification(new Notification("Ping error. Use another address or check Service Is Available", Notification.MessageType.Error));
					return false;
				}
			}
			catch (Exception ex)
			{
				AddNotification(new Notification(String.Format("Ping error: {0}", ex.Message), Notification.MessageType.Error));
				return false;
			}
		}

		public void AddNotification(Notification notification)
		{
			NotificationList.Items.Add(notification);
		}

		void NotificationList_DrawItem(object sender, DrawItemEventArgs e)
		{
			if (e.Index == -1) return;

			var notification = NotificationList.Items[e.Index] as Notification;
			if (notification == null) return;

			var color = Color.DarkGray;

			switch (notification.NotificationType)
			{
				case Notification.MessageType.Error:
					color = Color.DarkRed;
					break;
				case Notification.MessageType.Warning:
					color = Color.DarkOrange;
					break;
				case Notification.MessageType.Confirmation:
					color = Color.DarkGreen;
					break;
			}

			e.Graphics.DrawString(NotificationList.Items[e.Index].ToString(), new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold), new SolidBrush(color), e.Bounds);
		}
	}
}
