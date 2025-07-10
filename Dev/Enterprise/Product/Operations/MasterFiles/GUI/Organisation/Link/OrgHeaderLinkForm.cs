using System;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	[SuppressBindingMemberBashingTest]
	public partial class OrgHeaderLinkForm : ZChildForm
	{
		public OrgHeaderLinkForm(OrgHeaderLink organisationLink)
			: base(organisationLink)
		{
			Argument.NotNull(organisationLink, nameof(organisationLink));

			InitializeComponent();

			Organisation2Button.ColorChanger.SetNotificationBackColor(this.BackColor);
			Organisation1Button.ColorChanger.SetNotificationBackColor(this.BackColor);
			OtherButton.ColorChanger.SetNotificationBackColor(this.BackColor);

			OrganisationLink = organisationLink;
			Organisation1 = organisationLink.Organisation1;
			Organisation2 = organisationLink.Organisation2;

			if (!Organisation1.OH_Code.IsEmpty && !Organisation1.OH_FullName.IsEmpty)
			{
				Organisation1Button.Text = String.Format(CultureInfo.CurrentCulture,
					Res.GetString("307b0b2a-cc0c-4ca7-8bc0-ea8e90106ba4", "{0} - {1}"), Organisation1.OH_Code,
					Organisation1.OH_FullName);
			}
			else
			{
				Organisation1Button.Text = Res.GetString("241912b8-5f03-4f2e-a43a-ede4022889ca", "New Organization");
			}
			Organisation2Button.Text = String.Format(CultureInfo.CurrentCulture, Res.GetString("307b0b2a-cc0c-4ca7-8bc0-ea8e90106ba4", "{0} - {1}"), Organisation2.OH_Code, Organisation2.OH_FullName);
		}

		public OrgHeaderLink OrganisationLink { get; }

		public OrgHeader Organisation1 { get; }

		public OrgHeader Organisation2 { get; }

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void SaveAndCloseButton_Click(object sender, EventArgs e)
		{
			if (!Organisation1Button.Checked && !Organisation2Button.Checked && !OtherButton.Checked) //If no button is selected
			{
				var caption = Res.GetString("C38FD97E-EE12-4F99-9A6F-4AFA1C7D1141", "Warning");
				var message = Res.GetString("CC89DC91-D497-4285-9E59-28F3AECFA878", "Please select an option.");
				Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);

				return;
			}

			if (TryAddRelatedParty(Organisation1, Organisation2))
			{
				SaveAndCloseButton.Enabled = false;
				CancelLinkingButton.Enabled = false;

				if (Organisation1.IsInDatabase && Organisation2.IsInDatabase)
				{
					if (OrganisationLink.Save())
					{
						CloseForm();
					}
					else
					{
						OrganisationLink.DeleteRelatedParties();
						SaveAndCloseButton.Enabled = true;
						CancelLinkingButton.Enabled = true;
					}
				}
				else
				{
					CloseForm();
				}
			}
		}

		void CloseForm()
		{
			var caption = Res.GetString("D6B7EF61-B90E-4340-BEF5-D9F5FE327C64", "Success");
			var message = Res.GetString("5738380E-B3FF-4336-B53D-FFA02B31CE39", "Organizations successfully linked.");
			Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.None);
			DialogResult = DialogResult.OK;
			Close();
		}

		SecurityCheckpoint GetOrgDetailsSecurityCheckpoint(OrgHeader org1)
		{
			return org1.IsInDatabase ? Env.Security.OrgDetailsModifyRelatedParties : Env.Security.OrgDetailsNewModifyRelatedParties;
		}

		public bool TryAddRelatedParty(OrgHeader org1, OrgHeader org2)
		{
			var securityCheckpoint = GetOrgDetailsSecurityCheckpoint(Organisation1);
			var relatedSucceeded = false;
			if (securityCheckpoint.IsAllowed)
			{
				if (Organisation1Button.Checked)
				{
					relatedSucceeded = OrganisationLink.TryAddNewOrgRelatedParty(org1, org2);
				}
				else if (Organisation2Button.Checked)
				{
					relatedSucceeded = OrganisationLink.TryAddNewOrgRelatedParty(org2, org1);
				}
				else
				{
					var otherOrg = OrganisationFindBox.Guid == ZGuid.Empty ? null : org1.Factory.Load<OrgHeader>(OrganisationFindBox.Guid);
					relatedSucceeded = OrganisationLink.TryAddNewOrgRelatedParty(otherOrg, org1, org2);
				}
			}
			else
			{
				securityCheckpoint.ShowError();
			}
			return relatedSucceeded;
		}

#if DEBUG

		public ZRadioButton OtherButtonForTest => OtherButton;
		public ZRadioButton Organisation1ButtonForTest => Organisation1Button;
		public ZRadioButton Organisation2ButtonForTest => Organisation2Button;
		public ZOrganisationFindBox OrganisationFindBoxForTest => OrganisationFindBox;

#endif
	}
}
