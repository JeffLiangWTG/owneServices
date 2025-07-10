using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class SalesEnquiryFindOrgForm : ZChildForm
	{
		public SalesEnquiryFindOrgForm()
		{
			InitializeComponent();
		}

		public SalesEnquiryFindOrgForm(SalesEnquiry enquiry, EnquiryOrgFinder finder, bool saveEnquiryOnClosed)
			: base(finder)
		{
			this.enquiry = enquiry;
			this.saveEnquiryOnClosed = saveEnquiryOnClosed;

			InitializeComponent();

			Finder.ShouldLinkToExistingClientIntelligenceInfo.ValueChanged += LinkOrCreateOptionChanged;
			Finder.ShouldCreateNewClientIntelligenceInfo.ValueChanged += LinkOrCreateOptionChanged;
			Finder.SingleOrgPkInfo.ValueChanged += SingleOrgPkInfo_ValueChanged;
			SingleOrgGuidFindBox.Selected += SingleOrgGuidFindBox_Selected;
		}

		void SingleOrgGuidFindBox_Selected(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			// Force SingleOrgPk to be updated immediately upon popup selection
			SingleOrgGuidFindBox.Validate();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			UpdateEnabledForGroupBoxControls();

			SingleOrgAddressesDisplayGrid.MouseUp += SingleOrgAddressesDisplayGrid_MouseUpOrLeave;
			SingleOrgAddressesDisplayGrid.Leave += SingleOrgAddressesDisplayGrid_MouseUpOrLeave;
			SimilarOrgDisplayGrid.MouseUp += SimilarOrgDisplayGrid_MouseUpOrLeave;
			SimilarOrgDisplayGrid.Leave += SimilarOrgDisplayGrid_MouseUpOrLeave;
			Activated += SalesEnquiryFindOrgForm_Activated;
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			UpdateAddressDisplayGridVisibility();
		}

		readonly SalesEnquiry enquiry;
		readonly bool saveEnquiryOnClosed;

		public override string FormCaption
		{
			get { return Res.GetString("FFB63440-A803-42DB-B4C0-6151F076FABF", "Inquiry Organizations"); }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		EnquiryOrgFinder Finder
		{
			get { return (EnquiryOrgFinder)BusinessEntity; }
		}

		#region Event Handlers

		void LinkOrCreateOptionChanged(object sender, EventArgs e)
		{
			Finder.ClearAllNotifications();
			OkButton.Enabled = Finder.ShouldLinkToExistingClientIntelligence || Finder.ShouldCreateNewClientIntelligence;
			UpdateEnabledForGroupBoxControls();
		}

		void UpdateEnabledForGroupBoxControls()
		{
			foreach (Control control in LinkToExistingClientIntelligenceGroupBox.Controls)
			{
				if (control != LinkToExistingClientIntelligenceRadioButton)
				{
					control.Enabled = Finder.ShouldLinkToExistingClientIntelligence;
				}
			}
			foreach (Control control in CreateNewClientIntelligenceGroupBox.Controls)
			{
				if (control != CreateNewClientIntelligenceRadioButton)
				{
					control.Enabled = Finder.ShouldCreateNewClientIntelligence;
				}
			}

			ApproveWebAccessGroupBox.Enabled = Env.Security.OrgDetailsNewWebSecurity.IsAllowed;
		}

		void SingleOrgPkInfo_ValueChanged(object sender, EventArgs e)
		{
			Finder.SelectedAddressPk = ZGuid.Empty;
			UpdateAddressDisplayGridVisibility();
		}

		void UpdateAddressDisplayGridVisibility()
		{
			if (Finder.SingleOrgPk.IsValid)
			{
				SimilarOrgDisplayGrid.Visible = false;

				SingleOrgAddressesDisplayGrid.Visible = true;
				SingleOrgAddressesDisplayGrid.UnSelectAll();
				if (SingleOrgAddressesDisplayGrid.ListManager.Count > 0)
				{
					SingleOrgAddressesDisplayGrid.Select(0);
					SetSelectedAddressPkUsingSingleOrg();
				}
			}
			else
			{
				SingleOrgAddressesDisplayGrid.Visible = false;

				SimilarOrgDisplayGrid.Visible = true;
				SimilarOrgDisplayGrid.UnSelectAll();
				if (SimilarOrgDisplayGrid.ListManager.Count > 0)
				{
					SimilarOrgDisplayGrid.Select(0);
					SetSelectedAddressPkUsingSimilarOrg();
				}
			}
		}

		void SingleOrgAddressesDisplayGrid_MouseUpOrLeave(object sender, EventArgs e)
		{
			SetSelectedAddressPkUsingSingleOrg();
		}

		void SetSelectedAddressPkUsingSingleOrg()
		{
			var selectedOrgAddress = (SingleOrgAddressesDisplayGrid.SelectedRowCount == 1) ? (OrgAddress)SingleOrgAddressesDisplayGrid.SelectedElements[0] : null;
			Finder.SelectedAddressPk = (selectedOrgAddress != null) ? selectedOrgAddress.PK : ZGuid.Empty;
		}

		void SimilarOrgDisplayGrid_MouseUpOrLeave(object sender, EventArgs e)
		{
			SetSelectedAddressPkUsingSimilarOrg();
		}

		void SetSelectedAddressPkUsingSimilarOrg()
		{
			var selectedPatternMatch = (SimilarOrgDisplayGrid.SelectedRowCount == 1) ? (OrgPatternMatch)SimilarOrgDisplayGrid.SelectedElements[0] : null;
			Finder.SelectedAddressPk = (selectedPatternMatch != null) ? selectedPatternMatch.OS_OA : ZGuid.Empty;
		}

		void SalesEnquiryFindOrgForm_Activated(object sender, EventArgs e)
		{
			var oldGridPosition = SimilarOrgDisplayGrid.ListManager.Position;
			Finder.RefreshSimilarOrgMatches();
			if (SimilarOrgDisplayGrid.Visible)
			{
				if ((oldGridPosition >= 0) && (SimilarOrgDisplayGrid.ListManager.Count > oldGridPosition))
				{
					SimilarOrgDisplayGrid.ListManager.Position = oldGridPosition;
					SimilarOrgDisplayGrid.Select(oldGridPosition);
				}
				SetSelectedAddressPkUsingSimilarOrg();
			}
		}

		protected void OkButton_Click(object sender, EventArgs e)
		{
			if (Finder.ShouldLinkToExistingClientIntelligence)
			{
				PerformLink();
			}
			else if (Finder.ShouldCreateNewClientIntelligence)
			{
				PerformCreateNew();
			}
		}

		void PerformLink()
		{
			Finder.RunPreSaveValidation();

			if (!Finder.HasErrors)
			{
				DialogResult result = DialogResult.No;
				if (Finder.ShouldLinkOrganizationAddressToInquiry)
				{
					var missingSecurity = enquiry.MissingSecurityForLinkToOrganizationByLinkingToAddress(Finder.SelectedAddress.OA_OH);
					if (missingSecurity != null)
					{
						missingSecurity.ShowError();
					}
					else
					{
						result = Globals.Message.Show(
						Res.GetString("6de489db-5e59-4e6e-8704-c0cebe5291a6", "Are you sure you want to link selected Organization Address to Inquiry? This action cannot be reversed."),
						Res.GetString("357a11d5-f2de-4c8a-9bd6-0aba5167fcd4", "Link selected Organization Address to Inquiry"),
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Question);

						if (result == DialogResult.Yes)
						{
							enquiry.LinkToOrganizationByLinkingToAddress(Finder.SelectedAddressPk);
						}
					}
				}
				else if (Finder.ShouldAddInquiryAddressToOrganization)
				{
					var orgPk = Finder.IsSingleOrgValid ? Finder.SingleOrgPk : Finder.SelectedAddress.OA_OH;
					var missingSecurity = enquiry.MissingSecurityForLinkToOrganizationByAddingInquiryAddress(orgPk);
					if (missingSecurity != null)
					{
						missingSecurity.ShowError();
					}
					else
					{
						result = Globals.Message.Show(
						Res.GetString("74314476-452e-4113-a4c6-fc21fd9d9543", "Are you sure you want to add Inquiry Address to selected Organization? This action cannot be reversed."),
						Res.GetString("acfd0067-95cb-4c84-bf01-6ad41db081f5", "Add Inquiry Address to selected Organization"),
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Question);

						if (result == DialogResult.Yes)
						{
							enquiry.LinkToOrganizationByAddingInquiryAddress(orgPk);
						}
					}
				}

				if (result == DialogResult.Yes)
				{
					if (saveEnquiryOnClosed)
					{
						if (enquiry.HasErrors)
						{
							using (ZMessageBox msgBox = new ZErrorMessageBox(enquiry))
							{
								ZFormModaliser.ShowMessageBoxWithoutDispose(msgBox);
							}
							DialogResult = DialogResult.Cancel;
							Close();
						}
						else
						{
							ZExceptionReporting.ProcessWithSaveExceptionHandling(enquiry.Factory.Save, null, true);
							CloseAndSetDialogResult();
						}
					}
					else
					{
						CloseAndSetDialogResult();
					}
				}
			}
			else
			{
				Globals.Message.Show(Finder.NotificationMessagesAsText, Res.GetString("02C65D9C-A86E-40B0-A34E-BA0E91292306", "Cannot Link"), MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		void PerformCreateNew()
		{
			var missingSecurity = PerformNewSecurityCheckPoints.FirstOrDefault(x => !x.IsAllowed);
			if (missingSecurity == null)
			{
				Finder.RunPreSaveValidation();

				if (!Finder.HasErrors)
				{
					ZController controller = ZControllerFactory.Create(ControllerIDs.ClientIntelligence);
					controller.SetFormsModalTo(this);
					var newOrg = enquiry.CreateOrg(controller.Factory);
					var contact = newOrg.Contacts[0];
					if (Finder.ShouldAllowWebAccess)
					{
						contact.OC_WebAccessEnabled = true;
					}
					var orgForm = (ZForm)controller.ShowFormForNewEntity(newOrg);
					((OrgHeader)orgForm.BusinessEntity).OrgSaved += (s, e) =>
					{
						if (Finder.ShouldAllowWebAccess)
						{
							contact.GetWebPasswordEmail().SendEmail();
						}

						enquiry.LinkToOrganizationDirectly(newOrg.PK, newOrg.MainAddress.PK, contact.PK);

						if (saveEnquiryOnClosed)
						{
							enquiry.Factory.Save();
						}
					};
					orgForm.Closed += new EventHandler(orgForm_Closed);
				}
				else
				{
					Globals.Message.Show(Finder.NotificationMessagesAsText, Res.GetString("e64201d7-c090-443a-94e8-f9c2877c4c14", "Cannot Create"), MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			else
			{
				missingSecurity.ShowError();
			}
		}

		void orgForm_Closed(object sender, EventArgs e)
		{
			CloseAndSetDialogResult();
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			CloseAndSetDialogResult();
		}

		void CloseAndSetDialogResult()
		{
			Close();
			if (enquiry.Header != null)
			{
				DialogResult = DialogResult.OK;
			}
		}

		protected override void OnClosed(EventArgs e)
		{
			Activated -= SalesEnquiryFindOrgForm_Activated;

			base.OnClosed(e);
		}

		#endregion

		#region Security

		SecurityCheckpoint[] PerformNewSecurityCheckPoints
		{
			get
			{
				if (performNewSecurityCheckPoints == null)
				{
					performNewSecurityCheckPoints = new SecurityCheckpoint[] { Env.Security.ClientIntelligenceModify, Env.Security.OrgContactNew, Env.Security.OrgAddressNew, Env.Security.OrgAddressDetailsNonARAPNew };
				}
				return performNewSecurityCheckPoints;
			}
		}

		SecurityCheckpoint[] performNewSecurityCheckPoints;

		#endregion
	}
}
