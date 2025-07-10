using System;
using System.Drawing;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CommunicationDetailsUserControl : ZUserControl
	{
		public CommunicationDetailsUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				CategoryDropEdit.GetExtension<LabelCaptionRenderer>().Caption = OrganisationsDataRegistry.Instance.CategoryListLabel.Value;

				OrgSalesCall communication = CurrentDataItem as OrgSalesCall;
				SetupOverallDispositionLabel(communication);
				SetupInvitationButtons(communication);
				SetupLinkedInquiryControls();
			}
		}

		#region ReadOnly

		bool buttonsReadOnly;
		public void SetButtonsReadOnly()
		{
			buttonsReadOnly = true;
			SendInvitationButton.ReadOnly = buttonsReadOnly;
		}

		#endregion

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			var communication = CurrentDataItem as OrgSalesCall;
			if (communication != null)
			{
				UnhookLinkedInquiryEvents(communication);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			SetOverallDispositionLabelBackColor();
			UpdateInvitationButtons();
			UpdateLinkedInquiryControls();

			var communication = CurrentDataItem as OrgSalesCall;
			if (communication != null)
			{
				HookLinkedInquiryEvents(communication);
			}
		}

		#region OverallDispositionLabel

		void SetupOverallDispositionLabel(OrgSalesCall communication)
		{
			if (communication != null)
			{
				communication.OQ_StatusInfo.ValueChanged += OQ_StatusInfo_ValueChanged;
			}
		}

		void OQ_StatusInfo_ValueChanged(object sender, EventArgs e)
		{
			SetOverallDispositionLabelBackColor();
		}

		void SetOverallDispositionLabelBackColor()
		{
			OrgSalesCall communication = CurrentDataItem as OrgSalesCall;
			if (communication != null)
			{
				OverallDispositionLabel.BackColor = communication.IsClosed ? Color.Red : Color.LimeGreen;
			}
		}

		#endregion

		#region InvitationButtons

		void SetupInvitationButtons(OrgSalesCall communication)
		{
			if (OrganisationsDataRegistry.Instance.CommunicationAutoSendUponSave.Value)
			{
				SendInvitationButton.ReadOnly = true;
				SendInvitationButton.Text = Res.GetString("b47b85d5-4695-474a-af6d-935ef94054a8", "Auto Send");
			}
			else if (communication != null)
			{
				communication.OQ_NextCallInfo.ValueChanged += OQ_NextCallInfo_ValueChanged;
			}

			if (communication != null)
			{
				communication.LastInvitationActionDescriptionInfo.ValueChanged += LastInvitationActionDescriptionInfo_ValueChanged;
			}

			RegistryCalendarIntegrationLabel.Text = Res.GetString("be74fbca-9973-4847-8498-7442c4fe5fe6", "To send an invitation, enable the Registry setting: {0}", (Env.Registry.RawRegistry.CalendarIntegration as IMultilingualRegistryItem).LocationMultilingual);
		}

		void OQ_NextCallInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateInvitationButtons();
		}

		void LastInvitationActionDescriptionInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdateInvitationButtons();
		}

		void UpdateInvitationButtons()
		{
			OrgSalesCall communication = CurrentDataItem as OrgSalesCall;
			if (communication != null)
			{
				CancelInvitationButton.ReadOnly = buttonsReadOnly || !communication.IsInvitationSent;
				if (!OrganisationsDataRegistry.Instance.CommunicationAutoSendUponSave.Value)
				{
					SendInvitationButton.ReadOnly = buttonsReadOnly || communication.OQ_NextCall.IsEmpty;
					SendInvitationButton.Text = communication.IsInvitationSent ? Res.GetString("bee00815-bea0-4604-ad45-9aef7bdfad87", "Resend") : Res.GetString("6cd01189-d9e6-4a66-b148-15df48eca99d", "Send");
				}
			}
		}

		void CheckRegistryCalendarIntegration()
		{
			RegistryCalendarIntegrationLabel.Visible = !Env.Registry.CalendarIntegration;
		}

		protected void SendInvitationButton_Click(object sender, EventArgs e)
		{
			OrgSalesCall communication = CurrentDataItem as OrgSalesCall;
			if (communication != null)
			{
				var sendReminderResult = communication.SendCalendarReminder();
				if (sendReminderResult.Sent)
				{
					UpdateInvitationButtons();
					CheckRegistryCalendarIntegration();
				}
				else
				{
					Globals.Message.ShowError(sendReminderResult.Message);
				}
			}
		}

		void CancelInvitationButton_Click(object sender, EventArgs e)
		{
			OrgSalesCall communication = CurrentDataItem as OrgSalesCall;
			if (communication != null)
			{
				var sendReminderResult = communication.CancelCalendarReminder();
				if (sendReminderResult.Sent)
				{
					UpdateInvitationButtons();
				}
				else
				{
					Globals.Message.ShowError(sendReminderResult.Message);
				}
			}
		}

		#endregion

		#region Linked Inquiry Controls

		void SetupLinkedInquiryControls()
		{
			UpdateLinkedInquiryControls();
		}

		void UpdateLinkedInquiryControls()
		{
			var communication = CurrentDataItem as OrgSalesCall;
			var isLinkedToInquiry = communication?.IsLinkedToInquiry ?? false;

			ClientGuidFindBox.Visible = !isLinkedToInquiry;
			ContactGuidDropEdit.Visible = !isLinkedToInquiry;

			ClientTextBox.Visible = isLinkedToInquiry;
			ContactTextBox.Visible = isLinkedToInquiry;
		}

		void HookLinkedInquiryEvents(OrgSalesCall communication)
		{
			communication.LinkedInquiryChanged += communication_LinkedInquiryChanged;
		}

		void UnhookLinkedInquiryEvents(OrgSalesCall communication)
		{
			communication.LinkedInquiryChanged -= communication_LinkedInquiryChanged;
		}

		void communication_LinkedInquiryChanged(object sender, EventArgs e)
		{
			UpdateLinkedInquiryControls();
		}

		#endregion
	}
}
