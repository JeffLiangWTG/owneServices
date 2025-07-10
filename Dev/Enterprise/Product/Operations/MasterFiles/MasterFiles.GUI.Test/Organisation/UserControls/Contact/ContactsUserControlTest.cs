using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ContactsUserControl))]
	public class ContactsUserControlTest : OrganisationSecurityContainerControlBaseTest
	{
		#region Test Objects

		public class MockContactsPageControl : ContactsUserControl
		{
			public new void SendPasswordInstructionsButton_Click(object sender, EventArgs e)
			{
				base.SendPasswordInstructionsButton_Click(sender, e);
			}

			public new void UseCargoWiseWebPortalsStripButton_Click(object sender, EventArgs e)
			{
				base.UseCargoWiseWebPortalsStripButton_Click(sender, e);
			}

			public ZGrid OrgDocumentGrid => OrgDocumentBoundGrid;

			public ZGrid ContactsGrid
			{
				get { return base.OrgContactBoundGrid; }
			}

			public ZCheckBox OnlyShowWebAccessEnabledCheckbox => base.OnlyShowWebAccessEnabledContactsCheckBox;

			public new ZGrid CampaignsGrid
			{
				get { return base.CampaignsGrid; }
			}

			public new ZGrid SubscriptionsGrid => base.SubscriptionsGrid;

			public ZButton SuppressedDocs
			{
				get { return SuppressedDocsButton; }
			}

			public DialogResult ResultForTest;
			protected override DialogResult ShowFormWithoutDispose(ZForm form)
			{
				Organisation.SuppressedDocuments.AddNew().OD_DocumentGroup = ContactType.Warehouse.Code;
				Organisation.SuppressedDocuments.AddNew().OD_DocumentGroup = ContactType.Consignee.Code;
				return ResultForTest;
			}

			public new void ShowCampaignDetailsForm()
			{
				base.ShowCampaignDetailsForm();
			}

			public new ZController CampaignItemController
			{
				get { return base.CampaignItemController; }
			}

			public new void ViewCampaignButton_Click(object sender, EventArgs e)
			{
				base.ViewCampaignButton_Click(sender, e);
			}

			public new void ViewCampaignDocumentButton_Click(object sender, EventArgs e)
			{
				base.ViewCampaignDocumentButton_Click(sender, e);
			}

			public new void EditPersonButton_Click(object sender, EventArgs e)
			{
				base.EditPersonButton_Click(sender, e);
			}

			public new void SendCampaignButton_Click(object sender, EventArgs e)
			{
				base.SendCampaignButton_Click(sender, e);
			}

			public new void ResendCampaignButton_Click(object sender, EventArgs e)
			{
				base.ResendCampaignButton_Click(sender, e);
			}

			public new ZTemplateTabControl ContactDetailsTabControl
			{
				get { return base.ContactDetailsTabControl; }
			}

			public new ZTemplateTabControl CampaignsTabControl => base.CampaignsTabControl;

			public new ZString OpenSelectedAttributeURL()
			{
				return base.OpenSelectedAttributeURL();
			}

			public new ZTemplateTabControl PersonalInfoTabControl
			{
				get { return base.PersonalInfoTabControl; }
			}

			public new ZGrid AttributesGrid
			{
				get { return base.AttributesGrid; }
			}

			public new ZButton SendPasswordInstructionsButton => base.SendPasswordInstructionsButton;

			public new void OrgContactBoundGridContextMenu_Popup(object sender, EventArgs e)
			{
				base.OrgContactBoundGridContextMenu_Popup(sender, e);
			}

			public IContactCampaignSenderGUIManager CampaignSenderOverrideForTesting;
			protected override IContactCampaignSenderGUIManager GetContactCampaignSenderGUIManager()
			{
				return CampaignSenderOverrideForTesting ?? base.GetContactCampaignSenderGUIManager();
			}

			public new ZGroupBox ContactItemsGroupBox
			{
				get { return base.ContactItemsGroupBox; }
			}

			public new ZTextBox ContactNameTextBox
			{
				get { return base.ContactNameTextBox; }
			}

			public new KSplitContainer ContactsTopSplitContainer
			{
				get { return base.ContactsTopSplitContainer; }
			}

			public new ZTextBox ContactsFilterStringTextBox => base.ContactsFilterStringTextBox;

			public new ZCheckBox ShowInactiveContactsCheckBox => base.ShowInactiveContactsCheckBox;

			public ZLabel DuplicateDetectionStatusLabelForTest => base.DuplicateDetectionStatusLabel;

			public ZLinkLabel SuggestedJobCategoriesLabelForTest => SuggestedJobCategoriesLabel;

			public ZDropEditWithFixedWidth OC_JobCategoryDropEditForTest => OC_JobCategoryDropEdit;

			public ContextMenu SuggestedJobCategoriesContextMenuForTest => SuggestedJobCategoriesContextMenu;

			public ZToolStripButton EditPersonButtonForTest => EditPersonButton;

			public new ZGrid OrgContactBoundGrid => base.OrgContactBoundGrid;

			public new ZTextBox JobTitleTextBox => base.JobTitleTextBox;

			public bool HasAnyCreatedPersons => CreatedPersons.Any();

			protected override void FindSuggestedJobCategories(bool shouldOverwrite = true, bool useCache = false)
			{
				base.FindSuggestedJobCategories(shouldOverwrite, useCache);
				ProcessFindSuggestedJobCategories = true;
			}

			public bool ProcessFindSuggestedJobCategories { get; set; }

			public int CurrentRecalculatedExposed
			{
				get => currentRecalculated;
			}

			protected override void RegenerateContact(OrgContact contact)
			{
				var glowBizo = contact.Person?.CreateIGlbPerson();
				contact.Person?.PatternMatchingRecalculator.Regenerate(glowBizo);
			}

			public ZButton UnlockButtonForTest => UnlockButton;

			public new void UnlockButton_Click(object sender, EventArgs e) => base.UnlockButton_Click(sender, e);

			public ZDateEdit LockoutDateTimeBoundDateForTest => LockoutDateTimeBoundDate;

			public new ZPanel SendPasswordInstructionToolStripPanel => base.SendPasswordInstructionToolStripPanel;
		}

		#endregion

		[RequiresSTA]
		public void TestHookDuplicationDetectEventsWhenOrgInActive()
		{
			var org = CreateOrgForDedupTests();
			org.OH_IsActive = false;
			var contact = org.Contacts.AddNew();
			GlbPerson.CreateFromContact(Factory, contact);

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var orgForm = new ZOrganisationsForm(org))
			{
				orgForm.Show();
				orgForm.OrganisationsTabControl.SelectedTab = orgForm.ContactsTabPage;
				var actionsMenuItem = orgForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];

				var menuItemEnabledStatus = actionsMenuItem.MenuItems.FindByText("Find Duplicates").Enabled;
				AssertEquals(false, menuItemEnabledStatus);

				var duplicateDetectionStatusLabel = orgForm.Controls.Find("DuplicateDetectionStatusLabel", true).FirstOrDefault() as ZLabel;
				var duplicateDetectionStatusIcon = orgForm.Controls.Find("DuplicateDetectionStatusIcon", true).FirstOrDefault() as KPictureBox;
				orgForm.ContactsControl.SelectedContact.Person.FindDuplicates();

				CombineAssertions(() =>
				{
					AssertEquals(false, duplicateDetectionStatusLabel.Visible);
					AssertEquals(false, duplicateDetectionStatusIcon.Visible);
				});
			}
		}

		[RequiresSTA]
		public void TestMacroColumnInOrgDocumentBoundGrid()
		{
			PopulateOrgWithDummyValues(Org);
			var contact = Org.Contacts.AddNew();
			contact.OC_ContactName = "Justin";

			using (var form = new ZForm(Org))
			{
				using (var control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();
					Application.DoEvents();

					var macroColumn = control.OrgDocumentGrid.ColumnStyles.OfType<ZMacrosFindBoxColumnStyleInfo>().First();
					AssertNotNull(macroColumn);
					AssertNull(macroColumn.Roots);

					contact.Documents.AddNew();
					AssertNotNull(macroColumn.Roots);
				}
			}
		}

		public void TestExportMenuItemHasASecurityCheck()
		{
			PopulateOrgWithDummyValues(Org);
			var contact = Org.Contacts.AddNew();
			contact.OC_ContactName = "Andrew";

			using (var form = new ZForm(Org))
			{
				using (var control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();
					Env.Security.OrgContactViewExportContacts.IsAllowed = false;
					control.ContactsGrid.ContextMenu.DoPopup();
					Assert("Export To Excel is disabled", !control.ContactsGrid.ExportAllColumnsToExcelMenuItem.Enabled);

					Env.Security.OrgContactViewExportContacts.IsAllowed = true;
					control.ContactsGrid.ContextMenu.DoPopup();
					Assert("Export To Excel is enabled", control.ContactsGrid.ExportAllColumnsToExcelMenuItem.Enabled);
				}
			}
		}

		public void TestRegenerateProgressFormShown()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Org.Contacts.Add(contact);
			contact.OC_ContactName = "Test Name";
			Factory.Save();
			var currentUser = GlbStaff.CurrentUser;
			currentUser.GS_LoginName = User.SupportUserName;
			using (var form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			using (Env.SetTemporaryUserContext(User.ServiceUserName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(Org, "");
				control.AddRecalculatePatternTablesMenuItem(new RecalculatePatternsInitializer(Org));

				try
				{
					control.ContactsGrid.Select(0);
					var menuItem = control.ContactsGrid.ContextMenu.MenuItems.FindByText("Recalculate Pattern Tables");
					menuItem.PerformClick();
					Assert("Regenerate progressForm form is shown.", control.isrecalculateprogressFormShownForTest);
				}
				finally
				{
					if (ZFormModaliser.ActiveForm != null)
					{
						((ZForm)ZFormModaliser.ActiveForm).Dispose();
					}
				}
			}
		}

		[RequiresSTA]
		public void TestRegenerateMultipleContacts()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			Org.Contacts.Add(contact1);
			contact1.OC_ContactName = "Test Name1";
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			Org.Contacts.Add(contact2);
			contact2.OC_ContactName = "Test Name2";
			var contact3 = Factory.NewWithValidTestData<OrgContact>();
			Org.Contacts.Add(contact3);
			contact3.OC_ContactName = "Test Name3";
			var currentUser = GlbStaff.CurrentUser;
			Factory.Save();
			currentUser.GS_LoginName = User.SupportUserName;
			using (var form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			using (Env.SetTemporaryUserContext(User.ServiceUserName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(Org, "");
				control.AddRecalculatePatternTablesMenuItem(new RecalculatePatternsInitializer(Org));

				control.ContactsGrid.Select(0);
				control.ContactsGrid.Select(1);
				try
				{
					var menuItem = control.ContactsGrid.ContextMenu.MenuItems.FindByText("Recalculate Pattern Tables");
					menuItem.PerformClick();
					Assert("Regenerate progressForm form is shown.", control.isrecalculateprogressFormShownForTest);
					control.ContactsGrid.Select(2);
					Application.DoEvents();
					AssertEquals(2, control.CurrentRecalculatedExposed);
					menuItem.PerformClick();
					Application.DoEvents();
					AssertEquals(3, control.CurrentRecalculatedExposed);
				}
				finally
				{
					if (ZFormModaliser.ActiveForm != null)
					{
						((ZForm)ZFormModaliser.ActiveForm).Dispose();
					}
				}
			}
		}

		[RequiresSTA]
		public void TestDuplicateAlertControlAdjustLocationWhenSplitterMoved()
		{
			var contact = Org.Contacts.AddNew();
			var dummy = Factory.New<OrgHeader>();
			var scoringResult = new List<ScoringResult>();
			scoringResult.Add(new ScoringResult
			{
				MasterPK = contact.PK.ToGuid(),
				MasterType = typeof(OrgContact),
				Score = 1,
				TargetPK = Guid.NewGuid(),
				TargetType = typeof(OrgContact)
			});
			var args = new DuplicationEventArgs(dummy, new object(), scoringResult, new List<PatternMatchingResultModel>());
			var reloadedOrg = new BusinessObjectFactory().Load<OrgHeader>(Org.PK);
			using (var form = new ZForm(reloadedOrg))
			using (var control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(Org, "");
				control.ContactsGrid.Select(0);
				control.ContactDetailsTabControl.SelectedIndex = 0;
				control.DeduplicationHelper.ShowDuplicateAlert(control, args);

				try
				{
					var alertControl = control.DeduplicationHelper.ExistingAlertControl;
					AssertNotNull("Should contain DuplicateAlertControl", alertControl);

					var locationX1 = alertControl.PointToScreen(Point.Empty).X;
					var locationY1 = alertControl.PointToScreen(Point.Empty).Y;

					control.ContactsTopSplitContainer.SplitterDistance -= 10;

					var locationX2 = alertControl.PointToScreen(Point.Empty).X;
					var locationY2 = alertControl.PointToScreen(Point.Empty).Y;

					AssertEquals("Should have correct location", (locationX1 - 10), locationX2);
					AssertEquals("Should have correct location", locationY1, locationY2);

					control.ContactsTopSplitContainer.SplitterDistance -= 10;

					var locationX3 = alertControl.PointToScreen(Point.Empty).X;
					var locationY3 = alertControl.PointToScreen(Point.Empty).Y;

					AssertEquals("Should have correct location", (locationX1 - 20), locationX3);
					AssertEquals("Should have correct location", locationY1, locationY3);
				}
				finally
				{
					if (ZFormModaliser.ActiveForm != null)
					{
						((ZForm)ZFormModaliser.ActiveForm).Dispose();
					}
				}
			}
		}

		public void TestDuplicateAlertLocation()
		{
			var contact = Org.Contacts.AddNew();
			var dummy = Factory.New<OrgHeader>();
			var scoringResult = new List<ScoringResult>();
			scoringResult.Add(new ScoringResult
			{
				MasterPK = contact.PK.ToGuid(),
				MasterType = typeof(OrgContact),
				Score = 1,
				TargetPK = Guid.NewGuid(),
				TargetType = typeof(OrgContact)
			});
			var args = new DuplicationEventArgs(dummy, new object(), scoringResult, new List<PatternMatchingResultModel>());
			var reloadedOrg = new BusinessObjectFactory().Load<OrgHeader>(Org.PK);
			using (var form = new ZForm(reloadedOrg))
			using (var control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(Org, "");
				control.ContactsGrid.Select(0);
				control.ContactDetailsTabControl.SelectedIndex = 0;

				try
				{
					control.DeduplicationHelper.ShowDuplicateAlert(control, args);
					var alertControl = control.DeduplicationHelper.ExistingAlertControl;
					Assert(control.DuplicationAlertAnchorLocation.Equals(alertControl.Location));
				}
				finally
				{
					if (ZFormModaliser.ActiveForm != null)
					{
						((ZForm)ZFormModaliser.ActiveForm).Dispose();
					}
				}
			}
		}

		public void TestViewCampaign()
		{
			OrgContact contact = Org.Contacts.AddNew();
			Factory.Save();
			CreateCampaign(contact.PK);

			OrgHeader reloadedOrg = new BusinessObjectFactory().Load<OrgHeader>(Org.PK);
			using (ZForm form = new ZForm(reloadedOrg))
			using (MockContactsPageControl control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(Org, "");
				control.ContactsGrid.Select(0);
				control.ContactDetailsTabControl.SelectedIndex = 2;
				control.CampaignsTabControl.SelectedIndex = 0;
				control.CampaignsGrid.Select(0);
				try
				{
					control.ViewCampaignButton_Click(null, EventArgs.Empty);
					AssertEquals("Shows Form", ((BusinessObject)contact.Campaigns[0].Campaign).PK, ((BusinessObject)((ZForm)ZFormModaliser.ActiveForm).BusinessEntity).PK);
				}
				finally
				{
					if (ZFormModaliser.ActiveForm != null)
					{
						((ZForm)ZFormModaliser.ActiveForm).Dispose();
					}
				}
			}
		}

		public void TestOrgContactBoundGridContextMenu_Popup()
		{
			OrgContact contact = Org.Contacts.AddNew();
			contact.OC_ContactName = "TestContactGrid";
			Factory.Save();
			CreateCampaign(contact.PK);
			OrgContact contact1 = Org.Contacts.AddNew();
			contact1.OC_ContactName = "TestContactGrid1";

			OrgHeader reloadedOrg = new BusinessObjectFactory().Load<OrgHeader>(Org.PK);
			using (ZForm form = new ZForm(reloadedOrg))
			using (MockContactsPageControl control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(Org, "");
				try
				{
					control.ContactsGrid.CurrentRowIndex = 0;
					control.ContactsGrid.SetCurrentHitTestForTest(0, 0);
					control.ContactsGrid.OnPopup_CallForTesting();
					AssertEquals("This record should not be able to be deleted", false, control.ContactsGrid.DeleteMenuItem.Enabled);

					control.ContactsGrid.CurrentRowIndex = 1;
					control.ContactsGrid.SetCurrentHitTestForTest(1, 0);
					control.ContactsGrid.OnPopup_CallForTesting();
					AssertEquals("This record should be able to be deleted", true, control.ContactsGrid.DeleteMenuItem.Enabled);
				}
				finally
				{
					if (ZFormModaliser.ActiveForm != null)
					{
						((ZForm)ZFormModaliser.ActiveForm).Dispose();
					}
				}
			}
		}

		[RequiresSTA]
		public void TestViewCampaignDocument()
		{
			OrgContact contact = Org.Contacts.AddNew();
			CreateCampaign(contact.PK);
			contact.Campaigns[0].Campaign.G0_EmailSubject = "Some Test Campaign";
			Factory.Save();

			OrgHeader reloadedOrg = new BusinessObjectFactory().Load<OrgHeader>(Org.PK);
			using (ZForm form = new ZForm(reloadedOrg))
			{
				using (MockContactsPageControl control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();

					control.SetDataBinding(Org, "");
					control.ContactsGrid.Select(0);
					control.ContactDetailsTabControl.SelectedIndex = 2;
					control.CampaignsTabControl.SelectedIndex = 0;
					control.CampaignsGrid.Select(0);
					try
					{
						control.ViewCampaignDocumentButton_Click(null, EventArgs.Empty);
						AssertEquals("Shows Form", "Some Test Campaign", ((ZForm)ZFormModaliser.LastFormShownDialogForTest).FormCaption);
					}
					finally
					{
						if (ZFormModaliser.LastFormShownDialogForTest != null)
						{
							((ZForm)ZFormModaliser.LastFormShownDialogForTest).Dispose();
						}
					}
				}
			}
		}

		public void TestEditCampaign_NoCampaign()
		{
			OrgContact contact = Org.Contacts.AddNew();
			Factory.Save();

			using (ZForm form = new ZForm(Org))
			{
				using (MockContactsPageControl control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();

					control.SetDataBinding(Org, "");
					control.ContactsGrid.Select(0);

					control.ShowCampaignDetailsForm();
					AssertEquals("Msg shown about no campaigns", "Please select a Campaign to view.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		[RequiresSTA]
		public void TestSendCampaign()
		{
			OrgContact contact = Org.Contacts.AddNew();
			CreateCampaign(contact.PK);
			Factory.Save();

			OrgHeader reloadedOrg = new BusinessObjectFactory().Load<OrgHeader>(Org.PK);
			using (ZForm form = new ZForm(reloadedOrg))
			{
				using (MockContactsPageControl control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();

					control.SetDataBinding(Org, "");
					control.ContactsGrid.Select(0);

					control.SendCampaignButton_Click(this, EventArgs.Empty);
					AssertEquals("Send campaign form should show up", "Enterprise.MarketingManager.GUI.SendCampaignForContactForm", ZFormModaliser.LastFormShownDialogForTest.GetType().ToString());

					ZFormModaliser.LastFormShownDialogForTest = null;

					control.ContactsGrid.ListManager.RemoveAt(0);
					control.SendCampaignButton_Click(this, EventArgs.Empty);
					AssertEquals("Message shown about no contact selected", "Please select a contact to send Campaign.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			Env.Security.CampaignManagement.IsAllowed = false;
			using (ZForm form = new ZForm(reloadedOrg))
			{
				using (MockContactsPageControl control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();

					control.SetDataBinding(Org, "");
					control.ContactsGrid.Select(0);

					control.SendCampaignButton_Click(this, EventArgs.Empty);
					AssertEquals("Error message should show up", Env.Security.CampaignManagement.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestSendCampaign_WithChanges()
		{
			PopulateOrgWithDummyValues(Org);
			var contact = Org.Contacts.AddNew();
			var person = GlbPerson.CreateFromContact(contact.Factory, contact);
			contact.OC_ContactName = "Andrew";
			contact.OC_NotifyMode = Constants.ContactNotifyModes.Fax;

			using (var form = new ZForm(Org))
			{
				using (var control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();

					var campaignSender = new ContactCampaignSenderGUIManagerForTesting();
					control.CampaignSenderOverrideForTesting = campaignSender;
					control.SetDataBinding(Org, "");
					control.ContactsGrid.Select(0);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					control.SendCampaignButton_Click(this, EventArgs.Empty);
					AssertEquals("Save confirmation shown", Org.HumanReadableName + " must be saved before a campaign can be sent. Do you wish to save?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Should have not saved", false, Org.IsInDatabase);
					AssertNull("Campaign not sent to contact", campaignSender.LastSendCampaignContact);

					campaignSender.LastSendCampaignContact = null;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					control.SendCampaignButton_Click(this, EventArgs.Empty);
					AssertEquals("Save confirmation shown", Org.HumanReadableName + " must be saved before a campaign can be sent. Do you wish to save?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Should have saved", true, Org.IsInDatabase);
					AssertEquals("Campaign sent to selected contact", control.ContactsGrid.ListManager.GetCurrent(), campaignSender.LastSendCampaignContact);

					campaignSender.LastSendCampaignContact = null;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					Org.OH_FullName = "Changed Organization Name";

					control.SendCampaignButton_Click(this, EventArgs.Empty);
					AssertEquals("Save confirmation shown", Org.HumanReadableName + " must be saved before a campaign can be sent. Do you wish to save?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Should have not saved", true, Org.HasChanges);
					AssertNull("Campaign not sent to contact", campaignSender.LastSendCampaignContact);
				}
			}
		}

		public void TestResendCampaign()
		{
			WebDataRegistry.Instance.WebCampaignUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "http://unit.test.com/");
			OrgContact contact = Org.Contacts.AddNew();
			CreateCampaign(contact.PK);
			Factory.Save();

			OrgHeader reloadedOrg = new BusinessObjectFactory().Load<OrgHeader>(Org.PK);
			using (ZForm form = new ZForm(reloadedOrg))
			{
				using (MockContactsPageControl control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();

					control.SetDataBinding(Org, "");
					control.ContactsGrid.Select(0);

					try
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
						control.ResendCampaignButton_Click(null, EventArgs.Empty);
						AssertEquals("Message shown about no campaigns", "Please select a Campaign to resend.", UnitTestUserNotification.Instance.LastMessage.Text);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
						control.ContactDetailsTabControl.SelectedIndex = 2;
						control.CampaignsTabControl.SelectedIndex = 0;
						control.CampaignsGrid.Select(0);
						control.ResendCampaignButton_Click(null, EventArgs.Empty);
						AssertEquals("Message shown about confirmation", "You are about to send the campaign to the selected contacts. If already sent, the campaign will be re-sent to these contacts. Would you like to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
					}
					finally
					{
						ZFormModaliser.LastFormShownDialogForTest = null;
					}
				}
			}
		}

		public void TestResendCampaign_WithChanges()
		{
			PopulateOrgWithDummyValues(Org);
			var contact = Org.Contacts.AddNew();
			contact.OC_ContactName = "Andrew";
			contact.OC_NotifyMode = Constants.ContactNotifyModes.Fax;
			CreateCampaign(contact.PK);
			Factory.Save();

			var reloadedOrg = new BusinessObjectFactory().Load<OrgHeader>(Org.PK);
			using (var form = new ZForm(reloadedOrg))
			{
				using (var control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();

					var campaignSender = new ContactCampaignSenderGUIManagerForTesting();
					control.CampaignSenderOverrideForTesting = campaignSender;
					control.SetDataBinding(Org, "");
					control.ContactsGrid.Select(0);
					control.ContactDetailsTabControl.SelectedIndex = 2;
					control.CampaignsTabControl.SelectedIndex = 0;
					control.CampaignsGrid.Select(0);

					reloadedOrg.OH_FullName = "Changed Organization Name";

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					control.ResendCampaignButton_Click(this, EventArgs.Empty);
					AssertEquals("Save confirmation shown", reloadedOrg.HumanReadableName + " must be saved before a campaign can be sent. Do you wish to save?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Should have not saved", true, reloadedOrg.HasChanges);
					AssertNull("Campaign not resent", campaignSender.LastResentCampaign);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					control.ResendCampaignButton_Click(this, EventArgs.Empty);
					AssertEquals("Save confirmation shown", reloadedOrg.HumanReadableName + " must be saved before a campaign can be sent. Do you wish to save?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Should have saved", false, reloadedOrg.HasChanges);
					AssertEquals("Selected campaign resent", control.CampaignsGrid.ListManager.GetCurrent(), campaignSender.LastResentCampaign);
				}
			}
		}

		[RequiresSTA]
		public void TestSubscriptionsSecurityCheck()
		{
			var staffWithAccess = Factory.NewWithValidTestData<GlbStaff>();

			var allowedSecurityRecord = Factory.New<GlbSecurity>();
			allowedSecurityRecord.GU_SecurityRight = Env.Security.OrganisationControlSubscriptionPreferences.Code;
			allowedSecurityRecord.GU_SecurityItemIsAllowed = true;
			allowedSecurityRecord.GU_GS = staffWithAccess.PK;
			staffWithAccess.GroupSecurityPermissionsCollectionForBinding.Add(allowedSecurityRecord);

			var staffWithoutAccess = Factory.NewWithValidTestData<GlbStaff>();

			var deniedSecurityRecord = Factory.New<GlbSecurity>();
			deniedSecurityRecord.GU_SecurityRight = Env.Security.OrganisationControlSubscriptionPreferences.Code;
			deniedSecurityRecord.GU_SecurityItemIsAllowed = false;
			deniedSecurityRecord.GU_GS = staffWithoutAccess.PK;
			staffWithoutAccess.GroupSecurityPermissionsCollectionForBinding.Add(deniedSecurityRecord);

			Org.Subscriptions.AddNew();

			var contact = Org.Contacts.AddNew();
			contact.OC_Email = "e@ma.il";
			contact.Subscriptions.AddNew();
			Factory.Save();

			var reloadedOrg = Factory.Load<OrgHeader>(Org.PK);

			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffWithAccess.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (var form = new ZForm(reloadedOrg))
				using (var control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();

					control.SetDataBinding(reloadedOrg, "");
					control.ContactsGrid.Select(0);
					control.ContactDetailsTabControl.SelectedIndex = 2;
					control.CampaignsTabControl.SelectedIndex = 1;

					AssertEquals(false, control.SubscriptionsGrid.ReadOnly);
				}
				using (Env.SetTemporaryUserContext(new UserContext(staffWithoutAccess.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (var form = new ZForm(reloadedOrg))
				using (var control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();

					control.SetDataBinding(reloadedOrg, "");
					control.ContactsGrid.Select(0);
					control.ContactDetailsTabControl.SelectedIndex = 2;
					control.CampaignsTabControl.SelectedIndex = 1;

					AssertEquals(true, control.SubscriptionsGrid.ReadOnly);
				}
			}
		}

		IGlbCompanyCampaignItem CreateCampaign(ZGuid contactPK)
		{
			GlbStaff campaignStaff = Factory.NewWithValidTestData<GlbStaff>();
			campaignStaff.GS_Code = "CMS";
			campaignStaff.GS_EmailAddress = "cms@test.org";

			BusinessObject campaign = (BusinessObject)Factory.New<IGlbCompanyCampaign>();
			campaign[GlbCompanyCampaignSchema.G0_CampaignName.Name] = "Test Campaign";
			campaign[GlbCompanyCampaignSchema.G0_Category.Name] = "PRINT";
			campaign[GlbCompanyCampaignSchema.G0_EmailSubject.Name] = "Email Subject";
			((IGlbCompanyCampaign)campaign).HtmlDocumentBlob = new ZBlob(Encoding.ASCII.GetBytes("(*CampaignURL*)"));
			campaign[GlbCompanyCampaignSchema.G0_EstimatedStartedDate.Name] = ZDateTime.Now.AddDays(-1);
			campaign[GlbCompanyCampaignSchema.G0_GS_NKCampaignCoordinator.Name] = campaignStaff.GS_Code;
			campaign[GlbCompanyCampaignSchema.G0_GS_NKCampaignManager.Name] = campaignStaff.GS_Code;
			campaign[GlbCompanyCampaignSchema.G0_Type.Name] = "EXIST";

			BusinessObject campaignItem = (BusinessObject)Factory.New<IGlbCompanyCampaignItem>();
			campaignItem[GlbCompanyCampaignItemSchema.G8_G0.Name] = campaign.PK;
			campaignItem[GlbCompanyCampaignItemSchema.G8_RecipientTableCode.Name] = OrgContactSchema.Constants.Prefix;
			campaignItem[GlbCompanyCampaignItemSchema.G8_RecipientID.Name] = contactPK;

			Factory.Save();

			return (IGlbCompanyCampaignItem)campaignItem;
		}

		public void TestOpenURLAttribute()
		{
			OrgContact contact = Org.Contacts.AddNew();
			WebDataRegistry.Instance.WebTrackerUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "http://localhost/webtracker");
			Factory.Save();

			using (ZForm form = new ZForm(Org))
			{
				using (MockContactsPageControl control = new MockContactsPageControl())
				{
					ZString errorMessage;

					form.Controls.Add(control);
					form.Show();

					control.SetDataBinding(Org, "");
					control.ContactsGrid.Select(0);
					control.PersonalInfoTabControl.SelectedIndex = 1;

					errorMessage = control.OpenSelectedAttributeURL();
					AssertEquals("Error message shown about no SNL attribute selected", "Please select a Social Networking Link attribute to view its Web Site.", errorMessage.ToString());

					OrgContactAttribute attribute = contact.Attributes.AddNew();
					errorMessage = control.OpenSelectedAttributeURL();
					AssertEquals("Error message shown about URLs only supported for SNLs", "Web Site Addresses are only supported for Social Networking Links (SNL).", errorMessage.ToString());

					attribute.PC_Type = "SNL";
					errorMessage = control.OpenSelectedAttributeURL();
					AssertEquals("Error message shown about no URL entered", "You have not entered a web site address for this Social Networking Link.", errorMessage.ToString());
				}
			}

			WebDataRegistry.Instance.WebTrackerUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "");
		}

		public void TestSendPasswordInstructionsButtonClickShowsPromptForUnsavedChanges()
		{
			PopulateOrgWithDummyValues(Org);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var contact = Org.Contacts.AddNew();
			Org.CompanyData.OB_GB_ControllingBranch = Env.CurrentBranchPK;

			EnvProxy.Instance.Registry.MailboxDisplayName = "Test Dummy Company";
			Env.OutgoingMailManager.EmailsCreated.Clear();

			using (var form = new ZForm(Org))
			{
				using (var control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					WebDataRegistry.Instance.WebTrackerUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "http://localhost/webtracker");
					control.SetDataBinding(Org, "");
					control.ContactsGrid.Select(0);
					contact.OC_ContactName = "Test Name";
					contact.OC_WebAccessEnabled = true;
					contact.OC_Email = "test@example.com";

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					control.SendPasswordInstructionsButton_Click(this, EventArgs.Empty);
					AssertEquals("Msg shown about unsaved changes", "You must save this form first. Would you like to save now?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Msg shown about unsaved changes", "Cannot Send", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
					AssertEquals(true, Org.HasChanges);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					control.SendPasswordInstructionsButton_Click(this, EventArgs.Empty);
					AssertEquals("Msg shown about sending email successfully", "An email was sent to this contact containing the password Instruction and URL.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
					AssertEquals(false, Org.HasChanges);
				}
			}
		}

		[RequiresSTA]
		public void TestUseCargoWiseWebPortalsStripButtonClickShowsPromptForUnsavedChanges()
		{
			PopulateOrgWithDummyValues(Org);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			Org.Contacts.Add(contact);
			Org.CompanyData.OB_GB_ControllingBranch = Env.CurrentBranchPK;
			Factory.Save();

			EnvProxy.Instance.Registry.MailboxDisplayName = "Test Dummy Company";
			Env.OutgoingMailManager.EmailsCreated.Clear();
			using (var form = new ZForm(Org))
			{
				using (var control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					control.SetDataBinding(Org, "");
					control.ContactsGrid.Select(0);

					WebDataRegistry.Instance.ChoiceOfPasswordSetAndResetProcessFlowEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glow.com");

					contact.OC_WebAccessEnabled = true;
					contact.OC_ContactName = "Test Name";
					contact.OC_Email = "test@example.com";
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					control.UseCargoWiseWebPortalsStripButton_Click(this, EventArgs.Empty);
					AssertEquals("Msg shown about unsaved changes", "You must save this form first. Would you like to save now?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Msg shown about unsaved changes", "Cannot Send", UnitTestUserNotification.Instance.LastMessage.Caption);
					AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					control.UseCargoWiseWebPortalsStripButton_Click(this, EventArgs.Empty);
					AssertEquals("Msg shown about sending email successfully", "An email was sent to this contact containing the password Instruction and URL.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
					var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
					AssertEquals(Env.CurrentCompany.Name + " Password Set", sentEmail.Subject);
					AssertContains("https://glow.com", sentEmail.Body);
				}
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		[RequiresSTA]
		public void TestSendPasswordInstructionsButtonClickShowsPromptForInactiveUser()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var contact = Org.Contacts.AddNew();
			Factory.Save();

			using (var form = new ZForm(Org))
			{
				using (var control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(Org, "");
					control.ContactsGrid.Select(0);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					contact.OC_IsActive = false;
					contact.OC_WebAccessEnabled = true;
					contact.OC_Email = "test@example.com";
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					Factory.Save();
					control.SendPasswordInstructionsButton_Click(this, EventArgs.Empty);
					AssertEquals("This contact is not active. Would you like to activate the contact and send the password instruction?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(false, contact.OC_IsActive);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					Factory.Save();
					control.SendPasswordInstructionsButton_Click(this, EventArgs.Empty);
					AssertEquals("This contact is not active. Would you like to activate the contact and send the password instruction?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
					AssertEquals(true, contact.OC_IsActive);
				}
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		[RequiresSTA]
		public void TestEmailPasswordInstruction_ResetEmail()
		{
			var contact = Org.Contacts.AddNew();
			contact.GetLogs().AddNew(Events.WebAccessPasswordChanged);
			Org.CompanyData.OB_GB_ControllingBranch = Env.CurrentBranchPK;

			var webBranch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			EnvProxy.Instance.Registry.MailboxDisplayName = "Test Dummy Company";
			Env.OutgoingMailManager.EmailsCreated.Clear();

			using (var form = new ZForm(Org))
			{
				using (var control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					control.SetDataBinding(Org, "");
					control.ContactsGrid.Select(0);
					contact.OC_WebAccessEnabled = true;

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					control.SendPasswordInstructionsButton_Click(this, EventArgs.Empty);
					AssertEquals("Msg shown about unsaved changes", "You must save this form first. Would you like to save now?", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					Factory.Save();
					control.SendPasswordInstructionsButton_Click(this, EventArgs.Empty);
					AssertEquals("Msg shown about blank email", "Please specify a valid email address for this contact before sending the password instruction.", UnitTestUserNotification.Instance.LastMessage.Text);

					contact.OC_Email = "invalid";
					Factory.Save();
					control.SendPasswordInstructionsButton_Click(this, EventArgs.Empty);
					AssertEquals("Msg shown about blank / invalid email", "Please specify a valid email address for this contact before sending the password instruction.", UnitTestUserNotification.Instance.LastMessage.Text);

					contact.OC_Email = "test@example.com";
					contact.OC_WebAccessEnabled = false;
					Factory.Save();
					control.SendPasswordInstructionsButton_Click(this, EventArgs.Empty);
					AssertEquals("Msg shown about no web access", "Instructions can only be sent to users with web access enabled.", UnitTestUserNotification.Instance.LastMessage.Text);

					contact.OC_WebAccessEnabled = true;
					WebDataRegistry.Instance.WebTrackerUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "");
					Factory.Save();
					var expectedMessage = $"You need to provide a Web Tracker URL for {Env.CurrentCompany.Name} in the System Registry under path Web > Web Component URLs > WebTracker URL";
					control.SendPasswordInstructionsButton_Click(this, EventArgs.Empty);
					AssertEquals("Msg shown about no web url", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					WebDataRegistry.Instance.WebTrackerUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "http://localhost/webtracker");
					control.SendPasswordInstructionsButton_Click(this, EventArgs.Empty);
					AssertEquals("Msg shown about sending email successfully", "An email was sent to this contact containing the password Instruction and URL.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

					var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
					AssertEquals(Env.CurrentCompany.Name + " Password Reset", sentEmail.Subject);
					AssertContains("http://localhost/webtracker/Admin/ResetMasterPassword.aspx?ResetKey=", sentEmail.Body);

					Org.CompanyData.OB_GB_ControllingBranch = Guid.Empty;
					Factory.Save();
					DataRegistry.Instance.WebBranch = webBranch.PK.ToGuid();
					WebDataRegistry.Instance.WebTrackerUrl.SetValue(webBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "http://webbranch/webtracker");
					Env.OutgoingMailManager.EmailsCreated.Clear();
					control.SendPasswordInstructionsButton_Click(this, EventArgs.Empty);
					AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
					sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
					AssertContains("http://webbranch/webtracker/Admin/ResetMasterPassword.aspx?ResetKey=", sentEmail.Body);
				}
			}
		}

		[RequiresSTA]
		public void TestEmailPasswordInstruction_SetEmail()
		{
			var contact = Org.Contacts.AddNew();
			Org.CompanyData.OB_GB_ControllingBranch = Env.CurrentBranchPK;

			var webBranch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			EnvProxy.Instance.Registry.MailboxDisplayName = "Test Dummy Company";
			Env.OutgoingMailManager.EmailsCreated.Clear();

			using (var form = new ZForm(Org))
			{
				using (var control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					control.SetDataBinding(Org, "");
					control.ContactsGrid.Select(0);
					contact.OC_WebAccessEnabled = true;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					control.SendPasswordInstructionsButton_Click(this, EventArgs.Empty);
					AssertEquals("Msg shown about unsaved changes", "You must save this form first. Would you like to save now?", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					Factory.Save();
					control.SendPasswordInstructionsButton_Click(this, EventArgs.Empty);
					AssertEquals("Msg shown about blank email", "Please specify a valid email address for this contact before sending the password instruction.", UnitTestUserNotification.Instance.LastMessage.Text);

					contact.OC_Email = "invalid";
					Factory.Save();
					control.SendPasswordInstructionsButton_Click(this, EventArgs.Empty);
					AssertEquals("Msg shown about blank / invalid email", "Please specify a valid email address for this contact before sending the password instruction.", UnitTestUserNotification.Instance.LastMessage.Text);

					contact.OC_Email = "test@example.com";
					contact.OC_WebAccessEnabled = false;
					Factory.Save();
					control.SendPasswordInstructionsButton_Click(this, EventArgs.Empty);
					AssertEquals("Msg shown about no web access", "Instructions can only be sent to users with web access enabled.", UnitTestUserNotification.Instance.LastMessage.Text);

					contact.OC_WebAccessEnabled = true;
					WebDataRegistry.Instance.WebTrackerUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "");
					Factory.Save();
					var expectedMessage = $"You need to provide a Web Tracker URL for {Env.CurrentCompany.Name} in the System Registry under path Web > Web Component URLs > WebTracker URL";
					control.SendPasswordInstructionsButton_Click(this, EventArgs.Empty);
					AssertEquals("Msg shown about no web url", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

					WebDataRegistry.Instance.WebTrackerUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "http://localhost/webtracker");
					control.SendPasswordInstructionsButton_Click(this, EventArgs.Empty);
					AssertEquals("Msg shown about sending email successfully", "An email was sent to this contact containing the password Instruction and URL.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

					var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
					AssertEquals(Env.CurrentCompany.Name + " Password Set", sentEmail.Subject);
					AssertContains("http://localhost/webtracker/Admin/SetMasterPassword.aspx?SetKey=", sentEmail.Body);

					Org.CompanyData.OB_GB_ControllingBranch = Guid.Empty;
					contact.Logs.AddNew(AutoEvents.WebAccessPasswordChanged);
					Factory.Save();
					DataRegistry.Instance.WebBranch = webBranch.PK.ToGuid();
					WebDataRegistry.Instance.WebTrackerUrl.SetValue(webBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, "http://webbranch/webtracker");
					Env.OutgoingMailManager.EmailsCreated.Clear();
					control.SendPasswordInstructionsButton_Click(this, EventArgs.Empty);
					AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
					sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
					AssertContains("http://webbranch/webtracker/Admin/ResetMasterPassword.aspx?ResetKey=", sentEmail.Body);
				}
			}
		}

		[RequiresSTA]
		public void TestEmailPasswordInstructionToGlowPortals()
		{
			var contact = Org.Contacts.AddNew();
			Org.CompanyData.OB_GB_ControllingBranch = Env.CurrentBranchPK;
			Factory.Save();

			EnvProxy.Instance.Registry.MailboxDisplayName = "Test Dummy Company";
			Env.OutgoingMailManager.EmailsCreated.Clear();

			using (var form = new ZForm(Org))
			{
				using (var control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					control.SetDataBinding(Org, "");
					control.ContactsGrid.Select(0);

					WebDataRegistry.Instance.ChoiceOfPasswordSetAndResetProcessFlowEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
					GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");

					control.UseCargoWiseWebPortalsStripButton_Click(this, EventArgs.Empty);
					AssertEquals("Msg shown about registry item is off", "You need to enable the System Registry under the path Web > Enable Choice of Password Set/Reset Process Flow", UnitTestUserNotification.Instance.LastMessage.Text);

					WebDataRegistry.Instance.ChoiceOfPasswordSetAndResetProcessFlowEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

					control.UseCargoWiseWebPortalsStripButton_Click(this, EventArgs.Empty);
					AssertEquals("Msg shown about registry item is off", "You need to provide a Glow Portals Root Uri in the System Registry under the path GLOW > Services > GLOW Portals Root URL", UnitTestUserNotification.Instance.LastMessage.Text);

					GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glow.com");

					contact.OC_Email = "test@example.com";
					contact.OC_WebAccessEnabled = false;
					Factory.Save();
					control.UseCargoWiseWebPortalsStripButton_Click(this, EventArgs.Empty);
					AssertEquals("Msg shown about no web access", "Instructions can only be sent to users with web access enabled.", UnitTestUserNotification.Instance.LastMessage.Text);

					contact.OC_Email = "test@example.com";
					contact.OC_WebAccessEnabled = true;
					Factory.Save();
					control.UseCargoWiseWebPortalsStripButton_Click(this, EventArgs.Empty);
					AssertEquals("Msg shown about sending email successfully", "An email was sent to this contact containing the password Instruction and URL.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
					var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
					AssertEquals(Env.CurrentCompany.Name + " Password Set", sentEmail.Subject);
					AssertContains("https://glow.com", sentEmail.Body);
				}
			}
		}

		[RequiresSTA]
		public void TestSendPasswordInstructionToolStripPanelVisiable()
		{
			Test(true, "https://glow.com", true, false);
			Test(true, "", false, true);
			Test(true, null, false, true);
			Test(false, "https://glow.com", false, true);
			Test(false, null, false, true);

			var contact = Org.Contacts.AddNew();
			contact.GetLogs().AddNew(Events.WebAccessPasswordChanged);
			Org.CompanyData.OB_GB_ControllingBranch = Env.CurrentBranchPK;
			Factory.Save();

			void Test(bool registryItemValue, string glowPortalsRootUri, bool expectedSendPasswordInstructionToolStripPanelVisible, bool expectedSendPasswordInstructionsButtonVisiable)
			{
				WebDataRegistry.Instance.ChoiceOfPasswordSetAndResetProcessFlowEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryItemValue);
				GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glowPortalsRootUri);

				using (var form = new ZForm(Org))
				{
					using (var control = new MockContactsPageControl())
					{
						form.Controls.Add(control);
						form.Show();

						var tabControl = form.Controls.Find("ContactDetailsTabControl", true).FirstOrDefault() as ZTemplateTabControl;
						tabControl.SelectedIndex = 1;
						AssertEquals(expectedSendPasswordInstructionToolStripPanelVisible, control.SendPasswordInstructionToolStripPanel.Visible);
						AssertEquals(expectedSendPasswordInstructionsButtonVisiable, control.SendPasswordInstructionsButton.Visible);
					}
				}
			}
		}

		[ExpectNoExceptions]
		public void TestSuppressDocsForm()
		{
			AssertEquals("Documents count", 0, Org.SuppressedDocuments.Count);

			using (var form = new ZForm(Org))
			{
				using (var control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();

					control.ResultForTest = DialogResult.Cancel;
					control.SuppressedDocs.PerformClick();
					AssertEquals("Documents count should be 0", 0, Org.SuppressedDocuments.Count);

					control.ResultForTest = DialogResult.OK;
					control.SuppressedDocs.PerformClick();
					AssertEquals("Documents count should be 2", 2, Org.SuppressedDocuments.Count);

					Factory.Save();
				}
			}
		}

		public void TestEmailAndPhoneNumberColumnsAreInitiallyInVisible()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Minions";
			OrgContact contact1 = org.Contacts.AddNew();
			Factory.Save();

			using (ZOrganisationsForm form = new ZOrganisationsForm(org))
			{
				using (MockContactsPageControl control = new MockContactsPageControl())
				{
					control.InitialContactToSelect = contact1;
					form.Controls.Add(control);
					form.Show();

					AssertNotNull(control.ContactsGrid.Columns.SingleOrDefault(c => c.ColumnName == "OC_Email"));
					AssertEquals(false, control.ContactsGrid.Columns.SingleOrDefault(c => c.ColumnName == "OC_Email").IsVisible);
					AssertNotNull(control.ContactsGrid.Columns.SingleOrDefault(c => c.ColumnName == "OC_Phone_Formatted"));
					AssertEquals(false, control.ContactsGrid.Columns.SingleOrDefault(c => c.ColumnName == "OC_Phone_Formatted").IsVisible);
					AssertNotNull(control.ContactsGrid.Columns.SingleOrDefault(c => c.ColumnName == "OC_Mobile_Formatted"));
					AssertEquals(false, control.ContactsGrid.Columns.SingleOrDefault(c => c.ColumnName == "OC_Mobile_Formatted").IsVisible);
					AssertNotNull(control.ContactsGrid.Columns.SingleOrDefault(c => c.ColumnName == "OC_HomePhone_Formatted"));
					AssertEquals(false, control.ContactsGrid.Columns.SingleOrDefault(c => c.ColumnName == "OC_HomePhone_Formatted").IsVisible);
				}
			}
		}

		public void TestSetInitialContactToSelectInGrid()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Minions";
			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "Apple";
			OrgContact contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Banana";
			OrgContact contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.OC_ContactName = "Child";

			Factory.Save();
			using (ZOrganisationsForm form = new ZOrganisationsForm(org))
			{
				using (MockContactsPageControl control = new MockContactsPageControl())
				{
					control.InitialContactToSelect = contact1;
					form.Controls.Add(control);
					form.Show();
					AssertCorrectContact(contact1, control.ContactsGrid.ListManager.GetCurrent() as OrgContact);
				}
			}
			using (ZOrganisationsForm form = new ZOrganisationsForm(org))
			{
				using (MockContactsPageControl control = new MockContactsPageControl())
				{
					control.InitialContactToSelect = contact2;
					form.Controls.Add(control);
					form.Show();
					AssertCorrectContact(contact2, control.ContactsGrid.ListManager.GetCurrent() as OrgContact);
				}
			}
			using (ZOrganisationsForm form = new ZOrganisationsForm(org))
			{
				using (MockContactsPageControl control = new MockContactsPageControl())
				{
					control.InitialContactToSelect = contact3;
					form.Controls.Add(control);
					form.Show();
					Assert("Contact Child is not within the organisation, default contact selected", contact3.PK != (control.ContactsGrid.ListManager.GetCurrent() as OrgContact).PK);
				}
			}
		}

		[RequiresSTA]
		public void TestOnlyShowWebAccessEnabledContactsCheckBoxReadOnly()
		{
			using (MockContactsPageControl control = new MockContactsPageControl())
			{
				control.OnlyShowWebAccessEnabledCheckbox.ReadOnly = true;
				Assert("OnlyShowWebAccessEnabledContactsCheckBox is always not read only.", !control.OnlyShowWebAccessEnabledCheckbox.ReadOnly);
			}
		}

		#region TestUpdateContactSecurityGridVisibility

		[RequiresSTA]
		public void TestUpdateContactSecurityGridVisibility()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Minions";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Banana";
			Factory.Save();
			using (var form = new ZOrganisationsForm(org))
			{
				form.Show();
				form.OrganisationsTabControl.SelectedTab = form.ContactsTabPage;

				var contactsUserControl = (ContactsUserControl)form.ContactsTabPage.Controls[0];
				contactsUserControl.ContactDetailsTabControl.SelectedTab = contactsUserControl.SecurityTabPage;
				Assert(!contactsUserControl.WebSecuritySplitContainer.Visible);

				contactsUserControl.WebAccessCheckBox.Checked = true;
				Assert(contactsUserControl.WebSecuritySplitContainer.Visible);

				contactsUserControl.WebAccessCheckBox.Checked = false;
				Assert(!contactsUserControl.WebSecuritySplitContainer.Visible);
			}
		}

		#endregion

		#region TestWebWarehouseSecurityGridBoundToFilteredContacts

		public void TestWebWarehouseSecurityGridBoundToFilteredContacts()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "CargoWise";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "jason";
			Factory.Save();
			using (var form = new ZOrganisationsForm(org))
			{
				form.Show();
				form.OrganisationsTabControl.SelectedTab = form.ContactsTabPage;

				var contactsUserControl = (ContactsUserControl)form.ContactsTabPage.Controls[0];
				AssertEquals("FilteredContacts.WebWarehouseEligibility", contactsUserControl.WebWarehouseSecurityGrid.BindTo);
			}
		}

		#endregion

		#region Implementation

		void PopulateOrgWithDummyValues(OrgHeader org)
		{
			org.OH_FullName = "AU Organization";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_Address1 = "AAA St";
			org.MainAddress.OA_Code = "AAA St";
			org.MainAddress.OA_PostCode = "2222";
			org.PrimaryRegistrationNumber.Number = "23112936991";
		}

		void AssertCorrectContact(OrgContact contact1, OrgContact contact2)
		{
			ContactFieldsMatch(contact1, contact2);
			foreach (OrgDocument document in contact1.Documents)
			{
				Assert("Document is not correct", DocumentIsCorrect(document, contact2.Documents));
			}
		}

		void ContactFieldsMatch(OrgContact contact1, OrgContact contact2)
		{
			string[] fields = new string[] { OrgContact.Schema.PK, OrgContact.Schema.OC_OH, "OrganisationCode", "WorkingAddressPK", "WorkingAddressPK_ZAddress+AddressFK", "WorkingAddressPK_ZAddress+OrgPK" };
			ArrayList fieldsNotToCheck = new ArrayList(fields);

			foreach (ZPropertyInfo info in contact1.ZPropertyInfoHash)
			{
				if (!fieldsNotToCheck.Contains(info.Name))
				{
					AssertEquals(info.Name, contact1[info.Name], contact2[info.Name]);
				}
			}
		}

		bool DocumentIsCorrect(OrgDocument document, OrgDocumentCollection documents)
		{
			foreach (OrgDocument expected in documents)
			{
				if (DocumentMatches(document, expected))
				{
					return true;
				}
			}
			return false;
		}

		bool DocumentMatches(OrgDocument doc1, OrgDocument doc2)
		{
			string[] fields = new string[] { OrgDocument.Schema.PK, OrgDocument.Schema.OD_OC };
			ArrayList fieldsNotToCheck = new ArrayList(fields);

			bool result = true;
			foreach (ZPropertyInfo info in doc1.ZPropertyInfoHash)
			{
				if (!fieldsNotToCheck.Contains(info.Name))
				{
					result &= doc1[info.Name].ToString() == doc2[info.Name].ToString();
				}
			}
			return result;
		}

		OrgHeader Org;

		protected override void SetUp()
		{
			base.SetUp();
			Org = Factory.NewWithValidTestData<OrgHeader>();
		}

		class ContactCampaignSenderGUIManagerForTesting : IContactCampaignSenderGUIManager
		{
			public void SendCampaign(IOrgContact contact)
			{
				LastSendCampaignContact = contact;
			}

			public void ResendCampaign(IGlbCompanyCampaignItem sentCampaign)
			{
				LastResentCampaign = sentCampaign;
			}

			public IOrgContact LastSendCampaignContact;
			public IGlbCompanyCampaignItem LastResentCampaign;
		}

		#endregion

		protected override OrganisationSecurityContainerControl GetNewControlForTesting()
		{
			return new ContactsUserControl();
		}

		protected override string[] SecurityContainerPropertiesEnabledForThisControl
		{
			get { return new string[] { "IsModifyContact", "IsModifyContactContactDetails", "IsModifyContactDocDeliveryDetails", "IsModifyContactPersonalInformation" }; }
		}

		[RequiresSTA]
		public void TestSubscriptionGridsProperties()
		{
			Org.Subscriptions.AddNew();

			var contact = Org.Contacts.AddNew();
			contact.OC_Email = "e@ma.il";
			contact.Subscriptions.AddNew();
			Factory.Save();

			var reloadedOrg = new BusinessObjectFactory().Load<OrgHeader>(Org.PK);
			using (var form = new ZForm(reloadedOrg))
			using (var control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(reloadedOrg, "");
				control.ContactsGrid.Select(0);
				control.ContactDetailsTabControl.SelectedIndex = 2;
				control.CampaignsTabControl.SelectedIndex = 1;

				AssertEquals("Contact subscriptions grid should be visible", true, control.SubscriptionsGrid.Visible);
				AssertEquals("Contact subscriptions grid binding", $"{nameof(OrgHeader.FilteredContacts)}.{nameof(OrgContact.Subscriptions)}", control.SubscriptionsGrid.BindTo);
				AssertEquals("Contact subscriptions grid is not readonly", false, control.SubscriptionsGrid.ReadOnly);
			}
		}

		[RequiresSTA]
		public void TestPersonRecordSecurity()
		{
			var contact = Org.Contacts.AddNew();

			var staffWithPermissions = Factory.NewWithValidTestData<GlbStaff>();

			var allowedSecurityRecord = Factory.New<GlbSecurity>();
			allowedSecurityRecord.GU_SecurityRight = Env.Security.PersonIntelligenceView.Code;
			allowedSecurityRecord.GU_SecurityItemIsAllowed = true;
			allowedSecurityRecord.GU_GS = staffWithPermissions.PK;
			staffWithPermissions.GroupSecurityPermissionsCollectionForBinding.Add(allowedSecurityRecord);

			var staffWithoutPermissions = Factory.NewWithValidTestData<GlbStaff>();

			var deniedSecurityRecord = Factory.New<GlbSecurity>();
			deniedSecurityRecord.GU_SecurityRight = Env.Security.PersonIntelligenceView.Code;
			deniedSecurityRecord.GU_SecurityItemIsAllowed = false;
			deniedSecurityRecord.GU_GS = staffWithoutPermissions.PK;
			staffWithoutPermissions.GroupSecurityPermissionsCollectionForBinding.Add(deniedSecurityRecord);

			Factory.Save();

			var reloadedOrg = new BusinessObjectFactory().Load<OrgHeader>(Org.PK);
			using (Env.Security.SecurityCachingDisabler)
			{
				using (Env.SetTemporaryUserContext(new UserContext(staffWithPermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (var form = new ZForm(reloadedOrg))
				using (var control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();

					control.SetDataBinding(reloadedOrg, "");
					control.EditPersonButton_Click(this, EventArgs.Empty);

					AssertNotContains("Access Denied", UnitTestUserNotification.Instance.LastMessage.Caption);
				}

				using (Env.SetTemporaryUserContext(new UserContext(staffWithoutPermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				using (var form = new ZForm(reloadedOrg))
				using (var control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();

					control.SetDataBinding(reloadedOrg, "");
					control.EditPersonButton_Click(this, EventArgs.Empty);

					AssertContains("Access Denied", UnitTestUserNotification.Instance.LastMessage.Caption);
				}
			}
		}

		[RequiresSTA]
		public void TestSetSendPasswordInstructionsButtonEnableOrDisable()
		{
			var contact1 = Org.Contacts.AddNew();
			contact1.OC_ContactName = "contact 1";
			contact1.OC_Email = "contact1@wisetechglobal.com";

			var contact2 = Org.Contacts.AddNew();
			contact2.OC_ContactName = "contact 2";
			contact2.OC_Email = "contact2@wisetechglobal.com";
			contact2.OC_WebAccessEnabled = true;

			Factory.Save();

			var contact3 = Org.Contacts.AddNew();
			contact3.OC_ContactName = "contact 3";
			contact3.OC_Email = "contact3@wisetechglobal.com";
			contact3.OC_WebAccessEnabled = true;

			using (var form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show();

				void AssertButtons(string message, bool expectEnabled)
				{
					Assert(message, expectEnabled ? control.SendPasswordInstructionsButton.Enabled : !control.SendPasswordInstructionsButton.Enabled);
					Assert(message, expectEnabled ? control.SendPasswordInstructionToolStripPanel.Enabled : !control.SendPasswordInstructionToolStripPanel.Enabled);
				}

				AssertButtons("button should be disabled when open grid at first", false);

				control.ContactsGrid.PerformMouseDownForTest(2, 1);
				AssertButtons("button should be disabled as contact 3 is not saved yet", false);

				control.ContactsGrid.PerformMouseDownForTest(1, 1);
				AssertButtons("button should be enabled as contact 2 has enabled web access", true);

				control.ContactsGrid.PerformMouseDownForTest(0, 1);
				AssertButtons("button should be disabled for contact 1", false);

				contact1.OC_WebAccessEnabled = true;
				AssertButtons("button should be disabled for contact 1 as hasn't saved the change yet", false);

				Factory.Save();
				Org.RefreshBindingIncludingChildren();

				control.ContactsGrid.PerformMouseDownForTest(2, 1);
				AssertButtons("button should be enabled as contact 3 is saved", true);
			}
		}

		[TestDate(2019, 11, 01)]
		public void TestSendPasswordInstructionsLabel()
		{
			var contact = Org.Contacts.AddNew();
			Org.CompanyData.OB_GB_ControllingBranch = Env.CurrentBranchPK;
			Factory.Save();

			using (var form = new ZForm(Org))
			{
				using (var control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					Application.DoEvents();
					control.SetDataBinding(Org, "");
					control.ContactsGrid.Select(0);
					contact.OC_WebAccessEnabled = true;
					contact.OC_Email = "test@example.com";
					WebDataRegistry.Instance.WebTrackerUrl.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "http://localhost/webtracker");

					var sendPasswordInstructionLabel = form.Controls.Find("SendPasswordInstructionsLabel", true).Single() as ZLabel;
					AssertEquals("", sendPasswordInstructionLabel.Text);

					Factory.Save();
					control.SendPasswordInstructionsButton_Click(this, EventArgs.Empty);
					Application.DoEvents();
					AssertEquals("An email was sent to this contact containing the password Instruction and URL.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Last sent date: 01-Nov-19 00:00", sendPasswordInstructionLabel.Text);
				}
			}
		}

		#region DeduplicationTests

		public void TestDeDupSpinnerNotShownAtStart()
		{
			var org = CreateOrgForDedupTests();
			using (var form = new ZForm(org))
			using (var testControl = new ContactsUserControlForTest())
			{
				form.Controls.Add(testControl);
				form.Show();

				AssertEquals("Spinner should not be visible when form is created", false, testControl.DeduplicationStatusIconVisible);
			}
		}

		[RequiresSTA]
		public void TestShowNoDuplicatesFoundDisplaysCorrectly()
		{
			var org = CreateOrgForDedupTests();
			var args = new DuplicationEventArgs(null, null, null, null, DuplicationStatus.OK, new DeduplicationExclusionManager<GlbPerson>());

			using (var form = new ZForm(org))
			using (var testControl = new ContactsUserControlForTest())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.ShowDeduplicationStatus();
				Assert(testControl.DeduplicationStatusIconVisible);
				Assert(testControl.DeduplicationStatusLabelReads("Detecting duplicates"));
				Assert(testControl.DeduplicationStatusLabelColorIs(Color.Black));

				testControl.ShowNoDuplicatesFound(args);
				AssertEquals(false, testControl.DeduplicationStatusIconVisible);

				var originalRegValueExcludingInactive = SystemDataRegistry.Instance.PersonsExcludeInactivePotentialDuplicates.Value;

				try
				{
					SystemDataRegistry.Instance.PersonsExcludeInactivePotentialDuplicates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
					testControl.ShowNoDuplicatesFound(args);
					AssertEquals("No duplicates found.", testControl.DeduplicationStatusText);

					SystemDataRegistry.Instance.PersonsExcludeInactivePotentialDuplicates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					testControl.ShowNoDuplicatesFound(args);
					AssertEquals("No duplicates found.", testControl.DeduplicationStatusText);
				}
				finally
				{
					SystemDataRegistry.Instance.PersonsExcludeInactivePotentialDuplicates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalRegValueExcludingInactive);
				}
				Assert(testControl.DeduplicationStatusLabelColorIs(Color.DarkSeaGreen));
			}
		}

		[RequiresSTA]
		public void TestShowDuplicatesFoundDisplaysCorrectly()
		{
			var org = CreateOrgForDedupTests();
			using (var form = new ZForm(org))
			using (var testControl = new ContactsUserControlForTest())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.ShowDeduplicationStatus();
				Assert(testControl.DeduplicationStatusIconVisible);
				Assert(testControl.DeduplicationStatusLabelReads("Detecting duplicates"));
				Assert(testControl.DeduplicationStatusLabelColorIs(Color.Black));

				var args = new DuplicationEventArgs(null, null, null, null);
				testControl.ShowDuplicatesFound(args);
				AssertEquals(false, testControl.DeduplicationStatusIconVisible);
				Assert(testControl.DeduplicationStatusLabelReads("Duplicates found"));
				Assert(testControl.DeduplicationStatusLabelColorIs(Color.Blue));
				AssertEquals(args, testControl.CurrentDuplicationEventArgs);
			}
		}

		public void TestDeduplicationActionOccurred()
		{
			var org = CreateOrgForDedupTests();
			using (var form = new ZForm(org))
			using (var testControl = new ContactsUserControlForTest())
			{
				form.Controls.Add(testControl);
				form.Show();

				var eventArgs = new DuplicationEventArgs(null);

				eventArgs.InvokedAction = DeduplicationAction.Merge;
				testControl.ShowDuplicatesFound(eventArgs);
				Assert(testControl.DeduplicationStatusVisible);
				testControl.DeduplicationActionOccurred(this, eventArgs);
				Assert(!testControl.DeduplicationStatusVisible);

				eventArgs.InvokedAction = DeduplicationAction.Ignore;
				testControl.ShowDuplicatesFound(eventArgs);
				Assert(testControl.DeduplicationStatusVisible);
				testControl.DeduplicationActionOccurred(this, eventArgs);
				Assert(!testControl.DeduplicationStatusVisible);

				eventArgs.InvokedAction = DeduplicationAction.Link;
				testControl.ShowDuplicatesFound(eventArgs);
				Assert(testControl.DeduplicationStatusVisible);
				testControl.DeduplicationActionOccurred(this, eventArgs);
				Assert(!testControl.DeduplicationStatusVisible);

				eventArgs.InvokedAction = DeduplicationAction.NotMatched;
				testControl.ShowDuplicatesFound(eventArgs);
				Assert(testControl.DeduplicationStatusVisible);
				testControl.DeduplicationActionOccurred(this, eventArgs);
				Assert(!testControl.DeduplicationStatusVisible);

				eventArgs.InvokedAction = DeduplicationAction.OpenMaster;
				testControl.ShowDuplicatesFound(eventArgs);
				Assert(testControl.DeduplicationStatusVisible);
				testControl.DeduplicationActionOccurred(this, eventArgs);
				Assert(!testControl.DeduplicationStatusVisible);

				eventArgs.InvokedAction = DeduplicationAction.OpenTarget;
				testControl.ShowDuplicatesFound(eventArgs);
				Assert(testControl.DeduplicationStatusVisible);
				testControl.DeduplicationActionOccurred(this, eventArgs);
				Assert(!testControl.DeduplicationStatusVisible);

				eventArgs.InvokedAction = DeduplicationAction.None;
				testControl.ShowDuplicatesFound(eventArgs);
				testControl.DeduplicationActionOccurred(this, eventArgs);
				Assert(testControl.DeduplicationStatusVisible);
			}
		}

		public void TestShowDuplicationMessage()
		{
			var org = CreateOrgForDedupTests();
			using (var form = new ZForm(org))
			using (var testControl = new ContactsUserControlForTest())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.ShowDeduplicationStatus();
				Assert(testControl.DeduplicationStatusIconVisible);
				Assert(testControl.DeduplicationStatusLabelReads("Detecting duplicates"));
				Assert(testControl.DeduplicationStatusLabelColorIs(Color.Black));

				testControl.ShowDeduplicationTimeoutMessage();
				AssertEquals(false, testControl.DeduplicationStatusIconVisible);
				Assert(testControl.DeduplicationStatusLabelReads("This process has stopped due to timeout"));
				Assert(testControl.DeduplicationStatusLabelColorIs(Color.OrangeRed));

				testControl.ShowNotEnoughInformation();
				AssertEquals(false, testControl.DeduplicationStatusIconVisible);
				Assert(testControl.DeduplicationStatusLabelReads("Not enough information to detect duplicates"));
				Assert(testControl.DeduplicationStatusLabelColorIs(Color.DarkSeaGreen));

				testControl.ShowExcludedDuplicationMessage();
				AssertEquals(false, testControl.DeduplicationStatusIconVisible);
				Assert(testControl.DeduplicationStatusLabelReads("Excluded from De-duplication"));
				Assert(testControl.DeduplicationStatusLabelColorIs(Color.OrangeRed));
			}
		}

		public void TestDeduplicationHyperlinkInvokesDeduplicationSearch()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var isSearchingForDuplicates = false;
			org.Contacts.RemoveAndDeleteAll();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			org.Contacts.Add(contact);
			contact.Person.DeduplicationStarted += delegate
			{
				isSearchingForDuplicates = true;
			};
			((IDeduplicatable)contact.Person).ShouldRunDeduplication = true;

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(org))
			using (var testControl = new ContactsUserControlForTest())
			{
				form.Controls.Add(testControl);
				form.Show();

				var testTarget = Factory.NewWithValidTestData<GlbPerson>();
				var testTargetList = new List<object> { testTarget };

				testControl.ShowDuplicatesFound(new DuplicationEventArgs(org, testTargetList, null, null));
				testControl.DuplicateDetectionHyperlinkClicked();
				Assert(isSearchingForDuplicates);
			}
		}

		public void TestDeduplicationHyperlinkNotInvokesDeduplicationSearchWhenContainsError()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var isSearchingForDuplicates = false;
			org.Contacts.RemoveAndDeleteAll();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			org.Contacts.Add(contact);
			contact.OC_ContactName = "ABC";
			contact.Person.DeduplicationStarted += delegate
			{
				isSearchingForDuplicates = true;
			};
			((IDeduplicatable)contact.Person).ShouldRunDeduplication = true;

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(org))
			using (var testControl = new ContactsUserControlForTest())
			{
				form.Controls.Add(testControl);
				form.Show();

				var testTarget = Factory.NewWithValidTestData<GlbPerson>();
				var testTargetList = new List<object> { testTarget };

				testControl.ShowDuplicatesFound(new DuplicationEventArgs(org, testTargetList, null, null));

				contact.OC_ContactName = "";
				testControl.DuplicateDetectionHyperlinkClicked();
				Assert(!isSearchingForDuplicates);

				contact.OC_ContactName = "DEF";
				testControl.DuplicateDetectionHyperlinkClicked();
				Assert(isSearchingForDuplicates);
			}
		}

		public void TestDeduplicationAlert()
		{
			var org = CreateOrgForDedupTests();
			Factory.Save();

			var dummy = Factory.New<OrgHeader>();
			var scoringResult = new List<ScoringResult>();
			scoringResult.Add(new ScoringResult
			{
				MasterPK = ((OrgContact)org.Contacts.First()).Person.PK.ToGuid(),
				MasterType = typeof(GlbPerson),
				Score = 1,
				TargetPK = Guid.NewGuid(),
				TargetType = typeof(GlbPerson)
			});

			var args = new DuplicationEventArgs(dummy, new object(), scoringResult, new List<PatternMatchingResultModel>());
			using (var form = new ZForm(org))
			using (var testControl = new ContactsUserControlForTest())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.DeduplicationHelper.ShowDuplicateAlert(testControl, args);
				AssertNotNull(testControl.DeduplicationHelper.ExistingAlertControl);
			}
		}

		OrgHeader CreateOrgForDedupTests()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.Contacts.Add(contact);

			return org;
		}

		[RequiresSTA]
		public void TestShouldRunDedupWhenCurrentContactChangedAndIsRanDedupIsTrue()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			((IDeduplicatable)contact2.Person).IsDeduplicationStarted = true;
			org.Contacts.Add(contact1);
			org.Contacts.Add(contact2);
			Factory.Save();

			var isDeduplicationStarted = false;
			contact2.Person.DeduplicationStarted += (o, e) => { isDeduplicationStarted = true; };

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(org))
			using (var contactPageControl = new ContactsUserControlForTest())
			{
				form.Controls.Add(contactPageControl);
				form.Show();

				contactPageControl.ContactsGrid.PerformMouseDownForTest(0, 1);
				AssertEquals("Precondition, correct contact1 selected", contact1.PK, contactPageControl.ContactsGrid.GetCurrentPK());

				contactPageControl.ContactsGrid.PerformMouseDownForTest(1, 1);
				CombineAssertions(() =>
				{
					AssertEquals("Precondition, correct contact2 selected", contact2.PK, contactPageControl.ContactsGrid.GetCurrentPK());
					AssertEquals("Precondition, IsDeduplicationStarted flag for contact2 is true", true, ((IDeduplicatable)contact2.Person).IsDeduplicationStarted);
					AssertEquals("Dedup started for contact2", true, isDeduplicationStarted);
				});
			}
		}

		public void TestRunFindDuplicatesWhenMergeActionOccur()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			org.Contacts.Add(contact);
			Factory.Save();

			var isDeduplicationStarted = false;
			contact.Person.DeduplicationStarted += (o, e) => { isDeduplicationStarted = true; };

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(org))
			using (var contactPageControl = new ContactsUserControlForTest())
			{
				form.Controls.Add(contactPageControl);
				form.Show();

				contactPageControl.ContactsGrid.PerformMouseDownForTest(0, 1);
				AssertEquals("Precondition, contact selected", contact.PK, contactPageControl.ContactsGrid.GetCurrentPK());
				AssertEquals("Precondition, find duplicates do not run", false, isDeduplicationStarted);

				var eventArgs = new DuplicationEventArgs(null)
				{
					InvokedAction = DeduplicationAction.Merge
				};

				contactPageControl.DeduplicationActionOccurred(null, eventArgs);
				AssertEquals("Find duplicates has run", true, isDeduplicationStarted);

				isDeduplicationStarted = false;
				eventArgs.InvokedAction = DeduplicationAction.OpenMaster;
				contactPageControl.DeduplicationActionOccurred(null, eventArgs);
				AssertEquals("Find duplicates do not run", false, isDeduplicationStarted);
			}
		}

		[RequiresSTA]
		public void TestNotRunDedupWhenCurrentContactNotChangedAndIsRanDedupIsTrue()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OrgSaved += (sender, e) =>
			{
				org.RefreshBindingIncludingChildren(); // Mock in organization form
			};

			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			((IDeduplicatable)contact2.Person).IsDeduplicationStarted = true;
			org.Contacts.Add(contact1);
			org.Contacts.Add(contact2);
			Factory.Save();

			var deduplicationStartedCount = 0;
			contact2.Person.DeduplicationStarted += (o, e) => { deduplicationStartedCount++; };

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(org))
			using (var contactPageControl = new ContactsUserControlForTest())
			{
				form.Controls.Add(contactPageControl);
				form.Show();

				contactPageControl.ContactsGrid.PerformMouseDownForTest(0, 1);
				AssertEquals("Precondition, correct contact1 selected", contact1.PK, contactPageControl.ContactsGrid.GetCurrentPK());

				contactPageControl.ContactsGrid.PerformMouseDownForTest(1, 1);
				CombineAssertions(() =>
				{
					AssertEquals("Precondition, correct contact2 selected", contact2.PK, contactPageControl.ContactsGrid.GetCurrentPK());
					AssertEquals("Precondition, IsDeduplicationStarted flag for contact2 is true", true, ((IDeduplicatable)contact2.Person).IsDeduplicationStarted);
					AssertEquals("Deduplication started for contact2", 1, deduplicationStartedCount);
				});

				org.OH_FullName = "New Dummy Name";
				Factory.Save();
				AssertEquals("Deduplication not started for contact2 again", 1, deduplicationStartedCount);
			}
		}

		public void TestShouldNotRunDedupWhenCurrentContactChangedAndIsRanDedupIsFalse()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			((IDeduplicatable)contact2.Person).IsDeduplicationStarted = false;
			org.Contacts.Add(contact1);
			org.Contacts.Add(contact2);
			Factory.Save();

			var isDeduplicationStarted = false;
			contact2.Person.DeduplicationStarted += (o, e) => { isDeduplicationStarted = true; };

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(org))
			using (var contactPageControl = new ContactsUserControlForTest())
			{
				form.Controls.Add(contactPageControl);
				form.Show();

				contactPageControl.ContactsGrid.PerformMouseDownForTest(0, 1);
				AssertEquals("Precondition, correct contact1 selected", contact1.PK, contactPageControl.ContactsGrid.GetCurrentPK());

				contactPageControl.ContactsGrid.PerformMouseDownForTest(1, 1);
				CombineAssertions(() =>
				{
					AssertEquals("Precondition, correct contact2 selected", contact2.PK, contactPageControl.ContactsGrid.GetCurrentPK());
					AssertEquals("Precondition, IsDeduplicationStarted flag for contact2 is false", false, ((IDeduplicatable)contact2.Person).IsDeduplicationStarted);
					AssertEquals("Dedupe not started for contact2", false, isDeduplicationStarted);
				});
			}
		}

		[RequiresSTA]
		public void TestDuplicationEndedWhenDuplicatesFound()
		{
			var org = CreateOrgForDedupTests();
			using (var form = new ZForm(org))
			using (var testControl = new ContactsUserControlForTest())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.DuplicationEnded(this, new DuplicationEventArgs(null, new List<ScoringResult>() { new ScoringResult() }, null, ZGuid.Empty));
				Assert(testControl.DeduplicationStatusVisible);
				AssertEquals("Duplicates found", testControl.DeduplicationStatusText);
			}
		}

		[RequiresSTA]
		public void TestDuplicationEndedWhenExcludedFromDuplication()
		{
			var org = CreateOrgForDedupTests();
			using (var form = new ZForm(org))
			using (var testControl = new ContactsUserControlForTest())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.DuplicationEnded(this, new DuplicationEventArgs(null, null, null, null));
				Assert(testControl.DeduplicationStatusVisible);
				AssertEquals("Excluded from De-duplication", testControl.DeduplicationStatusText);
			}
		}

		[RequiresSTA]
		public void TestDuplicationEndedWithNoScoringResultsAndMinimumRequirementsNOTMet()
		{
			var org = CreateOrgForDedupTests();
			using (var form = new ZForm(org))
			using (var testControl = new ContactsUserControlForTest())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.DuplicationEnded(this, new DuplicationEventArgs(null, new List<ScoringResult>(), null, ZGuid.Empty));
				Assert(testControl.DeduplicationStatusVisible);
				AssertEquals("Not enough information to detect duplicates", testControl.DeduplicationStatusText);
			}
		}

		public void TestDeduplicationEndedWithNoScoringResultsAndMinimumRequirementsMet()
		{
			var org = CreateOrgForDedupTests();
			using (SystemDataRegistry.Instance.PersonsExcludeInactivePotentialDuplicates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm(org))
			using (var testControl = new ContactsUserControlForTest())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.DuplicationEnded(this, new DuplicationEventArgs(null, new List<ScoringResult>(), new List<PatternMatchingResultModel>(), ZGuid.Empty));
				Assert(testControl.DeduplicationStatusVisible);
				AssertEquals("No duplicates found.", testControl.DeduplicationStatusText);
			}
		}

		public void TestDeduplicationEndedWithTimeout()
		{
			var org = CreateOrgForDedupTests();
			using (var form = new ZForm(org))
			using (var testControl = new ContactsUserControlForTest())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.DuplicationEnded(this, new DuplicationEventArgs(null, null, null, null, DuplicationStatus.Timeout, new DeduplicationExclusionManager<OrgHeader>()));
				Assert(testControl.DeduplicationStatusVisible);
				AssertEquals("This process has stopped due to timeout", testControl.DeduplicationStatusText);
			}
		}

		public void TestDeduplicationEndedWithErrorOccurred()
		{
			var org = CreateOrgForDedupTests();
			using (var form = new ZForm(org))
			using (var testControl = new ContactsUserControlForTest())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.DuplicationEnded(this, new DuplicationEventArgs(null, null, null, null, DuplicationStatus.ErrorOccurred, new DeduplicationExclusionManager<OrgHeader>()));
				Assert(testControl.DeduplicationStatusVisible);
				AssertEquals("An error occurred while detecting duplicates", testControl.DeduplicationStatusText);
			}
		}

		public void TestDoesNotCreateTwoPersonsForANewContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.Contacts.RemoveAndDeleteAll();

			using (var form = new ZForm(org))
			using (var testControl = new ContactsUserControlForTest())
			{
				form.Controls.Add(testControl);
				form.Show();

				var contact = org.Contacts.AddNew();
				var name = "Any name that doesn't exist";
				AssertEquals("Premise for this test", 0, Factory.Load<GlbPerson>(new ZQuery(GlbPersonSchema.PER_FullName, name)).Length);

				contact.OC_ContactName = name;

				Factory.Save();

				var person = Factory.Load<GlbPerson>(new ZQuery(GlbPersonSchema.PER_FullName, name));
				AssertEquals("Only one person should exist", 1, person.Length);
				AssertEquals(contact.OC_PER, person.First().PK);
			}
		}

		public void TestDoesNotCreateTwoPersonsForAnExistingContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.Contacts.RemoveAndDeleteAll();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Any name that doesn't exist";
			var person = GlbPerson.CreateFromContact(Factory, contact);

			Factory.Save();
			AssertEquals("Premise for this test", 1, Factory.Load<GlbPerson>(new ZQuery(GlbPersonSchema.PER_FullName, contact.OC_ContactName)).Length);
			using (var form = new ZForm(org))
			using (var testControl = new ContactsUserControlForTest())
			{
				form.Controls.Add(testControl);
				form.Show();

				contact.OC_ContactName = "Another name that doesn't exist";

				Factory.Save();

				var existingPerson = Factory.Load<GlbPerson>(new ZQuery(GlbPersonSchema.PER_FullName, contact.OC_ContactName));
				AssertEquals("Only one person should exist", 1, existingPerson.Length);
				AssertEquals(contact.OC_PER, existingPerson.First().PK);
			}
		}

		public void TestShouldNotDetectDuplicates_WhenOpenNewOrganisation()
		{
			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(Org))
			{
				using (var control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();

					AssertNotNull(control.DuplicateDetectionStatusLabelForTest);
					Assert("Should not detect duplicates.", !control.DuplicateDetectionStatusLabelForTest.Visible);
				}
			}
		}

		[RequiresSTA]
		public void TestAllContactsSubscribeDetectDuplicatesEvent()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			Org.Contacts.AddNew().OC_ContactName = "Name1";
			Org.Contacts.AddNew().OC_ContactName = "Name2";
			Factory.Save();

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(Org))
			{
				using (var control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();

					control.ContactsGrid.PerformMouseDownForTest(0, 1);
					Assert("Precondition", !control.DuplicateDetectionStatusLabelForTest.Visible);

					var selectedContact = control.ContactsGrid.GetCurrent() as OrgContact;
					selectedContact.OC_ContactName = "CEO1";
					Factory.Save();
					Assert("Detecting...", control.DuplicateDetectionStatusLabelForTest.Visible);

					control.ContactsGrid.PerformMouseDownForTest(1, 1);
					control.DuplicateDetectionStatusLabelForTest.Visible = false;
					Assert("Precondition", !control.DuplicateDetectionStatusLabelForTest.Visible);

					selectedContact = control.ContactsGrid.GetCurrent() as OrgContact;
					selectedContact.OC_ContactName = "CEO2";
					Factory.Save();
					Assert("Detecting...", control.DuplicateDetectionStatusLabelForTest.Visible);
				}
			}
		}

		public void TestNewContactSubscribeDetectDuplicatesEventAndHasPersonInfo()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			Org.Contacts.AddNew().OC_ContactName = "Name1";

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(Org))
			{
				using (var control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();

					AssertEquals("Percondition", 1, control.ContactsGrid.ListManager.Count);
					var newContact = Org.Contacts.AddNew();
					newContact.OC_ContactName = "Name2";
					Org.RefreshBindingIncludingChildren();
					Factory.Save();
					AssertEquals(2, control.ContactsGrid.ListManager.Count);

					control.ContactsGrid.PerformMouseDownForTest(1, 1);
					newContact.OC_ContactName = "CEO1";
					Factory.Save();
					AssertEquals(newContact.PK, control.ContactsGrid.GetCurrentPK());
					AssertNotNull("Should have person info", newContact.Person);
					Assert("Detecting...", control.DuplicateDetectionStatusLabelForTest.Visible);
				}
			}
		}

		public void TestUpdatePersonInfoForNewContact()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var newContact = org.Contacts.AddNew();

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(org))
			{
				using (var control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(org, "");

					org.RefreshBindingIncludingChildren();

					newContact.OC_ContactName = "CEO1";
					AssertEquals(newContact.PK, control.ContactsGrid.GetCurrentPK());
					AssertNotNull("Should have person info", newContact.Person);

					newContact.OC_ContactName = "CEO2";
					Factory.Save();
					AssertEquals(newContact.OC_ContactName.Left(GlbPersonSchema.PER_FullName.MaxLength), newContact.Person.PER_FullName);

					newContact.OC_ContactName = string.Empty;
					Factory.Save();
					AssertEquals("#Imported as Empty", newContact.Person.PER_FullName);
				}
			}
		}

		[RequiresSTA]
		public void TestShouldDeletePersonInfoWhenDeleteContact()
		{
			Org.Contacts.AddNew();

			using (SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(Org))
			{
				using (var control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();

					var newContact = Org.Contacts.AddNew();
					Org.RefreshBindingIncludingChildren();

					control.ContactsGrid.PerformMouseDownForTest(1, 1);
					newContact.OC_ContactName = "CEO1";

					var personPk = newContact.Person.PK;
					AssertNotNull("Should have person info", Factory.Load<GlbPerson>(personPk));

					var contacts = new List<BusinessObject>() { newContact };
					control.OrgContactBoundGrid_RowsDeleted(control.ContactsGrid, new RowsDeletingEventArgs(contacts));
					AssertNull("Person info should be deleted", Factory.Load<GlbPerson>(personPk));
				}
			}
		}

		public void TestDuplicateDetectionStatusIcon_IsNextToContactNameTextBox()
		{
			var org = CreateOrgForDedupTests();
			using (var form = new ZForm(org))
			using (var testControl = new ContactsUserControlForTest())
			{
				form.Controls.Add(testControl);
				form.Show();

				AssertEquals(testControl.DuplicateDetectionStatusIconForTest.Parent, testControl.DuplicateDetectionStatusIconParent);
			}
		}

		#endregion

		#region Suggested Job Categories

		public void TestFindSuggestedJobCategories_WhenTitleChanged()
		{
			var contact = Org.Contacts.AddNew();
			contact.OC_Title = "Manager";
			contact.OC_JobCategory = OrgContactJobCategories.Codes.MAU;

			using (ZForm form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show();

				Assert("No suggestions.", !control.SuggestedJobCategoriesLabelForTest.Visible);

				contact.OC_Title = "CEO";
				AssertNotNull(contact.SuggestedJobCategories);
				Assert("Suggestions found.", control.SuggestedJobCategoriesLabelForTest.Visible);
			}
		}

		[RequiresSTA]
		public void TestShowOrHideSuggestedJobCategoriesLabel_WhenJobCategoryChanged()
		{
			var contact = Org.Contacts.AddNew();
			contact.OC_Title = "Manager";
			contact.OC_JobCategory = OrgContactJobCategories.Codes.MAU;

			using (ZForm form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show();

				Assert("SuggestionsLinkLabel hide.", !control.SuggestedJobCategoriesLabelForTest.Visible);
				contact.OC_JobCategory = OrgContactJobCategories.Codes.LEA;
				Assert("SuggestionsLinkLabel show.", control.SuggestedJobCategoriesLabelForTest.Visible);
			}
		}

		public void TestFindSuggestedJobCategories_WhenTitleIsEmpty()
		{
			var contact = Org.Contacts.AddNew();
			contact.OC_Title = "CEO";

			using (ZForm form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertNotNull(contact.SuggestedJobCategories);
				Assert(control.SuggestedJobCategoriesLabelForTest.Visible);

				contact.OC_Title = string.Empty;
				AssertEquals(0, contact.SuggestedJobCategories.Count);
				Assert("No Suggestions.", !control.SuggestedJobCategoriesLabelForTest.Visible);
			}
		}

		public void TestFindSuggestedJobCategories_AfterFirstLoad()
		{
			var contact = Org.Contacts.AddNew();
			contact.OC_Title = "CEO";

			using (ZForm form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertNotNull(contact.SuggestedJobCategories);
				Assert("Suggestions found.", control.SuggestedJobCategoriesLabelForTest.Visible);
			}
		}

		public void TestFindSuggestedJobCategories_WhenChangeCurrentItem()
		{
			var contact1 = Org.Contacts.AddNew();
			contact1.OC_Title = "CEO";

			var contact2 = Org.Contacts.AddNew();
			contact2.OC_Title = "Manager";
			contact2.OC_JobCategory = OrgContactJobCategories.Codes.LEA;

			using (ZForm form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.ContactsGrid.PerformMouseDownForTest(1, 1);
				AssertNotNull(contact2.SuggestedJobCategories);
				Assert("Suggestions found.", control.SuggestedJobCategoriesLabelForTest.Visible);
			}
		}

		public void TestFindSuggestedJobCategories_ShouldNotOverwriteJobCategoryForSavedContact_WhenChangeTitle()
		{
			var contact1 = Org.Contacts.AddNew();
			contact1.OC_ContactName = "Manager1";
			contact1.OC_Title = "ABC";
			contact1.OC_JobCategory = OrgContactJobCategories.Codes.EMU;
			Factory.Save();

			using (ZForm form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.ContactsGrid.PerformMouseDownForTest(0, 1);
				AssertEquals(OrgContactJobCategories.Codes.EMU, contact1.OC_JobCategory);
				Assert(!control.SuggestedJobCategoriesLabelForTest.Visible);

				contact1.OC_Title = "Manager";
				AssertEquals(OrgContactJobCategories.Codes.EMU, contact1.OC_JobCategory);
				Assert(control.SuggestedJobCategoriesLabelForTest.Visible);
			}
		}

		public void TestFindSuggestedJobCategories_ShouldOverwriteJobCategoryForUnSavedContact_WhenChangeTitle()
		{
			var contact1 = Org.Contacts.AddNew();
			contact1.OC_Title = "Manager";
			contact1.OC_JobCategory = OrgContactJobCategories.Codes.EMU;
			Factory.Save();

			using (ZForm form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.ContactsGrid.PerformMouseDownForTest(0, 1);
				AssertEquals(OrgContactJobCategories.Codes.EMU, contact1.OC_JobCategory);

				var newContact1 = Org.Contacts.AddNew();
				newContact1.OC_Title = "ABC";

				control.ContactsGrid.PerformMouseDownForTest(1, 1);
				newContact1.OC_JobCategory = OrgContactJobCategories.Codes.EMU;

				newContact1.OC_Title = "Manager";
				AssertEquals("Job category changed", OrgContactJobCategories.Codes.MAU, newContact1.OC_JobCategory);
				Assert(!control.SuggestedJobCategoriesLabelForTest.Visible);
			}
		}

		public void TestFindSuggestedJobCategories_ShouldNotOverwriteJobCategoryForSavedContact_WhenCurrentItemChanged()
		{
			var contact1 = Org.Contacts.AddNew();
			contact1.OC_ContactName = "Manager1";
			contact1.OC_Title = "Manager1";
			contact1.OC_JobCategory = OrgContactJobCategories.Codes.EMU;

			var contact2 = Org.Contacts.AddNew();
			contact2.OC_ContactName = "Manager2";
			contact2.OC_Title = "Manager2";
			contact2.OC_JobCategory = string.Empty;
			Factory.Save();

			using (ZForm form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.ContactsGrid.PerformMouseDownForTest(0, 1);
				AssertEquals(OrgContactJobCategories.Codes.EMU, contact1.OC_JobCategory);

				control.ContactsGrid.PerformMouseDownForTest(1, 1);
				AssertEquals(string.Empty, contact2.OC_JobCategory);
			}
		}

		public void TestFindSuggestedJobCategories_ShouldOverwriteJobCategoryForUnSavedContact_WhenCurrentItemChanged()
		{
			var contact1 = Org.Contacts.AddNew();
			contact1.OC_Title = "Manager";
			contact1.OC_JobCategory = OrgContactJobCategories.Codes.EMU;
			Factory.Save();

			using (ZForm form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.ContactsGrid.PerformMouseDownForTest(0, 1);
				AssertEquals(OrgContactJobCategories.Codes.EMU, contact1.OC_JobCategory);

				var newContact1 = Org.Contacts.AddNew();
				newContact1.OC_Title = "Manager";

				var newContact2 = Org.Contacts.AddNew();
				newContact2.OC_Title = "Manager";
				Org.RefreshBindingIncludingChildren();

				control.ContactsGrid.PerformMouseDownForTest(1, 1);
				newContact1.OC_JobCategory = OrgContactJobCategories.Codes.EMU;
				Assert(control.SuggestedJobCategoriesLabelForTest.Visible);

				control.ContactsGrid.PerformMouseDownForTest(2, 1);
				newContact2.OC_JobCategory = string.Empty;
				Assert(control.SuggestedJobCategoriesLabelForTest.Visible);

				control.ContactsGrid.PerformMouseDownForTest(1, 1);
				AssertEquals("Job category changed", OrgContactJobCategories.Codes.MAU, newContact1.OC_JobCategory);
				Assert(!control.SuggestedJobCategoriesLabelForTest.Visible);

				control.ContactsGrid.PerformMouseDownForTest(2, 1);
				AssertEquals("Job category changed", OrgContactJobCategories.Codes.MAU, newContact2.OC_JobCategory);
				Assert(!control.SuggestedJobCategoriesLabelForTest.Visible);
			}
		}

		[RequiresSTA]
		public void TestFindSuggestedJobCategories_JobCategoryCanBeResetToEmptyOrEMUEvenIfHasSuggestions()
		{
			var contact = Org.Contacts.AddNew();
			contact.OC_Title = "Manager";
			contact.OC_JobCategory = OrgContactJobCategories.Codes.MAU;

			using (ZForm form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(OrgContactJobCategories.Codes.MAU, contact.OC_JobCategory);
				Assert(!control.SuggestedJobCategoriesLabelForTest.Visible);

				contact.OC_JobCategory = OrgContactJobCategories.Codes.EMU;
				AssertEquals("Job category is reset to EMU", OrgContactJobCategories.Codes.EMU, contact.OC_JobCategory);
				Assert(control.SuggestedJobCategoriesLabelForTest.Visible);

				contact.OC_JobCategory = string.Empty;
				AssertNullOrEmpty("Job category is reset to empty", contact.OC_JobCategory);
				Assert(control.SuggestedJobCategoriesLabelForTest.Visible);
			}
		}

		[RequiresSTA]
		public void TestFindSuggestedJobCategories_ShouldNotFindAgain_WhenChangeCurrentItem()
		{
			var contact1 = Org.Contacts.AddNew();
			contact1.OC_Title = "OFFICER";

			var contact2 = Org.Contacts.AddNew();
			contact2.OC_Title = "Manager";

			using (var form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.ContactsGrid.PerformMouseDownForTest(1, 1);

				control.ProcessFindSuggestedJobCategories = false;
				control.ContactsGrid.PerformMouseDownForTest(0, 1);

				Assert("SuggestionsLinkLabel show", control.SuggestedJobCategoriesLabelForTest.Visible);
				Assert("Should not find again", !control.ProcessFindSuggestedJobCategories);
			}
		}

		[RequiresSTA]
		public void TestUpdateSavedJobTitle_ShouldAutoPopulateEmptyJobCategory_WhenSingleSuggestedCategory()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_OH = Org.PK;
			contact.OC_Title = "EMPLOYEE";
			contact.OC_JobCategory = string.Empty; //make sure job category is empty
			Factory.Save();

			using (ZForm form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show(); // Show the form to hook events on OC_Title change

				var suggestedCategories = JobCategoryHelper.GetJobCategories("MANAGER");
				AssertEquals("Precondition: ", 1, suggestedCategories.Count);
				AssertEquals("Precondition: ", true, contact.IsInDatabase);

				contact.OC_Title = "MANAGER";

				AssertEquals(suggestedCategories[0].Item1, contact.OC_JobCategory);
			}
		}

		[RequiresSTA]
		public void TestSetInitialJobTitle_ShouldAutoPopulateEmptyJobCategory_WhenSingleSuggestedCategory()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_OH = Org.PK;
			contact.OC_JobCategory = string.Empty;

			using (var form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show(); // Show the form to hook events on OC_Title change

				var suggestedCategories = JobCategoryHelper.GetJobCategories("MANAGER");
				AssertEquals("Precondition: ", 1, suggestedCategories.Count);
				AssertEquals("Precondition: ", false, contact.IsInDatabase);

				contact.OC_Title = "MANAGER";

				AssertEquals(suggestedCategories[0].Item1, contact.OC_JobCategory);
			}
		}

		[RequiresSTA]
		public void TestUpdateSavedJobTitle_ShouldNotAutoPopulateNonEmptyJobCategory_WhenSingleSuggestedCategory()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_OH = Org.PK;
			contact.OC_Title = "ACCOUNTANT";
			contact.OC_JobCategory = OrgContactJobCategories.Codes.EMF;  //Set to "Employee (Finance)" initially
			Factory.Save();

			using (var form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show(); // Show the form to hook events on OC_Title change

				var suggestedCategories = JobCategoryHelper.GetJobCategories("MANAGER");
				AssertEquals("Precondition: ", 1, suggestedCategories.Count);
				AssertEquals("Precondition: ", true, contact.IsInDatabase);

				contact.OC_Title = "MANAGER";

				AssertEquals(OrgContactJobCategories.Codes.EMF, contact.OC_JobCategory);
			}
		}

		public void TestSetJobTitle_ShouldNotAutoPopulateEmptyJobCategory_WhenMultipleSuggestedCategories()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_OH = Org.PK;
			contact.OC_JobCategory = string.Empty;

			using (var form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show(); // Show the form to hook events on OC_Title change

				var suggestedCategories = JobCategoryHelper.GetJobCategories("CTO");
				AssertGreaterThan("Precondition: ", suggestedCategories.Count, 1);
				AssertEquals("Precondition: ", false, contact.IsInDatabase);

				contact.OC_Title = "CTO";

				AssertEquals(string.Empty, contact.OC_JobCategory);
			}
		}

		[RequiresSTA]
		public void TestJobCategorySuggestionContextMenu_WhenClickLinkAndSelectSuggestion()
		{
			var contact = Org.Contacts.AddNew();
			contact.OC_Title = "Manager";

			var expectedJobCategory = OrgContactJobCategories.Codes.MAU;

			using (var form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show();
				var contextMenu = control.SuggestedJobCategoriesContextMenuForTest;
				AssertNotEquals("PreCondition", expectedJobCategory, contact.OC_JobCategory);
				Assert("SuggestionsLinkLabel show.", control.SuggestedJobCategoriesLabelForTest.Visible);
				contextMenu.Popup += delegate
				{
					AssertGreaterThan(contextMenu.MenuItems.Count, 0);
					contextMenu.MenuItems[0].PerformClick();
					contextMenu.Dispose();
				};

				control.SuggestedJobCategoriesLabelForTest.OnLinkClicked_Exposed(null);
				AssertEquals(expectedJobCategory, contact.OC_JobCategory);
			}
		}

		public void TestJobCategorySuggestionContextMenuChange_WhenSelectSuggestion()
		{
			var contact = Org.Contacts.AddNew();
			contact.OC_Title = "OFFICER";

			using (var form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show();
				var contextMenu = control.SuggestedJobCategoriesContextMenuForTest;
				contextMenu.Popup += delegate
				{
					AssertEquals(2, contextMenu.MenuItems.Count);
					contextMenu.MenuItems[0].PerformClick();
					contextMenu.Dispose();
				};

				control.SuggestedJobCategoriesLabelForTest.OnLinkClicked_Exposed(null);
			}
		}

		public void TestShowOrHideSuggestedJobCategoriesLabel_WhenChangeCurrentItem()
		{
			var contact1 = Org.Contacts.AddNew();
			contact1.OC_Title = "OFFICER";

			var contact2 = Org.Contacts.AddNew();
			contact2.OC_Title = "Manager";
			contact2.OC_JobCategory = OrgContactJobCategories.Codes.MAU;

			using (var form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show();
				var contextMenu = control.SuggestedJobCategoriesContextMenuForTest;

				Assert("SuggestionsLinkLabel show.", control.SuggestedJobCategoriesLabelForTest.Visible);

				control.ContactsGrid.PerformMouseDownForTest(1, 1);
				Assert("SuggestionsLinkLabel hide.", !control.SuggestedJobCategoriesLabelForTest.Visible);

				control.ContactsGrid.PerformMouseDownForTest(0, 1);
				Assert("SuggestionsLinkLabel show.", control.SuggestedJobCategoriesLabelForTest.Visible);

				contextMenu.Dispose();
			}
		}

		public void TestSuggestionLinkLabelAndJobCategoryDropDownAreCentered()
		{
			var contact = Org.Contacts.AddNew();
			contact.OC_Title = "CEO";

			using (ZForm form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show();

				var pJobCategoryDropEdit = control.PointToScreen(control.OC_JobCategoryDropEditForTest.Location);
				var pSuggestedJobCategoriesLabel = control.PointToScreen(control.SuggestedJobCategoriesLabelForTest.Location);
				var hMiddleJobCategoryDropEdit = pJobCategoryDropEdit.Y + control.OC_JobCategoryDropEditForTest.Size.Height / 2;
				var hMiddleSuggestedJobCategoriesLabel = pSuggestedJobCategoriesLabel.Y + (control.SuggestedJobCategoriesLabelForTest.Size.Height + 1) / 2;
				AssertEquals(hMiddleJobCategoryDropEdit, hMiddleSuggestedJobCategoriesLabel);
			}
		}

		#endregion

		#region Primary Working Address

		[RequiresSTA]
		public void TestChangingOC_OA_OrgAddressShouldPromptPrimaryPopup()
		{
			var newPerson = Factory.NewWithValidTestData<GlbPerson>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_PER = newPerson.PK;
			newPerson.SetPrimaryRelationship(staff);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Minions";
			var contact1 = org.Contacts.AddNew();
			contact1.OC_PER = newPerson.PK;

			var address1 = org.Addresses.AddNew();
			address1.OA_Address1 = "1 O'Riordan St";
			address1.OA_RL_NKRelatedPortCode = "AUBNE";
			var address2 = org.Addresses.AddNew();
			address2.OA_Address1 = "2 O'Riordan St";
			address2.OA_RL_NKRelatedPortCode = "AUSYD";
			address2.OA_City = "Sydney";
			address2.OA_State = "NSW";

			contact1.OC_OA_OrgAddress = address1.PK;
			Factory.Save();

			using (var form = new ZOrganisationsForm(org))
			{
				using (var control = new MockContactsPageControl())
				{
					control.InitialContactToSelect = contact1;
					form.Controls.Add(control);
					form.Show();

					UnitTestUserNotification.Instance.AddYesAnswer();
					contact1.OC_OA_OrgAddress = address2.PK;
					AssertEquals("This contact is not being used as the primary workplace for their person record. Do you want to make this organization the Person's primary workplace?", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(contact1.PK, newPerson.PrimaryRelationship.PPR_PrimaryId);
					AssertEquals(address2.City, newPerson.PrimarySource.City);
					AssertEquals(address2.OA_State, newPerson.PrimarySource.State);
					AssertEquals(address2.EffectiveRelatedPortCode.Country.RN_Desc, newPerson.PrimarySource.Country);
					AssertEquals(address2.OA_RL_NKRelatedPortCode, newPerson.PrimarySource.UNLOCO);
				}
			}
		}

		public void TestChangingOC_OA_OrgAddressShouldNotPromptPrimaryPopupIfContactIsPrimary()
		{
			var newPerson = Factory.NewWithValidTestData<GlbPerson>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_PER = newPerson.PK;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Minions";
			var contact1 = org.Contacts.AddNew();
			contact1.OC_PER = newPerson.PK;
			newPerson.SetPrimaryRelationship(contact1);

			var address1 = org.Addresses.AddNew();
			address1.OA_Address1 = "1 O'Riordan St";
			address1.OA_RL_NKRelatedPortCode = "AUBNE";
			var address2 = org.Addresses.AddNew();
			address2.OA_Address1 = "2 O'Riordan St";
			address2.OA_RL_NKRelatedPortCode = "AUSYD";

			contact1.OC_OA_OrgAddress = address1.PK;
			Factory.Save();

			using (var form = new ZOrganisationsForm(org))
			{
				using (var control = new MockContactsPageControl())
				{
					control.InitialContactToSelect = contact1;
					form.Controls.Add(control);
					form.Show();

					contact1.OC_OA_OrgAddress = address2.PK;
					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(contact1.PK, newPerson.PrimaryRelationship.PPR_PrimaryId);
				}
			}
		}

		public void TestChangingOC_OA_OrgAddressShouldNotPromptPrimaryPopupIfSecurityNotEnabled()
		{
			var staffWithoutPermissions = Factory.NewWithValidTestData<GlbStaff>();

			var deniedSecurityRecord = Factory.New<GlbSecurity>();
			deniedSecurityRecord.GU_SecurityRight = Env.Security.PersonIntelligencePrimaryWorkplace.Code;
			deniedSecurityRecord.GU_SecurityItemIsAllowed = false;
			deniedSecurityRecord.GU_GS = staffWithoutPermissions.PK;
			staffWithoutPermissions.GroupSecurityPermissionsCollectionForBinding.Add(deniedSecurityRecord);

			var newPerson = Factory.NewWithValidTestData<GlbPerson>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_PER = newPerson.PK;
			newPerson.SetPrimaryRelationship(staff);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Minions";
			var contact1 = org.Contacts.AddNew();
			contact1.OC_PER = newPerson.PK;

			var address1 = org.Addresses.AddNew();
			address1.OA_Address1 = "1 O'Riordan St";
			address1.OA_RL_NKRelatedPortCode = "AUBNE";
			var address2 = org.Addresses.AddNew();
			address2.OA_Address1 = "2 O'Riordan St";
			address2.OA_RL_NKRelatedPortCode = "AUSYD";

			contact1.OC_OA_OrgAddress = address1.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(staffWithoutPermissions.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
			using (var form = new ZForm(org))
			using (var control = new MockContactsPageControl())
			{
				control.InitialContactToSelect = contact1;
				form.Controls.Add(control);
				form.Show();

				UnitTestUserNotification.Instance.AddYesAnswer();
				contact1.OC_OA_OrgAddress = address2.PK;
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(staff.PK, newPerson.PrimaryRelationship.PPR_PrimaryId);
			}
		}

		#endregion

		#region Reload Person

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestOpenEditPersonFormAfterMerge()
		{
			var contact = Org.Contacts.AddNew();
			var originalPerson = GlbPerson.CreateFromContact(contact.Factory, contact);
			var newPerson = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();

			using (ZForm form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show();

				Db.Connection.ExecuteNonQuery($"UPDATE dbo.OrgContact set OC_PER = '{newPerson.PK}' WHERE OC_PK = '{contact.PK}'");
				originalPerson.Delete();
				Factory.Save();

				control.EditPersonButton_Click(this, EventArgs.Empty);
				AssertEquals(typeof(GlbPersonForm), ZFormModaliser.LastFormShownForTest.GetType());
			}
		}

		[RequiresSTA]
		public void TestReloadPersonWhenCurrentChanged()
		{
			var contact = Org.Contacts.AddNew();
			var originalPerson = GlbPerson.CreateFromContact(contact.Factory, contact);
			var newPerson = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();

			using (ZForm form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			using (SystemDataRegistry.Instance.PersonIntelligenceModuleEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				form.Controls.Add(control);
				form.Show();

				control.ContactsGrid.PerformMouseDownForTest(0, 1);
				Db.Connection.ExecuteNonQuery($"UPDATE dbo.OrgContact set OC_PER = '{newPerson.PK}' WHERE OC_PK = '{contact.PK}'");
				originalPerson.Delete();
				Factory.Save();

				Org.Contacts.AddNew();
				control.ContactsGrid.PerformMouseDownForTest(1, 1);
				control.ContactsGrid.PerformMouseDownForTest(0, 1);
				AssertEquals(true, control.EditPersonButtonForTest.Visible);

				control.EditPersonButton_Click(this, EventArgs.Empty);
				AssertEquals(typeof(GlbPersonForm), ZFormModaliser.LastFormShownForTest.GetType());
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestDoNotCreateNewPersonWhenListChanged()
		{
			var contact = Org.Contacts.AddNew();
			contact.OC_ContactName = "Peter";
			var originalPerson = GlbPerson.CreateFromContact(contact.Factory, contact);
			var newPerson = Factory.NewWithValidTestData<GlbPerson>();
			Factory.Save();

			using (ZForm form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			using (SystemDataRegistry.Instance.PersonIntelligenceModuleEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				form.Controls.Add(control);
				form.Show();

				Db.Connection.ExecuteNonQuery($"UPDATE dbo.OrgContact set OC_PER = '{newPerson.PK}' WHERE OC_PK = '{contact.PK}'");
				originalPerson.Delete();
				Factory.Save();
				AssertEquals(false, control.HasAnyCreatedPersons);

				contact.OC_ContactName = "Owen";
				AssertEquals(false, control.HasAnyCreatedPersons);
				AssertEquals(true, control.EditPersonButtonForTest.Visible);

				control.EditPersonButton_Click(this, EventArgs.Empty);
				AssertEquals(typeof(GlbPersonForm), ZFormModaliser.LastFormShownForTest.GetType());
			}
		}

		[RequiresSTA]
		public void TestEditPersonButtonAvailability()
		{
			var contact = Org.Contacts.AddNew();
			contact.OC_ContactName = "Peter";
			SystemDataRegistry.Instance.PersonIntelligenceModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Factory.Save();
			using (ZForm form = new ZForm(Org))
			{
				using (var control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();
					AssertEquals(true, control.EditPersonButtonForTest.Available);
				}
			}

			SystemDataRegistry.Instance.PersonIntelligenceModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (ZForm form = new ZForm(Org))
			{
				using (var control = new MockContactsPageControl())
				{
					form.Controls.Add(control);
					form.Show();
					AssertEquals(false, control.EditPersonButtonForTest.Available);
				}
			}
		}

		#endregion

		[RequiresSTA]
		public void TestDisplayFilterOptionAccordingToRegistryItem()
		{
			AssertFilterOptionDisplay(true);
			AssertFilterOptionDisplay(false);
		}

		void AssertFilterOptionDisplay(bool registryItemState)
		{
			using (OrganisationsDataRegistry.Instance.EnableControllingContactFilters.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryItemState))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				using (var form = new ZForm(org))
				using (var testControl = new ContactsUserControlForTest())
				{
					form.Controls.Add(testControl);
					form.Show();
					if (registryItemState)
					{
						Assert(testControl.ContactsFilterOptionDropEditForTest.Visible);
						AssertNull(testControl.ContactsFilterStringTextBoxForTest.CaptionResourceString.Caption);
					}
					else
					{
						Assert(!testControl.ContactsFilterOptionDropEditForTest.Visible);
						AssertEquals("Filter", testControl.ContactsFilterStringTextBoxForTest.CaptionResourceString.Caption);
					}
				}
			}
		}

		public void TestDeduplicationStartedOnDuplicateDetectionStatusLabelClick()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var contact = Factory.NewWithValidTestData<OrgContact>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.Contacts.Add(contact);

			var testTarget = Factory.NewWithValidTestData<GlbPerson>();
			var deduplicationGlbPerson = new DeduplicationGlbPerson(testTarget);
			var testTargetList = new List<DeduplicationGlbPerson>() { deduplicationGlbPerson };
			Factory.Save();

			using (var form = new ZForm(org))
			using (var testControl = new ContactsUserControl())
			{
				form.Controls.Add(testControl);
				form.Show();
				testControl.InitialContactToSelect = contact;

				SystemDataRegistry.Instance.PersonsEnableDuplicateDetection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				((IDeduplicatable)contact.Person).ShouldRunDeduplication = true;
				AssertEquals("Precondition", true, contact.Person.IsDeduplicationAllowed && ((IDeduplicatable)contact.Person).ShouldRunDeduplication && !(contact.Person.DeduplicationChildBizO ?? contact.Person).HasErrors);

				AssertEquals("Precondition", false, ((IDeduplicatable)contact.Person).IsDeduplicationStarted);

				testControl.DuplicateDetectionStatusLabelOnClick(null, null);

				AssertEquals("Deduplication started on label click", true, ((IDeduplicatable)contact.Person).IsDeduplicationStarted);
			}
		}

		public void TestUnlockButton_Click()
		{
			var contact = Org.Contacts.AddNew();
			var person = Factory.NewWithValidTestData<GlbPerson>();
			contact.OC_PER = person.PK;
			contact.OC_Email = "test@wise.com";
			CreateLockoutUserRecord(contact);
			Factory.Save();
			AssertEquals("Precondition", true, IsContactLockedOut(string.Empty, contact.OC_Email));

			using (var form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				control.InitialContactToSelect = contact;
				form.Controls.Add(control);
				form.Show();
				control.ContactDetailsTabControl.SelectedIndex = 1;
				Application.DoEvents();

				control.UnlockButton_Click(null, EventArgs.Empty);

				AssertEquals(false, IsContactLockedOut(string.Empty, contact.OC_Email));
				AssertEquals(true, Org.HasChanges);
			}
		}

		[RequiresSTA]
		public void TestUnlockContactWithCompany()
		{
			var contact = Org.Contacts.AddNew();
			var person = Factory.NewWithValidTestData<GlbPerson>();
			contact.OC_PER = person.PK;
			contact.OC_Email = "test@wise.com";
			CreateLockoutUserRecord(contact, Org.OH_Code);
			Factory.Save();
			AssertEquals("Precondition", true, IsContactLockedOut(Org.OH_Code, contact.OC_Email));
			AssertEquals("Precondition", false, IsContactLockedOut(string.Empty, contact.OC_Email));

			using (var form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				control.InitialContactToSelect = contact;
				form.Controls.Add(control);
				form.Show();
				control.ContactDetailsTabControl.SelectedIndex = 1;
				Application.DoEvents();

				control.UnlockButton_Click(null, EventArgs.Empty);

				AssertEquals(false, IsContactLockedOut(Org.OH_Code, contact.OC_Email));
				AssertEquals(false, IsContactLockedOut(string.Empty, contact.OC_Email));
				AssertEquals(true, Org.HasChanges);
			}
		}

		[RequiresSTA]
		public void TestUnlockButton_Visibility()
		{
			var contact = Org.Contacts.AddNew();
			var person = Factory.NewWithValidTestData<GlbPerson>();
			contact.OC_PER = person.PK;
			contact.OC_Email = "test@wise.com";
			Factory.Save();
			AssertEquals("Precondition", false, LoginAttemptRecorder.IsAnonymousUserLockedOut(string.Empty, contact.OC_Email));

			AssertUnlockButtonVisibility(false);

			CreateLockoutUserRecord(contact);

			AssertUnlockButtonVisibility(true);

			void AssertUnlockButtonVisibility(bool isLockedOut)
			{
				using (var form = new ZForm(Org))
				using (var control = new MockContactsPageControl())
				{
					control.InitialContactToSelect = contact;
					form.Controls.Add(control);
					form.Show();
					control.ContactDetailsTabControl.SelectedIndex = 1;
					Application.DoEvents();

					AssertEquals(isLockedOut, control.UnlockButtonForTest.Visible);
				}
			}
		}

		[RequiresSTA]
		public void TestWebSecurityLinkLabel_Click()
		{
			WebUrlLauncher.ClearLastUrlLaunched();
			AssertEquals("Precondition", string.Empty, WebUrlLauncher.LastUrlLaunched);
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			var contact = Org.Contacts.AddNew();
			Factory.Save();

			using (var form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				control.InitialContactToSelect = contact;
				form.Controls.Add(control);
				form.Show();
				control.ContactDetailsTabControl.SelectedIndex = 1;
				Application.DoEvents();

				var webSecurityLinkLabel = control.GetControl<ZLinkLabel>("webSecurityLinkLabel");
				webSecurityLinkLabel.OnLinkClicked_Exposed(null);
				var launchedUrl = WebUrlLauncher.LastUrlLaunched;
				var uri = new Uri(launchedUrl, UriKind.Absolute);
				var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
				var accessToken = queryKeyValuePairs["sso_otp"];
				var entityPK = queryKeyValuePairs["entityPK"];

				AssertEquals("Launched uri is correct.", "https", uri.Scheme);
				AssertEquals("Launched uri is correct.", "address", uri.Host);
				AssertEquals("Launched uri is correct.", "/goto/SSMContactDetail", uri.AbsolutePath);
				AssertNotNull("A Glow Access Token should be attached", accessToken);
				AssertEquals("Expected URL to include the PK of contact", contact.PK.ToString(), entityPK);
				AssertEquals("Launched uri is correct.", string.Empty, uri.Fragment);
				WebUrlLauncher.ClearLastUrlLaunched();
			}
		}

		[ExpectNoExceptions]
		public void TestWebSecurityLinkLabel_Click_WhenNoContactsInGrid()
		{
			using (var form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.ContactDetailsTabControl.SelectedIndex = 1;
				Application.DoEvents();

				var webSecurityLinkLabel = control.GetControl<ZLinkLabel>("webSecurityLinkLabel");
				webSecurityLinkLabel.OnLinkClicked_Exposed(null);
			}
		}

		public void TestWebSecurityLinkLabel_ShouldOnlyShowLinkIfValidUri()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			var contact = Org.Contacts.AddNew();
			Factory.Save();

			using (var form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				control.InitialContactToSelect = contact;
				form.Controls.Add(control);
				form.Show();
				control.ContactDetailsTabControl.SelectedIndex = 1;
				Application.DoEvents();

				var webSecurityLinkLabel = control.GetControl<ZLinkLabel>("webSecurityLinkLabel");
				AssertEquals("Should not show GLOW section", "These security rights apply to the selected contact and are defaulted from the default Organization security settings. Web Access will only be available for this contact if 'Web Access' is ticked, and a password has been specified.", webSecurityLinkLabel.Text);
			}

			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			using (var form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				control.InitialContactToSelect = contact;
				form.Controls.Add(control);
				form.Show();
				control.ContactDetailsTabControl.SelectedIndex = 1;
				Application.DoEvents();

				var webSecurityLinkLabel = control.GetControl<ZLinkLabel>("webSecurityLinkLabel");
				AssertEquals("Should show GLOW section", "These security rights apply to the selected contact and are defaulted from the default Organization security settings. Web Access will only be available for this contact if 'Web Access' is ticked, and a password has been specified. Click on CargoWise Web Portal User Administration to manage GLOW Web Security Rights.", webSecurityLinkLabel.Text);
			}
		}

		[RequiresSTA]
		public void TestLockoutDateTimeBoundDate()
		{
			var contact = Org.Contacts.AddNew();
			var person = Factory.NewWithValidTestData<GlbPerson>();
			contact.OC_PER = person.PK;
			contact.OC_Email = "test@wise.com";
			Factory.Save();
			AssertEquals("Precondition", false, LoginAttemptRecorder.IsAnonymousUserLockedOut(string.Empty, contact.OC_Email));

			AssertLockoutDateTimeBoundDate(false);

			CreateLockoutUserRecord(contact, contact.OrgCode);

			AssertLockoutDateTimeBoundDate(true);

			void AssertLockoutDateTimeBoundDate(bool isLockedOut)
			{
				using (var form = new ZForm(Org))
				using (var control = new MockContactsPageControl())
				{
					control.InitialContactToSelect = contact;
					form.Controls.Add(control);
					form.Show();
					control.ContactDetailsTabControl.SelectedIndex = 1;
					Application.DoEvents();

					AssertEquals(isLockedOut, control.LockoutDateTimeBoundDateForTest.Visible);
					AssertEquals(false, control.LockoutDateTimeBoundDateForTest.Enabled);
					AssertEquals(contact.LockoutDateTimeLocal, control.LockoutDateTimeBoundDateForTest.DateTimeValue);
				}
			}
		}

		void CreateLockoutUserRecord(OrgContact contact, string companyCode = "")
		{
			WebDataRegistry.Instance.WebLoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
			var loginFailureLog = Factory.NewWithValidTestData<StmLoginFailureLog>();
			loginFailureLog.SFL_IsLockOut = true;
			loginFailureLog.SFL_LoginName = string.IsNullOrEmpty(companyCode) ? contact.OC_Email : (ZString)(contact.OC_Email + " " + companyCode);
			loginFailureLog.SFL_TableCode = OrgContactSchema.Constants.Prefix;
			loginFailureLog.SFL_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();
		}

		bool IsContactLockedOut(string companyCode, string username)
		{
			return LoginAttemptRecorder.IsLockedOut(companyCode, username, null);
		}

		IOrgContactLoginAttemptRecorder LoginAttemptRecorder => loginAttemptRecorder ?? (loginAttemptRecorder = ObjectFactory.Get<IOrgContactLoginAttemptRecorder>());
		IOrgContactLoginAttemptRecorder loginAttemptRecorder;

		[RequiresSTA]
		public void TestDeactivateWithRedirection()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = org.Contacts.AddNew();
			contact1.OC_Email = "Mr1@email.com";
			contact1.OC_ContactName = "Mr 1";
			contact1.OC_WebAccessEnabled = true;
			var contact2 = org.Contacts.AddNew();
			contact2.OC_Email = "Mr2@email.com";
			contact2.OC_ContactName = "Mr 2";
			contact2.OC_WebAccessEnabled = true;
			Factory.Save();

			var contact3 = Factory.NewWithValidTestData<OrgContact>();
			contact3.OC_ContactName = "Mr 1";
			contact3.OC_IsActive = true;
			contact3.OC_WebAccessEnabled = true;
			contact3.OC_PER = contact1.OC_PER;
			var contact4 = Factory.NewWithValidTestData<OrgContact>();
			contact4.OC_ContactName = "Mr 2";
			contact4.OC_IsActive = false;
			contact4.OC_WebAccessEnabled = true;
			contact4.OC_PER = contact2.OC_PER;
			Factory.Save();

			using (var form = new ZForm(Org))
			using (var control = new MockContactsPageControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(org, "");

				control.ContactsGrid.SelectAllElements();
				var menuItem = control.ContactsGrid.ContextMenu.MenuItems.FindByText("Deactivate and Supersede Web Access");
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				menuItem.PerformClick();

				AssertEquals("Should not be deactivated with redirection since no was selected", false, contact1.WebAccessSuperseded);
				AssertEquals("Should not be deactivated with redirection since no was selected", true, contact1.OC_IsActive);
				AssertEquals("Should not be deactivated with redirection since no was selected", false, contact2.WebAccessSuperseded);
				AssertEquals("Should not be deactivated with redirection since no was selected", true, contact2.OC_IsActive);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				menuItem.PerformClick();

				AssertEquals("Should be deactivated with redirection since yes was selected", true, contact1.WebAccessSuperseded);
				AssertEquals("Should be deactivated with redirection since yes was selected", false, contact1.OC_IsActive);
				AssertEquals("Should be hard deactivated since it has no related active web access contacts", false, contact2.WebAccessSuperseded);
				AssertEquals("Should be deactivated with redirection since yes was selected", false, contact2.OC_IsActive);
			}
		}

		[RequiresSTA]
		public void TestSetCharacterCasing_WhenOrgAllowMixedCaseIsTrue()
		{
			Env.Registry.SetOrgAllowMixedCase(true);
			using (var control = new MockContactsPageControl())
			{
				AssertEquals("Character casing should be Normal", CharacterCasing.Normal, control.ContactNameTextBox.CharacterCasing);
				AssertEquals("Character casing should be Normal", CharacterCasing.Normal, control.JobTitleTextBox.CharacterCasing);
				AssertEquals("Character casing should be Normal", CharacterCasing.Normal, control.GetColumnStyleInfo(control.OrgContactBoundGrid.ColumnStyles, OrgContactSchema.OC_ContactName.Name).CharacterCasing);
				AssertEquals("Character casing should be Normal", CharacterCasing.Normal, control.GetColumnStyleInfo(control.OrgContactBoundGrid.ColumnStyles, OrgContactSchema.OC_Title.Name).CharacterCasing);
			}
		}

		[RequiresSTA]
		public void TestSetCharacterCasing_WhenOrgAllowMixedCaseIsFalse()
		{
			Env.Registry.SetOrgAllowMixedCase(false);
			using (var control = new MockContactsPageControl())
			{
				AssertEquals("Character casing should be Upper", CharacterCasing.Upper, control.ContactNameTextBox.CharacterCasing);
				AssertEquals("Character casing should be Upper", CharacterCasing.Upper, control.JobTitleTextBox.CharacterCasing);
				AssertEquals("Character casing should be Normal", CharacterCasing.Upper, control.GetColumnStyleInfo(control.OrgContactBoundGrid.ColumnStyles, OrgContactSchema.OC_ContactName.Name).CharacterCasing);
				AssertEquals("Character casing should be Normal", CharacterCasing.Upper, control.GetColumnStyleInfo(control.OrgContactBoundGrid.ColumnStyles, OrgContactSchema.OC_Title.Name).CharacterCasing);
			}
		}

		[RequiresSTA]
		public void TestGetColumnStyleInfo_ReturnsCorrectItem()
		{
			using (var control = new MockContactsPageControl())
			{
				var list = new ArrayList();
				var columnStyle1 = new ZTextBoxColumnStyleInfo();
				columnStyle1.ColumnName = "Column 1";
				var columnStyle2 = new ZTextBoxColumnStyleInfo();
				columnStyle2.ColumnName = "Column 2";
				list.Add(columnStyle1);
				list.Add(columnStyle2);
				AssertEquals("Returns correct item", columnStyle1, control.GetColumnStyleInfo(list, "Column 1"));
			}
		}

		[RequiresSTA]
		public void TestGetColumnStyleInfo_ReturnsNullWhenNotFound()
		{
			using (var control = new MockContactsPageControl())
			{
				var list = new ArrayList();
				var columnStyle1 = new ZTextBoxColumnStyleInfo();
				columnStyle1.ColumnName = "Column 1";
				AssertEquals("Returns correct item", null, control.GetColumnStyleInfo(list, "Not In List"));
			}
		}

		public class ContactsUserControlForTest : ContactsUserControl
		{
			public bool DeduplicationStatusIconVisible => DuplicateDetectionStatusLabel.Visible && DuplicateDetectionStatusIcon.Visible;

			public bool DeduplicationStatusLabelReads(string text) => DuplicateDetectionStatusLabel.Text.Equals(text);

			public bool HasTimeoutToolTip => "Duplicate results for this record can be accessed in the MDM admin panel".Equals(ToolTipService.GetToolTip(this.DuplicateDetectionStatusLabel));

			public ZGrid ContactsGrid => base.OrgContactBoundGrid;

			public bool DeduplicationStatusLabelColorIs(Color color) => DuplicateDetectionStatusLabel.ForeColor.Equals(color);

			public KPictureBox DuplicateDetectionStatusIconForTest => DuplicateDetectionStatusIcon;

			public ZTextBox ContactNameTextBoxForTest => ContactNameTextBox;

			public ZPanel DuplicateDetectionStatusIconParent => zPanelContactDetails;

			public ZDropEdit ContactsFilterOptionDropEditForTest => ContactsFilterOptionDropEdit;

			public ZTextBox ContactsFilterStringTextBoxForTest => ContactsFilterStringTextBox;

			public IDuplicationEventArgs CurrentDuplicationEventArgs
			{
				get { return currentDuplicationEventArgs; }
			}

			public void ListSelect(int row)
			{
				this.OrgContactBoundGrid.Select(row);
			}

			public void DuplicateDetectionHyperlinkClicked()
			{
				DuplicateDetectionStatusLabelOnClick(this, EventArgs.Empty);
			}
		}
	}
}
