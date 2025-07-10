using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class CargoIMPPhase2CreateMessageForm : ProgressForm, INotifications, INotificationSubscriberQueryUser
	{
		protected CargoIMPPhase2CreateMessageForm()
		{
			InitializeComponent();
			this.OutputTextbox.ReadOnly = true;
			ShowProgressBar = false;
			ShowCancelButton = false;
			Status = Res.GetString("ce46e06e-e8fb-4d57-a29b-d5b5c0bc94a4", "Do you wish to create CargoIMP Phase 2 message");
		}

		public CargoIMPPhase2CreateMessageForm(CargoIMPPhase2MessageManager messageManager, CargoIMPPhase2MessageTypes messageType)
			: this()
		{
			this.fMessageManager = messageManager;
			this.fMessageType = messageType;
		}

		readonly CargoIMPPhase2MessageManager fMessageManager;
		bool creationOK;
#if DEBUG
		internal
#endif
 readonly CargoIMPPhase2MessageTypes fMessageType;

		#region INotifications Members

		void INotifications.Add(INotification notification)
		{
			Notify(notification);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		public void Notify(INotification notification)
		{
			if (!string.IsNullOrEmpty(notification.Message) || notification is NewlineNotification)
			{
				OutputTextbox.AppendText(notification.Message + System.Environment.NewLine);
				Application.DoEvents();
			}
		}

		void INotificationSubscriberQueryUser.QueryUser(IQueryUserEventArgs e)
		{
			QueryUserCore(e);
		}

		protected virtual void QueryUserCore(IQueryUserEventArgs e)
		{
			new NotificationSubscriberGuiHelper().QueryUser(e);
		}

		#endregion

		public override string FormVerb
		{
			get { return ZString.Empty; }
		}

		void CreateButton_Click(object sender, System.EventArgs e)
		{
			CreateMessage();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		protected void CreateMessage()
		{
			SetAllButtonsEnabled(false);
			Status = Res.GetString("5c933218-1f15-416f-9a45-c02d6578d966", "Message creation in progress");
			creationOK = true;

			try
			{
				OutputTextbox.Text = Res.GetString("766d6a0d-9145-48c5-8748-80150f8dd3b2", "Creation of CargoIMP Phase 2 message - please be patient, this may take some time.") + "\r\n\r\n";
				Application.DoEvents();
				switch (fMessageType)
				{
					case CargoIMPPhase2MessageTypes.RouteMapInformation:
						creationOK = fMessageManager.CreateRouteMapInformationMessage(this);
						break;
					case CargoIMPPhase2MessageTypes.RouteMapCancellation:
						creationOK = fMessageManager.CreateRouteMapCancellationMessage(this);
						break;
				}
			}
			finally
			{
				SetAllButtonsEnabled(true);
			}

			if (creationOK)
			{
				OutputTextbox.AppendText(Res.GetString("351cf743-1d05-499a-990a-e42a4618f719", "Message creation complete."));
				Status = Res.GetString("f75f6162-a06e-4c95-814c-e120e85c2a1c", "Message creation complete");
			}
			else
			{
				OutputTextbox.AppendText(Res.GetString("4ac59e21-9e24-45cc-ba80-a467183a7025", "Message creation was unsuccessful."));
				Status = Res.GetString("c1e74c94-0f6c-4f31-882c-4a9b095ac059", "Message creation was unsuccessful");
			}
		}

		void SetAllButtonsEnabled(bool enabled)
		{
			CreateButton.Enabled = enabled;
			CloseButton.Text = enabled ? Res.GetString("Forwarding|CargoIMPPhase2CreateMessageForm|CloseButton", "Close") : Res.GetString("Forwarding|CargoIMPPhase2CreateMessageForm|CancelButton", "Cancel");
		}

		void CloseButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
