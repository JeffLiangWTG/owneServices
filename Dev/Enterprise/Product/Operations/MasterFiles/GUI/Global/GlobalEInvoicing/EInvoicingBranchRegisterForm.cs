using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.GlobalEInvoicingRegistration.SaudiArabia;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.MasterFiles.GUI.DigitalCertificateControl_p12_EInvoicing;

namespace Enterprise.MasterFiles.GUI
{
	public partial class EInvoicingBranchRegisterForm : ZChildForm
	{
		public EInvoicingBranchRegisterForm(EInvoicingBranchRegister branchRegister, DigitalCertificateControl_p12_EInvoicing certificateLoaderUserControl)
			: base(branchRegister)
		{
			register = branchRegister;
			certificateControl = certificateLoaderUserControl;

			progressLogTextBox.AllowOverlap(Cancel_Button);
			Cancel_Button.AllowOverlap(OKButton);
		}

		internal void OKButton_Click(object sender, EventArgs e)
		{
			if (!OKButton.Enabled)
			{
				return;
			}

			register.Validation.ValidateAll();
			if (register.OTPInfo.HasErrors())
			{
				Globals.Message.ShowError(register.OTPInfo.GetErrors().GetFirstMessage());
				return;
			}
			if (register.DebtorPKInfo.HasErrors())
			{
				Globals.Message.ShowError(register.DebtorPKInfo.GetErrors().GetFirstMessage());
				return;
			}

			using (new ZWaitCursorChanger())
			{
				OKButton.Enabled = false;
				var registrationRequestor = register.GetCountrySpecificRegistrationRequestor();
				if (registrationRequestor != null)
				{
					using (new DisposableAction(() => HookEvents(), () => UnhookEvents()))
					{
						registrationRequestor.Register();
					}
				}

				void HookEvents()
				{
					registrationRequestor.RegistrationSuccessful += RegistrationRequestor_RegistrationSuccessful;
					registrationRequestor.RegistrationFailed += RegistrationRequestor_RegistrationFailed;
					register.NewItemAddedToLog += Register_NewItemAddedToLog;
				}

				void UnhookEvents()
				{
					registrationRequestor.RegistrationSuccessful -= RegistrationRequestor_RegistrationSuccessful;
					registrationRequestor.RegistrationFailed -= RegistrationRequestor_RegistrationFailed;
					register.NewItemAddedToLog -= Register_NewItemAddedToLog;
				}
			}
		}

		void RegistrationRequestor_RegistrationSuccessful(object sender, EventArgs e)
		{
			if (e is SARegistrationEventArgs sae && certificateControl != null)
			{
				//Save certificate data to branch credential grid
				certificateControl.FileDataAsString = sae.BinarySecurityToken;
				certificateControl.OnDataRegistered(this, new DataRegistrationEventArgs(sae.BinarySecurityToken, sae.Secret, sae.RequestId));
				(register as ILogger).Log(LogType.Information, "\r\nAdded to Branch record\r\n");
			}
		}

		void RegistrationRequestor_RegistrationFailed(object sender, EventArgs e) => OKButton.Enabled = true;

		void Register_NewItemAddedToLog(object sender, EventArgs e)
		{
			this.progressLogTextBox.SelectionStart = this.progressLogTextBox.Text.Length;
			this.progressLogTextBox.ScrollToCaret();
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
