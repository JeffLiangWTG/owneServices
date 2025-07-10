using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Recruiter;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.BrowserInterop;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.MasterFiles.GUI.DuplicateAlertControlHelper;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ContactsUserControl : OrganisationSecurityContainerControl, ISupportDuplicationAlertControl
	{
		public ContactsUserControl()
		{
			InitializeComponent();
			SetCharacterCasing();

			if (OrganisationsDataRegistry.Instance.EnableControllingContactFilters.Value)
			{
				ContactsFilterOptionDropEdit.Visible = true;
				ContactsFilterStringTextBox.CaptionResourceString = null;
				OrgContactPanel.Resize += OnContactPanelResize;
			}

			var carbonCopyRecipientsColumnStyleInfo = new CopyRecipientsColumnStyleInfo<OrgDocumentCopyRecipient, OrgDocumentCopyRecipientCollection, OrgDocument>
			{
				ColumnName = "OD_CarbonCopyRecipientsAsString",
				EmailAddressPropertyName = OrgDocumentCopyRecipientSchema.ODR_EmailAddress.Name,
				GetCopyRecipients = GetCarbonCopyRecipients,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			};
			OrgDocumentBoundGrid.ColumnStyles.Add(carbonCopyRecipientsColumnStyleInfo);

			var blindCarbonCopyRecipientsColumnStyleInfo = new CopyRecipientsColumnStyleInfo<OrgDocumentCopyRecipient, OrgDocumentCopyRecipientCollection, OrgDocument>
			{
				ColumnName = "OD_BlindCarbonCopyRecipientsAsString",
				EmailAddressPropertyName = OrgDocumentCopyRecipientSchema.ODR_EmailAddress.Name,
				GetCopyRecipients = GetBlindCarbonCopyRecipients,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			};
			OrgDocumentBoundGrid.ColumnStyles.Add(blindCarbonCopyRecipientsColumnStyleInfo);

			UpdateContactSecurityGridVisibility();

			if (string.IsNullOrEmpty(URLHelpers.GlowPortalsUri))
			{
				webSecurityLinkLabel.Text = ResString.GetMultilingualString("ContactsUserControl|zLabel1.Text", "These security rights apply to the selected contact and are defaulted from the default Organization security settings. Web Access will only be available for this contact if 'Web Access' is ticked, and a password has been specified.");
				webSecurityLinkLabel.Links.Clear();
			}
			else
			{
				webSecurityLinkLabel.Text = ResString.GetMultilingualString("ContactsUserControl|webSecurityLinkLabel", "These security rights apply to the selected contact and are defaulted from the default Organization security settings. Web Access will only be available for this contact if 'Web Access' is ticked, and a password has been specified. Click on {0} to manage GLOW Web Security Rights.", CargoWiseWebPortalUserAdmin);
				var indexOfCargoWiseWebPortalUserAdmin = webSecurityLinkLabel.Text.IndexOf(CargoWiseWebPortalUserAdmin);

				if (indexOfCargoWiseWebPortalUserAdmin > 0)
				{
					webSecurityLinkLabel.LinkArea = new LinkArea(indexOfCargoWiseWebPortalUserAdmin, CargoWiseWebPortalUserAdmin.Length);
				}
				else
				{
					ErrorReporter.ReportOnce("GlowPortalUserAdmin link is invalid.", FormattableString.Invariant($"Text is {webSecurityLinkLabel.Text}"));
				}
			}

			CampaignsGrid.AfterBind += CampaignsGrid_AfterBind;
			OrgContactBoundGrid.ContextMenu.Popup += OrgContactBoundGridContextMenu_Popup;
			AddMarkMainEmailAsVerifiedMenuItem();
			AddDeactivateWithRedirectionMenuItem();
			OrgContactBoundGrid.AfterBind += OrgContactBoundGrid_AfterBind;

			if (!this.IsDesignMode())
			{
				EditPersonButton.Available = SystemDataRegistry.Instance.PersonIntelligenceModuleEnabled.Value;
				SubscriptionsGrid.ReadOnly = !Env.Security.OrganisationControlSubscriptionPreferences.IsAllowed;
			}

			AddEmailSubjectMacroColumn();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "User admin strings")]
		const string CargoWiseWebPortalUserAdmin = "CargoWise Web Portal User Administration";

		void GlowPortalLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (SelectedContact is null)
			{
				Globals.Message.ShowError(Res.GetString("E61505EA-4BF8-42F0-BBF3-D430BC335A5F", "A contact must be selected in the grid."));
				return;
			}

			var url = new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl("goto/SSMContactDetail", SelectedContact.HumanReadableName, additionalQueryStrings: new[] { ("entityPK", SelectedContact.PK.ToString()) });
			if (url != null)
			{
				WebUrlLauncher.Launch(url.ToString());
			}
			else
			{
				ErrorReporter.ReportOnce("Could not generate SSM url");
				Globals.Message.ShowError(Res.GetString("22ce125e-b8a0-46ca-88dd-0180e5955d96", "Could not generate URL."));
			}
		}

		void SecurityTabPage_VisibleChanged(object sender, EventArgs e)
		{
			var control = (Control)sender;
			if (!control.Visible)
			{
				return;
			}

			SendPasswordInstructionToolStripPanel.Visible = WebDataRegistry.Instance.ChoiceOfPasswordSetAndResetProcessFlowEnabled.Value && !string.IsNullOrEmpty(GlowRegistry.Instance.GlowPortalsUri.Value);
			SendPasswordInstructionsButton.Visible = !SendPasswordInstructionToolStripPanel.Visible;
		}

		void AddEmailSubjectMacroColumn()
		{
			var emailSubjectColumnInfo = new ZMacrosFindBoxColumnStyleInfo
			{
				CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("{D5B276DB-3752-41F9-BC00-9CE0A53194AB}", "Email Subject"),
				ColumnName = OrgDocumentSchema.OD_EmailSubjectMacro.Name,
				Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
				IsUsedForExpressions = true,
				AllowMultipleMacroses = true,
				UsePredefinedRoots = true,
				RootTypes = new[]
				{
					typeof(OrgHeader),
					typeof(OrgContact),
					typeof(OrgDocument),
					ObjectFactory.GetType<Forwarding.IForwardingConsol>(),
					ObjectFactory.GetType<Forwarding.IForwardingShipment>(),
					ObjectFactory.GetType<IARInvoice>(),
					ObjectFactory.GetType<IWorkItem>(),
					ObjectFactory.GetType<IDtbBooking>(),
					GenericWrapperLoader.GetFromDataContext(Core.Constants.DataContext.GenericFreightJob).GetWrapperType()
				}
			};
			OrgDocumentBoundGrid.ColumnStyles.Add(emailSubjectColumnInfo);

			OrgDocumentBoundGrid.AfterBind += (s, a) =>
			{
				SetRootsForMacroColumn();
				OrgDocumentBoundGrid.ListManager.CurrentChanged += (sender, args) =>
				{
					SetRootsForMacroColumn();
				};
			};

			void SetRootsForMacroColumn()
			{
				if (OrgDocumentBoundGrid.ListManager.GetCurrent() is OrgDocument currentDoc)
				{
					emailSubjectColumnInfo.Roots = new BusinessObject[] { currentDoc, currentDoc.Contact, currentDoc.Contact.ParentOrg };
				}
			}
		}

		#region RecalculatePatternTables

		public void AddRecalculatePatternTablesMenuItem(RecalculatePatternsInitializer recalculatePatternsInitializer)
		{
			recalculatePatternsInitializer.CreateRecalculateMenuItem(OrgContactBoundGrid.ContextMenu, RegenerateSelectedContacts);
		}

