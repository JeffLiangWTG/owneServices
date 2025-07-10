using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Registry.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class MDMSupportCertificateUserControl : RegistryZUserControl
	{
		public MDMSupportCertificateUserControl(MDMSupportCertificateRegistryDataType supportCertificateRegistryDataType)
		{
			InitializeComponent();

			targetClientIdBox.CaptionResourceString = supportCertificateRegistryDataType.ProductCode switch
			{
				MDMProductCodes.AVS => Enterprise.MasterFiles.GUI.Res.GetData("21978ca0-820f-4347-9c57-56efe839d998", "Target AVS ID"),
				MDMProductCodes.DPS => Enterprise.MasterFiles.GUI.Res.GetData("9870c687-62cb-4295-9184-3b4e41befb7c", "Target DPS ID"),
				_ => throw new ArgumentException("Invalid Product Code", nameof(supportCertificateRegistryDataType)),
			};
		}

		SystemToSystemTrustInfo MDMSupportCertificate => (SystemToSystemTrustInfo)DataSource;

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (MDMSupportCertificate != null)
			{
				UpdateCertificateText();
				UpdatePrivateKeyText();
			}
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			panel.SetReadOnly(readOnly);
		}

		protected virtual bool TryLoadCertificateFile(out byte[] certificateBytes)
		{
			using (var dialog = new ZOpenFileDialog())
			{
				dialog.Filter = (NoResString)"Certificate (*.cer)|*.cer";
				var loaded = dialog.ShowDialog(this) == DialogResult.OK;
				certificateBytes = loaded ? File.ReadAllBytes(dialog.ForceLocalFile()) : null;

				return loaded;
			}
		}

		protected virtual bool TryLoadPrivateKeyFile(out string privateKey)
		{
			using (var dialog = new ZOpenFileDialog())
			{
				dialog.Filter = (NoResString)"Private Key (*.key)|*.key";
				var loaded = dialog.ShowDialog(this) == DialogResult.OK;
				privateKey = loaded ? File.ReadAllText(dialog.ForceLocalFile()) : null;

				return loaded;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Visible Only to Support")]
		void UpdateCertificateText() => certificateBox.Text = MDMSupportCertificate.Certificate == ZBlob.Empty ? string.Empty : "Certificate Loaded.";

#pragma warning disable CS0618 // To be replaced with S2ST library once WI00771920 is implemented
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Visible Only to Support")]
		void UpdatePrivateKeyText() => privateKeyBox.Text = string.IsNullOrEmpty(MDMSupportCertificate.PrivateKey) ? string.Empty : "Private Key Loaded.";
#pragma warning restore CS0618 // To be replaced with S2ST library once WI00771920 is implemented

		void LoadCertificateButton_Click(object sender, EventArgs e)
		{
			if (TryLoadCertificateFile(out var certificateBytes))
			{
				MDMSupportCertificate.Certificate = certificateBytes;
				MDMSupportCertificate.HasChanges = true;
				UpdateCertificateText();
			}
		}

		void LoadPrivateKeyButton_Click(object sender, EventArgs e)
		{
			if (TryLoadPrivateKeyFile(out var privateKey))
			{
#pragma warning disable CS0618 // To be replaced with S2ST library once WI00771920 is implemented
				MDMSupportCertificate.PrivateKey = privateKey;
#pragma warning restore CS0618 // To be replaced with S2ST library once WI00771920 is implemented
				MDMSupportCertificate.HasChanges = true;
				UpdatePrivateKeyText();
			}
		}

		void TextBox_TextChanged(object sender, EventArgs e)
		{
			MDMSupportCertificate.HasChanges = true;
		}
	}
}
