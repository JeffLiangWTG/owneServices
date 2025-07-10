using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI
{
	public partial class ESignatureForm : ZChildForm
	{
		public ESignatureForm(MessageSendAcknowledgeAndSign signIn) : base(signIn)
		{
			InitializeComponent();

			acknowledgeAndSignData = signIn;
			this.SignButton.Enabled = !signIn.PINCode.IsEmpty;
			UpdateSignButtonLabelAndPinCodeTextBoxVisibility();
			SetNationalXMLVisibility();
		}

		readonly MessageSendAcknowledgeAndSign acknowledgeAndSignData;

		void SetNationalXMLVisibility()
		{
			if (!IsNCTSPhase5Message)
			{
				MessageContextNationGroupBox.Visible = false;
				MessageContentTableLayoutPanel.SetColumnSpan(MessageContextGroupBox, 2);
			}
		}

		void SignButton_Click(object sender, System.EventArgs e)
		{
			var currentUserInfo = acknowledgeAndSignData.CurrentUserExternalPasswordInfo;
			if (currentUserInfo == null)
			{
				Globals.Message.Show(Res.GetString("7C3BC35F-D93F-4FA7-B8B8-0178F7F9ADEA", "Please edit your staff record to add the broker in the Brokerage tab."));
			}
			else if (currentUserInfo.TR_Chipset.IsEmpty)
			{
				Globals.Message.Show(Res.GetString("00FF013E-2F75-409B-8EE5-710CEA1FE238", "Please edit your staff record to add the broker in the Brokerage tab. Make sure fill Chip-set."));
			}
			else if (!TRMessageSigner.IsValidCertificateSerialNumber(currentUserInfo.GP_CertificateSerialNumber))
			{
				Globals.Message.Show(Res.GetString("860738AE-3AEB-49FA-82F4-882031C64CF7", "Invalid certificate serial number: '{0}'.", currentUserInfo.GP_CertificateSerialNumber));
			}
			else
			{
				Sign();
				Close();
			}
		}

		void CancelButton_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			Close();
		}

		void Sign()
		{
			this.DialogResult = DialogResult.OK;
			acknowledgeAndSignData.CurrentUserExternalPasswordInfo.PINCode = this.PinCodeTextBox.Text;
		}

		void PinCodeTextBox_TextChanged(object sender, System.EventArgs e)
		{
			this.SignButton.Enabled = this.PinCodeTextBox.Text.Length > 3;
		}

		void UpdateSignButtonLabelAndPinCodeTextBoxVisibility()
		{
			if (IsExportUnionMessage || (IsNctsMessage && TRCustomsDataRegistry.Instance.IsSendTRNCTSMessageWithoutSign))
			{
				this.PinCodeTextBox.Enabled = false;
				this.SignButton.Enabled = true;
				this.SignButton.Text = Res.GetString("C43BA488-3B5F-48FF-A885-B1D3DD1F6D8C", "Send");
			} 
		}

		public ZBool IsNctsMessage => acknowledgeAndSignData.MessageType == TRMessageTypes.Codes.TRN || acknowledgeAndSignData.MessageType == TRMessageTypes.Codes.T1N || acknowledgeAndSignData.MessageType == TRMessageTypes.Codes.T2N || acknowledgeAndSignData.MessageType == TRMessageTypes.Codes.TR5;

		ZBool IsNCTSPhase5Message => acknowledgeAndSignData.MessageType == TRMessageTypes.Codes.TR5;

		public ZBool IsExportUnionMessage => acknowledgeAndSignData.MessageType == TRMessageTypes.Codes.EUT;
	}
}