#if DEBUG
		internal
#endif

		protected void RegenerateSelectedContacts(object sender, EventArgs e)
		{
			totalContactsToRecalculate = OrgContactBoundGrid.SelectedElements.Length;
			currentRecalculated = 0;

			if (totalContactsToRecalculate != 0)
			{
				recalculateprogressForm = new ProgressForm();
				recalculateprogressForm.Show(this);
				recalculateprogressForm.ShowCancelButton = false;
#if DEBUG
				if (Globals.IsTest)
				{
					isrecalculateprogressFormShownForTest = true;
				}
#endif
			}

			foreach (var element in OrgContactBoundGrid.SelectedElements)
			{
				var contact = (OrgContact)element;
				contact.Person.PatternMatchingRecalculator.Recalculated += Contact_Regenerated;
				RegenerateContact(contact);
			}
		}

		protected virtual void RegenerateContact(OrgContact contact)
		{
			contact.Person.RegeneratePatternTables();
		}

#if DEBUG
		internal bool isrecalculateprogressFormShownForTest;
#endif
#if DEBUG
		internal
#endif
		ProgressForm recalculateprogressForm;

		protected int currentRecalculated;
		protected int totalContactsToRecalculate;

		public void Contact_Regenerated(object sender, RecalculatedEventArgs e)
		{
			currentRecalculated++;
			((GlbPerson)sender).PatternMatchingRecalculator.Recalculated -= Contact_Regenerated;
			if (currentRecalculated >= totalContactsToRecalculate && recalculateprogressForm != null && !recalculateprogressForm.IsDisposed)
			{
				recalculateprogressForm.Close();
				recalculateprogressForm = null;
				if (string.IsNullOrEmpty(e.ErrorMessage))
				{
					var effectStr = currentRecalculated <= 1 ? currentRecalculated + Res.GetString("3AA6EBDE-627A-4783-B580-E460AB54B018", "{0}", " contact regenerated.") : currentRecalculated + Res.GetString("C45AE20C-1D5D-4BD6-8DE2-796E4DB5D730", "{0}", " contacts affected.");
					var caption = Res.GetString("4C73E180-E8B1-4473-9370-0F26344F437F", "Regeneration has completed successfully!");
					var message = Res.GetString("7B0B229C-E11D-469F-8C3C-0997D3681EDF", "Result: {0}", effectStr);
					Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("DDF72788-CBB7-45FB-853A-B56153B1C062", "Error Message: {0}", e.ErrorMessage), Res.GetString("EE1C63B0-A756-43A7-A440-4499ED3E81AA", "Pattern tables regeneration failed"));
				}
			}
			else
			{
				int progress = (currentRecalculated * 100) / totalContactsToRecalculate;
				recalculateprogressForm.SetStatusAndPercentComplete(Res.GetString("1DD38FA3-5D4D-4ACE-8DFF-FA29B7552B07", "Recalculating contacts..."), progress);
			}
#if DEBUG
			if (Globals.IsTest)
			{
				isrecalculateprogressFormShownForTest = true;
			}
