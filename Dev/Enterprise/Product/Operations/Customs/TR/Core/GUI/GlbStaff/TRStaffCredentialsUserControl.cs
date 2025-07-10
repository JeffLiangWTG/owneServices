using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI.Certificates;
using Enterprise.Customs.TR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI.GlbStaff
{
	public partial class TRStaffCredentialsUserControl : StaffCredentialsUserControl
	{
		public TRStaffCredentialsUserControl()
		{
			InitializeComponent();
		}

		TRGlbStaffWrapper StaffWrapper => (TRGlbStaffWrapper)CurrentDataItem;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			StatusReasonTextBox.Visible = PasswordStatusList.Codes.Invalid == StaffWrapper.TRBPassword?.GP_PasswordStatus.ToString();
		}

		void TRCustomsCertificateDefiningLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			const string trCustomsCertificateDefiningLinkURL = "https://uygulama.gtb.gov.tr/BilgeSifreIslemleri";
			WebUrlLauncher.Launch(trCustomsCertificateDefiningLinkURL);
		}

		void ChooseButton_Click(object sender, EventArgs e)
		{
			var chipset = StaffWrapper.TRBPassword.TR_Chipset;

			if (chipset.IsEmpty)
			{
				Globals.Message.ShowError(MessageSpecifyChipset);
				return;
			}

			var chosenCertificate = ChooseCertificate(chipset);

			if (chosenCertificate != null)
			{
				StaffWrapper.TRBPassword.GP_CertificateSerialNumber = chosenCertificate.SerialNumber;
			}
		}

		CryptokiCertificate ChooseCertificate(string chipset)
		{
			var selector = (chipset == ChipsetList.Codes.WINDOWS)
				? WindowsCertificateSelector.New(chipset)
				: TokenCertificateSelector.New(chipset);

			try
			{
				return selector.ChooseCertificate();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError(ex.Message);
				return null;
			}
		}

		void CertificateInfButton_Click(object sender, EventArgs e)
		{
			var chipset = StaffWrapper.TRBPassword.TR_Chipset;
			var serialNumberText = StaffWrapper.TRBPassword.GP_CertificateSerialNumber;

			if (chipset.IsEmpty)
			{
				Globals.Message.ShowError(MessageSpecifyChipset);
				return;
			}

			ShowCertificateInfo(chipset, serialNumberText);
		}

		void ShowCertificateInfo(string chipset, string serialNumberText)
		{
			var selector = (chipset == ChipsetList.Codes.WINDOWS)
				? WindowsCertificateSelector.New(chipset)
				: TokenCertificateSelector.New(chipset);

			try
			{
				selector.ShowCertificateInfo(serialNumberText);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError(ex.Message);
				return;
			}
		}

		static string MessageSpecifyChipset => Res.GetString("D5F421AA-6B6A-4B3C-A1EF-3B678DD7678B", "You must specify chipset.");
	}
}
