using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class BoleroInvitationForm : ZChildForm
	{
		public BoleroInvitationForm(BoleroInvitationDetails invitationDetails) : base(invitationDetails)
		{
			InvitationDetails = invitationDetails;
			this.Text = Res.GetString("8B688DC5-9609-434F-BD62-B4E14A0CE46F", "Invite {0} to enroll for Electronic Bills of Lading", InvitationDetails.Org.OH_FullName);
		}

		public BoleroInvitationDetails InvitationDetails { get; }

		public OnboardingRequestDTO OnboardingRequestDTO { get; private set; }

		void SendInviteButton_Click(object sender, EventArgs e)
		{
			InvitationDetails.Validation.ValidateAll();
			if (InvitationDetails.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				OnboardingRequestDTO = new OnboardingRequestDTO(InvitationDetails);

				if (!BoleroDTOValidator.ValidateBoleroDTOFields(OnboardingRequestDTO, out var errorMessages))
				{
					Globals.Message.ShowError(string.Join(System.Environment.NewLine, errorMessages));
					return;
				}

				Globals.Message.Show(Res.GetString("3644D1FE-2453-4E47-8437-3841EFB4CF6B", "Enrollment request sent."));

				this.DialogResult = DialogResult.OK;
				this.Close();
			}
		}

		protected override ZMessageBox CreateErrorMessageBox(IBusiness businessEntityForValidation, bool includeIgnoreOption)
		{
			return new ZErrorMessageBox(businessEntityForValidation, Res.GetString("7BE18039-8814-429A-88B1-052B0DC731AF", "There are errors that need to be corrected before the invite can be sent."), Res.GetString("490F3E0A-712E-4F7F-8C5C-E526D8A7E20C", "Unable to Send invite"), includeIgnoreOption);
		}
	}
}