#endif
		}

		#endregion

		void OnContactPanelResize(object sender, EventArgs e)
		{
			var margin = ControlDpiScalingHelper.ScaleToCurrentDpiX(5);
			ContactsFilterOptionDropEdit.PreBoundMaxLength = OrgContactPanel.Width >= ControlDpiScalingHelper.ScaleToCurrentDpiX(500) ? 11 : 3;
			ContactsFilterStringTextBox.Location = ControlDpiScalingHelper.NewScaledPoint(ContactsFilterOptionDropEdit.Location.X + ContactsFilterOptionDropEdit.Width + margin, ControlDpiScalingHelper.ScaleToCurrentDpiX(4), false);
			ContactsFilterStringTextBox.Size = ControlDpiScalingHelper.NewScaledSize(ShowInactiveContactsCheckBox.Location.X - ContactsFilterStringTextBox.Location.X - margin, ControlDpiScalingHelper.ScaleToCurrentDpiX(20), false);
		}

		void OrgContactBoundGrid_AfterBind(object sender, EventArgs e)
		{
			HookDuplicationDetectEvents();
			OrgContactBoundGrid.ListManager.CurrentChanged += ListManager_CurrentChanged;
			OrgContactBoundGrid.ListManager.ListChanged += ListManager_ListChanged;
			OrgContactBoundGrid.RowsDeleted += OrgContactBoundGrid_RowsDeleted;

			FindSuggestedJobCategories(false, false);
			HookFindSuggestedJobCategoriesEvent();
			SuggestedJobCategoriesLabel.LinkClicked += SuggestedJobCategoriesLabel_LinkClicked;

			SetSendPasswordInstructionsButton();
		}

		void CampaignsGrid_AfterBind(object sender, EventArgs e)
		{
			SelectedCampaignChanged(CampaignsGrid, EventArgs.Empty);
			CampaignsGrid.ListManager.CurrentChanged += SelectedCampaignChanged;
		}

		void SelectedCampaignChanged(object sender, EventArgs e)
		{
			var currentCampaignItem = SelectedCampaignItem;
			if (currentCampaignItem != null)
			{
				bool islearningCentreCampaignItem = currentCampaignItem is ILearningCentreCampaignItem;

				this.SendCampaignButton.Visible = !islearningCentreCampaignItem;
				this.ResendCampaignButton.Visible = !islearningCentreCampaignItem;
				this.ViewCampaignButton.Visible = !islearningCentreCampaignItem;
				this.ViewCampaignDocumentButton.Visible = !islearningCentreCampaignItem;
				this.EditCampaignButton.Visible = !islearningCentreCampaignItem;

				this.ViewExamButton.Visible = islearningCentreCampaignItem;
				this.ViewExamHistoryButton.Visible = islearningCentreCampaignItem;
				this.ResitExamButton.Visible = islearningCentreCampaignItem;

				IGlbCompanyCampaign campaign = currentCampaignItem.Campaign;
				if (campaign.IsTargetList)
				{
					ResendCampaignButton.Visible = false;
					EditCampaignButton.Visible = false;
					ViewCampaignDocumentButton.Visible = false;
				}
			}
		}

		#region CopyRecipients

		OrgDocumentCopyRecipientCollection GetCarbonCopyRecipients(OrgDocument orgDocument)
		{
			OrgDocumentCopyRecipientCollection result = null;
			if (orgDocument != null)
			{
				result = orgDocument.CarbonCopyRecipients;
			}
			return result;
		}

		OrgDocumentCopyRecipientCollection GetBlindCarbonCopyRecipients(OrgDocument orgDocument)
		{
			OrgDocumentCopyRecipientCollection result = null;
			if (orgDocument != null)
			{
				result = orgDocument.BlindCarbonCopyRecipients;
			}
			return result;
		}

		#endregion

		#region SetupContactDetails

		void AddMarkMainEmailAsVerifiedMenuItem()
		{
			OrgContactBoundGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("ContactsUserControl|b7fce869-c9ab-4483-85b4-6138cbe24439", "Reset Email (Main) Delivery Status"), MarkMainEmailAsVerified_Click));
		}

		void MarkMainEmailAsVerified_Click(object sender, EventArgs e)
		{
			foreach (var contact in OrgContactBoundGrid.SelectedElements.Cast<OrgContact>())
			{
				contact.IsNDR = false;
			}
		}

		void AddDeactivateWithRedirectionMenuItem()
		{
			OrgContactBoundGrid.ContextMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("ContactsUserControl|80eaad67-8152-48c0-bec4-504e8490485c", "Deactivate and Supersede Web Access"), AddDeactivateWithRedirection_Click));
		}

		void AddDeactivateWithRedirection_Click(object sender, EventArgs e)
		{
			OrgContactSupersedeHelper.SupersedeContacts(OrgContactBoundGrid.SelectedElements.Cast<OrgContact>().ToArray());
		}

		#endregion

		#region Security Grid

		protected void OrgContactBoundGridContextMenu_Popup(object sender, EventArgs e)
		{
			BusinessObject obj = (BusinessObject)OrgContactBoundGrid.List[OrgContactBoundGrid.CurrentRowIndex];
			if (obj != null)
			{
				OrgContactBoundGrid.DeleteMenuItem.Enabled = OrgContactBoundGrid.DeleteMenuItem.Enabled && obj.CanDelete;
			}

			OrgContactBoundGrid.ExportAllColumnsToExcelMenuItem.Enabled = Env.Security.OrgContactViewExportContacts.IsAllowed;
			OrgContactBoundGrid.ExportVisibleColumnsToExcelMenuItem.Enabled = Env.Security.OrgContactViewExportContacts.IsAllowed;
		}

		void WebAccessCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			UpdateContactSecurityGridVisibility();
		}

		void UpdateContactSecurityGridVisibility()
		{
			WebSecuritySplitContainer.Visible = WebAccessCheckBox.Checked;
			WebSecurityLabel.Visible = !WebAccessCheckBox.Checked;
		}

		void UpdateDocDeliveryButtonsForSecurity()
		{
			SuppressedDocsButton.Enabled = Organisation.SecurityProvider.HasModifyContactDocDeliveryDetailsSecurity;
			AutoDeliveryButton.Enabled = Organisation.SecurityProvider.HasModifyContactDocDeliveryDetailsSecurity;
		}

		#endregion

		#region Auto-Delivery Details

		protected virtual void AutoDeliveryButton_Click(object sender, EventArgs e)
		{
			DocAutoDeliveryContactViewer contactViewer = new DocAutoDeliveryContactViewer(Organisation);
			ZFormModaliser.ShowDialogAndDispose(new DocAutoDeliveryContactsForm(contactViewer));
		}

		#endregion

		#region Suppressed Documents

		void SuppressedDocsButton_Click(object sender, EventArgs e)
		{
			using (var form = new SuppressDocsForOrgForm(Organisation))
			{
				var oldDocumentPKs = new ArrayList();
				Organisation.SuppressedDocuments.ForEach(doc => oldDocumentPKs.Add(doc.PK));
				var result = ShowFormWithoutDispose(form);
				if (result == DialogResult.Cancel)
				{
					for (var i = Organisation.SuppressedDocuments.Count - 1; i >= 0; i--)
					{
						var document = Organisation.SuppressedDocuments[i];
						if (!oldDocumentPKs.Contains(document.PK))
						{
							Organisation.SuppressedDocuments.Remove(document);
							document.Delete();
						}
					}
				}
			}
		}

		protected virtual DialogResult ShowFormWithoutDispose(ZForm form)
		{
			return ZFormModaliser.ShowDialogWithoutDispose(form);
		}

		#endregion

		#region Lockout Contact

		IOrgContactLoginAttemptRecorder LoginAttemptRecorder => loginAttemptRecorder ?? (loginAttemptRecorder = ObjectFactory.Get<IOrgContactLoginAttemptRecorder>());
		IOrgContactLoginAttemptRecorder loginAttemptRecorder;

		void UpdateLockedOutControls()
		{
			var isLockedOut = false;
			if (SelectedContact != null)
			{
				isLockedOut = LoginAttemptRecorder.IsAnonymousUserLockedOut(SelectedContact.OrgCode, SelectedContact.OC_Email);
			}

			UnlockButton.Visible = isLockedOut;
			LockoutDateTimeBoundDate.Visible = isLockedOut;
		}

		protected void UnlockButton_Click(object sender, EventArgs e)
		{
			if (SelectedContact != null)
			{
				LoginAttemptRecorder.Unlock(SelectedContact.OrgCode, SelectedContact.OC_Email, true);
				Organisation.HasChanges = true;
			}

			UpdateLockedOutControls();
		}

		#endregion

		#region Web Password

		protected void SendPasswordInstructionsButton_Click(object sender, EventArgs e)
		{
			SendPasswordInstructions(PasswordInstructionUrlType.Default);
		}

		protected void UseCargoWiseWebPortalsStripButton_Click(object sender, EventArgs e)
		{
			if (!WebDataRegistry.Instance.ChoiceOfPasswordSetAndResetProcessFlowEnabled.Value)
			{
				Globals.Message.Show(Res.GetString("17a68e23-8791-46f5-aa40-1343016a93ad", "You need to enable the System Registry under the path Web > Enable Choice of Password Set/Reset Process Flow"));
				return;
			}
			if (string.IsNullOrEmpty(GlowRegistry.Instance.GlowPortalsUri.Value))
			{
				Globals.Message.Show(Res.GetString("d56d5c3b-ac93-4d4b-8fb9-709929477fcb", "You need to provide a Glow Portals Root Uri in the System Registry under the path GLOW > Services > GLOW Portals Root URL"));
				return;
			}
			SendPasswordInstructions(PasswordInstructionUrlType.Glow);
		}

		void SendPasswordInstructions(PasswordInstructionUrlType type)
		{
			if (Organisation.HasChanges)
			{
				var result = Globals.Message.Show(
					Res.GetString("ccd4a035-0656-48ab-a59a-ddf7b082a12d", "You must save this form first. Would you like to save now?"),
					Res.GetString("0e495a33-beea-4020-a6ba-557a216a5551", "Cannot Send"),
					MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);

				if (result == DialogResult.No || ((ZForm)FindForm()).FireSaveButton() == ContinueWithSave.No)
				{
					return;
				}
			}

			if (!SelectedContact.OC_IsActive)
			{
				var userConfirmation = Globals.Message.Show(
					Res.GetString("a8eeb857-97a5-468b-b0a4-d337aa71bc06", "This contact is not active. Would you like to activate the contact and send the password instruction?"),
					Res.GetString("09efaa65-1751-4a53-8114-624f836a6fb2", "Send password instructions"),
					MessageBoxButtons.YesNo, MessageBoxIcon.Question);

				if (userConfirmation == DialogResult.No)
				{
					return;
				}

				SelectedContact.OC_IsActive = true;
			}

			if (ContactSendEmailSetResetPassword.SendPasswordInstructions(SelectedContact, type: type))
			{
				SetSendPasswordInstructionsButton();
			}
		}

		#endregion

		#region	Campaigns

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource != null && Organisation != null)
			{
				UpdateDocDeliveryButtonsForSecurity();
			}
		}

		protected void ShowCampaignDetailsForm()
		{
			IGlbCompanyCampaignItem selected = SelectedCampaignItem;
			if (SelectedCampaignItem != null)
			{
				CampaignItemController.ShowEditForm((BusinessObject)selected);
			}
			else
			{
				Globals.Message.Show(ResString.GetMultilingualString("fcf73384-b105-469c-9755-37733835d617", "Please select a Campaign to view."),
					ResString.GetMultilingualString("c1d878af-971a-4f6a-a650-20634e827ad4", "Cannot View"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		protected void ViewCampaignButton_Click(object sender, EventArgs e)
		{
			IGlbCompanyCampaignItem selected = SelectedCampaignItem;
			if (selected != null)
			{
				CampaignController.ShowViewForm((BusinessObject)selected.Campaign);
			}
			else
			{
				Globals.Message.Show(ResString.GetMultilingualString("b72696fd-939a-4290-a9b3-df5173f6e8fe", "Please select a Campaign to edit."),
					ResString.GetMultilingualString("b52c9861-e179-4207-bb56-df5976ecd8ad", "Cannot Edit"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		protected void SendCampaignButton_Click(object sender, EventArgs e)
		{
			if (!Env.Security.CampaignManagement.IsAllowed)
			{
				Env.Security.CampaignManagement.ShowError();
			}
			else if (SelectedContact != null)
			{
				if (EnsureFormSavedForSendingCampaign())
				{
					GetContactCampaignSenderGUIManager().SendCampaign(SelectedContact);
				}
			}
			else
			{
				Globals.Message.Show(ResString.GetMultilingualString("b1224731-3f7a-4f65-b675-9c82b25f0431", "Please select a contact to send Campaign."),
					ResString.GetMultilingualString("b7e3c8ca-f49c-42a7-88fa-e29cbfc5c6af", "Cannot Send"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		protected void ResendCampaignButton_Click(object sender, EventArgs e)
		{
			IGlbCompanyCampaignItem selected = SelectedCampaignItem;
			if (!Env.Security.CampaignManagement.IsAllowed)
			{
				Env.Security.CampaignManagement.ShowError();
			}
			else
			{
				if (selected != null)
				{
					if (EnsureFormSavedForSendingCampaign())
					{
						GetContactCampaignSenderGUIManager().ResendCampaign(selected);
					}
				}
				else
				{
					Globals.Message.Show(ResString.GetMultilingualString("95ac350d-8e61-4804-af8b-cc2b57fee7f2", "Please select a Campaign to resend."),
						ResString.GetMultilingualString("0489c2ef-d17e-4637-b4d1-6bb104e92282", "Cannot Resend"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				}
			}
		}

		protected void ViewCampaignDocumentButton_Click(object sender, EventArgs e)
		{
			IGlbCompanyCampaignItem selected = SelectedCampaignItem;
			if (selected != null)
			{
				// Write the blob directly to file. No need to convert to a string which requires detecting the encoding.
				string filePath = Temp.GetTempFileNameWithExtension(".htm");
				File.WriteAllBytes(filePath, selected.Campaign.HtmlDocumentBlob);
				ZFormModaliser.ShowDialogAndDispose(RichTextEmailDisplayZForm.FromFile(filePath, selected.Campaign.G0_EmailSubject));
			}
			else
			{
				Globals.Message.Show(ResString.GetMultilingualString("50c48ac8-5c98-4045-af43-3799e9bc93ef", "Please select a Campaign to view."),
					ResString.GetMultilingualString("8c178f0c-8de2-4a3d-baa7-3b758b63f4db", "Cannot View"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		protected void EditPersonButton_Click(object sender, EventArgs e)
		{
			SelectedContact?.ReloadPerson();
			if (SelectedPerson != null)
			{
				var controller = ZControllerFactory.Create(ControllerIDs.GlbPerson);
				controller.SetFormsModalTo(ParentForm);
				controller.ShowEditForm(SelectedPerson);
			}
		}

		void EditCampaignButton_Click(object sender, EventArgs e)
		{
			ShowCampaignDetailsForm();
		}

		void CampaignsGrid_DoubleClick(object sender, EventArgs e)
		{
			if (EditCampaignButton.Visible)
			{
				ShowCampaignDetailsForm();
			}
			else
			{
				var ratingGUIManager = GetContactSkillRatingGUIManager();
				var selectedCampaign = SelectedCampaignItem;
				if (selectedCampaign != null)
				{
					ratingGUIManager.ShowExamHistory(selectedCampaign, ZString.Empty);
				}
			}
		}

		protected virtual IContactCampaignSenderGUIManager GetContactCampaignSenderGUIManager()
		{
			return ObjectFactory.Get<IContactCampaignSenderGUIManager>();
		}

		bool EnsureFormSavedForSendingCampaign()
		{
			var form = FindForm() as ZForm;
			var bizObj = (BusinessObject)form.BusinessEntity;
			if (bizObj.IsInDatabase && !bizObj.HasChanges)
			{
				return true;
			}

			return
				(ShowSaveConfirmationForSendingCampaign(bizObj) == DialogResult.Yes) &&
				(form.FireSaveButton() == ContinueWithSave.Yes);
		}

		DialogResult ShowSaveConfirmationForSendingCampaign(BusinessObject bizObj)
		{
			var caption = ResString.GetMultilingualString("6e4d82af-345a-4118-b18a-dcd95d636d58", "Save Confirmation");
			var message = ResString.GetMultilingualString("3bc38363-c4af-42e9-8646-f10a2f10a352", "{0} must be saved before a campaign can be sent. Do you wish to save?", bizObj.HumanReadableName);
			return Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.Yes);
		}

		protected OrgHeader Organisation
		{
			get
			{
				OrgHeader result = null;
				ZForm form = (ZForm)FindForm();
				if (form != null)
				{
					if (form.BusinessEntity is OrgHeader)
					{
						result = (OrgHeader)form.BusinessEntity;
					}
					else if (form.BusinessEntity is OrgContact)
					{
						result = ((OrgContact)form.BusinessEntity).Header;
					}
				}
				return result;
			}
		}

		public OrgContact SelectedContact
		{
			get
			{
				if (OrgContactBoundGrid.ListManager != null)
				{
					return (OrgContact)OrgContactBoundGrid.ListManager.GetCurrent();
				}
				return null;
			}
		}

		public OrgContact InitialContactToSelect
		{
			get;
			set;
		}

		void SetInitialContactToSelectInGrid()
		{
			if (InitialContactToSelect != null)
			{
				BusinessObjectCollection collection = OrgContactBoundGrid.ListManager.List as BusinessObjectCollection;
				if (collection != null)
				{
					var element = collection.Select((contact, index) => new { contact, index })
						.FirstOrDefault(node => node.contact.PK == InitialContactToSelect.PK);

					if (element != null)
					{
						OrgContactBoundGrid.ListManager.Position = element.index;
					}
				}
			}
		}

		void ContactsUserControl_Load(object sender, EventArgs e)
		{
			SetInitialContactToSelectInGrid();
			UnHookOC_OA_OrgAddressValueChangedEvent();
			HookOC_OA_OrgAddressValueChangedEvent();
			UpdateLockedOutControls();
		}

		IGlbCompanyCampaignItem SelectedCampaignItem
		{
			get { return CampaignsGrid.CurrentRowIndex > -1 ? (IGlbCompanyCampaignItem)CampaignsGrid.ListManager.GetCurrent() : null; }
		}

		ZController CampaignController
		{
			get
			{
				if (campaignController == null)
				{
					campaignController = ZControllerFactory.Create(ControllerIDs.GlbCompanyCampaign);
					campaignController.SetFormsModalTo(ParentForm);
				}
				return campaignController;
			}
		}
		ZController campaignController;

		protected ZController CampaignItemController
		{
			get
			{
				if (campaignItemController == null)
				{
					campaignItemController = ZControllerFactory.Create(ControllerIDs.GlbCompanyCampaignItem);
					campaignItemController.SetFormsModalTo(ParentForm);
				}
				return campaignItemController;
			}
		}
		ZController campaignItemController;

		#endregion

		#region Skill Rating

		void ViewExamButton_Click(object sender, EventArgs e)
		{
			if (!Env.Security.HRJobSkillExamCampaignEdit.IsAllowed)
			{
				Env.Security.HRJobSkillExamCampaignEdit.ShowError();
			}
			else
			{
				var ratingGUIManager = GetContactSkillRatingGUIManager();
				var selectedCampaign = SelectedCampaignItem;
				if (selectedCampaign != null)
				{
					ratingGUIManager.ShowExamDetails(selectedCampaign);
				}
			}
		}

		void ViewExamHistoryButton_Click(object sender, EventArgs e)
		{
			var ratingGUIManager = GetContactSkillRatingGUIManager();
			var selectedCampaign = SelectedCampaignItem;
			if (selectedCampaign != null)
			{
				ratingGUIManager.ShowExamHistory(selectedCampaign, ZString.Empty);
			}
		}

		void ResitExamButton_Click(object sender, EventArgs e)
		{
			if (!Env.Security.HRJobSkillExamCampaignEdit.IsAllowed)
			{
				Env.Security.HRJobSkillExamCampaignEdit.ShowError();
			}
			else
			{
				var ratingGUIManager = GetContactSkillRatingGUIManager();
				var selectedCampaign = SelectedCampaignItem;
				if (selectedCampaign != null)
				{
					ratingGUIManager.ResitExam(selectedCampaign);
				}
			}
		}

		IContactSkillRatingGUIManager GetContactSkillRatingGUIManager()
		{
			return ObjectFactory.Get<IContactSkillRatingGUIManager>();
		}

		#endregion

		#region Attributes

		void OpenURLButton_Click(object sender, EventArgs e)
		{
			ZString errorMessage = OpenSelectedAttributeURL();
			if (!errorMessage.IsEmpty)
			{
				Globals.Message.Show(errorMessage, ResString.GetMultilingualString("41e3175f-bf08-4362-8bd4-27e1f3c90622", "Cannot open URL"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		void AttributesGrid_DoubleClick(object sender, EventArgs e)
		{
			OpenSelectedAttributeURL();
		}

		protected ZString OpenSelectedAttributeURL()
		{
			ZString errorMessage = ZString.Empty;
			OrgContactAttribute selectedAttribute = (OrgContactAttribute)AttributesGrid.ListManager.GetCurrent();

			if (selectedAttribute != null)
			{
				if (!selectedAttribute.PC_URLInfo.ReadOnly)
				{
					if (!selectedAttribute.PC_URL.IsEmpty)
					{
						OpenURL(selectedAttribute.PC_URL);
					}
					else
					{
						errorMessage = ResString.GetMultilingualString("33fdec25-da04-405c-9c95-064f657b58d7", "You have not entered a web site address for this Social Networking Link.");
					}
				}
				else
				{
					errorMessage = ResString.GetMultilingualString("cb3b97ba-9fc2-4442-a55f-e63dd56719db", "Web Site Addresses are only supported for Social Networking Links (SNL).");
				}
			}
			else
			{
				errorMessage = ResString.GetMultilingualString("9c10dc8c-2112-44e4-b18f-c965e388b43c", "Please select a Social Networking Link attribute to view its Web Site.");
			}

			return errorMessage;
		}

		void OpenURL(string target)
		{
			WebUrlLauncher.Launch(target);
		}

		#endregion

		#region Contacts Filter

		void ContactsFilterStringTextBox_TextChanged(object sender, EventArgs e)
		{
			((OrgHeader)BindingSource.DataSource).ContactsFilterString = ContactsFilterStringTextBox.Text;
		}

		void ContactsFilterStringTextBox_ReadOnlyChanged(object sender, EventArgs e)
		{
			if (ContactsFilterStringTextBox.ReadOnly)
			{
				ContactsFilterStringTextBox.ReadOnly = false;
			}
		}

		void ShowInactiveContactsCheckBox_ReadOnlyChanged(object sender, EventArgs e)
		{
			if (ShowInactiveContactsCheckBox.ReadOnly)
			{
				ShowInactiveContactsCheckBox.ReadOnly = false;
			}
		}

		void OnlyShowWebAccessEnabledContactsCheckBox_ReadOnlyChanged(object sender, EventArgs e)
		{
			if (OnlyShowWebAccessEnabledContactsCheckBox.ReadOnly)
			{
				OnlyShowWebAccessEnabledContactsCheckBox.ReadOnly = false;
			}
		}

		#endregion

		#region ISupportDuplicationAlertControl

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1017", Justification = "The value's calculated from scaled value ")]
		public Point DuplicationAlertAnchorLocation
		{
			get
			{
				var margin = ControlDpiScalingHelper.ScaleToCurrentDpiX(2);
				var xLocation = ContactNameTextBox.Location.X + ContactNameTextBox.Width + margin;
				var yLocation = ContactNameTextBox.Location.Y + zPanelContactDetails.Location.Y;
				return new Point(xLocation, yLocation);
			}
		}

		public Control DuplicationAlertParentControl => ContactItemsGroupBox;

		public Control DuplicationAlertReferenceControl => null;

		public string DeduplicationStatusText => DuplicateDetectionStatusLabel.Text;

		public bool DeduplicationStatusVisible => DuplicateDetectionStatusLabel.Visible;

		public DuplicateAlertControlHelper DeduplicationHelper => duplicateAlertControlHelper ?? (duplicateAlertControlHelper = new DuplicateAlertControlHelper());
		DuplicateAlertControlHelper duplicateAlertControlHelper;

		public void ShowDeduplicationStatus()
		{
			DuplicateDetectionStatusIcon.Image = Properties.Resources.loader;
			var getTextAndColor = GetTextAndColor(DuplicationConditions.ShowDeduplicationStatus);
			DuplicateDetectionStatusLabel.Text = getTextAndColor.Text;
			DuplicateDetectionStatusLabel.ForeColor = getTextAndColor.ForeColor;
			DuplicateDetectionStatusLabel.Visible = true;
			DuplicateDetectionStatusIcon.Visible = true;
		}

		public void ShowNoDuplicatesFound(IDuplicationEventArgs duplicationEventArgs)
		{
			var getTextAndColor = GetTextAndColor(DuplicationConditions.ShowNoDuplicatesFound);
			SetDuplicateDetectionStatusLabel(getTextAndColor.Text, getTextAndColor.ForeColor);
			DeregisterEventHandlers();
		}

		public void ShowExcludedDuplicationMessage()
		{
			var getTextAndColor = GetTextAndColor(DuplicationConditions.ShowExcludedDuplicationMessage);
			SetDuplicateDetectionStatusLabel(getTextAndColor.Text, getTextAndColor.ForeColor);
			DeregisterEventHandlers();
		}

		public void ShowNotEnoughInformation()
		{
			var getTextAndColor = GetTextAndColor(DuplicationConditions.ShowNotEnoughInformation);
			SetDuplicateDetectionStatusLabel(getTextAndColor.Text, getTextAndColor.ForeColor);
		}

		public void ShowDeduplicationTimeoutMessage()
		{
			var getTextAndColor = GetTextAndColor(DuplicationConditions.ShowDeduplicationTimeoutMessage);
			SetDuplicateDetectionStatusLabel(getTextAndColor.Text, getTextAndColor.ForeColor);
			DeregisterEventHandlers();
		}

		public void ShowDeduplicationErrorOccurredMessage()
		{
			var getTextAndColor = GetTextAndColor(DuplicationConditions.ShowDeduplicationErrorOccurredMessage);
			SetDuplicateDetectionStatusLabel(getTextAndColor.Text, getTextAndColor.ForeColor);
			DeregisterEventHandlers();
		}

		protected IDuplicationEventArgs currentDuplicationEventArgs;

		public void ShowDuplicatesFound(IDuplicationEventArgs duplicationEventArgs)
		{
			currentDuplicationEventArgs = duplicationEventArgs;
			var getTextAndColor = GetTextAndColor(DuplicationConditions.ShowDuplicatesFound);
			SetDuplicateDetectionStatusLabel(getTextAndColor.Text, getTextAndColor.ForeColor);
			DeregisterEventHandlers();
			RegisterEventHandlers();
		}

		public void SetDuplicateDetectionStatusLabel(ResourceString text, Color foreColor)
		{
			DuplicateDetectionStatusLabel.Text = text;
			DuplicateDetectionStatusLabel.ForeColor = foreColor;
			DuplicateDetectionStatusLabel.Visible = true;
			DuplicateDetectionStatusIcon.Visible = false;
		}

		void RegisterEventHandlers()
		{
			DuplicateDetectionStatusLabel.MouseEnter += DuplicateDetectionStatusLabelOnMouseEnter;
			DuplicateDetectionStatusLabel.MouseLeave += DuplicateDetectionStatusLabelOnMouseLeave;
			DuplicateDetectionStatusLabel.Click += DuplicateDetectionStatusLabelOnClick;
		}

		void DeregisterEventHandlers()
		{
			DuplicateDetectionStatusLabel.MouseEnter -= DuplicateDetectionStatusLabelOnMouseEnter;
			DuplicateDetectionStatusLabel.MouseLeave -= DuplicateDetectionStatusLabelOnMouseLeave;
			DuplicateDetectionStatusLabel.Click -= DuplicateDetectionStatusLabelOnClick;
		}

		public void DuplicateDetectionStatusLabelOnMouseEnter(object sender, EventArgs eventArgs)
		{
			DuplicateDetectionStatusLabel.Font = new Font(DuplicateDetectionStatusLabel.Font, FontStyle.Underline);
			DuplicateDetectionStatusLabel.Cursor = Cursors.Hand;
		}

		public void DuplicateDetectionStatusLabelOnMouseLeave(object sender, EventArgs e)
		{
			DuplicateDetectionStatusLabel.Font = new Font(DuplicateDetectionStatusLabel.Font, FontStyle.Regular);
			DuplicateDetectionStatusLabel.Cursor = Cursors.Default;
		}

		public void DuplicateDetectionStatusLabelOnClick(object sender, EventArgs eventArgs)
		{
			var person = SelectedContact.Person;

			person?.FindDuplicates(SelectedContact);
		}
		public void DuplicationDetected(object sender, IDuplicationEventArgs e)
		{
			ShowDuplications(e);

			if (e.Master is OrgContact contact)
			{
				contact.AddOrUpdateDDRLogInfo(e);
			}
		}

		public void ShowDuplications(IDuplicationEventArgs e)
		{
			HideDeduplicationStatus();
			DeduplicationHelper.ShowDuplicateAlert(this, e);
		}

		public void HideDeduplicationStatus()
		{
			DuplicateDetectionStatusLabel.Visible = false;
			DuplicateDetectionStatusIcon.Visible = false;
		}

		public void DeduplicationActionOccurred(object sender, IDuplicationEventArgs duplicationEventArgs)
		{
			if (duplicationEventArgs.InvokedAction != DeduplicationAction.None)
			{
				if (DuplicateDetectionStatusLabel.Visible)
				{
					HideDeduplicationStatus();
				}

				if (duplicationEventArgs.InvokedAction == DeduplicationAction.Merge)
				{
					SelectedContact?.Person?.FindDuplicates(SelectedContact);
				}
			}
		}

		public void DuplicationEnded(object sender, IDuplicationEventArgs e)
		{
			if (e == null)
			{
				throw new ArgumentNullException(nameof(e));
			}

			if (!e.Results.IsNullOrEmpty())
			{
				ShowDuplicatesFound(e);
			}
			else
			{
				//resultsmodels == null means that minimum requirements are not met and dedup hasn't been performed. if it's performed the object is not null but empty.
				// TODO: refactor deduplication flow so it's clear to see if dedup has been perfomed or not and why
				if (e.IsErrorOccurred)
				{
					ShowDeduplicationErrorOccurredMessage();
				}
				else if (e.IsTimeout)
				{
					ShowDeduplicationTimeoutMessage();
				}
				else if (e.Results == null && e.ResultsModels == null && e.TargetObjects == null)
				{
					ShowExcludedDuplicationMessage();
				}
				else if (e.ResultsModels == null)
				{
					ShowNotEnoughInformation();
				}
				else
				{
					ShowNoDuplicatesFound(e);
				}

				DeduplicationHelper.CloseExistingDuplicateAlert();
			}
		}

		public void DuplicationStarted(object sender, EventArgs e)
		{
			ShowDeduplicationStatus();
		}

		public void UnHookDuplicationDetectEvents()
		{
			if (SelectedPerson != null)
			{
				SelectedPerson.DuplicationDetected -= DuplicationDetected;
				SelectedPerson.DeduplicationStarted -= DuplicationStarted;
				SelectedPerson.DeduplicationEnded -= DuplicationEnded;
				SelectedPerson.DeduplicationActionOccurred -= DeduplicationActionOccurred;
			}
		}

		public void HookDuplicationDetectEvents()
		{
			if (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.Value && SelectedPerson != null && SelectedContact.ParentOrg.OH_IsActive)
			{
				((IDeduplicatable)SelectedPerson).ShouldRunDeduplication = true;
				SelectedPerson.DuplicationDetected += DuplicationDetected;
				SelectedPerson.DeduplicationStarted += DuplicationStarted;
				SelectedPerson.DeduplicationEnded += DuplicationEnded;
				SelectedPerson.DeduplicationActionOccurred += DeduplicationActionOccurred;
			}
		}

		void CreatePersonInfoForContact(OrgContact contact)
		{
			if (contact != null && contact.Person == null)
			{
				var person = GlbPerson.CreateFromContact(contact.Factory, contact);
				CreatedPersons[contact.PK] = person;
			}
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			UnHookDuplicationDetectEvents();
			HookDuplicationDetectEvents();
			UpdateDedupComponentsWhenCurrentChanged();
			HookFindSuggestedJobCategoriesEvent();
			FindSuggestedJobCategoriesWhenCurrentChanged();
			SelectedContact?.ReloadPerson();
			EditPersonButton.Available = SelectedContact != null && SelectedPerson != null && SelectedPerson.IsInDatabase && SystemDataRegistry.Instance.PersonIntelligenceModuleEnabled.Value;
			UnHookOC_OA_OrgAddressValueChangedEvent();
			HookOC_OA_OrgAddressValueChangedEvent();
			SetSendPasswordInstructionsButton();
			UpdateLockedOutControls();
		}

		void SetSendPasswordInstructionsButton()
		{
			var isInDatabase = SelectedContact?.IsInDatabase ?? false;
			var passwordLog = isInDatabase ? SelectedContact.GetPasswordChangedOrSentLog() : null;

			SendPasswordInstructionsButton.Enabled = isInDatabase && (ZBool)SelectedContact.OC_WebAccessEnabledInfo.OriginalValue;
			SendPasswordInstructionToolStripPanel.Enabled = SendPasswordInstructionsButton.Enabled;

			SendPasswordInstructionsLabel.Visible = passwordLog != null;
			SendPasswordInstructionsLabel.Text = passwordLog != null ? Res.GetString("42892f35-f8b1-4c6a-87a3-5b615cd6bdc6", "Last sent date: {0}", passwordLog.PostedLocalBranchTime.ToLongTimeString()) : "";
		}

		ZGuid? lastSelectedContactPK;
		ContactSendEmailSetResetPassword contactSendEmailSetResetPassword;
		ContactSendEmailSetResetPassword ContactSendEmailSetResetPassword => contactSendEmailSetResetPassword ?? (contactSendEmailSetResetPassword = GetContactSendEmailSetResetPassword());

		protected virtual ContactSendEmailSetResetPassword GetContactSendEmailSetResetPassword() =>
			new ContactSendEmailSetResetPassword();

		void UpdateDedupComponentsWhenCurrentChanged()
		{
			if (SelectedContact != null && SelectedPerson != null && ((IDeduplicatable)SelectedPerson).IsDeduplicationStarted)
			{
				if (lastSelectedContactPK == null || lastSelectedContactPK.Value != SelectedContact.PK)
				{
					SelectedPerson?.FindDuplicates(SelectedContact);
				}
			}
			else
			{
				if (DuplicateDetectionStatusLabel.Visible)
				{
					HideDeduplicationStatus();
				}
				if (DeduplicationHelper.ExistingAlertControl != null)
				{
					DeduplicationHelper.CloseExistingDuplicateAlert();
				}
			}

			lastSelectedContactPK = SelectedContact?.PK;
		}

		void ListManager_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e)
		{
			SelectedContact?.ReloadPerson();

			// A valid contact must have a name.
			if (SelectedPerson == null && !string.IsNullOrEmpty(SelectedContact?.Name))
			{
				CreatePersonInfoForContact(SelectedContact);
			}
		}

		internal void OrgContactBoundGrid_RowsDeleted(object sender, RowsDeletingEventArgs e)
		{
			foreach (var deletedContact in e.Objects)
			{
				if (CreatedPersons.TryGetValue(deletedContact.PK, out var person))
				{
					person.Delete();
					CreatedPersons.Remove(deletedContact.PK);
				}
			}
		}

		GlbPerson SelectedPerson => SelectedContact?.Person;

		Dictionary<ZGuid, GlbPerson> createdPersons;
		protected Dictionary<ZGuid, GlbPerson> CreatedPersons
		{
			get
			{
				if (createdPersons == null)
				{
					createdPersons = new Dictionary<ZGuid, GlbPerson>();
				}

				return createdPersons;
			}
		}

		#endregion

		#region JobCategory

		void FindSuggestedJobCategoriesWhenCurrentChanged()
		{
			if (SelectedContact != null)
			{
				if (SelectedContact.SuggestedJobCategories.Any())
				{
					if (!SelectedContact.IsInDatabase)
					{
						JobCategoryHelper.OverwriteJobCategory(SelectedContact);
					}

					SetSuggestedJobCategoriesLabel();
				}
				else
				{
					FindSuggestedJobCategories(!SelectedContact.IsInDatabase, false);
				}
			}
		}

		void HookFindSuggestedJobCategoriesEvent()
		{
			JobCategoryHelper.RegisterPropertyChangedEvent(SelectedContact, FindSuggestedJobCategories);
		}

		protected virtual void FindSuggestedJobCategories(bool shouldOverwrite = true, bool useCache = false)
		{
			if (SelectedContact != null)
			{
				if (!useCache)
				{
					JobCategoryHelper.FindSuggestedJobCategories(SelectedContact, shouldOverwrite);
				}

				SetSuggestedJobCategoriesLabel();
			}
		}

		void SetSuggestedJobCategoriesLabel()
		{
			SuggestedJobCategoriesLabel.Visible = SelectedContact.SuggestedJobCategories.Any(x => x.Item1 != SelectedContact.OC_JobCategory);
		}

		void SetSuggestedJobCategoriesContextMenu()
		{
			SuggestedJobCategoriesContextMenu.MenuItems.Clear();
			foreach (var category in SelectedContact.SuggestedJobCategories.Where(x => x.Item1 != SelectedContact.OC_JobCategory))
			{
				SuggestedJobCategoriesContextMenu.MenuItems.Add(category.Item2, delegate
				{
					SelectedContact.OC_JobCategory = category.Item1;
				});
			}
		}

		void SuggestedJobCategoriesLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			SetSuggestedJobCategoriesContextMenu();

			var linkLabel = sender as Control;
			SuggestedJobCategoriesContextMenu.Show(linkLabel, ControlDpiScalingHelper.NewScaledPoint(0, linkLabel.Height), LeftRightAlignment.Right);
		}

		protected ContextMenu SuggestedJobCategoriesContextMenu { get; } = new ContextMenu();

		#endregion

		#region AddressOverrideAddressControl

		void UnHookOC_OA_OrgAddressValueChangedEvent()
		{
			if (previousSelectedContact != null)
			{
				previousSelectedContact.OC_OA_OrgAddressInfo.ValueChanged -= OC_OA_OrgAddress_ValueChanged;
			}

			if (SelectedContact != null)
			{
				SelectedContact.OC_OA_OrgAddressInfo.ValueChanged -= OC_OA_OrgAddress_ValueChanged;
			}
		}

		void HookOC_OA_OrgAddressValueChangedEvent()
		{
			if (SelectedContact != null && Env.Security.PersonIntelligencePrimaryWorkplace.IsAllowed)
			{
				SelectedContact.OC_OA_OrgAddressInfo.ValueChanged += OC_OA_OrgAddress_ValueChanged;
				previousSelectedContact = SelectedContact;
			}
		}

		OrgContact previousSelectedContact;

		void OC_OA_OrgAddress_ValueChanged(object sender, EventArgs e)
		{
			var primaryRelationship = SelectedContact.Person.PrimaryRelationship;
			if (primaryRelationship?.Primary == null || SelectedContact.Person.PrimaryRelationship.Primary.PK != SelectedContact.PK)
			{
				if (Globals.Message.Show(
					Res.GetString("1729c897-af6a-4b9f-b592-0a2cbed45f53",
						"This contact is not being used as the primary workplace for their person record. Do you want to make this organization the Person's primary workplace?"),
					Res.GetString("d7294dc3-9913-46b2-a152-a388d60812ed", "Change Primary Workplace"), MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes)
				{
					SelectedContact.Person.SetPrimaryRelationship(SelectedContact);
				}
			}
		}

		#endregion

		#region Character Case on Org Fields

		public void SetCharacterCasing()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				CharacterCasing requiredCasing = Env.Registry.OrgAllowMixedCase ? CharacterCasing.Normal : CharacterCasing.Upper;

				ContactNameTextBox.CharacterCasing = requiredCasing;
				JobTitleTextBox.CharacterCasing = requiredCasing;
				var contactNameColumn = GetColumnStyleInfo(OrgContactBoundGrid.ColumnStyles, OrgContactSchema.OC_ContactName.Name);
				if (contactNameColumn != null)
				{
					contactNameColumn.CharacterCasing = requiredCasing;
				}
				var jobTitleColumn = GetColumnStyleInfo(OrgContactBoundGrid.ColumnStyles, OrgContactSchema.OC_Title.Name);
				if (jobTitleColumn != null)
				{
					jobTitleColumn.CharacterCasing = requiredCasing;
				}
			}
		}

		public ZTextBoxColumnStyleInfo GetColumnStyleInfo(ArrayList columnStyles, string columnName)
		{
			foreach (var item in columnStyles)
			{
				var type = item.GetType();
				if (type == typeof(ZTextBoxColumnStyleInfo))
				{
					var columnStyle = item as ZTextBoxColumnStyleInfo;
					if (columnStyle.ColumnName.Equals(columnName))
					{
						return columnStyle;
					}
				}
			}
			return null;
		}

		#endregion

		#region Viewing the excel Password

		void ViewExcelOpeningPassWordButton_Click(object sender, EventArgs e)
		{
			ViewPassword(ExcelOpeningPasswordTextBox, Res.GetString("ContactsUserControl|c8c50150-93b3-4803-89ef-48336831d62f", "Excel Opening Password"));
		}

		void ViewExcelModifyingPassWordButton_Click(object sender, EventArgs e)
		{
			ViewPassword(ExcelModifyingPasswordTextBox, Res.GetString("ContactsUserControl|9da5f716-adb3-40fc-b336-173c99f36c7a", "Excel Modifying Password"));
		}

		void ViewPassword(ZTextBox passwordTextBox, string caption)
		{
			Globals.Message.ShowInformation(passwordTextBox.Text, caption);
		}

		#endregion
	}
}
