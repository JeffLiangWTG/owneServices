using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.DocumentSending;
using Enterprise.Customs.ZA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
	public sealed partial class VAT404DocumentSendingForm : ZChildForm
	{
		ZPanel zPanel1;
		ZButton backButton;
		ZButton searchButton;
		ZButton cancelButton;
		VAT404FilterUserControl vaT404FilterUserControl1;
		VAT404DocumentUserControl vaT404DocumentUserControl1;
		ZButton sendButton;
		CargoWise.Windows.UI.KSplitContainer mainSplitContainer;

		public VAT404DocumentSendingForm(VAT404DocumentInstruction declarationWrapper)
			: base(declarationWrapper)
		{
		}

		public override string FormHeading
		{
			get { return Res.GetString("336F34C4-A270-4BEE-841B-B24217F0317C", "Send VAT 404 Proof Of Payments to Importers"); }
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			SetControlVisibility(false);
		}

		#region Implementation

		void SetControlVisibility(bool hasFoundResult)
		{
			mainSplitContainer.Panel1Collapsed = hasFoundResult;
			mainSplitContainer.Panel2Collapsed = !hasFoundResult;
			searchButton.Visible = !hasFoundResult;
			backButton.Visible = hasFoundResult;
			sendButton.Visible = hasFoundResult;
		}

		#region Buttons Actions

		public new VAT404DocumentInstruction BusinessEntity => base.BusinessEntity as VAT404DocumentInstruction;

		void SendButton_Click(object sender, System.EventArgs e)
		{
			var validationResult = Customs.Business.MessageSendingValidation.New(this.BusinessEntity, null).CheckBusinessObjectLevelValidation();
			if (validationResult.ContainsError())
			{
				Globals.Message.ShowError(validationResult.ErrorNotificationsAsString(), "Please fix the errors before proceeding.");
			}
			else
			{
				IMessageNotificationCollector notification = new MessageNotificationCollector();
				if (BusinessEntity.RunPreDeliverCheck(notification))
				{
					BusinessEntity.Deliver(notification);
				}

				if (AggregateSendingResultAndNotify(notification))
				{
					this.Close();
				}
			}
		}

		bool AggregateSendingResultAndNotify(IMessageNotificationCollector notification)
		{
			var shouldCloseAfterSend = true;
			var notifications = notification?.Notifications;
			if (notifications != null)
			{
				var stringBuilder = new ZStringBuilder();

				if (notifications.ContainsInformation())
				{
					stringBuilder.Append("-- Successful --");
					stringBuilder.Append(notifications.InformationNotificationsAsString());
				}
				if (notifications.ContainsError())
				{
					stringBuilder.Append("-- Failed --");
					stringBuilder.Append(notifications.ErrorNotificationsAsString());
					shouldCloseAfterSend = false;
				}
				notification.ShowInformation(stringBuilder.ToStringWithNewLineBetweenAppends(), Res.GetString("415888E9-077D-4BCA-8285-F5B568C7F509", "Delivery Result"));
			}

			return shouldCloseAfterSend;
		}

		void CancelButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		void SearchButton_Click(object sender, System.EventArgs e)
		{
			var validationResult = Customs.Business.MessageSendingValidation.New(this.BusinessEntity, null).CheckBusinessObjectLevelValidation();
			if (validationResult.ContainsError())
			{
				Globals.Message.ShowError(validationResult.ErrorNotificationsAsString(), "Please fix the errors before proceeding.");
			}
			else
			{
				BusinessEntity.PerformSearch();
				if (BusinessEntity.VAT404Documents.Count == 0)
				{
					Globals.Message.ShowError(Res.GetString("91E82DA8-187C-4AFE-84B8-08012598EC22", "No Proof of Payments has been found with the searching criteria."), "No Proof of Payments Found");
				}
				else
				{
					SetControlVisibility(true);
				}
			}
		}

		void BackButton_Click(object sender, System.EventArgs e)
		{
			BusinessEntity.VAT404Documents.RemoveAll();
			SetControlVisibility(false);
		}

		#endregion

		#endregion
	}
}

